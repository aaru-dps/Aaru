// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads KryoFlux STREAM images.
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
using System.Globalization;
using System.IO;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Filters;
using Aaru.Helpers;
using Aaru.Logging;
using Aaru.CommonTypes;

namespace Aaru.Images;

public sealed partial class KryoFlux
{
#region IMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < Marshal.SizeOf<OobBlock>()) return ErrorNumber.InvalidArgument;

        var hdr = new byte[Marshal.SizeOf<OobBlock>()];
        stream.EnsureRead(hdr, 0, Marshal.SizeOf<OobBlock>());

        OobBlock header = Marshal.ByteArrayToStructureLittleEndian<OobBlock>(hdr);

        stream.Seek(-Marshal.SizeOf<OobBlock>(), SeekOrigin.End);

        hdr = new byte[Marshal.SizeOf<OobBlock>()];
        stream.EnsureRead(hdr, 0, Marshal.SizeOf<OobBlock>());

        OobBlock footer = Marshal.ByteArrayToStructureLittleEndian<OobBlock>(hdr);

        if(header.blockId   != BlockIds.Oob    ||
           header.blockType != OobTypes.KFInfo ||
           footer.blockId   != BlockIds.Oob    ||
           footer.blockType != OobTypes.EOF    ||
           footer.length    != 0x0D0D)
            return ErrorNumber.InvalidArgument;

        // TODO: This is supposing NoFilter, shouldn't
        tracks         = new SortedDictionary<byte, IFilter>();
        _trackCaptures = [];
        byte step    = 1;
        byte heads   = 2;
        var  topHead = false;

        string basename = Path.Combine(imageFilter.ParentFolder, imageFilter.Filename[..^8]);

        for(byte t = 0; t < 166; t += step)
        {
            int cylinder = t / heads;
            int head     = topHead ? 1 : t % heads;

            string trackfile = Directory.Exists(basename)
                                   ? Path.Combine(basename, $"{cylinder:D2}.{head:D1}.raw")
                                   : $"{basename}{cylinder:D2}.{head:D1}.raw";

            if(!File.Exists(trackfile))
            {
                switch(cylinder)
                {
                    case 0 when head == 0:
                        AaruLogging.Debug(MODULE_NAME,
                                          Localization.Cannot_find_cyl_0_hd_0_supposing_only_top_head_was_dumped);

                        topHead = true;
                        heads   = 1;

                        continue;
                    case 0:
                        AaruLogging.Debug(MODULE_NAME,
                                          Localization.Cannot_find_cyl_0_hd_1_supposing_only_bottom_head_was_dumped);

                        heads = 1;

                        continue;
                    case 1:
                        AaruLogging.Debug(MODULE_NAME, Localization.Cannot_find_cyl_1_supposing_double_stepping);

                        step = 2;

                        continue;
                }

                AaruLogging.Debug(MODULE_NAME, Localization.Arrived_end_of_disk_at_cylinder_0, cylinder);

                break;
            }

            var         trackFilter = new ZZZNoFilter();
            ErrorNumber errno       = trackFilter.Open(trackfile);

            if(errno != ErrorNumber.NoError) return errno;

            _imageInfo.CreationTime         = DateTime.MaxValue;
            _imageInfo.LastModificationTime = DateTime.MinValue;

            ErrorNumber processError = ProcessTrackFile(trackfile, (uint)head, (ushort)cylinder, trackFilter);

            if(processError != ErrorNumber.NoError) return processError;

            tracks.Add(t, trackFilter);
        }

        _imageInfo.Heads     = heads;
        _imageInfo.Cylinders = (uint)(tracks.Count / heads);

        // TODO: Find a way to determine the media type from the track data.
        _imageInfo.MediaType = MediaType.DOS_35_HD;

        return ErrorNumber.NoError;
    }

    ErrorNumber ProcessTrackFile(string trackfile, uint head, ushort track, IFilter trackFilter)
    {
        if(!File.Exists(trackfile)) return ErrorNumber.NoSuchFile;

        AaruLogging.Debug(MODULE_NAME, "Processing track file: {0} (head {1}, track {2})", trackfile, head, track);

        Stream trackStream = trackFilter.GetDataForkStream();
        trackStream.Seek(0, SeekOrigin.Begin);

        byte[] fileData = new byte[trackStream.Length];
        trackStream.EnsureRead(fileData, 0, (int)trackStream.Length);

        int fileSize = fileData.Length;

        uint[] cellValues          = new uint[fileSize];
        uint[] cellStreamPositions = new uint[fileSize];

        uint cellAccumulator = 0;
        int  streamOfs       = 0;
        uint streamPos       = 0;
        int  cellPos         = 0;

        // Sample Frequency
        double sck = SCK;

        List<(uint streamPosition, uint timer, uint sysTime)> indexEvents = [];

        int oobHeaderSize = Marshal.SizeOf<OobBlock>();

        while(streamOfs < fileSize)
        {
            byte curOp = fileData[streamOfs];
            int  curOpLen;

            if(curOp == (byte)BlockIds.Oob)
            {
                if(fileSize - streamOfs < oobHeaderSize) return ErrorNumber.InvalidArgument;

                byte[] oobBytes = new byte[oobHeaderSize];
                Array.Copy(fileData, streamOfs, oobBytes, 0, oobHeaderSize);
                OobBlock oobBlk = Marshal.ByteArrayToStructureLittleEndian<OobBlock>(oobBytes);

                if(oobBlk.blockType == OobTypes.EOF) break;

                curOpLen = oobHeaderSize + oobBlk.length;

                if(fileSize - streamOfs < curOpLen) return ErrorNumber.InvalidArgument;

                int oobDataStart = streamOfs + oobHeaderSize;

                switch(oobBlk.blockType)
                {
                    case OobTypes.KFInfo:
                    {
                        byte[] kfinfo = new byte[oobBlk.length];
                        Array.Copy(fileData, oobDataStart, kfinfo, 0, oobBlk.length);

                        string   kfinfoStr = StringHandlers.CToString(kfinfo);
                        string[] lines     = kfinfoStr.Split([','], StringSplitOptions.RemoveEmptyEntries);

                        DateTime blockDate = DateTime.Now;
                        DateTime blockTime = DateTime.Now;
                        bool     foundDate = false;

                        foreach(string[] kvp in lines.Select(static line => line.Split('='))
                                                     .Where(static kvp => kvp.Length == 2))
                        {
                            kvp[0] = kvp[0].Trim();
                            kvp[1] = kvp[1].Trim();
                            AaruLogging.Debug(MODULE_NAME, "\"{0}\" = \"{1}\"", kvp[0], kvp[1]);

                            switch(kvp[0])
                            {
                                case HOST_DATE:
                                    if(DateTime.TryParseExact(kvp[1],
                                                              "yyyy.MM.dd",
                                                              CultureInfo.InvariantCulture,
                                                              DateTimeStyles.AssumeLocal,
                                                              out blockDate))
                                        foundDate = true;

                                    break;
                                case HOST_TIME:
                                    DateTime.TryParseExact(kvp[1],
                                                           "HH:mm:ss",
                                                           CultureInfo.InvariantCulture,
                                                           DateTimeStyles.AssumeLocal,
                                                           out blockTime);

                                    break;
                                case KF_NAME:
                                    _imageInfo.Application = kvp[1];

                                    break;
                                case KF_VERSION:
                                    _imageInfo.ApplicationVersion = kvp[1];

                                    break;
                                case KF_SCK:
                                    if(double.TryParse(kvp[1],
                                                       NumberStyles.Float,
                                                       CultureInfo.InvariantCulture,
                                                       out double parsedSck))
                                        sck = parsedSck;

                                    break;
                                case KF_ICK:
                                    // Parse KF_ICK for validation purposes; parsed value is not currently used.
                                    // We use sample frequency space instead.
                                    _ = double.TryParse(kvp[1],
                                                        NumberStyles.Float,
                                                        CultureInfo.InvariantCulture,
                                                        out _);

                                    break;
                            }
                        }

                        if(foundDate)
                        {
                            DateTime blockTimestamp = new DateTime(blockDate.Year,
                                                                   blockDate.Month,
                                                                   blockDate.Day,
                                                                   blockTime.Hour,
                                                                   blockTime.Minute,
                                                                   blockTime.Second);

                            AaruLogging.Debug(MODULE_NAME, Localization.Found_timestamp_0, blockTimestamp);

                            if(blockTimestamp < Info.CreationTime) _imageInfo.CreationTime = blockTimestamp;

                            if(blockTimestamp > Info.LastModificationTime)
                                _imageInfo.LastModificationTime = blockTimestamp;
                        }

                        break;
                    }
                    case OobTypes.StreamInfo:
                    {
                        if(oobDataStart + Marshal.SizeOf<OobStreamRead>() <= fileSize)
                        {
                            byte[] streamReadBytes = new byte[Marshal.SizeOf<OobStreamRead>()];

                            Array.Copy(fileData, oobDataStart, streamReadBytes, 0, Marshal.SizeOf<OobStreamRead>());

                            OobStreamRead oobStreamRead =
                                Marshal.ByteArrayToStructureLittleEndian<OobStreamRead>(streamReadBytes);

                            AaruLogging.Debug(MODULE_NAME,
                                              "Stream Read at position {0}, elapsed time {1} ms",
                                              oobStreamRead.streamPosition,
                                              oobStreamRead.trTime);
                        }

                        break;
                    }
                    case OobTypes.Index:
                    {
                        if(oobDataStart + Marshal.SizeOf<OobIndex>() <= fileSize)
                        {
                            byte[] indexBytes = new byte[Marshal.SizeOf<OobIndex>()];
                            Array.Copy(fileData, oobDataStart, indexBytes, 0, Marshal.SizeOf<OobIndex>());

                            OobIndex oobIndex = Marshal.ByteArrayToStructureLittleEndian<OobIndex>(indexBytes);

                            AaruLogging.Debug(MODULE_NAME,
                                              "Index signal at stream position {0}, timer {1}, sysTime {2}",
                                              oobIndex.streamPosition,
                                              oobIndex.timer,
                                              oobIndex.sysTime);

                            indexEvents.Add((oobIndex.streamPosition, oobIndex.timer, oobIndex.sysTime));
                        }

                        break;
                    }
                    case OobTypes.StreamEnd:
                    {
                        if(oobDataStart + Marshal.SizeOf<OobStreamEnd>() <= fileSize)
                        {
                            byte[] streamEndBytes = new byte[Marshal.SizeOf<OobStreamEnd>()];

                            Array.Copy(fileData, oobDataStart, streamEndBytes, 0, Marshal.SizeOf<OobStreamEnd>());

                            OobStreamEnd oobStreamEnd =
                                Marshal.ByteArrayToStructureLittleEndian<OobStreamEnd>(streamEndBytes);

                            AaruLogging.Debug(MODULE_NAME,
                                              "Stream End at position {0}, result {1}",
                                              oobStreamEnd.streamPosition,
                                              oobStreamEnd.result);
                        }

                        break;
                    }
                }

                streamOfs += curOpLen;

                continue;
            }

            // Non-OOB: determine operation length and decode flux data
            bool newCell = false;

            switch(curOp)
            {
                case (byte)BlockIds.Nop1:
                    curOpLen = 1;

                    break;
                case (byte)BlockIds.Nop2:
                    curOpLen = 2;

                    break;
                case (byte)BlockIds.Nop3:
                    curOpLen = 3;

                    break;
                case (byte)BlockIds.Ovl16:
                    curOpLen        =  1;
                    cellAccumulator += 0x10000;

                    break;
                case (byte)BlockIds.Flux3:
                    if(streamOfs + 2 >= fileSize) return ErrorNumber.InvalidArgument;

                    curOpLen        =  3;
                    cellAccumulator += (uint)((fileData[streamOfs + 1] << 8) | fileData[streamOfs + 2]);
                    newCell         =  true;

                    break;
                default:
                    if(curOp >= 0x0E)
                    {
                        curOpLen        =  1;
                        cellAccumulator += curOp;
                        newCell         =  true;
                    }
                    else if((curOp & 0xF8) == 0)
                    {
                        if(streamOfs + 1 >= fileSize) return ErrorNumber.InvalidArgument;

                        curOpLen        =  2;
                        cellAccumulator += ((uint)curOp << 8) | fileData[streamOfs + 1];
                        newCell         =  true;
                    }
                    else
                        return ErrorNumber.InvalidArgument;

                    break;
            }

            if(fileSize - streamOfs < curOpLen) return ErrorNumber.InvalidArgument;

            if(newCell)
            {
                cellValues[cellPos]          = cellAccumulator;
                cellStreamPositions[cellPos] = streamPos;
                cellPos++;
                cellAccumulator = 0;
            }

            streamPos += (uint)curOpLen;
            streamOfs += curOpLen;
        }

        // Store final partial cell for index resolution boundary
        if(cellPos < cellValues.Length)
        {
            cellValues[cellPos]          = cellAccumulator;
            cellStreamPositions[cellPos] = streamPos;
        }

        int totalCells = cellPos;

        // Resolve index stream positions to cell positions
        List<uint> indexPositions = [];

        if(indexEvents.Count > 0)
        {
            int  nextIndex          = 0;
            uint nextIndexStreamPos = indexEvents[nextIndex].streamPosition;

            for(int i = 0; i < totalCells; i++)
            {
                if(nextIndex >= indexEvents.Count) break;

                int nextCellPos = i + 1;

                if(nextIndexStreamPos <= cellStreamPositions[nextCellPos])
                {
                    if(i == 0 && cellStreamPositions[0] >= nextIndexStreamPos) nextCellPos = 0;

                    indexPositions.Add((uint)nextCellPos);

                    AaruLogging.Debug(MODULE_NAME, "Index {0} resolved to cell position {1}", nextIndex, nextCellPos);

                    nextIndex++;

                    if(nextIndex < indexEvents.Count) nextIndexStreamPos = indexEvents[nextIndex].streamPosition;
                }
            }
        }

        // Build flux pulses array
        uint[] fluxPulses = new uint[totalCells];
        Array.Copy(cellValues, fluxPulses, totalCells);

        ulong resolution = CalculateResolution(sck);

        AaruLogging.Debug(MODULE_NAME,
                          "Decoded {0} flux pulses, {1} index signals, resolution {2} ps",
                          fluxPulses.Length,
                          indexPositions.Count,
                          resolution);

        TrackCapture capture = new TrackCapture
        {
            head           = head,
            track          = track,
            resolution     = resolution,
            fluxPulses     = fluxPulses,
            indexPositions = indexPositions.ToArray()
        };

        _trackCaptures.Add(capture);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadMediaTag(MediaTagType tag, out byte[] buffer)
    {
        buffer = null;

        return ErrorNumber.NotImplemented;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, bool negative, out byte[] buffer, out SectorStatus sectorStatus)
    {
        sectorStatus = SectorStatus.NotDumped;

        return ReadSectors(sectorAddress, negative, 1, out buffer, out _);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, bool negative, SectorTagType tag, out byte[] buffer) =>
        ReadSectorsTag(sectorAddress, negative, 1, tag, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                   out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        return ErrorNumber.NotImplemented;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong      sectorAddress, bool negative, uint length, SectorTagType tag,
                                      out byte[] buffer)
    {
        buffer = null;

        return ErrorNumber.NotImplemented;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong            sectorAddress, bool negative, out byte[] buffer,
                                      out SectorStatus sectorStatus)
    {
        sectorStatus = SectorStatus.NotDumped;

        return ReadSectorsLong(sectorAddress, negative, 1, out buffer, out _);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                       out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        return ErrorNumber.NotImplemented;
    }

#endregion

#region IFluxImage Members

    /// <inheritdoc />
    public ErrorNumber CapturesLength(uint head, ushort track, byte subTrack, out uint length)
    {
        length = 0;

        if(_trackCaptures == null) return ErrorNumber.NotOpened;

        // KryoFlux doesn't support subtracks - only subTrack 0 is valid
        if(subTrack != 0) return ErrorNumber.OutOfRange;

        // KryoFlux has one file per track/head, which results in exactly one capture
        bool hasCapture = _trackCaptures.Any(c => c.head == head && c.track == track);

        length = hasCapture ? 1u : 0u;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFluxIndexResolution(uint      head, ushort track, byte subTrack, uint captureIndex,
                                               out ulong resolution)
    {
        resolution = 0;

        // KryoFlux doesn't support subtracks - only subTrack 0 is valid
        if(subTrack != 0) return ErrorNumber.OutOfRange;

        // KryoFlux has one file per track/head, which results in exactly one capture (captureIndex 0)
        if(captureIndex != 0) return ErrorNumber.OutOfRange;

        TrackCapture capture = _trackCaptures.Find(c => c.head == head && c.track == track);

        if(capture == null) return ErrorNumber.OutOfRange;

        resolution = capture.resolution;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFluxDataResolution(uint      head, ushort track, byte subTrack, uint captureIndex,
                                              out ulong resolution)
    {
        resolution = 0;

        // KryoFlux doesn't support subtracks - only subTrack 0 is valid
        if(subTrack != 0) return ErrorNumber.OutOfRange;

        // KryoFlux has one file per track/head, which results in exactly one capture (captureIndex 0)
        if(captureIndex != 0) return ErrorNumber.OutOfRange;

        TrackCapture capture = _trackCaptures.Find(c => c.head == head && c.track == track);

        if(capture == null) return ErrorNumber.OutOfRange;

        resolution = capture.resolution;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFluxResolution(uint      head,            ushort    track, byte subTrack, uint captureIndex,
                                          out ulong indexResolution, out ulong dataResolution)
    {
        indexResolution = dataResolution = 0;

        // KryoFlux doesn't support subtracks - only subTrack 0 is valid
        if(subTrack != 0) return ErrorNumber.OutOfRange;

        // KryoFlux has one file per track/head, which results in exactly one capture (captureIndex 0)
        if(captureIndex != 0) return ErrorNumber.OutOfRange;

        TrackCapture capture = _trackCaptures.Find(c => c.head == head && c.track == track);

        if(capture == null) return ErrorNumber.OutOfRange;

        indexResolution = dataResolution = capture.resolution;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFluxCapture(uint       head,            ushort    track, byte subTrack, uint captureIndex,
                                       out ulong  indexResolution, out ulong dataResolution, out byte[] indexBuffer,
                                       out byte[] dataBuffer)
    {
        indexBuffer     = dataBuffer     = null;
        indexResolution = dataResolution = 0;

        // KryoFlux doesn't support subtracks - only subTrack 0 is valid
        if(subTrack != 0) return ErrorNumber.OutOfRange;

        // KryoFlux has one file per track/head, which results in exactly one capture (captureIndex 0)
        if(captureIndex != 0) return ErrorNumber.OutOfRange;

        ErrorNumber error =
            ReadFluxResolution(head, track, subTrack, captureIndex, out indexResolution, out dataResolution);

        if(error != ErrorNumber.NoError) return error;

        error = ReadFluxDataCapture(head, track, subTrack, captureIndex, out dataBuffer);

        if(error != ErrorNumber.NoError) return error;

        error = ReadFluxIndexCapture(head, track, subTrack, captureIndex, out indexBuffer);

        return error;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFluxIndexCapture(uint       head, ushort track, byte subTrack, uint captureIndex,
                                            out byte[] buffer)
    {
        buffer = null;

        // KryoFlux doesn't support subtracks - only subTrack 0 is valid
        if(subTrack != 0) return ErrorNumber.OutOfRange;

        // KryoFlux has one file per track/head, which results in exactly one capture (captureIndex 0)
        if(captureIndex != 0) return ErrorNumber.OutOfRange;

        TrackCapture capture = _trackCaptures.Find(c => c.head == head && c.track == track);

        if(capture == null) return ErrorNumber.OutOfRange;

        var  tmpBuffer        = new List<byte>();
        uint previousPosition = 0;

        foreach(uint indexPos in capture.indexPositions)
        {
            // Calculate ticks from start to this index position
            uint ticks = 0;

            for(uint i = previousPosition; i < indexPos && i < capture.fluxPulses.Length; i++)
                ticks += capture.fluxPulses[i];

            uint deltaTicks = ticks;
            tmpBuffer.AddRange(UInt32ToFluxRepresentation(deltaTicks));
            previousPosition = indexPos;
        }

        buffer = tmpBuffer.ToArray();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFluxDataCapture(uint head, ushort track, byte subTrack, uint captureIndex, out byte[] buffer)
    {
        buffer = null;

        // KryoFlux doesn't support subtracks - only subTrack 0 is valid
        if(subTrack != 0) return ErrorNumber.OutOfRange;

        // KryoFlux has one file per track/head, which results in exactly one capture (captureIndex 0)
        if(captureIndex != 0) return ErrorNumber.OutOfRange;

        TrackCapture capture = _trackCaptures.Find(c => c.head == head && c.track == track);

        if(capture == null) return ErrorNumber.OutOfRange;

        var tmpBuffer = new List<byte>();

        foreach(uint pulse in capture.fluxPulses) tmpBuffer.AddRange(UInt32ToFluxRepresentation(pulse));

        buffer = tmpBuffer.ToArray();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber SubTrackLength(uint head, ushort track, out byte length)
    {
        length = 0;

        if(_trackCaptures == null) return ErrorNumber.NotOpened;

        // KryoFlux doesn't support subtracks - filenames only contain cylinder and head
        // Check if any captures exist for this track
        List<TrackCapture> captures = _trackCaptures.FindAll(c => c.head == head && c.track == track);

        if(captures.Count <= 0) return ErrorNumber.OutOfRange;

        // Always return 1 since KryoFlux doesn't support subtracks
        length = 1;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetAllFluxCaptures(out List<FluxCapture> captures)
    {
        captures = [];

        if(_trackCaptures is { Count: > 0 })
        {
            // Group captures by head/track to assign capture indices
            // Note: KryoFlux doesn't support subtracks, so subTrack is always 0
            var grouped = _trackCaptures.GroupBy(c => new
                                         {
                                             c.head,
                                             c.track
                                         })
                                        .ToList();

            foreach(var group in grouped)
            {
                uint captureIndex = 0;

                foreach(TrackCapture trackCapture in group)
                {
                    captures.Add(new FluxCapture
                    {
                        Head            = trackCapture.head,
                        Track           = trackCapture.track,
                        SubTrack        = 0, // KryoFlux doesn't support subtracks
                        CaptureIndex    = captureIndex++,
                        IndexResolution = trackCapture.resolution,
                        DataResolution  = trackCapture.resolution
                    });
                }
            }
        }

        return ErrorNumber.NoError;
    }

#endregion
}