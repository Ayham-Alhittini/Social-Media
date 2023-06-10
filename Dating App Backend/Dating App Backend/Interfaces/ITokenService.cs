using Dating_App_Backend.Entities;

namespace Dating_App_Backend.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateToken(AppUser user);
    }
}
