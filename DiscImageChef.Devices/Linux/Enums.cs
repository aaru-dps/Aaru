using System;

namespace DiscImageChef.Devices.Linux
{
    static class Enums
    {
        internal const int O_RDONLY = 0;
        internal const int O_RDWR = 2;

        internal const ulong SG_GET_VERSION_NUM = 0x2282;
        internal const ulong SG_IO = 0x2285;

        internal const int SG_DXFER_FROM_DEV = -3;

        internal const uint SG_INFO_OK_MASK = 0x1;
        internal const uint SG_INFO_OK = 0x0;          /* no sense, host nor driver "noise" */
        internal const uint SG_INFO_CHECK = 0x1;
    }
}

