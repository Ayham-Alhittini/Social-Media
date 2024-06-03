using Dating_App_Backend.DTOs;
using Dating_App_Backend.Entities;

namespace Dating_App_Backend.Interfaces
{
    public interface IMessagesRepository
    {
        void AddMessage(Message message);
        void DeleteMessage(Message message);
        Task<Message> GetMessageAsync(int Id);
        Task<IEnumerable<MessageDto>> GetMessageThread(string groupName);
        Task<DateTime> GetLastConnectionRecored(string UserName, string GroupName);
        Task<bool> SaveAllAsync();

        void AddGroup(Group group);
        void RemoveConnection(Connection connection);
        Task<Connection> GetConnection(string connectionId);
        Task<Group> GetMessageGroup(string groupName);

        Task<int> GetUnreadCount(string username);
        Task<int> GetUnreadCountFromUser(string username, string groupName);
    }
}