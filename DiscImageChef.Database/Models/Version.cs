using System.ComponentModel.DataAnnotations;

namespace DiscImageChef.Database.Models
{
    public class Version
    {
        [Key]
        public int Id { get;              set; }
        public string Value        { get; set; }
        public bool   Synchronized { get; set; }
    }
}