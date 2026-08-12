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
//     Opens WinOnCD disc images.
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

public sealed partial class WinOnCD
{
    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < Marshal.SizeOf<C2dHeaderBlock>()) return ErrorNumber.InvalidArgument;

        // Step 1: Read and parse the file header
        int headerSize  = Marshal.SizeOf<C2dHeaderBlock>();
        var headerBytes = new byte[headerSize];
        stream.EnsureRead(headerBytes, 0, headerSize);

        C2dHeaderBlock header = Marshal.ByteArrayToStructureLittleEndian<C2dHeaderBlock>(headerBytes);

        string signature = Encoding.ASCII.GetString(header.signature).TrimEnd('\0');

        AaruLogging.Debug(MODULE_NAME, "Signature: {0}",         signature);
        AaruLogging.Debug(MODULE_NAME, "Header size: {0}",       header.header_size);
        AaruLogging.Debug(MODULE_NAME, "Has UPC/EAN: {0}",       header.has_upc_ean);
        AaruLogging.Debug(MODULE_NAME, "Track blocks: {0}",      header.num_track_blocks);
        AaruLogging.Debug(MODULE_NAME, "CD-Text size: {0}",      header.size_cdtext);
        AaruLogging.Debug(MODULE_NAME, "Tracks offset: 0x{0:X}", header.offset_tracks);
        AaruLogging.Debug(MODULE_NAME, "C2CK offset: 0x{0:X}",   header.offset_c2ck);

        string description = Encoding.ASCII.GetString(header.description).TrimEnd('\0');

        if(!string.IsNullOrWhiteSpace(description)) AaruLogging.Debug(MODULE_NAME, "Description: {0}", description);

        if(header.has_upc_ean != 0)
        {
            string upcEan = Encoding.ASCII.GetString(header.upc_ean).TrimEnd('\0');
            AaruLogging.Debug(MODULE_NAME, "UPC/EAN: {0}", upcEan);
        }

        // Step 2: Read all track blocks
        stream.Seek(header.offset_tracks, SeekOrigin.Begin);

        int trackBlockSize = Marshal.SizeOf<C2dTrackBlock>();
        var trackBlocks    = new C2dTrackBlock[header.num_track_blocks];

        for(var i = 0; i < header.num_track_blocks; i++)
        {
            var blockBytes = new byte[trackBlockSize];
            stream.EnsureRead(blockBytes, 0, trackBlockSize);
            trackBlocks[i] = Marshal.ByteArrayToStructureLittleEndian<C2dTrackBlock>(blockBytes);
        }

        // Step 3: Process track blocks and build tracks, sessions, and partitions
        Tracks      = [];
        Sessions    = [];
        Partitions  = [];
        _trackFlags = new Dictionary<uint, byte>();
        _trackIsrcs = new Dictionary<uint, string>();

        var    lastSession     = 0;
        var    lastPoint       = 0;
        uint   trackSequence   = 0;
        ushort sessionSequence = 0;

        Session currentSession = default;
        var     sessionStarted = false;

        for(var t = 0; t < header.num_track_blocks; t++)
        {
            C2dTrackBlock entry = trackBlocks[t];

            AaruLogging.Debug(MODULE_NAME, "Track block {0}:",        t);
            AaruLogging.Debug(MODULE_NAME, "  block size: {0}",       entry.block_size);
            AaruLogging.Debug(MODULE_NAME, "  first sector: {0}",     entry.first_sector);
            AaruLogging.Debug(MODULE_NAME, "  last sector: {0}",      entry.last_sector);
            AaruLogging.Debug(MODULE_NAME, "  image offset: 0x{0:X}", entry.image_offset);
            AaruLogging.Debug(MODULE_NAME, "  sector size: {0}",      entry.sector_size);
            AaruLogging.Debug(MODULE_NAME, "  flags: 0x{0:X}",        (byte)entry.flags);
            AaruLogging.Debug(MODULE_NAME, "  mode: {0}",             entry.mode);
            AaruLogging.Debug(MODULE_NAME, "  index: {0}",            entry.index);
            AaruLogging.Debug(MODULE_NAME, "  session: {0}",          entry.session);
            AaruLogging.Debug(MODULE_NAME, "  point: {0}",            entry.point);
            AaruLogging.Debug(MODULE_NAME, "  compressed: {0}",       entry.compressed);

            // Compressed tracks are not supported
            if(entry.compressed != 0)
            {
                AaruLogging.Error(MODULE_NAME + ": compressed tracks are not supported");

                return ErrorNumber.NotSupported;
            }

            // Create a new session when session number increases
            if(entry.session > lastSession)
            {
                if(sessionStarted) Sessions.Add(currentSession);

                sessionSequence++;

                currentSession = new Session
                {
                    Sequence   = sessionSequence,
                    StartTrack = uint.MaxValue,
                    EndTrack   = uint.MinValue
                };

                sessionStarted = true;
                lastSession    = entry.session;
                lastPoint      = 0;
            }

            // Create a new track when point number increases
            if(entry.point > lastPoint)
            {
                trackSequence++;
                lastPoint = entry.point;

                // Find track boundaries by scanning forward for all blocks with same point
                uint trackFirstSector = entry.first_sector;
                uint trackLastSector  = entry.last_sector;

                for(int n = t + 1; n < header.num_track_blocks; n++)
                {
                    if(trackBlocks[n].point != entry.point) break;

                    trackLastSector = trackBlocks[n].last_sector;
                }

                // Determine subchannel presence and effective sector size
                TrackSubchannelType subchannelType      = TrackSubchannelType.None;
                uint                effectiveSectorSize = entry.sector_size;

                if(effectiveSectorSize == 2448)
                {
                    subchannelType      = TrackSubchannelType.RawInterleaved;
                    effectiveSectorSize = 2352;

                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);
                }

                // Determine track type and sector sizes
                TrackType trackType;
                int       rawBytesPerSector;
                int       bytesPerSector;

                switch(entry.mode)
                {
                    case C2dMode.Audio:
                    case C2dMode.Audio2:
                        trackType         = TrackType.Audio;
                        rawBytesPerSector = (int)effectiveSectorSize;
                        bytesPerSector    = 2352;

                        break;
                    case C2dMode.Mode1:
                        trackType         = TrackType.CdMode1;
                        rawBytesPerSector = (int)effectiveSectorSize;
                        bytesPerSector    = 2048;

                        break;
                    case C2dMode.Mode2:
                        switch(effectiveSectorSize)
                        {
                            case 2048:
                                trackType         = TrackType.CdMode2Form1;
                                rawBytesPerSector = 2048;
                                bytesPerSector    = 2048;

                                break;
                            case 2324:
                                trackType         = TrackType.CdMode2Form2;
                                rawBytesPerSector = 2324;
                                bytesPerSector    = 2324;

                                break;
                            case 2336:
                                trackType         = TrackType.CdMode2Formless;
                                rawBytesPerSector = 2336;
                                bytesPerSector    = 2336;

                                break;
                            case 2352:
                                trackType         = TrackType.CdMode2Formless;
                                rawBytesPerSector = 2352;
                                bytesPerSector    = 2336;

                                break;
                            default:
                                AaruLogging.Error(MODULE_NAME + ": unknown sector size {0} for Mode 2 track",
                                                  effectiveSectorSize);

                                return ErrorNumber.InvalidArgument;
                        }

                        break;
                    default:
                        AaruLogging.Error(MODULE_NAME + ": unknown track mode {0}", entry.mode);

                        return ErrorNumber.InvalidArgument;
                }

                uint trackLength = trackLastSector - trackFirstSector + 1;
                var  pregap      = (ulong)(entry.point == 1 ? PREGAP_LENGTH : 0);

                var track = new Track
                {
                    BytesPerSector    = bytesPerSector,
                    RawBytesPerSector = rawBytesPerSector,
                    Sequence          = trackSequence,
                    Session           = sessionSequence,
                    StartSector       = trackFirstSector,
                    EndSector         = trackLastSector,
                    Type              = trackType,
                    SubchannelType    = subchannelType,
                    Description       = string.Format(Localization.Track_0, trackSequence),
                    File              = imageFilter.Filename,
                    FileType          = "BINARY",
                    Filter            = imageFilter,
                    FileOffset        = entry.image_offset,
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
                else
                {
                    track.Indexes = new Dictionary<ushort, int>
                    {
                        [1] = (int)trackFirstSector
                    };
                }

                // Update session bounds
                if(trackSequence < currentSession.StartTrack)
                {
                    currentSession.StartTrack  = trackSequence;
                    currentSession.StartSector = trackFirstSector;
                }

                if(trackSequence > currentSession.EndTrack)
                {
                    currentSession.EndTrack  = trackSequence;
                    currentSession.EndSector = trackLastSector;
                }

                // Update maximum sector size
                if(_imageInfo.SectorSize < (uint)rawBytesPerSector) _imageInfo.SectorSize = (uint)rawBytesPerSector;

                // Build partition
                var partition = new Partition
                {
                    Sequence    = trackSequence - 1,
                    Start       = trackFirstSector,
                    Length      = trackLength,
                    Offset      = entry.image_offset,
                    Size        = trackLength * (ulong)entry.sector_size,
                    Description = string.Format(Localization.Track_0, trackSequence),
                    Type        = trackType.Humanize()
                };

                // Store C2D flags for this track
                _trackFlags[trackSequence] = (byte)entry.flags;

                // Store ISRC if present
                string isrc = Encoding.ASCII.GetString(entry.isrc).TrimEnd('\0');

                if(!string.IsNullOrWhiteSpace(isrc)) _trackIsrcs[trackSequence] = isrc;

                Tracks.Add(track);
                Partitions.Add(partition);
            }
            else if(entry.index > 1)
            {
                // Index block before any track
                if(Tracks.Count == 0) return ErrorNumber.InvalidArgument;

                // Additional index for an existing track
                Track lastTrack = Tracks[^1];
                lastTrack.Indexes[entry.index] = (int)entry.first_sector;
                Tracks[^1]                     = lastTrack;
            }
        }

        // Add the last session
        if(sessionStarted) Sessions.Add(currentSession);

        // Set up image info
        _imageInfo.Sectors              = Tracks.Count > 0 ? Tracks[^1].EndSector + 1 : 0;
        _imageInfo.ImageSize            = (ulong)imageFilter.DataForkLength;
        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MetadataMediaType    = MetadataMediaType.OpticalDisc;
        _imageInfo.Application          = "WinOnCD";

        if(!string.IsNullOrWhiteSpace(description)) _imageInfo.Comments = description;

        if(header.has_upc_ean != 0)
        {
            _mcn                    = Encoding.ASCII.GetString(header.upc_ean).TrimEnd('\0');
            _imageInfo.MediaBarcode = _mcn;
            _imageInfo.ReadableMediaTags.Add(MediaTagType.CD_MCN);
        }

        // Read CD-Text if present
        if(header.size_cdtext > 0)
        {
            long cdTextOffset = header.offset_tracks + header.num_track_blocks * Marshal.SizeOf<C2dTrackBlock>();

            stream.Seek(cdTextOffset, SeekOrigin.Begin);
            _cdtext = new byte[header.size_cdtext];
            stream.EnsureRead(_cdtext, 0, (int)header.size_cdtext);
            _imageInfo.ReadableMediaTags.Add(MediaTagType.CD_TEXT);
        }

        if(_trackIsrcs.Count > 0 && !_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdTrackIsrc))
            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackIsrc);

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