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
//     Contains structures for Connectix and Microsoft Virtual PC disk images.
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

using System;

namespace Aaru.DiscImages
{
    public sealed partial class Vhd
    {
        struct HardDiskFooter
        {
            /// <summary>Offset 0x00, File magic number, <see cref="Vhd.IMAGE_COOKIE" /></summary>
            public ulong Cookie;
            /// <summary>Offset 0x08, Specific feature support</summary>
            public uint Features;
            /// <summary>Offset 0x0C, File format version</summary>
            public uint Version;
            /// <summary>Offset 0x10, Offset from beginning of file to next structure</summary>
            public ulong Offset;
            /// <summary>Offset 0x18, Creation date seconds since 2000/01/01 00:00:00 UTC</summary>
            public uint Timestamp;
            /// <summary>Offset 0x1C, Application that created this disk image</summary>
            public uint CreatorApplication;
            /// <summary>Offset 0x20, Version of the application that created this disk image</summary>
            public uint CreatorVersion;
            /// <summary>Offset 0x24, Host operating system of the application that created this disk image</summary>
            public uint CreatorHostOs;
            /// <summary>Offset 0x28, Original hard disk size, in bytes</summary>
            public ulong OriginalSize;
            /// <summary>Offset 0x30, Current hard disk size, in bytes</summary>
            public ulong CurrentSize;
            /// <summary>Offset 0x38, CHS geometry Cylinder mask = 0xFFFF0000 Heads mask = 0x0000FF00 Sectors mask = 0x000000FF</summary>
            public uint DiskGeometry;
            /// <summary>Offset 0x3C, Disk image type</summary>
            public uint DiskType;
            /// <summary>Offset 0x40, Checksum for this structure</summary>
            public uint Checksum;
            /// <summary>Offset 0x44, UUID, used to associate parent with differencing disk images</summary>
            public Guid UniqueId;
            /// <summary>Offset 0x54, If set, system is saved, so compaction and expansion cannot be performed</summary>
            public byte SavedState;
            /// <summary>Offset 0x55, 427 bytes reserved, should contain zeros.</summary>
            public byte[] Reserved;
        }

        struct ParentLocatorEntry
        {
            /// <summary>Offset 0x00, Describes the platform specific type this entry belongs to</summary>
            public uint PlatformCode;
            /// <summary>Offset 0x04, Describes the number of 512 bytes sectors used by this entry</summary>
            public uint PlatformDataSpace;
            /// <summary>Offset 0x08, Describes this entry's size in bytes</summary>
            public uint PlatformDataLength;
            /// <summary>Offset 0x0c, Reserved</summary>
            public uint Reserved;
            /// <summary>Offset 0x10, Offset on disk image this entry resides on</summary>
            public ulong PlatformDataOffset;
        }

        struct DynamicDiskHeader
        {
            /// <summary>Offset 0x00, Header magic, <see cref="Vhd.DYNAMIC_COOKIE" /></summary>
            public ulong Cookie;
            /// <summary>Offset 0x08, Offset to next structure on disk image. Currently unused, 0xFFFFFFFF</summary>
            public ulong DataOffset;
            /// <summary>Offset 0x10, Offset of the Block Allocation Table (BAT)</summary>
            public ulong TableOffset;
            /// <summary>Offset 0x18, Version of this header</summary>
            public uint HeaderVersion;
            /// <summary>Offset 0x1C, Maximum entries present in the BAT</summary>
            public uint MaxTableEntries;
            /// <summary>Offset 0x20, Size of a block in bytes Should always be a power of two of 512</summary>
            public uint BlockSize;
            /// <summary>Offset 0x24, Checksum of this header</summary>
            public uint Checksum;
            /// <summary>Offset 0x28, UUID of parent disk image for differencing type</summary>
            public Guid ParentId;
            /// <summary>Offset 0x38, Timestamp of parent disk image</summary>
            public uint ParentTimestamp;
            /// <summary>Offset 0x3C, Reserved</summary>
            public uint Reserved;
            /// <summary>Offset 0x40, 512 bytes UTF-16 of parent disk image filename</summary>
            public string ParentName;
            /// <summary>Offset 0x240, Parent disk image locator entry, <see cref="ParentLocatorEntry" /></summary>
            public ParentLocatorEntry[] LocatorEntries;
            /// <summary>Offset 0x300, 256 reserved bytes</summary>
            public byte[] Reserved2;
        }
    }
}