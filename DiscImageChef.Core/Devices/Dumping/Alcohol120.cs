// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Alcohol120.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains code for creating Alcohol 120% disc images.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;

namespace DiscImageChef.Core.Devices.Dumping
{
    // TODO: For >4.0, this class must disappear
    class Alcohol120
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
            Cd = 0x00,
            Cdr = 0x01,
            Cdrw = 0x02,
            Dvd = 0x10,
            Dvdr = 0x12
        }

        enum AlcoholTrackMode : byte
        {
            NoData = 0x00,
            Dvd = 0x02,
            Audio = 0xA9,
            Mode1 = 0xAA,
            Mode2 = 0xAB,
            Mode2F1 = 0xAC,
            Mode2F2 = 0xAD,
        }

        enum AlcoholSubchannelMode : byte
        {
            None = 0x00,
            Interleaved = 0x08
        }
        #endregion Internal enumerations

        string outputPrefix;
        string extension;

        byte[] bca;
        byte[] pfi;
        byte[] dmi;
        AlcoholHeader header;
        List<AlcoholTrack> tracks;
        List<AlcoholSession> sessions;
        Dictionary<byte, uint> trackLengths;
        AlcoholFooter footer;

        internal Alcohol120(string outputPrefix)
        {
            this.outputPrefix = outputPrefix;
            header = new AlcoholHeader
            {
                signature = "MEDIA DESCRIPTOR",
                unknown1 = new ushort[] {0x0002, 0x0000},
                unknown2 = new uint[2],
                unknown3 = new uint[6],
                unknown4 = new uint[3],
                version = new byte[] {1, 5}
            };
            header.version[0] = 1;
            header.version[1] = 5;
            tracks = new List<AlcoholTrack>();
            sessions = new List<AlcoholSession>();
            trackLengths = new Dictionary<byte, uint>();
            footer = new AlcoholFooter {widechar = 1};
        }

        internal void Close()
        {
            if(sessions.Count == 0 || tracks.Count == 0) return;

            // Calculate first offsets
            header.sessions = (ushort)sessions.Count;
            header.sessionOffset = 88;
            long nextOffset = 88 + sessions.Count * 24;

            // Calculate session blocks
            AlcoholSession[] sessionsArray = sessions.ToArray();
            for(int i = 0; i < sessionsArray.Length; i++)
            {
                sessionsArray[i].allBlocks = (byte)(sessionsArray[i].lastTrack - sessionsArray[i].firstTrack + 1 +
                                                    sessionsArray[i].nonTrackBlocks);
                sessionsArray[i].trackOffset = (uint)nextOffset;
                nextOffset += sessionsArray[i].allBlocks * 80;
            }

            // Calculate track blocks
            AlcoholTrack[] tracksArray = tracks.ToArray();
            AlcoholTrackExtra[] extrasArray = new AlcoholTrackExtra[trackLengths.Count];
            for(int i = 0; i < tracksArray.Length; i++)
            {
                if(tracksArray[i].point >= 0xA0) continue;
                if(!trackLengths.TryGetValue(tracksArray[i].point, out uint trkLen)) continue;

                if(tracksArray[i].mode == AlcoholTrackMode.Dvd) tracksArray[i].extraOffset = trkLen;
                else
                {
                    AlcoholTrackExtra extra = new AlcoholTrackExtra();
                    if(tracksArray[i].point == 1)
                    {
                        extra.pregap = 150;
                        sessionsArray[0].sessionStart = -150;
                    }

                    extra.sectors = trkLen;
                    extrasArray[tracksArray[i].point - 1] = extra;
                    tracksArray[i].extraOffset = (uint)nextOffset;
                    nextOffset += 8;
                }
            }

            // DVD things
            if(bca != null && bca.Length > 0)
            {
                header.bcaOffset = (uint)nextOffset;
                header.bcaLength = (ushort)bca.Length;
                nextOffset += bca.Length;
            }
            if(pfi != null && pfi.Length > 0 && dmi != null && dmi.Length > 0)
            {
                header.structuresOffset = (uint)nextOffset;
                nextOffset += 4100;
            }

            for(int i = 0; i < tracksArray.Length; i++) tracksArray[i].footerOffset = (uint)nextOffset;

            footer.filenameOffset = (uint)(nextOffset + 16);

            byte[] filename = Encoding.Unicode.GetBytes(outputPrefix + extension);

            // Open descriptor file here
            FileStream descriptorFile =
                new FileStream(outputPrefix + ".mds", FileMode.Create, FileAccess.ReadWrite, FileShare.None);

            byte[] tmp = new byte[88];
            IntPtr hdrPtr = Marshal.AllocHGlobal(88);
            Marshal.StructureToPtr(header, hdrPtr, false);
            Marshal.Copy(hdrPtr, tmp, 0, 88);

            descriptorFile.Write(tmp, 0, tmp.Length);

            foreach(AlcoholSession session in sessionsArray)
            {
                tmp = new byte[24];
                IntPtr sesPtr = Marshal.AllocHGlobal(24);
                Marshal.StructureToPtr(session, sesPtr, false);
                Marshal.Copy(sesPtr, tmp, 0, 24);
                descriptorFile.Write(tmp, 0, tmp.Length);
                Marshal.FreeHGlobal(sesPtr);
            }

            foreach(AlcoholTrack track in tracksArray)
            {
                tmp = new byte[80];
                IntPtr trkPtr = Marshal.AllocHGlobal(80);
                Marshal.StructureToPtr(track, trkPtr, false);
                Marshal.Copy(trkPtr, tmp, 0, 80);
                descriptorFile.Write(tmp, 0, tmp.Length);
                Marshal.FreeHGlobal(trkPtr);
            }

            if(header.type == AlcoholMediumType.Cd || header.type == AlcoholMediumType.Cdr ||
               header.type == AlcoholMediumType.Cdrw)
                foreach(AlcoholTrackExtra extra in extrasArray)
                {
                    tmp = new byte[8];
                    IntPtr trkxPtr = Marshal.AllocHGlobal(8);
                    Marshal.StructureToPtr(extra, trkxPtr, false);
                    Marshal.Copy(trkxPtr, tmp, 0, 8);
                    descriptorFile.Write(tmp, 0, tmp.Length);
                    Marshal.FreeHGlobal(trkxPtr);
                }

            if(bca != null && bca.Length > 0)
            {
                header.bcaOffset = (uint)descriptorFile.Position;
                header.bcaLength = (ushort)bca.Length;
                descriptorFile.Write(bca, 0, bca.Length);
            }

            if(pfi != null && pfi.Length > 0 && dmi != null && dmi.Length > 0)
            {
                descriptorFile.Write(new byte[4], 0, 4);
                descriptorFile.Write(dmi, 0, 2048);
                descriptorFile.Write(pfi, 0, 2048);
            }

            tmp = new byte[16];

            IntPtr ftrPtr = Marshal.AllocHGlobal(16);
            Marshal.StructureToPtr(footer, ftrPtr, false);
            Marshal.Copy(ftrPtr, tmp, 0, 16);
            Marshal.FreeHGlobal(ftrPtr);
            descriptorFile.Write(tmp, 0, tmp.Length);

            descriptorFile.Write(filename, 0, filename.Length);

            // This is because of marshalling strings
            descriptorFile.Position = 15;
            descriptorFile.WriteByte(0x52);

            descriptorFile.Dispose();
        }

        internal void SetMediaType(MediaType type)
        {
            switch(type)
            {
                case MediaType.DVDDownload:
                case MediaType.DVDPR:
                case MediaType.DVDPRDL:
                case MediaType.DVDPRW:
                case MediaType.DVDPRWDL:
                case MediaType.DVDR:
                case MediaType.DVDRAM:
                case MediaType.DVDRDL:
                case MediaType.DVDRW:
                case MediaType.DVDRWDL:
                    header.type = AlcoholMediumType.Dvdr;
                    break;
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
                case MediaType.PCD:
                case MediaType.PS1CD:
                case MediaType.PS2CD:
                case MediaType.SATURNCD:
                case MediaType.SuperCDROM2:
                case MediaType.SVCD:
                case MediaType.VCD:
                case MediaType.VCDHD:
                case MediaType.GDROM:
                case MediaType.ThreeDO:
                    header.type = AlcoholMediumType.Cd;
                    break;
                case MediaType.CDR:
                case MediaType.DDCDR:
                case MediaType.GDR:
                    header.type = AlcoholMediumType.Cdr;
                    break;
                case MediaType.CDRW:
                case MediaType.DDCDRW:
                case MediaType.CDMO:
                case MediaType.CDMRW:
                    header.type = AlcoholMediumType.Cdrw;
                    break;
                default:
                    header.type = AlcoholMediumType.Dvd;
                    break;
            }
        }

        internal void AddSessions(Session[] cdSessions)
        {
            foreach(Session cdSession in cdSessions)
            {
                AlcoholSession session = new AlcoholSession
                {
                    firstTrack = (ushort)cdSession.StartTrack,
                    lastTrack = (ushort)cdSession.EndTrack,
                    sessionSequence = cdSession.SessionSequence
                };
                sessions.Add(session);
            }
        }

        internal void SetTrackTypes(byte point, TrackType mode, TrackSubchannelType subMode)
        {
            AlcoholTrack[] trkArray = tracks.ToArray();

            for(int i = 0; i < trkArray.Length; i++)
            {
                if(trkArray[i].point != point) continue;

                switch(mode)
                {
                    case TrackType.Audio:
                        trkArray[i].mode = AlcoholTrackMode.Audio;
                        break;
                    case TrackType.Data:
                        trkArray[i].mode = AlcoholTrackMode.Dvd;
                        break;
                    case TrackType.CdMode1:
                        trkArray[i].mode = AlcoholTrackMode.Mode1;
                        break;
                    case TrackType.CdMode2Formless:
                        trkArray[i].mode = AlcoholTrackMode.Mode2;
                        break;
                    case TrackType.CdMode2Form1:
                        trkArray[i].mode = AlcoholTrackMode.Mode2F1;
                        break;
                    case TrackType.CdMode2Form2:
                        trkArray[i].mode = AlcoholTrackMode.Mode2F2;
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
                }

                switch(subMode)
                {
                    case TrackSubchannelType.None:
                        trkArray[i].subMode = AlcoholSubchannelMode.None;
                        break;
                    case TrackSubchannelType.RawInterleaved:
                        trkArray[i].subMode = AlcoholSubchannelMode.Interleaved;
                        break;
                    case TrackSubchannelType.Packed:
                    case TrackSubchannelType.Raw:
                    case TrackSubchannelType.PackedInterleaved:
                    case TrackSubchannelType.Q16:
                    case TrackSubchannelType.Q16Interleaved:
                        throw new FeatureUnsupportedImageException("Specified subchannel type is not supported.");
                    default: throw new ArgumentOutOfRangeException(nameof(subMode), subMode, null);
                }

                tracks = new List<AlcoholTrack>(trkArray);
                break;
            }
        }

        internal void SetTrackSizes(byte point, int sectorSize, long startLba, long startOffset, long length)
        {
            AlcoholTrack[] trkArray = tracks.ToArray();

            for(int i = 0; i < trkArray.Length; i++)
            {
                if(trkArray[i].point != point) continue;

                trkArray[i].sectorSize = (ushort)sectorSize;
                trkArray[i].startLba = (uint)startLba;
                trkArray[i].startOffset = (ulong)startOffset;

                tracks = new List<AlcoholTrack>(trkArray);
                break;
            }

            if(trackLengths.ContainsKey(point)) trackLengths.Remove(point);

            trackLengths.Add(point, (uint)length);

            AlcoholSession[] sess = sessions.ToArray();

            for(int i = 0; i < sess.Length; i++)
            {
                if(sess[i].firstTrack == point) sess[i].sessionStart = (int)startLba;
                if(sess[i].lastTrack == point) sess[i].sessionEnd = (int)(startLba + length);
            }

            sessions = new List<AlcoholSession>(sess);
        }

        internal void AddTrack(byte adrCtl, byte tno, byte point, byte min, byte sec, byte frame, byte zero, byte pmin,
                             byte psec, byte pframe, byte session)
        {
            AlcoholTrack trk = new AlcoholTrack
            {
                mode = AlcoholTrackMode.NoData,
                subMode = AlcoholSubchannelMode.None,
                adrCtl = adrCtl,
                tno = tno,
                point = point,
                min = min,
                sec = sec,
                frame = frame,
                zero = zero,
                pmin = pmin,
                psec = psec,
                pframe = pframe,
                unknown = new byte[18],
                files = 1,
                unknown2 = new byte[24]
            };

            tracks.Add(trk);

            if(point < 0xA0) return;

            AlcoholSession[] sess = sessions.ToArray();

            for(int i = 0; i < sess.Length; i++) if(sess[i].sessionSequence == session) sess[i].nonTrackBlocks++;

            sessions = new List<AlcoholSession>(sess);
        }

        internal void AddBca(byte[] bca)
        {
            this.bca = bca;
        }

        internal void AddPfi(byte[] pfi)
        {
            if(pfi.Length == 2052)
            {
                this.pfi = new byte[2048];
                Array.Copy(pfi, 4, this.pfi, 0, 2048);
            }
            else this.pfi = pfi;
        }

        internal void AddDmi(byte[] dmi)
        {
            if(dmi.Length == 2052)
            {
                this.dmi = new byte[2048];
                Array.Copy(dmi, 4, this.dmi, 0, 2048);
            }
            else this.dmi = dmi;
        }

        internal void SetExtension(string extension)
        {
            this.extension = extension;
        }
    }
}