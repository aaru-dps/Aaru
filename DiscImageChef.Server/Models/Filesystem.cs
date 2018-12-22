using System.ComponentModel.DataAnnotations;

namespace DiscImageChef.Server.Models
{
    public class Filesystem
    {
        [Key]
        public int Id { get;       set; }
        public string Name  { get; set; }
        public long   Count { get; set; }
    }
}