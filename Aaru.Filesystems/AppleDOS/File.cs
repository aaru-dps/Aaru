// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Filesystems
{
    public sealed partial class AppleDOS
    {
        /// <inheritdoc />
        public Errno GetAttributes(string path, out FileAttributes attributes)
        {
            attributes = new FileAttributes();

            if(!_mounted)
                return Errno.AccessDenied;

            string[] pathElements = path.Split(new[]
            {
                '/'
            }, StringSplitOptions.RemoveEmptyEntries);

            if(pathElements.Length != 1)
                return Errno.NotSupported;

            string filename = pathElements[0].ToUpperInvariant();

            if(!_fileCache.ContainsKey(filename))
                return Errno.NoSuchFile;

            attributes =  FileAttributes.Extents;
            attributes |= FileAttributes.File;

            if(_lockedFiles.Contains(filename))
                attributes |= FileAttributes.ReadOnly;

            if(_debug && (string.Compare(path, "$", StringComparison.InvariantCulture)     == 0 ||
                          string.Compare(path, "$Boot", StringComparison.InvariantCulture) == 0 ||
                          string.Compare(path, "$Vtoc", StringComparison.InvariantCulture) == 0))
                attributes |= FileAttributes.System;

            return Errno.NoError;
        }

        /// <inheritdoc />
        public Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            if(!_mounted)
                return Errno.AccessDenied;

            string[] pathElements = path.Split(new[]
            {
                '/'
            }, StringSplitOptions.RemoveEmptyEntries);

            if(pathElements.Length != 1)
                return Errno.NotSupported;

            byte[] file;
            string filename = pathElements[0].ToUpperInvariant();

            if(filename.Length > 30)
                return Errno.NameTooLong;

            if(_debug && (string.Compare(path, "$", StringComparison.InvariantCulture)     == 0 ||
                          string.Compare(path, "$Boot", StringComparison.InvariantCulture) == 0 ||
                          string.Compare(path, "$Vtoc", StringComparison.InvariantCulture) == 0))
                if(string.Compare(path, "$", StringComparison.InvariantCulture) == 0)
                    file = _catalogBlocks;
                else if(string.Compare(path, "$Vtoc", StringComparison.InvariantCulture) == 0)
                    file = _vtocBlocks;
                else
                    file = _bootBlocks;
            else
            {
                if(!_fileCache.TryGetValue(filename, out file))
                {
                    Errno error = CacheFile(filename);

                    if(error != Errno.NoError)
                        return error;

                    if(!_fileCache.TryGetValue(filename, out file))
                        return Errno.InvalidArgument;
                }
            }

            if(offset >= file.Length)
                return Errno.InvalidArgument;

            if(size + offset >= file.Length)
                size = file.Length - offset;

            buf = new byte[size];

            Array.Copy(file, offset, buf, 0, size);

            return Errno.NoError;
        }

        /// <inheritdoc />
        public Errno Stat(string path, out FileEntryInfo stat)
        {
            stat = null;

            if(!_mounted)
                return Errno.AccessDenied;

            string[] pathElements = path.Split(new[]
            {
                '/'
            }, StringSplitOptions.RemoveEmptyEntries);

            if(pathElements.Length != 1)
                return Errno.NotSupported;

            string filename = pathElements[0].ToUpperInvariant();

            if(filename.Length > 30)
                return Errno.NameTooLong;

            if(!_fileCache.ContainsKey(filename))
                return Errno.NoSuchFile;

            stat = new FileEntryInfo();

            _fileSizeCache.TryGetValue(filename, out int fileSize);
            GetAttributes(path, out FileAttributes attrs);

            if(_debug && (string.Compare(path, "$", StringComparison.InvariantCulture)     == 0 ||
                          string.Compare(path, "$Boot", StringComparison.InvariantCulture) == 0 ||
                          string.Compare(path, "$Vtoc", StringComparison.InvariantCulture) == 0))
            {
                if(string.Compare(path, "$", StringComparison.InvariantCulture) == 0)
                    stat.Length = _catalogBlocks.Length;
                else if(string.Compare(path, "$Boot", StringComparison.InvariantCulture) == 0)
                    stat.Length = _bootBlocks.Length;
                else if(string.Compare(path, "$Vtoc", StringComparison.InvariantCulture) == 0)
                    stat.Length = _vtocBlocks.Length;

                stat.Blocks = stat.Length / _vtoc.bytesPerSector;
            }
            else
            {
                stat.Length = fileSize;
                stat.Blocks = stat.Length / _vtoc.bytesPerSector;
            }

            stat.Attributes = attrs;
            stat.BlockSize  = _vtoc.bytesPerSector;
            stat.Links      = 1;

            return Errno.NoError;
        }

        /// <inheritdoc />
        public Errno MapBlock(string path, long fileBlock, out long deviceBlock)
        {
            deviceBlock = 0;

            // TODO: Not really important.
            return !_mounted ? Errno.AccessDenied : Errno.NotImplemented;
        }

        Errno CacheFile(string path)
        {
            string[] pathElements = path.Split(new[]
            {
                '/'
            }, StringSplitOptions.RemoveEmptyEntries);

            if(pathElements.Length != 1)
                return Errno.NotSupported;

            string filename = pathElements[0].ToUpperInvariant();

            if(filename.Length > 30)
                return Errno.NameTooLong;

            if(!_catalogCache.TryGetValue(filename, out ushort ts))
                return Errno.NoSuchFile;

            ulong  lba           = (ulong)((((ts & 0xFF00) >> 8) * _sectorsPerTrack) + (ts & 0xFF));
            var    fileMs        = new MemoryStream();
            var    tsListMs      = new MemoryStream();
            ushort expectedBlock = 0;

            while(lba != 0)
            {
                _usedSectors++;
                byte[] tsSectorB = _device.ReadSector(lba);

                if(_debug)
                    tsListMs.Write(tsSectorB, 0, tsSectorB.Length);

                // Read the track/sector list sector
                TrackSectorList tsSector = Marshal.ByteArrayToStructureLittleEndian<TrackSectorList>(tsSectorB);

                if(tsSector.sectorOffset > expectedBlock)
                {
                    byte[] hole = new byte[(tsSector.sectorOffset - expectedBlock) * _vtoc.bytesPerSector];
                    fileMs.Write(hole, 0, hole.Length);
                    expectedBlock = tsSector.sectorOffset;
                }

                foreach(TrackSectorListEntry entry in tsSector.entries)
                {
                    _track1UsedByFiles |= entry.track == 1;
                    _track2UsedByFiles |= entry.track == 2;
                    _usedSectors++;

                    ulong blockLba = (ulong)((entry.track * _sectorsPerTrack) + entry.sector);

                    if(blockLba == 0)
                        break;

                    byte[] fileBlock = _device.ReadSector(blockLba);
                    fileMs.Write(fileBlock, 0, fileBlock.Length);
                    expectedBlock++;
                }

                lba = (ulong)((tsSector.nextListTrack * _sectorsPerTrack) + tsSector.nextListSector);
            }

            if(_fileCache.ContainsKey(filename))
                _fileCache.Remove(filename);

            if(_extentCache.ContainsKey(filename))
                _extentCache.Remove(filename);

            _fileCache.Add(filename, fileMs.ToArray());
            _extentCache.Add(filename, tsListMs.ToArray());

            return Errno.NoError;
        }

        Errno CacheAllFiles()
        {
            _fileCache   = new Dictionary<string, byte[]>();
            _extentCache = new Dictionary<string, byte[]>();

            foreach(Errno error in _catalogCache.Keys.Select(CacheFile).Where(error => error != Errno.NoError))
                return error;

            uint tracksOnBoot = 1;

            if(!_track1UsedByFiles)
                tracksOnBoot++;

            if(!_track2UsedByFiles)
                tracksOnBoot++;

            _bootBlocks  =  _device.ReadSectors(0, (uint)(tracksOnBoot * _sectorsPerTrack));
            _usedSectors += (uint)(_bootBlocks.Length / _vtoc.bytesPerSector);

            return Errno.NoError;
        }
    }
}