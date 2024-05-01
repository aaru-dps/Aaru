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
//     Contains constants for Sydex TeleDisk disk images.
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

using System.Diagnostics.CodeAnalysis;

namespace Aaru.Images;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class TeleDisk
{
    // "TD" as little endian uint.
    const ushort TD_MAGIC = 0x4454;

    // "td" as little endian uint. Means whole file is compressed (aka Advanced Compression)
    const ushort TD_ADV_COMP_MAGIC = 0x6474;

    // DataRates
    const byte DATA_RATE_250_KBPS = 0x00;
    const byte DATA_RATE_300_KBPS = 0x01;
    const byte DATA_RATE_500_KBPS = 0x02;

    // TeleDisk drive types
    const byte DRIVE_TYPE_525_HD_DD_DISK = 0x00;
    const byte DRIVE_TYPE_525_HD         = 0x01;
    const byte DRIVE_TYPE_525_DD         = 0x02;
    const byte DRIVE_TYPE_35_DD          = 0x03;
    const byte DRIVE_TYPE_35_HD          = 0x04;
    const byte DRIVE_TYPE_8_INCH         = 0x05;
    const byte DRIVE_TYPE_35_ED          = 0x06;

    // Stepping
    const byte STEPPING_SINGLE    = 0x00;
    const byte STEPPING_DOUBLE    = 0x01;
    const byte STEPPING_EVEN_ONLY = 0x02;

    // If this bit is set, there is a comment block
    const byte COMMENT_BLOCK_PRESENT = 0x80;

    // CRC polynomial
    const ushort TELE_DISK_CRC_POLY = 0xA097;

    // Sector sizes table
    const byte SECTOR_SIZE_128 = 0x00;
    const byte SECTOR_SIZE_256 = 0x01;
    const byte SECTOR_SIZE_512 = 0x02;

    // Flags
    // Address mark repeats inside same track
    const byte FLAGS_SECTOR_DUPLICATE = 0x01;

    // Sector gave CRC error on reading
    const byte FLAGS_SECTOR_CRC_ERROR = 0x02;

    // Address mark indicates deleted sector
    const byte FLAGS_SECTOR_DELETED = 0x04;

    // Sector skipped as FAT said it's unused
    const byte FLAGS_SECTOR_SKIPPED = 0x10;

    // There was an address mark, but no data following
    const byte FLAGS_SECTOR_DATALESS = 0x20;

    // There was data without address mark
    const byte FLAGS_SECTOR_NO_ID = 0x40;

    // Data block encodings
    // Data is copied as is
    const byte DATA_BLOCK_COPY = 0x00;

    // Data is encoded as a pair of len.value uint16s
    const byte DATA_BLOCK_PATTERN = 0x01;

    // Data is encoded as RLE
    const byte DATA_BLOCK_RLE = 0x02;

    const int BUFSZ = 512;

    // ReSharper disable InconsistentNaming
    const byte SECTOR_SIZE_1K = 0x03;
    const byte SECTOR_SIZE_2K = 0x04;
    const byte SECTOR_SIZE_4K = 0x05;
    const byte SECTOR_SIZE_8K = 0x06;

    // ReSharper restore InconsistentNaming
}