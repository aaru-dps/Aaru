// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft FAT filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Microsoft FAT filesystem and shows information.
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
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems
{
    public sealed partial class FAT
    {
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
            byte[] fat32Id = new byte[8];
            byte[] msxId   = new byte[6];
            byte   fatId;
            byte[] dosOem   = new byte[8];
            byte[] atariOem = new byte[6];
            ushort bootable = 0;

            uint sectorsPerBpb = imagePlugin.Info.SectorSize < 512 ? 512 / imagePlugin.Info.SectorSize : 1;

            byte[] bpbSector = imagePlugin.ReadSectors(0            + partition.Start, sectorsPerBpb);
            byte[] fatSector = imagePlugin.ReadSector(sectorsPerBpb + partition.Start);

            HumanParameterBlock humanBpb = Marshal.ByteArrayToStructureBigEndian<HumanParameterBlock>(bpbSector);

            ulong expectedClusters = humanBpb.bpc > 0 ? partition.Size / humanBpb.bpc : 0;

            AaruConsole.DebugWriteLine("FAT plugin", "Human bpc = {0}", humanBpb.bpc);
            AaruConsole.DebugWriteLine("FAT plugin", "Human clusters = {0}", humanBpb.clusters);
            AaruConsole.DebugWriteLine("FAT plugin", "Human big_clusters = {0}", humanBpb.big_clusters);
            AaruConsole.DebugWriteLine("FAT plugin", "Human expected clusters = {0}", expectedClusters);

            // Check clusters for Human68k are correct
            bool humanClustersCorrect = humanBpb.clusters       == 0 ? humanBpb.big_clusters == expectedClusters
                                            : humanBpb.clusters == expectedClusters;

            // Check OEM for Human68k is correct
            bool humanOemCorrect = bpbSector[2]  >= 0x20 && bpbSector[3]  >= 0x20 && bpbSector[4]  >= 0x20 &&
                                   bpbSector[5]  >= 0x20 && bpbSector[6]  >= 0x20 && bpbSector[7]  >= 0x20 &&
                                   bpbSector[8]  >= 0x20 && bpbSector[9]  >= 0x20 && bpbSector[10] >= 0x20 &&
                                   bpbSector[11] >= 0x20 && bpbSector[12] >= 0x20 && bpbSector[13] >= 0x20 &&
                                   bpbSector[14] >= 0x20 && bpbSector[15] >= 0x20 && bpbSector[16] >= 0x20 &&
                                   bpbSector[17] >= 0x20;

            // Check correct branch for Human68k
            bool humanBranchCorrect = bpbSector[0] == 0x60 && bpbSector[1] >= 0x20 && bpbSector[1] < 0xFE;

            AaruConsole.DebugWriteLine("FAT plugin", "humanClustersCorrect = {0}", humanClustersCorrect);
            AaruConsole.DebugWriteLine("FAT plugin", "humanOemCorrect = {0}", humanOemCorrect);
            AaruConsole.DebugWriteLine("FAT plugin", "humanBranchCorrect = {0}", humanBranchCorrect);

            // If all Human68k checks are correct, it is a Human68k FAT16
            if(humanClustersCorrect &&
               humanOemCorrect      &&
               humanBranchCorrect   &&
               expectedClusters > 0)
                return true;

            Array.Copy(bpbSector, 0x02, atariOem, 0, 6);
            Array.Copy(bpbSector, 0x03, dosOem, 0, 8);
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

            bool   correctSpc  = spc == 1 || spc == 2 || spc == 4 || spc == 8 || spc == 16 || spc == 32 || spc == 64;
            string msxString   = Encoding.ASCII.GetString(msxId);
            string fat32String = Encoding.ASCII.GetString(fat32Id);

            bool atariOemCorrect = atariOem[0] >= 0x20 && atariOem[1] >= 0x20 && atariOem[2] >= 0x20 &&
                                   atariOem[3] >= 0x20 && atariOem[4] >= 0x20 && atariOem[5] >= 0x20;

            bool dosOemCorrect = dosOem[0] >= 0x20 && dosOem[1] >= 0x20 && dosOem[2] >= 0x20 && dosOem[3] >= 0x20 &&
                                 dosOem[4] >= 0x20 && dosOem[5] >= 0x20 && dosOem[6] >= 0x20 && dosOem[7] >= 0x20;

            string oemString = Encoding.ASCII.GetString(dosOem);

            AaruConsole.DebugWriteLine("FAT plugin", "atari_oem_correct = {0}", atariOemCorrect);
            AaruConsole.DebugWriteLine("FAT plugin", "dos_oem_correct = {0}", dosOemCorrect);
            AaruConsole.DebugWriteLine("FAT plugin", "bps = {0}", bps);
            AaruConsole.DebugWriteLine("FAT plugin", "bits in bps = {0}", bitsInBps);
            AaruConsole.DebugWriteLine("FAT plugin", "spc = {0}", spc);
            AaruConsole.DebugWriteLine("FAT plugin", "correct_spc = {0}", correctSpc);
            AaruConsole.DebugWriteLine("FAT plugin", "reserved_secs = {0}", reservedSecs);
            AaruConsole.DebugWriteLine("FAT plugin", "fats_no = {0}", numberOfFats);
            AaruConsole.DebugWriteLine("FAT plugin", "root_entries = {0}", rootEntries);
            AaruConsole.DebugWriteLine("FAT plugin", "sectors = {0}", sectors);
            AaruConsole.DebugWriteLine("FAT plugin", "media_descriptor = 0x{0:X2}", mediaDescriptor);
            AaruConsole.DebugWriteLine("FAT plugin", "fat_sectors = {0}", fatSectors);
            AaruConsole.DebugWriteLine("FAT plugin", "msx_id = \"{0}\"", msxString);
            AaruConsole.DebugWriteLine("FAT plugin", "big_sectors = {0}", bigSectors);
            AaruConsole.DebugWriteLine("FAT plugin", "bpb_signature = 0x{0:X2}", bpbSignature);
            AaruConsole.DebugWriteLine("FAT plugin", "fat32_signature = 0x{0:X2}", fat32Signature);
            AaruConsole.DebugWriteLine("FAT plugin", "fat32_id = \"{0}\"", fat32String);
            AaruConsole.DebugWriteLine("FAT plugin", "huge_sectors = {0}", hugeSectors);
            AaruConsole.DebugWriteLine("FAT plugin", "fat_id = 0x{0:X2}", fatId);

            ushort apricotBps             = BitConverter.ToUInt16(bpbSector, 0x50);
            byte   apricotSpc             = bpbSector[0x52];
            ushort apricotReservedSecs    = BitConverter.ToUInt16(bpbSector, 0x53);
            byte   apricotFatsNo          = bpbSector[0x55];
            ushort apricotRootEntries     = BitConverter.ToUInt16(bpbSector, 0x56);
            ushort apricotSectors         = BitConverter.ToUInt16(bpbSector, 0x58);
            byte   apricotMediaDescriptor = bpbSector[0x5A];
            ushort apricotFatSectors      = BitConverter.ToUInt16(bpbSector, 0x5B);

            bool apricotCorrectSpc = apricotSpc == 1  || apricotSpc == 2  || apricotSpc == 4 || apricotSpc == 8 ||
                                     apricotSpc == 16 || apricotSpc == 32 || apricotSpc == 64;

            int  bitsInApricotBps  = CountBits.Count(apricotBps);
            byte apricotPartitions = bpbSector[0x0C];

            AaruConsole.DebugWriteLine("FAT plugin", "apricot_bps = {0}", apricotBps);
            AaruConsole.DebugWriteLine("FAT plugin", "apricot_spc = {0}", apricotSpc);
            AaruConsole.DebugWriteLine("FAT plugin", "apricot_correct_spc = {0}", apricotCorrectSpc);
            AaruConsole.DebugWriteLine("FAT plugin", "apricot_reserved_secs = {0}", apricotReservedSecs);
            AaruConsole.DebugWriteLine("FAT plugin", "apricot_fats_no = {0}", apricotFatsNo);
            AaruConsole.DebugWriteLine("FAT plugin", "apricot_root_entries = {0}", apricotRootEntries);
            AaruConsole.DebugWriteLine("FAT plugin", "apricot_sectors = {0}", apricotSectors);
            AaruConsole.DebugWriteLine("FAT plugin", "apricot_media_descriptor = 0x{0:X2}", apricotMediaDescriptor);
            AaruConsole.DebugWriteLine("FAT plugin", "apricot_fat_sectors = {0}", apricotFatSectors);

            // This is to support FAT partitions on hybrid ISO/USB images
            if(imagePlugin.Info.XmlMediaType == XmlMediaType.OpticalDisc)
            {
                sectors     /= 4;
                bigSectors  /= 4;
                hugeSectors /= 4;
            }

            switch(oemString)
            {
                // exFAT
                case "EXFAT   ": return false;

                // NTFS
                case "NTFS    " when bootable == 0xAA55 && numberOfFats == 0 && fatSectors == 0: return false;

                // QNX4
                case "FQNX4FS ": return false;
            }

            // HPFS
            if(16 + partition.Start <= partition.End)
            {
                byte[] hpfsSbSector =
                    imagePlugin.ReadSector(16 + partition.Start); // Seek to superblock, on logical sector 16

                uint hpfsMagic1 = BitConverter.ToUInt32(hpfsSbSector, 0x000);
                uint hpfsMagic2 = BitConverter.ToUInt32(hpfsSbSector, 0x004);

                if(hpfsMagic1 == 0xF995E849 &&
                   hpfsMagic2 == 0xFA53E9C5)
                    return false;
            }

            switch(bitsInBps)
            {
                // FAT32 for sure
                case 1 when correctSpc && numberOfFats <= 2 && fatSectors == 0 && fat32Signature == 0x29 &&
                            fat32String                == "FAT32   ": return true;

                // short FAT32
                case 1 when correctSpc && numberOfFats <= 2 && fatSectors == 0 && fat32Signature == 0x28:
                    return sectors == 0 ? bigSectors        == 0
                                              ? hugeSectors <= partition.End - partition.Start + 1
                                              : bigSectors  <= partition.End - partition.Start + 1
                               : sectors <= partition.End - partition.Start + 1;

                // MSX-DOS FAT12
                case 1 when correctSpc && numberOfFats <= 2                                   && rootEntries > 0 &&
                            sectors                    <= partition.End - partition.Start + 1 && fatSectors  > 0 &&
                            msxString                  == "VOL_ID": return true;

                // EBPB
                case 1 when correctSpc && numberOfFats <= 2 && rootEntries > 0 && fatSectors > 0 &&
                            (bpbSignature == 0x28 || bpbSignature == 0x29):
                    return sectors       == 0 ? bigSectors <= partition.End - partition.Start + 1
                               : sectors <= partition.End                   - partition.Start + 1;

                // BPB
                case 1 when correctSpc && reservedSecs < partition.End - partition.Start && numberOfFats <= 2 &&
                            rootEntries                > 0                               && fatSectors   > 0:
                    return sectors       == 0 ? bigSectors <= partition.End - partition.Start + 1
                               : sectors <= partition.End                   - partition.Start + 1;
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
            if(imagePlugin.Info.Sectors    == 800 &&
               imagePlugin.Info.SectorSize == 512)
            {
                // DEC Rainbow boots up with a Z80, first byte should be DI (disable interrupts)
                byte z80Di = bpbSector[0];

                // First FAT1 sector resides at LBA 0x14
                byte[] fat1Sector0 = imagePlugin.ReadSector(0x14);

                // First FAT2 sector resides at LBA 0x1A
                byte[] fat2Sector0 = imagePlugin.ReadSector(0x1A);
                bool   equalFatIds = fat1Sector0[0] == fat2Sector0[0] && fat1Sector0[1] == fat2Sector0[1];

                // Volume is software interleaved 2:1
                var rootMs = new MemoryStream();

                foreach(byte[] tmp in from ulong rootSector in new ulong[]
                {
                    0x17, 0x19, 0x1B, 0x1D, 0x1E, 0x20
                } select imagePlugin.ReadSector(rootSector))
                    rootMs.Write(tmp, 0, tmp.Length);

                byte[] rootDir      = rootMs.ToArray();
                bool   validRootDir = true;

                // Iterate all root directory
                for(int e = 0; e < 96 * 32; e += 32)
                {
                    for(int c = 0; c < 11; c++)
                        if((rootDir[c + e] < 0x20 && rootDir[c + e] != 0x00 && rootDir[c + e] != 0x05) ||
                           rootDir[c + e] == 0xFF                                                      ||
                           rootDir[c + e] == 0x2E)
                        {
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

            byte   fat2        = fatSector[1];
            byte   fat3        = fatSector[2];
            ushort fatCluster2 = (ushort)(((fat2 << 8) + fat3) & 0xFFF);

            AaruConsole.DebugWriteLine("FAT plugin", "1st fat cluster 1 = {0:X3}", fatCluster2);

            if(fatCluster2 < 0xFF0)
                return false;

            ulong fat2SectorNo = 0;

            switch(fatId)
            {
                case 0xE5:
                    if(imagePlugin.Info.Sectors    == 2002 &&
                       imagePlugin.Info.SectorSize == 128)
                        fat2SectorNo = 2;

                    break;
                case 0xFD:
                    if(imagePlugin.Info.Sectors    == 4004 &&
                       imagePlugin.Info.SectorSize == 128)
                        fat2SectorNo = 7;
                    else if(imagePlugin.Info.Sectors    == 2002 &&
                            imagePlugin.Info.SectorSize == 128)
                        fat2SectorNo = 7;

                    break;
                case 0xFE:
                    if(imagePlugin.Info.Sectors    == 320 &&
                       imagePlugin.Info.SectorSize == 512)
                        fat2SectorNo = 2;
                    else if(imagePlugin.Info.Sectors    == 2002 &&
                            imagePlugin.Info.SectorSize == 128)
                        fat2SectorNo = 7;
                    else if(imagePlugin.Info.Sectors    == 1232 &&
                            imagePlugin.Info.SectorSize == 1024)
                        fat2SectorNo = 3;
                    else if(imagePlugin.Info.Sectors    == 616 &&
                            imagePlugin.Info.SectorSize == 1024)
                        fat2SectorNo = 2;
                    else if(imagePlugin.Info.Sectors    == 720 &&
                            imagePlugin.Info.SectorSize == 128)
                        fat2SectorNo = 5;
                    else if(imagePlugin.Info.Sectors    == 640 &&
                            imagePlugin.Info.SectorSize == 512)
                        fat2SectorNo = 2;

                    break;
                case 0xFF:
                    if(imagePlugin.Info.Sectors    == 640 &&
                       imagePlugin.Info.SectorSize == 512)
                        fat2SectorNo = 2;

                    break;
                default:
                    if(fatId < 0xE8)
                        return false;

                    fat2SectorNo = 2;

                    break;
            }

            if(fat2SectorNo > partition.End ||
               fat2SectorNo == 0)
                return false;

            AaruConsole.DebugWriteLine("FAT plugin", "2nd fat starts at = {0}", fat2SectorNo);

            byte[] fat2Sector = imagePlugin.ReadSector(fat2SectorNo);

            fat2        = fat2Sector[1];
            fat3        = fat2Sector[2];
            fatCluster2 = (ushort)(((fat2 << 8) + fat3) & 0xFFF);

            if(fatCluster2 < 0xFF0)
                return false;

            return fatId == fat2Sector[0];
        }

        /// <inheritdoc />
        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding    = encoding ?? Encoding.GetEncoding("IBM437");
            information = "";

            var sb = new StringBuilder();
            XmlFsType = new FileSystemType();

            uint sectorsPerBpb = imagePlugin.Info.SectorSize < 512 ? 512 / imagePlugin.Info.SectorSize : 1;

            byte[] bpbSector = imagePlugin.ReadSectors(0 + partition.Start, sectorsPerBpb);

            BpbKind bpbKind = DetectBpbKind(bpbSector, imagePlugin, partition, out BiosParameterBlockEbpb fakeBpb,
                                            out HumanParameterBlock humanBpb, out AtariParameterBlock atariBpb,
                                            out byte minBootNearJump, out bool andosOemCorrect, out bool bootable);

            bool   isFat12             = false;
            bool   isFat16             = false;
            bool   isFat32             = false;
            ulong  rootDirectorySector = 0;
            string extraInfo           = null;
            string bootChk             = null;
            XmlFsType.Bootable = bootable;

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

                    Fat32ParameterBlock fat32Bpb =
                        Marshal.ByteArrayToStructureLittleEndian<Fat32ParameterBlock>(bpbSector);

                    Fat32ParameterBlockShort shortFat32Bpb =
                        Marshal.ByteArrayToStructureLittleEndian<Fat32ParameterBlockShort>(bpbSector);

                    // This is to support FAT partitions on hybrid ISO/USB images
                    if(imagePlugin.Info.XmlMediaType == XmlMediaType.OpticalDisc)
                    {
                        fat32Bpb.bps       *= 4;
                        fat32Bpb.spc       /= 4;
                        fat32Bpb.big_spfat /= 4;
                        fat32Bpb.hsectors  /= 4;
                        fat32Bpb.sptrk     /= 4;
                    }

                    if(fat32Bpb.version != 0)
                    {
                        sb.AppendLine("FAT+");
                        XmlFsType.Type = "FAT+";
                    }
                    else
                    {
                        sb.AppendLine("Microsoft FAT32");
                        XmlFsType.Type = "FAT32";
                    }

                    if(fat32Bpb.oem_name != null)
                        if(fat32Bpb.oem_name[5] == 0x49 &&
                           fat32Bpb.oem_name[6] == 0x48 &&
                           fat32Bpb.oem_name[7] == 0x43)
                            sb.AppendLine("Volume has been modified by Windows 9x/Me Volume Tracker.");
                        else
                            XmlFsType.SystemIdentifier = StringHandlers.CToString(fat32Bpb.oem_name);

                    if(!string.IsNullOrEmpty(XmlFsType.SystemIdentifier))
                        sb.AppendFormat("OEM Name: {0}", XmlFsType.SystemIdentifier.Trim()).AppendLine();

                    sb.AppendFormat("{0} bytes per sector.", fat32Bpb.bps).AppendLine();
                    sb.AppendFormat("{0} sectors per cluster.", fat32Bpb.spc).AppendLine();
                    XmlFsType.ClusterSize = (uint)(fat32Bpb.bps * fat32Bpb.spc);
                    sb.AppendFormat("{0} sectors reserved between BPB and FAT.", fat32Bpb.rsectors).AppendLine();

                    if(fat32Bpb.big_sectors == 0 &&
                       fat32Bpb.signature   == 0x28)
                    {
                        sb.AppendFormat("{0} sectors on volume ({1} bytes).", shortFat32Bpb.huge_sectors,
                                        shortFat32Bpb.huge_sectors * shortFat32Bpb.bps).AppendLine();

                        XmlFsType.Clusters = shortFat32Bpb.huge_sectors / shortFat32Bpb.spc;
                    }
                    else if(fat32Bpb.sectors == 0)
                    {
                        sb.AppendFormat("{0} sectors on volume ({1} bytes).", fat32Bpb.big_sectors,
                                        fat32Bpb.big_sectors * fat32Bpb.bps).AppendLine();

                        XmlFsType.Clusters = fat32Bpb.big_sectors / fat32Bpb.spc;
                    }
                    else
                    {
                        sb.AppendFormat("{0} sectors on volume ({1} bytes).", fat32Bpb.sectors,
                                        fat32Bpb.sectors * fat32Bpb.bps).AppendLine();

                        XmlFsType.Clusters = (ulong)(fat32Bpb.sectors / fat32Bpb.spc);
                    }

                    sb.AppendFormat("{0} clusters on volume.", XmlFsType.Clusters).AppendLine();
                    sb.AppendFormat("Media descriptor: 0x{0:X2}", fat32Bpb.media).AppendLine();
                    sb.AppendFormat("{0} sectors per FAT.", fat32Bpb.big_spfat).AppendLine();
                    sb.AppendFormat("{0} sectors per track.", fat32Bpb.sptrk).AppendLine();
                    sb.AppendFormat("{0} heads.", fat32Bpb.heads).AppendLine();
                    sb.AppendFormat("{0} hidden sectors before BPB.", fat32Bpb.hsectors).AppendLine();
                    sb.AppendFormat("Cluster of root directory: {0}", fat32Bpb.root_cluster).AppendLine();
                    sb.AppendFormat("Sector of FSINFO structure: {0}", fat32Bpb.fsinfo_sector).AppendLine();
                    sb.AppendFormat("Sector of backup FAT32 parameter block: {0}", fat32Bpb.backup_sector).AppendLine();
                    sb.AppendFormat("Drive number: 0x{0:X2}", fat32Bpb.drive_no).AppendLine();
                    sb.AppendFormat("Volume Serial Number: 0x{0:X8}", fat32Bpb.serial_no).AppendLine();
                    XmlFsType.VolumeSerial = $"{fat32Bpb.serial_no:X8}";

                    if((fat32Bpb.flags & 0xF8) == 0x00)
                    {
                        if((fat32Bpb.flags & 0x01) == 0x01)
                        {
                            sb.AppendLine("Volume should be checked on next mount.");
                            XmlFsType.Dirty = true;
                        }

                        if((fat32Bpb.flags & 0x02) == 0x02)
                            sb.AppendLine("Disk surface should be on next mount.");
                    }

                    if((fat32Bpb.mirror_flags & 0x80) == 0x80)
                        sb.AppendFormat("FATs are out of sync. FAT #{0} is in use.", fat32Bpb.mirror_flags & 0xF).
                           AppendLine();
                    else
                        sb.AppendLine("All copies of FAT are the same.");

                    if((fat32Bpb.mirror_flags & 0x6F20) == 0x6F20)
                        sb.AppendLine("DR-DOS will boot this FAT32 using CHS.");
                    else if((fat32Bpb.mirror_flags & 0x4F20) == 0x4F20)
                        sb.AppendLine("DR-DOS will boot this FAT32 using LBA.");

                    if(fat32Bpb.signature == 0x29)
                    {
                        XmlFsType.VolumeName = StringHandlers.SpacePaddedToString(fat32Bpb.volume_label, Encoding);
                        XmlFsType.VolumeName = XmlFsType.VolumeName?.Replace("\0", "");

                        sb.AppendFormat("Filesystem type: {0}", Encoding.ASCII.GetString(fat32Bpb.fs_type)).
                           AppendLine();

                        bootChk = Sha1Context.Data(fat32Bpb.boot_code, out _);
                    }
                    else
                        bootChk = Sha1Context.Data(shortFat32Bpb.boot_code, out _);

                    // Check that jumps to a correct boot code position and has boot signature set.
                    // This will mean that the volume will boot, even if just to say "this is not bootable change disk"......
                    XmlFsType.Bootable =
                        (fat32Bpb.jump[0] == 0xEB && fat32Bpb.jump[1] >= minBootNearJump && fat32Bpb.jump[1] < 0x80) ||
                        (fat32Bpb.jump[0]                        == 0xE9            && fat32Bpb.jump.Length >= 3 &&
                         BitConverter.ToUInt16(fat32Bpb.jump, 1) >= minBootNearJump &&
                         BitConverter.ToUInt16(fat32Bpb.jump, 1) <= 0x1FC);

                    sectorsPerRealSector = fat32Bpb.bps / imagePlugin.Info.SectorSize;

                    // First root directory sector
                    rootDirectorySector =
                        (ulong)(((fat32Bpb.root_cluster - 2) * fat32Bpb.spc) + (fat32Bpb.big_spfat * fat32Bpb.fats_no) +
                                fat32Bpb.rsectors) * sectorsPerRealSector;

                    sectorsForRootDirectory = 1;

                    if(fat32Bpb.fsinfo_sector + partition.Start <= partition.End)
                    {
                        byte[] fsinfoSector = imagePlugin.ReadSector(fat32Bpb.fsinfo_sector + partition.Start);

                        FsInfoSector fsInfo = Marshal.ByteArrayToStructureLittleEndian<FsInfoSector>(fsinfoSector);

                        if(fsInfo.signature1 == FSINFO_SIGNATURE1 &&
                           fsInfo.signature2 == FSINFO_SIGNATURE2 &&
                           fsInfo.signature3 == FSINFO_SIGNATURE3)
                        {
                            if(fsInfo.free_clusters < 0xFFFFFFFF)
                            {
                                sb.AppendFormat("{0} free clusters", fsInfo.free_clusters).AppendLine();
                                XmlFsType.FreeClusters          = fsInfo.free_clusters;
                                XmlFsType.FreeClustersSpecified = true;
                            }

                            if(fsInfo.last_cluster > 2 &&
                               fsInfo.last_cluster < 0xFFFFFFFF)
                                sb.AppendFormat("Last allocated cluster {0}", fsInfo.last_cluster).AppendLine();
                        }
                    }

                    break;
                }

                // Some fields could overflow fake BPB, those will be handled below
                case BpbKind.Atari:
                {
                    ushort sum = 0;

                    for(int i = 0; i < bpbSector.Length; i += 2)
                        sum += BigEndianBitConverter.ToUInt16(bpbSector, i);

                    // TODO: Check this
                    if(sum == 0x1234)
                    {
                        XmlFsType.Bootable = true;
                        var atariSb = new StringBuilder();

                        atariSb.AppendFormat("cmdload will be loaded with value {0:X4}h",
                                             BigEndianBitConverter.ToUInt16(bpbSector, 0x01E)).AppendLine();

                        atariSb.AppendFormat("Boot program will be loaded at address {0:X4}h", atariBpb.ldaaddr).
                                AppendLine();

                        atariSb.AppendFormat("FAT and directory will be cached at address {0:X4}h", atariBpb.fatbuf).
                                AppendLine();

                        if(atariBpb.ldmode == 0)
                        {
                            byte[] tmp = new byte[8];
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

                            atariSb.AppendFormat("Boot program resides in file \"{0}\"", filename).AppendLine();
                        }
                        else
                            atariSb.
                                AppendFormat("Boot program starts in sector {0} and is {1} sectors long ({2} bytes)",
                                             atariBpb.ssect, atariBpb.sectcnt, atariBpb.sectcnt * atariBpb.bps).
                                AppendLine();

                        extraInfo = atariSb.ToString();
                    }

                    break;
                }

                case BpbKind.Human:
                    XmlFsType.Bootable = true;

                    break;
            }

            if(!isFat32)
            {
                // This is to support FAT partitions on hybrid ISO/USB images
                if(imagePlugin.Info.XmlMediaType == XmlMediaType.OpticalDisc)
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
                    int reservedSectors = fakeBpb.rsectors + (fakeBpb.fats_no * fakeBpb.spfat) +
                                          (fakeBpb.root_ent * 32              / fakeBpb.bps);

                    if(fakeBpb.sectors == 0)
                        clusters = (ulong)(fakeBpb.spc == 0 ? fakeBpb.big_sectors - reservedSectors
                                               : (fakeBpb.big_sectors - reservedSectors) / fakeBpb.spc);
                    else
                        clusters = (ulong)(fakeBpb.spc == 0 ? fakeBpb.sectors - reservedSectors
                                               : (fakeBpb.sectors - reservedSectors) / fakeBpb.spc);
                }
                else
                    clusters = humanBpb.clusters == 0 ? humanBpb.big_clusters : humanBpb.clusters;

                // This will walk all the FAT entries and check if they're valid FAT12 or FAT16 entries.
                // If the whole table is valid in both senses, it considers the type entry in the BPB.
                // BeOS is known to set the type as FAT16 but treat it as FAT12.
                if(!isFat12 &&
                   !isFat16)
                {
                    if(clusters < 4089)
                    {
                        ushort[] fat12 = new ushort[clusters];

                        _reservedSectors     = fakeBpb.rsectors;
                        sectorsPerRealSector = fakeBpb.bps / imagePlugin.Info.SectorSize;
                        _fatFirstSector      = partition.Start + (_reservedSectors * sectorsPerRealSector);

                        byte[] fatBytes = imagePlugin.ReadSectors(_fatFirstSector, fakeBpb.spfat);

                        int pos = 0;

                        for(int i = 0; i + 3 < fatBytes.Length && pos < fat12.Length; i += 3)
                        {
                            fat12[pos++] = (ushort)(((fatBytes[i + 1] & 0xF) << 8) + fatBytes[i + 0]);

                            if(pos >= fat12.Length)
                                break;

                            fat12[pos++] = (ushort)(((fatBytes[i + 1] & 0xF0) >> 4) + (fatBytes[i + 2] << 4));
                        }

                        bool fat12Valid = fat12[0] >= FAT12_RESERVED && fat12[1] >= FAT12_RESERVED;

                        foreach(ushort entry in fat12)
                        {
                            if(entry >= FAT12_RESERVED ||
                               entry <= clusters)
                                continue;

                            fat12Valid = false;

                            break;
                        }

                        ushort[] fat16 = MemoryMarshal.Cast<byte, ushort>(fatBytes).ToArray();

                        bool fat16Valid = fat16[0] >= FAT16_RESERVED && fat16[1] >= 0x3FF0;

                        foreach(ushort entry in fat16)
                        {
                            if(entry >= FAT16_RESERVED ||
                               entry <= clusters)
                                continue;

                            fat16Valid = false;

                            break;
                        }

                        isFat12 = fat12Valid;
                        isFat16 = fat16Valid;

                        // Check BPB type
                        if(isFat12 == isFat16)
                        {
                            isFat12 = fakeBpb.fs_type                           != null &&
                                      Encoding.ASCII.GetString(fakeBpb.fs_type) == "FAT12   ";

                            isFat16 = fakeBpb.fs_type                           != null &&
                                      Encoding.ASCII.GetString(fakeBpb.fs_type) == "FAT16   ";
                        }

                        if(!isFat12 &&
                           !isFat16)
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
                            sb.AppendLine("Atari FAT12");

                            break;
                        case BpbKind.Apricot:
                            sb.AppendLine("Apricot FAT12");

                            break;
                        case BpbKind.Human:
                            sb.AppendLine("Human68k FAT12");

                            break;
                        default:
                            sb.AppendLine("Microsoft FAT12");

                            break;
                    }

                    XmlFsType.Type = "FAT12";
                }
                else if(isFat16)
                {
                    sb.AppendLine(bpbKind == BpbKind.Atari
                                      ? "Atari FAT16"
                                      : bpbKind == BpbKind.Human
                                          ? "Human68k FAT16"
                                          : "Microsoft FAT16");

                    XmlFsType.Type = "FAT16";
                }

                if(bpbKind == BpbKind.Atari)
                {
                    if(atariBpb.serial_no[0] == 0x49 &&
                       atariBpb.serial_no[1] == 0x48 &&
                       atariBpb.serial_no[2] == 0x43)
                        sb.AppendLine("Volume has been modified by Windows 9x/Me Volume Tracker.");
                    else
                        XmlFsType.VolumeSerial =
                            $"{atariBpb.serial_no[0]:X2}{atariBpb.serial_no[1]:X2}{atariBpb.serial_no[2]:X2}";

                    XmlFsType.SystemIdentifier = StringHandlers.CToString(atariBpb.oem_name);

                    if(string.IsNullOrEmpty(XmlFsType.SystemIdentifier))
                        XmlFsType.SystemIdentifier = null;
                }
                else if(fakeBpb.oem_name != null)
                {
                    if(fakeBpb.oem_name[5] == 0x49 &&
                       fakeBpb.oem_name[6] == 0x48 &&
                       fakeBpb.oem_name[7] == 0x43)
                        sb.AppendLine("Volume has been modified by Windows 9x/Me Volume Tracker.");
                    else
                    {
                        // Later versions of Windows create a DOS 3 BPB without OEM name on 8 sectors/track floppies
                        // OEM ID should be ASCII, otherwise ignore it
                        if(fakeBpb.oem_name[0] >= 0x20 &&
                           fakeBpb.oem_name[0] <= 0x7F &&
                           fakeBpb.oem_name[1] >= 0x20 &&
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
                           fakeBpb.oem_name[7] <= 0x7F)
                            XmlFsType.SystemIdentifier = StringHandlers.CToString(fakeBpb.oem_name);
                        else if(fakeBpb.oem_name[0] < 0x20  &&
                                fakeBpb.oem_name[1] >= 0x20 &&
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
                                fakeBpb.oem_name[7] <= 0x7F)
                            XmlFsType.SystemIdentifier = StringHandlers.CToString(fakeBpb.oem_name, Encoding, start: 1);
                    }

                    if(fakeBpb.signature == 0x28 ||
                       fakeBpb.signature == 0x29)
                        XmlFsType.VolumeSerial = $"{fakeBpb.serial_no:X8}";
                }

                if(XmlFsType.SystemIdentifier != null)
                    sb.AppendFormat("OEM Name: {0}", XmlFsType.SystemIdentifier.Trim()).AppendLine();

                sb.AppendFormat("{0} bytes per sector.", fakeBpb.bps).AppendLine();

                if(bpbKind != BpbKind.Human)
                    if(fakeBpb.sectors == 0)
                        sb.AppendFormat("{0} sectors on volume ({1} bytes).", fakeBpb.big_sectors,
                                        fakeBpb.big_sectors * fakeBpb.bps).AppendLine();
                    else
                        sb.AppendFormat("{0} sectors on volume ({1} bytes).", fakeBpb.sectors,
                                        fakeBpb.sectors * fakeBpb.bps).AppendLine();
                else
                    sb.AppendFormat("{0} sectors on volume ({1} bytes).",
                                    clusters * humanBpb.bpc / imagePlugin.Info.SectorSize, clusters * humanBpb.bpc).
                       AppendLine();

                XmlFsType.Clusters = clusters;
                sb.AppendFormat("{0} sectors per cluster.", fakeBpb.spc).AppendLine();
                sb.AppendFormat("{0} clusters on volume.", XmlFsType.Clusters).AppendLine();
                XmlFsType.ClusterSize = (uint)(fakeBpb.bps * fakeBpb.spc);
                sb.AppendFormat("{0} sectors reserved between BPB and FAT.", fakeBpb.rsectors).AppendLine();
                sb.AppendFormat("{0} FATs.", fakeBpb.fats_no).AppendLine();
                sb.AppendFormat("{0} entries on root directory.", fakeBpb.root_ent).AppendLine();

                if(fakeBpb.media > 0)
                    sb.AppendFormat("Media descriptor: 0x{0:X2}", fakeBpb.media).AppendLine();

                sb.AppendFormat("{0} sectors per FAT.", fakeBpb.spfat).AppendLine();

                if(fakeBpb.sptrk > 0  &&
                   fakeBpb.sptrk < 64 &&
                   fakeBpb.heads > 0  &&
                   fakeBpb.heads < 256)
                {
                    sb.AppendFormat("{0} sectors per track.", fakeBpb.sptrk).AppendLine();
                    sb.AppendFormat("{0} heads.", fakeBpb.heads).AppendLine();
                }

                if(fakeBpb.hsectors <= partition.Start)
                    sb.AppendFormat("{0} hidden sectors before BPB.", fakeBpb.hsectors).AppendLine();

                if(fakeBpb.signature == 0x28 ||
                   fakeBpb.signature == 0x29 ||
                   andosOemCorrect)
                {
                    sb.AppendFormat("Drive number: 0x{0:X2}", fakeBpb.drive_no).AppendLine();

                    if(XmlFsType.VolumeSerial != null)
                        sb.AppendFormat("Volume Serial Number: {0}", XmlFsType.VolumeSerial).AppendLine();

                    if((fakeBpb.flags & 0xF8) == 0x00)
                    {
                        if((fakeBpb.flags & 0x01) == 0x01)
                        {
                            sb.AppendLine("Volume should be checked on next mount.");
                            XmlFsType.Dirty = true;
                        }

                        if((fakeBpb.flags & 0x02) == 0x02)
                            sb.AppendLine("Disk surface should be on next mount.");
                    }

                    if(fakeBpb.signature == 0x29 || andosOemCorrect)
                    {
                        XmlFsType.VolumeName = StringHandlers.SpacePaddedToString(fakeBpb.volume_label, Encoding);
                        XmlFsType.VolumeName = XmlFsType.VolumeName?.Replace("\0", "");
                        sb.AppendFormat("Filesystem type: {0}", Encoding.ASCII.GetString(fakeBpb.fs_type)).AppendLine();
                    }
                }
                else if(bpbKind                == BpbKind.Atari &&
                        XmlFsType.VolumeSerial != null)
                    sb.AppendFormat("Volume Serial Number: {0}", XmlFsType.VolumeSerial).AppendLine();

                bootChk = Sha1Context.Data(fakeBpb.boot_code, out _);

                // Workaround that PCExchange jumps into "FAT16   "...
                if(XmlFsType.SystemIdentifier == "PCX 2.0 ")
                    fakeBpb.jump[1] += 8;

                // Check that jumps to a correct boot code position and has boot signature set.
                // This will mean that the volume will boot, even if just to say "this is not bootable change disk"......
                if(XmlFsType.Bootable == false &&
                   fakeBpb.jump       != null)
                    XmlFsType.Bootable |=
                        (fakeBpb.jump[0] == 0xEB && fakeBpb.jump[1] >= minBootNearJump && fakeBpb.jump[1] < 0x80) ||
                        (fakeBpb.jump[0]                        == 0xE9            && fakeBpb.jump.Length >= 3 &&
                         BitConverter.ToUInt16(fakeBpb.jump, 1) >= minBootNearJump &&
                         BitConverter.ToUInt16(fakeBpb.jump, 1) <= 0x1FC);

                sectorsPerRealSector = fakeBpb.bps / imagePlugin.Info.SectorSize;

                // First root directory sector
                rootDirectorySector =
                    (ulong)((fakeBpb.spfat * fakeBpb.fats_no) + fakeBpb.rsectors) * sectorsPerRealSector;

                sectorsForRootDirectory = (uint)(fakeBpb.root_ent * 32 / imagePlugin.Info.SectorSize);
            }

            if(extraInfo != null)
                sb.Append(extraInfo);

            if(rootDirectorySector + partition.Start < partition.End &&
               imagePlugin.Info.XmlMediaType         != XmlMediaType.OpticalDisc)
            {
                byte[] rootDirectory =
                    imagePlugin.ReadSectors(rootDirectorySector + partition.Start, sectorsForRootDirectory);

                if(bpbKind == BpbKind.DecRainbow)
                {
                    var rootMs = new MemoryStream();

                    foreach(byte[] tmp in from ulong rootSector in new[]
                    {
                        0x17, 0x19, 0x1B, 0x1D, 0x1E, 0x20
                    } select imagePlugin.ReadSector(rootSector))
                        rootMs.Write(tmp, 0, tmp.Length);

                    rootDirectory = rootMs.ToArray();
                }

                for(int i = 0; i < rootDirectory.Length; i += 32)
                {
                    // Not a correct entry
                    if(rootDirectory[i] < DIRENT_MIN &&
                       rootDirectory[i] != DIRENT_E5)
                        continue;

                    // Deleted or subdirectory entry
                    if(rootDirectory[i] == DIRENT_SUBDIR ||
                       rootDirectory[i] == DIRENT_DELETED)
                        continue;

                    // Not a volume label
                    if(rootDirectory[i + 0x0B] != 0x08 &&
                       rootDirectory[i + 0x0B] != 0x28)
                        continue;

                    DirectoryEntry entry =
                        Marshal.ByteArrayToStructureLittleEndian<DirectoryEntry>(rootDirectory, i, 32);

                    byte[] fullname = new byte[11];
                    Array.Copy(entry.filename, 0, fullname, 0, 8);
                    Array.Copy(entry.extension, 0, fullname, 8, 3);
                    string volname = Encoding.GetString(fullname).Trim();

                    if(!string.IsNullOrEmpty(volname))
                        XmlFsType.VolumeName =
                            entry.caseinfo.HasFlag(CaseInfo.AllLowerCase) ? volname.ToLower() : volname;

                    if(entry.ctime > 0 &&
                       entry.cdate > 0)
                    {
                        XmlFsType.CreationDate = DateHandlers.DosToDateTime(entry.cdate, entry.ctime);

                        if(entry.ctime_ms > 0)
                            XmlFsType.CreationDate = XmlFsType.CreationDate.AddMilliseconds(entry.ctime_ms * 10);

                        XmlFsType.CreationDateSpecified = true;
                        sb.AppendFormat("Volume created on {0}", XmlFsType.CreationDate).AppendLine();
                    }

                    if(entry.mtime > 0 &&
                       entry.mdate > 0)
                    {
                        XmlFsType.ModificationDate          = DateHandlers.DosToDateTime(entry.mdate, entry.mtime);
                        XmlFsType.ModificationDateSpecified = true;
                        sb.AppendFormat("Volume last modified on {0}", XmlFsType.ModificationDate).AppendLine();
                    }

                    if(entry.adate > 0)
                        sb.AppendFormat("Volume last accessed on {0:d}", DateHandlers.DosToDateTime(entry.adate, 0)).
                           AppendLine();

                    break;
                }
            }

            if(!string.IsNullOrEmpty(XmlFsType.VolumeName))
                sb.AppendFormat("Volume label: {0}", XmlFsType.VolumeName).AppendLine();

            if(XmlFsType.Bootable)
            {
                // Intel short jump
                if(bpbSector[0] == 0xEB &&
                   bpbSector[1] < 0x80)
                {
                    int    sigSize  = bpbSector[510] == 0x55 && bpbSector[511] == 0xAA ? 2 : 0;
                    byte[] bootCode = new byte[512 - sigSize - bpbSector[1] - 2];
                    Array.Copy(bpbSector, bpbSector[1] + 2, bootCode, 0, bootCode.Length);
                    Sha1Context.Data(bootCode, out _);
                }

                // Intel big jump
                else if(bpbSector[0]                        == 0xE9 &&
                        BitConverter.ToUInt16(bpbSector, 1) < 0x1FC)
                {
                    int    sigSize  = bpbSector[510] == 0x55 && bpbSector[511] == 0xAA ? 2 : 0;
                    byte[] bootCode = new byte[512 - sigSize - BitConverter.ToUInt16(bpbSector, 1) - 3];
                    Array.Copy(bpbSector, BitConverter.ToUInt16(bpbSector, 1) + 3, bootCode, 0, bootCode.Length);
                    Sha1Context.Data(bootCode, out _);
                }

                sb.AppendLine("Volume is bootable");
                sb.AppendFormat("Boot code's SHA1: {0}", bootChk).AppendLine();
                string bootName = _knownBootHashes.FirstOrDefault(t => t.hash == bootChk).name;

                if(string.IsNullOrWhiteSpace(bootName))
                    sb.AppendLine("Unknown boot code.");
                else
                    sb.AppendFormat("Boot code corresponds to {0}", bootName).AppendLine();
            }

            information = sb.ToString();
        }
    }
}