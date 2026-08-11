// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Analyze.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple ProDOS filesystem plugin.
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
using Aaru.CommonTypes.Structs;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class ProDOSPlugin
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

            return TraverseDirectoryForAffectedSectors("/",
                                                       2,
                                                       true,
                                                       normalizedExtents,
                                                       files,
                                                       updateProgress,
                                                       pulseProgress);
        }
        finally
        {
            endProgress?.Invoke();
        }
    }

#endregion

    ErrorNumber TraverseDirectoryForAffectedSectors(string path, ushort directoryKeyBlock, bool isVolumeDirectory,
                                                    IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                                                    List<FileSectorInfo> files, UpdateProgressHandler updateProgress,
                                                    PulseProgressHandler pulseProgress)
    {
        pulseProgress?.Invoke(path);

        ErrorNumber errno = GetDirectoryEntries(directoryKeyBlock,
                                                isVolumeDirectory,
                                                out Dictionary<string, CachedEntry> entries);

        if(errno != ErrorNumber.NoError) return errno;

        KeyValuePair<string, CachedEntry>[] orderedEntries = entries
                                                            .Where(static entry =>
                                                                       !string.IsNullOrWhiteSpace(entry.Key) &&
                                                                       entry.Key is not "." and not "..")
                                                            .OrderBy(static entry => entry.Key,
                                                                     StringComparer.OrdinalIgnoreCase)
                                                            .ToArray();

        long maximum = orderedEntries.Length > 0 ? orderedEntries.Length : 1;

        for(var i = 0; i < orderedEntries.Length; i++)
        {
            string      entryPath = path == "/" ? "/" + orderedEntries[i].Key : path + "/" + orderedEntries[i].Key;
            CachedEntry entry     = orderedEntries[i].Value;

            updateProgress?.Invoke(entryPath, i + 1, maximum);

            if(entry.IsDirectory)
            {
                errno = TraverseDirectoryForAffectedSectors(entryPath,
                                                            entry.KeyBlock,
                                                            false,
                                                            sectorExtents,
                                                            files,
                                                            updateProgress,
                                                            pulseProgress);

                if(errno != ErrorNumber.NoError) return errno;

                continue;
            }

            errno = AddOverlappingForks(entryPath, entry, sectorExtents, files);

            if(errno != ErrorNumber.NoError) return errno;
        }

        return ErrorNumber.NoError;
    }

    ErrorNumber GetDirectoryEntries(ushort                              directoryKeyBlock, bool isVolumeDirectory,
                                    out Dictionary<string, CachedEntry> entries)
    {
        if(isVolumeDirectory)
        {
            entries = new Dictionary<string, CachedEntry>(_rootDirectoryCache, StringComparer.OrdinalIgnoreCase);

            return ErrorNumber.NoError;
        }

        return ReadDirectoryContents(directoryKeyBlock, false, out entries);
    }

    ErrorNumber AddOverlappingForks(string                                  path,          CachedEntry          entry,
                                    IReadOnlyList<(ulong Start, ulong End)> sectorExtents, List<FileSectorInfo> files)
    {
        if(entry.StorageType == EXTENDED_FILE_TYPE)
        {
            ErrorNumber errno = ReadBlock(entry.KeyBlock, out byte[] extBlock);

            if(errno != ErrorNumber.NoError) return errno;

            ExtendedKeyBlock extKeyBlock = Marshal.ByteArrayToStructureLittleEndian<ExtendedKeyBlock>(extBlock);

            var dataStorageType = extKeyBlock.data_fork.storage_type;

            var dataForkEof = (uint)(extKeyBlock.data_fork.eof[0]      |
                                     extKeyBlock.data_fork.eof[1] << 8 |
                                     extKeyBlock.data_fork.eof[2] << 16);

            errno = AddForkOverlaps(path,
                                    null,
                                    entry.KeyBlock,
                                    dataStorageType,
                                    extKeyBlock.data_fork.key_block,
                                    dataForkEof,
                                    sectorExtents,
                                    files);

            if(errno != ErrorNumber.NoError) return errno;

            var resourceStorageType = extKeyBlock.resource_fork.storage_type;

            var resourceForkEof = (uint)(extKeyBlock.resource_fork.eof[0]      |
                                         extKeyBlock.resource_fork.eof[1] << 8 |
                                         extKeyBlock.resource_fork.eof[2] << 16);

            return AddForkOverlaps(path,
                                   Xattrs.XATTR_APPLE_RESOURCE_FORK,
                                   entry.KeyBlock,
                                   resourceStorageType,
                                   extKeyBlock.resource_fork.key_block,
                                   resourceForkEof,
                                   sectorExtents,
                                   files);
        }

        return AddForkOverlaps(path,
                               null,
                               entry.KeyBlock,
                               entry.StorageType,
                               entry.KeyBlock,
                               entry.Eof,
                               sectorExtents,
                               files);
    }

    ErrorNumber AddForkOverlaps(string path, string stream, ushort inode, byte storageType, ushort keyBlock,
                                uint logicalSize, IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                                List<FileSectorInfo> files)
    {
        if(logicalSize == 0) return ErrorNumber.NoError;

        ErrorNumber errno = FindOverlappingExtents(storageType,
                                                   keyBlock,
                                                   logicalSize,
                                                   sectorExtents,
                                                   out List<(ulong Start, ulong End)> overlaps);

        if(errno != ErrorNumber.NoError) return errno;

        if(overlaps.Count == 0) return ErrorNumber.NoError;

        files.Add(new FileSectorInfo
        {
            Path            = path,
            Stream          = stream,
            Inode           = inode,
            AffectedSectors = overlaps
        });

        return ErrorNumber.NoError;
    }

    ErrorNumber FindOverlappingExtents(byte storageType, ushort keyBlock, uint logicalSize,
                                       IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                                       out List<(ulong Start, ulong End)> overlaps)
    {
        overlaps = [];

        if(logicalSize == 0) return ErrorNumber.NoError;

        ulong remainingBytes = logicalSize;

        switch(storageType)
        {
            case SEEDLING_FILE_TYPE:
                AddBlockOverlaps(keyBlock, Math.Min(remainingBytes, 512), sectorExtents, overlaps);

                break;

            case SAPLING_FILE_TYPE:
            {
                ErrorNumber errno = ReadBlock(keyBlock, out byte[] indexBlockData);

                if(errno != ErrorNumber.NoError) return errno;

                IndirectBlock indexBlock = Marshal.ByteArrayToStructureLittleEndian<IndirectBlock>(indexBlockData);

                for(var i = 0; i < 256 && remainingBytes > 0; i++)
                {
                    var   blockPointer = (ushort)(indexBlock.lsbyte[i] | indexBlock.msbyte[i] << 8);
                    ulong bytesInBlock = Math.Min(remainingBytes, 512);

                    if(blockPointer != 0) AddBlockOverlaps(blockPointer, bytesInBlock, sectorExtents, overlaps);

                    remainingBytes -= bytesInBlock;
                }

                break;
            }

            case TREE_FILE_TYPE:
            {
                ErrorNumber errno = ReadBlock(keyBlock, out byte[] masterIndexBlockData);

                if(errno != ErrorNumber.NoError) return errno;

                IndirectBlock masterIndex =
                    Marshal.ByteArrayToStructureLittleEndian<IndirectBlock>(masterIndexBlockData);

                for(var i = 0; i < 256 && remainingBytes > 0; i++)
                {
                    var indexBlockPointer = (ushort)(masterIndex.lsbyte[i] | masterIndex.msbyte[i] << 8);

                    if(indexBlockPointer == 0)
                    {
                        remainingBytes -= Math.Min(remainingBytes, 256UL * 512);

                        continue;
                    }

                    errno = ReadBlock(indexBlockPointer, out byte[] subIndexBlockData);

                    if(errno != ErrorNumber.NoError) return errno;

                    IndirectBlock subIndex = Marshal.ByteArrayToStructureLittleEndian<IndirectBlock>(subIndexBlockData);

                    for(var j = 0; j < 256 && remainingBytes > 0; j++)
                    {
                        var   blockPointer = (ushort)(subIndex.lsbyte[j] | subIndex.msbyte[j] << 8);
                        ulong bytesInBlock = Math.Min(remainingBytes, 512);

                        if(blockPointer != 0) AddBlockOverlaps(blockPointer, bytesInBlock, sectorExtents, overlaps);

                        remainingBytes -= bytesInBlock;
                    }
                }

                break;
            }

            case PASCAL_AREA_TYPE:
            {
                uint blocksUsed = (logicalSize + 511) / 512;

                for(uint blockOffset = 0; blockOffset < blocksUsed && remainingBytes > 0; blockOffset++)
                {
                    ulong bytesInBlock = Math.Min(remainingBytes, 512);
                    AddBlockOverlaps((ushort)(keyBlock + blockOffset), bytesInBlock, sectorExtents, overlaps);
                    remainingBytes -= bytesInBlock;
                }

                break;
            }

            default:
                return ErrorNumber.NoError;
        }

        overlaps = NormalizeAnalyzeExtents(overlaps);

        return ErrorNumber.NoError;
    }

    void AddBlockOverlaps(ushort blockNumber, ulong bytesInBlock, IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                          List<(ulong Start, ulong End)> overlaps)
    {
        if(blockNumber == 0 || bytesInBlock == 0) return;

        ulong sectorSize     = _imagePlugin.Info.SectorSize;
        ulong startSector    = _partition.Start + (ulong)blockNumber * _multiplier;
        ulong sectorsInBlock = (bytesInBlock + sectorSize - 1) / sectorSize;

        if(sectorsInBlock == 0) sectorsInBlock = 1;

        AddExtentOverlaps(startSector, startSector + sectorsInBlock - 1, sectorExtents, overlaps);
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