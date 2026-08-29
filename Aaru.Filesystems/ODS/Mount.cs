// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Mount.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Files-11 On-Disk Structure plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Mounts the Files-11 On-Disk Structure filesystem.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Aaru.Logging;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

public sealed partial class ODS
{
    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options,     string    @namespace)
    {
        _encoding = encoding ?? Encoding.GetEncoding("iso-8859-1");

        options ??= GetDefaultOptions();

        if(options.TryGetValue("debug", out string debugString)) bool.TryParse(debugString, out _debug);

        // Handle namespace selection - default to "default" if not specified
        @namespace ??= NAMESPACE_DEFAULT;

        switch(@namespace.ToLowerInvariant())
        {
            case NAMESPACE_DEFAULT:
            case "":
                _namespace = NAMESPACE_DEFAULT;

                break;
            case NAMESPACE_NOVERSIONS:
                _namespace = NAMESPACE_NOVERSIONS;

                break;
            default:
                return ErrorNumber.InvalidArgument;
        }

        // Validate sector size - ODS uses 512-byte blocks
        _sectorSize = imagePlugin.Info.SectorSize;

        if(_sectorSize < ODS_BLOCK_SIZE)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Sector size {0} is smaller than ODS block size {1}",
                              _sectorSize,
                              ODS_BLOCK_SIZE);

            return ErrorNumber.InvalidArgument;
        }

        // Calculate how many ODS blocks fit in one device sector
        _blocksPerSector = _sectorSize / ODS_BLOCK_SIZE;

        AaruLogging.Debug(MODULE_NAME, "Sector size: {0}, blocks per sector: {1}", _sectorSize, _blocksPerSector);

        // Read home block (same logic as Info.cs)
        ErrorNumber errno = imagePlugin.ReadSector(1 + partition.Start, false, out byte[] hbSector, out _);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading home block: {0}", errno);

            return errno;
        }

        _homeBlock = Marshal.ByteArrayToStructureLittleEndian<HomeBlock>(hbSector);

        // Optical disc - if format signature is invalid, try alternate location
        if(imagePlugin.Info.MetadataMediaType          == MetadataMediaType.OpticalDisc &&
           StringHandlers.CToString(_homeBlock.format) != "DECFILE11A  "                &&
           StringHandlers.CToString(_homeBlock.format) != "DECFILE11B  ")
        {
            if(hbSector.Length < 0x400)
            {
                AaruLogging.Debug(MODULE_NAME, "Sector too small for alternate home block location");

                return ErrorNumber.InvalidArgument;
            }

            errno = imagePlugin.ReadSector(partition.Start, false, out byte[] tmp, out _);

            if(errno != ErrorNumber.NoError) return errno;

            hbSector = new byte[0x200];
            Array.Copy(tmp, 0x200, hbSector, 0, 0x200);

            _homeBlock = Marshal.ByteArrayToStructureLittleEndian<HomeBlock>(hbSector);
        }

        // Validate home block format signature
        string format = StringHandlers.CToString(_homeBlock.format);

        if(format != "DECFILE11A  " && format != "DECFILE11B  ")
        {
            AaruLogging.Debug(MODULE_NAME, "Invalid format signature: {0}", format);

            return ErrorNumber.InvalidArgument;
        }

        // Validate structure level (must be 2 or 5)
        _structureLevel = (byte)(_homeBlock.struclev >> 8 & 0xFF);

        if(_structureLevel != 2 && _structureLevel != 5)
        {
            AaruLogging.Debug(MODULE_NAME, "Invalid structure level: {0}", _structureLevel);

            return ErrorNumber.InvalidArgument;
        }

        // Validate checksums
        if(!ValidateHomeBlockChecksums(hbSector))
        {
            AaruLogging.Debug(MODULE_NAME, "Home block checksum validation failed");

            return ErrorNumber.InvalidArgument;
        }

        // Validate critical home block fields (following Linux ODS5 implementation)
        if(_homeBlock.homelbn == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "Home block LBN is zero");

            return ErrorNumber.InvalidArgument;
        }

        if(_homeBlock.alhomelbn == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "Alternate home block LBN is zero");

            return ErrorNumber.InvalidArgument;
        }

        if(_homeBlock.altidxlbn == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "Backup index file header LBN is zero");

            return ErrorNumber.InvalidArgument;
        }

        if(_homeBlock.ibmaplbn == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "Index file bitmap LBN is zero");

            return ErrorNumber.InvalidArgument;
        }

        if(_homeBlock.maxfiles <= _homeBlock.resfiles || _homeBlock.maxfiles >= 1 << 24)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Invalid maxfiles: {0} (resfiles: {1})",
                              _homeBlock.maxfiles,
                              _homeBlock.resfiles);

            return ErrorNumber.InvalidArgument;
        }

        if(_homeBlock.ibmapsize == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "Index file bitmap size is zero");

            return ErrorNumber.InvalidArgument;
        }

        // Volume sets are not supported
        if(_homeBlock.rvn != 0)
        {
            AaruLogging.Debug(MODULE_NAME, "Volume sets are not supported (rvn={0})", _homeBlock.rvn);

            return ErrorNumber.NotSupported;
        }

        _image     = imagePlugin;
        _partition = partition;

        // Read and validate the MFD (Master File Directory / root directory)
        errno = ReadFileHeader(MFD_FID, out FileHeader mfdHeader);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading MFD file header: {0}", errno);

            return errno;
        }

        // Validate MFD is a directory
        if(!mfdHeader.filechar.HasFlag(FileCharacteristicFlags.Directory))
        {
            AaruLogging.Debug(MODULE_NAME, "MFD is not a directory");

            return ErrorNumber.InvalidArgument;
        }

        // Cache root directory contents
        errno = CacheRootDirectory(mfdHeader);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error caching root directory: {0}", errno);

            return errno;
        }

        // Build metadata
        Metadata = new FileSystem
        {
            Type         = FS_TYPE,
            Clusters     = _homeBlock.maxfiles,
            ClusterSize  = ODS_BLOCK_SIZE * _homeBlock.cluster,
            VolumeName   = StringHandlers.SpacePaddedToString(_homeBlock.volname, _encoding),
            VolumeSerial = $"{_homeBlock.serialnum:X8}"
        };

        // Count used file IDs from the index file bitmap
        uint usedFiles = CountUsedFileIds();

        // Count free clusters from the storage bitmap
        ulong freeClusters = CountFreeClusters();

        // Calculate total clusters on volume
        ulong totalClusters = (_partition.End - _partition.Start + 1) *
                              _sectorSize /
                              (ODS_BLOCK_SIZE * _homeBlock.cluster);

        // Build statfs info
        _statfs = new FileSystemInfo
        {
            Blocks         = totalClusters,
            FilenameLength = _structureLevel == 5 ? (ushort)ODS5_MAX_FILENAME : (ushort)ODS2_MAX_FILENAME,
            Files          = usedFiles,
            FreeBlocks     = freeClusters,
            FreeFiles      = _homeBlock.maxfiles - usedFiles,
            PluginId       = Id,
            Type           = FS_TYPE,
            Id = new FileSystemId
            {
                Serial32 = _homeBlock.serialnum,
                IsInt    = true
            }
        };

        _mounted = true;

        AaruLogging.Debug(MODULE_NAME,
                          "Mounted ODS-{0} volume: {1}",
                          _structureLevel,
                          StringHandlers.SpacePaddedToString(_homeBlock.volname, _encoding));

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Unmount()
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        _rootDirectoryCache?.Clear();
        _rootDirectoryCache = null;
        _statfs             = null;
        _mounted            = false;

        return ErrorNumber.NoError;
    }

    /// <summary>Caches the root directory (MFD) contents.</summary>
    /// <param name="mfdHeader">File header of the MFD.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber CacheRootDirectory(FileHeader mfdHeader)
    {
        _rootDirectoryCache = new Dictionary<string, CachedFile>();

        // Get mapping information
        byte[] mapData = GetMapData(mfdHeader);

        if(mapData == null || mapData.Length == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "MFD has no mapping data");

            return ErrorNumber.InvalidArgument;
        }

        // Calculate file size from FAT
        long fileSize = ((long)mfdHeader.recattr.efblk.Value - 1) * ODS_BLOCK_SIZE + mfdHeader.recattr.ffbyte;

        if(fileSize <= 0)
        {
            AaruLogging.Debug(MODULE_NAME, "MFD has invalid size");

            return ErrorNumber.InvalidArgument;
        }

        // Skip the MFD's self-referential entry (000000.DIR), matching on the whole file ID
        ErrorNumber errno = ReadDirectoryEntriesInto(mfdHeader, _rootDirectoryCache, mfdHeader.fid);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading root directory entries: {0}", errno);

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME, "Cached {0} root directory entries", _rootDirectoryCache.Count);

        return ErrorNumber.NoError;
    }

    /// <summary>Counts the number of used file IDs by reading the index file bitmap.</summary>
    /// <returns>Number of used file IDs.</returns>
    uint CountUsedFileIds()
    {
        // The index file bitmap is located at ibmaplbn and is ibmapsize blocks
        // Each bit represents a file ID - if set, the file ID is in use
        uint usedFiles = 0;

        // Bit count lookup table for counting set bits in a byte
        ReadOnlySpan<byte> bitCount =
        [
            0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 1, 2, 2,
            3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 1, 2, 2, 3, 2, 3,
            3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 2, 3, 3, 4, 3, 4, 4, 5, 3,
            4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4,
            3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5,
            6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4,
            4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 4, 5, 5, 6, 5,
            6, 6, 7, 5, 6, 6, 7, 6, 7, 7, 8
        ];

        // Read each block of the index file bitmap
        for(uint i = 0; i < _homeBlock.ibmapsize; i++)
        {
            uint        lbn   = _homeBlock.ibmaplbn + i;
            ErrorNumber errno = ReadOdsBlock(_image, _partition, lbn, out byte[] bitmapBlock);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Error reading index file bitmap block at LBN {0}: {1}", lbn, errno);

                break;
            }

            // Count set bits in each byte
            foreach(byte b in bitmapBlock) usedFiles += bitCount[b];
        }

        return usedFiles;
    }

    /// <summary>Counts the number of free clusters by reading the storage bitmap (BITMAP.SYS).</summary>
    /// <returns>Number of free clusters.</returns>
    ulong CountFreeClusters()
    {
        // Read the BITMAP.SYS file header to get its mapping information
        ErrorNumber errno = ReadFileHeader(BITMAP_FID, out FileHeader bitmapHeader);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading BITMAP.SYS file header: {0}", errno);

            return 0;
        }

        byte[] mapData = GetMapData(bitmapHeader);

        if(mapData == null || mapData.Length == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "BITMAP.SYS has no mapping data");

            return 0;
        }

        // Calculate the number of bitmap blocks needed
        // The bitmap starts at VBN 2 (VBN 1 is the Storage Control Block)
        // Each bit represents one cluster, and each cluster is _homeBlock.cluster blocks
        long fileSize = ((long)bitmapHeader.recattr.efblk.Value - 1) * ODS_BLOCK_SIZE + bitmapHeader.recattr.ffbyte;

        if(fileSize <= ODS_BLOCK_SIZE)
        {
            AaruLogging.Debug(MODULE_NAME, "BITMAP.SYS is too small");

            return 0;
        }

        ulong freeClusters = 0;

        // Bit count lookup table for counting set bits in a byte
        ReadOnlySpan<byte> bitCount =
        [
            0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 1, 2, 2,
            3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 1, 2, 2, 3, 2, 3,
            3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 2, 3, 3, 4, 3, 4, 4, 5, 3,
            4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4,
            3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5,
            6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4,
            4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 4, 5, 5, 6, 5,
            6, 6, 7, 5, 6, 6, 7, 6, 7, 7, 8
        ];

        // Read the bitmap starting from VBN 2 (skip VBN 1 which is the SCB)
        var vbn = 2;

        while((vbn - 1) * ODS_BLOCK_SIZE < fileSize)
        {
            errno = MapVbnToLbn(mapData, bitmapHeader.map_inuse, (uint)vbn, out uint lbn, out _);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Error mapping BITMAP.SYS VBN {0}: {1}", vbn, errno);

                break;
            }

            errno = ReadOdsBlock(_image, _partition, lbn, out byte[] bitmapBlock);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Error reading storage bitmap block at LBN {0}: {1}", lbn, errno);

                break;
            }

            // Count set bits in each byte (set bit = free cluster)
            foreach(byte b in bitmapBlock) freeClusters += bitCount[b];

            vbn++;
        }

        return freeClusters;
    }
}