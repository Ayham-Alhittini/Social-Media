using Microsoft.AspNetCore.Identity;

namespace Dating_App_Backend.Entities
{
    public class AppRole: IdentityRole<int>
    {
        public ICollection<AppUserRole> UserRoles { get; set; }

    }
}
