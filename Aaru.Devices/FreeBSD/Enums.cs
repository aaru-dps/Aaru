// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;

namespace Aaru.Devices.FreeBSD
{
    [Flags, Obsolete]
    internal enum FileFlags
    {
        /// <summary>O_RDONLY</summary>
        ReadOnly = 0x00000000,
        /// <summary>O_WRONLY</summary>
        WriteOnly = 0x00000001,
        /// <summary>O_RDWR</summary>
        ReadWrite = 0x00000002,
        /// <summary>O_NONBLOCK</summary>
        NonBlocking = 0x00000004,
        /// <summary>O_APPEND</summary>
        Append = 0x00000008,
        /// <summary>O_SHLOCK</summary>
        SharedLock = 0x00000010,
        /// <summary>O_EXLOCK</summary>
        ExclusiveLock = 0x00000020,
        /// <summary>O_ASYNC</summary>
        Async = 0x00000040,
        /// <summary>O_FSYNC</summary>
        SyncWrites = 0x00000080,
        /// <summary>O_NOFOLLOW</summary>
        NoFollowSymlink = 0x00000100,
        /// <summary>O_CREAT</summary>
        OpenOrCreate = 0x00000200,
        /// <summary>O_TRUNC</summary>
        Truncate = 0x00000400,
        /// <summary>O_EXCL</summary>
        CreateNew = 0x00000800,
        /// <summary>O_NOCTTY</summary>
        NoControlTty = 0x00008000,
        /// <summary>O_DIRECT</summary>
        Direct = 0x00010000,
        /// <summary>O_DIRECTORY</summary>
        Directory = 0x00020000,
        /// <summary>O_EXEC</summary>
        Execute = 0x00040000,
        /// <summary>O_TTY_INIT</summary>
        InitializeTty = 0x00080000,
        /// <summary>O_CLOEXEC</summary>
        CloseOnExec = 0x00100000
    }

    [Flags, Obsolete]
    internal enum CamAtaIoFlags : byte
    {
        /// <summary>48-bit command</summary>
        ExtendedCommand = 0x01,
        /// <summary>FPDMA command</summary>
        Fpdma = 0x02,
        /// <summary>Control, not a command</summary>
        Control = 0x04,
        /// <summary>Needs result</summary>
        NeedResult = 0x08,
        /// <summary>DMA command</summary>
        Dma = 0x10
    }

    /// <summary>XPT Opcodes for xpt_action</summary>
    [Flags, Obsolete]
    internal enum XptOpcode
    {
        // Function code flags are bits greater than 0xff

        /// <summary>Non-immediate function code</summary>
        XptFcQueued = 0x100, XptFcUserCcb = 0x200,
        /// <summary>Only for the transport layer device</summary>
        XptFcXptOnly = 0x400,
        /// <summary>Passes through the device queues</summary>
        XptFcDevQueued = 0x800 | XptFcQueued,

        // Common function commands: 0x00->0x0F

        /// <summary>Execute Nothing</summary>
        XptNoop = 0x00,
        /// <summary>Execute the requested I/O operation</summary>
        XptScsiIo = 0x01 | XptFcDevQueued,
        /// <summary>Get type information for specified device</summary>
        XptGdevType = 0x02,
        /// <summary>Get a list of peripheral devices</summary>
        XptGdevlist = 0x03,
        /// <summary>Path routing inquiry</summary>
        XptPathInq = 0x04,
        /// <summary>Release a frozen device queue</summary>
        XptRelSimq = 0x05,
        /// <summary>Set Asynchronous Callback Parameters</summary>
        XptSasyncCb = 0x06,
        /// <summary>Set device type information</summary>
        XptSdevType = 0x07,
        /// <summary>(Re)Scan the SCSI Bus</summary>
        XptScanBus = 0x08 | XptFcQueued | XptFcUserCcb | XptFcXptOnly,
        /// <summary>Get EDT entries matching the given pattern</summary>
        XptDevMatch = 0x09 | XptFcXptOnly,
        /// <summary>Turn on debugging for a bus, target or lun</summary>
        XptDebug = 0x0a,
        /// <summary>Path statistics (error counts, etc.)</summary>
        XptPathStats = 0x0b,
        /// <summary>Device statistics (error counts, etc.)</summary>
        XptGdevStats = 0x0c,
        /// <summary>Get/Set Device advanced information</summary>
        XptDevAdvinfo = 0x0e,
        /// <summary>Asynchronous event</summary>
        XptAsync = 0x0f | XptFcQueued | XptFcUserCcb | XptFcXptOnly,

        /// <summary>SCSI Control Functions: 0x10->0x1F</summary>
        /// <summary>Abort the specified CCB</summary>
        XptAbort = 0x10,
        /// <summary>Reset the specified SCSI bus</summary>
        XptResetBus = 0x11 | XptFcXptOnly,
        /// <summary>Bus Device Reset the specified SCSI device</summary>
        XptResetDev = 0x12 | XptFcDevQueued,
        /// <summary>Terminate the I/O process</summary>
        XptTermIo = 0x13,
        /// <summary>Scan Logical Unit</summary>
        XptScanLun = 0x14 | XptFcQueued | XptFcUserCcb | XptFcXptOnly,
        /// <summary>Get default/user transfer settings for the target</summary>
        XptGetTranSettings = 0x15,
        /// <summary>Set transfer rate/width negotiation settings</summary>
        XptSetTranSettings = 0x16,
        /// <summary>Calculate the geometry parameters for a device give the sector size and volume size.</summary>
        XptCalcGeometry = 0x17,
        /// <summary>Execute the requested ATA I/O operation</summary>
        XptAtaIo = 0x18 | XptFcDevQueued,

        /// <summary>Compat only</summary>
        XptGetSimKnobOld = 0x18,

        /// <summary>Set SIM specific knob values.</summary>
        XptSetSimKnob = 0x19,
        /// <summary>Get SIM specific knob values.</summary>
        XptGetSimKnob = 0x1a,
        /// <summary>Serial Management Protocol</summary>
        XptSmpIo = 0x1b | XptFcDevQueued,
        /// <summary>Scan Target</summary>
        XptScanTgt = 0x1E | XptFcQueued | XptFcUserCcb | XptFcXptOnly,

        // HBA engine commands 0x20->0x2F

        /// <summary>HBA engine feature inquiry</summary>
        XptEngInq = 0x20 | XptFcXptOnly,
        /// <summary>HBA execute engine request</summary>
        XptEngExec = 0x21 | XptFcDevQueued,

        // Target mode commands: 0x30->0x3F

        /// <summary>Enable LUN as a target</summary>
        XptEnLun = 0x30,
        /// <summary>Execute target I/O request</summary>
        XptTargetIo = 0x31 | XptFcDevQueued,
        /// <summary>Accept Host Target Mode CDB</summary>
        XptAcceptTargetIo = 0x32 | XptFcQueued | XptFcUserCcb,
        /// <summary>Continue Host Target I/O Connection</summary>
        XptContTargetIo = 0x33 | XptFcDevQueued,
        /// <summary>Notify Host Target driver of event (obsolete)</summary>
        XptImmedNotify = 0x34 | XptFcQueued | XptFcUserCcb,
        /// <summary>Acknowledgement of event (obsolete)</summary>
        XptNotifyAck = 0x35,
        /// <summary>Notify Host Target driver of event</summary>
        XptImmediateNotify = 0x36 | XptFcQueued | XptFcUserCcb,
        /// <summary>Acknowledgement of event</summary>
        XptNotifyAcknowledge = 0x37 | XptFcQueued | XptFcUserCcb,

        /// <summary>Vendor Unique codes: 0x80->0x8F</summary>
        XptVunique = 0x80
    }

    [Obsolete]
    internal enum CcbDevMatchStatus
    {
        CamDevMatchLast, CamDevMatchMore, CamDevMatchListChanged,
        CamDevMatchSizeError, CamDevMatchError
    }

    [Obsolete]
    internal enum DevMatchType
    {
        DevMatchPeriph = 0, DevMatchDevice, DevMatchBus
    }

    [Flags, Obsolete]
    internal enum PeriphPatternFlags
    {
        PeriphMatchNone = 0x000, PeriphMatchPath = 0x001, PeriphMatchTarget = 0x002,
        PeriphMatchLun  = 0x004, PeriphMatchName = 0x008, PeriphMatchUnit   = 0x010

        //  PERIPH_MATCH_ANY = 0x01f
    }

    [Flags, Obsolete]
    internal enum DevPatternFlags
    {
        DevMatchNone = 0x000, DevMatchPath    = 0x001, DevMatchTarget = 0x002,
        DevMatchLun  = 0x004, DevMatchInquiry = 0x008, DevMatchDevid  = 0x010

        //  DEV_MATCH_ANY = 0x00f
    }

    [Flags, Obsolete]
    internal enum BusPatternFlags
    {
        BusMatchNone = 0x000, BusMatchPath  = 0x001, BusMatchName = 0x002,
        BusMatchUnit = 0x004, BusMatchBusId = 0x008

        //  BUS_MATCH_ANY = 0x00f
    }

    [Flags, Obsolete]
    internal enum DevResultFlags
    {
        DevResultNoflag = 0x00, DevResultUnconfigured = 0x01
    }

    [Obsolete]
    internal enum CamProto
    {
        ProtoUnknown, ProtoUnspecified,

        /// <summary>Small Computer System Interface</summary>
        ProtoScsi,

        /// <summary>AT Attachment</summary>
        ProtoAta,

        /// <summary>AT Attachment Packetized Interface</summary>
        ProtoAtapi,

        /// <summary>SATA Port Multiplier</summary>
        ProtoSatapm,

        /// <summary>SATA Enclosure Management Bridge</summary>
        ProtoSemb,

        /// <summary>NVMe</summary>
        ProtoNvme,

        /// <summary>MMC, SD, SDIO</summary>
        ProtoMmcsd
    }

    [Flags, Obsolete]
    internal enum MmcCardFeatures
    {
        CardFeatureMemory = 0x1, CardFeatureSdhc = 0x1 << 1, CardFeatureSdio = 0x1 << 2,
        CardFeatureSd20   = 0x1                        << 3, CardFeatureMmc  = 0x1 << 4, CardFeature18V = 0x1 << 5
    }

    [Obsolete]
    internal enum CamGenerations : uint
    {
        CamBusGeneration    = 0x00, CamTargetGeneration = 0x01, CamDevGeneration = 0x02,
        CamPeriphGeneration = 0x03
    }

    [Flags, Obsolete]
    internal enum DevPosType
    {
        CamDevPosNone   = 0x000, CamDevPosBus    = 0x001, CamDevPosTarget = 0x002,
        CamDevPosDevice = 0x004, CamDevPosPeriph = 0x008, CamDevPosPdptr  = 0x010,

        //  CAM_DEV_POS_TYPEMASK = 0xf00,
        CamDevPosEdt = 0x100, CamDevPosPdrv = 0x200
    }

    [Obsolete]
    internal enum FreebsdIoctl : uint
    {
        Camiocommand = 0xC4D81802
    }

    [Flags, Obsolete]
    internal enum CcbFlags : uint
    {
        /// <summary>The CDB field is a pointer</summary>
        CamCdbPointer = 0x00000001,
        /// <summary>SIM queue actions are enabled</summary>
        CamQueueEnable = 0x00000002,
        /// <summary>CCB contains a linked CDB</summary>
        CamCdbLinked = 0x00000004,
        /// <summary>Perform transport negotiation with this command.</summary>
        CamNegotiate = 0x00000008,
        /// <summary>Data type with physical addrs</summary>
        CamDataIsphys = 0x00000010,
        /// <summary>Disable autosense feature</summary>
        CamDisAutosense = 0x00000020,
        /// <summary>Data direction (00:IN/OUT)</summary>
        CamDirBoth = 0x00000000,
        /// <summary>Data direction (01:DATA IN)</summary>
        CamDirIn = 0x00000040,
        /// <summary>Data direction (10:DATA OUT)</summary>
        CamDirOut = 0x00000080,
        /// <summary>Data direction (11:no data)</summary>
        CamDirNone = 0x000000C0,
        /// <summary>Data type (000:Virtual)</summary>
        CamDataVaddr = 0x00000000,
        /// <summary>Data type (001:Physical)</summary>
        CamDataPaddr = 0x00000010,
        /// <summary>Data type (010:sglist)</summary>
        CamDataSg = 0x00040000,
        /// <summary>Data type (011:sglist phys)</summary>
        CamDataSgPaddr = 0x00040010,
        /// <summary>Data type (100:bio)</summary>
        CamDataBio = 0x00200000,
        /// <summary>Use Soft reset alternative</summary>
        CamSoftRstOp = 0x00000100,
        /// <summary>Flush resid bytes on complete</summary>
        CamEngSync = 0x00000200,
        /// <summary>Disable DEV Q freezing</summary>
        CamDevQfrzdis = 0x00000400,
        /// <summary>Freeze DEV Q on execution</summary>
        CamDevQfreeze = 0x00000800,
        /// <summary>Command takes a lot of power</summary>
        CamHighPower = 0x00001000,
        /// <summary>Sense data is a pointer</summary>
        CamSensePtr = 0x00002000,
        /// <summary>Sense pointer is physical addr</summary>
        CamSensePhys = 0x00004000,
        /// <summary>Use the tag action in this ccb</summary>
        CamTagActionValid = 0x00008000,
        /// <summary>Pass driver does err. recovery</summary>
        CamPassErrRecover = 0x00010000,
        /// <summary>Disable disconnect</summary>
        CamDisDisconnect = 0x00020000,
        /// <summary>Message buffer ptr is physical</summary>
        CamMsgBufPhys = 0x00080000,
        /// <summary>Autosense data ptr is physical</summary>
        CamSnsBufPhys = 0x00100000,
        /// <summary>CDB poiner is physical</summary>
        CamCdbPhys = 0x00400000,
        /// <summary>SG list is for the HBA engine</summary>
        CamEngSglist = 0x00800000,

        /* Phase cognizant mode flags */
        /// <summary>Disable autosave/restore ptrs</summary>
        CamDisAutosrp = 0x01000000,
        /// <summary>Disable auto disconnect</summary>
        CamDisAutodisc = 0x02000000,
        /// <summary>Target CCB available</summary>
        CamTgtCcbAvail = 0x04000000,
        /// <summary>The SIM runs in phase mode</summary>
        CamTgtPhaseMode = 0x08000000,
        /// <summary>Message buffer valid</summary>
        CamMsgbValid = 0x10000000,
        /// <summary>Status buffer valid</summary>
        CamStatusValid = 0x20000000,
        /// <summary>Data buffer valid</summary>
        CamDatabValid = 0x40000000,
        /* Host target Mode flags */
        /// <summary>Send sense data with status</summary>
        CamSendSense = 0x08000000,
        /// <summary>Terminate I/O Message sup.</summary>
        CamTermIo = 0x10000000,
        /// <summary>Disconnects are mandatory</summary>
        CamDisconnect = 0x20000000,
        /// <summary>Send status after data phase</summary>
        CamSendStatus = 0x40000000,
        /// <summary>Call callback without lock.</summary>
        CamUnlocked = 0x80000000
    }

    [Obsolete]
    internal enum CamStatus : uint
    {
        /// <summary>CCB request is in progress</summary>
        CamReqInprog = 0x00,

        /// <summary>CCB request completed without error</summary>
        CamReqCmp = 0x01,

        /// <summary>CCB request aborted by the host</summary>
        CamReqAborted = 0x02,

        /// <summary>Unable to abort CCB request</summary>
        CamUaAbort = 0x03,

        /// <summary>CCB request completed with an error</summary>
        CamReqCmpErr = 0x04,

        /// <summary>CAM subsystem is busy</summary>
        CamBusy = 0x05,

        /// <summary>CCB request was invalid</summary>
        CamReqInvalid = 0x06,

        /// <summary>Supplied Path ID is invalid</summary>
        CamPathInvalid = 0x07,

        /// <summary>SCSI Device Not Installed/there</summary>
        CamDevNotThere = 0x08,

        /// <summary>Unable to terminate I/O CCB request</summary>
        CamUaTermio = 0x09,

        /// <summary>Target Selection Timeout</summary>
        CamSelTimeout = 0x0a,

        /// <summary>Command timeout</summary>
        CamCmdTimeout = 0x0b,

        /// <summary>SCSI error, look at error code in CCB</summary>
        CamScsiStatusError = 0x0c,

        /// <summary>Message Reject Received</summary>
        CamMsgRejectRec = 0x0d,

        /// <summary>SCSI Bus Reset Sent/Received</summary>
        CamScsiBusReset = 0x0e,

        /// <summary>Uncorrectable parity error occurred</summary>
        CamUncorParity = 0x0f,

        /// <summary>Autosense: request sense cmd fail</summary>
        CamAutosenseFail = 0x10,

        /// <summary>No HBA Detected error</summary>
        CamNoHba = 0x11,

        /// <summary>Data Overrun error</summary>
        CamDataRunErr = 0x12,

        /// <summary>Unexpected Bus Free</summary>
        CamUnexpBusfree = 0x13,

        /// <summary>Target Bus Phase Sequence Failure</summary>
        CamSequenceFail = 0x14,

        /// <summary>CCB length supplied is inadequate</summary>
        CamCcbLenErr = 0x15,

        /// <summary>Unable to provide requested capability</summary>
        CamProvideFail = 0x16,

        /// <summary>A SCSI BDR msg was sent to target</summary>
        CamBdrSent = 0x17,

        /// <summary>CCB request terminated by the host</summary>
        CamReqTermio = 0x18,

        /// <summary>Unrecoverable Host Bus Adapter Error</summary>
        CamUnrecHbaError = 0x19,

        /// <summary>Request was too large for this host</summary>
        CamReqTooBig = 0x1a,

        /// <summary>
        ///     This request should be requeued to preserve transaction ordering. This typically occurs when the SIM
        ///     recognizes an error that should freeze the queue and must place additional requests for the target at the sim level
        ///     back into the XPT queue.
        /// </summary>
        CamRequeueReq = 0x1b,

        /// <summary>ATA error, look at error code in CCB</summary>
        CamAtaStatusError = 0x1c,

        /// <summary>Initiator/Target Nexus lost.</summary>
        CamScsiItNexusLost = 0x1d,

        /// <summary>SMP error, look at error code in CCB</summary>
        CamSmpStatusError = 0x1e,

        /// <summary>Command completed without error but  exceeded the soft timeout threshold.</summary>
        CamReqSofttimeout = 0x1f,

        /*
         * 0x20 - 0x32 are unassigned
         */

        /// <summary>Initiator Detected Error</summary>
        CamIde = 0x33,

        /// <summary>Resource Unavailable</summary>
        CamResrcUnavail = 0x34,

        /// <summary>Unacknowledged Event by Host</summary>
        CamUnackedEvent = 0x35,

        /// <summary>Message Received in Host Target Mode</summary>
        CamMessageRecv = 0x36,

        /// <summary>Invalid CDB received in Host Target Mode</summary>
        CamInvalidCdb = 0x37,

        /// <summary>Lun supplied is invalid</summary>
        CamLunInvalid = 0x38,

        /// <summary>Target ID supplied is invalid</summary>
        CamTidInvalid = 0x39,

        /// <summary>The requested function is not available</summary>
        CamFuncNotavail = 0x3a,

        /// <summary>Nexus is not established</summary>
        CamNoNexus = 0x3b,

        /// <summary>The initiator ID is invalid</summary>
        CamIidInvalid = 0x3c,

        /// <summary>The SCSI CDB has been received</summary>
        CamCdbRecvd = 0x3d,

        /// <summary>The LUN is already enabled for target mode</summary>
        CamLunAlrdyEna = 0x3e,

        /// <summary>SCSI Bus Busy</summary>
        CamScsiBusy = 0x3f,

        /*
         * Flags
         */

        /// <summary>The DEV queue is frozen w/this err</summary>
        CamDevQfrzn = 0x40,

        /// <summary>Autosense data valid for target</summary>
        CamAutosnsValid = 0x80,

        /// <summary>SIM ready to take more commands</summary>
        CamReleaseSimq = 0x100,

        /// <summary>SIM has this command in its queue</summary>
        CamSimQueued = 0x200,

        /// <summary>Quality of service data is valid</summary>
        CamQosValid = 0x400,

        /// <summary>Mask bits for just the status #</summary>
        CamStatusMask = 0x3F,

        /*
         * Target Specific Adjunct Status
         */

        /// <summary>sent sense with status</summary>
        CamSentSense = 0x40000000
    }
}