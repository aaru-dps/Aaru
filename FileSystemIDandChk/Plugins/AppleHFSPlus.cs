using System;
using System.IO;
using System.Text;
using FileSystemIDandChk;

// Information from Inside Macintosh

namespace FileSystemIDandChk.Plugins
{
	class AppleHFSPlus : Plugin
	{
		private DateTime HFSDateDelta = new DateTime(1904, 01, 01, 00, 00, 00);
		
		public AppleHFSPlus(PluginBase Core)
        {
            base.Name = "Apple HFS+ filesystem";
            base.PluginUUID = new Guid("b4ef5baf-0ebe-41cd-866c-1a4bb2b75fd5");
        }
		
		public override bool Identify(FileStream stream, long offset)
		{
			byte[] sixteen_bit = new byte[2];
			byte[] thirtytwo_bit = new byte[4];
			UInt16 drSigWord;
			UInt16 xdrStABNt;
			UInt16 drAlBlSt;
			UInt32 drAlBlkSiz;
			
			ushort signature;
			long hfsp_offset;
			bool wrapped = false;
			
			stream.Seek(0x400 + offset, SeekOrigin.Begin);
			
			stream.Read(sixteen_bit, 0, 2);
			sixteen_bit = Swapping.SwapTwoBytes(sixteen_bit);
			
			drSigWord = BitConverter.ToUInt16(sixteen_bit, 0);
			
			if(drSigWord == 0x4244) // "BD"
			{
				stream.Seek(0x47C + offset, SeekOrigin.Begin); // Seek to embedded HFS+ signature
				stream.Read(sixteen_bit, 0, 2);
				sixteen_bit = Swapping.SwapTwoBytes(sixteen_bit);
				drSigWord = BitConverter.ToUInt16(sixteen_bit, 0);
				
				if(drSigWord == 0x482B) // "H+"
				{
					stream.Read(sixteen_bit, 0, 2);
					sixteen_bit = Swapping.SwapTwoBytes(sixteen_bit);
					xdrStABNt = BitConverter.ToUInt16(sixteen_bit, 0);
					
					stream.Seek(0x414 + offset, SeekOrigin.Begin);
					stream.Read(thirtytwo_bit, 0, 4);
					thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
					drAlBlkSiz = BitConverter.ToUInt32(thirtytwo_bit, 0);
					
					stream.Seek(0x41C + offset, SeekOrigin.Begin);
					stream.Read(sixteen_bit, 0, 2);
					sixteen_bit = Swapping.SwapTwoBytes(sixteen_bit);
					drAlBlSt = BitConverter.ToUInt16(sixteen_bit, 0);
					
					hfsp_offset = (drAlBlSt + xdrStABNt * (drAlBlkSiz / 512))*512;
					wrapped = true;
				}
				else
				{
					hfsp_offset = 0;
					wrapped = false;
				}
			}
			else
			{
				hfsp_offset = 0;
				wrapped = false;
			}
			
			stream.Seek(0x400 + offset + hfsp_offset, SeekOrigin.Begin);
				
			stream.Read(sixteen_bit, 0, 2);
			sixteen_bit = Swapping.SwapTwoBytes(sixteen_bit);
			signature = BitConverter.ToUInt16(sixteen_bit, 0);
			if(signature == 0x482B || signature == 0x4858)
				return true;
			else
				return false;
		}
		
		public override void GetInformation (FileStream stream, long offset, out string information)
		{
			information = "";
			
			byte[] sixteen_bit = new byte[2];
			byte[] thirtytwo_bit = new byte[4];
			byte[] sixtyfour_bit = new byte[8];
			UInt16 drSigWord;
			UInt16 xdrStABNt;
			UInt16 drAlBlSt;
			UInt32 drAlBlkSiz;
			HFSPlusVolumeHeader HPVH = new HFSPlusVolumeHeader();
			
			long hfsp_offset;
			bool wrapped = false;
			
			stream.Seek(0x400 + offset, SeekOrigin.Begin);
			
			stream.Read(sixteen_bit, 0, 2);
			sixteen_bit = Swapping.SwapTwoBytes(sixteen_bit);
			
			drSigWord = BitConverter.ToUInt16(sixteen_bit, 0);
			
			if(drSigWord == 0x4244) // "BD"
			{
				stream.Seek(0x47C + offset, SeekOrigin.Begin); // Seek to embedded HFS+ signature
				stream.Read(sixteen_bit, 0, 2);
				sixteen_bit = Swapping.SwapTwoBytes(sixteen_bit);
				drSigWord = BitConverter.ToUInt16(sixteen_bit, 0);
				
				if(drSigWord == 0x482B) // "H+"
				{
					stream.Read(sixteen_bit, 0, 2);
					sixteen_bit = Swapping.SwapTwoBytes(sixteen_bit);
					xdrStABNt = BitConverter.ToUInt16(sixteen_bit, 0);
					
					stream.Seek(0x414 + offset, SeekOrigin.Begin);
					stream.Read(thirtytwo_bit, 0, 4);
					thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
					drAlBlkSiz = BitConverter.ToUInt32(thirtytwo_bit, 0);
					
					stream.Seek(0x41C + offset, SeekOrigin.Begin);
					stream.Read(sixteen_bit, 0, 2);
					sixteen_bit = Swapping.SwapTwoBytes(sixteen_bit);
					drAlBlSt = BitConverter.ToUInt16(sixteen_bit, 0);
					
					hfsp_offset = (drAlBlSt + xdrStABNt * (drAlBlkSiz / 512))*512;
					wrapped = true;
				}
				else
				{
					hfsp_offset = 0;
					wrapped = false;
				}
			}
			else
			{
				hfsp_offset = 0;
				wrapped = false;
			}
			
			stream.Seek(0x400 + offset + hfsp_offset, SeekOrigin.Begin);
				
			stream.Read(sixteen_bit, 0, 2);
			sixteen_bit = Swapping.SwapTwoBytes(sixteen_bit);
			HPVH.signature = BitConverter.ToUInt16(sixteen_bit, 0);
			if(HPVH.signature == 0x482B || HPVH.signature == 0x4858)
			{
				StringBuilder sb = new StringBuilder();

				if(HPVH.signature == 0x482B)
					sb.AppendLine("HFS+ filesystem.");
				if(HPVH.signature == 0x4858)
					sb.AppendLine("HFSX filesystem.");
				if(wrapped)
					sb.AppendLine("Volume is wrapped inside an HFS volume.");
				
				stream.Read(sixteen_bit, 0, 2);
				sixteen_bit = Swapping.SwapTwoBytes(sixteen_bit);
				HPVH.version = BitConverter.ToUInt16(sixteen_bit, 0);
				
				if(HPVH.version == 4 || HPVH.version == 5)
				{
					stream.Read(thirtytwo_bit, 0, 4);
					thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
					HPVH.attributes = BitConverter.ToUInt32(thirtytwo_bit, 0);						
					stream.Read(thirtytwo_bit, 0, 4);
					HPVH.lastMountedVersion = Encoding.ASCII.GetString(sixteen_bit);						
					stream.Read(thirtytwo_bit, 0, 4);
					thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
					HPVH.journalInfoBlock = BitConverter.ToUInt32(thirtytwo_bit, 0);						

					stream.Read(thirtytwo_bit, 0, 4);
					thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
					HPVH.createDate = BitConverter.ToUInt32(thirtytwo_bit, 0);						
					stream.Read(thirtytwo_bit, 0, 4);
					thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
					HPVH.modifyDate = BitConverter.ToUInt32(thirtytwo_bit, 0);						
					stream.Read(thirtytwo_bit, 0, 4);
					thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
					HPVH.backupDate = BitConverter.ToUInt32(thirtytwo_bit, 0);						
					stream.Read(thirtytwo_bit, 0, 4);
					thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
					HPVH.checkedDate = BitConverter.ToUInt32(thirtytwo_bit, 0);						

					stream.Read(thirtytwo_bit, 0, 4);
					thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
					HPVH.fileCount = BitConverter.ToUInt32(thirtytwo_bit, 0);						
					stream.Read(thirtytwo_bit, 0, 4);
					thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
					HPVH.folderCount = BitConverter.ToUInt32(thirtytwo_bit, 0);						

					stream.Read(thirtytwo_bit, 0, 4);
					thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
					HPVH.blockSize = BitConverter.ToUInt32(thirtytwo_bit, 0);						
					stream.Read(thirtytwo_bit, 0, 4);
					thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
					HPVH.totalBlocks = BitConverter.ToUInt32(thirtytwo_bit, 0);						
					stream.Read(thirtytwo_bit, 0, 4);
					thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
					HPVH.freeBlocks = BitConverter.ToUInt32(thirtytwo_bit, 0);						

					stream.Read(thirtytwo_bit, 0, 4);
					thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
					HPVH.nextAllocation = BitConverter.ToUInt32(thirtytwo_bit, 0);						
					stream.Read(thirtytwo_bit, 0, 4);
					thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
					HPVH.rsrcClumpSize = BitConverter.ToUInt32(thirtytwo_bit, 0);						
					stream.Read(thirtytwo_bit, 0, 4);
					thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
					HPVH.dataClumpSize = BitConverter.ToUInt32(thirtytwo_bit, 0);						
					stream.Read(thirtytwo_bit, 0, 4);
					thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
					HPVH.nextCatalogID = BitConverter.ToUInt32(thirtytwo_bit, 0);						

					stream.Read(thirtytwo_bit, 0, 4);
					thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
					HPVH.writeCount = BitConverter.ToUInt32(thirtytwo_bit, 0);
					stream.Seek(8,SeekOrigin.Current); // Skipping encoding bitmap

					stream.Read(thirtytwo_bit, 0, 4);
					thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
					HPVH.drFndrInfo0 = BitConverter.ToUInt32(thirtytwo_bit, 0);
					stream.Read(thirtytwo_bit, 0, 4);
					thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
					HPVH.drFndrInfo1 = BitConverter.ToUInt32(thirtytwo_bit, 0);
					stream.Read(thirtytwo_bit, 0, 4);
					thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
					HPVH.drFndrInfo2 = BitConverter.ToUInt32(thirtytwo_bit, 0);
					stream.Read(thirtytwo_bit, 0, 4);
					thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
					HPVH.drFndrInfo3 = BitConverter.ToUInt32(thirtytwo_bit, 0);
					stream.Seek(4, SeekOrigin.Current); // Skipping reserved finder info
					stream.Read(thirtytwo_bit, 0, 4);
					thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
					HPVH.drFndrInfo5 = BitConverter.ToUInt32(thirtytwo_bit, 0);
					stream.Read(thirtytwo_bit, 0, 4);
					thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
					HPVH.drFndrInfo6 = BitConverter.ToUInt32(thirtytwo_bit, 0);
					stream.Read(thirtytwo_bit, 0, 4);
					thirtytwo_bit = Swapping.SwapFourBytes(thirtytwo_bit);
					HPVH.drFndrInfo7 = BitConverter.ToUInt32(thirtytwo_bit, 0);
					
					stream.Read(sixtyfour_bit, 0, 8);
					sixtyfour_bit = Swapping.SwapEightBytes(sixtyfour_bit);
					HPVH.allocationFile_logicalSize = BitConverter.ToUInt64(sixtyfour_bit, 0);
					stream.Seek(72, SeekOrigin.Current); // Skip to next file info
					stream.Read(sixtyfour_bit, 0, 8);
					sixtyfour_bit = Swapping.SwapEightBytes(sixtyfour_bit);
					HPVH.extentsFile_logicalSize = BitConverter.ToUInt64(sixtyfour_bit, 0);
					stream.Seek(72, SeekOrigin.Current); // Skip to next file info
					stream.Read(sixtyfour_bit, 0, 8);
					sixtyfour_bit = Swapping.SwapEightBytes(sixtyfour_bit);
					HPVH.catalogFile_logicalSize = BitConverter.ToUInt64(sixtyfour_bit, 0);
					stream.Seek(72, SeekOrigin.Current); // Skip to next file info
					stream.Read(sixtyfour_bit, 0, 8);
					sixtyfour_bit = Swapping.SwapEightBytes(sixtyfour_bit);
					HPVH.attributesFile_logicalSize = BitConverter.ToUInt64(sixtyfour_bit, 0);
					stream.Seek(72, SeekOrigin.Current); // Skip to next file info
					stream.Read(sixtyfour_bit, 0, 8);
					sixtyfour_bit = Swapping.SwapEightBytes(sixtyfour_bit);
					HPVH.startupFile_logicalSize = BitConverter.ToUInt64(sixtyfour_bit, 0);
					
					sb.AppendFormat("Filesystem version is {0}.", HPVH.version).AppendLine();

					if((HPVH.attributes & 0x80) == 0x80)
						sb.AppendLine("Volume is locked on hardware.");
					if((HPVH.attributes & 0x100) == 0x100)
						sb.AppendLine("Volume is unmounted.");
					if((HPVH.attributes & 0x200) == 0x200)
						sb.AppendLine("There are bad blocks in the extents file.");
					if((HPVH.attributes & 0x400) == 0x400)
						sb.AppendLine("Volume does not require cache.");
					if((HPVH.attributes & 0x800) == 0x800)
						sb.AppendLine("Volume state is inconsistent.");
					if((HPVH.attributes & 0x1000) == 0x1000)
						sb.AppendLine("CNIDs are reused.");
					if((HPVH.attributes & 0x2000) == 0x2000)
						sb.AppendLine("Volume is journaled.");
					if((HPVH.attributes & 0x8000) == 0x8000)
						sb.AppendLine("Volume is locked on software.");

					sb.AppendFormat("Implementation that last mounted the volume: \"{0}\".", HPVH.lastMountedVersion).AppendLine();
					if((HPVH.attributes & 0x2000) == 0x2000)
						sb.AppendFormat("Journal starts at allocation block {0}.", HPVH.journalInfoBlock).AppendLine();
					sb.AppendFormat("Creation date: {0}", HFSDateDelta.AddTicks((long)(HPVH.createDate*10000000))).AppendLine();
					sb.AppendFormat("Last modification date: {0}", HFSDateDelta.AddTicks((long)(HPVH.modifyDate*10000000))).AppendLine();
					sb.AppendFormat("Last backup date: {0}", HFSDateDelta.AddTicks((long)(HPVH.backupDate*10000000))).AppendLine();
					sb.AppendFormat("Last check date: {0}", HFSDateDelta.AddTicks((long)(HPVH.checkedDate*10000000))).AppendLine();
					sb.AppendFormat("{0} files on volume.", HPVH.fileCount).AppendLine();
					sb.AppendFormat("{0} folders on volume.", HPVH.folderCount).AppendLine();
					sb.AppendFormat("{0} bytes per allocation block.", HPVH.blockSize).AppendLine();
					sb.AppendFormat("{0} allocation blocks.", HPVH.totalBlocks).AppendLine();
					sb.AppendFormat("{0} free blocks.", HPVH.freeBlocks).AppendLine();
					sb.AppendFormat("Next allocation block: {0}.", HPVH.nextAllocation).AppendLine();
					sb.AppendFormat("Resource fork clump size: {0} bytes.", HPVH.rsrcClumpSize).AppendLine();
					sb.AppendFormat("Data fork clump size: {0} bytes.", HPVH.dataClumpSize).AppendLine();
					sb.AppendFormat("Next unused CNID: {0}.", HPVH.nextCatalogID).AppendLine();
					sb.AppendFormat("Volume has been mounted writable {0} times.", HPVH.writeCount).AppendLine();
					sb.AppendFormat("Allocation File is {0} bytes.", HPVH.allocationFile_logicalSize).AppendLine();
					sb.AppendFormat("Extents File is {0} bytes.", HPVH.extentsFile_logicalSize).AppendLine();
					sb.AppendFormat("Catalog File is {0} bytes.", HPVH.catalogFile_logicalSize).AppendLine();
					sb.AppendFormat("Attributes File is {0} bytes.", HPVH.attributesFile_logicalSize).AppendLine();
					sb.AppendFormat("Startup File is {0} bytes.", HPVH.startupFile_logicalSize).AppendLine();
					sb.AppendLine("Finder info:");
					sb.AppendFormat("CNID of bootable system's directory: {0}", HPVH.drFndrInfo0).AppendLine();
					sb.AppendFormat("CNID of first-run application's directory: {0}", HPVH.drFndrInfo1).AppendLine();
					sb.AppendFormat("CNID of previously opened directory: {0}", HPVH.drFndrInfo2).AppendLine();
					sb.AppendFormat("CNID of bootable Mac OS 8 or 9 directory: {0}", HPVH.drFndrInfo3).AppendLine();
					sb.AppendFormat("CNID of bootable Mac OS X directory: {0}", HPVH.drFndrInfo5).AppendLine();
					sb.AppendFormat("Mac OS X Volume ID: {0:X8}{1:X8}", HPVH.drFndrInfo6, HPVH.drFndrInfo7).AppendLine();
				}
				else
				{
					sb.AppendFormat("Filesystem version is {0}.", HPVH.version).AppendLine();
					sb.AppendLine("This version is not supported yet.");
				}
				
				information = sb.ToString();
			}
			else
				return;
		}
		
		private struct HFSPlusVolumeHeader // Should be offset 0x0400 bytes in volume
		{
			public UInt16 signature;          // "H+" for HFS+, "HX" for HFSX
			public UInt16 version;            // 4 for HFS+, 5 for HFSX
			public UInt32 attributes;         // Volume attributes
			public string lastMountedVersion; // Implementation that last mounted the volume.
											  // Reserved by Apple:
											  // "8.10" Mac OS 8.1 to 9.2.2
											  // "10.0" Mac OS X
											  // "HFSJ" Journaled implementation
											  // "fsck" /sbin/fsck
			public UInt32 journalInfoBlock;   // Allocation block number containing the journal
			 
			public ulong createDate;          // Date of volume creation
			public ulong modifyDate;          // Date of last volume modification
			public ulong backupDate;          // Date of last backup
			public ulong checkedDate;         // Date of last consistency check
			 
			public UInt32 fileCount;          // File on the volume
			public UInt32 folderCount;        // Folders on the volume
			 
			public UInt32 blockSize;          // Bytes per allocation block
			public UInt32 totalBlocks;        // Allocation blocks on the volume
			public UInt32 freeBlocks;         // Free allocation blocks
			 
			public UInt32 nextAllocation;     // Hint for next allocation block
			public UInt32 rsrcClumpSize;      // Resource fork clump size
			public UInt32 dataClumpSize;      // Data fork clump size
			public UInt32 nextCatalogID;      // Next unused CNID
			 
			public UInt32 writeCount;         // Times that the volume has been mounted writable
			public UInt64 encodingsBitmap;    // Used text encoding hints
 
			public UInt32 drFndrInfo0;        // finderInfo[0], CNID for bootable system's directory
			public UInt32 drFndrInfo1;        // finderInfo[1], CNID of the directory containing the boot application
			public UInt32 drFndrInfo2;        // finderInfo[2], CNID of the directory that should be opened on boot
			public UInt32 drFndrInfo3;        // finderInfo[3], CNID for Mac OS 8 or 9 directory
			public UInt32 drFndrInfo4;        // finderInfo[4], Reserved
			public UInt32 drFndrInfo5;        // finderInfo[5], CNID for Mac OS X directory
			public UInt32 drFndrInfo6;        // finderInfo[6], first part of Mac OS X volume ID
			public UInt32 drFndrInfo7;        // finderInfo[7], second part of Mac OS X volume ID
 
			// HFSPlusForkData     allocationFile;
			public UInt64 allocationFile_logicalSize;
			public UInt32 allocationFile_clumpSize;
			public UInt32 allocationFile_totalBlocks;
			public UInt32 allocationFile_extents_startBlock0;
    		public UInt32 allocationFile_extents_blockCount0;
			public UInt32 allocationFile_extents_startBlock1;
    		public UInt32 allocationFile_extents_blockCount1;
			public UInt32 allocationFile_extents_startBlock2;
    		public UInt32 allocationFile_extents_blockCount2;
			public UInt32 allocationFile_extents_startBlock3;
    		public UInt32 allocationFile_extents_blockCount3;
			public UInt32 allocationFile_extents_startBlock4;
    		public UInt32 allocationFile_extents_blockCount4;
			public UInt32 allocationFile_extents_startBlock5;
    		public UInt32 allocationFile_extents_blockCount5;
			public UInt32 allocationFile_extents_startBlock6;
    		public UInt32 allocationFile_extents_blockCount6;
			public UInt32 allocationFile_extents_startBlock7;
    		public UInt32 allocationFile_extents_blockCount7;
			// HFSPlusForkData     extentsFile;
			public UInt64 extentsFile_logicalSize;
			public UInt32 extentsFile_clumpSize;
			public UInt32 extentsFile_totalBlocks;
			public UInt32 extentsFile_extents_startBlock0;
    		public UInt32 extentsFile_extents_blockCount0;
			public UInt32 extentsFile_extents_startBlock1;
    		public UInt32 extentsFile_extents_blockCount1;
			public UInt32 extentsFile_extents_startBlock2;
    		public UInt32 extentsFile_extents_blockCount2;
			public UInt32 extentsFile_extents_startBlock3;
    		public UInt32 extentsFile_extents_blockCount3;
			public UInt32 extentsFile_extents_startBlock4;
    		public UInt32 extentsFile_extents_blockCount4;
			public UInt32 extentsFile_extents_startBlock5;
    		public UInt32 extentsFile_extents_blockCount5;
			public UInt32 extentsFile_extents_startBlock6;
    		public UInt32 extentsFile_extents_blockCount6;
			public UInt32 extentsFile_extents_startBlock7;
    		public UInt32 extentsFile_extents_blockCount7;
			// HFSPlusForkData     catalogFile;
			public UInt64 catalogFile_logicalSize;
			public UInt32 catalogFile_clumpSize;
			public UInt32 catalogFile_totalBlocks;
			public UInt32 catalogFile_extents_startBlock0;
    		public UInt32 catalogFile_extents_blockCount0;
			public UInt32 catalogFile_extents_startBlock1;
    		public UInt32 catalogFile_extents_blockCount1;
			public UInt32 catalogFile_extents_startBlock2;
    		public UInt32 catalogFile_extents_blockCount2;
			public UInt32 catalogFile_extents_startBlock3;
    		public UInt32 catalogFile_extents_blockCount3;
			public UInt32 catalogFile_extents_startBlock4;
    		public UInt32 catalogFile_extents_blockCount4;
			public UInt32 catalogFile_extents_startBlock5;
    		public UInt32 catalogFile_extents_blockCount5;
			public UInt32 catalogFile_extents_startBlock6;
    		public UInt32 catalogFile_extents_blockCount6;
			public UInt32 catalogFile_extents_startBlock7;
    		public UInt32 catalogFile_extents_blockCount7;
			// HFSPlusForkData     attributesFile;
			public UInt64 attributesFile_logicalSize;
			public UInt32 attributesFile_clumpSize;
			public UInt32 attributesFile_totalBlocks;
			public UInt32 attributesFile_extents_startBlock0;
    		public UInt32 attributesFile_extents_blockCount0;
			public UInt32 attributesFile_extents_startBlock1;
    		public UInt32 attributesFile_extents_blockCount1;
			public UInt32 attributesFile_extents_startBlock2;
    		public UInt32 attributesFile_extents_blockCount2;
			public UInt32 attributesFile_extents_startBlock3;
    		public UInt32 attributesFile_extents_blockCount3;
			public UInt32 attributesFile_extents_startBlock4;
    		public UInt32 attributesFile_extents_blockCount4;
			public UInt32 attributesFile_extents_startBlock5;
    		public UInt32 attributesFile_extents_blockCount5;
			public UInt32 attributesFile_extents_startBlock6;
    		public UInt32 attributesFile_extents_blockCount6;
			public UInt32 attributesFile_extents_startBlock7;
    		public UInt32 attributesFile_extents_blockCount7;
			// HFSPlusForkData     startupFile;
			public UInt64 startupFile_logicalSize;
			public UInt32 startupFile_clumpSize;
			public UInt32 startupFile_totalBlocks;
			public UInt32 startupFile_extents_startBlock0;
    		public UInt32 startupFile_extents_blockCount0;
			public UInt32 startupFile_extents_startBlock1;
    		public UInt32 startupFile_extents_blockCount1;
			public UInt32 startupFile_extents_startBlock2;
    		public UInt32 startupFile_extents_blockCount2;
			public UInt32 startupFile_extents_startBlock3;
    		public UInt32 startupFile_extents_blockCount3;
			public UInt32 startupFile_extents_startBlock4;
    		public UInt32 startupFile_extents_blockCount4;
			public UInt32 startupFile_extents_startBlock5;
    		public UInt32 startupFile_extents_blockCount5;
			public UInt32 startupFile_extents_startBlock6;
    		public UInt32 startupFile_extents_blockCount6;
			public UInt32 startupFile_extents_startBlock7;
    		public UInt32 startupFile_extents_blockCount7;
		}
	}
}

