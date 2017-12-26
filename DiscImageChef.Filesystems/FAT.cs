// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : FAT.cs
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.Checksums;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.DiscImages;
using DiscImageChef.Helpers;
using Schemas;

namespace DiscImageChef.Filesystems
{
    // TODO: Differentiate between Atari and X68k FAT, as this one uses a standard BPB.
    // X68K uses cdate/adate from direntry for extending filename
    public class FAT : Filesystem
    {
        const uint FSINFO_SIGNATURE1 = 0x41615252;
        const uint FSINFO_SIGNATURE2 = 0x61417272;
        const uint FSINFO_SIGNATURE3 = 0xAA550000;

        public FAT()
        {
            Name = "Microsoft File Allocation Table";
            PluginUuid = new Guid("33513B2C-0D26-0D2D-32C3-79D8611158E0");
            CurrentEncoding = Encoding.GetEncoding("IBM437");
        }

        public FAT(Encoding encoding)
        {
            Name = "Microsoft File Allocation Table";
            PluginUuid = new Guid("33513B2C-0D26-0D2D-32C3-79D8611158E0");
            CurrentEncoding = encoding ?? Encoding.GetEncoding("IBM437");
        }

        public FAT(ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "Microsoft File Allocation Table";
            PluginUuid = new Guid("33513B2C-0D26-0D2D-32C3-79D8611158E0");
            CurrentEncoding = encoding ?? Encoding.GetEncoding("IBM437");
        }

        public override bool Identify(ImagePlugin imagePlugin, Partition partition)
        {
            if(2 + partition.Start >= partition.End) return false;

            ushort bps;
            byte spc;
            byte numberOfFats;
            ushort reservedSecs;
            ushort rootEntries;
            ushort sectors;
            byte mediaDescriptor;
            ushort fatSectors;
            uint bigSectors;
            byte bpbSignature;
            byte fat32Signature;
            ulong hugeSectors;
            byte[] fat32Id = new byte[8];
            byte[] msxId = new byte[6];
            byte fatId;
            byte[] dosOem = new byte[8];
            byte[] atariOem = new byte[6];
            ushort bootable = 0;

            byte[] bpbSector = imagePlugin.ReadSector(0 + partition.Start);
            byte[] fatSector = imagePlugin.ReadSector(1 + partition.Start);

            Array.Copy(bpbSector, 0x02, atariOem, 0, 6);
            Array.Copy(bpbSector, 0x03, dosOem, 0, 8);
            bps = BitConverter.ToUInt16(bpbSector, 0x00B);
            spc = bpbSector[0x00D];
            reservedSecs = BitConverter.ToUInt16(bpbSector, 0x00E);
            numberOfFats = bpbSector[0x010];
            rootEntries = BitConverter.ToUInt16(bpbSector, 0x011);
            sectors = BitConverter.ToUInt16(bpbSector, 0x013);
            mediaDescriptor = bpbSector[0x015];
            fatSectors = BitConverter.ToUInt16(bpbSector, 0x016);
            Array.Copy(bpbSector, 0x052, msxId, 0, 6);
            bigSectors = BitConverter.ToUInt32(bpbSector, 0x020);
            bpbSignature = bpbSector[0x026];
            fat32Signature = bpbSector[0x042];
            Array.Copy(bpbSector, 0x052, fat32Id, 0, 8);
            hugeSectors = BitConverter.ToUInt64(bpbSector, 0x052);
            fatId = fatSector[0];
            int bitsInBps = CountBits.Count(bps);
            if(imagePlugin.ImageInfo.SectorSize >= 512) bootable = BitConverter.ToUInt16(bpbSector, 0x1FE);

            bool correctSpc = spc == 1 || spc == 2 || spc == 4 || spc == 8 || spc == 16 || spc == 32 || spc == 64;
            string msxString = Encoding.ASCII.GetString(msxId);
            string fat32String = Encoding.ASCII.GetString(fat32Id);
            bool atariOemCorrect = atariOem[0] >= 0x20 && atariOem[1] >= 0x20 && atariOem[2] >= 0x20 &&
                                   atariOem[3] >= 0x20 && atariOem[4] >= 0x20 && atariOem[5] >= 0x20;
            bool dosOemCorrect = dosOem[0] >= 0x20 && dosOem[1] >= 0x20 && dosOem[2] >= 0x20 && dosOem[3] >= 0x20 &&
                                 dosOem[4] >= 0x20 && dosOem[5] >= 0x20 && dosOem[6] >= 0x20 && dosOem[7] >= 0x20;
            string atariString = Encoding.ASCII.GetString(atariOem);
            string oemString = Encoding.ASCII.GetString(dosOem);

            DicConsole.DebugWriteLine("FAT plugin", "atari_oem_correct = {0}", atariOemCorrect);
            DicConsole.DebugWriteLine("FAT plugin", "dos_oem_correct = {0}", dosOemCorrect);
            DicConsole.DebugWriteLine("FAT plugin", "bps = {0}", bps);
            DicConsole.DebugWriteLine("FAT plugin", "bits in bps = {0}", bitsInBps);
            DicConsole.DebugWriteLine("FAT plugin", "spc = {0}", spc);
            DicConsole.DebugWriteLine("FAT plugin", "correct_spc = {0}", correctSpc);
            DicConsole.DebugWriteLine("FAT plugin", "reserved_secs = {0}", reservedSecs);
            DicConsole.DebugWriteLine("FAT plugin", "fats_no = {0}", numberOfFats);
            DicConsole.DebugWriteLine("FAT plugin", "root_entries = {0}", rootEntries);
            DicConsole.DebugWriteLine("FAT plugin", "sectors = {0}", sectors);
            DicConsole.DebugWriteLine("FAT plugin", "media_descriptor = 0x{0:X2}", mediaDescriptor);
            DicConsole.DebugWriteLine("FAT plugin", "fat_sectors = {0}", fatSectors);
            DicConsole.DebugWriteLine("FAT plugin", "msx_id = \"{0}\"", msxString);
            DicConsole.DebugWriteLine("FAT plugin", "big_sectors = {0}", bigSectors);
            DicConsole.DebugWriteLine("FAT plugin", "bpb_signature = 0x{0:X2}", bpbSignature);
            DicConsole.DebugWriteLine("FAT plugin", "fat32_signature = 0x{0:X2}", fat32Signature);
            DicConsole.DebugWriteLine("FAT plugin", "fat32_id = \"{0}\"", fat32String);
            DicConsole.DebugWriteLine("FAT plugin", "huge_sectors = {0}", hugeSectors);
            DicConsole.DebugWriteLine("FAT plugin", "fat_id = 0x{0:X2}", fatId);

            ushort apricotBps = BitConverter.ToUInt16(bpbSector, 0x50);
            byte apricotSpc = bpbSector[0x52];
            ushort apricotReservedSecs = BitConverter.ToUInt16(bpbSector, 0x53);
            byte apricotFatsNo = bpbSector[0x55];
            ushort apricotRootEntries = BitConverter.ToUInt16(bpbSector, 0x56);
            ushort apricotSectors = BitConverter.ToUInt16(bpbSector, 0x58);
            byte apricotMediaDescriptor = bpbSector[0x5A];
            ushort apricotFatSectors = BitConverter.ToUInt16(bpbSector, 0x5B);
            bool apricotCorrectSpc = apricotSpc == 1 || apricotSpc == 2 || apricotSpc == 4 || apricotSpc == 8 ||
                                     apricotSpc == 16 || apricotSpc == 32 || apricotSpc == 64;
            int bitsInApricotBps = CountBits.Count(apricotBps);
            byte apricotPartitions = bpbSector[0x0C];

            DicConsole.DebugWriteLine("FAT plugin", "apricot_bps = {0}", apricotBps);
            DicConsole.DebugWriteLine("FAT plugin", "apricot_spc = {0}", apricotSpc);
            DicConsole.DebugWriteLine("FAT plugin", "apricot_correct_spc = {0}", apricotCorrectSpc);
            DicConsole.DebugWriteLine("FAT plugin", "apricot_reserved_secs = {0}", apricotReservedSecs);
            DicConsole.DebugWriteLine("FAT plugin", "apricot_fats_no = {0}", apricotFatsNo);
            DicConsole.DebugWriteLine("FAT plugin", "apricot_root_entries = {0}", apricotRootEntries);
            DicConsole.DebugWriteLine("FAT plugin", "apricot_sectors = {0}", apricotSectors);
            DicConsole.DebugWriteLine("FAT plugin", "apricot_media_descriptor = 0x{0:X2}", apricotMediaDescriptor);
            DicConsole.DebugWriteLine("FAT plugin", "apricot_fat_sectors = {0}", apricotFatSectors);

            // This is to support FAT partitions on hybrid ISO/USB images
            if(imagePlugin.ImageInfo.XmlMediaType == XmlMediaType.OpticalDisc)
            {
                sectors /= 4;
                bigSectors /= 4;
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
                uint hpfsMagic1, hpfsMagic2;

                byte[] hpfsSbSector =
                    imagePlugin.ReadSector(16 + partition.Start); // Seek to superblock, on logical sector 16
                hpfsMagic1 = BitConverter.ToUInt32(hpfsSbSector, 0x000);
                hpfsMagic2 = BitConverter.ToUInt32(hpfsSbSector, 0x004);

                if(hpfsMagic1 == 0xF995E849 && hpfsMagic2 == 0xFA53E9C5) return false;
            }

            switch(bitsInBps)
            {
                // FAT32 for sure
                case 1 when correctSpc && numberOfFats <= 2 && sectors == 0 && fatSectors == 0 &&
                            fat32Signature == 0x29 && fat32String == "FAT32   ": return true;
                // short FAT32
                case 1 when correctSpc && numberOfFats <= 2 && sectors == 0 && fatSectors == 0 && fat32Signature == 0x28
                :
                    return bigSectors == 0
                               ? hugeSectors <= partition.End - partition.Start + 1
                               : bigSectors <= partition.End - partition.Start + 1;
                // MSX-DOS FAT12
                case 1 when correctSpc && numberOfFats <= 2 && rootEntries > 0 &&
                            sectors <= partition.End - partition.Start + 1 && fatSectors > 0 &&
                            msxString == "VOL_ID": return true;
                // EBPB
                case 1 when correctSpc && numberOfFats <= 2 && rootEntries > 0 && fatSectors > 0 &&
                            (bpbSignature == 0x28 || bpbSignature == 0x29):
                    return sectors == 0
                               ? bigSectors <= partition.End - partition.Start + 1
                               : sectors <= partition.End - partition.Start + 1;
                // BPB
                case 1 when correctSpc && reservedSecs < partition.End - partition.Start && numberOfFats <= 2 &&
                            rootEntries > 0 && fatSectors > 0:
                    return sectors == 0
                               ? bigSectors <= partition.End - partition.Start + 1
                               : sectors <= partition.End - partition.Start + 1;
            }

            // Apricot BPB
            if(bitsInApricotBps == 1 && apricotCorrectSpc && apricotReservedSecs < partition.End - partition.Start &&
               apricotFatsNo <= 2 && apricotRootEntries > 0 && apricotFatSectors > 0 &&
               apricotSectors <= partition.End - partition.Start + 1 && apricotPartitions == 0) return true;

            // All FAT12 without BPB can only be used on floppies, without partitions.
            if(partition.Start != 0) return false;

            // DEC Rainbow, lacks a BPB but has a very concrete structure...
            if(imagePlugin.ImageInfo.Sectors == 800 && imagePlugin.ImageInfo.SectorSize == 512)
            {
                // DEC Rainbow boots up with a Z80, first byte should be DI (disable interrupts)
                byte z80Di = bpbSector[0];
                // First FAT1 sector resides at LBA 0x14
                byte[] fat1Sector0 = imagePlugin.ReadSector(0x14);
                // First FAT2 sector resides at LBA 0x1A
                byte[] fat2Sector0 = imagePlugin.ReadSector(0x1A);
                bool equalFatIds = fat1Sector0[0] == fat2Sector0[0] && fat1Sector0[1] == fat2Sector0[1];
                // Volume is software interleaved 2:1
                MemoryStream rootMs = new MemoryStream();
                foreach(byte[] tmp in from ulong rootSector in new[] {0x17, 0x19, 0x1B, 0x1D, 0x1E, 0x20}
                                      select imagePlugin.ReadSector(rootSector)) rootMs.Write(tmp, 0, tmp.Length);

                byte[] rootDir = rootMs.ToArray();
                bool validRootDir = true;

                // Iterate all root directory
                for(int e = 0; e < 96 * 32; e += 32)
                {
                    for(int c = 0; c < 11; c++)
                        if(rootDir[c + e] < 0x20 && rootDir[c + e] != 0x00 && rootDir[c + e] != 0x05 ||
                           rootDir[c + e] == 0xFF || rootDir[c + e] == 0x2E)
                        {
                            validRootDir = false;
                            break;
                        }

                    if(!validRootDir) break;
                }

                if(z80Di == 0xF3 && equalFatIds && (fat1Sector0[0] & 0xF0) == 0xF0 && fat1Sector0[1] == 0xFF &&
                   validRootDir) return true;
            }

            byte fat2 = fatSector[1];
            byte fat3 = fatSector[2];
            ushort fat2ndCluster = (ushort)(((fat2 << 8) + fat3) & 0xFFF);

            DicConsole.DebugWriteLine("FAT plugin", "1st fat cluster 1 = {0:X3}", fat2ndCluster);
            if(fat2ndCluster < 0xFF0) return false;

            ulong fat2SectorNo = 0;

            switch(fatId)
            {
                case 0xE5:
                    if(imagePlugin.ImageInfo.Sectors == 2002 && imagePlugin.ImageInfo.SectorSize == 128)
                        fat2SectorNo = 2;
                    break;
                case 0xFD:
                    if(imagePlugin.ImageInfo.Sectors == 4004 && imagePlugin.ImageInfo.SectorSize == 128)
                        fat2SectorNo = 7;
                    else if(imagePlugin.ImageInfo.Sectors == 2002 && imagePlugin.ImageInfo.SectorSize == 128)
                        fat2SectorNo = 7;
                    break;
                case 0xFE:
                    if(imagePlugin.ImageInfo.Sectors == 320 && imagePlugin.ImageInfo.SectorSize == 512)
                        fat2SectorNo = 2;
                    else if(imagePlugin.ImageInfo.Sectors == 2002 && imagePlugin.ImageInfo.SectorSize == 128)
                        fat2SectorNo = 7;
                    else if(imagePlugin.ImageInfo.Sectors == 1232 && imagePlugin.ImageInfo.SectorSize == 1024)
                        fat2SectorNo = 3;
                    else if(imagePlugin.ImageInfo.Sectors == 616 && imagePlugin.ImageInfo.SectorSize == 1024)
                        fat2SectorNo = 2;
                    else if(imagePlugin.ImageInfo.Sectors == 720 && imagePlugin.ImageInfo.SectorSize == 128)
                        fat2SectorNo = 5;
                    else if(imagePlugin.ImageInfo.Sectors == 640 && imagePlugin.ImageInfo.SectorSize == 512)
                        fat2SectorNo = 2;
                    break;
                case 0xFF:
                    if(imagePlugin.ImageInfo.Sectors == 640 && imagePlugin.ImageInfo.SectorSize == 512)
                        fat2SectorNo = 2;
                    break;
                default:
                    if(fatId < 0xE8) return false;

                    fat2SectorNo = 2;
                    break;
            }

            if(fat2SectorNo > partition.End) return false;

            DicConsole.DebugWriteLine("FAT plugin", "2nd fat starts at = {0}", fat2SectorNo);

            byte[] fat2Sector = imagePlugin.ReadSector(fat2SectorNo);

            fat2 = fat2Sector[1];
            fat3 = fat2Sector[2];
            fat2ndCluster = (ushort)(((fat2 << 8) + fat3) & 0xFFF);
            if(fat2ndCluster < 0xFF0) return false;

            return fatId == fat2Sector[0];
        }

        public override void GetInformation(ImagePlugin imagePlugin, Partition partition, out string information)
        {
            information = "";

            StringBuilder sb = new StringBuilder();
            XmlFsType = new FileSystemType();

            bool useAtariBpb = false;
            bool useMsxBpb = false;
            bool useDos2Bpb = false;
            bool useDos3Bpb = false;
            bool useDos32Bpb = false;
            bool useDos33Bpb = false;
            bool userShortExtendedBpb = false;
            bool useExtendedBpb = false;
            bool useShortFat32 = false;
            bool useLongFat32 = false;
            bool andosOemCorrect = false;
            bool useApricotBpb = false;
            bool useDecRainbowBpb = false;

            AtariParameterBlock atariBpb = new AtariParameterBlock();
            MsxParameterBlock msxBpb = new MsxParameterBlock();
            BiosParameterBlock2 dos2Bpb = new BiosParameterBlock2();
            BiosParameterBlock30 dos30Bpb = new BiosParameterBlock30();
            BiosParameterBlock32 dos32Bpb = new BiosParameterBlock32();
            BiosParameterBlock33 dos33Bpb = new BiosParameterBlock33();
            BiosParameterBlockShortEbpb shortEbpb = new BiosParameterBlockShortEbpb();
            BiosParameterBlockEbpb ebpb = new BiosParameterBlockEbpb();
            Fat32ParameterBlockShort shortFat32Bpb = new Fat32ParameterBlockShort();
            Fat32ParameterBlock fat32Bpb = new Fat32ParameterBlock();
            ApricotLabel apricotBpb = new ApricotLabel();

            byte[] bpbSector = imagePlugin.ReadSectors(partition.Start, 2);

            if(imagePlugin.ImageInfo.SectorSize >= 256)
            {
                IntPtr bpbPtr = Marshal.AllocHGlobal(512);
                Marshal.Copy(bpbSector, 0, bpbPtr, 512);

                atariBpb = (AtariParameterBlock)Marshal.PtrToStructure(bpbPtr, typeof(AtariParameterBlock));
                msxBpb = (MsxParameterBlock)Marshal.PtrToStructure(bpbPtr, typeof(MsxParameterBlock));
                dos2Bpb = (BiosParameterBlock2)Marshal.PtrToStructure(bpbPtr, typeof(BiosParameterBlock2));
                dos30Bpb = (BiosParameterBlock30)Marshal.PtrToStructure(bpbPtr, typeof(BiosParameterBlock30));
                dos32Bpb = (BiosParameterBlock32)Marshal.PtrToStructure(bpbPtr, typeof(BiosParameterBlock32));
                dos33Bpb = (BiosParameterBlock33)Marshal.PtrToStructure(bpbPtr, typeof(BiosParameterBlock33));
                shortEbpb =
                    (BiosParameterBlockShortEbpb)Marshal.PtrToStructure(bpbPtr, typeof(BiosParameterBlockShortEbpb));
                ebpb = (BiosParameterBlockEbpb)Marshal.PtrToStructure(bpbPtr, typeof(BiosParameterBlockEbpb));
                shortFat32Bpb =
                    (Fat32ParameterBlockShort)Marshal.PtrToStructure(bpbPtr, typeof(Fat32ParameterBlockShort));
                fat32Bpb = (Fat32ParameterBlock)Marshal.PtrToStructure(bpbPtr, typeof(Fat32ParameterBlock));
                apricotBpb = (ApricotLabel)Marshal.PtrToStructure(bpbPtr, typeof(ApricotLabel));

                Marshal.FreeHGlobal(bpbPtr);

                int bitsInBpsAtari = CountBits.Count(atariBpb.bps);
                int bitsInBpsMsx = CountBits.Count(msxBpb.bps);
                int bitsInBpsDos20 = CountBits.Count(dos2Bpb.bps);
                int bitsInBpsDos30 = CountBits.Count(dos30Bpb.bps);
                int bitsInBpsDos32 = CountBits.Count(dos32Bpb.bps);
                int bitsInBpsDos33 = CountBits.Count(dos33Bpb.bps);
                int bitsInBpsDos34 = CountBits.Count(shortEbpb.bps);
                int bitsInBpsDos40 = CountBits.Count(ebpb.bps);
                int bitsInBpsFat32Short = CountBits.Count(shortFat32Bpb.bps);
                int bitsInBpsFat32 = CountBits.Count(fat32Bpb.bps);
                int bitsInBpsApricot = CountBits.Count(apricotBpb.mainBPB.bps);

                bool correctSpcAtari = atariBpb.spc == 1 || atariBpb.spc == 2 || atariBpb.spc == 4 ||
                                       atariBpb.spc == 8 || atariBpb.spc == 16 || atariBpb.spc == 32 ||
                                       atariBpb.spc == 64;
                bool correctSpcMsx = msxBpb.spc == 1 || msxBpb.spc == 2 || msxBpb.spc == 4 || msxBpb.spc == 8 ||
                                     msxBpb.spc == 16 || msxBpb.spc == 32 || msxBpb.spc == 64;
                bool correctSpcDos20 = dos2Bpb.spc == 1 || dos2Bpb.spc == 2 || dos2Bpb.spc == 4 || dos2Bpb.spc == 8 ||
                                       dos2Bpb.spc == 16 || dos2Bpb.spc == 32 || dos2Bpb.spc == 64;
                bool correctSpcDos30 = dos30Bpb.spc == 1 || dos30Bpb.spc == 2 || dos30Bpb.spc == 4 ||
                                       dos30Bpb.spc == 8 || dos30Bpb.spc == 16 || dos30Bpb.spc == 32 ||
                                       dos30Bpb.spc == 64;
                bool correctSpcDos32 = dos32Bpb.spc == 1 || dos32Bpb.spc == 2 || dos32Bpb.spc == 4 ||
                                       dos32Bpb.spc == 8 || dos32Bpb.spc == 16 || dos32Bpb.spc == 32 ||
                                       dos32Bpb.spc == 64;
                bool correctSpcDos33 = dos33Bpb.spc == 1 || dos33Bpb.spc == 2 || dos33Bpb.spc == 4 ||
                                       dos33Bpb.spc == 8 || dos33Bpb.spc == 16 || dos33Bpb.spc == 32 ||
                                       dos33Bpb.spc == 64;
                bool correctSpcDos34 = shortEbpb.spc == 1 || shortEbpb.spc == 2 || shortEbpb.spc == 4 ||
                                       shortEbpb.spc == 8 || shortEbpb.spc == 16 || shortEbpb.spc == 32 ||
                                       shortEbpb.spc == 64;
                bool correctSpcDos40 = ebpb.spc == 1 || ebpb.spc == 2 || ebpb.spc == 4 || ebpb.spc == 8 ||
                                       ebpb.spc == 16 || ebpb.spc == 32 || ebpb.spc == 64;
                bool correctSpcFat32Short = shortFat32Bpb.spc == 1 || shortFat32Bpb.spc == 2 ||
                                            shortFat32Bpb.spc == 4 || shortFat32Bpb.spc == 8 ||
                                            shortFat32Bpb.spc == 16 || shortFat32Bpb.spc == 32 ||
                                            shortFat32Bpb.spc == 64;
                bool correctSpcFat32 = fat32Bpb.spc == 1 || fat32Bpb.spc == 2 || fat32Bpb.spc == 4 ||
                                       fat32Bpb.spc == 8 || fat32Bpb.spc == 16 || fat32Bpb.spc == 32 ||
                                       fat32Bpb.spc == 64;
                bool correctSpcApricot = apricotBpb.mainBPB.spc == 1 || apricotBpb.mainBPB.spc == 2 ||
                                         apricotBpb.mainBPB.spc == 4 || apricotBpb.mainBPB.spc == 8 ||
                                         apricotBpb.mainBPB.spc == 16 || apricotBpb.mainBPB.spc == 32 ||
                                         apricotBpb.mainBPB.spc == 64;

                // This is to support FAT partitions on hybrid ISO/USB images
                if(imagePlugin.ImageInfo.XmlMediaType == XmlMediaType.OpticalDisc)
                {
                    atariBpb.sectors /= 4;
                    msxBpb.sectors /= 4;
                    dos2Bpb.sectors /= 4;
                    dos30Bpb.sectors /= 4;
                    dos32Bpb.sectors /= 4;
                    dos33Bpb.sectors /= 4;
                    dos33Bpb.big_sectors /= 4;
                    shortEbpb.sectors /= 4;
                    shortEbpb.big_sectors /= 4;
                    ebpb.sectors /= 4;
                    ebpb.big_sectors /= 4;
                    shortFat32Bpb.sectors /= 4;
                    shortFat32Bpb.big_sectors /= 4;
                    shortFat32Bpb.huge_sectors /= 4;
                    fat32Bpb.sectors /= 4;
                    fat32Bpb.big_sectors /= 4;
                    apricotBpb.mainBPB.sectors /= 4;
                }

                andosOemCorrect = dos33Bpb.oem_name[0] < 0x20 && dos33Bpb.oem_name[1] >= 0x20 &&
                                  dos33Bpb.oem_name[2] >= 0x20 && dos33Bpb.oem_name[3] >= 0x20 &&
                                  dos33Bpb.oem_name[4] >= 0x20 && dos33Bpb.oem_name[5] >= 0x20 &&
                                  dos33Bpb.oem_name[6] >= 0x20 && dos33Bpb.oem_name[7] >= 0x20;

                if(bitsInBpsFat32 == 1 && correctSpcFat32 && fat32Bpb.fats_no <= 2 && fat32Bpb.sectors == 0 &&
                   fat32Bpb.spfat == 0 && fat32Bpb.signature == 0x29 &&
                   Encoding.ASCII.GetString(fat32Bpb.fs_type) == "FAT32   ")
                {
                    DicConsole.DebugWriteLine("FAT plugin", "Using FAT32 BPB");
                    useLongFat32 = true;
                }
                else if(bitsInBpsFat32Short == 1 && correctSpcFat32Short && shortFat32Bpb.fats_no <= 2 &&
                        shortFat32Bpb.sectors == 0 && shortFat32Bpb.spfat == 0 && shortFat32Bpb.signature == 0x28)
                {
                    DicConsole.DebugWriteLine("FAT plugin", "Using short FAT32 BPB");
                    useShortFat32 = shortFat32Bpb.big_sectors == 0
                                        ? shortFat32Bpb.huge_sectors <= partition.End - partition.Start + 1
                                        : shortFat32Bpb.big_sectors <= partition.End - partition.Start + 1;
                }
                else if(bitsInBpsMsx == 1 && correctSpcMsx && msxBpb.fats_no <= 2 && msxBpb.root_ent > 0 &&
                        msxBpb.sectors <= partition.End - partition.Start + 1 && msxBpb.spfat > 0 &&
                        Encoding.ASCII.GetString(msxBpb.vol_id) == "VOL_ID")
                {
                    DicConsole.DebugWriteLine("FAT plugin", "Using MSX BPB");
                    useMsxBpb = true;
                }
                else if(bitsInBpsApricot == 1 && correctSpcApricot && apricotBpb.mainBPB.fats_no <= 2 &&
                        apricotBpb.mainBPB.root_ent > 0 &&
                        apricotBpb.mainBPB.sectors <= partition.End - partition.Start + 1 &&
                        apricotBpb.mainBPB.spfat > 0 && apricotBpb.partitionCount == 0)
                {
                    DicConsole.DebugWriteLine("FAT plugin", "Using Apricot BPB");
                    useApricotBpb = true;
                }
                else if(bitsInBpsDos40 == 1 && correctSpcDos40 && ebpb.fats_no <= 2 && ebpb.root_ent > 0 &&
                        ebpb.spfat > 0 && (ebpb.signature == 0x28 || ebpb.signature == 0x29 || andosOemCorrect))
                {
                    if(ebpb.sectors == 0)
                    {
                        if(ebpb.big_sectors <= partition.End - partition.Start + 1)
                            if(ebpb.signature == 0x29 || andosOemCorrect)
                            {
                                DicConsole.DebugWriteLine("FAT plugin", "Using DOS 4.0 BPB");
                                useExtendedBpb = true;
                            }
                            else
                            {
                                DicConsole.DebugWriteLine("FAT plugin", "Using DOS 3.4 BPB");
                                userShortExtendedBpb = true;
                            }
                    }
                    else if(ebpb.sectors <= partition.End - partition.Start + 1)
                        if(ebpb.signature == 0x29 || andosOemCorrect)
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using DOS 4.0 BPB");
                            useExtendedBpb = true;
                        }
                        else
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using DOS 3.4 BPB");
                            userShortExtendedBpb = true;
                        }
                }
                else if(bitsInBpsDos33 == 1 && correctSpcDos33 && dos33Bpb.rsectors < partition.End - partition.Start &&
                        dos33Bpb.fats_no <= 2 && dos33Bpb.root_ent > 0 && dos33Bpb.spfat > 0)
                    if(dos33Bpb.sectors == 0 && dos33Bpb.hsectors <= partition.Start && dos33Bpb.big_sectors > 0 &&
                       dos33Bpb.big_sectors <= partition.End - partition.Start + 1)
                    {
                        DicConsole.DebugWriteLine("FAT plugin", "Using DOS 3.3 BPB");
                        useDos33Bpb = true;
                    }
                    else if(dos33Bpb.big_sectors == 0 && dos33Bpb.hsectors <= partition.Start && dos33Bpb.sectors > 0 &&
                            dos33Bpb.sectors <= partition.End - partition.Start + 1)
                        if(atariBpb.jump[0] == 0x60 || atariBpb.jump[0] == 0xE9 && atariBpb.jump[1] == 0x00 &&
                           Encoding.ASCII.GetString(dos33Bpb.oem_name) != "NEXT    ")
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using Atari BPB");
                            useAtariBpb = true;
                        }
                        else
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using DOS 3.3 BPB");
                            useDos33Bpb = true;
                        }
                    else
                    {
                        if(dos32Bpb.hsectors <= partition.Start &&
                           dos32Bpb.hsectors + dos32Bpb.sectors == dos32Bpb.total_sectors)
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using DOS 3.2 BPB");
                            useDos32Bpb = true;
                        }
                        else if(dos30Bpb.sptrk > 0 && dos30Bpb.sptrk < 64 && dos30Bpb.heads > 0 && dos30Bpb.heads < 256)
                            if(atariBpb.jump[0] == 0x60 || atariBpb.jump[0] == 0xE9 && atariBpb.jump[1] == 0x00 &&
                               Encoding.ASCII.GetString(dos33Bpb.oem_name) != "NEXT    ")
                            {
                                DicConsole.DebugWriteLine("FAT plugin", "Using Atari BPB");
                                useAtariBpb = true;
                            }
                            else
                            {
                                DicConsole.DebugWriteLine("FAT plugin", "Using DOS 3.0 BPB");
                                useDos3Bpb = true;
                            }
                        else
                        {
                            if(atariBpb.jump[0] == 0x60 || atariBpb.jump[0] == 0xE9 && atariBpb.jump[1] == 0x00 &&
                               Encoding.ASCII.GetString(dos33Bpb.oem_name) != "NEXT    ")
                            {
                                DicConsole.DebugWriteLine("FAT plugin", "Using Atari BPB");
                                useAtariBpb = true;
                            }
                            else
                            {
                                DicConsole.DebugWriteLine("FAT plugin", "Using DOS 2.0 BPB");
                                useDos2Bpb = true;
                            }
                        }
                    }
            }

            BiosParameterBlockEbpb fakeBpb = new BiosParameterBlockEbpb();
            bool isFat12 = false;
            bool isFat16 = false;
            bool isFat32 = false;
            ulong rootDirectorySector = 0;
            string extraInfo = null;
            string bootChk = null;
            Sha1Context sha1Ctx = new Sha1Context();
            sha1Ctx.Init();

            // This is needed because for FAT16, GEMDOS increases bytes per sector count instead of using big_sectors field.
            uint sectorsPerRealSector;
            // This is needed because some OSes don't put volume label as first entry in the root directory
            uint sectorsForRootDirectory = 0;

            // DEC Rainbow, lacks a BPB but has a very concrete structure...
            if(imagePlugin.ImageInfo.Sectors == 800 && imagePlugin.ImageInfo.SectorSize == 512 && !useAtariBpb &&
               !useMsxBpb && !useDos2Bpb && !useDos3Bpb && !useDos32Bpb && !useDos33Bpb && !userShortExtendedBpb &&
               !useExtendedBpb && !useShortFat32 && !useLongFat32 && !useApricotBpb)
            {
                // DEC Rainbow boots up with a Z80, first byte should be DI (disable interrupts)
                byte z80Di = bpbSector[0];
                // First FAT1 sector resides at LBA 0x14
                byte[] fat1Sector0 = imagePlugin.ReadSector(0x14);
                // First FAT2 sector resides at LBA 0x1A
                byte[] fat2Sector0 = imagePlugin.ReadSector(0x1A);
                bool equalFatIds = fat1Sector0[0] == fat2Sector0[0] && fat1Sector0[1] == fat2Sector0[1];
                // Volume is software interleaved 2:1
                MemoryStream rootMs = new MemoryStream();
                foreach(byte[] tmp in from ulong rootSector in new[] {0x17, 0x19, 0x1B, 0x1D, 0x1E, 0x20}
                                      select imagePlugin.ReadSector(rootSector)) rootMs.Write(tmp, 0, tmp.Length);

                byte[] rootDir = rootMs.ToArray();
                bool validRootDir = true;

                // Iterate all root directory
                for(int e = 0; e < 96 * 32; e += 32)
                {
                    for(int c = 0; c < 11; c++)
                        if(rootDir[c + e] < 0x20 && rootDir[c + e] != 0x00 && rootDir[c + e] != 0x05 ||
                           rootDir[c + e] == 0xFF || rootDir[c + e] == 0x2E)
                        {
                            validRootDir = false;
                            break;
                        }

                    if(!validRootDir) break;
                }

                if(z80Di == 0xF3 && equalFatIds && (fat1Sector0[0] & 0xF0) == 0xF0 && fat1Sector0[1] == 0xFF &&
                   validRootDir)
                {
                    useDecRainbowBpb = true;
                    DicConsole.DebugWriteLine("FAT plugin", "Using DEC Rainbow hardcoded BPB.");
                    fakeBpb.bps = 512;
                    fakeBpb.spc = 1;
                    fakeBpb.rsectors = 20;
                    fakeBpb.fats_no = 2;
                    fakeBpb.root_ent = 96;
                    fakeBpb.sectors = 800;
                    fakeBpb.media = 0xFA;
                    fakeBpb.sptrk = 10;
                    fakeBpb.heads = 1;
                    fakeBpb.hsectors = 0;
                    fakeBpb.spfat = 3;
                    XmlFsType.Bootable = true;
                    fakeBpb.boot_code = bpbSector;
                    isFat12 = true;
                }
            }

            if(!useAtariBpb && !useMsxBpb && !useDos2Bpb && !useDos3Bpb && !useDos32Bpb && !useDos33Bpb &&
               !userShortExtendedBpb && !useExtendedBpb && !useShortFat32 && !useLongFat32 && !useApricotBpb &&
               !useDecRainbowBpb)
            {
                isFat12 = true;
                byte[] fatSector = imagePlugin.ReadSector(1 + partition.Start);
                switch(fatSector[0])
                {
                    case 0xE5:
                        if(imagePlugin.ImageInfo.Sectors == 2002 && imagePlugin.ImageInfo.SectorSize == 128)
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB.");
                            fakeBpb.bps = 128;
                            fakeBpb.spc = 4;
                            fakeBpb.rsectors = 1;
                            fakeBpb.fats_no = 2;
                            fakeBpb.root_ent = 64;
                            fakeBpb.sectors = 2002;
                            fakeBpb.media = 0xE5;
                            fakeBpb.sptrk = 26;
                            fakeBpb.heads = 1;
                            fakeBpb.hsectors = 0;
                            fakeBpb.spfat = 1;
                        }
                        break;
                    case 0xFD:
                        if(imagePlugin.ImageInfo.Sectors == 4004 && imagePlugin.ImageInfo.SectorSize == 128)
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB.");
                            fakeBpb.bps = 128;
                            fakeBpb.spc = 4;
                            fakeBpb.rsectors = 4;
                            fakeBpb.fats_no = 2;
                            fakeBpb.root_ent = 68;
                            fakeBpb.sectors = 4004;
                            fakeBpb.media = 0xFD;
                            fakeBpb.sptrk = 26;
                            fakeBpb.heads = 2;
                            fakeBpb.hsectors = 0;
                            fakeBpb.spfat = 6;
                        }
                        else if(imagePlugin.ImageInfo.Sectors == 2002 && imagePlugin.ImageInfo.SectorSize == 128)
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB.");
                            fakeBpb.bps = 128;
                            fakeBpb.spc = 4;
                            fakeBpb.rsectors = 4;
                            fakeBpb.fats_no = 2;
                            fakeBpb.root_ent = 68;
                            fakeBpb.sectors = 2002;
                            fakeBpb.media = 0xFD;
                            fakeBpb.sptrk = 26;
                            fakeBpb.heads = 1;
                            fakeBpb.hsectors = 0;
                            fakeBpb.spfat = 6;
                        }
                        break;
                    case 0xFE:
                        if(imagePlugin.ImageInfo.Sectors == 320 && imagePlugin.ImageInfo.SectorSize == 512)
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB for 5.25\" SSDD.");
                            fakeBpb.bps = 512;
                            fakeBpb.spc = 1;
                            fakeBpb.rsectors = 1;
                            fakeBpb.fats_no = 2;
                            fakeBpb.root_ent = 64;
                            fakeBpb.sectors = 320;
                            fakeBpb.media = 0xFE;
                            fakeBpb.sptrk = 8;
                            fakeBpb.heads = 1;
                            fakeBpb.hsectors = 0;
                            fakeBpb.spfat = 1;
                        }
                        else if(imagePlugin.ImageInfo.Sectors == 2002 && imagePlugin.ImageInfo.SectorSize == 128)
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB.");
                            fakeBpb.bps = 128;
                            fakeBpb.spc = 4;
                            fakeBpb.rsectors = 1;
                            fakeBpb.fats_no = 2;
                            fakeBpb.root_ent = 68;
                            fakeBpb.sectors = 2002;
                            fakeBpb.media = 0xFE;
                            fakeBpb.sptrk = 26;
                            fakeBpb.heads = 1;
                            fakeBpb.hsectors = 0;
                            fakeBpb.spfat = 6;
                        }
                        else if(imagePlugin.ImageInfo.Sectors == 1232 && imagePlugin.ImageInfo.SectorSize == 1024)
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB.");
                            fakeBpb.bps = 1024;
                            fakeBpb.spc = 1;
                            fakeBpb.rsectors = 1;
                            fakeBpb.fats_no = 2;
                            fakeBpb.root_ent = 192;
                            fakeBpb.sectors = 1232;
                            fakeBpb.media = 0xFE;
                            fakeBpb.sptrk = 8;
                            fakeBpb.heads = 2;
                            fakeBpb.hsectors = 0;
                            fakeBpb.spfat = 2;
                        }
                        else if(imagePlugin.ImageInfo.Sectors == 616 && imagePlugin.ImageInfo.SectorSize == 1024)
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB.");
                            fakeBpb.bps = 1024;
                            fakeBpb.spc = 1;
                            fakeBpb.rsectors = 1;
                            fakeBpb.fats_no = 2;
                            fakeBpb.root_ent = 6192;
                            fakeBpb.sectors = 616;
                            fakeBpb.media = 0xFE;
                            fakeBpb.sptrk = 8;
                            fakeBpb.heads = 2;
                            fakeBpb.hsectors = 0;
                        }
                        else if(imagePlugin.ImageInfo.Sectors == 720 && imagePlugin.ImageInfo.SectorSize == 128)
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB.");
                            fakeBpb.bps = 128;
                            fakeBpb.spc = 2;
                            fakeBpb.rsectors = 54;
                            fakeBpb.fats_no = 2;
                            fakeBpb.root_ent = 64;
                            fakeBpb.sectors = 720;
                            fakeBpb.media = 0xFE;
                            fakeBpb.sptrk = 18;
                            fakeBpb.heads = 1;
                            fakeBpb.hsectors = 0;
                            fakeBpb.spfat = 4;
                        }
                        else if(imagePlugin.ImageInfo.Sectors == 640 && imagePlugin.ImageInfo.SectorSize == 512)
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB for 5.25\" DSDD.");
                            fakeBpb.bps = 512;
                            fakeBpb.spc = 2;
                            fakeBpb.rsectors = 1;
                            fakeBpb.fats_no = 2;
                            fakeBpb.root_ent = 112;
                            fakeBpb.sectors = 640;
                            fakeBpb.media = 0xFF;
                            fakeBpb.sptrk = 8;
                            fakeBpb.heads = 2;
                            fakeBpb.hsectors = 0;
                            fakeBpb.spfat = 1;
                        }
                        break;
                    case 0xFF:
                        if(imagePlugin.ImageInfo.Sectors == 640 && imagePlugin.ImageInfo.SectorSize == 512)
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB for 5.25\" DSDD.");
                            fakeBpb.bps = 512;
                            fakeBpb.spc = 2;
                            fakeBpb.rsectors = 1;
                            fakeBpb.fats_no = 2;
                            fakeBpb.root_ent = 112;
                            fakeBpb.sectors = 640;
                            fakeBpb.media = 0xFF;
                            fakeBpb.sptrk = 8;
                            fakeBpb.heads = 2;
                            fakeBpb.hsectors = 0;
                            fakeBpb.spfat = 1;
                        }
                        break;
                }

                // This assumes a bootable sector will jump somewhere or disable interrupts in x86 code
                XmlFsType.Bootable |= bpbSector[0] == 0xFA || bpbSector[0] == 0xEB && bpbSector[1] <= 0x7F;
                fakeBpb.boot_code = bpbSector;
            }
            else if(useShortFat32 || useLongFat32)
            {
                isFat32 = true;

                // This is to support FAT partitions on hybrid ISO/USB images
                if(imagePlugin.ImageInfo.XmlMediaType == XmlMediaType.OpticalDisc)
                {
                    fat32Bpb.bps *= 4;
                    fat32Bpb.spc /= 4;
                    fat32Bpb.big_spfat /= 4;
                    fat32Bpb.hsectors /= 4;
                    fat32Bpb.sptrk /= 4;
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
                    if(fat32Bpb.oem_name[5] == 0x49 && fat32Bpb.oem_name[6] == 0x48 && fat32Bpb.oem_name[7] == 0x43)
                        sb.AppendLine("Volume has been modified by Windows 9x/Me Volume Tracker.");
                    else XmlFsType.SystemIdentifier = StringHandlers.CToString(fat32Bpb.oem_name);

                if(!string.IsNullOrEmpty(XmlFsType.SystemIdentifier))
                    sb.AppendFormat("OEM Name: {0}", XmlFsType.SystemIdentifier.Trim()).AppendLine();
                sb.AppendFormat("{0} bytes per sector.", fat32Bpb.bps).AppendLine();
                sb.AppendFormat("{0} sectors per cluster.", fat32Bpb.spc).AppendLine();
                XmlFsType.ClusterSize = fat32Bpb.bps * fat32Bpb.spc;
                sb.AppendFormat("{0} sectors reserved between BPB and FAT.", fat32Bpb.rsectors).AppendLine();
                if(fat32Bpb.big_sectors == 0 && fat32Bpb.signature == 0x28)
                {
                    sb.AppendFormat("{0} sectors on volume ({1} bytes).", shortFat32Bpb.huge_sectors,
                                    shortFat32Bpb.huge_sectors * shortFat32Bpb.bps).AppendLine();
                    XmlFsType.Clusters = (long)(shortFat32Bpb.huge_sectors / shortFat32Bpb.spc);
                }
                else
                {
                    sb.AppendFormat("{0} sectors on volume ({1} bytes).", fat32Bpb.big_sectors,
                                    fat32Bpb.big_sectors * fat32Bpb.bps).AppendLine();
                    XmlFsType.Clusters = fat32Bpb.big_sectors / fat32Bpb.spc;
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
                    if((fat32Bpb.flags & 0x02) == 0x02) sb.AppendLine("Disk surface should be on next mount.");
                }

                if((fat32Bpb.mirror_flags & 0x80) == 0x80)
                    sb.AppendFormat("FATs are out of sync. FAT #{0} is in use.", fat32Bpb.mirror_flags & 0xF)
                      .AppendLine();
                else sb.AppendLine("All copies of FAT are the same.");

                if((fat32Bpb.mirror_flags & 0x6F20) == 0x6F20) sb.AppendLine("DR-DOS will boot this FAT32 using CHS.");
                else if((fat32Bpb.mirror_flags & 0x4F20) == 0x4F20)
                    sb.AppendLine("DR-DOS will boot this FAT32 using LBA.");

                if(fat32Bpb.signature == 0x29)
                {
                    XmlFsType.VolumeName = Encoding.ASCII.GetString(fat32Bpb.volume_label);
                    sb.AppendFormat("Filesystem type: {0}", Encoding.ASCII.GetString(fat32Bpb.fs_type)).AppendLine();
                    bootChk = sha1Ctx.Data(fat32Bpb.boot_code, out _);
                }
                else bootChk = sha1Ctx.Data(shortFat32Bpb.boot_code, out _);

                // Check that jumps to a correct boot code position and has boot signature set.
                // This will mean that the volume will boot, even if just to say "this is not bootable change disk"......
                XmlFsType.Bootable |= fat32Bpb.jump[0] == 0xEB && fat32Bpb.jump[1] > 0x58 && fat32Bpb.jump[1] < 0x80 &&
                                      fat32Bpb.boot_signature == 0xAA55;

                sectorsPerRealSector = fat32Bpb.bps / imagePlugin.ImageInfo.SectorSize;
                // First root directory sector
                rootDirectorySector =
                    (ulong)((fat32Bpb.root_cluster - 2) * fat32Bpb.spc + fat32Bpb.big_spfat * fat32Bpb.fats_no +
                            fat32Bpb.rsectors) * sectorsPerRealSector;
                sectorsForRootDirectory = 1;

                if(fat32Bpb.fsinfo_sector + partition.Start <= partition.End)
                {
                    byte[] fsinfoSector = imagePlugin.ReadSector(fat32Bpb.fsinfo_sector + partition.Start);
                    IntPtr fsinfoPtr = Marshal.AllocHGlobal(512);
                    Marshal.Copy(fsinfoSector, 0, fsinfoPtr, 512);
                    FsInfoSector fsInfo = (FsInfoSector)Marshal.PtrToStructure(fsinfoPtr, typeof(FsInfoSector));
                    Marshal.FreeHGlobal(fsinfoPtr);

                    if(fsInfo.signature1 == FSINFO_SIGNATURE1 && fsInfo.signature2 == FSINFO_SIGNATURE2 &&
                       fsInfo.signature3 == FSINFO_SIGNATURE3)
                    {
                        if(fsInfo.free_clusters < 0xFFFFFFFF)
                        {
                            sb.AppendFormat("{0} free clusters", fsInfo.free_clusters).AppendLine();
                            XmlFsType.FreeClusters = fsInfo.free_clusters;
                            XmlFsType.FreeClustersSpecified = true;
                        }

                        if(fsInfo.last_cluster > 2 && fsInfo.last_cluster < 0xFFFFFFFF)
                            sb.AppendFormat("Last allocated cluster {0}", fsInfo.last_cluster).AppendLine();
                    }
                }
            }
            else if(useExtendedBpb) fakeBpb = ebpb;
            else if(userShortExtendedBpb)
            {
                fakeBpb.jump = shortEbpb.jump;
                fakeBpb.oem_name = shortEbpb.oem_name;
                fakeBpb.bps = shortEbpb.bps;
                fakeBpb.spc = shortEbpb.spc;
                fakeBpb.rsectors = shortEbpb.rsectors;
                fakeBpb.fats_no = shortEbpb.fats_no;
                fakeBpb.root_ent = shortEbpb.root_ent;
                fakeBpb.sectors = shortEbpb.sectors;
                fakeBpb.media = shortEbpb.media;
                fakeBpb.spfat = shortEbpb.spfat;
                fakeBpb.sptrk = shortEbpb.sptrk;
                fakeBpb.heads = shortEbpb.heads;
                fakeBpb.hsectors = shortEbpb.hsectors;
                fakeBpb.big_sectors = shortEbpb.big_sectors;
                fakeBpb.drive_no = shortEbpb.drive_no;
                fakeBpb.flags = shortEbpb.flags;
                fakeBpb.signature = shortEbpb.signature;
                fakeBpb.serial_no = shortEbpb.serial_no;
                fakeBpb.boot_code = shortEbpb.boot_code;
                fakeBpb.boot_signature = shortEbpb.boot_signature;
            }
            else if(useDos33Bpb)
            {
                fakeBpb.jump = dos33Bpb.jump;
                fakeBpb.oem_name = dos33Bpb.oem_name;
                fakeBpb.bps = dos33Bpb.bps;
                fakeBpb.spc = dos33Bpb.spc;
                fakeBpb.rsectors = dos33Bpb.rsectors;
                fakeBpb.fats_no = dos33Bpb.fats_no;
                fakeBpb.root_ent = dos33Bpb.root_ent;
                fakeBpb.sectors = dos33Bpb.sectors;
                fakeBpb.media = dos33Bpb.media;
                fakeBpb.spfat = dos33Bpb.spfat;
                fakeBpb.sptrk = dos33Bpb.sptrk;
                fakeBpb.heads = dos33Bpb.heads;
                fakeBpb.hsectors = dos33Bpb.hsectors;
                fakeBpb.big_sectors = dos33Bpb.big_sectors;
                fakeBpb.boot_code = dos33Bpb.boot_code;
                fakeBpb.boot_signature = dos33Bpb.boot_signature;
            }
            else if(useDos32Bpb)
            {
                fakeBpb.jump = dos32Bpb.jump;
                fakeBpb.oem_name = dos32Bpb.oem_name;
                fakeBpb.bps = dos32Bpb.bps;
                fakeBpb.spc = dos32Bpb.spc;
                fakeBpb.rsectors = dos32Bpb.rsectors;
                fakeBpb.fats_no = dos32Bpb.fats_no;
                fakeBpb.root_ent = dos32Bpb.root_ent;
                fakeBpb.sectors = dos32Bpb.sectors;
                fakeBpb.media = dos32Bpb.media;
                fakeBpb.spfat = dos32Bpb.spfat;
                fakeBpb.sptrk = dos32Bpb.sptrk;
                fakeBpb.heads = dos32Bpb.heads;
                fakeBpb.hsectors = dos32Bpb.hsectors;
                fakeBpb.boot_code = dos32Bpb.boot_code;
                fakeBpb.boot_signature = dos32Bpb.boot_signature;
            }
            else if(useDos3Bpb)
            {
                fakeBpb.jump = dos30Bpb.jump;
                fakeBpb.oem_name = dos30Bpb.oem_name;
                fakeBpb.bps = dos30Bpb.bps;
                fakeBpb.spc = dos30Bpb.spc;
                fakeBpb.rsectors = dos30Bpb.rsectors;
                fakeBpb.fats_no = dos30Bpb.fats_no;
                fakeBpb.root_ent = dos30Bpb.root_ent;
                fakeBpb.sectors = dos30Bpb.sectors;
                fakeBpb.media = dos30Bpb.media;
                fakeBpb.spfat = dos30Bpb.spfat;
                fakeBpb.sptrk = dos30Bpb.sptrk;
                fakeBpb.heads = dos30Bpb.heads;
                fakeBpb.hsectors = dos30Bpb.hsectors;
                fakeBpb.boot_code = dos30Bpb.boot_code;
                fakeBpb.boot_signature = dos30Bpb.boot_signature;
            }
            else if(useDos2Bpb)
            {
                fakeBpb.jump = dos2Bpb.jump;
                fakeBpb.oem_name = dos2Bpb.oem_name;
                fakeBpb.bps = dos2Bpb.bps;
                fakeBpb.spc = dos2Bpb.spc;
                fakeBpb.rsectors = dos2Bpb.rsectors;
                fakeBpb.fats_no = dos2Bpb.fats_no;
                fakeBpb.root_ent = dos2Bpb.root_ent;
                fakeBpb.sectors = dos2Bpb.sectors;
                fakeBpb.media = dos2Bpb.media;
                fakeBpb.spfat = dos2Bpb.spfat;
                fakeBpb.boot_code = dos2Bpb.boot_code;
                fakeBpb.boot_signature = dos2Bpb.boot_signature;
            }
            else if(useMsxBpb)
            {
                isFat12 = true;
                fakeBpb.jump = msxBpb.jump;
                fakeBpb.oem_name = msxBpb.oem_name;
                fakeBpb.bps = msxBpb.bps;
                fakeBpb.spc = msxBpb.spc;
                fakeBpb.rsectors = msxBpb.rsectors;
                fakeBpb.fats_no = msxBpb.fats_no;
                fakeBpb.root_ent = msxBpb.root_ent;
                fakeBpb.sectors = msxBpb.sectors;
                fakeBpb.media = msxBpb.media;
                fakeBpb.spfat = msxBpb.spfat;
                fakeBpb.sptrk = msxBpb.sptrk;
                fakeBpb.heads = msxBpb.heads;
                fakeBpb.hsectors = msxBpb.hsectors;
                fakeBpb.boot_code = msxBpb.boot_code;
                fakeBpb.boot_signature = msxBpb.boot_signature;
                fakeBpb.serial_no = msxBpb.serial_no;
                // TODO: Is there any way to check this?
                XmlFsType.Bootable = true;
            }
            else if(useAtariBpb)
            {
                fakeBpb.jump = atariBpb.jump;
                fakeBpb.oem_name = atariBpb.oem_name;
                fakeBpb.bps = atariBpb.bps;
                fakeBpb.spc = atariBpb.spc;
                fakeBpb.rsectors = atariBpb.rsectors;
                fakeBpb.fats_no = atariBpb.fats_no;
                fakeBpb.root_ent = atariBpb.root_ent;
                fakeBpb.sectors = atariBpb.sectors;
                fakeBpb.media = atariBpb.media;
                fakeBpb.spfat = atariBpb.spfat;
                fakeBpb.sptrk = atariBpb.sptrk;
                fakeBpb.heads = atariBpb.heads;
                fakeBpb.boot_code = atariBpb.boot_code;

                ushort sum = 0;
                BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
                for(int i = 0; i < bpbSector.Length; i += 2) sum += BigEndianBitConverter.ToUInt16(bpbSector, i);

                // TODO: Check this
                if(sum == 0x1234)
                {
                    XmlFsType.Bootable = true;
                    StringBuilder atariSb = new StringBuilder();
                    atariSb.AppendFormat("cmdload will be loaded with value {0:X4}h",
                                         BigEndianBitConverter.ToUInt16(bpbSector, 0x01E)).AppendLine();
                    atariSb.AppendFormat("Boot program will be loaded at address {0:X4}h", atariBpb.ldaaddr)
                           .AppendLine();
                    atariSb.AppendFormat("FAT and directory will be cached at address {0:X4}h", atariBpb.fatbuf)
                           .AppendLine();
                    if(atariBpb.ldmode == 0)
                    {
                        byte[] tmp = new byte[8];
                        Array.Copy(atariBpb.fname, 0, tmp, 0, 8);
                        string fname = Encoding.ASCII.GetString(tmp).Trim();
                        tmp = new byte[3];
                        Array.Copy(atariBpb.fname, 8, tmp, 0, 3);
                        string extension = Encoding.ASCII.GetString(tmp).Trim();
                        string filename;

                        if(string.IsNullOrEmpty(extension)) filename = fname;
                        else filename = fname + "." + extension;

                        atariSb.AppendFormat("Boot program resides in file \"{0}\"", filename).AppendLine();
                    }
                    else
                        atariSb.AppendFormat("Boot program starts in sector {0} and is {1} sectors long ({2} bytes)",
                                             atariBpb.ssect, atariBpb.sectcnt, atariBpb.sectcnt * atariBpb.bps)
                               .AppendLine();

                    extraInfo = atariSb.ToString();
                }
            }
            else if(useApricotBpb)
            {
                isFat12 = true;
                fakeBpb.bps = apricotBpb.mainBPB.bps;
                fakeBpb.spc = apricotBpb.mainBPB.spc;
                fakeBpb.rsectors = apricotBpb.mainBPB.rsectors;
                fakeBpb.fats_no = apricotBpb.mainBPB.fats_no;
                fakeBpb.root_ent = apricotBpb.mainBPB.root_ent;
                fakeBpb.sectors = apricotBpb.mainBPB.sectors;
                fakeBpb.media = apricotBpb.mainBPB.media;
                fakeBpb.spfat = apricotBpb.mainBPB.spfat;
                fakeBpb.sptrk = apricotBpb.spt;
                XmlFsType.Bootable = apricotBpb.bootType > 0;

                if(apricotBpb.bootLocation > 0 &&
                   apricotBpb.bootLocation + apricotBpb.bootSize < imagePlugin.ImageInfo.Sectors)
                    fakeBpb.boot_code = imagePlugin.ReadSectors(apricotBpb.bootLocation,
                                                                (uint)(apricotBpb.sectorSize * apricotBpb.bootSize) /
                                                                imagePlugin.ImageInfo.SectorSize);
            }

            if(!isFat32)
            {
                // This is to support FAT partitions on hybrid ISO/USB images
                if(imagePlugin.ImageInfo.XmlMediaType == XmlMediaType.OpticalDisc)
                {
                    fakeBpb.bps *= 4;
                    fakeBpb.spc /= 4;
                    fakeBpb.spfat /= 4;
                    fakeBpb.hsectors /= 4;
                    fakeBpb.sptrk /= 4;
                    fakeBpb.rsectors /= 4;

                    if(fakeBpb.spc == 0) fakeBpb.spc = 1;
                }

                // This assumes no sane implementation will violate cluster size rules
                // However nothing prevents this to happen
                // If first file on disk uses only one cluster there is absolutely no way to differentiate between FAT12 and FAT16,
                // so let's hope implementations use common sense?
                if(!isFat12 && !isFat16)
                {
                    ulong clusters;

                    if(fakeBpb.sectors == 0)
                        clusters = fakeBpb.spc == 0 ? fakeBpb.big_sectors : fakeBpb.big_sectors / fakeBpb.spc;
                    else clusters = fakeBpb.spc == 0 ? fakeBpb.sectors : (ulong)fakeBpb.sectors / fakeBpb.spc;

                    if(clusters < 4089) isFat12 = true;
                    else isFat16 = true;
                }

                if(isFat12)
                {
                    if(useAtariBpb) sb.AppendLine("Atari FAT12");
                    else if(useApricotBpb) sb.AppendLine("Apricot FAT12");
                    else sb.AppendLine("Microsoft FAT12");
                    XmlFsType.Type = "FAT12";
                }
                else if(isFat16)
                {
                    sb.AppendLine(useAtariBpb ? "Atari FAT16" : "Microsoft FAT16");
                    XmlFsType.Type = "FAT16";
                }

                if(useAtariBpb)
                {
                    if(atariBpb.serial_no[0] == 0x49 && atariBpb.serial_no[1] == 0x48 && atariBpb.serial_no[2] == 0x43)
                        sb.AppendLine("Volume has been modified by Windows 9x/Me Volume Tracker.");
                    else
                        XmlFsType.VolumeSerial =
                            $"{atariBpb.serial_no[0]:X2}{atariBpb.serial_no[1]:X2}{atariBpb.serial_no[2]:X2}";

                    XmlFsType.SystemIdentifier = StringHandlers.CToString(atariBpb.oem_name);
                    if(string.IsNullOrEmpty(XmlFsType.SystemIdentifier)) XmlFsType.SystemIdentifier = null;
                }
                else if(fakeBpb.oem_name != null)
                {
                    if(fakeBpb.oem_name[5] == 0x49 && fakeBpb.oem_name[6] == 0x48 && fakeBpb.oem_name[7] == 0x43)
                        sb.AppendLine("Volume has been modified by Windows 9x/Me Volume Tracker.");
                    else
                    {
                        // Later versions of Windows create a DOS 3 BPB without OEM name on 8 sectors/track floppies
                        // OEM ID should be ASCII, otherwise ignore it
                        if(fakeBpb.oem_name[0] >= 0x20 && fakeBpb.oem_name[0] <= 0x7F && fakeBpb.oem_name[1] >= 0x20 &&
                           fakeBpb.oem_name[1] <= 0x7F && fakeBpb.oem_name[2] >= 0x20 && fakeBpb.oem_name[2] <= 0x7F &&
                           fakeBpb.oem_name[3] >= 0x20 && fakeBpb.oem_name[3] <= 0x7F && fakeBpb.oem_name[4] >= 0x20 &&
                           fakeBpb.oem_name[4] <= 0x7F && fakeBpb.oem_name[5] >= 0x20 && fakeBpb.oem_name[5] <= 0x7F &&
                           fakeBpb.oem_name[6] >= 0x20 && fakeBpb.oem_name[6] <= 0x7F && fakeBpb.oem_name[7] >= 0x20 &&
                           fakeBpb.oem_name[7] <= 0x7F)
                            XmlFsType.SystemIdentifier = StringHandlers.CToString(fakeBpb.oem_name);
                        else if(fakeBpb.oem_name[0] < 0x20 && fakeBpb.oem_name[1] >= 0x20 &&
                                fakeBpb.oem_name[1] <= 0x7F && fakeBpb.oem_name[2] >= 0x20 &&
                                fakeBpb.oem_name[2] <= 0x7F && fakeBpb.oem_name[3] >= 0x20 &&
                                fakeBpb.oem_name[3] <= 0x7F && fakeBpb.oem_name[4] >= 0x20 &&
                                fakeBpb.oem_name[4] <= 0x7F && fakeBpb.oem_name[5] >= 0x20 &&
                                fakeBpb.oem_name[5] <= 0x7F && fakeBpb.oem_name[6] >= 0x20 &&
                                fakeBpb.oem_name[6] <= 0x7F && fakeBpb.oem_name[7] >= 0x20 &&
                                fakeBpb.oem_name[7] <= 0x7F)
                            XmlFsType.SystemIdentifier =
                                StringHandlers.CToString(fakeBpb.oem_name, CurrentEncoding, start: 1);
                    }

                    if(fakeBpb.signature == 0x28 || fakeBpb.signature == 0x29)
                        XmlFsType.VolumeSerial = $"{fakeBpb.serial_no:X8}";
                }

                if(XmlFsType.SystemIdentifier != null)
                    sb.AppendFormat("OEM Name: {0}", XmlFsType.SystemIdentifier.Trim()).AppendLine();

                sb.AppendFormat("{0} bytes per sector.", fakeBpb.bps).AppendLine();
                if(fakeBpb.sectors == 0)
                {
                    sb.AppendFormat("{0} sectors on volume ({1} bytes).", fakeBpb.big_sectors,
                                    fakeBpb.big_sectors * fakeBpb.bps).AppendLine();
                    XmlFsType.Clusters = fakeBpb.spc == 0 ? fakeBpb.big_sectors : fakeBpb.big_sectors / fakeBpb.spc;
                }
                else
                {
                    sb.AppendFormat("{0} sectors on volume ({1} bytes).", fakeBpb.sectors,
                                    fakeBpb.sectors * fakeBpb.bps).AppendLine();
                    XmlFsType.Clusters = fakeBpb.spc == 0 ? fakeBpb.sectors : fakeBpb.sectors / fakeBpb.spc;
                }
                sb.AppendFormat("{0} sectors per cluster.", fakeBpb.spc).AppendLine();
                sb.AppendFormat("{0} clusters on volume.", XmlFsType.Clusters).AppendLine();
                XmlFsType.ClusterSize = fakeBpb.bps * fakeBpb.spc;
                sb.AppendFormat("{0} sectors reserved between BPB and FAT.", fakeBpb.rsectors).AppendLine();
                sb.AppendFormat("{0} FATs.", fakeBpb.fats_no).AppendLine();
                sb.AppendFormat("{0} entries on root directory.", fakeBpb.root_ent).AppendLine();

                if(fakeBpb.media > 0) sb.AppendFormat("Media descriptor: 0x{0:X2}", fakeBpb.media).AppendLine();

                sb.AppendFormat("{0} sectors per FAT.", fakeBpb.spfat).AppendLine();

                if(fakeBpb.sptrk > 0 && fakeBpb.sptrk < 64 && fakeBpb.heads > 0 && fakeBpb.heads < 256)
                {
                    sb.AppendFormat("{0} sectors per track.", fakeBpb.sptrk).AppendLine();
                    sb.AppendFormat("{0} heads.", fakeBpb.heads).AppendLine();
                }
                if(fakeBpb.hsectors <= partition.Start)
                    sb.AppendFormat("{0} hidden sectors before BPB.", fakeBpb.hsectors).AppendLine();

                if(fakeBpb.signature == 0x28 || fakeBpb.signature == 0x29 || andosOemCorrect)
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
                        if((fakeBpb.flags & 0x02) == 0x02) sb.AppendLine("Disk surface should be on next mount.");
                    }

                    if(fakeBpb.signature == 0x29 || andosOemCorrect)
                    {
                        XmlFsType.VolumeName = Encoding.ASCII.GetString(fakeBpb.volume_label);
                        sb.AppendFormat("Filesystem type: {0}", Encoding.ASCII.GetString(fakeBpb.fs_type)).AppendLine();
                    }
                }
                else if(useAtariBpb && XmlFsType.VolumeSerial != null)
                    sb.AppendFormat("Volume Serial Number: {0}", XmlFsType.VolumeSerial).AppendLine();

                bootChk = sha1Ctx.Data(fakeBpb.boot_code, out _);

                // Check that jumps to a correct boot code position and has boot signature set.
                // This will mean that the volume will boot, even if just to say "this is not bootable change disk"......
                if(XmlFsType.Bootable == false && fakeBpb.jump != null)
                    XmlFsType.Bootable |= fakeBpb.jump[0] == 0xEB && fakeBpb.jump[1] > 0x58 && fakeBpb.jump[1] < 0x80 &&
                                          fakeBpb.boot_signature == 0xAA55;

                sectorsPerRealSector = fakeBpb.bps / imagePlugin.ImageInfo.SectorSize;
                // First root directory sector
                rootDirectorySector =
                    (ulong)(fakeBpb.spfat * fakeBpb.fats_no + fakeBpb.rsectors) * sectorsPerRealSector;
                sectorsForRootDirectory = (uint)(fakeBpb.root_ent * 32 / imagePlugin.ImageInfo.SectorSize);
            }

            if(extraInfo != null) sb.Append(extraInfo);

            if(rootDirectorySector + partition.Start < partition.End &&
               imagePlugin.ImageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
            {
                byte[] rootDirectory =
                    imagePlugin.ReadSectors(rootDirectorySector + partition.Start, sectorsForRootDirectory);

                if(useDecRainbowBpb)
                {
                    MemoryStream rootMs = new MemoryStream();
                    foreach(byte[] tmp in from ulong rootSector in new[] {0x17, 0x19, 0x1B, 0x1D, 0x1E, 0x20}
                                          select imagePlugin.ReadSector(rootSector)) rootMs.Write(tmp, 0, tmp.Length);

                    rootDirectory = rootMs.ToArray();
                }

                for(int i = 0; i < rootDirectory.Length; i += 32)
                {
                    // Not a correct entry
                    if(rootDirectory[i] < 0x20 && rootDirectory[i] != 0x05) continue;

                    // Deleted or subdirectory entry
                    if(rootDirectory[i] == 0x2E || rootDirectory[i] == 0xE5) continue;

                    // Not a volume label
                    if(rootDirectory[i + 0x0B] != 0x08 && rootDirectory[i + 0x0B] != 0x28) continue;

                    IntPtr entryPtr = Marshal.AllocHGlobal(32);
                    Marshal.Copy(rootDirectory, i, entryPtr, 32);
                    DirectoryEntry entry = (DirectoryEntry)Marshal.PtrToStructure(entryPtr, typeof(DirectoryEntry));
                    Marshal.FreeHGlobal(entryPtr);

                    byte[] fullname = new byte[11];
                    Array.Copy(entry.filename, 0, fullname, 0, 8);
                    Array.Copy(entry.extension, 0, fullname, 8, 3);
                    string volname = CurrentEncoding.GetString(fullname).Trim();
                    if(!string.IsNullOrEmpty(volname))
                        XmlFsType.VolumeName = (entry.caseinfo & 0x0C) > 0 ? volname.ToLower() : volname;

                    if(entry.ctime > 0 && entry.cdate > 0)
                    {
                        XmlFsType.CreationDate = DateHandlers.DosToDateTime(entry.cdate, entry.ctime);
                        if(entry.ctime_ms > 0)
                            XmlFsType.CreationDate = XmlFsType.CreationDate.AddMilliseconds(entry.ctime_ms * 10);
                        XmlFsType.CreationDateSpecified = true;
                        sb.AppendFormat("Volume created on {0}", XmlFsType.CreationDate).AppendLine();
                    }

                    if(entry.mtime > 0 && entry.mdate > 0)
                    {
                        XmlFsType.ModificationDate = DateHandlers.DosToDateTime(entry.mdate, entry.mtime);
                        XmlFsType.ModificationDateSpecified = true;
                        sb.AppendFormat("Volume last modified on {0}", XmlFsType.ModificationDate).AppendLine();
                    }

                    if(entry.adate > 0)
                        sb.AppendFormat("Volume last accessed on {0:d}", DateHandlers.DosToDateTime(entry.adate, 0))
                          .AppendLine();

                    break;
                }
            }

            if(!string.IsNullOrEmpty(XmlFsType.VolumeName))
                sb.AppendFormat("Volume label: {0}", XmlFsType.VolumeName).AppendLine();
            if(XmlFsType.Bootable)
            {
                sb.AppendLine("Volume is bootable");
                sb.AppendFormat("Boot code's SHA1: {0}", bootChk).AppendLine();
            }

            information = sb.ToString();
        }

        public override Errno Mount()
        {
            return Errno.NotImplemented;
        }

        public override Errno Mount(bool debug)
        {
            return Errno.NotImplemented;
        }

        public override Errno Unmount()
        {
            return Errno.NotImplemented;
        }

        public override Errno MapBlock(string path, long fileBlock, ref long deviceBlock)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetAttributes(string path, ref FileAttributes attributes)
        {
            return Errno.NotImplemented;
        }

        public override Errno ListXAttr(string path, ref List<string> xattrs)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadDir(string path, ref List<string> contents)
        {
            return Errno.NotImplemented;
        }

        public override Errno StatFs(ref FileSystemInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno Stat(string path, ref FileEntryInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadLink(string path, ref string dest)
        {
            return Errno.NotImplemented;
        }

        /// <summary>
        ///     BIOS Parameter Block as used by Atari ST GEMDOS on FAT12 volumes.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AtariParameterBlock
        {
            /// <summary>68000 BRA.S jump or x86 loop</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] jump;
            /// <summary>OEM Name, 6 bytes, space-padded, "Loader" for Atari ST boot loader</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public byte[] oem_name;
            /// <summary>Volume serial number</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] serial_no;
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors per cluster</summary>
            public byte spc;
            /// <summary>Reserved sectors between BPB and FAT (inclusive)</summary>
            public ushort rsectors;
            /// <summary>Number of FATs</summary>
            public byte fats_no;
            /// <summary>Number of entries on root directory</summary>
            public ushort root_ent;
            /// <summary>Sectors in volume</summary>
            public ushort sectors;
            /// <summary>Media descriptor, unused by GEMDOS</summary>
            public byte media;
            /// <summary>Sectors per FAT</summary>
            public ushort spfat;
            /// <summary>Sectors per track</summary>
            public ushort sptrk;
            /// <summary>Heads</summary>
            public ushort heads;
            /// <summary>Hidden sectors before BPB, unused by GEMDOS</summary>
            public ushort hsectors;
            /// <summary>Word to be loaded in the cmdload system variable. Big-endian.</summary>
            public ushort execflag;
            /// <summary>
            ///     Word indicating load mode. If zero, file named <see cref="fname" /> is located and loaded. It not, sectors
            ///     specified in <see cref="ssect" /> and <see cref="sectcnt" /> are loaded. Big endian.
            /// </summary>
            public ushort ldmode;
            /// <summary>Starting sector of boot code.</summary>
            public ushort ssect;
            /// <summary>Count of sectors of boot code.</summary>
            public ushort sectcnt;
            /// <summary>Address where boot code should be loaded.</summary>
            public ushort ldaaddr;
            /// <summary>Padding.</summary>
            public ushort padding;
            /// <summary>Address where FAT and root directory sectors must be loaded.</summary>
            public ushort fatbuf;
            /// <summary>Unknown.</summary>
            public ushort unknown;
            /// <summary>Filename to be loaded for booting.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)] public byte[] fname;
            /// <summary>Reserved</summary>
            public ushort reserved;
            /// <summary>Boot code.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 455)] public byte[] boot_code;
            /// <summary>Big endian word to make big endian sum of all sector words be equal to 0x1234 if disk is bootable.</summary>
            public ushort checksum;
        }

        /// <summary>
        ///     BIOS Parameter Block as used by MSX-DOS 2.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct MsxParameterBlock
        {
            /// <summary>x86 loop</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] jump;
            /// <summary>OEM Name, 8 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] oem_name;
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors per cluster</summary>
            public byte spc;
            /// <summary>Reserved sectors between BPB and FAT (inclusive)</summary>
            public ushort rsectors;
            /// <summary>Number of FATs</summary>
            public byte fats_no;
            /// <summary>Number of entries on root directory</summary>
            public ushort root_ent;
            /// <summary>Sectors in volume</summary>
            public ushort sectors;
            /// <summary>Media descriptor</summary>
            public byte media;
            /// <summary>Sectors per FAT</summary>
            public ushort spfat;
            /// <summary>Sectors per track</summary>
            public ushort sptrk;
            /// <summary>Heads</summary>
            public ushort heads;
            /// <summary>Hidden sectors before BPB</summary>
            public ushort hsectors;
            /// <summary>Jump for MSX-DOS 1 boot code</summary>
            public ushort msxdos_jmp;
            /// <summary>Set to "VOL_ID" by MSX-DOS 2</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)] public byte[] vol_id;
            /// <summary>Bigger than 0 if there are deleted files (MSX-DOS 2)</summary>
            public byte undelete_flag;
            /// <summary>Volume serial number (MSX-DOS 2)</summary>
            public uint serial_no;
            /// <summary>Reserved (MSX-DOS 2)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)] public byte[] reserved;
            /// <summary>Jump for MSX-DOS 2 boot code (MSX-DOS 2)</summary>
            public ushort msxdos2_jmp;
            /// <summary>Boot code.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 460)] public byte[] boot_code;
            /// <summary>Always 0x55 0xAA.</summary>
            public ushort boot_signature;
        }

        /// <summary>DOS 2.0 BIOS Parameter Block.</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BiosParameterBlock2
        {
            /// <summary>x86 jump</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] jump;
            /// <summary>OEM Name, 8 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] oem_name;
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors per cluster</summary>
            public byte spc;
            /// <summary>Reserved sectors between BPB and FAT</summary>
            public ushort rsectors;
            /// <summary>Number of FATs</summary>
            public byte fats_no;
            /// <summary>Number of entries on root directory</summary>
            public ushort root_ent;
            /// <summary>Sectors in volume</summary>
            public ushort sectors;
            /// <summary>Media descriptor</summary>
            public byte media;
            /// <summary>Sectors per FAT</summary>
            public ushort spfat;
            /// <summary>Boot code.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 486)] public byte[] boot_code;
            /// <summary>0x55 0xAA if bootable.</summary>
            public ushort boot_signature;
        }

        /// <summary>DOS 3.0 BIOS Parameter Block.</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BiosParameterBlock30
        {
            /// <summary>x86 jump</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] jump;
            /// <summary>OEM Name, 8 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] oem_name;
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors per cluster</summary>
            public byte spc;
            /// <summary>Reserved sectors between BPB and FAT</summary>
            public ushort rsectors;
            /// <summary>Number of FATs</summary>
            public byte fats_no;
            /// <summary>Number of entries on root directory</summary>
            public ushort root_ent;
            /// <summary>Sectors in volume</summary>
            public ushort sectors;
            /// <summary>Media descriptor</summary>
            public byte media;
            /// <summary>Sectors per FAT</summary>
            public ushort spfat;
            /// <summary>Sectors per track</summary>
            public ushort sptrk;
            /// <summary>Heads</summary>
            public ushort heads;
            /// <summary>Hidden sectors before BPB</summary>
            public ushort hsectors;
            /// <summary>Boot code.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 480)] public byte[] boot_code;
            /// <summary>Always 0x55 0xAA.</summary>
            public ushort boot_signature;
        }

        /// <summary>DOS 3.2 BIOS Parameter Block.</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BiosParameterBlock32
        {
            /// <summary>x86 jump</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] jump;
            /// <summary>OEM Name, 8 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] oem_name;
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors per cluster</summary>
            public byte spc;
            /// <summary>Reserved sectors between BPB and FAT</summary>
            public ushort rsectors;
            /// <summary>Number of FATs</summary>
            public byte fats_no;
            /// <summary>Number of entries on root directory</summary>
            public ushort root_ent;
            /// <summary>Sectors in volume</summary>
            public ushort sectors;
            /// <summary>Media descriptor</summary>
            public byte media;
            /// <summary>Sectors per FAT</summary>
            public ushort spfat;
            /// <summary>Sectors per track</summary>
            public ushort sptrk;
            /// <summary>Heads</summary>
            public ushort heads;
            /// <summary>Hidden sectors before BPB</summary>
            public ushort hsectors;
            /// <summary>Total sectors including hidden ones</summary>
            public ushort total_sectors;
            /// <summary>Boot code.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 478)] public byte[] boot_code;
            /// <summary>Always 0x55 0xAA.</summary>
            public ushort boot_signature;
        }

        /// <summary>DOS 3.31 BIOS Parameter Block.</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BiosParameterBlock33
        {
            /// <summary>x86 jump</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] jump;
            /// <summary>OEM Name, 8 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] oem_name;
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors per cluster</summary>
            public byte spc;
            /// <summary>Reserved sectors between BPB and FAT</summary>
            public ushort rsectors;
            /// <summary>Number of FATs</summary>
            public byte fats_no;
            /// <summary>Number of entries on root directory</summary>
            public ushort root_ent;
            /// <summary>Sectors in volume</summary>
            public ushort sectors;
            /// <summary>Media descriptor</summary>
            public byte media;
            /// <summary>Sectors per FAT</summary>
            public ushort spfat;
            /// <summary>Sectors per track</summary>
            public ushort sptrk;
            /// <summary>Heads</summary>
            public ushort heads;
            /// <summary>Hidden sectors before BPB</summary>
            public uint hsectors;
            /// <summary>Sectors in volume if > 65535</summary>
            public uint big_sectors;
            /// <summary>Boot code.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 474)] public byte[] boot_code;
            /// <summary>Always 0x55 0xAA.</summary>
            public ushort boot_signature;
        }

        /// <summary>DOS 3.4 BIOS Parameter Block.</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BiosParameterBlockShortEbpb
        {
            /// <summary>x86 jump</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] jump;
            /// <summary>OEM Name, 8 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] oem_name;
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors per cluster</summary>
            public byte spc;
            /// <summary>Reserved sectors between BPB and FAT</summary>
            public ushort rsectors;
            /// <summary>Number of FATs</summary>
            public byte fats_no;
            /// <summary>Number of entries on root directory</summary>
            public ushort root_ent;
            /// <summary>Sectors in volume</summary>
            public ushort sectors;
            /// <summary>Media descriptor</summary>
            public byte media;
            /// <summary>Sectors per FAT</summary>
            public ushort spfat;
            /// <summary>Sectors per track</summary>
            public ushort sptrk;
            /// <summary>Heads</summary>
            public ushort heads;
            /// <summary>Hidden sectors before BPB</summary>
            public uint hsectors;
            /// <summary>Sectors in volume if > 65535</summary>
            public uint big_sectors;
            /// <summary>Drive number</summary>
            public byte drive_no;
            /// <summary>Volume flags</summary>
            public byte flags;
            /// <summary>EPB signature, 0x28</summary>
            public byte signature;
            /// <summary>Volume serial number</summary>
            public uint serial_no;
            /// <summary>Boot code.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 467)] public byte[] boot_code;
            /// <summary>Always 0x55 0xAA.</summary>
            public ushort boot_signature;
        }

        /// <summary>DOS 4.0 or higher BIOS Parameter Block.</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BiosParameterBlockEbpb
        {
            /// <summary>x86 jump</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] jump;
            /// <summary>OEM Name, 8 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] oem_name;
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors per cluster</summary>
            public byte spc;
            /// <summary>Reserved sectors between BPB and FAT</summary>
            public ushort rsectors;
            /// <summary>Number of FATs</summary>
            public byte fats_no;
            /// <summary>Number of entries on root directory</summary>
            public ushort root_ent;
            /// <summary>Sectors in volume</summary>
            public ushort sectors;
            /// <summary>Media descriptor</summary>
            public byte media;
            /// <summary>Sectors per FAT</summary>
            public ushort spfat;
            /// <summary>Sectors per track</summary>
            public ushort sptrk;
            /// <summary>Heads</summary>
            public ushort heads;
            /// <summary>Hidden sectors before BPB</summary>
            public uint hsectors;
            /// <summary>Sectors in volume if > 65535</summary>
            public uint big_sectors;
            /// <summary>Drive number</summary>
            public byte drive_no;
            /// <summary>Volume flags</summary>
            public byte flags;
            /// <summary>EPB signature, 0x29</summary>
            public byte signature;
            /// <summary>Volume serial number</summary>
            public uint serial_no;
            /// <summary>Volume label, 11 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)] public byte[] volume_label;
            /// <summary>Filesystem type, 8 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] fs_type;
            /// <summary>Boot code.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 448)] public byte[] boot_code;
            /// <summary>Always 0x55 0xAA.</summary>
            public ushort boot_signature;
        }

        /// <summary>FAT32 Parameter Block</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Fat32ParameterBlockShort
        {
            /// <summary>x86 jump</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] jump;
            /// <summary>OEM Name, 8 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] oem_name;
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors per cluster</summary>
            public byte spc;
            /// <summary>Reserved sectors between BPB and FAT</summary>
            public ushort rsectors;
            /// <summary>Number of FATs</summary>
            public byte fats_no;
            /// <summary>Number of entries on root directory, set to 0</summary>
            public ushort root_ent;
            /// <summary>Sectors in volume, set to 0</summary>
            public ushort sectors;
            /// <summary>Media descriptor</summary>
            public byte media;
            /// <summary>Sectors per FAT, set to 0</summary>
            public ushort spfat;
            /// <summary>Sectors per track</summary>
            public ushort sptrk;
            /// <summary>Heads</summary>
            public ushort heads;
            /// <summary>Hidden sectors before BPB</summary>
            public uint hsectors;
            /// <summary>Sectors in volume</summary>
            public uint big_sectors;
            /// <summary>Sectors per FAT</summary>
            public uint big_spfat;
            /// <summary>FAT flags</summary>
            public ushort mirror_flags;
            /// <summary>FAT32 version</summary>
            public ushort version;
            /// <summary>Cluster of root directory</summary>
            public uint root_cluster;
            /// <summary>Sector of FSINFO structure</summary>
            public ushort fsinfo_sector;
            /// <summary>Sector of FAT32PB backup</summary>
            public ushort backup_sector;
            /// <summary>Reserved</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public byte[] reserved;
            /// <summary>Drive number</summary>
            public byte drive_no;
            /// <summary>Volume flags</summary>
            public byte flags;
            /// <summary>Signature, should be 0x28</summary>
            public byte signature;
            /// <summary>Volume serial number</summary>
            public uint serial_no;
            /// <summary>Volume label, 11 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)] public byte[] reserved2;
            /// <summary>Sectors in volume if <see cref="big_sectors" /> equals 0</summary>
            public ulong huge_sectors;
            /// <summary>Boot code.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 420)] public byte[] boot_code;
            /// <summary>Always 0x55 0xAA.</summary>
            public ushort boot_signature;
        }

        /// <summary>FAT32 Parameter Block</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Fat32ParameterBlock
        {
            /// <summary>x86 jump</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] jump;
            /// <summary>OEM Name, 8 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] oem_name;
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors per cluster</summary>
            public byte spc;
            /// <summary>Reserved sectors between BPB and FAT</summary>
            public ushort rsectors;
            /// <summary>Number of FATs</summary>
            public byte fats_no;
            /// <summary>Number of entries on root directory, set to 0</summary>
            public ushort root_ent;
            /// <summary>Sectors in volume, set to 0</summary>
            public ushort sectors;
            /// <summary>Media descriptor</summary>
            public byte media;
            /// <summary>Sectors per FAT, set to 0</summary>
            public ushort spfat;
            /// <summary>Sectors per track</summary>
            public ushort sptrk;
            /// <summary>Heads</summary>
            public ushort heads;
            /// <summary>Hidden sectors before BPB</summary>
            public uint hsectors;
            /// <summary>Sectors in volume</summary>
            public uint big_sectors;
            /// <summary>Sectors per FAT</summary>
            public uint big_spfat;
            /// <summary>FAT flags</summary>
            public ushort mirror_flags;
            /// <summary>FAT32 version</summary>
            public ushort version;
            /// <summary>Cluster of root directory</summary>
            public uint root_cluster;
            /// <summary>Sector of FSINFO structure</summary>
            public ushort fsinfo_sector;
            /// <summary>Sector of FAT32PB backup</summary>
            public ushort backup_sector;
            /// <summary>Reserved</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public byte[] reserved;
            /// <summary>Drive number</summary>
            public byte drive_no;
            /// <summary>Volume flags</summary>
            public byte flags;
            /// <summary>Signature, should be 0x29</summary>
            public byte signature;
            /// <summary>Volume serial number</summary>
            public uint serial_no;
            /// <summary>Volume label, 11 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)] public byte[] volume_label;
            /// <summary>Filesystem type, 8 bytes, space-padded, must be "FAT32   "</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] fs_type;
            /// <summary>Boot code.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 419)] public byte[] boot_code;
            /// <summary>Always 0x55 0xAA.</summary>
            public ushort boot_signature;
        }

        /// <summary>Apricot Label.</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ApricotLabel
        {
            /// <summary>Version of format which created disk</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] version;
            /// <summary>Operating system.</summary>
            public byte operatingSystem;
            /// <summary>Software write protection.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool writeProtected;
            /// <summary>Copy protected.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool copyProtected;
            /// <summary>Boot type.</summary>
            public byte bootType;
            /// <summary>Partitions.</summary>
            public byte partitionCount;
            /// <summary>Is hard disk?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool winchester;
            /// <summary>Sector size.</summary>
            public ushort sectorSize;
            /// <summary>Sectors per track.</summary>
            public ushort spt;
            /// <summary>Tracks per side.</summary>
            public uint cylinders;
            /// <summary>Sides.</summary>
            public byte heads;
            /// <summary>Interleave factor.</summary>
            public byte interleave;
            /// <summary>Skew factor.</summary>
            public ushort skew;
            /// <summary>Sector where boot code starts.</summary>
            public uint bootLocation;
            /// <summary>Size in sectors of boot code.</summary>
            public ushort bootSize;
            /// <summary>Address at which to load boot code.</summary>
            public uint bootAddress;
            /// <summary>Offset where to jump to boot.</summary>
            public ushort bootOffset;
            /// <summary>Segment where to jump to boot.</summary>
            public ushort bootSegment;
            /// <summary>First data sector.</summary>
            public uint firstDataBlock;
            /// <summary>Generation.</summary>
            public ushort generation;
            /// <summary>Copy count.</summary>
            public ushort copyCount;
            /// <summary>Maximum number of copies.</summary>
            public ushort maxCopies;
            /// <summary>Serial number.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] serialNumber;
            /// <summary>Part number.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] partNumber;
            /// <summary>Copyright.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)] public byte[] copyright;
            /// <summary>BPB for whole disk.</summary>
            public ApricotParameterBlock mainBPB;
            /// <summary>Name of FONT file.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] fontName;
            /// <summary>Name of KEYBOARD file.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] keyboardName;
            /// <summary>Minor BIOS version.</summary>
            public byte biosMinorVersion;
            /// <summary>Major BIOS version.</summary>
            public byte biosMajorVersion;
            /// <summary>Diagnostics enabled?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool diagnosticsFlag;
            /// <summary>Printer device.</summary>
            public byte prnDevice;
            /// <summary>Bell volume.</summary>
            public byte bellVolume;
            /// <summary>Cache enabled?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool enableCache;
            /// <summary>Graphics enabled?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool enableGraphics;
            /// <summary>Length in sectors of DOS.</summary>
            public byte dosLength;
            /// <summary>Length in sectors of FONT file.</summary>
            public byte fontLength;
            /// <summary>Length in sectors of KEYBOARD file.</summary>
            public byte keyboardLength;
            /// <summary>Starting sector of DOS.</summary>
            public ushort dosStart;
            /// <summary>Starting sector of FONT file.</summary>
            public ushort fontStart;
            /// <summary>Starting sector of KEYBOARD file.</summary>
            public ushort keyboardStart;
            /// <summary>Keyboard click volume.</summary>
            public byte keyboardVolume;
            /// <summary>Auto-repeat enabled?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool autorepeat;
            /// <summary>Auto-repeat lead-in.</summary>
            public byte autorepeatLeadIn;
            /// <summary>Auto-repeat interval.</summary>
            public byte autorepeatInterval;
            /// <summary>Microscreen mode.</summary>
            public byte microscreenMode;
            /// <summary>Spare area for keyboard values expansion.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)] public byte[] spareKeyboard;
            /// <summary>Screen line mode.</summary>
            public byte lineMode;
            /// <summary>Screen line width.</summary>
            public byte lineWidth;
            /// <summary>Screen disabled?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool imageOff;
            /// <summary>Spare area for screen values expansion.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)] public byte[] spareScreen;
            /// <summary>TX baud rate.</summary>
            public byte txBaudRate;
            /// <summary>RX baud rate.</summary>
            public byte rxBaudRate;
            /// <summary>TX bits.</summary>
            public byte txBits;
            /// <summary>RX bits.</summary>
            public byte rxBits;
            /// <summary>Stop bits.</summary>
            public byte stopBits;
            /// <summary>Parity enabled?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool parityCheck;
            /// <summary>Parity type.</summary>
            public byte parityType;
            /// <summary>Xon/Xoff enabled on TX.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool txXonXoff;
            /// <summary>Xon/Xoff enabled on RX.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool rxXonXoff;
            /// <summary>Xon character.</summary>
            public byte xonCharacter;
            /// <summary>Xoff character.</summary>
            public byte xoffCharacter;
            /// <summary>Xon/Xoff buffer on RX.</summary>
            public ushort rxXonXoffBuffer;
            /// <summary>DTR/DSR enabled?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool dtrDsr;
            /// <summary>CTS/RTS enabled?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool ctsRts;
            /// <summary>NULLs after CR.</summary>
            public byte nullsAfterCr;
            /// <summary>NULLs after 0xFF.</summary>
            public byte nullsAfterFF;
            /// <summary>Send LF after CR in serial port.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool lfAfterCRSerial;
            /// <summary>BIOS error report in serial port.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool biosErrorReportSerial;
            /// <summary>Spare area for serial port values expansion.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)] public byte[] spareSerial;
            /// <summary>Send LF after CR in parallel port.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool lfAfterCrParallel;
            /// <summary>Select line supported?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool selectLine;
            /// <summary>Paper empty supported?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool paperEmpty;
            /// <summary>Fault line supported?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool faultLine;
            /// <summary>BIOS error report in parallel port.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool biosErrorReportParallel;
            /// <summary>Spare area for parallel port values expansion.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)] public byte[] spareParallel;
            /// <summary>Spare area for Winchester values expansion.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)] public byte[] spareWinchester;
            /// <summary>Parking enabled?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool parkingEnabled;
            /// <summary>Format protection?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool formatProtection;
            /// <summary>Spare area for RAM disk values expansion.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] spareRamDisk;
            /// <summary>List of bad blocks.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public ushort[] badBlocks;
            /// <summary>Array of partition BPBs.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public ApricotParameterBlock[] partitions;
            /// <summary>Spare area.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 63)] public byte[] spare;
            /// <summary>CP/M double side indicator?.</summary>
            public bool cpmDoubleSided;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ApricotParameterBlock
        {
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors per cluster</summary>
            public byte spc;
            /// <summary>Reserved sectors between BPB and FAT</summary>
            public ushort rsectors;
            /// <summary>Number of FATs</summary>
            public byte fats_no;
            /// <summary>Number of entries on root directory</summary>
            public ushort root_ent;
            /// <summary>Sectors in volume</summary>
            public ushort sectors;
            /// <summary>Media descriptor</summary>
            public byte media;
            /// <summary>Sectors per FAT</summary>
            public ushort spfat;
            /// <summary>Disk type</summary>
            public byte diskType;
            /// <summary>Volume starting sector</summary>
            public ushort startSector;
        }

        /// <summary>FAT32 FS Information Sector</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct FsInfoSector
        {
            /// <summary>Signature must be <see cref="FAT.FSINFO_SIGNATURE1" /></summary>
            public uint signature1;
            /// <summary>Reserved</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 480)] public byte[] reserved1;
            /// <summary>Signature must be <see cref="FAT.FSINFO_SIGNATURE2" /></summary>
            public uint signature2;
            /// <summary>Free clusters</summary>
            public uint free_clusters;
            /// <summary>  cated cluster</summary>
            public uint last_cluster;
            /// <summary>Reserved</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public byte[] reserved2;
            /// <summary>Signature must be <see cref="FAT.FSINFO_SIGNATURE3" /></summary>
            public uint signature3;
        }

        [Flags]
        enum FatAttributes : byte
        {
            ReadOnly = 0x01,
            Hidden = 0x02,
            System = 0x04,
            VolumeLabel = 0x08,
            Subdirectory = 0x10,
            Archive = 0x20,
            Device = 0x40,
            Reserved = 0x80,
            LFN = 0x0F
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DirectoryEntry
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] filename;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] extension;
            public FatAttributes attributes;
            public byte caseinfo;
            public byte ctime_ms;
            public ushort ctime;
            public ushort cdate;
            public ushort adate;
            public ushort ea_handle;
            public ushort mtime;
            public ushort mdate;
            public ushort start_cluster;
            public uint size;
        }
    }
}