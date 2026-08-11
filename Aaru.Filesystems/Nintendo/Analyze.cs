// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Analyze.cs
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
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements the filesystem used by Nintendo Gamecube and Wii discs</summary>
public sealed partial class NintendoPlugin
{
#region IReadOnlyFilesystem Members

    /// <inheritdoc />
    public ErrorNumber GetFilesWithAffectedSectors(IEnumerable<(ulong Start, ulong End)> sectorExtents,
                                                   out List<FileSectorInfo>              files,
                                                   InitProgressHandler                   initProgress   = null,
                                                   UpdateProgressHandler                 updateProgress = null,
                                                   PulseProgressHandler                  pulseProgress  = null,
                                                   EndProgressHandler                    endProgress    = null)
    {
        files = [];

        if(!_mounted) return ErrorNumber.AccessDenied;
        if(sectorExtents is null) return ErrorNumber.InvalidArgument;

        List<(ulong Start, ulong End)> normalizedExtents = NormalizeAnalyzeExtents(sectorExtents);

        if(normalizedExtents.Count == 0) return ErrorNumber.NoError;

        try
        {
            initProgress?.Invoke();

            return TraverseDirectoryForAffectedSectors("/", normalizedExtents, files, updateProgress, pulseProgress);
        }
        finally
        {
            endProgress?.Invoke();
        }
    }

#endregion

    ErrorNumber TraverseDirectoryForAffectedSectors(string path, IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                                                    List<FileSectorInfo> files, UpdateProgressHandler updateProgress,
                                                    PulseProgressHandler pulseProgress)
    {
        pulseProgress?.Invoke(path);

        ErrorNumber errno = OpenDir(path, out IDirNode node);

        if(errno != ErrorNumber.NoError) return errno;

        try
        {
            List<string> entries = [];

            while(true)
            {
                errno = ReadDir(node, out string entryName);

                if(errno != ErrorNumber.NoError) return errno;

                if(entryName is null) break;

                entries.Add(entryName);
            }

            string[] orderedEntries = entries
                                     .Where(static entry =>
                                                !string.IsNullOrWhiteSpace(entry) && entry is not "." and not "..")
                                     .OrderBy(static entry => entry, StringComparer.OrdinalIgnoreCase)
                                     .ToArray();

            long maximum = orderedEntries.Length > 0 ? orderedEntries.Length : 1;

            for(var i = 0; i < orderedEntries.Length; i++)
            {
                string entryPath = path == "/" ? "/" + orderedEntries[i] : path + "/" + orderedEntries[i];

                updateProgress?.Invoke(entryPath, i + 1, maximum);

                errno = Stat(entryPath, out FileEntryInfo stat);

                if(errno != ErrorNumber.NoError) return errno;

                if(stat.Attributes.HasFlag(FileAttributes.Directory))
                {
                    errno = TraverseDirectoryForAffectedSectors(entryPath,
                                                                sectorExtents,
                                                                files,
                                                                updateProgress,
                                                                pulseProgress);

                    if(errno != ErrorNumber.NoError) return errno;

                    continue;
                }

                errno = AddOverlappingFile(entryPath, sectorExtents, files);

                if(errno != ErrorNumber.NoError) return errno;
            }

            return ErrorNumber.NoError;
        }
        finally
        {
            CloseDir(node);
        }
    }

    ErrorNumber AddOverlappingFile(string               path, IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                                   List<FileSectorInfo> files)
    {
        ErrorNumber errno = ResolvePath(path, out int partitionIndex, out int entryIndex);

        if(errno          != ErrorNumber.NoError) return errno;
        if(partitionIndex < 0) return ErrorNumber.NoError;
        if(entryIndex     == 0) return ErrorNumber.NoError;

        PartitionInfo partition = _partitions[partitionIndex];

        if(entryIndex >= 0 && partition.FstEntries[entryIndex].TypeAndNameOffset >> 24 != 0) return ErrorNumber.NoError;

        errno = FindOverlappingExtents(partition,
                                       entryIndex,
                                       sectorExtents,
                                       out List<(ulong Start, ulong End)> overlaps);

        if(errno          != ErrorNumber.NoError) return errno;
        if(overlaps.Count == 0) return ErrorNumber.NoError;

        uint rawId = entryIndex >= 0 ? (uint)entryIndex : unchecked((uint)entryIndex);

        files.Add(new FileSectorInfo
        {
            Path            = path,
            Inode           = (ulong)(uint)partitionIndex << 32 | rawId,
            AffectedSectors = overlaps
        });

        return ErrorNumber.NoError;
    }

    ErrorNumber FindOverlappingExtents(PartitionInfo                           partition, int entryIndex,
                                       IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                                       out List<(ulong Start, ulong End)>      overlaps)
    {
        overlaps = [];

        ulong fileDataOffset = entryIndex switch
                               {
                                   DOL_VIRTUAL_INDEX      => partition.DolOffset,
                                   BOOT_BIN_VIRTUAL_INDEX => BOOT_BIN_OFFSET,
                                   BI2_BIN_VIRTUAL_INDEX  => BI2_BIN_OFFSET,

                                   // Wii FST entries store file offsets divided by 4
                                   _ when entryIndex >= 0 => _isWii
                                                                 ? (ulong)partition.FstEntries[entryIndex]
                                                                      .OffsetOrParent <<
                                                                   2
                                                                 : partition.FstEntries[entryIndex].OffsetOrParent,
                                   _ => 0UL
                               };

        ulong length = entryIndex switch
                       {
                           DOL_VIRTUAL_INDEX      => partition.DolSize,
                           BOOT_BIN_VIRTUAL_INDEX => BOOT_BIN_SIZE,
                           BI2_BIN_VIRTUAL_INDEX  => BI2_BIN_SIZE,
                           _ when entryIndex >= 0 => partition.FstEntries[entryIndex].SizeOrNext,
                           _                      => 0UL
                       };

        if(length == 0) return ErrorNumber.NoError;

        if(_isWiiU)
        {
            ulong clusterOffset = 0;

            if(entryIndex >= 0)
            {
                fileDataOffset <<= 5;

                if(partition.WiiuFstEntries != null && partition.WiiuClusterOffsets != null)
                {
                    ushort clusterIndex = partition.WiiuFstEntries[entryIndex].ClusterIndex;

                    if(clusterIndex < partition.WiiuClusterOffsets.Length)
                        clusterOffset = partition.WiiuClusterOffsets[clusterIndex];
                }
            }

            AddAbsoluteByteRangeOverlaps(partition.PartitionOffset + clusterOffset + fileDataOffset,
                                         length,
                                         sectorExtents,
                                         overlaps);
        }
        else if(_isWii)
        {
            ulong currentOffset  = fileDataOffset;
            ulong remainingBytes = length;

            while(remainingBytes > 0)
            {
                ulong clusterIndex    = currentOffset / WII_CLUSTER_DATA_SIZE;
                ulong offsetInCluster = currentOffset % WII_CLUSTER_DATA_SIZE;
                ulong bytesInChunk    = Math.Min(remainingBytes, WII_CLUSTER_DATA_SIZE - offsetInCluster);

                ulong physicalOffset = partition.PartitionOffset       +
                                       partition.PartitionDataOffset   +
                                       clusterIndex * WII_CLUSTER_SIZE +
                                       WII_CLUSTER_HASH_SIZE           +
                                       offsetInCluster;

                AddAbsoluteByteRangeOverlaps(physicalOffset, bytesInChunk, sectorExtents, overlaps);

                currentOffset  += bytesInChunk;
                remainingBytes -= bytesInChunk;
            }
        }
        else
            AddAbsoluteByteRangeOverlaps(fileDataOffset, length, sectorExtents, overlaps);

        overlaps = NormalizeAnalyzeExtents(overlaps);

        return ErrorNumber.NoError;
    }

    void AddAbsoluteByteRangeOverlaps(ulong                                   absoluteByteOffset, ulong lengthBytes,
                                      IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                                      List<(ulong Start, ulong End)>          overlaps)
    {
        if(lengthBytes == 0) return;

        ulong sectorSize     = _imagePlugin.Info.SectorSize;
        ulong startSector    = absoluteByteOffset                              / sectorSize;
        ulong offsetInSector = absoluteByteOffset                              % sectorSize;
        ulong sectorCount    = (lengthBytes + offsetInSector + sectorSize - 1) / sectorSize;

        if(sectorCount == 0) sectorCount = 1;

        AddExtentOverlaps(startSector, startSector + sectorCount - 1, sectorExtents, overlaps);
    }

    static void AddExtentOverlaps(ulong                                   startSector, ulong endSector,
                                  IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                                  List<(ulong Start, ulong End)>          overlaps)
    {
        foreach((ulong Start, ulong End) requestedExtent in sectorExtents)
        {
            if(requestedExtent.End < startSector || requestedExtent.Start > endSector) continue;

            overlaps.Add((Math.Max(startSector, requestedExtent.Start), Math.Min(endSector, requestedExtent.End)));
        }
    }

    static List<(ulong Start, ulong End)> NormalizeAnalyzeExtents(IEnumerable<(ulong Start, ulong End)> extents)
    {
        var orderedExtents = extents.Where(static extent => extent.End >= extent.Start)
                                    .OrderBy(static extent => extent.Start)
                                    .ThenBy(static extent => extent.End)
                                    .ToList();

        List<(ulong Start, ulong End)> normalizedExtents = [];

        if(orderedExtents.Count == 0) return normalizedExtents;

        (ulong Start, ulong End) currentExtent = orderedExtents[0];

        for(var i = 1; i < orderedExtents.Count; i++)
        {
            if(orderedExtents[i].Start <= currentExtent.End + 1)
            {
                currentExtent.End = Math.Max(currentExtent.End, orderedExtents[i].End);

                continue;
            }

            normalizedExtents.Add(currentExtent);
            currentExtent = orderedExtents[i];
        }

        normalizedExtents.Add(currentExtent);

        return normalizedExtents;
    }
}