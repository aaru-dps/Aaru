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
// Copyright © 2021-2024 Michael Drüing
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Aaru.Images;

[SuppressMessage("ReSharper", "UnusedType.Local")]
public sealed partial class DiskDupe
{
    readonly DiskType[] _diskTypes =
    {
        new()
        {
            cyl = 0,
            hd  = 0,
            spt = 0
        }, // Type 0 - invalid
        new()
        {
            cyl = 40,
            hd  = 2,
            spt = 9
        }, // Type 1 - 360k
        new()
        {
            cyl = 80,
            hd  = 2,
            spt = 15
        }, // Type 2 - 1.2m
        new()
        {
            cyl = 80,
            hd  = 2,
            spt = 9
        }, // Type 3 - 720k
        new()
        {
            cyl = 80,
            hd  = 2,
            spt = 18
        } // Type 4 - 1.44m
    };

#region Nested type: DiskType

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    struct DiskType
    {
        public byte cyl;
        public byte hd;
        public byte spt;
    }

#endregion

#region Nested type: FileHeader

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

#endregion

#region Nested type: TrackInfo

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TrackInfo
    {
        public readonly byte present; // 1 = present, 0 = absent
        public readonly byte trackNumber;
        public readonly byte zero1;
        public readonly byte zero2;
        public readonly byte zero3;
        public readonly byte unknown; // always 1?
    }

#endregion
}