// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains structures for Nero Burning ROM disc images.
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

using System.Collections.Generic;

namespace DiscImageChef.DiscImages
{
    public partial class Nero
    {
        struct NeroV1Footer
        {
            /// <summary>
            ///     "NERO"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            ///     Offset of first chunk in file
            /// </summary>
            public uint FirstChunkOffset;
        }

        struct NeroV2Footer
        {
            /// <summary>
            ///     "NER5"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            ///     Offset of first chunk in file
            /// </summary>
            public ulong FirstChunkOffset;
        }

        struct NeroV2CueEntry
        {
            /// <summary>
            ///     Track mode. 0x01 for audio, 0x21 for copy-protected audio, 0x41 for data
            /// </summary>
            public byte Mode;

            /// <summary>
            ///     Track number in BCD
            /// </summary>
            public byte TrackNumber;

            /// <summary>
            ///     Index number in BCD
            /// </summary>
            public byte IndexNumber;

            /// <summary>
            ///     Always zero
            /// </summary>
            public byte Dummy;

            /// <summary>
            ///     LBA sector start for this entry
            /// </summary>
            public int LbaStart;
        }

        struct NeroV2Cuesheet
        {
            /// <summary>
            ///     "CUEX"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            ///     Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            ///     Cuesheet entries
            /// </summary>
            public List<NeroV2CueEntry> Entries;
        }

        struct NeroV1CueEntry
        {
            /// <summary>
            ///     Track mode. 0x01 for audio, 0x21 for copy-protected audio, 0x41 for data
            /// </summary>
            public byte Mode;

            /// <summary>
            ///     Track number in BCD
            /// </summary>
            public byte TrackNumber;

            /// <summary>
            ///     Index number in BCD
            /// </summary>
            public byte IndexNumber;

            /// <summary>
            ///     Always zero
            /// </summary>
            public ushort Dummy;

            /// <summary>
            ///     MSF start sector's minute for this entry
            /// </summary>
            public byte Minute;

            /// <summary>
            ///     MSF start sector's second for this entry
            /// </summary>
            public byte Second;

            /// <summary>
            ///     MSF start sector's frame for this entry
            /// </summary>
            public byte Frame;
        }

        struct NeroV1Cuesheet
        {
            /// <summary>
            ///     "CUES"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            ///     Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            ///     Cuesheet entries
            /// </summary>
            public List<NeroV1CueEntry> Entries;
        }

        struct NeroV1DaoEntry
        {
            /// <summary>
            ///     ISRC (12 bytes)
            /// </summary>
            public byte[] Isrc;

            /// <summary>
            ///     Size of sector inside image (in bytes)
            /// </summary>
            public ushort SectorSize;

            /// <summary>
            ///     Sector mode in image
            /// </summary>
            public ushort Mode;

            /// <summary>
            ///     Unknown
            /// </summary>
            public ushort Unknown;

            /// <summary>
            ///     Index 0 start
            /// </summary>
            public uint Index0;

            /// <summary>
            ///     Index 1 start
            /// </summary>
            public uint Index1;

            /// <summary>
            ///     End of track + 1
            /// </summary>
            public uint EndOfTrack;
        }

        struct NeroV1Dao
        {
            /// <summary>
            ///     "DAOI"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            ///     Chunk size (big endian)
            /// </summary>
            public uint ChunkSizeBe;

            /// <summary>
            ///     Chunk size (little endian)
            /// </summary>
            public uint ChunkSizeLe;

            /// <summary>
            ///     UPC (14 bytes, null-padded)
            /// </summary>
            public byte[] Upc;

            /// <summary>
            ///     TOC type
            /// </summary>
            public ushort TocType;

            /// <summary>
            ///     First track
            /// </summary>
            public byte FirstTrack;

            /// <summary>
            ///     Last track
            /// </summary>
            public byte LastTrack;

            /// <summary>
            ///     Tracks
            /// </summary>
            public List<NeroV1DaoEntry> Tracks;
        }

        struct NeroV2DaoEntry
        {
            /// <summary>
            ///     ISRC (12 bytes)
            /// </summary>
            public byte[] Isrc;

            /// <summary>
            ///     Size of sector inside image (in bytes)
            /// </summary>
            public ushort SectorSize;

            /// <summary>
            ///     Sector mode in image
            /// </summary>
            public ushort Mode;

            /// <summary>
            ///     Seems to be always 0.
            /// </summary>
            public ushort Unknown;

            /// <summary>
            ///     Index 0 start
            /// </summary>
            public ulong Index0;

            /// <summary>
            ///     Index 1 start
            /// </summary>
            public ulong Index1;

            /// <summary>
            ///     End of track + 1
            /// </summary>
            public ulong EndOfTrack;
        }

        struct NeroV2Dao
        {
            /// <summary>
            ///     "DAOX"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            ///     Chunk size (big endian)
            /// </summary>
            public uint ChunkSizeBe;

            /// <summary>
            ///     Chunk size (little endian)
            /// </summary>
            public uint ChunkSizeLe;

            /// <summary>
            ///     UPC (14 bytes, null-padded)
            /// </summary>
            public byte[] Upc;

            /// <summary>
            ///     TOC type
            /// </summary>
            public ushort TocType;

            /// <summary>
            ///     First track
            /// </summary>
            public byte FirstTrack;

            /// <summary>
            ///     Last track
            /// </summary>
            public byte LastTrack;

            /// <summary>
            ///     Tracks
            /// </summary>
            public List<NeroV2DaoEntry> Tracks;
        }

        struct NeroCdTextPack
        {
            /// <summary>
            ///     Pack type
            /// </summary>
            public byte PackType;

            /// <summary>
            ///     Track number
            /// </summary>
            public byte TrackNumber;

            /// <summary>
            ///     Pack number in block
            /// </summary>
            public byte PackNumber;

            /// <summary>
            ///     Block number
            /// </summary>
            public byte BlockNumber;

            /// <summary>
            ///     12 bytes of data
            /// </summary>
            public byte[] Text;

            /// <summary>
            ///     CRC
            /// </summary>
            public ushort Crc;
        }

        struct NeroCdText
        {
            /// <summary>
            ///     "CDTX"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            ///     Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            ///     CD-TEXT packs
            /// </summary>
            public List<NeroCdTextPack> Packs;
        }

        struct NeroV1TaoEntry
        {
            /// <summary>
            ///     Offset of track on image
            /// </summary>
            public uint Offset;

            /// <summary>
            ///     Length of track in bytes
            /// </summary>
            public uint Length;

            /// <summary>
            ///     Track mode
            /// </summary>
            public uint Mode;

            /// <summary>
            ///     LBA track start (plus 150 lead in sectors)
            /// </summary>
            public uint StartLba;

            /// <summary>
            ///     Unknown
            /// </summary>
            public uint Unknown;
        }

        struct NeroV1Tao
        {
            /// <summary>
            ///     "ETNF"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            ///     Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            ///     CD-TEXT packs
            /// </summary>
            public List<NeroV1TaoEntry> Tracks;
        }

        struct NeroV2TaoEntry
        {
            /// <summary>
            ///     Offset of track on image
            /// </summary>
            public ulong Offset;

            /// <summary>
            ///     Length of track in bytes
            /// </summary>
            public ulong Length;

            /// <summary>
            ///     Track mode
            /// </summary>
            public uint Mode;

            /// <summary>
            ///     LBA track start (plus 150 lead in sectors)
            /// </summary>
            public uint StartLba;

            /// <summary>
            ///     Unknown
            /// </summary>
            public uint Unknown;

            /// <summary>
            ///     Track length in sectors
            /// </summary>
            public uint Sectors;
        }

        struct NeroV2Tao
        {
            /// <summary>
            ///     "ETN2"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            ///     Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            ///     CD-TEXT packs
            /// </summary>
            public List<NeroV2TaoEntry> Tracks;
        }

        struct NeroSession
        {
            /// <summary>
            ///     "SINF"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            ///     Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            ///     Tracks in session
            /// </summary>
            public uint Tracks;
        }

        struct NeroMediaType
        {
            /// <summary>
            ///     "MTYP"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            ///     Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            ///     Media type
            /// </summary>
            public uint Type;
        }

        struct NeroDiscInformation
        {
            /// <summary>
            ///     "DINF"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            ///     Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            ///     Unknown
            /// </summary>
            public uint Unknown;
        }

        struct NeroTocChunk
        {
            /// <summary>
            ///     "TOCT"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            ///     Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            ///     Unknown
            /// </summary>
            public ushort Unknown;
        }

        struct NeroReloChunk
        {
            /// <summary>
            ///     "RELO"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            ///     Chunk size
            /// </summary>
            public uint ChunkSize;

            /// <summary>
            ///     Unknown
            /// </summary>
            public uint Unknown;
        }

        struct NeroEndOfChunkChain
        {
            /// <summary>
            ///     "END!"
            /// </summary>
            public uint ChunkId;

            /// <summary>
            ///     Chunk size
            /// </summary>
            public uint ChunkSize;
        }

        // Internal use only
        struct NeroTrack
        {
            public byte[] Isrc;
            public ushort SectorSize;
            public ulong  Offset;
            public ulong  Length;
            public ulong  EndOfTrack;
            public uint   Mode;
            public ulong  StartLba;
            public ulong  Sectors;
            public ulong  Index0;
            public ulong  Index1;
            public uint   Sequence;
        }
    }
}