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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;
using Aaru.Decoders.Floppy;

namespace Aaru.Images;

public sealed partial class Cpcdsk
{
#region Nested type: DiskInfo

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct DiskInfo
    {
        /// <summary>Second part of magic, should be "Disk-Info\r\n" in all, but some emulators write spaces instead.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public readonly byte[] magic;
        /// <summary>Creator application (can be null)</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        public readonly byte[] creator;
        /// <summary>Tracks</summary>
        public readonly byte tracks;
        /// <summary>Sides</summary>
        public readonly byte sides;
        /// <summary>
        ///     Size of a track including the 256 bytes header. Unused by extended format, as this format includes a table in
        ///     the next field
        /// </summary>
        public readonly ushort tracksize;
        /// <summary>Size of each track in the extended format. 0 indicates track is not formatted and not present in image.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 204)]
        public readonly byte[] tracksizeTable;
    }

#endregion

#region Nested type: SectorInfo

    /// <summary>Sector information</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct SectorInfo
    {
        /// <summary>Track number from address mark</summary>
        public readonly byte track;
        /// <summary>Side number from address mark</summary>
        public readonly byte side;
        /// <summary>Sector ID from address mark</summary>
        public readonly byte id;
        /// <summary>Sector size from address mark</summary>
        public readonly IBMSectorSizeCode size;
        /// <summary>ST1 register from controller</summary>
        public readonly byte st1;
        /// <summary>ST2 register from controller</summary>
        public readonly byte st2;
        /// <summary>
        ///     Length in bytes of this sector size. If it is bigger than expected sector size, it's a weak sector read
        ///     several times.
        /// </summary>
        public readonly ushort len;
    }

#endregion

#region Nested type: TrackInfo

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct TrackInfo
    {
        /// <summary>Magic number, "Track-Info\r\n"</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public readonly byte[] magic;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly byte[] carriageReturn;
        /// <summary>Padding</summary>
        public readonly uint padding;
        /// <summary>Track number</summary>
        public readonly byte track;
        /// <summary>Side number</summary>
        public readonly byte side;
        /// <summary>Controller data rate</summary>
        public readonly byte dataRate;
        /// <summary>Recording mode</summary>
        public readonly byte recordingMode;
        /// <summary>Bytes per sector</summary>
        public readonly IBMSectorSizeCode bps;
        /// <summary>How many sectors in this track</summary>
        public readonly byte sectors;
        /// <summary>GAP#3</summary>
        public readonly byte gap3;
        /// <summary>Filler</summary>
        public readonly byte filler;
        /// <summary>Information for up to 32 sectors</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly SectorInfo[] sectorsInfo;
    }

#endregion
}