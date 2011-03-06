using System;
using System.IO;
using System.Text;
using FileSystemIDandChk;

// Information from Inside Macintosh

namespace FileSystemIDandChk.Plugins
{
	class AppleMFS : Plugin
	{
		private DateTime MFSDateDelta = new DateTime(1904, 01, 01, 00, 00, 00);
		
		public AppleMFS(PluginBase Core)
        {
            base.Name = "Apple Macintosh File System";
            base.PluginUUID = new Guid("67591456-90fa-49bd-ac89-14ef750b8af3");
        }
		
		public override bool Identify(FileStream stream, long offset)
		{
			byte[] signature = new byte[2];
			ushort drSigWord;
			
			stream.Seek(0x400 + offset, SeekOrigin.Begin);
			
			stream.Read(signature, 0, 2);
			signature = Swapping.SwapTwoBytes(signature);
			
			drSigWord = BitConverter.ToUInt16(signature, 0);
			
			if(drSigWord == 0xD2D7)
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
			
			byte[] sixteen_bit = new byte[2];
			byte[] thirtytwo_bit = new byte[4];
			byte[] fifthteen_bytes = new byte[15];
			byte[] variable_size;
			
			stream.Seek(0x400 + offset, SeekOrigin.Begin);
			stream.Read(sixteen_bit, 0, 2);
			sixteen_bit = Swapping.SwapTwoBytes(sixteen_bit);
			MDB.drSigWord = BitConverter.ToUInt16(sixteen_bit, 0);
			if(MDB.drSigWord != 0xD2D7)
				return;
			
			stream.Read(thirtytwo_bit, 0, 4);
			thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
			MDB.drCrDate = BitConverter.ToUInt32(thirtytwo_bit, 0);
			stream.Read(thirtytwo_bit, 0, 4);
			thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
			MDB.drLsBkUp = BitConverter.ToUInt32(thirtytwo_bit, 0);
			stream.Read(sixteen_bit, 0, 2);
			sixteen_bit = Swapping.SwapTwoBytes(sixteen_bit);
			MDB.drAtrb = BitConverter.ToUInt16(sixteen_bit, 0);
			stream.Read(sixteen_bit, 0, 2);
			sixteen_bit = Swapping.SwapTwoBytes(sixteen_bit);
			MDB.drNmFls = BitConverter.ToUInt16(sixteen_bit, 0);
			stream.Read(sixteen_bit, 0, 2);
			sixteen_bit = Swapping.SwapTwoBytes(sixteen_bit);
			MDB.drDirSt = BitConverter.ToUInt16(sixteen_bit, 0);
			stream.Read(sixteen_bit, 0, 2);
			sixteen_bit = Swapping.SwapTwoBytes(sixteen_bit);
			MDB.drBlLen = BitConverter.ToUInt16(sixteen_bit, 0);
			stream.Read(sixteen_bit, 0, 2);
			sixteen_bit = Swapping.SwapTwoBytes(sixteen_bit);
			MDB.drNmAlBlks = BitConverter.ToUInt16(sixteen_bit, 0);
			stream.Read(thirtytwo_bit, 0, 4);
			thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
			MDB.drAlBlkSiz = BitConverter.ToUInt32(thirtytwo_bit, 0);
			stream.Read(thirtytwo_bit, 0, 4);
			thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
			MDB.drClpSiz = BitConverter.ToUInt32(thirtytwo_bit, 0);
			stream.Read(sixteen_bit, 0, 2);
			sixteen_bit = Swapping.SwapTwoBytes(sixteen_bit);
			MDB.drAlBlSt = BitConverter.ToUInt16(sixteen_bit, 0);
			stream.Read(thirtytwo_bit, 0, 4);
			thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
			MDB.drNxtFNum = BitConverter.ToUInt32(thirtytwo_bit, 0);
			stream.Read(sixteen_bit, 0, 2);
			sixteen_bit = Swapping.SwapTwoBytes(sixteen_bit);
			MDB.drFreeBks = BitConverter.ToUInt16(sixteen_bit, 0);
			MDB.drVNSiz = (byte)stream.ReadByte();
			variable_size = new byte[MDB.drVNSiz];
			stream.Read(variable_size, 0, MDB.drVNSiz);
			MDB.drVN = Encoding.ASCII.GetString(variable_size);
			
			stream.Seek(0 + offset, SeekOrigin.Begin);
			stream.Read(sixteen_bit, 0, 2);
			sixteen_bit = Swapping.SwapTwoBytes(sixteen_bit);
			BB.signature = BitConverter.ToUInt16(sixteen_bit, 0);
			
			if(BB.signature == 0x4C4B)
			{
				stream.Read(thirtytwo_bit, 0, 4);
				thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
				BB.branch = BitConverter.ToUInt32(thirtytwo_bit, 0);
				BB.boot_flags = (byte)stream.ReadByte();
				BB.boot_version = (byte)stream.ReadByte();
				
				stream.Read(sixteen_bit, 0, 2);
				sixteen_bit = Swapping.SwapTwoBytes(sixteen_bit);
				BB.sec_sv_pages = BitConverter.ToInt16(sixteen_bit, 0);
				
				stream.Seek(1, SeekOrigin.Current);
				stream.Read(fifthteen_bytes, 0, 15);
				BB.system_name = Encoding.ASCII.GetString(fifthteen_bytes);
				stream.Seek(1, SeekOrigin.Current);
				stream.Read(fifthteen_bytes, 0, 15);
				BB.finder_name = Encoding.ASCII.GetString(fifthteen_bytes);
				stream.Seek(1, SeekOrigin.Current);
				stream.Read(fifthteen_bytes, 0, 15);
				BB.debug_name = Encoding.ASCII.GetString(fifthteen_bytes);
				stream.Seek(1, SeekOrigin.Current);
				stream.Read(fifthteen_bytes, 0, 15);
				BB.disasm_name = Encoding.ASCII.GetString(fifthteen_bytes);
				stream.Seek(1, SeekOrigin.Current);
				stream.Read(fifthteen_bytes, 0, 15);
				BB.stupscr_name = Encoding.ASCII.GetString(fifthteen_bytes);
				stream.Seek(1, SeekOrigin.Current);
				stream.Read(fifthteen_bytes, 0, 15);
				BB.bootup_name = Encoding.ASCII.GetString(fifthteen_bytes);
				stream.Seek(1, SeekOrigin.Current);
				stream.Read(fifthteen_bytes, 0, 15);
				BB.clipbrd_name = Encoding.ASCII.GetString(fifthteen_bytes);
				
				stream.Read(sixteen_bit, 0, 2);
				sixteen_bit = Swapping.SwapTwoBytes(sixteen_bit);
				BB.max_files = BitConverter.ToUInt16(sixteen_bit, 0);
				stream.Read(sixteen_bit, 0, 2);
				sixteen_bit = Swapping.SwapTwoBytes(sixteen_bit);
				BB.queue_size = BitConverter.ToUInt16(sixteen_bit, 0);
				stream.Read(thirtytwo_bit, 0, 4);
				thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
				BB.heap_128k = BitConverter.ToUInt32(thirtytwo_bit, 0);
				stream.Read(thirtytwo_bit, 0, 4);
				thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
				BB.heap_256k = BitConverter.ToUInt32(thirtytwo_bit, 0);
				stream.Read(thirtytwo_bit, 0, 4);
				thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
				BB.heap_512k = BitConverter.ToUInt32(thirtytwo_bit, 0);
			}
			else
				BB.signature = 0x0000;
			
			sb.AppendLine("Apple Macintosh File System");
			sb.AppendLine();
			sb.AppendLine("Master Directory Block:");
			sb.AppendFormat("Creation date: {0}", MFSDateDelta.AddTicks((long)(MDB.drCrDate*10000000))).AppendLine();
			sb.AppendFormat("Last backup date: {0}", MFSDateDelta.AddTicks((long)(MDB.drLsBkUp*10000000))).AppendLine();
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
		
		private struct MFS_MasterDirectoryBlock // Should be offset 0x0400 bytes in volume
		{
			public UInt16 drSigWord;  // Signature, 0xD2D7
			public ulong drCrDate;   // Volume creation date
			public ulong drLsBkUp;   // Volume last backup date
			public UInt16 drAtrb;     // Volume attributes
			public UInt16 drNmFls;    // Volume number of files
			public UInt16 drDirSt;    // First directory block
			public UInt16 drBlLen;    // Length of directory in blocks
			public UInt16 drNmAlBlks; // Volume allocation blocks
			public UInt32 drAlBlkSiz; // Size of allocation blocks
			public UInt32 drClpSiz;   // Number of bytes to allocate
			public UInt16 drAlBlSt;   // First allocation block in block map
			public UInt32 drNxtFNum;  // Next unused file number
			public UInt16 drFreeBks;  // Number of unused allocation blocks
			public byte   drVNSiz;    // Length of volume name
			public string drVN;       // Characters of volume name
		}
		
		private struct MFS_BootBlock // Should be offset 0x0000 bytes in volume
		{
			public UInt16 signature;    // Signature, 0x4C4B if bootable
			public UInt32 branch;       // Branch
			public byte   boot_flags;   // Boot block flags
			public byte   boot_version; // Boot block version
			public short  sec_sv_pages; // Allocate secondary buffers
			public string system_name;  // System file name (10 bytes)
			public string finder_name;  // Finder file name (10 bytes)
			public string debug_name;   // Debugger file name (10 bytes)
			public string disasm_name;  // Disassembler file name (10 bytes)
			public string stupscr_name; // Startup screen file name (10 bytes)
			public string bootup_name;  // First program to execute on boot (10 bytes)
			public string clipbrd_name; // Clipboard file name (10 bytes)
			public UInt16 max_files;    // 1/4 of maximum opened at a time files
			public UInt16 queue_size;   // Event queue size
			public UInt32 heap_128k;    // Heap size on a Mac with 128KiB of RAM
			public UInt32 heap_256k;    // Heap size on a Mac with 256KiB of RAM
			public UInt32 heap_512k;    // Heap size on a Mac with 512KiB of RAM or more
		} // Follows boot code
	}
}

