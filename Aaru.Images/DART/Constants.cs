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
//     Contains constants for Apple Disk Archival/Retrieval Tool format.
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

using System.Diagnostics.CodeAnalysis;

namespace Aaru.DiscImages;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class Dart
{
    // Disk types
    const byte DISK_MAC    = 1;
    const byte DISK_LISA   = 2;
    const byte DISK_APPLE2 = 3;
    const byte DISK_MAC_HD = 16;
    const byte DISK_DOS    = 17;
    const byte DISK_DOS_HD = 18;

    // Compression types
    // "fast"
    const byte COMPRESS_RLE = 0;

    // "best"
    const byte COMPRESS_LZH = 1;

    // DART <= 1.4
    const byte COMPRESS_NONE = 2;

    // Valid sizes
    const short SIZE_LISA   = 400;
    const short SIZE_MAC_SS = 400;
    const short SIZE_MAC    = 800;
    const short SIZE_MAC_HD = 1440;
    const short SIZE_APPLE2 = 800;
    const short SIZE_DOS    = 720;
    const short SIZE_DOS_HD = 1440;

    // bLength array sizes
    const int BLOCK_ARRAY_LEN_LOW  = 40;
    const int BLOCK_ARRAY_LEN_HIGH = 72;

    const int SECTORS_PER_BLOCK = 40;
    const int SECTOR_SIZE       = 512;
    const int TAG_SECTOR_SIZE   = 12;
    const int DATA_SIZE         = SECTORS_PER_BLOCK * SECTOR_SIZE;
    const int TAG_SIZE          = SECTORS_PER_BLOCK * TAG_SECTOR_SIZE;
    const int BUFFER_SIZE       = (SECTORS_PER_BLOCK * SECTOR_SIZE) + (SECTORS_PER_BLOCK * TAG_SECTOR_SIZE);

    const string DART_REGEX =
        @"(?<version>\S+), tag checksum=\$(?<tagchk>[0123456789ABCDEF]{8}), data checksum=\$(?<datachk>[0123456789ABCDEF]{8})$";
}