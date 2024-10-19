// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains various DVD enumerations.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/


// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global

namespace Aaru.Decoders.DVD;

#region Public enumerations

public enum DiskCategory : byte
{
    /// <summary>DVD-ROM. Version 1 is ECMA-267 and ECMA-268.</summary>
    DVDROM = 0,
    /// <summary>DVD-RAM. Version 1 is ECMA-272. Version 6 is ECMA-330.</summary>
    DVDRAM = 1,
    /// <summary>DVD-R. Version 1 is ECMA-279. Version 5 is ECMA-359. Version 6 is ECMA-382.</summary>
    DVDR = 2,
    /// <summary>DVD-RW. Version 2 is ECMA-338. Version 3 is ECMA-384.</summary>
    DVDRW = 3,
    /// <summary>HD DVD-ROM</summary>
    HDDVDROM = 4,
    /// <summary>HD DVD-RAM</summary>
    HDDVDRAM = 5,
    /// <summary>HD DVD-R</summary>
    HDDVDR = 6,
    /// <summary>HD DVD-RW</summary>
    HDDVDRW = 7,
    /// <summary>UMD. Version 0 is ECMA-365.</summary>
    UMD = 8,
    /// <summary>DVD+RW. Version 1 is ECMA-274. Version 2 is ECMA-337. Version 3 is ECMA-371.</summary>
    DVDPRW = 9,
    /// <summary>DVD+R. Version 1 is ECMA-349.</summary>
    DVDPR = 10,
    /// <summary>DVD+RW DL. Version 1 is ECMA-374.</summary>
    DVDPRWDL = 13,
    /// <summary>DVD+R DL. Version 1 is ECMA-364.</summary>
    DVDPRDL = 14,
    /// <summary>According to standards this value is reserved. It's used by Nintendo GODs and WODs.</summary>
    Nintendo = 15
}

public enum MaximumRateField : byte
{
    /// <summary>2.52 Mbps</summary>
    TwoMbps = 0x00,
    /// <summary>5.04 Mbps</summary>
    FiveMbps = 0x01,
    /// <summary>10.08 Mbps</summary>
    TenMbps = 0x02,
    /// <summary>20.16 Mbps</summary>
    TwentyMbps = 0x03,
    /// <summary>30.24 Mbps</summary>
    ThirtyMbps = 0x04,
    Unspecified = 0x0F
}

public enum LayerTypeFieldMask : byte
{
    Embossed   = 0x01,
    Recordable = 0x02,
    Rewritable = 0x04,
    Reserved   = 0x08
}

public enum LinearDensityField : byte
{
    /// <summary>0.267 μm/bit</summary>
    TwoSix = 0x00,
    /// <summary>0.293 μm/bit</summary>
    TwoNine = 0x01,
    /// <summary>0.409 to 0.435 μm/bit</summary>
    FourZero = 0x02,
    /// <summary>0.280 to 0.291 μm/bit</summary>
    TwoEight = 0x04,
    /// <summary>0.153 μm/bit</summary>
    OneFive = 0x05,
    /// <summary>0.130 to 0.140 μm/bit</summary>
    OneThree = 0x06,
    /// <summary>0.353 μm/bit</summary>
    ThreeFive = 0x08
}

public enum TrackDensityField : byte
{
    /// <summary>0.74 μm/track</summary>
    Seven = 0x00,
    /// <summary>0.80 μm/track</summary>
    Eight = 0x01,
    /// <summary>0.615 μm/track</summary>
    Six = 0x02,
    /// <summary>0.40 μm/track</summary>
    Four = 0x03,
    /// <summary>0.34 μm/track</summary>
    Three = 0x04
}

public enum CopyrightType : byte
{
    /// <summary>There is no copy protection</summary>
    NoProtection = 0x00,
    /// <summary>Copy protection is CSS/CPPM</summary>
    CSS = 0x01,
    /// <summary>Copy protection is CPRM</summary>
    CPRM = 0x02,
    /// <summary>Copy protection is AACS</summary>
    AACS = 0x10
}

public enum WPDiscTypes : byte
{
    /// <summary>Should not write without a cartridge</summary>
    DoNotWrite = 0x00,
    /// <summary>Can write without a cartridge</summary>
    CanWrite = 0x01,
    Reserved1 = 0x02,
    Reserved2 = 0x03
}

public enum DVDSize
{
    /// <summary>120 mm</summary>
    OneTwenty = 0,
    /// <summary>80 mm</summary>
    Eighty = 1
}

public enum DVDRAMDiscType
{
    /// <summary>Shall not be recorded without a case</summary>
    Cased = 0,
    /// <summary>May be recorded without a case or within one</summary>
    Uncased = 1
}

public enum DVDLayerStructure
{
    Unspecified   = 0,
    InvertedStack = 1,
    TwoP          = 2,
    Reserved      = 3
}

public enum DVDRecordingSpeed
{
    None   = 0,
    Two    = 0,
    Four   = 0x10,
    Six    = 0x20,
    Eight  = 0x30,
    Ten    = 0x40,
    Twelve = 0x50
}

#endregion