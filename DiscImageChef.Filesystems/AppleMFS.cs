// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : AppleMFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Macintosh File System filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Apple Macintosh File System and shows information.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.Text;
using DiscImageChef;
using System.Collections.Generic;

// Information from Inside Macintosh
namespace DiscImageChef.Filesystems
{
    class AppleMFS : Filesystem
    {
        const UInt16 MFS_MAGIC = 0xD2D7;
        // "LK"
        const UInt16 MFSBB_MAGIC = 0x4C4B;

        public AppleMFS()
        {
            Name = "Apple Macintosh File System";
            PluginUUID = new Guid("36405F8D-0D26-4066-6538-5DBF5D065C3A");
        }

        public AppleMFS(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            Name = "Apple Macintosh File System";
            PluginUUID = new Guid("36405F8D-0D26-4066-6538-5DBF5D065C3A");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            UInt16 drSigWord;

            if((2 + partitionStart) >= imagePlugin.GetSectors())
                return false;

            byte[] mdb_sector = imagePlugin.ReadSector(2 + partitionStart);

            drSigWord = BigEndianBitConverter.ToUInt16(mdb_sector, 0x000);

            return drSigWord == MFS_MAGIC;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd, out string information)
        {
            information = "";

            StringBuilder sb = new StringBuilder();

            MFS_MasterDirectoryBlock MDB = new MFS_MasterDirectoryBlock();
            MFS_BootBlock BB = new MFS_BootBlock();

            byte[] pString = new byte[16];
            byte[] variable_size;

            byte[] mdb_sector = imagePlugin.ReadSector(2 + partitionStart);
            byte[] bb_sector = imagePlugin.ReadSector(0 + partitionStart);

            MDB.drSigWord = BigEndianBitConverter.ToUInt16(mdb_sector, 0x000);
            if(MDB.drSigWord != MFS_MAGIC)
                return;

            MDB.drCrDate = BigEndianBitConverter.ToUInt32(mdb_sector, 0x002);
            MDB.drLsBkUp = BigEndianBitConverter.ToUInt32(mdb_sector, 0x006);
            MDB.drAtrb = BigEndianBitConverter.ToUInt16(mdb_sector, 0x00A);
            MDB.drNmFls = BigEndianBitConverter.ToUInt16(mdb_sector, 0x00C);
            MDB.drDirSt = BigEndianBitConverter.ToUInt16(mdb_sector, 0x00E);
            MDB.drBlLen = BigEndianBitConverter.ToUInt16(mdb_sector, 0x010);
            MDB.drNmAlBlks = BigEndianBitConverter.ToUInt16(mdb_sector, 0x012);
            MDB.drAlBlkSiz = BigEndianBitConverter.ToUInt32(mdb_sector, 0x014);
            MDB.drClpSiz = BigEndianBitConverter.ToUInt32(mdb_sector, 0x018);
            MDB.drAlBlSt = BigEndianBitConverter.ToUInt16(mdb_sector, 0x01C);
            MDB.drNxtFNum = BigEndianBitConverter.ToUInt32(mdb_sector, 0x01E);
            MDB.drFreeBks = BigEndianBitConverter.ToUInt16(mdb_sector, 0x022);
            MDB.drVNSiz = mdb_sector[0x024];
            variable_size = new byte[MDB.drVNSiz];
            Array.Copy(mdb_sector, 0x025, variable_size, 0, MDB.drVNSiz);
            MDB.drVN = Encoding.ASCII.GetString(variable_size);

            BB.signature = BigEndianBitConverter.ToUInt16(bb_sector, 0x000);

            if(BB.signature == MFSBB_MAGIC)
            {
                BB.branch = BigEndianBitConverter.ToUInt32(bb_sector, 0x002);
                BB.boot_flags = bb_sector[0x006];
                BB.boot_version = bb_sector[0x007];

                BB.sec_sv_pages = BigEndianBitConverter.ToInt16(bb_sector, 0x008);

                Array.Copy(mdb_sector, 0x00A, pString, 0, 16);
                BB.system_name = StringHandlers.PascalToString(pString);
                Array.Copy(mdb_sector, 0x01A, pString, 0, 16);
                BB.finder_name = StringHandlers.PascalToString(pString);
                Array.Copy(mdb_sector, 0x02A, pString, 0, 16);
                BB.debug_name = StringHandlers.PascalToString(pString);
                Array.Copy(mdb_sector, 0x03A, pString, 0, 16);
                BB.disasm_name = StringHandlers.PascalToString(pString);
                Array.Copy(mdb_sector, 0x04A, pString, 0, 16);
                BB.stupscr_name = StringHandlers.PascalToString(pString);
                Array.Copy(mdb_sector, 0x05A, pString, 0, 16);
                BB.bootup_name = StringHandlers.PascalToString(pString);
                Array.Copy(mdb_sector, 0x06A, pString, 0, 16);
                BB.clipbrd_name = StringHandlers.PascalToString(pString);

                BB.max_files = BigEndianBitConverter.ToUInt16(bb_sector, 0x07A);
                BB.queue_size = BigEndianBitConverter.ToUInt16(bb_sector, 0x07C);
                BB.heap_128k = BigEndianBitConverter.ToUInt32(bb_sector, 0x07E);
                BB.heap_256k = BigEndianBitConverter.ToUInt32(bb_sector, 0x082);
                BB.heap_512k = BigEndianBitConverter.ToUInt32(bb_sector, 0x086);
            }
            else
                BB.signature = 0x0000;

            sb.AppendLine("Apple Macintosh File System");
            sb.AppendLine();
            sb.AppendLine("Master Directory Block:");
            sb.AppendFormat("Creation date: {0}", DateHandlers.MacToDateTime(MDB.drCrDate)).AppendLine();
            sb.AppendFormat("Last backup date: {0}", DateHandlers.MacToDateTime(MDB.drLsBkUp)).AppendLine();
            if((MDB.drAtrb & 0x80) == 0x80)
                sb.AppendLine("Volume is locked by hardware.");
            if((MDB.drAtrb & 0x8000) == 0x8000)
                sb.AppendLine("Volume is locked by software.");
            sb.AppendFormat("{0} files on volume", MDB.drNmFls).AppendLine();
            sb.AppendFormat("First directory block: {0}", MDB.drDirSt).AppendLine();
            sb.AppendFormat("{0} blocks in directory.", MDB.drBlLen).AppendLine();
            sb.AppendFormat("{0} volume allocation blocks.", MDB.drNmAlBlks).AppendLine();
            sb.AppendFormat("Size of allocation blocks: {0}", MDB.drAlBlkSiz).AppendLine();
            sb.AppendFormat("{0} bytes to allocate.", MDB.drClpSiz).AppendLine();
            sb.AppendFormat("{0} first allocation block.", MDB.drAlBlSt).AppendLine();
            sb.AppendFormat("Next unused file number: {0}", MDB.drNxtFNum).AppendLine();
            sb.AppendFormat("{0} unused allocation blocks.", MDB.drFreeBks).AppendLine();
            sb.AppendFormat("Volume name: {0}", MDB.drVN).AppendLine();

            if(BB.signature == MFSBB_MAGIC)
            {
                sb.AppendLine("Volume is bootable.");
                sb.AppendLine();
                sb.AppendLine("Boot Block:");
                if((BB.boot_flags & 0x40) == 0x40)
                    sb.AppendLine("Boot block should be executed.");
                if((BB.boot_flags & 0x80) == 0x80)
                {
                    sb.AppendLine("Boot block is in new unknown format.");
                }
                else
                {
                    if(BB.sec_sv_pages > 0)
                        sb.AppendLine("Allocate secondary sound buffer at boot.");
                    else if(BB.sec_sv_pages < 0)
                        sb.AppendLine("Allocate secondary sound and video buffers at boot.");

                    sb.AppendFormat("System filename: {0}", BB.system_name).AppendLine();
                    sb.AppendFormat("Finder filename: {0}", BB.finder_name).AppendLine();
                    sb.AppendFormat("Debugger filename: {0}", BB.debug_name).AppendLine();
                    sb.AppendFormat("Disassembler filename: {0}", BB.disasm_name).AppendLine();
                    sb.AppendFormat("Startup screen filename: {0}", BB.stupscr_name).AppendLine();
                    sb.AppendFormat("First program to execute at boot: {0}", BB.bootup_name).AppendLine();
                    sb.AppendFormat("Clipboard filename: {0}", BB.clipbrd_name).AppendLine();
                    sb.AppendFormat("Maximum opened files: {0}", BB.max_files * 4).AppendLine();
                    sb.AppendFormat("Event queue size: {0}", BB.queue_size).AppendLine();
                    sb.AppendFormat("Heap size with 128KiB of RAM: {0} bytes", BB.heap_128k).AppendLine();
                    sb.AppendFormat("Heap size with 256KiB of RAM: {0} bytes", BB.heap_256k).AppendLine();
                    sb.AppendFormat("Heap size with 512KiB of RAM or more: {0} bytes", BB.heap_512k).AppendLine();
                }
            }
            else
                sb.AppendLine("Volume is not bootable.");

            information = sb.ToString();

            xmlFSType = new Schemas.FileSystemType();
            if(MDB.drLsBkUp > 0)
            {
                xmlFSType.BackupDate = DateHandlers.MacToDateTime(MDB.drLsBkUp);
                xmlFSType.BackupDateSpecified = true;
            }
            xmlFSType.Bootable = BB.signature == MFSBB_MAGIC;
            xmlFSType.Clusters = MDB.drNmAlBlks;
            xmlFSType.ClusterSize = (int)MDB.drAlBlkSiz;
            if(MDB.drCrDate > 0)
            {
                xmlFSType.CreationDate = DateHandlers.MacToDateTime(MDB.drCrDate);
                xmlFSType.CreationDateSpecified = true;
            }
            xmlFSType.Files = MDB.drNmFls;
            xmlFSType.FilesSpecified = true;
            xmlFSType.FreeClusters = MDB.drFreeBks;
            xmlFSType.FreeClustersSpecified = true;
            xmlFSType.Type = "MFS";
            xmlFSType.VolumeName = MDB.drVN;

            return;
        }

        /// <summary>
        /// Master Directory Block, should be at offset 0x0400 bytes in volume
        /// </summary>
        struct MFS_MasterDirectoryBlock
        {
            /// <summary>0x000, Signature, 0xD2D7</summary>
            public UInt16 drSigWord;
            /// <summary>0x002, Volume creation date</summary>
            public UInt32 drCrDate;
            /// <summary>0x006, Volume last backup date</summary>
            public UInt32 drLsBkUp;
            /// <summary>0x00A, Volume attributes</summary>
            public UInt16 drAtrb;
            /// <summary>0x00C, Volume number of files</summary>
            public UInt16 drNmFls;
            /// <summary>0x00E, First directory block</summary>
            public UInt16 drDirSt;
            /// <summary>0x010, Length of directory in blocks</summary>
            public UInt16 drBlLen;
            /// <summary>0x012, Volume allocation blocks</summary>
            public UInt16 drNmAlBlks;
            /// <summary>0x014, Size of allocation blocks</summary>
            public UInt32 drAlBlkSiz;
            /// <summary>0x018, Number of bytes to allocate</summary>
            public UInt32 drClpSiz;
            /// <summary>0x01C, First allocation block in block map</summary>
            public UInt16 drAlBlSt;
            /// <summary>0x01E. Next unused file number</summary>
            public UInt32 drNxtFNum;
            /// <summary>0x022, Number of unused allocation blocks</summary>
            public UInt16 drFreeBks;
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
            public UInt16 signature;
            /// <summary>0x002, Branch</summary>
            public UInt32 branch;
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
            public UInt16 max_files;
            /// <summary>0x07C, Event queue size</summary>
            public UInt16 queue_size;
            /// <summary>0x07E, Heap size on a Mac with 128KiB of RAM</summary>
            public UInt32 heap_128k;
            /// <summary>0x082, Heap size on a Mac with 256KiB of RAM</summary>
            public UInt32 heap_256k;
            /// <summary>0x086, Heap size on a Mac with 512KiB of RAM or more</summary>
            public UInt32 heap_512k;
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
