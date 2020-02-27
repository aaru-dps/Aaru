using System.ComponentModel.DataAnnotations;

namespace Aaru.Database.Models
{
    public abstract class BaseModel<T>
    {
        [Key]
        public int Id { get; set; }
    }
}