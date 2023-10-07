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
using Aaru.Helpers.IO;
using DMI = Aaru.Decoders.Xbox.DMI;
using Sector = Aaru.Decoders.CD.Sector;
using Session = Aaru.CommonTypes.Structs.Session;

namespace Aaru.Images;

public sealed partial class BlindWrite5
{
#region IOpticalMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < 276)
            return ErrorNumber.InvalidArgument;

        var hdr = new byte[260];
        stream.EnsureRead(hdr, 0, 260);
        _header = Marshal.ByteArrayToStructureLittleEndian<Header>(hdr);

        AaruConsole.DebugWriteLine(MODULE_NAME, "header.signature = {0}", StringHandlers.CToString(_header.signature));

        for(var i = 0; i < _header.unknown1.Length; i++)
            AaruConsole.DebugWriteLine(MODULE_NAME, "header.unknown1[{1}] = 0x{0:X8}", _header.unknown1[i], i);

        AaruConsole.DebugWriteLine(MODULE_NAME, "header.profile = {0}",  _header.profile);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.sessions = {0}", _header.sessions);

        for(var i = 0; i < _header.unknown2.Length; i++)
            AaruConsole.DebugWriteLine(MODULE_NAME, "header.unknown2[{1}] = 0x{0:X8}", _header.unknown2[i], i);

        AaruConsole.DebugWriteLine(MODULE_NAME, "header.mcnIsValid = {0}",    _header.mcnIsValid);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.mcn = {0}",           StringHandlers.CToString(_header.mcn));
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.unknown3 = 0x{0:X4}", _header.unknown3);

        for(var i = 0; i < _header.unknown4.Length; i++)
            AaruConsole.DebugWriteLine(MODULE_NAME, "header.unknown4[{1}] = 0x{0:X8}", _header.unknown4[i], i);

        AaruConsole.DebugWriteLine(MODULE_NAME, "header.pmaLen = {0}",    _header.pmaLen);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.atipLen = {0}",   _header.atipLen);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.cdtLen = {0}",    _header.cdtLen);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.cdInfoLen = {0}", _header.cdInfoLen);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.bcaLen = {0}",    _header.bcaLen);

        for(var i = 0; i < _header.unknown5.Length; i++)
            AaruConsole.DebugWriteLine(MODULE_NAME, "header.unknown5[{1}] = 0x{0:X8}", _header.unknown5[i], i);

        AaruConsole.DebugWriteLine(MODULE_NAME, "header.dvdStrLen = {0}",  _header.dvdStrLen);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.dvdInfoLen = {0}", _header.dvdInfoLen);

        for(var i = 0; i < _header.unknown6.Length; i++)
            AaruConsole.DebugWriteLine(MODULE_NAME, "header.unknown6[{1}] = 0x{0:X2}", _header.unknown6[i], i);

        AaruConsole.DebugWriteLine(MODULE_NAME, "header.manufacturer = {0}",
                                   StringHandlers.CToString(_header.manufacturer));

        AaruConsole.DebugWriteLine(MODULE_NAME, "header.product = {0}", StringHandlers.CToString(_header.product));

        AaruConsole.DebugWriteLine(MODULE_NAME, "header.revision = {0}", StringHandlers.CToString(_header.revision));

        AaruConsole.DebugWriteLine(MODULE_NAME, "header.vendor = {0}", StringHandlers.CToString(_header.vendor));

        AaruConsole.DebugWriteLine(MODULE_NAME, "header.volumeId = {0}", StringHandlers.CToString(_header.volumeId));

        AaruConsole.DebugWriteLine(MODULE_NAME, "header.mode2ALen = {0}",   _header.mode2ALen);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.unkBlkLen = {0}",   _header.unkBlkLen);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.dataLen = {0}",     _header.dataLen);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.sessionsLen = {0}", _header.sessionsLen);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.dpmLen = {0}",      _header.dpmLen);

        _mode2A = new byte[_header.mode2ALen];

        if(_mode2A.Length > 0)
        {
            stream.EnsureRead(_mode2A, 0, _mode2A.Length);
            _mode2A[1] -= 2;
            var decoded2A = ModePage_2A.Decode(_mode2A);

            if(decoded2A is not null)
            {
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.mode_page_2A_0,
                                           Modes.PrettifyModePage_2A(decoded2A));
            }
            else
                _mode2A = null;
        }

        _unkBlock = new byte[_header.unkBlkLen];

        if(_unkBlock.Length > 0)
            stream.EnsureRead(_unkBlock, 0, _unkBlock.Length);

        var temp = new byte[_header.pmaLen];

        if(temp.Length > 0)
        {
            byte[] tushort = BitConverter.GetBytes((ushort)(temp.Length + 2));
            stream.EnsureRead(temp, 0, temp.Length);
            _pma    = new byte[temp.Length + 4];
            _pma[0] = tushort[1];
            _pma[1] = tushort[0];
            Array.Copy(temp, 0, _pma, 4, temp.Length);

            PMA.CDPMA? decodedPma = PMA.Decode(_pma);

            if(decodedPma.HasValue)
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.PMA_0, PMA.Prettify(decodedPma));
            else
                _pma = null;
        }

        _atip = new byte[_header.atipLen];

        if(_atip.Length > 0)
            stream.EnsureRead(_atip, 0, _atip.Length);
        else
            _atip = null;

        _cdtext = new byte[_header.cdtLen];

        if(_cdtext.Length > 0)
            stream.EnsureRead(_cdtext, 0, _cdtext.Length);
        else
            _cdtext = null;

        _bca = new byte[_header.bcaLen];

        if(_bca.Length > 0)
            stream.EnsureRead(_bca, 0, _bca.Length);
        else
            _bca = null;

        temp = new byte[_header.dvdStrLen];

        if(temp.Length > 0)
        {
            stream.EnsureRead(temp, 0, temp.Length);
            _dmi = new byte[2052];
            _pfi = new byte[2052];

            // TODO: CMI
            Array.Copy(temp, 2,     _dmi, 4, 2048);
            Array.Copy(temp, 0x802, _pfi, 4, 2048);

            _pfi[0] = 0x08;
            _pfi[1] = 0x02;
            _dmi[0] = 0x08;
            _dmi[1] = 0x02;

            PFI.PhysicalFormatInformation? decodedPfi = PFI.Decode(_pfi, MediaType.DVDROM);

            if(decodedPfi.HasValue)
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.PFI_0, PFI.Prettify(decodedPfi));
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
            stream.EnsureRead(_discInformation, 0, _discInformation.Length);

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Disc_information_0,
                                       PrintHex.ByteArrayToHexArrayString(_discInformation, 40));
        }
        else
            _discInformation = null;

        // How many data blocks
        var tmpArray = new byte[4];
        stream.EnsureRead(tmpArray, 0, tmpArray.Length);
        var dataBlockCount = BitConverter.ToUInt32(tmpArray, 0);

        stream.EnsureRead(tmpArray, 0, tmpArray.Length);
        var dataPathLen   = BitConverter.ToUInt32(tmpArray, 0);
        var dataPathBytes = new byte[dataPathLen];
        stream.EnsureRead(dataPathBytes, 0, dataPathBytes.Length);
        _dataPath = Encoding.Unicode.GetString(dataPathBytes);
        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Data_path_0, _dataPath);

        _dataFiles = new List<DataFile>();

        for(var cD = 0; cD < dataBlockCount; cD++)
        {
            tmpArray = new byte[52];

            var dataFile = new DataFile
            {
                Unknown1 = new uint[4],
                Unknown2 = new uint[3]
            };

            stream.EnsureRead(tmpArray, 0, tmpArray.Length);
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
            stream.EnsureRead(tmpArray, 0, tmpArray.Length);
            dataFile.FilenameBytes = tmpArray;
            tmpArray               = new byte[4];
            stream.EnsureRead(tmpArray, 0, tmpArray.Length);
            dataFile.Unknown3 = BitConverter.ToUInt32(tmpArray, 0);

            dataFile.Filename = Encoding.Unicode.GetString(dataFile.FilenameBytes);
            _dataFiles.Add(dataFile);

            AaruConsole.DebugWriteLine(MODULE_NAME, "dataFile.type = 0x{0:X8}", dataFile.Type);
            AaruConsole.DebugWriteLine(MODULE_NAME, "dataFile.length = {0}",    dataFile.Length);

            for(var i = 0; i < dataFile.Unknown1.Length; i++)
                AaruConsole.DebugWriteLine(MODULE_NAME, "dataFile.unknown1[{1}] = {0}", dataFile.Unknown1[i], i);

            AaruConsole.DebugWriteLine(MODULE_NAME, "dataFile.offset = {0}", dataFile.Offset);

            for(var i = 0; i < dataFile.Unknown2.Length; i++)
                AaruConsole.DebugWriteLine(MODULE_NAME, "dataFile.unknown2[{1}] = {0}", dataFile.Unknown2[i], i);

            AaruConsole.DebugWriteLine(MODULE_NAME, "dataFile.startLba = {0}",    dataFile.StartLba);
            AaruConsole.DebugWriteLine(MODULE_NAME, "dataFile.sectors = {0}",     dataFile.Sectors);
            AaruConsole.DebugWriteLine(MODULE_NAME, "dataFile.filenameLen = {0}", dataFile.FilenameLen);
            AaruConsole.DebugWriteLine(MODULE_NAME, "dataFile.filename = {0}",    dataFile.Filename);
            AaruConsole.DebugWriteLine(MODULE_NAME, "dataFile.unknown3 = {0}",    dataFile.Unknown3);
        }

        _bwSessions = new List<SessionDescriptor>();

        for(var ses = 0; ses < _header.sessions; ses++)
        {
            var session = new SessionDescriptor();
            tmpArray = new byte[16];
            stream.EnsureRead(tmpArray, 0, tmpArray.Length);
            session.Sequence   = BitConverter.ToUInt16(tmpArray, 0);
            session.Entries    = tmpArray[2];
            session.Unknown    = tmpArray[3];
            session.Start      = BitConverter.ToInt32(tmpArray, 4);
            session.End        = BitConverter.ToInt32(tmpArray, 8);
            session.FirstTrack = BitConverter.ToUInt16(tmpArray, 12);
            session.LastTrack  = BitConverter.ToUInt16(tmpArray, 14);
            session.Tracks     = new TrackDescriptor[session.Entries];

            AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].filename = {1}", ses, session.Sequence);
            AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].entries = {1}",  ses, session.Entries);
            AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].unknown = {1}",  ses, session.Unknown);
            AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].start = {1}",    ses, session.Start);
            AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].end = {1}",      ses, session.End);

            AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].firstTrack = {1}", ses, session.FirstTrack);

            AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].lastTrack = {1}", ses, session.LastTrack);

            for(var tSeq = 0; tSeq < session.Entries; tSeq++)
            {
                var trk = new byte[72];
                stream.EnsureRead(trk, 0, 72);
                session.Tracks[tSeq] = Marshal.ByteArrayToStructureLittleEndian<TrackDescriptor>(trk);

                if(session.Tracks[tSeq].type is TrackType.Dvd or TrackType.NotData)
                {
                    session.Tracks[tSeq].unknown9[0] = 0;
                    session.Tracks[tSeq].unknown9[1] = 0;
                    stream.Seek(-8, SeekOrigin.Current);
                }

                AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].track[{1}].type = {2}", ses, tSeq,
                                           session.Tracks[tSeq].type);

                for(var i = 0; i < session.Tracks[tSeq].unknown1.Length; i++)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].track[{1}].unknown1[{2}] = 0x{3:X2}", ses,
                                               tSeq, i, session.Tracks[tSeq].unknown1[i]);
                }

                AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].track[{1}].unknown2 = 0x{2:X8}", ses, tSeq,
                                           session.Tracks[tSeq].unknown2);

                AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].track[{1}].subchannel = {2}", ses, tSeq,
                                           session.Tracks[tSeq].subchannel);

                AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].track[{1}].unknown3 = 0x{2:X2}", ses, tSeq,
                                           session.Tracks[tSeq].unknown3);

                AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].track[{1}].ctl = {2}", ses, tSeq,
                                           session.Tracks[tSeq].ctl);

                AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].track[{1}].adr = {2}", ses, tSeq,
                                           session.Tracks[tSeq].adr);

                AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].track[{1}].point = {2}", ses, tSeq,
                                           session.Tracks[tSeq].point);

                AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].track[{1}].unknown4 = 0x{2:X2}", ses, tSeq,
                                           session.Tracks[tSeq].tno);

                AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].track[{1}].min = {2}", ses, tSeq,
                                           session.Tracks[tSeq].min);

                AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].track[{1}].sec = {2}", ses, tSeq,
                                           session.Tracks[tSeq].sec);

                AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].track[{1}].frame = {2}", ses, tSeq,
                                           session.Tracks[tSeq].frame);

                AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].track[{1}].zero = {2}", ses, tSeq,
                                           session.Tracks[tSeq].zero);

                AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].track[{1}].pmin = {2}", ses, tSeq,
                                           session.Tracks[tSeq].pmin);

                AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].track[{1}].psec = {2}", ses, tSeq,
                                           session.Tracks[tSeq].psec);

                AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].track[{1}].pframe = {2}", ses, tSeq,
                                           session.Tracks[tSeq].pframe);

                AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].track[{1}].unknown5 = 0x{2:X2}", ses, tSeq,
                                           session.Tracks[tSeq].unknown5);

                AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].track[{1}].pregap = {2}", ses, tSeq,
                                           session.Tracks[tSeq].pregap);

                for(var i = 0; i < session.Tracks[tSeq].unknown6.Length; i++)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].track[{1}].unknown6[{2}] = 0x{3:X8}", ses,
                                               tSeq, i, session.Tracks[tSeq].unknown6[i]);
                }

                AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].track[{1}].startLba = {2}", ses, tSeq,
                                           session.Tracks[tSeq].startLba);

                AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].track[{1}].sectors = {2}", ses, tSeq,
                                           session.Tracks[tSeq].sectors);

                for(var i = 0; i < session.Tracks[tSeq].unknown7.Length; i++)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].track[{1}].unknown7[{2}] = 0x{3:X8}", ses,
                                               tSeq, i, session.Tracks[tSeq].unknown7[i]);
                }

                AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].track[{1}].session = {2}", ses, tSeq,
                                           session.Tracks[tSeq].session);

                AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].track[{1}].unknown8 = 0x{2:X4}", ses, tSeq,
                                           session.Tracks[tSeq].unknown8);

                if(session.Tracks[tSeq].type is TrackType.Dvd or TrackType.NotData)
                    continue;

                {
                    for(var i = 0; i < session.Tracks[tSeq].unknown9.Length; i++)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, "session[{0}].track[{1}].unknown9[{2}] = 0x{3:X8}", ses,
                                                   tSeq, i, session.Tracks[tSeq].unknown9[i]);
                    }
                }
            }

            _bwSessions.Add(session);
        }

        _dpm = new byte[_header.dpmLen];
        stream.EnsureRead(_dpm, 0, _dpm.Length);

        // Unused
        tmpArray = new byte[4];
        stream.EnsureRead(tmpArray, 0, tmpArray.Length);

        var footer = new byte[16];
        stream.EnsureRead(footer, 0, footer.Length);

        if(_bw5Footer.SequenceEqual(footer))
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Correctly_arrived_end_of_image);
        else
        {
            AaruConsole.ErrorWriteLine(Localization.
                                           BlindWrite5_image_ends_after_expected_position_Probably_new_version_with_different_data_Errors_may_occur);
        }

        _filePaths = new List<DataFileCharacteristics>();

        foreach(DataFile dataFile in _dataFiles)
        {
            var    chars = new DataFileCharacteristics();
            string path  = Path.Combine(_dataPath, dataFile.Filename);

            if(PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder, path)) != null)
            {
                chars.FileFilter = PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder, path));
                chars.FilePath   = path;
            }
            else
            {
                path = Path.Combine(_dataPath, dataFile.Filename.ToLower(CultureInfo.CurrentCulture));

                if(PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder, path)) != null)
                {
                    chars.FileFilter = PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder, path));
                    chars.FilePath   = path;
                }
                else
                {
                    path = Path.Combine(_dataPath, dataFile.Filename.ToUpper(CultureInfo.CurrentCulture));

                    if(PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder, path)) != null)
                    {
                        chars.FileFilter =
                            PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder, path));
                        chars.FilePath = path;
                    }
                    else
                    {
                        path = Path.Combine(_dataPath.ToLower(CultureInfo.CurrentCulture), dataFile.Filename);

                        if(PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder, path)) != null)
                        {
                            chars.FileFilter =
                                PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder, path));

                            chars.FilePath = path;
                        }
                        else
                        {
                            path = Path.Combine(_dataPath.ToUpper(CultureInfo.CurrentCulture), dataFile.Filename);

                            if(PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder, path)) != null)
                            {
                                chars.FileFilter =
                                    PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder, path));

                                chars.FilePath = path;
                            }
                            else
                            {
                                path = Path.Combine(_dataPath, dataFile.Filename);

                                if(PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                          path.ToLower(CultureInfo.
                                                                              CurrentCulture))) !=
                                   null)
                                {
                                    chars.FilePath = path.ToLower(CultureInfo.CurrentCulture);

                                    chars.FileFilter =
                                        PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                               path.ToLower(CultureInfo.
                                                                                   CurrentCulture)));
                                }
                                else if(PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                               path.ToUpper(CultureInfo.
                                                                                   CurrentCulture))) !=
                                        null)
                                {
                                    chars.FilePath = path.ToUpper(CultureInfo.CurrentCulture);

                                    chars.FileFilter =
                                        PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                               path.ToUpper(CultureInfo.
                                                                                   CurrentCulture)));
                                }
                                else if(PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                               dataFile.Filename.
                                                                                   ToLower(CultureInfo.
                                                                                       CurrentCulture))) !=
                                        null)
                                {
                                    chars.FilePath = dataFile.Filename.ToLower(CultureInfo.CurrentCulture);

                                    chars.FileFilter =
                                        PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                               dataFile.Filename.
                                                                                   ToLower(CultureInfo.
                                                                                       CurrentCulture)));
                                }
                                else if(PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                               dataFile.Filename.
                                                                                   ToUpper(CultureInfo.
                                                                                       CurrentCulture))) !=
                                        null)
                                {
                                    chars.FilePath = dataFile.Filename.ToUpper(CultureInfo.CurrentCulture);

                                    chars.FileFilter =
                                        PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                               dataFile.Filename.
                                                                                   ToUpper(CultureInfo.
                                                                                       CurrentCulture)));
                                }
                                else if(PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                               dataFile.Filename)) !=
                                        null)
                                {
                                    chars.FilePath = dataFile.Filename;

                                    chars.FileFilter =
                                        PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                               dataFile.Filename));
                                }
                                else
                                {
                                    AaruConsole.ErrorWriteLine(Localization.Cannot_find_data_file_0, dataFile.Filename);

                                    continue;
                                }
                            }
                        }
                    }
                }
            }

            long sectorSize = dataFile.Length / dataFile.Sectors;

            if(sectorSize > 2352)
            {
                switch(sectorSize - 2352)
                {
                    case 16:
                        chars.Subchannel = TrackSubchannelType.Q16Interleaved;

                        break;
                    case 96:
                        chars.Subchannel = TrackSubchannelType.PackedInterleaved;

                        break;
                    default:
                        AaruConsole.ErrorWriteLine(Localization.BlindWrite5_found_unknown_subchannel_size_0,
                                                   sectorSize - 2352);

                        return ErrorNumber.NotSupported;
                }
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
        var  isDvd        = false;
        byte firstSession = byte.MaxValue;
        byte lastSession  = 0;
        _trackFlags        = new Dictionary<uint, byte>();
        _imageInfo.Sectors = 0;

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Building_maps);

        foreach(SessionDescriptor ses in _bwSessions)
        {
            Sessions.Add(new Session
            {
                Sequence    = ses.Sequence,
                StartSector = ses.Start < 0 ? 0 : (ulong)ses.Start,
                EndSector   = (ulong)ses.End - 1,
                StartTrack  = ses.FirstTrack,
                EndTrack    = ses.LastTrack
            });

            if(ses.Sequence < firstSession)
                firstSession = (byte)ses.Sequence;

            if(ses.Sequence > lastSession)
                lastSession = (byte)ses.Sequence;

            foreach(TrackDescriptor trk in ses.Tracks)
            {
                var adrCtl = (byte)((trk.adr << 4) + trk.ctl);
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
                        track.BytesPerSector    = 2352;
                        track.RawBytesPerSector = 2352;

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

                        track.BytesPerSector    = 2048;
                        track.RawBytesPerSector = 2352;

                        if(_imageInfo.SectorSize < 2048)
                            _imageInfo.SectorSize = 2048;

                        break;
                    case TrackType.Mode2:
                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                        track.BytesPerSector    = 2336;
                        track.RawBytesPerSector = 2352;

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

                        track.BytesPerSector    = 2336;
                        track.RawBytesPerSector = 2352;

                        if(_imageInfo.SectorSize < 2324)
                            _imageInfo.SectorSize = 2324;

                        break;
                    case TrackType.Dvd:
                        track.BytesPerSector    = 2048;
                        track.RawBytesPerSector = 2048;

                        if(_imageInfo.SectorSize < 2048)
                            _imageInfo.SectorSize = 2048;

                        isDvd = true;

                        break;
                }

                track.Description = string.Format(Localization.Track_0, trk.point);
                track.StartSector = (ulong)(trk.startLba + trk.pregap);
                track.EndSector   = (ulong)(trk.sectors + trk.startLba) - 1;

                var fileCharsForThisTrack = _filePaths.
                                            Where(chars => trk.startLba >= chars.StartLba &&
                                                           trk.startLba   + trk.sectors <=
                                                           chars.StartLba + chars.Sectors).
                                            ToList();

                if(fileCharsForThisTrack.Count == 0 &&
                   _filePaths.Any(f => Path.GetExtension(f.FilePath).ToLowerInvariant() == ".b00"))
                {
                    DataFileCharacteristics splitStartChars =
                        _filePaths.FirstOrDefault(f => Path.GetExtension(f.FilePath).ToLowerInvariant() == ".b00");

                    string filename           = Path.GetFileNameWithoutExtension(splitStartChars.FilePath);
                    var    lowerCaseExtension = false;
                    var    lowerCaseFileName  = false;
                    string basePath;

                    bool version5 = string.Compare(Path.GetExtension(imageFilter.Filename), ".B5T",
                                                   StringComparison.OrdinalIgnoreCase) ==
                                    0;

                    string firstExtension      = version5 ? "B5I" : "B6I";
                    string firstExtensionLower = version5 ? "b5i" : "b6i";

                    if(File.Exists(Path.Combine(imageFilter.ParentFolder, $"{filename}.{firstExtension}")))
                        basePath = imageFilter.ParentFolder;
                    else if(File.Exists(Path.Combine(imageFilter.ParentFolder, $"{filename}.{firstExtensionLower}")))
                    {
                        basePath           = imageFilter.ParentFolder;
                        lowerCaseExtension = true;
                    }
                    else if(File.Exists(Path.Combine(imageFilter.ParentFolder,
                                                     $"{filename.ToLower(CultureInfo.CurrentCulture)}.{firstExtension
                                                     }")))
                    {
                        basePath          = imageFilter.ParentFolder;
                        lowerCaseFileName = true;
                    }
                    else if(File.Exists(Path.Combine(imageFilter.ParentFolder,
                                                     $"{filename.ToLower(CultureInfo.CurrentCulture)}.{
                                                         firstExtensionLower}")))
                    {
                        basePath           = imageFilter.ParentFolder;
                        lowerCaseFileName  = true;
                        lowerCaseExtension = true;
                    }

                    else if(File.Exists(Path.Combine(_dataPath, $"{filename}.{firstExtension}")))
                        basePath = _dataPath;
                    else if(File.Exists(Path.Combine(_dataPath, $"{filename}.{firstExtensionLower}")))
                    {
                        basePath           = _dataPath;
                        lowerCaseExtension = true;
                    }
                    else if(File.Exists(Path.Combine(_dataPath,
                                                     $"{filename.ToLower(CultureInfo.CurrentCulture)}.{firstExtension
                                                     }")))
                    {
                        basePath          = _dataPath;
                        lowerCaseFileName = true;
                    }
                    else if(File.Exists(Path.Combine(_dataPath, $"{filename.ToLower(CultureInfo.CurrentCulture)}.{
                        firstExtensionLower}")))
                    {
                        basePath           = _dataPath;
                        lowerCaseFileName  = true;
                        lowerCaseExtension = true;
                    }

                    else if(File.Exists(Path.Combine(_dataPath.ToLower(CultureInfo.CurrentCulture),
                                                     $"{filename}.{firstExtension}")))
                        basePath = _dataPath.ToLower(CultureInfo.CurrentCulture);
                    else if(File.Exists(Path.Combine(_dataPath.ToLower(CultureInfo.CurrentCulture),
                                                     $"{filename}.{firstExtensionLower}")))
                    {
                        basePath           = _dataPath.ToLower(CultureInfo.CurrentCulture);
                        lowerCaseExtension = true;
                    }
                    else if(File.Exists(Path.Combine(_dataPath.ToLower(CultureInfo.CurrentCulture),
                                                     $"{filename.ToLower(CultureInfo.CurrentCulture)}.{firstExtension
                                                     }")))
                    {
                        basePath          = _dataPath.ToLower(CultureInfo.CurrentCulture);
                        lowerCaseFileName = true;
                    }
                    else if(File.Exists(Path.Combine(_dataPath.ToLower(CultureInfo.CurrentCulture),
                                                     $"{filename.ToLower(CultureInfo.CurrentCulture)}.{
                                                         firstExtensionLower}")))
                    {
                        basePath           = _dataPath.ToLower(CultureInfo.CurrentCulture);
                        lowerCaseFileName  = true;
                        lowerCaseExtension = true;
                    }

                    else if(File.Exists(Path.Combine(imageFilter.ParentFolder, _dataPath,
                                                     $"{filename}.{firstExtension}")))
                        basePath = Path.Combine(imageFilter.ParentFolder, _dataPath);
                    else if(File.Exists(Path.Combine(imageFilter.ParentFolder, _dataPath,
                                                     $"{filename}.{firstExtensionLower}")))
                    {
                        basePath           = Path.Combine(imageFilter.ParentFolder, _dataPath);
                        lowerCaseExtension = true;
                    }
                    else if(File.Exists(Path.Combine(imageFilter.ParentFolder, _dataPath,
                                                     $"{filename.ToLower(CultureInfo.CurrentCulture)}.{firstExtension
                                                     }")))
                    {
                        basePath          = Path.Combine(imageFilter.ParentFolder, _dataPath);
                        lowerCaseFileName = true;
                    }
                    else if(File.Exists(Path.Combine(imageFilter.ParentFolder, _dataPath,
                                                     $"{filename.ToLower(CultureInfo.CurrentCulture)}.{
                                                         firstExtensionLower}")))
                    {
                        basePath           = Path.Combine(imageFilter.ParentFolder, _dataPath);
                        lowerCaseFileName  = true;
                        lowerCaseExtension = true;
                    }

                    else if(File.Exists(Path.Combine(imageFilter.ParentFolder,
                                                     _dataPath.ToLower(CultureInfo.CurrentCulture),
                                                     $"{filename}.{firstExtension}")))
                    {
                        basePath = Path.Combine(imageFilter.ParentFolder,
                                                _dataPath.ToLower(CultureInfo.CurrentCulture));
                    }
                    else if(File.Exists(Path.Combine(imageFilter.ParentFolder,
                                                     _dataPath.ToLower(CultureInfo.CurrentCulture), $"{filename}.b00")))
                    {
                        basePath = Path.Combine(imageFilter.ParentFolder,
                                                _dataPath.ToLower(CultureInfo.CurrentCulture));

                        lowerCaseExtension = true;
                    }
                    else if(File.Exists(Path.Combine(imageFilter.ParentFolder,
                                                     _dataPath.ToLower(CultureInfo.CurrentCulture),
                                                     $"{filename.ToLower(CultureInfo.CurrentCulture)}.{firstExtension
                                                     }")))
                    {
                        basePath = Path.Combine(imageFilter.ParentFolder,
                                                _dataPath.ToLower(CultureInfo.CurrentCulture));

                        lowerCaseFileName = true;
                    }
                    else if(File.Exists(Path.Combine(imageFilter.ParentFolder,
                                                     _dataPath.ToLower(CultureInfo.CurrentCulture),
                                                     $"{filename.ToLower(CultureInfo.CurrentCulture)}.{
                                                         firstExtensionLower}")))
                    {
                        basePath = Path.Combine(imageFilter.ParentFolder,
                                                _dataPath.ToLower(CultureInfo.CurrentCulture));

                        lowerCaseFileName  = true;
                        lowerCaseExtension = true;
                    }
                    else if(File.Exists(Path.Combine("", $"{filename}.{firstExtension}")))
                        basePath = "";
                    else if(File.Exists(Path.Combine("", $"{filename}.{firstExtensionLower}")))
                    {
                        basePath           = "";
                        lowerCaseExtension = true;
                    }
                    else if(File.Exists(Path.Combine("",
                                                     $"{filename.ToLower(CultureInfo.CurrentCulture)}.{firstExtension
                                                     }")))
                    {
                        basePath          = "";
                        lowerCaseFileName = true;
                    }
                    else if(File.Exists(Path.Combine("", $"{filename.ToLower(CultureInfo.CurrentCulture)}.{
                        firstExtensionLower}")))
                    {
                        basePath           = "";
                        lowerCaseFileName  = true;
                        lowerCaseExtension = true;
                    }
                    else
                    {
                        AaruConsole.ErrorWriteLine(Localization.Could_not_find_image_for_track_0, trk.point);

                        return ErrorNumber.NoSuchFile;
                    }

                    var splitStream = new SplitJoinStream();

                    if(lowerCaseFileName)
                        filename = filename.ToLower(CultureInfo.CurrentCulture);

                    string extension = lowerCaseExtension ? "b{0:D2}" : "B{0:D2}";

                    try
                    {
                        splitStream.
                            Add(Path.Combine(basePath, $"{filename}.{(lowerCaseExtension ? firstExtensionLower : firstExtension)}"),
                                FileMode.Open, FileAccess.Read);

                        splitStream.AddRange(basePath, $"{filename}.{extension}");
                    }
                    catch(Exception)
                    {
                        AaruConsole.ErrorWriteLine(Localization.Could_not_find_image_for_track_0, trk.point);

                        return ErrorNumber.NoSuchFile;
                    }

                    track.Filter = new ZZZNoFilter();
                    track.Filter.Open(splitStream);
                    track.File = $"{filename}.{extension}";

                    if(trk.startLba >= 0)
                        track.FileOffset = (ulong)(trk.startLba * splitStartChars.SectorSize + splitStartChars.Offset);
                    else
                        track.FileOffset = (ulong)(trk.startLba * -1 * splitStartChars.SectorSize);

                    track.FileType = "BINARY";

                    if(splitStartChars.Subchannel != TrackSubchannelType.None)
                    {
                        track.SubchannelFilter = track.Filter;
                        track.SubchannelFile   = track.File;
                        track.SubchannelType   = splitStartChars.Subchannel;
                        track.SubchannelOffset = track.FileOffset;

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);
                    }

                    splitStartChars.FileFilter = new ZZZNoFilter();
                    splitStartChars.FileFilter.Open(splitStream);
                    splitStartChars.Sectors  = trk.sectors;
                    splitStartChars.StartLba = trk.startLba;
                    _filePaths.Clear();
                    _filePaths.Add(splitStartChars);
                }
                else
                {
                    foreach(DataFileCharacteristics chars in fileCharsForThisTrack)
                    {
                        track.Filter = chars.FileFilter;
                        track.File   = chars.FileFilter.Filename;

                        if(trk.startLba >= 0)
                        {
                            track.FileOffset = (ulong)((trk.startLba - chars.StartLba) * chars.SectorSize) +
                                               chars.Offset;
                        }
                        else
                            track.FileOffset = (ulong)(trk.startLba * -1 * chars.SectorSize);

                        track.FileType = "BINARY";

                        if(chars.Subchannel != TrackSubchannelType.None)
                        {
                            track.SubchannelFilter = track.Filter;
                            track.SubchannelFile   = track.File;
                            track.SubchannelType   = chars.Subchannel;
                            track.SubchannelOffset = track.FileOffset;

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);
                        }

                        break;
                    }
                }

                if(track.Filter is null)
                {
                    AaruConsole.ErrorWriteLine(Localization.Could_not_find_image_for_track_0, trk.point);

                    return ErrorNumber.NoSuchFile;
                }

                track.Pregap   = trk.pregap;
                track.Sequence = trk.point;
                track.Type     = BlindWriteTrackTypeToTrackType(trk.type);

                if(trk.pregap > 0 && track.StartSector > 0)
                {
                    track.Indexes[0] = (int)track.StartSector - (int)trk.pregap;

                    if(track.Indexes[0] < 0)
                        track.Indexes[0] = 0;
                }

                track.Indexes[1] = (int)track.StartSector;

                partition.Description = track.Description;

                partition.Size = (track.EndSector - track.StartSector) * (ulong)track.RawBytesPerSector;

                partition.Length   = track.EndSector - track.StartSector + 1;
                partition.Sequence = track.Sequence;
                partition.Offset   = offsetBytes;
                partition.Start    = track.StartSector;
                partition.Type     = track.Type.ToString();

                offsetBytes += partition.Size;

                if(track.StartSector >= trk.pregap)
                    track.StartSector -= trk.pregap;

                if(track.EndSector > _imageInfo.Sectors)
                    _imageInfo.Sectors = track.EndSector + 1;

                Tracks.Add(track);
                Partitions.Add(partition);
                _offsetMap.Add(track.Sequence, track.StartSector);
            }
        }

        foreach(Track track in Tracks)
        {
            Session trackSession =
                Sessions.FirstOrDefault(s => track.Sequence >= s.StartTrack && track.Sequence <= s.EndTrack);

            track.Session = trackSession.Sequence;
        }

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.printing_track_map);

        foreach(Track track in Tracks)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Partition_sequence_0, track.Sequence);

            AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Track_description_0, track.Description);

            AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Track_type_0, track.Type);

            AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Track_starting_sector_0, track.StartSector);

            AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Track_ending_sector_0, track.EndSector);
        }

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.printing_partition_map);

        foreach(Partition partition in Partitions)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Partition_sequence_0,    partition.Sequence);
            AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Partition_name_0, partition.Name);

            AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Partition_description_0, partition.Description);

            AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Partition_type_0, partition.Type);

            AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Partition_starting_sector_0, partition.Start);

            AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Partition_sectors_0, partition.Length);

            AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Partition_starting_offset_0, partition.Offset);

            AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Partition_size_in_bytes_0, partition.Size);
        }

        if(!isDvd)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Rebuilding_TOC);

            _fullToc = fullTocStream.ToArray();
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.TOC_len_0, _fullToc.Length);

            _fullToc[0] = firstSession;
            _fullToc[1] = lastSession;

            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackFlags);
        }

        _imageInfo.MediaType = BlindWriteProfileToMediaType(_header.profile);

        if(_dmi != null && _pfi != null)
        {
            PFI.PhysicalFormatInformation? pfi0 = PFI.Decode(_pfi, _imageInfo.MediaType);

            // All discs I tested the disk category and part version (as well as the start PSN for DVD-RAM) where modified by Alcohol
            // So much for archival value
            if(pfi0.HasValue)
            {
                _imageInfo.MediaType = pfi0.Value.DiskCategory switch
                                       {
                                           DiskCategory.DVDPR    => MediaType.DVDPR,
                                           DiskCategory.DVDPRDL  => MediaType.DVDPRDL,
                                           DiskCategory.DVDPRW   => MediaType.DVDPRW,
                                           DiskCategory.DVDPRWDL => MediaType.DVDPRWDL,
                                           DiskCategory.DVDR => pfi0.Value.PartVersion >= 6
                                                                    ? MediaType.DVDRDL
                                                                    : MediaType.DVDR,
                                           DiskCategory.DVDRAM => MediaType.DVDRAM,
                                           DiskCategory.DVDRW => pfi0.Value.PartVersion >= 15
                                                                     ? MediaType.DVDRWDL
                                                                     : MediaType.DVDRW,
                                           DiskCategory.HDDVDR   => MediaType.HDDVDR,
                                           DiskCategory.HDDVDRAM => MediaType.HDDVDRAM,
                                           DiskCategory.HDDVDROM => MediaType.HDDVDROM,
                                           DiskCategory.HDDVDRW  => MediaType.HDDVDRW,
                                           DiskCategory.Nintendo => pfi0.Value.DiscSize == DVDSize.Eighty
                                                                        ? MediaType.GOD
                                                                        : MediaType.WOD,
                                           DiskCategory.UMD => MediaType.UMD,
                                           _                => MediaType.DVDROM
                                       };

                if(DMI.IsXbox(_dmi))
                    _imageInfo.MediaType = MediaType.XGD;
                else if(DMI.IsXbox360(_dmi))
                    _imageInfo.MediaType = MediaType.XGD2;
            }
        }
        else if(_imageInfo.MediaType is MediaType.CD or MediaType.CDROM)
        {
            var data       = false;
            var mode2      = false;
            var firstAudio = false;
            var firstData  = false;
            var audio      = false;

            foreach(Track bwTrack in Tracks)
            {
                // First track is audio
                firstAudio |= bwTrack.Sequence == 1 && bwTrack.Type == CommonTypes.Enums.TrackType.Audio;

                // First track is data
                firstData |= bwTrack.Sequence == 1 && bwTrack.Type != CommonTypes.Enums.TrackType.Audio;

                // Any non first track is data
                data |= bwTrack.Sequence != 1 && bwTrack.Type != CommonTypes.Enums.TrackType.Audio;

                // Any non first track is audio
                audio |= bwTrack.Sequence != 1 && bwTrack.Type == CommonTypes.Enums.TrackType.Audio;

                switch(bwTrack.Type)
                {
                    case CommonTypes.Enums.TrackType.CdMode2Formless:
                    case CommonTypes.Enums.TrackType.CdMode2Form1:
                    case CommonTypes.Enums.TrackType.CdMode2Form2:
                        mode2 = true;

                        break;
                }
            }

            if(!data && !firstData)
                _imageInfo.MediaType = MediaType.CDDA;
            else if(firstAudio && data && Sessions.Count > 1 && mode2)
                _imageInfo.MediaType = MediaType.CDPLUS;
            else if(firstData && audio || mode2)
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

        if(string.Compare(Path.GetExtension(imageFilter.Filename), ".B5T", StringComparison.OrdinalIgnoreCase) == 0)
            _imageInfo.ApplicationVersion = "5";
        else if(string.Compare(Path.GetExtension(imageFilter.Filename), ".B6T", StringComparison.OrdinalIgnoreCase) ==
                0)
            _imageInfo.ApplicationVersion = "6";

        _imageInfo.Version = "5";

        _imageInfo.ImageSize            = (ulong)imageFilter.DataForkLength;
        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MetadataMediaType    = MetadataMediaType.OpticalDisc;

        if(_pma != null)
        {
            PMA.CDPMA pma0 = PMA.Decode(_pma).Value;

            foreach(uint id in from descriptor in pma0.PMADescriptors
                               where descriptor.ADR == 2
                               select (uint)((descriptor.Min << 16) + (descriptor.Sec << 8) + descriptor.Frame))
                _imageInfo.MediaSerialNumber = $"{id & 0x00FFFFFF:X6}";
        }

        if(_atip != null)
        {
            var atipTmp = new byte[_atip.Length + 4];
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

        var isBd = false;

        if(_imageInfo.MediaType is MediaType.BDR or MediaType.BDRE or MediaType.BDROM)
        {
            isDvd = false;
            isBd  = true;
        }

        if(isBd && _imageInfo.Sectors > 24438784)
        {
            _imageInfo.MediaType = _imageInfo.MediaType switch
                                   {
                                       MediaType.BDR  => MediaType.BDRXL,
                                       MediaType.BDRE => MediaType.BDREXL,
                                       _              => _imageInfo.MediaType
                                   };
        }

        AaruConsole.DebugWriteLine(MODULE_NAME, "ImageInfo.mediaType = {0}", _imageInfo.MediaType);

        if(_mode2A != null)
            _imageInfo.ReadableMediaTags.Add(MediaTagType.SCSI_MODEPAGE_2A);

        if(_pma != null)
            _imageInfo.ReadableMediaTags.Add(MediaTagType.CD_PMA);

        if(_atip != null)
            _imageInfo.ReadableMediaTags.Add(MediaTagType.CD_ATIP);

        if(_cdtext != null)
            _imageInfo.ReadableMediaTags.Add(MediaTagType.CD_TEXT);

        if(_bca != null)
        {
            if(isDvd)
                _imageInfo.ReadableMediaTags.Add(MediaTagType.DVD_BCA);
            else if(isBd)
                _imageInfo.ReadableMediaTags.Add(MediaTagType.BD_BCA);
        }

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

        if(_imageInfo is { MediaType: MediaType.XGD2, Sectors: 25063 or 4229664 or 4246304 })

            // Wxripper unlock
            _imageInfo.MediaType = MediaType.XGD3;

        AaruConsole.VerboseWriteLine(Localization.BlindWrite_image_describes_a_disc_of_type_0, _imageInfo.MediaType);

        if(_header.profile is ProfileNumber.CDR or ProfileNumber.CDRW or ProfileNumber.CDROM)
            return ErrorNumber.NoError;

        foreach(Track track in Tracks)
        {
            track.Pregap = 0;
            track.Indexes?.Clear();
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadMediaTag(MediaTagType tag, out byte[] buffer)
    {
        buffer = null;

        switch(tag)
        {
            case MediaTagType.SCSI_MODEPAGE_2A:
                buffer = _mode2A?.Clone() as byte[];

                return buffer != null ? ErrorNumber.NoError : ErrorNumber.NoData;
            case MediaTagType.CD_PMA:
                buffer = _pma?.Clone() as byte[];

                return buffer != null ? ErrorNumber.NoError : ErrorNumber.NoData;
            case MediaTagType.CD_ATIP:
                buffer = _atip?.Clone() as byte[];

                return buffer != null ? ErrorNumber.NoError : ErrorNumber.NoData;
            case MediaTagType.CD_TEXT:
                buffer = _cdtext?.Clone() as byte[];

                return buffer != null ? ErrorNumber.NoError : ErrorNumber.NoData;
            case MediaTagType.DVD_BCA:
            case MediaTagType.BD_BCA:
                buffer = _bca?.Clone() as byte[];

                return buffer != null ? ErrorNumber.NoError : ErrorNumber.NoData;
            case MediaTagType.DVD_PFI:
                buffer = _pfi?.Clone() as byte[];

                return buffer != null ? ErrorNumber.NoError : ErrorNumber.NoData;
            case MediaTagType.DVD_DMI:
                buffer = _dmi?.Clone() as byte[];

                return buffer != null ? ErrorNumber.NoError : ErrorNumber.NoData;
            case MediaTagType.CD_FullTOC:
                buffer = _fullToc?.Clone() as byte[];

                return buffer != null ? ErrorNumber.NoError : ErrorNumber.NoData;
            default:
                return ErrorNumber.NotSupported;
        }
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer) => ReadSectors(sectorAddress, 1, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, SectorTagType tag, out byte[] buffer) =>
        ReadSectorsTag(sectorAddress, 1, tag, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, uint track, out byte[] buffer) =>
        ReadSectors(sectorAddress, 1, track, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag, out byte[] buffer) =>
        ReadSectorsTag(sectorAddress, 1, track, tag, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap
                                                 where sectorAddress >= kvp.Value
                                                 from track in Tracks
                                                 where track.Sequence == kvp.Key
                                                 where sectorAddress                       - kvp.Value <
                                                       track.EndSector - track.StartSector + 1
                                                 select kvp)
            return ReadSectors(sectorAddress - kvp.Value, length, kvp.Key, out buffer);

        return ErrorNumber.SectorNotFound;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap
                                                 where sectorAddress >= kvp.Value
                                                 from track in Tracks
                                                 where track.Sequence == kvp.Key
                                                 where sectorAddress                       - kvp.Value <
                                                       track.EndSector - track.StartSector + 1
                                                 select kvp)
            return ReadSectorsTag(sectorAddress - kvp.Value, length, kvp.Key, tag, out buffer);

        return ErrorNumber.SectorNotFound;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, uint track, out byte[] buffer)
    {
        buffer = null;

        // TODO: Cross data files
        Track aaruTrack = Tracks.FirstOrDefault(bwTrack => bwTrack.Sequence == track);

        if(aaruTrack is null)
            return ErrorNumber.SectorNotFound;

        if(length + sectorAddress > aaruTrack.EndSector - aaruTrack.StartSector + 1)
            return ErrorNumber.OutOfRange;

        DataFileCharacteristics chars = (from characteristics in _filePaths
                                         let firstSector = characteristics.StartLba
                                         let lastSector = firstSector + characteristics.Sectors - 1
                                         let wantedSector = (int)(sectorAddress + aaruTrack.StartSector)
                                         where wantedSector >= firstSector && wantedSector <= lastSector
                                         select characteristics).FirstOrDefault();

        if(string.IsNullOrEmpty(chars.FilePath) || chars.FileFilter == null)
            return ErrorNumber.SectorNotFound;

        uint sectorOffset;
        uint sectorSize;
        uint sectorSkip;
        var  mode2 = false;

        switch(aaruTrack.Type)
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
            default:
                return ErrorNumber.NotSupported;
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
            default:
                return ErrorNumber.NotSupported;
        }

        buffer = new byte[sectorSize * length];

        _imageStream = chars.FileFilter.GetDataForkStream();
        var br = new BinaryReader(_imageStream);

        br.BaseStream.
           Seek((long)aaruTrack.FileOffset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                SeekOrigin.Begin);

        if(mode2)
        {
            var mode2Ms = new MemoryStream((int)(sectorSize * length));

            buffer = br.ReadBytes((int)((sectorSize + sectorSkip) * length));

            for(var i = 0; i < length; i++)
            {
                var sector = new byte[sectorSize];
                Array.Copy(buffer, (sectorSize + sectorSkip) * i, sector, 0, sectorSize);
                sector = Sector.GetUserDataFromMode2(sector);
                mode2Ms.Write(sector, 0, sector.Length);
            }

            buffer = mode2Ms.ToArray();
        }
        else if(sectorOffset == 0 && sectorSkip == 0)
            buffer = br.ReadBytes((int)(sectorSize * length));
        else
        {
            for(var i = 0; i < length; i++)
            {
                br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                byte[] sector = br.ReadBytes((int)sectorSize);
                br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
            }
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong      sectorAddress, uint length, uint track, SectorTagType tag,
                                      out byte[] buffer)
    {
        buffer = null;

        // TODO: Cross data files
        Track aaruTrack = Tracks.FirstOrDefault(bwTrack => bwTrack.Sequence == track);

        if(aaruTrack is null)
            return ErrorNumber.SectorNotFound;

        if(length + sectorAddress > aaruTrack.EndSector - aaruTrack.StartSector + 1)
            return ErrorNumber.OutOfRange;

        DataFileCharacteristics chars = (from characteristics in _filePaths
                                         let firstSector = characteristics.StartLba
                                         let lastSector = firstSector + characteristics.Sectors - 1
                                         let wantedSector = (int)(sectorAddress + aaruTrack.StartSector)
                                         where wantedSector >= firstSector && wantedSector <= lastSector
                                         select characteristics).FirstOrDefault();

        if(string.IsNullOrEmpty(chars.FilePath) || chars.FileFilter == null)
            return ErrorNumber.SectorNotFound;

        if(aaruTrack.Type == CommonTypes.Enums.TrackType.Data)
            return ErrorNumber.NotSupported;

        switch(tag)
        {
            case SectorTagType.CdSectorEcc:
            case SectorTagType.CdSectorEccP:
            case SectorTagType.CdSectorEccQ:
            case SectorTagType.CdSectorEdc:
            case SectorTagType.CdSectorHeader:
            case SectorTagType.CdSectorSubchannel:
            case SectorTagType.CdSectorSubHeader:
            case SectorTagType.CdSectorSync:
                break;
            case SectorTagType.CdTrackFlags:
                if(!_trackFlags.TryGetValue((uint)sectorAddress, out byte flag))
                    return ErrorNumber.NoData;

                buffer = new[]
                {
                    flag
                };

                return ErrorNumber.NoError;
            default:
                return ErrorNumber.NotSupported;
        }

        uint sectorOffset;
        uint sectorSize;
        uint sectorSkip;

        switch(aaruTrack.Type)
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
                        return ErrorNumber.NotSupported;
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
                                    return ErrorNumber.NotSupported;

                                buffer = new byte[length * 96];

                                return ErrorNumber.NoError;
                            }

                            default:
                                return ErrorNumber.NotSupported;
                        }

                        break;
                    }
                    default:
                        return ErrorNumber.NotSupported;
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
                        return ErrorNumber.NotSupported;
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
                                    return ErrorNumber.NotSupported;

                                buffer = new byte[length * 96];

                                return ErrorNumber.NoError;
                            }

                            default:
                                return ErrorNumber.NotSupported;
                        }

                        break;
                    }
                    default:
                        return ErrorNumber.NotSupported;
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
                                    return ErrorNumber.NotSupported;

                                buffer = new byte[length * 96];

                                return ErrorNumber.NoError;
                            }

                            default:
                                return ErrorNumber.NotSupported;
                        }

                        break;
                    }
                    default:
                        return ErrorNumber.NotSupported;
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
                                    return ErrorNumber.NotSupported;

                                buffer = new byte[length * 96];

                                return ErrorNumber.NoError;
                            }

                            default:
                                return ErrorNumber.NotSupported;
                        }

                        break;
                    }
                    default:
                        return ErrorNumber.NotSupported;
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
                                    return ErrorNumber.NotSupported;

                                buffer = new byte[length * 96];

                                return ErrorNumber.NoError;
                            }
                            default:
                                return ErrorNumber.NotSupported;
                        }

                        break;
                    }
                    default:
                        return ErrorNumber.NotSupported;
                }

                break;
            }
            default:
                return ErrorNumber.NotSupported;
        }

        if(tag != SectorTagType.CdSectorSubchannel)
        {
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
                default:
                    return ErrorNumber.NotSupported;
            }
        }

        buffer = new byte[sectorSize * length];

        _imageStream = aaruTrack.Filter.GetDataForkStream();
        var br = new BinaryReader(_imageStream);

        br.BaseStream.
           Seek((long)aaruTrack.FileOffset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                SeekOrigin.Begin);

        if(sectorOffset == 0 && sectorSkip == 0)
            buffer = br.ReadBytes((int)(sectorSize * length));
        else
        {
            for(var i = 0; i < length; i++)
            {
                br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                byte[] sector = br.ReadBytes((int)sectorSize);
                br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
            }
        }

        if(tag != SectorTagType.CdSectorSubchannel)
            return ErrorNumber.NoError;

        buffer = chars.Subchannel switch
                 {
                     TrackSubchannelType.Q16Interleaved    => Subchannel.ConvertQToRaw(buffer),
                     TrackSubchannelType.PackedInterleaved => Subchannel.Interleave(buffer),
                     _                                     => buffer
                 };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong sectorAddress, out byte[] buffer) =>
        ReadSectorsLong(sectorAddress, 1, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong sectorAddress, uint track, out byte[] buffer) =>
        ReadSectorsLong(sectorAddress, 1, track, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap
                                                 where sectorAddress >= kvp.Value
                                                 from track in Tracks
                                                 where track.Sequence == kvp.Key
                                                 where sectorAddress                       - kvp.Value <
                                                       track.EndSector - track.StartSector + 1
                                                 select kvp)
            return ReadSectorsLong(sectorAddress - kvp.Value, length, kvp.Key, out buffer);

        return ErrorNumber.SectorNotFound;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong sectorAddress, uint length, uint track, out byte[] buffer)
    {
        buffer = null;

        // TODO: Cross data files
        Track aaruTrack = Tracks.FirstOrDefault(bwTrack => bwTrack.Sequence == track);

        if(aaruTrack is null)
            return ErrorNumber.SectorNotFound;

        if(length + sectorAddress > aaruTrack.EndSector - aaruTrack.StartSector + 1)
            return ErrorNumber.OutOfRange;

        DataFileCharacteristics chars = (from characteristics in _filePaths
                                         let firstSector = characteristics.StartLba
                                         let lastSector = firstSector + characteristics.Sectors - 1
                                         let wantedSector = (int)(sectorAddress + aaruTrack.StartSector)
                                         where wantedSector >= firstSector && wantedSector <= lastSector
                                         select characteristics).FirstOrDefault();

        if(string.IsNullOrEmpty(chars.FilePath) || chars.FileFilter == null)
            return ErrorNumber.SectorNotFound;

        uint sectorOffset;
        uint sectorSize;
        uint sectorSkip;

        switch(aaruTrack.Type)
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
            default:
                return ErrorNumber.NotSupported;
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
            default:
                return ErrorNumber.NotSupported;
        }

        buffer = new byte[sectorSize * length];

        _imageStream = aaruTrack.Filter.GetDataForkStream();
        var br = new BinaryReader(_imageStream);

        br.BaseStream.Seek((long)aaruTrack.FileOffset + (long)(sectorAddress * (sectorSize + sectorSkip)),
                           SeekOrigin.Begin);

        if(sectorSkip == 0)
            buffer = br.ReadBytes((int)(sectorSize * length));
        else
        {
            for(var i = 0; i < length; i++)
            {
                br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                byte[] sector = br.ReadBytes((int)sectorSize);
                br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
            }
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public List<Track> GetSessionTracks(Session session) =>
        Sessions.Contains(session) ? GetSessionTracks(session.Sequence) : null;

    /// <inheritdoc />
    public List<Track> GetSessionTracks(ushort session) =>
        Tracks.Where(aaruTrack => aaruTrack.Session == session).ToList();

#endregion
}