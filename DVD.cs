/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : BD.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Decoders.

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Decodes DVD structures.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$
using System;
using System.Text;

namespace DiscImageChef.Decoders
{
    /// <summary>
    /// Information from the following standards:
    /// ANSI X3.304-1997
    /// T10/1048-D revision 9.0
    /// T10/1048-D revision 10a
    /// T10/1228-D revision 7.0c
    /// T10/1228-D revision 11a
    /// T10/1363-D revision 10g
    /// T10/1545-D revision 1d
    /// T10/1545-D revision 5
    /// T10/1545-D revision 5a
    /// T10/1675-D revision 2c
    /// T10/1675-D revision 4
    /// T10/1836-D revision 2g
    /// ECMA 365
    /// </summary>
    public static class DVD
    {
        #region Public enumerations
        public enum DiskCategory : byte
        {
            DVDROM = 0x00,
            DVDRAM = 0x01,
            DVDR = 0x02,
            DVDRW = 0x03,
            HDDVDROM = 0x04,
            HDDVDRAM = 0x05,
            HDDVDR = 0x06,
            Reserved1 = 0x07,
            UMD = 0x08,
            DVDPRW = 0x09,
            DVDPR = 0x0A,
            Reserved2 = 0x0B,
            Reserved3 = 0x0C,
            DVDPRWDL = 0x0D,
            DVDPRDL = 0x0E,
            Reserved4 = 0x0F
        }

        public enum MaximumRateField : byte
        {
            /// <summary>
            /// 2.52 Mbps
            /// </summary>
            TwoMbps = 0x00,
            /// <summary>
            /// 5.04 Mbps
            /// </summary>
            FiveMbps = 0x01,
            /// <summary>
            /// 10.08 Mbps
            /// </summary>
            TenMbps = 0x02,
            /// <summary>
            /// 20.16 Mbps
            /// </summary>
            TwentyMbps = 0x03,
            /// <summary>
            /// 30.24 Mbps
            /// </summary>
            ThirtyMbps = 0x04,
            Unspecified = 0x0F
        }

        public enum LayerTypeFieldMask : byte
        {
            Embossed = 0x01,
            Recordable = 0x02,
            Rewritable = 0x04,
            Reserved = 0x08
        }

        public enum LinearDensityField : byte
        {
            /// <summary>
            /// 0.267 μm/bit
            /// </summary>
            TwoSix = 0x00,
            /// <summary>
            /// 0.293 μm/bit
            /// </summary>
            TwoNine = 0x01,
            /// <summary>
            /// 0.409 to 0.435 μm/bit
            /// </summary>
            FourZero = 0x02,
            /// <summary>
            /// 0.280 to 0.291 μm/bit
            /// </summary>
            TwoEight = 0x04,
            /// <summary>
            /// 0.153 μm/bit
            /// </summary>
            OneFive = 0x05,
            /// <summary>
            /// 0.130 to 0.140 μm/bit
            /// </summary>
            OneThree = 0x06,
            /// <summary>
            /// 0.353 μm/bit
            /// </summary>
            ThreeFive = 0x08,
        }

        public enum TrackDensityField : byte
        {
            /// <summary>
            /// 0.74 μm/track
            /// </summary>
            Seven = 0x00,
            /// <summary>
            /// 0.80 μm/track
            /// </summary>
            Eight = 0x01,
            /// <summary>
            /// 0.615 μm/track
            /// </summary>
            Six = 0x02,
            /// <summary>
            /// 0.40 μm/track
            /// </summary>
            Four = 0x03,
            /// <summary>
            /// 0.34 μm/track
            /// </summary>
            Three = 0x04
        }

        public enum CopyrightType : byte
        {
            /// <summary>
            /// There is no copy protection
            /// </summary>
            NoProtection = 0x00,
            /// <summary>
            /// Copy protection is CSS/CPPM
            /// </summary>
            CSS = 0x01,
            /// <summary>
            /// Copy protection is CPRM
            /// </summary>
            CPRM = 0x02,
            /// <summary>
            /// Copy protection is AACS
            /// </summary>
            AACS = 0x10
        }

        public enum WPDiscTypes : byte
        {
            /// <summary>
            /// Should not write without a cartridge
            /// </summary>
            DoNotWrite = 0x00,
            /// <summary>
            /// Can write without a cartridge
            /// </summary>
            CanWrite = 0x01,
            Reserved1 = 0x02,
            Reserved2 = 0x03
        }
        #endregion

        #region Public structures
        public struct PhysicalFormatInformation
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Byte 4, bits 7 to 4
            /// Disk category field
            /// </summary>
            public byte DiskCategory;
            /// <summary>
            /// Byte 4, bits 3 to 0
            /// Media version
            /// </summary>
            public byte PartVersion;
            /// <summary>
            /// Byte 5, bits 7 to 4
            /// 120mm if 0, 80mm if 1. If UMD (60mm) 0 also. Reserved rest of values
            /// </summary>
            public byte DiscSize;
            /// <summary>
            /// Byte 5, bits 3 to 0
            /// Maximum data rate
            /// </summary>
            public byte MaximumRate;
            /// <summary>
            /// Byte 6, bit 7
            /// Reserved
            /// </summary>
            public bool Reserved3;
            /// <summary>
            /// Byte 6, bits 6 to 5
            /// Number of layers
            /// </summary>
            public byte Layers;
            /// <summary>
            /// Byte 6, bit 4
            /// Track path
            /// </summary>
            public bool TrackPath;
            /// <summary>
            /// Byte 6, bits 3 to 0
            /// Layer type
            /// </summary>
            public byte LayerType;
            /// <summary>
            /// Byte 7, bits 7 to 4
            /// Linear density field
            /// </summary>
            public byte LinearDensity;
            /// <summary>
            /// Byte 7, bits 3 to 0
            /// Track density field
            /// </summary>
            public byte TrackDensity;
            /// <summary>
            /// Bytes 8 to 11
            /// PSN where Data Area starts
            /// </summary>
            public UInt32 DataAreaStartPSN;
            /// <summary>
            /// Bytes 12 to 15
            /// PSN where Data Area ends
            /// </summary>
            public UInt32 DataAreaEndPSN;
            /// <summary>
            /// Bytes 16 to 19
            /// PSN where Data Area ends in Layer 0
            /// </summary>
            public UInt32 Layer0EndPSN;
            /// <summary>
            /// Byte 20, bit 7
            /// True if BCA exists. GC/Wii discs do not have this bit set, but there is a BCA, making it unreadable in normal DVD drives
            /// </summary>
            public bool BCA;
            /// <summary>
            /// Byte 20, bits 6 to 0
            /// Reserved
            /// </summary>
            public byte Reserved4;
            /// <summary>
            /// Bytes 21 to 22
            /// UMD only, media attribute, application-defined, part of media specific in rest of discs
            /// </summary>
            public UInt16 MediaAttribute;
            /// <summary>
            /// Bytes 21 to 2051, set to zeroes in UMD (at least according to ECMA).
            /// Media specific
            /// </summary>
            public byte[] MediaSpecific;
        }

        public struct LeadInCopyright
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Byte 4
            /// Copy protection system type
            /// </summary>
            public byte CopyrightType;
            /// <summary>
            /// Byte 5
            /// Bitmask of regions where this disc is playable
            /// </summary>
            public byte RegionInformation;
            /// <summary>
            /// Byte 6
            /// Reserved
            /// </summary>
            public byte Reserved3;
            /// <summary>
            /// Byte 7
            /// Reserved
            /// </summary>
            public byte Reserved4;
        }

        public struct DiscKey
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Bytes 4 to 2052
            /// Disc key for CSS, Album Identifier for CPPM
            /// </summary>
            public byte[] Key;
        }

        public struct BurstCuttingArea
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Bytes 4 to end
            /// Burst cutting area contents, 12 to 188 bytes
            /// </summary>
            public byte[] BCA;
        }

        public struct DiscManufacturingInformation
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Bytes 4 to 2052
            /// Disc Manufacturing Information
            /// </summary>
            public byte[] DMI;
        }

        public struct DiscMediaIdentifier
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Bytes 4 to end
            /// Disc Media Identifier for CPRM
            /// </summary>
            public byte[] MediaIdentifier;
        }

        public struct DiscMediaKeyBlock
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Bytes 4 to end
            /// Disc Media Key Block for CPRM
            /// </summary>
            public byte[] MediaKeyBlock;
        }

        public struct DiscDefinitionStructure
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Bytes 4 to 2052
            /// DVD-RAM / HD DVD-RAM disc definition structure
            /// </summary>
            public byte[] DDS;
        }

        public struct MediumStatus
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Byte 4, bit 7
            /// Medium is in a cartridge
            /// </summary>
            public bool Cartridge;
            /// <summary>
            /// Byte 4, bit 6
            /// Medium has been taken out/inserted in a cartridge
            /// </summary>
            public bool OUT;
            /// <summary>
            /// Byte 4, bits 5 to 4
            /// Reserved
            /// </summary>
            public byte Reserved3;
            /// <summary>
            /// Byte 4, bit 3
            /// Media is write protected by reason stablished in RAMSWI
            /// </summary>
            public bool MSWI;
            /// <summary>
            /// Byte 4, bit 2
            /// Media is write protected by cartridge
            /// </summary>
            public bool CWP;
            /// <summary>
            /// Byte 4, bit 1
            /// Media is persistently write protected
            /// </summary>
            public bool PWP;
            /// <summary>
            /// Byte 4, bit 0
            /// Reserved
            /// </summary>
            public bool Reserved4;
            /// <summary>
            /// Byte 5
            /// Writable status depending on cartridge
            /// </summary>
            public byte DiscType;
            /// <summary>
            /// Byte 6
            /// Reserved
            /// </summary>
            public byte Reserved5;
            /// <summary>
            /// Byte 7
            /// Reason of specific write protection, only defined 0x01 as "bare disc wp", and 0xFF as unspecified. Rest reserved.
            /// </summary>
            public byte RAMSWI;
        }

        public struct SpareAreaInformation
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Bytes 4 to 7
            /// Data length
            /// </summary>
            public UInt32 UnusedPrimaryBlocks;
            /// <summary>
            /// Bytes 8 to 11
            /// Data length
            /// </summary>
            public UInt32 UnusedSupplementaryBlocks;
            /// <summary>
            /// Bytes 12 to 15
            /// Data length
            /// </summary>
            public UInt32 AllocatedSupplementaryBlocks;
        }

        public struct LastBorderOutRMD
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Bytes 4 to end
            /// RMD in last recorded Border-out
            /// </summary>
            public byte[] RMD;
        }

        public struct PreRecordedInformation
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Bytes 4 to end
            /// Pre-recorded Information in Lead-in for writable media
            /// </summary>
            public byte[] PRI;
        }

        public struct UniqueDiscIdentifier
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Byte 4
            /// Reserved
            /// </summary>
            public byte Reserved3;
            /// <summary>
            /// Byte 5
            /// Reserved
            /// </summary>
            public byte Reserved4;
            /// <summary>
            /// Bytes 6 to 7
            /// Random number
            /// </summary>
            public UInt16 RandomNumber;
            /// <summary>
            /// Byte 8 to 11
            /// Year
            /// </summary>
            public UInt32 Year;
            /// <summary>
            /// Byte 12 to 13
            /// Month
            /// </summary>
            public UInt16 Month;
            /// <summary>
            /// Byte 14 to 15
            /// Day
            /// </summary>
            public UInt16 Day;
            /// <summary>
            /// Byte 16 to 17
            /// Hour
            /// </summary>
            public UInt16 Hour;
            /// <summary>
            /// Byte 18 to 19
            /// Minute
            /// </summary>
            public UInt16 Minute;
            /// <summary>
            /// Byte 20 to 21
            /// Second
            /// </summary>
            public UInt16 Second;
        }

        public struct PhysicalFormatInformationForWritables
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Byte 4, bits 7 to 4
            /// Disk category field
            /// </summary>
            public byte DiskCategory;
            /// <summary>
            /// Byte 4, bits 3 to 0
            /// Media version
            /// </summary>
            public byte PartVersion;
            /// <summary>
            /// Byte 5, bits 7 to 4
            /// 120mm if 0, 80mm if 1
            /// </summary>
            public byte DiscSize;
            /// <summary>
            /// Byte 5, bits 3 to 0
            /// Maximum data rate
            /// </summary>
            public byte MaximumRate;
            /// <summary>
            /// Byte 6, bit 7
            /// Reserved
            /// </summary>
            public bool Reserved3;
            /// <summary>
            /// Byte 6, bits 6 to 5
            /// Number of layers
            /// </summary>
            public byte Layers;
            /// <summary>
            /// Byte 6, bit 4
            /// Track path
            /// </summary>
            public bool TrackPath;
            /// <summary>
            /// Byte 6, bits 3 to 0
            /// Layer type
            /// </summary>
            public byte LayerType;
            /// <summary>
            /// Byte 7, bits 7 to 4
            /// Linear density field
            /// </summary>
            public byte LinearDensity;
            /// <summary>
            /// Byte 7, bits 3 to 0
            /// Track density field
            /// </summary>
            public byte TrackDensity;
            /// <summary>
            /// Bytes 8 to 11
            /// PSN where Data Area starts
            /// </summary>
            public UInt32 DataAreaStartPSN;
            /// <summary>
            /// Bytes 12 to 15
            /// PSN where Data Area ends
            /// </summary>
            public UInt32 DataAreaEndPSN;
            /// <summary>
            /// Bytes 16 to 19
            /// PSN where Data Area ends in Layer 0
            /// </summary>
            public UInt32 Layer0EndPSN;
            /// <summary>
            /// Byte 20, bit 7
            /// True if BCA exists
            /// </summary>
            public bool BCA;
            /// <summary>
            /// Byte 20, bits 6 to 0
            /// Reserved
            /// </summary>
            public byte Reserved4;
            /// <summary>
            /// Bytes 21 to 2051
            /// Media specific, content defined in each specification
            /// </summary>
            public byte[] MediaSpecific;
        }

        public struct ADIPInformation
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Bytes 4 to 259
            /// ADIP, defined in DVD standards
            /// </summary>
            public byte[] ADIP;
        }

        public struct HDLeadInCopyright
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Bytes 4 to 2052
            /// HD DVD Lead-In Copyright Information
            /// </summary>
            public byte[] CopyrightInformation;
        }

        public struct HDMediumStatus
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Byte 4, bits 7 to 1
            /// Reserved
            /// </summary>
            public byte Reserved3;
            /// <summary>
            /// Byte 4, bit 0
            /// Test Zone has been extended
            /// </summary>
            public bool ExtendedTestZone;
            /// <summary>
            /// Byte 5
            /// Number of remaining RMDs in RDZ
            /// </summary>
            public byte RemainingRMDs;
            /// <summary>
            /// Bytes 6 to 7
            /// Number of remaining RMDs in current RMZ
            /// </summary>
            public UInt16 CurrentRemainingRMDs;
        }

        public struct LayerCapacity
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Byte 4, bit 7
            /// If set, L0 capacity is immutable
            /// </summary>
            public bool InitStatus;
            /// <summary>
            /// Byte 4, bits 6 to 0
            /// Reserved
            /// </summary>
            public byte Reserved3;
            /// <summary>
            /// Byte 5
            /// Reserved
            /// </summary>
            public byte Reserved4;
            /// <summary>
            /// Byte 6
            /// Reserved
            /// </summary>
            public byte Reserved5;
            /// <summary>
            /// Byte 7
            /// Reserved
            /// </summary>
            public byte Reserved6;
            /// <summary>
            /// Byte 8 to 11
            /// L0 Data Area Capacity
            /// </summary>
            public UInt32 Capacity;
        }

        public struct MiddleZoneStartAddress
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length = 10
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Byte 4, bit 7
            /// If set, L0 shifter middle area is immutable
            /// </summary>
            public bool InitStatus;
            /// <summary>
            /// Byte 4, bits 6 to 0
            /// Reserved
            /// </summary>
            public byte Reserved3;
            /// <summary>
            /// Byte 5
            /// Reserved
            /// </summary>
            public byte Reserved4;
            /// <summary>
            /// Byte 6
            /// Reserved
            /// </summary>
            public byte Reserved5;
            /// <summary>
            /// Byte 7
            /// Reserved
            /// </summary>
            public byte Reserved6;
            /// <summary>
            /// Byte 8 to 11
            /// Start LBA of Shifted Middle Area on L0
            /// </summary>
            public UInt32 ShiftedMiddleAreaStartAddress;
        }

        public struct JumpIntervalSize
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length = 10
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Byte 4
            /// Reserved
            /// </summary>
            public byte Reserved3;
            /// <summary>
            /// Byte 5
            /// Reserved
            /// </summary>
            public byte Reserved4;
            /// <summary>
            /// Byte 6
            /// Reserved
            /// </summary>
            public byte Reserved5;
            /// <summary>
            /// Byte 7
            /// Reserved
            /// </summary>
            public byte Reserved6;
            /// <summary>
            /// Byte 8 to 11
            /// Jump Interval size for the Regular Interval Layer Jump
            /// </summary>
            public UInt32 Size;
        }

        public struct ManualLayerJumpAddress
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length = 10
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Byte 4
            /// Reserved
            /// </summary>
            public byte Reserved3;
            /// <summary>
            /// Byte 5
            /// Reserved
            /// </summary>
            public byte Reserved4;
            /// <summary>
            /// Byte 6
            /// Reserved
            /// </summary>
            public byte Reserved5;
            /// <summary>
            /// Byte 7
            /// Reserved
            /// </summary>
            public byte Reserved6;
            /// <summary>
            /// Byte 8 to 11
            /// LBA for the manual layer jump
            /// </summary>
            public UInt32 LBA;
        }
        #endregion Public structures
    }
}

