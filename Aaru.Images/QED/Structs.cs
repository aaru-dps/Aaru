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
//     Contains structures for QEMU Enhanced Disk images.
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

namespace Aaru.DiscImages
{
    public sealed partial class Qed
    {
        /// <summary>QED header, big-endian</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct QedHeader
        {
            /// <summary>
            ///     <see cref="Qed.QED_MAGIC" />
            /// </summary>
            public uint magic;
            /// <summary>Cluster size in bytes</summary>
            public uint cluster_size;
            /// <summary>L1 and L2 table size in cluster</summary>
            public uint table_size;
            /// <summary>Header size in clusters</summary>
            public uint header_size;
            /// <summary>Incompatible features</summary>
            public readonly ulong features;
            /// <summary>Compatible features</summary>
            public readonly ulong compat_features;
            /// <summary>Self-resetting features</summary>
            public readonly ulong autoclear_features;
            /// <summary>Offset to L1 table</summary>
            public ulong l1_table_offset;
            /// <summary>Image size</summary>
            public ulong image_size;
            /// <summary>Offset inside file to string containing backing file</summary>
            public readonly ulong backing_file_offset;
            /// <summary>Size of <see cref="backing_file_offset" /></summary>
            public readonly uint backing_file_size;
        }
    }
}