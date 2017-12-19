// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Identify.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes ATA IDENTIFY DEVICE response.
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
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.Console;

namespace DiscImageChef.Decoders.ATA
{
    /// <summary>
    /// Information from following standards:
    /// T10-791D rev. 4c (ATA)
    /// T10-948D rev. 4c (ATA-2)
    /// T13-1153D rev. 18 (ATA/ATAPI-4)
    /// T13-1321D rev. 3 (ATA/ATAPI-5)
    /// T13-1410D rev. 3b (ATA/ATAPI-6)
    /// T13-1532D rev. 4b (ATA/ATAPI-7)
    /// T13-1699D rev. 3f (ATA8-ACS)
    /// T13-1699D rev. 4a (ATA8-ACS)
    /// T13-2015D rev. 2 (ACS-2)
    /// T13-2161D rev. 5 (ACS-3)
    /// CF+ &amp; CF Specification rev. 1.4 (CFA)
    /// </summary>
    public static class Identify
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
        public struct IdentifyDevice
        {
            /// <summary>
            /// Word 0
            /// General device configuration
            /// On ATAPI devices:
            /// Bits 12 to 8 indicate device type as SCSI defined
            /// Bits 6 to 5:
            /// 0 = Device shall set DRQ within 3 ms of receiving PACKET
            /// 1 = Device shall assert INTRQ when DRQ is set to one
            /// 2 = Device shall set DRQ within 50 µs of receiving PACKET
            /// Bits 1 to 0:
            /// 0 = 12 byte command packet
            /// 1 = 16 byte command packet
            /// CompactFlash is 0x848A (non magnetic, removable, not MFM, hardsector, and UltraFAST)
            /// </summary>
            public GeneralConfigurationBit GeneralConfiguration;
            /// <summary>
            /// Word 1
            /// Cylinders in default translation mode
            /// Obsoleted in ATA/ATAPI-6
            /// </summary>
            public ushort Cylinders;
            /// <summary>
            /// Word 2
            /// Specific configuration
            /// </summary>
            public SpecificConfigurationEnum SpecificConfiguration;
            /// <summary>
            /// Word 3
            /// Heads in default translation mode
            /// Obsoleted in ATA/ATAPI-6
            /// </summary>
            public ushort Heads;
            /// <summary>
            /// Word 4
            /// Unformatted bytes per track in default translation mode
            /// Obsoleted in ATA-2
            /// </summary>
            public ushort UnformattedBPT;
            /// <summary>
            /// Word 5
            /// Unformatted bytes per sector in default translation mode
            /// Obsoleted in ATA-2
            /// </summary>
            public ushort UnformattedBPS;
            /// <summary>
            /// Word 6
            /// Sectors per track in default translation mode
            /// Obsoleted in ATA/ATAPI-6
            /// </summary>
            public ushort SectorsPerTrack;
            /// <summary>
            /// Words 7 to 8
            /// CFA: Number of sectors per card
            /// </summary>
            public uint SectorsPerCard;
            /// <summary>
            /// Word 9
            /// Vendor unique
            /// Obsoleted in ATA/ATAPI-4
            /// </summary>
            public ushort VendorWord9;
            /// <summary>
            /// Words 10 to 19
            /// Device serial number, right justified, padded with spaces
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string SerialNumber;
            /// <summary>
            /// Word 20
            /// Manufacturer defined
            /// Obsoleted in ATA-2
            /// 0x0001 = single ported single sector buffer
            /// 0x0002 = dual ported multi sector buffer
            /// 0x0003 = dual ported multi sector buffer with reading
            /// </summary>
            public ushort BufferType;
            /// <summary>
            /// Word 21
            /// Size of buffer in 512 byte increments
            /// Obsoleted in ATA-2
            /// </summary>
            public ushort BufferSize;
            /// <summary>
            /// Word 22
            /// Bytes of ECC available in READ/WRITE LONG commands
            /// Obsoleted in ATA/ATAPI-4
            /// </summary>
            public ushort EccBytes;
            /// <summary>
            /// Words 23 to 26
            /// Firmware revision, left justified, padded with spaces
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
            public string FirmwareRevision;
            /// <summary>
            /// Words 27 to 46
            /// Model number, left justified, padded with spaces
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
            public string Model;
            /// <summary>
            /// Word 47 bits 7 to 0
            /// Maximum number of sectors that can be transferred per
            /// interrupt on read and write multiple commands
            /// </summary>
            public byte MultipleMaxSectors;
            /// <summary>
            /// Word 47 bits 15 to 8
            /// Vendor unique
            /// ATA/ATAPI-4 says it must be 0x80
            /// </summary>
            public byte VendorWord47;
            /// <summary>
            /// Word 48
            /// ATA-1: Set to 1 if it can perform doubleword I/O
            /// ATA-2 to ATA/ATAPI-7: Reserved
            /// ATA8-ACS: Trusted Computing feature set
            /// </summary>
            public TrustedComputingBit TrustedComputing;
            /// <summary>
            /// Word 49
            /// Capabilities
            /// </summary>
            public CapabilitiesBit Capabilities;
            /// <summary>
            /// Word 50
            /// Capabilities
            /// </summary>
            public CapabilitiesBit2 Capabilities2;
            /// <summary>
            /// Word 51 bits 7 to 0
            /// Vendor unique
            /// Obsoleted in ATA/ATAPI-4
            /// </summary>
            public byte VendorWord51;
            /// <summary>
            /// Word 51 bits 15 to 8
            /// Transfer timing mode in PIO
            /// Obsoleted in ATA/ATAPI-4
            /// </summary>
            public byte PIOTransferTimingMode;
            /// <summary>
            /// Word 52 bits 7 to 0
            /// Vendor unique
            /// Obsoleted in ATA/ATAPI-4
            /// </summary>
            public byte VendorWord52;
            /// <summary>
            /// Word 52 bits 15 to 8
            /// Transfer timing mode in DMA
            /// Obsoleted in ATA/ATAPI-4
            /// </summary>
            public byte DMATransferTimingMode;
            /// <summary>
            /// Word 53 bits 7 to 0
            /// Reports if words 54 to 58 are valid
            /// </summary>
            public ExtendedIdentifyBit ExtendedIdentify;
            /// <summary>
            /// Word 53 bits 15 to 8
            /// Free-fall Control Sensitivity
            /// </summary>
            public byte FreeFallSensitivity;
            /// <summary>
            /// Word 54
            /// Cylinders in current translation mode
            /// Obsoleted in ATA/ATAPI-6
            /// </summary>
            public ushort CurrentCylinders;
            /// <summary>
            /// Word 55
            /// Heads in current translation mode
            /// Obsoleted in ATA/ATAPI-6
            /// </summary>
            public ushort CurrentHeads;
            /// <summary>
            /// Word 56
            /// Sectors per track in current translation mode
            /// Obsoleted in ATA/ATAPI-6
            /// </summary>
            public ushort CurrentSectorsPerTrack;
            /// <summary>
            /// Words 57 to 58
            /// Total sectors currently user-addressable
            /// Obsoleted in ATA/ATAPI-6
            /// </summary>
            public uint CurrentSectors;
            /// <summary>
            /// Word 59 bits 7 to 0
            /// Number of sectors currently set to transfer on a READ/WRITE MULTIPLE command
            /// </summary>
            public byte MultipleSectorNumber;
            /// <summary>
            /// Word 59 bits 15 to 8
            /// Indicates if <see cref="MultipleSectorNumber"/> is valid
            /// </summary>
            public CapabilitiesBit3 Capabilities3;
            /// <summary>
            /// Words 60 to 61
            /// If drive supports LBA, how many sectors are addressable using LBA
            /// </summary>
            public uint LBASectors;
            /// <summary>
            /// Word 62 bits 7 to 0
            /// Single word DMA modes available
            /// Obsoleted in ATA/ATAPI-4
            /// In ATAPI it's not obsolete, indicates UDMA mode (UDMA7 is instead MDMA0)
            /// </summary>
            public TransferMode DMASupported;
            /// <summary>
            /// Word 62 bits 15 to 8
            /// Single word DMA mode currently active
            /// Obsoleted in ATA/ATAPI-4
            /// In ATAPI it's not obsolete, bits 0 and 1 indicate MDMA mode+1,
            /// bit 10 indicates DMA is supported and bit 15 indicates DMADIR bit
            /// in PACKET is required for DMA transfers
            /// </summary>
            public TransferMode DMAActive;
            /// <summary>
            /// Word 63 bits 7 to 0
            /// Multiword DMA modes available
            /// </summary>
            public TransferMode MDMASupported;
            /// <summary>
            /// Word 63 bits 15 to 8
            /// Multiword DMA mode currently active
            /// </summary>
            public TransferMode MDMAActive;

            /// <summary>
            /// Word 64 bits 7 to 0
            /// Supported Advanced PIO transfer modes
            /// </summary>
            public TransferMode APIOSupported;
            /// <summary>
            /// Word 64 bits 15 to 8
            /// Reserved
            /// </summary>
            public byte ReservedWord64;
            /// <summary>
            /// Word 65
            /// Minimum MDMA transfer cycle time per word in nanoseconds
            /// </summary>
            public ushort MinMDMACycleTime;
            /// <summary>
            /// Word 66
            /// Recommended MDMA transfer cycle time per word in nanoseconds
            /// </summary>
            public ushort RecMDMACycleTime;
            /// <summary>
            /// Word 67
            /// Minimum PIO transfer cycle time without flow control in nanoseconds
            /// </summary>
            public ushort MinPIOCycleTimeNoFlow;
            /// <summary>
            /// Word 68
            /// Minimum PIO transfer cycle time with IORDY flow control in nanoseconds
            /// </summary>
            public ushort MinPIOCycleTimeFlow;

            /// <summary>
            /// Word 69
            /// Additional supported
            /// </summary>
            public CommandSetBit5 CommandSet5;
            /// <summary>
            /// Word 70
            /// Reserved
            /// </summary>
            public ushort ReservedWord70;
            /// <summary>
            /// Word 71
            /// ATAPI: Typical time in ns from receipt of PACKET to release bus
            /// </summary>
            public ushort PacketBusRelease;
            /// <summary>
            /// Word 72
            /// ATAPI: Typical time in ns from receipt of SERVICE to clear BSY
            /// </summary>
            public ushort ServiceBusyClear;
            /// <summary>
            /// Word 73
            /// Reserved
            /// </summary>
            public ushort ReservedWord73;
            /// <summary>
            /// Word 74
            /// Reserved
            /// </summary>
            public ushort ReservedWord74;

            /// <summary>
            /// Word 75
            /// Maximum Queue depth
            /// </summary>
            public ushort MaxQueueDepth;

            /// <summary>
            /// Word 76
            /// Serial ATA Capabilities
            /// </summary>
            public SATACapabilitiesBit SATACapabilities;
            /// <summary>
            /// Word 77
            /// Serial ATA Additional Capabilities
            /// </summary>
            public SATACapabilitiesBit2 SATACapabilities2;

            /// <summary>
            /// Word 78
            /// Supported Serial ATA features
            /// </summary>
            public SATAFeaturesBit SATAFeatures;
            /// <summary>
            /// Word 79
            /// Enabled Serial ATA features
            /// </summary>
            public SATAFeaturesBit EnabledSATAFeatures;

            /// <summary>
            /// Word 80
            /// Major version of ATA/ATAPI standard supported
            /// </summary>
            public MajorVersionBit MajorVersion;
            /// <summary>
            /// Word 81
            /// Minimum version of ATA/ATAPI standard supported
            /// </summary>
            public ushort MinorVersion;

            /// <summary>
            /// Word 82
            /// Supported command/feature sets
            /// </summary>
            public CommandSetBit CommandSet;
            /// <summary>
            /// Word 83
            /// Supported command/feature sets
            /// </summary>
            public CommandSetBit2 CommandSet2;
            /// <summary>
            /// Word 84
            /// Supported command/feature sets
            /// </summary>
            public CommandSetBit3 CommandSet3;

            /// <summary>
            /// Word 85
            /// Enabled command/feature sets
            /// </summary>
            public CommandSetBit EnabledCommandSet;
            /// <summary>
            /// Word 86
            /// Enabled command/feature sets
            /// </summary>
            public CommandSetBit2 EnabledCommandSet2;
            /// <summary>
            /// Word 87
            /// Enabled command/feature sets
            /// </summary>
            public CommandSetBit3 EnabledCommandSet3;

            /// <summary>
            /// Word 88 bits 7 to 0
            /// Supported Ultra DMA transfer modes
            /// </summary>
            public TransferMode UDMASupported;
            /// <summary>
            /// Word 88 bits 15 to 8
            /// Selected Ultra DMA transfer modes
            /// </summary>
            public TransferMode UDMAActive;

            /// <summary>
            /// Word 89
            /// Time required for security erase completion
            /// </summary>
            public ushort SecurityEraseTime;
            /// <summary>
            /// Word 90
            /// Time required for enhanced security erase completion
            /// </summary>
            public ushort EnhancedSecurityEraseTime;
            /// <summary>
            /// Word 91
            /// Current advanced power management value
            /// </summary>
            public ushort CurrentAPM;

            /// <summary>
            /// Word 92
            /// Master password revision code
            /// </summary>
            public ushort MasterPasswordRevisionCode;
            /// <summary>
            /// Word 93
            /// Hardware reset result
            /// </summary>
            public ushort HardwareResetResult;

            /// <summary>
            /// Word 94 bits 7 to 0
            /// Current AAM value
            /// </summary>
            public byte CurrentAAM;
            /// <summary>
            /// Word 94 bits 15 to 8
            /// Vendor's recommended AAM value
            /// </summary>
            public byte RecommendedAAM;

            /// <summary>
            /// Word 95
            /// Stream minimum request size
            /// </summary>
            public ushort StreamMinReqSize;
            /// <summary>
            /// Word 96
            /// Streaming transfer time in DMA
            /// </summary>
            public ushort StreamTransferTimeDMA;
            /// <summary>
            /// Word 97
            /// Streaming access latency in DMA and PIO
            /// </summary>
            public ushort StreamAccessLatency;
            /// <summary>
            /// Words 98 to 99
            /// Streaming performance granularity
            /// </summary>
            public uint StreamPerformanceGranularity;

            /// <summary>
            /// Words 100 to 103
            /// 48-bit LBA addressable sectors
            /// </summary>
            public ulong LBA48Sectors;

            /// <summary>
            /// Word 104
            /// Streaming transfer time in PIO
            /// </summary>
            public ushort StreamTransferTimePIO;

            /// <summary>
            /// Word 105
            /// Maximum number of 512-byte block per DATA SET MANAGEMENT command
            /// </summary>
            public ushort DataSetMgmtSize;

            /// <summary>
            /// Word 106
            /// Bit 15 should be zero
            /// Bit 14 should be one
            /// Bit 13 set indicates device has multiple logical sectors per physical sector
            /// Bit 12 set indicates logical sector has more than 256 words (512 bytes)
            /// Bits 11 to 4 are reserved
            /// Bits 3 to 0 indicate power of two of logical sectors per physical sector
            /// </summary>
            public ushort PhysLogSectorSize;

            /// <summary>
            /// Word 107
            /// Interseek delay for ISO-7779 acoustic testing, in microseconds
            /// </summary>
            public ushort InterseekDelay;

            /// <summary>
            /// Words 108 to 111
            /// World Wide Name
            /// </summary>
            public ulong WWN;

            /// <summary>
            /// Words 112 to 115
            /// Reserved for WWN extension to 128 bit
            /// </summary>
            public ulong WWNExtension;

            /// <summary>
            /// Word 116
            /// Reserved for technical report
            /// </summary>
            public ushort ReservedWord116;

            /// <summary>
            /// Words 117 to 118
            /// Words per logical sector
            /// </summary>
            public uint LogicalSectorWords;

            /// <summary>
            /// Word 119
            /// Supported command/feature sets
            /// </summary>
            public CommandSetBit4 CommandSet4;
            /// <summary>
            /// Word 120
            /// Supported command/feature sets
            /// </summary>
            public CommandSetBit4 EnabledCommandSet4;

            /// <summary>
            /// Words 121 to 125
            /// Reserved
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public ushort[] ReservedWords121;

            /// <summary>
            /// Word 126
            /// ATAPI byte count limit
            /// </summary>
            public ushort ATAPIByteCount;

            /// <summary>
            /// Word 127
            /// Removable Media Status Notification feature set support
            /// Bits 15 to 2 are reserved
            /// Bits 1 to 0 must be 0 for not supported or 1 for supported. 2 and 3 are reserved.
            /// Obsoleted in ATA8-ACS
            /// </summary>
            public ushort RemovableStatusSet;

            /// <summary>
            /// Word 128
            /// Security status
            /// </summary>
            public SecurityStatusBit SecurityStatus;

            /// <summary>
            /// Words 129 to 159
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 31)]
            public ushort[] ReservedWords129;

            /// <summary>
            /// Word 160
            /// CFA power mode
            /// Bit 15 must be set
            /// Bit 13 indicates mode 1 is required for one or more commands
            /// Bit 12 indicates mode 1 is disabled
            /// Bits 11 to 0 indicates maximum current in mA
            /// </summary>
            public ushort CFAPowerMode;

            /// <summary>
            /// Words 161 to 167
            /// Reserved for CFA
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            public ushort[] ReservedCFA;

            /// <summary>
            /// Word 168
            /// Bits 15 to 4, reserved
            /// Bits 3 to 0, device nominal form factor
            /// </summary>
            public DeviceFormFactorEnum DeviceFormFactor;
            /// <summary>
            /// Word 169
            /// DATA SET MANAGEMENT support
            /// </summary>
            public DataSetMgmtBit DataSetMgmt;
            /// <summary>
            /// Words 170 to 173
            /// Additional product identifier
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
            public string AdditionalPID;

            /// <summary>
            /// Word 174
            /// Reserved
            /// </summary>
            public ushort ReservedWord174;
            /// <summary>
            /// Word 175
            /// Reserved
            /// </summary>
            public ushort ReservedWord175;

            /// <summary>
            /// Words 176 to 195
            /// Current media serial number
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
            public string MediaSerial;
            /// <summary>
            /// Words 196 to 205
            /// Current media manufacturer
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string MediaManufacturer;

            /// <summary>
            /// Word 206
            /// SCT Command Transport features
            /// </summary>
            public SCTCommandTransportBit SCTCommandTransport;

            /// <summary>
            /// Word 207
            /// Reserved for CE-ATA
            /// </summary>
            public ushort ReservedCEATAWord207;
            /// <summary>
            /// Word 208
            /// Reserved for CE-ATA
            /// </summary>
            public ushort ReservedCEATAWord208;

            /// <summary>
            /// Word 209
            /// Alignment of logical block within a larger physical block
            /// Bit 15 shall be cleared to zero
            /// Bit 14 shall be set to one
            /// Bits 13 to 0 indicate logical sector offset within the first physical sector
            /// </summary>
            public ushort LogicalAlignment;

            /// <summary>
            /// Words 210 to 211
            /// Write/Read/Verify sector count mode 3 only
            /// </summary>
            public uint WRVSectorCountMode3;
            /// <summary>
            /// Words 212 to 213
            /// Write/Read/Verify sector count mode 2 only
            /// </summary>
            public uint WRVSectorCountMode2;

            /// <summary>
            /// Word 214
            /// NV Cache capabilities
            /// Bits 15 to 12 feature set version
            /// Bits 11 to 18 power mode feature set version
            /// Bits 7 to 5 reserved
            /// Bit 4 feature set enabled
            /// Bits 3 to 2 reserved
            /// Bit 1 power mode feature set enabled
            /// Bit 0 power mode feature set supported
            /// </summary>
            public ushort NVCacheCaps;
            /// <summary>
            /// Words 215 to 216
            /// NV Cache Size in Logical BLocks
            /// </summary>
            public uint NVCacheSize;
            /// <summary>
            /// Word 217
            /// Nominal media rotation rate
            /// In ACS-1 meant NV Cache read speed in MB/s
            /// </summary>
            public ushort NominalRotationRate;
            /// <summary>
            /// Word 218
            /// NV Cache write speed in MB/s
            /// Reserved since ACS-2
            /// </summary>
            public ushort NVCacheWriteSpeed;
            /// <summary>
            /// Word 219 bits 7 to 0
            /// Estimated device spin up in seconds
            /// </summary>
            public byte NVEstimatedSpinUp;
            /// <summary>
            /// Word 219 bits 15 to 8
            /// NV Cache reserved
            /// </summary>
            public byte NVReserved;

            /// <summary>
            /// Word 220 bits 7 to 0
            /// Write/Read/Verify feature set current mode
            /// </summary>
            public byte WRVMode;
            /// <summary>
            /// Word 220 bits 15 to 8
            /// Reserved
            /// </summary>
            public byte WRVReserved;

            /// <summary>
            /// Word 221
            /// Reserved
            /// </summary>
            public ushort ReservedWord221;

            /// <summary>
            /// Word 222
            /// Transport major revision number
            /// Bits 15 to 12 indicate transport type. 0 parallel, 1 serial, 0xE PCIe.
            /// Bits 11 to 0 indicate revision
            /// </summary>
            public ushort TransportMajorVersion;
            /// <summary>
            /// Word 223
            /// Transport minor revision number
            /// </summary>
            public ushort TransportMinorVersion;

            /// <summary>
            /// Words 224 to 229
            /// Reserved for CE-ATA
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public ushort[] ReservedCEATA224;

            /// <summary>
            /// Words 230 to 233
            /// 48-bit LBA if Word 69 bit 3 is set
            /// </summary>
            public ulong ExtendedUserSectors;

            /// <summary>
            /// Word 234
            /// Minimum number of 512 byte units per DOWNLOAD MICROCODE mode 3
            /// </summary>
            public ushort MinDownloadMicroMode3;
            /// <summary>
            /// Word 235
            /// Maximum number of 512 byte units per DOWNLOAD MICROCODE mode 3
            /// </summary>
            public ushort MaxDownloadMicroMode3;

            /// <summary>
            /// Words 236 to 254
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)]
            public ushort[] ReservedWords;

            /// <summary>
            /// Word 255 bits 7 to 0
            /// Should be 0xA5
            /// </summary>
            public byte Signature;
            /// <summary>
            /// Word 255 bits 15 to 8
            /// Checksum
            /// </summary>
            public byte Checksum;
        }

        /// <summary>
        /// General configuration flag bits.
        /// </summary>
        [Flags]
        public enum GeneralConfigurationBit : ushort
        {
            /// <summary>
            /// Set on ATAPI
            /// </summary>
            NonMagnetic = 0x8000,
            /// <summary>
            /// Format speed tolerance gap is required
            /// Obsoleted in ATA-2
            /// </summary>
            FormatGapReq = 0x4000,
            /// <summary>
            /// Track offset option is available
            /// Obsoleted in ATA-2
            /// </summary>
            TrackOffset = 0x2000,
            /// <summary>
            /// Data strobe offset option is available
            /// Obsoleted in ATA-2
            /// </summary>
            DataStrobeOffset = 0x1000,
            /// <summary>
            /// Rotational speed tolerance is higher than 0,5%
            /// Obsoleted in ATA-2
            /// </summary>
            RotationalSpeedTolerance = 0x0800,
            /// <summary>
            /// Disk transfer rate is &gt; 10 Mb/s
            /// Obsoleted in ATA-2
            /// </summary>
            UltraFastIDE = 0x0400,
            /// <summary>
            /// Disk transfer rate is  &gt; 5 Mb/s but &lt;= 10 Mb/s 
            /// Obsoleted in ATA-2
            /// </summary>
            FastIDE = 0x0200,
            /// <summary>
            /// Disk transfer rate is &lt;= 5 Mb/s
            /// Obsoleted in ATA-2
            /// </summary>
            SlowIDE = 0x0100,
            /// <summary>
            /// Drive uses removable media
            /// </summary>
            Removable = 0x0080,
            /// <summary>
            /// Drive is fixed
            /// Obsoleted in ATA/ATAPI-6
            /// </summary>
            Fixed = 0x0040,
            /// <summary>
            /// Spindle motor control is implemented
            /// Obsoleted in ATA-2
            /// </summary>
            SpindleControl = 0x0020,
            /// <summary>
            /// Head switch time is bigger than 15 µsec.
            /// Obsoleted in ATA-2
            /// </summary>
            HighHeadSwitch = 0x0010,
            /// <summary>
            /// Drive is not MFM encoded
            /// Obsoleted in ATA-2
            /// </summary>
            NotMFM = 0x0008,
            /// <summary>
            /// Drive is soft sectored
            /// Obsoleted in ATA-2
            /// </summary>
            SoftSector = 0x0004,
            /// <summary>
            /// Response incomplete
            /// Since ATA/ATAPI-5
            /// </summary>
            IncompleteResponse = 0x0004,
            /// <summary>
            /// Drive is hard sectored
            /// Obsoleted in ATA-2
            /// </summary>
            HardSector = 0x0002,
            /// <summary>
            /// Reserved
            /// </summary>
            Reserved = 0x0001
        }

        /// <summary>
        /// Capabilities flag bits.
        /// </summary>
        [Flags]
        public enum CapabilitiesBit : ushort
        {
            /// <summary>
            /// ATAPI: Interleaved DMA supported
            /// </summary>
            InterleavedDMA = 0x8000,
            /// <summary>
            /// ATAPI: Command queueing supported
            /// </summary>
            CommandQueue = 0x4000,
            /// <summary>
            /// Standby timer values are standard
            /// </summary>
            StandardStanbyTimer = 0x2000,
            /// <summary>
            /// ATAPI: Overlap operation supported
            /// </summary>
            OverlapOperation = 0x2000,
            /// <summary>
            /// ATAPI: ATA software reset required
            /// Obsoleted in ATA/ATAPI-4
            /// </summary>
            RequiresATASoftReset = 0x1000,
            /// <summary>
            /// IORDY is supported
            /// </summary>
            IORDY = 0x0800,
            /// <summary>
            /// IORDY can be disabled
            /// </summary>
            CanDisableIORDY = 0x0400,
            /// <summary>
            /// LBA is supported
            /// </summary>
            LBASupport = 0x0200,
            /// <summary>
            /// DMA is supported
            /// </summary>
            DMASupport = 0x0100,
            /// <summary>
            /// Vendor unique
            /// Obsoleted in ATA/ATAPI-4
            /// </summary>
            VendorBit7 = 0x0080,
            /// <summary>
            /// Vendor unique
            /// Obsoleted in ATA/ATAPI-4
            /// </summary>
            VendorBit6 = 0x0040,
            /// <summary>
            /// Vendor unique
            /// Obsoleted in ATA/ATAPI-4
            /// </summary>
            VendorBit5 = 0x0020,
            /// <summary>
            /// Vendor unique
            /// Obsoleted in ATA/ATAPI-4
            /// </summary>
            VendorBit4 = 0x0010,
            /// <summary>
            /// Vendor unique
            /// Obsoleted in ATA/ATAPI-4
            /// </summary>
            VendorBit3 = 0x0008,
            /// <summary>
            /// Vendor unique
            /// Obsoleted in ATA/ATAPI-4
            /// </summary>
            VendorBit2 = 0x0004,
            /// <summary>
            /// Long Physical Alignment setting bit 1
            /// </summary>
            PhysicalAlignment1 = 0x0002,
            /// <summary>
            /// Long Physical Alignment setting bit 0
            /// </summary>
            PhysicalAlignment0 = 0x0001
        }

        /// <summary>
        /// Extended identify flag bits.
        /// </summary>
        [Flags]
        public enum ExtendedIdentifyBit : byte
        {
            /// <summary>
            /// Reserved
            /// </summary>
            Reserved07 = 0x80,
            /// <summary>
            /// Reserved
            /// </summary>
            Reserved06 = 0x40,
            /// <summary>
            /// Reserved
            /// </summary>
            Reserved05 = 0x20,
            /// <summary>
            /// Reserved
            /// </summary>
            Reserved04 = 0x10,
            /// <summary>
            /// Reserved
            /// </summary>
            Reserved03 = 0x08,
            /// <summary>
            /// Identify word 88 is valid
            /// </summary>
            Word88Valid = 0x04,
            /// <summary>
            /// Identify words 64 to 70 are valid
            /// </summary>
            Words64to70Valid = 0x02,
            /// <summary>
            /// Identify words 54 to 58 are valid
            /// </summary>
            Words54to58Valid = 0x01
        }

        /// <summary>
        /// More capabilities flag bits.
        /// </summary>
        [Flags]
        public enum CapabilitiesBit2 : ushort
        {
            /// <summary>
            /// MUST NOT be set
            /// </summary>
            MustBeClear = 0x8000,
            /// <summary>
            /// MUST be set
            /// </summary>
            MustBeSet = 0x4000,
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
            /// <summary>
            /// Indicates a device specific minimum standby timer value
            /// </summary>
            SpecificStandbyTimer = 0x0001,
        }

        [Flags]
        public enum TransferMode : byte
        {
            Mode7 = 0x80,
            Mode6 = 0x40,
            Mode5 = 0x20,
            Mode4 = 0x10,
            Mode3 = 0x08,
            Mode2 = 0x04,
            Mode1 = 0x02,
            Mode0 = 0x01
        }

        /// <summary>
        /// More capabilities flag bits.
        /// </summary>
        [Flags]
        public enum CommandSetBit : ushort
        {
            /// <summary>
            /// Already obsolete in ATA/ATAPI-4, reserved in ATA3
            /// </summary>
            Obsolete15 = 0x8000,
            /// <summary>
            /// NOP is supported
            /// </summary>
            Nop = 0x4000,
            /// <summary>
            /// READ BUFFER is supported
            /// </summary>
            ReadBuffer = 0x2000,
            /// <summary>
            /// WRITE BUFFER is supported
            /// </summary>
            WriteBuffer = 0x1000,
            /// <summary>
            /// Already obsolete in ATA/ATAPI-4, reserved in ATA3
            /// </summary>
            Obsolete11 = 0x0800,
            /// <summary>
            /// Host Protected Area is supported
            /// </summary>
            HPA = 0x0400,
            /// <summary>
            /// DEVICE RESET is supported
            /// </summary>
            DeviceReset = 0x0200,
            /// <summary>
            /// SERVICE interrupt is supported
            /// </summary>
            Service = 0x0100,
            /// <summary>
            /// Release is supported
            /// </summary>
            Release = 0x0080,
            /// <summary>
            /// Look-ahead is supported
            /// </summary>
            LookAhead = 0x0040,
            /// <summary>
            /// Write cache is supported
            /// </summary>
            WriteCache = 0x0020,
            /// <summary>
            /// PACKET command set is supported
            /// </summary>
            Packet = 0x0010,
            /// <summary>
            /// Power Management feature set is supported
            /// </summary>
            PowerManagement = 0x0008,
            /// <summary>
            /// Removable Media feature set is supported
            /// </summary>
            RemovableMedia = 0x0004,
            /// <summary>
            /// Security Mode feature set is supported
            /// </summary>
            SecurityMode = 0x0002,
            /// <summary>
            /// SMART feature set is supported
            /// </summary>
            SMART = 0x0001,
        }

        /// <summary>
        /// More capabilities flag bits.
        /// </summary>
        [Flags]
        public enum CommandSetBit2 : ushort
        {
            /// <summary>
            /// MUST NOT be set
            /// </summary>
            MustBeClear = 0x8000,
            /// <summary>
            /// MUST BE SET
            /// </summary>
            MustBeSet = 0x4000,
            /// <summary>
            /// FLUSH CACHE EXT supported
            /// </summary>
            FlushCacheExt = 0x2000,
            /// <summary>
            /// FLUSH CACHE supported
            /// </summary>
            FlushCache = 0x1000,
            /// <summary>
            /// Device Configuration Overlay feature set supported
            /// </summary>
            DCO = 0x0800,
            /// <summary>
            /// 48-bit LBA supported
            /// </summary>
            LBA48 = 0x0400,
            /// <summary>
            /// Automatic Acoustic Management supported
            /// </summary>
            AAM = 0x0200,
            /// <summary>
            /// SET MAX security extension supported
            /// </summary>
            SetMax = 0x0100,
            /// <summary>
            /// Address Offset Reserved Area Boot NCITS TR27:2001
            /// </summary>
            AddressOffsetReservedAreaBoot = 0x0080,
            /// <summary>
            /// SET FEATURES required to spin-up
            /// </summary>
            SetFeaturesRequired = 0x0040,
            /// <summary>
            /// Power-Up in standby feature set supported
            /// </summary>
            PowerUpInStandby = 0x0020,
            /// <summary>
            /// Removable Media Status Notification feature set is supported
            /// </summary>
            RemovableNotification = 0x0010,
            /// <summary>
            /// Advanced Power Management feature set is supported
            /// </summary>
            APM = 0x0008,
            /// <summary>
            /// Compact Flash feature set is supported
            /// </summary>
            CompactFlash = 0x0004,
            /// <summary>
            /// READ DMA QUEUED and WRITE DMA QUEUED are supported
            /// </summary>
            RWQueuedDMA = 0x0002,
            /// <summary>
            /// DOWNLOAD MICROCODE is supported
            /// </summary>
            DownloadMicrocode = 0x0001,
        }

        /// <summary>
        /// More capabilities flag bits.
        /// </summary>
        [Flags]
        public enum CommandSetBit3 : ushort
        {
            /// <summary>
            /// MUST NOT be set
            /// </summary>
            MustBeClear = 0x8000,
            /// <summary>
            /// MUST BE SET
            /// </summary>
            MustBeSet = 0x4000,
            /// <summary>
            /// IDLE IMMEDIATE with UNLOAD FEATURE is supported
            /// </summary>
            IdleImmediate = 0x2000,
            /// <summary>
            /// Reserved for INCITS TR-37/2004
            /// </summary>
            Reserved12 = 0x1000,
            /// <summary>
            /// Reserved for INCITS TR-37/2004
            /// </summary>
            Reserved11 = 0x0800,
            /// <summary>
            /// URG bit is supported in WRITE STREAM DMA EXT and WRITE STREAM EXT
            /// </summary>
            WriteURG = 0x0400,
            /// <summary>
            /// URG bit is supported in READ STREAM DMA EXT and READ STREAM EXT
            /// </summary>
            ReadURG = 0x0200,
            /// <summary>
            /// 64-bit World Wide Name is supported
            /// </summary>
            WWN = 0x0100,
            /// <summary>
            /// WRITE DMA QUEUED FUA EXT is supported
            /// </summary>
            FUAWriteQ = 0x0080,
            /// <summary>
            /// WRITE DMA FUA EXT and WRITE MULTIPLE FUA EXT are supported
            /// </summary>
            FUAWrite = 0x0040,
            /// <summary>
            /// General Purpose Logging feature supported
            /// </summary>
            GPL = 0x0020,
            /// <summary>
            /// Sstreaming feature set is supported
            /// </summary>
            Streaming = 0x0010,
            /// <summary>
            /// Media Card Pass Through command set supported
            /// </summary>
            MCPT = 0x0008,
            /// <summary>
            /// Media serial number supported
            /// </summary>
            MediaSerial = 0x0004,
            /// <summary>
            /// SMART self-test supported
            /// </summary>
            SMARTSelfTest = 0x0002,
            /// <summary>
            /// SMART error logging supported
            /// </summary>
            SMARTLog = 0x0001,
        }

        /// <summary>
        /// More capabilities flag bits.
        /// </summary>
        [Flags]
        public enum SecurityStatusBit : ushort
        {
            Reserved15 = 0x8000,
            Reserved14 = 0x4000,
            Reserved13 = 0x2000,
            Reserved12 = 0x1000,
            Reserved11 = 0x0800,
            Reserved10 = 0x0400,
            Reserved09 = 0x0200,
            /// <summary>
            /// Maximum security level
            /// </summary>
            Maximum = 0x0100,
            Reserved07 = 0x0080,
            Reserved06 = 0x0040,
            /// <summary>
            /// Supports enhanced security erase
            /// </summary>
            Enhanced = 0x0020,
            /// <summary>
            /// Security count expired
            /// </summary>
            Expired = 0x0010,
            /// <summary>
            /// Security frozen
            /// </summary>
            Frozen = 0x0008,
            /// <summary>
            /// Security locked
            /// </summary>
            Locked = 0x0004,
            /// <summary>
            /// Security enabled
            /// </summary>
            Enabled = 0x0002,
            /// <summary>
            /// Security supported
            /// </summary>
            Supported = 0x0001,
        }

        /// <summary>
        /// Word 80
        /// Major version
        /// </summary>
        [Flags]
        public enum MajorVersionBit : ushort
        {
            Reserved15 = 0x8000,
            Reserved14 = 0x4000,
            Reserved13 = 0x2000,
            Reserved12 = 0x1000,
            /// <summary>
            /// ACS-4
            /// </summary>
            ACS4 = 0x0800,
            /// <summary>
            /// ACS-3
            /// </summary>
            ACS3 = 0x0400,
            /// <summary>
            /// ACS-2
            /// </summary>
            ACS2 = 0x0200,
            /// <summary>
            /// ATA8-ACS
            /// </summary>
            Ata8ACS = 0x0100,
            /// <summary>
            /// ATA/ATAPI-7
            /// </summary>
            AtaAtapi7 = 0x0080,
            /// <summary>
            /// ATA/ATAPI-6
            /// </summary>
            AtaAtapi6 = 0x0040,
            /// <summary>
            /// ATA/ATAPI-5
            /// </summary>
            AtaAtapi5 = 0x0020,
            /// <summary>
            /// ATA/ATAPI-4
            /// </summary>
            AtaAtapi4 = 0x0010,
            /// <summary>
            /// ATA-3
            /// </summary>
            Ata3 = 0x0008,
            /// <summary>
            /// ATA-2
            /// </summary>
            Ata2 = 0x0004,
            /// <summary>
            /// ATA-1
            /// </summary>
            Ata1 = 0x0002,
            Reserved00 = 0x0001,
        }

        public enum SpecificConfigurationEnum : ushort
        {
            /// <summary>
            /// Device requires SET FEATURES to spin up and
            /// IDENTIFY DEVICE response is incomplete
            /// </summary>
            RequiresSetIncompleteResponse = 0x37C8,
            /// <summary>
            /// Device requires SET FEATURES to spin up and
            /// IDENTIFY DEVICE response is complete
            /// </summary>
            RequiresSetCompleteResponse = 0x738C,
            /// <summary>
            /// Device does not requires SET FEATURES to spin up and
            /// IDENTIFY DEVICE response is incomplete
            /// </summary>
            NotRequiresSetIncompleteResponse = 0x8C73,
            /// <summary>
            /// Device does not requires SET FEATURES to spin up and
            /// IDENTIFY DEVICE response is complete
            /// </summary>
            NotRequiresSetCompleteResponse = 0xC837
        }

        [Flags]
        public enum TrustedComputingBit : ushort
        {
            /// <summary>
            /// MUST NOT be set
            /// </summary>
            Clear = 0x8000,
            /// <summary>
            /// MUST be set
            /// </summary>
            Set = 0x4000,
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
            /// <summary>
            /// Trusted Computing feature set is supported
            /// </summary>
            TrustedComputing = 0x0001,
        }

        /// <summary>
        /// More capabilities flag bits.
        /// </summary>
        [Flags]
        public enum CommandSetBit4 : ushort
        {
            /// <summary>
            /// MUST NOT be set
            /// </summary>
            MustBeClear = 0x8000,
            /// <summary>
            /// MUST be set
            /// </summary>
            MustBeSet = 0x4000,
            Reserved13 = 0x2000,
            Reserved12 = 0x1000,
            Reserved11 = 0x0800,
            Reserved10 = 0x0400,
            /// <summary>
            /// DSN feature set is supported
            /// </summary>
            DSN = 0x0200,
            /// <summary>
            /// Accessible Max Address Configuration is supported
            /// </summary>
            AMAC = 0x0100,
            /// <summary>
            /// Extended Power Conditions is supported
            /// </summary>
            ExtPowerCond = 0x0080,
            /// <summary>
            /// Extended Status Reporting is supported
            /// </summary>
            ExtStatusReport = 0x0040,
            /// <summary>
            /// Free-fall Control feature set is supported
            /// </summary>
            FreeFallControl = 0x0020,
            /// <summary>
            /// Supports segmented feature in DOWNLOAD MICROCODE
            /// </summary>
            SegmentedDownloadMicrocode = 0x0010,
            /// <summary>
            /// READ/WRITE DMA EXT GPL are supported
            /// </summary>
            RWDMAExtGpl = 0x0008,
            /// <summary>
            /// WRITE UNCORRECTABLE is supported
            /// </summary>
            WriteUnc = 0x0004,
            /// <summary>
            /// Write/Read/Verify is supported
            /// </summary>
            WRV = 0x0002,
            /// <summary>
            /// Reserved for DT1825
            /// </summary>
            DT1825 = 0x0001,
        }

        [Flags]
        public enum SCTCommandTransportBit : ushort
        {
            Vendor15 = 0x8000,
            Vendor14 = 0x4000,
            Vendor13 = 0x2000,
            Vendor12 = 0x1000,
            Reserved11 = 0x0800,
            Reserved10 = 0x0400,
            Reserved09 = 0x0200,
            Reserved08 = 0x0100,
            Reserved07 = 0x0080,
            Reserved06 = 0x0040,
            /// <summary>
            /// SCT Command Transport Data Tables supported
            /// </summary>
            DataTables = 0x0020,
            /// <summary>
            /// SCT Command Transport Features Control supported
            /// </summary>
            FeaturesControl = 0x0010,
            /// <summary>
            /// SCT Command Transport Error Recovery Control supported
            /// </summary>
            ErrorRecoveryControl = 0x0008,
            /// <summary>
            /// SCT Command Transport Write Same supported
            /// </summary>
            WriteSame = 0x0004,
            /// <summary>
            /// SCT Command Transport Long Sector Address supported
            /// </summary>
            LongSectorAccess = 0x0002,
            /// <summary>
            /// SCT Command Transport supported
            /// </summary>
            Supported = 0x0001,
        }

        [Flags]
        public enum SATACapabilitiesBit : ushort
        {
            /// <summary>
            /// Supports READ LOG DMA EXT
            /// </summary>
            ReadLogDMAExt = 0x8000,
            /// <summary>
            /// Supports device automatic partial to slumber transitions
            /// </summary>
            DevSlumbTrans = 0x4000,
            /// <summary>
            /// Supports host automatic partial to slumber transitions
            /// </summary>
            HostSlumbTrans = 0x2000,
            /// <summary>
            /// Supports NCQ priroty
            /// </summary>
            NCQPriority = 0x1000,
            /// <summary>
            /// Supports unload while NCQ commands are outstanding
            /// </summary>
            UnloadNCQ = 0x0800,
            /// <summary>
            /// Supports PHY Event Counters
            /// </summary>
            PHYEventCounter = 0x0400,
            /// <summary>
            /// Supports receipt of host initiated power management requests
            /// </summary>
            PowerReceipt = 0x0200,
            /// <summary>
            /// Supports NCQ
            /// </summary>
            NCQ = 0x0100,
            Reserved07 = 0x0080,
            Reserved06 = 0x0040,
            Reserved05 = 0x0020,
            Reserved04 = 0x0010,
            /// <summary>
            /// Supports SATA Gen. 3 Signaling Speed (6.0Gb/s)
            /// </summary>
            Gen3Speed = 0x0008,
            /// <summary>
            /// Supports SATA Gen. 2 Signaling Speed (3.0Gb/s)
            /// </summary>
            Gen2Speed = 0x0004,
            /// <summary>
            /// Supports SATA Gen. 1 Signaling Speed (1.5Gb/s)
            /// </summary>
            Gen1Speed = 0x0002,
            /// <summary>
            /// MUST NOT be set
            /// </summary>
            Clear = 0x0001,
        }

        [Flags]
        public enum SATAFeaturesBit : ushort
        {
            Reserved15 = 0x8000,
            Reserved14 = 0x4000,
            Reserved13 = 0x2000,
            Reserved12 = 0x1000,
            Reserved11 = 0x0800,
            Reserved10 = 0x0400,
            Reserved09 = 0x0200,
            Reserved08 = 0x0100,
            /// <summary>
            /// Supports NCQ autosense
            /// </summary>
            NCQAutoSense = 0x0080,
            /// <summary>
            /// Automatic Partial to Slumber transitions are enabled
            /// </summary>
            EnabledSlumber = 0x0080,
            /// <summary>
            /// Supports Software Settings Preservation
            /// </summary>
            SettingsPreserve = 0x0040,
            /// <summary>
            /// Supports hardware feature control
            /// </summary>
            HardwareFeatureControl = 0x0020,
            /// <summary>
            /// ATAPI: Asynchronous notification
            /// </summary>
            AsyncNotification = 0x0020,
            /// <summary>
            /// Supports in-order data delivery
            /// </summary>
            InOrderData = 0x0010,
            /// <summary>
            /// Supports initiating power management
            /// </summary>
            InitPowerMgmt = 0x0008,
            /// <summary>
            /// Supports DMA Setup auto-activation
            /// </summary>
            DMASetup = 0x0004,
            /// <summary>
            /// Supports non-zero buffer offsets
            /// </summary>
            NonZeroBufferOffset = 0x0002,
            /// <summary>
            /// MUST NOT be set
            /// </summary>
            Clear = 0x0001,
        }

        [Flags]
        public enum CapabilitiesBit3 : byte
        {
            /// <summary>
            /// BLOCK ERASE EXT supported
            /// </summary>
            BlockErase = 0x0080,
            /// <summary>
            /// OVERWRITE EXT supported
            /// </summary>
            Overwrite = 0x0040,
            /// <summary>
            /// CRYPTO SCRAMBLE EXT supported
            /// </summary>
            CryptoScramble = 0x0020,
            /// <summary>
            /// Sanitize feature set is supported
            /// </summary>
            Sanitize = 0x0010,
            /// <summary>
            /// If unset, sanitize commands are specified by ACS-2
            /// </summary>
            SanitizeCommands = 0x0008,
            /// <summary>
            /// SANITIZE ANTIFREEZE LOCK EXT is supported
            /// </summary>
            SanitizeAntifreeze = 0x0004,
            Reserved01 = 0x0002,
            /// <summary>
            /// Multiple logical sector setting is valid
            /// </summary>
            MultipleValid = 0x0001,
        }

        [Flags]
        public enum CommandSetBit5 : ushort
        {
            /// <summary>
            /// Supports CFast Specification
            /// </summary>
            CFast = 0x8000,
            /// <summary>
            /// Deterministic read after TRIM is supported
            /// </summary>
            DeterministicTrim = 0x4000,
            /// <summary>
            /// Long physical sector alignment error reporting control is supported
            /// </summary>
            LongPhysSectorAligError = 0x2000,
            /// <summary>
            /// DEVICE CONFIGURATION IDENTIFY DMA and DEVICE CONFIGURATION SET DMA are supported
            /// </summary>
            DeviceConfDMA = 0x1000,
            /// <summary>
            /// READ BUFFER DMA is supported
            /// </summary>
            ReadBufferDMA = 0x0800,
            /// <summary>
            /// WRITE BUFFER DMA is supported
            /// </summary>
            WriteBufferDMA = 0x0400,
            /// <summary>
            /// SET PASSWORD DMA and SET UNLOCK DMA are supported
            /// </summary>
            SetMaxDMA = 0x0200,
            /// <summary>
            /// DOWNLOAD MICROCODE DMA is supported
            /// </summary>
            DownloadMicroCodeDMA = 0x0100,
            /// <summary>
            /// Reserved for IEEE-1667
            /// </summary>
            IEEE1667 = 0x0080,
            /// <summary>
            /// Optional ATA 28-bit commands are supported
            /// </summary>
            Ata28 = 0x0040,
            /// <summary>
            /// Read zero after TRIM is supported
            /// </summary>
            ReadZeroTrim = 0x0020,
            /// <summary>
            /// Device encrypts all user data
            /// </summary>
            Encrypted = 0x0010,
            /// <summary>
            /// Extended number of user addressable sectors is supported
            /// </summary>
            ExtSectors = 0x0008,
            /// <summary>
            /// All write cache is non-volatile
            /// </summary>
            AllCacheNV = 0x0004,
            /// <summary>
            /// Zoned capabilities bit 1
            /// </summary>
            ZonedBit1 = 0x0002,
            /// <summary>
            /// Zoned capabilities bit 0
            /// </summary>
            ZonedBit0 = 0x0001,
        }

        public enum DeviceFormFactorEnum : ushort
        {
            /// <summary>
            /// Size not reported
            /// </summary>
            NotReported = 0,
            /// <summary>
            /// 5.25"
            /// </summary>
            FiveAndQuarter = 1,
            /// <summary>
            /// 3.5"
            /// </summary>
            ThreeAndHalf = 2,
            /// <summary>
            /// 2.5"
            /// </summary>
            TwoAndHalf = 3,
            /// <summary>
            /// 1.8"
            /// </summary>
            OnePointEight = 4,
            /// <summary>
            /// Less than 1.8"
            /// </summary>
            LessThanOnePointEight = 5
        }

        [Flags]
        public enum DataSetMgmtBit : ushort
        {
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
            /// <summary>
            /// TRIM is suported
            /// </summary>
            Trim = 0x0001,
        }

        [Flags]
        public enum SATACapabilitiesBit2 : ushort
        {
            Reserved15 = 0x8000,
            Reserved14 = 0x4000,
            Reserved13 = 0x2000,
            Reserved12 = 0x1000,
            Reserved11 = 0x0800,
            Reserved10 = 0x0400,
            Reserved09 = 0x0200,
            Reserved08 = 0x0100,
            Reserved07 = 0x0080,
            /// <summary>
            /// Supports RECEIVE FPDMA QUEUED and SEND FPDMA QUEUED
            /// </summary>
            FPDMAQ = 0x0040,
            /// <summary>
            /// Supports NCQ Queue Management
            /// </summary>
            NCQMgmt = 0x0020,
            /// <summary>
            /// ATAPI: Supports host environment detect
            /// </summary>
            HostEnvDetect = 0x0020,
            /// <summary>
            /// Supports NCQ streaming
            /// </summary>
            NCQStream = 0x0010,
            /// <summary>
            /// ATAPI: Supports device attention on slimline connected devices
            /// </summary>
            DevAttSlimline = 0x0010,
            /// <summary>
            /// Coded value indicating current negotiated Serial ATA signal speed
            /// </summary>
            CurrentSpeedBit2 = 0x0008,
            /// <summary>
            /// Coded value indicating current negotiated Serial ATA signal speed
            /// </summary>
            CurrentSpeedBit1 = 0x0004,
            /// <summary>
            /// Coded value indicating current negotiated Serial ATA signal speed
            /// </summary>
            CurrentSpeedBit0 = 0x0002,
            /// <summary>
            /// MUST NOT be set
            /// </summary>
            Clear = 0x0001,
        }

        public static IdentifyDevice? Decode(byte[] IdentifyDeviceResponse)
        {
            if(IdentifyDeviceResponse == null)
                return null;

            if(IdentifyDeviceResponse.Length != 512)
            {
                DicConsole.DebugWriteLine("ATA/ATAPI IDENTIFY decoder", "IDENTIFY response is different than 512 bytes, not decoding.");
                return null;
            }

            IntPtr ptr = Marshal.AllocHGlobal(512);
            Marshal.Copy(IdentifyDeviceResponse, 0, ptr, 512);
            IdentifyDevice ATAID = (IdentifyDevice)Marshal.PtrToStructure(ptr, typeof(IdentifyDevice));
            Marshal.FreeHGlobal(ptr);

            ATAID.WWN = DescrambleWWN(ATAID.WWN);
            ATAID.WWNExtension = DescrambleWWN(ATAID.WWNExtension);

            ATAID.SerialNumber = DescrambleATAString(IdentifyDeviceResponse, 10 * 2, 20);
            ATAID.FirmwareRevision = DescrambleATAString(IdentifyDeviceResponse, 23 * 2, 8);
            ATAID.Model = DescrambleATAString(IdentifyDeviceResponse, 27 * 2, 40);
            ATAID.AdditionalPID = DescrambleATAString(IdentifyDeviceResponse, 170 * 2, 8);
            ATAID.MediaSerial = DescrambleATAString(IdentifyDeviceResponse, 176 * 2, 40);
            ATAID.MediaManufacturer = DescrambleATAString(IdentifyDeviceResponse, 196 * 2, 20);

            return ATAID;
        }

        public static string Prettify(byte[] IdentifyDeviceResponse)
        {
            if(IdentifyDeviceResponse.Length != 512)
                return null;

            IdentifyDevice? decoded = Decode(IdentifyDeviceResponse);
            return Prettify(decoded);
        }

        public static string Prettify(IdentifyDevice? IdentifyDeviceResponse)
        {
            if(IdentifyDeviceResponse == null)
                return null;

            StringBuilder sb = new StringBuilder();

            bool atapi = false;
            bool cfa = false;

            IdentifyDevice ATAID = IdentifyDeviceResponse.Value;
            if(ATAID.GeneralConfiguration.HasFlag(GeneralConfigurationBit.NonMagnetic))
            {
                if((ushort)ATAID.GeneralConfiguration != 0x848A)
                {
                    //if (ATAID.CommandSet.HasFlag(CommandSetBit.Packet))
                    //{
                    atapi = true;
                    cfa = false;
                    //}
                }
                else
                {
                    atapi = false;
                    cfa = true;
                }
            }

            if(atapi && !cfa)
                sb.AppendLine("ATAPI device");
            else if(!atapi && cfa)
                sb.AppendLine("CompactFlash device");
            else
                sb.AppendLine("ATA device");

            if(ATAID.Model != "")
                sb.AppendFormat("Model: {0}", ATAID.Model).AppendLine();
            if(ATAID.FirmwareRevision != "")
                sb.AppendFormat("Firmware revision: {0}", ATAID.FirmwareRevision).AppendLine();
            if(ATAID.SerialNumber != "")
                sb.AppendFormat("Serial #: {0}", ATAID.SerialNumber).AppendLine();
            if(ATAID.AdditionalPID != "")
                sb.AppendFormat("Additional product ID: {0}", ATAID.AdditionalPID).AppendLine();

            if(ATAID.CommandSet3.HasFlag(CommandSetBit3.MustBeSet) &&
                !ATAID.CommandSet3.HasFlag(CommandSetBit3.MustBeClear))
            {
                if(ATAID.EnabledCommandSet3.HasFlag(CommandSetBit3.MediaSerial))
                {
                    if(ATAID.MediaManufacturer != "")
                        sb.AppendFormat("Media manufacturer: {0}", ATAID.MediaManufacturer).AppendLine();
                    if(ATAID.MediaSerial != "")
                        sb.AppendFormat("Media serial #: {0}", ATAID.MediaSerial).AppendLine();
                }

                if(ATAID.EnabledCommandSet3.HasFlag(CommandSetBit3.WWN))
                {
                    sb.AppendFormat("World Wide Name: {0:X16}", ATAID.WWN).AppendLine();
                }
            }

            bool ata1 = false, ata2 = false, ata3 = false, ata4 = false, ata5 = false, ata6 = false, ata7 = false, acs = false, acs2 = false, acs3 = false, acs4 = false;

            if((ushort)ATAID.MajorVersion == 0x0000 || (ushort)ATAID.MajorVersion == 0xFFFF)
            {
                // Obsolete in ATA-2, if present, device supports ATA-1
                ata1 |= ATAID.GeneralConfiguration.HasFlag(GeneralConfigurationBit.FastIDE) ||
                ATAID.GeneralConfiguration.HasFlag(GeneralConfigurationBit.SlowIDE) ||
                ATAID.GeneralConfiguration.HasFlag(GeneralConfigurationBit.UltraFastIDE);

                ata2 |= ATAID.ExtendedIdentify.HasFlag(ExtendedIdentifyBit.Words64to70Valid);

                if(!ata1 && !ata2 && !atapi && !cfa)
                    ata2 = true;

                ata4 |= atapi;
                ata3 |= cfa;

                if(cfa && ata1)
                    ata1 = false;
                if(cfa && ata2)
                    ata2 = false;

                ata5 |= ATAID.Signature == 0xA5;
            }
            else
            {
                ata1 |= ATAID.MajorVersion.HasFlag(MajorVersionBit.Ata1);
                ata2 |= ATAID.MajorVersion.HasFlag(MajorVersionBit.Ata2);
                ata3 |= ATAID.MajorVersion.HasFlag(MajorVersionBit.Ata3);
                ata4 |= ATAID.MajorVersion.HasFlag(MajorVersionBit.AtaAtapi4);
                ata5 |= ATAID.MajorVersion.HasFlag(MajorVersionBit.AtaAtapi5);
                ata6 |= ATAID.MajorVersion.HasFlag(MajorVersionBit.AtaAtapi6);
                ata7 |= ATAID.MajorVersion.HasFlag(MajorVersionBit.AtaAtapi7);
                acs |= ATAID.MajorVersion.HasFlag(MajorVersionBit.Ata8ACS);
                acs2 |= ATAID.MajorVersion.HasFlag(MajorVersionBit.ACS2);
                acs3 |= ATAID.MajorVersion.HasFlag(MajorVersionBit.ACS3);
                acs4 |= ATAID.MajorVersion.HasFlag(MajorVersionBit.ACS4);
            }

            int maxatalevel = 0;
            int minatalevel = 255;
            sb.Append("Supported ATA versions: ");
            if(ata1)
            {
                sb.Append("ATA-1 ");
                maxatalevel = 1;
                if(minatalevel > 1)
                    minatalevel = 1;
            }
            if(ata2)
            {
                sb.Append("ATA-2 ");
                maxatalevel = 2;
                if(minatalevel > 2)
                    minatalevel = 2;
            }
            if(ata3)
            {
                sb.Append("ATA-3 ");
                maxatalevel = 3;
                if(minatalevel > 3)
                    minatalevel = 3;
            }
            if(ata4)
            {
                sb.Append("ATA/ATAPI-4 ");
                maxatalevel = 4;
                if(minatalevel > 4)
                    minatalevel = 4;
            }
            if(ata5)
            {
                sb.Append("ATA/ATAPI-5 ");
                maxatalevel = 5;
                if(minatalevel > 5)
                    minatalevel = 5;
            }
            if(ata6)
            {
                sb.Append("ATA/ATAPI-6 ");
                maxatalevel = 6;
                if(minatalevel > 6)
                    minatalevel = 6;
            }
            if(ata7)
            {
                sb.Append("ATA/ATAPI-7 ");
                maxatalevel = 7;
                if(minatalevel > 7)
                    minatalevel = 7;
            }
            if(acs)
            {
                sb.Append("ATA8-ACS ");
                maxatalevel = 8;
                if(minatalevel > 8)
                    minatalevel = 8;
            }
            if(acs2)
            {
                sb.Append("ATA8-ACS2 ");
                maxatalevel = 9;
                if(minatalevel > 9)
                    minatalevel = 9;
            }
            if(acs3)
            {
                sb.Append("ATA8-ACS3 ");
                maxatalevel = 10;
                if(minatalevel > 10)
                    minatalevel = 10;
            }
            if(acs4)
            {
                sb.Append("ATA8-ACS4 ");
                maxatalevel = 11;
                if(minatalevel > 11)
                    minatalevel = 11;
            }
            sb.AppendLine();

            sb.Append("Maximum ATA revision supported: ");
            if(maxatalevel >= 3)
            {
                switch(ATAID.MinorVersion)
                {
                    case 0x0000:
                    case 0xFFFF:
                        sb.AppendLine("Minor ATA version not specified");
                        break;
                    case 0x0001:
                        sb.AppendLine("ATA (ATA-1) X3T9.2 781D prior to revision 4");
                        break;
                    case 0x0002:
                        sb.AppendLine("ATA-1 published, ANSI X3.221-1994");
                        break;
                    case 0x0003:
                        sb.AppendLine("ATA (ATA-1) X3T9.2 781D revision 4");
                        break;
                    case 0x0004:
                        sb.AppendLine("ATA-2 published, ANSI X3.279-1996");
                        break;
                    case 0x0005:
                        sb.AppendLine("ATA-2 X3T10 948D prior to revision 2k");
                        break;
                    case 0x0006:
                        sb.AppendLine("ATA-3 X3T10 2008D revision 1");
                        break;
                    case 0x0007:
                        sb.AppendLine("ATA-2 X3T10 948D revision 2k");
                        break;
                    case 0x0008:
                        sb.AppendLine("ATA-3 X3T10 2008D revision 0");
                        break;
                    case 0x0009:
                        sb.AppendLine("ATA-2 X3T10 948D revision 3");
                        break;
                    case 0x000A:
                        sb.AppendLine("ATA-3 published, ANSI X3.298-1997");
                        break;
                    case 0x000B:
                        sb.AppendLine("ATA-3 X3T10 2008D revision 6");
                        break;
                    case 0x000C:
                        sb.AppendLine("ATA-3 X3T13 2008D revision 7");
                        break;
                    case 0x000D:
                        sb.AppendLine("ATA/ATAPI-4 X3T13 1153D revision 6");
                        break;
                    case 0x000E:
                        sb.AppendLine("ATA/ATAPI-4 T13 1153D revision 13");
                        break;
                    case 0x000F:
                        sb.AppendLine("ATA/ATAPI-4 X3T13 1153D revision 7");
                        break;
                    case 0x0010:
                        sb.AppendLine("ATA/ATAPI-4 T13 1153D revision 18");
                        break;
                    case 0x0011:
                        sb.AppendLine("ATA/ATAPI-4 T13 1153D revision 15");
                        break;
                    case 0x0012:
                        sb.AppendLine("ATA/ATAPI-4 published, ANSI INCITS 317-1998");
                        break;
                    case 0x0013:
                        sb.AppendLine("ATA/ATAPI-5 T13 1321D revision 3");
                        break;
                    case 0x0014:
                        sb.AppendLine("ATA/ATAPI-4 T13 1153D revision 14");
                        break;
                    case 0x0015:
                        sb.AppendLine("ATA/ATAPI-5 T13 1321D revision 1");
                        break;
                    case 0x0016:
                        sb.AppendLine("ATA/ATAPI-5 published, ANSI INCITS 340-2000");
                        break;
                    case 0x0017:
                        sb.AppendLine("ATA/ATAPI-4 T13 1153D revision 17");
                        break;
                    case 0x0018:
                        sb.AppendLine("ATA/ATAPI-6 T13 1410D revision 0");
                        break;
                    case 0x0019:
                        sb.AppendLine("ATA/ATAPI-6 T13 1410D revision 3a");
                        break;
                    case 0x001A:
                        sb.AppendLine("ATA/ATAPI-7 T13 1532D revision 1");
                        break;
                    case 0x001B:
                        sb.AppendLine("ATA/ATAPI-6 T13 1410D revision 2");
                        break;
                    case 0x001C:
                        sb.AppendLine("ATA/ATAPI-6 T13 1410D revision 1");
                        break;
                    case 0x001D:
                        sb.AppendLine("ATA/ATAPI-7 published ANSI INCITS 397-2005");
                        break;
                    case 0x001E:
                        sb.AppendLine("ATA/ATAPI-7 T13 1532D revision 0");
                        break;
                    case 0x001F:
                        sb.AppendLine("ACS-3 Revision 3b");
                        break;
                    case 0x0021:
                        sb.AppendLine("ATA/ATAPI-7 T13 1532D revision 4a");
                        break;
                    case 0x0022:
                        sb.AppendLine("ATA/ATAPI-6 published, ANSI INCITS 361-2002");
                        break;
                    case 0x0027:
                        sb.AppendLine("ATA8-ACS revision 3c");
                        break;
                    case 0x0028:
                        sb.AppendLine("ATA8-ACS revision 6");
                        break;
                    case 0x0029:
                        sb.AppendLine("ATA8-ACS revision 4");
                        break;
                    case 0x0031:
                        sb.AppendLine("ACS-2 Revision 2");
                        break;
                    case 0x0033:
                        sb.AppendLine("ATA8-ACS Revision 3e");
                        break;
                    case 0x0039:
                        sb.AppendLine("ATA8-ACS Revision 4c");
                        break;
                    case 0x0042:
                        sb.AppendLine("ATA8-ACS Revision 3f");
                        break;
                    case 0x0052:
                        sb.AppendLine("ATA8-ACS revision 3b");
                        break;
                    case 0x006D:
                        sb.AppendLine("ACS-3 Revision 5");
                        break;
                    case 0x0082:
                        sb.AppendLine("ACS-2 published, ANSI INCITS 482-2012");
                        break;
                    case 0x0107:
                        sb.AppendLine("ATA8-ACS revision 2d");
                        break;
                    case 0x0110:
                        sb.AppendLine("ACS-2 Revision 3");
                        break;
                    case 0x011B:
                        sb.AppendLine("ACS-3 Revision 4");
                        break;
                    default:
                        sb.AppendFormat("Unknown ATA revision 0x{0:X4}", ATAID.MinorVersion).AppendLine();
                        break;
                }
            }

            switch((ATAID.TransportMajorVersion & 0xF000) >> 12)
            {
                case 0x0:
                    sb.Append("Parallel ATA device: ");
                    if((ATAID.TransportMajorVersion & 0x0002) == 0x0002)
                        sb.Append("ATA/ATAPI-7 ");
                    if((ATAID.TransportMajorVersion & 0x0001) == 0x0001)
                        sb.Append("ATA8-APT ");
                    sb.AppendLine();
                    break;
                case 0x1:
                    sb.Append("Serial ATA device: ");
                    if((ATAID.TransportMajorVersion & 0x0001) == 0x0001)
                        sb.Append("ATA8-AST ");
                    if((ATAID.TransportMajorVersion & 0x0002) == 0x0002)
                        sb.Append("SATA 1.0a ");
                    if((ATAID.TransportMajorVersion & 0x0004) == 0x0004)
                        sb.Append("SATA II Extensions ");
                    if((ATAID.TransportMajorVersion & 0x0008) == 0x0008)
                        sb.Append("SATA 2.5 ");
                    if((ATAID.TransportMajorVersion & 0x0010) == 0x0010)
                        sb.Append("SATA 2.6 ");
                    if((ATAID.TransportMajorVersion & 0x0020) == 0x0020)
                        sb.Append("SATA 3.0 ");
                    if((ATAID.TransportMajorVersion & 0x0040) == 0x0040)
                        sb.Append("SATA 3.1 ");
                    sb.AppendLine();
                    break;
                case 0xE:
                    sb.AppendLine("SATA Express device");
                    break;
                default:
                    sb.AppendFormat("Unknown transport type 0x{0:X1}", (ATAID.TransportMajorVersion & 0xF000) >> 12).AppendLine();
                    break;
            }

            if(atapi)
            {
                // Bits 12 to 8, SCSI Peripheral Device Type
                switch((SCSI.PeripheralDeviceTypes)(((ushort)ATAID.GeneralConfiguration & 0x1F00) >> 8))
                {
                    case SCSI.PeripheralDeviceTypes.DirectAccess: //0x00,
                        sb.AppendLine("ATAPI Direct-access device");
                        break;
                    case SCSI.PeripheralDeviceTypes.SequentialAccess: //0x01,
                        sb.AppendLine("ATAPI Sequential-access device");
                        break;
                    case SCSI.PeripheralDeviceTypes.PrinterDevice: //0x02,
                        sb.AppendLine("ATAPI Printer device");
                        break;
                    case SCSI.PeripheralDeviceTypes.ProcessorDevice: //0x03,
                        sb.AppendLine("ATAPI Processor device");
                        break;
                    case SCSI.PeripheralDeviceTypes.WriteOnceDevice: //0x04,
                        sb.AppendLine("ATAPI Write-once device");
                        break;
                    case SCSI.PeripheralDeviceTypes.MultiMediaDevice: //0x05,
                        sb.AppendLine("ATAPI CD-ROM/DVD/etc device");
                        break;
                    case SCSI.PeripheralDeviceTypes.ScannerDevice: //0x06,
                        sb.AppendLine("ATAPI Scanner device");
                        break;
                    case SCSI.PeripheralDeviceTypes.OpticalDevice: //0x07,
                        sb.AppendLine("ATAPI Optical memory device");
                        break;
                    case SCSI.PeripheralDeviceTypes.MediumChangerDevice: //0x08,
                        sb.AppendLine("ATAPI Medium change device");
                        break;
                    case SCSI.PeripheralDeviceTypes.CommsDevice: //0x09,
                        sb.AppendLine("ATAPI Communications device");
                        break;
                    case SCSI.PeripheralDeviceTypes.PrePressDevice1: //0x0A,
                        sb.AppendLine("ATAPI Graphics arts pre-press device (defined in ASC IT8)");
                        break;
                    case SCSI.PeripheralDeviceTypes.PrePressDevice2: //0x0B,
                        sb.AppendLine("ATAPI Graphics arts pre-press device (defined in ASC IT8)");
                        break;
                    case SCSI.PeripheralDeviceTypes.ArrayControllerDevice: //0x0C,
                        sb.AppendLine("ATAPI Array controller device");
                        break;
                    case SCSI.PeripheralDeviceTypes.EnclosureServiceDevice: //0x0D,
                        sb.AppendLine("ATAPI Enclosure services device");
                        break;
                    case SCSI.PeripheralDeviceTypes.SimplifiedDevice: //0x0E,
                        sb.AppendLine("ATAPI Simplified direct-access device");
                        break;
                    case SCSI.PeripheralDeviceTypes.OCRWDevice: //0x0F,
                        sb.AppendLine("ATAPI Optical card reader/writer device");
                        break;
                    case SCSI.PeripheralDeviceTypes.BridgingExpander: //0x10,
                        sb.AppendLine("ATAPI Bridging Expanders");
                        break;
                    case SCSI.PeripheralDeviceTypes.ObjectDevice: //0x11,
                        sb.AppendLine("ATAPI Object-based Storage Device");
                        break;
                    case SCSI.PeripheralDeviceTypes.ADCDevice: //0x12,
                        sb.AppendLine("ATAPI Automation/Drive Interface");
                        break;
                    case SCSI.PeripheralDeviceTypes.WellKnownDevice: //0x1E,
                        sb.AppendLine("ATAPI Well known logical unit");
                        break;
                    case SCSI.PeripheralDeviceTypes.UnknownDevice: //0x1F
                        sb.AppendLine("ATAPI Unknown or no device type");
                        break;
                    default:
                        sb.AppendFormat("ATAPI Unknown device type field value 0x{0:X2}", ((ushort)ATAID.GeneralConfiguration & 0x1F00) >> 8).AppendLine();
                        break;
                }

                // ATAPI DRQ behaviour
                switch(((ushort)ATAID.GeneralConfiguration & 0x60) >> 5)
                {
                    case 0:
                        sb.AppendLine("Device shall set DRQ within 3 ms of receiving PACKET");
                        break;
                    case 1:
                        sb.AppendLine("Device shall assert INTRQ when DRQ is set to one");
                        break;
                    case 2:
                        sb.AppendLine("Device shall set DRQ within 50 µs of receiving PACKET");
                        break;
                    default:
                        sb.AppendFormat("Unknown ATAPI DRQ behaviour code {0}", ((ushort)ATAID.GeneralConfiguration & 0x60) >> 5).AppendLine();
                        break;
                }

                // ATAPI PACKET size
                switch((ushort)ATAID.GeneralConfiguration & 0x03)
                {
                    case 0:
                        sb.AppendLine("ATAPI device uses 12 byte command packet");
                        break;
                    case 1:
                        sb.AppendLine("ATAPI device uses 16 byte command packet");
                        break;
                    default:
                        sb.AppendFormat("Unknown ATAPI packet size code {0}", (ushort)ATAID.GeneralConfiguration & 0x03).AppendLine();
                        break;
                }
            }
            else if(!cfa)
            {
                if(minatalevel >= 5)
                {
                    if(ATAID.GeneralConfiguration.HasFlag(GeneralConfigurationBit.IncompleteResponse))
                        sb.AppendLine("Incomplete identify response");
                }
                if(ATAID.GeneralConfiguration.HasFlag(GeneralConfigurationBit.NonMagnetic))
                    sb.AppendLine("Device uses non-magnetic media");

                if(ATAID.GeneralConfiguration.HasFlag(GeneralConfigurationBit.Removable))
                    sb.AppendLine("Device is removable");

                if(minatalevel <= 5)
                {
                    if(ATAID.GeneralConfiguration.HasFlag(GeneralConfigurationBit.Fixed))
                        sb.AppendLine("Device is fixed");
                }

                if(ata1)
                {
                    if(ATAID.GeneralConfiguration.HasFlag(GeneralConfigurationBit.SlowIDE))
                        sb.AppendLine("Device transfer rate is <= 5 Mb/s");
                    if(ATAID.GeneralConfiguration.HasFlag(GeneralConfigurationBit.FastIDE))
                        sb.AppendLine("Device transfer rate is > 5 Mb/s but <= 10 Mb/s");
                    if(ATAID.GeneralConfiguration.HasFlag(GeneralConfigurationBit.UltraFastIDE))
                        sb.AppendLine("Device transfer rate is > 10 Mb/s");
                    if(ATAID.GeneralConfiguration.HasFlag(GeneralConfigurationBit.SoftSector))
                        sb.AppendLine("Device is soft sectored");
                    if(ATAID.GeneralConfiguration.HasFlag(GeneralConfigurationBit.HardSector))
                        sb.AppendLine("Device is hard sectored");
                    if(ATAID.GeneralConfiguration.HasFlag(GeneralConfigurationBit.NotMFM))
                        sb.AppendLine("Device is not MFM encoded");
                    if(ATAID.GeneralConfiguration.HasFlag(GeneralConfigurationBit.FormatGapReq))
                        sb.AppendLine("Format speed tolerance gap is required");
                    if(ATAID.GeneralConfiguration.HasFlag(GeneralConfigurationBit.TrackOffset))
                        sb.AppendLine("Track offset option is available");
                    if(ATAID.GeneralConfiguration.HasFlag(GeneralConfigurationBit.DataStrobeOffset))
                        sb.AppendLine("Data strobe offset option is available");
                    if(ATAID.GeneralConfiguration.HasFlag(GeneralConfigurationBit.RotationalSpeedTolerance))
                        sb.AppendLine("Rotational speed tolerance is higher than 0,5%");
                    if(ATAID.GeneralConfiguration.HasFlag(GeneralConfigurationBit.SpindleControl))
                        sb.AppendLine("Spindle motor control is implemented");
                    if(ATAID.GeneralConfiguration.HasFlag(GeneralConfigurationBit.HighHeadSwitch))
                        sb.AppendLine("Head switch time is bigger than 15 µs.");
                }
            }

            if(ATAID.NominalRotationRate != 0x0000 &&
               ATAID.NominalRotationRate != 0xFFFF)
            {
                if(ATAID.NominalRotationRate == 0x0001)
                    sb.AppendLine("Device does not rotate.");
                else
                    sb.AppendFormat("Device rotate at {0} rpm", ATAID.NominalRotationRate).AppendLine();
            }

            uint logicalsectorsize = 0;
            if(!atapi)
            {
                uint physicalsectorsize;

                if((ATAID.PhysLogSectorSize & 0x8000) == 0x0000 &&
                    (ATAID.PhysLogSectorSize & 0x4000) == 0x4000)
                {
                    if((ATAID.PhysLogSectorSize & 0x1000) == 0x1000)
                    {
                        if(ATAID.LogicalSectorWords <= 255 || ATAID.LogicalAlignment == 0xFFFF)
                            logicalsectorsize = 512;
                        else
                            logicalsectorsize = ATAID.LogicalSectorWords * 2;
                    }
                    else
                        logicalsectorsize = 512;

                    if((ATAID.PhysLogSectorSize & 0x2000) == 0x2000)
                    {
#pragma warning disable IDE0004 // Remove Unnecessary Cast
                        physicalsectorsize = logicalsectorsize * (uint)Math.Pow(2, (double)(ATAID.PhysLogSectorSize & 0xF));
#pragma warning restore IDE0004 // Remove Unnecessary Cast
                    }
                    else
                        physicalsectorsize = logicalsectorsize;
                }
                else
                {
                    logicalsectorsize = 512;
                    physicalsectorsize = 512;
                }

                sb.AppendFormat("Physical sector size: {0} bytes", physicalsectorsize).AppendLine();
                sb.AppendFormat("Logical sector size: {0} bytes", logicalsectorsize).AppendLine();

                if((logicalsectorsize != physicalsectorsize) &&
                    (ATAID.LogicalAlignment & 0x8000) == 0x0000 &&
                    (ATAID.LogicalAlignment & 0x4000) == 0x4000)
                {
                    sb.AppendFormat("Logical sector starts at offset {0} from physical sector", ATAID.LogicalAlignment & 0x3FFF).AppendLine();
                }

                if(minatalevel <= 5)
                {
                    if(ATAID.CurrentCylinders > 0 && ATAID.CurrentHeads > 0 && ATAID.CurrentSectorsPerTrack > 0)
                    {
                        sb.AppendFormat("Cylinders: {0} max., {1} current", ATAID.Cylinders, ATAID.CurrentCylinders).AppendLine();
                        sb.AppendFormat("Heads: {0} max., {1} current", ATAID.Heads, ATAID.CurrentHeads).AppendLine();
                        sb.AppendFormat("Sectors per track: {0} max., {1} current", ATAID.SectorsPerTrack, ATAID.CurrentSectorsPerTrack).AppendLine();
                        sb.AppendFormat("Sectors addressable in CHS mode: {0} max., {1} current", ATAID.Cylinders * ATAID.Heads * ATAID.SectorsPerTrack,
                            ATAID.CurrentSectors).AppendLine();
                    }
                    else
                    {
                        sb.AppendFormat("Cylinders: {0}", ATAID.Cylinders).AppendLine();
                        sb.AppendFormat("Heads: {0}", ATAID.Heads).AppendLine();
                        sb.AppendFormat("Sectors per track: {0}", ATAID.SectorsPerTrack).AppendLine();
                        sb.AppendFormat("Sectors addressable in CHS mode: {0}", ATAID.Cylinders * ATAID.Heads * ATAID.SectorsPerTrack).AppendLine();
                    }
                }

                if(ATAID.Capabilities.HasFlag(CapabilitiesBit.LBASupport))
                {
                    sb.AppendFormat("{0} sectors in 28-bit LBA mode", ATAID.LBASectors).AppendLine();
                }

                if(ATAID.CommandSet2.HasFlag(CommandSetBit2.LBA48))
                {
                    sb.AppendFormat("{0} sectors in 48-bit LBA mode", ATAID.LBA48Sectors).AppendLine();
                }

                if(minatalevel <= 5)
                {
                    if(ATAID.CurrentSectors > 0)
                        sb.AppendFormat("Device size in CHS mode: {0} bytes, {1} Mb, {2} MiB", (ulong)ATAID.CurrentSectors * logicalsectorsize,
                            ((ulong)ATAID.CurrentSectors * logicalsectorsize) / 1000 / 1000, ((ulong)ATAID.CurrentSectors * 512) / 1024 / 1024).AppendLine();
                    else
                    {
                        ulong currentSectors = (ulong)(ATAID.Cylinders * ATAID.Heads * ATAID.SectorsPerTrack);
                        sb.AppendFormat("Device size in CHS mode: {0} bytes, {1} Mb, {2} MiB", currentSectors * logicalsectorsize,
                            (currentSectors * logicalsectorsize) / 1000 / 1000, (currentSectors * 512) / 1024 / 1024).AppendLine();
                    }
                }

                if(ATAID.Capabilities.HasFlag(CapabilitiesBit.LBASupport))
                {
                    if((((ulong)ATAID.LBASectors * logicalsectorsize) / 1024 / 1024) > 1000000)
                    {
                        sb.AppendFormat("Device size in 28-bit LBA mode: {0} bytes, {1} Tb, {2} TiB", (ulong)ATAID.LBASectors * logicalsectorsize,
                            ((ulong)ATAID.LBASectors * logicalsectorsize) / 1000 / 1000 / 1000 / 1000, ((ulong)ATAID.LBASectors * 512) / 1024 / 1024 / 1024 / 1024).AppendLine();
                    }
                    else if((((ulong)ATAID.LBASectors * logicalsectorsize) / 1024 / 1024) > 1000)
                    {
                        sb.AppendFormat("Device size in 28-bit LBA mode: {0} bytes, {1} Gb, {2} GiB", (ulong)ATAID.LBASectors * logicalsectorsize,
                            ((ulong)ATAID.LBASectors * logicalsectorsize) / 1000 / 1000 / 1000, ((ulong)ATAID.LBASectors * 512) / 1024 / 1024 / 1024).AppendLine();
                    }
                    else
                    {
                        sb.AppendFormat("Device size in 28-bit LBA mode: {0} bytes, {1} Mb, {2} MiB", (ulong)ATAID.LBASectors * logicalsectorsize,
                            ((ulong)ATAID.LBASectors * logicalsectorsize) / 1000 / 1000, ((ulong)ATAID.LBASectors * 512) / 1024 / 1024).AppendLine();
                    }
                }

                if(ATAID.CommandSet2.HasFlag(CommandSetBit2.LBA48))
                {
                    if(ATAID.CommandSet5.HasFlag(CommandSetBit5.ExtSectors))
                    {
                        if(((ATAID.ExtendedUserSectors * logicalsectorsize) / 1024 / 1024) > 1000000)
                        {
                            sb.AppendFormat("Device size in 48-bit LBA mode: {0} bytes, {1} Tb, {2} TiB", ATAID.ExtendedUserSectors * logicalsectorsize,
                                (ATAID.ExtendedUserSectors * logicalsectorsize) / 1000 / 1000 / 1000 / 1000, (ATAID.ExtendedUserSectors * logicalsectorsize) / 1024 / 1024 / 1024 / 1024).AppendLine();
                        }
                        else if(((ATAID.ExtendedUserSectors * logicalsectorsize) / 1024 / 1024) > 1000)
                        {
                            sb.AppendFormat("Device size in 48-bit LBA mode: {0} bytes, {1} Gb, {2} GiB", ATAID.ExtendedUserSectors * logicalsectorsize,
                                (ATAID.ExtendedUserSectors * logicalsectorsize) / 1000 / 1000 / 1000, (ATAID.ExtendedUserSectors * logicalsectorsize) / 1024 / 1024 / 1024).AppendLine();
                        }
                        else
                        {
                            sb.AppendFormat("Device size in 48-bit LBA mode: {0} bytes, {1} Mb, {2} MiB", ATAID.ExtendedUserSectors * logicalsectorsize,
                                (ATAID.ExtendedUserSectors * logicalsectorsize) / 1000 / 1000, (ATAID.ExtendedUserSectors * logicalsectorsize) / 1024 / 1024).AppendLine();
                        }
                    }
                    else
                    {
                        if(((ATAID.LBA48Sectors * logicalsectorsize) / 1024 / 1024) > 1000000)
                        {
                            sb.AppendFormat("Device size in 48-bit LBA mode: {0} bytes, {1} Tb, {2} TiB", ATAID.LBA48Sectors * logicalsectorsize,
                                (ATAID.LBA48Sectors * logicalsectorsize) / 1000 / 1000 / 1000 / 1000, (ATAID.LBA48Sectors * logicalsectorsize) / 1024 / 1024 / 1024 / 1024).AppendLine();
                        }
                        else if(((ATAID.LBA48Sectors * logicalsectorsize) / 1024 / 1024) > 1000)
                        {
                            sb.AppendFormat("Device size in 48-bit LBA mode: {0} bytes, {1} Gb, {2} GiB", ATAID.LBA48Sectors * logicalsectorsize,
                                (ATAID.LBA48Sectors * logicalsectorsize) / 1000 / 1000 / 1000, (ATAID.LBA48Sectors * logicalsectorsize) / 1024 / 1024 / 1024).AppendLine();
                        }
                        else
                        {
                            sb.AppendFormat("Device size in 48-bit LBA mode: {0} bytes, {1} Mb, {2} MiB", ATAID.LBA48Sectors * logicalsectorsize,
                                (ATAID.LBA48Sectors * logicalsectorsize) / 1000 / 1000, (ATAID.LBA48Sectors * logicalsectorsize) / 1024 / 1024).AppendLine();
                        }
                    }
                }

                if(ata1 || cfa)
                {
                    if(cfa)
                    {
                        sb.AppendFormat("{0} sectors in card", ATAID.SectorsPerCard).AppendLine();
                    }
                    if(ATAID.UnformattedBPT > 0)
                        sb.AppendFormat("{0} bytes per unformatted track", ATAID.UnformattedBPT).AppendLine();
                    if(ATAID.UnformattedBPS > 0)
                        sb.AppendFormat("{0} bytes per unformatted sector", ATAID.UnformattedBPS).AppendLine();
                }
            }
            if((ushort)ATAID.SpecificConfiguration != 0x0000 &&
               (ushort)ATAID.SpecificConfiguration != 0xFFFF)
            {
                switch(ATAID.SpecificConfiguration)
                {
                    case SpecificConfigurationEnum.RequiresSetIncompleteResponse:
                        sb.AppendLine("Device requires SET FEATURES to spin up and IDENTIFY DEVICE response is incomplete.");
                        break;
                    case SpecificConfigurationEnum.RequiresSetCompleteResponse:
                        sb.AppendLine("Device requires SET FEATURES to spin up and IDENTIFY DEVICE response is complete.");
                        break;
                    case SpecificConfigurationEnum.NotRequiresSetIncompleteResponse:
                        sb.AppendLine("Device does not require SET FEATURES to spin up and IDENTIFY DEVICE response is incomplete.");
                        break;
                    case SpecificConfigurationEnum.NotRequiresSetCompleteResponse:
                        sb.AppendLine("Device does not require SET FEATURES to spin up and IDENTIFY DEVICE response is complete.");
                        break;
                    default:
                        sb.AppendFormat("Unknown device specific configuration 0x{0:X4}", (ushort)ATAID.SpecificConfiguration).AppendLine();
                        break;
                }
            }

            // Obsolete since ATA-2, however, it is yet used in ATA-8 devices
            if(ATAID.BufferSize != 0x0000 && ATAID.BufferSize != 0xFFFF &&
                ATAID.BufferType != 0x0000 && ATAID.BufferType != 0xFFFF)
            {
                switch(ATAID.BufferType)
                {
                    case 1:
                        sb.AppendFormat("{0} KiB of single ported single sector buffer", (ATAID.BufferSize * 512) / 1024).AppendLine();
                        break;
                    case 2:
                        sb.AppendFormat("{0} KiB of dual ported multi sector buffer", (ATAID.BufferSize * 512) / 1024).AppendLine();
                        break;
                    case 3:
                        sb.AppendFormat("{0} KiB of dual ported multi sector buffer with read caching", (ATAID.BufferSize * 512) / 1024).AppendLine();
                        break;
                    default:
                        sb.AppendFormat("{0} KiB of unknown type {1} buffer", (ATAID.BufferSize * 512) / 1024, ATAID.BufferType).AppendLine();
                        break;
                }
            }

            if(ATAID.EccBytes != 0x0000 && ATAID.EccBytes != 0xFFFF)
                sb.AppendFormat("READ/WRITE LONG has {0} extra bytes", ATAID.EccBytes).AppendLine();

            sb.AppendLine();

            sb.Append("Device capabilities:");
            if(ATAID.Capabilities.HasFlag(CapabilitiesBit.StandardStanbyTimer))
                sb.AppendLine().Append("Standby time values are standard");
            if(ATAID.Capabilities.HasFlag(CapabilitiesBit.IORDY))
            {
                sb.AppendLine().Append("IORDY is supported");
                if(ATAID.Capabilities.HasFlag(CapabilitiesBit.CanDisableIORDY))
                    sb.Append(" and can be disabled");
            }
            if(ATAID.Capabilities.HasFlag(CapabilitiesBit.DMASupport))
                sb.AppendLine().Append("DMA is supported");

            if(ATAID.Capabilities2.HasFlag(CapabilitiesBit2.MustBeSet) &&
               !ATAID.Capabilities2.HasFlag(CapabilitiesBit2.MustBeClear))
            {
                if(ATAID.Capabilities2.HasFlag(CapabilitiesBit2.SpecificStandbyTimer))
                    sb.AppendLine().Append("Device indicates a specific minimum standby timer value");
            }

            if(ATAID.Capabilities3.HasFlag(CapabilitiesBit3.MultipleValid))
            {
                sb.AppendLine().AppendFormat("A maximum of {0} sectors can be transferred per interrupt on READ/WRITE MULTIPLE", ATAID.MultipleSectorNumber);
                sb.AppendLine().AppendFormat("Device supports setting a maximum of {0} sectors", ATAID.MultipleMaxSectors);
            }

            if(ATAID.Capabilities.HasFlag(CapabilitiesBit.PhysicalAlignment1) ||
                ATAID.Capabilities.HasFlag(CapabilitiesBit.PhysicalAlignment0))
            {
                sb.AppendLine().AppendFormat("Long Physical Alignment setting is {0}", (ushort)ATAID.Capabilities & 0x03);
            }

            if(ata1)
            {
                if(ATAID.TrustedComputing.HasFlag(TrustedComputingBit.TrustedComputing))
                    sb.AppendLine().Append("Device supports doubleword I/O");
            }

            if(atapi)
            {
                if(ATAID.Capabilities.HasFlag(CapabilitiesBit.InterleavedDMA))
                    sb.AppendLine().Append("ATAPI device supports interleaved DMA");
                if(ATAID.Capabilities.HasFlag(CapabilitiesBit.CommandQueue))
                    sb.AppendLine().Append("ATAPI device supports command queueing");
                if(ATAID.Capabilities.HasFlag(CapabilitiesBit.OverlapOperation))
                    sb.AppendLine().Append("ATAPI device supports overlapped operations");
                if(ATAID.Capabilities.HasFlag(CapabilitiesBit.RequiresATASoftReset))
                    sb.AppendLine().Append("ATAPI device requires ATA software reset");
            }

            if(minatalevel <= 3)
            {
                sb.AppendLine().AppendFormat("PIO timing mode: {0}", ATAID.PIOTransferTimingMode);
                sb.AppendLine().AppendFormat("DMA timing mode: {0}", ATAID.DMATransferTimingMode);
            }

            sb.AppendLine().Append("Advanced PIO: ");
            if(ATAID.APIOSupported.HasFlag(TransferMode.Mode0))
            {
                sb.Append("PIO0 ");
            }
            if(ATAID.APIOSupported.HasFlag(TransferMode.Mode1))
            {
                sb.Append("PIO1 ");
            }
            if(ATAID.APIOSupported.HasFlag(TransferMode.Mode2))
            {
                sb.Append("PIO2 ");
            }
            if(ATAID.APIOSupported.HasFlag(TransferMode.Mode3))
            {
                sb.Append("PIO3 ");
            }
            if(ATAID.APIOSupported.HasFlag(TransferMode.Mode4))
            {
                sb.Append("PIO4 ");
            }
            if(ATAID.APIOSupported.HasFlag(TransferMode.Mode5))
            {
                sb.Append("PIO5 ");
            }
            if(ATAID.APIOSupported.HasFlag(TransferMode.Mode6))
            {
                sb.Append("PIO6 ");
            }
            if(ATAID.APIOSupported.HasFlag(TransferMode.Mode7))
            {
                sb.Append("PIO7 ");
            }

            if(minatalevel <= 3 && !atapi)
            {
                sb.AppendLine().Append("Single-word DMA: ");
                if(ATAID.DMASupported.HasFlag(TransferMode.Mode0))
                {
                    sb.Append("DMA0 ");
                    if(ATAID.DMAActive.HasFlag(TransferMode.Mode0))
                        sb.Append("(active) ");
                }
                if(ATAID.DMASupported.HasFlag(TransferMode.Mode1))
                {
                    sb.Append("DMA1 ");
                    if(ATAID.DMAActive.HasFlag(TransferMode.Mode1))
                        sb.Append("(active) ");
                }
                if(ATAID.DMASupported.HasFlag(TransferMode.Mode2))
                {
                    sb.Append("DMA2 ");
                    if(ATAID.DMAActive.HasFlag(TransferMode.Mode2))
                        sb.Append("(active) ");
                }
                if(ATAID.DMASupported.HasFlag(TransferMode.Mode3))
                {
                    sb.Append("DMA3 ");
                    if(ATAID.DMAActive.HasFlag(TransferMode.Mode3))
                        sb.Append("(active) ");
                }
                if(ATAID.DMASupported.HasFlag(TransferMode.Mode4))
                {
                    sb.Append("DMA4 ");
                    if(ATAID.DMAActive.HasFlag(TransferMode.Mode4))
                        sb.Append("(active) ");
                }
                if(ATAID.DMASupported.HasFlag(TransferMode.Mode5))
                {
                    sb.Append("DMA5 ");
                    if(ATAID.DMAActive.HasFlag(TransferMode.Mode5))
                        sb.Append("(active) ");
                }
                if(ATAID.DMASupported.HasFlag(TransferMode.Mode6))
                {
                    sb.Append("DMA6 ");
                    if(ATAID.DMAActive.HasFlag(TransferMode.Mode6))
                        sb.Append("(active) ");
                }
                if(ATAID.DMASupported.HasFlag(TransferMode.Mode7))
                {
                    sb.Append("DMA7 ");
                    if(ATAID.DMAActive.HasFlag(TransferMode.Mode7))
                        sb.Append("(active) ");
                }
            }

            sb.AppendLine().Append("Multi-word DMA: ");
            if(ATAID.MDMASupported.HasFlag(TransferMode.Mode0))
            {
                sb.Append("MDMA0 ");
                if(ATAID.MDMAActive.HasFlag(TransferMode.Mode0))
                    sb.Append("(active) ");
            }
            if(ATAID.MDMASupported.HasFlag(TransferMode.Mode1))
            {
                sb.Append("MDMA1 ");
                if(ATAID.MDMAActive.HasFlag(TransferMode.Mode1))
                    sb.Append("(active) ");
            }
            if(ATAID.MDMASupported.HasFlag(TransferMode.Mode2))
            {
                sb.Append("MDMA2 ");
                if(ATAID.MDMAActive.HasFlag(TransferMode.Mode2))
                    sb.Append("(active) ");
            }
            if(ATAID.MDMASupported.HasFlag(TransferMode.Mode3))
            {
                sb.Append("MDMA3 ");
                if(ATAID.MDMAActive.HasFlag(TransferMode.Mode3))
                    sb.Append("(active) ");
            }
            if(ATAID.MDMASupported.HasFlag(TransferMode.Mode4))
            {
                sb.Append("MDMA4 ");
                if(ATAID.MDMAActive.HasFlag(TransferMode.Mode4))
                    sb.Append("(active) ");
            }
            if(ATAID.MDMASupported.HasFlag(TransferMode.Mode5))
            {
                sb.Append("MDMA5 ");
                if(ATAID.MDMAActive.HasFlag(TransferMode.Mode5))
                    sb.Append("(active) ");
            }
            if(ATAID.MDMASupported.HasFlag(TransferMode.Mode6))
            {
                sb.Append("MDMA6 ");
                if(ATAID.MDMAActive.HasFlag(TransferMode.Mode6))
                    sb.Append("(active) ");
            }
            if(ATAID.MDMASupported.HasFlag(TransferMode.Mode7))
            {
                sb.Append("MDMA7 ");
                if(ATAID.MDMAActive.HasFlag(TransferMode.Mode7))
                    sb.Append("(active) ");
            }

            sb.AppendLine().Append("Ultra DMA: ");
            if(ATAID.UDMASupported.HasFlag(TransferMode.Mode0))
            {
                sb.Append("UDMA0 ");
                if(ATAID.UDMAActive.HasFlag(TransferMode.Mode0))
                    sb.Append("(active) ");
            }
            if(ATAID.UDMASupported.HasFlag(TransferMode.Mode1))
            {
                sb.Append("UDMA1 ");
                if(ATAID.UDMAActive.HasFlag(TransferMode.Mode1))
                    sb.Append("(active) ");
            }
            if(ATAID.UDMASupported.HasFlag(TransferMode.Mode2))
            {
                sb.Append("UDMA2 ");
                if(ATAID.UDMAActive.HasFlag(TransferMode.Mode2))
                    sb.Append("(active) ");
            }
            if(ATAID.UDMASupported.HasFlag(TransferMode.Mode3))
            {
                sb.Append("UDMA3 ");
                if(ATAID.UDMAActive.HasFlag(TransferMode.Mode3))
                    sb.Append("(active) ");
            }
            if(ATAID.UDMASupported.HasFlag(TransferMode.Mode4))
            {
                sb.Append("UDMA4 ");
                if(ATAID.UDMAActive.HasFlag(TransferMode.Mode4))
                    sb.Append("(active) ");
            }
            if(ATAID.UDMASupported.HasFlag(TransferMode.Mode5))
            {
                sb.Append("UDMA5 ");
                if(ATAID.UDMAActive.HasFlag(TransferMode.Mode5))
                    sb.Append("(active) ");
            }
            if(ATAID.UDMASupported.HasFlag(TransferMode.Mode6))
            {
                sb.Append("UDMA6 ");
                if(ATAID.UDMAActive.HasFlag(TransferMode.Mode6))
                    sb.Append("(active) ");
            }
            if(ATAID.UDMASupported.HasFlag(TransferMode.Mode7))
            {
                sb.Append("UDMA7 ");
                if(ATAID.UDMAActive.HasFlag(TransferMode.Mode7))
                    sb.Append("(active) ");
            }

            if(ATAID.MinMDMACycleTime != 0 && ATAID.RecMDMACycleTime != 0)
            {
                sb.AppendLine().AppendFormat("At minimum {0} ns. transfer cycle time per word in MDMA, " +
                    "{1} ns. recommended", ATAID.MinMDMACycleTime, ATAID.RecMDMACycleTime);
            }
            if(ATAID.MinPIOCycleTimeNoFlow != 0)
            {
                sb.AppendLine().AppendFormat("At minimum {0} ns. transfer cycle time per word in PIO, " +
                    "without flow control", ATAID.MinPIOCycleTimeNoFlow);
            }
            if(ATAID.MinPIOCycleTimeFlow != 0)
            {
                sb.AppendLine().AppendFormat("At minimum {0} ns. transfer cycle time per word in PIO, " +
                    "with IORDY flow control", ATAID.MinPIOCycleTimeFlow);
            }

            if(ATAID.MaxQueueDepth != 0)
            {
                sb.AppendLine().AppendFormat("{0} depth of queue maximum", ATAID.MaxQueueDepth + 1);
            }

            if(atapi)
            {
                if(ATAID.PacketBusRelease != 0)
                    sb.AppendLine().AppendFormat("{0} ns. typical to release bus from receipt of PACKET", ATAID.PacketBusRelease);
                if(ATAID.ServiceBusyClear != 0)
                    sb.AppendLine().AppendFormat("{0} ns. typical to clear BSY bit from receipt of SERVICE", ATAID.ServiceBusyClear);
            }

            if(((ATAID.TransportMajorVersion & 0xF000) >> 12) == 0x1 ||
               ((ATAID.TransportMajorVersion & 0xF000) >> 12) == 0xE)
            {
                if(!ATAID.SATACapabilities.HasFlag(SATACapabilitiesBit.Clear))
                {
                    if(ATAID.SATACapabilities.HasFlag(SATACapabilitiesBit.Gen1Speed))
                    {
                        sb.AppendLine().Append("SATA 1.5Gb/s is supported");
                    }
                    if(ATAID.SATACapabilities.HasFlag(SATACapabilitiesBit.Gen2Speed))
                    {
                        sb.AppendLine().Append("SATA 3.0Gb/s is supported");
                    }
                    if(ATAID.SATACapabilities.HasFlag(SATACapabilitiesBit.Gen3Speed))
                    {
                        sb.AppendLine().Append("SATA 6.0Gb/s is supported");
                    }
                    if(ATAID.SATACapabilities.HasFlag(SATACapabilitiesBit.PowerReceipt))
                    {
                        sb.AppendLine().Append("Receipt of host initiated power management requests is supported");
                    }
                    if(ATAID.SATACapabilities.HasFlag(SATACapabilitiesBit.PHYEventCounter))
                    {
                        sb.AppendLine().Append("PHY Event counters are supported");
                    }
                    if(ATAID.SATACapabilities.HasFlag(SATACapabilitiesBit.HostSlumbTrans))
                    {
                        sb.AppendLine().Append("Supports host automatic partial to slumber transitions is supported");
                    }
                    if(ATAID.SATACapabilities.HasFlag(SATACapabilitiesBit.DevSlumbTrans))
                    {
                        sb.AppendLine().Append("Supports device automatic partial to slumber transitions is supported");
                    }
                    if(ATAID.SATACapabilities.HasFlag(SATACapabilitiesBit.NCQ))
                    {
                        sb.AppendLine().Append("NCQ is supported");

                        if(ATAID.SATACapabilities.HasFlag(SATACapabilitiesBit.NCQPriority))
                        {
                            sb.AppendLine().Append("NCQ priority is supported");
                        }
                        if(ATAID.SATACapabilities.HasFlag(SATACapabilitiesBit.UnloadNCQ))
                        {
                            sb.AppendLine().Append("Unload is supported with outstanding NCQ commands");
                        }
                    }
                }

                if(!ATAID.SATACapabilities2.HasFlag(SATACapabilitiesBit2.Clear))
                {
                    if(!ATAID.SATACapabilities.HasFlag(SATACapabilitiesBit.Clear) &&
                        ATAID.SATACapabilities.HasFlag(SATACapabilitiesBit.NCQ))
                    {
                        if(ATAID.SATACapabilities2.HasFlag(SATACapabilitiesBit2.NCQMgmt))
                        {
                            sb.AppendLine().Append("NCQ queue management is supported");
                        }
                        if(ATAID.SATACapabilities2.HasFlag(SATACapabilitiesBit2.NCQStream))
                        {
                            sb.AppendLine().Append("NCQ streaming is supported");
                        }
                    }

                    if(atapi)
                    {
                        if(ATAID.SATACapabilities2.HasFlag(SATACapabilitiesBit2.HostEnvDetect))
                        {
                            sb.AppendLine().Append("ATAPI device supports host environment detection");
                        }
                        if(ATAID.SATACapabilities2.HasFlag(SATACapabilitiesBit2.DevAttSlimline))
                        {
                            sb.AppendLine().Append("ATAPI device supports attention on slimline connected devices");
                        }
                    }

                    //sb.AppendFormat("Negotiated speed = {0}", ((ushort)ATAID.SATACapabilities2 & 0x000E) >> 1);
                }
            }

            if(ATAID.InterseekDelay != 0x0000 && ATAID.InterseekDelay != 0xFFFF)
            {
                sb.AppendLine().AppendFormat("{0} microseconds of interseek delay for ISO-7779 accoustic testing", ATAID.InterseekDelay);
            }

            if((ushort)ATAID.DeviceFormFactor != 0x0000 && (ushort)ATAID.DeviceFormFactor != 0xFFFF)
            {
                switch(ATAID.DeviceFormFactor)
                {
                    case DeviceFormFactorEnum.FiveAndQuarter:
                        sb.AppendLine().Append("Device nominal size is 5.25\"");
                        break;
                    case DeviceFormFactorEnum.ThreeAndHalf:
                        sb.AppendLine().Append("Device nominal size is 3.5\"");
                        break;
                    case DeviceFormFactorEnum.TwoAndHalf:
                        sb.AppendLine().Append("Device nominal size is 2.5\"");
                        break;
                    case DeviceFormFactorEnum.OnePointEight:
                        sb.AppendLine().Append("Device nominal size is 1.8\"");
                        break;
                    case DeviceFormFactorEnum.LessThanOnePointEight:
                        sb.AppendLine().Append("Device nominal size is smaller than 1.8\"");
                        break;
                    default:
                        sb.AppendLine().AppendFormat("Device nominal size field value {0} is unknown", ATAID.DeviceFormFactor);
                        break;
                }
            }

            if(atapi)
            {
                if(ATAID.ATAPIByteCount > 0)
                    sb.AppendLine().AppendFormat("{0} bytes count limit for ATAPI", ATAID.ATAPIByteCount);
            }

            if(cfa)
            {
                if((ATAID.CFAPowerMode & 0x8000) == 0x8000)
                {
                    sb.AppendLine().Append("CompactFlash device supports power mode 1");
                    if((ATAID.CFAPowerMode & 0x2000) == 0x2000)
                        sb.AppendLine().Append("CompactFlash power mode 1 required for one or more commands");
                    if((ATAID.CFAPowerMode & 0x1000) == 0x1000)
                        sb.AppendLine().Append("CompactFlash power mode 1 is disabled");

                    sb.AppendLine().AppendFormat("CompactFlash device uses a maximum of {0} mA", (ATAID.CFAPowerMode & 0x0FFF));
                }
            }

            sb.AppendLine();

            sb.AppendLine().Append("Command set and features:");
            if(ATAID.CommandSet.HasFlag(CommandSetBit.Nop))
            {
                sb.AppendLine().Append("NOP is supported");
                if(ATAID.EnabledCommandSet.HasFlag(CommandSetBit.Nop))
                    sb.Append(" and enabled");
            }
            if(ATAID.CommandSet.HasFlag(CommandSetBit.ReadBuffer))
            {
                sb.AppendLine().Append("READ BUFFER is supported");
                if(ATAID.EnabledCommandSet.HasFlag(CommandSetBit.ReadBuffer))
                    sb.Append(" and enabled");
            }
            if(ATAID.CommandSet.HasFlag(CommandSetBit.WriteBuffer))
            {
                sb.AppendLine().Append("WRITE BUFFER is supported");
                if(ATAID.EnabledCommandSet.HasFlag(CommandSetBit.WriteBuffer))
                    sb.Append(" and enabled");
            }
            if(ATAID.CommandSet.HasFlag(CommandSetBit.HPA))
            {
                sb.AppendLine().Append("Host Protected Area is supported");
                if(ATAID.EnabledCommandSet.HasFlag(CommandSetBit.HPA))
                    sb.Append(" and enabled");
            }
            if(ATAID.CommandSet.HasFlag(CommandSetBit.DeviceReset))
            {
                sb.AppendLine().Append("DEVICE RESET is supported");
                if(ATAID.EnabledCommandSet.HasFlag(CommandSetBit.DeviceReset))
                    sb.Append(" and enabled");
            }
            if(ATAID.CommandSet.HasFlag(CommandSetBit.Service))
            {
                sb.AppendLine().Append("SERVICE interrupt is supported");
                if(ATAID.EnabledCommandSet.HasFlag(CommandSetBit.Service))
                    sb.Append(" and enabled");
            }
            if(ATAID.CommandSet.HasFlag(CommandSetBit.Release))
            {
                sb.AppendLine().Append("Release is supported");
                if(ATAID.EnabledCommandSet.HasFlag(CommandSetBit.Release))
                    sb.Append(" and enabled");
            }
            if(ATAID.CommandSet.HasFlag(CommandSetBit.LookAhead))
            {
                sb.AppendLine().Append("Look-ahead read is supported");
                if(ATAID.EnabledCommandSet.HasFlag(CommandSetBit.LookAhead))
                    sb.Append(" and enabled");
            }
            if(ATAID.CommandSet.HasFlag(CommandSetBit.WriteCache))
            {
                sb.AppendLine().Append("Write cache is supported");
                if(ATAID.EnabledCommandSet.HasFlag(CommandSetBit.WriteCache))
                    sb.Append(" and enabled");
            }
            if(ATAID.CommandSet.HasFlag(CommandSetBit.Packet))
            {
                sb.AppendLine().Append("PACKET is supported");
                if(ATAID.EnabledCommandSet.HasFlag(CommandSetBit.Packet))
                    sb.Append(" and enabled");
            }
            if(ATAID.CommandSet.HasFlag(CommandSetBit.PowerManagement))
            {
                sb.AppendLine().Append("Power management is supported");
                if(ATAID.EnabledCommandSet.HasFlag(CommandSetBit.PowerManagement))
                    sb.Append(" and enabled");
            }
            if(ATAID.CommandSet.HasFlag(CommandSetBit.RemovableMedia))
            {
                sb.AppendLine().Append("Removable media feature set is supported");
                if(ATAID.EnabledCommandSet.HasFlag(CommandSetBit.RemovableMedia))
                    sb.Append(" and enabled");
            }
            if(ATAID.CommandSet.HasFlag(CommandSetBit.SecurityMode))
            {
                sb.AppendLine().Append("Security mode is supported");
                if(ATAID.EnabledCommandSet.HasFlag(CommandSetBit.SecurityMode))
                    sb.Append(" and enabled");
            }
            if(ATAID.Capabilities.HasFlag(CapabilitiesBit.LBASupport))
                sb.AppendLine().Append("28-bit LBA is supported");

            if(ATAID.CommandSet2.HasFlag(CommandSetBit2.MustBeSet) &&
                !ATAID.CommandSet2.HasFlag(CommandSetBit2.MustBeClear))
            {
                if(ATAID.CommandSet2.HasFlag(CommandSetBit2.LBA48))
                {
                    sb.AppendLine().Append("48-bit LBA is supported");
                    if(ATAID.EnabledCommandSet2.HasFlag(CommandSetBit2.LBA48))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet2.HasFlag(CommandSetBit2.FlushCache))
                {
                    sb.AppendLine().Append("FLUSH CACHE is supported");
                    if(ATAID.EnabledCommandSet2.HasFlag(CommandSetBit2.FlushCache))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet2.HasFlag(CommandSetBit2.FlushCacheExt))
                {
                    sb.AppendLine().Append("FLUSH CACHE EXT is supported");
                    if(ATAID.EnabledCommandSet2.HasFlag(CommandSetBit2.FlushCacheExt))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet2.HasFlag(CommandSetBit2.DCO))
                {
                    sb.AppendLine().Append("Device Configuration Overlay feature set is supported");
                    if(ATAID.EnabledCommandSet2.HasFlag(CommandSetBit2.DCO))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet2.HasFlag(CommandSetBit2.AAM))
                {
                    sb.AppendLine().Append("Automatic Acoustic Management is supported");
                    if(ATAID.EnabledCommandSet2.HasFlag(CommandSetBit2.AAM))
                    {
                        sb.AppendFormat(" and enabled with value {0} (vendor recommends {1}",
                            ATAID.CurrentAAM, ATAID.RecommendedAAM);
                    }
                }
                if(ATAID.CommandSet2.HasFlag(CommandSetBit2.SetMax))
                {
                    sb.AppendLine().Append("SET MAX security extension is supported");
                    if(ATAID.EnabledCommandSet2.HasFlag(CommandSetBit2.SetMax))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet2.HasFlag(CommandSetBit2.AddressOffsetReservedAreaBoot))
                {
                    sb.AppendLine().Append("Address Offset Reserved Area Boot is supported");
                    if(ATAID.EnabledCommandSet2.HasFlag(CommandSetBit2.AddressOffsetReservedAreaBoot))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet2.HasFlag(CommandSetBit2.SetFeaturesRequired))
                {
                    sb.AppendLine().Append("SET FEATURES is required before spin-up");
                }
                if(ATAID.CommandSet2.HasFlag(CommandSetBit2.PowerUpInStandby))
                {
                    sb.AppendLine().Append("Power-up in standby is supported");
                    if(ATAID.EnabledCommandSet2.HasFlag(CommandSetBit2.PowerUpInStandby))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet2.HasFlag(CommandSetBit2.RemovableNotification))
                {
                    sb.AppendLine().Append("Removable Media Status Notification is supported");
                    if(ATAID.EnabledCommandSet2.HasFlag(CommandSetBit2.RemovableNotification))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet2.HasFlag(CommandSetBit2.APM))
                {
                    sb.AppendLine().Append("Advanced Power Management is supported");
                    if(ATAID.EnabledCommandSet2.HasFlag(CommandSetBit2.APM))
                    {
                        sb.AppendFormat(" and enabled with value {0}", ATAID.CurrentAPM);
                    }
                }
                if(ATAID.CommandSet2.HasFlag(CommandSetBit2.CompactFlash))
                {
                    sb.AppendLine().Append("CompactFlash feature set is supported");
                    if(ATAID.EnabledCommandSet2.HasFlag(CommandSetBit2.CompactFlash))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet2.HasFlag(CommandSetBit2.RWQueuedDMA))
                {
                    sb.AppendLine().Append("READ DMA QUEUED and WRITE DMA QUEUED are supported");
                    if(ATAID.EnabledCommandSet2.HasFlag(CommandSetBit2.RWQueuedDMA))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet2.HasFlag(CommandSetBit2.DownloadMicrocode))
                {
                    sb.AppendLine().Append("DOWNLOAD MICROCODE is supported");
                    if(ATAID.EnabledCommandSet2.HasFlag(CommandSetBit2.DownloadMicrocode))
                        sb.Append(" and enabled");
                }
            }

            if(ATAID.CommandSet.HasFlag(CommandSetBit.SMART))
            {
                sb.AppendLine().Append("S.M.A.R.T. is supported");
                if(ATAID.EnabledCommandSet.HasFlag(CommandSetBit.SMART))
                    sb.Append(" and enabled");
            }

            if(ATAID.SCTCommandTransport.HasFlag(SCTCommandTransportBit.Supported))
                sb.AppendLine().Append("S.M.A.R.T. Command Transport is supported");

            if(ATAID.CommandSet3.HasFlag(CommandSetBit3.MustBeSet) &&
               !ATAID.CommandSet3.HasFlag(CommandSetBit3.MustBeClear))
            {
                if(ATAID.CommandSet3.HasFlag(CommandSetBit3.SMARTSelfTest))
                {
                    sb.AppendLine().Append("S.M.A.R.T. self-testing is supported");
                    if(ATAID.EnabledCommandSet3.HasFlag(CommandSetBit3.SMARTSelfTest))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet3.HasFlag(CommandSetBit3.SMARTLog))
                {
                    sb.AppendLine().Append("S.M.A.R.T. error logging is supported");
                    if(ATAID.EnabledCommandSet3.HasFlag(CommandSetBit3.SMARTLog))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet3.HasFlag(CommandSetBit3.IdleImmediate))
                {
                    sb.AppendLine().Append("IDLE IMMEDIATE with UNLOAD FEATURE is supported");
                    if(ATAID.EnabledCommandSet3.HasFlag(CommandSetBit3.IdleImmediate))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet3.HasFlag(CommandSetBit3.WriteURG))
                {
                    sb.AppendLine().Append("URG bit is supported in WRITE STREAM DMA EXT and WRITE STREAM EXT");
                }
                if(ATAID.CommandSet3.HasFlag(CommandSetBit3.ReadURG))
                {
                    sb.AppendLine().Append("URG bit is supported in READ STREAM DMA EXT and READ STREAM EXT");
                }
                if(ATAID.CommandSet3.HasFlag(CommandSetBit3.WWN))
                {
                    sb.AppendLine().Append("Device has a World Wide Name");
                }
                if(ATAID.CommandSet3.HasFlag(CommandSetBit3.FUAWriteQ))
                {
                    sb.AppendLine().Append("WRITE DMA QUEUED FUA EXT is supported");
                    if(ATAID.EnabledCommandSet3.HasFlag(CommandSetBit3.FUAWriteQ))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet3.HasFlag(CommandSetBit3.FUAWrite))
                {
                    sb.AppendLine().Append("WRITE DMA FUA EXT and WRITE MULTIPLE FUA EXT are supported");
                    if(ATAID.EnabledCommandSet3.HasFlag(CommandSetBit3.FUAWrite))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet3.HasFlag(CommandSetBit3.GPL))
                {
                    sb.AppendLine().Append("General Purpose Logging is supported");
                    if(ATAID.EnabledCommandSet3.HasFlag(CommandSetBit3.GPL))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet3.HasFlag(CommandSetBit3.Streaming))
                {
                    sb.AppendLine().Append("Streaming feature set is supported");
                    if(ATAID.EnabledCommandSet3.HasFlag(CommandSetBit3.Streaming))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet3.HasFlag(CommandSetBit3.MCPT))
                {
                    sb.AppendLine().Append("Media Card Pass Through command set is supported");
                    if(ATAID.EnabledCommandSet3.HasFlag(CommandSetBit3.MCPT))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet3.HasFlag(CommandSetBit3.MediaSerial))
                {
                    sb.AppendLine().Append("Media Serial is supported");
                    if(ATAID.EnabledCommandSet3.HasFlag(CommandSetBit3.MediaSerial))
                        sb.Append(" and valid");
                }
            }

            if(ATAID.CommandSet4.HasFlag(CommandSetBit4.MustBeSet) &&
                !ATAID.CommandSet4.HasFlag(CommandSetBit4.MustBeClear))
            {
                if(ATAID.CommandSet4.HasFlag(CommandSetBit4.DSN))
                {
                    sb.AppendLine().Append("DSN feature set is supported");
                    if(ATAID.EnabledCommandSet4.HasFlag(CommandSetBit4.DSN))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet4.HasFlag(CommandSetBit4.AMAC))
                {
                    sb.AppendLine().Append("Accessible Max Address Configuration is supported");
                    if(ATAID.EnabledCommandSet4.HasFlag(CommandSetBit4.AMAC))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet4.HasFlag(CommandSetBit4.ExtPowerCond))
                {
                    sb.AppendLine().Append("Extended Power Conditions are supported");
                    if(ATAID.EnabledCommandSet4.HasFlag(CommandSetBit4.ExtPowerCond))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet4.HasFlag(CommandSetBit4.ExtStatusReport))
                {
                    sb.AppendLine().Append("Extended Status Reporting is supported");
                    if(ATAID.EnabledCommandSet4.HasFlag(CommandSetBit4.ExtStatusReport))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet4.HasFlag(CommandSetBit4.FreeFallControl))
                {
                    sb.AppendLine().Append("Free-fall control feature set is supported");
                    if(ATAID.EnabledCommandSet4.HasFlag(CommandSetBit4.FreeFallControl))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet4.HasFlag(CommandSetBit4.SegmentedDownloadMicrocode))
                {
                    sb.AppendLine().Append("Segmented feature in DOWNLOAD MICROCODE is supported");
                    if(ATAID.EnabledCommandSet4.HasFlag(CommandSetBit4.SegmentedDownloadMicrocode))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet4.HasFlag(CommandSetBit4.RWDMAExtGpl))
                {
                    sb.AppendLine().Append("READ/WRITE DMA EXT GPL are supported");
                    if(ATAID.EnabledCommandSet4.HasFlag(CommandSetBit4.RWDMAExtGpl))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet4.HasFlag(CommandSetBit4.WriteUnc))
                {
                    sb.AppendLine().Append("WRITE UNCORRECTABLE is supported");
                    if(ATAID.EnabledCommandSet4.HasFlag(CommandSetBit4.WriteUnc))
                        sb.Append(" and enabled");
                }
                if(ATAID.CommandSet4.HasFlag(CommandSetBit4.WRV))
                {
                    sb.AppendLine().Append("Write/Read/Verify is supported");
                    if(ATAID.EnabledCommandSet4.HasFlag(CommandSetBit4.WRV))
                        sb.Append(" and enabled");
                    sb.AppendLine().AppendFormat("{0} sectors for Write/Read/Verify mode 2", ATAID.WRVSectorCountMode2);
                    sb.AppendLine().AppendFormat("{0} sectors for Write/Read/Verify mode 3", ATAID.WRVSectorCountMode3);
                    if(ATAID.EnabledCommandSet4.HasFlag(CommandSetBit4.WRV))
                        sb.AppendLine().AppendFormat("Current Write/Read/Verify mode: {0}", ATAID.WRVMode);
                }
                if(ATAID.CommandSet4.HasFlag(CommandSetBit4.DT1825))
                {
                    sb.AppendLine().Append("DT1825 is supported");
                    if(ATAID.EnabledCommandSet4.HasFlag(CommandSetBit4.DT1825))
                        sb.Append(" and enabled");
                }
            }
            if(ATAID.Capabilities3.HasFlag(CapabilitiesBit3.BlockErase))
                sb.AppendLine().Append("BLOCK ERASE EXT is supported");
            if(ATAID.Capabilities3.HasFlag(CapabilitiesBit3.Overwrite))
                sb.AppendLine().Append("OVERWRITE EXT is supported");
            if(ATAID.Capabilities3.HasFlag(CapabilitiesBit3.CryptoScramble))
                sb.AppendLine().Append("CRYPTO SCRAMBLE EXT is supported");

            if(ATAID.CommandSet5.HasFlag(CommandSetBit5.DeviceConfDMA))
            {
                sb.AppendLine().Append("DEVICE CONFIGURATION IDENTIFY DMA and DEVICE CONFIGURATION SET DMA are supported");
            }
            if(ATAID.CommandSet5.HasFlag(CommandSetBit5.ReadBufferDMA))
            {
                sb.AppendLine().Append("READ BUFFER DMA is supported");
            }
            if(ATAID.CommandSet5.HasFlag(CommandSetBit5.WriteBufferDMA))
            {
                sb.AppendLine().Append("WRITE BUFFER DMA is supported");
            }
            if(ATAID.CommandSet5.HasFlag(CommandSetBit5.DownloadMicroCodeDMA))
            {
                sb.AppendLine().Append("DOWNLOAD MICROCODE DMA is supported");
            }
            if(ATAID.CommandSet5.HasFlag(CommandSetBit5.SetMaxDMA))
            {
                sb.AppendLine().Append("SET PASSWORD DMA and SET UNLOCK DMA are supported");
            }
            if(ATAID.CommandSet5.HasFlag(CommandSetBit5.Ata28))
            {
                sb.AppendLine().Append("Not all 28-bit commands are supported");
            }

            if(ATAID.CommandSet5.HasFlag(CommandSetBit5.CFast))
            {
                sb.AppendLine().Append("Device follows CFast specification");
            }
            if(ATAID.CommandSet5.HasFlag(CommandSetBit5.IEEE1667))
            {
                sb.AppendLine().Append("Device follows IEEE-1667");
            }

            if(ATAID.CommandSet5.HasFlag(CommandSetBit5.DeterministicTrim))
            {
                sb.AppendLine().Append("Read after TRIM is deterministic");
                if(ATAID.CommandSet5.HasFlag(CommandSetBit5.ReadZeroTrim))
                {
                    sb.AppendLine().Append("Read after TRIM returns empty data");
                }
            }
            if(ATAID.CommandSet5.HasFlag(CommandSetBit5.LongPhysSectorAligError))
            {
                sb.AppendLine().Append("Device supports Long Physical Sector Alignment Error Reporting Control");
            }
            if(ATAID.CommandSet5.HasFlag(CommandSetBit5.Encrypted))
            {
                sb.AppendLine().Append("Device encrypts all user data");
            }
            if(ATAID.CommandSet5.HasFlag(CommandSetBit5.AllCacheNV))
            {
                sb.AppendLine().Append("Device's write cache is non-volatile");
            }
            if(ATAID.CommandSet5.HasFlag(CommandSetBit5.ZonedBit0) ||
                ATAID.CommandSet5.HasFlag(CommandSetBit5.ZonedBit1))
            {
                sb.AppendLine().Append("Device is zoned");
            }

            if(ATAID.Capabilities3.HasFlag(CapabilitiesBit3.Sanitize))
            {
                sb.AppendLine().Append("Sanitize feature set is supported");
                if(ATAID.Capabilities3.HasFlag(CapabilitiesBit3.SanitizeCommands))
                    sb.AppendLine().Append("Sanitize commands are specified by ACS-3 or higher");
                else
                    sb.AppendLine().Append("Sanitize commands are specified by ACS-2");

                if(ATAID.Capabilities3.HasFlag(CapabilitiesBit3.SanitizeAntifreeze))
                    sb.AppendLine().Append("SANITIZE ANTIFREEZE LOCK EXT is supported");
            }

            if(!ata1 && maxatalevel >= 8)
            {
                if(ATAID.TrustedComputing.HasFlag(TrustedComputingBit.Set) &&
                    !ATAID.TrustedComputing.HasFlag(TrustedComputingBit.Clear) &&
                    ATAID.TrustedComputing.HasFlag(TrustedComputingBit.TrustedComputing))
                    sb.AppendLine().Append("Trusted Computing feature set is supported");
            }

            if(((ATAID.TransportMajorVersion & 0xF000) >> 12) == 0x1 ||
                ((ATAID.TransportMajorVersion & 0xF000) >> 12) == 0xE)
            {
                if(!ATAID.SATACapabilities.HasFlag(SATACapabilitiesBit.Clear))
                {
                    if(ATAID.SATACapabilities.HasFlag(SATACapabilitiesBit.ReadLogDMAExt))
                        sb.AppendLine().Append("READ LOG DMA EXT is supported");
                }

                if(!ATAID.SATACapabilities2.HasFlag(SATACapabilitiesBit2.Clear))
                {
                    if(ATAID.SATACapabilities2.HasFlag(SATACapabilitiesBit2.FPDMAQ))
                        sb.AppendLine().Append("RECEIVE FPDMA QUEUED and SEND FPDMA QUEUED are supported");
                }

                if(!ATAID.SATAFeatures.HasFlag(SATAFeaturesBit.Clear))
                {
                    if(ATAID.SATAFeatures.HasFlag(SATAFeaturesBit.NonZeroBufferOffset))
                    {
                        sb.AppendLine().Append("Non-zero buffer offsets are supported");
                        if(ATAID.EnabledSATAFeatures.HasFlag(SATAFeaturesBit.NonZeroBufferOffset))
                            sb.Append(" and enabled");
                    }
                    if(ATAID.SATAFeatures.HasFlag(SATAFeaturesBit.DMASetup))
                    {
                        sb.AppendLine().Append("DMA Setup auto-activation is supported");
                        if(ATAID.EnabledSATAFeatures.HasFlag(SATAFeaturesBit.DMASetup))
                            sb.Append(" and enabled");
                    }
                    if(ATAID.SATAFeatures.HasFlag(SATAFeaturesBit.InitPowerMgmt))
                    {
                        sb.AppendLine().Append("Device-initiated power management is supported");
                        if(ATAID.EnabledSATAFeatures.HasFlag(SATAFeaturesBit.InitPowerMgmt))
                            sb.Append(" and enabled");
                    }
                    if(ATAID.SATAFeatures.HasFlag(SATAFeaturesBit.InOrderData))
                    {
                        sb.AppendLine().Append("In-order data delivery is supported");
                        if(ATAID.EnabledSATAFeatures.HasFlag(SATAFeaturesBit.InOrderData))
                            sb.Append(" and enabled");
                    }
                    if(!atapi)
                    {
                        if(ATAID.SATAFeatures.HasFlag(SATAFeaturesBit.HardwareFeatureControl))
                        {
                            sb.AppendLine().Append("Hardware Feature Control is supported");
                            if(ATAID.EnabledSATAFeatures.HasFlag(SATAFeaturesBit.HardwareFeatureControl))
                                sb.Append(" and enabled");
                        }
                    }
                    if(atapi)
                    {
                        if(ATAID.SATAFeatures.HasFlag(SATAFeaturesBit.AsyncNotification))
                        {
                            sb.AppendLine().Append("Asynchronous notification is supported");
                            if(ATAID.EnabledSATAFeatures.HasFlag(SATAFeaturesBit.AsyncNotification))
                                sb.Append(" and enabled");
                        }
                    }
                    if(ATAID.SATAFeatures.HasFlag(SATAFeaturesBit.SettingsPreserve))
                    {
                        sb.AppendLine().Append("Software Settings Preservation is supported");
                        if(ATAID.EnabledSATAFeatures.HasFlag(SATAFeaturesBit.SettingsPreserve))
                            sb.Append(" and enabled");
                    }
                    if(ATAID.SATAFeatures.HasFlag(SATAFeaturesBit.NCQAutoSense))
                    {
                        sb.AppendLine().Append("NCQ Autosense is supported");
                    }
                    if(ATAID.EnabledSATAFeatures.HasFlag(SATAFeaturesBit.EnabledSlumber))
                    {
                        sb.AppendLine().Append("Automatic Partial to Slumber transitions are enabled");
                    }

                }
            }
            if((ATAID.RemovableStatusSet & 0x03) > 0)
            {
                sb.AppendLine().Append("Removable Media Status Notification feature set is supported");
            }

            if(ATAID.FreeFallSensitivity != 0x00 && ATAID.FreeFallSensitivity != 0xFF)
            {
                sb.AppendLine().AppendFormat("Free-fall sensitivity set to {0}", ATAID.FreeFallSensitivity);
            }

            if(ATAID.DataSetMgmt.HasFlag(DataSetMgmtBit.Trim))
                sb.AppendLine().Append("TRIM is supported");
            if(ATAID.DataSetMgmtSize > 0)
            {
                sb.AppendLine().AppendFormat("DATA SET MANAGEMENT can receive a maximum of {0} blocks of 512 bytes", ATAID.DataSetMgmtSize);
            }

            sb.AppendLine().AppendLine();
            if(ATAID.SecurityStatus.HasFlag(SecurityStatusBit.Supported))
            {
                sb.AppendLine("Security:");
                if(ATAID.SecurityStatus.HasFlag(SecurityStatusBit.Enabled))
                {
                    sb.AppendLine("Security is enabled");
                    if(ATAID.SecurityStatus.HasFlag(SecurityStatusBit.Locked))
                        sb.AppendLine("Security is locked");
                    else
                        sb.AppendLine("Security is not locked");

                    if(ATAID.SecurityStatus.HasFlag(SecurityStatusBit.Frozen))
                        sb.AppendLine("Security is frozen");
                    else
                        sb.AppendLine("Security is not frozen");

                    if(ATAID.SecurityStatus.HasFlag(SecurityStatusBit.Expired))
                        sb.AppendLine("Security count has expired");
                    else
                        sb.AppendLine("Security count has notexpired");

                    if(ATAID.SecurityStatus.HasFlag(SecurityStatusBit.Maximum))
                        sb.AppendLine("Security level is maximum");
                    else
                        sb.AppendLine("Security level is high");
                }
                else
                    sb.AppendLine("Security is not enabled");

                if(ATAID.SecurityStatus.HasFlag(SecurityStatusBit.Enhanced))
                    sb.AppendLine("Supports enhanced security erase");

                sb.AppendFormat("{0} minutes to complete secure erase", ATAID.SecurityEraseTime * 2).AppendLine();
                if(ATAID.SecurityStatus.HasFlag(SecurityStatusBit.Enhanced))
                    sb.AppendFormat("{0} minutes to complete enhanced secure erase", ATAID.EnhancedSecurityEraseTime * 2).AppendLine();

                sb.AppendFormat("Master password revision code: {0}", ATAID.MasterPasswordRevisionCode).AppendLine();
            }

            if(ATAID.CommandSet3.HasFlag(CommandSetBit3.MustBeSet) &&
                !ATAID.CommandSet3.HasFlag(CommandSetBit3.MustBeClear) &&
                ATAID.CommandSet3.HasFlag(CommandSetBit3.Streaming))
            {
                sb.AppendLine().AppendLine("Streaming:");
                sb.AppendFormat("Minimum request size is {0}", ATAID.StreamMinReqSize);
                sb.AppendFormat("Streaming transfer time in PIO is {0}", ATAID.StreamTransferTimePIO);
                sb.AppendFormat("Streaming transfer time in DMA is {0}", ATAID.StreamTransferTimeDMA);
                sb.AppendFormat("Streaming access latency is {0}", ATAID.StreamAccessLatency);
                sb.AppendFormat("Streaming performance granularity is {0}", ATAID.StreamPerformanceGranularity);
            }

            if(ATAID.SCTCommandTransport.HasFlag(SCTCommandTransportBit.Supported))
            {
                sb.AppendLine().AppendLine("S.M.A.R.T. Command Transport (SCT):");
                if(ATAID.SCTCommandTransport.HasFlag(SCTCommandTransportBit.LongSectorAccess))
                    sb.AppendLine("SCT Long Sector Address is supported");
                if(ATAID.SCTCommandTransport.HasFlag(SCTCommandTransportBit.WriteSame))
                    sb.AppendLine("SCT Write Same is supported");
                if(ATAID.SCTCommandTransport.HasFlag(SCTCommandTransportBit.ErrorRecoveryControl))
                    sb.AppendLine("SCT Error Recovery Control is supported");
                if(ATAID.SCTCommandTransport.HasFlag(SCTCommandTransportBit.FeaturesControl))
                    sb.AppendLine("SCT Features Control is supported");
                if(ATAID.SCTCommandTransport.HasFlag(SCTCommandTransportBit.DataTables))
                    sb.AppendLine("SCT Data Tables are supported");
            }

            if((ATAID.NVCacheCaps & 0x0010) == 0x0010)
            {
                sb.AppendLine().AppendLine("Non-Volatile Cache:");
                sb.AppendLine().AppendFormat("Version {0}", (ATAID.NVCacheCaps & 0xF000) >> 12).AppendLine();
                if((ATAID.NVCacheCaps & 0x0001) == 0x0001)
                {
                    sb.Append("Power mode feature set is supported");
                    if((ATAID.NVCacheCaps & 0x0002) == 0x0002)
                        sb.Append(" and enabled");
                    sb.AppendLine();

                    sb.AppendLine().AppendFormat("Version {0}", (ATAID.NVCacheCaps & 0x0F00) >> 8).AppendLine();
                }
                sb.AppendLine().AppendFormat("Non-Volatile Cache is {0} bytes", ATAID.NVCacheSize * logicalsectorsize).AppendLine();
            }

#if DEBUG
            sb.AppendLine();
            if(ATAID.VendorWord9 != 0x0000 && ATAID.VendorWord9 != 0xFFFF)
                sb.AppendFormat("Word 9: 0x{0:X4}", ATAID.VendorWord9).AppendLine();
            if((ATAID.VendorWord47 & 0x7F) != 0x7F && (ATAID.VendorWord47 & 0x7F) != 0x00)
                sb.AppendFormat("Word 47 bits 15 to 8: 0x{0:X2}", ATAID.VendorWord47).AppendLine();
            if(ATAID.VendorWord51 != 0x00 && ATAID.VendorWord51 != 0xFF)
                sb.AppendFormat("Word 51 bits 7 to 0: 0x{0:X2}", ATAID.VendorWord51).AppendLine();
            if(ATAID.VendorWord52 != 0x00 && ATAID.VendorWord52 != 0xFF)
                sb.AppendFormat("Word 52 bits 7 to 0: 0x{0:X2}", ATAID.VendorWord52).AppendLine();
            if(ATAID.ReservedWord64 != 0x00 && ATAID.ReservedWord64 != 0xFF)
                sb.AppendFormat("Word 64 bits 15 to 8: 0x{0:X2}", ATAID.ReservedWord64).AppendLine();
            if(ATAID.ReservedWord70 != 0x0000 && ATAID.ReservedWord70 != 0xFFFF)
                sb.AppendFormat("Word 70: 0x{0:X4}", ATAID.ReservedWord70).AppendLine();
            if(ATAID.ReservedWord73 != 0x0000 && ATAID.ReservedWord73 != 0xFFFF)
                sb.AppendFormat("Word 73: 0x{0:X4}", ATAID.ReservedWord73).AppendLine();
            if(ATAID.ReservedWord74 != 0x0000 && ATAID.ReservedWord74 != 0xFFFF)
                sb.AppendFormat("Word 74: 0x{0:X4}", ATAID.ReservedWord74).AppendLine();
            if(ATAID.ReservedWord116 != 0x0000 && ATAID.ReservedWord116 != 0xFFFF)
                sb.AppendFormat("Word 116: 0x{0:X4}", ATAID.ReservedWord116).AppendLine();
            for(int i = 0; i < ATAID.ReservedWords121.Length; i++)
            {
                if(ATAID.ReservedWords121[i] != 0x0000 && ATAID.ReservedWords121[i] != 0xFFFF)
                    sb.AppendFormat("Word {1}: 0x{0:X4}", ATAID.ReservedWords121[i], 121 + i).AppendLine();
            }
            for(int i = 0; i < ATAID.ReservedWords129.Length; i++)
            {
                if(ATAID.ReservedWords129[i] != 0x0000 && ATAID.ReservedWords129[i] != 0xFFFF)
                    sb.AppendFormat("Word {1}: 0x{0:X4}", ATAID.ReservedWords129[i], 129 + i).AppendLine();
            }
            for(int i = 0; i < ATAID.ReservedCFA.Length; i++)
            {
                if(ATAID.ReservedCFA[i] != 0x0000 && ATAID.ReservedCFA[i] != 0xFFFF)
                    sb.AppendFormat("Word {1} (CFA): 0x{0:X4}", ATAID.ReservedCFA[i], 161 + i).AppendLine();
            }
            if(ATAID.ReservedWord174 != 0x0000 && ATAID.ReservedWord174 != 0xFFFF)
                sb.AppendFormat("Word 174: 0x{0:X4}", ATAID.ReservedWord174).AppendLine();
            if(ATAID.ReservedWord175 != 0x0000 && ATAID.ReservedWord175 != 0xFFFF)
                sb.AppendFormat("Word 175: 0x{0:X4}", ATAID.ReservedWord175).AppendLine();
            if(ATAID.ReservedCEATAWord207 != 0x0000 && ATAID.ReservedCEATAWord207 != 0xFFFF)
                sb.AppendFormat("Word 207 (CE-ATA): 0x{0:X4}", ATAID.ReservedCEATAWord207).AppendLine();
            if(ATAID.ReservedCEATAWord208 != 0x0000 && ATAID.ReservedCEATAWord208 != 0xFFFF)
                sb.AppendFormat("Word 208 (CE-ATA): 0x{0:X4}", ATAID.ReservedCEATAWord208).AppendLine();
            if(ATAID.NVReserved != 0x00 && ATAID.NVReserved != 0xFF)
                sb.AppendFormat("Word 219 bits 15 to 8: 0x{0:X2}", ATAID.NVReserved).AppendLine();
            if(ATAID.WRVReserved != 0x00 && ATAID.WRVReserved != 0xFF)
                sb.AppendFormat("Word 220 bits 15 to 8: 0x{0:X2}", ATAID.WRVReserved).AppendLine();
            if(ATAID.ReservedWord221 != 0x0000 && ATAID.ReservedWord221 != 0xFFFF)
                sb.AppendFormat("Word 221: 0x{0:X4}", ATAID.ReservedWord221).AppendLine();
            for(int i = 0; i < ATAID.ReservedCEATA224.Length; i++)
            {
                if(ATAID.ReservedCEATA224[i] != 0x0000 && ATAID.ReservedCEATA224[i] != 0xFFFF)
                    sb.AppendFormat("Word {1} (CE-ATA): 0x{0:X4}", ATAID.ReservedCEATA224[i], 224 + i).AppendLine();
            }
            for(int i = 0; i < ATAID.ReservedWords.Length; i++)
            {
                if(ATAID.ReservedWords[i] != 0x0000 && ATAID.ReservedWords[i] != 0xFFFF)
                    sb.AppendFormat("Word {1}: 0x{0:X4}", ATAID.ReservedWords[i], 236 + i).AppendLine();
            }
#endif
            return sb.ToString();
        }

        static ulong DescrambleWWN(ulong WWN)
        {
            byte[] qwb = BitConverter.GetBytes(WWN);
            byte[] qword = new byte[8];

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

        static string DescrambleATAString(byte[] buffer, int offset, int length)
        {
            byte[] outbuf;
            outbuf = buffer[offset + length - 1] != 0x00 ? new byte[length + 1] : new byte[length];

            for(int i = 0; i < length; i += 2)
            {
                outbuf[i] = buffer[offset + i + 1];
                outbuf[i + 1] = buffer[offset + i];
            }

            string outStr = StringHandlers.CToString(outbuf);
            return outStr.Trim();
        }
    }
}

