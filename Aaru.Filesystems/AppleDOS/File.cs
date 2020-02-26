// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple DOS filesystem plugin.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Helpers;
using FileAttributes = DiscImageChef.CommonTypes.Structs.FileAttributes;

namespace DiscImageChef.Filesystems.AppleDOS
{
    public partial class AppleDOS
    {
        public Errno GetAttributes(string path, out FileAttributes attributes)
        {
            attributes = new FileAttributes();
            if(!mounted) return Errno.AccessDenied;

            string[] pathElements = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if(pathElements.Length != 1) return Errno.NotSupported;

            string filename = pathElements[0].ToUpperInvariant();

            if(!fileCache.ContainsKey(filename)) return Errno.NoSuchFile;

            attributes =  FileAttributes.Extents;
            attributes |= FileAttributes.File;
            if(lockedFiles.Contains(filename)) attributes |= FileAttributes.ReadOnly;

            if(debug && (string.Compare(path, "$",     StringComparison.InvariantCulture) == 0 ||
                         string.Compare(path, "$Boot", StringComparison.InvariantCulture) == 0 ||
                         string.Compare(path, "$Vtoc", StringComparison.InvariantCulture) == 0))
                attributes |= FileAttributes.System;

            return Errno.NoError;
        }

        public Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            if(!mounted) return Errno.AccessDenied;

            string[] pathElements = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if(pathElements.Length != 1) return Errno.NotSupported;

            byte[] file;
            string filename = pathElements[0].ToUpperInvariant();
            if(filename.Length > 30) return Errno.NameTooLong;

            if(debug && (string.Compare(path, "$",     StringComparison.InvariantCulture) == 0 ||
                         string.Compare(path, "$Boot", StringComparison.InvariantCulture) == 0 ||
                         string.Compare(path, "$Vtoc", StringComparison.InvariantCulture) == 0))
                if(string.Compare(path,      "$",     StringComparison.InvariantCulture) == 0) file = catalogBlocks;
                else if(string.Compare(path, "$Vtoc", StringComparison.InvariantCulture) == 0) file = vtocBlocks;
                else file                                                                           = bootBlocks;
            else
            {
                if(!fileCache.TryGetValue(filename, out file))
                {
                    Errno error = CacheFile(filename);
                    if(error != Errno.NoError) return error;

                    if(!fileCache.TryGetValue(filename, out file)) return Errno.InvalidArgument;
                }
            }

            if(offset >= file.Length) return Errno.InvalidArgument;

            if(size + offset >= file.Length) size = file.Length - offset;

            buf = new byte[size];

            Array.Copy(file, offset, buf, 0, size);

            return Errno.NoError;
        }

        public Errno Stat(string path, out FileEntryInfo stat)
        {
            stat = null;
            if(!mounted) return Errno.AccessDenied;

            string[] pathElements = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if(pathElements.Length != 1) return Errno.NotSupported;

            string filename = pathElements[0].ToUpperInvariant();
            if(filename.Length > 30) return Errno.NameTooLong;

            if(!fileCache.ContainsKey(filename)) return Errno.NoSuchFile;

            stat = new FileEntryInfo();

            fileSizeCache.TryGetValue(filename, out int filesize);
            GetAttributes(path, out FileAttributes attrs);

            if(debug && (string.Compare(path, "$",     StringComparison.InvariantCulture) == 0 ||
                         string.Compare(path, "$Boot", StringComparison.InvariantCulture) == 0 ||
                         string.Compare(path, "$Vtoc", StringComparison.InvariantCulture) == 0))
            {
                if(string.Compare(path, "$", StringComparison.InvariantCulture) == 0)
                    stat.Length = catalogBlocks.Length;
                else if(string.Compare(path, "$Boot", StringComparison.InvariantCulture) == 0)
                    stat.Length = bootBlocks.Length;
                else if(string.Compare(path, "$Vtoc", StringComparison.InvariantCulture) == 0)
                    stat.Length = vtocBlocks.Length;

                stat.Blocks = stat.Length / vtoc.bytesPerSector;
            }
            else
            {
                stat.Length = filesize;
                stat.Blocks = stat.Length / vtoc.bytesPerSector;
            }

            stat.Attributes = attrs;
            stat.BlockSize  = vtoc.bytesPerSector;
            stat.Links      = 1;

            return Errno.NoError;
        }

        public Errno MapBlock(string path, long fileBlock, out long deviceBlock)
        {
            deviceBlock = 0;
            // TODO: Not really important.
            return !mounted ? Errno.AccessDenied : Errno.NotImplemented;
        }

        Errno CacheFile(string path)
        {
            string[] pathElements = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if(pathElements.Length != 1) return Errno.NotSupported;

            string filename = pathElements[0].ToUpperInvariant();
            if(filename.Length > 30) return Errno.NameTooLong;

            if(!catalogCache.TryGetValue(filename, out ushort ts)) return Errno.NoSuchFile;

            ulong        lba           = (ulong)(((ts & 0xFF00) >> 8) * sectorsPerTrack + (ts & 0xFF));
            MemoryStream fileMs        = new MemoryStream();
            MemoryStream tsListMs      = new MemoryStream();
            ushort       expectedBlock = 0;

            while(lba != 0)
            {
                usedSectors++;
                byte[] tsSectorB = device.ReadSector(lba);
                if(debug) tsListMs.Write(tsSectorB, 0, tsSectorB.Length);

                // Read the track/sector list sector
                TrackSectorList tsSector = Marshal.ByteArrayToStructureLittleEndian<TrackSectorList>(tsSectorB);

                if(tsSector.sectorOffset > expectedBlock)
                {
                    byte[] hole = new byte[(tsSector.sectorOffset - expectedBlock) * vtoc.bytesPerSector];
                    fileMs.Write(hole, 0, hole.Length);
                    expectedBlock = tsSector.sectorOffset;
                }

                foreach(TrackSectorListEntry entry in tsSector.entries)
                {
                    track1UsedByFiles |= entry.track == 1;
                    track2UsedByFiles |= entry.track == 2;
                    usedSectors++;

                    ulong blockLba = (ulong)(entry.track * sectorsPerTrack + entry.sector);
                    if(blockLba == 0) break;

                    byte[] fileBlock = device.ReadSector(blockLba);
                    fileMs.Write(fileBlock, 0, fileBlock.Length);
                    expectedBlock++;
                }

                lba = (ulong)(tsSector.nextListTrack * sectorsPerTrack + tsSector.nextListSector);
            }

            if(fileCache.ContainsKey(filename)) fileCache.Remove(filename);
            if(extentCache.ContainsKey(filename)) extentCache.Remove(filename);

            fileCache.Add(filename, fileMs.ToArray());
            extentCache.Add(filename, tsListMs.ToArray());

            return Errno.NoError;
        }

        Errno CacheAllFiles()
        {
            fileCache   = new Dictionary<string, byte[]>();
            extentCache = new Dictionary<string, byte[]>();

            foreach(Errno error in catalogCache.Keys.Select(CacheFile).Where(error => error != Errno.NoError))
                return error;

            uint tracksOnBoot = 1;
            if(!track1UsedByFiles) tracksOnBoot++;
            if(!track2UsedByFiles) tracksOnBoot++;

            bootBlocks  =  device.ReadSectors(0, (uint)(tracksOnBoot * sectorsPerTrack));
            usedSectors += (uint)(bootBlocks.Length / vtoc.bytesPerSector);

            return Errno.NoError;
        }
    }
}