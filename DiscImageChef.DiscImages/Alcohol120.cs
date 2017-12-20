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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.DiscImages
{
    public class Alcohol120 : ImagePlugin
    {
        #region Internal Structures
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AlcoholHeader
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string signature;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] version;
            public AlcoholMediumType type;
            public ushort sessions;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public ushort[] unknown1;
            public ushort bcaLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public uint[] unknown2;
            public uint bcaOffset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public uint[] unknown3;
            public uint structuresOffset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public uint[] unknown4;
            public uint sessionOffset;
            public uint dpmOffset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AlcoholSession
        {
            public int sessionStart;
            public int sessionEnd;
            public ushort sessionSequence;
            public byte allBlocks;
            public byte nonTrackBlocks;
            public ushort firstTrack;
            public ushort lastTrack;
            public uint unknown;
            public uint trackOffset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AlcoholTrack
        {
            public AlcoholTrackMode mode;
            public AlcoholSubchannelMode subMode;
            public byte adrCtl;
            public byte tno;
            public byte point;
            public byte min;
            public byte sec;
            public byte frame;
            public byte zero;
            public byte pmin;
            public byte psec;
            public byte pframe;
            public uint extraOffset;
            public ushort sectorSize;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)] public byte[] unknown;
            public uint startLba;
            public ulong startOffset;
            public uint files;
            public uint footerOffset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)] public byte[] unknown2;
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
        #endregion Internal Structures

        #region Internal enumerations
        enum AlcoholMediumType : ushort
        {
            CD = 0x00,
            CDR = 0x01,
            CDRW = 0x02,
            DVD = 0x10,
            DVDR = 0x12
        }

        enum AlcoholTrackMode : byte
        {
            NoData = 0x00,
            DVD = 0x02,
            Audio = 0xA9,
            Mode1 = 0xAA,
            Mode2 = 0xAB,
            Mode2F1 = 0xAC,
            Mode2F2 = 0xAD,
            Mode2F1Alt = 0xEC
        }

        enum AlcoholSubchannelMode : byte
        {
            None = 0x00,
            Interleaved = 0x08
        }
        #endregion Internal enumerations

        #region Internal variables
        Dictionary<uint, ulong> offsetmap;
        List<Partition> partitions;
        Dictionary<int, AlcoholSession> alcSessions;
        Dictionary<int, AlcoholTrack> alcTracks;
        Dictionary<int, Dictionary<int, AlcoholTrack>> alcToc;
        Dictionary<int, AlcoholTrackExtra> alcTrackExtras;
        AlcoholFooter alcFooter;
        Filter alcImage;
        byte[] bca;
        List<Session> sessions;
        Stream imageStream;
        byte[] fullToc;
        bool isDvd;
        byte[] dmi;
        byte[] pfi;
        #endregion

        #region Public Methods
        public Alcohol120()
        {
            Name = "Alcohol 120% Media Descriptor Structure";
            PluginUuid = new Guid("A78FBEBA-0307-4915-BDE3-B8A3B57F843F");
            ImageInfo = new ImageInfo();
            ImageInfo.ReadableSectorTags = new List<SectorTagType>();
            ImageInfo.ReadableMediaTags = new List<MediaTagType>();
            ImageInfo.ImageHasPartitions = false;
            ImageInfo.ImageHasSessions = false;
            ImageInfo.ImageVersion = null;
            ImageInfo.ImageApplication = null;
            ImageInfo.ImageApplicationVersion = null;
            ImageInfo.ImageCreator = null;
            ImageInfo.ImageComments = null;
            ImageInfo.MediaManufacturer = null;
            ImageInfo.MediaModel = null;
            ImageInfo.MediaSerialNumber = null;
            ImageInfo.MediaBarcode = null;
            ImageInfo.MediaPartNumber = null;
            ImageInfo.MediaSequence = 0;
            ImageInfo.LastMediaSequence = 0;
            ImageInfo.DriveManufacturer = null;
            ImageInfo.DriveModel = null;
            ImageInfo.DriveSerialNumber = null;
            ImageInfo.DriveFirmwareRevision = null;
        }

        public override bool IdentifyImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            if(stream.Length < 88) return false;

            byte[] hdr = new byte[88];
            stream.Read(hdr, 0, 88);
            AlcoholHeader header = new AlcoholHeader();
            IntPtr hdrPtr = Marshal.AllocHGlobal(88);
            Marshal.Copy(hdr, 0, hdrPtr, 88);
            header = (AlcoholHeader)Marshal.PtrToStructure(hdrPtr, typeof(AlcoholHeader));
            Marshal.FreeHGlobal(hdrPtr);

            return header.signature == "MEDIA DESCRIPTO";
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            if(stream.Length < 88) return false;

            isDvd = false;
            byte[] hdr = new byte[88];
            stream.Read(hdr, 0, 88);
            AlcoholHeader header = new AlcoholHeader();
            IntPtr hdrPtr = Marshal.AllocHGlobal(88);
            Marshal.Copy(hdr, 0, hdrPtr, 88);
            header = (AlcoholHeader)Marshal.PtrToStructure(hdrPtr, typeof(AlcoholHeader));
            Marshal.FreeHGlobal(hdrPtr);

            DicConsole.DebugWriteLine("Alcohol 120% plugin", "header.signature = {0}", header.signature);
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
                AlcoholSession session = new AlcoholSession();
                IntPtr sesPtr = Marshal.AllocHGlobal(24);
                Marshal.Copy(sesHdr, 0, sesPtr, 24);
                session = (AlcoholSession)Marshal.PtrToStructure(sesPtr, typeof(AlcoholSession));
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
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{1}].lastTrack = {0}", session.lastTrack, i);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{1}].unknown = 0x{0:X8}", session.unknown, i);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "session[{1}].trackOffset = {0}", session.trackOffset,
                                          i);

                alcSessions.Add(session.sessionSequence, session);
            }

            long footerOff = 0;

            alcTracks = new Dictionary<int, AlcoholTrack>();
            alcToc = new Dictionary<int, Dictionary<int, AlcoholTrack>>();
            foreach(AlcoholSession session in alcSessions.Values)
            {
                stream.Seek(session.trackOffset, SeekOrigin.Begin);
                Dictionary<int, AlcoholTrack> sesToc = new Dictionary<int, AlcoholTrack>();
                for(int i = 0; i < session.allBlocks; i++)
                {
                    byte[] trkHdr;
                    AlcoholTrack track;
                    IntPtr trkPtr;

                    trkHdr = new byte[80];
                    stream.Read(trkHdr, 0, 80);
                    track = new AlcoholTrack();
                    trkPtr = Marshal.AllocHGlobal(80);
                    Marshal.Copy(trkHdr, 0, trkPtr, 80);
                    track = (AlcoholTrack)Marshal.PtrToStructure(trkPtr, typeof(AlcoholTrack));
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

                    if(!sesToc.ContainsKey(track.point)) sesToc.Add(track.point, track);

                    if(track.point < 0xA0) alcTracks.Add(track.point, track);

                    if(footerOff == 0) footerOff = track.footerOffset;

                    isDvd |= track.mode == AlcoholTrackMode.DVD;
                }

                alcToc.Add(session.sessionSequence, sesToc);
            }

            alcTrackExtras = new Dictionary<int, AlcoholTrackExtra>();
            foreach(AlcoholTrack track in alcTracks.Values)
            {
                if(track.extraOffset > 0 && !isDvd)
                {
                    byte[] extHdr = new byte[8];
                    stream.Seek(track.extraOffset, SeekOrigin.Begin);
                    stream.Read(extHdr, 0, 8);
                    AlcoholTrackExtra extra = new AlcoholTrackExtra();
                    IntPtr extPtr = Marshal.AllocHGlobal(8);
                    Marshal.Copy(extHdr, 0, extPtr, 8);
                    extra = (AlcoholTrackExtra)Marshal.PtrToStructure(extPtr, typeof(AlcoholTrackExtra));
                    Marshal.FreeHGlobal(extPtr);

                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "track[{1}].extra.pregap = {0}", extra.pregap,
                                              track.point);
                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "track[{1}].extra.sectors = {0}", extra.sectors,
                                              track.point);

                    alcTrackExtras.Add(track.point, extra);
                }
                else if(isDvd)
                {
                    AlcoholTrackExtra extra = new AlcoholTrackExtra();
                    extra.sectors = track.extraOffset;
                    alcTrackExtras.Add(track.point, extra);
                }
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
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "footer.widechar = {0}", alcFooter.widechar);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "footer.unknown1 = 0x{0:X8}", alcFooter.unknown1);
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "footer.unknown2 = 0x{0:X8}", alcFooter.unknown2);
            }

            string alcFile = "*.mdf";

            if(alcFooter.filenameOffset > 0)
            {
                stream.Seek(alcFooter.filenameOffset, SeekOrigin.Begin);
                byte[] filename;
                if(header.dpmOffset == 0) filename = new byte[stream.Length - stream.Position];
                else filename = new byte[header.dpmOffset - stream.Position];

                stream.Read(filename, 0, filename.Length);
                if(alcFooter.widechar == 1) alcFile = Encoding.Unicode.GetString(filename);
                else alcFile = Encoding.Default.GetString(filename);

                DicConsole.DebugWriteLine("Alcohol 120% plugin", "footer.filename = {0}", alcFile);
            }

            if(alcFooter.filenameOffset == 0 ||
               string.Compare(alcFile, "*.mdf", StringComparison.InvariantCultureIgnoreCase) == 0)
                alcFile = Path.GetFileNameWithoutExtension(imageFilter.GetBasePath()) + ".mdf";

            if(header.bcaLength > 0 && header.bcaOffset > 0 && isDvd)
            {
                bca = new byte[header.bcaLength];
                stream.Seek(header.bcaOffset, SeekOrigin.Begin);
                int readBytes = stream.Read(bca, 0, bca.Length);

                if(readBytes == bca.Length)
                {
                    switch(header.type)
                    {
                        case AlcoholMediumType.DVD:
                        case AlcoholMediumType.DVDR:
                            ImageInfo.ReadableMediaTags.Add(MediaTagType.DVD_BCA);
                            break;
                    }
                }
            }

            ImageInfo.MediaType = AlcoholMediumTypeToMediaType(header.type);

            if(isDvd)
            {
                // TODO: Second layer
                if(header.structuresOffset >= 0)
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

                    Decoders.DVD.PFI.PhysicalFormatInformation? pfi0 = Decoders.DVD.PFI.Decode(pfi);

                    // All discs I tested the disk category and part version (as well as the start PSN for DVD-RAM) where modified by Alcohol
                    // So much for archival value
                    if(pfi0.HasValue)
                    {
                        switch(pfi0.Value.DiskCategory)
                        {
                            case Decoders.DVD.DiskCategory.DVDPR:
                                ImageInfo.MediaType = MediaType.DVDPR;
                                break;
                            case Decoders.DVD.DiskCategory.DVDPRDL:
                                ImageInfo.MediaType = MediaType.DVDPRDL;
                                break;
                            case Decoders.DVD.DiskCategory.DVDPRW:
                                ImageInfo.MediaType = MediaType.DVDPRW;
                                break;
                            case Decoders.DVD.DiskCategory.DVDPRWDL:
                                ImageInfo.MediaType = MediaType.DVDPRWDL;
                                break;
                            case Decoders.DVD.DiskCategory.DVDR:
                                if(pfi0.Value.PartVersion == 6) ImageInfo.MediaType = MediaType.DVDRDL;
                                else ImageInfo.MediaType = MediaType.DVDR;
                                break;
                            case Decoders.DVD.DiskCategory.DVDRAM:
                                ImageInfo.MediaType = MediaType.DVDRAM;
                                break;
                            default:
                                ImageInfo.MediaType = MediaType.DVDROM;
                                break;
                            case Decoders.DVD.DiskCategory.DVDRW:
                                if(pfi0.Value.PartVersion == 3) ImageInfo.MediaType = MediaType.DVDRWDL;
                                else ImageInfo.MediaType = MediaType.DVDRW;
                                break;
                            case Decoders.DVD.DiskCategory.HDDVDR:
                                ImageInfo.MediaType = MediaType.HDDVDR;
                                break;
                            case Decoders.DVD.DiskCategory.HDDVDRAM:
                                ImageInfo.MediaType = MediaType.HDDVDRAM;
                                break;
                            case Decoders.DVD.DiskCategory.HDDVDROM:
                                ImageInfo.MediaType = MediaType.HDDVDROM;
                                break;
                            case Decoders.DVD.DiskCategory.HDDVDRW:
                                ImageInfo.MediaType = MediaType.HDDVDRW;
                                break;
                            case Decoders.DVD.DiskCategory.Nintendo:
                                if(pfi0.Value.DiscSize == Decoders.DVD.DVDSize.Eighty)
                                    ImageInfo.MediaType = MediaType.GOD;
                                else ImageInfo.MediaType = MediaType.WOD;
                                break;
                            case Decoders.DVD.DiskCategory.UMD:
                                ImageInfo.MediaType = MediaType.UMD;
                                break;
                        }

                        if(Decoders.Xbox.DMI.IsXbox(dmi)) ImageInfo.MediaType = MediaType.XGD;
                        else if(Decoders.Xbox.DMI.IsXbox360(dmi)) ImageInfo.MediaType = MediaType.XGD2;

                        ImageInfo.ReadableMediaTags.Add(MediaTagType.DVD_PFI);
                        ImageInfo.ReadableMediaTags.Add(MediaTagType.DVD_DMI);
                    }
                }
            }
            else if(header.type == AlcoholMediumType.CD)
            {
                bool data = false;
                bool mode2 = false;
                bool firstaudio = false;
                bool firstdata = false;
                bool audio = false;

                foreach(AlcoholTrack _track in alcTracks.Values)
                {
                    // First track is audio
                    firstaudio |= _track.point == 1 && _track.mode == AlcoholTrackMode.Audio;

                    // First track is data
                    firstdata |= _track.point == 1 && _track.mode != AlcoholTrackMode.Audio;

                    // Any non first track is data
                    data |= _track.point != 1 && _track.mode != AlcoholTrackMode.Audio;

                    // Any non first track is audio
                    audio |= _track.point != 1 && _track.mode == AlcoholTrackMode.Audio;

                    switch(_track.mode)
                    {
                        case AlcoholTrackMode.Mode2:
                        case AlcoholTrackMode.Mode2F1:
                        case AlcoholTrackMode.Mode2F2:
                            mode2 = true;
                            break;
                    }
                }

                if(!data && !firstdata) ImageInfo.MediaType = MediaType.CDDA;
                else if(firstaudio && data && sessions.Count > 1 && mode2) ImageInfo.MediaType = MediaType.CDPLUS;
                else if(firstdata && audio || mode2) ImageInfo.MediaType = MediaType.CDROMXA;
                else if(!audio) ImageInfo.MediaType = MediaType.CDROM;
                else ImageInfo.MediaType = MediaType.CD;
            }

            DicConsole.DebugWriteLine("Alcohol 120% plugin", "ImageInfo.mediaType = {0}", ImageInfo.MediaType);

            sessions = new List<Session>();
            foreach(AlcoholSession alcSes in alcSessions.Values)
            {
                Session session = new Session();
                AlcoholTrack stTrk;
                AlcoholTrack enTrk;
                AlcoholTrackExtra enTrkExt;

                if(!alcTracks.TryGetValue(alcSes.firstTrack, out stTrk)) break;
                if(!alcTracks.TryGetValue(alcSes.lastTrack, out enTrk)) break;
                if(!alcTrackExtras.TryGetValue(alcSes.lastTrack, out enTrkExt)) break;

                session.StartSector = stTrk.startLba;
                session.StartTrack = alcSes.firstTrack;
                session.SessionSequence = alcSes.sessionSequence;
                session.EndSector = enTrk.startLba + enTrkExt.sectors - 1;
                session.EndTrack = alcSes.lastTrack;

                sessions.Add(session);
            }

            partitions = new List<Partition>();
            offsetmap = new Dictionary<uint, ulong>();
            ulong byte_offset = 0;

            foreach(AlcoholTrack trk in alcTracks.Values)
            {
                AlcoholTrackExtra extra;
                if(alcTrackExtras.TryGetValue(trk.point, out extra))
                {
                    Partition partition = new Partition();

                    partition.Description = string.Format("Track {0}.", trk.point);
                    partition.Start = trk.startLba;
                    partition.Size = extra.sectors * trk.sectorSize;
                    partition.Length = extra.sectors;
                    partition.Sequence = trk.point;
                    partition.Offset = byte_offset;
                    partition.Type = trk.mode.ToString();

                    partitions.Add(partition);
                    ImageInfo.Sectors += extra.sectors;
                    byte_offset += partition.Size;
                }

                if(!offsetmap.ContainsKey(trk.point)) offsetmap.Add(trk.point, trk.startLba);

                switch(trk.mode)
                {
                    case AlcoholTrackMode.Mode1:
                    case AlcoholTrackMode.Mode2F1:
                    case AlcoholTrackMode.Mode2F1Alt:
                        if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                            ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);
                        if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                            ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);
                        if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                            ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);
                        if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEcc))
                            ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEcc);
                        if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccP))
                            ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccP);
                        if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccQ))
                            ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccQ);
                        if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                            ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);
                        if(ImageInfo.SectorSize < 2048) ImageInfo.SectorSize = 2048;
                        break;
                    case AlcoholTrackMode.Mode2:
                        if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                            ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);
                        if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                            ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);
                        if(ImageInfo.SectorSize < 2336) ImageInfo.SectorSize = 2336;
                        break;
                    case AlcoholTrackMode.Mode2F2:
                        if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                            ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);
                        if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                            ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);
                        if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                            ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);
                        if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                            ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);
                        if(ImageInfo.SectorSize < 2324) ImageInfo.SectorSize = 2324;
                        break;
                    case AlcoholTrackMode.DVD:
                        ImageInfo.SectorSize = 2048;
                        break;
                    default:
                        ImageInfo.SectorSize = 2352;
                        break;
                }
            }

            DicConsole.DebugWriteLine("Alcohol 120% plugin", "printing partition map");
            foreach(Partition partition in partitions)
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

            ImageInfo.ImageApplication = "Alcohol 120%";

            DicConsole.DebugWriteLine("Alcohol 120% plugin", "Data filename: {0}", alcFile);

            FiltersList filtersList = new FiltersList();
            alcImage = filtersList.GetFilter(alcFile);

            if(alcImage == null) throw new Exception("Cannot open data file");

            ImageInfo.ImageSize = (ulong)alcImage.GetDataForkLength();
            ImageInfo.ImageCreationTime = alcImage.GetCreationTime();
            ImageInfo.ImageLastModificationTime = alcImage.GetLastWriteTime();
            ImageInfo.XmlMediaType = XmlMediaType.OpticalDisc;
            ImageInfo.ImageVersion = string.Format("{0}.{1}", header.version[0], header.version[1]);

            if(!isDvd)
            {
                DicConsole.DebugWriteLine("Alcohol 120% plugin", "Rebuilding TOC");
                byte firstSession = byte.MaxValue;
                byte lastSession = 0;
                MemoryStream tocMs = new MemoryStream();
                tocMs.Write(new byte[] {0, 0, 0, 0}, 0, 4); // Reserved for TOC response size and session numbers
                foreach(KeyValuePair<int, Dictionary<int, AlcoholTrack>> sessionToc in alcToc)
                {
                    if(sessionToc.Key < firstSession) firstSession = (byte)sessionToc.Key;
                    if(sessionToc.Key > lastSession) lastSession = (byte)sessionToc.Key;

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
                BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
                byte[] fullTocSize = BigEndianBitConverter.GetBytes((short)(fullToc.Length - 2));
                fullToc[0] = fullTocSize[0];
                fullToc[1] = fullTocSize[1];
                fullToc[2] = firstSession;
                fullToc[3] = lastSession;

                Decoders.CD.FullTOC.CDFullTOC? decodedFullToc = Decoders.CD.FullTOC.Decode(fullToc);

                if(!decodedFullToc.HasValue)
                {
                    DicConsole.DebugWriteLine("Alcohol 120% plugin", "TOC not correctly rebuilt");
                    fullToc = null;
                }
                else ImageInfo.ReadableMediaTags.Add(MediaTagType.CD_FullTOC);

                ImageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackFlags);
            }

            if(ImageInfo.MediaType == MediaType.XGD2)
            {
                // All XGD3 all have the same number of blocks
                if(ImageInfo.Sectors == 25063 || // Locked (or non compatible drive)
                   ImageInfo.Sectors == 4229664 || // Xtreme unlock
                   ImageInfo.Sectors == 4246304) // Wxripper unlock
                    ImageInfo.MediaType = MediaType.XGD3;
            }

            DicConsole.VerboseWriteLine("Alcohol 120% image describes a disc of type {0}", ImageInfo.MediaType);

            return true;
        }

        public override bool ImageHasPartitions()
        {
            return ImageInfo.ImageHasPartitions;
        }

        public override ulong GetImageSize()
        {
            return ImageInfo.ImageSize;
        }

        public override ulong GetSectors()
        {
            return ImageInfo.Sectors;
        }

        public override uint GetSectorSize()
        {
            return ImageInfo.SectorSize;
        }

        public override byte[] ReadDiskTag(MediaTagType tag)
        {
            switch(tag)
            {
                case MediaTagType.DVD_BCA:
                {
                    if(bca != null) { return (byte[])bca.Clone(); }

                    throw new FeatureNotPresentImageException("Image does not contain BCA information.");
                }
                case MediaTagType.DVD_PFI:
                {
                    if(pfi != null) { return (byte[])pfi.Clone(); }

                    throw new FeatureNotPresentImageException("Image does not contain PFI.");
                }
                case MediaTagType.DVD_DMI:
                {
                    if(dmi != null) { return (byte[])dmi.Clone(); }

                    throw new FeatureNotPresentImageException("Image does not contain DMI.");
                }
                case MediaTagType.CD_FullTOC:
                {
                    if(fullToc != null) { return (byte[])fullToc.Clone(); }

                    throw new FeatureNotPresentImageException("Image does not contain TOC information.");
                }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
            }
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            return ReadSectorsTag(sectorAddress, 1, tag);
        }

        public override byte[] ReadSector(ulong sectorAddress, uint track)
        {
            return ReadSectors(sectorAddress, 1, track);
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            return ReadSectorsTag(sectorAddress, 1, track, tag);
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in offsetmap)
            {
                if(sectorAddress >= kvp.Value)
                {
                    foreach(AlcoholTrack track in alcTracks.Values)
                    {
                        AlcoholTrackExtra extra;

                        if(track.point == kvp.Key && alcTrackExtras.TryGetValue(track.point, out extra))
                        {
                            if(sectorAddress - kvp.Value < extra.sectors)
                                return ReadSectors(sectorAddress - kvp.Value, length, kvp.Key);
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            foreach(KeyValuePair<uint, ulong> kvp in offsetmap)
            {
                if(sectorAddress >= kvp.Value)
                {
                    foreach(AlcoholTrack track in alcTracks.Values)
                    {
                        AlcoholTrackExtra extra;

                        if(track.point == kvp.Key && alcTrackExtras.TryGetValue(track.point, out extra))
                        {
                            if(sectorAddress - kvp.Value < extra.sectors)
                                return ReadSectorsTag(sectorAddress - kvp.Value, length, kvp.Key, tag);
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            AlcoholTrack _track;
            AlcoholTrackExtra _extra;

            if(!alcTracks.TryGetValue((int)track, out _track) || !alcTrackExtras.TryGetValue((int)track, out _extra))
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > _extra.sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      string
                                                          .Format("Requested more sectors ({0}) than present in track ({1}), won't cross tracks",
                                                                  length + sectorAddress, _extra.sectors));

            uint sector_offset;
            uint sector_size;
            uint sector_skip;

            switch(_track.mode)
            {
                case AlcoholTrackMode.Mode1:
                {
                    sector_offset = 16;
                    sector_size = 2048;
                    sector_skip = 288;
                    break;
                }
                case AlcoholTrackMode.Mode2:
                {
                    sector_offset = 16;
                    sector_size = 2336;
                    sector_skip = 0;
                    break;
                }
                case AlcoholTrackMode.Mode2F1:
                case AlcoholTrackMode.Mode2F1Alt:
                {
                    sector_offset = 24;
                    sector_size = 2048;
                    sector_skip = 280;
                    break;
                }
                case AlcoholTrackMode.Mode2F2:
                {
                    sector_offset = 24;
                    sector_size = 2324;
                    sector_skip = 4;
                    break;
                }
                case AlcoholTrackMode.Audio:
                {
                    sector_offset = 0;
                    sector_size = 2352;
                    sector_skip = 0;
                    break;
                }
                case AlcoholTrackMode.DVD:
                {
                    sector_offset = 0;
                    sector_size = 2048;
                    sector_skip = 0;
                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            switch(_track.subMode)
            {
                case AlcoholSubchannelMode.None:
                    sector_skip += 0;
                    break;
                case AlcoholSubchannelMode.Interleaved:
                    sector_skip += 96;
                    break;
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported subchannel type");
            }

            byte[] buffer = new byte[sector_size * length];

            imageStream = alcImage.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);
            br.BaseStream
              .Seek((long)_track.startOffset + (long)(sectorAddress * (sector_offset + sector_size + sector_skip)),
                    SeekOrigin.Begin);
            if(sector_offset == 0 && sector_skip == 0) buffer = br.ReadBytes((int)(sector_size * length));
            else
            {
                for(int i = 0; i < length; i++)
                {
                    byte[] sector;
                    br.BaseStream.Seek(sector_offset, SeekOrigin.Current);
                    sector = br.ReadBytes((int)sector_size);
                    br.BaseStream.Seek(sector_skip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sector_size, sector_size);
                }
            }

            return buffer;
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            AlcoholTrack _track;
            AlcoholTrackExtra _extra;

            if(!alcTracks.TryGetValue((int)track, out _track) || !alcTrackExtras.TryGetValue((int)track, out _extra))
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > _extra.sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      string
                                                          .Format("Requested more sectors ({0}) than present in track ({1}), won't cross tracks",
                                                                  length, _extra.sectors));

            uint sector_offset;
            uint sector_size;
            uint sector_skip;

            if(_track.mode == AlcoholTrackMode.DVD)
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
                case SectorTagType.CdTrackFlags: return new byte[] {(byte)(_track.adrCtl & 0x0F)};
                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
            }

            switch(_track.mode)
            {
                case AlcoholTrackMode.Mode1:
                    switch(tag)
                    {
                        case SectorTagType.CdSectorSync:
                        {
                            sector_offset = 0;
                            sector_size = 12;
                            sector_skip = 2340;
                            break;
                        }
                        case SectorTagType.CdSectorHeader:
                        {
                            sector_offset = 12;
                            sector_size = 4;
                            sector_skip = 2336;
                            break;
                        }
                        case SectorTagType.CdSectorSubHeader:
                            throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                        case SectorTagType.CdSectorEcc:
                        {
                            sector_offset = 2076;
                            sector_size = 276;
                            sector_skip = 0;
                            break;
                        }
                        case SectorTagType.CdSectorEccP:
                        {
                            sector_offset = 2076;
                            sector_size = 172;
                            sector_skip = 104;
                            break;
                        }
                        case SectorTagType.CdSectorEccQ:
                        {
                            sector_offset = 2248;
                            sector_size = 104;
                            sector_skip = 0;
                            break;
                        }
                        case SectorTagType.CdSectorEdc:
                        {
                            sector_offset = 2064;
                            sector_size = 4;
                            sector_skip = 284;
                            break;
                        }
                        case SectorTagType.CdSectorSubchannel:
                        {
                            switch(_track.subMode)
                            {
                                case AlcoholSubchannelMode.Interleaved:

                                    sector_offset = 2352;
                                    sector_size = 96;
                                    sector_skip = 0;
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
                            sector_offset = 0;
                            sector_size = 8;
                            sector_skip = 2328;
                            break;
                        }
                        case SectorTagType.CdSectorEdc:
                        {
                            sector_offset = 2332;
                            sector_size = 4;
                            sector_skip = 0;
                            break;
                        }
                        case SectorTagType.CdSectorSubchannel:
                        {
                            switch(_track.subMode)
                            {
                                case AlcoholSubchannelMode.Interleaved:

                                    sector_offset = 2352;
                                    sector_size = 96;
                                    sector_skip = 0;
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
                            sector_offset = 0;
                            sector_size = 12;
                            sector_skip = 2340;
                            break;
                        }
                        case SectorTagType.CdSectorHeader:
                        {
                            sector_offset = 12;
                            sector_size = 4;
                            sector_skip = 2336;
                            break;
                        }
                        case SectorTagType.CdSectorSubHeader:
                        {
                            sector_offset = 16;
                            sector_size = 8;
                            sector_skip = 2328;
                            break;
                        }
                        case SectorTagType.CdSectorEcc:
                        {
                            sector_offset = 2076;
                            sector_size = 276;
                            sector_skip = 0;
                            break;
                        }
                        case SectorTagType.CdSectorEccP:
                        {
                            sector_offset = 2076;
                            sector_size = 172;
                            sector_skip = 104;
                            break;
                        }
                        case SectorTagType.CdSectorEccQ:
                        {
                            sector_offset = 2248;
                            sector_size = 104;
                            sector_skip = 0;
                            break;
                        }
                        case SectorTagType.CdSectorEdc:
                        {
                            sector_offset = 2072;
                            sector_size = 4;
                            sector_skip = 276;
                            break;
                        }
                        case SectorTagType.CdSectorSubchannel:
                        {
                            switch(_track.subMode)
                            {
                                case AlcoholSubchannelMode.Interleaved:

                                    sector_offset = 2352;
                                    sector_size = 96;
                                    sector_skip = 0;
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
                            sector_offset = 0;
                            sector_size = 12;
                            sector_skip = 2340;
                            break;
                        }
                        case SectorTagType.CdSectorHeader:
                        {
                            sector_offset = 12;
                            sector_size = 4;
                            sector_skip = 2336;
                            break;
                        }
                        case SectorTagType.CdSectorSubHeader:
                        {
                            sector_offset = 16;
                            sector_size = 8;
                            sector_skip = 2328;
                            break;
                        }
                        case SectorTagType.CdSectorEdc:
                        {
                            sector_offset = 2348;
                            sector_size = 4;
                            sector_skip = 0;
                            break;
                        }
                        case SectorTagType.CdSectorSubchannel:
                        {
                            switch(_track.subMode)
                            {
                                case AlcoholSubchannelMode.Interleaved:

                                    sector_offset = 2352;
                                    sector_size = 96;
                                    sector_skip = 0;
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
                            switch(_track.subMode)
                            {
                                case AlcoholSubchannelMode.Interleaved:

                                    sector_offset = 2352;
                                    sector_size = 96;
                                    sector_skip = 0;
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

            switch(_track.subMode)
            {
                case AlcoholSubchannelMode.None:
                    sector_skip += 0;
                    break;
                case AlcoholSubchannelMode.Interleaved:
                    if(tag != SectorTagType.CdSectorSubchannel) sector_skip += 96;
                    break;
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported subchannel type");
            }

            byte[] buffer = new byte[sector_size * length];

            imageStream = alcImage.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);
            br.BaseStream
              .Seek((long)_track.startOffset + (long)(sectorAddress * (sector_offset + sector_size + sector_skip)),
                    SeekOrigin.Begin);
            if(sector_offset == 0 && sector_skip == 0) buffer = br.ReadBytes((int)(sector_size * length));
            else
            {
                for(int i = 0; i < length; i++)
                {
                    byte[] sector;
                    br.BaseStream.Seek(sector_offset, SeekOrigin.Current);
                    sector = br.ReadBytes((int)sector_size);
                    br.BaseStream.Seek(sector_skip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sector_size, sector_size);
                }
            }

            return buffer;
        }

        public override byte[] ReadSectorLong(ulong sectorAddress)
        {
            return ReadSectorsLong(sectorAddress, 1);
        }

        public override byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            return ReadSectorsLong(sectorAddress, 1, track);
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in offsetmap)
            {
                if(sectorAddress >= kvp.Value)
                {
                    foreach(AlcoholTrack track in alcTracks.Values)
                    {
                        AlcoholTrackExtra extra;

                        if(track.point == kvp.Key && alcTrackExtras.TryGetValue(track.point, out extra))
                        {
                            if(sectorAddress - kvp.Value < extra.sectors)
                                return ReadSectorsLong(sectorAddress - kvp.Value, length, kvp.Key);
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            AlcoholTrack _track;
            AlcoholTrackExtra _extra;

            if(!alcTracks.TryGetValue((int)track, out _track) || !alcTrackExtras.TryGetValue((int)track, out _extra))
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > _extra.sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      string
                                                          .Format("Requested more sectors ({0}) than present in track ({1}), won't cross tracks",
                                                                  length, _extra.sectors));

            uint sector_offset;
            uint sector_size;
            uint sector_skip;

            switch(_track.mode)
            {
                case AlcoholTrackMode.Mode1:
                case AlcoholTrackMode.Mode2:
                case AlcoholTrackMode.Mode2F1:
                case AlcoholTrackMode.Mode2F1Alt:
                case AlcoholTrackMode.Mode2F2:
                case AlcoholTrackMode.Audio:
                case AlcoholTrackMode.DVD:
                {
                    sector_offset = 0;
                    sector_size = _track.sectorSize;
                    sector_skip = 0;
                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sector_size * length];

            imageStream = alcImage.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);
            br.BaseStream
              .Seek((long)_track.startOffset + (long)(sectorAddress * (sector_offset + sector_size + sector_skip)),
                    SeekOrigin.Begin);
            buffer = br.ReadBytes((int)(sector_size * length));

            return buffer;
        }

        public override string GetImageFormat()
        {
            return "Alcohol 120% Media Descriptor Structure";
        }

        public override string GetImageVersion()
        {
            return ImageInfo.ImageVersion;
        }

        public override string GetImageApplication()
        {
            return ImageInfo.ImageApplication;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.MediaType;
        }

        public override List<Partition> GetPartitions()
        {
            return partitions;
        }

        public override List<Track> GetTracks()
        {
            List<Track> tracks = new List<Track>();

            foreach(AlcoholTrack track in alcTracks.Values)
            {
                ushort sessionNo = 0;

                foreach(Session session in sessions)
                {
                    if(track.point >= session.StartTrack || track.point <= session.EndTrack)
                    {
                        sessionNo = session.SessionSequence;
                        break;
                    }
                }

                AlcoholTrackExtra extra;
                if(alcTrackExtras.TryGetValue(track.point, out extra))
                {
                    Track _track = new Track();

                    _track.Indexes = new Dictionary<int, ulong>();
                    _track.Indexes.Add(1, track.startLba);
                    _track.TrackStartSector = track.startLba;
                    _track.TrackEndSector = extra.sectors - 1;
                    _track.TrackPregap = extra.pregap;
                    _track.TrackSession = sessionNo;
                    _track.TrackSequence = track.point;
                    _track.TrackType = AlcoholTrackTypeToTrackType(track.mode);
                    _track.TrackFilter = alcImage;
                    _track.TrackFile = alcImage.GetFilename();
                    _track.TrackFileOffset = track.startOffset;
                    _track.TrackFileType = "BINARY";
                    _track.TrackRawBytesPerSector = track.sectorSize;
                    _track.TrackBytesPerSector = AlcoholTrackModeToCookedBytesPerSector(track.mode);
                    switch(track.subMode)
                    {
                        case AlcoholSubchannelMode.Interleaved:
                            _track.TrackSubchannelFilter = alcImage;
                            _track.TrackSubchannelFile = alcImage.GetFilename();
                            _track.TrackSubchannelOffset = track.startOffset;
                            _track.TrackSubchannelType = TrackSubchannelType.RawInterleaved;
                            _track.TrackRawBytesPerSector += 96;
                            break;
                        case AlcoholSubchannelMode.None:
                            _track.TrackSubchannelType = TrackSubchannelType.None;
                            break;
                    }

                    tracks.Add(_track);
                }
            }

            return tracks;
        }

        public override List<Track> GetSessionTracks(Session session)
        {
            if(sessions.Contains(session)) { return GetSessionTracks(session.SessionSequence); }

            throw new ImageNotSupportedException("Session does not exist in disc image");
        }

        public override List<Track> GetSessionTracks(ushort session)
        {
            List<Track> tracks = new List<Track>();

            foreach(AlcoholTrack track in alcTracks.Values)
            {
                ushort sessionNo = 0;

                foreach(Session ses in sessions)
                {
                    if(track.point >= ses.StartTrack || track.point <= ses.EndTrack)
                    {
                        sessionNo = ses.SessionSequence;
                        break;
                    }
                }

                AlcoholTrackExtra extra;
                if(alcTrackExtras.TryGetValue(track.point, out extra) && session == sessionNo)
                {
                    Track _track = new Track();

                    _track.Indexes = new Dictionary<int, ulong>();
                    _track.Indexes.Add(1, track.startLba);
                    _track.TrackStartSector = track.startLba;
                    _track.TrackEndSector = extra.sectors - 1;
                    _track.TrackPregap = extra.pregap;
                    _track.TrackSession = sessionNo;
                    _track.TrackSequence = track.point;
                    _track.TrackType = AlcoholTrackTypeToTrackType(track.mode);
                    _track.TrackFilter = alcImage;
                    _track.TrackFile = alcImage.GetFilename();
                    _track.TrackFileOffset = track.startOffset;
                    _track.TrackFileType = "BINARY";
                    _track.TrackRawBytesPerSector = track.sectorSize;
                    _track.TrackBytesPerSector = AlcoholTrackModeToCookedBytesPerSector(track.mode);
                    switch(track.subMode)
                    {
                        case AlcoholSubchannelMode.Interleaved:
                            _track.TrackSubchannelFilter = alcImage;
                            _track.TrackSubchannelFile = alcImage.GetFilename();
                            _track.TrackSubchannelOffset = track.startOffset;
                            _track.TrackSubchannelType = TrackSubchannelType.RawInterleaved;
                            _track.TrackRawBytesPerSector += 96;
                            break;
                        case AlcoholSubchannelMode.None:
                            _track.TrackSubchannelType = TrackSubchannelType.None;
                            break;
                    }

                    tracks.Add(_track);
                }
            }

            return tracks;
        }

        public override List<Session> GetSessions()
        {
            return sessions;
        }

        public override bool? VerifySector(ulong sectorAddress)
        {
            byte[] buffer = ReadSectorLong(sectorAddress);
            return Checksums.CdChecksums.CheckCdSector(buffer);
        }

        public override bool? VerifySector(ulong sectorAddress, uint track)
        {
            byte[] buffer = ReadSectorLong(sectorAddress, track);
            return Checksums.CdChecksums.CheckCdSector(buffer);
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            byte[] buffer = ReadSectorsLong(sectorAddress, length);
            int bps = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * bps, sector, 0, bps);
                bool? sectorStatus = Checksums.CdChecksums.CheckCdSector(sector);

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
            if(failingLbas.Count > 0) return false;

            return true;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            byte[] buffer = ReadSectorsLong(sectorAddress, length, track);
            int bps = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * bps, sector, 0, bps);
                bool? sectorStatus = Checksums.CdChecksums.CheckCdSector(sector);

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
            if(failingLbas.Count > 0) return false;

            return true;
        }

        public override bool? VerifyMediaImage()
        {
            return null;
        }
        #endregion Public Methods

        #region Private methods
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
                default: return 0;
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
                case AlcoholTrackMode.Mode2: return 2336;
                case AlcoholTrackMode.Audio: return 2352;
                case AlcoholTrackMode.DVD: return 2048;
                default: return 0;
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
                case AlcoholTrackMode.Mode2: return TrackType.CdMode2Formless;
                case AlcoholTrackMode.Audio: return TrackType.Audio;
                default: return TrackType.Data;
            }
        }

        static MediaType AlcoholMediumTypeToMediaType(AlcoholMediumType discType)
        {
            switch(discType)
            {
                case AlcoholMediumType.CD: return MediaType.CD;
                case AlcoholMediumType.CDR: return MediaType.CDR;
                case AlcoholMediumType.CDRW: return MediaType.CDRW;
                case AlcoholMediumType.DVD: return MediaType.DVDROM;
                case AlcoholMediumType.DVDR: return MediaType.DVDR;
                default: return MediaType.Unknown;
            }
        }
        #endregion Private methods

        #region Unsupported features
        public override string GetImageApplicationVersion()
        {
            return ImageInfo.ImageApplicationVersion;
        }

        public override DateTime GetImageCreationTime()
        {
            return ImageInfo.ImageCreationTime;
        }

        public override DateTime GetImageLastModificationTime()
        {
            return ImageInfo.ImageLastModificationTime;
        }

        public override string GetImageComments()
        {
            return ImageInfo.ImageComments;
        }

        public override string GetMediaSerialNumber()
        {
            return ImageInfo.MediaSerialNumber;
        }

        public override string GetMediaBarcode()
        {
            return ImageInfo.MediaBarcode;
        }

        public override int GetMediaSequence()
        {
            return ImageInfo.MediaSequence;
        }

        public override int GetLastDiskSequence()
        {
            return ImageInfo.LastMediaSequence;
        }

        public override string GetDriveManufacturer()
        {
            return ImageInfo.DriveManufacturer;
        }

        public override string GetDriveModel()
        {
            return ImageInfo.DriveModel;
        }

        public override string GetDriveSerialNumber()
        {
            return ImageInfo.DriveSerialNumber;
        }

        public override string GetMediaPartNumber()
        {
            return ImageInfo.MediaPartNumber;
        }

        public override string GetMediaManufacturer()
        {
            return ImageInfo.MediaManufacturer;
        }

        public override string GetMediaModel()
        {
            return ImageInfo.MediaModel;
        }

        public override string GetImageName()
        {
            return ImageInfo.ImageName;
        }

        public override string GetImageCreator()
        {
            return ImageInfo.ImageCreator;
        }
        #endregion Unsupported features
    }
}