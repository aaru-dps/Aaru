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
//     Contains structures for partclone disk images.
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

public sealed partial class PartClone
{
    /// <summary>PartClone disk image header, little-endian</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Header
    {
        /// <summary>Magic, <see cref="PartClone._partCloneMagic" /></summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public readonly byte[] magic;
        /// <summary>Source filesystem</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public readonly byte[] filesystem;
        /// <summary>Version</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly byte[] version;
        /// <summary>Padding</summary>
        public readonly ushort padding;
        /// <summary>Block (sector) size</summary>
        public readonly uint blockSize;
        /// <summary>Size of device containing the cloned partition</summary>
        public readonly ulong deviceSize;
        /// <summary>Total blocks in cloned partition</summary>
        public readonly ulong totalBlocks;
        /// <summary>Used blocks in cloned partition</summary>
        public readonly ulong usedBlocks;
        /// <summary>Empty space</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4096)]
        public readonly byte[] buffer;
    }
}