// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads BlindWrite 5 disc images.
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.CommonTypes.Structs.Devices.SCSI.Modes;
using Aaru.Console;
using Aaru.Decoders.CD;
using Aaru.Decoders.DVD;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Helpers;
using DMI = Aaru.Decoders.Xbox.DMI;
using Session = Aaru.CommonTypes.Structs.Session;

namespace Aaru.DiscImages
{
    public partial class BlindWrite5
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 276)
                return false;

            byte[] hdr = new byte[260];
            stream.Read(hdr, 0, 260);
            header = Marshal.ByteArrayToStructureLittleEndian<Bw5Header>(hdr);

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.signature = {0}",
                                       StringHandlers.CToString(header.signature));

            for(int i = 0; i < header.unknown1.Length; i++)
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.unknown1[{1}] = 0x{0:X8}", header.unknown1[i],
                                           i);

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.profile = {0}", header.profile);
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.sessions = {0}", header.sessions);

            for(int i = 0; i < header.unknown2.Length; i++)
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.unknown2[{1}] = 0x{0:X8}", header.unknown2[i],
                                           i);

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.mcnIsValid = {0}", header.mcnIsValid);
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.mcn = {0}", StringHandlers.CToString(header.mcn));
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.unknown3 = 0x{0:X4}", header.unknown3);

            for(int i = 0; i < header.unknown4.Length; i++)
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.unknown4[{1}] = 0x{0:X8}", header.unknown4[i],
                                           i);

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.pmaLen = {0}", header.pmaLen);
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.atipLen = {0}", header.atipLen);
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.cdtLen = {0}", header.cdtLen);
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.cdInfoLen = {0}", header.cdInfoLen);
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.bcaLen = {0}", header.bcaLen);

            for(int i = 0; i < header.unknown5.Length; i++)
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.unknown5[{1}] = 0x{0:X8}", header.unknown5[i],
                                           i);

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.dvdStrLen = {0}", header.dvdStrLen);
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.dvdInfoLen = {0}", header.dvdInfoLen);

            for(int i = 0; i < header.unknown6.Length; i++)
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.unknown6[{1}] = 0x{0:X2}", header.unknown6[i],
                                           i);

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.manufacturer = {0}",
                                       StringHandlers.CToString(header.manufacturer));

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.product = {0}",
                                       StringHandlers.CToString(header.product));

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.revision = {0}",
                                       StringHandlers.CToString(header.revision));

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.vendor = {0}",
                                       StringHandlers.CToString(header.vendor));

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.volumeId = {0}",
                                       StringHandlers.CToString(header.volumeId));

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.mode2ALen = {0}", header.mode2ALen);
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.unkBlkLen = {0}", header.unkBlkLen);
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.dataLen = {0}", header.dataLen);
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.sessionsLen = {0}", header.sessionsLen);
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.dpmLen = {0}", header.dpmLen);

            mode2A = new byte[header.mode2ALen];

            if(mode2A.Length > 0)
            {
                stream.Read(mode2A, 0, mode2A.Length);
                mode2A[1] -= 2;
                var decoded2A = ModePage_2A.Decode(mode2A);

                if(!(decoded2A is null))
                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "mode page 2A: {0}",
                                               Modes.PrettifyModePage_2A(decoded2A));
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
                pma    = new byte[temp.Length + 4];
                pma[0] = tushort[1];
                pma[1] = tushort[0];
                Array.Copy(temp, 0, pma, 4, temp.Length);

                PMA.CDPMA? decodedPma = PMA.Decode(pma);

                if(decodedPma.HasValue)
                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "PMA: {0}", PMA.Prettify(decodedPma));
                else
                    pma = null;
            }

            atip = new byte[header.atipLen];

            if(atip.Length > 0)
                stream.Read(atip, 0, atip.Length);
            else
                atip = null;

            cdtext = new byte[header.cdtLen];

            if(cdtext.Length > 0)
                stream.Read(cdtext, 0, cdtext.Length);
            else
                cdtext = null;

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

                // TODO: CMI
                Array.Copy(temp, 2, dmi, 4, 2048);
                Array.Copy(temp, 0x802, pfi, 4, 2048);

                pfi[0] = 0x08;
                pfi[1] = 0x02;
                dmi[0] = 0x08;
                dmi[1] = 0x02;

                PFI.PhysicalFormatInformation? decodedPfi = PFI.Decode(pfi);

                if(decodedPfi.HasValue)
                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "PFI: {0}", PFI.Prettify(decodedPfi));
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

                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "Disc information: {0}",
                                           PrintHex.ByteArrayToHexArrayString(discInformation, 40));
            }
            else
                discInformation = null;

            // How many data blocks
            byte[] tmpArray = new byte[4];
            stream.Read(tmpArray, 0, tmpArray.Length);
            uint dataBlockCount = BitConverter.ToUInt32(tmpArray, 0);

            stream.Read(tmpArray, 0, tmpArray.Length);
            uint   dataPathLen   = BitConverter.ToUInt32(tmpArray, 0);
            byte[] dataPathBytes = new byte[dataPathLen];
            stream.Read(dataPathBytes, 0, dataPathBytes.Length);
            dataPath = Encoding.Unicode.GetString(dataPathBytes);
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "Data path: {0}", dataPath);

            dataFiles = new List<Bw5DataFile>();

            for(int cD = 0; cD < dataBlockCount; cD++)
            {
                tmpArray = new byte[52];

                var dataFile = new Bw5DataFile
                {
                    Unknown1 = new uint[4], Unknown2 = new uint[3]
                };

                stream.Read(tmpArray, 0, tmpArray.Length);
                dataFile.Type          = BitConverter.ToUInt32(tmpArray, 0);
                dataFile.Length        = BitConverter.ToUInt32(tmpArray, 4);
                dataFile.Unknown1[0]   = BitConverter.ToUInt32(tmpArray, 8);
                dataFile.Unknown1[1]   = BitConverter.ToUInt32(tmpArray, 12);
                dataFile.Unknown1[2]   = BitConverter.ToUInt32(tmpArray, 16);
                dataFile.Unknown1[3]   = BitConverter.ToUInt32(tmpArray, 20);
                dataFile.Offset        = BitConverter.ToUInt32(tmpArray, 24);
                dataFile.Unknown2[0]   = BitConverter.ToUInt32(tmpArray, 28);
                dataFile.Unknown2[1]   = BitConverter.ToUInt32(tmpArray, 32);
                dataFile.Unknown2[2]   = BitConverter.ToUInt32(tmpArray, 36);
                dataFile.StartLba      = BitConverter.ToInt32(tmpArray, 40);
                dataFile.Sectors       = BitConverter.ToInt32(tmpArray, 44);
                dataFile.FilenameLen   = BitConverter.ToUInt32(tmpArray, 48);
                dataFile.FilenameBytes = new byte[dataFile.FilenameLen];

                tmpArray = new byte[dataFile.FilenameLen];
                stream.Read(tmpArray, 0, tmpArray.Length);
                dataFile.FilenameBytes = tmpArray;
                tmpArray               = new byte[4];
                stream.Read(tmpArray, 0, tmpArray.Length);
                dataFile.Unknown3 = BitConverter.ToUInt32(tmpArray, 0);

                dataFile.Filename = Encoding.Unicode.GetString(dataFile.FilenameBytes);
                dataFiles.Add(dataFile);

                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.type = 0x{0:X8}", dataFile.Type);
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.length = {0}", dataFile.Length);

                for(int i = 0; i < dataFile.Unknown1.Length; i++)
                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.unknown1[{1}] = {0}",
                                               dataFile.Unknown1[i], i);

                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.offset = {0}", dataFile.Offset);

                for(int i = 0; i < dataFile.Unknown2.Length; i++)
                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.unknown2[{1}] = {0}",
                                               dataFile.Unknown2[i], i);

                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.startLba = {0}", dataFile.StartLba);
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.sectors = {0}", dataFile.Sectors);
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.filenameLen = {0}", dataFile.FilenameLen);
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.filename = {0}", dataFile.Filename);
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "dataFile.unknown3 = {0}", dataFile.Unknown3);
            }

            bwSessions = new List<Bw5SessionDescriptor>();

            for(int ses = 0; ses < header.sessions; ses++)
            {
                var session = new Bw5SessionDescriptor();
                tmpArray = new byte[16];
                stream.Read(tmpArray, 0, tmpArray.Length);
                session.Sequence   = BitConverter.ToUInt16(tmpArray, 0);
                session.Entries    = tmpArray[2];
                session.Unknown    = tmpArray[3];
                session.Start      = BitConverter.ToInt32(tmpArray, 4);
                session.End        = BitConverter.ToInt32(tmpArray, 8);
                session.FirstTrack = BitConverter.ToUInt16(tmpArray, 12);
                session.LastTrack  = BitConverter.ToUInt16(tmpArray, 14);
                session.Tracks     = new Bw5TrackDescriptor[session.Entries];

                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].filename = {1}", ses, session.Sequence);
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].entries = {1}", ses, session.Entries);
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].unknown = {1}", ses, session.Unknown);
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].start = {1}", ses, session.Start);
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].end = {1}", ses, session.End);

                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].firstTrack = {1}", ses,
                                           session.FirstTrack);

                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].lastTrack = {1}", ses,
                                           session.LastTrack);

                for(int tSeq = 0; tSeq < session.Entries; tSeq++)
                {
                    byte[] trk = new byte[72];
                    stream.Read(trk, 0, 72);
                    session.Tracks[tSeq] = Marshal.ByteArrayToStructureLittleEndian<Bw5TrackDescriptor>(trk);

                    if(session.Tracks[tSeq].type == Bw5TrackType.Dvd ||
                       session.Tracks[tSeq].type == Bw5TrackType.NotData)
                    {
                        session.Tracks[tSeq].unknown9[0] = 0;
                        session.Tracks[tSeq].unknown9[1] = 0;
                        stream.Seek(-8, SeekOrigin.Current);
                    }

                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].type = {2}", ses, tSeq,
                                               session.Tracks[tSeq].type);

                    for(int i = 0; i < session.Tracks[tSeq].unknown1.Length; i++)
                        AaruConsole.DebugWriteLine("BlindWrite5 plugin",
                                                   "session[{0}].track[{1}].unknown1[{2}] = 0x{3:X2}", ses, tSeq, i,
                                                   session.Tracks[tSeq].unknown1[i]);

                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].unknown2 = 0x{2:X8}", ses,
                                               tSeq, session.Tracks[tSeq].unknown2);

                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].subchannel = {2}", ses,
                                               tSeq, session.Tracks[tSeq].subchannel);

                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].unknown3 = 0x{2:X2}", ses,
                                               tSeq, session.Tracks[tSeq].unknown3);

                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].ctl = {2}", ses, tSeq,
                                               session.Tracks[tSeq].ctl);

                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].adr = {2}", ses, tSeq,
                                               session.Tracks[tSeq].adr);

                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].point = {2}", ses, tSeq,
                                               session.Tracks[tSeq].point);

                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].unknown4 = 0x{2:X2}", ses,
                                               tSeq, session.Tracks[tSeq].tno);

                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].min = {2}", ses, tSeq,
                                               session.Tracks[tSeq].min);

                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].sec = {2}", ses, tSeq,
                                               session.Tracks[tSeq].sec);

                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].frame = {2}", ses, tSeq,
                                               session.Tracks[tSeq].frame);

                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].zero = {2}", ses, tSeq,
                                               session.Tracks[tSeq].zero);

                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].pmin = {2}", ses, tSeq,
                                               session.Tracks[tSeq].pmin);

                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].psec = {2}", ses, tSeq,
                                               session.Tracks[tSeq].psec);

                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].pframe = {2}", ses, tSeq,
                                               session.Tracks[tSeq].pframe);

                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].unknown5 = 0x{2:X2}", ses,
                                               tSeq, session.Tracks[tSeq].unknown5);

                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].pregap = {2}", ses, tSeq,
                                               session.Tracks[tSeq].pregap);

                    for(int i = 0; i < session.Tracks[tSeq].unknown6.Length; i++)
                        AaruConsole.DebugWriteLine("BlindWrite5 plugin",
                                                   "session[{0}].track[{1}].unknown6[{2}] = 0x{3:X8}", ses, tSeq, i,
                                                   session.Tracks[tSeq].unknown6[i]);

                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].startLba = {2}", ses,
                                               tSeq, session.Tracks[tSeq].startLba);

                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].sectors = {2}", ses, tSeq,
                                               session.Tracks[tSeq].sectors);

                    for(int i = 0; i < session.Tracks[tSeq].unknown7.Length; i++)
                        AaruConsole.DebugWriteLine("BlindWrite5 plugin",
                                                   "session[{0}].track[{1}].unknown7[{2}] = 0x{3:X8}", ses, tSeq, i,
                                                   session.Tracks[tSeq].unknown7[i]);

                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].session = {2}", ses, tSeq,
                                               session.Tracks[tSeq].session);

                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "session[{0}].track[{1}].unknown8 = 0x{2:X4}", ses,
                                               tSeq, session.Tracks[tSeq].unknown8);

                    if(session.Tracks[tSeq].type == Bw5TrackType.Dvd ||
                       session.Tracks[tSeq].type == Bw5TrackType.NotData)
                        continue;

                    {
                        for(int i = 0; i < session.Tracks[tSeq].unknown9.Length; i++)
                            AaruConsole.DebugWriteLine("BlindWrite5 plugin",
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
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "Correctly arrived end of image");
            else
                AaruConsole.
                    ErrorWriteLine("BlindWrite5 image ends after expected position. Probably new version with different data. Errors may occur.");

            filePaths = new List<DataFileCharacteristics>();

            foreach(Bw5DataFile dataFile in dataFiles)
            {
                var    chars       = new DataFileCharacteristics();
                string path        = Path.Combine(dataPath, dataFile.Filename);
                var    filtersList = new FiltersList();

                if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path)) != null)
                {
                    chars.FileFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path));
                    chars.FilePath   = path;
                }
                else
                {
                    path = Path.Combine(dataPath, dataFile.Filename.ToLower(CultureInfo.CurrentCulture));

                    if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path)) != null)
                    {
                        chars.FileFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path));
                        chars.FilePath   = path;
                    }
                    else
                    {
                        path = Path.Combine(dataPath, dataFile.Filename.ToUpper(CultureInfo.CurrentCulture));

                        if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path)) != null)
                        {
                            chars.FileFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path));
                            chars.FilePath   = path;
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
                                                                               path.ToLower(CultureInfo.
                                                                                                CurrentCulture)));
                                    }
                                    else if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                               path.ToUpper(CultureInfo.
                                                                                                CurrentCulture))) !=
                                            null)
                                    {
                                        chars.FilePath = path.ToUpper(CultureInfo.CurrentCulture);

                                        chars.FileFilter =
                                            filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                               path.ToUpper(CultureInfo.
                                                                                                CurrentCulture)));
                                    }
                                    else if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                               dataFile.Filename.ToLower(CultureInfo.
                                                                                                             CurrentCulture))) !=
                                            null)
                                    {
                                        chars.FilePath = dataFile.Filename.ToLower(CultureInfo.CurrentCulture);

                                        chars.FileFilter =
                                            filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                               dataFile.Filename.ToLower(CultureInfo.
                                                                                                             CurrentCulture)));
                                    }
                                    else if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                               dataFile.Filename.ToUpper(CultureInfo.
                                                                                                             CurrentCulture))) !=
                                            null)
                                    {
                                        chars.FilePath = dataFile.Filename.ToUpper(CultureInfo.CurrentCulture);

                                        chars.FileFilter =
                                            filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                               dataFile.Filename.ToUpper(CultureInfo.
                                                                                                             CurrentCulture)));
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
                                        AaruConsole.ErrorWriteLine("Cannot find data file {0}", dataFile.Filename);

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
                            AaruConsole.ErrorWriteLine("BlindWrite5 found unknown subchannel size: {0}",
                                                       sectorSize - 2352);

                            return false;
                    }
                else
                    chars.Subchannel = TrackSubchannelType.None;

                chars.SectorSize = sectorSize;
                chars.StartLba   = dataFile.StartLba;
                chars.Sectors    = dataFile.Sectors;

                filePaths.Add(chars);
            }

            Sessions   = new List<Session>();
            Tracks     = new List<Track>();
            Partitions = new List<Partition>();
            var fullTocStream = new MemoryStream();

            fullTocStream.Write(new byte[]
            {
                0, 0
            }, 0, 2);

            ulong offsetBytes = 0;
            offsetmap = new Dictionary<uint, ulong>();
            bool isDvd        = false;
            byte firstSession = byte.MaxValue;
            byte lastSession  = 0;
            trackFlags        = new Dictionary<uint, byte>();
            imageInfo.Sectors = 0;

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "Building maps");

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

                if(ses.Sequence < firstSession)
                    firstSession = (byte)ses.Sequence;

                if(ses.Sequence > lastSession)
                    lastSession = (byte)ses.Sequence;

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

                    if(trk.point >= 0xA0)
                        continue;

                    var track     = new Track();
                    var partition = new Partition();

                    trackFlags.Add(trk.point, trk.ctl);

                    switch(trk.type)
                    {
                        case Bw5TrackType.Audio:
                            track.TrackBytesPerSector    = 2352;
                            track.TrackRawBytesPerSector = 2352;

                            if(imageInfo.SectorSize < 2352)
                                imageInfo.SectorSize = 2352;

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

                            track.TrackBytesPerSector    = 2048;
                            track.TrackRawBytesPerSector = 2352;

                            if(imageInfo.SectorSize < 2048)
                                imageInfo.SectorSize = 2048;

                            break;
                        case Bw5TrackType.Mode2:
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                            track.TrackBytesPerSector    = 2336;
                            track.TrackRawBytesPerSector = 2352;

                            if(imageInfo.SectorSize < 2336)
                                imageInfo.SectorSize = 2336;

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

                            track.TrackBytesPerSector    = 2336;
                            track.TrackRawBytesPerSector = 2352;

                            if(imageInfo.SectorSize < 2324)
                                imageInfo.SectorSize = 2324;

                            break;
                        case Bw5TrackType.Dvd:
                            track.TrackBytesPerSector    = 2048;
                            track.TrackRawBytesPerSector = 2048;

                            if(imageInfo.SectorSize < 2048)
                                imageInfo.SectorSize = 2048;

                            isDvd = true;

                            break;
                    }

                    track.TrackDescription = $"Track {trk.point}";
                    track.TrackStartSector = (ulong)(trk.startLba + trk.pregap);
                    track.TrackEndSector   = (ulong)(trk.sectors  + trk.startLba);

                    foreach(DataFileCharacteristics chars in filePaths.Where(chars => trk.startLba >= chars.StartLba &&
                                                                                      trk.startLba   + trk.sectors <=
                                                                                      chars.StartLba + chars.Sectors))
                    {
                        track.TrackFilter = chars.FileFilter;
                        track.TrackFile   = chars.FileFilter.GetFilename();

                        if(trk.startLba >= 0)
                            track.TrackFileOffset = (ulong)((trk.startLba - chars.StartLba) * chars.SectorSize);
                        else
                            track.TrackFileOffset = (ulong)(trk.startLba * -1 * chars.SectorSize);

                        track.TrackFileType = "BINARY";

                        if(chars.Subchannel != TrackSubchannelType.None)
                        {
                            track.TrackSubchannelFilter = track.TrackFilter;
                            track.TrackSubchannelFile   = track.TrackFile;
                            track.TrackSubchannelType   = chars.Subchannel;
                            track.TrackSubchannelOffset = track.TrackFileOffset;

                            if(chars.Subchannel == TrackSubchannelType.PackedInterleaved)
                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);
                        }

                        break;
                    }

                    track.TrackPregap   = trk.pregap;
                    track.TrackSequence = trk.point;
                    track.TrackType     = BlindWriteTrackTypeToTrackType(trk.type);

                    track.Indexes = new Dictionary<int, ulong>
                    {
                        {
                            1, track.TrackStartSector
                        }
                    };

                    partition.Description = track.TrackDescription;

                    partition.Size = (track.TrackEndSector - track.TrackStartSector) *
                                     (ulong)track.TrackRawBytesPerSector;

                    partition.Length   = track.TrackEndSector - track.TrackStartSector;
                    partition.Sequence = track.TrackSequence;
                    partition.Offset   = offsetBytes;
                    partition.Start    = track.TrackStartSector;
                    partition.Type     = track.TrackType.ToString();

                    offsetBytes += partition.Size;

                    Tracks.Add(track);
                    Partitions.Add(partition);
                    offsetmap.Add(track.TrackSequence, track.TrackStartSector);
                    imageInfo.Sectors += partition.Length;
                }
            }

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "printing track map");

            foreach(Track track in Tracks)
            {
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "Partition sequence: {0}", track.TrackSequence);

                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition description: {0}",
                                           track.TrackDescription);

                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition type: {0}", track.TrackType);

                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition starting sector: {0}",
                                           track.TrackStartSector);

                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition ending sector: {0}",
                                           track.TrackEndSector);
            }

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "printing partition map");

            foreach(Partition partition in Partitions)
            {
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "Partition sequence: {0}", partition.Sequence);
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition name: {0}", partition.Name);
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition description: {0}", partition.Description);
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition type: {0}", partition.Type);
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition starting sector: {0}", partition.Start);
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition sectors: {0}", partition.Length);
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition starting offset: {0}", partition.Offset);
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "\tPartition size in bytes: {0}", partition.Size);
            }

            if(!isDvd)
            {
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "Rebuilding TOC");

                fullToc = fullTocStream.ToArray();
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "TOC len {0}", fullToc.Length);

                fullToc[0] = firstSession;
                fullToc[1] = lastSession;

                imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackFlags);
            }

            imageInfo.MediaType = BlindWriteProfileToMediaType(header.profile);

            if(dmi != null &&
               pfi != null)
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

                    if(DMI.IsXbox(dmi))
                        imageInfo.MediaType = MediaType.XGD;
                    else if(DMI.IsXbox360(dmi))
                        imageInfo.MediaType = MediaType.XGD2;
                }
            }
            else if(imageInfo.MediaType == MediaType.CD ||
                    imageInfo.MediaType == MediaType.CDROM)
            {
                bool data       = false;
                bool mode2      = false;
                bool firstaudio = false;
                bool firstdata  = false;
                bool audio      = false;

                foreach(Track bwTrack in Tracks)
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

            imageInfo.DriveManufacturer     = StringHandlers.CToString(header.manufacturer);
            imageInfo.DriveModel            = StringHandlers.CToString(header.product);
            imageInfo.DriveFirmwareRevision = StringHandlers.CToString(header.revision);
            imageInfo.Application           = "BlindWrite";

            if(string.Compare(Path.GetExtension(imageFilter.GetFilename()), "B5T",
                              StringComparison.OrdinalIgnoreCase) == 0)
                imageInfo.ApplicationVersion = "5";
            else if(string.Compare(Path.GetExtension(imageFilter.GetFilename()), "B6T",
                                   StringComparison.OrdinalIgnoreCase) == 0)
                imageInfo.ApplicationVersion = "6";

            imageInfo.Version = "5";

            imageInfo.ImageSize            = (ulong)imageFilter.GetDataForkLength();
            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.XmlMediaType         = XmlMediaType.OpticalDisc;

            if(pma != null)
            {
                PMA.CDPMA pma0 = PMA.Decode(pma).Value;

                foreach(uint id in from descriptor in pma0.PMADescriptors where descriptor.ADR == 2
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
                    int frm  = atip0.LeadInStartFrame - type;
                    imageInfo.MediaManufacturer = ATIP.ManufacturerFromATIP(atip0.LeadInStartSec, frm);
                }
            }

            bool isBd = false;

            if(imageInfo.MediaType == MediaType.BDR  ||
               imageInfo.MediaType == MediaType.BDRE ||
               imageInfo.MediaType == MediaType.BDROM)
            {
                isDvd = false;
                isBd  = true;
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

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "ImageInfo.mediaType = {0}", imageInfo.MediaType);

            if(mode2A != null)
                imageInfo.ReadableMediaTags.Add(MediaTagType.SCSI_MODEPAGE_2A);

            if(pma != null)
                imageInfo.ReadableMediaTags.Add(MediaTagType.CD_PMA);

            if(atip != null)
                imageInfo.ReadableMediaTags.Add(MediaTagType.CD_ATIP);

            if(cdtext != null)
                imageInfo.ReadableMediaTags.Add(MediaTagType.CD_TEXT);

            if(bca != null)
                if(isDvd)
                    imageInfo.ReadableMediaTags.Add(MediaTagType.DVD_BCA);
                else if(isBd)
                    imageInfo.ReadableMediaTags.Add(MediaTagType.BD_BCA);

            byte[] tmp;

            if(dmi != null)
            {
                tmp = new byte[2048];
                Array.Copy(dmi, 4, tmp, 0, 2048);
                dmi = tmp;
                imageInfo.ReadableMediaTags.Add(MediaTagType.DVD_DMI);
            }

            if(pfi != null)
            {
                tmp = new byte[2048];
                Array.Copy(pfi, 4, tmp, 0, 2048);
                pfi = tmp;
                imageInfo.ReadableMediaTags.Add(MediaTagType.DVD_PFI);
            }

            if(fullToc != null)
                imageInfo.ReadableMediaTags.Add(MediaTagType.CD_FullTOC);

            if(imageInfo.MediaType == MediaType.XGD2)
                if(imageInfo.Sectors == 25063   || // Locked (or non compatible drive)
                   imageInfo.Sectors == 4229664 || // Xtreme unlock
                   imageInfo.Sectors == 4246304)   // Wxripper unlock
                    imageInfo.MediaType = MediaType.XGD3;

            AaruConsole.VerboseWriteLine("BlindWrite image describes a disc of type {0}", imageInfo.MediaType);

            return true;
        }

        public byte[] ReadDiskTag(MediaTagType tag)
        {
            switch(tag)
            {
                case MediaTagType.SCSI_MODEPAGE_2A:
                {
                    if(mode2A != null)
                        return (byte[])mode2A.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain SCSI MODE PAGE 2Ah.");
                }
                case MediaTagType.CD_PMA:
                {
                    if(pma != null)
                        return (byte[])pma.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain PMA information.");
                }
                case MediaTagType.CD_ATIP:
                {
                    if(atip != null)
                        return (byte[])atip.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain ATIP information.");
                }
                case MediaTagType.CD_TEXT:
                {
                    if(cdtext != null)
                        return (byte[])cdtext.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain CD-Text information.");
                }
                case MediaTagType.DVD_BCA:
                case MediaTagType.BD_BCA:
                {
                    if(bca != null)
                        return (byte[])bca.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain BCA information.");
                }
                case MediaTagType.DVD_PFI:
                {
                    if(pfi != null)
                        return (byte[])pfi.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain PFI.");
                }
                case MediaTagType.DVD_DMI:
                {
                    if(dmi != null)
                        return (byte[])dmi.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain DMI.");
                }
                case MediaTagType.CD_FullTOC:
                {
                    if(fullToc != null)
                        return (byte[])fullToc.Clone();

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
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in offsetmap where sectorAddress      >= kvp.Value
                                                     from track in Tracks where track.TrackSequence == kvp.Key
                                                     where sectorAddress        - kvp.Value <
                                                           track.TrackEndSector - track.TrackStartSector select kvp)
                return ReadSectors(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in offsetmap where sectorAddress      >= kvp.Value
                                                     from track in Tracks where track.TrackSequence == kvp.Key
                                                     where sectorAddress        - kvp.Value <
                                                           track.TrackEndSector - track.TrackStartSector select kvp)
                return ReadSectorsTag(sectorAddress - kvp.Value, length, kvp.Key, tag);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            // TODO: Cross data files
            var aaruTrack = new Track();
            var chars     = new DataFileCharacteristics();

            aaruTrack.TrackSequence = 0;

            foreach(Track bwTrack in Tracks.Where(bwTrack => bwTrack.TrackSequence == track))
            {
                aaruTrack = bwTrack;

                break;
            }

            if(aaruTrack.TrackSequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > aaruTrack.TrackEndSector)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({aaruTrack.TrackEndSector}), won't cross tracks");

            foreach(DataFileCharacteristics characteristics in filePaths.Where(characteristics =>
                                                                                   (long)sectorAddress >=
                                                                                   characteristics.StartLba &&
                                                                                   length < (ulong)characteristics.
                                                                                       Sectors - sectorAddress))
            {
                chars = characteristics;

                break;
            }

            if(string.IsNullOrEmpty(chars.FilePath) ||
               chars.FileFilter == null)
                throw new ArgumentOutOfRangeException(nameof(chars.FileFilter), "Track does not exist in disc image");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;
            bool mode2 = false;

            switch(aaruTrack.TrackType)
            {
                case TrackType.CdMode1:
                {
                    sectorOffset = 16;
                    sectorSize   = 2048;
                    sectorSkip   = 288;

                    break;
                }
                case TrackType.CdMode2Formless:
                case TrackType.CdMode2Form1:
                case TrackType.CdMode2Form2:
                {
                    mode2        = true;
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 0;

                    break;
                }
                case TrackType.Audio:
                {
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 0;

                    break;
                }
                case TrackType.Data:
                {
                    sectorOffset = 0;
                    sectorSize   = 2048;
                    sectorSkip   = 0;

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
            var br = new BinaryReader(imageStream);

            br.BaseStream.
               Seek((long)aaruTrack.TrackFileOffset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
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
            // TODO: Cross data files
            var aaruTrack = new Track();
            var chars     = new DataFileCharacteristics();

            aaruTrack.TrackSequence = 0;

            foreach(Track bwTrack in Tracks.Where(bwTrack => bwTrack.TrackSequence == track))
            {
                aaruTrack = bwTrack;

                break;
            }

            if(aaruTrack.TrackSequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > aaruTrack.TrackEndSector)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({aaruTrack.TrackEndSector}), won't cross tracks");

            foreach(DataFileCharacteristics characteristics in filePaths.Where(characteristics =>
                                                                                   (long)sectorAddress >=
                                                                                   characteristics.StartLba &&
                                                                                   length < (ulong)characteristics.
                                                                                       Sectors - sectorAddress))
            {
                chars = characteristics;

                break;
            }

            if(string.IsNullOrEmpty(chars.FilePath) ||
               chars.FileFilter == null)
                throw new ArgumentOutOfRangeException(nameof(chars.FileFilter), "Track does not exist in disc image");

            if(aaruTrack.TrackType == TrackType.Data)
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
                    if(trackFlags.TryGetValue(track, out byte flag))
                        return new[]
                        {
                            flag
                        };

                    throw new ArgumentException("Unsupported tag requested", nameof(tag));
                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
            }

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch(aaruTrack.TrackType)
            {
                case TrackType.CdMode1:
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

            imageStream = aaruTrack.TrackFilter.GetDataForkStream();
            var br = new BinaryReader(imageStream);

            br.BaseStream.
               Seek((long)aaruTrack.TrackFileOffset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
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
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in offsetmap where sectorAddress      >= kvp.Value
                                                     from track in Tracks where track.TrackSequence == kvp.Key
                                                     where sectorAddress        - kvp.Value <
                                                           track.TrackEndSector - track.TrackStartSector select kvp)
                return ReadSectorsLong(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            // TODO: Cross data files
            var aaruTrack = new Track();
            var chars     = new DataFileCharacteristics();

            aaruTrack.TrackSequence = 0;

            foreach(Track bwTrack in Tracks.Where(bwTrack => bwTrack.TrackSequence == track))
            {
                aaruTrack = bwTrack;

                break;
            }

            if(aaruTrack.TrackSequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > aaruTrack.TrackEndSector)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({aaruTrack.TrackEndSector}), won't cross tracks");

            foreach(DataFileCharacteristics characteristics in filePaths.Where(characteristics =>
                                                                                   (long)sectorAddress >=
                                                                                   characteristics.StartLba &&
                                                                                   length < (ulong)characteristics.
                                                                                       Sectors - sectorAddress))
            {
                chars = characteristics;

                break;
            }

            if(string.IsNullOrEmpty(chars.FilePath) ||
               chars.FileFilter == null)
                throw new ArgumentOutOfRangeException(nameof(chars.FileFilter), "Track does not exist in disc image");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch(aaruTrack.TrackType)
            {
                case TrackType.CdMode1:
                case TrackType.CdMode2Formless:
                case TrackType.CdMode2Form1:
                case TrackType.CdMode2Form2:
                case TrackType.Audio:
                {
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 0;

                    break;
                }
                case TrackType.Data:
                {
                    sectorOffset = 0;
                    sectorSize   = 2048;
                    sectorSkip   = 0;

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

            imageStream = aaruTrack.TrackFilter.GetDataForkStream();
            var br = new BinaryReader(imageStream);

            br.BaseStream.
               Seek((long)aaruTrack.TrackFileOffset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
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

        public List<Track> GetSessionTracks(ushort session) =>
            Tracks.Where(aaruTrack => aaruTrack.TrackSession == session).ToList();
    }
}