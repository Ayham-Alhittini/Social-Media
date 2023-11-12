using System.ComponentModel.DataAnnotations;

namespace Dating_App_Backend.DTOs
{
    public class CreateChatGroupDto
    {
        [Required]
        public string GroupName { get; set; }
        public string GroupDescription { get; set; }
        
        public IFormFile GroupPicture { get; set; }
    }
}
