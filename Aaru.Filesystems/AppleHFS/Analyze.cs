// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Analyze.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Hierarchical File System plugin.
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

public sealed partial class AppleHFS
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
                                                       kRootCnid,
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

    ErrorNumber TraverseDirectoryForAffectedSectors(string path, uint directoryCnid,
                                                    List<(ulong Start, ulong End)> sectorExtents,
                                                    List<FileSectorInfo> files, UpdateProgressHandler updateProgress,
                                                    PulseProgressHandler pulseProgress)
    {
        pulseProgress?.Invoke(path);

        ErrorNumber errno = CacheDirectoryIfNeeded(directoryCnid);

        if(errno != ErrorNumber.NoError) return errno;

        Dictionary<string, CatalogEntry> directoryEntries = GetDirectoryEntries(directoryCnid);

        if(directoryEntries == null) return ErrorNumber.NoSuchFile;

        var orderedEntries = directoryEntries.Values.Where(static entry => entry != null)
                                             .OrderBy(static entry => entry.Name,
                                                      StringComparer.CurrentCultureIgnoreCase)
                                             .ToList();

        long maximum = orderedEntries.Count > 0 ? orderedEntries.Count : 1;

        for(var i = 0; i < orderedEntries.Count; i++)
        {
            CatalogEntry catalogEntry = orderedEntries[i];
            string       entryName    = NormalizePathComponent(catalogEntry.Name);
            string       entryPath    = path == "/" ? "/" + entryName : path + "/" + entryName;

            updateProgress?.Invoke(entryPath, i + 1, maximum);

            if(catalogEntry is DirectoryEntry directoryEntry)
            {
                errno = TraverseDirectoryForAffectedSectors(entryPath,
                                                            directoryEntry.CNID,
                                                            sectorExtents,
                                                            files,
                                                            updateProgress,
                                                            pulseProgress);

                if(errno != ErrorNumber.NoError) return errno;

                continue;
            }

            if(catalogEntry is FileEntry fileEntry)
            {
                errno = AddOverlappingForks(entryPath, fileEntry, sectorExtents, files);

                if(errno != ErrorNumber.NoError) return errno;
            }
        }

        return ErrorNumber.NoError;
    }

    ErrorNumber AddOverlappingForks(string                                  path,          FileEntry fileEntry,
                                    IReadOnlyList<(ulong Start, ulong End)> sectorExtents, List<FileSectorInfo> files)
    {
        ErrorNumber errno = AddForkOverlaps(path,
                                            null,
                                            fileEntry.CNID,
                                            ForkType.Data,
                                            fileEntry.DataForkLogicalSize,
                                            fileEntry.DataForkExtents,
                                            sectorExtents,
                                            files);

        if(errno != ErrorNumber.NoError) return errno;

        if(fileEntry.ResourceForkLogicalSize == 0) return ErrorNumber.NoError;

        return AddForkOverlaps(path,
                               Xattrs.XATTR_APPLE_RESOURCE_FORK,
                               fileEntry.CNID,
                               ForkType.Resource,
                               fileEntry.ResourceForkLogicalSize,
                               fileEntry.ResourceForkExtents,
                               sectorExtents,
                               files);
    }

    ErrorNumber AddForkOverlaps(string path, string stream, uint fileId, ForkType forkType, uint logicalSize,
                                ExtDataRec firstExtents, IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                                List<FileSectorInfo> files)
    {
        if(logicalSize == 0) return ErrorNumber.NoError;

        ErrorNumber errno = GetFileExtents(fileId, forkType, firstExtents, out List<ExtDescriptor> allExtents);

        if(errno != ErrorNumber.NoError) return errno;

        List<(ulong Start, ulong End)> overlaps = FindOverlappingExtents(allExtents, logicalSize, sectorExtents);

        if(overlaps.Count == 0) return ErrorNumber.NoError;

        files.Add(new FileSectorInfo
        {
            Path            = path,
            Stream          = stream,
            Inode           = fileId,
            AffectedSectors = overlaps
        });

        return ErrorNumber.NoError;
    }

    List<(ulong Start, ulong End)> FindOverlappingExtents(IReadOnlyList<ExtDescriptor> allExtents, uint logicalSize,
                                                          IReadOnlyList<(ulong Start, ulong End)> sectorExtents)
    {
        List<(ulong Start, ulong End)> overlaps       = [];
        ulong                          remainingBytes = logicalSize;

        foreach(ExtDescriptor extent in allExtents)
        {
            if(extent.xdrNumABlks == 0 || remainingBytes == 0) break;

            ulong extentSizeBytes = (ulong)extent.xdrNumABlks * _mdb.drAlBlkSiz;
            ulong bytesInExtent   = Math.Min(extentSizeBytes, remainingBytes);

            if(bytesInExtent == 0) break;

            ulong extentOffsetSector512 = (ulong)extent.xdrStABN * _mdb.drAlBlkSiz / 512;

            HfsOffsetToDeviceSector(extentOffsetSector512, out ulong deviceSector, out uint byteOffset);

            ulong sectorsInExtent = (bytesInExtent + byteOffset + _sectorSize - 1) / _sectorSize;

            if(sectorsInExtent == 0) sectorsInExtent = 1;

            AddExtentOverlaps(deviceSector, deviceSector + sectorsInExtent - 1, sectorExtents, overlaps);

            remainingBytes -= bytesInExtent;
        }

        return NormalizeAnalyzeExtents(overlaps);
    }

    static string NormalizePathComponent(string name) => name?.Replace("/", ":") ?? string.Empty;

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