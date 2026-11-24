// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Analyze.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SmartFileSystem plugin.
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
using Aaru.Helpers;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class SFS
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

    ErrorNumber TraverseDirectoryForAffectedSectors(string path,
                                                    IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                                                    List<FileSectorInfo> files,
                                                    UpdateProgressHandler updateProgress,
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

            string[] orderedEntries = entries.Where(static entry => !string.IsNullOrWhiteSpace(entry))
                                             .OrderBy(static entry => entry, StringComparer.Ordinal)
                                             .ToArray();
            long maximum = orderedEntries.Length > 0 ? orderedEntries.Length : 1;

            for(int i = 0; i < orderedEntries.Length; i++)
            {
                string entryPath = path == "/" ? "/" + orderedEntries[i] : path + "/" + orderedEntries[i];

                updateProgress?.Invoke(entryPath, i + 1, maximum);

                errno = GetObjectNodeForPath(entryPath, out uint objectNode);

                if(errno != ErrorNumber.NoError) return errno;

                errno = FindObjectNode(objectNode, out uint objectBlock);

                if(errno != ErrorNumber.NoError) return errno;

                errno = ReadBlock(objectBlock, out byte[] objectData);

                if(errno != ErrorNumber.NoError) return errno;

                errno = FindObjectInContainer(objectData, objectNode, out int objectOffset);

                if(errno != ErrorNumber.NoError) return errno;

                int  bitsOffset  = _isSfs2 ? 26 : 24;
                byte objectBits  = objectData[objectOffset + bitsOffset];
                bool isDirectory = (objectBits & (byte)ObjectBits.Directory) != 0;

                if(isDirectory)
                {
                    errno = TraverseDirectoryForAffectedSectors(entryPath,
                                                                sectorExtents,
                                                                files,
                                                                updateProgress,
                                                                pulseProgress);

                    if(errno != ErrorNumber.NoError) return errno;

                    continue;
                }

                // Links own no data extents: the data field of a soft link points to its SLNK block and the one of
                // a hard link holds the object node of its target, whose data is reported under its own path
                if((objectBits & (byte)ObjectBits.Link) != 0) continue;

                // SFS\0 and SFS\2 both store the first extent at offset 12
                uint firstExtent = BigEndianBitConverter.ToUInt32(objectData, objectOffset + 12);

                errno = Stat(entryPath, out FileEntryInfo stat);

                if(errno != ErrorNumber.NoError) return errno;

                errno = AddOverlappingFile(entryPath,
                                           objectNode,
                                           (ulong)Math.Max(0, stat.Length),
                                           firstExtent,
                                           sectorExtents,
                                           files);

                if(errno != ErrorNumber.NoError) return errno;
            }

            return ErrorNumber.NoError;
        }
        finally
        {
            CloseDir(node);
        }
    }

    ErrorNumber AddOverlappingFile(string path, uint objectNode, ulong logicalSize, uint firstExtent,
                                   IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                                   List<FileSectorInfo> files)
    {
        if(logicalSize == 0 || firstExtent == 0) return ErrorNumber.NoError;

        ErrorNumber errno = FindOverlappingExtents(firstExtent,
                                                   logicalSize,
                                                   sectorExtents,
                                                   out List<(ulong Start, ulong End)> overlaps);

        if(errno != ErrorNumber.NoError) return errno;
        if(overlaps.Count == 0) return ErrorNumber.NoError;

        files.Add(new FileSectorInfo
        {
            Path            = path,
            Inode           = objectNode,
            AffectedSectors = overlaps
        });

        return ErrorNumber.NoError;
    }

    ErrorNumber FindOverlappingExtents(uint firstExtent, ulong logicalSize,
                                       IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                                       out List<(ulong Start, ulong End)> overlaps)
    {
        overlaps = [];

        ulong remainingBytes = logicalSize;
        uint  currentExtent  = firstExtent;

        while(currentExtent != 0 && remainingBytes > 0)
        {
            ErrorNumber errno = FindExtentBNode(currentExtent,
                                                out uint extentKey,
                                                out uint extentNext,
                                                out uint extentBlocks);

            if(errno != ErrorNumber.NoError) return errno;

            if(extentBlocks > 0)
            {
                ulong extentBytes = Math.Min((ulong)extentBlocks * _blockSize, remainingBytes);
                AddByteRangeOverlaps((ulong)extentKey * _blockSize, extentBytes, sectorExtents, overlaps);
                remainingBytes -= extentBytes;
            }

            currentExtent = extentNext;
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