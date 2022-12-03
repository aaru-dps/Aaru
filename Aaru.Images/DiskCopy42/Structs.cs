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
//     Contains structures for Apple DiskCopy 4.2 disk images.
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

namespace Aaru.DiscImages;

public sealed partial class DiskCopy42
{
    // DiskCopy 4.2 header, big-endian, data-fork, start of file, 84 bytes
    struct Header
    {
        /// <summary>0x00, 64 bytes, pascal string, disk name or "-not a Macintosh disk-", filled with garbage</summary>
        public string DiskName;
        /// <summary>0x40, size of data in bytes (usually sectors*512)</summary>
        public uint DataSize;
        /// <summary>0x44, size of tags in bytes (usually sectors*12)</summary>
        public uint TagSize;
        /// <summary>0x48, checksum of data bytes</summary>
        public uint DataChecksum;
        /// <summary>0x4C, checksum of tag bytes</summary>
        public uint TagChecksum;
        /// <summary>0x50, format of disk, see constants</summary>
        public byte Format;
        /// <summary>0x51, format of sectors, see constants</summary>
        public byte FmtByte;
        /// <summary>0x52, is disk image valid? always 0x01</summary>
        public byte Valid;
        /// <summary>0x53, reserved, always 0x00</summary>
        public byte Reserved;
    }
}