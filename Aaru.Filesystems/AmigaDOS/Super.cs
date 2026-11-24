// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Super.cs
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

using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class AmigaDOSPlugin
{
    /// <inheritdoc />
    public ErrorNumber StatFs(out FileSystemInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Long name volumes cache the number of allocated blocks in the root block, otherwise the bitmap would
        // need to be counted
        ulong freeBlocks = 0;

        if(_hasLongNames                 &&
           _rootBlock.bitmapFlag    != 0 &&
           _rootBlock.numBlocksUsed != 0 &&
           _rootBlock.numBlocksUsed <= _totalBlocks)
            freeBlocks = _totalBlocks - _rootBlock.numBlocksUsed;

        stat = new FileSystemInfo
        {
            Blocks         = _totalBlocks,
            FilenameLength = MAX_NAME_LENGTH,
            FreeBlocks     = freeBlocks,
            FreeFiles      = 0, // AmigaDOS doesn't have a fixed inode count
            PluginId       = Id,
            Type           = _isFfs ? FS_TYPE_FFS : FS_TYPE_OFS
        };

        return ErrorNumber.NoError;
    }
}