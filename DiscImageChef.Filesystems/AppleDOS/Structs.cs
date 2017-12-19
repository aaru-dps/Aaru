// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;

namespace DiscImageChef.Filesystems.AppleDOS
{
    public partial class AppleDOS : Filesystem
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VTOC
        {
            public byte unused1;
            public byte catalogTrack;
            public byte catalogSector;
            public byte dosRelease;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] unused2;
            public byte volumeNumber;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] unused3;
            public byte maxTrackSectorPairsPerSector;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] unused4;
            public byte lastAllocatedSector;
            public sbyte allocationDirection;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] unused5;
            public byte tracks;
            public byte sectorsPerTrack;
            public ushort bytesPerSector;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)] public byte[] bitmap;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CatalogSector
        {
            public byte unused1;
            public byte trackOfNext;
            public byte sectorOfNext;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] unused2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)] public FileEntry[] entries;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct FileEntry
        {
            public byte extentTrack;
            public byte extentSector;
            public byte typeAndFlags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)] public byte[] filename;
            public ushort length;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TrackSectorList
        {
            public byte unused1;
            public byte nextListTrack;
            public byte nextListSector;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] unused2;
            public ushort sectorOffset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)] public byte[] unused3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 122)] public TrackSectorListEntry[] entries;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TrackSectorListEntry
        {
            public byte track;
            public byte sector;
        }
    }
}