// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Alcohol120.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Alcohol 120% disc images.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.Checksums;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Decoders.CD;
using DiscImageChef.Decoders.DVD;
using DiscImageChef.Filters;
using Schemas;
using DMI = DiscImageChef.Decoders.Xbox.DMI;

namespace DiscImageChef.DiscImages
{
    public class Alcohol120 : IWritableImage
    {
        readonly byte[] alcoholSignature =
            {0x4d, 0x45, 0x44, 0x49, 0x41, 0x20, 0x44, 0x45, 0x53, 0x43, 0x52, 0x49, 0x50, 0x54, 0x4f, 0x52};
        AlcoholFooter                                  alcFooter;
        IFilter                                        alcImage;
        Dictionary<int, AlcoholSession>                alcSessions;
        Dictionary<int, Dictionary<int, AlcoholTrack>> alcToc;
        Dictionary<int, AlcoholTrackExtra>             alcTrackExtras;
        Dictionary<int, AlcoholTrack>                  alcTracks;
        byte[]                                         bca;
        FileStream                                     descriptorStream;
        byte[]                                         dmi;
        byte[]                                         fullToc;
        ImageInfo                                      imageInfo;
        Stream                                         imageStream;
        bool                                           isDvd;
        Dictionary<uint, ulong>                        offsetmap;
        byte[]                                         pfi;
        Dictionary<byte, byte>                         trackFlags;
        List<Track>                                    writingTracks;

        public Alcohol120()
        {
            imageInfo = new ImageInfo
            {
                ReadableSectorTags    = new List<SectorTagType>(),
                ReadableMediaTags     = new List<MediaTagType>(),
                HasPartitions         = true,
                HasSessions           = true,
                Version               = null,
                Application           = null,
                ApplicationVersion    = null,
                Creator               = null,
                Comments              = null,
                MediaManufacturer     = null,
                MediaModel            = null,
                MediaSerialNumber     = null,
                MediaBarcode          = null,
                MediaPartNumber       = null,
                MediaSequence         = 0,
                LastMediaSequence     = 0,
                DriveManufacturer     = null,
                DriveModel            = null,
                DriveSerialNumber     = null,
                DriveFirmwareRevision = null
            };
        }

        public ImageInfo Info => imageInfo;
        public string    Name => "Alcohol 120% Media Descriptor Structure";
        public Guid      Id   => new Guid("A78FBEBA-0307-4915-BDE3-B8A3B57F843F");

        public string Format => "Alcohol 120% Media Descriptor Structure";

        public List<Partition> Partitions { get; private set; }

        public List<Track> Tracks
        {
            get
            {
                List<Track> tracks = new List<Track>();

                foreach(AlcoholTrack alcTrack in alcTracks.Values)
                {
                    ushort sessionNo =
                        (from session in Sessions
                         where alcTrack.point >= session.StartTrack || alcTrack.point <= session.EndTrack
                         select session.SessionSequence).FirstOrDefault();

                    if(!alcTrackExtras.TryGetValue(alcTrack.point, out AlcoholTrackExtra alcExtra)) continue;

                    Track dicTrack = new Track
                    {
                        Indexes                = new Dictionary<int, ulong> {{1, alcTrack.startLba}},
                        TrackStartSector       = alcTrack.startLba,
                        TrackEndSector         = alcTrack.startLba + alcExtra.sectors - 1,
                        TrackPregap            = alcExtra.pregap,
                        TrackSession           = sessionNo,
                        TrackSequence          = alcTrack.point,
                        TrackType              = AlcoholTrackTypeToTrackType(alcTrack.mode),
                        TrackFilter            = alcImage,
                        TrackFile              = alcImage.GetFilename(),
                        TrackFileOffset        = alcTrack.startOffset,
                        TrackFileType          = "BINARY",
                        TrackRawBytesPerSector = alcTrack.sectorSize,
                        TrackBytesPerSector    = AlcoholTrackModeToCookedBytesPerSector(alcTrack.mode)
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

        public List<Session> Sessions { get; private set; }

        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            if(stream.Length < 88) return false;

            byte[] hdr = new byte[88];
            stream.Read(hdr, 0, 88);
            IntPtr hdrPtr = Marshal.AllocHGlobal(88);
            Marshal.Copy(hdr, 0, hdrPtr, 88);
            AlcoholHeader header = (AlcoholHeader)Marshal.PtrToStructure(hdrPtr, typeof(AlcoholHeader));
            Marshal.FreeHGlobal(hdrPtr);

            return header.signature.SequenceEqual(alcoholSignature);
        }

        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            if(stream.Length < 88) return false;

            isDvd = false;
            byte[] hdr = new byte[88];
            stream.Read(hdr, 0, 88);
            IntPtr hdrPtr = Marshal.AllocHGlobal(88);
            Marshal.Copy(hdr, 0, hdrPtr, 88);
            AlcoholHeader header = (AlcoholHeader)Marshal.PtrToStructure(hdrPtr, typeof(AlcoholHeader));
            Marshal.FreeHGlobal(hdrPtr);

            DicConsole.DebugWriteLine("Alcohol 120% plugin", "header.signature = {0}",
                                      Encoding.ASCII.GetString(header.signature));
            DicConsole.DebugWriteLine("Alcohol 120% plugin", "header.version = {0}.{1}", header.version[0],
                                      header.version[1]);
            DicConsole.DebugWriteLine("Alcohol 120% plugin", "header.type = {0}",     header.type);
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
            DicConsole.DebugWriteLine("Alcohol 120% plugin", "header.dpmOffset = {0}",     header.dpmOffset);

            stream.Seek(header.sessionOffset, SeekOrigin.Begin);
            alcSessions = new Dictionary<int, AlcoholSession>();
            for(int i = 0; i < header.sessions; i++)
            {
                byte[] sesHdr = new byte[24];
                stream.Read(sesHdr, 0, 24);
                IntPtr sesPtr = Marshal.AllocHGlobal(24);
                Marshal.Copy(sesHdr, 0, sesPtr, 24);
                AlcoholSession session = (AlcoholSession)Marshal.PtrToStructure(sesPtr, typeof(AlcoholSession));
                Marshal.FreeHGlobal(sesPtr);

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
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{1}].lastTrack = {0}", session.lastTrack,
                                          i);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{1}].unknown = 0x{0:X8}", session.unknown,
                                          i);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{1}].trackOffset = {0}", session.trackOffset,
                                          i);

                alcSessions.Add(session.sessionSequence, session);
            }

            long footerOff = 0;

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
                    IntPtr trkPtr = Marshal.AllocHGlobal(80);
                    Marshal.Copy(trkHdr, 0, trkPtr, 80);
                    AlcoholTrack track = (AlcoholTrack)Marshal.PtrToStructure(trkPtr, typeof(AlcoholTrack));
                    Marshal.FreeHGlobal(trkPtr);

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

                    if(track.subMode == AlcoholSubchannelMode.Interleaved) track.sectorSize -= 96;

                    if(!sesToc.ContainsKey(track.point)) sesToc.Add(track.point, track);

                    if(track.point < 0xA0) alcTracks.Add(track.point, track);

                    if(footerOff == 0) footerOff = track.footerOffset;

                    isDvd |= track.mode == AlcoholTrackMode.DVD;
                }

                alcToc.Add(session.sessionSequence, sesToc);
            }

            alcTrackExtras = new Dictionary<int, AlcoholTrackExtra>();
            foreach(AlcoholTrack track in alcTracks.Values)
                if(track.extraOffset > 0 && !isDvd)
                {
                    byte[] extHdr = new byte[8];
                    stream.Seek(track.extraOffset, SeekOrigin.Begin);
                    stream.Read(extHdr, 0, 8);
                    IntPtr extPtr = Marshal.AllocHGlobal(8);
                    Marshal.Copy(extHdr, 0, extPtr, 8);
                    AlcoholTrackExtra extra =
                        (AlcoholTrackExtra)Marshal.PtrToStructure(extPtr, typeof(AlcoholTrackExtra));
                    Marshal.FreeHGlobal(extPtr);

                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "track[{1}].extra.pregap = {0}", extra.pregap,
                                              track.point);
                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "track[{1}].extra.sectors = {0}", extra.sectors,
                                              track.point);

                    alcTrackExtras.Add(track.point, extra);
                }
                else if(isDvd)
                {
                    AlcoholTrackExtra extra = new AlcoholTrackExtra {sectors = track.extraOffset};
                    alcTrackExtras.Add(track.point, extra);
                }

            if(footerOff > 0)
            {
                byte[] footer = new byte[16];
                stream.Seek(footerOff, SeekOrigin.Begin);
                stream.Read(footer, 0, 16);
                alcFooter = new AlcoholFooter();
                IntPtr footPtr = Marshal.AllocHGlobal(16);
                Marshal.Copy(footer, 0, footPtr, 16);
                alcFooter = (AlcoholFooter)Marshal.PtrToStructure(footPtr, typeof(AlcoholFooter));
                Marshal.FreeHGlobal(footPtr);

                DicConsole.DebugWriteLine("Alcohol 120% plugin", "footer.filenameOffset = {0}",
                                          alcFooter.filenameOffset);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "footer.widechar = {0}",      alcFooter.widechar);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "footer.unknown1 = 0x{0:X8}", alcFooter.unknown1);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "footer.unknown2 = 0x{0:X8}", alcFooter.unknown2);
            }

            string alcFile = "*.mdf";

            if(alcFooter.filenameOffset > 0)
            {
                stream.Seek(alcFooter.filenameOffset, SeekOrigin.Begin);
                byte[] filename = header.dpmOffset == 0
                                      ? new byte[stream.Length    - stream.Position]
                                      : new byte[header.dpmOffset - stream.Position];

                stream.Read(filename, 0, filename.Length);
                alcFile = alcFooter.widechar == 1
                              ? Encoding.Unicode.GetString(filename)
                              : Encoding.Default.GetString(filename);

                DicConsole.DebugWriteLine("Alcohol 120% plugin", "footer.filename = {0}", alcFile);
            }

            if(alcFooter.filenameOffset                                                      == 0 ||
               string.Compare(alcFile, "*.mdf", StringComparison.InvariantCultureIgnoreCase) == 0)
                alcFile = Path.GetFileNameWithoutExtension(imageFilter.GetBasePath()) + ".mdf";

            if(header.bcaLength > 0 && header.bcaOffset > 0 && isDvd)
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
                Session session = new Session();

                if(!alcTracks.TryGetValue(alcSes.firstTrack, out AlcoholTrack startingTrack)) break;
                if(!alcTracks.TryGetValue(alcSes.lastTrack,  out AlcoholTrack endingTrack)) break;
                if(!alcTrackExtras.TryGetValue(alcSes.lastTrack, out AlcoholTrackExtra endingTrackExtra)) break;

                session.StartSector     = startingTrack.startLba;
                session.StartTrack      = alcSes.firstTrack;
                session.SessionSequence = alcSes.sessionSequence;
                session.EndSector       = endingTrack.startLba + endingTrackExtra.sectors - 1;
                session.EndTrack        = alcSes.lastTrack;

                Sessions.Add(session);
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

                    Array.Copy(structures, 0,     dmi, 0, 2052);
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

                        if(DMI.IsXbox(dmi)) imageInfo.MediaType         = MediaType.XGD;
                        else if(DMI.IsXbox360(dmi)) imageInfo.MediaType = MediaType.XGD2;

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
                            mode2 = true;
                            break;
                    }
                }

                if(!data                                         && !firstdata) imageInfo.MediaType = MediaType.CDDA;
                else if(firstaudio && data && Sessions.Count > 1 && mode2) imageInfo.MediaType      = MediaType.CDPLUS;
                else if(firstdata && audio || mode2) imageInfo.MediaType                            = MediaType.CDROMXA;
                else if(!audio) imageInfo.MediaType                                                 = MediaType.CDROM;
                else imageInfo.MediaType                                                            = MediaType.CD;
            }

            DicConsole.DebugWriteLine("Alcohol 120% plugin", "ImageInfo.mediaType = {0}", imageInfo.MediaType);

            Partitions = new List<Partition>();
            offsetmap  = new Dictionary<uint, ulong>();
            ulong byteOffset = 0;

            foreach(AlcoholTrack trk in alcTracks.Values)
            {
                if(alcTrackExtras.TryGetValue(trk.point, out AlcoholTrackExtra extra))
                {
                    Partition partition = new Partition
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
                    imageInfo.Sectors += extra.sectors;
                    byteOffset        += partition.Size;
                }

                if(!offsetmap.ContainsKey(trk.point)) offsetmap.Add(trk.point, trk.startLba);

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
                        if(imageInfo.SectorSize < 2048) imageInfo.SectorSize = 2048;
                        break;
                    case AlcoholTrackMode.Mode2:
                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);
                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);
                        if(imageInfo.SectorSize < 2336) imageInfo.SectorSize = 2336;
                        break;
                    case AlcoholTrackMode.Mode2F2:
                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);
                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);
                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);
                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);
                        if(imageInfo.SectorSize < 2324) imageInfo.SectorSize = 2324;
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
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "Partition sequence: {0}",
                                          partition.Sequence);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "\tPartition name: {0}", partition.Name);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "\tPartition description: {0}",
                                          partition.Description);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "\tPartition type: {0}",            partition.Type);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "\tPartition starting sector: {0}", partition.Start);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "\tPartition sectors: {0}",         partition.Length);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "\tPartition starting offset: {0}", partition.Offset);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "\tPartition size in bytes: {0}",   partition.Size);
            }

            imageInfo.Application = "Alcohol 120%";

            DicConsole.DebugWriteLine("Alcohol 120% plugin", "Data filename: {0}", alcFile);

            FiltersList filtersList = new FiltersList();
            alcImage = filtersList.GetFilter(alcFile);

            if(alcImage == null) throw new Exception("Cannot open data file");

            imageInfo.ImageSize            = (ulong)alcImage.GetDataForkLength();
            imageInfo.CreationTime         = alcImage.GetCreationTime();
            imageInfo.LastModificationTime = alcImage.GetLastWriteTime();
            imageInfo.XmlMediaType         = XmlMediaType.OpticalDisc;
            imageInfo.Version              = $"{header.version[0]}.{header.version[1]}";

            if(!isDvd)
            {
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "Rebuilding TOC");
                byte         firstSession = byte.MaxValue;
                byte         lastSession  = 0;
                MemoryStream tocMs        = new MemoryStream();
                tocMs.Write(new byte[] {0, 0, 0, 0}, 0, 4); // Reserved for TOC response size and session numbers
                foreach(KeyValuePair<int, Dictionary<int, AlcoholTrack>> sessionToc in alcToc)
                {
                    if(sessionToc.Key < firstSession) firstSession = (byte)sessionToc.Key;
                    if(sessionToc.Key > lastSession) lastSession   = (byte)sessionToc.Key;

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

                fullToc                              = tocMs.ToArray();
                BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
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
                else imageInfo.ReadableMediaTags.Add(MediaTagType.CD_FullTOC);

                imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackFlags);
            }

            if(imageInfo.MediaType == MediaType.XGD2)
                if(imageInfo.Sectors == 25063   || // Locked (or non compatible drive)
                   imageInfo.Sectors == 4229664 || // Xtreme unlock
                   imageInfo.Sectors == 4246304)   // Wxripper unlock
                    imageInfo.MediaType = MediaType.XGD3;

            DicConsole.VerboseWriteLine("Alcohol 120% image describes a disc of type {0}", imageInfo.MediaType);

            return true;
        }

        public byte[] ReadDiskTag(MediaTagType tag)
        {
            switch(tag)
            {
                case MediaTagType.DVD_BCA:
                {
                    if(bca != null) return (byte[])bca.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain BCA information.");
                }
                case MediaTagType.DVD_PFI:
                {
                    if(pfi != null) return (byte[])pfi.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain PFI.");
                }
                case MediaTagType.DVD_DMI:
                {
                    if(dmi != null) return (byte[])dmi.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain DMI.");
                }
                case MediaTagType.CD_FullTOC:
                {
                    if(fullToc != null) return (byte[])fullToc.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain TOC information.");
                }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
            }
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            return ReadSectorsTag(sectorAddress, 1, tag);
        }

        public byte[] ReadSector(ulong sectorAddress, uint track)
        {
            return ReadSectors(sectorAddress, 1, track);
        }

        public byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            return ReadSectorsTag(sectorAddress, 1, track, tag);
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in offsetmap)
                if(sectorAddress >= kvp.Value)
                    foreach(AlcoholTrack track in alcTracks.Values)
                    {
                        if(track.point != kvp.Key ||
                           !alcTrackExtras.TryGetValue(track.point, out AlcoholTrackExtra extra)) continue;

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
                           !alcTrackExtras.TryGetValue(track.point, out AlcoholTrackExtra extra)) continue;

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
                {
                    sectorOffset = 16;
                    sectorSize   = 2336;
                    sectorSkip   = 0;
                    break;
                }
                case AlcoholTrackMode.Mode2F1:
                case AlcoholTrackMode.Mode2F1Alt:
                {
                    sectorOffset = 24;
                    sectorSize   = 2048;
                    sectorSkip   = 280;
                    break;
                }
                case AlcoholTrackMode.Mode2F2:
                {
                    sectorOffset = 24;
                    sectorSize   = 2324;
                    sectorSkip   = 4;
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
            BinaryReader br = new BinaryReader(imageStream);
            br.BaseStream
              .Seek((long)alcTrack.startOffset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                    SeekOrigin.Begin);
            if(sectorOffset == 0 && sectorSkip == 0) buffer = br.ReadBytes((int)(sectorSize * length));
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
                case SectorTagType.CdTrackFlags: return new[] {(byte)(alcTrack.adrCtl & 0x0F)};
                default:                         throw new ArgumentException("Unsupported tag requested", nameof(tag));
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
                    if(tag != SectorTagType.CdSectorSubchannel) sectorSkip += 96;
                    break;
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported subchannel type");
            }

            byte[] buffer = new byte[sectorSize * length];

            imageStream = alcImage.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);
            br.BaseStream
              .Seek((long)alcTrack.startOffset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                    SeekOrigin.Begin);
            if(sectorOffset == 0 && sectorSkip == 0) buffer = br.ReadBytes((int)(sectorSize * length));
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

        public byte[] ReadSectorLong(ulong sectorAddress)
        {
            return ReadSectorsLong(sectorAddress, 1);
        }

        public byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            return ReadSectorsLong(sectorAddress, 1, track);
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in offsetmap)
                if(sectorAddress >= kvp.Value)
                    foreach(AlcoholTrack alcTrack in alcTracks.Values)
                    {
                        if(alcTrack.point != kvp.Key ||
                           !alcTrackExtras.TryGetValue(alcTrack.point, out AlcoholTrackExtra alcExtra)) continue;

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

            if(alcTrack.subMode == AlcoholSubchannelMode.Interleaved) sectorSkip = 96;

            byte[] buffer = new byte[sectorSize * length];

            imageStream = alcImage.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);

            br.BaseStream
              .Seek((long)alcTrack.startOffset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                    SeekOrigin.Begin);

            if(sectorOffset == 0 && sectorSkip == 0) buffer = br.ReadBytes((int)(sectorSize * length));
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
            if(Sessions.Contains(session)) return GetSessionTracks(session.SessionSequence);

            throw new ImageNotSupportedException("Session does not exist in disc image");
        }

        public List<Track> GetSessionTracks(ushort session)
        {
            List<Track> tracks = new List<Track>();

            foreach(AlcoholTrack alcTrack in alcTracks.Values)
            {
                ushort sessionNo =
                    (from ses in Sessions
                     where alcTrack.point >= ses.StartTrack || alcTrack.point <= ses.EndTrack
                     select ses.SessionSequence).FirstOrDefault();

                if(!alcTrackExtras.TryGetValue(alcTrack.point, out AlcoholTrackExtra alcExtra) ||
                   session != sessionNo) continue;

                Track dicTrack = new Track
                {
                    Indexes                = new Dictionary<int, ulong> {{1, alcTrack.startLba}},
                    TrackStartSector       = alcTrack.startLba,
                    TrackEndSector         = alcExtra.sectors - 1,
                    TrackPregap            = alcExtra.pregap,
                    TrackSession           = sessionNo,
                    TrackSequence          = alcTrack.point,
                    TrackType              = AlcoholTrackTypeToTrackType(alcTrack.mode),
                    TrackFilter            = alcImage,
                    TrackFile              = alcImage.GetFilename(),
                    TrackFileOffset        = alcTrack.startOffset,
                    TrackFileType          = "BINARY",
                    TrackRawBytesPerSector = alcTrack.sectorSize,
                    TrackBytesPerSector    = AlcoholTrackModeToCookedBytesPerSector(alcTrack.mode)
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

        public bool? VerifySector(ulong sectorAddress)
        {
            byte[] buffer = ReadSectorLong(sectorAddress);
            return CdChecksums.CheckCdSector(buffer);
        }

        public bool? VerifySector(ulong sectorAddress, uint track)
        {
            byte[] buffer = ReadSectorLong(sectorAddress, track);
            return CdChecksums.CheckCdSector(buffer);
        }

        public bool? VerifySectors(ulong           sectorAddress, uint length, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas)
        {
            byte[] buffer = ReadSectorsLong(sectorAddress, length);
            int    bps    = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * bps, sector, 0, bps);
                bool? sectorStatus = CdChecksums.CheckCdSector(sector);

                switch(sectorStatus)
                {
                    case null:
                        unknownLbas.Add((ulong)i + sectorAddress);
                        break;
                    case false:
                        failingLbas.Add((ulong)i + sectorAddress);
                        break;
                }
            }

            if(unknownLbas.Count > 0) return null;

            return failingLbas.Count <= 0;
        }

        public bool? VerifySectors(ulong           sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas)
        {
            byte[] buffer = ReadSectorsLong(sectorAddress, length, track);
            int    bps    = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * bps, sector, 0, bps);
                bool? sectorStatus = CdChecksums.CheckCdSector(sector);

                switch(sectorStatus)
                {
                    case null:
                        unknownLbas.Add((ulong)i + sectorAddress);
                        break;
                    case false:
                        failingLbas.Add((ulong)i + sectorAddress);
                        break;
                }
            }

            if(unknownLbas.Count > 0) return null;

            return failingLbas.Count <= 0;
        }

        public bool? VerifyMediaImage()
        {
            return null;
        }

        public List<DumpHardwareType> DumpHardware => null;
        public CICMMetadataType       CicmMetadata => null;

        public IEnumerable<MediaTagType> SupportedMediaTags =>
            new[] {MediaTagType.CD_FullTOC, MediaTagType.DVD_BCA, MediaTagType.DVD_DMI, MediaTagType.DVD_PFI};
        public IEnumerable<SectorTagType> SupportedSectorTags =>
            new[]
            {
                SectorTagType.CdSectorEcc, SectorTagType.CdSectorEccP, SectorTagType.CdSectorEccQ,
                SectorTagType.CdSectorEdc, SectorTagType.CdSectorHeader, SectorTagType.CdSectorSubHeader,
                SectorTagType.CdSectorSync, SectorTagType.CdTrackFlags, SectorTagType.CdSectorSubchannel
            };
        public IEnumerable<MediaType> SupportedMediaTypes =>
            new[]
            {
                MediaType.BDR, MediaType.BDRE, MediaType.BDREXL, MediaType.BDROM, MediaType.BDRXL, MediaType.CBHD,
                MediaType.CD, MediaType.CDDA, MediaType.CDEG, MediaType.CDG, MediaType.CDI, MediaType.CDMIDI,
                MediaType.CDMRW, MediaType.CDPLUS, MediaType.CDR, MediaType.CDROM, MediaType.CDROMXA, MediaType.CDRW,
                MediaType.CDV, MediaType.DDCD, MediaType.DDCDR, MediaType.DDCDRW, MediaType.DVDDownload,
                MediaType.DVDPR, MediaType.DVDPRDL, MediaType.DVDPRW, MediaType.DVDPRWDL, MediaType.DVDR,
                MediaType.DVDRAM, MediaType.DVDRDL, MediaType.DVDROM, MediaType.DVDRW, MediaType.DVDRWDL, MediaType.EVD,
                MediaType.FDDVD, MediaType.DTSCD, MediaType.FVD, MediaType.HDDVDR, MediaType.HDDVDRAM,
                MediaType.HDDVDRDL, MediaType.HDDVDROM, MediaType.HDDVDRW, MediaType.HDDVDRWDL, MediaType.HDVMD,
                MediaType.HVD, MediaType.JaguarCD, MediaType.MEGACD, MediaType.PD650, MediaType.PD650_WORM,
                MediaType.PS1CD, MediaType.PS2CD, MediaType.PS2DVD, MediaType.PS3BD, MediaType.PS3DVD, MediaType.PS4BD,
                MediaType.SuperCDROM2, MediaType.SVCD, MediaType.SVOD, MediaType.SATURNCD, MediaType.ThreeDO,
                MediaType.UDO, MediaType.UDO2, MediaType.UDO2_WORM, MediaType.UMD, MediaType.VCD, MediaType.VCDHD,
                MediaType.NeoGeoCD, MediaType.PCFX
            };
        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new (string name, Type type, string description)[] { };
        public IEnumerable<string> KnownExtensions => new[] {".mds"};
        public bool                IsWriting       { get; private set; }
        public string              ErrorMessage    { get; private set; }

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
                case MediaType.DDCD:
                case MediaType.DDCDR:
                case MediaType.DDCDRW:
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

                    fullToc = data;
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

            long currentExtraOffset = currentTrackOffset;
            for(int i = 1; i <= sessions; i++)
            {
                currentExtraOffset += Marshal.SizeOf(typeof(AlcoholTrack)) * 3;
                currentExtraOffset +=
                    Marshal.SizeOf(typeof(AlcoholTrack)) * writingTracks.Count(t => t.TrackSession == i);
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
                                        (int)(writingTracks[0].TrackEndSector - writingTracks[0].TrackStartSector + 1),
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
                                      (uint)(writingTracks[0].TrackEndSector - writingTracks[0].TrackStartSector + 1),
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
                                        sessionStart    = i == 1 ? -150 : (int)firstTrack.TrackStartSector,
                                        sessionEnd      = (int)lastTrack.TrackEndSector + 1,
                                        sessionSequence = (ushort)i,
                                        allBlocks       = (byte)(writingTracks.Count(t => t.TrackSession == i) + 3),
                                        nonTrackBlocks  = 3,
                                        firstTrack      = (ushort)firstTrack.TrackSequence,
                                        lastTrack       = (ushort)lastTrack.TrackSequence,
                                        trackOffset     = (uint)currentTrackOffset
                                    });

                    Dictionary<int, AlcoholTrack> thisSessionTracks = new Dictionary<int, AlcoholTrack>();
                    trackFlags.TryGetValue((byte)firstTrack.TrackSequence, out byte firstTrackControl);
                    trackFlags.TryGetValue((byte)lastTrack.TrackSequence,  out byte lastTrackControl);
                    if(firstTrackControl == 0 && firstTrack.TrackType != TrackType.Audio)
                        firstTrackControl = (byte)CdFlags.DataTrack;
                    if(lastTrackControl == 0 && lastTrack.TrackType != TrackType.Audio)
                        lastTrackControl = (byte)CdFlags.DataTrack;
                    (byte hour, byte minute, byte second, byte frame) leadinPmsf =
                        LbaToMsf(lastTrack.TrackEndSector + 1);

                    thisSessionTracks.Add(0xA0,
                                          new AlcoholTrack
                                          {
                                              adrCtl   = (byte)((1 << 4) + firstTrackControl),
                                              pmin     = (byte)firstTrack.TrackSequence,
                                              mode     = AlcoholTrackMode.NoData,
                                              point    = 0xA0,
                                              unknown  = new byte[18],
                                              unknown2 = new byte[24]
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
                                              zero     = leadinPmsf.hour,
                                              pmin     = leadinPmsf.minute,
                                              psec     = leadinPmsf.second,
                                              pframe   = leadinPmsf.frame,
                                              mode     = AlcoholTrackMode.NoData,
                                              point    = 0xA2,
                                              unknown  = new byte[18],
                                              unknown2 = new byte[24]
                                          });

                    currentTrackOffset += Marshal.SizeOf(typeof(AlcoholTrack)) * 3;

                    foreach(Track track in writingTracks.Where(t => t.TrackSession == i).OrderBy(t => t.TrackSequence))
                    {
                        (byte hour, byte minute, byte second, byte frame) msf = LbaToMsf(track.TrackStartSector);
                        trackFlags.TryGetValue((byte)track.TrackSequence, out byte trackControl);
                        if(trackControl == 0 && track.TrackType != TrackType.Audio)
                            trackControl = (byte)CdFlags.DataTrack;

                        thisSessionTracks.Add((int)track.TrackSequence, new AlcoholTrack
                        {
                            mode = TrackTypeToAlcohol(track.TrackType),
                            subMode =
                                track.TrackSubchannelType != TrackSubchannelType.None
                                    ? AlcoholSubchannelMode.Interleaved
                                    : AlcoholSubchannelMode.None,
                            adrCtl = (byte)((1 << 4) + trackControl),
                            point  = (byte)track.TrackSequence,
                            zero   = msf.hour,
                            pmin   = msf.minute,
                            psec   = msf.second,
                            pframe = msf.frame,
                            sectorSize =
                                (ushort)(track.TrackRawBytesPerSector +
                                         (track.TrackSubchannelType != TrackSubchannelType.None ? 96 : 0)),
                            startLba     = (uint)track.TrackStartSector,
                            startOffset  = track.TrackFileOffset,
                            files        = 1,
                            extraOffset  = (uint)currentExtraOffset,
                            footerOffset = (uint)footerOffset,
                            // Alcohol seems to set that for all CD tracks
                            // Daemon Tools expect it to be like this
                            unknown = new byte[]
                            {
                                0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00
                            },
                            unknown2 = new byte[24]
                        });

                        currentTrackOffset += Marshal.SizeOf(typeof(AlcoholTrack));
                        currentExtraOffset += Marshal.SizeOf(typeof(AlcoholTrackExtra));

                        alcTrackExtras.Add((int)track.TrackSequence,
                                           new AlcoholTrackExtra
                                           {
                                               pregap  = (uint)(track.TrackSequence == 1 ? 150 : 0),
                                               sectors = (uint)(track.TrackEndSector - track.TrackStartSector + 1)
                                           });
                    }

                    if(i < sessions)
                    {
                        (byte hour, byte minute, byte second, byte frame) leadoutAmsf =
                            LbaToMsf(writingTracks.First(t => t.TrackSession == i + 1).TrackStartSector - 150);
                        (byte hour, byte minute, byte second, byte frame) leadoutPmsf =
                            LbaToMsf(writingTracks.OrderBy(t => t.TrackSession).ThenBy(t => t.TrackSequence).Last()
                                                  .TrackStartSector);

                        thisSessionTracks.Add(0xB0,
                                              new AlcoholTrack
                                              {
                                                  point  = 0xB0,
                                                  adrCtl = 0x50,
                                                  zero =
                                                      (byte)(((leadoutAmsf.hour & 0xF) << 4) +
                                                             (leadoutPmsf.hour & 0xF)),
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
                                              new AlcoholTrack
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
                filenameOffset = (uint)(footerOffset + Marshal.SizeOf(typeof(AlcoholFooter))),
                widechar       = 1
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

        public bool SetMetadata(ImageInfo metadata)
        {
            return true;
        }

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

        public bool SetDumpHardware(List<DumpHardwareType> dumpHardware)
        {
            // Not supported
            return false;
        }

        public bool SetCicmMetadata(CICMMetadataType metadata)
        {
            // Not supported
            return false;
        }

        static ushort AlcoholTrackModeToBytesPerSector(AlcoholTrackMode trackMode)
        {
            switch(trackMode)
            {
                case AlcoholTrackMode.Audio:
                case AlcoholTrackMode.Mode1:
                case AlcoholTrackMode.Mode2:
                case AlcoholTrackMode.Mode2F2:
                case AlcoholTrackMode.Mode2F1:
                case AlcoholTrackMode.Mode2F1Alt: return 2352;
                case AlcoholTrackMode.DVD: return 2048;
                default:                   return 0;
            }
        }

        static ushort AlcoholTrackModeToCookedBytesPerSector(AlcoholTrackMode trackMode)
        {
            switch(trackMode)
            {
                case AlcoholTrackMode.Mode1:
                case AlcoholTrackMode.Mode2F1:
                case AlcoholTrackMode.Mode2F1Alt: return 2048;
                case AlcoholTrackMode.Mode2F2: return 2324;
                case AlcoholTrackMode.Mode2:   return 2336;
                case AlcoholTrackMode.Audio:   return 2352;
                case AlcoholTrackMode.DVD:     return 2048;
                default:                       return 0;
            }
        }

        static TrackType AlcoholTrackTypeToTrackType(AlcoholTrackMode trackType)
        {
            switch(trackType)
            {
                case AlcoholTrackMode.Mode1: return TrackType.CdMode1;
                case AlcoholTrackMode.Mode2F1:
                case AlcoholTrackMode.Mode2F1Alt: return TrackType.CdMode2Form1;
                case AlcoholTrackMode.Mode2F2: return TrackType.CdMode2Form2;
                case AlcoholTrackMode.Mode2:   return TrackType.CdMode2Formless;
                case AlcoholTrackMode.Audio:   return TrackType.Audio;
                default:                       return TrackType.Data;
            }
        }

        static MediaType AlcoholMediumTypeToMediaType(AlcoholMediumType discType)
        {
            switch(discType)
            {
                case AlcoholMediumType.CD:   return MediaType.CD;
                case AlcoholMediumType.CDR:  return MediaType.CDR;
                case AlcoholMediumType.CDRW: return MediaType.CDRW;
                case AlcoholMediumType.DVD:  return MediaType.DVDROM;
                case AlcoholMediumType.DVDR: return MediaType.DVDR;
                default:                     return MediaType.Unknown;
            }
        }

        static AlcoholMediumType MediaTypeToAlcohol(MediaType type)
        {
            switch(type)
            {
                case MediaType.CD:
                case MediaType.CDDA:
                case MediaType.CDEG:
                case MediaType.CDG:
                case MediaType.CDI:
                case MediaType.CDMIDI:
                case MediaType.CDPLUS:
                case MediaType.CDROM:
                case MediaType.CDROMXA:
                case MediaType.CDV:
                case MediaType.DDCD:
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
                case MediaType.VCDHD: return AlcoholMediumType.CD;
                case MediaType.DDCDR:
                case MediaType.CDR: return AlcoholMediumType.CDR;
                case MediaType.CDRW:
                case MediaType.DDCDRW:
                case MediaType.CDMRW: return AlcoholMediumType.CDRW;
                case MediaType.DVDR:
                case MediaType.DVDRW:
                case MediaType.DVDPR:
                case MediaType.DVDRDL:
                case MediaType.DVDRWDL:
                case MediaType.DVDPRDL:
                case MediaType.DVDPRWDL: return AlcoholMediumType.DVDR;
                default: return AlcoholMediumType.DVD;
            }
        }

        static AlcoholTrackMode TrackTypeToAlcohol(TrackType type)
        {
            switch(type)
            {
                case TrackType.Audio:           return AlcoholTrackMode.Audio;
                case TrackType.CdMode1:         return AlcoholTrackMode.Mode1;
                case TrackType.CdMode2Formless: return AlcoholTrackMode.Mode2;
                case TrackType.CdMode2Form1:    return AlcoholTrackMode.Mode2F1;
                case TrackType.CdMode2Form2:    return AlcoholTrackMode.Mode2F2;
                default:                        return AlcoholTrackMode.DVD;
            }
        }

        static (byte hour, byte minute, byte second, byte frame) LbaToMsf(ulong sector)
        {
            return ((byte)((sector + 150) / 75 / 60 / 60), (byte)((sector + 150) / 75 / 60 % 60),
                       (byte)((sector + 150)                                          / 75 % 60),
                       (byte)((sector + 150)                                               % 75));
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AlcoholHeader
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] signature;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] version;
            public AlcoholMediumType type;
            public ushort            sessions;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public ushort[] unknown1;
            public ushort bcaLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public uint[] unknown2;
            public uint bcaOffset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public uint[] unknown3;
            public uint structuresOffset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public uint[] unknown4;
            public uint sessionOffset;
            public uint dpmOffset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AlcoholSession
        {
            public int    sessionStart;
            public int    sessionEnd;
            public ushort sessionSequence;
            public byte   allBlocks;
            public byte   nonTrackBlocks;
            public ushort firstTrack;
            public ushort lastTrack;
            public uint   unknown;
            public uint   trackOffset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AlcoholTrack
        {
            public AlcoholTrackMode      mode;
            public AlcoholSubchannelMode subMode;
            public byte                  adrCtl;
            public byte                  tno;
            public byte                  point;
            public byte                  min;
            public byte                  sec;
            public byte                  frame;
            public byte                  zero;
            public byte                  pmin;
            public byte                  psec;
            public byte                  pframe;
            public uint                  extraOffset;
            public ushort                sectorSize;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
            public byte[] unknown;
            public uint  startLba;
            public ulong startOffset;
            public uint  files;
            public uint  footerOffset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
            public byte[] unknown2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AlcoholTrackExtra
        {
            public uint pregap;
            public uint sectors;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AlcoholFooter
        {
            public uint filenameOffset;
            public uint widechar;
            public uint unknown1;
            public uint unknown2;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        enum AlcoholMediumType : ushort
        {
            CD   = 0x00,
            CDR  = 0x01,
            CDRW = 0x02,
            DVD  = 0x10,
            DVDR = 0x12
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        enum AlcoholTrackMode : byte
        {
            NoData     = 0x00,
            DVD        = 0x02,
            Audio      = 0xA9,
            Mode1      = 0xAA,
            Mode2      = 0xAB,
            Mode2F1    = 0xAC,
            Mode2F2    = 0xAD,
            Mode2F1Alt = 0xEC
        }

        enum AlcoholSubchannelMode : byte
        {
            None        = 0x00,
            Interleaved = 0x08
        }
    }
}