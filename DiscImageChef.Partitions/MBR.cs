// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.DiscImages;

namespace DiscImageChef.Partitions
{
    // TODO: Support AAP extensions
    public class MBR : PartitionPlugin
    {
        const ulong GptMagic = 0x5452415020494645;

        public MBR()
        {
            Name = "Master Boot Record";
            PluginUuid = new Guid("5E8A34E8-4F1A-59E6-4BF7-7EA647063A76");
        }

        public override bool GetInformation(DiscImages.ImagePlugin imagePlugin,
                                            out List<CommonTypes.Partition> partitions, ulong sectorOffset)
        {
            ulong counter = 0;

            partitions = new List<CommonTypes.Partition>();

            if(imagePlugin.GetSectorSize() < 512) return false;

            uint sectorSize = imagePlugin.GetSectorSize();
            // Divider of sector size in MBR between real sector size
            ulong divider = 1;

            if(imagePlugin.ImageInfo.XmlMediaType == DiscImages.XmlMediaType.OpticalDisc)
            {
                sectorSize = 512;
                divider = 4;
            }

            byte[] sector = imagePlugin.ReadSector(sectorOffset);

            GCHandle handle = GCHandle.Alloc(sector, GCHandleType.Pinned);
            MasterBootRecord mbr =
                (MasterBootRecord)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(MasterBootRecord));
            TimedMasterBootRecord mbr_time =
                (TimedMasterBootRecord)Marshal.PtrToStructure(handle.AddrOfPinnedObject(),
                                                              typeof(TimedMasterBootRecord));
            SerializedMasterBootRecord mbr_serial =
                (SerializedMasterBootRecord)Marshal.PtrToStructure(handle.AddrOfPinnedObject(),
                                                                   typeof(SerializedMasterBootRecord));
            ModernMasterBootRecord mbr_modern =
                (ModernMasterBootRecord)Marshal.PtrToStructure(handle.AddrOfPinnedObject(),
                                                               typeof(ModernMasterBootRecord));
            NecMasterBootRecord mbr_nec =
                (NecMasterBootRecord)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(NecMasterBootRecord));
            DiskManagerMasterBootRecord mbr_ontrack =
                (DiskManagerMasterBootRecord)Marshal.PtrToStructure(handle.AddrOfPinnedObject(),
                                                                    typeof(DiskManagerMasterBootRecord));
            handle.Free();

            DicConsole.DebugWriteLine("MBR plugin", "xmlmedia = {0}", imagePlugin.ImageInfo.XmlMediaType);
            DicConsole.DebugWriteLine("MBR plugin", "mbr.magic = {0:X4}", mbr.magic);

            if(mbr.magic != MBR_Magic) return false; // Not MBR

            byte[] hdrBytes = imagePlugin.ReadSector(1 + sectorOffset);

            ulong signature = BitConverter.ToUInt64(hdrBytes, 0);

            DicConsole.DebugWriteLine("MBR Plugin", "gpt.signature = 0x{0:X16}", signature);

            if(signature == GptMagic) return false;

            if(signature != GptMagic && imagePlugin.ImageInfo.XmlMediaType == XmlMediaType.OpticalDisc)
            {
                hdrBytes = imagePlugin.ReadSector(sectorOffset);
                signature = BitConverter.ToUInt64(hdrBytes, 512);
                DicConsole.DebugWriteLine("MBR Plugin", "gpt.signature @ 0x200 = 0x{0:X16}", signature);
                if(signature == GptMagic) return false;
            }

            MBRPartitionEntry[] entries;

            if(mbr_ontrack.dm_magic == DM_Magic) entries = mbr_ontrack.entries;
            else if(mbr_nec.nec_magic == NEC_Magic) entries = mbr_nec.entries;
            else entries = mbr.entries;

            foreach(MBRPartitionEntry entry in entries)
            {
                byte start_sector = (byte)(entry.start_sector & 0x3F);
                ushort start_cylinder = (ushort)(((entry.start_sector & 0xC0) << 2) | entry.start_cylinder);
                byte end_sector = (byte)(entry.end_sector & 0x3F);
                ushort end_cylinder = (ushort)(((entry.end_sector & 0xC0) << 2) | entry.end_cylinder);
                ulong lba_start = entry.lba_start;
                ulong lba_sectors = entry.lba_sectors;

                // Let's start the fun...

                bool valid = true;
                bool extended = false;
                bool minix = false;

                if(entry.status != 0x00 && entry.status != 0x80) return false; // Maybe a FAT filesystem

                valid &= entry.type != 0x00;
                if(entry.type == 0x05 || entry.type == 0x0F || entry.type == 0x15 || entry.type == 0x1F ||
                   entry.type == 0x85 || entry.type == 0x91 || entry.type == 0x9B || entry.type == 0xC5 ||
                   entry.type == 0xCF || entry.type == 0xD5)
                {
                    valid = false;
                    extended = true; // Extended partition
                }
                minix |= entry.type == 0x81 || entry.type == 0x80; // MINIX partition

                valid &= entry.lba_start != 0 || entry.lba_sectors != 0 || entry.start_cylinder != 0 ||
                         entry.start_head != 0 || entry.start_sector != 0 || entry.end_cylinder != 0 ||
                         entry.end_head != 0 || entry.end_sector != 0;
                if(entry.lba_start == 0 && entry.lba_sectors == 0 && valid)
                {
                    lba_start = Helpers.CHS.ToLBA(start_cylinder, entry.start_head, start_sector,
                                                  imagePlugin.ImageInfo.Heads, imagePlugin.ImageInfo.SectorsPerTrack);
                    lba_sectors = Helpers.CHS.ToLBA(end_cylinder, entry.end_head, entry.end_sector,
                                                    imagePlugin.ImageInfo.Heads,
                                                    imagePlugin.ImageInfo.SectorsPerTrack) - lba_start;
                }

                // For optical media
                lba_start /= divider;
                lba_sectors /= divider;

                if(minix && lba_start == sectorOffset) minix = false;

                if(lba_start > imagePlugin.GetSectors())
                {
                    valid = false;
                    extended = false;
                }

                // Some buggy implementations do some rounding errors getting a few sectors beyond device size
                if(lba_start + lba_sectors > imagePlugin.GetSectors())
                    lba_sectors = imagePlugin.GetSectors() - lba_start;

                DicConsole.DebugWriteLine("MBR plugin", "entry.status {0}", entry.status);
                DicConsole.DebugWriteLine("MBR plugin", "entry.type {0}", entry.type);
                DicConsole.DebugWriteLine("MBR plugin", "entry.lba_start {0}", entry.lba_start);
                DicConsole.DebugWriteLine("MBR plugin", "entry.lba_sectors {0}", entry.lba_sectors);
                DicConsole.DebugWriteLine("MBR plugin", "entry.start_cylinder {0}", start_cylinder);
                DicConsole.DebugWriteLine("MBR plugin", "entry.start_head {0}", entry.start_head);
                DicConsole.DebugWriteLine("MBR plugin", "entry.start_sector {0}", start_sector);
                DicConsole.DebugWriteLine("MBR plugin", "entry.end_cylinder {0}", end_cylinder);
                DicConsole.DebugWriteLine("MBR plugin", "entry.end_head {0}", entry.end_head);
                DicConsole.DebugWriteLine("MBR plugin", "entry.end_sector {0}", end_sector);

                DicConsole.DebugWriteLine("MBR plugin", "entry.minix = {0}", minix);

                DicConsole.DebugWriteLine("MBR plugin", "lba_start {0}", lba_start);
                DicConsole.DebugWriteLine("MBR plugin", "lba_sectors {0}", lba_sectors);

                if(valid && minix) // Let's mix the fun
                {
                    if(GetMinix(imagePlugin, lba_start, divider, sectorOffset, sectorSize,
                                out List<Partition> mnx_parts)) partitions.AddRange(mnx_parts);
                    else minix = false;
                }

                if(valid && !minix)
                {
                    CommonTypes.Partition part = new CommonTypes.Partition();
                    if((lba_start > 0 || imagePlugin.ImageInfo.XmlMediaType == XmlMediaType.OpticalDisc) &&
                       lba_sectors > 0)
                    {
                        part.Start = lba_start + sectorOffset;
                        part.Length = lba_sectors;
                        part.Offset = part.Start * sectorSize;
                        part.Size = part.Length * sectorSize;
                    }
                    else valid = false;

                    if(valid)
                    {
                        part.Type = string.Format("0x{0:X2}", entry.type);
                        part.Name = DecodeMBRType(entry.type);
                        part.Sequence = counter;
                        part.Description = entry.status == 0x80 ? "Partition is bootable." : "";
                        part.Scheme = Name;

                        counter++;

                        partitions.Add(part);
                    }
                }

                DicConsole.DebugWriteLine("MBR plugin", "entry.extended = {0}", extended);

                if(extended) // Let's extend the fun
                {
                    bool processing_extended = true;
                    ulong chain_start = lba_start;

                    while(processing_extended)
                    {
                        sector = imagePlugin.ReadSector(lba_start);

                        handle = GCHandle.Alloc(sector, GCHandleType.Pinned);
                        ExtendedBootRecord ebr =
                            (ExtendedBootRecord)Marshal.PtrToStructure(handle.AddrOfPinnedObject(),
                                                                       typeof(ExtendedBootRecord));
                        handle.Free();

                        DicConsole.DebugWriteLine("MBR plugin", "ebr.magic == MBR_Magic = {0}", ebr.magic == MBR_Magic);

                        if(ebr.magic != MBR_Magic) break;

                        ulong next_start = 0;

                        foreach(MBRPartitionEntry ebr_entry in ebr.entries)
                        {
                            bool ext_valid = true;
                            start_sector = (byte)(ebr_entry.start_sector & 0x3F);
                            start_cylinder =
                                (ushort)(((ebr_entry.start_sector & 0xC0) << 2) | ebr_entry.start_cylinder);
                            end_sector = (byte)(ebr_entry.end_sector & 0x3F);
                            end_cylinder = (ushort)(((ebr_entry.end_sector & 0xC0) << 2) | ebr_entry.end_cylinder);
                            ulong ext_start = ebr_entry.lba_start;
                            ulong ext_sectors = ebr_entry.lba_sectors;
                            bool ext_minix = false;

                            DicConsole.DebugWriteLine("MBR plugin", "ebr_entry.status {0}", ebr_entry.status);
                            DicConsole.DebugWriteLine("MBR plugin", "ebr_entry.type {0}", ebr_entry.type);
                            DicConsole.DebugWriteLine("MBR plugin", "ebr_entry.lba_start {0}", ebr_entry.lba_start);
                            DicConsole.DebugWriteLine("MBR plugin", "ebr_entry.lba_sectors {0}", ebr_entry.lba_sectors);
                            DicConsole.DebugWriteLine("MBR plugin", "ebr_entry.start_cylinder {0}", start_cylinder);
                            DicConsole.DebugWriteLine("MBR plugin", "ebr_entry.start_head {0}", ebr_entry.start_head);
                            DicConsole.DebugWriteLine("MBR plugin", "ebr_entry.start_sector {0}", start_sector);
                            DicConsole.DebugWriteLine("MBR plugin", "ebr_entry.end_cylinder {0}", end_cylinder);
                            DicConsole.DebugWriteLine("MBR plugin", "ebr_entry.end_head {0}", ebr_entry.end_head);
                            DicConsole.DebugWriteLine("MBR plugin", "ebr_entry.end_sector {0}", end_sector);

                            // Let's start the fun...
                            ext_valid &= ebr_entry.status == 0x00 || ebr_entry.status == 0x80;
                            ext_valid &= ebr_entry.type != 0x00;
                            ext_valid &= ebr_entry.lba_start != 0 || ebr_entry.lba_sectors != 0 ||
                                         ebr_entry.start_cylinder != 0 || ebr_entry.start_head != 0 ||
                                         ebr_entry.start_sector != 0 || ebr_entry.end_cylinder != 0 ||
                                         ebr_entry.end_head != 0 || ebr_entry.end_sector != 0;
                            if(ebr_entry.lba_start == 0 && ebr_entry.lba_sectors == 0 && ext_valid)
                            {
                                ext_start = Helpers.CHS.ToLBA(start_cylinder, ebr_entry.start_head, start_sector,
                                                              imagePlugin.ImageInfo.Heads,
                                                              imagePlugin.ImageInfo.SectorsPerTrack);
                                ext_sectors =
                                    Helpers.CHS.ToLBA(end_cylinder, ebr_entry.end_head, ebr_entry.end_sector,
                                                      imagePlugin.ImageInfo.Heads,
                                                      imagePlugin.ImageInfo.SectorsPerTrack) - ext_start;
                            }
                            ext_minix |= ebr_entry.type == 0x81 || ebr_entry.type == 0x80;

                            // For optical media
                            ext_start /= divider;
                            ext_sectors /= divider;

                            DicConsole.DebugWriteLine("MBR plugin", "ext_start {0}", ext_start);
                            DicConsole.DebugWriteLine("MBR plugin", "ext_sectors {0}", ext_sectors);

                            if(ebr_entry.type == 0x05 || ebr_entry.type == 0x0F || ebr_entry.type == 0x15 ||
                               ebr_entry.type == 0x1F || ebr_entry.type == 0x85 || ebr_entry.type == 0x91 ||
                               ebr_entry.type == 0x9B || ebr_entry.type == 0xC5 || ebr_entry.type == 0xCF ||
                               ebr_entry.type == 0xD5)
                            {
                                ext_valid = false;
                                next_start = chain_start + ext_start;
                            }

                            ext_start += lba_start;
                            ext_valid &= ext_start <= imagePlugin.GetSectors();

                            // Some buggy implementations do some rounding errors getting a few sectors beyond device size
                            if(ext_start + ext_sectors > imagePlugin.GetSectors())
                                ext_sectors = imagePlugin.GetSectors() - ext_start;

                            if(ext_valid && ext_minix) // Let's mix the fun
                            {
                                if(GetMinix(imagePlugin, lba_start, divider, sectorOffset, sectorSize,
                                            out List<Partition> mnx_parts)) partitions.AddRange(mnx_parts);
                                else ext_minix = false;
                            }

                            if(ext_valid && !ext_minix)
                            {
                                Partition part = new Partition();
                                if(ext_start > 0 && ext_sectors > 0)
                                {
                                    part.Start = ext_start + sectorOffset;
                                    part.Length = ext_sectors;
                                    part.Offset = part.Start * sectorSize;
                                    part.Size = part.Length * sectorSize;
                                }
                                else ext_valid = false;

                                if(ext_valid)
                                {
                                    part.Type = string.Format("0x{0:X2}", ebr_entry.type);
                                    part.Name = DecodeMBRType(ebr_entry.type);
                                    part.Sequence = counter;
                                    part.Description = ebr_entry.status == 0x80 ? "Partition is bootable." : "";
                                    part.Scheme = Name;
                                    counter++;

                                    partitions.Add(part);
                                }
                            }
                        }

                        DicConsole.DebugWriteLine("MBR plugin", "next_start {0}", next_start);
                        processing_extended &= next_start != 0;
                        processing_extended &= next_start <= imagePlugin.GetSectors();
                        lba_start = next_start;
                    }
                }
            }

            // An empty MBR may exist, NeXT creates one and then hardcodes its disklabel
            return partitions.Count != 0;
        }

        static bool GetMinix(ImagePlugin imagePlugin, ulong start, ulong divider, ulong sectorOffset, uint sectorSize,
                             out List<Partition> partitions)
        {
            partitions = new List<Partition>();

            byte[] sector = imagePlugin.ReadSector(start);

            GCHandle handle = GCHandle.Alloc(sector, GCHandleType.Pinned);
            ExtendedBootRecord mnx =
                (ExtendedBootRecord)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(ExtendedBootRecord));
            handle.Free();

            DicConsole.DebugWriteLine("MBR plugin", "mnx.magic == MBR_Magic = {0}", mnx.magic == MBR_Magic);

            if(mnx.magic != MBR_Magic) return false;

            bool any_mnx = false;

            foreach(MBRPartitionEntry mnx_entry in mnx.entries)
            {
                bool mnx_valid = true;
                byte start_sector = (byte)(mnx_entry.start_sector & 0x3F);
                ushort start_cylinder = (ushort)(((mnx_entry.start_sector & 0xC0) << 2) | mnx_entry.start_cylinder);
                byte end_sector = (byte)(mnx_entry.end_sector & 0x3F);
                ushort end_cylinder = (ushort)(((mnx_entry.end_sector & 0xC0) << 2) | mnx_entry.end_cylinder);
                ulong mnx_start = mnx_entry.lba_start;
                ulong mnx_sectors = mnx_entry.lba_sectors;

                DicConsole.DebugWriteLine("MBR plugin", "mnx_entry.status {0}", mnx_entry.status);
                DicConsole.DebugWriteLine("MBR plugin", "mnx_entry.type {0}", mnx_entry.type);
                DicConsole.DebugWriteLine("MBR plugin", "mnx_entry.lba_start {0}", mnx_entry.lba_start);
                DicConsole.DebugWriteLine("MBR plugin", "mnx_entry.lba_sectors {0}", mnx_entry.lba_sectors);
                DicConsole.DebugWriteLine("MBR plugin", "mnx_entry.start_cylinder {0}", start_cylinder);
                DicConsole.DebugWriteLine("MBR plugin", "mnx_entry.start_head {0}", mnx_entry.start_head);
                DicConsole.DebugWriteLine("MBR plugin", "mnx_entry.start_sector {0}", start_sector);
                DicConsole.DebugWriteLine("MBR plugin", "mnx_entry.end_cylinder {0}", end_cylinder);
                DicConsole.DebugWriteLine("MBR plugin", "mnx_entry.end_head {0}", mnx_entry.end_head);
                DicConsole.DebugWriteLine("MBR plugin", "mnx_entry.end_sector {0}", end_sector);

                mnx_valid &= mnx_entry.status == 0x00 || mnx_entry.status == 0x80;
                mnx_valid &= mnx_entry.type == 0x81 || mnx_entry.type == 0x80;
                mnx_valid &= mnx_entry.lba_start != 0 || mnx_entry.lba_sectors != 0 || mnx_entry.start_cylinder != 0 ||
                             mnx_entry.start_head != 0 || mnx_entry.start_sector != 0 || mnx_entry.end_cylinder != 0 ||
                             mnx_entry.end_head != 0 || mnx_entry.end_sector != 0;
                if(mnx_entry.lba_start == 0 && mnx_entry.lba_sectors == 0 && mnx_valid)
                {
                    mnx_start = Helpers.CHS.ToLBA(start_cylinder, mnx_entry.start_head, start_sector,
                                                  imagePlugin.ImageInfo.Heads, imagePlugin.ImageInfo.SectorsPerTrack);
                    mnx_sectors = Helpers.CHS.ToLBA(end_cylinder, mnx_entry.end_head, mnx_entry.end_sector,
                                                    imagePlugin.ImageInfo.Heads,
                                                    imagePlugin.ImageInfo.SectorsPerTrack) - mnx_start;
                }

                // For optical media
                mnx_start /= divider;
                mnx_sectors /= divider;

                DicConsole.DebugWriteLine("MBR plugin", "mnx_start {0}", mnx_start);
                DicConsole.DebugWriteLine("MBR plugin", "mnx_sectors {0}", mnx_sectors);

                if(mnx_valid)
                {
                    CommonTypes.Partition part = new CommonTypes.Partition();
                    if(mnx_start > 0 && mnx_sectors > 0)
                    {
                        part.Start = mnx_start + sectorOffset;
                        part.Length = mnx_sectors;
                        part.Offset = part.Start * sectorSize;
                        part.Size = part.Length * sectorSize;
                    }
                    else mnx_valid = false;

                    if(mnx_valid)
                    {
                        any_mnx = true;
                        part.Type = "MINIX";
                        part.Name = "MINIX";
                        part.Description = mnx_entry.status == 0x80 ? "Partition is bootable." : "";
                        part.Scheme = "MINIX";

                        partitions.Add(part);
                    }
                }
            }

            return any_mnx;
        }

        static readonly string[] MBRTypes =
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

        static string DecodeMBRType(byte type)
        {
            return MBRTypes[type];
        }

        const ushort MBR_Magic = 0xAA55;
        const ushort NEC_Magic = 0xA55A;
        const ushort DM_Magic = 0x55AA;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct MasterBootRecord
        {
            /// <summary>Boot code</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 446)] public byte[] boot_code;
            /// <summary>Partitions</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public MBRPartitionEntry[] entries;
            /// <summary><see cref="MBR_Magic"/></summary>
            public ushort magic;
        }

        // TODO: IBM Boot Manager
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ExtendedBootRecord
        {
            /// <summary>Boot code, almost always unused</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 446)] public byte[] boot_code;
            /// <summary>Partitions or pointers</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public MBRPartitionEntry[] entries;
            /// <summary><see cref="MBR_Magic"/></summary>
            public ushort magic;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TimedMasterBootRecord
        {
            /// <summary>Boot code</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 218)] public byte[] boot_code;
            /// <summary>Set to 0</summary>
            public ushort zero;
            /// <summary>Original physical drive</summary>
            public byte drive;
            /// <summary>Disk timestamp, seconds</summary>
            public byte seconds;
            /// <summary>Disk timestamp, minutes</summary>
            public byte minutes;
            /// <summary>Disk timestamp, hours</summary>
            public byte hours;
            /// <summary>Boot code, continuation</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 222)] public byte[] boot_code2;
            /// <summary>Partitions</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public MBRPartitionEntry[] entries;
            /// <summary><see cref="MBR_Magic"/></summary>
            public ushort magic;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct SerializedMasterBootRecord
        {
            /// <summary>Boot code</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 440)] public byte[] boot_code;
            /// <summary>Disk serial number</summary>
            public uint serial;
            /// <summary>Set to 0</summary>
            public ushort zero;
            /// <summary>Partitions</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public MBRPartitionEntry[] entries;
            /// <summary><see cref="MBR_Magic"/></summary>
            public ushort magic;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ModernMasterBootRecord
        {
            /// <summary>Boot code</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 218)] public byte[] boot_code;
            /// <summary>Set to 0</summary>
            public ushort zero;
            /// <summary>Original physical drive</summary>
            public byte drive;
            /// <summary>Disk timestamp, seconds</summary>
            public byte seconds;
            /// <summary>Disk timestamp, minutes</summary>
            public byte minutes;
            /// <summary>Disk timestamp, hours</summary>
            public byte hours;
            /// <summary>Boot code, continuation</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 216)] public byte[] boot_code2;
            /// <summary>Disk serial number</summary>
            public uint serial;
            /// <summary>Set to 0</summary>
            public ushort zero2;
            /// <summary>Partitions</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public MBRPartitionEntry[] entries;
            /// <summary><see cref="MBR_Magic"/></summary>
            public ushort magic;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct NecMasterBootRecord
        {
            /// <summary>Boot code</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 380)] public byte[] boot_code;
            /// <summary><see cref="NEC_Magic"/></summary>
            public ushort nec_magic;
            /// <summary>Partitions</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public MBRPartitionEntry[] entries;
            /// <summary><see cref="MBR_Magic"/></summary>
            public ushort magic;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DiskManagerMasterBootRecord
        {
            /// <summary>Boot code</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 252)] public byte[] boot_code;
            /// <summary><see cref="DM_Magic"/></summary>
            public ushort dm_magic;
            /// <summary>Partitions</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public MBRPartitionEntry[] entries;
            /// <summary><see cref="MBR_Magic"/></summary>
            public ushort magic;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct MBRPartitionEntry
        {
            /// <summary>Partition status, 0x80 or 0x00, else invalid</summary>
            public byte status;
            /// <summary>Starting head [0,254]</summary>
            public byte start_head;
            /// <summary>Starting sector [1,63]</summary>
            public byte start_sector;
            /// <summary>Starting cylinder [0,1023]</summary>
            public byte start_cylinder;
            /// <summary>Partition type</summary>
            public byte type;
            /// <summary>Ending head [0,254]</summary>
            public byte end_head;
            /// <summary>Ending sector [1,63]</summary>
            public byte end_sector;
            /// <summary>Ending cylinder [0,1023]</summary>
            public byte end_cylinder;
            /// <summary>Starting absolute sector</summary>
            public uint lba_start;
            /// <summary>Total sectors</summary>
            public uint lba_sectors;
        }
    }
}