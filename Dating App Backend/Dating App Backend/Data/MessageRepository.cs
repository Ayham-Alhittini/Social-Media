using AutoMapper;
using AutoMapper.QueryableExtensions;
using Dating_App_Backend.DTOs;
using Dating_App_Backend.Entities;
using Dating_App_Backend.Helper;
using Dating_App_Backend.Interfaces;
using Microsoft.AspNetCore.Mvc;
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
        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
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

        public async Task<IEnumerable<MessageDto>> GetMessagesList(string currentUser)
        {
            var messages =  await _context.Messages
                .Where(m => m.LastMessage && (m.RecipenetUsername == currentUser || m.SenderUsername == currentUser))
                .OrderByDescending(m => m.MessageSent)
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .ToListAsync();


            var unread = await GetUnread(currentUser);

            foreach(var message in messages)
            {
                if (unread.ContainsKey(message.Id))
                {
                    message.UnreadCount = unread[message.Id];
                }
            }

            return messages;
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentName, string recipeName)
        {
            var messages = await _context.Messages.
                Where
                (
                    m => (m.SenderUsername == currentName && m.RecipenetUsername == recipeName && !m.SenderDeleted) ||
                    (m.SenderUsername == recipeName && m.RecipenetUsername == currentName && !m.RecipenetDeleted)
                ).Include(s => s.Sender).ThenInclude(p => p.Photos)
                .Include(s => s.Recipenet).ThenInclude(p => p.Photos).
                OrderBy(d => d.MessageSent).ToListAsync();

            var unread = _context.Messages.
                Where
                (
                    m => m.DateRead == null && m.RecipenetUsername == currentName
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

        public async Task<Dictionary<int, int>> GetUnread(string currentUser)
        {
            Dictionary<int, int> unreadCounter = new Dictionary<int, int>();

            var messages =  await _context.Messages
                .Where(m => m.RecipenetUsername == currentUser && m.DateRead == null)
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .ToListAsync();


            foreach (var message in messages)
            {
                if (unreadCounter.ContainsKey(message.Id))
                {
                    unreadCounter[message.Id]++;
                }
                else
                {
                    unreadCounter.Add(message.Id, 1);
                }
            }

            return unreadCounter;

        }

        public async Task<Message> PreviousLast(string user1, string user2)
        {
            return await _context.Messages
                .Where(m => (m.SenderUsername == user1 && m.RecipenetUsername == user2 && m.LastMessage) || 
                    (m.SenderUsername == user2 && m.RecipenetUsername == user1 && m.LastMessage)
                )
                .FirstOrDefaultAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

    }
}
