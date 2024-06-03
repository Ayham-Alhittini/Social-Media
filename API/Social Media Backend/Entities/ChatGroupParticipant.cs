using System.ComponentModel.DataAnnotations.Schema;

namespace Dating_App_Backend.Entities
{
    public class ChatGroupParticipant
    {
        public string ChatGroupId { get; set; }
        public ChatGroup ChatGroup { get; set; }

        [ForeignKey("Participant")]
        public int ParticipantId { get; set; }
        public string ParticipantPhotoUrl { get; set; }
        public string ParticipantUserName { get; set; }
        public string ParticipantKnownAs { get; set; }
        public AppUser Participant { get; set; }
        public string ParticipantRoleId { get; set; }
        public ChatGroupParticipantRole ParticipantRole { get; set; }
        public DateTime JoinnedSince { get; set; } = DateTime.UtcNow;
    }
}
