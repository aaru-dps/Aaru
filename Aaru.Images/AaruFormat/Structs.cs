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
//     Contains structures for Aaru Format disk images.
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

namespace Aaru.DiscImages;

using System.Runtime.InteropServices;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;

public sealed partial class AaruFormat
{
    /// <summary>Header, at start of file</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    struct AaruHeader
    {
        /// <summary>Header identifier, <see cref="AARU_MAGIC" /></summary>
        public ulong identifier;
        /// <summary>UTF-16LE name of the application that created the image</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string application;
        /// <summary>Image format major version. A new major version means a possibly incompatible change of format</summary>
        public byte imageMajorVersion;
        /// <summary>Image format minor version. A new minor version indicates a compatible change of format</summary>
        public byte imageMinorVersion;
        /// <summary>Major version of the application that created the image</summary>
        public byte applicationMajorVersion;
        /// <summary>Minor version of the application that created the image</summary>
        public byte applicationMinorVersion;
        /// <summary>Type of media contained on image</summary>
        public MediaType mediaType;
        /// <summary>Offset to index</summary>
        public ulong indexOffset;
        /// <summary>Windows filetime (100 nanoseconds since 1601/01/01 00:00:00 UTC) of image creation time</summary>
        public long creationTime;
        /// <summary>Windows filetime (100 nanoseconds since 1601/01/01 00:00:00 UTC) of image last written time</summary>
        public long lastWrittenTime;
    }

    /// <summary>Header for a deduplication table. Table follows it</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct DdtHeader
    {
        /// <summary>Identifier, <see cref="BlockType.DeDuplicationTable" /></summary>
        public BlockType identifier;
        /// <summary>Type of data pointed by this DDT</summary>
        public DataType type;
        /// <summary>Compression algorithm used to compress the DDT</summary>
        public CompressionType compression;
        /// <summary>Each entry is ((byte offset in file) &lt;&lt; shift) + (sector offset in block)</summary>
        public byte shift;
        /// <summary>How many entries are in the table</summary>
        public ulong entries;
        /// <summary>Compressed length for the DDT</summary>
        public ulong cmpLength;
        /// <summary>Uncompressed length for the DDT</summary>
        public ulong length;
        /// <summary>CRC64-ECMA of the compressed DDT</summary>
        public ulong cmpCrc64;
        /// <summary>CRC64-ECMA of the uncompressed DDT</summary>
        public readonly ulong crc64;
    }

    /// <summary>Header for the index, followed by entries</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct IndexHeader
    {
        /// <summary>Identifier, <see cref="BlockType.Index" /></summary>
        public BlockType identifier;
        /// <summary>How many entries follow this header</summary>
        public ushort entries;
        /// <summary>CRC64-ECMA of the index</summary>
        public ulong crc64;
    }

    /// <summary>Header for the index, followed by entries</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct IndexHeader2
    {
        /// <summary>Identifier, <see cref="BlockType.Index2" /></summary>
        public BlockType identifier;
        /// <summary>How many entries follow this header</summary>
        public ulong entries;
        /// <summary>CRC64-ECMA of the index</summary>
        public ulong crc64;
    }

    /// <summary>Index entry</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct IndexEntry
    {
        /// <summary>Type of item pointed by this entry</summary>
        public BlockType blockType;
        /// <summary>Type of data contained by the block pointed by this entry</summary>
        public DataType dataType;
        /// <summary>Offset in file where item is stored</summary>
        public ulong offset;
    }

    /// <summary>Block header, precedes block data</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BlockHeader
    {
        /// <summary>Identifier, <see cref="BlockType.DataBlock" /></summary>
        public BlockType identifier;
        /// <summary>Type of data contained by this block</summary>
        public DataType type;
        /// <summary>Compression algorithm used to compress the block</summary>
        public CompressionType compression;
        /// <summary>Size in bytes of each sector contained in this block</summary>
        public uint sectorSize;
        /// <summary>Compressed length for the block</summary>
        public uint cmpLength;
        /// <summary>Uncompressed length for the block</summary>
        public uint length;
        /// <summary>CRC64-ECMA of the compressed block</summary>
        public ulong cmpCrc64;
        /// <summary>CRC64-ECMA of the uncompressed block</summary>
        public ulong crc64;
    }

    /// <summary>Geometry block, contains physical geometry information</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct GeometryBlock
    {
        /// <summary>Identifier, <see cref="BlockType.GeometryBlock" /></summary>
        public BlockType identifier;
        public uint cylinders;
        public uint heads;
        public uint sectorsPerTrack;
    }

    /// <summary>Metadata block, contains metadata</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct MetadataBlock
    {
        /// <summary>Identifier, <see cref="BlockType.MetadataBlock" /></summary>
        public BlockType identifier;
        /// <summary>Size in bytes of this whole metadata block</summary>
        public uint blockSize;
        /// <summary>Sequence of media set this media belongs to</summary>
        public int mediaSequence;
        /// <summary>Total number of media on the media set this media belongs to</summary>
        public int lastMediaSequence;
        /// <summary>Offset to start of creator string from start of this block</summary>
        public uint creatorOffset;
        /// <summary>Length in bytes of the null-terminated UTF-16LE creator string</summary>
        public uint creatorLength;
        /// <summary>Offset to start of creator string from start of this block</summary>
        public uint commentsOffset;
        /// <summary>Length in bytes of the null-terminated UTF-16LE creator string</summary>
        public uint commentsLength;
        /// <summary>Offset to start of creator string from start of this block</summary>
        public uint mediaTitleOffset;
        /// <summary>Length in bytes of the null-terminated UTF-16LE creator string</summary>
        public uint mediaTitleLength;
        /// <summary>Offset to start of creator string from start of this block</summary>
        public uint mediaManufacturerOffset;
        /// <summary>Length in bytes of the null-terminated UTF-16LE creator string</summary>
        public uint mediaManufacturerLength;
        /// <summary>Offset to start of creator string from start of this block</summary>
        public uint mediaModelOffset;
        /// <summary>Length in bytes of the null-terminated UTF-16LE creator string</summary>
        public uint mediaModelLength;
        /// <summary>Offset to start of creator string from start of this block</summary>
        public uint mediaSerialNumberOffset;
        /// <summary>Length in bytes of the null-terminated UTF-16LE creator string</summary>
        public uint mediaSerialNumberLength;
        /// <summary>Offset to start of creator string from start of this block</summary>
        public uint mediaBarcodeOffset;
        /// <summary>Length in bytes of the null-terminated UTF-16LE creator string</summary>
        public uint mediaBarcodeLength;
        /// <summary>Offset to start of creator string from start of this block</summary>
        public uint mediaPartNumberOffset;
        /// <summary>Length in bytes of the null-terminated UTF-16LE creator string</summary>
        public uint mediaPartNumberLength;
        /// <summary>Offset to start of creator string from start of this block</summary>
        public uint driveManufacturerOffset;
        /// <summary>Length in bytes of the null-terminated UTF-16LE creator string</summary>
        public uint driveManufacturerLength;
        /// <summary>Offset to start of creator string from start of this block</summary>
        public uint driveModelOffset;
        /// <summary>Length in bytes of the null-terminated UTF-16LE creator string</summary>
        public uint driveModelLength;
        /// <summary>Offset to start of creator string from start of this block</summary>
        public uint driveSerialNumberOffset;
        /// <summary>Length in bytes of the null-terminated UTF-16LE creator string</summary>
        public uint driveSerialNumberLength;
        /// <summary>Offset to start of creator string from start of this block</summary>
        public uint driveFirmwareRevisionOffset;
        /// <summary>Length in bytes of the null-terminated UTF-16LE creator string</summary>
        public uint driveFirmwareRevisionLength;
    }

    /// <summary>Contains list of optical disc tracks</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TracksHeader
    {
        /// <summary>Identifier, <see cref="BlockType.TracksBlock" /></summary>
        public BlockType identifier;
        /// <summary>How many entries follow this header</summary>
        public ushort entries;
        /// <summary>CRC64-ECMA of the block</summary>
        public ulong crc64;
    }

    /// <summary>Optical disc track</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    struct TrackEntry
    {
        /// <summary>Track sequence</summary>
        public byte sequence;
        /// <summary>Track type</summary>
        public TrackType type;
        /// <summary>Track starting LBA</summary>
        public long start;
        /// <summary>Track last LBA</summary>
        public long end;
        /// <summary>Track pregap in sectors</summary>
        public long pregap;
        /// <summary>Track session</summary>
        public byte session;
        /// <summary>Track's ISRC in ASCII</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string isrc;
        /// <summary>Track flags</summary>
        public byte flags;
    }

    /// <summary>Geometry block, contains physical geometry information</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct CicmMetadataBlock
    {
        /// <summary>Identifier, <see cref="BlockType.CicmBlock" /></summary>
        public BlockType identifier;
        public uint length;
    }

    /// <summary>Dump hardware block, contains a list of hardware used to dump the media on this image</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct DumpHardwareHeader
    {
        /// <summary>Identifier, <see cref="BlockType.DumpHardwareBlock" /></summary>
        public BlockType identifier;
        /// <summary>How many entries follow this header</summary>
        public ushort entries;
        /// <summary>Size of the whole block, not including this header, in bytes</summary>
        public uint length;
        /// <summary>CRC64-ECMA of the block</summary>
        public ulong crc64;
    }

    /// <summary>Dump hardware entry, contains length of strings that follow, in the same order as the length, this structure</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct DumpHardwareEntry
    {
        /// <summary>Length of UTF-8 manufacturer string</summary>
        public uint manufacturerLength;
        /// <summary>Length of UTF-8 model string</summary>
        public uint modelLength;
        /// <summary>Length of UTF-8 revision string</summary>
        public uint revisionLength;
        /// <summary>Length of UTF-8 firmware version string</summary>
        public uint firmwareLength;
        /// <summary>Length of UTF-8 serial string</summary>
        public uint serialLength;
        /// <summary>Length of UTF-8 software name string</summary>
        public uint softwareNameLength;
        /// <summary>Length of UTF-8 software version string</summary>
        public uint softwareVersionLength;
        /// <summary>Length of UTF-8 software operating system string</summary>
        public uint softwareOperatingSystemLength;
        /// <summary>How many extents are after the strings</summary>
        public uint extents;
    }

    /// <summary>
    ///     Checksum block, contains a checksum of all user data sectors (except for optical discs that is 2352 bytes raw
    ///     sector if available
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ChecksumHeader
    {
        /// <summary>Identifier, <see cref="BlockType.ChecksumBlock" /></summary>
        public BlockType identifier;
        /// <summary>Length in bytes of the block</summary>
        public uint length;
        /// <summary>How many checksums follow</summary>
        public byte entries;
    }

    /// <summary>Checksum entry, followed by checksum data itself</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ChecksumEntry
    {
        /// <summary>Checksum algorithm</summary>
        public ChecksumAlgorithm type;
        /// <summary>Length in bytes of checksum that follows this structure</summary>
        public uint length;
    }

    /// <summary>Tape file block, contains a list of all files in a tape</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TapeFileHeader
    {
        /// <summary>Identifier, <see cref="BlockType.TapeFileBlock" /></summary>
        public BlockType identifier;
        /// <summary>How many entries follow this header</summary>
        public uint entries;
        /// <summary>Size of the whole block, not including this header, in bytes</summary>
        public ulong length;
        /// <summary>CRC64-ECMA of the block</summary>
        public ulong crc64;
    }

    /// <summary>Tape file entry</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TapeFileEntry
    {
        /// <summary>File number</summary>
        public uint File;
        /// <summary>Partition number</summary>
        public readonly byte Partition;
        /// <summary>First block, inclusive, of the file</summary>
        public ulong FirstBlock;
        /// <summary>Last block, inclusive, of the file</summary>
        public ulong LastBlock;
    }

    /// <summary>Tape partition block, contains a list of all partitions in a tape</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TapePartitionHeader
    {
        /// <summary>Identifier, <see cref="BlockType.TapePartitionBlock" /></summary>
        public BlockType identifier;
        /// <summary>How many entries follow this header</summary>
        public byte entries;
        /// <summary>Size of the whole block, not including this header, in bytes</summary>
        public ulong length;
        /// <summary>CRC64-ECMA of the block</summary>
        public ulong crc64;
    }

    /// <summary>Tape partition entry</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TapePartitionEntry
    {
        /// <summary>Partition number</summary>
        public byte Number;
        /// <summary>First block, inclusive, of the partition</summary>
        public ulong FirstBlock;
        /// <summary>Last block, inclusive, of the partition</summary>
        public ulong LastBlock;
    }

    /// <summary>
    ///     Compact Disc track indexes block, contains a cache of all Compact Disc indexes to not need to interpret
    ///     subchannel
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct CompactDiscIndexesHeader
    {
        /// <summary>Identifier, <see cref="BlockType.CompactDiscIndexesBlock" /></summary>
        public BlockType identifier;
        /// <summary>How many entries follow this header</summary>
        public ushort entries;
        /// <summary>Size of the whole block, not including this header, in bytes</summary>
        public readonly ulong length;
        /// <summary>CRC64-ECMA of the block</summary>
        public ulong crc64;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct CompactDiscIndexEntry
    {
        /// <summary>How many entries follow this header</summary>
        public ushort Track;
        /// <summary>Size of the whole block, not including this header, in bytes</summary>
        public ushort Index;
        /// <summary>CRC64-ECMA of the block</summary>
        public int Lba;
    }
}