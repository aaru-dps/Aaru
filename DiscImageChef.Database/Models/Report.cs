using System;
using DiscImageChef.CommonTypes.Metadata;

namespace DiscImageChef.Database.Models
{
    public class Report : DeviceReportV2
    {
        public DateTime Created  { get; set; }
        public bool     Uploaded { get; set; }
    }
}