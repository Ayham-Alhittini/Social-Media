using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dating_App_Backend.Entities
{
    public class ChatGroup
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string GroupName { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        [ForeignKey("GroupCreator")]
        public int GroupCreatorId { get; set; }
        public AppUser GroupCreator { get; set; }

        public List<ChatGroupParticipant> GroupParticipants { get; set; }

        public string GroupPhotoUrl { get; set; }

        public string GroupPhotoId { get; set; }
        public AppFile GroupPhoto { get; set; }
        public string GroupDescription { get; set; }
        public ICollection<ParticipantGroupInvite> GroupInvites { get; set; }
        public ICollection<Message> GroupMessages { get; set; }
    }
}
