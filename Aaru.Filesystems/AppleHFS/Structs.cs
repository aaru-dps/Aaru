// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Hierarchical File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Apple Hierarchical File System structures.
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

// ReSharper disable UnusedType.Local

// ReSharper disable IdentifierTypo
// ReSharper disable MemberCanBePrivate.Local

using System.Runtime.InteropServices;

namespace Aaru.Filesystems;

// Information from Inside Macintosh
// https://developer.apple.com/legacy/library/documentation/mac/pdf/Files/File_Manager.pdf
public sealed partial class AppleHFS
{
    /// <summary>Master Directory Block, should be sector 2 in volume</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct MasterDirectoryBlock // Should be sector 2 in volume
    {
        /// <summary>0x000, Signature, 0x4244</summary>
        public readonly ushort drSigWord;
        /// <summary>0x002, Volume creation date</summary>
        public readonly uint drCrDate;
        /// <summary>0x006, Volume last modification date</summary>
        public readonly uint drLsMod;
        /// <summary>0x00A, Volume attributes</summary>
        public readonly AppleCommon.VolumeAttributes drAtrb;
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

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct NodeDescriptor
    {
        /// <summary>A link to the next node of this type, or <c>null</c> if this is the last one.</summary>
        public readonly uint ndFLink;
        /// <summary>A link to the previous node of this type, or <c>null</c> if this is the first one.</summary>
        public readonly uint ndBLink;
        /// <summary>The type of this node.</summary>
        public readonly NodeType ndType;
        /// <summary>The depth of this node in the B*-tree hierarchy. Maximum depth is apparently 8.</summary>
        public readonly sbyte ndNHeight;
        /// <summary>The number of records contained in this node.</summary>
        public readonly ushort ndNRecs;
        /// <summary>Reserved, should be 0.</summary>
        public readonly ushort ndResv2;
    }

    /// <summary>B*-tree header</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct BTHdrRed
    {
        /// <summary>Current depth of tree.</summary>
        public readonly ushort bthDepth;
        /// <summary>Number of root node.</summary>
        public readonly uint bthRoot;
        /// <summary>Number of leaf records in tree.</summary>
        public readonly uint bthNRecs;
        /// <summary>Number of first leaf node.</summary>
        public readonly uint bthFNode;
        /// <summary>Number of last leaf node.</summary>
        public readonly uint bthLNode;
        /// <summary>Size of a node.</summary>
        public readonly ushort bthNodeSize;
        /// <summary>Maximum length of a key.</summary>
        public readonly ushort bthKeyLen;
        /// <summary>Total number of nodes in tree.</summary>
        public readonly uint bthNNodes;
        /// <summary>Number of free nodes.</summary>
        public readonly uint bthFree;
        /// <summary>Reserved</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 76)]
        public readonly sbyte[] bthResv;
    }

    /// <summary>Catalog key record</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct CatKeyRec
    {
        /// <summary>Key length.</summary>
        public readonly sbyte ckrKeyLen;
        /// <summary>Reserved.</summary>
        public readonly sbyte ckrResrv1;
        /// <summary>Parent directory ID.</summary>
        public readonly uint ckrParID;
        /// <summary>Catalog node name. Full 32 bytes in index nodes but only the needed bytes, padded to word, in leaf nodes.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] ckrCName;
    }

    /// <summary>Catalog data record header</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct CatDataRec
    {
        public readonly CatDataType cdrType;
        public readonly sbyte       cdrResvr2;
    }

    /// <summary>Directory record</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct CdrDirRec
    {
        public readonly CatDataRec dirHdr;
        /// <summary>Directory flags.</summary>
        public readonly ushort dirFlags;
        /// <summary>Directory valence.</summary>
        public readonly ushort dirVal;
        /// <summary>Directory ID.</summary>
        public readonly uint dirDirID;
        /// <summary>Date and time of creation.</summary>
        public readonly uint dirCrDat;
        /// <summary>Date and time of last modification.</summary>
        public readonly uint dirMdDat;
        /// <summary>Date and time of last backup.</summary>
        public readonly uint dirBkDat;
        /// <summary>Finder information.</summary>
        public readonly AppleCommon.DInfo dirUsrInfo;
        /// <summary>Additional Finder information.</summary>
        public readonly AppleCommon.DXInfo dirFndrInfo;
        /// <summary>Reserved</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly uint[] dirResrv;
    }

    /// <summary>File record</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct CdrFilRec
    {
        public readonly CatDataRec filHdr;
        /// <summary>File flags.</summary>
        public readonly sbyte filFlags;
        /// <summary>File type.</summary>
        public readonly sbyte filType;
        /// <summary>Finder information.</summary>
        public readonly AppleCommon.FInfo filUsrWds;
        /// <summary>File ID.</summary>
        public readonly uint filFlNum;
        /// <summary>First allocation block of data fork.</summary>
        public readonly ushort filStBlk;
        /// <summary>Logical EOF of data fork.</summary>
        public readonly uint filLgLen;
        /// <summary>Physical EOF of data fork.</summary>
        public readonly uint filPyLen;
        /// <summary>First allocation block of resource fork.</summary>
        public readonly ushort filRStBlk;
        /// <summary>Logical EOF of resource fork.</summary>
        public readonly uint filRLgLen;
        /// <summary>Physical EOF of resource fork.</summary>
        public readonly uint filRPyLen;
        /// <summary>Date and time of creation.</summary>
        public readonly uint filCrDat;
        /// <summary>Date and time of last modification.</summary>
        public readonly uint filMdDat;
        /// <summary>Date and time of last backup.</summary>
        public readonly uint filBkDat;
        /// <summary>Additional Finder information.</summary>
        public readonly AppleCommon.FXInfo filFndrInfo;
        /// <summary>File clump size.</summary>
        public readonly ushort filClpSize;
        /// <summary>First data fork extent record.</summary>
        public readonly ExtDataRec filExtRec;
        /// <summary>First resource fork extent record.</summary>
        public readonly ExtDataRec filRExtRec;
        /// <summary>Reserved</summary>
        public readonly uint filResrv;
    }

    /// <summary>Directory thread record</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct CdrThdRec
    {
        public readonly CatDataRec thdHdr;
        /// <summary>Reserved.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly uint[] thdResrv;
        /// <summary>Parent ID for this directory.</summary>
        public readonly uint thdParID;
        /// <summary>Name of this directory.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] thdCName;
    }

    /// <summary>File thread record</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct CdrFThdRec
    {
        public readonly CatDataRec fthdHdr;
        /// <summary>Reserved.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly uint[] fthdResrv;
        /// <summary>Parent ID for this file.</summary>
        public readonly uint fthdParID;
        /// <summary>Name of this file.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] fthdCName;
    }

    /// <summary>Extent descriptor</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct ExtDescriptor
    {
        /// <summary>First allocation block</summary>
        public readonly ushort xdrStABN;
        /// <summary>Number of allocation blocks</summary>
        public readonly ushort xdrNumABlks;
    }

    /// <summary>Extent data record</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct ExtDataRec
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly ExtDescriptor[] xdr;
    }

    /// <summary>Extent key record</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct ExtKeyRec
    {
        /// <summary>Key length.</summary>
        public readonly sbyte xkrKeyLen;
        /// <summary>Fork type.</summary>
        public readonly ForkType xkrFkType;
        /// <summary>File number.</summary>
        public readonly uint xkrFNum;
        /// <summary>Starting file allocation block.</summary>
        public readonly ushort xkrFABN;
    }
}