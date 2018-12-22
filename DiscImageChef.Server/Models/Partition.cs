using System.ComponentModel.DataAnnotations;

namespace DiscImageChef.Server.Models
{
    public class Partition
    {
        [Key]
        public int Id { get;       set; }
        public string Name  { get; set; }
        public long   Count { get; set; }
    }
}