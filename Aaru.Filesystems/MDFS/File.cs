// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
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

using System;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;

namespace Aaru.Filesystems;

public sealed partial class MDFS
{
    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        ErrorNumber errno = GetEntry(path, out string _, out Entry entry);

        if(errno != ErrorNumber.NoError) return errno;

        stat = new FileEntryInfo
        {
            Attributes = entry.IsDirectory ? FileAttributes.Directory : FileAttributes.File,
            Inode      = entry.Id,
            Length     = entry.Length,
            Links      = 1
        };

        if(entry.IsDirectory)
        {
            stat.BlockSize = BLOCK_SIZE;
            stat.Blocks    = entry.Length / BLOCK_SIZE;
        }
        else
        {
            stat.BlockSize = _vd.AllocationUnitBytes;
            stat.Blocks    = entry.DataCount;
        }

        stat.CreationTime     = ToDateTime(entry.CreationTime);
        stat.LastWriteTime    = ToDateTime(entry.ModificationTime);
        stat.StatusChangeTime = ToDateTime(entry.StatusTime);

        if((entry.Attributes & ATTR_INVISIBLE)      != 0) stat.Attributes |= FileAttributes.Hidden;
        if((entry.Attributes & ATTR_SYSTEM)         != 0) stat.Attributes |= FileAttributes.System;
        if((entry.Attributes & ATTR_PROTECT)        != 0) stat.Attributes |= FileAttributes.ReadOnly;
        if((entry.Attributes & ATTR_BACKUP)         != 0) stat.Attributes |= FileAttributes.Archive;
        if((entry.Attributes & ATTR_INHIBIT_DELETE) != 0) stat.Attributes |= FileAttributes.Undeletable;

        if(entry.FinderInfo is null) return ErrorNumber.NoError;

        // Same position for files (FInfo.fdFlags) and directories (DInfo.frFlags)
        AppleCommon.FinderFlags finderFlags = (AppleCommon.FinderFlags)BigEndianBitConverter.ToUInt16(entry.FinderInfo, 8);

        if(finderFlags.HasFlag(AppleCommon.FinderFlags.kIsAlias)) stat.Attributes |= FileAttributes.Alias;

        if(finderFlags.HasFlag(AppleCommon.FinderFlags.kHasBundle)) stat.Attributes |= FileAttributes.Bundle;

        if(finderFlags.HasFlag(AppleCommon.FinderFlags.kHasBeenInited))
            stat.Attributes |= FileAttributes.HasBeenInited;

        if(finderFlags.HasFlag(AppleCommon.FinderFlags.kHasCustomIcon))
            stat.Attributes |= FileAttributes.HasCustomIcon;

        if(finderFlags.HasFlag(AppleCommon.FinderFlags.kHasNoINITs)) stat.Attributes |= FileAttributes.HasNoINITs;

        if(finderFlags.HasFlag(AppleCommon.FinderFlags.kIsInvisible)) stat.Attributes |= FileAttributes.Hidden;

        if(finderFlags.HasFlag(AppleCommon.FinderFlags.kIsOnDesk)) stat.Attributes |= FileAttributes.IsOnDesk;

        if(finderFlags.HasFlag(AppleCommon.FinderFlags.kIsShared)) stat.Attributes |= FileAttributes.Shared;

        if(finderFlags.HasFlag(AppleCommon.FinderFlags.kIsStationery)) stat.Attributes |= FileAttributes.Stationery;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber OpenFile(string path, out IFileNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        ErrorNumber errno = GetEntry(path, out string _, out Entry entry);

        if(errno != ErrorNumber.NoError) return errno;

        if(entry.IsDirectory) return ErrorNumber.IsDirectory;

        errno = GetDataExtents(entry, out Extent[] extents);

        if(errno != ErrorNumber.NoError) return errno;

        node = new MdfsFileNode
        {
            Path    = path,
            Length  = entry.Length,
            Offset  = 0,
            Extents = extents
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not MdfsFileNode mdfsNode) return ErrorNumber.InvalidArgument;

        mdfsNode.Extents = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not MdfsFileNode mdfsNode || mdfsNode.Extents is null) return ErrorNumber.InvalidArgument;

        if(buffer is null || buffer.Length < length || length < 0) return ErrorNumber.InvalidArgument;

        if(mdfsNode.Offset < 0) return ErrorNumber.InvalidArgument;

        if(mdfsNode.Offset >= mdfsNode.Length) return ErrorNumber.NoError;

        length = Math.Min(length, mdfsNode.Length - mdfsNode.Offset);

        if(length == 0) return ErrorNumber.NoError;

        ErrorNumber errno = ReadFromExtents(mdfsNode.Extents, (ulong)mdfsNode.Offset, (uint)length, out byte[] data);

        if(errno != ErrorNumber.NoError) return errno;

        Array.Copy(data, 0, buffer, 0, length);
        read             =  length;
        mdfsNode.Offset += length;

        return ErrorNumber.NoError;
    }
}