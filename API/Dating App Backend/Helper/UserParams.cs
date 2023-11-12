namespace Dating_App_Backend.Helper
{
    public class UserParams: PaginationParams
    {
        public int UserId { get; set; }
        public string CurrentUserName { get; set; }
        public string Gender { get; set; }
        public int MinAge { get; set; } = 18;
        public int MaxAge { get; set; } = 100;
        public string OrderBy { get; set; } = "lastActive";

    }
}
