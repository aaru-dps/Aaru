// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;

namespace DiscImageChef.Devices.Windows
{
    [Flags]
    enum FileAttributes : uint
    {
        /// <summary>
        /// FILE_ATTRIBUTE_ARCHIVE
        /// </summary>
        Archive = 0x20,
        /// <summary>
        /// FILE_ATTRIBUTE_COMPRESSED
        /// </summary>
        Compressed = 0x800,
        /// <summary>
        /// FILE_ATTRIBUTE_DEVICE
        /// </summary>
        Device = 0x40,
        /// <summary>
        /// FILE_ATTRIBUTE_DIRECTORY
        /// </summary>
        Directory = 0x10,
        /// <summary>
        /// FILE_ATTRIBUTE_ENCRYPTED
        /// </summary>
        Encrypted = 0x4000,
        /// <summary>
        /// FILE_ATTRIBUTE_HIDDEN
        /// </summary>
        Hidden = 0x02,
        /// <summary>
        /// FILE_ATTRIBUTE_INTEGRITY_STREAM
        /// </summary>
        IntegrityStream = 0x8000,
        /// <summary>
        /// FILE_ATTRIBUTE_NORMAL
        /// </summary>
        Normal = 0x80,
        /// <summary>
        /// FILE_ATTRIBUTE_NOT_CONTENT_INDEXED
        /// </summary>
        NotContentIndexed = 0x2000,
        /// <summary>
        /// FILE_ATTRIBUTE_NO_SCRUB_DATA
        /// </summary>
        NoScrubData = 0x20000,
        /// <summary>
        /// FILE_ATTRIBUTE_OFFLINE
        /// </summary>
        Offline = 0x1000,
        /// <summary>
        /// FILE_ATTRIBUTE_READONLY
        /// </summary>
        Readonly = 0x01,
        /// <summary>
        /// FILE_ATTRIBUTE_REPARSE_POINT
        /// </summary>
        ReparsePoint = 0x400,
        /// <summary>
        /// FILE_ATTRIBUTE_SPARSE_FILE
        /// </summary>
        SparseFile = 0x200,
        /// <summary>
        /// FILE_ATTRIBUTE_SYSTEM
        /// </summary>
        System = 0x04,
        /// <summary>
        /// FILE_ATTRIBUTE_TEMPORARY
        /// </summary>
        Temporary = 0x100,
        /// <summary>
        /// FILE_ATTRIBUTE_VIRTUAL
        /// </summary>
        Virtual = 0x10000
    }

    [Flags]
    enum FileAccess : uint
    {
        /// <summary>
        /// FILE_READ_DATA
        /// </summary>
        ReadData = 0x0001,
        /// <summary>
        /// FILE_LIST_DIRECTORY
        /// </summary>
        ListDirectory = ReadData,
        /// <summary>
        /// FILE_WRITE_DATA
        /// </summary>
        WriteData = 0x0002,
        /// <summary>
        /// FILE_ADD_FILE
        /// </summary>
        AddFile = WriteData,
        /// <summary>
        /// FILE_APPEND_DATA
        /// </summary>
        AppendData = 0x0004,
        /// <summary>
        /// FILE_ADD_SUBDIRECTORY
        /// </summary>
        AddSubdirectory = AppendData,
        /// <summary>
        /// FILE_CREATE_PIPE_INSTANCE
        /// </summary>
        CreatePipeInstance = AppendData,
        /// <summary>
        /// FILE_READ_EA
        /// </summary>
        ReadEA = 0x0008,
        /// <summary>
        /// FILE_WRITE_EA
        /// </summary>
        WriteEA = 0x0010,
        /// <summary>
        /// FILE_EXECUTE
        /// </summary>
        Execute = 0x0020,
        /// <summary>
        /// FILE_TRAVERSE
        /// </summary>
        Traverse = Execute,
        /// <summary>
        /// FILE_DELETE_CHILD
        /// </summary>
        DeleteChild = 0x0040,
        /// <summary>
        /// FILE_READ_ATTRIBUTES
        /// </summary>
        ReadAttributes = 0x0080,
        /// <summary>
        /// FILE_WRITE_ATTRIBUTES
        /// </summary>
        WriteAttributes = 0x0100,
        /// <summary>
        /// GENERIC_READ
        /// </summary>
        GenericRead = 0x80000000,
        /// <summary>
        /// GENERIC_WRITE
        /// </summary>
        GenericWrite = 0x40000000,
        /// <summary>
        /// GENERIC_EXECUTE
        /// </summary>
        GenericExecute = 0x20000000,
        /// <summary>
        /// GENERIC_ALL
        /// </summary>
        GenericAll = 0x10000000
    }

    [Flags]
    enum FileShare : uint
    {
        /// <summary>
        /// FILE_SHARE_NONE
        /// </summary>
        None = 0x00,
        /// <summary>
        /// FILE_SHARE_READ
        /// </summary>
        Read = 0x01,
        /// <summary>
        /// FILE_SHARE_WRITE
        /// </summary>
        Write = 0x02,
        /// <summary>
        /// FILE_SHARE_DELETE
        /// </summary>
        Delete = 0x03
    }

    [Flags]
    enum FileMode : uint
    {
        /// <summary>
        /// NEW
        /// </summary>
        New = 0x01,
        /// <summary>
        /// CREATE_ALWAYS
        /// </summary>
        CreateAlways = 0x02,
        /// <summary>
        /// OPEN_EXISTING
        /// </summary>
        OpenExisting = 0x03,
        /// <summary>
        /// OPEN_ALWAYS
        /// </summary>
        OpenAlways = 0x04,
        /// <summary>
        /// TRUNCATE_EXISTING
        /// </summary>
        TruncateExisting = 0x05
    }

    /// <summary>
    /// Direction of SCSI transfer
    /// </summary>
    enum ScsiIoctlDirection : byte
    {
        /// <summary>
        /// From host to device
        /// SCSI_IOCTL_DATA_OUT
        /// </summary>
        Out = 0,
        /// <summary>
        /// From device to host
        /// SCSI_IOCTL_DATA_IN
        /// </summary>
        In = 1,
        /// <summary>
        /// Unspecified direction, or bidirectional, or no data
        /// SCSI_IOCTL_DATA_UNSPECIFIED
        /// </summary>
        Unspecified = 2
    }

    enum WindowsIoctl : uint
    {
        IOCTL_ATA_PASS_THROUGH = 0x4D02C,
        IOCTL_ATA_PASS_THROUGH_DIRECT = 0x4D030,
        /// <summary>
        /// ScsiPassThrough
        /// </summary>
        IOCTL_SCSI_PASS_THROUGH = 0x4D004,
        /// <summary>
        /// ScsiPassThroughDirect
        /// </summary>
        IOCTL_SCSI_PASS_THROUGH_DIRECT = 0x4D014,
        /// <summary>
        /// ScsiGetAddress
        /// </summary>
        IOCTL_SCSI_GET_ADDRESS = 0x41018
    }

    [Flags]
    enum AtaFlags : ushort
    {
        /// <summary>
        /// ATA_FLAGS_DRDY_REQUIRED
        /// </summary>
        DrdyRequired = 0x01,
        /// <summary>
        /// ATA_FLAGS_DATA_IN
        /// </summary>
        DataIn = 0x02,
        /// <summary>
        /// ATA_FLAGS_DATA_OUT
        /// </summary>
        DataOut = 0x04,
        /// <summary>
        /// ATA_FLAGS_48BIT_COMMAND
        /// </summary>
        ExtendedCommand = 0x08,
        /// <summary>
        /// ATA_FLAGS_USE_DMA
        /// </summary>
        DMA = 0x10,
        /// <summary>
        /// ATA_FLAGS_NO_MULTIPLE
        /// </summary>
        NoMultiple = 0x20
    }
}
