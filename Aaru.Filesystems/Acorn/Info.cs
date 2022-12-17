// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Acorn filesystem plugin.
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
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of Acorn's Advanced Data Filing System (ADFS)</summary>
public sealed partial class AcornADFS
{
    // TODO: BBC Master hard disks are untested...
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(partition.Start >= partition.End)
            return false;

        ulong sbSector;
        uint  sectorsToRead;

        if(imagePlugin.Info.SectorSize < 256)
            return false;

        byte[]      sector;
        ErrorNumber errno;

        // ADFS-S, ADFS-M, ADFS-L, ADFS-D without partitions
        if(partition.Start == 0)
        {
            errno = imagePlugin.ReadSector(0, out sector);

            if(errno != ErrorNumber.NoError)
                return false;

            byte          oldChk0 = AcornMapChecksum(sector, 255);
            OldMapSector0 oldMap0 = Marshal.ByteArrayToStructureLittleEndian<OldMapSector0>(sector);

            errno = imagePlugin.ReadSector(1, out sector);

            if(errno != ErrorNumber.NoError)
                return false;

            byte          oldChk1 = AcornMapChecksum(sector, 255);
            OldMapSector1 oldMap1 = Marshal.ByteArrayToStructureLittleEndian<OldMapSector1>(sector);

            AaruConsole.DebugWriteLine("ADFS Plugin", "oldMap0.checksum = {0}", oldMap0.checksum);
            AaruConsole.DebugWriteLine("ADFS Plugin", "oldChk0 = {0}", oldChk0);

            // According to documentation map1 MUST start on sector 1. On ADFS-D it starts at 0x100, not on sector 1 (0x400)
            if(oldMap0.checksum == oldChk0 &&
               oldMap1.checksum != oldChk1 &&
               sector.Length    >= 512)
            {
                errno = imagePlugin.ReadSector(0, out sector);

                if(errno != ErrorNumber.NoError)
                    return false;

                byte[] tmp = new byte[256];
                Array.Copy(sector, 256, tmp, 0, 256);
                oldChk1 = AcornMapChecksum(tmp, 255);
                oldMap1 = Marshal.ByteArrayToStructureLittleEndian<OldMapSector1>(tmp);
            }

            AaruConsole.DebugWriteLine("ADFS Plugin", "oldMap1.checksum = {0}", oldMap1.checksum);
            AaruConsole.DebugWriteLine("ADFS Plugin", "oldChk1 = {0}", oldChk1);

            if(oldMap0.checksum == oldChk0 &&
               oldMap1.checksum == oldChk1 &&
               oldMap0.checksum != 0       &&
               oldMap1.checksum != 0)
            {
                sbSector      = OLD_DIRECTORY_LOCATION / imagePlugin.Info.SectorSize;
                sectorsToRead = OLD_DIRECTORY_SIZE     / imagePlugin.Info.SectorSize;

                if(OLD_DIRECTORY_SIZE % imagePlugin.Info.SectorSize > 0)
                    sectorsToRead++;

                errno = imagePlugin.ReadSectors(sbSector, sectorsToRead, out sector);

                if(errno != ErrorNumber.NoError)
                    return false;

                if(sector.Length > OLD_DIRECTORY_SIZE)
                {
                    byte[] tmp = new byte[OLD_DIRECTORY_SIZE];
                    Array.Copy(sector, 0, tmp, 0, OLD_DIRECTORY_SIZE - 53);
                    Array.Copy(sector, sector.Length                 - 54, tmp, OLD_DIRECTORY_SIZE - 54, 53);
                    sector = tmp;
                }

                OldDirectory oldRoot = Marshal.ByteArrayToStructureLittleEndian<OldDirectory>(sector);
                byte         dirChk  = AcornDirectoryChecksum(sector, (int)OLD_DIRECTORY_SIZE - 1);

                AaruConsole.DebugWriteLine("ADFS Plugin", "oldRoot.header.magic at 0x200 = {0}", oldRoot.header.magic);

                AaruConsole.DebugWriteLine("ADFS Plugin", "oldRoot.tail.magic at 0x200 = {0}", oldRoot.tail.magic);

                AaruConsole.DebugWriteLine("ADFS Plugin", "oldRoot.tail.checkByte at 0x200 = {0}",
                                           oldRoot.tail.checkByte);

                AaruConsole.DebugWriteLine("ADFS Plugin", "dirChk at 0x200 = {0}", dirChk);

                if((oldRoot.header.magic == OLD_DIR_MAGIC && oldRoot.tail.magic == OLD_DIR_MAGIC) ||
                   (oldRoot.header.magic == NEW_DIR_MAGIC && oldRoot.tail.magic == NEW_DIR_MAGIC))
                    return true;

                // RISC OS says the old directory can't be in the new location, hard disks created by RISC OS 3.10 do that...
                sbSector      = NEW_DIRECTORY_LOCATION / imagePlugin.Info.SectorSize;
                sectorsToRead = NEW_DIRECTORY_SIZE     / imagePlugin.Info.SectorSize;

                if(NEW_DIRECTORY_SIZE % imagePlugin.Info.SectorSize > 0)
                    sectorsToRead++;

                errno = imagePlugin.ReadSectors(sbSector, sectorsToRead, out sector);

                if(errno != ErrorNumber.NoError)
                    return false;

                if(sector.Length > OLD_DIRECTORY_SIZE)
                {
                    byte[] tmp = new byte[OLD_DIRECTORY_SIZE];
                    Array.Copy(sector, 0, tmp, 0, OLD_DIRECTORY_SIZE - 53);
                    Array.Copy(sector, sector.Length                 - 54, tmp, OLD_DIRECTORY_SIZE - 54, 53);
                    sector = tmp;
                }

                oldRoot = Marshal.ByteArrayToStructureLittleEndian<OldDirectory>(sector);
                dirChk  = AcornDirectoryChecksum(sector, (int)OLD_DIRECTORY_SIZE - 1);

                AaruConsole.DebugWriteLine("ADFS Plugin", "oldRoot.header.magic at 0x400 = {0}", oldRoot.header.magic);

                AaruConsole.DebugWriteLine("ADFS Plugin", "oldRoot.tail.magic at 0x400 = {0}", oldRoot.tail.magic);

                AaruConsole.DebugWriteLine("ADFS Plugin", "oldRoot.tail.checkByte at 0x400 = {0}",
                                           oldRoot.tail.checkByte);

                AaruConsole.DebugWriteLine("ADFS Plugin", "dirChk at 0x400 = {0}", dirChk);

                if((oldRoot.header.magic == OLD_DIR_MAGIC && oldRoot.tail.magic == OLD_DIR_MAGIC) ||
                   (oldRoot.header.magic == NEW_DIR_MAGIC && oldRoot.tail.magic == NEW_DIR_MAGIC))
                    return true;
            }
        }

        // Partitioning or not, new formats follow:
        DiscRecord drSb;

        errno = imagePlugin.ReadSector(partition.Start, out sector);

        if(errno != ErrorNumber.NoError)
            return false;

        byte newChk = NewMapChecksum(sector);
        AaruConsole.DebugWriteLine("ADFS Plugin", "newChk = {0}", newChk);
        AaruConsole.DebugWriteLine("ADFS Plugin", "map.zoneChecksum = {0}", sector[0]);

        sbSector      = BOOT_BLOCK_LOCATION / imagePlugin.Info.SectorSize;
        sectorsToRead = BOOT_BLOCK_SIZE     / imagePlugin.Info.SectorSize;

        if(BOOT_BLOCK_SIZE % imagePlugin.Info.SectorSize > 0)
            sectorsToRead++;

        if(sbSector + partition.Start + sectorsToRead >= partition.End)
            return false;

        errno = imagePlugin.ReadSectors(sbSector + partition.Start, sectorsToRead, out byte[] bootSector);

        if(errno != ErrorNumber.NoError)
            return false;

        int bootChk = 0;

        if(bootSector.Length < 512)
            return false;

        for(int i = 0; i < 0x1FF; i++)
            bootChk = (bootChk & 0xFF) + (bootChk >> 8) + bootSector[i];

        AaruConsole.DebugWriteLine("ADFS Plugin", "bootChk = {0}", bootChk);
        AaruConsole.DebugWriteLine("ADFS Plugin", "bBlock.checksum = {0}", bootSector[0x1FF]);

        if(newChk == sector[0] &&
           newChk != 0)
        {
            NewMap nmap = Marshal.ByteArrayToStructureLittleEndian<NewMap>(sector);
            drSb = nmap.discRecord;
        }
        else if(bootChk == bootSector[0x1FF])
        {
            BootBlock bBlock = Marshal.ByteArrayToStructureLittleEndian<BootBlock>(bootSector);
            drSb = bBlock.discRecord;
        }
        else
            return false;

        AaruConsole.DebugWriteLine("ADFS Plugin", "drSb.log2secsize = {0}", drSb.log2secsize);
        AaruConsole.DebugWriteLine("ADFS Plugin", "drSb.idlen = {0}", drSb.idlen);
        AaruConsole.DebugWriteLine("ADFS Plugin", "drSb.disc_size_high = {0}", drSb.disc_size_high);
        AaruConsole.DebugWriteLine("ADFS Plugin", "drSb.disc_size = {0}", drSb.disc_size);

        AaruConsole.DebugWriteLine("ADFS Plugin", "IsNullOrEmpty(drSb.reserved) = {0}",
                                   ArrayHelpers.ArrayIsNullOrEmpty(drSb.reserved));

        if(drSb.log2secsize is < 8 or > 10)
            return false;

        if(drSb.idlen < drSb.log2secsize + 3 ||
           drSb.idlen > 19)
            return false;

        if(drSb.disc_size_high >> drSb.log2secsize != 0)
            return false;

        if(!ArrayHelpers.ArrayIsNullOrEmpty(drSb.reserved))
            return false;

        ulong bytes = drSb.disc_size_high;
        bytes *= 0x100000000;
        bytes += drSb.disc_size;

        return bytes <= imagePlugin.Info.Sectors * imagePlugin.Info.SectorSize;
    }

    // TODO: Find root directory on volumes with DiscRecord
    // TODO: Support big directories (ADFS-G?)
    // TODO: Find the real freemap on volumes with DiscRecord, as DiscRecord's discid may be empty but this one isn't
    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        Encoding = encoding ?? Encoding.GetEncoding("iso-8859-1");
        var sbInformation = new StringBuilder();
        metadata    = new FileSystem();
        information = "";
        ErrorNumber errno;

        ulong  sbSector;
        byte[] sector;
        uint   sectorsToRead;
        ulong  bytes;

        // ADFS-S, ADFS-M, ADFS-L, ADFS-D without partitions
        if(partition.Start == 0)
        {
            errno = imagePlugin.ReadSector(0, out sector);

            if(errno != ErrorNumber.NoError)
                return;

            byte          oldChk0 = AcornMapChecksum(sector, 255);
            OldMapSector0 oldMap0 = Marshal.ByteArrayToStructureLittleEndian<OldMapSector0>(sector);

            errno = imagePlugin.ReadSector(1, out sector);

            if(errno != ErrorNumber.NoError)
                return;

            byte          oldChk1 = AcornMapChecksum(sector, 255);
            OldMapSector1 oldMap1 = Marshal.ByteArrayToStructureLittleEndian<OldMapSector1>(sector);

            // According to documentation map1 MUST start on sector 1. On ADFS-D it starts at 0x100, not on sector 1 (0x400)
            if(oldMap0.checksum == oldChk0 &&
               oldMap1.checksum != oldChk1 &&
               sector.Length    >= 512)
            {
                errno = imagePlugin.ReadSector(0, out sector);

                if(errno != ErrorNumber.NoError)
                    return;

                byte[] tmp = new byte[256];
                Array.Copy(sector, 256, tmp, 0, 256);
                oldChk1 = AcornMapChecksum(tmp, 255);
                oldMap1 = Marshal.ByteArrayToStructureLittleEndian<OldMapSector1>(tmp);
            }

            if(oldMap0.checksum == oldChk0 &&
               oldMap1.checksum == oldChk1 &&
               oldMap0.checksum != 0       &&
               oldMap1.checksum != 0)
            {
                bytes = (ulong)((oldMap0.size[2] << 16) + (oldMap0.size[1] << 8) + oldMap0.size[0]) * 256;
                byte[] namebytes = new byte[10];

                for(int i = 0; i < 5; i++)
                {
                    namebytes[i * 2]       = oldMap0.name[i];
                    namebytes[(i * 2) + 1] = oldMap1.name[i];
                }

                metadata = new FileSystem
                {
                    Bootable    = oldMap1.boot != 0, // Or not?
                    Clusters    = bytes / imagePlugin.Info.SectorSize,
                    ClusterSize = imagePlugin.Info.SectorSize,
                    Type        = FS_TYPE
                };

                if(ArrayHelpers.ArrayIsNullOrEmpty(namebytes))
                {
                    sbSector      = OLD_DIRECTORY_LOCATION / imagePlugin.Info.SectorSize;
                    sectorsToRead = OLD_DIRECTORY_SIZE     / imagePlugin.Info.SectorSize;

                    if(OLD_DIRECTORY_SIZE % imagePlugin.Info.SectorSize > 0)
                        sectorsToRead++;

                    errno = imagePlugin.ReadSectors(sbSector, sectorsToRead, out sector);

                    if(errno != ErrorNumber.NoError)
                        return;

                    if(sector.Length > OLD_DIRECTORY_SIZE)
                    {
                        byte[] tmp = new byte[OLD_DIRECTORY_SIZE];
                        Array.Copy(sector, 0, tmp, 0, OLD_DIRECTORY_SIZE - 53);
                        Array.Copy(sector, sector.Length                 - 54, tmp, OLD_DIRECTORY_SIZE - 54, 53);
                        sector = tmp;
                    }

                    OldDirectory oldRoot = Marshal.ByteArrayToStructureLittleEndian<OldDirectory>(sector);

                    if(oldRoot.header.magic == OLD_DIR_MAGIC &&
                       oldRoot.tail.magic   == OLD_DIR_MAGIC)
                        namebytes = oldRoot.tail.name;
                    else
                    {
                        // RISC OS says the old directory can't be in the new location, hard disks created by RISC OS 3.10 do that...
                        sbSector      = NEW_DIRECTORY_LOCATION / imagePlugin.Info.SectorSize;
                        sectorsToRead = NEW_DIRECTORY_SIZE     / imagePlugin.Info.SectorSize;

                        if(NEW_DIRECTORY_SIZE % imagePlugin.Info.SectorSize > 0)
                            sectorsToRead++;

                        errno = imagePlugin.ReadSectors(sbSector, sectorsToRead, out sector);

                        if(errno != ErrorNumber.NoError)
                            return;

                        if(sector.Length > OLD_DIRECTORY_SIZE)
                        {
                            byte[] tmp = new byte[OLD_DIRECTORY_SIZE];
                            Array.Copy(sector, 0, tmp, 0, OLD_DIRECTORY_SIZE - 53);

                            Array.Copy(sector, sector.Length - 54, tmp, OLD_DIRECTORY_SIZE - 54, 53);

                            sector = tmp;
                        }

                        oldRoot = Marshal.ByteArrayToStructureLittleEndian<OldDirectory>(sector);

                        if(oldRoot.header.magic == OLD_DIR_MAGIC &&
                           oldRoot.tail.magic   == OLD_DIR_MAGIC)
                            namebytes = oldRoot.tail.name;
                        else
                        {
                            errno = imagePlugin.ReadSectors(sbSector, sectorsToRead, out sector);

                            if(errno != ErrorNumber.NoError)
                                return;

                            if(sector.Length > NEW_DIRECTORY_SIZE)
                            {
                                byte[] tmp = new byte[NEW_DIRECTORY_SIZE];
                                Array.Copy(sector, 0, tmp, 0, NEW_DIRECTORY_SIZE - 41);

                                Array.Copy(sector, sector.Length - 42, tmp, NEW_DIRECTORY_SIZE - 42, 41);

                                sector = tmp;
                            }

                            NewDirectory newRoot = Marshal.ByteArrayToStructureLittleEndian<NewDirectory>(sector);

                            if(newRoot.header.magic == NEW_DIR_MAGIC &&
                               newRoot.tail.magic   == NEW_DIR_MAGIC)
                                namebytes = newRoot.tail.title;
                        }
                    }
                }

                sbInformation.AppendLine(Localization.Acorn_Advanced_Disc_Filing_System);
                sbInformation.AppendLine();
                sbInformation.AppendFormat(Localization._0_bytes_per_sector, imagePlugin.Info.SectorSize).AppendLine();
                sbInformation.AppendFormat(Localization.Volume_has_0_bytes, bytes).AppendLine();

                sbInformation.AppendFormat(Localization.Volume_name_0, StringHandlers.CToString(namebytes, Encoding)).
                              AppendLine();

                if(oldMap1.discId > 0)
                {
                    metadata.VolumeSerial = $"{oldMap1.discId:X4}";
                    sbInformation.AppendFormat(Localization.Volume_ID_0_X4, oldMap1.discId).AppendLine();
                }

                if(!ArrayHelpers.ArrayIsNullOrEmpty(namebytes))
                    metadata.VolumeName = StringHandlers.CToString(namebytes, Encoding);

                information = sbInformation.ToString();

                return;
            }
        }

        // Partitioning or not, new formats follow:
        DiscRecord drSb;

        errno = imagePlugin.ReadSector(partition.Start, out sector);

        if(errno != ErrorNumber.NoError)
            return;

        byte newChk = NewMapChecksum(sector);
        AaruConsole.DebugWriteLine("ADFS Plugin", "newChk = {0}", newChk);
        AaruConsole.DebugWriteLine("ADFS Plugin", "map.zoneChecksum = {0}", sector[0]);

        sbSector      = BOOT_BLOCK_LOCATION / imagePlugin.Info.SectorSize;
        sectorsToRead = BOOT_BLOCK_SIZE     / imagePlugin.Info.SectorSize;

        if(BOOT_BLOCK_SIZE % imagePlugin.Info.SectorSize > 0)
            sectorsToRead++;

        errno = imagePlugin.ReadSectors(sbSector + partition.Start, sectorsToRead, out byte[] bootSector);

        if(errno != ErrorNumber.NoError)
            return;

        int bootChk = 0;

        for(int i = 0; i < 0x1FF; i++)
            bootChk = (bootChk & 0xFF) + (bootChk >> 8) + bootSector[i];

        AaruConsole.DebugWriteLine("ADFS Plugin", "bootChk = {0}", bootChk);
        AaruConsole.DebugWriteLine("ADFS Plugin", "bBlock.checksum = {0}", bootSector[0x1FF]);

        if(newChk == sector[0] &&
           newChk != 0)
        {
            NewMap nmap = Marshal.ByteArrayToStructureLittleEndian<NewMap>(sector);
            drSb = nmap.discRecord;
        }
        else if(bootChk == bootSector[0x1FF])
        {
            BootBlock bBlock = Marshal.ByteArrayToStructureLittleEndian<BootBlock>(bootSector);
            drSb = bBlock.discRecord;
        }
        else
            return;

        AaruConsole.DebugWriteLine("ADFS Plugin", "drSb.log2secsize = {0}", drSb.log2secsize);
        AaruConsole.DebugWriteLine("ADFS Plugin", "drSb.spt = {0}", drSb.spt);
        AaruConsole.DebugWriteLine("ADFS Plugin", "drSb.heads = {0}", drSb.heads);
        AaruConsole.DebugWriteLine("ADFS Plugin", "drSb.density = {0}", drSb.density);
        AaruConsole.DebugWriteLine("ADFS Plugin", "drSb.idlen = {0}", drSb.idlen);
        AaruConsole.DebugWriteLine("ADFS Plugin", "drSb.log2bpmb = {0}", drSb.log2bpmb);
        AaruConsole.DebugWriteLine("ADFS Plugin", "drSb.skew = {0}", drSb.skew);
        AaruConsole.DebugWriteLine("ADFS Plugin", "drSb.bootoption = {0}", drSb.bootoption);
        AaruConsole.DebugWriteLine("ADFS Plugin", "drSb.lowsector = {0}", drSb.lowsector);
        AaruConsole.DebugWriteLine("ADFS Plugin", "drSb.nzones = {0}", drSb.nzones);
        AaruConsole.DebugWriteLine("ADFS Plugin", "drSb.zone_spare = {0}", drSb.zone_spare);
        AaruConsole.DebugWriteLine("ADFS Plugin", "drSb.root = {0}", drSb.root);
        AaruConsole.DebugWriteLine("ADFS Plugin", "drSb.disc_size = {0}", drSb.disc_size);
        AaruConsole.DebugWriteLine("ADFS Plugin", "drSb.disc_id = {0}", drSb.disc_id);

        AaruConsole.DebugWriteLine("ADFS Plugin", "drSb.disc_name = {0}",
                                   StringHandlers.CToString(drSb.disc_name, Encoding));

        AaruConsole.DebugWriteLine("ADFS Plugin", "drSb.disc_type = {0}", drSb.disc_type);
        AaruConsole.DebugWriteLine("ADFS Plugin", "drSb.disc_size_high = {0}", drSb.disc_size_high);
        AaruConsole.DebugWriteLine("ADFS Plugin", "drSb.flags = {0}", drSb.flags);
        AaruConsole.DebugWriteLine("ADFS Plugin", "drSb.nzones_high = {0}", drSb.nzones_high);
        AaruConsole.DebugWriteLine("ADFS Plugin", "drSb.format_version = {0}", drSb.format_version);
        AaruConsole.DebugWriteLine("ADFS Plugin", "drSb.root_size = {0}", drSb.root_size);

        if(drSb.log2secsize is < 8 or > 10)
            return;

        if(drSb.idlen < drSb.log2secsize + 3 ||
           drSb.idlen > 19)
            return;

        if(drSb.disc_size_high >> drSb.log2secsize != 0)
            return;

        if(!ArrayHelpers.ArrayIsNullOrEmpty(drSb.reserved))
            return;

        bytes =  drSb.disc_size_high;
        bytes *= 0x100000000;
        bytes += drSb.disc_size;

        ulong zones = drSb.nzones_high;
        zones *= 0x100000000;
        zones += drSb.nzones;

        if(bytes > imagePlugin.Info.Sectors * imagePlugin.Info.SectorSize)
            return;

        metadata = new FileSystem();

        sbInformation.AppendLine(Localization.Acorn_Advanced_Disc_Filing_System);
        sbInformation.AppendLine();
        sbInformation.AppendFormat(Localization.Version_0, drSb.format_version).AppendLine();
        sbInformation.AppendFormat(Localization._0_bytes_per_sector, 1 << drSb.log2secsize).AppendLine();
        sbInformation.AppendFormat(Localization._0_sectors_per_track, drSb.spt).AppendLine();
        sbInformation.AppendFormat(Localization._0_heads, drSb.heads).AppendLine();
        sbInformation.AppendFormat(Localization.Density_code_0, drSb.density).AppendLine();
        sbInformation.AppendFormat(Localization.Skew_0, drSb.skew).AppendLine();
        sbInformation.AppendFormat(Localization.Boot_option_0, drSb.bootoption).AppendLine();

        // TODO: What the hell is this field refering to?
        sbInformation.AppendFormat(Localization.Root_starts_at_frag_0, drSb.root).AppendLine();

        //sbInformation.AppendFormat("Root is {0} bytes long", drSb.root_size).AppendLine();
        sbInformation.AppendFormat(Localization.Volume_has_0_bytes_in_1_zones, bytes, zones).AppendLine();
        sbInformation.AppendFormat(Localization.Volume_flags_0_X4, drSb.flags).AppendLine();

        if(drSb.disc_id > 0)
        {
            metadata.VolumeSerial = $"{drSb.disc_id:X4}";
            sbInformation.AppendFormat(Localization.Volume_ID_0_X4, drSb.disc_id).AppendLine();
        }

        if(!ArrayHelpers.ArrayIsNullOrEmpty(drSb.disc_name))
        {
            string discname = StringHandlers.CToString(drSb.disc_name, Encoding);
            metadata.VolumeName = discname;
            sbInformation.AppendFormat(Localization.Volume_name_0, discname).AppendLine();
        }

        information = sbInformation.ToString();

        metadata.Bootable    |= drSb.bootoption != 0; // Or not?
        metadata.Clusters    =  bytes / (ulong)(1 << drSb.log2secsize);
        metadata.ClusterSize =  (uint)(1 << drSb.log2secsize);
        metadata.Type        =  FS_TYPE;
    }
}