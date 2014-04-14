using System;
using System.Text;
using FileSystemIDandChk;

// Information from Inside Macintosh
namespace FileSystemIDandChk.Plugins
{
    class AppleMFS : Plugin
    {
        const UInt16 MFS_MAGIC = 0xD2D7;
        // "LK"
        const UInt16 MFSBB_MAGIC = 0x4C4B;

        public AppleMFS(PluginBase Core)
        {
            Name = "Apple Macintosh File System";
            PluginUUID = new Guid("36405F8D-0D26-4066-6538-5DBF5D065C3A");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset)
        {
            UInt16 drSigWord;

            byte[] mdb_sector = imagePlugin.ReadSector(2 + partitionOffset);

            drSigWord = BigEndianBitConverter.ToUInt16(mdb_sector, 0x000);
			
            return drSigWord == MFS_MAGIC;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset, out string information)
        {
            information = "";
			
            StringBuilder sb = new StringBuilder();
			
            MFS_MasterDirectoryBlock MDB = new MFS_MasterDirectoryBlock();
            MFS_BootBlock BB = new MFS_BootBlock();
			
            byte[] pString = new byte[16];
            byte[] variable_size;

            byte[] mdb_sector = imagePlugin.ReadSector(2 + partitionOffset);
            byte[] bb_sector = imagePlugin.ReadSector(0 + partitionOffset);

            MDB.drSigWord = BigEndianBitConverter.ToUInt16(mdb_sector, 0x000);
            if (MDB.drSigWord != MFS_MAGIC)
                return;
			
            MDB.drCrDate = BigEndianBitConverter.ToUInt32(mdb_sector, 0x002);
            MDB.drLsBkUp = BigEndianBitConverter.ToUInt32(mdb_sector, 0x00A);
            MDB.drAtrb = BigEndianBitConverter.ToUInt16(mdb_sector, 0x012);
            MDB.drNmFls = BigEndianBitConverter.ToUInt16(mdb_sector, 0x014);
            MDB.drDirSt = BigEndianBitConverter.ToUInt16(mdb_sector, 0x016);
            MDB.drBlLen = BigEndianBitConverter.ToUInt16(mdb_sector, 0x018);
            MDB.drNmAlBlks = BigEndianBitConverter.ToUInt16(mdb_sector, 0x01A);
            MDB.drAlBlkSiz = BigEndianBitConverter.ToUInt32(mdb_sector, 0x01C);
            MDB.drClpSiz = BigEndianBitConverter.ToUInt32(mdb_sector, 0x020);
            MDB.drAlBlSt = BigEndianBitConverter.ToUInt16(mdb_sector, 0x024);
            MDB.drNxtFNum = BigEndianBitConverter.ToUInt32(mdb_sector, 0x026);
            MDB.drFreeBks = BigEndianBitConverter.ToUInt16(mdb_sector, 0x02A);
            MDB.drVNSiz = mdb_sector[0x02C];
            variable_size = new byte[MDB.drVNSiz];
            Array.Copy(mdb_sector, 0x02D, variable_size, 0, MDB.drVNSiz);
            MDB.drVN = Encoding.ASCII.GetString(variable_size);
			
            BB.signature = BigEndianBitConverter.ToUInt16(bb_sector, 0x000);
			
            if (BB.signature == MFSBB_MAGIC)
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
            if ((MDB.drAtrb & 0x80) == 0x80)
                sb.AppendLine("Volume is locked by hardware.");
            if ((MDB.drAtrb & 0x8000) == 0x8000)
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
			
            if (BB.signature == MFSBB_MAGIC)
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

        struct MFS_MasterDirectoryBlock // Should be offset 0x0400 bytes in volume
        {
            public UInt16 drSigWord;
            // 0x000, Signature, 0xD2D7
            public ulong drCrDate;
            // 0x002, Volume creation date
            public ulong drLsBkUp;
            // 0x00A, Volume last backup date
            public UInt16 drAtrb;
            // 0x012, Volume attributes
            public UInt16 drNmFls;
            // 0x014, Volume number of files
            public UInt16 drDirSt;
            // 0x016, First directory block
            public UInt16 drBlLen;
            // 0x018, Length of directory in blocks
            public UInt16 drNmAlBlks;
            // 0x01A, Volume allocation blocks
            public UInt32 drAlBlkSiz;
            // 0x01C, Size of allocation blocks
            public UInt32 drClpSiz;
            // 0x020, Number of bytes to allocate
            public UInt16 drAlBlSt;
            // 0x024, First allocation block in block map
            public UInt32 drNxtFNum;
            // 0x026. Next unused file number
            public UInt16 drFreeBks;
            // 0x02A, Number of unused allocation blocks
            public byte drVNSiz;
            // 0x02C, Length of volume name
            public string drVN;
            // 0x02D, Characters of volume name
        }

        struct MFS_BootBlock // Should be offset 0x0000 bytes in volume
        {
            public UInt16 signature;
            // 0x000, Signature, 0x4C4B if bootable
            public UInt32 branch;
            // 0x002, Branch
            public byte boot_flags;
            // 0x006, Boot block flags
            public byte boot_version;
            // 0x007, Boot block version
            public short sec_sv_pages;
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
