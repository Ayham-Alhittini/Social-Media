using AutoMapper;
using AutoMapper.QueryableExtensions;
using Dating_App_Backend.DTOs;
using Dating_App_Backend.Entities;
using Dating_App_Backend.Helper;
using Dating_App_Backend.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dating_App_Backend.Data
{
    public class MessageRepository : IMessagesRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public MessageRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Message> GetMessageAsync(int Id)
        {
            return await _context.Messages.FindAsync(Id);
        }

        public async Task<PagedList<MessageDto>> GetMessageForUser(MessageParams messageParams)
        {
            var query = _context.Messages.OrderByDescending(x => x.MessageSent).AsQueryable();

            query = messageParams.Container switch
            {
                "inbox" => query.Where(x => x.RecipenetUsername == messageParams.Username && !x.RecipenetDeleted),
                "outbox" => query.Where(x => x.SenderUsername == messageParams.Username && !x.SenderDeleted),
                _ => query.Where(x => x.RecipenetUsername == messageParams.Username && x.DateRead == null)
            };

            return await PagedList<MessageDto>.CreateAsync(
                query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider).AsNoTracking(),
                messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups.Include(x => x.Connections)
                .FirstOrDefaultAsync(x => x.Name == groupName);
        }


        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentName, string otherName)
        {
            var messages = await _context.Messages.
                Where
                (
                    m => (m.SenderUsername == currentName && m.RecipenetUsername == otherName && !m.SenderDeleted) ||
                    (m.SenderUsername == otherName && m.RecipenetUsername == currentName && !m.RecipenetDeleted)
                ).Include(s => s.Sender).ThenInclude(p => p.Photos)
                .Include(s => s.Recipenet).ThenInclude(p => p.Photos).
                OrderBy(d => d.MessageSent).ToListAsync();


            var unread = _context.Messages.
                Where
                (
                    m => m.DateRead == null && m.RecipenetUsername == currentName && m.SenderUsername == otherName
                ).ToList();

            if (unread.Any())
            {
                foreach (var  message in unread) 
                { 
                    message.DateRead = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
            }

            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<int> GetUnreadCount(string username)
        {
            return await _context.Messages.Where(msg => msg.RecipenetUsername == username && msg.DateRead == null).CountAsync();
        }

        public async Task<int> GetUnreadCountFromUser(string username, string senderUsername)
        {
            return await _context.Messages.Where(msg => msg.DateRead == null &&  msg.RecipenetUsername == username && msg.SenderUsername == senderUsername).CountAsync();
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

    }
}
