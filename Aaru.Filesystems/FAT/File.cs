// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft FAT filesystem plugin.
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
using System.IO;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Filesystems;

public sealed partial class FAT
{
    /// <inheritdoc />
    public ErrorNumber MapBlock(string path, long fileBlock, out long deviceBlock)
    {
        deviceBlock = 0;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        ErrorNumber err = Stat(path, out FileEntryInfo stat);

        if(err != ErrorNumber.NoError)
            return err;

        if(stat.Attributes.HasFlag(FileAttributes.Directory) &&
           !_debug)
            return ErrorNumber.IsDirectory;

        uint[] clusters = GetClusters((uint)stat.Inode);

        if(fileBlock >= clusters.Length)
            return ErrorNumber.InvalidArgument;

        deviceBlock = (long)(_firstClusterSector + (clusters[fileBlock] * _sectorsPerCluster));

        return ErrorNumber.NoError;
    }

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
    public ErrorNumber Read(string path, long offset, long size, ref byte[] buf)
    {
        if(!_mounted)
            return ErrorNumber.AccessDenied;

        ErrorNumber err = Stat(path, out FileEntryInfo stat);

        if(err != ErrorNumber.NoError)
            return err;

        if(stat.Attributes.HasFlag(FileAttributes.Directory) &&
           !_debug)
            return ErrorNumber.IsDirectory;

        if(size == 0)
        {
            buf = Array.Empty<byte>();

            return ErrorNumber.NoError;
        }

        if(offset >= stat.Length)
            return ErrorNumber.InvalidArgument;

        if(size + offset >= stat.Length)
            size = stat.Length - offset;

        uint[] clusters = GetClusters((uint)stat.Inode);

        if(clusters is null)
            return ErrorNumber.InvalidArgument;

        long firstCluster    = offset                   / _bytesPerCluster;
        long offsetInCluster = offset                   % _bytesPerCluster;
        long sizeInClusters  = (size + offsetInCluster) / _bytesPerCluster;

        if((size + offsetInCluster) % _bytesPerCluster > 0)
            sizeInClusters++;

        var ms = new MemoryStream();

        for(int i = 0; i < sizeInClusters; i++)
        {
            if(i + firstCluster >= clusters.Length)
                return ErrorNumber.InvalidArgument;

            ErrorNumber errno =
                _image.ReadSectors(_firstClusterSector + (clusters[i + firstCluster] * _sectorsPerCluster),
                                   _sectorsPerCluster, out byte[] buffer);

            if(errno != ErrorNumber.NoError)
                return errno;

            ms.Write(buffer, 0, buffer.Length);
        }

        ms.Position = offsetInCluster;
        buf         = new byte[size];
        ms.EnsureRead(buf, 0, (int)size);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        ErrorNumber err = GetFileEntry(path, out CompleteDirectoryEntry completeEntry);

        if(err != ErrorNumber.NoError)
            return err;

        DirectoryEntry entry = completeEntry.Dirent;

        stat = new FileEntryInfo
        {
            Attributes = new FileAttributes(),
            Blocks     = entry.size / _bytesPerCluster,
            BlockSize  = _bytesPerCluster,
            Length     = entry.size,
            Inode      = (ulong)(_fat32 ? (entry.ea_handle << 16) + entry.start_cluster : entry.start_cluster),
            Links      = 1
        };

        if(entry.cdate > 0 ||
           entry.ctime > 0)
            stat.CreationTime = DateHandlers.DosToDateTime(entry.cdate, entry.ctime);

        if(_namespace != Namespace.Human)
        {
            if(entry.mdate > 0 ||
               entry.mtime > 0)
                stat.LastWriteTime = DateHandlers.DosToDateTime(entry.mdate, entry.mtime);

            if(entry.ctime_ms > 0)
                stat.CreationTime = stat.CreationTime?.AddMilliseconds(entry.ctime_ms * 10);
        }

        if(entry.size % _bytesPerCluster > 0)
            stat.Blocks++;

        if(entry.attributes.HasFlag(FatAttributes.Subdirectory))
        {
            stat.Attributes |= FileAttributes.Directory;

            if((_fat32 && entry.ea_handle << 16 > 0) ||
               entry.start_cluster > 0)
                stat.Blocks = _fat32 ? GetClusters((uint)((entry.ea_handle << 16) + entry.start_cluster))?.Length ?? 0
                                  : GetClusters(entry.start_cluster)?.Length                                      ?? 0;

            stat.Length = stat.Blocks * stat.BlockSize;
        }

        if(entry.attributes.HasFlag(FatAttributes.ReadOnly))
            stat.Attributes |= FileAttributes.ReadOnly;

        if(entry.attributes.HasFlag(FatAttributes.Hidden))
            stat.Attributes |= FileAttributes.Hidden;

        if(entry.attributes.HasFlag(FatAttributes.System))
            stat.Attributes |= FileAttributes.System;

        if(entry.attributes.HasFlag(FatAttributes.Archive))
            stat.Attributes |= FileAttributes.Archive;

        if(entry.attributes.HasFlag(FatAttributes.Device))
            stat.Attributes |= FileAttributes.Device;

        return ErrorNumber.NoError;
    }

    uint[] GetClusters(uint startCluster)
    {
        if(startCluster == 0)
            return Array.Empty<uint>();

        if(startCluster >= XmlFsType.Clusters)
            return null;

        List<uint> clusters = new();

        uint nextCluster = startCluster;

        ulong nextSector = (nextCluster / _fatEntriesPerSector) + _fatFirstSector + (_useFirstFat ? 0 : _sectorsPerFat);

        int nextEntry = (int)(nextCluster % _fatEntriesPerSector);

        ulong       currentSector = nextSector;
        ErrorNumber errno         = _image.ReadSector(currentSector, out byte[] fatData);

        if(errno != ErrorNumber.NoError)
            return null;

        if(_fat32)
            while((nextCluster & FAT32_MASK) > 0 &&
                  (nextCluster & FAT32_MASK) <= FAT32_RESERVED)
            {
                clusters.Add(nextCluster);

                if(currentSector != nextSector)
                {
                    errno = _image.ReadSector(nextSector, out fatData);

                    if(errno != ErrorNumber.NoError)
                        return null;

                    currentSector = nextSector;
                }

                nextCluster = BitConverter.ToUInt32(fatData, nextEntry * 4);

                nextSector = (nextCluster / _fatEntriesPerSector) + _fatFirstSector +
                             (_useFirstFat ? 0 : _sectorsPerFat);

                nextEntry = (int)(nextCluster % _fatEntriesPerSector);
            }
        else if(_fat16)
            while(nextCluster is > 0 and <= FAT16_RESERVED)
            {
                if(nextCluster > _fatEntries.Length)
                    return null;

                clusters.Add(nextCluster);
                nextCluster = _fatEntries[nextCluster];
            }
        else
            while(nextCluster is > 0 and <= FAT12_RESERVED)
            {
                if(nextCluster > _fatEntries.Length)
                    return null;

                clusters.Add(nextCluster);
                nextCluster = _fatEntries[nextCluster];
            }

        return clusters.ToArray();
    }

    ErrorNumber GetFileEntry(string path, out CompleteDirectoryEntry entry)
    {
        entry = null;

        string cutPath = path.StartsWith('/') ? path[1..].ToLower(_cultureInfo) : path.ToLower(_cultureInfo);

        string[] pieces = cutPath.Split(new[]
        {
            '/'
        }, StringSplitOptions.RemoveEmptyEntries);

        if(pieces.Length == 0)
            return ErrorNumber.InvalidArgument;

        string parentPath = string.Join("/", pieces, 0, pieces.Length - 1);

        if(!_directoryCache.TryGetValue(parentPath, out _))
        {
            ErrorNumber err = ReadDir(parentPath, out _);

            if(err != ErrorNumber.NoError)
                return err;
        }

        Dictionary<string, CompleteDirectoryEntry> parent;

        if(pieces.Length == 1)
            parent = _rootDirectoryCache;
        else if(!_directoryCache.TryGetValue(parentPath, out parent))
            return ErrorNumber.InvalidArgument;

        KeyValuePair<string, CompleteDirectoryEntry> dirent =
            parent.FirstOrDefault(t => t.Key.ToLower(_cultureInfo) == pieces[^1]);

        if(string.IsNullOrEmpty(dirent.Key))
            return ErrorNumber.NoSuchFile;

        entry = dirent.Value;

        return ErrorNumber.NoError;
    }

    static byte LfnChecksum(byte[] name, byte[] extension)
    {
        byte sum = 0;

        for(int i = 0; i < 8; i++)
            sum = (byte)(((sum & 1) << 7) + (sum >> 1) + name[i]);

        for(int i = 0; i < 3; i++)
            sum = (byte)(((sum & 1) << 7) + (sum >> 1) + extension[i]);

        return sum;
    }
}