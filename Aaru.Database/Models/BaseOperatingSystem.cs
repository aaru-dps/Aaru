namespace DiscImageChef.Database.Models
{
    public abstract class BaseOperatingSystem : BaseModel<int>
    {
        public string Name         { get; set; }
        public string Version      { get; set; }
        public bool   Synchronized { get; set; }
        public ulong  Count        { get; set; }
    }
}