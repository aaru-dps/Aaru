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
        bool            aborted;

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
            aborted          = false;
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

        public void Abort()
        {
            aborted = true;
        }

        /// <summary>
        ///     Event raised when the progress bar is not longer needed
        /// </summary>
        public event EndProgressHandler EndProgress;
        /// <summary>
        ///     Event raised when a progress bar is needed
        /// </summary>
        public event InitProgressHandler InitProgress;
        /// <summary>
        ///     Event raised to report status updates
        /// </summary>
        public event UpdateStatusHandler UpdateStatus;
        /// <summary>
        ///     Event raised to report a fatal error that stops the dumping operation and should call user's attention
        /// </summary>
        public event ErrorMessageHandler StoppingErrorMessage;
        /// <summary>
        ///     Event raised to update the values of a determinate progress bar
        /// </summary>
        public event UpdateProgressHandler UpdateProgress;
        /// <summary>
        ///     Event raised to update the status of an undeterminate progress bar
        /// </summary>
        public event PulseProgressHandler PulseProgress;
        public event ScanTimeHandler       ScanTime;
        public event ScanUnreadableHandler ScanUnreadable;
    }
}