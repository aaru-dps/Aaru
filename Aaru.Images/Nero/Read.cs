﻿// /***************************************************************************
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
//     Reads Nero Burning ROM disc images.
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
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public sealed partial class Nero
    {
        /// <inheritdoc />
        public bool Open(IFilter imageFilter)
        {
            try
            {
                // Look for the footer
                _imageStream = imageFilter.GetDataForkStream();
                var footerV1 = new FooterV1();
                var footerV2 = new FooterV2();

                _imageStream.Seek(-8, SeekOrigin.End);
                byte[] buffer = new byte[8];
                _imageStream.Read(buffer, 0, 8);
                footerV1.ChunkId          = BigEndianBitConverter.ToUInt32(buffer, 0);
                footerV1.FirstChunkOffset = BigEndianBitConverter.ToUInt32(buffer, 4);

                _imageStream.Seek(-12, SeekOrigin.End);
                buffer = new byte[12];
                _imageStream.Read(buffer, 0, 12);
                footerV2.ChunkId          = BigEndianBitConverter.ToUInt32(buffer, 0);
                footerV2.FirstChunkOffset = BigEndianBitConverter.ToUInt64(buffer, 4);

                AaruConsole.DebugWriteLine("Nero plugin", "imageStream.Length = {0}", _imageStream.Length);

                AaruConsole.DebugWriteLine("Nero plugin", "footerV1.ChunkID = 0x{0:X8} (\"{1}\")", footerV1.ChunkId,
                                           Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(footerV1.ChunkId)));

                AaruConsole.DebugWriteLine("Nero plugin", "footerV1.FirstChunkOffset = {0}", footerV1.FirstChunkOffset);

                AaruConsole.DebugWriteLine("Nero plugin", "footerV2.ChunkID = 0x{0:X8} (\"{1}\")", footerV2.ChunkId,
                                           Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(footerV2.ChunkId)));

                AaruConsole.DebugWriteLine("Nero plugin", "footerV2.FirstChunkOffset = {0}", footerV2.FirstChunkOffset);

                // Check footer version
                if(footerV1.ChunkId          == NERO_FOOTER_V1 &&
                   footerV1.FirstChunkOffset < (ulong)_imageStream.Length)
                    _imageNewFormat = false;
                else if(footerV2.ChunkId          == NERO_FOOTER_V2 &&
                        footerV2.FirstChunkOffset < (ulong)_imageStream.Length)
                    _imageNewFormat = true;
                else
                {
                    AaruConsole.DebugWrite("Nero plugin", "Nero version not recognized.");

                    return false;
                }

                // Seek to first chunk
                if(_imageNewFormat)
                    _imageStream.Seek((long)footerV2.FirstChunkOffset, SeekOrigin.Begin);
                else
                    _imageStream.Seek(footerV1.FirstChunkOffset, SeekOrigin.Begin);

                bool   parsing        = true;
                ushort currentSession = 1;
                uint   currentTrack   = 1;

                Tracks      = new List<Track>();
                _trackIsrCs = new Dictionary<uint, byte[]>();

                _imageInfo.MediaType  = CommonTypes.MediaType.CD;
                _imageInfo.Sectors    = 0;
                _imageInfo.SectorSize = 0;
                bool oldFormat          = false;
                int  currentLba         = -150;
                bool corruptedTrackMode = false;

                // Parse chunks
                while(parsing)
                {
                    byte[] chunkHeaderBuffer = new byte[8];

                    _imageStream.Read(chunkHeaderBuffer, 0, 8);
                    uint chunkId     = BigEndianBitConverter.ToUInt32(chunkHeaderBuffer, 0);
                    uint chunkLength = BigEndianBitConverter.ToUInt32(chunkHeaderBuffer, 4);

                    AaruConsole.DebugWriteLine("Nero plugin", "ChunkID = 0x{0:X8} (\"{1}\")", chunkId,
                                               Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(chunkId)));

                    AaruConsole.DebugWriteLine("Nero plugin", "ChunkLength = {0}", chunkLength);

                    switch(chunkId)
                    {
                        case NERO_CUE_V1:
                        {
                            AaruConsole.DebugWriteLine("Nero plugin", "Found \"CUES\" chunk, parsing {0} bytes",
                                                       chunkLength);

                            var newCuesheetV1 = new CuesheetV1
                            {
                                ChunkId   = chunkId,
                                ChunkSize = chunkLength,
                                Entries   = new List<CueEntryV1>()
                            };

                            byte[] tmpBuffer = new byte[8];

                            for(int i = 0; i < newCuesheetV1.ChunkSize; i += 8)
                            {
                                var entry = new CueEntryV1();
                                _imageStream.Read(tmpBuffer, 0, 8);
                                entry.Mode = tmpBuffer[0];

                                entry.TrackNumber = (byte)((((tmpBuffer[1] & 0xF0) >> 4) * 10) + (tmpBuffer[1] & 0xF));
                                entry.IndexNumber = (byte)((((tmpBuffer[2] & 0xF0) >> 4) * 10) + (tmpBuffer[2] & 0xF));
                                entry.Dummy       = BigEndianBitConverter.ToUInt16(tmpBuffer, 3);
                                entry.Minute      = (byte)((((tmpBuffer[5] & 0xF0) >> 4) * 10) + (tmpBuffer[5] & 0xF));
                                entry.Second      = (byte)((((tmpBuffer[6] & 0xF0) >> 4) * 10) + (tmpBuffer[6] & 0xF));
                                entry.Frame       = (byte)((((tmpBuffer[7] & 0xF0) >> 4) * 10) + (tmpBuffer[7] & 0xF));

                                AaruConsole.DebugWriteLine("Nero plugin", "Cuesheet entry {0}", (i / 8) + 1);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Mode = {1:X2}", (i / 8) + 1,
                                                           entry.Mode);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].TrackNumber = {1:X2}",
                                                           (i / 8) + 1, entry.TrackNumber);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].IndexNumber = {1:X2}",
                                                           (i / 8) + 1, entry.IndexNumber);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Dummy = {1:X4}", (i / 8) + 1,
                                                           entry.Dummy);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Minute = {1:X2}", (i / 8) + 1,
                                                           entry.Minute);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Second = {1:X2}", (i / 8) + 1,
                                                           entry.Second);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Frame = {1:X2}", (i / 8) + 1,
                                                           entry.Frame);

                                newCuesheetV1.Entries.Add(entry);
                            }

                            if(_cuesheetV1 is null)
                                _cuesheetV1 = newCuesheetV1;
                            else
                                _cuesheetV1.Entries.AddRange(newCuesheetV1.Entries);

                            break;
                        }

                        case NERO_CUE_V2:
                        {
                            AaruConsole.DebugWriteLine("Nero plugin", "Found \"CUEX\" chunk, parsing {0} bytes",
                                                       chunkLength);

                            var newCuesheetV2 = new CuesheetV2
                            {
                                ChunkId   = chunkId,
                                ChunkSize = chunkLength,
                                Entries   = new List<CueEntryV2>()
                            };

                            byte[] tmpBuffer = new byte[8];

                            for(int i = 0; i < newCuesheetV2.ChunkSize; i += 8)
                            {
                                var entry = new CueEntryV2();
                                _imageStream.Read(tmpBuffer, 0, 8);
                                entry.Mode        = tmpBuffer[0];
                                entry.TrackNumber = (byte)((((tmpBuffer[1] & 0xF0) >> 4) * 10) + (tmpBuffer[1] & 0xF));
                                entry.IndexNumber = (byte)((((tmpBuffer[2] & 0xF0) >> 4) * 10) + (tmpBuffer[2] & 0xF));
                                entry.Dummy       = tmpBuffer[3];
                                entry.LbaStart    = BigEndianBitConverter.ToInt32(tmpBuffer, 4);

                                AaruConsole.DebugWriteLine("Nero plugin", "Cuesheet entry {0}", (i / 8) + 1);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Mode = 0x{1:X2}", (i / 8) + 1,
                                                           entry.Mode);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].TrackNumber = {1:X2}",
                                                           (i / 8) + 1, entry.TrackNumber);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].IndexNumber = {1:X2}",
                                                           (i / 8) + 1, entry.IndexNumber);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Dummy = {1:X2}", (i / 8) + 1,
                                                           entry.Dummy);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].LBAStart = {1}", (i / 8) + 1,
                                                           entry.LbaStart);

                                newCuesheetV2.Entries.Add(entry);
                            }

                            if(_cuesheetV2 is null)
                                _cuesheetV2 = newCuesheetV2;
                            else
                                _cuesheetV2.Entries.AddRange(newCuesheetV2.Entries);

                            break;
                        }

                        case NERO_DAO_V1:
                        {
                            AaruConsole.DebugWriteLine("Nero plugin", "Found \"DAOI\" chunk, parsing {0} bytes",
                                                       chunkLength);

                            _neroDaov1 = new DaoV1
                            {
                                ChunkId     = chunkId,
                                ChunkSizeBe = chunkLength
                            };

                            byte[] tmpBuffer = new byte[22];
                            _imageStream.Read(tmpBuffer, 0, 22);
                            _neroDaov1.ChunkSizeLe = BigEndianBitConverter.ToUInt32(tmpBuffer, 0);
                            _neroDaov1.Upc         = new byte[14];
                            Array.Copy(tmpBuffer, 4, _neroDaov1.Upc, 0, 14);
                            _neroDaov1.TocType    = BigEndianBitConverter.ToUInt16(tmpBuffer, 18);
                            _neroDaov1.FirstTrack = tmpBuffer[20];
                            _neroDaov1.LastTrack  = tmpBuffer[21];
                            _neroDaov1.Tracks     = new List<DaoEntryV1>();

                            AaruConsole.DebugWriteLine("Nero plugin", "neroDAOV1.ChunkSizeLe = {0} bytes",
                                                       _neroDaov1.ChunkSizeLe);

                            AaruConsole.DebugWriteLine("Nero plugin", "neroDAOV1.UPC = \"{0}\"",
                                                       StringHandlers.CToString(_neroDaov1.Upc));

                            AaruConsole.DebugWriteLine("Nero plugin", "neroDAOV1.TocType = 0x{0:X4}",
                                                       _neroDaov1.TocType);

                            AaruConsole.DebugWriteLine("Nero plugin", "neroDAOV1.FirstTrack = {0}",
                                                       _neroDaov1.FirstTrack);

                            AaruConsole.DebugWriteLine("Nero plugin", "neroDAOV1.LastTrack = {0}",
                                                       _neroDaov1.LastTrack);

                            _upc = _neroDaov1.Upc;

                            tmpBuffer = new byte[30];

                            for(int i = 0; i < _neroDaov1.ChunkSizeBe - 22; i += 30)
                            {
                                var entry = new DaoEntryV1();
                                _imageStream.Read(tmpBuffer, 0, 30);
                                entry.Isrc = new byte[12];
                                Array.Copy(tmpBuffer, 4, entry.Isrc, 0, 12);
                                entry.SectorSize = BigEndianBitConverter.ToUInt16(tmpBuffer, 12);
                                entry.Mode       = BitConverter.ToUInt16(tmpBuffer, 14);
                                entry.Unknown    = BigEndianBitConverter.ToUInt16(tmpBuffer, 16);
                                entry.Index0     = BigEndianBitConverter.ToUInt32(tmpBuffer, 18);
                                entry.Index1     = BigEndianBitConverter.ToUInt32(tmpBuffer, 22);
                                entry.EndOfTrack = BigEndianBitConverter.ToUInt32(tmpBuffer, 26);

                                // MagicISO
                                if(entry.SectorSize == 2352)
                                {
                                    if(entry.Mode == 0x0000)
                                    {
                                        corruptedTrackMode = true;
                                        entry.Mode         = 0x0005;
                                    }
                                    else if(entry.Mode == 0x0002 ||
                                            entry.Mode == 0x0003)
                                    {
                                        corruptedTrackMode = true;
                                        entry.Mode         = 0x0006;
                                    }
                                }

                                AaruConsole.DebugWriteLine("Nero plugin", "Disc-At-Once entry {0}", (i / 32) + 1);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].ISRC = \"{1}\"", (i / 32) + 1,
                                                           StringHandlers.CToString(entry.Isrc));

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].SectorSize = {1}",
                                                           (i / 32) + 1, entry.SectorSize);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Mode = {1} (0x{2:X4})",
                                                           (i / 32) + 1, (DaoMode)entry.Mode, entry.Mode);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Unknown = 0x{1:X4}",
                                                           (i / 32) + 1, entry.Unknown);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Index0 = {1}", (i / 32) + 1,
                                                           entry.Index0);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Index1 = {1}", (i / 32) + 1,
                                                           entry.Index1);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].EndOfTrack = {1}",
                                                           (i / 32) + 1, entry.EndOfTrack);

                                _neroDaov1.Tracks.Add(entry);

                                if(entry.SectorSize > _imageInfo.SectorSize)
                                    _imageInfo.SectorSize = entry.SectorSize;

                                _trackIsrCs.Add(currentTrack, entry.Isrc);

                                var neroTrack = new NeroTrack
                                {
                                    EndOfTrack = entry.EndOfTrack,
                                    Isrc       = entry.Isrc,
                                    Length     = entry.EndOfTrack - entry.Index0,
                                    Mode       = entry.Mode,
                                    Offset     = entry.Index0,
                                    SectorSize = entry.SectorSize,
                                    Index0     = entry.Index0,
                                    Index1     = entry.Index1,
                                    Sequence   = currentTrack
                                };

                                _neroTracks.Add(currentTrack, neroTrack);
                                currentTrack++;
                            }

                            break;
                        }

                        case NERO_DAO_V2:
                        {
                            AaruConsole.DebugWriteLine("Nero plugin", "Found \"DAOX\" chunk, parsing {0} bytes",
                                                       chunkLength);

                            _neroDaov2 = new DaoV2
                            {
                                ChunkId     = chunkId,
                                ChunkSizeBe = chunkLength
                            };

                            byte[] tmpBuffer = new byte[22];
                            _imageStream.Read(tmpBuffer, 0, 22);
                            _neroDaov2.ChunkSizeLe = BigEndianBitConverter.ToUInt32(tmpBuffer, 0);
                            _neroDaov2.Upc         = new byte[14];
                            Array.Copy(tmpBuffer, 4, _neroDaov2.Upc, 0, 14);
                            _neroDaov2.TocType    = BigEndianBitConverter.ToUInt16(tmpBuffer, 18);
                            _neroDaov2.FirstTrack = tmpBuffer[20];
                            _neroDaov2.LastTrack  = tmpBuffer[21];
                            _neroDaov2.Tracks     = new List<DaoEntryV2>();

                            _upc = _neroDaov2.Upc;

                            AaruConsole.DebugWriteLine("Nero plugin", "neroDAOV2.ChunkSizeLe = {0} bytes",
                                                       _neroDaov2.ChunkSizeLe);

                            AaruConsole.DebugWriteLine("Nero plugin", "neroDAOV2.UPC = \"{0}\"",
                                                       StringHandlers.CToString(_neroDaov2.Upc));

                            AaruConsole.DebugWriteLine("Nero plugin", "neroDAOV2.TocType = 0x{0:X4}",
                                                       _neroDaov2.TocType);

                            AaruConsole.DebugWriteLine("Nero plugin", "neroDAOV2.FirstTrack = {0}",
                                                       _neroDaov2.FirstTrack);

                            AaruConsole.DebugWriteLine("Nero plugin", "neroDAOV2.LastTrack = {0}",
                                                       _neroDaov2.LastTrack);

                            tmpBuffer = new byte[42];

                            for(int i = 0; i < _neroDaov2.ChunkSizeBe - 22; i += 42)
                            {
                                var entry = new DaoEntryV2();
                                _imageStream.Read(tmpBuffer, 0, 42);
                                entry.Isrc = new byte[12];
                                Array.Copy(tmpBuffer, 4, entry.Isrc, 0, 12);
                                entry.SectorSize = BigEndianBitConverter.ToUInt16(tmpBuffer, 12);
                                entry.Mode       = BitConverter.ToUInt16(tmpBuffer, 14);
                                entry.Unknown    = BigEndianBitConverter.ToUInt16(tmpBuffer, 16);
                                entry.Index0     = BigEndianBitConverter.ToUInt64(tmpBuffer, 18);
                                entry.Index1     = BigEndianBitConverter.ToUInt64(tmpBuffer, 26);
                                entry.EndOfTrack = BigEndianBitConverter.ToUInt64(tmpBuffer, 34);

                                // MagicISO
                                if(entry.SectorSize == 2352)
                                {
                                    if(entry.Mode == 0x0000)
                                    {
                                        corruptedTrackMode = true;
                                        entry.Mode         = 0x0005;
                                    }
                                    else if(entry.Mode == 0x0002 ||
                                            entry.Mode == 0x0003)
                                    {
                                        corruptedTrackMode = true;
                                        entry.Mode         = 0x0006;
                                    }
                                }

                                AaruConsole.DebugWriteLine("Nero plugin", "Disc-At-Once entry {0}", (i / 32) + 1);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].ISRC = \"{1}\"", (i / 32) + 1,
                                                           StringHandlers.CToString(entry.Isrc));

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].SectorSize = {1}",
                                                           (i / 32) + 1, entry.SectorSize);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Mode = {1} (0x{2:X4})",
                                                           (i / 32) + 1, (DaoMode)entry.Mode, entry.Mode);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Unknown = {1:X2}",
                                                           (i / 32) + 1, entry.Unknown);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Index0 = {1}", (i / 32) + 1,
                                                           entry.Index0);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Index1 = {1}", (i / 32) + 1,
                                                           entry.Index1);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].EndOfTrack = {1}",
                                                           (i / 32) + 1, entry.EndOfTrack);

                                _neroDaov2.Tracks.Add(entry);

                                if(entry.SectorSize > _imageInfo.SectorSize)
                                    _imageInfo.SectorSize = entry.SectorSize;

                                _trackIsrCs.Add(currentTrack, entry.Isrc);

                                var neroTrack = new NeroTrack
                                {
                                    EndOfTrack = entry.EndOfTrack,
                                    Isrc       = entry.Isrc,
                                    Length     = entry.EndOfTrack - entry.Index0,
                                    Mode       = entry.Mode,
                                    Offset     = entry.Index0,
                                    SectorSize = entry.SectorSize,
                                    Index0     = entry.Index0,
                                    Index1     = entry.Index1,
                                    Sequence   = currentTrack
                                };

                                _neroTracks.Add(currentTrack, neroTrack);

                                currentTrack++;
                            }

                            break;
                        }

                        case NERO_CDTEXT:
                        {
                            AaruConsole.DebugWriteLine("Nero plugin", "Found \"CDTX\" chunk, parsing {0} bytes",
                                                       chunkLength);

                            _cdtxt = new CdText
                            {
                                ChunkId   = chunkId,
                                ChunkSize = chunkLength,
                                Packs     = new List<CdTextPack>()
                            };

                            byte[] tmpBuffer = new byte[18];

                            for(int i = 0; i < _cdtxt.ChunkSize; i += 18)
                            {
                                var entry = new CdTextPack();
                                _imageStream.Read(tmpBuffer, 0, 18);

                                entry.PackType    = tmpBuffer[0];
                                entry.TrackNumber = tmpBuffer[1];
                                entry.PackNumber  = tmpBuffer[2];
                                entry.BlockNumber = tmpBuffer[3];
                                entry.Text        = new byte[12];
                                Array.Copy(tmpBuffer, 4, entry.Text, 0, 12);
                                entry.Crc = BigEndianBitConverter.ToUInt16(tmpBuffer, 16);

                                AaruConsole.DebugWriteLine("Nero plugin", "CD-TEXT entry {0}", (i / 18) + 1);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].PackType = 0x{1:X2}",
                                                           (i / 18) + 1, entry.PackType);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].TrackNumber = 0x{1:X2}",
                                                           (i / 18) + 1, entry.TrackNumber);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].PackNumber = 0x{1:X2}",
                                                           (i / 18) + 1, entry.PackNumber);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].BlockNumber = 0x{1:X2}",
                                                           (i / 18) + 1, entry.BlockNumber);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Text = \"{1}\"", (i / 18) + 1,
                                                           StringHandlers.CToString(entry.Text));

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].CRC = 0x{1:X4}", (i / 18) + 1,
                                                           entry.Crc);

                                _cdtxt.Packs.Add(entry);
                            }

                            break;
                        }

                        case NERO_TAO_V0:
                        {
                            AaruConsole.DebugWriteLine("Nero plugin", "Found \"TINF\" chunk, parsing {0} bytes",
                                                       chunkLength);

                            oldFormat = true;

                            _taoV0 = new TaoV0
                            {
                                ChunkId   = chunkId,
                                ChunkSize = chunkLength,
                                Tracks    = new List<TaoEntryV0>()
                            };

                            byte[] tmpBuffer = new byte[12];

                            for(int i = 0; i < _taoV0.ChunkSize; i += 12)
                            {
                                var entry = new TaoEntryV0();
                                _imageStream.Read(tmpBuffer, 0, 12);

                                entry.Offset = BigEndianBitConverter.ToUInt32(tmpBuffer, 0);
                                entry.Length = BigEndianBitConverter.ToUInt32(tmpBuffer, 4);
                                entry.Mode   = BigEndianBitConverter.ToUInt32(tmpBuffer, 8);

                                AaruConsole.DebugWriteLine("Nero plugin", "Track-at-Once entry {0}", (i / 20) + 1);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Offset = {1}", (i / 20) + 1,
                                                           entry.Offset);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Length = {1} bytes",
                                                           (i / 20) + 1, entry.Length);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Mode = {1} (0x{2:X4})",
                                                           (i / 20) + 1, (DaoMode)entry.Mode, entry.Mode);

                                _taoV0.Tracks.Add(entry);

                                if(NeroTrackModeToBytesPerSector((DaoMode)entry.Mode) > _imageInfo.SectorSize)
                                    _imageInfo.SectorSize = NeroTrackModeToBytesPerSector((DaoMode)entry.Mode);

                                // StartLba points to INDEX 0 and Nero always introduces a pregap of 150 sectors,

                                var neroTrack = new NeroTrack
                                {
                                    EndOfTrack     = entry.Offset + entry.Length,
                                    Isrc           = new byte[12],
                                    Length         = entry.Length,
                                    Mode           = entry.Mode,
                                    Offset         = entry.Offset,
                                    SectorSize     = NeroTrackModeToBytesPerSector((DaoMode)entry.Mode),
                                    UseLbaForIndex = true,
                                    Sequence       = currentTrack,
                                    StartLba       = currentLba > 0 ? (ulong)currentLba : 0
                                };

                                _neroTracks.Add(currentTrack, neroTrack);

                                currentTrack++;
                                currentLba += (int)(neroTrack.Length / neroTrack.SectorSize);
                            }

                            break;
                        }

                        case NERO_TAO_V1:
                        {
                            AaruConsole.DebugWriteLine("Nero plugin", "Found \"ETNF\" chunk, parsing {0} bytes",
                                                       chunkLength);

                            _taoV1 = new TaoV1
                            {
                                ChunkId   = chunkId,
                                ChunkSize = chunkLength,
                                Tracks    = new List<TaoEntryV1>()
                            };

                            byte[] tmpBuffer = new byte[20];

                            for(int i = 0; i < _taoV1.ChunkSize; i += 20)
                            {
                                var entry = new TaoEntryV1();
                                _imageStream.Read(tmpBuffer, 0, 20);

                                entry.Offset   = BigEndianBitConverter.ToUInt32(tmpBuffer, 0);
                                entry.Length   = BigEndianBitConverter.ToUInt32(tmpBuffer, 4);
                                entry.Mode     = BigEndianBitConverter.ToUInt32(tmpBuffer, 8);
                                entry.StartLba = BigEndianBitConverter.ToUInt32(tmpBuffer, 12);
                                entry.Unknown  = BigEndianBitConverter.ToUInt32(tmpBuffer, 16);

                                AaruConsole.DebugWriteLine("Nero plugin", "Track-at-Once entry {0}", (i / 20) + 1);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Offset = {1}", (i / 20) + 1,
                                                           entry.Offset);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Length = {1} bytes",
                                                           (i / 20) + 1, entry.Length);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Mode = {1} (0x{2:X4})",
                                                           (i / 20) + 1, (DaoMode)entry.Mode, entry.Mode);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].StartLBA = {1}", (i / 20) + 1,
                                                           entry.StartLba);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Unknown = 0x{1:X4}",
                                                           (i / 20) + 1, entry.Unknown);

                                _taoV1.Tracks.Add(entry);

                                if(NeroTrackModeToBytesPerSector((DaoMode)entry.Mode) > _imageInfo.SectorSize)
                                    _imageInfo.SectorSize = NeroTrackModeToBytesPerSector((DaoMode)entry.Mode);

                                // StartLba points to INDEX 0 and Nero always introduces a pregap of 150 sectors,

                                var neroTrack = new NeroTrack
                                {
                                    EndOfTrack     = entry.Offset + entry.Length,
                                    Isrc           = new byte[12],
                                    Length         = entry.Length,
                                    Mode           = entry.Mode,
                                    Offset         = entry.Offset,
                                    SectorSize     = NeroTrackModeToBytesPerSector((DaoMode)entry.Mode),
                                    UseLbaForIndex = true,
                                    Sequence       = currentTrack,
                                    StartLba       = entry.StartLba
                                };

                                _neroTracks.Add(currentTrack, neroTrack);

                                currentTrack++;
                            }

                            break;
                        }

                        case NERO_TAO_V2:
                        {
                            AaruConsole.DebugWriteLine("Nero plugin", "Found \"ETN2\" chunk, parsing {0} bytes",
                                                       chunkLength);

                            _taoV2 = new TaoV2
                            {
                                ChunkId   = chunkId,
                                ChunkSize = chunkLength,
                                Tracks    = new List<TaoEntryV2>()
                            };

                            byte[] tmpBuffer = new byte[32];

                            for(int i = 0; i < _taoV2.ChunkSize; i += 32)
                            {
                                var entry = new TaoEntryV2();
                                _imageStream.Read(tmpBuffer, 0, 32);

                                entry.Offset   = BigEndianBitConverter.ToUInt64(tmpBuffer, 0);
                                entry.Length   = BigEndianBitConverter.ToUInt64(tmpBuffer, 8);
                                entry.Mode     = BigEndianBitConverter.ToUInt32(tmpBuffer, 16);
                                entry.StartLba = BigEndianBitConverter.ToUInt32(tmpBuffer, 20);
                                entry.Unknown  = BigEndianBitConverter.ToUInt32(tmpBuffer, 24);
                                entry.Sectors  = BigEndianBitConverter.ToUInt32(tmpBuffer, 28);

                                AaruConsole.DebugWriteLine("Nero plugin", "Track-at-Once entry {0}", (i / 32) + 1);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Offset = {1}", (i / 32) + 1,
                                                           entry.Offset);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Length = {1} bytes",
                                                           (i / 32) + 1, entry.Length);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Mode = {1} (0x{2:X4})",
                                                           (i / 32) + 1, (DaoMode)entry.Mode, entry.Mode);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].StartLBA = {1}", (i / 32) + 1,
                                                           entry.StartLba);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Unknown = 0x{1:X4}",
                                                           (i / 32) + 1, entry.Unknown);

                                AaruConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Sectors = {1}", (i / 32) + 1,
                                                           entry.Sectors);

                                _taoV2.Tracks.Add(entry);

                                ushort v2Bps = NeroTrackModeToBytesPerSector((DaoMode)entry.Mode);

                                if(NeroTrackModeToBytesPerSector((DaoMode)entry.Mode) > _imageInfo.SectorSize)
                                    _imageInfo.SectorSize = v2Bps;

                                // StartLba points to INDEX 1 and Nero always introduces a pregap of 150 sectors,
                                // so let's move it to INDEX 0 and fix the pointers
                                if(entry.StartLba >= 150)
                                {
                                    entry.StartLba -= 150;
                                    entry.Offset   -= (ulong)(150 * v2Bps);
                                    entry.Length   += (ulong)(150 * v2Bps);
                                }

                                var neroTrack = new NeroTrack
                                {
                                    EndOfTrack     = entry.Offset + entry.Length,
                                    Isrc           = new byte[12],
                                    Length         = entry.Length,
                                    Mode           = entry.Mode,
                                    Offset         = entry.Offset,
                                    StartLba       = entry.StartLba,
                                    UseLbaForIndex = true,
                                    SectorSize     = NeroTrackModeToBytesPerSector((DaoMode)entry.Mode),
                                    Sequence       = currentTrack
                                };

                                _neroTracks.Add(currentTrack, neroTrack);

                                currentTrack++;
                            }

                            break;
                        }

                        case NERO_SESSION:
                        {
                            AaruConsole.DebugWriteLine("Nero plugin", "Found \"SINF\" chunk, parsing {0} bytes",
                                                       chunkLength);

                            byte[] tmpBuffer = new byte[4];
                            _imageStream.Read(tmpBuffer, 0, 4);
                            uint sessionTracks = BigEndianBitConverter.ToUInt32(tmpBuffer, 0);
                            _neroSessions.Add(currentSession, sessionTracks);

                            AaruConsole.DebugWriteLine("Nero plugin", "\tSession {0} has {1} tracks", currentSession,
                                                       sessionTracks);

                            currentSession++;

                            break;
                        }

                        case NERO_DISC_TYPE:
                        {
                            AaruConsole.DebugWriteLine("Nero plugin", "Found \"MTYP\" chunk, parsing {0} bytes",
                                                       chunkLength);

                            _mediaType = new MediaType
                            {
                                ChunkId   = chunkId,
                                ChunkSize = chunkLength
                            };

                            byte[] tmpBuffer = new byte[4];
                            _imageStream.Read(tmpBuffer, 0, 4);
                            _mediaType.Type = BigEndianBitConverter.ToUInt32(tmpBuffer, 0);

                            AaruConsole.DebugWriteLine("Nero plugin", "\tMedia type is {0} ({1})",
                                                       (NeroMediaTypes)_mediaType.Type, _mediaType.Type);

                            _imageInfo.MediaType = NeroMediaTypeToMediaType((NeroMediaTypes)_mediaType.Type);

                            break;
                        }

                        case NERO_DISC_INFO:
                        {
                            AaruConsole.DebugWriteLine("Nero plugin", "Found \"DINF\" chunk, parsing {0} bytes",
                                                       chunkLength);

                            _discInfo = new DiscInformation
                            {
                                ChunkId   = chunkId,
                                ChunkSize = chunkLength
                            };

                            byte[] tmpBuffer = new byte[4];
                            _imageStream.Read(tmpBuffer, 0, 4);
                            _discInfo.Unknown = BigEndianBitConverter.ToUInt32(tmpBuffer, 0);

                            AaruConsole.DebugWriteLine("Nero plugin", "\tneroDiscInfo.Unknown = 0x{0:X4} ({0})",
                                                       _discInfo.Unknown);

                            break;
                        }

                        case NERO_RELOCATION:
                        {
                            AaruConsole.DebugWriteLine("Nero plugin", "Found \"RELO\" chunk, parsing {0} bytes",
                                                       chunkLength);

                            _relo = new ReloChunk
                            {
                                ChunkId   = chunkId,
                                ChunkSize = chunkLength
                            };

                            byte[] tmpBuffer = new byte[4];
                            _imageStream.Read(tmpBuffer, 0, 4);
                            _relo.Unknown = BigEndianBitConverter.ToUInt32(tmpBuffer, 0);

                            AaruConsole.DebugWriteLine("Nero plugin", "\tneroRELO.Unknown = 0x{0:X4} ({0})",
                                                       _relo.Unknown);

                            break;
                        }

                        case NERO_TOC:
                        {
                            AaruConsole.DebugWriteLine("Nero plugin", "Found \"TOCT\" chunk, parsing {0} bytes",
                                                       chunkLength);

                            _toc = new TocChunk
                            {
                                ChunkId   = chunkId,
                                ChunkSize = chunkLength
                            };

                            byte[] tmpBuffer = new byte[2];
                            _imageStream.Read(tmpBuffer, 0, 2);
                            _toc.Unknown = BigEndianBitConverter.ToUInt16(tmpBuffer, 0);

                            switch(tmpBuffer[0])
                            {
                                case 0:
                                    _imageInfo.MediaType = CommonTypes.MediaType.CDROM;

                                    break;
                                case 0x10:
                                    _imageInfo.MediaType = CommonTypes.MediaType.CDI;

                                    break;
                                case 0x20:
                                    _imageInfo.MediaType = CommonTypes.MediaType.CDROMXA;

                                    break;
                            }

                            AaruConsole.DebugWriteLine("Nero plugin", "\tneroTOC.Unknown = 0x{0:X4} ({0})",
                                                       _toc.Unknown);

                            break;
                        }

                        case NERO_END:
                        {
                            AaruConsole.DebugWriteLine("Nero plugin", "Found \"END!\" chunk, finishing parse");
                            parsing = false;

                            break;
                        }

                        default:
                        {
                            AaruConsole.DebugWriteLine("Nero plugin", "Unknown chunk ID \"{0}\", skipping...",
                                                       Encoding.ASCII.GetString(BigEndianBitConverter.
                                                                                    GetBytes(chunkId)));

                            _imageStream.Seek(chunkLength, SeekOrigin.Current);

                            break;
                        }
                    }
                }

                if(corruptedTrackMode)
                    AaruConsole.
                        ErrorWriteLine("Inconsistent track mode and track sector size found. A best try to fix has been done. It is recommended this disc is dumped with another software.");

                _imageInfo.HasPartitions         = true;
                _imageInfo.HasSessions           = true;
                _imageInfo.Creator               = null;
                _imageInfo.CreationTime          = imageFilter.GetCreationTime();
                _imageInfo.LastModificationTime  = imageFilter.GetLastWriteTime();
                _imageInfo.MediaTitle            = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
                _imageInfo.Comments              = null;
                _imageInfo.MediaManufacturer     = null;
                _imageInfo.MediaModel            = null;
                _imageInfo.MediaSerialNumber     = null;
                _imageInfo.MediaBarcode          = null;
                _imageInfo.MediaPartNumber       = null;
                _imageInfo.DriveManufacturer     = null;
                _imageInfo.DriveModel            = null;
                _imageInfo.DriveSerialNumber     = null;
                _imageInfo.DriveFirmwareRevision = null;
                _imageInfo.MediaSequence         = 0;
                _imageInfo.LastMediaSequence     = 0;

                _imageInfo.Application = "Nero Burning ROM";

                if(_imageNewFormat)
                {
                    _imageInfo.ImageSize          = footerV2.FirstChunkOffset;
                    _imageInfo.Version            = "Nero Burning ROM >= 5.5";
                    _imageInfo.ApplicationVersion = ">= 5.5";
                }
                else if(oldFormat)
                {
                    _imageInfo.ImageSize          = footerV1.FirstChunkOffset;
                    _imageInfo.Version            = "Nero Burning ROM 4";
                    _imageInfo.ApplicationVersion = "4";
                }
                else
                {
                    _imageInfo.ImageSize          = footerV1.FirstChunkOffset;
                    _imageInfo.Version            = "Nero Burning ROM <= 5.0";
                    _imageInfo.ApplicationVersion = "<= 5.0";
                }

                if(_neroSessions.Count == 0)
                    _neroSessions.Add(1, currentTrack);

                AaruConsole.DebugWriteLine("Nero plugin", "Building offset, track and session maps");

                bool onlyOneSession = currentSession == 1 || currentSession == 2;
                currentSession = 1;
                _neroSessions.TryGetValue(1, out uint currentSessionMaxTrack);
                uint  currentSessionCurrentTrack = 1;
                var   currentSessionStruct       = new CommonTypes.Structs.Session();
                ulong partitionSequence          = 0;
                ulong partitionStartByte         = 0;
                int   trackCounter               = 1;
                _trackFlags = new Dictionary<uint, byte>();

                if(currentSessionMaxTrack == 0)
                    currentSessionMaxTrack = 1;

                bool moreTracksThanSessionTracks = currentSessionMaxTrack < _neroTracks.Count;

                // Process tracks
                foreach(NeroTrack neroTrack in _neroTracks.Values)
                {
                    if(neroTrack.Offset >= (_imageNewFormat ? footerV2.FirstChunkOffset : footerV1.FirstChunkOffset))
                    {
                        AaruConsole.
                            ErrorWriteLine("This image contains a track that is set to start outside the file.");

                        AaruConsole.ErrorWriteLine("Breaking track processing and trying recovery of information.");

                        break;
                    }

                    AaruConsole.DebugWriteLine("Nero plugin", "\tcurrentSession = {0}", currentSession);
                    AaruConsole.DebugWriteLine("Nero plugin", "\tcurrentSessionMaxTrack = {0}", currentSessionMaxTrack);

                    AaruConsole.DebugWriteLine("Nero plugin", "\tcurrentSessionCurrentTrack = {0}",
                                               currentSessionCurrentTrack);

                    var track = new Track();

                    // Process indexes
                    if(_cuesheetV1?.Entries?.Count > 0)
                        foreach(CueEntryV1 entry in _cuesheetV1.Entries.Where(e => e.TrackNumber == neroTrack.Sequence).
                                                                OrderBy(e => e.IndexNumber))
                        {
                            track.Indexes[entry.IndexNumber] =
                                (entry.Minute * 60 * 75) + (entry.Second * 75) + entry.Frame - 150;

                            _trackFlags[entry.TrackNumber] = (byte)((entry.Mode & 0xF0) >> 4);
                        }
                    else if(_cuesheetV2?.Entries?.Count > 0)
                        foreach(CueEntryV2 entry in _cuesheetV2.Entries.Where(e => e.TrackNumber == neroTrack.Sequence).
                                                                OrderBy(e => e.IndexNumber))
                        {
                            track.Indexes[entry.IndexNumber] = entry.LbaStart;
                            _trackFlags[entry.TrackNumber]   = (byte)((entry.Mode & 0xF0) >> 4);
                        }

                    // Act if there are no indexes
                    if(track.Indexes.Count == 0)
                    {
                        if(!neroTrack.UseLbaForIndex)
                            continue; // This track start is unknown, continue and pray the goddess

                        // This always happens in TAO discs and Nero uses 150 sectors of pregap for them
                        // but we need to move the offsets if it's the first track of a session
                        if(currentSessionCurrentTrack == 1)
                        {
                            track.Indexes[1] =  (int)neroTrack.StartLba;
                            track.Indexes[0] =  track.Indexes[1] - 150;
                            neroTrack.Offset -= (ulong)(150 * neroTrack.SectorSize);
                            neroTrack.Length += (ulong)(150 * neroTrack.SectorSize);
                        }
                        else
                        {
                            track.Indexes[0] = (int)neroTrack.StartLba;
                            track.Indexes[1] = track.Indexes[0] + 150;
                        }
                    }

                    // Prevent duplicate index 0
                    if(track.Indexes.ContainsKey(0) &&
                       track.Indexes[0] == track.Indexes[1])
                        track.Indexes.Remove(0);

                    // There's a pregap
                    if(track.Indexes.ContainsKey(0))
                    {
                        track.TrackPregap = (ulong)(track.Indexes[1] - track.Indexes[0]);

                        // Negative pregap, skip it
                        if(track.Indexes[0] < 0)
                        {
                            neroTrack.Length       -= track.TrackPregap * neroTrack.SectorSize;
                            neroTrack.Offset       += track.TrackPregap * neroTrack.SectorSize;
                            track.TrackStartSector =  (ulong)track.Indexes[1];
                        }
                        else
                            track.TrackStartSector = (ulong)track.Indexes[0];
                    }
                    else
                        track.TrackStartSector = (ulong)track.Indexes[1];

                    // Handle hidden tracks
                    if(neroTrack.Sequence     == 1 &&
                       track.TrackStartSector > 0)
                    {
                        neroTrack.Length       += track.TrackStartSector * neroTrack.SectorSize;
                        neroTrack.Offset       -= track.TrackStartSector * neroTrack.SectorSize;
                        track.TrackStartSector =  0;
                    }

                    // Common track data
                    track.TrackDescription    = StringHandlers.CToString(neroTrack.Isrc);
                    track.TrackEndSector      = (neroTrack.Length / neroTrack.SectorSize) + track.TrackStartSector - 1;
                    track.TrackSequence       = neroTrack.Sequence;
                    track.TrackSession        = currentSession;
                    track.TrackType           = NeroTrackModeToTrackType((DaoMode)neroTrack.Mode);
                    track.TrackFile           = imageFilter.GetFilename();
                    track.TrackFilter         = imageFilter;
                    track.TrackFileOffset     = neroTrack.Offset;
                    track.TrackFileType       = "BINARY";
                    track.TrackSubchannelType = TrackSubchannelType.None;
                    neroTrack.Sectors         = neroTrack.Length / neroTrack.SectorSize;

                    // Flags not set for this track
                    if(!_trackFlags.ContainsKey(track.TrackSequence))
                    {
                        switch(track.TrackType)
                        {
                            case TrackType.Audio:
                                _trackFlags[track.TrackSequence] = 0;

                                break;
                            case TrackType.Data:
                            case TrackType.CdMode1:
                            case TrackType.CdMode2Formless:
                            case TrackType.CdMode2Form1:
                            case TrackType.CdMode2Form2:
                                _trackFlags[track.TrackSequence] = 4;

                                break;
                        }
                    }

                    // If ISRC is not empty
                    if(!string.IsNullOrWhiteSpace(track.TrackDescription))
                    {
                        _trackIsrCs[neroTrack.Sequence] = neroTrack.Isrc;

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdTrackIsrc))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackIsrc);
                    }

                    bool rawMode1 = false;
                    bool rawMode2 = false;

                    switch((DaoMode)neroTrack.Mode)
                    {
                        case DaoMode.AudioAlt:
                        case DaoMode.Audio:
                            track.TrackBytesPerSector    = 2352;
                            track.TrackRawBytesPerSector = 2352;

                            break;
                        case DaoMode.AudioSub:
                            track.TrackBytesPerSector    = 2352;
                            track.TrackRawBytesPerSector = 2352;
                            track.TrackSubchannelType    = TrackSubchannelType.RawInterleaved;

                            break;
                        case DaoMode.Data:
                        case DaoMode.DataM2F1:
                            track.TrackBytesPerSector    = 2048;
                            track.TrackRawBytesPerSector = 2048;

                            break;
                        case DaoMode.DataM2F2:
                            track.TrackBytesPerSector    = 2336;
                            track.TrackRawBytesPerSector = 2336;

                            break;
                        case DaoMode.DataM2Raw:
                            track.TrackBytesPerSector    = 2352;
                            track.TrackRawBytesPerSector = 2352;
                            rawMode2                     = true;

                            break;
                        case DaoMode.DataM2RawSub:
                            track.TrackBytesPerSector    = 2352;
                            track.TrackRawBytesPerSector = 2352;
                            track.TrackSubchannelType    = TrackSubchannelType.RawInterleaved;
                            rawMode2                     = true;

                            break;
                        case DaoMode.DataRaw:
                            track.TrackBytesPerSector    = 2048;
                            track.TrackRawBytesPerSector = 2352;
                            rawMode1                     = true;

                            break;
                        case DaoMode.DataRawSub:
                            track.TrackBytesPerSector    = 2048;
                            track.TrackRawBytesPerSector = 2352;
                            track.TrackSubchannelType    = TrackSubchannelType.RawInterleaved;
                            rawMode1                     = true;

                            break;
                    }

                    AaruConsole.DebugWriteLine("Nero plugin", "\t\t _track.TrackDescription = {0}",
                                               track.TrackDescription);

                    AaruConsole.DebugWriteLine("Nero plugin", "\t\t _track.TrackEndSector = {0}", track.TrackEndSector);
                    AaruConsole.DebugWriteLine("Nero plugin", "\t\t _track.TrackPregap = {0}", track.TrackPregap);
                    AaruConsole.DebugWriteLine("Nero plugin", "\t\t _track.TrackSequence = {0}", track.TrackSequence);
                    AaruConsole.DebugWriteLine("Nero plugin", "\t\t _track.TrackSession = {0}", track.TrackSession);

                    AaruConsole.DebugWriteLine("Nero plugin", "\t\t _track.TrackStartSector = {0}",
                                               track.TrackStartSector);

                    AaruConsole.DebugWriteLine("Nero plugin", "\t\t _track.TrackType = {0}", track.TrackType);

                    // Check readability of sector tags
                    if(rawMode1 || rawMode2)
                    {
                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader) && rawMode2)
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);
                    }

                    if(track.TrackSubchannelType == TrackSubchannelType.RawInterleaved)
                    {
                        track.TrackSubchannelFilter = imageFilter;
                        track.TrackSubchannelFile   = imageFilter.GetFilename();
                        track.TrackSubchannelOffset = neroTrack.Offset;

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);
                    }

                    Tracks.Add(track);

                    // Build session
                    if(currentSessionCurrentTrack == 1)
                        currentSessionStruct = new CommonTypes.Structs.Session
                        {
                            SessionSequence = currentSession,
                            StartSector     = track.TrackStartSector,
                            StartTrack      = track.TrackSequence
                        };

                    currentSessionCurrentTrack++;

                    if(currentSessionCurrentTrack > currentSessionMaxTrack)
                    {
                        currentSession++;
                        _neroSessions.TryGetValue(currentSession, out currentSessionMaxTrack);
                        currentSessionCurrentTrack     = 1;
                        currentSessionStruct.EndTrack  = track.TrackSequence;
                        currentSessionStruct.EndSector = track.TrackEndSector;
                        Sessions.Add(currentSessionStruct);
                    }
                    else if(trackCounter == _neroTracks.Count)
                    {
                        _neroSessions.TryGetValue(currentSession, out currentSessionMaxTrack);
                        currentSessionCurrentTrack     = 1;
                        currentSessionStruct.EndTrack  = track.TrackSequence;
                        currentSessionStruct.EndSector = track.TrackEndSector;
                        Sessions.Add(currentSessionStruct);
                    }

                    // Add to offset map
                    _offsetmap.Add(track.TrackSequence, track.TrackStartSector);

                    AaruConsole.DebugWriteLine("Nero plugin", "\t\t Offset[{0}]: {1}", track.TrackSequence,
                                               track.TrackStartSector);

                    // Create partition
                    var partition = new Partition
                    {
                        Description = $"Track {track.TrackSequence}",
                        Size        = neroTrack.EndOfTrack - neroTrack.Index1,
                        Name        = StringHandlers.CToString(neroTrack.Isrc),
                        Sequence    = partitionSequence,
                        Offset      = partitionStartByte,
                        Start       = (ulong)track.Indexes[1],
                        Type        = NeroTrackModeToTrackType((DaoMode)neroTrack.Mode).ToString()
                    };

                    partition.Length = partition.Size / neroTrack.SectorSize;
                    Partitions.Add(partition);
                    partitionSequence++;
                    partitionStartByte += partition.Size;

                    if(track.TrackEndSector + 1 > _imageInfo.Sectors)
                        _imageInfo.Sectors = track.TrackEndSector + 1;

                    trackCounter++;
                }

                if(Tracks.Count == 0)
                {
                    if(_neroTracks.Count != 1      ||
                       !_neroTracks.ContainsKey(1) ||
                       (_imageNewFormat ? footerV2.FirstChunkOffset : footerV1.FirstChunkOffset) %
                       _neroTracks[1].SectorSize != 0)
                    {
                        AaruConsole.ErrorWriteLine("Image corrupted beyond recovery, cannot open.");

                        return false;
                    }

                    var track = new Track();

                    // Common track data
                    track.TrackDescription = StringHandlers.CToString(_neroTracks[1].Isrc);

                    track.TrackEndSector = ((_imageNewFormat ? footerV2.FirstChunkOffset : footerV1.FirstChunkOffset) /
                                            _neroTracks[1].SectorSize) - 150;

                    track.TrackSequence       = _neroTracks[1].Sequence;
                    track.TrackSession        = currentSession;
                    track.TrackType           = NeroTrackModeToTrackType((DaoMode)_neroTracks[1].Mode);
                    track.TrackFile           = imageFilter.GetFilename();
                    track.TrackFilter         = imageFilter;
                    track.TrackFileType       = "BINARY";
                    track.TrackSubchannelType = TrackSubchannelType.None;
                    track.Indexes[1]          = 0;

                    bool rawMode1 = false;
                    bool rawMode2 = false;
                    int  subSize  = 0;

                    switch((DaoMode)_neroTracks[1].Mode)
                    {
                        case DaoMode.AudioAlt:
                        case DaoMode.Audio:
                            track.TrackBytesPerSector    = 2352;
                            track.TrackRawBytesPerSector = 2352;

                            break;
                        case DaoMode.AudioSub:
                            track.TrackBytesPerSector    = 2352;
                            track.TrackRawBytesPerSector = 2352;
                            track.TrackSubchannelType    = TrackSubchannelType.RawInterleaved;
                            subSize                      = 96;

                            break;
                        case DaoMode.Data:
                        case DaoMode.DataM2F1:
                            track.TrackBytesPerSector    = 2048;
                            track.TrackRawBytesPerSector = 2048;

                            break;
                        case DaoMode.DataM2F2:
                            track.TrackBytesPerSector    = 2336;
                            track.TrackRawBytesPerSector = 2336;

                            break;
                        case DaoMode.DataM2Raw:
                            track.TrackBytesPerSector    = 2352;
                            track.TrackRawBytesPerSector = 2352;
                            rawMode2                     = true;

                            break;
                        case DaoMode.DataM2RawSub:
                            track.TrackBytesPerSector    = 2352;
                            track.TrackRawBytesPerSector = 2352;
                            track.TrackSubchannelType    = TrackSubchannelType.RawInterleaved;
                            rawMode2                     = true;
                            subSize                      = 96;

                            break;
                        case DaoMode.DataRaw:
                            track.TrackBytesPerSector    = 2048;
                            track.TrackRawBytesPerSector = 2352;
                            rawMode1                     = true;

                            break;
                        case DaoMode.DataRawSub:
                            track.TrackBytesPerSector    = 2048;
                            track.TrackRawBytesPerSector = 2352;
                            track.TrackSubchannelType    = TrackSubchannelType.RawInterleaved;
                            rawMode1                     = true;
                            subSize                      = 96;

                            break;
                    }

                    // Check readability of sector tags
                    if(rawMode1 || rawMode2)
                    {
                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader) && rawMode2)
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);
                    }

                    if(track.TrackSubchannelType == TrackSubchannelType.RawInterleaved)
                    {
                        track.TrackSubchannelFilter = imageFilter;
                        track.TrackSubchannelFile   = imageFilter.GetFilename();
                        track.TrackSubchannelOffset = (ulong)(150 * (track.TrackRawBytesPerSector + subSize));

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);
                    }

                    track.TrackFileOffset  = (ulong)(150 * (track.TrackRawBytesPerSector + subSize));
                    _neroTracks[1].Offset  = track.TrackFileOffset;
                    _neroTracks[1].Sectors = track.TrackEndSector - track.TrackStartSector + 1;

                    // Add to offset map
                    _offsetmap.Add(track.TrackSequence, track.TrackStartSector);

                    // This is basically what MagicISO does with DVD images
                    if(track.TrackRawBytesPerSector == 2048)
                    {
                        _imageInfo.MediaType = CommonTypes.MediaType.DVDROM;
                        track.TrackType      = TrackType.Data;
                    }

                    _imageInfo.Sectors = track.TrackEndSector + 1;

                    Tracks.Add(track);

                    // Create partition
                    var partition = new Partition
                    {
                        Description = $"Track {track.TrackSequence}",
                        Length      = track.TrackEndSector - track.TrackStartSector + 1,
                        Name        = StringHandlers.CToString(_neroTracks[1].Isrc),
                        Sequence    = 1,
                        Offset      = 0,
                        Start       = (ulong)track.Indexes[1],
                        Type        = track.TrackType.ToString()
                    };

                    partition.Size = partition.Length * _neroTracks[1].SectorSize;
                    Partitions.Add(partition);

                    Sessions.Add(new CommonTypes.Structs.Session
                    {
                        SessionSequence = 1,
                        StartSector     = 0,
                        StartTrack      = 1,
                        EndTrack        = 1,
                        EndSector       = track.TrackEndSector
                    });

                    AaruConsole.ErrorWriteLine("Warning! This image is missing the last 150 sectors.");
                }

                // MagicISO meets these conditions when disc contains more than 15 tracks and a single session
                if(_imageNewFormat             &&
                   Tracks.Count > 0xF          &&
                   moreTracksThanSessionTracks &&
                   onlyOneSession              &&
                   Tracks.Any(t => t.TrackSession > 0))
                {
                    foreach(Track track in Tracks)
                        track.TrackSession = 1;

                    Sessions.Clear();

                    Track firstTrack = Tracks.First();
                    Track lastTrack  = Tracks.Last();

                    Sessions.Add(new CommonTypes.Structs.Session
                    {
                        EndSector       = lastTrack.TrackEndSector,
                        StartSector     = firstTrack.TrackStartSector,
                        SessionSequence = 1,
                        EndTrack        = lastTrack.TrackSequence,
                        StartTrack      = firstTrack.TrackSequence
                    });
                }

                if(_trackFlags.Count > 0 &&
                   !_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdTrackFlags))
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackFlags);

                _neroFilter = imageFilter;

                if(_imageInfo.MediaType == CommonTypes.MediaType.Unknown ||
                   _imageInfo.MediaType == CommonTypes.MediaType.CD)
                {
                    bool data       = false;
                    bool mode2      = false;
                    bool firstAudio = false;
                    bool firstData  = false;
                    bool audio      = false;

                    for(int i = 0; i < _neroTracks.Count; i++)
                    {
                        // First track is audio
                        firstAudio |= i == 0 && ((DaoMode)_neroTracks.ElementAt(i).Value.Mode == DaoMode.Audio    ||
                                                 (DaoMode)_neroTracks.ElementAt(i).Value.Mode == DaoMode.AudioAlt ||
                                                 (DaoMode)_neroTracks.ElementAt(i).Value.Mode == DaoMode.AudioSub);

                        // First track is data
                        firstData |= i == 0 && (DaoMode)_neroTracks.ElementAt(i).Value.Mode != DaoMode.Audio &&
                                     (DaoMode)_neroTracks.ElementAt(i).Value.Mode != DaoMode.AudioAlt &&
                                     (DaoMode)_neroTracks.ElementAt(i).Value.Mode != DaoMode.AudioSub;

                        // Any non first track is data
                        data |= i != 0 && (DaoMode)_neroTracks.ElementAt(i).Value.Mode != DaoMode.Audio &&
                                (DaoMode)_neroTracks.ElementAt(i).Value.Mode != DaoMode.AudioAlt &&
                                (DaoMode)_neroTracks.ElementAt(i).Value.Mode != DaoMode.AudioSub;

                        // Any non first track is audio
                        audio |= i != 0 && ((DaoMode)_neroTracks.ElementAt(i).Value.Mode == DaoMode.Audio    ||
                                            (DaoMode)_neroTracks.ElementAt(i).Value.Mode == DaoMode.AudioAlt ||
                                            (DaoMode)_neroTracks.ElementAt(i).Value.Mode == DaoMode.AudioSub);

                        switch((DaoMode)_neroTracks.ElementAt(i).Value.Mode)
                        {
                            case DaoMode.DataM2F1:
                            case DaoMode.DataM2F2:
                            case DaoMode.DataM2Raw:
                            case DaoMode.DataM2RawSub:
                                mode2 = true;

                                break;
                        }
                    }

                    if(!data &&
                       !firstData)
                        _imageInfo.MediaType = CommonTypes.MediaType.CDDA;
                    else if(firstAudio         &&
                            data               &&
                            Sessions.Count > 1 &&
                            mode2)
                        _imageInfo.MediaType = CommonTypes.MediaType.CDPLUS;
                    else if((firstData && audio) || mode2)
                        _imageInfo.MediaType = CommonTypes.MediaType.CDROMXA;
                    else if(!audio)
                        _imageInfo.MediaType = CommonTypes.MediaType.CDROM;
                    else
                        _imageInfo.MediaType = CommonTypes.MediaType.CD;
                }

                _imageInfo.XmlMediaType = XmlMediaType.OpticalDisc;
                AaruConsole.VerboseWriteLine("Nero image contains a disc of type {0}", _imageInfo.MediaType);

                _sectorBuilder = new SectorBuilder();

                _isCd = _imageInfo.MediaType == CommonTypes.MediaType.CD            ||
                        _imageInfo.MediaType == CommonTypes.MediaType.CDDA          ||
                        _imageInfo.MediaType == CommonTypes.MediaType.CDG           ||
                        _imageInfo.MediaType == CommonTypes.MediaType.CDEG          ||
                        _imageInfo.MediaType == CommonTypes.MediaType.CDI           ||
                        _imageInfo.MediaType == CommonTypes.MediaType.CDROM         ||
                        _imageInfo.MediaType == CommonTypes.MediaType.CDROMXA       ||
                        _imageInfo.MediaType == CommonTypes.MediaType.CDPLUS        ||
                        _imageInfo.MediaType == CommonTypes.MediaType.CDMO          ||
                        _imageInfo.MediaType == CommonTypes.MediaType.CDR           ||
                        _imageInfo.MediaType == CommonTypes.MediaType.CDRW          ||
                        _imageInfo.MediaType == CommonTypes.MediaType.CDMRW         ||
                        _imageInfo.MediaType == CommonTypes.MediaType.VCD           ||
                        _imageInfo.MediaType == CommonTypes.MediaType.SVCD          ||
                        _imageInfo.MediaType == CommonTypes.MediaType.PCD           ||
                        _imageInfo.MediaType == CommonTypes.MediaType.DTSCD         ||
                        _imageInfo.MediaType == CommonTypes.MediaType.CDMIDI        ||
                        _imageInfo.MediaType == CommonTypes.MediaType.CDV           ||
                        _imageInfo.MediaType == CommonTypes.MediaType.CDIREADY      ||
                        _imageInfo.MediaType == CommonTypes.MediaType.FMTOWNS       ||
                        _imageInfo.MediaType == CommonTypes.MediaType.PS1CD         ||
                        _imageInfo.MediaType == CommonTypes.MediaType.PS2CD         ||
                        _imageInfo.MediaType == CommonTypes.MediaType.MEGACD        ||
                        _imageInfo.MediaType == CommonTypes.MediaType.SATURNCD      ||
                        _imageInfo.MediaType == CommonTypes.MediaType.GDROM         ||
                        _imageInfo.MediaType == CommonTypes.MediaType.GDR           ||
                        _imageInfo.MediaType == CommonTypes.MediaType.MilCD         ||
                        _imageInfo.MediaType == CommonTypes.MediaType.SuperCDROM2   ||
                        _imageInfo.MediaType == CommonTypes.MediaType.JaguarCD      ||
                        _imageInfo.MediaType == CommonTypes.MediaType.ThreeDO       ||
                        _imageInfo.MediaType == CommonTypes.MediaType.PCFX          ||
                        _imageInfo.MediaType == CommonTypes.MediaType.NeoGeoCD      ||
                        _imageInfo.MediaType == CommonTypes.MediaType.CDTV          ||
                        _imageInfo.MediaType == CommonTypes.MediaType.CD32          ||
                        _imageInfo.MediaType == CommonTypes.MediaType.Playdia       ||
                        _imageInfo.MediaType == CommonTypes.MediaType.Pippin        ||
                        _imageInfo.MediaType == CommonTypes.MediaType.VideoNow      ||
                        _imageInfo.MediaType == CommonTypes.MediaType.VideoNowColor ||
                        _imageInfo.MediaType == CommonTypes.MediaType.VideoNowXp    ||
                        _imageInfo.MediaType == CommonTypes.MediaType.CVD;

                if(_isCd)
                    return true;

                foreach(Track track in Tracks)
                {
                    track.TrackPregap = 0;
                    track.Indexes?.Clear();
                }

                _imageInfo.ReadableMediaTags.Remove(MediaTagType.CD_MCN);
                _imageInfo.ReadableSectorTags.Remove(SectorTagType.CdTrackIsrc);
                _imageInfo.ReadableSectorTags.Remove(SectorTagType.CdTrackFlags);

                return true;
            }
            catch
            {
                AaruConsole.DebugWrite("Nero plugin", "Exception occurred opening file.");

                return false;
            }
        }

        /// <inheritdoc />
        public byte[] ReadDiskTag(MediaTagType tag)
        {
            switch(tag)
            {
                case MediaTagType.CD_MCN:  return _upc;
                case MediaTagType.CD_TEXT: throw new NotImplementedException("Not yet implemented");
                default:
                    throw new FeaturedNotSupportedByDiscImageException("Requested disk tag not supported by image");
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
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetmap where sectorAddress     >= kvp.Value
                                                     from track in Tracks where track.TrackSequence == kvp.Key
                                                     where sectorAddress        - kvp.Value <=
                                                           track.TrackEndSector - track.TrackStartSector select kvp)
                return ReadSectors(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), $"Sector address {sectorAddress} not found");
        }

        /// <inheritdoc />
        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetmap where sectorAddress     >= kvp.Value
                                                     from track in Tracks where track.TrackSequence == kvp.Key
                                                     where sectorAddress        - kvp.Value <=
                                                           track.TrackEndSector - track.TrackStartSector select kvp)
                return ReadSectorsTag(sectorAddress - kvp.Value, length, kvp.Key, tag);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), $"Sector address {sectorAddress} not found");
        }

        /// <inheritdoc />
        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            if(!_neroTracks.TryGetValue(track, out NeroTrack aaruTrack))
                throw new ArgumentOutOfRangeException(nameof(track), "Track not found");

            if(length > aaruTrack.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length}) than present in track ({aaruTrack.Sectors}), won't cross tracks");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;
            bool mode2 = false;

            switch((DaoMode)aaruTrack.Mode)
            {
                case DaoMode.Data:
                case DaoMode.DataM2F1:
                {
                    sectorOffset = 0;
                    sectorSize   = 2048;
                    sectorSkip   = 0;

                    break;
                }

                case DaoMode.DataM2F2:
                {
                    mode2        = true;
                    sectorOffset = 0;
                    sectorSize   = 2336;
                    sectorSkip   = 0;

                    break;
                }

                case DaoMode.Audio:
                case DaoMode.AudioAlt:
                {
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 0;

                    break;
                }

                case DaoMode.DataRaw:
                {
                    sectorOffset = 16;
                    sectorSize   = 2048;
                    sectorSkip   = 288;

                    break;
                }

                case DaoMode.DataM2Raw:
                {
                    mode2        = true;
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 0;

                    break;
                }

                case DaoMode.DataRawSub:
                {
                    sectorOffset = 16;
                    sectorSize   = 2048;
                    sectorSkip   = 288 + 96;

                    break;
                }

                case DaoMode.DataM2RawSub:
                {
                    mode2        = true;
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 96;

                    break;
                }

                case DaoMode.AudioSub:
                {
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 96;

                    break;
                }

                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sectorSize * length];

            _imageStream = _neroFilter.GetDataForkStream();
            var br = new BinaryReader(_imageStream);

            br.BaseStream.
               Seek((long)aaruTrack.Offset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                    SeekOrigin.Begin);

            if(mode2)
            {
                var mode2Ms = new MemoryStream((int)(sectorSize * length));

                buffer = br.ReadBytes((int)((sectorSize + sectorSkip) * length));

                for(int i = 0; i < length; i++)
                {
                    byte[] sector = new byte[sectorSize];
                    Array.Copy(buffer, (sectorSize + sectorSkip) * i, sector, 0, sectorSize);
                    sector = Sector.GetUserData(sector);
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
            if(tag == SectorTagType.CdTrackFlags ||
               tag == SectorTagType.CdTrackIsrc)
                track = (uint)sectorAddress;

            if(!_neroTracks.TryGetValue(track, out NeroTrack aaruTrack))
                throw new ArgumentOutOfRangeException(nameof(track), "Track not found");

            if(length > aaruTrack.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length}) than present in track ({aaruTrack.Sectors}), won't cross tracks");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

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
                    if(_trackFlags.TryGetValue(track, out byte flag))
                        return new[]
                        {
                            flag
                        };

                    throw new ArgumentException("Unsupported tag requested", nameof(tag));
                case SectorTagType.CdTrackIsrc: return aaruTrack.Isrc;
                case SectorTagType.CdTrackText:
                    throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
            }

            switch((DaoMode)aaruTrack.Mode)
            {
                case DaoMode.Data:
                case DaoMode.DataM2F1: throw new ArgumentException("No tags in image for requested track", nameof(tag));
                case DaoMode.DataM2F2:
                {
                    switch(tag)
                    {
                        case SectorTagType.CdSectorSync:
                        case SectorTagType.CdSectorHeader:
                        case SectorTagType.CdSectorSubchannel:
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

                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                }

                case DaoMode.Audio:
                case DaoMode.AudioAlt: throw new ArgumentException("There are no tags on audio tracks", nameof(tag));
                case DaoMode.DataRaw:
                {
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

                        case SectorTagType.CdSectorSubchannel:
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

                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                }

                case DaoMode.DataM2RawSub:
                    switch(tag)
                    {
                        case SectorTagType.CdSectorSync:
                        {
                            sectorOffset = 0;
                            sectorSize   = 12;
                            sectorSkip   = 2340 + 96;

                            break;
                        }

                        case SectorTagType.CdSectorHeader:
                        {
                            sectorOffset = 12;
                            sectorSize   = 4;
                            sectorSkip   = 2336 + 96;

                            break;
                        }

                        case SectorTagType.CdSectorSubHeader:
                        {
                            sectorOffset = 16;
                            sectorSize   = 8;
                            sectorSkip   = 2328 + 96;

                            break;
                        }

                        case SectorTagType.CdSectorEdc:
                        {
                            sectorOffset = 2348;
                            sectorSize   = 4;
                            sectorSkip   = 0 + 96;

                            break;
                        }

                        case SectorTagType.CdSectorSubchannel:
                        {
                            sectorOffset = 2352;
                            sectorSize   = 96;
                            sectorSkip   = 0;

                            break;
                        }

                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                case DaoMode.DataRawSub:
                {
                    switch(tag)
                    {
                        case SectorTagType.CdSectorSync:
                        {
                            sectorOffset = 0;
                            sectorSize   = 12;
                            sectorSkip   = 2340 + 96;

                            break;
                        }

                        case SectorTagType.CdSectorHeader:
                        {
                            sectorOffset = 12;
                            sectorSize   = 4;
                            sectorSkip   = 2336 + 96;

                            break;
                        }

                        case SectorTagType.CdSectorSubchannel:
                        {
                            sectorOffset = 2352;
                            sectorSize   = 96;
                            sectorSkip   = 0;

                            break;
                        }

                        case SectorTagType.CdSectorSubHeader:
                            throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                        case SectorTagType.CdSectorEcc:
                        {
                            sectorOffset = 2076;
                            sectorSize   = 276;
                            sectorSkip   = 0 + 96;

                            break;
                        }

                        case SectorTagType.CdSectorEccP:
                        {
                            sectorOffset = 2076;
                            sectorSize   = 172;
                            sectorSkip   = 104 + 96;

                            break;
                        }

                        case SectorTagType.CdSectorEccQ:
                        {
                            sectorOffset = 2248;
                            sectorSize   = 104;
                            sectorSkip   = 0 + 96;

                            break;
                        }

                        case SectorTagType.CdSectorEdc:
                        {
                            sectorOffset = 2064;
                            sectorSize   = 4;
                            sectorSkip   = 284 + 96;

                            break;
                        }

                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                }

                case DaoMode.AudioSub:
                {
                    if(tag != SectorTagType.CdSectorSubchannel)
                        throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));

                    sectorOffset = 2352;
                    sectorSize   = 96;
                    sectorSkip   = 0;

                    break;
                }

                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sectorSize * length];

            _imageStream = _neroFilter.GetDataForkStream();
            var br = new BinaryReader(_imageStream);

            br.BaseStream.
               Seek((long)aaruTrack.Offset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
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
        public byte[] ReadSectorLong(ulong sectorAddress) => ReadSectorsLong(sectorAddress, 1);

        /// <inheritdoc />
        public byte[] ReadSectorLong(ulong sectorAddress, uint track) => ReadSectorsLong(sectorAddress, 1, track);

        /// <inheritdoc />
        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetmap where sectorAddress     >= kvp.Value
                                                     from track in Tracks where track.TrackSequence == kvp.Key
                                                     where sectorAddress        - kvp.Value <=
                                                           track.TrackEndSector - track.TrackStartSector select kvp)
                return ReadSectorsLong(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), $"Sector address {sectorAddress} not found");
        }

        /// <inheritdoc />
        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            if(!_isCd)
                return ReadSectors(sectorAddress, length, track);

            if(!_neroTracks.TryGetValue(track, out NeroTrack aaruTrack))
                throw new ArgumentOutOfRangeException(nameof(track), "Track not found");

            if(length > aaruTrack.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length}) than present in track ({aaruTrack.Sectors}), won't cross tracks");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch((DaoMode)aaruTrack.Mode)
            {
                case DaoMode.Data:
                case DaoMode.DataM2F1:
                {
                    sectorOffset = 0;
                    sectorSize   = 2048;
                    sectorSkip   = 0;

                    break;
                }

                case DaoMode.DataM2F2:
                {
                    sectorOffset = 0;
                    sectorSize   = 2336;
                    sectorSkip   = 0;

                    break;
                }

                case DaoMode.DataRaw:
                case DaoMode.DataM2Raw:
                case DaoMode.Audio:
                case DaoMode.AudioAlt:
                {
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 0;

                    break;
                }

                case DaoMode.DataRawSub:
                case DaoMode.DataM2RawSub:
                case DaoMode.AudioSub:
                {
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 96;

                    break;
                }

                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sectorSize * length];

            _imageStream = _neroFilter.GetDataForkStream();
            var br = new BinaryReader(_imageStream);

            br.BaseStream.
               Seek((long)aaruTrack.Offset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
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

            switch((DaoMode)aaruTrack.Mode)
            {
                case DaoMode.Data:
                {
                    byte[] fullSector = new byte[2352];
                    byte[] fullBuffer = new byte[2352 * length];

                    for(uint i = 0; i < length; i++)
                    {
                        Array.Copy(buffer, i * 2048, fullSector, 16, 2048);
                        _sectorBuilder.ReconstructPrefix(ref fullSector, TrackType.CdMode1, (long)(sectorAddress + i));
                        _sectorBuilder.ReconstructEcc(ref fullSector, TrackType.CdMode1);
                        Array.Copy(fullSector, 0, fullBuffer, i * 2352, 2352);
                    }

                    buffer = fullBuffer;

                    break;
                }
                case DaoMode.DataM2F1:
                {
                    byte[] fullSector = new byte[2352];
                    byte[] fullBuffer = new byte[2352 * length];

                    for(uint i = 0; i < length; i++)
                    {
                        Array.Copy(buffer, i * 2048, fullSector, 24, 2048);

                        _sectorBuilder.ReconstructPrefix(ref fullSector, TrackType.CdMode2Form1,
                                                         (long)(sectorAddress + i));

                        _sectorBuilder.ReconstructEcc(ref fullSector, TrackType.CdMode2Form1);
                        Array.Copy(fullSector, 0, fullBuffer, i * 2352, 2352);
                    }

                    buffer = fullBuffer;

                    break;
                }
                case DaoMode.DataM2F2:
                {
                    byte[] fullSector = new byte[2352];
                    byte[] fullBuffer = new byte[2352 * length];

                    for(uint i = 0; i < length; i++)
                    {
                        _sectorBuilder.ReconstructPrefix(ref fullSector, TrackType.CdMode2Formless,
                                                         (long)(sectorAddress + i));

                        Array.Copy(buffer, i                    * 2336, fullSector, 16, 2336);
                        Array.Copy(fullSector, 0, fullBuffer, i * 2352, 2352);
                    }

                    buffer = fullBuffer;

                    break;
                }
            }

            return buffer;
        }

        /// <inheritdoc />
        public List<Track> GetSessionTracks(CommonTypes.Structs.Session session) =>
            GetSessionTracks(session.SessionSequence);

        /// <inheritdoc />
        public List<Track> GetSessionTracks(ushort session) =>
            Tracks.Where(track => track.TrackSession == session).ToList();
    }
}