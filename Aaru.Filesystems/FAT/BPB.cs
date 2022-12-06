// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BPB.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft FAT filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Microsoft FAT Boot Parameter Block variant.
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
using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Filesystems
{
    public sealed partial class FAT
    {
        static BpbKind DetectBpbKind(byte[] bpbSector, IMediaImage imagePlugin, Partition partition,
                                     out BiosParameterBlockEbpb fakeBpb, out HumanParameterBlock humanBpb,
                                     out AtariParameterBlock atariBpb, out byte minBootNearJump,
                                     out bool andosOemCorrect, out bool bootable)
        {
            fakeBpb         = new BiosParameterBlockEbpb();
            minBootNearJump = 0;
            andosOemCorrect = false;
            bootable        = false;

            humanBpb = Marshal.ByteArrayToStructureBigEndian<HumanParameterBlock>(bpbSector);
            atariBpb = Marshal.ByteArrayToStructureLittleEndian<AtariParameterBlock>(bpbSector);

            ulong expectedClusters = humanBpb.bpc > 0 ? partition.Size / humanBpb.bpc : 0;

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
            bool humanBranchCorrect = bpbSector[0] == 0x60 && bpbSector[1] >= 0x1C && bpbSector[1] < 0xFE;

            AaruConsole.DebugWriteLine("FAT plugin", "humanClustersCorrect = {0}", humanClustersCorrect);
            AaruConsole.DebugWriteLine("FAT plugin", "humanOemCorrect = {0}", humanOemCorrect);
            AaruConsole.DebugWriteLine("FAT plugin", "humanBranchCorrect = {0}", humanBranchCorrect);

            // If all Human68k checks are correct, it is a Human68k FAT16
            bool useHumanBpb = humanClustersCorrect && humanOemCorrect && humanBranchCorrect && expectedClusters > 0;

            if(useHumanBpb)
            {
                AaruConsole.DebugWriteLine("FAT plugin", "Using Human68k BPB");

                fakeBpb.jump        = humanBpb.jump;
                fakeBpb.oem_name    = humanBpb.oem_name;
                fakeBpb.bps         = (ushort)imagePlugin.Info.SectorSize;
                fakeBpb.spc         = (byte)(humanBpb.bpc / fakeBpb.bps);
                fakeBpb.fats_no     = 2;
                fakeBpb.root_ent    = humanBpb.root_ent;
                fakeBpb.media       = humanBpb.media;
                fakeBpb.spfat       = (ushort)(humanBpb.cpfat * fakeBpb.spc);
                fakeBpb.boot_code   = humanBpb.boot_code;
                fakeBpb.sectors     = humanBpb.clusters;
                fakeBpb.big_sectors = humanBpb.big_clusters;
                fakeBpb.rsectors    = 1;

                return BpbKind.Human;
            }

            var msxBpb     = new MsxParameterBlock();
            var dos2Bpb    = new BiosParameterBlock2();
            var dos30Bpb   = new BiosParameterBlock30();
            var dos32Bpb   = new BiosParameterBlock32();
            var dos33Bpb   = new BiosParameterBlock33();
            var shortEbpb  = new BiosParameterBlockShortEbpb();
            var ebpb       = new BiosParameterBlockEbpb();
            var apricotBpb = new ApricotLabel();

            bool useAtariBpb          = false;
            bool useMsxBpb            = false;
            bool useDos2Bpb           = false;
            bool useDos3Bpb           = false;
            bool useDos32Bpb          = false;
            bool useDos33Bpb          = false;
            bool userShortExtendedBpb = false;
            bool useExtendedBpb       = false;
            bool useShortFat32        = false;
            bool useLongFat32         = false;
            bool useApricotBpb        = false;
            bool useDecRainbowBpb     = false;

            if(imagePlugin.Info.SectorSize >= 256)
            {
                msxBpb    = Marshal.ByteArrayToStructureLittleEndian<MsxParameterBlock>(bpbSector);
                dos2Bpb   = Marshal.ByteArrayToStructureLittleEndian<BiosParameterBlock2>(bpbSector);
                dos30Bpb  = Marshal.ByteArrayToStructureLittleEndian<BiosParameterBlock30>(bpbSector);
                dos32Bpb  = Marshal.ByteArrayToStructureLittleEndian<BiosParameterBlock32>(bpbSector);
                dos33Bpb  = Marshal.ByteArrayToStructureLittleEndian<BiosParameterBlock33>(bpbSector);
                shortEbpb = Marshal.ByteArrayToStructureLittleEndian<BiosParameterBlockShortEbpb>(bpbSector);
                ebpb      = Marshal.ByteArrayToStructureLittleEndian<BiosParameterBlockEbpb>(bpbSector);

                Fat32ParameterBlockShort shortFat32Bpb =
                    Marshal.ByteArrayToStructureLittleEndian<Fat32ParameterBlockShort>(bpbSector);

                Fat32ParameterBlock fat32Bpb = Marshal.ByteArrayToStructureLittleEndian<Fat32ParameterBlock>(bpbSector);
                apricotBpb = Marshal.ByteArrayToStructureLittleEndian<ApricotLabel>(bpbSector);

                int bitsInBpsMsx        = CountBits.Count(msxBpb.bps);
                int bitsInBpsDos33      = CountBits.Count(dos33Bpb.bps);
                int bitsInBpsDos40      = CountBits.Count(ebpb.bps);
                int bitsInBpsFat32Short = CountBits.Count(shortFat32Bpb.bps);
                int bitsInBpsFat32      = CountBits.Count(fat32Bpb.bps);
                int bitsInBpsApricot    = CountBits.Count(apricotBpb.mainBPB.bps);

                bool correctSpcMsx = msxBpb.spc == 1  || msxBpb.spc == 2  || msxBpb.spc == 4 || msxBpb.spc == 8 ||
                                     msxBpb.spc == 16 || msxBpb.spc == 32 || msxBpb.spc == 64;

                bool correctSpcDos33 = dos33Bpb.spc == 1 || dos33Bpb.spc == 2  || dos33Bpb.spc == 4  ||
                                       dos33Bpb.spc == 8 || dos33Bpb.spc == 16 || dos33Bpb.spc == 32 ||
                                       dos33Bpb.spc == 64;

                bool correctSpcDos40 = ebpb.spc == 1  || ebpb.spc == 2  || ebpb.spc == 4 || ebpb.spc == 8 ||
                                       ebpb.spc == 16 || ebpb.spc == 32 || ebpb.spc == 64;

                bool correctSpcFat32Short = shortFat32Bpb.spc == 1  || shortFat32Bpb.spc == 2  ||
                                            shortFat32Bpb.spc == 4  || shortFat32Bpb.spc == 8  ||
                                            shortFat32Bpb.spc == 16 || shortFat32Bpb.spc == 32 ||
                                            shortFat32Bpb.spc == 64;

                bool correctSpcFat32 = fat32Bpb.spc == 1 || fat32Bpb.spc == 2  || fat32Bpb.spc == 4  ||
                                       fat32Bpb.spc == 8 || fat32Bpb.spc == 16 || fat32Bpb.spc == 32 ||
                                       fat32Bpb.spc == 64;

                bool correctSpcApricot = apricotBpb.mainBPB.spc == 1  || apricotBpb.mainBPB.spc == 2  ||
                                         apricotBpb.mainBPB.spc == 4  || apricotBpb.mainBPB.spc == 8  ||
                                         apricotBpb.mainBPB.spc == 16 || apricotBpb.mainBPB.spc == 32 ||
                                         apricotBpb.mainBPB.spc == 64;

                // This is to support FAT partitions on hybrid ISO/USB images
                if(imagePlugin.Info.XmlMediaType == XmlMediaType.OpticalDisc)
                {
                    atariBpb.sectors           /= 4;
                    msxBpb.sectors             /= 4;
                    dos2Bpb.sectors            /= 4;
                    dos30Bpb.sectors           /= 4;
                    dos32Bpb.sectors           /= 4;
                    dos33Bpb.sectors           /= 4;
                    dos33Bpb.big_sectors       /= 4;
                    shortEbpb.sectors          /= 4;
                    shortEbpb.big_sectors      /= 4;
                    ebpb.sectors               /= 4;
                    ebpb.big_sectors           /= 4;
                    shortFat32Bpb.sectors      /= 4;
                    shortFat32Bpb.big_sectors  /= 4;
                    shortFat32Bpb.huge_sectors /= 4;
                    fat32Bpb.sectors           /= 4;
                    fat32Bpb.big_sectors       /= 4;
                    apricotBpb.mainBPB.sectors /= 4;
                }

                andosOemCorrect = dos33Bpb.oem_name[0] < 0x20  && dos33Bpb.oem_name[1] >= 0x20 &&
                                  dos33Bpb.oem_name[2] >= 0x20 && dos33Bpb.oem_name[3] >= 0x20 &&
                                  dos33Bpb.oem_name[4] >= 0x20 && dos33Bpb.oem_name[5] >= 0x20 &&
                                  dos33Bpb.oem_name[6] >= 0x20 && dos33Bpb.oem_name[7] >= 0x20;

                if(bitsInBpsFat32 == 1                                &&
                   correctSpcFat32                                    &&
                   fat32Bpb.fats_no                           <= 2    &&
                   fat32Bpb.spfat                             == 0    &&
                   fat32Bpb.signature                         == 0x29 &&
                   Encoding.ASCII.GetString(fat32Bpb.fs_type) == "FAT32   ")
                {
                    AaruConsole.DebugWriteLine("FAT plugin", "Using FAT32 BPB");
                    minBootNearJump = 0x58;

                    return BpbKind.LongFat32;
                }

                if(bitsInBpsFat32Short == 1     &&
                   correctSpcFat32Short         &&
                   shortFat32Bpb.fats_no   <= 2 &&
                   shortFat32Bpb.sectors   == 0 &&
                   shortFat32Bpb.spfat     == 0 &&
                   shortFat32Bpb.signature == 0x28)
                {
                    AaruConsole.DebugWriteLine("FAT plugin", "Using short FAT32 BPB");

                    minBootNearJump = 0x57;

                    return BpbKind.ShortFat32;
                }

                if(bitsInBpsMsx == 1                                                              &&
                   correctSpcMsx                                                                  &&
                   msxBpb.fats_no                          <= 2                                   &&
                   msxBpb.root_ent                         > 0                                    &&
                   msxBpb.sectors                          <= partition.End - partition.Start + 1 &&
                   msxBpb.spfat                            > 0                                    &&
                   Encoding.ASCII.GetString(msxBpb.vol_id) == "VOL_ID")
                {
                    AaruConsole.DebugWriteLine("FAT plugin", "Using MSX BPB");
                    useMsxBpb = true;
                }
                else if(bitsInBpsApricot == 1                                              &&
                        correctSpcApricot                                                  &&
                        apricotBpb.mainBPB.fats_no  <= 2                                   &&
                        apricotBpb.mainBPB.root_ent > 0                                    &&
                        apricotBpb.mainBPB.sectors  <= partition.End - partition.Start + 1 &&
                        apricotBpb.mainBPB.spfat    > 0                                    &&
                        apricotBpb.partitionCount   == 0)
                {
                    AaruConsole.DebugWriteLine("FAT plugin", "Using Apricot BPB");
                    useApricotBpb = true;
                }
                else if(bitsInBpsDos40 == 1 &&
                        correctSpcDos40     &&
                        ebpb.fats_no  <= 2  &&
                        ebpb.root_ent > 0   &&
                        ebpb.spfat    > 0   &&
                        (ebpb.signature == 0x28 || ebpb.signature == 0x29 || andosOemCorrect))
                {
                    if(ebpb.sectors == 0)
                    {
                        if(ebpb.big_sectors <= partition.End - partition.Start + 1)
                            if(ebpb.signature == 0x29 || andosOemCorrect)
                            {
                                AaruConsole.DebugWriteLine("FAT plugin", "Using DOS 4.0 BPB");
                                useExtendedBpb  = true;
                                minBootNearJump = 0x3C;
                            }
                            else
                            {
                                AaruConsole.DebugWriteLine("FAT plugin", "Using DOS 3.4 BPB");
                                userShortExtendedBpb = true;
                                minBootNearJump      = 0x29;
                            }
                    }
                    else if(ebpb.sectors <= partition.End - partition.Start + 1)
                        if(ebpb.signature == 0x29 || andosOemCorrect)
                        {
                            AaruConsole.DebugWriteLine("FAT plugin", "Using DOS 4.0 BPB");
                            useExtendedBpb  = true;
                            minBootNearJump = 0x3C;
                        }
                        else
                        {
                            AaruConsole.DebugWriteLine("FAT plugin", "Using DOS 3.4 BPB");
                            userShortExtendedBpb = true;
                            minBootNearJump      = 0x29;
                        }
                }
                else if(bitsInBpsDos33 == 1                                 &&
                        correctSpcDos33                                     &&
                        dos33Bpb.rsectors < partition.End - partition.Start &&
                        dos33Bpb.fats_no  <= 2                              &&
                        dos33Bpb.root_ent > 0                               &&
                        dos33Bpb.spfat    > 0)
                    if(dos33Bpb.sectors     == 0               &&
                       dos33Bpb.hsectors    <= partition.Start &&
                       dos33Bpb.big_sectors > 0                &&
                       dos33Bpb.big_sectors <= partition.End - partition.Start + 1)
                    {
                        AaruConsole.DebugWriteLine("FAT plugin", "Using DOS 3.3 BPB");
                        useDos33Bpb     = true;
                        minBootNearJump = 0x22;
                    }
                    else if(dos33Bpb.big_sectors == 0               &&
                            dos33Bpb.hsectors    <= partition.Start &&
                            dos33Bpb.sectors     > 0                &&
                            dos33Bpb.sectors     <= partition.End - partition.Start + 1)
                        if(atariBpb.jump[0] == 0x60 ||
                           (atariBpb.jump[0]                            == 0xE9 && atariBpb.jump[1] == 0x00 &&
                            Encoding.ASCII.GetString(dos33Bpb.oem_name) != "NEXT    ") ||
                           partition.Type == "GEM"                                     ||
                           partition.Type == "BGM")
                        {
                            AaruConsole.DebugWriteLine("FAT plugin", "Using Atari BPB");
                            useAtariBpb = true;
                        }
                        else
                        {
                            AaruConsole.DebugWriteLine("FAT plugin", "Using DOS 3.3 BPB");
                            useDos33Bpb     = true;
                            minBootNearJump = 0x22;
                        }
                    else
                    {
                        if(dos32Bpb.hsectors                    <= partition.Start &&
                           dos32Bpb.hsectors + dos32Bpb.sectors == dos32Bpb.total_sectors)
                        {
                            AaruConsole.DebugWriteLine("FAT plugin", "Using DOS 3.2 BPB");
                            useDos32Bpb     = true;
                            minBootNearJump = 0x1E;
                        }
                        else if(dos30Bpb.sptrk > 0  &&
                                dos30Bpb.sptrk < 64 &&
                                dos30Bpb.heads > 0  &&
                                dos30Bpb.heads < 256)
                            if(atariBpb.jump[0] == 0x60 ||
                               (atariBpb.jump[0]                            == 0xE9 && atariBpb.jump[1] == 0x00 &&
                                Encoding.ASCII.GetString(dos33Bpb.oem_name) != "NEXT    "))
                            {
                                AaruConsole.DebugWriteLine("FAT plugin", "Using Atari BPB");
                                useAtariBpb = true;
                            }
                            else
                            {
                                AaruConsole.DebugWriteLine("FAT plugin", "Using DOS 3.0 BPB");
                                useDos3Bpb      = true;
                                minBootNearJump = 0x1C;
                            }
                        else
                        {
                            if(atariBpb.jump[0] == 0x60 ||
                               (atariBpb.jump[0]                            == 0xE9 && atariBpb.jump[1] == 0x00 &&
                                Encoding.ASCII.GetString(dos33Bpb.oem_name) != "NEXT    "))
                            {
                                AaruConsole.DebugWriteLine("FAT plugin", "Using Atari BPB");
                                useAtariBpb = true;
                            }
                            else
                            {
                                AaruConsole.DebugWriteLine("FAT plugin", "Using DOS 2.0 BPB");
                                useDos2Bpb      = true;
                                minBootNearJump = 0x16;
                            }
                        }
                    }
            }

            // DEC Rainbow, lacks a BPB but has a very concrete structure...
            if(imagePlugin.Info.Sectors    == 800 &&
               imagePlugin.Info.SectorSize == 512 &&
               !useAtariBpb                       &&
               !useMsxBpb                         &&
               !useDos2Bpb                        &&
               !useDos3Bpb                        &&
               !useDos32Bpb                       &&
               !useDos33Bpb                       &&
               !userShortExtendedBpb              &&
               !useExtendedBpb                    &&
               !useShortFat32                     &&
               !useLongFat32                      &&
               !useApricotBpb)
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

                foreach(byte[] tmp in from ulong rootSector in new[]
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
                {
                    AaruConsole.DebugWriteLine("FAT plugin", "Using DEC Rainbow hardcoded BPB.");
                    fakeBpb.bps       = 512;
                    fakeBpb.spc       = 1;
                    fakeBpb.rsectors  = 20;
                    fakeBpb.fats_no   = 2;
                    fakeBpb.root_ent  = 96;
                    fakeBpb.sectors   = 800;
                    fakeBpb.media     = 0xFA;
                    fakeBpb.sptrk     = 10;
                    fakeBpb.heads     = 1;
                    fakeBpb.hsectors  = 0;
                    fakeBpb.spfat     = 3;
                    bootable          = true;
                    fakeBpb.boot_code = bpbSector;

                    return BpbKind.DecRainbow;
                }
            }

            if(!useAtariBpb          &&
               !useMsxBpb            &&
               !useDos2Bpb           &&
               !useDos3Bpb           &&
               !useDos32Bpb          &&
               !useDos33Bpb          &&
               !useHumanBpb          &&
               !userShortExtendedBpb &&
               !useExtendedBpb       &&
               !useShortFat32        &&
               !useLongFat32         &&
               !useApricotBpb        &&
               !useDecRainbowBpb)
            {
                byte[] fatSector = imagePlugin.ReadSector(1 + partition.Start);

                switch(fatSector[0])
                {
                    case 0xE5:
                        if(imagePlugin.Info.Sectors    == 2002 &&
                           imagePlugin.Info.SectorSize == 128)
                        {
                            AaruConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB.");
                            fakeBpb.bps      = 128;
                            fakeBpb.spc      = 4;
                            fakeBpb.rsectors = 1;
                            fakeBpb.fats_no  = 2;
                            fakeBpb.root_ent = 64;
                            fakeBpb.sectors  = 2002;
                            fakeBpb.media    = 0xE5;
                            fakeBpb.sptrk    = 26;
                            fakeBpb.heads    = 1;
                            fakeBpb.hsectors = 0;
                            fakeBpb.spfat    = 1;
                        }

                        break;
                    case 0xFD:
                        if(imagePlugin.Info.Sectors    == 4004 &&
                           imagePlugin.Info.SectorSize == 128)
                        {
                            AaruConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB.");
                            fakeBpb.bps      = 128;
                            fakeBpb.spc      = 4;
                            fakeBpb.rsectors = 4;
                            fakeBpb.fats_no  = 2;
                            fakeBpb.root_ent = 68;
                            fakeBpb.sectors  = 4004;
                            fakeBpb.media    = 0xFD;
                            fakeBpb.sptrk    = 26;
                            fakeBpb.heads    = 2;
                            fakeBpb.hsectors = 0;
                            fakeBpb.spfat    = 6;
                        }
                        else if(imagePlugin.Info.Sectors    == 2002 &&
                                imagePlugin.Info.SectorSize == 128)
                        {
                            AaruConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB.");
                            fakeBpb.bps      = 128;
                            fakeBpb.spc      = 4;
                            fakeBpb.rsectors = 4;
                            fakeBpb.fats_no  = 2;
                            fakeBpb.root_ent = 68;
                            fakeBpb.sectors  = 2002;
                            fakeBpb.media    = 0xFD;
                            fakeBpb.sptrk    = 26;
                            fakeBpb.heads    = 1;
                            fakeBpb.hsectors = 0;
                            fakeBpb.spfat    = 6;
                        }

                        break;
                    case 0xFE:
                        switch(imagePlugin.Info.Sectors)
                        {
                            case 320 when imagePlugin.Info.SectorSize == 512:
                                AaruConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB for 5.25\" SSDD.");
                                fakeBpb.bps      = 512;
                                fakeBpb.spc      = 1;
                                fakeBpb.rsectors = 1;
                                fakeBpb.fats_no  = 2;
                                fakeBpb.root_ent = 64;
                                fakeBpb.sectors  = 320;
                                fakeBpb.media    = 0xFE;
                                fakeBpb.sptrk    = 8;
                                fakeBpb.heads    = 1;
                                fakeBpb.hsectors = 0;
                                fakeBpb.spfat    = 1;

                                break;
                            case 2002 when imagePlugin.Info.SectorSize == 128:
                                AaruConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB.");
                                fakeBpb.bps      = 128;
                                fakeBpb.spc      = 4;
                                fakeBpb.rsectors = 1;
                                fakeBpb.fats_no  = 2;
                                fakeBpb.root_ent = 68;
                                fakeBpb.sectors  = 2002;
                                fakeBpb.media    = 0xFE;
                                fakeBpb.sptrk    = 26;
                                fakeBpb.heads    = 1;
                                fakeBpb.hsectors = 0;
                                fakeBpb.spfat    = 6;

                                break;
                            case 1232 when imagePlugin.Info.SectorSize == 1024:
                                AaruConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB.");
                                fakeBpb.bps      = 1024;
                                fakeBpb.spc      = 1;
                                fakeBpb.rsectors = 1;
                                fakeBpb.fats_no  = 2;
                                fakeBpb.root_ent = 192;
                                fakeBpb.sectors  = 1232;
                                fakeBpb.media    = 0xFE;
                                fakeBpb.sptrk    = 8;
                                fakeBpb.heads    = 2;
                                fakeBpb.hsectors = 0;
                                fakeBpb.spfat    = 2;

                                break;
                            case 616 when imagePlugin.Info.SectorSize == 1024:
                                AaruConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB.");
                                fakeBpb.bps      = 1024;
                                fakeBpb.spc      = 1;
                                fakeBpb.rsectors = 1;
                                fakeBpb.fats_no  = 2;
                                fakeBpb.root_ent = 6192;
                                fakeBpb.sectors  = 616;
                                fakeBpb.media    = 0xFE;
                                fakeBpb.sptrk    = 8;
                                fakeBpb.heads    = 2;
                                fakeBpb.hsectors = 0;

                                break;
                            case 720 when imagePlugin.Info.SectorSize == 128:
                                AaruConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB.");
                                fakeBpb.bps      = 128;
                                fakeBpb.spc      = 2;
                                fakeBpb.rsectors = 54;
                                fakeBpb.fats_no  = 2;
                                fakeBpb.root_ent = 64;
                                fakeBpb.sectors  = 720;
                                fakeBpb.media    = 0xFE;
                                fakeBpb.sptrk    = 18;
                                fakeBpb.heads    = 1;
                                fakeBpb.hsectors = 0;
                                fakeBpb.spfat    = 4;

                                break;
                            case 640 when imagePlugin.Info.SectorSize == 512:
                                AaruConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB for 5.25\" DSDD.");
                                fakeBpb.bps      = 512;
                                fakeBpb.spc      = 2;
                                fakeBpb.rsectors = 1;
                                fakeBpb.fats_no  = 2;
                                fakeBpb.root_ent = 112;
                                fakeBpb.sectors  = 640;
                                fakeBpb.media    = 0xFF;
                                fakeBpb.sptrk    = 8;
                                fakeBpb.heads    = 2;
                                fakeBpb.hsectors = 0;
                                fakeBpb.spfat    = 1;

                                break;
                        }

                        break;
                    case 0xFF:
                        if(imagePlugin.Info.Sectors    == 640 &&
                           imagePlugin.Info.SectorSize == 512)
                        {
                            AaruConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB for 5.25\" DSDD.");
                            fakeBpb.bps      = 512;
                            fakeBpb.spc      = 2;
                            fakeBpb.rsectors = 1;
                            fakeBpb.fats_no  = 2;
                            fakeBpb.root_ent = 112;
                            fakeBpb.sectors  = 640;
                            fakeBpb.media    = 0xFF;
                            fakeBpb.sptrk    = 8;
                            fakeBpb.heads    = 2;
                            fakeBpb.hsectors = 0;
                            fakeBpb.spfat    = 1;
                        }

                        break;
                }

                // This assumes a bootable sector will jump somewhere or disable interrupts in x86 code
                bootable |= bpbSector[0] == 0xFA || (bpbSector[0] == 0xEB && bpbSector[1] <= 0x7F) ||
                            (bpbSector[0] == 0xE9 && BitConverter.ToUInt16(bpbSector, 1) <= 0x1FC);

                fakeBpb.boot_code = bpbSector;

                return BpbKind.Hardcoded;
            }

            if(useExtendedBpb)
            {
                fakeBpb = ebpb;

                return BpbKind.Extended;
            }

            if(userShortExtendedBpb)
            {
                fakeBpb.jump           = shortEbpb.jump;
                fakeBpb.oem_name       = shortEbpb.oem_name;
                fakeBpb.bps            = shortEbpb.bps;
                fakeBpb.spc            = shortEbpb.spc;
                fakeBpb.rsectors       = shortEbpb.rsectors;
                fakeBpb.fats_no        = shortEbpb.fats_no;
                fakeBpb.root_ent       = shortEbpb.root_ent;
                fakeBpb.sectors        = shortEbpb.sectors;
                fakeBpb.media          = shortEbpb.media;
                fakeBpb.spfat          = shortEbpb.spfat;
                fakeBpb.sptrk          = shortEbpb.sptrk;
                fakeBpb.heads          = shortEbpb.heads;
                fakeBpb.hsectors       = shortEbpb.hsectors;
                fakeBpb.big_sectors    = shortEbpb.big_sectors;
                fakeBpb.drive_no       = shortEbpb.drive_no;
                fakeBpb.flags          = shortEbpb.flags;
                fakeBpb.signature      = shortEbpb.signature;
                fakeBpb.serial_no      = shortEbpb.serial_no;
                fakeBpb.boot_code      = shortEbpb.boot_code;
                fakeBpb.boot_signature = shortEbpb.boot_signature;

                return BpbKind.ShortExtended;
            }

            if(useDos33Bpb)
            {
                fakeBpb.jump           = dos33Bpb.jump;
                fakeBpb.oem_name       = dos33Bpb.oem_name;
                fakeBpb.bps            = dos33Bpb.bps;
                fakeBpb.spc            = dos33Bpb.spc;
                fakeBpb.rsectors       = dos33Bpb.rsectors;
                fakeBpb.fats_no        = dos33Bpb.fats_no;
                fakeBpb.root_ent       = dos33Bpb.root_ent;
                fakeBpb.sectors        = dos33Bpb.sectors;
                fakeBpb.media          = dos33Bpb.media;
                fakeBpb.spfat          = dos33Bpb.spfat;
                fakeBpb.sptrk          = dos33Bpb.sptrk;
                fakeBpb.heads          = dos33Bpb.heads;
                fakeBpb.hsectors       = dos33Bpb.hsectors;
                fakeBpb.big_sectors    = dos33Bpb.big_sectors;
                fakeBpb.boot_code      = dos33Bpb.boot_code;
                fakeBpb.boot_signature = dos33Bpb.boot_signature;

                return BpbKind.Dos33;
            }

            if(useDos32Bpb)
            {
                fakeBpb.jump           = dos32Bpb.jump;
                fakeBpb.oem_name       = dos32Bpb.oem_name;
                fakeBpb.bps            = dos32Bpb.bps;
                fakeBpb.spc            = dos32Bpb.spc;
                fakeBpb.rsectors       = dos32Bpb.rsectors;
                fakeBpb.fats_no        = dos32Bpb.fats_no;
                fakeBpb.root_ent       = dos32Bpb.root_ent;
                fakeBpb.sectors        = dos32Bpb.sectors;
                fakeBpb.media          = dos32Bpb.media;
                fakeBpb.spfat          = dos32Bpb.spfat;
                fakeBpb.sptrk          = dos32Bpb.sptrk;
                fakeBpb.heads          = dos32Bpb.heads;
                fakeBpb.hsectors       = dos32Bpb.hsectors;
                fakeBpb.boot_code      = dos32Bpb.boot_code;
                fakeBpb.boot_signature = dos32Bpb.boot_signature;

                return BpbKind.Dos32;
            }

            if(useDos3Bpb)
            {
                fakeBpb.jump           = dos30Bpb.jump;
                fakeBpb.oem_name       = dos30Bpb.oem_name;
                fakeBpb.bps            = dos30Bpb.bps;
                fakeBpb.spc            = dos30Bpb.spc;
                fakeBpb.rsectors       = dos30Bpb.rsectors;
                fakeBpb.fats_no        = dos30Bpb.fats_no;
                fakeBpb.root_ent       = dos30Bpb.root_ent;
                fakeBpb.sectors        = dos30Bpb.sectors;
                fakeBpb.media          = dos30Bpb.media;
                fakeBpb.spfat          = dos30Bpb.spfat;
                fakeBpb.sptrk          = dos30Bpb.sptrk;
                fakeBpb.heads          = dos30Bpb.heads;
                fakeBpb.hsectors       = dos30Bpb.hsectors;
                fakeBpb.boot_code      = dos30Bpb.boot_code;
                fakeBpb.boot_signature = dos30Bpb.boot_signature;

                return BpbKind.Dos3;
            }

            if(useDos2Bpb)
            {
                fakeBpb.jump           = dos2Bpb.jump;
                fakeBpb.oem_name       = dos2Bpb.oem_name;
                fakeBpb.bps            = dos2Bpb.bps;
                fakeBpb.spc            = dos2Bpb.spc;
                fakeBpb.rsectors       = dos2Bpb.rsectors;
                fakeBpb.fats_no        = dos2Bpb.fats_no;
                fakeBpb.root_ent       = dos2Bpb.root_ent;
                fakeBpb.sectors        = dos2Bpb.sectors;
                fakeBpb.media          = dos2Bpb.media;
                fakeBpb.spfat          = dos2Bpb.spfat;
                fakeBpb.boot_code      = dos2Bpb.boot_code;
                fakeBpb.boot_signature = dos2Bpb.boot_signature;

                return BpbKind.Dos2;
            }

            if(useMsxBpb)
            {
                fakeBpb.jump           = msxBpb.jump;
                fakeBpb.oem_name       = msxBpb.oem_name;
                fakeBpb.bps            = msxBpb.bps;
                fakeBpb.spc            = msxBpb.spc;
                fakeBpb.rsectors       = msxBpb.rsectors;
                fakeBpb.fats_no        = msxBpb.fats_no;
                fakeBpb.root_ent       = msxBpb.root_ent;
                fakeBpb.sectors        = msxBpb.sectors;
                fakeBpb.media          = msxBpb.media;
                fakeBpb.spfat          = msxBpb.spfat;
                fakeBpb.sptrk          = msxBpb.sptrk;
                fakeBpb.heads          = msxBpb.heads;
                fakeBpb.hsectors       = msxBpb.hsectors;
                fakeBpb.boot_code      = msxBpb.boot_code;
                fakeBpb.boot_signature = msxBpb.boot_signature;
                fakeBpb.serial_no      = msxBpb.serial_no;

                // TODO: Is there any way to check this?
                bootable = true;

                return BpbKind.Msx;
            }

            if(useAtariBpb)
            {
                fakeBpb.jump      = atariBpb.jump;
                fakeBpb.oem_name  = atariBpb.oem_name;
                fakeBpb.bps       = atariBpb.bps;
                fakeBpb.spc       = atariBpb.spc;
                fakeBpb.rsectors  = atariBpb.rsectors;
                fakeBpb.fats_no   = atariBpb.fats_no;
                fakeBpb.root_ent  = atariBpb.root_ent;
                fakeBpb.sectors   = atariBpb.sectors;
                fakeBpb.media     = atariBpb.media;
                fakeBpb.spfat     = atariBpb.spfat;
                fakeBpb.sptrk     = atariBpb.sptrk;
                fakeBpb.heads     = atariBpb.heads;
                fakeBpb.boot_code = atariBpb.boot_code;

                return BpbKind.Atari;
            }

            if(useApricotBpb)
            {
                fakeBpb.bps      = apricotBpb.mainBPB.bps;
                fakeBpb.spc      = apricotBpb.mainBPB.spc;
                fakeBpb.rsectors = apricotBpb.mainBPB.rsectors;
                fakeBpb.fats_no  = apricotBpb.mainBPB.fats_no;
                fakeBpb.root_ent = apricotBpb.mainBPB.root_ent;
                fakeBpb.sectors  = apricotBpb.mainBPB.sectors;
                fakeBpb.media    = apricotBpb.mainBPB.media;
                fakeBpb.spfat    = apricotBpb.mainBPB.spfat;
                fakeBpb.sptrk    = apricotBpb.spt;
                bootable         = apricotBpb.bootType > 0;

                if(apricotBpb.bootLocation                       > 0 &&
                   apricotBpb.bootLocation + apricotBpb.bootSize < imagePlugin.Info.Sectors)
                    fakeBpb.boot_code = imagePlugin.ReadSectors(apricotBpb.bootLocation,
                                                                (uint)(apricotBpb.sectorSize * apricotBpb.bootSize) /
                                                                imagePlugin.Info.SectorSize);

                return BpbKind.Apricot;
            }

            return BpbKind.None;
        }
    }
}