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
// Copyright © 2011-2023 Natalia Portillo
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
using Aaru.Filters;
using Aaru.Helpers;
using DMI = Aaru.Decoders.Xbox.DMI;
using Session = Aaru.CommonTypes.Structs.Session;

namespace Aaru.DiscImages
{
    public sealed partial class BlindWrite5
    {
        /// <inheritdoc />
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 276)
                return false;

            byte[] hdr = new byte[260];
            stream.Read(hdr, 0, 260);
            _header = Marshal.ByteArrayToStructureLittleEndian<Header>(hdr);

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.signature = {0}",
                                       StringHandlers.CToString(_header.signature));

            for(int i = 0; i < _header.unknown1.Length; i++)
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.unknown1[{1}] = 0x{0:X8}", _header.unknown1[i],
                                           i);

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.profile = {0}", _header.profile);
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.sessions = {0}", _header.sessions);

            for(int i = 0; i < _header.unknown2.Length; i++)
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.unknown2[{1}] = 0x{0:X8}", _header.unknown2[i],
                                           i);

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.mcnIsValid = {0}", _header.mcnIsValid);
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.mcn = {0}", StringHandlers.CToString(_header.mcn));
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.unknown3 = 0x{0:X4}", _header.unknown3);

            for(int i = 0; i < _header.unknown4.Length; i++)
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.unknown4[{1}] = 0x{0:X8}", _header.unknown4[i],
                                           i);

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.pmaLen = {0}", _header.pmaLen);
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.atipLen = {0}", _header.atipLen);
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.cdtLen = {0}", _header.cdtLen);
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.cdInfoLen = {0}", _header.cdInfoLen);
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.bcaLen = {0}", _header.bcaLen);

            for(int i = 0; i < _header.unknown5.Length; i++)
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.unknown5[{1}] = 0x{0:X8}", _header.unknown5[i],
                                           i);

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.dvdStrLen = {0}", _header.dvdStrLen);
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.dvdInfoLen = {0}", _header.dvdInfoLen);

            for(int i = 0; i < _header.unknown6.Length; i++)
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.unknown6[{1}] = 0x{0:X2}", _header.unknown6[i],
                                           i);

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.manufacturer = {0}",
                                       StringHandlers.CToString(_header.manufacturer));

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.product = {0}",
                                       StringHandlers.CToString(_header.product));

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.revision = {0}",
                                       StringHandlers.CToString(_header.revision));

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.vendor = {0}",
                                       StringHandlers.CToString(_header.vendor));

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.volumeId = {0}",
                                       StringHandlers.CToString(_header.volumeId));

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.mode2ALen = {0}", _header.mode2ALen);
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.unkBlkLen = {0}", _header.unkBlkLen);
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.dataLen = {0}", _header.dataLen);
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.sessionsLen = {0}", _header.sessionsLen);
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "header.dpmLen = {0}", _header.dpmLen);

            _mode2A = new byte[_header.mode2ALen];

            if(_mode2A.Length > 0)
            {
                stream.Read(_mode2A, 0, _mode2A.Length);
                _mode2A[1] -= 2;
                var decoded2A = ModePage_2A.Decode(_mode2A);

                if(!(decoded2A is null))
                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "mode page 2A: {0}",
                                               Modes.PrettifyModePage_2A(decoded2A));
                else
                    _mode2A = null;
            }

            _unkBlock = new byte[_header.unkBlkLen];

            if(_unkBlock.Length > 0)
                stream.Read(_unkBlock, 0, _unkBlock.Length);

            byte[] temp = new byte[_header.pmaLen];

            if(temp.Length > 0)
            {
                byte[] tushort = BitConverter.GetBytes((ushort)(temp.Length + 2));
                stream.Read(temp, 0, temp.Length);
                _pma    = new byte[temp.Length + 4];
                _pma[0] = tushort[1];
                _pma[1] = tushort[0];
                Array.Copy(temp, 0, _pma, 4, temp.Length);

                PMA.CDPMA? decodedPma = PMA.Decode(_pma);

                if(decodedPma.HasValue)
                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "PMA: {0}", PMA.Prettify(decodedPma));
                else
                    _pma = null;
            }

            _atip = new byte[_header.atipLen];

            if(_atip.Length > 0)
                stream.Read(_atip, 0, _atip.Length);
            else
                _atip = null;

            _cdtext = new byte[_header.cdtLen];

            if(_cdtext.Length > 0)
                stream.Read(_cdtext, 0, _cdtext.Length);
            else
                _cdtext = null;

            _bca = new byte[_header.bcaLen];

            if(_bca.Length > 0)
                stream.Read(_bca, 0, _bca.Length);
            else
                _bca = null;

            temp = new byte[_header.dvdStrLen];

            if(temp.Length > 0)
            {
                stream.Read(temp, 0, temp.Length);
                _dmi = new byte[2052];
                _pfi = new byte[2052];

                // TODO: CMI
                Array.Copy(temp, 2, _dmi, 4, 2048);
                Array.Copy(temp, 0x802, _pfi, 4, 2048);

                _pfi[0] = 0x08;
                _pfi[1] = 0x02;
                _dmi[0] = 0x08;
                _dmi[1] = 0x02;

                PFI.PhysicalFormatInformation? decodedPfi = PFI.Decode(_pfi, MediaType.DVDROM);

                if(decodedPfi.HasValue)
                    AaruConsole.DebugWriteLine("BlindWrite5 plugin", "PFI: {0}", PFI.Prettify(decodedPfi));
                else
                {
                    _pfi = null;
                    _dmi = null;
                }
            }

            switch(_header.profile)
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
                    _discInformation = new byte[_header.cdInfoLen];

                    break;
                default:
                    _discInformation = new byte[_header.dvdInfoLen];

                    break;
            }

            if(_discInformation.Length > 0)
            {
                stream.Read(_discInformation, 0, _discInformation.Length);

                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "Disc information: {0}",
                                           PrintHex.ByteArrayToHexArrayString(_discInformation, 40));
            }
            else
                _discInformation = null;

            // How many data blocks
            byte[] tmpArray = new byte[4];
            stream.Read(tmpArray, 0, tmpArray.Length);
            uint dataBlockCount = BitConverter.ToUInt32(tmpArray, 0);

            stream.Read(tmpArray, 0, tmpArray.Length);
            uint   dataPathLen   = BitConverter.ToUInt32(tmpArray, 0);
            byte[] dataPathBytes = new byte[dataPathLen];
            stream.Read(dataPathBytes, 0, dataPathBytes.Length);
            _dataPath = Encoding.Unicode.GetString(dataPathBytes);
            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "Data path: {0}", _dataPath);

            _dataFiles = new List<DataFile>();

            for(int cD = 0; cD < dataBlockCount; cD++)
            {
                tmpArray = new byte[52];

                var dataFile = new DataFile
                {
                    Unknown1 = new uint[4],
                    Unknown2 = new uint[3]
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
                _dataFiles.Add(dataFile);

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

            _bwSessions = new List<SessionDescriptor>();

            for(int ses = 0; ses < _header.sessions; ses++)
            {
                var session = new SessionDescriptor();
                tmpArray = new byte[16];
                stream.Read(tmpArray, 0, tmpArray.Length);
                session.Sequence   = BitConverter.ToUInt16(tmpArray, 0);
                session.Entries    = tmpArray[2];
                session.Unknown    = tmpArray[3];
                session.Start      = BitConverter.ToInt32(tmpArray, 4);
                session.End        = BitConverter.ToInt32(tmpArray, 8);
                session.FirstTrack = BitConverter.ToUInt16(tmpArray, 12);
                session.LastTrack  = BitConverter.ToUInt16(tmpArray, 14);
                session.Tracks     = new TrackDescriptor[session.Entries];

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
                    session.Tracks[tSeq] = Marshal.ByteArrayToStructureLittleEndian<TrackDescriptor>(trk);

                    if(session.Tracks[tSeq].type == TrackType.Dvd ||
                       session.Tracks[tSeq].type == TrackType.NotData)
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

                    if(session.Tracks[tSeq].type == TrackType.Dvd ||
                       session.Tracks[tSeq].type == TrackType.NotData)
                        continue;

                    {
                        for(int i = 0; i < session.Tracks[tSeq].unknown9.Length; i++)
                            AaruConsole.DebugWriteLine("BlindWrite5 plugin",
                                                       "session[{0}].track[{1}].unknown9[{2}] = 0x{3:X8}", ses, tSeq, i,
                                                       session.Tracks[tSeq].unknown9[i]);
                    }
                }

                _bwSessions.Add(session);
            }

            _dpm = new byte[_header.dpmLen];
            stream.Read(_dpm, 0, _dpm.Length);

            // Unused
            tmpArray = new byte[4];
            stream.Read(tmpArray, 0, tmpArray.Length);

            byte[] footer = new byte[16];
            stream.Read(footer, 0, footer.Length);

            if(_bw5Footer.SequenceEqual(footer))
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "Correctly arrived end of image");
            else
                AaruConsole.
                    ErrorWriteLine("BlindWrite5 image ends after expected position. Probably new version with different data. Errors may occur.");

            _filePaths = new List<DataFileCharacteristics>();

            foreach(DataFile dataFile in _dataFiles)
            {
                var    chars       = new DataFileCharacteristics();
                string path        = Path.Combine(_dataPath, dataFile.Filename);
                var    filtersList = new FiltersList();

                if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path)) != null)
                {
                    chars.FileFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path));
                    chars.FilePath   = path;
                }
                else
                {
                    path = Path.Combine(_dataPath, dataFile.Filename.ToLower(CultureInfo.CurrentCulture));

                    if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path)) != null)
                    {
                        chars.FileFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path));
                        chars.FilePath   = path;
                    }
                    else
                    {
                        path = Path.Combine(_dataPath, dataFile.Filename.ToUpper(CultureInfo.CurrentCulture));

                        if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path)) != null)
                        {
                            chars.FileFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path));
                            chars.FilePath   = path;
                        }
                        else
                        {
                            path = Path.Combine(_dataPath.ToLower(CultureInfo.CurrentCulture), dataFile.Filename);

                            if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path)) != null)
                            {
                                chars.FileFilter =
                                    filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path));

                                chars.FilePath = path;
                            }
                            else
                            {
                                path = Path.Combine(_dataPath.ToUpper(CultureInfo.CurrentCulture), dataFile.Filename);

                                if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path)) != null)
                                {
                                    chars.FileFilter =
                                        filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), path));

                                    chars.FilePath = path;
                                }
                                else
                                {
                                    path = Path.Combine(_dataPath, dataFile.Filename);

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
                                                                                   CurrentCulture))) != null)
                                    {
                                        chars.FilePath = path.ToUpper(CultureInfo.CurrentCulture);

                                        chars.FileFilter =
                                            filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                               path.ToUpper(CultureInfo.
                                                                                   CurrentCulture)));
                                    }
                                    else if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                               dataFile.Filename.ToLower(CultureInfo.
                                                                                   CurrentCulture))) != null)
                                    {
                                        chars.FilePath = dataFile.Filename.ToLower(CultureInfo.CurrentCulture);

                                        chars.FileFilter =
                                            filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                               dataFile.Filename.ToLower(CultureInfo.
                                                                                   CurrentCulture)));
                                    }
                                    else if(filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                               dataFile.Filename.ToUpper(CultureInfo.
                                                                                   CurrentCulture))) != null)
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

                                        continue;
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
                chars.Offset     = dataFile.Offset;

                _filePaths.Add(chars);
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
            _offsetMap = new Dictionary<uint, ulong>();
            bool isDvd        = false;
            byte firstSession = byte.MaxValue;
            byte lastSession  = 0;
            _trackFlags        = new Dictionary<uint, byte>();
            _imageInfo.Sectors = 0;

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "Building maps");

            foreach(SessionDescriptor ses in _bwSessions)
            {
                Sessions.Add(new Session
                {
                    SessionSequence = ses.Sequence,
                    StartSector     = ses.Start < 0 ? 0 : (ulong)ses.Start,
                    EndSector       = (ulong)ses.End - 1,
                    StartTrack      = ses.FirstTrack,
                    EndTrack        = ses.LastTrack
                });

                if(ses.Sequence < firstSession)
                    firstSession = (byte)ses.Sequence;

                if(ses.Sequence > lastSession)
                    lastSession = (byte)ses.Sequence;

                foreach(TrackDescriptor trk in ses.Tracks)
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

                    _trackFlags.Add(trk.point, trk.ctl);

                    switch(trk.type)
                    {
                        case TrackType.Audio:
                            track.TrackBytesPerSector    = 2352;
                            track.TrackRawBytesPerSector = 2352;

                            if(_imageInfo.SectorSize < 2352)
                                _imageInfo.SectorSize = 2352;

                            break;
                        case TrackType.Mode1:
                        case TrackType.Mode2F1:
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

                            track.TrackBytesPerSector    = 2048;
                            track.TrackRawBytesPerSector = 2352;

                            if(_imageInfo.SectorSize < 2048)
                                _imageInfo.SectorSize = 2048;

                            break;
                        case TrackType.Mode2:
                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                            track.TrackBytesPerSector    = 2336;
                            track.TrackRawBytesPerSector = 2352;

                            if(_imageInfo.SectorSize < 2336)
                                _imageInfo.SectorSize = 2336;

                            break;
                        case TrackType.Mode2F2:
                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);

                            track.TrackBytesPerSector    = 2336;
                            track.TrackRawBytesPerSector = 2352;

                            if(_imageInfo.SectorSize < 2324)
                                _imageInfo.SectorSize = 2324;

                            break;
                        case TrackType.Dvd:
                            track.TrackBytesPerSector    = 2048;
                            track.TrackRawBytesPerSector = 2048;

                            if(_imageInfo.SectorSize < 2048)
                                _imageInfo.SectorSize = 2048;

                            isDvd = true;

                            break;
                    }

                    track.TrackDescription = $"Track {trk.point}";
                    track.TrackStartSector = (ulong)(trk.startLba + trk.pregap);
                    track.TrackEndSector   = (ulong)(trk.sectors + trk.startLba) - 1;

                    List<DataFileCharacteristics> fileCharsForThisTrack = _filePaths.
                                                                          Where(chars =>
                                                                                    trk.startLba >=
                                                                                    chars.StartLba &&
                                                                                    trk.startLba   + trk.sectors <=
                                                                                    chars.StartLba + chars.Sectors).
                                                                          ToList();

                    if(fileCharsForThisTrack.Count == 0 &&
                       _filePaths.Any(f => Path.GetExtension(f.FilePath).ToLowerInvariant() == ".b00"))
                    {
                        DataFileCharacteristics splitStartChars =
                            _filePaths.FirstOrDefault(f => Path.GetExtension(f.FilePath).ToLowerInvariant() == ".b00");

                        string filename           = Path.GetFileNameWithoutExtension(splitStartChars.FilePath);
                        bool   lowerCaseExtension = false;
                        bool   lowerCaseFileName  = false;
                        string basePath;

                        bool version5 = string.Compare(Path.GetExtension(imageFilter.GetFilename()), ".B5T",
                                                       StringComparison.OrdinalIgnoreCase) == 0;

                        string firstExtension      = version5 ? "B5I" : "B6I";
                        string firstExtensionLower = version5 ? "b5i" : "b6i";

                        if(File.Exists(Path.Combine(imageFilter.GetParentFolder(), $"{filename}.{firstExtension}")))
                        {
                            basePath = imageFilter.GetParentFolder();
                        }
                        else if(File.Exists(Path.Combine(imageFilter.GetParentFolder(),
                                                         $"{filename}.{firstExtensionLower}")))
                        {
                            basePath           = imageFilter.GetParentFolder();
                            lowerCaseExtension = true;
                        }
                        else if(File.Exists(Path.Combine(imageFilter.GetParentFolder(),
                                                         $"{filename.ToLower(CultureInfo.CurrentCulture)}.{firstExtension}")))
                        {
                            basePath          = imageFilter.GetParentFolder();
                            lowerCaseFileName = true;
                        }
                        else if(File.Exists(Path.Combine(imageFilter.GetParentFolder(),
                                                         $"{filename.ToLower(CultureInfo.CurrentCulture)}.{firstExtensionLower}")))
                        {
                            basePath           = imageFilter.GetParentFolder();
                            lowerCaseFileName  = true;
                            lowerCaseExtension = true;
                        }

                        else if(File.Exists(Path.Combine(_dataPath, $"{filename}.{firstExtension}")))
                        {
                            basePath = _dataPath;
                        }
                        else if(File.Exists(Path.Combine(_dataPath, $"{filename}.{firstExtensionLower}")))
                        {
                            basePath           = _dataPath;
                            lowerCaseExtension = true;
                        }
                        else if(File.Exists(Path.Combine(_dataPath,
                                                         $"{filename.ToLower(CultureInfo.CurrentCulture)}.{firstExtension}")))
                        {
                            basePath          = _dataPath;
                            lowerCaseFileName = true;
                        }
                        else if(File.Exists(Path.Combine(_dataPath,
                                                         $"{filename.ToLower(CultureInfo.CurrentCulture)}.{firstExtensionLower}")))
                        {
                            basePath           = _dataPath;
                            lowerCaseFileName  = true;
                            lowerCaseExtension = true;
                        }

                        else if(File.Exists(Path.Combine(_dataPath.ToLower(CultureInfo.CurrentCulture),
                                                         $"{filename}.{firstExtension}")))
                        {
                            basePath = _dataPath.ToLower(CultureInfo.CurrentCulture);
                        }
                        else if(File.Exists(Path.Combine(_dataPath.ToLower(CultureInfo.CurrentCulture),
                                                         $"{filename}.{firstExtensionLower}")))
                        {
                            basePath           = _dataPath.ToLower(CultureInfo.CurrentCulture);
                            lowerCaseExtension = true;
                        }
                        else if(File.Exists(Path.Combine(_dataPath.ToLower(CultureInfo.CurrentCulture),
                                                         $"{filename.ToLower(CultureInfo.CurrentCulture)}.{firstExtension}")))
                        {
                            basePath          = _dataPath.ToLower(CultureInfo.CurrentCulture);
                            lowerCaseFileName = true;
                        }
                        else if(File.Exists(Path.Combine(_dataPath.ToLower(CultureInfo.CurrentCulture),
                                                         $"{filename.ToLower(CultureInfo.CurrentCulture)}.{firstExtensionLower}")))
                        {
                            basePath           = _dataPath.ToLower(CultureInfo.CurrentCulture);
                            lowerCaseFileName  = true;
                            lowerCaseExtension = true;
                        }

                        else if(File.Exists(Path.Combine(imageFilter.GetParentFolder(), _dataPath,
                                                         $"{filename}.{firstExtension}")))
                        {
                            basePath = Path.Combine(imageFilter.GetParentFolder(), _dataPath);
                        }
                        else if(File.Exists(Path.Combine(imageFilter.GetParentFolder(), _dataPath,
                                                         $"{filename}.{firstExtensionLower}")))
                        {
                            basePath           = Path.Combine(imageFilter.GetParentFolder(), _dataPath);
                            lowerCaseExtension = true;
                        }
                        else if(File.Exists(Path.Combine(imageFilter.GetParentFolder(), _dataPath,
                                                         $"{filename.ToLower(CultureInfo.CurrentCulture)}.{firstExtension}")))
                        {
                            basePath          = Path.Combine(imageFilter.GetParentFolder(), _dataPath);
                            lowerCaseFileName = true;
                        }
                        else if(File.Exists(Path.Combine(imageFilter.GetParentFolder(), _dataPath,
                                                         $"{filename.ToLower(CultureInfo.CurrentCulture)}.{firstExtensionLower}")))
                        {
                            basePath           = Path.Combine(imageFilter.GetParentFolder(), _dataPath);
                            lowerCaseFileName  = true;
                            lowerCaseExtension = true;
                        }

                        else if(File.Exists(Path.Combine(imageFilter.GetParentFolder(),
                                                         _dataPath.ToLower(CultureInfo.CurrentCulture),
                                                         $"{filename}.{firstExtension}")))
                        {
                            basePath = Path.Combine(imageFilter.GetParentFolder(),
                                                    _dataPath.ToLower(CultureInfo.CurrentCulture));
                        }
                        else if(File.Exists(Path.Combine(imageFilter.GetParentFolder(),
                                                         _dataPath.ToLower(CultureInfo.CurrentCulture),
                                                         $"{filename}.b00")))
                        {
                            basePath = Path.Combine(imageFilter.GetParentFolder(),
                                                    _dataPath.ToLower(CultureInfo.CurrentCulture));

                            lowerCaseExtension = true;
                        }
                        else if(File.Exists(Path.Combine(imageFilter.GetParentFolder(),
                                                         _dataPath.ToLower(CultureInfo.CurrentCulture),
                                                         $"{filename.ToLower(CultureInfo.CurrentCulture)}.{firstExtension}")))
                        {
                            basePath = Path.Combine(imageFilter.GetParentFolder(),
                                                    _dataPath.ToLower(CultureInfo.CurrentCulture));

                            lowerCaseFileName = true;
                        }
                        else if(File.Exists(Path.Combine(imageFilter.GetParentFolder(),
                                                         _dataPath.ToLower(CultureInfo.CurrentCulture),
                                                         $"{filename.ToLower(CultureInfo.CurrentCulture)}.{firstExtensionLower}")))
                        {
                            basePath = Path.Combine(imageFilter.GetParentFolder(),
                                                    _dataPath.ToLower(CultureInfo.CurrentCulture));

                            lowerCaseFileName  = true;
                            lowerCaseExtension = true;
                        }
                        else if(File.Exists(Path.Combine("", $"{filename}.{firstExtension}")))
                        {
                            basePath = "";
                        }
                        else if(File.Exists(Path.Combine("", $"{filename}.{firstExtensionLower}")))
                        {
                            basePath           = "";
                            lowerCaseExtension = true;
                        }
                        else if(
                            File.Exists(Path.Combine("",
                                                     $"{filename.ToLower(CultureInfo.CurrentCulture)}.{firstExtension}")))
                        {
                            basePath          = "";
                            lowerCaseFileName = true;
                        }
                        else if(File.Exists(Path.Combine("",
                                                         $"{filename.ToLower(CultureInfo.CurrentCulture)}.{firstExtensionLower}")))
                        {
                            basePath           = "";
                            lowerCaseFileName  = true;
                            lowerCaseExtension = true;
                        }
                        else
                        {
                            AaruConsole.ErrorWriteLine("Could not find image for track {0}", trk.point);

                            return false;
                        }

                        var splitStream = new SplitJoinStream();

                        if(lowerCaseFileName)
                            filename = filename.ToLower(CultureInfo.CurrentCulture);

                        string extension = lowerCaseExtension ? "b{0:D2}" : "B{0:D2}";

                        try
                        {
                            splitStream.
                                Add(Path.Combine(basePath, $"{filename}.{(lowerCaseExtension ? firstExtensionLower : firstExtension)}"),
                                    FileMode.Open);

                            splitStream.AddRange(basePath, $"{filename}.{extension}");
                        }
                        catch(Exception)
                        {
                            AaruConsole.ErrorWriteLine("Could not find image for track {0}", trk.point);

                            return false;
                        }

                        track.TrackFilter = splitStream.Filter;
                        track.TrackFile   = $"{filename}.{extension}";

                        if(trk.startLba >= 0)
                            track.TrackFileOffset =
                                (ulong)((trk.startLba * splitStartChars.SectorSize) + splitStartChars.Offset);
                        else
                            track.TrackFileOffset = (ulong)(trk.startLba * -1 * splitStartChars.SectorSize);

                        track.TrackFileType = "BINARY";

                        if(splitStartChars.Subchannel != TrackSubchannelType.None)
                        {
                            track.TrackSubchannelFilter = track.TrackFilter;
                            track.TrackSubchannelFile   = track.TrackFile;
                            track.TrackSubchannelType   = splitStartChars.Subchannel;
                            track.TrackSubchannelOffset = track.TrackFileOffset;

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);
                        }

                        splitStartChars.FileFilter = splitStream.Filter;
                        splitStartChars.Sectors    = trk.sectors;
                        splitStartChars.StartLba   = trk.startLba;
                        _filePaths.Clear();
                        _filePaths.Add(splitStartChars);
                    }
                    else
                        foreach(DataFileCharacteristics chars in fileCharsForThisTrack)
                        {
                            track.TrackFilter = chars.FileFilter;
                            track.TrackFile   = chars.FileFilter.GetFilename();

                            if(trk.startLba >= 0)
                                track.TrackFileOffset =
                                    (ulong)((trk.startLba - chars.StartLba) * chars.SectorSize) + chars.Offset;
                            else
                                track.TrackFileOffset = (ulong)(trk.startLba * -1 * chars.SectorSize);

                            track.TrackFileType = "BINARY";

                            if(chars.Subchannel != TrackSubchannelType.None)
                            {
                                track.TrackSubchannelFilter = track.TrackFilter;
                                track.TrackSubchannelFile   = track.TrackFile;
                                track.TrackSubchannelType   = chars.Subchannel;
                                track.TrackSubchannelOffset = track.TrackFileOffset;

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);
                            }

                            break;
                        }

                    if(track.TrackFilter is null)
                    {
                        AaruConsole.ErrorWriteLine("Could not find image for track {0}", trk.point);

                        return false;
                    }

                    track.TrackPregap   = trk.pregap;
                    track.TrackSequence = trk.point;
                    track.TrackType     = BlindWriteTrackTypeToTrackType(trk.type);

                    if(trk.pregap             > 0 &&
                       track.TrackStartSector > 0)
                    {
                        track.Indexes[0] = (int)track.TrackStartSector - (int)trk.pregap;

                        if(track.Indexes[0] < 0)
                            track.Indexes[0] = 0;
                    }

                    track.Indexes[1] = (int)track.TrackStartSector;

                    partition.Description = track.TrackDescription;

                    partition.Size = (track.TrackEndSector - track.TrackStartSector) *
                                     (ulong)track.TrackRawBytesPerSector;

                    partition.Length   = track.TrackEndSector - track.TrackStartSector + 1;
                    partition.Sequence = track.TrackSequence;
                    partition.Offset   = offsetBytes;
                    partition.Start    = track.TrackStartSector;
                    partition.Type     = track.TrackType.ToString();

                    offsetBytes += partition.Size;

                    if(track.TrackStartSector >= trk.pregap)
                        track.TrackStartSector -= trk.pregap;

                    if(track.TrackEndSector > _imageInfo.Sectors)
                        _imageInfo.Sectors = track.TrackEndSector + 1;

                    Tracks.Add(track);
                    Partitions.Add(partition);
                    _offsetMap.Add(track.TrackSequence, track.TrackStartSector);
                }
            }

            foreach(Track track in Tracks)
            {
                Session trackSession =
                    Sessions.FirstOrDefault(s => track.TrackSequence >= s.StartTrack &&
                                                 track.TrackSequence <= s.EndTrack);

                track.TrackSession = trackSession.SessionSequence;
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

                _fullToc = fullTocStream.ToArray();
                AaruConsole.DebugWriteLine("BlindWrite5 plugin", "TOC len {0}", _fullToc.Length);

                _fullToc[0] = firstSession;
                _fullToc[1] = lastSession;

                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackFlags);
            }

            _imageInfo.MediaType = BlindWriteProfileToMediaType(_header.profile);

            if(_dmi != null &&
               _pfi != null)
            {
                PFI.PhysicalFormatInformation? pfi0 = PFI.Decode(_pfi, _imageInfo.MediaType);

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
                            _imageInfo.MediaType = pfi0.Value.PartVersion >= 6 ? MediaType.DVDRDL : MediaType.DVDR;

                            break;
                        case DiskCategory.DVDRAM:
                            _imageInfo.MediaType = MediaType.DVDRAM;

                            break;
                        default:
                            _imageInfo.MediaType = MediaType.DVDROM;

                            break;
                        case DiskCategory.DVDRW:
                            _imageInfo.MediaType = pfi0.Value.PartVersion >= 15 ? MediaType.DVDRWDL : MediaType.DVDRW;

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
                }
            }
            else if(_imageInfo.MediaType == MediaType.CD ||
                    _imageInfo.MediaType == MediaType.CDROM)
            {
                bool data       = false;
                bool mode2      = false;
                bool firstAudio = false;
                bool firstData  = false;
                bool audio      = false;

                foreach(Track bwTrack in Tracks)
                {
                    // First track is audio
                    firstAudio |= bwTrack.TrackSequence == 1 && bwTrack.TrackType == CommonTypes.Enums.TrackType.Audio;

                    // First track is data
                    firstData |= bwTrack.TrackSequence == 1 && bwTrack.TrackType != CommonTypes.Enums.TrackType.Audio;

                    // Any non first track is data
                    data |= bwTrack.TrackSequence != 1 && bwTrack.TrackType != CommonTypes.Enums.TrackType.Audio;

                    // Any non first track is audio
                    audio |= bwTrack.TrackSequence != 1 && bwTrack.TrackType == CommonTypes.Enums.TrackType.Audio;

                    switch(bwTrack.TrackType)
                    {
                        case CommonTypes.Enums.TrackType.CdMode2Formless:
                        case CommonTypes.Enums.TrackType.CdMode2Form1:
                        case CommonTypes.Enums.TrackType.CdMode2Form2:
                            mode2 = true;

                            break;
                    }
                }

                if(!data &&
                   !firstData)
                    _imageInfo.MediaType = MediaType.CDDA;
                else if(firstAudio         &&
                        data               &&
                        Sessions.Count > 1 &&
                        mode2)
                    _imageInfo.MediaType = MediaType.CDPLUS;
                else if((firstData && audio) || mode2)
                    _imageInfo.MediaType = MediaType.CDROMXA;
                else if(!audio)
                    _imageInfo.MediaType = MediaType.CDROM;
                else
                    _imageInfo.MediaType = MediaType.CD;
            }

            _imageInfo.DriveManufacturer     = StringHandlers.CToString(_header.manufacturer);
            _imageInfo.DriveModel            = StringHandlers.CToString(_header.product);
            _imageInfo.DriveFirmwareRevision = StringHandlers.CToString(_header.revision);
            _imageInfo.Application           = "BlindWrite";

            if(string.Compare(Path.GetExtension(imageFilter.GetFilename()), ".B5T",
                              StringComparison.OrdinalIgnoreCase) == 0)
                _imageInfo.ApplicationVersion = "5";
            else if(string.Compare(Path.GetExtension(imageFilter.GetFilename()), ".B6T",
                                   StringComparison.OrdinalIgnoreCase) == 0)
                _imageInfo.ApplicationVersion = "6";

            _imageInfo.Version = "5";

            _imageInfo.ImageSize            = (ulong)imageFilter.GetDataForkLength();
            _imageInfo.CreationTime         = imageFilter.GetCreationTime();
            _imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            _imageInfo.XmlMediaType         = XmlMediaType.OpticalDisc;

            if(_pma != null)
            {
                PMA.CDPMA pma0 = PMA.Decode(_pma).Value;

                foreach(uint id in from descriptor in pma0.PMADescriptors where descriptor.ADR == 2
                                   select (uint)((descriptor.Min << 16) + (descriptor.Sec << 8) + descriptor.Frame))
                    _imageInfo.MediaSerialNumber = $"{id & 0x00FFFFFF:X6}";
            }

            if(_atip != null)
            {
                byte[] atipTmp = new byte[_atip.Length + 4];
                Array.Copy(_atip, 0, atipTmp, 4, _atip.Length);
                atipTmp[0] = (byte)((_atip.Length & 0xFF00) >> 8);
                atipTmp[1] = (byte)(_atip.Length & 0xFF);

                ATIP.CDATIP atip0 = ATIP.Decode(atipTmp);

                _imageInfo.MediaType = atip0?.DiscType ?? false ? MediaType.CDRW : MediaType.CDR;

                if(atip0.LeadInStartMin == 97)
                {
                    int type = atip0.LeadInStartFrame % 10;
                    int frm  = atip0.LeadInStartFrame - type;
                    _imageInfo.MediaManufacturer = ATIP.ManufacturerFromATIP(atip0.LeadInStartSec, frm);
                }
            }

            bool isBd = false;

            if(_imageInfo.MediaType == MediaType.BDR  ||
               _imageInfo.MediaType == MediaType.BDRE ||
               _imageInfo.MediaType == MediaType.BDROM)
            {
                isDvd = false;
                isBd  = true;
            }

            if(isBd && _imageInfo.Sectors > 24438784)
                switch(_imageInfo.MediaType)
                {
                    case MediaType.BDR:
                        _imageInfo.MediaType = MediaType.BDRXL;

                        break;
                    case MediaType.BDRE:
                        _imageInfo.MediaType = MediaType.BDREXL;

                        break;
                }

            AaruConsole.DebugWriteLine("BlindWrite5 plugin", "ImageInfo.mediaType = {0}", _imageInfo.MediaType);

            if(_mode2A != null)
                _imageInfo.ReadableMediaTags.Add(MediaTagType.SCSI_MODEPAGE_2A);

            if(_pma != null)
                _imageInfo.ReadableMediaTags.Add(MediaTagType.CD_PMA);

            if(_atip != null)
                _imageInfo.ReadableMediaTags.Add(MediaTagType.CD_ATIP);

            if(_cdtext != null)
                _imageInfo.ReadableMediaTags.Add(MediaTagType.CD_TEXT);

            if(_bca != null)
                if(isDvd)
                    _imageInfo.ReadableMediaTags.Add(MediaTagType.DVD_BCA);
                else if(isBd)
                    _imageInfo.ReadableMediaTags.Add(MediaTagType.BD_BCA);

            byte[] tmp;

            if(_dmi != null)
            {
                tmp = new byte[2048];
                Array.Copy(_dmi, 4, tmp, 0, 2048);
                _dmi = tmp;
                _imageInfo.ReadableMediaTags.Add(MediaTagType.DVD_DMI);
            }

            if(_pfi != null)
            {
                tmp = new byte[2048];
                Array.Copy(_pfi, 4, tmp, 0, 2048);
                _pfi = tmp;
                _imageInfo.ReadableMediaTags.Add(MediaTagType.DVD_PFI);
            }

            if(_fullToc != null)
                _imageInfo.ReadableMediaTags.Add(MediaTagType.CD_FullTOC);

            if(_imageInfo.MediaType == MediaType.XGD2)
                if(_imageInfo.Sectors == 25063   || // Locked (or non compatible drive)
                   _imageInfo.Sectors == 4229664 || // Xtreme unlock
                   _imageInfo.Sectors == 4246304)   // Wxripper unlock
                    _imageInfo.MediaType = MediaType.XGD3;

            AaruConsole.VerboseWriteLine("BlindWrite image describes a disc of type {0}", _imageInfo.MediaType);

            if(_header.profile != ProfileNumber.CDR  &&
               _header.profile != ProfileNumber.CDRW &&
               _header.profile != ProfileNumber.CDROM)
                foreach(Track track in Tracks)
                {
                    track.TrackPregap = 0;
                    track.Indexes?.Clear();
                }

            return true;
        }

        /// <inheritdoc />
        public byte[] ReadDiskTag(MediaTagType tag)
        {
            switch(tag)
            {
                case MediaTagType.SCSI_MODEPAGE_2A:
                {
                    if(_mode2A != null)
                        return (byte[])_mode2A.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain SCSI MODE PAGE 2Ah.");
                }
                case MediaTagType.CD_PMA:
                {
                    if(_pma != null)
                        return (byte[])_pma.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain PMA information.");
                }
                case MediaTagType.CD_ATIP:
                {
                    if(_atip != null)
                        return (byte[])_atip.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain ATIP information.");
                }
                case MediaTagType.CD_TEXT:
                {
                    if(_cdtext != null)
                        return (byte[])_cdtext.Clone();

                    throw new FeatureNotPresentImageException("Image does not contain CD-Text information.");
                }
                case MediaTagType.DVD_BCA:
                case MediaTagType.BD_BCA:
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

        /// <inheritdoc />
        public byte[] ReadSector(ulong sectorAddress) => ReadSectors(sectorAddress, 1);

        /// <inheritdoc />
        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag) => ReadSectorsTag(sectorAddress, 1, tag);

        /// <inheritdoc />
        public byte[] ReadSector(ulong sectorAddress, uint track) => ReadSectors(sectorAddress, 1, track);

        /// <inheritdoc />
        public byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag) =>
            ReadSectorsTag(sectorAddress, 1, track, tag);

        /// <inheritdoc />
        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap where sectorAddress     >= kvp.Value
                                                     from track in Tracks where track.TrackSequence == kvp.Key
                                                     where sectorAddress                                 - kvp.Value <
                                                           track.TrackEndSector - track.TrackStartSector + 1 select kvp)
                return ReadSectors(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        /// <inheritdoc />
        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap where sectorAddress     >= kvp.Value
                                                     from track in Tracks where track.TrackSequence == kvp.Key
                                                     where sectorAddress                                 - kvp.Value <
                                                           track.TrackEndSector - track.TrackStartSector + 1 select kvp)
                return ReadSectorsTag(sectorAddress - kvp.Value, length, kvp.Key, tag);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        /// <inheritdoc />
        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            // TODO: Cross data files
            var aaruTrack = new Track
            {
                TrackSequence = 0
            };

            foreach(Track bwTrack in Tracks.Where(bwTrack => bwTrack.TrackSequence == track))
            {
                aaruTrack = bwTrack;

                break;
            }

            if(aaruTrack is null)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > aaruTrack.TrackEndSector - aaruTrack.TrackStartSector + 1)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({aaruTrack.TrackEndSector - aaruTrack.TrackStartSector + 1}), won't cross tracks");

            DataFileCharacteristics chars = (from characteristics in _filePaths let firstSector =
                                                 characteristics.StartLba let lastSector =
                                                 firstSector + characteristics.Sectors - 1 let wantedSector =
                                                 (int)(sectorAddress + aaruTrack.TrackStartSector)
                                             where wantedSector >= firstSector && wantedSector <= lastSector
                                             select characteristics).FirstOrDefault();

            if(string.IsNullOrEmpty(chars.FilePath) ||
               chars.FileFilter == null)
                throw new ArgumentOutOfRangeException(nameof(chars.FileFilter), "Track does not exist in disc image");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;
            bool mode2 = false;

            switch(aaruTrack.TrackType)
            {
                case CommonTypes.Enums.TrackType.CdMode1:
                {
                    sectorOffset = 16;
                    sectorSize   = 2048;
                    sectorSkip   = 288;

                    break;
                }
                case CommonTypes.Enums.TrackType.CdMode2Formless:
                case CommonTypes.Enums.TrackType.CdMode2Form1:
                case CommonTypes.Enums.TrackType.CdMode2Form2:
                {
                    mode2        = true;
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 0;

                    break;
                }
                case CommonTypes.Enums.TrackType.Audio:
                {
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 0;

                    break;
                }
                case CommonTypes.Enums.TrackType.Data:
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

            _imageStream = chars.FileFilter.GetDataForkStream();
            var br = new BinaryReader(_imageStream);

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

        /// <inheritdoc />
        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            // TODO: Cross data files
            var aaruTrack = new Track
            {
                TrackSequence = 0
            };

            foreach(Track bwTrack in Tracks.Where(bwTrack => bwTrack.TrackSequence == track))
            {
                aaruTrack = bwTrack;

                break;
            }

            if(aaruTrack is null)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > aaruTrack.TrackEndSector - aaruTrack.TrackStartSector + 1)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({aaruTrack.TrackEndSector - aaruTrack.TrackStartSector + 1}), won't cross tracks");

            DataFileCharacteristics chars = (from characteristics in _filePaths let firstSector =
                                                 characteristics.StartLba let lastSector =
                                                 firstSector + characteristics.Sectors - 1 let wantedSector =
                                                 (int)(sectorAddress + aaruTrack.TrackStartSector)
                                             where wantedSector >= firstSector && wantedSector <= lastSector
                                             select characteristics).FirstOrDefault();

            if(string.IsNullOrEmpty(chars.FilePath) ||
               chars.FileFilter == null)
                throw new ArgumentOutOfRangeException(nameof(chars.FileFilter), "Track does not exist in disc image");

            if(aaruTrack.TrackType == CommonTypes.Enums.TrackType.Data)
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
                    if(_trackFlags.TryGetValue((uint)sectorAddress, out byte flag))
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
                case CommonTypes.Enums.TrackType.CdMode1:
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
                            switch(chars.Subchannel)
                            {
                                case TrackSubchannelType.PackedInterleaved:
                                {
                                    sectorOffset = 2352;
                                    sectorSize   = 96;
                                    sectorSkip   = 0;

                                    break;
                                }

                                case TrackSubchannelType.Q16Interleaved:
                                {
                                    sectorOffset = 2352;
                                    sectorSize   = 16;
                                    sectorSkip   = 0;

                                    break;
                                }
                                case TrackSubchannelType.None:
                                {
                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                                        throw new
                                            FeatureSupportedButNotImplementedImageException("Unsupported track type");

                                    return new byte[length * 96];
                                }

                                default: throw new ArgumentOutOfRangeException();
                            }

                            break;
                        }
                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                case CommonTypes.Enums.TrackType.CdMode2Formless:
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
                            switch(chars.Subchannel)
                            {
                                case TrackSubchannelType.PackedInterleaved:
                                {
                                    sectorOffset = 2352;
                                    sectorSize   = 96;
                                    sectorSkip   = 0;

                                    break;
                                }

                                case TrackSubchannelType.Q16Interleaved:
                                {
                                    sectorOffset = 2352;
                                    sectorSize   = 16;
                                    sectorSkip   = 0;

                                    break;
                                }
                                case TrackSubchannelType.None:
                                {
                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                                        throw new
                                            FeatureSupportedButNotImplementedImageException("Unsupported track type");

                                    return new byte[length * 96];
                                }

                                default: throw new ArgumentOutOfRangeException();
                            }

                            break;
                        }
                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                }
                case CommonTypes.Enums.TrackType.CdMode2Form1:
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
                            switch(chars.Subchannel)
                            {
                                case TrackSubchannelType.PackedInterleaved:
                                {
                                    sectorOffset = 2352;
                                    sectorSize   = 96;
                                    sectorSkip   = 0;

                                    break;
                                }

                                case TrackSubchannelType.Q16Interleaved:
                                {
                                    sectorOffset = 2352;
                                    sectorSize   = 16;
                                    sectorSkip   = 0;

                                    break;
                                }
                                case TrackSubchannelType.None:
                                {
                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                                        throw new
                                            FeatureSupportedButNotImplementedImageException("Unsupported track type");

                                    return new byte[length * 96];
                                }

                                default: throw new ArgumentOutOfRangeException();
                            }

                            break;
                        }
                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                case CommonTypes.Enums.TrackType.CdMode2Form2:
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
                            switch(chars.Subchannel)
                            {
                                case TrackSubchannelType.PackedInterleaved:
                                {
                                    sectorOffset = 2352;
                                    sectorSize   = 96;
                                    sectorSkip   = 0;

                                    break;
                                }

                                case TrackSubchannelType.Q16Interleaved:
                                {
                                    sectorOffset = 2352;
                                    sectorSize   = 16;
                                    sectorSkip   = 0;

                                    break;
                                }
                                case TrackSubchannelType.None:
                                {
                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                                        throw new
                                            FeatureSupportedButNotImplementedImageException("Unsupported track type");

                                    return new byte[length * 96];
                                }

                                default: throw new ArgumentOutOfRangeException();
                            }

                            break;
                        }
                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                case CommonTypes.Enums.TrackType.Audio:
                {
                    switch(tag)
                    {
                        case SectorTagType.CdSectorSubchannel:
                        {
                            switch(chars.Subchannel)
                            {
                                case TrackSubchannelType.PackedInterleaved:
                                {
                                    sectorOffset = 2352;
                                    sectorSize   = 96;
                                    sectorSkip   = 0;

                                    break;
                                }

                                case TrackSubchannelType.Q16Interleaved:
                                {
                                    sectorOffset = 2352;
                                    sectorSize   = 16;
                                    sectorSkip   = 0;

                                    break;
                                }
                                case TrackSubchannelType.None:
                                {
                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                                        throw new
                                            FeatureSupportedButNotImplementedImageException("Unsupported track type");

                                    return new byte[length * 96];
                                }
                                default: throw new ArgumentOutOfRangeException();
                            }

                            break;
                        }
                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            if(tag != SectorTagType.CdSectorSubchannel)
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

            _imageStream = aaruTrack.TrackFilter.GetDataForkStream();
            var br = new BinaryReader(_imageStream);

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

            if(tag != SectorTagType.CdSectorSubchannel)
                return buffer;

            return chars.Subchannel switch
            {
                TrackSubchannelType.Q16Interleaved    => Subchannel.ConvertQToRaw(buffer),
                TrackSubchannelType.PackedInterleaved => Subchannel.Interleave(buffer),
                _                                     => buffer
            };
        }

        /// <inheritdoc />
        public byte[] ReadSectorLong(ulong sectorAddress) => ReadSectorsLong(sectorAddress, 1);

        /// <inheritdoc />
        public byte[] ReadSectorLong(ulong sectorAddress, uint track) => ReadSectorsLong(sectorAddress, 1, track);

        /// <inheritdoc />
        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap where sectorAddress     >= kvp.Value
                                                     from track in Tracks where track.TrackSequence == kvp.Key
                                                     where sectorAddress                                 - kvp.Value <
                                                           track.TrackEndSector - track.TrackStartSector + 1 select kvp)
                return ReadSectorsLong(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        /// <inheritdoc />
        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            // TODO: Cross data files
            var aaruTrack = new Track
            {
                TrackSequence = 0
            };

            foreach(Track bwTrack in Tracks.Where(bwTrack => bwTrack.TrackSequence == track))
            {
                aaruTrack = bwTrack;

                break;
            }

            if(aaruTrack is null)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > aaruTrack.TrackEndSector - aaruTrack.TrackStartSector + 1)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({aaruTrack.TrackEndSector - aaruTrack.TrackStartSector + 1}), won't cross tracks");

            DataFileCharacteristics chars = (from characteristics in _filePaths let firstSector =
                                                 characteristics.StartLba let lastSector =
                                                 firstSector + characteristics.Sectors - 1 let wantedSector =
                                                 (int)(sectorAddress + aaruTrack.TrackStartSector)
                                             where wantedSector >= firstSector && wantedSector <= lastSector
                                             select characteristics).FirstOrDefault();

            if(string.IsNullOrEmpty(chars.FilePath) ||
               chars.FileFilter == null)
                throw new ArgumentOutOfRangeException(nameof(chars.FileFilter), "Track does not exist in disc image");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch(aaruTrack.TrackType)
            {
                case CommonTypes.Enums.TrackType.CdMode1:
                case CommonTypes.Enums.TrackType.CdMode2Formless:
                case CommonTypes.Enums.TrackType.CdMode2Form1:
                case CommonTypes.Enums.TrackType.CdMode2Form2:
                case CommonTypes.Enums.TrackType.Audio:
                {
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 0;

                    break;
                }
                case CommonTypes.Enums.TrackType.Data:
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

            _imageStream = aaruTrack.TrackFilter.GetDataForkStream();
            var br = new BinaryReader(_imageStream);

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

        /// <inheritdoc />
        public List<Track> GetSessionTracks(Session session)
        {
            if(Sessions.Contains(session))
                return GetSessionTracks(session.SessionSequence);

            throw new ImageNotSupportedException("Session does not exist in disc image");
        }

        /// <inheritdoc />
        public List<Track> GetSessionTracks(ushort session) =>
            Tracks.Where(aaruTrack => aaruTrack.TrackSession == session).ToList();
    }
}