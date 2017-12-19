// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : RDB.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Amiga Rigid Disk Block.
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
using System.Text;
using DiscImageChef.Console;

namespace DiscImageChef.PartPlugins
{
    public class AmigaRigidDiskBlock : PartPlugin
    {
        /// <summary>
        /// RDB magic number "RDSK"
        /// </summary>
        const uint RigidDiskBlockMagic = 0x5244534B;
        /// <summary>
        /// Bad block list magic number "BADB"
        /// </summary>
        const uint BadBlockListMagic = 0x42414442;
        /// <summary>
        /// Partition entry magic number "PART"
        /// </summary>
        const uint PartitionBlockMagic = 0x50415254;
        /// <summary>
        /// Filesystem header magic number "FSHD"
        /// </summary>
        const uint FilesystemHeaderMagic = 0x46534844;
        /// <summary>
        /// LoadSeg block magic number "LSEG"
        /// </summary>
        const uint LoadSegMagic = 0x4C534547;

        /// <summary>
        /// Type ID for Amiga Original File System, "DOS\0"
        /// </summary>
        const uint TypeIDOFS = 0x444F5300;
        /// <summary>
        /// Type ID for Amiga Fast File System, "DOS\1"
        /// </summary>
        const uint TypeIDFFS = 0x444F5301;
        /// <summary>
        /// Type ID for Amiga Original File System with international characters, "DOS\2"
        /// </summary>
        const uint TypeIDOFSi = 0x444F5302;
        /// <summary>
        /// Type ID for Amiga Fast File System with international characters, "DOS\3"
        /// </summary>
        const uint TypeIDFFSi = 0x444F5303;
        /// <summary>
        /// Type ID for Amiga Original File System with directory cache, "DOS\4"
        /// </summary>
        const uint TypeIDOFSc = 0x444F5304;
        /// <summary>
        /// Type ID for Amiga Fast File System with directory cache, "DOS\5"
        /// </summary>
        const uint TypeIDFFSc = 0x444F5305;
        /// <summary>
        /// Type ID for Amiga Original File System with long filenames, "DOS\6"
        /// </summary>
        const uint TypeIDOFS2 = 0x444F5306;
        /// <summary>
        /// Type ID for Amiga Fast File System with long filenames, "DOS\7"
        /// </summary>
        const uint TypeIDFFS2 = 0x444F5307;
        /// <summary>
        /// Type ID for Amiga UNIX boot filesystem
        /// </summary>
        const uint TypeIDAMIXBoot = 0x554E4900;
        /// <summary>
        /// Type ID for Amiga UNIX System V filesystem
        /// </summary>
        const uint TypeIDAMIXSysV = 0x554E4901;
        /// <summary>
        /// Type ID for Amiga UNIX BSD filesystem
        /// </summary>
        const uint TypeIDAMIXFFS = 0x554E4902;
        /// <summary>
        /// Type ID for Amiga UNIX Reserved partition (swap)
        /// </summary>
        const uint TypeIDAMIXReserved = 0x72657376;
        /// <summary>
        /// Type ID for ProfessionalFileSystem, "PFS\1"
        /// </summary>
        const uint TypeIDPFS = 0x50465301;
        /// <summary>
        /// Type ID for ProfessionalFileSystem, "PFS\2"
        /// </summary>
        const uint TypeIDPFS2 = 0x50465302;
        /// <summary>
        /// Type ID for ProfessionalFileSystem, "muAF"
        /// </summary>
        const uint TypeIDPFSm = 0x6D754146;
        /// <summary>
        /// Type ID for ProfessionalFileSystem, "AFS\1"
        /// </summary>
        const uint TypeIDAFS = 0x41465301;
        /// <summary>
        /// Type ID for SmartFileSystem v1, "SFS\0"
        /// </summary>
        const uint TypeIDSFS = 0x53465300;
        /// <summary>
        /// Type ID for SmartFileSystem v2, "SFS\2"
        /// </summary>
        const uint TypeIDSFS2 = 0x53465302;
        /// <summary>
        /// Type ID for JXFS, "JXF\4"
        /// </summary>
        const uint TypeIDJXFS = 0x4A584604;
        /// <summary>
        /// Type ID for FAT, as set by CrossDOS, "MSD\0"
        /// </summary>
        const uint TypeIDCrossDOS = 0x4D534400;
        /// <summary>
        /// Type ID for HFS, as set by CrossMac, "MAC\0"
        /// </summary>
        const uint TypeIDCrossMac = 0x4D414300;
        /// <summary>
        /// Type ID for 4.2UFS, for BFFS, "BFFS"
        /// </summary>
        const uint TypeIDBFFS = 0x42464653;
        /// <summary>
        /// Type ID for Amiga Original File System with multi-user patches, "muF\0"
        /// </summary>
        const uint TypeIDmuOFS = 0x6D754600;
        /// <summary>
        /// Type ID for Amiga Fast File System with multi-user patches, "muF\1"
        /// </summary>
        const uint TypeIDmuFFS = 0x6D754601;
        /// <summary>
        /// Type ID for Amiga Original File System with international characters and multi-user patches, "muF\2"
        /// </summary>
        const uint TypeIDmuOFSi = 0x6D754602;
        /// <summary>
        /// Type ID for Amiga Fast File System with international characters and multi-user patches, "muF\3"
        /// </summary>
        const uint TypeIDmuFFSi = 0x6D754603;
        /// <summary>
        /// Type ID for Amiga Original File System with directory cache and multi-user patches, "muF\4"
        /// </summary>
        const uint TypeIDmuOFSc = 0x6D754604;
        /// <summary>
        /// Type ID for Amiga Fast File System with directory cache and multi-user patches, "muF\5"
        /// </summary>
        const uint TypeIDmuFFSc = 0x6D754605;
        /// <summary>
        /// Type ID for BSD unused, "BSD\0"
        /// </summary>
        const uint TypeIDOldBSDUnused = 0x42534400;
        /// <summary>
        /// Type ID for BSD swap, "BSD\1"
        /// </summary>
        const uint TypeIDOldBSDSwap = 0x42534401;
        /// <summary>
        /// Type ID for BSD 4.2 FFS, "BSD\7"
        /// </summary>
        const uint TypeIDOldBSD42FFS = 0x42534407;
        /// <summary>
        /// Type ID for BSD 4.4 LFS, "BSD\9"
        /// </summary>
        const uint TypeIDOldBSD44LFS = 0x42534409;
        /// <summary>
        /// Type ID for NetBSD unused root partition, "NBR\0"
        /// </summary>
        const uint TypeIDNetBSDRootUnused = 0x4E425200;
        /// <summary>
        /// Type ID for NetBSD 4.2 FFS root partition, "NBR\7"
        /// </summary>
        const uint TypeIDNetBSDRoot42FFS = 0x4E425207;
        /// <summary>
        /// Type ID for NetBSD 4.4 LFS root partition, "NBR\9"
        /// </summary>
        const uint TypeIDNetBSDRoot44LFS = 0x4E425209;
        /// <summary>
        /// Type ID for NetBSD unused user partition, "NBR\0"
        /// </summary>
        const uint TypeIDNetBSDUserUnused = 0x4E425500;
        /// <summary>
        /// Type ID for NetBSD 4.2 FFS user partition, "NBR\7"
        /// </summary>
        const uint TypeIDNetBSDUser42FFS = 0x4E425507;
        /// <summary>
        /// Type ID for NetBSD 4.4 LFS user partition, "NBR\9"
        /// </summary>
        const uint TypeIDNetBSDUser44LFS = 0x4E425509;
        /// <summary>
        /// Type ID for NetBSD swap partition
        /// </summary>
        const uint TypeIDNetBSDSwap = 0x4E425300;
        /// <summary>
        /// Type ID for Linux filesystem partition, "LNX\0"
        /// </summary>
        const uint TypeIDLinux = 0x4C4E5800;
        /// <summary>
        /// Type ID for Linux swap partition, "SWP\0"
        /// </summary>
        const uint TypeIDLinuxSwap = 0x53575000;
        /// <summary>
        /// Type ID for RaidFrame partition, "RAID"
        /// </summary>
        const uint TypeIDRaidFrame = 0x52414944;
        /// <summary>
        /// Type ID for RaidFrame partition, "RAI\0"
        /// </summary>
        const uint TypeIDRaidFrame0 = 0x52414900;

        /// <summary>
        /// No disks to be configured after this one
        /// </summary>
        const uint FlagsNoDisks = 0x00000001;
        /// <summary>
        /// No LUNs to be configured after this one
        /// </summary>
        const uint FlagsNoLUNs = 0x00000002;
        /// <summary>
        /// No target IDs to be configured after this one
        /// </summary>
        const uint FlagsNoTargets = 0x00000004;
        /// <summary>
        /// Don't try to perform reselection with this drive
        /// </summary>
        const uint FlagsNoReselection = 0x00000008;
        /// <summary>
        /// Disk identification is valid
        /// </summary>
        const uint FlagsValidDiskID = 0x00000010;
        /// <summary>
        /// Controller identification is valid
        /// </summary>
        const uint FlagsValidControllerID = 0x00000020;
        /// <summary>
        ///  Drive supports synchronous SCSI mode
        /// </summary>
        const uint FlagsSynchSCSI = 0x00000040;

        /// <summary>
        /// Partition is bootable
        /// </summary>
        const uint FlagsBootable = 0x00000001;
        /// <summary>
        /// Partition should not be mounted automatically
        /// </summary>
        const uint FlagsNoAutomount = 0x00000002;

        public AmigaRigidDiskBlock()
        {
            Name = "Amiga Rigid Disk Block";
            PluginUUID = new Guid("8D72ED97-1854-4170-9CE4-6E8446FD9863");
        }

        /// <summary>
        /// Amiga Rigid Disk Block, header for partitioning scheme
        /// Can be in any sector from 0 to 15, inclusive
        /// </summary>
        struct RigidDiskBlock
        {
            /// <summary>
            /// "RDSK"
            /// </summary>
            public uint magic;
            /// <summary>
            /// Size in longs
            /// </summary>
            public uint size;
            /// <summary>
            /// Checksum
            /// </summary>
            public int checksum;
            /// <summary>
            /// SCSI target ID, 7 for non-SCSI
            /// </summary>
            public uint targetID;
            /// <summary>
            /// Block size in bytes
            /// </summary>
            public uint block_size;
            /// <summary>
            /// Flags
            /// </summary>
            public uint flags;
            /// <summary>
            /// Pointer to first BadBlockList, 0xFFFFFFFF means last block in device
            /// </summary>
            public uint badblock_ptr;
            /// <summary>
            /// Pointer to first PartitionEntry, 0xFFFFFFFF means last block in device
            /// </summary>
            public uint partition_ptr;
            /// <summary>
            /// Pointer to first FileSystemHeader, 0xFFFFFFFF means last block in device
            /// </summary>
            public uint fsheader_ptr;
            /// <summary>
            /// Optional drive specific init code
            /// </summary>
            public uint driveinitcode;
            /// <summary>
            /// Reserved, should be 0xFFFFFFFF
            /// </summary>
            public uint reserved1;
            /// <summary>
            /// Reserved, should be 0xFFFFFFFF
            /// </summary>
            public uint reserved2;
            /// <summary>
            /// Reserved, should be 0xFFFFFFFF
            /// </summary>
            public uint reserved3;
            /// <summary>
            /// Reserved, should be 0xFFFFFFFF
            /// </summary>
            public uint reserved4;
            /// <summary>
            /// Reserved, should be 0xFFFFFFFF
            /// </summary>
            public uint reserved5;
            /// <summary>
            /// Reserved, should be 0xFFFFFFFF
            /// </summary>
            public uint reserved6;
            /// <summary>
            /// Cylinders in drive
            /// </summary>
            public uint cylinders;
            /// <summary>
            /// Sectors per track
            /// </summary>
            public uint spt;
            /// <summary>
            /// Heads in drive
            /// </summary>
            public uint heads;
            /// <summary>
            /// Drive interleave
            /// </summary>
            public uint interleave;
            /// <summary>
            /// Cylinder for parking heads
            /// </summary>
            public uint parking;
            /// <summary>
            /// Reserved, should be zero
            /// </summary>
            public uint reserved7;
            /// <summary>
            /// Reserved, should be zero
            /// </summary>
            public uint reserved8;
            /// <summary>
            /// Reserved, should be zero
            /// </summary>
            public uint reserved9;
            /// <summary>
            /// Starting cylinder for write precompensation
            /// </summary>
            public uint writeprecomp;
            /// <summary>
            /// Starting cylinder for reduced write current
            /// </summary>
            public uint reducedwrite;
            /// <summary>
            /// Drive step rate
            /// </summary>
            public uint steprate;
            /// <summary>
            /// Reserved, should be zero
            /// </summary>
            public uint reserved10;
            /// <summary>
            /// Reserved, should be zero
            /// </summary>
            public uint reserved11;
            /// <summary>
            /// Reserved, should be zero
            /// </summary>
            public uint reserved12;
            /// <summary>
            /// Reserved, should be zero
            /// </summary>
            public uint reserved13;
            /// <summary>
            /// Reserved, should be zero
            /// </summary>
            public uint reserved14;
            /// <summary>
            /// Low block of RDB reserved blocks
            /// </summary>
            public uint RDBBlockLow;
            /// <summary>
            /// High block of RDB reserved blocks
            /// </summary>
            public uint RDBBlockHigh;
            /// <summary>
            /// Low cylinder for partitionable area
            /// </summary>
            public uint LowCylinder;
            /// <summary>
            /// High cylinder for partitionable area
            /// </summary>
            public uint HighCylinder;
            /// <summary>
            /// Blocks per cylinder
            /// </summary>
            public uint CylBlocks;
            /// <summary>
            /// Seconds for head autoparking
            /// </summary>
            public uint AutoParkSeconds;
            /// <summary>
            /// Highest block used by RDB
            /// </summary>
            public uint HighRDSKBlock;
            /// <summary>
            /// Reserved, should be zero
            /// </summary>
            public uint reserved15;
            /// <summary>
            /// Disk vendor, 8 bytes
            /// </summary>
            public string diskVendor;
            /// <summary>
            /// Disk product, 16 bytes
            /// </summary>
            public string diskProduct;
            /// <summary>
            /// Disk revision, 4 bytes
            /// </summary>
            public string diskRevision;
            /// <summary>
            /// Controller vendor, 8 bytes
            /// </summary>
            public string controllerVendor;
            /// <summary>
            /// Controller product, 16 bytes
            /// </summary>
            public string controllerProduct;
            /// <summary>
            /// Controller revision, 4 bytes
            /// </summary>
            public string controllerRevision;
            /// <summary>
            /// Reserved, should be zero
            /// </summary>
            public uint reserved16;
            /// <summary>
            /// Reserved, should be zero
            /// </summary>
            public uint reserved17;
            /// <summary>
            /// Reserved, should be zero
            /// </summary>
            public uint reserved18;
            /// <summary>
            /// Reserved, should be zero
            /// </summary>
            public uint reserved19;
            /// <summary>
            /// Reserved, should be zero
            /// </summary>
            public uint reserved20;
            /// <summary>
            /// Reserved, should be zero
            /// </summary>
            public uint reserved21;
            /// <summary>
            /// Reserved, should be zero
            /// </summary>
            public uint reserved22;
            /// <summary>
            /// Reserved, should be zero
            /// </summary>
            public uint reserved23;
            /// <summary>
            /// Reserved, should be zero
            /// </summary>
            public uint reserved24;
            /// <summary>
            /// Reserved, should be zero
            /// </summary>
            public uint reserved25;
        }

        /// <summary>
        /// Pair for spare blocks
        /// </summary>
        struct BadBlockEntry
        {
            /// <summary>
            /// Bad block pointer
            /// </summary>
            public uint badBlock;
            /// <summary>
            /// Replacement block pointer
            /// </summary>
            public uint goodBlock;
        }

        /// <summary>
        /// List of bad blocks and spares
        /// </summary>
        struct BadBlockList
        {
            /// <summary>
            /// "BADB"
            /// </summary>
            public uint magic;
            /// <summary>
            /// Size in longs
            /// </summary>
            public uint size;
            /// <summary>
            /// Checksum
            /// </summary>
            public int checksum;
            /// <summary>
            /// SCSI target ID, 7 for non-SCSI
            /// </summary>
            public uint targetID;
            /// <summary>
            /// Pointer for next BadBlockList
            /// </summary>
            public uint next_ptr;
            /// <summary>
            /// Reserved
            /// </summary>
            public uint reserved;
            /// <summary>
            /// Bad block entries, up to block filling, 8 bytes each
            /// </summary>
            public BadBlockEntry[] blockPairs;
        }

        /// <summary>
        /// DOSEnvVec, used by AmigaDOS
        /// </summary>
        struct DOSEnvironmentVector
        {
            /// <summary>
            /// Size in longs, should be 16, minimum 11
            /// </summary>
            public uint size;
            /// <summary>
            /// Block size in longs
            /// </summary>
            public uint block_size;
            /// <summary>
            /// Unknown, 0
            /// </summary>
            public uint sec_org;
            /// <summary>
            /// Heads in drive
            /// </summary>
            public uint surfaces;
            /// <summary>
            /// Sectors per block
            /// </summary>
            public uint spb;
            /// <summary>
            /// Blocks per track
            /// </summary>
            public uint bpt;
            /// <summary>
            /// DOS reserved blocks at start of partition
            /// </summary>
            public uint reservedblocks;
            /// <summary>
            /// DOS reserved blocks at end of partition
            /// </summary>
            public uint prealloc;
            /// <summary>
            /// Interleave
            /// </summary>
            public uint interleave;
            /// <summary>
            /// First cylinder of a partition, inclusive
            /// </summary>
            public uint lowCylinder;
            /// <summary>
            /// Last cylinder of a partition, inclusive
            /// </summary>
            public uint highCylinder;
            /// <summary>
            /// Buffers, usually 30
            /// </summary>
            public uint numBuffer;
            /// <summary>
            /// Type of memory to allocate for buffers
            /// </summary>
            public uint bufMemType;
            /// <summary>
            /// Maximum transfer, usually 0x7FFFFFFF
            /// </summary>
            public uint maxTransfer;
            /// <summary>
            /// Address mask to block out certain memory, usually 0xFFFFFFFE
            /// </summary>
            public uint Mask;
            /// <summary>
            /// Boot priority
            /// </summary>
            public uint bootPriority;
            /// <summary>
            /// Partition type, and filesystem driver identification for AmigaDOS
            /// </summary>
            public uint dosType;
            /// <summary>
            /// Default baud rate for SER and AUX handlers
            /// </summary>
            public uint baud;
            /// <summary>
            /// Flow control values for SER and AUX handlers
            /// </summary>
            public uint control;
            /// <summary>
            /// Since Kickstart 2, how many boot blocks are to be loaded
            /// </summary>
            public uint bootBlocks;
        }

        struct PartitionEntry
        {
            /// <summary>
            /// "PART"
            /// </summary>
            public uint magic;
            /// <summary>
            /// Size in longs
            /// </summary>
            public uint size;
            /// <summary>
            /// Checksum
            /// </summary>
            public int checksum;
            /// <summary>
            /// SCSI target ID, 7 for non-SCSI
            /// </summary>
            public uint targetID;
            /// <summary>
            /// Pointer to next PartitionEntry
            /// </summary>
            public uint next_ptr;
            /// <summary>
            /// Partition flags
            /// </summary>
            public uint flags;
            /// <summary>
            /// Reserved
            /// </summary>
            public uint reserved1;
            /// <summary>
            /// Reserved
            /// </summary>
            public uint reserved2;
            /// <summary>
            /// Preferred flags for OpenDevice()
            /// </summary>
            public uint devFlags;
            /// <summary>
            /// Length of drive name
            /// </summary>
            public uint driveNameLen;
            /// <summary>
            /// Drive name, 31 bytes
            /// </summary>
            public string driveName;
            /// <summary>
            /// Reserved
            /// </summary>
            public uint reserved3;
            /// <summary>
            /// Reserved
            /// </summary>
            public uint reserved4;
            /// <summary>
            /// Reserved
            /// </summary>
            public uint reserved5;
            /// <summary>
            /// Reserved
            /// </summary>
            public uint reserved6;
            /// <summary>
            /// Reserved
            /// </summary>
            public uint reserved7;
            /// <summary>
            /// Reserved
            /// </summary>
            public uint reserved8;
            /// <summary>
            /// Reserved
            /// </summary>
            public uint reserved9;
            /// <summary>
            /// Reserved
            /// </summary>
            public uint reserved10;
            /// <summary>
            /// Reserved
            /// </summary>
            public uint reserved11;
            /// <summary>
            /// Reserved
            /// </summary>
            public uint reserved12;
            /// <summary>
            /// Reserved
            /// </summary>
            public uint reserved13;
            /// <summary>
            /// Reserved
            /// </summary>
            public uint reserved14;
            /// <summary>
            /// Reserved
            /// </summary>
            public uint reserved15;
            /// <summary>
            /// Reserved
            /// </summary>
            public uint reserved16;
            /// <summary>
            /// Reserved
            /// </summary>
            public uint reserved17;
            /// <summary>
            /// DOSEnvVec, more information about partition
            /// </summary>
            public DOSEnvironmentVector dosEnvVec;
        }

        /// <summary>
        /// Device node, mostly useless, except for pointer to first LoadSegment block
        /// </summary>
        struct DeviceNode
        {
            /// <summary>
            /// Device node type, =0
            /// </summary>
            public uint type;
            /// <summary>
            /// DOS task field, =0
            /// </summary>
            public uint task;
            /// <summary>
            /// Unused, =0
            /// </summary>
            public uint locked;
            /// <summary>
            /// Filename handler to LoadSegment, =0
            /// </summary>
            public uint handler;
            /// <summary>
            /// Stack size when starting task, =0
            /// </summary>
            public uint stackSize;
            /// <summary>
            /// Task priority, =0
            /// </summary>
            public uint priority;
            /// <summary>
            /// Startup message, =0
            /// </summary>
            public uint startup;
            /// <summary>
            /// Pointer to first LoadSegment block
            /// </summary>
            public uint seglist_ptr;
            /// <summary>
            /// BCPL globabl vector when starting task, =0xFFFFFFFF
            /// </summary>
            public uint global_vec;
        }

        /// <summary>
        /// File system header
        /// </summary>
        struct FileSystemHeader
        {
            /// <summary>
            /// "FSHD"
            /// </summary>
            public uint magic;
            /// <summary>
            /// Size in longs, 64
            /// </summary>
            public uint size;
            /// <summary>
            /// Checksum
            /// </summary>
            public int checksum;
            /// <summary>
            /// SCSI target ID, 7 for non-SCSI
            /// </summary>
            public uint targetID;
            /// <summary>
            /// Pointer to next FileSystemHeader block
            /// </summary>
            public uint next_ptr;
            /// <summary>
            /// Flags, unknown
            /// </summary>
            public uint flags;
            /// <summary>
            /// Reserved
            /// </summary>
            public uint reserved1;
            /// <summary>
            /// Reserved
            /// </summary>
            public uint reserved2;
            /// <summary>
            /// Partition type, and filesystem driver identification for AmigaDOS
            /// </summary>
            public uint dosType;
            /// <summary>
            /// Filesystem version
            /// Mask 0xFFFF0000, >>16, major version
            /// Mask 0x0000FFFF, minor version
            /// </summary>
            public uint version;
            /// <summary>
            /// Bits for DeviceNode fields that should be substituted into a standard device node
            /// </summary>
            public uint patchFlags;
            /// <summary>
            /// Device node
            /// </summary>
            public DeviceNode dnode;
        }

        /// <summary>
        /// Filesystem code
        /// </summary>
        struct LoadSegment
        {
            /// <summary>
            /// "LSEG"
            /// </summary>
            public uint magic;
            /// <summary>
            /// Size in longs
            /// </summary>
            public uint size;
            /// <summary>
            /// Checksum
            /// </summary>
            public int checksum;
            /// <summary>
            /// SCSI target ID, 7 for non-SCSI
            /// </summary>
            public uint targetID;
            /// <summary>
            /// Pointer to next LoadSegment
            /// </summary>
            public uint next_ptr;
            /// <summary>
            /// Executable code, with relocation hunks, til end of sector
            /// </summary>
            public byte[] loadData;
        }

        public override bool GetInformation(ImagePlugins.ImagePlugin imagePlugin,
                                            out List<CommonTypes.Partition> partitions, ulong sectorOffset)
        {
            partitions = new List<CommonTypes.Partition>();
            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
            ulong RDBBlock = 0;
            bool foundRDB = false;

            while(RDBBlock < 16 && !foundRDB)
            {
                if(imagePlugin.GetSectors() <= RDBBlock) return false;

                if(RDBBlock + sectorOffset >= imagePlugin.GetSectors()) break;

                byte[] tmpSector = imagePlugin.ReadSector(RDBBlock + sectorOffset);
                uint magic = BigEndianBitConverter.ToUInt32(tmpSector, 0);

                DicConsole.DebugWriteLine("Amiga RDB plugin", "Possible magic at block {0} is 0x{1:X8}", RDBBlock,
                                          magic);

                if(magic == RigidDiskBlockMagic)
                {
                    DicConsole.DebugWriteLine("Amiga RDB plugin", "Found RDB magic at block {0}", RDBBlock);

                    foundRDB = true;
                    break;
                }

                RDBBlock++;
            }

            if(!foundRDB) return false;

            RDBBlock += sectorOffset;

            byte[] sector;
            byte[] tmpString;

            RigidDiskBlock RDB = new RigidDiskBlock();

            sector = imagePlugin.ReadSector(RDBBlock);

            RDB.magic = BigEndianBitConverter.ToUInt32(sector, 0x00);
            RDB.size = BigEndianBitConverter.ToUInt32(sector, 0x04);
            RDB.checksum = BigEndianBitConverter.ToInt32(sector, 0x08);
            RDB.targetID = BigEndianBitConverter.ToUInt32(sector, 0x0C);
            RDB.block_size = BigEndianBitConverter.ToUInt32(sector, 0x10);
            RDB.flags = BigEndianBitConverter.ToUInt32(sector, 0x04);
            RDB.badblock_ptr = BigEndianBitConverter.ToUInt32(sector, 0x18);
            RDB.partition_ptr = BigEndianBitConverter.ToUInt32(sector, 0x1C);
            RDB.fsheader_ptr = BigEndianBitConverter.ToUInt32(sector, 0x20);
            RDB.driveinitcode = BigEndianBitConverter.ToUInt32(sector, 0x24);
            RDB.reserved1 = BigEndianBitConverter.ToUInt32(sector, 0x28);
            RDB.reserved2 = BigEndianBitConverter.ToUInt32(sector, 0x2C);
            RDB.reserved3 = BigEndianBitConverter.ToUInt32(sector, 0x30);
            RDB.reserved4 = BigEndianBitConverter.ToUInt32(sector, 0x34);
            RDB.reserved5 = BigEndianBitConverter.ToUInt32(sector, 0x38);
            RDB.reserved6 = BigEndianBitConverter.ToUInt32(sector, 0x3C);
            RDB.cylinders = BigEndianBitConverter.ToUInt32(sector, 0x40);
            RDB.spt = BigEndianBitConverter.ToUInt32(sector, 0x44);
            RDB.heads = BigEndianBitConverter.ToUInt32(sector, 0x48);
            RDB.interleave = BigEndianBitConverter.ToUInt32(sector, 0x4C);
            RDB.parking = BigEndianBitConverter.ToUInt32(sector, 0x50);
            RDB.reserved7 = BigEndianBitConverter.ToUInt32(sector, 0x54);
            RDB.reserved8 = BigEndianBitConverter.ToUInt32(sector, 0x58);
            RDB.reserved9 = BigEndianBitConverter.ToUInt32(sector, 0x5C);
            RDB.writeprecomp = BigEndianBitConverter.ToUInt32(sector, 0x60);
            RDB.reducedwrite = BigEndianBitConverter.ToUInt32(sector, 0x64);
            RDB.steprate = BigEndianBitConverter.ToUInt32(sector, 0x68);
            RDB.reserved10 = BigEndianBitConverter.ToUInt32(sector, 0x6C);
            RDB.reserved11 = BigEndianBitConverter.ToUInt32(sector, 0x70);
            RDB.reserved12 = BigEndianBitConverter.ToUInt32(sector, 0x74);
            RDB.reserved13 = BigEndianBitConverter.ToUInt32(sector, 0x78);
            RDB.reserved14 = BigEndianBitConverter.ToUInt32(sector, 0x7C);
            RDB.RDBBlockLow = BigEndianBitConverter.ToUInt32(sector, 0x80);
            RDB.RDBBlockHigh = BigEndianBitConverter.ToUInt32(sector, 0x84);
            RDB.LowCylinder = BigEndianBitConverter.ToUInt32(sector, 0x88);
            RDB.HighCylinder = BigEndianBitConverter.ToUInt32(sector, 0x8C);
            RDB.CylBlocks = BigEndianBitConverter.ToUInt32(sector, 0x90);
            RDB.AutoParkSeconds = BigEndianBitConverter.ToUInt32(sector, 0x94);
            RDB.HighCylinder = BigEndianBitConverter.ToUInt32(sector, 0x98);
            RDB.reserved15 = BigEndianBitConverter.ToUInt32(sector, 0x9C);

            tmpString = new byte[8];
            Array.Copy(sector, 0xA0, tmpString, 0, 8);
            RDB.diskVendor = StringHandlers.SpacePaddedToString(tmpString);
            tmpString = new byte[16];
            Array.Copy(sector, 0xA8, tmpString, 0, 16);
            RDB.diskProduct = StringHandlers.SpacePaddedToString(tmpString);
            tmpString = new byte[4];
            Array.Copy(sector, 0xB8, tmpString, 0, 4);
            RDB.diskRevision = StringHandlers.SpacePaddedToString(tmpString);

            tmpString = new byte[8];
            Array.Copy(sector, 0xBC, tmpString, 0, 8);
            RDB.controllerVendor = StringHandlers.SpacePaddedToString(tmpString);
            tmpString = new byte[16];
            Array.Copy(sector, 0xC4, tmpString, 0, 16);
            RDB.controllerProduct = StringHandlers.SpacePaddedToString(tmpString);
            tmpString = new byte[4];
            Array.Copy(sector, 0xD4, tmpString, 0, 4);
            RDB.controllerRevision = StringHandlers.SpacePaddedToString(tmpString);

            RDB.reserved16 = BigEndianBitConverter.ToUInt32(sector, 0xD8);
            RDB.reserved17 = BigEndianBitConverter.ToUInt32(sector, 0xDC);
            RDB.reserved18 = BigEndianBitConverter.ToUInt32(sector, 0xE0);
            RDB.reserved19 = BigEndianBitConverter.ToUInt32(sector, 0xE4);
            RDB.reserved20 = BigEndianBitConverter.ToUInt32(sector, 0xE8);
            RDB.reserved21 = BigEndianBitConverter.ToUInt32(sector, 0xEC);
            RDB.reserved22 = BigEndianBitConverter.ToUInt32(sector, 0xF0);
            RDB.reserved23 = BigEndianBitConverter.ToUInt32(sector, 0xF4);
            RDB.reserved24 = BigEndianBitConverter.ToUInt32(sector, 0xF8);
            RDB.reserved25 = BigEndianBitConverter.ToUInt32(sector, 0xFC);

            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.magic = 0x{0:X8}", RDB.magic);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.size = {0} longs, {1} bytes", RDB.size, RDB.size * 4);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.checksum = 0x{0:X8}", RDB.checksum);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.targetID = {0}", RDB.targetID);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.block_size = {0}", RDB.block_size);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.badblock_ptr = {0}", RDB.badblock_ptr);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.partition_ptr = {0}", RDB.partition_ptr);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.fsheader_ptr = {0}", RDB.fsheader_ptr);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.driveinitcode = {0}", RDB.driveinitcode);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reserved1 = 0x{0:X8}", RDB.reserved1);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reserved2 = 0x{0:X8}", RDB.reserved2);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reserved3 = 0x{0:X8}", RDB.reserved3);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reserved4 = 0x{0:X8}", RDB.reserved4);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reserved5 = 0x{0:X8}", RDB.reserved5);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reserved6 = 0x{0:X8}", RDB.reserved6);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.cylinders = {0}", RDB.cylinders);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.spt = {0}", RDB.spt);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.heads = {0}", RDB.heads);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.interleave = {0}", RDB.interleave);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.parking = {0}", RDB.parking);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reserved7 = 0x{0:X8}", RDB.reserved7);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reserved8 = 0x{0:X8}", RDB.reserved8);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reserved9 = 0x{0:X8}", RDB.reserved9);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.writeprecomp = {0}", RDB.writeprecomp);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reducedwrite = {0}", RDB.reducedwrite);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.steprate = {0}", RDB.steprate);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reserved10 = 0x{0:X8}", RDB.reserved10);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reserved11 = 0x{0:X8}", RDB.reserved11);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reserved12 = 0x{0:X8}", RDB.reserved12);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reserved13 = 0x{0:X8}", RDB.reserved13);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reserved14 = 0x{0:X8}", RDB.reserved14);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.RDBBlockLow = {0}", RDB.RDBBlockLow);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.RDBBlockHigh = {0}", RDB.RDBBlockHigh);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.LowCylinder = {0}", RDB.LowCylinder);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.HighCylinder = {0}", RDB.HighCylinder);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.CylBlocks = {0}", RDB.CylBlocks);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.AutoParkSeconds = {0}", RDB.AutoParkSeconds);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.HighCylinder = {0}", RDB.HighCylinder);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reserved15 = 0x{0:X8}", RDB.reserved15);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.diskVendor = \"{0}\"", RDB.diskVendor);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.diskProduct = \"{0}\"", RDB.diskProduct);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.diskRevision = \"{0}\"", RDB.diskRevision);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.controllerVendor = \"{0}\"", RDB.controllerVendor);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.controllerProduct = \"{0}\"", RDB.controllerProduct);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.controllerRevision = \"{0}\"", RDB.controllerRevision);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reserved16 = 0x{0:X8}", RDB.reserved16);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reserved17 = 0x{0:X8}", RDB.reserved17);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reserved18 = 0x{0:X8}", RDB.reserved18);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reserved19 = 0x{0:X8}", RDB.reserved19);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reserved20 = 0x{0:X8}", RDB.reserved20);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reserved21 = 0x{0:X8}", RDB.reserved21);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reserved22 = 0x{0:X8}", RDB.reserved22);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reserved23 = 0x{0:X8}", RDB.reserved23);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reserved24 = 0x{0:X8}", RDB.reserved24);
            DicConsole.DebugWriteLine("Amiga RDB plugin", "RDB.reserved25 = 0x{0:X8}", RDB.reserved25);

            ulong nextBlock;

            // Reading BadBlock list
            List<BadBlockList> BadBlockChain = new List<BadBlockList>();
            nextBlock = RDB.badblock_ptr;
            while(nextBlock != 0xFFFFFFFF)
            {
                DicConsole.DebugWriteLine("Amiga RDB plugin", "Going to block {0} in search of a BadBlock block",
                                          nextBlock);

                sector = imagePlugin.ReadSector(nextBlock);
                uint magic = BigEndianBitConverter.ToUInt32(sector, 0);

                if(magic != BadBlockListMagic) break;

                DicConsole.DebugWriteLine("Amiga RDB plugin", "Found BadBlock block");

                BadBlockList chainEntry = new BadBlockList
                {
                    magic = BigEndianBitConverter.ToUInt32(sector, 0x00),
                    size = BigEndianBitConverter.ToUInt32(sector, 0x04),
                    checksum = BigEndianBitConverter.ToInt32(sector, 0x08),
                    targetID = BigEndianBitConverter.ToUInt32(sector, 0x0C),
                    next_ptr = BigEndianBitConverter.ToUInt32(sector, 0x10),
                    reserved = BigEndianBitConverter.ToUInt32(sector, 0x14)
                };
                ulong entries = (chainEntry.size - 6) / 2;
                chainEntry.blockPairs = new BadBlockEntry[entries];

                DicConsole.DebugWriteLine("Amiga RDB plugin", "chainEntry.magic = 0x{0:X8}", chainEntry.magic);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "chainEntry.size = {0} longs, {1} bytes", chainEntry.size,
                                          chainEntry.size * 4);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "chainEntry.checksum = 0x{0:X8}", chainEntry.checksum);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "chainEntry.targetID = {0}", chainEntry.targetID);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "chainEntry.next_ptr = {0}", chainEntry.next_ptr);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "chainEntry.reserved = 0x{0:X8}", chainEntry.reserved);

                for(ulong i = 0; i < entries; i++)
                {
                    chainEntry.blockPairs[i].badBlock = BigEndianBitConverter.ToUInt32(sector, (int)(0x18 + i * 8 + 0));
                    chainEntry.blockPairs[i].goodBlock =
                        BigEndianBitConverter.ToUInt32(sector, (int)(0x18 + i * 8 + 4));

                    DicConsole.DebugWriteLine("Amiga RDB plugin", "Bad block at {0} replaced with good block at {1}",
                                              chainEntry.blockPairs[i].badBlock, chainEntry.blockPairs[i].goodBlock);
                }

                BadBlockChain.Add(chainEntry);
                nextBlock = chainEntry.next_ptr;
            }

            // Reading BadBlock list
            List<PartitionEntry> PartitionEntries = new List<PartitionEntry>();
            nextBlock = RDB.partition_ptr;
            while(nextBlock != 0xFFFFFFFF)
            {
                DicConsole.DebugWriteLine("Amiga RDB plugin", "Going to block {0} in search of a PartitionEntry block",
                                          nextBlock + sectorOffset);

                sector = imagePlugin.ReadSector(nextBlock + sectorOffset);
                uint magic = BigEndianBitConverter.ToUInt32(sector, 0);

                if(magic != PartitionBlockMagic) break;

                DicConsole.DebugWriteLine("Amiga RDB plugin", "Found PartitionEntry block");

                PartitionEntry partEntry = new PartitionEntry
                {
                    magic = BigEndianBitConverter.ToUInt32(sector, 0x00),
                    size = BigEndianBitConverter.ToUInt32(sector, 0x04),
                    checksum = BigEndianBitConverter.ToInt32(sector, 0x08),
                    targetID = BigEndianBitConverter.ToUInt32(sector, 0x0C),
                    next_ptr = BigEndianBitConverter.ToUInt32(sector, 0x10),
                    flags = BigEndianBitConverter.ToUInt32(sector, 0x14),
                    reserved1 = BigEndianBitConverter.ToUInt32(sector, 0x18),
                    reserved2 = BigEndianBitConverter.ToUInt32(sector, 0x1C),
                    devFlags = BigEndianBitConverter.ToUInt32(sector, 0x20),
                    driveNameLen = sector[0x24],
                    reserved3 = BigEndianBitConverter.ToUInt32(sector, 0x44),
                    reserved4 = BigEndianBitConverter.ToUInt32(sector, 0x48),
                    reserved5 = BigEndianBitConverter.ToUInt32(sector, 0x4C),
                    reserved6 = BigEndianBitConverter.ToUInt32(sector, 0x50),
                    reserved7 = BigEndianBitConverter.ToUInt32(sector, 0x54),
                    reserved8 = BigEndianBitConverter.ToUInt32(sector, 0x58),
                    reserved9 = BigEndianBitConverter.ToUInt32(sector, 0x5C),
                    reserved10 = BigEndianBitConverter.ToUInt32(sector, 0x60),
                    reserved11 = BigEndianBitConverter.ToUInt32(sector, 0x64),
                    reserved12 = BigEndianBitConverter.ToUInt32(sector, 0x68),
                    reserved13 = BigEndianBitConverter.ToUInt32(sector, 0x6C),
                    reserved14 = BigEndianBitConverter.ToUInt32(sector, 0x70),
                    reserved15 = BigEndianBitConverter.ToUInt32(sector, 0x74),
                    reserved16 = BigEndianBitConverter.ToUInt32(sector, 0x78),
                    reserved17 = BigEndianBitConverter.ToUInt32(sector, 0x7C),
                    dosEnvVec = new DOSEnvironmentVector
                    {
                        size = BigEndianBitConverter.ToUInt32(sector, 0x80),
                        block_size = BigEndianBitConverter.ToUInt32(sector, 0x84),
                        sec_org = BigEndianBitConverter.ToUInt32(sector, 0x88),
                        surfaces = BigEndianBitConverter.ToUInt32(sector, 0x8C),
                        spb = BigEndianBitConverter.ToUInt32(sector, 0x90),
                        bpt = BigEndianBitConverter.ToUInt32(sector, 0x94),
                        reservedblocks = BigEndianBitConverter.ToUInt32(sector, 0x98),
                        prealloc = BigEndianBitConverter.ToUInt32(sector, 0x9C),
                        interleave = BigEndianBitConverter.ToUInt32(sector, 0xA0),
                        lowCylinder = BigEndianBitConverter.ToUInt32(sector, 0xA4),
                        highCylinder = BigEndianBitConverter.ToUInt32(sector, 0xA8),
                        numBuffer = BigEndianBitConverter.ToUInt32(sector, 0xAC),
                        bufMemType = BigEndianBitConverter.ToUInt32(sector, 0xB0),
                        maxTransfer = BigEndianBitConverter.ToUInt32(sector, 0xB4),
                        Mask = BigEndianBitConverter.ToUInt32(sector, 0xB8),
                        bootPriority = BigEndianBitConverter.ToUInt32(sector, 0xBC),
                        dosType = BigEndianBitConverter.ToUInt32(sector, 0xC0),
                        baud = BigEndianBitConverter.ToUInt32(sector, 0xC4),
                        control = BigEndianBitConverter.ToUInt32(sector, 0xC8),
                        bootBlocks = BigEndianBitConverter.ToUInt32(sector, 0xCC)
                    }
                };

                byte[] driveName = new byte[32];
                Array.Copy(sector, 0x24, driveName, 0, 32);
                partEntry.driveName = StringHandlers.PascalToString(driveName, Encoding.GetEncoding("iso-8859-1"));

                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.magic = 0x{0:X8}", partEntry.magic);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.size = {0} longs, {1} bytes", partEntry.size,
                                          partEntry.size * 4);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.checksum = 0x{0:X8}", partEntry.checksum);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.targetID = {0}", partEntry.targetID);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.next_ptr = {0}", partEntry.next_ptr);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.flags = 0x{0:X8}", partEntry.flags);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.reserved1 = 0x{0:X8}", partEntry.reserved1);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.reserved2 = 0x{0:X8}", partEntry.reserved2);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.devFlags = 0x{0:X8}", partEntry.devFlags);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.driveNameLen = {0}", partEntry.driveNameLen);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.driveName = \"{0}\"", partEntry.driveName);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.reserved3 = 0x{0:X8}", partEntry.reserved3);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.reserved4 = 0x{0:X8}", partEntry.reserved4);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.reserved5 = 0x{0:X8}", partEntry.reserved5);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.reserved6 = 0x{0:X8}", partEntry.reserved6);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.reserved7 = 0x{0:X8}", partEntry.reserved7);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.reserved8 = 0x{0:X8}", partEntry.reserved8);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.reserved9 = 0x{0:X8}", partEntry.reserved9);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.reserved10 = 0x{0:X8}", partEntry.reserved10);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.reserved11 = 0x{0:X8}", partEntry.reserved11);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.reserved12 = 0x{0:X8}", partEntry.reserved12);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.reserved13 = 0x{0:X8}", partEntry.reserved13);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.reserved14 = 0x{0:X8}", partEntry.reserved14);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.reserved15 = 0x{0:X8}", partEntry.reserved15);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.reserved16 = 0x{0:X8}", partEntry.reserved16);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.reserved17 = 0x{0:X8}", partEntry.reserved17);

                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.dosEnvVec.size = {0} longs, {1} bytes",
                                          partEntry.dosEnvVec.size, partEntry.dosEnvVec.size * 4);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.dosEnvVec.block_size = {0} longs, {1} bytes",
                                          partEntry.dosEnvVec.block_size, partEntry.dosEnvVec.block_size * 4);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.dosEnvVec.sec_org = 0x{0:X8}",
                                          partEntry.dosEnvVec.sec_org);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.dosEnvVec.surfaces = {0}",
                                          partEntry.dosEnvVec.surfaces);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.dosEnvVec.spb = {0}", partEntry.dosEnvVec.spb);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.dosEnvVec.bpt = {0}", partEntry.dosEnvVec.bpt);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.dosEnvVec.reservedblocks = {0}",
                                          partEntry.dosEnvVec.reservedblocks);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.dosEnvVec.prealloc = {0}",
                                          partEntry.dosEnvVec.prealloc);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.dosEnvVec.interleave = {0}",
                                          partEntry.dosEnvVec.interleave);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.dosEnvVec.lowCylinder = {0}",
                                          partEntry.dosEnvVec.lowCylinder);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.dosEnvVec.highCylinder = {0}",
                                          partEntry.dosEnvVec.highCylinder);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.dosEnvVec.numBuffer = {0}",
                                          partEntry.dosEnvVec.numBuffer);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.dosEnvVec.bufMemType = {0}",
                                          partEntry.dosEnvVec.bufMemType);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.dosEnvVec.maxTransfer = {0}",
                                          partEntry.dosEnvVec.maxTransfer);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.dosEnvVec.Mask = 0x{0:X8}",
                                          partEntry.dosEnvVec.Mask);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.dosEnvVec.bootPriority = {0}",
                                          partEntry.dosEnvVec.bootPriority);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.dosEnvVec.dosType = {0}",
                                          AmigaDOSTypeToString(partEntry.dosEnvVec.dosType, true));
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.dosEnvVec.baud = {0}",
                                          partEntry.dosEnvVec.baud);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.dosEnvVec.control = 0x{0:X8}",
                                          partEntry.dosEnvVec.control);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "partEntry.dosEnvVec.bootBlocks = {0}",
                                          partEntry.dosEnvVec.bootBlocks);

                PartitionEntries.Add(partEntry);
                nextBlock = partEntry.next_ptr;
            }

            // Reading BadBlock list
            List<FileSystemHeader> FSHDEntries = new List<FileSystemHeader>();
            List<LoadSegment> SegmentEntries = new List<LoadSegment>();
            nextBlock = RDB.fsheader_ptr;
            while(nextBlock != 0xFFFFFFFF)
            {
                DicConsole.DebugWriteLine("Amiga RDB plugin",
                                          "Going to block {0} in search of a FileSystemHeader block", nextBlock);

                sector = imagePlugin.ReadSector(nextBlock);
                uint magic = BigEndianBitConverter.ToUInt32(sector, 0);

                if(magic != FilesystemHeaderMagic) break;

                DicConsole.DebugWriteLine("Amiga RDB plugin", "Found FileSystemHeader block");

                FileSystemHeader FSHD = new FileSystemHeader
                {
                    magic = BigEndianBitConverter.ToUInt32(sector, 0x00),
                    size = BigEndianBitConverter.ToUInt32(sector, 0x04),
                    checksum = BigEndianBitConverter.ToInt32(sector, 0x08),
                    targetID = BigEndianBitConverter.ToUInt32(sector, 0x0C),
                    next_ptr = BigEndianBitConverter.ToUInt32(sector, 0x10),
                    flags = BigEndianBitConverter.ToUInt32(sector, 0x14),
                    reserved1 = BigEndianBitConverter.ToUInt32(sector, 0x18),
                    reserved2 = BigEndianBitConverter.ToUInt32(sector, 0x1C),
                    dosType = BigEndianBitConverter.ToUInt32(sector, 0x20),
                    version = BigEndianBitConverter.ToUInt32(sector, 0x24),
                    patchFlags = BigEndianBitConverter.ToUInt32(sector, 0x28),
                    dnode = new DeviceNode
                    {
                        type = BigEndianBitConverter.ToUInt32(sector, 0x2C),
                        task = BigEndianBitConverter.ToUInt32(sector, 0x30),
                        locked = BigEndianBitConverter.ToUInt32(sector, 0x34),
                        handler = BigEndianBitConverter.ToUInt32(sector, 0x38),
                        stackSize = BigEndianBitConverter.ToUInt32(sector, 0x3C),
                        priority = BigEndianBitConverter.ToUInt32(sector, 0x40),
                        startup = BigEndianBitConverter.ToUInt32(sector, 0x44),
                        seglist_ptr = BigEndianBitConverter.ToUInt32(sector, 0x48),
                        global_vec = BigEndianBitConverter.ToUInt32(sector, 0x4C)
                    }
                };

                DicConsole.DebugWriteLine("Amiga RDB plugin", "FSHD.magic = 0x{0:X8}", FSHD.magic);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "FSHD.size = {0} longs, {1} bytes", FSHD.size,
                                          FSHD.size * 4);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "FSHD.checksum = 0x{0:X8}", FSHD.checksum);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "FSHD.targetID = {0}", FSHD.targetID);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "FSHD.next_ptr = {0}", FSHD.next_ptr);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "FSHD.flags = 0x{0:X8}", FSHD.flags);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "FSHD.reserved1 = 0x{0:X8}", FSHD.reserved1);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "FSHD.reserved2 = 0x{0:X8}", FSHD.reserved2);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "FSHD.dosType = {0}", AmigaDOSTypeToString(FSHD.dosType));
                DicConsole.DebugWriteLine("Amiga RDB plugin", "FSHD.version = {0:D2}.{1:D2} (0x{2:X8})",
                                          (FSHD.version & 0xFFFF0000) >> 16, FSHD.version & 0xFFFF, FSHD.version);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "FSHD.patchFlags = 0x{0:X8}", FSHD.patchFlags);

                DicConsole.DebugWriteLine("Amiga RDB plugin", "FSHD.dnode.type = {0}", FSHD.dnode.type);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "FSHD.dnode.task = {0}", FSHD.dnode.task);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "FSHD.dnode.locked = {0}", FSHD.dnode.locked);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "FSHD.dnode.handler = {0}", FSHD.dnode.handler);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "FSHD.dnode.stackSize = {0}", FSHD.dnode.stackSize);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "FSHD.dnode.priority = {0}", FSHD.dnode.priority);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "FSHD.dnode.startup = {0}", FSHD.dnode.startup);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "FSHD.dnode.seglist_ptr = {0}", FSHD.dnode.seglist_ptr);
                DicConsole.DebugWriteLine("Amiga RDB plugin", "FSHD.dnode.global_vec = 0x{0:X8}",
                                          FSHD.dnode.global_vec);

                nextBlock = FSHD.dnode.seglist_ptr;
                bool thereAreLoadSegments = false;
                Checksums.SHA1Context sha1Ctx = new Checksums.SHA1Context();
                sha1Ctx.Init();
                while(nextBlock != 0xFFFFFFFF)
                {
                    DicConsole.DebugWriteLine("Amiga RDB plugin", "Going to block {0} in search of a LoadSegment block",
                                              nextBlock);

                    sector = imagePlugin.ReadSector(nextBlock);
                    uint magicSeg = BigEndianBitConverter.ToUInt32(sector, 0);

                    if(magicSeg != LoadSegMagic) break;

                    DicConsole.DebugWriteLine("Amiga RDB plugin", "Found LoadSegment block");

                    thereAreLoadSegments = true;
                    LoadSegment loadSeg = new LoadSegment();
                    loadSeg.magic = BigEndianBitConverter.ToUInt32(sector, 0x00);
                    loadSeg.size = BigEndianBitConverter.ToUInt32(sector, 0x04);
                    loadSeg.checksum = BigEndianBitConverter.ToInt32(sector, 0x08);
                    loadSeg.targetID = BigEndianBitConverter.ToUInt32(sector, 0x0C);
                    loadSeg.next_ptr = BigEndianBitConverter.ToUInt32(sector, 0x10);
                    loadSeg.loadData = new byte[(loadSeg.size - 5) * 4];
                    Array.Copy(sector, 0x14, loadSeg.loadData, 0, (loadSeg.size - 5) * 4);

                    DicConsole.DebugWriteLine("Amiga RDB plugin", "loadSeg.magic = 0x{0:X8}", loadSeg.magic);
                    DicConsole.DebugWriteLine("Amiga RDB plugin", "loadSeg.size = {0} longs, {1} bytes", loadSeg.size,
                                              loadSeg.size * 4);
                    DicConsole.DebugWriteLine("Amiga RDB plugin", "loadSeg.checksum = 0x{0:X8}", loadSeg.checksum);
                    DicConsole.DebugWriteLine("Amiga RDB plugin", "loadSeg.targetID = {0}", loadSeg.targetID);
                    DicConsole.DebugWriteLine("Amiga RDB plugin", "loadSeg.next_ptr = {0}", loadSeg.next_ptr);

                    SegmentEntries.Add(loadSeg);
                    nextBlock = loadSeg.next_ptr;

                    sha1Ctx.Update(loadSeg.loadData);
                }

                if(thereAreLoadSegments)
                {
                    string loadSegSHA1 = sha1Ctx.End();
                    DicConsole.DebugWriteLine("Amiga RDB plugin", "LoadSegment data SHA1: {0}", loadSegSHA1);
                }

                FSHDEntries.Add(FSHD);
                nextBlock = FSHD.next_ptr;
            }

            ulong sequence = 0;
            foreach(PartitionEntry RDBEntry in PartitionEntries)
            {
                CommonTypes.Partition entry = new CommonTypes.Partition
                {
                    Description = AmigaDOSTypeToDescriptionString(RDBEntry.dosEnvVec.dosType),
                    Name = RDBEntry.driveName,
                    Sequence = sequence,
                    Length =
                        (RDBEntry.dosEnvVec.highCylinder + 1 - RDBEntry.dosEnvVec.lowCylinder) *
                        RDBEntry.dosEnvVec.surfaces * RDBEntry.dosEnvVec.bpt,
                    Start = RDBEntry.dosEnvVec.lowCylinder * RDBEntry.dosEnvVec.surfaces * RDBEntry.dosEnvVec.bpt +
                            sectorOffset,
                    Type = AmigaDOSTypeToString(RDBEntry.dosEnvVec.dosType),
                    Scheme = Name
                };
                entry.Offset = entry.Start * RDB.block_size;
                entry.Size = entry.Length * RDB.block_size;

                partitions.Add(entry);
                sequence++;
            }

            return true;
        }

        static string AmigaDOSTypeToDescriptionString(uint AmigaDOSType)
        {
            switch(AmigaDOSType)
            {
                case TypeIDOFS: return "Amiga Original File System";
                case TypeIDFFS: return "Amiga Fast File System";
                case TypeIDOFSi: return "Amiga Original File System with international characters";
                case TypeIDFFSi: return "Amiga Fast File System with international characters";
                case TypeIDOFSc: return "Amiga Original File System with directory cache";
                case TypeIDFFSc: return "Amiga Fast File System with directory cache";
                case TypeIDOFS2: return "Amiga Original File System with long filenames";
                case TypeIDFFS2: return "Amiga Fast File System with long filenames";
                case TypeIDAMIXSysV: return "Amiga UNIX System V filesystem";
                case TypeIDAMIXBoot: return "Amiga UNIX boot filesystem";
                case TypeIDAMIXFFS: return "Amiga UNIX BSD filesystem";
                case TypeIDAMIXReserved: return "Amiga UNIX Reserved partition (swap)";
                case TypeIDPFS:
                case TypeIDPFS2:
                case TypeIDPFSm:
                case TypeIDAFS: return "ProfessionalFileSystem";
                case TypeIDSFS: return "SmartFileSystem v1";
                case TypeIDSFS2: return "SmartFileSystem v2";
                case TypeIDJXFS: return "JXFS";
                case TypeIDCrossDOS: return "FAT, as set by CrossDOS";
                case TypeIDCrossMac: return "HFS, as set by CrossMac";
                case TypeIDBFFS: return "4.2UFS, for BFFS";
                case TypeIDmuOFS: return "Amiga Original File System with multi-user patches";
                case TypeIDmuFFS: return "Amiga Fast File System with multi-user patches";
                case TypeIDmuOFSi:
                    return "Amiga Original File System with international characters and multi-user patches";
                case TypeIDmuFFSi: return "Amiga Fast File System with international characters and multi-user patches";
                case TypeIDmuOFSc: return "Amiga Original File System with directory cache and multi-user patches";
                case TypeIDmuFFSc: return "Amiga Fast File System with directory cache and multi-user patches";
                case TypeIDOldBSDUnused: return "BSD unused";
                case TypeIDOldBSDSwap: return "BSD swap";
                case TypeIDOldBSD42FFS: return "BSD 4.2 FFS";
                case TypeIDOldBSD44LFS: return "BSD 4.4 LFS";
                case TypeIDNetBSDRootUnused: return "NetBSD unused root partition";
                case TypeIDNetBSDRoot42FFS: return "NetBSD 4.2 FFS root partition";
                case TypeIDNetBSDRoot44LFS: return "NetBSD 4.4 LFS root partition";
                case TypeIDNetBSDUserUnused: return "NetBSD unused user partition";
                case TypeIDNetBSDUser42FFS: return "NetBSD 4.2 FFS user partition";
                case TypeIDNetBSDUser44LFS: return "NetBSD 4.4 LFS user partition";
                case TypeIDNetBSDSwap: return "NetBSD swap partition";
                case TypeIDLinux: return "Linux filesystem partition";
                case TypeIDLinuxSwap: return "Linux swap partition";
                case TypeIDRaidFrame:
                case TypeIDRaidFrame0: return "RaidFrame partition";

                default:
                {
                    if((AmigaDOSType & TypeIDOFS) == TypeIDOFS)
                        return string.Format("Unknown Amiga DOS filesystem type {0}",
                                             AmigaDOSTypeToString(AmigaDOSType));

                    if((AmigaDOSType & TypeIDAMIXSysV) == TypeIDAMIXSysV)
                        return string.Format("Unknown Amiga UNIX filesystem type {0}",
                                             AmigaDOSTypeToString(AmigaDOSType));

                    if((AmigaDOSType & 0x50465300) == 0x50465300 || (AmigaDOSType & 0x41465300) == 0x41465300)
                        return string.Format("Unknown ProfessionalFileSystem type {0}",
                                             AmigaDOSTypeToString(AmigaDOSType));

                    if((AmigaDOSType & TypeIDSFS) == TypeIDSFS)
                        return string.Format("Unknown SmartFileSystem type {0}", AmigaDOSTypeToString(AmigaDOSType));

                    if((AmigaDOSType & TypeIDmuOFS) == TypeIDmuOFS)
                        return string.Format("Unknown Amiga DOS multi-user filesystem type {0}",
                                             AmigaDOSTypeToString(AmigaDOSType));

                    if((AmigaDOSType & TypeIDOldBSDUnused) == TypeIDOldBSDUnused)
                        return string.Format("Unknown BSD filesystem type {0}", AmigaDOSTypeToString(AmigaDOSType));

                    if((AmigaDOSType & TypeIDNetBSDRootUnused) == TypeIDNetBSDRootUnused)
                        return string.Format("Unknown NetBSD root filesystem type {0}",
                                             AmigaDOSTypeToString(AmigaDOSType));

                    if((AmigaDOSType & TypeIDNetBSDUserUnused) == TypeIDNetBSDUserUnused)
                        return string.Format("Unknown NetBSD user filesystem type {0}",
                                             AmigaDOSTypeToString(AmigaDOSType));

                    if((AmigaDOSType & TypeIDNetBSDSwap) == TypeIDNetBSDSwap)
                        return string.Format("Unknown NetBSD swap filesystem type {0}",
                                             AmigaDOSTypeToString(AmigaDOSType));

                    if((AmigaDOSType & TypeIDLinux) == TypeIDLinux ||
                       (AmigaDOSType & TypeIDLinuxSwap) == TypeIDLinuxSwap)
                        return string.Format("Unknown Linux filesystem type {0}", AmigaDOSTypeToString(AmigaDOSType));

                    return string.Format("Unknown partition type {0}", AmigaDOSTypeToString(AmigaDOSType));
                }
            }
        }

        static string AmigaDOSTypeToString(uint AmigaDOSType)
        {
            return AmigaDOSTypeToString(AmigaDOSType, true);
        }

        static string AmigaDOSTypeToString(uint AmigaDOSType, bool quoted)
        {
            byte[] textPart = new byte[3];
            string textPartString;

            textPart[0] = (byte)((AmigaDOSType & 0xFF000000) >> 24);
            textPart[1] = (byte)((AmigaDOSType & 0x00FF0000) >> 16);
            textPart[2] = (byte)((AmigaDOSType & 0x0000FF00) >> 8);

            textPartString = Encoding.ASCII.GetString(textPart);

            return quoted
                       ? string.Format("\"{0}\\{1}\"", textPartString, AmigaDOSType & 0xFF)
                       : string.Format("{0}\\{1}", textPartString, AmigaDOSType & 0xFF);
        }
    }
}