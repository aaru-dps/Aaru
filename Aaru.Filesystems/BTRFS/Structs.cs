// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : B-tree file system plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the B-tree file system and shows information.
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

using System;
using System.Runtime.InteropServices;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the b-tree filesystem (btrfs)</summary>
public sealed partial class BTRFS
{
#region Nested type: DevItem

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct DevItem
    {
        public readonly ulong id;
        public readonly ulong bytes;
        public readonly ulong used;
        public readonly uint  optimal_align;
        public readonly uint  optimal_width;
        public readonly uint  minimal_size;
        public readonly ulong type;
        public readonly ulong generation;
        public readonly ulong start_offset;
        public readonly uint  dev_group;
        public readonly byte  seek_speed;
        public readonly byte  bandwidth;
        public readonly Guid  device_uuid;
        public readonly Guid  uuid;
    }

#endregion

#region Nested type: SuperBlock

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct SuperBlock
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
        public readonly byte[] checksum;
        public readonly Guid    uuid;
        public readonly ulong   pba;
        public readonly ulong   flags;
        public readonly ulong   magic;
        public readonly ulong   generation;
        public readonly ulong   root_lba;
        public readonly ulong   chunk_lba;
        public readonly ulong   log_lba;
        public readonly ulong   log_root_transid;
        public readonly ulong   total_bytes;
        public readonly ulong   bytes_used;
        public readonly ulong   root_dir_objectid;
        public readonly ulong   num_devices;
        public readonly uint    sectorsize;
        public readonly uint    nodesize;
        public readonly uint    leafsize;
        public readonly uint    stripesize;
        public readonly uint    n;
        public readonly ulong   chunk_root_generation;
        public readonly ulong   compat_flags;
        public readonly ulong   compat_ro_flags;
        public readonly ulong   incompat_flags;
        public readonly ushort  csum_type;
        public readonly byte    root_level;
        public readonly byte    chunk_root_level;
        public readonly byte    log_root_level;
        public readonly DevItem dev_item;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x100)]
        public readonly string label;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x100)]
        public readonly byte[] reserved;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x800)]
        public readonly byte[] chunkpairs;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x4D5)]
        public readonly byte[] unused;
    }

#endregion
}