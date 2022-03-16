// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Identify.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Common structures for ATA devices.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines a high level interpretation of the ATA IDENTIFY response.
//
// --[ License ] --------------------------------------------------------------
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

// ReSharper disable UnusedMember.Global

namespace Aaru.CommonTypes.Structs.Devices.ATA;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.Console;
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;

/// <summary>
///     Information from following standards: T10-791D rev. 4c (ATA) T10-948D rev. 4c (ATA-2) T13-1153D rev. 18
///     (ATA/ATAPI-4) T13-1321D rev. 3 (ATA/ATAPI-5) T13-1410D rev. 3b (ATA/ATAPI-6) T13-1532D rev. 4b (ATA/ATAPI-7)
///     T13-1699D rev. 3f (ATA8-ACS) T13-1699D rev. 4a (ATA8-ACS) T13-2015D rev. 2 (ACS-2) T13-2161D rev. 5 (ACS-3) CF+
///     &amp; CF Specification rev. 1.4 (CFA)
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class Identify
{
    /// <summary>Capabilities flag bits.</summary>
    [Flags]
    public enum CapabilitiesBit : ushort
    {
        /// <summary>ATAPI: Interleaved DMA supported</summary>
        InterleavedDMA = 0x8000,
        /// <summary>ATAPI: Command queueing supported</summary>
        CommandQueue = 0x4000,
        /// <summary>Standby timer values are standard</summary>
        StandardStandbyTimer = 0x2000,
        /// <summary>ATAPI: Overlap operation supported</summary>
        OverlapOperation = 0x2000,
        /// <summary>ATAPI: ATA software reset required Obsoleted in ATA/ATAPI-4</summary>
        RequiresATASoftReset = 0x1000,
        /// <summary>IORDY is supported</summary>
        IORDY = 0x0800,
        /// <summary>IORDY can be disabled</summary>
        CanDisableIORDY = 0x0400,
        /// <summary>LBA is supported</summary>
        LBASupport = 0x0200,
        /// <summary>DMA is supported</summary>
        DMASupport = 0x0100,
        /// <summary>Vendor unique Obsoleted in ATA/ATAPI-4</summary>
        VendorBit7 = 0x0080,
        /// <summary>Vendor unique Obsoleted in ATA/ATAPI-4</summary>
        VendorBit6 = 0x0040,
        /// <summary>Vendor unique Obsoleted in ATA/ATAPI-4</summary>
        VendorBit5 = 0x0020,
        /// <summary>Vendor unique Obsoleted in ATA/ATAPI-4</summary>
        VendorBit4 = 0x0010,
        /// <summary>Vendor unique Obsoleted in ATA/ATAPI-4</summary>
        VendorBit3 = 0x0008,
        /// <summary>Vendor unique Obsoleted in ATA/ATAPI-4</summary>
        VendorBit2 = 0x0004,
        /// <summary>Long Physical Alignment setting bit 1</summary>
        PhysicalAlignment1 = 0x0002,
        /// <summary>Long Physical Alignment setting bit 0</summary>
        PhysicalAlignment0 = 0x0001
    }

    /// <summary>More capabilities flag bits.</summary>
    [Flags]
    public enum CapabilitiesBit2 : ushort
    {
        /// <summary>MUST NOT be set</summary>
        MustBeClear = 0x8000,
        /// <summary>MUST be set</summary>
        MustBeSet = 0x4000,
        #pragma warning disable 1591
        Reserved13 = 0x2000,
        Reserved12 = 0x1000,
        Reserved11 = 0x0800,
        Reserved10 = 0x0400,
        Reserved09 = 0x0200,
        Reserved08 = 0x0100,
        Reserved07 = 0x0080,
        Reserved06 = 0x0040,
        Reserved05 = 0x0020,
        Reserved04 = 0x0010,
        Reserved03 = 0x0008,
        Reserved02 = 0x0004,
        Reserved01 = 0x0002,
        #pragma warning restore 1591
        /// <summary>Indicates a device specific minimum standby timer value</summary>
        SpecificStandbyTimer = 0x0001
    }

    /// <summary>Even more capabilities flag bits.</summary>
    [Flags]
    public enum CapabilitiesBit3 : byte
    {
        /// <summary>BLOCK ERASE EXT supported</summary>
        BlockErase = 0x0080,
        /// <summary>OVERWRITE EXT supported</summary>
        Overwrite = 0x0040,
        /// <summary>CRYPTO SCRAMBLE EXT supported</summary>
        CryptoScramble = 0x0020,
        /// <summary>Sanitize feature set is supported</summary>
        Sanitize = 0x0010,
        /// <summary>If unset, sanitize commands are specified by ACS-2</summary>
        SanitizeCommands = 0x0008,
        /// <summary>SANITIZE ANTIFREEZE LOCK EXT is supported</summary>
        SanitizeAntifreeze = 0x0004,
        #pragma warning disable 1591
        Reserved01 = 0x0002,
        #pragma warning restore 1591
        /// <summary>Multiple logical sector setting is valid</summary>
        MultipleValid = 0x0001
    }

    /// <summary>Command set flag bits.</summary>
    [Flags]
    public enum CommandSetBit : ushort
    {
        /// <summary>Already obsolete in ATA/ATAPI-4, reserved in ATA3</summary>
        Obsolete15 = 0x8000,
        /// <summary>NOP is supported</summary>
        Nop = 0x4000,
        /// <summary>READ BUFFER is supported</summary>
        ReadBuffer = 0x2000,
        /// <summary>WRITE BUFFER is supported</summary>
        WriteBuffer = 0x1000,
        /// <summary>Already obsolete in ATA/ATAPI-4, reserved in ATA3</summary>
        Obsolete11 = 0x0800,
        /// <summary>Host Protected Area is supported</summary>
        HPA = 0x0400,
        /// <summary>DEVICE RESET is supported</summary>
        DeviceReset = 0x0200,
        /// <summary>SERVICE interrupt is supported</summary>
        Service = 0x0100,
        /// <summary>Release is supported</summary>
        Release = 0x0080,
        /// <summary>Look-ahead is supported</summary>
        LookAhead = 0x0040,
        /// <summary>Write cache is supported</summary>
        WriteCache = 0x0020,
        /// <summary>PACKET command set is supported</summary>
        Packet = 0x0010,
        /// <summary>Power Management feature set is supported</summary>
        PowerManagement = 0x0008,
        /// <summary>Removable Media feature set is supported</summary>
        RemovableMedia = 0x0004,
        /// <summary>Security Mode feature set is supported</summary>
        SecurityMode = 0x0002,
        /// <summary>SMART feature set is supported</summary>
        SMART = 0x0001
    }

    /// <summary>More command set flag bits.</summary>
    [Flags]
    public enum CommandSetBit2 : ushort
    {
        /// <summary>MUST NOT be set</summary>
        MustBeClear = 0x8000,
        /// <summary>MUST BE SET</summary>
        MustBeSet = 0x4000,
        /// <summary>FLUSH CACHE EXT supported</summary>
        FlushCacheExt = 0x2000,
        /// <summary>FLUSH CACHE supported</summary>
        FlushCache = 0x1000,
        /// <summary>Device Configuration Overlay feature set supported</summary>
        DCO = 0x0800,
        /// <summary>48-bit LBA supported</summary>
        LBA48 = 0x0400,
        /// <summary>Automatic Acoustic Management supported</summary>
        AAM = 0x0200,
        /// <summary>SET MAX security extension supported</summary>
        SetMax = 0x0100,
        /// <summary>Address Offset Reserved Area Boot NCITS TR27:2001</summary>
        AddressOffsetReservedAreaBoot = 0x0080,
        /// <summary>SET FEATURES required to spin-up</summary>
        SetFeaturesRequired = 0x0040,
        /// <summary>Power-Up in standby feature set supported</summary>
        PowerUpInStandby = 0x0020,
        /// <summary>Removable Media Status Notification feature set is supported</summary>
        RemovableNotification = 0x0010,
        /// <summary>Advanced Power Management feature set is supported</summary>
        APM = 0x0008,
        /// <summary>Compact Flash feature set is supported</summary>
        CompactFlash = 0x0004,
        /// <summary>READ DMA QUEUED and WRITE DMA QUEUED are supported</summary>
        RWQueuedDMA = 0x0002,
        /// <summary>DOWNLOAD MICROCODE is supported</summary>
        DownloadMicrocode = 0x0001
    }

    /// <summary>Even more command set flag bits.</summary>
    [Flags]
    public enum CommandSetBit3 : ushort
    {
        /// <summary>MUST NOT be set</summary>
        MustBeClear = 0x8000,
        /// <summary>MUST BE SET</summary>
        MustBeSet = 0x4000,
        /// <summary>IDLE IMMEDIATE with UNLOAD FEATURE is supported</summary>
        IdleImmediate = 0x2000,
        /// <summary>Reserved for INCITS TR-37/2004</summary>
        Reserved12 = 0x1000,
        /// <summary>Reserved for INCITS TR-37/2004</summary>
        Reserved11 = 0x0800,
        /// <summary>URG bit is supported in WRITE STREAM DMA EXT and WRITE STREAM EXT</summary>
        WriteURG = 0x0400,
        /// <summary>URG bit is supported in READ STREAM DMA EXT and READ STREAM EXT</summary>
        ReadURG = 0x0200,
        /// <summary>64-bit World Wide Name is supported</summary>
        WWN = 0x0100,
        /// <summary>WRITE DMA QUEUED FUA EXT is supported</summary>
        FUAWriteQ = 0x0080,
        /// <summary>WRITE DMA FUA EXT and WRITE MULTIPLE FUA EXT are supported</summary>
        FUAWrite = 0x0040,
        /// <summary>General Purpose Logging feature supported</summary>
        GPL = 0x0020,
        /// <summary>Streaming feature set is supported</summary>
        Streaming = 0x0010,
        /// <summary>Media Card Pass Through command set supported</summary>
        MCPT = 0x0008,
        /// <summary>Media serial number supported</summary>
        MediaSerial = 0x0004,
        /// <summary>SMART self-test supported</summary>
        SMARTSelfTest = 0x0002,
        /// <summary>SMART error logging supported</summary>
        SMARTLog = 0x0001
    }

    /// <summary>Yet more command set flag bits.</summary>
    [Flags]
    public enum CommandSetBit4 : ushort
    {
        /// <summary>MUST NOT be set</summary>
        MustBeClear = 0x8000,
        /// <summary>MUST be set</summary>
        MustBeSet = 0x4000,
        #pragma warning disable 1591
        Reserved13 = 0x2000,
        Reserved12 = 0x1000,
        Reserved11 = 0x0800,
        Reserved10 = 0x0400,
        #pragma warning restore 1591
        /// <summary>DSN feature set is supported</summary>
        DSN = 0x0200,
        /// <summary>Accessible Max Address Configuration is supported</summary>
        AMAC = 0x0100,
        /// <summary>Extended Power Conditions is supported</summary>
        ExtPowerCond = 0x0080,
        /// <summary>Extended Status Reporting is supported</summary>
        ExtStatusReport = 0x0040,
        /// <summary>Free-fall Control feature set is supported</summary>
        FreeFallControl = 0x0020,
        /// <summary>Supports segmented feature in DOWNLOAD MICROCODE</summary>
        SegmentedDownloadMicrocode = 0x0010,
        /// <summary>READ/WRITE DMA EXT GPL are supported</summary>
        RWDMAExtGpl = 0x0008,
        /// <summary>WRITE UNCORRECTABLE is supported</summary>
        WriteUnc = 0x0004,
        /// <summary>Write/Read/Verify is supported</summary>
        WRV = 0x0002,
        /// <summary>Reserved for DT1825</summary>
        DT1825 = 0x0001
    }

    /// <summary>Yet again more command set flag bits.</summary>
    [Flags]
    public enum CommandSetBit5 : ushort
    {
        /// <summary>Supports CFast Specification</summary>
        CFast = 0x8000,
        /// <summary>Deterministic read after TRIM is supported</summary>
        DeterministicTrim = 0x4000,
        /// <summary>Long physical sector alignment error reporting control is supported</summary>
        LongPhysSectorAligError = 0x2000,
        /// <summary>DEVICE CONFIGURATION IDENTIFY DMA and DEVICE CONFIGURATION SET DMA are supported</summary>
        DeviceConfDMA = 0x1000,
        /// <summary>READ BUFFER DMA is supported</summary>
        ReadBufferDMA = 0x0800,
        /// <summary>WRITE BUFFER DMA is supported</summary>
        WriteBufferDMA = 0x0400,
        /// <summary>SET PASSWORD DMA and SET UNLOCK DMA are supported</summary>
        SetMaxDMA = 0x0200,
        /// <summary>DOWNLOAD MICROCODE DMA is supported</summary>
        DownloadMicroCodeDMA = 0x0100,
        /// <summary>Reserved for IEEE-1667</summary>
        IEEE1667 = 0x0080,
        /// <summary>Optional ATA 28-bit commands are supported</summary>
        Ata28 = 0x0040,
        /// <summary>Read zero after TRIM is supported</summary>
        ReadZeroTrim = 0x0020,
        /// <summary>Device encrypts all user data</summary>
        Encrypted = 0x0010,
        /// <summary>Extended number of user addressable sectors is supported</summary>
        ExtSectors = 0x0008,
        /// <summary>All write cache is non-volatile</summary>
        AllCacheNV = 0x0004,
        /// <summary>Zoned capabilities bit 1</summary>
        ZonedBit1 = 0x0002,
        /// <summary>Zoned capabilities bit 0</summary>
        ZonedBit0 = 0x0001
    }

    /// <summary>Data set management flag bits.</summary>
    [Flags]
    public enum DataSetMgmtBit : ushort
    {
        #pragma warning disable 1591
        Reserved15 = 0x8000,
        Reserved14 = 0x4000,
        Reserved13 = 0x2000,
        Reserved12 = 0x1000,
        Reserved11 = 0x0800,
        Reserved10 = 0x0400,
        Reserved09 = 0x0200,
        Reserved08 = 0x0100,
        Reserved07 = 0x0080,
        Reserved06 = 0x0040,
        Reserved05 = 0x0020,
        Reserved04 = 0x0010,
        Reserved03 = 0x0008,
        Reserved02 = 0x0004,
        Reserved01 = 0x0002,
        #pragma warning restore 1591
        /// <summary>TRIM is supported</summary>
        Trim = 0x0001
    }

    /// <summary>Device form factor</summary>
    public enum DeviceFormFactorEnum : ushort
    {
        /// <summary>Size not reported</summary>
        NotReported = 0,
        /// <summary>5.25"</summary>
        FiveAndQuarter = 1,
        /// <summary>3.5"</summary>
        ThreeAndHalf = 2,
        /// <summary>2.5"</summary>
        TwoAndHalf = 3,
        /// <summary>1.8"</summary>
        OnePointEight = 4,
        /// <summary>Less than 1.8"</summary>
        LessThanOnePointEight = 5
    }

    /// <summary>Extended identify flag bits.</summary>
    [Flags]
    public enum ExtendedIdentifyBit : byte
    {
        /// <summary>Reserved</summary>
        Reserved07 = 0x80,
        /// <summary>Reserved</summary>
        Reserved06 = 0x40,
        /// <summary>Reserved</summary>
        Reserved05 = 0x20,
        /// <summary>Reserved</summary>
        Reserved04 = 0x10,
        /// <summary>Reserved</summary>
        Reserved03 = 0x08,
        /// <summary>Identify word 88 is valid</summary>
        Word88Valid = 0x04,
        /// <summary>Identify words 64 to 70 are valid</summary>
        Words64to70Valid = 0x02,
        /// <summary>Identify words 54 to 58 are valid</summary>
        Words54to58Valid = 0x01
    }

    /// <summary>General configuration flag bits.</summary>
    [Flags]
    public enum GeneralConfigurationBit : ushort
    {
        /// <summary>Set on ATAPI</summary>
        NonMagnetic = 0x8000,
        /// <summary>Format speed tolerance gap is required Obsoleted in ATA-2</summary>
        FormatGapReq = 0x4000,
        /// <summary>Track offset option is available Obsoleted in ATA-2</summary>
        TrackOffset = 0x2000,
        /// <summary>Data strobe offset option is available Obsoleted in ATA-2</summary>
        DataStrobeOffset = 0x1000,
        /// <summary>Rotational speed tolerance is higher than 0,5% Obsoleted in ATA-2</summary>
        RotationalSpeedTolerance = 0x0800,
        /// <summary>Disk transfer rate is &gt; 10 Mb/s Obsoleted in ATA-2</summary>
        UltraFastIDE = 0x0400,
        /// <summary>Disk transfer rate is  &gt; 5 Mb/s but &lt;= 10 Mb/s Obsoleted in ATA-2</summary>
        FastIDE = 0x0200,
        /// <summary>Disk transfer rate is &lt;= 5 Mb/s Obsoleted in ATA-2</summary>
        SlowIDE = 0x0100,
        /// <summary>Drive uses removable media</summary>
        Removable = 0x0080,
        /// <summary>Drive is fixed Obsoleted in ATA/ATAPI-6</summary>
        Fixed = 0x0040,
        /// <summary>Spindle motor control is implemented Obsoleted in ATA-2</summary>
        SpindleControl = 0x0020,
        /// <summary>Head switch time is bigger than 15 µsec. Obsoleted in ATA-2</summary>
        HighHeadSwitch = 0x0010,
        /// <summary>Drive is not MFM encoded Obsoleted in ATA-2</summary>
        NotMFM = 0x0008,
        /// <summary>Drive is soft sectored Obsoleted in ATA-2</summary>
        SoftSector = 0x0004,
        /// <summary>Response incomplete Since ATA/ATAPI-5</summary>
        IncompleteResponse = 0x0004,
        /// <summary>Drive is hard sectored Obsoleted in ATA-2</summary>
        HardSector = 0x0002,
        /// <summary>Reserved</summary>
        Reserved = 0x0001
    }

    /// <summary>Word 80 Major version</summary>
    [Flags]
    public enum MajorVersionBit : ushort
    {
        #pragma warning disable 1591
        Reserved15 = 0x8000,
        Reserved14 = 0x4000,
        Reserved13 = 0x2000,
        Reserved12 = 0x1000,
        #pragma warning restore 1591
        /// <summary>ACS-4</summary>
        ACS4 = 0x0800,
        /// <summary>ACS-3</summary>
        ACS3 = 0x0400,
        /// <summary>ACS-2</summary>
        ACS2 = 0x0200,
        /// <summary>ATA8-ACS</summary>
        Ata8ACS = 0x0100,
        /// <summary>ATA/ATAPI-7</summary>
        AtaAtapi7 = 0x0080,
        /// <summary>ATA/ATAPI-6</summary>
        AtaAtapi6 = 0x0040,
        /// <summary>ATA/ATAPI-5</summary>
        AtaAtapi5 = 0x0020,
        /// <summary>ATA/ATAPI-4</summary>
        AtaAtapi4 = 0x0010,
        /// <summary>ATA-3</summary>
        Ata3 = 0x0008,
        /// <summary>ATA-2</summary>
        Ata2 = 0x0004,
        /// <summary>ATA-1</summary>
        Ata1 = 0x0002,
        #pragma warning disable 1591
        Reserved00 = 0x0001
        #pragma warning restore 1591
    }

    /// <summary>SATA capabilities flags</summary>
    [Flags]
    public enum SATACapabilitiesBit : ushort
    {
        /// <summary>Supports READ LOG DMA EXT</summary>
        ReadLogDMAExt = 0x8000,
        /// <summary>Supports device automatic partial to slumber transitions</summary>
        DevSlumbTrans = 0x4000,
        /// <summary>Supports host automatic partial to slumber transitions</summary>
        HostSlumbTrans = 0x2000,
        /// <summary>Supports NCQ priority</summary>
        NCQPriority = 0x1000,
        /// <summary>Supports unload while NCQ commands are outstanding</summary>
        UnloadNCQ = 0x0800,
        /// <summary>Supports PHY Event Counters</summary>
        PHYEventCounter = 0x0400,
        /// <summary>Supports receipt of host initiated power management requests</summary>
        PowerReceipt = 0x0200,
        /// <summary>Supports NCQ</summary>
        NCQ = 0x0100,
        #pragma warning disable 1591
        Reserved07 = 0x0080,
        Reserved06 = 0x0040,
        Reserved05 = 0x0020,
        Reserved04 = 0x0010,
        #pragma warning restore 1591
        /// <summary>Supports SATA Gen. 3 Signaling Speed (6.0Gb/s)</summary>
        Gen3Speed = 0x0008,
        /// <summary>Supports SATA Gen. 2 Signaling Speed (3.0Gb/s)</summary>
        Gen2Speed = 0x0004,
        /// <summary>Supports SATA Gen. 1 Signaling Speed (1.5Gb/s)</summary>
        Gen1Speed = 0x0002,
        /// <summary>MUST NOT be set</summary>
        Clear = 0x0001
    }

    /// <summary>More SATA capabilities flags</summary>
    [Flags]
    public enum SATACapabilitiesBit2 : ushort
    {
        #pragma warning disable 1591
        Reserved15 = 0x8000,
        Reserved14 = 0x4000,
        Reserved13 = 0x2000,
        Reserved12 = 0x1000,
        Reserved11 = 0x0800,
        Reserved10 = 0x0400,
        Reserved09 = 0x0200,
        Reserved08 = 0x0100,
        Reserved07 = 0x0080,
        #pragma warning restore 1591
        /// <summary>Supports RECEIVE FPDMA QUEUED and SEND FPDMA QUEUED</summary>
        FPDMAQ = 0x0040,
        /// <summary>Supports NCQ Queue Management</summary>
        NCQMgmt = 0x0020,
        /// <summary>ATAPI: Supports host environment detect</summary>
        HostEnvDetect = 0x0020,
        /// <summary>Supports NCQ streaming</summary>
        NCQStream = 0x0010,
        /// <summary>ATAPI: Supports device attention on slimline connected devices</summary>
        DevAttSlimline = 0x0010,
        /// <summary>Coded value indicating current negotiated Serial ATA signal speed</summary>
        CurrentSpeedBit2 = 0x0008,
        /// <summary>Coded value indicating current negotiated Serial ATA signal speed</summary>
        CurrentSpeedBit1 = 0x0004,
        /// <summary>Coded value indicating current negotiated Serial ATA signal speed</summary>
        CurrentSpeedBit0 = 0x0002,
        /// <summary>MUST NOT be set</summary>
        Clear = 0x0001
    }

    /// <summary>SATA features flags</summary>
    [Flags]
    public enum SATAFeaturesBit : ushort
    {
        #pragma warning disable 1591
        Reserved15 = 0x8000,
        Reserved14 = 0x4000,
        Reserved13 = 0x2000,
        Reserved12 = 0x1000,
        Reserved11 = 0x0800,
        Reserved10 = 0x0400,
        Reserved09 = 0x0200,
        Reserved08 = 0x0100,
        #pragma warning restore 1591
        /// <summary>Supports NCQ autosense</summary>
        NCQAutoSense = 0x0080,
        /// <summary>Automatic Partial to Slumber transitions are enabled</summary>
        EnabledSlumber = 0x0080,
        /// <summary>Supports Software Settings Preservation</summary>
        SettingsPreserve = 0x0040,
        /// <summary>Supports hardware feature control</summary>
        HardwareFeatureControl = 0x0020,
        /// <summary>ATAPI: Asynchronous notification</summary>
        AsyncNotification = 0x0020,
        /// <summary>Supports in-order data delivery</summary>
        InOrderData = 0x0010,
        /// <summary>Supports initiating power management</summary>
        InitPowerMgmt = 0x0008,
        /// <summary>Supports DMA Setup auto-activation</summary>
        DMASetup = 0x0004,
        /// <summary>Supports non-zero buffer offsets</summary>
        NonZeroBufferOffset = 0x0002,
        /// <summary>MUST NOT be set</summary>
        Clear = 0x0001
    }

    /// <summary>SCT Command Transport flags</summary>
    [Flags]
    public enum SCTCommandTransportBit : ushort
    {
        #pragma warning disable 1591
        Vendor15   = 0x8000,
        Vendor14   = 0x4000,
        Vendor13   = 0x2000,
        Vendor12   = 0x1000,
        Reserved11 = 0x0800,
        Reserved10 = 0x0400,
        Reserved09 = 0x0200,
        Reserved08 = 0x0100,
        Reserved07 = 0x0080,
        Reserved06 = 0x0040,
        #pragma warning restore 1591
        /// <summary>SCT Command Transport Data Tables supported</summary>
        DataTables = 0x0020,
        /// <summary>SCT Command Transport Features Control supported</summary>
        FeaturesControl = 0x0010,
        /// <summary>SCT Command Transport Error Recovery Control supported</summary>
        ErrorRecoveryControl = 0x0008,
        /// <summary>SCT Command Transport Write Same supported</summary>
        WriteSame = 0x0004,
        /// <summary>SCT Command Transport Long Sector Address supported</summary>
        LongSectorAccess = 0x0002,
        /// <summary>SCT Command Transport supported</summary>
        Supported = 0x0001
    }

    /// <summary>Security status flag bits.</summary>
    [Flags]
    public enum SecurityStatusBit : ushort
    {
        #pragma warning disable 1591
        Reserved15 = 0x8000,
        Reserved14 = 0x4000,
        Reserved13 = 0x2000,
        Reserved12 = 0x1000,
        Reserved11 = 0x0800,
        Reserved10 = 0x0400,
        Reserved09 = 0x0200,
        #pragma warning restore 1591
        /// <summary>Maximum security level</summary>
        Maximum = 0x0100,
        #pragma warning disable 1591
        Reserved07 = 0x0080,
        Reserved06 = 0x0040,
        #pragma warning restore 1591
        /// <summary>Supports enhanced security erase</summary>
        Enhanced = 0x0020,
        /// <summary>Security count expired</summary>
        Expired = 0x0010,
        /// <summary>Security frozen</summary>
        Frozen = 0x0008,
        /// <summary>Security locked</summary>
        Locked = 0x0004,
        /// <summary>Security enabled</summary>
        Enabled = 0x0002,
        /// <summary>Security supported</summary>
        Supported = 0x0001
    }

    /// <summary>Specific configuration flags</summary>
    public enum SpecificConfigurationEnum : ushort
    {
        /// <summary>Device requires SET FEATURES to spin up and IDENTIFY DEVICE response is incomplete</summary>
        RequiresSetIncompleteResponse = 0x37C8,
        /// <summary>Device requires SET FEATURES to spin up and IDENTIFY DEVICE response is complete</summary>
        RequiresSetCompleteResponse = 0x738C,
        /// <summary>Device does not requires SET FEATURES to spin up and IDENTIFY DEVICE response is incomplete</summary>
        NotRequiresSetIncompleteResponse = 0x8C73,
        /// <summary>Device does not requires SET FEATURES to spin up and IDENTIFY DEVICE response is complete</summary>
        NotRequiresSetCompleteResponse = 0xC837
    }

    /// <summary>Transfer mode flags</summary>
    [Flags]
    public enum TransferMode : byte
    {
        #pragma warning disable 1591
        Mode7 = 0x80,
        Mode6 = 0x40,
        Mode5 = 0x20,
        Mode4 = 0x10,
        Mode3 = 0x08,
        Mode2 = 0x04,
        Mode1 = 0x02,
        Mode0 = 0x01
        #pragma warning restore 1591
    }

    /// <summary>Trusted Computing flags</summary>
    [Flags]
    public enum TrustedComputingBit : ushort
    {
        /// <summary>MUST NOT be set</summary>
        Clear = 0x8000,
        /// <summary>MUST be set</summary>
        Set = 0x4000,
        #pragma warning disable 1591
        Reserved13 = 0x2000,
        Reserved12 = 0x1000,
        Reserved11 = 0x0800,
        Reserved10 = 0x0400,
        Reserved09 = 0x0200,
        Reserved08 = 0x0100,
        Reserved07 = 0x0080,
        Reserved06 = 0x0040,
        Reserved05 = 0x0020,
        Reserved04 = 0x0010,
        Reserved03 = 0x0008,
        Reserved02 = 0x0004,
        Reserved01 = 0x0002,
        #pragma warning restore 1591
        /// <summary>Trusted Computing feature set is supported</summary>
        TrustedComputing = 0x0001
    }

    /// <summary>IDENTIFY DEVICE decoded response</summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
    public struct IdentifyDevice
    {
        /// <summary>
        ///     Word 0 General device configuration On ATAPI devices: Bits 12 to 8 indicate device type as SCSI defined Bits 6
        ///     to 5: 0 = Device shall set DRQ within 3 ms of receiving PACKET 1 = Device shall assert INTRQ when DRQ is set to one
        ///     2 = Device shall set DRQ within 50 µs of receiving PACKET Bits 1 to 0: 0 = 12 byte command packet 1 = 16 byte
        ///     command packet CompactFlash is 0x848A (non magnetic, removable, not MFM, hardsector, and UltraFAST)
        /// </summary>
        public GeneralConfigurationBit GeneralConfiguration;
        /// <summary>Word 1 Cylinders in default translation mode Obsoleted in ATA/ATAPI-6</summary>
        public ushort Cylinders;
        /// <summary>Word 2 Specific configuration</summary>
        public SpecificConfigurationEnum SpecificConfiguration;
        /// <summary>Word 3 Heads in default translation mode Obsoleted in ATA/ATAPI-6</summary>
        public ushort Heads;
        /// <summary>Word 4 Unformatted bytes per track in default translation mode Obsoleted in ATA-2</summary>
        public ushort UnformattedBPT;
        /// <summary>Word 5 Unformatted bytes per sector in default translation mode Obsoleted in ATA-2</summary>
        public ushort UnformattedBPS;
        /// <summary>Word 6 Sectors per track in default translation mode Obsoleted in ATA/ATAPI-6</summary>
        public ushort SectorsPerTrack;
        /// <summary>Words 7 to 8 CFA: Number of sectors per card</summary>
        public uint SectorsPerCard;
        /// <summary>Word 9 Vendor unique Obsoleted in ATA/ATAPI-4</summary>
        public ushort VendorWord9;
        /// <summary>Words 10 to 19 Device serial number, right justified, padded with spaces</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string SerialNumber;
        /// <summary>
        ///     Word 20 Manufacturer defined Obsoleted in ATA-2 0x0001 = single ported single sector buffer 0x0002 = dual
        ///     ported multi sector buffer 0x0003 = dual ported multi sector buffer with reading
        /// </summary>
        public ushort BufferType;
        /// <summary>Word 21 Size of buffer in 512 byte increments Obsoleted in ATA-2</summary>
        public ushort BufferSize;
        /// <summary>Word 22 Bytes of ECC available in READ/WRITE LONG commands Obsoleted in ATA/ATAPI-4</summary>
        public ushort EccBytes;
        /// <summary>Words 23 to 26 Firmware revision, left justified, padded with spaces</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string FirmwareRevision;
        /// <summary>Words 27 to 46 Model number, left justified, padded with spaces</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string Model;
        /// <summary>
        ///     Word 47 bits 7 to 0 Maximum number of sectors that can be transferred per interrupt on read and write multiple
        ///     commands
        /// </summary>
        public byte MultipleMaxSectors;
        /// <summary>Word 47 bits 15 to 8 Vendor unique ATA/ATAPI-4 says it must be 0x80</summary>
        public byte VendorWord47;
        /// <summary>
        ///     Word 48 ATA-1: Set to 1 if it can perform doubleword I/O ATA-2 to ATA/ATAPI-7: Reserved ATA8-ACS: Trusted
        ///     Computing feature set
        /// </summary>
        public TrustedComputingBit TrustedComputing;
        /// <summary>Word 49 Capabilities</summary>
        public CapabilitiesBit Capabilities;
        /// <summary>Word 50 Capabilities</summary>
        public CapabilitiesBit2 Capabilities2;
        /// <summary>Word 51 bits 7 to 0 Vendor unique Obsoleted in ATA/ATAPI-4</summary>
        public byte VendorWord51;
        /// <summary>Word 51 bits 15 to 8 Transfer timing mode in PIO Obsoleted in ATA/ATAPI-4</summary>
        public byte PIOTransferTimingMode;
        /// <summary>Word 52 bits 7 to 0 Vendor unique Obsoleted in ATA/ATAPI-4</summary>
        public byte VendorWord52;
        /// <summary>Word 52 bits 15 to 8 Transfer timing mode in DMA Obsoleted in ATA/ATAPI-4</summary>
        public byte DMATransferTimingMode;
        /// <summary>Word 53 bits 7 to 0 Reports if words 54 to 58 are valid</summary>
        public ExtendedIdentifyBit ExtendedIdentify;
        /// <summary>Word 53 bits 15 to 8 Free-fall Control Sensitivity</summary>
        public byte FreeFallSensitivity;
        /// <summary>Word 54 Cylinders in current translation mode Obsoleted in ATA/ATAPI-6</summary>
        public ushort CurrentCylinders;
        /// <summary>Word 55 Heads in current translation mode Obsoleted in ATA/ATAPI-6</summary>
        public ushort CurrentHeads;
        /// <summary>Word 56 Sectors per track in current translation mode Obsoleted in ATA/ATAPI-6</summary>
        public ushort CurrentSectorsPerTrack;
        /// <summary>Words 57 to 58 Total sectors currently user-addressable Obsoleted in ATA/ATAPI-6</summary>
        public uint CurrentSectors;
        /// <summary>Word 59 bits 7 to 0 Number of sectors currently set to transfer on a READ/WRITE MULTIPLE command</summary>
        public byte MultipleSectorNumber;
        /// <summary>Word 59 bits 15 to 8 Indicates if <see cref="MultipleSectorNumber" /> is valid</summary>
        public CapabilitiesBit3 Capabilities3;
        /// <summary>Words 60 to 61 If drive supports LBA, how many sectors are addressable using LBA</summary>
        public uint LBASectors;
        /// <summary>
        ///     Word 62 bits 7 to 0 Single word DMA modes available Obsoleted in ATA/ATAPI-4 In ATAPI it's not obsolete,
        ///     indicates UDMA mode (UDMA7 is instead MDMA0)
        /// </summary>
        public TransferMode DMASupported;
        /// <summary>
        ///     Word 62 bits 15 to 8 Single word DMA mode currently active Obsoleted in ATA/ATAPI-4 In ATAPI it's not
        ///     obsolete, bits 0 and 1 indicate MDMA mode+1, bit 10 indicates DMA is supported and bit 15 indicates DMADIR bit in
        ///     PACKET is required for DMA transfers
        /// </summary>
        public TransferMode DMAActive;
        /// <summary>Word 63 bits 7 to 0 Multiword DMA modes available</summary>
        public TransferMode MDMASupported;
        /// <summary>Word 63 bits 15 to 8 Multiword DMA mode currently active</summary>
        public TransferMode MDMAActive;

        /// <summary>Word 64 bits 7 to 0 Supported Advanced PIO transfer modes</summary>
        public TransferMode APIOSupported;
        /// <summary>Word 64 bits 15 to 8 Reserved</summary>
        public byte ReservedWord64;
        /// <summary>Word 65 Minimum MDMA transfer cycle time per word in nanoseconds</summary>
        public ushort MinMDMACycleTime;
        /// <summary>Word 66 Recommended MDMA transfer cycle time per word in nanoseconds</summary>
        public ushort RecMDMACycleTime;
        /// <summary>Word 67 Minimum PIO transfer cycle time without flow control in nanoseconds</summary>
        public ushort MinPIOCycleTimeNoFlow;
        /// <summary>Word 68 Minimum PIO transfer cycle time with IORDY flow control in nanoseconds</summary>
        public ushort MinPIOCycleTimeFlow;

        /// <summary>Word 69 Additional supported</summary>
        public CommandSetBit5 CommandSet5;
        /// <summary>Word 70 Reserved</summary>
        public ushort ReservedWord70;
        /// <summary>Word 71 ATAPI: Typical time in ns from receipt of PACKET to release bus</summary>
        public ushort PacketBusRelease;
        /// <summary>Word 72 ATAPI: Typical time in ns from receipt of SERVICE to clear BSY</summary>
        public ushort ServiceBusyClear;
        /// <summary>Word 73 Reserved</summary>
        public ushort ReservedWord73;
        /// <summary>Word 74 Reserved</summary>
        public ushort ReservedWord74;

        /// <summary>Word 75 Maximum Queue depth</summary>
        public ushort MaxQueueDepth;

        /// <summary>Word 76 Serial ATA Capabilities</summary>
        public SATACapabilitiesBit SATACapabilities;
        /// <summary>Word 77 Serial ATA Additional Capabilities</summary>
        public SATACapabilitiesBit2 SATACapabilities2;

        /// <summary>Word 78 Supported Serial ATA features</summary>
        public SATAFeaturesBit SATAFeatures;
        /// <summary>Word 79 Enabled Serial ATA features</summary>
        public SATAFeaturesBit EnabledSATAFeatures;

        /// <summary>Word 80 Major version of ATA/ATAPI standard supported</summary>
        public MajorVersionBit MajorVersion;
        /// <summary>Word 81 Minimum version of ATA/ATAPI standard supported</summary>
        public ushort MinorVersion;

        /// <summary>Word 82 Supported command/feature sets</summary>
        public CommandSetBit CommandSet;
        /// <summary>Word 83 Supported command/feature sets</summary>
        public CommandSetBit2 CommandSet2;
        /// <summary>Word 84 Supported command/feature sets</summary>
        public CommandSetBit3 CommandSet3;

        /// <summary>Word 85 Enabled command/feature sets</summary>
        public CommandSetBit EnabledCommandSet;
        /// <summary>Word 86 Enabled command/feature sets</summary>
        public CommandSetBit2 EnabledCommandSet2;
        /// <summary>Word 87 Enabled command/feature sets</summary>
        public CommandSetBit3 EnabledCommandSet3;

        /// <summary>Word 88 bits 7 to 0 Supported Ultra DMA transfer modes</summary>
        public TransferMode UDMASupported;
        /// <summary>Word 88 bits 15 to 8 Selected Ultra DMA transfer modes</summary>
        public TransferMode UDMAActive;

        /// <summary>Word 89 Time required for security erase completion</summary>
        public ushort SecurityEraseTime;
        /// <summary>Word 90 Time required for enhanced security erase completion</summary>
        public ushort EnhancedSecurityEraseTime;
        /// <summary>Word 91 Current advanced power management value</summary>
        public ushort CurrentAPM;

        /// <summary>Word 92 Master password revision code</summary>
        public ushort MasterPasswordRevisionCode;
        /// <summary>Word 93 Hardware reset result</summary>
        public ushort HardwareResetResult;

        /// <summary>Word 94 bits 7 to 0 Current AAM value</summary>
        public byte CurrentAAM;
        /// <summary>Word 94 bits 15 to 8 Vendor's recommended AAM value</summary>
        public byte RecommendedAAM;

        /// <summary>Word 95 Stream minimum request size</summary>
        public ushort StreamMinReqSize;
        /// <summary>Word 96 Streaming transfer time in DMA</summary>
        public ushort StreamTransferTimeDMA;
        /// <summary>Word 97 Streaming access latency in DMA and PIO</summary>
        public ushort StreamAccessLatency;
        /// <summary>Words 98 to 99 Streaming performance granularity</summary>
        public uint StreamPerformanceGranularity;

        /// <summary>Words 100 to 103 48-bit LBA addressable sectors</summary>
        public ulong LBA48Sectors;

        /// <summary>Word 104 Streaming transfer time in PIO</summary>
        public ushort StreamTransferTimePIO;

        /// <summary>Word 105 Maximum number of 512-byte block per DATA SET MANAGEMENT command</summary>
        public ushort DataSetMgmtSize;

        /// <summary>
        ///     Word 106 Bit 15 should be zero Bit 14 should be one Bit 13 set indicates device has multiple logical sectors
        ///     per physical sector Bit 12 set indicates logical sector has more than 256 words (512 bytes) Bits 11 to 4 are
        ///     reserved Bits 3 to 0 indicate power of two of logical sectors per physical sector
        /// </summary>
        public ushort PhysLogSectorSize;

        /// <summary>Word 107 Interseek delay for ISO-7779 acoustic testing, in microseconds</summary>
        public ushort InterseekDelay;

        /// <summary>Words 108 to 111 World Wide Name</summary>
        public ulong WWN;

        /// <summary>Words 112 to 115 Reserved for WWN extension to 128 bit</summary>
        public ulong WWNExtension;

        /// <summary>Word 116 Reserved for technical report</summary>
        public ushort ReservedWord116;

        /// <summary>Words 117 to 118 Words per logical sector</summary>
        public uint LogicalSectorWords;

        /// <summary>Word 119 Supported command/feature sets</summary>
        public CommandSetBit4 CommandSet4;
        /// <summary>Word 120 Supported command/feature sets</summary>
        public CommandSetBit4 EnabledCommandSet4;

        /// <summary>Words 121 to 125 Reserved</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public ushort[] ReservedWords121;

        /// <summary>Word 126 ATAPI byte count limit</summary>
        public ushort ATAPIByteCount;

        /// <summary>
        ///     Word 127 Removable Media Status Notification feature set support Bits 15 to 2 are reserved Bits 1 to 0 must be
        ///     0 for not supported or 1 for supported. 2 and 3 are reserved. Obsoleted in ATA8-ACS
        /// </summary>
        public ushort RemovableStatusSet;

        /// <summary>Word 128 Security status</summary>
        public SecurityStatusBit SecurityStatus;

        /// <summary>Words 129 to 159</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 31)]
        public ushort[] ReservedWords129;

        /// <summary>
        ///     Word 160 CFA power mode Bit 15 must be set Bit 13 indicates mode 1 is required for one or more commands Bit 12
        ///     indicates mode 1 is disabled Bits 11 to 0 indicates maximum current in mA
        /// </summary>
        public ushort CFAPowerMode;

        /// <summary>Words 161 to 167 Reserved for CFA</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public ushort[] ReservedCFA;

        /// <summary>Word 168 Bits 15 to 4, reserved Bits 3 to 0, device nominal form factor</summary>
        public DeviceFormFactorEnum DeviceFormFactor;
        /// <summary>Word 169 DATA SET MANAGEMENT support</summary>
        public DataSetMgmtBit DataSetMgmt;
        /// <summary>Words 170 to 173 Additional product identifier</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string AdditionalPID;

        /// <summary>Word 174 Reserved</summary>
        public ushort ReservedWord174;
        /// <summary>Word 175 Reserved</summary>
        public ushort ReservedWord175;

        /// <summary>Words 176 to 195 Current media serial number</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string MediaSerial;
        /// <summary>Words 196 to 205 Current media manufacturer</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string MediaManufacturer;

        /// <summary>Word 206 SCT Command Transport features</summary>
        public SCTCommandTransportBit SCTCommandTransport;

        /// <summary>Word 207 Reserved for CE-ATA</summary>
        public ushort ReservedCEATAWord207;
        /// <summary>Word 208 Reserved for CE-ATA</summary>
        public ushort ReservedCEATAWord208;

        /// <summary>
        ///     Word 209 Alignment of logical block within a larger physical block Bit 15 shall be cleared to zero Bit 14
        ///     shall be set to one Bits 13 to 0 indicate logical sector offset within the first physical sector
        /// </summary>
        public ushort LogicalAlignment;

        /// <summary>Words 210 to 211 Write/Read/Verify sector count mode 3 only</summary>
        public uint WRVSectorCountMode3;
        /// <summary>Words 212 to 213 Write/Read/Verify sector count mode 2 only</summary>
        public uint WRVSectorCountMode2;

        /// <summary>
        ///     Word 214 NV Cache capabilities Bits 15 to 12 feature set version Bits 11 to 18 power mode feature set version
        ///     Bits 7 to 5 reserved Bit 4 feature set enabled Bits 3 to 2 reserved Bit 1 power mode feature set enabled Bit 0
        ///     power mode feature set supported
        /// </summary>
        public ushort NVCacheCaps;
        /// <summary>Words 215 to 216 NV Cache Size in Logical BLocks</summary>
        public uint NVCacheSize;
        /// <summary>Word 217 Nominal media rotation rate In ACS-1 meant NV Cache read speed in MB/s</summary>
        public ushort NominalRotationRate;
        /// <summary>Word 218 NV Cache write speed in MB/s Reserved since ACS-2</summary>
        public ushort NVCacheWriteSpeed;
        /// <summary>Word 219 bits 7 to 0 Estimated device spin up in seconds</summary>
        public byte NVEstimatedSpinUp;
        /// <summary>Word 219 bits 15 to 8 NV Cache reserved</summary>
        public byte NVReserved;

        /// <summary>Word 220 bits 7 to 0 Write/Read/Verify feature set current mode</summary>
        public byte WRVMode;
        /// <summary>Word 220 bits 15 to 8 Reserved</summary>
        public byte WRVReserved;

        /// <summary>Word 221 Reserved</summary>
        public ushort ReservedWord221;

        /// <summary>
        ///     Word 222 Transport major revision number Bits 15 to 12 indicate transport type. 0 parallel, 1 serial, 0xE
        ///     PCIe. Bits 11 to 0 indicate revision
        /// </summary>
        public ushort TransportMajorVersion;
        /// <summary>Word 223 Transport minor revision number</summary>
        public ushort TransportMinorVersion;

        /// <summary>Words 224 to 229 Reserved for CE-ATA</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public ushort[] ReservedCEATA224;

        /// <summary>Words 230 to 233 48-bit LBA if Word 69 bit 3 is set</summary>
        public ulong ExtendedUserSectors;

        /// <summary>Word 234 Minimum number of 512 byte units per DOWNLOAD MICROCODE mode 3</summary>
        public ushort MinDownloadMicroMode3;
        /// <summary>Word 235 Maximum number of 512 byte units per DOWNLOAD MICROCODE mode 3</summary>
        public ushort MaxDownloadMicroMode3;

        /// <summary>Words 236 to 254</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)]
        public ushort[] ReservedWords;

        /// <summary>Word 255 bits 7 to 0 Should be 0xA5</summary>
        public byte Signature;
        /// <summary>Word 255 bits 15 to 8 Checksum</summary>
        public byte Checksum;
    }

    /// <summary>Decodes a raw IDENTIFY DEVICE response</summary>
    /// <param name="IdentifyDeviceResponse">Raw IDENTIFY DEVICE response</param>
    /// <returns>Decoded IDENTIFY DEVICE</returns>
    public static IdentifyDevice? Decode(byte[] IdentifyDeviceResponse)
    {
        if(IdentifyDeviceResponse == null)
            return null;

        if(IdentifyDeviceResponse.Length != 512)
        {
            AaruConsole.DebugWriteLine("ATA/ATAPI IDENTIFY decoder",
                                       "IDENTIFY response is different than 512 bytes, not decoding.");

            return null;
        }

        IdentifyDevice ATAID = Marshal.ByteArrayToStructureLittleEndian<IdentifyDevice>(IdentifyDeviceResponse);

        ATAID.WWN          = DescrambleWWN(ATAID.WWN);
        ATAID.WWNExtension = DescrambleWWN(ATAID.WWNExtension);

        ATAID.SerialNumber      = DescrambleATAString(IdentifyDeviceResponse, 10  * 2, 20);
        ATAID.FirmwareRevision  = DescrambleATAString(IdentifyDeviceResponse, 23  * 2, 8);
        ATAID.Model             = DescrambleATAString(IdentifyDeviceResponse, 27  * 2, 40);
        ATAID.AdditionalPID     = DescrambleATAString(IdentifyDeviceResponse, 170 * 2, 8);
        ATAID.MediaSerial       = DescrambleATAString(IdentifyDeviceResponse, 176 * 2, 40);
        ATAID.MediaManufacturer = DescrambleATAString(IdentifyDeviceResponse, 196 * 2, 20);

        return ATAID;
    }

    /// <summary>Encodes a raw IDENTIFY DEVICE response</summary>
    /// <param name="identify">Decoded IDENTIFY DEVICE</param>
    /// <returns>Raw IDENTIFY DEVICE response</returns>
    public static byte[] Encode(IdentifyDevice? identify)
    {
        if(identify is null)
            return null;

        IdentifyDevice ataId = identify.Value;

        ataId.WWN          = DescrambleWWN(ataId.WWN);
        ataId.WWNExtension = DescrambleWWN(ataId.WWNExtension);

        var    buf = new byte[512];
        IntPtr ptr = System.Runtime.InteropServices.Marshal.AllocHGlobal(512);
        System.Runtime.InteropServices.Marshal.StructureToPtr(ataId, ptr, false);
        System.Runtime.InteropServices.Marshal.Copy(ptr, buf, 0, 512);
        System.Runtime.InteropServices.Marshal.FreeHGlobal(ptr);

        byte[] str = ScrambleATAString(ataId.SerialNumber, 20);
        Array.Copy(str, 0, buf, 10 * 2, 20);
        str = ScrambleATAString(ataId.FirmwareRevision, 8);
        Array.Copy(str, 0, buf, 23 * 2, 8);
        str = ScrambleATAString(ataId.Model, 40);
        Array.Copy(str, 0, buf, 27 * 2, 40);
        str = ScrambleATAString(ataId.AdditionalPID, 8);
        Array.Copy(str, 0, buf, 170 * 2, 8);
        str = ScrambleATAString(ataId.MediaSerial, 40);
        Array.Copy(str, 0, buf, 176 * 2, 40);
        str = ScrambleATAString(ataId.MediaManufacturer, 20);
        Array.Copy(str, 0, buf, 196 * 2, 20);

        return buf;
    }

    static ulong DescrambleWWN(ulong WWN)
    {
        byte[] qwb   = BitConverter.GetBytes(WWN);
        var    qword = new byte[8];

        qword[7] = qwb[1];
        qword[6] = qwb[0];
        qword[5] = qwb[3];
        qword[4] = qwb[2];
        qword[3] = qwb[5];
        qword[2] = qwb[4];
        qword[1] = qwb[7];
        qword[0] = qwb[6];

        return BitConverter.ToUInt64(qword, 0);
    }

    static string DescrambleATAString(IList<byte> buffer, int offset, int length)
    {
        byte[] outbuf = buffer[offset + length - 1] != 0x00 ? new byte[length + 1] : new byte[length];

        for(var i = 0; i < length; i += 2)
        {
            outbuf[i] = buffer[offset + i                  + 1];
            outbuf[i                  + 1] = buffer[offset + i];
        }

        string outStr = StringHandlers.CToString(outbuf);

        return outStr.Trim();
    }

    static byte[] ScrambleATAString(string str, int length)
    {
        var buf = new byte[length];

        for(var i = 0; i < length; i++)
            buf[i] = 0x20;

        if(str is null)
            return buf;

        byte[] bytes = Encoding.ASCII.GetBytes(str);

        if(bytes.Length % 2 != 0)
        {
            var tmp = new byte[bytes.Length + 1];
            tmp[^1] = 0x20;
            Array.Copy(bytes, 0, tmp, 0, bytes.Length);
            bytes = tmp;
        }

        for(var i = 0; i < bytes.Length; i += 2)
        {
            buf[i] = bytes[i + 1];
            buf[i            + 1] = bytes[i];
        }

        return buf;
    }
}