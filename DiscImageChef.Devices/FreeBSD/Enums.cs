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
// Copyright © 2011-2018 Natalia Portillo
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
        XPT_SCAN_BUS = 0x08 | XPT_FC_QUEUED | XPT_FC_USER_CCB | XPT_FC_XPT_ONLY,
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
        XPT_ASYNC = 0x0f | XPT_FC_QUEUED | XPT_FC_USER_CCB | XPT_FC_XPT_ONLY,

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
        XPT_SCAN_LUN = 0x14 | XPT_FC_QUEUED | XPT_FC_USER_CCB | XPT_FC_XPT_ONLY,
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
        XPT_SCAN_TGT = 0x1E | XPT_FC_QUEUED | XPT_FC_USER_CCB | XPT_FC_XPT_ONLY,

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

    enum ccb_dev_match_status
    {
        CAM_DEV_MATCH_LAST,
        CAM_DEV_MATCH_MORE,
        CAM_DEV_MATCH_LIST_CHANGED,
        CAM_DEV_MATCH_SIZE_ERROR,
        CAM_DEV_MATCH_ERROR
    }

    enum dev_match_type
    {
        DEV_MATCH_PERIPH = 0,
        DEV_MATCH_DEVICE,
        DEV_MATCH_BUS
    }

    [Flags]
    enum periph_pattern_flags
    {
        PERIPH_MATCH_NONE = 0x000,
        PERIPH_MATCH_PATH = 0x001,
        PERIPH_MATCH_TARGET = 0x002,
        PERIPH_MATCH_LUN = 0x004,
        PERIPH_MATCH_NAME = 0x008,
        PERIPH_MATCH_UNIT = 0x010,
        //  PERIPH_MATCH_ANY = 0x01f
    }

    [Flags]
    enum dev_pattern_flags
    {
        DEV_MATCH_NONE = 0x000,
        DEV_MATCH_PATH = 0x001,
        DEV_MATCH_TARGET = 0x002,
        DEV_MATCH_LUN = 0x004,
        DEV_MATCH_INQUIRY = 0x008,
        DEV_MATCH_DEVID = 0x010,
        //  DEV_MATCH_ANY = 0x00f
    }

    [Flags]
    enum bus_pattern_flags
    {
        BUS_MATCH_NONE = 0x000,
        BUS_MATCH_PATH = 0x001,
        BUS_MATCH_NAME = 0x002,
        BUS_MATCH_UNIT = 0x004,
        BUS_MATCH_BUS_ID = 0x008,
        //  BUS_MATCH_ANY = 0x00f
    }

    [Flags]
    enum dev_result_flags
    {
        DEV_RESULT_NOFLAG = 0x00,
        DEV_RESULT_UNCONFIGURED = 0x01
    }

    enum cam_proto
    {
        PROTO_UNKNOWN,
        PROTO_UNSPECIFIED,

        /// <summary>
        /// Small Computer System Interface
        /// </summary>
        PROTO_SCSI,

        /// <summary>
        /// AT Attachment
        /// </summary>
        PROTO_ATA,

        /// <summary>
        /// AT Attachment Packetized Interface
        /// </summary>
        PROTO_ATAPI,

        /// <summary>
        /// SATA Port Multiplier
        /// </summary>
        PROTO_SATAPM,

        /// <summary>
        /// SATA Enclosure Management Bridge
        /// </summary>
        PROTO_SEMB,

        /// <summary>
        /// NVMe
        /// </summary>
        PROTO_NVME,

        /// <summary>
        /// MMC, SD, SDIO
        /// </summary>
        PROTO_MMCSD,
    }

    [Flags]
    enum mmc_card_features
    {
        CARD_FEATURE_MEMORY = 0x1,
        CARD_FEATURE_SDHC = 0x1 << 1,
        CARD_FEATURE_SDIO = 0x1 << 2,
        CARD_FEATURE_SD20 = 0x1 << 3,
        CARD_FEATURE_MMC = 0x1 << 4,
        CARD_FEATURE_18V = 0x1 << 5,
    }

    enum cam_generations : uint
    {
        CAM_BUS_GENERATION = 0x00,
        CAM_TARGET_GENERATION = 0x01,
        CAM_DEV_GENERATION = 0x02,
        CAM_PERIPH_GENERATION = 0x03,
    }

    [Flags]
    enum dev_pos_type
    {
        CAM_DEV_POS_NONE = 0x000,
        CAM_DEV_POS_BUS = 0x001,
        CAM_DEV_POS_TARGET = 0x002,
        CAM_DEV_POS_DEVICE = 0x004,
        CAM_DEV_POS_PERIPH = 0x008,
        CAM_DEV_POS_PDPTR = 0x010,
        //  CAM_DEV_POS_TYPEMASK = 0xf00,
        CAM_DEV_POS_EDT = 0x100,
        CAM_DEV_POS_PDRV = 0x200
    }

    enum FreebsdIoctl : uint
    {
        CAMIOCOMMAND = 0xC4D81802,
    }

    [Flags]
    enum ccb_flags : uint
    {
        /// <summary>
        /// The CDB field is a pointer
        /// </summary>
        CAM_CDB_POINTER = 0x00000001,
        /// <summary>
        /// SIM queue actions are enabled
        /// </summary>
        CAM_QUEUE_ENABLE = 0x00000002,
        /// <summary>
        /// CCB contains a linked CDB
        /// </summary>
        CAM_CDB_LINKED = 0x00000004,
        /// <summary>
        /// Perform transport negotiation with this command.
        /// </summary>
        CAM_NEGOTIATE = 0x00000008,
        /// <summary>
        /// Data type with physical addrs
        /// </summary>
        CAM_DATA_ISPHYS = 0x00000010,
        /// <summary>
        /// Disable autosense feature
        /// </summary>
        CAM_DIS_AUTOSENSE = 0x00000020,
        /// <summary>
        /// Data direction (00:IN/OUT)
        /// </summary>
        CAM_DIR_BOTH = 0x00000000,
        /// <summary>
        /// Data direction (01:DATA IN)
        /// </summary>
        CAM_DIR_IN = 0x00000040,
        /// <summary>
        /// Data direction (10:DATA OUT)
        /// </summary>
        CAM_DIR_OUT = 0x00000080,
        /// <summary>
        /// Data direction (11:no data)
        /// </summary>
        CAM_DIR_NONE = 0x000000C0,
        /// <summary>
        /// Data type (000:Virtual)
        /// </summary>
        CAM_DATA_VADDR = 0x00000000,
        /// <summary>
        /// Data type (001:Physical)
        /// </summary>
        CAM_DATA_PADDR = 0x00000010,
        /// <summary>
        /// Data type (010:sglist)
        /// </summary>
        CAM_DATA_SG = 0x00040000,
        /// <summary>
        /// Data type (011:sglist phys)
        /// </summary>
        CAM_DATA_SG_PADDR = 0x00040010,
        /// <summary>
        /// Data type (100:bio)
        /// </summary>
        CAM_DATA_BIO = 0x00200000,
        /// <summary>
        /// Use Soft reset alternative
        /// </summary>
        CAM_SOFT_RST_OP = 0x00000100,
        /// <summary>
        /// Flush resid bytes on complete
        /// </summary>
        CAM_ENG_SYNC = 0x00000200,
        /// <summary>
        /// Disable DEV Q freezing
        /// </summary>
        CAM_DEV_QFRZDIS = 0x00000400,
        /// <summary>
        /// Freeze DEV Q on execution
        /// </summary>
        CAM_DEV_QFREEZE = 0x00000800,
        /// <summary>
        /// Command takes a lot of power
        /// </summary>
        CAM_HIGH_POWER = 0x00001000,
        /// <summary>
        /// Sense data is a pointer
        /// </summary>
        CAM_SENSE_PTR = 0x00002000,
        /// <summary>
        /// Sense pointer is physical addr
        /// </summary>
        CAM_SENSE_PHYS = 0x00004000,
        /// <summary>
        /// Use the tag action in this ccb
        /// </summary>
        CAM_TAG_ACTION_VALID = 0x00008000,
        /// <summary>
        /// Pass driver does err. recovery
        /// </summary>
        CAM_PASS_ERR_RECOVER = 0x00010000,
        /// <summary>
        /// Disable disconnect
        /// </summary>
        CAM_DIS_DISCONNECT = 0x00020000,
        /// <summary>
        /// Message buffer ptr is physical
        /// </summary>
        CAM_MSG_BUF_PHYS = 0x00080000,
        /// <summary>
        /// Autosense data ptr is physical
        /// </summary>
        CAM_SNS_BUF_PHYS = 0x00100000,
        /// <summary>
        /// CDB poiner is physical
        /// </summary>
        CAM_CDB_PHYS = 0x00400000,
        /// <summary>
        /// SG list is for the HBA engine
        /// </summary>
        CAM_ENG_SGLIST = 0x00800000,

        /* Phase cognizant mode flags */
        /// <summary>
        /// Disable autosave/restore ptrs
        /// </summary>
        CAM_DIS_AUTOSRP = 0x01000000,
        /// <summary>
        /// Disable auto disconnect
        /// </summary>
        CAM_DIS_AUTODISC = 0x02000000,
        /// <summary>
        /// Target CCB available
        /// </summary>
        CAM_TGT_CCB_AVAIL = 0x04000000,
        /// <summary>
        /// The SIM runs in phase mode
        /// </summary>
        CAM_TGT_PHASE_MODE = 0x08000000,
        /// <summary>
        /// Message buffer valid
        /// </summary>
        CAM_MSGB_VALID = 0x10000000,
        /// <summary>
        /// Status buffer valid
        /// </summary>
        CAM_STATUS_VALID = 0x20000000,
        /// <summary>
        /// Data buffer valid
        /// </summary>
        CAM_DATAB_VALID = 0x40000000,
        /* Host target Mode flags */
        /// <summary>
        /// Send sense data with status
        /// </summary>
        CAM_SEND_SENSE = 0x08000000,
        /// <summary>
        /// Terminate I/O Message sup.
        /// </summary>
        CAM_TERM_IO = 0x10000000,
        /// <summary>
        /// Disconnects are mandatory
        /// </summary>
        CAM_DISCONNECT = 0x20000000,
        /// <summary>
        /// Send status after data phase
        /// </summary>
        CAM_SEND_STATUS = 0x40000000,
        /// <summary>
        /// Call callback without lock.
        /// </summary>
        CAM_UNLOCKED = 0x80000000
    }

    enum cam_status : uint
    {
        /// <summary>CCB request is in progress</summary>
        CAM_REQ_INPROG = 0x00,

        /// <summary>CCB request completed without error</summary>
        CAM_REQ_CMP = 0x01,

        /// <summary>CCB request aborted by the host</summary>
        CAM_REQ_ABORTED = 0x02,

        /// <summary>Unable to abort CCB request</summary>
        CAM_UA_ABORT = 0x03,

        /// <summary>CCB request completed with an error</summary>
        CAM_REQ_CMP_ERR = 0x04,

        /// <summary>CAM subsystem is busy</summary>
        CAM_BUSY = 0x05,

        /// <summary>CCB request was invalid</summary>
        CAM_REQ_INVALID = 0x06,

        /// <summary>Supplied Path ID is invalid</summary>
        CAM_PATH_INVALID = 0x07,

        /// <summary>SCSI Device Not Installed/there</summary>
        CAM_DEV_NOT_THERE = 0x08,

        /// <summary>Unable to terminate I/O CCB request</summary>
        CAM_UA_TERMIO = 0x09,

        /// <summary>Target Selection Timeout</summary>
        CAM_SEL_TIMEOUT = 0x0a,

        /// <summary>Command timeout</summary>
        CAM_CMD_TIMEOUT = 0x0b,

        /// <summary>SCSI error, look at error code in CCB</summary>
        CAM_SCSI_STATUS_ERROR = 0x0c,

        /// <summary>Message Reject Received</summary>
        CAM_MSG_REJECT_REC = 0x0d,

        /// <summary>SCSI Bus Reset Sent/Received</summary>
        CAM_SCSI_BUS_RESET = 0x0e,

        /// <summary>Uncorrectable parity error occurred</summary>
        CAM_UNCOR_PARITY = 0x0f,

        /// <summary>Autosense: request sense cmd fail</summary>
        CAM_AUTOSENSE_FAIL = 0x10,

        /// <summary>No HBA Detected error</summary>
        CAM_NO_HBA = 0x11,

        /// <summary>Data Overrun error</summary>
        CAM_DATA_RUN_ERR = 0x12,

        /// <summary>Unexpected Bus Free</summary>
        CAM_UNEXP_BUSFREE = 0x13,

        /// <summary>Target Bus Phase Sequence Failure</summary>
        CAM_SEQUENCE_FAIL = 0x14,

        /// <summary>CCB length supplied is inadequate</summary>
        CAM_CCB_LEN_ERR = 0x15,

        /// <summary>Unable to provide requested capability</summary>
        CAM_PROVIDE_FAIL = 0x16,

        /// <summary>A SCSI BDR msg was sent to target</summary>
        CAM_BDR_SENT = 0x17,

        /// <summary>CCB request terminated by the host</summary>
        CAM_REQ_TERMIO = 0x18,

        /// <summary>Unrecoverable Host Bus Adapter Error</summary>
        CAM_UNREC_HBA_ERROR = 0x19,

        /// <summary>Request was too large for this host</summary>
        CAM_REQ_TOO_BIG = 0x1a,

        /// <summary>This request should be requeued to preserve transaction ordering. This typically occurs when the SIM recognizes an error that should freeze the queue and must place additional requests for the target at the sim level back into the XPT queue.</summary>
        CAM_REQUEUE_REQ = 0x1b,

        /// <summary>ATA error, look at error code in CCB</summary>
        CAM_ATA_STATUS_ERROR = 0x1c,

        /// <summary>Initiator/Target Nexus lost.</summary>
        CAM_SCSI_IT_NEXUS_LOST = 0x1d,

        /// <summary>SMP error, look at error code in CCB</summary>
        CAM_SMP_STATUS_ERROR = 0x1e,

        /// <summary>Command completed without error but  exceeded the soft timeout threshold.</summary>
        CAM_REQ_SOFTTIMEOUT = 0x1f,

        /*
         * 0x20 - 0x32 are unassigned
         */

        /// <summary>Initiator Detected Error</summary>
        CAM_IDE = 0x33,

        /// <summary>Resource Unavailable</summary>
        CAM_RESRC_UNAVAIL = 0x34,

        /// <summary>Unacknowledged Event by Host</summary>
        CAM_UNACKED_EVENT = 0x35,

        /// <summary>Message Received in Host Target Mode</summary>
        CAM_MESSAGE_RECV = 0x36,

        /// <summary>Invalid CDB received in Host Target Mode</summary>
        CAM_INVALID_CDB = 0x37,

        /// <summary>Lun supplied is invalid</summary>
        CAM_LUN_INVALID = 0x38,

        /// <summary>Target ID supplied is invalid</summary>
        CAM_TID_INVALID = 0x39,

        /// <summary>The requested function is not available</summary>
        CAM_FUNC_NOTAVAIL = 0x3a,

        /// <summary>Nexus is not established</summary>
        CAM_NO_NEXUS = 0x3b,

        /// <summary>The initiator ID is invalid</summary>
        CAM_IID_INVALID = 0x3c,

        /// <summary>The SCSI CDB has been received</summary>
        CAM_CDB_RECVD = 0x3d,

        /// <summary>The LUN is already enabled for target mode</summary>
        CAM_LUN_ALRDY_ENA = 0x3e,

        /// <summary>SCSI Bus Busy</summary>
        CAM_SCSI_BUSY = 0x3f,

        /*
         * Flags
         */

        /// <summary>The DEV queue is frozen w/this err</summary>
        CAM_DEV_QFRZN = 0x40,

        /// <summary>Autosense data valid for target</summary>
        CAM_AUTOSNS_VALID = 0x80,

        /// <summary>SIM ready to take more commands</summary>
        CAM_RELEASE_SIMQ = 0x100,

        /// <summary>SIM has this command in its queue</summary>
        CAM_SIM_QUEUED = 0x200,

        /// <summary>Quality of service data is valid</summary>
        CAM_QOS_VALID = 0x400,

        /// <summary>Mask bits for just the status #</summary>
        CAM_STATUS_MASK = 0x3F,

        /*
         * Target Specific Adjunct Status
         */

        /// <summary>sent sense with status</summary>
        CAM_SENT_SENSE = 0x40000000
    }
}