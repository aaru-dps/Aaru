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
//     Manages Intel/Microsoft MBR and UNIX slicing inside it.
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DiscImageChef.Console;

// TODO: Support AAP extensions
namespace DiscImageChef.PartPlugins
{
    public class MBR : PartPlugin
    {
        public MBR()
        {
            Name = "Master Boot Record";
            PluginUUID = new Guid("5E8A34E8-4F1A-59E6-4BF7-7EA647063A76");
        }

        public override bool GetInformation(ImagePlugins.ImagePlugin imagePlugin, out List<CommonTypes.Partition> partitions, ulong sectorOffset)
        {
            ulong counter = 0;

            partitions = new List<CommonTypes.Partition>();

            if(imagePlugin.GetSectorSize() < 512)
                return false;

            uint sectorSize = imagePlugin.GetSectorSize();
            // Divider of sector size in MBR between real sector size
            ulong divider = 1;

            if(imagePlugin.ImageInfo.xmlMediaType == ImagePlugins.XmlMediaType.OpticalDisc)
            {
                sectorSize = 512;
                divider = 4;
            }

            byte[] sector = imagePlugin.ReadSector(sectorOffset);

            GCHandle handle = GCHandle.Alloc(sector, GCHandleType.Pinned);
            MasterBootRecord mbr = (MasterBootRecord)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(MasterBootRecord));
            TimedMasterBootRecord mbr_time = (TimedMasterBootRecord)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(TimedMasterBootRecord));
            SerializedMasterBootRecord mbr_serial = (SerializedMasterBootRecord)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(SerializedMasterBootRecord));
            ModernMasterBootRecord mbr_modern = (ModernMasterBootRecord)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(ModernMasterBootRecord));
            NecMasterBootRecord mbr_nec = (NecMasterBootRecord)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(NecMasterBootRecord));
            DiskManagerMasterBootRecord mbr_ontrack = (DiskManagerMasterBootRecord)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(DiskManagerMasterBootRecord));
            handle.Free();

            DicConsole.DebugWriteLine("MBR plugin", "mbr.magic = {0:X4}", mbr.magic);

            if(mbr.magic != MBR_Magic)
                return false; // Not MBR

            MBRPartitionEntry[] entries;

            if(mbr_ontrack.dm_magic == DM_Magic)
                entries = mbr_ontrack.entries;
            else if(mbr_nec.nec_magic == NEC_Magic)
                entries = mbr_nec.entries;
            else
                entries = mbr.entries;

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
                //bool disklabel = false;

                if(entry.status != 0x00 && entry.status != 0x80)
                    return false; // Maybe a FAT filesystem
                valid &= entry.type != 0x00;
                if(entry.type == 0xEE || entry.type == 0xEF)
                    return false; // This is a GPT
                if(entry.type == 0x05 || entry.type == 0x0F || entry.type == 0x15 || entry.type == 0x1F || entry.type == 0x85 ||
                  entry.type == 0x91 || entry.type == 0x9B || entry.type == 0xC5 || entry.type == 0xCF || entry.type == 0xD5)
                {
                    valid = false;
                    extended = true; // Extended partition
                }

                valid &= entry.lba_start != 0 || entry.lba_sectors != 0 || entry.start_cylinder != 0 || entry.start_head != 0 || entry.start_sector != 0 || entry.end_cylinder != 0 || entry.end_head != 0 || entry.end_sector != 0;
                if(entry.lba_start == 0 && entry.lba_sectors == 0 && valid)
                {
                    lba_start = CHStoLBA(start_cylinder, entry.start_head, start_sector);
                    lba_sectors = CHStoLBA(end_cylinder, entry.end_head, entry.end_sector) - lba_start;
                }

                // For optical media
                lba_start /= divider;
                lba_sectors /= divider;

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

                if(valid)
                {
                    CommonTypes.Partition part = new CommonTypes.Partition();
                    if(lba_start > 0 && lba_sectors > 0)
                    {
                        part.Start = entry.lba_start + sectorOffset;
                        part.Length = entry.lba_sectors;
                        part.Offset = part.Start * sectorSize;
                        part.Size = part.Length * sectorSize;
                    }
                    else
                        valid = false;

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
                        ExtendedBootRecord ebr = (ExtendedBootRecord)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(ExtendedBootRecord));
                        handle.Free();

                        DicConsole.DebugWriteLine("MBR plugin", "ebr.magic == MBR_Magic = {0}", ebr.magic == MBR_Magic);

                        if(ebr.magic != MBR_Magic)
                            break;

                        ulong next_start = 0;

                        foreach(MBRPartitionEntry ebr_entry in ebr.entries)
                        {
                            bool ext_valid = true;
                            start_sector = (byte)(ebr_entry.start_sector & 0x3F);
                            start_cylinder = (ushort)(((ebr_entry.start_sector & 0xC0) << 2) | ebr_entry.start_cylinder);
                            end_sector = (byte)(ebr_entry.end_sector & 0x3F);
                            end_cylinder = (ushort)(((ebr_entry.end_sector & 0xC0) << 2) | ebr_entry.end_cylinder);
                            ulong ext_start = ebr_entry.lba_start;
                            ulong ext_sectors = ebr_entry.lba_sectors;

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
                            ext_valid &= ebr_entry.lba_start != 0 || ebr_entry.lba_sectors != 0 || ebr_entry.start_cylinder != 0 || ebr_entry.start_head != 0 ||
                                            ebr_entry.start_sector != 0 || ebr_entry.end_cylinder != 0 || ebr_entry.end_head != 0 || ebr_entry.end_sector != 0;
                            if(ebr_entry.lba_start == 0 && ebr_entry.lba_sectors == 0 && ext_valid)
                            {
                                ext_start = CHStoLBA(start_cylinder, ebr_entry.start_head, start_sector);
                                ext_sectors = CHStoLBA(end_cylinder, ebr_entry.end_head, ebr_entry.end_sector) - ext_start;
                            }

                            // For optical media
                            lba_start /= divider;
                            lba_sectors /= divider;

                            if(ebr_entry.type == 0x05 || ebr_entry.type == 0x0F || ebr_entry.type == 0x15 || ebr_entry.type == 0x1F || ebr_entry.type == 0x85 ||
                                ebr_entry.type == 0x91 || ebr_entry.type == 0x9B || ebr_entry.type == 0xC5 || ebr_entry.type == 0xCF || ebr_entry.type == 0xD5)
                            {
                                ext_valid = false;
                                next_start = chain_start + ext_start;
                            }

                            ext_start += lba_start;
                            ext_valid &= ext_start <= imagePlugin.GetSectors();

                            // Some buggy implementations do some rounding errors getting a few sectors beyond device size
                            if(ext_start + ext_sectors > imagePlugin.GetSectors())
                                ext_sectors = imagePlugin.GetSectors() - ext_start;

                            if(ext_valid)
                            {
                                CommonTypes.Partition part = new CommonTypes.Partition();
                                if(ext_start > 0 && ext_sectors > 0)
                                {
                                    part.Start = ext_start + sectorOffset;
                                    part.Length = ext_sectors;
                                    part.Offset = part.Start * sectorSize;
                                    part.Size = part.Length * sectorSize;
                                }
                                else
                                    ext_valid = false;

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
                        processing_extended &= (next_start <= imagePlugin.GetSectors());
                        lba_start = next_start;
                    }
                }
            }

            // An empty MBR may exist, NeXT creates one and then hardcodes its disklabel
            return partitions.Count != 0;
        }

        static uint CHStoLBA(ushort cyl, byte head, byte sector)
        {
#pragma warning disable IDE0004 // Remove Unnecessary Cast
            return (((uint)cyl * 16) + (uint)head) * 63 + (uint)sector - 1;
#pragma warning restore IDE0004 // Remove Unnecessary Cast
        }

        static string DecodeMBRType(byte type)
        {
            switch(type)
            {
                case 0x01:
                    return "FAT12";
                case 0x02:
                    return "XENIX root";
                case 0x03:
                    return "XENIX /usr";
                case 0x04:
                    return "FAT16 < 32 MiB";
                case 0x05:
                    return "Extended";
                case 0x06:
                    return "FAT16";
                case 0x07:
                    return "IFS (HPFS/NTFS)";
                case 0x08:
                    return "AIX boot, OS/2, Commodore DOS";
                case 0x09:
                    return "AIX data, Coherent, QNX";
                case 0x0A:
                    return "Coherent swap, OPUS, OS/2 Boot Manager";
                case 0x0B:
                    return "FAT32";
                case 0x0C:
                    return "FAT32 (LBA)";
                case 0x0E:
                    return "FAT16 (LBA)";
                case 0x0F:
                    return "Extended (LBA)";
                case 0x10:
                    return "OPUS";
                case 0x11:
                    return "Hidden FAT12";
                case 0x12:
                    return "Compaq diagnostics, recovery partition";
                case 0x14:
                    return "Hidden FAT16 < 32 MiB, AST-DOS";
                case 0x16:
                    return "Hidden FAT16";
                case 0x17:
                    return "Hidden IFS (HPFS/NTFS)";
                case 0x18:
                    return "AST-Windows swap";
                case 0x19:
                    return "Willowtech Photon coS";
                case 0x1B:
                    return "Hidden FAT32";
                case 0x1C:
                    return "Hidden FAT32 (LBA)";
                case 0x1E:
                    return "Hidden FAT16 (LBA)";
                case 0x20:
                    return "Willowsoft Overture File System";
                case 0x21:
                    return "Oxygen FSo2";
                case 0x22:
                    return "Oxygen Extended ";
                case 0x23:
                    return "SpeedStor reserved";
                case 0x24:
                    return "NEC-DOS";
                case 0x26:
                    return "SpeedStor reserved";
                case 0x27:
                    return "Hidden NTFS";
                case 0x31:
                    return "SpeedStor reserved";
                case 0x33:
                    return "SpeedStor reserved";
                case 0x34:
                    return "SpeedStor reserved";
                case 0x36:
                    return "SpeedStor reserved";
                case 0x38:
                    return "Theos";
                case 0x39:
                    return "Plan 9";
                case 0x3C:
                    return "Partition Magic";
                case 0x3D:
                    return "Hidden NetWare";
                case 0x40:
                    return "VENIX 80286";
                case 0x41:
                    return "PReP Boot";
                case 0x42:
                    return "Secure File System";
                case 0x43:
                    return "PTS-DOS";
                case 0x45:
                    return "Priam, EUMEL/Elan";
                case 0x46:
                    return "EUMEL/Elan";
                case 0x47:
                    return "EUMEL/Elan";
                case 0x48:
                    return "EUMEL/Elan";
                case 0x4A:
                    return "ALFS/THIN lightweight filesystem for DOS";
                case 0x4D:
                    return "QNX 4";
                case 0x4E:
                    return "QNX 4";
                case 0x4F:
                    return "QNX 4, Oberon";
                case 0x50:
                    return "Ontrack DM, R/O, FAT";
                case 0x51:
                    return "Ontrack DM, R/W, FAT";
                case 0x52:
                    return "CP/M, Microport UNIX";
                case 0x53:
                    return "Ontrack DM 6";
                case 0x54:
                    return "Ontrack DM 6";
                case 0x55:
                    return "EZ-Drive";
                case 0x56:
                    return "Golden Bow VFeature";
                case 0x5C:
                    return "Priam EDISK";
                case 0x61:
                    return "SpeedStor";
                case 0x63:
                    return "GNU Hurd, System V, 386/ix";
                case 0x64:
                    return "NetWare 286";
                case 0x65:
                    return "NetWare";
                case 0x66:
                    return "NetWare 386";
                case 0x67:
                    return "NetWare";
                case 0x68:
                    return "NetWare";
                case 0x69:
                    return "NetWare NSS";
                case 0x70:
                    return "DiskSecure Multi-Boot";
                case 0x72:
                    return "UNIX 7th Edition";
                case 0x75:
                    return "IBM PC/IX";
                case 0x80:
                    return "Old MINIX";
                case 0x81:
                    return "MINIX, Old Linux";
                case 0x82:
                    return "Linux swap, Solaris";
                case 0x83:
                    return "Linux";
                case 0x84:
                    return "Hidden by OS/2, APM hibernation";
                case 0x85:
                    return "Linux extended";
                case 0x86:
                    return "NT Stripe Set";
                case 0x87:
                    return "NT Stripe Set";
                case 0x88:
                    return "Linux Plaintext";
                case 0x8E:
                    return "Linux LVM";
                case 0x93:
                    return "Amoeba, Hidden Linux";
                case 0x94:
                    return "Amoeba bad blocks";
                case 0x99:
                    return "Mylex EISA SCSI";
                case 0x9F:
                    return "BSD/OS";
                case 0xA0:
                    return "Hibernation";
                case 0xA1:
                    return "HP Volume Expansion";
                case 0xA3:
                    return "HP Volume Expansion";
                case 0xA4:
                    return "HP Volume Expansion";
                case 0xA5:
                    return "FreeBSD";
                case 0xA6:
                    return "OpenBSD";
                case 0xA7:
                    return "NeXTStep";
                case 0xA8:
                    return "Apple UFS";
                case 0xA9:
                    return "NetBSD";
                case 0xAA:
                    return "Olivetti DOS FAT12";
                case 0xAB:
                    return "Apple Boot";
                case 0xAF:
                    return "Apple HFS";
                case 0xB0:
                    return "BootStar";
                case 0xB1:
                    return "HP Volume Expansion";
                case 0xB3:
                    return "HP Volume Expansion";
                case 0xB4:
                    return "HP Volume Expansion";
                case 0xB6:
                    return "HP Volume Expansion";
                case 0xB7:
                    return "BSDi";
                case 0xB8:
                    return "BSDi swap";
                case 0xBB:
                    return "PTS BootWizard";
                case 0xBE:
                    return "Solaris boot";
                case 0xBF:
                    return "Solaris";
                case 0xC0:
                    return "Novell DOS, DR-DOS secured";
                case 0xC1:
                    return "DR-DOS secured FAT12";
                case 0xC2:
                    return "DR-DOS reserved";
                case 0xC3:
                    return "DR-DOS reserved";
                case 0xC4:
                    return "DR-DOS secured FAT16 < 32 MiB";
                case 0xC6:
                    return "DR-DOS secured FAT16";
                case 0xC7:
                    return "Syrinx";
                case 0xC8:
                    return "DR-DOS reserved";
                case 0xC9:
                    return "DR-DOS reserved";
                case 0xCA:
                    return "DR-DOS reserved";
                case 0xCB:
                    return "DR-DOS secured FAT32";
                case 0xCC:
                    return "DR-DOS secured FAT32 (LBA)";
                case 0xCD:
                    return "DR-DOS reserved";
                case 0xCE:
                    return "DR-DOS secured FAT16 (LBA)";
                case 0xCF:
                    return "DR-DOS secured extended (LBA)";
                case 0xD0:
                    return "Multiuser DOS secured FAT12";
                case 0xD1:
                    return "Multiuser DOS secured FAT12";
                case 0xD4:
                    return "Multiuser DOS secured FAT16 < 32 MiB";
                case 0xD5:
                    return "Multiuser DOS secured extended";
                case 0xD6:
                    return "Multiuser DOS secured FAT16";
                case 0xD8:
                    return "CP/M";
                case 0xDA:
                    return "Filesystem-less data";
                case 0xDB:
                    return "CP/M, CCP/M, CTOS";
                case 0xDE:
                    return "Dell partition";
                case 0xDF:
                    return "BootIt EMBRM";
                case 0xE1:
                    return "SpeedStor";
                case 0xE2:
                    return "DOS read/only";
                case 0xE3:
                    return "SpeedStor";
                case 0xE4:
                    return "SpeedStor";
                case 0xE5:
                    return "Tandy DOS";
                case 0xE6:
                    return "SpeedStor";
                case 0xEB:
                    return "BeOS";
                case 0xED:
                    return "Spryt*x";
                case 0xEE:
                    return "Guid Partition Table";
                case 0xEF:
                    return "EFI system partition";
                case 0xF0:
                    return "Linux boot";
                case 0xF1:
                    return "SpeedStor";
                case 0xF2:
                    return "DOS 3.3 secondary, Unisys DOS";
                case 0xF3:
                    return "SpeedStor";
                case 0xF4:
                    return "SpeedStor";
                case 0xF5:
                    return "Prologue";
                case 0xF6:
                    return "SpeedStor";
                case 0xFB:
                    return "VMWare VMFS";
                case 0xFC:
                    return "VMWare VMKCORE";
                case 0xFD:
                    return "Linux RAID, FreeDOS";
                case 0xFE:
                    return "SpeedStor, LANStep, PS/2 IML";
                case 0xFF:
                    return "Xenix bad block";
                default:
                    return "Unknown";
            }
        }

        public const ushort MBR_Magic = 0xAA55;
        public const ushort NEC_Magic = 0xA55A;
        public const ushort DM_Magic = 0x55AA;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MasterBootRecord
        {
            /// <summary>Boot code</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 446)]
            public byte[] boot_code;
            /// <summary>Partitions</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public MBRPartitionEntry[] entries;
            /// <summary><see cref="MBR_Magic"/></summary>
            public ushort magic;
        }

        // TODO: IBM Boot Manager
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ExtendedBootRecord
        {
            /// <summary>Boot code, almost always unused</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 446)]
            public byte[] boot_code;
            /// <summary>Partitions or pointers</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public MBRPartitionEntry[] entries;
            /// <summary><see cref="MBR_Magic"/></summary>
            public ushort magic;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TimedMasterBootRecord
        {
            /// <summary>Boot code</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 218)]
            public byte[] boot_code;
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
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 222)]
            public byte[] boot_code2;
            /// <summary>Partitions</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public MBRPartitionEntry[] entries;
            /// <summary><see cref="MBR_Magic"/></summary>
            public ushort magic;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SerializedMasterBootRecord
        {
            /// <summary>Boot code</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 440)]
            public byte[] boot_code;
            /// <summary>Disk serial number</summary>
            public uint serial;
            /// <summary>Set to 0</summary>
            public ushort zero;
            /// <summary>Partitions</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public MBRPartitionEntry[] entries;
            /// <summary><see cref="MBR_Magic"/></summary>
            public ushort magic;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ModernMasterBootRecord
        {
            /// <summary>Boot code</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 218)]
            public byte[] boot_code;
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
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 216)]
            public byte[] boot_code2;
            /// <summary>Disk serial number</summary>
            public uint serial;
            /// <summary>Set to 0</summary>
            public ushort zero2;
            /// <summary>Partitions</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public MBRPartitionEntry[] entries;
            /// <summary><see cref="MBR_Magic"/></summary>
            public ushort magic;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct NecMasterBootRecord
        {
            /// <summary>Boot code</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 380)]
            public byte[] boot_code;
            /// <summary><see cref="NEC_Magic"/></summary>
            public ushort nec_magic;
            /// <summary>Partitions</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public MBRPartitionEntry[] entries;
            /// <summary><see cref="MBR_Magic"/></summary>
            public ushort magic;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DiskManagerMasterBootRecord
        {
            /// <summary>Boot code</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 252)]
            public byte[] boot_code;
            /// <summary><see cref="DM_Magic"/></summary>
            public ushort dm_magic;
            /// <summary>Partitions</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public MBRPartitionEntry[] entries;
            /// <summary><see cref="MBR_Magic"/></summary>
            public ushort magic;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MBRPartitionEntry
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