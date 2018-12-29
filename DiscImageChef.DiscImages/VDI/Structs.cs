// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains structures for VirtualBox disk images.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace DiscImageChef.DiscImages
{
    public partial class Vdi
    {
        /// <summary>
        ///     VDI disk image header, little-endian
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VdiHeader
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string creator;
            /// <summary>
            ///     Magic, <see cref="Vdi.VDI_MAGIC" />
            /// </summary>
            public uint magic;
            /// <summary>
            ///     Version
            /// </summary>
            public ushort majorVersion;
            public ushort        minorVersion;
            public int           headerSize;
            public VdiImageType  imageType;
            public VdiImageFlags imageFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string comments;
            public uint  offsetBlocks;
            public uint  offsetData;
            public uint  cylinders;
            public uint  heads;
            public uint  spt;
            public uint  sectorSize;
            public uint  unused;
            public ulong size;
            public uint  blockSize;
            public uint  blockExtraData;
            public uint  blocks;
            public uint  allocatedBlocks;
            public Guid  uuid;
            public Guid  snapshotUuid;
            public Guid  linkUuid;
            public Guid  parentUuid;
        }
    }
}