// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft exFAT filesystem plugin.
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
using System.Collections.Generic;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;

namespace Aaru.Filesystems;

// Information from https://github.com/MicrosoftDocs/win32/blob/docs/desktop-src/FileIO/exfat-specification.md
/// <inheritdoc />
public sealed partial class exFAT
{
    /// <inheritdoc />
    public ErrorNumber OpenFile(string path, out IFileNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        ErrorNumber errno = GetFileEntry(path, out CompleteDirectoryEntry entry);

        if(errno != ErrorNumber.NoError) return errno;

        if(entry.IsDirectory) return ErrorNumber.IsDirectory;

        node = new ExFatFileNode
        {
            Path         = path,
            Length       = (long)entry.DataLength,
            Offset       = 0,
            FirstCluster = entry.FirstCluster,
            IsContiguous = entry.IsContiguous
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not ExFatFileNode myNode) return ErrorNumber.InvalidArgument;

        myNode.Offset = -1;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(buffer is null || buffer.Length < length) return ErrorNumber.InvalidArgument;

        if(node is not ExFatFileNode myNode) return ErrorNumber.InvalidArgument;

        if(myNode.Offset < 0) return ErrorNumber.InvalidArgument;

        if(length < 0) return ErrorNumber.InvalidArgument;

        // End of file reached
        if(myNode.Offset >= myNode.Length) return ErrorNumber.NoError;

        // Adjust length if it would read past end of file
        if(myNode.Offset + length > myNode.Length) length = myNode.Length - myNode.Offset;

        if(length == 0) return ErrorNumber.NoError;

        // Calculate which clusters we need to read
        long firstCluster    = myNode.Offset / _bytesPerCluster;
        long offsetInCluster = myNode.Offset % _bytesPerCluster;
        long bytesToRead     = length;
        long sizeInClusters  = (bytesToRead + offsetInCluster + _bytesPerCluster - 1) / _bytesPerCluster;

        long bufferOffset = 0;

        for(long i = 0; i < sizeInClusters; i++)
        {
            // Get the cluster number for this position
            uint clusterNumber;

            if(myNode.IsContiguous)
            {
                // For contiguous files, clusters are sequential starting from FirstCluster
                clusterNumber = (uint)(myNode.FirstCluster + firstCluster + i);
            }
            else
            {
                // For non-contiguous files, follow the FAT chain
                clusterNumber = GetClusterAtPosition(myNode.FirstCluster, (uint)(firstCluster + i));
            }

            if(clusterNumber < 2 || clusterNumber > _clusterCount + 1) return ErrorNumber.InvalidArgument;

            // Calculate sector for this cluster
            ulong sector = _clusterHeapOffset + (ulong)(clusterNumber - 2) * _sectorsPerCluster;

            // Read the cluster
            ErrorNumber errno = _image.ReadSectors(sector, false, _sectorsPerCluster, out byte[] clusterData, out _);

            if(errno != ErrorNumber.NoError) return errno;

            // Calculate how much to copy from this cluster
            long clusterOffset        = i == 0 ? offsetInCluster : 0;
            long bytesFromThisCluster = _bytesPerCluster - clusterOffset;

            if(bytesFromThisCluster > bytesToRead - bufferOffset) bytesFromThisCluster = bytesToRead - bufferOffset;

            // Copy data to buffer
            Array.Copy(clusterData, clusterOffset, buffer, bufferOffset, bytesFromThisCluster);
            bufferOffset += bytesFromThisCluster;
        }

        read          =  bufferOffset;
        myNode.Offset += read;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        ErrorNumber errno = GetFileEntry(path, out CompleteDirectoryEntry entry);

        if(errno != ErrorNumber.NoError) return errno;

        stat = new FileEntryInfo
        {
            Attributes = new CommonTypes.Structs.FileAttributes(),
            BlockSize  = _bytesPerCluster,
            Length     = (long)entry.DataLength,
            Inode      = entry.FirstCluster,
            Links      = 1
        };

        // Calculate blocks
        stat.Blocks = (long)(entry.DataLength / _bytesPerCluster);

        if(entry.DataLength % _bytesPerCluster > 0) stat.Blocks++;

        // Convert timestamps
        stat.CreationTimeUtc = DateHandlers.ExFatToDateTime(entry.FileEntry.CreateTimestamp,
                                                            entry.FileEntry.Create10msIncrement,
                                                            entry.FileEntry.CreateUtcOffset);

        stat.LastWriteTimeUtc = DateHandlers.ExFatToDateTime(entry.FileEntry.LastModifiedTimestamp,
                                                             entry.FileEntry.LastModified10msIncrement,
                                                             entry.FileEntry.LastModifiedUtcOffset);

        stat.AccessTimeUtc = DateHandlers.ExFatToDateTime(entry.FileEntry.LastAccessedTimestamp,
                                                          0,
                                                          entry.FileEntry.LastAccessedUtcOffset);

        // Set file attributes
        if(entry.IsDirectory)
            stat.Attributes |= CommonTypes.Structs.FileAttributes.Directory;
        else
            stat.Attributes |= CommonTypes.Structs.FileAttributes.File;

        if(entry.FileEntry.FileAttributes.HasFlag(FileAttributes.ReadOnly))
            stat.Attributes |= CommonTypes.Structs.FileAttributes.ReadOnly;

        if(entry.FileEntry.FileAttributes.HasFlag(FileAttributes.Hidden))
            stat.Attributes |= CommonTypes.Structs.FileAttributes.Hidden;

        if(entry.FileEntry.FileAttributes.HasFlag(FileAttributes.System))
            stat.Attributes |= CommonTypes.Structs.FileAttributes.System;

        if(entry.FileEntry.FileAttributes.HasFlag(FileAttributes.Archive))
            stat.Attributes |= CommonTypes.Structs.FileAttributes.Archive;

        return ErrorNumber.NoError;
    }

    // ...existing code...
    /// <param name="path">Path to the file or directory.</param>
    /// <param name="entry">The directory entry if found.</param>
    /// <returns>Error number.</returns>
    ErrorNumber GetFileEntry(string path, out CompleteDirectoryEntry entry)
    {
        entry = null;

        if(string.IsNullOrWhiteSpace(path) || path == "/") return ErrorNumber.IsDirectory;

        string cutPath = path.StartsWith("/", StringComparison.Ordinal) ? path[1..] : path;

        string[] pieces = cutPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if(pieces.Length == 0) return ErrorNumber.IsDirectory;

        // Get parent directory
        string parentPath;
        string fileName;

        if(pieces.Length == 1)
        {
            parentPath = "/";
            fileName   = pieces[0];
        }
        else
        {
            parentPath = string.Join("/", pieces, 0, pieces.Length - 1);
            fileName   = pieces[^1];
        }

        ErrorNumber errno =
            GetDirectoryEntries(parentPath, out Dictionary<string, CompleteDirectoryEntry> parentEntries);

        if(errno != ErrorNumber.NoError) return errno;

        // Find the entry (case-insensitive per exFAT spec)
        foreach(KeyValuePair<string, CompleteDirectoryEntry> kvp in parentEntries)
        {
            if(kvp.Key.Equals(fileName, StringComparison.OrdinalIgnoreCase))
            {
                entry = kvp.Value;

                return ErrorNumber.NoError;
            }
        }

        return ErrorNumber.NoSuchFile;
    }
}