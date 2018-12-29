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
//     Contains structures for CPCEMU disk images.
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
using DiscImageChef.Decoders.Floppy;

namespace DiscImageChef.DiscImages
{
    public partial class Cpcdsk
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CpcDiskInfo
        {
            /// <summary>
            ///     Magic number, "MV - CPCEMU Disk-File" in old files, "EXTENDED CPC DSK File" in extended ones
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 21)]
            public byte[] magic;
            /// <summary>
            ///     Second part of magic, should be "\r\nDisk-Info\r\n" in all, but some emulators write spaces instead.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
            public byte[] magic2;
            /// <summary>
            ///     Creator application (can be null)
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public byte[] creator;
            /// <summary>
            ///     Tracks
            /// </summary>
            public byte tracks;
            /// <summary>
            ///     Sides
            /// </summary>
            public byte sides;
            /// <summary>
            ///     Size of a track including the 256 bytes header. Unused by extended format, as this format includes a table in the
            ///     next field
            /// </summary>
            public ushort tracksize;
            /// <summary>
            ///     Size of each track in the extended format. 0 indicates track is not formatted and not present in image.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 204)]
            public byte[] tracksizeTable;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CpcTrackInfo
        {
            /// <summary>
            ///     Magic number, "Track-Info\r\n"
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public byte[] magic;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] carriageReturn;
            /// <summary>
            ///     Padding
            /// </summary>
            public uint padding;
            /// <summary>
            ///     Track number
            /// </summary>
            public byte track;
            /// <summary>
            ///     Side number
            /// </summary>
            public byte side;
            /// <summary>
            ///     Controller data rate
            /// </summary>
            public byte dataRate;
            /// <summary>
            ///     Recording mode
            /// </summary>
            public byte recordingMode;
            /// <summary>
            ///     Bytes per sector
            /// </summary>
            public IBMSectorSizeCode bps;
            /// <summary>
            ///     How many sectors in this track
            /// </summary>
            public byte sectors;
            /// <summary>
            ///     GAP#3
            /// </summary>
            public byte gap3;
            /// <summary>
            ///     Filler
            /// </summary>
            public byte filler;
            /// <summary>
            ///     Informatino for up to 32 sectors
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public CpcSectorInfo[] sectorsInfo;
        }

        /// <summary>
        ///     Sector information
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CpcSectorInfo
        {
            /// <summary>
            ///     Track number from address mark
            /// </summary>
            public byte track;
            /// <summary>
            ///     Side number from address mark
            /// </summary>
            public byte side;
            /// <summary>
            ///     Sector ID from address mark
            /// </summary>
            public byte id;
            /// <summary>
            ///     Sector size from address mark
            /// </summary>
            public IBMSectorSizeCode size;
            /// <summary>
            ///     ST1 register from controller
            /// </summary>
            public byte st1;
            /// <summary>
            ///     ST2 register from controller
            /// </summary>
            public byte st2;
            /// <summary>
            ///     Length in bytes of this sector size. If it is bigger than expected sector size, it's a weak sector read several
            ///     times.
            /// </summary>
            public ushort len;
        }
    }
}