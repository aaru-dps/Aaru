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
//     Contains structures for Sydex TeleDisk disk images.
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

public sealed partial class TeleDisk
{
#region Nested type: CommentBlockHeader

    struct CommentBlockHeader
    {
        /// <summary>CRC of comment block after crc field</summary>
        public ushort Crc;
        /// <summary>Length of comment</summary>
        public ushort Length;
        public byte Year;
        public byte Month;
        public byte Day;
        public byte Hour;
        public byte Minute;
        public byte Second;
    }

#endregion

#region Nested type: DataHeader

    struct DataHeader
    {
        /// <summary>Size of all data (encoded) + next field (1)</summary>
        public ushort DataSize;
        /// <summary>Encoding used for data block</summary>
        public byte DataEncoding;
    }

#endregion

#region Nested type: Header

    struct Header
    {
        /// <summary>"TD" or "td" depending on compression</summary>
        public ushort Signature;
        /// <summary>Sequence, but TeleDisk seems to complaing if != 0</summary>
        public byte Sequence;
        /// <summary>Random, same byte for all disks in the same set</summary>
        public byte DiskSet;
        /// <summary>TeleDisk version, major in high nibble, minor in low nibble</summary>
        public byte Version;
        /// <summary>Data rate</summary>
        public byte DataRate;
        /// <summary>BIOS drive type</summary>
        public byte DriveType;
        /// <summary>Stepping used</summary>
        public byte Stepping;
        /// <summary>If set means image only allocates sectors marked in-use by FAT12</summary>
        public byte DosAllocation;
        /// <summary>Sides of disk</summary>
        public byte Sides;
        /// <summary>CRC of all the previous</summary>
        public ushort Crc;
    }

#endregion

#region Nested type: SectorHeader

    struct SectorHeader
    {
        /// <summary>Cylinder as stored on sector address mark</summary>
        public byte Cylinder;
        /// <summary>Head as stored on sector address mark</summary>
        public byte Head;
        /// <summary>Sector number as stored on sector address mark</summary>
        public byte SectorNumber;
        /// <summary>Sector size</summary>
        public byte SectorSize;
        /// <summary>Sector flags</summary>
        public byte Flags;
        /// <summary>Lower byte of TeleDisk CRC of sector header, data header and data block</summary>
        public byte Crc;
    }

#endregion

#region Nested type: TrackHeader

    struct TrackHeader
    {
        /// <summary>Sectors in the track, 0xFF if end of disk image (there is no spoon)</summary>
        public byte Sectors;
        /// <summary>Cylinder the head was on</summary>
        public byte Cylinder;
        /// <summary>Head/side used</summary>
        public byte Head;
        /// <summary>Lower byte of CRC of previous fields</summary>
        public byte Crc;
    }

#endregion
}