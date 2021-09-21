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
//     Reads MAME Compressed Hunks of Data disk images.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.CommonTypes.Structs.Devices.ATA;
using Aaru.Console;
using Aaru.Decoders.CD;
using Aaru.Helpers;
using Session = Aaru.CommonTypes.Structs.Session;

namespace Aaru.DiscImages
{
    public sealed partial class Chd
    {
        /// <inheritdoc />
        [SuppressMessage("ReSharper", "UnusedVariable")]
        public ErrorNumber Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            byte[] magic = new byte[8];
            stream.Read(magic, 0, 8);

            if(!_chdTag.SequenceEqual(magic))
                return ErrorNumber.InvalidArgument;

            // Read length
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            uint length = BitConverter.ToUInt32(buffer.Reverse().ToArray(), 0);
            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            uint version = BitConverter.ToUInt32(buffer.Reverse().ToArray(), 0);

            buffer = new byte[length];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, (int)length);

            ulong nextMetaOff = 0;

            switch(version)
            {
                case 1:
                {
                    HeaderV1 hdrV1 = Marshal.ByteArrayToStructureBigEndian<HeaderV1>(buffer);

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV1.tag = \"{0}\"",
                                               Encoding.ASCII.GetString(hdrV1.tag));

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV1.length = {0} bytes", hdrV1.length);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV1.version = {0}", hdrV1.version);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV1.flags = {0}", (Flags)hdrV1.flags);

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV1.compression = {0}", (Compression)hdrV1.compression);

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV1.hunksize = {0}", hdrV1.hunksize);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV1.totalhunks = {0}", hdrV1.totalhunks);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV1.cylinders = {0}", hdrV1.cylinders);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV1.heads = {0}", hdrV1.heads);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV1.sectors = {0}", hdrV1.sectors);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV1.md5 = {0}", ArrayHelpers.ByteArrayToHex(hdrV1.md5));

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV1.parentmd5 = {0}",
                                               ArrayHelpers.ArrayIsNullOrEmpty(hdrV1.parentmd5) ? "null"
                                                   : ArrayHelpers.ByteArrayToHex(hdrV1.parentmd5));

                    AaruConsole.DebugWriteLine("CHD plugin", "Reading Hunk map.");
                    DateTime start = DateTime.UtcNow;

                    _hunkTable = new ulong[hdrV1.totalhunks];

                    uint hunkSectorCount = (uint)Math.Ceiling((double)hdrV1.totalhunks * 8 / 512);

                    byte[] hunkSectorBytes = new byte[512];

                    for(int i = 0; i < hunkSectorCount; i++)
                    {
                        stream.Read(hunkSectorBytes, 0, 512);

                        // This does the big-endian trick but reverses the order of elements also
                        Array.Reverse(hunkSectorBytes);
                        HunkSector hunkSector = Marshal.ByteArrayToStructureLittleEndian<HunkSector>(hunkSectorBytes);

                        // This restores the order of elements
                        Array.Reverse(hunkSector.hunkEntry);

                        if(_hunkTable.Length >= (i                                  * 512 / 8) + (512 / 8))
                            Array.Copy(hunkSector.hunkEntry, 0, _hunkTable, i * 512 / 8, 512 / 8);
                        else
                            Array.Copy(hunkSector.hunkEntry, 0, _hunkTable, i * 512 / 8,
                                       _hunkTable.Length - (i * 512 / 8));
                    }

                    DateTime end = DateTime.UtcNow;
                    AaruConsole.DebugWriteLine("CHD plugin", "Took {0} seconds", (end - start).TotalSeconds);

                    _imageInfo.MediaType    = MediaType.GENERIC_HDD;
                    _imageInfo.Sectors      = hdrV1.hunksize * hdrV1.totalhunks;
                    _imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
                    _imageInfo.SectorSize   = 512;
                    _imageInfo.Version      = "1";
                    _imageInfo.ImageSize    = _imageInfo.SectorSize * hdrV1.hunksize * hdrV1.totalhunks;

                    _totalHunks     = hdrV1.totalhunks;
                    _sectorsPerHunk = hdrV1.hunksize;
                    _hdrCompression = hdrV1.compression;
                    _mapVersion     = 1;
                    _isHdd          = true;

                    _imageInfo.Cylinders       = hdrV1.cylinders;
                    _imageInfo.Heads           = hdrV1.heads;
                    _imageInfo.SectorsPerTrack = hdrV1.sectors;

                    break;
                }

                case 2:
                {
                    HeaderV2 hdrV2 = Marshal.ByteArrayToStructureBigEndian<HeaderV2>(buffer);

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV2.tag = \"{0}\"",
                                               Encoding.ASCII.GetString(hdrV2.tag));

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV2.length = {0} bytes", hdrV2.length);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV2.version = {0}", hdrV2.version);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV2.flags = {0}", (Flags)hdrV2.flags);

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV2.compression = {0}", (Compression)hdrV2.compression);

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV2.hunksize = {0}", hdrV2.hunksize);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV2.totalhunks = {0}", hdrV2.totalhunks);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV2.cylinders = {0}", hdrV2.cylinders);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV2.heads = {0}", hdrV2.heads);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV2.sectors = {0}", hdrV2.sectors);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV2.md5 = {0}", ArrayHelpers.ByteArrayToHex(hdrV2.md5));

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV2.parentmd5 = {0}",
                                               ArrayHelpers.ArrayIsNullOrEmpty(hdrV2.parentmd5) ? "null"
                                                   : ArrayHelpers.ByteArrayToHex(hdrV2.parentmd5));

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV2.seclen = {0}", hdrV2.seclen);

                    AaruConsole.DebugWriteLine("CHD plugin", "Reading Hunk map.");
                    DateTime start = DateTime.UtcNow;

                    _hunkTable = new ulong[hdrV2.totalhunks];

                    // How many sectors uses the BAT
                    uint hunkSectorCount = (uint)Math.Ceiling((double)hdrV2.totalhunks * 8 / 512);

                    byte[] hunkSectorBytes = new byte[512];

                    for(int i = 0; i < hunkSectorCount; i++)
                    {
                        stream.Read(hunkSectorBytes, 0, 512);

                        // This does the big-endian trick but reverses the order of elements also
                        Array.Reverse(hunkSectorBytes);
                        HunkSector hunkSector = Marshal.ByteArrayToStructureLittleEndian<HunkSector>(hunkSectorBytes);

                        // This restores the order of elements
                        Array.Reverse(hunkSector.hunkEntry);

                        if(_hunkTable.Length >= (i                                  * 512 / 8) + (512 / 8))
                            Array.Copy(hunkSector.hunkEntry, 0, _hunkTable, i * 512 / 8, 512 / 8);
                        else
                            Array.Copy(hunkSector.hunkEntry, 0, _hunkTable, i * 512 / 8,
                                       _hunkTable.Length - (i * 512 / 8));
                    }

                    DateTime end = DateTime.UtcNow;
                    AaruConsole.DebugWriteLine("CHD plugin", "Took {0} seconds", (end - start).TotalSeconds);

                    _imageInfo.MediaType    = MediaType.GENERIC_HDD;
                    _imageInfo.Sectors      = hdrV2.hunksize * hdrV2.totalhunks;
                    _imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
                    _imageInfo.SectorSize   = hdrV2.seclen;
                    _imageInfo.Version      = "2";
                    _imageInfo.ImageSize    = _imageInfo.SectorSize * hdrV2.hunksize * hdrV2.totalhunks;

                    _totalHunks     = hdrV2.totalhunks;
                    _sectorsPerHunk = hdrV2.hunksize;
                    _hdrCompression = hdrV2.compression;
                    _mapVersion     = 1;
                    _isHdd          = true;

                    _imageInfo.Cylinders       = hdrV2.cylinders;
                    _imageInfo.Heads           = hdrV2.heads;
                    _imageInfo.SectorsPerTrack = hdrV2.sectors;

                    break;
                }

                case 3:
                {
                    HeaderV3 hdrV3 = Marshal.ByteArrayToStructureBigEndian<HeaderV3>(buffer);

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV3.tag = \"{0}\"",
                                               Encoding.ASCII.GetString(hdrV3.tag));

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV3.length = {0} bytes", hdrV3.length);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV3.version = {0}", hdrV3.version);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV3.flags = {0}", (Flags)hdrV3.flags);

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV3.compression = {0}", (Compression)hdrV3.compression);

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV3.totalhunks = {0}", hdrV3.totalhunks);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV3.logicalbytes = {0}", hdrV3.logicalbytes);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV3.metaoffset = {0}", hdrV3.metaoffset);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV3.md5 = {0}", ArrayHelpers.ByteArrayToHex(hdrV3.md5));

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV3.parentmd5 = {0}",
                                               ArrayHelpers.ArrayIsNullOrEmpty(hdrV3.parentmd5) ? "null"
                                                   : ArrayHelpers.ByteArrayToHex(hdrV3.parentmd5));

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV3.hunkbytes = {0}", hdrV3.hunkbytes);

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV3.sha1 = {0}",
                                               ArrayHelpers.ByteArrayToHex(hdrV3.sha1));

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV3.parentsha1 = {0}",
                                               ArrayHelpers.ArrayIsNullOrEmpty(hdrV3.parentsha1) ? "null"
                                                   : ArrayHelpers.ByteArrayToHex(hdrV3.parentsha1));

                    AaruConsole.DebugWriteLine("CHD plugin", "Reading Hunk map.");
                    DateTime start = DateTime.UtcNow;

                    _hunkMap = new byte[hdrV3.totalhunks * 16];
                    stream.Read(_hunkMap, 0, _hunkMap.Length);

                    DateTime end = DateTime.UtcNow;
                    AaruConsole.DebugWriteLine("CHD plugin", "Took {0} seconds", (end - start).TotalSeconds);

                    nextMetaOff = hdrV3.metaoffset;

                    _imageInfo.ImageSize = hdrV3.logicalbytes;
                    _imageInfo.Version   = "3";

                    _totalHunks     = hdrV3.totalhunks;
                    _bytesPerHunk   = hdrV3.hunkbytes;
                    _hdrCompression = hdrV3.compression;
                    _mapVersion     = 3;

                    break;
                }

                case 4:
                {
                    HeaderV4 hdrV4 = Marshal.ByteArrayToStructureBigEndian<HeaderV4>(buffer);

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV4.tag = \"{0}\"",
                                               Encoding.ASCII.GetString(hdrV4.tag));

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV4.length = {0} bytes", hdrV4.length);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV4.version = {0}", hdrV4.version);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV4.flags = {0}", (Flags)hdrV4.flags);

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV4.compression = {0}", (Compression)hdrV4.compression);

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV4.totalhunks = {0}", hdrV4.totalhunks);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV4.logicalbytes = {0}", hdrV4.logicalbytes);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV4.metaoffset = {0}", hdrV4.metaoffset);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV4.hunkbytes = {0}", hdrV4.hunkbytes);

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV4.sha1 = {0}",
                                               ArrayHelpers.ByteArrayToHex(hdrV4.sha1));

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV4.parentsha1 = {0}",
                                               ArrayHelpers.ArrayIsNullOrEmpty(hdrV4.parentsha1) ? "null"
                                                   : ArrayHelpers.ByteArrayToHex(hdrV4.parentsha1));

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV4.rawsha1 = {0}",
                                               ArrayHelpers.ByteArrayToHex(hdrV4.rawsha1));

                    AaruConsole.DebugWriteLine("CHD plugin", "Reading Hunk map.");
                    DateTime start = DateTime.UtcNow;

                    _hunkMap = new byte[hdrV4.totalhunks * 16];
                    stream.Read(_hunkMap, 0, _hunkMap.Length);

                    DateTime end = DateTime.UtcNow;
                    AaruConsole.DebugWriteLine("CHD plugin", "Took {0} seconds", (end - start).TotalSeconds);

                    nextMetaOff = hdrV4.metaoffset;

                    _imageInfo.ImageSize = hdrV4.logicalbytes;
                    _imageInfo.Version   = "4";

                    _totalHunks     = hdrV4.totalhunks;
                    _bytesPerHunk   = hdrV4.hunkbytes;
                    _hdrCompression = hdrV4.compression;
                    _mapVersion     = 3;

                    break;
                }

                case 5:
                {
                    // TODO: Check why reading is misaligned
                    AaruConsole.ErrorWriteLine("CHD version 5 is not yet supported.");

                    return ErrorNumber.NotSupported;

                    HeaderV5 hdrV5 = Marshal.ByteArrayToStructureBigEndian<HeaderV5>(buffer);

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV5.tag = \"{0}\"",
                                               Encoding.ASCII.GetString(hdrV5.tag));

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV5.length = {0} bytes", hdrV5.length);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV5.version = {0}", hdrV5.version);

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV5.compressor0 = \"{0}\"",
                                               Encoding.ASCII.GetString(BigEndianBitConverter.
                                                                            GetBytes(hdrV5.compressor0)));

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV5.compressor1 = \"{0}\"",
                                               Encoding.ASCII.GetString(BigEndianBitConverter.
                                                                            GetBytes(hdrV5.compressor1)));

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV5.compressor2 = \"{0}\"",
                                               Encoding.ASCII.GetString(BigEndianBitConverter.
                                                                            GetBytes(hdrV5.compressor2)));

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV5.compressor3 = \"{0}\"",
                                               Encoding.ASCII.GetString(BigEndianBitConverter.
                                                                            GetBytes(hdrV5.compressor3)));

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV5.logicalbytes = {0}", hdrV5.logicalbytes);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV5.mapoffset = {0}", hdrV5.mapoffset);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV5.metaoffset = {0}", hdrV5.metaoffset);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV5.hunkbytes = {0}", hdrV5.hunkbytes);
                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV5.unitbytes = {0}", hdrV5.unitbytes);

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV5.sha1 = {0}",
                                               ArrayHelpers.ByteArrayToHex(hdrV5.sha1));

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV5.parentsha1 = {0}",
                                               ArrayHelpers.ArrayIsNullOrEmpty(hdrV5.parentsha1) ? "null"
                                                   : ArrayHelpers.ByteArrayToHex(hdrV5.parentsha1));

                    AaruConsole.DebugWriteLine("CHD plugin", "hdrV5.rawsha1 = {0}",
                                               ArrayHelpers.ByteArrayToHex(hdrV5.rawsha1));

                    // TODO: Implement compressed CHD v5
                    if(hdrV5.compressor0 == 0)
                    {
                        AaruConsole.DebugWriteLine("CHD plugin", "Reading Hunk map.");
                        DateTime start = DateTime.UtcNow;

                        _hunkTableSmall = new uint[hdrV5.logicalbytes / hdrV5.hunkbytes];

                        uint hunkSectorCount = (uint)Math.Ceiling((double)_hunkTableSmall.Length * 4 / 512);

                        byte[] hunkSectorBytes = new byte[512];

                        stream.Seek((long)hdrV5.mapoffset, SeekOrigin.Begin);

                        for(int i = 0; i < hunkSectorCount; i++)
                        {
                            stream.Read(hunkSectorBytes, 0, 512);

                            // This does the big-endian trick but reverses the order of elements also
                            Array.Reverse(hunkSectorBytes);

                            HunkSectorSmall hunkSector =
                                Marshal.ByteArrayToStructureLittleEndian<HunkSectorSmall>(hunkSectorBytes);

                            // This restores the order of elements
                            Array.Reverse(hunkSector.hunkEntry);

                            if(_hunkTableSmall.Length >= (i                                  * 512 / 4) + (512 / 4))
                                Array.Copy(hunkSector.hunkEntry, 0, _hunkTableSmall, i * 512 / 4, 512 / 4);
                            else
                                Array.Copy(hunkSector.hunkEntry, 0, _hunkTableSmall, i * 512 / 4,
                                           _hunkTableSmall.Length - (i * 512 / 4));
                        }

                        DateTime end = DateTime.UtcNow;
                        AaruConsole.DebugWriteLine("CHD plugin", "Took {0} seconds", (end - start).TotalSeconds);
                    }
                    else
                    {
                        AaruConsole.ErrorWriteLine("Cannot read compressed CHD version 5");

                        return ErrorNumber.NotSupported;
                    }

                    nextMetaOff = hdrV5.metaoffset;

                    _imageInfo.ImageSize = hdrV5.logicalbytes;
                    _imageInfo.Version   = "5";

                    _totalHunks      = (uint)(hdrV5.logicalbytes / hdrV5.hunkbytes);
                    _bytesPerHunk    = hdrV5.hunkbytes;
                    _hdrCompression  = hdrV5.compressor0;
                    _hdrCompression1 = hdrV5.compressor1;
                    _hdrCompression2 = hdrV5.compressor2;
                    _hdrCompression3 = hdrV5.compressor3;
                    _mapVersion      = 5;

                    break;
                }

                default:
                    AaruConsole.ErrorWriteLine($"Unsupported CHD version {version}");

                    return ErrorNumber.NotSupported;
            }

            if(_mapVersion >= 3)
            {
                _isCdrom   = false;
                _isHdd     = false;
                _isGdrom   = false;
                _swapAudio = false;
                _tracks    = new Dictionary<uint, Track>();

                AaruConsole.DebugWriteLine("CHD plugin", "Reading metadata.");

                ulong currentSector = 0;
                uint  currentTrack  = 1;

                while(nextMetaOff > 0)
                {
                    byte[] hdrBytes = new byte[16];
                    stream.Seek((long)nextMetaOff, SeekOrigin.Begin);
                    stream.Read(hdrBytes, 0, hdrBytes.Length);
                    MetadataHeader header = Marshal.ByteArrayToStructureBigEndian<MetadataHeader>(hdrBytes);
                    byte[]         meta   = new byte[header.flagsAndLength & 0xFFFFFF];
                    stream.Read(meta, 0, meta.Length);

                    AaruConsole.DebugWriteLine("CHD plugin", "Found metadata \"{0}\"",
                                               Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(header.tag)));

                    switch(header.tag)
                    {
                        // "GDDD"
                        case HARD_DISK_METADATA:
                            if(_isCdrom || _isGdrom)
                            {
                                AaruConsole.
                                    ErrorWriteLine("Image cannot be a hard disk and a C/GD-ROM at the same time, aborting.");

                                return ErrorNumber.NotSupported;
                            }

                            string gddd      = StringHandlers.CToString(meta);
                            var    gdddRegEx = new Regex(REGEX_METADATA_HDD);
                            Match  gdddMatch = gdddRegEx.Match(gddd);

                            if(gdddMatch.Success)
                            {
                                _isHdd                     = true;
                                _imageInfo.SectorSize      = uint.Parse(gdddMatch.Groups["bps"].Value);
                                _imageInfo.Cylinders       = uint.Parse(gdddMatch.Groups["cylinders"].Value);
                                _imageInfo.Heads           = uint.Parse(gdddMatch.Groups["heads"].Value);
                                _imageInfo.SectorsPerTrack = uint.Parse(gdddMatch.Groups["sectors"].Value);
                            }

                            break;

                        // "CHCD"
                        case CDROM_OLD_METADATA:
                            if(_isHdd)
                            {
                                AaruConsole.
                                    ErrorWriteLine("Image cannot be a hard disk and a CD-ROM at the same time, aborting.");

                                return ErrorNumber.NotSupported;
                            }

                            if(_isGdrom)
                            {
                                AaruConsole.
                                    ErrorWriteLine("Image cannot be a GD-ROM and a CD-ROM at the same time, aborting.");

                                return ErrorNumber.NotSupported;
                            }

                            uint chdTracksNumber = BigEndianBitConverter.ToUInt32(meta, 0);

                            // Byteswapped
                            if(chdTracksNumber > 99)
                                chdTracksNumber = BigEndianBitConverter.ToUInt32(meta, 0);

                            currentSector = 0;

                            for(uint i = 0; i < chdTracksNumber; i++)
                            {
                                var chdTrack = new TrackOld
                                {
                                    type        = BigEndianBitConverter.ToUInt32(meta, (int)(4 + (i * 24) + 0)),
                                    subType     = BigEndianBitConverter.ToUInt32(meta, (int)(4 + (i * 24) + 4)),
                                    dataSize    = BigEndianBitConverter.ToUInt32(meta, (int)(4 + (i * 24) + 8)),
                                    subSize     = BigEndianBitConverter.ToUInt32(meta, (int)(4 + (i * 24) + 12)),
                                    frames      = BigEndianBitConverter.ToUInt32(meta, (int)(4 + (i * 24) + 16)),
                                    extraFrames = BigEndianBitConverter.ToUInt32(meta, (int)(4 + (i * 24) + 20))
                                };

                                var aaruTrack = new Track();

                                switch((TrackTypeOld)chdTrack.type)
                                {
                                    case TrackTypeOld.Audio:
                                        aaruTrack.BytesPerSector    = 2352;
                                        aaruTrack.RawBytesPerSector = 2352;
                                        aaruTrack.Type              = TrackType.Audio;

                                        break;
                                    case TrackTypeOld.Mode1:
                                        aaruTrack.BytesPerSector    = 2048;
                                        aaruTrack.RawBytesPerSector = 2048;
                                        aaruTrack.Type              = TrackType.CdMode1;

                                        break;
                                    case TrackTypeOld.Mode1Raw:
                                        aaruTrack.BytesPerSector    = 2048;
                                        aaruTrack.RawBytesPerSector = 2352;
                                        aaruTrack.Type              = TrackType.CdMode1;

                                        break;
                                    case TrackTypeOld.Mode2:
                                    case TrackTypeOld.Mode2FormMix:
                                        aaruTrack.BytesPerSector    = 2336;
                                        aaruTrack.RawBytesPerSector = 2336;
                                        aaruTrack.Type              = TrackType.CdMode2Formless;

                                        break;
                                    case TrackTypeOld.Mode2Form1:
                                        aaruTrack.BytesPerSector    = 2048;
                                        aaruTrack.RawBytesPerSector = 2048;
                                        aaruTrack.Type              = TrackType.CdMode2Form1;

                                        break;
                                    case TrackTypeOld.Mode2Form2:
                                        aaruTrack.BytesPerSector    = 2324;
                                        aaruTrack.RawBytesPerSector = 2324;
                                        aaruTrack.Type              = TrackType.CdMode2Form2;

                                        break;
                                    case TrackTypeOld.Mode2Raw:
                                        aaruTrack.BytesPerSector    = 2336;
                                        aaruTrack.RawBytesPerSector = 2352;
                                        aaruTrack.Type              = TrackType.CdMode2Formless;

                                        break;
                                    default:
                                    {
                                        AaruConsole.ErrorWriteLine($"Unsupported track type {chdTrack.type}");

                                        return ErrorNumber.NotSupported;
                                    }
                                }

                                switch((SubTypeOld)chdTrack.subType)
                                {
                                    case SubTypeOld.Cooked:
                                        aaruTrack.SubchannelFile   = imageFilter.Filename;
                                        aaruTrack.SubchannelType   = TrackSubchannelType.PackedInterleaved;
                                        aaruTrack.SubchannelFilter = imageFilter;

                                        break;
                                    case SubTypeOld.None:
                                        aaruTrack.SubchannelType = TrackSubchannelType.None;

                                        break;
                                    case SubTypeOld.Raw:
                                        aaruTrack.SubchannelFile   = imageFilter.Filename;
                                        aaruTrack.SubchannelType   = TrackSubchannelType.RawInterleaved;
                                        aaruTrack.SubchannelFilter = imageFilter;

                                        break;
                                    default:
                                    {
                                        AaruConsole.ErrorWriteLine($"Unsupported subchannel type {chdTrack.type}");

                                        return ErrorNumber.NotSupported;
                                    }
                                }

                                aaruTrack.Description = $"Track {i + 1}";
                                aaruTrack.EndSector   = currentSector + chdTrack.frames - 1;
                                aaruTrack.File        = imageFilter.Filename;
                                aaruTrack.FileType    = "BINARY";
                                aaruTrack.Filter      = imageFilter;
                                aaruTrack.StartSector = currentSector;
                                aaruTrack.Sequence    = i + 1;
                                aaruTrack.Session     = 1;

                                if(aaruTrack.Sequence == 1)
                                    aaruTrack.Indexes.Add(0, -150);

                                aaruTrack.Indexes.Add(1, (int)currentSector);
                                currentSector += chdTrack.frames + chdTrack.extraFrames;
                                _tracks.Add(aaruTrack.Sequence, aaruTrack);
                            }

                            _isCdrom = true;

                            break;

                        // "CHTR"
                        case CDROM_TRACK_METADATA:
                            if(_isHdd)
                            {
                                AaruConsole.
                                    ErrorWriteLine("Image cannot be a hard disk and a CD-ROM at the same time, aborting.");

                                return ErrorNumber.NotSupported;
                            }

                            if(_isGdrom)
                            {
                                AaruConsole.
                                    ErrorWriteLine("Image cannot be a GD-ROM and a CD-ROM at the same time, aborting.");

                                return ErrorNumber.NotSupported;
                            }

                            string chtr      = StringHandlers.CToString(meta);
                            var    chtrRegEx = new Regex(REGEX_METADATA_CDROM);
                            Match  chtrMatch = chtrRegEx.Match(chtr);

                            if(chtrMatch.Success)
                            {
                                _isCdrom = true;

                                uint   trackNo   = uint.Parse(chtrMatch.Groups["track"].Value);
                                uint   frames    = uint.Parse(chtrMatch.Groups["frames"].Value);
                                string subtype   = chtrMatch.Groups["sub_type"].Value;
                                string tracktype = chtrMatch.Groups["track_type"].Value;

                                if(trackNo != currentTrack)
                                {
                                    AaruConsole.ErrorWriteLine("Unsorted tracks, cannot proceed.");

                                    return ErrorNumber.NotSupported;
                                }

                                var aaruTrack = new Track();

                                switch(tracktype)
                                {
                                    case TRACK_TYPE_AUDIO:
                                        aaruTrack.BytesPerSector    = 2352;
                                        aaruTrack.RawBytesPerSector = 2352;
                                        aaruTrack.Type              = TrackType.Audio;

                                        break;
                                    case TRACK_TYPE_MODE1:
                                    case TRACK_TYPE_MODE1_2K:
                                        aaruTrack.BytesPerSector    = 2048;
                                        aaruTrack.RawBytesPerSector = 2048;
                                        aaruTrack.Type              = TrackType.CdMode1;

                                        break;
                                    case TRACK_TYPE_MODE1_RAW:
                                    case TRACK_TYPE_MODE1_RAW_2K:
                                        aaruTrack.BytesPerSector    = 2048;
                                        aaruTrack.RawBytesPerSector = 2352;
                                        aaruTrack.Type              = TrackType.CdMode1;

                                        break;
                                    case TRACK_TYPE_MODE2:
                                    case TRACK_TYPE_MODE2_2K:
                                    case TRACK_TYPE_MODE2_FM:
                                        aaruTrack.BytesPerSector    = 2336;
                                        aaruTrack.RawBytesPerSector = 2336;
                                        aaruTrack.Type              = TrackType.CdMode2Formless;

                                        break;
                                    case TRACK_TYPE_MODE2_F1:
                                    case TRACK_TYPE_MODE2_F1_2K:
                                        aaruTrack.BytesPerSector    = 2048;
                                        aaruTrack.RawBytesPerSector = 2048;
                                        aaruTrack.Type              = TrackType.CdMode2Form1;

                                        break;
                                    case TRACK_TYPE_MODE2_F2:
                                    case TRACK_TYPE_MODE2_F2_2K:
                                        aaruTrack.BytesPerSector    = 2324;
                                        aaruTrack.RawBytesPerSector = 2324;
                                        aaruTrack.Type              = TrackType.CdMode2Form2;

                                        break;
                                    case TRACK_TYPE_MODE2_RAW:
                                    case TRACK_TYPE_MODE2_RAW_2K:
                                        aaruTrack.BytesPerSector    = 2336;
                                        aaruTrack.RawBytesPerSector = 2352;
                                        aaruTrack.Type              = TrackType.CdMode2Formless;

                                        break;
                                    default:
                                    {
                                        AaruConsole.ErrorWriteLine($"Unsupported track type {tracktype}");

                                        return ErrorNumber.NotSupported;
                                    }
                                }

                                switch(subtype)
                                {
                                    case SUB_TYPE_COOKED:
                                        aaruTrack.SubchannelFile   = imageFilter.Filename;
                                        aaruTrack.SubchannelType   = TrackSubchannelType.PackedInterleaved;
                                        aaruTrack.SubchannelFilter = imageFilter;

                                        break;
                                    case SUB_TYPE_NONE:
                                        aaruTrack.SubchannelType = TrackSubchannelType.None;

                                        break;
                                    case SUB_TYPE_RAW:
                                        aaruTrack.SubchannelFile   = imageFilter.Filename;
                                        aaruTrack.SubchannelType   = TrackSubchannelType.RawInterleaved;
                                        aaruTrack.SubchannelFilter = imageFilter;

                                        break;
                                    default:
                                    {
                                        AaruConsole.ErrorWriteLine($"Unsupported subchannel type {subtype}");

                                        return ErrorNumber.NotSupported;
                                    }
                                }

                                aaruTrack.Description = $"Track {trackNo}";
                                aaruTrack.EndSector   = currentSector + frames - 1;
                                aaruTrack.File        = imageFilter.Filename;
                                aaruTrack.FileType    = "BINARY";
                                aaruTrack.Filter      = imageFilter;
                                aaruTrack.StartSector = currentSector;
                                aaruTrack.Sequence    = trackNo;
                                aaruTrack.Session     = 1;

                                if(aaruTrack.Sequence == 1)
                                    aaruTrack.Indexes.Add(0, -150);

                                aaruTrack.Indexes.Add(1, (int)currentSector);
                                currentSector += frames;
                                currentTrack++;
                                _tracks.Add(aaruTrack.Sequence, aaruTrack);
                            }

                            break;

                        // "CHT2"
                        case CDROM_TRACK_METADATA2:
                            if(_isHdd)
                            {
                                AaruConsole.
                                    ErrorWriteLine("Image cannot be a hard disk and a CD-ROM at the same time, aborting.");

                                return ErrorNumber.NotSupported;
                            }

                            if(_isGdrom)
                            {
                                AaruConsole.
                                    ErrorWriteLine("Image cannot be a GD-ROM and a CD-ROM at the same time, aborting.");

                                return ErrorNumber.NotSupported;
                            }

                            string cht2      = StringHandlers.CToString(meta);
                            var    cht2RegEx = new Regex(REGEX_METADATA_CDROM2);
                            Match  cht2Match = cht2RegEx.Match(cht2);

                            if(cht2Match.Success)
                            {
                                _isCdrom = true;

                                uint   trackNo   = uint.Parse(cht2Match.Groups["track"].Value);
                                uint   frames    = uint.Parse(cht2Match.Groups["frames"].Value);
                                string subtype   = cht2Match.Groups["sub_type"].Value;
                                string trackType = cht2Match.Groups["track_type"].Value;

                                uint pregap = uint.Parse(cht2Match.Groups["pregap"].Value);

                                // What is this, really? Same as track type?
                                string pregapType = cht2Match.Groups["pgtype"].Value;

                                // Read above, but for subchannel
                                string pregapSubType = cht2Match.Groups["pgsub"].Value;

                                // This is a recommendation (shall) of 150 sectors at the end of the last data track,
                                // or of any data track followed by an audio track, according to Yellow Book.
                                // It is indistinguishable from normal data.
                                // TODO: Does CHD store it, or like CDRWin, ignores it?
                                uint postgap = uint.Parse(cht2Match.Groups["postgap"].Value);

                                if(trackNo != currentTrack)
                                {
                                    AaruConsole.ErrorWriteLine("Unsorted tracks, cannot proceed.");

                                    return ErrorNumber.NotSupported;
                                }

                                var aaruTrack = new Track();

                                switch(trackType)
                                {
                                    case TRACK_TYPE_AUDIO:
                                        aaruTrack.BytesPerSector    = 2352;
                                        aaruTrack.RawBytesPerSector = 2352;
                                        aaruTrack.Type              = TrackType.Audio;

                                        break;
                                    case TRACK_TYPE_MODE1:
                                    case TRACK_TYPE_MODE1_2K:
                                        aaruTrack.BytesPerSector    = 2048;
                                        aaruTrack.RawBytesPerSector = 2048;
                                        aaruTrack.Type              = TrackType.CdMode1;

                                        break;
                                    case TRACK_TYPE_MODE1_RAW:
                                    case TRACK_TYPE_MODE1_RAW_2K:
                                        aaruTrack.BytesPerSector    = 2048;
                                        aaruTrack.RawBytesPerSector = 2352;
                                        aaruTrack.Type              = TrackType.CdMode1;

                                        break;
                                    case TRACK_TYPE_MODE2:
                                    case TRACK_TYPE_MODE2_2K:
                                    case TRACK_TYPE_MODE2_FM:
                                        aaruTrack.BytesPerSector    = 2336;
                                        aaruTrack.RawBytesPerSector = 2336;
                                        aaruTrack.Type              = TrackType.CdMode2Formless;

                                        break;
                                    case TRACK_TYPE_MODE2_F1:
                                    case TRACK_TYPE_MODE2_F1_2K:
                                        aaruTrack.BytesPerSector    = 2048;
                                        aaruTrack.RawBytesPerSector = 2048;
                                        aaruTrack.Type              = TrackType.CdMode2Form1;

                                        break;
                                    case TRACK_TYPE_MODE2_F2:
                                    case TRACK_TYPE_MODE2_F2_2K:
                                        aaruTrack.BytesPerSector    = 2324;
                                        aaruTrack.RawBytesPerSector = 2324;
                                        aaruTrack.Type              = TrackType.CdMode2Form2;

                                        break;
                                    case TRACK_TYPE_MODE2_RAW:
                                    case TRACK_TYPE_MODE2_RAW_2K:
                                        aaruTrack.BytesPerSector    = 2336;
                                        aaruTrack.RawBytesPerSector = 2352;
                                        aaruTrack.Type              = TrackType.CdMode2Formless;

                                        break;
                                    default:
                                    {
                                        AaruConsole.ErrorWriteLine($"Unsupported track type {trackType}");

                                        return ErrorNumber.NotSupported;
                                    }
                                }

                                switch(subtype)
                                {
                                    case SUB_TYPE_COOKED:
                                        aaruTrack.SubchannelFile   = imageFilter.Filename;
                                        aaruTrack.SubchannelType   = TrackSubchannelType.PackedInterleaved;
                                        aaruTrack.SubchannelFilter = imageFilter;

                                        break;
                                    case SUB_TYPE_NONE:
                                        aaruTrack.SubchannelType = TrackSubchannelType.None;

                                        break;
                                    case SUB_TYPE_RAW:
                                        aaruTrack.SubchannelFile   = imageFilter.Filename;
                                        aaruTrack.SubchannelType   = TrackSubchannelType.RawInterleaved;
                                        aaruTrack.SubchannelFilter = imageFilter;

                                        break;
                                    default:
                                    {
                                        AaruConsole.ErrorWriteLine($"Unsupported subchannel type {subtype}");

                                        return ErrorNumber.NotSupported;
                                    }
                                }

                                aaruTrack.Description = $"Track {trackNo}";
                                aaruTrack.EndSector   = currentSector + frames - 1;
                                aaruTrack.File        = imageFilter.Filename;
                                aaruTrack.FileType    = "BINARY";
                                aaruTrack.Filter      = imageFilter;
                                aaruTrack.StartSector = currentSector;
                                aaruTrack.Sequence    = trackNo;
                                aaruTrack.Session     = 1;

                                if(aaruTrack.Sequence == 1)
                                {
                                    if(pregap <= 150)
                                    {
                                        aaruTrack.Indexes.Add(0, -150);
                                        aaruTrack.Pregap = 150;
                                    }
                                    else
                                    {
                                        aaruTrack.Indexes.Add(0, -1 * (int)pregap);
                                        aaruTrack.Pregap = pregap;
                                    }

                                    aaruTrack.Indexes.Add(1, (int)currentSector);
                                }
                                else if(pregap > 0)
                                {
                                    aaruTrack.Indexes.Add(0, (int)currentSector);
                                    aaruTrack.Pregap = pregap;
                                    aaruTrack.Indexes.Add(1, (int)(currentSector + pregap));
                                }
                                else
                                    aaruTrack.Indexes.Add(1, (int)currentSector);

                                currentSector += frames;
                                currentTrack++;
                                _tracks.Add(aaruTrack.Sequence, aaruTrack);
                            }

                            break;

                        // "CHGT"
                        case GDROM_OLD_METADATA:
                            _swapAudio = true;
                            goto case GDROM_METADATA;

                        // "CHGD"
                        case GDROM_METADATA:
                            if(_isHdd)
                            {
                                AaruConsole.
                                    ErrorWriteLine("Image cannot be a hard disk and a GD-ROM at the same time, aborting.");

                                return ErrorNumber.NotSupported;
                            }

                            if(_isCdrom)
                            {
                                AaruConsole.
                                    ErrorWriteLine("Image cannot be a CD-ROM and a GD-ROM at the same time, aborting.");

                                return ErrorNumber.NotSupported;
                            }

                            string chgd      = StringHandlers.CToString(meta);
                            var    chgdRegEx = new Regex(REGEX_METADATA_GDROM);
                            Match  chgdMatch = chgdRegEx.Match(chgd);

                            if(chgdMatch.Success)
                            {
                                _isGdrom = true;

                                uint   trackNo   = uint.Parse(chgdMatch.Groups["track"].Value);
                                uint   frames    = uint.Parse(chgdMatch.Groups["frames"].Value);
                                string subtype   = chgdMatch.Groups["sub_type"].Value;
                                string trackType = chgdMatch.Groups["track_type"].Value;

                                // TODO: Check pregap, postgap and pad behaviour
                                uint   pregap        = uint.Parse(chgdMatch.Groups["pregap"].Value);
                                string pregapType    = chgdMatch.Groups["pgtype"].Value;
                                string pregapSubType = chgdMatch.Groups["pgsub"].Value;
                                uint   postgap       = uint.Parse(chgdMatch.Groups["postgap"].Value);
                                uint   pad           = uint.Parse(chgdMatch.Groups["pad"].Value);

                                if(trackNo != currentTrack)
                                {
                                    AaruConsole.ErrorWriteLine("Unsorted tracks, cannot proceed.");

                                    return ErrorNumber.NotSupported;
                                }

                                var aaruTrack = new Track();

                                switch(trackType)
                                {
                                    case TRACK_TYPE_AUDIO:
                                        aaruTrack.BytesPerSector    = 2352;
                                        aaruTrack.RawBytesPerSector = 2352;
                                        aaruTrack.Type              = TrackType.Audio;

                                        break;
                                    case TRACK_TYPE_MODE1:
                                    case TRACK_TYPE_MODE1_2K:
                                        aaruTrack.BytesPerSector    = 2048;
                                        aaruTrack.RawBytesPerSector = 2048;
                                        aaruTrack.Type              = TrackType.CdMode1;

                                        break;
                                    case TRACK_TYPE_MODE1_RAW:
                                    case TRACK_TYPE_MODE1_RAW_2K:
                                        aaruTrack.BytesPerSector    = 2048;
                                        aaruTrack.RawBytesPerSector = 2352;
                                        aaruTrack.Type              = TrackType.CdMode1;

                                        break;
                                    case TRACK_TYPE_MODE2:
                                    case TRACK_TYPE_MODE2_2K:
                                    case TRACK_TYPE_MODE2_FM:
                                        aaruTrack.BytesPerSector    = 2336;
                                        aaruTrack.RawBytesPerSector = 2336;
                                        aaruTrack.Type              = TrackType.CdMode2Formless;

                                        break;
                                    case TRACK_TYPE_MODE2_F1:
                                    case TRACK_TYPE_MODE2_F1_2K:
                                        aaruTrack.BytesPerSector    = 2048;
                                        aaruTrack.RawBytesPerSector = 2048;
                                        aaruTrack.Type              = TrackType.CdMode2Form1;

                                        break;
                                    case TRACK_TYPE_MODE2_F2:
                                    case TRACK_TYPE_MODE2_F2_2K:
                                        aaruTrack.BytesPerSector    = 2324;
                                        aaruTrack.RawBytesPerSector = 2324;
                                        aaruTrack.Type              = TrackType.CdMode2Form2;

                                        break;
                                    case TRACK_TYPE_MODE2_RAW:
                                    case TRACK_TYPE_MODE2_RAW_2K:
                                        aaruTrack.BytesPerSector    = 2336;
                                        aaruTrack.RawBytesPerSector = 2352;
                                        aaruTrack.Type              = TrackType.CdMode2Formless;

                                        break;
                                    default:
                                    {
                                        AaruConsole.ErrorWriteLine($"Unsupported track type {trackType}");

                                        return ErrorNumber.NotSupported;
                                    }
                                }

                                switch(subtype)
                                {
                                    case SUB_TYPE_COOKED:
                                        aaruTrack.SubchannelFile   = imageFilter.Filename;
                                        aaruTrack.SubchannelType   = TrackSubchannelType.PackedInterleaved;
                                        aaruTrack.SubchannelFilter = imageFilter;

                                        break;
                                    case SUB_TYPE_NONE:
                                        aaruTrack.SubchannelType = TrackSubchannelType.None;

                                        break;
                                    case SUB_TYPE_RAW:
                                        aaruTrack.SubchannelFile   = imageFilter.Filename;
                                        aaruTrack.SubchannelType   = TrackSubchannelType.RawInterleaved;
                                        aaruTrack.SubchannelFilter = imageFilter;

                                        break;
                                    default:
                                    {
                                        AaruConsole.ErrorWriteLine($"Unsupported subchannel type {subtype}");

                                        return ErrorNumber.NotSupported;
                                    }
                                }

                                aaruTrack.Description = $"Track {trackNo}";
                                aaruTrack.EndSector   = currentSector + frames - 1;
                                aaruTrack.File        = imageFilter.Filename;
                                aaruTrack.FileType    = "BINARY";
                                aaruTrack.Filter      = imageFilter;
                                aaruTrack.StartSector = currentSector;
                                aaruTrack.Sequence    = trackNo;
                                aaruTrack.Session     = (ushort)(trackNo > 2 ? 2 : 1);

                                if(aaruTrack.Sequence == 1)
                                {
                                    if(pregap <= 150)
                                    {
                                        aaruTrack.Indexes.Add(0, -150);
                                        aaruTrack.Pregap = 150;
                                    }
                                    else
                                    {
                                        aaruTrack.Indexes.Add(0, -1 * (int)pregap);
                                        aaruTrack.Pregap = pregap;
                                    }

                                    aaruTrack.Indexes.Add(1, (int)currentSector);
                                }
                                else if(pregap > 0)
                                {
                                    aaruTrack.Indexes.Add(0, (int)currentSector);
                                    aaruTrack.Pregap = pregap;
                                    aaruTrack.Indexes.Add(1, (int)(currentSector + pregap));
                                }
                                else
                                    aaruTrack.Indexes.Add(1, (int)currentSector);

                                currentSector += frames;
                                currentTrack++;
                                _tracks.Add(aaruTrack.Sequence, aaruTrack);
                            }

                            break;

                        // "IDNT"
                        case HARD_DISK_IDENT_METADATA:
                            Identify.IdentifyDevice? idnt = CommonTypes.Structs.Devices.ATA.Identify.Decode(meta);

                            if(idnt.HasValue)
                            {
                                _imageInfo.MediaManufacturer     = idnt.Value.MediaManufacturer;
                                _imageInfo.MediaSerialNumber     = idnt.Value.MediaSerial;
                                _imageInfo.DriveModel            = idnt.Value.Model;
                                _imageInfo.DriveSerialNumber     = idnt.Value.SerialNumber;
                                _imageInfo.DriveFirmwareRevision = idnt.Value.FirmwareRevision;

                                if(idnt.Value.CurrentCylinders       > 0 &&
                                   idnt.Value.CurrentHeads           > 0 &&
                                   idnt.Value.CurrentSectorsPerTrack > 0)
                                {
                                    _imageInfo.Cylinders       = idnt.Value.CurrentCylinders;
                                    _imageInfo.Heads           = idnt.Value.CurrentHeads;
                                    _imageInfo.SectorsPerTrack = idnt.Value.CurrentSectorsPerTrack;
                                }
                                else
                                {
                                    _imageInfo.Cylinders       = idnt.Value.Cylinders;
                                    _imageInfo.Heads           = idnt.Value.Heads;
                                    _imageInfo.SectorsPerTrack = idnt.Value.SectorsPerTrack;
                                }
                            }

                            _identify = meta;

                            if(!_imageInfo.ReadableMediaTags.Contains(MediaTagType.ATA_IDENTIFY))
                                _imageInfo.ReadableMediaTags.Add(MediaTagType.ATA_IDENTIFY);

                            break;
                        case PCMCIA_CIS_METADATA:
                            _cis = meta;

                            if(!_imageInfo.ReadableMediaTags.Contains(MediaTagType.PCMCIA_CIS))
                                _imageInfo.ReadableMediaTags.Add(MediaTagType.PCMCIA_CIS);

                            break;
                    }

                    nextMetaOff = header.next;
                }

                if(_isHdd)
                {
                    _sectorsPerHunk         = _bytesPerHunk        / _imageInfo.SectorSize;
                    _imageInfo.Sectors      = _imageInfo.ImageSize / _imageInfo.SectorSize;
                    _imageInfo.MediaType    = MediaType.GENERIC_HDD;
                    _imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
                }
                else if(_isCdrom)
                {
                    // Hardcoded on MAME for CD-ROM
                    _sectorsPerHunk         = 8;
                    _imageInfo.MediaType    = MediaType.CDROM;
                    _imageInfo.XmlMediaType = XmlMediaType.OpticalDisc;

                    foreach(Track aaruTrack in _tracks.Values)
                        _imageInfo.Sectors += aaruTrack.EndSector - aaruTrack.StartSector + 1;
                }
                else if(_isGdrom)
                {
                    // Hardcoded on MAME for GD-ROM
                    _sectorsPerHunk         = 8;
                    _imageInfo.MediaType    = MediaType.GDROM;
                    _imageInfo.XmlMediaType = XmlMediaType.OpticalDisc;

                    foreach(Track aaruTrack in _tracks.Values)
                        _imageInfo.Sectors += aaruTrack.EndSector - aaruTrack.StartSector + 1;
                }
                else
                {
                    AaruConsole.ErrorWriteLine("Image does not represent a known media, aborting");

                    return ErrorNumber.NotSupported;
                }
            }

            if(_isCdrom || _isGdrom)
            {
                _offsetmap  = new Dictionary<ulong, uint>();
                _partitions = new List<Partition>();
                ulong partPos = 0;

                foreach(Track aaruTrack in _tracks.Values)
                {
                    var partition = new Partition
                    {
                        Description = aaruTrack.Description,
                        Size = (aaruTrack.EndSector - (ulong)aaruTrack.Indexes[1] + 1) *
                               (ulong)aaruTrack.RawBytesPerSector,
                        Length   = aaruTrack.EndSector - (ulong)aaruTrack.Indexes[1] + 1,
                        Sequence = aaruTrack.Sequence,
                        Offset   = partPos,
                        Start    = (ulong)aaruTrack.Indexes[1],
                        Type     = aaruTrack.Type.ToString()
                    };

                    partPos += partition.Length;
                    _offsetmap.Add(aaruTrack.StartSector, aaruTrack.Sequence);

                    if(aaruTrack.SubchannelType != TrackSubchannelType.None)
                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);

                    switch(aaruTrack.Type)
                    {
                        case TrackType.CdMode1:
                        case TrackType.CdMode2Form1:
                            if(aaruTrack.RawBytesPerSector == 2352)
                            {
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
                            }

                            break;
                        case TrackType.CdMode2Form2:
                            if(aaruTrack.RawBytesPerSector == 2352)
                            {
                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);
                            }

                            break;
                        case TrackType.CdMode2Formless:
                            if(aaruTrack.RawBytesPerSector == 2352)
                            {
                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);
                            }

                            break;
                    }

                    if(aaruTrack.BytesPerSector > _imageInfo.SectorSize)
                        _imageInfo.SectorSize = (uint)aaruTrack.BytesPerSector;

                    _partitions.Add(partition);
                }

                _imageInfo.HasPartitions = true;
                _imageInfo.HasSessions   = true;
            }

            _maxBlockCache  = (int)(MAX_CACHE_SIZE / (_imageInfo.SectorSize * _sectorsPerHunk));
            _maxSectorCache = (int)(MAX_CACHE_SIZE / _imageInfo.SectorSize);

            _imageStream = stream;

            _sectorCache = new Dictionary<ulong, byte[]>();
            _hunkCache   = new Dictionary<ulong, byte[]>();

            // TODO: Detect CompactFlash
            // TODO: Get manufacturer and drive name from CIS if applicable
            if(_cis != null)
                _imageInfo.MediaType = MediaType.PCCardTypeI;

            _sectorBuilder = new SectorBuilder();

            return ErrorNumber.NoError;
        }

        /// <inheritdoc />
        public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer)
        {
            buffer = null;

            if(sectorAddress > _imageInfo.Sectors - 1)
                return ErrorNumber.OutOfRange;

            var  track = new Track();
            uint sectorSize;

            if(!_sectorCache.TryGetValue(sectorAddress, out byte[] sector))
            {
                if(_isHdd)
                    sectorSize = _imageInfo.SectorSize;
                else
                {
                    track      = GetTrack(sectorAddress);
                    sectorSize = (uint)track.RawBytesPerSector;
                }

                ulong hunkNo = sectorAddress              / _sectorsPerHunk;
                ulong secOff = sectorAddress * sectorSize % (_sectorsPerHunk * sectorSize);

                byte[] hunk = GetHunk(hunkNo);

                sector = new byte[_imageInfo.SectorSize];
                Array.Copy(hunk, (int)secOff, sector, 0, sector.Length);

                if(_sectorCache.Count >= _maxSectorCache)
                    _sectorCache.Clear();

                _sectorCache.Add(sectorAddress, sector);
            }

            if(_isHdd)
            {
                buffer = sector;

                return ErrorNumber.NoError;
            }

            uint sectorOffset;
            bool mode2 = false;

            switch(track.Type)
            {
                case TrackType.CdMode1:
                {
                    if(track.RawBytesPerSector == 2352)
                    {
                        sectorOffset = 16;
                        sectorSize   = 2048;
                    }
                    else
                    {
                        sectorOffset = 0;
                        sectorSize   = 2048;
                    }

                    break;
                }
                case TrackType.CdMode2Form1:
                {
                    if(track.RawBytesPerSector == 2352)
                    {
                        sectorOffset = 0;
                        sectorSize   = 2352;
                        mode2        = true;
                    }
                    else
                    {
                        sectorOffset = 0;
                        sectorSize   = 2048;
                    }

                    break;
                }

                case TrackType.CdMode2Form2:
                {
                    if(track.RawBytesPerSector == 2352)
                    {
                        sectorOffset = 0;
                        sectorSize   = 2352;
                        mode2        = true;
                    }
                    else
                    {
                        sectorOffset = 0;
                        sectorSize   = 2324;
                    }

                    break;
                }

                case TrackType.CdMode2Formless:
                {
                    sectorOffset = 0;
                    sectorSize   = (uint)track.RawBytesPerSector;
                    mode2        = true;

                    break;
                }

                case TrackType.Audio:
                {
                    sectorOffset = 0;
                    sectorSize   = 2352;

                    break;
                }

                default: return ErrorNumber.NotSupported;
            }

            buffer = new byte[sectorSize];

            if(mode2)
                buffer = Sector.GetUserDataFromMode2(sector);
            else if(track.Type == TrackType.Audio && _swapAudio)
                for(int i = 0; i < 2352; i += 2)
                {
                    buffer[i + 1] = sector[i];
                    buffer[i]     = sector[i + 1];
                }
            else
                Array.Copy(sector, sectorOffset, buffer, 0, sectorSize);

            return ErrorNumber.NoError;
        }

        /// <inheritdoc />
        public ErrorNumber ReadSectorTag(ulong sectorAddress, SectorTagType tag, out byte[] buffer)
        {
            buffer = null;

            if(_isHdd)
                return ErrorNumber.NotSupported;

            if(sectorAddress > _imageInfo.Sectors - 1)
                return ErrorNumber.OutOfRange;

            var track = new Track();

            uint sectorSize;

            if(!_sectorCache.TryGetValue(sectorAddress, out byte[] sector))
            {
                track      = GetTrack(sectorAddress);
                sectorSize = (uint)track.RawBytesPerSector;

                ulong hunkNo = sectorAddress              / _sectorsPerHunk;
                ulong secOff = sectorAddress * sectorSize % (_sectorsPerHunk * sectorSize);

                byte[] hunk = GetHunk(hunkNo);

                sector = new byte[_imageInfo.SectorSize];
                Array.Copy(hunk, (int)secOff, sector, 0, sector.Length);

                if(_sectorCache.Count >= _maxSectorCache)
                    _sectorCache.Clear();

                _sectorCache.Add(sectorAddress, sector);
            }

            if(_isHdd)
            {
                buffer = sector;

                return ErrorNumber.NoError;
            }

            uint sectorOffset;

            if(tag == SectorTagType.CdSectorSubchannel)
                switch(track.SubchannelType)
                {
                    case TrackSubchannelType.None: return ErrorNumber.NoData;
                    case TrackSubchannelType.RawInterleaved:
                        sectorOffset = (uint)track.RawBytesPerSector;
                        sectorSize   = 96;

                        break;
                    default: return ErrorNumber.NotSupported;
                }
            else
                switch(track.Type)
                {
                    case TrackType.CdMode1:
                    case TrackType.CdMode2Form1:
                    {
                        if(track.RawBytesPerSector == 2352)
                            switch(tag)
                            {
                                case SectorTagType.CdSectorSync:
                                {
                                    sectorOffset = 0;
                                    sectorSize   = 12;

                                    break;
                                }

                                case SectorTagType.CdSectorHeader:
                                {
                                    sectorOffset = 12;
                                    sectorSize   = 4;

                                    break;
                                }

                                case SectorTagType.CdSectorSubHeader: return ErrorNumber.NotSupported;
                                case SectorTagType.CdSectorEcc:
                                {
                                    sectorOffset = 2076;
                                    sectorSize   = 276;

                                    break;
                                }

                                case SectorTagType.CdSectorEccP:
                                {
                                    sectorOffset = 2076;
                                    sectorSize   = 172;

                                    break;
                                }

                                case SectorTagType.CdSectorEccQ:
                                {
                                    sectorOffset = 2248;
                                    sectorSize   = 104;

                                    break;
                                }

                                case SectorTagType.CdSectorEdc:
                                {
                                    sectorOffset = 2064;
                                    sectorSize   = 4;

                                    break;
                                }

                                default: return ErrorNumber.NotSupported;
                            }
                        else
                            return ErrorNumber.NoData;

                        break;
                    }

                    case TrackType.CdMode2Form2:
                    {
                        if(track.RawBytesPerSector == 2352)
                            switch(tag)
                            {
                                case SectorTagType.CdSectorSync:
                                {
                                    sectorOffset = 0;
                                    sectorSize   = 12;

                                    break;
                                }

                                case SectorTagType.CdSectorHeader:
                                {
                                    sectorOffset = 12;
                                    sectorSize   = 4;

                                    break;
                                }

                                case SectorTagType.CdSectorSubHeader:
                                {
                                    sectorOffset = 16;
                                    sectorSize   = 8;

                                    break;
                                }

                                case SectorTagType.CdSectorEdc:
                                {
                                    sectorOffset = 2348;
                                    sectorSize   = 4;

                                    break;
                                }

                                default: return ErrorNumber.NotSupported;
                            }
                        else
                            switch(tag)
                            {
                                case SectorTagType.CdSectorSync:
                                case SectorTagType.CdSectorHeader:
                                case SectorTagType.CdSectorSubchannel:
                                case SectorTagType.CdSectorEcc:
                                case SectorTagType.CdSectorEccP:
                                case SectorTagType.CdSectorEccQ: return ErrorNumber.NotSupported;
                                case SectorTagType.CdSectorSubHeader:
                                {
                                    sectorOffset = 0;
                                    sectorSize   = 8;

                                    break;
                                }

                                case SectorTagType.CdSectorEdc:
                                {
                                    sectorOffset = 2332;
                                    sectorSize   = 4;

                                    break;
                                }

                                default: return ErrorNumber.NotSupported;
                            }

                        break;
                    }

                    case TrackType.CdMode2Formless:
                    {
                        if(track.RawBytesPerSector == 2352)
                            switch(tag)
                            {
                                case SectorTagType.CdSectorSync:
                                case SectorTagType.CdSectorHeader:
                                case SectorTagType.CdSectorEcc:
                                case SectorTagType.CdSectorEccP:
                                case SectorTagType.CdSectorEccQ: return ErrorNumber.NotSupported;
                                case SectorTagType.CdSectorSubHeader:
                                {
                                    sectorOffset = 0;
                                    sectorSize   = 8;

                                    break;
                                }

                                case SectorTagType.CdSectorEdc:
                                {
                                    sectorOffset = 2332;
                                    sectorSize   = 4;

                                    break;
                                }

                                default: return ErrorNumber.NotSupported;
                            }
                        else
                            return ErrorNumber.NoData;

                        break;
                    }

                    case TrackType.Audio: return ErrorNumber.NoData;
                    default:              return ErrorNumber.NotImplemented;
                }

            buffer = new byte[sectorSize];

            if(track.Type == TrackType.Audio && _swapAudio)
                for(int i = 0; i < 2352; i += 2)
                {
                    buffer[i + 1] = sector[i];
                    buffer[i]     = sector[i + 1];
                }
            else
                Array.Copy(sector, sectorOffset, buffer, 0, sectorSize);

            if(track.Type == TrackType.Audio && _swapAudio)
                for(int i = 0; i < 2352; i += 2)
                {
                    buffer[i + 1] = sector[i];
                    buffer[i]     = sector[i + 1];
                }
            else
                Array.Copy(sector, sectorOffset, buffer, 0, sectorSize);

            return ErrorNumber.NoError;
        }

        /// <inheritdoc />
        public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
        {
            buffer = null;

            if(sectorAddress > _imageInfo.Sectors - 1)
                return ErrorNumber.OutOfRange;

            if(sectorAddress + length > _imageInfo.Sectors)
                return ErrorNumber.OutOfRange;

            var ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                ErrorNumber errno = ReadSector(sectorAddress + i, out byte[] sector);

                if(errno != ErrorNumber.NoError)
                    return errno;

                ms.Write(sector, 0, sector.Length);
            }

            buffer = ms.ToArray();

            return ErrorNumber.NoError;
        }

        /// <inheritdoc />
        public ErrorNumber ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag, out byte[] buffer)
        {
            buffer = null;

            if(sectorAddress > _imageInfo.Sectors - 1)
                return ErrorNumber.OutOfRange;

            if(sectorAddress + length > _imageInfo.Sectors)
                return ErrorNumber.OutOfRange;

            var ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                ErrorNumber errno = ReadSectorTag(sectorAddress + i, tag, out byte[] sector);

                if(errno != ErrorNumber.NoError)
                    return errno;

                ms.Write(sector, 0, sector.Length);
            }

            buffer = ms.ToArray();

            return ErrorNumber.NoError;
        }

        /// <inheritdoc />
        public ErrorNumber ReadSectorLong(ulong sectorAddress, out byte[] buffer)
        {
            buffer = null;

            if(_isHdd)
                return ReadSector(sectorAddress, out buffer);

            if(sectorAddress > _imageInfo.Sectors - 1)
                return ErrorNumber.OutOfRange;

            var track = new Track();

            if(!_sectorCache.TryGetValue(sectorAddress, out byte[] sector))
            {
                track = GetTrack(sectorAddress);
                uint sectorSize = (uint)track.RawBytesPerSector;

                ulong hunkNo = sectorAddress              / _sectorsPerHunk;
                ulong secOff = sectorAddress * sectorSize % (_sectorsPerHunk * sectorSize);

                byte[] hunk = GetHunk(hunkNo);

                sector = new byte[_imageInfo.SectorSize];
                Array.Copy(hunk, (int)secOff, sector, 0, sector.Length);

                if(_sectorCache.Count >= _maxSectorCache)
                    _sectorCache.Clear();

                _sectorCache.Add(sectorAddress, sector);
            }

            buffer = new byte[track.RawBytesPerSector];

            if(track.Type == TrackType.Audio && _swapAudio)
                for(int i = 0; i < 2352; i += 2)
                {
                    buffer[i + 1] = sector[i];
                    buffer[i]     = sector[i + 1];
                }
            else
                Array.Copy(sector, 0, buffer, 0, track.RawBytesPerSector);

            switch(track.Type)
            {
                case TrackType.CdMode1 when track.RawBytesPerSector == 2048:
                {
                    byte[] fullSector = new byte[2352];

                    Array.Copy(buffer, 0, fullSector, 16, 2048);
                    _sectorBuilder.ReconstructPrefix(ref fullSector, TrackType.CdMode1, (long)sectorAddress);
                    _sectorBuilder.ReconstructEcc(ref fullSector, TrackType.CdMode1);

                    buffer = fullSector;

                    break;
                }
                case TrackType.CdMode2Form1 when track.RawBytesPerSector == 2048:
                {
                    byte[] fullSector = new byte[2352];

                    Array.Copy(buffer, 0, fullSector, 24, 2048);
                    _sectorBuilder.ReconstructPrefix(ref fullSector, TrackType.CdMode2Form1, (long)sectorAddress);
                    _sectorBuilder.ReconstructEcc(ref fullSector, TrackType.CdMode2Form1);

                    buffer = fullSector;

                    break;
                }
                case TrackType.CdMode2Form1 when track.RawBytesPerSector == 2324:
                {
                    byte[] fullSector = new byte[2352];

                    Array.Copy(buffer, 0, fullSector, 24, 2324);
                    _sectorBuilder.ReconstructPrefix(ref fullSector, TrackType.CdMode2Form2, (long)sectorAddress);
                    _sectorBuilder.ReconstructEcc(ref fullSector, TrackType.CdMode2Form2);

                    buffer = fullSector;

                    break;
                }
                case TrackType.CdMode2Formless when track.RawBytesPerSector == 2336:
                {
                    byte[] fullSector = new byte[2352];

                    _sectorBuilder.ReconstructPrefix(ref fullSector, TrackType.CdMode2Formless, (long)sectorAddress);
                    Array.Copy(buffer, 0, fullSector, 16, 2336);

                    buffer = fullSector;

                    break;
                }
            }

            return ErrorNumber.NoError;
        }

        /// <inheritdoc />
        public ErrorNumber ReadSectorsLong(ulong sectorAddress, uint length, out byte[] buffer)
        {
            buffer = null;

            if(sectorAddress > _imageInfo.Sectors - 1)
                return ErrorNumber.OutOfRange;

            if(sectorAddress + length > _imageInfo.Sectors)
                return ErrorNumber.OutOfRange;

            var ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                ErrorNumber errno = ReadSectorLong(sectorAddress + i, out byte[] sector);

                if(errno != ErrorNumber.NoError)
                    return errno;

                ms.Write(sector, 0, sector.Length);
            }

            buffer = ms.ToArray();

            return ErrorNumber.NoError;
        }

        /// <inheritdoc />
        public ErrorNumber ReadMediaTag(MediaTagType tag, out byte[] buffer)
        {
            buffer = null;

            switch(tag)
            {
                case MediaTagType.ATA_IDENTIFY:
                    if(_imageInfo.ReadableMediaTags.Contains(MediaTagType.ATA_IDENTIFY))
                        buffer = _identify?.Clone() as byte[];

                    return buffer == null ? ErrorNumber.NoData : ErrorNumber.NoError;
                case MediaTagType.PCMCIA_CIS:
                    if(_imageInfo.ReadableMediaTags.Contains(MediaTagType.PCMCIA_CIS))
                        buffer = _cis?.Clone() as byte[];

                    return buffer == null ? ErrorNumber.NoData : ErrorNumber.NoError;
                default: return ErrorNumber.NotSupported;
            }
        }

        /// <inheritdoc />
        public List<Track> GetSessionTracks(Session session)
        {
            if(_isHdd)
                throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

            return GetSessionTracks(session.Sequence);
        }

        /// <inheritdoc />
        public List<Track> GetSessionTracks(ushort session)
        {
            if(_isHdd)
                throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

            return _tracks.Values.Where(track => track.Session == session).ToList();
        }

        /// <inheritdoc />
        public ErrorNumber ReadSector(ulong sectorAddress, uint track, out byte[] buffer)
        {
            buffer = null;

            return _isHdd ? ErrorNumber.NotSupported : ReadSector(GetAbsoluteSector(sectorAddress, track), out buffer);
        }

        /// <inheritdoc />
        public ErrorNumber ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag, out byte[] buffer)
        {
            buffer = null;

            return _isHdd ? ErrorNumber.NotSupported
                       : ReadSectorTag(GetAbsoluteSector(sectorAddress, track), tag, out buffer);
        }

        /// <inheritdoc />
        public ErrorNumber ReadSectors(ulong sectorAddress, uint length, uint track, out byte[] buffer)
        {
            buffer = null;

            return _isHdd ? ErrorNumber.NotSupported
                       : ReadSectors(GetAbsoluteSector(sectorAddress, track), length, out buffer);
        }

        /// <inheritdoc />
        public ErrorNumber ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag,
                                          out byte[] buffer)
        {
            buffer = null;

            return _isHdd ? ErrorNumber.NotSupported
                       : ReadSectorsTag(GetAbsoluteSector(sectorAddress, track), length, tag, out buffer);
        }

        /// <inheritdoc />
        public ErrorNumber ReadSectorLong(ulong sectorAddress, uint track, out byte[] buffer)
        {
            buffer = null;

            return _isHdd ? ErrorNumber.NotSupported
                       : ReadSectorLong(GetAbsoluteSector(sectorAddress, track), out buffer);
        }

        /// <inheritdoc />
        public ErrorNumber ReadSectorsLong(ulong sectorAddress, uint length, uint track, out byte[] buffer)
        {
            buffer = null;

            return _isHdd ? ErrorNumber.NotSupported
                       : ReadSectorLong(GetAbsoluteSector(sectorAddress, track), length, out buffer);
        }
    }
}