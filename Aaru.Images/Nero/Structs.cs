// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/



// ReSharper disable NotAccessedField.Local

namespace Aaru.DiscImages;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

[SuppressMessage("ReSharper", "UnusedType.Local")]
public sealed partial class Nero
{
    struct FooterV1
    {
        /// <summary>"NERO"</summary>
        public uint ChunkId;

        /// <summary>Offset of first chunk in file</summary>
        public uint FirstChunkOffset;
    }

    struct FooterV2
    {
        /// <summary>"NER5"</summary>
        public uint ChunkId;

        /// <summary>Offset of first chunk in file</summary>
        public ulong FirstChunkOffset;
    }

    struct CueEntryV2
    {
        /// <summary>Track mode. 0x01 for audio, 0x21 for copy-protected audio, 0x41 for data</summary>
        public byte Mode;

        /// <summary>Track number in BCD</summary>
        public byte TrackNumber;

        /// <summary>Index number in BCD</summary>
        public byte IndexNumber;

        /// <summary>Always zero</summary>
        public byte Dummy;

        /// <summary>LBA sector start for this entry</summary>
        public int LbaStart;
    }

    class CuesheetV2
    {
        /// <summary>"CUEX"</summary>
        public uint ChunkId;

        /// <summary>Chunk size</summary>
        public uint ChunkSize;

        /// <summary>Cuesheet entries</summary>
        public List<CueEntryV2> Entries;
    }

    struct CueEntryV1
    {
        /// <summary>Track mode. 0x01 for audio, 0x21 for copy-protected audio, 0x41 for data</summary>
        public byte Mode;

        /// <summary>Track number in BCD</summary>
        public byte TrackNumber;

        /// <summary>Index number in BCD</summary>
        public byte IndexNumber;

        /// <summary>Always zero</summary>
        public ushort Dummy;

        /// <summary>MSF start sector's minute for this entry</summary>
        public byte Minute;

        /// <summary>MSF start sector's second for this entry</summary>
        public byte Second;

        /// <summary>MSF start sector's frame for this entry</summary>
        public byte Frame;
    }

    class CuesheetV1
    {
        /// <summary>"CUES"</summary>
        public uint ChunkId;

        /// <summary>Chunk size</summary>
        public uint ChunkSize;

        /// <summary>Cuesheet entries</summary>
        public List<CueEntryV1> Entries;
    }

    struct DaoEntryV1
    {
        /// <summary>ISRC (12 bytes)</summary>
        public byte[] Isrc;

        /// <summary>Size of sector inside image (in bytes)</summary>
        public ushort SectorSize;

        /// <summary>Sector mode in image</summary>
        public ushort Mode;

        /// <summary>Unknown</summary>
        public ushort Unknown;

        /// <summary>Index 0 start</summary>
        public uint Index0;

        /// <summary>Index 1 start</summary>
        public uint Index1;

        /// <summary>End of track + 1</summary>
        public uint EndOfTrack;
    }

    struct DaoV1
    {
        /// <summary>"DAOI"</summary>
        public uint ChunkId;

        /// <summary>Chunk size (big endian)</summary>
        public uint ChunkSizeBe;

        /// <summary>Chunk size (little endian)</summary>
        public uint ChunkSizeLe;

        /// <summary>UPC (14 bytes, null-padded)</summary>
        public byte[] Upc;

        /// <summary>TOC type</summary>
        public ushort TocType;

        /// <summary>First track</summary>
        public byte FirstTrack;

        /// <summary>Last track</summary>
        public byte LastTrack;

        /// <summary>Tracks</summary>
        public List<DaoEntryV1> Tracks;
    }

    struct DaoEntryV2
    {
        /// <summary>ISRC (12 bytes)</summary>
        public byte[] Isrc;

        /// <summary>Size of sector inside image (in bytes)</summary>
        public ushort SectorSize;

        /// <summary>Sector mode in image</summary>
        public ushort Mode;

        /// <summary>Seems to be always 0.</summary>
        public ushort Unknown;

        /// <summary>Index 0 start</summary>
        public ulong Index0;

        /// <summary>Index 1 start</summary>
        public ulong Index1;

        /// <summary>End of track + 1</summary>
        public ulong EndOfTrack;
    }

    struct DaoV2
    {
        /// <summary>"DAOX"</summary>
        public uint ChunkId;

        /// <summary>Chunk size (big endian)</summary>
        public uint ChunkSizeBe;

        /// <summary>Chunk size (little endian)</summary>
        public uint ChunkSizeLe;

        /// <summary>UPC (14 bytes, null-padded)</summary>
        public byte[] Upc;

        /// <summary>TOC type</summary>
        public ushort TocType;

        /// <summary>First track</summary>
        public byte FirstTrack;

        /// <summary>Last track</summary>
        public byte LastTrack;

        /// <summary>Tracks</summary>
        public List<DaoEntryV2> Tracks;
    }

    struct CdTextPack
    {
        /// <summary>Pack type</summary>
        public byte PackType;

        /// <summary>Track number</summary>
        public byte TrackNumber;

        /// <summary>Pack number in block</summary>
        public byte PackNumber;

        /// <summary>Block number</summary>
        public byte BlockNumber;

        /// <summary>12 bytes of data</summary>
        public byte[] Text;

        /// <summary>CRC</summary>
        public ushort Crc;
    }

    struct CdText
    {
        /// <summary>"CDTX"</summary>
        public uint ChunkId;

        /// <summary>Chunk size</summary>
        public uint ChunkSize;

        /// <summary>CD-TEXT packs</summary>
        public List<CdTextPack> Packs;
    }

    struct TaoEntryV0
    {
        /// <summary>Offset of track on image</summary>
        public uint Offset;

        /// <summary>Length of track in bytes</summary>
        public uint Length;

        /// <summary>Track mode</summary>
        public uint Mode;
    }

    struct TaoV0
    {
        /// <summary>"TINF"</summary>
        public uint ChunkId;

        /// <summary>Chunk size</summary>
        public uint ChunkSize;

        /// <summary>CD-TEXT packs</summary>
        public List<TaoEntryV0> Tracks;
    }

    struct TaoEntryV1
    {
        /// <summary>Offset of track on image</summary>
        public uint Offset;

        /// <summary>Length of track in bytes</summary>
        public uint Length;

        /// <summary>Track mode</summary>
        public uint Mode;

        /// <summary>LBA track start (plus 150 lead in sectors)</summary>
        public uint StartLba;

        /// <summary>Unknown</summary>
        public uint Unknown;
    }

    struct TaoV1
    {
        /// <summary>"ETNF"</summary>
        public uint ChunkId;

        /// <summary>Chunk size</summary>
        public uint ChunkSize;

        /// <summary>CD-TEXT packs</summary>
        public List<TaoEntryV1> Tracks;
    }

    struct TaoEntryV2
    {
        /// <summary>Offset of track on image</summary>
        public ulong Offset;

        /// <summary>Length of track in bytes</summary>
        public ulong Length;

        /// <summary>Track mode</summary>
        public uint Mode;

        /// <summary>LBA track start (plus 150 lead in sectors)</summary>
        public uint StartLba;

        /// <summary>Unknown</summary>
        public uint Unknown;

        /// <summary>Track length in sectors</summary>
        public uint Sectors;
    }

    struct TaoV2
    {
        /// <summary>"ETN2"</summary>
        public uint ChunkId;

        /// <summary>Chunk size</summary>
        public uint ChunkSize;

        /// <summary>CD-TEXT packs</summary>
        public List<TaoEntryV2> Tracks;
    }

    struct Session
    {
        /// <summary>"SINF"</summary>
        public uint ChunkId;

        /// <summary>Chunk size</summary>
        public uint ChunkSize;

        /// <summary>Tracks in session</summary>
        public uint Tracks;
    }

    struct MediaType
    {
        /// <summary>"MTYP"</summary>
        public uint ChunkId;

        /// <summary>Chunk size</summary>
        public uint ChunkSize;

        /// <summary>Media type</summary>
        public uint Type;
    }

    struct DiscInformation
    {
        /// <summary>"DINF"</summary>
        public uint ChunkId;

        /// <summary>Chunk size</summary>
        public uint ChunkSize;

        /// <summary>Unknown</summary>
        public uint Unknown;
    }

    struct TocChunk
    {
        /// <summary>"TOCT"</summary>
        public uint ChunkId;

        /// <summary>Chunk size</summary>
        public uint ChunkSize;

        /// <summary>Unknown</summary>
        public ushort Unknown;
    }

    struct ReloChunk
    {
        /// <summary>"RELO"</summary>
        public uint ChunkId;

        /// <summary>Chunk size</summary>
        public uint ChunkSize;

        /// <summary>Unknown</summary>
        public uint Unknown;
    }

    struct EndOfChunkChain
    {
        /// <summary>"END!"</summary>
        public uint ChunkId;

        /// <summary>Chunk size</summary>
        public uint ChunkSize;
    }

    // Internal use only
    class NeroTrack
    {
        public ulong  EndOfTrack;
        public ulong  Index0;
        public ulong  Index1;
        public byte[] Isrc;
        public ulong  Length;
        public uint   Mode;
        public ulong  Offset;
        public ulong  Sectors;
        public ushort SectorSize;
        public uint   Sequence;
        public ulong  StartLba;
        public bool   UseLbaForIndex;
    }
}