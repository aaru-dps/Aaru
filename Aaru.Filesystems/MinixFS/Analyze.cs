// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Analyze.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : MINIX filesystem plugin.
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

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class MinixFS
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
                                                       ROOT_INODE,
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

    ErrorNumber TraverseDirectoryForAffectedSectors(string path, uint directoryInodeNumber,
                                                    IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                                                    List<FileSectorInfo> files, UpdateProgressHandler updateProgress,
                                                    PulseProgressHandler pulseProgress)
    {
        pulseProgress?.Invoke(path);

        ErrorNumber errno = ReadInode(directoryInodeNumber, out object directoryInodeObj);

        if(errno != ErrorNumber.NoError) return errno;

        ushort mode;
        uint[] zones;
        uint   size;
        int    directZones;

        GetInodeInformation(directoryInodeObj, out mode, out size, out zones, out directZones);

        if((mode & (ushort)InodeMode.TypeMask) != (ushort)InodeMode.Directory) return ErrorNumber.NotDirectory;

        errno = GetDirectoryEntries(directoryInodeNumber, out Dictionary<string, uint> directoryEntries);

        if(errno != ErrorNumber.NoError) return errno;

        KeyValuePair<string, uint>[] orderedEntries = directoryEntries
                                                     .Where(static entry =>
                                                                !string.IsNullOrWhiteSpace(entry.Key) &&
                                                                entry.Key is not "." and not "..")
                                                     .OrderBy(static entry => entry.Key, StringComparer.Ordinal)
                                                     .ToArray();

        long maximum = orderedEntries.Length > 0 ? orderedEntries.Length : 1;

        for(var i = 0; i < orderedEntries.Length; i++)
        {
            string entryPath = path == "/" ? "/" + orderedEntries[i].Key : path + "/" + orderedEntries[i].Key;

            updateProgress?.Invoke(entryPath, i + 1, maximum);

            errno = ReadInode(orderedEntries[i].Value, out object inodeObj);

            if(errno != ErrorNumber.NoError) return errno;

            GetInodeInformation(inodeObj, out mode, out size, out zones, out directZones);

            if((mode & (ushort)InodeMode.TypeMask) == (ushort)InodeMode.Directory)
            {
                errno = TraverseDirectoryForAffectedSectors(entryPath,
                                                            orderedEntries[i].Value,
                                                            sectorExtents,
                                                            files,
                                                            updateProgress,
                                                            pulseProgress);

                if(errno != ErrorNumber.NoError) return errno;

                continue;
            }

            errno = AddOverlappingFile(entryPath,
                                       orderedEntries[i].Value,
                                       size,
                                       zones,
                                       directZones,
                                       sectorExtents,
                                       files);

            if(errno != ErrorNumber.NoError) return errno;
        }

        return ErrorNumber.NoError;
    }

    ErrorNumber GetDirectoryEntries(uint directoryInodeNumber, out Dictionary<string, uint> directoryEntries)
    {
        if(directoryInodeNumber == ROOT_INODE)
        {
            directoryEntries = new Dictionary<string, uint>(_rootDirectoryCache, StringComparer.Ordinal);

            return ErrorNumber.NoError;
        }

        return GetDirectoryContents(directoryInodeNumber, out directoryEntries);
    }

    static void GetInodeInformation(object  inodeObj, out ushort mode, out uint size, out uint[] zones,
                                    out int directZones)
    {
        if(inodeObj is V1DiskInode v1Inode)
        {
            mode  = v1Inode.d1_mode;
            size  = v1Inode.d1_size;
            zones = new uint[v1Inode.d1_zone.Length];

            for(var i = 0; i < v1Inode.d1_zone.Length; i++) zones[i] = v1Inode.d1_zone[i];

            directZones = V1_NR_DZONES;

            return;
        }

        var v2Inode = (V2DiskInode)inodeObj;
        mode        = v2Inode.d2_mode;
        size        = v2Inode.d2_size;
        zones       = v2Inode.d2_zone;
        directZones = V2_NR_DZONES;
    }

    ErrorNumber AddOverlappingFile(string path, uint inodeNumber, uint logicalSize, uint[] zones, int directZones,
                                   IReadOnlyList<(ulong Start, ulong End)> sectorExtents, List<FileSectorInfo> files)
    {
        if(logicalSize == 0) return ErrorNumber.NoError;

        ErrorNumber errno = FindOverlappingExtents(logicalSize,
                                                   zones,
                                                   directZones,
                                                   sectorExtents,
                                                   out List<(ulong Start, ulong End)> overlaps);

        if(errno != ErrorNumber.NoError) return errno;

        if(overlaps.Count == 0) return ErrorNumber.NoError;

        files.Add(new FileSectorInfo
        {
            Path            = path,
            Inode           = inodeNumber,
            AffectedSectors = overlaps
        });

        return ErrorNumber.NoError;
    }

    ErrorNumber FindOverlappingExtents(uint logicalSize, uint[] zones, int directZones,
                                       IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                                       out List<(ulong Start, ulong End)> overlaps)
    {
        overlaps = [];

        if(logicalSize == 0) return ErrorNumber.NoError;

        ulong remainingBytes = logicalSize;
        ulong sectorSize     = _imagePlugin.Info.SectorSize;
        var   totalBlocks    = (int)((logicalSize + (uint)_blockSize - 1) / (uint)_blockSize);

        for(var logicalBlock = 0; logicalBlock < totalBlocks && remainingBytes > 0; logicalBlock++)
        {
            ulong bytesInBlock = Math.Min((ulong)_blockSize, remainingBytes);

            ErrorNumber errno = ReadMap(zones, directZones, logicalBlock, out int physicalBlock);

            if(errno != ErrorNumber.NoError) return errno;

            if(physicalBlock == 0)
            {
                remainingBytes -= bytesInBlock;

                continue;
            }

            ulong byteOffset      = (ulong)physicalBlock * (ulong)_blockSize;
            ulong startSector     = _partition.Start + byteOffset / sectorSize;
            ulong offsetInSector  = byteOffset                                       % sectorSize;
            ulong sectorsInExtent = (bytesInBlock + offsetInSector + sectorSize - 1) / sectorSize;

            if(sectorsInExtent == 0) sectorsInExtent = 1;

            AddExtentOverlaps(startSector, startSector + sectorsInExtent - 1, sectorExtents, overlaps);

            remainingBytes -= bytesInBlock;
        }

        overlaps = NormalizeAnalyzeExtents(overlaps);

        return ErrorNumber.NoError;
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