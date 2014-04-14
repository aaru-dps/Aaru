using System;
using System.IO;
using System.Text;
using FileSystemIDandChk;

// Information from Inside Macintosh
// https://developer.apple.com/legacy/library/documentation/mac/pdf/Files/File_Manager.pdf
namespace FileSystemIDandChk.Plugins
{
    class AppleHFS : Plugin
    {
        const UInt16 HFS_MAGIC = 0x4244;
        // "BD"
        const UInt16 HFSP_MAGIC = 0x482B;
        // "H+"
        const UInt16 HFSBB_MAGIC = 0x4C4B;
        // "LK"
        public AppleHFS(PluginBase Core)
        {
            Name = "Apple Hierarchical File System";
            PluginUUID = new Guid("36405F8D-0D26-6ECC-0BBB-1D5225FF404F");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset)
        {
            byte[] mdb_sector = imagePlugin.ReadSector(2 + partitionOffset);
            UInt16 drSigWord = BigEndianBitConverter.ToUInt16(mdb_sector, 0);

            if (drSigWord == HFS_MAGIC)
            {
                drSigWord = BigEndianBitConverter.ToUInt16(mdb_sector, 0x7C); // Seek to embedded HFS+ signature
				
                return drSigWord != HFSP_MAGIC;
            }
            return false;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset, out string information)
        {
            information = "";
			
            StringBuilder sb = new StringBuilder();
			
            HFS_MasterDirectoryBlock MDB = new HFS_MasterDirectoryBlock();
            HFS_BootBlock BB = new HFS_BootBlock();

            byte[] pString;

            byte[] bb_sector = imagePlugin.ReadSector(2 + partitionOffset); // BB's first sector
            byte[] mdb_sector = imagePlugin.ReadSector(2 + partitionOffset); // MDB sector
            MDB.drSigWord = BigEndianBitConverter.ToUInt16(mdb_sector, 0x000);
            if (MDB.drSigWord != HFS_MAGIC)
                return;
			
            MDB.drCrDate = BigEndianBitConverter.ToUInt32(mdb_sector, 0x002);
            MDB.drLsMod = BigEndianBitConverter.ToUInt32(mdb_sector, 0x006);
            MDB.drAtrb = BigEndianBitConverter.ToUInt16(mdb_sector, 0x00A);
            MDB.drNmFls = BigEndianBitConverter.ToUInt16(mdb_sector, 0x00C);
            MDB.drVBMSt = BigEndianBitConverter.ToUInt16(mdb_sector, 0X00E);
            MDB.drAllocPtr = BigEndianBitConverter.ToUInt16(mdb_sector, 0x010);
            MDB.drNmAlBlks = BigEndianBitConverter.ToUInt16(mdb_sector, 0x012);
            MDB.drAlBlkSiz = BigEndianBitConverter.ToUInt32(mdb_sector, 0x014);
            MDB.drClpSiz = BigEndianBitConverter.ToUInt32(mdb_sector, 0x018);
            MDB.drAlBlSt = BigEndianBitConverter.ToUInt16(mdb_sector, 0x01C);
            MDB.drNxtCNID = BigEndianBitConverter.ToUInt32(mdb_sector, 0x01E);
            MDB.drFreeBks = BigEndianBitConverter.ToUInt16(mdb_sector, 0x022);
            pString = new byte[28];
            Array.Copy(mdb_sector, 0x024, pString, 0, 28);
            MDB.drVN = StringHandlers.PascalToString(pString);
			
            MDB.drVolBkUp = BigEndianBitConverter.ToUInt32(mdb_sector, 0x040);
            MDB.drVSeqNum = BigEndianBitConverter.ToUInt16(mdb_sector, 0x044);
            MDB.drWrCnt = BigEndianBitConverter.ToUInt32(mdb_sector, 0x046);
            MDB.drXTClpSiz = BigEndianBitConverter.ToUInt32(mdb_sector, 0x04A);
            MDB.drCTClpSiz = BigEndianBitConverter.ToUInt32(mdb_sector, 0x04E);
            MDB.drNmRtDirs = BigEndianBitConverter.ToUInt16(mdb_sector, 0x052);
            MDB.drFilCnt = BigEndianBitConverter.ToUInt32(mdb_sector, 0x054);
            MDB.drDirCnt = BigEndianBitConverter.ToUInt32(mdb_sector, 0x058);
			
            MDB.drFndrInfo0 = BigEndianBitConverter.ToUInt32(mdb_sector, 0x05C);
            MDB.drFndrInfo1 = BigEndianBitConverter.ToUInt32(mdb_sector, 0x060);
            MDB.drFndrInfo2 = BigEndianBitConverter.ToUInt32(mdb_sector, 0x064);
            MDB.drFndrInfo3 = BigEndianBitConverter.ToUInt32(mdb_sector, 0x068);
            MDB.drFndrInfo4 = BigEndianBitConverter.ToUInt32(mdb_sector, 0x06C);
            MDB.drFndrInfo5 = BigEndianBitConverter.ToUInt32(mdb_sector, 0x070);
            MDB.drFndrInfo6 = BigEndianBitConverter.ToUInt32(mdb_sector, 0x074);
            MDB.drFndrInfo7 = BigEndianBitConverter.ToUInt32(mdb_sector, 0x078);

            MDB.drVCSize = BigEndianBitConverter.ToUInt16(mdb_sector, 0x07C);
            MDB.drVBMCSize = BigEndianBitConverter.ToUInt16(mdb_sector, 0x07E);
            MDB.drCtlCSize = BigEndianBitConverter.ToUInt16(mdb_sector, 0x080);

            // For HFS+ embedded volume
            MDB.drEmbedSigWord = BigEndianBitConverter.ToUInt16(mdb_sector, 0x07C);
            MDB.xdrStABNt = BigEndianBitConverter.ToUInt16(mdb_sector, 0x07E);
            MDB.xdrNumABlks = BigEndianBitConverter.ToUInt16(mdb_sector, 0x080);
			
            MDB.drXTFlSize = BigEndianBitConverter.ToUInt32(mdb_sector, 0x082);
            MDB.drCTFlSize = BigEndianBitConverter.ToUInt32(mdb_sector, 0x092);
			
            BB.signature = BigEndianBitConverter.ToUInt16(bb_sector, 0x000);
			
            if (BB.signature == HFSBB_MAGIC)
            {
                BB.branch = BigEndianBitConverter.ToUInt32(bb_sector, 0x002);
                BB.boot_flags = bb_sector[0x006];
                BB.boot_version = bb_sector[0x007];
				
                BB.sec_sv_pages = BigEndianBitConverter.ToInt16(bb_sector, 0x008);

                pString = new byte[16];
                Array.Copy(bb_sector, 0x00A, pString, 0, 16);
                BB.system_name = StringHandlers.PascalToString(pString);
                pString = new byte[16];
                Array.Copy(bb_sector, 0x01A, pString, 0, 16);
                BB.finder_name = StringHandlers.PascalToString(pString);
                pString = new byte[16];
                Array.Copy(bb_sector, 0x02A, pString, 0, 16);
                BB.debug_name = StringHandlers.PascalToString(pString);
                pString = new byte[16];
                Array.Copy(bb_sector, 0x03A, pString, 0, 16);
                BB.disasm_name = StringHandlers.PascalToString(pString);
                pString = new byte[16];
                Array.Copy(bb_sector, 0x04A, pString, 0, 16);
                BB.stupscr_name = StringHandlers.PascalToString(pString);
                pString = new byte[16];
                Array.Copy(bb_sector, 0x05A, pString, 0, 16);
                BB.bootup_name = StringHandlers.PascalToString(pString);
                pString = new byte[16];
                Array.Copy(bb_sector, 0x06A, pString, 0, 16);
                BB.clipbrd_name = StringHandlers.PascalToString(pString);
				
                BB.max_files = BigEndianBitConverter.ToUInt16(bb_sector, 0x07A);
                BB.queue_size = BigEndianBitConverter.ToUInt16(bb_sector, 0x07C);
                BB.heap_128k = BigEndianBitConverter.ToUInt32(bb_sector, 0x07E);
                BB.heap_256k = BigEndianBitConverter.ToUInt32(bb_sector, 0x082);
                BB.heap_512k = BigEndianBitConverter.ToUInt32(bb_sector, 0x086);
            }
            else
                BB.signature = 0x0000;
			
            sb.AppendLine("Apple Hierarchical File System");
            sb.AppendLine();
            sb.AppendLine("Master Directory Block:");
            sb.AppendFormat("Creation date: {0}", DateHandlers.MacToDateTime(MDB.drCrDate)).AppendLine();
            sb.AppendFormat("Last modification date: {0}", DateHandlers.MacToDateTime(MDB.drLsMod)).AppendLine();
            sb.AppendFormat("Last backup date: {0}", DateHandlers.MacToDateTime(MDB.drVolBkUp)).AppendLine();
            sb.AppendFormat("Backup sequence number: {0}", MDB.drVSeqNum).AppendLine();
			
            if ((MDB.drAtrb & 0x80) == 0x80)
                sb.AppendLine("Volume is locked by hardware.");
            if ((MDB.drAtrb & 0x100) == 0x100)
                sb.AppendLine("Volume was unmonted.");
            else
                sb.AppendLine("Volume is mounted.");
            if ((MDB.drAtrb & 0x200) == 0x200)
                sb.AppendLine("Volume has spared bad blocks.");
            if ((MDB.drAtrb & 0x400) == 0x400)
                sb.AppendLine("Volume does not need cache.");
            if ((MDB.drAtrb & 0x800) == 0x800)
                sb.AppendLine("Boot volume is inconsistent.");
            if ((MDB.drAtrb & 0x1000) == 0x1000)
                sb.AppendLine("There are reused CNIDs.");
            if ((MDB.drAtrb & 0x2000) == 0x2000)
                sb.AppendLine("Volume is journaled.");
            if ((MDB.drAtrb & 0x4000) == 0x4000)
                sb.AppendLine("Volume is seriously inconsistent.");
            if ((MDB.drAtrb & 0x8000) == 0x8000)
                sb.AppendLine("Volume is locked by software.");
			
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

            sb.AppendFormat("Volume name: {0}", MDB.drVN).AppendLine();
			
            sb.AppendLine("Finder info:");
            sb.AppendFormat("CNID of bootable system's directory: {0}", MDB.drFndrInfo0).AppendLine();
            sb.AppendFormat("CNID of first-run application's directory: {0}", MDB.drFndrInfo1).AppendLine();
            sb.AppendFormat("CNID of previously opened directory: {0}", MDB.drFndrInfo2).AppendLine();
            sb.AppendFormat("CNID of bootable Mac OS 8 or 9 directory: {0}", MDB.drFndrInfo3).AppendLine();
            sb.AppendFormat("CNID of bootable Mac OS X directory: {0}", MDB.drFndrInfo5).AppendLine();
            sb.AppendFormat("Mac OS X Volume ID: {0:X8}{1:X8}", MDB.drFndrInfo6, MDB.drFndrInfo7).AppendLine();
			
            if (MDB.drEmbedSigWord == HFSP_MAGIC)
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
			
            if (BB.signature == HFSBB_MAGIC)
            {
                sb.AppendLine("Volume is bootable.");
                sb.AppendLine();
                sb.AppendLine("Boot Block:");
                if ((BB.boot_flags & 0x40) == 0x40)
                    sb.AppendLine("Boot block should be executed.");
                if ((BB.boot_flags & 0x80) == 0x80)
                {
                    sb.AppendLine("Boot block is in new unknown format.");
                }
                else
                {
                    if (BB.sec_sv_pages > 0)
                        sb.AppendLine("Allocate secondary sound buffer at boot.");
                    else if (BB.sec_sv_pages < 0)
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
			
            return;
        }

        struct HFS_MasterDirectoryBlock // Should be sector 2 in volume
        {
            public UInt16 drSigWord;
            // 0x000, Signature, 0x4244
            public UInt32 drCrDate;
            // 0x002, Volume creation date
            public UInt32 drLsMod;
            // 0x006, Volume last modification date
            public UInt16 drAtrb;
            // 0x00A, Volume attributes
            public UInt16 drNmFls;
            // 0x00C, Files in root directory
            public UInt16 drVBMSt;
            // 0x00E, Start 512-byte sector of volume bitmap
            public UInt16 drAllocPtr;
            // 0x010, Allocation block to begin next allocation
            public UInt16 drNmAlBlks;
            // 0x012, Allocation blocks
            public UInt32 drAlBlkSiz;
            // 0x014, Bytes per allocation block
            public UInt32 drClpSiz;
            // 0x018, Bytes to allocate when extending a file
            public UInt16 drAlBlSt;
            // 0x01C, Start 512-byte sector of first allocation block
            public UInt32 drNxtCNID;
            // 0x01E, CNID for next file
            public UInt16 drFreeBks;
            // 0x022, Free allocation blocks
            public string drVN;
            // 0x024, Volume name (28 bytes)
            public UInt32 drVolBkUp;
            // 0x040, Volume last backup time
            public UInt16 drVSeqNum;
            // 0x044, Volume backup sequence number
            public UInt32 drWrCnt;
            // 0x046, Filesystem write count
            public UInt32 drXTClpSiz;
            // 0x04A, Bytes to allocate when extending the extents B-Tree
            public UInt32 drCTClpSiz;
            // 0x04E, Bytes to allocate when extending the catalog B-Tree
            public UInt16 drNmRtDirs;
            // 0x052, Number of directories in root directory
            public UInt32 drFilCnt;
            // 0x054, Number of files in the volume
            public UInt32 drDirCnt;
            // 0x058, Number of directories in the volume
            public UInt32 drFndrInfo0;
            // 0x05C, finderInfo[0], CNID for bootable system's directory
            public UInt32 drFndrInfo1;
            // 0x060, finderInfo[1], CNID of the directory containing the boot application
            public UInt32 drFndrInfo2;
            // 0x064, finderInfo[2], CNID of the directory that should be opened on boot
            public UInt32 drFndrInfo3;
            // 0x068, finderInfo[3], CNID for Mac OS 8 or 9 directory
            public UInt32 drFndrInfo4;
            // 0x06C, finderInfo[4], Reserved
            public UInt32 drFndrInfo5;
            // 0x070, finderInfo[5], CNID for Mac OS X directory
            public UInt32 drFndrInfo6;
            // 0x074, finderInfo[6], first part of Mac OS X volume ID
            public UInt32 drFndrInfo7;
            // 0x078, finderInfo[7], second part of Mac OS X volume ID
            // If wrapping HFS+
            public UInt16 drEmbedSigWord;
            // 0x07C, Embedded volume signature, "H+" if HFS+ is embedded ignore following two fields if not
            public UInt16 xdrStABNt;
            // 0x07E, Starting block number of embedded HFS+ volume
            public UInt16 xdrNumABlks;
            // 0x080, Allocation blocks used by embedded volume
            // If not
            public UInt16 drVCSize;
            // 0x07C, Size in blocks of volume cache
            public UInt16 drVBMCSize;
            // 0x07E, Size in blocks of volume bitmap cache
            public UInt16 drCtlCSize;
            // 0x080, Size in blocks of volume common cache
            // End of variable variables :D
            public UInt32 drXTFlSize;
            // 0x082, Bytes in the extents B-Tree
            // 3 HFS extents following, 32 bits each
            public UInt32 drCTFlSize;
            // 0x092, Bytes in the catalog B-Tree
            // 3 HFS extents following, 32 bits each
        }

        struct HFS_BootBlock // Should be sectors 0 and 1 in volume
        {
            public UInt16 signature;
            // 0x000, Signature, 0x4C4B if bootable
            public UInt32 branch;
            // 0x002, Branch
            public byte boot_flags;
            // 0x006, Boot block flags
            public byte boot_version;
            // 0x007, Boot block version
            public Int16 sec_sv_pages;
            // 0x008, Allocate secondary buffers
            public string system_name;
            // 0x00A, System file name (16 bytes)
            public string finder_name;
            // 0x01A, Finder file name (16 bytes)
            public string debug_name;
            // 0x02A, Debugger file name (16 bytes)
            public string disasm_name;
            // 0x03A, Disassembler file name (16 bytes)
            public string stupscr_name;
            // 0x04A, Startup screen file name (16 bytes)
            public string bootup_name;
            // 0x05A, First program to execute on boot (16 bytes)
            public string clipbrd_name;
            // 0x06A, Clipboard file name (16 bytes)
            public UInt16 max_files;
            // 0x07A, 1/4 of maximum opened at a time files
            public UInt16 queue_size;
            // 0x07C, Event queue size
            public UInt32 heap_128k;
            // 0x07E, Heap size on a Mac with 128KiB of RAM
            public UInt32 heap_256k;
            // 0x082, Heap size on a Mac with 256KiB of RAM
            public UInt32 heap_512k;
            // 0x086, Heap size on a Mac with 512KiB of RAM or more
        }
        // Follows boot code
    }
}
