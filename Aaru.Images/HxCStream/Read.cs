// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Rebecca Wallander <sakcheen+github@gmail.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads HxC Stream flux images.
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
// Copyright © 2011-2026 Rebecca Wallander
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Compression;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Images;

public sealed partial class HxCStream
{
#region IFluxImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        _trackCaptures       = [];
        _trackFilePaths      = [];
        _mediaTags           = [];
        _imageInfo.Heads     = 0;
        _imageInfo.Cylinders = 0;

        string filename     = imageFilter.Filename;
        string parentFolder = imageFilter.ParentFolder;

        // We always open a single file - extract basename to find related track files
        if(!filename.EndsWith(".hxcstream", StringComparison.OrdinalIgnoreCase)) return ErrorNumber.InvalidArgument;

        // Extract basename - remove the track number pattern (e.g., "track00.0.hxcstream" -> "track")
        // The pattern is {basename}{cylinder:D2}.{head:D1}.hxcstream
        string basename     = filename[..^14]; // Remove ".XX.X.hxcstream" (14 chars)
        string fullBasename = Path.Combine(parentFolder, basename);

        AaruLogging.Debug(MODULE_NAME, "Opening HxCStream image from file: {0}", filename);
        AaruLogging.Debug(MODULE_NAME, "Basename: {0}",                          basename);
        AaruLogging.Debug(MODULE_NAME, "Full basename path: {0}",                fullBasename);

        // Discover track files by trying different cylinder/head combinations
        var trackFiles  = new Dictionary<(int cylinder, int head), string>();
        int minCylinder = int.MaxValue;
        int maxCylinder = int.MinValue;
        int minHead     = int.MaxValue;
        int maxHead     = int.MinValue;

        // Search for related track files
        for(var cylinder = 0; cylinder < 166; cylinder++)
        {
            for(var head = 0; head < 2; head++)
            {
                var trackfile = $"{fullBasename}{cylinder:D2}.{head:D1}.hxcstream";

                if(File.Exists(trackfile))
                {
                    trackFiles[(cylinder, head)] = trackfile;
                    minCylinder                  = Math.Min(minCylinder, cylinder);
                    maxCylinder                  = Math.Max(maxCylinder, cylinder);
                    minHead                      = Math.Min(minHead, head);
                    maxHead                      = Math.Max(maxHead, head);
                }
            }
        }

        if(trackFiles.Count == 0) return ErrorNumber.NoData;

        _imageInfo.Cylinders = (uint)(maxCylinder - minCylinder + 1);
        _imageInfo.Heads     = (uint)(maxHead     - minHead     + 1);

        AaruLogging.Debug(MODULE_NAME, "Found {0} track files", trackFiles.Count);

        AaruLogging.Debug(MODULE_NAME,
                          "Cylinder range: {0} to {1} ({2} cylinders)",
                          minCylinder,
                          maxCylinder,
                          _imageInfo.Cylinders);

        AaruLogging.Debug(MODULE_NAME, "Head range: {0} to {1} ({2} heads)", minHead, maxHead, _imageInfo.Heads);

        // Process each track file
        var trackIndex  = 0;
        int totalTracks = trackFiles.Count;

        AaruLogging.Debug(MODULE_NAME, "Processing {0} track files...", totalTracks);

        foreach((int cylinder, int head) key in trackFiles.Keys.OrderBy(k => k.cylinder).ThenBy(k => k.head).ToList())
        {
            trackIndex++;
            string trackfile = trackFiles[key];
            _trackFilePaths.Add(trackfile);

            AaruLogging.Debug(MODULE_NAME,
                              "Processing track {0}/{1}: cylinder {2}, head {3}",
                              trackIndex,
                              totalTracks,
                              key.cylinder,
                              key.head);

            ErrorNumber error = ProcessTrackFile(trackfile, (uint)key.head, (ushort)key.cylinder);

            if(error != ErrorNumber.NoError) return error;
        }

        AaruLogging.Debug(MODULE_NAME, "Successfully processed all {0} track files", totalTracks);

        _imageInfo.MetadataMediaType = MetadataMediaType.BlockMedia;

        return ErrorNumber.NoError;
    }

    ErrorNumber ProcessTrackFile(string trackfile, uint head, ushort track)
    {
        if(!File.Exists(trackfile)) return ErrorNumber.NoSuchFile;

        AaruLogging.Debug(MODULE_NAME, "Processing track file: {0} (head {1}, track {2})", trackfile, head, track);

        using FileStream fileStream = File.OpenRead(trackfile);
        var              fileData   = new byte[fileStream.Length];
        fileStream.EnsureRead(fileData, 0, (int)fileStream.Length);

        AaruLogging.Debug(MODULE_NAME, "Track file size: {0} bytes", fileData.Length);

        uint   samplePeriod = DEFAULT_RESOLUTION; // Default 40,000 ns (25 MHz)
        var    fluxPulses   = new List<uint>();
        var    ioStream     = new List<ushort>();
        string metadata     = null;

        long fileOffset = 0;

        while(fileOffset < fileData.Length)
        {
            if(fileOffset + Marshal.SizeOf<HxCStreamChunkHeader>() > fileData.Length)
                return ErrorNumber.InvalidArgument;

            HxCStreamChunkHeader chunkHeader =
                Marshal.ByteArrayToStructureLittleEndian<HxCStreamChunkHeader>(fileData,
                                                                               (int)fileOffset,
                                                                               Marshal.SizeOf<HxCStreamChunkHeader>());

            AaruLogging.Debug(MODULE_NAME,
                              "Chunk at offset {0}: signature = \"{1}\", size = {2}, packetNumber = {3}",
                              fileOffset,
                              StringHandlers.CToString(chunkHeader.signature),
                              chunkHeader.size,
                              chunkHeader.packetNumber);

            if(!_hxcStreamSignature.SequenceEqual(chunkHeader.signature)) return ErrorNumber.InvalidArgument;

            if(chunkHeader.size > fileData.Length - fileOffset) return ErrorNumber.InvalidArgument;

            // A chunk must at least hold its own header and CRC, and size 0 would loop forever
            if(chunkHeader.size < Marshal.SizeOf<HxCStreamChunkHeader>() + 4) return ErrorNumber.InvalidArgument;

            // Verify CRC32 - calculate CRC of chunk data (excluding the CRC itself)
            var chunkData = new byte[chunkHeader.size - 4];
            Array.Copy(fileData, (int)fileOffset, chunkData, 0, (int)(chunkHeader.size - 4));

            var storedCrc = BitConverter.ToUInt32(fileData, (int)(fileOffset + chunkHeader.size - 4));

            if(!VerifyChunkCrc32(chunkData, storedCrc))
            {
                AaruLogging.Error(MODULE_NAME, "CRC32 mismatch in chunk at offset {0}", fileOffset);

                return ErrorNumber.InvalidArgument;
            }

            AaruLogging.Debug(MODULE_NAME, "Chunk CRC32 verified successfully");

            long packetOffset = fileOffset                    + Marshal.SizeOf<HxCStreamChunkHeader>();
            long chunkEnd     = fileOffset + chunkHeader.size - 4;

            while(packetOffset < chunkEnd)
            {
                if(packetOffset + 4 > fileData.Length) break;

                var type = BitConverter.ToUInt32(fileData, (int)packetOffset);

                AaruLogging.Debug(MODULE_NAME, "Packet at offset {0}: type = 0x{1:X8}", packetOffset, type);

                switch(type)
                {
                    case 0x0: // Metadata
                    {
                        if(packetOffset + Marshal.SizeOf<HxCStreamMetadataHeader>() > fileData.Length)
                            return ErrorNumber.InvalidArgument;

                        HxCStreamMetadataHeader metadataHeader =
                            Marshal.ByteArrayToStructureLittleEndian<HxCStreamMetadataHeader>(fileData,
                                (int)packetOffset,
                                Marshal.SizeOf<HxCStreamMetadataHeader>());

                        AaruLogging.Debug(MODULE_NAME,
                                          "Metadata packet: type = 0x{0:X8}, payloadSize = {1}",
                                          metadataHeader.type,
                                          metadataHeader.payloadSize);

                        if(packetOffset + Marshal.SizeOf<HxCStreamMetadataHeader>() + metadataHeader.payloadSize >
                           fileData.Length)
                            return ErrorNumber.InvalidArgument;

                        var metadataBytes = new byte[metadataHeader.payloadSize];

                        Array.Copy(fileData,
                                   (int)packetOffset + Marshal.SizeOf<HxCStreamMetadataHeader>(),
                                   metadataBytes,
                                   0,
                                   (int)metadataHeader.payloadSize);

                        metadata = Encoding.UTF8.GetString(metadataBytes);

                        AaruLogging.Debug(MODULE_NAME, "Metadata content: {0}", metadata);

                        // Parse metadata and populate ImageInfo (only parse once, from first chunk)
                        if(string.IsNullOrEmpty(_imageInfo.Application)) ParseMetadata(metadata, _imageInfo);

                        // Check for sample rate in metadata
                        if(metadata.Contains("sample_rate_hz 25000000"))
                        {
                            samplePeriod = 40000;
                            AaruLogging.Debug(MODULE_NAME, "Sample rate detected: 25 MHz (40000 ns period)");
                        }
                        else if(metadata.Contains("sample_rate_hz 50000000"))
                        {
                            samplePeriod = 20000;
                            AaruLogging.Debug(MODULE_NAME, "Sample rate detected: 50 MHz (20000 ns period)");
                        }
                        else
                            AaruLogging.Debug(MODULE_NAME, "Using default sample rate: 25 MHz (40000 ns period)");

                        packetOffset += Marshal.SizeOf<HxCStreamMetadataHeader>() + metadataHeader.payloadSize;

                        // Align to 4 bytes
                        if(packetOffset % 4 != 0) packetOffset += 4 - packetOffset % 4;

                        break;
                    }
                    case 0x1: // Packed IO stream
                    {
                        if(packetOffset + Marshal.SizeOf<HxCStreamPackedIoHeader>() > fileData.Length)
                            return ErrorNumber.InvalidArgument;

                        HxCStreamPackedIoHeader ioHeader =
                            Marshal.ByteArrayToStructureLittleEndian<HxCStreamPackedIoHeader>(fileData,
                                (int)packetOffset,
                                Marshal.SizeOf<HxCStreamPackedIoHeader>());

                        AaruLogging.Debug(MODULE_NAME,
                                          "Packed IO stream packet: type = 0x{0:X8}, payloadSize = {1}, packedSize = {2}, unpackedSize = {3}",
                                          ioHeader.type,
                                          ioHeader.payloadSize,
                                          ioHeader.packedSize,
                                          ioHeader.unpackedSize);

                        if(packetOffset + Marshal.SizeOf<HxCStreamPackedIoHeader>() + ioHeader.packedSize >
                           fileData.Length)
                            return ErrorNumber.InvalidArgument;

                        var packedData = new byte[ioHeader.packedSize];

                        Array.Copy(fileData,
                                   (int)packetOffset + Marshal.SizeOf<HxCStreamPackedIoHeader>(),
                                   packedData,
                                   0,
                                   (int)ioHeader.packedSize);

                        var unpackedData = new byte[ioHeader.unpackedSize];
                        int decoded      = LZ4.DecodeBuffer(packedData, unpackedData);

                        if(decoded != ioHeader.unpackedSize) return ErrorNumber.InvalidArgument;

                        AaruLogging.Debug(MODULE_NAME,
                                          "Decompressed IO stream: {0} bytes -> {1} bytes ({2} 16-bit values)",
                                          ioHeader.packedSize,
                                          decoded,
                                          decoded / 2);

                        // Convert to ushort array
                        for(var i = 0; i < unpackedData.Length; i += 2)
                            if(i + 1 < unpackedData.Length)
                                ioStream.Add(BitConverter.ToUInt16(unpackedData, i));

                        packetOffset += Marshal.SizeOf<HxCStreamPackedIoHeader>() + ioHeader.packedSize;

                        // Align to 4 bytes
                        if(packetOffset % 4 != 0) packetOffset += 4 - packetOffset % 4;

                        break;
                    }
                    case 0x2: // Packed flux stream
                    {
                        if(packetOffset + Marshal.SizeOf<HxCStreamPackedStreamHeader>() > fileData.Length)
                            return ErrorNumber.InvalidArgument;

                        HxCStreamPackedStreamHeader streamHeader =
                            Marshal.ByteArrayToStructureLittleEndian<HxCStreamPackedStreamHeader>(fileData,
                                (int)packetOffset,
                                Marshal.SizeOf<HxCStreamPackedStreamHeader>());

                        AaruLogging.Debug(MODULE_NAME,
                                          "Packed flux stream packet: type = 0x{0:X8}, payloadSize = {1}, packedSize = {2}, unpackedSize = {3}, numberOfPulses = {4}",
                                          streamHeader.type,
                                          streamHeader.payloadSize,
                                          streamHeader.packedSize,
                                          streamHeader.unpackedSize,
                                          streamHeader.numberOfPulses);

                        if(packetOffset + Marshal.SizeOf<HxCStreamPackedStreamHeader>() + streamHeader.packedSize >
                           fileData.Length)
                            return ErrorNumber.InvalidArgument;

                        var packedData = new byte[streamHeader.packedSize];

                        Array.Copy(fileData,
                                   (int)packetOffset + Marshal.SizeOf<HxCStreamPackedStreamHeader>(),
                                   packedData,
                                   0,
                                   (int)streamHeader.packedSize);

                        var unpackedData = new byte[streamHeader.unpackedSize];
                        int decoded      = LZ4.DecodeBuffer(packedData, unpackedData);

                        if(decoded != streamHeader.unpackedSize) return ErrorNumber.InvalidArgument;

                        AaruLogging.Debug(MODULE_NAME,
                                          "Decompressed flux stream: {0} bytes -> {1} bytes",
                                          streamHeader.packedSize,
                                          decoded);

                        // Decode variable-length pulses
                        uint numberOfPulses = streamHeader.numberOfPulses;

                        uint[] pulses = DecodeVariableLengthPulses(unpackedData,
                                                                   streamHeader.unpackedSize,
                                                                   ref numberOfPulses);

                        AaruLogging.Debug(MODULE_NAME,
                                          "Decoded {0} flux pulses (expected {1})",
                                          pulses.Length,
                                          numberOfPulses);

                        if(pulses.Length != numberOfPulses) return ErrorNumber.InvalidArgument;

                        fluxPulses.AddRange(pulses);

                        packetOffset += Marshal.SizeOf<HxCStreamPackedStreamHeader>() + streamHeader.packedSize;

                        // Align to 4 bytes
                        if(packetOffset % 4 != 0) packetOffset += 4 - packetOffset % 4;

                        break;
                    }
                    default:
                        AaruLogging.Error(MODULE_NAME, "Unknown packet type: 0x{0:X8}", type);

                        return ErrorNumber.InvalidArgument;
                }
            }

            fileOffset += chunkHeader.size;
        }

        AaruLogging.Debug(MODULE_NAME,
                          "Finished processing chunks. Total flux pulses: {0}, IO stream values: {1}",
                          fluxPulses.Count,
                          ioStream.Count);

        // Extract write protect status from IO stream
        // The write protect flag should be constant for a capture, so we only need to check the first value
        if(ioStream.Count > 0 && !_mediaTags.ContainsKey(MediaTagType.Floppy_WriteProtection))
        {
            IoStreamState firstState     = DecodeIoStreamValue(ioStream[0]);
            bool          writeProtected = firstState.WriteProtect;

            // Store as boolean: 1 byte where 0 = false (not write protected), 1 = true (write protected)
            _mediaTags[MediaTagType.Floppy_WriteProtection] = writeProtected ? [1] : [0];

            AaruLogging.Debug(MODULE_NAME,
                              "Write protect status from IO stream: {0}",
                              writeProtected ? "write protected" : "not write protected");
        }

        // Extract index signals from IO stream
        var indexPositions = new List<uint>();

        if(ioStream.Count > 0)
        {
            IoStreamState previousState = DecodeIoStreamValue(ioStream[0]);
            bool          oldIndex      = previousState.IndexSignal;
            bool          startsAtIndex = previousState.IndexSignal;
            if(startsAtIndex) indexPositions.Add(0);

            uint totalTicks = 0;
            var  pulseIndex = 0;

            for(var i = 0; i < ioStream.Count; i++)
            {
                IoStreamState currentState = DecodeIoStreamValue(ioStream[i]);
                bool          currentIndex = currentState.IndexSignal;

                if(currentIndex != oldIndex && currentIndex)
                {
                    // Index signal transition to high
                    // Map to flux stream position
                    var targetTicks = (uint)(i * 16);

                    while(pulseIndex < fluxPulses.Count && totalTicks < targetTicks)
                    {
                        totalTicks += fluxPulses[pulseIndex];
                        pulseIndex++;
                    }

                    if(pulseIndex < fluxPulses.Count) indexPositions.Add((uint)pulseIndex);
                }

                oldIndex = currentIndex;
            }

            AaruLogging.Debug(MODULE_NAME, "Extracted {0} index positions from IO stream", indexPositions.Count);
        }
        else
            AaruLogging.Debug(MODULE_NAME, "No IO stream data available, no index positions extracted");

        // Create track capture
        // Note: HxCStream doesn't support subtracks, so subTrack is always 0
        var capture = new TrackCapture
        {
            head           = head,
            track          = track,
            resolution     = samplePeriod,
            fluxPulses     = fluxPulses.ToArray(),
            indexPositions = indexPositions.ToArray()
        };

        AaruLogging.Debug(MODULE_NAME,
                          "Created track capture: head = {0}, track = {1}, resolution = {2} ns, fluxPulses = {3}, indexPositions = {4}",
                          capture.head,
                          capture.track,
                          capture.resolution,
                          capture.fluxPulses.Length,
                          capture.indexPositions.Length);

        _trackCaptures.Add(capture);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CapturesLength(uint head, ushort track, byte subTrack, out uint length)
    {
        length = 0;

        if(_trackCaptures == null) return ErrorNumber.NotOpened;

        // HxCStream doesn't support subtracks - only subTrack 0 is valid
        if(subTrack != 0) return ErrorNumber.OutOfRange;

        // HxCStream has one file per track/head, which results in exactly one capture
        // Check if a capture exists for this track
        bool hasCapture = _trackCaptures.Any(c => c.head == head && c.track == track);

        length = hasCapture ? 1u : 0u;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFluxIndexResolution(uint      head, ushort track, byte subTrack, uint captureIndex,
                                               out ulong resolution)
    {
        resolution = 0;

        // HxCStream doesn't support subtracks - only subTrack 0 is valid
        if(subTrack != 0) return ErrorNumber.OutOfRange;

        // HxCStream has one file per track/head, which results in exactly one capture (captureIndex 0)
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

        // HxCStream doesn't support subtracks - only subTrack 0 is valid
        if(subTrack != 0) return ErrorNumber.OutOfRange;

        // HxCStream has one file per track/head, which results in exactly one capture (captureIndex 0)
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

        // HxCStream doesn't support subtracks - only subTrack 0 is valid
        if(subTrack != 0) return ErrorNumber.OutOfRange;

        // HxCStream has one file per track/head, which results in exactly one capture (captureIndex 0)
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

        // HxCStream doesn't support subtracks - only subTrack 0 is valid
        if(subTrack != 0) return ErrorNumber.OutOfRange;

        // HxCStream has one file per track/head, which results in exactly one capture (captureIndex 0)
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

        // HxCStream doesn't support subtracks - only subTrack 0 is valid
        if(subTrack != 0) return ErrorNumber.OutOfRange;

        // HxCStream has one file per track/head, which results in exactly one capture (captureIndex 0)
        if(captureIndex != 0) return ErrorNumber.OutOfRange;

        TrackCapture capture = _trackCaptures.Find(c => c.head == head && c.track == track);

        if(capture == null) return ErrorNumber.OutOfRange;

        var  tmpBuffer     = new List<byte>();
        uint previousTicks = 0;

        foreach(uint indexPos in capture.indexPositions)
        {
            // Convert index position to ticks
            uint ticks                                                                = 0;
            for(uint i = 0; i < indexPos && i < capture.fluxPulses.Length; i++) ticks += capture.fluxPulses[i];

            uint deltaTicks = ticks - previousTicks;
            tmpBuffer.AddRange(UInt32ToFluxRepresentation(deltaTicks));
            previousTicks = ticks;
        }

        buffer = tmpBuffer.ToArray();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFluxDataCapture(uint head, ushort track, byte subTrack, uint captureIndex, out byte[] buffer)
    {
        buffer = null;

        // HxCStream doesn't support subtracks - only subTrack 0 is valid
        if(subTrack != 0) return ErrorNumber.OutOfRange;

        // HxCStream has one file per track/head, which results in exactly one capture (captureIndex 0)
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

        // HxCStream doesn't support subtracks - filenames only contain cylinder and head
        // Check if any captures exist for this track
        List<TrackCapture> captures = _trackCaptures.FindAll(c => c.head == head && c.track == track);

        if(captures.Count <= 0) return ErrorNumber.OutOfRange;

        // Always return 1 since HxCStream doesn't support subtracks
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
            // Note: HxCStream doesn't support subtracks, so subTrack is always 0
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
                        SubTrack        = 0, // HxCStream doesn't support subtracks
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

#region IMediaImage Members

    /// <inheritdoc />
    public ErrorNumber ReadMediaTag(MediaTagType tag, out byte[] buffer)
    {
        buffer = null;

        if(_mediaTags == null) return ErrorNumber.NotOpened;

        if(_mediaTags.TryGetValue(tag, out byte[] tagData))
        {
            buffer = new byte[tagData.Length];
            Array.Copy(tagData, buffer, tagData.Length);

            return ErrorNumber.NoError;
        }

        return ErrorNumber.NoData;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, bool negative, out byte[] buffer, out SectorStatus sectorStatus)
    {
        buffer       = null;
        sectorStatus = SectorStatus.NotDumped;

        return ErrorNumber.NotImplemented;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong            sectorAddress, bool negative, out byte[] buffer,
                                      out SectorStatus sectorStatus)
    {
        buffer       = null;
        sectorStatus = SectorStatus.NotDumped;

        return ErrorNumber.NotImplemented;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                   out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        return ErrorNumber.NotImplemented;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
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
    public ErrorNumber ReadSectorTag(ulong sectorAddress, bool negative, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        return ErrorNumber.NotImplemented;
    }

#endregion
}