// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : High Performance Optical File System plugin.
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

public sealed partial class HPOFS
{
    // Do not translate
    const string FS_TYPE = "hpofs";

    /// <summary>LBA of the media information block (MEDINFO or SMISUBCL)</summary>
    const ulong MEDINFO_SECTOR = 13;
    /// <summary>LBA of the volume information block (VOLINFO)</summary>
    const ulong VOLINFO_SECTOR = 14;
    /// <summary>LBA of the copy of the boot area, at boot area + 0x7F</summary>
    const ulong BACKUP_AREA_OFFSET = 0x7F;
    /// <summary>LBA of the file record of the directory system file</summary>
    const ulong DIRECTORY_RECORD_SECTOR = 6;
    /// <summary>LBA of the file record of the free space system file</summary>
    const ulong FREEFILE_RECORD_SECTOR = 7;

    /// <summary>Size of a file record header</summary>
    const int FILE_RECORD_SIZE = 0x40;
    /// <summary>Size of a subfile descriptor</summary>
    const int SUBFILE_DESCRIPTOR_SIZE = 0x34;
    /// <summary>Size of a B-tree node header</summary>
    const int NODE_HEADER_SIZE = 0x24;
    /// <summary>Size of an extent list block header</summary>
    const int EXTENT_LIST_HEADER_SIZE = 0x20;
    /// <summary>Size of an extent descriptor</summary>
    const int EXTENT_SIZE = 8;
    /// <summary>Size of an extended attribute descriptor, without name</summary>
    const int EA_DESCRIPTOR_SIZE = 0x14;
    /// <summary>Size of the "EALSUBCL" header</summary>
    const int EA_HEADER_SIZE = 0x0C;

    /// <summary>Subfile holding file data</summary>
    const int SUBFILE_DATA = 1;
    /// <summary>Subfile holding extended attributes</summary>
    const int SUBFILE_EA = 2;

    /// <summary>Mask of the number of subfiles in <see cref="FileRecord.flags" /></summary>
    const ushort FLAGS_SUBFILES_MASK = 0x6000;
    /// <summary>Shift of the number of subfiles in <see cref="FileRecord.flags" /></summary>
    const int FLAGS_SUBFILES_SHIFT = 13;

    /// <summary>48-bit LBA meaning "no extent"</summary>
    const ulong NO_LBA = 0xFFFFFFFFFFFF;

    /// <summary>Byte that separates components of a path in directory keys, ASCII and EBCDIC volumes alike</summary>
    const byte PATH_SEPARATOR = 0x2F;
    /// <summary>Prefix of system file names in the directory</summary>
    const byte SYSTEM_NAME_PREFIX = 0x0F;

    /// <summary>Codepage type for EBCDIC volumes</summary>
    const ushort CODEPAGE_TYPE_EBCDIC = 2;

    /// <summary>RPS level of WORM volumes, names stored uppercase</summary>
    const uint RPS_WORM = 1;
    /// <summary>RPS level of rewritable volumes, names stored uppercase</summary>
    const uint RPS_REWRITABLE = 10;
    /// <summary>RPS level of WORM volumes, mixed case names</summary>
    const uint RPS_WORM_MIXED_CASE = 257;
    /// <summary>RPS level of rewritable volumes, mixed case names</summary>
    const uint RPS_REWRITABLE_MIXED_CASE = 266;

    /// <summary>Flag in <see cref="FileRecord.flags" /> marking a deleted entry on WORM volumes</summary>
    const ushort FLAGS_DELETED = 0x0400;

    /// <summary>Offset in the SMI block where <see cref="MediaInformationBlock.rest" /> starts</summary>
    const int SMI_REST_OFFSET = 0xFC;
    /// <summary>Offset in the SMI block of the path separator character</summary>
    const int SMI_SEPARATOR_OFFSET = 0x109;
    /// <summary>Offset in the SMI block of the LBA of the primary VMI block</summary>
    const int SMI_VMI_PRIMARY_OFFSET = 0x188;
    /// <summary>Offset in the SMI block of the LBA of the backup VMI block</summary>
    const int SMI_VMI_BACKUP_OFFSET = 0x190;
    /// <summary>Offset in the SMI block of the number of sectors in a directory segment</summary>
    const int SMI_SEGMENT_SECTORS_OFFSET = 0x19A;
    /// <summary>Offset in the VMI block of the volume owner</summary>
    const int VMI_OWNER_OFFSET = 0x48;
    /// <summary>Offset in the VMI block of the maximum percentage of the volume that can be filled</summary>
    const int VMI_PERCENT_FULL_OFFSET = 0x7C;
    /// <summary>Offset in the VMI block of the next sector to be written</summary>
    const int VMI_NEXT_WRITE_OFFSET = 0xF4;
    /// <summary>Offset in the VMI block of the LBA of the segment holding the root directory entry</summary>
    const int VMI_ROOT_ANCHOR_OFFSET = 0x104;
    /// <summary>Offset of the first directory entry in the first sector of a WORM directory segment</summary>
    const int SEGMENT_FIRST_ENTRY_OFFSET = 0x18;
    /// <summary>Offset of the directory entry in the other sectors of a WORM directory segment</summary>
    const int SEGMENT_ENTRY_OFFSET = 4;
    /// <summary>Size of the link to the next segment, at the end of the last sector of a full segment</summary>
    const int SEGMENT_LINK_SIZE = 8;
    /// <summary>Value preceding the link to the next segment</summary>
    const ushort SEGMENT_LINK_MARKER = 0x01F8;
    /// <summary>Size of a WORM directory entry header, signature to name length</summary>
    const int WORM_ENTRY_HEADER_SIZE = 0x16;

    /// <summary>Maximum length of a path component, from HPOFSDOC.TXT</summary>
    const int MAX_FILENAME_LENGTH = 254;

    /// <summary>Xattr prefix for OS/2 extended attributes, as used by the HPFS and FAT plugins</summary>
    const string EA_PREFIX_OS2 = "com.microsoft.os2";

    /// <summary>Start of the primary and backup copies of the boot area</summary>
    readonly ulong[] _superblockAreas = [0, BACKUP_AREA_OFFSET];

    readonly byte[] _dataSignature    = "DATA"u8.ToArray();
    readonly byte[] _eaSignature      = "EALSUBCL"u8.ToArray();
    readonly byte[] _mastSignature    = "MAST"u8.ToArray();
    readonly byte[] _entrySignature   = "DESSTR"u8.ToArray();
    readonly byte[] _medinfoSignature = "MEDINFO "u8.ToArray();
    readonly byte[] _segmentSignature = "DSS-SEG "u8.ToArray();
    readonly byte[] _smiSignature     = "SMISUBCL"u8.ToArray();
    readonly byte[] _type             = "HPOFS\0\0\0"u8.ToArray();
    readonly byte[] _vmiSignature     = "VMISUBCL"u8.ToArray();
    readonly byte[] _volinfoSignature = "VOLINFO "u8.ToArray();

    /// <summary>Signatures of extent list blocks, indexed by subfile number - 1</summary>
    readonly byte[][] _extentListSignatures = ["SUBF1EXT"u8.ToArray(), "SUBF2EXT"u8.ToArray(), "SUBF3EXT"u8.ToArray()];
}