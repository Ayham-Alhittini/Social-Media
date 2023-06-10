using Dating_App_Backend.DTOs;
using Dating_App_Backend.Entities;
using Dating_App_Backend.Helper;

namespace Dating_App_Backend.Interfaces
{
    public interface IMessagesRepository
    {
        void AddMessage(Message message);
        void DeleteMessage(Message message);
        Task<Message> GetMessageAsync(int Id);
        Task<Message> PreviousLast(string user1, string user2);
        Task<IEnumerable<MessageDto>> GetMessageThread(string currentName, string recipeName);
        Task<IEnumerable<MessageDto>> GetMessagesList(string currentUser);
        Task<Dictionary<int, int>> GetUnread(string currentUser);
        Task<bool> SaveAllAsync();
    }
}
