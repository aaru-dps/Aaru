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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of Amiga Fast File System (AFFS)</summary>
public sealed partial class AmigaDOSPlugin
{
#region Nested type: BootBlock

    /// <summary>Boot block, first 2 sectors</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BootBlock
    {
        /// <summary>Offset 0x00, "DOSx" disk type</summary>
        public readonly uint diskType;
        /// <summary>Offset 0x04, Checksum</summary>
        public readonly uint checksum;
        /// <summary>Offset 0x08, Pointer to root block, mostly invalid</summary>
        public readonly uint root_ptr;
        /// <summary>Offset 0x0C, Boot code, til completion. Size is intentionally incorrect to allow marshaling to work.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public byte[] bootCode;
    }

#endregion

#region Nested type: RootBlock

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct RootBlock
    {
        /// <summary>Offset 0x00, block type, value = T_HEADER (2)</summary>
        public uint type;
        /// <summary>Offset 0x04, unused</summary>
        public readonly uint headerKey;
        /// <summary>Offset 0x08, unused</summary>
        public readonly uint highSeq;
        /// <summary>Offset 0x0C, longs used by hash table</summary>
        public uint hashTableSize;
        /// <summary>Offset 0x10, unused</summary>
        public readonly uint firstData;
        /// <summary>Offset 0x14, Rootblock checksum</summary>
        public uint checksum;
        /// <summary>
        ///     Offset 0x18, Hashtable, size = (block size / 4) - 56 or size = hashTableSize. Size intentionally bad to allow
        ///     marshalling to work.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public uint[] hashTable;
        /// <summary>Offset 0x18+hashTableSize*4+0, bitmap flag, 0xFFFFFFFF if valid</summary>
        public readonly uint bitmapFlag;
        /// <summary>Offset 0x18+hashTableSize*4+4, bitmap pages, 25 entries</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 25)]
        public readonly uint[] bitmapPages;
        /// <summary>Offset 0x18+hashTableSize*4+104, pointer to bitmap extension block</summary>
        public readonly uint bitmapExtensionBlock;
        /// <summary>Offset 0x18+hashTableSize*4+108, last root alteration days since 1978/01/01</summary>
        public readonly uint rDays;
        /// <summary>Offset 0x18+hashTableSize*4+112, last root alteration minutes past midnight</summary>
        public readonly uint rMins;
        /// <summary>Offset 0x18+hashTableSize*4+116, last root alteration ticks (1/50 secs)</summary>
        public readonly uint rTicks;
        /// <summary>Offset 0x18+hashTableSize*4+120, disk name, pascal string, 31 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 31)]
        public readonly byte[] diskName;
        /// <summary>Offset 0x18+hashTableSize*4+151, unused</summary>
        public readonly byte padding;
        /// <summary>Offset 0x18+hashTableSize*4+152, unused</summary>
        public readonly uint reserved1;
        /// <summary>Offset 0x18+hashTableSize*4+156, unused</summary>
        public readonly uint reserved2;
        /// <summary>Offset 0x18+hashTableSize*4+160, last disk alteration days since 1978/01/01</summary>
        public readonly uint vDays;
        /// <summary>Offset 0x18+hashTableSize*4+164, last disk alteration minutes past midnight</summary>
        public readonly uint vMins;
        /// <summary>Offset 0x18+hashTableSize*4+168, last disk alteration ticks (1/50 secs)</summary>
        public readonly uint vTicks;
        /// <summary>Offset 0x18+hashTableSize*4+172, filesystem creation days since 1978/01/01</summary>
        public readonly uint cDays;
        /// <summary>Offset 0x18+hashTableSize*4+176, filesystem creation minutes since 1978/01/01</summary>
        public readonly uint cMins;
        /// <summary>Offset 0x18+hashTableSize*4+180, filesystem creation ticks since 1978/01/01</summary>
        public readonly uint cTicks;
        /// <summary>Offset 0x18+hashTableSize*4+184, unused</summary>
        public readonly uint nextHash;
        /// <summary>Offset 0x18+hashTableSize*4+188, unused</summary>
        public readonly uint parentDir;
        /// <summary>Offset 0x18+hashTableSize*4+192, first directory cache block</summary>
        public readonly uint extension;
        /// <summary>Offset 0x18+hashTableSize*4+196, block secondary type = ST_ROOT (1)</summary>
        public uint sec_type;
    }

#endregion
}