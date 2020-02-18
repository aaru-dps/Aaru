// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : AppleHFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Hierarchical File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Apple Hierarchical File System and shows information.
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

namespace DiscImageChef.Filesystems
{
    // Information from Inside Macintosh
    // https://developer.apple.com/legacy/library/documentation/mac/pdf/Files/File_Manager.pdf
    public partial class AppleHFS
    {
        /// <summary>Should be sectors 0 and 1 in volume, followed by boot code</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HfsBootBlock // Should be sectors 0 and 1 in volume
        {
            /// <summary>0x000, Signature, 0x4C4B if bootable</summary>
            public readonly ushort bbID;
            /// <summary>0x002, Branch</summary>
            public readonly uint bbEntry;
            /// <summary>0x007, Boot block version</summary>
            public readonly ushort bbVersion;
            /// <summary>0x006, Boot block flags</summary>
            public readonly short bbPageFlags;
            /// <summary>0x00A, System file name (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] bbSysName;
            /// <summary>0x01A, Finder file name (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] bbShellName;
            /// <summary>0x02A, Debugger file name (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] bbDbg1Name;
            /// <summary>0x03A, Disassembler file name (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] bbDbg2Name;
            /// <summary>0x04A, Startup screen file name (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] bbScreenName;
            /// <summary>0x05A, First program to execute on boot (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] bbHelloName;
            /// <summary>0x06A, Clipboard file name (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] bbScrapName;
            /// <summary>0x07A, 1/4 of maximum opened at a time files</summary>
            public readonly ushort bbCntFCBs;
            /// <summary>0x07C, Event queue size</summary>
            public readonly ushort bbCntEvts;
            /// <summary>0x07E, Heap size on a Mac with 128KiB of RAM</summary>
            public readonly uint bb128KSHeap;
            /// <summary>0x082, Heap size on a Mac with 256KiB of RAM</summary>
            public readonly uint bb256KSHeap;
            /// <summary>0x086, Heap size on a Mac with 512KiB of RAM or more</summary>
            public readonly uint bbSysHeapSize;
            /// <summary>Padding</summary>
            public readonly ushort filler;
            /// <summary>Additional system heap space</summary>
            public readonly uint bbSysHeapExtra;
            /// <summary>Fraction of RAM for system heap</summary>
            public readonly uint bbSysHeapFract;
        }

        /// <summary>Master Directory Block, should be sector 2 in volume</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HfsMasterDirectoryBlock // Should be sector 2 in volume
        {
            /// <summary>0x000, Signature, 0x4244</summary>
            public readonly ushort drSigWord;
            /// <summary>0x002, Volume creation date</summary>
            public readonly uint drCrDate;
            /// <summary>0x006, Volume last modification date</summary>
            public readonly uint drLsMod;
            /// <summary>0x00A, Volume attributes</summary>
            public readonly ushort drAtrb;
            /// <summary>0x00C, Files in root directory</summary>
            public readonly ushort drNmFls;
            /// <summary>0x00E, Start 512-byte sector of volume bitmap</summary>
            public readonly ushort drVBMSt;
            /// <summary>0x010, Allocation block to begin next allocation</summary>
            public readonly ushort drAllocPtr;
            /// <summary>0x012, Allocation blocks</summary>
            public readonly ushort drNmAlBlks;
            /// <summary>0x014, Bytes per allocation block</summary>
            public readonly uint drAlBlkSiz;
            /// <summary>0x018, Bytes to allocate when extending a file</summary>
            public readonly uint drClpSiz;
            /// <summary>0x01C, Start 512-byte sector of first allocation block</summary>
            public readonly ushort drAlBlSt;
            /// <summary>0x01E, CNID for next file</summary>
            public readonly uint drNxtCNID;
            /// <summary>0x022, Free allocation blocks</summary>
            public readonly ushort drFreeBks;
            /// <summary>0x024, Volume name (28 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
            public readonly byte[] drVN;
            /// <summary>0x040, Volume last backup time</summary>
            public readonly uint drVolBkUp;
            /// <summary>0x044, Volume backup sequence number</summary>
            public readonly ushort drVSeqNum;
            /// <summary>0x046, Filesystem write count</summary>
            public readonly uint drWrCnt;
            /// <summary>0x04A, Bytes to allocate when extending the extents B-Tree</summary>
            public readonly uint drXTClpSiz;
            /// <summary>0x04E, Bytes to allocate when extending the catalog B-Tree</summary>
            public readonly uint drCTClpSiz;
            /// <summary>0x052, Number of directories in root directory</summary>
            public readonly ushort drNmRtDirs;
            /// <summary>0x054, Number of files in the volume</summary>
            public readonly uint drFilCnt;
            /// <summary>0x058, Number of directories in the volume</summary>
            public readonly uint drDirCnt;
            /// <summary>0x05C, finderInfo[0], CNID for bootable system's directory</summary>
            public readonly uint drFndrInfo0;
            /// <summary>0x060, finderInfo[1], CNID of the directory containing the boot application</summary>
            public readonly uint drFndrInfo1;
            /// <summary>0x064, finderInfo[2], CNID of the directory that should be opened on boot</summary>
            public readonly uint drFndrInfo2;
            /// <summary>0x068, finderInfo[3], CNID for Mac OS 8 or 9 directory</summary>
            public readonly uint drFndrInfo3;
            /// <summary>0x06C, finderInfo[4], Reserved</summary>
            public readonly uint drFndrInfo4;
            /// <summary>0x070, finderInfo[5], CNID for Mac OS X directory</summary>
            public readonly uint drFndrInfo5;
            /// <summary>0x074, finderInfo[6], first part of Mac OS X volume ID</summary>
            public readonly uint drFndrInfo6;
            /// <summary>0x078, finderInfo[7], second part of Mac OS X volume ID</summary>
            public readonly uint drFndrInfo7;

            // If wrapping HFS+
            /// <summary>0x07C, Embedded volume signature, "H+" if HFS+ is embedded ignore following two fields if not</summary>
            public readonly ushort drEmbedSigWord;
            /// <summary>0x07E, Starting block number of embedded HFS+ volume</summary>
            public readonly ushort xdrStABNt;
            /// <summary>0x080, Allocation blocks used by embedded volume</summary>
            public readonly ushort xdrNumABlks;

            // If not
            /// <summary>0x07C, Size in blocks of volume cache</summary>
            public readonly ushort drVCSize;
            /// <summary>0x07E, Size in blocks of volume bitmap cache</summary>
            public readonly ushort drVBMCSize;
            /// <summary>0x080, Size in blocks of volume common cache</summary>
            public readonly ushort drCtlCSize;

            // End of variable variables :D
            /// <summary>0x082, Bytes in the extents B-Tree 3 HFS extents following, 32 bits each</summary>
            public readonly uint drXTFlSize;
            /// <summary>0x092, Bytes in the catalog B-Tree 3 HFS extents following, 32 bits each</summary>
            public readonly uint drCTFlSize;
        }
    }
}