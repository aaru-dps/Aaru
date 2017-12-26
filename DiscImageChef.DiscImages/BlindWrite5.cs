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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.Checksums;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Decoders.CD;
using DiscImageChef.Decoders.DVD;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Decoders.SCSI.MMC;
using DiscImageChef.Filters;
using DMI = DiscImageChef.Decoders.Xbox.DMI;

namespace DiscImageChef.DiscImages
{
    public class BlindWrite5 : IMediaImage
    {
        /// <summary>"BWT5 STREAM FOOT"</summary>
        readonly byte[] bw5Footer =
            {0x42, 0x57, 0x54, 0x35, 0x20, 0x53, 0x54, 0x52, 0x45, 0x41, 0x4D, 0x20, 0x46, 0x4F, 0x4F, 0x54};
        /// <summary>"BWT5 STREAM SIGN"</summary>
        readonly byte[] bw5Signature =
            {0x42, 0x57, 0x54, 0x35, 0x20, 0x53, 0x54, 0x52, 0x45, 0x41, 0x4D, 0x20, 0x53, 0x49, 0x47, 0x4E};
        byte[] atip;
        byte[] bca;
        List<Bw5SessionDescriptor> bwSessions;
        byte[] cdtext;
        List<Bw5DataFile> dataFiles;
        string dataPath;
        byte[] discInformation;
        byte[] dmi;
        byte[] dpm;
        List<DataFileCharacteristics> filePaths;
        byte[] fullToc;

        Bw5Header header;
        ImageInfo imageInfo;
        Stream imageStream;
        byte[] mode2A;
        Dictionary<uint, ulong> offsetmap;
        List<Partition> partitions;
        byte[] pfi;
        byte[] pma;
        List<Session> sessions;
        Dictionary<uint, byte> trackFlags;
        List<Track> tracks;
        byte[] unkBlock;

        public BlindWrite5()
        {
            imageInfo = new ImageInfo
            {
                ReadableSectorTags = new List<SectorTagType>(),
                ReadableMediaTags = new List<MediaTagType>(),
                HasPartitions = true,
                HasSessions = true,
                Version = null,
                ApplicationVersion = null,
                MediaTitle = null,
                Creator = null,
                MediaManufacturer = null,
                MediaModel = null,
                MediaPartNumber = null,
                MediaSequence = 0,
                LastMediaSequence = 0,
                DriveManufacturer = null,
                DriveModel = null,
                DriveSerialNumber = null,
                DriveFirmwareRevision = null
            };
        }

        public virtual ImageInfo Info => imageInfo;

        public virtual string Name => "BlindWrite 5";
        public virtual Guid Id => new Guid("9CB7A381-0509-4F9F-B801-3F65434BC3EE");

        public virtual string ImageFormat => "BlindWrite 5 TOC file";

        public virtual List<Partition> Partitions => partitions;

        public virtual List<Track> Tracks => tracks;

        public virtual List<Session> Sessions => sessions;

        public virtual bool IdentifyImage(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            if(stream.Length < 276) return false;

            byte[] signature = new byte[16];
            stream.Read(signature, 0, 16);

            byte[] footer = new byte[16];
            stream.Seek(-16, SeekOrigin.End);
            stream.Read(footer, 0, 16);

            return bw5Signature.SequenceEqual(signature) && bw5Footer.SequenceEqual(footer);
        }

        public virtual bool OpenImage(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            if(stream.Length < 276) return false;

            byte[] hdr = new byte[260];
            stream.Read(hdr, 0, 260);
            header = new Bw5Header();
            IntPtr hdrPtr = Marshal.AllocHGlobal(260);
            Marshal.Copy(hdr, 0, hdrPtr, 260);
            header = (Bw5Header)Marshal.PtrToStructure(hdrPtr, typeof(Bw5Header));
            Marshal.FreeHGlobal(hdrPtr);

            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.signature = {0}",
                                      StringHandlers.CToString(header.signature));
            for(int i = 0; i < header.unknown1.Length; i++)
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.unknown1[{1}] = 0x{0:X8}", header.unknown1[i],
                                          i);

            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.profile = {0}", header.profile);
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.sessions = {0}", header.sessions);
            for(int i = 0; i < header.unknown2.Length; i++)
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.unknown2[{1}] = 0x{0:X8}", header.unknown2[i],
                                          i);

            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.mcnIsValid = {0}", header.mcnIsValid);
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.mcn = {0}", StringHandlers.CToString(header.mcn));
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.unknown3 = 0x{0:X4}", header.unknown3);
            for(int i = 0; i < header.unknown4.Length; i++)
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.unknown4[{1}] = 0x{0:X8}", header.unknown4[i],
                                          i);

            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.pmaLen = {0}", header.pmaLen);
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.atipLen = {0}", header.atipLen);
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.cdtLen = {0}", header.cdtLen);
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.cdInfoLen = {0}", header.cdInfoLen);
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.bcaLen = {0}", header.bcaLen);
            for(int i = 0; i < header.unknown5.Length; i++)
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.unknown5[{1}] = 0x{0:X8}", header.unknown5[i],
                                          i);

            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.dvdStrLen = {0}", header.dvdStrLen);
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.dvdInfoLen = {0}", header.dvdInfoLen);
            for(int i = 0; i < header.unknown6.Length; i++)
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.unknown6[{1}] = 0x{0:X2}", header.unknown6[i],
                                          i);

            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.manufacturer = {0}",
                                      StringHandlers.CToString(header.manufacturer));
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.product = {0}",
                                      StringHandlers.CToString(header.product));
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.revision = {0}",
                                      StringHandlers.CToString(header.revision));
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.vendor = {0}",
                                      StringHandlers.CToString(header.vendor));
            DicConsole.DebugWriteLine("BlindWrite5 plugin", "header.volumeId = {0}",
                                      StringHandlers.CToString(header.volumeId));
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
                Modes.ModePage_2A? decoded2A = Modes.DecodeModePage_2A(mode2A);
                if(decoded2A.HasValue)
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "mode page 2A: {0}",
                                              Modes.PrettifyModePage_2A(decoded2A));
                else mode2A = null;
            }

            unkBlock = new byte[header.unkBlkLen];
            if(unkBlock.Length > 0) stream.Read(unkBlock, 0, unkBlock.Length);

            byte[] temp = new byte[header.pmaLen];
            if(temp.Length > 0)
            {
                byte[] tushort = BitConverter.GetBytes((ushort)(temp.Length + 2));
                stream.Read(temp, 0, temp.Length);
                pma = new byte[temp.Length + 4];
                pma[0] = tushort[1];
                pma[1] = tushort[0];
                Array.Copy(temp, 0, pma, 4, temp.Length);

                PMA.CDPMA? decodedPma = PMA.Decode(pma);
                if(decodedPma.HasValue)
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "PMA: {0}", PMA.Prettify(decodedPma));
                else pma = null;
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

                ATIP.CDATIP? decodedAtip = ATIP.Decode(atip);
                if(decodedAtip.HasValue)
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "ATIP: {0}", ATIP.Prettify(decodedAtip));
                else atip = null;
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

                CDTextOnLeadIn.CDText? decodedCdText = CDTextOnLeadIn.Decode(cdtext);
                if(decodedCdText.HasValue)
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "CD-Text: {0}",
                                              CDTextOnLeadIn.Prettify(decodedCdText));
                else cdtext = null;
            }

            bca = new byte[header.bcaLen];
            if(bca.Length > 0) stream.Read(bca, 0, bca.Length);
            else bca = null;

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

                PFI.PhysicalFormatInformation? decodedPfi = PFI.Decode(pfi);
                if(decodedPfi.HasValue)
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "PFI: {0}", PFI.Prettify(decodedPfi));
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
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "Disc information: {0}",
                                          PrintHex.ByteArrayToHexArrayString(discInformation, 40));
            }
            else discInformation = null;

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

            dataFiles = new List<Bw5DataFile>();
            for(int cD = 0; cD < dataBlockCount; cD++)
            {
                tmpArray = new byte[52];
                Bw5DataFile dataFile = new Bw5DataFile {Unknown1 = new uint[4], Unknown2 = new uint[3]};

                stream.Read(tmpArray, 0, tmpArray.Length);
                dataFile.Type = BitConverter.ToUInt32(tmpArray, 0);
                dataFile.Length = BitConverter.ToUInt32(tmpArray, 4);
                dataFile.Unknown1[0] = BitConverter.ToUInt32(tmpArray, 8);
                dataFile.Unknown1[1] = BitConverter.ToUInt32(tmpArray, 12);
                dataFile.Unknown1[2] = BitConverter.ToUInt32(tmpArray, 16);
                dataFile.Unknown1[3] = BitConverter.ToUInt32(tmpArray, 20);
                dataFile.Offset = BitConverter.ToUInt32(tmpArray, 24);
                dataFile.Unknown2[0] = BitConverter.ToUInt32(tmpArray, 28);
                dataFile.Unknown2[1] = BitConverter.ToUInt32(tmpArray, 32);
                dataFile.Unknown2[2] = BitConverter.ToUInt32(tmpArray, 36);
                dataFile.StartLba = BitConverter.ToInt32(tmpArray, 40);
                dataFile.Sectors = BitConverter.ToInt32(tmpArray, 44);
                dataFile.FilenameLen = BitConverter.ToUInt32(tmpArray, 48);
                dataFile.FilenameBytes = new byte[dataFile.FilenameLen];

                tmpArray = new byte[dataFile.FilenameLen];
                stream.Read(tmpArray, 0, tmpArray.Length);
                dataFile.FilenameBytes = tmpArray;
                tmpArray = new byte[4];
                stream.Read(tmpArray, 0, tmpArray.Length);
                dataFile.Unknown3 = BitConverter.ToUInt32(tmpArray, 0);

                dataFile.Filename = Encoding.Unicode.GetString(dataFile.FilenameBytes);
                dataFiles.Add(dataFile);

                DicConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.type = 0x{0:X8}", dataFile.Type);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.length = {0}", dataFile.Length);
                for(int i = 0; i < dataFile.Unknown1.Length; i++)
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.unknown1[{1}] = {0}",
                                              dataFile.Unknown1[i], i);

                DicConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.offset = {0}", dataFile.Offset);
                for(int i = 0; i < dataFile.Unknown2.Length; i++)
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.unknown2[{1}] = {0}",
                                              dataFile.Unknown2[i], i);

                DicConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.startLba = {0}", dataFile.StartLba);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.sectors = {0}", dataFile.Sectors);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.filenameLen = {0}", dataFile.FilenameLen);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.filename = {0}", dataFile.Filename);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.unknown3 = {0}", dataFile.Unknown3);
            }

            bwSessions = new List<Bw5SessionDescriptor>();
            for(int ses = 0; ses < header.sessions; ses++)
            {
                Bw5SessionDescriptor session = new Bw5SessionDescriptor();
                tmpArray = new byte[16];
                stream.Read(tmpArray, 0, tmpArray.Length);
                session.Sequence = BitConverter.ToUInt16(tmpArray, 0);
                session.Entries = tmpArray[2];
                session.Unknown = tmpArray[3];
                session.Start = BitConverter.ToInt32(tmpArray, 4);
                session.End = BitConverter.ToInt32(tmpArray, 8);
                session.FirstTrack = BitConverter.ToUInt16(tmpArray, 12);
                session.LastTrack = BitConverter.ToUInt16(tmpArray, 14);
                session.Tracks = new Bw5TrackDescriptor[session.Entries];

                DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].filename = {1}", ses, session.Sequence);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].entries = {1}", ses, session.Entries);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].unknown = {1}", ses, session.Unknown);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].start = {1}", ses, session.Start);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].end = {1}", ses, session.End);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].firstTrack = {1}", ses,
                                          session.FirstTrack);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].lastTrack = {1}", ses, session.LastTrack);

                for(int tSeq = 0; tSeq < session.Entries; tSeq++)
                {
                    byte[] trk = new byte[72];
                    stream.Read(trk, 0, 72);
                    session.Tracks[tSeq] = new Bw5TrackDescriptor();
                    IntPtr trkPtr = Marshal.AllocHGlobal(72);
                    Marshal.Copy(trk, 0, trkPtr, 72);
                    session.Tracks[tSeq] =
                        (Bw5TrackDescriptor)Marshal.PtrToStructure(trkPtr, typeof(Bw5TrackDescriptor));
                    Marshal.FreeHGlobal(trkPtr);

                    if(session.Tracks[tSeq].type == Bw5TrackType.Dvd ||
                       session.Tracks[tSeq].type == Bw5TrackType.NotData)
                    {
                        session.Tracks[tSeq].unknown9[0] = 0;
                        session.Tracks[tSeq].unknown9[1] = 0;
                        stream.Seek(-8, SeekOrigin.Current);
                    }

                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].type = {2}", ses, tSeq,
                                              session.Tracks[tSeq].type);
                    for(int i = 0; i < session.Tracks[tSeq].unknown1.Length; i++)
                        DicConsole.DebugWriteLine("BlindWrite5 plugin",
                                                  "session[{0}].track[{1}].unknown1[{2}] = 0x{3:X2}", ses, tSeq, i,
                                                  session.Tracks[tSeq].unknown1[i]);

                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].unknown2 = 0x{2:X8}", ses,
                                              tSeq, session.Tracks[tSeq].unknown2);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].subchannel = {2}", ses,
                                              tSeq, session.Tracks[tSeq].subchannel);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].unknown3 = 0x{2:X2}", ses,
                                              tSeq, session.Tracks[tSeq].unknown3);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].ctl = {2}", ses, tSeq,
                                              session.Tracks[tSeq].ctl);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].adr = {2}", ses, tSeq,
                                              session.Tracks[tSeq].adr);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].point = {2}", ses, tSeq,
                                              session.Tracks[tSeq].point);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].unknown4 = 0x{2:X2}", ses,
                                              tSeq, session.Tracks[tSeq].unknown4);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].min = {2}", ses, tSeq,
                                              session.Tracks[tSeq].min);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].sec = {2}", ses, tSeq,
                                              session.Tracks[tSeq].sec);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].frame = {2}", ses, tSeq,
                                              session.Tracks[tSeq].frame);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].zero = {2}", ses, tSeq,
                                              session.Tracks[tSeq].zero);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].pmin = {2}", ses, tSeq,
                                              session.Tracks[tSeq].pmin);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].psec = {2}", ses, tSeq,
                                              session.Tracks[tSeq].psec);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].pframe = {2}", ses, tSeq,
                                              session.Tracks[tSeq].pframe);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].unknown5 = 0x{2:X2}", ses,
                                              tSeq, session.Tracks[tSeq].unknown5);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].pregap = {2}", ses, tSeq,
                                              session.Tracks[tSeq].pregap);
                    for(int i = 0; i < session.Tracks[tSeq].unknown6.Length; i++)
                        DicConsole.DebugWriteLine("BlindWrite5 plugin",
                                                  "session[{0}].track[{1}].unknown6[{2}] = 0x{3:X8}", ses, tSeq, i,
                                                  session.Tracks[tSeq].unknown6[i]);

                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].startLba = {2}", ses, tSeq,
                                              session.Tracks[tSeq].startLba);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].sectors = {2}", ses, tSeq,
                                              session.Tracks[tSeq].sectors);
                    for(int i = 0; i < session.Tracks[tSeq].unknown7.Length; i++)
                        DicConsole.DebugWriteLine("BlindWrite5 plugin",
                                                  "session[{0}].track[{1}].unknown7[{2}] = 0x{3:X8}", ses, tSeq, i,
                                                  session.Tracks[tSeq].unknown7[i]);

                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].session = {2}", ses, tSeq,
                                              session.Tracks[tSeq].session);
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].unknown8 = 0x{2:X4}", ses,
                                              tSeq, session.Tracks[tSeq].unknown8);
                    if(session.Tracks[tSeq].type == Bw5TrackType.Dvd ||
                       session.Tracks[tSeq].type == Bw5TrackType.NotData) continue;

                    {
                        for(int i = 0; i < session.Tracks[tSeq].unknown9.Length; i++)
                            DicConsole.DebugWriteLine("BlindWrite5 plugin",
                                                      "session[{0}].track[{1}].unknown9[{2}] = 0x{3:X8}", ses, tSeq, i,
                                                      session.Tracks[tSeq].unknown9[i]);
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

            if(bw5Footer.SequenceEqual(footer))
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "Correctly arrived end of image");
            else
                DicConsole
                    .ErrorWriteLine("BlindWrite5 image ends after expected position. Probably new version with different data. Errors may occur.");

            filePaths = new List<DataFileCharacteristics>();
            foreach(Bw5DataFile dataFile in dataFiles)
            {
                DataFileCharacteristics chars = new DataFileCharacteristics();
                string path = Path.Combine(dataPath, dataFile.Filename);
                FiltersList filtersList = new FiltersList();

                if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path)) != null)
                {
                    chars.FileFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path));
                    chars.FilePath = path;
                }
                else
                {
                    path = Path.Combine(dataPath, dataFile.Filename.ToLower(CultureInfo.CurrentCulture));
                    if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path)) != null)
                    {
                        chars.FileFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path));
                        chars.FilePath = path;
                    }
                    else
                    {
                        path = Path.Combine(dataPath, dataFile.Filename.ToUpper(CultureInfo.CurrentCulture));
                        if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path)) != null)
                        {
                            chars.FileFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path));
                            chars.FilePath = path;
                        }
                        else
                        {
                            path = Path.Combine(dataPath.ToLower(CultureInfo.CurrentCulture), dataFile.Filename);
                            if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path)) != null)
                            {
                                chars.FileFilter =
                                    filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path));
                                chars.FilePath = path;
                            }
                            else
                            {
                                path = Path.Combine(dataPath.ToUpper(CultureInfo.CurrentCulture), dataFile.Filename);
                                if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path)) != null)
                                {
                                    chars.FileFilter =
                                        filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path));
                                    chars.FilePath = path;
                                }
                                else
                                {
                                    path = Path.Combine(dataPath, dataFile.Filename);
                                    if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                          path.ToLower(CultureInfo.CurrentCulture))) !=
                                       null)
                                    {
                                        chars.FilePath = path.ToLower(CultureInfo.CurrentCulture);
                                        chars.FileFilter =
                                            filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                               path.ToLower(CultureInfo
                                                                                                .CurrentCulture)));
                                    }
                                    else if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                               path.ToUpper(CultureInfo
                                                                                                .CurrentCulture))) !=
                                            null)
                                    {
                                        chars.FilePath = path.ToUpper(CultureInfo.CurrentCulture);
                                        chars.FileFilter =
                                            filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                               path.ToUpper(CultureInfo
                                                                                                .CurrentCulture)));
                                    }
                                    else if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                               dataFile.Filename.ToLower(CultureInfo
                                                                                                             .CurrentCulture))) !=
                                            null)
                                    {
                                        chars.FilePath = dataFile.Filename.ToLower(CultureInfo.CurrentCulture);
                                        chars.FileFilter =
                                            filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                               dataFile.Filename.ToLower(CultureInfo
                                                                                                             .CurrentCulture)));
                                    }
                                    else if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                               dataFile.Filename.ToUpper(CultureInfo
                                                                                                             .CurrentCulture))) !=
                                            null)
                                    {
                                        chars.FilePath = dataFile.Filename.ToUpper(CultureInfo.CurrentCulture);
                                        chars.FileFilter =
                                            filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                               dataFile.Filename.ToUpper(CultureInfo
                                                                                                             .CurrentCulture)));
                                    }
                                    else if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                               dataFile.Filename)) != null)
                                    {
                                        chars.FilePath = dataFile.Filename;
                                        chars.FileFilter =
                                            filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                               dataFile.Filename));
                                    }
                                    else
                                    {
                                        DicConsole.ErrorWriteLine("Cannot find data file {0}", dataFile.Filename);
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }

                long sectorSize = dataFile.Length / dataFile.Sectors;
                if(sectorSize > 2352)
                    switch(sectorSize - 2352)
                    {
                        case 16:
                            chars.Subchannel = TrackSubchannelType.Q16Interleaved;
                            break;
                        case 96:
                            chars.Subchannel = TrackSubchannelType.PackedInterleaved;
                            break;
                        default:
                            DicConsole.ErrorWriteLine("BlindWrite5 found unknown subchannel size: {0}",
                                                      sectorSize - 2352);
                            return false;
                    }
                else chars.Subchannel = TrackSubchannelType.None;

                chars.SectorSize = sectorSize;
                chars.StartLba = dataFile.StartLba;
                chars.Sectors = dataFile.Sectors;

                filePaths.Add(chars);
            }

            sessions = new List<Session>();
            tracks = new List<Track>();
            partitions = new List<Partition>();
            MemoryStream fullTocStream = new MemoryStream();
            fullTocStream.Write(new byte[] {0, 0, 0, 0}, 0, 4);
            ulong offsetBytes = 0;
            offsetmap = new Dictionary<uint, ulong>();
            bool isDvd = false;
            byte firstSession = byte.MaxValue;
            byte lastSession = 0;
            trackFlags = new Dictionary<uint, byte>();
            imageInfo.Sectors = 0;

            DicConsole.DebugWriteLine("BlindWrite5 plugin", "Building maps");
            foreach(Bw5SessionDescriptor ses in bwSessions)
            {
                // TODO: This does nothing, should it?
                /*
                Session session = new Session {SessionSequence = ses.Sequence};
                if(ses.Start < 0) session.StartSector = 0;
                else session.StartSector = (ulong)ses.Start;
                session.EndSector = (ulong)ses.End;
                session.StartTrack = ses.FirstTrack;
                session.EndTrack = ses.LastTrack;
                */

                if(ses.Sequence < firstSession) firstSession = (byte)ses.Sequence;
                if(ses.Sequence > lastSession) lastSession = (byte)ses.Sequence;

                foreach(Bw5TrackDescriptor trk in ses.Tracks)
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

                    if(trk.point >= 0xA0) continue;

                    Track track = new Track();
                    Partition partition = new Partition();

                    trackFlags.Add(trk.point, trk.ctl);

                    switch(trk.type)
                    {
                        case Bw5TrackType.Audio:
                            track.TrackBytesPerSector = 2352;
                            track.TrackRawBytesPerSector = 2352;
                            if(imageInfo.SectorSize < 2352) imageInfo.SectorSize = 2352;
                            break;
                        case Bw5TrackType.Mode1:
                        case Bw5TrackType.Mode2F1:
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
                            track.TrackBytesPerSector = 2048;
                            track.TrackRawBytesPerSector = 2352;
                            if(imageInfo.SectorSize < 2048) imageInfo.SectorSize = 2048;
                            break;
                        case Bw5TrackType.Mode2:
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);
                            track.TrackBytesPerSector = 2336;
                            track.TrackRawBytesPerSector = 2352;
                            if(imageInfo.SectorSize < 2336) imageInfo.SectorSize = 2336;
                            break;
                        case Bw5TrackType.Mode2F2:
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);
                            track.TrackBytesPerSector = 2336;
                            track.TrackRawBytesPerSector = 2352;
                            if(imageInfo.SectorSize < 2324) imageInfo.SectorSize = 2324;
                            break;
                        case Bw5TrackType.Dvd:
                            track.TrackBytesPerSector = 2048;
                            track.TrackRawBytesPerSector = 2048;
                            if(imageInfo.SectorSize < 2048) imageInfo.SectorSize = 2048;
                            isDvd = true;
                            break;
                    }

                    track.TrackDescription = $"Track {trk.point}";
                    track.TrackStartSector = (ulong)(trk.startLba + trk.pregap);
                    track.TrackEndSector = (ulong)(trk.sectors + trk.startLba);

                    foreach(DataFileCharacteristics chars in filePaths.Where(chars => trk.startLba >= chars.StartLba &&
                                                                                      trk.startLba + trk.sectors <=
                                                                                      chars.StartLba + chars.Sectors))
                    {
                        track.TrackFilter = chars.FileFilter;
                        track.TrackFile = chars.FileFilter.GetFilename();
                        if(trk.startLba >= 0)
                            track.TrackFileOffset = (ulong)((trk.startLba - chars.StartLba) * chars.SectorSize);
                        else track.TrackFileOffset = (ulong)(trk.startLba * -1 * chars.SectorSize);
                        track.TrackFileType = "BINARY";
                        if(chars.Subchannel != TrackSubchannelType.None)
                        {
                            track.TrackSubchannelFilter = track.TrackFilter;
                            track.TrackSubchannelFile = track.TrackFile;
                            track.TrackSubchannelType = chars.Subchannel;
                            track.TrackSubchannelOffset = track.TrackFileOffset;

                            if(chars.Subchannel == TrackSubchannelType.PackedInterleaved)
                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);
                        }

                        break;
                    }

                    track.TrackPregap = trk.pregap;
                    track.TrackSequence = trk.point;
                    track.TrackType = BlindWriteTrackTypeToTrackType(trk.type);
                    track.Indexes = new Dictionary<int, ulong> {{1, track.TrackStartSector}};

                    partition.Description = track.TrackDescription;
                    partition.Size = (track.TrackEndSector - track.TrackStartSector) *
                                     (ulong)track.TrackRawBytesPerSector;
                    partition.Length = track.TrackEndSector - track.TrackStartSector;
                    partition.Sequence = track.TrackSequence;
                    partition.Offset = offsetBytes;
                    partition.Start = track.TrackStartSector;
                    partition.Type = track.TrackType.ToString();

                    offsetBytes += partition.Size;

                    tracks.Add(track);
                    partitions.Add(partition);
                    offsetmap.Add(track.TrackSequence, track.TrackStartSector);
                    imageInfo.Sectors += partition.Length;
                }
            }

            DicConsole.DebugWriteLine("BlindWrite5 plugin", "printing track map");
            foreach(Track track in tracks)
            {
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "Partition sequence: {0}", track.TrackSequence);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition description: {0}", track.TrackDescription);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition type: {0}", track.TrackType);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition starting sector: {0}",
                                          track.TrackStartSector);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition ending sector: {0}", track.TrackEndSector);
            }

            DicConsole.DebugWriteLine("BlindWrite5 plugin", "printing partition map");
            foreach(Partition partition in partitions)
            {
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "Partition sequence: {0}", partition.Sequence);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition name: {0}", partition.Name);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition description: {0}", partition.Description);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition type: {0}", partition.Type);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition starting sector: {0}", partition.Start);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition sectors: {0}", partition.Length);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition starting offset: {0}", partition.Offset);
                DicConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition size in bytes: {0}", partition.Size);
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

                FullTOC.CDFullTOC? decodedFullToc = FullTOC.Decode(fullToc);

                if(!decodedFullToc.HasValue)
                {
                    DicConsole.DebugWriteLine("BlindWrite5 plugin", "TOC not correctly rebuilt");
                    fullToc = null;
                }
                else DicConsole.DebugWriteLine("BlindWrite5 plugin", "TOC correctly rebuilt");

                imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackFlags);
            }

            imageInfo.MediaType = BlindWriteProfileToMediaType(header.profile);

            if(dmi != null && pfi != null)
            {
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
                            imageInfo.MediaType = pfi0.Value.DiscSize == DVDSize.Eighty ? MediaType.GOD : MediaType.WOD;
                            break;
                        case DiskCategory.UMD:
                            imageInfo.MediaType = MediaType.UMD;
                            break;
                    }

                    if(DMI.IsXbox(dmi)) imageInfo.MediaType = MediaType.XGD;
                    else if(DMI.IsXbox360(dmi)) imageInfo.MediaType = MediaType.XGD2;
                }
            }
            else if(imageInfo.MediaType == MediaType.CD || imageInfo.MediaType == MediaType.CDROM)
            {
                bool data = false;
                bool mode2 = false;
                bool firstaudio = false;
                bool firstdata = false;
                bool audio = false;

                foreach(Track bwTrack in tracks)
                {
                    // First track is audio
                    firstaudio |= bwTrack.TrackSequence == 1 && bwTrack.TrackType == TrackType.Audio;

                    // First track is data
                    firstdata |= bwTrack.TrackSequence == 1 && bwTrack.TrackType != TrackType.Audio;

                    // Any non first track is data
                    data |= bwTrack.TrackSequence != 1 && bwTrack.TrackType != TrackType.Audio;

                    // Any non first track is audio
                    audio |= bwTrack.TrackSequence != 1 && bwTrack.TrackType == TrackType.Audio;

                    switch(bwTrack.TrackType)
                    {
                        case TrackType.CdMode2Formless:
                        case TrackType.CdMode2Form1:
                        case TrackType.CdMode2Form2:
                            mode2 = true;
                            break;
                    }
                }

                if(!data && !firstdata) imageInfo.MediaType = MediaType.CDDA;
                else if(firstaudio && data && sessions.Count > 1 && mode2) imageInfo.MediaType = MediaType.CDPLUS;
                else if(firstdata && audio || mode2) imageInfo.MediaType = MediaType.CDROMXA;
                else if(!audio) imageInfo.MediaType = MediaType.CDROM;
                else imageInfo.MediaType = MediaType.CD;
            }

            imageInfo.DriveManufacturer = StringHandlers.CToString(header.manufacturer);
            imageInfo.DriveModel = StringHandlers.CToString(header.product);
            imageInfo.DriveFirmwareRevision = StringHandlers.CToString(header.revision);
            imageInfo.Application = "BlindWrite";
            if(string.Compare(Path.GetExtension(imageFilter.GetFilename()), "B5T",
                              StringComparison.OrdinalIgnoreCase) == 0) imageInfo.ApplicationVersion = "5";
            else if(string.Compare(Path.GetExtension(imageFilter.GetFilename()), "B6T",
                                   StringComparison.OrdinalIgnoreCase) == 0) imageInfo.ApplicationVersion = "6";
            imageInfo.Version = "5";

            imageInfo.ImageSize = (ulong)imageFilter.GetDataForkLength();
            imageInfo.CreationTime = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.XmlMediaType = XmlMediaType.OpticalDisc;

            if(pma != null)
            {
                PMA.CDPMA pma0 = PMA.Decode(pma).Value;

                foreach(uint id in from descriptor in pma0.PMADescriptors
                                   where descriptor.ADR == 2
                                   select (uint)((descriptor.Min << 16) + (descriptor.Sec << 8) + descriptor.Frame))
                    imageInfo.MediaSerialNumber = $"{id & 0x00FFFFFF:X6}";
            }

            if(atip != null)
            {
                ATIP.CDATIP atip0 = ATIP.Decode(atip).Value;

                imageInfo.MediaType = atip0.DiscType ? MediaType.CDRW : MediaType.CDR;

                if(atip0.LeadInStartMin == 97)
                {
                    int type = atip0.LeadInStartFrame % 10;
                    int frm = atip0.LeadInStartFrame - type;
                    imageInfo.MediaManufacturer = ATIP.ManufacturerFromATIP(atip0.LeadInStartSec, frm);
                }
            }

            bool isBd = false;
            if(imageInfo.MediaType == MediaType.BDR || imageInfo.MediaType == MediaType.BDRE ||
               imageInfo.MediaType == MediaType.BDROM)
            {
                isDvd = false;
                isBd = true;
            }

            if(isBd && imageInfo.Sectors > 24438784)
                switch(imageInfo.MediaType)
                {
                    case MediaType.BDR:
                        imageInfo.MediaType = MediaType.BDRXL;
                        break;
                    case MediaType.BDRE:
                        imageInfo.MediaType = MediaType.BDREXL;
                        break;
                }

            DicConsole.DebugWriteLine("BlindWrite5 plugin", "ImageInfo.mediaType = {0}", imageInfo.MediaType);

            if(mode2A != null) imageInfo.ReadableMediaTags.Add(MediaTagType.SCSI_MODEPAGE_2A);
            if(pma != null) imageInfo.ReadableMediaTags.Add(MediaTagType.CD_PMA);
            if(atip != null) imageInfo.ReadableMediaTags.Add(MediaTagType.CD_ATIP);
            if(cdtext != null) imageInfo.ReadableMediaTags.Add(MediaTagType.CD_TEXT);
            if(bca != null)
                if(isDvd) imageInfo.ReadableMediaTags.Add(MediaTagType.DVD_BCA);
                else if(isBd) imageInfo.ReadableMediaTags.Add(MediaTagType.BD_BCA);
            if(dmi != null) imageInfo.ReadableMediaTags.Add(MediaTagType.DVD_DMI);
            if(pfi != null) imageInfo.ReadableMediaTags.Add(MediaTagType.DVD_PFI);
            if(fullToc != null) imageInfo.ReadableMediaTags.Add(MediaTagType.CD_FullTOC);

            if(imageInfo.MediaType == MediaType.XGD2)
                if(imageInfo.Sectors == 25063 || // Locked (or non compatible drive)
                   imageInfo.Sectors == 4229664 || // Xtreme unlock
                   imageInfo.Sectors == 4246304) // Wxripper unlock
                    imageInfo.MediaType = MediaType.XGD3;

            DicConsole.VerboseWriteLine("BlindWrite image describes a disc of type {0}", imageInfo.MediaType);

            return true;
        }

        public virtual byte[] ReadDiskTag(MediaTagType tag)
        {
            switch(tag)
            {
                case MediaTagType.SCSI_MODEPAGE_2A:
                {
                    if(mode2A != null) return (byte[])mode2A.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain SCSI MODE PAGE 2Ah.");
                }
                case MediaTagType.CD_PMA:
                {
                    if(pma != null) return (byte[])pma.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain PMA information.");
                }
                case MediaTagType.CD_ATIP:
                {
                    if(atip != null) return (byte[])atip.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain ATIP information.");
                }
                case MediaTagType.CD_TEXT:
                {
                    if(cdtext != null) return (byte[])cdtext.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain CD-Text information.");
                }
                case MediaTagType.DVD_BCA:
                case MediaTagType.BD_BCA:
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

        public virtual byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public virtual byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            return ReadSectorsTag(sectorAddress, 1, tag);
        }

        public virtual byte[] ReadSector(ulong sectorAddress, uint track)
        {
            return ReadSectors(sectorAddress, 1, track);
        }

        public virtual byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            return ReadSectorsTag(sectorAddress, 1, track, tag);
        }

        public virtual byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in offsetmap
                                                     where sectorAddress >= kvp.Value
                                                     from track in tracks
                                                     where track.TrackSequence == kvp.Key
                                                     where sectorAddress - kvp.Value <
                                                           track.TrackEndSector - track.TrackStartSector
                                                     select kvp)
                return ReadSectors(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public virtual byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in offsetmap
                                                     where sectorAddress >= kvp.Value
                                                     from track in tracks
                                                     where track.TrackSequence == kvp.Key
                                                     where sectorAddress - kvp.Value <
                                                           track.TrackEndSector - track.TrackStartSector
                                                     select kvp)
                return ReadSectorsTag(sectorAddress - kvp.Value, length, kvp.Key, tag);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public virtual byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            // TODO: Cross data files
            Track dicTrack = new Track();
            DataFileCharacteristics chars = new DataFileCharacteristics();

            dicTrack.TrackSequence = 0;

            foreach(Track bwTrack in tracks.Where(bwTrack => bwTrack.TrackSequence == track))
            {
                dicTrack = bwTrack;
                break;
            }

            if(dicTrack.TrackSequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > dicTrack.TrackEndSector)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({dicTrack.TrackEndSector}), won't cross tracks");

            foreach(DataFileCharacteristics _chars in filePaths.Where(_chars =>
                                                                          (long)sectorAddress >= _chars.StartLba &&
                                                                          length < (ulong)_chars.Sectors -
                                                                          sectorAddress))
            {
                chars = _chars;
                break;
            }

            if(string.IsNullOrEmpty(chars.FilePath) || chars.FileFilter == null)
                throw new ArgumentOutOfRangeException(nameof(chars.FileFilter), "Track does not exist in disc image");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch(dicTrack.TrackType)
            {
                case TrackType.CdMode1:
                {
                    sectorOffset = 16;
                    sectorSize = 2048;
                    sectorSkip = 288;
                    break;
                }
                case TrackType.CdMode2Formless:
                {
                    sectorOffset = 16;
                    sectorSize = 2336;
                    sectorSkip = 0;
                    break;
                }
                case TrackType.CdMode2Form1:
                {
                    sectorOffset = 24;
                    sectorSize = 2048;
                    sectorSkip = 280;
                    break;
                }
                case TrackType.CdMode2Form2:
                {
                    sectorOffset = 24;
                    sectorSize = 2324;
                    sectorSkip = 4;
                    break;
                }
                case TrackType.Audio:
                {
                    sectorOffset = 0;
                    sectorSize = 2352;
                    sectorSkip = 0;
                    break;
                }
                case TrackType.Data:
                {
                    sectorOffset = 0;
                    sectorSize = 2048;
                    sectorSkip = 0;
                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            switch(chars.Subchannel)
            {
                case TrackSubchannelType.None:
                    sectorSkip += 0;
                    break;
                case TrackSubchannelType.Q16Interleaved:
                    sectorSkip += 16;
                    break;
                case TrackSubchannelType.PackedInterleaved:
                    sectorSkip += 96;
                    break;
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported subchannel type");
            }

            byte[] buffer = new byte[sectorSize * length];

            imageStream = chars.FileFilter.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);
            br.BaseStream
              .Seek((long)dicTrack.TrackFileOffset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
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

        public virtual byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            // TODO: Cross data files
            Track dicTrack = new Track();
            DataFileCharacteristics chars = new DataFileCharacteristics();

            dicTrack.TrackSequence = 0;

            foreach(Track bwTrack in tracks.Where(bwTrack => bwTrack.TrackSequence == track))
            {
                dicTrack = bwTrack;
                break;
            }

            if(dicTrack.TrackSequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > dicTrack.TrackEndSector)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({dicTrack.TrackEndSector}), won't cross tracks");

            foreach(DataFileCharacteristics _chars in filePaths.Where(_chars =>
                                                                          (long)sectorAddress >= _chars.StartLba &&
                                                                          length < (ulong)_chars.Sectors -
                                                                          sectorAddress))
            {
                chars = _chars;
                break;
            }

            if(string.IsNullOrEmpty(chars.FilePath) || chars.FileFilter == null)
                throw new ArgumentOutOfRangeException(nameof(chars.FileFilter), "Track does not exist in disc image");

            if(dicTrack.TrackType == TrackType.Data)
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
                    if(trackFlags.TryGetValue(track, out byte flag)) return new[] {flag};

                    throw new ArgumentException("Unsupported tag requested", nameof(tag));
                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
            }

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch(dicTrack.TrackType)
            {
                case TrackType.CdMode1:
                    switch(tag)
                    {
                        case SectorTagType.CdSectorSync:
                        {
                            sectorOffset = 0;
                            sectorSize = 12;
                            sectorSkip = 2340;
                            break;
                        }
                        case SectorTagType.CdSectorHeader:
                        {
                            sectorOffset = 12;
                            sectorSize = 4;
                            sectorSkip = 2336;
                            break;
                        }
                        case SectorTagType.CdSectorSubHeader:
                            throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                        case SectorTagType.CdSectorEcc:
                        {
                            sectorOffset = 2076;
                            sectorSize = 276;
                            sectorSkip = 0;
                            break;
                        }
                        case SectorTagType.CdSectorEccP:
                        {
                            sectorOffset = 2076;
                            sectorSize = 172;
                            sectorSkip = 104;
                            break;
                        }
                        case SectorTagType.CdSectorEccQ:
                        {
                            sectorOffset = 2248;
                            sectorSize = 104;
                            sectorSkip = 0;
                            break;
                        }
                        case SectorTagType.CdSectorEdc:
                        {
                            sectorOffset = 2064;
                            sectorSize = 4;
                            sectorSkip = 284;
                            break;
                        }
                        case SectorTagType.CdSectorSubchannel:
                            throw new NotImplementedException("Packed subchannel not yet supported");
                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                case TrackType.CdMode2Formless:
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
                            sectorSize = 8;
                            sectorSkip = 2328;
                            break;
                        }
                        case SectorTagType.CdSectorEdc:
                        {
                            sectorOffset = 2332;
                            sectorSize = 4;
                            sectorSkip = 0;
                            break;
                        }
                        case SectorTagType.CdSectorSubchannel:
                            throw new NotImplementedException("Packed subchannel not yet supported");
                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                }
                case TrackType.CdMode2Form1:
                    switch(tag)
                    {
                        case SectorTagType.CdSectorSync:
                        {
                            sectorOffset = 0;
                            sectorSize = 12;
                            sectorSkip = 2340;
                            break;
                        }
                        case SectorTagType.CdSectorHeader:
                        {
                            sectorOffset = 12;
                            sectorSize = 4;
                            sectorSkip = 2336;
                            break;
                        }
                        case SectorTagType.CdSectorSubHeader:
                        {
                            sectorOffset = 16;
                            sectorSize = 8;
                            sectorSkip = 2328;
                            break;
                        }
                        case SectorTagType.CdSectorEcc:
                        {
                            sectorOffset = 2076;
                            sectorSize = 276;
                            sectorSkip = 0;
                            break;
                        }
                        case SectorTagType.CdSectorEccP:
                        {
                            sectorOffset = 2076;
                            sectorSize = 172;
                            sectorSkip = 104;
                            break;
                        }
                        case SectorTagType.CdSectorEccQ:
                        {
                            sectorOffset = 2248;
                            sectorSize = 104;
                            sectorSkip = 0;
                            break;
                        }
                        case SectorTagType.CdSectorEdc:
                        {
                            sectorOffset = 2072;
                            sectorSize = 4;
                            sectorSkip = 276;
                            break;
                        }
                        case SectorTagType.CdSectorSubchannel:
                            throw new NotImplementedException("Packed subchannel not yet supported");
                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                case TrackType.CdMode2Form2:
                    switch(tag)
                    {
                        case SectorTagType.CdSectorSync:
                        {
                            sectorOffset = 0;
                            sectorSize = 12;
                            sectorSkip = 2340;
                            break;
                        }
                        case SectorTagType.CdSectorHeader:
                        {
                            sectorOffset = 12;
                            sectorSize = 4;
                            sectorSkip = 2336;
                            break;
                        }
                        case SectorTagType.CdSectorSubHeader:
                        {
                            sectorOffset = 16;
                            sectorSize = 8;
                            sectorSkip = 2328;
                            break;
                        }
                        case SectorTagType.CdSectorEdc:
                        {
                            sectorOffset = 2348;
                            sectorSize = 4;
                            sectorSkip = 0;
                            break;
                        }
                        case SectorTagType.CdSectorSubchannel:
                            throw new NotImplementedException("Packed subchannel not yet supported");
                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                case TrackType.Audio:
                {
                    switch(tag)
                    {
                        case SectorTagType.CdSectorSubchannel:
                            throw new NotImplementedException("Packed subchannel not yet supported");
                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            switch(chars.Subchannel)
            {
                case TrackSubchannelType.None:
                    sectorSkip += 0;
                    break;
                case TrackSubchannelType.Q16Interleaved:
                    sectorSkip += 16;
                    break;
                case TrackSubchannelType.PackedInterleaved:
                    sectorSkip += 96;
                    break;
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported subchannel type");
            }

            byte[] buffer = new byte[sectorSize * length];

            imageStream = dicTrack.TrackFilter.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);
            br.BaseStream
              .Seek((long)dicTrack.TrackFileOffset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
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

        public virtual byte[] ReadSectorLong(ulong sectorAddress)
        {
            return ReadSectorsLong(sectorAddress, 1);
        }

        public virtual byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            return ReadSectorsLong(sectorAddress, 1, track);
        }

        public virtual byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in offsetmap
                                                     where sectorAddress >= kvp.Value
                                                     from track in tracks
                                                     where track.TrackSequence == kvp.Key
                                                     where sectorAddress - kvp.Value <
                                                           track.TrackEndSector - track.TrackStartSector
                                                     select kvp)
                return ReadSectorsLong(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public virtual byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            // TODO: Cross data files
            Track dicTrack = new Track();
            DataFileCharacteristics chars = new DataFileCharacteristics();

            dicTrack.TrackSequence = 0;

            foreach(Track bwTrack in tracks.Where(bwTrack => bwTrack.TrackSequence == track))
            {
                dicTrack = bwTrack;
                break;
            }

            if(dicTrack.TrackSequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > dicTrack.TrackEndSector)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({dicTrack.TrackEndSector}), won't cross tracks");

            foreach(DataFileCharacteristics _chars in filePaths.Where(_chars =>
                                                                          (long)sectorAddress >= _chars.StartLba &&
                                                                          length < (ulong)_chars.Sectors -
                                                                          sectorAddress))
            {
                chars = _chars;
                break;
            }

            if(string.IsNullOrEmpty(chars.FilePath) || chars.FileFilter == null)
                throw new ArgumentOutOfRangeException(nameof(chars.FileFilter), "Track does not exist in disc image");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch(dicTrack.TrackType)
            {
                case TrackType.CdMode1:
                case TrackType.CdMode2Formless:
                case TrackType.CdMode2Form1:
                case TrackType.CdMode2Form2:
                case TrackType.Audio:
                {
                    sectorOffset = 0;
                    sectorSize = 2352;
                    sectorSkip = 0;
                    break;
                }
                case TrackType.Data:
                {
                    sectorOffset = 0;
                    sectorSize = 2048;
                    sectorSkip = 0;
                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            switch(chars.Subchannel)
            {
                case TrackSubchannelType.None:
                    sectorSkip += 0;
                    break;
                case TrackSubchannelType.Q16Interleaved:
                    sectorSkip += 16;
                    break;
                case TrackSubchannelType.PackedInterleaved:
                    sectorSkip += 96;
                    break;
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported subchannel type");
            }

            byte[] buffer = new byte[sectorSize * length];

            imageStream = dicTrack.TrackFilter.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);
            br.BaseStream
              .Seek((long)dicTrack.TrackFileOffset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
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

        public virtual List<Track> GetSessionTracks(Session session)
        {
            if(sessions.Contains(session)) return GetSessionTracks(session.SessionSequence);

            throw new ImageNotSupportedException("Session does not exist in disc image");
        }

        public virtual List<Track> GetSessionTracks(ushort session)
        {
            return tracks.Where(dicTrack => dicTrack.TrackSession == session).ToList();
        }

        public virtual bool? VerifySector(ulong sectorAddress)
        {
            byte[] buffer = ReadSectorLong(sectorAddress);
            return CdChecksums.CheckCdSector(buffer);
        }

        public virtual bool? VerifySector(ulong sectorAddress, uint track)
        {
            byte[] buffer = ReadSectorLong(sectorAddress, track);
            return CdChecksums.CheckCdSector(buffer);
        }

        public virtual bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
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

        public virtual bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
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

        public virtual bool? VerifyMediaImage()
        {
            return null;
        }

        static TrackType BlindWriteTrackTypeToTrackType(Bw5TrackType trackType)
        {
            switch(trackType)
            {
                case Bw5TrackType.Mode1: return TrackType.CdMode1;
                case Bw5TrackType.Mode2F1: return TrackType.CdMode2Form1;
                case Bw5TrackType.Mode2F2: return TrackType.CdMode2Form2;
                case Bw5TrackType.Mode2: return TrackType.CdMode2Formless;
                case Bw5TrackType.Audio: return TrackType.Audio;
                default: return TrackType.Data;
            }
        }

        static MediaType BlindWriteProfileToMediaType(ProfileNumber profile)
        {
            switch(profile)
            {
                case ProfileNumber.BDRE: return MediaType.BDRE;
                case ProfileNumber.BDROM: return MediaType.BDROM;
                case ProfileNumber.BDRRdm:
                case ProfileNumber.BDRSeq: return MediaType.BDR;
                case ProfileNumber.CDR:
                case ProfileNumber.HDBURNR: return MediaType.CDR;
                case ProfileNumber.CDROM:
                case ProfileNumber.HDBURNROM: return MediaType.CDROM;
                case ProfileNumber.CDRW:
                case ProfileNumber.HDBURNRW: return MediaType.CDRW;
                case ProfileNumber.DDCDR: return MediaType.DDCDR;
                case ProfileNumber.DDCDROM: return MediaType.DDCD;
                case ProfileNumber.DDCDRW: return MediaType.DDCDRW;
                case ProfileNumber.DVDDownload: return MediaType.DVDDownload;
                case ProfileNumber.DVDRAM: return MediaType.DVDRAM;
                case ProfileNumber.DVDRDLJump:
                case ProfileNumber.DVDRDLSeq: return MediaType.DVDRDL;
                case ProfileNumber.DVDRDLPlus: return MediaType.DVDPRDL;
                case ProfileNumber.DVDROM: return MediaType.DVDROM;
                case ProfileNumber.DVDRPlus: return MediaType.DVDPR;
                case ProfileNumber.DVDRSeq: return MediaType.DVDR;
                case ProfileNumber.DVDRWDL: return MediaType.DVDRWDL;
                case ProfileNumber.DVDRWDLPlus: return MediaType.DVDPRWDL;
                case ProfileNumber.DVDRWPlus: return MediaType.DVDPRW;
                case ProfileNumber.DVDRWRes:
                case ProfileNumber.DVDRWSeq: return MediaType.DVDRW;
                case ProfileNumber.HDDVDR: return MediaType.HDDVDR;
                case ProfileNumber.HDDVDRAM: return MediaType.HDDVDRAM;
                case ProfileNumber.HDDVDRDL: return MediaType.HDDVDRDL;
                case ProfileNumber.HDDVDROM: return MediaType.HDDVDROM;
                case ProfileNumber.HDDVDRW: return MediaType.HDDVDRW;
                case ProfileNumber.HDDVDRWDL: return MediaType.HDDVDRWDL;
                case ProfileNumber.ASMO:
                case ProfileNumber.MOErasable: return MediaType.UnknownMO;
                case ProfileNumber.NonRemovable: return MediaType.GENERIC_HDD;
                default: return MediaType.CD;
            }
        }

        enum Bw5TrackType : byte
        {
            NotData = 0,
            Audio = 1,
            Mode1 = 2,
            Mode2 = 3,
            Mode2F1 = 4,
            Mode2F2 = 5,
            Dvd = 6
        }

        enum Bw5TrackSubchannel : byte
        {
            None = 0,
            Q16 = 2,
            Linear = 4
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Bw5Header
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] signature;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public uint[] unknown1;
            public ProfileNumber profile;
            public ushort sessions;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public uint[] unknown2;
            [MarshalAs(UnmanagedType.U1, SizeConst = 3)] public bool mcnIsValid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)] public byte[] mcn;
            public ushort unknown3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public uint[] unknown4;
            public ushort pmaLen;
            public ushort atipLen;
            public ushort cdtLen;
            public ushort cdInfoLen;
            public uint bcaLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public uint[] unknown5;
            public uint dvdStrLen;
            public uint dvdInfoLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] unknown6;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] manufacturer;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] product;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] revision;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] public byte[] vendor;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] volumeId;
            public uint mode2ALen;
            public uint unkBlkLen;
            public uint dataLen;
            public uint sessionsLen;
            public uint dpmLen;
        }

        struct Bw5DataFile
        {
            public uint Type;
            public uint Length;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public uint[] Unknown1;
            public uint Offset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public uint[] Unknown2;
            public int StartLba;
            public int Sectors;
            public uint FilenameLen;
            public byte[] FilenameBytes;
            public uint Unknown3;

            public string Filename;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Bw5TrackDescriptor
        {
            public Bw5TrackType type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] unknown1;
            public uint unknown2;
            public Bw5TrackSubchannel subchannel;
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
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public uint[] unknown6;
            public int startLba;
            public int sectors;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public uint[] unknown7;
            public uint session;
            public ushort unknown8;
            // Seems to be only on non DVD track descriptors
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public uint[] unknown9;
        }

        struct Bw5SessionDescriptor
        {
            public ushort Sequence;
            public byte Entries;
            public byte Unknown;
            public int Start;
            public int End;
            public ushort FirstTrack;
            public ushort LastTrack;
            public Bw5TrackDescriptor[] Tracks;
        }

        struct DataFileCharacteristics
        {
            public IFilter FileFilter;
            public string FilePath;
            public TrackSubchannelType Subchannel;
            public long SectorSize;
            public int StartLba;
            public int Sectors;
        }
    }
}