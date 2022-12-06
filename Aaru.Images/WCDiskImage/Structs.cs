// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Michael Drüing <michael@drueing.de>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains structures for d2f disk images.
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
// Copyright © 2018-2023 Michael Drüing
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;

namespace Aaru.DiscImages
{
    public sealed partial class WCDiskImage
    {
        /// <summary>The expected signature of a proper image file.</summary>
        const string FILE_SIGNATURE = "WC DISK IMAGE\x1a\x1a";

        /// <summary>The global header of a WCDiskImage file</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct FileHeader
        {
            /// <summary>The signature should be "WC DISK IMAGE\0x1a\0x1a\0x00"</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] signature;

            /// <summary>Version (currently only version 0 and 1 is known)</summary>
            public readonly byte version;

            /// <summary>The number of heads. Only 1 or 2 is supported</summary>
            public readonly byte heads;

            /// <summary>Sectors per track, maximum is 18</summary>
            public readonly byte sectorsPerTrack;

            /// <summary>The number of tracks/cylinders. 80 is the maximum</summary>
            public readonly byte cylinders;

            /// <summary>
            ///     The "extra tracks" that are present. What this means is that the tracks 81 and 82 might contain data (on an
            ///     80-track disk) and that this data was dumped too. The order is head0extra1, head1extra1, head0extra2, head1extra2.
            ///     1 means track is present.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public readonly byte[] extraTracks;

            /// <summary>Additional metadata present flags.</summary>
            public readonly ExtraFlag extraFlags;

            /// <summary>Padding to make the header 32 bytes.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            public readonly byte[] reserved;
        }

        enum ExtraFlag : byte
        {
            /// <summary>Set if a Comment is present after the image</summary>
            Comment = 0x01,

            /// <summary>Set if a directory listing is present after the image</summary>
            Directory = 0x02
        }

        /// <summary>The Sector header that precedes each sector</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct SectorHeader
        {
            /// <summary>The sector flag (i.e. type)</summary>
            public readonly SectorFlag flag;

            /// <summary>The head this sector belongs to.</summary>
            public readonly byte head;

            /// <summary>The sector number within the track. Must be consecutive.</summary>
            public readonly byte sector;

            /// <summary>The cylinder number this sector belongs to.</summary>
            public readonly byte cylinder;

            /// <summary>A simple CRC16 over the data, to detect errors.</summary>
            public readonly short crc;
        }

        enum SectorFlag : byte
        {
            /// <summary>A normal sector</summary>
            Normal = 0x00,

            /// <summary>A bad sector that could not be read</summary>
            BadSector = 0x01,

            /// <summary>A sector filled with a repeating byte value. The value is encoded in the LSB of the <c>crc</c> field.</summary>
            RepeatByte = 0x02,

            /// <summary>
            ///     Not a sector but a comment. Must come after all user data. The <c>crc</c> field is the length of the comment
            ///     data.
            /// </summary>
            Comment = 0x03,

            /// <summary>Not a sector but the directory information. The <c>crc</c> field is the length of the data.</summary>
            Directory = 0x04
        }
    }
}