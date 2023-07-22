using AutoMapper;
using Dating_App_Backend.DTOs;
using Dating_App_Backend.Entities;
using Dating_App_Backend.Extensions;
using Dating_App_Backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Dating_App_Backend.SignalR
{
    [Authorize]
    public class MessageHub: Hub
    {
        private readonly IMessagesRepository _messagesRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IHubContext<PresenceHub> _presenceHub;

        public MessageHub(IMessagesRepository messagesRepository, IUserRepository userRepository, IMapper mapper,
            IHubContext<PresenceHub> hubContext)
        {
            _messagesRepository = messagesRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _presenceHub = hubContext;

        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"];
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await AddToGroup(groupName);



            int unread = await _messagesRepository.GetUnreadCountFromUser(Context.User.GetUsername(), otherUser);

            await Clients.Caller.SendAsync("UnreadMessagesCount", unread);

            var messages = await _messagesRepository
                .GetMessageThread(Context.User.GetUsername(), otherUser);

            await Clients.Group(groupName).SendAsync("ReciveMessageThread", messages);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var sender = await _userRepository.GetUserByNameAsync(Context.User.GetUsername());
            var recipenet = await _userRepository.GetUserByNameAsync(createMessageDto.RecipenetUsername);

            if (recipenet == null)
            {
                throw new HubException("User not found");
            }


            var groupName = GetGroupName(sender.UserName, recipenet.UserName);

            var message = new Message
            {
                Sender = sender,
                Recipenet = recipenet,
                SenderUsername = sender.UserName,
                RecipenetUsername = recipenet.UserName,
                Content = createMessageDto.Content,
                GroupName = groupName,
            };

            
            var group = await _messagesRepository.GetMessageGroup(groupName);

            if (group.Connections.Any(x => x.UserName == recipenet.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await PresenceTracker.GetUserConnections(recipenet.UserName);
                if (connections != null) //user is online
                {
                    await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageRecived", new {
                        username = sender.UserName, knownAs = sender.KnownAs, message = _mapper.Map<MessageDto>(message)
                    });
                }

            }


            _messagesRepository.AddMessage(message);

            if (await _messagesRepository.SaveAllAsync())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await RemoveFromMessageGroup();
            await base.OnDisconnectedAsync(exception);
        }

        private string GetGroupName(string user1, string user2)
        {
            var flag = string.CompareOrdinal(user1, user2) < 0;

            return flag ? $"{user1}-{user2}" : $"{user2}-{user1}";
        }


        private async Task<bool> AddToGroup(string groupName)
        {
            var group = await _messagesRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());
            if (group == null)
            {
                group = new Group(groupName);
                _messagesRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            return await _messagesRepository.SaveAllAsync();
        }

        private async Task RemoveFromMessageGroup()
        {
            var connection = await _messagesRepository.GetConnection(Context.ConnectionId);
            _messagesRepository.RemoveConnection(connection);
            await _messagesRepository.SaveAllAsync();
        }
    }
}
