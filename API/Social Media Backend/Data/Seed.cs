using CloudinaryDotNet.Actions;
using Dating_App_Backend.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Dating_App_Backend.Data
{
    public class Seed
    {
        public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, DataContext context)
        {
            if (await userManager.Users.AnyAsync()) return;

            var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");

            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

            var roles = new List<AppRole>
            {
                new AppRole{Name = "Member"},
                new AppRole{Name = "Admin"},
                new AppRole{Name = "Moderator"},
            };

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);
            }

            foreach ( var user in users )
            {

                user.UserName = user.UserName.ToLower();

                await userManager.CreateAsync(user, "Pa$$w0rd");
                await userManager.AddToRoleAsync(user, "Member");
            }

            var admin = new AppUser
            {
                UserName = "admin"
            };
            await userManager.CreateAsync(admin, "Pa$$w0rd");
            await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });


            ///seeding for the chat group participant roles

            var ParticipantRoles = new List<ChatGroupParticipantRole>()
            {
                new ChatGroupParticipantRole
                {
                    RoleName = ParticipantRolesSrc.GroupCreator
                },
                new ChatGroupParticipantRole
                {
                    RoleName = ParticipantRolesSrc.GroupAdmin
                },
                new ChatGroupParticipantRole
                {
                    RoleName = ParticipantRolesSrc.GroupParticipant
                }
            };

            await context.AddRangeAsync(ParticipantRoles);
            await context.SaveChangesAsync();
        }
    }
}
