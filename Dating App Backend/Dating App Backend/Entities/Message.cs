namespace Dating_App_Backend.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderUsername { get; set; }
        public AppUser Sender { get; set; }
        public int RecipenetId { get; set; }
        public string RecipenetUsername { get; set; }
        public AppUser Recipenet { get; set; }
        public string Content { get; set; }
        public DateTime? DateRead { get; set; }
        public DateTime MessageSent { get; set; } = DateTime.UtcNow;
        public bool SenderDeleted { get; set; }
        public bool RecipenetDeleted { get; set; }
        public string GroupName { get; set; }

    }
}
