using System;
using System.IO;
using System.Text;
using FileSystemIDandChk;

// Information from Inside Macintosh
// TODO: Implement support for disc images
/*
namespace FileSystemIDandChk.Plugins
{
	class AppleMFS : Plugin
	{
		private const UInt16 MFS_MAGIC   = 0xD2D7;
		private const UInt16 MFSBB_MAGIC = 0x4C4B; // "LK"

		public AppleMFS(PluginBase Core)
        {
            base.Name = "Apple Macintosh File System";
			base.PluginUUID = new Guid("36405F8D-0D26-4066-6538-5DBF5D065C3A");
        }
		
		public override bool Identify(FileStream stream, long offset)
		{
			UInt16 drSigWord;

			EndianAwareBinaryReader eabr = new EndianAwareBinaryReader(stream, false); // BigEndian
			eabr.BaseStream.Seek(0x400 + offset, SeekOrigin.Begin);
			
			drSigWord = eabr.ReadUInt16();
			
			if(drSigWord == MFS_MAGIC)
				return true;
			else
				return false;
		}
		
		public override void GetInformation (FileStream stream, long offset, out string information)
		{
			information = "";
			
			StringBuilder sb = new StringBuilder();
			
			MFS_MasterDirectoryBlock MDB = new MFS_MasterDirectoryBlock();
			MFS_BootBlock BB = new MFS_BootBlock();
			
			byte[] pString;
			byte[] variable_size;

			EndianAwareBinaryReader eabr = new EndianAwareBinaryReader(stream, false); // BigEndian
			eabr.BaseStream.Seek(0x400 + offset, SeekOrigin.Begin);
			MDB.drSigWord = eabr.ReadUInt16();
			if(MDB.drSigWord != MFS_MAGIC)
				return;
			
			MDB.drCrDate = eabr.ReadUInt32();
			MDB.drLsBkUp = eabr.ReadUInt32();
			MDB.drAtrb = eabr.ReadUInt16();
			MDB.drNmFls = eabr.ReadUInt16();
			MDB.drDirSt = eabr.ReadUInt16();
			MDB.drBlLen = eabr.ReadUInt16();
			MDB.drNmAlBlks = eabr.ReadUInt16();
			MDB.drAlBlkSiz = eabr.ReadUInt32();
			MDB.drClpSiz = eabr.ReadUInt32();
			MDB.drAlBlSt = eabr.ReadUInt16();
			MDB.drNxtFNum = eabr.ReadUInt32();
			MDB.drFreeBks = eabr.ReadUInt16();
			MDB.drVNSiz = eabr.ReadByte();
			variable_size = eabr.ReadBytes(MDB.drVNSiz);
			MDB.drVN = Encoding.ASCII.GetString(variable_size);
			
			eabr.BaseStream.Seek(0 + offset, SeekOrigin.Begin);
			BB.signature = eabr.ReadUInt16();
			
			if(BB.signature == MFSBB_MAGIC)
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
		
		private struct MFS_MasterDirectoryBlock // Should be offset 0x0400 bytes in volume
		{
			public UInt16 drSigWord;  // 0x000, Signature, 0xD2D7
			public ulong  drCrDate;   // 0x002, Volume creation date
			public ulong  drLsBkUp;   // 0x00A, Volume last backup date
			public UInt16 drAtrb;     // 0x012, Volume attributes
			public UInt16 drNmFls;    // 0x014, Volume number of files
			public UInt16 drDirSt;    // 0x016, First directory block
			public UInt16 drBlLen;    // 0x018, Length of directory in blocks
			public UInt16 drNmAlBlks; // 0x01A, Volume allocation blocks
			public UInt32 drAlBlkSiz; // 0x01C, Size of allocation blocks
			public UInt32 drClpSiz;   // 0x020, Number of bytes to allocate
			public UInt16 drAlBlSt;   // 0x024, First allocation block in block map
			public UInt32 drNxtFNum;  // 0x026. Next unused file number
			public UInt16 drFreeBks;  // 0x02A, Number of unused allocation blocks
			public byte   drVNSiz;    // 0x02C, Length of volume name
			public string drVN;       // 0x02D, Characters of volume name
		}
		
		private struct MFS_BootBlock // Should be offset 0x0000 bytes in volume
		{
			public UInt16 signature;    // 0x000, Signature, 0x4C4B if bootable
			public UInt32 branch;       // 0x002, Branch
			public byte   boot_flags;   // 0x006, Boot block flags
			public byte   boot_version; // 0x007, Boot block version
			public short  sec_sv_pages; // 0x008, Allocate secondary buffers
			public string system_name;  // 0x00A, System file name (10 bytes)
			public string finder_name;  // 0x014, Finder file name (10 bytes)
			public string debug_name;   // 0x01E, Debugger file name (10 bytes)
			public string disasm_name;  // 0x028, Disassembler file name (10 bytes)
			public string stupscr_name; // 0x032, Startup screen file name (10 bytes)
			public string bootup_name;  // 0x03C, First program to execute on boot (10 bytes)
			public string clipbrd_name; // 0x046, Clipboard file name (10 bytes)
			public UInt16 max_files;    // 0x050, 1/4 of maximum opened at a time files
			public UInt16 queue_size;   // 0x052, Event queue size
			public UInt32 heap_128k;    // 0x054, Heap size on a Mac with 128KiB of RAM
			public UInt32 heap_256k;    // 0x058, Heap size on a Mac with 256KiB of RAM
			public UInt32 heap_512k;    // 0x05C, Heap size on a Mac with 512KiB of RAM or more
		} // Follows boot code
	}
}
*/
