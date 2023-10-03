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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.DiscImages;

public sealed partial class VMware
{
#region Nested type: CowHeader

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct CowHeader
    {
        public readonly uint magic;
        public readonly uint version;
        public readonly uint flags;
        public readonly uint sectors;
        public readonly uint grainSize;
        public readonly uint gdOffset;
        public readonly uint numGDEntries;
        public readonly uint freeSector;
        public readonly uint cylinders;
        public readonly uint heads;
        public readonly uint spt;

        // It stats on cylinders, above, but, don't care
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024 - 12)]
        public readonly byte[] parentFileName;
        public readonly uint parentGeneration;
        public readonly uint generation;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
        public readonly byte[] name;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public readonly byte[] description;
        public readonly uint savedGeneration;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] reserved;
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool uncleanShutdown;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 396)]
        public readonly byte[] padding;
    }

#endregion

#region Nested type: Extent

    struct Extent
    {
        public string  Access;
        public uint    Sectors;
        public string  Type;
        public IFilter Filter;
        public string  Filename;
        public uint    Offset;
    }

#endregion

#region Nested type: ExtentHeader

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ExtentHeader
    {
        public readonly uint  magic;
        public readonly uint  version;
        public readonly uint  flags;
        public readonly ulong capacity;
        public readonly ulong grainSize;
        public readonly ulong descriptorOffset;
        public readonly ulong descriptorSize;
        public readonly uint  GTEsPerGT;
        public readonly ulong rgdOffset;
        public readonly ulong gdOffset;
        public readonly ulong overhead;
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool uncleanShutdown;
        public readonly byte   singleEndLineChar;
        public readonly byte   nonEndLineChar;
        public readonly byte   doubleEndLineChar1;
        public readonly byte   doubleEndLineChar2;
        public readonly ushort compression;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 433)]
        public readonly byte[] padding;
    }

#endregion
}