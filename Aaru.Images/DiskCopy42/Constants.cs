// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Constants.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains constants for Apple DiskCopy 4.2 disk images.
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

// ReSharper disable InconsistentNaming

namespace Aaru.DiscImages;

public sealed partial class DiskCopy42
{
    // format byte
    /// <summary>3.5", single side, double density, GCR</summary>
    const byte kSonyFormat400K = 0x00;
    /// <summary>3.5", double side, double density, GCR</summary>
    const byte kSonyFormat800K = 0x01;
    /// <summary>3.5", double side, double density, MFM</summary>
    const byte kSonyFormat720K = 0x02;
    /// <summary>3.5", double side, high density, MFM</summary>
    const byte kSonyFormat1440K = 0x03;
    /// <summary>3.5", double side, high density, MFM, 21 sectors/track (aka, Microsoft DMF)</summary>

    // Unchecked value</summary>
    const byte kSonyFormat1680K = 0x04;
    /// <summary>Defined by Sigma Seven's BLU</summary>
    const byte kSigmaFormatTwiggy = 0x54;
    /// <summary>Defined by LisaEm</summary>
    const byte kNotStandardFormat = 0x5D;

    // There should be a value for Apple HD20 hard disks, unknown...
    // fmyByte byte
    // Based on GCR nibble
    // Always 0x02 for MFM disks
    // Unknown for Apple HD20
    /// <summary>Defined by Sigma Seven's BLU</summary>
    const byte kSigmaFmtByteTwiggy = 0x01;
    /// <summary>3.5" single side double density GCR and MFM all use same code</summary>
    const byte kSonyFmtByte400K = 0x02;
    const byte kSonyFmtByte720K  = kSonyFmtByte400K;
    const byte kSonyFmtByte1440K = kSonyFmtByte400K;
    const byte kSonyFmtByte1680K = kSonyFmtByte400K;
    /// <summary>3.5" double side double density GCR, 512 bytes/sector, interleave 2:1</summary>
    const byte kSonyFmtByte800K = 0x22;
    /// <summary>
    ///     3.5" double side double density GCR, 512 bytes/sector, interleave 2:1, incorrect value (but appears on
    ///     official documentation)
    /// </summary>
    const byte kSonyFmtByte800KIncorrect = 0x12;
    /// <summary>3.5" double side double density GCR, ProDOS format, interleave 4:1</summary>
    const byte kSonyFmtByteProDos = 0x24;
    /// <summary>Unformatted sectors</summary>
    const byte kInvalidFmtByte = 0x96;
    /// <summary>Defined by LisaEm</summary>
    const byte kFmtNotStandard = 0x93;
    /// <summary>Used incorrectly by Mac OS X with certaing disk images</summary>
    const byte kMacOSXFmtByte = 0x00;
    const string REGEX_DCPY = @"(?<application>\S+)\s(?<version>\S+)\rData checksum=\$(?<checksum>\S+)$";
}