// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

// ReSharper disable NotAccessedField.Local

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Partitions;

/// <inheritdoc />
/// <summary>Implements decoding of the Amiga Rigid Disk Block</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed class AmigaRigidDiskBlock : IPartition
{
    /// <summary>RDB magic number "RDSK"</summary>
    const uint RIGID_DISK_BLOCK_MAGIC = 0x5244534B;
    /// <summary>Bad block list magic number "BADB"</summary>
    const uint BAD_BLOCK_LIST_MAGIC = 0x42414442;
    /// <summary>Partition entry magic number "PART"</summary>
    const uint PARTITION_BLOCK_MAGIC = 0x50415254;
    /// <summary>Filesystem header magic number "FSHD"</summary>
    const uint FILESYSTEM_HEADER_MAGIC = 0x46534844;
    /// <summary>LoadSeg block magic number "LSEG"</summary>
    const uint LOAD_SEG_MAGIC = 0x4C534547;

    /// <summary>Type ID for Amiga Original File System, "DOS\0"</summary>
    const uint TYPEID_OFS = 0x444F5300;
    /// <summary>Type ID for Amiga Fast File System, "DOS\1"</summary>
    const uint TYPEID_FFS = 0x444F5301;
    /// <summary>Type ID for Amiga Original File System with international characters, "DOS\2"</summary>
    const uint TYPEID_OFS_INTL = 0x444F5302;
    /// <summary>Type ID for Amiga Fast File System with international characters, "DOS\3"</summary>
    const uint TYPEID_FFS_INTL = 0x444F5303;
    /// <summary>Type ID for Amiga Original File System with directory cache, "DOS\4"</summary>
    const uint TYPEID_OFS_CACHE = 0x444F5304;
    /// <summary>Type ID for Amiga Fast File System with directory cache, "DOS\5"</summary>
    const uint TYPEID_FFS_CACHE = 0x444F5305;
    /// <summary>Type ID for Amiga Original File System with long filenames, "DOS\6"</summary>
    const uint TYPEID_OFS2 = 0x444F5306;
    /// <summary>Type ID for Amiga Fast File System with long filenames, "DOS\7"</summary>
    const uint TYPEID_FFS2 = 0x444F5307;
    /// <summary>Type ID for Amiga UNIX boot filesystem</summary>
    const uint TYPEID_AMIX_BOOT = 0x554E4900;
    /// <summary>Type ID for Amiga UNIX System V filesystem</summary>
    const uint TYPEID_AMIX_SYSV = 0x554E4901;
    /// <summary>Type ID for Amiga UNIX BSD filesystem</summary>
    const uint TYPEID_AMIX_FFS = 0x554E4902;
    /// <summary>Type ID for Amiga UNIX Reserved partition (swap)</summary>
    const uint TYPEID_AMIX_RESERVED = 0x72657376;
    /// <summary>Type ID for ProfessionalFileSystem, "PFS\1"</summary>
    const uint TYPEID_PFS = 0x50465301;
    /// <summary>Type ID for ProfessionalFileSystem, "PFS\2"</summary>
    const uint TYPEID_PFS2 = 0x50465302;
    /// <summary>Type ID for ProfessionalFileSystem, "muAF"</summary>
    const uint TYPEID_PFS_MUSER = 0x6D754146;
    /// <summary>Type ID for ProfessionalFileSystem, "AFS\1"</summary>
    const uint TYPEID_AFS = 0x41465301;
    /// <summary>Type ID for SmartFileSystem v1, "SFS\0"</summary>
    const uint TYPEID_SFS = 0x53465300;
    /// <summary>Type ID for SmartFileSystem v2, "SFS\2"</summary>
    const uint TYPEID_SFS2 = 0x53465302;
    /// <summary>Type ID for JXFS, "JXF\4"</summary>
    const uint TYPEID_JXFS = 0x4A584604;
    /// <summary>Type ID for FAT, as set by CrossDOS, "MSD\0"</summary>
    const uint TYPEID_CROSS_DOS = 0x4D534400;
    /// <summary>Type ID for HFS, as set by CrossMac, "MAC\0"</summary>
    const uint TYPEID_CROSS_MAC = 0x4D414300;
    /// <summary>Type ID for 4.2UFS, for BFFS, "BFFS"</summary>
    const uint TYPEID_BFFS = 0x42464653;
    /// <summary>Type ID for Amiga Original File System with multi-user patches, "muF\0"</summary>
    const uint TYPEID_OFS_MUSER = 0x6D754600;
    /// <summary>Type ID for Amiga Fast File System with multi-user patches, "muF\1"</summary>
    const uint TYPEID_FFS_MUSER = 0x6D754601;
    /// <summary>Type ID for Amiga Original File System with international characters and multi-user patches, "muF\2"</summary>
    const uint TYPEID_OFS_INTL_MUSER = 0x6D754602;
    /// <summary>Type ID for Amiga Fast File System with international characters and multi-user patches, "muF\3"</summary>
    const uint TYPEID_FFS_INTL_MUSER = 0x6D754603;
    /// <summary>Type ID for Amiga Original File System with directory cache and multi-user patches, "muF\4"</summary>
    const uint TYPEID_OFS_CACHE_MUSER = 0x6D754604;
    /// <summary>Type ID for Amiga Fast File System with directory cache and multi-user patches, "muF\5"</summary>
    const uint TYPEID_FFS_CACHE_MUSER = 0x6D754605;
    /// <summary>Type ID for BSD unused, "BSD\0"</summary>
    const uint TYPEID_OLD_BSD_UNUSED = 0x42534400;
    /// <summary>Type ID for BSD swap, "BSD\1"</summary>
    const uint TYPEID_OLD_BSD_SWAP = 0x42534401;
    /// <summary>Type ID for BSD 4.2 FFS, "BSD\7"</summary>
    const uint TYPEID_OLD_BSD42_FFS = 0x42534407;
    /// <summary>Type ID for BSD 4.4 LFS, "BSD\9"</summary>
    const uint TYPEID_OLD_BSD44_LFS = 0x42534409;
    /// <summary>Type ID for NetBSD unused root partition, "NBR\0"</summary>
    const uint TYPEID_NETBSD_ROOT_UNUSED = 0x4E425200;
    /// <summary>Type ID for NetBSD 4.2 FFS root partition, "NBR\7"</summary>

    // ReSharper disable once InconsistentNaming
    const uint TYPEID_NETBSD_ROOT_42FFS = 0x4E425207;
    /// <summary>Type ID for NetBSD 4.4 LFS root partition, "NBR\9"</summary>

    // ReSharper disable once InconsistentNaming
    const uint TYPEID_NETBSD_ROOT_44LFS = 0x4E425209;
    /// <summary>Type ID for NetBSD unused user partition, "NBR\0"</summary>
    const uint TYPEID_NETBSD_USER_UNUSED = 0x4E425500;
    /// <summary>Type ID for NetBSD 4.2 FFS user partition, "NBR\7"</summary>

    // ReSharper disable once InconsistentNaming
    const uint TYPEID_NETBSD_USER_42FFS = 0x4E425507;
    /// <summary>Type ID for NetBSD 4.4 LFS user partition, "NBR\9"</summary>

    // ReSharper disable once InconsistentNaming
    const uint TYPEID_NETBSD_USER_44LFS = 0x4E425509;
    /// <summary>Type ID for NetBSD swap partition</summary>
    const uint TYPEID_NETBSD_SWAP = 0x4E425300;
    /// <summary>Type ID for Linux filesystem partition, "LNX\0"</summary>
    const uint TYPEID_LINUX = 0x4C4E5800;
    /// <summary>Type ID for Linux swap partition, "SWP\0"</summary>
    const uint TYPEID_LINUX_SWAP = 0x53575000;
    /// <summary>Type ID for RaidFrame partition, "RAID"</summary>
    const uint TYPEID_RAID_FRAME = 0x52414944;
    /// <summary>Type ID for RaidFrame partition, "RAI\0"</summary>
    const uint TYPEID_RAID_FRAME0 = 0x52414900;

    /// <summary>No disks to be configured after this one</summary>
    const uint FLAGS_NO_DISKS = 0x00000001;
    /// <summary>No LUNs to be configured after this one</summary>
    const uint FLAGS_NO_LUNS = 0x00000002;
    /// <summary>No target IDs to be configured after this one</summary>
    const uint FLAGS_NO_TARGETS = 0x00000004;
    /// <summary>Don't try to perform reselection with this drive</summary>
    const uint FLAGS_NO_RESELECTION = 0x00000008;
    /// <summary>Disk identification is valid</summary>
    const uint FLAGS_VALID_DISK_ID = 0x00000010;
    /// <summary>Controller identification is valid</summary>
    const uint FLAGS_VALID_CONTROLLER_ID = 0x00000020;
    /// <summary>Drive supports synchronous SCSI mode</summary>
    const uint FLAGS_SYNCH_SCSI = 0x00000040;

    /// <summary>Partition is bootable</summary>
    const uint FLAGS_BOOTABLE = 0x00000001;
    /// <summary>Partition should not be mounted automatically</summary>
    const uint FLAGS_NO_AUTOMOUNT = 0x00000002;
    const string MODULE_NAME = "Amiga Rigid Disk Block (RDB) plugin";

#region IPartition Members

    /// <inheritdoc />
    public string Name => Localization.AmigaRigidDiskBlock_Name;

    /// <inheritdoc />
    public Guid Id => new("8D72ED97-1854-4170-9CE4-6E8446FD9863");

    /// <inheritdoc />
    public string Author => Authors.NATALIA_PORTILLO;

    /// <inheritdoc />
    public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
    {
        partitions = new List<Partition>();
        ulong       rdbBlock = 0;
        var         foundRdb = false;
        ErrorNumber errno;

        while(rdbBlock < 16)
        {
            if(imagePlugin.Info.Sectors <= rdbBlock)
                return false;

            if(rdbBlock + sectorOffset >= imagePlugin.Info.Sectors)
                break;

            errno = imagePlugin.ReadSector(rdbBlock + sectorOffset, out byte[] tmpSector);

            if(errno != ErrorNumber.NoError)
            {
                rdbBlock++;

                continue;
            }

            var magic = BigEndianBitConverter.ToUInt32(tmpSector, 0);

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Possible_magic_at_block_0_is_1_X8, rdbBlock, magic);

            if(magic == RIGID_DISK_BLOCK_MAGIC)
            {
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_RDB_magic_at_block_0, rdbBlock);

                foundRdb = true;

                break;
            }

            rdbBlock++;
        }

        if(!foundRdb)
            return false;

        rdbBlock += sectorOffset;

        var rdb = new RigidDiskBlock();

        errno = imagePlugin.ReadSector(rdbBlock, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        rdb.Magic           = BigEndianBitConverter.ToUInt32(sector, 0x00);
        rdb.Size            = BigEndianBitConverter.ToUInt32(sector, 0x04);
        rdb.Checksum        = BigEndianBitConverter.ToInt32(sector, 0x08);
        rdb.TargetId        = BigEndianBitConverter.ToUInt32(sector, 0x0C);
        rdb.BlockSize       = BigEndianBitConverter.ToUInt32(sector, 0x10);
        rdb.Flags           = BigEndianBitConverter.ToUInt32(sector, 0x04);
        rdb.BadblockPtr     = BigEndianBitConverter.ToUInt32(sector, 0x18);
        rdb.PartitionPtr    = BigEndianBitConverter.ToUInt32(sector, 0x1C);
        rdb.FsheaderPtr     = BigEndianBitConverter.ToUInt32(sector, 0x20);
        rdb.Driveinitcode   = BigEndianBitConverter.ToUInt32(sector, 0x24);
        rdb.Reserved1       = BigEndianBitConverter.ToUInt32(sector, 0x28);
        rdb.Reserved2       = BigEndianBitConverter.ToUInt32(sector, 0x2C);
        rdb.Reserved3       = BigEndianBitConverter.ToUInt32(sector, 0x30);
        rdb.Reserved4       = BigEndianBitConverter.ToUInt32(sector, 0x34);
        rdb.Reserved5       = BigEndianBitConverter.ToUInt32(sector, 0x38);
        rdb.Reserved6       = BigEndianBitConverter.ToUInt32(sector, 0x3C);
        rdb.Cylinders       = BigEndianBitConverter.ToUInt32(sector, 0x40);
        rdb.Spt             = BigEndianBitConverter.ToUInt32(sector, 0x44);
        rdb.Heads           = BigEndianBitConverter.ToUInt32(sector, 0x48);
        rdb.Interleave      = BigEndianBitConverter.ToUInt32(sector, 0x4C);
        rdb.Parking         = BigEndianBitConverter.ToUInt32(sector, 0x50);
        rdb.Reserved7       = BigEndianBitConverter.ToUInt32(sector, 0x54);
        rdb.Reserved8       = BigEndianBitConverter.ToUInt32(sector, 0x58);
        rdb.Reserved9       = BigEndianBitConverter.ToUInt32(sector, 0x5C);
        rdb.Writeprecomp    = BigEndianBitConverter.ToUInt32(sector, 0x60);
        rdb.Reducedwrite    = BigEndianBitConverter.ToUInt32(sector, 0x64);
        rdb.Steprate        = BigEndianBitConverter.ToUInt32(sector, 0x68);
        rdb.Reserved10      = BigEndianBitConverter.ToUInt32(sector, 0x6C);
        rdb.Reserved11      = BigEndianBitConverter.ToUInt32(sector, 0x70);
        rdb.Reserved12      = BigEndianBitConverter.ToUInt32(sector, 0x74);
        rdb.Reserved13      = BigEndianBitConverter.ToUInt32(sector, 0x78);
        rdb.Reserved14      = BigEndianBitConverter.ToUInt32(sector, 0x7C);
        rdb.RdbBlockLow     = BigEndianBitConverter.ToUInt32(sector, 0x80);
        rdb.RdbBlockHigh    = BigEndianBitConverter.ToUInt32(sector, 0x84);
        rdb.LowCylinder     = BigEndianBitConverter.ToUInt32(sector, 0x88);
        rdb.HighCylinder    = BigEndianBitConverter.ToUInt32(sector, 0x8C);
        rdb.CylBlocks       = BigEndianBitConverter.ToUInt32(sector, 0x90);
        rdb.AutoParkSeconds = BigEndianBitConverter.ToUInt32(sector, 0x94);
        rdb.HighCylinder    = BigEndianBitConverter.ToUInt32(sector, 0x98);
        rdb.Reserved15      = BigEndianBitConverter.ToUInt32(sector, 0x9C);

        var tmpString = new byte[8];
        Array.Copy(sector, 0xA0, tmpString, 0, 8);
        rdb.DiskVendor = StringHandlers.SpacePaddedToString(tmpString);
        tmpString      = new byte[16];
        Array.Copy(sector, 0xA8, tmpString, 0, 16);
        rdb.DiskProduct = StringHandlers.SpacePaddedToString(tmpString);
        tmpString       = new byte[4];
        Array.Copy(sector, 0xB8, tmpString, 0, 4);
        rdb.DiskRevision = StringHandlers.SpacePaddedToString(tmpString);

        tmpString = new byte[8];
        Array.Copy(sector, 0xBC, tmpString, 0, 8);
        rdb.ControllerVendor = StringHandlers.SpacePaddedToString(tmpString);
        tmpString            = new byte[16];
        Array.Copy(sector, 0xC4, tmpString, 0, 16);
        rdb.ControllerProduct = StringHandlers.SpacePaddedToString(tmpString);
        tmpString             = new byte[4];
        Array.Copy(sector, 0xD4, tmpString, 0, 4);
        rdb.ControllerRevision = StringHandlers.SpacePaddedToString(tmpString);

        rdb.Reserved16 = BigEndianBitConverter.ToUInt32(sector, 0xD8);
        rdb.Reserved17 = BigEndianBitConverter.ToUInt32(sector, 0xDC);
        rdb.Reserved18 = BigEndianBitConverter.ToUInt32(sector, 0xE0);
        rdb.Reserved19 = BigEndianBitConverter.ToUInt32(sector, 0xE4);
        rdb.Reserved20 = BigEndianBitConverter.ToUInt32(sector, 0xE8);
        rdb.Reserved21 = BigEndianBitConverter.ToUInt32(sector, 0xEC);
        rdb.Reserved22 = BigEndianBitConverter.ToUInt32(sector, 0xF0);
        rdb.Reserved23 = BigEndianBitConverter.ToUInt32(sector, 0xF4);
        rdb.Reserved24 = BigEndianBitConverter.ToUInt32(sector, 0xF8);
        rdb.Reserved25 = BigEndianBitConverter.ToUInt32(sector, 0xFC);

        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.magic = 0x{0:X8}",             rdb.Magic);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.size = {0} longs, {1} bytes",  rdb.Size, rdb.Size * 4);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.checksum = 0x{0:X8}",          rdb.Checksum);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.targetID = {0}",               rdb.TargetId);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.block_size = {0}",             rdb.BlockSize);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.badblock_ptr = {0}",           rdb.BadblockPtr);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.partition_ptr = {0}",          rdb.PartitionPtr);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.fsheader_ptr = {0}",           rdb.FsheaderPtr);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.driveinitcode = {0}",          rdb.Driveinitcode);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reserved1 = 0x{0:X8}",         rdb.Reserved1);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reserved2 = 0x{0:X8}",         rdb.Reserved2);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reserved3 = 0x{0:X8}",         rdb.Reserved3);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reserved4 = 0x{0:X8}",         rdb.Reserved4);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reserved5 = 0x{0:X8}",         rdb.Reserved5);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reserved6 = 0x{0:X8}",         rdb.Reserved6);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.cylinders = {0}",              rdb.Cylinders);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.spt = {0}",                    rdb.Spt);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.heads = {0}",                  rdb.Heads);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.interleave = {0}",             rdb.Interleave);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.parking = {0}",                rdb.Parking);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reserved7 = 0x{0:X8}",         rdb.Reserved7);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reserved8 = 0x{0:X8}",         rdb.Reserved8);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reserved9 = 0x{0:X8}",         rdb.Reserved9);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.writeprecomp = {0}",           rdb.Writeprecomp);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reducedwrite = {0}",           rdb.Reducedwrite);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.steprate = {0}",               rdb.Steprate);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reserved10 = 0x{0:X8}",        rdb.Reserved10);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reserved11 = 0x{0:X8}",        rdb.Reserved11);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reserved12 = 0x{0:X8}",        rdb.Reserved12);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reserved13 = 0x{0:X8}",        rdb.Reserved13);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reserved14 = 0x{0:X8}",        rdb.Reserved14);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.RDBBlockLow = {0}",            rdb.RdbBlockLow);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.RDBBlockHigh = {0}",           rdb.RdbBlockHigh);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.LowCylinder = {0}",            rdb.LowCylinder);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.HighCylinder = {0}",           rdb.HighCylinder);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.CylBlocks = {0}",              rdb.CylBlocks);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.AutoParkSeconds = {0}",        rdb.AutoParkSeconds);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.HighCylinder = {0}",           rdb.HighCylinder);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reserved15 = 0x{0:X8}",        rdb.Reserved15);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.diskVendor = \"{0}\"",         rdb.DiskVendor);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.diskProduct = \"{0}\"",        rdb.DiskProduct);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.diskRevision = \"{0}\"",       rdb.DiskRevision);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.controllerVendor = \"{0}\"",   rdb.ControllerVendor);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.controllerProduct = \"{0}\"",  rdb.ControllerProduct);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.controllerRevision = \"{0}\"", rdb.ControllerRevision);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reserved16 = 0x{0:X8}",        rdb.Reserved16);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reserved17 = 0x{0:X8}",        rdb.Reserved17);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reserved18 = 0x{0:X8}",        rdb.Reserved18);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reserved19 = 0x{0:X8}",        rdb.Reserved19);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reserved20 = 0x{0:X8}",        rdb.Reserved20);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reserved21 = 0x{0:X8}",        rdb.Reserved21);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reserved22 = 0x{0:X8}",        rdb.Reserved22);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reserved23 = 0x{0:X8}",        rdb.Reserved23);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reserved24 = 0x{0:X8}",        rdb.Reserved24);
        AaruConsole.DebugWriteLine(MODULE_NAME, "RDB.reserved25 = 0x{0:X8}",        rdb.Reserved25);

        // Reading BadBlock list
        List<BadBlockList> badBlockChain = new();
        ulong              nextBlock     = rdb.BadblockPtr;

        while(nextBlock != 0xFFFFFFFF)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Going_to_block_0_in_search_of_a_BadBlock_block,
                                       nextBlock);

            errno = imagePlugin.ReadSector(nextBlock, out sector);

            if(errno != ErrorNumber.NoError)
                break;

            var magic = BigEndianBitConverter.ToUInt32(sector, 0);

            if(magic != BAD_BLOCK_LIST_MAGIC)
                break;

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_BadBlock_block);

            var chainEntry = new BadBlockList
            {
                Magic    = BigEndianBitConverter.ToUInt32(sector, 0x00),
                Size     = BigEndianBitConverter.ToUInt32(sector, 0x04),
                Checksum = BigEndianBitConverter.ToInt32(sector, 0x08),
                TargetId = BigEndianBitConverter.ToUInt32(sector, 0x0C),
                NextPtr  = BigEndianBitConverter.ToUInt32(sector, 0x10),
                Reserved = BigEndianBitConverter.ToUInt32(sector, 0x14)
            };

            ulong entries = (chainEntry.Size - 6) / 2;
            chainEntry.BlockPairs = new BadBlockEntry[entries];

            AaruConsole.DebugWriteLine(MODULE_NAME, "chainEntry.magic = 0x{0:X8}", chainEntry.Magic);

            AaruConsole.DebugWriteLine(MODULE_NAME, "chainEntry.size = {0} longs, {1} bytes", chainEntry.Size,
                                       chainEntry.Size * 4);

            AaruConsole.DebugWriteLine(MODULE_NAME, "chainEntry.checksum = 0x{0:X8}", chainEntry.Checksum);
            AaruConsole.DebugWriteLine(MODULE_NAME, "chainEntry.targetID = {0}",      chainEntry.TargetId);
            AaruConsole.DebugWriteLine(MODULE_NAME, "chainEntry.next_ptr = {0}",      chainEntry.NextPtr);
            AaruConsole.DebugWriteLine(MODULE_NAME, "chainEntry.reserved = 0x{0:X8}", chainEntry.Reserved);

            for(ulong i = 0; i < entries; i++)
            {
                chainEntry.BlockPairs[i].BadBlock = BigEndianBitConverter.ToUInt32(sector, (int)(0x18 + i * 8 + 0));

                chainEntry.BlockPairs[i].GoodBlock = BigEndianBitConverter.ToUInt32(sector, (int)(0x18 + i * 8 + 4));

                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Bad_block_at_0_replaced_with_good_block_at_1,
                                           chainEntry.BlockPairs[i].BadBlock, chainEntry.BlockPairs[i].GoodBlock);
            }

            badBlockChain.Add(chainEntry);
            nextBlock = chainEntry.NextPtr;
        }

        // Reading BadBlock list
        List<PartitionEntry> partitionEntries = new();
        nextBlock = rdb.PartitionPtr;

        while(nextBlock != 0xFFFFFFFF)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Going_to_block_0_in_search_of_a_PartitionEntry_block,
                                       nextBlock + sectorOffset);

            errno = imagePlugin.ReadSector(nextBlock + sectorOffset, out sector);

            if(errno != ErrorNumber.NoError)
                break;

            var magic = BigEndianBitConverter.ToUInt32(sector, 0);

            if(magic != PARTITION_BLOCK_MAGIC)
                break;

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_PartitionEntry_block);

            var partEntry = new PartitionEntry
            {
                Magic        = BigEndianBitConverter.ToUInt32(sector, 0x00),
                Size         = BigEndianBitConverter.ToUInt32(sector, 0x04),
                Checksum     = BigEndianBitConverter.ToInt32(sector, 0x08),
                TargetId     = BigEndianBitConverter.ToUInt32(sector, 0x0C),
                NextPtr      = BigEndianBitConverter.ToUInt32(sector, 0x10),
                Flags        = BigEndianBitConverter.ToUInt32(sector, 0x14),
                Reserved1    = BigEndianBitConverter.ToUInt32(sector, 0x18),
                Reserved2    = BigEndianBitConverter.ToUInt32(sector, 0x1C),
                DevFlags     = BigEndianBitConverter.ToUInt32(sector, 0x20),
                DriveNameLen = sector[0x24],
                Reserved3    = BigEndianBitConverter.ToUInt32(sector, 0x44),
                Reserved4    = BigEndianBitConverter.ToUInt32(sector, 0x48),
                Reserved5    = BigEndianBitConverter.ToUInt32(sector, 0x4C),
                Reserved6    = BigEndianBitConverter.ToUInt32(sector, 0x50),
                Reserved7    = BigEndianBitConverter.ToUInt32(sector, 0x54),
                Reserved8    = BigEndianBitConverter.ToUInt32(sector, 0x58),
                Reserved9    = BigEndianBitConverter.ToUInt32(sector, 0x5C),
                Reserved10   = BigEndianBitConverter.ToUInt32(sector, 0x60),
                Reserved11   = BigEndianBitConverter.ToUInt32(sector, 0x64),
                Reserved12   = BigEndianBitConverter.ToUInt32(sector, 0x68),
                Reserved13   = BigEndianBitConverter.ToUInt32(sector, 0x6C),
                Reserved14   = BigEndianBitConverter.ToUInt32(sector, 0x70),
                Reserved15   = BigEndianBitConverter.ToUInt32(sector, 0x74),
                Reserved16   = BigEndianBitConverter.ToUInt32(sector, 0x78),
                Reserved17   = BigEndianBitConverter.ToUInt32(sector, 0x7C),
                DosEnvVec = new DosEnvironmentVector
                {
                    Size           = BigEndianBitConverter.ToUInt32(sector, 0x80),
                    BlockSize      = BigEndianBitConverter.ToUInt32(sector, 0x84),
                    SecOrg         = BigEndianBitConverter.ToUInt32(sector, 0x88),
                    Surfaces       = BigEndianBitConverter.ToUInt32(sector, 0x8C),
                    Spb            = BigEndianBitConverter.ToUInt32(sector, 0x90),
                    Bpt            = BigEndianBitConverter.ToUInt32(sector, 0x94),
                    Reservedblocks = BigEndianBitConverter.ToUInt32(sector, 0x98),
                    Prealloc       = BigEndianBitConverter.ToUInt32(sector, 0x9C),
                    Interleave     = BigEndianBitConverter.ToUInt32(sector, 0xA0),
                    LowCylinder    = BigEndianBitConverter.ToUInt32(sector, 0xA4),
                    HighCylinder   = BigEndianBitConverter.ToUInt32(sector, 0xA8),
                    NumBuffer      = BigEndianBitConverter.ToUInt32(sector, 0xAC),
                    BufMemType     = BigEndianBitConverter.ToUInt32(sector, 0xB0),
                    MaxTransfer    = BigEndianBitConverter.ToUInt32(sector, 0xB4),
                    Mask           = BigEndianBitConverter.ToUInt32(sector, 0xB8),
                    BootPriority   = BigEndianBitConverter.ToUInt32(sector, 0xBC),
                    DosType        = BigEndianBitConverter.ToUInt32(sector, 0xC0),
                    Baud           = BigEndianBitConverter.ToUInt32(sector, 0xC4),
                    Control        = BigEndianBitConverter.ToUInt32(sector, 0xC8),
                    BootBlocks     = BigEndianBitConverter.ToUInt32(sector, 0xCC)
                }
            };

            var driveName = new byte[32];
            Array.Copy(sector, 0x24, driveName, 0, 32);
            partEntry.DriveName = StringHandlers.PascalToString(driveName, Encoding.GetEncoding("iso-8859-1"));

            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.magic = 0x{0:X8}", partEntry.Magic);

            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.size = {0} longs, {1} bytes", partEntry.Size,
                                       partEntry.Size * 4);

            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.checksum = 0x{0:X8}",   partEntry.Checksum);
            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.targetID = {0}",        partEntry.TargetId);
            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.next_ptr = {0}",        partEntry.NextPtr);
            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.flags = 0x{0:X8}",      partEntry.Flags);
            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.reserved1 = 0x{0:X8}",  partEntry.Reserved1);
            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.reserved2 = 0x{0:X8}",  partEntry.Reserved2);
            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.devFlags = 0x{0:X8}",   partEntry.DevFlags);
            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.driveNameLen = {0}",    partEntry.DriveNameLen);
            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.driveName = \"{0}\"",   partEntry.DriveName);
            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.reserved3 = 0x{0:X8}",  partEntry.Reserved3);
            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.reserved4 = 0x{0:X8}",  partEntry.Reserved4);
            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.reserved5 = 0x{0:X8}",  partEntry.Reserved5);
            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.reserved6 = 0x{0:X8}",  partEntry.Reserved6);
            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.reserved7 = 0x{0:X8}",  partEntry.Reserved7);
            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.reserved8 = 0x{0:X8}",  partEntry.Reserved8);
            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.reserved9 = 0x{0:X8}",  partEntry.Reserved9);
            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.reserved10 = 0x{0:X8}", partEntry.Reserved10);
            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.reserved11 = 0x{0:X8}", partEntry.Reserved11);
            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.reserved12 = 0x{0:X8}", partEntry.Reserved12);
            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.reserved13 = 0x{0:X8}", partEntry.Reserved13);
            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.reserved14 = 0x{0:X8}", partEntry.Reserved14);
            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.reserved15 = 0x{0:X8}", partEntry.Reserved15);
            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.reserved16 = 0x{0:X8}", partEntry.Reserved16);
            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.reserved17 = 0x{0:X8}", partEntry.Reserved17);

            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.dosEnvVec.size = {0} longs, {1} bytes",
                                       partEntry.DosEnvVec.Size, partEntry.DosEnvVec.Size * 4);

            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.dosEnvVec.block_size = {0} longs, {1} bytes",
                                       partEntry.DosEnvVec.BlockSize, partEntry.DosEnvVec.BlockSize * 4);

            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.dosEnvVec.sec_org = 0x{0:X8}",
                                       partEntry.DosEnvVec.SecOrg);

            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.dosEnvVec.surfaces = {0}", partEntry.DosEnvVec.Surfaces);

            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.dosEnvVec.spb = {0}", partEntry.DosEnvVec.Spb);

            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.dosEnvVec.bpt = {0}", partEntry.DosEnvVec.Bpt);

            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.dosEnvVec.reservedblocks = {0}",
                                       partEntry.DosEnvVec.Reservedblocks);

            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.dosEnvVec.prealloc = {0}", partEntry.DosEnvVec.Prealloc);

            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.dosEnvVec.interleave = {0}",
                                       partEntry.DosEnvVec.Interleave);

            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.dosEnvVec.lowCylinder = {0}",
                                       partEntry.DosEnvVec.LowCylinder);

            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.dosEnvVec.highCylinder = {0}",
                                       partEntry.DosEnvVec.HighCylinder);

            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.dosEnvVec.numBuffer = {0}",
                                       partEntry.DosEnvVec.NumBuffer);

            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.dosEnvVec.bufMemType = {0}",
                                       partEntry.DosEnvVec.BufMemType);

            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.dosEnvVec.maxTransfer = {0}",
                                       partEntry.DosEnvVec.MaxTransfer);

            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.dosEnvVec.Mask = 0x{0:X8}", partEntry.DosEnvVec.Mask);

            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.dosEnvVec.bootPriority = {0}",
                                       partEntry.DosEnvVec.BootPriority);

            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.dosEnvVec.dosType = {0}",
                                       AmigaDosTypeToString(partEntry.DosEnvVec.DosType));

            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.dosEnvVec.baud = {0}", partEntry.DosEnvVec.Baud);

            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.dosEnvVec.control = 0x{0:X8}",
                                       partEntry.DosEnvVec.Control);

            AaruConsole.DebugWriteLine(MODULE_NAME, "partEntry.dosEnvVec.bootBlocks = {0}",
                                       partEntry.DosEnvVec.BootBlocks);

            partitionEntries.Add(partEntry);
            nextBlock = partEntry.NextPtr;
        }

        // Reading BadBlock list
        List<FileSystemHeader> fshdEntries    = new();
        List<LoadSegment>      segmentEntries = new();
        nextBlock = rdb.FsheaderPtr;

        while(nextBlock != 0xFFFFFFFF)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Going_to_block_0_in_search_of_a_FileSystemHeader_block,
                                       nextBlock);

            errno = imagePlugin.ReadSector(nextBlock, out sector);

            if(errno != ErrorNumber.NoError)
                break;

            var magic = BigEndianBitConverter.ToUInt32(sector, 0);

            if(magic != FILESYSTEM_HEADER_MAGIC)
                break;

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_FileSystemHeader_block);

            var fshd = new FileSystemHeader
            {
                Magic      = BigEndianBitConverter.ToUInt32(sector, 0x00),
                Size       = BigEndianBitConverter.ToUInt32(sector, 0x04),
                Checksum   = BigEndianBitConverter.ToInt32(sector, 0x08),
                TargetId   = BigEndianBitConverter.ToUInt32(sector, 0x0C),
                NextPtr    = BigEndianBitConverter.ToUInt32(sector, 0x10),
                Flags      = BigEndianBitConverter.ToUInt32(sector, 0x14),
                Reserved1  = BigEndianBitConverter.ToUInt32(sector, 0x18),
                Reserved2  = BigEndianBitConverter.ToUInt32(sector, 0x1C),
                DosType    = BigEndianBitConverter.ToUInt32(sector, 0x20),
                Version    = BigEndianBitConverter.ToUInt32(sector, 0x24),
                PatchFlags = BigEndianBitConverter.ToUInt32(sector, 0x28),
                Dnode = new DeviceNode
                {
                    Type       = BigEndianBitConverter.ToUInt32(sector, 0x2C),
                    Task       = BigEndianBitConverter.ToUInt32(sector, 0x30),
                    Locked     = BigEndianBitConverter.ToUInt32(sector, 0x34),
                    Handler    = BigEndianBitConverter.ToUInt32(sector, 0x38),
                    StackSize  = BigEndianBitConverter.ToUInt32(sector, 0x3C),
                    Priority   = BigEndianBitConverter.ToUInt32(sector, 0x40),
                    Startup    = BigEndianBitConverter.ToUInt32(sector, 0x44),
                    SeglistPtr = BigEndianBitConverter.ToUInt32(sector, 0x48),
                    GlobalVec  = BigEndianBitConverter.ToUInt32(sector, 0x4C)
                }
            };

            AaruConsole.DebugWriteLine(MODULE_NAME, "FSHD.magic = 0x{0:X8}", fshd.Magic);

            AaruConsole.DebugWriteLine(MODULE_NAME, "FSHD.size = {0} longs, {1} bytes", fshd.Size, fshd.Size * 4);

            AaruConsole.DebugWriteLine(MODULE_NAME, "FSHD.checksum = 0x{0:X8}",  fshd.Checksum);
            AaruConsole.DebugWriteLine(MODULE_NAME, "FSHD.targetID = {0}",       fshd.TargetId);
            AaruConsole.DebugWriteLine(MODULE_NAME, "FSHD.next_ptr = {0}",       fshd.NextPtr);
            AaruConsole.DebugWriteLine(MODULE_NAME, "FSHD.flags = 0x{0:X8}",     fshd.Flags);
            AaruConsole.DebugWriteLine(MODULE_NAME, "FSHD.reserved1 = 0x{0:X8}", fshd.Reserved1);
            AaruConsole.DebugWriteLine(MODULE_NAME, "FSHD.reserved2 = 0x{0:X8}", fshd.Reserved2);

            AaruConsole.DebugWriteLine(MODULE_NAME, "FSHD.dosType = {0}", AmigaDosTypeToString(fshd.DosType));

            AaruConsole.DebugWriteLine(MODULE_NAME, "FSHD.version = {0:D2}.{1:D2} (0x{2:X8})",
                                       (fshd.Version & 0xFFFF0000) >> 16, fshd.Version & 0xFFFF, fshd.Version);

            AaruConsole.DebugWriteLine(MODULE_NAME, "FSHD.patchFlags = 0x{0:X8}", fshd.PatchFlags);

            AaruConsole.DebugWriteLine(MODULE_NAME, "FSHD.dnode.type = {0}",        fshd.Dnode.Type);
            AaruConsole.DebugWriteLine(MODULE_NAME, "FSHD.dnode.task = {0}",        fshd.Dnode.Task);
            AaruConsole.DebugWriteLine(MODULE_NAME, "FSHD.dnode.locked = {0}",      fshd.Dnode.Locked);
            AaruConsole.DebugWriteLine(MODULE_NAME, "FSHD.dnode.handler = {0}",     fshd.Dnode.Handler);
            AaruConsole.DebugWriteLine(MODULE_NAME, "FSHD.dnode.stackSize = {0}",   fshd.Dnode.StackSize);
            AaruConsole.DebugWriteLine(MODULE_NAME, "FSHD.dnode.priority = {0}",    fshd.Dnode.Priority);
            AaruConsole.DebugWriteLine(MODULE_NAME, "FSHD.dnode.startup = {0}",     fshd.Dnode.Startup);
            AaruConsole.DebugWriteLine(MODULE_NAME, "FSHD.dnode.seglist_ptr = {0}", fshd.Dnode.SeglistPtr);

            AaruConsole.DebugWriteLine(MODULE_NAME, "FSHD.dnode.global_vec = 0x{0:X8}", fshd.Dnode.GlobalVec);

            nextBlock = fshd.Dnode.SeglistPtr;
            var thereAreLoadSegments = false;
            var sha1Ctx              = new Sha1Context();

            while(nextBlock != 0xFFFFFFFF)
            {
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Going_to_block_0_in_search_of_a_LoadSegment_block,
                                           nextBlock);

                errno = imagePlugin.ReadSector(nextBlock, out sector);

                if(errno != ErrorNumber.NoError)
                    break;

                var magicSeg = BigEndianBitConverter.ToUInt32(sector, 0);

                if(magicSeg != LOAD_SEG_MAGIC)
                    break;

                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_LoadSegment_block);

                thereAreLoadSegments = true;

                var loadSeg = new LoadSegment
                {
                    Magic    = BigEndianBitConverter.ToUInt32(sector, 0x00),
                    Size     = BigEndianBitConverter.ToUInt32(sector, 0x04),
                    Checksum = BigEndianBitConverter.ToInt32(sector, 0x08),
                    TargetId = BigEndianBitConverter.ToUInt32(sector, 0x0C),
                    NextPtr  = BigEndianBitConverter.ToUInt32(sector, 0x10)
                };

                loadSeg.LoadData = new byte[(loadSeg.Size - 5) * 4];
                Array.Copy(sector, 0x14, loadSeg.LoadData, 0, (loadSeg.Size - 5) * 4);

                AaruConsole.DebugWriteLine(MODULE_NAME, "loadSeg.magic = 0x{0:X8}", loadSeg.Magic);

                AaruConsole.DebugWriteLine(MODULE_NAME, "loadSeg.size = {0} longs, {1} bytes", loadSeg.Size,
                                           loadSeg.Size * 4);

                AaruConsole.DebugWriteLine(MODULE_NAME, "loadSeg.checksum = 0x{0:X8}", loadSeg.Checksum);
                AaruConsole.DebugWriteLine(MODULE_NAME, "loadSeg.targetID = {0}",      loadSeg.TargetId);
                AaruConsole.DebugWriteLine(MODULE_NAME, "loadSeg.next_ptr = {0}",      loadSeg.NextPtr);

                segmentEntries.Add(loadSeg);
                nextBlock = loadSeg.NextPtr;

                sha1Ctx.Update(loadSeg.LoadData);
            }

            if(thereAreLoadSegments)
            {
                string loadSegSha1 = sha1Ctx.End();
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.LoadSegment_data_SHA1_0, loadSegSha1);
            }

            fshdEntries.Add(fshd);
            nextBlock = fshd.NextPtr;
        }

        ulong sequence = 0;

        foreach(Partition entry in partitionEntries.Select(rdbEntry => new Partition
            {
                Description = AmigaDosTypeToDescriptionString(rdbEntry.DosEnvVec.DosType),
                Name        = rdbEntry.DriveName,
                Sequence    = sequence,
                Length = (rdbEntry.DosEnvVec.HighCylinder + 1 - rdbEntry.DosEnvVec.LowCylinder) *
                         rdbEntry.DosEnvVec.Surfaces                                            *
                         rdbEntry.DosEnvVec.Bpt,
                Start = rdbEntry.DosEnvVec.LowCylinder * rdbEntry.DosEnvVec.Surfaces * rdbEntry.DosEnvVec.Bpt +
                        sectorOffset,
                Type   = AmigaDosTypeToString(rdbEntry.DosEnvVec.DosType),
                Scheme = Name,
                Offset = (rdbEntry.DosEnvVec.LowCylinder * rdbEntry.DosEnvVec.Surfaces * rdbEntry.DosEnvVec.Bpt +
                          sectorOffset) *
                         rdb.BlockSize,
                Size = (rdbEntry.DosEnvVec.HighCylinder + 1 - rdbEntry.DosEnvVec.LowCylinder) *
                       rdbEntry.DosEnvVec.Surfaces                                            *
                       rdbEntry.DosEnvVec.Bpt                                                 *
                       rdb.BlockSize
            }))
        {
            partitions.Add(entry);
            sequence++;
        }

        return true;
    }

#endregion

    static string AmigaDosTypeToDescriptionString(uint amigaDosType)
    {
        switch(amigaDosType)
        {
            case TYPEID_OFS:
                return Localization.Amiga_Original_File_System;
            case TYPEID_FFS:
                return Localization.Amiga_Fast_File_System;
            case TYPEID_OFS_INTL:
                return Localization.Amiga_Original_File_System_with_international_characters;
            case TYPEID_FFS_INTL:
                return Localization.Amiga_Fast_File_System_with_international_characters;
            case TYPEID_OFS_CACHE:
                return Localization.Amiga_Original_File_System_with_directory_cache;
            case TYPEID_FFS_CACHE:
                return Localization.Amiga_Fast_File_System_with_directory_cache;
            case TYPEID_OFS2:
                return Localization.Amiga_Original_File_System_with_long_filenames;
            case TYPEID_FFS2:
                return Localization.Amiga_Fast_File_System_with_long_filenames;
            case TYPEID_AMIX_SYSV:
                return Localization.Amiga_UNIX_System_V_filesystem;
            case TYPEID_AMIX_BOOT:
                return Localization.Amiga_UNIX_boot_filesystem;
            case TYPEID_AMIX_FFS:
                return Localization.Amiga_UNIX_BSD_filesystem;
            case TYPEID_AMIX_RESERVED:
                return Localization.Amiga_UNIX_Reserved_partition__swap_;
            case TYPEID_PFS:
            case TYPEID_PFS2:
            case TYPEID_PFS_MUSER:
            case TYPEID_AFS:
                return Localization.ProfessionalFileSystem;
            case TYPEID_SFS:
                return Localization.SmartFileSystem_v1;
            case TYPEID_SFS2:
                return Localization.SmartFileSystem_v2;
            case TYPEID_JXFS:
                return Localization.JXFS;
            case TYPEID_CROSS_DOS:
                return Localization.FAT_as_set_by_CrossDOS;
            case TYPEID_CROSS_MAC:
                return Localization.HFS_as_set_by_CrossMac;
            case TYPEID_BFFS:
                return Localization._4_2_UFS_for_BFFS;
            case TYPEID_OFS_MUSER:
                return Localization.Amiga_Original_File_System_with_multi_user_patches;
            case TYPEID_FFS_MUSER:
                return Localization.Amiga_Fast_File_System_with_multi_user_patches;
            case TYPEID_OFS_INTL_MUSER:
                return Localization.Amiga_Original_File_System_with_international_characters_and_multi_user_patches;
            case TYPEID_FFS_INTL_MUSER:
                return Localization.Amiga_Fast_File_System_with_international_characters_and_multi_user_patches;
            case TYPEID_OFS_CACHE_MUSER:
                return Localization.Amiga_Original_File_System_with_directory_cache_and_multi_user_patches;
            case TYPEID_FFS_CACHE_MUSER:
                return Localization.Amiga_Fast_File_System_with_directory_cache_and_multi_user_patches;
            case TYPEID_OLD_BSD_UNUSED:
                return Localization.BSD_unused;
            case TYPEID_OLD_BSD_SWAP:
                return Localization.BSD_swap;
            case TYPEID_OLD_BSD42_FFS:
                return Localization.BSD_4_2_FFS;
            case TYPEID_OLD_BSD44_LFS:
                return Localization.BSD_4_4_LFS;
            case TYPEID_NETBSD_ROOT_UNUSED:
                return Localization.NetBSD_unused_root_partition;
            case TYPEID_NETBSD_ROOT_42FFS:
                return Localization.NetBSD_4_2_FFS_root_partition;
            case TYPEID_NETBSD_ROOT_44LFS:
                return Localization.NetBSD_4_4_LFS_root_partition;
            case TYPEID_NETBSD_USER_UNUSED:
                return Localization.NetBSD_unused_user_partition;
            case TYPEID_NETBSD_USER_42FFS:
                return Localization.NetBSD_4_2_FFS_user_partition;
            case TYPEID_NETBSD_USER_44LFS:
                return Localization.NetBSD_4_4_LFS_user_partition;
            case TYPEID_NETBSD_SWAP:
                return Localization.NetBSD_swap_partition;
            case TYPEID_LINUX:
                return Localization.Linux_filesystem_partition;
            case TYPEID_LINUX_SWAP:
                return Localization.Linux_swap_partition;
            case TYPEID_RAID_FRAME:
            case TYPEID_RAID_FRAME0:
                return Localization.RaidFrame_partition;

            default:
            {
                if((amigaDosType & TYPEID_OFS) == TYPEID_OFS)
                {
                    return string.Format(Localization.Unknown_Amiga_DOS_filesystem_type_0,
                                         AmigaDosTypeToString(amigaDosType));
                }

                if((amigaDosType & TYPEID_AMIX_SYSV) == TYPEID_AMIX_SYSV)
                {
                    return string.Format(Localization.Unknown_Amiga_UNIX_filesystem_type_0,
                                         AmigaDosTypeToString(amigaDosType));
                }

                if((amigaDosType & 0x50465300) == 0x50465300 || (amigaDosType & 0x41465300) == 0x41465300)
                {
                    return string.Format(Localization.Unknown_ProfessionalFileSystem_type_0,
                                         AmigaDosTypeToString(amigaDosType));
                }

                if((amigaDosType & TYPEID_SFS) == TYPEID_SFS)
                {
                    return string.Format(Localization.Unknown_SmartFileSystem_type_0,
                                         AmigaDosTypeToString(amigaDosType));
                }

                if((amigaDosType & TYPEID_OFS_MUSER) == TYPEID_OFS_MUSER)
                {
                    return string.Format(Localization.Unknown_Amiga_DOS_multi_user_filesystem_type_0,
                                         AmigaDosTypeToString(amigaDosType));
                }

                if((amigaDosType & TYPEID_OLD_BSD_UNUSED) == TYPEID_OLD_BSD_UNUSED)
                {
                    return string.Format(Localization.Unknown_BSD_filesystem_type_0,
                                         AmigaDosTypeToString(amigaDosType));
                }

                if((amigaDosType & TYPEID_NETBSD_ROOT_UNUSED) == TYPEID_NETBSD_ROOT_UNUSED)
                {
                    return string.Format(Localization.Unknown_NetBSD_root_filesystem_type_0,
                                         AmigaDosTypeToString(amigaDosType));
                }

                if((amigaDosType & TYPEID_NETBSD_USER_UNUSED) == TYPEID_NETBSD_USER_UNUSED)
                {
                    return string.Format(Localization.Unknown_NetBSD_user_filesystem_type_0,
                                         AmigaDosTypeToString(amigaDosType));
                }

                if((amigaDosType & TYPEID_NETBSD_SWAP) == TYPEID_NETBSD_SWAP)
                {
                    return string.Format(Localization.Unknown_NetBSD_swap_filesystem_type_0,
                                         AmigaDosTypeToString(amigaDosType));
                }

                if((amigaDosType & TYPEID_LINUX)      == TYPEID_LINUX ||
                   (amigaDosType & TYPEID_LINUX_SWAP) == TYPEID_LINUX_SWAP)
                {
                    return string.Format(Localization.Unknown_Linux_filesystem_type_0,
                                         AmigaDosTypeToString(amigaDosType));
                }

                return string.Format(Localization.Unknown_partition_type_0, AmigaDosTypeToString(amigaDosType));
            }
        }
    }

    static string AmigaDosTypeToString(uint amigaDosType, bool quoted = true)
    {
        var textPart = new byte[3];

        textPart[0] = (byte)((amigaDosType & 0xFF000000) >> 24);
        textPart[1] = (byte)((amigaDosType & 0x00FF0000) >> 16);
        textPart[2] = (byte)((amigaDosType & 0x0000FF00) >> 8);

        string textPartString = Encoding.ASCII.GetString(textPart);

        return quoted ? $"\"{textPartString}\\{amigaDosType & 0xFF}\"" : $"{textPartString}\\{amigaDosType & 0xFF}";
    }

#region Nested type: BadBlockEntry

    /// <summary>Pair for spare blocks</summary>
    struct BadBlockEntry
    {
        /// <summary>Bad block pointer</summary>
        public uint BadBlock;
        /// <summary>Replacement block pointer</summary>
        public uint GoodBlock;
    }

#endregion

#region Nested type: BadBlockList

    /// <summary>List of bad blocks and spares</summary>
    struct BadBlockList
    {
        /// <summary>"BADB"</summary>
        public uint Magic;
        /// <summary>Size in longs</summary>
        public uint Size;
        /// <summary>Checksum</summary>
        public int Checksum;
        /// <summary>SCSI target ID, 7 for non-SCSI</summary>
        public uint TargetId;
        /// <summary>Pointer for next BadBlockList</summary>
        public uint NextPtr;
        /// <summary>Reserved</summary>
        public uint Reserved;
        /// <summary>Bad block entries, up to block filling, 8 bytes each</summary>
        public BadBlockEntry[] BlockPairs;
    }

#endregion

#region Nested type: DeviceNode

    /// <summary>Device node, mostly useless, except for pointer to first LoadSegment block</summary>
    struct DeviceNode
    {
        /// <summary>Device node type, =0</summary>
        public uint Type;
        /// <summary>DOS task field, =0</summary>
        public uint Task;
        /// <summary>Unused, =0</summary>
        public uint Locked;
        /// <summary>Filename handler to LoadSegment, =0</summary>
        public uint Handler;
        /// <summary>Stack size when starting task, =0</summary>
        public uint StackSize;
        /// <summary>Task priority, =0</summary>
        public uint Priority;
        /// <summary>Startup message, =0</summary>
        public uint Startup;
        /// <summary>Pointer to first LoadSegment block</summary>
        public uint SeglistPtr;
        /// <summary>BCPL globabl vector when starting task, =0xFFFFFFFF</summary>
        public uint GlobalVec;
    }

#endregion

#region Nested type: DosEnvironmentVector

    /// <summary>DOSEnvVec, used by AmigaDOS</summary>
    struct DosEnvironmentVector
    {
        /// <summary>Size in longs, should be 16, minimum 11</summary>
        public uint Size;
        /// <summary>Block size in longs</summary>
        public uint BlockSize;
        /// <summary>Unknown, 0</summary>
        public uint SecOrg;
        /// <summary>Heads in drive</summary>
        public uint Surfaces;
        /// <summary>Sectors per block</summary>
        public uint Spb;
        /// <summary>Blocks per track</summary>
        public uint Bpt;
        /// <summary>DOS reserved blocks at start of partition</summary>
        public uint Reservedblocks;
        /// <summary>DOS reserved blocks at end of partition</summary>
        public uint Prealloc;
        /// <summary>Interleave</summary>
        public uint Interleave;
        /// <summary>First cylinder of a partition, inclusive</summary>
        public uint LowCylinder;
        /// <summary>Last cylinder of a partition, inclusive</summary>
        public uint HighCylinder;
        /// <summary>Buffers, usually 30</summary>
        public uint NumBuffer;
        /// <summary>Type of memory to allocate for buffers</summary>
        public uint BufMemType;
        /// <summary>Maximum transfer, usually 0x7FFFFFFF</summary>
        public uint MaxTransfer;
        /// <summary>Address mask to block out certain memory, usually 0xFFFFFFFE</summary>
        public uint Mask;
        /// <summary>Boot priority</summary>
        public uint BootPriority;
        /// <summary>Partition type, and filesystem driver identification for AmigaDOS</summary>
        public uint DosType;
        /// <summary>Default baud rate for SER and AUX handlers</summary>
        public uint Baud;
        /// <summary>Flow control values for SER and AUX handlers</summary>
        public uint Control;
        /// <summary>Since Kickstart 2, how many boot blocks are to be loaded</summary>
        public uint BootBlocks;
    }

#endregion

#region Nested type: FileSystemHeader

    /// <summary>File system header</summary>
    struct FileSystemHeader
    {
        /// <summary>"FSHD"</summary>
        public uint Magic;
        /// <summary>Size in longs, 64</summary>
        public uint Size;
        /// <summary>Checksum</summary>
        public int Checksum;
        /// <summary>SCSI target ID, 7 for non-SCSI</summary>
        public uint TargetId;
        /// <summary>Pointer to next FileSystemHeader block</summary>
        public uint NextPtr;
        /// <summary>Flags, unknown</summary>
        public uint Flags;
        /// <summary>Reserved</summary>
        public uint Reserved1;
        /// <summary>Reserved</summary>
        public uint Reserved2;
        /// <summary>Partition type, and filesystem driver identification for AmigaDOS</summary>
        public uint DosType;
        /// <summary>Filesystem version Mask 0xFFFF0000, >>16, major version Mask 0x0000FFFF, minor version</summary>
        public uint Version;
        /// <summary>Bits for DeviceNode fields that should be substituted into a standard device node</summary>
        public uint PatchFlags;
        /// <summary>Device node</summary>
        public DeviceNode Dnode;
    }

#endregion

#region Nested type: LoadSegment

    /// <summary>Filesystem code</summary>
    struct LoadSegment
    {
        /// <summary>"LSEG"</summary>
        public uint Magic;
        /// <summary>Size in longs</summary>
        public uint Size;
        /// <summary>Checksum</summary>
        public int Checksum;
        /// <summary>SCSI target ID, 7 for non-SCSI</summary>
        public uint TargetId;
        /// <summary>Pointer to next LoadSegment</summary>
        public uint NextPtr;
        /// <summary>Executable code, with relocation hunks, til end of sector</summary>
        public byte[] LoadData;
    }

#endregion

#region Nested type: PartitionEntry

    struct PartitionEntry
    {
        /// <summary>"PART"</summary>
        public uint Magic;
        /// <summary>Size in longs</summary>
        public uint Size;
        /// <summary>Checksum</summary>
        public int Checksum;
        /// <summary>SCSI target ID, 7 for non-SCSI</summary>
        public uint TargetId;
        /// <summary>Pointer to next PartitionEntry</summary>
        public uint NextPtr;
        /// <summary>Partition flags</summary>
        public uint Flags;
        /// <summary>Reserved</summary>
        public uint Reserved1;
        /// <summary>Reserved</summary>
        public uint Reserved2;
        /// <summary>Preferred flags for OpenDevice()</summary>
        public uint DevFlags;
        /// <summary>Length of drive name</summary>
        public uint DriveNameLen;
        /// <summary>Drive name, 31 bytes</summary>
        public string DriveName;
        /// <summary>Reserved</summary>
        public uint Reserved3;
        /// <summary>Reserved</summary>
        public uint Reserved4;
        /// <summary>Reserved</summary>
        public uint Reserved5;
        /// <summary>Reserved</summary>
        public uint Reserved6;
        /// <summary>Reserved</summary>
        public uint Reserved7;
        /// <summary>Reserved</summary>
        public uint Reserved8;
        /// <summary>Reserved</summary>
        public uint Reserved9;
        /// <summary>Reserved</summary>
        public uint Reserved10;
        /// <summary>Reserved</summary>
        public uint Reserved11;
        /// <summary>Reserved</summary>
        public uint Reserved12;
        /// <summary>Reserved</summary>
        public uint Reserved13;
        /// <summary>Reserved</summary>
        public uint Reserved14;
        /// <summary>Reserved</summary>
        public uint Reserved15;
        /// <summary>Reserved</summary>
        public uint Reserved16;
        /// <summary>Reserved</summary>
        public uint Reserved17;
        /// <summary>DOSEnvVec, more information about partition</summary>
        public DosEnvironmentVector DosEnvVec;
    }

#endregion

#region Nested type: RigidDiskBlock

    /// <summary>Amiga Rigid Disk Block, header for partitioning scheme Can be in any sector from 0 to 15, inclusive</summary>
    struct RigidDiskBlock
    {
        /// <summary>"RDSK"</summary>
        public uint Magic;
        /// <summary>Size in longs</summary>
        public uint Size;
        /// <summary>Checksum</summary>
        public int Checksum;
        /// <summary>SCSI target ID, 7 for non-SCSI</summary>
        public uint TargetId;
        /// <summary>Block size in bytes</summary>
        public uint BlockSize;
        /// <summary>Flags</summary>
        public uint Flags;
        /// <summary>Pointer to first BadBlockList, 0xFFFFFFFF means last block in device</summary>
        public uint BadblockPtr;
        /// <summary>Pointer to first PartitionEntry, 0xFFFFFFFF means last block in device</summary>
        public uint PartitionPtr;
        /// <summary>Pointer to first FileSystemHeader, 0xFFFFFFFF means last block in device</summary>
        public uint FsheaderPtr;
        /// <summary>Optional drive specific init code</summary>
        public uint Driveinitcode;
        /// <summary>Reserved, should be 0xFFFFFFFF</summary>
        public uint Reserved1;
        /// <summary>Reserved, should be 0xFFFFFFFF</summary>
        public uint Reserved2;
        /// <summary>Reserved, should be 0xFFFFFFFF</summary>
        public uint Reserved3;
        /// <summary>Reserved, should be 0xFFFFFFFF</summary>
        public uint Reserved4;
        /// <summary>Reserved, should be 0xFFFFFFFF</summary>
        public uint Reserved5;
        /// <summary>Reserved, should be 0xFFFFFFFF</summary>
        public uint Reserved6;
        /// <summary>Cylinders in drive</summary>
        public uint Cylinders;
        /// <summary>Sectors per track</summary>
        public uint Spt;
        /// <summary>Heads in drive</summary>
        public uint Heads;
        /// <summary>Drive interleave</summary>
        public uint Interleave;
        /// <summary>Cylinder for parking heads</summary>
        public uint Parking;
        /// <summary>Reserved, should be zero</summary>
        public uint Reserved7;
        /// <summary>Reserved, should be zero</summary>
        public uint Reserved8;
        /// <summary>Reserved, should be zero</summary>
        public uint Reserved9;
        /// <summary>Starting cylinder for write precompensation</summary>
        public uint Writeprecomp;
        /// <summary>Starting cylinder for reduced write current</summary>
        public uint Reducedwrite;
        /// <summary>Drive step rate</summary>
        public uint Steprate;
        /// <summary>Reserved, should be zero</summary>
        public uint Reserved10;
        /// <summary>Reserved, should be zero</summary>
        public uint Reserved11;
        /// <summary>Reserved, should be zero</summary>
        public uint Reserved12;
        /// <summary>Reserved, should be zero</summary>
        public uint Reserved13;
        /// <summary>Reserved, should be zero</summary>
        public uint Reserved14;
        /// <summary>Low block of RDB reserved blocks</summary>
        public uint RdbBlockLow;
        /// <summary>High block of RDB reserved blocks</summary>
        public uint RdbBlockHigh;
        /// <summary>Low cylinder for partitionable area</summary>
        public uint LowCylinder;
        /// <summary>High cylinder for partitionable area</summary>
        public uint HighCylinder;
        /// <summary>Blocks per cylinder</summary>
        public uint CylBlocks;
        /// <summary>Seconds for head autoparking</summary>
        public uint AutoParkSeconds;
        /// <summary>Highest block used by RDB</summary>
        public uint HighRdskBlock;
        /// <summary>Reserved, should be zero</summary>
        public uint Reserved15;
        /// <summary>Disk vendor, 8 bytes</summary>
        public string DiskVendor;
        /// <summary>Disk product, 16 bytes</summary>
        public string DiskProduct;
        /// <summary>Disk revision, 4 bytes</summary>
        public string DiskRevision;
        /// <summary>Controller vendor, 8 bytes</summary>
        public string ControllerVendor;
        /// <summary>Controller product, 16 bytes</summary>
        public string ControllerProduct;
        /// <summary>Controller revision, 4 bytes</summary>
        public string ControllerRevision;
        /// <summary>Reserved, should be zero</summary>
        public uint Reserved16;
        /// <summary>Reserved, should be zero</summary>
        public uint Reserved17;
        /// <summary>Reserved, should be zero</summary>
        public uint Reserved18;
        /// <summary>Reserved, should be zero</summary>
        public uint Reserved19;
        /// <summary>Reserved, should be zero</summary>
        public uint Reserved20;
        /// <summary>Reserved, should be zero</summary>
        public uint Reserved21;
        /// <summary>Reserved, should be zero</summary>
        public uint Reserved22;
        /// <summary>Reserved, should be zero</summary>
        public uint Reserved23;
        /// <summary>Reserved, should be zero</summary>
        public uint Reserved24;
        /// <summary>Reserved, should be zero</summary>
        public uint Reserved25;
    }

#endregion
}