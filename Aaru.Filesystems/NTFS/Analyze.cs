// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Analyze.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft NT File System plugin.
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
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class NTFS
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
                                                       (uint)SystemFileNumber.Root,
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

    ErrorNumber TraverseDirectoryForAffectedSectors(string path, uint directoryMftRecordNumber,
                                                    List<(ulong Start, ulong End)> sectorExtents,
                                                    List<FileSectorInfo> files, UpdateProgressHandler updateProgress,
                                                    PulseProgressHandler pulseProgress)
    {
        pulseProgress?.Invoke(path);

        Dictionary<string, ulong> directoryEntries;

        if(directoryMftRecordNumber == (uint)SystemFileNumber.Root)
            directoryEntries = new Dictionary<string, ulong>(_rootDirectoryCache, StringComparer.OrdinalIgnoreCase);
        else
        {
            ErrorNumber errno = GetDirectoryEntries(directoryMftRecordNumber, out directoryEntries);

            if(errno != ErrorNumber.NoError) return errno;
        }

        KeyValuePair<string, ulong>[] orderedEntries = directoryEntries
                                                      .Where(static entry => !string.IsNullOrWhiteSpace(entry.Key))
                                                      .OrderBy(static entry => entry.Key,
                                                               StringComparer.OrdinalIgnoreCase)
                                                      .ToArray();

        long maximum = orderedEntries.Length > 0 ? orderedEntries.Length : 1;

        for(var i = 0; i < orderedEntries.Length; i++)
        {
            string entryPath = path == "/" ? "/" + orderedEntries[i].Key : path + "/" + orderedEntries[i].Key;

            updateProgress?.Invoke(entryPath, i + 1, maximum);

            var         childMftRecordNumber = (uint)(orderedEntries[i].Value & 0x0000FFFFFFFFFFFF);
            ErrorNumber errno                = ReadMftRecord(childMftRecordNumber, out byte[] recordData);

            if(errno != ErrorNumber.NoError) return errno;

            MftRecord header = ParseMftRecordHeader(recordData);

            if(header.magic != NtfsRecordMagic.File || !header.flags.HasFlag(MftRecordFlags.InUse)) continue;

            if(header.flags.HasFlag(MftRecordFlags.IsDirectory))
            {
                errno = TraverseDirectoryForAffectedSectors(entryPath,
                                                            childMftRecordNumber,
                                                            sectorExtents,
                                                            files,
                                                            updateProgress,
                                                            pulseProgress);

                if(errno != ErrorNumber.NoError) return errno;

                continue;
            }

            errno = AddOverlappingFile(entryPath, childMftRecordNumber, recordData, header, sectorExtents, files);

            if(errno != ErrorNumber.NoError) return errno;
        }

        return ErrorNumber.NoError;
    }

    ErrorNumber AddOverlappingFile(string path, uint mftRecordNumber, byte[] recordData, in MftRecord header,
                                   IReadOnlyList<(ulong Start, ulong End)> sectorExtents, List<FileSectorInfo> files)
    {
        ErrorNumber errno = FindAllAttributes(recordData, header, mftRecordNumber, out List<FoundAttribute> attrs);

        if(errno != ErrorNumber.NoError) return errno;

        HashSet<string> processedStreams = new(StringComparer.OrdinalIgnoreCase);

        foreach(FoundAttribute attr in attrs)
        {
            var attrType = (AttributeType)BitConverter.ToUInt32(attr.RecordData, attr.Offset);

            if(attrType != AttributeType.Data) continue;

            string streamName    = GetAttributeName(attr.RecordData, attr.Offset);
            string processedName = streamName ?? string.Empty;

            if(!processedStreams.Add(processedName)) continue;

            errno = AddOverlappingStream(path, mftRecordNumber, streamName, attr, sectorExtents, files);

            if(errno != ErrorNumber.NoError) return errno;
        }

        return ErrorNumber.NoError;
    }

    ErrorNumber AddOverlappingStream(string path, uint mftRecordNumber, string streamName, in FoundAttribute attr,
                                     IReadOnlyList<(ulong Start, ulong End)> sectorExtents, List<FileSectorInfo> files)
    {
        byte                           nonResident = attr.RecordData[attr.Offset + 8];
        List<(ulong Start, ulong End)> overlaps;

        if(nonResident == 0)
        {
            var valueLength = BitConverter.ToUInt32(attr.RecordData, attr.Offset + 0x10);

            if(valueLength == 0) return ErrorNumber.NoError;

            var  valueOffset  = BitConverter.ToUInt16(attr.RecordData, attr.Offset + 0x14);
            int  valueStart   = attr.Offset + valueOffset;
            uint recordNumber = GetResidentRecordNumber(attr.RecordData, mftRecordNumber);

            overlaps = FindResidentOverlaps(recordNumber, valueStart, valueLength, sectorExtents);
        }
        else
        {
            ErrorNumber errno = AssembleNonResidentRuns(mftRecordNumber,
                                                        AttributeType.Data,
                                                        streamName,
                                                        out List<(long offset, long length)> dataRuns,
                                                        out long dataSize,
                                                        out _,
                                                        out _,
                                                        out _);

            if(errno    != ErrorNumber.NoError) return errno;
            if(dataSize <= 0) return ErrorNumber.NoError;

            overlaps = FindNonResidentOverlaps(dataRuns, dataSize, sectorExtents);
        }

        if(overlaps.Count == 0) return ErrorNumber.NoError;

        files.Add(new FileSectorInfo
        {
            Path            = path,
            Stream          = string.IsNullOrWhiteSpace(streamName) ? null : streamName,
            Inode           = mftRecordNumber,
            AffectedSectors = overlaps
        });

        return ErrorNumber.NoError;
    }

    uint GetResidentRecordNumber(byte[] recordData, uint fallbackRecordNumber)
    {
        MftRecord header = ParseMftRecordHeader(recordData);

        return header.mft_record_number != 0 ? header.mft_record_number : fallbackRecordNumber;
    }

    static string GetAttributeName(byte[] recordData, int offset)
    {
        byte nameLength = recordData[offset + 9];

        if(nameLength == 0) return null;

        var nameOffset = BitConverter.ToUInt16(recordData, offset + 0x0A);
        int nameStart  = offset + nameOffset;
        int nameBytes  = nameLength * 2;

        if(nameStart < 0 || nameStart + nameBytes > recordData.Length) return null;

        return Encoding.Unicode.GetString(recordData, nameStart, nameBytes);
    }

    List<(ulong Start, ulong End)> FindResidentOverlaps(uint recordNumber, int valueStart, uint valueLength,
                                                        IReadOnlyList<(ulong Start, ulong End)> sectorExtents)
    {
        List<(ulong Start, ulong End)> overlaps = [];

        AddMftByteRangeOverlaps((long)recordNumber * _mftRecordSize + valueStart, valueLength, sectorExtents, overlaps);

        return NormalizeAnalyzeExtents(overlaps);
    }

    void AddMftByteRangeOverlaps(long                                    relativeByteOffset, ulong byteLength,
                                 IReadOnlyList<(ulong Start, ulong End)> sectorExtents,
                                 List<(ulong Start, ulong End)>          overlaps)
    {
        if(byteLength == 0) return;

        if(_mftDataRuns is { Count: > 0 })
        {
            var  bytesRemaining = (long)byteLength;
            long currentOffset  = relativeByteOffset;
            long runByteStart   = 0;

            foreach((long clusterOffset, long clusterLength) in _mftDataRuns)
            {
                long runBytes   = clusterLength * _bytesPerCluster;
                long runByteEnd = runByteStart + runBytes;

                if(runByteEnd <= currentOffset)
                {
                    runByteStart = runByteEnd;

                    continue;
                }

                if(bytesRemaining <= 0) break;

                long offsetInRun    = currentOffset - runByteStart;
                long bytesInThisRun = Math.Min(runBytes - offsetInRun, bytesRemaining);

                if(bytesInThisRun <= 0)
                {
                    runByteStart = runByteEnd;

                    continue;
                }

                if(clusterOffset != 0)
                {
                    long  physicalByteOffset = clusterOffset * _bytesPerCluster + offsetInRun;
                    ulong startSector        = _partition.Start + (ulong)(physicalByteOffset / _bytesPerSector);
                    var   offsetInSector     = (ulong)(physicalByteOffset % _bytesPerSector);

                    ulong sectorCount = ((ulong)bytesInThisRun + offsetInSector + _bytesPerSector - 1) /
                                        _bytesPerSector;

                    if(sectorCount == 0) sectorCount = 1;

                    AddExtentOverlaps(startSector, startSector + sectorCount - 1, sectorExtents, overlaps);
                }

                currentOffset  += bytesInThisRun;
                bytesRemaining -= bytesInThisRun;
                runByteStart   =  runByteEnd;
            }

            return;
        }

        long  mftStartByte       = _bpb.mft_lsn * _bytesPerCluster;
        long  byteOffset         = mftStartByte     + relativeByteOffset;
        ulong start              = _partition.Start + (ulong)(byteOffset / _bytesPerSector);
        var   byteOffsetInSector = (ulong)(byteOffset % _bytesPerSector);
        ulong sectors            = (byteLength + byteOffsetInSector + _bytesPerSector - 1) / _bytesPerSector;

        if(sectors == 0) sectors = 1;

        AddExtentOverlaps(start, start + sectors - 1, sectorExtents, overlaps);
    }

    List<(ulong Start, ulong End)> FindNonResidentOverlaps(List<(long offset, long length)> dataRuns, long dataSize,
                                                           IReadOnlyList<(ulong Start, ulong End)> sectorExtents)
    {
        List<(ulong Start, ulong End)> overlaps = [];

        if(dataSize <= 0 || dataRuns is null || dataRuns.Count == 0) return overlaps;

        long remainingBytes = dataSize;

        foreach((long clusterOffset, long clusterLength) in dataRuns)
        {
            if(remainingBytes <= 0) break;

            if(clusterLength <= 0) continue;

            long runBytes   = clusterLength * _bytesPerCluster;
            long bytesInRun = Math.Min(runBytes, remainingBytes);

            if(bytesInRun <= 0) break;

            if(clusterOffset != 0)
            {
                ulong startSector = _partition.Start + (ulong)clusterOffset * _sectorsPerCluster;
                ulong sectorCount = ((ulong)bytesInRun + _bytesPerSector - 1) / _bytesPerSector;

                if(sectorCount == 0) sectorCount = 1;

                AddExtentOverlaps(startSector, startSector + sectorCount - 1, sectorExtents, overlaps);
            }

            remainingBytes -= bytesInRun;
        }

        return NormalizeAnalyzeExtents(overlaps);
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