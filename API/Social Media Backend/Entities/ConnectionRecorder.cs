namespace Dating_App_Backend.Entities
{
    public class ConnectionRecorder
    {
        public string UserName { get; set; }
        public string GroupName { get; set; }
        public DateTime ConnectionDate { get; set; }
        public DateTime DisconnectionDate { get; set; }
    }
}
