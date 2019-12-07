using System.ComponentModel.DataAnnotations;

namespace DiscImageChef.Database.Models
{
    public abstract class BaseModel<T>
    {
        [Key]
        public int Id { get; set; }
    }
}