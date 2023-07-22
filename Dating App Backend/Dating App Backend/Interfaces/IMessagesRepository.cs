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
        Task<IEnumerable<MessageDto>> GetMessageThread(string currentName, string recipeName);
        Task<bool> SaveAllAsync();

        void AddGroup(Group group);
        void RemoveConnection(Connection connection);
        Task<Connection> GetConnection(string connectionId);
        Task<Group> GetMessageGroup(string groupName);

        Task<int> GetUnreadCount(string username);
        Task<int> GetUnreadCountFromUser(string username, string senderUsername);

    }
}
