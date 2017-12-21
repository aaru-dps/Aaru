// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Nero.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Nero Burning ROM disc images.
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
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.DiscImages
{
    public class Nero : ImagePlugin
    {
        #region Internal structures
        struct NeroV1Footer
        {
            /// <summary>
            /// "NERO"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            /// Offset of first chunk in file
            /// </summary>
            public uint FirstChunkOffset;
        }

        struct NeroV2Footer
        {
            /// <summary>
            /// "NER5"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            /// Offset of first chunk in file
            /// </summary>
            public ulong FirstChunkOffset;
        }

        struct NeroV2CueEntry
        {
            /// <summary>
            /// Track mode. 0x01 for audio, 0x21 for copy-protected audio, 0x41 for data
            /// </summary>
            public byte Mode;

            /// <summary>
            /// Track number in BCD
            /// </summary>
            public byte TrackNumber;

            /// <summary>
            /// Index number in BCD
            /// </summary>
            public byte IndexNumber;

            /// <summary>
            /// Always zero
            /// </summary>
            public byte Dummy;

            /// <summary>
            /// LBA sector start for this entry
            /// </summary>
            public int LbaStart;
        }

        struct NeroV2Cuesheet
        {
            /// <summary>
            /// "CUEX"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            /// Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            /// Cuesheet entries
            /// </summary>
            public List<NeroV2CueEntry> Entries;
        }

        struct NeroV1CueEntry
        {
            /// <summary>
            /// Track mode. 0x01 for audio, 0x21 for copy-protected audio, 0x41 for data
            /// </summary>
            public byte Mode;

            /// <summary>
            /// Track number in BCD
            /// </summary>
            public byte TrackNumber;

            /// <summary>
            /// Index number in BCD
            /// </summary>
            public byte IndexNumber;

            /// <summary>
            /// Always zero
            /// </summary>
            public ushort Dummy;

            /// <summary>
            /// MSF start sector's minute for this entry
            /// </summary>
            public byte Minute;

            /// <summary>
            /// MSF start sector's second for this entry
            /// </summary>
            public byte Second;

            /// <summary>
            /// MSF start sector's frame for this entry
            /// </summary>
            public byte Frame;
        }

        struct NeroV1Cuesheet
        {
            /// <summary>
            /// "CUES"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            /// Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            /// Cuesheet entries
            /// </summary>
            public List<NeroV1CueEntry> Entries;
        }

        struct NeroV1DaoEntry
        {
            /// <summary>
            /// ISRC (12 bytes)
            /// </summary>
            public byte[] Isrc;

            /// <summary>
            /// Size of sector inside image (in bytes)
            /// </summary>
            public ushort SectorSize;

            /// <summary>
            /// Sector mode in image
            /// </summary>
            public ushort Mode;

            /// <summary>
            /// Unknown
            /// </summary>
            public ushort Unknown;

            /// <summary>
            /// Index 0 start
            /// </summary>
            public uint Index0;

            /// <summary>
            /// Index 1 start
            /// </summary>
            public uint Index1;

            /// <summary>
            /// End of track + 1
            /// </summary>
            public uint EndOfTrack;
        }

        struct NeroV1Dao
        {
            /// <summary>
            /// "DAOI"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            /// Chunk size (big endian)
            /// </summary>
            public uint ChunkSizeBe;

            /// <summary>
            /// Chunk size (little endian)
            /// </summary>
            public uint ChunkSizeLe;

            /// <summary>
            /// UPC (14 bytes, null-padded)
            /// </summary>
            public byte[] Upc;

            /// <summary>
            /// TOC type
            /// </summary>
            public ushort TocType;

            /// <summary>
            /// First track
            /// </summary>
            public byte FirstTrack;

            /// <summary>
            /// Last track
            /// </summary>
            public byte LastTrack;

            /// <summary>
            /// Tracks
            /// </summary>
            public List<NeroV1DaoEntry> Tracks;
        }

        struct NeroV2DaoEntry
        {
            /// <summary>
            /// ISRC (12 bytes)
            /// </summary>
            public byte[] Isrc;

            /// <summary>
            /// Size of sector inside image (in bytes)
            /// </summary>
            public ushort SectorSize;

            /// <summary>
            /// Sector mode in image
            /// </summary>
            public ushort Mode;

            /// <summary>
            /// Seems to be always 0.
            /// </summary>
            public ushort Unknown;

            /// <summary>
            /// Index 0 start
            /// </summary>
            public ulong Index0;

            /// <summary>
            /// Index 1 start
            /// </summary>
            public ulong Index1;

            /// <summary>
            /// End of track + 1
            /// </summary>
            public ulong EndOfTrack;
        }

        struct NeroV2Dao
        {
            /// <summary>
            /// "DAOX"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            /// Chunk size (big endian)
            /// </summary>
            public uint ChunkSizeBe;

            /// <summary>
            /// Chunk size (little endian)
            /// </summary>
            public uint ChunkSizeLe;

            /// <summary>
            /// UPC (14 bytes, null-padded)
            /// </summary>
            public byte[] Upc;

            /// <summary>
            /// TOC type
            /// </summary>
            public ushort TocType;

            /// <summary>
            /// First track
            /// </summary>
            public byte FirstTrack;

            /// <summary>
            /// Last track
            /// </summary>
            public byte LastTrack;

            /// <summary>
            /// Tracks
            /// </summary>
            public List<NeroV2DaoEntry> Tracks;
        }

        struct NeroCdTextPack
        {
            /// <summary>
            /// Pack type
            /// </summary>
            public byte PackType;

            /// <summary>
            /// Track number
            /// </summary>
            public byte TrackNumber;

            /// <summary>
            /// Pack number in block
            /// </summary>
            public byte PackNumber;

            /// <summary>
            /// Block number
            /// </summary>
            public byte BlockNumber;

            /// <summary>
            /// 12 bytes of data
            /// </summary>
            public byte[] Text;

            /// <summary>
            /// CRC
            /// </summary>
            public ushort Crc;
        }

        struct NeroCdText
        {
            /// <summary>
            /// "CDTX"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            /// Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            /// CD-TEXT packs
            /// </summary>
            public List<NeroCdTextPack> Packs;
        }

        struct NeroV1TaoEntry
        {
            /// <summary>
            /// Offset of track on image
            /// </summary>
            public uint Offset;

            /// <summary>
            /// Length of track in bytes
            /// </summary>
            public uint Length;

            /// <summary>
            /// Track mode
            /// </summary>
            public uint Mode;

            /// <summary>
            /// LBA track start (plus 150 lead in sectors)
            /// </summary>
            public uint StartLba;

            /// <summary>
            /// Unknown
            /// </summary>
            public uint Unknown;
        }

        struct NeroV1Tao
        {
            /// <summary>
            /// "ETNF"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            /// Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            /// CD-TEXT packs
            /// </summary>
            public List<NeroV1TaoEntry> Tracks;
        }

        struct NeroV2TaoEntry
        {
            /// <summary>
            /// Offset of track on image
            /// </summary>
            public ulong Offset;

            /// <summary>
            /// Length of track in bytes
            /// </summary>
            public ulong Length;

            /// <summary>
            /// Track mode
            /// </summary>
            public uint Mode;

            /// <summary>
            /// LBA track start (plus 150 lead in sectors)
            /// </summary>
            public uint StartLba;

            /// <summary>
            /// Unknown
            /// </summary>
            public uint Unknown;

            /// <summary>
            /// Track length in sectors
            /// </summary>
            public uint Sectors;
        }

        struct NeroV2Tao
        {
            /// <summary>
            /// "ETN2"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            /// Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            /// CD-TEXT packs
            /// </summary>
            public List<NeroV2TaoEntry> Tracks;
        }

        struct NeroSession
        {
            /// <summary>
            /// "SINF"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            /// Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            /// Tracks in session
            /// </summary>
            public uint Tracks;
        }

        struct NeroMediaType
        {
            /// <summary>
            /// "MTYP"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            /// Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            /// Media type
            /// </summary>
            public uint Type;
        }

        struct NeroDiscInformation
        {
            /// <summary>
            /// "DINF"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            /// Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            /// Unknown
            /// </summary>
            public uint Unknown;
        }

        struct NeroTocChunk
        {
            /// <summary>
            /// "TOCT"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            /// Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            /// Unknown
            /// </summary>
            public ushort Unknown;
        }

        struct NeroReloChunk
        {
            /// <summary>
            /// "RELO"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            /// Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            /// Unknown
            /// </summary>
            public uint Unknown;
        }

        struct NeroEndOfChunkChain
        {
            /// <summary>
            /// "END!"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            /// Chunk size
            /// </summary>
            public uint ChunkSize;
        }

        // Internal use only
        struct NeroTrack
        {
            public byte[] Isrc;
            public ushort SectorSize;
            public ulong Offset;
            public ulong Length;
            public ulong EndOfTrack;
            public uint Mode;
            public ulong StartLba;
            public ulong Sectors;
            public ulong Index0;
            public ulong Index1;
            public uint Sequence;
        }
        #endregion

        #region Internal consts
        // "NERO"
        public const uint NeroV1FooterId = 0x4E45524F;

        // "NER5"
        public const uint NeroV2FooterId = 0x4E455235;

        // "CUES"
        public const uint NeroV1Cueid = 0x43554553;

        // "CUEX"
        public const uint NeroV2Cueid = 0x43554558;

        // "ETNF"
        public const uint NeroV1Taoid = 0x45544E46;

        // "ETN2"
        public const uint NeroV2Taoid = 0x45544E32;

        // "DAOI"
        public const uint NeroV1Daoid = 0x44414F49;

        // "DAOX"
        public const uint NeroV2Daoid = 0x44414F58;

        // "CDTX"
        public const uint NeroCdTextId = 0x43445458;

        // "SINF"
        public const uint NeroSessionId = 0x53494E46;

        // "MTYP"
        public const uint NeroDiskTypeId = 0x4D545950;

        // "DINF"
        public const uint NeroDiscInfoId = 0x44494E46;

        // "TOCT"
        public const uint NeroTocid = 0x544F4354;

        // "RELO"
        public const uint NeroReloId = 0x52454C4F;

        // "END!"
        public const uint NeroEndId = 0x454E4421;

        public enum DaoMode : ushort
        {
            Data = 0x0000,
            DataM2F1 = 0x0002,
            DataM2F2 = 0x0003,
            DataRaw = 0x0005,
            DataM2Raw = 0x0006,
            Audio = 0x0007,
            DataRawSub = 0x000F,
            AudioSub = 0x0010,
            DataM2RawSub = 0x0011
        }

        [Flags]
        public enum NeroMediaTypes : uint
        {
            /// <summary>
            /// No media
            /// </summary>
            NeroMtypNone = 0x00000,
            /// <summary>
            /// CD-R/RW
            /// </summary>
            NeroMtypCd = 0x00001,
            /// <summary>
            /// DDCD-R/RW
            /// </summary>
            NeroMtypDdcd = 0x00002,
            /// <summary>
            /// DVD-R/RW
            /// </summary>
            NeroMtypDvdM = 0x00004,
            /// <summary>
            /// DVD+RW
            /// </summary>
            NeroMtypDvdP = 0x00008,
            /// <summary>
            /// DVD-RAM
            /// </summary>
            NeroMtypDvdRam = 0x00010,
            /// <summary>
            /// Multi-level disc
            /// </summary>
            NeroMtypMl = 0x00020,
            /// <summary>
            /// Mount Rainier
            /// </summary>
            NeroMtypMrw = 0x00040,
            /// <summary>
            /// Exclude CD-R
            /// </summary>
            NeroMtypNoCdr = 0x00080,
            /// <summary>
            /// Exclude CD-RW
            /// </summary>
            NeroMtypNoCdrw = 0x00100,
            /// <summary>
            /// CD-RW
            /// </summary>
            NeroMtypCdrw = NeroMtypCd | NeroMtypNoCdr,
            /// <summary>
            /// CD-R
            /// </summary>
            NeroMtypCdr = NeroMtypCd | NeroMtypNoCdrw,
            /// <summary>
            /// DVD-ROM
            /// </summary>
            NeroMtypDvdRom = 0x00200,
            /// <summary>
            /// CD-ROM
            /// </summary>
            NeroMtypCdrom = 0x00400,
            /// <summary>
            /// Exclude DVD-RW
            /// </summary>
            NeroMtypNoDvdMRw = 0x00800,
            /// <summary>
            /// Exclude DVD-R
            /// </summary>
            NeroMtypNoDvdMR = 0x01000,
            /// <summary>
            /// Exclude DVD+RW
            /// </summary>
            NeroMtypNoDvdPRw = 0x02000,
            /// <summary>
            /// Exclude DVD+R
            /// </summary>
            NeroMtypNoDvdPR = 0x04000,
            /// <summary>
            /// DVD-R
            /// </summary>
            NeroMtypDvdMR = NeroMtypDvdM | NeroMtypNoDvdMRw,
            /// <summary>
            /// DVD-RW
            /// </summary>
            NeroMtypDvdMRw = NeroMtypDvdM | NeroMtypNoDvdMR,
            /// <summary>
            /// DVD+R
            /// </summary>
            NeroMtypDvdPR = NeroMtypDvdP | NeroMtypNoDvdPRw,
            /// <summary>
            /// DVD+RW
            /// </summary>
            NeroMtypDvdPRw = NeroMtypDvdP | NeroMtypNoDvdPR,
            /// <summary>
            /// Packet-writing (fixed)
            /// </summary>
            NeroMtypFpacket = 0x08000,
            /// <summary>
            /// Packet-writing (variable)
            /// </summary>
            NeroMtypVpacket = 0x10000,
            /// <summary>
            /// Packet-writing (any)
            /// </summary>
            NeroMtypPacketw = NeroMtypMrw | NeroMtypFpacket | NeroMtypVpacket,
            /// <summary>
            /// HD-Burn
            /// </summary>
            NeroMtypHdb = 0x20000,
            /// <summary>
            /// DVD+R DL
            /// </summary>
            NeroMtypDvdPR9 = 0x40000,
            /// <summary>
            /// DVD-R DL
            /// </summary>
            NeroMtypDvdMR9 = 0x80000,
            /// <summary>
            /// Any DVD double-layer
            /// </summary>
            NeroMtypDvdAnyR9 = NeroMtypDvdPR9 | NeroMtypDvdMR9,
            /// <summary>
            /// Any DVD
            /// </summary>
            NeroMtypDvdAny = NeroMtypDvdM | NeroMtypDvdP | NeroMtypDvdRam | NeroMtypDvdAnyR9,
            /// <summary>
            /// BD-ROM
            /// </summary>
            NeroMtypBdRom = 0x100000,
            /// <summary>
            /// BD-R
            /// </summary>
            NeroMtypBdR = 0x200000,
            /// <summary>
            /// BD-RE
            /// </summary>
            NeroMtypBdRe = 0x400000,
            /// <summary>
            /// BD-R/RE
            /// </summary>
            NeroMtypBd = NeroMtypBdR | NeroMtypBdRe,
            /// <summary>
            /// Any BD
            /// </summary>
            NeroMtypBdAny = NeroMtypBd | NeroMtypBdRom,
            /// <summary>
            /// HD DVD-ROM
            /// </summary>
            NeroMtypHdDvdRom = 0x0800000,
            /// <summary>
            /// HD DVD-R
            /// </summary>
            NeroMtypHdDvdR = 0x1000000,
            /// <summary>
            /// HD DVD-RW
            /// </summary>
            NeroMtypHdDvdRw = 0x2000000,
            /// <summary>
            /// HD DVD-R/RW
            /// </summary>
            NeroMtypHdDvd = NeroMtypHdDvdR | NeroMtypHdDvdRw,
            /// <summary>
            /// Any HD DVD
            /// </summary>
            NeroMtypHdDvdAny = NeroMtypHdDvd | NeroMtypHdDvdRom,
            /// <summary>
            /// Any DVD, old
            /// </summary>
            NeroMtypDvdAnyOld = NeroMtypDvdM | NeroMtypDvdP | NeroMtypDvdRam
        }
        #endregion

        #region Internal variables
        Filter imageFilter;
        Stream imageStream;
        bool imageNewFormat;
        Dictionary<ushort, uint> neroSessions;
        NeroV1Cuesheet neroCuesheetV1;
        NeroV2Cuesheet neroCuesheetV2;
        NeroV1Dao neroDaov1;
        NeroV2Dao neroDaov2;
        NeroCdText neroCdtxt;
        NeroV1Tao neroTaov1;
        NeroV2Tao neroTaov2;
        NeroMediaType neroMediaTyp;
        NeroDiscInformation neroDiscInfo;
        NeroTocChunk neroToc;
        NeroReloChunk neroRelo;

        List<Track> imageTracks;
        Dictionary<uint, byte[]> trackIsrCs;
        byte[] upc;
        Dictionary<uint, NeroTrack> neroTracks;
        Dictionary<uint, ulong> offsetmap;
        List<Session> imageSessions;
        List<Partition> imagePartitions;
        #endregion

        #region Methods
        public Nero()
        {
            Name = "Nero Burning ROM image";
            PluginUuid = new Guid("D160F9FF-5941-43FC-B037-AD81DD141F05");
            imageNewFormat = false;
            ImageInfo = new ImageInfo();
            ImageInfo.ReadableSectorTags = new List<SectorTagType>();
            ImageInfo.ReadableMediaTags = new List<MediaTagType>();
            neroSessions = new Dictionary<ushort, uint>();
            neroTracks = new Dictionary<uint, NeroTrack>();
            offsetmap = new Dictionary<uint, ulong>();
            imageSessions = new List<Session>();
            imagePartitions = new List<Partition>();
        }

        // Due to .cue format, this method must parse whole file, ignoring errors (those will be thrown by OpenImage()).
        public override bool IdentifyImage(Filter imageFilter)
        {
            imageStream = imageFilter.GetDataForkStream();
            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            byte[] buffer;
            NeroV1Footer footerV1 = new NeroV1Footer();
            NeroV2Footer footerV2 = new NeroV2Footer();

            imageStream.Seek(-8, SeekOrigin.End);
            buffer = new byte[8];
            imageStream.Read(buffer, 0, 8);
            footerV1.ChunkId = BigEndianBitConverter.ToUInt32(buffer, 0);
            footerV1.FirstChunkOffset = BigEndianBitConverter.ToUInt32(buffer, 4);

            imageStream.Seek(-12, SeekOrigin.End);
            buffer = new byte[12];
            imageStream.Read(buffer, 0, 12);
            footerV2.ChunkId = BigEndianBitConverter.ToUInt32(buffer, 0);
            footerV2.FirstChunkOffset = BigEndianBitConverter.ToUInt64(buffer, 4);

            DicConsole.DebugWriteLine("Nero plugin", "imageStream.Length = {0}", imageStream.Length);
            DicConsole.DebugWriteLine("Nero plugin", "footerV1.ChunkID = 0x{0:X8}", footerV1.ChunkId);
            DicConsole.DebugWriteLine("Nero plugin", "footerV1.FirstChunkOffset = {0}", footerV1.FirstChunkOffset);
            DicConsole.DebugWriteLine("Nero plugin", "footerV2.ChunkID = 0x{0:X8}", footerV2.ChunkId);
            DicConsole.DebugWriteLine("Nero plugin", "footerV2.FirstChunkOffset = {0}", footerV2.FirstChunkOffset);

            if(footerV2.ChunkId == NeroV2FooterId && footerV2.FirstChunkOffset < (ulong)imageStream.Length) return true;
            if(footerV1.ChunkId == NeroV1FooterId && footerV1.FirstChunkOffset < (ulong)imageStream.Length) return true;

            return false;
        }

        public override bool OpenImage(Filter imageFilter)
        {
            try
            {
                imageStream = imageFilter.GetDataForkStream();
                BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

                byte[] buffer;
                NeroV1Footer footerV1 = new NeroV1Footer();
                NeroV2Footer footerV2 = new NeroV2Footer();

                imageStream.Seek(-8, SeekOrigin.End);
                buffer = new byte[8];
                imageStream.Read(buffer, 0, 8);
                footerV1.ChunkId = BigEndianBitConverter.ToUInt32(buffer, 0);
                footerV1.FirstChunkOffset = BigEndianBitConverter.ToUInt32(buffer, 4);

                imageStream.Seek(-12, SeekOrigin.End);
                buffer = new byte[12];
                imageStream.Read(buffer, 0, 12);
                footerV2.ChunkId = BigEndianBitConverter.ToUInt32(buffer, 0);
                footerV2.FirstChunkOffset = BigEndianBitConverter.ToUInt64(buffer, 4);

                DicConsole.DebugWriteLine("Nero plugin", "imageStream.Length = {0}", imageStream.Length);
                DicConsole.DebugWriteLine("Nero plugin", "footerV1.ChunkID = 0x{0:X8} (\"{1}\")", footerV1.ChunkId,
                                          System.Text.Encoding.ASCII
                                                .GetString(BigEndianBitConverter.GetBytes(footerV1.ChunkId)));
                DicConsole.DebugWriteLine("Nero plugin", "footerV1.FirstChunkOffset = {0}", footerV1.FirstChunkOffset);
                DicConsole.DebugWriteLine("Nero plugin", "footerV2.ChunkID = 0x{0:X8} (\"{1}\")", footerV2.ChunkId,
                                          System.Text.Encoding.ASCII
                                                .GetString(BigEndianBitConverter.GetBytes(footerV2.ChunkId)));
                DicConsole.DebugWriteLine("Nero plugin", "footerV2.FirstChunkOffset = {0}", footerV2.FirstChunkOffset);

                if(footerV1.ChunkId == NeroV1FooterId && footerV1.FirstChunkOffset < (ulong)imageStream.Length)
                    imageNewFormat = false;
                else if(footerV2.ChunkId == NeroV2FooterId && footerV2.FirstChunkOffset < (ulong)imageStream.Length)
                    imageNewFormat = true;
                else
                {
                    DicConsole.DebugWrite("Nero plugin", "Nero version not recognized.");
                    return false;
                }

                if(imageNewFormat) imageStream.Seek((long)footerV2.FirstChunkOffset, SeekOrigin.Begin);
                else imageStream.Seek(footerV1.FirstChunkOffset, SeekOrigin.Begin);

                bool parsing = true;
                ushort currentsession = 1;
                uint currenttrack = 1;

                imageTracks = new List<Track>();
                trackIsrCs = new Dictionary<uint, byte[]>();

                ImageInfo.MediaType = MediaType.CD;
                ImageInfo.Sectors = 0;
                ImageInfo.SectorSize = 0;

                while(parsing)
                {
                    byte[] chunkHeaderBuffer = new byte[8];
                    uint chunkId;
                    uint chunkLength;

                    imageStream.Read(chunkHeaderBuffer, 0, 8);
                    chunkId = BigEndianBitConverter.ToUInt32(chunkHeaderBuffer, 0);
                    chunkLength = BigEndianBitConverter.ToUInt32(chunkHeaderBuffer, 4);

                    DicConsole.DebugWriteLine("Nero plugin", "ChunkID = 0x{0:X8} (\"{1}\")", chunkId,
                                              System.Text.Encoding.ASCII
                                                    .GetString(BigEndianBitConverter.GetBytes(chunkId)));
                    DicConsole.DebugWriteLine("Nero plugin", "ChunkLength = {0}", chunkLength);

                    switch(chunkId)
                    {
                        case NeroV1Cueid:
                        {
                            DicConsole.DebugWriteLine("Nero plugin", "Found \"CUES\" chunk, parsing {0} bytes",
                                                      chunkLength);

                            neroCuesheetV1 = new NeroV1Cuesheet();
                            neroCuesheetV1.ChunkId = chunkId;
                            neroCuesheetV1.ChunkSize = chunkLength;
                            neroCuesheetV1.Entries = new List<NeroV1CueEntry>();

                            byte[] tmpbuffer = new byte[8];
                            for(int i = 0; i < neroCuesheetV1.ChunkSize; i += 8)
                            {
                                NeroV1CueEntry entry = new NeroV1CueEntry();
                                imageStream.Read(tmpbuffer, 0, 8);
                                entry.Mode = tmpbuffer[0];
                                entry.TrackNumber = tmpbuffer[1];
                                entry.IndexNumber = tmpbuffer[2];
                                entry.Dummy = BigEndianBitConverter.ToUInt16(tmpbuffer, 3);
                                entry.Minute = tmpbuffer[5];
                                entry.Second = tmpbuffer[6];
                                entry.Frame = tmpbuffer[7];

                                DicConsole.DebugWriteLine("Nero plugin", "Cuesheet entry {0}", i / 8 + 1);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Mode = {1:X2}", i / 8 + 1,
                                                          entry.Mode);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].TrackNumber = {1:X2}",
                                                          i / 8 + 1, entry.TrackNumber);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].IndexNumber = {1:X2}",
                                                          i / 8 + 1, entry.IndexNumber);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Dummy = {1:X4}", i / 8 + 1,
                                                          entry.Dummy);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Minute = {1:X2}", i / 8 + 1,
                                                          entry.Minute);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Second = {1:X2}", i / 8 + 1,
                                                          entry.Second);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Frame = {1:X2}", i / 8 + 1,
                                                          entry.Frame);

                                neroCuesheetV1.Entries.Add(entry);
                            }

                            break;
                        }
                        case NeroV2Cueid:
                        {
                            DicConsole.DebugWriteLine("Nero plugin", "Found \"CUEX\" chunk, parsing {0} bytes",
                                                      chunkLength);

                            neroCuesheetV2 = new NeroV2Cuesheet();
                            neroCuesheetV2.ChunkId = chunkId;
                            neroCuesheetV2.ChunkSize = chunkLength;
                            neroCuesheetV2.Entries = new List<NeroV2CueEntry>();

                            byte[] tmpbuffer = new byte[8];
                            for(int i = 0; i < neroCuesheetV2.ChunkSize; i += 8)
                            {
                                NeroV2CueEntry entry = new NeroV2CueEntry();
                                imageStream.Read(tmpbuffer, 0, 8);
                                entry.Mode = tmpbuffer[0];
                                entry.TrackNumber = tmpbuffer[1];
                                entry.IndexNumber = tmpbuffer[2];
                                entry.Dummy = tmpbuffer[3];
                                entry.LbaStart = BigEndianBitConverter.ToInt32(tmpbuffer, 4);

                                DicConsole.DebugWriteLine("Nero plugin", "Cuesheet entry {0}", i / 8 + 1);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Mode = 0x{1:X2}", i / 8 + 1,
                                                          entry.Mode);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].TrackNumber = {1:X2}",
                                                          i / 8 + 1, entry.TrackNumber);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].IndexNumber = {1:X2}",
                                                          i / 8 + 1, entry.IndexNumber);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Dummy = {1:X2}", i / 8 + 1,
                                                          entry.Dummy);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].LBAStart = {1}", i / 8 + 1,
                                                          entry.LbaStart);

                                neroCuesheetV2.Entries.Add(entry);
                            }

                            break;
                        }
                        case NeroV1Daoid:
                        {
                            DicConsole.DebugWriteLine("Nero plugin", "Found \"DAOI\" chunk, parsing {0} bytes",
                                                      chunkLength);

                            neroDaov1 = new NeroV1Dao();
                            neroDaov1.ChunkId = chunkId;
                            neroDaov1.ChunkSizeBe = chunkLength;

                            byte[] tmpbuffer = new byte[22];
                            imageStream.Read(tmpbuffer, 0, 22);
                            neroDaov1.ChunkSizeLe = BigEndianBitConverter.ToUInt32(tmpbuffer, 0);
                            neroDaov1.Upc = new byte[14];
                            Array.Copy(tmpbuffer, 4, neroDaov1.Upc, 0, 14);
                            neroDaov1.TocType = BigEndianBitConverter.ToUInt16(tmpbuffer, 18);
                            neroDaov1.FirstTrack = tmpbuffer[20];
                            neroDaov1.LastTrack = tmpbuffer[21];
                            neroDaov1.Tracks = new List<NeroV1DaoEntry>();

                            if(!ImageInfo.ReadableMediaTags.Contains(MediaTagType.CD_MCN))
                                ImageInfo.ReadableMediaTags.Add(MediaTagType.CD_MCN);

                            if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdTrackIsrc))
                                ImageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackIsrc);

                            DicConsole.DebugWriteLine("Nero plugin", "neroDAOV1.ChunkSizeLe = {0} bytes",
                                                      neroDaov1.ChunkSizeLe);
                            DicConsole.DebugWriteLine("Nero plugin", "neroDAOV1.UPC = \"{0}\"",
                                                      StringHandlers.CToString(neroDaov1.Upc));
                            DicConsole.DebugWriteLine("Nero plugin", "neroDAOV1.TocType = 0x{0:X4}", neroDaov1.TocType);
                            DicConsole.DebugWriteLine("Nero plugin", "neroDAOV1.FirstTrack = {0}",
                                                      neroDaov1.FirstTrack);
                            DicConsole.DebugWriteLine("Nero plugin", "neroDAOV1.LastTrack = {0}", neroDaov1.LastTrack);

                            upc = neroDaov1.Upc;

                            tmpbuffer = new byte[30];
                            for(int i = 0; i < neroDaov1.ChunkSizeBe - 22; i += 30)
                            {
                                NeroV1DaoEntry entry = new NeroV1DaoEntry();
                                imageStream.Read(tmpbuffer, 0, 30);
                                entry.Isrc = new byte[12];
                                Array.Copy(tmpbuffer, 4, entry.Isrc, 0, 12);
                                entry.SectorSize = BigEndianBitConverter.ToUInt16(tmpbuffer, 12);
                                entry.Mode = BitConverter.ToUInt16(tmpbuffer, 14);
                                entry.Unknown = BigEndianBitConverter.ToUInt16(tmpbuffer, 16);
                                entry.Index0 = BigEndianBitConverter.ToUInt32(tmpbuffer, 18);
                                entry.Index1 = BigEndianBitConverter.ToUInt32(tmpbuffer, 22);
                                entry.EndOfTrack = BigEndianBitConverter.ToUInt32(tmpbuffer, 26);

                                DicConsole.DebugWriteLine("Nero plugin", "Disc-At-Once entry {0}", i / 32 + 1);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].ISRC = \"{1}\"", i / 32 + 1,
                                                          StringHandlers.CToString(entry.Isrc));
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].SectorSize = {1}",
                                                          i / 32 + 1, entry.SectorSize);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Mode = {1} (0x{2:X4})",
                                                          i / 32 + 1, (DaoMode)entry.Mode, entry.Mode);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Unknown = 0x{1:X4}",
                                                          i / 32 + 1, entry.Unknown);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Index0 = {1}", i / 32 + 1,
                                                          entry.Index0);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Index1 = {1}", i / 32 + 1,
                                                          entry.Index1);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].EndOfTrack = {1}",
                                                          i / 32 + 1, entry.EndOfTrack);

                                neroDaov1.Tracks.Add(entry);

                                if(entry.SectorSize > ImageInfo.SectorSize) ImageInfo.SectorSize = entry.SectorSize;

                                trackIsrCs.Add(currenttrack, entry.Isrc);
                                if(currenttrack == 1) entry.Index0 = entry.Index1;

                                NeroTrack neroTrack = new NeroTrack();
                                neroTrack.EndOfTrack = entry.EndOfTrack;
                                neroTrack.Isrc = entry.Isrc;
                                neroTrack.Length = entry.EndOfTrack - entry.Index0;
                                neroTrack.Mode = entry.Mode;
                                neroTrack.Offset = entry.Index0;
                                neroTrack.Sectors = neroTrack.Length / entry.SectorSize;
                                neroTrack.SectorSize = entry.SectorSize;
                                neroTrack.StartLba = ImageInfo.Sectors;
                                neroTrack.Index0 = entry.Index0;
                                neroTrack.Index1 = entry.Index1;
                                neroTrack.Sequence = currenttrack;
                                neroTracks.Add(currenttrack, neroTrack);

                                ImageInfo.Sectors += neroTrack.Sectors;

                                currenttrack++;
                            }

                            break;
                        }
                        case NeroV2Daoid:
                        {
                            DicConsole.DebugWriteLine("Nero plugin", "Found \"DAOX\" chunk, parsing {0} bytes",
                                                      chunkLength);

                            neroDaov2 = new NeroV2Dao();
                            neroDaov2.ChunkId = chunkId;
                            neroDaov2.ChunkSizeBe = chunkLength;

                            byte[] tmpbuffer = new byte[22];
                            imageStream.Read(tmpbuffer, 0, 22);
                            neroDaov2.ChunkSizeLe = BigEndianBitConverter.ToUInt32(tmpbuffer, 0);
                            neroDaov2.Upc = new byte[14];
                            Array.Copy(tmpbuffer, 4, neroDaov2.Upc, 0, 14);
                            neroDaov2.TocType = BigEndianBitConverter.ToUInt16(tmpbuffer, 18);
                            neroDaov2.FirstTrack = tmpbuffer[20];
                            neroDaov2.LastTrack = tmpbuffer[21];
                            neroDaov2.Tracks = new List<NeroV2DaoEntry>();

                            if(!ImageInfo.ReadableMediaTags.Contains(MediaTagType.CD_MCN))
                                ImageInfo.ReadableMediaTags.Add(MediaTagType.CD_MCN);

                            if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdTrackIsrc))
                                ImageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackIsrc);

                            upc = neroDaov2.Upc;

                            DicConsole.DebugWriteLine("Nero plugin", "neroDAOV2.ChunkSizeLe = {0} bytes",
                                                      neroDaov2.ChunkSizeLe);
                            DicConsole.DebugWriteLine("Nero plugin", "neroDAOV2.UPC = \"{0}\"",
                                                      StringHandlers.CToString(neroDaov2.Upc));
                            DicConsole.DebugWriteLine("Nero plugin", "neroDAOV2.TocType = 0x{0:X4}", neroDaov2.TocType);
                            DicConsole.DebugWriteLine("Nero plugin", "neroDAOV2.FirstTrack = {0}",
                                                      neroDaov2.FirstTrack);
                            DicConsole.DebugWriteLine("Nero plugin", "neroDAOV2.LastTrack = {0}", neroDaov2.LastTrack);

                            tmpbuffer = new byte[42];
                            for(int i = 0; i < neroDaov2.ChunkSizeBe - 22; i += 42)
                            {
                                NeroV2DaoEntry entry = new NeroV2DaoEntry();
                                imageStream.Read(tmpbuffer, 0, 42);
                                entry.Isrc = new byte[12];
                                Array.Copy(tmpbuffer, 4, entry.Isrc, 0, 12);
                                entry.SectorSize = BigEndianBitConverter.ToUInt16(tmpbuffer, 12);
                                entry.Mode = BitConverter.ToUInt16(tmpbuffer, 14);
                                entry.Unknown = BigEndianBitConverter.ToUInt16(tmpbuffer, 16);
                                entry.Index0 = BigEndianBitConverter.ToUInt64(tmpbuffer, 18);
                                entry.Index1 = BigEndianBitConverter.ToUInt64(tmpbuffer, 26);
                                entry.EndOfTrack = BigEndianBitConverter.ToUInt64(tmpbuffer, 34);

                                DicConsole.DebugWriteLine("Nero plugin", "Disc-At-Once entry {0}", i / 32 + 1);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].ISRC = \"{1}\"", i / 32 + 1,
                                                          StringHandlers.CToString(entry.Isrc));
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].SectorSize = {1}",
                                                          i / 32 + 1, entry.SectorSize);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Mode = {1} (0x{2:X4})",
                                                          i / 32 + 1, (DaoMode)entry.Mode, entry.Mode);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Unknown = {1:X2}",
                                                          i / 32 + 1, entry.Unknown);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Index0 = {1}", i / 32 + 1,
                                                          entry.Index0);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Index1 = {1}", i / 32 + 1,
                                                          entry.Index1);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].EndOfTrack = {1}",
                                                          i / 32 + 1, entry.EndOfTrack);

                                neroDaov2.Tracks.Add(entry);

                                if(entry.SectorSize > ImageInfo.SectorSize) ImageInfo.SectorSize = entry.SectorSize;

                                trackIsrCs.Add(currenttrack, entry.Isrc);

                                if(currenttrack == 1) entry.Index0 = entry.Index1;

                                NeroTrack neroTrack = new NeroTrack();
                                neroTrack.EndOfTrack = entry.EndOfTrack;
                                neroTrack.Isrc = entry.Isrc;
                                neroTrack.Length = entry.EndOfTrack - entry.Index0;
                                neroTrack.Mode = entry.Mode;
                                neroTrack.Offset = entry.Index0;
                                neroTrack.Sectors = neroTrack.Length / entry.SectorSize;
                                neroTrack.SectorSize = entry.SectorSize;
                                neroTrack.StartLba = ImageInfo.Sectors;
                                neroTrack.Index0 = entry.Index0;
                                neroTrack.Index1 = entry.Index1;
                                neroTrack.Sequence = currenttrack;
                                neroTracks.Add(currenttrack, neroTrack);

                                ImageInfo.Sectors += neroTrack.Sectors;

                                currenttrack++;
                            }

                            break;
                        }
                        case NeroCdTextId:
                        {
                            DicConsole.DebugWriteLine("Nero plugin", "Found \"CDTX\" chunk, parsing {0} bytes",
                                                      chunkLength);

                            neroCdtxt = new NeroCdText();
                            neroCdtxt.ChunkId = chunkId;
                            neroCdtxt.ChunkSize = chunkLength;
                            neroCdtxt.Packs = new List<NeroCdTextPack>();

                            byte[] tmpbuffer = new byte[18];
                            for(int i = 0; i < neroCdtxt.ChunkSize; i += 18)
                            {
                                NeroCdTextPack entry = new NeroCdTextPack();
                                imageStream.Read(tmpbuffer, 0, 18);

                                entry.PackType = tmpbuffer[0];
                                entry.TrackNumber = tmpbuffer[1];
                                entry.PackNumber = tmpbuffer[2];
                                entry.BlockNumber = tmpbuffer[3];
                                entry.Text = new byte[12];
                                Array.Copy(tmpbuffer, 4, entry.Text, 0, 12);
                                entry.Crc = BigEndianBitConverter.ToUInt16(tmpbuffer, 16);

                                DicConsole.DebugWriteLine("Nero plugin", "CD-TEXT entry {0}", i / 18 + 1);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].PackType = 0x{1:X2}",
                                                          i / 18 + 1, entry.PackType);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].TrackNumber = 0x{1:X2}",
                                                          i / 18 + 1, entry.TrackNumber);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].PackNumber = 0x{1:X2}",
                                                          i / 18 + 1, entry.PackNumber);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].BlockNumber = 0x{1:X2}",
                                                          i / 18 + 1, entry.BlockNumber);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Text = \"{1}\"", i / 18 + 1,
                                                          StringHandlers.CToString(entry.Text));
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].CRC = 0x{1:X4}", i / 18 + 1,
                                                          entry.Crc);

                                neroCdtxt.Packs.Add(entry);
                            }

                            break;
                        }
                        case NeroV1Taoid:
                        {
                            DicConsole.DebugWriteLine("Nero plugin", "Found \"ETNF\" chunk, parsing {0} bytes",
                                                      chunkLength);

                            neroTaov1 = new NeroV1Tao();
                            neroTaov1.ChunkId = chunkId;
                            neroTaov1.ChunkSize = chunkLength;
                            neroTaov1.Tracks = new List<NeroV1TaoEntry>();

                            byte[] tmpbuffer = new byte[20];
                            for(int i = 0; i < neroTaov1.ChunkSize; i += 20)
                            {
                                NeroV1TaoEntry entry = new NeroV1TaoEntry();
                                imageStream.Read(tmpbuffer, 0, 20);

                                entry.Offset = BigEndianBitConverter.ToUInt32(tmpbuffer, 0);
                                entry.Length = BigEndianBitConverter.ToUInt32(tmpbuffer, 4);
                                entry.Mode = BigEndianBitConverter.ToUInt32(tmpbuffer, 8);
                                entry.StartLba = BigEndianBitConverter.ToUInt32(tmpbuffer, 12);
                                entry.Unknown = BigEndianBitConverter.ToUInt32(tmpbuffer, 16);

                                DicConsole.DebugWriteLine("Nero plugin", "Track-at-Once entry {0}", i / 20 + 1);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Offset = {1}", i / 20 + 1,
                                                          entry.Offset);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Length = {1} bytes",
                                                          i / 20 + 1, entry.Length);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Mode = {1} (0x{2:X4})",
                                                          i / 20 + 1, (DaoMode)entry.Mode, entry.Mode);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].StartLBA = {1}", i / 20 + 1,
                                                          entry.StartLba);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Unknown = 0x{1:X4}",
                                                          i / 20 + 1, entry.Unknown);

                                neroTaov1.Tracks.Add(entry);

                                if(NeroTrackModeToBytesPerSector((DaoMode)entry.Mode) > ImageInfo.SectorSize)
                                    ImageInfo.SectorSize = NeroTrackModeToBytesPerSector((DaoMode)entry.Mode);

                                NeroTrack neroTrack = new NeroTrack();
                                neroTrack.EndOfTrack = entry.Offset + entry.Length;
                                neroTrack.Isrc = new byte[12];
                                neroTrack.Length = entry.Length;
                                neroTrack.Mode = entry.Mode;
                                neroTrack.Offset = entry.Offset;
                                neroTrack.Sectors =
                                    neroTrack.Length / NeroTrackModeToBytesPerSector((DaoMode)entry.Mode);
                                neroTrack.SectorSize = NeroTrackModeToBytesPerSector((DaoMode)entry.Mode);
                                neroTrack.StartLba = ImageInfo.Sectors;
                                neroTrack.Index0 = entry.Offset;
                                neroTrack.Index1 = entry.Offset;
                                neroTrack.Sequence = currenttrack;
                                neroTracks.Add(currenttrack, neroTrack);

                                ImageInfo.Sectors += neroTrack.Sectors;

                                currenttrack++;
                            }

                            break;
                        }
                        case NeroV2Taoid:
                        {
                            DicConsole.DebugWriteLine("Nero plugin", "Found \"ETN2\" chunk, parsing {0} bytes",
                                                      chunkLength);

                            neroTaov2 = new NeroV2Tao();
                            neroTaov2.ChunkId = chunkId;
                            neroTaov2.ChunkSize = chunkLength;
                            neroTaov2.Tracks = new List<NeroV2TaoEntry>();

                            byte[] tmpbuffer = new byte[32];
                            for(int i = 0; i < neroTaov2.ChunkSize; i += 32)
                            {
                                NeroV2TaoEntry entry = new NeroV2TaoEntry();
                                imageStream.Read(tmpbuffer, 0, 32);

                                entry.Offset = BigEndianBitConverter.ToUInt64(tmpbuffer, 0);
                                entry.Length = BigEndianBitConverter.ToUInt64(tmpbuffer, 8);
                                entry.Mode = BigEndianBitConverter.ToUInt32(tmpbuffer, 16);
                                entry.StartLba = BigEndianBitConverter.ToUInt32(tmpbuffer, 20);
                                entry.Unknown = BigEndianBitConverter.ToUInt32(tmpbuffer, 24);
                                entry.Sectors = BigEndianBitConverter.ToUInt32(tmpbuffer, 28);

                                DicConsole.DebugWriteLine("Nero plugin", "Track-at-Once entry {0}", i / 32 + 1);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Offset = {1}", i / 32 + 1,
                                                          entry.Offset);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Length = {1} bytes",
                                                          i / 32 + 1, entry.Length);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Mode = {1} (0x{2:X4})",
                                                          i / 32 + 1, (DaoMode)entry.Mode, entry.Mode);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].StartLBA = {1}", i / 32 + 1,
                                                          entry.StartLba);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Unknown = 0x{1:X4}",
                                                          i / 32 + 1, entry.Unknown);
                                DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Sectors = {1}", i / 32 + 1,
                                                          entry.Sectors);

                                neroTaov2.Tracks.Add(entry);

                                if(NeroTrackModeToBytesPerSector((DaoMode)entry.Mode) > ImageInfo.SectorSize)
                                    ImageInfo.SectorSize = NeroTrackModeToBytesPerSector((DaoMode)entry.Mode);

                                NeroTrack neroTrack = new NeroTrack();
                                neroTrack.EndOfTrack = entry.Offset + entry.Length;
                                neroTrack.Isrc = new byte[12];
                                neroTrack.Length = entry.Length;
                                neroTrack.Mode = entry.Mode;
                                neroTrack.Offset = entry.Offset;
                                neroTrack.Sectors =
                                    neroTrack.Length / NeroTrackModeToBytesPerSector((DaoMode)entry.Mode);
                                neroTrack.SectorSize = NeroTrackModeToBytesPerSector((DaoMode)entry.Mode);
                                neroTrack.StartLba = ImageInfo.Sectors;
                                neroTrack.Index0 = entry.Offset;
                                neroTrack.Index1 = entry.Offset;
                                neroTrack.Sequence = currenttrack;
                                neroTracks.Add(currenttrack, neroTrack);

                                ImageInfo.Sectors += neroTrack.Sectors;

                                currenttrack++;
                            }

                            break;
                        }
                        case NeroSessionId:
                        {
                            DicConsole.DebugWriteLine("Nero plugin", "Found \"SINF\" chunk, parsing {0} bytes",
                                                      chunkLength);

                            uint sessionTracks;
                            byte[] tmpbuffer = new byte[4];
                            imageStream.Read(tmpbuffer, 0, 4);
                            sessionTracks = BigEndianBitConverter.ToUInt32(tmpbuffer, 0);
                            neroSessions.Add(currentsession, sessionTracks);

                            DicConsole.DebugWriteLine("Nero plugin", "\tSession {0} has {1} tracks", currentsession,
                                                      sessionTracks);

                            currentsession++;
                            break;
                        }
                        case NeroDiskTypeId:
                        {
                            DicConsole.DebugWriteLine("Nero plugin", "Found \"MTYP\" chunk, parsing {0} bytes",
                                                      chunkLength);

                            neroMediaTyp = new NeroMediaType();

                            neroMediaTyp.ChunkId = chunkId;
                            neroMediaTyp.ChunkSize = chunkLength;

                            byte[] tmpbuffer = new byte[4];
                            imageStream.Read(tmpbuffer, 0, 4);
                            neroMediaTyp.Type = BigEndianBitConverter.ToUInt32(tmpbuffer, 0);

                            DicConsole.DebugWriteLine("Nero plugin", "\tMedia type is {0} ({1})",
                                                      (NeroMediaTypes)neroMediaTyp.Type, neroMediaTyp.Type);

                            ImageInfo.MediaType = NeroMediaTypeToMediaType((NeroMediaTypes)neroMediaTyp.Type);

                            break;
                        }
                        case NeroDiscInfoId:
                        {
                            DicConsole.DebugWriteLine("Nero plugin", "Found \"DINF\" chunk, parsing {0} bytes",
                                                      chunkLength);

                            neroDiscInfo = new NeroDiscInformation();
                            neroDiscInfo.ChunkId = chunkId;
                            neroDiscInfo.ChunkSize = chunkLength;
                            byte[] tmpbuffer = new byte[4];
                            imageStream.Read(tmpbuffer, 0, 4);
                            neroDiscInfo.Unknown = BigEndianBitConverter.ToUInt32(tmpbuffer, 0);

                            DicConsole.DebugWriteLine("Nero plugin", "\tneroDiscInfo.Unknown = 0x{0:X4} ({0})",
                                                      neroDiscInfo.Unknown);

                            break;
                        }
                        case NeroReloId:
                        {
                            DicConsole.DebugWriteLine("Nero plugin", "Found \"RELO\" chunk, parsing {0} bytes",
                                                      chunkLength);

                            neroRelo = new NeroReloChunk();
                            neroRelo.ChunkId = chunkId;
                            neroRelo.ChunkSize = chunkLength;
                            byte[] tmpbuffer = new byte[4];
                            imageStream.Read(tmpbuffer, 0, 4);
                            neroRelo.Unknown = BigEndianBitConverter.ToUInt32(tmpbuffer, 0);

                            DicConsole.DebugWriteLine("Nero plugin", "\tneroRELO.Unknown = 0x{0:X4} ({0})",
                                                      neroRelo.Unknown);

                            break;
                        }
                        case NeroTocid:
                        {
                            DicConsole.DebugWriteLine("Nero plugin", "Found \"TOCT\" chunk, parsing {0} bytes",
                                                      chunkLength);

                            neroToc = new NeroTocChunk();
                            neroToc.ChunkId = chunkId;
                            neroToc.ChunkSize = chunkLength;
                            byte[] tmpbuffer = new byte[2];
                            imageStream.Read(tmpbuffer, 0, 2);
                            neroToc.Unknown = BigEndianBitConverter.ToUInt16(tmpbuffer, 0);

                            DicConsole.DebugWriteLine("Nero plugin", "\tneroTOC.Unknown = 0x{0:X4} ({0})",
                                                      neroToc.Unknown);

                            break;
                        }
                        case NeroEndId:
                        {
                            DicConsole.DebugWriteLine("Nero plugin", "Found \"END!\" chunk, finishing parse");
                            parsing = false;
                            break;
                        }
                        default:
                        {
                            DicConsole.DebugWriteLine("Nero plugin", "Unknown chunk ID \"{0}\", skipping...",
                                                      System.Text.Encoding.ASCII.GetString(BigEndianBitConverter
                                                                                               .GetBytes(chunkId)));
                            imageStream.Seek(chunkLength, SeekOrigin.Current);
                            break;
                        }
                    }
                }

                ImageInfo.ImageHasPartitions = true;
                ImageInfo.ImageHasSessions = true;
                ImageInfo.ImageCreator = null;
                ImageInfo.ImageCreationTime = imageFilter.GetCreationTime();
                ImageInfo.ImageLastModificationTime = imageFilter.GetLastWriteTime();
                ImageInfo.ImageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
                ImageInfo.ImageComments = null;
                ImageInfo.MediaManufacturer = null;
                ImageInfo.MediaModel = null;
                ImageInfo.MediaSerialNumber = null;
                ImageInfo.MediaBarcode = null;
                ImageInfo.MediaPartNumber = null;
                ImageInfo.DriveManufacturer = null;
                ImageInfo.DriveModel = null;
                ImageInfo.DriveSerialNumber = null;
                ImageInfo.DriveFirmwareRevision = null;
                ImageInfo.MediaSequence = 0;
                ImageInfo.LastMediaSequence = 0;
                if(imageNewFormat)
                {
                    ImageInfo.ImageSize = footerV2.FirstChunkOffset;
                    ImageInfo.ImageVersion = "Nero Burning ROM >= 5.5";
                    ImageInfo.ImageApplication = "Nero Burning ROM";
                    ImageInfo.ImageApplicationVersion = ">= 5.5";
                }
                else
                {
                    ImageInfo.ImageSize = footerV1.FirstChunkOffset;
                    ImageInfo.ImageVersion = "Nero Burning ROM <= 5.0";
                    ImageInfo.ImageApplication = "Nero Burning ROM";
                    ImageInfo.ImageApplicationVersion = "<= 5.0";
                }

                if(neroSessions.Count == 0) neroSessions.Add(1, currenttrack);

                DicConsole.DebugWriteLine("Nero plugin", "Building offset, track and session maps");

                currentsession = 1;
                uint currentsessionmaxtrack;
                neroSessions.TryGetValue(1, out currentsessionmaxtrack);
                uint currentsessioncurrenttrack = 1;
                Session currentsessionstruct = new Session();
                ulong partitionSequence = 0;
                ulong partitionStartByte = 0;
                for(uint i = 1; i <= neroTracks.Count; i++)
                {
                    NeroTrack neroTrack;
                    if(neroTracks.TryGetValue(i, out neroTrack))
                    {
                        DicConsole.DebugWriteLine("Nero plugin", "\tcurrentsession = {0}", currentsession);
                        DicConsole.DebugWriteLine("Nero plugin", "\tcurrentsessionmaxtrack = {0}",
                                                  currentsessionmaxtrack);
                        DicConsole.DebugWriteLine("Nero plugin", "\tcurrentsessioncurrenttrack = {0}",
                                                  currentsessioncurrenttrack);

                        Track _track = new Track();
                        if(neroTrack.Sequence == 1) neroTrack.Index0 = neroTrack.Index1;

                        _track.Indexes = new Dictionary<int, ulong>();
                        if(neroTrack.Index0 < neroTrack.Index1)
                            _track.Indexes.Add(0, neroTrack.Index0 / neroTrack.SectorSize);
                        _track.Indexes.Add(1, neroTrack.Index1 / neroTrack.SectorSize);
                        _track.TrackDescription = StringHandlers.CToString(neroTrack.Isrc);
                        _track.TrackEndSector = neroTrack.Length / neroTrack.SectorSize + neroTrack.StartLba - 1;
                        _track.TrackPregap = (neroTrack.Index1 - neroTrack.Index0) / neroTrack.SectorSize;
                        _track.TrackSequence = neroTrack.Sequence;
                        _track.TrackSession = currentsession;
                        _track.TrackStartSector = neroTrack.StartLba;
                        _track.TrackType = NeroTrackModeToTrackType((DaoMode)neroTrack.Mode);
                        _track.TrackFile = imageFilter.GetFilename();
                        _track.TrackFilter = imageFilter;
                        _track.TrackFileOffset = neroTrack.Offset;
                        _track.TrackFileType = "BINARY";
                        _track.TrackSubchannelType = TrackSubchannelType.None;
                        switch((DaoMode)neroTrack.Mode)
                        {
                            case DaoMode.Audio:
                                _track.TrackBytesPerSector = 2352;
                                _track.TrackRawBytesPerSector = 2352;
                                break;
                            case DaoMode.AudioSub:
                                _track.TrackBytesPerSector = 2352;
                                _track.TrackRawBytesPerSector = 2448;
                                _track.TrackSubchannelType = TrackSubchannelType.RawInterleaved;
                                break;
                            case DaoMode.Data:
                            case DaoMode.DataM2F1:
                                _track.TrackBytesPerSector = 2048;
                                _track.TrackRawBytesPerSector = 2048;
                                break;
                            case DaoMode.DataM2F2:
                                _track.TrackBytesPerSector = 2336;
                                _track.TrackRawBytesPerSector = 2336;
                                break;
                            case DaoMode.DataM2Raw:
                                _track.TrackBytesPerSector = 2352;
                                _track.TrackRawBytesPerSector = 2352;
                                break;
                            case DaoMode.DataM2RawSub:
                                _track.TrackBytesPerSector = 2352;
                                _track.TrackRawBytesPerSector = 2448;
                                _track.TrackSubchannelType = TrackSubchannelType.RawInterleaved;
                                break;
                            case DaoMode.DataRaw:
                                _track.TrackBytesPerSector = 2048;
                                _track.TrackRawBytesPerSector = 2352;
                                break;
                            case DaoMode.DataRawSub:
                                _track.TrackBytesPerSector = 2048;
                                _track.TrackRawBytesPerSector = 2448;
                                _track.TrackSubchannelType = TrackSubchannelType.RawInterleaved;
                                break;
                        }

                        if(_track.TrackSubchannelType == TrackSubchannelType.RawInterleaved)
                        {
                            _track.TrackSubchannelFilter = imageFilter;
                            _track.TrackSubchannelFile = imageFilter.GetFilename();
                            _track.TrackSubchannelOffset = neroTrack.Offset;
                        }

                        imageTracks.Add(_track);

                        DicConsole.DebugWriteLine("Nero plugin", "\t\t _track.TrackDescription = {0}",
                                                  _track.TrackDescription);
                        DicConsole.DebugWriteLine("Nero plugin", "\t\t _track.TrackEndSector = {0}",
                                                  _track.TrackEndSector);
                        DicConsole.DebugWriteLine("Nero plugin", "\t\t _track.TrackPregap = {0}", _track.TrackPregap);
                        DicConsole.DebugWriteLine("Nero plugin", "\t\t _track.TrackSequence = {0}",
                                                  _track.TrackSequence);
                        DicConsole.DebugWriteLine("Nero plugin", "\t\t _track.TrackSession = {0}", _track.TrackSession);
                        DicConsole.DebugWriteLine("Nero plugin", "\t\t _track.TrackStartSector = {0}",
                                                  _track.TrackStartSector);
                        DicConsole.DebugWriteLine("Nero plugin", "\t\t _track.TrackType = {0}", _track.TrackType);

                        if(currentsessioncurrenttrack == 1)
                        {
                            currentsessionstruct = new Session();
                            currentsessionstruct.SessionSequence = currentsession;
                            currentsessionstruct.StartSector = _track.TrackStartSector;
                            currentsessionstruct.StartTrack = _track.TrackSequence;
                        }
                        currentsessioncurrenttrack++;
                        if(currentsessioncurrenttrack > currentsessionmaxtrack)
                        {
                            currentsession++;
                            neroSessions.TryGetValue(currentsession, out currentsessionmaxtrack);
                            currentsessioncurrenttrack = 1;
                            currentsessionstruct.EndTrack = _track.TrackSequence;
                            currentsessionstruct.EndSector = _track.TrackEndSector;
                            imageSessions.Add(currentsessionstruct);
                        }

                        if(i == neroTracks.Count)
                        {
                            neroSessions.TryGetValue(currentsession, out currentsessionmaxtrack);
                            currentsessioncurrenttrack = 1;
                            currentsessionstruct.EndTrack = _track.TrackSequence;
                            currentsessionstruct.EndSector = _track.TrackEndSector;
                            imageSessions.Add(currentsessionstruct);
                        }

                        offsetmap.Add(_track.TrackSequence, _track.TrackStartSector);
                        DicConsole.DebugWriteLine("Nero plugin", "\t\t Offset[{0}]: {1}", _track.TrackSequence,
                                                  _track.TrackStartSector);

                        Partition partition;

                        /*if(_neroTrack.Index0 < _neroTrack.Index1)
                        {
                            partition = new Partition();
                            partition.PartitionDescription = string.Format("Track {0} Index 0", _track.TrackSequence);
                            partition.PartitionLength = (_neroTrack.Index1 - _neroTrack.Index0);
                            partition.PartitionName = StringHandlers.CToString(_neroTrack.ISRC);
                            partition.PartitionSectors = partition.PartitionLength / _neroTrack.SectorSize;
                            partition.PartitionSequence = PartitionSequence;
                            partition.PartitionStart = _neroTrack.Index0;
                            partition.PartitionStartSector = _neroTrack.StartLBA;
                            partition.PartitionType = NeroTrackModeToTrackType((DAOMode)_neroTrack.Mode).ToString();
                            ImagePartitions.Add(partition);
                            PartitionSequence++;
                        }*/

                        partition = new Partition();
                        partition.Description = string.Format("Track {0} Index 1", _track.TrackSequence);
                        partition.Size = neroTrack.EndOfTrack - neroTrack.Index1;
                        partition.Name = StringHandlers.CToString(neroTrack.Isrc);
                        partition.Length = partition.Size / neroTrack.SectorSize;
                        partition.Sequence = partitionSequence;
                        partition.Offset = partitionStartByte;
                        partition.Start = neroTrack.StartLba +
                                          (neroTrack.Index1 - neroTrack.Index0) / neroTrack.SectorSize;
                        partition.Type = NeroTrackModeToTrackType((DaoMode)neroTrack.Mode).ToString();
                        imagePartitions.Add(partition);
                        partitionSequence++;
                        partitionStartByte += partition.Size;
                    }
                }

                this.imageFilter = imageFilter;

                if(ImageInfo.MediaType == MediaType.Unknown || ImageInfo.MediaType == MediaType.CD)
                {
                    bool data = false;
                    bool mode2 = false;
                    bool firstaudio = false;
                    bool firstdata = false;
                    bool audio = false;

                    for(int i = 0; i < neroTracks.Count; i++)
                    {
                        // First track is audio
                        firstaudio |= i == 0 && ((DaoMode)neroTracks.ElementAt(i).Value.Mode == DaoMode.Audio ||
                                                 (DaoMode)neroTracks.ElementAt(i).Value.Mode == DaoMode.AudioSub);

                        // First track is data
                        firstdata |= i == 0 && (DaoMode)neroTracks.ElementAt(i).Value.Mode != DaoMode.Audio && (DaoMode)neroTracks.ElementAt(i).Value.Mode != DaoMode.AudioSub;

                        // Any non first track is data
                        data |= i != 0 && (DaoMode)neroTracks.ElementAt(i).Value.Mode != DaoMode.Audio && (DaoMode)neroTracks.ElementAt(i).Value.Mode != DaoMode.AudioSub;

                        // Any non first track is audio
                        audio |= i != 0 && ((DaoMode)neroTracks.ElementAt(i).Value.Mode == DaoMode.Audio ||
                                            (DaoMode)neroTracks.ElementAt(i).Value.Mode == DaoMode.AudioSub);

                        switch((DaoMode)neroTracks.ElementAt(i).Value.Mode)
                        {
                            case DaoMode.DataM2F1:
                            case DaoMode.DataM2F2:
                            case DaoMode.DataM2Raw:
                            case DaoMode.DataM2RawSub:
                                mode2 = true;
                                break;
                        }
                    }

                    if(!data && !firstdata) ImageInfo.MediaType = MediaType.CDDA;
                    else if(firstaudio && data && imageSessions.Count > 1 && mode2)
                        ImageInfo.MediaType = MediaType.CDPLUS;
                    else if(firstdata && audio || mode2) ImageInfo.MediaType = MediaType.CDROMXA;
                    else if(!audio) ImageInfo.MediaType = MediaType.CDROM;
                    else ImageInfo.MediaType = MediaType.CD;
                }

                ImageInfo.XmlMediaType = XmlMediaType.OpticalDisc;
                DicConsole.VerboseWriteLine("Nero image contains a disc of type {0}", ImageInfo.MediaType);

                return true;
            }
            catch
            {
                DicConsole.DebugWrite("Nero plugin", "Exception ocurred opening file.");
                return false;
            }
        }

        public override bool ImageHasPartitions()
        {
            // Even if they only have 1 track, there is a partition (track 1)
            return true;
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

        public override byte[] ReadDiskTag(MediaTagType tag)
        {
            switch(tag)
            {
                case MediaTagType.CD_MCN: return upc;
                case MediaTagType.CD_TEXT: throw new NotImplementedException("Not yet implemented");
                default:
                    throw new FeaturedNotSupportedByDiscImageException("Requested disk tag not supported by image");
            }
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            return ReadSectorsTag(sectorAddress, 1, tag);
        }

        public override byte[] ReadSector(ulong sectorAddress, uint track)
        {
            return ReadSectors(sectorAddress, 1, track);
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            return ReadSectorsTag(sectorAddress, 1, track, tag);
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in offsetmap)
                if(sectorAddress >= kvp.Value)
                    foreach(Track _track in imageTracks)
                        if(_track.TrackSequence == kvp.Key)
                            if(sectorAddress - kvp.Value < _track.TrackEndSector - _track.TrackStartSector)
                                return ReadSectors(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                  string.Format("Sector address {0} not found", sectorAddress));
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            foreach(KeyValuePair<uint, ulong> kvp in offsetmap)
                if(sectorAddress >= kvp.Value)
                    foreach(Track _track in imageTracks)
                        if(_track.TrackSequence == kvp.Key)
                            if(sectorAddress - kvp.Value < _track.TrackEndSector - _track.TrackStartSector)
                                return ReadSectorsTag(sectorAddress - kvp.Value, length, kvp.Key, tag);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                  string.Format("Sector address {0} not found", sectorAddress));
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            NeroTrack _track;

            if(!neroTracks.TryGetValue(track, out _track))
                throw new ArgumentOutOfRangeException(nameof(track), "Track not found");

            if(length > _track.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      string
                                                          .Format("Requested more sectors ({0}) than present in track ({1}), won't cross tracks",
                                                                  length, _track.Sectors));

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch((DaoMode)_track.Mode)
            {
                case DaoMode.Data:
                case DaoMode.DataM2F1:
                {
                    sectorOffset = 0;
                    sectorSize = 2048;
                    sectorSkip = 0;
                    break;
                }
                case DaoMode.DataM2F2:
                {
                    sectorOffset = 8;
                    sectorSize = 2324;
                    sectorSkip = 4;
                    break;
                }
                case DaoMode.Audio:
                {
                    sectorOffset = 0;
                    sectorSize = 2352;
                    sectorSkip = 0;
                    break;
                }
                case DaoMode.DataRaw:
                {
                    sectorOffset = 16;
                    sectorSize = 2048;
                    sectorSkip = 288;
                    break;
                }
                case DaoMode.DataM2Raw:
                {
                    sectorOffset = 16;
                    sectorSize = 2336;
                    sectorSkip = 0;
                    break;
                }
                // TODO: Supposing Nero suffixes the subchannel to the channel
                case DaoMode.DataRawSub:
                {
                    sectorOffset = 16;
                    sectorSize = 2048;
                    sectorSkip = 288 + 96;
                    break;
                }
                case DaoMode.DataM2RawSub:
                {
                    sectorOffset = 16;
                    sectorSize = 2336;
                    sectorSkip = 96;
                    break;
                }
                case DaoMode.AudioSub:
                {
                    sectorOffset = 0;
                    sectorSize = 2352;
                    sectorSkip = 96;
                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sectorSize * length];

            imageStream = imageFilter.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);
            br.BaseStream
              .Seek((long)_track.Offset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                    SeekOrigin.Begin);
            if(sectorOffset == 0 && sectorSkip == 0) buffer = br.ReadBytes((int)(sectorSize * length));
            else
                for(int i = 0; i < length; i++)
                {
                    byte[] sector;
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }

            return buffer;
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            NeroTrack _track;

            if(!neroTracks.TryGetValue(track, out _track))
                throw new ArgumentOutOfRangeException(nameof(track), "Track not found");

            if(length > _track.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      string
                                                          .Format("Requested more sectors ({0}) than present in track ({1}), won't cross tracks",
                                                                  length, _track.Sectors));

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
                {
                    byte[] flags = new byte[1];
                    flags[0] = 0x00;

                    if((DaoMode)_track.Mode != DaoMode.Audio && (DaoMode)_track.Mode != DaoMode.AudioSub)
                        flags[0] += 0x40;

                    return flags;
                }
                case SectorTagType.CdTrackIsrc: return _track.Isrc;
                case SectorTagType.CdTrackText:
                    throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
            }

            switch((DaoMode)_track.Mode)
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
                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                }
                case DaoMode.Audio: throw new ArgumentException("There are no tags on audio tracks", nameof(tag));
                case DaoMode.DataRaw:
                {
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
                        case SectorTagType.CdSectorSubchannel:
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
                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                }
                // TODO
                case DaoMode.DataM2RawSub:
                    throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
                case DaoMode.DataRawSub:
                {
                    switch(tag)
                    {
                        case SectorTagType.CdSectorSync:
                        {
                            sectorOffset = 0;
                            sectorSize = 12;
                            sectorSkip = 2340 + 96;
                            break;
                        }
                        case SectorTagType.CdSectorHeader:
                        {
                            sectorOffset = 12;
                            sectorSize = 4;
                            sectorSkip = 2336 + 96;
                            break;
                        }
                        case SectorTagType.CdSectorSubchannel:
                        {
                            sectorOffset = 2352;
                            sectorSize = 96;
                            sectorSkip = 0;
                            break;
                        }
                        case SectorTagType.CdSectorSubHeader:
                            throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                        case SectorTagType.CdSectorEcc:
                        {
                            sectorOffset = 2076;
                            sectorSize = 276;
                            sectorSkip = 0 + 96;
                            break;
                        }
                        case SectorTagType.CdSectorEccP:
                        {
                            sectorOffset = 2076;
                            sectorSize = 172;
                            sectorSkip = 104 + 96;
                            break;
                        }
                        case SectorTagType.CdSectorEccQ:
                        {
                            sectorOffset = 2248;
                            sectorSize = 104;
                            sectorSkip = 0 + 96;
                            break;
                        }
                        case SectorTagType.CdSectorEdc:
                        {
                            sectorOffset = 2064;
                            sectorSize = 4;
                            sectorSkip = 284 + 96;
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
                    sectorSize = 96;
                    sectorSkip = 0;
                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sectorSize * length];

            imageStream = imageFilter.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);
            br.BaseStream
              .Seek((long)_track.Offset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                    SeekOrigin.Begin);
            if(sectorOffset == 0 && sectorSkip == 0) buffer = br.ReadBytes((int)(sectorSize * length));
            else
                for(int i = 0; i < length; i++)
                {
                    byte[] sector;
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }

            return buffer;
        }

        public override byte[] ReadSectorLong(ulong sectorAddress)
        {
            return ReadSectorsLong(sectorAddress, 1);
        }

        public override byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            return ReadSectorsLong(sectorAddress, 1, track);
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in offsetmap)
                if(sectorAddress >= kvp.Value)
                    foreach(Track _track in imageTracks)
                        if(_track.TrackSequence == kvp.Key)
                            if(sectorAddress - kvp.Value < _track.TrackEndSector - _track.TrackStartSector)
                                return ReadSectorsLong(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                  string.Format("Sector address {0} not found", sectorAddress));
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            NeroTrack _track;

            if(!neroTracks.TryGetValue(track, out _track))
                throw new ArgumentOutOfRangeException(nameof(track), "Track not found");

            if(length > _track.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      string
                                                          .Format("Requested more sectors ({0}) than present in track ({1}), won't cross tracks",
                                                                  length, _track.Sectors));

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch((DaoMode)_track.Mode)
            {
                case DaoMode.Data:
                case DaoMode.DataM2F1:
                {
                    sectorOffset = 0;
                    sectorSize = 2048;
                    sectorSkip = 0;
                    break;
                }
                case DaoMode.DataM2F2:
                {
                    sectorOffset = 0;
                    sectorSize = 2336;
                    sectorSkip = 0;
                    break;
                }
                case DaoMode.DataRaw:
                case DaoMode.DataM2Raw:
                case DaoMode.Audio:
                {
                    sectorOffset = 0;
                    sectorSize = 2352;
                    sectorSkip = 0;
                    break;
                }
                case DaoMode.DataRawSub:
                case DaoMode.DataM2RawSub:
                case DaoMode.AudioSub:
                {
                    sectorOffset = 0;
                    sectorSize = 2448;
                    sectorSkip = 0;
                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sectorSize * length];

            imageStream = imageFilter.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);

            br.BaseStream
              .Seek((long)_track.Offset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                    SeekOrigin.Begin);

            if(sectorOffset == 0 && sectorSkip == 0) buffer = br.ReadBytes((int)(sectorSize * length));
            else
                for(int i = 0; i < length; i++)
                {
                    byte[] sector;
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);

                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }

            return buffer;
        }

        public override string GetImageFormat()
        {
            return "Nero Burning ROM";
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

        public override string GetMediaBarcode()
        {
            return ImageInfo.MediaBarcode;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.MediaType;
        }

        public override List<Partition> GetPartitions()
        {
            return imagePartitions;
        }

        public override List<Track> GetTracks()
        {
            return imageTracks;
        }

        public override List<Track> GetSessionTracks(Session session)
        {
            return GetSessionTracks(session.SessionSequence);
        }

        public override List<Track> GetSessionTracks(ushort session)
        {
            List<Track> sessionTracks = new List<Track>();
            foreach(Track _track in imageTracks) if(_track.TrackSession == session) sessionTracks.Add(_track);

            return sessionTracks;
        }

        public override List<Session> GetSessions()
        {
            return imageSessions;
        }

        public override bool? VerifySector(ulong sectorAddress)
        {
            byte[] buffer = ReadSectorLong(sectorAddress);
            return Checksums.CdChecksums.CheckCdSector(buffer);
        }

        public override bool? VerifySector(ulong sectorAddress, uint track)
        {
            byte[] buffer = ReadSectorLong(sectorAddress, track);
            return Checksums.CdChecksums.CheckCdSector(buffer);
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
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
                bool? sectorStatus = Checksums.CdChecksums.CheckCdSector(sector);

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
            byte[] buffer = ReadSectorsLong(sectorAddress, length, track);
            int bps = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * bps, sector, 0, bps);
                bool? sectorStatus = Checksums.CdChecksums.CheckCdSector(sector);

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
            return null;
        }
        #endregion

        #region Private methods
        static MediaType NeroMediaTypeToMediaType(NeroMediaTypes type)
        {
            switch(type)
            {
                case NeroMediaTypes.NeroMtypDdcd: return MediaType.DDCD;
                case NeroMediaTypes.NeroMtypDvdM:
                case NeroMediaTypes.NeroMtypDvdMR: return MediaType.DVDR;
                case NeroMediaTypes.NeroMtypDvdP:
                case NeroMediaTypes.NeroMtypDvdPR: return MediaType.DVDPR;
                case NeroMediaTypes.NeroMtypDvdRam: return MediaType.DVDRAM;
                case NeroMediaTypes.NeroMtypMl:
                case NeroMediaTypes.NeroMtypMrw:
                case NeroMediaTypes.NeroMtypCdrw: return MediaType.CDRW;
                case NeroMediaTypes.NeroMtypCdr: return MediaType.CDR;
                case NeroMediaTypes.NeroMtypDvdRom:
                case NeroMediaTypes.NeroMtypDvdAny:
                case NeroMediaTypes.NeroMtypDvdAnyR9:
                case NeroMediaTypes.NeroMtypDvdAnyOld: return MediaType.DVDROM;
                case NeroMediaTypes.NeroMtypCdrom: return MediaType.CDROM;
                case NeroMediaTypes.NeroMtypDvdMRw: return MediaType.DVDRW;
                case NeroMediaTypes.NeroMtypDvdPRw: return MediaType.DVDPRW;
                case NeroMediaTypes.NeroMtypDvdPR9: return MediaType.DVDPRDL;
                case NeroMediaTypes.NeroMtypDvdMR9: return MediaType.DVDRDL;
                case NeroMediaTypes.NeroMtypBd:
                case NeroMediaTypes.NeroMtypBdAny:
                case NeroMediaTypes.NeroMtypBdRom: return MediaType.BDROM;
                case NeroMediaTypes.NeroMtypBdR: return MediaType.BDR;
                case NeroMediaTypes.NeroMtypBdRe: return MediaType.BDRE;
                case NeroMediaTypes.NeroMtypHdDvd:
                case NeroMediaTypes.NeroMtypHdDvdAny:
                case NeroMediaTypes.NeroMtypHdDvdRom: return MediaType.HDDVDROM;
                case NeroMediaTypes.NeroMtypHdDvdR: return MediaType.HDDVDR;
                case NeroMediaTypes.NeroMtypHdDvdRw: return MediaType.HDDVDRW;
                default: return MediaType.CD;
            }
        }

        static TrackType NeroTrackModeToTrackType(DaoMode mode)
        {
            switch(mode)
            {
                case DaoMode.Data:
                case DaoMode.DataRaw:
                case DaoMode.DataRawSub: return TrackType.CdMode1;
                case DaoMode.DataM2F1: return TrackType.CdMode2Form1;
                case DaoMode.DataM2F2: return TrackType.CdMode2Form2;
                case DaoMode.DataM2RawSub:
                case DaoMode.DataM2Raw: return TrackType.CdMode2Formless;
                case DaoMode.Audio:
                case DaoMode.AudioSub: return TrackType.Audio;
                default: return TrackType.Data;
            }
        }

        static ushort NeroTrackModeToBytesPerSector(DaoMode mode)
        {
            switch(mode)
            {
                case DaoMode.Data:
                case DaoMode.DataM2F1: return 2048;
                case DaoMode.DataM2F2: return 2336;
                case DaoMode.DataRaw:
                case DaoMode.DataM2Raw:
                case DaoMode.Audio: return 2352;
                case DaoMode.DataM2RawSub:
                case DaoMode.DataRawSub:
                case DaoMode.AudioSub: return 2448;
                default: return 2352;
            }
        }
        #endregion

        #region Unsupported features
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

        public override string GetMediaPartNumber()
        {
            return ImageInfo.MediaPartNumber;
        }

        public override string GetMediaManufacturer()
        {
            return ImageInfo.MediaManufacturer;
        }

        public override string GetMediaModel()
        {
            return ImageInfo.MediaModel;
        }

        public override string GetImageName()
        {
            return ImageInfo.ImageName;
        }

        public override string GetImageCreator()
        {
            return ImageInfo.ImageCreator;
        }

        public override string GetImageComments()
        {
            return ImageInfo.ImageComments;
        }

        public override string GetMediaSerialNumber()
        {
            return ImageInfo.MediaSerialNumber;
        }
        #endregion
    }
}