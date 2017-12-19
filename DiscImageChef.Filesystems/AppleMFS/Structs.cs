// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Macintosh File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Apple Macintosh File System structures.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;

namespace DiscImageChef.Filesystems.AppleMFS
{
    // Information from Inside Macintosh Volume II
    public partial class AppleMFS : Filesystem
    {
        /// <summary>
        /// Master Directory Block, should be at offset 0x0400 bytes in volume
        /// </summary>
        struct MFS_MasterDirectoryBlock
        {
            /// <summary>0x000, Signature, 0xD2D7</summary>
            public ushort drSigWord;
            /// <summary>0x002, Volume creation date</summary>
            public uint drCrDate;
            /// <summary>0x006, Volume last backup date</summary>
            public uint drLsBkUp;
            /// <summary>0x00A, Volume attributes</summary>
            public ushort drAtrb;
            /// <summary>0x00C, Volume number of files</summary>
            public ushort drNmFls;
            /// <summary>0x00E, First directory sector</summary>
            public ushort drDirSt;
            /// <summary>0x010, Length of directory in sectors</summary>
            public ushort drBlLen;
            /// <summary>0x012, Volume allocation blocks</summary>
            public ushort drNmAlBlks;
            /// <summary>0x014, Size of allocation blocks</summary>
            public uint drAlBlkSiz;
            /// <summary>0x018, Number of bytes to allocate</summary>
            public uint drClpSiz;
            /// <summary>0x01C, First allocation block in block map</summary>
            public ushort drAlBlSt;
            /// <summary>0x01E. Next unused file number</summary>
            public uint drNxtFNum;
            /// <summary>0x022, Number of unused allocation blocks</summary>
            public ushort drFreeBks;
            /// <summary>0x024, Length of volume name</summary>
            public byte drVNSiz;
            /// <summary>0x025, Characters of volume name</summary>
            public string drVN;
        }

        /// <summary>
        /// Should be at offset 0x0000 in volume, followed by boot code
        /// </summary>
        struct MFS_BootBlock
        {
            /// <summary>0x000, Signature, 0x4C4B if bootable</summary>
            public ushort signature;
            /// <summary>0x002, Branch</summary>
            public uint branch;
            /// <summary>0x006, Boot block flags</summary>
            public byte boot_flags;
            /// <summary>0x007, Boot block version</summary>
            public byte boot_version;
            /// <summary>0x008, Allocate secondary buffers</summary>
            public short sec_sv_pages;
            /// <summary>0x00A, System file name (16 bytes)</summary>
            public string system_name;
            /// <summary>0x01A, Finder file name (16 bytes)</summary>
            public string finder_name;
            /// <summary>0x02A, Debugger file name (16 bytes)</summary>
            public string debug_name;
            /// <summary>0x03A, Disassembler file name (16 bytes)</summary>
            public string disasm_name;
            /// <summary>0x04A, Startup screen file name (16 bytes)</summary>
            public string stupscr_name;
            /// <summary>0x05A, First program to execute on boot (16 bytes)</summary>
            public string bootup_name;
            /// <summary>0x06A, Clipboard file name (16 bytes)</summary>
            public string clipbrd_name;
            /// <summary>0x07A, 1/4 of maximum opened at a time files</summary>
            public ushort max_files;
            /// <summary>0x07C, Event queue size</summary>
            public ushort queue_size;
            /// <summary>0x07E, Heap size on a Mac with 128KiB of RAM</summary>
            public uint heap_128k;
            /// <summary>0x082, Heap size on a Mac with 256KiB of RAM</summary>
            public uint heap_256k;
            /// <summary>0x086, Heap size on a Mac with 512KiB of RAM or more</summary>
            public uint heap_512k;
        }

        [Flags]
        enum MFS_FileFlags : byte
        {
            Locked = 0x01,
            Used = 0x80
        }

        [Flags]
        enum MFS_FinderFlags : ushort
        {
            kIsOnDesk = 0x0001,
            kColor = 0x000E,
            kRequireSwitchLaunch = 0x0020,
            kIsShared = 0x0040,
            kHasNoINITs = 0x0080,
            kHasBeenInited = 0x0100,
            kHasCustomIcon = 0x0400,
            kLetter = 0x0200,
            kChanged = 0x0200,
            kIsStationery = 0x0800,
            kNameLocked = 0x1000,
            kHasBundle = 0x2000,
            kIsInvisible = 0x4000,
            kIsAlias = 0x8000
        }

        struct MFS_Point
        {
            public short x;
            public short y;
        }

        struct MFS_FinderInfo
        {
            public uint fdType;
            public uint fdCreator;
            public MFS_FinderFlags fdFlags;
            public MFS_Point fdLocation;
            public short fdFldr;
        }

        struct MFS_FileEntry
        {
            /// <summary>0x00, Entry flags</summary>
            public MFS_FileFlags flFlags;
            /// <summary>0x01, Version number</summary>
            public byte flTyp;
            /// <summary>0x02, FinderInfo</summary>
            public byte[] flUsrWds;
            /// <summary>0x12, file ID</summary>
            public uint flFlNum;
            /// <summary>0x16, first allocation block of data fork</summary>
            public ushort flStBlk;
            /// <summary>0x18, logical end-of-file of data fork</summary>
            public uint flLgLen;
            /// <summary>0x1C, physical end-of-file of data fork</summary>
            public uint flPyLen;
            /// <summary>0x20, first allocation block of resource fork</summary>
            public ushort flRStBlk;
            /// <summary>0x22, logical end-of-file of resource fork</summary>
            public uint flRLgLen;
            /// <summary>0x26, physical end-of-file of resource fork</summary>
            public uint flRPyLen;
            /// <summary>0x2A, date and time of creation</summary>
            public uint flCrDat;
            /// <summary>0x2E, date and time of last modification</summary>
            public uint flMdDat;
            /// <summary>0x32, file name prefixed with length</summary>
            public byte[] flNam;
        }
    }
}