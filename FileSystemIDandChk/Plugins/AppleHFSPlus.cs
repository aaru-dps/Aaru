using System;
using System.Text;
using FileSystemIDandChk;

// Information from Apple TechNote 1150: https://developer.apple.com/legacy/library/technotes/tn/tn1150.html
namespace FileSystemIDandChk.Plugins
{
    class AppleHFSPlus : Plugin
    {
        const UInt16 HFS_MAGIC = 0x4244;
        // "BD"
        const UInt16 HFSP_MAGIC = 0x482B;
        // "H+"
        const UInt16 HFSX_MAGIC = 0x4858;
        // "HX"
        public AppleHFSPlus(PluginBase Core)
        {
            Name = "Apple HFS+ filesystem";
            PluginUUID = new Guid("36405F8D-0D26-6EBE-436F-62F0586B4F08");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset)
        {
            UInt16 drSigWord;
            UInt16 xdrStABNt;
            UInt16 drAlBlSt;
            UInt32 drAlBlkSiz;
			
            byte[] vh_sector;
            ulong hfsp_offset;

            vh_sector = imagePlugin.ReadSector(2 + partitionOffset); // Read volume header, of HFS Wrapper MDB
			
            drSigWord = BigEndianBitConverter.ToUInt16(vh_sector, 0); // Check for HFS Wrapper MDB
			
            if (drSigWord == HFS_MAGIC) // "BD"
            {
                drSigWord = BigEndianBitConverter.ToUInt16(vh_sector, 0x07C); // Read embedded HFS+ signature
				
                if (drSigWord == HFSP_MAGIC) // "H+"
                {
                    xdrStABNt = BigEndianBitConverter.ToUInt16(vh_sector, 0x07E); // Starting block number of embedded HFS+ volume
					
                    drAlBlkSiz = BigEndianBitConverter.ToUInt32(vh_sector, 0x014); // Block size
					
                    drAlBlSt = BigEndianBitConverter.ToUInt16(vh_sector, 0x01C); // Start of allocated blocks (in 512-byte/block)
					
                    hfsp_offset = (drAlBlSt + xdrStABNt * (drAlBlkSiz / 512)) * (imagePlugin.GetSectorSize() / 512);
                }
                else
                {
                    hfsp_offset = 0;
                }
            }
            else
            {
                hfsp_offset = 0;
            }
			
            vh_sector = imagePlugin.ReadSector(2 + partitionOffset + hfsp_offset); // Read volume header
				
            drSigWord = BigEndianBitConverter.ToUInt16(vh_sector, 0);
            if (drSigWord == HFSP_MAGIC || drSigWord == HFSX_MAGIC)
                return true;
            return false;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset, out string information)
        {
            information = "";
			
            UInt16 drSigWord;
            UInt16 xdrStABNt;
            UInt16 drAlBlSt;
            UInt32 drAlBlkSiz;
            HFSPlusVolumeHeader HPVH = new HFSPlusVolumeHeader();
			
            ulong hfsp_offset;
            bool wrapped;
            byte[] vh_sector;
			
            vh_sector = imagePlugin.ReadSector(2 + partitionOffset); // Read volume header, of HFS Wrapper MDB

            drSigWord = BigEndianBitConverter.ToUInt16(vh_sector, 0); // Check for HFS Wrapper MDB
			
            if (drSigWord == HFS_MAGIC) // "BD"
            {
                drSigWord = BigEndianBitConverter.ToUInt16(vh_sector, 0x07C); // Read embedded HFS+ signature
				
                if (drSigWord == HFSP_MAGIC) // "H+"
                {
                    xdrStABNt = BigEndianBitConverter.ToUInt16(vh_sector, 0x07E); // Starting block number of embedded HFS+ volume

                    drAlBlkSiz = BigEndianBitConverter.ToUInt32(vh_sector, 0x014); // Block size

                    drAlBlSt = BigEndianBitConverter.ToUInt16(vh_sector, 0x01C); // Start of allocated blocks (in 512-byte/block)

                    hfsp_offset = (drAlBlSt + xdrStABNt * (drAlBlkSiz / 512)) * (imagePlugin.GetSectorSize() / 512);
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
			
            vh_sector = imagePlugin.ReadSector(2 + partitionOffset + hfsp_offset); // Read volume header
				
            HPVH.signature = BigEndianBitConverter.ToUInt16(vh_sector, 0x000);
            if (HPVH.signature == HFSP_MAGIC || HPVH.signature == HFSX_MAGIC)
            {
                StringBuilder sb = new StringBuilder();

                if (HPVH.signature == 0x482B)
                    sb.AppendLine("HFS+ filesystem.");
                if (HPVH.signature == 0x4858)
                    sb.AppendLine("HFSX filesystem.");
                if (wrapped)
                    sb.AppendLine("Volume is wrapped inside an HFS volume.");
				
                HPVH.version = BigEndianBitConverter.ToUInt16(vh_sector, 0x002);
				
                if (HPVH.version == 4 || HPVH.version == 5)
                {
                    HPVH.attributes = BigEndianBitConverter.ToUInt32(vh_sector, 0x004);
                    byte[] lastMountedVersion_b = new byte[4];
                    Array.Copy(vh_sector, 0x008, lastMountedVersion_b, 0, 4);
                    HPVH.lastMountedVersion = Encoding.ASCII.GetString(lastMountedVersion_b);						
                    HPVH.journalInfoBlock = BigEndianBitConverter.ToUInt32(vh_sector, 0x00C);						

                    HPVH.createDate = BigEndianBitConverter.ToUInt32(vh_sector, 0x010);						
                    HPVH.modifyDate = BigEndianBitConverter.ToUInt32(vh_sector, 0x018);						
                    HPVH.backupDate = BigEndianBitConverter.ToUInt32(vh_sector, 0x020);						
                    HPVH.checkedDate = BigEndianBitConverter.ToUInt32(vh_sector, 0x028);						

                    HPVH.fileCount = BigEndianBitConverter.ToUInt32(vh_sector, 0x030);						
                    HPVH.folderCount = BigEndianBitConverter.ToUInt32(vh_sector, 0x034);						

                    HPVH.blockSize = BigEndianBitConverter.ToUInt32(vh_sector, 0x038);						
                    HPVH.totalBlocks = BigEndianBitConverter.ToUInt32(vh_sector, 0x03C);						
                    HPVH.freeBlocks = BigEndianBitConverter.ToUInt32(vh_sector, 0x040);						

                    HPVH.nextAllocation = BigEndianBitConverter.ToUInt32(vh_sector, 0x044);						
                    HPVH.rsrcClumpSize = BigEndianBitConverter.ToUInt32(vh_sector, 0x048);						
                    HPVH.dataClumpSize = BigEndianBitConverter.ToUInt32(vh_sector, 0x04C);						
                    HPVH.nextCatalogID = BigEndianBitConverter.ToUInt32(vh_sector, 0x050);						

                    HPVH.writeCount = BigEndianBitConverter.ToUInt32(vh_sector, 0x054);

                    HPVH.drFndrInfo0 = BigEndianBitConverter.ToUInt32(vh_sector, 0x060);
                    HPVH.drFndrInfo1 = BigEndianBitConverter.ToUInt32(vh_sector, 0x064);
                    HPVH.drFndrInfo2 = BigEndianBitConverter.ToUInt32(vh_sector, 0x068);
                    HPVH.drFndrInfo3 = BigEndianBitConverter.ToUInt32(vh_sector, 0x06C);
                    HPVH.drFndrInfo5 = BigEndianBitConverter.ToUInt32(vh_sector, 0x074);
                    HPVH.drFndrInfo6 = BigEndianBitConverter.ToUInt32(vh_sector, 0x078);
                    HPVH.drFndrInfo7 = BigEndianBitConverter.ToUInt32(vh_sector, 0x07C);
					
                    HPVH.allocationFile_logicalSize = BigEndianBitConverter.ToUInt64(vh_sector, 0x080);
                    HPVH.extentsFile_logicalSize = BigEndianBitConverter.ToUInt64(vh_sector, 0x0D0);
                    HPVH.catalogFile_logicalSize = BigEndianBitConverter.ToUInt64(vh_sector, 0x120);
                    HPVH.attributesFile_logicalSize = BigEndianBitConverter.ToUInt64(vh_sector, 0x170);
                    HPVH.startupFile_logicalSize = BigEndianBitConverter.ToUInt64(vh_sector, 0x1C0);
					
                    sb.AppendFormat("Filesystem version is {0}.", HPVH.version).AppendLine();

                    if ((HPVH.attributes & 0x80) == 0x80)
                        sb.AppendLine("Volume is locked on hardware.");
                    if ((HPVH.attributes & 0x100) == 0x100)
                        sb.AppendLine("Volume is unmounted.");
                    if ((HPVH.attributes & 0x200) == 0x200)
                        sb.AppendLine("There are bad blocks in the extents file.");
                    if ((HPVH.attributes & 0x400) == 0x400)
                        sb.AppendLine("Volume does not require cache.");
                    if ((HPVH.attributes & 0x800) == 0x800)
                        sb.AppendLine("Volume state is inconsistent.");
                    if ((HPVH.attributes & 0x1000) == 0x1000)
                        sb.AppendLine("CNIDs are reused.");
                    if ((HPVH.attributes & 0x2000) == 0x2000)
                        sb.AppendLine("Volume is journaled.");
                    if ((HPVH.attributes & 0x8000) == 0x8000)
                        sb.AppendLine("Volume is locked on software.");

                    sb.AppendFormat("Implementation that last mounted the volume: \"{0}\".", HPVH.lastMountedVersion).AppendLine();
                    if ((HPVH.attributes & 0x2000) == 0x2000)
                        sb.AppendFormat("Journal starts at allocation block {0}.", HPVH.journalInfoBlock).AppendLine();
                    sb.AppendFormat("Creation date: {0}", DateHandlers.MacToDateTime(HPVH.createDate)).AppendLine();
                    sb.AppendFormat("Last modification date: {0}", DateHandlers.MacToDateTime(HPVH.modifyDate)).AppendLine();
                    sb.AppendFormat("Last backup date: {0}", DateHandlers.MacToDateTime(HPVH.backupDate)).AppendLine();
                    sb.AppendFormat("Last check date: {0}", DateHandlers.MacToDateTime(HPVH.checkedDate)).AppendLine();
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
        // Size = 532 bytes
        struct HFSPlusVolumeHeader // Should be offset 0x0400 bytes in volume
        {
            public UInt16 signature;
            // 0x000, "H+" for HFS+, "HX" for HFSX
            public UInt16 version;
            // 0x002, 4 for HFS+, 5 for HFSX
            public UInt32 attributes;
            // 0x004, Volume attributes
            public string lastMountedVersion;
            // 0x008, Implementation that last mounted the volume.
            // Reserved by Apple:
            // "8.10" Mac OS 8.1 to 9.2.2
            // "10.0" Mac OS X
            // "HFSJ" Journaled implementation
            // "fsck" /sbin/fsck
            public UInt32 journalInfoBlock;
            // 0x00C, Allocation block number containing the journal
            public ulong createDate;
            // 0x010, Date of volume creation
            public ulong modifyDate;
            // 0x018, Date of last volume modification
            public ulong backupDate;
            // 0x020, Date of last backup
            public ulong checkedDate;
            // 0x028, Date of last consistency check
            public UInt32 fileCount;
            // 0x030, File on the volume
            public UInt32 folderCount;
            // 0x034, Folders on the volume
            public UInt32 blockSize;
            // 0x038, Bytes per allocation block
            public UInt32 totalBlocks;
            // 0x03C, Allocation blocks on the volume
            public UInt32 freeBlocks;
            // 0x040, Free allocation blocks
            public UInt32 nextAllocation;
            // 0x044, Hint for next allocation block
            public UInt32 rsrcClumpSize;
            // 0x048, Resource fork clump size
            public UInt32 dataClumpSize;
            // 0x04C, Data fork clump size
            public UInt32 nextCatalogID;
            // 0x050, Next unused CNID
            public UInt32 writeCount;
            // 0x054, Times that the volume has been mounted writable
            public UInt64 encodingsBitmap;
            // 0x058, Used text encoding hints
            public UInt32 drFndrInfo0;
            // 0x060, finderInfo[0], CNID for bootable system's directory
            public UInt32 drFndrInfo1;
            // 0x064, finderInfo[1], CNID of the directory containing the boot application
            public UInt32 drFndrInfo2;
            // 0x068, finderInfo[2], CNID of the directory that should be opened on boot
            public UInt32 drFndrInfo3;
            // 0x06C, finderInfo[3], CNID for Mac OS 8 or 9 directory
            public UInt32 drFndrInfo4;
            // 0x070, finderInfo[4], Reserved
            public UInt32 drFndrInfo5;
            // 0x074, finderInfo[5], CNID for Mac OS X directory
            public UInt32 drFndrInfo6;
            // 0x078, finderInfo[6], first part of Mac OS X volume ID
            public UInt32 drFndrInfo7;
            // 0x07C, finderInfo[7], second part of Mac OS X volume ID
            // HFSPlusForkData     allocationFile;
            public UInt64 allocationFile_logicalSize;
            // 0x080
            public UInt32 allocationFile_clumpSize;
            // 0x088
            public UInt32 allocationFile_totalBlocks;
            // 0x08C
            public UInt32 allocationFile_extents_startBlock0;
            // 0x090
            public UInt32 allocationFile_extents_blockCount0;
            // 0x094
            public UInt32 allocationFile_extents_startBlock1;
            // 0x098
            public UInt32 allocationFile_extents_blockCount1;
            // 0x09C
            public UInt32 allocationFile_extents_startBlock2;
            // 0x0A0
            public UInt32 allocationFile_extents_blockCount2;
            // 0x0A4
            public UInt32 allocationFile_extents_startBlock3;
            // 0x0A8
            public UInt32 allocationFile_extents_blockCount3;
            // 0x0AC
            public UInt32 allocationFile_extents_startBlock4;
            // 0x0B0
            public UInt32 allocationFile_extents_blockCount4;
            // 0x0B4
            public UInt32 allocationFile_extents_startBlock5;
            // 0x0B8
            public UInt32 allocationFile_extents_blockCount5;
            // 0x0BC
            public UInt32 allocationFile_extents_startBlock6;
            // 0x0C0
            public UInt32 allocationFile_extents_blockCount6;
            // 0x0C4
            public UInt32 allocationFile_extents_startBlock7;
            // 0x0C8
            public UInt32 allocationFile_extents_blockCount7;
            // 0x0CC
            // HFSPlusForkData     extentsFile;
            public UInt64 extentsFile_logicalSize;
            // 0x0D0
            public UInt32 extentsFile_clumpSize;
            // 0x0D8
            public UInt32 extentsFile_totalBlocks;
            // 0x0DC
            public UInt32 extentsFile_extents_startBlock0;
            // 0x0E0
            public UInt32 extentsFile_extents_blockCount0;
            // 0x0E4
            public UInt32 extentsFile_extents_startBlock1;
            // 0x0E8
            public UInt32 extentsFile_extents_blockCount1;
            // 0x0EC
            public UInt32 extentsFile_extents_startBlock2;
            // 0x0F0
            public UInt32 extentsFile_extents_blockCount2;
            // 0x0F4
            public UInt32 extentsFile_extents_startBlock3;
            // 0x0F8
            public UInt32 extentsFile_extents_blockCount3;
            // 0x0FC
            public UInt32 extentsFile_extents_startBlock4;
            // 0x100
            public UInt32 extentsFile_extents_blockCount4;
            // 0x104
            public UInt32 extentsFile_extents_startBlock5;
            // 0x108
            public UInt32 extentsFile_extents_blockCount5;
            // 0x10C
            public UInt32 extentsFile_extents_startBlock6;
            // 0x110
            public UInt32 extentsFile_extents_blockCount6;
            // 0x114
            public UInt32 extentsFile_extents_startBlock7;
            // 0x118
            public UInt32 extentsFile_extents_blockCount7;
            // 0x11C
            // HFSPlusForkData     catalogFile;
            public UInt64 catalogFile_logicalSize;
            // 0x120
            public UInt32 catalogFile_clumpSize;
            // 0x128
            public UInt32 catalogFile_totalBlocks;
            // 0x12C
            public UInt32 catalogFile_extents_startBlock0;
            // 0x130
            public UInt32 catalogFile_extents_blockCount0;
            // 0x134
            public UInt32 catalogFile_extents_startBlock1;
            // 0x138
            public UInt32 catalogFile_extents_blockCount1;
            // 0x13C
            public UInt32 catalogFile_extents_startBlock2;
            // 0x140
            public UInt32 catalogFile_extents_blockCount2;
            // 0x144
            public UInt32 catalogFile_extents_startBlock3;
            // 0x148
            public UInt32 catalogFile_extents_blockCount3;
            // 0x14C
            public UInt32 catalogFile_extents_startBlock4;
            // 0x150
            public UInt32 catalogFile_extents_blockCount4;
            // 0x154
            public UInt32 catalogFile_extents_startBlock5;
            // 0x158
            public UInt32 catalogFile_extents_blockCount5;
            // 0x15C
            public UInt32 catalogFile_extents_startBlock6;
            // 0x160
            public UInt32 catalogFile_extents_blockCount6;
            // 0x164
            public UInt32 catalogFile_extents_startBlock7;
            // 0x168
            public UInt32 catalogFile_extents_blockCount7;
            // 0x16C
            // HFSPlusForkData     attributesFile;
            public UInt64 attributesFile_logicalSize;
            // 0x170
            public UInt32 attributesFile_clumpSize;
            // 0x178
            public UInt32 attributesFile_totalBlocks;
            // 0x17C
            public UInt32 attributesFile_extents_startBlock0;
            // 0x180
            public UInt32 attributesFile_extents_blockCount0;
            // 0x184
            public UInt32 attributesFile_extents_startBlock1;
            // 0x188
            public UInt32 attributesFile_extents_blockCount1;
            // 0x18C
            public UInt32 attributesFile_extents_startBlock2;
            // 0x190
            public UInt32 attributesFile_extents_blockCount2;
            // 0x194
            public UInt32 attributesFile_extents_startBlock3;
            // 0x198
            public UInt32 attributesFile_extents_blockCount3;
            // 0x19C
            public UInt32 attributesFile_extents_startBlock4;
            // 0x1A0
            public UInt32 attributesFile_extents_blockCount4;
            // 0x1A4
            public UInt32 attributesFile_extents_startBlock5;
            // 0x1A8
            public UInt32 attributesFile_extents_blockCount5;
            // 0x1AC
            public UInt32 attributesFile_extents_startBlock6;
            // 0x1B0
            public UInt32 attributesFile_extents_blockCount6;
            // 0x1B4
            public UInt32 attributesFile_extents_startBlock7;
            // 0x1B8
            public UInt32 attributesFile_extents_blockCount7;
            // 0x1BC
            // HFSPlusForkData     startupFile;
            public UInt64 startupFile_logicalSize;
            // 0x1C0
            public UInt32 startupFile_clumpSize;
            // 0x1C8
            public UInt32 startupFile_totalBlocks;
            // 0x1CC
            public UInt32 startupFile_extents_startBlock0;
            // 0x1D0
            public UInt32 startupFile_extents_blockCount0;
            // 0x1D4
            public UInt32 startupFile_extents_startBlock1;
            // 0x1D8
            public UInt32 startupFile_extents_blockCount1;
            // 0x1E0
            public UInt32 startupFile_extents_startBlock2;
            // 0x1E4
            public UInt32 startupFile_extents_blockCount2;
            // 0x1E8
            public UInt32 startupFile_extents_startBlock3;
            // 0x1EC
            public UInt32 startupFile_extents_blockCount3;
            // 0x1F0
            public UInt32 startupFile_extents_startBlock4;
            // 0x1F4
            public UInt32 startupFile_extents_blockCount4;
            // 0x1F8
            public UInt32 startupFile_extents_startBlock5;
            // 0x1FC
            public UInt32 startupFile_extents_blockCount5;
            // 0x200
            public UInt32 startupFile_extents_startBlock6;
            // 0x204
            public UInt32 startupFile_extents_blockCount6;
            // 0x208
            public UInt32 startupFile_extents_startBlock7;
            // 0x20C
            public UInt32 startupFile_extents_blockCount7;
            // 0x210
        }
    }
}
