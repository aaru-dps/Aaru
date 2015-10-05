using System;

namespace DiscImageChef.Devices.Windows
{
    static class Enums
    {
        internal const uint FILE_SHARE_READ = 0x1;
        internal const uint FILE_SHARE_WRITE = 0x1;

        internal const uint OPEN_EXISTING = 0x3;

        internal const uint FILE_ATTRIBUTE_NORMAL = 0x80;

        internal const uint GENERIC_READ = 0x80000000;
        internal const uint GENERIC_WRITE = 0x40000000;

        internal const byte SCSI_IOCTL_DATA_OUT = 0; //Give data to SCSI device (e.g. for writing)
        internal const byte SCSI_IOCTL_DATA_IN       =       1; //Get data from SCSI device (e.g. for reading)
        internal const byte SCSI_IOCTL_DATA_UNSPECIFIED =    2; //No data (e.g. for ejecting)

        internal const uint IOCTL_SCSI_PASS_THROUGH_DIRECT = 0x4D014;
    }
}

