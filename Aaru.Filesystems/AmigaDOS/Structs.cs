// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Amiga Fast File System plugin.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;
using Aaru.CommonTypes.Attributes;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class AmigaDOSPlugin
{
#region Nested type: BootBlock

    /// <summary>Boot block, first 2 sectors</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct BootBlock
    {
        /// <summary>Offset 0x00, "DOSx" disk type</summary>
        public uint diskType;
        /// <summary>Offset 0x04, Checksum</summary>
        public uint checksum;
        /// <summary>Offset 0x08, Pointer to root block, mostly invalid</summary>
        public uint root_ptr;
        /// <summary>Offset 0x0C, Boot code, til completion. Size is intentionally incorrect to allow marshaling to work.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public byte[] bootCode;
    }

#endregion

#region Nested type: RootBlock

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct RootBlock
    {
        /// <summary>Offset 0x00, block type, value = T_HEADER (2)</summary>
        public uint type;
        /// <summary>Offset 0x04, unused</summary>
        public uint headerKey;
        /// <summary>Offset 0x08, unused</summary>
        public uint highSeq;
        /// <summary>Offset 0x0C, longs used by hash table</summary>
        public uint hashTableSize;
        /// <summary>Offset 0x10, unused</summary>
        public uint firstData;
        /// <summary>Offset 0x14, Rootblock checksum</summary>
        public uint checksum;
        /// <summary>
        ///     Offset 0x18, Hashtable, size = (block size / 4) - 56 or size = hashTableSize. Size intentionally bad to allow
        ///     marshalling to work.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public uint[] hashTable;
        /// <summary>Offset 0x18+hashTableSize*4+0, bitmap flag, 0xFFFFFFFF if valid</summary>
        public uint bitmapFlag;
        /// <summary>Offset 0x18+hashTableSize*4+4, bitmap pages, 25 entries</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 25)]
        public uint[] bitmapPages;
        /// <summary>Offset 0x18+hashTableSize*4+104, pointer to bitmap extension block</summary>
        public uint bitmapExtensionBlock;
        /// <summary>Offset 0x18+hashTableSize*4+108, last root alteration days since 1978/01/01</summary>
        public uint rDays;
        /// <summary>Offset 0x18+hashTableSize*4+112, last root alteration minutes past midnight</summary>
        public uint rMins;
        /// <summary>Offset 0x18+hashTableSize*4+116, last root alteration ticks (1/50 secs)</summary>
        public uint rTicks;
        /// <summary>Offset 0x18+hashTableSize*4+120, disk name, pascal string, 31 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 31)]
        public byte[] diskName;
        /// <summary>Offset 0x18+hashTableSize*4+151, unused</summary>
        public byte padding;
        /// <summary>Offset 0x18+hashTableSize*4+152, unused</summary>
        public uint reserved1;
        /// <summary>
        ///     Offset 0x18+hashTableSize*4+156, blocks allocated in the bitmap (DOS\6 and DOS\7 only, valid when the
        ///     bitmap flag is set and the value is not zero)
        /// </summary>
        public uint numBlocksUsed;
        /// <summary>Offset 0x18+hashTableSize*4+160, last disk alteration days since 1978/01/01</summary>
        public uint vDays;
        /// <summary>Offset 0x18+hashTableSize*4+164, last disk alteration minutes past midnight</summary>
        public uint vMins;
        /// <summary>Offset 0x18+hashTableSize*4+168, last disk alteration ticks (1/50 secs)</summary>
        public uint vTicks;
        /// <summary>Offset 0x18+hashTableSize*4+172, filesystem creation days since 1978/01/01</summary>
        public uint cDays;
        /// <summary>Offset 0x18+hashTableSize*4+176, filesystem creation minutes since 1978/01/01</summary>
        public uint cMins;
        /// <summary>Offset 0x18+hashTableSize*4+180, filesystem creation ticks since 1978/01/01</summary>
        public uint cTicks;
        /// <summary>Offset 0x18+hashTableSize*4+184, copy of the DOS type (DOS\6 and DOS\7 only)</summary>
        public uint fileSystemType;
        /// <summary>Offset 0x18+hashTableSize*4+188, unused</summary>
        public uint parentDir;
        /// <summary>Offset 0x18+hashTableSize*4+192, first directory cache block</summary>
        public uint extension;
        /// <summary>Offset 0x18+hashTableSize*4+196, block secondary type = ST_ROOT (1)</summary>
        public uint sec_type;
    }

#endregion

#region Nested type: FileHeaderBlock

    /// <summary>File header block structure</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct FileHeaderBlock
    {
        /// <summary>Offset 0x00, block type = T_HEADER (2)</summary>
        public uint type;
        /// <summary>Offset 0x04, self pointer (current block number)</summary>
        public uint headerKey;
        /// <summary>Offset 0x08, number of data blocks in this header block</summary>
        public uint highSeq;
        /// <summary>Offset 0x0C, unused (== 0)</summary>
        public uint dataSize;
        /// <summary>Offset 0x10, first data block</summary>
        public uint firstData;
        /// <summary>Offset 0x14, checksum</summary>
        public uint checksum;
        /// <summary>
        ///     Offset 0x18, data block pointers, max 72 entries for 512-byte blocks.
        ///     Size intentionally incorrect to allow marshaling.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public uint[] dataBlocks;

        // Following fields are at end of block (variable offset based on block size)
        // Access bits: bit0=delete, bit1=modify, bit2=write, bit3=read
        // uint access;
        // uint byteSize;
        // byte commLen;
        // byte[79] comment;
        // uint days, mins, ticks;
        // byte nameLen;
        // byte[30] fileName;
        // uint real; // unused
        // uint nextLink; // link chain
        // uint nextSameHash;
        // uint parent;
        // uint extension; // pointer to extension block
        // uint secType; // == ST_FILE (-3)
    }

#endregion

#region Nested type: FileExtensionBlock

    /// <summary>File header extension block structure</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct FileExtensionBlock
    {
        /// <summary>Offset 0x00, block type = T_LIST (16)</summary>
        public uint type;
        /// <summary>Offset 0x04, self pointer</summary>
        public uint headerKey;
        /// <summary>Offset 0x08, number of data blocks in this extension</summary>
        public uint highSeq;
        /// <summary>Offset 0x0C, unused (== 0)</summary>
        public uint dataSize;
        /// <summary>Offset 0x10, unused (== 0)</summary>
        public uint firstData;
        /// <summary>Offset 0x14, checksum</summary>
        public uint checksum;
        /// <summary>
        ///     Offset 0x18, data block pointers.
        ///     Size intentionally incorrect to allow marshaling.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public uint[] dataBlocks;

        // Following fields are at end of block
        // uint info; // == 0
        // uint nextSameHash; // == 0
        // uint parent; // header block
        // uint extension; // next extension block
        // uint secType; // == ST_FILE (-3)
    }

#endregion

#region Nested type: DirectoryBlock

    /// <summary>Directory block structure</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct DirectoryBlock
    {
        /// <summary>Offset 0x00, block type = T_HEADER (2)</summary>
        public uint type;
        /// <summary>Offset 0x04, self pointer</summary>
        public uint headerKey;
        /// <summary>Offset 0x08, unused (== 0)</summary>
        public uint highSeq;
        /// <summary>Offset 0x0C, unused (== 0)</summary>
        public uint hashTableSize;
        /// <summary>Offset 0x10, unused (== 0)</summary>
        public uint reserved;
        /// <summary>Offset 0x14, checksum</summary>
        public uint checksum;
        /// <summary>
        ///     Offset 0x18, hash table.
        ///     Size intentionally incorrect to allow marshaling.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public uint[] hashTable;

        // Following fields are at end of block
        // uint access;
        // uint reserved2;
        // byte commLen;
        // byte[79] comment;
        // uint days, mins, ticks;
        // byte nameLen;
        // byte[30] dirName;
        // uint real; // == 0
        // uint nextLink;
        // uint nextSameHash;
        // uint parent;
        // uint extension; // first directory cache (FFS)
        // uint secType; // == ST_DIR (2)
    }

#endregion

#region Nested type: OFSDataBlock

    /// <summary>OFS (Old File System) data block structure</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct OFSDataBlock
    {
        /// <summary>Offset 0x00, block type = T_DATA (8)</summary>
        public uint type;
        /// <summary>Offset 0x04, pointer to file header block</summary>
        public uint headerKey;
        /// <summary>Offset 0x08, sequence number of this data block</summary>
        public uint seqNum;
        /// <summary>Offset 0x0C, data size in this block (&lt;= 488)</summary>
        public uint dataSize;
        /// <summary>Offset 0x10, next data block pointer</summary>
        public uint nextData;
        /// <summary>Offset 0x14, checksum</summary>
        public uint checksum;
        /// <summary>Offset 0x18, file data (488 bytes for 512-byte blocks)</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 488)]
        public byte[] data;
    }

#endregion

#region Nested type: BitmapBlock

    /// <summary>Bitmap block structure</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct BitmapBlock
    {
        /// <summary>Offset 0x00, checksum</summary>
        public uint checksum;
        /// <summary>Offset 0x04, bitmap data (127 longs for 512-byte blocks)</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 127)]
        public uint[] map;
    }

#endregion

#region Nested type: BitmapExtensionBlock

    /// <summary>Bitmap extension block structure</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct BitmapExtensionBlock
    {
        /// <summary>Offset 0x00, bitmap block pointers (127 entries for 512-byte blocks)</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 127)]
        public uint[] bmPages;
        /// <summary>Offset 0x1FC, next bitmap extension block</summary>
        public uint nextBlock;
    }

#endregion

#region Nested type: LinkBlock

    /// <summary>Hard/soft link block structure</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct LinkBlock
    {
        /// <summary>Offset 0x00, block type = T_HEADER (2)</summary>
        public uint type;
        /// <summary>Offset 0x04, self pointer</summary>
        public uint headerKey;
        /// <summary>Offset 0x08, reserved</summary>
        public uint reserved1;
        /// <summary>Offset 0x0C, reserved</summary>
        public uint reserved2;
        /// <summary>Offset 0x10, reserved</summary>
        public uint reserved3;
        /// <summary>Offset 0x14, checksum</summary>
        public uint checksum;
        /// <summary>Offset 0x18, real name for hard links (64 bytes)</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] realName;

        // Following fields are at end of block
        // uint days, mins, ticks;
        // byte nameLen;
        // byte[30] name;
        // uint realEntry; // pointer to original file/dir
        // uint nextLink; // link chain
        // uint nextSameHash;
        // uint parent;
        // uint reserved;
        // uint secType; // ST_LFILE (-4), ST_LDIR (4), or ST_LSOFT (3)
    }

#endregion

#region Nested type: DirectoryCacheBlock

    /// <summary>Directory cache block structure (FFS with DIRCACHE flag)</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct DirectoryCacheBlock
    {
        /// <summary>Offset 0x00, block type = T_DIRC (33)</summary>
        public uint type;
        /// <summary>Offset 0x04, self pointer</summary>
        public uint headerKey;
        /// <summary>Offset 0x08, parent directory</summary>
        public uint parent;
        /// <summary>Offset 0x0C, number of records in this block</summary>
        public uint recordsNb;
        /// <summary>Offset 0x10, next directory cache block</summary>
        public uint nextDirC;
        /// <summary>Offset 0x14, checksum</summary>
        public uint checksum;
        /// <summary>Offset 0x18, cache records (488 bytes for 512-byte blocks)</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 488)]
        public byte[] records;
    }

#endregion
}