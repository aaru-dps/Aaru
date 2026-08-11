// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Nintendo optical filesystems plugin.
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
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements the filesystem used by Nintendo Gamecube and Wii discs</summary>
public sealed partial class NintendoPlugin
{
    /// <summary>
    ///     Resolve a filesystem path to a partition index and FST entry index.
    ///     In multi-partition mode, the first path component selects the partition.
    /// </summary>
    /// <param name="path">Absolute path (e.g., "/" or "/DATA/somefile" or "/somefile")</param>
    /// <param name="partitionIndex">
    ///     Index into <see cref="_partitions" />, or -1 for the virtual multi-partition root
    /// </param>
    /// <param name="entryIndex">FST entry index within the partition (may be a negative virtual index)</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ResolvePath(string path, out int partitionIndex, out int entryIndex)
    {
        entryIndex     = 0;
        partitionIndex = -1;

        if(string.IsNullOrWhiteSpace(path) || path == "/")
        {
            if(!_multiPartition) partitionIndex = 0;

            return ErrorNumber.NoError;
        }

        string   cutPath = path.StartsWith("/", StringComparison.Ordinal) ? path[1..] : path;
        string[] pieces  = cutPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if(_multiPartition)
        {
            // First component is partition name
            for(var i = 0; i < _partitions.Length; i++)
            {
                if(!_partitions[i].Name.Equals(pieces[0], StringComparison.OrdinalIgnoreCase)) continue;

                partitionIndex = i;

                break;
            }

            if(partitionIndex < 0) return ErrorNumber.NoSuchFile;

            // Only the partition name was given — resolve to its root
            if(pieces.Length == 1)
            {
                entryIndex = 0;

                return ErrorNumber.NoError;
            }

            return ResolveWithinPartition(_partitions[partitionIndex], pieces[1..], out entryIndex);
        }

        partitionIndex = 0;

        return ResolveWithinPartition(_partitions[0], pieces, out entryIndex);
    }

    /// <summary>Resolve path components within a specific partition's FST</summary>
    /// <param name="partition">Partition whose FST to search</param>
    /// <param name="pieces">Path components to resolve (no partition prefix)</param>
    /// <param name="entryIndex">Resulting FST entry index</param>
    /// <returns>Error number indicating success or failure</returns>
    static ErrorNumber ResolveWithinPartition(PartitionInfo partition, string[] pieces, out int entryIndex)
    {
        entryIndex = 0;
        var currentDirIndex = 0;

        for(var p = 0; p < pieces.Length; p++)
        {
            Dictionary<string, int> currentEntries = currentDirIndex == 0
                                                         ? partition.RootDirectoryCache
                                                         : GetDirectoryEntries(partition, currentDirIndex);

            KeyValuePair<string, int> match =
                currentEntries.FirstOrDefault(e => e.Key.Equals(pieces[p], StringComparison.OrdinalIgnoreCase));

            if(match.Key is null) return ErrorNumber.NoSuchFile;

            int idx = match.Value;

            if(p < pieces.Length - 1)
            {
                // Intermediate components must be directories
                // Virtual files (negative indices) cannot be directories
                if(idx < 0) return ErrorNumber.NotDirectory;

                if(partition.FstEntries[idx].TypeAndNameOffset >> 24 == 0) return ErrorNumber.NotDirectory;

                currentDirIndex = idx;
            }
            else
            {
                // Final component — can be file or directory
                entryIndex = idx;
            }
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Get the path within a partition, stripping the partition prefix in multi-partition mode</summary>
    string GetInPartitionPath(string path)
    {
        string cutPath = path.TrimStart('/');

        if(!_multiPartition || string.IsNullOrEmpty(cutPath)) return cutPath;

        int slashIdx = cutPath.IndexOf('/');

        return slashIdx >= 0 ? cutPath[(slashIdx + 1)..] : "";
    }

#region IReadOnlyFilesystem Members

    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        ErrorNumber errno = ResolvePath(path, out int partitionIndex, out int entryIndex);

        if(errno != ErrorNumber.NoError) return errno;

        // Virtual root in multi-partition mode
        if(partitionIndex < 0)
        {
            stat = new FileEntryInfo
            {
                Attributes = FileAttributes.Directory,
                BlockSize  = 2048,
                Blocks     = 0,
                Length     = 0
            };

            return ErrorNumber.NoError;
        }

        PartitionInfo partition = _partitions[partitionIndex];

        // Partition root (or root of single partition)
        if(entryIndex == 0)
        {
            stat = new FileEntryInfo
            {
                Attributes = FileAttributes.Directory,
                BlockSize  = 2048,
                Blocks     = 0,
                Length     = 0
            };

            return ErrorNumber.NoError;
        }

        // Handle virtual files
        if(entryIndex is DOL_VIRTUAL_INDEX or BOOT_BIN_VIRTUAL_INDEX or BI2_BIN_VIRTUAL_INDEX)
        {
            long virtualSize = entryIndex switch
                               {
                                   DOL_VIRTUAL_INDEX      => partition.DolSize,
                                   BOOT_BIN_VIRTUAL_INDEX => BOOT_BIN_SIZE,
                                   BI2_BIN_VIRTUAL_INDEX  => BI2_BIN_SIZE,
                                   _                      => 0
                               };

            stat = new FileEntryInfo
            {
                Attributes = FileAttributes.File,
                BlockSize  = 2048,
                Blocks     = (virtualSize + 2047) / 2048,
                Length     = virtualSize
            };

            return ErrorNumber.NoError;
        }

        bool isDirectory = partition.FstEntries[entryIndex].TypeAndNameOffset >> 24 != 0;

        if(isDirectory)
        {
            stat = new FileEntryInfo
            {
                Attributes = FileAttributes.Directory,
                BlockSize  = 2048,
                Blocks     = 0,
                Length     = 0
            };
        }
        else
        {
            long length = partition.FstEntries[entryIndex].SizeOrNext;

            stat = new FileEntryInfo
            {
                Attributes = FileAttributes.File,
                BlockSize  = 2048,
                Blocks     = (length + 2047) / 2048,
                Length     = length
            };
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber OpenFile(string path, out IFileNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        ErrorNumber errno = ResolvePath(path, out int partitionIndex, out int entryIndex);

        if(errno != ErrorNumber.NoError) return errno;

        // Virtual root and partition roots are directories
        if(partitionIndex < 0) return ErrorNumber.IsDirectory;

        PartitionInfo partition = _partitions[partitionIndex];

        if(entryIndex == 0) return ErrorNumber.IsDirectory;

        // Handle virtual files
        if(entryIndex is DOL_VIRTUAL_INDEX or BOOT_BIN_VIRTUAL_INDEX or BI2_BIN_VIRTUAL_INDEX)
        {
            long virtualSize = entryIndex switch
                               {
                                   DOL_VIRTUAL_INDEX      => partition.DolSize,
                                   BOOT_BIN_VIRTUAL_INDEX => BOOT_BIN_SIZE,
                                   BI2_BIN_VIRTUAL_INDEX  => BI2_BIN_SIZE,
                                   _                      => 0
                               };

            node = new NintendoFileNode
            {
                Path           = path,
                Length         = virtualSize,
                Offset         = 0,
                FstIndex       = entryIndex,
                PartitionIndex = partitionIndex
            };

            return ErrorNumber.NoError;
        }

        if(partition.FstEntries[entryIndex].TypeAndNameOffset >> 24 != 0) return ErrorNumber.IsDirectory;

        node = new NintendoFileNode
        {
            Path           = path,
            Length         = partition.FstEntries[entryIndex].SizeOrNext,
            Offset         = 0,
            FstIndex       = entryIndex,
            PartitionIndex = partitionIndex
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(node is not NintendoFileNode) return ErrorNumber.InvalidArgument;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not NintendoFileNode myNode) return ErrorNumber.InvalidArgument;

        if(myNode.Offset >= myNode.Length) return ErrorNumber.NoError;

        long remaining = myNode.Length - myNode.Offset;

        if(length > remaining) length = remaining;

        if(length > buffer.Length) length = buffer.Length;

        PartitionInfo partition = _partitions[myNode.PartitionIndex];

        // Get the file's data offset from the FST entry or virtual file offset
        // Wii FST entries store file offsets divided by 4
        ulong fileDataOffset = myNode.FstIndex switch
                               {
                                   DOL_VIRTUAL_INDEX      => partition.DolOffset,
                                   BOOT_BIN_VIRTUAL_INDEX => BOOT_BIN_OFFSET,
                                   BI2_BIN_VIRTUAL_INDEX  => BI2_BIN_OFFSET,
                                   _ => _isWii
                                            ? (ulong)partition.FstEntries[myNode.FstIndex].OffsetOrParent << 2
                                            : partition.FstEntries[myNode.FstIndex].OffsetOrParent
                               };

        if(_isWiiU)
        {
            // Wii U: read from encrypted partition using cluster offsets
            ulong  fileOff   = (ulong)partition.FstEntries[myNode.FstIndex].OffsetOrParent << 5;
            ushort clusterId = partition.WiiuFstEntries?[myNode.FstIndex].ClusterIndex ?? 0;

            ulong clusterOff = partition.WiiuClusterOffsets != null && clusterId < partition.WiiuClusterOffsets.Length
                                   ? partition.WiiuClusterOffsets[clusterId]
                                   : 0;

            ulong volumeOff = clusterOff + fileOff + (ulong)myNode.Offset;

            byte[] data =
                ReadWiiuVolumeDecrypted(partition.WiiuKey, partition.PartitionOffset, volumeOff, (uint)length);

            if(data == null) return ErrorNumber.InOutError;

            Array.Copy(data, 0, buffer, 0, length);
        }
        else if(_isWii)
        {
            // Wii: read from encrypted partition data
            byte[] data = ReadWiiPartitionData(partition, fileDataOffset + (ulong)myNode.Offset, (uint)length);

            if(data == null) return ErrorNumber.InOutError;

            Array.Copy(data, 0, buffer, 0, length);
        }
        else
        {
            // GameCube: read directly from image, accounting for sub-sector offset
            ulong absoluteOffset = fileDataOffset + (ulong)myNode.Offset;
            uint  sectorSize     = _imagePlugin.Info.SectorSize;
            ulong startSector    = absoluteOffset / sectorSize;
            var   sectorOffset   = (uint)(absoluteOffset % sectorSize);
            uint  sectorsToRead  = (sectorOffset + (uint)length + sectorSize - 1) / sectorSize;

            ErrorNumber errno =
                _imagePlugin.ReadSectors(startSector, false, sectorsToRead, out byte[] sectorData, out _);

            if(errno != ErrorNumber.NoError) return errno;

            Array.Copy(sectorData, sectorOffset, buffer, 0, length);
        }

        read          =  length;
        myNode.Offset += length;

        return ErrorNumber.NoError;
    }

#endregion
}