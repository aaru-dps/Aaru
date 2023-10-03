// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Windows direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains enumerations necessary for directly interfacing devices under
//     Windows.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

// ReSharper disable UnusedMember.Global

using System;
using System.Diagnostics.CodeAnalysis;

namespace Aaru.Devices.Windows;

[Flags]
enum FileAttributes : uint
{
    /// <summary>FILE_ATTRIBUTE_ARCHIVE</summary>
    Archive = 0x20,

    /// <summary>FILE_ATTRIBUTE_COMPRESSED</summary>
    Compressed = 0x800,

    /// <summary>FILE_ATTRIBUTE_DEVICE</summary>
    Device = 0x40,

    /// <summary>FILE_ATTRIBUTE_DIRECTORY</summary>
    Directory = 0x10,

    /// <summary>FILE_ATTRIBUTE_ENCRYPTED</summary>
    Encrypted = 0x4000,

    /// <summary>FILE_ATTRIBUTE_HIDDEN</summary>
    Hidden = 0x02,

    /// <summary>FILE_ATTRIBUTE_INTEGRITY_STREAM</summary>
    IntegrityStream = 0x8000,

    /// <summary>FILE_ATTRIBUTE_NORMAL</summary>
    Normal = 0x80,

    /// <summary>FILE_ATTRIBUTE_NOT_CONTENT_INDEXED</summary>
    NotContentIndexed = 0x2000,

    /// <summary>FILE_ATTRIBUTE_NO_SCRUB_DATA</summary>
    NoScrubData = 0x20000,

    /// <summary>FILE_ATTRIBUTE_OFFLINE</summary>
    Offline = 0x1000,

    /// <summary>FILE_ATTRIBUTE_READONLY</summary>
    Readonly = 0x01,

    /// <summary>FILE_ATTRIBUTE_REPARSE_POINT</summary>
    ReparsePoint = 0x400,

    /// <summary>FILE_ATTRIBUTE_SPARSE_FILE</summary>
    SparseFile = 0x200,

    /// <summary>FILE_ATTRIBUTE_SYSTEM</summary>
    System = 0x04,

    /// <summary>FILE_ATTRIBUTE_TEMPORARY</summary>
    Temporary = 0x100,

    /// <summary>FILE_ATTRIBUTE_VIRTUAL</summary>
    Virtual = 0x10000,

    /// <summary>FILE_FLAG_BACKUP_SEMANTICS</summary>
    BackupSemantics = 0x02000000,

    /// <summary>FILE_FLAG_DELETE_ON_CLOSE</summary>
    DeleteOnClose = 0x04000000,

    /// <summary>FILE_FLAG_NO_BUFFERING</summary>
    NoBuffering = 0x20000000,

    /// <summary>FILE_FLAG_OPEN_NO_RECALL</summary>
    OpenNoRecall = 0x00100000,

    /// <summary>FILE_FLAG_OPEN_REPARSE_POINT</summary>
    OpenReparsePoint = 0x00200000,

    /// <summary>FILE_FLAG_OVERLAPPED</summary>
    Overlapped = 0x40000000,

    /// <summary>FILE_FLAG_POSIX_SEMANTICS</summary>
    PosixSemantics = 0x0100000,

    /// <summary>FILE_FLAG_RANDOM_ACCESS</summary>
    RandomAccess = 0x10000000,

    /// <summary>FILE_FLAG_SESSION_AWARE</summary>
    SessionAware = 0x00800000,

    /// <summary>FILE_FLAG_SEQUENTIAL_SCAN</summary>
    SequentialScan = 0x08000000,

    /// <summary>FILE_FLAG_WRITE_THROUGH</summary>
    WriteThrough = 0x80000000
}

[Flags]
enum FileAccess : uint
{
    /// <summary>FILE_READ_DATA</summary>
    ReadData = 0x0001,

    /// <summary>FILE_LIST_DIRECTORY</summary>
    ListDirectory = ReadData,

    /// <summary>FILE_WRITE_DATA</summary>
    WriteData = 0x0002,

    /// <summary>FILE_ADD_FILE</summary>
    AddFile = WriteData,

    /// <summary>FILE_APPEND_DATA</summary>
    AppendData = 0x0004,

    /// <summary>FILE_ADD_SUBDIRECTORY</summary>
    AddSubdirectory = AppendData,

    /// <summary>FILE_CREATE_PIPE_INSTANCE</summary>
    CreatePipeInstance = AppendData,

    /// <summary>FILE_READ_EA</summary>
    ReadEa = 0x0008,

    /// <summary>FILE_WRITE_EA</summary>
    WriteEa = 0x0010,

    /// <summary>FILE_EXECUTE</summary>
    Execute = 0x0020,

    /// <summary>FILE_TRAVERSE</summary>
    Traverse = Execute,

    /// <summary>FILE_DELETE_CHILD</summary>
    DeleteChild = 0x0040,

    /// <summary>FILE_READ_ATTRIBUTES</summary>
    ReadAttributes = 0x0080,

    /// <summary>FILE_WRITE_ATTRIBUTES</summary>
    WriteAttributes = 0x0100,

    /// <summary>GENERIC_READ</summary>
    GenericRead = 0x80000000,

    /// <summary>GENERIC_WRITE</summary>
    GenericWrite = 0x40000000,

    /// <summary>GENERIC_EXECUTE</summary>
    GenericExecute = 0x20000000,

    /// <summary>GENERIC_ALL</summary>
    GenericAll = 0x10000000
}

[Flags]
enum FileShare : uint
{
    /// <summary>FILE_SHARE_NONE</summary>
    None = 0x00,

    /// <summary>FILE_SHARE_READ</summary>
    Read = 0x01,

    /// <summary>FILE_SHARE_WRITE</summary>
    Write = 0x02,

    /// <summary>FILE_SHARE_DELETE</summary>
    Delete = 0x03
}

[Flags]
enum FileMode : uint
{
    /// <summary>NEW</summary>
    New = 0x01,

    /// <summary>CREATE_ALWAYS</summary>
    CreateAlways = 0x02,

    /// <summary>OPEN_EXISTING</summary>
    OpenExisting = 0x03,

    /// <summary>OPEN_ALWAYS</summary>
    OpenAlways = 0x04,

    /// <summary>TRUNCATE_EXISTING</summary>
    TruncateExisting = 0x05
}

/// <summary>Direction of SCSI transfer</summary>
enum ScsiIoctlDirection : byte
{
    /// <summary>From host to device SCSI_IOCTL_DATA_OUT</summary>
    Out = 0,

    /// <summary>From device to host SCSI_IOCTL_DATA_IN</summary>
    In = 1,

    /// <summary>Unspecified direction, or bidirectional, or no data SCSI_IOCTL_DATA_UNSPECIFIED</summary>
    Unspecified = 2
}

enum WindowsIoctl : uint
{
    IoctlAtaPassThrough       = 0x4D02C,
    IoctlAtaPassThroughDirect = 0x4D030,

    /// <summary>ScsiPassThrough</summary>
    IoctlScsiPassThrough = 0x4D004,

    /// <summary>ScsiPassThroughDirect</summary>
    IoctlScsiPassThroughDirect = 0x4D014,

    /// <summary>ScsiGetAddress</summary>
    IoctlScsiGetAddress = 0x41018,
    IoctlStorageQueryProperty       = 0x2D1400,
    IoctlIdePassThrough             = 0x4D028,
    IoctlStorageGetDeviceNumber     = 0x2D1080,
    IoctlSffdiskQueryDeviceProtocol = 0x71E80,
    IoctlSffdiskDeviceCommand       = 0x79E84
}

[Flags]
enum AtaFlags : ushort
{
    /// <summary>ATA_FLAGS_DRDY_REQUIRED</summary>
    DrdyRequired = 0x01,

    /// <summary>ATA_FLAGS_DATA_IN</summary>
    DataIn = 0x02,

    /// <summary>ATA_FLAGS_DATA_OUT</summary>
    DataOut = 0x04,

    /// <summary>ATA_FLAGS_48BIT_COMMAND</summary>
    ExtendedCommand = 0x08,

    /// <summary>ATA_FLAGS_USE_DMA</summary>
    Dma = 0x10,

    /// <summary>ATA_FLAGS_NO_MULTIPLE</summary>
    NoMultiple = 0x20
}

enum StoragePropertyId
{
    Device           = 0,
    Adapter          = 1,
    Id               = 2,
    UniqueId         = 3,
    WriteCache       = 4,
    Miniport         = 5,
    AccessAlignment  = 6,
    SeekPenalty      = 7,
    Trim             = 8,
    WriteAggregation = 9,
    Telemetry        = 10,
    LbProvisioning   = 11,
    Power            = 12,
    Copyoffload      = 13,
    Resiliency       = 14
}

enum StorageQueryType
{
    Standard = 0,
    Exists   = 1,
    Mask     = 2,
    Max      = 3
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
enum StorageBusType
{
    Unknown           = 0,
    SCSI              = 1,
    ATAPI             = 2,
    ATA               = 3,
    FireWire          = 4,
    SSA               = 5,
    Fibre             = 6,
    USB               = 7,
    RAID              = 8,
    iSCSI             = 9,
    SAS               = 0xA,
    SATA              = 0xB,
    SecureDigital     = 0xC,
    MultiMediaCard    = 0xD,
    Virtual           = 0xE,
    FileBackedVirtual = 0xF,
    Spaces            = 16,
    SCM               = 18,
    UFS               = 19,
    Max               = 20,
    MaxReserved       = 127,
    NVMe              = 0x11
}

[Flags]
enum DeviceGetClassFlags : uint
{
    /// <summary>DIGCF_DEFAULT</summary>
    Default = 0x01,

    /// <summary>DIGCF_PRESENT</summary>
    Present = 0x02,

    /// <summary>DIGCF_ALLCLASSES</summary>
    AllClasses = 0x04,

    /// <summary>DIGCF_PROFILE</summary>
    Profile = 0x08,

    /// <summary>DIGCF_DEVICEINTERFACE</summary>
    DeviceInterface = 0x10
}

enum SdCommandClass : uint
{
    Standard,
    AppCmd
}

enum SdTransferDirection : uint
{
    Unspecified,
    Read,
    Write
}

enum SdTransferType : uint
{
    Unspecified,
    CmdOnly,
    SingleBlock,
    MultiBlock,
    MultiBlockNoCmd12
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
enum SdResponseType : uint
{
    Unspecified,
    None,
    R1,
    R1b,
    R2,
    R3,
    R4,
    R5,
    R5b,
    R6
}

enum SffdiskDcmd : uint
{
    GetVersion,
    LockChannel,
    UnlockChannel,
    DeviceCommand
}

static class Consts
{
    public static Guid GuidSffProtocolSd  = new("AD7536A8-D055-4C40-AA4D-96312DDB6B38");
    public static Guid GuidSffProtocolMmc = new("77274D3F-2365-4491-A030-8BB44AE60097");

    public static Guid GuidDevinterfaceDisk =
        new(0x53F56307, 0xB6BF, 0x11D0, 0x94, 0xF2, 0x00, 0xA0, 0xC9, 0x1E, 0xFB, 0x8B);
}

enum MoveMethod : uint
{
    Begin   = 0,
    Current = 1,
    End     = 2
}