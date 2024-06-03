using Dating_App_Backend.Entities;

namespace Dating_App_Backend.DTOs
{
    public class LikeDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public int Age { get; set; }
        public string KnownAs { get; set; }
        public string PhotoUrl { get; set; }
        public bool IsLiked { get; set; }
    }
}
