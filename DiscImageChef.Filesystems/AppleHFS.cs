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

using System;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using Schemas;
using Marshal = DiscImageChef.Helpers.Marshal;

namespace DiscImageChef.Filesystems
{
    // Information from Inside Macintosh
    // https://developer.apple.com/legacy/library/documentation/mac/pdf/Files/File_Manager.pdf
    public class AppleHFS : IFilesystem
    {
        /// <summary>
        ///     "BD", HFS magic
        /// </summary>
        const ushort HFS_MAGIC = 0x4244;
        /// <summary>
        ///     "H+", HFS+ magic
        /// </summary>
        const ushort HFSP_MAGIC = 0x482B;
        /// <summary>
        ///     "LK", HFS bootblock magic
        /// </summary>
        const ushort HFSBB_MAGIC = 0x4C4B;

        public FileSystemType XmlFsType { get; private set; }
        public Encoding       Encoding  { get; private set; }
        public string         Name      => "Apple Hierarchical File System";
        public Guid           Id        => new Guid("36405F8D-0D26-6ECC-0BBB-1D5225FF404F");
        public string         Author    => "Natalia Portillo";

        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(2 + partition.Start >= partition.End) return false;

            byte[] mdbSector;
            ushort drSigWord;

            if(imagePlugin.Info.SectorSize == 2352 || imagePlugin.Info.SectorSize == 2448 ||
               imagePlugin.Info.SectorSize == 2048)
            {
                mdbSector = imagePlugin.ReadSectors(partition.Start, 2);

                foreach(int offset in new[] {0, 0x200, 0x400, 0x600, 0x800, 0xA00})
                {
                    drSigWord = BigEndianBitConverter.ToUInt16(mdbSector, offset);
                    if(drSigWord != HFS_MAGIC) continue;

                    drSigWord =
                        BigEndianBitConverter.ToUInt16(mdbSector, offset + 0x7C); // Seek to embedded HFS+ signature

                    return drSigWord != HFSP_MAGIC;
                }
            }
            else
            {
                mdbSector = imagePlugin.ReadSector(2 + partition.Start);
                drSigWord = BigEndianBitConverter.ToUInt16(mdbSector, 0);

                if(drSigWord != HFS_MAGIC) return false;

                drSigWord = BigEndianBitConverter.ToUInt16(mdbSector, 0x7C); // Seek to embedded HFS+ signature

                return drSigWord != HFSP_MAGIC;
            }

            return false;
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding    encoding)
        {
            Encoding    = encoding ?? Encoding.GetEncoding("macintosh");
            information = "";

            StringBuilder sb = new StringBuilder();

            byte[] bbSector  = null;
            byte[] mdbSector = null;
            ushort drSigWord;

            bool apmFromHddOnCd = false;

            if(imagePlugin.Info.SectorSize == 2352 || imagePlugin.Info.SectorSize == 2448 ||
               imagePlugin.Info.SectorSize == 2048)
            {
                byte[] tmpSector = imagePlugin.ReadSectors(partition.Start, 2);

                foreach(int offset in new[] {0, 0x200, 0x400, 0x600, 0x800, 0xA00})
                {
                    drSigWord = BigEndianBitConverter.ToUInt16(tmpSector, offset);
                    if(drSigWord != HFS_MAGIC) continue;

                    bbSector  = new byte[1024];
                    mdbSector = new byte[512];
                    if(offset >= 0x400) Array.Copy(tmpSector, offset - 0x400, bbSector, 0, 1024);
                    Array.Copy(tmpSector, offset, mdbSector, 0, 512);
                    apmFromHddOnCd = true;
                    break;
                }

                if(!apmFromHddOnCd) return;
            }
            else
            {
                mdbSector = imagePlugin.ReadSector(2 + partition.Start);
                drSigWord = BigEndianBitConverter.ToUInt16(mdbSector, 0);

                if(drSigWord == HFS_MAGIC) bbSector = imagePlugin.ReadSector(partition.Start);
                else return;
            }

            HfsMasterDirectoryBlock mdb = Marshal.ByteArrayToStructureBigEndian<HfsMasterDirectoryBlock>(mdbSector);
            HfsBootBlock            bb  = Marshal.ByteArrayToStructureBigEndian<HfsBootBlock>(bbSector);

            sb.AppendLine("Apple Hierarchical File System");
            sb.AppendLine();
            if(apmFromHddOnCd)
                sb.AppendLine("HFS uses 512 bytes/sector while device uses 2048 bytes/sector.").AppendLine();
            sb.AppendLine("Master Directory Block:");
            sb.AppendFormat("Creation date: {0}", DateHandlers.MacToDateTime(mdb.drCrDate)).AppendLine();
            sb.AppendFormat("Last modification date: {0}", DateHandlers.MacToDateTime(mdb.drLsMod)).AppendLine();
            if(mdb.drVolBkUp > 0)
            {
                sb.AppendFormat("Last backup date: {0}", DateHandlers.MacToDateTime(mdb.drVolBkUp)).AppendLine();
                sb.AppendFormat("Backup sequence number: {0}", mdb.drVSeqNum).AppendLine();
            }
            else sb.AppendLine("Volume has never been backed up");

            if((mdb.drAtrb & 0x80) == 0x80) sb.AppendLine("Volume is locked by hardware.");
            sb.AppendLine((mdb.drAtrb & 0x100) == 0x100 ? "Volume was unmonted." : "Volume is mounted.");
            if((mdb.drAtrb & 0x200)  == 0x200) sb.AppendLine("Volume has spared bad blocks.");
            if((mdb.drAtrb & 0x400)  == 0x400) sb.AppendLine("Volume does not need cache.");
            if((mdb.drAtrb & 0x800)  == 0x800) sb.AppendLine("Boot volume is inconsistent.");
            if((mdb.drAtrb & 0x1000) == 0x1000) sb.AppendLine("There are reused CNIDs.");
            if((mdb.drAtrb & 0x2000) == 0x2000) sb.AppendLine("Volume is journaled.");
            if((mdb.drAtrb & 0x4000) == 0x4000) sb.AppendLine("Volume is seriously inconsistent.");
            if((mdb.drAtrb & 0x8000) == 0x8000) sb.AppendLine("Volume is locked by software.");

            sb.AppendFormat("{0} files on root directory", mdb.drNmFls).AppendLine();
            sb.AppendFormat("{0} directories on root directory", mdb.drNmRtDirs).AppendLine();
            sb.AppendFormat("{0} files on volume", mdb.drFilCnt).AppendLine();
            sb.AppendFormat("{0} directories on volume", mdb.drDirCnt).AppendLine();
            sb.AppendFormat("Volume write count: {0}", mdb.drWrCnt).AppendLine();

            sb.AppendFormat("Volume bitmap starting sector (in 512-bytes): {0}", mdb.drVBMSt).AppendLine();
            sb.AppendFormat("Next allocation block: {0}.", mdb.drAllocPtr).AppendLine();
            sb.AppendFormat("{0} volume allocation blocks.", mdb.drNmAlBlks).AppendLine();
            sb.AppendFormat("{0} bytes per allocation block.", mdb.drAlBlkSiz).AppendLine();
            sb.AppendFormat("{0} bytes to allocate when extending a file.", mdb.drClpSiz).AppendLine();
            sb.AppendFormat("{0} bytes to allocate when extending a Extents B-Tree.", mdb.drXTClpSiz).AppendLine();
            sb.AppendFormat("{0} bytes to allocate when extending a Catalog B-Tree.", mdb.drCTClpSiz).AppendLine();
            sb.AppendFormat("Sector of first allocation block: {0}", mdb.drAlBlSt).AppendLine();
            sb.AppendFormat("Next unused CNID: {0}", mdb.drNxtCNID).AppendLine();
            sb.AppendFormat("{0} unused allocation blocks.", mdb.drFreeBks).AppendLine();

            sb.AppendFormat("{0} bytes in the Extents B-Tree", mdb.drXTFlSize).AppendLine();
            sb.AppendFormat("{0} bytes in the Catalog B-Tree", mdb.drCTFlSize).AppendLine();

            sb.AppendFormat("Volume name: {0}", StringHandlers.PascalToString(mdb.drVN, Encoding)).AppendLine();

            sb.AppendLine("Finder info:");
            sb.AppendFormat("CNID of bootable system's directory: {0}", mdb.drFndrInfo0).AppendLine();
            sb.AppendFormat("CNID of first-run application's directory: {0}", mdb.drFndrInfo1).AppendLine();
            sb.AppendFormat("CNID of previously opened directory: {0}", mdb.drFndrInfo2).AppendLine();
            sb.AppendFormat("CNID of bootable Mac OS 8 or 9 directory: {0}", mdb.drFndrInfo3).AppendLine();
            sb.AppendFormat("CNID of bootable Mac OS X directory: {0}", mdb.drFndrInfo5).AppendLine();
            if(mdb.drFndrInfo6 != 0 && mdb.drFndrInfo7 != 0)
                sb.AppendFormat("Mac OS X Volume ID: {0:X8}{1:X8}", mdb.drFndrInfo6, mdb.drFndrInfo7).AppendLine();

            if(mdb.drEmbedSigWord == HFSP_MAGIC)
            {
                sb.AppendLine("Volume wraps a HFS+ volume.");
                sb.AppendFormat("Starting block of the HFS+ volume: {0}", mdb.xdrStABNt).AppendLine();
                sb.AppendFormat("Allocations blocks of the HFS+ volume: {0}", mdb.xdrNumABlks).AppendLine();
            }
            else
            {
                sb.AppendFormat("{0} blocks in volume cache", mdb.drVCSize).AppendLine();
                sb.AppendFormat("{0} blocks in volume bitmap cache", mdb.drVBMCSize).AppendLine();
                sb.AppendFormat("{0} blocks in volume common cache", mdb.drCtlCSize).AppendLine();
            }

            if(bb.signature == HFSBB_MAGIC)
            {
                sb.AppendLine("Volume is bootable.");
                sb.AppendLine();
                sb.AppendLine("Boot Block:");
                if((bb.boot_flags & 0x40) == 0x40) sb.AppendLine("Boot block should be executed.");
                if((bb.boot_flags & 0x80) == 0x80) sb.AppendLine("Boot block is in new unknown format.");
                else
                {
                    if(bb.boot_flags      > 0) sb.AppendLine("Allocate secondary sound buffer at boot.");
                    else if(bb.boot_flags < 0) sb.AppendLine("Allocate secondary sound and video buffers at boot.");

                    sb.AppendFormat("System filename: {0}", StringHandlers.PascalToString(bb.system_name, Encoding))
                      .AppendLine();
                    sb.AppendFormat("Finder filename: {0}", StringHandlers.PascalToString(bb.finder_name, Encoding))
                      .AppendLine();
                    sb.AppendFormat("Debugger filename: {0}", StringHandlers.PascalToString(bb.debug_name, Encoding))
                      .AppendLine();
                    sb.AppendFormat("Disassembler filename: {0}",
                                    StringHandlers.PascalToString(bb.disasm_name, Encoding)).AppendLine();
                    sb.AppendFormat("Startup screen filename: {0}",
                                    StringHandlers.PascalToString(bb.stupscr_name, Encoding)).AppendLine();
                    sb.AppendFormat("First program to execute at boot: {0}",
                                    StringHandlers.PascalToString(bb.bootup_name, Encoding)).AppendLine();
                    sb.AppendFormat("Clipboard filename: {0}", StringHandlers.PascalToString(bb.clipbrd_name, Encoding))
                      .AppendLine();
                    sb.AppendFormat("Maximum opened files: {0}", bb.max_files * 4).AppendLine();
                    sb.AppendFormat("Event queue size: {0}", bb.queue_size).AppendLine();
                    sb.AppendFormat("Heap size with 128KiB of RAM: {0} bytes", bb.heap_128k).AppendLine();
                    sb.AppendFormat("Heap size with 256KiB of RAM: {0} bytes", bb.heap_256k).AppendLine();
                    sb.AppendFormat("Heap size with 512KiB of RAM or more: {0} bytes", bb.heap_512k).AppendLine();
                }
            }
            else if(mdb.drFndrInfo0 != 0 || mdb.drFndrInfo3 != 0 || mdb.drFndrInfo5 != 0)
                sb.AppendLine("Volume is bootable.");
            else sb.AppendLine("Volume is not bootable.");

            information = sb.ToString();

            XmlFsType = new FileSystemType();
            if(mdb.drVolBkUp > 0)
            {
                XmlFsType.BackupDate          = DateHandlers.MacToDateTime(mdb.drVolBkUp);
                XmlFsType.BackupDateSpecified = true;
            }

            XmlFsType.Bootable = bb.signature    == HFSBB_MAGIC || mdb.drFndrInfo0 != 0 || mdb.drFndrInfo3 != 0 ||
                                 mdb.drFndrInfo5 != 0;
            XmlFsType.Clusters    = mdb.drNmAlBlks;
            XmlFsType.ClusterSize = mdb.drAlBlkSiz;
            if(mdb.drCrDate > 0)
            {
                XmlFsType.CreationDate          = DateHandlers.MacToDateTime(mdb.drCrDate);
                XmlFsType.CreationDateSpecified = true;
            }

            XmlFsType.Dirty                 = (mdb.drAtrb & 0x100) != 0x100;
            XmlFsType.Files                 = mdb.drFilCnt;
            XmlFsType.FilesSpecified        = true;
            XmlFsType.FreeClusters          = mdb.drFreeBks;
            XmlFsType.FreeClustersSpecified = true;
            if(mdb.drLsMod > 0)
            {
                XmlFsType.ModificationDate          = DateHandlers.MacToDateTime(mdb.drLsMod);
                XmlFsType.ModificationDateSpecified = true;
            }

            XmlFsType.Type       = "HFS";
            XmlFsType.VolumeName = StringHandlers.PascalToString(mdb.drVN, Encoding);
            if(mdb.drFndrInfo6 != 0 && mdb.drFndrInfo7 != 0)
                XmlFsType.VolumeSerial = $"{mdb.drFndrInfo6:X8}{mdb.drFndrInfo7:X8}";
        }

        static byte[] Read2048SectorAs512(IMediaImage imagePlugin, ulong lba)
        {
            ulong lba2K     = lba / 4;
            int   remainder = (int)(lba % 4);

            byte[] buffer = imagePlugin.ReadSector(lba2K);
            byte[] sector = new byte[512];

            Array.Copy(buffer, remainder * 512, sector, 0, 512);

            return sector;
        }

        /// <summary>
        ///     Master Directory Block, should be sector 2 in volume
        /// </summary>
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
            /// <summary>
            ///     0x082, Bytes in the extents B-Tree
            ///     3 HFS extents following, 32 bits each
            /// </summary>
            public readonly uint drXTFlSize;
            /// <summary>
            ///     0x092, Bytes in the catalog B-Tree
            ///     3 HFS extents following, 32 bits each
            /// </summary>
            public readonly uint drCTFlSize;
        }

        /// <summary>
        ///     Should be sectors 0 and 1 in volume, followed by boot code
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HfsBootBlock // Should be sectors 0 and 1 in volume
        {
            /// <summary>0x000, Signature, 0x4C4B if bootable</summary>
            public readonly ushort signature;
            /// <summary>0x002, Branch</summary>
            public readonly uint branch;
            /// <summary>0x007, Boot block version</summary>
            public readonly ushort boot_version;
            /// <summary>0x006, Boot block flags</summary>
            public readonly short boot_flags;
            /// <summary>0x00A, System file name (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] system_name;
            /// <summary>0x01A, Finder file name (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] finder_name;
            /// <summary>0x02A, Debugger file name (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] debug_name;
            /// <summary>0x03A, Disassembler file name (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] disasm_name;
            /// <summary>0x04A, Startup screen file name (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] stupscr_name;
            /// <summary>0x05A, First program to execute on boot (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] bootup_name;
            /// <summary>0x06A, Clipboard file name (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] clipbrd_name;
            /// <summary>0x07A, 1/4 of maximum opened at a time files</summary>
            public readonly ushort max_files;
            /// <summary>0x07C, Event queue size</summary>
            public readonly ushort queue_size;
            /// <summary>0x07E, Heap size on a Mac with 128KiB of RAM</summary>
            public readonly uint heap_128k;
            /// <summary>0x082, Heap size on a Mac with 256KiB of RAM</summary>
            public readonly uint heap_256k;
            /// <summary>0x086, Heap size on a Mac with 512KiB of RAM or more</summary>
            public readonly uint heap_512k;
            /// <summary>Padding</summary>
            public readonly ushort padding;
            /// <summary>Additional system heap space</summary>
            public readonly uint heap_extra;
            /// <summary>Fraction of RAM for system heap</summary>
            public readonly uint heap_fract;
        }
    }
}