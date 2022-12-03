// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of Amiga Fast File System (AFFS)</summary>
public sealed class AmigaDOSPlugin : IFilesystem
{
    const uint FFS_MASK  = 0x444F5300;
    const uint MUFS_MASK = 0x6D754600;

    const uint TYPE_HEADER  = 2;
    const uint SUBTYPE_ROOT = 1;

    const string FS_TYPE_OFS  = "aofs";
    const string FS_TYPE_FFS  = "affs";
    const string FS_TYPE_OFS2 = "aofs2";
    const string FS_TYPE_FFS2 = "affs2";

    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public string Name => Localization.AmigaDOSPlugin_Name;
    /// <inheritdoc />
    public Guid Id => new("3c882400-208c-427d-a086-9119852a1bc7");
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(partition.Start + 4 >= partition.End)
            return false;

        // Boot block is unless defined otherwise, 2 blocks
        // Funny, you may need boot block to find root block if it's not in standard place just to know size of
        // block size and then read the whole boot block.
        // However while you can set a block size different from the sector size on formatting, the bootblock block
        // size for floppies is the sector size, and for RDB is usually is the hard disk sector size,
        // so this is not entirely wrong...
        ErrorNumber errno = imagePlugin.ReadSectors(0 + partition.Start, 2, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        BootBlock bblk = Marshal.ByteArrayToStructureBigEndian<BootBlock>(sector);

        // AROS boot floppies...
        if(sector.Length               >= 512      &&
           sector[510]                 == 0x55     &&
           sector[511]                 == 0xAA     &&
           (bblk.diskType & FFS_MASK)  != FFS_MASK &&
           (bblk.diskType & MUFS_MASK) != MUFS_MASK)
        {
            errno = imagePlugin.ReadSectors(1 + partition.Start, 2, out sector);

            if(errno != ErrorNumber.NoError)
                return false;

            bblk = Marshal.ByteArrayToStructureBigEndian<BootBlock>(sector);
        }

        // Not FFS or MuFS?
        if((bblk.diskType & FFS_MASK)  != FFS_MASK &&
           (bblk.diskType & MUFS_MASK) != MUFS_MASK)
            return false;

        // Clear checksum on sector
        sector[4] = sector[5] = sector[6] = sector[7] = 0;
        uint bsum             = AmigaBootChecksum(sector);

        AaruConsole.DebugWriteLine("AmigaDOS plugin", "bblk.checksum = 0x{0:X8}", bblk.checksum);
        AaruConsole.DebugWriteLine("AmigaDOS plugin", "bsum = 0x{0:X8}", bsum);

        ulong bRootPtr = 0;

        // If bootblock is correct, let's take its rootblock pointer
        if(bsum == bblk.checksum)
        {
            bRootPtr = bblk.root_ptr + partition.Start;
            AaruConsole.DebugWriteLine("AmigaDOS plugin", Localization.Bootblock_points_to_0_as_Rootblock, bRootPtr);
        }

        ulong[] rootPtrs =
        {
            bRootPtr + partition.Start, ((partition.End - partition.Start + 1) / 2) + partition.Start - 2,
            ((partition.End                             - partition.Start + 1) / 2) + partition.Start - 1,
            ((partition.End - partition.Start + 1) / 2) + partition.Start,
            ((partition.End - partition.Start + 1) / 2) + partition.Start + 4
        };

        var rblk = new RootBlock();

        // So to handle even number of sectors
        foreach(ulong rootPtr in rootPtrs.Where(rootPtr => rootPtr < partition.End && rootPtr >= partition.Start))
        {
            AaruConsole.DebugWriteLine("AmigaDOS plugin", Localization.Searching_for_Rootblock_in_sector_0, rootPtr);

            errno = imagePlugin.ReadSector(rootPtr, out sector);

            if(errno != ErrorNumber.NoError)
                continue;

            rblk.type = BigEndianBitConverter.ToUInt32(sector, 0x00);
            AaruConsole.DebugWriteLine("AmigaDOS plugin", "rblk.type = {0}", rblk.type);

            if(rblk.type != TYPE_HEADER)
                continue;

            rblk.hashTableSize = BigEndianBitConverter.ToUInt32(sector, 0x0C);

            AaruConsole.DebugWriteLine("AmigaDOS plugin", "rblk.hashTableSize = {0}", rblk.hashTableSize);

            uint blockSize       = (rblk.hashTableSize + 56) * 4;
            uint sectorsPerBlock = (uint)(blockSize / sector.Length);

            AaruConsole.DebugWriteLine("AmigaDOS plugin", "blockSize = {0}", blockSize);
            AaruConsole.DebugWriteLine("AmigaDOS plugin", "sectorsPerBlock = {0}", sectorsPerBlock);

            if(blockSize % sector.Length > 0)
                sectorsPerBlock++;

            if(rootPtr + sectorsPerBlock >= partition.End)
                continue;

            errno = imagePlugin.ReadSectors(rootPtr, sectorsPerBlock, out sector);

            if(errno != ErrorNumber.NoError)
                continue;

            // Clear checksum on sector
            rblk.checksum = BigEndianBitConverter.ToUInt32(sector, 20);
            sector[20]    = sector[21] = sector[22] = sector[23] = 0;
            uint rsum                  = AmigaChecksum(sector);

            AaruConsole.DebugWriteLine("AmigaDOS plugin", "rblk.checksum = 0x{0:X8}", rblk.checksum);
            AaruConsole.DebugWriteLine("AmigaDOS plugin", "rsum = 0x{0:X8}", rsum);

            rblk.sec_type = BigEndianBitConverter.ToUInt32(sector, sector.Length - 4);
            AaruConsole.DebugWriteLine("AmigaDOS plugin", "rblk.sec_type = {0}", rblk.sec_type);

            if(rblk.sec_type == SUBTYPE_ROOT &&
               rblk.checksum == rsum)
                return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding = encoding ?? Encoding.GetEncoding("iso-8859-1");
        var sbInformation = new StringBuilder();
        XmlFsType   = new FileSystemType();
        information = null;
        ErrorNumber errno = imagePlugin.ReadSectors(0 + partition.Start, 2, out byte[] bootBlockSectors);

        if(errno != ErrorNumber.NoError)
            return;

        BootBlock bootBlk = Marshal.ByteArrayToStructureBigEndian<BootBlock>(bootBlockSectors);
        bootBlk.bootCode = new byte[bootBlockSectors.Length - 12];
        Array.Copy(bootBlockSectors, 12, bootBlk.bootCode, 0, bootBlk.bootCode.Length);
        bootBlockSectors[4] = bootBlockSectors[5] = bootBlockSectors[6] = bootBlockSectors[7] = 0;
        uint bsum                                 = AmigaBootChecksum(bootBlockSectors);

        ulong bRootPtr = 0;

        // If bootblock is correct, let's take its rootblock pointer
        if(bsum == bootBlk.checksum)
        {
            bRootPtr = bootBlk.root_ptr + partition.Start;
            AaruConsole.DebugWriteLine("AmigaDOS plugin", Localization.Bootblock_points_to_0_as_Rootblock, bRootPtr);
        }

        ulong[] rootPtrs =
        {
            bRootPtr + partition.Start, ((partition.End - partition.Start + 1) / 2) + partition.Start - 2,
            ((partition.End                             - partition.Start + 1) / 2) + partition.Start - 1,
            ((partition.End - partition.Start + 1) / 2) + partition.Start,
            ((partition.End - partition.Start + 1) / 2) + partition.Start + 4
        };

        var    rootBlk         = new RootBlock();
        byte[] rootBlockSector = null;

        bool rootFound = false;
        uint blockSize = 0;

        // So to handle even number of sectors
        foreach(ulong rootPtr in rootPtrs.Where(rootPtr => rootPtr < partition.End && rootPtr >= partition.Start))
        {
            AaruConsole.DebugWriteLine("AmigaDOS plugin", Localization.Searching_for_Rootblock_in_sector_0, rootPtr);

            errno = imagePlugin.ReadSector(rootPtr, out rootBlockSector);

            if(errno != ErrorNumber.NoError)
                continue;

            rootBlk.type = BigEndianBitConverter.ToUInt32(rootBlockSector, 0x00);
            AaruConsole.DebugWriteLine("AmigaDOS plugin", "rootBlk.type = {0}", rootBlk.type);

            if(rootBlk.type != TYPE_HEADER)
                continue;

            rootBlk.hashTableSize = BigEndianBitConverter.ToUInt32(rootBlockSector, 0x0C);

            AaruConsole.DebugWriteLine("AmigaDOS plugin", "rootBlk.hashTableSize = {0}", rootBlk.hashTableSize);

            blockSize = (rootBlk.hashTableSize + 56) * 4;
            uint sectorsPerBlock = (uint)(blockSize / rootBlockSector.Length);

            AaruConsole.DebugWriteLine("AmigaDOS plugin", "blockSize = {0}", blockSize);
            AaruConsole.DebugWriteLine("AmigaDOS plugin", "sectorsPerBlock = {0}", sectorsPerBlock);

            if(blockSize % rootBlockSector.Length > 0)
                sectorsPerBlock++;

            if(rootPtr + sectorsPerBlock >= partition.End)
                continue;

            errno = imagePlugin.ReadSectors(rootPtr, sectorsPerBlock, out rootBlockSector);

            if(errno != ErrorNumber.NoError)
                continue;

            // Clear checksum on sector
            rootBlk.checksum    = BigEndianBitConverter.ToUInt32(rootBlockSector, 20);
            rootBlockSector[20] = rootBlockSector[21] = rootBlockSector[22] = rootBlockSector[23] = 0;
            uint rsum                                 = AmigaChecksum(rootBlockSector);

            AaruConsole.DebugWriteLine("AmigaDOS plugin", "rootBlk.checksum = 0x{0:X8}", rootBlk.checksum);
            AaruConsole.DebugWriteLine("AmigaDOS plugin", "rsum = 0x{0:X8}", rsum);

            rootBlk.sec_type = BigEndianBitConverter.ToUInt32(rootBlockSector, rootBlockSector.Length - 4);
            AaruConsole.DebugWriteLine("AmigaDOS plugin", "rootBlk.sec_type = {0}", rootBlk.sec_type);

            if(rootBlk.sec_type != SUBTYPE_ROOT ||
               rootBlk.checksum != rsum)
                continue;

            errno = imagePlugin.ReadSectors(rootPtr, sectorsPerBlock, out rootBlockSector);

            if(errno != ErrorNumber.NoError)
                continue;

            rootFound = true;

            break;
        }

        if(!rootFound)
            return;

        rootBlk = MarshalRootBlock(rootBlockSector);

        string diskName = StringHandlers.PascalToString(rootBlk.diskName, Encoding);

        switch(bootBlk.diskType & 0xFF)
        {
            case 0:
                sbInformation.Append(Localization.Amiga_Original_File_System);
                XmlFsType.Type = FS_TYPE_OFS;

                break;
            case 1:
                sbInformation.Append(Localization.Amiga_Fast_File_System);
                XmlFsType.Type = FS_TYPE_FFS;

                break;
            case 2:
                sbInformation.Append(Localization.Amiga_Original_File_System_with_international_characters);
                XmlFsType.Type = FS_TYPE_OFS;

                break;
            case 3:
                sbInformation.Append(Localization.Amiga_Fast_File_System_with_international_characters);
                XmlFsType.Type = FS_TYPE_FFS;

                break;
            case 4:
                sbInformation.Append(Localization.Amiga_Original_File_System_with_directory_cache);
                XmlFsType.Type = FS_TYPE_OFS;

                break;
            case 5:
                sbInformation.Append(Localization.Amiga_Fast_File_System_with_directory_cache);
                XmlFsType.Type = FS_TYPE_FFS;

                break;
            case 6:
                sbInformation.Append(Localization.Amiga_Original_File_System_with_long_filenames);
                XmlFsType.Type = FS_TYPE_OFS2;

                break;
            case 7:
                sbInformation.Append(Localization.Amiga_Fast_File_System_with_long_filenames);
                XmlFsType.Type = FS_TYPE_FFS2;

                break;
        }

        if((bootBlk.diskType & 0x6D754600) == 0x6D754600)
            sbInformation.Append(Localization.with_multi_user_patches);

        sbInformation.AppendLine();

        sbInformation.AppendFormat(Localization.Volume_name_0, diskName).AppendLine();

        if(bootBlk.checksum == bsum)
        {
            var sha1Ctx = new Sha1Context();
            sha1Ctx.Update(bootBlk.bootCode);
            sbInformation.AppendLine(Localization.Volume_is_bootable);
            sbInformation.AppendFormat(Localization.Boot_code_SHA1_0, sha1Ctx.End()).AppendLine();
        }

        if(rootBlk.bitmapFlag == 0xFFFFFFFF)
            sbInformation.AppendLine(Localization.Volume_bitmap_is_valid);

        if(rootBlk.bitmapExtensionBlock != 0x00000000 &&
           rootBlk.bitmapExtensionBlock != 0xFFFFFFFF)
            sbInformation.AppendFormat(Localization.Bitmap_extension_at_block_0, rootBlk.bitmapExtensionBlock).
                          AppendLine();

        if((bootBlk.diskType & 0xFF) == 4 ||
           (bootBlk.diskType & 0xFF) == 5)
            sbInformation.AppendFormat(Localization.Directory_cache_starts_at_block_0, rootBlk.extension).AppendLine();

        ulong blocks = (partition.End - partition.Start + 1) * imagePlugin.Info.SectorSize / blockSize;

        sbInformation.AppendFormat(Localization.Volume_block_size_is_0_bytes, blockSize).AppendLine();
        sbInformation.AppendFormat(Localization.Volume_has_0_blocks, blocks).AppendLine();

        sbInformation.AppendFormat(Localization.Volume_created_on_0,
                                   DateHandlers.AmigaToDateTime(rootBlk.cDays, rootBlk.cMins, rootBlk.cTicks)).
                      AppendLine();

        sbInformation.AppendFormat(Localization.Volume_last_modified_on_0,
                                   DateHandlers.AmigaToDateTime(rootBlk.vDays, rootBlk.vMins, rootBlk.vTicks)).
                      AppendLine();

        sbInformation.AppendFormat(Localization.Volume_root_directory_last_modified_on_0,
                                   DateHandlers.AmigaToDateTime(rootBlk.rDays, rootBlk.rMins, rootBlk.rTicks)).
                      AppendLine();

        sbInformation.AppendFormat(Localization.Root_block_checksum_is_0, rootBlk.checksum).AppendLine();
        information = sbInformation.ToString();

        XmlFsType.CreationDate = DateHandlers.AmigaToDateTime(rootBlk.cDays, rootBlk.cMins, rootBlk.cTicks);

        XmlFsType.CreationDateSpecified = true;

        XmlFsType.ModificationDate = DateHandlers.AmigaToDateTime(rootBlk.vDays, rootBlk.vMins, rootBlk.vTicks);

        XmlFsType.ModificationDateSpecified = true;
        XmlFsType.Dirty                     = rootBlk.bitmapFlag != 0xFFFFFFFF;
        XmlFsType.Clusters                  = blocks;
        XmlFsType.ClusterSize               = blockSize;
        XmlFsType.VolumeName                = diskName;
        XmlFsType.Bootable                  = bsum == bootBlk.checksum;

        // Useful as a serial
        XmlFsType.VolumeSerial = $"{rootBlk.checksum:X8}";
    }

    static RootBlock MarshalRootBlock(byte[] block)
    {
        byte[] tmp = new byte[228];
        Array.Copy(block, 0, tmp, 0, 24);
        Array.Copy(block, block.Length - 200, tmp, 28, 200);
        RootBlock root = Marshal.ByteArrayToStructureBigEndian<RootBlock>(tmp);
        root.hashTable = new uint[(block.Length - 224) / 4];

        for(int i = 0; i < root.hashTable.Length; i++)
            root.hashTable[i] = BigEndianBitConverter.ToUInt32(block, 24 + (i * 4));

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
        uint sum = 0;

        for(int i = 0; i < data.Length; i += 4)
        {
            uint psum = sum;

            if((sum += (uint)((data[i] << 24) + (data[i + 1] << 16) + (data[i + 2] << 8) + data[i + 3])) < psum)
                sum++;
        }

        return ~sum;
    }

    /// <summary>Boot block, first 2 sectors</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BootBlock
    {
        /// <summary>Offset 0x00, "DOSx" disk type</summary>
        public readonly uint diskType;
        /// <summary>Offset 0x04, Checksum</summary>
        public readonly uint checksum;
        /// <summary>Offset 0x08, Pointer to root block, mostly invalid</summary>
        public readonly uint root_ptr;
        /// <summary>Offset 0x0C, Boot code, til completion. Size is intentionally incorrect to allow marshaling to work.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public byte[] bootCode;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct RootBlock
    {
        /// <summary>Offset 0x00, block type, value = T_HEADER (2)</summary>
        public uint type;
        /// <summary>Offset 0x04, unused</summary>
        public readonly uint headerKey;
        /// <summary>Offset 0x08, unused</summary>
        public readonly uint highSeq;
        /// <summary>Offset 0x0C, longs used by hash table</summary>
        public uint hashTableSize;
        /// <summary>Offset 0x10, unused</summary>
        public readonly uint firstData;
        /// <summary>Offset 0x14, Rootblock checksum</summary>
        public uint checksum;
        /// <summary>
        ///     Offset 0x18, Hashtable, size = (block size / 4) - 56 or size = hashTableSize. Size intentionally bad to allow
        ///     marshalling to work.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public uint[] hashTable;
        /// <summary>Offset 0x18+hashTableSize*4+0, bitmap flag, 0xFFFFFFFF if valid</summary>
        public readonly uint bitmapFlag;
        /// <summary>Offset 0x18+hashTableSize*4+4, bitmap pages, 25 entries</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 25)]
        public readonly uint[] bitmapPages;
        /// <summary>Offset 0x18+hashTableSize*4+104, pointer to bitmap extension block</summary>
        public readonly uint bitmapExtensionBlock;
        /// <summary>Offset 0x18+hashTableSize*4+108, last root alteration days since 1978/01/01</summary>
        public readonly uint rDays;
        /// <summary>Offset 0x18+hashTableSize*4+112, last root alteration minutes past midnight</summary>
        public readonly uint rMins;
        /// <summary>Offset 0x18+hashTableSize*4+116, last root alteration ticks (1/50 secs)</summary>
        public readonly uint rTicks;
        /// <summary>Offset 0x18+hashTableSize*4+120, disk name, pascal string, 31 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 31)]
        public readonly byte[] diskName;
        /// <summary>Offset 0x18+hashTableSize*4+151, unused</summary>
        public readonly byte padding;
        /// <summary>Offset 0x18+hashTableSize*4+152, unused</summary>
        public readonly uint reserved1;
        /// <summary>Offset 0x18+hashTableSize*4+156, unused</summary>
        public readonly uint reserved2;
        /// <summary>Offset 0x18+hashTableSize*4+160, last disk alteration days since 1978/01/01</summary>
        public readonly uint vDays;
        /// <summary>Offset 0x18+hashTableSize*4+164, last disk alteration minutes past midnight</summary>
        public readonly uint vMins;
        /// <summary>Offset 0x18+hashTableSize*4+168, last disk alteration ticks (1/50 secs)</summary>
        public readonly uint vTicks;
        /// <summary>Offset 0x18+hashTableSize*4+172, filesystem creation days since 1978/01/01</summary>
        public readonly uint cDays;
        /// <summary>Offset 0x18+hashTableSize*4+176, filesystem creation minutes since 1978/01/01</summary>
        public readonly uint cMins;
        /// <summary>Offset 0x18+hashTableSize*4+180, filesystem creation ticks since 1978/01/01</summary>
        public readonly uint cTicks;
        /// <summary>Offset 0x18+hashTableSize*4+184, unused</summary>
        public readonly uint nextHash;
        /// <summary>Offset 0x18+hashTableSize*4+188, unused</summary>
        public readonly uint parentDir;
        /// <summary>Offset 0x18+hashTableSize*4+192, first directory cache block</summary>
        public readonly uint extension;
        /// <summary>Offset 0x18+hashTableSize*4+196, block secondary type = ST_ROOT (1)</summary>
        public uint sec_type;
    }
}