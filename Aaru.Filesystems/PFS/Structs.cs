// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Professional File System plugin.
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

// ReSharper disable UnusedType.Local

using System.Runtime.InteropServices;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the Professional File System</summary>
public sealed partial class PFS
{
    /// <summary>Boot block, first 2 sectors</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct BootBlock
    {
        /// <summary>"PFS\1" disk type</summary>
        public readonly uint diskType;
        /// <summary>Boot code, til completion</summary>
        public readonly byte[] bootCode;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct RootBlock
    {
        /// <summary>Disk type</summary>
        public readonly uint diskType;
        /// <summary>Options</summary>
        public readonly uint options;
        /// <summary>Current datestamp</summary>
        public readonly uint datestamp;
        /// <summary>Volume creation day</summary>
        public readonly ushort creationday;
        /// <summary>Volume creation minute</summary>
        public readonly ushort creationminute;
        /// <summary>Volume creation tick</summary>
        public readonly ushort creationtick;
        /// <summary>AmigaDOS protection bits</summary>
        public readonly ushort protection;
        /// <summary>Volume label (Pascal string)</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] diskname;
        /// <summary>Last reserved block</summary>
        public readonly uint lastreserved;
        /// <summary>First reserved block</summary>
        public readonly uint firstreserved;
        /// <summary>Free reserved blocks</summary>
        public readonly uint reservedfree;
        /// <summary>Size of reserved blocks in bytes</summary>
        public readonly ushort reservedblocksize;
        /// <summary>Blocks in rootblock, including bitmap</summary>
        public readonly ushort rootblockclusters;
        /// <summary>Free blocks</summary>
        public readonly uint blocksfree;
        /// <summary>Blocks that must be always free</summary>
        public readonly uint alwaysfree;
        /// <summary>Current bitmapfield number for allocation</summary>
        public readonly uint rovingPointer;
        /// <summary>Pointer to deldir</summary>
        public readonly uint delDirPtr;
        /// <summary>Disk size in sectors</summary>
        public readonly uint diskSize;
        /// <summary>Rootblock extension</summary>
        public readonly uint extension;
        /// <summary>Unused</summary>
        public readonly uint unused;
    }
}