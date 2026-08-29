// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Analyze.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Files-11 On-Disk Structure plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Sector overlap analysis for the Files-11 On-Disk Structure.
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

public sealed partial class ODS
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
                                                       _rootDirectoryCache,
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

    ErrorNumber TraverseDirectoryForAffectedSectors(string path, Dictionary<string, CachedFile> directoryEntries,
                                                    IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                                                    List<FileSectorInfo> files, UpdateProgressHandler updateProgress,
                                                    PulseProgressHandler pulseProgress)
    {
        pulseProgress?.Invoke(path);

        (string Filename, CachedFile File)[] orderedEntries = GetDirectoryEntries(directoryEntries);
        long                                 maximum        = orderedEntries.Length > 0 ? orderedEntries.Length : 1;

        for(var i = 0; i < orderedEntries.Length; i++)
        {
            string entryPath = path == "/" ? "/" + orderedEntries[i].Filename : path + "/" + orderedEntries[i].Filename;
            CachedFile cachedFile = orderedEntries[i].File;

            updateProgress?.Invoke(entryPath, i + 1, maximum);

            ErrorNumber errno = ReadFileHeader(cachedFile.Fid, out FileHeader fileHeader);

            if(errno != ErrorNumber.NoError) return errno;

            if(fileHeader.filechar.HasFlag(FileCharacteristicFlags.Directory))
            {
                errno = ReadDirectoryEntries(fileHeader,
                                             out Dictionary<string, CachedFile> subDirectory,
                                             cachedFile.Fid);

                if(errno != ErrorNumber.NoError) return errno;

                errno = TraverseDirectoryForAffectedSectors(entryPath,
                                                            subDirectory,
                                                            sectorExtents,
                                                            files,
                                                            updateProgress,
                                                            pulseProgress);

                if(errno != ErrorNumber.NoError) return errno;

                continue;
            }

            errno = AddOverlappingFile(entryPath, cachedFile, fileHeader, sectorExtents, files);

            if(errno != ErrorNumber.NoError) return errno;
        }

        return ErrorNumber.NoError;
    }

    ErrorNumber AddOverlappingFile(string path, CachedFile cachedFile, FileHeader fileHeader,
                                   IReadOnlyList<(ulong Start, ulong End)> sectorExtents, List<FileSectorInfo> files)
    {
        uint efblk       = fileHeader.recattr.efblk.Value;
        long logicalSize = efblk > 0 ? (efblk - 1) * ODS_BLOCK_SIZE + fileHeader.recattr.ffbyte : 0;

        if(logicalSize <= 0) return ErrorNumber.NoError;

        ErrorNumber errno = FindOverlappingExtents(fileHeader,
                                                   (ulong)logicalSize,
                                                   sectorExtents,
                                                   out List<(ulong Start, ulong End)> overlaps);

        if(errno != ErrorNumber.NoError) return errno;

        if(overlaps.Count == 0) return ErrorNumber.NoError;

        files.Add(new FileSectorInfo
        {
            Path            = path,
            Inode           = cachedFile.Fid.num,
            AffectedSectors = overlaps
        });

        return ErrorNumber.NoError;
    }

    ErrorNumber FindOverlappingExtents(FileHeader                              fileHeader, ulong logicalSize,
                                       IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                                       out List<(ulong Start, ulong End)>      overlaps)
    {
        overlaps = [];

        byte[] mapData = GetMapData(fileHeader);

        if(logicalSize == 0 || mapData == null || mapData.Length == 0) return ErrorNumber.NoError;

        var fileNode = new OdsFileNode
        {
            FileHeader    = fileHeader,
            MapData       = mapData,
            ExtensionMaps = null
        };

        if(fileHeader.ext_fid.num != 0 || fileHeader.ext_fid.nmx != 0)
        {
            ErrorNumber loadErr = LoadExtensionHeaders(fileNode);

            if(loadErr != ErrorNumber.NoError) return loadErr;
        }

        ulong remainingBytes = logicalSize;
        uint  vbn            = 1;

        while(remainingBytes > 0)
        {
            ErrorNumber errno = MapVbnToLbnMultiExtent(fileNode, vbn, out uint lbn, out uint extentBlocks);

            if(errno != ErrorNumber.NoError) return errno;

            if(extentBlocks == 0) extentBlocks = 1;

            ulong extentBytes = (ulong)extentBlocks * ODS_BLOCK_SIZE;
            ulong bytesInRun  = Math.Min(extentBytes, remainingBytes);

            if(bytesInRun == 0) break;

            ulong absoluteByteOffset = (ulong)lbn * ODS_BLOCK_SIZE;
            ulong startSector        = _partition.Start + absoluteByteOffset / _sectorSize;
            ulong offsetInSector     = absoluteByteOffset                              % _sectorSize;
            ulong sectorsInRun       = (bytesInRun + offsetInSector + _sectorSize - 1) / _sectorSize;

            if(sectorsInRun == 0) sectorsInRun = 1;

            AddExtentOverlaps(startSector, startSector + sectorsInRun - 1, sectorExtents, overlaps);

            remainingBytes -= bytesInRun;
            vbn            += extentBlocks;
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