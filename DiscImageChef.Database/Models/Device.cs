using System;
using DiscImageChef.CommonTypes.Metadata;

namespace DiscImageChef.Database.Models
{
    public class Device : DeviceReportV2
    {
        public DateTime LastSynchronized { get; set; }
    }
}