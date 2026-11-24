// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SmartFileSystem plugin.
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
public sealed partial class SFS
{
    /// <summary>Identifier for SFS v1</summary>
    const uint SFS_MAGIC = 0x53465300;
    /// <summary>Identifier for SFS v2</summary>
    const uint SFS2_MAGIC = 0x53465302;

    /// <summary>Object container block identifier ('OBJC')</summary>
    const uint OBJECTCONTAINER_ID = 0x4F424A43;
    /// <summary>Hash table block identifier ('HTAB')</summary>
    const uint HASHTABLE_ID = 0x48544142;
    /// <summary>Soft link block identifier ('SLNK')</summary>
    const uint SOFTLINK_ID = 0x534C4E4B;
    /// <summary>Node container block identifier ('NDC ')</summary>
    const uint NODECONTAINER_ID = 0x4E444320;
    /// <summary>Bitmap block identifier ('BITM')</summary>
    const uint BITMAP_ID = 0x4249544D;
    /// <summary>B-tree node container block identifier ('BNDC')</summary>
    const uint BNODECONTAINER_ID = 0x424E4443;

    /// <summary>Node number of the root directory object</summary>
    const uint ROOTNODE = 1;
    /// <summary>Node number of the recycled directory object</summary>
    const uint RECYCLEDNODE = 2;

    /// <summary>Structure version for SFS\0</summary>
    const ushort STRUCTURE_VERSION_SFS0 = 3;
    /// <summary>Structure version for SFS\2</summary>
    const ushort STRUCTURE_VERSION_SFS2 = 4;

    /// <summary>Size of the fixed part of an Object structure in SFS\0 (before name and comment)</summary>
    const int OBJECT_SIZE_SFS0 = 25;
    /// <summary>Size of the fixed part of an Object structure in SFS\2 (before name and comment)</summary>
    const int OBJECT_SIZE_SFS2 = 27;

    /// <summary>Size of ExtentBNode in SFS\0 (14 bytes - 16-bit blocks field)</summary>
    const int EXTENTBNODE_SIZE_SFS0 = 14;
    /// <summary>Size of ExtentBNode in SFS\2 (16 bytes - 32-bit blocks field)</summary>
    const int EXTENTBNODE_SIZE_SFS2 = 16;

    /// <summary>Initial value of the block checksum in SFS\0</summary>
    const uint CHECKSUM_SEED_SFS0 = 1;
    /// <summary>Initial value of the block checksum in SFS\2</summary>
    const uint CHECKSUM_SEED_SFS2 = 2;

    const string FS_TYPE = "sfs";
}