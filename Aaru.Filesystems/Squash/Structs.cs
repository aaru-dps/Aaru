// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Squash file system plugin.
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
/// <summary>Implements detection of the squash filesystem</summary>
public sealed partial class Squash
{
#region Nested type: SuperBlock

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct SuperBlock
    {
        public readonly uint   magic;
        public readonly uint   inodes;
        public readonly uint   mkfs_time;
        public readonly uint   block_size;
        public readonly uint   fragments;
        public readonly ushort compression;
        public readonly ushort block_log;
        public readonly ushort flags;
        public readonly ushort no_ids;
        public readonly ushort s_major;
        public readonly ushort s_minor;
        public readonly ulong  root_inode;
        public readonly ulong  bytes_used;
        public readonly ulong  id_table_start;
        public readonly ulong  xattr_id_table_start;
        public readonly ulong  inode_table_start;
        public readonly ulong  directory_table_start;
        public readonly ulong  fragment_table_start;
        public readonly ulong  lookup_table_start;
    }

#endregion
}