// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Tar plugin.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Archives;

public sealed partial class Tar
{
    const int BLOCK_SIZE = 512;

    /// <summary>Maximum Unix timestamp convertible by <see cref="System.DateTimeOffset.FromUnixTimeSeconds" /></summary>
    const long MAX_UNIX_TIME = 253402300799;

    // Header field offsets and sizes
    const int NAME_OFFSET     = 0;
    const int NAME_LENGTH     = 100;
    const int MODE_OFFSET     = 100;
    const int MODE_LENGTH     = 8;
    const int UID_OFFSET      = 108;
    const int UID_LENGTH      = 8;
    const int GID_OFFSET      = 116;
    const int GID_LENGTH      = 8;
    const int SIZE_OFFSET     = 124;
    const int SIZE_LENGTH     = 12;
    const int MTIME_OFFSET    = 136;
    const int MTIME_LENGTH    = 12;
    const int CHECKSUM_OFFSET = 148;
    const int CHECKSUM_LENGTH = 8;
    const int TYPEFLAG_OFFSET = 156;
    const int LINKNAME_OFFSET = 157;
    const int LINKNAME_LENGTH = 100;
    const int MAGIC_OFFSET    = 257;
    const int MAGIC_LENGTH    = 6;
    const int VERSION_OFFSET  = 263;
    const int VERSION_LENGTH  = 2;
    const int UNAME_OFFSET    = 265;
    const int UNAME_LENGTH    = 32;
    const int GNAME_OFFSET    = 297;
    const int GNAME_LENGTH    = 32;
    const int DEVMAJOR_OFFSET = 329;
    const int DEVMAJOR_LENGTH = 8;
    const int DEVMINOR_OFFSET = 337;
    const int DEVMINOR_LENGTH = 8;
    const int PREFIX_OFFSET   = 345;
    const int PREFIX_LENGTH   = 155;

    // GNU sparse header offsets within the main header
    const int SPARSE_MAP_OFFSET      = 386;
    const int SPARSE_MAP_ENTRY_SIZE  = 24;
    const int SPARSE_MAP_ENTRIES     = 4;
    const int SPARSE_EXTENDED_OFFSET = 482;
    const int SPARSE_REALSIZE_OFFSET = 483;
    const int SPARSE_REALSIZE_LENGTH = 12;

    // Extended sparse block (512 bytes): 21 entries of 24 bytes + 1 byte extended flag at offset 504
    const int SPARSE_EXTENDED_ENTRIES     = 21;
    const int SPARSE_EXTENDED_FLAG_OFFSET = 504;

    // STAR magic at offset 508
    const int STAR_MAGIC_OFFSET = 508;
    const int STAR_MAGIC_LENGTH = 4;

    // GNU long name/link sentinel filename
    const string GNU_LONGLINK = "././@LongLink";

    // Magic values
    static readonly byte[] USTAR_MAGIC = "ustar\0"u8.ToArray();
    static readonly byte[] GNU_MAGIC   = "ustar  \0"u8.ToArray();
    static readonly byte[] STAR_MAGIC  = "tar\0"u8.ToArray();
}