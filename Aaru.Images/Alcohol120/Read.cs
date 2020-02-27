// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads Alcohol 120% disc images.
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
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Decoders.CD;
using Aaru.Decoders.DVD;
using Aaru.Helpers;
using DMI = Aaru.Decoders.Xbox.DMI;
using Session = Aaru.CommonTypes.Structs.Session;

namespace Aaru.DiscImages
{
    public partial class Alcohol120
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 88)
                return false;

            isDvd = false;
            byte[] hdr = new byte[88];
            stream.Read(hdr, 0, 88);
            AlcoholHeader header = Marshal.ByteArrayToStructureLittleEndian<AlcoholHeader>(hdr);

            DicConsole.DebugWriteLine("Alcohol 120% plugin", "header.signature = {0}",
                                      Encoding.ASCII.GetString(header.signature));

            DicConsole.DebugWriteLine("Alcohol 120% plugin", "header.version = {0}.{1}", header.version[0],
                                      header.version[1]);

            DicConsole.DebugWriteLine("Alcohol 120% plugin", "header.type = {0}", header.type);
            DicConsole.DebugWriteLine("Alcohol 120% plugin", "header.sessions = {0}", header.sessions);

            for(int i = 0; i < header.unknown1.Length; i++)
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "header.unknown1[{1}] = 0x{0:X4}", header.unknown1[i],
                                          i);

            DicConsole.DebugWriteLine("Alcohol 120% plugin", "header.bcaLength = {0}", header.bcaLength);

            for(int i = 0; i < header.unknown2.Length; i++)
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "header.unknown2[{1}] = 0x{0:X8}", header.unknown2[i],
                                          i);

            DicConsole.DebugWriteLine("Alcohol 120% plugin", "header.bcaOffset = {0}", header.bcaOffset);

            for(int i = 0; i < header.unknown3.Length; i++)
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "header.unknown3[{1}] = 0x{0:X8}", header.unknown3[i],
                                          i);

            DicConsole.DebugWriteLine("Alcohol 120% plugin", "header.structuresOffset = {0}", header.structuresOffset);

            for(int i = 0; i < header.unknown4.Length; i++)
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "header.unknown4[{1}] = 0x{0:X8}", header.unknown4[i],
                                          i);

            DicConsole.DebugWriteLine("Alcohol 120% plugin", "header.sessionOffset = {0}", header.sessionOffset);
            DicConsole.DebugWriteLine("Alcohol 120% plugin", "header.dpmOffset = {0}", header.dpmOffset);

            stream.Seek(header.sessionOffset, SeekOrigin.Begin);
            alcSessions = new Dictionary<int, AlcoholSession>();

            for(int i = 0; i < header.sessions; i++)
            {
                byte[] sesHdr = new byte[24];
                stream.Read(sesHdr, 0, 24);
                AlcoholSession session = Marshal.SpanToStructureLittleEndian<AlcoholSession>(sesHdr);

                DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{1}].sessionStart = {0}",
                                          session.sessionStart, i);

                DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{1}].sessionEnd = {0}", session.sessionEnd,
                                          i);

                DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{1}].sessionSequence = {0}",
                                          session.sessionSequence, i);

                DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{1}].allBlocks = {0}", session.allBlocks, i);

                DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{1}].nonTrackBlocks = {0}",
                                          session.nonTrackBlocks, i);

                DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{1}].firstTrack = {0}", session.firstTrack,
                                          i);

                DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{1}].lastTrack = {0}", session.lastTrack, i);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{1}].unknown = 0x{0:X8}", session.unknown, i);

                DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{1}].trackOffset = {0}", session.trackOffset,
                                          i);

                alcSessions.Add(session.sessionSequence, session);
            }

            long footerOff         = 0;
            bool oldIncorrectImage = false;

            alcTracks = new Dictionary<int, AlcoholTrack>();
            alcToc    = new Dictionary<int, Dictionary<int, AlcoholTrack>>();

            foreach(AlcoholSession session in alcSessions.Values)
            {
                stream.Seek(session.trackOffset, SeekOrigin.Begin);
                Dictionary<int, AlcoholTrack> sesToc = new Dictionary<int, AlcoholTrack>();

                for(int i = 0; i < session.allBlocks; i++)
                {
                    byte[] trkHdr = new byte[80];
                    stream.Read(trkHdr, 0, 80);
                    AlcoholTrack track = Marshal.ByteArrayToStructureLittleEndian<AlcoholTrack>(trkHdr);

                    if(track.mode == AlcoholTrackMode.Mode2F1Alt ||
                       track.mode == AlcoholTrackMode.Mode2F1Alt)
                        oldIncorrectImage = true;

                    // Solve our own mistake here, sorry, but anyway seems Alcohol doesn't support DDCD
                    if(track.zero > 0)
                    {
                        track.pmin        += (byte)(track.zero * 60);
                        track.zero        =  0;
                        oldIncorrectImage =  true;
                    }

                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].mode = {0}", track.mode,
                                              track.point, session.sessionSequence);

                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].subMode = {0}",
                                              track.subMode, track.point, session.sessionSequence);

                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].adrCtl = {0}",
                                              track.adrCtl, track.point, session.sessionSequence);

                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].tno = {0}", track.tno,
                                              track.point, session.sessionSequence);

                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].point = {0:X2}",
                                              track.point, track.point, session.sessionSequence);

                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].min = {0}", track.min,
                                              track.point, session.sessionSequence);

                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].sec = {0}", track.sec,
                                              track.point, session.sessionSequence);

                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].frame = {0}", track.frame,
                                              track.point, session.sessionSequence);

                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].zero = {0}", track.zero,
                                              track.point, session.sessionSequence);

                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].pmin = {0}", track.pmin,
                                              track.point, session.sessionSequence);

                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].psec = {0}", track.psec,
                                              track.point, session.sessionSequence);

                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].pframe = {0}",
                                              track.pframe, track.point, session.sessionSequence);

                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].extraOffset = {0}",
                                              track.extraOffset, track.point, session.sessionSequence);

                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].sectorSize = {0}",
                                              track.sectorSize, track.point, session.sessionSequence);

                    //for(int j = 0; j < track.unknown.Length; j++)
                    //    DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].unknown[{2}] = {0}", track.unknown[j], i, j, session.sessionSequence);
                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].startLba = {0}",
                                              track.startLba, track.point, session.sessionSequence);

                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].startOffset = {0}",
                                              track.startOffset, track.point, session.sessionSequence);

                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].files = {0}", track.files,
                                              track.point, session.sessionSequence);

                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].footerOffset = {0}",
                                              track.footerOffset, track.point, session.sessionSequence);

                    //for(int j = 0; j < track.unknown2.Length; j++)
                    //    DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].unknown2[{2}] = {0}", track.unknown2[j], i, j, session.sessionSequence);

                    if(track.subMode == AlcoholSubchannelMode.Interleaved)
                        track.sectorSize -= 96;

                    if(!sesToc.ContainsKey(track.point))
                        sesToc.Add(track.point, track);

                    if(track.point < 0xA0)
                        alcTracks.Add(track.point, track);

                    if(footerOff == 0)
                        footerOff = track.footerOffset;

                    isDvd |= track.mode == AlcoholTrackMode.DVD;
                }

                alcToc.Add(session.sessionSequence, sesToc);
            }

            alcTrackExtras = new Dictionary<int, AlcoholTrackExtra>();

            foreach(AlcoholTrack track in alcTracks.Values)
                if(track.extraOffset > 0 &&
                   !isDvd)
                {
                    byte[] extHdr = new byte[8];
                    stream.Seek(track.extraOffset, SeekOrigin.Begin);
                    stream.Read(extHdr, 0, 8);
                    AlcoholTrackExtra extra = Marshal.SpanToStructureLittleEndian<AlcoholTrackExtra>(extHdr);

                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "track[{1}].extra.pregap = {0}", extra.pregap,
                                              track.point);

                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "track[{1}].extra.sectors = {0}", extra.sectors,
                                              track.point);

                    alcTrackExtras.Add(track.point, extra);
                }
                else if(isDvd)
                {
                    var extra = new AlcoholTrackExtra
                    {
                        sectors = track.extraOffset
                    };

                    alcTrackExtras.Add(track.point, extra);
                }

            if(footerOff > 0)
            {
                byte[] footer = new byte[16];
                stream.Seek(footerOff, SeekOrigin.Begin);
                stream.Read(footer, 0, 16);
                alcFooter = Marshal.SpanToStructureLittleEndian<AlcoholFooter>(footer);

                DicConsole.DebugWriteLine("Alcohol 120% plugin", "footer.filenameOffset = {0}",
                                          alcFooter.filenameOffset);

                DicConsole.DebugWriteLine("Alcohol 120% plugin", "footer.widechar = {0}", alcFooter.widechar);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "footer.unknown1 = 0x{0:X8}", alcFooter.unknown1);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "footer.unknown2 = 0x{0:X8}", alcFooter.unknown2);
            }

            string alcFile = "*.mdf";

            if(alcFooter.filenameOffset > 0)
            {
                stream.Seek(alcFooter.filenameOffset, SeekOrigin.Begin);

                byte[] filename = header.dpmOffset == 0 ? new byte[stream.Length - stream.Position]
                                      : new byte[header.dpmOffset                - stream.Position];

                stream.Read(filename, 0, filename.Length);

                alcFile = alcFooter.widechar == 1 ? Encoding.Unicode.GetString(filename)
                              : Encoding.Default.GetString(filename);

                DicConsole.DebugWriteLine("Alcohol 120% plugin", "footer.filename = {0}", alcFile);
            }

            if(alcFooter.filenameOffset                                                      == 0 ||
               string.Compare(alcFile, "*.mdf", StringComparison.InvariantCultureIgnoreCase) == 0)
                alcFile = Path.GetFileNameWithoutExtension(imageFilter.GetBasePath()) + ".mdf";

            if(header.bcaLength > 0 &&
               header.bcaOffset > 0 &&
               isDvd)
            {
                bca = new byte[header.bcaLength];
                stream.Seek(header.bcaOffset, SeekOrigin.Begin);
                int readBytes = stream.Read(bca, 0, bca.Length);

                if(readBytes == bca.Length)
                    switch(header.type)
                    {
                        case AlcoholMediumType.DVD:
                        case AlcoholMediumType.DVDR:
                            imageInfo.ReadableMediaTags.Add(MediaTagType.DVD_BCA);

                            break;
                    }
            }

            imageInfo.MediaType = AlcoholMediumTypeToMediaType(header.type);

            Sessions = new List<Session>();

            foreach(AlcoholSession alcSes in alcSessions.Values)
            {
                var session = new Session();

                if(!alcTracks.TryGetValue(alcSes.firstTrack, out AlcoholTrack startingTrack))
                    break;

                if(!alcTracks.TryGetValue(alcSes.lastTrack, out AlcoholTrack endingTrack))
                    break;

                if(!alcTrackExtras.TryGetValue(alcSes.lastTrack, out AlcoholTrackExtra endingTrackExtra))
                    break;

                session.StartSector     = startingTrack.startLba;
                session.StartTrack      = alcSes.firstTrack;
                session.SessionSequence = alcSes.sessionSequence;
                session.EndSector       = (endingTrack.startLba + endingTrackExtra.sectors) - 1;
                session.EndTrack        = alcSes.lastTrack;

                Sessions.Add(session);

                if(session.EndSector > imageInfo.Sectors)
                    imageInfo.Sectors = session.EndSector + 1;
            }

            if(isDvd)
            {
                // TODO: Second layer
                if(header.structuresOffset > 0)
                {
                    byte[] structures = new byte[4100];
                    stream.Seek(header.structuresOffset, SeekOrigin.Begin);
                    stream.Read(structures, 0, 4100);
                    dmi = new byte[2052];
                    pfi = new byte[2052];

                    Array.Copy(structures, 0, dmi, 0, 2052);
                    Array.Copy(structures, 0x804, pfi, 4, 2048);

                    pfi[0] = 0x08;
                    pfi[1] = 0x02;
                    dmi[0] = 0x08;
                    dmi[1] = 0x02;

                    PFI.PhysicalFormatInformation? pfi0 = PFI.Decode(pfi);

                    // All discs I tested the disk category and part version (as well as the start PSN for DVD-RAM) where modified by Alcohol
                    // So much for archival value
                    if(pfi0.HasValue)
                    {
                        switch(pfi0.Value.DiskCategory)
                        {
                            case DiskCategory.DVDPR:
                                imageInfo.MediaType = MediaType.DVDPR;

                                break;
                            case DiskCategory.DVDPRDL:
                                imageInfo.MediaType = MediaType.DVDPRDL;

                                break;
                            case DiskCategory.DVDPRW:
                                imageInfo.MediaType = MediaType.DVDPRW;

                                break;
                            case DiskCategory.DVDPRWDL:
                                imageInfo.MediaType = MediaType.DVDPRWDL;

                                break;
                            case DiskCategory.DVDR:
                                imageInfo.MediaType = pfi0.Value.PartVersion == 6 ? MediaType.DVDRDL : MediaType.DVDR;

                                break;
                            case DiskCategory.DVDRAM:
                                imageInfo.MediaType = MediaType.DVDRAM;

                                break;
                            default:
                                imageInfo.MediaType = MediaType.DVDROM;

                                break;
                            case DiskCategory.DVDRW:
                                imageInfo.MediaType = pfi0.Value.PartVersion == 3 ? MediaType.DVDRWDL : MediaType.DVDRW;

                                break;
                            case DiskCategory.HDDVDR:
                                imageInfo.MediaType = MediaType.HDDVDR;

                                break;
                            case DiskCategory.HDDVDRAM:
                                imageInfo.MediaType = MediaType.HDDVDRAM;

                                break;
                            case DiskCategory.HDDVDROM:
                                imageInfo.MediaType = MediaType.HDDVDROM;

                                break;
                            case DiskCategory.HDDVDRW:
                                imageInfo.MediaType = MediaType.HDDVDRW;

                                break;
                            case DiskCategory.Nintendo:
                                imageInfo.MediaType =
                                    pfi0.Value.DiscSize == DVDSize.Eighty ? MediaType.GOD : MediaType.WOD;

                                break;
                            case DiskCategory.UMD:
                                imageInfo.MediaType = MediaType.UMD;

                                break;
                        }

                        if(DMI.IsXbox(dmi))
                            imageInfo.MediaType = MediaType.XGD;
                        else if(DMI.IsXbox360(dmi))
                            imageInfo.MediaType = MediaType.XGD2;

                        imageInfo.ReadableMediaTags.Add(MediaTagType.DVD_PFI);
                        imageInfo.ReadableMediaTags.Add(MediaTagType.DVD_DMI);
                    }
                }
            }
            else if(header.type == AlcoholMediumType.CD)
            {
                bool data       = false;
                bool mode2      = false;
                bool firstaudio = false;
                bool firstdata  = false;
                bool audio      = false;

                foreach(AlcoholTrack alcoholTrack in alcTracks.Values)
                {
                    // First track is audio
                    firstaudio |= alcoholTrack.point == 1 && alcoholTrack.mode == AlcoholTrackMode.Audio;

                    // First track is data
                    firstdata |= alcoholTrack.point == 1 && alcoholTrack.mode != AlcoholTrackMode.Audio;

                    // Any non first track is data
                    data |= alcoholTrack.point != 1 && alcoholTrack.mode != AlcoholTrackMode.Audio;

                    // Any non first track is audio
                    audio |= alcoholTrack.point != 1 && alcoholTrack.mode == AlcoholTrackMode.Audio;

                    switch(alcoholTrack.mode)
                    {
                        case AlcoholTrackMode.Mode2:
                        case AlcoholTrackMode.Mode2F1:
                        case AlcoholTrackMode.Mode2F2:
                        case AlcoholTrackMode.Mode2F1Alt:
                        case AlcoholTrackMode.Mode2F2Alt:
                            mode2 = true;

                            break;
                    }
                }

                if(!data &&
                   !firstdata)
                    imageInfo.MediaType = MediaType.CDDA;
                else if(firstaudio         &&
                        data               &&
                        Sessions.Count > 1 &&
                        mode2)
                    imageInfo.MediaType = MediaType.CDPLUS;
                else if((firstdata && audio) || mode2)
                    imageInfo.MediaType = MediaType.CDROMXA;
                else if(!audio)
                    imageInfo.MediaType = MediaType.CDROM;
                else
                    imageInfo.MediaType = MediaType.CD;
            }

            DicConsole.DebugWriteLine("Alcohol 120% plugin", "ImageInfo.mediaType = {0}", imageInfo.MediaType);

            Partitions = new List<Partition>();
            offsetmap  = new Dictionary<uint, ulong>();
            ulong byteOffset = 0;

            foreach(AlcoholTrack trk in alcTracks.Values)
            {
                if(alcTrackExtras.TryGetValue(trk.point, out AlcoholTrackExtra extra))
                {
                    var partition = new Partition
                    {
                        Description = $"Track {trk.point}.", Start           = trk.startLba,
                        Size        = extra.sectors * trk.sectorSize, Length = extra.sectors, Sequence = trk.point,
                        Offset      = byteOffset, Type                       = trk.mode.ToString()
                    };

                    Partitions.Add(partition);
                    byteOffset += partition.Size;
                }

                if(!offsetmap.ContainsKey(trk.point))
                    offsetmap.Add(trk.point, trk.startLba);

                switch(trk.mode)
                {
                    case AlcoholTrackMode.Mode1:
                    case AlcoholTrackMode.Mode2F1:
                    case AlcoholTrackMode.Mode2F1Alt:
                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);

                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEcc))
                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEcc);

                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccP))
                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccP);

                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccQ))
                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccQ);

                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);

                        if(imageInfo.SectorSize < 2048)
                            imageInfo.SectorSize = 2048;

                        break;
                    case AlcoholTrackMode.Mode2:
                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                        if(imageInfo.SectorSize < 2336)
                            imageInfo.SectorSize = 2336;

                        break;
                    case AlcoholTrackMode.Mode2F2:
                    case AlcoholTrackMode.Mode2F2Alt:
                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);

                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);

                        if(imageInfo.SectorSize < 2324)
                            imageInfo.SectorSize = 2324;

                        break;
                    case AlcoholTrackMode.DVD:
                        imageInfo.SectorSize = 2048;

                        break;
                    default:
                        imageInfo.SectorSize = 2352;

                        break;
                }

                if(trk.subMode != AlcoholSubchannelMode.None &&
                   !imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);
            }

            DicConsole.DebugWriteLine("Alcohol 120% plugin", "printing partition map");

            foreach(Partition partition in Partitions)
            {
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "Partition sequence: {0}", partition.Sequence);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "\tPartition name: {0}", partition.Name);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "\tPartition description: {0}", partition.Description);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "\tPartition type: {0}", partition.Type);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "\tPartition starting sector: {0}", partition.Start);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "\tPartition sectors: {0}", partition.Length);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "\tPartition starting offset: {0}", partition.Offset);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "\tPartition size in bytes: {0}", partition.Size);
            }

            imageInfo.Application = "Alcohol 120%";

            DicConsole.DebugWriteLine("Alcohol 120% plugin", "Data filename: {0}", alcFile);

            var filtersList = new FiltersList();
            alcImage = filtersList.GetFilter(alcFile);

            if(alcImage == null)
                throw new Exception("Cannot open data file");

            imageInfo.ImageSize            = (ulong)alcImage.GetDataForkLength();
            imageInfo.CreationTime         = alcImage.GetCreationTime();
            imageInfo.LastModificationTime = alcImage.GetLastWriteTime();
            imageInfo.XmlMediaType         = XmlMediaType.OpticalDisc;
            imageInfo.Version              = $"{header.version[0]}.{header.version[1]}";

            if(!isDvd)
            {
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "Rebuilding TOC");
                byte firstSession = byte.MaxValue;
                byte lastSession  = 0;
                var  tocMs        = new MemoryStream();

                tocMs.Write(new byte[]
                {
                    0, 0, 0, 0
                }, 0, 4); // Reserved for TOC response size and session numbers

                foreach(KeyValuePair<int, Dictionary<int, AlcoholTrack>> sessionToc in alcToc)
                {
                    if(sessionToc.Key < firstSession)
                        firstSession = (byte)sessionToc.Key;

                    if(sessionToc.Key > lastSession)
                        lastSession = (byte)sessionToc.Key;

                    foreach(AlcoholTrack sessionTrack in sessionToc.Value.Values)
                    {
                        tocMs.WriteByte((byte)sessionToc.Key);
                        tocMs.WriteByte(sessionTrack.adrCtl);
                        tocMs.WriteByte(sessionTrack.tno);
                        tocMs.WriteByte(sessionTrack.point);
                        tocMs.WriteByte(sessionTrack.min);
                        tocMs.WriteByte(sessionTrack.sec);
                        tocMs.WriteByte(sessionTrack.frame);
                        tocMs.WriteByte(sessionTrack.zero);
                        tocMs.WriteByte(sessionTrack.pmin);
                        tocMs.WriteByte(sessionTrack.psec);
                        tocMs.WriteByte(sessionTrack.pframe);
                    }
                }

                fullToc = tocMs.ToArray();
                byte[] fullTocSize = BigEndianBitConverter.GetBytes((short)(fullToc.Length - 2));
                fullToc[0] = fullTocSize[0];
                fullToc[1] = fullTocSize[1];
                fullToc[2] = firstSession;
                fullToc[3] = lastSession;

                FullTOC.CDFullTOC? decodedFullToc = FullTOC.Decode(fullToc);

                if(!decodedFullToc.HasValue)
                {
                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "TOC not correctly rebuilt");
                    fullToc = null;
                }
                else
                    imageInfo.ReadableMediaTags.Add(MediaTagType.CD_FullTOC);

                imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackFlags);
            }

            if(imageInfo.MediaType == MediaType.XGD2)
                if(imageInfo.Sectors == 25063   || // Locked (or non compatible drive)
                   imageInfo.Sectors == 4229664 || // Xtreme unlock
                   imageInfo.Sectors == 4246304)   // Wxripper unlock
                    imageInfo.MediaType = MediaType.XGD3;

            DicConsole.VerboseWriteLine("Alcohol 120% image describes a disc of type {0}", imageInfo.MediaType);

            if(oldIncorrectImage)
                DicConsole.WriteLine("Incorrect Alcohol 120% image created by an old version of Aaru. Convert image to correct it.");

            return true;
        }

        public byte[] ReadDiskTag(MediaTagType tag)
        {
            switch(tag)
            {
                case MediaTagType.DVD_BCA:
                {
                    if(bca != null)
                        return(byte[])bca.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain BCA information.");
                }

                case MediaTagType.DVD_PFI:
                {
                    if(pfi != null)
                        return(byte[])pfi.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain PFI.");
                }

                case MediaTagType.DVD_DMI:
                {
                    if(dmi != null)
                        return(byte[])dmi.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain DMI.");
                }

                case MediaTagType.CD_FullTOC:
                {
                    if(fullToc != null)
                        return(byte[])fullToc.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain TOC information.");
                }

                default:
                    throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
            }
        }

        public byte[] ReadSector(ulong sectorAddress) => ReadSectors(sectorAddress, 1);

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag) => ReadSectorsTag(sectorAddress, 1, tag);

        public byte[] ReadSector(ulong sectorAddress, uint track) => ReadSectors(sectorAddress, 1, track);

        public byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag) =>
            ReadSectorsTag(sectorAddress, 1, track, tag);

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in offsetmap)
                if(sectorAddress >= kvp.Value)
                    foreach(AlcoholTrack track in alcTracks.Values)
                    {
                        if(track.point != kvp.Key ||
                           !alcTrackExtras.TryGetValue(track.point, out AlcoholTrackExtra extra))
                            continue;

                        if(sectorAddress - kvp.Value < extra.sectors)
                            return ReadSectors(sectorAddress - kvp.Value, length, kvp.Key);
                    }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            foreach(KeyValuePair<uint, ulong> kvp in offsetmap)
                if(sectorAddress >= kvp.Value)
                    foreach(AlcoholTrack track in alcTracks.Values)
                    {
                        if(track.point != kvp.Key ||
                           !alcTrackExtras.TryGetValue(track.point, out AlcoholTrackExtra extra))
                            continue;

                        if(sectorAddress - kvp.Value < extra.sectors)
                            return ReadSectorsTag(sectorAddress - kvp.Value, length, kvp.Key, tag);
                    }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            if(!alcTracks.TryGetValue((int)track, out AlcoholTrack alcTrack) ||
               !alcTrackExtras.TryGetValue((int)track, out AlcoholTrackExtra alcExtra))
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > alcExtra.sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({alcExtra.sectors}), won't cross tracks");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;
            bool mode2 = false;

            switch(alcTrack.mode)
            {
                case AlcoholTrackMode.Mode1:
                {
                    sectorOffset = 16;
                    sectorSize   = 2048;
                    sectorSkip   = 288;

                    break;
                }

                case AlcoholTrackMode.Mode2:
                case AlcoholTrackMode.Mode2F1:
                case AlcoholTrackMode.Mode2F1Alt:
                case AlcoholTrackMode.Mode2F2:
                case AlcoholTrackMode.Mode2F2Alt:
                {
                    mode2        = true;
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 0;

                    break;
                }

                case AlcoholTrackMode.Audio:
                {
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 0;

                    break;
                }

                case AlcoholTrackMode.DVD:
                {
                    sectorOffset = 0;
                    sectorSize   = 2048;
                    sectorSkip   = 0;

                    break;
                }

                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            switch(alcTrack.subMode)
            {
                case AlcoholSubchannelMode.None:
                    sectorSkip += 0;

                    break;
                case AlcoholSubchannelMode.Interleaved:
                    sectorSkip += 96;

                    break;
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported subchannel type");
            }

            byte[] buffer = new byte[sectorSize * length];

            imageStream = alcImage.GetDataForkStream();
            var br = new BinaryReader(imageStream);

            br.BaseStream.
               Seek((long)alcTrack.startOffset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                    SeekOrigin.Begin);

            if(mode2)
            {
                var mode2Ms = new MemoryStream((int)(sectorSize * length));

                buffer = br.ReadBytes((int)((sectorSize + sectorSkip) * length));

                for(int i = 0; i < length; i++)
                {
                    byte[] sector = new byte[sectorSize];
                    Array.Copy(buffer, (sectorSize + sectorSkip) * i, sector, 0, sectorSize);
                    sector = Sector.GetUserDataFromMode2(sector);
                    mode2Ms.Write(sector, 0, sector.Length);
                }

                buffer = mode2Ms.ToArray();
            }
            else if(sectorOffset == 0 &&
                    sectorSkip   == 0)
                buffer = br.ReadBytes((int)(sectorSize * length));
            else
                for(int i = 0; i < length; i++)
                {
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    byte[] sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }

            return buffer;
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            if(!alcTracks.TryGetValue((int)track, out AlcoholTrack alcTrack) ||
               !alcTrackExtras.TryGetValue((int)track, out AlcoholTrackExtra alcExtra))
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > alcExtra.sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length}) than present in track ({alcExtra.sectors}), won't cross tracks");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            if(alcTrack.mode == AlcoholTrackMode.DVD)
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
                    return new[]
                    {
                        (byte)(alcTrack.adrCtl & 0x0F)
                    };
                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
            }

            switch(alcTrack.mode)
            {
                case AlcoholTrackMode.Mode1:
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
                        {
                            switch(alcTrack.subMode)
                            {
                                case AlcoholSubchannelMode.Interleaved:

                                    sectorOffset = 2352;
                                    sectorSize   = 96;
                                    sectorSkip   = 0;

                                    break;
                                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                            }

                            break;
                        }

                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                case AlcoholTrackMode.Mode2:
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
                        {
                            switch(alcTrack.subMode)
                            {
                                case AlcoholSubchannelMode.Interleaved:

                                    sectorOffset = 2352;
                                    sectorSize   = 96;
                                    sectorSkip   = 0;

                                    break;
                                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                            }

                            break;
                        }

                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                }

                case AlcoholTrackMode.Mode2F1:
                case AlcoholTrackMode.Mode2F1Alt:
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
                        {
                            sectorOffset = 16;
                            sectorSize   = 8;
                            sectorSkip   = 2328;

                            break;
                        }

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
                            sectorOffset = 2072;
                            sectorSize   = 4;
                            sectorSkip   = 276;

                            break;
                        }

                        case SectorTagType.CdSectorSubchannel:
                        {
                            switch(alcTrack.subMode)
                            {
                                case AlcoholSubchannelMode.Interleaved:

                                    sectorOffset = 2352;
                                    sectorSize   = 96;
                                    sectorSkip   = 0;

                                    break;
                                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                            }

                            break;
                        }

                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                case AlcoholTrackMode.Mode2F2:
                case AlcoholTrackMode.Mode2F2Alt:
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
                        {
                            sectorOffset = 16;
                            sectorSize   = 8;
                            sectorSkip   = 2328;

                            break;
                        }

                        case SectorTagType.CdSectorEdc:
                        {
                            sectorOffset = 2348;
                            sectorSize   = 4;
                            sectorSkip   = 0;

                            break;
                        }

                        case SectorTagType.CdSectorSubchannel:
                        {
                            switch(alcTrack.subMode)
                            {
                                case AlcoholSubchannelMode.Interleaved:

                                    sectorOffset = 2352;
                                    sectorSize   = 96;
                                    sectorSkip   = 0;

                                    break;
                                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                            }

                            break;
                        }

                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                case AlcoholTrackMode.Audio:
                {
                    switch(tag)
                    {
                        case SectorTagType.CdSectorSubchannel:
                        {
                            switch(alcTrack.subMode)
                            {
                                case AlcoholSubchannelMode.Interleaved:

                                    sectorOffset = 2352;
                                    sectorSize   = 96;
                                    sectorSkip   = 0;

                                    break;
                                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                            }

                            break;
                        }

                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                }

                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            switch(alcTrack.subMode)
            {
                case AlcoholSubchannelMode.None:
                    sectorSkip += 0;

                    break;
                case AlcoholSubchannelMode.Interleaved:
                    if(tag != SectorTagType.CdSectorSubchannel)
                        sectorSkip += 96;

                    break;
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported subchannel type");
            }

            byte[] buffer = new byte[sectorSize * length];

            imageStream = alcImage.GetDataForkStream();
            var br = new BinaryReader(imageStream);

            br.BaseStream.
               Seek((long)alcTrack.startOffset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                    SeekOrigin.Begin);

            if(sectorOffset == 0 &&
               sectorSkip   == 0)
                buffer = br.ReadBytes((int)(sectorSize * length));
            else
                for(int i = 0; i < length; i++)
                {
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    byte[] sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }

            return buffer;
        }

        public byte[] ReadSectorLong(ulong sectorAddress) => ReadSectorsLong(sectorAddress, 1);

        public byte[] ReadSectorLong(ulong sectorAddress, uint track) => ReadSectorsLong(sectorAddress, 1, track);

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in offsetmap)
                if(sectorAddress >= kvp.Value)
                    foreach(AlcoholTrack alcTrack in alcTracks.Values)
                    {
                        if(alcTrack.point != kvp.Key ||
                           !alcTrackExtras.TryGetValue(alcTrack.point, out AlcoholTrackExtra alcExtra))
                            continue;

                        if(sectorAddress - kvp.Value < alcExtra.sectors)
                            return ReadSectorsLong(sectorAddress - kvp.Value, length, kvp.Key);
                    }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            if(!alcTracks.TryGetValue((int)track, out AlcoholTrack alcTrack) ||
               !alcTrackExtras.TryGetValue((int)track, out AlcoholTrackExtra alcExtra))
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > alcExtra.sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length}) than present in track ({alcExtra.sectors}), won't cross tracks");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch(alcTrack.mode)
            {
                case AlcoholTrackMode.Mode1:
                case AlcoholTrackMode.Mode2:
                case AlcoholTrackMode.Mode2F1:
                case AlcoholTrackMode.Mode2F1Alt:
                case AlcoholTrackMode.Mode2F2:
                case AlcoholTrackMode.Mode2F2Alt:
                case AlcoholTrackMode.Audio:
                case AlcoholTrackMode.DVD:
                {
                    sectorOffset = 0;
                    sectorSize   = alcTrack.sectorSize;
                    sectorSkip   = 0;

                    break;
                }

                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            if(alcTrack.subMode == AlcoholSubchannelMode.Interleaved)
                sectorSkip = 96;

            byte[] buffer = new byte[sectorSize * length];

            imageStream = alcImage.GetDataForkStream();
            var br = new BinaryReader(imageStream);

            br.BaseStream.
               Seek((long)alcTrack.startOffset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                    SeekOrigin.Begin);

            if(sectorOffset == 0 &&
               sectorSkip   == 0)
                buffer = br.ReadBytes((int)(sectorSize * length));
            else
                for(int i = 0; i < length; i++)
                {
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    byte[] sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);

                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }

            return buffer;
        }

        public List<Track> GetSessionTracks(Session session)
        {
            if(Sessions.Contains(session))
                return GetSessionTracks(session.SessionSequence);

            throw new ImageNotSupportedException("Session does not exist in disc image");
        }

        public List<Track> GetSessionTracks(ushort session)
        {
            List<Track> tracks = new List<Track>();

            foreach(AlcoholTrack alcTrack in alcTracks.Values)
            {
                ushort sessionNo =
                    (from ses in Sessions where alcTrack.point >= ses.StartTrack || alcTrack.point <= ses.EndTrack
                     select ses.SessionSequence).FirstOrDefault();

                if(!alcTrackExtras.TryGetValue(alcTrack.point, out AlcoholTrackExtra alcExtra) ||
                   session != sessionNo)
                    continue;

                var dicTrack = new Track
                {
                    Indexes = new Dictionary<int, ulong>
                    {
                        {
                            1, alcTrack.startLba
                        }
                    },
                    TrackStartSector    = alcTrack.startLba,
                    TrackEndSector      = alcExtra.sectors - 1,
                    TrackPregap         = alcExtra.pregap, TrackSession = sessionNo,
                    TrackSequence       = alcTrack.point,
                    TrackType           = AlcoholTrackTypeToTrackType(alcTrack.mode), TrackFilter = alcImage,
                    TrackFile           = alcImage.GetFilename(),
                    TrackFileOffset     = alcTrack.startOffset,
                    TrackFileType       = "BINARY", TrackRawBytesPerSector = alcTrack.sectorSize,
                    TrackBytesPerSector = AlcoholTrackModeToCookedBytesPerSector(alcTrack.mode)
                };

                switch(alcTrack.subMode)
                {
                    case AlcoholSubchannelMode.Interleaved:
                        dicTrack.TrackSubchannelFilter = alcImage;
                        dicTrack.TrackSubchannelFile   = alcImage.GetFilename();
                        dicTrack.TrackSubchannelOffset = alcTrack.startOffset;
                        dicTrack.TrackSubchannelType   = TrackSubchannelType.RawInterleaved;

                        break;
                    case AlcoholSubchannelMode.None:
                        dicTrack.TrackSubchannelType = TrackSubchannelType.None;

                        break;
                }

                tracks.Add(dicTrack);
            }

            return tracks;
        }
    }
}