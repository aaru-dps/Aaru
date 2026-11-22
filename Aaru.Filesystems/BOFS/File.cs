// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : BeOS old filesystem plugin.
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
using Aaru.Logging;

namespace Aaru.Filesystems;

public sealed partial class BOFS
{
    /// <inheritdoc />
    public ErrorNumber OpenFile(string path, out IFileNode node)
    {
        node = null;

        if(string.IsNullOrEmpty(path) || path == "/") return ErrorNumber.NotSupported;

        // Use helper to lookup the entry
        ErrorNumber lookupErr = LookupEntry(path, out FileEntry entry);

        if(lookupErr != ErrorNumber.NoError) return ErrorNumber.NoSuchFile;

        // Check if it's a file, not a directory
        if(entry.FileType == DIR_TYPE) return ErrorNumber.NotSupported;

        var fileNode = new BOFSFileNode
        {
            Path   = path,
            Length = entry.LogicalSize,
            Offset = 0,
            Entry  = entry
        };

        // Check if file is contiguous (high bit set) or fragmented
        if((entry.FirstAllocList & 0x80000000) != 0)
        {
            // Contiguous file
            fileNode.ContiguousSector = entry.FirstAllocList & 0x7FFFFFFF;
            fileNode.Extents          = null;
        }
        else
        {
            // Fragmented file - read FAT
            fileNode.ContiguousSector = -1;

            ErrorNumber errno = ReadExtents(entry.FirstAllocList, out List<(uint start, uint count)> extents);

            if(errno != ErrorNumber.NoError) return errno;

            fileNode.Extents = extents;
        }

        node = fileNode;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(node is not BOFSFileNode fileNode) return ErrorNumber.InvalidArgument;

        // Nothing to clean up - no caching
        fileNode.Offset = 0;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(node is not BOFSFileNode fileNode) return ErrorNumber.InvalidArgument;

        // Directories cannot be read as files
        if(fileNode.Entry.FileType == DIR_TYPE) return ErrorNumber.IsDirectory;

        if(fileNode.Offset < 0 || fileNode.Offset > fileNode.Length) return ErrorNumber.InvalidArgument;

        if(buffer == null) return ErrorNumber.InvalidArgument;

        // Clamp read length to file size and buffer size
        long toRead                                           = length;
        if(fileNode.Offset + toRead > fileNode.Length) toRead = fileNode.Length - fileNode.Offset;

        if(toRead <= 0) return ErrorNumber.NoError;

        if(toRead > buffer.Length) toRead = buffer.Length;

        const int bofsLogicalSectorSize = 512;

        // Read data using logical_to_physical logic
        long bytesRead     = 0;
        long currentOffset = fileNode.Offset;

        while(bytesRead < toRead)
        {
            // Calculate how much to read in this iteration
            long remaining = toRead - bytesRead;

            // Get physical location and max readable length from this extent
            ErrorNumber getPhysErr = GetPhysicalLocation(fileNode,
                                                         currentOffset,
                                                         remaining,
                                                         out long physicalByteOffset,
                                                         out long maxReadLength);

            if(getPhysErr != ErrorNumber.NoError) return getPhysErr;

            // Read from physical location
            var   absoluteByteOffset   = (ulong)physicalByteOffset;
            ulong deviceSectorOffset   = absoluteByteOffset / _imagePlugin.Info.SectorSize;
            ulong offsetInDeviceSector = absoluteByteOffset % _imagePlugin.Info.SectorSize;
            ulong absoluteDeviceSector = _partition.Start + deviceSectorOffset;

            ulong bytesNeeded = offsetInDeviceSector + (ulong)maxReadLength;
            var sectorsToRead = (uint)((bytesNeeded + _imagePlugin.Info.SectorSize - 1) / _imagePlugin.Info.SectorSize);

            ErrorNumber errno =
                _imagePlugin.ReadSectors(absoluteDeviceSector, false, sectorsToRead, out byte[] sectorData, out _);

            if(errno != ErrorNumber.NoError) return errno;

            if(sectorData.Length < (int)(offsetInDeviceSector + (ulong)maxReadLength))
                return ErrorNumber.InvalidArgument;

            // Copy data to buffer
            Array.Copy(sectorData, (int)offsetInDeviceSector, buffer, bytesRead, maxReadLength);

            bytesRead     += maxReadLength;
            currentOffset += maxReadLength;
        }

        read            =  bytesRead;
        fileNode.Offset += bytesRead;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        if(string.IsNullOrEmpty(path) || path == "/")
        {
            // Root directory - use mode from superblock
            stat = new FileEntryInfo
            {
                Attributes = FileAttributes.Directory,
                Inode      = 0,
                Links      = 2, // . and ..
                Mode       = _track0.VersionNumber >= VERSION_3 ? (uint)_track0.RootMode : null,
                Length     = 0,
                Blocks     = 0,
                BlockSize  = _track0.BytesPerSector
            };

            return ErrorNumber.NoError;
        }

        // Use helper to lookup the entry
        ErrorNumber lookupErr = LookupEntry(path, out FileEntry entry);

        return lookupErr != ErrorNumber.NoError ? ErrorNumber.NoSuchFile : PopulateStat(entry, out stat);
    }

    /// <summary>Get physical byte offset and max readable length for a logical file offset</summary>
    private ErrorNumber GetPhysicalLocation(BOFSFileNode file,               long logicalOffset, long requestedLength,
                                            out long     physicalByteOffset, out long maxReadLength)
    {
        physicalByteOffset = 0;
        maxReadLength      = 0;

        const int bofsLogicalSectorSize = 512;

        if(file.Extents == null)
        {
            // Contiguous file
            physicalByteOffset = file.ContiguousSector * bofsLogicalSectorSize + logicalOffset;
            maxReadLength      = requestedLength;

            return ErrorNumber.NoError;
        }

        // Fragmented file - search FAT for the extent containing logicalOffset
        long currentOffset = 0;

        foreach((uint startSector, uint sizeSectors) in file.Extents)
        {
            long extentSizeBytes = (long)sizeSectors * bofsLogicalSectorSize;

            // Check if logicalOffset is in this extent
            if(logicalOffset < currentOffset + extentSizeBytes)
            {
                // Found the extent
                long offsetInExtent = logicalOffset - currentOffset;
                physicalByteOffset = (long)startSector * bofsLogicalSectorSize + offsetInExtent;

                // Max readable is from this point to end of extent
                maxReadLength = extentSizeBytes - offsetInExtent;

                // But don't exceed requested length
                if(maxReadLength > requestedLength) maxReadLength = requestedLength;

                return ErrorNumber.NoError;
            }

            currentOffset += extentSizeBytes;
        }

        return ErrorNumber.InvalidArgument;
    }

    private ErrorNumber PopulateStat(FileEntry entry, out FileEntryInfo stat)
    {
        stat = new FileEntryInfo
        {
            Inode  = (ulong)entry.RecordId,
            Links  = 1,
            Length = entry.LogicalSize,
            Blocks = entry.PhysicalSize > 0
                         ? (long)((ulong)(entry.PhysicalSize + _track0.BytesPerSector - 1) /
                                  (ulong)_track0.BytesPerSector)
                         : 0,
            BlockSize = _track0.BytesPerSector,
            Mode      = _track0.VersionNumber >= VERSION_3 ? (uint)entry.Mode : null
        };

        // Set attributes based on FileType
        if(entry.FileType == DIR_TYPE)
            stat.Attributes |= FileAttributes.Directory;
        else
            stat.Attributes |= FileAttributes.File;

        // Convert BeOS timestamps (seconds since 1970) to .NET DateTime
        if(entry.CreationDate != 0) stat.CreationTimeUtc = DateHandlers.UnixToDateTime(entry.CreationDate);

        if(entry.ModificationDate != 0) stat.LastWriteTimeUtc = DateHandlers.UnixToDateTime(entry.ModificationDate);

        return ErrorNumber.NoError;
    }

    /// <summary>Reads every FAT block in a file's chain and collects its extents</summary>
    /// <remarks>
    ///     Each 512-byte FAT block starts with the sector of the next FAT block (0 ends the chain) and a second word,
    ///     followed by up to 63 pairs of (start sector, sector count). A start sector of 0xFFFFFFFF ends the pairs.
    /// </remarks>
    /// <param name="firstFatSector">Sector of the first FAT block</param>
    /// <param name="extents">Extents in file order</param>
    /// <returns>Error code</returns>
    private ErrorNumber ReadExtents(int firstFatSector, out List<(uint start, uint count)> extents)
    {
        extents = [];

        const int bofsLogicalSectorSize = 512;
        const int extentsPerBlock       = 63;
        var       visited               = new HashSet<uint>();
        var       fatSector             = (uint)firstFatSector;

        while(fatSector != 0)
        {
            if(fatSector >= (uint)_track0.TotalSectors || !visited.Add(fatSector))
            {
                AaruLogging.Debug(MODULE_NAME, "ReadExtents: invalid or looping FAT block {0}", fatSector);

                return ErrorNumber.InvalidArgument;
            }

            ulong fatByteOffset        = (ulong)fatSector * bofsLogicalSectorSize;
            ulong offsetInDeviceSector = fatByteOffset % _imagePlugin.Info.SectorSize;
            ulong absoluteDeviceSector = _partition.Start + fatByteOffset / _imagePlugin.Info.SectorSize;

            uint  deviceSectorSize = _imagePlugin.Info.SectorSize;
            ulong bytesNeeded      = offsetInDeviceSector + bofsLogicalSectorSize;
            var   sectorsToRead    = (uint)((bytesNeeded + deviceSectorSize - 1) / deviceSectorSize);

            ErrorNumber errno =
                _imagePlugin.ReadSectors(absoluteDeviceSector, false, sectorsToRead, out byte[] sectorData, out _);

            if(errno != ErrorNumber.NoError) return errno;

            if(sectorData.Length < (int)(offsetInDeviceSector + bofsLogicalSectorSize))
                return ErrorNumber.InvalidArgument;

            var offset = (int)offsetInDeviceSector;

            for(var i = 0; i < extentsPerBlock; i++)
            {
                uint start = BigEndianBitConverter.ToUInt32(sectorData, offset + 8 + i * 8);
                uint count = BigEndianBitConverter.ToUInt32(sectorData, offset + 12 + i * 8);

                if(start == 0xFFFFFFFF) break;

                extents.Add((start, count));
            }

            fatSector = BigEndianBitConverter.ToUInt32(sectorData, offset);
        }

        return ErrorNumber.NoError;
    }

    /// <summary>File node for tracking open file state</summary>
    private sealed class BOFSFileNode : IFileNode
    {
        /// <summary>File entry data</summary>
        public FileEntry Entry { get; set; }

        /// <summary>Extents (start sector, sector count) gathered from the FAT chain, null if contiguous</summary>
        public List<(uint start, uint count)> Extents { get; set; }

        /// <summary>Contiguous sector if not fragmented, -1 otherwise</summary>
        public long ContiguousSector { get; set; }
        /// <inheritdoc />
        public string Path { get; set; }

        /// <inheritdoc />
        public long Length { get; set; }

        /// <inheritdoc />
        public long Offset { get; set; }
    }
}