// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple DOS filesystem plugin.
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
using System.IO;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Filesystems;

public sealed partial class AppleDOS
{
    /// <inheritdoc />
    public ErrorNumber GetAttributes(string path, out FileAttributes attributes)
    {
        attributes = new FileAttributes();

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        string[] pathElements = path.Split(new[]
        {
            '/'
        }, StringSplitOptions.RemoveEmptyEntries);

        if(pathElements.Length != 1)
            return ErrorNumber.NotSupported;

        string filename = pathElements[0].ToUpperInvariant();

        if(!_fileCache.ContainsKey(filename))
            return ErrorNumber.NoSuchFile;

        attributes =  FileAttributes.Extents;
        attributes |= FileAttributes.File;

        if(_lockedFiles.Contains(filename))
            attributes |= FileAttributes.ReadOnly;

        if(_debug && (string.Compare(path, "$", StringComparison.InvariantCulture)     == 0 ||
                      string.Compare(path, "$Boot", StringComparison.InvariantCulture) == 0 ||
                      string.Compare(path, "$Vtoc", StringComparison.InvariantCulture) == 0))
            attributes |= FileAttributes.System;

        return ErrorNumber.NoError;
    }

    public ErrorNumber OpenFile(string path, out IFileNode node)
    {
        node = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        string[] pathElements = path.Split(new[]
        {
            '/'
        }, StringSplitOptions.RemoveEmptyEntries);

        if(pathElements.Length != 1)
            return ErrorNumber.NotSupported;

        byte[] file;
        string filename = pathElements[0].ToUpperInvariant();

        if(filename.Length > 30)
            return ErrorNumber.NameTooLong;

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
                ErrorNumber error = CacheFile(filename);

                if(error != ErrorNumber.NoError)
                    return error;

                if(!_fileCache.TryGetValue(filename, out file))
                    return ErrorNumber.InvalidArgument;
            }
        }

        node = new AppleDosFileNode
        {
            Path   = path,
            Length = file.Length,
            Offset = 0,
            _cache = file
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(!_mounted)
            return ErrorNumber.AccessDenied;

        if(node is not AppleDosFileNode mynode)
            return ErrorNumber.InvalidArgument;

        mynode._cache = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        if(buffer is null ||
           buffer.Length < length)
            return ErrorNumber.InvalidArgument;

        if(node is not AppleDosFileNode mynode)
            return ErrorNumber.InvalidArgument;

        read = length;

        if(length + mynode.Offset >= mynode.Length)
            read = mynode.Length - mynode.Offset;

        Array.Copy(mynode._cache, mynode.Offset, buffer, 0, read);

        mynode.Offset += read;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        string[] pathElements = path.Split(new[]
        {
            '/'
        }, StringSplitOptions.RemoveEmptyEntries);

        if(pathElements.Length != 1)
            return ErrorNumber.NotSupported;

        string filename = pathElements[0].ToUpperInvariant();

        if(filename.Length > 30)
            return ErrorNumber.NameTooLong;

        if(!_fileCache.ContainsKey(filename))
            return ErrorNumber.NoSuchFile;

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

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber MapBlock(string path, long fileBlock, out long deviceBlock)
    {
        deviceBlock = 0;

        // TODO: Not really important.
        return !_mounted ? ErrorNumber.AccessDenied : ErrorNumber.NotImplemented;
    }

    ErrorNumber CacheFile(string path)
    {
        string[] pathElements = path.Split(new[]
        {
            '/'
        }, StringSplitOptions.RemoveEmptyEntries);

        if(pathElements.Length != 1)
            return ErrorNumber.NotSupported;

        string filename = pathElements[0].ToUpperInvariant();

        if(filename.Length > 30)
            return ErrorNumber.NameTooLong;

        if(!_catalogCache.TryGetValue(filename, out ushort ts))
            return ErrorNumber.NoSuchFile;

        ulong  lba           = (ulong)((((ts & 0xFF00) >> 8) * _sectorsPerTrack) + (ts & 0xFF));
        var    fileMs        = new MemoryStream();
        var    tsListMs      = new MemoryStream();
        ushort expectedBlock = 0;

        while(lba != 0)
        {
            _usedSectors++;
            ErrorNumber errno = _device.ReadSector(lba, out byte[] tsSectorB);

            if(errno != ErrorNumber.NoError)
                return errno;

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

                errno = _device.ReadSector(blockLba, out byte[] fileBlock);

                if(errno != ErrorNumber.NoError)
                    return errno;

                fileMs.Write(fileBlock, 0, fileBlock.Length);
                expectedBlock++;
            }

            lba = (ulong)((tsSector.nextListTrack * _sectorsPerTrack) + tsSector.nextListSector);
        }

        if(_fileCache.ContainsKey(filename))
            _fileCache.Remove(filename);

        if(_extentCache.ContainsKey(filename))
            _extentCache.Remove(filename);

        if(_fileSizeCache.ContainsKey(filename))
            _fileSizeCache.Remove(filename);

        _fileCache.Add(filename, fileMs.ToArray());
        _extentCache.Add(filename, tsListMs.ToArray());
        _fileSizeCache.Add(filename, (int)fileMs.Length);

        return ErrorNumber.NoError;
    }

    ErrorNumber CacheAllFiles()
    {
        _fileCache   = new Dictionary<string, byte[]>();
        _extentCache = new Dictionary<string, byte[]>();

        foreach(ErrorNumber error in _catalogCache.Keys.Select(CacheFile).Where(error => error != ErrorNumber.NoError))
            return error;

        uint tracksOnBoot = 1;

        if(!_track1UsedByFiles)
            tracksOnBoot++;

        if(!_track2UsedByFiles)
            tracksOnBoot++;

        ErrorNumber errno = _device.ReadSectors(0, (uint)(tracksOnBoot * _sectorsPerTrack), out _bootBlocks);

        if(errno != ErrorNumber.NoError)
            return errno;

        _usedSectors += (uint)(_bootBlocks.Length / _vtoc.bytesPerSector);

        return ErrorNumber.NoError;
    }
}