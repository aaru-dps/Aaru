using System;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.Devices;

namespace DiscImageChef.Core.Devices.Scanning
{
    public partial class MediaScan
    {
        readonly Device dev;
        readonly string devicePath;
        readonly string ibgLogPath;
        readonly string mhddLogPath;

        /// <param name="mhddLogPath">Path to a MHDD log file</param>
        /// <param name="ibgLogPath">Path to a IMGBurn log file</param>
        /// <param name="devicePath">Device path</param>
        /// <param name="dev">Device</param>
        public MediaScan(string mhddLogPath, string ibgLogPath, string devicePath, Device dev)
        {
            this.mhddLogPath = mhddLogPath;
            this.ibgLogPath  = ibgLogPath;
            this.devicePath  = devicePath;
            this.dev         = dev;
        }

        public ScanResults Scan()
        {
            switch(dev.Type)
            {
                case DeviceType.ATA: return Ata();
                case DeviceType.MMC:
                case DeviceType.SecureDigital: return SecureDigital();
                case DeviceType.NVMe: return Nvme();
                case DeviceType.ATAPI:
                case DeviceType.SCSI: return Scsi();
                default: throw new NotSupportedException("Unknown device type.");
            }
        }
    }
}