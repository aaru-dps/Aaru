using System;
using System.IO;
using System.Text;
using FileSystemIDandChk;

// Information from Inside Macintosh

namespace FileSystemIDandChk.Plugins
{
	class AppleHFS : Plugin
	{
		private const UInt16 HFS_MAGIC   = 0x4244; // "BD"
		private const UInt16 HFSP_MAGIC  = 0x482B; // "H+"
		private const UInt16 HFSBB_MAGIC = 0x4C4B; // "LK"

		public AppleHFS(PluginBase Core)
        {
            base.Name = "Apple Hierarchical File System";
			base.PluginUUID = new Guid("36405F8D-0D26-6ECC-0BBB-1D5225FF404F");
        }
		
		public override bool Identify(FileStream stream, long offset)
		{
			byte[] signature = new byte[2];
			UInt16 drSigWord;

			EndianAwareBinaryReader eabr = new EndianAwareBinaryReader(stream, false); // BigEndian
			eabr.BaseStream.Seek(0x400 + offset, SeekOrigin.Begin);
			
			drSigWord = eabr.ReadUInt16();
			
			if(drSigWord == HFS_MAGIC)
			{
				eabr.BaseStream.Seek(0x47C + offset, SeekOrigin.Begin); // Seek to embedded HFS+ signature
				drSigWord = eabr.ReadUInt16();
				
				if(drSigWord == HFSP_MAGIC) // "H+"
					return false;
				else
					return true;
			}
			else
				return false;
		}
		
		public override void GetInformation (FileStream stream, long offset, out string information)
		{
			information = "";
			
			StringBuilder sb = new StringBuilder();
			
			HFS_MasterDirectoryBlock MDB = new HFS_MasterDirectoryBlock();
			HFS_BootBlock BB = new HFS_BootBlock();

			byte[] pString;

			EndianAwareBinaryReader eabr = new EndianAwareBinaryReader(stream, false); // BigEndian
			eabr.BaseStream.Seek(0x400 + offset, SeekOrigin.Begin);
			MDB.drSigWord = eabr.ReadUInt16();
			if(MDB.drSigWord != HFS_MAGIC)
				return;
			
			MDB.drCrDate = eabr.ReadUInt32();
			MDB.drLsMod = eabr.ReadUInt32();
			MDB.drAtrb = eabr.ReadUInt16();
			MDB.drNmFls = eabr.ReadUInt16();
			MDB.drVBMSt = eabr.ReadUInt16();
			MDB.drAllocPtr = eabr.ReadUInt16();
			MDB.drNmAlBlks = eabr.ReadUInt16();
			MDB.drAlBlkSiz = eabr.ReadUInt32();
			MDB.drClpSiz = eabr.ReadUInt32();
			MDB.drAlBlSt = eabr.ReadUInt16();
			MDB.drNxtCNID = eabr.ReadUInt32();
			MDB.drFreeBks = eabr.ReadUInt16();
			pString = eabr.ReadBytes(28);
			MDB.drVN = StringHandlers.PascalToString(pString);
			
			MDB.drVolBkUp = eabr.ReadUInt32();
			MDB.drVSeqNum = eabr.ReadUInt16();
			MDB.drWrCnt = eabr.ReadUInt32();
			MDB.drXTClpSiz = eabr.ReadUInt32();
			MDB.drCTClpSiz = eabr.ReadUInt32();
			MDB.drNmRtDirs = eabr.ReadUInt16();
			MDB.drFilCnt = eabr.ReadUInt32();
			MDB.drDirCnt = eabr.ReadUInt32();
			
			MDB.drFndrInfo0 = eabr.ReadUInt32();
			MDB.drFndrInfo1 = eabr.ReadUInt32();
			MDB.drFndrInfo2 = eabr.ReadUInt32();
			MDB.drFndrInfo3 = eabr.ReadUInt32();
			MDB.drFndrInfo4 = eabr.ReadUInt32();
			MDB.drFndrInfo5 = eabr.ReadUInt32();
			MDB.drFndrInfo6 = eabr.ReadUInt32();
			MDB.drFndrInfo7 = eabr.ReadUInt32();
			
			MDB.drEmbedSigWord = eabr.ReadUInt16();
			MDB.xdrStABNt = eabr.ReadUInt16();
			MDB.xdrNumABlks = eabr.ReadUInt16();
			
			MDB.drXTFlSize = eabr.ReadUInt32();
			eabr.BaseStream.Seek(12, SeekOrigin.Current);
			MDB.drCTFlSize = eabr.ReadUInt32();
			eabr.BaseStream.Seek(12, SeekOrigin.Current);
			
			eabr.BaseStream.Seek(0 + offset, SeekOrigin.Begin);
			BB.signature = eabr.ReadUInt16();
			
			if(BB.signature == HFSBB_MAGIC)
			{
				BB.branch = eabr.ReadUInt32();
				BB.boot_flags = eabr.ReadByte();
				BB.boot_version = eabr.ReadByte();
				
				BB.sec_sv_pages = eabr.ReadInt16();

				pString = eabr.ReadBytes(16);
				BB.system_name = StringHandlers.PascalToString(pString);
				pString = eabr.ReadBytes(16);
				BB.finder_name = StringHandlers.PascalToString(pString);
				pString = eabr.ReadBytes(16);
				BB.debug_name = StringHandlers.PascalToString(pString);
				pString = eabr.ReadBytes(16);
				BB.disasm_name = StringHandlers.PascalToString(pString);
				pString = eabr.ReadBytes(16);
				BB.stupscr_name = StringHandlers.PascalToString(pString);
				pString = eabr.ReadBytes(16);
				BB.bootup_name = StringHandlers.PascalToString(pString);
				pString = eabr.ReadBytes(16);
				BB.clipbrd_name = StringHandlers.PascalToString(pString);
				
				BB.max_files = eabr.ReadUInt16();
				BB.queue_size = eabr.ReadUInt16();
				BB.heap_128k = eabr.ReadUInt32();
				BB.heap_256k = eabr.ReadUInt32();
				BB.heap_512k = eabr.ReadUInt32();
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
			
			if((MDB.drAtrb & 0x80) == 0x80)
				sb.AppendLine("Volume is locked by hardware.");
			if((MDB.drAtrb & 0x100) == 0x100)
				sb.AppendLine("Volume was unmonted.");
			else
				sb.AppendLine("Volume is mounted.");
			if((MDB.drAtrb & 0x200) == 0x200)
				sb.AppendLine("Volume has spared bad blocks.");
			if((MDB.drAtrb & 0x400) == 0x400)
				sb.AppendLine("Volume does not need cache.");
			if((MDB.drAtrb & 0x800) == 0x800)
				sb.AppendLine("Boot volume is inconsistent.");
			if((MDB.drAtrb & 0x1000) == 0x1000)
				sb.AppendLine("There are reused CNIDs.");
			if((MDB.drAtrb & 0x2000) == 0x2000)
				sb.AppendLine("Volume is journaled.");
			if((MDB.drAtrb & 0x4000) == 0x4000)
				sb.AppendLine("Volume is seriously inconsistent.");
			if((MDB.drAtrb & 0x8000) == 0x8000)
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
			
			if(MDB.drEmbedSigWord == 0x482B)
			{
				sb.AppendLine("Volume wraps a HFS+ volume.");
				sb.AppendFormat("Starting block of the HFS+ volume: {0}", MDB.xdrStABNt).AppendLine();
				sb.AppendFormat("Allocations blocks of the HFS+ volume: {0}", MDB.xdrNumABlks).AppendLine();
			}
			
			if(BB.signature == 0x4C4B)
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
					sb.AppendFormat("Maximum opened files: {0}", BB.max_files*4).AppendLine();
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
		
		private struct HFS_MasterDirectoryBlock // Should be offset 0x0400 bytes in volume
		{
			public UInt16 drSigWord;      // Signature, 0x4244
			public ulong  drCrDate;       // Volume creation date
			public ulong  drLsMod;        // Volume last modification date
			public UInt16 drAtrb;         // Volume attributes
			public UInt16 drNmFls;        // Files in root directory
			public UInt16 drVBMSt;        // Start 512-byte sector of volume bitmap
			public UInt16 drAllocPtr;     // Allocation block to begin next allocation
			public UInt16 drNmAlBlks;     // Allocation blocks
			public UInt32  drAlBlkSiz;     // Bytes per allocation block
			public UInt32  drClpSiz;       // Bytes to allocate when extending a file
			public UInt16 drAlBlSt;       // Start 512-byte sector of first allocation block
			public UInt32  drNxtCNID;      // CNID for next file
			public UInt16 drFreeBks;      // Free allocation blocks
			public string drVN;           // Volume name (28 bytes)
			public ulong  drVolBkUp;      // Volume last backup time
			public UInt16 drVSeqNum;      // Volume backup sequence number
			public UInt32  drWrCnt;        // Filesystem write count
			public UInt32  drXTClpSiz;     // Bytes to allocate when extending the extents B-Tree
			public UInt32  drCTClpSiz;     // Bytes to allocate when extending the catalog B-Tree
			public UInt16 drNmRtDirs;     // Number of directories in root directory
			public UInt32  drFilCnt;       // Number of files in the volume
			public UInt32  drDirCnt;       // Number of directories in the volume
			public UInt32  drFndrInfo0;    // finderInfo[0], CNID for bootable system's directory
			public UInt32  drFndrInfo1;    // finderInfo[1], CNID of the directory containing the boot application
			public UInt32  drFndrInfo2;    // finderInfo[2], CNID of the directory that should be opened on boot
			public UInt32  drFndrInfo3;    // finderInfo[3], CNID for Mac OS 8 or 9 directory
			public UInt32  drFndrInfo4;    // finderInfo[4], Reserved
			public UInt32  drFndrInfo5;    // finderInfo[5], CNID for Mac OS X directory
			public UInt32  drFndrInfo6;    // finderInfo[6], first part of Mac OS X volume ID
			public UInt32  drFndrInfo7;    // finderInfo[7], second part of Mac OS X volume ID
			public UInt16 drEmbedSigWord; // Embedded volume signature, "H+" if HFS+ is embedded ignore following two fields if not
			public UInt16 xdrStABNt;      // Starting block number of embedded HFS+ volume
			public UInt16 xdrNumABlks;    // Allocation blocks used by embedded volume
			public UInt32  drXTFlSize;     // Bytes in the extents B-Tree
			// 3 HFS extents following, 32 bits each
			public UInt32  drCTFlSize;     // Bytes in the catalog B-Tree
			// 3 HFS extents following, 32 bits each
		}
		
		private struct HFS_BootBlock // Should be offset 0x0000 bytes in volume
		{
			public UInt16 signature;    // Signature, 0x4C4B if bootable
			public UInt32  branch;       // Branch
			public byte   boot_flags;   // Boot block flags
			public byte   boot_version; // Boot block version
			public Int16  sec_sv_pages; // Allocate secondary buffers
			public string system_name;  // System file name (10 bytes)
			public string finder_name;  // Finder file name (10 bytes)
			public string debug_name;   // Debugger file name (10 bytes)
			public string disasm_name;  // Disassembler file name (10 bytes)
			public string stupscr_name; // Startup screen file name (10 bytes)
			public string bootup_name;  // First program to execute on boot (10 bytes)
			public string clipbrd_name; // Clipboard file name (10 bytes)
			public UInt16 max_files;    // 1/4 of maximum opened at a time files
			public UInt16 queue_size;   // Event queue size
			public UInt32  heap_128k;    // Heap size on a Mac with 128KiB of RAM
			public UInt32  heap_256k;    // Heap size on a Mac with 256KiB of RAM
			public UInt32  heap_512k;    // Heap size on a Mac with 512KiB of RAM or more
		} // Follows boot code
	}
}

