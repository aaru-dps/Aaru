// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft FAT filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Methods to handle files.
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
using System.IO;
using System.Linq;
using DiscImageChef.CommonTypes.Structs;
using FileAttributes = DiscImageChef.CommonTypes.Structs.FileAttributes;

namespace DiscImageChef.Filesystems.FAT
{
    public partial class FAT
    {
        public Errno MapBlock(string path, long fileBlock, out long deviceBlock)
        {
            deviceBlock = 0;
            if(!mounted) return Errno.AccessDenied;

            Errno err = Stat(path, out FileEntryInfo stat);

            if(err != Errno.NoError) return err;

            if(stat.Attributes.HasFlag(FileAttributes.Directory) && !debug) return Errno.IsDirectory;

            uint[] clusters = GetClusters((uint)stat.Inode);

            if(fileBlock >= clusters.Length) return Errno.InvalidArgument;

            deviceBlock = (long)(firstClusterSector + (clusters[fileBlock] - 2) * sectorsPerCluster);

            return Errno.NoError;
        }

        public Errno GetAttributes(string path, out FileAttributes attributes)
        {
            attributes = new FileAttributes();
            if(!mounted) return Errno.AccessDenied;

            Errno err = Stat(path, out FileEntryInfo stat);

            if(err != Errno.NoError) return err;

            attributes = stat.Attributes;

            return Errno.NoError;
        }

        public Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            if(!mounted) return Errno.AccessDenied;

            Errno err = Stat(path, out FileEntryInfo stat);

            if(err != Errno.NoError) return err;

            if(stat.Attributes.HasFlag(FileAttributes.Directory) && !debug) return Errno.IsDirectory;

            if(offset >= stat.Length) return Errno.InvalidArgument;

            if(size + offset >= stat.Length) size = stat.Length - offset;

            uint[] clusters = GetClusters((uint)stat.Inode);

            long firstCluster    = offset                   / bytesPerCluster;
            long offsetInCluster = offset                   % bytesPerCluster;
            long sizeInClusters  = (size + offsetInCluster) / bytesPerCluster;
            if((size + offsetInCluster) % bytesPerCluster > 0) sizeInClusters++;

            MemoryStream ms = new MemoryStream();

            for(int i = 0; i < sizeInClusters; i++)
            {
                if(i + firstCluster >= clusters.Length) return Errno.InvalidArgument;

                byte[] buffer =
                    image.ReadSectors(firstClusterSector + (clusters[i + firstCluster] - 2) * sectorsPerCluster,
                                      sectorsPerCluster);

                ms.Write(buffer, 0, buffer.Length);
            }

            ms.Position = offsetInCluster;
            buf         = new byte[size];
            ms.Read(buf, 0, (int)size);

            return Errno.NoError;
        }

        public Errno Stat(string path, out FileEntryInfo stat)
        {
            stat = null;
            if(!mounted) return Errno.AccessDenied;

            Errno err = GetFileEntry(path, out DirectoryEntry entry);
            if(err != Errno.NoError) return err;

            stat = new FileEntryInfo
            {
                Attributes    = new FileAttributes(),
                Blocks        = entry.size / bytesPerCluster,
                BlockSize     = bytesPerCluster,
                Length        = entry.size,
                Inode         = entry.start_cluster,
                Links         = 1,
                CreationTime  = DateHandlers.DosToDateTime(entry.cdate, entry.ctime),
                LastWriteTime = DateHandlers.DosToDateTime(entry.mdate, entry.mtime)
            };

            stat.CreationTime = stat.CreationTime?.AddMilliseconds(entry.ctime_ms * 10);

            if(entry.size % bytesPerCluster > 0) stat.Blocks++;

            if(entry.attributes.HasFlag(FatAttributes.Subdirectory))
            {
                stat.Attributes |= FileAttributes.Directory;
                stat.Blocks     =  GetClusters(entry.start_cluster).Length;
                stat.Length     =  stat.Blocks * stat.BlockSize;
            }

            if(entry.attributes.HasFlag(FatAttributes.ReadOnly)) stat.Attributes |= FileAttributes.ReadOnly;
            if(entry.attributes.HasFlag(FatAttributes.Hidden)) stat.Attributes   |= FileAttributes.Hidden;
            if(entry.attributes.HasFlag(FatAttributes.System)) stat.Attributes   |= FileAttributes.System;
            if(entry.attributes.HasFlag(FatAttributes.Archive)) stat.Attributes  |= FileAttributes.Archive;
            if(entry.attributes.HasFlag(FatAttributes.Device)) stat.Attributes   |= FileAttributes.Device;

            return Errno.NoError;
        }

        uint[] GetClusters(uint startCluster)
        {
            if(startCluster == 0) return null;

            if(startCluster >= XmlFsType.Clusters) return null;

            List<uint> clusters = new List<uint>();

            uint nextCluster = startCluster;

            ulong nextSector = nextCluster / fatEntriesPerSector + fatFirstSector + (useFirstFat ? 0 : sectorsPerFat);
            int   nextEntry  = (int)(nextCluster % fatEntriesPerSector);

            ulong  currentSector = nextSector;
            byte[] fatData       = image.ReadSector(currentSector);

            if(fat32)
                while((nextCluster & FAT32_MASK) > 0 && (nextCluster & FAT32_MASK) <= FAT32_FORMATTED)
                {
                    clusters.Add(nextCluster);

                    if(currentSector != nextSector)
                    {
                        fatData       = image.ReadSector(nextSector);
                        currentSector = nextSector;
                    }

                    nextCluster = BitConverter.ToUInt32(fatData, nextEntry * 4);
                    nextSector = nextCluster / fatEntriesPerSector + fatFirstSector +
                                  (useFirstFat ? 0 : sectorsPerFat);
                    nextEntry = (int)(nextCluster % fatEntriesPerSector);
                }
            else if(fat16)
                while(nextCluster > 0 && nextCluster <= FAT16_FORMATTED)
                {
                    clusters.Add(nextCluster);
                    nextCluster = fatEntries[nextCluster];
                }
            else
                while(nextCluster > 0 && nextCluster <= FAT12_FORMATTED)
                {
                    clusters.Add(nextCluster);
                    nextCluster = fatEntries[nextCluster];
                }

            return clusters.ToArray();
        }

        Errno GetFileEntry(string path, out DirectoryEntry entry)
        {
            entry = new DirectoryEntry();

            string cutPath =
                path.StartsWith("/") ? path.Substring(1).ToLower(cultureInfo) : path.ToLower(cultureInfo);
            string[] pieces = cutPath.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

            if(pieces.Length == 0) return Errno.InvalidArgument;

            string parentPath = string.Join("/", pieces, 0, pieces.Length - 1);

            Errno err = ReadDir(parentPath, out _);

            if(err != Errno.NoError) return err;

            Dictionary<string, DirectoryEntry> parent;

            if(pieces.Length == 1) parent = rootDirectoryCache;
            else if(!directoryCache.TryGetValue(parentPath, out parent)) return Errno.InvalidArgument;

            KeyValuePair<string, DirectoryEntry> dirent =
                parent.FirstOrDefault(t => t.Key.ToLower(cultureInfo) == pieces[pieces.Length - 1]);

            if(string.IsNullOrEmpty(dirent.Key)) return Errno.NoSuchFile;

            entry = dirent.Value;
            return Errno.NoError;
        }

        byte LfnChecksum(byte[] name, byte[] extension)
        {
            byte sum = 0;

            for(int i = 0; i < 8; i++) sum = (byte)(((sum & 1) << 7) + (sum >> 1) + name[i]);
            for(int i = 0; i < 3; i++) sum = (byte)(((sum & 1) << 7) + (sum >> 1) + extension[i]);

            return sum;
        }
    }
}