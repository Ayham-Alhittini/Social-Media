using System.ComponentModel.DataAnnotations.Schema;

namespace Dating_App_Backend.Entities
{
    public class ParticipantGroupInvite
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public AppUser User { get; set; }
        [ForeignKey("ChatGroup")]
        public string ChatGroupId { get; set; }
        public string GroupName { get; set; }
        public string GroupDesc { get; set; }
        public int InvitedById { get; set; }
        public string InviteByKnownAs { get; set; }
        public AppUser InvitedBy { get; set; }
        public ChatGroup ChatGroup { get; set; }
    }
}
