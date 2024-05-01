// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Write.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Writes Alcohol 120% disc images.
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
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Decoders.CD;
using Aaru.Helpers;
using TrackType = Aaru.CommonTypes.Enums.TrackType;

namespace Aaru.Images;

public sealed partial class Alcohol120
{
#region IWritableOpticalImage Members

    /// <inheritdoc />
    public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                       uint   sectorSize)
    {
        if(!SupportedMediaTypes.Contains(mediaType))
        {
            ErrorMessage = string.Format(Localization.Unsupported_media_format_0, mediaType);

            return false;
        }

        _imageInfo = new ImageInfo
        {
            MediaType  = mediaType,
            SectorSize = sectorSize,
            Sectors    = sectors
        };

        try
        {
            _descriptorStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);

            _imageStream =
                new FileStream(Path.Combine(Path.GetDirectoryName(path) ?? "", Path.GetFileNameWithoutExtension(path)) +
                               ".mdf",
                               FileMode.OpenOrCreate,
                               FileAccess.ReadWrite,
                               FileShare.None);
        }
        catch(IOException ex)
        {
            ErrorMessage = string.Format(Localization.Could_not_create_new_image_file_exception_0, ex.Message);
            AaruConsole.WriteException(ex);

            return false;
        }

        _imageInfo.MediaType = mediaType;

        switch(mediaType)
        {
            case MediaType.CD:
            case MediaType.CDDA:
            case MediaType.CDEG:
            case MediaType.CDG:
            case MediaType.CDI:
            case MediaType.CDMIDI:
            case MediaType.CDMRW:
            case MediaType.CDPLUS:
            case MediaType.CDR:
            case MediaType.CDROM:
            case MediaType.CDROMXA:
            case MediaType.CDRW:
            case MediaType.CDV:
            case MediaType.DTSCD:
            case MediaType.JaguarCD:
            case MediaType.MEGACD:
            case MediaType.PS1CD:
            case MediaType.PS2CD:
            case MediaType.SuperCDROM2:
            case MediaType.SVCD:
            case MediaType.SATURNCD:
            case MediaType.ThreeDO:
            case MediaType.VCD:
            case MediaType.VCDHD:
            case MediaType.NeoGeoCD:
            case MediaType.PCFX:
            case MediaType.CDTV:
            case MediaType.CD32:
            case MediaType.Nuon:
            case MediaType.Playdia:
            case MediaType.Pippin:
            case MediaType.FMTOWNS:
            case MediaType.MilCD:
            case MediaType.VideoNow:
            case MediaType.VideoNowColor:
            case MediaType.VideoNowXp:
            case MediaType.CVD:
                _isDvd = false;

                break;
            default:
                _isDvd = true;

                break;
        }

        _trackFlags = new Dictionary<byte, byte>();

        IsWriting    = true;
        ErrorMessage = null;

        return true;
    }

    /// <inheritdoc />
    public bool WriteMediaTag(byte[] data, MediaTagType tag)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        switch(tag)
        {
            case MediaTagType.CD_FullTOC:
                if(_isDvd)
                {
                    ErrorMessage = string.Format(Localization.Unsupported_media_tag_0_for_medium_type_1,
                                                 tag,
                                                 _imageInfo.MediaType);

                    return false;
                }

                _fullToc = data;

                return true;
            case MediaTagType.DVD_PFI:
                if(!_isDvd)
                {
                    ErrorMessage = string.Format(Localization.Unsupported_media_tag_0_for_medium_type_1,
                                                 tag,
                                                 _imageInfo.MediaType);

                    return false;
                }

                _pfi = data;

                return true;
            case MediaTagType.DVD_DMI:
                if(!_isDvd)
                {
                    ErrorMessage = string.Format(Localization.Unsupported_media_tag_0_for_medium_type_1,
                                                 tag,
                                                 _imageInfo.MediaType);

                    return false;
                }

                _dmi = data;

                return true;
            case MediaTagType.DVD_BCA:
                if(!_isDvd)
                {
                    ErrorMessage = string.Format(Localization.Unsupported_media_tag_0_for_medium_type_1,
                                                 tag,
                                                 _imageInfo.MediaType);

                    return false;
                }

                _bca = data;

                return true;
            default:
                ErrorMessage = string.Format(Localization.Unsupported_media_tag_0, tag);

                return false;
        }
    }

    /// <inheritdoc />
    public bool WriteSector(byte[] data, ulong sectorAddress)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        CommonTypes.Structs.Track track =
            _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.StartSector && sectorAddress <= trk.EndSector);

        if(!_isDvd)
        {
            ErrorMessage = Localization.Cannot_write_non_long_sectors_to_CD_images;

            return false;
        }

        if(track is null)
        {
            ErrorMessage = string.Format(Localization.Cant_find_track_containing_0, sectorAddress);

            return false;
        }

        if(track.BytesPerSector != track.RawBytesPerSector)
        {
            ErrorMessage = Localization.Invalid_write_mode_for_this_sector;

            return false;
        }

        if(data.Length != track.RawBytesPerSector)
        {
            ErrorMessage = Localization.Incorrect_data_size;

            return false;
        }

        _imageStream.Seek((long)(track.FileOffset +
                                 (sectorAddress - track.StartSector) * (ulong)track.RawBytesPerSector),
                          SeekOrigin.Begin);

        _imageStream.Write(data, 0, data.Length);

        return true;
    }

    /// <inheritdoc />
    public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        if(!_isDvd)
        {
            ErrorMessage = Localization.Cannot_write_non_long_sectors_to_CD_images;

            return false;
        }

        CommonTypes.Structs.Track track =
            _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.StartSector && sectorAddress <= trk.EndSector);

        if(track is null)
        {
            ErrorMessage = string.Format(Localization.Cant_find_track_containing_0, sectorAddress);

            return false;
        }

        if(track.BytesPerSector != track.RawBytesPerSector)
        {
            ErrorMessage = Localization.Invalid_write_mode_for_this_sector;

            return false;
        }

        if(sectorAddress + length > track.EndSector + 1)
        {
            ErrorMessage = Localization.Cant_cross_tracks;

            return false;
        }

        if(data.Length % track.RawBytesPerSector != 0)
        {
            ErrorMessage = Localization.Incorrect_data_size;

            return false;
        }

        switch(track.SubchannelType)
        {
            case TrackSubchannelType.None:
                _imageStream.Seek((long)(track.FileOffset +
                                         (sectorAddress - track.StartSector) * (ulong)track.RawBytesPerSector),
                                  SeekOrigin.Begin);

                _imageStream.Write(data, 0, data.Length);

                ErrorMessage = "";

                return true;
            case TrackSubchannelType.Raw:
            case TrackSubchannelType.RawInterleaved:
                _imageStream.Seek((long)(track.FileOffset +
                                         (sectorAddress - track.StartSector) * (ulong)(track.RawBytesPerSector + 96)),
                                  SeekOrigin.Begin);

                for(uint i = 0; i < length; i++)
                {
                    _imageStream.Write(data, (int)(i * track.RawBytesPerSector), track.RawBytesPerSector);
                    _imageStream.Position += 96;
                }

                ErrorMessage = "";

                return true;
            default:
                ErrorMessage = Localization.Invalid_subchannel_mode_for_this_sector;

                return false;
        }
    }

    /// <inheritdoc />
    public bool WriteSectorLong(byte[] data, ulong sectorAddress)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        CommonTypes.Structs.Track track =
            _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.StartSector && sectorAddress <= trk.EndSector);

        if(track is null)
        {
            ErrorMessage = string.Format(Localization.Cant_find_track_containing_0, sectorAddress);

            return false;
        }

        if(data.Length != track.RawBytesPerSector)
        {
            ErrorMessage = Localization.Incorrect_data_size;

            return false;
        }

        var subchannelSize = (uint)(track.SubchannelType != TrackSubchannelType.None ? 96 : 0);

        _imageStream.Seek((long)(track.FileOffset +
                                 (sectorAddress - track.StartSector) *
                                 (ulong)(track.RawBytesPerSector + subchannelSize)),
                          SeekOrigin.Begin);

        _imageStream.Write(data, 0, data.Length);

        return true;
    }

    /// <inheritdoc />
    public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        CommonTypes.Structs.Track track =
            _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.StartSector && sectorAddress <= trk.EndSector);

        if(track is null)
        {
            ErrorMessage = string.Format(Localization.Cant_find_track_containing_0, sectorAddress);

            return false;
        }

        if(sectorAddress + length > track.EndSector + 1)
        {
            ErrorMessage = Localization.Cant_cross_tracks;

            return false;
        }

        if(data.Length % track.RawBytesPerSector != 0)
        {
            ErrorMessage = Localization.Incorrect_data_size;

            return false;
        }

        var subchannelSize = (uint)(track.SubchannelType != TrackSubchannelType.None ? 96 : 0);

        for(uint i = 0; i < length; i++)
        {
            _imageStream.Seek((long)(track.FileOffset +
                                     (i                              + sectorAddress - track.StartSector) *
                                     (ulong)(track.RawBytesPerSector + subchannelSize)),
                              SeekOrigin.Begin);

            _imageStream.Write(data, (int)(i * track.RawBytesPerSector), track.RawBytesPerSector);
        }

        return true;
    }

    /// <inheritdoc />
    public bool SetTracks(List<CommonTypes.Structs.Track> tracks)
    {
        ulong currentDataOffset = 0;

        _writingTracks = [];

        if(!_isDvd)
        {
            CommonTypes.Structs.Track[] tmpTracks = tracks.OrderBy(t => t.Sequence).ToArray();

            for(var i = 1; i < tmpTracks.Length; i++)
            {
                CommonTypes.Structs.Track firstTrackInSession =
                    tracks.FirstOrDefault(t => t.Session == tmpTracks[i].Session);

                if(firstTrackInSession is null) continue;

                if(tmpTracks[i].Sequence == firstTrackInSession.Sequence)
                {
                    if(tmpTracks[i].Sequence > 1) tmpTracks[i].StartSector += 150;

                    continue;
                }

                tmpTracks[i - 1].EndSector += tmpTracks[i].Pregap;
                tmpTracks[i].Pregap        =  0;
                tmpTracks[i].StartSector   =  tmpTracks[i - 1].EndSector + 1;
            }

            tracks = tmpTracks.ToList();
        }

        foreach(CommonTypes.Structs.Track track in tracks.OrderBy(t => t.Sequence))
        {
            uint subchannelSize;

            switch(track.SubchannelType)
            {
                case TrackSubchannelType.None:
                    subchannelSize = 0;

                    break;
                case TrackSubchannelType.Raw:
                case TrackSubchannelType.RawInterleaved:
                    subchannelSize = 96;

                    break;
                default:
                    ErrorMessage = string.Format(Localization.Unsupported_subchannel_type_0, track.SubchannelType);

                    return false;
            }

            track.FileOffset = currentDataOffset;

            currentDataOffset += (ulong)(track.RawBytesPerSector + subchannelSize) *
                                 (track.EndSector                - track.StartSector + 1);

            _writingTracks.Add(track);
        }

        return true;
    }

    /// <inheritdoc />
    public bool Close()
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Image_is_not_opened_for_writing;

            return false;
        }

        byte sessions = byte.MinValue;

        foreach(CommonTypes.Structs.Track t in _writingTracks.Where(t => t.Session > byte.MinValue))
            sessions = (byte)t.Session;

        var header = new Header
        {
            signature        = _alcoholSignature,
            version          = [1, 5],
            type             = MediaTypeToMediumType(_imageInfo.MediaType),
            sessions         = sessions,
            structuresOffset = (uint)(_pfi == null ? 0 : 96),
            sessionOffset    = (uint)(_pfi == null ? 96 : 4196),
            unknown1         = new ushort[2],
            unknown2         = new uint[2],
            unknown3         = new uint[6],
            unknown4         = new uint[3]
        };

        // Alcohol sets this always, Daemon Tool expects this
        header.unknown1[0] = 2;

        _alcSessions    = new Dictionary<int, Session>();
        _alcTracks      = new Dictionary<int, Track>();
        _alcToc         = new Dictionary<int, Dictionary<int, Track>>();
        _writingTracks  = _writingTracks.OrderBy(t => t.Session).ThenBy(t => t.Sequence).ToList();
        _alcTrackExtras = new Dictionary<int, TrackExtra>();
        long currentTrackOffset = header.sessionOffset + Marshal.SizeOf<Session>() * sessions;

        byte[] tmpToc = null;

        if(_fullToc != null)
        {
            byte[] fullTocSize = BigEndianBitConverter.GetBytes((short)_fullToc.Length);
            tmpToc = new byte[_fullToc.Length + 2];
            Array.Copy(_fullToc, 0, tmpToc, 2, _fullToc.Length);
            tmpToc[0] = fullTocSize[0];
            tmpToc[1] = fullTocSize[1];
        }

        FullTOC.CDFullTOC? decodedToc = FullTOC.Decode(tmpToc);

        long currentExtraOffset = currentTrackOffset;
        var  extraCount         = 0;

        for(var i = 1; i <= sessions; i++)
        {
            if(decodedToc.HasValue)
            {
                extraCount += decodedToc.Value.TrackDescriptors.Count(t => t.SessionNumber == i);

                currentExtraOffset += Marshal.SizeOf<Track>() *
                                      decodedToc.Value.TrackDescriptors.Count(t => t.SessionNumber == i);
            }
            else
            {
                currentExtraOffset += Marshal.SizeOf<Track>() * 3;
                extraCount         += 3;

                currentExtraOffset += Marshal.SizeOf<Track>() * _writingTracks.Count(t => t.Session == i);

                extraCount += _writingTracks.Count(t => t.Session == i);

                if(i >= sessions) continue;

                currentExtraOffset += Marshal.SizeOf<Track>() * 2;
                extraCount         += 2;
            }
        }

        long footerOffset = currentExtraOffset + Marshal.SizeOf<TrackExtra>() * extraCount;

        if(_bca != null)
        {
            header.bcaOffset =  (uint)footerOffset;
            footerOffset     += _bca.Length;
        }

        if(_isDvd)
        {
            _alcSessions.Add(1,
                             new Session
                             {
                                 sessionEnd = (int)(_writingTracks[0].EndSector - _writingTracks[0].StartSector + 1),
                                 sessionSequence = 1,
                                 allBlocks = 1,
                                 nonTrackBlocks = 3,
                                 firstTrack = 1,
                                 lastTrack = 1,
                                 trackOffset = 4220
                             });

            footerOffset = 4300;

            if(_bca != null) footerOffset += _bca.Length;

            _alcTracks.Add(1,
                           new Track
                           {
                               mode         = TrackMode.DVD,
                               adrCtl       = 20,
                               point        = 1,
                               extraOffset  = (uint)(_writingTracks[0].EndSector - _writingTracks[0].StartSector + 1),
                               sectorSize   = 2048,
                               files        = 1,
                               footerOffset = (uint)footerOffset,
                               unknown      = new byte[18],
                               unknown2     = new byte[24]
                           });

            _alcToc.Add(1, _alcTracks);
        }
        else
        {
            for(var i = 1; i <= sessions; i++)
            {
                CommonTypes.Structs.Track firstTrack = _writingTracks.First(t => t.Session == i);
                CommonTypes.Structs.Track lastTrack  = _writingTracks.Last(t => t.Session  == i);

                _alcSessions.Add(i,
                                 new Session
                                 {
                                     sessionStart    = (int)firstTrack.StartSector - 150,
                                     sessionEnd      = (int)lastTrack.EndSector    + 1,
                                     sessionSequence = (ushort)i,
                                     allBlocks =
                                         (byte)(decodedToc?.TrackDescriptors.Count(t => t.SessionNumber == i) ??
                                                _writingTracks.Count(t => t.Session == i) + 3),
                                     nonTrackBlocks =
                                         (byte)(decodedToc?.TrackDescriptors.Count(t => t.SessionNumber == i &&
                                                    t.POINT is >= 0xA0 and <= 0xAF) ??
                                                3),
                                     firstTrack  = (ushort)firstTrack.Sequence,
                                     lastTrack   = (ushort)lastTrack.Sequence,
                                     trackOffset = (uint)currentTrackOffset
                                 });

                Dictionary<int, Track> thisSessionTracks = new();
                _trackFlags.TryGetValue((byte)firstTrack.Sequence, out byte firstTrackControl);
                _trackFlags.TryGetValue((byte)lastTrack.Sequence,  out byte lastTrackControl);

                if(firstTrackControl == 0 && firstTrack.Type != TrackType.Audio)
                    firstTrackControl = (byte)CdFlags.DataTrack;

                if(lastTrackControl == 0 && lastTrack.Type != TrackType.Audio)
                    lastTrackControl = (byte)CdFlags.DataTrack;

                (byte minute, byte second, byte frame) leadinPmsf = LbaToMsf(lastTrack.EndSector + 1);

                if(decodedToc?.TrackDescriptors.Any(t => t.SessionNumber == i && t.POINT is >= 0xA0 and <= 0xAF) ==
                   true)
                {
                    foreach(FullTOC.TrackDataDescriptor tocTrk in
                            decodedToc.Value.TrackDescriptors.Where(t => t.SessionNumber == i &&
                                                                         t.POINT is >= 0xA0 and <= 0xAF))
                    {
                        thisSessionTracks.Add(tocTrk.POINT,
                                              new Track
                                              {
                                                  adrCtl      = (byte)((tocTrk.ADR << 4) + tocTrk.CONTROL),
                                                  tno         = tocTrk.TNO,
                                                  point       = tocTrk.POINT,
                                                  min         = tocTrk.Min,
                                                  sec         = tocTrk.Sec,
                                                  frame       = tocTrk.Frame,
                                                  zero        = tocTrk.Zero,
                                                  pmin        = tocTrk.PMIN,
                                                  psec        = tocTrk.PSEC,
                                                  pframe      = tocTrk.PFRAME,
                                                  mode        = TrackMode.NoData,
                                                  unknown     = new byte[18],
                                                  unknown2    = new byte[24],
                                                  extraOffset = (uint)currentExtraOffset
                                              });

                        currentTrackOffset += Marshal.SizeOf<Track>();
                        currentExtraOffset += Marshal.SizeOf<TrackExtra>();
                    }
                }
                else
                {
                    thisSessionTracks.Add(0xA0,
                                          new Track
                                          {
                                              adrCtl   = (byte)((1 << 4) + firstTrackControl),
                                              pmin     = (byte)firstTrack.Sequence,
                                              mode     = TrackMode.NoData,
                                              point    = 0xA0,
                                              unknown  = new byte[18],
                                              unknown2 = new byte[24],
                                              psec = (byte)(_imageInfo.MediaType == MediaType.CDI
                                                                ? 0x10
                                                                : _writingTracks.Any(t => t.Type is TrackType
                                                                       .CdMode2Form1
                                                                 or TrackType.CdMode2Form2
                                                                 or TrackType.CdMode2Formless)
                                                                    ? 0x20
                                                                    : 0),
                                              extraOffset = (uint)currentExtraOffset
                                          });

                    thisSessionTracks.Add(0xA1,
                                          new Track
                                          {
                                              adrCtl      = (byte)((1 << 4) + lastTrackControl),
                                              pmin        = (byte)lastTrack.Sequence,
                                              mode        = TrackMode.NoData,
                                              point       = 0xA1,
                                              unknown     = new byte[18],
                                              unknown2    = new byte[24],
                                              extraOffset = (uint)currentExtraOffset
                                          });

                    thisSessionTracks.Add(0xA2,
                                          new Track
                                          {
                                              adrCtl      = (byte)((1 << 4) + firstTrackControl),
                                              zero        = 0,
                                              pmin        = leadinPmsf.minute,
                                              psec        = leadinPmsf.second,
                                              pframe      = leadinPmsf.frame,
                                              mode        = TrackMode.NoData,
                                              point       = 0xA2,
                                              unknown     = new byte[18],
                                              unknown2    = new byte[24],
                                              extraOffset = (uint)currentExtraOffset
                                          });

                    currentExtraOffset += Marshal.SizeOf<TrackExtra>() * 3;
                    currentTrackOffset += Marshal.SizeOf<Track>()      * 3;
                }

                foreach(CommonTypes.Structs.Track track in _writingTracks.Where(t => t.Session == i)
                                                                         .OrderBy(t => t.Sequence))
                {
                    var alcTrk = new Track();

                    if(decodedToc?.TrackDescriptors.Any(t => t.SessionNumber == i && t.POINT == track.Sequence) == true)
                    {
                        FullTOC.TrackDataDescriptor tocTrk =
                            decodedToc.Value.TrackDescriptors.First(t => t.SessionNumber == i &&
                                                                         t.POINT         == track.Sequence);

                        alcTrk.adrCtl = (byte)((tocTrk.ADR << 4) + tocTrk.CONTROL);
                        alcTrk.tno    = tocTrk.TNO;
                        alcTrk.point  = tocTrk.POINT;
                        alcTrk.min    = tocTrk.Min;
                        alcTrk.sec    = tocTrk.Sec;
                        alcTrk.frame  = tocTrk.Frame;
                        alcTrk.zero   = tocTrk.Zero;
                        alcTrk.pmin   = tocTrk.PMIN;
                        alcTrk.psec   = tocTrk.PSEC;
                        alcTrk.pframe = tocTrk.PFRAME;
                    }
                    else
                    {
                        (byte minute, byte second, byte frame) msf = LbaToMsf((ulong)track.Indexes[1]);
                        _trackFlags.TryGetValue((byte)track.Sequence, out byte trackControl);

                        if(trackControl == 0 && track.Type != TrackType.Audio) trackControl = (byte)CdFlags.DataTrack;

                        alcTrk.adrCtl = (byte)((1 << 4) + trackControl);
                        alcTrk.point  = (byte)track.Sequence;
                        alcTrk.zero   = 0;
                        alcTrk.pmin   = msf.minute;
                        alcTrk.psec   = msf.second;
                        alcTrk.pframe = msf.frame;
                    }

                    alcTrk.mode = TrackTypeToTrackMode(track.Type);

                    alcTrk.subMode = track.SubchannelType != TrackSubchannelType.None
                                         ? SubchannelMode.Interleaved
                                         : SubchannelMode.None;

                    alcTrk.sectorSize = (ushort)(track.RawBytesPerSector +
                                                 (track.SubchannelType != TrackSubchannelType.None ? 96 : 0));

                    alcTrk.startLba     = (uint)(track.StartSector + track.Pregap);
                    alcTrk.startOffset  = track.FileOffset + alcTrk.sectorSize * track.Pregap;
                    alcTrk.files        = 1;
                    alcTrk.extraOffset  = (uint)currentExtraOffset;
                    alcTrk.footerOffset = (uint)footerOffset;

                    if(track.Sequence == firstTrack.Sequence)
                    {
                        alcTrk.startLba    -= (uint)track.Pregap;
                        alcTrk.startOffset -= alcTrk.sectorSize * track.Pregap;
                    }

                    // Alcohol seems to set that for all CD tracks
                    // Daemon Tools expect it to be like this
                    alcTrk.unknown =
                    [
                        0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00
                    ];

                    alcTrk.unknown2 = new byte[24];

                    thisSessionTracks.Add((int)track.Sequence, alcTrk);

                    currentTrackOffset += Marshal.SizeOf<Track>();
                    currentExtraOffset += Marshal.SizeOf<TrackExtra>();

                    var trkExtra = new TrackExtra
                    {
                        sectors = (uint)(track.EndSector - track.StartSector + 1)
                    };

                    if(track.Sequence == firstTrack.Sequence)
                        trkExtra.pregap = 150;

                    // When track mode changes there's a mandatory gap, Alcohol needs it
                    else if(thisSessionTracks.TryGetValue((int)(track.Sequence - 1), out Track previousTrack)      &&
                            _alcTrackExtras.TryGetValue((int)(track.Sequence   - 1), out TrackExtra previousExtra) &&
                            previousTrack.mode != alcTrk.mode)
                    {
                        previousExtra.sectors -= 150;
                        trkExtra.pregap       =  150;
                        _alcTrackExtras.Remove((int)(track.Sequence - 1));
                        _alcTrackExtras.Add((int)(track.Sequence    - 1), previousExtra);
                    }
                    else
                        trkExtra.pregap = 0;

                    _alcTrackExtras.Add((int)track.Sequence, trkExtra);
                }

                if(decodedToc?.TrackDescriptors.Any(t => t.SessionNumber == i && t.POINT >= 0xB0) == true)
                {
                    foreach(FullTOC.TrackDataDescriptor tocTrk in
                            decodedToc.Value.TrackDescriptors.Where(t => t.SessionNumber == i && t.POINT >= 0xB0))
                    {
                        thisSessionTracks.Add(tocTrk.POINT,
                                              new Track
                                              {
                                                  adrCtl      = (byte)((tocTrk.ADR << 4) + tocTrk.CONTROL),
                                                  tno         = tocTrk.TNO,
                                                  point       = tocTrk.POINT,
                                                  min         = tocTrk.Min,
                                                  sec         = tocTrk.Sec,
                                                  frame       = tocTrk.Frame,
                                                  zero        = tocTrk.Zero,
                                                  pmin        = tocTrk.PMIN,
                                                  psec        = tocTrk.PSEC,
                                                  pframe      = tocTrk.PFRAME,
                                                  mode        = TrackMode.NoData,
                                                  unknown     = new byte[18],
                                                  unknown2    = new byte[24],
                                                  extraOffset = (uint)currentExtraOffset
                                              });

                        currentExtraOffset += Marshal.SizeOf<TrackExtra>();
                        currentTrackOffset += Marshal.SizeOf<Track>();
                    }
                }
                else if(i < sessions)
                {
                    (byte minute, byte second, byte frame) leadoutAmsf =
                        LbaToMsf(_writingTracks.First(t => t.Session == i + 1).StartSector - 150);

                    (byte minute, byte second, byte frame) leadoutPmsf =
                        LbaToMsf(_writingTracks.OrderBy(t => t.Session).ThenBy(t => t.Sequence).Last().StartSector);

                    thisSessionTracks.Add(0xB0,
                                          new Track
                                          {
                                              point    = 0xB0,
                                              adrCtl   = 0x50,
                                              zero     = 0,
                                              min      = leadoutAmsf.minute,
                                              sec      = leadoutAmsf.second,
                                              frame    = leadoutAmsf.frame,
                                              pmin     = leadoutPmsf.minute,
                                              psec     = leadoutPmsf.second,
                                              pframe   = leadoutPmsf.frame,
                                              unknown  = new byte[18],
                                              unknown2 = new byte[24]
                                          });

                    thisSessionTracks.Add(0xC0,
                                          new Track
                                          {
                                              point    = 0xC0,
                                              adrCtl   = 0x50,
                                              min      = 128,
                                              pmin     = 97,
                                              psec     = 25,
                                              unknown  = new byte[18],
                                              unknown2 = new byte[24]
                                          });

                    currentTrackOffset += Marshal.SizeOf<Track>() * 2;
                }

                _alcToc.Add(i, thisSessionTracks);
            }
        }

        _alcFooter = new Footer
        {
            filenameOffset = (uint)(footerOffset + Marshal.SizeOf<Footer>()),
            widechar       = 1
        };

        byte[] filename = Encoding.Unicode.GetBytes("*.mdf"); // Yup, Alcohol stores no filename but a wildcard.

        // Write header
        _descriptorStream.Seek(0, SeekOrigin.Begin);
        var  block    = new byte[Marshal.SizeOf<Header>()];
        nint blockPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(Marshal.SizeOf<Header>());
        System.Runtime.InteropServices.Marshal.StructureToPtr(header, blockPtr, true);
        System.Runtime.InteropServices.Marshal.Copy(blockPtr, block, 0, block.Length);
        System.Runtime.InteropServices.Marshal.FreeHGlobal(blockPtr);
        _descriptorStream.Write(block, 0, block.Length);

        // Write DVD structures if present
        if(header.structuresOffset != 0)
        {
            if(_dmi != null)
            {
                _descriptorStream.Seek(header.structuresOffset, SeekOrigin.Begin);

                switch(_dmi.Length)
                {
                    case 2052:
                        _descriptorStream.Write(_dmi, 0, 2052);

                        break;
                    case 2048:
                        _descriptorStream.Write([0x08, 0x02, 0x00, 0x00], 0, 4);

                        _descriptorStream.Write(_dmi, 0, 2048);

                        break;
                }
            }

            // TODO: Create fake PFI if none present
            if(_pfi != null)
            {
                _descriptorStream.Seek(header.structuresOffset + 2052, SeekOrigin.Begin);
                _descriptorStream.Write(_pfi, _pfi.Length      - 2048, 2048);
            }
        }

        // Write sessions
        _descriptorStream.Seek(header.sessionOffset, SeekOrigin.Begin);

        foreach(Session session in _alcSessions.Values)
        {
            block    = new byte[Marshal.SizeOf<Session>()];
            blockPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(Marshal.SizeOf<Session>());
            System.Runtime.InteropServices.Marshal.StructureToPtr(session, blockPtr, true);
            System.Runtime.InteropServices.Marshal.Copy(blockPtr, block, 0, block.Length);
            System.Runtime.InteropServices.Marshal.FreeHGlobal(blockPtr);
            _descriptorStream.Write(block, 0, block.Length);
        }

        // Write tracks
        foreach(KeyValuePair<int, Dictionary<int, Track>> kvp in _alcToc)
        {
            _descriptorStream.Seek(_alcSessions.First(t => t.Key == kvp.Key).Value.trackOffset, SeekOrigin.Begin);

            foreach(Track track in kvp.Value.Values)
            {
                Track alcoholTrack = track;

                // Write extra
                if(!_isDvd)
                {
                    long position = _descriptorStream.Position;
                    _descriptorStream.Seek(alcoholTrack.extraOffset, SeekOrigin.Begin);

                    block = new byte[Marshal.SizeOf<TrackExtra>()];

                    if(_alcTrackExtras.TryGetValue(alcoholTrack.point, out TrackExtra extra))
                    {
                        blockPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(Marshal.SizeOf<TrackExtra>());

                        System.Runtime.InteropServices.Marshal.StructureToPtr(extra, blockPtr, true);
                        System.Runtime.InteropServices.Marshal.Copy(blockPtr, block, 0, block.Length);
                        System.Runtime.InteropServices.Marshal.FreeHGlobal(blockPtr);
                    }
                    else
                        alcoholTrack.extraOffset = 0;

                    _descriptorStream.Write(block, 0, block.Length);

                    _descriptorStream.Seek(position, SeekOrigin.Begin);
                }

                block    = new byte[Marshal.SizeOf<Track>()];
                blockPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(Marshal.SizeOf<Track>());
                System.Runtime.InteropServices.Marshal.StructureToPtr(alcoholTrack, blockPtr, true);
                System.Runtime.InteropServices.Marshal.Copy(blockPtr, block, 0, block.Length);
                System.Runtime.InteropServices.Marshal.FreeHGlobal(blockPtr);
                _descriptorStream.Write(block, 0, block.Length);
            }
        }

        // Write BCA
        if(_bca != null)
        {
            _descriptorStream.Seek(header.bcaOffset, SeekOrigin.Begin);
            _descriptorStream.Write(_bca, 0, _bca.Length);
        }

        // Write footer
        _descriptorStream.Seek(footerOffset, SeekOrigin.Begin);
        block    = new byte[Marshal.SizeOf<Footer>()];
        blockPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(Marshal.SizeOf<Footer>());
        System.Runtime.InteropServices.Marshal.StructureToPtr(_alcFooter, blockPtr, true);
        System.Runtime.InteropServices.Marshal.Copy(blockPtr, block, 0, block.Length);
        System.Runtime.InteropServices.Marshal.FreeHGlobal(blockPtr);
        _descriptorStream.Write(block, 0, block.Length);

        // Write filename
        _descriptorStream.Write(filename, 0, filename.Length);

        // Write filename null termination
        _descriptorStream.Write([0, 0], 0, 2);

        _descriptorStream.Flush();
        _descriptorStream.Close();
        _imageStream.Flush();
        _imageStream.Close();

        IsWriting    = false;
        ErrorMessage = "";

        return true;
    }

    /// <inheritdoc />
    public bool SetImageInfo(ImageInfo imageInfo) => true;

    /// <inheritdoc />
    public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
    {
        ErrorMessage = Localization.Unsupported_feature;

        return false;
    }

    /// <inheritdoc />
    public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        CommonTypes.Structs.Track track =
            _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.StartSector && sectorAddress <= trk.EndSector);

        if(track is null)
        {
            ErrorMessage = string.Format(Localization.Cant_find_track_containing_0, sectorAddress);

            return false;
        }

        switch(tag)
        {
            case SectorTagType.CdTrackFlags:
            {
                if(data.Length != 1)
                {
                    ErrorMessage = Localization.Incorrect_data_size_for_track_flags;

                    return false;
                }

                _trackFlags[(byte)sectorAddress] = data[0];

                return true;
            }
            case SectorTagType.CdSectorSubchannel:
            {
                if(track.SubchannelType == 0)
                {
                    ErrorMessage =
                        string.Format(Localization.Trying_to_write_subchannel_to_track_0_that_does_not_have_subchannel,
                                      track.Sequence);

                    return false;
                }

                if(data.Length != 96)
                {
                    ErrorMessage = Localization.Incorrect_data_size_for_subchannel;

                    return false;
                }

                _imageStream.Seek((long)(track.FileOffset +
                                         (sectorAddress - track.StartSector) * (ulong)(track.RawBytesPerSector + 96)) +
                                  track.RawBytesPerSector,
                                  SeekOrigin.Begin);

                _imageStream.Write(data, 0, data.Length);

                return true;
            }
            default:
                ErrorMessage = string.Format(Localization.Unsupported_tag_type_0, tag);

                return false;
        }
    }

    /// <inheritdoc />
    public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        CommonTypes.Structs.Track track =
            _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.StartSector && sectorAddress <= trk.EndSector);

        if(track is null)
        {
            ErrorMessage = string.Format(Localization.Cant_find_track_containing_0, sectorAddress);

            return false;
        }

        switch(tag)
        {
            case SectorTagType.CdTrackFlags:
                return WriteSectorTag(data, sectorAddress, tag);
            case SectorTagType.CdSectorSubchannel:
            {
                if(track.SubchannelType == 0)
                {
                    ErrorMessage =
                        string.Format(Localization.Trying_to_write_subchannel_to_track_0_that_does_not_have_subchannel,
                                      track.Sequence);

                    return false;
                }

                if(data.Length % 96 != 0)
                {
                    ErrorMessage = Localization.Incorrect_data_size_for_subchannel;

                    return false;
                }

                for(uint i = 0; i < length; i++)
                {
                    _imageStream.Seek((long)(track.FileOffset +
                                             (i                              + sectorAddress - track.StartSector) *
                                             (ulong)(track.RawBytesPerSector + 96)) +
                                      track.RawBytesPerSector,
                                      SeekOrigin.Begin);

                    _imageStream.Write(data, (int)(i * 96), 96);
                }

                return true;
            }
            default:
                ErrorMessage = string.Format(Localization.Unsupported_tag_type_0, tag);

                return false;
        }
    }

    /// <inheritdoc />
    public bool SetDumpHardware(List<DumpHardware> dumpHardware) => false;

    /// <inheritdoc />
    public bool SetMetadata(Metadata metadata) => false;

#endregion
}