// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CHD.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages MAME Compressed Hunks of Data disk images.
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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using DiscImageChef.Checksums;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Filters;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;

namespace DiscImageChef.DiscImages
{
    // TODO: Implement PCMCIA support
    public class Chd : ImagePlugin
    {
        #region Internal Structures
        enum ChdCompression : uint
        {
            None = 0,
            Zlib = 1,
            ZlibPlus = 2,
            Av = 3
        }

        enum ChdFlags : uint
        {
            HasParent = 1,
            Writable = 2
        }

        enum Chdv3EntryFlags : byte
        {
            /// <summary>Invalid</summary>
            Invalid = 0,
            /// <summary>Compressed with primary codec</summary>
            Compressed = 1,
            /// <summary>Uncompressed</summary>
            Uncompressed = 2,
            /// <summary>Use offset as data</summary>
            Mini = 3,
            /// <summary>Same as another hunk in file</summary>
            SelfHunk = 4,
            /// <summary>Same as another hunk in parent</summary>
            ParentHunk = 5,
            /// <summary>Compressed with secondary codec (FLAC)</summary>
            SecondCompressed = 6
        }

        enum ChdOldTrackType : uint
        {
            Mode1 = 0,
            Mode1Raw,
            Mode2,
            Mode2Form1,
            Mode2Form2,
            Mode2FormMix,
            Mode2Raw,
            Audio
        }

        enum ChdOldSubType : uint
        {
            Cooked = 0,
            Raw,
            None
        }

        // Hunks are represented in a 64 bit integer with 44 bit as offset, 20 bits as length
        // Sectors are fixed at 512 bytes/sector
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ChdHeaderV1
        {
            /// <summary>
            /// Magic identifier, 'MComprHD'
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] tag;
            /// <summary>
            /// Length of header
            /// </summary>
            public uint length;
            /// <summary>
            /// Image format version
            /// </summary>
            public uint version;
            /// <summary>
            /// Image flags, <see cref="ChdFlags"/>
            /// </summary>
            public uint flags;
            /// <summary>
            /// Compression algorithm, <see cref="ChdCompression"/>
            /// </summary>
            public uint compression;
            /// <summary>
            /// Sectors per hunk
            /// </summary>
            public uint hunksize;
            /// <summary>
            /// Total # of hunk in image
            /// </summary>
            public uint totalhunks;
            /// <summary>
            /// Cylinders on disk
            /// </summary>
            public uint cylinders;
            /// <summary>
            /// Heads per cylinder
            /// </summary>
            public uint heads;
            /// <summary>
            /// Sectors per track
            /// </summary>
            public uint sectors;
            /// <summary>
            /// MD5 of raw data
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] md5;
            /// <summary>
            /// MD5 of parent file
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] parentmd5;
        }

        // Hunks are represented in a 64 bit integer with 44 bit as offset, 20 bits as length
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ChdHeaderV2
        {
            /// <summary>
            /// Magic identifier, 'MComprHD'
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] tag;
            /// <summary>
            /// Length of header
            /// </summary>
            public uint length;
            /// <summary>
            /// Image format version
            /// </summary>
            public uint version;
            /// <summary>
            /// Image flags, <see cref="ChdFlags"/>
            /// </summary>
            public uint flags;
            /// <summary>
            /// Compression algorithm, <see cref="ChdCompression"/>
            /// </summary>
            public uint compression;
            /// <summary>
            /// Sectors per hunk
            /// </summary>
            public uint hunksize;
            /// <summary>
            /// Total # of hunk in image
            /// </summary>
            public uint totalhunks;
            /// <summary>
            /// Cylinders on disk
            /// </summary>
            public uint cylinders;
            /// <summary>
            /// Heads per cylinder
            /// </summary>
            public uint heads;
            /// <summary>
            /// Sectors per track
            /// </summary>
            public uint sectors;
            /// <summary>
            /// MD5 of raw data
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] md5;
            /// <summary>
            /// MD5 of parent file
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] parentmd5;
            /// <summary>
            /// Bytes per sector
            /// </summary>
            public uint seclen;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ChdHeaderV3
        {
            /// <summary>
            /// Magic identifier, 'MComprHD'
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] tag;
            /// <summary>
            /// Length of header
            /// </summary>
            public uint length;
            /// <summary>
            /// Image format version
            /// </summary>
            public uint version;
            /// <summary>
            /// Image flags, <see cref="ChdFlags"/>
            /// </summary>
            public uint flags;
            /// <summary>
            /// Compression algorithm, <see cref="ChdCompression"/>
            /// </summary>
            public uint compression;
            /// <summary>
            /// Total # of hunk in image
            /// </summary>
            public uint totalhunks;
            /// <summary>
            /// Total bytes in image
            /// </summary>
            public ulong logicalbytes;
            /// <summary>
            /// Offset to first metadata blob
            /// </summary>
            public ulong metaoffset;
            /// <summary>
            /// MD5 of raw data
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] md5;
            /// <summary>
            /// MD5 of parent file
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] parentmd5;
            /// <summary>
            /// Bytes per hunk
            /// </summary>
            public uint hunkbytes;
            /// <summary>
            /// SHA1 of raw data
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] public byte[] sha1;
            /// <summary>
            /// SHA1 of parent file
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] public byte[] parentsha1;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ChdMapV3Entry
        {
            /// <summary>
            /// Offset to hunk from start of image
            /// </summary>
            public ulong offset;
            /// <summary>
            /// CRC32 of uncompressed hunk
            /// </summary>
            public uint crc;
            /// <summary>
            /// Lower 16 bits of length
            /// </summary>
            public ushort lengthLsb;
            /// <summary>
            /// Upper 8 bits of length
            /// </summary>
            public byte length;
            /// <summary>
            /// Hunk flags
            /// </summary>
            public byte flags;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ChdTrackOld
        {
            public uint type;
            public uint subType;
            public uint dataSize;
            public uint subSize;
            public uint frames;
            public uint extraFrames;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ChdHeaderV4
        {
            /// <summary>
            /// Magic identifier, 'MComprHD'
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] tag;
            /// <summary>
            /// Length of header
            /// </summary>
            public uint length;
            /// <summary>
            /// Image format version
            /// </summary>
            public uint version;
            /// <summary>
            /// Image flags, <see cref="ChdFlags"/>
            /// </summary>
            public uint flags;
            /// <summary>
            /// Compression algorithm, <see cref="ChdCompression"/>
            /// </summary>
            public uint compression;
            /// <summary>
            /// Total # of hunk in image
            /// </summary>
            public uint totalhunks;
            /// <summary>
            /// Total bytes in image
            /// </summary>
            public ulong logicalbytes;
            /// <summary>
            /// Offset to first metadata blob
            /// </summary>
            public ulong metaoffset;
            /// <summary>
            /// Bytes per hunk
            /// </summary>
            public uint hunkbytes;
            /// <summary>
            /// SHA1 of raw+meta data
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] public byte[] sha1;
            /// <summary>
            /// SHA1 of parent file
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] public byte[] parentsha1;
            /// <summary>
            /// SHA1 of raw data
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] public byte[] rawsha1;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ChdHeaderV5
        {
            /// <summary>
            /// Magic identifier, 'MComprHD'
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] tag;
            /// <summary>
            /// Length of header
            /// </summary>
            public uint length;
            /// <summary>
            /// Image format version
            /// </summary>
            public uint version;
            /// <summary>
            /// Compressor 0
            /// </summary>
            public uint compressor0;
            /// <summary>
            /// Compressor 1
            /// </summary>
            public uint compressor1;
            /// <summary>
            /// Compressor 2
            /// </summary>
            public uint compressor2;
            /// <summary>
            /// Compressor 3
            /// </summary>
            public uint compressor3;
            /// <summary>
            /// Total bytes in image
            /// </summary>
            public ulong logicalbytes;
            /// <summary>
            /// Offset to hunk map
            /// </summary>
            public ulong mapoffset;
            /// <summary>
            /// Offset to first metadata blob
            /// </summary>
            public ulong metaoffset;
            /// <summary>
            /// Bytes per hunk
            /// </summary>
            public uint hunkbytes;
            /// <summary>
            /// Bytes per unit within hunk
            /// </summary>
            public uint unitbytes;
            /// <summary>
            /// SHA1 of raw data
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] public byte[] rawsha1;
            /// <summary>
            /// SHA1 of raw+meta data
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] public byte[] sha1;
            /// <summary>
            /// SHA1 of parent file
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] public byte[] parentsha1;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ChdCompressedMapHeaderV5
        {
            /// <summary>
            /// Length of compressed map
            /// </summary>
            public uint length;
            /// <summary>
            /// Offset of first block (48 bits) and CRC16 of map (16 bits)
            /// </summary>
            public ulong startAndCrc;
            /// <summary>
            /// Bits used to encode compressed length on map entry
            /// </summary>
            public byte bitsUsedToEncodeCompLength;
            /// <summary>
            /// Bits used to encode self-refs
            /// </summary>
            public byte bitsUsedToEncodeSelfRefs;
            /// <summary>
            /// Bits used to encode parent unit refs
            /// </summary>
            public byte bitsUsedToEncodeParentUnits;
            public byte reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ChdMapV5Entry
        {
            /// <summary>
            /// Compression (8 bits) and length (24 bits)
            /// </summary>
            public uint compAndLength;
            /// <summary>
            /// Offset (48 bits) and CRC (16 bits)
            /// </summary>
            public ulong offsetAndCrc;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ChdMetadataHeader
        {
            public uint tag;
            public uint flagsAndLength;
            public ulong next;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HunkSector
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] public ulong[] hunkEntry;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HunkSectorSmall
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public uint[] hunkEntry;
        }
        #endregion

        #region Internal Constants
        /// <summary>"MComprHD"</summary>
        readonly byte[] chdTag = {0x4D, 0x43, 0x6F, 0x6D, 0x70, 0x72, 0x48, 0x44};
        /// <summary>"GDDD"</summary>
        const uint HARD_DISK_METADATA = 0x47444444;
        /// <summary>"IDNT"</summary>
        const uint HARD_DISK_IDENT_METADATA = 0x49444E54;
        /// <summary>"KEY "</summary>
        const uint HARD_DISK_KEY_METADATA = 0x4B455920;
        /// <summary>"CIS "</summary>
        const uint PCMCIA_CIS_METADATA = 0x43495320;
        /// <summary>"CHCD"</summary>
        const uint CDROM_OLD_METADATA = 0x43484344;
        /// <summary>"CHTR"</summary>
        const uint CDROM_TRACK_METADATA = 0x43485452;
        /// <summary>"CHT2"</summary>
        const uint CDROM_TRACK_METADATA2 = 0x43485432;
        /// <summary>"CHGT"</summary>
        const uint GDROM_OLD_METADATA = 0x43484754;
        /// <summary>"CHGD"</summary>
        const uint GDROM_METADATA = 0x43484744;
        /// <summary>"AVAV"</summary>
        const uint AV_METADATA = 0x41564156;
        /// <summary>"AVLD"</summary>
        const uint AV_LASER_DISC_METADATA = 0x41564C44;

        const string HARD_DISK_METADATA_REGEX =
            "CYLS:(?<cylinders>\\d+),HEADS:(?<heads>\\d+),SECS:(?<sectors>\\d+),BPS:(?<bps>\\d+)";
        const string CDROM_METADATA_REGEX =
            "TRACK:(?<track>\\d+) TYPE:(?<track_type>\\S+) SUBTYPE:(?<sub_type>\\S+) FRAMES:(?<frames>\\d+)";
        const string CDROM_METADATA2_REGEX =
                "TRACK:(?<track>\\d+) TYPE:(?<track_type>\\S+) SUBTYPE:(?<sub_type>\\S+) FRAMES:(?<frames>\\d+) PREGAP:(?<pregap>\\d+) PGTYPE:(?<pgtype>\\S+) PGSUB:(?<pgsub>\\S+) POSTGAP:(?<postgap>\\d+)"
            ;
        const string GDROM_METADATA_REGEX =
                "TRACK:(?<track>\\d+) TYPE:(?<track_type>\\S+) SUBTYPE:(?<sub_type>\\S+) FRAMES:(?<frames>\\d+) PAD:(?<pad>\\d+) PREGAP:(?<pregap>\\d+) PGTYPE:(?<pgtype>\\S+) PGSUB:(?<pgsub>\\S+) POSTGAP:(?<postgap>\\d+)"
            ;

        const string TRACK_TYPE_MODE1 = "MODE1";
        const string TRACK_TYPE_MODE1_2K = "MODE1/2048";
        const string TRACK_TYPE_MODE1_RAW = "MODE1_RAW";
        const string TRACK_TYPE_MODE1_RAW_2K = "MODE1/2352";
        const string TRACK_TYPE_MODE2 = "MODE2";
        const string TRACK_TYPE_MODE2_2K = "MODE2/2336";
        const string TRACK_TYPE_MODE2_F1 = "MODE2_FORM1";
        const string TRACK_TYPE_MODE2_F1_2K = "MODE2/2048";
        const string TRACK_TYPE_MODE2_F2 = "MODE2_FORM2";
        const string TRACK_TYPE_MODE2_F2_2K = "MODE2/2324";
        const string TRACK_TYPE_MODE2_FM = "MODE2_FORM_MIX";
        const string TRACK_TYPE_MODE2_RAW = "MODE2_RAW";
        const string TRACK_TYPE_MODE2_RAW_2K = "MODE2/2352";
        const string TRACK_TYPE_AUDIO = "AUDIO";

        const string SUB_TYPE_COOKED = "RW";
        const string SUB_TYPE_RAW = "RW_RAW";
        const string SUB_TYPE_NONE = "NONE";
        #endregion

        #region Internal variables
        ulong[] hunkTable;
        uint[] hunkTableSmall;
        uint hdrCompression;
        uint hdrCompression1;
        uint hdrCompression2;
        uint hdrCompression3;
        Stream imageStream;
        uint sectorsPerHunk;
        byte[] hunkMap;
        uint mapVersion;
        uint bytesPerHunk;
        uint totalHunks;
        byte[] expectedChecksum;
        bool isCdrom;
        bool isHdd;
        bool isGdrom;
        bool swapAudio;

        const int MAX_CACHE_SIZE = 16777216;
        int maxBlockCache;
        int maxSectorCache;

        Dictionary<ulong, byte[]> sectorCache;
        Dictionary<ulong, byte[]> hunkCache;

        Dictionary<uint, Track> tracks;
        List<Partition> partitions;
        Dictionary<ulong, uint> offsetmap;

        byte[] identify;
        byte[] cis;
        #endregion

        public Chd()
        {
            Name = "MAME Compressed Hunks of Data";
            PluginUuid = new Guid("0D50233A-08BD-47D4-988B-27EAA0358597");
            ImageInfo = new ImageInfo();
            ImageInfo.ReadableSectorTags = new List<SectorTagType>();
            ImageInfo.ReadableMediaTags = new List<MediaTagType>();
            ImageInfo.ImageHasPartitions = false;
            ImageInfo.ImageHasSessions = false;
            ImageInfo.ImageApplication = "MAME";
            ImageInfo.ImageCreator = null;
            ImageInfo.ImageComments = null;
            ImageInfo.MediaManufacturer = null;
            ImageInfo.MediaModel = null;
            ImageInfo.MediaSerialNumber = null;
            ImageInfo.MediaBarcode = null;
            ImageInfo.MediaPartNumber = null;
            ImageInfo.MediaSequence = 0;
            ImageInfo.LastMediaSequence = 0;
            ImageInfo.DriveManufacturer = null;
            ImageInfo.DriveModel = null;
            ImageInfo.DriveSerialNumber = null;
            ImageInfo.DriveFirmwareRevision = null;
        }

        public override bool IdentifyImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            byte[] magic = new byte[8];
            stream.Read(magic, 0, 8);

            return chdTag.SequenceEqual(magic);
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            byte[] buffer;
            byte[] magic = new byte[8];
            stream.Read(magic, 0, 8);
            if(!chdTag.SequenceEqual(magic)) return false;
            // Read length
            buffer = new byte[4];
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

                    DicConsole.DebugWriteLine("CHD plugin", "hdrV1.tag = \"{0}\"", Encoding.ASCII.GetString(hdrV1.tag));
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV1.length = {0} bytes", hdrV1.length);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV1.version = {0}", hdrV1.version);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV1.flags = {0}", (ChdFlags)hdrV1.flags);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV1.compression = {0}",
                                              (ChdCompression)hdrV1.compression);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV1.hunksize = {0}", hdrV1.hunksize);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV1.totalhunks = {0}", hdrV1.totalhunks);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV1.cylinders = {0}", hdrV1.cylinders);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV1.heads = {0}", hdrV1.heads);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV1.sectors = {0}", hdrV1.sectors);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV1.md5 = {0}", ArrayHelpers.ByteArrayToHex(hdrV1.md5));
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV1.parentmd5 = {0}",
                                              ArrayHelpers.ArrayIsNullOrEmpty(hdrV1.parentmd5)
                                                  ? "null"
                                                  : ArrayHelpers.ByteArrayToHex(hdrV1.parentmd5));

                    DicConsole.DebugWriteLine("CHD plugin", "Reading Hunk map.");
                    DateTime start = DateTime.UtcNow;

                    hunkTable = new ulong[hdrV1.totalhunks];

                    uint hunkSectorCount = (uint)Math.Ceiling((double)hdrV1.totalhunks * 8 / 512);

                    byte[] hunkSectorBytes = new byte[512];
                    HunkSector hunkSector;

                    for(int i = 0; i < hunkSectorCount; i++)
                    {
                        stream.Read(hunkSectorBytes, 0, 512);
                        // This does the big-endian trick but reverses the order of elements also
                        Array.Reverse(hunkSectorBytes);
                        GCHandle handle = GCHandle.Alloc(hunkSectorBytes, GCHandleType.Pinned);
                        hunkSector =
                            (HunkSector)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(HunkSector));
                        handle.Free();
                        // This restores the order of elements
                        Array.Reverse(hunkSector.hunkEntry);
                        if(hunkTable.Length >= i * 512 / 8 + 512 / 8)
                            Array.Copy(hunkSector.hunkEntry, 0, hunkTable, i * 512 / 8, 512 / 8);
                        else
                            Array.Copy(hunkSector.hunkEntry, 0, hunkTable, i * 512 / 8,
                                       hunkTable.Length - i * 512 / 8);
                    }

                    DateTime end = DateTime.UtcNow;
                    System.Console.WriteLine("Took {0} seconds", (end - start).TotalSeconds);

                    ImageInfo.MediaType = MediaType.GENERIC_HDD;
                    ImageInfo.Sectors = hdrV1.hunksize * hdrV1.totalhunks;
                    ImageInfo.XmlMediaType = XmlMediaType.BlockMedia;
                    ImageInfo.SectorSize = 512;
                    ImageInfo.ImageVersion = "1";
                    ImageInfo.ImageSize = ImageInfo.SectorSize * hdrV1.hunksize * hdrV1.totalhunks;

                    totalHunks = hdrV1.totalhunks;
                    sectorsPerHunk = hdrV1.hunksize;
                    hdrCompression = hdrV1.compression;
                    mapVersion = 1;
                    isHdd = true;

                    ImageInfo.Cylinders = hdrV1.cylinders;
                    ImageInfo.Heads = hdrV1.heads;
                    ImageInfo.SectorsPerTrack = hdrV1.sectors;

                    break;
                }
                case 2:
                {
                    ChdHeaderV2 hdrV2 = BigEndianMarshal.ByteArrayToStructureBigEndian<ChdHeaderV2>(buffer);

                    DicConsole.DebugWriteLine("CHD plugin", "hdrV2.tag = \"{0}\"", Encoding.ASCII.GetString(hdrV2.tag));
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV2.length = {0} bytes", hdrV2.length);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV2.version = {0}", hdrV2.version);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV2.flags = {0}", (ChdFlags)hdrV2.flags);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV2.compression = {0}",
                                              (ChdCompression)hdrV2.compression);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV2.hunksize = {0}", hdrV2.hunksize);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV2.totalhunks = {0}", hdrV2.totalhunks);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV2.cylinders = {0}", hdrV2.cylinders);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV2.heads = {0}", hdrV2.heads);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV2.sectors = {0}", hdrV2.sectors);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV2.md5 = {0}", ArrayHelpers.ByteArrayToHex(hdrV2.md5));
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
                    HunkSector hunkSector;

                    for(int i = 0; i < hunkSectorCount; i++)
                    {
                        stream.Read(hunkSectorBytes, 0, 512);
                        // This does the big-endian trick but reverses the order of elements also
                        Array.Reverse(hunkSectorBytes);
                        GCHandle handle = GCHandle.Alloc(hunkSectorBytes, GCHandleType.Pinned);
                        hunkSector =
                            (HunkSector)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(HunkSector));
                        handle.Free();
                        // This restores the order of elements
                        Array.Reverse(hunkSector.hunkEntry);
                        if(hunkTable.Length >= i * 512 / 8 + 512 / 8)
                            Array.Copy(hunkSector.hunkEntry, 0, hunkTable, i * 512 / 8, 512 / 8);
                        else
                            Array.Copy(hunkSector.hunkEntry, 0, hunkTable, i * 512 / 8,
                                       hunkTable.Length - i * 512 / 8);
                    }

                    DateTime end = DateTime.UtcNow;
                    System.Console.WriteLine("Took {0} seconds", (end - start).TotalSeconds);

                    ImageInfo.MediaType = MediaType.GENERIC_HDD;
                    ImageInfo.Sectors = hdrV2.hunksize * hdrV2.totalhunks;
                    ImageInfo.XmlMediaType = XmlMediaType.BlockMedia;
                    ImageInfo.SectorSize = hdrV2.seclen;
                    ImageInfo.ImageVersion = "2";
                    ImageInfo.ImageSize = ImageInfo.SectorSize * hdrV2.hunksize * hdrV2.totalhunks;

                    totalHunks = hdrV2.totalhunks;
                    sectorsPerHunk = hdrV2.hunksize;
                    hdrCompression = hdrV2.compression;
                    mapVersion = 1;
                    isHdd = true;

                    ImageInfo.Cylinders = hdrV2.cylinders;
                    ImageInfo.Heads = hdrV2.heads;
                    ImageInfo.SectorsPerTrack = hdrV2.sectors;

                    break;
                }
                case 3:
                {
                    ChdHeaderV3 hdrV3 = BigEndianMarshal.ByteArrayToStructureBigEndian<ChdHeaderV3>(buffer);

                    DicConsole.DebugWriteLine("CHD plugin", "hdrV3.tag = \"{0}\"", Encoding.ASCII.GetString(hdrV3.tag));
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV3.length = {0} bytes", hdrV3.length);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV3.version = {0}", hdrV3.version);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV3.flags = {0}", (ChdFlags)hdrV3.flags);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV3.compression = {0}",
                                              (ChdCompression)hdrV3.compression);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV3.totalhunks = {0}", hdrV3.totalhunks);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV3.logicalbytes = {0}", hdrV3.logicalbytes);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV3.metaoffset = {0}", hdrV3.metaoffset);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV3.md5 = {0}", ArrayHelpers.ByteArrayToHex(hdrV3.md5));
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
                    System.Console.WriteLine("Took {0} seconds", (end - start).TotalSeconds);

                    nextMetaOff = hdrV3.metaoffset;

                    ImageInfo.ImageSize = hdrV3.logicalbytes;
                    ImageInfo.ImageVersion = "3";

                    totalHunks = hdrV3.totalhunks;
                    bytesPerHunk = hdrV3.hunkbytes;
                    hdrCompression = hdrV3.compression;
                    mapVersion = 3;

                    break;
                }
                case 4:
                {
                    ChdHeaderV4 hdrV4 = BigEndianMarshal.ByteArrayToStructureBigEndian<ChdHeaderV4>(buffer);

                    DicConsole.DebugWriteLine("CHD plugin", "hdrV4.tag = \"{0}\"", Encoding.ASCII.GetString(hdrV4.tag));
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV4.length = {0} bytes", hdrV4.length);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV4.version = {0}", hdrV4.version);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV4.flags = {0}", (ChdFlags)hdrV4.flags);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV4.compression = {0}",
                                              (ChdCompression)hdrV4.compression);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV4.totalhunks = {0}", hdrV4.totalhunks);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV4.logicalbytes = {0}", hdrV4.logicalbytes);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV4.metaoffset = {0}", hdrV4.metaoffset);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV4.hunkbytes = {0}", hdrV4.hunkbytes);
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
                    System.Console.WriteLine("Took {0} seconds", (end - start).TotalSeconds);

                    nextMetaOff = hdrV4.metaoffset;

                    ImageInfo.ImageSize = hdrV4.logicalbytes;
                    ImageInfo.ImageVersion = "4";

                    totalHunks = hdrV4.totalhunks;
                    bytesPerHunk = hdrV4.hunkbytes;
                    hdrCompression = hdrV4.compression;
                    mapVersion = 3;

                    break;
                }
                case 5:
                {
                    ChdHeaderV5 hdrV5 = BigEndianMarshal.ByteArrayToStructureBigEndian<ChdHeaderV5>(buffer);

                    DicConsole.DebugWriteLine("CHD plugin", "hdrV5.tag = \"{0}\"", Encoding.ASCII.GetString(hdrV5.tag));
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV5.length = {0} bytes", hdrV5.length);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV5.version = {0}", hdrV5.version);
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
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV5.mapoffset = {0}", hdrV5.mapoffset);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV5.metaoffset = {0}", hdrV5.metaoffset);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV5.hunkbytes = {0}", hdrV5.hunkbytes);
                    DicConsole.DebugWriteLine("CHD plugin", "hdrV5.unitbytes = {0}", hdrV5.unitbytes);
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
                        HunkSectorSmall hunkSector;

                        stream.Seek((long)hdrV5.mapoffset, SeekOrigin.Begin);

                        for(int i = 0; i < hunkSectorCount; i++)
                        {
                            stream.Read(hunkSectorBytes, 0, 512);
                            // This does the big-endian trick but reverses the order of elements also
                            Array.Reverse(hunkSectorBytes);
                            GCHandle handle = GCHandle.Alloc(hunkSectorBytes, GCHandleType.Pinned);
                            hunkSector =
                                (HunkSectorSmall)Marshal.PtrToStructure(handle.AddrOfPinnedObject(),
                                                                        typeof(HunkSectorSmall));
                            handle.Free();
                            // This restores the order of elements
                            Array.Reverse(hunkSector.hunkEntry);
                            if(hunkTableSmall.Length >= i * 512 / 4 + 512 / 4)
                                Array.Copy(hunkSector.hunkEntry, 0, hunkTableSmall, i * 512 / 4, 512 / 4);
                            else
                                Array.Copy(hunkSector.hunkEntry, 0, hunkTableSmall, i * 512 / 4,
                                           hunkTableSmall.Length - i * 512 / 4);
                        }

                        DateTime end = DateTime.UtcNow;
                        System.Console.WriteLine("Took {0} seconds", (end - start).TotalSeconds);
                    }
                    else throw new ImageNotSupportedException("Cannot read compressed CHD version 5");

                    nextMetaOff = hdrV5.metaoffset;

                    ImageInfo.ImageSize = hdrV5.logicalbytes;
                    ImageInfo.ImageVersion = "5";

                    totalHunks = (uint)(hdrV5.logicalbytes / hdrV5.hunkbytes);
                    bytesPerHunk = hdrV5.hunkbytes;
                    hdrCompression = hdrV5.compressor0;
                    hdrCompression1 = hdrV5.compressor1;
                    hdrCompression2 = hdrV5.compressor2;
                    hdrCompression3 = hdrV5.compressor3;
                    mapVersion = 5;

                    break;
                }
                default: throw new ImageNotSupportedException(string.Format("Unsupported CHD version {0}", version));
            }

            if(mapVersion >= 3)
            {
                byte[] meta;
                isCdrom = false;
                isHdd = false;
                isGdrom = false;
                swapAudio = false;
                tracks = new Dictionary<uint, Track>();

                DicConsole.DebugWriteLine("CHD plugin", "Reading metadata.");

                ulong currentSector = 0;
                uint currentTrack = 1;

                while(nextMetaOff > 0)
                {
                    byte[] hdrBytes = new byte[16];
                    stream.Seek((long)nextMetaOff, SeekOrigin.Begin);
                    stream.Read(hdrBytes, 0, hdrBytes.Length);
                    ChdMetadataHeader header =
                        BigEndianMarshal.ByteArrayToStructureBigEndian<ChdMetadataHeader>(hdrBytes);
                    meta = new byte[header.flagsAndLength & 0xFFFFFF];
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

                            string gddd = StringHandlers.CToString(meta);
                            Regex gdddRegEx = new Regex(HARD_DISK_METADATA_REGEX);
                            Match gdddMatch = gdddRegEx.Match(gddd);
                            if(gdddMatch.Success)
                            {
                                isHdd = true;
                                ImageInfo.SectorSize = uint.Parse(gdddMatch.Groups["bps"].Value);
                                ImageInfo.Cylinders = uint.Parse(gdddMatch.Groups["cylinders"].Value);
                                ImageInfo.Heads = uint.Parse(gdddMatch.Groups["heads"].Value);
                                ImageInfo.SectorsPerTrack = uint.Parse(gdddMatch.Groups["sectors"].Value);
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

                            uint _tracks = BigEndianBitConverter.ToUInt32(meta, 0);

                            // Byteswapped
                            if(_tracks > 99)
                            {
                                BigEndianBitConverter.IsLittleEndian = !BitConverter.IsLittleEndian;
                                _tracks = BigEndianBitConverter.ToUInt32(meta, 0);
                            }

                            currentSector = 0;

                            for(uint i = 0; i < _tracks; i++)
                            {
                                ChdTrackOld _trk = new ChdTrackOld();
                                _trk.type = BigEndianBitConverter.ToUInt32(meta, (int)(4 + i * 24 + 0));
                                _trk.subType = BigEndianBitConverter.ToUInt32(meta, (int)(4 + i * 24 + 4));
                                _trk.dataSize = BigEndianBitConverter.ToUInt32(meta, (int)(4 + i * 24 + 8));
                                _trk.subSize = BigEndianBitConverter.ToUInt32(meta, (int)(4 + i * 24 + 12));
                                _trk.frames = BigEndianBitConverter.ToUInt32(meta, (int)(4 + i * 24 + 16));
                                _trk.extraFrames = BigEndianBitConverter.ToUInt32(meta, (int)(4 + i * 24 + 20));

                                Track _track = new Track();
                                switch((ChdOldTrackType)_trk.type)
                                {
                                    case ChdOldTrackType.Audio:
                                        _track.TrackBytesPerSector = 2352;
                                        _track.TrackRawBytesPerSector = 2352;
                                        _track.TrackType = TrackType.Audio;
                                        break;
                                    case ChdOldTrackType.Mode1:
                                        _track.TrackBytesPerSector = 2048;
                                        _track.TrackRawBytesPerSector = 2048;
                                        _track.TrackType = TrackType.CdMode1;
                                        break;
                                    case ChdOldTrackType.Mode1Raw:
                                        _track.TrackBytesPerSector = 2048;
                                        _track.TrackRawBytesPerSector = 2352;
                                        _track.TrackType = TrackType.CdMode1;
                                        break;
                                    case ChdOldTrackType.Mode2:
                                    case ChdOldTrackType.Mode2FormMix:
                                        _track.TrackBytesPerSector = 2336;
                                        _track.TrackRawBytesPerSector = 2336;
                                        _track.TrackType = TrackType.CdMode2Formless;
                                        break;
                                    case ChdOldTrackType.Mode2Form1:
                                        _track.TrackBytesPerSector = 2048;
                                        _track.TrackRawBytesPerSector = 2048;
                                        _track.TrackType = TrackType.CdMode2Form1;
                                        break;
                                    case ChdOldTrackType.Mode2Form2:
                                        _track.TrackBytesPerSector = 2324;
                                        _track.TrackRawBytesPerSector = 2324;
                                        _track.TrackType = TrackType.CdMode2Form2;
                                        break;
                                    case ChdOldTrackType.Mode2Raw:
                                        _track.TrackBytesPerSector = 2336;
                                        _track.TrackRawBytesPerSector = 2352;
                                        _track.TrackType = TrackType.CdMode2Formless;
                                        break;
                                    default:
                                        throw new ImageNotSupportedException(string.Format("Unsupported track type {0}",
                                                                                           _trk.type));
                                }

                                switch((ChdOldSubType)_trk.subType)
                                {
                                    case ChdOldSubType.Cooked:
                                        _track.TrackSubchannelFile = imageFilter.GetFilename();
                                        _track.TrackSubchannelType = TrackSubchannelType.PackedInterleaved;
                                        _track.TrackSubchannelFilter = imageFilter;
                                        break;
                                    case ChdOldSubType.None:
                                        _track.TrackSubchannelType = TrackSubchannelType.None;
                                        break;
                                    case ChdOldSubType.Raw:
                                        _track.TrackSubchannelFile = imageFilter.GetFilename();
                                        _track.TrackSubchannelType = TrackSubchannelType.RawInterleaved;
                                        _track.TrackSubchannelFilter = imageFilter;
                                        break;
                                    default:
                                        throw new
                                            ImageNotSupportedException(string.Format("Unsupported subchannel type {0}",
                                                                                     _trk.type));
                                }

                                _track.Indexes = new Dictionary<int, ulong>();
                                _track.TrackDescription = string.Format("Track {0}", i + 1);
                                _track.TrackEndSector = currentSector + _trk.frames - 1;
                                _track.TrackFile = imageFilter.GetFilename();
                                _track.TrackFileType = "BINARY";
                                _track.TrackFilter = imageFilter;
                                _track.TrackStartSector = currentSector;
                                _track.TrackSequence = i + 1;
                                _track.TrackSession = 1;
                                currentSector += _trk.frames + _trk.extraFrames;
                                tracks.Add(_track.TrackSequence, _track);
                            }

                            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
                            isCdrom = true;

                            break;
                        // "CHTR"
                        case CDROM_TRACK_METADATA:
                            if(isHdd)
                                throw new
                                    ImageNotSupportedException("Image cannot be a hard disk and a CD-ROM at the same time, aborting.");

                            if(isGdrom)
                                throw new
                                    ImageNotSupportedException("Image cannot be a GD-ROM and a CD-ROM at the same time, aborting.");

                            string chtr = StringHandlers.CToString(meta);
                            Regex chtrRegEx = new Regex(CDROM_METADATA_REGEX);
                            Match chtrMatch = chtrRegEx.Match(chtr);
                            if(chtrMatch.Success)
                            {
                                isCdrom = true;

                                uint trackNo = uint.Parse(chtrMatch.Groups["track"].Value);
                                uint frames = uint.Parse(chtrMatch.Groups["frames"].Value);
                                string subtype = chtrMatch.Groups["sub_type"].Value;
                                string tracktype = chtrMatch.Groups["track_type"].Value;

                                if(trackNo != currentTrack)
                                    throw new ImageNotSupportedException("Unsorted tracks, cannot proceed.");

                                Track _track = new Track();
                                switch(tracktype)
                                {
                                    case TRACK_TYPE_AUDIO:
                                        _track.TrackBytesPerSector = 2352;
                                        _track.TrackRawBytesPerSector = 2352;
                                        _track.TrackType = TrackType.Audio;
                                        break;
                                    case TRACK_TYPE_MODE1:
                                    case TRACK_TYPE_MODE1_2K:
                                        _track.TrackBytesPerSector = 2048;
                                        _track.TrackRawBytesPerSector = 2048;
                                        _track.TrackType = TrackType.CdMode1;
                                        break;
                                    case TRACK_TYPE_MODE1_RAW:
                                    case TRACK_TYPE_MODE1_RAW_2K:
                                        _track.TrackBytesPerSector = 2048;
                                        _track.TrackRawBytesPerSector = 2352;
                                        _track.TrackType = TrackType.CdMode1;
                                        break;
                                    case TRACK_TYPE_MODE2:
                                    case TRACK_TYPE_MODE2_2K:
                                    case TRACK_TYPE_MODE2_FM:
                                        _track.TrackBytesPerSector = 2336;
                                        _track.TrackRawBytesPerSector = 2336;
                                        _track.TrackType = TrackType.CdMode2Formless;
                                        break;
                                    case TRACK_TYPE_MODE2_F1:
                                    case TRACK_TYPE_MODE2_F1_2K:
                                        _track.TrackBytesPerSector = 2048;
                                        _track.TrackRawBytesPerSector = 2048;
                                        _track.TrackType = TrackType.CdMode2Form1;
                                        break;
                                    case TRACK_TYPE_MODE2_F2:
                                    case TRACK_TYPE_MODE2_F2_2K:
                                        _track.TrackBytesPerSector = 2324;
                                        _track.TrackRawBytesPerSector = 2324;
                                        _track.TrackType = TrackType.CdMode2Form2;
                                        break;
                                    case TRACK_TYPE_MODE2_RAW:
                                    case TRACK_TYPE_MODE2_RAW_2K:
                                        _track.TrackBytesPerSector = 2336;
                                        _track.TrackRawBytesPerSector = 2352;
                                        _track.TrackType = TrackType.CdMode2Formless;
                                        break;
                                    default:
                                        throw new ImageNotSupportedException(string.Format("Unsupported track type {0}",
                                                                                           tracktype));
                                }

                                switch(subtype)
                                {
                                    case SUB_TYPE_COOKED:
                                        _track.TrackSubchannelFile = imageFilter.GetFilename();
                                        _track.TrackSubchannelType = TrackSubchannelType.PackedInterleaved;
                                        _track.TrackSubchannelFilter = imageFilter;
                                        break;
                                    case SUB_TYPE_NONE:
                                        _track.TrackSubchannelType = TrackSubchannelType.None;
                                        break;
                                    case SUB_TYPE_RAW:
                                        _track.TrackSubchannelFile = imageFilter.GetFilename();
                                        _track.TrackSubchannelType = TrackSubchannelType.RawInterleaved;
                                        _track.TrackSubchannelFilter = imageFilter;
                                        break;
                                    default:
                                        throw new
                                            ImageNotSupportedException(string.Format("Unsupported subchannel type {0}",
                                                                                     subtype));
                                }

                                _track.Indexes = new Dictionary<int, ulong>();
                                _track.TrackDescription = string.Format("Track {0}", trackNo);
                                _track.TrackEndSector = currentSector + frames - 1;
                                _track.TrackFile = imageFilter.GetFilename();
                                _track.TrackFileType = "BINARY";
                                _track.TrackFilter = imageFilter;
                                _track.TrackStartSector = currentSector;
                                _track.TrackSequence = trackNo;
                                _track.TrackSession = 1;
                                currentSector += frames;
                                currentTrack++;
                                tracks.Add(_track.TrackSequence, _track);
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

                            string cht2 = StringHandlers.CToString(meta);
                            Regex cht2RegEx = new Regex(CDROM_METADATA2_REGEX);
                            Match cht2Match = cht2RegEx.Match(cht2);
                            if(cht2Match.Success)
                            {
                                isCdrom = true;

                                uint trackNo = uint.Parse(cht2Match.Groups["track"].Value);
                                uint frames = uint.Parse(cht2Match.Groups["frames"].Value);
                                string subtype = cht2Match.Groups["sub_type"].Value;
                                string tracktype = cht2Match.Groups["track_type"].Value;
                                // TODO: Check pregap and postgap behaviour
                                uint pregap = uint.Parse(cht2Match.Groups["pregap"].Value);
                                string pregapType = cht2Match.Groups["pgtype"].Value;
                                string pregapSubType = cht2Match.Groups["pgsub"].Value;
                                uint postgap = uint.Parse(cht2Match.Groups["postgap"].Value);

                                if(trackNo != currentTrack)
                                    throw new ImageNotSupportedException("Unsorted tracks, cannot proceed.");

                                Track _track = new Track();
                                switch(tracktype)
                                {
                                    case TRACK_TYPE_AUDIO:
                                        _track.TrackBytesPerSector = 2352;
                                        _track.TrackRawBytesPerSector = 2352;
                                        _track.TrackType = TrackType.Audio;
                                        break;
                                    case TRACK_TYPE_MODE1:
                                    case TRACK_TYPE_MODE1_2K:
                                        _track.TrackBytesPerSector = 2048;
                                        _track.TrackRawBytesPerSector = 2048;
                                        _track.TrackType = TrackType.CdMode1;
                                        break;
                                    case TRACK_TYPE_MODE1_RAW:
                                    case TRACK_TYPE_MODE1_RAW_2K:
                                        _track.TrackBytesPerSector = 2048;
                                        _track.TrackRawBytesPerSector = 2352;
                                        _track.TrackType = TrackType.CdMode1;
                                        break;
                                    case TRACK_TYPE_MODE2:
                                    case TRACK_TYPE_MODE2_2K:
                                    case TRACK_TYPE_MODE2_FM:
                                        _track.TrackBytesPerSector = 2336;
                                        _track.TrackRawBytesPerSector = 2336;
                                        _track.TrackType = TrackType.CdMode2Formless;
                                        break;
                                    case TRACK_TYPE_MODE2_F1:
                                    case TRACK_TYPE_MODE2_F1_2K:
                                        _track.TrackBytesPerSector = 2048;
                                        _track.TrackRawBytesPerSector = 2048;
                                        _track.TrackType = TrackType.CdMode2Form1;
                                        break;
                                    case TRACK_TYPE_MODE2_F2:
                                    case TRACK_TYPE_MODE2_F2_2K:
                                        _track.TrackBytesPerSector = 2324;
                                        _track.TrackRawBytesPerSector = 2324;
                                        _track.TrackType = TrackType.CdMode2Form2;
                                        break;
                                    case TRACK_TYPE_MODE2_RAW:
                                    case TRACK_TYPE_MODE2_RAW_2K:
                                        _track.TrackBytesPerSector = 2336;
                                        _track.TrackRawBytesPerSector = 2352;
                                        _track.TrackType = TrackType.CdMode2Formless;
                                        break;
                                    default:
                                        throw new ImageNotSupportedException(string.Format("Unsupported track type {0}",
                                                                                           tracktype));
                                }

                                switch(subtype)
                                {
                                    case SUB_TYPE_COOKED:
                                        _track.TrackSubchannelFile = imageFilter.GetFilename();
                                        _track.TrackSubchannelType = TrackSubchannelType.PackedInterleaved;
                                        _track.TrackSubchannelFilter = imageFilter;
                                        break;
                                    case SUB_TYPE_NONE:
                                        _track.TrackSubchannelType = TrackSubchannelType.None;
                                        break;
                                    case SUB_TYPE_RAW:
                                        _track.TrackSubchannelFile = imageFilter.GetFilename();
                                        _track.TrackSubchannelType = TrackSubchannelType.RawInterleaved;
                                        _track.TrackSubchannelFilter = imageFilter;
                                        break;
                                    default:
                                        throw new
                                            ImageNotSupportedException(string.Format("Unsupported subchannel type {0}",
                                                                                     subtype));
                                }

                                _track.Indexes = new Dictionary<int, ulong>();
                                _track.TrackDescription = string.Format("Track {0}", trackNo);
                                _track.TrackEndSector = currentSector + frames - 1;
                                _track.TrackFile = imageFilter.GetFilename();
                                _track.TrackFileType = "BINARY";
                                _track.TrackFilter = imageFilter;
                                _track.TrackStartSector = currentSector;
                                _track.TrackSequence = trackNo;
                                _track.TrackSession = 1;
                                currentSector += frames;
                                currentTrack++;
                                tracks.Add(_track.TrackSequence, _track);
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

                            string chgd = StringHandlers.CToString(meta);
                            Regex chgdRegEx = new Regex(GDROM_METADATA_REGEX);
                            Match chgdMatch = chgdRegEx.Match(chgd);
                            if(chgdMatch.Success)
                            {
                                isGdrom = true;

                                uint trackNo = uint.Parse(chgdMatch.Groups["track"].Value);
                                uint frames = uint.Parse(chgdMatch.Groups["frames"].Value);
                                string subtype = chgdMatch.Groups["sub_type"].Value;
                                string tracktype = chgdMatch.Groups["track_type"].Value;
                                // TODO: Check pregap, postgap and pad behaviour
                                uint pregap = uint.Parse(chgdMatch.Groups["pregap"].Value);
                                string pregapType = chgdMatch.Groups["pgtype"].Value;
                                string pregapSubType = chgdMatch.Groups["pgsub"].Value;
                                uint postgap = uint.Parse(chgdMatch.Groups["postgap"].Value);
                                uint pad = uint.Parse(chgdMatch.Groups["pad"].Value);

                                if(trackNo != currentTrack)
                                    throw new ImageNotSupportedException("Unsorted tracks, cannot proceed.");

                                Track _track = new Track();
                                switch(tracktype)
                                {
                                    case TRACK_TYPE_AUDIO:
                                        _track.TrackBytesPerSector = 2352;
                                        _track.TrackRawBytesPerSector = 2352;
                                        _track.TrackType = TrackType.Audio;
                                        break;
                                    case TRACK_TYPE_MODE1:
                                    case TRACK_TYPE_MODE1_2K:
                                        _track.TrackBytesPerSector = 2048;
                                        _track.TrackRawBytesPerSector = 2048;
                                        _track.TrackType = TrackType.CdMode1;
                                        break;
                                    case TRACK_TYPE_MODE1_RAW:
                                    case TRACK_TYPE_MODE1_RAW_2K:
                                        _track.TrackBytesPerSector = 2048;
                                        _track.TrackRawBytesPerSector = 2352;
                                        _track.TrackType = TrackType.CdMode1;
                                        break;
                                    case TRACK_TYPE_MODE2:
                                    case TRACK_TYPE_MODE2_2K:
                                    case TRACK_TYPE_MODE2_FM:
                                        _track.TrackBytesPerSector = 2336;
                                        _track.TrackRawBytesPerSector = 2336;
                                        _track.TrackType = TrackType.CdMode2Formless;
                                        break;
                                    case TRACK_TYPE_MODE2_F1:
                                    case TRACK_TYPE_MODE2_F1_2K:
                                        _track.TrackBytesPerSector = 2048;
                                        _track.TrackRawBytesPerSector = 2048;
                                        _track.TrackType = TrackType.CdMode2Form1;
                                        break;
                                    case TRACK_TYPE_MODE2_F2:
                                    case TRACK_TYPE_MODE2_F2_2K:
                                        _track.TrackBytesPerSector = 2324;
                                        _track.TrackRawBytesPerSector = 2324;
                                        _track.TrackType = TrackType.CdMode2Form2;
                                        break;
                                    case TRACK_TYPE_MODE2_RAW:
                                    case TRACK_TYPE_MODE2_RAW_2K:
                                        _track.TrackBytesPerSector = 2336;
                                        _track.TrackRawBytesPerSector = 2352;
                                        _track.TrackType = TrackType.CdMode2Formless;
                                        break;
                                    default:
                                        throw new ImageNotSupportedException(string.Format("Unsupported track type {0}",
                                                                                           tracktype));
                                }

                                switch(subtype)
                                {
                                    case SUB_TYPE_COOKED:
                                        _track.TrackSubchannelFile = imageFilter.GetFilename();
                                        _track.TrackSubchannelType = TrackSubchannelType.PackedInterleaved;
                                        _track.TrackSubchannelFilter = imageFilter;
                                        break;
                                    case SUB_TYPE_NONE:
                                        _track.TrackSubchannelType = TrackSubchannelType.None;
                                        break;
                                    case SUB_TYPE_RAW:
                                        _track.TrackSubchannelFile = imageFilter.GetFilename();
                                        _track.TrackSubchannelType = TrackSubchannelType.RawInterleaved;
                                        _track.TrackSubchannelFilter = imageFilter;
                                        break;
                                    default:
                                        throw new
                                            ImageNotSupportedException(string.Format("Unsupported subchannel type {0}",
                                                                                     subtype));
                                }

                                _track.Indexes = new Dictionary<int, ulong>();
                                _track.TrackDescription = string.Format("Track {0}", trackNo);
                                _track.TrackEndSector = currentSector + frames - 1;
                                _track.TrackFile = imageFilter.GetFilename();
                                _track.TrackFileType = "BINARY";
                                _track.TrackFilter = imageFilter;
                                _track.TrackStartSector = currentSector;
                                _track.TrackSequence = trackNo;
                                _track.TrackSession = (ushort)(trackNo > 2 ? 2 : 1);
                                currentSector += frames;
                                currentTrack++;
                                tracks.Add(_track.TrackSequence, _track);
                            }

                            break;
                        // "IDNT"
                        case HARD_DISK_IDENT_METADATA:
                            Identify.IdentifyDevice? idnt = Identify.Decode(meta);
                            if(idnt.HasValue)
                            {
                                ImageInfo.MediaManufacturer = idnt.Value.MediaManufacturer;
                                ImageInfo.MediaSerialNumber = idnt.Value.MediaSerial;
                                ImageInfo.DriveModel = idnt.Value.Model;
                                ImageInfo.DriveSerialNumber = idnt.Value.SerialNumber;
                                ImageInfo.DriveFirmwareRevision = idnt.Value.FirmwareRevision;
                                if(idnt.Value.CurrentCylinders > 0 && idnt.Value.CurrentHeads > 0 &&
                                   idnt.Value.CurrentSectorsPerTrack > 0)
                                {
                                    ImageInfo.Cylinders = idnt.Value.CurrentCylinders;
                                    ImageInfo.Heads = idnt.Value.CurrentHeads;
                                    ImageInfo.SectorsPerTrack = idnt.Value.CurrentSectorsPerTrack;
                                }
                                else
                                {
                                    ImageInfo.Cylinders = idnt.Value.Cylinders;
                                    ImageInfo.Heads = idnt.Value.Heads;
                                    ImageInfo.SectorsPerTrack = idnt.Value.SectorsPerTrack;
                                }
                            }
                            identify = meta;
                            if(!ImageInfo.ReadableMediaTags.Contains(MediaTagType.ATA_IDENTIFY))
                                ImageInfo.ReadableMediaTags.Add(MediaTagType.ATA_IDENTIFY);
                            break;
                        case PCMCIA_CIS_METADATA:
                            cis = meta;
                            if(!ImageInfo.ReadableMediaTags.Contains(MediaTagType.PCMCIA_CIS))
                                ImageInfo.ReadableMediaTags.Add(MediaTagType.PCMCIA_CIS);
                            break;
                    }

                    nextMetaOff = header.next;
                }

                if(isHdd)
                {
                    sectorsPerHunk = bytesPerHunk / ImageInfo.SectorSize;
                    ImageInfo.Sectors = ImageInfo.ImageSize / ImageInfo.SectorSize;
                    ImageInfo.MediaType = MediaType.GENERIC_HDD;
                    ImageInfo.XmlMediaType = XmlMediaType.BlockMedia;
                }
                else if(isCdrom)
                {
                    // Hardcoded on MAME for CD-ROM
                    sectorsPerHunk = 8;
                    ImageInfo.MediaType = MediaType.CDROM;
                    ImageInfo.XmlMediaType = XmlMediaType.OpticalDisc;

                    foreach(Track _trk in tracks.Values)
                        ImageInfo.Sectors += _trk.TrackEndSector - _trk.TrackStartSector + 1;
                }
                else if(isGdrom)
                {
                    // Hardcoded on MAME for GD-ROM
                    sectorsPerHunk = 8;
                    ImageInfo.MediaType = MediaType.GDROM;
                    ImageInfo.XmlMediaType = XmlMediaType.OpticalDisc;

                    foreach(Track _trk in tracks.Values)
                        ImageInfo.Sectors += _trk.TrackEndSector - _trk.TrackStartSector + 1;
                }
                else throw new ImageNotSupportedException("Image does not represent a known media, aborting");
            }

            if(isCdrom || isGdrom)
            {
                offsetmap = new Dictionary<ulong, uint>();
                partitions = new List<Partition>();
                ulong partPos = 0;
                foreach(Track _track in tracks.Values)
                {
                    Partition partition = new Partition();
                    partition.Description = _track.TrackDescription;
                    partition.Size = (_track.TrackEndSector - _track.TrackStartSector + 1) *
                                     (ulong)_track.TrackRawBytesPerSector;
                    partition.Length = _track.TrackEndSector - _track.TrackStartSector + 1;
                    partition.Sequence = _track.TrackSequence;
                    partition.Offset = partPos;
                    partition.Start = _track.TrackStartSector;
                    partition.Type = _track.TrackType.ToString();
                    partPos += partition.Length;
                    offsetmap.Add(_track.TrackStartSector, _track.TrackSequence);

                    if(_track.TrackSubchannelType != TrackSubchannelType.None)
                        if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                            ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);

                    switch(_track.TrackType)
                    {
                        case TrackType.CdMode1:
                        case TrackType.CdMode2Form1:
                            if(_track.TrackRawBytesPerSector == 2352)
                            {
                                if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                    ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);
                                if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                    ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);
                                if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                                    ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);
                                if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEcc))
                                    ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEcc);
                                if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccP))
                                    ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccP);
                                if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccQ))
                                    ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccQ);
                                if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                                    ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);
                            }
                            break;
                        case TrackType.CdMode2Form2:
                            if(_track.TrackRawBytesPerSector == 2352)
                            {
                                if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                    ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);
                                if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                    ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);
                                if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                                    ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);
                                if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                                    ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);
                            }
                            break;
                        case TrackType.CdMode2Formless:
                            if(_track.TrackRawBytesPerSector == 2352)
                            {
                                if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                    ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);
                                if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                    ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);
                            }
                            break;
                    }

                    if(_track.TrackBytesPerSector > ImageInfo.SectorSize)
                        ImageInfo.SectorSize = (uint)_track.TrackBytesPerSector;

                    partitions.Add(partition);
                }

                ImageInfo.ImageHasPartitions = true;
                ImageInfo.ImageHasSessions = true;
            }

            maxBlockCache = (int)(MAX_CACHE_SIZE / (ImageInfo.SectorSize * sectorsPerHunk));
            maxSectorCache = (int)(MAX_CACHE_SIZE / ImageInfo.SectorSize);

            imageStream = stream;

            sectorCache = new Dictionary<ulong, byte[]>();
            hunkCache = new Dictionary<ulong, byte[]>();

            // TODO: Detect CompactFlash
            // TODO: Get manufacturer and drive name from CIS if applicable
            if(cis != null) ImageInfo.MediaType = MediaType.PCCardTypeI;

            return true;
        }

        Track GetTrack(ulong sector)
        {
            Track track = new Track();
            foreach(KeyValuePair<ulong, uint> kvp in offsetmap.Where(kvp => sector >= kvp.Key)) tracks.TryGetValue(kvp.Value, out track);

            return track;
        }

        ulong GetAbsoluteSector(ulong relativeSector, uint track)
        {
            Track _track;
            tracks.TryGetValue(track, out _track);
            return _track.TrackStartSector + relativeSector;
        }

        byte[] GetHunk(ulong hunkNo)
        {
            byte[] hunk;

            if(hunkCache.TryGetValue(hunkNo, out hunk)) return hunk;

            switch(mapVersion)
            {
                case 1:
                    ulong offset = hunkTable[hunkNo] & 0x00000FFFFFFFFFFF;
                    ulong length = hunkTable[hunkNo] >> 44;

                    byte[] compHunk = new byte[length];
                    imageStream.Seek((long)offset, SeekOrigin.Begin);
                    imageStream.Read(compHunk, 0, compHunk.Length);

                    if(length == sectorsPerHunk * ImageInfo.SectorSize) hunk = compHunk;
                    else if((ChdCompression)hdrCompression > ChdCompression.Zlib)
                        throw new ImageNotSupportedException(string.Format("Unsupported compression {0}",
                                                                           (ChdCompression)hdrCompression));
                    else
                    {
                        DeflateStream zStream =
                            new DeflateStream(new MemoryStream(compHunk), CompressionMode.Decompress);
                        hunk = new byte[sectorsPerHunk * ImageInfo.SectorSize];
                        int read = zStream.Read(hunk, 0, (int)(sectorsPerHunk * ImageInfo.SectorSize));
                        if(read != sectorsPerHunk * ImageInfo.SectorSize)
                            throw new
                                IOException(string
                                                .Format("Unable to decompress hunk correctly, got {0} bytes, expected {1}",
                                                        read, sectorsPerHunk * ImageInfo.SectorSize));

                        zStream.Close();
                    }

                    break;
                case 3:
                    byte[] entryBytes = new byte[16];
                    Array.Copy(hunkMap, (int)(hunkNo * 16), entryBytes, 0, 16);
                    ChdMapV3Entry entry = BigEndianMarshal.ByteArrayToStructureBigEndian<ChdMapV3Entry>(entryBytes);
                    switch((Chdv3EntryFlags)(entry.flags & 0x0F))
                    {
                        case Chdv3EntryFlags.Invalid: throw new ArgumentException("Invalid hunk found.");
                        case Chdv3EntryFlags.Compressed:
                            switch((ChdCompression)hdrCompression)
                            {
                                case ChdCompression.None: goto uncompressedV3;
                                case ChdCompression.Zlib:
                                case ChdCompression.ZlibPlus:
                                    if(isHdd)
                                    {
                                        byte[] zHunk = new byte[(entry.lengthLsb << 16) + entry.lengthLsb];
                                        imageStream.Seek((long)entry.offset, SeekOrigin.Begin);
                                        imageStream.Read(zHunk, 0, zHunk.Length);
                                        DeflateStream zStream =
                                            new DeflateStream(new MemoryStream(zHunk), CompressionMode.Decompress);
                                        hunk = new byte[bytesPerHunk];
                                        int read = zStream.Read(hunk, 0, (int)bytesPerHunk);
                                        if(read != bytesPerHunk)
                                            throw new
                                                IOException(string
                                                                .Format("Unable to decompress hunk correctly, got {0} bytes, expected {1}",
                                                                        read, bytesPerHunk));

                                        zStream.Close();
                                    }
                                    // TODO: Guess wth is MAME doing with these hunks
                                    else
                                        throw new
                                            ImageNotSupportedException("Compressed CD/GD-ROM hunks are not yet supported");

                                    break;
                                case ChdCompression.Av:
                                    throw new
                                        ImageNotSupportedException(string.Format("Unsupported compression {0}",
                                                                                 (ChdCompression)hdrCompression));
                            }

                            break;
                        case Chdv3EntryFlags.Uncompressed:
                            uncompressedV3:
                            hunk = new byte[bytesPerHunk];
                            imageStream.Seek((long)entry.offset, SeekOrigin.Begin);
                            imageStream.Read(hunk, 0, hunk.Length);
                            break;
                        case Chdv3EntryFlags.Mini:
                            hunk = new byte[bytesPerHunk];
                            byte[] mini;
                            mini = BigEndianBitConverter.GetBytes(entry.offset);
                            for(int i = 0; i < bytesPerHunk; i++) hunk[i] = mini[i % 8];

                            break;
                        case Chdv3EntryFlags.SelfHunk: return GetHunk(entry.offset);
                        case Chdv3EntryFlags.ParentHunk:
                            throw new ImageNotSupportedException("Parent images are not supported");
                        case Chdv3EntryFlags.SecondCompressed:
                            throw new ImageNotSupportedException("FLAC is not supported");
                        default:
                            throw new ImageNotSupportedException(string.Format("Hunk type {0} is not supported",
                                                                               entry.flags & 0xF));
                    }

                    break;
                case 5:
                    if(hdrCompression == 0)
                    {
                        hunk = new byte[bytesPerHunk];
                        imageStream.Seek(hunkTableSmall[hunkNo] * bytesPerHunk, SeekOrigin.Begin);
                        imageStream.Read(hunk, 0, hunk.Length);
                    }
                    else throw new ImageNotSupportedException("Compressed v5 hunks not yet supported");

                    break;
                default:
                    throw new ImageNotSupportedException(string.Format("Unsupported hunk map version {0}",
                                                                       mapVersion));
            }

            if(hunkCache.Count >= maxBlockCache) hunkCache.Clear();

            hunkCache.Add(hunkNo, hunk);

            return hunk;
        }

        public override bool? VerifySector(ulong sectorAddress)
        {
            if(isHdd) return null;

            byte[] buffer = ReadSectorLong(sectorAddress);
            return CdChecksums.CheckCdSector(buffer);
        }

        public override bool? VerifySector(ulong sectorAddress, uint track)
        {
            if(isHdd)
                throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

            return VerifySector(GetAbsoluteSector(sectorAddress, track));
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            unknownLbas = new List<ulong>();
            failingLbas = new List<ulong>();
            if(isHdd) return null;

            byte[] buffer = ReadSectorsLong(sectorAddress, length);
            int bps = (int)(buffer.Length / length);
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
            if(failingLbas.Count > 0) return false;

            return true;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            unknownLbas = new List<ulong>();
            failingLbas = new List<ulong>();
            if(isHdd) return null;

            byte[] buffer = ReadSectorsLong(sectorAddress, length, track);
            int bps = (int)(buffer.Length / length);
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
            if(failingLbas.Count > 0) return false;

            return true;
        }

        public override bool? VerifyMediaImage()
        {
            byte[] calculated;
            if(mapVersion >= 3)
            {
                Sha1Context sha1Ctx = new Sha1Context();
                sha1Ctx.Init();
                for(uint i = 0; i < totalHunks; i++) sha1Ctx.Update(GetHunk(i));

                calculated = sha1Ctx.Final();
            }
            else
            {
                Md5Context md5Ctx = new Md5Context();
                md5Ctx.Init();
                for(uint i = 0; i < totalHunks; i++) md5Ctx.Update(GetHunk(i));

                calculated = md5Ctx.Final();
            }

            return expectedChecksum.SequenceEqual(calculated);
        }

        public override bool ImageHasPartitions()
        {
            return ImageInfo.ImageHasPartitions;
        }

        public override ulong GetImageSize()
        {
            return ImageInfo.ImageSize;
        }

        public override ulong GetSectors()
        {
            return ImageInfo.Sectors;
        }

        public override uint GetSectorSize()
        {
            return ImageInfo.SectorSize;
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      string.Format("Sector address {0} not found", sectorAddress));

            byte[] sector;
            Track track = new Track();

            if(!sectorCache.TryGetValue(sectorAddress, out sector))
            {
                uint sectorSize;

                if(isHdd) sectorSize = ImageInfo.SectorSize;
                else
                {
                    track = GetTrack(sectorAddress);
                    sectorSize = (uint)track.TrackRawBytesPerSector;
                }

                ulong hunkNo = sectorAddress / sectorsPerHunk;
                ulong secOff = sectorAddress * sectorSize % (sectorsPerHunk * sectorSize);

                byte[] hunk = GetHunk(hunkNo);

                sector = new byte[ImageInfo.SectorSize];
                Array.Copy(hunk, (int)secOff, sector, 0, sector.Length);

                if(sectorCache.Count >= maxSectorCache) sectorCache.Clear();

                sectorCache.Add(sectorAddress, sector);
            }

            if(isHdd) return sector;

            uint sector_offset;
            uint sector_size;

            switch(track.TrackType)
            {
                case TrackType.CdMode1:
                case TrackType.CdMode2Form1:
                {
                    if(track.TrackRawBytesPerSector == 2352)
                    {
                        sector_offset = 16;
                        sector_size = 2048;
                    }
                    else
                    {
                        sector_offset = 0;
                        sector_size = 2048;
                    }
                    break;
                }
                case TrackType.CdMode2Form2:
                {
                    if(track.TrackRawBytesPerSector == 2352)
                    {
                        sector_offset = 16;
                        sector_size = 2324;
                    }
                    else
                    {
                        sector_offset = 0;
                        sector_size = 2324;
                    }
                    break;
                }
                case TrackType.CdMode2Formless:
                {
                    if(track.TrackRawBytesPerSector == 2352)
                    {
                        sector_offset = 16;
                        sector_size = 2336;
                    }
                    else
                    {
                        sector_offset = 0;
                        sector_size = 2336;
                    }
                    break;
                }
                case TrackType.Audio:
                {
                    sector_offset = 0;
                    sector_size = 2352;
                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sector_size];

            if(track.TrackType == TrackType.Audio && swapAudio)
                for(int i = 0; i < 2352; i += 2)
                {
                    buffer[i + 1] = sector[i];
                    buffer[i] = sector[i + 1];
                }
            else Array.Copy(sector, sector_offset, buffer, 0, sector_size);

            return buffer;
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            if(isHdd) throw new FeatureNotPresentImageException("Hard disk images do not have sector tags");

            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      string.Format("Sector address {0} not found", sectorAddress));

            byte[] sector;
            Track track = new Track();

            if(!sectorCache.TryGetValue(sectorAddress, out sector))
            {
                uint sectorSize;

                track = GetTrack(sectorAddress);
                sectorSize = (uint)track.TrackRawBytesPerSector;

                ulong hunkNo = sectorAddress / sectorsPerHunk;
                ulong secOff = sectorAddress * sectorSize % (sectorsPerHunk * sectorSize);

                byte[] hunk = GetHunk(hunkNo);

                sector = new byte[ImageInfo.SectorSize];
                Array.Copy(hunk, (int)secOff, sector, 0, sector.Length);

                if(sectorCache.Count >= maxSectorCache) sectorCache.Clear();

                sectorCache.Add(sectorAddress, sector);
            }

            if(isHdd) return sector;

            uint sector_offset;
            uint sector_size;

            if(tag == SectorTagType.CdSectorSubchannel)
                switch(track.TrackSubchannelType) {
                    case TrackSubchannelType.None: throw new FeatureNotPresentImageException("Requested sector does not contain subchannel");
                    case TrackSubchannelType.RawInterleaved:
                        sector_offset = (uint)track.TrackRawBytesPerSector;
                        sector_size = 96;
                        break;
                    default:
                        throw new
                            FeatureSupportedButNotImplementedImageException(string.Format("Unsupported subchannel type {0}",
                                                                                          track.TrackSubchannelType));
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
                                    sector_offset = 0;
                                    sector_size = 12;
                                    break;
                                }
                                case SectorTagType.CdSectorHeader:
                                {
                                    sector_offset = 12;
                                    sector_size = 4;
                                    break;
                                }
                                case SectorTagType.CdSectorSubHeader:
                                    throw new ArgumentException("Unsupported tag requested for this track",
                                                                nameof(tag));
                                case SectorTagType.CdSectorEcc:
                                {
                                    sector_offset = 2076;
                                    sector_size = 276;
                                    break;
                                }
                                case SectorTagType.CdSectorEccP:
                                {
                                    sector_offset = 2076;
                                    sector_size = 172;
                                    break;
                                }
                                case SectorTagType.CdSectorEccQ:
                                {
                                    sector_offset = 2248;
                                    sector_size = 104;
                                    break;
                                }
                                case SectorTagType.CdSectorEdc:
                                {
                                    sector_offset = 2064;
                                    sector_size = 4;
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
                                    sector_offset = 0;
                                    sector_size = 12;
                                    break;
                                }
                                case SectorTagType.CdSectorHeader:
                                {
                                    sector_offset = 12;
                                    sector_size = 4;
                                    break;
                                }
                                case SectorTagType.CdSectorSubHeader:
                                {
                                    sector_offset = 16;
                                    sector_size = 8;
                                    break;
                                }
                                case SectorTagType.CdSectorEdc:
                                {
                                    sector_offset = 2348;
                                    sector_size = 4;
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
                                    sector_offset = 0;
                                    sector_size = 8;
                                    break;
                                }
                                case SectorTagType.CdSectorEdc:
                                {
                                    sector_offset = 2332;
                                    sector_size = 4;
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
                                    sector_offset = 0;
                                    sector_size = 8;
                                    break;
                                }
                                case SectorTagType.CdSectorEdc:
                                {
                                    sector_offset = 2332;
                                    sector_size = 4;
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

            byte[] buffer = new byte[sector_size];

            if(track.TrackType == TrackType.Audio && swapAudio)
                for(int i = 0; i < 2352; i += 2)
                {
                    buffer[i + 1] = sector[i];
                    buffer[i] = sector[i + 1];
                }
            else Array.Copy(sector, sector_offset, buffer, 0, sector_size);

            if(track.TrackType == TrackType.Audio && swapAudio)
                for(int i = 0; i < 2352; i += 2)
                {
                    buffer[i + 1] = sector[i];
                    buffer[i] = sector[i + 1];
                }
            else Array.Copy(sector, sector_offset, buffer, 0, sector_size);

            return buffer;
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      string.Format("Sector address {0} not found", sectorAddress));

            if(sectorAddress + length > ImageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      string.Format("Requested more sectors ({0}) than available ({1})",
                                                                    sectorAddress + length, ImageInfo.Sectors));

            MemoryStream ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      string.Format("Sector address {0} not found", sectorAddress));

            if(sectorAddress + length > ImageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      string.Format("Requested more sectors ({0}) than available ({1})",
                                                                    sectorAddress + length, ImageInfo.Sectors));

            MemoryStream ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSectorTag(sectorAddress + i, tag);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }

        public override byte[] ReadSectorLong(ulong sectorAddress)
        {
            if(isHdd) return ReadSector(sectorAddress);

            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      string.Format("Sector address {0} not found", sectorAddress));

            byte[] sector;
            Track track = new Track();

            if(!sectorCache.TryGetValue(sectorAddress, out sector))
            {
                uint sectorSize;

                track = GetTrack(sectorAddress);
                sectorSize = (uint)track.TrackRawBytesPerSector;

                ulong hunkNo = sectorAddress / sectorsPerHunk;
                ulong secOff = sectorAddress * sectorSize % (sectorsPerHunk * sectorSize);

                byte[] hunk = GetHunk(hunkNo);

                sector = new byte[ImageInfo.SectorSize];
                Array.Copy(hunk, (int)secOff, sector, 0, sector.Length);

                if(sectorCache.Count >= maxSectorCache) sectorCache.Clear();

                sectorCache.Add(sectorAddress, sector);
            }

            byte[] buffer = new byte[track.TrackRawBytesPerSector];

            if(track.TrackType == TrackType.Audio && swapAudio)
                for(int i = 0; i < 2352; i += 2)
                {
                    buffer[i + 1] = sector[i];
                    buffer[i] = sector[i + 1];
                }
            else Array.Copy(sector, 0, buffer, 0, track.TrackRawBytesPerSector);

            return buffer;
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      string.Format("Sector address {0} not found", sectorAddress));

            if(sectorAddress + length > ImageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      string.Format("Requested more sectors ({0}) than available ({1})",
                                                                    sectorAddress + length, ImageInfo.Sectors));

            MemoryStream ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSectorLong(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }

        public override string GetImageFormat()
        {
            return "Compressed Hunks of Data";
        }

        public override string GetImageVersion()
        {
            return ImageInfo.ImageVersion;
        }

        public override string GetImageApplication()
        {
            return ImageInfo.ImageApplication;
        }

        public override string GetImageApplicationVersion()
        {
            return ImageInfo.ImageApplicationVersion;
        }

        public override DateTime GetImageCreationTime()
        {
            return ImageInfo.ImageCreationTime;
        }

        public override DateTime GetImageLastModificationTime()
        {
            return ImageInfo.ImageLastModificationTime;
        }

        public override string GetImageName()
        {
            return ImageInfo.ImageName;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.MediaType;
        }

        #region Unsupported features
        public override byte[] ReadDiskTag(MediaTagType tag)
        {
            if(ImageInfo.ReadableMediaTags.Contains(MediaTagType.ATA_IDENTIFY)) return identify;

            if(ImageInfo.ReadableMediaTags.Contains(MediaTagType.PCMCIA_CIS)) return cis;

            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override string GetImageCreator()
        {
            return ImageInfo.ImageCreator;
        }

        public override string GetImageComments()
        {
            return ImageInfo.ImageComments;
        }

        public override string GetMediaManufacturer()
        {
            return ImageInfo.MediaManufacturer;
        }

        public override string GetMediaModel()
        {
            return ImageInfo.MediaModel;
        }

        public override string GetMediaSerialNumber()
        {
            return ImageInfo.MediaSerialNumber;
        }

        public override string GetMediaBarcode()
        {
            return ImageInfo.MediaBarcode;
        }

        public override string GetMediaPartNumber()
        {
            return ImageInfo.MediaPartNumber;
        }

        public override int GetMediaSequence()
        {
            return ImageInfo.MediaSequence;
        }

        public override int GetLastDiskSequence()
        {
            return ImageInfo.LastMediaSequence;
        }

        public override string GetDriveManufacturer()
        {
            return ImageInfo.DriveManufacturer;
        }

        public override string GetDriveModel()
        {
            return ImageInfo.DriveModel;
        }

        public override string GetDriveSerialNumber()
        {
            return ImageInfo.DriveSerialNumber;
        }

        public override List<Partition> GetPartitions()
        {
            if(isHdd)
                throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

            return partitions;
        }

        public override List<Track> GetTracks()
        {
            if(isHdd)
                throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

            return tracks.Values.ToList();
        }

        public override List<Track> GetSessionTracks(Session session)
        {
            if(isHdd)
                throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

            return GetSessionTracks(session.SessionSequence);
        }

        public override List<Track> GetSessionTracks(ushort session)
        {
            if(isHdd)
                throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

            return tracks.Values.Where(track => track.TrackSession == session).ToList();
        }

        public override List<Session> GetSessions()
        {
            if(isHdd)
                throw new
                    FeaturedNotSupportedByDiscImageException("Cannot access optical sessions on a hard disk image");

            throw new NotImplementedException();
        }

        public override byte[] ReadSector(ulong sectorAddress, uint track)
        {
            if(isHdd)
                throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

            return ReadSector(GetAbsoluteSector(sectorAddress, track));
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            if(isHdd)
                throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

            return ReadSectorTag(GetAbsoluteSector(sectorAddress, track), tag);
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            if(isHdd)
                throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

            return ReadSectors(GetAbsoluteSector(sectorAddress, track), length);
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            if(isHdd)
                throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

            return ReadSectorsTag(GetAbsoluteSector(sectorAddress, track), length, tag);
        }

        public override byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            if(isHdd)
                throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

            return ReadSectorLong(GetAbsoluteSector(sectorAddress, track));
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            if(isHdd)
                throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

            return ReadSectorLong(GetAbsoluteSector(sectorAddress, track), length);
        }
        #endregion Unsupported features
    }
}