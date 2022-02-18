// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems
{
    public sealed partial class XboxFatPlugin
    {
        /// <inheritdoc />
        public ErrorNumber Mount(IMediaImage imagePlugin, Partition partition, Encoding encoding,
                                 Dictionary<string, string> options, string @namespace)
        {
            Encoding      = Encoding.GetEncoding("iso-8859-15");
            _littleEndian = true;

            options ??= GetDefaultOptions();

            if(options.TryGetValue("debug", out string debugString))
                bool.TryParse(debugString, out _debug);

            if(imagePlugin.Info.SectorSize < 512)
                return ErrorNumber.InvalidArgument;

            AaruConsole.DebugWriteLine("Xbox FAT plugin", "Reading superblock");

            ErrorNumber errno = imagePlugin.ReadSector(partition.Start, out byte[] sector);

            if(errno != ErrorNumber.NoError)
                return errno;

            _superblock = Marshal.ByteArrayToStructureLittleEndian<Superblock>(sector);

            if(_superblock.magic == FATX_CIGAM)
            {
                _superblock   = Marshal.ByteArrayToStructureBigEndian<Superblock>(sector);
                _littleEndian = false;
            }

            if(_superblock.magic != FATX_MAGIC)
                return ErrorNumber.InvalidArgument;

            AaruConsole.DebugWriteLine("Xbox FAT plugin",
                                       _littleEndian ? "Filesystem is little endian" : "Filesystem is big endian");

            int logicalSectorsPerPhysicalSectors = partition.Offset == 0 && _littleEndian ? 8 : 1;

            AaruConsole.DebugWriteLine("Xbox FAT plugin", "logicalSectorsPerPhysicalSectors = {0}",
                                       logicalSectorsPerPhysicalSectors);

            string volumeLabel = StringHandlers.CToString(_superblock.volumeLabel,
                                                          !_littleEndian ? Encoding.BigEndianUnicode : Encoding.Unicode,
                                                          true);

            XmlFsType = new FileSystemType
            {
                Type = "FATX filesystem",
                ClusterSize = (uint)(_superblock.sectorsPerCluster * logicalSectorsPerPhysicalSectors *
                                     imagePlugin.Info.SectorSize),
                VolumeName   = volumeLabel,
                VolumeSerial = $"{_superblock.id:X8}"
            };

            XmlFsType.Clusters = (partition.End - partition.Start + 1) * imagePlugin.Info.SectorSize /
                                 XmlFsType.ClusterSize;

            _statfs = new FileSystemInfo
            {
                Blocks         = XmlFsType.Clusters,
                FilenameLength = MAX_FILENAME,
                Files          = 0, // Requires traversing all directories
                FreeFiles      = 0,
                Id =
                {
                    IsInt    = true,
                    Serial32 = _superblock.magic
                },
                PluginId   = Id,
                Type       = _littleEndian ? "Xbox FAT" : "Xbox 360 FAT",
                FreeBlocks = 0 // Requires traversing the FAT
            };

            AaruConsole.DebugWriteLine("Xbox FAT plugin", "XmlFsType.ClusterSize: {0}", XmlFsType.ClusterSize);
            AaruConsole.DebugWriteLine("Xbox FAT plugin", "XmlFsType.VolumeName: {0}", XmlFsType.VolumeName);
            AaruConsole.DebugWriteLine("Xbox FAT plugin", "XmlFsType.VolumeSerial: {0}", XmlFsType.VolumeSerial);
            AaruConsole.DebugWriteLine("Xbox FAT plugin", "stat.Blocks: {0}", _statfs.Blocks);
            AaruConsole.DebugWriteLine("Xbox FAT plugin", "stat.FilenameLength: {0}", _statfs.FilenameLength);
            AaruConsole.DebugWriteLine("Xbox FAT plugin", "stat.Id: {0}", _statfs.Id.Serial32);
            AaruConsole.DebugWriteLine("Xbox FAT plugin", "stat.Type: {0}", _statfs.Type);

            byte[] buffer;
            _fatStartSector = (FAT_START / imagePlugin.Info.SectorSize) + partition.Start;
            uint fatSize;

            AaruConsole.DebugWriteLine("Xbox FAT plugin", "fatStartSector: {0}", _fatStartSector);

            if(_statfs.Blocks > MAX_XFAT16_CLUSTERS)
            {
                AaruConsole.DebugWriteLine("Xbox FAT plugin", "Reading FAT32");

                fatSize = (uint)((_statfs.Blocks + 1) * sizeof(uint) / imagePlugin.Info.SectorSize);

                if((uint)((_statfs.Blocks + 1) * sizeof(uint) % imagePlugin.Info.SectorSize) > 0)
                    fatSize++;

                long fatClusters = fatSize * imagePlugin.Info.SectorSize / 4096;

                if(fatSize * imagePlugin.Info.SectorSize % 4096 > 0)
                    fatClusters++;

                fatSize = (uint)(fatClusters * 4096 / imagePlugin.Info.SectorSize);

                AaruConsole.DebugWriteLine("Xbox FAT plugin", "FAT is {0} sectors", fatSize);

                errno = imagePlugin.ReadSectors(_fatStartSector, fatSize, out buffer);

                if(errno != ErrorNumber.NoError)
                    return errno;

                AaruConsole.DebugWriteLine("Xbox FAT plugin", "Casting FAT");
                _fat32 = MemoryMarshal.Cast<byte, uint>(buffer).ToArray();

                if(!_littleEndian)
                    for(int i = 0; i < _fat32.Length; i++)
                        _fat32[i] = Swapping.Swap(_fat32[i]);

                AaruConsole.DebugWriteLine("Xbox FAT plugin", "fat32[0] == FATX32_ID = {0}", _fat32[0] == FATX32_ID);

                if(_fat32[0] != FATX32_ID)
                    return ErrorNumber.InvalidArgument;
            }
            else
            {
                AaruConsole.DebugWriteLine("Xbox FAT plugin", "Reading FAT16");

                fatSize = (uint)((_statfs.Blocks + 1) * sizeof(ushort) / imagePlugin.Info.SectorSize);

                if((uint)((_statfs.Blocks + 1) * sizeof(ushort) % imagePlugin.Info.SectorSize) > 0)
                    fatSize++;

                long fatClusters = fatSize * imagePlugin.Info.SectorSize / 4096;

                if(fatSize * imagePlugin.Info.SectorSize % 4096 > 0)
                    fatClusters++;

                fatSize = (uint)(fatClusters * 4096 / imagePlugin.Info.SectorSize);

                AaruConsole.DebugWriteLine("Xbox FAT plugin", "FAT is {0} sectors", fatSize);

                errno = imagePlugin.ReadSectors(_fatStartSector, fatSize, out buffer);

                if(errno != ErrorNumber.NoError)
                    return errno;

                AaruConsole.DebugWriteLine("Xbox FAT plugin", "Casting FAT");
                _fat16 = MemoryMarshal.Cast<byte, ushort>(buffer).ToArray();

                if(!_littleEndian)
                    for(int i = 0; i < _fat16.Length; i++)
                        _fat16[i] = Swapping.Swap(_fat16[i]);

                AaruConsole.DebugWriteLine("Xbox FAT plugin", "fat16[0] == FATX16_ID = {0}", _fat16[0] == FATX16_ID);

                if(_fat16[0] != FATX16_ID)
                    return ErrorNumber.InvalidArgument;
            }

            _sectorsPerCluster  = (uint)(_superblock.sectorsPerCluster * logicalSectorsPerPhysicalSectors);
            _imagePlugin        = imagePlugin;
            _firstClusterSector = _fatStartSector + fatSize;
            _bytesPerCluster    = _sectorsPerCluster * imagePlugin.Info.SectorSize;

            AaruConsole.DebugWriteLine("Xbox FAT plugin", "sectorsPerCluster = {0}", _sectorsPerCluster);
            AaruConsole.DebugWriteLine("Xbox FAT plugin", "bytesPerCluster = {0}", _bytesPerCluster);
            AaruConsole.DebugWriteLine("Xbox FAT plugin", "firstClusterSector = {0}", _firstClusterSector);

            uint[] rootDirectoryClusters = GetClusters(_superblock.rootDirectoryCluster);

            if(rootDirectoryClusters is null)
                return ErrorNumber.InvalidArgument;

            byte[] rootDirectoryBuffer = new byte[_bytesPerCluster * rootDirectoryClusters.Length];

            AaruConsole.DebugWriteLine("Xbox FAT plugin", "Reading root directory");

            for(int i = 0; i < rootDirectoryClusters.Length; i++)
            {
                errno =
                    imagePlugin.ReadSectors(_firstClusterSector + ((rootDirectoryClusters[i] - 1) * _sectorsPerCluster),
                                            _sectorsPerCluster, out buffer);

                if(errno != ErrorNumber.NoError)
                    return errno;

                Array.Copy(buffer, 0, rootDirectoryBuffer, i * _bytesPerCluster, _bytesPerCluster);
            }

            _rootDirectory = new Dictionary<string, DirectoryEntry>();

            int pos = 0;

            while(pos < rootDirectoryBuffer.Length)
            {
                DirectoryEntry entry = _littleEndian
                                           ? Marshal.
                                               ByteArrayToStructureLittleEndian<
                                                   DirectoryEntry>(rootDirectoryBuffer, pos,
                                                                   Marshal.SizeOf<DirectoryEntry>())
                                           : Marshal.ByteArrayToStructureBigEndian<DirectoryEntry>(rootDirectoryBuffer,
                                               pos, Marshal.SizeOf<DirectoryEntry>());

                pos += Marshal.SizeOf<DirectoryEntry>();

                if(entry.filenameSize == UNUSED_DIRENTRY ||
                   entry.filenameSize == FINISHED_DIRENTRY)
                    break;

                if(entry.filenameSize == DELETED_DIRENTRY ||
                   entry.filenameSize > MAX_FILENAME)
                    continue;

                string filename = Encoding.GetString(entry.filename, 0, entry.filenameSize);

                _rootDirectory.Add(filename, entry);
            }

            _cultureInfo    = new CultureInfo("en-US", false);
            _directoryCache = new Dictionary<string, Dictionary<string, DirectoryEntry>>();
            _mounted        = true;

            return ErrorNumber.NoError;
        }

        /// <inheritdoc />
        public ErrorNumber Unmount()
        {
            if(!_mounted)
                return ErrorNumber.AccessDenied;

            _fat16       = null;
            _fat32       = null;
            _imagePlugin = null;
            _mounted     = false;

            return ErrorNumber.NoError;
        }

        /// <inheritdoc />
        public ErrorNumber StatFs(out FileSystemInfo stat)
        {
            stat = null;

            if(!_mounted)
                return ErrorNumber.AccessDenied;

            stat = _statfs.ShallowCopy();

            return ErrorNumber.NoError;
        }
    }
}