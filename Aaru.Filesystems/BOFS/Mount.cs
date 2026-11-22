// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Mount.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : BeOS old filesystem plugin.
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
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Logging;
using Partition = Aaru.CommonTypes.Partition;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements mounting of the BeOS old filesystem</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class BOFS
{
    const string MODULE_NAME = "BOFS plugin";

    /// <summary>Mounts a BOFS volume</summary>
    /// <param name="imagePlugin">The media image to mount</param>
    /// <param name="partition">The partition to mount</param>
    /// <param name="encoding">The encoding to use for text</param>
    /// <param name="options">Optional mount options</param>
    /// <param name="namespace">The namespace prefix to use</param>
    /// <returns>Error code indicating success or failure</returns>
    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options,     string    @namespace)
    {
        AaruLogging.Debug(MODULE_NAME, "Mounting volume");

        _imagePlugin = imagePlugin;
        _partition   = partition;
        _encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");

        ErrorNumber errno = ReadSuperblock();

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading superblock: {0}", errno);

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME, "Superblock read successfully. Version: 0x{0:X8}", _track0.VersionNumber);

        AaruLogging.Debug(MODULE_NAME, "Volume name: {0}", StringHandlers.CToString(_track0.VolumeName, _encoding));

        AaruLogging.Debug(MODULE_NAME, "Block size: {0} bytes", _track0.BytesPerSector);

        AaruLogging.Debug(MODULE_NAME,
                          "Total blocks: {0} ({1} bytes)",
                          _track0.TotalSectors,
                          (long)_track0.TotalSectors * _track0.BytesPerSector);

        AaruLogging.Debug(MODULE_NAME,
                          "Used blocks: {0} ({1} bytes)",
                          _track0.SectorsUsed,
                          (long)_track0.SectorsUsed * _track0.BytesPerSector);

        errno = LoadRootDirectory();

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error loading root directory: {0}", errno);

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME, "Root directory loaded successfully");

        Metadata = new FileSystem
        {
            Clusters     = (ulong)_track0.TotalSectors,
            ClusterSize  = (uint)_track0.BytesPerSector,
            Dirty        = _track0.CleanShutdown == 0,
            FreeClusters = (ulong)(_track0.TotalSectors - _track0.SectorsUsed),
            Type         = FS_TYPE,
            VolumeName   = StringHandlers.CToString(_track0.VolumeName, _encoding)
        };

        AaruLogging.Debug(MODULE_NAME,
                          "Mount complete. Dirty: {0}, Free clusters: {1}",
                          Metadata.Dirty,
                          Metadata.FreeClusters);

        return ErrorNumber.NoError;
    }

    /// <summary>Reads and validates the filesystem superblock</summary>
    /// <remarks>
    ///     Reads the Track0 superblock from the first BOFS logical sector and validates
    ///     the version number and block size to ensure this is a valid BOFS volume.
    ///     Device sector size must be at least 512 bytes to contain a full BOFS logical sector.
    /// </remarks>
    /// <returns>Error code indicating success or failure</returns>
    private ErrorNumber ReadSuperblock()
    {
        AaruLogging.Debug(MODULE_NAME, "Reading superblock...");
        AaruLogging.Debug(MODULE_NAME, "Device sector size: {0} bytes", _imagePlugin.Info.SectorSize);

        ulong     deviceSectorSize      = _imagePlugin.Info.SectorSize;
        const int bofsLogicalSectorSize = 512;

        // BOFS requires device sector size to be at least 512 bytes
        if(deviceSectorSize < bofsLogicalSectorSize)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Device sector size {0} is smaller than BOFS logical sector size {1}",
                              deviceSectorSize,
                              bofsLogicalSectorSize);

            return ErrorNumber.InvalidArgument;
        }

        ErrorNumber errno = _imagePlugin.ReadSectors(_partition.Start, false, 1, out byte[] sectorData, out _);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading superblock: {0}", errno);

            return errno;
        }

        // Extract first 512 bytes for the superblock
        var sbBuffer = new byte[bofsLogicalSectorSize];
        Array.Copy(sectorData, 0, sbBuffer, 0, bofsLogicalSectorSize);

        _track0 = Marshal.ByteArrayToStructureBigEndian<Track0>(sbBuffer);

        AaruLogging.Debug(MODULE_NAME, "Validating superblock...");

        // Validate superblock
        if(_track0.VersionNumber is not (VERSION_2 or VERSION_3) || _track0.BytesPerSector != 512)
        {
            AaruLogging.Debug(MODULE_NAME, "Superblock validation failed!");
            AaruLogging.Debug(MODULE_NAME, "  Version: 0x{0:X8} (expected 0x20000 or 0x30000)", _track0.VersionNumber);
            AaruLogging.Debug(MODULE_NAME, "  Bytes per sector: {0} (expected 512)", _track0.BytesPerSector);

            return ErrorNumber.InvalidArgument;
        }

        AaruLogging.Debug(MODULE_NAME, "Superblock validation successful!");

        return ErrorNumber.NoError;
    }

    /// <summary>Loads and caches all root directory entries</summary>
    /// <remarks>
    ///     Locates and reads all root directory blocks starting from FirstDirectorySector
    ///     and following the linked list of directory blocks. Parses FileEntry structures
    ///     from each block and caches them in a dictionary indexed by filename for fast access.
    /// </remarks>
    /// <returns>Error code indicating success or failure</returns>
    private ErrorNumber LoadRootDirectory()
    {
        AaruLogging.Debug(MODULE_NAME, "Loading root directory entries...");
        AaruLogging.Debug(MODULE_NAME, "First directory sector: {0}", _track0.FirstDirectorySector);

        _rootDirectoryCache.Clear();

        int       sector = _track0.FirstDirectorySector;
        const int bofsLogicalSectorSize = 512;
        const int directoryBlockSize = 8192; // Directory blocks are 8192 bytes (64-byte header + 63 × 128-byte entries)
        ulong     deviceSectorSize = _imagePlugin.Info.SectorSize;

        while(sector != 0)
        {
            AaruLogging.Debug(MODULE_NAME, "Reading directory block from sector {0}", sector);

            // FirstDirectorySector is a 512-byte sector number relative to partition start
            // Directory blocks are always 4096 bytes
            // Convert starting sector to byte offset within partition, then to device sector + offset
            ulong byteOffset = (ulong)sector * bofsLogicalSectorSize;

            // Which device sector contains this byte
            ulong deviceSectorOffsetFromPartition = byteOffset / deviceSectorSize;
            ulong offsetInDeviceSector            = byteOffset % deviceSectorSize;

            // Absolute device sector address
            ulong absoluteDeviceSector = _partition.Start + deviceSectorOffsetFromPartition;

            // Calculate how many device sectors needed to read the complete 4096-byte directory block
            // We need: offsetInDeviceSector + directoryBlockSize bytes total
            ulong bytesNeeded   = offsetInDeviceSector + directoryBlockSize;
            var   sectorsToRead = (uint)((bytesNeeded + deviceSectorSize - 1) / deviceSectorSize);

            AaruLogging.Debug(MODULE_NAME,
                              "Sector {0} -> byte offset {1}, device sector offset {2}, absolute sector {3}, offset 0x{4:X}, reading {5} device sectors for {6} bytes",
                              sector,
                              byteOffset,
                              deviceSectorOffsetFromPartition,
                              absoluteDeviceSector,
                              offsetInDeviceSector,
                              sectorsToRead,
                              directoryBlockSize);

            ErrorNumber errno =
                _imagePlugin.ReadSectors(absoluteDeviceSector, false, sectorsToRead, out byte[] sectorData, out _);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "Error reading sectors starting at {0}: {1}",
                                  absoluteDeviceSector,
                                  errno);

                return errno;
            }

            // Verify we have enough data
            if(sectorData.Length < (long)offsetInDeviceSector + directoryBlockSize)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "Read {0} bytes but need {1} bytes (offset {2} + block size {3})",
                                  sectorData.Length,
                                  offsetInDeviceSector + directoryBlockSize,
                                  offsetInDeviceSector,
                                  directoryBlockSize);

                return ErrorNumber.InvalidArgument;
            }

            var dirBlockBuffer = new byte[directoryBlockSize];
            Array.Copy(sectorData, (int)offsetInDeviceSector, dirBlockBuffer, 0, directoryBlockSize);

            // Directory block structure:
            // Bytes 0-63: DirectoryBlockHeader (NextDirectoryBlock, LinkUp, Filler[14])
            // Bytes 64-4095: 63 FileEntry structures (128 bytes each)

            // Parse header
            var nextDirectoryBlock = BigEndianBitConverter.ToInt32(dirBlockBuffer, 0);

            AaruLogging.Debug(MODULE_NAME, "Directory block header - NextBlock: {0}", nextDirectoryBlock);

            // Parse entries - they start at offset 64 (after the 64-byte header)
            var       entries    = new FileEntry[63];
            const int headerSize = 64;
            const int entrySize  = 128; // FileEntry is 128 bytes

            for(var i = 0; i < 63; i++)
            {
                var entryData = new byte[entrySize];
                Array.Copy(dirBlockBuffer, headerSize + i * entrySize, entryData, 0, entrySize);
                entries[i] = Marshal.ByteArrayToStructureBigEndian<FileEntry>(entryData);
            }

            // Parse all file entries
            foreach(FileEntry entry in entries)
            {
                // Skip empty entries (filename starts with null byte)
                if(entry.FileName == null || entry.FileName.Length == 0 || entry.FileName[0] == 0) continue;

                string filename = StringHandlers.CToString(entry.FileName, _encoding);

                if(string.IsNullOrWhiteSpace(filename))
                {
                    AaruLogging.Debug(MODULE_NAME, "  Skipping: Filename is null or whitespace");

                    continue;
                }

                // For files, validate size; for directories, don't
                if(entry.FileType != DIR_TYPE)
                {
                    // Skip entries with unreasonable sizes (likely garbage)
                    if(entry.LogicalSize < 0 || entry.LogicalSize > _track0.TotalSectors * _track0.BytesPerSector)
                    {
                        AaruLogging.Debug(MODULE_NAME,
                                          "  Skipping: Unreasonable size {0} (max: {1})",
                                          entry.LogicalSize,
                                          _track0.TotalSectors * _track0.BytesPerSector);

                        continue;
                    }
                }

                _rootDirectoryCache[filename] = entry;

                AaruLogging.Debug(MODULE_NAME,
                                  "Cached root entry: {0} (FileType: 0x{1:X8}, Size: {2})",
                                  filename,
                                  entry.FileType,
                                  entry.LogicalSize);
            }

            AaruLogging.Debug(MODULE_NAME,
                              "Directory block at BOFS sector {0}: next block = {1}",
                              sector,
                              nextDirectoryBlock);

            sector = nextDirectoryBlock;
        }

        AaruLogging.Debug(MODULE_NAME, "Loaded {0} root directory entries", _rootDirectoryCache.Count);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Unmount()
    {
        AaruLogging.Debug(MODULE_NAME, "Unmounting volume");

        // Clear the root directory cache
        _rootDirectoryCache.Clear();

        // Clear instance fields
        _imagePlugin = null;
        _partition   = default(Partition);
        _encoding    = null;
        _track0      = default(Track0);

        // Clear metadata
        Metadata = null;

        AaruLogging.Debug(MODULE_NAME, "Volume unmounted successfully");

        return ErrorNumber.NoError;
    }
}