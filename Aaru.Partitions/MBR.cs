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
// Copyright © 2011-2023 Natalia Portillo
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

namespace Aaru.Partitions;

// TODO: Support AAP extensions
/// <inheritdoc />
/// <summary>Implements decoding of Intel/Microsoft Master Boot Record and extensions</summary>
public sealed class MBR : IPartition
{
    const ulong GPT_MAGIC = 0x5452415020494645;

    const ushort MBR_MAGIC   = 0xAA55;
    const ushort NEC_MAGIC   = 0xA55A;
    const ushort DM_MAGIC    = 0x55AA;
    const string MODULE_NAME = "Master Boot Record (MBR) plugin";

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    static readonly string[] _mbrTypes =
    {
        // 0x00
        Localization.Empty, Localization.FAT12, Localization.XENIX_root, Localization.XENIX_usr,

        // 0x04
        Localization.FAT16_32_MiB, Localization.Extended, Localization.FAT16, Localization.IFS_HPFS_NTFS,

        // 0x08
        Localization.AIX_boot_OS2_Commodore_DOS, Localization.AIX_data_Coherent_QNX,
        Localization.Coherent_swap_OPUS_OS_2_Boot_Manager, Localization.FAT32,

        // 0x0C
        Localization.FAT32_LBA, Localization.Unknown_partition_type, Localization.FAT16_LBA, Localization.Extended_LBA,

        // 0x10
        Localization.OPUS, Localization.Hidden_FAT12, Localization.Compaq_diagnostics_recovery_partition,
        Localization.Unknown_partition_type,

        // 0x14
        Localization.Hidden_FAT16_32_MiB_AST_DOS, Localization.Unknown_partition_type, Localization.Hidden_FAT16,
        Localization.Hidden_IFS_HPFS_NTFS,

        // 0x18
        Localization.AST_Windows_swap, Localization.Willowtech_Photon_coS, Localization.Unknown_partition_type,
        Localization.Hidden_FAT32,

        // 0x1C
        Localization.Hidden_FAT32_LBA, Localization.Unknown_partition_type, Localization.Hidden_FAT16_LBA,
        Localization.Unknown_partition_type,

        // 0x20
        Localization.Willowsoft_Overture_File_System, Localization.Oxygen_FSo2, Localization.Oxygen_Extended,
        Localization.SpeedStor_reserved,

        // 0x24
        Localization.NEC_DOS, Localization.Unknown_partition_type, Localization.SpeedStor_reserved,
        Localization.Hidden_NTFS,

        // 0x28
        Localization.Unknown_partition_type, Localization.Unknown_partition_type, Localization.Unknown_partition_type,
        Localization.Unknown_partition_type,

        // 0x2C
        Localization.Unknown_partition_type, Localization.Unknown_partition_type, Localization.Unknown_partition_type,
        Localization.Unknown_partition_type,

        // 0x30
        Localization.Unknown_partition_type, Localization.SpeedStor_reserved, Localization.Unknown_partition_type,
        Localization.SpeedStor_reserved,

        // 0x34
        Localization.SpeedStor_reserved, Localization.Unknown_partition_type, Localization.SpeedStor_reserved,
        Localization.Unknown_partition_type,

        // 0x38
        Localization.Theos, Localization.Plan_9, Localization.Unknown_partition_type,
        Localization.Unknown_partition_type,

        // 0x3C
        Localization.Partition_Magic, Localization.Hidden_NetWare, Localization.Unknown_partition_type,
        Localization.Unknown_partition_type,

        // 0x40
        Localization.VENIX_80286, Localization.PReP_Boot, Localization.Secure_File_System, Localization.PTS_DOS,

        // 0x44
        Localization.Unknown_partition_type, Localization.Priam_EUMEL_Elan, Localization.EUMEL_Elan,
        Localization.EUMEL_Elan,

        // 0x48
        Localization.EUMEL_Elan, Localization.Unknown_partition_type,
        Localization.ALFS_THIN_lightweight_filesystem_for_DOS, Localization.Unknown_partition_type,

        // 0x4C
        Localization.Unknown_partition_type, Localization.QNX_4, Localization.QNX_4, Localization.QNX_4_Oberon,

        // 0x50
        Localization.Ontrack_DM_RO_FAT, Localization.Ontrack_DM_RW_FAT, Localization.CPM_Microport_UNIX,
        Localization.Ontrack_DM_6,

        // 0x54
        Localization.Ontrack_DM_6, Localization.EZ_Drive, Localization.Golden_Bow_VFeature,
        Localization.Unknown_partition_type,

        // 0x58
        Localization.Unknown_partition_type, Localization.Unknown_partition_type, Localization.Unknown_partition_type,
        Localization.Unknown_partition_type,

        // 0x5C
        Localization.Priam_EDISK, Localization.Unknown_partition_type, Localization.Unknown_partition_type,
        Localization.Unknown_partition_type,

        // 0x60
        Localization.Unknown_partition_type, Localization.SpeedStor, Localization.Unknown_partition_type,
        Localization.GNU_Hurd_System_V_386ix,

        // 0x64
        Localization.NetWare_286, Localization.NetWare, Localization.NetWare_386, Localization.NetWare,

        // 0x68
        Localization.NetWare, Localization.NetWare_NSS, Localization.Unknown_partition_type,
        Localization.Unknown_partition_type,

        // 0x6C
        Localization.Unknown_partition_type, Localization.Unknown_partition_type, Localization.Unknown_partition_type,
        Localization.Unknown_partition_type,

        // 0x70
        Localization.DiskSecure_Multi_Boot, Localization.Unknown_partition_type, Localization.UNIX_7th_Edition,
        Localization.Unknown_partition_type,

        // 0x74
        Localization.Unknown_partition_type, Localization.IBM_PC_IX, Localization.Unknown_partition_type,
        Localization.Unknown_partition_type,

        // 0x78
        Localization.Unknown_partition_type, Localization.Unknown_partition_type, Localization.Unknown_partition_type,
        Localization.Unknown_partition_type,

        // 0x7C
        Localization.Unknown_partition_type, Localization.Unknown_partition_type, Localization.Unknown_partition_type,
        Localization.Unknown_partition_type,

        // 0x80
        Localization.Old_MINIX, Localization.MINIX_Old_Linux, Localization.Linux_swap_Solaris, Localization.Linux,

        // 0x84
        Localization.Hidden_by_OS2_APM_hibernation, Localization.Linux_extended, Localization.NT_Stripe_Set,
        Localization.NT_Stripe_Set,

        // 0x88
        Localization.Linux_Plaintext, Localization.Unknown_partition_type, Localization.Unknown_partition_type,
        Localization.Unknown_partition_type,

        // 0x8C
        Localization.Unknown_partition_type, Localization.Unknown_partition_type, Localization.Linux_LVM,
        Localization.Unknown_partition_type,

        // 0x90
        Localization.Unknown_partition_type, Localization.Unknown_partition_type, Localization.Unknown_partition_type,
        Localization.Amoeba_Hidden_Linux,

        // 0x94
        Localization.Amoeba_bad_blocks, Localization.Unknown_partition_type, Localization.Unknown_partition_type,
        Localization.Unknown_partition_type,

        // 0x98
        Localization.Unknown_partition_type, Localization.Mylex_EISA_SCSI, Localization.Unknown_partition_type,
        Localization.Unknown_partition_type,

        // 0x9C
        Localization.Unknown_partition_type, Localization.Unknown_partition_type, Localization.Unknown_partition_type,
        Localization.BSD_OS,

        // 0xA0
        Localization.Hibernation, Localization.HP_Volume_Expansion, Localization.Unknown_partition_type,
        Localization.HP_Volume_Expansion,

        // 0xA4
        Localization.HP_Volume_Expansion, Localization.FreeBSD, Localization.OpenBSD, Localization.NeXTStep,

        // 0xA8
        Localization.Apple_UFS, Localization.NetBSD, Localization.Olivetti_DOS_FAT12, Localization.Apple_Boot,

        // 0xAC
        Localization.Unknown_partition_type, Localization.Unknown_partition_type, Localization.Unknown_partition_type,
        Localization.Apple_HFS,

        // 0xB0
        Localization.BootStar, Localization.HP_Volume_Expansion, Localization.Unknown_partition_type,
        Localization.HP_Volume_Expansion,

        // 0xB4
        Localization.HP_Volume_Expansion, Localization.Unknown_partition_type, Localization.HP_Volume_Expansion,
        Localization.BSDi,

        // 0xB8
        "BSDi swap", Localization.Unknown_partition_type, Localization.Unknown_partition_type,
        Localization.PTS_BootWizard,

        // 0xBC
        Localization.Unknown_partition_type, Localization.Unknown_partition_type, Localization.Solaris_boot,
        Localization.Solaris,

        // 0xC0
        Localization.Novell_DOS_DR_DOS_secured, Localization.DR_DOS_secured_FAT12, Localization.DR_DOS_reserved,
        Localization.DR_DOS_reserved,

        // 0xC4
        Localization.DR_DOS_secured_FAT16_32_MiB, Localization.Unknown_partition_type,
        Localization.DR_DOS_secured_FAT16, Localization.Syrinx,

        // 0xC8
        Localization.DR_DOS_reserved, Localization.DR_DOS_reserved, Localization.DR_DOS_reserved,
        Localization.DR_DOS_secured_FAT32,

        // 0xCC
        Localization.DR_DOS_secured_FAT32_LBA, Localization.DR_DOS_reserved, Localization.DR_DOS_secured_FAT16_LBA,
        Localization.DR_DOS_secured_extended_LBA,

        // 0xD0
        Localization.Multiuser_DOS_secured_FAT12, Localization.Multiuser_DOS_secured_FAT12,
        Localization.Unknown_partition_type, Localization.Unknown_partition_type,

        // 0xD4
        Localization.Multiuser_DOS_secured_FAT16_32_MiB, Localization.Multiuser_DOS_secured_extended,
        Localization.Multiuser_DOS_secured_FAT16, Localization.Unknown_partition_type,

        // 0xD8
        Localization.CPM, Localization.Unknown_partition_type, Localization.Filesystem_less_data,
        Localization.CPM_CCPM_CTOS,

        // 0xDC
        Localization.Unknown_partition_type, Localization.Unknown_partition_type, Localization.Dell_partition,
        Localization.BootIt_EMBRM,

        // 0xE0
        Localization.Unknown_partition_type, Localization.SpeedStor, Localization.DOS_read_only, Localization.SpeedStor,

        // 0xE4
        Localization.SpeedStor, Localization.Tandy_DOS, Localization.SpeedStor, Localization.Unknown_partition_type,

        // 0xE8
        Localization.Unknown_partition_type, Localization.Unknown_partition_type, Localization.Unknown_partition_type,
        Localization.BeOS,

        // 0xEC
        Localization.Unknown_partition_type, Localization.Sprytx, Localization.Guid_Partition_Table,
        Localization.EFI_system_partition,

        // 0xF0
        Localization.Linux_boot, Localization.SpeedStor, Localization.DOS_3_3_secondary_Unisys_DOS,
        Localization.SpeedStor,

        // 0xF4
        Localization.SpeedStor, Localization.Prologue, Localization.SpeedStor, Localization.Unknown_partition_type,

        // 0xF8
        Localization.Unknown_partition_type, Localization.Unknown_partition_type, Localization.Unknown_partition_type,
        Localization.VMware_VMFS,

        // 0xFC
        Localization.VMWare_VMKCORE, Localization.Linux_RAID_FreeDOS, Localization.SpeedStor_LANStep_PS2_IML,
        Localization.Xenix_bad_block
    };

#region IPartition Members

    /// <inheritdoc />
    public string Name => Localization.MBR_Name;

    /// <inheritdoc />
    public Guid Id => new("5E8A34E8-4F1A-59E6-4BF7-7EA647063A76");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

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

        if(imagePlugin.Info.MetadataMediaType == MetadataMediaType.OpticalDisc)
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

        AaruConsole.DebugWriteLine(MODULE_NAME, "xmlmedia = {0}",     imagePlugin.Info.MetadataMediaType);
        AaruConsole.DebugWriteLine(MODULE_NAME, "mbr.magic = {0:X4}", mbr.magic);

        if(mbr.magic != MBR_MAGIC)
            return false; // Not MBR

        errno = imagePlugin.ReadSector(1 + sectorOffset, out byte[] hdrBytes);

        if(errno != ErrorNumber.NoError)
            return false;

        var signature = BitConverter.ToUInt64(hdrBytes, 0);

        AaruConsole.DebugWriteLine(MODULE_NAME, "gpt.signature = 0x{0:X16}", signature);

        if(signature == GPT_MAGIC)
            return false;

        if(imagePlugin.Info.MetadataMediaType == MetadataMediaType.OpticalDisc)
        {
            errno = imagePlugin.ReadSector(sectorOffset, out hdrBytes);

            if(errno != ErrorNumber.NoError)
                return false;

            signature = BitConverter.ToUInt64(hdrBytes, 512);
            AaruConsole.DebugWriteLine(MODULE_NAME, "gpt.signature @ 0x200 = 0x{0:X16}", signature);

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
            var   startSector   = (byte)(entry.start_sector & 0x3F);
            var   startCylinder = (ushort)((entry.start_sector & 0xC0) << 2 | entry.start_cylinder);
            var   endSector     = (byte)(entry.end_sector & 0x3F);
            var   endCylinder   = (ushort)((entry.end_sector & 0xC0) << 2 | entry.end_cylinder);
            ulong lbaStart      = entry.lba_start;
            ulong lbaSectors    = entry.lba_sectors;

            // Let's start the fun...

            var valid    = true;
            var extended = false;
            var minix    = false;

            if(entry.status != 0x00 && entry.status != 0x80)
                return false; // Maybe a FAT filesystem

            valid &= entry.type != 0x00;

            if(entry.type is 0x05 or 0x0F or 0x15 or 0x1F or 0x85 or 0x91 or 0x9B or 0xC5 or 0xCF or 0xD5)
            {
                valid    = false;
                extended = true; // Extended partition
            }

            minix |= entry.type is 0x81 or 0x80; // MINIX partition

            valid &= entry.lba_start      != 0 ||
                     entry.lba_sectors    != 0 ||
                     entry.start_cylinder != 0 ||
                     entry.start_head     != 0 ||
                     entry.start_sector   != 0 ||
                     entry.end_cylinder   != 0 ||
                     entry.end_head       != 0 ||
                     entry.end_sector     != 0;

            if(entry is { lba_start: 0, lba_sectors: 0 } && valid)
            {
                lbaStart = CHS.ToLBA(startCylinder, entry.start_head, startSector, imagePlugin.Info.Heads,
                                     imagePlugin.Info.SectorsPerTrack);

                lbaSectors = CHS.ToLBA(endCylinder, entry.end_head, entry.end_sector, imagePlugin.Info.Heads,
                                       imagePlugin.Info.SectorsPerTrack) -
                             lbaStart;
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

            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.status {0}",         entry.status);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.type {0}",           entry.type);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.lba_start {0}",      entry.lba_start);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.lba_sectors {0}",    entry.lba_sectors);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.start_cylinder {0}", startCylinder);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.start_head {0}",     entry.start_head);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.start_sector {0}",   startSector);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.end_cylinder {0}",   endCylinder);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.end_head {0}",       entry.end_head);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.end_sector {0}",     endSector);

            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.minix = {0}", minix);

            AaruConsole.DebugWriteLine(MODULE_NAME, "lba_start {0}",   lbaStart);
            AaruConsole.DebugWriteLine(MODULE_NAME, "lba_sectors {0}", lbaSectors);

            if(valid && minix) // Let's mix the fun
            {
                if(GetMinix(imagePlugin, lbaStart, divider, sectorOffset, sectorSize, out List<Partition> mnxParts))
                    partitions.AddRange(mnxParts);
                else
                    minix = false;
            }

            if(valid && !minix)
            {
                var part = new Partition();

                if((lbaStart > 0 || imagePlugin.Info.MetadataMediaType == MetadataMediaType.OpticalDisc) &&
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
                    part.Description = entry.status == 0x80 ? Localization.Partition_is_bootable : "";
                    part.Scheme      = Name;

                    counter++;

                    partitions.Add(part);
                }
            }

            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.extended = {0}", extended);

            if(!extended)
                continue;

            var   processingExtended = true;
            ulong chainStart         = lbaStart;

            while(processingExtended)
            {
                errno = imagePlugin.ReadSector(lbaStart, out sector);

                if(errno != ErrorNumber.NoError)
                    break;

                ExtendedBootRecord ebr = Marshal.ByteArrayToStructureLittleEndian<ExtendedBootRecord>(sector);

                AaruConsole.DebugWriteLine(MODULE_NAME, "ebr.magic == MBR_Magic = {0}", ebr.magic == MBR_MAGIC);

                if(ebr.magic != MBR_MAGIC)
                    break;

                ulong nextStart = 0;

                foreach(PartitionEntry ebrEntry in ebr.entries)
                {
                    var extValid = true;
                    startSector   = (byte)(ebrEntry.start_sector & 0x3F);
                    startCylinder = (ushort)((ebrEntry.start_sector & 0xC0) << 2 | ebrEntry.start_cylinder);
                    endSector     = (byte)(ebrEntry.end_sector & 0x3F);
                    endCylinder   = (ushort)((ebrEntry.end_sector & 0xC0) << 2 | ebrEntry.end_cylinder);
                    ulong extStart   = ebrEntry.lba_start;
                    ulong extSectors = ebrEntry.lba_sectors;
                    var   extMinix   = false;

                    AaruConsole.DebugWriteLine(MODULE_NAME, "ebr_entry.status {0}",         ebrEntry.status);
                    AaruConsole.DebugWriteLine(MODULE_NAME, "ebr_entry.type {0}",           ebrEntry.type);
                    AaruConsole.DebugWriteLine(MODULE_NAME, "ebr_entry.lba_start {0}",      ebrEntry.lba_start);
                    AaruConsole.DebugWriteLine(MODULE_NAME, "ebr_entry.lba_sectors {0}",    ebrEntry.lba_sectors);
                    AaruConsole.DebugWriteLine(MODULE_NAME, "ebr_entry.start_cylinder {0}", startCylinder);
                    AaruConsole.DebugWriteLine(MODULE_NAME, "ebr_entry.start_head {0}",     ebrEntry.start_head);
                    AaruConsole.DebugWriteLine(MODULE_NAME, "ebr_entry.start_sector {0}",   startSector);
                    AaruConsole.DebugWriteLine(MODULE_NAME, "ebr_entry.end_cylinder {0}",   endCylinder);
                    AaruConsole.DebugWriteLine(MODULE_NAME, "ebr_entry.end_head {0}",       ebrEntry.end_head);
                    AaruConsole.DebugWriteLine(MODULE_NAME, "ebr_entry.end_sector {0}",     endSector);

                    // Let's start the fun...
                    extValid &= ebrEntry.status is 0x00 or 0x80;
                    extValid &= ebrEntry.type != 0x00;

                    extValid &= ebrEntry.lba_start      != 0 ||
                                ebrEntry.lba_sectors    != 0 ||
                                ebrEntry.start_cylinder != 0 ||
                                ebrEntry.start_head     != 0 ||
                                ebrEntry.start_sector   != 0 ||
                                ebrEntry.end_cylinder   != 0 ||
                                ebrEntry.end_head       != 0 ||
                                ebrEntry.end_sector     != 0;

                    if(ebrEntry is { lba_start: 0, lba_sectors: 0 } && extValid)
                    {
                        extStart = CHS.ToLBA(startCylinder, ebrEntry.start_head, startSector, imagePlugin.Info.Heads,
                                             imagePlugin.Info.SectorsPerTrack);

                        extSectors = CHS.ToLBA(endCylinder, ebrEntry.end_head, ebrEntry.end_sector,
                                               imagePlugin.Info.Heads, imagePlugin.Info.SectorsPerTrack) -
                                     extStart;
                    }

                    extMinix |= ebrEntry.type is 0x81 or 0x80;

                    // For optical media
                    extStart   /= divider;
                    extSectors /= divider;

                    AaruConsole.DebugWriteLine(MODULE_NAME, "ext_start {0}",   extStart);
                    AaruConsole.DebugWriteLine(MODULE_NAME, "ext_sectors {0}", extSectors);

                    if(ebrEntry.type is 0x05 or 0x0F or 0x15 or 0x1F or 0x85 or 0x91 or 0x9B or 0xC5 or 0xCF or 0xD5)
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
                    {
                        if(GetMinix(imagePlugin, lbaStart, divider, sectorOffset, sectorSize,
                                    out List<Partition> mnxParts))
                            partitions.AddRange(mnxParts);
                        else
                            extMinix = false;
                    }

                    if(!extValid || extMinix)
                        continue;

                    var part = new Partition();

                    if(extStart > 0 && extSectors > 0)
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
                    part.Description = ebrEntry.status == 0x80 ? Localization.Partition_is_bootable : "";
                    part.Scheme      = Name;
                    counter++;

                    partitions.Add(part);
                }

                AaruConsole.DebugWriteLine(MODULE_NAME, "next_start {0}", nextStart);
                processingExtended &= nextStart != 0;
                processingExtended &= nextStart <= imagePlugin.Info.Sectors;
                lbaStart           =  nextStart;
            }
        }

        // An empty MBR may exist, NeXT creates one and then hardcodes its disklabel
        return partitions.Count != 0;
    }

#endregion

    static bool GetMinix(IMediaImage imagePlugin, ulong start, ulong divider, ulong sectorOffset, uint sectorSize,
                         out List<Partition> partitions)
    {
        partitions = new List<Partition>();

        ErrorNumber errno = imagePlugin.ReadSector(start, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        ExtendedBootRecord mnx = Marshal.ByteArrayToStructureLittleEndian<ExtendedBootRecord>(sector);

        AaruConsole.DebugWriteLine(MODULE_NAME, "mnx.magic == MBR_Magic = {0}", mnx.magic == MBR_MAGIC);

        if(mnx.magic != MBR_MAGIC)
            return false;

        var anyMnx = false;

        foreach(PartitionEntry mnxEntry in mnx.entries)
        {
            var   mnxValid      = true;
            var   startSector   = (byte)(mnxEntry.start_sector & 0x3F);
            var   startCylinder = (ushort)((mnxEntry.start_sector & 0xC0) << 2 | mnxEntry.start_cylinder);
            var   endSector     = (byte)(mnxEntry.end_sector & 0x3F);
            var   endCylinder   = (ushort)((mnxEntry.end_sector & 0xC0) << 2 | mnxEntry.end_cylinder);
            ulong mnxStart      = mnxEntry.lba_start;
            ulong mnxSectors    = mnxEntry.lba_sectors;

            AaruConsole.DebugWriteLine(MODULE_NAME, "mnx_entry.status {0}",         mnxEntry.status);
            AaruConsole.DebugWriteLine(MODULE_NAME, "mnx_entry.type {0}",           mnxEntry.type);
            AaruConsole.DebugWriteLine(MODULE_NAME, "mnx_entry.lba_start {0}",      mnxEntry.lba_start);
            AaruConsole.DebugWriteLine(MODULE_NAME, "mnx_entry.lba_sectors {0}",    mnxEntry.lba_sectors);
            AaruConsole.DebugWriteLine(MODULE_NAME, "mnx_entry.start_cylinder {0}", startCylinder);
            AaruConsole.DebugWriteLine(MODULE_NAME, "mnx_entry.start_head {0}",     mnxEntry.start_head);
            AaruConsole.DebugWriteLine(MODULE_NAME, "mnx_entry.start_sector {0}",   startSector);
            AaruConsole.DebugWriteLine(MODULE_NAME, "mnx_entry.end_cylinder {0}",   endCylinder);
            AaruConsole.DebugWriteLine(MODULE_NAME, "mnx_entry.end_head {0}",       mnxEntry.end_head);
            AaruConsole.DebugWriteLine(MODULE_NAME, "mnx_entry.end_sector {0}",     endSector);

            mnxValid &= mnxEntry.status is 0x00 or 0x80;
            mnxValid &= mnxEntry.type is 0x81 or 0x80;

            mnxValid &= mnxEntry.lba_start      != 0 ||
                        mnxEntry.lba_sectors    != 0 ||
                        mnxEntry.start_cylinder != 0 ||
                        mnxEntry.start_head     != 0 ||
                        mnxEntry.start_sector   != 0 ||
                        mnxEntry.end_cylinder   != 0 ||
                        mnxEntry.end_head       != 0 ||
                        mnxEntry.end_sector     != 0;

            if(mnxEntry is { lba_start: 0, lba_sectors: 0 } && mnxValid)
            {
                mnxStart = CHS.ToLBA(startCylinder, mnxEntry.start_head, startSector, imagePlugin.Info.Heads,
                                     imagePlugin.Info.SectorsPerTrack);

                mnxSectors = CHS.ToLBA(endCylinder, mnxEntry.end_head, mnxEntry.end_sector, imagePlugin.Info.Heads,
                                       imagePlugin.Info.SectorsPerTrack) -
                             mnxStart;
            }

            // For optical media
            mnxStart   /= divider;
            mnxSectors /= divider;

            AaruConsole.DebugWriteLine(MODULE_NAME, "mnx_start {0}",   mnxStart);
            AaruConsole.DebugWriteLine(MODULE_NAME, "mnx_sectors {0}", mnxSectors);

            if(!mnxValid)
                continue;

            var part = new Partition();

            if(mnxStart > 0 && mnxSectors > 0)
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
            part.Name        = Localization.MINIX;
            part.Description = mnxEntry.status == 0x80 ? Localization.Partition_is_bootable : "";
            part.Scheme      = "MINIX";

            partitions.Add(part);
        }

        return anyMnx;
    }

    static string DecodeMbrType(byte type) => _mbrTypes[type];

#region Nested type: DiskManagerMasterBootRecord

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

#endregion

#region Nested type: ExtendedBootRecord

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

#endregion

#region Nested type: MasterBootRecord

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

#endregion

#region Nested type: ModernMasterBootRecord

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

#endregion

#region Nested type: NecMasterBootRecord

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

#endregion

#region Nested type: PartitionEntry

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

#endregion

#region Nested type: SerializedMasterBootRecord

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

#endregion

#region Nested type: TimedMasterBootRecord

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

#endregion
}