namespace Dating_App_Backend.DTOs
{
    public class InviteListItemDto
    {
        public int Id { get; set; }
        public string ChatGroupId { get; set; }
        public string GroupName { get; set; }
        public string GroupDesc { get; set; }
        public string InviteByKnownAs { get; set; }
    }
}
