// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : AmigaDOS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Amiga Fast File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Amiga Fast File System and shows information.
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Text;
using System.Collections.Generic;
using DiscImageChef.Console;
using System.Runtime.InteropServices;

namespace DiscImageChef.Filesystems
{
    public class AmigaDOSPlugin : Filesystem
    {
        public AmigaDOSPlugin()
        {
            Name = "Amiga DOS filesystem";
            PluginUUID = new Guid("3c882400-208c-427d-a086-9119852a1bc7");
            CurrentEncoding = Encoding.GetEncoding("iso-8859-1");
        }

        public AmigaDOSPlugin(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd, Encoding encoding)
        {
            Name = "Amiga DOS filesystem";
            PluginUUID = new Guid("3c882400-208c-427d-a086-9119852a1bc7");
            if(encoding == null)
                CurrentEncoding = Encoding.GetEncoding("iso-8859-1");
        }

        /// <summary>
        /// Boot block, first 2 sectors
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BootBlock
        {
            /// <summary>
            /// Offset 0x00, "DOSx" disk type
            /// </summary>
            public uint diskType;
            /// <summary>
            /// Offset 0x04, Checksum
            /// </summary>
            public uint checksum;
            /// <summary>
            /// Offset 0x08, Pointer to root block, mostly invalid
            /// </summary>
            public uint root_ptr;
            /// <summary>
            /// Offset 0x0C, Boot code, til completion. Size is intentionally incorrect to allow marshaling to work.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public byte[] bootCode;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct RootBlock
        {
            /// <summary>
            /// Offset 0x00, block type, value = T_HEADER (2)
            /// </summary>
            public uint type;
            /// <summary>
            /// Offset 0x04, unused
            /// </summary>
            public uint headerKey;
            /// <summary>
            /// Offset 0x08, unused
            /// </summary>
            public uint highSeq;
            /// <summary>
            /// Offset 0x0C, longs used by hash table
            /// </summary>
            public uint hashTableSize;
            /// <summary>
            /// Offset 0x10, unused
            /// </summary>
            public uint firstData;
            /// <summary>
            /// Offset 0x14, Rootblock checksum
            /// </summary>
            public uint checksum;
            /// <summary>
            /// Offset 0x18, Hashtable, size = (block size / 4) - 56 or size = hashTableSize.
            /// Size intentionally bad to allow marshalling to work.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public uint[] hashTable;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+0, bitmap flag, 0xFFFFFFFF if valid
            /// </summary>
            public uint bitmapFlag;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+4, bitmap pages, 25 entries
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 25)]
            public uint[] bitmapPages;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+104, pointer to bitmap extension block
            /// </summary>
            public uint bitmapExtensionBlock;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+108, last root alteration days since 1978/01/01
            /// </summary>
            public uint rDays;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+112, last root alteration minutes past midnight
            /// </summary>
            public uint rMins;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+116, last root alteration ticks (1/50 secs)
            /// </summary>
            public uint rTicks;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+120, disk name, pascal string, 31 bytes
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 31)]
            public byte[] diskName;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+151, unused
            /// </summary>
            public byte padding;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+152, unused
            /// </summary>
            public uint reserved1;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+156, unused
            /// </summary>
            public uint reserved2;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+160, last disk alteration days since 1978/01/01
            /// </summary>
            public uint vDays;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+164, last disk alteration minutes past midnight
            /// </summary>
            public uint vMins;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+168, last disk alteration ticks (1/50 secs)
            /// </summary>
            public uint vTicks;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+172, filesystem creation days since 1978/01/01
            /// </summary>
            public uint cDays;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+176, filesystem creation minutes since 1978/01/01
            /// </summary>
            public uint cMins;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+180, filesystem creation ticks since 1978/01/01
            /// </summary>
            public uint cTicks;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+184, unused
            /// </summary>
            public uint nextHash;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+188, unused
            /// </summary>
            public uint parentDir;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+192, first directory cache block
            /// </summary>
            public uint extension;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+196, block secondary type = ST_ROOT (1)
            /// </summary>
            public uint sec_type;
        }

        public const uint FFS_Mask = 0x444F5300;
        public const uint MuFS_Mask = 0x6D754600;

        public const uint TypeHeader = 2;
        public const uint SubTypeRoot = 1;

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            if(partitionStart >= partitionEnd)
                return false;

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            // Boot block is unless defined otherwise, 2 blocks
            // Funny, you may need boot block to find root block if it's not in standard place just to know size of
            // block size and then read the whole boot block.
            // However while you can set a block size different from the sector size on formatting, the bootblock block
            // size for floppies is the sector size, and for RDB is usually is the hard disk sector size,
            // so this is not entirely wrong...
            byte[] sector = imagePlugin.ReadSectors(0 + partitionStart, 2);
            BootBlock bblk = BigEndianMarshal.ByteArrayToStructureBigEndian<BootBlock>(sector);

            // Not FFS or MuFS?
            if((bblk.diskType & FFS_Mask) != FFS_Mask &&
               (bblk.diskType & MuFS_Mask) != MuFS_Mask)
                return false;

            // Clear checksum on sector
            sector[4] = sector[5] = sector[6] = sector[7] = 0;
            uint bsum = AmigaBootChecksum(sector);

            DicConsole.DebugWriteLine("AmigaDOS plugin", "bblk.checksum = 0x{0:X8}", bblk.checksum);
            DicConsole.DebugWriteLine("AmigaDOS plugin", "bsum = 0x{0:X8}", bsum);

            ulong b_root_ptr = 0;

            // If bootblock is correct, let's take its rootblock pointer
            if(bsum == bblk.checksum)
            {
                b_root_ptr = bblk.root_ptr + partitionStart;
                DicConsole.DebugWriteLine("AmigaDOS plugin", "Bootblock points to {0} as Rootblock", b_root_ptr);
            }

            ulong[] root_ptrs = { b_root_ptr + partitionStart, ((partitionEnd - partitionStart) + 1) / 2 + partitionStart - 2,
                ((partitionEnd - partitionStart) + 1) / 2 + partitionStart - 1, ((partitionEnd - partitionStart) + 1) / 2 + partitionStart};

            RootBlock rblk = new RootBlock();

            // So to handle even number of sectors
            foreach(ulong root_ptr in root_ptrs)
            {
                if(root_ptr >= partitionEnd || root_ptr < partitionStart)
                    continue;

                DicConsole.DebugWriteLine("AmigaDOS plugin", "Searching for Rootblock in sector {0}", root_ptr);

                sector = imagePlugin.ReadSector(root_ptr);

                rblk.type = BigEndianBitConverter.ToUInt32(sector, 0x00);
                DicConsole.DebugWriteLine("AmigaDOS plugin", "rblk.type = {0}", rblk.type);
                if(rblk.type != TypeHeader)
                    continue;

                rblk.hashTableSize = BigEndianBitConverter.ToUInt32(sector, 0x0C);

                DicConsole.DebugWriteLine("AmigaDOS plugin", "rblk.hashTableSize = {0}", rblk.hashTableSize);

                uint blockSize = (rblk.hashTableSize + 56) * 4;
                uint sectorsPerBlock = (uint)(blockSize / sector.Length);

                DicConsole.DebugWriteLine("AmigaDOS plugin", "blockSize = {0}", blockSize);
                DicConsole.DebugWriteLine("AmigaDOS plugin", "sectorsPerBlock = {0}", sectorsPerBlock);

                if(blockSize % sector.Length > 0)
                    sectorsPerBlock++;

                if(root_ptr + sectorsPerBlock >= partitionEnd)
                    continue;

                sector = imagePlugin.ReadSectors(root_ptr, sectorsPerBlock);

                // Clear checksum on sector
                rblk.checksum = BigEndianBitConverter.ToUInt32(sector, 20);
                sector[20] = sector[21] = sector[22] = sector[23] = 0;
                uint rsum = AmigaChecksum(sector);

                DicConsole.DebugWriteLine("AmigaDOS plugin", "rblk.checksum = 0x{0:X8}", rblk.checksum);
                DicConsole.DebugWriteLine("AmigaDOS plugin", "rsum = 0x{0:X8}", rsum);

                rblk.sec_type = BigEndianBitConverter.ToUInt32(sector, sector.Length - 4);
                DicConsole.DebugWriteLine("AmigaDOS plugin", "rblk.sec_type = {0}", rblk.sec_type);

                if(rblk.sec_type == SubTypeRoot && rblk.checksum == rsum)
                    return true;
            }

            return false;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd, out string information)
        {
            StringBuilder sbInformation = new StringBuilder();
            xmlFSType = new Schemas.FileSystemType();
            information = null;
            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            byte[] BootBlockSectors = imagePlugin.ReadSectors(0 + partitionStart, 2);

            BootBlock bootBlk = BigEndianMarshal.ByteArrayToStructureBigEndian<BootBlock>(BootBlockSectors);
            bootBlk.bootCode = new byte[BootBlockSectors.Length - 12];
            Array.Copy(BootBlockSectors, 12, bootBlk.bootCode, 0, bootBlk.bootCode.Length);
            BootBlockSectors[4] = BootBlockSectors[5] = BootBlockSectors[6] = BootBlockSectors[7] = 0;
            uint bsum = AmigaBootChecksum(BootBlockSectors);

            ulong b_root_ptr = 0;

            // If bootblock is correct, let's take its rootblock pointer
            if(bsum == bootBlk.checksum)
            {
                b_root_ptr = bootBlk.root_ptr + partitionStart;
                DicConsole.DebugWriteLine("AmigaDOS plugin", "Bootblock points to {0} as Rootblock", b_root_ptr);
            }

            ulong[] root_ptrs = { b_root_ptr + partitionStart, ((partitionEnd - partitionStart) + 1) / 2 + partitionStart - 2,
                ((partitionEnd - partitionStart) + 1) / 2 + partitionStart - 1, ((partitionEnd - partitionStart) + 1) / 2 + partitionStart};

            RootBlock rootBlk = new RootBlock();
            byte[] RootBlockSector = null;

            bool root_found = false;
            uint blockSize = 0;

            // So to handle even number of sectors
            foreach(ulong root_ptr in root_ptrs)
            {
                if(root_ptr >= partitionEnd || root_ptr < partitionStart)
                    continue;

                DicConsole.DebugWriteLine("AmigaDOS plugin", "Searching for Rootblock in sector {0}", root_ptr);

                RootBlockSector = imagePlugin.ReadSector(root_ptr);

                rootBlk.type = BigEndianBitConverter.ToUInt32(RootBlockSector, 0x00);
                DicConsole.DebugWriteLine("AmigaDOS plugin", "rootBlk.type = {0}", rootBlk.type);
                if(rootBlk.type != TypeHeader)
                    continue;

                rootBlk.hashTableSize = BigEndianBitConverter.ToUInt32(RootBlockSector, 0x0C);

                DicConsole.DebugWriteLine("AmigaDOS plugin", "rootBlk.hashTableSize = {0}", rootBlk.hashTableSize);

                blockSize = (rootBlk.hashTableSize + 56) * 4;
                uint sectorsPerBlock = (uint)(blockSize / RootBlockSector.Length);

                DicConsole.DebugWriteLine("AmigaDOS plugin", "blockSize = {0}", blockSize);
                DicConsole.DebugWriteLine("AmigaDOS plugin", "sectorsPerBlock = {0}", sectorsPerBlock);

                if(blockSize % RootBlockSector.Length > 0)
                    sectorsPerBlock++;

                if(root_ptr + sectorsPerBlock >= partitionEnd)
                    continue;

                RootBlockSector = imagePlugin.ReadSectors(root_ptr, sectorsPerBlock);

                // Clear checksum on sector
                rootBlk.checksum = BigEndianBitConverter.ToUInt32(RootBlockSector, 20);
                RootBlockSector[20] = RootBlockSector[21] = RootBlockSector[22] = RootBlockSector[23] = 0;
                uint rsum = AmigaChecksum(RootBlockSector);

                DicConsole.DebugWriteLine("AmigaDOS plugin", "rootBlk.checksum = 0x{0:X8}", rootBlk.checksum);
                DicConsole.DebugWriteLine("AmigaDOS plugin", "rsum = 0x{0:X8}", rsum);

                rootBlk.sec_type = BigEndianBitConverter.ToUInt32(RootBlockSector, RootBlockSector.Length - 4);
                DicConsole.DebugWriteLine("AmigaDOS plugin", "rootBlk.sec_type = {0}", rootBlk.sec_type);

                if(rootBlk.sec_type == SubTypeRoot && rootBlk.checksum == rsum)
                {
                    RootBlockSector = imagePlugin.ReadSectors(root_ptr, sectorsPerBlock);
                    root_found = true;
                    break;
                }
            }

            if(!root_found)
                return;

            rootBlk = MarshalRootBlock(RootBlockSector);

            string diskName = StringHandlers.PascalToString(rootBlk.diskName, CurrentEncoding);

            switch(bootBlk.diskType & 0xFF)
            {
                case 0:
                    sbInformation.Append("Amiga Original File System");
                    xmlFSType.Type = "Amiga OFS";
                    break;
                case 1:
                    sbInformation.Append("Amiga Fast File System");
                    xmlFSType.Type = "Amiga FFS";
                    break;
                case 2:
                    sbInformation.Append("Amiga Original File System with international characters");
                    xmlFSType.Type = "Amiga OFS";
                    break;
                case 3:
                    sbInformation.Append("Amiga Fast File System with international characters");
                    xmlFSType.Type = "Amiga FFS";
                    break;
                case 4:
                    sbInformation.Append("Amiga Original File System with directory cache");
                    xmlFSType.Type = "Amiga OFS";
                    break;
                case 5:
                    sbInformation.Append("Amiga Fast File System with directory cache");
                    xmlFSType.Type = "Amiga FFS";
                    break;
                case 6:
                    sbInformation.Append("Amiga Original File System with long filenames");
                    xmlFSType.Type = "Amiga OFS2";
                    break;
                case 7:
                    sbInformation.Append("Amiga Fast File System with long filenames");
                    xmlFSType.Type = "Amiga FFS2";
                    break;
            }

            if((bootBlk.diskType & 0x6D754600) == 0x6D754600)
                sbInformation.Append(", with multi-user patches");

            sbInformation.AppendLine();

            sbInformation.AppendFormat("Volume name: {0}", diskName).AppendLine();

            if(bootBlk.checksum == bsum)
            {
                Checksums.SHA1Context sha1Ctx = new Checksums.SHA1Context();
                sha1Ctx.Init();
                sha1Ctx.Update(bootBlk.bootCode);
                sbInformation.AppendLine("Volume is bootable");
                sbInformation.AppendFormat("Boot code SHA1 is {0}", sha1Ctx.End()).AppendLine();
            }

            if(rootBlk.bitmapFlag == 0xFFFFFFFF)
                sbInformation.AppendLine("Volume bitmap is valid");

            if(rootBlk.bitmapExtensionBlock != 0x00000000 && rootBlk.bitmapExtensionBlock != 0xFFFFFFFF)
                sbInformation.AppendFormat("Bitmap extension at block {0}", rootBlk.bitmapExtensionBlock).AppendLine();

            if((bootBlk.diskType & 0xFF) == 4 || (bootBlk.diskType & 0xFF) == 5)
                sbInformation.AppendFormat("Directory cache starts at block {0}", rootBlk.extension).AppendLine();

            long blocks = (long)((((partitionEnd - partitionStart) + 1) * imagePlugin.ImageInfo.sectorSize) / blockSize);

            sbInformation.AppendFormat("Volume block size is {0} bytes", blockSize).AppendLine();
            sbInformation.AppendFormat("Volume has {0} blocks", blocks).AppendLine();
            sbInformation.AppendFormat("Volume created on {0}", DateHandlers.AmigaToDateTime(rootBlk.cDays, rootBlk.cMins, rootBlk.cTicks)).AppendLine();
            sbInformation.AppendFormat("Volume last modified on {0}", DateHandlers.AmigaToDateTime(rootBlk.vDays, rootBlk.vMins, rootBlk.vTicks)).AppendLine();
            sbInformation.AppendFormat("Volume root directory last modified on on {0}", DateHandlers.AmigaToDateTime(rootBlk.rDays, rootBlk.rMins, rootBlk.rTicks)).AppendLine();
            sbInformation.AppendFormat("Root block checksum is 0x{0:X8}", rootBlk.checksum).AppendLine();
            information = sbInformation.ToString();

            xmlFSType.CreationDate = DateHandlers.AmigaToDateTime(rootBlk.cDays, rootBlk.cMins, rootBlk.cTicks);
            xmlFSType.CreationDateSpecified = true;
            xmlFSType.ModificationDate = DateHandlers.AmigaToDateTime(rootBlk.vDays, rootBlk.vMins, rootBlk.vTicks);
            xmlFSType.ModificationDateSpecified = true;
            xmlFSType.Dirty = rootBlk.bitmapFlag != 0xFFFFFFFF;
            xmlFSType.Clusters = blocks;
            xmlFSType.ClusterSize = (int)blockSize;
            xmlFSType.VolumeName = diskName;
            xmlFSType.Bootable = bsum == bootBlk.checksum;
            // Useful as a serial
            xmlFSType.VolumeSerial = string.Format("{0:X8}", rootBlk.checksum);
        }

        static RootBlock MarshalRootBlock(byte[] block)
        {
            byte[] tmp = new byte[228];
            Array.Copy(block, 0, tmp, 0, 24);
            Array.Copy(block, block.Length - 200, tmp, 28, 200);
            RootBlock root = BigEndianMarshal.ByteArrayToStructureBigEndian<RootBlock>(tmp);
            root.hashTable = new uint[(block.Length - 224) / 4];
            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
            for(int i = 0; i < root.hashTable.Length; i++)
                root.hashTable[i] = BigEndianBitConverter.ToUInt32(block, 24 + i * 4);
            
            return root;
        }

        static uint AmigaChecksum(byte[] data)
        {
            uint sum = 0;

            for(int i = 0; i < data.Length; i += 4)
                sum += (uint)((data[i] << 24) + (data[i + 1] << 16) + (data[i + 2] << 8) + data[i + 3]);

            return (uint)-sum;
        }

        static uint AmigaBootChecksum(byte[] data)
        {
            uint sum, psum;

            sum = 0;
            for(int i = 0; i < data.Length; i+=4) {
                psum = sum;
                if((sum += (uint)((data[i] << 24) + (data[i + 1] << 16) + (data[i + 2] << 8) + data[i + 3])) < psum)
                    sum++;
            }

            return ~sum;
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