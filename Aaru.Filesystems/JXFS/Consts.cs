// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : JXFileSystem plugin.
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
public sealed partial class JXFS
{
    /// <summary>Superblock identifier and DosType, "JXF\4"</summary>
    const uint JXFS_MAGIC = 0x4A584604;
    /// <summary>The only superblock version known</summary>
    const ushort JXFS_VERSION = 0x11;

    /// <summary>Clean journal block, "TROK"</summary>
    const uint TRANSACTION_OK_ID = 0x54524F4B;

    /// <summary>Node container block, "INDC"</summary>
    const uint NODECONTAINER_ID = 0x494E4443;
    /// <summary>Directory container block, "OBJC"</summary>
    const uint OBJECTCONTAINER_ID = 0x4F424A43;
    /// <summary>Small file data block, "DATA"</summary>
    const uint DATA_ID = 0x44415441;
    /// <summary>Attribute block, "ATTR"</summary>
    const uint ATTRIBUTE_ID = 0x41545452;
    /// <summary>Soft link block, "SLNK"</summary>
    const uint SOFTLINK_ID = 0x534C4E4B;
    /// <summary>Extent container block, "EXTC"</summary>
    const uint EXTENTCONTAINER_ID = 0x45585443;

    /// <summary>Seed of the block checksum: the seed plus the sum of all words in the block must be 0</summary>
    const uint CHECKSUM_SEED = 3;

    /// <summary>Block where the journal starts</summary>
    const uint JOURNAL_BLOCK = 1;
    /// <summary>Block where the volume bitmap starts</summary>
    const uint BITMAP_START = 0x800;
    /// <summary>Header bytes of a bitmap block</summary>
    const int BITMAP_HEADER_SIZE = 0x0C;
    /// <summary>Blocks in an extent unit, also blocks per volume bitmap bit</summary>
    const int EXTENT_UNIT_SHIFT = 7;

    /// <summary>Header bytes of a node container block</summary>
    const int NODECONTAINER_HEADER_SIZE = 0x14;
    /// <summary>Bytes in a node</summary>
    const int NODE_SIZE = 0x2C;
    /// <summary>Header bytes of a directory container block</summary>
    const int OBJECTCONTAINER_HEADER_SIZE = 0x20;
    /// <summary>Header bytes of a small file data block</summary>
    const int DATA_HEADER_SIZE = 0x20;
    /// <summary>Header bytes of an attribute block</summary>
    const int ATTRIBUTE_HEADER_SIZE = 0x1C;
    /// <summary>Offset of the target in a soft link block</summary>
    const int SOFTLINK_TARGET_OFFSET = 0x20;
    /// <summary>Header bytes of an extent container block</summary>
    const int EXTENTCONTAINER_HEADER_SIZE = 0x24;
    /// <summary>Bytes in an extent</summary>
    const int EXTENT_SIZE = 6;

    /// <summary>Offset of the block number relative to its system file, in blocks that belong to one</summary>
    const int RELATIVE_BLOCK_OFFSET = 0x0C;
    /// <summary>Offset of the next block in a directory container chain</summary>
    const int OBJECTCONTAINER_NEXT_OFFSET = 0x18;
    /// <summary>Offset of the next block in a small file data block chain</summary>
    const int DATA_NEXT_OFFSET = 0x1C;
    /// <summary>Offset of the next block in an extent container chain</summary>
    const int EXTENTCONTAINER_NEXT_OFFSET = 0x18;
    /// <summary>Offset, from the end of the root directory container, of the free blocks count</summary>
    const int ROOT_FREE_BLOCKS_FROM_END = 0x1C;

    /// <summary>Node of the root directory</summary>
    const uint ROOT_NODE = 1;
    /// <summary>Node of the <c>:node:</c> system file</summary>
    const uint NODE_FILE_NODE = 2;
    /// <summary>Node of the <c>:dir:</c> system file</summary>
    const uint DIR_FILE_NODE = 3;
    /// <summary>Node of the <c>:misc:</c> system file</summary>
    const uint MISC_FILE_NODE = 4;
    /// <summary>Highest node that the filesystem reads straight from the first node container</summary>
    const uint LAST_BOOTSTRAP_NODE = 5;
    /// <summary>Directory entry key of the root directory</summary>
    const uint ROOT_KEY = 0x40;

    /// <summary>Attribute holding the owner user</summary>
    const string ATTRIBUTE_UID = ":uid:";
    /// <summary>Attribute holding the owner group</summary>
    const string ATTRIBUTE_GID = ":gid:";
    /// <summary>Attribute holding the comment</summary>
    const string ATTRIBUTE_COMMENT = ":comment:";
    /// <summary>Offset of a numeric attribute value from the start of its entry</summary>
    const int ATTRIBUTE_NUMBER_OFFSET = 8;
    /// <summary>Offset of the comment from the start of its entry</summary>
    const int ATTRIBUTE_COMMENT_OFFSET = 0x0C;

    /// <summary>The protection on disk is the AmigaDOS one with the RWED bits inverted</summary>
    const uint PROTECTION_INVERTED_BITS = 0x0F;

    /// <summary>Longest name the filesystem accepts</summary>
    const ushort MAX_NAME_LENGTH = 255;

    /// <summary>Upper bound for any chain walk, to stop on corrupted loops</summary>
    const int MAX_CHAIN_LENGTH = 0x1000000;

    const string FS_TYPE = "jxfs";
}