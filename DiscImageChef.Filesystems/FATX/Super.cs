// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Super.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : FATX filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles mounting and umounting the FATX filesystem.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using Schemas;
using Marshal = DiscImageChef.Helpers.Marshal;

namespace DiscImageChef.Filesystems.FATX
{
    public partial class XboxFatPlugin
    {
        public Errno Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                           Dictionary<string, string> options,     string    @namespace)
        {
            Encoding     = Encoding.GetEncoding("iso-8859-15");
            littleEndian = true;
            if(options == null) options = GetDefaultOptions();
            if(options.TryGetValue("debug", out string debugString)) bool.TryParse(debugString, out debug);

            if(imagePlugin.Info.SectorSize < 512) return Errno.InvalidArgument;

            DicConsole.DebugWriteLine("Xbox FAT plugin", "Reading superblock");

            byte[] sector = imagePlugin.ReadSector(partition.Start);

            superblock = Marshal.ByteArrayToStructureLittleEndian<Superblock>(sector);

            if(superblock.magic == FATX_CIGAM)
            {
                superblock   = Marshal.ByteArrayToStructureBigEndian<Superblock>(sector);
                littleEndian = false;
            }

            if(superblock.magic != FATX_MAGIC) return Errno.InvalidArgument;

            DicConsole.DebugWriteLine("Xbox FAT plugin",
                                      littleEndian ? "Filesystem is little endian" : "Filesystem is big endian");

            int logicalSectorsPerPhysicalSectors = partition.Offset == 0 && littleEndian ? 8 : 1;
            DicConsole.DebugWriteLine("Xbox FAT plugin", "logicalSectorsPerPhysicalSectors = {0}",
                                      logicalSectorsPerPhysicalSectors);

            string volumeLabel = StringHandlers.CToString(superblock.volumeLabel,
                                                          !littleEndian ? Encoding.BigEndianUnicode : Encoding.Unicode,
                                                          true);

            XmlFsType = new FileSystemType
            {
                Type = "FATX filesystem",
                ClusterSize =
                    (uint)(superblock.sectorsPerCluster * logicalSectorsPerPhysicalSectors *
                           imagePlugin.Info.SectorSize),
                VolumeName   = volumeLabel,
                VolumeSerial = $"{superblock.id:X8}"
            };
            XmlFsType.Clusters = (partition.End - partition.Start + 1) * imagePlugin.Info.SectorSize /
                                 XmlFsType.ClusterSize;

            statfs = new FileSystemInfo
            {
                Blocks         = XmlFsType.Clusters,
                FilenameLength = MAX_FILENAME,
                Files          = 0, // Requires traversing all directories
                FreeFiles      = 0,
                Id             = {IsInt = true, Serial32 = superblock.magic},
                PluginId       = Id,
                Type           = littleEndian ? "Xbox FAT" : "Xbox 360 FAT",
                FreeBlocks     = 0 // Requires traversing the FAT
            };

            DicConsole.DebugWriteLine("Xbox FAT plugin", "XmlFsType.ClusterSize: {0}",  XmlFsType.ClusterSize);
            DicConsole.DebugWriteLine("Xbox FAT plugin", "XmlFsType.VolumeName: {0}",   XmlFsType.VolumeName);
            DicConsole.DebugWriteLine("Xbox FAT plugin", "XmlFsType.VolumeSerial: {0}", XmlFsType.VolumeSerial);
            DicConsole.DebugWriteLine("Xbox FAT plugin", "stat.Blocks: {0}",            statfs.Blocks);
            DicConsole.DebugWriteLine("Xbox FAT plugin", "stat.FilenameLength: {0}",    statfs.FilenameLength);
            DicConsole.DebugWriteLine("Xbox FAT plugin", "stat.Id: {0}",                statfs.Id.Serial32);
            DicConsole.DebugWriteLine("Xbox FAT plugin", "stat.Type: {0}",              statfs.Type);

            byte[] buffer;
            fatStartSector = FAT_START / imagePlugin.Info.SectorSize + partition.Start;
            uint fatSize;

            DicConsole.DebugWriteLine("Xbox FAT plugin", "fatStartSector: {0}", fatStartSector);

            if(statfs.Blocks > MAX_XFAT16_CLUSTERS)
            {
                DicConsole.DebugWriteLine("Xbox FAT plugin", "Reading FAT32");

                fatSize = (uint)((statfs.Blocks + 1) * sizeof(uint) / imagePlugin.Info.SectorSize);
                if((uint)((statfs.Blocks        + 1) * sizeof(uint) % imagePlugin.Info.SectorSize) > 0) fatSize++;

                long fatClusters = fatSize * imagePlugin.Info.SectorSize / 4096;
                if(fatSize * imagePlugin.Info.SectorSize % 4096 > 0) fatClusters++;
                fatSize = (uint)(fatClusters * 4096 / imagePlugin.Info.SectorSize);

                DicConsole.DebugWriteLine("Xbox FAT plugin", "FAT is {0} sectors", fatSize);

                buffer = imagePlugin.ReadSectors(fatStartSector, fatSize);

                DicConsole.DebugWriteLine("Xbox FAT plugin", "Casting FAT");
                fat32 = MemoryMarshal.Cast<byte, uint>(buffer).ToArray();
                if(!littleEndian)
                    for(int i = 0; i < fat32.Length; i++)
                        fat32[i] = Swapping.Swap(fat32[i]);

                DicConsole.DebugWriteLine("Xbox FAT plugin", "fat32[0] == FATX32_ID = {0}", fat32[0] == FATX32_ID);
                if(fat32[0] != FATX32_ID) return Errno.InvalidArgument;
            }
            else
            {
                DicConsole.DebugWriteLine("Xbox FAT plugin", "Reading FAT16");

                fatSize = (uint)((statfs.Blocks + 1) * sizeof(ushort) / imagePlugin.Info.SectorSize);
                if((uint)((statfs.Blocks        + 1) * sizeof(ushort) % imagePlugin.Info.SectorSize) > 0) fatSize++;

                long fatClusters = fatSize * imagePlugin.Info.SectorSize / 4096;
                if(fatSize * imagePlugin.Info.SectorSize % 4096 > 0) fatClusters++;
                fatSize = (uint)(fatClusters * 4096 / imagePlugin.Info.SectorSize);

                DicConsole.DebugWriteLine("Xbox FAT plugin", "FAT is {0} sectors", fatSize);

                buffer = imagePlugin.ReadSectors(fatStartSector, fatSize);

                DicConsole.DebugWriteLine("Xbox FAT plugin", "Casting FAT");
                fat16 = MemoryMarshal.Cast<byte, ushort>(buffer).ToArray();
                if(!littleEndian)
                    for(int i = 0; i < fat16.Length; i++)
                        fat16[i] = Swapping.Swap(fat16[i]);

                DicConsole.DebugWriteLine("Xbox FAT plugin", "fat16[0] == FATX16_ID = {0}", fat16[0] == FATX16_ID);
                if(fat16[0] != FATX16_ID) return Errno.InvalidArgument;
            }

            sectorsPerCluster  = (uint)(superblock.sectorsPerCluster * logicalSectorsPerPhysicalSectors);
            this.imagePlugin   = imagePlugin;
            firstClusterSector = fatStartSector + fatSize;
            bytesPerCluster    = sectorsPerCluster * imagePlugin.Info.SectorSize;

            DicConsole.DebugWriteLine("Xbox FAT plugin", "sectorsPerCluster = {0}",  sectorsPerCluster);
            DicConsole.DebugWriteLine("Xbox FAT plugin", "bytesPerCluster = {0}",    bytesPerCluster);
            DicConsole.DebugWriteLine("Xbox FAT plugin", "firstClusterSector = {0}", firstClusterSector);

            uint[] rootDirectoryClusters = GetClusters(superblock.rootDirectoryCluster);

            if(rootDirectoryClusters is null) return Errno.InvalidArgument;

            byte[] rootDirectoryBuffer = new byte[bytesPerCluster * rootDirectoryClusters.Length];

            DicConsole.DebugWriteLine("Xbox FAT plugin", "Reading root directory");
            for(int i = 0; i < rootDirectoryClusters.Length; i++)
            {
                buffer =
                    imagePlugin.ReadSectors(firstClusterSector + (rootDirectoryClusters[i] - 1) * sectorsPerCluster,
                                            sectorsPerCluster);
                Array.Copy(buffer, 0, rootDirectoryBuffer, i * bytesPerCluster, bytesPerCluster);
            }

            rootDirectory = new Dictionary<string, DirectoryEntry>();

            int pos = 0;
            while(pos < rootDirectoryBuffer.Length)
            {
                DirectoryEntry entry = littleEndian
                                           ? Marshal
                                              .ByteArrayToStructureLittleEndian<DirectoryEntry
                                               >(rootDirectoryBuffer, pos, Marshal.SizeOf<DirectoryEntry>())
                                           : Marshal.ByteArrayToStructureBigEndian<DirectoryEntry>(rootDirectoryBuffer,
                                                                                                   pos,
                                                                                                   Marshal
                                                                                                      .SizeOf<
                                                                                                           DirectoryEntry
                                                                                                       >());

                pos += Marshal.SizeOf<DirectoryEntry>();

                if(entry.filenameSize == UNUSED_DIRENTRY || entry.filenameSize == FINISHED_DIRENTRY) break;

                if(entry.filenameSize == DELETED_DIRENTRY || entry.filenameSize > MAX_FILENAME) continue;

                string filename = Encoding.GetString(entry.filename, 0, entry.filenameSize);

                rootDirectory.Add(filename, entry);
            }

            cultureInfo    = new CultureInfo("en-US", false);
            directoryCache = new Dictionary<string, Dictionary<string, DirectoryEntry>>();
            mounted        = true;

            return Errno.NoError;
        }

        public Errno Unmount()
        {
            if(!mounted) return Errno.AccessDenied;

            fat16       = null;
            fat32       = null;
            imagePlugin = null;
            mounted     = false;

            return Errno.NoError;
        }

        public Errno StatFs(out FileSystemInfo stat)
        {
            stat = null;
            if(!mounted) return Errno.AccessDenied;

            stat = statfs.ShallowCopy();

            return Errno.NoError;
        }
    }
}