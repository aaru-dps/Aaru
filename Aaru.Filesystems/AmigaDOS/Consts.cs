// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Amiga Fast File System plugin.
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

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class AmigaDOSPlugin
{
    const uint FFS_MASK  = 0x444F5300;
    const uint MUFS_MASK = 0x6D754600;

    const uint TYPE_HEADER  = 2;
    const uint TYPE_DATA    = 8;
    const uint TYPE_LIST    = 16;
    const uint SUBTYPE_ROOT = 1;
    const uint SUBTYPE_DIR  = 2;
    const int  SUBTYPE_FILE = -3;

    /// <summary>Maximum filename length for AmigaDOS</summary>
    const int MAX_NAME_LENGTH = 30;

    /// <summary>Maximum filename length for long name volumes (DOS\6 and DOS\7)</summary>
    const int MAX_LONG_NAME_LENGTH = 107;

    /// <summary>Maximum comment length</summary>
    const int MAX_COMMENT_LENGTH = 79;

    /// <summary>Primary type of a comment block (DOS\6 and DOS\7)</summary>
    const uint TYPE_COMMENT = 64;

    /// <summary>Bytes from the end of a header block to the comment, or to the merged name and comment</summary>
    const int ENTRY_COMMENT_FROM_END = 184;

    /// <summary>Bytes from the end of a header block to the name</summary>
    const int ENTRY_NAME_FROM_END = 80;

    /// <summary>Bytes from the end of a header block to the date stamp</summary>
    const int ENTRY_DATE_FROM_END = 92;

    /// <summary>Size of the merged name and comment field of long name header blocks</summary>
    const int LONG_NAME_AND_COMMENT_SIZE = 112;

    /// <summary>Bytes from the end of a long name header block to the comment block pointer</summary>
    const int LONG_COMMENT_BLOCK_FROM_END = 72;

    /// <summary>Bytes from the end of a long name header block to the date stamp</summary>
    const int LONG_DATE_FROM_END = 60;

    /// <summary>Offset of the comment in a comment block</summary>
    const int COMMENT_BLOCK_TEXT_OFFSET = 24;

    /// <summary>OFS data block header size in bytes</summary>
    const int OFS_DATA_HEADER_SIZE = 24;

    /// <summary>Block table start offset (in longs)</summary>
    const int BLK_TABLE_START = 6;

    const string FS_TYPE_OFS  = "aofs";
    const string FS_TYPE_FFS  = "affs";
    const string FS_TYPE_OFS2 = "aofs2";
    const string FS_TYPE_FFS2 = "affs2";
}