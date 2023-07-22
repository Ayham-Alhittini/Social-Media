using System.ComponentModel.DataAnnotations;

namespace Dating_App_Backend.Entities
{
    public class Group
    {
        public Group()
        {
            
        }
        public Group(string name)
        {
            Name = name;
        }

        [Key]
        public string Name { get; set; }
        public ICollection<Connection> Connections { get; set; } = new List<Connection>();
    }
}
