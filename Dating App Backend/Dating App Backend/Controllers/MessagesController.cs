using AutoMapper;
using Dating_App_Backend.DTOs;
using Dating_App_Backend.Entities;
using Dating_App_Backend.Extensions;
using Dating_App_Backend.Helper;
using Dating_App_Backend.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dating_App_Backend.Controllers
{
    public class MessagesController : BaseApiController
    {
        private readonly IMessagesRepository _messagesRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public MessagesController(IMessagesRepository messagesRepository, IUserRepository userRepository, IMapper mapper)
        {
            _messagesRepository = messagesRepository;
            _userRepository = userRepository;
            _mapper = mapper;

        }
        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var sender = await _userRepository.GetUserByNameAsync(User.GetUsername());
            var recipenet = await _userRepository.GetUserByNameAsync(createMessageDto.RecipenetUsername);

            if (recipenet == null)
            {
                return NotFound();
            }


            ///get previous message to set it no last
            var preLastMessage = await _messagesRepository.PreviousLast(User.GetUsername(), createMessageDto.RecipenetUsername);
            if (preLastMessage != null)
            {
                preLastMessage.LastMessage = false;
            }


            var message = new Message
            {
                Sender = sender,
                Recipenet = recipenet,
                SenderUsername = sender.UserName,
                RecipenetUsername = recipenet.UserName,
                Content = createMessageDto.Content,
                LastMessage = true
            };

            _messagesRepository.AddMessage(message);

            if (await _messagesRepository.SaveAllAsync())
            {
                return Ok(_mapper.Map<MessageDto>(message));
            }

            return BadRequest("Failed To Send The Message");
        }
        [HttpGet("list")]
        public async Task<ActionResult> GetMessageForUser()
        {
            ///get any common message that i send or he send
            ///should define last message property
            ///define unread count as well 
            
            return Ok(await _messagesRepository.GetMessagesList(User.GetUsername()));
        }

    

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            string curentName = User.GetUsername();

            if (await _userRepository.GetUserByNameAsync(username) != null)
            {
                return Ok(await _messagesRepository.GetMessageThread(curentName, username));
            }
            return NotFound();
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUsername();
            var message = await _messagesRepository.GetMessageAsync(id);

            if (message.SenderUsername != username && message.RecipenetUsername != username )
            {
                return Unauthorized();
            }

            if (username == message.SenderUsername)
            {
                message.SenderDeleted = true;
            }
            else
            {
                message.RecipenetDeleted = true;
            }

            if (message.RecipenetDeleted && message.SenderDeleted)
            {
                _messagesRepository.DeleteMessage(message);
            }
            if (await _messagesRepository.SaveAllAsync())
            {
                return Ok();
            }

            return BadRequest("Failed to delete message");
        }
    }
}
