// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : EWF logical evidence plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains constants for Expert Witness Format logical evidence files.
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

using System.Diagnostics.CodeAnalysis;

namespace Aaru.Archives;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class EwfArchive
{
    /// <summary>EWF v1 logical evidence signature: "LVF\x09\x0D\x0A\xFF\x00"</summary>
    static readonly byte[] LVF_SIGNATURE = [0x4C, 0x56, 0x46, 0x09, 0x0D, 0x0A, 0xFF, 0x00];

    /// <summary>EWF v2 logical evidence signature: "LEF2\x0D\x0A\x81\x00"</summary>
    static readonly byte[] LEF2_SIGNATURE = [0x4C, 0x45, 0x46, 0x32, 0x0D, 0x0A, 0x81, 0x00];

    /// <summary>EWF v1 disk image signature (needed for segment traversal)</summary>
    static readonly byte[] EVF_SIGNATURE = [0x45, 0x56, 0x46, 0x09, 0x0D, 0x0A, 0xFF, 0x00];

    /// <summary>EWF v2 disk image signature (needed for segment traversal)</summary>
    static readonly byte[] EVF2_SIGNATURE = [0x45, 0x56, 0x46, 0x32, 0x0D, 0x0A, 0x81, 0x00];

    const int SIGNATURE_LENGTH = 8;

    const string SECTION_TYPE_HEADER  = "header";
    const string SECTION_TYPE_HEADER2 = "header2";
    const string SECTION_TYPE_VOLUME  = "volume";
    const string SECTION_TYPE_DATA    = "data";
    const string SECTION_TYPE_TABLE   = "table";
    const string SECTION_TYPE_TABLE2  = "table2";
    const string SECTION_TYPE_SECTORS = "sectors";
    const string SECTION_TYPE_LTREE   = "ltree";
    const string SECTION_TYPE_LTYPES  = "ltypes";
    const string SECTION_TYPE_DONE    = "done";
    const string SECTION_TYPE_NEXT    = "next";

    const int SECTION_DESCRIPTOR_V1_SIZE = 76;
    const int FILE_HEADER_V1_SIZE        = 13;
    const int FILE_HEADER_V2_SIZE        = 32;

    const uint DEFAULT_SECTORS_PER_CHUNK = 64;
    const uint DEFAULT_BYTES_PER_SECTOR  = 512;
    const uint DEFAULT_CHUNK_SIZE        = DEFAULT_SECTORS_PER_CHUNK * DEFAULT_BYTES_PER_SECTOR;

    const uint TABLE_ENTRY_V1_COMPRESSED_FLAG = 0x80000000;
    const uint TABLE_ENTRY_V1_OFFSET_MASK     = 0x7FFFFFFF;

    const int VOLUME_SECTION_SIZE_ENCASE = 1052;
    const int VOLUME_SECTION_SIZE_SMART  = 94;

    const int MAX_CACHE_SIZE = 16777216;

    /// <summary>Ltree header size in bytes</summary>
    const int LTREE_HEADER_SIZE = 48;

    /// <summary>Maximum accepted decompressed ltree size, 256 MiB</summary>
    const uint MAX_LTREE_SIZE = 268435456;
}