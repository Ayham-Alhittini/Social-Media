namespace Dating_App_Backend.Entities
{
    public class ChatGroupParticipantRole
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string RoleName { get; set; }
    }
}
