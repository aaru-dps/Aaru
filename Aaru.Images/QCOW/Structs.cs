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
//     Contains structures for QEMU Copy-On-Write disk images.
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

public sealed partial class Qcow
{
    /// <summary>QCOW header, big-endian</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Header
    {
        /// <summary>
        ///     <see cref="Qcow.QCOW_MAGIC" />
        /// </summary>
        public uint magic;
        /// <summary>Must be 1</summary>
        public uint version;
        /// <summary>Offset inside file to string containing backing file</summary>
        public readonly ulong backing_file_offset;
        /// <summary>Size of <see cref="backing_file_offset" /></summary>
        public readonly uint backing_file_size;
        /// <summary>Modification time</summary>
        public uint mtime;
        /// <summary>Size in bytes</summary>
        public ulong size;
        /// <summary>Cluster bits</summary>
        public byte cluster_bits;
        /// <summary>L2 table bits</summary>
        public byte l2_bits;
        /// <summary>Padding</summary>
        public readonly ushort padding;
        /// <summary>Encryption method</summary>
        public readonly uint crypt_method;
        /// <summary>Offset to L1 table</summary>
        public ulong l1_table_offset;
    }
}