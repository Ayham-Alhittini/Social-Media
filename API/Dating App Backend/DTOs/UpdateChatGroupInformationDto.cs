using System.ComponentModel.DataAnnotations;

namespace Dating_App_Backend.DTOs
{
    public class UpdateChatGroupInformationDto
    {
        [Required]
        public string GroupId { get; set; }
        [Required]
        public string GroupName { get; set; }
        [Required]
        public string GroupDescription { get; set; }
    }
}
