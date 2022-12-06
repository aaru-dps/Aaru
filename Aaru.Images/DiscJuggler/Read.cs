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
//     Reads DiscJuggler disc images.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Decoders.CD;
using Aaru.Helpers;
using Session = Aaru.CommonTypes.Structs.Session;

namespace Aaru.DiscImages
{
    public sealed partial class DiscJuggler
    {
        /// <inheritdoc />
        public bool Open(IFilter imageFilter)
        {
            _imageStream = imageFilter.GetDataForkStream();

            // Read size of image descriptor
            _imageStream.Seek(-4, SeekOrigin.End);
            byte[] dscLenB = new byte[4];
            _imageStream.Read(dscLenB, 0, 4);
            int dscLen = BitConverter.ToInt32(dscLenB, 0);

            if(dscLen >= _imageStream.Length)
                return false;

            byte[] descriptor = new byte[dscLen];
            _imageStream.Seek(-dscLen, SeekOrigin.End);
            _imageStream.Read(descriptor, 0, dscLen);

            // Sessions
            if(descriptor[0] > 99 ||
               descriptor[0] == 0)
                return false;

            int position = 1;

            ushort sessionSequence = 0;
            Sessions    = new List<Session>();
            Tracks      = new List<Track>();
            Partitions  = new List<Partition>();
            _offsetMap  = new Dictionary<uint, ulong>();
            _trackFlags = new Dictionary<uint, byte>();
            ushort mediumType;
            byte   maxS = descriptor[0];

            AaruConsole.DebugWriteLine("DiscJuggler plugin", "maxS = {0}", maxS);
            uint  lastSessionTrack = 0;
            ulong currentOffset    = 0;

            // Read sessions
            for(byte s = 0; s <= maxS; s++)
            {
                AaruConsole.DebugWriteLine("DiscJuggler plugin", "s = {0}", s);

                // Seems all sessions start with this data
                if(descriptor[position + 0]  != 0x00 ||
                   descriptor[position + 2]  != 0x00 ||
                   descriptor[position + 3]  != 0x00 ||
                   descriptor[position + 4]  != 0x00 ||
                   descriptor[position + 5]  != 0x00 ||
                   descriptor[position + 6]  != 0x00 ||
                   descriptor[position + 7]  != 0x00 ||
                   descriptor[position + 8]  != 0x00 ||
                   descriptor[position + 9]  != 0x01 ||
                   descriptor[position + 10] != 0x00 ||
                   descriptor[position + 11] != 0x00 ||
                   descriptor[position + 12] != 0x00 ||
                   descriptor[position + 13] != 0xFF ||
                   descriptor[position + 14] != 0xFF)
                {
                    bool nextFound = false;

                    // But on generated (not dumped) image, it can have some data between last written session and
                    // next open one, so depend on if we already got a track
                    while(position + 16 < descriptor.Length)
                    {
                        if(descriptor[position + 0]  != 0x00 ||
                           descriptor[position + 2]  != 0x00 ||
                           descriptor[position + 3]  != 0x00 ||
                           descriptor[position + 4]  != 0x00 ||
                           descriptor[position + 5]  != 0x00 ||
                           descriptor[position + 6]  != 0x00 ||
                           descriptor[position + 7]  != 0x00 ||
                           descriptor[position + 8]  != 0x00 ||
                           descriptor[position + 9]  != 0x01 ||
                           descriptor[position + 10] != 0x00 ||
                           descriptor[position + 11] != 0x00 ||
                           descriptor[position + 12] != 0x00 ||
                           descriptor[position + 13] != 0xFF ||
                           descriptor[position + 14] != 0xFF)
                        {
                            position++;

                            continue;
                        }

                        nextFound = true;

                        break;
                    }

                    if(!nextFound)
                        return Tracks.Count > 0;

                    position += 15;

                    break;
                }

                // Too many tracks
                if(descriptor[position + 1] > 99)
                    return false;

                byte maxT = descriptor[position + 1];
                AaruConsole.DebugWriteLine("DiscJuggler plugin", "maxT = {0}", maxT);

                sessionSequence++;

                var session = new Session
                {
                    SessionSequence = sessionSequence,
                    EndTrack        = uint.MinValue,
                    StartTrack      = uint.MaxValue
                };

                position += 15;
                bool addedATrack = false;

                // Read track
                for(byte t = 0; t < maxT; t++)
                {
                    AaruConsole.DebugWriteLine("DiscJuggler plugin", "t = {0}", t);
                    var track = new Track();

                    // Skip unknown
                    position += 16;

                    byte[] trackFilenameB = new byte[descriptor[position]];
                    position++;
                    Array.Copy(descriptor, position, trackFilenameB, 0, trackFilenameB.Length);
                    position        += trackFilenameB.Length;
                    track.TrackFile =  Path.GetFileName(Encoding.Default.GetString(trackFilenameB));
                    AaruConsole.DebugWriteLine("DiscJuggler plugin", "\tfilename = {0}", track.TrackFile);

                    // Skip unknown
                    position += 29;

                    mediumType =  BitConverter.ToUInt16(descriptor, position);
                    position   += 2;
                    AaruConsole.DebugWriteLine("DiscJuggler plugin", "\tmediumType = {0}", mediumType);

                    // Read indices
                    ushort maxI = BitConverter.ToUInt16(descriptor, position);
                    position += 2;
                    AaruConsole.DebugWriteLine("DiscJuggler plugin", "\tmaxI = {0}", maxI);

                    // This is not really the index position, but, the index length, go figure
                    for(ushort i = 0; i < maxI; i++)
                    {
                        int index = BitConverter.ToInt32(descriptor, position);
                        track.Indexes.Add(i, index);
                        position += 4;
                        AaruConsole.DebugWriteLine("DiscJuggler plugin", "\tindex[{1}] = {0}", index, i);
                    }

                    // Read CD-Text
                    uint maxC = BitConverter.ToUInt32(descriptor, position);
                    position += 4;
                    AaruConsole.DebugWriteLine("DiscJuggler plugin", "\tmaxC = {0}", maxC);

                    for(uint c = 0; c < maxC; c++)
                    {
                        for(int cb = 0; cb < 18; cb++)
                        {
                            int bLen = descriptor[position];
                            position++;
                            AaruConsole.DebugWriteLine("DiscJuggler plugin", "\tc[{1}][{2}].Length = {0}", bLen, c, cb);

                            if(bLen <= 0)
                                continue;

                            byte[] textBlk = new byte[bLen];
                            Array.Copy(descriptor, position, textBlk, 0, bLen);
                            position += bLen;

                            // Track title
                            if(cb != 10)
                                continue;

                            track.TrackDescription = Encoding.Default.GetString(textBlk, 0, bLen);

                            AaruConsole.DebugWriteLine("DiscJuggler plugin", "\tTrack title = {0}",
                                                       track.TrackDescription);
                        }
                    }

                    position += 2;
                    uint trackMode = BitConverter.ToUInt32(descriptor, position);
                    AaruConsole.DebugWriteLine("DiscJuggler plugin", "\ttrackMode = {0}", trackMode);
                    position += 4;

                    // Skip unknown
                    position += 4;

                    session.SessionSequence = (ushort)(BitConverter.ToUInt32(descriptor, position) + 1);
                    track.TrackSession      = session.SessionSequence;
                    AaruConsole.DebugWriteLine("DiscJuggler plugin", "\tsession = {0}", session.SessionSequence);
                    position            += 4;
                    track.TrackSequence =  BitConverter.ToUInt32(descriptor, position) + lastSessionTrack + 1;

                    AaruConsole.DebugWriteLine("DiscJuggler plugin", "\ttrack = {1} + {2} + 1 = {0}",
                                               track.TrackSequence, BitConverter.ToUInt32(descriptor, position),
                                               lastSessionTrack);

                    // There's always an index 0 in the image
                    track.TrackPregap = (ulong)track.Indexes[0];

                    if(track.TrackSequence == 1)
                        track.Indexes[0] = -150;

                    if(track.Indexes[0] == 0)
                        track.Indexes.Remove(0);

                    position               += 4;
                    track.TrackStartSector =  BitConverter.ToUInt32(descriptor, position);
                    AaruConsole.DebugWriteLine("DiscJuggler plugin", "\ttrackStart = {0}", track.TrackStartSector);
                    position += 4;
                    uint trackLen = BitConverter.ToUInt32(descriptor, position);

                    // DiscJuggler counts the first track pregap start as 0 instead of -150, we need to adjust appropriately
                    if(track.TrackStartSector == 0)
                        trackLen -= 150;
                    else
                        track.TrackStartSector -= 150;

                    int leftLen = (int)trackLen;

                    // Convert index length to index position
                    foreach(KeyValuePair<ushort, int> idx in track.Indexes.Reverse())
                    {
                        leftLen -= idx.Value;

                        if(idx.Key             == 0 &&
                           track.TrackSequence == 1)
                            continue;

                        track.Indexes[idx.Key] = (int)track.TrackStartSector + leftLen;
                    }

                    track.TrackEndSector = track.TrackStartSector + trackLen - 1;
                    AaruConsole.DebugWriteLine("DiscJuggler plugin", "\ttrackEnd = {0}", track.TrackEndSector);
                    position += 4;

                    if(track.TrackSequence > session.EndTrack)
                    {
                        session.EndTrack  = track.TrackSequence;
                        session.EndSector = track.TrackEndSector;
                    }

                    if(track.TrackSequence < session.StartTrack)
                    {
                        session.StartTrack  = track.TrackSequence;
                        session.StartSector = track.TrackStartSector;
                    }

                    // Skip unknown
                    position += 16;

                    uint readMode = BitConverter.ToUInt32(descriptor, position);
                    AaruConsole.DebugWriteLine("DiscJuggler plugin", "\treadMode = {0}", readMode);
                    position += 4;
                    uint trackCtl = BitConverter.ToUInt32(descriptor, position);
                    AaruConsole.DebugWriteLine("DiscJuggler plugin", "\ttrackCtl = {0}", trackCtl);
                    position += 4;

                    // Skip unknown
                    position += 9;

                    byte[] isrc = new byte[12];
                    Array.Copy(descriptor, position, isrc, 0, 12);
                    AaruConsole.DebugWriteLine("DiscJuggler plugin", "\tisrc = {0}", StringHandlers.CToString(isrc));
                    position += 12;
                    uint isrcValid = BitConverter.ToUInt32(descriptor, position);
                    AaruConsole.DebugWriteLine("DiscJuggler plugin", "\tisrc_valid = {0}", isrcValid);
                    position += 4;

                    // Skip unknown
                    position += 87;

                    byte sessionType = descriptor[position];
                    AaruConsole.DebugWriteLine("DiscJuggler plugin", "\tsessionType = {0}", sessionType);
                    position++;

                    // Skip unknown
                    position += 5;

                    byte trackFollows = descriptor[position];
                    AaruConsole.DebugWriteLine("DiscJuggler plugin", "\ttrackFollows = {0}", trackFollows);
                    position += 2;

                    uint endAddress = BitConverter.ToUInt32(descriptor, position);
                    AaruConsole.DebugWriteLine("DiscJuggler plugin", "\tendAddress = {0}", endAddress);
                    position += 4;

                    // As to skip the lead-in
                    bool firstTrack = currentOffset == 0;

                    track.TrackSubchannelType = TrackSubchannelType.None;

                    switch(trackMode)
                    {
                        // Audio
                        case 0:
                            if(_imageInfo.SectorSize < 2352)
                                _imageInfo.SectorSize = 2352;

                            track.TrackType              = TrackType.Audio;
                            track.TrackBytesPerSector    = 2352;
                            track.TrackRawBytesPerSector = 2352;

                            switch(readMode)
                            {
                                case 2:
                                    if(firstTrack)
                                        currentOffset += 150 * (ulong)track.TrackRawBytesPerSector;

                                    track.TrackFileOffset =  currentOffset;
                                    currentOffset         += trackLen * (ulong)track.TrackRawBytesPerSector;

                                    break;
                                case 3:
                                    if(firstTrack)
                                        currentOffset += 150 * (ulong)(track.TrackRawBytesPerSector + 16);

                                    track.TrackFileOffset       = currentOffset;
                                    track.TrackSubchannelFile   = track.TrackFile;
                                    track.TrackSubchannelOffset = currentOffset;
                                    track.TrackSubchannelType   = TrackSubchannelType.Q16Interleaved;

                                    currentOffset += trackLen * (ulong)(track.TrackRawBytesPerSector + 16);

                                    break;
                                case 4:
                                    if(firstTrack)
                                        currentOffset += 150 * (ulong)(track.TrackRawBytesPerSector + 96);

                                    track.TrackFileOffset       = currentOffset;
                                    track.TrackSubchannelFile   = track.TrackFile;
                                    track.TrackSubchannelOffset = currentOffset;
                                    track.TrackSubchannelType   = TrackSubchannelType.RawInterleaved;

                                    currentOffset += trackLen * (ulong)(track.TrackRawBytesPerSector + 96);

                                    break;
                                default: throw new ImageNotSupportedException($"Unknown read mode {readMode}");
                            }

                            break;

                        // Mode 1 or DVD
                        case 1:
                            if(_imageInfo.SectorSize < 2048)
                                _imageInfo.SectorSize = 2048;

                            track.TrackType           = TrackType.CdMode1;
                            track.TrackBytesPerSector = 2048;

                            switch(readMode)
                            {
                                case 0:
                                    track.TrackRawBytesPerSector = 2048;

                                    if(firstTrack)
                                        currentOffset += 150 * (ulong)track.TrackRawBytesPerSector;

                                    track.TrackFileOffset =  currentOffset;
                                    currentOffset         += trackLen * (ulong)track.TrackRawBytesPerSector;

                                    break;
                                case 1:
                                    throw
                                        new ImageNotSupportedException($"Invalid read mode {readMode} for this track");
                                case 2:
                                    track.TrackRawBytesPerSector =  2352;
                                    currentOffset                += trackLen * (ulong)track.TrackRawBytesPerSector;

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEcc))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEcc);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccP))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccP);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccQ))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccQ);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);

                                    break;
                                case 3:
                                    track.TrackRawBytesPerSector = 2352;

                                    if(firstTrack)
                                        currentOffset += 150 * (ulong)(track.TrackRawBytesPerSector + 16);

                                    track.TrackFileOffset       = currentOffset;
                                    track.TrackSubchannelFile   = track.TrackFile;
                                    track.TrackSubchannelOffset = currentOffset;
                                    track.TrackSubchannelType   = TrackSubchannelType.Q16Interleaved;

                                    currentOffset += trackLen * (ulong)(track.TrackRawBytesPerSector + 16);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEcc))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEcc);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccP))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccP);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccQ))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccQ);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);

                                    break;
                                case 4:
                                    track.TrackRawBytesPerSector = 2352;

                                    if(firstTrack)
                                        currentOffset += 150 * (ulong)(track.TrackRawBytesPerSector + 96);

                                    track.TrackFileOffset       = currentOffset;
                                    track.TrackSubchannelFile   = track.TrackFile;
                                    track.TrackSubchannelOffset = currentOffset;
                                    track.TrackSubchannelType   = TrackSubchannelType.RawInterleaved;

                                    currentOffset += trackLen * (ulong)(track.TrackRawBytesPerSector + 96);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEcc))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEcc);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccP))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccP);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccQ))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccQ);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);

                                    break;
                                default: throw new ImageNotSupportedException($"Unknown read mode {readMode}");
                            }

                            break;

                        // Mode 2
                        case 2:
                            if(_imageInfo.SectorSize < 2336)
                                _imageInfo.SectorSize = 2336;

                            track.TrackType           = TrackType.CdMode2Formless;
                            track.TrackBytesPerSector = 2336;

                            switch(readMode)
                            {
                                case 0:
                                    throw
                                        new ImageNotSupportedException($"Invalid read mode {readMode} for this track");
                                case 1:
                                    track.TrackRawBytesPerSector = 2336;

                                    if(firstTrack)
                                        currentOffset += 150 * (ulong)track.TrackRawBytesPerSector;

                                    track.TrackFileOffset =  currentOffset;
                                    currentOffset         += trackLen * (ulong)track.TrackRawBytesPerSector;

                                    break;
                                case 2:
                                    track.TrackRawBytesPerSector =  2352;
                                    currentOffset                += trackLen * (ulong)track.TrackRawBytesPerSector;

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                                    break;
                                case 3:
                                    track.TrackRawBytesPerSector = 2352;

                                    if(firstTrack)
                                        currentOffset += 150 * (ulong)(track.TrackRawBytesPerSector + 16);

                                    track.TrackFileOffset       = currentOffset;
                                    track.TrackSubchannelFile   = track.TrackFile;
                                    track.TrackSubchannelOffset = currentOffset;
                                    track.TrackSubchannelType   = TrackSubchannelType.Q16Interleaved;

                                    currentOffset += trackLen * (ulong)(track.TrackRawBytesPerSector + 16);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                                    break;
                                case 4:
                                    track.TrackRawBytesPerSector = 2352;

                                    if(firstTrack)
                                        currentOffset += 150 * (ulong)(track.TrackRawBytesPerSector + 96);

                                    track.TrackFileOffset       = currentOffset;
                                    track.TrackSubchannelFile   = track.TrackFile;
                                    track.TrackSubchannelOffset = currentOffset;
                                    track.TrackSubchannelType   = TrackSubchannelType.RawInterleaved;

                                    currentOffset += trackLen * (ulong)(track.TrackRawBytesPerSector + 96);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                                    break;
                                default: throw new ImageNotSupportedException($"Unknown read mode {readMode}");
                            }

                            break;
                        default: throw new ImageNotSupportedException($"Unknown track mode {trackMode}");
                    }

                    track.TrackFile   = imageFilter.GetFilename();
                    track.TrackFilter = imageFilter;

                    if(track.TrackSubchannelType != TrackSubchannelType.None)
                    {
                        track.TrackSubchannelFile   = imageFilter.GetFilename();
                        track.TrackSubchannelFilter = imageFilter;

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);
                    }

                    var partition = new Partition
                    {
                        Description = track.TrackDescription,
                        Length      = trackLen,
                        Sequence    = track.TrackSequence,
                        Offset      = track.TrackFileOffset,
                        Start       = track.TrackStartSector,
                        Type        = track.TrackType.ToString()
                    };

                    if(track.TrackSequence > 1 &&
                       track.TrackPregap   > 0)
                    {
                        partition.Start  += track.TrackPregap;
                        partition.Length -= track.TrackPregap;
                    }

                    partition.Size = partition.Length * (ulong)track.TrackBytesPerSector;

                    if(track.TrackEndSector + 1 > _imageInfo.Sectors)
                        _imageInfo.Sectors = track.TrackEndSector + 1;

                    Partitions.Add(partition);
                    _offsetMap.Add(track.TrackSequence, track.TrackStartSector);
                    Tracks.Add(track);
                    _trackFlags.Add(track.TrackSequence, (byte)(trackCtl & 0xFF));
                    addedATrack = true;
                }

                if(!addedATrack)
                    continue;

                lastSessionTrack = session.EndTrack;
                Sessions.Add(session);
                AaruConsole.DebugWriteLine("DiscJuggler plugin", "session.StartTrack = {0}", session.StartTrack);
                AaruConsole.DebugWriteLine("DiscJuggler plugin", "session.StartSector = {0}", session.StartSector);
                AaruConsole.DebugWriteLine("DiscJuggler plugin", "session.EndTrack = {0}", session.EndTrack);
                AaruConsole.DebugWriteLine("DiscJuggler plugin", "session.EndSector = {0}", session.EndSector);

                AaruConsole.DebugWriteLine("DiscJuggler plugin", "session.SessionSequence = {0}",
                                           session.SessionSequence);
            }

            // Skip unknown
            position += 16;

            AaruConsole.DebugWriteLine("DiscJuggler plugin", "Current position = {0}", position);
            byte[] filenameB = new byte[descriptor[position]];
            position++;
            Array.Copy(descriptor, position, filenameB, 0, filenameB.Length);
            position += filenameB.Length;
            string filename = Path.GetFileName(Encoding.Default.GetString(filenameB));
            AaruConsole.DebugWriteLine("DiscJuggler plugin", "filename = {0}", filename);

            // Skip unknown
            position += 29;

            mediumType =  BitConverter.ToUInt16(descriptor, position);
            position   += 2;
            AaruConsole.DebugWriteLine("DiscJuggler plugin", "mediumType = {0}", mediumType);

            uint discSize = BitConverter.ToUInt32(descriptor, position);
            position += 4;
            AaruConsole.DebugWriteLine("DiscJuggler plugin", "discSize = {0}", discSize);

            byte[] volidB = new byte[descriptor[position]];
            position++;
            Array.Copy(descriptor, position, volidB, 0, volidB.Length);
            position += volidB.Length;
            string volid = Path.GetFileName(Encoding.Default.GetString(volidB));
            AaruConsole.DebugWriteLine("DiscJuggler plugin", "volid = {0}", volid);

            // Skip unknown
            position += 9;

            byte[] mcn = new byte[13];
            Array.Copy(descriptor, position, mcn, 0, 13);
            AaruConsole.DebugWriteLine("DiscJuggler plugin", "mcn = {0}", StringHandlers.CToString(mcn));
            position += 13;
            uint mcnValid = BitConverter.ToUInt32(descriptor, position);
            AaruConsole.DebugWriteLine("DiscJuggler plugin", "mcn_valid = {0}", mcnValid);
            position += 4;

            uint cdtextLen = BitConverter.ToUInt32(descriptor, position);
            AaruConsole.DebugWriteLine("DiscJuggler plugin", "cdtextLen = {0}", cdtextLen);
            position += 4;

            if(cdtextLen > 0)
            {
                _cdtext = new byte[cdtextLen];
                Array.Copy(descriptor, position, _cdtext, 0, cdtextLen);
                position += (int)cdtextLen;
                _imageInfo.ReadableMediaTags.Add(MediaTagType.CD_TEXT);
            }

            // Skip unknown
            position += 12;

            AaruConsole.DebugWriteLine("DiscJuggler plugin", "End position = {0}", position);

            _imageInfo.MediaType = DecodeCdiMediumType(mediumType);

            if(_imageInfo.MediaType == MediaType.CDROM)
            {
                bool data       = false;
                bool mode2      = false;
                bool firstaudio = false;
                bool firstdata  = false;
                bool audio      = false;

                for(int i = 0; i < Tracks.Count; i++)
                {
                    // First track is audio
                    firstaudio |= i == 0 && Tracks[i].TrackType == TrackType.Audio;

                    // First track is data
                    firstdata |= i == 0 && Tracks[i].TrackType != TrackType.Audio;

                    // Any non first track is data
                    data |= i != 0 && Tracks[i].TrackType != TrackType.Audio;

                    // Any non first track is audio
                    audio |= i != 0 && Tracks[i].TrackType == TrackType.Audio;

                    switch(Tracks[i].TrackType)
                    {
                        case TrackType.CdMode2Form1:
                        case TrackType.CdMode2Form2:
                        case TrackType.CdMode2Formless:
                            mode2 = true;

                            break;
                    }
                }

                if(!data &&
                   !firstdata)
                    _imageInfo.MediaType = MediaType.CDDA;
                else if(firstaudio         &&
                        data               &&
                        Sessions.Count > 1 &&
                        mode2)
                    _imageInfo.MediaType = MediaType.CDPLUS;
                else if((firstdata && audio) || mode2)
                    _imageInfo.MediaType = MediaType.CDROMXA;
                else if(!audio)
                    _imageInfo.MediaType = MediaType.CDROM;
                else
                    _imageInfo.MediaType = MediaType.CD;
            }

            if(_trackFlags.Count > 0 &&
               !_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdTrackFlags))
                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackFlags);

            _imageInfo.Application          = "DiscJuggler";
            _imageInfo.ImageSize            = (ulong)imageFilter.GetDataForkLength();
            _imageInfo.CreationTime         = imageFilter.GetCreationTime();
            _imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            _imageInfo.XmlMediaType         = XmlMediaType.OpticalDisc;

            _sectorBuilder = new SectorBuilder();

            _isCd = mediumType == 152;

            if(_isCd)
                return true;

            foreach(Track track in Tracks)
            {
                track.TrackPregap = 0;
                track.Indexes?.Clear();
            }

            _imageInfo.ReadableSectorTags.Remove(SectorTagType.CdSectorSync);
            _imageInfo.ReadableSectorTags.Remove(SectorTagType.CdSectorHeader);
            _imageInfo.ReadableSectorTags.Remove(SectorTagType.CdSectorSubHeader);
            _imageInfo.ReadableSectorTags.Remove(SectorTagType.CdSectorEcc);
            _imageInfo.ReadableSectorTags.Remove(SectorTagType.CdSectorEccP);
            _imageInfo.ReadableSectorTags.Remove(SectorTagType.CdSectorEccQ);
            _imageInfo.ReadableSectorTags.Remove(SectorTagType.CdSectorEdc);
            _imageInfo.ReadableSectorTags.Remove(SectorTagType.CdTrackFlags);
            _imageInfo.ReadableSectorTags.Remove(SectorTagType.CdTrackIsrc);

            return true;
        }

        /// <inheritdoc />
        public byte[] ReadDiskTag(MediaTagType tag)
        {
            switch(tag)
            {
                case MediaTagType.CD_TEXT:
                {
                    if(_cdtext        != null &&
                       _cdtext.Length > 0)
                        return _cdtext;

                    throw new FeatureNotPresentImageException("Image does not contain CD-TEXT information.");
                }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
            }
        }

        /// <inheritdoc />
        public byte[] ReadSector(ulong sectorAddress) => ReadSectors(sectorAddress, 1);

        /// <inheritdoc />
        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag) => ReadSectorsTag(sectorAddress, 1, tag);

        /// <inheritdoc />
        public byte[] ReadSector(ulong sectorAddress, uint track) => ReadSectors(sectorAddress, 1, track);

        /// <inheritdoc />
        public byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag) =>
            ReadSectorsTag(sectorAddress, 1, track, tag);

        /// <inheritdoc />
        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap where sectorAddress     >= kvp.Value
                                                     from track in Tracks where track.TrackSequence == kvp.Key
                                                     where sectorAddress                                 - kvp.Value <
                                                           track.TrackEndSector - track.TrackStartSector + 1 select kvp)
                return ReadSectors(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), $"Sector address {sectorAddress} not found");
        }

        /// <inheritdoc />
        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap where sectorAddress     >= kvp.Value
                                                     from track in Tracks where track.TrackSequence == kvp.Key
                                                     where sectorAddress                                 - kvp.Value <
                                                           track.TrackEndSector - track.TrackStartSector + 1 select kvp)
                return ReadSectorsTag(sectorAddress - kvp.Value, length, kvp.Key, tag);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), $"Sector address {sectorAddress} not found");
        }

        /// <inheritdoc />
        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            var aaruTrack = new Track
            {
                TrackSequence = 0
            };

            foreach(Track linqTrack in Tracks.Where(linqTrack => linqTrack.TrackSequence == track))
            {
                aaruTrack = linqTrack;

                break;
            }

            if(aaruTrack is null)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > aaruTrack.TrackEndSector - aaruTrack.TrackStartSector + 1)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({aaruTrack.TrackEndSector - aaruTrack.TrackStartSector + 1}), won't cross tracks");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;
            bool mode2 = false;

            switch(aaruTrack.TrackType)
            {
                case TrackType.Audio:
                {
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 0;

                    break;
                }
                case TrackType.CdMode1:
                    if(aaruTrack.TrackRawBytesPerSector == 2352)
                    {
                        sectorOffset = 16;
                        sectorSize   = 2048;
                        sectorSkip   = 288;
                    }
                    else
                    {
                        sectorOffset = 0;
                        sectorSize   = 2048;
                        sectorSkip   = 0;
                    }

                    break;
                case TrackType.CdMode2Formless:
                {
                    mode2        = true;
                    sectorOffset = 0;
                    sectorSize   = (uint)aaruTrack.TrackRawBytesPerSector;
                    sectorSkip   = 0;
                }

                    break;
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            switch(aaruTrack.TrackSubchannelType)
            {
                case TrackSubchannelType.None:
                    sectorSkip += 0;

                    break;
                case TrackSubchannelType.Q16Interleaved:
                    sectorSkip += 16;

                    break;
                case TrackSubchannelType.RawInterleaved:
                    sectorSkip += 96;

                    break;
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported subchannel type");
            }

            byte[] buffer = new byte[sectorSize * length];

            _imageStream.
                Seek((long)(aaruTrack.TrackFileOffset + (sectorAddress * (sectorOffset + sectorSize + sectorSkip))),
                     SeekOrigin.Begin);

            if(mode2)
            {
                var mode2Ms = new MemoryStream((int)(sectorSize * length));

                buffer = new byte[(aaruTrack.TrackRawBytesPerSector + sectorSkip) * length];
                _imageStream.Read(buffer, 0, buffer.Length);

                for(int i = 0; i < length; i++)
                {
                    byte[] sector = new byte[aaruTrack.TrackRawBytesPerSector];

                    Array.Copy(buffer, (aaruTrack.TrackRawBytesPerSector + sectorSkip) * i, sector, 0,
                               aaruTrack.TrackRawBytesPerSector);

                    sector = Sector.GetUserDataFromMode2(sector);
                    mode2Ms.Write(sector, 0, sector.Length);
                }

                buffer = mode2Ms.ToArray();
            }
            else if(sectorOffset == 0 &&
                    sectorSkip   == 0)
                _imageStream.Read(buffer, 0, buffer.Length);
            else
                for(int i = 0; i < length; i++)
                {
                    byte[] sector = new byte[sectorSize];
                    _imageStream.Seek(sectorOffset, SeekOrigin.Current);
                    _imageStream.Read(sector, 0, sector.Length);
                    _imageStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }

            return buffer;
        }

        /// <inheritdoc />
        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            if(tag == SectorTagType.CdTrackFlags)
                track = (uint)sectorAddress;

            var aaruTrack = new Track
            {
                TrackSequence = 0
            };

            foreach(Track linqTrack in Tracks.Where(linqTrack => linqTrack.TrackSequence == track))
            {
                aaruTrack = linqTrack;

                break;
            }

            if(aaruTrack is null)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > aaruTrack.TrackEndSector - aaruTrack.TrackStartSector + 1)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({aaruTrack.TrackEndSector - aaruTrack.TrackStartSector + 1}), won't cross tracks");

            if(aaruTrack.TrackType == TrackType.Data)
                throw new ArgumentException("Unsupported tag requested", nameof(tag));

            switch(tag)
            {
                case SectorTagType.CdSectorEcc:
                case SectorTagType.CdSectorEccP:
                case SectorTagType.CdSectorEccQ:
                case SectorTagType.CdSectorEdc:
                case SectorTagType.CdSectorHeader:
                case SectorTagType.CdSectorSubchannel:
                case SectorTagType.CdSectorSubHeader:
                case SectorTagType.CdSectorSync: break;
                case SectorTagType.CdTrackFlags:
                    if(_trackFlags.TryGetValue(track, out byte flag))
                        return new[]
                        {
                            flag
                        };

                    throw new ArgumentException("Unsupported tag requested", nameof(tag));
                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
            }

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch(aaruTrack.TrackType)
            {
                case TrackType.CdMode1:
                    if(aaruTrack.TrackRawBytesPerSector != 2352)
                        throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));

                    switch(tag)
                    {
                        case SectorTagType.CdSectorSync:
                        {
                            sectorOffset = 0;
                            sectorSize   = 12;
                            sectorSkip   = 2340;

                            break;
                        }
                        case SectorTagType.CdSectorHeader:
                        {
                            sectorOffset = 12;
                            sectorSize   = 4;
                            sectorSkip   = 2336;

                            break;
                        }
                        case SectorTagType.CdSectorSubHeader:
                            throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                        case SectorTagType.CdSectorEcc:
                        {
                            sectorOffset = 2076;
                            sectorSize   = 276;
                            sectorSkip   = 0;

                            break;
                        }
                        case SectorTagType.CdSectorEccP:
                        {
                            sectorOffset = 2076;
                            sectorSize   = 172;
                            sectorSkip   = 104;

                            break;
                        }
                        case SectorTagType.CdSectorEccQ:
                        {
                            sectorOffset = 2248;
                            sectorSize   = 104;
                            sectorSkip   = 0;

                            break;
                        }
                        case SectorTagType.CdSectorEdc:
                        {
                            sectorOffset = 2064;
                            sectorSize   = 4;
                            sectorSkip   = 284;

                            break;
                        }
                        case SectorTagType.CdSectorSubchannel:
                            switch(aaruTrack.TrackSubchannelType)
                            {
                                case TrackSubchannelType.None:
                                    throw new ArgumentException("Unsupported tag requested for this track",
                                                                nameof(tag));
                                case TrackSubchannelType.Q16Interleaved:

                                    sectorSize = 16;

                                    break;
                                default:
                                    sectorSize = 96;

                                    break;
                            }

                            sectorOffset = 2352;
                            sectorSkip   = 0;

                            break;
                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                case TrackType.CdMode2Formless:
                    if(aaruTrack.TrackRawBytesPerSector != 2352)
                        throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));

                {
                    switch(tag)
                    {
                        case SectorTagType.CdSectorSync:
                        case SectorTagType.CdSectorHeader:
                        case SectorTagType.CdSectorEcc:
                        case SectorTagType.CdSectorEccP:
                        case SectorTagType.CdSectorEccQ:
                            throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                        case SectorTagType.CdSectorSubHeader:
                        {
                            sectorOffset = 0;
                            sectorSize   = 8;
                            sectorSkip   = 2328;

                            break;
                        }
                        case SectorTagType.CdSectorEdc:
                        {
                            sectorOffset = 2332;
                            sectorSize   = 4;
                            sectorSkip   = 0;

                            break;
                        }
                        case SectorTagType.CdSectorSubchannel:
                            switch(aaruTrack.TrackSubchannelType)
                            {
                                case TrackSubchannelType.None:
                                    throw new ArgumentException("Unsupported tag requested for this track",
                                                                nameof(tag));
                                case TrackSubchannelType.Q16Interleaved:
                                    sectorSize = 16;

                                    break;
                                default:
                                    sectorSize = 96;

                                    break;
                            }

                            sectorOffset = 2352;
                            sectorSkip   = 0;

                            break;
                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                }
                case TrackType.Audio:
                {
                    switch(tag)
                    {
                        case SectorTagType.CdSectorSubchannel:
                            switch(aaruTrack.TrackSubchannelType)
                            {
                                case TrackSubchannelType.None:
                                    throw new ArgumentException("Unsupported tag requested for this track",
                                                                nameof(tag));
                                case TrackSubchannelType.Q16Interleaved:
                                    sectorSize = 16;

                                    break;
                                default:
                                    sectorSize = 96;

                                    break;
                            }

                            sectorOffset = 2352;
                            sectorSkip   = 0;

                            break;
                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            switch(aaruTrack.TrackSubchannelType)
            {
                case TrackSubchannelType.None:
                    sectorSkip += 0;

                    break;
                case TrackSubchannelType.Q16Interleaved:
                    sectorSkip += 16;

                    break;
                case TrackSubchannelType.RawInterleaved:
                    sectorSkip += 96;

                    break;
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported subchannel type");
            }

            byte[] buffer = new byte[sectorSize * length];

            _imageStream.
                Seek((long)(aaruTrack.TrackFileOffset + (sectorAddress * (sectorOffset + sectorSize + sectorSkip))),
                     SeekOrigin.Begin);

            if(sectorOffset == 0 &&
               sectorSkip   == 0)
                _imageStream.Read(buffer, 0, buffer.Length);
            else
                for(int i = 0; i < length; i++)
                {
                    byte[] sector = new byte[sectorSize];
                    _imageStream.Seek(sectorOffset, SeekOrigin.Current);
                    _imageStream.Read(sector, 0, sector.Length);
                    _imageStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }

            if(aaruTrack.TrackSubchannelType == TrackSubchannelType.Q16Interleaved &&
               tag                           == SectorTagType.CdSectorSubchannel)
                return Subchannel.ConvertQToRaw(buffer);

            return buffer;
        }

        /// <inheritdoc />
        public byte[] ReadSectorLong(ulong sectorAddress) => ReadSectorsLong(sectorAddress, 1);

        /// <inheritdoc />
        public byte[] ReadSectorLong(ulong sectorAddress, uint track) => ReadSectorsLong(sectorAddress, 1, track);

        /// <inheritdoc />
        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap where sectorAddress     >= kvp.Value
                                                     from track in Tracks where track.TrackSequence == kvp.Key
                                                     where sectorAddress                                 - kvp.Value <
                                                           track.TrackEndSector - track.TrackStartSector + 1 select kvp)
                return ReadSectorsLong(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), $"Sector address {sectorAddress} not found");
        }

        /// <inheritdoc />
        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            if(!_isCd)
                return ReadSectors(sectorAddress, length, track);

            var aaruTrack = new Track
            {
                TrackSequence = 0
            };

            foreach(Track linqTrack in Tracks.Where(linqTrack => linqTrack.TrackSequence == track))
            {
                aaruTrack = linqTrack;

                break;
            }

            if(aaruTrack is null)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > aaruTrack.TrackEndSector - aaruTrack.TrackStartSector + 1)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({aaruTrack.TrackEndSector - aaruTrack.TrackStartSector + 1}), won't cross tracks");

            uint sectorSize = (uint)aaruTrack.TrackRawBytesPerSector;
            uint sectorSkip = 0;

            switch(aaruTrack.TrackSubchannelType)
            {
                case TrackSubchannelType.None:
                    sectorSkip += 0;

                    break;
                case TrackSubchannelType.Q16Interleaved:
                    sectorSkip += 16;

                    break;
                case TrackSubchannelType.RawInterleaved:
                    sectorSkip += 96;

                    break;
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported subchannel type");
            }

            byte[] buffer = new byte[sectorSize * length];

            _imageStream.Seek((long)(aaruTrack.TrackFileOffset + (sectorAddress * (sectorSize + sectorSkip))),
                              SeekOrigin.Begin);

            if(sectorSkip == 0)
                _imageStream.Read(buffer, 0, buffer.Length);
            else
                for(int i = 0; i < length; i++)
                {
                    byte[] sector = new byte[sectorSize];
                    _imageStream.Read(sector, 0, sector.Length);
                    _imageStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }

            switch(aaruTrack.TrackType)
            {
                case TrackType.CdMode1 when aaruTrack.TrackRawBytesPerSector == 2048:
                {
                    byte[] fullSector = new byte[2352];
                    byte[] fullBuffer = new byte[2352 * length];

                    for(uint i = 0; i < length; i++)
                    {
                        Array.Copy(buffer, i * 2048, fullSector, 16, 2048);
                        _sectorBuilder.ReconstructPrefix(ref fullSector, TrackType.CdMode1, (long)(sectorAddress + i));
                        _sectorBuilder.ReconstructEcc(ref fullSector, TrackType.CdMode1);
                        Array.Copy(fullSector, 0, fullBuffer, i * 2352, 2352);
                    }

                    buffer = fullBuffer;

                    break;
                }
                case TrackType.CdMode2Formless when aaruTrack.TrackRawBytesPerSector == 2336:
                {
                    byte[] fullSector = new byte[2352];
                    byte[] fullBuffer = new byte[2352 * length];

                    for(uint i = 0; i < length; i++)
                    {
                        _sectorBuilder.ReconstructPrefix(ref fullSector, TrackType.CdMode2Formless,
                                                         (long)(sectorAddress + i));

                        Array.Copy(buffer, i                    * 2336, fullSector, 16, 2336);
                        Array.Copy(fullSector, 0, fullBuffer, i * 2352, 2352);
                    }

                    buffer = fullBuffer;

                    break;
                }
            }

            return buffer;
        }

        /// <inheritdoc />
        public List<Track> GetSessionTracks(Session session)
        {
            if(Sessions.Contains(session))
                return GetSessionTracks(session.SessionSequence);

            throw new ImageNotSupportedException("Session does not exist in disc image");
        }

        /// <inheritdoc />
        public List<Track> GetSessionTracks(ushort session) =>
            Tracks.Where(track => track.TrackSession == session).ToList();
    }
}