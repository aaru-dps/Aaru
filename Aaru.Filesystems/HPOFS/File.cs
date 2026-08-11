// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : High Performance Optical File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     File operations for the High Performance Optical File System.
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

public sealed partial class HPOFS
{
    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize path
        string cleanPath = path.Replace('\\', '/').Trim('/');

        // Root directory
        if(string.IsNullOrEmpty(cleanPath))
        {
            stat = new FileEntryInfo
            {
                Attributes = FileAttributes.Directory,
                BlockSize  = _bpb.bps,
                Links      = 1
            };

            return ErrorNumber.NoError;
        }

        // Split into directory + filename
        int lastSep = cleanPath.LastIndexOf('/');

        CachedDirectoryEntry entry;

        if(lastSep < 0)
        {
            // Root-level entry
            if(!_rootDirectoryCache.TryGetValue(cleanPath, out entry)) return ErrorNumber.NoSuchFile;
        }
        else
        {
            string dirPath  = cleanPath[..lastSep];
            string fileName = cleanPath[(lastSep + 1)..];

            if(!_directoryCache.TryGetValue(dirPath, out Dictionary<string, CachedDirectoryEntry> dirEntries))
                return ErrorNumber.NoSuchFile;

            if(!dirEntries.TryGetValue(fileName, out entry)) return ErrorNumber.NoSuchFile;
        }

        FileAttributes attrs = FileAttributes.None;

        if(entry.IsDirectory) attrs              |= FileAttributes.Directory;
        if((entry.Attributes & 0x01) != 0) attrs |= FileAttributes.ReadOnly;
        if((entry.Attributes & 0x02) != 0) attrs |= FileAttributes.Hidden;
        if((entry.Attributes & 0x04) != 0) attrs |= FileAttributes.System;
        if((entry.Attributes & 0x20) != 0) attrs |= FileAttributes.Archive;

        stat = new FileEntryInfo
        {
            Attributes = attrs,
            BlockSize  = _bpb.bps,
            Links      = 1,
            Length     = entry.FileSize,
            Blocks     = entry.FileSize > 0 ? (entry.FileSize + _bpb.bps - 1) / _bpb.bps : 0,
            Inode      = entry.SectorAddress
        };

        if(entry.CreationTimestamp > 0)
            stat.CreationTimeUtc = DateTimeOffset.FromUnixTimeSeconds(entry.CreationTimestamp).UtcDateTime;

        if(entry.ModificationTimestamp > 0)
            stat.LastWriteTimeUtc = DateTimeOffset.FromUnixTimeSeconds(entry.ModificationTimestamp).UtcDateTime;

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

        // Find the directory entry to get extent information
        string cleanPath = path.Replace('\\', '/').Trim('/');
        int    lastSep   = cleanPath.LastIndexOf('/');

        CachedDirectoryEntry entry;

        if(lastSep < 0)
        {
            if(!_rootDirectoryCache.TryGetValue(cleanPath, out entry)) return ErrorNumber.NoSuchFile;
        }
        else
        {
            string dirPath  = cleanPath[..lastSep];
            string fileName = cleanPath[(lastSep + 1)..];

            if(!_directoryCache.TryGetValue(dirPath, out Dictionary<string, CachedDirectoryEntry> dirEntries))
                return ErrorNumber.NoSuchFile;

            if(!dirEntries.TryGetValue(fileName, out entry)) return ErrorNumber.NoSuchFile;
        }

        // Build the extent list
        List<(uint startLba, ushort sectorCount)> extents = BuildFileExtentList(entry);

        if(extents.Count == 0 && entry.FileSize > 0) return ErrorNumber.InvalidArgument;

        node = new HpofsFileNode
        {
            Path    = path,
            Length  = entry.FileSize,
            Offset  = 0,
            Extents = extents
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not HpofsFileNode mynode) return ErrorNumber.InvalidArgument;

        mynode.Extents = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(buffer is null || buffer.Length < length) return ErrorNumber.InvalidArgument;

        if(node is not HpofsFileNode mynode) return ErrorNumber.InvalidArgument;

        read = length;

        if(length + mynode.Offset >= mynode.Length) read = mynode.Length - mynode.Offset;

        if(read <= 0)
        {
            read = 0;

            return ErrorNumber.NoError;
        }

        long bytesRead   = 0;
        long remaining   = read;
        long fileOffset  = mynode.Offset;
        long extentStart = 0;

        foreach((uint startLba, ushort sectorCount) in mynode.Extents)
        {
            long extentBytes = (long)sectorCount * _bpb.bps;

            // Skip extents that are entirely before the current offset
            if(extentStart + extentBytes <= fileOffset)
            {
                extentStart += extentBytes;

                continue;
            }

            // Calculate position within this extent
            long offsetInExtent = fileOffset - extentStart;
            long bytesToRead    = Math.Min(remaining, extentBytes - offsetInExtent);

            // Determine which sectors to read
            long firstSector    = offsetInExtent                                / _bpb.bps;
            long offsetInSector = offsetInExtent                                % _bpb.bps;
            long sectorsToRead  = (bytesToRead + offsetInSector + _bpb.bps - 1) / _bpb.bps;

            ErrorNumber errno = _image.ReadSectors((ulong)(startLba + firstSector) + _partition.Start,
                                                   false,
                                                   (uint)sectorsToRead,
                                                   out byte[] buf,
                                                   out _);

            if(errno != ErrorNumber.NoError)
            {
                read = bytesRead;

                return errno;
            }

            Array.Copy(buf, offsetInSector, buffer, bytesRead, bytesToRead);

            bytesRead  += bytesToRead;
            remaining  -= bytesToRead;
            fileOffset += bytesToRead;

            if(remaining <= 0) break;

            extentStart += extentBytes;
        }

        mynode.Offset += bytesRead;
        read          =  bytesRead;

        return ErrorNumber.NoError;
    }

    /// <summary>Builds the list of data extents for a file from its inline extent and optional SUBF chain</summary>
    List<(uint startLba, ushort sectorCount)> BuildFileExtentList(CachedDirectoryEntry entry)
    {
        List<(uint startLba, ushort sectorCount)> extents = new();

        // Add the inline extent from the B-tree record (+0x40 sector count, +0x44 start LBA)
        if(entry.DataSectorCount > 0 && entry.DataStartLba != EXTENT_END_MARKER)
            extents.Add((entry.DataStartLba, entry.DataSectorCount));

        // If there is a SUBF sector for additional extents, read and parse it
        if(entry.SubfSector == EXTENT_END_MARKER || entry.SubfSector == 0) return extents;

        ErrorNumber subfErrno =
            _image.ReadSector(entry.SubfSector + _partition.Start, false, out byte[] subfData, out _);

        if(subfErrno != ErrorNumber.NoError || subfData.Length < 0x20) return extents;

        if(!subfData[..4].SequenceEqual(_subfSignature)) return extents;

        var extentCount = BigEndianBitConverter.ToUInt16(subfData, 0x10);

        for(var ei = 0; ei < extentCount; ei++)
        {
            int extOff = 0x20 + ei * 8;

            if(extOff + 8 > subfData.Length) break;

            var sectorCount = BigEndianBitConverter.ToUInt16(subfData, extOff);
            var startLba    = BigEndianBitConverter.ToUInt32(subfData, extOff + 4);

            if(startLba == EXTENT_END_MARKER) break;

            if(sectorCount > 0) extents.Add((startLba, sectorCount));
        }

        return extents;
    }
}