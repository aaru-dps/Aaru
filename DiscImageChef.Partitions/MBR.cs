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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using DiscImageChef;

// TODO: Support AAP, AST, SpeedStor and Ontrack extensions
namespace DiscImageChef.PartPlugins
{
    class MBR : PartPlugin
    {
        const UInt16 MBRSignature = 0xAA55;

        public MBR()
        {
            Name = "Master Boot Record";
            PluginUUID = new Guid("5E8A34E8-4F1A-59E6-4BF7-7EA647063A76");
        }

        public override bool GetInformation(ImagePlugins.ImagePlugin imagePlugin, out List<CommonTypes.Partition> partitions)
        {
            byte cyl_sect1, cyl_sect2; // For decoding cylinder and sector
            UInt16 signature;
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
                                UInt32 magic = BitConverter.ToUInt32(disklabel_sector, 0);

                                if(magic == 0x82564557)
                                {
                                    UInt16 no_parts = BitConverter.ToUInt16(disklabel_sector, 126);

                                    // TODO: Handle disklabels bigger than 1 sector or search max no_parts
                                    for(int j = 0; j < no_parts; j++)
                                    {
                                        CommonTypes.Partition part = new CommonTypes.Partition();
                                        byte bsd_type;

                                        part.PartitionSectors = BitConverter.ToUInt32(disklabel_sector, 134 + j * 16 + 4);
                                        part.PartitionStartSector = BitConverter.ToUInt32(disklabel_sector, 134 + j * 16 + 0);
                                        part.PartitionLength = part.PartitionSectors * imagePlugin.GetSectorSize();
                                        part.PartitionStart = part.PartitionStartSector * imagePlugin.GetSectorSize();
                                        bsd_type = disklabel_sector[134 + j * 16 + 8];

                                        part.PartitionType = String.Format("BSD: {0}", bsd_type);
                                        part.PartitionName = decodeBSDType(bsd_type);

                                        part.PartitionSequence = counter;
                                        part.PartitionDescription = "Partition inside a BSD disklabel.";

                                        if(bsd_type != 0)
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
                                UInt32 magic;
                                byte[] unix_dl_sector = imagePlugin.ReadSector(entry.lba_start + 29); // UNIX disklabel starts on sector 29 of partition
                                magic = BitConverter.ToUInt32(unix_dl_sector, 4);

                                if(magic == UNIXDiskLabel_MAGIC)
                                {
                                    UNIXDiskLabel dl = new UNIXDiskLabel();
                                    UNIXVTOC vtoc = new UNIXVTOC(); // old/new
                                    bool isNewDL = false;
                                    int vtocoffset = 0;

                                    vtoc.magic = BitConverter.ToUInt32(unix_dl_sector, 172);
                                    if(vtoc.magic == UNIXVTOC_MAGIC)
                                    {
                                        isNewDL = true;
                                        vtocoffset = 72;
                                    }
                                    else
                                    {
                                        vtoc.magic = BitConverter.ToUInt32(unix_dl_sector, 172);
                                        if(vtoc.magic != UNIXDiskLabel_MAGIC)
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

                                    if(vtoc.magic == UNIXVTOC_MAGIC)
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
                                            UNIXVTOCEntry vtoc_ent = new UNIXVTOCEntry();

                                            vtoc_ent.tag = BitConverter.ToUInt16(unix_dl_sector, 160 + vtocoffset + j * 12 + 0); // 160/232 + j*12
                                            vtoc_ent.flags = BitConverter.ToUInt16(unix_dl_sector, 160 + vtocoffset + j * 12 + 2); // 162/234 + j*12
                                            vtoc_ent.start = BitConverter.ToUInt32(unix_dl_sector, 160 + vtocoffset + j * 12 + 6); // 166/238 + j*12
                                            vtoc_ent.length = BitConverter.ToUInt32(unix_dl_sector, 160 + vtocoffset + j * 12 + 10); // 170/242 + j*12

                                            if((vtoc_ent.flags & 0x200) == 0x200 && vtoc_ent.tag != UNIX_TAG_EMPTY && vtoc_ent.tag != UNIX_TAG_WHOLE)
                                            {
                                                CommonTypes.Partition part = new CommonTypes.Partition();
                                                // TODO: Check if device bps == disklabel bps
                                                part.PartitionStartSector = vtoc_ent.start;
                                                part.PartitionSectors = vtoc_ent.length;
                                                part.PartitionStart = vtoc_ent.start * dl.bps;
                                                part.PartitionLength = vtoc_ent.length * dl.bps;
                                                part.PartitionSequence = counter;
                                                part.PartitionType = String.Format("UNIX: {0}", decodeUNIXTAG(vtoc_ent.tag, isNewDL));

                                                string info = "";

                                                if((vtoc_ent.flags & 0x01) == 0x01)
                                                    info += " (do not mount)";
                                                if((vtoc_ent.flags & 0x10) == 0x10)
                                                    info += " (do not mount)";

                                                part.PartitionDescription = "UNIX slice" + info + ".";

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
                                UInt32 magic = BitConverter.ToUInt32(disklabel_sector, 12); // 12
                                UInt32 version = BitConverter.ToUInt32(disklabel_sector, 16); // 16

                                if(magic == 0x600DDEEE && version == 1)
                                {
                                    for(int j = 0; j < 16; j++)
                                    {
                                        CommonTypes.Partition part = new CommonTypes.Partition();
                                        part.PartitionStartSector = BitConverter.ToUInt32(disklabel_sector, 68 + j * 12 + 4);
                                        part.PartitionSectors = BitConverter.ToUInt32(disklabel_sector, 68 + j * 12 + 8);
                                        part.PartitionStart = part.PartitionStartSector * imagePlugin.GetSectorSize(); // 68+4+j*12
                                        part.PartitionLength = part.PartitionSectors * imagePlugin.GetSectorSize(); // 68+8+j*12
                                        part.PartitionDescription = "Solaris slice.";

                                        part.PartitionSequence = counter;

                                        if(part.PartitionLength > 0)
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
                                        part.PartitionDescription = "Minix subpartition";
                                        part.PartitionType = "Minix";
                                        part.PartitionStartSector = BitConverter.ToUInt32(disklabel_sector, 0x1BE + j * 16 + 8);
                                        part.PartitionSectors = BitConverter.ToUInt32(disklabel_sector, 0x1BE + j * 16 + 12);
                                        part.PartitionStart = part.PartitionStartSector * imagePlugin.GetSectorSize();
                                        part.PartitionLength = part.PartitionSectors * imagePlugin.GetSectorSize();
                                        part.PartitionSequence = counter;
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
                        part.PartitionStartSector = entry.lba_start;
                        part.PartitionSectors = entry.lba_sectors;
                        part.PartitionStart = part.PartitionStartSector * imagePlugin.GetSectorSize();
                        part.PartitionLength = part.PartitionSectors * imagePlugin.GetSectorSize();
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
                        part.PartitionType = String.Format("0x{0:X2}", entry.type);
                        part.PartitionName = decodeMBRType(entry.type);
                        part.PartitionSequence = counter;
                        part.PartitionDescription = entry.status == 0x80 ? "Partition is bootable." : "";

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
                                            UInt32 magic = BitConverter.ToUInt32(disklabel_sector, 0);

                                            if(magic == 0x82564557)
                                            {
                                                UInt16 no_parts = BitConverter.ToUInt16(disklabel_sector, 126);

                                                // TODO: Handle disklabels bigger than 1 sector or search max no_parts
                                                for(int j = 0; j < no_parts; j++)
                                                {
                                                    CommonTypes.Partition part = new CommonTypes.Partition();
                                                    byte bsd_type;

                                                    part.PartitionSectors = BitConverter.ToUInt32(disklabel_sector, 134 + j * 16 + 4);
                                                    part.PartitionStartSector = BitConverter.ToUInt32(disklabel_sector, 134 + j * 16 + 0);
                                                    part.PartitionLength = part.PartitionSectors * imagePlugin.GetSectorSize();
                                                    part.PartitionStart = part.PartitionStartSector * imagePlugin.GetSectorSize();
                                                    bsd_type = disklabel_sector[134 + j * 16 + 8];

                                                    part.PartitionType = String.Format("BSD: {0}", bsd_type);
                                                    part.PartitionName = decodeBSDType(bsd_type);

                                                    part.PartitionSequence = counter;
                                                    part.PartitionDescription = "Partition inside a BSD disklabel.";

                                                    if(bsd_type != 0)
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
                                            UInt32 magic;
                                            byte[] unix_dl_sector = imagePlugin.ReadSector(entry.lba_start + 29); // UNIX disklabel starts on sector 29 of partition
                                            magic = BitConverter.ToUInt32(unix_dl_sector, 4);

                                            if(magic == UNIXDiskLabel_MAGIC)
                                            {
                                                UNIXDiskLabel dl = new UNIXDiskLabel();
                                                UNIXVTOC vtoc = new UNIXVTOC(); // old/new
                                                bool isNewDL = false;
                                                int vtocoffset = 0;

                                                vtoc.magic = BitConverter.ToUInt32(unix_dl_sector, 172);
                                                if(vtoc.magic == UNIXVTOC_MAGIC)
                                                {
                                                    isNewDL = true;
                                                    vtocoffset = 72;
                                                }
                                                else
                                                {
                                                    vtoc.magic = BitConverter.ToUInt32(unix_dl_sector, 172);
                                                    if(vtoc.magic != UNIXDiskLabel_MAGIC)
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

                                                if(vtoc.magic == UNIXVTOC_MAGIC)
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
                                                        UNIXVTOCEntry vtoc_ent = new UNIXVTOCEntry();

                                                        vtoc_ent.tag = BitConverter.ToUInt16(unix_dl_sector, 160 + vtocoffset + j * 12 + 0); // 160/232 + j*12
                                                        vtoc_ent.flags = BitConverter.ToUInt16(unix_dl_sector, 160 + vtocoffset + j * 12 + 2); // 162/234 + j*12
                                                        vtoc_ent.start = BitConverter.ToUInt32(unix_dl_sector, 160 + vtocoffset + j * 12 + 6); // 166/238 + j*12
                                                        vtoc_ent.length = BitConverter.ToUInt32(unix_dl_sector, 160 + vtocoffset + j * 12 + 10); // 170/242 + j*12

                                                        if((vtoc_ent.flags & 0x200) == 0x200 && vtoc_ent.tag != UNIX_TAG_EMPTY && vtoc_ent.tag != UNIX_TAG_WHOLE)
                                                        {
                                                            CommonTypes.Partition part = new CommonTypes.Partition();
                                                            // TODO: Check if device bps == disklabel bps
                                                            part.PartitionStartSector = vtoc_ent.start;
                                                            part.PartitionSectors = vtoc_ent.length;
                                                            part.PartitionStart = vtoc_ent.start * dl.bps;
                                                            part.PartitionLength = vtoc_ent.length * dl.bps;
                                                            part.PartitionSequence = counter;
                                                            part.PartitionType = String.Format("UNIX: {0}", decodeUNIXTAG(vtoc_ent.tag, isNewDL));

                                                            string info = "";

                                                            if((vtoc_ent.flags & 0x01) == 0x01)
                                                                info += " (do not mount)";
                                                            if((vtoc_ent.flags & 0x10) == 0x10)
                                                                info += " (do not mount)";

                                                            part.PartitionDescription = "UNIX slice" + info + ".";

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
                                            UInt32 magic = BitConverter.ToUInt32(disklabel_sector, 12); // 12
                                            UInt32 version = BitConverter.ToUInt32(disklabel_sector, 16); // 16

                                            if(magic == 0x600DDEEE && version == 1)
                                            {
                                                for(int j = 0; j < 16; j++)
                                                {
                                                    CommonTypes.Partition part = new CommonTypes.Partition();
                                                    part.PartitionStartSector = BitConverter.ToUInt32(disklabel_sector, 68 + j * 12 + 4);
                                                    part.PartitionSectors = BitConverter.ToUInt32(disklabel_sector, 68 + j * 12 + 8);
                                                    part.PartitionStart = part.PartitionStartSector * imagePlugin.GetSectorSize(); // 68+4+j*12
                                                    part.PartitionLength = part.PartitionSectors * imagePlugin.GetSectorSize(); // 68+8+j*12
                                                    part.PartitionDescription = "Solaris slice.";

                                                    part.PartitionSequence = counter;

                                                    if(part.PartitionLength > 0)
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
                                                    part.PartitionDescription = "Minix subpartition";
                                                    part.PartitionType = "Minix";
                                                    part.PartitionStartSector = BitConverter.ToUInt32(disklabel_sector, 0x1BE + j * 16 + 8);
                                                    part.PartitionSectors = BitConverter.ToUInt32(disklabel_sector, 0x1BE + j * 16 + 12);
                                                    part.PartitionStart = part.PartitionStartSector * imagePlugin.GetSectorSize();
                                                    part.PartitionLength = part.PartitionSectors * imagePlugin.GetSectorSize();
                                                    part.PartitionSequence = counter;
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
                                    part.PartitionStartSector = entry2.lba_start;
                                    part.PartitionSectors = entry2.lba_sectors;
                                    part.PartitionStart = part.PartitionStartSector * imagePlugin.GetSectorSize();
                                    part.PartitionLength = part.PartitionSectors * imagePlugin.GetSectorSize();
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
                                    part.PartitionType = String.Format("0x{0:X2}", entry2.type);
                                    part.PartitionName = decodeMBRType(entry2.type);
                                    part.PartitionSequence = counter;
                                    part.PartitionDescription = entry2.status == 0x80 ? "Partition is bootable." : "";

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

        static UInt32 CHStoLBA(ushort cyl, byte head, byte sector)
        {
            return (((UInt32)cyl * 16) + (UInt32)head) * 63 + (UInt32)sector - 1;
        }

        static string decodeBSDType(byte type)
        {
            switch(type)
            {
                case 1:
                    return "Swap";
                case 2:
                    return "UNIX Version 6";
                case 3:
                    return "UNIX Version 7";
                case 4:
                    return "System V";
                case 5:
                    return "4.1BSD";
                case 6:
                    return "UNIX Eigth Edition";
                case 7:
                    return "4.2BSD";
                case 8:
                    return "MS-DOS";
                case 9:
                    return "4.4LFS";
                case 11:
                    return "HPFS";
                case 12:
                    return "ISO9660";
                case 13:
                    return "Boot";
                case 14:
                    return "Amiga FFS";
                case 15:
                    return "Apple HFS";
                default:
                    return "Unknown";
            }
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
            public UInt16 start_cylinder;
            // Starting cylinder [0,1023]
            public byte type;
            // Partition type
            public byte end_head;
            // Ending head [0,254]
            public byte end_sector;
            // Ending sector [1,63]
            public UInt16 end_cylinder;
            // Ending cylinder [0,1023]
            public UInt32 lba_start;
            // Starting absolute sector
            public UInt32 lba_sectors;
            // Total sectors
        }

        const UInt32 UNIXDiskLabel_MAGIC = 0xCA5E600D;
        const UInt32 UNIXVTOC_MAGIC = 0x600DDEEE;
        // Same as Solaris VTOC
        struct UNIXDiskLabel
        {
            public UInt32 type;
            // Drive type, seems always 0
            public UInt32 magic;
            // UNIXDiskLabel_MAGIC
            public UInt32 version;
            // Only seen 1
            public string serial;
            // 12 bytes, serial number of the device
            public UInt32 cyls;
            // data cylinders per device
            public UInt32 trks;
            // data tracks per cylinder
            public UInt32 secs;
            // data sectors per track
            public UInt32 bps;
            // data bytes per sector
            public UInt32 start;
            // first sector of this partition
            public byte[] unknown1;
            // 48 bytes
            public UInt32 alt_tbl;
            // byte offset of alternate table
            public UInt32 alt_len;
            // byte length of alternate table
            // From onward here, is not on old version
            public UInt32 phys_cyl;
            // physical cylinders per device
            public UInt32 phys_trk;
            // physical tracks per cylinder
            public UInt32 phys_sec;
            // physical sectors per track
            public UInt32 phys_bytes;
            // physical bytes per sector
            public UInt32 unknown2;
            //
            public UInt32 unknown3;
            //
            public byte[] pad;
            // 32bytes
        }

        struct UNIXVTOC
        {
            public UInt32 magic;
            // UNIXVTOC_MAGIC
            public UInt32 version;
            // 1
            public string name;
            // 8 bytes
            public UInt16 slices;
            // # of slices
            public UInt16 unknown;
            //
            public byte[] reserved;
            // 40 bytes
        }

        struct UNIXVTOCEntry
        {
            public UInt16 tag;
            // TAG
            public UInt16 flags;
            // Flags (see below)
            public UInt32 start;
            // Start sector
            public UInt32 length;
            // Length of slice in sectors
        }

        const UInt16 UNIX_TAG_EMPTY = 0x0000;
        // empty
        const UInt16 UNIX_TAG_BOOT = 0x0001;
        // boot
        const UInt16 UNIX_TAG_ROOT = 0x0002;
        // root
        const UInt16 UNIX_TAG_SWAP = 0x0003;
        // swap
        const UInt16 UNIX_TAG_USER = 0x0004;
        // /usr
        const UInt16 UNIX_TAG_WHOLE = 0x0005;
        // whole disk
        const UInt16 UNIX_TAG_STAND = 0x0006;
        // stand partition ??
        const UInt16 UNIX_TAG_ALT_S = 0x0006;
        // alternate sector space
        const UInt16 UNIX_TAG_VAR = 0x0007;
        // /var
        const UInt16 UNIX_TAG_OTHER = 0x0007;
        // non UNIX
        const UInt16 UNIX_TAG_HOME = 0x0008;
        // /home
        const UInt16 UNIX_TAG_ALT_T = 0x0008;
        // alternate track space
        const UInt16 UNIX_TAG_ALT_ST = 0x0009;
        // alternate sector track
        const UInt16 UNIX_TAG_NEW_STAND = 0x0009;
        // stand partition ??
        const UInt16 UNIX_TAG_CACHE = 0x000A;
        // cache
        const UInt16 UNIX_TAG_NEW_VAR = 0x000A;
        // /var
        const UInt16 UNIX_TAG_RESERVED = 0x000B;
        // reserved
        const UInt16 UNIX_TAG_NEW_HOME = 0x000B;
        // /home
        const UInt16 UNIX_TAG_DUMP = 0x000C;
        // dump partition
        const UInt16 UNIX_TAG_NEW_ALT_ST = 0x000D;
        // alternate sector track
        const UInt16 UNIX_TAG_VM_PUBLIC = 0x000E;
        // volume mgt public partition
        const UInt16 UNIX_TAG_VM_PRIVATE = 0x000F;
        // volume mgt private partition
        static string decodeUNIXTAG(UInt16 type, bool isNew)
        {
            switch(type)
            {
                case UNIX_TAG_EMPTY:
                    return "Unused";
                case UNIX_TAG_BOOT:
                    return "Boot";
                case UNIX_TAG_ROOT:
                    return "/";
                case UNIX_TAG_SWAP:
                    return "Swap";
                case UNIX_TAG_USER:
                    return "/usr";
                case UNIX_TAG_WHOLE:
                    return "Whole disk";
                case UNIX_TAG_STAND:
                    return isNew ? "Stand" : "Alternate sector space";
                case UNIX_TAG_VAR:
                    return isNew ? "/var" : "non UNIX";
                case UNIX_TAG_HOME:
                    return isNew ? "/home" : "Alternate track space";
                case UNIX_TAG_ALT_ST:
                    return isNew ? "Alternate sector track" : "Stand";
                case UNIX_TAG_CACHE:
                    return isNew ? "Cache" : "/var";
                case UNIX_TAG_RESERVED:
                    return isNew ? "Reserved" : "/home";
                case UNIX_TAG_DUMP:
                    return "dump";
                case UNIX_TAG_NEW_ALT_ST:
                    return "Alternate sector track";
                case UNIX_TAG_VM_PUBLIC:
                    return "volume mgt public partition";
                case UNIX_TAG_VM_PRIVATE:
                    return "volume mgt private partition";
                default:
                    return String.Format("Unknown TAG: 0x{0:X4}", type);
            }
        }
    }
}