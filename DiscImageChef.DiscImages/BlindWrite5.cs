// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : BlindWrite5.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages BlindWrite 5 disc images.
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Decoders.SCSI.MMC;
using System.Text;
using System.Globalization;
using DiscImageChef.Filters;

namespace DiscImageChef.ImagePlugins
{
    public class BlindWrite5 : ImagePlugin
    {
        #region Internal Constants
        /// <summary>"BWT5 STREAM SIGN"</summary>
        readonly byte[] BW5_Signature = { 0x42, 0x57, 0x54, 0x35, 0x20, 0x53, 0x54, 0x52, 0x45, 0x41, 0x4D, 0x20, 0x53, 0x49, 0x47, 0x4E };
        /// <summary>"BWT5 STREAM FOOT"</summary>
        readonly byte[] BW5_Footer = { 0x42, 0x57, 0x54, 0x35, 0x20, 0x53, 0x54, 0x52, 0x45, 0x41, 0x4D, 0x20, 0x46, 0x4F, 0x4F, 0x54 };
        #endregion Internal Constants

        #region Internal enumerations
        enum BW5_TrackType : byte
        {
            NotData = 0,
            Audio = 1,
            Mode1 = 2,
            Mode2 = 3,
            Mode2F1 = 4,
            Mode2F2 = 5,
            DVD = 6
        }

        enum BW5_TrackSubchannel : byte
        {
            None = 0,
            Q16 = 2,
            Linear = 4,
        }
        #endregion Internal enumerations

        #region Internal Structures
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BW5_Header
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] signature;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public uint[] unknown1;
            public ProfileNumber profile;
            public ushort sessions;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public uint[] unknown2;
            [MarshalAs(UnmanagedType.U1, SizeConst = 3)]
            public bool mcnIsValid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
            public byte[] mcn;
            public ushort unknown3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public uint[] unknown4;
            public ushort pmaLen;
            public ushort atipLen;
            public ushort cdtLen;
            public ushort cdInfoLen;
            public uint bcaLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public uint[] unknown5;
            public uint dvdStrLen;
            public uint dvdInfoLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] unknown6;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] manufacturer;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] product;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] revision;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] vendor;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] volumeId;
            public uint mode2ALen;
            public uint unkBlkLen;
            public uint dataLen;
            public uint sessionsLen;
            public uint dpmLen;
        }

        struct BW5_DataFile
        {
            public uint type;
            public uint length;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public uint[] unknown1;
            public uint offset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public uint[] unknown2;
            public int startLba;
            public int sectors;
            public uint filenameLen;
            public byte[] filenameBytes;
            public uint unknown3;

            public string filename;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BW5_TrackDescriptor
        {
            public BW5_TrackType type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] unknown1;
            public uint unknown2;
            public BW5_TrackSubchannel subchannel;
            public byte unknown3;
            public byte ctl;
            public byte adr;
            public byte point;
            public byte unknown4;
            public byte min;
            public byte sec;
            public byte frame;
            public byte zero;
            public byte pmin;
            public byte psec;
            public byte pframe;
            public byte unknown5;
            public uint pregap;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public uint[] unknown6;
            public int startLba;
            public int sectors;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public uint[] unknown7;
            public uint session;
            public ushort unknown8;
            // Seems to be only on non DVD track descriptors
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public uint[] unknown9;
        }

        struct BW5_SessionDescriptor
        {
            public ushort sequence;
            public byte entries;
            public byte unknown;
            public int start;
            public int end;
            public ushort firstTrack;
            public ushort lastTrack;
            public BW5_TrackDescriptor[] tracks;
        }

        struct DataFileCharacteristics
        {
            public Filter fileFilter;
            public string filePath;
            public TrackSubchannelType subchannel;
            public long sectorSize;
            public int startLba;
            public int sectors;
        }
        #endregion Internal Structures

        #region Internal variables
        BW5_Header header;
        byte[] mode2A;
        byte[] unkBlock;
        byte[] pma;
        byte[] atip;
        byte[] cdtext;
        byte[] bca;
        byte[] dmi;
        byte[] pfi;
        byte[] discInformation;
        string dataPath;
        List<BW5_DataFile> dataFiles;
        List<BW5_SessionDescriptor> bwSessions;
        byte[] dpm;
        List<Session> sessions;
        List<Track> tracks;
        List<Partition> partitions;
        List<DataFileCharacteristics> filePaths;
        byte[] fullToc;
        Dictionary<uint, ulong> offsetmap;
        Dictionary<uint, byte> trackFlags;
        Stream imageStream;
        #endregion Internal variables

        #region Public Methods
        public BlindWrite5()
        {
            Name = "BlindWrite 5";
            PluginUUID = new Guid("9CB7A381-0509-4F9F-B801-3F65434BC3EE");
            ImageInfo = new ImageInfo();
            ImageInfo.readableSectorTags = new List<SectorTagType>();
            ImageInfo.readableMediaTags = new List<MediaTagType>();
            ImageInfo.imageHasPartitions = true;
            ImageInfo.imageHasSessions = true;
            ImageInfo.imageVersion = null;
            ImageInfo.imageApplicationVersion = null;
            ImageInfo.imageName = null;
            ImageInfo.imageCreator = null;
            ImageInfo.mediaManufacturer = null;
            ImageInfo.mediaModel = null;
            ImageInfo.mediaPartNumber = null;
            ImageInfo.mediaSequence = 0;
            ImageInfo.lastMediaSequence = 0;
            ImageInfo.driveManufacturer = null;
            ImageInfo.driveModel = null;
            ImageInfo.driveSerialNumber = null;
            ImageInfo.driveFirmwareRevision = null;
        }

        public override bool IdentifyImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            if(stream.Length < 276)
                return false;

            byte[] signature = new byte[16];
            stream.Read(signature, 0, 16);

            byte[] footer = new byte[16];
            stream.Seek(-16, SeekOrigin.End);
            stream.Read(footer, 0, 16);

            return BW5_Signature.SequenceEqual(signature) && BW5_Footer.SequenceEqual(footer);
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            if(stream.Length < 276)
                return false;

            byte[] hdr = new byte[260];
            stream.Read(hdr, 0, 260);
            header = new BW5_Header();
            IntPtr hdrPtr = Marshal.AllocHGlobal(260);
            Marshal.Copy(hdr, 0, hdrPtr, 260);
            header = (BW5_Header)Marshal.PtrToStructure(hdrPtr, typeof(BW5_Header));
            Marshal.FreeHGlobal(hdrPtr);

            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.signature = {0}", StringHandlers.CToString(header.signature));
            for(int i = 0; i < header.unknown1.Length; i++)
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.unknown1[{1}] = 0x{0:X8}", header.unknown1[i], i);
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.profile = {0}", header.profile);
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.sessions = {0}", header.sessions);
            for(int i = 0; i < header.unknown2.Length; i++)
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.unknown2[{1}] = 0x{0:X8}", header.unknown2[i], i);
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.mcnIsValid = {0}", header.mcnIsValid);
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.mcn = {0}", StringHandlers.CToString(header.mcn));
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.unknown3 = 0x{0:X4}", header.unknown3);
            for(int i = 0; i < header.unknown4.Length; i++)
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.unknown4[{1}] = 0x{0:X8}", header.unknown4[i], i);
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.pmaLen = {0}", header.pmaLen);
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.atipLen = {0}", header.atipLen);
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.cdtLen = {0}", header.cdtLen);
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.cdInfoLen = {0}", header.cdInfoLen);
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.bcaLen = {0}", header.bcaLen);
            for(int i = 0; i < header.unknown5.Length; i++)
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.unknown5[{1}] = 0x{0:X8}", header.unknown5[i], i);
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.dvdStrLen = {0}", header.dvdStrLen);
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.dvdInfoLen = {0}", header.dvdInfoLen);
            for(int i = 0; i < header.unknown6.Length; i++)
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.unknown6[{1}] = 0x{0:X2}", header.unknown6[i], i);
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.manufacturer = {0}", StringHandlers.CToString(header.manufacturer));
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.product = {0}", StringHandlers.CToString(header.product));
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.revision = {0}", StringHandlers.CToString(header.revision));
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.vendor = {0}", StringHandlers.CToString(header.vendor));
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.volumeId = {0}", StringHandlers.CToString(header.volumeId));
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.mode2ALen = {0}", header.mode2ALen);
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.unkBlkLen = {0}", header.unkBlkLen);
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.dataLen = {0}", header.dataLen);
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.sessionsLen = {0}", header.sessionsLen);
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.dpmLen = {0}", header.dpmLen);

            mode2A = new byte[header.mode2ALen];
            if(mode2A.Length > 0)
            {
                stream.Read(mode2A, 0, mode2A.Length);
                mode2A[1] -= 2;
                Decoders.SCSI.Modes.ModePage_2A? decoded2A = Decoders.SCSI.Modes.DecodeModePage_2A(mode2A);
                if(decoded2A.HasValue)
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "mode page 2A: {0}", Decoders.SCSI.Modes.PrettifyModePage_2A(decoded2A));
                else
                    mode2A = null;
            }

            unkBlock = new byte[header.unkBlkLen];
            if(unkBlock.Length > 0)
                stream.Read(unkBlock, 0, unkBlock.Length);

            byte[] temp = new byte[header.pmaLen];
            if(temp.Length > 0)
            {
                byte[] tushort = BitConverter.GetBytes((ushort)(temp.Length + 2));
                stream.Read(temp, 0, temp.Length);
                pma = new byte[temp.Length + 4];
                pma[0] = tushort[1];
                pma[1] = tushort[0];
                Array.Copy(temp, 0, pma, 4, temp.Length);

                Decoders.CD.PMA.CDPMA? decodedPma = Decoders.CD.PMA.Decode(pma);
                if(decodedPma.HasValue)
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "PMA: {0}", Decoders.CD.PMA.Prettify(decodedPma));
                else
                    pma = null;
            }

            temp = new byte[header.atipLen];
            if(temp.Length > 0)
            {
                byte[] tushort = BitConverter.GetBytes((ushort)(temp.Length + 2));
                stream.Read(temp, 0, temp.Length);
                atip = new byte[temp.Length + 4];
                atip[0] = tushort[1];
                atip[1] = tushort[0];
                Array.Copy(temp, 0, atip, 4, temp.Length);

                Decoders.CD.ATIP.CDATIP? decodedAtip = Decoders.CD.ATIP.Decode(atip);
                if(decodedAtip.HasValue)
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "ATIP: {0}", Decoders.CD.ATIP.Prettify(decodedAtip));
                else
                    atip = null;
            }

            temp = new byte[header.cdtLen];
            if(temp.Length > 0)
            {
                byte[] tushort = BitConverter.GetBytes((ushort)(temp.Length + 2));
                stream.Read(temp, 0, temp.Length);
                cdtext = new byte[temp.Length + 4];
                cdtext[0] = tushort[1];
                cdtext[1] = tushort[0];
                Array.Copy(temp, 0, cdtext, 4, temp.Length);

                Decoders.CD.CDTextOnLeadIn.CDText? decodedCdText = Decoders.CD.CDTextOnLeadIn.Decode(cdtext);
                if(decodedCdText.HasValue)
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "CD-Text: {0}", Decoders.CD.CDTextOnLeadIn.Prettify(decodedCdText));
                else
                    cdtext = null;
            }

            bca = new byte[header.bcaLen];
            if(bca.Length > 0)
                stream.Read(bca, 0, bca.Length);
            else
                bca = null;

            temp = new byte[header.dvdStrLen];
            if(temp.Length > 0)
            {
                stream.Read(temp, 0, temp.Length);
                dmi = new byte[2052];
                pfi = new byte[2052];

                Array.Copy(temp, 0, dmi, 0, 2050);
                Array.Copy(temp, 0x802, pfi, 4, 2048);

                pfi[0] = 0x08;
                pfi[1] = 0x02;
                dmi[0] = 0x08;
                dmi[1] = 0x02;

                Decoders.DVD.PFI.PhysicalFormatInformation? decodedPfi = Decoders.DVD.PFI.Decode(pfi);
                if(decodedPfi.HasValue)
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "PFI: {0}", Decoders.DVD.PFI.Prettify(decodedPfi));
                else
                {
                    pfi = null;
                    dmi = null;
                }
            }

            switch(header.profile)
            {
                case ProfileNumber.CDR:
                case ProfileNumber.CDROM:
                case ProfileNumber.CDRW:
                case ProfileNumber.DDCDR:
                case ProfileNumber.DDCDROM:
                case ProfileNumber.DDCDRW:
                case ProfileNumber.HDBURNROM:
                case ProfileNumber.HDBURNR:
                case ProfileNumber.HDBURNRW:
                    discInformation = new byte[header.cdInfoLen];
                    break;
                default:
                    discInformation = new byte[header.dvdInfoLen];
                    break;
            }

            if(discInformation.Length > 0)
            {
                stream.Read(discInformation, 0, discInformation.Length);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "Disc information: {0}", PrintHex.ByteArrayToHexArrayString(discInformation, 40));
            }
            else
                discInformation = null;

            // How many data blocks
            byte[] tmpArray = new byte[4];
            stream.Read(tmpArray, 0, tmpArray.Length);
            uint dataBlockCount = BitConverter.ToUInt32(tmpArray, 0);

            stream.Read(tmpArray, 0, tmpArray.Length);
            uint dataPathLen = BitConverter.ToUInt32(tmpArray, 0);
            byte[] dataPathBytes = new byte[dataPathLen];
            stream.Read(dataPathBytes, 0, dataPathBytes.Length);
            dataPath = Encoding.Unicode.GetString(dataPathBytes);
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "Data path: {0}", dataPath);

            dataFiles = new List<BW5_DataFile>();
            for(int cD = 0; cD < dataBlockCount; cD++)
            {
                tmpArray = new byte[52];
                BW5_DataFile dataFile = new BW5_DataFile();
                dataFile.unknown1 = new uint[4];
                dataFile.unknown2 = new uint[3];

                stream.Read(tmpArray, 0, tmpArray.Length);
                dataFile.type = BitConverter.ToUInt32(tmpArray, 0);
                dataFile.length = BitConverter.ToUInt32(tmpArray, 4);
                dataFile.unknown1[0] = BitConverter.ToUInt32(tmpArray, 8);
                dataFile.unknown1[1] = BitConverter.ToUInt32(tmpArray, 12);
                dataFile.unknown1[2] = BitConverter.ToUInt32(tmpArray, 16);
                dataFile.unknown1[3] = BitConverter.ToUInt32(tmpArray, 20);
                dataFile.offset = BitConverter.ToUInt32(tmpArray, 24);
                dataFile.unknown2[0] = BitConverter.ToUInt32(tmpArray, 28);
                dataFile.unknown2[1] = BitConverter.ToUInt32(tmpArray, 32);
                dataFile.unknown2[2] = BitConverter.ToUInt32(tmpArray, 36);
                dataFile.startLba = BitConverter.ToInt32(tmpArray, 40);
                dataFile.sectors = BitConverter.ToInt32(tmpArray, 44);
                dataFile.filenameLen = BitConverter.ToUInt32(tmpArray, 48);
                dataFile.filenameBytes = new byte[dataFile.filenameLen];

                tmpArray = new byte[dataFile.filenameLen];
                stream.Read(tmpArray, 0, tmpArray.Length);
                dataFile.filenameBytes = tmpArray;
                tmpArray = new byte[4];
                stream.Read(tmpArray, 0, tmpArray.Length);
                dataFile.unknown3 = BitConverter.ToUInt32(tmpArray, 0);

                dataFile.filename = Encoding.Unicode.GetString(dataFile.filenameBytes);
                dataFiles.Add(dataFile);

                DicConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.type = 0x{0:X8}", dataFile.type);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.length = {0}", dataFile.length);
                for(int i = 0; i < dataFile.unknown1.Length; i++)
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.unknown1[{1}] = {0}", dataFile.unknown1[i], i);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.offset = {0}", dataFile.offset);
                for(int i = 0; i < dataFile.unknown2.Length; i++)
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.unknown2[{1}] = {0}", dataFile.unknown2[i], i);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.startLba = {0}", dataFile.startLba);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.sectors = {0}", dataFile.sectors);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.filenameLen = {0}", dataFile.filenameLen);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.filename = {0}", dataFile.filename);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.unknown3 = {0}", dataFile.unknown3);
            }

            bwSessions = new List<BW5_SessionDescriptor>();
            for(int ses = 0; ses < header.sessions; ses++)
            {
                BW5_SessionDescriptor session = new BW5_SessionDescriptor();
                tmpArray = new byte[16];
                stream.Read(tmpArray, 0, tmpArray.Length);
                session.sequence = BitConverter.ToUInt16(tmpArray, 0);
                session.entries = tmpArray[2];
                session.unknown = tmpArray[3];
                session.start = BitConverter.ToInt32(tmpArray, 4);
                session.end = BitConverter.ToInt32(tmpArray, 8);
                session.firstTrack = BitConverter.ToUInt16(tmpArray, 12);
                session.lastTrack = BitConverter.ToUInt16(tmpArray, 14);
                session.tracks = new BW5_TrackDescriptor[session.entries];

                DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].filename = {1}", ses, session.sequence);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].entries = {1}", ses, session.entries);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].unknown = {1}", ses, session.unknown);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].start = {1}", ses, session.start);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].end = {1}", ses, session.end);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].firstTrack = {1}", ses, session.firstTrack);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].lastTrack = {1}", ses, session.lastTrack);

                for(int tSeq = 0; tSeq < session.entries; tSeq++)
                {
                    byte[] trk = new byte[72];
                    stream.Read(trk, 0, 72);
                    session.tracks[tSeq] = new BW5_TrackDescriptor();
                    IntPtr trkPtr = Marshal.AllocHGlobal(72);
                    Marshal.Copy(trk, 0, trkPtr, 72);
                    session.tracks[tSeq] = (BW5_TrackDescriptor)Marshal.PtrToStructure(trkPtr, typeof(BW5_TrackDescriptor));
                    Marshal.FreeHGlobal(trkPtr);

                    if(session.tracks[tSeq].type == BW5_TrackType.DVD ||
                       session.tracks[tSeq].type == BW5_TrackType.NotData)
                    {
                        session.tracks[tSeq].unknown9[0] = 0;
                        session.tracks[tSeq].unknown9[1] = 0;
                        stream.Seek(-8, SeekOrigin.Current);
                    }

                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].type = {2}", ses, tSeq, session.tracks[tSeq].type);
                    for(int i = 0; i < session.tracks[tSeq].unknown1.Length; i++)
                        DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].unknown1[{2}] = 0x{3:X2}", ses, tSeq, i, session.tracks[tSeq].unknown1[i]);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].unknown2 = 0x{2:X8}", ses, tSeq, session.tracks[tSeq].unknown2);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].subchannel = {2}", ses, tSeq, session.tracks[tSeq].subchannel);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].unknown3 = 0x{2:X2}", ses, tSeq, session.tracks[tSeq].unknown3);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].ctl = {2}", ses, tSeq, session.tracks[tSeq].ctl);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].adr = {2}", ses, tSeq, session.tracks[tSeq].adr);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].point = {2}", ses, tSeq, session.tracks[tSeq].point);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].unknown4 = 0x{2:X2}", ses, tSeq, session.tracks[tSeq].unknown4);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].min = {2}", ses, tSeq, session.tracks[tSeq].min);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].sec = {2}", ses, tSeq, session.tracks[tSeq].sec);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].frame = {2}", ses, tSeq, session.tracks[tSeq].frame);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].zero = {2}", ses, tSeq, session.tracks[tSeq].zero);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].pmin = {2}", ses, tSeq, session.tracks[tSeq].pmin);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].psec = {2}", ses, tSeq, session.tracks[tSeq].psec);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].pframe = {2}", ses, tSeq, session.tracks[tSeq].pframe);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].unknown5 = 0x{2:X2}", ses, tSeq, session.tracks[tSeq].unknown5);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].pregap = {2}", ses, tSeq, session.tracks[tSeq].pregap);
                    for(int i = 0; i < session.tracks[tSeq].unknown6.Length; i++)
                        DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].unknown6[{2}] = 0x{3:X8}", ses, tSeq, i, session.tracks[tSeq].unknown6[i]);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].startLba = {2}", ses, tSeq, session.tracks[tSeq].startLba);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].sectors = {2}", ses, tSeq, session.tracks[tSeq].sectors);
                    for(int i = 0; i < session.tracks[tSeq].unknown7.Length; i++)
                        DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].unknown7[{2}] = 0x{3:X8}", ses, tSeq, i, session.tracks[tSeq].unknown7[i]);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].session = {2}", ses, tSeq, session.tracks[tSeq].session);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].unknown8 = 0x{2:X4}", ses, tSeq, session.tracks[tSeq].unknown8);
                    if(session.tracks[tSeq].type != BW5_TrackType.DVD &&
                       session.tracks[tSeq].type != BW5_TrackType.NotData)
                    {
                        for(int i = 0; i < session.tracks[tSeq].unknown9.Length; i++)
                            DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].unknown9[{2}] = 0x{3:X8}", ses, tSeq, i, session.tracks[tSeq].unknown9[i]);
                    }
                }

                bwSessions.Add(session);
            }

            dpm = new byte[header.dpmLen];
            stream.Read(dpm, 0, dpm.Length);

            // Unused
            tmpArray = new byte[4];
            stream.Read(tmpArray, 0, tmpArray.Length);

            byte[] footer = new byte[16];
            stream.Read(footer, 0, footer.Length);

            if(BW5_Footer.SequenceEqual(footer))
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "Correctly arrived end of image");
            else
                DicConsole.ErrorWriteLine("BlindWrite5 image ends after expected position. Probably new version with different data. Errors may occur.");

            FiltersList filtersList = new FiltersList();

            filePaths = new List<DataFileCharacteristics>();
            foreach(BW5_DataFile dataFile in dataFiles)
            {
                DataFileCharacteristics chars = new DataFileCharacteristics();
                string path = Path.Combine(dataPath, dataFile.filename);

                if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path)) != null)
                {
                    chars.fileFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path));
                    chars.filePath = path;
                }
                else
                {
                    path = Path.Combine(dataPath, dataFile.filename.ToLower(CultureInfo.CurrentCulture));
                    if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path)) != null)
                    {
                        chars.fileFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path));
                        chars.filePath = path;
                    }
                    else
                    {
                        path = Path.Combine(dataPath, dataFile.filename.ToUpper(CultureInfo.CurrentCulture));
                        if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path)) != null)
                        {
                            chars.fileFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path));
                            chars.filePath = path;
                        }
                        else
                        {
                            path = Path.Combine(dataPath.ToLower(CultureInfo.CurrentCulture), dataFile.filename);
                            if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path)) != null)
                            {
                                chars.fileFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path));
                                chars.filePath = path;
                            }
                            else
                            {
                                path = Path.Combine(dataPath.ToUpper(CultureInfo.CurrentCulture), dataFile.filename);
                                if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path)) != null)
                                {
                                    chars.fileFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path));
                                    chars.filePath = path;
                                }
                                else
                                {
                                    path = Path.Combine(dataPath, dataFile.filename);
                                    if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path.ToLower(CultureInfo.CurrentCulture))) != null)
                                    {
                                        chars.filePath = path.ToLower(CultureInfo.CurrentCulture);
                                        chars.fileFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path.ToLower(CultureInfo.CurrentCulture)));
                                    }
                                    else if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path.ToUpper(CultureInfo.CurrentCulture))) != null)
                                    {
                                        chars.filePath = path.ToUpper(CultureInfo.CurrentCulture);
                                        chars.fileFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path.ToUpper(CultureInfo.CurrentCulture)));
                                    }
                                    else if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), dataFile.filename.ToLower(CultureInfo.CurrentCulture))) != null)
                                    {
                                        chars.filePath = dataFile.filename.ToLower(CultureInfo.CurrentCulture);
                                        chars.fileFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), dataFile.filename.ToLower(CultureInfo.CurrentCulture)));
                                    }
                                    else if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), dataFile.filename.ToUpper(CultureInfo.CurrentCulture))) != null)
                                    {
                                        chars.filePath = dataFile.filename.ToUpper(CultureInfo.CurrentCulture);
                                        chars.fileFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), dataFile.filename.ToUpper(CultureInfo.CurrentCulture)));
                                    }
                                    else if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), dataFile.filename)) != null)
                                    {
                                        chars.filePath = dataFile.filename;
                                        chars.fileFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), dataFile.filename));
                                    }
                                    else
                                    {
                                        DicConsole.ErrorWriteLine("Cannot find data file {0}", dataFile.filename);
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }

                long sectorSize = dataFile.length / dataFile.sectors;
                if(sectorSize > 2352)
                {
                    if((sectorSize - 2352) == 16)
                        chars.subchannel = TrackSubchannelType.Q16Interleaved;
                    else if((sectorSize - 2352) == 96)
                        chars.subchannel = TrackSubchannelType.PackedInterleaved;
                    else
                    {
                        DicConsole.ErrorWriteLine("BlindWrite5 found unknown subchannel size: {0}", sectorSize - 2352);
                        return false;
                    }
                }
                else
                    chars.subchannel = TrackSubchannelType.None;
                chars.sectorSize = sectorSize;
                chars.startLba = dataFile.startLba;
                chars.sectors = dataFile.sectors;

                filePaths.Add(chars);
            }

            sessions = new List<Session>();
            tracks = new List<Track>();
            partitions = new List<Partition>();
            MemoryStream fullTocStream = new MemoryStream();
            fullTocStream.Write(new byte[] { 0, 0, 0, 0 }, 0, 4);
            ulong offsetBytes = 0;
            offsetmap = new Dictionary<uint, ulong>();
            bool isDvd = false;
            byte firstSession = byte.MaxValue;
            byte lastSession = 0;
            trackFlags = new Dictionary<uint, byte>();
            ImageInfo.sectors = 0;

            DicConsole.DebugWriteLine("BlindWrite5 plugin", "Building maps");
            foreach(BW5_SessionDescriptor ses in bwSessions)
            {
                Session session = new Session();
                session.SessionSequence = ses.sequence;
                if(ses.start < 0)
                    session.StartSector = 0;
                else
                    session.StartSector = (ulong)ses.start;
                session.EndSector = (ulong)ses.end;
                session.StartTrack = ses.firstTrack;
                session.EndTrack = ses.lastTrack;

                if(ses.sequence < firstSession)
                    firstSession = (byte)ses.sequence;
                if(ses.sequence > lastSession)
                    lastSession = (byte)ses.sequence;

                foreach(BW5_TrackDescriptor trk in ses.tracks)
                {
                    byte adrCtl = (byte)((trk.adr << 4) + trk.ctl);
                    fullTocStream.WriteByte((byte)trk.session);
                    fullTocStream.WriteByte(adrCtl);
                    fullTocStream.WriteByte(0x00);
                    fullTocStream.WriteByte(trk.point);
                    fullTocStream.WriteByte(trk.min);
                    fullTocStream.WriteByte(trk.sec);
                    fullTocStream.WriteByte(trk.frame);
                    fullTocStream.WriteByte(trk.zero);
                    fullTocStream.WriteByte(trk.pmin);
                    fullTocStream.WriteByte(trk.psec);
                    fullTocStream.WriteByte(trk.pframe);

                    if(trk.point < 0xA0)
                    {
                        Track track = new Track();
                        Partition partition = new Partition();

                        trackFlags.Add(trk.point, trk.ctl);

                        switch(trk.type)
                        {
                            case BW5_TrackType.Audio:
                                track.TrackBytesPerSector = 2352;
                                track.TrackRawBytesPerSector = 2352;
                                if(ImageInfo.sectorSize < 2352)
                                    ImageInfo.sectorSize = 2352;
                                break;
                            case BW5_TrackType.Mode1:
                            case BW5_TrackType.Mode2F1:
                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSync))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSync);
                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorHeader))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorHeader);
                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSubHeader))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSubHeader);
                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorECC))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorECC);
                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorECC_P))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorECC_P);
                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorECC_Q))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorECC_Q);
                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorEDC))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorEDC);
                                track.TrackBytesPerSector = 2048;
                                track.TrackRawBytesPerSector = 2352;
                                if(ImageInfo.sectorSize < 2048)
                                    ImageInfo.sectorSize = 2048;
                                break;
                            case BW5_TrackType.Mode2:
                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSync))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSync);
                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorHeader))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorHeader);
                                track.TrackBytesPerSector = 2336;
                                track.TrackRawBytesPerSector = 2352;
                                if(ImageInfo.sectorSize < 2336)
                                    ImageInfo.sectorSize = 2336;
                                break;
                            case BW5_TrackType.Mode2F2:
                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSync))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSync);
                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorHeader))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorHeader);
                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSubHeader))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSubHeader);
                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorEDC))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorEDC);
                                track.TrackBytesPerSector = 2336;
                                track.TrackRawBytesPerSector = 2352;
                                if(ImageInfo.sectorSize < 2324)
                                    ImageInfo.sectorSize = 2324;
                                break;
                            case BW5_TrackType.DVD:
                                track.TrackBytesPerSector = 2048;
                                track.TrackRawBytesPerSector = 2048;
                                if(ImageInfo.sectorSize < 2048)
                                    ImageInfo.sectorSize = 2048;
                                isDvd = true;
                                break;
                        }

                        track.TrackDescription = string.Format("Track {0}", trk.point);
                        track.TrackStartSector = (ulong)(trk.startLba + trk.pregap);
                        track.TrackEndSector = (ulong)(trk.sectors + trk.startLba);

                        foreach(DataFileCharacteristics chars in filePaths)
                        {
                            if(trk.startLba >= chars.startLba && (trk.startLba + trk.sectors) <= (chars.startLba + chars.sectors))
                            {
                                track.TrackFilter = chars.fileFilter;
                                track.TrackFile = chars.fileFilter.GetFilename();
                                if(trk.startLba >= 0)
                                   track.TrackFileOffset = (ulong)((trk.startLba - chars.startLba) * chars.sectorSize);
                                else
                                    track.TrackFileOffset = (ulong)((trk.startLba * -1) * chars.sectorSize);
                                track.TrackFileType = "BINARY";
                                if(chars.subchannel != TrackSubchannelType.None)
                                {
                                    track.TrackSubchannelFilter = track.TrackFilter;
                                    track.TrackSubchannelFile = track.TrackFile;
                                    track.TrackSubchannelType = chars.subchannel;
                                    track.TrackSubchannelOffset = track.TrackFileOffset;

                                    if(chars.subchannel == TrackSubchannelType.PackedInterleaved)
                                    {
                                        if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSubchannel))
                                            ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSubchannel);

                                    }
                                }

                                break;
                            }
                        }

                        track.TrackPregap = trk.pregap;
                        track.TrackSequence = trk.point;
                        track.TrackType = BlindWriteTrackTypeToTrackType(trk.type);
                        track.Indexes = new Dictionary<int, ulong>();
                        track.Indexes.Add(1, track.TrackStartSector);

                        partition.PartitionDescription = track.TrackDescription;
                        partition.PartitionLength = (track.TrackEndSector - track.TrackStartSector) * (ulong)track.TrackRawBytesPerSector;
                        partition.PartitionSectors = (track.TrackEndSector - track.TrackStartSector);
                        partition.PartitionSequence = track.TrackSequence;
                        partition.PartitionStart = offsetBytes;
                        partition.PartitionStartSector = track.TrackStartSector;
                        partition.PartitionType = track.TrackType.ToString();

                        offsetBytes += partition.PartitionLength;

                        tracks.Add(track);
                        partitions.Add(partition);
                        offsetmap.Add(track.TrackSequence, track.TrackStartSector);
                        ImageInfo.sectors += partition.PartitionSectors;
                    }
                }
            }

            DicConsole.DebugWriteLine("BlindWrite5 plugin", "printing track map");
            foreach(Track track in tracks)
            {
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "Partition sequence: {0}", track.TrackSequence);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition description: {0}", track.TrackDescription);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition type: {0}", track.TrackType);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition starting sector: {0}", track.TrackStartSector);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition ending sector: {0}", track.TrackEndSector);
            }

            DicConsole.DebugWriteLine("BlindWrite5 plugin", "printing partition map");
            foreach(Partition partition in partitions)
            {
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "Partition sequence: {0}", partition.PartitionSequence);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition name: {0}", partition.PartitionName);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition description: {0}", partition.PartitionDescription);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition type: {0}", partition.PartitionType);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition starting sector: {0}", partition.PartitionStartSector);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition sectors: {0}", partition.PartitionSectors);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition starting offset: {0}", partition.PartitionStart);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition size in bytes: {0}", partition.PartitionLength);
            }

            if(!isDvd)
            {
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "Rebuilding TOC");

                fullToc = fullTocStream.ToArray();
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "TOC len {0}", fullToc.Length);

                byte[] fullTocSize = BitConverter.GetBytes((short)(fullToc.Length - 2));
                fullToc[0] = fullTocSize[1];
                fullToc[1] = fullTocSize[0];
                fullToc[2] = firstSession;
                fullToc[3] = lastSession;

                Decoders.CD.FullTOC.CDFullTOC? decodedFullToc = Decoders.CD.FullTOC.Decode(fullToc);

                if(!decodedFullToc.HasValue)
                {
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "TOC not correctly rebuilt");
                    fullToc = null;
                }
                else
                {
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "TOC correctly rebuilt");
                }

                ImageInfo.readableSectorTags.Add(SectorTagType.CDTrackFlags);
            }

            ImageInfo.mediaType = BlindWriteProfileToMediaType(header.profile);

            if(dmi != null && pfi != null)
            {

                Decoders.DVD.PFI.PhysicalFormatInformation? pfi0 = Decoders.DVD.PFI.Decode(pfi);

                // All discs I tested the disk category and part version (as well as the start PSN for DVD-RAM) where modified by Alcohol
                // So much for archival value
                if(pfi0.HasValue)
                {
                    switch(pfi0.Value.DiskCategory)
                    {
                        case Decoders.DVD.DiskCategory.DVDPR:
                            ImageInfo.mediaType = MediaType.DVDPR;
                            break;
                        case Decoders.DVD.DiskCategory.DVDPRDL:
                            ImageInfo.mediaType = MediaType.DVDPRDL;
                            break;
                        case Decoders.DVD.DiskCategory.DVDPRW:
                            ImageInfo.mediaType = MediaType.DVDPRW;
                            break;
                        case Decoders.DVD.DiskCategory.DVDPRWDL:
                            ImageInfo.mediaType = MediaType.DVDPRWDL;
                            break;
                        case Decoders.DVD.DiskCategory.DVDR:
                            if(pfi0.Value.PartVersion == 6)
                                ImageInfo.mediaType = MediaType.DVDRDL;
                            else
                                ImageInfo.mediaType = MediaType.DVDR;
                            break;
                        case Decoders.DVD.DiskCategory.DVDRAM:
                            ImageInfo.mediaType = MediaType.DVDRAM;
                            break;
                        default:
                            ImageInfo.mediaType = MediaType.DVDROM;
                            break;
                        case Decoders.DVD.DiskCategory.DVDRW:
                            if(pfi0.Value.PartVersion == 3)
                                ImageInfo.mediaType = MediaType.DVDRWDL;
                            else
                                ImageInfo.mediaType = MediaType.DVDRW;
                            break;
                        case Decoders.DVD.DiskCategory.HDDVDR:
                            ImageInfo.mediaType = MediaType.HDDVDR;
                            break;
                        case Decoders.DVD.DiskCategory.HDDVDRAM:
                            ImageInfo.mediaType = MediaType.HDDVDRAM;
                            break;
                        case Decoders.DVD.DiskCategory.HDDVDROM:
                            ImageInfo.mediaType = MediaType.HDDVDROM;
                            break;
                        case Decoders.DVD.DiskCategory.HDDVDRW:
                            ImageInfo.mediaType = MediaType.HDDVDRW;
                            break;
                        case Decoders.DVD.DiskCategory.Nintendo:
                            if(pfi0.Value.DiscSize == Decoders.DVD.DVDSize.Eighty)
                                ImageInfo.mediaType = MediaType.GOD;
                            else
                                ImageInfo.mediaType = MediaType.WOD;
                            break;
                        case Decoders.DVD.DiskCategory.UMD:
                            ImageInfo.mediaType = MediaType.UMD;
                            break;
                    }

					if(Decoders.Xbox.DMI.IsXbox(dmi))
						ImageInfo.mediaType = MediaType.XGD;
					else if(Decoders.Xbox.DMI.IsXbox360(dmi))
                        ImageInfo.mediaType = MediaType.XGD2;
                }
            }
            else if(ImageInfo.mediaType == MediaType.CD || ImageInfo.mediaType == MediaType.CDROM)
            {
                bool data = false;
                bool mode2 = false;
                bool firstaudio = false;
                bool firstdata = false;
                bool audio = false;

                foreach(Track _track in tracks)
                {
                    // First track is audio
                    firstaudio |= _track.TrackSequence == 1 && _track.TrackType == TrackType.Audio;

                    // First track is data
                    firstdata |= _track.TrackSequence == 1 && _track.TrackType != TrackType.Audio;

                    // Any non first track is data
                    data |= _track.TrackSequence != 1 && _track.TrackType != TrackType.Audio;

                    // Any non first track is audio
                    audio |= _track.TrackSequence != 1 && _track.TrackType == TrackType.Audio;

                    switch(_track.TrackType)
                    {
                        case TrackType.CDMode2Formless:
                        case TrackType.CDMode2Form1:
                        case TrackType.CDMode2Form2:
                            mode2 = true;
                            break;
                    }
                }

                if(!data && !firstdata)
                    ImageInfo.mediaType = MediaType.CDDA;
                else if(firstaudio && data && sessions.Count > 1 && mode2)
                    ImageInfo.mediaType = MediaType.CDPLUS;
                else if((firstdata && audio) || mode2)
                    ImageInfo.mediaType = MediaType.CDROMXA;
                else if(!audio)
                    ImageInfo.mediaType = MediaType.CDROM;
                else
                    ImageInfo.mediaType = MediaType.CD;
            }

            ImageInfo.driveManufacturer = StringHandlers.CToString(header.manufacturer);
            ImageInfo.driveModel = StringHandlers.CToString(header.product);
            ImageInfo.driveFirmwareRevision = StringHandlers.CToString(header.revision);
            ImageInfo.imageApplication = "BlindWrite";
            if(string.Compare(Path.GetExtension(imageFilter.GetFilename()), "B5T", StringComparison.OrdinalIgnoreCase) == 0)
                ImageInfo.imageApplicationVersion = "5";
            else if(string.Compare(Path.GetExtension(imageFilter.GetFilename()), "B6T", StringComparison.OrdinalIgnoreCase) == 0)
                ImageInfo.imageApplicationVersion = "6";
            ImageInfo.imageVersion = "5";

            ImageInfo.imageSize = (ulong)imageFilter.GetDataForkLength();
            ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.xmlMediaType = XmlMediaType.OpticalDisc;

            if(pma != null)
            {
                Decoders.CD.PMA.CDPMA pma0 = Decoders.CD.PMA.Decode(pma).Value;

                foreach(Decoders.CD.PMA.CDPMADescriptors descriptor in pma0.PMADescriptors)
                {
                    if(descriptor.ADR == 2)
                    {
                        uint id = (uint)((descriptor.Min << 16) + (descriptor.Sec << 8) + descriptor.Frame);
                        ImageInfo.mediaSerialNumber = string.Format("{0:X6}", id & 0x00FFFFFF);
                    }
                }
            }

            if(atip != null)
            {
                Decoders.CD.ATIP.CDATIP atip0 = Decoders.CD.ATIP.Decode(atip).Value;

                if(atip0.DiscType)
                    ImageInfo.mediaType = MediaType.CDRW;
                else
                    ImageInfo.mediaType = MediaType.CDR;

                if(atip0.LeadInStartMin == 97)
                {
                    int type = atip0.LeadInStartFrame % 10;
                    int frm = atip0.LeadInStartFrame - type;
                    ImageInfo.mediaManufacturer = Decoders.CD.ATIP.ManufacturerFromATIP(atip0.LeadInStartSec, frm);
                }
            }

            bool isBD = false;
            if(ImageInfo.mediaType == MediaType.BDR ||
               ImageInfo.mediaType == MediaType.BDRE ||
               ImageInfo.mediaType == MediaType.BDROM)
            {
                isDvd = false;
                isBD = true;
            }

            if(isBD && ImageInfo.sectors > 24438784)
            {
                if(ImageInfo.mediaType == MediaType.BDR)
                    ImageInfo.mediaType = MediaType.BDRXL;
                if(ImageInfo.mediaType == MediaType.BDRE)
                    ImageInfo.mediaType = MediaType.BDREXL;
            }

            DicConsole.DebugWriteLine("BlindWrite5 plugin", "ImageInfo.mediaType = {0}", ImageInfo.mediaType);

            if(mode2A != null)
                ImageInfo.readableMediaTags.Add(MediaTagType.SCSI_MODEPAGE_2A);
            if(pma != null)
                ImageInfo.readableMediaTags.Add(MediaTagType.CD_PMA);
            if(atip != null)
                ImageInfo.readableMediaTags.Add(MediaTagType.CD_ATIP);
            if(cdtext != null)
                ImageInfo.readableMediaTags.Add(MediaTagType.CD_TEXT);
            if(bca != null)
            {
                if(isDvd)
                    ImageInfo.readableMediaTags.Add(MediaTagType.DVD_BCA);
                else if(isBD)
                    ImageInfo.readableMediaTags.Add(MediaTagType.BD_BCA);
            }
            if(dmi != null)
                ImageInfo.readableMediaTags.Add(MediaTagType.DVD_DMI);
            if(pfi != null)
                ImageInfo.readableMediaTags.Add(MediaTagType.DVD_PFI);
            if(fullToc != null)
                ImageInfo.readableMediaTags.Add(MediaTagType.CD_FullTOC);

			if(ImageInfo.mediaType == MediaType.XGD2)
			{
				// All XGD3 all have the same number of blocks
				if(ImageInfo.sectors == 25063 || // Locked (or non compatible drive)
				   ImageInfo.sectors == 4229664 || // Xtreme unlock
				   ImageInfo.sectors == 4246304) // Wxripper unlock
					ImageInfo.mediaType = MediaType.XGD3;
			}

            DicConsole.VerboseWriteLine("BlindWrite image describes a disc of type {0}", ImageInfo.mediaType);

            return true;
        }

        public override bool ImageHasPartitions()
        {
            return ImageInfo.imageHasPartitions;
        }

        public override ulong GetImageSize()
        {
            return ImageInfo.imageSize;
        }

        public override ulong GetSectors()
        {
            return ImageInfo.sectors;
        }

        public override uint GetSectorSize()
        {
            return ImageInfo.sectorSize;
        }

        public override byte[] ReadDiskTag(MediaTagType tag)
        {
            switch(tag)
            {
                case MediaTagType.SCSI_MODEPAGE_2A:
                    {
                        if(mode2A != null)
                        {
                            return (byte[])mode2A.Clone();
                        }
                        throw new FeatureNotPresentImageException("Image does not contain SCSI MODE PAGE 2Ah.");
                    }
                case MediaTagType.CD_PMA:
                    {
                        if(pma != null)
                        {
                            return (byte[])pma.Clone();
                        }
                        throw new FeatureNotPresentImageException("Image does not contain PMA information.");
                    }
                case MediaTagType.CD_ATIP:
                    {
                        if(atip != null)
                        {
                            return (byte[])atip.Clone();
                        }
                        throw new FeatureNotPresentImageException("Image does not contain ATIP information.");
                    }
                case MediaTagType.CD_TEXT:
                    {
                        if(cdtext != null)
                        {
                            return (byte[])cdtext.Clone();
                        }
                        throw new FeatureNotPresentImageException("Image does not contain CD-Text information.");
                    }
                case MediaTagType.DVD_BCA:
                case MediaTagType.BD_BCA:
                    {
                        if(bca != null)
                        {
                            return (byte[])bca.Clone();
                        }
                        throw new FeatureNotPresentImageException("Image does not contain BCA information.");
                    }
                case MediaTagType.DVD_PFI:
                    {
                        if(pfi != null)
                        {
                            return (byte[])pfi.Clone();
                        }
                        throw new FeatureNotPresentImageException("Image does not contain PFI.");
                    }
                case MediaTagType.DVD_DMI:
                    {
                        if(dmi != null)
                        {
                            return (byte[])dmi.Clone();
                        }
                        throw new FeatureNotPresentImageException("Image does not contain DMI.");
                    }
                case MediaTagType.CD_FullTOC:
                    {
                        if(fullToc != null)
                        {
                            return (byte[])fullToc.Clone();
                        }
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
                    foreach(Track track in tracks)
                    {
                        if(track.TrackSequence == kvp.Key)
                        {
                            if((sectorAddress - kvp.Value) < (track.TrackEndSector - track.TrackStartSector))
                                return ReadSectors((sectorAddress - kvp.Value), length, kvp.Key);
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
                    foreach(Track track in tracks)
                    {
                        if(track.TrackSequence == kvp.Key)
                        {
                            if((sectorAddress - kvp.Value) < (track.TrackEndSector - track.TrackStartSector))
                                return ReadSectorsTag((sectorAddress - kvp.Value), length, kvp.Key, tag);
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            // TODO: Cross data files
            Track _track = new Track();
            DataFileCharacteristics chars = new DataFileCharacteristics();

            _track.TrackSequence = 0;

            foreach(Track bwTrack in tracks)
            {
                if(bwTrack.TrackSequence == track)
                {
                    _track = bwTrack;
                    break;
                }
            }

            if(_track.TrackSequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > (_track.TrackEndSector))
                throw new ArgumentOutOfRangeException(nameof(length), string.Format("Requested more sectors ({0}) than present in track ({1}), won't cross tracks", length + sectorAddress, _track.TrackEndSector));

            foreach(DataFileCharacteristics _chars in filePaths)
            {
                if((long)sectorAddress >= _chars.startLba && length < ((ulong)_chars.sectors - sectorAddress))
                {
                    chars = _chars;
                    break;
                }
            }

            if(string.IsNullOrEmpty(chars.filePath) || chars.fileFilter == null)
                throw new ArgumentOutOfRangeException(nameof(chars.fileFilter), "Track does not exist in disc image");

            uint sector_offset;
            uint sector_size;
            uint sector_skip;

            switch(_track.TrackType)
            {
                case TrackType.CDMode1:
                    {
                        sector_offset = 16;
                        sector_size = 2048;
                        sector_skip = 288;
                        break;
                    }
                case TrackType.CDMode2Formless:
                    {
                        sector_offset = 16;
                        sector_size = 2336;
                        sector_skip = 0;
                        break;
                    }
                case TrackType.CDMode2Form1:
                    {
                        sector_offset = 24;
                        sector_size = 2048;
                        sector_skip = 280;
                        break;
                    }
                case TrackType.CDMode2Form2:
                    {
                        sector_offset = 24;
                        sector_size = 2324;
                        sector_skip = 4;
                        break;
                    }
                case TrackType.Audio:
                    {
                        sector_offset = 0;
                        sector_size = 2352;
                        sector_skip = 0;
                        break;
                    }
                case TrackType.Data:
                    {
                        sector_offset = 0;
                        sector_size = 2048;
                        sector_skip = 0;
                        break;
                    }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            switch(chars.subchannel)
            {
                case TrackSubchannelType.None:
                    sector_skip += 0;
                    break;
                case TrackSubchannelType.Q16Interleaved:
                    sector_skip += 16;
                    break;
                case TrackSubchannelType.PackedInterleaved:
                    sector_skip += 96;
                    break;
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Unsupported subchannel type");
            }

            byte[] buffer = new byte[sector_size * length];

            imageStream = chars.fileFilter.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);
            br.BaseStream.Seek((long)_track.TrackFileOffset + (long)(sectorAddress * (sector_offset + sector_size + sector_skip)), SeekOrigin.Begin);

            if(sector_offset == 0 && sector_skip == 0)
                buffer = br.ReadBytes((int)(sector_size * length));
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
            // TODO: Cross data files
            Track _track = new Track();
            DataFileCharacteristics chars = new DataFileCharacteristics();

            _track.TrackSequence = 0;

            foreach(Track bwTrack in tracks)
            {
                if(bwTrack.TrackSequence == track)
                {
                    _track = bwTrack;
                    break;
                }
            }

            if(_track.TrackSequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > (_track.TrackEndSector))
                throw new ArgumentOutOfRangeException(nameof(length), string.Format("Requested more sectors ({0}) than present in track ({1}), won't cross tracks", length + sectorAddress, _track.TrackEndSector));

            foreach(DataFileCharacteristics _chars in filePaths)
            {
                if((long)sectorAddress >= _chars.startLba && length < ((ulong)_chars.sectors - sectorAddress))
                {
                    chars = _chars;
                    break;
                }
            }

            if(string.IsNullOrEmpty(chars.filePath) || chars.fileFilter == null)
                throw new ArgumentOutOfRangeException(nameof(chars.fileFilter), "Track does not exist in disc image");

            if(_track.TrackType == TrackType.Data)
                throw new ArgumentException("Unsupported tag requested", nameof(tag));

            switch(tag)
            {
                case SectorTagType.CDSectorECC:
                case SectorTagType.CDSectorECC_P:
                case SectorTagType.CDSectorECC_Q:
                case SectorTagType.CDSectorEDC:
                case SectorTagType.CDSectorHeader:
                case SectorTagType.CDSectorSubchannel:
                case SectorTagType.CDSectorSubHeader:
                case SectorTagType.CDSectorSync:
                    break;
                case SectorTagType.CDTrackFlags:
                    byte flag;
                    if(trackFlags.TryGetValue(track, out flag))
                        return new byte[] { flag };
                    throw new ArgumentException("Unsupported tag requested", nameof(tag));
                default:
                    throw new ArgumentException("Unsupported tag requested", nameof(tag));
            }

            uint sector_offset;
            uint sector_size;
            uint sector_skip;

            switch(_track.TrackType)
            {
                case TrackType.CDMode1:
                    switch(tag)
                    {
                        case SectorTagType.CDSectorSync:
                            {
                                sector_offset = 0;
                                sector_size = 12;
                                sector_skip = 2340;
                                break;
                            }
                        case SectorTagType.CDSectorHeader:
                            {
                                sector_offset = 12;
                                sector_size = 4;
                                sector_skip = 2336;
                                break;
                            }
                        case SectorTagType.CDSectorSubHeader:
                            throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                        case SectorTagType.CDSectorECC:
                            {
                                sector_offset = 2076;
                                sector_size = 276;
                                sector_skip = 0;
                                break;
                            }
                        case SectorTagType.CDSectorECC_P:
                            {
                                sector_offset = 2076;
                                sector_size = 172;
                                sector_skip = 104;
                                break;
                            }
                        case SectorTagType.CDSectorECC_Q:
                            {
                                sector_offset = 2248;
                                sector_size = 104;
                                sector_skip = 0;
                                break;
                            }
                        case SectorTagType.CDSectorEDC:
                            {
                                sector_offset = 2064;
                                sector_size = 4;
                                sector_skip = 284;
                                break;
                            }
                        case SectorTagType.CDSectorSubchannel:
                            throw new NotImplementedException("Packed subchannel not yet supported");
                        default:
                            throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }
                    break;
                case TrackType.CDMode2Formless:
                    {
                        switch(tag)
                        {
                            case SectorTagType.CDSectorSync:
                            case SectorTagType.CDSectorHeader:
                            case SectorTagType.CDSectorECC:
                            case SectorTagType.CDSectorECC_P:
                            case SectorTagType.CDSectorECC_Q:
                                throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                            case SectorTagType.CDSectorSubHeader:
                                {
                                    sector_offset = 0;
                                    sector_size = 8;
                                    sector_skip = 2328;
                                    break;
                                }
                            case SectorTagType.CDSectorEDC:
                                {
                                    sector_offset = 2332;
                                    sector_size = 4;
                                    sector_skip = 0;
                                    break;
                                }
                            case SectorTagType.CDSectorSubchannel:
                                throw new NotImplementedException("Packed subchannel not yet supported");
                            default:
                                throw new ArgumentException("Unsupported tag requested", nameof(tag));
                        }
                        break;
                    }
                case TrackType.CDMode2Form1:
                    switch(tag)
                    {
                        case SectorTagType.CDSectorSync:
                            {
                                sector_offset = 0;
                                sector_size = 12;
                                sector_skip = 2340;
                                break;
                            }
                        case SectorTagType.CDSectorHeader:
                            {
                                sector_offset = 12;
                                sector_size = 4;
                                sector_skip = 2336;
                                break;
                            }
                        case SectorTagType.CDSectorSubHeader:
                            {
                                sector_offset = 16;
                                sector_size = 8;
                                sector_skip = 2328;
                                break;
                            }
                        case SectorTagType.CDSectorECC:
                            {
                                sector_offset = 2076;
                                sector_size = 276;
                                sector_skip = 0;
                                break;
                            }
                        case SectorTagType.CDSectorECC_P:
                            {
                                sector_offset = 2076;
                                sector_size = 172;
                                sector_skip = 104;
                                break;
                            }
                        case SectorTagType.CDSectorECC_Q:
                            {
                                sector_offset = 2248;
                                sector_size = 104;
                                sector_skip = 0;
                                break;
                            }
                        case SectorTagType.CDSectorEDC:
                            {
                                sector_offset = 2072;
                                sector_size = 4;
                                sector_skip = 276;
                                break;
                            }
                        case SectorTagType.CDSectorSubchannel:
                            throw new NotImplementedException("Packed subchannel not yet supported");
                        default:
                            throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }
                    break;
                case TrackType.CDMode2Form2:
                    switch(tag)
                    {
                        case SectorTagType.CDSectorSync:
                            {
                                sector_offset = 0;
                                sector_size = 12;
                                sector_skip = 2340;
                                break;
                            }
                        case SectorTagType.CDSectorHeader:
                            {
                                sector_offset = 12;
                                sector_size = 4;
                                sector_skip = 2336;
                                break;
                            }
                        case SectorTagType.CDSectorSubHeader:
                            {
                                sector_offset = 16;
                                sector_size = 8;
                                sector_skip = 2328;
                                break;
                            }
                        case SectorTagType.CDSectorEDC:
                            {
                                sector_offset = 2348;
                                sector_size = 4;
                                sector_skip = 0;
                                break;
                            }
                        case SectorTagType.CDSectorSubchannel:
                            throw new NotImplementedException("Packed subchannel not yet supported");
                        default:
                            throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }
                    break;
                case TrackType.Audio:
                    {
                        switch(tag)
                        {
                            case SectorTagType.CDSectorSubchannel:
                                throw new NotImplementedException("Packed subchannel not yet supported");
                            default:
                                throw new ArgumentException("Unsupported tag requested", nameof(tag));
                        }
                    }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            switch(chars.subchannel)
            {
                case TrackSubchannelType.None:
                    sector_skip += 0;
                    break;
                case TrackSubchannelType.Q16Interleaved:
                    sector_skip += 16;
                    break;
                case TrackSubchannelType.PackedInterleaved:
                    sector_skip += 96;
                    break;
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Unsupported subchannel type");
            }

            byte[] buffer = new byte[sector_size * length];

            imageStream = _track.TrackFilter.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);
            br.BaseStream.Seek((long)_track.TrackFileOffset + (long)(sectorAddress * (sector_offset + sector_size + sector_skip)), SeekOrigin.Begin);
            if(sector_offset == 0 && sector_skip == 0)
                buffer = br.ReadBytes((int)(sector_size * length));
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
                    foreach(Track track in tracks)
                    {
                        if(track.TrackSequence == kvp.Key)
                        {
                            if((sectorAddress - kvp.Value) < (track.TrackEndSector - track.TrackStartSector))
                                return ReadSectorsLong((sectorAddress - kvp.Value), length, kvp.Key);
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            // TODO: Cross data files
            Track _track = new Track();
            DataFileCharacteristics chars = new DataFileCharacteristics();

            _track.TrackSequence = 0;

            foreach(Track bwTrack in tracks)
            {
                if(bwTrack.TrackSequence == track)
                {
                    _track = bwTrack;
                    break;
                }
            }

            if(_track.TrackSequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > (_track.TrackEndSector))
                throw new ArgumentOutOfRangeException(nameof(length), string.Format("Requested more sectors ({0}) than present in track ({1}), won't cross tracks", length + sectorAddress, _track.TrackEndSector));

            foreach(DataFileCharacteristics _chars in filePaths)
            {
                if((long)sectorAddress >= _chars.startLba && length < ((ulong)_chars.sectors - sectorAddress))
                {
                    chars = _chars;
                    break;
                }
            }

            if(string.IsNullOrEmpty(chars.filePath) || chars.fileFilter == null)
                throw new ArgumentOutOfRangeException(nameof(chars.fileFilter), "Track does not exist in disc image");

            uint sector_offset;
            uint sector_size;
            uint sector_skip;

            switch(_track.TrackType)
            {
                case TrackType.CDMode1:
                case TrackType.CDMode2Formless:
                case TrackType.CDMode2Form1:
                case TrackType.CDMode2Form2:
                case TrackType.Audio:
                    {
                        sector_offset = 0;
                        sector_size = 2352;
                        sector_skip = 0;
                        break;
                    }
                case TrackType.Data:
                    {
                        sector_offset = 0;
                        sector_size = 2048;
                        sector_skip = 0;
                        break;
                    }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            switch(chars.subchannel)
            {
                case TrackSubchannelType.None:
                    sector_skip += 0;
                    break;
                case TrackSubchannelType.Q16Interleaved:
                    sector_skip += 16;
                    break;
                case TrackSubchannelType.PackedInterleaved:
                    sector_skip += 96;
                    break;
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Unsupported subchannel type");
            }

            byte[] buffer = new byte[sector_size * length];

            imageStream = _track.TrackFilter.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);
            br.BaseStream.Seek((long)_track.TrackFileOffset + (long)(sectorAddress * (sector_offset + sector_size + sector_skip)), SeekOrigin.Begin);
            if(sector_offset == 0 && sector_skip == 0)
                buffer = br.ReadBytes((int)(sector_size * length));
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

        public override string GetImageFormat()
        {
            return "BlindWrite 5 TOC file";
        }

        public override string GetImageVersion()
        {
            return ImageInfo.imageVersion;
        }

        public override string GetImageApplication()
        {
            return ImageInfo.imageApplication;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.mediaType;
        }

        public override List<Partition> GetPartitions()
        {
            return partitions;
        }

        public override List<Track> GetTracks()
        {
            return tracks;
        }

        public override List<Track> GetSessionTracks(Session session)
        {
            if(sessions.Contains(session))
            {
                return GetSessionTracks(session.SessionSequence);
            }
            throw new ImageNotSupportedException("Session does not exist in disc image");
        }

        public override List<Track> GetSessionTracks(ushort session)
        {
            List<Track> _tracks = new List<Track>();
            foreach(Track _track in tracks)
            {
                if(_track.TrackSession == session)
                    _tracks.Add(_track);
            }

            return _tracks;
        }
        public override List<Session> GetSessions()
        {
            return sessions;
        }

        public override bool? VerifySector(ulong sectorAddress)
        {
            byte[] buffer = ReadSectorLong(sectorAddress);
            return Checksums.CDChecksums.CheckCDSector(buffer);
        }

        public override bool? VerifySector(ulong sectorAddress, uint track)
        {
            byte[] buffer = ReadSectorLong(sectorAddress, track);
            return Checksums.CDChecksums.CheckCDSector(buffer);
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
        {
            byte[] buffer = ReadSectorsLong(sectorAddress, length);
            int bps = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];
            FailingLBAs = new List<ulong>();
            UnknownLBAs = new List<ulong>();

            for(int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * bps, sector, 0, bps);
                bool? sectorStatus = Checksums.CDChecksums.CheckCDSector(sector);

                switch(sectorStatus)
                {
                    case null:
                        UnknownLBAs.Add((ulong)i + sectorAddress);
                        break;
                    case false:
                        FailingLBAs.Add((ulong)i + sectorAddress);
                        break;
                }
            }

            if(UnknownLBAs.Count > 0)
                return null;
            if(FailingLBAs.Count > 0)
                return false;
            return true;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
        {
            byte[] buffer = ReadSectorsLong(sectorAddress, length, track);
            int bps = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];
            FailingLBAs = new List<ulong>();
            UnknownLBAs = new List<ulong>();

            for(int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * bps, sector, 0, bps);
                bool? sectorStatus = Checksums.CDChecksums.CheckCDSector(sector);

                switch(sectorStatus)
                {
                    case null:
                        UnknownLBAs.Add((ulong)i + sectorAddress);
                        break;
                    case false:
                        FailingLBAs.Add((ulong)i + sectorAddress);
                        break;
                }
            }

            if(UnknownLBAs.Count > 0)
                return null;
            if(FailingLBAs.Count > 0)
                return false;
            return true;
        }

        public override bool? VerifyMediaImage()
        {
            return null;
        }
        #endregion Public Methods

        #region Private methods
        static TrackType BlindWriteTrackTypeToTrackType(BW5_TrackType trackType)
        {
            switch(trackType)
            {
                case BW5_TrackType.Mode1:
                    return TrackType.CDMode1;
                case BW5_TrackType.Mode2F1:
                    return TrackType.CDMode2Form1;
                case BW5_TrackType.Mode2F2:
                    return TrackType.CDMode2Form2;
                case BW5_TrackType.Mode2:
                    return TrackType.CDMode2Formless;
                case BW5_TrackType.Audio:
                    return TrackType.Audio;
                default:
                    return TrackType.Data;
            }
        }

        static MediaType BlindWriteProfileToMediaType(ProfileNumber profile)
        {
            switch(profile)
            {
                case ProfileNumber.BDRE:
                    return MediaType.BDRE;
                case ProfileNumber.BDROM:
                    return MediaType.BDROM;
                case ProfileNumber.BDRRdm:
                case ProfileNumber.BDRSeq:
                    return MediaType.BDR;
                case ProfileNumber.CDR:
                case ProfileNumber.HDBURNR:
                    return MediaType.CDR;
                case ProfileNumber.CDROM:
                case ProfileNumber.HDBURNROM:
                    return MediaType.CDROM;
                case ProfileNumber.CDRW:
                case ProfileNumber.HDBURNRW:
                    return MediaType.CDRW;
                case ProfileNumber.DDCDR:
                    return MediaType.DDCDR;
                case ProfileNumber.DDCDROM:
                    return MediaType.DDCD;
                case ProfileNumber.DDCDRW:
                    return MediaType.DDCDRW;
                case ProfileNumber.DVDDownload:
                    return MediaType.DVDDownload;
                case ProfileNumber.DVDRAM:
                    return MediaType.DVDRAM;
                case ProfileNumber.DVDRDLJump:
                case ProfileNumber.DVDRDLSeq:
                    return MediaType.DVDRDL;
                case ProfileNumber.DVDRDLPlus:
                    return MediaType.DVDPRDL;
                case ProfileNumber.DVDROM:
                    return MediaType.DVDROM;
                case ProfileNumber.DVDRPlus:
                    return MediaType.DVDPR;
                case ProfileNumber.DVDRSeq:
                    return MediaType.DVDR;
                case ProfileNumber.DVDRWDL:
                    return MediaType.DVDRWDL;
                case ProfileNumber.DVDRWDLPlus:
                    return MediaType.DVDPRWDL;
                case ProfileNumber.DVDRWPlus:
                    return MediaType.DVDPRW;
                case ProfileNumber.DVDRWRes:
                case ProfileNumber.DVDRWSeq:
                    return MediaType.DVDRW;
                case ProfileNumber.HDDVDR:
                    return MediaType.HDDVDR;
                case ProfileNumber.HDDVDRAM:
                    return MediaType.HDDVDRAM;
                case ProfileNumber.HDDVDRDL:
                    return MediaType.HDDVDRDL;
                case ProfileNumber.HDDVDROM:
                    return MediaType.HDDVDROM;
                case ProfileNumber.HDDVDRW:
                    return MediaType.HDDVDRW;
                case ProfileNumber.HDDVDRWDL:
                    return MediaType.HDDVDRWDL;
                case ProfileNumber.ASMO:
                case ProfileNumber.MOErasable:
                    return MediaType.UnknownMO;
                case ProfileNumber.NonRemovable:
                    return MediaType.GENERIC_HDD;
                default:
                    return MediaType.CD;
            }
        }
        #endregion Private methods

        #region Unsupported features
        public override string GetImageApplicationVersion()
        {
            return ImageInfo.imageApplicationVersion;
        }

        public override DateTime GetImageCreationTime()
        {
            return ImageInfo.imageCreationTime;
        }

        public override DateTime GetImageLastModificationTime()
        {
            return ImageInfo.imageLastModificationTime;
        }

        public override string GetImageComments()
        {
            return ImageInfo.imageComments;
        }

        public override string GetMediaSerialNumber()
        {
            return ImageInfo.mediaSerialNumber;
        }

        public override string GetMediaBarcode()
        {
            return ImageInfo.mediaBarcode;
        }

        public override int GetMediaSequence()
        {
            return ImageInfo.mediaSequence;
        }

        public override int GetLastDiskSequence()
        {
            return ImageInfo.lastMediaSequence;
        }

        public override string GetDriveManufacturer()
        {
            return ImageInfo.driveManufacturer;
        }

        public override string GetDriveModel()
        {
            return ImageInfo.driveModel;
        }

        public override string GetDriveSerialNumber()
        {
            return ImageInfo.driveSerialNumber;
        }

        public override string GetMediaPartNumber()
        {
            return ImageInfo.mediaPartNumber;
        }

        public override string GetMediaManufacturer()
        {
            return ImageInfo.mediaManufacturer;
        }

        public override string GetMediaModel()
        {
            return ImageInfo.mediaModel;
        }

        public override string GetImageName()
        {
            return ImageInfo.imageName;
        }

        public override string GetImageCreator()
        {
            return ImageInfo.imageCreator;
        }
        #endregion Unsupported features
    }
}

