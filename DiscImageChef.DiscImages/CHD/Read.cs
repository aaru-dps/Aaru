// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using DiscImageChef.Checksums;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Exceptions;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;

namespace DiscImageChef.DiscImages
{
    public partial class Chd
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            byte[] magic = new byte[8];
            stream.Read(magic, 0, 8);
            if(!chdTag.SequenceEqual(magic)) return false;

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
                    ChdHeaderV1 hdrV1 = BigEndianMarshal.ByteArrayToStructureBigEndian<ChdHeaderV1>(buffer);

                    DicConsole.DebugWriteLine("CHD plugin", "hdrV1.tag = \"{0}\"",
                                              Encoding.ASCII.GetString(hdrV1.tag));
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV1.length = {0} bytes", hdrV1.length);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV1.version = {0}",      hdrV1.version);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV1.flags = {0}",        (ChdFlags)hdrV1.flags);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV1.compression = {0}",
                                              (ChdCompression)hdrV1.compression);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV1.hunksize = {0}",   hdrV1.hunksize);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV1.totalhunks = {0}", hdrV1.totalhunks);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV1.cylinders = {0}",  hdrV1.cylinders);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV1.heads = {0}",      hdrV1.heads);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV1.sectors = {0}",    hdrV1.sectors);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV1.md5 = {0}",
                                              ArrayHelpers.ByteArrayToHex(hdrV1.md5));
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV1.parentmd5 = {0}",
                                              ArrayHelpers.ArrayIsNullOrEmpty(hdrV1.parentmd5)
                                                  ? "null"
                                                  : ArrayHelpers.ByteArrayToHex(hdrV1.parentmd5));

                    DicConsole.DebugWriteLine("CHD plugin", "Reading Hunk map.");
                    DateTime start = DateTime.UtcNow;

                    hunkTable = new ulong[hdrV1.totalhunks];

                    uint hunkSectorCount = (uint)Math.Ceiling((double)hdrV1.totalhunks * 8 / 512);

                    byte[] hunkSectorBytes = new byte[512];

                    for(int i = 0; i < hunkSectorCount; i++)
                    {
                        stream.Read(hunkSectorBytes, 0, 512);
                        // This does the big-endian trick but reverses the order of elements also
                        Array.Reverse(hunkSectorBytes);
                        GCHandle handle = GCHandle.Alloc(hunkSectorBytes, GCHandleType.Pinned);
                        HunkSector hunkSector =
                            (HunkSector)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(HunkSector));
                        handle.Free();
                        // This restores the order of elements
                        Array.Reverse(hunkSector.hunkEntry);
                        if(hunkTable.Length >= i * 512 / 8 + 512 / 8)
                            Array.Copy(hunkSector.hunkEntry, 0, hunkTable, i * 512 / 8, 512 / 8);
                        else
                            Array.Copy(hunkSector.hunkEntry, 0, hunkTable, i * 512 / 8, hunkTable.Length - i * 512 / 8);
                    }

                    DateTime end = DateTime.UtcNow;
                    DicConsole.DebugWriteLine("CHD plugin", "Took {0} seconds", (end - start).TotalSeconds);

                    imageInfo.MediaType    = MediaType.GENERIC_HDD;
                    imageInfo.Sectors      = hdrV1.hunksize * hdrV1.totalhunks;
                    imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
                    imageInfo.SectorSize   = 512;
                    imageInfo.Version      = "1";
                    imageInfo.ImageSize    = imageInfo.SectorSize * hdrV1.hunksize * hdrV1.totalhunks;

                    totalHunks     = hdrV1.totalhunks;
                    sectorsPerHunk = hdrV1.hunksize;
                    hdrCompression = hdrV1.compression;
                    mapVersion     = 1;
                    isHdd          = true;

                    imageInfo.Cylinders       = hdrV1.cylinders;
                    imageInfo.Heads           = hdrV1.heads;
                    imageInfo.SectorsPerTrack = hdrV1.sectors;

                    break;
                }
                case 2:
                {
                    ChdHeaderV2 hdrV2 = BigEndianMarshal.ByteArrayToStructureBigEndian<ChdHeaderV2>(buffer);

                    DicConsole.DebugWriteLine("CHD plugin", "hdrV2.tag = \"{0}\"",
                                              Encoding.ASCII.GetString(hdrV2.tag));
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV2.length = {0} bytes", hdrV2.length);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV2.version = {0}",      hdrV2.version);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV2.flags = {0}",        (ChdFlags)hdrV2.flags);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV2.compression = {0}",
                                              (ChdCompression)hdrV2.compression);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV2.hunksize = {0}",   hdrV2.hunksize);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV2.totalhunks = {0}", hdrV2.totalhunks);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV2.cylinders = {0}",  hdrV2.cylinders);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV2.heads = {0}",      hdrV2.heads);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV2.sectors = {0}",    hdrV2.sectors);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV2.md5 = {0}",
                                              ArrayHelpers.ByteArrayToHex(hdrV2.md5));
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV2.parentmd5 = {0}",
                                              ArrayHelpers.ArrayIsNullOrEmpty(hdrV2.parentmd5)
                                                  ? "null"
                                                  : ArrayHelpers.ByteArrayToHex(hdrV2.parentmd5));
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV2.seclen = {0}", hdrV2.seclen);

                    DicConsole.DebugWriteLine("CHD plugin", "Reading Hunk map.");
                    DateTime start = DateTime.UtcNow;

                    hunkTable = new ulong[hdrV2.totalhunks];

                    // How many sectors uses the BAT
                    uint hunkSectorCount = (uint)Math.Ceiling((double)hdrV2.totalhunks * 8 / 512);

                    byte[] hunkSectorBytes = new byte[512];

                    for(int i = 0; i < hunkSectorCount; i++)
                    {
                        stream.Read(hunkSectorBytes, 0, 512);
                        // This does the big-endian trick but reverses the order of elements also
                        Array.Reverse(hunkSectorBytes);
                        GCHandle handle = GCHandle.Alloc(hunkSectorBytes, GCHandleType.Pinned);
                        HunkSector hunkSector =
                            (HunkSector)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(HunkSector));
                        handle.Free();
                        // This restores the order of elements
                        Array.Reverse(hunkSector.hunkEntry);
                        if(hunkTable.Length >= i * 512 / 8 + 512 / 8)
                            Array.Copy(hunkSector.hunkEntry, 0, hunkTable, i * 512 / 8, 512 / 8);
                        else
                            Array.Copy(hunkSector.hunkEntry, 0, hunkTable, i * 512 / 8, hunkTable.Length - i * 512 / 8);
                    }

                    DateTime end = DateTime.UtcNow;
                    DicConsole.DebugWriteLine("CHD plugin", "Took {0} seconds", (end - start).TotalSeconds);

                    imageInfo.MediaType    = MediaType.GENERIC_HDD;
                    imageInfo.Sectors      = hdrV2.hunksize * hdrV2.totalhunks;
                    imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
                    imageInfo.SectorSize   = hdrV2.seclen;
                    imageInfo.Version      = "2";
                    imageInfo.ImageSize    = imageInfo.SectorSize * hdrV2.hunksize * hdrV2.totalhunks;

                    totalHunks     = hdrV2.totalhunks;
                    sectorsPerHunk = hdrV2.hunksize;
                    hdrCompression = hdrV2.compression;
                    mapVersion     = 1;
                    isHdd          = true;

                    imageInfo.Cylinders       = hdrV2.cylinders;
                    imageInfo.Heads           = hdrV2.heads;
                    imageInfo.SectorsPerTrack = hdrV2.sectors;

                    break;
                }
                case 3:
                {
                    ChdHeaderV3 hdrV3 = BigEndianMarshal.ByteArrayToStructureBigEndian<ChdHeaderV3>(buffer);

                    DicConsole.DebugWriteLine("CHD plugin", "hdrV3.tag = \"{0}\"",
                                              Encoding.ASCII.GetString(hdrV3.tag));
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV3.length = {0} bytes", hdrV3.length);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV3.version = {0}",      hdrV3.version);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV3.flags = {0}",        (ChdFlags)hdrV3.flags);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV3.compression = {0}",
                                              (ChdCompression)hdrV3.compression);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV3.totalhunks = {0}",   hdrV3.totalhunks);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV3.logicalbytes = {0}", hdrV3.logicalbytes);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV3.metaoffset = {0}",   hdrV3.metaoffset);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV3.md5 = {0}",
                                              ArrayHelpers.ByteArrayToHex(hdrV3.md5));
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV3.parentmd5 = {0}",
                                              ArrayHelpers.ArrayIsNullOrEmpty(hdrV3.parentmd5)
                                                  ? "null"
                                                  : ArrayHelpers.ByteArrayToHex(hdrV3.parentmd5));
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV3.hunkbytes = {0}", hdrV3.hunkbytes);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV3.sha1 = {0}",
                                              ArrayHelpers.ByteArrayToHex(hdrV3.sha1));
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV3.parentsha1 = {0}",
                                              ArrayHelpers.ArrayIsNullOrEmpty(hdrV3.parentsha1)
                                                  ? "null"
                                                  : ArrayHelpers.ByteArrayToHex(hdrV3.parentsha1));

                    DicConsole.DebugWriteLine("CHD plugin", "Reading Hunk map.");
                    DateTime start = DateTime.UtcNow;

                    hunkMap = new byte[hdrV3.totalhunks * 16];
                    stream.Read(hunkMap, 0, hunkMap.Length);

                    DateTime end = DateTime.UtcNow;
                    DicConsole.DebugWriteLine("CHD plugin", "Took {0} seconds", (end - start).TotalSeconds);

                    nextMetaOff = hdrV3.metaoffset;

                    imageInfo.ImageSize = hdrV3.logicalbytes;
                    imageInfo.Version   = "3";

                    totalHunks     = hdrV3.totalhunks;
                    bytesPerHunk   = hdrV3.hunkbytes;
                    hdrCompression = hdrV3.compression;
                    mapVersion     = 3;

                    break;
                }
                case 4:
                {
                    ChdHeaderV4 hdrV4 = BigEndianMarshal.ByteArrayToStructureBigEndian<ChdHeaderV4>(buffer);

                    DicConsole.DebugWriteLine("CHD plugin", "hdrV4.tag = \"{0}\"",
                                              Encoding.ASCII.GetString(hdrV4.tag));
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV4.length = {0} bytes", hdrV4.length);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV4.version = {0}",      hdrV4.version);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV4.flags = {0}",        (ChdFlags)hdrV4.flags);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV4.compression = {0}",
                                              (ChdCompression)hdrV4.compression);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV4.totalhunks = {0}",   hdrV4.totalhunks);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV4.logicalbytes = {0}", hdrV4.logicalbytes);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV4.metaoffset = {0}",   hdrV4.metaoffset);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV4.hunkbytes = {0}",    hdrV4.hunkbytes);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV4.sha1 = {0}",
                                              ArrayHelpers.ByteArrayToHex(hdrV4.sha1));
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV4.parentsha1 = {0}",
                                              ArrayHelpers.ArrayIsNullOrEmpty(hdrV4.parentsha1)
                                                  ? "null"
                                                  : ArrayHelpers.ByteArrayToHex(hdrV4.parentsha1));
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV4.rawsha1 = {0}",
                                              ArrayHelpers.ByteArrayToHex(hdrV4.rawsha1));

                    DicConsole.DebugWriteLine("CHD plugin", "Reading Hunk map.");
                    DateTime start = DateTime.UtcNow;

                    hunkMap = new byte[hdrV4.totalhunks * 16];
                    stream.Read(hunkMap, 0, hunkMap.Length);

                    DateTime end = DateTime.UtcNow;
                    DicConsole.DebugWriteLine("CHD plugin", "Took {0} seconds", (end - start).TotalSeconds);

                    nextMetaOff = hdrV4.metaoffset;

                    imageInfo.ImageSize = hdrV4.logicalbytes;
                    imageInfo.Version   = "4";

                    totalHunks     = hdrV4.totalhunks;
                    bytesPerHunk   = hdrV4.hunkbytes;
                    hdrCompression = hdrV4.compression;
                    mapVersion     = 3;

                    break;
                }
                case 5:
                {
                    ChdHeaderV5 hdrV5 = BigEndianMarshal.ByteArrayToStructureBigEndian<ChdHeaderV5>(buffer);

                    DicConsole.DebugWriteLine("CHD plugin", "hdrV5.tag = \"{0}\"",
                                              Encoding.ASCII.GetString(hdrV5.tag));
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV5.length = {0} bytes", hdrV5.length);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV5.version = {0}",      hdrV5.version);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV5.compressor0 = \"{0}\"",
                                              Encoding.ASCII.GetString(BigEndianBitConverter
                                                                          .GetBytes(hdrV5.compressor0)));
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV5.compressor1 = \"{0}\"",
                                              Encoding.ASCII.GetString(BigEndianBitConverter
                                                                          .GetBytes(hdrV5.compressor1)));
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV5.compressor2 = \"{0}\"",
                                              Encoding.ASCII.GetString(BigEndianBitConverter
                                                                          .GetBytes(hdrV5.compressor2)));
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV5.compressor3 = \"{0}\"",
                                              Encoding.ASCII.GetString(BigEndianBitConverter
                                                                          .GetBytes(hdrV5.compressor3)));
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV5.logicalbytes = {0}", hdrV5.logicalbytes);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV5.mapoffset = {0}",    hdrV5.mapoffset);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV5.metaoffset = {0}",   hdrV5.metaoffset);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV5.hunkbytes = {0}",    hdrV5.hunkbytes);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV5.unitbytes = {0}",    hdrV5.unitbytes);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV5.sha1 = {0}",
                                              ArrayHelpers.ByteArrayToHex(hdrV5.sha1));
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV5.parentsha1 = {0}",
                                              ArrayHelpers.ArrayIsNullOrEmpty(hdrV5.parentsha1)
                                                  ? "null"
                                                  : ArrayHelpers.ByteArrayToHex(hdrV5.parentsha1));
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV5.rawsha1 = {0}",
                                              ArrayHelpers.ByteArrayToHex(hdrV5.rawsha1));

                    // TODO: Implement compressed CHD v5
                    if(hdrV5.compressor0 == 0)
                    {
                        DicConsole.DebugWriteLine("CHD plugin", "Reading Hunk map.");
                        DateTime start = DateTime.UtcNow;

                        hunkTableSmall = new uint[hdrV5.logicalbytes / hdrV5.hunkbytes];

                        uint hunkSectorCount = (uint)Math.Ceiling((double)hunkTableSmall.Length * 4 / 512);

                        byte[] hunkSectorBytes = new byte[512];

                        stream.Seek((long)hdrV5.mapoffset, SeekOrigin.Begin);

                        for(int i = 0; i < hunkSectorCount; i++)
                        {
                            stream.Read(hunkSectorBytes, 0, 512);
                            // This does the big-endian trick but reverses the order of elements also
                            Array.Reverse(hunkSectorBytes);
                            GCHandle handle = GCHandle.Alloc(hunkSectorBytes, GCHandleType.Pinned);
                            HunkSectorSmall hunkSector =
                                (HunkSectorSmall)Marshal.PtrToStructure(handle.AddrOfPinnedObject(),
                                                                        typeof(HunkSectorSmall));
                            handle.Free();
                            // This restores the order of elements
                            Array.Reverse(hunkSector.hunkEntry);
                            if(hunkTableSmall.Length >= i * 512 / 4 + 512 / 4)
                                Array.Copy(hunkSector.hunkEntry, 0, hunkTableSmall, i * 512 / 4, 512 / 4);
                            else
                                Array.Copy(hunkSector.hunkEntry, 0, hunkTableSmall, i * 512 / 4,
                                           hunkTableSmall.Length - i * 512            / 4);
                        }

                        DateTime end = DateTime.UtcNow;
                        DicConsole.DebugWriteLine("CHD plugin", "Took {0} seconds", (end - start).TotalSeconds);
                    }
                    else throw new ImageNotSupportedException("Cannot read compressed CHD version 5");

                    nextMetaOff = hdrV5.metaoffset;

                    imageInfo.ImageSize = hdrV5.logicalbytes;
                    imageInfo.Version   = "5";

                    totalHunks      = (uint)(hdrV5.logicalbytes / hdrV5.hunkbytes);
                    bytesPerHunk    = hdrV5.hunkbytes;
                    hdrCompression  = hdrV5.compressor0;
                    hdrCompression1 = hdrV5.compressor1;
                    hdrCompression2 = hdrV5.compressor2;
                    hdrCompression3 = hdrV5.compressor3;
                    mapVersion      = 5;

                    break;
                }
                default: throw new ImageNotSupportedException($"Unsupported CHD version {version}");
            }

            if(mapVersion >= 3)
            {
                isCdrom   = false;
                isHdd     = false;
                isGdrom   = false;
                swapAudio = false;
                tracks    = new Dictionary<uint, Track>();

                DicConsole.DebugWriteLine("CHD plugin", "Reading metadata.");

                ulong currentSector = 0;
                uint  currentTrack  = 1;

                while(nextMetaOff > 0)
                {
                    byte[] hdrBytes = new byte[16];
                    stream.Seek((long)nextMetaOff, SeekOrigin.Begin);
                    stream.Read(hdrBytes, 0, hdrBytes.Length);
                    ChdMetadataHeader header =
                        BigEndianMarshal.ByteArrayToStructureBigEndian<ChdMetadataHeader>(hdrBytes);
                    byte[] meta = new byte[header.flagsAndLength & 0xFFFFFF];
                    stream.Read(meta, 0, meta.Length);
                    DicConsole.DebugWriteLine("CHD plugin", "Found metadata \"{0}\"",
                                              Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(header.tag)));

                    switch(header.tag)
                    {
                        // "GDDD"
                        case HARD_DISK_METADATA:
                            if(isCdrom || isGdrom)
                                throw new
                                    ImageNotSupportedException("Image cannot be a hard disk and a C/GD-ROM at the same time, aborting.");

                            string gddd      = StringHandlers.CToString(meta);
                            Regex  gdddRegEx = new Regex(REGEX_METADATA_HDD);
                            Match  gdddMatch = gdddRegEx.Match(gddd);
                            if(gdddMatch.Success)
                            {
                                isHdd                     = true;
                                imageInfo.SectorSize      = uint.Parse(gdddMatch.Groups["bps"].Value);
                                imageInfo.Cylinders       = uint.Parse(gdddMatch.Groups["cylinders"].Value);
                                imageInfo.Heads           = uint.Parse(gdddMatch.Groups["heads"].Value);
                                imageInfo.SectorsPerTrack = uint.Parse(gdddMatch.Groups["sectors"].Value);
                            }

                            break;
                        // "CHCD"
                        case CDROM_OLD_METADATA:
                            if(isHdd)
                                throw new
                                    ImageNotSupportedException("Image cannot be a hard disk and a CD-ROM at the same time, aborting.");

                            if(isGdrom)
                                throw new
                                    ImageNotSupportedException("Image cannot be a GD-ROM and a CD-ROM at the same time, aborting.");

                            uint chdTracksNumber = BigEndianBitConverter.ToUInt32(meta, 0);

                            // Byteswapped
                            if(chdTracksNumber > 99)
                            {
                                BigEndianBitConverter.IsLittleEndian = !BitConverter.IsLittleEndian;
                                chdTracksNumber                      = BigEndianBitConverter.ToUInt32(meta, 0);
                            }

                            currentSector = 0;

                            for(uint i = 0; i < chdTracksNumber; i++)
                            {
                                ChdTrackOld chdTrack = new ChdTrackOld
                                {
                                    type        = BigEndianBitConverter.ToUInt32(meta, (int)(4 + i * 24 + 0)),
                                    subType     = BigEndianBitConverter.ToUInt32(meta, (int)(4 + i * 24 + 4)),
                                    dataSize    = BigEndianBitConverter.ToUInt32(meta, (int)(4 + i * 24 + 8)),
                                    subSize     = BigEndianBitConverter.ToUInt32(meta, (int)(4 + i * 24 + 12)),
                                    frames      = BigEndianBitConverter.ToUInt32(meta, (int)(4 + i * 24 + 16)),
                                    extraFrames = BigEndianBitConverter.ToUInt32(meta, (int)(4 + i * 24 + 20))
                                };

                                Track dicTrack = new Track();
                                switch((ChdOldTrackType)chdTrack.type)
                                {
                                    case ChdOldTrackType.Audio:
                                        dicTrack.TrackBytesPerSector    = 2352;
                                        dicTrack.TrackRawBytesPerSector = 2352;
                                        dicTrack.TrackType              = TrackType.Audio;
                                        break;
                                    case ChdOldTrackType.Mode1:
                                        dicTrack.TrackBytesPerSector    = 2048;
                                        dicTrack.TrackRawBytesPerSector = 2048;
                                        dicTrack.TrackType              = TrackType.CdMode1;
                                        break;
                                    case ChdOldTrackType.Mode1Raw:
                                        dicTrack.TrackBytesPerSector    = 2048;
                                        dicTrack.TrackRawBytesPerSector = 2352;
                                        dicTrack.TrackType              = TrackType.CdMode1;
                                        break;
                                    case ChdOldTrackType.Mode2:
                                    case ChdOldTrackType.Mode2FormMix:
                                        dicTrack.TrackBytesPerSector    = 2336;
                                        dicTrack.TrackRawBytesPerSector = 2336;
                                        dicTrack.TrackType              = TrackType.CdMode2Formless;
                                        break;
                                    case ChdOldTrackType.Mode2Form1:
                                        dicTrack.TrackBytesPerSector    = 2048;
                                        dicTrack.TrackRawBytesPerSector = 2048;
                                        dicTrack.TrackType              = TrackType.CdMode2Form1;
                                        break;
                                    case ChdOldTrackType.Mode2Form2:
                                        dicTrack.TrackBytesPerSector    = 2324;
                                        dicTrack.TrackRawBytesPerSector = 2324;
                                        dicTrack.TrackType              = TrackType.CdMode2Form2;
                                        break;
                                    case ChdOldTrackType.Mode2Raw:
                                        dicTrack.TrackBytesPerSector    = 2336;
                                        dicTrack.TrackRawBytesPerSector = 2352;
                                        dicTrack.TrackType              = TrackType.CdMode2Formless;
                                        break;
                                    default:
                                        throw new ImageNotSupportedException($"Unsupported track type {chdTrack.type}");
                                }

                                switch((ChdOldSubType)chdTrack.subType)
                                {
                                    case ChdOldSubType.Cooked:
                                        dicTrack.TrackSubchannelFile   = imageFilter.GetFilename();
                                        dicTrack.TrackSubchannelType   = TrackSubchannelType.PackedInterleaved;
                                        dicTrack.TrackSubchannelFilter = imageFilter;
                                        break;
                                    case ChdOldSubType.None:
                                        dicTrack.TrackSubchannelType = TrackSubchannelType.None;
                                        break;
                                    case ChdOldSubType.Raw:
                                        dicTrack.TrackSubchannelFile   = imageFilter.GetFilename();
                                        dicTrack.TrackSubchannelType   = TrackSubchannelType.RawInterleaved;
                                        dicTrack.TrackSubchannelFilter = imageFilter;
                                        break;
                                    default:
                                        throw new
                                            ImageNotSupportedException($"Unsupported subchannel type {chdTrack.type}");
                                }

                                dicTrack.Indexes          =  new Dictionary<int, ulong>();
                                dicTrack.TrackDescription =  $"Track {i + 1}";
                                dicTrack.TrackEndSector   =  currentSector + chdTrack.frames - 1;
                                dicTrack.TrackFile        =  imageFilter.GetFilename();
                                dicTrack.TrackFileType    =  "BINARY";
                                dicTrack.TrackFilter      =  imageFilter;
                                dicTrack.TrackStartSector =  currentSector;
                                dicTrack.TrackSequence    =  i + 1;
                                dicTrack.TrackSession     =  1;
                                currentSector             += chdTrack.frames + chdTrack.extraFrames;
                                tracks.Add(dicTrack.TrackSequence, dicTrack);
                            }

                            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
                            isCdrom                              = true;

                            break;
                        // "CHTR"
                        case CDROM_TRACK_METADATA:
                            if(isHdd)
                                throw new
                                    ImageNotSupportedException("Image cannot be a hard disk and a CD-ROM at the same time, aborting.");

                            if(isGdrom)
                                throw new
                                    ImageNotSupportedException("Image cannot be a GD-ROM and a CD-ROM at the same time, aborting.");

                            string chtr      = StringHandlers.CToString(meta);
                            Regex  chtrRegEx = new Regex(REGEX_METADATA_CDROM);
                            Match  chtrMatch = chtrRegEx.Match(chtr);
                            if(chtrMatch.Success)
                            {
                                isCdrom = true;

                                uint   trackNo   = uint.Parse(chtrMatch.Groups["track"].Value);
                                uint   frames    = uint.Parse(chtrMatch.Groups["frames"].Value);
                                string subtype   = chtrMatch.Groups["sub_type"].Value;
                                string tracktype = chtrMatch.Groups["track_type"].Value;

                                if(trackNo != currentTrack)
                                    throw new ImageNotSupportedException("Unsorted tracks, cannot proceed.");

                                Track dicTrack = new Track();
                                switch(tracktype)
                                {
                                    case TRACK_TYPE_AUDIO:
                                        dicTrack.TrackBytesPerSector    = 2352;
                                        dicTrack.TrackRawBytesPerSector = 2352;
                                        dicTrack.TrackType              = TrackType.Audio;
                                        break;
                                    case TRACK_TYPE_MODE1:
                                    case TRACK_TYPE_MODE1_2K:
                                        dicTrack.TrackBytesPerSector    = 2048;
                                        dicTrack.TrackRawBytesPerSector = 2048;
                                        dicTrack.TrackType              = TrackType.CdMode1;
                                        break;
                                    case TRACK_TYPE_MODE1_RAW:
                                    case TRACK_TYPE_MODE1_RAW_2K:
                                        dicTrack.TrackBytesPerSector    = 2048;
                                        dicTrack.TrackRawBytesPerSector = 2352;
                                        dicTrack.TrackType              = TrackType.CdMode1;
                                        break;
                                    case TRACK_TYPE_MODE2:
                                    case TRACK_TYPE_MODE2_2K:
                                    case TRACK_TYPE_MODE2_FM:
                                        dicTrack.TrackBytesPerSector    = 2336;
                                        dicTrack.TrackRawBytesPerSector = 2336;
                                        dicTrack.TrackType              = TrackType.CdMode2Formless;
                                        break;
                                    case TRACK_TYPE_MODE2_F1:
                                    case TRACK_TYPE_MODE2_F1_2K:
                                        dicTrack.TrackBytesPerSector    = 2048;
                                        dicTrack.TrackRawBytesPerSector = 2048;
                                        dicTrack.TrackType              = TrackType.CdMode2Form1;
                                        break;
                                    case TRACK_TYPE_MODE2_F2:
                                    case TRACK_TYPE_MODE2_F2_2K:
                                        dicTrack.TrackBytesPerSector    = 2324;
                                        dicTrack.TrackRawBytesPerSector = 2324;
                                        dicTrack.TrackType              = TrackType.CdMode2Form2;
                                        break;
                                    case TRACK_TYPE_MODE2_RAW:
                                    case TRACK_TYPE_MODE2_RAW_2K:
                                        dicTrack.TrackBytesPerSector    = 2336;
                                        dicTrack.TrackRawBytesPerSector = 2352;
                                        dicTrack.TrackType              = TrackType.CdMode2Formless;
                                        break;
                                    default:
                                        throw new ImageNotSupportedException($"Unsupported track type {tracktype}");
                                }

                                switch(subtype)
                                {
                                    case SUB_TYPE_COOKED:
                                        dicTrack.TrackSubchannelFile   = imageFilter.GetFilename();
                                        dicTrack.TrackSubchannelType   = TrackSubchannelType.PackedInterleaved;
                                        dicTrack.TrackSubchannelFilter = imageFilter;
                                        break;
                                    case SUB_TYPE_NONE:
                                        dicTrack.TrackSubchannelType = TrackSubchannelType.None;
                                        break;
                                    case SUB_TYPE_RAW:
                                        dicTrack.TrackSubchannelFile   = imageFilter.GetFilename();
                                        dicTrack.TrackSubchannelType   = TrackSubchannelType.RawInterleaved;
                                        dicTrack.TrackSubchannelFilter = imageFilter;
                                        break;
                                    default:
                                        throw new ImageNotSupportedException($"Unsupported subchannel type {subtype}");
                                }

                                dicTrack.Indexes          =  new Dictionary<int, ulong>();
                                dicTrack.TrackDescription =  $"Track {trackNo}";
                                dicTrack.TrackEndSector   =  currentSector + frames - 1;
                                dicTrack.TrackFile        =  imageFilter.GetFilename();
                                dicTrack.TrackFileType    =  "BINARY";
                                dicTrack.TrackFilter      =  imageFilter;
                                dicTrack.TrackStartSector =  currentSector;
                                dicTrack.TrackSequence    =  trackNo;
                                dicTrack.TrackSession     =  1;
                                currentSector             += frames;
                                currentTrack++;
                                tracks.Add(dicTrack.TrackSequence, dicTrack);
                            }

                            break;
                        // "CHT2"
                        case CDROM_TRACK_METADATA2:
                            if(isHdd)
                                throw new
                                    ImageNotSupportedException("Image cannot be a hard disk and a CD-ROM at the same time, aborting.");

                            if(isGdrom)
                                throw new
                                    ImageNotSupportedException("Image cannot be a GD-ROM and a CD-ROM at the same time, aborting.");

                            string cht2      = StringHandlers.CToString(meta);
                            Regex  cht2RegEx = new Regex(REGEX_METADATA_CDROM2);
                            Match  cht2Match = cht2RegEx.Match(cht2);
                            if(cht2Match.Success)
                            {
                                isCdrom = true;

                                uint   trackNo   = uint.Parse(cht2Match.Groups["track"].Value);
                                uint   frames    = uint.Parse(cht2Match.Groups["frames"].Value);
                                string subtype   = cht2Match.Groups["sub_type"].Value;
                                string tracktype = cht2Match.Groups["track_type"].Value;
                                // TODO: Check pregap and postgap behaviour
                                uint   pregap        = uint.Parse(cht2Match.Groups["pregap"].Value);
                                string pregapType    = cht2Match.Groups["pgtype"].Value;
                                string pregapSubType = cht2Match.Groups["pgsub"].Value;
                                uint   postgap       = uint.Parse(cht2Match.Groups["postgap"].Value);

                                if(trackNo != currentTrack)
                                    throw new ImageNotSupportedException("Unsorted tracks, cannot proceed.");

                                Track dicTrack = new Track();
                                switch(tracktype)
                                {
                                    case TRACK_TYPE_AUDIO:
                                        dicTrack.TrackBytesPerSector    = 2352;
                                        dicTrack.TrackRawBytesPerSector = 2352;
                                        dicTrack.TrackType              = TrackType.Audio;
                                        break;
                                    case TRACK_TYPE_MODE1:
                                    case TRACK_TYPE_MODE1_2K:
                                        dicTrack.TrackBytesPerSector    = 2048;
                                        dicTrack.TrackRawBytesPerSector = 2048;
                                        dicTrack.TrackType              = TrackType.CdMode1;
                                        break;
                                    case TRACK_TYPE_MODE1_RAW:
                                    case TRACK_TYPE_MODE1_RAW_2K:
                                        dicTrack.TrackBytesPerSector    = 2048;
                                        dicTrack.TrackRawBytesPerSector = 2352;
                                        dicTrack.TrackType              = TrackType.CdMode1;
                                        break;
                                    case TRACK_TYPE_MODE2:
                                    case TRACK_TYPE_MODE2_2K:
                                    case TRACK_TYPE_MODE2_FM:
                                        dicTrack.TrackBytesPerSector    = 2336;
                                        dicTrack.TrackRawBytesPerSector = 2336;
                                        dicTrack.TrackType              = TrackType.CdMode2Formless;
                                        break;
                                    case TRACK_TYPE_MODE2_F1:
                                    case TRACK_TYPE_MODE2_F1_2K:
                                        dicTrack.TrackBytesPerSector    = 2048;
                                        dicTrack.TrackRawBytesPerSector = 2048;
                                        dicTrack.TrackType              = TrackType.CdMode2Form1;
                                        break;
                                    case TRACK_TYPE_MODE2_F2:
                                    case TRACK_TYPE_MODE2_F2_2K:
                                        dicTrack.TrackBytesPerSector    = 2324;
                                        dicTrack.TrackRawBytesPerSector = 2324;
                                        dicTrack.TrackType              = TrackType.CdMode2Form2;
                                        break;
                                    case TRACK_TYPE_MODE2_RAW:
                                    case TRACK_TYPE_MODE2_RAW_2K:
                                        dicTrack.TrackBytesPerSector    = 2336;
                                        dicTrack.TrackRawBytesPerSector = 2352;
                                        dicTrack.TrackType              = TrackType.CdMode2Formless;
                                        break;
                                    default:
                                        throw new ImageNotSupportedException($"Unsupported track type {tracktype}");
                                }

                                switch(subtype)
                                {
                                    case SUB_TYPE_COOKED:
                                        dicTrack.TrackSubchannelFile   = imageFilter.GetFilename();
                                        dicTrack.TrackSubchannelType   = TrackSubchannelType.PackedInterleaved;
                                        dicTrack.TrackSubchannelFilter = imageFilter;
                                        break;
                                    case SUB_TYPE_NONE:
                                        dicTrack.TrackSubchannelType = TrackSubchannelType.None;
                                        break;
                                    case SUB_TYPE_RAW:
                                        dicTrack.TrackSubchannelFile   = imageFilter.GetFilename();
                                        dicTrack.TrackSubchannelType   = TrackSubchannelType.RawInterleaved;
                                        dicTrack.TrackSubchannelFilter = imageFilter;
                                        break;
                                    default:
                                        throw new ImageNotSupportedException($"Unsupported subchannel type {subtype}");
                                }

                                dicTrack.Indexes          =  new Dictionary<int, ulong>();
                                dicTrack.TrackDescription =  $"Track {trackNo}";
                                dicTrack.TrackEndSector   =  currentSector + frames - 1;
                                dicTrack.TrackFile        =  imageFilter.GetFilename();
                                dicTrack.TrackFileType    =  "BINARY";
                                dicTrack.TrackFilter      =  imageFilter;
                                dicTrack.TrackStartSector =  currentSector;
                                dicTrack.TrackSequence    =  trackNo;
                                dicTrack.TrackSession     =  1;
                                currentSector             += frames;
                                currentTrack++;
                                tracks.Add(dicTrack.TrackSequence, dicTrack);
                            }

                            break;
                        // "CHGT"
                        case GDROM_OLD_METADATA:
                            swapAudio = true;
                            goto case GDROM_METADATA;
                        // "CHGD"
                        case GDROM_METADATA:
                            if(isHdd)
                                throw new
                                    ImageNotSupportedException("Image cannot be a hard disk and a GD-ROM at the same time, aborting.");

                            if(isCdrom)
                                throw new
                                    ImageNotSupportedException("Image cannot be a CD-ROM and a GD-ROM at the same time, aborting.");

                            string chgd      = StringHandlers.CToString(meta);
                            Regex  chgdRegEx = new Regex(REGEX_METADATA_GDROM);
                            Match  chgdMatch = chgdRegEx.Match(chgd);
                            if(chgdMatch.Success)
                            {
                                isGdrom = true;

                                uint   trackNo   = uint.Parse(chgdMatch.Groups["track"].Value);
                                uint   frames    = uint.Parse(chgdMatch.Groups["frames"].Value);
                                string subtype   = chgdMatch.Groups["sub_type"].Value;
                                string tracktype = chgdMatch.Groups["track_type"].Value;
                                // TODO: Check pregap, postgap and pad behaviour
                                uint   pregap        = uint.Parse(chgdMatch.Groups["pregap"].Value);
                                string pregapType    = chgdMatch.Groups["pgtype"].Value;
                                string pregapSubType = chgdMatch.Groups["pgsub"].Value;
                                uint   postgap       = uint.Parse(chgdMatch.Groups["postgap"].Value);
                                uint   pad           = uint.Parse(chgdMatch.Groups["pad"].Value);

                                if(trackNo != currentTrack)
                                    throw new ImageNotSupportedException("Unsorted tracks, cannot proceed.");

                                Track dicTrack = new Track();
                                switch(tracktype)
                                {
                                    case TRACK_TYPE_AUDIO:
                                        dicTrack.TrackBytesPerSector    = 2352;
                                        dicTrack.TrackRawBytesPerSector = 2352;
                                        dicTrack.TrackType              = TrackType.Audio;
                                        break;
                                    case TRACK_TYPE_MODE1:
                                    case TRACK_TYPE_MODE1_2K:
                                        dicTrack.TrackBytesPerSector    = 2048;
                                        dicTrack.TrackRawBytesPerSector = 2048;
                                        dicTrack.TrackType              = TrackType.CdMode1;
                                        break;
                                    case TRACK_TYPE_MODE1_RAW:
                                    case TRACK_TYPE_MODE1_RAW_2K:
                                        dicTrack.TrackBytesPerSector    = 2048;
                                        dicTrack.TrackRawBytesPerSector = 2352;
                                        dicTrack.TrackType              = TrackType.CdMode1;
                                        break;
                                    case TRACK_TYPE_MODE2:
                                    case TRACK_TYPE_MODE2_2K:
                                    case TRACK_TYPE_MODE2_FM:
                                        dicTrack.TrackBytesPerSector    = 2336;
                                        dicTrack.TrackRawBytesPerSector = 2336;
                                        dicTrack.TrackType              = TrackType.CdMode2Formless;
                                        break;
                                    case TRACK_TYPE_MODE2_F1:
                                    case TRACK_TYPE_MODE2_F1_2K:
                                        dicTrack.TrackBytesPerSector    = 2048;
                                        dicTrack.TrackRawBytesPerSector = 2048;
                                        dicTrack.TrackType              = TrackType.CdMode2Form1;
                                        break;
                                    case TRACK_TYPE_MODE2_F2:
                                    case TRACK_TYPE_MODE2_F2_2K:
                                        dicTrack.TrackBytesPerSector    = 2324;
                                        dicTrack.TrackRawBytesPerSector = 2324;
                                        dicTrack.TrackType              = TrackType.CdMode2Form2;
                                        break;
                                    case TRACK_TYPE_MODE2_RAW:
                                    case TRACK_TYPE_MODE2_RAW_2K:
                                        dicTrack.TrackBytesPerSector    = 2336;
                                        dicTrack.TrackRawBytesPerSector = 2352;
                                        dicTrack.TrackType              = TrackType.CdMode2Formless;
                                        break;
                                    default:
                                        throw new ImageNotSupportedException($"Unsupported track type {tracktype}");
                                }

                                switch(subtype)
                                {
                                    case SUB_TYPE_COOKED:
                                        dicTrack.TrackSubchannelFile   = imageFilter.GetFilename();
                                        dicTrack.TrackSubchannelType   = TrackSubchannelType.PackedInterleaved;
                                        dicTrack.TrackSubchannelFilter = imageFilter;
                                        break;
                                    case SUB_TYPE_NONE:
                                        dicTrack.TrackSubchannelType = TrackSubchannelType.None;
                                        break;
                                    case SUB_TYPE_RAW:
                                        dicTrack.TrackSubchannelFile   = imageFilter.GetFilename();
                                        dicTrack.TrackSubchannelType   = TrackSubchannelType.RawInterleaved;
                                        dicTrack.TrackSubchannelFilter = imageFilter;
                                        break;
                                    default:
                                        throw new ImageNotSupportedException($"Unsupported subchannel type {subtype}");
                                }

                                dicTrack.Indexes          =  new Dictionary<int, ulong>();
                                dicTrack.TrackDescription =  $"Track {trackNo}";
                                dicTrack.TrackEndSector   =  currentSector + frames - 1;
                                dicTrack.TrackFile        =  imageFilter.GetFilename();
                                dicTrack.TrackFileType    =  "BINARY";
                                dicTrack.TrackFilter      =  imageFilter;
                                dicTrack.TrackStartSector =  currentSector;
                                dicTrack.TrackSequence    =  trackNo;
                                dicTrack.TrackSession     =  (ushort)(trackNo > 2 ? 2 : 1);
                                currentSector             += frames;
                                currentTrack++;
                                tracks.Add(dicTrack.TrackSequence, dicTrack);
                            }

                            break;
                        // "IDNT"
                        case HARD_DISK_IDENT_METADATA:
                            Decoders.ATA.Identify.IdentifyDevice? idnt = Decoders.ATA.Identify.Decode(meta);
                            if(idnt.HasValue)
                            {
                                imageInfo.MediaManufacturer     = idnt.Value.MediaManufacturer;
                                imageInfo.MediaSerialNumber     = idnt.Value.MediaSerial;
                                imageInfo.DriveModel            = idnt.Value.Model;
                                imageInfo.DriveSerialNumber     = idnt.Value.SerialNumber;
                                imageInfo.DriveFirmwareRevision = idnt.Value.FirmwareRevision;
                                if(idnt.Value.CurrentCylinders       > 0 && idnt.Value.CurrentHeads > 0 &&
                                   idnt.Value.CurrentSectorsPerTrack > 0)
                                {
                                    imageInfo.Cylinders       = idnt.Value.CurrentCylinders;
                                    imageInfo.Heads           = idnt.Value.CurrentHeads;
                                    imageInfo.SectorsPerTrack = idnt.Value.CurrentSectorsPerTrack;
                                }
                                else
                                {
                                    imageInfo.Cylinders       = idnt.Value.Cylinders;
                                    imageInfo.Heads           = idnt.Value.Heads;
                                    imageInfo.SectorsPerTrack = idnt.Value.SectorsPerTrack;
                                }
                            }

                            identify = meta;
                            if(!imageInfo.ReadableMediaTags.Contains(MediaTagType.ATA_IDENTIFY))
                                imageInfo.ReadableMediaTags.Add(MediaTagType.ATA_IDENTIFY);
                            break;
                        case PCMCIA_CIS_METADATA:
                            cis = meta;
                            if(!imageInfo.ReadableMediaTags.Contains(MediaTagType.PCMCIA_CIS))
                                imageInfo.ReadableMediaTags.Add(MediaTagType.PCMCIA_CIS);
                            break;
                    }

                    nextMetaOff = header.next;
                }

                if(isHdd)
                {
                    sectorsPerHunk         = bytesPerHunk        / imageInfo.SectorSize;
                    imageInfo.Sectors      = imageInfo.ImageSize / imageInfo.SectorSize;
                    imageInfo.MediaType    = MediaType.GENERIC_HDD;
                    imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
                }
                else if(isCdrom)
                {
                    // Hardcoded on MAME for CD-ROM
                    sectorsPerHunk         = 8;
                    imageInfo.MediaType    = MediaType.CDROM;
                    imageInfo.XmlMediaType = XmlMediaType.OpticalDisc;

                    foreach(Track dicTrack in tracks.Values)
                        imageInfo.Sectors += dicTrack.TrackEndSector - dicTrack.TrackStartSector + 1;
                }
                else if(isGdrom)
                {
                    // Hardcoded on MAME for GD-ROM
                    sectorsPerHunk         = 8;
                    imageInfo.MediaType    = MediaType.GDROM;
                    imageInfo.XmlMediaType = XmlMediaType.OpticalDisc;

                    foreach(Track dicTrack in tracks.Values)
                        imageInfo.Sectors += dicTrack.TrackEndSector - dicTrack.TrackStartSector + 1;
                }
                else throw new ImageNotSupportedException("Image does not represent a known media, aborting");
            }

            if(isCdrom || isGdrom)
            {
                offsetmap  = new Dictionary<ulong, uint>();
                partitions = new List<Partition>();
                ulong partPos = 0;
                foreach(Track dicTrack in tracks.Values)
                {
                    Partition partition = new Partition
                    {
                        Description = dicTrack.TrackDescription,
                        Size =
                            (dicTrack.TrackEndSector - dicTrack.TrackStartSector + 1) *
                            (ulong)dicTrack.TrackRawBytesPerSector,
                        Length   = dicTrack.TrackEndSector - dicTrack.TrackStartSector + 1,
                        Sequence = dicTrack.TrackSequence,
                        Offset   = partPos,
                        Start    = dicTrack.TrackStartSector,
                        Type     = dicTrack.TrackType.ToString()
                    };
                    partPos += partition.Length;
                    offsetmap.Add(dicTrack.TrackStartSector, dicTrack.TrackSequence);

                    if(dicTrack.TrackSubchannelType != TrackSubchannelType.None)
                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);

                    switch(dicTrack.TrackType)
                    {
                        case TrackType.CdMode1:
                        case TrackType.CdMode2Form1:
                            if(dicTrack.TrackRawBytesPerSector == 2352)
                            {
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
                            }

                            break;
                        case TrackType.CdMode2Form2:
                            if(dicTrack.TrackRawBytesPerSector == 2352)
                            {
                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);
                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);
                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);
                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);
                            }

                            break;
                        case TrackType.CdMode2Formless:
                            if(dicTrack.TrackRawBytesPerSector == 2352)
                            {
                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);
                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);
                            }

                            break;
                    }

                    if(dicTrack.TrackBytesPerSector > imageInfo.SectorSize)
                        imageInfo.SectorSize = (uint)dicTrack.TrackBytesPerSector;

                    partitions.Add(partition);
                }

                imageInfo.HasPartitions = true;
                imageInfo.HasSessions   = true;
            }

            maxBlockCache  = (int)(MAX_CACHE_SIZE / (imageInfo.SectorSize * sectorsPerHunk));
            maxSectorCache = (int)(MAX_CACHE_SIZE / imageInfo.SectorSize);

            imageStream = stream;

            sectorCache = new Dictionary<ulong, byte[]>();
            hunkCache   = new Dictionary<ulong, byte[]>();

            // TODO: Detect CompactFlash
            // TODO: Get manufacturer and drive name from CIS if applicable
            if(cis != null) imageInfo.MediaType = MediaType.PCCardTypeI;

            return true;
        }

        public bool? VerifySector(ulong sectorAddress)
        {
            if(isHdd) return null;

            byte[] buffer = ReadSectorLong(sectorAddress);
            return CdChecksums.CheckCdSector(buffer);
        }

        public bool? VerifySector(ulong sectorAddress, uint track)
        {
            if(isHdd)
                throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

            return VerifySector(GetAbsoluteSector(sectorAddress, track));
        }

        public bool? VerifySectors(ulong           sectorAddress, uint length, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas)
        {
            unknownLbas = new List<ulong>();
            failingLbas = new List<ulong>();
            if(isHdd) return null;

            byte[] buffer = ReadSectorsLong(sectorAddress, length);
            int    bps    = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];

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

        public bool? VerifySectors(ulong           sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas)
        {
            unknownLbas = new List<ulong>();
            failingLbas = new List<ulong>();
            if(isHdd) return null;

            byte[] buffer = ReadSectorsLong(sectorAddress, length, track);
            int    bps    = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];

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

        public bool? VerifyMediaImage()
        {
            byte[] calculated;
            if(mapVersion >= 3)
            {
                Sha1Context sha1Ctx = new Sha1Context();
                for(uint i = 0; i < totalHunks; i++) sha1Ctx.Update(GetHunk(i));

                calculated = sha1Ctx.Final();
            }
            else
            {
                Md5Context md5Ctx = new Md5Context();
                for(uint i = 0; i < totalHunks; i++) md5Ctx.Update(GetHunk(i));

                calculated = md5Ctx.Final();
            }

            return expectedChecksum.SequenceEqual(calculated);
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            Track track = new Track();
            uint  sectorSize;

            if(!sectorCache.TryGetValue(sectorAddress, out byte[] sector))
            {
                if(isHdd) sectorSize = imageInfo.SectorSize;
                else
                {
                    track      = GetTrack(sectorAddress);
                    sectorSize = (uint)track.TrackRawBytesPerSector;
                }

                ulong hunkNo = sectorAddress              / sectorsPerHunk;
                ulong secOff = sectorAddress * sectorSize % (sectorsPerHunk * sectorSize);

                byte[] hunk = GetHunk(hunkNo);

                sector = new byte[imageInfo.SectorSize];
                Array.Copy(hunk, (int)secOff, sector, 0, sector.Length);

                if(sectorCache.Count >= maxSectorCache) sectorCache.Clear();

                sectorCache.Add(sectorAddress, sector);
            }

            if(isHdd) return sector;

            uint sectorOffset;

            switch(track.TrackType)
            {
                case TrackType.CdMode1:
                case TrackType.CdMode2Form1:
                {
                    if(track.TrackRawBytesPerSector == 2352)
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
                case TrackType.CdMode2Form2:
                {
                    if(track.TrackRawBytesPerSector == 2352)
                    {
                        sectorOffset = 16;
                        sectorSize   = 2324;
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
                    if(track.TrackRawBytesPerSector == 2352)
                    {
                        sectorOffset = 16;
                        sectorSize   = 2336;
                    }
                    else
                    {
                        sectorOffset = 0;
                        sectorSize   = 2336;
                    }

                    break;
                }
                case TrackType.Audio:
                {
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sectorSize];

            if(track.TrackType == TrackType.Audio && swapAudio)
                for(int i = 0; i < 2352; i += 2)
                {
                    buffer[i + 1] = sector[i];
                    buffer[i]     = sector[i + 1];
                }
            else Array.Copy(sector, sectorOffset, buffer, 0, sectorSize);

            return buffer;
        }

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            if(isHdd) throw new FeatureNotPresentImageException("Hard disk images do not have sector tags");

            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            Track track = new Track();

            uint sectorSize;

            if(!sectorCache.TryGetValue(sectorAddress, out byte[] sector))
            {
                track      = GetTrack(sectorAddress);
                sectorSize = (uint)track.TrackRawBytesPerSector;

                ulong hunkNo = sectorAddress              / sectorsPerHunk;
                ulong secOff = sectorAddress * sectorSize % (sectorsPerHunk * sectorSize);

                byte[] hunk = GetHunk(hunkNo);

                sector = new byte[imageInfo.SectorSize];
                Array.Copy(hunk, (int)secOff, sector, 0, sector.Length);

                if(sectorCache.Count >= maxSectorCache) sectorCache.Clear();

                sectorCache.Add(sectorAddress, sector);
            }

            if(isHdd) return sector;

            uint sectorOffset;

            if(tag == SectorTagType.CdSectorSubchannel)
                switch(track.TrackSubchannelType)
                {
                    case TrackSubchannelType.None:
                        throw new FeatureNotPresentImageException("Requested sector does not contain subchannel");
                    case TrackSubchannelType.RawInterleaved:
                        sectorOffset = (uint)track.TrackRawBytesPerSector;
                        sectorSize   = 96;
                        break;
                    default:
                        throw new
                            FeatureSupportedButNotImplementedImageException($"Unsupported subchannel type {track.TrackSubchannelType}");
                }
            else
                switch(track.TrackType)
                {
                    case TrackType.CdMode1:
                    case TrackType.CdMode2Form1:
                    {
                        if(track.TrackRawBytesPerSector == 2352)
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
                                    throw new ArgumentException("Unsupported tag requested for this track",
                                                                nameof(tag));
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
                                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                            }
                        else throw new FeatureNotPresentImageException("Requested sector does not contain tags");

                        break;
                    }
                    case TrackType.CdMode2Form2:
                    {
                        if(track.TrackRawBytesPerSector == 2352)
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
                                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                            }
                        else
                            switch(tag)
                            {
                                case SectorTagType.CdSectorSync:
                                case SectorTagType.CdSectorHeader:
                                case SectorTagType.CdSectorSubchannel:
                                case SectorTagType.CdSectorEcc:
                                case SectorTagType.CdSectorEccP:
                                case SectorTagType.CdSectorEccQ:
                                    throw new ArgumentException("Unsupported tag requested for this track",
                                                                nameof(tag));
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
                                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                            }

                        break;
                    }
                    case TrackType.CdMode2Formless:
                    {
                        if(track.TrackRawBytesPerSector == 2352)
                            switch(tag)
                            {
                                case SectorTagType.CdSectorSync:
                                case SectorTagType.CdSectorHeader:
                                case SectorTagType.CdSectorEcc:
                                case SectorTagType.CdSectorEccP:
                                case SectorTagType.CdSectorEccQ:
                                    throw new ArgumentException("Unsupported tag requested for this track",
                                                                nameof(tag));
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
                                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                            }
                        else throw new FeatureNotPresentImageException("Requested sector does not contain tags");

                        break;
                    }
                    case TrackType.Audio:
                        throw new FeatureNotPresentImageException("Requested sector does not contain tags");
                    default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
                }

            byte[] buffer = new byte[sectorSize];

            if(track.TrackType == TrackType.Audio && swapAudio)
                for(int i = 0; i < 2352; i += 2)
                {
                    buffer[i + 1] = sector[i];
                    buffer[i]     = sector[i + 1];
                }
            else Array.Copy(sector, sectorOffset, buffer, 0, sectorSize);

            if(track.TrackType == TrackType.Audio && swapAudio)
                for(int i = 0; i < 2352; i += 2)
                {
                    buffer[i + 1] = sector[i];
                    buffer[i]     = sector[i + 1];
                }
            else Array.Copy(sector, sectorOffset, buffer, 0, sectorSize);

            return buffer;
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({sectorAddress + length}) than available ({imageInfo.Sectors})");

            MemoryStream ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({sectorAddress + length}) than available ({imageInfo.Sectors})");

            MemoryStream ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSectorTag(sectorAddress + i, tag);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }

        public byte[] ReadSectorLong(ulong sectorAddress)
        {
            if(isHdd) return ReadSector(sectorAddress);

            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            Track track = new Track();

            if(!sectorCache.TryGetValue(sectorAddress, out byte[] sector))
            {
                track = GetTrack(sectorAddress);
                uint sectorSize = (uint)track.TrackRawBytesPerSector;

                ulong hunkNo = sectorAddress              / sectorsPerHunk;
                ulong secOff = sectorAddress * sectorSize % (sectorsPerHunk * sectorSize);

                byte[] hunk = GetHunk(hunkNo);

                sector = new byte[imageInfo.SectorSize];
                Array.Copy(hunk, (int)secOff, sector, 0, sector.Length);

                if(sectorCache.Count >= maxSectorCache) sectorCache.Clear();

                sectorCache.Add(sectorAddress, sector);
            }

            byte[] buffer = new byte[track.TrackRawBytesPerSector];

            if(track.TrackType == TrackType.Audio && swapAudio)
                for(int i = 0; i < 2352; i += 2)
                {
                    buffer[i + 1] = sector[i];
                    buffer[i]     = sector[i + 1];
                }
            else Array.Copy(sector, 0, buffer, 0, track.TrackRawBytesPerSector);

            return buffer;
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({sectorAddress + length}) than available ({imageInfo.Sectors})");

            MemoryStream ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSectorLong(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }

        public byte[] ReadDiskTag(MediaTagType tag)
        {
            if(imageInfo.ReadableMediaTags.Contains(MediaTagType.ATA_IDENTIFY)) return identify;

            if(imageInfo.ReadableMediaTags.Contains(MediaTagType.PCMCIA_CIS)) return cis;

            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public List<Track> GetSessionTracks(Session session)
        {
            if(isHdd)
                throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

            return GetSessionTracks(session.SessionSequence);
        }

        public List<Track> GetSessionTracks(ushort session)
        {
            if(isHdd)
                throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

            return tracks.Values.Where(track => track.TrackSession == session).ToList();
        }

        public byte[] ReadSector(ulong sectorAddress, uint track)
        {
            if(isHdd)
                throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

            return ReadSector(GetAbsoluteSector(sectorAddress, track));
        }

        public byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            if(isHdd)
                throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

            return ReadSectorTag(GetAbsoluteSector(sectorAddress, track), tag);
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            if(isHdd)
                throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

            return ReadSectors(GetAbsoluteSector(sectorAddress, track), length);
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            if(isHdd)
                throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

            return ReadSectorsTag(GetAbsoluteSector(sectorAddress, track), length, tag);
        }

        public byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            if(isHdd)
                throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

            return ReadSectorLong(GetAbsoluteSector(sectorAddress, track));
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            if(isHdd)
                throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

            return ReadSectorLong(GetAbsoluteSector(sectorAddress, track), length);
        }
    }
}