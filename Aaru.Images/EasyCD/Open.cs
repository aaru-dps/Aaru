// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Open.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Opens Easy CD Creator disc images.
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
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Decoders.CD;
using Aaru.Helpers;
using Aaru.Logging;
using Humanizer;
using Session = Aaru.CommonTypes.Structs.Session;

namespace Aaru.Images;

public sealed partial class EasyCD
{
    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < 12) return ErrorNumber.InvalidArgument;

        // Step 1: Scan all RIFF blocks to find "disc" and "ofs " blocks
        long discOffset = 0;
        uint discLength = 0;
        long ofsOffset  = 0;
        uint ofsLength  = 0;

        stream.Seek(0, SeekOrigin.Begin);

        while(stream.Position < stream.Length)
        {
            var headerBytes = new byte[12];

            if(stream.EnsureRead(headerBytes, 0, 12) < 12) break;

            // Verify RIFF signature
            if(headerBytes[0] != _cifRiffSignature[0] ||
               headerBytes[1] != _cifRiffSignature[1] ||
               headerBytes[2] != _cifRiffSignature[2] ||
               headerBytes[3] != _cifRiffSignature[3])
            {
                AaruLogging.Error(MODULE_NAME + ": expected RIFF signature, got {0}",
                                  Encoding.ASCII.GetString(headerBytes, 0, 4));

                return ErrorNumber.InvalidArgument;
            }

            uint blockLength = BitConverter.ToUInt32(headerBytes, 4) - 4; // Length includes the type field
            long dataOffset  = stream.Position;                           // Current position is right after header

            byte[] blockType = headerBytes[8..12];

            AaruLogging.Debug(MODULE_NAME,
                              "RIFF chunk type '{0}': offset {1} (0x{1:X}), length {2} (0x{2:X})",
                              Encoding.ASCII.GetString(blockType),
                              dataOffset,
                              blockLength);

            if(blockType[0] == _cifDiscType[0] &&
               blockType[1] == _cifDiscType[1] &&
               blockType[2] == _cifDiscType[2] &&
               blockType[3] == _cifDiscType[3])
            {
                discOffset = dataOffset;
                discLength = blockLength;
            }
            else if(blockType[0] == _cifOffsetType[0] &&
                    blockType[1] == _cifOffsetType[1] &&
                    blockType[2] == _cifOffsetType[2] &&
                    blockType[3] == _cifOffsetType[3])
            {
                ofsOffset = dataOffset;
                ofsLength = blockLength;
            }

            // Skip the block contents
            stream.Seek(blockLength, SeekOrigin.Current);

            // RIFF blocks are padded to even boundaries
            if(blockLength % 2 != 0) stream.Seek(1, SeekOrigin.Current);
        }

        if(discOffset == 0 || discLength == 0)
        {
            AaruLogging.Error(MODULE_NAME + ": 'disc' block not found");

            return ErrorNumber.InvalidArgument;
        }

        if(ofsOffset == 0 || ofsLength == 0)
        {
            AaruLogging.Error(MODULE_NAME + ": 'ofs ' block not found");

            return ErrorNumber.InvalidArgument;
        }

        // Step 2: Parse the "ofs " block to get offset entries
        stream.Seek(ofsOffset, SeekOrigin.Begin);

        // Skip 8 dummy bytes
        stream.Seek(8, SeekOrigin.Current);

        // Read number of offset entries
        var numEntriesBytes = new byte[2];
        stream.EnsureRead(numEntriesBytes, 0, 2);
        var numOffsetEntries = BitConverter.ToUInt16(numEntriesBytes, 0);

        AaruLogging.Debug(MODULE_NAME, "Number of offset entries: {0}", numOffsetEntries);

        var offsetEntries = new CifOffsetEntry[numOffsetEntries];
        int ofsEntrySize  = Marshal.SizeOf<CifOffsetEntry>();

        for(var i = 0; i < numOffsetEntries && stream.Position < ofsOffset + ofsLength; i++)
        {
            var entryBytes = new byte[ofsEntrySize];
            stream.EnsureRead(entryBytes, 0, ofsEntrySize);
            CifOffsetEntry entry = Marshal.ByteArrayToStructureLittleEndian<CifOffsetEntry>(entryBytes);

            // Verify RIFF signature
            if(!entry.signature.SequenceEqual(_cifRiffSignature))
            {
                AaruLogging.Error(MODULE_NAME + ": expected RIFF in offset entry, got {0}",
                                  Encoding.ASCII.GetString(entry.signature));

                return ErrorNumber.InvalidArgument;
            }

            // Adjust length and offset
            entry.length -= 4;  // Length includes the type field size
            entry.offset += 12; // Offset points to RIFF block start, skip full 12-byte header

            AaruLogging.Debug(MODULE_NAME,
                              "Offset entry #{0}: type '{1}', offset {2} (0x{2:X}), length {3} (0x{3:X})",
                              i,
                              Encoding.ASCII.GetString(entry.type),
                              entry.offset,
                              entry.length);

            offsetEntries[i] = entry;
        }

        // Step 3: Parse the "disc" block
        stream.Seek(discOffset, SeekOrigin.Begin);

        // Skip 8 dummy bytes
        stream.Seek(8, SeekOrigin.Current);

        // Read disc descriptor
        int discDescSize  = Marshal.SizeOf<CifDiscDescriptor>();
        var discDescBytes = new byte[discDescSize];
        stream.EnsureRead(discDescBytes, 0, discDescSize);
        CifDiscDescriptor discDescriptor = Marshal.ByteArrayToStructureLittleEndian<CifDiscDescriptor>(discDescBytes);

        AaruLogging.Debug(MODULE_NAME, "Disc descriptor:");
        AaruLogging.Debug(MODULE_NAME, "  descriptor length: {0} (0x{0:X})", discDescriptor.descriptorLength);
        AaruLogging.Debug(MODULE_NAME, "  sessions: {0}",                    discDescriptor.numSessions);
        AaruLogging.Debug(MODULE_NAME, "  tracks: {0}",                      discDescriptor.numTracks);
        AaruLogging.Debug(MODULE_NAME, "  title length: {0}",                discDescriptor.titleLength);
        AaruLogging.Debug(MODULE_NAME, "  image type: {0}",                  discDescriptor.imageType);

        // Read and skip variable-length title/artist
        int variableLen = discDescriptor.descriptorLength - discDescSize;

        if(variableLen > 0)
        {
            var titleArtistData = new byte[variableLen];
            stream.EnsureRead(titleArtistData, 0, variableLen);

            if(discDescriptor.titleLength > 0 && discDescriptor.titleLength <= variableLen)
            {
                string title = Encoding.Default.GetString(titleArtistData, 0, discDescriptor.titleLength).TrimEnd('\0');

                AaruLogging.Debug(MODULE_NAME, "  title: \"{0}\"", title);

                if(!string.IsNullOrWhiteSpace(title)) _imageInfo.MediaTitle = title;
            }
        }

        // Initialize track/session/partition lists
        Tracks     = [];
        Sessions   = [];
        Partitions = [];

        var          trackCounter      = 0;
        uint         trackSequence     = 0;
        ushort       sessionSequence   = 0;
        ulong        currentSector     = 0;
        CifTrackType previousTrackType = CifTrackType.Audio;

        // Read sessions
        for(var s = 0; s < discDescriptor.numSessions; s++)
        {
            // Read session descriptor
            int sessionDescSize  = Marshal.SizeOf<CifSessionDescriptor>();
            var sessionDescBytes = new byte[sessionDescSize];
            stream.EnsureRead(sessionDescBytes, 0, sessionDescSize);

            CifSessionDescriptor sessionDesc =
                Marshal.ByteArrayToStructureLittleEndian<CifSessionDescriptor>(sessionDescBytes);

            // Skip variable part of session descriptor
            int sessionVariableLen = sessionDesc.descriptorLength - sessionDescSize;

            if(sessionVariableLen > 0) stream.Seek(sessionVariableLen, SeekOrigin.Current);

            sessionSequence++;

            AaruLogging.Debug(MODULE_NAME, "Session {0}:",        sessionSequence);
            AaruLogging.Debug(MODULE_NAME, "  tracks: {0}",       sessionDesc.numTracks);
            AaruLogging.Debug(MODULE_NAME, "  session type: {0}", sessionDesc.sessionType);

            var session = new Session
            {
                Sequence   = sessionSequence,
                StartTrack = uint.MaxValue,
                EndTrack   = uint.MinValue
            };

            // Read tracks in this session
            for(var t = 0; t < sessionDesc.numTracks; t++)
            {
                // Read the track descriptor fixed part
                int trackDescSize  = Marshal.SizeOf<CifTrackDescriptor>();
                var trackDescBytes = new byte[trackDescSize];
                stream.EnsureRead(trackDescBytes, 0, trackDescSize);

                CifTrackDescriptor trackDesc =
                    Marshal.ByteArrayToStructureLittleEndian<CifTrackDescriptor>(trackDescBytes);

                AaruLogging.Debug(MODULE_NAME, "Track descriptor:");
                AaruLogging.Debug(MODULE_NAME, "  descriptor length: {0} (0x{0:X})", trackDesc.descriptorLength);
                AaruLogging.Debug(MODULE_NAME, "  sectors: {0} (0x{0:X})",           trackDesc.numSectors);
                AaruLogging.Debug(MODULE_NAME, "  track type: {0}",                  trackDesc.trackType);
                AaruLogging.Debug(MODULE_NAME, "  dao mode: {0}",                    trackDesc.daoMode);
                AaruLogging.Debug(MODULE_NAME, "  sector data size: {0} (0x{0:X})",  trackDesc.sectorDataSize);

                // Read audio or data track descriptor part
                int trackVariableLen = trackDesc.descriptorLength - trackDescSize;

                if(trackVariableLen > 0) stream.Seek(trackVariableLen, SeekOrigin.Current);

                // Get the corresponding offset entry
                if(trackCounter >= numOffsetEntries)
                {
                    AaruLogging.Error(MODULE_NAME + ": track counter exceeds offset entries");

                    return ErrorNumber.InvalidArgument;
                }

                CifOffsetEntry offsetEntry = offsetEntries[trackCounter++];

                // Determine sector size and track type
                int       sectorSize;
                TrackType aTrackType;

                switch(trackDesc.trackType)
                {
                    case CifTrackType.Audio:
                        sectorSize = AUDIO_SECTOR_SIZE;
                        aTrackType = TrackType.Audio;

                        break;
                    case CifTrackType.Mode1:
                        sectorSize = MODE1_SECTOR_SIZE;
                        aTrackType = TrackType.CdMode1;

                        break;
                    case CifTrackType.Mode2Form1:
                        sectorSize = MODE2_FORM1_SECTOR_SIZE;
                        aTrackType = TrackType.CdMode2Form1;

                        break;
                    case CifTrackType.Mode2Mixed:
                        sectorSize = MODE2_MIXED_SECTOR_SIZE;
                        aTrackType = TrackType.CdMode2Formless;

                        break;
                    default:
                        AaruLogging.Error(MODULE_NAME + ": unknown track type {0}", trackDesc.trackType);

                        return ErrorNumber.InvalidArgument;
                }

                // Compute actual track length (minimum MIN_TRACK_LENGTH)
                uint trackLength = Math.Max(trackDesc.numSectors, MIN_TRACK_LENGTH);

                AaruLogging.Debug(MODULE_NAME, "  computed track length: {0} (0x{0:X})", trackLength);

                // Adjust track length if it exceeds available data
                if(trackLength * (ulong)sectorSize > offsetEntry.length)
                {
                    uint adjusted = offsetEntry.length / (uint)sectorSize;

                    AaruLogging.Debug(MODULE_NAME,
                                      "Declared track length {0} exceeds available data " +
                                      "(offset entry length {1}); adjusting to {2}",
                                      trackLength,
                                      offsetEntry.length,
                                      adjusted);

                    trackLength = adjusted;
                }

                // A zero-length track would underflow EndSector below and swallow every LBA lookup
                if(trackLength == 0)
                {
                    AaruLogging.Error(MODULE_NAME + ": track {0} has no data", trackCounter);

                    return ErrorNumber.InvalidArgument;
                }

                trackSequence++;

                // Determine if we need a pregap
                ulong pregap = 0;

                if(t == 0                                                                               ||
                   previousTrackType == CifTrackType.Audio && trackDesc.trackType != CifTrackType.Audio ||
                   previousTrackType != CifTrackType.Audio && trackDesc.trackType == CifTrackType.Audio)
                    pregap = PREGAP_LENGTH;

                int userDataSize = aTrackType == TrackType.CdMode2Form1 ? MODE1_SECTOR_SIZE : sectorSize;

                var track = new Track
                {
                    BytesPerSector    = userDataSize,
                    RawBytesPerSector = sectorSize,
                    Sequence          = trackSequence,
                    Session           = sessionSequence,
                    StartSector       = currentSector,
                    EndSector         = currentSector + trackLength - 1,
                    Type              = aTrackType,
                    SubchannelType    = TrackSubchannelType.None,
                    Description       = string.Format(Localization.Track_0, trackSequence),
                    File              = imageFilter.Filename,
                    FileType          = "BINARY",
                    Filter            = imageFilter,
                    FileOffset        = offsetEntry.offset,
                    Pregap            = pregap
                };

                if(trackSequence == 1)
                {
                    track.Indexes = new Dictionary<ushort, int>
                    {
                        [0] = -150,
                        [1] = 0
                    };
                }
                else if(pregap > 0)
                {
                    track.Indexes = new Dictionary<ushort, int>
                    {
                        [0] = (int)currentSector,
                        [1] = (int)(currentSector + pregap)
                    };
                }
                else
                {
                    track.Indexes = new Dictionary<ushort, int>
                    {
                        [1] = (int)currentSector
                    };
                }

                // Update session bounds
                if(trackSequence < session.StartTrack)
                {
                    session.StartTrack  = trackSequence;
                    session.StartSector = currentSector;
                }

                if(trackSequence > session.EndTrack)
                {
                    session.EndTrack  = trackSequence;
                    session.EndSector = currentSector + trackLength - 1;
                }

                // Update max sector size
                if(_imageInfo.SectorSize < (uint)sectorSize) _imageInfo.SectorSize = (uint)sectorSize;

                // Build partition
                var partition = new Partition
                {
                    Sequence    = trackSequence - 1,
                    Start       = currentSector,
                    Length      = trackLength,
                    Offset      = offsetEntry.offset,
                    Size        = trackLength * (ulong)sectorSize,
                    Description = string.Format(Localization.Track_0, trackSequence),
                    Type        = aTrackType.Humanize()
                };

                Tracks.Add(track);
                Partitions.Add(partition);

                currentSector     += trackLength;
                previousTrackType =  trackDesc.trackType;
            }

            Sessions.Add(session);
        }

        // Set up image info
        _imageInfo.Sectors              = currentSector;
        _imageInfo.ImageSize            = (ulong)imageFilter.DataForkLength;
        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MetadataMediaType    = MetadataMediaType.OpticalDisc;
        _imageInfo.Application          = "Easy CD Creator";

        // Determine media type based on tracks
        var hasData      = false;
        var hasAudio     = false;
        var hasMode2     = false;
        var firstIsAudio = false;
        var firstIsData  = false;

        for(var i = 0; i < Tracks.Count; i++)
        {
            firstIsAudio |= i == 0 && Tracks[i].Type == TrackType.Audio;
            firstIsData  |= i == 0 && Tracks[i].Type != TrackType.Audio;
            hasData      |= i != 0 && Tracks[i].Type != TrackType.Audio;
            hasAudio     |= i != 0 && Tracks[i].Type == TrackType.Audio;

            hasMode2 = Tracks[i].Type switch
                       {
                           TrackType.CdMode2Form1 or TrackType.CdMode2Form2 or TrackType.CdMode2Formless => true,
                           _                                                                             => hasMode2
                       };
        }

        if(!hasData && !firstIsData)
            _imageInfo.MediaType = MediaType.CDDA;
        else if(firstIsAudio && hasData && Sessions.Count > 1 && hasMode2)
            _imageInfo.MediaType = MediaType.CDPLUS;
        else if(firstIsData && hasAudio || hasMode2)
            _imageInfo.MediaType = MediaType.CDROMXA;
        else if(!hasAudio)
            _imageInfo.MediaType = MediaType.CDROM;
        else
            _imageInfo.MediaType = MediaType.CD;

        _imageStream   = stream;
        _sectorBuilder = new SectorBuilder();

        return ErrorNumber.NoError;
    }
}