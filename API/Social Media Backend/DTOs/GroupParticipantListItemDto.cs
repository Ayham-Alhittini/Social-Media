namespace Dating_App_Backend.DTOs
{
    public class GroupParticipantListItemDto
    {
        public string ChatGroupId { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantPhotoUrl { get; set; }
        public string ParticipantUserName { get; set; }
        public string ParticipantKnownAs { get; set; }
        public string ParticipantRoleId { get; set; }
        public string ParticipantRoleName { get; set; }
    }
}
