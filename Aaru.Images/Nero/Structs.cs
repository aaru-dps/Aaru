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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

// ReSharper disable NotAccessedField.Local

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Aaru.DiscImages;

[SuppressMessage("ReSharper", "UnusedType.Local")]
public sealed partial class Nero
{
#region Nested type: CdText

    struct CdText
    {
        /// <summary>"CDTX"</summary>
        public uint ChunkId;

        /// <summary>Chunk size</summary>
        public uint ChunkSize;

        /// <summary>CD-TEXT packs</summary>
        public List<CdTextPack> Packs;
    }

#endregion

#region Nested type: CdTextPack

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

#endregion

#region Nested type: CueEntryV1

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

#endregion

#region Nested type: CueEntryV2

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

#endregion

#region Nested type: CuesheetV1

    sealed class CuesheetV1
    {
        /// <summary>"CUES"</summary>
        public uint ChunkId;

        /// <summary>Chunk size</summary>
        public uint ChunkSize;

        /// <summary>Cuesheet entries</summary>
        public List<CueEntryV1> Entries;
    }

#endregion

#region Nested type: CuesheetV2

    sealed class CuesheetV2
    {
        /// <summary>"CUEX"</summary>
        public uint ChunkId;

        /// <summary>Chunk size</summary>
        public uint ChunkSize;

        /// <summary>Cuesheet entries</summary>
        public List<CueEntryV2> Entries;
    }

#endregion

#region Nested type: DaoEntryV1

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

#endregion

#region Nested type: DaoEntryV2

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

#endregion

#region Nested type: DaoV1

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

#endregion

#region Nested type: DaoV2

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

#endregion

#region Nested type: DiscInformation

    struct DiscInformation
    {
        /// <summary>"DINF"</summary>
        public uint ChunkId;

        /// <summary>Chunk size</summary>
        public uint ChunkSize;

        /// <summary>Unknown</summary>
        public uint Unknown;
    }

#endregion

#region Nested type: EndOfChunkChain

    struct EndOfChunkChain
    {
        /// <summary>"END!"</summary>
        public uint ChunkId;

        /// <summary>Chunk size</summary>
        public uint ChunkSize;
    }

#endregion

#region Nested type: FooterV1

    struct FooterV1
    {
        /// <summary>"NERO"</summary>
        public uint ChunkId;

        /// <summary>Offset of first chunk in file</summary>
        public uint FirstChunkOffset;
    }

#endregion

#region Nested type: FooterV2

    struct FooterV2
    {
        /// <summary>"NER5"</summary>
        public uint ChunkId;

        /// <summary>Offset of first chunk in file</summary>
        public ulong FirstChunkOffset;
    }

#endregion

#region Nested type: MediaType

    struct MediaType
    {
        /// <summary>"MTYP"</summary>
        public uint ChunkId;

        /// <summary>Chunk size</summary>
        public uint ChunkSize;

        /// <summary>Media type</summary>
        public uint Type;
    }

#endregion

#region Nested type: NeroTrack

    // Internal use only
    sealed class NeroTrack
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

#endregion

#region Nested type: ReloChunk

    struct ReloChunk
    {
        /// <summary>"RELO"</summary>
        public uint ChunkId;

        /// <summary>Chunk size</summary>
        public uint ChunkSize;

        /// <summary>Unknown</summary>
        public uint Unknown;
    }

#endregion

#region Nested type: Session

    struct Session
    {
        /// <summary>"SINF"</summary>
        public uint ChunkId;

        /// <summary>Chunk size</summary>
        public uint ChunkSize;

        /// <summary>Tracks in session</summary>
        public uint Tracks;
    }

#endregion

#region Nested type: TaoEntryV0

    struct TaoEntryV0
    {
        /// <summary>Offset of track on image</summary>
        public uint Offset;

        /// <summary>Length of track in bytes</summary>
        public uint Length;

        /// <summary>Track mode</summary>
        public uint Mode;
    }

#endregion

#region Nested type: TaoEntryV1

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

#endregion

#region Nested type: TaoEntryV2

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

#endregion

#region Nested type: TaoV0

    struct TaoV0
    {
        /// <summary>"TINF"</summary>
        public uint ChunkId;

        /// <summary>Chunk size</summary>
        public uint ChunkSize;

        /// <summary>CD-TEXT packs</summary>
        public List<TaoEntryV0> Tracks;
    }

#endregion

#region Nested type: TaoV1

    struct TaoV1
    {
        /// <summary>"ETNF"</summary>
        public uint ChunkId;

        /// <summary>Chunk size</summary>
        public uint ChunkSize;

        /// <summary>CD-TEXT packs</summary>
        public List<TaoEntryV1> Tracks;
    }

#endregion

#region Nested type: TaoV2

    struct TaoV2
    {
        /// <summary>"ETN2"</summary>
        public uint ChunkId;

        /// <summary>Chunk size</summary>
        public uint ChunkSize;

        /// <summary>CD-TEXT packs</summary>
        public List<TaoEntryV2> Tracks;
    }

#endregion

#region Nested type: TocChunk

    struct TocChunk
    {
        /// <summary>"TOCT"</summary>
        public uint ChunkId;

        /// <summary>Chunk size</summary>
        public uint ChunkSize;

        /// <summary>Unknown</summary>
        public ushort Unknown;
    }

#endregion
}