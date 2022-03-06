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
//     Contains structures for SuperCardPro flux images.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;

namespace Aaru.DiscImages;

public sealed partial class SuperCardPro
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ScpHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] signature;
        public byte        version;
        public ScpDiskType type;
        public byte        revolutions;
        public byte        start;
        public byte        end;
        public ScpFlags    flags;
        public byte        bitCellEncoding;
        public byte        heads;
        public byte        reserved;
        public uint        checksum;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 168)]
        public uint[] offsets;
    }

    public struct TrackHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] Signature;
        public byte         TrackNumber;
        public TrackEntry[] Entries;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrackEntry
    {
        public uint indexTime;
        public uint trackLength;
        public uint dataOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Footer
    {
        public readonly uint manufacturerOffset;
        public readonly uint modelOffset;
        public readonly uint serialOffset;
        public readonly uint creatorOffset;
        public readonly uint applicationOffset;
        public readonly uint commentsOffset;
        public readonly long creationTime;
        public readonly long modificationTime;
        public readonly byte applicationVersion;
        public readonly byte hardwareVersion;
        public readonly byte firmwareVersion;
        public readonly byte imageVersion;
        public readonly uint signature;
    }
}