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
using Aaru.Console;
using Aaru.Decoders.CD;
using Session = Aaru.CommonTypes.Structs.Session;

namespace Aaru.DiscImages
{
    public partial class BlindWrite4
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 19)
                return false;

            byte[] tmpArray  = new byte[19];
            byte[] tmpUShort = new byte[2];
            byte[] tmpUInt   = new byte[4];
            byte[] tmpULong  = new byte[8];

            stream.Read(tmpArray, 0, 19);

            if(!bw4Signature.SequenceEqual(tmpArray))
                return false;

            header = new Bw4Header
            {
                Signature = tmpArray
            };

            // Seems to always be 2
            stream.Read(tmpUInt, 0, 4);
            header.Unknown1 = BitConverter.ToUInt32(tmpUInt, 0);

            // Seems to be a timetamp
            stream.Read(tmpULong, 0, 8);
            header.Timestamp = BitConverter.ToUInt64(tmpULong, 0);

            stream.Read(tmpUInt, 0, 4);
            header.VolumeIdLength = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray              = new byte[header.VolumeIdLength];
            stream.Read(tmpArray, 0, tmpArray.Length);
            header.VolumeIdBytes    = tmpArray;
            header.VolumeIdentifier = StringHandlers.CToString(header.VolumeIdBytes, Encoding.Default);

            stream.Read(tmpUInt, 0, 4);
            header.SysIdLength = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray           = new byte[header.SysIdLength];
            stream.Read(tmpArray, 0, tmpArray.Length);
            header.SysIdBytes       = tmpArray;
            header.SystemIdentifier = StringHandlers.CToString(header.SysIdBytes, Encoding.Default);

            stream.Read(tmpUInt, 0, 4);
            header.CommentsLength = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray              = new byte[header.CommentsLength];
            stream.Read(tmpArray, 0, tmpArray.Length);
            header.CommentsBytes = tmpArray;
            header.Comments      = StringHandlers.CToString(header.CommentsBytes, Encoding.Default);

            stream.Read(tmpUInt, 0, 4);
            header.TrackDescriptors = BitConverter.ToUInt32(tmpUInt, 0);

            stream.Read(tmpUInt, 0, 4);
            header.DataFileLength = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray              = new byte[header.DataFileLength];
            stream.Read(tmpArray, 0, tmpArray.Length);
            header.DataFileBytes = tmpArray;
            header.DataFile      = StringHandlers.CToString(header.DataFileBytes, Encoding.Default);

            stream.Read(tmpUInt, 0, 4);
            header.SubchannelFileLength = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray                    = new byte[header.SubchannelFileLength];
            stream.Read(tmpArray, 0, tmpArray.Length);
            header.SubchannelFileBytes = tmpArray;
            header.SubchannelFile      = StringHandlers.CToString(header.SubchannelFileBytes, Encoding.Default);

            stream.Read(tmpUInt, 0, 4);
            header.Unknown2 = BitConverter.ToUInt32(tmpUInt, 0);
            header.Unknown3 = (byte)stream.ReadByte();
            tmpArray        = new byte[header.Unknown3];
            stream.Read(tmpArray, 0, header.Unknown3);
            header.Unknown4 = tmpArray;

            AaruConsole.DebugWriteLine("BlindWrite4 plugin", "header.signature = {0}",
                                       StringHandlers.CToString(header.Signature));

            AaruConsole.DebugWriteLine("BlindWrite4 plugin", "header.unknown1 = {0}", header.Unknown1);
            AaruConsole.DebugWriteLine("BlindWrite4 plugin", "header.timestamp = {0}", header.Timestamp);
            AaruConsole.DebugWriteLine("BlindWrite4 plugin", "header.volumeIdLength = {0}", header.VolumeIdLength);
            AaruConsole.DebugWriteLine("BlindWrite4 plugin", "header.volumeIdentifier = {0}", header.VolumeIdentifier);
            AaruConsole.DebugWriteLine("BlindWrite4 plugin", "header.sysIdLength = {0}", header.SysIdLength);
            AaruConsole.DebugWriteLine("BlindWrite4 plugin", "header.systemIdentifier = {0}", header.SystemIdentifier);
            AaruConsole.DebugWriteLine("BlindWrite4 plugin", "header.commentsLength = {0}", header.CommentsLength);
            AaruConsole.DebugWriteLine("BlindWrite4 plugin", "header.comments = {0}", header.Comments);
            AaruConsole.DebugWriteLine("BlindWrite4 plugin", "header.trackDescriptors = {0}", header.TrackDescriptors);
            AaruConsole.DebugWriteLine("BlindWrite4 plugin", "header.dataFileLength = {0}", header.DataFileLength);
            AaruConsole.DebugWriteLine("BlindWrite4 plugin", "header.dataFilter = {0}", header.DataFilter);
            AaruConsole.DebugWriteLine("BlindWrite4 plugin", "header.dataFile = {0}", header.DataFile);

            AaruConsole.DebugWriteLine("BlindWrite4 plugin", "header.subchannelFileLength = {0}",
                                       header.SubchannelFileLength);

            AaruConsole.DebugWriteLine("BlindWrite4 plugin", "header.subchannelFilter = {0}", header.SubchannelFilter);
            AaruConsole.DebugWriteLine("BlindWrite4 plugin", "header.subchannelFile = {0}", header.SubchannelFile);
            AaruConsole.DebugWriteLine("BlindWrite4 plugin", "header.unknown2 = {0}", header.Unknown2);
            AaruConsole.DebugWriteLine("BlindWrite4 plugin", "header.unknown3 = {0}", header.Unknown3);
            AaruConsole.DebugWriteLine("BlindWrite4 plugin", "header.unknown4.Length = {0}", header.Unknown4.Length);

            bwTracks = new List<Bw4TrackDescriptor>();

            for(int i = 0; i < header.TrackDescriptors; i++)
            {
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "stream.Position = {0}", stream.Position);

                var track = new Bw4TrackDescriptor();

                stream.Read(tmpUInt, 0, 4);
                track.filenameLen = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray          = new byte[track.filenameLen];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.filenameBytes = tmpArray;
                track.filename      = StringHandlers.CToString(track.filenameBytes, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.offset     = BitConverter.ToUInt32(tmpUInt, 0);
                track.subchannel = (byte)stream.ReadByte();

                tmpArray = new byte[3];
                stream.Read(tmpArray, 0, 3);
                track.unknown1 = tmpArray;

                stream.Read(tmpUInt, 0, 4);
                track.unknown2 = BitConverter.ToUInt32(tmpUInt, 0);
                track.unknown3 = (byte)stream.ReadByte();
                track.session  = (byte)stream.ReadByte();
                track.unknown4 = (byte)stream.ReadByte();
                track.adrCtl   = (byte)stream.ReadByte();

                track.unknown5  = (byte)stream.ReadByte();
                track.trackMode = (Bw4TrackType)stream.ReadByte();
                track.unknown6  = (byte)stream.ReadByte();
                track.point     = (byte)stream.ReadByte();

                stream.Read(tmpUInt, 0, 4);
                track.unknown7 = BitConverter.ToUInt32(tmpUInt, 0);
                stream.Read(tmpUInt, 0, 4);
                track.unknown8 = BitConverter.ToUInt32(tmpUInt, 0);
                stream.Read(tmpUInt, 0, 4);
                track.unknown9 = BitConverter.ToUInt32(tmpUInt, 0);
                stream.Read(tmpUInt, 0, 4);
                track.unknown10 = BitConverter.ToUInt32(tmpUInt, 0);
                stream.Read(tmpUShort, 0, 2);
                track.unknown11 = BitConverter.ToUInt16(tmpUShort, 0);
                stream.Read(tmpUInt, 0, 4);
                track.lastSector = BitConverter.ToUInt32(tmpUInt, 0);
                track.unknown12  = (byte)stream.ReadByte();

                stream.Read(tmpUInt, 0, 4);
                track.pregap = BitConverter.ToInt32(tmpUInt, 0);
                stream.Read(tmpUInt, 0, 4);
                track.startSector = BitConverter.ToInt32(tmpUInt, 0);

                track.unknown13 = new uint[2];

                for(int j = 0; j < track.unknown13.Length; j++)
                {
                    stream.Read(tmpUInt, 0, 4);
                    track.unknown13[j] = BitConverter.ToUInt32(tmpUInt, 0);
                }

                stream.Read(tmpUInt, 0, 4);
                track.titleLen = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray       = new byte[track.titleLen];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.titleBytes = tmpArray;
                track.title      = StringHandlers.CToString(track.titleBytes, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.performerLen = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray           = new byte[track.performerLen];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.performerBytes = tmpArray;
                track.performer      = StringHandlers.CToString(track.performerBytes, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.unkStrLen1 = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray         = new byte[track.unkStrLen1];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.unkStrBytes1 = tmpArray;
                track.unkString1   = StringHandlers.CToString(track.unkStrBytes1, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.unkStrLen2 = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray         = new byte[track.unkStrLen2];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.unkStrBytes2 = tmpArray;
                track.unkString2   = StringHandlers.CToString(track.unkStrBytes2, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.unkStrLen3 = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray         = new byte[track.unkStrLen3];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.unkStrBytes3 = tmpArray;
                track.unkString3   = StringHandlers.CToString(track.unkStrBytes3, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.unkStrLen4 = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray         = new byte[track.unkStrLen4];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.unkStrBytes4 = tmpArray;
                track.unkString4   = StringHandlers.CToString(track.unkStrBytes4, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.discIdLen = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray        = new byte[track.discIdLen];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.discIdBytes = tmpArray;
                track.discId      = StringHandlers.CToString(track.discIdBytes, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.unkStrLen5 = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray         = new byte[track.unkStrLen5];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.unkStrBytes5 = tmpArray;
                track.unkString5   = StringHandlers.CToString(track.unkStrBytes5, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.unkStrLen6 = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray         = new byte[track.unkStrLen6];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.unkStrBytes6 = tmpArray;
                track.unkString6   = StringHandlers.CToString(track.unkStrBytes6, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.unkStrLen7 = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray         = new byte[track.unkStrLen7];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.unkStrBytes7 = tmpArray;
                track.unkString7   = StringHandlers.CToString(track.unkStrBytes7, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.unkStrLen8 = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray         = new byte[track.unkStrLen8];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.unkStrBytes8 = tmpArray;
                track.unkString8   = StringHandlers.CToString(track.unkStrBytes8, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.unkStrLen9 = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray         = new byte[track.unkStrLen9];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.unkStrBytes9 = tmpArray;
                track.unkString9   = StringHandlers.CToString(track.unkStrBytes9, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.unkStrLen10 = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray          = new byte[track.unkStrLen10];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.unkStrBytes10 = tmpArray;
                track.unkString10   = StringHandlers.CToString(track.unkStrBytes10, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.unkStrLen11 = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray          = new byte[track.unkStrLen11];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.unkStrBytes11 = tmpArray;
                track.unkString11   = StringHandlers.CToString(track.unkStrBytes11, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.isrcLen = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray      = new byte[track.isrcLen];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.isrcBytes = tmpArray;
                track.isrcUpc   = StringHandlers.CToString(track.isrcBytes, Encoding.Default);

                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.filenameLen = {0}", track.filenameLen);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.filename = {0}", track.filename);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.offset = {0}", track.offset);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.subchannel = {0}", track.subchannel);

                for(int j = 0; j < track.unknown1.Length; j++)
                    AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown1[{1}] = 0x{0:X8}",
                                               track.unknown1[j], j);

                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown2 = {0}", track.unknown2);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown3 = {0}", track.unknown3);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.session = {0}", track.session);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown4 = {0}", track.unknown4);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.adrCtl = {0}", track.adrCtl);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown5 = {0}", track.unknown5);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.trackMode = {0}", track.trackMode);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown6 = {0}", track.unknown6);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.point = {0}", track.point);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown7 = {0}", track.unknown7);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown8 = {0}", track.unknown8);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown9 = {0}", track.unknown9);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown10 = {0}", track.unknown10);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown11 = {0}", track.unknown11);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.lastSector = {0}", track.lastSector);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown12 = {0}", track.unknown12);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.pregap = {0}", track.pregap);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.startSector = {0}", track.startSector);

                for(int j = 0; j < track.unknown13.Length; j++)
                    AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown13[{1}] = 0x{0:X8}",
                                               track.unknown13[j], j);

                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.titleLen = {0}", track.titleLen);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.title = {0}", track.title);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.performerLen = {0}", track.performerLen);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.performer = {0}", track.performer);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkStrLen1 = {0}", track.unkStrLen1);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkString1 = {0}", track.unkString1);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkStrLen2 = {0}", track.unkStrLen2);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkString2 = {0}", track.unkString2);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkStrLen3 = {0}", track.unkStrLen3);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkString3 = {0}", track.unkString3);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkStrLen4 = {0}", track.unkStrLen4);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkString4 = {0}", track.unkString4);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.discIdLen = {0}", track.discIdLen);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.discId = {0}", track.discId);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkStrLen5 = {0}", track.unkStrLen5);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkString5 = {0}", track.unkString5);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkStrLen6 = {0}", track.unkStrLen6);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkString6 = {0}", track.unkString6);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkStrLen7 = {0}", track.unkStrLen7);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkString7 = {0}", track.unkString7);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkStrLen8 = {0}", track.unkStrLen8);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkString8 = {0}", track.unkString8);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkStrLen9 = {0}", track.unkStrLen9);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkString9 = {0}", track.unkString9);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkStrLen10 = {0}", track.unkStrLen10);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkString10 = {0}", track.unkString10);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkStrLen11 = {0}", track.unkStrLen11);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkString11 = {0}", track.unkString11);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.isrcLen = {0}", track.isrcLen);
                AaruConsole.DebugWriteLine("BlindWrite4 plugin", "track.isrcUpc = {0}", track.isrcUpc);

                bwTracks.Add(track);
            }

            var filtersList = new FiltersList();

            if(!string.IsNullOrEmpty(header.DataFile))
                while(true)
                {
                    dataFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), header.DataFile));

                    if(dataFilter != null)
                        break;

                    dataFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                    header.DataFile.ToLower(CultureInfo.
                                                                                                CurrentCulture)));

                    if(dataFilter != null)
                        break;

                    dataFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                    header.DataFile.ToUpper(CultureInfo.
                                                                                                CurrentCulture)));

                    if(dataFilter != null)
                        break;

                    dataFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), header.
                                                                                                   DataFile.Split(new[]
                                                                                                                  {
                                                                                                                      '\\'
                                                                                                                  },
                                                                                                                  StringSplitOptions.
                                                                                                                      RemoveEmptyEntries).
                                                                                                   Last()));

                    if(dataFilter != null)
                        break;

                    dataFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), header.
                                                                                                   DataFile.Split(new[]
                                                                                                                  {
                                                                                                                      '\\'
                                                                                                                  },
                                                                                                                  StringSplitOptions.
                                                                                                                      RemoveEmptyEntries).
                                                                                                   Last().
                                                                                                   ToLower(CultureInfo.
                                                                                                               CurrentCulture)));

                    if(dataFilter != null)
                        break;

                    dataFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), header.
                                                                                                   DataFile.Split(new[]
                                                                                                                  {
                                                                                                                      '\\'
                                                                                                                  },
                                                                                                                  StringSplitOptions.
                                                                                                                      RemoveEmptyEntries).
                                                                                                   Last().
                                                                                                   ToUpper(CultureInfo.
                                                                                                               CurrentCulture)));

                    if(dataFilter != null)
                        break;

                    throw new ArgumentException($"Data file {header.DataFile} not found");
                }
            else
                throw new ArgumentException("Unable to find data file");

            if(!string.IsNullOrEmpty(header.SubchannelFile))
            {
                filtersList = new FiltersList();

                subFilter =
                    ((((filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), header.SubchannelFile)) ??
                        filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                           header.SubchannelFile.ToLower(CultureInfo.CurrentCulture)))
                       ) ?? filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                               header.SubchannelFile.
                                                                      ToUpper(CultureInfo.CurrentCulture)))) ??
                      filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), header.
                                                                                        SubchannelFile.Split(new[]
                                                                                                             {
                                                                                                                 '\\'
                                                                                                             },
                                                                                                             StringSplitOptions.
                                                                                                                 RemoveEmptyEntries).
                                                                                        Last()))
                     ) ?? filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), header.
                                                                                            SubchannelFile.Split(new[]
                                                                                                                 {
                                                                                                                     '\\'
                                                                                                                 },
                                                                                                                 StringSplitOptions.
                                                                                                                     RemoveEmptyEntries).
                                                                                            Last().ToLower(CultureInfo.
                                                                                                               CurrentCulture)))
                    ) ?? filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), header.
                                                                                           SubchannelFile.Split(new[]
                                                                                                                {
                                                                                                                    '\\'
                                                                                                                },
                                                                                                                StringSplitOptions.
                                                                                                                    RemoveEmptyEntries).
                                                                                           Last().ToUpper(CultureInfo.
                                                                                                              CurrentCulture)));
            }

            Tracks     = new List<Track>();
            Partitions = new List<Partition>();
            offsetmap  = new Dictionary<uint, ulong>();
            trackFlags = new Dictionary<uint, byte>();
            ushort maxSession = 0;
            ulong  currentPos = 0;

            foreach(Bw4TrackDescriptor bwTrack in bwTracks)
                if(bwTrack.point < 0xA0)
                {
                    var track = new Track
                    {
                        TrackDescription = bwTrack.title, TrackEndSector = bwTrack.lastSector
                    };

                    if(!string.IsNullOrEmpty(bwTrack.filename))
                        do
                        {
                            track.TrackFilter =
                                filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), bwTrack.filename));

                            if(track.TrackFilter != null)
                                break;

                            track.TrackFilter =
                                filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                   bwTrack.filename.ToLower(CultureInfo.
                                                                                                CurrentCulture)));

                            if(track.TrackFilter != null)
                                break;

                            track.TrackFilter =
                                filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                   bwTrack.filename.ToUpper(CultureInfo.
                                                                                                CurrentCulture)));

                            if(track.TrackFilter != null)
                                break;

                            track.TrackFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                                   bwTrack.filename.Split(new[]
                                                                                                          {
                                                                                                              '\\'
                                                                                                          },
                                                                                                          StringSplitOptions.
                                                                                                              RemoveEmptyEntries).
                                                                                   Last()));

                            if(track.TrackFilter != null)
                                break;

                            track.TrackFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                                   bwTrack.filename.Split(new[]
                                                                                                          {
                                                                                                              '\\'
                                                                                                          },
                                                                                                          StringSplitOptions.
                                                                                                              RemoveEmptyEntries).
                                                                                           Last().
                                                                                           ToLower(CultureInfo.
                                                                                                       CurrentCulture)));

                            if(track.TrackFilter != null)
                                break;

                            track.TrackFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                                   bwTrack.filename.Split(new[]
                                                                                                          {
                                                                                                              '\\'
                                                                                                          },
                                                                                                          StringSplitOptions.
                                                                                                              RemoveEmptyEntries).
                                                                                           Last().
                                                                                           ToUpper(CultureInfo.
                                                                                                       CurrentCulture)));

                            track.TrackFilter = dataFilter;
                        } while(true);
                    else
                        track.TrackFilter = dataFilter;

                    track.TrackFile = dataFilter.GetFilename();

                    if(bwTrack.pregap != 0)
                        track.TrackFileOffset += (ulong)(bwTrack.startSector - bwTrack.pregap) * 2352;

                    track.TrackFileType          = "BINARY";
                    track.TrackPregap            = (ulong)(bwTrack.startSector - bwTrack.pregap);
                    track.TrackRawBytesPerSector = 2352;
                    track.TrackSequence          = bwTrack.point;
                    track.TrackSession           = bwTrack.session;

                    if(track.TrackSession > maxSession)
                        maxSession = track.TrackSession;

                    track.TrackStartSector      = (ulong)bwTrack.startSector;
                    track.TrackSubchannelFilter = subFilter;
                    track.TrackSubchannelFile   = subFilter.GetFilename();
                    track.TrackSubchannelOffset = (track.TrackStartSector * 96) + (track.TrackPregap * 96);

                    if(subFilter          != null &&
                       bwTrack.subchannel > 0)
                    {
                        track.TrackSubchannelType = TrackSubchannelType.Packed;

                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);
                    }
                    else
                        track.TrackSubchannelType = TrackSubchannelType.None;

                    switch(bwTrack.trackMode)
                    {
                        case Bw4TrackType.Audio:
                            track.TrackType           = TrackType.Audio;
                            imageInfo.SectorSize      = 2352;
                            track.TrackBytesPerSector = 2352;

                            break;
                        case Bw4TrackType.Mode1:
                            track.TrackType = TrackType.CdMode1;

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

                            if(imageInfo.SectorSize < 2048)
                                imageInfo.SectorSize = 2048;

                            track.TrackBytesPerSector = 2048;

                            break;
                        case Bw4TrackType.Mode2:
                            track.TrackType = TrackType.CdMode2Formless;

                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                            if(imageInfo.SectorSize < 2336)
                                imageInfo.SectorSize = 2336;

                            track.TrackBytesPerSector = 2336;

                            break;
                        default:
                            track.TrackType              = TrackType.Data;
                            track.TrackRawBytesPerSector = 2048;
                            imageInfo.SectorSize         = 2048;
                            track.TrackBytesPerSector    = 2048;

                            break;
                    }

                    if(bwTrack.pregap > 0)
                        track.Indexes.Add(0, bwTrack.pregap);

                    track.Indexes.Add(1, bwTrack.startSector);

                    var partition = new Partition();

                    if(bwTrack.pregap > 0)
                        currentPos += (ulong)(bwTrack.startSector - bwTrack.pregap) * 2352;

                    partition.Description = track.TrackDescription;
                    partition.Size        = ((track.TrackEndSector - track.TrackStartSector) + 1) * 2352;
                    partition.Length      = track.TrackEndSector - track.TrackStartSector;
                    partition.Sequence    = track.TrackSequence;
                    partition.Offset      = currentPos;
                    partition.Start       = track.TrackStartSector;
                    partition.Type        = track.TrackType.ToString();

                    Partitions.Add(partition);
                    Tracks.Add(track);
                    currentPos += partition.Size;

                    if(!offsetmap.ContainsKey(track.TrackSequence))
                        offsetmap.Add(track.TrackSequence, track.TrackStartSector);

                    if(!trackFlags.ContainsKey(track.TrackSequence))
                        trackFlags.Add(track.TrackSequence, (byte)(bwTrack.adrCtl & 0x0F));

                    imageInfo.Sectors += (ulong)((bwTrack.lastSector - bwTrack.startSector) + 1);
                }
                else
                {
                    imageInfo.MediaBarcode      = bwTrack.isrcUpc;
                    imageInfo.MediaSerialNumber = bwTrack.discId;
                    imageInfo.MediaTitle        = bwTrack.title;

                    if(!string.IsNullOrEmpty(bwTrack.isrcUpc) &&
                       !imageInfo.ReadableMediaTags.Contains(MediaTagType.CD_MCN))
                        imageInfo.ReadableMediaTags.Add(MediaTagType.CD_MCN);
                }

            Sessions = new List<Session>();

            for(ushort i = 1; i <= maxSession; i++)
            {
                var session = new Session
                {
                    SessionSequence = i, StartTrack = uint.MaxValue, StartSector = uint.MaxValue
                };

                foreach(Track track in Tracks.Where(track => track.TrackSession == i))
                {
                    if(track.TrackSequence < session.StartTrack)
                        session.StartTrack = track.TrackSequence;

                    if(track.TrackSequence > session.EndTrack)
                        session.StartTrack = track.TrackSequence;

                    if(track.TrackStartSector < session.StartSector)
                        session.StartSector = track.TrackStartSector;

                    if(track.TrackEndSector > session.EndSector)
                        session.EndSector = track.TrackEndSector;
                }

                Sessions.Add(session);
            }

            imageInfo.MediaType = MediaType.CD;

            imageInfo.Application        = "BlindWrite";
            imageInfo.ApplicationVersion = "4";
            imageInfo.Version            = "4";

            imageInfo.ImageSize            = (ulong)dataFilter.GetDataForkLength();
            imageInfo.CreationTime         = dataFilter.GetCreationTime();
            imageInfo.LastModificationTime = dataFilter.GetLastWriteTime();
            imageInfo.XmlMediaType         = XmlMediaType.OpticalDisc;

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

            imageInfo.Comments = header.Comments;

            AaruConsole.VerboseWriteLine("BlindWrite image describes a disc of type {0}", imageInfo.MediaType);

            if(!string.IsNullOrEmpty(imageInfo.Comments))
                AaruConsole.VerboseWriteLine("BlindrWrite comments: {0}", imageInfo.Comments);

            return true;
        }

        public byte[] ReadDiskTag(MediaTagType tag)
        {
            switch(tag)
            {
                case MediaTagType.CD_MCN:
                {
                    if(imageInfo.MediaSerialNumber != null)
                        return Encoding.ASCII.GetBytes(imageInfo.MediaSerialNumber);

                    throw new FeatureNotPresentImageException("Image does not contain MCN information.");
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
                                                     where sectorAddress                                   - kvp.Value <
                                                           (track.TrackEndSector - track.TrackStartSector) + 1
                                                     select kvp)
                return ReadSectors(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in offsetmap where sectorAddress      >= kvp.Value
                                                     from track in Tracks where track.TrackSequence == kvp.Key
                                                     where sectorAddress                                   - kvp.Value <
                                                           (track.TrackEndSector - track.TrackStartSector) + 1
                                                     select kvp)
                return ReadSectorsTag(sectorAddress - kvp.Value, length, kvp.Key, tag);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            var aaruTrack = new Track
            {
                TrackSequence = 0
            };

            foreach(Track bwTrack in Tracks.Where(bwTrack => bwTrack.TrackSequence == track))
            {
                aaruTrack = bwTrack;

                break;
            }

            if(aaruTrack.TrackSequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > (aaruTrack.TrackEndSector - aaruTrack.TrackStartSector) + 1)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({(aaruTrack.TrackEndSector - aaruTrack.TrackStartSector) + 1}), won't cross tracks");

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

            byte[] buffer = new byte[sectorSize * length];

            imageStream = aaruTrack.TrackFilter.GetDataForkStream();
            var br = new BinaryReader(imageStream);

            br.BaseStream.
               Seek((long)aaruTrack.TrackFileOffset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                    SeekOrigin.Begin);

            if(mode2)
            {
                var mode2Ms = new MemoryStream((int)(sectorSize * length));

                buffer = br.ReadBytes((int)(sectorSize * length));

                for(int i = 0; i < length; i++)
                {
                    byte[] sector = new byte[sectorSize];
                    Array.Copy(buffer, sectorSize * i, sector, 0, sectorSize);
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
            var aaruTrack = new Track
            {
                TrackSequence = 0
            };

            foreach(Track bwTrack in Tracks.Where(bwTrack => bwTrack.TrackSequence == track))
            {
                aaruTrack = bwTrack;

                break;
            }

            if(aaruTrack.TrackSequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > (aaruTrack.TrackEndSector - aaruTrack.TrackStartSector) + 1)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({(aaruTrack.TrackEndSector - aaruTrack.TrackStartSector) + 1}), won't cross tracks");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

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
                    if(trackFlags.TryGetValue((uint)sectorAddress, out byte flag))
                        return new[]
                        {
                            flag
                        };

                    throw new ArgumentException("Unsupported tag requested", nameof(tag));
                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
            }

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
                                                     where sectorAddress                                   - kvp.Value <
                                                           (track.TrackEndSector - track.TrackStartSector) + 1
                                                     select kvp)
                return ReadSectorsLong(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            var aaruTrack = new Track
            {
                TrackSequence = 0
            };

            foreach(Track bwTrack in Tracks.Where(bwTrack => bwTrack.TrackSequence == track))
            {
                aaruTrack = bwTrack;

                break;
            }

            if(aaruTrack.TrackSequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > (aaruTrack.TrackEndSector - aaruTrack.TrackStartSector) + 1)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({(aaruTrack.TrackEndSector - aaruTrack.TrackStartSector) + 1}), won't cross tracks");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch(aaruTrack.TrackType)
            {
                case TrackType.Audio:
                case TrackType.CdMode1:
                case TrackType.CdMode2Formless:
                case TrackType.Data:
                {
                    sectorOffset = 0;
                    sectorSize   = (uint)aaruTrack.TrackRawBytesPerSector;
                    sectorSkip   = 0;

                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            imageStream = aaruTrack.TrackFilter.GetDataForkStream();
            var br = new BinaryReader(imageStream);

            br.BaseStream.
               Seek((long)aaruTrack.TrackFileOffset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                    SeekOrigin.Begin);

            byte[] buffer = br.ReadBytes((int)(sectorSize * length));

            return buffer;
        }

        public List<Track> GetSessionTracks(Session session)
        {
            if(Sessions.Contains(session))
                return GetSessionTracks(session.SessionSequence);

            throw new ImageNotSupportedException("Session does not exist in disc image");
        }

        public List<Track> GetSessionTracks(ushort session) =>
            Tracks.Where(track => track.TrackSession == session).ToList();
    }
}