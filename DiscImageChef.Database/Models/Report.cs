using System;
using DiscImageChef.CommonTypes.Metadata;

namespace DiscImageChef.Database.Models
{
    public class Report : DeviceReportV2
    {
        public Report(DeviceReportV2 report)
        {
            ATA            = report.ATA;
            ATAPI          = report.ATA;
            CompactFlash   = report.CompactFlash;
            FireWire       = report.FireWire;
            Created        = DateTime.UtcNow;
            MultiMediaCard = report.MultiMediaCard;
            PCMCIA         = report.PCMCIA;
            SCSI           = report.SCSI;
            SecureDigital  = report.SecureDigital;
            USB            = report.USB;
            Uploaded       = false;
        }

        public DateTime Created  { get; set; }
        public bool     Uploaded { get; set; }
    }
}