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
//     Contains structures for QEMU Copy-On-Write v2 disk images.
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

using System.Runtime.InteropServices;

namespace Aaru.DiscImages;

public sealed partial class Qcow2
{
    /// <summary>QCOW header, big-endian</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Header
    {
        /// <summary>
        ///     <see cref="Qcow2.QCOW_MAGIC" />
        /// </summary>
        public uint magic;
        /// <summary>Must be 1</summary>
        public uint version;
        /// <summary>Offset inside file to string containing backing file</summary>
        public readonly ulong backing_file_offset;
        /// <summary>Size of <see cref="backing_file_offset" /></summary>
        public readonly uint backing_file_size;
        /// <summary>Cluster bits</summary>
        public uint cluster_bits;
        /// <summary>Size in bytes</summary>
        public ulong size;
        /// <summary>Encryption method</summary>
        public readonly uint crypt_method;
        /// <summary>Size of L1 table</summary>
        public uint l1_size;
        /// <summary>Offset to L1 table</summary>
        public ulong l1_table_offset;
        /// <summary>Offset to reference count table</summary>
        public ulong refcount_table_offset;
        /// <summary>How many clusters does the refcount table span</summary>
        public uint refcount_table_clusters;
        /// <summary>Number of snapshots</summary>
        public readonly uint nb_snapshots;
        /// <summary>Offset to QCowSnapshotHeader</summary>
        public readonly ulong snapshots_offset;

        // Added in version 3
        public readonly ulong features;
        public readonly ulong compat_features;
        public readonly ulong autoclear_features;
        public readonly uint  refcount_order;
        public          uint  header_length;
    }
}