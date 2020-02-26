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
//     Contains structures for partimage disk images.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;

namespace DiscImageChef.DiscImages
{
    public partial class Partimage
    {
        /// <summary>
        ///     Partimage disk image header, little-endian
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PartimageHeader
        {
            /// <summary>
            ///     Magic, <see cref="Partimage.partimageMagic" />
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] magic;
            /// <summary>
            ///     Source filesystem
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] version;
            /// <summary>
            ///     Volume number
            /// </summary>
            public uint volumeNumber;
            /// <summary>
            ///     Image identifier
            /// </summary>
            public ulong identificator;
            /// <summary>
            ///     Empty space
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 404)]
            public byte[] reserved;
        }

        struct PortableTm
        {
            public uint Second;
            public uint Minute;
            public uint Hour;
            public uint DayOfMonth;
            public uint Month;
            public uint Year;
            public uint DayOfWeek;
            public uint DayOfYear;
            public uint IsDst;

            public uint GmtOff;
            public uint Timezone;
        }

        /// <summary>
        ///     Partimage CMainHeader
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PartimageMainHeader
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
            public byte[] szFileSystem; // ext2fs, ntfs, reiserfs, ...
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DESCRIPTION)]
            public byte[] szPartDescription; // user description of the partition
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DEVICENAMELEN)]
            public byte[] szOriginalDevice; // original partition name
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4095)]
            public byte[] szFirstImageFilepath; //MAXPATHLEN]; // for splitted image files

            // system and hardware infos
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_UNAMEINFOLEN)]
            public byte[] szUnameSysname;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_UNAMEINFOLEN)]
            public byte[] szUnameNodename;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_UNAMEINFOLEN)]
            public byte[] szUnameRelease;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_UNAMEINFOLEN)]
            public byte[] szUnameVersion;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_UNAMEINFOLEN)]
            public byte[] szUnameMachine;

            public PCompression dwCompression; // COMPRESS_XXXXXX
            public uint         dwMainFlags;
            public PortableTm   dateCreate; // date of image creation
            public ulong        qwPartSize; // size of the partition in bytes
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_HOSTNAMESIZE)]
            public byte[] szHostname;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] szVersion; // version of the image file

            // MBR backup
            public uint dwMbrCount; // how many MBR are saved in the image file
            public uint dwMbrSize;  // size of a MBR record (allow to change the size in the next versions)

            // future encryption support
            public PEncryption dwEncryptAlgo; // algo used to encrypt data
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] cHashTestKey; // used to test the password without giving it

            // reserved for future use (save DiskLabel, Extended partitions, ...)
            public uint dwReservedFuture000;
            public uint dwReservedFuture001;
            public uint dwReservedFuture002;
            public uint dwReservedFuture003;
            public uint dwReservedFuture004;
            public uint dwReservedFuture005;
            public uint dwReservedFuture006;
            public uint dwReservedFuture007;
            public uint dwReservedFuture008;
            public uint dwReservedFuture009;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6524)]
            public byte[] cReserved; // Adjust to fit with total header size

            public uint crc;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CMbr // must be 1024
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MBR_SIZE_WHOLE)]
            public byte[] cData;
            public uint dwDataCRC;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DEVICENAMELEN)]
            public byte[] szDevice; // ex: "hda"

            // disk identificators
            ulong qwBlocksCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DESC_MODEL)]
            public byte[] szDescModel;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 884)]
            public byte[] cReserved; // for future use

            //public byte[] szDescGeometry[MAX_DESC_GEOMETRY];
            //public byte[] szDescIdentify[MAX_DESC_IDENTIFY];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CCheck
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            byte[] cMagic;      // must be 'C','H','K'
            public uint  dwCRC; // CRC of the CHECK_FREQUENCY blocks
            public ulong qwPos; // number of the last block written
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CLocalHeader // size must be 16384 (adjust the reserved data)
        {
            public ulong qwBlockSize;
            public ulong qwUsedBlocks;
            public ulong qwBlocksCount;
            public ulong qwBitmapSize; // bytes in the bitmap
            public ulong qwBadBlocksCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] szLabel;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16280)]
            public byte[] cReserved; // Adjust to fit with total header size

            public uint crc;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CMainTail // size must be 16384 (adjust the reserved data)
        {
            public ulong qwCRC;
            public uint  dwVolumeNumber;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16372)]
            public byte[] cReserved; // Adjust to fit with total header size
        }
    }
}