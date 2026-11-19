// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : MiniDisc File System plugin.
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

namespace Aaru.Filesystems;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class MDFS
{
    const string FS_TYPE = "mdfs";

    /// <summary>MD DATA logical block size</summary>
    const uint BLOCK_SIZE = 2048;
    /// <summary>Volume descriptor location on every disc formatted by Sony's software, data track start + 32</summary>
    const ulong DEFAULT_VD_LBA = 0x420;
    /// <summary>Offset from the data track start to the volume descriptor</summary>
    const uint VD_OFFSET_FROM_DATA_TRACK = 0x20;
    /// <summary>Volume descriptor version</summary>
    const byte VD_VERSION = 1;

    /// <summary>Mirrored volume management area: 16 sectors, then their 16 copies</summary>
    const int MIRROR_RUN = 16;

    const int MAX_FILENAME_LENGTH = 255;
    const int MAX_ERB_DEPTH       = 8;

    /// <summary>Length of the Mac OS extension that follows the name: length word, type, version, Finder info</summary>
    const ushort MAC_EXTENSION_LENGTH = 0x24;
    const int FINDER_INFO_LENGTH = 32;

    // Directory record attributes, names from Sony's MDfsck
    /// <summary>D: directory</summary>
    const ushort ATTR_DIRECTORY = 0x0001;
    /// <summary>V: invisible</summary>
    const ushort ATTR_INVISIBLE = 0x0002;
    /// <summary>S: system</summary>
    const ushort ATTR_SYSTEM = 0x0004;
    /// <summary>T: deleted record</summary>
    const ushort ATTR_DELETED = 0x0008;
    /// <summary>P: protected</summary>
    const ushort ATTR_PROTECT = 0x0040;
    /// <summary>B: needs backup</summary>
    const ushort ATTR_BACKUP = 0x0080;
    /// <summary>L: cannot be deleted</summary>
    const ushort ATTR_INHIBIT_DELETE = 0x0100;
    /// <summary>R: cannot be renamed</summary>
    const ushort ATTR_INHIBIT_RENAME = 0x0200;
    /// <summary>C: cannot be copied</summary>
    const ushort ATTR_INHIBIT_COPY = 0x0400;
    /// <summary>E: extent in an unknown format, records with it are ignored</summary>
    const ushort ATTR_UNKNOWN_EXTENT = 0x2000;
    /// <summary>F: data fork location is an extent record id</summary>
    const ushort ATTR_DATA_EXTENT_RECORD = 0x4000;
    /// <summary>A: resource fork location is an extent record id</summary>
    const ushort ATTR_RESOURCE_EXTENT_RECORD = 0x8000;

    // Volume attributes
    const ushort VOL_MIRROR      = 0x0001;
    const ushort VOL_INVISIBLE   = 0x0002;
    const ushort VOL_PROTECT     = 0x0040;
    const ushort VOL_BACKUP      = 0x0080;
    const ushort VOL_INH_FORMAT  = 0x0100;
    const ushort VOL_INH_RENAME  = 0x0200;
    const ushort VOL_INH_COPY    = 0x0400;
    const ushort VOL_EXTENT_32   = 0x8000;

    // Management table tags, in the top byte
    const byte MTB_DIRECTORY_FIRST  = 0xD0;
    const byte MTB_DIRECTORY_MIDDLE = 0xE0;

    // Character set codes of names
    const byte CSC_WINDOWS = 1;
    const byte CSC_MAC     = 3;

    // Directory record field offsets
    const int REC_LENGTH            = 0x00;
    const int REC_ATTRIBUTES        = 0x02;
    const int REC_CHARSET           = 0x04;
    const int REC_FCB               = 0x05;
    const int REC_CREATION          = 0x10;
    const int REC_MODIFICATION      = 0x14;
    const int REC_STATUS            = 0x18;
    const int REC_ID                = 0x1C;
    const int REC_LENGTH_BYTES      = 0x20;
    const int REC_DIR_LOCATION      = 0x24;
    const int REC_DATA_LOCATION     = 0x24;
    const int REC_DATA_COUNT        = 0x28;
    const int REC_RESOURCE_LENGTH   = 0x2C;
    const int REC_RESOURCE_LOCATION = 0x30;
    const int REC_RESOURCE_COUNT    = 0x34;
    const int REC_DIR_NAME_LENGTH   = 0x28;
    const int REC_FILE_NAME_LENGTH  = 0x38;
    const int FCB_LENGTH            = 11;

    /// <summary>"\0MD001"</summary>
    static readonly byte[] _signature = [0x00, 0x4D, 0x44, 0x30, 0x30, 0x31];
    /// <summary>P-TOC signatures, "MINI" for audio and "MINX" for MD DATA discs</summary>
    static readonly byte[] _ptocAudio = "MINI"u8.ToArray();
    static readonly byte[] _ptocData  = "MINX"u8.ToArray();
}