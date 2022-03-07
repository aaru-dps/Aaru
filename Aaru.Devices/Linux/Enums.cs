// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Linux direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains enumerations necessary for directly interfacing devices under
//     Linux.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Devices.Linux;

using System;

[Flags]
enum FileFlags
{
    /// <summary>O_RDONLY</summary>
    Readonly = 0x0,
    /// <summary>O_WRONLY</summary>
    Writeonly = 0x1,
    /// <summary>O_RDWR</summary>
    ReadWrite = 0x2,
    /// <summary>O_CREAT</summary>
    OpenOrCreate = 0x40,
    /// <summary>O_EXCL</summary>
    CreateNew = 0x80,
    /// <summary>O_NOCTTY</summary>
    NoControlTty = 0x100,
    /// <summary>O_TRUNC</summary>
    Truncate = 0x200,
    /// <summary>O_APPEND</summary>
    Append = 0x400,
    /// <summary>O_NONBLOCK</summary>
    NonBlocking = 0x800,
    /// <summary>O_DSYNC</summary>
    Synchronous = 0x1000,
    /// <summary>O_ASYNC</summary>
    Async = 0x2000,
    /// <summary>O_DIRECT</summary>
    Direct = 0x4000,
    /// <summary>O_LARGEFILE</summary>
    LargeFile = 0x8000,
    /// <summary>O_DIRECTORY</summary>
    Directory = 0x10000,
    /// <summary>O_NOFOLLOW</summary>
    NoFollowSymlink = 0x20000,
    /// <summary>O_NOATIME</summary>
    NoAccessTime = 0x40000,
    /// <summary>O_CLOEXEC</summary>
    CloseOnExec = 0x80000
}

/// <summary>Direction of SCSI transfer</summary>
enum ScsiIoctlDirection
{
    /// <summary>No data transfer happens SG_DXFER_NONE</summary>
    None = -1,
    /// <summary>From host to device SG_DXFER_TO_DEV</summary>
    Out = -2,
    /// <summary>From device to host SG_DXFER_FROM_DEV</summary>
    In = -3,
    /// <summary>Bidirectional device/host SG_DXFER_TO_FROM_DEV</summary>
    Unspecified = -4,
    /// <summary>Unspecified SG_DXFER_UNKNOWN</summary>
    Unknown = -5
}

enum LinuxIoctl : uint
{
    // SCSI IOCtls
    SgGetVersionNum = 0x2282,
    SgIo            = 0x2285,

    // MMC IOCtl
    MmcIocCmd      = 0xC048B300,
    MmcIocMultiCmd = 0xC008B301
}

[Flags]
enum SgInfo : uint
{
    /// <summary>Mask to check OK</summary>
    OkMask = 0x01,
    /// <summary>No sense or driver noise</summary>
    Ok = 0x00,
    /// <summary>Check Condition</summary>
    CheckCondition = 0x01,

    /// <summary>Direct I/O mask</summary>
    DirectIoMask = 0x06,
    /// <summary>Transfer via kernel buffers (or no transfer)</summary>
    IndirectIo = 0x00,
    /// <summary>Direct I/O performed</summary>
    DirectIo = 0x02,
    /// <summary>Partial direct and partial indirect I/O</summary>
    MixedIo = 0x04
}

[Flags]
enum SgFlags : uint
{
    DirectIo         = 1,
    UnusedLunInhibit = 2,
    MmapIo           = 4,
    NoDxfer          = 0x10000,
    QAtTail          = 0x10,
    QAtHead          = 0x20
}

enum SeekWhence
{
    Begin   = 0,
    Current = 1,
    End     = 2,
    Data    = 3,
    Hole    = 4
}