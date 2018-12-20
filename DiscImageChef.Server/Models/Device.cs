using System;
using DiscImageChef.CommonTypes.Metadata;

namespace DiscImageChef.Server.Models
{
    public class Device : DeviceReportV2
    {
        public Device()
        {
            AddedWhen = DateTime.UtcNow;
        }

        public Device(DeviceReportV2 report)
        {
            ATA            = report.ATA;
            ATAPI          = report.ATAPI;
            CompactFlash   = report.CompactFlash;
            FireWire       = report.FireWire;
            AddedWhen      = DateTime.UtcNow;
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

        public DateTime AddedWhen { get; set; }
    }
}