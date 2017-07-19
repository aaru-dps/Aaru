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

// TODO: Support AAP, AST, SpeedStor and Ontrack extensions
namespace DiscImageChef.PartPlugins
{
    public class MBR : PartPlugin
    {
        const ushort MBRSignature = 0xAA55;

        public MBR()
        {
            Name = "Master Boot Record";
            PluginUUID = new Guid("5E8A34E8-4F1A-59E6-4BF7-7EA647063A76");
        }

        public override bool GetInformation(ImagePlugins.ImagePlugin imagePlugin, out List<CommonTypes.Partition> partitions)
        {
            byte cyl_sect1, cyl_sect2; // For decoding cylinder and sector
            ushort signature;
            ulong counter = 0;

            partitions = new List<CommonTypes.Partition>();

            if(imagePlugin.GetSectorSize() < 512)
                return false;

            byte[] sector = imagePlugin.ReadSector(0);

            signature = BitConverter.ToUInt16(sector, 0x1FE);

            if(signature != MBRSignature)
                return false; // Not MBR

            for(int i = 0; i < 4; i++)
            {
                MBRPartitionEntry entry = new MBRPartitionEntry();

                entry.status = sector[0x1BE + 16 * i + 0x00];
                entry.start_head = sector[0x1BE + 16 * i + 0x01];

                cyl_sect1 = sector[0x1BE + 16 * i + 0x02];
                cyl_sect2 = sector[0x1BE + 16 * i + 0x03];

                entry.start_sector = (byte)(cyl_sect1 & 0x3F);
                entry.start_cylinder = (ushort)(((cyl_sect1 & 0xC0) << 2) | cyl_sect2);

                entry.type = sector[0x1BE + 16 * i + 0x04];
                entry.end_head = sector[0x1BE + 16 * i + 0x05];

                cyl_sect1 = sector[0x1BE + 16 * i + 0x06];
                cyl_sect2 = sector[0x1BE + 16 * i + 0x07];

                entry.end_sector = (byte)(cyl_sect1 & 0x3F);
                entry.end_cylinder = (ushort)(((cyl_sect1 & 0xC0) << 2) | cyl_sect2);

                entry.lba_start = BitConverter.ToUInt32(sector, 0x1BE + 16 * i + 0x08);
                entry.lba_sectors = BitConverter.ToUInt32(sector, 0x1BE + 16 * i + 0x0C);

                // Let's start the fun...

                bool valid = true;
                bool extended = false;
                bool disklabel = false;

                if(entry.status != 0x00 && entry.status != 0x80)
                    return false; // Maybe a FAT filesystem
                valid &= entry.type != 0x00;
                if(entry.type == 0xEE || entry.type == 0xEF)
                    return false; // This is a GPT
                if(entry.type == 0x05 || entry.type == 0x0F || entry.type == 0x85)
                {
                    valid = false;
                    extended = true; // Extended partition
                }
                if(entry.type == 0x82 || entry.type == 0xBF || entry.type == 0xA5 || entry.type == 0xA6 || entry.type == 0xA9 ||
                    entry.type == 0xB7 || entry.type == 0x81 || entry.type == 0x63)
                {
                    valid = false;
                    disklabel = true;
                }

                valid &= entry.lba_start != 0 || entry.lba_sectors != 0 || entry.start_cylinder != 0 || entry.start_head != 0 || entry.start_sector != 0 || entry.end_cylinder != 0 || entry.end_head != 0 || entry.end_sector != 0;
                if(entry.lba_start == 0 && entry.lba_sectors == 0 && valid)
                {
                    entry.lba_start = CHStoLBA(entry.start_cylinder, entry.start_head, entry.start_sector);
                    entry.lba_sectors = CHStoLBA(entry.end_cylinder, entry.end_head, entry.end_sector) - entry.lba_start;
                }

                if(entry.lba_start > imagePlugin.GetSectors() || entry.lba_start + entry.lba_sectors > imagePlugin.GetSectors())
                {
                    valid = false;
                    disklabel = false;
                    extended = false;
                }

                if(disklabel)
                {
                    byte[] disklabel_sector = imagePlugin.ReadSector(entry.lba_start);

                    switch(entry.type)
                    {
                        case 0xA5:
                        case 0xA6:
                        case 0xA9:
                        case 0xB7: // BSD disklabels
                            {
                                BSD.DiskLabel bsdDisklabel = BSD.GetDiskLabel(disklabel_sector);

                                if(bsdDisklabel.d_magic == BSD.DISKMAGIC && bsdDisklabel.d_magic2 == BSD.DISKMAGIC)
                                {
                                    // TODO: Handle disklabels bigger than 1 sector or search max no_parts
                                    foreach(BSD.BSDPartition bsdPartition in bsdDisklabel.d_partitions)
                                    {

                                        CommonTypes.Partition part = new CommonTypes.Partition();

                                        part.Length = bsdPartition.p_size;
                                        part.Start = bsdPartition.p_offset;
                                        part.Size = bsdPartition.p_size * bsdDisklabel.d_secsize;
                                        part.Offset = bsdPartition.p_offset * bsdDisklabel.d_secsize;

                                        part.Type = string.Format("BSD: {0}", bsdPartition.p_fstype);
                                        part.Name = BSD.fsTypeToString(bsdPartition.p_fstype);

                                        part.Sequence = counter;
                                        part.Description = "Partition inside a BSD disklabel.";

                                        if(bsdPartition.p_fstype != BSD.fsType.Unused)
                                        {
                                            partitions.Add(part);
                                            counter++;
                                        }
                                    }
                                }
                                else
                                    valid = true;
                                break;
                            }
                        case 0x63: // UNIX disklabel
                            {
                                uint magic;
                                byte[] unix_dl_sector = imagePlugin.ReadSector(entry.lba_start + 29); // UNIX disklabel starts on sector 29 of partition
                                magic = BitConverter.ToUInt32(unix_dl_sector, 4);

                                if(magic == UNIX.UNIXDiskLabel_MAGIC)
                                {
                                    UNIX.UNIXDiskLabel dl = new UNIX.UNIXDiskLabel();
                                    UNIX.UNIXVTOC vtoc = new UNIX.UNIXVTOC(); // old/new
                                    bool isNewDL = false;
                                    int vtocoffset = 0;

                                    vtoc.magic = BitConverter.ToUInt32(unix_dl_sector, 172);
                                    if(vtoc.magic == UNIX.UNIXVTOC_MAGIC)
                                    {
                                        isNewDL = true;
                                        vtocoffset = 72;
                                    }
                                    else
                                    {
                                        vtoc.magic = BitConverter.ToUInt32(unix_dl_sector, 172);
                                        if(vtoc.magic != UNIX.UNIXDiskLabel_MAGIC)
                                        {
                                            valid = true;
                                            break;
                                        }
                                    }

                                    dl.version = BitConverter.ToUInt32(unix_dl_sector, 8); // 8
                                    byte[] dl_serial = new byte[12];
                                    Array.Copy(unix_dl_sector, 12, dl_serial, 0, 12);
                                    dl.serial = StringHandlers.CToString(dl_serial); // 12
                                    dl.cyls = BitConverter.ToUInt32(unix_dl_sector, 24); // 24
                                    dl.trks = BitConverter.ToUInt32(unix_dl_sector, 28); // 28
                                    dl.secs = BitConverter.ToUInt32(unix_dl_sector, 32); // 32
                                    dl.bps = BitConverter.ToUInt32(unix_dl_sector, 36); // 36
                                    dl.start = BitConverter.ToUInt32(unix_dl_sector, 40); // 40
                                    //dl.unknown1 = br.ReadBytes(48); // 44
                                    dl.alt_tbl = BitConverter.ToUInt32(unix_dl_sector, 92); // 92
                                    dl.alt_len = BitConverter.ToUInt32(unix_dl_sector, 96); // 96

                                    if(isNewDL) // Old version VTOC starts here
                                    {
                                        dl.phys_cyl = BitConverter.ToUInt32(unix_dl_sector, 100); // 100
                                        dl.phys_trk = BitConverter.ToUInt32(unix_dl_sector, 104); // 104
                                        dl.phys_sec = BitConverter.ToUInt32(unix_dl_sector, 108); // 108
                                        dl.phys_bytes = BitConverter.ToUInt32(unix_dl_sector, 112); // 112
                                        dl.unknown2 = BitConverter.ToUInt32(unix_dl_sector, 116); // 116
                                        dl.unknown3 = BitConverter.ToUInt32(unix_dl_sector, 120); // 120
                                        //dl.pad = br.ReadBytes(48); // 124
                                    }

                                    if(vtoc.magic == UNIX.UNIXVTOC_MAGIC)
                                    {
                                        vtoc.version = BitConverter.ToUInt32(unix_dl_sector, 104 + vtocoffset); // 104/176
                                        byte[] vtoc_name = new byte[8];
                                        Array.Copy(unix_dl_sector, 108 + vtocoffset, vtoc_name, 0, 8);
                                        vtoc.name = StringHandlers.CToString(vtoc_name); // 108/180
                                        vtoc.slices = BitConverter.ToUInt16(unix_dl_sector, 116 + vtocoffset); // 116/188
                                        vtoc.unknown = BitConverter.ToUInt16(unix_dl_sector, 118 + vtocoffset); // 118/190
                                                                                                                //vtoc.reserved = br.ReadBytes(40); // 120/192

                                        // TODO: What if number of slices overlaps sector (>23)?
                                        for(int j = 0; j < vtoc.slices; j++)
                                        {
                                            UNIX.UNIXVTOCEntry vtoc_ent = new UNIX.UNIXVTOCEntry();

                                            vtoc_ent.tag = (DiscImageChef.PartPlugins.UNIX.UNIX_TAG)BitConverter.ToUInt16(unix_dl_sector, 160 + vtocoffset + j * 12 + 0); // 160/232 + j*12
                                            vtoc_ent.flags = BitConverter.ToUInt16(unix_dl_sector, 160 + vtocoffset + j * 12 + 2); // 162/234 + j*12
                                            vtoc_ent.start = BitConverter.ToUInt32(unix_dl_sector, 160 + vtocoffset + j * 12 + 6); // 166/238 + j*12
                                            vtoc_ent.length = BitConverter.ToUInt32(unix_dl_sector, 160 + vtocoffset + j * 12 + 10); // 170/242 + j*12

                                            if((vtoc_ent.flags & 0x200) == 0x200 && vtoc_ent.tag != UNIX.UNIX_TAG.EMPTY && vtoc_ent.tag != UNIX.UNIX_TAG.WHOLE)
                                            {
                                                CommonTypes.Partition part = new CommonTypes.Partition();
                                                // TODO: Check if device bps == disklabel bps
                                                part.Start = vtoc_ent.start;
                                                part.Length = vtoc_ent.length;
                                                part.Offset = vtoc_ent.start * dl.bps;
                                                part.Size = vtoc_ent.length * dl.bps;
                                                part.Sequence = counter;
                                                part.Type = string.Format("UNIX: {0}", UNIX.decodeUNIXTAG(vtoc_ent.tag, isNewDL));

                                                string info = "";

                                                if((vtoc_ent.flags & 0x01) == 0x01)
                                                    info += " (do not mount)";
                                                if((vtoc_ent.flags & 0x10) == 0x10)
                                                    info += " (do not mount)";

                                                part.Description = "UNIX slice" + info + ".";

                                                partitions.Add(part);
                                                counter++;
                                            }
                                        }
                                    }
                                }
                                else
                                    valid = true;
                                break;
                            }
                        case 0x82:
                        case 0xBF: // Solaris disklabel
                            {
                                uint magic = BitConverter.ToUInt32(disklabel_sector, 12); // 12
                                uint version = BitConverter.ToUInt32(disklabel_sector, 16); // 16

                                if(magic == 0x600DDEEE && version == 1)
                                {
                                    for(int j = 0; j < 16; j++)
                                    {
                                        CommonTypes.Partition part = new CommonTypes.Partition();
                                        part.Start = BitConverter.ToUInt32(disklabel_sector, 68 + j * 12 + 4);
                                        part.Length = BitConverter.ToUInt32(disklabel_sector, 68 + j * 12 + 8);
                                        part.Offset = part.Start * imagePlugin.GetSectorSize(); // 68+4+j*12
                                        part.Size = part.Length * imagePlugin.GetSectorSize(); // 68+8+j*12
                                        part.Description = "Solaris slice.";

                                        part.Sequence = counter;

                                        if(part.Size > 0)
                                        {
                                            partitions.Add(part);
                                            counter++;
                                        }
                                    }
                                }
                                else
                                    valid = true;
                                break;
                            }
                        case 0x81: // Minix subpartitions
                            {
                                bool minix_subs = false;
                                byte type;

                                for(int j = 0; j < 4; j++)
                                {
                                    type = disklabel_sector[0x1BE + j * 16 + 4];

                                    if(type == 0x81)
                                    {
                                        CommonTypes.Partition part = new CommonTypes.Partition();
                                        minix_subs = true;
                                        part.Description = "Minix subpartition";
                                        part.Type = "Minix";
                                        part.Start = BitConverter.ToUInt32(disklabel_sector, 0x1BE + j * 16 + 8);
                                        part.Length = BitConverter.ToUInt32(disklabel_sector, 0x1BE + j * 16 + 12);
                                        part.Offset = part.Start * imagePlugin.GetSectorSize();
                                        part.Size = part.Length * imagePlugin.GetSectorSize();
                                        part.Sequence = counter;
                                        partitions.Add(part);
                                        counter++;
                                    }
                                }
                                valid |= !minix_subs;

                                break;
                            }
                        default:
                            valid = true;
                            break;
                    }
                }

                if(valid)
                {
                    CommonTypes.Partition part = new CommonTypes.Partition();
                    if(entry.lba_start > 0 && entry.lba_sectors > 0)
                    {
                        part.Start = entry.lba_start;
                        part.Length = entry.lba_sectors;
                        part.Offset = part.Start * imagePlugin.GetSectorSize();
                        part.Size = part.Length * imagePlugin.GetSectorSize();
                    }
                    /*					else if(entry.start_head < 255 && entry.end_head < 255 &&
                                                entry.start_sector > 0 && entry.start_sector < 64 &&
                                                entry.end_sector > 0 && entry.end_sector < 64 &&
                                                entry.start_cylinder < 1024 && entry.end_cylinder < 1024)
                                        {

                                        } */ // As we don't know the maxium cyl, head or sect of the device we need LBA
                    else
                        valid = false;

                    if(valid)
                    {
                        part.Type = string.Format("0x{0:X2}", entry.type);
                        part.Name = decodeMBRType(entry.type);
                        part.Sequence = counter;
                        part.Description = entry.status == 0x80 ? "Partition is bootable." : "";

                        counter++;

                        partitions.Add(part);
                    }
                }

                if(extended) // Let's extend the fun
                {
                    bool ext_valid = true;
                    bool ext_disklabel = false;
                    bool processing_extended = true;

                    sector = imagePlugin.ReadSector(entry.lba_start);

                    while(processing_extended)
                    {
                        for(int l = 0; l < 2; l++)
                        {
                            bool ext_extended = false;

                            MBRPartitionEntry entry2 = new MBRPartitionEntry();

                            entry2.status = sector[0x1BE + 16 * i + 0x00];
                            entry2.start_head = sector[0x1BE + 16 * i + 0x01];

                            cyl_sect1 = sector[0x1BE + 16 * i + 0x02];
                            cyl_sect2 = sector[0x1BE + 16 * i + 0x03];

                            entry2.start_sector = (byte)(cyl_sect1 & 0x3F);
                            entry2.start_cylinder = (ushort)(((cyl_sect1 & 0xC0) << 2) | cyl_sect2);

                            entry2.type = sector[0x1BE + 16 * i + 0x04];
                            entry2.end_head = sector[0x1BE + 16 * i + 0x05];

                            cyl_sect1 = sector[0x1BE + 16 * i + 0x06];
                            cyl_sect2 = sector[0x1BE + 16 * i + 0x07];

                            entry2.end_sector = (byte)(cyl_sect1 & 0x3F);
                            entry2.end_cylinder = (ushort)(((cyl_sect1 & 0xC0) << 2) | cyl_sect2);

                            entry2.lba_start = BitConverter.ToUInt32(sector, 0x1BE + 16 * i + 0x08);
                            entry2.lba_sectors = BitConverter.ToUInt32(sector, 0x1BE + 16 * i + 0x0C);

                            // Let's start the fun...

                            ext_valid &= entry2.status == 0x00 || entry2.status == 0x80;
                            valid &= entry2.type != 0x00;
                            if(entry2.type == 0x82 || entry2.type == 0xBF || entry2.type == 0xA5 || entry2.type == 0xA6 ||
                                entry2.type == 0xA9 || entry2.type == 0xB7 || entry2.type == 0x81 || entry2.type == 0x63)
                            {
                                ext_valid = false;
                                ext_disklabel = true;
                            }
                            if(entry2.type == 0x05 || entry2.type == 0x0F || entry2.type == 0x85)
                            {
                                ext_valid = false;
                                ext_disklabel = false;
                                ext_extended = true; // Extended partition
                            }
                            else
                                processing_extended &= l != 1;

                            if(ext_disklabel)
                            {
                                byte[] disklabel_sector = imagePlugin.ReadSector(entry2.lba_start);

                                switch(entry2.type)
                                {
                                    case 0xA5:
                                    case 0xA6:
                                    case 0xA9:
                                    case 0xB7: // BSD disklabels
                                        {
                                            BSD.DiskLabel bsdDisklabel = BSD.GetDiskLabel(disklabel_sector);

                                            if(bsdDisklabel.d_magic == BSD.DISKMAGIC && bsdDisklabel.d_magic2 == BSD.DISKMAGIC)
                                            {
                                                // TODO: Handle disklabels bigger than 1 sector or search max no_parts
                                                foreach(BSD.BSDPartition bsdPartition in bsdDisklabel.d_partitions)
                                                {

                                                    CommonTypes.Partition part = new CommonTypes.Partition();

                                                    part.Length = bsdPartition.p_size;
                                                    part.Start = bsdPartition.p_offset;
                                                    part.Size = bsdPartition.p_size * bsdDisklabel.d_secsize;
                                                    part.Offset = bsdPartition.p_offset * bsdDisklabel.d_secsize;

                                                    part.Type = string.Format("BSD: {0}", bsdPartition.p_fstype);
                                                    part.Name = BSD.fsTypeToString(bsdPartition.p_fstype);

                                                    part.Sequence = counter;
                                                    part.Description = "Partition inside a BSD disklabel.";

                                                    if(bsdPartition.p_fstype != BSD.fsType.Unused)
                                                    {
                                                        partitions.Add(part);
                                                        counter++;
                                                    }
                                                }
                                            }
                                            else
                                                ext_valid = true;
                                            break;
                                        }
                                    case 0x63: // UNIX disklabel
                                        {
                                            uint magic;
                                            byte[] unix_dl_sector = imagePlugin.ReadSector(entry.lba_start + 29); // UNIX disklabel starts on sector 29 of partition
                                            magic = BitConverter.ToUInt32(unix_dl_sector, 4);

                                            if(magic == UNIX.UNIXDiskLabel_MAGIC)
                                            {
                                                UNIX.UNIXDiskLabel dl = new UNIX.UNIXDiskLabel();
                                                UNIX.UNIXVTOC vtoc = new UNIX.UNIXVTOC(); // old/new
                                                bool isNewDL = false;
                                                int vtocoffset = 0;

                                                vtoc.magic = BitConverter.ToUInt32(unix_dl_sector, 172);
                                                if(vtoc.magic == UNIX.UNIXVTOC_MAGIC)
                                                {
                                                    isNewDL = true;
                                                    vtocoffset = 72;
                                                }
                                                else
                                                {
                                                    vtoc.magic = BitConverter.ToUInt32(unix_dl_sector, 172);
                                                    if(vtoc.magic != UNIX.UNIXDiskLabel_MAGIC)
                                                    {
                                                        valid = true;
                                                        break;
                                                    }
                                                }

                                                dl.version = BitConverter.ToUInt32(unix_dl_sector, 8); // 8
                                                byte[] dl_serial = new byte[12];
                                                Array.Copy(unix_dl_sector, 12, dl_serial, 0, 12);
                                                dl.serial = StringHandlers.CToString(dl_serial); // 12
                                                dl.cyls = BitConverter.ToUInt32(unix_dl_sector, 24); // 24
                                                dl.trks = BitConverter.ToUInt32(unix_dl_sector, 28); // 28
                                                dl.secs = BitConverter.ToUInt32(unix_dl_sector, 32); // 32
                                                dl.bps = BitConverter.ToUInt32(unix_dl_sector, 36); // 36
                                                dl.start = BitConverter.ToUInt32(unix_dl_sector, 40); // 40
                                                //dl.unknown1 = br.ReadBytes(48); // 44
                                                dl.alt_tbl = BitConverter.ToUInt32(unix_dl_sector, 92); // 92
                                                dl.alt_len = BitConverter.ToUInt32(unix_dl_sector, 96); // 96

                                                if(isNewDL) // Old version VTOC starts here
                                                {
                                                    dl.phys_cyl = BitConverter.ToUInt32(unix_dl_sector, 100); // 100
                                                    dl.phys_trk = BitConverter.ToUInt32(unix_dl_sector, 104); // 104
                                                    dl.phys_sec = BitConverter.ToUInt32(unix_dl_sector, 108); // 108
                                                    dl.phys_bytes = BitConverter.ToUInt32(unix_dl_sector, 112); // 112
                                                    dl.unknown2 = BitConverter.ToUInt32(unix_dl_sector, 116); // 116
                                                    dl.unknown3 = BitConverter.ToUInt32(unix_dl_sector, 120); // 120
                                                    //dl.pad = br.ReadBytes(48); // 124
                                                }

                                                if(vtoc.magic == UNIX.UNIXVTOC_MAGIC)
                                                {
                                                    vtoc.version = BitConverter.ToUInt32(unix_dl_sector, 104 + vtocoffset); // 104/176
                                                    byte[] vtoc_name = new byte[8];
                                                    Array.Copy(unix_dl_sector, 108 + vtocoffset, vtoc_name, 0, 8);
                                                    vtoc.name = StringHandlers.CToString(vtoc_name); // 108/180
                                                    vtoc.slices = BitConverter.ToUInt16(unix_dl_sector, 116 + vtocoffset); // 116/188
                                                    vtoc.unknown = BitConverter.ToUInt16(unix_dl_sector, 118 + vtocoffset); // 118/190
                                                    //vtoc.reserved = br.ReadBytes(40); // 120/192

                                                    // TODO: What if number of slices overlaps sector (>23)?
                                                    for(int j = 0; j < vtoc.slices; j++)
                                                    {
                                                        UNIX.UNIXVTOCEntry vtoc_ent = new UNIX.UNIXVTOCEntry();

                                                        vtoc_ent.tag = (DiscImageChef.PartPlugins.UNIX.UNIX_TAG)BitConverter.ToUInt16(unix_dl_sector, 160 + vtocoffset + j * 12 + 0); // 160/232 + j*12
                                                        vtoc_ent.flags = BitConverter.ToUInt16(unix_dl_sector, 160 + vtocoffset + j * 12 + 2); // 162/234 + j*12
                                                        vtoc_ent.start = BitConverter.ToUInt32(unix_dl_sector, 160 + vtocoffset + j * 12 + 6); // 166/238 + j*12
                                                        vtoc_ent.length = BitConverter.ToUInt32(unix_dl_sector, 160 + vtocoffset + j * 12 + 10); // 170/242 + j*12

                                                        if((vtoc_ent.flags & 0x200) == 0x200 && vtoc_ent.tag != UNIX.UNIX_TAG.EMPTY && vtoc_ent.tag != UNIX.UNIX_TAG.WHOLE)
                                                        {
                                                            CommonTypes.Partition part = new CommonTypes.Partition();
                                                            // TODO: Check if device bps == disklabel bps
                                                            part.Start = vtoc_ent.start;
                                                            part.Length = vtoc_ent.length;
                                                            part.Offset = vtoc_ent.start * dl.bps;
                                                            part.Size = vtoc_ent.length * dl.bps;
                                                            part.Sequence = counter;
                                                            part.Type = string.Format("UNIX: {0}", UNIX.decodeUNIXTAG(vtoc_ent.tag, isNewDL));

                                                            string info = "";

                                                            if((vtoc_ent.flags & 0x01) == 0x01)
                                                                info += " (do not mount)";
                                                            if((vtoc_ent.flags & 0x10) == 0x10)
                                                                info += " (do not mount)";

                                                            part.Description = "UNIX slice" + info + ".";

                                                            partitions.Add(part);
                                                            counter++;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                                ext_valid = true;
                                            break;
                                        }
                                    case 0x82:
                                    case 0xBF: // Solaris disklabel
                                        {
                                            uint magic = BitConverter.ToUInt32(disklabel_sector, 12); // 12
                                            uint version = BitConverter.ToUInt32(disklabel_sector, 16); // 16

                                            if(magic == 0x600DDEEE && version == 1)
                                            {
                                                for(int j = 0; j < 16; j++)
                                                {
                                                    CommonTypes.Partition part = new CommonTypes.Partition();
                                                    part.Start = BitConverter.ToUInt32(disklabel_sector, 68 + j * 12 + 4);
                                                    part.Length = BitConverter.ToUInt32(disklabel_sector, 68 + j * 12 + 8);
                                                    part.Offset = part.Start * imagePlugin.GetSectorSize(); // 68+4+j*12
                                                    part.Size = part.Length * imagePlugin.GetSectorSize(); // 68+8+j*12
                                                    part.Description = "Solaris slice.";

                                                    part.Sequence = counter;

                                                    if(part.Size > 0)
                                                    {
                                                        partitions.Add(part);
                                                        counter++;
                                                    }
                                                }
                                            }
                                            else
                                                ext_valid = true;
                                            break;
                                        }
                                    case 0x81: // Minix subpartitions
                                        {
                                            bool minix_subs = false;
                                            byte type;

                                            for(int j = 0; j < 4; j++)
                                            {
                                                type = disklabel_sector[0x1BE + j * 16 + 4];

                                                if(type == 0x81)
                                                {
                                                    CommonTypes.Partition part = new CommonTypes.Partition();
                                                    minix_subs = true;
                                                    part.Description = "Minix subpartition";
                                                    part.Type = "Minix";
                                                    part.Start = BitConverter.ToUInt32(disklabel_sector, 0x1BE + j * 16 + 8);
                                                    part.Length = BitConverter.ToUInt32(disklabel_sector, 0x1BE + j * 16 + 12);
                                                    part.Offset = part.Start * imagePlugin.GetSectorSize();
                                                    part.Size = part.Length * imagePlugin.GetSectorSize();
                                                    part.Sequence = counter;
                                                    partitions.Add(part);
                                                    counter++;
                                                }
                                            }
                                            ext_valid |= !minix_subs;

                                            break;
                                        }
                                    default:
                                        ext_valid = true;
                                        break;
                                }

                            }

                            if(ext_valid)
                            {
                                CommonTypes.Partition part = new CommonTypes.Partition();
                                if(entry2.lba_start > 0 && entry2.lba_sectors > 0)
                                {
                                    part.Start = entry2.lba_start;
                                    part.Length = entry2.lba_sectors;
                                    part.Offset = part.Start * imagePlugin.GetSectorSize();
                                    part.Size = part.Length * imagePlugin.GetSectorSize();
                                }
                                /*					else if(entry2.start_head < 255 && entry2.end_head < 255 &&
                                                            entry2.start_sector > 0 && entry2.start_sector < 64 &&
                                                            entry2.end_sector > 0 && entry2.end_sector < 64 &&
                                                            entry2.start_cylinder < 1024 && entry2.end_cylinder < 1024)
                                                    {

                                                    } */ // As we don't know the maxium cyl, head or sect of the device we need LBA
                                else
                                    ext_valid = false;

                                if(ext_valid)
                                {
                                    part.Type = string.Format("0x{0:X2}", entry2.type);
                                    part.Name = decodeMBRType(entry2.type);
                                    part.Sequence = counter;
                                    part.Description = entry2.status == 0x80 ? "Partition is bootable." : "";

                                    counter++;

                                    partitions.Add(part);
                                }
                            }

                            if(ext_extended)
                            {
                                break;
                            }
                        }
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

        static string decodeMBRType(byte type)
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

        public struct MBRPartitionEntry
        {
            public byte status;
            // Partition status, 0x80 or 0x00, else invalid
            public byte start_head;
            // Starting head [0,254]
            public byte start_sector;
            // Starting sector [1,63]
            public ushort start_cylinder;
            // Starting cylinder [0,1023]
            public byte type;
            // Partition type
            public byte end_head;
            // Ending head [0,254]
            public byte end_sector;
            // Ending sector [1,63]
            public ushort end_cylinder;
            // Ending cylinder [0,1023]
            public uint lba_start;
            // Starting absolute sector
            public uint lba_sectors;
            // Total sectors
        }

    }
}