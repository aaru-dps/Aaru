// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;

namespace DiscImageChef.Filesystems
{
    public class AcornADFS : Filesystem
    {
        /// <summary>
        /// Location for boot block, in bytes
        /// </summary>
        const ulong bootBlockLocation = 0xC00;
        /// <summary>
        /// Size of boot block, in bytes
        /// </summary>
        const uint bootBlockSize = 0x200;
        /// <summary>
        /// Location of new directory, in bytes
        /// </summary>
        const ulong newDirectoryLocation = 0x400;
        /// <summary>
        /// Location of old directory, in bytes
        /// </summary>
        const ulong oldDirectoryLocation = 0x200;
        /// <summary>
        /// Size of old directory
        /// </summary>
        const uint oldDirectorySize = 1280;
        /// <summary>
        /// Size of new directory
        /// </summary>
        const uint newDirectorySize = 2048;

        public AcornADFS()
        {
            Name = "Acorn Advanced Disc Filing System";
            PluginUUID = new Guid("BAFC1E50-9C64-4CD3-8400-80628CC27AFA");
            CurrentEncoding = Encoding.GetEncoding("iso-8859-1");
        }

        public AcornADFS(Encoding encoding)
        {
            Name = "Acorn Advanced Disc Filing System";
            PluginUUID = new Guid("BAFC1E50-9C64-4CD3-8400-80628CC27AFA");
            if(encoding == null) CurrentEncoding = Encoding.GetEncoding("iso-8859-1");
            else CurrentEncoding = encoding;
        }

        public AcornADFS(DiscImages.ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "Acorn Advanced Disc Filing System";
            PluginUUID = new Guid("BAFC1E50-9C64-4CD3-8400-80628CC27AFA");
            if(encoding == null) CurrentEncoding = Encoding.GetEncoding("iso-8859-1");
            else CurrentEncoding = encoding;
        }

        /// <summary>
        /// Boot block, used in hard disks and ADFS-F and higher.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BootBlock
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x1C0)] public byte[] spare;
            public DiscRecord discRecord;
            public byte flags;
            public ushort startCylinder;
            public byte checksum;
        }

        /// <summary>
        /// Disc record, used in hard disks and ADFS-E and higher.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DiscRecord
        {
            public byte log2secsize;
            public byte spt;
            public byte heads;
            public byte density;
            public byte idlen;
            public byte log2bpmb;
            public byte skew;
            public byte bootoption;
            public byte lowsector;
            public byte nzones;
            public ushort zone_spare;
            public uint root;
            public uint disc_size;
            public ushort disc_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] public byte[] disc_name;
            public uint disc_type;
            public uint disc_size_high;
            public byte flags;
            public byte nzones_high;
            public uint format_version;
            public uint root_size;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] reserved;
        }

        /// <summary>
        /// Free block map, sector 0, used in ADFS-S, ADFS-L, ADFS-M and ADFS-D
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct OldMapSector0
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 82 * 3)] public byte[] freeStart;
            public byte reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)] public byte[] name;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] size;
            public byte checksum;
        }

        /// <summary>
        /// Free block map, sector 1, used in ADFS-S, ADFS-L, ADFS-M and ADFS-D
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct OldMapSector1
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 82 * 3)] public byte[] freeStart;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)] public byte[] name;
            public ushort discId;
            public byte boot;
            public byte freeEnd;
            public byte checksum;
        }

        /// <summary>
        /// Free block map, sector 0, used in ADFS-E
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct NewMap
        {
            public byte zoneChecksum;
            public ushort freeLink;
            public byte crossChecksum;
            public DiscRecord discRecord;
        }

        /// <summary>
        /// Directory header, common to "old" and "new" directories
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DirectoryHeader
        {
            public byte masterSequence;
            public uint magic;
        }

        /// <summary>
        /// New directory format magic number, "Nick"
        /// </summary>
        const uint newDirMagic = 0x6B63694E;
        /// <summary>
        /// Old directory format magic number, "Hugo"
        /// </summary>
        const uint oldDirMagic = 0x6F677548;

        /// <summary>
        /// Directory header, common to "old" and "new" directories
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DirectoryEntry
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] public byte[] name;
            public uint load;
            public uint exec;
            public uint length;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] address;
            public byte atts;
        }

        /// <summary>
        /// Directory tail, new format
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct NewDirectoryTail
        {
            public byte lastMark;
            public ushort reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] parent;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)] public byte[] title;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] public byte[] name;
            public byte endMasSeq;
            public uint magic;
            public byte checkByte;
        }

        /// <summary>
        /// Directory tail, old format
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct OldDirectoryTail
        {
            public byte lastMark;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] public byte[] name;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] parent;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)] public byte[] title;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)] public byte[] reserved;
            public byte endMasSeq;
            public uint magic;
            public byte checkByte;
        }

        /// <summary>
        /// Directory, old format
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct OldDirectory
        {
            public DirectoryHeader header;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 47)] public DirectoryEntry[] entries;
            public OldDirectoryTail tail;
        }

        /// <summary>
        /// Directory, new format
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct NewDirectory
        {
            public DirectoryHeader header;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 77)] public DirectoryEntry[] entries;
            public NewDirectoryTail tail;
        }

        // TODO: BBC Master hard disks are untested...
        public override bool Identify(DiscImages.ImagePlugin imagePlugin, Partition partition)
        {
            if(partition.Start >= partition.End) return false;

            ulong sbSector;
            uint sectorsToRead;

            if(imagePlugin.ImageInfo.SectorSize < 256) return false;

            byte[] sector;
            GCHandle ptr;

            // ADFS-S, ADFS-M, ADFS-L, ADFS-D without partitions
            if(partition.Start == 0)
            {
                OldMapSector0 oldMap0;
                OldMapSector1 oldMap1;
                OldDirectory oldRoot;
                byte oldChk0;
                byte oldChk1;
                byte dirChk;

                sector = imagePlugin.ReadSector(0);
                oldChk0 = AcornMapChecksum(sector, 255);
                ptr = GCHandle.Alloc(sector, GCHandleType.Pinned);
                oldMap0 = (OldMapSector0)Marshal.PtrToStructure(ptr.AddrOfPinnedObject(), typeof(OldMapSector0));

                sector = imagePlugin.ReadSector(1);
                oldChk1 = AcornMapChecksum(sector, 255);
                ptr = GCHandle.Alloc(sector, GCHandleType.Pinned);
                oldMap1 = (OldMapSector1)Marshal.PtrToStructure(ptr.AddrOfPinnedObject(), typeof(OldMapSector1));

                DicConsole.DebugWriteLine("ADFS Plugin", "oldMap0.checksum = {0}", oldMap0.checksum);
                DicConsole.DebugWriteLine("ADFS Plugin", "oldChk0 = {0}", oldChk0);

                // According to documentation map1 MUST start on sector 1. On ADFS-D it starts at 0x100, not on sector 1 (0x400)
                if(oldMap0.checksum == oldChk0 && oldMap1.checksum != oldChk1 && sector.Length >= 512)
                {
                    sector = imagePlugin.ReadSector(0);
                    byte[] tmp = new byte[256];
                    Array.Copy(sector, 256, tmp, 0, 256);
                    oldChk1 = AcornMapChecksum(tmp, 255);
                    ptr = GCHandle.Alloc(tmp, GCHandleType.Pinned);
                    oldMap1 = (OldMapSector1)Marshal.PtrToStructure(ptr.AddrOfPinnedObject(), typeof(OldMapSector1));
                }

                DicConsole.DebugWriteLine("ADFS Plugin", "oldMap1.checksum = {0}", oldMap1.checksum);
                DicConsole.DebugWriteLine("ADFS Plugin", "oldChk1 = {0}", oldChk1);

                if(oldMap0.checksum == oldChk0 && oldMap1.checksum == oldChk1 && oldMap0.checksum != 0 &&
                   oldMap1.checksum != 0)
                {
                    sbSector = oldDirectoryLocation / imagePlugin.ImageInfo.SectorSize;
                    sectorsToRead = oldDirectorySize / imagePlugin.ImageInfo.SectorSize;
                    if(oldDirectorySize % imagePlugin.ImageInfo.SectorSize > 0) sectorsToRead++;

                    sector = imagePlugin.ReadSectors(sbSector, sectorsToRead);
                    if(sector.Length > oldDirectorySize)
                    {
                        byte[] tmp = new byte[oldDirectorySize];
                        Array.Copy(sector, 0, tmp, 0, oldDirectorySize - 53);
                        Array.Copy(sector, sector.Length - 54, tmp, oldDirectorySize - 54, 53);
                        sector = tmp;
                    }
                    ptr = GCHandle.Alloc(sector, GCHandleType.Pinned);
                    oldRoot = (OldDirectory)Marshal.PtrToStructure(ptr.AddrOfPinnedObject(), typeof(OldDirectory));
                    dirChk = AcornDirectoryChecksum(sector, (int)oldDirectorySize - 1);

                    DicConsole.DebugWriteLine("ADFS Plugin", "oldRoot.header.magic at 0x200 = {0}",
                                              oldRoot.header.magic);
                    DicConsole.DebugWriteLine("ADFS Plugin", "oldRoot.tail.magic at 0x200 = {0}", oldRoot.tail.magic);
                    DicConsole.DebugWriteLine("ADFS Plugin", "oldRoot.tail.checkByte at 0x200 = {0}",
                                              oldRoot.tail.checkByte);
                    DicConsole.DebugWriteLine("ADFS Plugin", "dirChk at 0x200 = {0}", dirChk);

                    if(oldRoot.header.magic == oldDirMagic && oldRoot.tail.magic == oldDirMagic ||
                       oldRoot.header.magic == newDirMagic && oldRoot.tail.magic == newDirMagic) return true;

                    // RISC OS says the old directory can't be in the new location, hard disks created by RISC OS 3.10 do that...
                    sbSector = newDirectoryLocation / imagePlugin.ImageInfo.SectorSize;
                    sectorsToRead = newDirectorySize / imagePlugin.ImageInfo.SectorSize;
                    if(newDirectorySize % imagePlugin.ImageInfo.SectorSize > 0) sectorsToRead++;

                    sector = imagePlugin.ReadSectors(sbSector, sectorsToRead);
                    if(sector.Length > oldDirectorySize)
                    {
                        byte[] tmp = new byte[oldDirectorySize];
                        Array.Copy(sector, 0, tmp, 0, oldDirectorySize - 53);
                        Array.Copy(sector, sector.Length - 54, tmp, oldDirectorySize - 54, 53);
                        sector = tmp;
                    }
                    ptr = GCHandle.Alloc(sector, GCHandleType.Pinned);
                    oldRoot = (OldDirectory)Marshal.PtrToStructure(ptr.AddrOfPinnedObject(), typeof(OldDirectory));
                    dirChk = AcornDirectoryChecksum(sector, (int)oldDirectorySize - 1);

                    DicConsole.DebugWriteLine("ADFS Plugin", "oldRoot.header.magic at 0x400 = {0}",
                                              oldRoot.header.magic);
                    DicConsole.DebugWriteLine("ADFS Plugin", "oldRoot.tail.magic at 0x400 = {0}", oldRoot.tail.magic);
                    DicConsole.DebugWriteLine("ADFS Plugin", "oldRoot.tail.checkByte at 0x400 = {0}",
                                              oldRoot.tail.checkByte);
                    DicConsole.DebugWriteLine("ADFS Plugin", "dirChk at 0x400 = {0}", dirChk);

                    if(oldRoot.header.magic == oldDirMagic && oldRoot.tail.magic == oldDirMagic ||
                       oldRoot.header.magic == newDirMagic && oldRoot.tail.magic == newDirMagic) return true;
                }
            }

            // Partitioning or not, new formats follow:
            DiscRecord drSb;

            sector = imagePlugin.ReadSector(partition.Start);
            byte newChk = NewMapChecksum(sector);
            DicConsole.DebugWriteLine("ADFS Plugin", "newChk = {0}", newChk);
            DicConsole.DebugWriteLine("ADFS Plugin", "map.zoneChecksum = {0}", sector[0]);

            sbSector = bootBlockLocation / imagePlugin.ImageInfo.SectorSize;
            sectorsToRead = bootBlockSize / imagePlugin.ImageInfo.SectorSize;
            if(bootBlockSize % imagePlugin.ImageInfo.SectorSize > 0) sectorsToRead++;

            if(sbSector + partition.Start + sectorsToRead >= partition.End) return false;

            byte[] bootSector = imagePlugin.ReadSectors(sbSector + partition.Start, sectorsToRead);
            int bootChk = 0;
            for(int i = 0; i < 0x1FF; i++) bootChk = (bootChk & 0xFF) + (bootChk >> 8) + bootSector[i];

            DicConsole.DebugWriteLine("ADFS Plugin", "bootChk = {0}", bootChk);
            DicConsole.DebugWriteLine("ADFS Plugin", "bBlock.checksum = {0}", bootSector[0x1FF]);

            if(newChk == sector[0] && newChk != 0)
            {
                ptr = GCHandle.Alloc(sector, GCHandleType.Pinned);
                NewMap nmap = (NewMap)Marshal.PtrToStructure(ptr.AddrOfPinnedObject(), typeof(NewMap));
                ptr.Free();
                drSb = nmap.discRecord;
            }
            else if(bootChk == bootSector[0x1FF])
            {
                ptr = GCHandle.Alloc(bootSector, GCHandleType.Pinned);
                BootBlock bBlock = (BootBlock)Marshal.PtrToStructure(ptr.AddrOfPinnedObject(), typeof(BootBlock));
                ptr.Free();
                drSb = bBlock.discRecord;
            }
            else return false;

            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.log2secsize = {0}", drSb.log2secsize);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.idlen = {0}", drSb.idlen);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.disc_size_high = {0}", drSb.disc_size_high);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.disc_size = {0}", drSb.disc_size);
            DicConsole.DebugWriteLine("ADFS Plugin", "IsNullOrEmpty(drSb.reserved) = {0}",
                                      ArrayHelpers.ArrayIsNullOrEmpty(drSb.reserved));

            if(drSb.log2secsize < 8 || drSb.log2secsize > 10) return false;

            if(drSb.idlen < drSb.log2secsize + 3 || drSb.idlen > 19) return false;

            if(drSb.disc_size_high >> drSb.log2secsize != 0) return false;

            if(!ArrayHelpers.ArrayIsNullOrEmpty(drSb.reserved)) return false;

            ulong bytes = drSb.disc_size_high;
            bytes *= 0x100000000;
            bytes += drSb.disc_size;

            if(bytes > imagePlugin.GetSectors() * imagePlugin.GetSectorSize()) return false;

            return true;
        }

        // TODO: Find root directory on volumes with DiscRecord
        // TODO: Support big directories (ADFS-G?)
        // TODO: Find the real freemap on volumes with DiscRecord, as DiscRecord's discid may be empty but this one isn't
        public override void GetInformation(DiscImages.ImagePlugin imagePlugin, Partition partition,
                                            out string information)
        {
            StringBuilder sbInformation = new StringBuilder();
            xmlFSType = new Schemas.FileSystemType();
            information = "";

            ulong sbSector;
            byte[] sector;
            uint sectorsToRead;
            GCHandle ptr;
            ulong bytes;
            string discname;

            // ADFS-S, ADFS-M, ADFS-L, ADFS-D without partitions
            if(partition.Start == 0)
            {
                OldMapSector0 oldMap0;
                OldMapSector1 oldMap1;
                OldDirectory oldRoot;
                NewDirectory newRoot;
                byte oldChk0;
                byte oldChk1;

                sector = imagePlugin.ReadSector(0);
                oldChk0 = AcornMapChecksum(sector, 255);
                ptr = GCHandle.Alloc(sector, GCHandleType.Pinned);
                oldMap0 = (OldMapSector0)Marshal.PtrToStructure(ptr.AddrOfPinnedObject(), typeof(OldMapSector0));

                sector = imagePlugin.ReadSector(1);
                oldChk1 = AcornMapChecksum(sector, 255);
                ptr = GCHandle.Alloc(sector, GCHandleType.Pinned);
                oldMap1 = (OldMapSector1)Marshal.PtrToStructure(ptr.AddrOfPinnedObject(), typeof(OldMapSector1));

                // According to documentation map1 MUST start on sector 1. On ADFS-D it starts at 0x100, not on sector 1 (0x400)
                if(oldMap0.checksum == oldChk0 && oldMap1.checksum != oldChk1 && sector.Length >= 512)
                {
                    sector = imagePlugin.ReadSector(0);
                    byte[] tmp = new byte[256];
                    Array.Copy(sector, 256, tmp, 0, 256);
                    oldChk1 = AcornMapChecksum(tmp, 255);
                    ptr = GCHandle.Alloc(tmp, GCHandleType.Pinned);
                    oldMap1 = (OldMapSector1)Marshal.PtrToStructure(ptr.AddrOfPinnedObject(), typeof(OldMapSector1));
                }

                if(oldMap0.checksum == oldChk0 && oldMap1.checksum == oldChk1 && oldMap0.checksum != 0 &&
                   oldMap1.checksum != 0)
                {
                    bytes = (ulong)((oldMap0.size[2] << 16) + (oldMap0.size[1] << 8) + oldMap0.size[0]) * 256;
                    byte[] namebytes = new byte[10];
                    for(int i = 0; i < 5; i++)
                    {
                        namebytes[i * 2] = oldMap0.name[i];
                        namebytes[i * 2 + 1] = oldMap1.name[i];
                    }

                    xmlFSType = new Schemas.FileSystemType
                    {
                        Bootable = oldMap1.boot != 0, // Or not?
                        Clusters = (long)(bytes / imagePlugin.ImageInfo.SectorSize),
                        ClusterSize = (int)imagePlugin.ImageInfo.SectorSize,
                        Type = "Acorn Advanced Disc Filing System",
                    };

                    if(ArrayHelpers.ArrayIsNullOrEmpty(namebytes))
                    {
                        sbSector = oldDirectoryLocation / imagePlugin.ImageInfo.SectorSize;
                        sectorsToRead = oldDirectorySize / imagePlugin.ImageInfo.SectorSize;
                        if(oldDirectorySize % imagePlugin.ImageInfo.SectorSize > 0) sectorsToRead++;

                        sector = imagePlugin.ReadSectors(sbSector, sectorsToRead);
                        if(sector.Length > oldDirectorySize)
                        {
                            byte[] tmp = new byte[oldDirectorySize];
                            Array.Copy(sector, 0, tmp, 0, oldDirectorySize - 53);
                            Array.Copy(sector, sector.Length - 54, tmp, oldDirectorySize - 54, 53);
                            sector = tmp;
                        }
                        ptr = GCHandle.Alloc(sector, GCHandleType.Pinned);
                        oldRoot = (OldDirectory)Marshal.PtrToStructure(ptr.AddrOfPinnedObject(), typeof(OldDirectory));

                        if(oldRoot.header.magic == oldDirMagic && oldRoot.tail.magic == oldDirMagic)
                        {
                            namebytes = oldRoot.tail.name;
                        }
                        else
                        {
                            // RISC OS says the old directory can't be in the new location, hard disks created by RISC OS 3.10 do that...
                            sbSector = newDirectoryLocation / imagePlugin.ImageInfo.SectorSize;
                            sectorsToRead = newDirectorySize / imagePlugin.ImageInfo.SectorSize;
                            if(newDirectorySize % imagePlugin.ImageInfo.SectorSize > 0) sectorsToRead++;

                            sector = imagePlugin.ReadSectors(sbSector, sectorsToRead);
                            if(sector.Length > oldDirectorySize)
                            {
                                byte[] tmp = new byte[oldDirectorySize];
                                Array.Copy(sector, 0, tmp, 0, oldDirectorySize - 53);
                                Array.Copy(sector, sector.Length - 54, tmp, oldDirectorySize - 54, 53);
                                sector = tmp;
                            }
                            ptr = GCHandle.Alloc(sector, GCHandleType.Pinned);
                            oldRoot = (OldDirectory)Marshal.PtrToStructure(ptr.AddrOfPinnedObject(),
                                                                           typeof(OldDirectory));

                            if(oldRoot.header.magic == oldDirMagic && oldRoot.tail.magic == oldDirMagic)
                            {
                                namebytes = oldRoot.tail.name;
                            }
                            else
                            {
                                sector = imagePlugin.ReadSectors(sbSector, sectorsToRead);
                                if(sector.Length > newDirectorySize)
                                {
                                    byte[] tmp = new byte[newDirectorySize];
                                    Array.Copy(sector, 0, tmp, 0, newDirectorySize - 41);
                                    Array.Copy(sector, sector.Length - 42, tmp, newDirectorySize - 42, 41);
                                    sector = tmp;
                                }
                                ptr = GCHandle.Alloc(sector, GCHandleType.Pinned);
                                newRoot = (NewDirectory)Marshal.PtrToStructure(ptr.AddrOfPinnedObject(),
                                                                               typeof(NewDirectory));
                                if(newRoot.header.magic == newDirMagic && newRoot.tail.magic == newDirMagic)
                                {
                                    namebytes = newRoot.tail.title;
                                }
                            }
                        }
                    }

                    sbInformation.AppendLine("Acorn Advanced Disc Filing System");
                    sbInformation.AppendLine();
                    sbInformation.AppendFormat("{0} bytes per sector", imagePlugin.ImageInfo.SectorSize).AppendLine();
                    sbInformation.AppendFormat("Volume has {0} bytes", bytes).AppendLine();
                    sbInformation.AppendFormat("Volume name: {0}", StringHandlers.CToString(namebytes, CurrentEncoding))
                                 .AppendLine();
                    if(oldMap1.discId > 0)
                    {
                        xmlFSType.VolumeSerial = string.Format("{0:X4}", oldMap1.discId);
                        sbInformation.AppendFormat("Volume ID: {0:X4}", oldMap1.discId).AppendLine();
                    }
                    if(!ArrayHelpers.ArrayIsNullOrEmpty(namebytes))
                        xmlFSType.VolumeName = StringHandlers.CToString(namebytes, CurrentEncoding);

                    information = sbInformation.ToString();

                    return;
                }
            }

            // Partitioning or not, new formats follow:
            DiscRecord drSb;

            sector = imagePlugin.ReadSector(partition.Start);
            byte newChk = NewMapChecksum(sector);
            DicConsole.DebugWriteLine("ADFS Plugin", "newChk = {0}", newChk);
            DicConsole.DebugWriteLine("ADFS Plugin", "map.zoneChecksum = {0}", sector[0]);

            sbSector = bootBlockLocation / imagePlugin.ImageInfo.SectorSize;
            sectorsToRead = bootBlockSize / imagePlugin.ImageInfo.SectorSize;
            if(bootBlockSize % imagePlugin.ImageInfo.SectorSize > 0) sectorsToRead++;

            byte[] bootSector = imagePlugin.ReadSectors(sbSector + partition.Start, sectorsToRead);
            int bootChk = 0;
            for(int i = 0; i < 0x1FF; i++) bootChk = (bootChk & 0xFF) + (bootChk >> 8) + bootSector[i];

            DicConsole.DebugWriteLine("ADFS Plugin", "bootChk = {0}", bootChk);
            DicConsole.DebugWriteLine("ADFS Plugin", "bBlock.checksum = {0}", bootSector[0x1FF]);

            if(newChk == sector[0] && newChk != 0)
            {
                ptr = GCHandle.Alloc(sector, GCHandleType.Pinned);
                NewMap nmap = (NewMap)Marshal.PtrToStructure(ptr.AddrOfPinnedObject(), typeof(NewMap));
                ptr.Free();
                drSb = nmap.discRecord;
            }
            else if(bootChk == bootSector[0x1FF])
            {
                ptr = GCHandle.Alloc(bootSector, GCHandleType.Pinned);
                BootBlock bBlock = (BootBlock)Marshal.PtrToStructure(ptr.AddrOfPinnedObject(), typeof(BootBlock));
                ptr.Free();
                drSb = bBlock.discRecord;
            }
            else return;

            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.log2secsize = {0}", drSb.log2secsize);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.spt = {0}", drSb.spt);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.heads = {0}", drSb.heads);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.density = {0}", drSb.density);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.idlen = {0}", drSb.idlen);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.log2bpmb = {0}", drSb.log2bpmb);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.skew = {0}", drSb.skew);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.bootoption = {0}", drSb.bootoption);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.lowsector = {0}", drSb.lowsector);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.nzones = {0}", drSb.nzones);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.zone_spare = {0}", drSb.zone_spare);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.root = {0}", drSb.root);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.disc_size = {0}", drSb.disc_size);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.disc_id = {0}", drSb.disc_id);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.disc_name = {0}",
                                      StringHandlers.CToString(drSb.disc_name, CurrentEncoding));
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.disc_type = {0}", drSb.disc_type);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.disc_size_high = {0}", drSb.disc_size_high);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.flags = {0}", drSb.flags);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.nzones_high = {0}", drSb.nzones_high);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.format_version = {0}", drSb.format_version);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.root_size = {0}", drSb.root_size);

            if(drSb.log2secsize < 8 || drSb.log2secsize > 10) return;

            if(drSb.idlen < drSb.log2secsize + 3 || drSb.idlen > 19) return;

            if(drSb.disc_size_high >> drSb.log2secsize != 0) return;

            if(!ArrayHelpers.ArrayIsNullOrEmpty(drSb.reserved)) return;

            bytes = drSb.disc_size_high;
            bytes *= 0x100000000;
            bytes += drSb.disc_size;

            ulong zones = drSb.nzones_high;
            zones *= 0x100000000;
            zones += drSb.nzones;

            if(bytes > imagePlugin.GetSectors() * imagePlugin.GetSectorSize()) return;

            xmlFSType = new Schemas.FileSystemType();

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
                xmlFSType.VolumeSerial = string.Format("{0:X4}", drSb.disc_id);
                sbInformation.AppendFormat("Volume ID: {0:X4}", drSb.disc_id).AppendLine();
            }
            if(!ArrayHelpers.ArrayIsNullOrEmpty(drSb.disc_name))
            {
                discname = StringHandlers.CToString(drSb.disc_name, CurrentEncoding);
                xmlFSType.VolumeName = discname;
                sbInformation.AppendFormat("Volume name: {0}", discname).AppendLine();
            }

            information = sbInformation.ToString();

            xmlFSType.Bootable |= drSb.bootoption != 0; // Or not?
            xmlFSType.Clusters = (long)(bytes / (ulong)(1 << drSb.log2secsize));
            xmlFSType.ClusterSize = 1 << drSb.log2secsize;
            xmlFSType.Type = "Acorn Advanced Disc Filing System";
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

        byte AcornMapChecksum(byte[] data, int length)
        {
            int sum = 0;
            int carry = 0;

            if(length > data.Length) length = data.Length;

            // ADC r0, r0, r1
            // MOVS r0, r0, LSL #24
            // MOV r0, r0, LSR #24
            for(int i = length - 1; i >= 0; i--)
            {
                sum += data[i] + carry;
                if(sum > 0xFF)
                {
                    carry = 1;
                    sum &= 0xFF;
                }
                else carry = 0;
            }

            return (byte)(sum & 0xFF);
        }

        byte NewMapChecksum(byte[] map_base)
        {
            uint sum_vector0;
            uint sum_vector1;
            uint sum_vector2;
            uint sum_vector3;
            uint rover;

            sum_vector0 = 0;
            sum_vector1 = 0;
            sum_vector2 = 0;
            sum_vector3 = 0;

            for(rover = (uint)(map_base.Length - 4); rover > 0; rover -= 4)
            {
                sum_vector0 += map_base[rover + 0] + (sum_vector3 >> 8);
                sum_vector3 &= 0xff;
                sum_vector1 += map_base[rover + 1] + (sum_vector0 >> 8);
                sum_vector0 &= 0xff;
                sum_vector2 += map_base[rover + 2] + (sum_vector1 >> 8);
                sum_vector1 &= 0xff;
                sum_vector3 += map_base[rover + 3] + (sum_vector2 >> 8);
                sum_vector2 &= 0xff;
            }

            /*
                    Don't add the check byte when calculating its value
            */
            sum_vector0 += sum_vector3 >> 8;
            sum_vector1 += map_base[rover + 1] + (sum_vector0 >> 8);
            sum_vector2 += map_base[rover + 2] + (sum_vector1 >> 8);
            sum_vector3 += map_base[rover + 3] + (sum_vector2 >> 8);

            return (byte)((sum_vector0 ^ sum_vector1 ^ sum_vector2 ^ sum_vector3) & 0xff);
        }

        // TODO: This is not correct...
        byte AcornDirectoryChecksum(byte[] data, int length)
        {
            uint sum = 0;

            if(length > data.Length) length = data.Length;

            // EOR r0, r1, r0, ROR #13
            for(int i = 0; i < length; i++)
            {
                uint carry = sum & 0x1FFF;
                sum >>= 13;
                sum ^= data[i];
                sum += carry << 19;
            }

            return (byte)(((sum & 0xFF000000) >> 24) ^ ((sum & 0xFF0000) >> 16) ^ ((sum & 0xFF00) >> 8) ^ (sum & 0xFF));
        }
    }
}