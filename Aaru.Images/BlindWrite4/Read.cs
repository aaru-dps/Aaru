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
//     Reads BlindWrite 4 disc images.
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
using Aaru.Console;
using Aaru.Decoders.CD;
using Aaru.Helpers;
using Session = Aaru.CommonTypes.Structs.Session;

namespace Aaru.Images;

public sealed partial class BlindWrite4
{
#region IOpticalMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < 19)
            return ErrorNumber.InvalidArgument;

        var tmpArray  = new byte[19];
        var tmpUShort = new byte[2];
        var tmpUInt   = new byte[4];
        var tmpULong  = new byte[8];

        stream.EnsureRead(tmpArray, 0, 19);

        if(!_bw4Signature.SequenceEqual(tmpArray))
            return ErrorNumber.InvalidArgument;

        _header = new Header
        {
            Signature = tmpArray
        };

        // Seems to always be 2
        stream.EnsureRead(tmpUInt, 0, 4);
        _header.Unknown1 = BitConverter.ToUInt32(tmpUInt, 0);

        // Seems to be a timetamp
        stream.EnsureRead(tmpULong, 0, 8);
        _header.Timestamp = BitConverter.ToUInt64(tmpULong, 0);

        stream.EnsureRead(tmpUInt, 0, 4);
        _header.VolumeIdLength = BitConverter.ToUInt32(tmpUInt, 0);
        tmpArray               = new byte[_header.VolumeIdLength];
        stream.EnsureRead(tmpArray, 0, tmpArray.Length);
        _header.VolumeIdBytes    = tmpArray;
        _header.VolumeIdentifier = StringHandlers.CToString(_header.VolumeIdBytes, Encoding.Default);

        stream.EnsureRead(tmpUInt, 0, 4);
        _header.SysIdLength = BitConverter.ToUInt32(tmpUInt, 0);
        tmpArray            = new byte[_header.SysIdLength];
        stream.EnsureRead(tmpArray, 0, tmpArray.Length);
        _header.SysIdBytes       = tmpArray;
        _header.SystemIdentifier = StringHandlers.CToString(_header.SysIdBytes, Encoding.Default);

        stream.EnsureRead(tmpUInt, 0, 4);
        _header.CommentsLength = BitConverter.ToUInt32(tmpUInt, 0);
        tmpArray               = new byte[_header.CommentsLength];
        stream.EnsureRead(tmpArray, 0, tmpArray.Length);
        _header.CommentsBytes = tmpArray;
        _header.Comments      = StringHandlers.CToString(_header.CommentsBytes, Encoding.Default);

        stream.EnsureRead(tmpUInt, 0, 4);
        _header.TrackDescriptors = BitConverter.ToUInt32(tmpUInt, 0);

        stream.EnsureRead(tmpUInt, 0, 4);
        _header.DataFileLength = BitConverter.ToUInt32(tmpUInt, 0);
        tmpArray               = new byte[_header.DataFileLength];
        stream.EnsureRead(tmpArray, 0, tmpArray.Length);
        _header.DataFileBytes = tmpArray;
        _header.DataFile      = StringHandlers.CToString(_header.DataFileBytes, Encoding.Default);

        stream.EnsureRead(tmpUInt, 0, 4);
        _header.SubchannelFileLength = BitConverter.ToUInt32(tmpUInt, 0);
        tmpArray                     = new byte[_header.SubchannelFileLength];
        stream.EnsureRead(tmpArray, 0, tmpArray.Length);
        _header.SubchannelFileBytes = tmpArray;
        _header.SubchannelFile      = StringHandlers.CToString(_header.SubchannelFileBytes, Encoding.Default);

        stream.EnsureRead(tmpUInt, 0, 4);
        _header.Unknown2 = BitConverter.ToUInt32(tmpUInt, 0);
        _header.Unknown3 = (byte)stream.ReadByte();
        tmpArray         = new byte[_header.Unknown3];
        stream.EnsureRead(tmpArray, 0, _header.Unknown3);
        _header.Unknown4 = tmpArray;

        AaruConsole.DebugWriteLine(MODULE_NAME, "header.signature = {0}", StringHandlers.CToString(_header.Signature));

        AaruConsole.DebugWriteLine(MODULE_NAME, "header.unknown1 = {0}",         _header.Unknown1);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.timestamp = {0}",        _header.Timestamp);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.volumeIdLength = {0}",   _header.VolumeIdLength);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.volumeIdentifier = {0}", _header.VolumeIdentifier);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.sysIdLength = {0}",      _header.SysIdLength);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.systemIdentifier = {0}", _header.SystemIdentifier);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.commentsLength = {0}",   _header.CommentsLength);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.comments = {0}",         _header.Comments);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.trackDescriptors = {0}", _header.TrackDescriptors);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.dataFileLength = {0}",   _header.DataFileLength);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.dataFilter = {0}",       _header.DataFilter);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.dataFile = {0}",         _header.DataFile);

        AaruConsole.DebugWriteLine(MODULE_NAME, "header.subchannelFileLength = {0}", _header.SubchannelFileLength);

        AaruConsole.DebugWriteLine(MODULE_NAME, "header.subchannelFilter = {0}", _header.SubchannelFilter);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.subchannelFile = {0}",   _header.SubchannelFile);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.unknown2 = {0}",         _header.Unknown2);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.unknown3 = {0}",         _header.Unknown3);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.unknown4.Length = {0}",  _header.Unknown4.Length);

        _bwTracks = new List<TrackDescriptor>();

        for(var i = 0; i < _header.TrackDescriptors; i++)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, "stream.Position = {0}", stream.Position);

            var track = new TrackDescriptor();

            stream.EnsureRead(tmpUInt, 0, 4);
            track.filenameLen = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray          = new byte[track.filenameLen];
            stream.EnsureRead(tmpArray, 0, tmpArray.Length);
            track.filenameBytes = tmpArray;
            track.filename      = StringHandlers.CToString(track.filenameBytes, Encoding.Default);

            stream.EnsureRead(tmpUInt, 0, 4);
            track.offset     = BitConverter.ToUInt32(tmpUInt, 0);
            track.subchannel = (byte)stream.ReadByte();

            tmpArray = new byte[3];
            stream.EnsureRead(tmpArray, 0, 3);
            track.unknown1 = tmpArray;

            stream.EnsureRead(tmpUInt, 0, 4);
            track.unknown2 = BitConverter.ToUInt32(tmpUInt, 0);
            track.unknown3 = (byte)stream.ReadByte();
            track.session  = (byte)stream.ReadByte();
            track.unknown4 = (byte)stream.ReadByte();
            track.adrCtl   = (byte)stream.ReadByte();

            track.unknown5  = (byte)stream.ReadByte();
            track.trackMode = (TrackType)stream.ReadByte();
            track.unknown6  = (byte)stream.ReadByte();
            track.point     = (byte)stream.ReadByte();

            stream.EnsureRead(tmpUInt, 0, 4);
            track.unknown7 = BitConverter.ToUInt32(tmpUInt, 0);
            stream.EnsureRead(tmpUInt, 0, 4);
            track.unknown8 = BitConverter.ToUInt32(tmpUInt, 0);
            stream.EnsureRead(tmpUInt, 0, 4);
            track.pregapOffsetAdjustment = BitConverter.ToUInt32(tmpUInt, 0);
            stream.EnsureRead(tmpUInt, 0, 4);
            track.unknown10 = BitConverter.ToUInt32(tmpUInt, 0);
            stream.EnsureRead(tmpUShort, 0, 2);
            track.unknown11 = BitConverter.ToUInt16(tmpUShort, 0);
            stream.EnsureRead(tmpUInt, 0, 4);
            track.lastSector = BitConverter.ToUInt32(tmpUInt, 0);
            track.unknown12  = (byte)stream.ReadByte();

            // This is off by one
            track.lastSector--;

            stream.EnsureRead(tmpUInt, 0, 4);
            track.pregap = BitConverter.ToInt32(tmpUInt, 0);
            stream.EnsureRead(tmpUInt, 0, 4);
            track.startSector = BitConverter.ToInt32(tmpUInt, 0);

            track.unknown13 = new uint[2];

            for(var j = 0; j < track.unknown13.Length; j++)
            {
                stream.EnsureRead(tmpUInt, 0, 4);
                track.unknown13[j] = BitConverter.ToUInt32(tmpUInt, 0);
            }

            stream.EnsureRead(tmpUInt, 0, 4);
            track.titleLen = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray       = new byte[track.titleLen];
            stream.EnsureRead(tmpArray, 0, tmpArray.Length);
            track.titleBytes = tmpArray;
            track.title      = StringHandlers.CToString(track.titleBytes, Encoding.Default);

            stream.EnsureRead(tmpUInt, 0, 4);
            track.performerLen = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray           = new byte[track.performerLen];
            stream.EnsureRead(tmpArray, 0, tmpArray.Length);
            track.performerBytes = tmpArray;
            track.performer      = StringHandlers.CToString(track.performerBytes, Encoding.Default);

            stream.EnsureRead(tmpUInt, 0, 4);
            track.unkStrLen1 = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray         = new byte[track.unkStrLen1];
            stream.EnsureRead(tmpArray, 0, tmpArray.Length);
            track.unkStrBytes1 = tmpArray;
            track.unkString1   = StringHandlers.CToString(track.unkStrBytes1, Encoding.Default);

            stream.EnsureRead(tmpUInt, 0, 4);
            track.unkStrLen2 = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray         = new byte[track.unkStrLen2];
            stream.EnsureRead(tmpArray, 0, tmpArray.Length);
            track.unkStrBytes2 = tmpArray;
            track.unkString2   = StringHandlers.CToString(track.unkStrBytes2, Encoding.Default);

            stream.EnsureRead(tmpUInt, 0, 4);
            track.unkStrLen3 = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray         = new byte[track.unkStrLen3];
            stream.EnsureRead(tmpArray, 0, tmpArray.Length);
            track.unkStrBytes3 = tmpArray;
            track.unkString3   = StringHandlers.CToString(track.unkStrBytes3, Encoding.Default);

            stream.EnsureRead(tmpUInt, 0, 4);
            track.unkStrLen4 = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray         = new byte[track.unkStrLen4];
            stream.EnsureRead(tmpArray, 0, tmpArray.Length);
            track.unkStrBytes4 = tmpArray;
            track.unkString4   = StringHandlers.CToString(track.unkStrBytes4, Encoding.Default);

            stream.EnsureRead(tmpUInt, 0, 4);
            track.discIdLen = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray        = new byte[track.discIdLen];
            stream.EnsureRead(tmpArray, 0, tmpArray.Length);
            track.discIdBytes = tmpArray;
            track.discId      = StringHandlers.CToString(track.discIdBytes, Encoding.Default);

            stream.EnsureRead(tmpUInt, 0, 4);
            track.unkStrLen5 = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray         = new byte[track.unkStrLen5];
            stream.EnsureRead(tmpArray, 0, tmpArray.Length);
            track.unkStrBytes5 = tmpArray;
            track.unkString5   = StringHandlers.CToString(track.unkStrBytes5, Encoding.Default);

            stream.EnsureRead(tmpUInt, 0, 4);
            track.unkStrLen6 = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray         = new byte[track.unkStrLen6];
            stream.EnsureRead(tmpArray, 0, tmpArray.Length);
            track.unkStrBytes6 = tmpArray;
            track.unkString6   = StringHandlers.CToString(track.unkStrBytes6, Encoding.Default);

            stream.EnsureRead(tmpUInt, 0, 4);
            track.unkStrLen7 = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray         = new byte[track.unkStrLen7];
            stream.EnsureRead(tmpArray, 0, tmpArray.Length);
            track.unkStrBytes7 = tmpArray;
            track.unkString7   = StringHandlers.CToString(track.unkStrBytes7, Encoding.Default);

            stream.EnsureRead(tmpUInt, 0, 4);
            track.unkStrLen8 = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray         = new byte[track.unkStrLen8];
            stream.EnsureRead(tmpArray, 0, tmpArray.Length);
            track.unkStrBytes8 = tmpArray;
            track.unkString8   = StringHandlers.CToString(track.unkStrBytes8, Encoding.Default);

            stream.EnsureRead(tmpUInt, 0, 4);
            track.unkStrLen9 = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray         = new byte[track.unkStrLen9];
            stream.EnsureRead(tmpArray, 0, tmpArray.Length);
            track.unkStrBytes9 = tmpArray;
            track.unkString9   = StringHandlers.CToString(track.unkStrBytes9, Encoding.Default);

            stream.EnsureRead(tmpUInt, 0, 4);
            track.unkStrLen10 = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray          = new byte[track.unkStrLen10];
            stream.EnsureRead(tmpArray, 0, tmpArray.Length);
            track.unkStrBytes10 = tmpArray;
            track.unkString10   = StringHandlers.CToString(track.unkStrBytes10, Encoding.Default);

            stream.EnsureRead(tmpUInt, 0, 4);
            track.unkStrLen11 = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray          = new byte[track.unkStrLen11];
            stream.EnsureRead(tmpArray, 0, tmpArray.Length);
            track.unkStrBytes11 = tmpArray;
            track.unkString11   = StringHandlers.CToString(track.unkStrBytes11, Encoding.Default);

            stream.EnsureRead(tmpUInt, 0, 4);
            track.isrcLen = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray      = new byte[track.isrcLen];
            stream.EnsureRead(tmpArray, 0, tmpArray.Length);
            track.isrcBytes = tmpArray;
            track.isrcUpc   = StringHandlers.CToString(track.isrcBytes, Encoding.Default);

            AaruConsole.DebugWriteLine(MODULE_NAME, "track.filenameLen = {0}", track.filenameLen);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.filename = {0}",    track.filename);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.offset = {0}",      track.offset);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.subchannel = {0}",  track.subchannel);

            for(var j = 0; j < track.unknown1.Length; j++)
                AaruConsole.DebugWriteLine(MODULE_NAME, "track.unknown1[{1}] = 0x{0:X8}", track.unknown1[j], j);

            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unknown2 = {0}",    track.unknown2);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unknown3 = {0}",    track.unknown3);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.session = {0}",     track.session);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unknown4 = {0}",    track.unknown4);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.adrCtl = {0}",      track.adrCtl);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unknown5 = {0}",    track.unknown5);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.trackMode = {0}",   track.trackMode);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unknown6 = {0}",    track.unknown6);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.point = {0}",       track.point);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unknown7 = {0}",    track.unknown7);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unknown8 = {0}",    track.unknown8);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unknown9 = {0}",    track.pregapOffsetAdjustment);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unknown10 = {0}",   track.unknown10);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unknown11 = {0}",   track.unknown11);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.lastSector = {0}",  track.lastSector);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unknown12 = {0}",   track.unknown12);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.pregap = {0}",      track.pregap);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.startSector = {0}", track.startSector);

            for(var j = 0; j < track.unknown13.Length; j++)
                AaruConsole.DebugWriteLine(MODULE_NAME, "track.unknown13[{1}] = 0x{0:X8}", track.unknown13[j], j);

            AaruConsole.DebugWriteLine(MODULE_NAME, "track.titleLen = {0}",     track.titleLen);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.title = {0}",        track.title);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.performerLen = {0}", track.performerLen);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.performer = {0}",    track.performer);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unkStrLen1 = {0}",   track.unkStrLen1);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unkString1 = {0}",   track.unkString1);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unkStrLen2 = {0}",   track.unkStrLen2);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unkString2 = {0}",   track.unkString2);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unkStrLen3 = {0}",   track.unkStrLen3);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unkString3 = {0}",   track.unkString3);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unkStrLen4 = {0}",   track.unkStrLen4);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unkString4 = {0}",   track.unkString4);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.discIdLen = {0}",    track.discIdLen);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.discId = {0}",       track.discId);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unkStrLen5 = {0}",   track.unkStrLen5);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unkString5 = {0}",   track.unkString5);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unkStrLen6 = {0}",   track.unkStrLen6);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unkString6 = {0}",   track.unkString6);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unkStrLen7 = {0}",   track.unkStrLen7);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unkString7 = {0}",   track.unkString7);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unkStrLen8 = {0}",   track.unkStrLen8);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unkString8 = {0}",   track.unkString8);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unkStrLen9 = {0}",   track.unkStrLen9);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unkString9 = {0}",   track.unkString9);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unkStrLen10 = {0}",  track.unkStrLen10);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unkString10 = {0}",  track.unkString10);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unkStrLen11 = {0}",  track.unkStrLen11);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.unkString11 = {0}",  track.unkString11);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.isrcLen = {0}",      track.isrcLen);
            AaruConsole.DebugWriteLine(MODULE_NAME, "track.isrcUpc = {0}",      track.isrcUpc);

            _bwTracks.Add(track);
        }


        if(!string.IsNullOrEmpty(_header.DataFile))
        {
            while(true)
            {
                _dataFilter =
                    PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder, _header.DataFile));

                if(_dataFilter != null)
                    break;

                _dataFilter = PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                              _header.DataFile.ToLower(CultureInfo.
                                                                                  CurrentCulture)));

                if(_dataFilter != null)
                    break;

                _dataFilter = PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                              _header.DataFile.ToUpper(CultureInfo.
                                                                                  CurrentCulture)));

                if(_dataFilter != null)
                    break;

                _dataFilter = PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                              _header.DataFile.Split(new[]
                                                                                  {
                                                                                      '\\'
                                                                                  },
                                                                                  StringSplitOptions.
                                                                                      RemoveEmptyEntries).
                                                                                  Last()));

                if(_dataFilter != null)
                    break;

                _dataFilter = PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                              _header.DataFile.Split(new[]
                                                                                  {
                                                                                      '\\'
                                                                                  },
                                                                                  StringSplitOptions.
                                                                                      RemoveEmptyEntries).
                                                                                  Last().
                                                                                  ToLower(CultureInfo.CurrentCulture)));

                if(_dataFilter != null)
                    break;

                _dataFilter = PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                              _header.DataFile.Split(new[]
                                                                                  {
                                                                                      '\\'
                                                                                  },
                                                                                  StringSplitOptions.
                                                                                      RemoveEmptyEntries).
                                                                                  Last().
                                                                                  ToUpper(CultureInfo.CurrentCulture)));

                if(_dataFilter != null)
                    break;

                AaruConsole.ErrorWriteLine(string.Format(Localization.Data_file_0_not_found, _header.DataFile));

                return ErrorNumber.NoSuchFile;
            }
        }
        else
        {
            AaruConsole.ErrorWriteLine(Localization.Unable_to_find_data_file);

            return ErrorNumber.NoSuchFile;
        }

        if(!string.IsNullOrEmpty(_header.SubchannelFile))
        {
            _subFilter =
                ((((PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                    _header.SubchannelFile)) ??
                    PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                    _header.SubchannelFile.ToLower(CultureInfo.
                                                                        CurrentCulture)))) ??
                   PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                   _header.SubchannelFile.ToUpper(CultureInfo.
                                                                       CurrentCulture)))) ??
                  PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder, _header.SubchannelFile.
                                                                      Split(new[]
                                                                      {
                                                                          '\\'
                                                                      }, StringSplitOptions.RemoveEmptyEntries).
                                                                      Last()))) ??
                 PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder, _header.SubchannelFile.
                                                                     Split(new[]
                                                                     {
                                                                         '\\'
                                                                     }, StringSplitOptions.RemoveEmptyEntries).
                                                                     Last().
                                                                     ToLower(CultureInfo.CurrentCulture)))) ??
                PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder, _header.SubchannelFile.
                                                                    Split(new[]
                                                                    {
                                                                        '\\'
                                                                    }, StringSplitOptions.RemoveEmptyEntries).
                                                                    Last().
                                                                    ToUpper(CultureInfo.CurrentCulture)));
        }

        Tracks      = new List<Track>();
        Partitions  = new List<Partition>();
        _offsetMap  = new Dictionary<uint, ulong>();
        _trackFlags = new Dictionary<uint, byte>();
        ushort maxSession = 0;

        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackFlags);

        foreach(TrackDescriptor bwTrack in _bwTracks)
        {
            if(bwTrack.point < 0xA0)
            {
                var track = new Track
                {
                    Description = bwTrack.title,
                    EndSector   = bwTrack.lastSector
                };

                if(!string.IsNullOrEmpty(bwTrack.filename))
                {
                    do
                    {
                        track.Filter =
                            PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                            bwTrack.filename));

                        if(track.Filter != null)
                            break;

                        track.Filter = PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                              bwTrack.filename.ToLower(CultureInfo.
                                                                                  CurrentCulture)));

                        if(track.Filter != null)
                            break;

                        track.Filter = PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                              bwTrack.filename.ToUpper(CultureInfo.
                                                                                  CurrentCulture)));

                        if(track.Filter != null)
                            break;

                        track.Filter = PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                              bwTrack.filename.Split(new[]
                                                                                      {
                                                                                          '\\'
                                                                                      },
                                                                                      StringSplitOptions.
                                                                                          RemoveEmptyEntries).
                                                                                  Last()));

                        if(track.Filter != null)
                            break;

                        track.Filter = PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                              bwTrack.filename.Split(new[]
                                                                                      {
                                                                                          '\\'
                                                                                      },
                                                                                      StringSplitOptions.
                                                                                          RemoveEmptyEntries).
                                                                                  Last().
                                                                                  ToLower(CultureInfo.
                                                                                      CurrentCulture)));

                        if(track.Filter != null)
                            break;

                        track.Filter = PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                              bwTrack.filename.Split(new[]
                                                                                      {
                                                                                          '\\'
                                                                                      },
                                                                                      StringSplitOptions.
                                                                                          RemoveEmptyEntries).
                                                                                  Last().
                                                                                  ToUpper(CultureInfo.
                                                                                      CurrentCulture)));

                        track.Filter = _dataFilter;
                    } while(true);
                }
                else
                    track.Filter = _dataFilter;

                track.File = _dataFilter.Filename;

                track.FileOffset = bwTrack.offset + (150 - bwTrack.pregapOffsetAdjustment) * 2352;

                track.SubchannelOffset = (bwTrack.offset / 2352 + (150 - bwTrack.pregapOffsetAdjustment)) * 96;

                if(bwTrack.pregap > 0)
                {
                    track.Pregap      = (ulong)(bwTrack.startSector - bwTrack.pregap);
                    track.StartSector = (ulong)bwTrack.pregap;

                    track.FileOffset       -= track.Pregap * 2352;
                    track.SubchannelOffset -= track.Pregap * 96;
                }
                else
                {
                    track.Pregap = (ulong)(bwTrack.startSector - bwTrack.pregap);

                    if(bwTrack.pregap < 0)
                        track.StartSector = 0;
                    else
                        track.StartSector = (ulong)bwTrack.pregap;
                }

                track.FileType          = "BINARY";
                track.RawBytesPerSector = 2352;
                track.Sequence          = bwTrack.point;
                track.Session           = bwTrack.session;

                if(track.Session > maxSession)
                    maxSession = track.Session;

                track.SubchannelFilter = _subFilter;
                track.SubchannelFile   = _subFilter?.Filename;

                if(_subFilter != null)
                {
                    track.SubchannelType = TrackSubchannelType.Packed;

                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);
                }
                else
                    track.SubchannelType = TrackSubchannelType.None;

                switch(bwTrack.trackMode)
                {
                    case TrackType.Audio:
                        track.Type            = CommonTypes.Enums.TrackType.Audio;
                        _imageInfo.SectorSize = 2352;
                        track.BytesPerSector  = 2352;

                        break;
                    case TrackType.Mode1:
                        track.Type = CommonTypes.Enums.TrackType.CdMode1;

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

                        track.BytesPerSector = 2048;

                        break;
                    case TrackType.Mode2:
                        track.Type = CommonTypes.Enums.TrackType.CdMode2Formless;

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                        if(_imageInfo.SectorSize < 2336)
                            _imageInfo.SectorSize = 2336;

                        track.BytesPerSector = 2336;

                        break;
                    default:
                        track.Type              = CommonTypes.Enums.TrackType.Data;
                        track.RawBytesPerSector = 2048;
                        _imageInfo.SectorSize   = 2048;
                        track.BytesPerSector    = 2048;

                        break;
                }

                if(bwTrack.pregap != 0)
                    track.Indexes.Add(0, bwTrack.pregap);

                track.Indexes.Add(1, bwTrack.startSector);

                var partition = new Partition
                {
                    Description = track.Description,
                    Size        = (track.EndSector - track.StartSector + 1) * 2352,
                    Length      = track.EndSector - track.StartSector + 1,
                    Sequence    = track.Sequence,
                    Start       = track.StartSector + track.Pregap
                };

                partition.Offset = partition.Start * 2352;
                partition.Type   = track.Type.ToString();

                Partitions.Add(partition);
                Tracks.Add(track);

                if(bwTrack.pregap > 0)
                    _offsetMap[track.Sequence] = (ulong)bwTrack.pregap;
                else
                    _offsetMap[track.Sequence] = 0;

                _offsetMap.TryAdd(track.Sequence, track.StartSector);

                _trackFlags.TryAdd(track.Sequence, (byte)(bwTrack.adrCtl & 0x0F));

                if(bwTrack.lastSector > _imageInfo.Sectors)
                    _imageInfo.Sectors = bwTrack.lastSector + 1;
            }
            else
            {
                _imageInfo.MediaBarcode      = bwTrack.isrcUpc;
                _imageInfo.MediaSerialNumber = bwTrack.discId;
                _imageInfo.MediaTitle        = bwTrack.title;

                if(!string.IsNullOrEmpty(bwTrack.isrcUpc) &&
                   !_imageInfo.ReadableMediaTags.Contains(MediaTagType.CD_MCN))
                    _imageInfo.ReadableMediaTags.Add(MediaTagType.CD_MCN);
            }
        }

        Sessions = new List<Session>();

        for(ushort i = 1; i <= maxSession; i++)
        {
            var session = new Session
            {
                Sequence    = i,
                StartTrack  = uint.MaxValue,
                StartSector = uint.MaxValue
            };

            foreach(Track track in Tracks.Where(track => track.Session == i))
            {
                if(track.Sequence < session.StartTrack)
                    session.StartTrack = track.Sequence;

                if(track.Sequence > session.EndTrack)
                    session.StartTrack = track.Sequence;

                if(track.StartSector < session.StartSector)
                    session.StartSector = track.StartSector;

                if(track.EndSector > session.EndSector)
                    session.EndSector = track.EndSector;
            }

            Sessions.Add(session);
        }

        // As long as subchannel is written for any track, it is present for all tracks
        if(Tracks.Any(t => t.SubchannelType == TrackSubchannelType.Packed))
        {
            foreach(Track track in Tracks)
                track.SubchannelType = TrackSubchannelType.Packed;
        }

        _imageInfo.MediaType = MediaType.CD;

        _imageInfo.Application        = "BlindWrite";
        _imageInfo.ApplicationVersion = "4";
        _imageInfo.Version            = "4";

        _imageInfo.ImageSize            = (ulong)_dataFilter.DataForkLength;
        _imageInfo.CreationTime         = _dataFilter.CreationTime;
        _imageInfo.LastModificationTime = _dataFilter.LastWriteTime;
        _imageInfo.MetadataMediaType    = MetadataMediaType.OpticalDisc;

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

            mode2 = bwTrack.Type switch
                    {
                        CommonTypes.Enums.TrackType.CdMode2Formless => true,
                        _                                           => mode2
                    };
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

        _imageInfo.Comments = _header.Comments;

        AaruConsole.VerboseWriteLine(Localization.BlindWrite_image_describes_a_disc_of_type_0, _imageInfo.MediaType);

        if(!string.IsNullOrEmpty(_imageInfo.Comments))
            AaruConsole.VerboseWriteLine(Localization.BlindWrite_comments_0, _imageInfo.Comments);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadMediaTag(MediaTagType tag, out byte[] buffer)
    {
        buffer = null;

        switch(tag)
        {
            case MediaTagType.CD_MCN:
            {
                if(_imageInfo.MediaSerialNumber == null)
                    return ErrorNumber.NoData;

                buffer = Encoding.ASCII.GetBytes(_imageInfo.MediaSerialNumber);

                return ErrorNumber.NoError;
            }
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
        Track aaruTrack = Tracks.FirstOrDefault(bwTrack => bwTrack.Sequence == track);

        if(aaruTrack is null)
            return ErrorNumber.SectorNotFound;

        if(length + sectorAddress > aaruTrack.EndSector - aaruTrack.StartSector + 1)
            return ErrorNumber.OutOfRange;

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

        buffer = new byte[sectorSize * length];

        _imageStream = aaruTrack.Filter.GetDataForkStream();
        var br = new BinaryReader(_imageStream);

        br.BaseStream.
           Seek((long)aaruTrack.FileOffset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                SeekOrigin.Begin);

        if(mode2)
        {
            var mode2Ms = new MemoryStream((int)(sectorSize * length));

            buffer = br.ReadBytes((int)(sectorSize * length));

            for(var i = 0; i < length; i++)
            {
                var sector = new byte[sectorSize];
                Array.Copy(buffer, sectorSize * i, sector, 0, sectorSize);
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
        Track aaruTrack = Tracks.FirstOrDefault(bwTrack => bwTrack.Sequence == track);

        if(aaruTrack is null)
            return ErrorNumber.SectorNotFound;

        if(length + sectorAddress > aaruTrack.EndSector - aaruTrack.StartSector + 1)
            return ErrorNumber.OutOfRange;

        uint sectorOffset = 0;
        uint sectorSize   = 0;
        uint sectorSkip   = 0;

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
                        sectorOffset = 0;
                        sectorSize   = 96;
                        sectorSkip   = 0;

                        break;
                    }
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
                        sectorOffset = 0;
                        sectorSize   = 96;
                        sectorSkip   = 0;

                        break;
                    }
                }

                break;
            }
            case CommonTypes.Enums.TrackType.Audio:
            {
                switch(tag)
                {
                    case SectorTagType.CdSectorSubchannel:
                    {
                        sectorOffset = 0;
                        sectorSize   = 96;
                        sectorSkip   = 0;

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

        buffer = new byte[sectorSize * length];

        _imageStream = tag == SectorTagType.CdSectorSubchannel
                           ? aaruTrack.SubchannelFilter.GetDataForkStream()
                           : aaruTrack.Filter.GetDataForkStream();

        var br = new BinaryReader(_imageStream);

        br.BaseStream.
           Seek((long)(tag == SectorTagType.CdSectorSubchannel ? aaruTrack.SubchannelOffset : aaruTrack.FileOffset) + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
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

        if(tag == SectorTagType.CdSectorSubchannel)
            buffer = Subchannel.Interleave(buffer);

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
        Track aaruTrack = Tracks.FirstOrDefault(bwTrack => bwTrack.Sequence == track);

        if(aaruTrack is null)
            return ErrorNumber.SectorNotFound;

        if(length + sectorAddress > aaruTrack.EndSector - aaruTrack.StartSector + 1)
            return ErrorNumber.OutOfRange;

        uint sectorSize;

        switch(aaruTrack.Type)
        {
            case CommonTypes.Enums.TrackType.Audio:
            case CommonTypes.Enums.TrackType.CdMode1:
            case CommonTypes.Enums.TrackType.CdMode2Formless:
            case CommonTypes.Enums.TrackType.Data:
            {
                sectorSize = (uint)aaruTrack.RawBytesPerSector;

                break;
            }
            default:
                return ErrorNumber.NotSupported;
        }

        _imageStream = aaruTrack.Filter.GetDataForkStream();
        var br = new BinaryReader(_imageStream);

        br.BaseStream.Seek((long)aaruTrack.FileOffset + (long)(sectorAddress * sectorSize), SeekOrigin.Begin);

        buffer = br.ReadBytes((int)(sectorSize * length));

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public List<Track> GetSessionTracks(Session session) =>
        Sessions.Contains(session) ? GetSessionTracks(session.Sequence) : null;

    /// <inheritdoc />
    public List<Track> GetSessionTracks(ushort session) => Tracks.Where(track => track.Session == session).ToList();

#endregion
}