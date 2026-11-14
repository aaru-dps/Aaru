// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Analyze.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CP/M filesystem plugin.
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
using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class CPM
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

        if(_device is null || _workingDefinition is null || _dpb is null || _sectorMask is null)
            return ErrorNumber.InvalidArgument;

        List<(ulong Start, ulong End)> normalizedExtents = NormalizeAnalyzeExtents(sectorExtents);

        if(normalizedExtents.Count == 0) return ErrorNumber.NoError;

        try
        {
            initProgress?.Invoke();
            pulseProgress?.Invoke("/");

            ErrorNumber errno = AnalyzeDirectory(normalizedExtents, files, updateProgress);

            return errno;
        }
        finally
        {
            endProgress?.Invoke();
        }
    }

#endregion

    ErrorNumber AnalyzeDirectory(IReadOnlyList<(ulong Start, ulong End)> sectorExtents, List<FileSectorInfo> files,
                                 UpdateProgressHandler                   updateProgress)
    {
        int dirOff;
        int dirSectors = (_dpb.drm + 1) * 32 / _workingDefinition.bytesPerSector;

        if(_workingDefinition.sofs > 0)
            dirOff = _workingDefinition.sofs;
        else
            dirOff = _workingDefinition.ofs * _workingDefinition.sectorsPerTrack;

        using MemoryStream dirMs = new();

        for(var d = 0; d < dirSectors; d++)
        {
            ulong physicalSector = LogicalSectorToPhysicalSector((ulong)(d + dirOff));

            if(physicalSector >= _device.Info.Sectors) return ErrorNumber.OutOfRange;

            ErrorNumber errno = _device.ReadSector(physicalSector, false, out byte[] sector, out _);

            if(errno != ErrorNumber.NoError) return errno;

            if(_workingDefinition.complement)
                for(var b = 0; b < sector.Length; b++)
                    sector[b] = (byte)(~sector[b] & 0xFF);

            dirMs.Write(sector, 0, sector.Length);
        }

        Dictionary<string, Dictionary<int, List<ushort>>> fileExtents = ParseFileExtents(dirMs.ToArray());

        string[] orderedFiles = fileExtents.Keys.OrderBy(static entry => entry, StringComparer.CurrentCultureIgnoreCase)
                                           .ToArray();

        long maximum = orderedFiles.Length > 0 ? orderedFiles.Length : 1;

        for(var i = 0; i < orderedFiles.Length; i++)
        {
            string path = "/" + orderedFiles[i];

            updateProgress?.Invoke(path, i + 1, maximum);

            AddOverlappingFile(path, fileExtents[orderedFiles[i]], sectorExtents, files);
        }

        return ErrorNumber.NoError;
    }

    Dictionary<string, Dictionary<int, List<ushort>>> ParseFileExtents(byte[] directory)
    {
        Dictionary<string, Dictionary<int, List<ushort>>> fileExtents = [];
        int blockSize = 128 << _dpb.bsh;
        ulong sectorCount = _partition.End >= _partition.Start ? _partition.End - _partition.Start + 1 : 0;
        ulong allocationBlockCount = blockSize > 0 ? sectorCount * _device.Info.SectorSize / (ulong)blockSize : 0;

        for(var dOff = 0; dOff < directory.Length; dOff += 32)
        {
            switch(directory[dOff] & 0x7F)
            {
                case < 0x10 when allocationBlockCount > 256:
                {
                    DirectoryEntry16 entry =
                        Marshal.ByteArrayToStructureLittleEndian<DirectoryEntry16>(directory, dOff, 32);

                    AddDirectoryEntryExtents(in entry, fileExtents);

                    break;
                }
                case < 0x10:
                {
                    DirectoryEntry entry =
                        Marshal.ByteArrayToStructureLittleEndian<DirectoryEntry>(directory, dOff, 32);

                    AddDirectoryEntryExtents(in entry, fileExtents);

                    break;
                }
            }
        }

        return fileExtents;
    }

    void AddDirectoryEntryExtents(in DirectoryEntry16                               entry,
                                  Dictionary<string, Dictionary<int, List<ushort>>> fileExtents)
    {
        var filenameBytes  = (byte[])entry.filename.Clone();
        var extensionBytes = (byte[])entry.extension.Clone();
        var validEntry     = true;

        for(var i = 0; i < 8; i++)
        {
            filenameBytes[i] &= 0x7F;
            validEntry       &= filenameBytes[i] >= 0x20;
        }

        for(var i = 0; i < 3; i++)
        {
            extensionBytes[i] &= 0x7F;
            validEntry        &= extensionBytes[i] >= 0x20;
        }

        if(!validEntry) return;

        string filename  = Encoding.ASCII.GetString(filenameBytes).Trim();
        string extension = Encoding.ASCII.GetString(extensionBytes).Trim();
        int    user      = entry.statusUser & 0x0F;

        // A file without name nor extension cannot be addressed, hide it
        if(filename.Length == 0 && extension.Length == 0) return;

        if(user > 0) filename                         = $"{user:X1}:{filename}";
        if(!string.IsNullOrEmpty(extension)) filename = filename + "." + extension;

        int entryNo = (32 * entry.extentCounter + entry.extentCounterHigh) / (_dpb.exm + 1);

        if(!fileExtents.TryGetValue(filename, out Dictionary<int, List<ushort>> extentBlocks)) extentBlocks = [];
        if(!extentBlocks.TryGetValue(entryNo, out List<ushort> blocks)) blocks                              = [];

        foreach(ushort block in entry.allocations.Where(static blk => blk != 0 && blk != ushort.MaxValue))
            if(!blocks.Contains(block))
                blocks.Add(block);

        extentBlocks[entryNo] = blocks;
        fileExtents[filename] = extentBlocks;
    }

    void AddDirectoryEntryExtents(in DirectoryEntry                                 entry,
                                  Dictionary<string, Dictionary<int, List<ushort>>> fileExtents)
    {
        var filenameBytes  = (byte[])entry.filename.Clone();
        var extensionBytes = (byte[])entry.extension.Clone();
        var validEntry     = true;

        for(var i = 0; i < 8; i++)
        {
            filenameBytes[i] &= 0x7F;
            validEntry       &= filenameBytes[i] >= 0x20;
        }

        for(var i = 0; i < 3; i++)
        {
            extensionBytes[i] &= 0x7F;
            validEntry        &= extensionBytes[i] >= 0x20;
        }

        if(!validEntry) return;

        string filename  = Encoding.ASCII.GetString(filenameBytes).Trim();
        string extension = Encoding.ASCII.GetString(extensionBytes).Trim();
        int    user      = entry.statusUser & 0x0F;

        // A file without name nor extension cannot be addressed, hide it
        if(filename.Length == 0 && extension.Length == 0) return;

        if(user > 0) filename                         = $"{user:X1}:{filename}";
        if(!string.IsNullOrEmpty(extension)) filename = filename + "." + extension;

        int entryNo = (32 * entry.extentCounterHigh + entry.extentCounter) / (_dpb.exm + 1);

        if(!fileExtents.TryGetValue(filename, out Dictionary<int, List<ushort>> extentBlocks)) extentBlocks = [];
        if(!extentBlocks.TryGetValue(entryNo, out List<ushort> blocks)) blocks                              = [];

        foreach(byte block in entry.allocations.Where(static blk => blk != 0))
            if(!blocks.Contains(block))
                blocks.Add(block);

        extentBlocks[entryNo] = blocks;
        fileExtents[filename] = extentBlocks;
    }

    void AddOverlappingFile(string path, IReadOnlyDictionary<int, List<ushort>> extentBlocks,
                            IReadOnlyList<(ulong Start, ulong End)> sectorExtents, List<FileSectorInfo> files)
    {
        if(extentBlocks.Count == 0) return;

        List<(ulong Start, ulong End)> overlaps = FindOverlappingExtents(extentBlocks, sectorExtents);

        if(overlaps.Count == 0) return;

        ulong inode = extentBlocks.OrderBy(static entry => entry.Key)
                                  .SelectMany(static entry => entry.Value)
                                  .Select(static block => (ulong)block)
                                  .FirstOrDefault();

        files.Add(new FileSectorInfo
        {
            Path            = path,
            Inode           = inode,
            AffectedSectors = overlaps
        });
    }

    List<(ulong Start, ulong End)> FindOverlappingExtents(IReadOnlyDictionary<int, List<ushort>>  extentBlocks,
                                                          IReadOnlyList<(ulong Start, ulong End)> sectorExtents)
    {
        List<(ulong Start, ulong End)> overlaps = [];

        foreach(KeyValuePair<int, List<ushort>> extent in extentBlocks.OrderBy(static entry => entry.Key))
        {
            foreach(ushort allocationBlock in extent.Value)
                AddAllocationBlockOverlaps(allocationBlock, sectorExtents, overlaps);
        }

        return NormalizeAnalyzeExtents(overlaps);
    }

    void AddAllocationBlockOverlaps(ushort allocationBlock, IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                                    List<(ulong Start, ulong End)> overlaps)
    {
        if(allocationBlock == 0) return;

        int  blockSize  = 128 << _dpb.bsh;
        uint sectorSize = _device.Info.SectorSize;

        if(blockSize <= 0 || sectorSize == 0) return;

        if(sectorSize > blockSize)
        {
            uint blocksPerSector = sectorSize / (uint)blockSize;

            if(blocksPerSector == 0) return;

            AddLogicalSectorOverlap(allocationBlock / blocksPerSector, sectorExtents, overlaps);

            return;
        }

        if(sectorSize < blockSize)
        {
            uint sectorsPerBlock = (uint)blockSize / sectorSize;

            if(sectorsPerBlock == 0) return;

            ulong firstLogicalSector = (ulong)allocationBlock * sectorsPerBlock;

            for(uint i = 0; i < sectorsPerBlock; i++)
                AddLogicalSectorOverlap(firstLogicalSector + i, sectorExtents, overlaps);

            return;
        }

        AddLogicalSectorOverlap(allocationBlock, sectorExtents, overlaps);
    }

    void AddLogicalSectorOverlap(ulong logicalSector, IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                                 List<(ulong Start, ulong End)> overlaps)
    {
        ulong physicalSector = LogicalSectorToPhysicalSector(logicalSector);

        AddExtentOverlaps(physicalSector, physicalSector, sectorExtents, overlaps);
    }

    ulong LogicalSectorToPhysicalSector(ulong logicalSector)
    {
        if(_sectorMask is null || _sectorMask.Length == 0) return _partition.Start + logicalSector;

        return _partition.Start                                                      +
               logicalSector / (ulong)_sectorMask.Length * (ulong)_sectorMask.Length +
               (ulong)_sectorMask[(int)(logicalSector % (ulong)_sectorMask.Length)];
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