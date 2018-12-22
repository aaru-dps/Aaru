using System.ComponentModel.DataAnnotations;

namespace DiscImageChef.Server.Models
{
    public class Version
    {
        [Key]
        public int Id { get;       set; }
        public string Value { get; set; }
        public long   Count { get; set; }
    }
}