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

            _isDvd = false;
            byte[] hdr = new byte[88];
            stream.Read(hdr, 0, 88);
            _header = Marshal.ByteArrayToStructureLittleEndian<AlcoholHeader>(hdr);

            AaruConsole.DebugWriteLine("Alcohol 120% plugin", "header.signature = {0}",
                                       Encoding.ASCII.GetString(_header.signature));

            AaruConsole.DebugWriteLine("Alcohol 120% plugin", "header.version = {0}.{1}", _header.version[0],
                                       _header.version[1]);

            AaruConsole.DebugWriteLine("Alcohol 120% plugin", "header.type = {0}", _header.type);
            AaruConsole.DebugWriteLine("Alcohol 120% plugin", "header.sessions = {0}", _header.sessions);

            for(int i = 0; i < _header.unknown1.Length; i++)
                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "header.unknown1[{1}] = 0x{0:X4}",
                                           _header.unknown1[i], i);

            AaruConsole.DebugWriteLine("Alcohol 120% plugin", "header.bcaLength = {0}", _header.bcaLength);

            for(int i = 0; i < _header.unknown2.Length; i++)
                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "header.unknown2[{1}] = 0x{0:X8}",
                                           _header.unknown2[i], i);

            AaruConsole.DebugWriteLine("Alcohol 120% plugin", "header.bcaOffset = {0}", _header.bcaOffset);

            for(int i = 0; i < _header.unknown3.Length; i++)
                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "header.unknown3[{1}] = 0x{0:X8}",
                                           _header.unknown3[i], i);

            AaruConsole.DebugWriteLine("Alcohol 120% plugin", "header.structuresOffset = {0}",
                                       _header.structuresOffset);

            for(int i = 0; i < _header.unknown4.Length; i++)
                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "header.unknown4[{1}] = 0x{0:X8}",
                                           _header.unknown4[i], i);

            AaruConsole.DebugWriteLine("Alcohol 120% plugin", "header.sessionOffset = {0}", _header.sessionOffset);
            AaruConsole.DebugWriteLine("Alcohol 120% plugin", "header.dpmOffset = {0}", _header.dpmOffset);

            if(_header.version[0] > _maximumSupportedVersion)
                return false;

            stream.Seek(_header.sessionOffset, SeekOrigin.Begin);
            _alcSessions = new Dictionary<int, AlcoholSession>();

            for(int i = 0; i < _header.sessions; i++)
            {
                byte[] sesHdr = new byte[24];
                stream.Read(sesHdr, 0, 24);
                AlcoholSession session = Marshal.SpanToStructureLittleEndian<AlcoholSession>(sesHdr);

                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{1}].sessionStart = {0}",
                                           session.sessionStart, i);

                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{1}].sessionEnd = {0}", session.sessionEnd,
                                           i);

                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{1}].sessionSequence = {0}",
                                           session.sessionSequence, i);

                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{1}].allBlocks = {0}", session.allBlocks, i);

                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{1}].nonTrackBlocks = {0}",
                                           session.nonTrackBlocks, i);

                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{1}].firstTrack = {0}", session.firstTrack,
                                           i);

                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{1}].lastTrack = {0}", session.lastTrack, i);

                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{1}].unknown = 0x{0:X8}", session.unknown,
                                           i);

                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{1}].trackOffset = {0}", session.trackOffset,
                                           i);

                _alcSessions.Add(session.sessionSequence, session);
            }

            long footerOff         = 0;
            bool oldIncorrectImage = false;

            _alcTracks = new Dictionary<int, AlcoholTrack>();
            _alcToc    = new Dictionary<int, Dictionary<int, AlcoholTrack>>();

            foreach(AlcoholSession session in _alcSessions.Values)
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
                    if(track.zero  > 0  &&
                       track.point >= 1 &&
                       track.point <= 99)
                    {
                        track.pmin        += (byte)(track.zero * 60);
                        track.zero        =  0;
                        oldIncorrectImage =  true;
                    }

                    AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].mode = {0}", track.mode,
                                               track.point, session.sessionSequence);

                    AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].subMode = {0}",
                                               track.subMode, track.point, session.sessionSequence);

                    AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].adrCtl = {0}",
                                               track.adrCtl, track.point, session.sessionSequence);

                    AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].tno = {0}", track.tno,
                                               track.point, session.sessionSequence);

                    AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].point = {0:X2}",
                                               track.point, track.point, session.sessionSequence);

                    AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].min = {0}", track.min,
                                               track.point, session.sessionSequence);

                    AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].sec = {0}", track.sec,
                                               track.point, session.sessionSequence);

                    AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].frame = {0}",
                                               track.frame, track.point, session.sessionSequence);

                    AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].zero = {0}", track.zero,
                                               track.point, session.sessionSequence);

                    AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].pmin = {0}", track.pmin,
                                               track.point, session.sessionSequence);

                    AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].psec = {0}", track.psec,
                                               track.point, session.sessionSequence);

                    AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].pframe = {0}",
                                               track.pframe, track.point, session.sessionSequence);

                    AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].extraOffset = {0}",
                                               track.extraOffset, track.point, session.sessionSequence);

                    AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].sectorSize = {0}",
                                               track.sectorSize, track.point, session.sessionSequence);

                    //for(int j = 0; j < track.unknown.Length; j++)
                    //    AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].unknown[{2}] = {0}", track.unknown[j], i, j, session.sessionSequence);
                    AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].startLba = {0}",
                                               track.startLba, track.point, session.sessionSequence);

                    AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].startOffset = {0}",
                                               track.startOffset, track.point, session.sessionSequence);

                    AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].files = {0}",
                                               track.files, track.point, session.sessionSequence);

                    AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].footerOffset = {0}",
                                               track.footerOffset, track.point, session.sessionSequence);

                    //for(int j = 0; j < track.unknown2.Length; j++)
                    //    AaruConsole.DebugWriteLine("Alcohol 120% plugin", "session[{2}].track[{1}].unknown2[{2}] = {0}", track.unknown2[j], i, j, session.sessionSequence);

                    if(track.subMode == AlcoholSubchannelMode.Interleaved)
                        track.sectorSize -= 96;

                    if(!sesToc.ContainsKey(track.point))
                        sesToc.Add(track.point, track);

                    if(track.point < 0xA0)
                        _alcTracks.Add(track.point, track);

                    if(footerOff == 0)
                        footerOff = track.footerOffset;

                    _isDvd |= track.mode == AlcoholTrackMode.DVD;
                }

                _alcToc.Add(session.sessionSequence, sesToc);
            }

            _alcTrackExtras = new Dictionary<int, AlcoholTrackExtra>();

            foreach(AlcoholTrack track in _alcTracks.Values)
                if(track.extraOffset > 0 &&
                   !_isDvd)
                {
                    byte[] extHdr = new byte[8];
                    stream.Seek(track.extraOffset, SeekOrigin.Begin);
                    stream.Read(extHdr, 0, 8);
                    AlcoholTrackExtra extra = Marshal.SpanToStructureLittleEndian<AlcoholTrackExtra>(extHdr);

                    AaruConsole.DebugWriteLine("Alcohol 120% plugin", "track[{1}].extra.pregap = {0}", extra.pregap,
                                               track.point);

                    AaruConsole.DebugWriteLine("Alcohol 120% plugin", "track[{1}].extra.sectors = {0}", extra.sectors,
                                               track.point);

                    _alcTrackExtras.Add(track.point, extra);
                }
                else if(_isDvd)
                {
                    var extra = new AlcoholTrackExtra
                    {
                        sectors = track.extraOffset
                    };

                    _alcTrackExtras.Add(track.point, extra);
                }

            if(footerOff > 0)
            {
                byte[] footer = new byte[16];
                stream.Seek(footerOff, SeekOrigin.Begin);
                stream.Read(footer, 0, 16);
                _alcFooter = Marshal.SpanToStructureLittleEndian<AlcoholFooter>(footer);

                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "footer.filenameOffset = {0}",
                                           _alcFooter.filenameOffset);

                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "footer.widechar = {0}", _alcFooter.widechar);
                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "footer.unknown1 = 0x{0:X8}", _alcFooter.unknown1);
                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "footer.unknown2 = 0x{0:X8}", _alcFooter.unknown2);
            }

            string alcFile = "*.mdf";

            if(_alcFooter.filenameOffset > 0)
            {
                stream.Seek(_alcFooter.filenameOffset, SeekOrigin.Begin);

                byte[] filename = _header.dpmOffset == 0 ? new byte[stream.Length - stream.Position]
                                      : new byte[_header.dpmOffset                - stream.Position];

                stream.Read(filename, 0, filename.Length);

                alcFile = _alcFooter.widechar == 1 ? Encoding.Unicode.GetString(filename)
                              : Encoding.Default.GetString(filename);

                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "footer.filename = {0}", alcFile);
            }

            if(_alcFooter.filenameOffset                                                     == 0 ||
               string.Compare(alcFile, "*.mdf", StringComparison.InvariantCultureIgnoreCase) == 0)
                alcFile = Path.GetFileNameWithoutExtension(imageFilter.GetBasePath()) + ".mdf";

            if(_header.bcaLength > 0 &&
               _header.bcaOffset > 0 &&
               _isDvd)
            {
                _bca = new byte[_header.bcaLength];
                stream.Seek(_header.bcaOffset, SeekOrigin.Begin);
                int readBytes = stream.Read(_bca, 0, _bca.Length);

                if(readBytes == _bca.Length)
                    switch(_header.type)
                    {
                        case AlcoholMediumType.DVD:
                        case AlcoholMediumType.DVDR:
                            _imageInfo.ReadableMediaTags.Add(MediaTagType.DVD_BCA);

                            break;
                    }
            }

            _imageInfo.MediaType = AlcoholMediumTypeToMediaType(_header.type);

            Sessions = new List<Session>();

            foreach(AlcoholSession alcSes in _alcSessions.Values)
            {
                var session = new Session();

                if(!_alcTracks.TryGetValue(alcSes.firstTrack, out AlcoholTrack startingTrack))
                    break;

                if(!_alcTracks.TryGetValue(alcSes.lastTrack, out AlcoholTrack endingTrack))
                    break;

                if(!_alcTrackExtras.TryGetValue(alcSes.lastTrack, out AlcoholTrackExtra endingTrackExtra))
                    break;

                session.StartSector     = startingTrack.startLba;
                session.StartTrack      = alcSes.firstTrack;
                session.SessionSequence = alcSes.sessionSequence;
                session.EndSector       = (endingTrack.startLba + endingTrackExtra.sectors) - 1;
                session.EndTrack        = alcSes.lastTrack;

                Sessions.Add(session);

                if(session.EndSector > _imageInfo.Sectors)
                    _imageInfo.Sectors = session.EndSector + 1;
            }

            if(_isDvd)
            {
                // TODO: Second layer
                if(_header.structuresOffset > 0)
                {
                    byte[] structures = new byte[4100];
                    stream.Seek(_header.structuresOffset, SeekOrigin.Begin);
                    stream.Read(structures, 0, 4100);
                    _dmi = new byte[2052];
                    _pfi = new byte[2052];

                    // TODO: CMI
                    Array.Copy(structures, 4, _dmi, 4, 2048);
                    Array.Copy(structures, 0x804, _pfi, 4, 2048);

                    _pfi[0] = 0x08;
                    _pfi[1] = 0x02;
                    _dmi[0] = 0x08;
                    _dmi[1] = 0x02;

                    PFI.PhysicalFormatInformation? pfi0 = PFI.Decode(_pfi);

                    // All discs I tested the disk category and part version (as well as the start PSN for DVD-RAM) where modified by Alcohol
                    // So much for archival value
                    if(pfi0.HasValue)
                    {
                        switch(pfi0.Value.DiskCategory)
                        {
                            case DiskCategory.DVDPR:
                                _imageInfo.MediaType = MediaType.DVDPR;

                                break;
                            case DiskCategory.DVDPRDL:
                                _imageInfo.MediaType = MediaType.DVDPRDL;

                                break;
                            case DiskCategory.DVDPRW:
                                _imageInfo.MediaType = MediaType.DVDPRW;

                                break;
                            case DiskCategory.DVDPRWDL:
                                _imageInfo.MediaType = MediaType.DVDPRWDL;

                                break;
                            case DiskCategory.DVDR:
                                _imageInfo.MediaType = pfi0.Value.PartVersion == 6 ? MediaType.DVDRDL : MediaType.DVDR;

                                break;
                            case DiskCategory.DVDRAM:
                                _imageInfo.MediaType = MediaType.DVDRAM;

                                break;
                            default:
                                _imageInfo.MediaType = MediaType.DVDROM;

                                break;
                            case DiskCategory.DVDRW:
                                _imageInfo.MediaType =
                                    pfi0.Value.PartVersion == 3 ? MediaType.DVDRWDL : MediaType.DVDRW;

                                break;
                            case DiskCategory.HDDVDR:
                                _imageInfo.MediaType = MediaType.HDDVDR;

                                break;
                            case DiskCategory.HDDVDRAM:
                                _imageInfo.MediaType = MediaType.HDDVDRAM;

                                break;
                            case DiskCategory.HDDVDROM:
                                _imageInfo.MediaType = MediaType.HDDVDROM;

                                break;
                            case DiskCategory.HDDVDRW:
                                _imageInfo.MediaType = MediaType.HDDVDRW;

                                break;
                            case DiskCategory.Nintendo:
                                _imageInfo.MediaType =
                                    pfi0.Value.DiscSize == DVDSize.Eighty ? MediaType.GOD : MediaType.WOD;

                                break;
                            case DiskCategory.UMD:
                                _imageInfo.MediaType = MediaType.UMD;

                                break;
                        }

                        if(DMI.IsXbox(_dmi))
                            _imageInfo.MediaType = MediaType.XGD;
                        else if(DMI.IsXbox360(_dmi))
                            _imageInfo.MediaType = MediaType.XGD2;

                        byte[] tmp = new byte[2048];
                        Array.Copy(_dmi, 4, tmp, 0, 2048);
                        _dmi = tmp;
                        tmp  = new byte[2048];
                        Array.Copy(_pfi, 4, tmp, 0, 2048);
                        _pfi = tmp;

                        _imageInfo.ReadableMediaTags.Add(MediaTagType.DVD_PFI);
                        _imageInfo.ReadableMediaTags.Add(MediaTagType.DVD_DMI);
                    }
                }
            }
            else if(_header.type == AlcoholMediumType.CD)
            {
                bool data       = false;
                bool mode2      = false;
                bool firstaudio = false;
                bool firstdata  = false;
                bool audio      = false;

                foreach(AlcoholTrack alcoholTrack in _alcTracks.Values)
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

            AaruConsole.DebugWriteLine("Alcohol 120% plugin", "ImageInfo.mediaType = {0}", _imageInfo.MediaType);

            Partitions = new List<Partition>();
            _offsetmap = new Dictionary<uint, ulong>();
            ulong byteOffset = 0;

            foreach(AlcoholTrack trk in _alcTracks.Values)
            {
                if(_alcTrackExtras.TryGetValue(trk.point, out AlcoholTrackExtra extra))
                {
                    var partition = new Partition
                    {
                        Description = $"Track {trk.point}.",
                        Start       = trk.startLba,
                        Size        = extra.sectors * trk.sectorSize,
                        Length      = extra.sectors,
                        Sequence    = trk.point,
                        Offset      = byteOffset,
                        Type        = trk.mode.ToString()
                    };

                    Partitions.Add(partition);
                    byteOffset += partition.Size;
                }

                if(!_offsetmap.ContainsKey(trk.point))
                    _offsetmap.Add(trk.point, trk.startLba);

                switch(trk.mode)
                {
                    case AlcoholTrackMode.Mode1:
                    case AlcoholTrackMode.Mode2F1:
                    case AlcoholTrackMode.Mode2F1Alt:
                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEcc))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEcc);

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccP))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccP);

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccQ))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccQ);

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);

                        if(_imageInfo.SectorSize < 2048)
                            _imageInfo.SectorSize = 2048;

                        break;
                    case AlcoholTrackMode.Mode2:
                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);

                        if(_imageInfo.SectorSize < 2336)
                            _imageInfo.SectorSize = 2336;

                        break;
                    case AlcoholTrackMode.Mode2F2:
                    case AlcoholTrackMode.Mode2F2Alt:
                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);

                        if(_imageInfo.SectorSize < 2324)
                            _imageInfo.SectorSize = 2324;

                        break;
                    case AlcoholTrackMode.DVD:
                        _imageInfo.SectorSize = 2048;

                        break;
                    default:
                        _imageInfo.SectorSize = 2352;

                        break;
                }

                if(trk.subMode != AlcoholSubchannelMode.None &&
                   !_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);
            }

            AaruConsole.DebugWriteLine("Alcohol 120% plugin", "printing partition map");

            foreach(Partition partition in Partitions)
            {
                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "Partition sequence: {0}", partition.Sequence);
                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "\tPartition name: {0}", partition.Name);

                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "\tPartition description: {0}",
                                           partition.Description);

                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "\tPartition type: {0}", partition.Type);
                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "\tPartition starting sector: {0}", partition.Start);
                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "\tPartition sectors: {0}", partition.Length);
                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "\tPartition starting offset: {0}", partition.Offset);
                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "\tPartition size in bytes: {0}", partition.Size);
            }

            _imageInfo.Application = "Alcohol 120%";

            AaruConsole.DebugWriteLine("Alcohol 120% plugin", "Data filename: {0}", alcFile);

            var filtersList = new FiltersList();
            _alcImage = filtersList.GetFilter(alcFile);

            if(_alcImage == null)
                throw new Exception("Cannot open data file");

            _imageInfo.ImageSize            = (ulong)_alcImage.GetDataForkLength();
            _imageInfo.CreationTime         = _alcImage.GetCreationTime();
            _imageInfo.LastModificationTime = _alcImage.GetLastWriteTime();
            _imageInfo.XmlMediaType         = XmlMediaType.OpticalDisc;
            _imageInfo.Version              = $"{_header.version[0]}.{_header.version[1]}";

            if(!_isDvd)
            {
                AaruConsole.DebugWriteLine("Alcohol 120% plugin", "Rebuilding TOC");
                byte firstSession = byte.MaxValue;
                byte lastSession  = 0;
                var  tocMs        = new MemoryStream();

                tocMs.Write(new byte[]
                {
                    0, 0
                }, 0, 2); // Reserved for TOC session numbers

                foreach(KeyValuePair<int, Dictionary<int, AlcoholTrack>> sessionToc in _alcToc)
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

                _fullToc    = tocMs.ToArray();
                _fullToc[0] = firstSession;
                _fullToc[1] = lastSession;

                _imageInfo.ReadableMediaTags.Add(MediaTagType.CD_FullTOC);
                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackFlags);
            }

            if(_imageInfo.MediaType == MediaType.XGD2)
                if(_imageInfo.Sectors == 25063   || // Locked (or non compatible drive)
                   _imageInfo.Sectors == 4229664 || // Xtreme unlock
                   _imageInfo.Sectors == 4246304)   // Wxripper unlock
                    _imageInfo.MediaType = MediaType.XGD3;

            AaruConsole.VerboseWriteLine("Alcohol 120% image describes a disc of type {0}", _imageInfo.MediaType);

            if(oldIncorrectImage)
                AaruConsole.
                    WriteLine("Incorrect Alcohol 120% image created by an old version of Aaru. Convert image to correct it.");

            return true;
        }

        public byte[] ReadDiskTag(MediaTagType tag)
        {
            switch(tag)
            {
                case MediaTagType.DVD_BCA:
                {
                    if(_bca != null)
                        return (byte[])_bca.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain BCA information.");
                }

                case MediaTagType.DVD_PFI:
                {
                    if(_pfi != null)
                        return (byte[])_pfi.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain PFI.");
                }

                case MediaTagType.DVD_DMI:
                {
                    if(_dmi != null)
                        return (byte[])_dmi.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain DMI.");
                }

                case MediaTagType.CD_FullTOC:
                {
                    if(_fullToc != null)
                        return (byte[])_fullToc.Clone();

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
            foreach(KeyValuePair<uint, ulong> kvp in _offsetmap)
                if(sectorAddress >= kvp.Value)
                    foreach(AlcoholTrack track in _alcTracks.Values)
                    {
                        if(track.point != kvp.Key ||
                           !_alcTrackExtras.TryGetValue(track.point, out AlcoholTrackExtra extra))
                            continue;

                        if(sectorAddress - kvp.Value < extra.sectors)
                            return ReadSectors(sectorAddress - kvp.Value, length, kvp.Key);
                    }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            foreach(KeyValuePair<uint, ulong> kvp in _offsetmap)
                if(sectorAddress >= kvp.Value)
                    foreach(AlcoholTrack track in _alcTracks.Values)
                    {
                        if(track.point != kvp.Key ||
                           !_alcTrackExtras.TryGetValue(track.point, out AlcoholTrackExtra extra))
                            continue;

                        if(sectorAddress - kvp.Value < extra.sectors)
                            return ReadSectorsTag(sectorAddress - kvp.Value, length, kvp.Key, tag);
                    }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            if(!_alcTracks.TryGetValue((int)track, out AlcoholTrack alcTrack) ||
               !_alcTrackExtras.TryGetValue((int)track, out AlcoholTrackExtra alcExtra))
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

            _imageStream = _alcImage.GetDataForkStream();
            var br = new BinaryReader(_imageStream);

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
            if(tag == SectorTagType.CdTrackFlags)
                track = (uint)sectorAddress;

            if(!_alcTracks.TryGetValue((int)track, out AlcoholTrack alcTrack) ||
               !_alcTrackExtras.TryGetValue((int)track, out AlcoholTrackExtra alcExtra))
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

            _imageStream = _alcImage.GetDataForkStream();
            var br = new BinaryReader(_imageStream);

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
            foreach(KeyValuePair<uint, ulong> kvp in _offsetmap)
                if(sectorAddress >= kvp.Value)
                    foreach(AlcoholTrack alcTrack in _alcTracks.Values)
                    {
                        if(alcTrack.point != kvp.Key ||
                           !_alcTrackExtras.TryGetValue(alcTrack.point, out AlcoholTrackExtra alcExtra))
                            continue;

                        if(sectorAddress - kvp.Value < alcExtra.sectors)
                            return ReadSectorsLong(sectorAddress - kvp.Value, length, kvp.Key);
                    }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            if(!_alcTracks.TryGetValue((int)track, out AlcoholTrack alcTrack) ||
               !_alcTrackExtras.TryGetValue((int)track, out AlcoholTrackExtra alcExtra))
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

            _imageStream = _alcImage.GetDataForkStream();
            var br = new BinaryReader(_imageStream);

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

            foreach(AlcoholTrack alcTrack in _alcTracks.Values)
            {
                ushort sessionNo =
                    (from ses in Sessions where alcTrack.point >= ses.StartTrack || alcTrack.point <= ses.EndTrack
                     select ses.SessionSequence).FirstOrDefault();

                if(!_alcTrackExtras.TryGetValue(alcTrack.point, out AlcoholTrackExtra alcExtra) ||
                   session != sessionNo)
                    continue;

                var aaruTrack = new Track
                {
                    TrackStartSector       = alcTrack.startLba,
                    TrackEndSector         = alcExtra.sectors - 1,
                    TrackPregap            = alcExtra.pregap,
                    TrackSession           = sessionNo,
                    TrackSequence          = alcTrack.point,
                    TrackType              = AlcoholTrackTypeToTrackType(alcTrack.mode),
                    TrackFilter            = _alcImage,
                    TrackFile              = _alcImage.GetFilename(),
                    TrackFileOffset        = alcTrack.startOffset,
                    TrackFileType          = "BINARY",
                    TrackRawBytesPerSector = alcTrack.sectorSize,
                    TrackBytesPerSector    = AlcoholTrackModeToCookedBytesPerSector(alcTrack.mode)
                };

                if(alcExtra.pregap > 0)
                    aaruTrack.Indexes.Add(0, (int)(alcTrack.startLba - alcExtra.pregap));

                aaruTrack.Indexes.Add(1, (int)alcTrack.startLba);

                switch(alcTrack.subMode)
                {
                    case AlcoholSubchannelMode.Interleaved:
                        aaruTrack.TrackSubchannelFilter = _alcImage;
                        aaruTrack.TrackSubchannelFile   = _alcImage.GetFilename();
                        aaruTrack.TrackSubchannelOffset = alcTrack.startOffset;
                        aaruTrack.TrackSubchannelType   = TrackSubchannelType.RawInterleaved;

                        break;
                    case AlcoholSubchannelMode.None:
                        aaruTrack.TrackSubchannelType = TrackSubchannelType.None;

                        break;
                }

                tracks.Add(aaruTrack);
            }

            return tracks;
        }
    }
}