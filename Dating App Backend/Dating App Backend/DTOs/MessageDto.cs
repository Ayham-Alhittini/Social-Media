using Dating_App_Backend.Entities;

namespace Dating_App_Backend.DTOs
{
    public class MessageDto
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderUsername { get; set; }
        public string SenderPhotoUrl { get; set; }
        public int RecipenetId { get; set; }
        public string RecipenetUsername { get; set; }
        public string RecipenetPhotoUrl { get; set; }
        public string Content { get; set; }
        public DateTime? DateRead { get; set; }
        public DateTime MessageSent { get; set; }
        public int UnreadCount { get; set; }
    }
}
