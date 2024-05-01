// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CBM.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commodore file system plugin.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Helpers;
using Claunia.Encoding;
using Encoding = System.Text.Encoding;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;
using FileSystemInfo = Aaru.CommonTypes.Structs.FileSystemInfo;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the filesystem used in 8-bit Commodore microcomputers</summary>
public sealed partial class CBM
{
#region IReadOnlyFilesystem Members

    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options,     string    @namespace)
    {
        if(partition.Start > 0) return ErrorNumber.InvalidArgument;

        if(imagePlugin.Info.SectorSize != 256) return ErrorNumber.InvalidArgument;

        if(imagePlugin.Info.Sectors != 683  &&
           imagePlugin.Info.Sectors != 768  &&
           imagePlugin.Info.Sectors != 1366 &&
           imagePlugin.Info.Sectors != 3200)
            return ErrorNumber.InvalidArgument;

        options ??= GetDefaultOptions();

        if(options.TryGetValue("debug", out string debugString)) bool.TryParse(debugString, out _debug);

        encoding = new PETSCII();
        byte[] sector;
        ulong  rootLba;
        string volumeName = null;

        Metadata = new FileSystem
        {
            Type        = FS_TYPE,
            Clusters    = imagePlugin.Info.Sectors,
            ClusterSize = 256
        };

        uint serial;

        // Commodore 1581
        if(imagePlugin.Info.Sectors == 3200)
        {
            ErrorNumber errno = imagePlugin.ReadSector(1560, out _diskHeader);

            if(errno != ErrorNumber.NoError) return errno;

            Header cbmHdr = Marshal.ByteArrayToStructureBigEndian<Header>(_diskHeader);

            if(cbmHdr.diskDosVersion != 0x44 || cbmHdr is not { dosVersion: 0x33, diskVersion: 0x44 })
                return ErrorNumber.InvalidArgument;

            _bam = new byte[512];

            // Got to first BAM sector
            errno = imagePlugin.ReadSector(1561, out sector);

            if(errno != ErrorNumber.NoError) return errno;

            Array.Copy(sector, 0, _bam, 0, 256);

            if(_bam[0] > 0)
            {
                // Got to next (and last) BAM sector
                errno = imagePlugin.ReadSector((ulong)((_bam[0] - 1) * 40), out sector);

                if(errno != ErrorNumber.NoError) return errno;

                Array.Copy(sector, 0, _bam, 256, 256);
            }

            if(cbmHdr.directoryTrack == 0) return ErrorNumber.InvalidArgument;

            rootLba               = CbmChsToLba(cbmHdr.directoryTrack, cbmHdr.directorySector, true);
            serial                = cbmHdr.diskId;
            Metadata.VolumeName   = StringHandlers.CToString(cbmHdr.name, encoding);
            Metadata.VolumeSerial = $"{cbmHdr.diskId}";
            _is1581               = true;
        }
        else
        {
            ErrorNumber errno = imagePlugin.ReadSector(357, out _bam);

            if(errno != ErrorNumber.NoError) return errno;

            BAM cbmBam = Marshal.ByteArrayToStructureBigEndian<BAM>(_bam);

            if(cbmBam is not ({ dosVersion: 0x41, doubleSided   : 0x00 or 0x80 }
                          and { unused1   : 0x00, directoryTrack: 0x12 }))
                return ErrorNumber.InvalidArgument;

            rootLba = CbmChsToLba(cbmBam.directoryTrack, cbmBam.directorySector, false);
            serial  = cbmBam.diskId;

            Metadata.VolumeName   = StringHandlers.CToString(cbmBam.name, encoding);
            Metadata.VolumeSerial = $"{cbmBam.diskId}";
        }

        if(rootLba >= imagePlugin.Info.Sectors) return ErrorNumber.IllegalSeek;

        ulong nextLba                  = rootLba;
        var   rootMs                   = new MemoryStream();
        var   relativeFileWarningShown = false;

        do
        {
            ErrorNumber errno = imagePlugin.ReadSector(nextLba, out sector);

            if(errno != ErrorNumber.NoError) return errno;

            rootMs.Write(sector, 0, 256);

            if(sector[0] == 0) break;

            nextLba = CbmChsToLba(sector[0], sector[1], _is1581);
        } while(nextLba > 0);

        _root = rootMs.ToArray();

        _statfs = new FileSystemInfo
        {
            Blocks         = imagePlugin.Info.Sectors,
            FilenameLength = 14,
            Files          = 0,
            FreeBlocks =
                imagePlugin.Info.Sectors -
                (ulong)(_diskHeader?.Length ?? 0 / 256 - _bam.Length / 256 - _root.Length / 256),
            FreeFiles = (ulong)(_root.Length / 32),
            Id = new FileSystemId
            {
                Serial32 = serial,
                IsInt    = true
            },
            PluginId = Id,
            Type     = "CBMFS"
        };

        // As this filesystem comes in (by nowadays standards) very small sizes, we can cache all files
        _cache = new Dictionary<string, CachedFile>();
        var   offset = 0;
        ulong fileId = 0;

        if(_debug)
        {
            // Root
            _cache.Add("$",
                       new CachedFile
                       {
                           attributes = FileAttributes.Directory | FileAttributes.Hidden | FileAttributes.System,
                           length     = (ulong)_root.Length,
                           data       = _root,
                           blocks     = _root.Length / 256,
                           id         = fileId++
                       });

            // BAM
            _cache.Add("$BAM",
                       new CachedFile
                       {
                           attributes = FileAttributes.File | FileAttributes.Hidden | FileAttributes.System,
                           length     = (ulong)_bam.Length,
                           data       = _bam,
                           blocks     = _bam.Length / 256,
                           id         = fileId++
                       });

            _statfs.Files += 2;

            // 1581 disk header
            if(_diskHeader != null)
            {
                _cache.Add("$DISK_HEADER",
                           new CachedFile
                           {
                               attributes = FileAttributes.File | FileAttributes.Hidden | FileAttributes.System,
                               length     = (ulong)_diskHeader.Length,
                               data       = _diskHeader,
                               blocks     = _diskHeader.Length / 256,
                               id         = fileId++
                           });

                _statfs.Files++;
            }
        }

        while(offset < _root.Length)
        {
            DirectoryEntry dirEntry = Marshal.ByteArrayToStructureBigEndian<DirectoryEntry>(_root, offset, 32);

            if(dirEntry.fileType == 0)
            {
                offset += 32;

                continue;
            }

            _statfs.Files++;
            _statfs.FreeFiles--;

            for(var i = 0; i < dirEntry.name.Length; i++)
            {
                if(dirEntry.name[i] == 0xA0) dirEntry.name[i] = 0;
            }

            string name = StringHandlers.CToString(dirEntry.name, encoding);

            if((dirEntry.fileType & 0x07) == 4 && !relativeFileWarningShown)
            {
                AaruConsole.WriteLine(Localization.CBM_Mount_REL_file_warning);
                relativeFileWarningShown = true;
            }

            var data = new MemoryStream();

            nextLba = CbmChsToLba(dirEntry.firstFileBlockTrack, dirEntry.firstFileBlockSector, _is1581);

            _statfs.FreeBlocks -= (ulong)dirEntry.blocks;

            while(dirEntry.blocks > 0)
            {
                if(dirEntry.firstFileBlockTrack == 0) break;

                ErrorNumber errno = imagePlugin.ReadSector(nextLba, out sector);

                if(errno != ErrorNumber.NoError) break;

                byte toRead = sector[0] == 0 ? sector[1] : (byte)254;
                if(toRead == 255) toRead--;

                data.Write(sector, 2, toRead);

                if(sector[0] == 0) break;

                nextLba = CbmChsToLba(sector[0], sector[1], _is1581);
            }

            FileAttributes attributes = FileAttributes.File;

            if((dirEntry.fileType & 0x80) != 0x80) attributes |= FileAttributes.Open;
            if((dirEntry.fileType & 0x40) > 0) attributes     |= FileAttributes.ReadOnly;
            if((dirEntry.fileType & 7)    == 2) attributes    |= FileAttributes.Executable;

            _cache[name] = new CachedFile
            {
                attributes = attributes,
                length     = (ulong)data.Length,
                data       = data.ToArray(),
                blocks     = dirEntry.blocks,
                id         = fileId++
            };

            offset += 32;
        }

        _mounted = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Unmount()
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        _mounted = false;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber StatFs(out FileSystemInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        stat = _statfs.ShallowCopy();

        return ErrorNumber.NoError;
    }

#endregion
}