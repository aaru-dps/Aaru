// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Decoders.CD;
using Schemas;
using TrackType = DiscImageChef.CommonTypes.Enums.TrackType;

namespace DiscImageChef.DiscImages
{
    public partial class Alcohol120
    {
        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint   sectorSize)
        {
            if(!SupportedMediaTypes.Contains(mediaType))
            {
                ErrorMessage = $"Unsupport media format {mediaType}";
                return false;
            }

            imageInfo = new ImageInfo {MediaType = mediaType, SectorSize = sectorSize, Sectors = sectors};

            try
            {
                descriptorStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                imageStream =
                    new
                        FileStream(Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path)) + ".mdf",
                                   FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";
                return false;
            }

            imageInfo.MediaType = mediaType;

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
                    isDvd = false;
                    break;
                default:
                    isDvd = true;
                    break;
            }

            trackFlags = new Dictionary<byte, byte>();

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
                    if(isDvd)
                    {
                        ErrorMessage = $"Unsupported media tag {tag} for medium type {imageInfo.MediaType}";
                        return false;
                    }

                    byte[] fullTocSize = BigEndianBitConverter.GetBytes((short)data.Length);
                    fullToc = new byte[data.Length + 2];
                    Array.Copy(data, 0, fullToc, 2, data.Length);
                    fullToc[0] = fullTocSize[0];
                    fullToc[1] = fullTocSize[1];
                    return true;
                case MediaTagType.DVD_PFI:
                    if(!isDvd)
                    {
                        ErrorMessage = $"Unsupported media tag {tag} for medium type {imageInfo.MediaType}";
                        return false;
                    }

                    pfi = data;
                    return true;
                case MediaTagType.DVD_DMI:
                    if(!isDvd)
                    {
                        ErrorMessage = $"Unsupported media tag {tag} for medium type {imageInfo.MediaType}";
                        return false;
                    }

                    dmi = data;
                    return true;
                case MediaTagType.DVD_BCA:
                    if(!isDvd)
                    {
                        ErrorMessage = $"Unsupported media tag {tag} for medium type {imageInfo.MediaType}";
                        return false;
                    }

                    bca = data;
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
                writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                    sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
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

            imageStream.Seek((long)(track.TrackFileOffset + (sectorAddress - track.TrackStartSector) * (ulong)track.TrackRawBytesPerSector),
                             SeekOrigin.Begin);
            imageStream.Write(data, 0, data.Length);
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
                writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                    sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
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
                    imageStream
                       .Seek((long)(track.TrackFileOffset + (sectorAddress - track.TrackStartSector) * (ulong)track.TrackRawBytesPerSector),
                             SeekOrigin.Begin);
                    imageStream.Write(data, 0, data.Length);

                    ErrorMessage = "";
                    return true;
                case TrackSubchannelType.Raw:
                case TrackSubchannelType.RawInterleaved:
                    imageStream
                       .Seek((long)(track.TrackFileOffset + (sectorAddress - track.TrackStartSector) * (ulong)(track.TrackRawBytesPerSector + 96)),
                             SeekOrigin.Begin);
                    for(uint i = 0; i < length; i++)
                    {
                        imageStream.Write(data, (int)(i * track.TrackRawBytesPerSector), track.TrackRawBytesPerSector);
                        imageStream.Position += 96;
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
                writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                    sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
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

            imageStream.Seek((long)(track.TrackFileOffset + (sectorAddress - track.TrackStartSector) * (ulong)(track.TrackRawBytesPerSector + subchannelSize)),
                             SeekOrigin.Begin);
            imageStream.Write(data, 0, data.Length);

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
                writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                    sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
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
                imageStream.Seek((long)(track.TrackFileOffset + (i + sectorAddress - track.TrackStartSector) * (ulong)(track.TrackRawBytesPerSector + subchannelSize)),
                                 SeekOrigin.Begin);
                imageStream.Write(data, (int)(i * track.TrackRawBytesPerSector), track.TrackRawBytesPerSector);
            }

            return true;
        }

        public bool SetTracks(List<Track> tracks)
        {
            ulong currentDataOffset = 0;

            writingTracks = new List<Track>();
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
                                     (newTrack.TrackEndSector                - newTrack.TrackStartSector + 1);

                writingTracks.Add(newTrack);
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

            foreach(Track t in writingTracks)
                if(t.TrackSession > byte.MinValue)
                    sessions = (byte)t.TrackSession;

            AlcoholHeader header = new AlcoholHeader
            {
                signature        = alcoholSignature,
                version          = new byte[] {1, 5},
                type             = MediaTypeToAlcohol(imageInfo.MediaType),
                sessions         = sessions,
                structuresOffset = (uint)(pfi == null ? 0 : 96),
                sessionOffset    = (uint)(pfi == null ? 96 : 4196),
                unknown1         = new ushort[2],
                unknown2         = new uint[2],
                unknown3         = new uint[6],
                unknown4         = new uint[3]
            };
            // Alcohol sets this always, Daemon Tool expects this
            header.unknown1[0] = 2;

            alcSessions    = new Dictionary<int, AlcoholSession>();
            alcTracks      = new Dictionary<int, AlcoholTrack>();
            alcToc         = new Dictionary<int, Dictionary<int, AlcoholTrack>>();
            writingTracks  = writingTracks.OrderBy(t => t.TrackSession).ThenBy(t => t.TrackSequence).ToList();
            alcTrackExtras = new Dictionary<int, AlcoholTrackExtra>();
            long currentTrackOffset = header.sessionOffset + Marshal.SizeOf(typeof(AlcoholSession)) * sessions;

            FullTOC.CDFullTOC? decodedToc = FullTOC.Decode(fullToc);

            long currentExtraOffset = currentTrackOffset;
            for(int i = 1; i <= sessions; i++)
                if(decodedToc.HasValue)
                    currentExtraOffset += Marshal.SizeOf(typeof(AlcoholTrack)) *
                                          decodedToc.Value.TrackDescriptors.Count(t => t.SessionNumber == i);
                else
                {
                    currentExtraOffset += Marshal.SizeOf(typeof(AlcoholTrack)) * 3;
                    currentExtraOffset += Marshal.SizeOf(typeof(AlcoholTrack)) *
                                          writingTracks.Count(t => t.TrackSession == i);
                    if(i < sessions) currentExtraOffset += Marshal.SizeOf(typeof(AlcoholTrack)) * 2;
                }

            long footerOffset = currentExtraOffset + Marshal.SizeOf(typeof(AlcoholTrackExtra)) * writingTracks.Count;
            if(bca != null)
            {
                header.bcaOffset =  (uint)footerOffset;
                footerOffset     += bca.Length;
            }

            if(isDvd)
            {
                alcSessions.Add(1,
                                new AlcoholSession
                                {
                                    sessionEnd =
                                        (int)(writingTracks[0].TrackEndSector - writingTracks[0].TrackStartSector +
                                              1),
                                    sessionSequence = 1,
                                    allBlocks       = 1,
                                    nonTrackBlocks  = 3,
                                    firstTrack      = 1,
                                    lastTrack       = 1,
                                    trackOffset     = 4220
                                });

                footerOffset = 4300;
                if(bca != null) footerOffset += bca.Length;

                alcTracks.Add(1,
                              new AlcoholTrack
                              {
                                  mode   = AlcoholTrackMode.DVD,
                                  adrCtl = 20,
                                  point  = 1,
                                  extraOffset =
                                      (uint)(writingTracks[0].TrackEndSector - writingTracks[0].TrackStartSector +
                                             1),
                                  sectorSize   = 2048,
                                  files        = 1,
                                  footerOffset = (uint)footerOffset,
                                  unknown      = new byte[18],
                                  unknown2     = new byte[24]
                              });

                alcToc.Add(1, alcTracks);
            }
            else
                for(int i = 1; i <= sessions; i++)
                {
                    Track firstTrack = writingTracks.First(t => t.TrackSession == i);
                    Track lastTrack  = writingTracks.Last(t => t.TrackSession  == i);

                    alcSessions.Add(i,
                                    new AlcoholSession
                                    {
                                        sessionStart    = (int)firstTrack.TrackStartSector - 150,
                                        sessionEnd      = (int)lastTrack.TrackEndSector    + 1,
                                        sessionSequence = (ushort)i,
                                        allBlocks =
                                            (byte)(decodedToc?.TrackDescriptors.Count(t => t.SessionNumber == i) ??
                                                   writingTracks.Count(t => t.TrackSession == i) + 3),
                                        nonTrackBlocks =
                                            (byte)(decodedToc?.TrackDescriptors.Count(t => t.SessionNumber == i    &&
                                                                                           t.POINT         >= 0xA0 &&
                                                                                           t.POINT         <= 0xAF) ??
                                                   3),
                                        firstTrack  = (ushort)firstTrack.TrackSequence,
                                        lastTrack   = (ushort)lastTrack.TrackSequence,
                                        trackOffset = (uint)currentTrackOffset
                                    });

                    Dictionary<int, AlcoholTrack> thisSessionTracks = new Dictionary<int, AlcoholTrack>();
                    trackFlags.TryGetValue((byte)firstTrack.TrackSequence, out byte firstTrackControl);
                    trackFlags.TryGetValue((byte)lastTrack.TrackSequence,  out byte lastTrackControl);
                    if(firstTrackControl == 0 && firstTrack.TrackType != TrackType.Audio)
                        firstTrackControl = (byte)CdFlags.DataTrack;
                    if(lastTrackControl == 0 && lastTrack.TrackType != TrackType.Audio)
                        lastTrackControl = (byte)CdFlags.DataTrack;
                    (byte minute, byte second, byte frame) leadinPmsf = LbaToMsf(lastTrack.TrackEndSector + 1);

                    if(decodedToc.HasValue &&
                       decodedToc.Value.TrackDescriptors.Any(t => t.SessionNumber == i && t.POINT >= 0xA0 &&
                                                                  t.POINT         <= 0xAF))
                        foreach(FullTOC.TrackDataDescriptor tocTrk in
                            decodedToc.Value.TrackDescriptors.Where(t => t.SessionNumber == i && t.POINT >= 0xA0 &&
                                                                         t.POINT         <= 0xAF))
                        {
                            thisSessionTracks.Add(tocTrk.POINT,
                                                  new AlcoholTrack
                                                  {
                                                      adrCtl   = (byte)((tocTrk.ADR << 4) + tocTrk.CONTROL),
                                                      tno      = tocTrk.TNO,
                                                      point    = tocTrk.POINT,
                                                      min      = tocTrk.Min,
                                                      sec      = tocTrk.Sec,
                                                      frame    = tocTrk.Frame,
                                                      zero     = tocTrk.Zero,
                                                      pmin     = tocTrk.PMIN,
                                                      psec     = tocTrk.PSEC,
                                                      pframe   = tocTrk.PFRAME,
                                                      mode     = AlcoholTrackMode.NoData,
                                                      unknown  = new byte[18],
                                                      unknown2 = new byte[24]
                                                  });
                            currentTrackOffset += Marshal.SizeOf(typeof(AlcoholTrack));
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
                            psec = (byte)(imageInfo.MediaType == MediaType.CDI
                                              ? 0x10
                                              : writingTracks.Any(t => t.TrackType == TrackType.CdMode2Form1 ||
                                                                       t.TrackType == TrackType.CdMode2Form2 ||
                                                                       t.TrackType == TrackType.CdMode2Formless)
                                                  ? 0x20
                                                  : 0)
                        });

                        thisSessionTracks.Add(0xA1,
                                              new AlcoholTrack
                                              {
                                                  adrCtl   = (byte)((1 << 4) + lastTrackControl),
                                                  pmin     = (byte)lastTrack.TrackSequence,
                                                  mode     = AlcoholTrackMode.NoData,
                                                  point    = 0xA1,
                                                  unknown  = new byte[18],
                                                  unknown2 = new byte[24]
                                              });

                        thisSessionTracks.Add(0xA2,
                                              new AlcoholTrack
                                              {
                                                  adrCtl   = (byte)((1 << 4) + firstTrackControl),
                                                  zero     = 0,
                                                  pmin     = leadinPmsf.minute,
                                                  psec     = leadinPmsf.second,
                                                  pframe   = leadinPmsf.frame,
                                                  mode     = AlcoholTrackMode.NoData,
                                                  point    = 0xA2,
                                                  unknown  = new byte[18],
                                                  unknown2 = new byte[24]
                                              });
                        currentTrackOffset += Marshal.SizeOf(typeof(AlcoholTrack)) * 3;
                    }

                    foreach(Track track in writingTracks.Where(t => t.TrackSession == i).OrderBy(t => t.TrackSequence))
                    {
                        AlcoholTrack alcTrk = new AlcoholTrack();
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
                            trackFlags.TryGetValue((byte)track.TrackSequence, out byte trackControl);
                            if(trackControl == 0 && track.TrackType != TrackType.Audio)
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
                                             ? AlcoholSubchannelMode.Interleaved
                                             : AlcoholSubchannelMode.None;
                        alcTrk.sectorSize = (ushort)(track.TrackRawBytesPerSector +
                                                     (track.TrackSubchannelType != TrackSubchannelType.None ? 96 : 0));
                        alcTrk.startLba     = (uint)track.TrackStartSector;
                        alcTrk.startOffset  = track.TrackFileOffset;
                        alcTrk.files        = 1;
                        alcTrk.extraOffset  = (uint)currentExtraOffset;
                        alcTrk.footerOffset = (uint)footerOffset;
                        // Alcohol seems to set that for all CD tracks
                        // Daemon Tools expect it to be like this
                        alcTrk.unknown = new byte[]
                        {
                            0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00
                        };
                        alcTrk.unknown2 = new byte[24];

                        thisSessionTracks.Add((int)track.TrackSequence, alcTrk);

                        currentTrackOffset += Marshal.SizeOf(typeof(AlcoholTrack));
                        currentExtraOffset += Marshal.SizeOf(typeof(AlcoholTrackExtra));

                        AlcoholTrackExtra trkExtra = new AlcoholTrackExtra
                        {
                            sectors = (uint)(track.TrackEndSector - track.TrackStartSector + 1)
                        };

                        // When track mode changes there's a mandatory gap, Alcohol needs it
                        if(track.TrackSequence == firstTrack.TrackSequence) trkExtra.pregap = 150;
                        else if(thisSessionTracks.TryGetValue((int)(track.TrackSequence - 1),
                                                              out AlcoholTrack previousTrack) &&
                                alcTrackExtras.TryGetValue((int)(track.TrackSequence - 1),
                                                           out AlcoholTrackExtra previousExtra) &&
                                previousTrack.mode != alcTrk.mode)
                        {
                            previousExtra.sectors -= 150;
                            trkExtra.pregap       =  150;
                            alcTrackExtras.Remove((int)(track.TrackSequence - 1));
                            alcTrackExtras.Add((int)(track.TrackSequence    - 1), previousExtra);
                        }
                        else trkExtra.pregap = 0;

                        alcTrackExtras.Add((int)track.TrackSequence, trkExtra);
                    }

                    if(decodedToc.HasValue &&
                       decodedToc.Value.TrackDescriptors.Any(t => t.SessionNumber == i && t.POINT >= 0xB0))
                        foreach(FullTOC.TrackDataDescriptor tocTrk in
                            decodedToc.Value.TrackDescriptors.Where(t => t.SessionNumber == i && t.POINT >= 0xB0))
                        {
                            thisSessionTracks.Add(tocTrk.POINT,
                                                  new AlcoholTrack
                                                  {
                                                      adrCtl   = (byte)((tocTrk.ADR << 4) + tocTrk.CONTROL),
                                                      tno      = tocTrk.TNO,
                                                      point    = tocTrk.POINT,
                                                      min      = tocTrk.Min,
                                                      sec      = tocTrk.Sec,
                                                      frame    = tocTrk.Frame,
                                                      zero     = tocTrk.Zero,
                                                      pmin     = tocTrk.PMIN,
                                                      psec     = tocTrk.PSEC,
                                                      pframe   = tocTrk.PFRAME,
                                                      mode     = AlcoholTrackMode.NoData,
                                                      unknown  = new byte[18],
                                                      unknown2 = new byte[24]
                                                  });
                            currentTrackOffset += Marshal.SizeOf(typeof(AlcoholTrack));
                        }
                    else if(i < sessions)
                    {
                        (byte minute, byte second, byte frame) leadoutAmsf =
                            LbaToMsf(writingTracks.First(t => t.TrackSession == i + 1).TrackStartSector - 150);
                        (byte minute, byte second, byte frame) leadoutPmsf =
                            LbaToMsf(writingTracks.OrderBy(t => t.TrackSession).ThenBy(t => t.TrackSequence).Last()
                                                  .TrackStartSector);

                        thisSessionTracks.Add(0xB0,
                                              new AlcoholTrack
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

                        currentTrackOffset += Marshal.SizeOf(typeof(AlcoholTrack)) * 2;
                    }

                    alcToc.Add(i, thisSessionTracks);
                }

            alcFooter = new AlcoholFooter
            {
                filenameOffset = (uint)(footerOffset + Marshal.SizeOf(typeof(AlcoholFooter))), widechar = 1
            };

            byte[] filename = Encoding.Unicode.GetBytes("*.mdf"); // Yup, Alcohol stores no filename but a wildcard.

            IntPtr blockPtr;

            // Write header
            descriptorStream.Seek(0, SeekOrigin.Begin);
            byte[] block = new byte[Marshal.SizeOf(header)];
            blockPtr = Marshal.AllocHGlobal(Marshal.SizeOf(header));
            Marshal.StructureToPtr(header, blockPtr, true);
            Marshal.Copy(blockPtr, block, 0, block.Length);
            Marshal.FreeHGlobal(blockPtr);
            descriptorStream.Write(block, 0, block.Length);

            // Write DVD structures if pressent
            if(header.structuresOffset != 0)
            {
                if(dmi != null)
                {
                    descriptorStream.Seek(header.structuresOffset, SeekOrigin.Begin);
                    if(dmi.Length      == 2052) descriptorStream.Write(dmi, 0, 2052);
                    else if(dmi.Length == 2048)
                    {
                        descriptorStream.Write(new byte[] {0x08, 0x02, 0x00, 0x00}, 0, 4);
                        descriptorStream.Write(dmi,                                 0, 2048);
                    }
                }

                // TODO: Create fake PFI if none present
                if(pfi != null)
                {
                    descriptorStream.Seek(header.structuresOffset + 2052, SeekOrigin.Begin);
                    descriptorStream.Write(pfi, pfi.Length        - 2048, 2048);
                }
            }

            // Write sessions
            descriptorStream.Seek(header.sessionOffset, SeekOrigin.Begin);
            foreach(AlcoholSession session in alcSessions.Values)
            {
                block    = new byte[Marshal.SizeOf(session)];
                blockPtr = Marshal.AllocHGlobal(Marshal.SizeOf(session));
                Marshal.StructureToPtr(session, blockPtr, true);
                Marshal.Copy(blockPtr, block, 0, block.Length);
                Marshal.FreeHGlobal(blockPtr);
                descriptorStream.Write(block, 0, block.Length);
            }

            // Write tracks
            foreach(KeyValuePair<int, Dictionary<int, AlcoholTrack>> kvp in alcToc)
            {
                descriptorStream.Seek(alcSessions.First(t => t.Key == kvp.Key).Value.trackOffset, SeekOrigin.Begin);
                foreach(AlcoholTrack track in kvp.Value.Values)
                {
                    block    = new byte[Marshal.SizeOf(track)];
                    blockPtr = Marshal.AllocHGlobal(Marshal.SizeOf(track));
                    Marshal.StructureToPtr(track, blockPtr, true);
                    Marshal.Copy(blockPtr, block, 0, block.Length);
                    Marshal.FreeHGlobal(blockPtr);
                    descriptorStream.Write(block, 0, block.Length);

                    if(isDvd) continue;

                    // Write extra
                    long position = descriptorStream.Position;
                    descriptorStream.Seek(track.extraOffset, SeekOrigin.Begin);
                    if(alcTrackExtras.TryGetValue(track.point, out AlcoholTrackExtra extra))
                    {
                        block    = new byte[Marshal.SizeOf(extra)];
                        blockPtr = Marshal.AllocHGlobal(Marshal.SizeOf(extra));
                        Marshal.StructureToPtr(extra, blockPtr, true);
                        Marshal.Copy(blockPtr, block, 0, block.Length);
                        Marshal.FreeHGlobal(blockPtr);
                        descriptorStream.Write(block, 0, block.Length);
                    }

                    descriptorStream.Seek(position, SeekOrigin.Begin);
                }
            }

            // Write BCA
            if(bca != null)
            {
                descriptorStream.Seek(header.bcaOffset, SeekOrigin.Begin);
                descriptorStream.Write(bca, 0, bca.Length);
            }

            // Write footer
            descriptorStream.Seek(footerOffset, SeekOrigin.Begin);
            block    = new byte[Marshal.SizeOf(alcFooter)];
            blockPtr = Marshal.AllocHGlobal(Marshal.SizeOf(alcFooter));
            Marshal.StructureToPtr(alcFooter, blockPtr, true);
            Marshal.Copy(blockPtr, block, 0, block.Length);
            Marshal.FreeHGlobal(blockPtr);
            descriptorStream.Write(block, 0, block.Length);

            // Write filename
            descriptorStream.Write(filename, 0, filename.Length);
            // Write filename null termination
            descriptorStream.Write(new byte[] {0, 0}, 0, 2);

            descriptorStream.Flush();
            descriptorStream.Close();
            imageStream.Flush();
            imageStream.Close();

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
                writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                    sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
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

                    trackFlags.Add((byte)track.TrackSequence, data[0]);

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

                    imageStream
                       .Seek((long)(track.TrackFileOffset + (sectorAddress - track.TrackStartSector) * (ulong)(track.TrackRawBytesPerSector + 96)) + track.TrackRawBytesPerSector,
                             SeekOrigin.Begin);
                    imageStream.Write(data, 0, data.Length);

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
                writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                    sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
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
                        imageStream
                           .Seek((long)(track.TrackFileOffset + (i + sectorAddress - track.TrackStartSector) * (ulong)(track.TrackRawBytesPerSector + 96)) + track.TrackRawBytesPerSector,
                                 SeekOrigin.Begin);
                        imageStream.Write(data, (int)(i * 96), 96);
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