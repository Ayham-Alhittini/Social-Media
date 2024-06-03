using AutoMapper;
using Dating_App_Backend.Data;
using Dating_App_Backend.DTOs;
using Dating_App_Backend.Entities;
using Dating_App_Backend.Extensions;
using Dating_App_Backend.Helper;
using Dating_App_Backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Dating_App_Backend.SignalR
{
    [Authorize]
    public class MessageHub: Hub
    {
        private readonly IMessagesRepository _messagesRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly DataContext _context;

        public MessageHub(IMessagesRepository messagesRepository, IUserRepository userRepository, IMapper mapper,
            IHubContext<PresenceHub> hubContext, DataContext context)
        {
            _messagesRepository = messagesRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _presenceHub = hubContext;
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            string groupName = httpContext.Request.Query["groupName"];

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await AddToGroup(groupName);

            string UserName = Context.User.GetUsername();

            ///to subtract from total messages indicator
            int unread = await _messagesRepository.GetUnreadCountFromUser(Context.User.GetUsername(), groupName);

            await Clients.Caller.SendAsync("UnreadMessagesCount", unread);

            var connectionRecored = await _context.ConnectionRecorders
                .FindAsync(UserName, groupName);

            if (connectionRecored == null)///if first time open this chat
            {
                connectionRecored = new Entities.ConnectionRecorder
                {
                    UserName = Context.User.GetUsername(),
                    GroupName = groupName,
                    ConnectionDate = DateTime.UtcNow,
                    DisconnectionDate = DateTime.MaxValue
                };
                _context.ConnectionRecorders.Add(connectionRecored);
            }
            else
            {
                connectionRecored.ConnectionDate = DateTime.UtcNow;
                connectionRecored.DisconnectionDate = DateTime.MaxValue;
            }
            //tell everybody i am here
            await Clients.OthersInGroup(groupName).SendAsync("LastConnection", connectionRecored.DisconnectionDate);
            await _context.SaveChangesAsync();

            var messages = await _messagesRepository
                .GetMessageThread(groupName);

            ///we get last connection for the recipient to know which text do he read and which not
            var lastConnection = await _messagesRepository
                .GetLastConnectionRecored(Context.User.GetUsername(), groupName);

            await Clients.Caller.SendAsync("LastConnection", lastConnection);
            await Clients.Group(groupName).SendAsync("ReciveMessageThread", messages);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            string currectUserName = Context.User.GetUsername();
            var sender = await _userRepository.GetUserByNameAsync(currectUserName);

            //single chat
            if (CommonMethod.IsSingleChat(createMessageDto.GroupName))
            {
                var chatUsers = createMessageDto.GroupName.Split("-");

                ///if not part of the chat forbid him
                if (chatUsers[0] != currectUserName && chatUsers[1] != currectUserName)
                {
                    throw new HubException("Forbid, not part of the chat");
                }

                string recipientUserName = chatUsers[0] == currectUserName ? chatUsers[1] : chatUsers[0];

                
                var recipenet = await _userRepository.GetUserByNameAsync(recipientUserName);

                ///in case recipient not exists
                if (recipenet == null)
                {
                    throw new HubException("User not found");
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
                    //we need this even though the recipient not online | because the caller get the message from it as well
                    await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));

                    ///notify recipient
                    var presenceConnections = await PresenceTracker.GetUserConnections(recipientUserName);
                    if (presenceConnections != null)//recipient online
                    {
                        var group = await _messagesRepository.GetMessageGroup(createMessageDto.GroupName);
                        ///if not connected to the chat
                        if (!group.Connections.Any(c => c.UserName == recipientUserName))
                        {
                            await _presenceHub.Clients.Clients(presenceConnections).SendAsync("NewMessageRecived", new
                            {
                                username = sender.UserName,
                                knownAs = sender.KnownAs,
                                message = _mapper.Map<MessageDto>(message)
                            });
                        }
                    }
                }
                else
                {
                    throw new HubException("Unable to send message");
                }
            }
            //chat group
            else
            {
                ///check if caller is on the group
                var participant = await _context.ChatGroupParticipants
                .Where(p => p.ParticipantId == Context.User.GetUserId() && p.ChatGroupId == createMessageDto.GroupName)
                .Include(p => p.ChatGroup)
                .FirstOrDefaultAsync();

                if (participant == null)
                {
                    throw new HubException("Forbid, not part of the chat");
                }

                //send the message
                var message = new Message
                {
                    IsGroupMessage = true,
                    ChatGroupId = createMessageDto.GroupName,
                    Content = createMessageDto.Content,
                    RecipenetUsername = participant.ChatGroup.GroupName,
                    SenderUsername =  currectUserName,
                    Sender = sender,
                    GroupName = createMessageDto.GroupName
                };

                _messagesRepository.AddMessage(message);

                if (await _messagesRepository.SaveAllAsync())
                {
                    await Clients.Group(createMessageDto.GroupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));


                    ///notify the participants

                    var group = await _messagesRepository.GetMessageGroup(createMessageDto.GroupName);

                    var groupParticipants = await _context.ChatGroupParticipants
                        .Where(g => g.ChatGroupId == createMessageDto.GroupName)
                        .Select(g => g.ParticipantUserName)
                        .ToListAsync();

                    foreach (var groupParticipant in groupParticipants)
                    {
                        /// if this participant is online and not connected to the chat then send him a notification

                        var presenceConnections = await PresenceTracker.GetUserConnections(groupParticipant);

                        if (presenceConnections != null && !group.Connections.Any(c => c.UserName == groupParticipant))
                        {
                            await _presenceHub.Clients.Clients(presenceConnections).SendAsync("NewMessageRecived", new
                            {
                                username = sender.UserName,
                                knownAs = sender.KnownAs,
                                message = _mapper.Map<MessageDto>(message)
                            });
                        }
                    }
                }
                else
                {
                    throw new HubException("Unable to send message");
                }
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {

            //set the connection recorder disconnection date

            ///find the group name
            var connection = await _context.Connections
                .FirstOrDefaultAsync(c => c.ConnectionId == Context.ConnectionId);

            var connectionRecorder = await _context.ConnectionRecorders
                .FindAsync(Context.User.GetUsername(), connection.GroupName);

            if (connectionRecorder == null)
            {
                throw new HubException("Connectoin recored can not be null!");
            }

            connectionRecorder.DisconnectionDate = DateTime.UtcNow;

            ///tell eveybody i am leaving
            await Clients.OthersInGroup(connection.GroupName)
                .SendAsync("LastConnection", connectionRecorder.DisconnectionDate);

            await _context.SaveChangesAsync();

            await RemoveFromMessageGroup();

            await base.OnDisconnectedAsync(exception);
        }

        private async Task<bool> AddToGroup(string groupName)
        {
            var group = await _messagesRepository.GetMessageGroup(groupName);
            var connection = new Entities.Connection(Context.ConnectionId, Context.User.GetUsername());
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
