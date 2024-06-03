using Dapper;
using Dating_App_Backend.DTOs;
using Dating_App_Backend.Entities;
using Dating_App_Backend.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Dating_App_Backend.Data
{
    public class MessageRepository : IMessagesRepository
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;

        public MessageRepository(DataContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
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

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups.Include(x => x.Connections)
                .FirstOrDefaultAsync(x => x.Name == groupName);
        }


        public async Task<IEnumerable<MessageDto>> GetMessageThread(string groupName)
        {
            string connectionString = _config.GetConnectionString("defaultConection");

            using var connection = new SqlConnection(connectionString);

            string query =
                @"
                    select 
                    Id,
                    SenderUsername,
                    (select url from Photos where AppUserId = SenderId) as SenderPhotoUrl,
                    Content,
                    GroupName,
                    MessageSent,
                    IsGroupMessage,
                    IsSystemMessage
                    from Messages
                    where GroupName = @groupName
                    order by MessageSent
                ";

            var messages = await connection.QueryAsync<MessageDto>(query, new { groupName = groupName });


            return messages;
        }

        public async Task<DateTime> GetLastConnectionRecored(string UserName, string GroupName)///UserName = Caller
        {
            //if it's a group chat then return max value | so it's look like it's read even though no one see it
            var participants = GroupName.Split('-');
            if (participants.Length > 2)
            {
                return DateTime.MaxValue;
            }

            string recipientUserName = participants[0] == UserName ? participants[1] : participants[0];

            var lastConnection = await _context.ConnectionRecorders
                .FindAsync(recipientUserName, GroupName);

            if (lastConnection == null) return DateTime.MinValue;

            return lastConnection.DisconnectionDate;
        }

        public async Task<int> GetUnreadCount(string username)
        {
            string connectionString = _config.GetConnectionString("defaultConection");
            using var connection = new SqlConnection(connectionString);
            string query =
                @"
                WITH UnreadMessages as (
	                select GroupName, COUNT(*) as UnreadMessages from Messages
	                where (GroupName like '%' + @user + '%' or GroupName in (select ChatGroupId from ChatGroupParticipants where ParticipantUserName = @user)) 
	                and MessageSent > coalesce((select DisconnectionDate from ConnectionRecorders where UserName = @user and GroupName = Messages.GroupName), '0001-01-01')
	                Group by GroupName
                )
                select coalesce(SUM(UnreadMessages), 0) as TotalUnread from UnreadMessages
                ";

            return await connection.QueryFirstOrDefaultAsync<int>(query, new { user = username });
        }

        public async Task<int> GetUnreadCountFromUser(string username, string groupName)
        {
            string connectionString = _config.GetConnectionString("defaultConection");
            using var connection = new SqlConnection(connectionString);

            string query =
                @"
                select COUNT(*) as ReadMessages from Messages 
                where GroupName = @groupName and MessageSent > coalesce((
	                select DisconnectionDate from ConnectionRecorders
	                where UserName = @user and GroupName = @groupName
                ), '0001-01-01');
                ";
            return await connection.QueryFirstOrDefaultAsync<int>(query, new { user = username , groupName });
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
