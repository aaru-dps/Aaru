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
//     Contains structures for VMware disk images.
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

using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes.Interfaces;

namespace DiscImageChef.DiscImages
{
    public partial class VMware
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VMwareExtentHeader
        {
            public                               uint   magic;
            public                               uint   version;
            public                               uint   flags;
            public                               ulong  capacity;
            public                               ulong  grainSize;
            public                               ulong  descriptorOffset;
            public                               ulong  descriptorSize;
            public                               uint   GTEsPerGT;
            public                               ulong  rgdOffset;
            public                               ulong  gdOffset;
            public                               ulong  overhead;
            [MarshalAs(UnmanagedType.U1)] public bool   uncleanShutdown;
            public                               byte   singleEndLineChar;
            public                               byte   nonEndLineChar;
            public                               byte   doubleEndLineChar1;
            public                               byte   doubleEndLineChar2;
            public                               ushort compression;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 433)]
            public byte[] padding;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VMwareCowHeader
        {
            public uint magic;
            public uint version;
            public uint flags;
            public uint sectors;
            public uint grainSize;
            public uint gdOffset;
            public uint numGDEntries;
            public uint freeSector;
            public uint cylinders;
            public uint heads;
            public uint spt;
            // It stats on cylinders, above, but, don't care
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024 - 12)]
            public byte[] parentFileName;
            public uint parentGeneration;
            public uint generation;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
            public byte[] name;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
            public byte[] description;
            public uint savedGeneration;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] reserved;
            [MarshalAs(UnmanagedType.U1)] public bool uncleanShutdown;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 396)]
            public byte[] padding;
        }

        struct VMwareExtent
        {
            public string  Access;
            public uint    Sectors;
            public string  Type;
            public IFilter Filter;
            public string  Filename;
            public uint    Offset;
        }
    }
}