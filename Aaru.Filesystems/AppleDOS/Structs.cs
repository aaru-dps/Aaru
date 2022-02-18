// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple DOS filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Apple DOS filesystem structures.
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

namespace Aaru.Filesystems
{
    public sealed partial class AppleDOS
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct Vtoc
        {
            public readonly byte unused1;
            public readonly byte catalogTrack;
            public readonly byte catalogSector;
            public readonly byte dosRelease;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public readonly byte[] unused2;
            public readonly byte volumeNumber;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public readonly byte[] unused3;
            public readonly byte maxTrackSectorPairsPerSector;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] unused4;
            public readonly byte  lastAllocatedSector;
            public readonly sbyte allocationDirection;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public readonly byte[] unused5;
            public readonly byte   tracks;
            public readonly byte   sectorsPerTrack;
            public readonly ushort bytesPerSector;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            public readonly byte[] bitmap;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct CatalogSector
        {
            public readonly byte unused1;
            public readonly byte trackOfNext;
            public readonly byte sectorOfNext;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] unused2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            public readonly FileEntry[] entries;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct FileEntry
        {
            public readonly byte extentTrack;
            public readonly byte extentSector;
            public readonly byte typeAndFlags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
            public readonly byte[] filename;
            public readonly ushort length;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct TrackSectorList
        {
            public readonly byte unused1;
            public readonly byte nextListTrack;
            public readonly byte nextListSector;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public readonly byte[] unused2;
            public readonly ushort sectorOffset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public readonly byte[] unused3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 122)]
            public readonly TrackSectorListEntry[] entries;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct TrackSectorListEntry
        {
            public readonly byte track;
            public readonly byte sector;
        }
    }
}