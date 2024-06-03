namespace Dating_App_Backend.DTOs
{
    public class GroupDto
    {
        public string Id { get; set; }
        public string GroupName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string GroupPhotoUrl { get; set; }
        public string GroupDescription { get; set; }
        public List<GroupParticipantListItemDto> GroupParticipants { get; set; } = new List<GroupParticipantListItemDto>();
    }
}
