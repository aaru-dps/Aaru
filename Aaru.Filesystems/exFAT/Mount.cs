// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Mount.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft exFAT filesystem plugin.
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
using Aaru.Logging;
using FileSystemInfo = Aaru.CommonTypes.Structs.FileSystemInfo;
using Marshal = Aaru.Helpers.Marshal;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

// Information from https://github.com/MicrosoftDocs/win32/blob/docs/desktop-src/FileIO/exfat-specification.md
/// <inheritdoc />
public sealed partial class exFAT
{
#region IReadOnlyFilesystem Members

    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options,     string    @namespace)
    {
        Metadata = new FileSystem();

        options ??= GetDefaultOptions();

        if(options.TryGetValue("debug", out string debugString)) bool.TryParse(debugString, out _debug);

        AaruLogging.Debug(MODULE_NAME, Localization.Reading_BPB);

        ErrorNumber errno = imagePlugin.ReadSector(0 + partition.Start, false, out byte[] vbrSector, out _);

        if(errno != ErrorNumber.NoError) return errno;

        if(vbrSector.Length < 512) return ErrorNumber.InvalidArgument;

        VolumeBootRecord vbr = Marshal.ByteArrayToStructureLittleEndian<VolumeBootRecord>(vbrSector);

        // Validate signature
        if(!_signature.SequenceEqual(vbr.FileSystemName)) return ErrorNumber.InvalidArgument;

        // Validate boot signature
        if(vbr.BootSignature != BOOT_SIGNATURE) return ErrorNumber.InvalidArgument;

        // Get filesystem parameters
        _bytesPerSector              = (uint)(1 << vbr.BytesPerSectorShift);
        _sectorsPerCluster           = (uint)(1 << vbr.SectorsPerClusterShift);
        _bytesPerCluster             = _bytesPerSector * _sectorsPerCluster;
        _fatFirstSector              = partition.Start + vbr.FatOffset;
        _clusterHeapOffset           = partition.Start + vbr.ClusterHeapOffset;
        _clusterCount                = vbr.ClusterCount;
        _firstClusterOfRootDirectory = vbr.FirstClusterOfRootDirectory;

        if((ulong)_clusterCount * _sectorsPerCluster > partition.End - partition.Start + 1)
            return ErrorNumber.InvalidArgument;

        // Determine which FAT to use
        _useFirstFat = !vbr.VolumeFlags.HasFlag(VolumeFlags.ActiveFat);

        AaruLogging.Debug(MODULE_NAME, "Bytes per sector: {0}",             _bytesPerSector);
        AaruLogging.Debug(MODULE_NAME, "Sectors per cluster: {0}",          _sectorsPerCluster);
        AaruLogging.Debug(MODULE_NAME, "Bytes per cluster: {0}",            _bytesPerCluster);
        AaruLogging.Debug(MODULE_NAME, "FAT first sector: {0}",             _fatFirstSector);
        AaruLogging.Debug(MODULE_NAME, "Cluster heap offset: {0}",          _clusterHeapOffset);
        AaruLogging.Debug(MODULE_NAME, "Cluster count: {0}",                _clusterCount);
        AaruLogging.Debug(MODULE_NAME, "Root directory first cluster: {0}", _firstClusterOfRootDirectory);
        AaruLogging.Debug(MODULE_NAME, "Using first FAT: {0}",              _useFirstFat);

        // Validate root directory cluster
        if(_firstClusterOfRootDirectory < 2 || _firstClusterOfRootDirectory > _clusterCount + 1)
            return ErrorNumber.InvalidArgument;

        // Read FAT
        AaruLogging.Debug(MODULE_NAME, "Reading FAT");

        ulong fatSector = _useFirstFat ? _fatFirstSector : _fatFirstSector + vbr.FatLength;

        errno = imagePlugin.ReadSectors(fatSector, false, vbr.FatLength, out byte[] fatBytes, out _);

        if(errno != ErrorNumber.NoError) return errno;

        // Cast FAT bytes to uint array (exFAT uses 32-bit FAT entries)
        _fatEntries = new uint[_clusterCount + 2];

        Buffer.BlockCopy(fatBytes, 0, _fatEntries, 0, (int)Math.Min(fatBytes.Length, ((long)_clusterCount + 2) * 4));

        // Validate FAT entries 0 and 1
        if((_fatEntries[0] & 0xFFFFFFF0) != 0xFFFFFFF0)
            AaruLogging.Debug(MODULE_NAME, "FAT entry 0 is not valid: {0:X8}", _fatEntries[0]);

        if(_fatEntries[1] != 0xFFFFFFFF)
            AaruLogging.Debug(MODULE_NAME, "FAT entry 1 is not valid: {0:X8}", _fatEntries[1]);

        // Store image reference (needed by ReadClusterChain)
        _image = imagePlugin;

        // Read root directory
        AaruLogging.Debug(MODULE_NAME, "Reading root directory");

        byte[] rootDirectory = ReadClusterChain(_firstClusterOfRootDirectory);

        if(rootDirectory is null) return ErrorNumber.InvalidArgument;

        // Initialize caches
        _rootDirectoryCache = new Dictionary<string, CompleteDirectoryEntry>();
        _directoryCache     = new Dictionary<string, Dictionary<string, CompleteDirectoryEntry>>();

        // Parse root directory
        ParseDirectory(rootDirectory, _rootDirectoryCache);

        // Setup filesystem information
        _statfs = new FileSystemInfo
        {
            FilenameLength = 255,
            Files          = (ulong)_rootDirectoryCache.Count,
            FreeFiles      = 0,
            PluginId       = Id,
            FreeBlocks     = 0, // Would require traversing the FAT
            Blocks         = _clusterCount,
            Type           = FS_TYPE
        };

        // Setup metadata
        Metadata.Type         = FS_TYPE;
        Metadata.ClusterSize  = _bytesPerCluster;
        Metadata.Clusters     = _clusterCount;
        Metadata.VolumeSerial = $"{vbr.VolumeSerialNumber:X8}";
        Metadata.Dirty        = vbr.VolumeFlags.HasFlag(VolumeFlags.VolumeDirty);

        _mounted = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Unmount()
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        _mounted            = false;
        _image              = null;
        _fatEntries         = null;
        _rootDirectoryCache = null;
        _directoryCache     = null;
        _statfs             = null;

        return ErrorNumber.NoError;
    }

#endregion
}