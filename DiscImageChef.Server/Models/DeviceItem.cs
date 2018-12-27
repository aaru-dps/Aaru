namespace DiscImageChef.Server.Models
{
    public class DeviceItem
    {
        public string Manufacturer { get; set; }
        public string Model        { get; set; }
        public string Revision     { get; set; }
        public string Bus          { get; set; }
        public int    ReportId     { get; set; }
    }
}