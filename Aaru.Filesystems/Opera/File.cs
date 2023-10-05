// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Opera filesystem plugin.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

namespace Aaru.Filesystems;

public sealed partial class OperaFS
{
#region IReadOnlyFilesystem Members

    /// <inheritdoc />
    public ErrorNumber GetAttributes(string path, out FileAttributes attributes)
    {
        attributes = new FileAttributes();

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        ErrorNumber err = Stat(path, out FileEntryInfo stat);

        if(err != ErrorNumber.NoError)
            return err;

        attributes = stat.Attributes;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber OpenFile(string path, out IFileNode node)
    {
        node = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        ErrorNumber err = GetFileEntry(path, out DirectoryEntryWithPointers entry);

        if(err != ErrorNumber.NoError)
            return err;

        if((entry.Entry.flags & FLAGS_MASK) == (uint)FileFlags.Directory && !_debug)
            return ErrorNumber.IsDirectory;

        if(entry.Pointers.Length < 1)
            return ErrorNumber.InvalidArgument;

        node = new OperaFileNode
        {
            Path   = path,
            Length = entry.Entry.byte_count,
            Offset = 0,
            Dentry = entry
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(!_mounted)
            return ErrorNumber.AccessDenied;

        if(node is not OperaFileNode mynode)
            return ErrorNumber.InvalidArgument;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        if(buffer is null || buffer.Length < length)
            return ErrorNumber.InvalidArgument;

        if(node is not OperaFileNode mynode)
            return ErrorNumber.InvalidArgument;

        read = length;

        if(length + mynode.Offset >= mynode.Length)
            read = mynode.Length - mynode.Offset;

        long firstBlock    = mynode.Offset          / mynode.Dentry.Entry.block_size;
        long offsetInBlock = mynode.Offset          % mynode.Dentry.Entry.block_size;
        long sizeInBlocks  = (read + offsetInBlock) / mynode.Dentry.Entry.block_size;

        if((read + offsetInBlock) % mynode.Dentry.Entry.block_size > 0)
            sizeInBlocks++;

        uint fileBlockSizeRatio;

        if(_image.Info.SectorSize is 2336 or 2352 or 2448)
            fileBlockSizeRatio = mynode.Dentry.Entry.block_size / 2048;
        else
            fileBlockSizeRatio = mynode.Dentry.Entry.block_size / _image.Info.SectorSize;

        ErrorNumber errno = _image.ReadSectors((ulong)(mynode.Dentry.Pointers[0] + firstBlock * fileBlockSizeRatio),
                                               (uint)(sizeInBlocks * fileBlockSizeRatio), out byte[] buf);

        if(errno != ErrorNumber.NoError)
        {
            read = 0;

            return errno;
        }

        Array.Copy(buf, offsetInBlock, buffer, 0, read);

        mynode.Offset += read;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        ErrorNumber err = GetFileEntry(path, out DirectoryEntryWithPointers entryWithPointers);

        if(err != ErrorNumber.NoError)
            return err;

        DirectoryEntry entry = entryWithPointers.Entry;

        stat = new FileEntryInfo
        {
            Attributes = new FileAttributes(),
            Blocks     = entry.block_count,
            BlockSize  = entry.block_size,
            Length     = entry.byte_count,
            Inode      = entry.id,
            Links      = (ulong)entryWithPointers.Pointers.Length
        };

        var flags = (FileFlags)(entry.flags & FLAGS_MASK);

        switch(flags)
        {
            case FileFlags.Directory:
                stat.Attributes |= FileAttributes.Directory;

                break;
            case FileFlags.Special:
                stat.Attributes |= FileAttributes.Device;

                break;
        }

        return ErrorNumber.NoError;
    }

#endregion

    ErrorNumber GetFileEntry(string path, out DirectoryEntryWithPointers entry)
    {
        entry = null;

        string cutPath = path.StartsWith("/", StringComparison.Ordinal)
                             ? path[1..].ToLower(CultureInfo.CurrentUICulture)
                             : path.ToLower(CultureInfo.CurrentUICulture);

        string[] pieces = cutPath.Split(new[]
        {
            '/'
        }, StringSplitOptions.RemoveEmptyEntries);

        if(pieces.Length == 0)
            return ErrorNumber.InvalidArgument;

        var parentPath = string.Join("/", pieces, 0, pieces.Length - 1);

        if(!_directoryCache.TryGetValue(parentPath, out _))
        {
            ErrorNumber err = OpenDir(parentPath, out IDirNode node);

            if(err != ErrorNumber.NoError)
                return err;

            CloseDir(node);
        }

        Dictionary<string, DirectoryEntryWithPointers> parent;

        if(pieces.Length == 1)
            parent = _rootDirectoryCache;
        else if(!_directoryCache.TryGetValue(parentPath, out parent))
            return ErrorNumber.InvalidArgument;

        KeyValuePair<string, DirectoryEntryWithPointers> dirent =
            parent.FirstOrDefault(t => t.Key.Equals(pieces[^1], StringComparison.CurrentCultureIgnoreCase));

        if(string.IsNullOrEmpty(dirent.Key))
            return ErrorNumber.NoSuchFile;

        entry = dirent.Value;

        return ErrorNumber.NoError;
    }
}