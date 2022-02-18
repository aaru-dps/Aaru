// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MBR.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Intel/Microsoft MBR (aka Master Boot Record).
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Partitions
{
    // TODO: Support AAP extensions
    /// <inheritdoc />
    /// <summary>Implements decoding of Intel/Microsoft Master Boot Record and extensions</summary>
    public sealed class MBR : IPartition
    {
        const ulong GPT_MAGIC = 0x5452415020494645;

        const ushort MBR_MAGIC = 0xAA55;
        const ushort NEC_MAGIC = 0xA55A;
        const ushort DM_MAGIC  = 0x55AA;

        static readonly string[] _mbrTypes =
        {
            // 0x00
            "Empty", "FAT12", "XENIX root", "XENIX /usr",

            // 0x04
            "FAT16 < 32 MiB", "Extended", "FAT16", "IFS (HPFS/NTFS)",

            // 0x08
            "AIX boot, OS/2, Commodore DOS", "AIX data, Coherent, QNX", "Coherent swap, OPUS, OS/2 Boot Manager",
            "FAT32",

            // 0x0C
            "FAT32 (LBA)", "Unknown", "FAT16 (LBA)", "Extended (LBA)",

            // 0x10
            "OPUS", "Hidden FAT12", "Compaq diagnostics, recovery partition", "Unknown",

            // 0x14
            "Hidden FAT16 < 32 MiB, AST-DOS", "Unknown", "Hidden FAT16", "Hidden IFS (HPFS/NTFS)",

            // 0x18
            "AST-Windows swap", "Willowtech Photon coS", "Unknown", "Hidden FAT32",

            // 0x1C
            "Hidden FAT32 (LBA)", "Unknown", "Hidden FAT16 (LBA)", "Unknown",

            // 0x20
            "Willowsoft Overture File System", "Oxygen FSo2", "Oxygen Extended ", "SpeedStor reserved",

            // 0x24
            "NEC-DOS", "Unknown", "SpeedStor reserved", "Hidden NTFS",

            // 0x28
            "Unknown", "Unknown", "Unknown", "Unknown",

            // 0x2C
            "Unknown", "Unknown", "Unknown", "Unknown",

            // 0x30
            "Unknown", "SpeedStor reserved", "Unknown", "SpeedStor reserved",

            // 0x34
            "SpeedStor reserved", "Unknown", "SpeedStor reserved", "Unknown",

            // 0x38
            "Theos", "Plan 9", "Unknown", "Unknown",

            // 0x3C
            "Partition Magic", "Hidden NetWare", "Unknown", "Unknown",

            // 0x40
            "VENIX 80286", "PReP Boot", "Secure File System", "PTS-DOS",

            // 0x44
            "Unknown", "Priam, EUMEL/Elan", "EUMEL/Elan", "EUMEL/Elan",

            // 0x48
            "EUMEL/Elan", "Unknown", "ALFS/THIN lightweight filesystem for DOS", "Unknown",

            // 0x4C
            "Unknown", "QNX 4", "QNX 4", "QNX 4, Oberon",

            // 0x50
            "Ontrack DM, R/O, FAT", "Ontrack DM, R/W, FAT", "CP/M, Microport UNIX", "Ontrack DM 6",

            // 0x54
            "Ontrack DM 6", "EZ-Drive", "Golden Bow VFeature", "Unknown",

            // 0x58
            "Unknown", "Unknown", "Unknown", "Unknown",

            // 0x5C
            "Priam EDISK", "Unknown", "Unknown", "Unknown",

            // 0x60
            "Unknown", "SpeedStor", "Unknown", "GNU Hurd, System V, 386/ix",

            // 0x64
            "NetWare 286", "NetWare", "NetWare 386", "NetWare",

            // 0x68
            "NetWare", "NetWare NSS", "Unknown", "Unknown",

            // 0x6C
            "Unknown", "Unknown", "Unknown", "Unknown",

            // 0x70
            "DiskSecure Multi-Boot", "Unknown", "UNIX 7th Edition", "Unknown",

            // 0x74
            "Unknown", "IBM PC/IX", "Unknown", "Unknown",

            // 0x78
            "Unknown", "Unknown", "Unknown", "Unknown",

            // 0x7C
            "Unknown", "Unknown", "Unknown", "Unknown",

            // 0x80
            "Old MINIX", "MINIX, Old Linux", "Linux swap, Solaris", "Linux",

            // 0x84
            "Hidden by OS/2, APM hibernation", "Linux extended", "NT Stripe Set", "NT Stripe Set",

            // 0x88
            "Linux Plaintext", "Unknown", "Unknown", "Unknown",

            // 0x8C
            "Unknown", "Unknown", "Linux LVM", "Unknown",

            // 0x90
            "Unknown", "Unknown", "Unknown", "Amoeba, Hidden Linux",

            // 0x94
            "Amoeba bad blocks", "Unknown", "Unknown", "Unknown",

            // 0x98
            "Unknown", "Mylex EISA SCSI", "Unknown", "Unknown",

            // 0x9C
            "Unknown", "Unknown", "Unknown", "BSD/OS",

            // 0xA0
            "Hibernation", "HP Volume Expansion", "Unknown", "HP Volume Expansion",

            // 0xA4
            "HP Volume Expansion", "FreeBSD", "OpenBSD", "NeXTStep",

            // 0xA8
            "Apple UFS", "NetBSD", "Olivetti DOS FAT12", "Apple Boot",

            // 0xAC
            "Unknown", "Unknown", "Unknown", "Apple HFS",

            // 0xB0
            "BootStar", "HP Volume Expansion", "Unknown", "HP Volume Expansion",

            // 0xB4
            "HP Volume Expansion", "Unknown", "HP Volume Expansion", "BSDi",

            // 0xB8
            "BSDi swap", "Unknown", "Unknown", "PTS BootWizard",

            // 0xBC
            "Unknown", "Unknown", "Solaris boot", "Solaris",

            // 0xC0
            "Novell DOS, DR-DOS secured", "DR-DOS secured FAT12", "DR-DOS reserved", "DR-DOS reserved",

            // 0xC4
            "DR-DOS secured FAT16 < 32 MiB", "Unknown", "DR-DOS secured FAT16", "Syrinx",

            // 0xC8
            "DR-DOS reserved", "DR-DOS reserved", "DR-DOS reserved", "DR-DOS secured FAT32",

            // 0xCC
            "DR-DOS secured FAT32 (LBA)", "DR-DOS reserved", "DR-DOS secured FAT16 (LBA)",
            "DR-DOS secured extended (LBA)",

            // 0xD0
            "Multiuser DOS secured FAT12", "Multiuser DOS secured FAT12", "Unknown", "Unknown",

            // 0xD4
            "Multiuser DOS secured FAT16 < 32 MiB", "Multiuser DOS secured extended", "Multiuser DOS secured FAT16",
            "Unknown",

            // 0xD8
            "CP/M", "Unknown", "Filesystem-less data", "CP/M, CCP/M, CTOS",

            // 0xDC
            "Unknown", "Unknown", "Dell partition", "BootIt EMBRM",

            // 0xE0
            "Unknown", "SpeedStor", "DOS read/only", "SpeedStor",

            // 0xE4
            "SpeedStor", "Tandy DOS", "SpeedStor", "Unknown",

            // 0xE8
            "Unknown", "Unknown", "Unknown", "BeOS",

            // 0xEC
            "Unknown", "Spryt*x", "Guid Partition Table", "EFI system partition",

            // 0xF0
            "Linux boot", "SpeedStor", "DOS 3.3 secondary, Unisys DOS", "SpeedStor",

            // 0xF4
            "SpeedStor", "Prologue", "SpeedStor", "Unknown",

            // 0xF8
            "Unknown", "Unknown", "Unknown", "VMWare VMFS",

            // 0xFC
            "VMWare VMKCORE", "Linux RAID, FreeDOS", "SpeedStor, LANStep, PS/2 IML", "Xenix bad block"
        };

        /// <inheritdoc />
        public string Name => "Master Boot Record";
        /// <inheritdoc />
        public Guid Id => new("5E8A34E8-4F1A-59E6-4BF7-7EA647063A76");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "UnusedVariable")]
        public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
        {
            ulong counter = 0;

            partitions = new List<Partition>();

            if(imagePlugin.Info.SectorSize < 512)
                return false;

            uint sectorSize = imagePlugin.Info.SectorSize;

            // Divider of sector size in MBR between real sector size
            ulong divider = 1;

            if(imagePlugin.Info.XmlMediaType == XmlMediaType.OpticalDisc)
            {
                sectorSize = 512;
                divider    = 4;
            }

            ErrorNumber errno = imagePlugin.ReadSector(sectorOffset, out byte[] sector);

            if(errno != ErrorNumber.NoError)
                return false;

            MasterBootRecord      mbr     = Marshal.ByteArrayToStructureLittleEndian<MasterBootRecord>(sector);
            TimedMasterBootRecord mbrTime = Marshal.ByteArrayToStructureLittleEndian<TimedMasterBootRecord>(sector);

            SerializedMasterBootRecord mbrSerial =
                Marshal.ByteArrayToStructureLittleEndian<SerializedMasterBootRecord>(sector);

            ModernMasterBootRecord mbrModern = Marshal.ByteArrayToStructureLittleEndian<ModernMasterBootRecord>(sector);
            NecMasterBootRecord    mbrNec    = Marshal.ByteArrayToStructureLittleEndian<NecMasterBootRecord>(sector);

            DiskManagerMasterBootRecord mbrOntrack =
                Marshal.ByteArrayToStructureLittleEndian<DiskManagerMasterBootRecord>(sector);

            AaruConsole.DebugWriteLine("MBR plugin", "xmlmedia = {0}", imagePlugin.Info.XmlMediaType);
            AaruConsole.DebugWriteLine("MBR plugin", "mbr.magic = {0:X4}", mbr.magic);

            if(mbr.magic != MBR_MAGIC)
                return false; // Not MBR

            errno = imagePlugin.ReadSector(1 + sectorOffset, out byte[] hdrBytes);

            if(errno != ErrorNumber.NoError)
                return false;

            ulong signature = BitConverter.ToUInt64(hdrBytes, 0);

            AaruConsole.DebugWriteLine("MBR Plugin", "gpt.signature = 0x{0:X16}", signature);

            if(signature == GPT_MAGIC)
                return false;

            if(signature                     != GPT_MAGIC &&
               imagePlugin.Info.XmlMediaType == XmlMediaType.OpticalDisc)
            {
                errno = imagePlugin.ReadSector(sectorOffset, out hdrBytes);

                if(errno != ErrorNumber.NoError)
                    return false;

                signature = BitConverter.ToUInt64(hdrBytes, 512);
                AaruConsole.DebugWriteLine("MBR Plugin", "gpt.signature @ 0x200 = 0x{0:X16}", signature);

                if(signature == GPT_MAGIC)
                    return false;
            }

            PartitionEntry[] entries;

            if(mbrOntrack.dm_magic == DM_MAGIC)
                entries = mbrOntrack.entries;
            else if(mbrNec.nec_magic == NEC_MAGIC)
                entries = mbrNec.entries;
            else
                entries = mbr.entries;

            foreach(PartitionEntry entry in entries)
            {
                byte   startSector   = (byte)(entry.start_sector & 0x3F);
                ushort startCylinder = (ushort)(((entry.start_sector & 0xC0) << 2) | entry.start_cylinder);
                byte   endSector     = (byte)(entry.end_sector & 0x3F);
                ushort endCylinder   = (ushort)(((entry.end_sector & 0xC0) << 2) | entry.end_cylinder);
                ulong  lbaStart      = entry.lba_start;
                ulong  lbaSectors    = entry.lba_sectors;

                // Let's start the fun...

                bool valid    = true;
                bool extended = false;
                bool minix    = false;

                if(entry.status != 0x00 &&
                   entry.status != 0x80)
                    return false; // Maybe a FAT filesystem

                valid &= entry.type != 0x00;

                if(entry.type == 0x05 ||
                   entry.type == 0x0F ||
                   entry.type == 0x15 ||
                   entry.type == 0x1F ||
                   entry.type == 0x85 ||
                   entry.type == 0x91 ||
                   entry.type == 0x9B ||
                   entry.type == 0xC5 ||
                   entry.type == 0xCF ||
                   entry.type == 0xD5)
                {
                    valid    = false;
                    extended = true; // Extended partition
                }

                minix |= entry.type == 0x81 || entry.type == 0x80; // MINIX partition

                valid &= entry.lba_start  != 0 || entry.lba_sectors  != 0 || entry.start_cylinder != 0 ||
                         entry.start_head != 0 || entry.start_sector != 0 || entry.end_cylinder   != 0 ||
                         entry.end_head   != 0 || entry.end_sector   != 0;

                if(entry.lba_start   == 0 &&
                   entry.lba_sectors == 0 &&
                   valid)
                {
                    lbaStart = CHS.ToLBA(startCylinder, entry.start_head, startSector, imagePlugin.Info.Heads,
                                         imagePlugin.Info.SectorsPerTrack);

                    lbaSectors = CHS.ToLBA(endCylinder, entry.end_head, entry.end_sector, imagePlugin.Info.Heads,
                                           imagePlugin.Info.SectorsPerTrack) - lbaStart;
                }

                // For optical media
                lbaStart   /= divider;
                lbaSectors /= divider;

                if(minix && lbaStart == sectorOffset)
                    minix = false;

                if(lbaStart > imagePlugin.Info.Sectors)
                {
                    valid    = false;
                    extended = false;
                }

                // Some buggy implementations do some rounding errors getting a few sectors beyond device size
                if(lbaStart + lbaSectors > imagePlugin.Info.Sectors)
                    lbaSectors = imagePlugin.Info.Sectors - lbaStart;

                AaruConsole.DebugWriteLine("MBR plugin", "entry.status {0}", entry.status);
                AaruConsole.DebugWriteLine("MBR plugin", "entry.type {0}", entry.type);
                AaruConsole.DebugWriteLine("MBR plugin", "entry.lba_start {0}", entry.lba_start);
                AaruConsole.DebugWriteLine("MBR plugin", "entry.lba_sectors {0}", entry.lba_sectors);
                AaruConsole.DebugWriteLine("MBR plugin", "entry.start_cylinder {0}", startCylinder);
                AaruConsole.DebugWriteLine("MBR plugin", "entry.start_head {0}", entry.start_head);
                AaruConsole.DebugWriteLine("MBR plugin", "entry.start_sector {0}", startSector);
                AaruConsole.DebugWriteLine("MBR plugin", "entry.end_cylinder {0}", endCylinder);
                AaruConsole.DebugWriteLine("MBR plugin", "entry.end_head {0}", entry.end_head);
                AaruConsole.DebugWriteLine("MBR plugin", "entry.end_sector {0}", endSector);

                AaruConsole.DebugWriteLine("MBR plugin", "entry.minix = {0}", minix);

                AaruConsole.DebugWriteLine("MBR plugin", "lba_start {0}", lbaStart);
                AaruConsole.DebugWriteLine("MBR plugin", "lba_sectors {0}", lbaSectors);

                if(valid && minix) // Let's mix the fun
                    if(GetMinix(imagePlugin, lbaStart, divider, sectorOffset, sectorSize, out List<Partition> mnxParts))
                        partitions.AddRange(mnxParts);
                    else
                        minix = false;

                if(valid && !minix)
                {
                    var part = new Partition();

                    if((lbaStart > 0 || imagePlugin.Info.XmlMediaType == XmlMediaType.OpticalDisc) &&
                       lbaSectors > 0)
                    {
                        part.Start  = lbaStart + sectorOffset;
                        part.Length = lbaSectors;
                        part.Offset = part.Start  * sectorSize;
                        part.Size   = part.Length * sectorSize;
                    }
                    else
                        valid = false;

                    if(valid)
                    {
                        part.Type        = $"0x{entry.type:X2}";
                        part.Name        = DecodeMbrType(entry.type);
                        part.Sequence    = counter;
                        part.Description = entry.status == 0x80 ? "Partition is bootable." : "";
                        part.Scheme      = Name;

                        counter++;

                        partitions.Add(part);
                    }
                }

                AaruConsole.DebugWriteLine("MBR plugin", "entry.extended = {0}", extended);

                if(!extended)
                    continue;

                bool  processingExtended = true;
                ulong chainStart         = lbaStart;

                while(processingExtended)
                {
                    errno = imagePlugin.ReadSector(lbaStart, out sector);

                    if(errno != ErrorNumber.NoError)
                        break;

                    ExtendedBootRecord ebr = Marshal.ByteArrayToStructureLittleEndian<ExtendedBootRecord>(sector);

                    AaruConsole.DebugWriteLine("MBR plugin", "ebr.magic == MBR_Magic = {0}", ebr.magic == MBR_MAGIC);

                    if(ebr.magic != MBR_MAGIC)
                        break;

                    ulong nextStart = 0;

                    foreach(PartitionEntry ebrEntry in ebr.entries)
                    {
                        bool extValid = true;
                        startSector   = (byte)(ebrEntry.start_sector & 0x3F);
                        startCylinder = (ushort)(((ebrEntry.start_sector & 0xC0) << 2) | ebrEntry.start_cylinder);
                        endSector     = (byte)(ebrEntry.end_sector & 0x3F);
                        endCylinder   = (ushort)(((ebrEntry.end_sector & 0xC0) << 2) | ebrEntry.end_cylinder);
                        ulong extStart   = ebrEntry.lba_start;
                        ulong extSectors = ebrEntry.lba_sectors;
                        bool  extMinix   = false;

                        AaruConsole.DebugWriteLine("MBR plugin", "ebr_entry.status {0}", ebrEntry.status);
                        AaruConsole.DebugWriteLine("MBR plugin", "ebr_entry.type {0}", ebrEntry.type);
                        AaruConsole.DebugWriteLine("MBR plugin", "ebr_entry.lba_start {0}", ebrEntry.lba_start);
                        AaruConsole.DebugWriteLine("MBR plugin", "ebr_entry.lba_sectors {0}", ebrEntry.lba_sectors);
                        AaruConsole.DebugWriteLine("MBR plugin", "ebr_entry.start_cylinder {0}", startCylinder);
                        AaruConsole.DebugWriteLine("MBR plugin", "ebr_entry.start_head {0}", ebrEntry.start_head);
                        AaruConsole.DebugWriteLine("MBR plugin", "ebr_entry.start_sector {0}", startSector);
                        AaruConsole.DebugWriteLine("MBR plugin", "ebr_entry.end_cylinder {0}", endCylinder);
                        AaruConsole.DebugWriteLine("MBR plugin", "ebr_entry.end_head {0}", ebrEntry.end_head);
                        AaruConsole.DebugWriteLine("MBR plugin", "ebr_entry.end_sector {0}", endSector);

                        // Let's start the fun...
                        extValid &= ebrEntry.status == 0x00 || ebrEntry.status == 0x80;
                        extValid &= ebrEntry.type != 0x00;

                        extValid &= ebrEntry.lba_start      != 0 || ebrEntry.lba_sectors  != 0 ||
                                    ebrEntry.start_cylinder != 0 || ebrEntry.start_head   != 0 ||
                                    ebrEntry.start_sector   != 0 || ebrEntry.end_cylinder != 0 ||
                                    ebrEntry.end_head       != 0 || ebrEntry.end_sector   != 0;

                        if(ebrEntry.lba_start   == 0 &&
                           ebrEntry.lba_sectors == 0 &&
                           extValid)
                        {
                            extStart = CHS.ToLBA(startCylinder, ebrEntry.start_head, startSector,
                                                 imagePlugin.Info.Heads, imagePlugin.Info.SectorsPerTrack);

                            extSectors = CHS.ToLBA(endCylinder, ebrEntry.end_head, ebrEntry.end_sector,
                                                   imagePlugin.Info.Heads, imagePlugin.Info.SectorsPerTrack) - extStart;
                        }

                        extMinix |= ebrEntry.type == 0x81 || ebrEntry.type == 0x80;

                        // For optical media
                        extStart   /= divider;
                        extSectors /= divider;

                        AaruConsole.DebugWriteLine("MBR plugin", "ext_start {0}", extStart);
                        AaruConsole.DebugWriteLine("MBR plugin", "ext_sectors {0}", extSectors);

                        if(ebrEntry.type == 0x05 ||
                           ebrEntry.type == 0x0F ||
                           ebrEntry.type == 0x15 ||
                           ebrEntry.type == 0x1F ||
                           ebrEntry.type == 0x85 ||
                           ebrEntry.type == 0x91 ||
                           ebrEntry.type == 0x9B ||
                           ebrEntry.type == 0xC5 ||
                           ebrEntry.type == 0xCF ||
                           ebrEntry.type == 0xD5)
                        {
                            extValid  = false;
                            nextStart = chainStart + extStart;
                        }

                        extStart += lbaStart;
                        extValid &= extStart <= imagePlugin.Info.Sectors;

                        // Some buggy implementations do some rounding errors getting a few sectors beyond device size
                        if(extStart + extSectors > imagePlugin.Info.Sectors)
                            extSectors = imagePlugin.Info.Sectors - extStart;

                        if(extValid && extMinix) // Let's mix the fun
                            if(GetMinix(imagePlugin, lbaStart, divider, sectorOffset, sectorSize,
                                        out List<Partition> mnxParts))
                                partitions.AddRange(mnxParts);
                            else
                                extMinix = false;

                        if(!extValid || extMinix)
                            continue;

                        var part = new Partition();

                        if(extStart   > 0 &&
                           extSectors > 0)
                        {
                            part.Start  = extStart + sectorOffset;
                            part.Length = extSectors;
                            part.Offset = part.Start  * sectorSize;
                            part.Size   = part.Length * sectorSize;
                        }
                        else
                            extValid = false;

                        if(!extValid)
                            continue;

                        part.Type        = $"0x{ebrEntry.type:X2}";
                        part.Name        = DecodeMbrType(ebrEntry.type);
                        part.Sequence    = counter;
                        part.Description = ebrEntry.status == 0x80 ? "Partition is bootable." : "";
                        part.Scheme      = Name;
                        counter++;

                        partitions.Add(part);
                    }

                    AaruConsole.DebugWriteLine("MBR plugin", "next_start {0}", nextStart);
                    processingExtended &= nextStart != 0;
                    processingExtended &= nextStart <= imagePlugin.Info.Sectors;
                    lbaStart           =  nextStart;
                }
            }

            // An empty MBR may exist, NeXT creates one and then hardcodes its disklabel
            return partitions.Count != 0;
        }

        static bool GetMinix(IMediaImage imagePlugin, ulong start, ulong divider, ulong sectorOffset, uint sectorSize,
                             out List<Partition> partitions)
        {
            partitions = new List<Partition>();

            ErrorNumber errno = imagePlugin.ReadSector(start, out byte[] sector);

            if(errno != ErrorNumber.NoError)
                return false;

            ExtendedBootRecord mnx = Marshal.ByteArrayToStructureLittleEndian<ExtendedBootRecord>(sector);

            AaruConsole.DebugWriteLine("MBR plugin", "mnx.magic == MBR_Magic = {0}", mnx.magic == MBR_MAGIC);

            if(mnx.magic != MBR_MAGIC)
                return false;

            bool anyMnx = false;

            foreach(PartitionEntry mnxEntry in mnx.entries)
            {
                bool   mnxValid      = true;
                byte   startSector   = (byte)(mnxEntry.start_sector & 0x3F);
                ushort startCylinder = (ushort)(((mnxEntry.start_sector & 0xC0) << 2) | mnxEntry.start_cylinder);
                byte   endSector     = (byte)(mnxEntry.end_sector & 0x3F);
                ushort endCylinder   = (ushort)(((mnxEntry.end_sector & 0xC0) << 2) | mnxEntry.end_cylinder);
                ulong  mnxStart      = mnxEntry.lba_start;
                ulong  mnxSectors    = mnxEntry.lba_sectors;

                AaruConsole.DebugWriteLine("MBR plugin", "mnx_entry.status {0}", mnxEntry.status);
                AaruConsole.DebugWriteLine("MBR plugin", "mnx_entry.type {0}", mnxEntry.type);
                AaruConsole.DebugWriteLine("MBR plugin", "mnx_entry.lba_start {0}", mnxEntry.lba_start);
                AaruConsole.DebugWriteLine("MBR plugin", "mnx_entry.lba_sectors {0}", mnxEntry.lba_sectors);
                AaruConsole.DebugWriteLine("MBR plugin", "mnx_entry.start_cylinder {0}", startCylinder);
                AaruConsole.DebugWriteLine("MBR plugin", "mnx_entry.start_head {0}", mnxEntry.start_head);
                AaruConsole.DebugWriteLine("MBR plugin", "mnx_entry.start_sector {0}", startSector);
                AaruConsole.DebugWriteLine("MBR plugin", "mnx_entry.end_cylinder {0}", endCylinder);
                AaruConsole.DebugWriteLine("MBR plugin", "mnx_entry.end_head {0}", mnxEntry.end_head);
                AaruConsole.DebugWriteLine("MBR plugin", "mnx_entry.end_sector {0}", endSector);

                mnxValid &= mnxEntry.status == 0x00 || mnxEntry.status == 0x80;
                mnxValid &= mnxEntry.type   == 0x81 || mnxEntry.type   == 0x80;

                mnxValid &= mnxEntry.lba_start  != 0 || mnxEntry.lba_sectors  != 0 || mnxEntry.start_cylinder != 0 ||
                            mnxEntry.start_head != 0 || mnxEntry.start_sector != 0 || mnxEntry.end_cylinder   != 0 ||
                            mnxEntry.end_head   != 0 || mnxEntry.end_sector   != 0;

                if(mnxEntry.lba_start   == 0 &&
                   mnxEntry.lba_sectors == 0 &&
                   mnxValid)
                {
                    mnxStart = CHS.ToLBA(startCylinder, mnxEntry.start_head, startSector, imagePlugin.Info.Heads,
                                         imagePlugin.Info.SectorsPerTrack);

                    mnxSectors = CHS.ToLBA(endCylinder, mnxEntry.end_head, mnxEntry.end_sector, imagePlugin.Info.Heads,
                                           imagePlugin.Info.SectorsPerTrack) - mnxStart;
                }

                // For optical media
                mnxStart   /= divider;
                mnxSectors /= divider;

                AaruConsole.DebugWriteLine("MBR plugin", "mnx_start {0}", mnxStart);
                AaruConsole.DebugWriteLine("MBR plugin", "mnx_sectors {0}", mnxSectors);

                if(!mnxValid)
                    continue;

                var part = new Partition();

                if(mnxStart   > 0 &&
                   mnxSectors > 0)
                {
                    part.Start  = mnxStart + sectorOffset;
                    part.Length = mnxSectors;
                    part.Offset = part.Start  * sectorSize;
                    part.Size   = part.Length * sectorSize;
                }
                else
                    mnxValid = false;

                if(!mnxValid)
                    continue;

                anyMnx           = true;
                part.Type        = "MINIX";
                part.Name        = "MINIX";
                part.Description = mnxEntry.status == 0x80 ? "Partition is bootable." : "";
                part.Scheme      = "MINIX";

                partitions.Add(part);
            }

            return anyMnx;
        }

        static string DecodeMbrType(byte type) => _mbrTypes[type];

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct MasterBootRecord
        {
            /// <summary>Boot code</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 446)]
            public readonly byte[] boot_code;
            /// <summary>Partitions</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public readonly PartitionEntry[] entries;
            /// <summary>
            ///     <see cref="MBR.MBR_MAGIC" />
            /// </summary>
            public readonly ushort magic;
        }

        // TODO: IBM Boot Manager
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct ExtendedBootRecord
        {
            /// <summary>Boot code, almost always unused</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 446)]
            public readonly byte[] boot_code;
            /// <summary>Partitions or pointers</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public readonly PartitionEntry[] entries;
            /// <summary>
            ///     <see cref="MBR.MBR_MAGIC" />
            /// </summary>
            public readonly ushort magic;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct TimedMasterBootRecord
        {
            /// <summary>Boot code</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 218)]
            public readonly byte[] boot_code;
            /// <summary>Set to 0</summary>
            public readonly ushort zero;
            /// <summary>Original physical drive</summary>
            public readonly byte drive;
            /// <summary>Disk timestamp, seconds</summary>
            public readonly byte seconds;
            /// <summary>Disk timestamp, minutes</summary>
            public readonly byte minutes;
            /// <summary>Disk timestamp, hours</summary>
            public readonly byte hours;
            /// <summary>Boot code, continuation</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 222)]
            public readonly byte[] boot_code2;
            /// <summary>Partitions</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public readonly PartitionEntry[] entries;
            /// <summary>
            ///     <see cref="MBR.MBR_MAGIC" />
            /// </summary>
            public readonly ushort magic;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct SerializedMasterBootRecord
        {
            /// <summary>Boot code</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 440)]
            public readonly byte[] boot_code;
            /// <summary>Disk serial number</summary>
            public readonly uint serial;
            /// <summary>Set to 0</summary>
            public readonly ushort zero;
            /// <summary>Partitions</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public readonly PartitionEntry[] entries;
            /// <summary>
            ///     <see cref="MBR.MBR_MAGIC" />
            /// </summary>
            public readonly ushort magic;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct ModernMasterBootRecord
        {
            /// <summary>Boot code</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 218)]
            public readonly byte[] boot_code;
            /// <summary>Set to 0</summary>
            public readonly ushort zero;
            /// <summary>Original physical drive</summary>
            public readonly byte drive;
            /// <summary>Disk timestamp, seconds</summary>
            public readonly byte seconds;
            /// <summary>Disk timestamp, minutes</summary>
            public readonly byte minutes;
            /// <summary>Disk timestamp, hours</summary>
            public readonly byte hours;
            /// <summary>Boot code, continuation</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 216)]
            public readonly byte[] boot_code2;
            /// <summary>Disk serial number</summary>
            public readonly uint serial;
            /// <summary>Set to 0</summary>
            public readonly ushort zero2;
            /// <summary>Partitions</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public readonly PartitionEntry[] entries;
            /// <summary>
            ///     <see cref="MBR.MBR_MAGIC" />
            /// </summary>
            public readonly ushort magic;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct NecMasterBootRecord
        {
            /// <summary>Boot code</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 380)]
            public readonly byte[] boot_code;
            /// <summary>
            ///     <see cref="MBR.NEC_MAGIC" />
            /// </summary>
            public readonly ushort nec_magic;
            /// <summary>Partitions</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly PartitionEntry[] entries;
            /// <summary>
            ///     <see cref="MBR.MBR_MAGIC" />
            /// </summary>
            public readonly ushort magic;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct DiskManagerMasterBootRecord
        {
            /// <summary>Boot code</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 252)]
            public readonly byte[] boot_code;
            /// <summary>
            ///     <see cref="MBR.DM_MAGIC" />
            /// </summary>
            public readonly ushort dm_magic;
            /// <summary>Partitions</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly PartitionEntry[] entries;
            /// <summary>
            ///     <see cref="MBR.MBR_MAGIC" />
            /// </summary>
            public readonly ushort magic;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct PartitionEntry
        {
            /// <summary>Partition status, 0x80 or 0x00, else invalid</summary>
            public readonly byte status;
            /// <summary>Starting head [0,254]</summary>
            public readonly byte start_head;
            /// <summary>Starting sector [1,63]</summary>
            public readonly byte start_sector;
            /// <summary>Starting cylinder [0,1023]</summary>
            public readonly byte start_cylinder;
            /// <summary>Partition type</summary>
            public readonly byte type;
            /// <summary>Ending head [0,254]</summary>
            public readonly byte end_head;
            /// <summary>Ending sector [1,63]</summary>
            public readonly byte end_sector;
            /// <summary>Ending cylinder [0,1023]</summary>
            public readonly byte end_cylinder;
            /// <summary>Starting absolute sector</summary>
            public readonly uint lba_start;
            /// <summary>Total sectors</summary>
            public readonly uint lba_sectors;
        }
    }
}