namespace Aaru.Database.Models
{
    public abstract class NameCountModel<T> : BaseModel<T>
    {
        public string Name         { get; set; }
        public bool   Synchronized { get; set; }
        public ulong  Count        { get; set; }
    }
}