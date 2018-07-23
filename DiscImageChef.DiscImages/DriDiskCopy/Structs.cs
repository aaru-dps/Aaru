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
//     Contains structures for Digital Research's DISKCOPY disk images.
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

namespace DiscImageChef.DiscImages
{
    public partial class DriDiskCopy
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DriFooter
        {
            /// <summary>Signature: "DiskImage 2.01 (C) 1990,1991 Digital Research Inc\0"</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 51)]
            public byte[] signature;
            /// <summary>Information about the disk image, mostly imitates FAT BPB</summary>
            public DriBpb bpb;
            /// <summary>Information about the disk image, mostly imitates FAT BPB, copy</summary>
            public DriBpb bpbcopy;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DriBpb
        {
            /// <summary>Seems to be always 0x05</summary>
            public byte five;
            /// <summary>A drive code that corresponds (but it not equal to) CMOS drive types</summary>
            public DriDriveCodes driveCode;
            /// <summary>Unknown seems to be always 2</summary>
            public ushort unknown;
            /// <summary>Cylinders</summary>
            public ushort cylinders;
            /// <summary>Seems to always be 0</summary>
            public byte unknown2;
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors per cluster</summary>
            public byte spc;
            /// <summary>Sectors between BPB and FAT</summary>
            public ushort rsectors;
            /// <summary>How many FATs</summary>
            public byte fats_no;
            /// <summary>Entries in root directory</summary>
            public ushort root_entries;
            /// <summary>Total sectors</summary>
            public ushort sectors;
            /// <summary>Media descriptor</summary>
            public byte media_descriptor;
            /// <summary>Sectors per FAT</summary>
            public ushort spfat;
            /// <summary>Sectors per track</summary>
            public ushort sptrack;
            /// <summary>Heads</summary>
            public ushort heads;
            /// <summary>Hidden sectors before BPB</summary>
            public uint hsectors;
            /// <summary>Drive number</summary>
            public byte drive_no;
            /// <summary>Seems to be 0</summary>
            public ulong unknown3;
            /// <summary>Seems to be 0</summary>
            public byte unknown4;
            /// <summary>Sectors per track (again?)</summary>
            public ushort sptrack2;
            /// <summary>Seems to be 0</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 144)]
            public byte[] unknown5;
        }
    }
}