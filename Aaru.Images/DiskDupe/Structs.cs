// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Michael Drüing <michael@drueing.de>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains structures for DiskDupe DDI disk images.
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
// Copyright © 2021 Michael Drüing
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Aaru.DiscImages
{
    [SuppressMessage("ReSharper", "UnusedType.Local")]
    public sealed partial class DiskDupe
    {
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        struct DiskType {
            public byte cyl;
            public byte hd;
            public byte spt;
        }

        readonly DiskType[] _diskTypes = {
            new DiskType { cyl = 0,  hd = 0, spt = 0  },  // Type 0 - invalid
            new DiskType { cyl = 40, hd = 2, spt = 9  },  // Type 1 - 360k
            new DiskType { cyl = 80, hd = 2, spt = 15 },  // Type 2 - 1.2m
            new DiskType { cyl = 80, hd = 2, spt = 9  },  // Type 3 - 720k
            new DiskType { cyl = 80, hd = 2, spt = 18 }   // Type 4 - 1.44m
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TrackInfo {
            public byte present; // 1 = present, 0 = absent
            public byte trackNumber;
            public byte zero1;
            public byte zero2;
            public byte zero3;
            public byte unknown; // always 1?
        }

        /// <summary>The global header of a DDI image file</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct FileHeader
        {
            /// <summary>The file signature</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public byte[] signature;

            /// <summary>Disk type</summary>
            public byte diskType;
        }
    }
}
