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
//     Contains structures for HD-Copy disk images.
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
// Copyright © 2017 Michael Drüing
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.DiscImages;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

[SuppressMessage("ReSharper", "UnusedType.Local")]
public sealed partial class HdCopy
{
    /// <summary>The global header of a HDCP image file</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct FileHeader
    {
        /// <summary>Last cylinder (zero-based)</summary>
        public byte lastCylinder;

        /// <summary>Sectors per track</summary>
        public byte sectorsPerTrack;

        /// <summary>
        ///     The track map. It contains one byte for each track. Up to 82 tracks (41 tracks * 2 sides) are supported. 0
        ///     means track is not present, 1 means it is present. The first 2 tracks are always present.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2 * 82)]
        public byte[] trackMap;
    }

    /// <summary>The header for a RLE-compressed block</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct BlockHeader
    {
        /// <summary>The length of the compressed block, in bytes. Little-endian.</summary>
        public readonly ushort length;

        /// <summary>The byte value used as RLE escape sequence</summary>
        public readonly byte escape;
    }
}