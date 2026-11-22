// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Analyze.cs
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
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

namespace Aaru.Filesystems;

public sealed partial class BOFS
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
            List<string> directoryEntries = [];

            while(true)
            {
                errno = ReadDir(node, out string entryName);

                if(errno != ErrorNumber.NoError) return errno;

                if(entryName is null) break;

                directoryEntries.Add(entryName);
            }

            string[] orderedEntries = directoryEntries.Where(static name => !string.IsNullOrWhiteSpace(name))
                                                      .OrderBy(static name => name, StringComparer.Ordinal)
                                                      .ToArray();

            long maximum = orderedEntries.Length > 0 ? orderedEntries.Length : 1;

            for(var i = 0; i < orderedEntries.Length; i++)
            {
                string entryPath = path == "/" ? "/" + orderedEntries[i] : path + "/" + orderedEntries[i];

                updateProgress?.Invoke(entryPath, i + 1, maximum);

                errno = LookupEntry(entryPath, out FileEntry entry);

                if(errno != ErrorNumber.NoError) return errno;

                if(entry.FileType == DIR_TYPE)
                {
                    errno = TraverseDirectoryForAffectedSectors(entryPath,
                                                                sectorExtents,
                                                                files,
                                                                updateProgress,
                                                                pulseProgress);

                    if(errno != ErrorNumber.NoError) return errno;

                    continue;
                }

                errno = AddOverlappingFile(entryPath, entry, sectorExtents, files);

                if(errno != ErrorNumber.NoError) return errno;
            }

            return ErrorNumber.NoError;
        }
        finally
        {
            CloseDir(node);
        }
    }

    ErrorNumber AddOverlappingFile(string path, FileEntry entry, IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                                   List<FileSectorInfo> files)
    {
        if(entry.LogicalSize <= 0) return ErrorNumber.NoError;

        ErrorNumber errno = FindOverlappingExtents(entry, sectorExtents, out List<(ulong Start, ulong End)> overlaps);

        if(errno != ErrorNumber.NoError) return errno;

        if(overlaps.Count == 0) return ErrorNumber.NoError;

        files.Add(new FileSectorInfo
        {
            Path            = path,
            Inode           = (ulong)entry.RecordId,
            AffectedSectors = overlaps
        });

        return ErrorNumber.NoError;
    }

    ErrorNumber FindOverlappingExtents(FileEntry entry, IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                                       out List<(ulong Start, ulong End)> overlaps)
    {
        overlaps = [];

        if(entry.LogicalSize <= 0) return ErrorNumber.NoError;

        var         remainingBytes        = (ulong)entry.LogicalSize;
        ulong       sectorSize            = _imagePlugin.Info.SectorSize;
        const ulong bofsLogicalSectorSize = 512;

        if((entry.FirstAllocList & unchecked((int)0x80000000)) != 0)
        {
            ulong contiguousSector = (uint)(entry.FirstAllocList & 0x7FFFFFFF);
            ulong byteOffset       = contiguousSector * bofsLogicalSectorSize;
            ulong startSector      = _partition.Start + byteOffset / sectorSize;
            ulong offsetInSector   = byteOffset                                         % sectorSize;
            ulong sectorsInRun     = (remainingBytes + offsetInSector + sectorSize - 1) / sectorSize;

            if(sectorsInRun == 0) sectorsInRun = 1;

            AddExtentOverlaps(startSector, startSector + sectorsInRun - 1, sectorExtents, overlaps);
            overlaps = NormalizeAnalyzeExtents(overlaps);

            return ErrorNumber.NoError;
        }

        ErrorNumber errno = ReadExtents(entry.FirstAllocList, out List<(uint start, uint count)> extents);

        if(errno != ErrorNumber.NoError) return errno;

        foreach((uint startLogicalSector, uint sizeLogicalSectors) in extents)
        {
            if(remainingBytes == 0) break;

            if(sizeLogicalSectors == 0) continue;

            ulong extentBytes = sizeLogicalSectors * bofsLogicalSectorSize;
            ulong bytesInRun  = Math.Min(extentBytes, remainingBytes);

            if(bytesInRun == 0) continue;

            ulong byteOffset     = startLogicalSector * bofsLogicalSectorSize;
            ulong startSector    = _partition.Start + byteOffset / sectorSize;
            ulong offsetInSector = byteOffset                                     % sectorSize;
            ulong sectorsInRun   = (bytesInRun + offsetInSector + sectorSize - 1) / sectorSize;

            if(sectorsInRun == 0) sectorsInRun = 1;

            AddExtentOverlaps(startSector, startSector + sectorsInRun - 1, sectorExtents, overlaps);
            remainingBytes -= bytesInRun;
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