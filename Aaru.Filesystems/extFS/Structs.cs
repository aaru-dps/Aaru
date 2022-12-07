// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Linux extended filesystem plugin.
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

using System.Diagnostics.CodeAnalysis;

namespace Aaru.Filesystems;

// Information from the Linux kernel
/// <inheritdoc />
/// <summary>Implements detection of the Linux extended filesystem</summary>

// ReSharper disable once InconsistentNaming
public sealed partial class extFS
{
    /// <summary>ext superblock</summary>
    #pragma warning disable CS0649
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    struct SuperBlock
    {
        /// <summary>0x000, inodes on volume</summary>
        public uint inodes;
        /// <summary>0x004, zones on volume</summary>
        public uint zones;
        /// <summary>0x008, first free block</summary>
        public uint firstfreeblk;
        /// <summary>0x00C, free blocks count</summary>
        public uint freecountblk;
        /// <summary>0x010, first free inode</summary>
        public uint firstfreeind;
        /// <summary>0x014, free inodes count</summary>
        public uint freecountind;
        /// <summary>0x018, first data zone</summary>
        public uint firstdatazone;
        /// <summary>0x01C, log zone size</summary>
        public uint logzonesize;
        /// <summary>0x020, max zone size</summary>
        public uint maxsize;
        /// <summary>0x024, reserved</summary>
        public uint reserved1;
        /// <summary>0x028, reserved</summary>
        public uint reserved2;
        /// <summary>0x02C, reserved</summary>
        public uint reserved3;
        /// <summary>0x030, reserved</summary>
        public uint reserved4;
        /// <summary>0x034, reserved</summary>
        public uint reserved5;
        /// <summary>0x038, 0x137D (little endian)</summary>
        public ushort magic;
    }
    #pragma warning restore CS0649
}