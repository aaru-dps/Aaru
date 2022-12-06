// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Acorn.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Acorn filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Acorn filesystem and shows information.
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
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems
{
    /// <inheritdoc />
    /// <summary>Implements detection of Acorn's Advanced Data Filing System (ADFS)</summary>
    public sealed class AcornADFS : IFilesystem
    {
        /// <summary>Location for boot block, in bytes</summary>
        const ulong BOOT_BLOCK_LOCATION = 0xC00;
        /// <summary>Size of boot block, in bytes</summary>
        const uint BOOT_BLOCK_SIZE = 0x200;
        /// <summary>Location of new directory, in bytes</summary>
        const ulong NEW_DIRECTORY_LOCATION = 0x400;
        /// <summary>Location of old directory, in bytes</summary>
        const ulong OLD_DIRECTORY_LOCATION = 0x200;
        /// <summary>Size of old directory</summary>
        const uint OLD_DIRECTORY_SIZE = 1280;
        /// <summary>Size of new directory</summary>
        const uint NEW_DIRECTORY_SIZE = 2048;

        /// <summary>New directory format magic number, "Nick"</summary>
        const uint NEW_DIR_MAGIC = 0x6B63694E;
        /// <summary>Old directory format magic number, "Hugo"</summary>
        const uint OLD_DIR_MAGIC = 0x6F677548;

        /// <inheritdoc />
        public FileSystemType XmlFsType { get; private set; }
        /// <inheritdoc />
        public string Name => "Acorn Advanced Disc Filing System";
        /// <inheritdoc />
        public Guid Id => new Guid("BAFC1E50-9C64-4CD3-8400-80628CC27AFA");
        /// <inheritdoc />
        public Encoding Encoding { get; private set; }
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

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

            byte[] sector;

            // ADFS-S, ADFS-M, ADFS-L, ADFS-D without partitions
            if(partition.Start == 0)
            {
                sector = imagePlugin.ReadSector(0);
                byte          oldChk0 = AcornMapChecksum(sector, 255);
                OldMapSector0 oldMap0 = Marshal.ByteArrayToStructureLittleEndian<OldMapSector0>(sector);

                sector = imagePlugin.ReadSector(1);
                byte          oldChk1 = AcornMapChecksum(sector, 255);
                OldMapSector1 oldMap1 = Marshal.ByteArrayToStructureLittleEndian<OldMapSector1>(sector);

                AaruConsole.DebugWriteLine("ADFS Plugin", "oldMap0.checksum = {0}", oldMap0.checksum);
                AaruConsole.DebugWriteLine("ADFS Plugin", "oldChk0 = {0}", oldChk0);

                // According to documentation map1 MUST start on sector 1. On ADFS-D it starts at 0x100, not on sector 1 (0x400)
                if(oldMap0.checksum == oldChk0 &&
                   oldMap1.checksum != oldChk1 &&
                   sector.Length    >= 512)
                {
                    sector = imagePlugin.ReadSector(0);
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

                    sector = imagePlugin.ReadSectors(sbSector, sectorsToRead);

                    if(sector.Length > OLD_DIRECTORY_SIZE)
                    {
                        byte[] tmp = new byte[OLD_DIRECTORY_SIZE];
                        Array.Copy(sector, 0, tmp, 0, OLD_DIRECTORY_SIZE - 53);
                        Array.Copy(sector, sector.Length                 - 54, tmp, OLD_DIRECTORY_SIZE - 54, 53);
                        sector = tmp;
                    }

                    OldDirectory oldRoot = Marshal.ByteArrayToStructureLittleEndian<OldDirectory>(sector);
                    byte         dirChk  = AcornDirectoryChecksum(sector, (int)OLD_DIRECTORY_SIZE - 1);

                    AaruConsole.DebugWriteLine("ADFS Plugin", "oldRoot.header.magic at 0x200 = {0}",
                                               oldRoot.header.magic);

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

                    sector = imagePlugin.ReadSectors(sbSector, sectorsToRead);

                    if(sector.Length > OLD_DIRECTORY_SIZE)
                    {
                        byte[] tmp = new byte[OLD_DIRECTORY_SIZE];
                        Array.Copy(sector, 0, tmp, 0, OLD_DIRECTORY_SIZE - 53);
                        Array.Copy(sector, sector.Length                 - 54, tmp, OLD_DIRECTORY_SIZE - 54, 53);
                        sector = tmp;
                    }

                    oldRoot = Marshal.ByteArrayToStructureLittleEndian<OldDirectory>(sector);
                    dirChk  = AcornDirectoryChecksum(sector, (int)OLD_DIRECTORY_SIZE - 1);

                    AaruConsole.DebugWriteLine("ADFS Plugin", "oldRoot.header.magic at 0x400 = {0}",
                                               oldRoot.header.magic);

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

            sector = imagePlugin.ReadSector(partition.Start);
            byte newChk = NewMapChecksum(sector);
            AaruConsole.DebugWriteLine("ADFS Plugin", "newChk = {0}", newChk);
            AaruConsole.DebugWriteLine("ADFS Plugin", "map.zoneChecksum = {0}", sector[0]);

            sbSector      = BOOT_BLOCK_LOCATION / imagePlugin.Info.SectorSize;
            sectorsToRead = BOOT_BLOCK_SIZE     / imagePlugin.Info.SectorSize;

            if(BOOT_BLOCK_SIZE % imagePlugin.Info.SectorSize > 0)
                sectorsToRead++;

            if(sbSector + partition.Start + sectorsToRead >= partition.End)
                return false;

            byte[] bootSector = imagePlugin.ReadSectors(sbSector + partition.Start, sectorsToRead);
            int    bootChk    = 0;

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

            if(drSb.log2secsize < 8 ||
               drSb.log2secsize > 10)
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
        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding = encoding ?? Encoding.GetEncoding("iso-8859-1");
            var sbInformation = new StringBuilder();
            XmlFsType   = new FileSystemType();
            information = "";

            ulong  sbSector;
            byte[] sector;
            uint   sectorsToRead;
            ulong  bytes;

            // ADFS-S, ADFS-M, ADFS-L, ADFS-D without partitions
            if(partition.Start == 0)
            {
                sector = imagePlugin.ReadSector(0);
                byte          oldChk0 = AcornMapChecksum(sector, 255);
                OldMapSector0 oldMap0 = Marshal.ByteArrayToStructureLittleEndian<OldMapSector0>(sector);

                sector = imagePlugin.ReadSector(1);
                byte          oldChk1 = AcornMapChecksum(sector, 255);
                OldMapSector1 oldMap1 = Marshal.ByteArrayToStructureLittleEndian<OldMapSector1>(sector);

                // According to documentation map1 MUST start on sector 1. On ADFS-D it starts at 0x100, not on sector 1 (0x400)
                if(oldMap0.checksum == oldChk0 &&
                   oldMap1.checksum != oldChk1 &&
                   sector.Length    >= 512)
                {
                    sector = imagePlugin.ReadSector(0);
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

                    XmlFsType = new FileSystemType
                    {
                        Bootable    = oldMap1.boot != 0, // Or not?
                        Clusters    = bytes / imagePlugin.Info.SectorSize,
                        ClusterSize = imagePlugin.Info.SectorSize,
                        Type        = "Acorn Advanced Disc Filing System"
                    };

                    if(ArrayHelpers.ArrayIsNullOrEmpty(namebytes))
                    {
                        sbSector      = OLD_DIRECTORY_LOCATION / imagePlugin.Info.SectorSize;
                        sectorsToRead = OLD_DIRECTORY_SIZE     / imagePlugin.Info.SectorSize;

                        if(OLD_DIRECTORY_SIZE % imagePlugin.Info.SectorSize > 0)
                            sectorsToRead++;

                        sector = imagePlugin.ReadSectors(sbSector, sectorsToRead);

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

                            sector = imagePlugin.ReadSectors(sbSector, sectorsToRead);

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
                                sector = imagePlugin.ReadSectors(sbSector, sectorsToRead);

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

                    sbInformation.AppendLine("Acorn Advanced Disc Filing System");
                    sbInformation.AppendLine();
                    sbInformation.AppendFormat("{0} bytes per sector", imagePlugin.Info.SectorSize).AppendLine();
                    sbInformation.AppendFormat("Volume has {0} bytes", bytes).AppendLine();

                    sbInformation.AppendFormat("Volume name: {0}", StringHandlers.CToString(namebytes, Encoding)).
                                  AppendLine();

                    if(oldMap1.discId > 0)
                    {
                        XmlFsType.VolumeSerial = $"{oldMap1.discId:X4}";
                        sbInformation.AppendFormat("Volume ID: {0:X4}", oldMap1.discId).AppendLine();
                    }

                    if(!ArrayHelpers.ArrayIsNullOrEmpty(namebytes))
                        XmlFsType.VolumeName = StringHandlers.CToString(namebytes, Encoding);

                    information = sbInformation.ToString();

                    return;
                }
            }

            // Partitioning or not, new formats follow:
            DiscRecord drSb;

            sector = imagePlugin.ReadSector(partition.Start);
            byte newChk = NewMapChecksum(sector);
            AaruConsole.DebugWriteLine("ADFS Plugin", "newChk = {0}", newChk);
            AaruConsole.DebugWriteLine("ADFS Plugin", "map.zoneChecksum = {0}", sector[0]);

            sbSector      = BOOT_BLOCK_LOCATION / imagePlugin.Info.SectorSize;
            sectorsToRead = BOOT_BLOCK_SIZE     / imagePlugin.Info.SectorSize;

            if(BOOT_BLOCK_SIZE % imagePlugin.Info.SectorSize > 0)
                sectorsToRead++;

            byte[] bootSector = imagePlugin.ReadSectors(sbSector + partition.Start, sectorsToRead);
            int    bootChk    = 0;

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

            if(drSb.log2secsize < 8 ||
               drSb.log2secsize > 10)
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

            XmlFsType = new FileSystemType();

            sbInformation.AppendLine("Acorn Advanced Disc Filing System");
            sbInformation.AppendLine();
            sbInformation.AppendFormat("Version {0}", drSb.format_version).AppendLine();
            sbInformation.AppendFormat("{0} bytes per sector", 1 << drSb.log2secsize).AppendLine();
            sbInformation.AppendFormat("{0} sectors per track", drSb.spt).AppendLine();
            sbInformation.AppendFormat("{0} heads", drSb.heads).AppendLine();
            sbInformation.AppendFormat("Density code: {0}", drSb.density).AppendLine();
            sbInformation.AppendFormat("Skew: {0}", drSb.skew).AppendLine();
            sbInformation.AppendFormat("Boot option: {0}", drSb.bootoption).AppendLine();

            // TODO: What the hell is this field refering to?
            sbInformation.AppendFormat("Root starts at frag {0}", drSb.root).AppendLine();

            //sbInformation.AppendFormat("Root is {0} bytes long", drSb.root_size).AppendLine();
            sbInformation.AppendFormat("Volume has {0} bytes in {1} zones", bytes, zones).AppendLine();
            sbInformation.AppendFormat("Volume flags: 0x{0:X4}", drSb.flags).AppendLine();

            if(drSb.disc_id > 0)
            {
                XmlFsType.VolumeSerial = $"{drSb.disc_id:X4}";
                sbInformation.AppendFormat("Volume ID: {0:X4}", drSb.disc_id).AppendLine();
            }

            if(!ArrayHelpers.ArrayIsNullOrEmpty(drSb.disc_name))
            {
                string discname = StringHandlers.CToString(drSb.disc_name, Encoding);
                XmlFsType.VolumeName = discname;
                sbInformation.AppendFormat("Volume name: {0}", discname).AppendLine();
            }

            information = sbInformation.ToString();

            XmlFsType.Bootable    |= drSb.bootoption != 0; // Or not?
            XmlFsType.Clusters    =  bytes / (ulong)(1 << drSb.log2secsize);
            XmlFsType.ClusterSize =  (uint)(1 << drSb.log2secsize);
            XmlFsType.Type        =  "Acorn Advanced Disc Filing System";
        }

        byte AcornMapChecksum(byte[] data, int length)
        {
            int sum   = 0;
            int carry = 0;

            if(length > data.Length)
                length = data.Length;

            // ADC r0, r0, r1
            // MOVS r0, r0, LSL #24
            // MOV r0, r0, LSR #24
            for(int i = length - 1; i >= 0; i--)
            {
                sum += data[i] + carry;

                if(sum > 0xFF)
                {
                    carry =  1;
                    sum   &= 0xFF;
                }
                else
                    carry = 0;
            }

            return (byte)(sum & 0xFF);
        }

        static byte NewMapChecksum(byte[] mapBase)
        {
            uint rover;
            uint sumVector0 = 0;
            uint sumVector1 = 0;
            uint sumVector2 = 0;
            uint sumVector3 = 0;

            for(rover = (uint)(mapBase.Length - 4); rover > 0; rover -= 4)
            {
                sumVector0 += mapBase[rover + 0] + (sumVector3 >> 8);
                sumVector3 &= 0xff;
                sumVector1 += mapBase[rover + 1] + (sumVector0 >> 8);
                sumVector0 &= 0xff;
                sumVector2 += mapBase[rover + 2] + (sumVector1 >> 8);
                sumVector1 &= 0xff;
                sumVector3 += mapBase[rover + 3] + (sumVector2 >> 8);
                sumVector2 &= 0xff;
            }

            /*
                    Don't add the check byte when calculating its value
            */
            sumVector0 += sumVector3 >> 8;
            sumVector1 += mapBase[rover + 1] + (sumVector0 >> 8);
            sumVector2 += mapBase[rover + 2] + (sumVector1 >> 8);
            sumVector3 += mapBase[rover + 3] + (sumVector2 >> 8);

            return (byte)((sumVector0 ^ sumVector1 ^ sumVector2 ^ sumVector3) & 0xff);
        }

        // TODO: This is not correct...
        static byte AcornDirectoryChecksum(IList<byte> data, int length)
        {
            uint sum = 0;

            if(length > data.Count)
                length = data.Count;

            // EOR r0, r1, r0, ROR #13
            for(int i = 0; i < length; i++)
            {
                uint carry = sum & 0x1FFF;
                sum >>= 13;
                sum ^=  data[i];
                sum +=  carry << 19;
            }

            return (byte)(((sum & 0xFF000000) >> 24) ^ ((sum & 0xFF0000) >> 16) ^ ((sum & 0xFF00) >> 8) ^ (sum & 0xFF));
        }

        /// <summary>Boot block, used in hard disks and ADFS-F and higher.</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct BootBlock
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x1C0)]
            public readonly byte[] spare;
            public readonly DiscRecord discRecord;
            public readonly byte       flags;
            public readonly ushort     startCylinder;
            public readonly byte       checksum;
        }

        /// <summary>Disc record, used in hard disks and ADFS-E and higher.</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct DiscRecord
        {
            public readonly byte   log2secsize;
            public readonly byte   spt;
            public readonly byte   heads;
            public readonly byte   density;
            public readonly byte   idlen;
            public readonly byte   log2bpmb;
            public readonly byte   skew;
            public readonly byte   bootoption;
            public readonly byte   lowsector;
            public readonly byte   nzones;
            public readonly ushort zone_spare;
            public readonly uint   root;
            public readonly uint   disc_size;
            public readonly ushort disc_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public readonly byte[] disc_name;
            public readonly uint disc_type;
            public readonly uint disc_size_high;
            public readonly byte flags;
            public readonly byte nzones_high;
            public readonly uint format_version;
            public readonly uint root_size;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] reserved;
        }

        /// <summary>Free block map, sector 0, used in ADFS-S, ADFS-L, ADFS-M and ADFS-D</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct OldMapSector0
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 82 * 3)]
            public readonly byte[] freeStart;
            public readonly byte reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public readonly byte[] name;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public readonly byte[] size;
            public readonly byte checksum;
        }

        /// <summary>Free block map, sector 1, used in ADFS-S, ADFS-L, ADFS-M and ADFS-D</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct OldMapSector1
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 82 * 3)]
            public readonly byte[] freeStart;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public readonly byte[] name;
            public readonly ushort discId;
            public readonly byte   boot;
            public readonly byte   freeEnd;
            public readonly byte   checksum;
        }

        /// <summary>Free block map, sector 0, used in ADFS-E</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct NewMap
        {
            public readonly byte       zoneChecksum;
            public readonly ushort     freeLink;
            public readonly byte       crossChecksum;
            public readonly DiscRecord discRecord;
        }

        /// <summary>Directory header, common to "old" and "new" directories</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct DirectoryHeader
        {
            public readonly byte masterSequence;
            public readonly uint magic;
        }

        /// <summary>Directory header, common to "old" and "new" directories</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct DirectoryEntry
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public readonly byte[] name;
            public readonly uint load;
            public readonly uint exec;
            public readonly uint length;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public readonly byte[] address;
            public readonly byte atts;
        }

        /// <summary>Directory tail, new format</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct NewDirectoryTail
        {
            public readonly byte   lastMark;
            public readonly ushort reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public readonly byte[] parent;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)]
            public readonly byte[] title;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public readonly byte[] name;
            public readonly byte endMasSeq;
            public readonly uint magic;
            public readonly byte checkByte;
        }

        /// <summary>Directory tail, old format</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct OldDirectoryTail
        {
            public readonly byte lastMark;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public readonly byte[] name;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public readonly byte[] parent;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)]
            public readonly byte[] title;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public readonly byte[] reserved;
            public readonly byte endMasSeq;
            public readonly uint magic;
            public readonly byte checkByte;
        }

        /// <summary>Directory, old format</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct OldDirectory
        {
            public readonly DirectoryHeader header;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 47)]
            public readonly DirectoryEntry[] entries;
            public readonly OldDirectoryTail tail;
        }

        /// <summary>Directory, new format</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct NewDirectory
        {
            public readonly DirectoryHeader header;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 77)]
            public readonly DirectoryEntry[] entries;
            public readonly NewDirectoryTail tail;
        }
    }
}