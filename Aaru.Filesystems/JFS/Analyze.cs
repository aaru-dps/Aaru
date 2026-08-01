// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Analyze.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : IBM JFS filesystem plugin.
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
public sealed partial class JFS
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
                                                       ROOT_I,
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
                                                    List<FileSectorInfo> files,
                                                    UpdateProgressHandler updateProgress,
                                                    PulseProgressHandler pulseProgress)
    {
        pulseProgress?.Invoke(path);

        ErrorNumber errno = GetFilesetInode(directoryInodeNumber, out Inode directoryInode);

        if(errno != ErrorNumber.NoError) return errno;
        if((directoryInode.di_mode & 0xF000) != 0x4000) return ErrorNumber.NotDirectory;

        errno = GetDirectoryEntries(directoryInodeNumber, directoryInode, out Dictionary<string, uint> directoryEntries);

        if(errno != ErrorNumber.NoError) return errno;

        KeyValuePair<string, uint>[] orderedEntries = directoryEntries
                                                     .Where(static entry =>
                                                                !string.IsNullOrWhiteSpace(entry.Key) &&
                                                                entry.Key is not "." and not "..")
                                                     .OrderBy(static entry => entry.Key, StringComparer.Ordinal)
                                                     .ToArray();

        long maximum = orderedEntries.Length > 0 ? orderedEntries.Length : 1;

        for(int i = 0; i < orderedEntries.Length; i++)
        {
            string entryPath = path == "/" ? "/" + orderedEntries[i].Key : path + "/" + orderedEntries[i].Key;

            updateProgress?.Invoke(entryPath, i + 1, maximum);

            errno = GetFilesetInode(orderedEntries[i].Value, out Inode inode);

            if(errno != ErrorNumber.NoError) return errno;

            if((inode.di_mode & 0xF000) == 0x4000)
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

            errno = AddOverlappingFile(entryPath, orderedEntries[i].Value, inode, sectorExtents, files);

            if(errno != ErrorNumber.NoError) return errno;
        }

        return ErrorNumber.NoError;
    }

    ErrorNumber GetDirectoryEntries(uint directoryInodeNumber, in Inode directoryInode,
                                    out Dictionary<string, uint> directoryEntries)
    {
        if(directoryInodeNumber == ROOT_I)
        {
            directoryEntries = new Dictionary<string, uint>(_rootDirectoryCache, StringComparer.Ordinal);

            return ErrorNumber.NoError;
        }

        return GetDirectoryEntries(directoryInodeNumber, directoryInode.di_u, out directoryEntries);
    }

    ErrorNumber AddOverlappingFile(string path, uint inodeNumber, in Inode inode,
                                   IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                                   List<FileSectorInfo> files)
    {
        ulong logicalSize = (ulong)inode.di_size;

        if(logicalSize == 0) return ErrorNumber.NoError;

        ErrorNumber errno = FindOverlappingExtents(inode, logicalSize, sectorExtents, out List<(ulong Start, ulong End)> overlaps);

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

    ErrorNumber FindOverlappingExtents(in Inode inode, ulong logicalSize,
                                       IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                                       out List<(ulong Start, ulong End)> overlaps)
    {
        overlaps = [];

        ulong blockSize  = (ulong)_superblock.s_bsize;
        ulong blockCount = (logicalSize + blockSize - 1) / blockSize;

        for(long logicalBlock = 0; logicalBlock < (long)blockCount; logicalBlock++)
        {
            ErrorNumber errno = XTreeLookup(inode.di_u, false, logicalBlock, out long physicalBlock);

            if(errno == ErrorNumber.InvalidArgument || physicalBlock == 0) continue;
            if(errno != ErrorNumber.NoError) return errno;

            ulong logicalOffset = (ulong)logicalBlock * blockSize;
            ulong bytesInBlock  = Math.Min(blockSize, logicalSize - logicalOffset);
            AddByteRangeOverlaps((ulong)physicalBlock * blockSize, bytesInBlock, sectorExtents, overlaps);
        }

        overlaps = NormalizeAnalyzeExtents(overlaps);

        return ErrorNumber.NoError;
    }

    void AddByteRangeOverlaps(ulong absoluteByteOffset, ulong lengthBytes,
                              IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                              List<(ulong Start, ulong End)> overlaps)
    {
        if(lengthBytes == 0) return;

        ulong sectorSize     = _imagePlugin.Info.SectorSize;
        ulong startSector    = _partition.Start + absoluteByteOffset / sectorSize;
        ulong offsetInSector = absoluteByteOffset % sectorSize;
        ulong sectorCount    = (lengthBytes + offsetInSector + sectorSize - 1) / sectorSize;

        if(sectorCount == 0) sectorCount = 1;

        AddExtentOverlaps(startSector, startSector + sectorCount - 1, sectorExtents, overlaps);
    }

    static void AddExtentOverlaps(ulong startSector, ulong endSector,
                                  IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                                  List<(ulong Start, ulong End)> overlaps)
    {
        foreach((ulong Start, ulong End) requestedExtent in sectorExtents)
        {
            if(requestedExtent.End < startSector || requestedExtent.Start > endSector) continue;

            overlaps.Add((Math.Max(startSector, requestedExtent.Start), Math.Min(endSector, requestedExtent.End)));
        }
    }

    static List<(ulong Start, ulong End)> NormalizeAnalyzeExtents(IEnumerable<(ulong Start, ulong End)> extents)
    {
        List<(ulong Start, ulong End)> orderedExtents = extents.Where(static extent => extent.End >= extent.Start)
                                                               .OrderBy(static extent => extent.Start)
                                                               .ThenBy(static extent => extent.End)
                                                               .ToList();
        List<(ulong Start, ulong End)> normalizedExtents = [];

        if(orderedExtents.Count == 0) return normalizedExtents;

        (ulong Start, ulong End) currentExtent = orderedExtents[0];

        for(int i = 1; i < orderedExtents.Count; i++)
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