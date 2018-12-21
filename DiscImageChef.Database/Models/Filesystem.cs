using System.ComponentModel.DataAnnotations;

namespace DiscImageChef.Database.Models
{
    public class Filesystem
    {
        [Key]
        public int Id { get;              set; }
        public string Name         { get; set; }
        public bool   Synchronized { get; set; }
    }
}