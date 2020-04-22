// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AaruFormat.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Aaru Format disk images.
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

/*
 The idea of the format is being able to easily store, retrieve, and access any data that can be read from media.

 At the start of a file there's a header that contains a format version, application creator name, and a pointer to
 the index.

 The index points to one or several DeDuplication Tables, or media tag blocks.

 A deduplication table is a table of offsets to blocks and sectors inside blocks. Each entry equals to an LBA and points
 to a byte offset in the file shift left to the number of sectors contained in a block, plus the number of sector inside
 the block.
 Each block must contain sectors of equal size, but that size can be different between blocks.
 The deduplication table should be stored decompressed if its size is too big to be stored on-memory. This is chosen at
 creation time but it is a good idea to set the limit to 256MiB (this allows for a device of 33 million sectors,
 17Gb at 512 bps, 68Gb at 2048 bps and 137Gb at 4096 bps).

 Sector tags that are simply too small to be deduplicated are contained in a single block pointed by the index (e.g.
 Apple GCR sector tags).

 Optical disks contain a track block that describes the tracks.
 TODO: Streaming tapes contain a file block that describes the files and an optional partition block that describes the tape
 partitions.

 There are also blocks for image metadata, contents metadata and dump hardware information.

 A differencing image will have all the metadata and deduplication tables, but the entries in these ones will be set to
 0 if the block is stored in the parent image. TODO: This is not yet implemented.

 Also because the file becomes useless without the index and deduplication table, each can be stored twice. In case of
 the index it should just be searched for. In case of deduplication tables, both copies should be indexed.

 Finally, writing new data to an existing image is just Copy-On-Write. Create a new block with the modified data, change
 the pointer in the corresponding deduplication table.

 P.S.: Data Position Measurement is doable, as soon as I know how to do it.
 P.S.2: Support for floppy image containg bitslices and/or fluxes will be added soon.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Aaru.Checksums;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using CUETools.Codecs;
using CUETools.Codecs.Flake;
using SharpCompress.Compressors.LZMA;

namespace Aaru.DiscImages
{
    public partial class AaruFormat : IWritableOpticalImage, IVerifiableImage, IWritableTapeImage
    {
        bool alreadyWrittenZero;
        /// <summary>Cache of uncompressed blocks.</summary>
        Dictionary<ulong, byte[]> blockCache;
        /// <summary>Cache of block headers.</summary>
        Dictionary<ulong, BlockHeader> blockHeaderCache;
        /// <summary>Stream used for writing blocks.</summary>
        MemoryStream blockStream;
        /// <summary>Provides checksum for deduplication of sectors.</summary>
        SHA256 checksumProvider;
        bool compress;
        /// <summary>Provides CRC64.</summary>
        Crc64Context crc64;
        /// <summary>Header of the currently writing block.</summary>
        BlockHeader currentBlockHeader;
        /// <summary>Sector offset of writing position in currently writing block.</summary>
        uint currentBlockOffset;
        /// <summary>Current size in bytes of the block cache</summary>
        uint currentCacheSize;
        /// <summary>Cache of DDT entries.</summary>
        Dictionary<ulong, ulong> ddtEntryCache;
        MemoryStream decompressedStream;
        bool         deduplicate;
        /// <summary>On-memory deduplication table indexed by checksum.</summary>
        Dictionary<string, ulong> deduplicationTable;
        /// <summary><see cref="CUETools.Codecs.FLAKE" /> writer.</summary>
        AudioEncoder flakeWriter;
        /// <summary><see cref="CUETools.Codecs.FLAKE" /> settings.</summary>
        EncoderSettings flakeWriterSettings;
        /// <summary>Block with logical geometry.</summary>
        GeometryBlock geometryBlock;
        /// <summary>Image header.</summary>
        AaruHeader header;
        /// <summary>Image information.</summary>
        ImageInfo imageInfo;
        /// <summary>Image data stream.</summary>
        Stream imageStream;
        /// <summary>Index.</summary>
        List<IndexEntry> index;
        /// <summary>If set to <c>true</c>, the DDT entries are in-memory.</summary>
        bool inMemoryDdt;
        ulong lastWrittenBlock;
        /// <summary>LZMA stream.</summary>
        LzmaStream lzmaBlockStream;
        /// <summary>LZMA properties.</summary>
        LzmaEncoderProperties lzmaEncoderProperties;
        Md5Context md5Provider;
        /// <summary>Cache of media tags.</summary>
        Dictionary<MediaTagType, byte[]> mediaTags;
        byte[] mode2Subheaders;
        /// <summary>If DDT is on-disk, this is the image stream offset at which it starts.</summary>
        long outMemoryDdtPosition;
        bool rewinded;
        /// <summary>Cache for data that prefixes the user data on a sector (e.g. sync).</summary>
        byte[] sectorPrefix;
        uint[]       sectorPrefixDdt;
        MemoryStream sectorPrefixMs;
        /// <summary>Cache for data that goes side by side with user data (e.g. CompactDisc subchannel).</summary>
        byte[] sectorSubchannel;
        /// <summary>Cache for data that suffixes the user data on a sector (e.g. edc, ecc).</summary>
        byte[] sectorSuffix;
        uint[]        sectorSuffixDdt;
        MemoryStream  sectorSuffixMs;
        Sha1Context   sha1Provider;
        Sha256Context sha256Provider;
        /// <summary>Shift for calculating number of sectors in a block.</summary>
        byte shift;
        SpamSumContext spamsumProvider;
        /// <summary>Cache for bytes to write/rad on-disk.</summary>
        byte[] structureBytes;
        /// <summary>Cache for pointer for marshaling structures.</summary>
        IntPtr structurePointer;
        Dictionary<ulong, ulong> tapeDdt;
        /// <summary>Cache of CompactDisc track's flags</summary>
        Dictionary<byte, byte> trackFlags;
        /// <summary>Cache of CompactDisc track's ISRC</summary>
        Dictionary<byte, string> trackIsrcs;
        /// <summary>In-memory deduplication table</summary>
        ulong[] userDataDdt;
        bool  writingLong;
        ulong writtenSectors;

        public AaruFormat() => imageInfo = new ImageInfo
        {
            ReadableSectorTags = new List<SectorTagType>(), ReadableMediaTags = new List<MediaTagType>(),
            HasPartitions      = false, HasSessions                           = false, Version = null,
            Application        = "Aaru",
            ApplicationVersion = null, Creator = null, Comments = null,
            MediaManufacturer  = null,
            MediaModel         = null, MediaSerialNumber = null, MediaBarcode = null,
            MediaPartNumber    = null,
            MediaSequence      = 0, LastMediaSequence = 0, DriveManufacturer = null,
            DriveModel         = null,
            DriveSerialNumber  = null, DriveFirmwareRevision = null
        };
    }
}