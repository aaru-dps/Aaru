// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Files-11 On-Disk Structure plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Files-11 On-Disk Structure and shows information.
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

namespace Aaru.Filesystems;

// Information from VMS File System Internals by Kirby McCoy
// ISBN: 1-55558-056-4
// With some hints from http://www.decuslib.com/DECUS/vmslt97b/gnusoftware/gccaxp/7_1/vms/hm2def.h
// Expects the home block to be always in sector #1 (does not check deltas)
// Assumes a sector size of 512 bytes (VMS does on HDDs and optical drives, dunno about M.O.)
// Book only describes ODS-2. Need to test ODS-1 and ODS-5
// There is an ODS with signature "DECFILES11A", yet to be seen
// Time is a 64 bit unsigned integer, tenths of microseconds since 1858/11/17 00:00:00.
// TODO: Implement checksum
/// <inheritdoc />
/// <summary>Implements detection of DEC's On-Disk Structure, aka the ODS filesystem</summary>
public sealed partial class ODS
{
#region Nested type: HomeBlock

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct HomeBlock
    {
        /// <summary>0x000, LBN of THIS home block</summary>
        public readonly uint homelbn;
        /// <summary>0x004, LBN of the secondary home block</summary>
        public readonly uint alhomelbn;
        /// <summary>0x008, LBN of backup INDEXF.SYS;1</summary>
        public readonly uint altidxlbn;
        /// <summary>0x00C, High byte contains filesystem version (1, 2 or 5), low byte contains revision (1)</summary>
        public readonly ushort struclev;
        /// <summary>0x00E, Number of blocks each bit of the volume bitmap represents</summary>
        public readonly ushort cluster;
        /// <summary>0x010, VBN of THIS home block</summary>
        public readonly ushort homevbn;
        /// <summary>0x012, VBN of the secondary home block</summary>
        public readonly ushort alhomevbn;
        /// <summary>0x014, VBN of backup INDEXF.SYS;1</summary>
        public readonly ushort altidxvbn;
        /// <summary>0x016, VBN of the bitmap</summary>
        public readonly ushort ibmapvbn;
        /// <summary>0x018, LBN of the bitmap</summary>
        public readonly uint ibmaplbn;
        /// <summary>0x01C, Max files on volume</summary>
        public readonly uint maxfiles;
        /// <summary>0x020, Bitmap size in sectors</summary>
        public readonly ushort ibmapsize;
        /// <summary>0x022, Reserved files, 5 at minimum</summary>
        public readonly ushort resfiles;
        /// <summary>0x024, Device type, ODS-2 defines it as always 0</summary>
        public readonly ushort devtype;
        /// <summary>0x026, Relative volume number (number of the volume in a set)</summary>
        public readonly ushort rvn;
        /// <summary>0x028, Total number of volumes in the set this volume is</summary>
        public readonly ushort setcount;
        /// <summary>0x02A, Flags</summary>
        public readonly ushort volchar;
        /// <summary>0x02C, User ID of the volume owner</summary>
        public readonly uint volowner;
        /// <summary>0x030, Security mask (??)</summary>
        public readonly uint sec_mask;
        /// <summary>0x034, Volume permissions (system, owner, group and other)</summary>
        public readonly ushort protect;
        /// <summary>0x036, Default file protection, unsupported in ODS-2</summary>
        public readonly ushort fileprot;
        /// <summary>0x038, Default file record protection</summary>
        public readonly ushort recprot;
        /// <summary>0x03A, Checksum of all preceding entries</summary>
        public readonly ushort checksum1;
        /// <summary>0x03C, Creation date</summary>
        public readonly ulong credate;
        /// <summary>0x044, Window size (pointers for the window)</summary>
        public readonly byte window;
        /// <summary>0x045, Directories to be stored in cache</summary>
        public readonly byte lru_lim;
        /// <summary>0x046, Default allocation size in blocks</summary>
        public readonly ushort extend;
        /// <summary>0x048, Minimum file retention period</summary>
        public readonly ulong retainmin;
        /// <summary>0x050, Maximum file retention period</summary>
        public readonly ulong retainmax;
        /// <summary>0x058, Last modification date</summary>
        public readonly ulong revdate;
        /// <summary>0x060, Minimum security class, 20 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly byte[] min_class;
        /// <summary>0x074, Maximum security class, 20 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly byte[] max_class;
        /// <summary>0x088, File lookup table FID</summary>
        public readonly ushort filetab_fid1;
        /// <summary>0x08A, File lookup table FID</summary>
        public readonly ushort filetab_fid2;
        /// <summary>0x08C, File lookup table FID</summary>
        public readonly ushort filetab_fid3;
        /// <summary>0x08E, Lowest structure level on the volume</summary>
        public readonly ushort lowstruclev;
        /// <summary>0x090, Highest structure level on the volume</summary>
        public readonly ushort highstruclev;
        /// <summary>0x092, Volume copy date (??)</summary>
        public readonly ulong copydate;
        /// <summary>0x09A, 302 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 302)]
        public readonly byte[] reserved1;
        /// <summary>0x1C8, Physical drive serial number</summary>
        public readonly uint serialnum;
        /// <summary>0x1CC, Name of the volume set, 12 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] strucname;
        /// <summary>0x1D8, Volume label, 12 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] volname;
        /// <summary>0x1E4, Name of the volume owner, 12 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] ownername;
        /// <summary>0x1F0, ODS-2 defines it as "DECFILE11B", 12 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] format;
        /// <summary>0x1FC, Reserved</summary>
        public readonly ushort reserved2;
        /// <summary>0x1FE, Checksum of preceding 255 words (16 bit units)</summary>
        public readonly ushort checksum2;
    }

#endregion
}