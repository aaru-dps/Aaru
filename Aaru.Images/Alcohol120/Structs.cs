// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains structures for Alcohol 120% disc images.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;

namespace Aaru.DiscImages
{
    public sealed partial class Alcohol120
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Header
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] signature;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] version;
            public MediumType type;
            public ushort     sessions;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public ushort[] unknown1;
            public readonly ushort bcaLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public uint[] unknown2;
            public uint bcaOffset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public uint[] unknown3;
            public uint structuresOffset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public uint[] unknown4;
            public          uint sessionOffset;
            public readonly uint dpmOffset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Session
        {
            public          int    sessionStart;
            public          int    sessionEnd;
            public          ushort sessionSequence;
            public          byte   allBlocks;
            public          byte   nonTrackBlocks;
            public          ushort firstTrack;
            public          ushort lastTrack;
            public readonly uint   unknown;
            public          uint   trackOffset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Track
        {
            public TrackMode      mode;
            public SubchannelMode subMode;
            public byte           adrCtl;
            public byte           tno;
            public byte           point;
            public byte           min;
            public byte           sec;
            public byte           frame;
            public byte           zero;
            public byte           pmin;
            public byte           psec;
            public byte           pframe;
            public uint           extraOffset;
            public ushort         sectorSize;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
            public byte[] unknown;
            public uint  startLba;
            public ulong startOffset;
            public uint  files;
            public uint  footerOffset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
            public byte[] unknown2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TrackExtra
        {
            public uint pregap;
            public uint sectors;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Footer
        {
            public          uint filenameOffset;
            public          uint widechar;
            public readonly uint unknown1;
            public readonly uint unknown2;
        }
    }
}