using AutoMapper;
using Dating_App_Backend.DTOs;
using Dating_App_Backend.Entities;
using Dating_App_Backend.Extensions;
using Dating_App_Backend.Helper;
using Dating_App_Backend.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Net.NetworkInformation;

namespace Dating_App_Backend.Controllers
{
    public class MessagesController : BaseApiController
    {
        private readonly IMessagesRepository _messagesRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public MessagesController(IMessagesRepository messagesRepository, IUserRepository userRepository, IMapper mapper, IConfiguration configuration)
        {
            _messagesRepository = messagesRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _configuration = configuration;

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

            string groupName = GetGroupName(sender.UserName, recipenet.UserName);

            var message = new Message
            {
                Sender = sender,
                Recipenet = recipenet,
                SenderUsername = sender.UserName,
                RecipenetUsername = recipenet.UserName,
                Content = createMessageDto.Content,
                GroupName = groupName
            };

            _messagesRepository.AddMessage(message);

            if (await _messagesRepository.SaveAllAsync())
            {
                return Ok(_mapper.Map<MessageDto>(message));
            }

            return BadRequest("Failed To Send The Message");
        }

        [HttpGet("get-unread-count")]
        public async Task<ActionResult> GetUnreadCount()
        {
            return Ok(new { result = await _messagesRepository.GetUnreadCount(User.GetUsername()) });
        }
        
        [HttpGet("get-unread-count/{senderName}")]
        public async Task<ActionResult> GetUnreadCount(string senderName)
        {
            return Ok(new { result = await _messagesRepository.GetUnreadCountFromUser(User.GetUsername(), senderName) });
        }


        [HttpGet("list")]
        public async Task<ActionResult> GetMessageForUser()
        {
            List<MessageDto> messages = new List<MessageDto>();
            string query = @"
            SELECT m1.SenderUsername, m1.RecipenetUsername, m1.Content, m1.MessageSent,
            iif(m1.RecipenetUsername = @username,(select count(*) from Messages mm where mm.DateRead is null and mm.groupName = m1.groupName), 0) 
            as unreadCount ,
            iif(m1.RecipenetUsername = @username, (select url from photos where AppUserId = m1.senderId), (select url from photos where AppUserId = m1.recipenetId)) as photoUrl

            FROM Messages m1 
            INNER JOIN 
            (SELECT GroupName, max(MessageSent) as LastMessage FROM Messages 

            WHERE GroupName like '%'+@username+'%' and not (RecipenetUsername = @username and RecipenetDeleted = 1 or SenderUsername = @username and SenderDeleted = 1)

            GROUP BY GroupName) m2 

            ON m1.GroupName = m2.GroupName AND m1.MessageSent = m2.LastMessage
            Order by m1.MessageSent desc;
            ";

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("defaultConection")))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", User.GetUsername());
                    command.Parameters.AddWithValue("@usernamePatern", "%" + User.GetUsername() + "%");

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            MessageDto message = new MessageDto
                            {
                                SenderUsername = reader.GetString(0),
                                RecipenetUsername = reader.GetString(1),
                                Content = reader.GetString(2),
                                MessageSent = reader.GetDateTime(3),
                                UnreadCount = reader.GetInt32(4),
                                listPhotoUrl = reader.GetString(5),
                            };

                            messages.Add(message);
                        }
                    }
                }
            }
            
            return Ok(messages);
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
        private string GetGroupName(string user1, string user2)
        {
            var flag = string.CompareOrdinal(user1, user2) < 0;

            return flag ? $"{user1}-{user2}" : $"{user2}-{user1}";
        }
    }
}
