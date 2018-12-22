using System.ComponentModel.DataAnnotations;

namespace DiscImageChef.Server.Models
{
    public class Media
    {
        [Key]
        public int Id { get;       set; }
        public string Type  { get; set; }
        public bool   Real  { get; set; }
        public long   Count { get; set; }
    }
}