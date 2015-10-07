using System;

namespace DiscImageChef.Devices.Linux
{
    [Flags]
    enum FileFlags : int
    {
        /// <summary>
        /// O_RDONLY
        /// </summary>
        Readonly = 00000000,
        /// <summary>
        /// O_WRONLY
        /// </summary>
        Writeonly = 00000001,
        /// <summary>
        /// O_RDWR
        /// </summary>
        ReadWrite = 00000002,
        /// <summary>
        /// O_CREAT
        /// </summary>
        OpenOrCreate = 00000100,
        /// <summary>
        /// O_EXCL
        /// </summary>
        CreateNew = 00000200,
        /// <summary>
        /// O_NOCTTY
        /// </summary>
        NoControlTTY = 00000400,
        /// <summary>
        /// O_TRUNC
        /// </summary>
        Truncate = 00001000,
        /// <summary>
        /// O_APPEND
        /// </summary>
        Append = 00002000,
        /// <summary>
        /// O_NONBLOCK
        /// </summary>
        NonBlocking = 00004000,
        /// <summary>
        /// O_DSYNC
        /// </summary>
        Synchronous = 00010000,
        /// <summary>
        /// O_ASYNC
        /// </summary>
        Async = 00020000,
        /// <summary>
        /// O_DIRECT
        /// </summary>
        Direct = 00040000,
        /// <summary>
        /// O_LARGEFILE
        /// </summary>
        LargeFile = 00100000,
        /// <summary>
        /// O_DIRECTORY
        /// </summary>
        Directory = 00200000,
        /// <summary>
        /// O_NOFOLLOW
        /// </summary>
        NoFollowSymlink = 00400000,
        /// <summary>
        /// O_NOATIME
        /// </summary>
        NoAccessTime = 01000000,
        /// <summary>
        /// O_CLOEXEC
        /// </summary>
        CloseOnExec = 02000000
    }

    /// <summary>
    /// Direction of SCSI transfer
    /// </summary>
    enum ScsiIoctlDirection : int
    {
        /// <summary>
        /// No data transfer happens
        /// SG_DXFER_NONE
        /// </summary>
        None = -1,
        /// <summary>
        /// From host to device
        /// SG_DXFER_TO_DEV
        /// </summary>
        Out = -2,
        /// <summary>
        /// From device to host
        /// SG_DXFER_FROM_DEV
        /// </summary>
        In = -3,
        /// <summary>
        /// Bidirectional device/host
        /// SG_DXFER_TO_FROM_DEV
        /// </summary>
        Unspecified = -4,
        /// <summary>
        /// Unspecified
        /// SG_DXFER_UNKNOWN
        /// </summary>
        Unknown = -5
    }

    enum LinuxIoctl : ulong
    {
        // SCSI IOCtls
        SG_EMULATED_HOST = 0x2203,
        SG_GET_VERSION_NUM = 0x2282,
        SG_IO = 0x2285,
        SG_GET_SCSI_ID = 0x2276,

        // ATA IOCtls
        HDIO_DRIVE_CMD = 0x031F,
        HDIO_DRIVE_TASK = 0x031E,
        HDIO_DRIVE_TASKFILE = 0x031D
    }

    [Flags]
    enum SgInfo : uint
    {
        /// <summary>
        /// Mask to check OK
        /// </summary>
        OkMask = 0x01,
        /// <summary>
        /// No sense or driver noise
        /// </summary>
        Ok = 0x00,
        /// <summary>
        /// Check Condition
        /// </summary>
        CheckCondition = 0x01,

        /// <summary>
        /// Direct I/O mask
        /// </summary>
        DirectIoMask = 0x06,
        /// <summary>
        /// Transfer via kernel buffers (or no transfer)
        /// </summary>
        IndirectIo = 0x00,
        /// <summary>
        /// Direct I/O performed
        /// </summary>
        DirectIo = 0x02,
        /// <summary>
        /// Partial direct and partial indirect I/O
        /// </summary>
        MixedIo = 0x04
    }
}

