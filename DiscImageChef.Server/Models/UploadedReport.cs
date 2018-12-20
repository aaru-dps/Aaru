using System;
using DiscImageChef.CommonTypes.Metadata;

namespace DiscImageChef.Server.Models
{
    public class UploadedReport : DeviceReportV2
    {
        public UploadedReport()
        {
            UploadedWhen = DateTime.UtcNow;
        }

        public UploadedReport(DeviceReportV2 report)
        {
            ATA            = report.ATA;
            ATAPI          = report.ATAPI;
            CompactFlash   = report.CompactFlash;
            FireWire       = report.FireWire;
            UploadedWhen   = DateTime.UtcNow;
            MultiMediaCard = report.MultiMediaCard;
            PCMCIA         = report.PCMCIA;
            SCSI           = report.SCSI;
            SecureDigital  = report.SecureDigital;
            USB            = report.USB;
            Manufacturer   = report.Manufacturer;
            Model          = report.Model;
            Revision       = report.Revision;
            Type           = report.Type;
        }

        public DateTime UploadedWhen { get; set; }
    }
}