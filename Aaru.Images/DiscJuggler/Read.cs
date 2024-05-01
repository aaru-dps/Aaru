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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Decoders.CD;
using Aaru.Helpers;
using Session = Aaru.CommonTypes.Structs.Session;

namespace Aaru.Images;

public sealed partial class DiscJuggler
{
#region IOpticalMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        _imageStream = imageFilter.GetDataForkStream();

        // Read size of image descriptor
        _imageStream.Seek(-4, SeekOrigin.End);
        var dscLenB = new byte[4];
        _imageStream.EnsureRead(dscLenB, 0, 4);
        var dscLen = BitConverter.ToInt32(dscLenB, 0);

        if(dscLen >= _imageStream.Length) return ErrorNumber.InvalidArgument;

        var descriptor = new byte[dscLen];
        _imageStream.Seek(-dscLen, SeekOrigin.End);
        _imageStream.EnsureRead(descriptor, 0, dscLen);

        // Sessions
        if(descriptor[0] > 99 || descriptor[0] == 0) return ErrorNumber.InvalidArgument;

        var position = 1;

        ushort sessionSequence = 0;
        Sessions    = [];
        Tracks      = [];
        Partitions  = [];
        _offsetMap  = new Dictionary<uint, ulong>();
        _trackFlags = new Dictionary<uint, byte>();
        ushort mediumType;
        byte   maxS = descriptor[0];

        AaruConsole.DebugWriteLine(MODULE_NAME, "maxS = {0}", maxS);
        uint  lastSessionTrack = 0;
        ulong currentOffset    = 0;

        // Read sessions
        for(byte s = 0; s <= maxS; s++)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, "s = {0}", s);

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
                var nextFound = false;

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

                if(!nextFound) return Tracks.Count > 0 ? ErrorNumber.NoError : ErrorNumber.InvalidArgument;

                position += 15;

                break;
            }

            // Too many tracks
            if(descriptor[position + 1] > 99) return ErrorNumber.InvalidArgument;

            byte maxT = descriptor[position + 1];
            AaruConsole.DebugWriteLine(MODULE_NAME, "maxT = {0}", maxT);

            sessionSequence++;

            var session = new Session
            {
                Sequence   = sessionSequence,
                EndTrack   = uint.MinValue,
                StartTrack = uint.MaxValue
            };

            position += 15;
            var addedATrack = false;

            // Read track
            for(byte t = 0; t < maxT; t++)
            {
                AaruConsole.DebugWriteLine(MODULE_NAME, "t = {0}", t);
                var track = new Track();

                // Skip unknown
                position += 16;

                var trackFilenameB = new byte[descriptor[position]];
                position++;
                Array.Copy(descriptor, position, trackFilenameB, 0, trackFilenameB.Length);
                position   += trackFilenameB.Length;
                track.File =  Path.GetFileName(Encoding.Default.GetString(trackFilenameB));
                AaruConsole.DebugWriteLine(MODULE_NAME, "\tfilename = {0}", track.File);

                // Skip unknown
                position += 29;

                mediumType =  BitConverter.ToUInt16(descriptor, position);
                position   += 2;
                AaruConsole.DebugWriteLine(MODULE_NAME, "\tmediumType = {0}", mediumType);

                // Read indices
                var maxI = BitConverter.ToUInt16(descriptor, position);
                position += 2;
                AaruConsole.DebugWriteLine(MODULE_NAME, "\tmaxI = {0}", maxI);

                // This is not really the index position, but, the index length, go figure
                for(ushort i = 0; i < maxI; i++)
                {
                    var index = BitConverter.ToInt32(descriptor, position);
                    track.Indexes.Add(i, index);
                    position += 4;
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\tindex[{1}] = {0}", index, i);
                }

                // Read CD-Text
                var maxC = BitConverter.ToUInt32(descriptor, position);
                position += 4;
                AaruConsole.DebugWriteLine(MODULE_NAME, "\tmaxC = {0}", maxC);

                for(uint c = 0; c < maxC; c++)
                {
                    for(var cb = 0; cb < 18; cb++)
                    {
                        int bLen = descriptor[position];
                        position++;
                        AaruConsole.DebugWriteLine(MODULE_NAME, "\tc[{1}][{2}].Length = {0}", bLen, c, cb);

                        if(bLen <= 0) continue;

                        var textBlk = new byte[bLen];
                        Array.Copy(descriptor, position, textBlk, 0, bLen);
                        position += bLen;

                        // Track title
                        if(cb != 10) continue;

                        track.Description = Encoding.Default.GetString(textBlk, 0, bLen);

                        AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Track_title_0, track.Description);
                    }
                }

                position += 2;
                var trackMode = BitConverter.ToUInt32(descriptor, position);
                AaruConsole.DebugWriteLine(MODULE_NAME, "\ttrackMode = {0}", trackMode);
                position += 4;

                // Skip unknown
                position += 4;

                session.Sequence = (ushort)(BitConverter.ToUInt32(descriptor, position) + 1);
                track.Session    = session.Sequence;
                AaruConsole.DebugWriteLine(MODULE_NAME, "\tsession = {0}", session.Sequence);
                position       += 4;
                track.Sequence =  BitConverter.ToUInt32(descriptor, position) + lastSessionTrack + 1;

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "\ttrack = {1} + {2} + 1 = {0}",
                                           track.Sequence,
                                           BitConverter.ToUInt32(descriptor, position),
                                           lastSessionTrack);

                // There's always an index 0 in the image
                track.Pregap = (ulong)track.Indexes[0];

                if(track.Sequence == 1) track.Indexes[0] = -150;

                if(track.Indexes[0] == 0) track.Indexes.Remove(0);

                position          += 4;
                track.StartSector =  BitConverter.ToUInt32(descriptor, position);
                AaruConsole.DebugWriteLine(MODULE_NAME, "\ttrackStart = {0}", track.StartSector);
                position += 4;
                var trackLen = BitConverter.ToUInt32(descriptor, position);

                // DiscJuggler counts the first track pregap start as 0 instead of -150, we need to adjust appropriately
                if(track.StartSector == 0)
                    trackLen -= 150;
                else
                    track.StartSector -= 150;

                var leftLen = (int)trackLen;

                // Convert index length to index position
                foreach(KeyValuePair<ushort, int> idx in track.Indexes.Reverse())
                {
                    leftLen -= idx.Value;

                    if(idx.Key == 0 && track.Sequence == 1) continue;

                    track.Indexes[idx.Key] = (int)track.StartSector + leftLen;
                }

                track.EndSector = track.StartSector + trackLen - 1;
                AaruConsole.DebugWriteLine(MODULE_NAME, "\ttrackEnd = {0}", track.EndSector);
                position += 4;

                if(track.Sequence > session.EndTrack)
                {
                    session.EndTrack  = track.Sequence;
                    session.EndSector = track.EndSector;
                }

                if(track.Sequence < session.StartTrack)
                {
                    session.StartTrack  = track.Sequence;
                    session.StartSector = track.StartSector;
                }

                // Skip unknown
                position += 16;

                var readMode = BitConverter.ToUInt32(descriptor, position);
                AaruConsole.DebugWriteLine(MODULE_NAME, "\treadMode = {0}", readMode);
                position += 4;
                var trackCtl = BitConverter.ToUInt32(descriptor, position);
                AaruConsole.DebugWriteLine(MODULE_NAME, "\ttrackCtl = {0}", trackCtl);
                position += 4;

                // Skip unknown
                position += 9;

                var isrc = new byte[12];
                Array.Copy(descriptor, position, isrc, 0, 12);
                AaruConsole.DebugWriteLine(MODULE_NAME, "\tisrc = {0}", StringHandlers.CToString(isrc));
                position += 12;
                var isrcValid = BitConverter.ToUInt32(descriptor, position);
                AaruConsole.DebugWriteLine(MODULE_NAME, "\tisrc_valid = {0}", isrcValid);
                position += 4;

                // Skip unknown
                position += 87;

                byte sessionType = descriptor[position];
                AaruConsole.DebugWriteLine(MODULE_NAME, "\tsessionType = {0}", sessionType);
                position++;

                // Skip unknown
                position += 5;

                byte trackFollows = descriptor[position];
                AaruConsole.DebugWriteLine(MODULE_NAME, "\ttrackFollows = {0}", trackFollows);
                position += 2;

                var endAddress = BitConverter.ToUInt32(descriptor, position);
                AaruConsole.DebugWriteLine(MODULE_NAME, "\tendAddress = {0}", endAddress);
                position += 4;

                // As to skip the lead-in
                bool firstTrack = currentOffset == 0;

                track.SubchannelType = TrackSubchannelType.None;

                switch(trackMode)
                {
                    // Audio
                    case 0:
                        if(_imageInfo.SectorSize < 2352) _imageInfo.SectorSize = 2352;

                        track.Type              = TrackType.Audio;
                        track.BytesPerSector    = 2352;
                        track.RawBytesPerSector = 2352;

                        switch(readMode)
                        {
                            case 2:
                                if(firstTrack) currentOffset += 150 * (ulong)track.RawBytesPerSector;

                                track.FileOffset =  currentOffset;
                                currentOffset    += trackLen * (ulong)track.RawBytesPerSector;

                                break;
                            case 3:
                                if(firstTrack) currentOffset += 150 * (ulong)(track.RawBytesPerSector + 16);

                                track.FileOffset       = currentOffset;
                                track.SubchannelFile   = track.File;
                                track.SubchannelOffset = currentOffset;
                                track.SubchannelType   = TrackSubchannelType.Q16Interleaved;

                                currentOffset += trackLen * (ulong)(track.RawBytesPerSector + 16);

                                break;
                            case 4:
                                if(firstTrack) currentOffset += 150 * (ulong)(track.RawBytesPerSector + 96);

                                track.FileOffset       = currentOffset;
                                track.SubchannelFile   = track.File;
                                track.SubchannelOffset = currentOffset;
                                track.SubchannelType   = TrackSubchannelType.RawInterleaved;

                                currentOffset += trackLen * (ulong)(track.RawBytesPerSector + 96);

                                break;
                            default:
                                AaruConsole.ErrorWriteLine(string.Format(Localization.Unknown_read_mode_0, readMode));

                                return ErrorNumber.InvalidArgument;
                        }

                        break;

                    // Mode 1 or DVD
                    case 1:
                        if(_imageInfo.SectorSize < 2048) _imageInfo.SectorSize = 2048;

                        track.Type           = TrackType.CdMode1;
                        track.BytesPerSector = 2048;

                        switch(readMode)
                        {
                            case 0:
                                track.RawBytesPerSector = 2048;

                                if(firstTrack) currentOffset += 150 * (ulong)track.RawBytesPerSector;

                                track.FileOffset =  currentOffset;
                                currentOffset    += trackLen * (ulong)track.RawBytesPerSector;

                                break;
                            case 1:
                                AaruConsole.ErrorWriteLine(string.Format(Localization
                                                                            .Invalid_read_mode_0_for_this_track,
                                                                         readMode));

                                return ErrorNumber.InvalidArgument;
                            case 2:
                                track.RawBytesPerSector =  2352;
                                currentOffset           += trackLen * (ulong)track.RawBytesPerSector;

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
                                track.RawBytesPerSector = 2352;

                                if(firstTrack) currentOffset += 150 * (ulong)(track.RawBytesPerSector + 16);

                                track.FileOffset       = currentOffset;
                                track.SubchannelFile   = track.File;
                                track.SubchannelOffset = currentOffset;
                                track.SubchannelType   = TrackSubchannelType.Q16Interleaved;

                                currentOffset += trackLen * (ulong)(track.RawBytesPerSector + 16);

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
                                track.RawBytesPerSector = 2352;

                                if(firstTrack) currentOffset += 150 * (ulong)(track.RawBytesPerSector + 96);

                                track.FileOffset       = currentOffset;
                                track.SubchannelFile   = track.File;
                                track.SubchannelOffset = currentOffset;
                                track.SubchannelType   = TrackSubchannelType.RawInterleaved;

                                currentOffset += trackLen * (ulong)(track.RawBytesPerSector + 96);

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
                            default:
                                AaruConsole.ErrorWriteLine(string.Format(Localization.Unknown_read_mode_0, readMode));

                                return ErrorNumber.InvalidArgument;
                        }

                        break;

                    // Mode 2
                    case 2:
                        if(_imageInfo.SectorSize < 2336) _imageInfo.SectorSize = 2336;

                        track.Type           = TrackType.CdMode2Formless;
                        track.BytesPerSector = 2336;

                        switch(readMode)
                        {
                            case 0:
                                AaruConsole.ErrorWriteLine(string.Format(Localization
                                                                            .Invalid_read_mode_0_for_this_track,
                                                                         readMode));

                                return ErrorNumber.InvalidArgument;
                            case 1:
                                track.RawBytesPerSector = 2336;

                                if(firstTrack) currentOffset += 150 * (ulong)track.RawBytesPerSector;

                                track.FileOffset =  currentOffset;
                                currentOffset    += trackLen * (ulong)track.RawBytesPerSector;

                                break;
                            case 2:
                                track.RawBytesPerSector =  2352;
                                currentOffset           += trackLen * (ulong)track.RawBytesPerSector;

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                                break;
                            case 3:
                                track.RawBytesPerSector = 2352;

                                if(firstTrack) currentOffset += 150 * (ulong)(track.RawBytesPerSector + 16);

                                track.FileOffset       = currentOffset;
                                track.SubchannelFile   = track.File;
                                track.SubchannelOffset = currentOffset;
                                track.SubchannelType   = TrackSubchannelType.Q16Interleaved;

                                currentOffset += trackLen * (ulong)(track.RawBytesPerSector + 16);

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                                break;
                            case 4:
                                track.RawBytesPerSector = 2352;

                                if(firstTrack) currentOffset += 150 * (ulong)(track.RawBytesPerSector + 96);

                                track.FileOffset       = currentOffset;
                                track.SubchannelFile   = track.File;
                                track.SubchannelOffset = currentOffset;
                                track.SubchannelType   = TrackSubchannelType.RawInterleaved;

                                currentOffset += trackLen * (ulong)(track.RawBytesPerSector + 96);

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                                break;
                            default:
                                AaruConsole.ErrorWriteLine(string.Format(Localization.Unknown_read_mode_0, readMode));

                                return ErrorNumber.InvalidArgument;
                        }

                        break;
                    default:
                        AaruConsole.ErrorWriteLine(string.Format(Localization.Unknown_track_mode_0, trackMode));

                        return ErrorNumber.InvalidArgument;
                }

                track.File   = imageFilter.Filename;
                track.Filter = imageFilter;

                if(track.SubchannelType != TrackSubchannelType.None)
                {
                    track.SubchannelFile   = imageFilter.Filename;
                    track.SubchannelFilter = imageFilter;

                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);
                }

                var partition = new Partition
                {
                    Description = track.Description,
                    Length      = trackLen,
                    Sequence    = track.Sequence,
                    Offset      = track.FileOffset,
                    Start       = track.StartSector,
                    Type        = track.Type.ToString()
                };

                if(track.Sequence > 1 && track.Pregap > 0)
                {
                    partition.Start  += track.Pregap;
                    partition.Length -= track.Pregap;
                }

                partition.Size = partition.Length * (ulong)track.BytesPerSector;

                if(track.EndSector + 1 > _imageInfo.Sectors) _imageInfo.Sectors = track.EndSector + 1;

                Partitions.Add(partition);
                _offsetMap.Add(track.Sequence, track.StartSector);
                Tracks.Add(track);
                _trackFlags.Add(track.Sequence, (byte)(trackCtl & 0xFF));
                addedATrack = true;
            }

            if(!addedATrack) continue;

            lastSessionTrack = session.EndTrack;
            Sessions.Add(session);
            AaruConsole.DebugWriteLine(MODULE_NAME, "session.StartTrack = {0}",  session.StartTrack);
            AaruConsole.DebugWriteLine(MODULE_NAME, "session.StartSector = {0}", session.StartSector);
            AaruConsole.DebugWriteLine(MODULE_NAME, "session.EndTrack = {0}",    session.EndTrack);
            AaruConsole.DebugWriteLine(MODULE_NAME, "session.EndSector = {0}",   session.EndSector);

            AaruConsole.DebugWriteLine(MODULE_NAME, "session.Sequence = {0}", session.Sequence);
        }

        // Skip unknown
        position += 16;

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Current_position_equals_0, position);
        var filenameB = new byte[descriptor[position]];
        position++;
        Array.Copy(descriptor, position, filenameB, 0, filenameB.Length);
        position += filenameB.Length;
        string filename = Path.GetFileName(Encoding.Default.GetString(filenameB));
        AaruConsole.DebugWriteLine(MODULE_NAME, "filename = {0}", filename);

        // Skip unknown
        position += 29;

        mediumType =  BitConverter.ToUInt16(descriptor, position);
        position   += 2;
        AaruConsole.DebugWriteLine(MODULE_NAME, "mediumType = {0}", mediumType);

        var discSize = BitConverter.ToUInt32(descriptor, position);
        position += 4;
        AaruConsole.DebugWriteLine(MODULE_NAME, "discSize = {0}", discSize);

        var volidB = new byte[descriptor[position]];
        position++;
        Array.Copy(descriptor, position, volidB, 0, volidB.Length);
        position += volidB.Length;
        string volid = Path.GetFileName(Encoding.Default.GetString(volidB));
        AaruConsole.DebugWriteLine(MODULE_NAME, "volid = {0}", volid);

        // Skip unknown
        position += 9;

        var mcn = new byte[13];
        Array.Copy(descriptor, position, mcn, 0, 13);
        AaruConsole.DebugWriteLine(MODULE_NAME, "mcn = {0}", StringHandlers.CToString(mcn));
        position += 13;
        var mcnValid = BitConverter.ToUInt32(descriptor, position);
        AaruConsole.DebugWriteLine(MODULE_NAME, "mcn_valid = {0}", mcnValid);
        position += 4;

        var cdtextLen = BitConverter.ToUInt32(descriptor, position);
        AaruConsole.DebugWriteLine(MODULE_NAME, "cdtextLen = {0}", cdtextLen);
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

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.End_position_equals_0, position);

        _imageInfo.MediaType = DecodeCdiMediumType(mediumType);

        if(_imageInfo.MediaType == MediaType.CDROM)
        {
            var data       = false;
            var mode2      = false;
            var firstaudio = false;
            var firstdata  = false;
            var audio      = false;

            for(var i = 0; i < Tracks.Count; i++)
            {
                // First track is audio
                firstaudio |= i == 0 && Tracks[i].Type == TrackType.Audio;

                // First track is data
                firstdata |= i == 0 && Tracks[i].Type != TrackType.Audio;

                // Any non first track is data
                data |= i != 0 && Tracks[i].Type != TrackType.Audio;

                // Any non first track is audio
                audio |= i != 0 && Tracks[i].Type == TrackType.Audio;

                switch(Tracks[i].Type)
                {
                    case TrackType.CdMode2Form1:
                    case TrackType.CdMode2Form2:
                    case TrackType.CdMode2Formless:
                        mode2 = true;

                        break;
                }
            }

            if(!data && !firstdata)
                _imageInfo.MediaType = MediaType.CDDA;
            else if(firstaudio && data && Sessions.Count > 1 && mode2)
                _imageInfo.MediaType = MediaType.CDPLUS;
            else if(firstdata && audio || mode2)
                _imageInfo.MediaType = MediaType.CDROMXA;
            else if(!audio)
                _imageInfo.MediaType = MediaType.CDROM;
            else
                _imageInfo.MediaType = MediaType.CD;
        }

        if(_trackFlags.Count > 0 && !_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdTrackFlags))
            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackFlags);

        _imageInfo.Application          = "DiscJuggler";
        _imageInfo.ImageSize            = (ulong)imageFilter.DataForkLength;
        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MetadataMediaType    = MetadataMediaType.OpticalDisc;

        _sectorBuilder = new SectorBuilder();

        _isCd = mediumType == 152;

        if(_isCd) return ErrorNumber.NoError;

        foreach(Track track in Tracks)
        {
            track.Pregap = 0;
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

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadMediaTag(MediaTagType tag, out byte[] buffer)
    {
        buffer = null;

        switch(tag)
        {
            case MediaTagType.CD_TEXT:
            {
                if(_cdtext is { Length: > 0 }) buffer = _cdtext?.Clone() as byte[];

                return buffer != null ? ErrorNumber.NoError : ErrorNumber.NoData;
            }
            default:
                return ErrorNumber.NotSupported;
        }
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer) => ReadSectors(sectorAddress, 1, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, SectorTagType tag, out byte[] buffer) =>
        ReadSectorsTag(sectorAddress, 1, tag, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, uint track, out byte[] buffer) =>
        ReadSectors(sectorAddress, 1, track, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag, out byte[] buffer) =>
        ReadSectorsTag(sectorAddress, 1, track, tag, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap
                                                 where sectorAddress >= kvp.Value
                                                 from track in Tracks
                                                 where track.Sequence == kvp.Key
                                                 where sectorAddress                       - kvp.Value <
                                                       track.EndSector - track.StartSector + 1
                                                 select kvp)
            return ReadSectors(sectorAddress - kvp.Value, length, kvp.Key, out buffer);

        return ErrorNumber.SectorNotFound;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap
                                                 where sectorAddress >= kvp.Value
                                                 from track in Tracks
                                                 where track.Sequence == kvp.Key
                                                 where sectorAddress                       - kvp.Value <
                                                       track.EndSector - track.StartSector + 1
                                                 select kvp)
            return ReadSectorsTag(sectorAddress - kvp.Value, length, kvp.Key, tag, out buffer);

        return ErrorNumber.SectorNotFound;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, uint track, out byte[] buffer)
    {
        buffer = null;

        Track aaruTrack = Tracks.FirstOrDefault(linqTrack => linqTrack.Sequence == track);

        if(aaruTrack is null) return ErrorNumber.SectorNotFound;

        if(length + sectorAddress > aaruTrack.EndSector - aaruTrack.StartSector + 1) return ErrorNumber.OutOfRange;

        uint sectorOffset;
        uint sectorSize;
        uint sectorSkip;
        var  mode2 = false;

        switch(aaruTrack.Type)
        {
            case TrackType.Audio:
            {
                sectorOffset = 0;
                sectorSize   = 2352;
                sectorSkip   = 0;

                break;
            }
            case TrackType.CdMode1:
                if(aaruTrack.RawBytesPerSector == 2352)
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
                sectorSize   = (uint)aaruTrack.RawBytesPerSector;
                sectorSkip   = 0;
            }

                break;
            default:
                return ErrorNumber.NotSupported;
        }

        switch(aaruTrack.SubchannelType)
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
            default:
                return ErrorNumber.NotSupported;
        }

        buffer = new byte[sectorSize * length];

        _imageStream.Seek((long)(aaruTrack.FileOffset + sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                          SeekOrigin.Begin);

        if(mode2)
        {
            var mode2Ms = new MemoryStream((int)(sectorSize * length));

            buffer = new byte[(aaruTrack.RawBytesPerSector + sectorSkip) * length];
            _imageStream.EnsureRead(buffer, 0, buffer.Length);

            for(var i = 0; i < length; i++)
            {
                var sector = new byte[aaruTrack.RawBytesPerSector];

                Array.Copy(buffer,
                           (aaruTrack.RawBytesPerSector + sectorSkip) * i,
                           sector,
                           0,
                           aaruTrack.RawBytesPerSector);

                sector = Sector.GetUserDataFromMode2(sector);
                mode2Ms.Write(sector, 0, sector.Length);
            }

            buffer = mode2Ms.ToArray();
        }
        else if(sectorOffset == 0 && sectorSkip == 0)
            _imageStream.EnsureRead(buffer, 0, buffer.Length);
        else
        {
            for(var i = 0; i < length; i++)
            {
                var sector = new byte[sectorSize];
                _imageStream.Seek(sectorOffset, SeekOrigin.Current);
                _imageStream.EnsureRead(sector, 0, sector.Length);
                _imageStream.Seek(sectorSkip, SeekOrigin.Current);
                Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
            }
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong      sectorAddress, uint length, uint track, SectorTagType tag,
                                      out byte[] buffer)
    {
        buffer = null;

        if(tag == SectorTagType.CdTrackFlags) track = (uint)sectorAddress;

        Track aaruTrack = Tracks.FirstOrDefault(linqTrack => linqTrack.Sequence == track);

        if(aaruTrack is null) return ErrorNumber.SectorNotFound;

        if(length + sectorAddress > aaruTrack.EndSector - aaruTrack.StartSector + 1) return ErrorNumber.OutOfRange;

        if(aaruTrack.Type == TrackType.Data) return ErrorNumber.NotSupported;

        switch(tag)
        {
            case SectorTagType.CdSectorEcc:
            case SectorTagType.CdSectorEccP:
            case SectorTagType.CdSectorEccQ:
            case SectorTagType.CdSectorEdc:
            case SectorTagType.CdSectorHeader:
            case SectorTagType.CdSectorSubchannel:
            case SectorTagType.CdSectorSubHeader:
            case SectorTagType.CdSectorSync:
                break;
            case SectorTagType.CdTrackFlags:
                if(!_trackFlags.TryGetValue(track, out byte flag)) return ErrorNumber.NoData;

                buffer = [flag];

                return ErrorNumber.NoError;
            default:
                return ErrorNumber.NotSupported;
        }

        uint sectorOffset;
        uint sectorSize;
        uint sectorSkip;

        switch(aaruTrack.Type)
        {
            case TrackType.CdMode1:
                if(aaruTrack.RawBytesPerSector != 2352) return ErrorNumber.NotSupported;

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
                        return ErrorNumber.NotSupported;
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
                        switch(aaruTrack.SubchannelType)
                        {
                            case TrackSubchannelType.None:
                                return ErrorNumber.NoData;
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
                    default:
                        return ErrorNumber.NotSupported;
                }

                break;
            case TrackType.CdMode2Formless:
                if(aaruTrack.RawBytesPerSector != 2352) return ErrorNumber.NotSupported;

            {
                switch(tag)
                {
                    case SectorTagType.CdSectorSync:
                    case SectorTagType.CdSectorHeader:
                    case SectorTagType.CdSectorEcc:
                    case SectorTagType.CdSectorEccP:
                    case SectorTagType.CdSectorEccQ:
                        return ErrorNumber.NotSupported;
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
                        switch(aaruTrack.SubchannelType)
                        {
                            case TrackSubchannelType.None:
                                return ErrorNumber.NoData;
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
                    default:
                        return ErrorNumber.NotSupported;
                }

                break;
            }
            case TrackType.Audio:
            {
                switch(tag)
                {
                    case SectorTagType.CdSectorSubchannel:
                        switch(aaruTrack.SubchannelType)
                        {
                            case TrackSubchannelType.None:
                                return ErrorNumber.NoData;
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
                    default:
                        return ErrorNumber.NotSupported;
                }

                break;
            }
            default:
                return ErrorNumber.NotSupported;
        }

        switch(aaruTrack.SubchannelType)
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
            default:
                return ErrorNumber.NotSupported;
        }

        buffer = new byte[sectorSize * length];

        _imageStream.Seek((long)(aaruTrack.FileOffset + sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                          SeekOrigin.Begin);

        if(sectorOffset == 0 && sectorSkip == 0)
            _imageStream.EnsureRead(buffer, 0, buffer.Length);
        else
        {
            for(var i = 0; i < length; i++)
            {
                var sector = new byte[sectorSize];
                _imageStream.Seek(sectorOffset, SeekOrigin.Current);
                _imageStream.EnsureRead(sector, 0, sector.Length);
                _imageStream.Seek(sectorSkip, SeekOrigin.Current);
                Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
            }
        }

        if(aaruTrack.SubchannelType == TrackSubchannelType.Q16Interleaved && tag == SectorTagType.CdSectorSubchannel)
            buffer = Subchannel.ConvertQToRaw(buffer);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong sectorAddress, out byte[] buffer) =>
        ReadSectorsLong(sectorAddress, 1, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong sectorAddress, uint track, out byte[] buffer) =>
        ReadSectorsLong(sectorAddress, 1, track, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap
                                                 where sectorAddress >= kvp.Value
                                                 from track in Tracks
                                                 where track.Sequence == kvp.Key
                                                 where sectorAddress                       - kvp.Value <
                                                       track.EndSector - track.StartSector + 1
                                                 select kvp)
            return ReadSectorsLong(sectorAddress - kvp.Value, length, kvp.Key, out buffer);

        return ErrorNumber.SectorNotFound;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong sectorAddress, uint length, uint track, out byte[] buffer)
    {
        buffer = null;

        if(!_isCd) return ReadSectors(sectorAddress, length, track, out buffer);

        Track aaruTrack = Tracks.FirstOrDefault(linqTrack => linqTrack.Sequence == track);

        if(aaruTrack is null) return ErrorNumber.SectorNotFound;

        if(length + sectorAddress > aaruTrack.EndSector - aaruTrack.StartSector + 1) return ErrorNumber.OutOfRange;

        var  sectorSize = (uint)aaruTrack.RawBytesPerSector;
        uint sectorSkip = 0;

        switch(aaruTrack.SubchannelType)
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
            default:
                return ErrorNumber.NotSupported;
        }

        buffer = new byte[sectorSize * length];

        _imageStream.Seek((long)(aaruTrack.FileOffset + sectorAddress * (sectorSize + sectorSkip)), SeekOrigin.Begin);

        if(sectorSkip == 0)
            _imageStream.EnsureRead(buffer, 0, buffer.Length);
        else
        {
            for(var i = 0; i < length; i++)
            {
                var sector = new byte[sectorSize];
                _imageStream.EnsureRead(sector, 0, sector.Length);
                _imageStream.Seek(sectorSkip, SeekOrigin.Current);
                Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
            }
        }

        switch(aaruTrack.Type)
        {
            case TrackType.CdMode1 when aaruTrack.RawBytesPerSector == 2048:
            {
                var fullSector = new byte[2352];
                var fullBuffer = new byte[2352 * length];

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
            case TrackType.CdMode2Formless when aaruTrack.RawBytesPerSector == 2336:
            {
                var fullSector = new byte[2352];
                var fullBuffer = new byte[2352 * length];

                for(uint i = 0; i < length; i++)
                {
                    _sectorBuilder.ReconstructPrefix(ref fullSector,
                                                     TrackType.CdMode2Formless,
                                                     (long)(sectorAddress + i));

                    Array.Copy(buffer,     i * 2336, fullSector, 16,       2336);
                    Array.Copy(fullSector, 0,        fullBuffer, i * 2352, 2352);
                }

                buffer = fullBuffer;

                break;
            }
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public List<Track> GetSessionTracks(Session session) =>
        Sessions.Contains(session) ? GetSessionTracks(session.Sequence) : null;

    /// <inheritdoc />
    public List<Track> GetSessionTracks(ushort session) => Tracks.Where(track => track.Session == session).ToList();

#endregion
}