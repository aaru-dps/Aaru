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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;

namespace DiscImageChef.Filesystems
{
    // Information from Inside Macintosh
    // https://developer.apple.com/legacy/library/documentation/mac/pdf/Files/File_Manager.pdf
    public class AppleHFS : Filesystem
    {
        /// <summary>
        /// "BD", HFS magic
        /// </summary>
        const ushort HFS_MAGIC = 0x4244;
        /// <summary>
        /// "H+", HFS+ magic
        /// </summary>
        const ushort HFSP_MAGIC = 0x482B;
        /// <summary>
        /// "LK", HFS bootblock magic
        /// </summary>
        const ushort HFSBB_MAGIC = 0x4C4B;

        public AppleHFS()
        {
            Name = "Apple Hierarchical File System";
            PluginUUID = new Guid("36405F8D-0D26-6ECC-0BBB-1D5225FF404F");
            CurrentEncoding = Encoding.GetEncoding("macintosh");
        }

        public AppleHFS(Encoding encoding)
        {
            Name = "Apple Hierarchical File System";
            PluginUUID = new Guid("36405F8D-0D26-6ECC-0BBB-1D5225FF404F");
            if(encoding == null) CurrentEncoding = Encoding.GetEncoding("macintosh");
            else CurrentEncoding = encoding;
        }

        public AppleHFS(DiscImages.ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "Apple Hierarchical File System";
            PluginUUID = new Guid("36405F8D-0D26-6ECC-0BBB-1D5225FF404F");
            if(encoding == null) CurrentEncoding = Encoding.GetEncoding("macintosh");
            else CurrentEncoding = encoding;
        }

        public override bool Identify(DiscImages.ImagePlugin imagePlugin, Partition partition)
        {
            if(2 + partition.Start >= partition.End) return false;

            byte[] mdb_sector;
            ushort drSigWord;

            if(imagePlugin.GetSectorSize() == 2352 || imagePlugin.GetSectorSize() == 2448 ||
               imagePlugin.GetSectorSize() == 2048)
            {
                mdb_sector = imagePlugin.ReadSectors(partition.Start, 2);

                foreach(int offset in new[] {0, 0x200, 0x400, 0x600, 0x800, 0xA00})
                {
                    drSigWord = BigEndianBitConverter.ToUInt16(mdb_sector, offset);
                    if(drSigWord != HFS_MAGIC) continue;

                    drSigWord =
                        BigEndianBitConverter
                            .ToUInt16(mdb_sector, offset + 0x7C); // Seek to embedded HFS+ signature

                    return drSigWord != HFSP_MAGIC;
                }
            }
            else
            {
                mdb_sector = imagePlugin.ReadSector(2 + partition.Start);
                drSigWord = BigEndianBitConverter.ToUInt16(mdb_sector, 0);

                if(drSigWord != HFS_MAGIC) return false;

                drSigWord = BigEndianBitConverter.ToUInt16(mdb_sector, 0x7C); // Seek to embedded HFS+ signature

                return drSigWord != HFSP_MAGIC;
            }

            return false;
        }

        public override void GetInformation(DiscImages.ImagePlugin imagePlugin, Partition partition,
                                            out string information)
        {
            information = "";

            StringBuilder sb = new StringBuilder();

            HFS_MasterDirectoryBlock MDB = new HFS_MasterDirectoryBlock();
            HFS_BootBlock BB = new HFS_BootBlock();

            byte[] bb_sector = null;
            byte[] mdb_sector = null;
            ushort drSigWord;

            bool APMFromHDDOnCD = false;

            if(imagePlugin.GetSectorSize() == 2352 || imagePlugin.GetSectorSize() == 2448 ||
               imagePlugin.GetSectorSize() == 2048)
            {
                byte[] tmp_sector = imagePlugin.ReadSectors(partition.Start, 2);

                foreach(int offset in new[] {0, 0x200, 0x400, 0x600, 0x800, 0xA00})
                {
                    drSigWord = BigEndianBitConverter.ToUInt16(tmp_sector, offset);
                    if(drSigWord != HFS_MAGIC) continue;

                    bb_sector = new byte[1024];
                    mdb_sector = new byte[512];
                    if(offset >= 0x400) Array.Copy(tmp_sector, offset - 0x400, bb_sector, 0, 1024);
                    Array.Copy(tmp_sector, offset, mdb_sector, 0, 512);
                    APMFromHDDOnCD = true;
                    break;
                }

                if(!APMFromHDDOnCD) return;
            }
            else
            {
                mdb_sector = imagePlugin.ReadSector(2 + partition.Start);
                drSigWord = BigEndianBitConverter.ToUInt16(mdb_sector, 0);

                if(drSigWord == HFS_MAGIC) bb_sector = imagePlugin.ReadSector(partition.Start);
                else return;
            }

            MDB = BigEndianMarshal.ByteArrayToStructureBigEndian<HFS_MasterDirectoryBlock>(mdb_sector);
            BB = BigEndianMarshal.ByteArrayToStructureBigEndian<HFS_BootBlock>(bb_sector);

            sb.AppendLine("Apple Hierarchical File System");
            sb.AppendLine();
            if(APMFromHDDOnCD)
                sb.AppendLine("HFS uses 512 bytes/sector while device uses 2048 bytes/sector.").AppendLine();
            sb.AppendLine("Master Directory Block:");
            sb.AppendFormat("Creation date: {0}", DateHandlers.MacToDateTime(MDB.drCrDate)).AppendLine();
            sb.AppendFormat("Last modification date: {0}", DateHandlers.MacToDateTime(MDB.drLsMod)).AppendLine();
            if(MDB.drVolBkUp > 0)
            {
                sb.AppendFormat("Last backup date: {0}", DateHandlers.MacToDateTime(MDB.drVolBkUp)).AppendLine();
                sb.AppendFormat("Backup sequence number: {0}", MDB.drVSeqNum).AppendLine();
            }
            else sb.AppendLine("Volume has never been backed up");

            if((MDB.drAtrb & 0x80) == 0x80) sb.AppendLine("Volume is locked by hardware.");
            if((MDB.drAtrb & 0x100) == 0x100) sb.AppendLine("Volume was unmonted.");
            else sb.AppendLine("Volume is mounted.");
            if((MDB.drAtrb & 0x200) == 0x200) sb.AppendLine("Volume has spared bad blocks.");
            if((MDB.drAtrb & 0x400) == 0x400) sb.AppendLine("Volume does not need cache.");
            if((MDB.drAtrb & 0x800) == 0x800) sb.AppendLine("Boot volume is inconsistent.");
            if((MDB.drAtrb & 0x1000) == 0x1000) sb.AppendLine("There are reused CNIDs.");
            if((MDB.drAtrb & 0x2000) == 0x2000) sb.AppendLine("Volume is journaled.");
            if((MDB.drAtrb & 0x4000) == 0x4000) sb.AppendLine("Volume is seriously inconsistent.");
            if((MDB.drAtrb & 0x8000) == 0x8000) sb.AppendLine("Volume is locked by software.");

            sb.AppendFormat("{0} files on root directory", MDB.drNmFls).AppendLine();
            sb.AppendFormat("{0} directories on root directory", MDB.drNmRtDirs).AppendLine();
            sb.AppendFormat("{0} files on volume", MDB.drFilCnt).AppendLine();
            sb.AppendFormat("{0} directories on volume", MDB.drDirCnt).AppendLine();
            sb.AppendFormat("Volume write count: {0}", MDB.drWrCnt).AppendLine();

            sb.AppendFormat("Volume bitmap starting sector (in 512-bytes): {0}", MDB.drVBMSt).AppendLine();
            sb.AppendFormat("Next allocation block: {0}.", MDB.drAllocPtr).AppendLine();
            sb.AppendFormat("{0} volume allocation blocks.", MDB.drNmAlBlks).AppendLine();
            sb.AppendFormat("{0} bytes per allocation block.", MDB.drAlBlkSiz).AppendLine();
            sb.AppendFormat("{0} bytes to allocate when extending a file.", MDB.drClpSiz).AppendLine();
            sb.AppendFormat("{0} bytes to allocate when extending a Extents B-Tree.", MDB.drXTClpSiz).AppendLine();
            sb.AppendFormat("{0} bytes to allocate when extending a Catalog B-Tree.", MDB.drCTClpSiz).AppendLine();
            sb.AppendFormat("Sector of first allocation block: {0}", MDB.drAlBlSt).AppendLine();
            sb.AppendFormat("Next unused CNID: {0}", MDB.drNxtCNID).AppendLine();
            sb.AppendFormat("{0} unused allocation blocks.", MDB.drFreeBks).AppendLine();

            sb.AppendFormat("{0} bytes in the Extents B-Tree", MDB.drXTFlSize).AppendLine();
            sb.AppendFormat("{0} bytes in the Catalog B-Tree", MDB.drCTFlSize).AppendLine();

            sb.AppendFormat("Volume name: {0}", StringHandlers.PascalToString(MDB.drVN, CurrentEncoding)).AppendLine();

            sb.AppendLine("Finder info:");
            sb.AppendFormat("CNID of bootable system's directory: {0}", MDB.drFndrInfo0).AppendLine();
            sb.AppendFormat("CNID of first-run application's directory: {0}", MDB.drFndrInfo1).AppendLine();
            sb.AppendFormat("CNID of previously opened directory: {0}", MDB.drFndrInfo2).AppendLine();
            sb.AppendFormat("CNID of bootable Mac OS 8 or 9 directory: {0}", MDB.drFndrInfo3).AppendLine();
            sb.AppendFormat("CNID of bootable Mac OS X directory: {0}", MDB.drFndrInfo5).AppendLine();
            if(MDB.drFndrInfo6 != 0 && MDB.drFndrInfo7 != 0)
                sb.AppendFormat("Mac OS X Volume ID: {0:X8}{1:X8}", MDB.drFndrInfo6, MDB.drFndrInfo7).AppendLine();

            if(MDB.drEmbedSigWord == HFSP_MAGIC)
            {
                sb.AppendLine("Volume wraps a HFS+ volume.");
                sb.AppendFormat("Starting block of the HFS+ volume: {0}", MDB.xdrStABNt).AppendLine();
                sb.AppendFormat("Allocations blocks of the HFS+ volume: {0}", MDB.xdrNumABlks).AppendLine();
            }
            else
            {
                sb.AppendFormat("{0} blocks in volume cache", MDB.drVCSize).AppendLine();
                sb.AppendFormat("{0} blocks in volume bitmap cache", MDB.drVBMCSize).AppendLine();
                sb.AppendFormat("{0} blocks in volume common cache", MDB.drCtlCSize).AppendLine();
            }

            if(BB.signature == HFSBB_MAGIC)
            {
                sb.AppendLine("Volume is bootable.");
                sb.AppendLine();
                sb.AppendLine("Boot Block:");
                if((BB.boot_flags & 0x40) == 0x40) sb.AppendLine("Boot block should be executed.");
                if((BB.boot_flags & 0x80) == 0x80) sb.AppendLine("Boot block is in new unknown format.");
                else
                {
                    if(BB.boot_flags > 0) sb.AppendLine("Allocate secondary sound buffer at boot.");
                    else if(BB.boot_flags < 0) sb.AppendLine("Allocate secondary sound and video buffers at boot.");

                    sb.AppendFormat("System filename: {0}",
                                    StringHandlers.PascalToString(BB.system_name, CurrentEncoding)).AppendLine();
                    sb.AppendFormat("Finder filename: {0}",
                                    StringHandlers.PascalToString(BB.finder_name, CurrentEncoding)).AppendLine();
                    sb.AppendFormat("Debugger filename: {0}",
                                    StringHandlers.PascalToString(BB.debug_name, CurrentEncoding)).AppendLine();
                    sb.AppendFormat("Disassembler filename: {0}",
                                    StringHandlers.PascalToString(BB.disasm_name, CurrentEncoding)).AppendLine();
                    sb.AppendFormat("Startup screen filename: {0}",
                                    StringHandlers.PascalToString(BB.stupscr_name, CurrentEncoding)).AppendLine();
                    sb.AppendFormat("First program to execute at boot: {0}",
                                    StringHandlers.PascalToString(BB.bootup_name, CurrentEncoding)).AppendLine();
                    sb.AppendFormat("Clipboard filename: {0}",
                                    StringHandlers.PascalToString(BB.clipbrd_name, CurrentEncoding)).AppendLine();
                    sb.AppendFormat("Maximum opened files: {0}", BB.max_files * 4).AppendLine();
                    sb.AppendFormat("Event queue size: {0}", BB.queue_size).AppendLine();
                    sb.AppendFormat("Heap size with 128KiB of RAM: {0} bytes", BB.heap_128k).AppendLine();
                    sb.AppendFormat("Heap size with 256KiB of RAM: {0} bytes", BB.heap_256k).AppendLine();
                    sb.AppendFormat("Heap size with 512KiB of RAM or more: {0} bytes", BB.heap_512k).AppendLine();
                }
            }
            else if(MDB.drFndrInfo0 != 0 || MDB.drFndrInfo3 != 0 || MDB.drFndrInfo5 != 0)
                sb.AppendLine("Volume is bootable.");
            else sb.AppendLine("Volume is not bootable.");

            information = sb.ToString();

            xmlFSType = new Schemas.FileSystemType();
            if(MDB.drVolBkUp > 0)
            {
                xmlFSType.BackupDate = DateHandlers.MacToDateTime(MDB.drVolBkUp);
                xmlFSType.BackupDateSpecified = true;
            }
            xmlFSType.Bootable = BB.signature == HFSBB_MAGIC || MDB.drFndrInfo0 != 0 || MDB.drFndrInfo3 != 0 || MDB.drFndrInfo5 != 0;
            xmlFSType.Clusters = MDB.drNmAlBlks;
            xmlFSType.ClusterSize = (int)MDB.drAlBlkSiz;
            if(MDB.drCrDate > 0)
            {
                xmlFSType.CreationDate = DateHandlers.MacToDateTime(MDB.drCrDate);
                xmlFSType.CreationDateSpecified = true;
            }
            xmlFSType.Dirty = (MDB.drAtrb & 0x100) != 0x100;
            xmlFSType.Files = MDB.drFilCnt;
            xmlFSType.FilesSpecified = true;
            xmlFSType.FreeClusters = MDB.drFreeBks;
            xmlFSType.FreeClustersSpecified = true;
            if(MDB.drLsMod > 0)
            {
                xmlFSType.ModificationDate = DateHandlers.MacToDateTime(MDB.drLsMod);
                xmlFSType.ModificationDateSpecified = true;
            }
            xmlFSType.Type = "HFS";
            xmlFSType.VolumeName = StringHandlers.PascalToString(MDB.drVN, CurrentEncoding);
            if(MDB.drFndrInfo6 != 0 && MDB.drFndrInfo7 != 0)
                xmlFSType.VolumeSerial = string.Format("{0:X8}{1:X8}", MDB.drFndrInfo6, MDB.drFndrInfo7);

            return;
        }

        static byte[] Read2048SectorAs512(DiscImages.ImagePlugin imagePlugin, ulong LBA)
        {
            ulong LBA2k = LBA / 4;
            int Remainder = (int)(LBA % 4);

            byte[] buffer = imagePlugin.ReadSector(LBA2k);
            byte[] sector = new byte[512];

            Array.Copy(buffer, Remainder * 512, sector, 0, 512);

            return sector;
        }

        /// <summary>
        /// Master Directory Block, should be sector 2 in volume
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HFS_MasterDirectoryBlock // Should be sector 2 in volume
        {
            /// <summary>0x000, Signature, 0x4244</summary>
            public ushort drSigWord;
            /// <summary>0x002, Volume creation date</summary>
            public uint drCrDate;
            /// <summary>0x006, Volume last modification date</summary>
            public uint drLsMod;
            /// <summary>0x00A, Volume attributes</summary>
            public ushort drAtrb;
            /// <summary>0x00C, Files in root directory</summary>
            public ushort drNmFls;
            /// <summary>0x00E, Start 512-byte sector of volume bitmap</summary>
            public ushort drVBMSt;
            /// <summary>0x010, Allocation block to begin next allocation</summary>
            public ushort drAllocPtr;
            /// <summary>0x012, Allocation blocks</summary>
            public ushort drNmAlBlks;
            /// <summary>0x014, Bytes per allocation block</summary>
            public uint drAlBlkSiz;
            /// <summary>0x018, Bytes to allocate when extending a file</summary>
            public uint drClpSiz;
            /// <summary>0x01C, Start 512-byte sector of first allocation block</summary>
            public ushort drAlBlSt;
            /// <summary>0x01E, CNID for next file</summary>
            public uint drNxtCNID;
            /// <summary>0x022, Free allocation blocks</summary>
            public ushort drFreeBks;
            /// <summary>0x024, Volume name (28 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)] public byte[] drVN;
            /// <summary>0x040, Volume last backup time</summary>
            public uint drVolBkUp;
            /// <summary>0x044, Volume backup sequence number</summary>
            public ushort drVSeqNum;
            /// <summary>0x046, Filesystem write count</summary>
            public uint drWrCnt;
            /// <summary>0x04A, Bytes to allocate when extending the extents B-Tree</summary>
            public uint drXTClpSiz;
            /// <summary>0x04E, Bytes to allocate when extending the catalog B-Tree</summary>
            public uint drCTClpSiz;
            /// <summary>0x052, Number of directories in root directory</summary>
            public ushort drNmRtDirs;
            /// <summary>0x054, Number of files in the volume</summary>
            public uint drFilCnt;
            /// <summary>0x058, Number of directories in the volume</summary>
            public uint drDirCnt;
            /// <summary>0x05C, finderInfo[0], CNID for bootable system's directory</summary>
            public uint drFndrInfo0;
            /// <summary>0x060, finderInfo[1], CNID of the directory containing the boot application</summary>
            public uint drFndrInfo1;
            /// <summary>0x064, finderInfo[2], CNID of the directory that should be opened on boot</summary>
            public uint drFndrInfo2;
            /// <summary>0x068, finderInfo[3], CNID for Mac OS 8 or 9 directory</summary>
            public uint drFndrInfo3;
            /// <summary>0x06C, finderInfo[4], Reserved</summary>
            public uint drFndrInfo4;
            /// <summary>0x070, finderInfo[5], CNID for Mac OS X directory</summary>
            public uint drFndrInfo5;
            /// <summary>0x074, finderInfo[6], first part of Mac OS X volume ID</summary>
            public uint drFndrInfo6;
            /// <summary>0x078, finderInfo[7], second part of Mac OS X volume ID</summary>
            public uint drFndrInfo7;
            // If wrapping HFS+
            /// <summary>0x07C, Embedded volume signature, "H+" if HFS+ is embedded ignore following two fields if not</summary>
            public ushort drEmbedSigWord;
            /// <summary>0x07E, Starting block number of embedded HFS+ volume</summary>
            public ushort xdrStABNt;
            /// <summary>0x080, Allocation blocks used by embedded volume</summary>
            public ushort xdrNumABlks;
            // If not
            /// <summary>0x07C, Size in blocks of volume cache</summary>
            public ushort drVCSize;
            /// <summary>0x07E, Size in blocks of volume bitmap cache</summary>
            public ushort drVBMCSize;
            /// <summary>0x080, Size in blocks of volume common cache</summary>
            public ushort drCtlCSize;
            // End of variable variables :D
            /// <summary>0x082, Bytes in the extents B-Tree
            /// 3 HFS extents following, 32 bits each</summary>
            public uint drXTFlSize;
            /// <summary>0x092, Bytes in the catalog B-Tree
            /// 3 HFS extents following, 32 bits each</summary>
            public uint drCTFlSize;
        }

        /// <summary>
        /// Should be sectors 0 and 1 in volume, followed by boot code
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HFS_BootBlock // Should be sectors 0 and 1 in volume
        {
            /// <summary>0x000, Signature, 0x4C4B if bootable</summary>
            public ushort signature;
            /// <summary>0x002, Branch</summary>
            public uint branch;
            /// <summary>0x007, Boot block version</summary>
            public ushort boot_version;
            /// <summary>0x006, Boot block flags</summary>
            public short boot_flags;
            /// <summary>0x00A, System file name (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] system_name;
            /// <summary>0x01A, Finder file name (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] finder_name;
            /// <summary>0x02A, Debugger file name (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] debug_name;
            /// <summary>0x03A, Disassembler file name (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] disasm_name;
            /// <summary>0x04A, Startup screen file name (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] stupscr_name;
            /// <summary>0x05A, First program to execute on boot (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] bootup_name;
            /// <summary>0x06A, Clipboard file name (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] clipbrd_name;
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
            /// <summary>Padding</summary>
            public ushort padding;
            /// <summary>Additional system heap space</summary>
            public uint heap_extra;
            /// <summary>Fraction of RAM for system heap</summary>
            public uint heap_fract;
        }

        public override Errno Mount()
        {
            return Errno.NotImplemented;
        }

        public override Errno Mount(bool debug)
        {
            return Errno.NotImplemented;
        }

        public override Errno Unmount()
        {
            return Errno.NotImplemented;
        }

        public override Errno MapBlock(string path, long fileBlock, ref long deviceBlock)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetAttributes(string path, ref FileAttributes attributes)
        {
            return Errno.NotImplemented;
        }

        public override Errno ListXAttr(string path, ref List<string> xattrs)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadDir(string path, ref List<string> contents)
        {
            return Errno.NotImplemented;
        }

        public override Errno StatFs(ref FileSystemInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno Stat(string path, ref FileEntryInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadLink(string path, ref string dest)
        {
            return Errno.NotImplemented;
        }
    }
}