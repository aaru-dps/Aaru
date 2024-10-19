// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : FATX filesystem plugin.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Filesystems;

public sealed partial class XboxFatPlugin
{
#region IReadOnlyFilesystem Members

    /// <inheritdoc />
    public ErrorNumber GetAttributes(string path, out FileAttributes attributes)
    {
        attributes = new FileAttributes();

        if(!_mounted) return ErrorNumber.AccessDenied;

        ErrorNumber err = Stat(path, out FileEntryInfo stat);

        if(err != ErrorNumber.NoError) return err;

        attributes = stat.Attributes;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber OpenFile(string path, out IFileNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        ErrorNumber err = Stat(path, out FileEntryInfo stat);

        if(err != ErrorNumber.NoError) return err;

        if(stat.Attributes.HasFlag(FileAttributes.Directory) && !_debug) return ErrorNumber.IsDirectory;

        uint[] clusters = GetClusters((uint)stat.Inode);

        node = new FatxFileNode
        {
            Path     = path,
            Length   = stat.Length,
            Offset   = 0,
            Clusters = clusters
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not FatxFileNode mynode) return ErrorNumber.InvalidArgument;

        mynode.Clusters = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(buffer is null || buffer.Length < length) return ErrorNumber.InvalidArgument;

        if(node is not FatxFileNode mynode) return ErrorNumber.InvalidArgument;

        read = length;

        if(length + mynode.Offset >= mynode.Length) read = mynode.Length - mynode.Offset;

        long firstCluster    = mynode.Offset            / _bytesPerCluster;
        long offsetInCluster = mynode.Offset            % _bytesPerCluster;
        long sizeInClusters  = (read + offsetInCluster) / _bytesPerCluster;

        if((read + offsetInCluster) % _bytesPerCluster > 0) sizeInClusters++;

        var ms = new MemoryStream();

        for(var i = 0; i < sizeInClusters; i++)
        {
            if(i + firstCluster >= mynode.Clusters.Length) return ErrorNumber.InvalidArgument;

            ErrorNumber errno =
                _imagePlugin.ReadSectors(_firstClusterSector +
                                         (mynode.Clusters[i + firstCluster] - 1) * _sectorsPerCluster,
                                         _sectorsPerCluster,
                                         out byte[] buf);

            if(errno != ErrorNumber.NoError) return errno;

            ms.Write(buf, 0, buf.Length);
        }

        ms.Position = offsetInCluster;
        ms.EnsureRead(buffer, 0, (int)read);
        mynode.Offset += read;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(_debug && (string.IsNullOrEmpty(path) || path is "$" or "/"))
        {
            stat = new FileEntryInfo
            {
                Attributes = FileAttributes.Directory | FileAttributes.System | FileAttributes.Hidden,
                Blocks     = GetClusters(_superblock.rootDirectoryCluster).Length,
                BlockSize  = _bytesPerCluster,
                Length     = GetClusters(_superblock.rootDirectoryCluster).Length * _bytesPerCluster,
                Inode      = _superblock.rootDirectoryCluster,
                Links      = 1
            };

            return ErrorNumber.NoError;
        }

        ErrorNumber err = GetFileEntry(path, out DirectoryEntry entry);

        if(err != ErrorNumber.NoError) return err;

        stat = new FileEntryInfo
        {
            Attributes = new FileAttributes(),
            Blocks     = entry.length / _bytesPerCluster,
            BlockSize  = _bytesPerCluster,
            Length     = entry.length,
            Inode      = entry.firstCluster,
            Links      = 1,
            CreationTime =
                _littleEndian
                    ? DateHandlers.DosToDateTime(entry.creationDate, entry.creationTime).AddYears(20)
                    : DateHandlers.DosToDateTime(entry.creationTime, entry.creationDate),
            AccessTime =
                _littleEndian
                    ? DateHandlers.DosToDateTime(entry.lastAccessDate, entry.lastAccessTime).AddYears(20)
                    : DateHandlers.DosToDateTime(entry.lastAccessTime, entry.lastAccessDate),
            LastWriteTime = _littleEndian
                                ? DateHandlers.DosToDateTime(entry.lastWrittenDate, entry.lastWrittenTime).AddYears(20)
                                : DateHandlers.DosToDateTime(entry.lastWrittenTime, entry.lastWrittenDate)
        };

        if(entry.length % _bytesPerCluster > 0) stat.Blocks++;

        if(entry.attributes.HasFlag(Attributes.Directory))
        {
            stat.Attributes |= FileAttributes.Directory;
            stat.Blocks     =  GetClusters(entry.firstCluster).Length;
            stat.Length     =  stat.Blocks * stat.BlockSize;
        }

        if(entry.attributes.HasFlag(Attributes.ReadOnly)) stat.Attributes |= FileAttributes.ReadOnly;

        if(entry.attributes.HasFlag(Attributes.Hidden)) stat.Attributes |= FileAttributes.Hidden;

        if(entry.attributes.HasFlag(Attributes.System)) stat.Attributes |= FileAttributes.System;

        if(entry.attributes.HasFlag(Attributes.Archive)) stat.Attributes |= FileAttributes.Archive;

        return ErrorNumber.NoError;
    }

#endregion

    uint[] GetClusters(uint startCluster)
    {
        if(startCluster == 0) return null;

        if(_fat16 is null)
        {
            if(startCluster >= _fat32.Length) return null;
        }
        else if(startCluster >= _fat16.Length) return null;

        List<uint> clusters = [];

        uint nextCluster = startCluster;

        if(_fat16 is null)
        {
            while((nextCluster & FAT32_MASK) > 0 && (nextCluster & FAT32_MASK) <= FAT32_RESERVED)
            {
                clusters.Add(nextCluster);
                nextCluster = _fat32[nextCluster];
            }
        }
        else
        {
            while(nextCluster is > 0 and <= FAT16_RESERVED)
            {
                clusters.Add(nextCluster);
                nextCluster = _fat16[nextCluster];
            }
        }

        return clusters.ToArray();
    }

    ErrorNumber GetFileEntry(string path, out DirectoryEntry entry)
    {
        entry = new DirectoryEntry();

        string cutPath = path.StartsWith('/') ? path[1..].ToLower(_cultureInfo) : path.ToLower(_cultureInfo);

        string[] pieces = cutPath.Split(new[]
                                        {
                                            '/'
                                        },
                                        StringSplitOptions.RemoveEmptyEntries);

        if(pieces.Length == 0) return ErrorNumber.InvalidArgument;

        var parentPath = string.Join("/", pieces, 0, pieces.Length - 1);

        ErrorNumber err = OpenDir(parentPath, out IDirNode node);

        if(err != ErrorNumber.NoError) return err;

        CloseDir(node);

        Dictionary<string, DirectoryEntry> parent;

        if(pieces.Length == 1)
            parent = _rootDirectory;
        else if(!_directoryCache.TryGetValue(parentPath, out parent)) return ErrorNumber.InvalidArgument;

        KeyValuePair<string, DirectoryEntry> dirent =
            parent.FirstOrDefault(t => t.Key.ToLower(_cultureInfo) == pieces[^1]);

        if(string.IsNullOrEmpty(dirent.Key)) return ErrorNumber.NoSuchFile;

        entry = dirent.Value;

        return ErrorNumber.NoError;
    }
}