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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

#pragma warning disable CS0649
namespace Aaru.DiscImages;

[SuppressMessage("ReSharper", "UnusedType.Local")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public sealed partial class Partimage
{
#region Nested type: CCheck

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct CCheck
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        readonly byte[] cMagic;      // must be 'C','H','K'
        public readonly uint  dwCRC; // CRC of the CHECK_FREQUENCY blocks
        public readonly ulong qwPos; // number of the last block written
    }

#endregion

#region Nested type: CLocalHeader

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct CLocalHeader // size must be 16384 (adjust the reserved data)
    {
        public readonly ulong qwBlockSize;
        public readonly ulong qwUsedBlocks;
        public readonly ulong qwBlocksCount;
        public readonly ulong qwBitmapSize; // bytes in the bitmap
        public readonly ulong qwBadBlocksCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public readonly byte[] szLabel;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16280)]
        public readonly byte[] cReserved; // Adjust to fit with total header size

        public readonly uint crc;
    }

#endregion

#region Nested type: CMainTail

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct CMainTail // size must be 16384 (adjust the reserved data)
    {
        public readonly ulong qwCRC;
        public readonly uint  dwVolumeNumber;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16372)]
        public readonly byte[] cReserved; // Adjust to fit with total header size
    }

#endregion

#region Nested type: CMbr

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct CMbr // must be 1024
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MBR_SIZE_WHOLE)]
        public readonly byte[] cData;
        public readonly uint dwDataCRC;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DEVICENAMELEN)]
        public readonly byte[] szDevice; // ex: "hda"

        // disk identificators
        readonly ulong qwBlocksCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DESC_MODEL)]
        public readonly byte[] szDescModel;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 884)]
        public readonly byte[] cReserved; // for future use

        //public byte[] szDescGeometry[MAX_DESC_GEOMETRY];
        //public byte[] szDescIdentify[MAX_DESC_IDENTIFY];
    }

#endregion

#region Nested type: Header

    /// <summary>Partimage disk image header, little-endian</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Header
    {
        /// <summary>Magic, <see cref="Partimage._partimageMagic" /></summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] magic;
        /// <summary>Source filesystem</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public readonly byte[] version;
        /// <summary>Volume number</summary>
        public readonly uint volumeNumber;
        /// <summary>Image identifier</summary>
        public readonly ulong identificator;
        /// <summary>Empty space</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 404)]
        public readonly byte[] reserved;
    }

#endregion

#region Nested type: MainHeader

    /// <summary>Partimage CMainHeader</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct MainHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public readonly byte[] szFileSystem; // ext2fs, ntfs, reiserfs, ...
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DESCRIPTION)]
        public readonly byte[] szPartDescription; // user description of the partition
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DEVICENAMELEN)]
        public readonly byte[] szOriginalDevice; // original partition name
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4095)]
        public readonly byte[] szFirstImageFilepath; //MAXPATHLEN]; // for splitted image files

        // system and hardware infos
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_UNAMEINFOLEN)]
        public readonly byte[] szUnameSysname;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_UNAMEINFOLEN)]
        public readonly byte[] szUnameNodename;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_UNAMEINFOLEN)]
        public readonly byte[] szUnameRelease;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_UNAMEINFOLEN)]
        public readonly byte[] szUnameVersion;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_UNAMEINFOLEN)]
        public readonly byte[] szUnameMachine;

        public readonly PCompression dwCompression; // COMPRESS_XXXXXX
        public readonly uint         dwMainFlags;
        public readonly PortableTm   dateCreate; // date of image creation
        public readonly ulong        qwPartSize; // size of the partition in bytes
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_HOSTNAMESIZE)]
        public readonly byte[] szHostname;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public readonly byte[] szVersion; // version of the image file

        // MBR backup
        public readonly uint dwMbrCount; // how many MBR are saved in the image file
        public readonly uint dwMbrSize;  // size of a MBR record (allow to change the size in the next versions)

        // future encryption support
        public readonly PEncryption dwEncryptAlgo; // algo used to encrypt data
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] cHashTestKey; // used to test the password without giving it

        // reserved for future use (save DiskLabel, Extended partitions, ...)
        public readonly uint dwReservedFuture000;
        public readonly uint dwReservedFuture001;
        public readonly uint dwReservedFuture002;
        public readonly uint dwReservedFuture003;
        public readonly uint dwReservedFuture004;
        public readonly uint dwReservedFuture005;
        public readonly uint dwReservedFuture006;
        public readonly uint dwReservedFuture007;
        public readonly uint dwReservedFuture008;
        public readonly uint dwReservedFuture009;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6524)]
        public readonly byte[] cReserved; // Adjust to fit with total header size

        public readonly uint crc;
    }

#endregion

#region Nested type: PortableTm

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

#endregion
}