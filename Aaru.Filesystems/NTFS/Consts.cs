// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft NT File System plugin.
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

public sealed partial class NTFS
{
    const string FS_TYPE = "ntfs";

    // Default directory index name ($I30 = filename index)
    const string INDEX_NAME = "$I30";

    // Windows reparse point tags
    const uint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;
    const uint IO_REPARSE_TAG_SYMLINK     = 0xA000000C;

    // Data Deduplication reparse tag
    const uint IO_REPARSE_TAG_DEDUP = 0x80000013;

    // Windows Overlay Filter reparse tag
    const uint IO_REPARSE_TAG_WOF = 0x80000017;

    // WSL reparse point tags
    const uint IO_REPARSE_TAG_AF_UNIX    = 0x80000023;
    const uint IO_REPARSE_TAG_LX_FIFO    = 0x80000024;
    const uint IO_REPARSE_TAG_LX_CHR     = 0x80000025;
    const uint IO_REPARSE_TAG_LX_BLK     = 0x80000026;
    const uint IO_REPARSE_TAG_LX_SYMLINK = 0xA000001D;

    // WOF compression algorithms
    const uint WOF_COMPRESSION_XPRESS4K  = 0;
    const uint WOF_COMPRESSION_LZX32K    = 1;
    const uint WOF_COMPRESSION_XPRESS8K  = 2;
    const uint WOF_COMPRESSION_XPRESS16K = 3;

    // WOF provider constants
    const uint WOF_CURRENT_VERSION          = 1;
    const uint WOF_PROVIDER_SYSTEM          = 2;
    const uint WOF_PROVIDER_CURRENT_VERSION = 1;

    // WofCompressedData named stream
    const string WOF_COMPRESSED_DATA_STREAM = "WofCompressedData";

    /// <summary>Maximum number of bytes of contiguous clusters read from the image at once</summary>
    const long MAX_CONTIGUOUS_READ = 1048576;
}