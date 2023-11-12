using Dating_App_Backend.DTOs;
using Dating_App_Backend.Entities;
using Dating_App_Backend.Interfaces;

namespace Dating_App_Backend.Data
{
    public class ChatGroupRepository : IChatGroupRepository
    {
        private readonly DataContext _context;
        public ChatGroupRepository(DataContext context)
        {
            _context = context;
        }
    }
}
