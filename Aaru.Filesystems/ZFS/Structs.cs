// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ZFS filesystem plugin.
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

namespace Aaru.Filesystems;

/*
 * The ZFS on-disk structure is quite undocumented, so this has been checked using several test images and reading the comments and headers (but not the code)
 * of ZFS-On-Linux.
 *
 * The most basic structure, the vdev label, is as follows:
 * 8KiB of blank space
 * 8KiB reserved for boot code, stored as a ZIO block with magic and checksum
 * 112KiB of nvlist, usually encoded using XDR
 * 128KiB of copies of the 1KiB uberblock
 *
 * Two vdev labels, L0 and L1 are stored at the start of the vdev.
 * Another two, L2 and L3 are stored at the end.
 *
 * The nvlist is nothing more than a double linked list of name/value pairs where name is a string and value is an arbitrary type (and can be an array of it).
 * On-disk they are stored sequentially (no pointers) and can be encoded in XDR (an old Sun serialization method that stores everything as 4 bytes chunks) or
 * natively (that is as the host natively stores that values, for example on Intel an extended float would be 10 bytes (80 bit).
 * It can also be encoded little or big endian.
 * Because of this variations, ZFS stored a header indicating the used encoding and endianess before the encoded nvlist.
 */
/// <inheritdoc />
/// <summary>Implements detection for the Zettabyte File System (ZFS)</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedType.Local")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public sealed partial class ZFS
{
#region Nested type: DVA

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct DVA
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly ulong[] word;
    }

#endregion

#region Nested type: NVS_Item

    /// <summary>This represent an encoded nvpair (an item of an nvlist)</summary>
    struct NVS_Item
    {
        /// <summary>Size in bytes when encoded in XDR</summary>
        public uint encodedSize;
        /// <summary>Size in bytes when decoded</summary>
        public uint decodedSize;
        /// <summary>On disk, it is null-padded for alignment to 4 bytes and prepended by a 4 byte length indicator</summary>
        public string name;
        /// <summary>Data type</summary>
        public NVS_DataTypes dataType;
        /// <summary>How many elements are here</summary>
        public uint elements;
        /// <summary>On disk size is relative to <see cref="dataType" /> and <see cref="elements" /> always aligned to 4 bytes</summary>
        public object value;
    }

#endregion

#region Nested type: NVS_Method

    /// <summary>This structure indicates which encoding method and endianness is used to encode the nvlist</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct NVS_Method
    {
        public readonly byte encoding;
        public readonly byte endian;
        public readonly byte reserved1;
        public readonly byte reserved2;
    }

#endregion

#region Nested type: NVS_XDR_Header

    /// <summary>This structure gives information about the encoded nvlist</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct NVS_XDR_Header
    {
        public readonly NVS_Method encodingAndEndian;
        public readonly uint       version;
        public readonly uint       flags;
    }

#endregion

#region Nested type: SPA_BlockPointer

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct SPA_BlockPointer
    {
        /// <summary>Data virtual address</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly DVA[] dataVirtualAddress;
        /// <summary>Block properties</summary>
        public readonly ulong properties;
        /// <summary>Reserved for future expansion</summary>
        public readonly ulong[] padding;
        /// <summary>TXG when block was allocated</summary>
        public readonly ulong birthTxg;
        /// <summary>Transaction group at birth</summary>
        public readonly ulong birth;
        /// <summary>Fill count</summary>
        public readonly ulong fill;
        public readonly ZIO_Checksum checksum;
    }

#endregion

#region Nested type: ZFS_Uberblock

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ZFS_Uberblock
    {
        public readonly ulong            magic;
        public readonly ulong            spaVersion;
        public readonly ulong            lastTxg;
        public readonly ulong            guidSum;
        public readonly ulong            timestamp;
        public readonly SPA_BlockPointer mosPtr;
        public readonly ulong            softwareVersion;
    }

#endregion

#region Nested type: ZIO_Checksum

    struct ZIO_Checksum
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public ulong[] word;
    }

#endregion

#region Nested type: ZIO_Empty

    /// <summary>
    ///     There is an empty ZIO at sector 16 or sector 31, with magic and checksum, to detect it is really ZFS I
    ///     suppose.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ZIO_Empty
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 472)]
        public readonly byte[] empty;
        public readonly ulong        magic;
        public readonly ZIO_Checksum checksum;
    }

#endregion
}