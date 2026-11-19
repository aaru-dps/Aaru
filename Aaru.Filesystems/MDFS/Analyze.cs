// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Analyze.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : MiniDisc File System plugin.
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
public sealed partial class MDFS
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

        List<(ulong Start, ulong End)> requested = NormalizeAnalyzeExtents(sectorExtents);

        if(requested.Count == 0) return ErrorNumber.NoError;

        try
        {
            initProgress?.Invoke();

            // The volume management area holds the volume descriptor, the bitmap, the tables and all directories
            ulong vmaBlocks = Math.Max(1, (_vd.VmaLen + BLOCK_SIZE - 1) / BLOCK_SIZE);

            AddOverlaps("/",
                        null,
                        _root.Id,
                        [(_partition.Start + (ulong)_vd.VmaLoc * _sectorsPerBlock, vmaBlocks * _sectorsPerBlock)],
                        requested,
                        files);

            long current = 0;

            foreach(KeyValuePair<string, Entry> item in _entries.OrderBy(static e => e.Key, StringComparer.Ordinal))
            {
                current++;
                updateProgress?.Invoke("/" + item.Key, current, _entries.Count);

                if(item.Value.IsDirectory) continue;

                ErrorNumber errno = GetDataExtents(item.Value, out Extent[] data);

                if(errno != ErrorNumber.NoError) return errno;

                AddOverlaps("/" + item.Key, null, item.Value.Id, ToSectorRuns(data), requested, files);

                if(item.Value.ResourceLength == 0) continue;

                errno = GetResourceExtents(item.Value, out Extent[] resource);

                if(errno != ErrorNumber.NoError) return errno;

                AddOverlaps("/" + item.Key,
                            Xattrs.XATTR_APPLE_RESOURCE_FORK,
                            item.Value.Id,
                            ToSectorRuns(resource),
                            requested,
                            files);
            }
        }
        finally
        {
            endProgress?.Invoke();
        }

        return ErrorNumber.NoError;
    }

#endregion

    /// <summary>Converts allocation unit runs to device sector runs</summary>
    IEnumerable<(ulong Start, ulong Count)> ToSectorRuns(IEnumerable<Extent> extents) =>
        extents.Select(extent => (_partition.Start + (ulong)extent.Start * _allocSize * _sectorsPerBlock,
                                  (ulong)extent.Count * _allocSize * _sectorsPerBlock));

    /// <summary>Adds the parts of some sector runs that overlap the requested sectors</summary>
    static void AddOverlaps(string path, string stream, ulong inode, IEnumerable<(ulong Start, ulong Count)> runs,
                            IReadOnlyList<(ulong Start, ulong End)> requested, List<FileSectorInfo> files)
    {
        List<(ulong Start, ulong End)> overlaps = [];

        foreach((ulong Start, ulong Count) run in runs)
        {
            if(run.Count == 0) continue;

            ulong end = run.Start + run.Count - 1;

            foreach((ulong Start, ulong End) wanted in requested)
            {
                if(wanted.End < run.Start || wanted.Start > end) continue;

                overlaps.Add((Math.Max(run.Start, wanted.Start), Math.Min(end, wanted.End)));
            }
        }

        if(overlaps.Count == 0) return;

        files.Add(new FileSectorInfo
        {
            Path            = path,
            Stream          = stream,
            Inode           = inode,
            AffectedSectors = NormalizeAnalyzeExtents(overlaps)
        });
    }

    static List<(ulong Start, ulong End)> NormalizeAnalyzeExtents(IEnumerable<(ulong Start, ulong End)> extents)
    {
        List<(ulong Start, ulong End)> ordered = extents.Where(static extent => extent.End >= extent.Start)
                                                        .OrderBy(static extent => extent.Start)
                                                        .ThenBy(static extent => extent.End)
                                                        .ToList();

        List<(ulong Start, ulong End)> normalized = [];

        if(ordered.Count == 0) return normalized;

        (ulong Start, ulong End) current = ordered[0];

        for(int i = 1; i < ordered.Count; i++)
        {
            if(ordered[i].Start <= current.End + 1)
            {
                current.End = Math.Max(current.End, ordered[i].End);

                continue;
            }

            normalized.Add(current);
            current = ordered[i];
        }

        normalized.Add(current);

        return normalized;
    }
}