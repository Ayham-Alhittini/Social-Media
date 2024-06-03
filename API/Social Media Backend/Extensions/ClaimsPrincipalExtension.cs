using System.Security.Claims;

namespace Dating_App_Backend.Extensions
{
    public  static class ClaimsPrincipalExtension
    {
        public static string GetUsername(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value;
        }
        public static int GetUserId(this ClaimsPrincipal user)
        {
            return int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
        public static string GetKnownAs(this ClaimsPrincipal user)
        {
            return user.FindFirst("KnownAs")?.Value;
        }
    }
}
