using System.ComponentModel.DataAnnotations;

namespace DiscImageChef.Database.Models
{
    public class Media
    {
        [Key]
        public int Id { get;              set; }
        public string Type         { get; set; }
        public bool   Real         { get; set; }
        public bool   Synchronized { get; set; }
    }
}