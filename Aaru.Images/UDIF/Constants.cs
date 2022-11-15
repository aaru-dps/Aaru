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
//     Contains constants for Apple Universal Disk Image Format.
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

using System.Diagnostics.CodeAnalysis;

namespace Aaru.DiscImages;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class Udif
{
    const uint UDIF_SIGNATURE  = 0x6B6F6C79;
    const uint CHUNK_SIGNATURE = 0x6D697368;

    // All chunk types with this mask are compressed
    const uint CHUNK_TYPE_COMPRESSED_MASK = 0x80000000;

    const uint CHUNK_TYPE_ZERO    = 0x00000000;
    const uint CHUNK_TYPE_COPY    = 0x00000001;
    const uint CHUNK_TYPE_NOCOPY  = 0x00000002;
    const uint CHUNK_TYPE_KENCODE = 0x80000001;
    const uint CHUNK_TYPE_RLE     = 0x80000002;
    const uint CHUNK_TYPE_LZH     = 0x80000003;
    const uint CHUNK_TYPE_ADC     = 0x80000004;
    const uint CHUNK_TYPE_ZLIB    = 0x80000005;
    const uint CHUNK_TYPE_BZIP    = 0x80000006;
    const uint CHUNK_TYPE_LZFSE   = 0x80000007;
    const uint CHUNK_TYPE_LZMA    = 0x80000008;
    const uint CHUNK_TYPE_COMMNT  = 0x7FFFFFFE;
    const uint CHUNK_TYPE_END     = 0xFFFFFFFF;

    const uint UDIF_CHECKSUM_TYPE_NONE   = 0;
    const uint UDIF_CHECKSUM_TYPE_CRC28  = 1;
    const uint UDIF_CHECKSUM_TYPE_CRC32  = 2;
    const uint UDIF_CHECKSUM_TYPE_DC42   = 3;
    const uint UDIF_CHECKSUM_TYPE_MD5    = 4;
    const uint UDIF_CHECKSUM_TYPE_SHA    = 5;
    const uint UDIF_CHECKSUM_TYPE_SHA1   = 6;
    const uint UDIF_CHECKSUM_TYPE_SHA256 = 7;
    const uint UDIF_CHECKSUM_TYPE_SHA384 = 8;
    const uint UDIF_CHECKSUM_TYPE_SHA512 = 9;

    const string RESOURCE_FORK_KEY  = "resource-fork";
    const string BLOCK_KEY          = "blkx";
    const uint   BLOCK_OS_TYPE      = 0x626C6B78;
    const uint   MAX_CACHE_SIZE     = 16777216;
    const uint   SECTOR_SIZE        = 512;
    const uint   MAX_CACHED_SECTORS = MAX_CACHE_SIZE / SECTOR_SIZE;
}