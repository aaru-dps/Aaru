// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft FAT filesystem plugin.
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.Checksums;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

public sealed partial class FAT
{
#region IReadOnlyFilesystem Members

    /// <inheritdoc />
    [SuppressMessage("ReSharper", "JoinDeclarationAndInitializer")]
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(2 + partition.Start >= partition.End)
            return false;

        ushort bps;
        byte   spc;
        byte   numberOfFats;
        ushort reservedSecs;
        ushort rootEntries;
        ushort sectors;
        byte   mediaDescriptor;
        ushort fatSectors;
        uint   bigSectors;
        byte   bpbSignature;
        byte   fat32Signature;
        ulong  hugeSectors;
        var    fat32Id = new byte[8];
        var    msxId   = new byte[6];
        byte   fatId;
        var    dosOem   = new byte[8];
        var    atariOem = new byte[6];
        ushort bootable = 0;

        uint sectorsPerBpb = imagePlugin.Info.SectorSize < 512 ? 512 / imagePlugin.Info.SectorSize : 1;

        ErrorNumber errno = imagePlugin.ReadSectors(0 + partition.Start, sectorsPerBpb, out byte[] bpbSector);

        if(errno != ErrorNumber.NoError)
            return false;

        errno = imagePlugin.ReadSector(sectorsPerBpb + partition.Start, out byte[] fatSector);

        if(errno != ErrorNumber.NoError)
            return false;

        HumanParameterBlock humanBpb = Marshal.ByteArrayToStructureBigEndian<HumanParameterBlock>(bpbSector);

        ulong expectedClusters = humanBpb.bpc > 0 ? partition.Size / humanBpb.bpc : 0;

        AaruConsole.DebugWriteLine(MODULE_NAME, "Human bpc = {0}",               humanBpb.bpc);
        AaruConsole.DebugWriteLine(MODULE_NAME, "Human clusters = {0}",          humanBpb.clusters);
        AaruConsole.DebugWriteLine(MODULE_NAME, "Human big_clusters = {0}",      humanBpb.big_clusters);
        AaruConsole.DebugWriteLine(MODULE_NAME, "Human expected clusters = {0}", expectedClusters);

        // Check clusters for Human68k are correct
        bool humanClustersCorrect = humanBpb.clusters           == 0
                                        ? humanBpb.big_clusters == expectedClusters
                                        : humanBpb.clusters     == expectedClusters;

        // Check OEM for Human68k is correct
        bool humanOemCorrect = bpbSector[2]  >= 0x20 &&
                               bpbSector[3]  >= 0x20 &&
                               bpbSector[4]  >= 0x20 &&
                               bpbSector[5]  >= 0x20 &&
                               bpbSector[6]  >= 0x20 &&
                               bpbSector[7]  >= 0x20 &&
                               bpbSector[8]  >= 0x20 &&
                               bpbSector[9]  >= 0x20 &&
                               bpbSector[10] >= 0x20 &&
                               bpbSector[11] >= 0x20 &&
                               bpbSector[12] >= 0x20 &&
                               bpbSector[13] >= 0x20 &&
                               bpbSector[14] >= 0x20 &&
                               bpbSector[15] >= 0x20 &&
                               bpbSector[16] >= 0x20 &&
                               bpbSector[17] >= 0x20;

        // Check correct branch for Human68k
        bool humanBranchCorrect = bpbSector[0] == 0x60 && bpbSector[1] >= 0x20 && bpbSector[1] < 0xFE;

        AaruConsole.DebugWriteLine(MODULE_NAME, "humanClustersCorrect = {0}", humanClustersCorrect);
        AaruConsole.DebugWriteLine(MODULE_NAME, "humanOemCorrect = {0}",      humanOemCorrect);
        AaruConsole.DebugWriteLine(MODULE_NAME, "humanBranchCorrect = {0}",   humanBranchCorrect);

        // If all Human68k checks are correct, it is a Human68k FAT16
        if(humanClustersCorrect && humanOemCorrect && humanBranchCorrect && expectedClusters > 0)
            return true;

        Array.Copy(bpbSector, 0x02, atariOem, 0, 6);
        Array.Copy(bpbSector, 0x03, dosOem,   0, 8);
        bps             = BitConverter.ToUInt16(bpbSector, 0x00B);
        spc             = bpbSector[0x00D];
        reservedSecs    = BitConverter.ToUInt16(bpbSector, 0x00E);
        numberOfFats    = bpbSector[0x010];
        rootEntries     = BitConverter.ToUInt16(bpbSector, 0x011);
        sectors         = BitConverter.ToUInt16(bpbSector, 0x013);
        mediaDescriptor = bpbSector[0x015];
        fatSectors      = BitConverter.ToUInt16(bpbSector, 0x016);
        Array.Copy(bpbSector, 0x052, msxId, 0, 6);
        bigSectors     = BitConverter.ToUInt32(bpbSector, 0x020);
        bpbSignature   = bpbSector[0x026];
        fat32Signature = bpbSector[0x042];
        Array.Copy(bpbSector, 0x052, fat32Id, 0, 8);
        hugeSectors = BitConverter.ToUInt64(bpbSector, 0x052);
        fatId       = fatSector[0];
        int bitsInBps = CountBits.Count(bps);

        if(imagePlugin.Info.SectorSize >= 512)
            bootable = BitConverter.ToUInt16(bpbSector, 0x1FE);

        bool   correctSpc  = spc is 1 or 2 or 4 or 8 or 16 or 32 or 64;
        string msxString   = Encoding.ASCII.GetString(msxId);
        string fat32String = Encoding.ASCII.GetString(fat32Id);

        bool atariOemCorrect = atariOem[0] >= 0x20 &&
                               atariOem[1] >= 0x20 &&
                               atariOem[2] >= 0x20 &&
                               atariOem[3] >= 0x20 &&
                               atariOem[4] >= 0x20 &&
                               atariOem[5] >= 0x20;

        bool dosOemCorrect = dosOem[0] >= 0x20 &&
                             dosOem[1] >= 0x20 &&
                             dosOem[2] >= 0x20 &&
                             dosOem[3] >= 0x20 &&
                             dosOem[4] >= 0x20 &&
                             dosOem[5] >= 0x20 &&
                             dosOem[6] >= 0x20 &&
                             dosOem[7] >= 0x20;

        string oemString = Encoding.ASCII.GetString(dosOem);

        AaruConsole.DebugWriteLine(MODULE_NAME, "atari_oem_correct = {0}",     atariOemCorrect);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dos_oem_correct = {0}",       dosOemCorrect);
        AaruConsole.DebugWriteLine(MODULE_NAME, "bps = {0}",                   bps);
        AaruConsole.DebugWriteLine(MODULE_NAME, "bits in bps = {0}",           bitsInBps);
        AaruConsole.DebugWriteLine(MODULE_NAME, "spc = {0}",                   spc);
        AaruConsole.DebugWriteLine(MODULE_NAME, "correct_spc = {0}",           correctSpc);
        AaruConsole.DebugWriteLine(MODULE_NAME, "reserved_secs = {0}",         reservedSecs);
        AaruConsole.DebugWriteLine(MODULE_NAME, "fats_no = {0}",               numberOfFats);
        AaruConsole.DebugWriteLine(MODULE_NAME, "root_entries = {0}",          rootEntries);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sectors = {0}",               sectors);
        AaruConsole.DebugWriteLine(MODULE_NAME, "media_descriptor = 0x{0:X2}", mediaDescriptor);
        AaruConsole.DebugWriteLine(MODULE_NAME, "fat_sectors = {0}",           fatSectors);
        AaruConsole.DebugWriteLine(MODULE_NAME, "msx_id = \"{0}\"",            msxString);
        AaruConsole.DebugWriteLine(MODULE_NAME, "big_sectors = {0}",           bigSectors);
        AaruConsole.DebugWriteLine(MODULE_NAME, "bpb_signature = 0x{0:X2}",    bpbSignature);
        AaruConsole.DebugWriteLine(MODULE_NAME, "fat32_signature = 0x{0:X2}",  fat32Signature);
        AaruConsole.DebugWriteLine(MODULE_NAME, "fat32_id = \"{0}\"",          fat32String);
        AaruConsole.DebugWriteLine(MODULE_NAME, "huge_sectors = {0}",          hugeSectors);
        AaruConsole.DebugWriteLine(MODULE_NAME, "fat_id = 0x{0:X2}",           fatId);

        var  apricotBps             = BitConverter.ToUInt16(bpbSector, 0x50);
        byte apricotSpc             = bpbSector[0x52];
        var  apricotReservedSecs    = BitConverter.ToUInt16(bpbSector, 0x53);
        byte apricotFatsNo          = bpbSector[0x55];
        var  apricotRootEntries     = BitConverter.ToUInt16(bpbSector, 0x56);
        var  apricotSectors         = BitConverter.ToUInt16(bpbSector, 0x58);
        byte apricotMediaDescriptor = bpbSector[0x5A];
        var  apricotFatSectors      = BitConverter.ToUInt16(bpbSector, 0x5B);

        bool apricotCorrectSpc = apricotSpc is 1 or 2 or 4 or 8 or 16 or 32 or 64;

        int  bitsInApricotBps  = CountBits.Count(apricotBps);
        byte apricotPartitions = bpbSector[0x0C];

        AaruConsole.DebugWriteLine(MODULE_NAME, "apricot_bps = {0}",                   apricotBps);
        AaruConsole.DebugWriteLine(MODULE_NAME, "apricot_spc = {0}",                   apricotSpc);
        AaruConsole.DebugWriteLine(MODULE_NAME, "apricot_correct_spc = {0}",           apricotCorrectSpc);
        AaruConsole.DebugWriteLine(MODULE_NAME, "apricot_reserved_secs = {0}",         apricotReservedSecs);
        AaruConsole.DebugWriteLine(MODULE_NAME, "apricot_fats_no = {0}",               apricotFatsNo);
        AaruConsole.DebugWriteLine(MODULE_NAME, "apricot_root_entries = {0}",          apricotRootEntries);
        AaruConsole.DebugWriteLine(MODULE_NAME, "apricot_sectors = {0}",               apricotSectors);
        AaruConsole.DebugWriteLine(MODULE_NAME, "apricot_media_descriptor = 0x{0:X2}", apricotMediaDescriptor);
        AaruConsole.DebugWriteLine(MODULE_NAME, "apricot_fat_sectors = {0}",           apricotFatSectors);

        // This is to support FAT partitions on hybrid ISO/USB images
        if(imagePlugin.Info.MetadataMediaType == MetadataMediaType.OpticalDisc)
        {
            sectors     /= 4;
            bigSectors  /= 4;
            hugeSectors /= 4;
        }

        switch(oemString)
        {
            // exFAT
            case "EXFAT   ":
                return false;

            // NTFS
            case "NTFS    " when bootable == 0xAA55 && numberOfFats == 0 && fatSectors == 0:
                return false;

            // QNX4
            case "FQNX4FS ":
                return false;
        }

        // HPFS
        if(16 + partition.Start <= partition.End)
        {
            errno = imagePlugin.ReadSector(16 + partition.Start,
                                           out byte[] hpfsSbSector); // Seek to superblock, on logical sector 16

            if(errno != ErrorNumber.NoError)
                return false;

            var hpfsMagic1 = BitConverter.ToUInt32(hpfsSbSector, 0x000);
            var hpfsMagic2 = BitConverter.ToUInt32(hpfsSbSector, 0x004);

            if(hpfsMagic1 == 0xF995E849 && hpfsMagic2 == 0xFA53E9C5)
                return false;
        }

        switch(bitsInBps)
        {
            // FAT32 for sure
            case 1 when correctSpc             &&
                        numberOfFats   <= 2    &&
                        fatSectors     == 0    &&
                        fat32Signature == 0x29 &&
                        fat32String    == "FAT32   ":
                return true;

            // short FAT32
            case 1 when correctSpc && numberOfFats <= 2 && fatSectors == 0 && fat32Signature == 0x28:
                return sectors == 0
                           ? bigSectors        == 0
                                 ? hugeSectors <= partition.End - partition.Start + 1
                                 : bigSectors  <= partition.End - partition.Start + 1
                           : sectors <= partition.End - partition.Start + 1;

            // MSX-DOS FAT12
            case 1 when correctSpc                                          &&
                        numberOfFats <= 2                                   &&
                        rootEntries  > 0                                    &&
                        sectors      <= partition.End - partition.Start + 1 &&
                        fatSectors   > 0                                    &&
                        msxString    == "VOL_ID":
                return true;

            // EBPB
            case 1 when correctSpc        &&
                        numberOfFats <= 2 &&
                        rootEntries  > 0  &&
                        fatSectors   > 0  &&
                        bpbSignature is 0x28 or 0x29:
                return sectors          == 0
                           ? bigSectors <= partition.End - partition.Start + 1
                           : sectors    <= partition.End - partition.Start + 1;

            // BPB
            case 1 when correctSpc                                     &&
                        reservedSecs < partition.End - partition.Start &&
                        numberOfFats <= 2                              &&
                        rootEntries  > 0                               &&
                        fatSectors   > 0:
                return sectors          == 0
                           ? bigSectors <= partition.End - partition.Start + 1
                           : sectors    <= partition.End - partition.Start + 1;
        }

        // Apricot BPB
        if(bitsInApricotBps == 1                                      &&
           apricotCorrectSpc                                          &&
           apricotReservedSecs < partition.End - partition.Start      &&
           apricotFatsNo       <= 2                                   &&
           apricotRootEntries  > 0                                    &&
           apricotFatSectors   > 0                                    &&
           apricotSectors      <= partition.End - partition.Start + 1 &&
           apricotPartitions   == 0)
            return true;

        // All FAT12 without BPB can only be used on floppies, without partitions.
        if(partition.Start != 0)
            return false;

        // DEC Rainbow, lacks a BPB but has a very concrete structure...
        if(imagePlugin.Info is { Sectors: 800, SectorSize: 512 })
        {
            // DEC Rainbow boots up with a Z80, first byte should be DI (disable interrupts)
            byte z80Di = bpbSector[0];

            // First FAT1 sector resides at LBA 0x14
            errno = imagePlugin.ReadSector(0x14, out byte[] fat1Sector0);

            if(errno != ErrorNumber.NoError)
                return false;

            // First FAT2 sector resides at LBA 0x1A
            errno = imagePlugin.ReadSector(0x1A, out byte[] fat2Sector0);

            if(errno != ErrorNumber.NoError)
                return false;

            bool equalFatIds = fat1Sector0[0] == fat2Sector0[0] && fat1Sector0[1] == fat2Sector0[1];

            // Volume is software interleaved 2:1
            var rootMs = new MemoryStream();

            foreach(ulong rootSector in new ulong[]
                {
                    0x17, 0x19, 0x1B, 0x1D, 0x1E, 0x20
                })
            {
                errno = imagePlugin.ReadSector(rootSector, out byte[] tmp);

                if(errno != ErrorNumber.NoError)
                    return false;

                rootMs.Write(tmp, 0, tmp.Length);
            }

            byte[] rootDir      = rootMs.ToArray();
            var    validRootDir = true;

            // Iterate all root directory
            for(var e = 0; e < 96 * 32; e += 32)
            {
                for(var c = 0; c < 11; c++)
                {
                    if((rootDir[c + e] >= 0x20 || rootDir[c + e] == 0x00 || rootDir[c + e] == 0x05) &&
                       rootDir[c + e] != 0xFF                                                       &&
                       rootDir[c + e] != 0x2E)
                        continue;

                    validRootDir = false;

                    break;
                }

                if(!validRootDir)
                    break;
            }

            if(z80Di == 0xF3                   &&
               equalFatIds                     &&
               (fat1Sector0[0] & 0xF0) == 0xF0 &&
               fat1Sector0[1]          == 0xFF &&
               validRootDir)
                return true;
        }

        byte fat2        = fatSector[1];
        byte fat3        = fatSector[2];
        var  fatCluster2 = (ushort)((fat2 << 8) + fat3 & 0xFFF);

        AaruConsole.DebugWriteLine(MODULE_NAME, "1st fat cluster 1 = {0:X3}", fatCluster2);

        if(fatCluster2 < 0xFF0)
            return false;

        ulong fat2SectorNo = 0;

        switch(fatId)
        {
            case 0xE5:
                if(imagePlugin.Info is { Sectors: 2002, SectorSize: 128 })
                    fat2SectorNo = 2;

                break;
            case 0xFD:
                switch(imagePlugin.Info.Sectors)
                {
                    case 4004 when imagePlugin.Info.SectorSize == 128:
                    case 2002 when imagePlugin.Info.SectorSize == 128:
                        fat2SectorNo = 7;

                        break;
                }

                break;
            case 0xFE:
                fat2SectorNo = imagePlugin.Info.Sectors switch
                               {
                                   320 when imagePlugin.Info.SectorSize  == 512  => 2,
                                   2002 when imagePlugin.Info.SectorSize == 128  => 7,
                                   1232 when imagePlugin.Info.SectorSize == 1024 => 3,
                                   616 when imagePlugin.Info.SectorSize  == 1024 => 2,
                                   720 when imagePlugin.Info.SectorSize  == 128  => 5,
                                   640 when imagePlugin.Info.SectorSize  == 512  => 2,
                                   _                                             => fat2SectorNo
                               };

                break;
            case 0xFF:
                if(imagePlugin.Info is { Sectors: 640, SectorSize: 512 })
                    fat2SectorNo = 2;

                break;
            default:
                if(fatId < 0xE8)
                    return false;

                fat2SectorNo = 2;

                break;
        }

        if(fat2SectorNo > partition.End || fat2SectorNo == 0)
            return false;

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Second_fat_starts_at_0, fat2SectorNo);

        errno = imagePlugin.ReadSector(fat2SectorNo, out byte[] fat2Sector);

        if(errno != ErrorNumber.NoError)
            return false;

        fat2        = fat2Sector[1];
        fat3        = fat2Sector[2];
        fatCluster2 = (ushort)((fat2 << 8) + fat3 & 0xFFF);

        if(fatCluster2 < 0xFF0)
            return false;

        return fatId == fat2Sector[0];
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        encoding    ??= Encoding.GetEncoding("IBM437");
        information =   "";

        var sb = new StringBuilder();
        metadata = new FileSystem();

        uint sectorsPerBpb = imagePlugin.Info.SectorSize < 512 ? 512 / imagePlugin.Info.SectorSize : 1;

        ErrorNumber errno = imagePlugin.ReadSectors(0 + partition.Start, sectorsPerBpb, out byte[] bpbSector);

        if(errno != ErrorNumber.NoError)
            return;

        BpbKind bpbKind = DetectBpbKind(bpbSector, imagePlugin, partition, out BiosParameterBlockEbpb fakeBpb,
                                        out HumanParameterBlock humanBpb, out AtariParameterBlock atariBpb,
                                        out byte minBootNearJump, out bool andosOemCorrect, out bool bootable);

        var    isFat12             = false;
        var    isFat16             = false;
        var    isFat32             = false;
        ulong  rootDirectorySector = 0;
        string extraInfo           = null;
        string bootChk             = null;
        metadata.Bootable = bootable;

        // This is needed because for FAT16, GEMDOS increases bytes per sector count instead of using big_sectors field.
        uint sectorsPerRealSector;

        // This is needed because some OSes don't put volume label as first entry in the root directory
        uint sectorsForRootDirectory = 0;

        switch(bpbKind)
        {
            case BpbKind.DecRainbow:
            case BpbKind.Hardcoded:
            case BpbKind.Msx:
            case BpbKind.Apricot:
                isFat12 = true;

                break;
            case BpbKind.ShortFat32:
            case BpbKind.LongFat32:
            {
                isFat32 = true;

                Fat32ParameterBlock fat32Bpb = Marshal.ByteArrayToStructureLittleEndian<Fat32ParameterBlock>(bpbSector);

                Fat32ParameterBlockShort shortFat32Bpb =
                    Marshal.ByteArrayToStructureLittleEndian<Fat32ParameterBlockShort>(bpbSector);

                // This is to support FAT partitions on hybrid ISO/USB images
                if(imagePlugin.Info.MetadataMediaType == MetadataMediaType.OpticalDisc)
                {
                    fat32Bpb.bps       *= 4;
                    fat32Bpb.spc       /= 4;
                    fat32Bpb.big_spfat /= 4;
                    fat32Bpb.hsectors  /= 4;
                    fat32Bpb.sptrk     /= 4;
                }

                if(fat32Bpb.version != 0)
                {
                    sb.AppendLine(Localization.FAT_Plus);
                    metadata.Type = FS_TYPE_FAT_PLUS;
                }
                else
                {
                    sb.AppendLine(Localization.Microsoft_FAT32);
                    metadata.Type = FS_TYPE_FAT32;
                }

                if(fat32Bpb.oem_name != null)
                {
                    if(fat32Bpb.oem_name[5] == 0x49 && fat32Bpb.oem_name[6] == 0x48 && fat32Bpb.oem_name[7] == 0x43)
                        sb.AppendLine(Localization.Volume_has_been_modified_by_Windows_9x_Me_Volume_Tracker);
                    else
                        metadata.SystemIdentifier = StringHandlers.CToString(fat32Bpb.oem_name);
                }

                if(!string.IsNullOrEmpty(metadata.SystemIdentifier))
                    sb.AppendFormat(Localization.OEM_name_0, metadata.SystemIdentifier.Trim()).AppendLine();

                sb.AppendFormat(Localization._0_bytes_per_sector,    fat32Bpb.bps).AppendLine();
                sb.AppendFormat(Localization._0_sectors_per_cluster, fat32Bpb.spc).AppendLine();
                metadata.ClusterSize = (uint)(fat32Bpb.bps * fat32Bpb.spc);
                sb.AppendFormat(Localization._0_sectors_reserved_between_BPB_and_FAT, fat32Bpb.rsectors).AppendLine();

                if(fat32Bpb is { big_sectors: 0, signature: 0x28 })
                {
                    sb.AppendFormat(Localization._0_sectors_on_volume_1_bytes, shortFat32Bpb.huge_sectors,
                                    shortFat32Bpb.huge_sectors * shortFat32Bpb.bps).
                       AppendLine();

                    metadata.Clusters = shortFat32Bpb.huge_sectors / shortFat32Bpb.spc;
                }
                else if(fat32Bpb.sectors == 0)
                {
                    sb.AppendFormat(Localization._0_sectors_on_volume_1_bytes, fat32Bpb.big_sectors,
                                    fat32Bpb.big_sectors * fat32Bpb.bps).
                       AppendLine();

                    metadata.Clusters = fat32Bpb.big_sectors / fat32Bpb.spc;
                }
                else
                {
                    sb.AppendFormat(Localization._0_sectors_on_volume_1_bytes, fat32Bpb.sectors,
                                    fat32Bpb.sectors * fat32Bpb.bps).
                       AppendLine();

                    metadata.Clusters = (ulong)(fat32Bpb.sectors / fat32Bpb.spc);
                }

                sb.AppendFormat(Localization._0_clusters_on_volume,        metadata.Clusters).AppendLine();
                sb.AppendFormat(Localization.Media_descriptor_0,           fat32Bpb.media).AppendLine();
                sb.AppendFormat(Localization._0_sectors_per_FAT,           fat32Bpb.big_spfat).AppendLine();
                sb.AppendFormat(Localization._0_sectors_per_track,         fat32Bpb.sptrk).AppendLine();
                sb.AppendFormat(Localization._0_heads,                     fat32Bpb.heads).AppendLine();
                sb.AppendFormat(Localization._0_hidden_sectors_before_BPB, fat32Bpb.hsectors).AppendLine();
                sb.AppendFormat(Localization.Cluster_of_root_directory_0,  fat32Bpb.root_cluster).AppendLine();
                sb.AppendFormat(Localization.Sector_of_FSINFO_structure_0, fat32Bpb.fsinfo_sector).AppendLine();

                sb.AppendFormat(Localization.Sector_of_backup_FAT32_parameter_block_0, fat32Bpb.backup_sector).
                   AppendLine();

                sb.AppendFormat(Localization.Drive_number_0,         fat32Bpb.drive_no).AppendLine();
                sb.AppendFormat(Localization.Volume_Serial_Number_0, fat32Bpb.serial_no).AppendLine();
                metadata.VolumeSerial = $"{fat32Bpb.serial_no:X8}";

                if((fat32Bpb.flags & 0xF8) == 0x00)
                {
                    if((fat32Bpb.flags & 0x01) == 0x01)
                    {
                        sb.AppendLine(Localization.Volume_should_be_checked_on_next_mount);
                        metadata.Dirty = true;
                    }

                    if((fat32Bpb.flags & 0x02) == 0x02)
                        sb.AppendLine(Localization.Disk_surface_should_be_checked_on_next_mount);
                }

                if((fat32Bpb.mirror_flags & 0x80) == 0x80)
                {
                    sb.AppendFormat(Localization.FATs_are_out_of_sync_FAT_0_is_in_use, fat32Bpb.mirror_flags & 0xF).
                       AppendLine();
                }
                else
                    sb.AppendLine(Localization.All_copies_of_FAT_are_the_same);

                if((fat32Bpb.mirror_flags & 0x6F20) == 0x6F20)
                    sb.AppendLine(Localization.DR_DOS_will_boot_this_FAT32_using_CHS);
                else if((fat32Bpb.mirror_flags & 0x4F20) == 0x4F20)
                    sb.AppendLine(Localization.DR_DOS_will_boot_this_FAT32_using_LBA);

                if(fat32Bpb.signature == 0x29)
                {
                    metadata.VolumeName = StringHandlers.SpacePaddedToString(fat32Bpb.volume_label, encoding);
                    metadata.VolumeName = metadata.VolumeName?.Replace("\0", "");

                    sb.AppendFormat(Localization.Filesystem_type_0, Encoding.ASCII.GetString(fat32Bpb.fs_type)).
                       AppendLine();

                    bootChk = Sha1Context.Data(fat32Bpb.boot_code, out _);
                }
                else
                    bootChk = Sha1Context.Data(shortFat32Bpb.boot_code, out _);

                // Check that jumps to a correct boot code position and has boot signature set.
                // This will mean that the volume will boot, even if just to say "this is not bootable change disk"......
                metadata.Bootable =
                    fat32Bpb.jump[0] == 0xEB && fat32Bpb.jump[1] >= minBootNearJump && fat32Bpb.jump[1] < 0x80 ||
                    fat32Bpb.jump[0]                        == 0xE9            &&
                    fat32Bpb.jump.Length                    >= 3               &&
                    BitConverter.ToUInt16(fat32Bpb.jump, 1) >= minBootNearJump &&
                    BitConverter.ToUInt16(fat32Bpb.jump, 1) <= 0x1FC;

                sectorsPerRealSector = fat32Bpb.bps / imagePlugin.Info.SectorSize;

                // First root directory sector
                rootDirectorySector =
                    (ulong)((fat32Bpb.root_cluster - 2) * fat32Bpb.spc     +
                            fat32Bpb.big_spfat          * fat32Bpb.fats_no +
                            fat32Bpb.rsectors) *
                    sectorsPerRealSector;

                sectorsForRootDirectory = 1;

                if(fat32Bpb.fsinfo_sector + partition.Start <= partition.End)
                {
                    errno = imagePlugin.ReadSector(fat32Bpb.fsinfo_sector + partition.Start, out byte[] fsinfoSector);

                    if(errno != ErrorNumber.NoError)
                        return;

                    FsInfoSector fsInfo = Marshal.ByteArrayToStructureLittleEndian<FsInfoSector>(fsinfoSector);

                    if(fsInfo.signature1 == FSINFO_SIGNATURE1 &&
                       fsInfo is { signature2: FSINFO_SIGNATURE2, signature3: FSINFO_SIGNATURE3 })
                    {
                        if(fsInfo.free_clusters < 0xFFFFFFFF)
                        {
                            sb.AppendFormat(Localization._0_free_clusters, fsInfo.free_clusters).AppendLine();
                            metadata.FreeClusters = fsInfo.free_clusters;
                        }

                        if(fsInfo.last_cluster is > 2 and < 0xFFFFFFFF)
                            sb.AppendFormat(Localization.Last_allocated_cluster_0, fsInfo.last_cluster).AppendLine();
                    }
                }

                break;
            }

            // Some fields could overflow fake BPB, those will be handled below
            case BpbKind.Atari:
            {
                ushort sum = 0;

                for(var i = 0; i < bpbSector.Length; i += 2)
                    sum += BigEndianBitConverter.ToUInt16(bpbSector, i);

                // TODO: Check this
                if(sum == 0x1234)
                {
                    metadata.Bootable = true;
                    var atariSb = new StringBuilder();

                    atariSb.AppendFormat(Localization.cmdload_will_be_loaded_with_value_0,
                                         BigEndianBitConverter.ToUInt16(bpbSector, 0x01E)).
                            AppendLine();

                    atariSb.AppendFormat(Localization.Boot_program_will_be_loaded_at_address_0, atariBpb.ldaaddr).
                            AppendLine();

                    atariSb.AppendFormat(Localization.FAT_and_directory_will_be_cached_at_address_0, atariBpb.fatbuf).
                            AppendLine();

                    if(atariBpb.ldmode == 0)
                    {
                        var tmp = new byte[8];
                        Array.Copy(atariBpb.fname, 0, tmp, 0, 8);
                        string fname = Encoding.ASCII.GetString(tmp).Trim();
                        tmp = new byte[3];
                        Array.Copy(atariBpb.fname, 8, tmp, 0, 3);
                        string extension = Encoding.ASCII.GetString(tmp).Trim();
                        string filename;

                        if(string.IsNullOrEmpty(extension))
                            filename = fname;
                        else
                            filename = fname + "." + extension;

                        atariSb.AppendFormat(Localization.Boot_program_resides_in_file_0, filename).AppendLine();
                    }
                    else
                    {
                        atariSb.AppendFormat(Localization.Boot_program_starts_in_sector_0_and_is_1_sectors_long_2_bytes,
                                             atariBpb.ssect, atariBpb.sectcnt, atariBpb.sectcnt * atariBpb.bps).
                                AppendLine();
                    }

                    extraInfo = atariSb.ToString();
                }

                break;
            }

            case BpbKind.Human:
                metadata.Bootable = true;

                break;
        }

        if(!isFat32)
        {
            // This is to support FAT partitions on hybrid ISO/USB images
            if(imagePlugin.Info.MetadataMediaType == MetadataMediaType.OpticalDisc)
            {
                fakeBpb.bps      *= 4;
                fakeBpb.spc      /= 4;
                fakeBpb.spfat    /= 4;
                fakeBpb.hsectors /= 4;
                fakeBpb.sptrk    /= 4;
                fakeBpb.rsectors /= 4;

                if(fakeBpb.spc == 0)
                    fakeBpb.spc = 1;
            }

            ulong clusters;

            if(bpbKind != BpbKind.Human)
            {
                int reservedSectors = fakeBpb.rsectors                      +
                                      fakeBpb.fats_no       * fakeBpb.spfat +
                                      fakeBpb.root_ent * 32 / fakeBpb.bps;

                if(fakeBpb.sectors == 0)
                {
                    clusters = (ulong)(fakeBpb.spc == 0
                                           ? fakeBpb.big_sectors - reservedSectors
                                           : (fakeBpb.big_sectors - reservedSectors) / fakeBpb.spc);
                }
                else
                {
                    clusters = (ulong)(fakeBpb.spc == 0
                                           ? fakeBpb.sectors - reservedSectors
                                           : (fakeBpb.sectors - reservedSectors) / fakeBpb.spc);
                }
            }
            else
                clusters = humanBpb.clusters == 0 ? humanBpb.big_clusters : humanBpb.clusters;

            // This will walk all the FAT entries and check if they're valid FAT12 or FAT16 entries.
            // If the whole table is valid in both senses, it considers the type entry in the BPB.
            // BeOS is known to set the type as FAT16 but treat it as FAT12.
            if(!isFat12 && !isFat16)
            {
                if(clusters < 4089)
                {
                    // The first 2 FAT entries do not count as allocation clusters in FAT12 and FAT16
                    var fat12 = new ushort[clusters + 2];

                    _reservedSectors     = fakeBpb.rsectors;
                    sectorsPerRealSector = fakeBpb.bps / imagePlugin.Info.SectorSize;
                    _fatFirstSector      = partition.Start + _reservedSectors * sectorsPerRealSector;

                    errno = imagePlugin.ReadSectors(_fatFirstSector, fakeBpb.spfat, out byte[] fatBytes);

                    if(errno != ErrorNumber.NoError)
                        return;

                    var pos = 0;

                    for(var i = 0; i + 3 < fatBytes.Length && pos < fat12.Length; i += 3)
                    {
                        fat12[pos++] = (ushort)(((fatBytes[i + 1] & 0xF) << 8) + fatBytes[i + 0]);

                        if(pos >= fat12.Length)
                            break;

                        fat12[pos++] = (ushort)(((fatBytes[i + 1] & 0xF0) >> 4) + (fatBytes[i + 2] << 4));
                    }

                    bool fat12Valid = fat12[0] >= FAT12_RESERVED && fat12[1] >= FAT12_RESERVED;

                    if(fat12.Any(entry => entry < FAT12_RESERVED && entry > clusters + 2))
                        fat12Valid = false;

                    ushort[] fat16 = MemoryMarshal.Cast<byte, ushort>(fatBytes).ToArray();

                    bool fat16Valid = fat16[0] >= FAT16_RESERVED && fat16[1] >= 0x3FF0;

                    if(fat16.Any(entry => entry < FAT16_RESERVED && entry > clusters + 2))
                        fat16Valid = false;

                    isFat12 = fat12Valid;
                    isFat16 = fat16Valid;

                    // Check BPB type
                    if(isFat12 == isFat16)
                    {
                        isFat12 = fakeBpb.fs_type != null && Encoding.ASCII.GetString(fakeBpb.fs_type) == "FAT12   ";

                        isFat16 = fakeBpb.fs_type != null && Encoding.ASCII.GetString(fakeBpb.fs_type) == "FAT16   ";
                    }

                    if(!isFat12 && !isFat16)
                        isFat12 = true;
                }
                else
                    isFat16 = true;
            }

            if(isFat12)
            {
                switch(bpbKind)
                {
                    case BpbKind.Atari:
                        sb.AppendLine(Localization.Atari_FAT12);

                        break;
                    case BpbKind.Apricot:
                        sb.AppendLine(Localization.Apricot_FAT12);

                        break;
                    case BpbKind.Human:
                        sb.AppendLine(Localization.Human68k_FAT12);

                        break;
                    default:
                        sb.AppendLine(Localization.Microsoft_FAT12);

                        break;
                }

                metadata.Type = FS_TYPE_FAT12;
            }
            else if(isFat16)
            {
                sb.AppendLine(bpbKind switch
                              {
                                  BpbKind.Atari => Localization.Atari_FAT16,
                                  BpbKind.Human => Localization.Human68k_FAT16,
                                  _             => Localization.Microsoft_FAT16
                              });

                metadata.Type = FS_TYPE_FAT16;
            }

            if(bpbKind == BpbKind.Atari)
            {
                if(atariBpb.serial_no[0] == 0x49 && atariBpb.serial_no[1] == 0x48 && atariBpb.serial_no[2] == 0x43)
                    sb.AppendLine(Localization.Volume_has_been_modified_by_Windows_9x_Me_Volume_Tracker);
                else
                {
                    metadata.VolumeSerial = $"{atariBpb.serial_no[0]:X2}{atariBpb.serial_no[1]:X2}{atariBpb.serial_no[2]
                        :X2}";
                }

                metadata.SystemIdentifier = StringHandlers.CToString(atariBpb.oem_name);

                if(string.IsNullOrEmpty(metadata.SystemIdentifier))
                    metadata.SystemIdentifier = null;
            }
            else if(fakeBpb.oem_name != null)
            {
                if(fakeBpb.oem_name[5] == 0x49 && fakeBpb.oem_name[6] == 0x48 && fakeBpb.oem_name[7] == 0x43)
                    sb.AppendLine(Localization.Volume_has_been_modified_by_Windows_9x_Me_Volume_Tracker);
                else
                {
                    metadata.SystemIdentifier = fakeBpb.oem_name[0] switch
                                                {
                                                    // Later versions of Windows create a DOS 3 BPB without OEM name on 8 sectors/track floppies
                                                    // OEM ID should be ASCII, otherwise ignore it
                                                    >= 0x20 and <= 0x7F when fakeBpb.oem_name[1] >= 0x20 &&
                                                                             fakeBpb.oem_name[1] <= 0x7F &&
                                                                             fakeBpb.oem_name[2] >= 0x20 &&
                                                                             fakeBpb.oem_name[2] <= 0x7F &&
                                                                             fakeBpb.oem_name[3] >= 0x20 &&
                                                                             fakeBpb.oem_name[3] <= 0x7F &&
                                                                             fakeBpb.oem_name[4] >= 0x20 &&
                                                                             fakeBpb.oem_name[4] <= 0x7F &&
                                                                             fakeBpb.oem_name[5] >= 0x20 &&
                                                                             fakeBpb.oem_name[5] <= 0x7F &&
                                                                             fakeBpb.oem_name[6] >= 0x20 &&
                                                                             fakeBpb.oem_name[6] <= 0x7F &&
                                                                             fakeBpb.oem_name[7] >= 0x20 &&
                                                                             fakeBpb.oem_name[7] <= 0x7F =>
                                                        StringHandlers.CToString(fakeBpb.oem_name),
                                                    < 0x20 when fakeBpb.oem_name[1] >= 0x20 &&
                                                                fakeBpb.oem_name[1] <= 0x7F &&
                                                                fakeBpb.oem_name[2] >= 0x20 &&
                                                                fakeBpb.oem_name[2] <= 0x7F &&
                                                                fakeBpb.oem_name[3] >= 0x20 &&
                                                                fakeBpb.oem_name[3] <= 0x7F &&
                                                                fakeBpb.oem_name[4] >= 0x20 &&
                                                                fakeBpb.oem_name[4] <= 0x7F &&
                                                                fakeBpb.oem_name[5] >= 0x20 &&
                                                                fakeBpb.oem_name[5] <= 0x7F &&
                                                                fakeBpb.oem_name[6] >= 0x20 &&
                                                                fakeBpb.oem_name[6] <= 0x7F &&
                                                                fakeBpb.oem_name[7] >= 0x20 &&
                                                                fakeBpb.oem_name[7] <= 0x7F =>
                                                        StringHandlers.CToString(fakeBpb.oem_name, encoding, start: 1),
                                                    _ => metadata.SystemIdentifier
                                                };
                }

                if(fakeBpb.signature is 0x28 or 0x29)
                    metadata.VolumeSerial = $"{fakeBpb.serial_no:X8}";
            }

            if(metadata.SystemIdentifier != null)
                sb.AppendFormat(Localization.OEM_name_0, metadata.SystemIdentifier.Trim()).AppendLine();

            sb.AppendFormat(Localization._0_bytes_per_sector, fakeBpb.bps).AppendLine();

            if(bpbKind != BpbKind.Human)
            {
                if(fakeBpb.sectors == 0)
                {
                    sb.AppendFormat(Localization._0_sectors_on_volume_1_bytes, fakeBpb.big_sectors,
                                    fakeBpb.big_sectors * fakeBpb.bps).
                       AppendLine();
                }
                else
                {
                    sb.AppendFormat(Localization._0_sectors_on_volume_1_bytes, fakeBpb.sectors,
                                    fakeBpb.sectors * fakeBpb.bps).
                       AppendLine();
                }
            }
            else
            {
                sb.AppendFormat(Localization._0_sectors_on_volume_1_bytes,
                                clusters * humanBpb.bpc / imagePlugin.Info.SectorSize, clusters * humanBpb.bpc).
                   AppendLine();
            }

            metadata.Clusters = clusters;
            sb.AppendFormat(Localization._0_sectors_per_cluster, fakeBpb.spc).AppendLine();
            sb.AppendFormat(Localization._0_clusters_on_volume,  metadata.Clusters).AppendLine();
            metadata.ClusterSize = (uint)(fakeBpb.bps * fakeBpb.spc);
            sb.AppendFormat(Localization._0_sectors_reserved_between_BPB_and_FAT, fakeBpb.rsectors).AppendLine();
            sb.AppendFormat(Localization._0_FATs,                                 fakeBpb.fats_no).AppendLine();
            sb.AppendFormat(Localization._0_entries_in_root_directory,            fakeBpb.root_ent).AppendLine();

            if(fakeBpb.media > 0)
                sb.AppendFormat(Localization.Media_descriptor_0, fakeBpb.media).AppendLine();

            sb.AppendFormat(Localization._0_sectors_per_FAT, fakeBpb.spfat).AppendLine();

            if(fakeBpb.sptrk is > 0 and < 64 && fakeBpb.heads is > 0 and < 256)
            {
                sb.AppendFormat(Localization._0_sectors_per_track, fakeBpb.sptrk).AppendLine();
                sb.AppendFormat(Localization._0_heads,             fakeBpb.heads).AppendLine();
            }

            if(fakeBpb.hsectors <= partition.Start)
                sb.AppendFormat(Localization._0_hidden_sectors_before_BPB, fakeBpb.hsectors).AppendLine();

            if(fakeBpb.signature is 0x28 or 0x29 || andosOemCorrect)
            {
                sb.AppendFormat(Localization.Drive_number_0, fakeBpb.drive_no).AppendLine();

                if(metadata.VolumeSerial != null)
                    sb.AppendFormat(Localization.Volume_Serial_Number_0, metadata.VolumeSerial).AppendLine();

                if((fakeBpb.flags & 0xF8) == 0x00)
                {
                    if((fakeBpb.flags & 0x01) == 0x01)
                    {
                        sb.AppendLine(Localization.Volume_should_be_checked_on_next_mount);
                        metadata.Dirty = true;
                    }

                    if((fakeBpb.flags & 0x02) == 0x02)
                        sb.AppendLine(Localization.Disk_surface_should_be_checked_on_next_mount);
                }

                if(fakeBpb.signature == 0x29 || andosOemCorrect)
                {
                    metadata.VolumeName = StringHandlers.SpacePaddedToString(fakeBpb.volume_label, encoding);
                    metadata.VolumeName = metadata.VolumeName?.Replace("\0", "");

                    sb.AppendFormat(Localization.Filesystem_type_0, Encoding.ASCII.GetString(fakeBpb.fs_type)).
                       AppendLine();
                }
            }
            else if(bpbKind == BpbKind.Atari && metadata.VolumeSerial != null)
                sb.AppendFormat(Localization.Volume_Serial_Number_0, metadata.VolumeSerial).AppendLine();

            bootChk = Sha1Context.Data(fakeBpb.boot_code, out _);

            // Workaround that PCExchange jumps into "FAT16   "...
            if(metadata.SystemIdentifier == "PCX 2.0 ")
                fakeBpb.jump[1] += 8;

            // Check that jumps to a correct boot code position and has boot signature set.
            // This will mean that the volume will boot, even if just to say "this is not bootable change disk"......
            if(metadata.Bootable == false && fakeBpb.jump != null)
            {
                metadata.Bootable |=
                    fakeBpb.jump[0] == 0xEB && fakeBpb.jump[1] >= minBootNearJump && fakeBpb.jump[1] < 0x80 ||
                    fakeBpb.jump[0]                        == 0xE9            &&
                    fakeBpb.jump.Length                    >= 3               &&
                    BitConverter.ToUInt16(fakeBpb.jump, 1) >= minBootNearJump &&
                    BitConverter.ToUInt16(fakeBpb.jump, 1) <= 0x1FC;
            }

            sectorsPerRealSector = fakeBpb.bps / imagePlugin.Info.SectorSize;

            // First root directory sector
            rootDirectorySector = (ulong)(fakeBpb.spfat * fakeBpb.fats_no + fakeBpb.rsectors) * sectorsPerRealSector;

            sectorsForRootDirectory = (uint)(fakeBpb.root_ent * 32 / imagePlugin.Info.SectorSize);
        }

        if(extraInfo != null)
            sb.Append(extraInfo);

        if(rootDirectorySector + partition.Start < partition.End &&
           imagePlugin.Info.MetadataMediaType    != MetadataMediaType.OpticalDisc)
        {
            errno = imagePlugin.ReadSectors(rootDirectorySector + partition.Start, sectorsForRootDirectory,
                                            out byte[] rootDirectory);

            if(errno != ErrorNumber.NoError)
                return;

            if(bpbKind == BpbKind.DecRainbow)
            {
                var rootMs = new MemoryStream();

                foreach(ulong rootSector in new ulong[]
                    {
                        0x17, 0x19, 0x1B, 0x1D, 0x1E, 0x20
                    })
                {
                    errno = imagePlugin.ReadSector(rootSector, out byte[] tmp);

                    if(errno != ErrorNumber.NoError)
                        return;

                    rootMs.Write(tmp, 0, tmp.Length);
                }

                rootDirectory = rootMs.ToArray();
            }

            for(var i = 0; i < rootDirectory.Length; i += 32)
            {
                // Not a correct entry
                if(rootDirectory[i] < DIRENT_MIN && rootDirectory[i] != DIRENT_E5)
                    continue;

                // Deleted or subdirectory entry
                if(rootDirectory[i] == DIRENT_SUBDIR || rootDirectory[i] == DIRENT_DELETED)
                    continue;

                // Not a volume label
                if(rootDirectory[i + 0x0B] != 0x08 && rootDirectory[i + 0x0B] != 0x28)
                    continue;

                DirectoryEntry entry = Marshal.ByteArrayToStructureLittleEndian<DirectoryEntry>(rootDirectory, i, 32);

                var fullname = new byte[11];
                Array.Copy(entry.filename,  0, fullname, 0, 8);
                Array.Copy(entry.extension, 0, fullname, 8, 3);
                string volname = encoding.GetString(fullname).Trim();

                if(!string.IsNullOrEmpty(volname))
                    metadata.VolumeName = entry.caseinfo.HasFlag(CaseInfo.AllLowerCase) ? volname.ToLower() : volname;

                if(entry is { ctime: > 0, cdate: > 0 })
                {
                    metadata.CreationDate = DateHandlers.DosToDateTime(entry.cdate, entry.ctime);

                    if(entry.ctime_ms > 0)
                        metadata.CreationDate = metadata.CreationDate?.AddMilliseconds(entry.ctime_ms * 10);

                    sb.AppendFormat(Localization.Volume_created_on_0, metadata.CreationDate).AppendLine();
                }

                if(entry is { mtime: > 0, mdate: > 0 })
                {
                    metadata.ModificationDate = DateHandlers.DosToDateTime(entry.mdate, entry.mtime);
                    sb.AppendFormat(Localization.Volume_last_modified_on_0, metadata.ModificationDate).AppendLine();
                }

                if(entry.adate > 0)
                {
                    sb.AppendFormat(Localization.Volume_last_accessed_on_0_d,
                                    DateHandlers.DosToDateTime(entry.adate, 0)).
                       AppendLine();
                }

                break;
            }
        }

        if(!string.IsNullOrEmpty(metadata.VolumeName))
            sb.AppendFormat(Localization.Volume_label_0, metadata.VolumeName).AppendLine();

        if(metadata.Bootable)
        {
            switch(bpbSector[0])
            {
                // Intel short jump
                case 0xEB when bpbSector[1] < 0x80:
                {
                    int sigSize  = bpbSector[510] == 0x55 && bpbSector[511] == 0xAA ? 2 : 0;
                    var bootCode = new byte[512 - sigSize - bpbSector[1] - 2];
                    Array.Copy(bpbSector, bpbSector[1] + 2, bootCode, 0, bootCode.Length);
                    Sha1Context.Data(bootCode, out _);

                    break;
                }

                // Intel big jump
                case 0xE9 when BitConverter.ToUInt16(bpbSector, 1) < 0x1FC:
                {
                    int sigSize  = bpbSector[510] == 0x55 && bpbSector[511] == 0xAA ? 2 : 0;
                    var bootCode = new byte[512 - sigSize - BitConverter.ToUInt16(bpbSector, 1) - 3];
                    Array.Copy(bpbSector, BitConverter.ToUInt16(bpbSector, 1) + 3, bootCode, 0, bootCode.Length);
                    Sha1Context.Data(bootCode, out _);

                    break;
                }
            }

            sb.AppendLine(Localization.Volume_is_bootable);
            sb.AppendFormat(Localization.Boot_code_SHA1_0, bootChk).AppendLine();
            string bootName = _knownBootHashes.FirstOrDefault(t => t.hash == bootChk).name;

            if(string.IsNullOrWhiteSpace(bootName))
                sb.AppendLine(Localization.Unknown_boot_code);
            else
                sb.AppendFormat(Localization.Boot_code_corresponds_to_0, bootName).AppendLine();
        }

        information = sb.ToString();
    }

#endregion
}