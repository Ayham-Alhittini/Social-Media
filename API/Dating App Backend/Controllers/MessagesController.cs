using AutoMapper;
using Dapper;
using Dating_App_Backend.Data;
using Dating_App_Backend.DTOs;
using Dating_App_Backend.Entities;
using Dating_App_Backend.Extensions;
using Dating_App_Backend.Helper;
using Dating_App_Backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Dating_App_Backend.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IMessagesRepository _messagesRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly IConfiguration _config;
        public MessagesController(IMessagesRepository messagesRepository,
            IUserRepository userRepository,
            IMapper mapper, DataContext context,
            IConfiguration config)
        {
            _messagesRepository = messagesRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _context = context;
            _config = config;
        }
        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            //single chat
            if (CommonMethod.IsSingleChat(createMessageDto.GroupName))
            {
                var chatUsers = createMessageDto.GroupName.Split("-");
                string currectUserName = User.GetUsername();

                ///if not part of the chat forbid him
                if (chatUsers[0] != currectUserName && chatUsers[1] != currectUserName)
                {
                    return Forbid();
                }

                string recipientUserName = chatUsers[0] == currectUserName ? chatUsers[1] : chatUsers[0];

                var sender = await _userRepository.GetUserByNameAsync(currectUserName);
                var recipenet = await _userRepository.GetUserByNameAsync(recipientUserName);

                ///in case recipient not exists
                if (recipenet == null)
                {
                    return NotFound();
                }

                ///get group name in case they were in wrong order
                string groupName = CommonMethod.GetGroupName(chatUsers[0], chatUsers[1]);

                var message = new Message
                {
                    Sender = sender,
                    Recipenet = recipenet,
                    SenderUsername = currectUserName,
                    RecipenetUsername = recipientUserName,
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
            //chat group
            else
            {
                ///check if caller is on the group
                var participant = await _context.ChatGroupParticipants
                .Where(p => p.ParticipantId == User.GetUserId() && p.ChatGroupId == createMessageDto.GroupName)
                .Include(p => p.ChatGroup)
                .FirstOrDefaultAsync();

                if (participant == null)
                {
                    return Forbid();
                }

                //send the message
                var sender = await _userRepository.GetUserByNameAsync(User.GetUsername());

                var message = new Message
                {
                    IsGroupMessage = true,
                    ChatGroupId = createMessageDto.GroupName,
                    Content = createMessageDto.Content,
                    RecipenetUsername = participant.ChatGroup.GroupName,
                    SenderUsername = User.GetUsername(),
                    Sender = sender,
                    GroupName = createMessageDto.GroupName
                };

                _context.Messages.Add(message);

                if (await _messagesRepository.SaveAllAsync())
                {
                    return Ok(_mapper.Map<MessageDto>(message));
                }

                return BadRequest("Failed To Send The Message");
            }
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

            using var connection = new SqlConnection(_config.GetConnectionString("defaultConection"));
            string query = @"               --get user groups                WITH UserGroups AS (                    select ChatGroupId from ChatGroupParticipants where ParticipantUserName = @user                ),                --get last message identifier for the user                LastMessage AS (                    select GroupName, MAX(MessageSent) MessageSent from Messages                    where GroupName like '%' + @user +'%' or GroupName                     in (select * from UserGroups)                    group by GroupName                )                --get all messages related to the user                select                 (case when mas.IsGroupMessage = 1 then (select GroupPhotoUrl from ChatGroups where Id = LastMessage.GroupName)                     when SenderUsername = @user then (select Url from Photos where AppUserId = RecipenetId)                    else (select Url from Photos where AppUserId = SenderId)                    end) AS ChatPhoto,                (case when mas.IsGroupMessage = 1 then (select ChatGroups.GroupName from ChatGroups where Id = LastMessage.GroupName)                     when SenderUsername = @user then RecipenetUsername                    else SenderUsername                    end) AS ChatName,                (select COUNT(*) from Messages 				where 				GroupName = LastMessage.GroupName				and SenderUsername != @user 				and MessageSent > COALESCE((select DisconnectionDate from ConnectionRecorders where UserName = @user and GroupName = LastMessage.GroupName), '0001-01-01 00:00:00.000'))                as UnreadCount,                    Content,                    SenderUsername,                    LastMessage.MessageSent,                    LastMessage.GroupName,                    IsGroupMessage,                    IsSystemMessage                from Messages mas inner join LastMessage on mas.GroupName = LastMessage.GroupName and mas.MessageSent = LastMessage.MessageSent                where LastMessage.GroupName like '%' + @user +'%' or LastMessage.GroupName in (select * from UserGroups)                order by LastMessage.MessageSent desc            ";

            var result = await connection.QueryAsync<MessageChatListItemDto>(query, new { user = User.GetUsername() });

            return Ok(result);
        }

        [HttpGet("thread/{groupName}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string groupName)
        {
            /////////////////the only validation to check if user have the role to see the chat////////////////////

            //check if it's a single chat
            var users = groupName.Split('-');
            if (users.Length == 2 && (users[0] == User.GetUsername() || users[1] == User.GetUsername())) 
            {
                return Ok(await _messagesRepository.GetMessageThread(groupName));
            }

            ///check if user participant in the group
            var isParticipant = await _context.ChatGroupParticipants
                .Where(p => p.ParticipantUserName == User.GetUsername() && p.ChatGroupId == groupName)///group name for chat groups is the same as the chat group id
                .AnyAsync();

            return isParticipant ? Ok(await _messagesRepository.GetMessageThread(groupName)) : Forbid();
        }        
    }
}
