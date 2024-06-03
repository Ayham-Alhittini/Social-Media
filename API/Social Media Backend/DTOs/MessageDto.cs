namespace Dating_App_Backend.DTOs
{
    public class MessageDto
    {
        public int Id { get; set; }
        public string SenderUsername { get; set; }
        public string SenderPhotoUrl { get; set; }
        public string Content { get; set; }
        public string GroupName { get; set; }
        public DateTime MessageSent { get; set; }
        public bool IsGroupMessage { get; set; }
        public bool IsSystemMessage { get; set; }
    }
}
