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
//     Contains structures for Parallels disk images.
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

namespace Aaru.DiscImages;

public sealed partial class Parallels
{
    /// <summary>Parallels disk image header, little-endian</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Header
    {
        /// <summary>Magic, <see cref="Parallels._magic" /> or <see cref="Parallels._extMagic" /></summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] magic;
        /// <summary>Version</summary>
        public uint version;
        /// <summary>Disk geometry parameter</summary>
        public uint heads;
        /// <summary>Disk geometry parameter</summary>
        public uint cylinders;
        /// <summary>Cluster size in sectors</summary>
        public uint cluster_size;
        /// <summary>Entries in BAT (clusters in image)</summary>
        public uint bat_entries;
        /// <summary>Disk size in sectors</summary>
        public ulong sectors;
        /// <summary>
        ///     Set to <see cref="Parallels.PARALLELS_INUSE" /> if image is opened by any software,
        ///     <see cref="Parallels.PARALLELS_CLOSED" /> if not, and 0 if old version
        /// </summary>
        public uint in_use;
        /// <summary>Offset in sectors to start of data</summary>
        public uint data_off;
        /// <summary>Flags</summary>
        public readonly uint flags;
        /// <summary>Offset in sectors to format extension</summary>
        public readonly ulong ext_off;
    }
}