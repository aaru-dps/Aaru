// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Nero.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using DiscImageChef.Console;
using DiscImageChef.CommonTypes;
using DiscImageChef.Filters;

namespace DiscImageChef.ImagePlugins
{
    class Nero : ImagePlugin
    {
        #region Internal structures

        struct NeroV1Footer
        {
            /// <summary>
            /// "NERO"
            /// </summary>
            public uint ChunkID;

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
            public uint ChunkID;

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
            public int LBAStart;
        }

        struct NeroV2Cuesheet
        {
            /// <summary>
            /// "CUEX"
            /// </summary>
            public uint ChunkID;

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
            public uint ChunkID;

            /// <summary>
            /// Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            /// Cuesheet entries
            /// </summary>
            public List<NeroV1CueEntry> Entries;
        }

        struct NeroV1DAOEntry
        {
            /// <summary>
            /// ISRC (12 bytes)
            /// </summary>
            public byte[] ISRC;

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

        struct NeroV1DAO
        {
            /// <summary>
            /// "DAOI"
            /// </summary>
            public uint ChunkID;

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
            public byte[] UPC;

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
            public List<NeroV1DAOEntry> Tracks;
        }

        struct NeroV2DAOEntry
        {
            /// <summary>
            /// ISRC (12 bytes)
            /// </summary>
            public byte[] ISRC;

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

        struct NeroV2DAO
        {
            /// <summary>
            /// "DAOX"
            /// </summary>
            public uint ChunkID;

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
            public byte[] UPC;

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
            public List<NeroV2DAOEntry> Tracks;
        }

        struct NeroCDTextPack
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
            public ushort CRC;
        }

        struct NeroCDText
        {
            /// <summary>
            /// "CDTX"
            /// </summary>
            public uint ChunkID;

            /// <summary>
            /// Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            /// CD-TEXT packs
            /// </summary>
            public List<NeroCDTextPack> Packs;
        }

        struct NeroV1TAOEntry
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
            public uint StartLBA;

            /// <summary>
            /// Unknown
            /// </summary>
            public uint Unknown;
        }

        struct NeroV1TAO
        {
            /// <summary>
            /// "ETNF"
            /// </summary>
            public uint ChunkID;

            /// <summary>
            /// Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            /// CD-TEXT packs
            /// </summary>
            public List<NeroV1TAOEntry> Tracks;
        }

        struct NeroV2TAOEntry
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
            public uint StartLBA;

            /// <summary>
            /// Unknown
            /// </summary>
            public uint Unknown;

            /// <summary>
            /// Track length in sectors
            /// </summary>
            public uint Sectors;
        }

        struct NeroV2TAO
        {
            /// <summary>
            /// "ETN2"
            /// </summary>
            public uint ChunkID;

            /// <summary>
            /// Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            /// CD-TEXT packs
            /// </summary>
            public List<NeroV2TAOEntry> Tracks;
        }

        struct NeroSession
        {
            /// <summary>
            /// "SINF"
            /// </summary>
            public uint ChunkID;

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
            public uint ChunkID;

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
            public uint ChunkID;

            /// <summary>
            /// Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            /// Unknown
            /// </summary>
            public uint Unknown;
        }

        struct NeroTOCChunk
        {
            /// <summary>
            /// "TOCT"
            /// </summary>
            public uint ChunkID;

            /// <summary>
            /// Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            /// Unknown
            /// </summary>
            public ushort Unknown;
        }

        struct NeroRELOChunk
        {
            /// <summary>
            /// "RELO"
            /// </summary>
            public uint ChunkID;

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
            public uint ChunkID;

            /// <summary>
            /// Chunk size
            /// </summary>
            public uint ChunkSize;
        }

        // Internal use only
        struct NeroTrack
        {
            public byte[] ISRC;
            public ushort SectorSize;
            public ulong Offset;
            public ulong Length;
            public ulong EndOfTrack;
            public uint Mode;
            public ulong StartLBA;
            public ulong Sectors;
            public ulong Index0;
            public ulong Index1;
            public uint Sequence;
        }

        #endregion

        #region Internal consts

        // "NERO"
        public const uint NeroV1FooterID = 0x4E45524F;

        // "NER5"
        public const uint NeroV2FooterID = 0x4E455235;

        // "CUES"
        public const uint NeroV1CUEID = 0x43554553;

        // "CUEX"
        public const uint NeroV2CUEID = 0x43554558;

        // "ETNF"
        public const uint NeroV1TAOID = 0x45544E46;

        // "ETN2"
        public const uint NeroV2TAOID = 0x45544E32;

        // "DAOI"
        public const uint NeroV1DAOID = 0x44414F49;

        // "DAOX"
        public const uint NeroV2DAOID = 0x44414F58;

        // "CDTX"
        public const uint NeroCDTextID = 0x43445458;

        // "SINF"
        public const uint NeroSessionID = 0x53494E46;

        // "MTYP"
        public const uint NeroDiskTypeID = 0x4D545950;

        // "DINF"
        public const uint NeroDiscInfoID = 0x44494E46;

        // "TOCT"
        public const uint NeroTOCID = 0x544F4354;

        // "RELO"
        public const uint NeroReloID = 0x52454C4F;

        // "END!"
        public const uint NeroEndID = 0x454E4421;

        public enum DAOMode : ushort
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
            NERO_MTYP_NONE = 0x00000,
            /// <summary>
            /// CD-R/RW
            /// </summary>
            NERO_MTYP_CD = 0x00001,
            /// <summary>
            /// DDCD-R/RW
            /// </summary>
            NERO_MTYP_DDCD = 0x00002,
            /// <summary>
            /// DVD-R/RW
            /// </summary>
            NERO_MTYP_DVD_M = 0x00004,
            /// <summary>
            /// DVD+RW
            /// </summary>
            NERO_MTYP_DVD_P = 0x00008,
            /// <summary>
            /// DVD-RAM
            /// </summary>
            NERO_MTYP_DVD_RAM = 0x00010,
            /// <summary>
            /// Multi-level disc
            /// </summary>
            NERO_MTYP_ML = 0x00020,
            /// <summary>
            /// Mount Rainier
            /// </summary>
            NERO_MTYP_MRW = 0x00040,
            /// <summary>
            /// Exclude CD-R
            /// </summary>
            NERO_MTYP_NO_CDR = 0x00080,
            /// <summary>
            /// Exclude CD-RW
            /// </summary>
            NERO_MTYP_NO_CDRW = 0x00100,
            /// <summary>
            /// CD-RW
            /// </summary>
            NERO_MTYP_CDRW = NERO_MTYP_CD | NERO_MTYP_NO_CDR,
            /// <summary>
            /// CD-R
            /// </summary>
            NERO_MTYP_CDR = NERO_MTYP_CD | NERO_MTYP_NO_CDRW,
            /// <summary>
            /// DVD-ROM
            /// </summary>
            NERO_MTYP_DVD_ROM = 0x00200,
            /// <summary>
            /// CD-ROM
            /// </summary>
            NERO_MTYP_CDROM = 0x00400,
            /// <summary>
            /// Exclude DVD-RW
            /// </summary>
            NERO_MTYP_NO_DVD_M_RW = 0x00800,
            /// <summary>
            /// Exclude DVD-R
            /// </summary>
            NERO_MTYP_NO_DVD_M_R = 0x01000,
            /// <summary>
            /// Exclude DVD+RW
            /// </summary>
            NERO_MTYP_NO_DVD_P_RW = 0x02000,
            /// <summary>
            /// Exclude DVD+R
            /// </summary>
            NERO_MTYP_NO_DVD_P_R = 0x04000,
            /// <summary>
            /// DVD-R
            /// </summary>
            NERO_MTYP_DVD_M_R = NERO_MTYP_DVD_M | NERO_MTYP_NO_DVD_M_RW,
            /// <summary>
            /// DVD-RW
            /// </summary>
            NERO_MTYP_DVD_M_RW = NERO_MTYP_DVD_M | NERO_MTYP_NO_DVD_M_R,
            /// <summary>
            /// DVD+R
            /// </summary>
            NERO_MTYP_DVD_P_R = NERO_MTYP_DVD_P | NERO_MTYP_NO_DVD_P_RW,
            /// <summary>
            /// DVD+RW
            /// </summary>
            NERO_MTYP_DVD_P_RW = NERO_MTYP_DVD_P | NERO_MTYP_NO_DVD_P_R,
            /// <summary>
            /// Packet-writing (fixed)
            /// </summary>
            NERO_MTYP_FPACKET = 0x08000,
            /// <summary>
            /// Packet-writing (variable)
            /// </summary>
            NERO_MTYP_VPACKET = 0x10000,
            /// <summary>
            /// Packet-writing (any)
            /// </summary>
            NERO_MTYP_PACKETW = NERO_MTYP_MRW | NERO_MTYP_FPACKET | NERO_MTYP_VPACKET,
            /// <summary>
            /// HD-Burn
            /// </summary>
            NERO_MTYP_HDB = 0x20000,
            /// <summary>
            /// DVD+R DL
            /// </summary>
            NERO_MTYP_DVD_P_R9 = 0x40000,
            /// <summary>
            /// DVD-R DL
            /// </summary>
            NERO_MTYP_DVD_M_R9 = 0x80000,
            /// <summary>
            /// Any DVD double-layer
            /// </summary>
            NERO_MTYP_DVD_ANY_R9 = NERO_MTYP_DVD_P_R9 | NERO_MTYP_DVD_M_R9,
            /// <summary>
            /// Any DVD
            /// </summary>
            NERO_MTYP_DVD_ANY = NERO_MTYP_DVD_M | NERO_MTYP_DVD_P | NERO_MTYP_DVD_RAM | NERO_MTYP_DVD_ANY_R9,
            /// <summary>
            /// BD-ROM
            /// </summary>
            NERO_MTYP_BD_ROM = 0x100000,
            /// <summary>
            /// BD-R
            /// </summary>
            NERO_MTYP_BD_R = 0x200000,
            /// <summary>
            /// BD-RE
            /// </summary>
            NERO_MTYP_BD_RE = 0x400000,
            /// <summary>
            /// BD-R/RE
            /// </summary>
            NERO_MTYP_BD = NERO_MTYP_BD_R | NERO_MTYP_BD_RE,
            /// <summary>
            /// Any BD
            /// </summary>
            NERO_MTYP_BD_ANY = NERO_MTYP_BD | NERO_MTYP_BD_ROM,
            /// <summary>
            /// HD DVD-ROM
            /// </summary>
            NERO_MTYP_HD_DVD_ROM = 0x0800000,
            /// <summary>
            /// HD DVD-R
            /// </summary>
            NERO_MTYP_HD_DVD_R = 0x1000000,
            /// <summary>
            /// HD DVD-RW
            /// </summary>
            NERO_MTYP_HD_DVD_RW = 0x2000000,
            /// <summary>
            /// HD DVD-R/RW
            /// </summary>
            NERO_MTYP_HD_DVD = NERO_MTYP_HD_DVD_R | NERO_MTYP_HD_DVD_RW,
            /// <summary>
            /// Any HD DVD
            /// </summary>
            NERO_MTYP_HD_DVD_ANY = NERO_MTYP_HD_DVD | NERO_MTYP_HD_DVD_ROM,
            /// <summary>
            /// Any DVD, old
            /// </summary>
            NERO_MTYP_DVD_ANY_OLD = NERO_MTYP_DVD_M | NERO_MTYP_DVD_P | NERO_MTYP_DVD_RAM,
        }

        #endregion

        #region Internal variables

        Filter _imageFilter;
        Stream imageStream;
        bool imageNewFormat;
        Dictionary<ushort, uint> neroSessions;
        NeroV1Cuesheet neroCuesheetV1;
        NeroV2Cuesheet neroCuesheetV2;
        NeroV1DAO neroDAOV1;
        NeroV2DAO neroDAOV2;
        NeroCDText neroCDTXT;
        NeroV1TAO neroTAOV1;
        NeroV2TAO neroTAOV2;
        NeroMediaType neroMediaTyp;
        NeroDiscInformation neroDiscInfo;
        NeroTOCChunk neroTOC;
        NeroRELOChunk neroRELO;

        List<Track> imageTracks;
        Dictionary<uint, byte[]> TrackISRCs;
        byte[] UPC;
        Dictionary<uint, NeroTrack> neroTracks;
        Dictionary<uint, ulong> offsetmap;
        List<Session> imageSessions;
        List<Partition> ImagePartitions;

        #endregion

        #region Methods

        public Nero()
        {
            Name = "Nero Burning ROM image";
            PluginUUID = new Guid("D160F9FF-5941-43FC-B037-AD81DD141F05");
            imageNewFormat = false;
            ImageInfo = new ImageInfo();
            ImageInfo.readableSectorTags = new List<SectorTagType>();
            ImageInfo.readableMediaTags = new List<MediaTagType>();
            neroSessions = new Dictionary<ushort, uint>();
            neroTracks = new Dictionary<uint, NeroTrack>();
            offsetmap = new Dictionary<uint, ulong>();
            imageSessions = new List<Session>();
            ImagePartitions = new List<Partition>();
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
            footerV1.ChunkID = BigEndianBitConverter.ToUInt32(buffer, 0);
            footerV1.FirstChunkOffset = BigEndianBitConverter.ToUInt32(buffer, 4);

            imageStream.Seek(-12, SeekOrigin.End);
            buffer = new byte[12];
            imageStream.Read(buffer, 0, 12);
            footerV2.ChunkID = BigEndianBitConverter.ToUInt32(buffer, 0);
            footerV2.FirstChunkOffset = BigEndianBitConverter.ToUInt64(buffer, 4);

            DicConsole.DebugWriteLine("Nero plugin", "imageStream.Length = {0}", imageStream.Length);
            DicConsole.DebugWriteLine("Nero plugin", "footerV1.ChunkID = 0x{0:X8}", footerV1.ChunkID);
            DicConsole.DebugWriteLine("Nero plugin", "footerV1.FirstChunkOffset = {0}", footerV1.FirstChunkOffset);
            DicConsole.DebugWriteLine("Nero plugin", "footerV2.ChunkID = 0x{0:X8}", footerV2.ChunkID);
            DicConsole.DebugWriteLine("Nero plugin", "footerV2.FirstChunkOffset = {0}", footerV2.FirstChunkOffset);

            if(footerV2.ChunkID == NeroV2FooterID && footerV2.FirstChunkOffset < (ulong)imageStream.Length)
            {
                return true;
            }
            if(footerV1.ChunkID == NeroV1FooterID && footerV1.FirstChunkOffset < (ulong)imageStream.Length)
            {
                return true;
            }

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
                footerV1.ChunkID = BigEndianBitConverter.ToUInt32(buffer, 0);
                footerV1.FirstChunkOffset = BigEndianBitConverter.ToUInt32(buffer, 4);

                imageStream.Seek(-12, SeekOrigin.End);
                buffer = new byte[12];
                imageStream.Read(buffer, 0, 12);
                footerV2.ChunkID = BigEndianBitConverter.ToUInt32(buffer, 0);
                footerV2.FirstChunkOffset = BigEndianBitConverter.ToUInt64(buffer, 4);

                DicConsole.DebugWriteLine("Nero plugin", "imageStream.Length = {0}", imageStream.Length);
                DicConsole.DebugWriteLine("Nero plugin", "footerV1.ChunkID = 0x{0:X8} (\"{1}\")", footerV1.ChunkID, System.Text.Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(footerV1.ChunkID)));
                DicConsole.DebugWriteLine("Nero plugin", "footerV1.FirstChunkOffset = {0}", footerV1.FirstChunkOffset);
                DicConsole.DebugWriteLine("Nero plugin", "footerV2.ChunkID = 0x{0:X8} (\"{1}\")", footerV2.ChunkID, System.Text.Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(footerV2.ChunkID)));
                DicConsole.DebugWriteLine("Nero plugin", "footerV2.FirstChunkOffset = {0}", footerV2.FirstChunkOffset);

                if(footerV1.ChunkID == NeroV1FooterID && footerV1.FirstChunkOffset < (ulong)imageStream.Length)
                    imageNewFormat = false;
                else if(footerV2.ChunkID == NeroV2FooterID && footerV2.FirstChunkOffset < (ulong)imageStream.Length)
                    imageNewFormat = true;
                else
                    return false;

                if(imageNewFormat)
                    imageStream.Seek((long)footerV2.FirstChunkOffset, SeekOrigin.Begin);
                else
                    imageStream.Seek(footerV1.FirstChunkOffset, SeekOrigin.Begin);

                bool parsing = true;
                ushort currentsession = 1;
                uint currenttrack = 1;

                imageTracks = new List<Track>();
                TrackISRCs = new Dictionary<uint, byte[]>();

                ImageInfo.mediaType = MediaType.CD;
                ImageInfo.sectors = 0;
                ImageInfo.sectorSize = 0;

                while(parsing)
                {
                    byte[] ChunkHeaderBuffer = new byte[8];
                    uint ChunkID;
                    uint ChunkLength;

                    imageStream.Read(ChunkHeaderBuffer, 0, 8);
                    ChunkID = BigEndianBitConverter.ToUInt32(ChunkHeaderBuffer, 0);
                    ChunkLength = BigEndianBitConverter.ToUInt32(ChunkHeaderBuffer, 4);

                    DicConsole.DebugWriteLine("Nero plugin", "ChunkID = 0x{0:X8} (\"{1}\")", ChunkID, System.Text.Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(ChunkID)));
                    DicConsole.DebugWriteLine("Nero plugin", "ChunkLength = {0}", ChunkLength);

                    switch(ChunkID)
                    {
                        case NeroV1CUEID:
                            {
                                DicConsole.DebugWriteLine("Nero plugin", "Found \"CUES\" chunk, parsing {0} bytes", ChunkLength);

                                neroCuesheetV1 = new NeroV1Cuesheet();
                                neroCuesheetV1.ChunkID = ChunkID;
                                neroCuesheetV1.ChunkSize = ChunkLength;
                                neroCuesheetV1.Entries = new List<NeroV1CueEntry>();

                                byte[] tmpbuffer = new byte[8];
                                for(int i = 0; i < neroCuesheetV1.ChunkSize; i += 8)
                                {
                                    NeroV1CueEntry _entry = new NeroV1CueEntry();
                                    imageStream.Read(tmpbuffer, 0, 8);
                                    _entry.Mode = tmpbuffer[0];
                                    _entry.TrackNumber = tmpbuffer[1];
                                    _entry.IndexNumber = tmpbuffer[2];
                                    _entry.Dummy = BigEndianBitConverter.ToUInt16(tmpbuffer, 3);
                                    _entry.Minute = tmpbuffer[5];
                                    _entry.Second = tmpbuffer[6];
                                    _entry.Frame = tmpbuffer[7];

                                    DicConsole.DebugWriteLine("Nero plugin", "Cuesheet entry {0}", (i / 8) + 1);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Mode = {1:X2}", (i / 8) + 1, _entry.Mode);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].TrackNumber = {1:X2}", (i / 8) + 1, _entry.TrackNumber);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].IndexNumber = {1:X2}", (i / 8) + 1, _entry.IndexNumber);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Dummy = {1:X4}", (i / 8) + 1, _entry.Dummy);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Minute = {1:X2}", (i / 8) + 1, _entry.Minute);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Second = {1:X2}", (i / 8) + 1, _entry.Second);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Frame = {1:X2}", (i / 8) + 1, _entry.Frame);

                                    neroCuesheetV1.Entries.Add(_entry);
                                }

                                break;
                            }
                        case NeroV2CUEID:
                            {
                                DicConsole.DebugWriteLine("Nero plugin", "Found \"CUEX\" chunk, parsing {0} bytes", ChunkLength);

                                neroCuesheetV2 = new NeroV2Cuesheet();
                                neroCuesheetV2.ChunkID = ChunkID;
                                neroCuesheetV2.ChunkSize = ChunkLength;
                                neroCuesheetV2.Entries = new List<NeroV2CueEntry>();

                                byte[] tmpbuffer = new byte[8];
                                for(int i = 0; i < neroCuesheetV2.ChunkSize; i += 8)
                                {
                                    NeroV2CueEntry _entry = new NeroV2CueEntry();
                                    imageStream.Read(tmpbuffer, 0, 8);
                                    _entry.Mode = tmpbuffer[0];
                                    _entry.TrackNumber = tmpbuffer[1];
                                    _entry.IndexNumber = tmpbuffer[2];
                                    _entry.Dummy = tmpbuffer[3];
                                    _entry.LBAStart = BigEndianBitConverter.ToInt32(tmpbuffer, 4);

                                    DicConsole.DebugWriteLine("Nero plugin", "Cuesheet entry {0}", (i / 8) + 1);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Mode = 0x{1:X2}", (i / 8) + 1, _entry.Mode);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].TrackNumber = {1:X2}", (i / 8) + 1, _entry.TrackNumber);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].IndexNumber = {1:X2}", (i / 8) + 1, _entry.IndexNumber);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Dummy = {1:X2}", (i / 8) + 1, _entry.Dummy);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].LBAStart = {1}", (i / 8) + 1, _entry.LBAStart);

                                    neroCuesheetV2.Entries.Add(_entry);
                                }

                                break;
                            }
                        case NeroV1DAOID:
                            {
                                DicConsole.DebugWriteLine("Nero plugin", "Found \"DAOI\" chunk, parsing {0} bytes", ChunkLength);

                                neroDAOV1 = new NeroV1DAO();
                                neroDAOV1.ChunkID = ChunkID;
                                neroDAOV1.ChunkSizeBe = ChunkLength;

                                byte[] tmpbuffer = new byte[22];
                                imageStream.Read(tmpbuffer, 0, 22);
                                neroDAOV1.ChunkSizeLe = BigEndianBitConverter.ToUInt32(tmpbuffer, 0);
                                neroDAOV1.UPC = new byte[14];
                                Array.Copy(tmpbuffer, 4, neroDAOV1.UPC, 0, 14);
                                neroDAOV1.TocType = BigEndianBitConverter.ToUInt16(tmpbuffer, 18);
                                neroDAOV1.FirstTrack = tmpbuffer[20];
                                neroDAOV1.LastTrack = tmpbuffer[21];
                                neroDAOV1.Tracks = new List<NeroV1DAOEntry>();

                                if(!ImageInfo.readableMediaTags.Contains(MediaTagType.CD_MCN))
                                    ImageInfo.readableMediaTags.Add(MediaTagType.CD_MCN);

                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDTrackISRC))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDTrackISRC);

                                DicConsole.DebugWriteLine("Nero plugin", "neroDAOV1.ChunkSizeLe = {0} bytes", neroDAOV1.ChunkSizeLe);
                                DicConsole.DebugWriteLine("Nero plugin", "neroDAOV1.UPC = \"{0}\"", StringHandlers.CToString(neroDAOV1.UPC));
                                DicConsole.DebugWriteLine("Nero plugin", "neroDAOV1.TocType = 0x{0:X4}", neroDAOV1.TocType);
                                DicConsole.DebugWriteLine("Nero plugin", "neroDAOV1.FirstTrack = {0}", neroDAOV1.FirstTrack);
                                DicConsole.DebugWriteLine("Nero plugin", "neroDAOV1.LastTrack = {0}", neroDAOV1.LastTrack);

                                UPC = neroDAOV1.UPC;

                                tmpbuffer = new byte[30];
                                for(int i = 0; i < (neroDAOV1.ChunkSizeBe - 22); i += 30)
                                {
                                    NeroV1DAOEntry _entry = new NeroV1DAOEntry();
                                    imageStream.Read(tmpbuffer, 0, 30);
                                    _entry.ISRC = new byte[12];
                                    Array.Copy(tmpbuffer, 4, _entry.ISRC, 0, 12);
                                    _entry.SectorSize = BigEndianBitConverter.ToUInt16(tmpbuffer, 12);
                                    _entry.Mode = BitConverter.ToUInt16(tmpbuffer, 14);
                                    _entry.Unknown = BigEndianBitConverter.ToUInt16(tmpbuffer, 16);
                                    _entry.Index0 = BigEndianBitConverter.ToUInt32(tmpbuffer, 18);
                                    _entry.Index1 = BigEndianBitConverter.ToUInt32(tmpbuffer, 22);
                                    _entry.EndOfTrack = BigEndianBitConverter.ToUInt32(tmpbuffer, 26);

                                    DicConsole.DebugWriteLine("Nero plugin", "Disc-At-Once entry {0}", (i / 32) + 1);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].ISRC = \"{1}\"", (i / 32) + 1, StringHandlers.CToString(_entry.ISRC));
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].SectorSize = {1}", (i / 32) + 1, _entry.SectorSize);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Mode = {1} (0x{2:X4})", (i / 32) + 1, (DAOMode)_entry.Mode, _entry.Mode);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Unknown = 0x{1:X4}", (i / 32) + 1, _entry.Unknown);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Index0 = {1}", (i / 32) + 1, _entry.Index0);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Index1 = {1}", (i / 32) + 1, _entry.Index1);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].EndOfTrack = {1}", (i / 32) + 1, _entry.EndOfTrack);

                                    neroDAOV1.Tracks.Add(_entry);

                                    if(_entry.SectorSize > ImageInfo.sectorSize)
                                        ImageInfo.sectorSize = _entry.SectorSize;

                                    TrackISRCs.Add(currenttrack, _entry.ISRC);
                                    if(currenttrack == 1)
                                        _entry.Index0 = _entry.Index1;

                                    NeroTrack _neroTrack = new NeroTrack();
                                    _neroTrack.EndOfTrack = _entry.EndOfTrack;
                                    _neroTrack.ISRC = _entry.ISRC;
                                    _neroTrack.Length = _entry.EndOfTrack - _entry.Index0;
                                    _neroTrack.Mode = _entry.Mode;
                                    _neroTrack.Offset = _entry.Index0;
                                    _neroTrack.Sectors = _neroTrack.Length / _entry.SectorSize;
                                    _neroTrack.SectorSize = _entry.SectorSize;
                                    _neroTrack.StartLBA = ImageInfo.sectors;
                                    _neroTrack.Index0 = _entry.Index0;
                                    _neroTrack.Index1 = _entry.Index1;
                                    _neroTrack.Sequence = currenttrack;
                                    neroTracks.Add(currenttrack, _neroTrack);

                                    ImageInfo.sectors += _neroTrack.Sectors;

                                    currenttrack++;
                                }

                                break;
                            }
                        case NeroV2DAOID:
                            {
                                DicConsole.DebugWriteLine("Nero plugin", "Found \"DAOX\" chunk, parsing {0} bytes", ChunkLength);

                                neroDAOV2 = new NeroV2DAO();
                                neroDAOV2.ChunkID = ChunkID;
                                neroDAOV2.ChunkSizeBe = ChunkLength;

                                byte[] tmpbuffer = new byte[22];
                                imageStream.Read(tmpbuffer, 0, 22);
                                neroDAOV2.ChunkSizeLe = BigEndianBitConverter.ToUInt32(tmpbuffer, 0);
                                neroDAOV2.UPC = new byte[14];
                                Array.Copy(tmpbuffer, 4, neroDAOV2.UPC, 0, 14);
                                neroDAOV2.TocType = BigEndianBitConverter.ToUInt16(tmpbuffer, 18);
                                neroDAOV1.FirstTrack = tmpbuffer[20];
                                neroDAOV2.LastTrack = tmpbuffer[21];
                                neroDAOV2.Tracks = new List<NeroV2DAOEntry>();

                                if(!ImageInfo.readableMediaTags.Contains(MediaTagType.CD_MCN))
                                    ImageInfo.readableMediaTags.Add(MediaTagType.CD_MCN);

                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDTrackISRC))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDTrackISRC);

                                UPC = neroDAOV2.UPC;

                                DicConsole.DebugWriteLine("Nero plugin", "neroDAOV2.ChunkSizeLe = {0} bytes", neroDAOV2.ChunkSizeLe);
                                DicConsole.DebugWriteLine("Nero plugin", "neroDAOV2.UPC = \"{0}\"", StringHandlers.CToString(neroDAOV2.UPC));
                                DicConsole.DebugWriteLine("Nero plugin", "neroDAOV2.TocType = 0x{0:X4}", neroDAOV2.TocType);
                                DicConsole.DebugWriteLine("Nero plugin", "neroDAOV2.FirstTrack = {0}", neroDAOV2.FirstTrack);
                                DicConsole.DebugWriteLine("Nero plugin", "neroDAOV2.LastTrack = {0}", neroDAOV2.LastTrack);

                                tmpbuffer = new byte[42];
                                for(int i = 0; i < (neroDAOV2.ChunkSizeBe - 22); i += 42)
                                {
                                    NeroV2DAOEntry _entry = new NeroV2DAOEntry();
                                    imageStream.Read(tmpbuffer, 0, 42);
                                    _entry.ISRC = new byte[12];
                                    Array.Copy(tmpbuffer, 4, _entry.ISRC, 0, 12);
                                    _entry.SectorSize = BigEndianBitConverter.ToUInt16(tmpbuffer, 12);
                                    _entry.Mode = BitConverter.ToUInt16(tmpbuffer, 14);
                                    _entry.Unknown = BigEndianBitConverter.ToUInt16(tmpbuffer, 16);
                                    _entry.Index0 = BigEndianBitConverter.ToUInt64(tmpbuffer, 18);
                                    _entry.Index1 = BigEndianBitConverter.ToUInt64(tmpbuffer, 26);
                                    _entry.EndOfTrack = BigEndianBitConverter.ToUInt64(tmpbuffer, 34);

                                    DicConsole.DebugWriteLine("Nero plugin", "Disc-At-Once entry {0}", (i / 32) + 1);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].ISRC = \"{1}\"", (i / 32) + 1, StringHandlers.CToString(_entry.ISRC));
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].SectorSize = {1}", (i / 32) + 1, _entry.SectorSize);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Mode = {1} (0x{2:X4})", (i / 32) + 1, (DAOMode)_entry.Mode, _entry.Mode);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Unknown = {1:X2}", (i / 32) + 1, _entry.Unknown);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Index0 = {1}", (i / 32) + 1, _entry.Index0);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Index1 = {1}", (i / 32) + 1, _entry.Index1);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].EndOfTrack = {1}", (i / 32) + 1, _entry.EndOfTrack);

                                    neroDAOV2.Tracks.Add(_entry);

                                    if(_entry.SectorSize > ImageInfo.sectorSize)
                                        ImageInfo.sectorSize = _entry.SectorSize;

                                    TrackISRCs.Add(currenttrack, _entry.ISRC);

                                    if(currenttrack == 1)
                                        _entry.Index0 = _entry.Index1;

                                    NeroTrack _neroTrack = new NeroTrack();
                                    _neroTrack.EndOfTrack = _entry.EndOfTrack;
                                    _neroTrack.ISRC = _entry.ISRC;
                                    _neroTrack.Length = _entry.EndOfTrack - _entry.Index0;
                                    _neroTrack.Mode = _entry.Mode;
                                    _neroTrack.Offset = _entry.Index0;
                                    _neroTrack.Sectors = _neroTrack.Length / _entry.SectorSize;
                                    _neroTrack.SectorSize = _entry.SectorSize;
                                    _neroTrack.StartLBA = ImageInfo.sectors;
                                    _neroTrack.Index0 = _entry.Index0;
                                    _neroTrack.Index1 = _entry.Index1;
                                    _neroTrack.Sequence = currenttrack;
                                    neroTracks.Add(currenttrack, _neroTrack);

                                    ImageInfo.sectors += _neroTrack.Sectors;

                                    currenttrack++;
                                }

                                break;
                            }
                        case NeroCDTextID:
                            {
                                DicConsole.DebugWriteLine("Nero plugin", "Found \"CDTX\" chunk, parsing {0} bytes", ChunkLength);

                                neroCDTXT = new NeroCDText();
                                neroCDTXT.ChunkID = ChunkID;
                                neroCDTXT.ChunkSize = ChunkLength;
                                neroCDTXT.Packs = new List<NeroCDTextPack>();

                                byte[] tmpbuffer = new byte[18];
                                for(int i = 0; i < (neroCDTXT.ChunkSize); i += 18)
                                {
                                    NeroCDTextPack _entry = new NeroCDTextPack();
                                    imageStream.Read(tmpbuffer, 0, 18);

                                    _entry.PackType = tmpbuffer[0];
                                    _entry.TrackNumber = tmpbuffer[1];
                                    _entry.PackNumber = tmpbuffer[2];
                                    _entry.BlockNumber = tmpbuffer[3];
                                    _entry.Text = new byte[12];
                                    Array.Copy(tmpbuffer, 4, _entry.Text, 0, 12);
                                    _entry.CRC = BigEndianBitConverter.ToUInt16(tmpbuffer, 16);

                                    DicConsole.DebugWriteLine("Nero plugin", "CD-TEXT entry {0}", (i / 18) + 1);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].PackType = 0x{1:X2}", (i / 18) + 1, _entry.PackType);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].TrackNumber = 0x{1:X2}", (i / 18) + 1, _entry.TrackNumber);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].PackNumber = 0x{1:X2}", (i / 18) + 1, _entry.PackNumber);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].BlockNumber = 0x{1:X2}", (i / 18) + 1, _entry.BlockNumber);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Text = \"{1}\"", (i / 18) + 1, StringHandlers.CToString(_entry.Text));
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].CRC = 0x{1:X4}", (i / 18) + 1, _entry.CRC);

                                    neroCDTXT.Packs.Add(_entry);
                                }

                                break;
                            }
                        case NeroV1TAOID:
                            {
                                DicConsole.DebugWriteLine("Nero plugin", "Found \"ETNF\" chunk, parsing {0} bytes", ChunkLength);

                                neroTAOV1 = new NeroV1TAO();
                                neroTAOV1.ChunkID = ChunkID;
                                neroTAOV1.ChunkSize = ChunkLength;
                                neroTAOV1.Tracks = new List<NeroV1TAOEntry>();

                                byte[] tmpbuffer = new byte[20];
                                for(int i = 0; i < (neroTAOV1.ChunkSize); i += 20)
                                {
                                    NeroV1TAOEntry _entry = new NeroV1TAOEntry();
                                    imageStream.Read(tmpbuffer, 0, 20);

                                    _entry.Offset = BigEndianBitConverter.ToUInt32(tmpbuffer, 0);
                                    _entry.Length = BigEndianBitConverter.ToUInt32(tmpbuffer, 4);
                                    _entry.Mode = BigEndianBitConverter.ToUInt32(tmpbuffer, 8);
                                    _entry.StartLBA = BigEndianBitConverter.ToUInt32(tmpbuffer, 12);
                                    _entry.Unknown = BigEndianBitConverter.ToUInt32(tmpbuffer, 16);

                                    DicConsole.DebugWriteLine("Nero plugin", "Track-at-Once entry {0}", (i / 20) + 1);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Offset = {1}", (i / 20) + 1, _entry.Offset);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Length = {1} bytes", (i / 20) + 1, _entry.Length);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Mode = {1} (0x{2:X4})", (i / 20) + 1, (DAOMode)_entry.Mode, _entry.Mode);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].StartLBA = {1}", (i / 20) + 1, _entry.StartLBA);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Unknown = 0x{1:X4}", (i / 20) + 1, _entry.Unknown);

                                    neroTAOV1.Tracks.Add(_entry);

                                    if(NeroTrackModeToBytesPerSector((DAOMode)_entry.Mode) > ImageInfo.sectorSize)
                                        ImageInfo.sectorSize = NeroTrackModeToBytesPerSector((DAOMode)_entry.Mode);

                                    NeroTrack _neroTrack = new NeroTrack();
                                    _neroTrack.EndOfTrack = _entry.Offset + _entry.Length;
                                    _neroTrack.ISRC = new byte[12];
                                    _neroTrack.Length = _entry.Length;
                                    _neroTrack.Mode = _entry.Mode;
                                    _neroTrack.Offset = _entry.Offset;
                                    _neroTrack.Sectors = _neroTrack.Length / NeroTrackModeToBytesPerSector((DAOMode)_entry.Mode);
                                    _neroTrack.SectorSize = NeroTrackModeToBytesPerSector((DAOMode)_entry.Mode);
                                    _neroTrack.StartLBA = ImageInfo.sectors;
                                    _neroTrack.Index0 = _entry.Offset;
                                    _neroTrack.Index1 = _entry.Offset;
                                    _neroTrack.Sequence = currenttrack;
                                    neroTracks.Add(currenttrack, _neroTrack);

                                    ImageInfo.sectors += _neroTrack.Sectors;

                                    currenttrack++;
                                }

                                break;
                            }
                        case NeroV2TAOID:
                            {
                                DicConsole.DebugWriteLine("Nero plugin", "Found \"ETN2\" chunk, parsing {0} bytes", ChunkLength);

                                neroTAOV2 = new NeroV2TAO();
                                neroTAOV2.ChunkID = ChunkID;
                                neroTAOV2.ChunkSize = ChunkLength;
                                neroTAOV2.Tracks = new List<NeroV2TAOEntry>();

                                byte[] tmpbuffer = new byte[32];
                                for(int i = 0; i < (neroTAOV2.ChunkSize); i += 32)
                                {
                                    NeroV2TAOEntry _entry = new NeroV2TAOEntry();
                                    imageStream.Read(tmpbuffer, 0, 32);

                                    _entry.Offset = BigEndianBitConverter.ToUInt64(tmpbuffer, 0);
                                    _entry.Length = BigEndianBitConverter.ToUInt64(tmpbuffer, 8);
                                    _entry.Mode = BigEndianBitConverter.ToUInt32(tmpbuffer, 16);
                                    _entry.StartLBA = BigEndianBitConverter.ToUInt32(tmpbuffer, 20);
                                    _entry.Unknown = BigEndianBitConverter.ToUInt32(tmpbuffer, 24);
                                    _entry.Sectors = BigEndianBitConverter.ToUInt32(tmpbuffer, 28);

                                    DicConsole.DebugWriteLine("Nero plugin", "Track-at-Once entry {0}", (i / 32) + 1);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Offset = {1}", (i / 32) + 1, _entry.Offset);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Length = {1} bytes", (i / 32) + 1, _entry.Length);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Mode = {1} (0x{2:X4})", (i / 32) + 1, (DAOMode)_entry.Mode, _entry.Mode);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].StartLBA = {1}", (i / 32) + 1, _entry.StartLBA);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Unknown = 0x{1:X4}", (i / 32) + 1, _entry.Unknown);
                                    DicConsole.DebugWriteLine("Nero plugin", "\t _entry[{0}].Sectors = {1}", (i / 32) + 1, _entry.Sectors);

                                    neroTAOV2.Tracks.Add(_entry);

                                    if(NeroTrackModeToBytesPerSector((DAOMode)_entry.Mode) > ImageInfo.sectorSize)
                                        ImageInfo.sectorSize = NeroTrackModeToBytesPerSector((DAOMode)_entry.Mode);

                                    NeroTrack _neroTrack = new NeroTrack();
                                    _neroTrack.EndOfTrack = _entry.Offset + _entry.Length;
                                    _neroTrack.ISRC = new byte[12];
                                    _neroTrack.Length = _entry.Length;
                                    _neroTrack.Mode = _entry.Mode;
                                    _neroTrack.Offset = _entry.Offset;
                                    _neroTrack.Sectors = _neroTrack.Length / NeroTrackModeToBytesPerSector((DAOMode)_entry.Mode);
                                    _neroTrack.SectorSize = NeroTrackModeToBytesPerSector((DAOMode)_entry.Mode);
                                    _neroTrack.StartLBA = ImageInfo.sectors;
                                    _neroTrack.Index0 = _entry.Offset;
                                    _neroTrack.Index1 = _entry.Offset;
                                    _neroTrack.Sequence = currenttrack;
                                    neroTracks.Add(currenttrack, _neroTrack);

                                    ImageInfo.sectors += _neroTrack.Sectors;

                                    currenttrack++;
                                }

                                break;
                            }
                        case NeroSessionID:
                            {
                                DicConsole.DebugWriteLine("Nero plugin", "Found \"SINF\" chunk, parsing {0} bytes", ChunkLength);

                                uint sessionTracks;
                                byte[] tmpbuffer = new byte[4];
                                imageStream.Read(tmpbuffer, 0, 4);
                                sessionTracks = BigEndianBitConverter.ToUInt32(tmpbuffer, 0);
                                neroSessions.Add(currentsession, sessionTracks);

                                DicConsole.DebugWriteLine("Nero plugin", "\tSession {0} has {1} tracks", currentsession, sessionTracks);

                                currentsession++;
                                break;
                            }
                        case NeroDiskTypeID:
                            {
                                DicConsole.DebugWriteLine("Nero plugin", "Found \"MTYP\" chunk, parsing {0} bytes", ChunkLength);

                                neroMediaTyp = new NeroMediaType();

                                neroMediaTyp.ChunkID = ChunkID;
                                neroMediaTyp.ChunkSize = ChunkLength;

                                byte[] tmpbuffer = new byte[4];
                                imageStream.Read(tmpbuffer, 0, 4);
                                neroMediaTyp.Type = BigEndianBitConverter.ToUInt32(tmpbuffer, 0);

                                DicConsole.DebugWriteLine("Nero plugin", "\tMedia type is {0} ({1})", (NeroMediaTypes)neroMediaTyp.Type, neroMediaTyp.Type);

                                ImageInfo.mediaType = NeroMediaTypeToMediaType((NeroMediaTypes)neroMediaTyp.Type);

                                break;
                            }
                        case NeroDiscInfoID:
                            {
                                DicConsole.DebugWriteLine("Nero plugin", "Found \"DINF\" chunk, parsing {0} bytes", ChunkLength);

                                neroDiscInfo = new NeroDiscInformation();
                                neroDiscInfo.ChunkID = ChunkID;
                                neroDiscInfo.ChunkSize = ChunkLength;
                                byte[] tmpbuffer = new byte[4];
                                imageStream.Read(tmpbuffer, 0, 4);
                                neroDiscInfo.Unknown = BigEndianBitConverter.ToUInt32(tmpbuffer, 0);

                                DicConsole.DebugWriteLine("Nero plugin", "\tneroDiscInfo.Unknown = 0x{0:X4} ({0})", neroDiscInfo.Unknown);

                                break;
                            }
                        case NeroReloID:
                            {
                                DicConsole.DebugWriteLine("Nero plugin", "Found \"RELO\" chunk, parsing {0} bytes", ChunkLength);

                                neroRELO = new NeroRELOChunk();
                                neroRELO.ChunkID = ChunkID;
                                neroRELO.ChunkSize = ChunkLength;
                                byte[] tmpbuffer = new byte[4];
                                imageStream.Read(tmpbuffer, 0, 4);
                                neroRELO.Unknown = BigEndianBitConverter.ToUInt32(tmpbuffer, 0);

                                DicConsole.DebugWriteLine("Nero plugin", "\tneroRELO.Unknown = 0x{0:X4} ({0})", neroRELO.Unknown);

                                break;
                            }
                        case NeroTOCID:
                            {
                                DicConsole.DebugWriteLine("Nero plugin", "Found \"TOCT\" chunk, parsing {0} bytes", ChunkLength);

                                neroTOC = new NeroTOCChunk();
                                neroTOC.ChunkID = ChunkID;
                                neroTOC.ChunkSize = ChunkLength;
                                byte[] tmpbuffer = new byte[2];
                                imageStream.Read(tmpbuffer, 0, 2);
                                neroTOC.Unknown = BigEndianBitConverter.ToUInt16(tmpbuffer, 0);

                                DicConsole.DebugWriteLine("Nero plugin", "\tneroTOC.Unknown = 0x{0:X4} ({0})", neroTOC.Unknown);

                                break;
                            }
                        case NeroEndID:
                            {
                                DicConsole.DebugWriteLine("Nero plugin", "Found \"END!\" chunk, finishing parse");
                                parsing = false;
                                break;
                            }
                        default:
                            {
                                DicConsole.DebugWriteLine("Nero plugin", "Unknown chunk ID \"{0}\", skipping...", System.Text.Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(ChunkID)));
                                imageStream.Seek(ChunkLength, SeekOrigin.Current);
                                break;
                            }
                    }
                }

                ImageInfo.imageHasPartitions = true;
                ImageInfo.imageHasSessions = true;
                ImageInfo.imageCreator = null;
                ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
                ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
                ImageInfo.imageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
                ImageInfo.imageComments = null;
                ImageInfo.mediaManufacturer = null;
                ImageInfo.mediaModel = null;
                ImageInfo.mediaSerialNumber = null;
                ImageInfo.mediaBarcode = null;
                ImageInfo.mediaPartNumber = null;
                ImageInfo.driveManufacturer = null;
                ImageInfo.driveModel = null;
                ImageInfo.driveSerialNumber = null;
                ImageInfo.driveFirmwareRevision = null;
                ImageInfo.mediaSequence = 0;
                ImageInfo.lastMediaSequence = 0;
                if(imageNewFormat)
                {
                    ImageInfo.imageSize = footerV2.FirstChunkOffset;
                    ImageInfo.imageVersion = "Nero Burning ROM >= 5.5";
                    ImageInfo.imageApplication = "Nero Burning ROM";
                    ImageInfo.imageApplicationVersion = ">= 5.5";
                }
                else
                {
                    ImageInfo.imageSize = footerV1.FirstChunkOffset;
                    ImageInfo.imageVersion = "Nero Burning ROM <= 5.0";
                    ImageInfo.imageApplication = "Nero Burning ROM";
                    ImageInfo.imageApplicationVersion = "<= 5.0";
                }

                if(neroSessions.Count == 0)
                    neroSessions.Add(1, currenttrack);

                DicConsole.DebugWriteLine("Nero plugin", "Building offset, track and session maps");

                currentsession = 1;
                uint currentsessionmaxtrack;
                neroSessions.TryGetValue(1, out currentsessionmaxtrack);
                uint currentsessioncurrenttrack = 1;
                Session currentsessionstruct = new Session();
                ulong PartitionSequence = 0;
                ulong partitionStartByte = 0;
                for(uint i = 1; i <= neroTracks.Count; i++)
                {
                    NeroTrack _neroTrack;
                    if(neroTracks.TryGetValue(i, out _neroTrack))
                    {
                        DicConsole.DebugWriteLine("Nero plugin", "\tcurrentsession = {0}", currentsession);
                        DicConsole.DebugWriteLine("Nero plugin", "\tcurrentsessionmaxtrack = {0}", currentsessionmaxtrack);
                        DicConsole.DebugWriteLine("Nero plugin", "\tcurrentsessioncurrenttrack = {0}", currentsessioncurrenttrack);

                        Track _track = new Track();
                        if(_neroTrack.Sequence == 1)
                            _neroTrack.Index0 = _neroTrack.Index1;

                        _track.Indexes = new Dictionary<int, ulong>();
                        if(_neroTrack.Index0 < _neroTrack.Index1)
                            _track.Indexes.Add(0, _neroTrack.Index0 / _neroTrack.SectorSize);
                        _track.Indexes.Add(1, _neroTrack.Index1 / _neroTrack.SectorSize);
                        _track.TrackDescription = StringHandlers.CToString(_neroTrack.ISRC);
                        _track.TrackEndSector = (_neroTrack.Length / _neroTrack.SectorSize) + _neroTrack.StartLBA - 1;
                        _track.TrackPregap = (_neroTrack.Index1 - _neroTrack.Index0) / _neroTrack.SectorSize;
                        _track.TrackSequence = _neroTrack.Sequence;
                        _track.TrackSession = currentsession;
                        _track.TrackStartSector = _neroTrack.StartLBA;
                        _track.TrackType = NeroTrackModeToTrackType((DAOMode)_neroTrack.Mode);
                        _track.TrackFile = imageFilter.GetFilename();
                        _track.TrackFilter = imageFilter;
                        _track.TrackFileOffset = _neroTrack.Offset;
                        _track.TrackFileType = "BINARY";
                        _track.TrackSubchannelType = TrackSubchannelType.None;
                        switch((DAOMode)_neroTrack.Mode)
                        {
                            case DAOMode.Audio:
                                _track.TrackBytesPerSector = 2352;
                                _track.TrackRawBytesPerSector = 2352;
                                break;
                            case DAOMode.AudioSub:
                                _track.TrackBytesPerSector = 2352;
                                _track.TrackRawBytesPerSector = 2448;
                                _track.TrackSubchannelType = TrackSubchannelType.RawInterleaved;
                                break;
                            case DAOMode.Data:
                            case DAOMode.DataM2F1:
                                _track.TrackBytesPerSector = 2048;
                                _track.TrackRawBytesPerSector = 2048;
                                break;
                            case DAOMode.DataM2F2:
                                _track.TrackBytesPerSector = 2336;
                                _track.TrackRawBytesPerSector = 2336;
                                break;
                            case DAOMode.DataM2Raw:
                                _track.TrackBytesPerSector = 2352;
                                _track.TrackRawBytesPerSector = 2352;
                                break;
                            case DAOMode.DataM2RawSub:
                                _track.TrackBytesPerSector = 2352;
                                _track.TrackRawBytesPerSector = 2448;
                                _track.TrackSubchannelType = TrackSubchannelType.RawInterleaved;
                                break;
                            case DAOMode.DataRaw:
                                _track.TrackBytesPerSector = 2048;
                                _track.TrackRawBytesPerSector = 2352;
                                break;
                            case DAOMode.DataRawSub:
                                _track.TrackBytesPerSector = 2048;
                                _track.TrackRawBytesPerSector = 2448;
                                _track.TrackSubchannelType = TrackSubchannelType.RawInterleaved;
                                break;
                        }

                        if(_track.TrackSubchannelType == TrackSubchannelType.RawInterleaved)
                        {
                            _track.TrackSubchannelFilter = imageFilter;
                            _track.TrackSubchannelFile = imageFilter.GetFilename();
                            _track.TrackSubchannelOffset = _neroTrack.Offset;
                        }

                        imageTracks.Add(_track);

                        DicConsole.DebugWriteLine("Nero plugin", "\t\t _track.TrackDescription = {0}", _track.TrackDescription);
                        DicConsole.DebugWriteLine("Nero plugin", "\t\t _track.TrackEndSector = {0}", _track.TrackEndSector);
                        DicConsole.DebugWriteLine("Nero plugin", "\t\t _track.TrackPregap = {0}", _track.TrackPregap);
                        DicConsole.DebugWriteLine("Nero plugin", "\t\t _track.TrackSequence = {0}", _track.TrackSequence);
                        DicConsole.DebugWriteLine("Nero plugin", "\t\t _track.TrackSession = {0}", _track.TrackSession);
                        DicConsole.DebugWriteLine("Nero plugin", "\t\t _track.TrackStartSector = {0}", _track.TrackStartSector);
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
                        DicConsole.DebugWriteLine("Nero plugin", "\t\t Offset[{0}]: {1}", _track.TrackSequence, _track.TrackStartSector);

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
                        partition.PartitionDescription = string.Format("Track {0} Index 1", _track.TrackSequence);
                        partition.PartitionLength = (_neroTrack.EndOfTrack - _neroTrack.Index1);
                        partition.PartitionName = StringHandlers.CToString(_neroTrack.ISRC);
                        partition.PartitionSectors = partition.PartitionLength / _neroTrack.SectorSize;
                        partition.PartitionSequence = PartitionSequence;
                        partition.PartitionStart = partitionStartByte;
                        partition.PartitionStartSector = _neroTrack.StartLBA + ((_neroTrack.Index1 - _neroTrack.Index0) / _neroTrack.SectorSize);
                        partition.PartitionType = NeroTrackModeToTrackType((DAOMode)_neroTrack.Mode).ToString();
                        ImagePartitions.Add(partition);
                        PartitionSequence++;
                        partitionStartByte += partition.PartitionLength;
                    }
                }

                _imageFilter = imageFilter;

                if(ImageInfo.mediaType == MediaType.Unknown || ImageInfo.mediaType == MediaType.CD)
                {
                    bool data = false;
                    bool mode2 = false;
                    bool firstaudio = false;
                    bool firstdata = false;
                    bool audio = false;

                    for(uint i = 0; i < neroTracks.Count; i++)
                    {
                        // First track is audio
                        firstaudio |= i == 0 && ((DAOMode)neroTracks[i].Mode == DAOMode.Audio || (DAOMode)neroTracks[i].Mode == DAOMode.AudioSub);

                        // First track is data
                        firstdata |= i == 0 && ((DAOMode)neroTracks[i].Mode != DAOMode.Audio && (DAOMode)neroTracks[i].Mode != DAOMode.AudioSub);

                        // Any non first track is data
                        data |= i != 0 && ((DAOMode)neroTracks[i].Mode != DAOMode.Audio && (DAOMode)neroTracks[i].Mode != DAOMode.AudioSub);

                        // Any non first track is audio
                        audio |= i != 0 && ((DAOMode)neroTracks[i].Mode == DAOMode.Audio || (DAOMode)neroTracks[i].Mode == DAOMode.AudioSub);

                        switch((DAOMode)neroTracks[i].Mode)
                        {
                            case DAOMode.DataM2F1:
                            case DAOMode.DataM2F2:
                            case DAOMode.DataM2Raw:
                            case DAOMode.DataM2RawSub:
                                mode2 = true;
                                break;
                        }
                    }

                    if(!data && !firstdata)
                        ImageInfo.mediaType = MediaType.CDDA;
                    else if(firstaudio && data && imageSessions.Count > 1 && mode2)
                        ImageInfo.mediaType = MediaType.CDPLUS;
                    else if((firstdata && audio) || mode2)
                        ImageInfo.mediaType = MediaType.CDROMXA;
                    else if(!audio)
                        ImageInfo.mediaType = MediaType.CDROM;
                    else
                        ImageInfo.mediaType = MediaType.CD;
                }


                ImageInfo.xmlMediaType = XmlMediaType.OpticalDisc;
                DicConsole.VerboseWriteLine("Nero image contains a disc of type {0}", ImageInfo.mediaType);

                return true;
            }
            catch
            {
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
            return ImageInfo.imageSize;
        }

        public override ulong GetSectors()
        {
            return ImageInfo.sectors;
        }

        public override uint GetSectorSize()
        {
            return ImageInfo.sectorSize;
        }

        public override byte[] ReadDiskTag(MediaTagType tag)
        {
            switch(tag)
            {
                case MediaTagType.CD_MCN:
                    return UPC;
                case MediaTagType.CD_TEXT:
                    throw new NotImplementedException("Not yet implemented");
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
            {
                if(sectorAddress >= kvp.Value)
                {
                    foreach(Track _track in imageTracks)
                    {
                        if(_track.TrackSequence == kvp.Key)
                        {
                            if((sectorAddress - kvp.Value) < (_track.TrackEndSector - _track.TrackStartSector))
                                return ReadSectors((sectorAddress - kvp.Value), length, kvp.Key);
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            foreach(KeyValuePair<uint, ulong> kvp in offsetmap)
            {
                if(sectorAddress >= kvp.Value)
                {
                    foreach(Track _track in imageTracks)
                    {
                        if(_track.TrackSequence == kvp.Key)
                        {
                            if((sectorAddress - kvp.Value) < (_track.TrackEndSector - _track.TrackStartSector))
                                return ReadSectorsTag((sectorAddress - kvp.Value), length, kvp.Key, tag);
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            NeroTrack _track;

            if(!neroTracks.TryGetValue(track, out _track))
                throw new ArgumentOutOfRangeException(nameof(track), "Track not found");

            if(length > _track.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), string.Format("Requested more sectors ({0}) than present in track ({1}), won't cross tracks", length, _track.Sectors));

            uint sector_offset;
            uint sector_size;
            uint sector_skip;

            switch((DAOMode)_track.Mode)
            {
                case DAOMode.Data:
                case DAOMode.DataM2F1:
                    {
                        sector_offset = 0;
                        sector_size = 2048;
                        sector_skip = 0;
                        break;
                    }
                case DAOMode.DataM2F2:
                    {
                        sector_offset = 8;
                        sector_size = 2324;
                        sector_skip = 4;
                        break;
                    }
                case DAOMode.Audio:
                    {
                        sector_offset = 0;
                        sector_size = 2352;
                        sector_skip = 0;
                        break;
                    }
                case DAOMode.DataRaw:
                    {
                        sector_offset = 16;
                        sector_size = 2048;
                        sector_skip = 288;
                        break;
                    }
                case DAOMode.DataM2Raw:
                    {
                        sector_offset = 16;
                        sector_size = 2336;
                        sector_skip = 0;
                        break;
                    }
                // TODO: Supposing Nero suffixes the subchannel to the channel
                case DAOMode.DataRawSub:
                    {
                        sector_offset = 16;
                        sector_size = 2048;
                        sector_skip = 288 + 96;
                        break;
                    }
                case DAOMode.DataM2RawSub:
                    {
                        sector_offset = 16;
                        sector_size = 2336;
                        sector_skip = 96;
                        break;
                    }
                case DAOMode.AudioSub:
                    {
                        sector_offset = 0;
                        sector_size = 2352;
                        sector_skip = 96;
                        break;
                    }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sector_size * length];

            imageStream = _imageFilter.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);
            br.BaseStream.Seek((long)_track.Offset + (long)(sectorAddress * (sector_offset + sector_size + sector_skip)), SeekOrigin.Begin);
            if(sector_offset == 0 && sector_skip == 0)
                buffer = br.ReadBytes((int)(sector_size * length));
            else
            {
                for(int i = 0; i < length; i++)
                {
                    byte[] sector;
                    br.BaseStream.Seek(sector_offset, SeekOrigin.Current);
                    sector = br.ReadBytes((int)sector_size);
                    br.BaseStream.Seek(sector_skip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sector_size, sector_size);
                }
            }

            return buffer;
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            NeroTrack _track;

            if(!neroTracks.TryGetValue(track, out _track))
                throw new ArgumentOutOfRangeException(nameof(track), "Track not found");

            if(length > _track.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), string.Format("Requested more sectors ({0}) than present in track ({1}), won't cross tracks", length, _track.Sectors));

            uint sector_offset;
            uint sector_size;
            uint sector_skip;

            switch(tag)
            {
                case SectorTagType.CDSectorECC:
                case SectorTagType.CDSectorECC_P:
                case SectorTagType.CDSectorECC_Q:
                case SectorTagType.CDSectorEDC:
                case SectorTagType.CDSectorHeader:
                case SectorTagType.CDSectorSubchannel:
                case SectorTagType.CDSectorSubHeader:
                case SectorTagType.CDSectorSync:
                    break;
                case SectorTagType.CDTrackFlags:
                    {
                        byte[] flags = new byte[1];
                        flags[0] = 0x00;

                        if((DAOMode)_track.Mode != DAOMode.Audio && (DAOMode)_track.Mode != DAOMode.AudioSub)
                            flags[0] += 0x40;

                        return flags;
                    }
                case SectorTagType.CDTrackISRC:
                    return _track.ISRC;
                case SectorTagType.CDTrackText:
                    throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
                default:
                    throw new ArgumentException("Unsupported tag requested", nameof(tag));
            }

            switch((DAOMode)_track.Mode)
            {
                case DAOMode.Data:
                case DAOMode.DataM2F1:
                    throw new ArgumentException("No tags in image for requested track", nameof(tag));
                case DAOMode.DataM2F2:
                    {
                        switch(tag)
                        {
                            case SectorTagType.CDSectorSync:
                            case SectorTagType.CDSectorHeader:
                            case SectorTagType.CDSectorSubchannel:
                            case SectorTagType.CDSectorECC:
                            case SectorTagType.CDSectorECC_P:
                            case SectorTagType.CDSectorECC_Q:
                                throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                            case SectorTagType.CDSectorSubHeader:
                                {
                                    sector_offset = 0;
                                    sector_size = 8;
                                    sector_skip = 2328;
                                    break;
                                }
                            case SectorTagType.CDSectorEDC:
                                {
                                    sector_offset = 2332;
                                    sector_size = 4;
                                    sector_skip = 0;
                                    break;
                                }
                            default:
                                throw new ArgumentException("Unsupported tag requested", nameof(tag));
                        }
                        break;
                    }
                case DAOMode.Audio:
                    throw new ArgumentException("There are no tags on audio tracks", nameof(tag));
                case DAOMode.DataRaw:
                    {
                        switch(tag)
                        {
                            case SectorTagType.CDSectorSync:
                                {
                                    sector_offset = 0;
                                    sector_size = 12;
                                    sector_skip = 2340;
                                    break;
                                }
                            case SectorTagType.CDSectorHeader:
                                {
                                    sector_offset = 12;
                                    sector_size = 4;
                                    sector_skip = 2336;
                                    break;
                                }
                            case SectorTagType.CDSectorSubchannel:
                            case SectorTagType.CDSectorSubHeader:
                                throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                            case SectorTagType.CDSectorECC:
                                {
                                    sector_offset = 2076;
                                    sector_size = 276;
                                    sector_skip = 0;
                                    break;
                                }
                            case SectorTagType.CDSectorECC_P:
                                {
                                    sector_offset = 2076;
                                    sector_size = 172;
                                    sector_skip = 104;
                                    break;
                                }
                            case SectorTagType.CDSectorECC_Q:
                                {
                                    sector_offset = 2248;
                                    sector_size = 104;
                                    sector_skip = 0;
                                    break;
                                }
                            case SectorTagType.CDSectorEDC:
                                {
                                    sector_offset = 2064;
                                    sector_size = 4;
                                    sector_skip = 284;
                                    break;
                                }
                            default:
                                throw new ArgumentException("Unsupported tag requested", nameof(tag));
                        }
                        break;
                    }
                // TODO
                case DAOMode.DataM2RawSub:
                    throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
                case DAOMode.DataRawSub:
                    {
                        switch(tag)
                        {
                            case SectorTagType.CDSectorSync:
                                {
                                    sector_offset = 0;
                                    sector_size = 12;
                                    sector_skip = 2340 + 96;
                                    break;
                                }
                            case SectorTagType.CDSectorHeader:
                                {
                                    sector_offset = 12;
                                    sector_size = 4;
                                    sector_skip = 2336 + 96;
                                    break;
                                }
                            case SectorTagType.CDSectorSubchannel:
                                {
                                    sector_offset = 2352;
                                    sector_size = 96;
                                    sector_skip = 0;
                                    break;
                                }
                            case SectorTagType.CDSectorSubHeader:
                                throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                            case SectorTagType.CDSectorECC:
                                {
                                    sector_offset = 2076;
                                    sector_size = 276;
                                    sector_skip = 0 + 96;
                                    break;
                                }
                            case SectorTagType.CDSectorECC_P:
                                {
                                    sector_offset = 2076;
                                    sector_size = 172;
                                    sector_skip = 104 + 96;
                                    break;
                                }
                            case SectorTagType.CDSectorECC_Q:
                                {
                                    sector_offset = 2248;
                                    sector_size = 104;
                                    sector_skip = 0 + 96;
                                    break;
                                }
                            case SectorTagType.CDSectorEDC:
                                {
                                    sector_offset = 2064;
                                    sector_size = 4;
                                    sector_skip = 284 + 96;
                                    break;
                                }
                            default:
                                throw new ArgumentException("Unsupported tag requested", nameof(tag));
                        }
                        break;
                    }
                case DAOMode.AudioSub:
                    {
                        if(tag != SectorTagType.CDSectorSubchannel)
                            throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));

                        sector_offset = 2352;
                        sector_size = 96;
                        sector_skip = 0;
                        break;
                    }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sector_size * length];

            imageStream = _imageFilter.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);
            br.BaseStream.Seek((long)_track.Offset + (long)(sectorAddress * (sector_offset + sector_size + sector_skip)), SeekOrigin.Begin);
            if(sector_offset == 0 && sector_skip == 0)
                buffer = br.ReadBytes((int)(sector_size * length));
            else
            {
                for(int i = 0; i < length; i++)
                {
                    byte[] sector;
                    br.BaseStream.Seek(sector_offset, SeekOrigin.Current);
                    sector = br.ReadBytes((int)sector_size);
                    br.BaseStream.Seek(sector_skip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sector_size, sector_size);
                }
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
            {
                if(sectorAddress >= kvp.Value)
                {
                    foreach(Track _track in imageTracks)
                    {
                        if(_track.TrackSequence == kvp.Key)
                        {
                            if((sectorAddress - kvp.Value) < (_track.TrackEndSector - _track.TrackStartSector))
                                return ReadSectorsLong((sectorAddress - kvp.Value), length, kvp.Key);
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            NeroTrack _track;

            if(!neroTracks.TryGetValue(track, out _track))
                throw new ArgumentOutOfRangeException(nameof(track), "Track not found");

            if(length > _track.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), string.Format("Requested more sectors ({0}) than present in track ({1}), won't cross tracks", length, _track.Sectors));

            uint sector_offset;
            uint sector_size;
            uint sector_skip;

            switch((DAOMode)_track.Mode)
            {
                case DAOMode.Data:
                case DAOMode.DataM2F1:
                    {
                        sector_offset = 0;
                        sector_size = 2048;
                        sector_skip = 0;
                        break;
                    }
                case DAOMode.DataM2F2:
                    {
                        sector_offset = 0;
                        sector_size = 2336;
                        sector_skip = 0;
                        break;
                    }
                case DAOMode.DataRaw:
                case DAOMode.DataM2Raw:
                case DAOMode.Audio:
                    {
                        sector_offset = 0;
                        sector_size = 2352;
                        sector_skip = 0;
                        break;
                    }
                case DAOMode.DataRawSub:
                case DAOMode.DataM2RawSub:
                case DAOMode.AudioSub:
                    {
                        sector_offset = 0;
                        sector_size = 2448;
                        sector_skip = 0;
                        break;
                    }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sector_size * length];

            imageStream = _imageFilter.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);

            br.BaseStream.Seek((long)_track.Offset + (long)(sectorAddress * (sector_offset + sector_size + sector_skip)), SeekOrigin.Begin);

            if(sector_offset == 0 && sector_skip == 0)
                buffer = br.ReadBytes((int)(sector_size * length));
            else
            {
                for(int i = 0; i < length; i++)
                {
                    byte[] sector;
                    br.BaseStream.Seek(sector_offset, SeekOrigin.Current);
                    sector = br.ReadBytes((int)sector_size);
                    br.BaseStream.Seek(sector_skip, SeekOrigin.Current);

                    Array.Copy(sector, 0, buffer, i * sector_size, sector_size);
                }
            }

            return buffer;
        }

        public override string GetImageFormat()
        {
            return "Nero Burning ROM";
        }

        public override string GetImageVersion()
        {
            return ImageInfo.imageVersion;
        }

        public override string GetImageApplication()
        {
            return ImageInfo.imageApplication;
        }

        public override string GetImageApplicationVersion()
        {
            return ImageInfo.imageApplicationVersion;
        }

        public override DateTime GetImageCreationTime()
        {
            return ImageInfo.imageCreationTime;
        }

        public override DateTime GetImageLastModificationTime()
        {
            return ImageInfo.imageLastModificationTime;
        }

        public override string GetMediaBarcode()
        {
            return ImageInfo.mediaBarcode;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.mediaType;
        }

        public override List<Partition> GetPartitions()
        {
            return ImagePartitions;
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
            foreach(Track _track in imageTracks)
                if(_track.TrackSession == session)
                    sessionTracks.Add(_track);

            return sessionTracks;
        }

        public override List<Session> GetSessions()
        {
            return imageSessions;
        }

        public override bool? VerifySector(ulong sectorAddress)
        {
            byte[] buffer = ReadSectorLong(sectorAddress);
            return Checksums.CDChecksums.CheckCDSector(buffer);
        }

        public override bool? VerifySector(ulong sectorAddress, uint track)
        {
            byte[] buffer = ReadSectorLong(sectorAddress, track);
            return Checksums.CDChecksums.CheckCDSector(buffer);
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
        {
            byte[] buffer = ReadSectorsLong(sectorAddress, length);
            int bps = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];
            FailingLBAs = new List<ulong>();
            UnknownLBAs = new List<ulong>();

            for(int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * bps, sector, 0, bps);
                bool? sectorStatus = Checksums.CDChecksums.CheckCDSector(sector);

                switch(sectorStatus)
                {
                    case null:
                        UnknownLBAs.Add((ulong)i + sectorAddress);
                        break;
                    case false:
                        FailingLBAs.Add((ulong)i + sectorAddress);
                        break;
                }
            }

            if(UnknownLBAs.Count > 0)
                return null;
            if(FailingLBAs.Count > 0)
                return false;
            return true;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
        {
            byte[] buffer = ReadSectorsLong(sectorAddress, length, track);
            int bps = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];
            FailingLBAs = new List<ulong>();
            UnknownLBAs = new List<ulong>();

            for(int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * bps, sector, 0, bps);
                bool? sectorStatus = Checksums.CDChecksums.CheckCDSector(sector);

                switch(sectorStatus)
                {
                    case null:
                        UnknownLBAs.Add((ulong)i + sectorAddress);
                        break;
                    case false:
                        FailingLBAs.Add((ulong)i + sectorAddress);
                        break;
                }
            }

            if(UnknownLBAs.Count > 0)
                return null;
            if(FailingLBAs.Count > 0)
                return false;
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
                case NeroMediaTypes.NERO_MTYP_DDCD:
                    return MediaType.DDCD;
                case NeroMediaTypes.NERO_MTYP_DVD_M:
                case NeroMediaTypes.NERO_MTYP_DVD_M_R:
                    return MediaType.DVDR;
                case NeroMediaTypes.NERO_MTYP_DVD_P:
                case NeroMediaTypes.NERO_MTYP_DVD_P_R:
                    return MediaType.DVDPR;
                case NeroMediaTypes.NERO_MTYP_DVD_RAM:
                    return MediaType.DVDRAM;
                case NeroMediaTypes.NERO_MTYP_ML:
                case NeroMediaTypes.NERO_MTYP_MRW:
                case NeroMediaTypes.NERO_MTYP_CDRW:
                    return MediaType.CDRW;
                case NeroMediaTypes.NERO_MTYP_CDR:
                    return MediaType.CDR;
                case NeroMediaTypes.NERO_MTYP_DVD_ROM:
                case NeroMediaTypes.NERO_MTYP_DVD_ANY:
                case NeroMediaTypes.NERO_MTYP_DVD_ANY_R9:
                case NeroMediaTypes.NERO_MTYP_DVD_ANY_OLD:
                    return MediaType.DVDROM;
                case NeroMediaTypes.NERO_MTYP_CDROM:
                    return MediaType.CDROM;
                case NeroMediaTypes.NERO_MTYP_DVD_M_RW:
                    return MediaType.DVDRW;
                case NeroMediaTypes.NERO_MTYP_DVD_P_RW:
                    return MediaType.DVDPRW;
                case NeroMediaTypes.NERO_MTYP_DVD_P_R9:
                    return MediaType.DVDPRDL;
                case NeroMediaTypes.NERO_MTYP_DVD_M_R9:
                    return MediaType.DVDRDL;
                case NeroMediaTypes.NERO_MTYP_BD:
                case NeroMediaTypes.NERO_MTYP_BD_ANY:
                case NeroMediaTypes.NERO_MTYP_BD_ROM:
                    return MediaType.BDROM;
                case NeroMediaTypes.NERO_MTYP_BD_R:
                    return MediaType.BDR;
                case NeroMediaTypes.NERO_MTYP_BD_RE:
                    return MediaType.BDRE;
                case NeroMediaTypes.NERO_MTYP_HD_DVD:
                case NeroMediaTypes.NERO_MTYP_HD_DVD_ANY:
                case NeroMediaTypes.NERO_MTYP_HD_DVD_ROM:
                    return MediaType.HDDVDROM;
                case NeroMediaTypes.NERO_MTYP_HD_DVD_R:
                    return MediaType.HDDVDR;
                case NeroMediaTypes.NERO_MTYP_HD_DVD_RW:
                    return MediaType.HDDVDRW;
                default:
                    return MediaType.CD;
            }
        }

        static TrackType NeroTrackModeToTrackType(DAOMode mode)
        {
            switch(mode)
            {
                case DAOMode.Data:
                case DAOMode.DataRaw:
                case DAOMode.DataRawSub:
                    return TrackType.CDMode1;
                case DAOMode.DataM2F1:
                    return TrackType.CDMode2Form1;
                case DAOMode.DataM2F2:
                    return TrackType.CDMode2Form2;
                case DAOMode.DataM2RawSub:
                case DAOMode.DataM2Raw:
                    return TrackType.CDMode2Formless;
                case DAOMode.Audio:
                case DAOMode.AudioSub:
                    return TrackType.Audio;
                default:
                    return TrackType.Data;
            }
        }

        static ushort NeroTrackModeToBytesPerSector(DAOMode mode)
        {
            switch(mode)
            {
                case DAOMode.Data:
                case DAOMode.DataM2F1:
                    return 2048;
                case DAOMode.DataM2F2:
                    return 2336;
                case DAOMode.DataRaw:
                case DAOMode.DataM2Raw:
                case DAOMode.Audio:
                    return 2352;
                case DAOMode.DataM2RawSub:
                case DAOMode.DataRawSub:
                case DAOMode.AudioSub:
                    return 2448;
                default:
                    return 2352;
            }
        }

        #endregion

        #region Unsupported features

        public override int GetMediaSequence()
        {
            return ImageInfo.mediaSequence;
        }

        public override int GetLastDiskSequence()
        {
            return ImageInfo.lastMediaSequence;
        }

        public override string GetDriveManufacturer()
        {
            return ImageInfo.driveManufacturer;
        }

        public override string GetDriveModel()
        {
            return ImageInfo.driveModel;
        }

        public override string GetDriveSerialNumber()
        {
            return ImageInfo.driveSerialNumber;
        }

        public override string GetMediaPartNumber()
        {
            return ImageInfo.mediaPartNumber;
        }

        public override string GetMediaManufacturer()
        {
            return ImageInfo.mediaManufacturer;
        }

        public override string GetMediaModel()
        {
            return ImageInfo.mediaModel;
        }

        public override string GetImageName()
        {
            return ImageInfo.imageName;
        }

        public override string GetImageCreator()
        {
            return ImageInfo.imageCreator;
        }

        public override string GetImageComments()
        {
            return ImageInfo.imageComments;
        }

        public override string GetMediaSerialNumber()
        {
            return ImageInfo.mediaSerialNumber;
        }

        #endregion
    }
}