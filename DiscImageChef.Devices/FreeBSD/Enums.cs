// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : FreeBSD direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains enumerations necessary for directly interfacing devices under
//     FreeBSD.
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
namespace DiscImageChef.Devices.FreeBSD
{
    [Flags]
    enum FileFlags
    {
        /// <summary>
        /// O_RDONLY
        ///</summary>
        ReadOnly = 0x00000000,
        /// <summary>
        /// O_WRONLY
        ///</summary>
        WriteOnly = 0x00000001,
        /// <summary>
        /// O_RDWR
        ///</summary>
        ReadWrite = 0x00000002,
        /// <summary>
        /// O_NONBLOCK
        ///</summary>
        NonBlocking = 0x00000004,
        /// <summary>
        /// O_APPEND
        ///</summary>
        Append = 0x00000008,
        /// <summary>
        /// O_SHLOCK
        ///</summary>
        SharedLock = 0x00000010,
        /// <summary>
        /// O_EXLOCK
        ///</summary>
        ExclusiveLock = 0x00000020,
        /// <summary>
        /// O_ASYNC
        ///</summary>
        Async = 0x00000040,
        /// <summary>
        /// O_FSYNC
        ///</summary>
        SyncWrites = 0x00000080,
        /// <summary>
        /// O_NOFOLLOW
        ///</summary>
        NoFollowSymlink = 0x00000100,
        /// <summary>
        /// O_CREAT
        ///</summary>
        OpenOrCreate = 0x00000200,
        /// <summary>
        /// O_TRUNC
        ///</summary>
        Truncate = 0x00000400,
        /// <summary>
        /// O_EXCL
        ///</summary>
        CreateNew = 0x00000800,
        /// <summary>
        /// O_NOCTTY
        ///</summary>
        NoControlTTY = 0x00008000,
        /// <summary>
        /// O_DIRECT
        ///</summary>
        Direct = 0x00010000,
        /// <summary>
        /// O_DIRECTORY
        ///</summary>
        Directory = 0x00020000,
        /// <summary>
        /// O_EXEC
        ///</summary>
        Execute = 0x00040000,
        /// <summary>
        /// O_TTY_INIT
        ///</summary>
        InitializeTTY = 0x00080000,
        /// <summary>
        /// O_CLOEXEC
        ///</summary>
        CloseOnExec = 0x00100000
    }

    [Flags]
    enum CamAtaIoFlags : byte
    {
        /// <summary>
        /// 48-bit command
        ///</summary>
        ExtendedCommand = 0x01,
        /// <summary>
        /// FPDMA command
        ///</summary>
        FPDMA = 0x02,
        /// <summary>
        /// Control, not a command
        ///</summary>
        Control = 0x04,
        /// <summary>
        /// Needs result
        ///</summary>
        NeedResult = 0x08,
        /// <summary>
        /// DMA command
        ///</summary>
        DMA = 0x10
    }

    /// <summary>XPT Opcodes for xpt_action</summary>
    [Flags]
    enum xpt_opcode
    {
        // Function code flags are bits greater than 0xff

        /// <summary>Non-immediate function code</summary>
        XPT_FC_QUEUED = 0x100,
        XPT_FC_USER_CCB = 0x200,
        /// <summary>Only for the transport layer device</summary>
        XPT_FC_XPT_ONLY = 0x400,
        /// <summary>Passes through the device queues</summary>
        XPT_FC_DEV_QUEUED = 0x800 | XPT_FC_QUEUED,

        // Common function commands: 0x00->0x0F

        /// <summary>Execute Nothing</summary>
        XPT_NOOP = 0x00,
        /// <summary>Execute the requested I/O operation</summary>
        XPT_SCSI_IO = 0x01 | XPT_FC_DEV_QUEUED,
        /// <summary>Get type information for specified device</summary>
        XPT_GDEV_TYPE = 0x02,
        /// <summary>Get a list of peripheral devices</summary>
        XPT_GDEVLIST = 0x03,
        /// <summary>Path routing inquiry</summary>
        XPT_PATH_INQ = 0x04,
        /// <summary>Release a frozen device queue</summary>
        XPT_REL_SIMQ = 0x05,
        /// <summary>Set Asynchronous Callback Parameters</summary>
        XPT_SASYNC_CB = 0x06,
        /// <summary>Set device type information</summary>
        XPT_SDEV_TYPE = 0x07,
        /// <summary>(Re)Scan the SCSI Bus</summary>
        XPT_SCAN_BUS = 0x08 | XPT_FC_QUEUED | XPT_FC_USER_CCB
                           | XPT_FC_XPT_ONLY,
        /// <summary>Get EDT entries matching the given pattern</summary>
        XPT_DEV_MATCH = 0x09 | XPT_FC_XPT_ONLY,
        /// <summary>Turn on debugging for a bus, target or lun</summary>
        XPT_DEBUG = 0x0a,
        /// <summary>Path statistics (error counts, etc.)</summary>
        XPT_PATH_STATS = 0x0b,
        /// <summary>Device statistics (error counts, etc.)</summary>
        XPT_GDEV_STATS = 0x0c,
        /// <summary>Get/Set Device advanced information</summary>
        XPT_DEV_ADVINFO = 0x0e,
        /// <summary>Asynchronous event</summary>
        XPT_ASYNC = 0x0f | XPT_FC_QUEUED | XPT_FC_USER_CCB
                           | XPT_FC_XPT_ONLY,

        /// <summary>SCSI Control Functions: 0x10->0x1F</summary>

        /// <summary>Abort the specified CCB</summary>
        XPT_ABORT = 0x10,
        /// <summary>Reset the specified SCSI bus</summary>
        XPT_RESET_BUS = 0x11 | XPT_FC_XPT_ONLY,
        /// <summary>Bus Device Reset the specified SCSI device</summary>
        XPT_RESET_DEV = 0x12 | XPT_FC_DEV_QUEUED,
        /// <summary>Terminate the I/O process</summary>
        XPT_TERM_IO = 0x13,
        /// <summary>Scan Logical Unit</summary>
        XPT_SCAN_LUN = 0x14 | XPT_FC_QUEUED | XPT_FC_USER_CCB
                           | XPT_FC_XPT_ONLY,
        /// <summary>Get default/user transfer settings for the target</summary>
        XPT_GET_TRAN_SETTINGS = 0x15,
        /// <summary>Set transfer rate/width negotiation settings</summary>
        XPT_SET_TRAN_SETTINGS = 0x16,
        /// <summary>Calculate the geometry parameters for a device give the sector size and volume size.</summary>
        XPT_CALC_GEOMETRY = 0x17,
        /// <summary>Execute the requested ATA I/O operation</summary>
        XPT_ATA_IO = 0x18 | XPT_FC_DEV_QUEUED,

        /// <summary>Compat only</summary>
        XPT_GET_SIM_KNOB_OLD = 0x18,

        /// <summary>Set SIM specific knob values.</summary>
        XPT_SET_SIM_KNOB = 0x19,
        /// <summary>Get SIM specific knob values.</summary>
        XPT_GET_SIM_KNOB = 0x1a,
        /// <summary>Serial Management Protocol</summary>
        XPT_SMP_IO = 0x1b | XPT_FC_DEV_QUEUED,
        /// <summary>Scan Target</summary>
        XPT_SCAN_TGT = 0x1E | XPT_FC_QUEUED | XPT_FC_USER_CCB
                           | XPT_FC_XPT_ONLY,

        // HBA engine commands 0x20->0x2F

        /// <summary>HBA engine feature inquiry</summary>
        XPT_ENG_INQ = 0x20 | XPT_FC_XPT_ONLY,
        /// <summary>HBA execute engine request</summary>
        XPT_ENG_EXEC = 0x21 | XPT_FC_DEV_QUEUED,

        // Target mode commands: 0x30->0x3F

        /// <summary>Enable LUN as a target</summary>
        XPT_EN_LUN = 0x30,
        /// <summary>Execute target I/O request</summary>
        XPT_TARGET_IO = 0x31 | XPT_FC_DEV_QUEUED,
        /// <summary>Accept Host Target Mode CDB</summary>
        XPT_ACCEPT_TARGET_IO = 0x32 | XPT_FC_QUEUED | XPT_FC_USER_CCB,
        /// <summary>Continue Host Target I/O Connection</summary>
        XPT_CONT_TARGET_IO = 0x33 | XPT_FC_DEV_QUEUED,
        /// <summary>Notify Host Target driver of event (obsolete)</summary>
        XPT_IMMED_NOTIFY = 0x34 | XPT_FC_QUEUED | XPT_FC_USER_CCB,
        /// <summary>Acknowledgement of event (obsolete)</summary>
        XPT_NOTIFY_ACK = 0x35,
        /// <summary>Notify Host Target driver of event</summary>
        XPT_IMMEDIATE_NOTIFY = 0x36 | XPT_FC_QUEUED | XPT_FC_USER_CCB,
        /// <summary>Acknowledgement of event</summary>
        XPT_NOTIFY_ACKNOWLEDGE = 0x37 | XPT_FC_QUEUED | XPT_FC_USER_CCB,

        /// <summary>Vendor Unique codes: 0x80->0x8F</summary>
        XPT_VUNIQUE = 0x80
    }
}

