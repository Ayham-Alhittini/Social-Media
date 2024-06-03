namespace Dating_App_Backend.DTOs
{
    public class MessageChatListItemDto
    {
        public string ChatPhoto { get; set; }
        public string ChatName { get; set; }
        public string Content { get; set; }
        public string SenderUsername { get; set; }
        public DateTime MessageSent { get; set; }
        public int UnreadCount { get; set; }
        public string GroupName { get; set; }
        public bool IsGroupMessage { get; set; }
        public bool IsSystemMessage { get; set; }
    }
}
