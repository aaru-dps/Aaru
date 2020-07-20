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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Decoders.CD;
using Aaru.Helpers;
using Schemas;
using TrackType = Aaru.CommonTypes.Enums.TrackType;

namespace Aaru.DiscImages
{
    public partial class Alcohol120
    {
        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint sectorSize)
        {
            if(!SupportedMediaTypes.Contains(mediaType))
            {
                ErrorMessage = $"Unsupport media format {mediaType}";

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
                    new
                        FileStream(Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path)) + ".mdf",
                                   FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";

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

        public bool WriteMediaTag(byte[] data, MediaTagType tag)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            switch(tag)
            {
                case MediaTagType.CD_FullTOC:
                    if(_isDvd)
                    {
                        ErrorMessage = $"Unsupported media tag {tag} for medium type {_imageInfo.MediaType}";

                        return false;
                    }

                    _fullToc = data;

                    return true;
                case MediaTagType.DVD_PFI:
                    if(!_isDvd)
                    {
                        ErrorMessage = $"Unsupported media tag {tag} for medium type {_imageInfo.MediaType}";

                        return false;
                    }

                    _pfi = data;

                    return true;
                case MediaTagType.DVD_DMI:
                    if(!_isDvd)
                    {
                        ErrorMessage = $"Unsupported media tag {tag} for medium type {_imageInfo.MediaType}";

                        return false;
                    }

                    _dmi = data;

                    return true;
                case MediaTagType.DVD_BCA:
                    if(!_isDvd)
                    {
                        ErrorMessage = $"Unsupported media tag {tag} for medium type {_imageInfo.MediaType}";

                        return false;
                    }

                    _bca = data;

                    return true;
                default:
                    ErrorMessage = $"Unsupported media tag {tag}";

                    return false;
            }
        }

        public bool WriteSector(byte[] data, ulong sectorAddress)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            Track track =
                _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                     sectorAddress <= trk.TrackEndSector);

            if(track is null)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";

                return false;
            }

            if(track.TrackBytesPerSector != track.TrackRawBytesPerSector)
            {
                ErrorMessage = "Invalid write mode for this sector";

                return false;
            }

            if(data.Length != track.TrackRawBytesPerSector)
            {
                ErrorMessage = "Incorrect data size";

                return false;
            }

            _imageStream.
                Seek((long)(track.TrackFileOffset + ((sectorAddress - track.TrackStartSector) * (ulong)track.TrackRawBytesPerSector)),
                     SeekOrigin.Begin);

            _imageStream.Write(data, 0, data.Length);

            return true;
        }

        public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            Track track =
                _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                     sectorAddress <= trk.TrackEndSector);

            if(track is null)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";

                return false;
            }

            if(track.TrackBytesPerSector != track.TrackRawBytesPerSector)
            {
                ErrorMessage = "Invalid write mode for this sector";

                return false;
            }

            if(sectorAddress + length > track.TrackEndSector + 1)
            {
                ErrorMessage = "Can't cross tracks";

                return false;
            }

            if(data.Length % track.TrackRawBytesPerSector != 0)
            {
                ErrorMessage = "Incorrect data size";

                return false;
            }

            switch(track.TrackSubchannelType)
            {
                case TrackSubchannelType.None:
                    _imageStream.
                        Seek((long)(track.TrackFileOffset + ((sectorAddress - track.TrackStartSector) * (ulong)track.TrackRawBytesPerSector)),
                             SeekOrigin.Begin);

                    _imageStream.Write(data, 0, data.Length);

                    ErrorMessage = "";

                    return true;
                case TrackSubchannelType.Raw:
                case TrackSubchannelType.RawInterleaved:
                    _imageStream.
                        Seek((long)(track.TrackFileOffset + ((sectorAddress - track.TrackStartSector) * (ulong)(track.TrackRawBytesPerSector + 96))),
                             SeekOrigin.Begin);

                    for(uint i = 0; i < length; i++)
                    {
                        _imageStream.Write(data, (int)(i * track.TrackRawBytesPerSector), track.TrackRawBytesPerSector);
                        _imageStream.Position += 96;
                    }

                    ErrorMessage = "";

                    return true;
                default:
                    ErrorMessage = "Invalid subchannel mode for this sector";

                    return false;
            }
        }

        public bool WriteSectorLong(byte[] data, ulong sectorAddress)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            Track track =
                _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                     sectorAddress <= trk.TrackEndSector);

            if(track is null)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";

                return false;
            }

            if(data.Length != track.TrackRawBytesPerSector)
            {
                ErrorMessage = "Incorrect data size";

                return false;
            }

            uint subchannelSize = (uint)(track.TrackSubchannelType != TrackSubchannelType.None ? 96 : 0);

            _imageStream.
                Seek((long)(track.TrackFileOffset + ((sectorAddress - track.TrackStartSector) * (ulong)(track.TrackRawBytesPerSector + subchannelSize))),
                     SeekOrigin.Begin);

            _imageStream.Write(data, 0, data.Length);

            return true;
        }

        public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            Track track =
                _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                     sectorAddress <= trk.TrackEndSector);

            if(track is null)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";

                return false;
            }

            if(sectorAddress + length > track.TrackEndSector + 1)
            {
                ErrorMessage = "Can't cross tracks";

                return false;
            }

            if(data.Length % track.TrackRawBytesPerSector != 0)
            {
                ErrorMessage = "Incorrect data size";

                return false;
            }

            uint subchannelSize = (uint)(track.TrackSubchannelType != TrackSubchannelType.None ? 96 : 0);

            for(uint i = 0; i < length; i++)
            {
                _imageStream.
                    Seek((long)(track.TrackFileOffset + (((i + sectorAddress) - track.TrackStartSector) * (ulong)(track.TrackRawBytesPerSector + subchannelSize))),
                         SeekOrigin.Begin);

                _imageStream.Write(data, (int)(i * track.TrackRawBytesPerSector), track.TrackRawBytesPerSector);
            }

            return true;
        }

        public bool SetTracks(List<Track> tracks)
        {
            ulong currentDataOffset = 0;

            _writingTracks = new List<Track>();

            if(!_isDvd)
            {
                Track[] tmpTracks = tracks.OrderBy(t => t.TrackSequence).ToArray();

                for(int i = 1; i < tmpTracks.Length; i++)
                {
                    Track firstTrackInSession = tracks.FirstOrDefault(t => t.TrackSession == tmpTracks[i].TrackSession);

                    if(tmpTracks[i].TrackSequence == firstTrackInSession.TrackSequence)
                    {
                        if(tmpTracks[i].TrackSequence > 1)
                            tmpTracks[i].TrackStartSector += 150;

                        continue;
                    }

                    tmpTracks[i - 1].TrackEndSector += tmpTracks[i].TrackPregap;
                    tmpTracks[i].TrackPregap        =  0;
                    tmpTracks[i].TrackStartSector   =  tmpTracks[i - 1].TrackEndSector + 1;
                }

                tracks = tmpTracks.ToList();
            }

            foreach(Track track in tracks.OrderBy(t => t.TrackSequence))
            {
                Track newTrack = track;
                uint  subchannelSize;

                switch(track.TrackSubchannelType)
                {
                    case TrackSubchannelType.None:
                        subchannelSize = 0;

                        break;
                    case TrackSubchannelType.Raw:
                    case TrackSubchannelType.RawInterleaved:
                        subchannelSize = 96;

                        break;
                    default:
                        ErrorMessage = $"Unsupported subchannel type {track.TrackSubchannelType}";

                        return false;
                }

                newTrack.TrackFileOffset = currentDataOffset;

                currentDataOffset += (ulong)(newTrack.TrackRawBytesPerSector + subchannelSize) *
                                     ((newTrack.TrackEndSector - newTrack.TrackStartSector) + 1);

                _writingTracks.Add(newTrack);
            }

            return true;
        }

        public bool Close()
        {
            if(!IsWriting)
            {
                ErrorMessage = "Image is not opened for writing";

                return false;
            }

            byte sessions = byte.MinValue;

            foreach(Track t in _writingTracks)
                if(t.TrackSession > byte.MinValue)
                    sessions = (byte)t.TrackSession;

            var header = new AlcoholHeader
            {
                signature = _alcoholSignature,
                version = new byte[]
                {
                    1, 5
                },
                type             = MediaTypeToAlcohol(_imageInfo.MediaType),
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

            _alcSessions    = new Dictionary<int, AlcoholSession>();
            _alcTracks      = new Dictionary<int, AlcoholTrack>();
            _alcToc         = new Dictionary<int, Dictionary<int, AlcoholTrack>>();
            _writingTracks  = _writingTracks.OrderBy(t => t.TrackSession).ThenBy(t => t.TrackSequence).ToList();
            _alcTrackExtras = new Dictionary<int, AlcoholTrackExtra>();
            long currentTrackOffset = header.sessionOffset + (Marshal.SizeOf<AlcoholSession>() * sessions);

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
            int  extraCount         = 0;

            for(int i = 1; i <= sessions; i++)
                if(decodedToc.HasValue)
                {
                    extraCount += decodedToc.Value.TrackDescriptors.Count(t => t.SessionNumber == i);

                    currentExtraOffset += Marshal.SizeOf<AlcoholTrack>() *
                                          decodedToc.Value.TrackDescriptors.Count(t => t.SessionNumber == i);
                }
                else
                {
                    currentExtraOffset += Marshal.SizeOf<AlcoholTrack>() * 3;
                    extraCount         += 3;

                    currentExtraOffset += Marshal.SizeOf<AlcoholTrack>() *
                                          _writingTracks.Count(t => t.TrackSession == i);

                    extraCount += _writingTracks.Count(t => t.TrackSession == i);

                    if(i < sessions)
                    {
                        currentExtraOffset += Marshal.SizeOf<AlcoholTrack>() * 2;
                        extraCount         += 2;
                    }
                }

            long footerOffset = currentExtraOffset + (Marshal.SizeOf<AlcoholTrackExtra>() * extraCount);

            if(_bca != null)
            {
                header.bcaOffset =  (uint)footerOffset;
                footerOffset     += _bca.Length;
            }

            if(_isDvd)
            {
                _alcSessions.Add(1, new AlcoholSession
                {
                    sessionEnd = (int)((_writingTracks[0].TrackEndSector - _writingTracks[0].TrackStartSector) + 1),
                    sessionSequence = 1,
                    allBlocks = 1,
                    nonTrackBlocks = 3,
                    firstTrack = 1,
                    lastTrack = 1,
                    trackOffset = 4220
                });

                footerOffset = 4300;

                if(_bca != null)
                    footerOffset += _bca.Length;

                _alcTracks.Add(1, new AlcoholTrack
                {
                    mode         = AlcoholTrackMode.DVD,
                    adrCtl       = 20,
                    point        = 1,
                    extraOffset  = (uint)((_writingTracks[0].TrackEndSector - _writingTracks[0].TrackStartSector) + 1),
                    sectorSize   = 2048,
                    files        = 1,
                    footerOffset = (uint)footerOffset,
                    unknown      = new byte[18],
                    unknown2     = new byte[24]
                });

                _alcToc.Add(1, _alcTracks);
            }
            else
                for(int i = 1; i <= sessions; i++)
                {
                    Track firstTrack = _writingTracks.First(t => t.TrackSession == i);
                    Track lastTrack  = _writingTracks.Last(t => t.TrackSession  == i);

                    _alcSessions.Add(i, new AlcoholSession
                    {
                        sessionStart    = (int)firstTrack.TrackStartSector - 150,
                        sessionEnd      = (int)lastTrack.TrackEndSector    + 1,
                        sessionSequence = (ushort)i,
                        allBlocks = (byte)(decodedToc?.TrackDescriptors.Count(t => t.SessionNumber == i) ??
                                           _writingTracks.Count(t => t.TrackSession == i) + 3),
                        nonTrackBlocks =
                            (byte)(decodedToc?.TrackDescriptors.Count(t => t.SessionNumber == i && t.POINT >= 0xA0 &&
                                                                           t.POINT         <= 0xAF) ?? 3),
                        firstTrack  = (ushort)firstTrack.TrackSequence,
                        lastTrack   = (ushort)lastTrack.TrackSequence,
                        trackOffset = (uint)currentTrackOffset
                    });

                    Dictionary<int, AlcoholTrack> thisSessionTracks = new Dictionary<int, AlcoholTrack>();
                    _trackFlags.TryGetValue((byte)firstTrack.TrackSequence, out byte firstTrackControl);
                    _trackFlags.TryGetValue((byte)lastTrack.TrackSequence, out byte lastTrackControl);

                    if(firstTrackControl    == 0 &&
                       firstTrack.TrackType != TrackType.Audio)
                        firstTrackControl = (byte)CdFlags.DataTrack;

                    if(lastTrackControl    == 0 &&
                       lastTrack.TrackType != TrackType.Audio)
                        lastTrackControl = (byte)CdFlags.DataTrack;

                    (byte minute, byte second, byte frame) leadinPmsf = LbaToMsf(lastTrack.TrackEndSector + 1);

                    if(decodedToc.HasValue &&
                       decodedToc.Value.TrackDescriptors.Any(t => t.SessionNumber == i && t.POINT >= 0xA0 &&
                                                                  t.POINT         <= 0xAF))
                        foreach(FullTOC.TrackDataDescriptor tocTrk in
                            decodedToc.Value.TrackDescriptors.Where(t => t.SessionNumber == i && t.POINT >= 0xA0 &&
                                                                         t.POINT         <= 0xAF))
                        {
                            thisSessionTracks.Add(tocTrk.POINT, new AlcoholTrack
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
                                mode        = AlcoholTrackMode.NoData,
                                unknown     = new byte[18],
                                unknown2    = new byte[24],
                                extraOffset = (uint)currentExtraOffset
                            });

                            currentTrackOffset += Marshal.SizeOf<AlcoholTrack>();
                            currentExtraOffset += Marshal.SizeOf<AlcoholTrackExtra>();
                        }
                    else
                    {
                        thisSessionTracks.Add(0xA0, new AlcoholTrack
                        {
                            adrCtl   = (byte)((1 << 4) + firstTrackControl),
                            pmin     = (byte)firstTrack.TrackSequence,
                            mode     = AlcoholTrackMode.NoData,
                            point    = 0xA0,
                            unknown  = new byte[18],
                            unknown2 = new byte[24],
                            psec = (byte)(_imageInfo.MediaType == MediaType.CDI
                                              ? 0x10
                                              : _writingTracks.Any(t => t.TrackType == TrackType.CdMode2Form1 ||
                                                                        t.TrackType == TrackType.CdMode2Form2 ||
                                                                        t.TrackType == TrackType.CdMode2Formless)
                                                  ? 0x20
                                                  : 0),
                            extraOffset = (uint)currentExtraOffset
                        });

                        thisSessionTracks.Add(0xA1, new AlcoholTrack
                        {
                            adrCtl      = (byte)((1 << 4) + lastTrackControl),
                            pmin        = (byte)lastTrack.TrackSequence,
                            mode        = AlcoholTrackMode.NoData,
                            point       = 0xA1,
                            unknown     = new byte[18],
                            unknown2    = new byte[24],
                            extraOffset = (uint)currentExtraOffset
                        });

                        thisSessionTracks.Add(0xA2, new AlcoholTrack
                        {
                            adrCtl      = (byte)((1 << 4) + firstTrackControl),
                            zero        = 0,
                            pmin        = leadinPmsf.minute,
                            psec        = leadinPmsf.second,
                            pframe      = leadinPmsf.frame,
                            mode        = AlcoholTrackMode.NoData,
                            point       = 0xA2,
                            unknown     = new byte[18],
                            unknown2    = new byte[24],
                            extraOffset = (uint)currentExtraOffset
                        });

                        currentExtraOffset += Marshal.SizeOf<AlcoholTrackExtra>() * 3;
                        currentTrackOffset += Marshal.SizeOf<AlcoholTrack>()      * 3;
                    }

                    foreach(Track track in _writingTracks.Where(t => t.TrackSession == i).OrderBy(t => t.TrackSequence))
                    {
                        var alcTrk = new AlcoholTrack();

                        if(decodedToc.HasValue &&
                           decodedToc.Value.TrackDescriptors.Any(t => t.SessionNumber == i &&
                                                                      t.POINT         == track.TrackSequence))
                        {
                            FullTOC.TrackDataDescriptor tocTrk =
                                decodedToc.Value.TrackDescriptors.First(t => t.SessionNumber == i &&
                                                                             t.POINT         == track.TrackSequence);

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
                            (byte minute, byte second, byte frame) msf = LbaToMsf(track.TrackStartSector);
                            _trackFlags.TryGetValue((byte)track.TrackSequence, out byte trackControl);

                            if(trackControl    == 0 &&
                               track.TrackType != TrackType.Audio)
                                trackControl = (byte)CdFlags.DataTrack;

                            alcTrk.adrCtl = (byte)((1 << 4) + trackControl);
                            alcTrk.point  = (byte)track.TrackSequence;
                            alcTrk.zero   = 0;
                            alcTrk.pmin   = msf.minute;
                            alcTrk.psec   = msf.second;
                            alcTrk.pframe = msf.frame;
                        }

                        alcTrk.mode = TrackTypeToAlcohol(track.TrackType);

                        alcTrk.subMode = track.TrackSubchannelType != TrackSubchannelType.None
                                             ? AlcoholSubchannelMode.Interleaved : AlcoholSubchannelMode.None;

                        alcTrk.sectorSize = (ushort)(track.TrackRawBytesPerSector +
                                                     (track.TrackSubchannelType != TrackSubchannelType.None ? 96 : 0));

                        alcTrk.startLba     = (uint)(track.TrackStartSector + track.TrackPregap);
                        alcTrk.startOffset  = track.TrackFileOffset + (alcTrk.sectorSize * track.TrackPregap);
                        alcTrk.files        = 1;
                        alcTrk.extraOffset  = (uint)currentExtraOffset;
                        alcTrk.footerOffset = (uint)footerOffset;

                        if(track.TrackSequence == firstTrack.TrackSequence &&
                           track.TrackSequence > 1)
                        {
                            alcTrk.startLba    -= 150;
                            alcTrk.startOffset -= (ulong)(alcTrk.sectorSize * 150);
                        }

                        // Alcohol seems to set that for all CD tracks
                        // Daemon Tools expect it to be like this
                        alcTrk.unknown = new byte[]
                        {
                            0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00
                        };

                        alcTrk.unknown2 = new byte[24];

                        thisSessionTracks.Add((int)track.TrackSequence, alcTrk);

                        currentTrackOffset += Marshal.SizeOf<AlcoholTrack>();
                        currentExtraOffset += Marshal.SizeOf<AlcoholTrackExtra>();

                        var trkExtra = new AlcoholTrackExtra
                        {
                            sectors = (uint)((track.TrackEndSector - track.TrackStartSector) + 1)
                        };

                        if(track.TrackSequence == firstTrack.TrackSequence)
                            trkExtra.pregap = 150;

                        // When track mode changes there's a mandatory gap, Alcohol needs it
                        else if(thisSessionTracks.TryGetValue((int)(track.TrackSequence - 1),
                                                              out AlcoholTrack previousTrack) &&
                                _alcTrackExtras.TryGetValue((int)(track.TrackSequence - 1),
                                                            out AlcoholTrackExtra previousExtra) &&
                                previousTrack.mode != alcTrk.mode)
                        {
                            previousExtra.sectors -= 150;
                            trkExtra.pregap       =  150;
                            _alcTrackExtras.Remove((int)(track.TrackSequence - 1));
                            _alcTrackExtras.Add((int)(track.TrackSequence    - 1), previousExtra);
                        }
                        else
                            trkExtra.pregap = 0;

                        _alcTrackExtras.Add((int)track.TrackSequence, trkExtra);
                    }

                    if(decodedToc.HasValue &&
                       decodedToc.Value.TrackDescriptors.Any(t => t.SessionNumber == i && t.POINT >= 0xB0))
                        foreach(FullTOC.TrackDataDescriptor tocTrk in
                            decodedToc.Value.TrackDescriptors.Where(t => t.SessionNumber == i && t.POINT >= 0xB0))
                        {
                            thisSessionTracks.Add(tocTrk.POINT, new AlcoholTrack
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
                                mode        = AlcoholTrackMode.NoData,
                                unknown     = new byte[18],
                                unknown2    = new byte[24],
                                extraOffset = (uint)currentExtraOffset
                            });

                            currentExtraOffset += Marshal.SizeOf<AlcoholTrackExtra>();
                            currentTrackOffset += Marshal.SizeOf<AlcoholTrack>();
                        }
                    else if(i < sessions)
                    {
                        (byte minute, byte second, byte frame) leadoutAmsf =
                            LbaToMsf(_writingTracks.First(t => t.TrackSession == i + 1).TrackStartSector - 150);

                        (byte minute, byte second, byte frame) leadoutPmsf =
                            LbaToMsf(_writingTracks.OrderBy(t => t.TrackSession).ThenBy(t => t.TrackSequence).Last().
                                                    TrackStartSector);

                        thisSessionTracks.Add(0xB0, new AlcoholTrack
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

                        thisSessionTracks.Add(0xC0, new AlcoholTrack
                        {
                            point    = 0xC0,
                            adrCtl   = 0x50,
                            min      = 128,
                            pmin     = 97,
                            psec     = 25,
                            unknown  = new byte[18],
                            unknown2 = new byte[24]
                        });

                        currentTrackOffset += Marshal.SizeOf<AlcoholTrack>() * 2;
                    }

                    _alcToc.Add(i, thisSessionTracks);
                }

            _alcFooter = new AlcoholFooter
            {
                filenameOffset = (uint)(footerOffset + Marshal.SizeOf<AlcoholFooter>()),
                widechar       = 1
            };

            byte[] filename = Encoding.Unicode.GetBytes("*.mdf"); // Yup, Alcohol stores no filename but a wildcard.

            IntPtr blockPtr;

            // Write header
            _descriptorStream.Seek(0, SeekOrigin.Begin);
            byte[] block = new byte[Marshal.SizeOf<AlcoholHeader>()];
            blockPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(Marshal.SizeOf<AlcoholHeader>());
            System.Runtime.InteropServices.Marshal.StructureToPtr(header, blockPtr, true);
            System.Runtime.InteropServices.Marshal.Copy(blockPtr, block, 0, block.Length);
            System.Runtime.InteropServices.Marshal.FreeHGlobal(blockPtr);
            _descriptorStream.Write(block, 0, block.Length);

            // Write DVD structures if pressent
            if(header.structuresOffset != 0)
            {
                if(_dmi != null)
                {
                    _descriptorStream.Seek(header.structuresOffset, SeekOrigin.Begin);

                    if(_dmi.Length == 2052)
                        _descriptorStream.Write(_dmi, 0, 2052);
                    else if(_dmi.Length == 2048)
                    {
                        _descriptorStream.Write(new byte[]
                        {
                            0x08, 0x02, 0x00, 0x00
                        }, 0, 4);

                        _descriptorStream.Write(_dmi, 0, 2048);
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

            foreach(AlcoholSession session in _alcSessions.Values)
            {
                block    = new byte[Marshal.SizeOf<AlcoholSession>()];
                blockPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(Marshal.SizeOf<AlcoholSession>());
                System.Runtime.InteropServices.Marshal.StructureToPtr(session, blockPtr, true);
                System.Runtime.InteropServices.Marshal.Copy(blockPtr, block, 0, block.Length);
                System.Runtime.InteropServices.Marshal.FreeHGlobal(blockPtr);
                _descriptorStream.Write(block, 0, block.Length);
            }

            // Write tracks
            foreach(KeyValuePair<int, Dictionary<int, AlcoholTrack>> kvp in _alcToc)
            {
                _descriptorStream.Seek(_alcSessions.First(t => t.Key == kvp.Key).Value.trackOffset, SeekOrigin.Begin);

                foreach(AlcoholTrack track in kvp.Value.Values)
                {
                    AlcoholTrack alcoholTrack = track;

                    // Write extra
                    if(!_isDvd)
                    {
                        long position = _descriptorStream.Position;
                        _descriptorStream.Seek(alcoholTrack.extraOffset, SeekOrigin.Begin);

                        block = new byte[Marshal.SizeOf<AlcoholTrackExtra>()];

                        if(_alcTrackExtras.TryGetValue(alcoholTrack.point, out AlcoholTrackExtra extra))
                        {
                            blockPtr =
                                System.Runtime.InteropServices.Marshal.
                                       AllocHGlobal(Marshal.SizeOf<AlcoholTrackExtra>());

                            System.Runtime.InteropServices.Marshal.StructureToPtr(extra, blockPtr, true);
                            System.Runtime.InteropServices.Marshal.Copy(blockPtr, block, 0, block.Length);
                            System.Runtime.InteropServices.Marshal.FreeHGlobal(blockPtr);
                        }
                        else
                            alcoholTrack.extraOffset = 0;

                        _descriptorStream.Write(block, 0, block.Length);

                        _descriptorStream.Seek(position, SeekOrigin.Begin);
                    }

                    block    = new byte[Marshal.SizeOf<AlcoholTrack>()];
                    blockPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(Marshal.SizeOf<AlcoholTrack>());
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
            block    = new byte[Marshal.SizeOf<AlcoholFooter>()];
            blockPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(Marshal.SizeOf<AlcoholFooter>());
            System.Runtime.InteropServices.Marshal.StructureToPtr(_alcFooter, blockPtr, true);
            System.Runtime.InteropServices.Marshal.Copy(blockPtr, block, 0, block.Length);
            System.Runtime.InteropServices.Marshal.FreeHGlobal(blockPtr);
            _descriptorStream.Write(block, 0, block.Length);

            // Write filename
            _descriptorStream.Write(filename, 0, filename.Length);

            // Write filename null termination
            _descriptorStream.Write(new byte[]
            {
                0, 0
            }, 0, 2);

            _descriptorStream.Flush();
            _descriptorStream.Close();
            _imageStream.Flush();
            _imageStream.Close();

            IsWriting    = false;
            ErrorMessage = "";

            return true;
        }

        public bool SetMetadata(ImageInfo metadata) => true;

        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
        {
            ErrorMessage = "Unsupported feature";

            return false;
        }

        public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            Track track =
                _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                     sectorAddress <= trk.TrackEndSector);

            if(track is null)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";

                return false;
            }

            switch(tag)
            {
                case SectorTagType.CdTrackFlags:
                {
                    if(data.Length != 1)
                    {
                        ErrorMessage = "Incorrect data size for track flags";

                        return false;
                    }

                    _trackFlags[(byte)sectorAddress] = data[0];

                    return true;
                }
                case SectorTagType.CdSectorSubchannel:
                {
                    if(track.TrackSubchannelType == 0)
                    {
                        ErrorMessage =
                            $"Trying to write subchannel to track {track.TrackSequence}, that does not have subchannel";

                        return false;
                    }

                    if(data.Length != 96)
                    {
                        ErrorMessage = "Incorrect data size for subchannel";

                        return false;
                    }

                    _imageStream.
                        Seek((long)(track.TrackFileOffset + ((sectorAddress - track.TrackStartSector) * (ulong)(track.TrackRawBytesPerSector + 96))) + track.TrackRawBytesPerSector,
                             SeekOrigin.Begin);

                    _imageStream.Write(data, 0, data.Length);

                    return true;
                }
                default:
                    ErrorMessage = $"Unsupported tag type {tag}";

                    return false;
            }
        }

        public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            Track track =
                _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                     sectorAddress <= trk.TrackEndSector);

            if(track is null)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";

                return false;
            }

            switch(tag)
            {
                case SectorTagType.CdTrackFlags: return WriteSectorTag(data, sectorAddress, tag);
                case SectorTagType.CdSectorSubchannel:
                {
                    if(track.TrackSubchannelType == 0)
                    {
                        ErrorMessage =
                            $"Trying to write subchannel to track {track.TrackSequence}, that does not have subchannel";

                        return false;
                    }

                    if(data.Length % 96 != 0)
                    {
                        ErrorMessage = "Incorrect data size for subchannel";

                        return false;
                    }

                    for(uint i = 0; i < length; i++)
                    {
                        _imageStream.
                            Seek((long)(track.TrackFileOffset + (((i + sectorAddress) - track.TrackStartSector) * (ulong)(track.TrackRawBytesPerSector + 96))) + track.TrackRawBytesPerSector,
                                 SeekOrigin.Begin);

                        _imageStream.Write(data, (int)(i * 96), 96);
                    }

                    return true;
                }
                default:
                    ErrorMessage = $"Unsupported tag type {tag}";

                    return false;
            }
        }

        public bool SetDumpHardware(List<DumpHardwareType> dumpHardware) => false;

        public bool SetCicmMetadata(CICMMetadataType metadata) => false;
    }
}