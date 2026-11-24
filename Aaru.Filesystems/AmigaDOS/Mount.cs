// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Mount.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Amiga Fast File System plugin.
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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Logging;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class AmigaDOSPlugin
{
    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options,     string    @namespace)
    {
        AaruLogging.Debug(MODULE_NAME, "Mounting AmigaDOS volume");

        _imagePlugin = imagePlugin;
        _partition   = partition;
        _encoding    = encoding ?? Encoding.GetEncoding("iso-8859-1");

        // Read boot block (first 2 sectors)
        ErrorNumber errno =
            _imagePlugin.ReadSectors(0 + _partition.Start, false, 2, out byte[] bootBlockSectors, out _);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading boot block: {0}", errno);

            return errno;
        }

        _bootBlock = Marshal.ByteArrayToStructureBigEndian<BootBlock>(bootBlockSectors);

        // AROS boot floppies have a different layout
        if(bootBlockSectors.Length           >= 512      &&
           bootBlockSectors[510]             == 0x55     &&
           bootBlockSectors[511]             == 0xAA     &&
           (_bootBlock.diskType & FFS_MASK)  != FFS_MASK &&
           (_bootBlock.diskType & MUFS_MASK) != MUFS_MASK)
        {
            errno = _imagePlugin.ReadSectors(1 + _partition.Start, false, 2, out bootBlockSectors, out _);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Error reading boot block (AROS): {0}", errno);

                return errno;
            }

            _bootBlock = Marshal.ByteArrayToStructureBigEndian<BootBlock>(bootBlockSectors);
        }

        // Validate DOS type
        if((_bootBlock.diskType & FFS_MASK) != FFS_MASK && (_bootBlock.diskType & MUFS_MASK) != MUFS_MASK)
        {
            AaruLogging.Debug(MODULE_NAME, "Invalid DOS type: 0x{0:X8}", _bootBlock.diskType);

            return ErrorNumber.InvalidArgument;
        }

        // Determine filesystem variant from low byte
        // 0 = OFS, 1 = FFS, 2 = OFS INTL, 3 = FFS INTL, 4 = OFS DIRCACHE, 5 = FFS DIRCACHE, 6 = OFS2, 7 = FFS2
        var dosFlags = (byte)(_bootBlock.diskType & 0xFF);

        // The AmigaOS 4 FastFileSystem uses international case folding for DOS\2 to DOS\7, keeps a directory
        // cache only on DOS\4 and DOS\5, and enables long names only on DOS\6 and DOS\7
        _isFfs        = (dosFlags & 0x01) == 0x01; // Bit 0: FFS vs OFS
        _isIntl       = dosFlags          >= 2;
        _hasDirCache  = dosFlags is 4 or 5;
        _hasLongNames = dosFlags is 6 or 7;
        _isMuFs       = (_bootBlock.diskType & MUFS_MASK) == MUFS_MASK;

        AaruLogging.Debug(MODULE_NAME,
                          "DOS type: 0x{0:X8}, FFS={1}, INTL={2}, DIRCACHE={3}, LongNames={4}, MuFS={5}",
                          _bootBlock.diskType,
                          _isFfs,
                          _isIntl,
                          _hasDirCache,
                          _hasLongNames,
                          _isMuFs);

        // FFS2/OFS2 (long filenames) is not supported - we don't have proper implementation reference
        if(_hasLongNames)
        {
            AaruLogging.Debug(MODULE_NAME, "FFS2/OFS2 (long filenames) is not supported");

            return ErrorNumber.NotSupported;
        }

        // Validate boot block checksum
        bootBlockSectors[4] = bootBlockSectors[5] = bootBlockSectors[6] = bootBlockSectors[7] = 0;
        uint bsum                                 = AmigaBootChecksum(bootBlockSectors);

        _bootBlockValid = bsum == _bootBlock.checksum;

        AaruLogging.Debug(MODULE_NAME,
                          "Boot block checksum: 0x{0:X8}, calculated: 0x{1:X8}, valid: {2}",
                          _bootBlock.checksum,
                          bsum,
                          _bootBlockValid);

        // Find root block
        ulong bRootPtr = 0;

        if(_bootBlockValid && _bootBlock.root_ptr != 0) bRootPtr = _bootBlock.root_ptr + _partition.Start;

        // Try standard root block locations
        ulong[] rootPtrs =
        [
            bRootPtr, (_partition.End - _partition.Start + 1) / 2 + _partition.Start - 2,
            (_partition.End           - _partition.Start + 1) / 2 + _partition.Start - 1,
            (_partition.End                              - _partition.Start + 1) / 2 + _partition.Start,
            (_partition.End                              - _partition.Start + 1) / 2 + _partition.Start + 4
        ];

        byte[] rootBlockSector = null;
        var    rootFound       = false;

        foreach(ulong rootPtr in rootPtrs.Where(rootPtr => rootPtr < _partition.End && rootPtr >= _partition.Start))
        {
            AaruLogging.Debug(MODULE_NAME, "Searching for root block at sector {0}", rootPtr);

            errno = _imagePlugin.ReadSector(rootPtr, false, out rootBlockSector, out _);

            if(errno != ErrorNumber.NoError) continue;

            var type = BigEndianBitConverter.ToUInt32(rootBlockSector, 0x00);

            if(type != TYPE_HEADER) continue;

            var hashTableSize = BigEndianBitConverter.ToUInt32(rootBlockSector, 0x0C);
            _blockSize       = (hashTableSize + 56) * 4;
            _sectorsPerBlock = (uint)(_blockSize / rootBlockSector.Length);

            if(_blockSize % rootBlockSector.Length > 0) _sectorsPerBlock++;

            if(rootPtr + _sectorsPerBlock >= _partition.End) continue;

            errno = _imagePlugin.ReadSectors(rootPtr, false, _sectorsPerBlock, out rootBlockSector, out _);

            if(errno != ErrorNumber.NoError) continue;

            // Validate checksum
            var checksum                              = BigEndianBitConverter.ToUInt32(rootBlockSector, 20);
            rootBlockSector[20] = rootBlockSector[21] = rootBlockSector[22] = rootBlockSector[23] = 0;
            uint rsum                                 = AmigaChecksum(rootBlockSector);

            var secType = BigEndianBitConverter.ToUInt32(rootBlockSector, rootBlockSector.Length - 4);

            if(secType != SUBTYPE_ROOT || checksum != rsum) continue;

            // Re-read sector since we modified it for checksum calculation
            errno = _imagePlugin.ReadSectors(rootPtr, false, _sectorsPerBlock, out rootBlockSector, out _);

            if(errno != ErrorNumber.NoError) continue;

            _rootBlockSector = (uint)(rootPtr - _partition.Start);
            rootFound        = true;

            AaruLogging.Debug(MODULE_NAME, "Found root block at sector {0}, block size {1}", rootPtr, _blockSize);

            break;
        }

        if(!rootFound)
        {
            AaruLogging.Debug(MODULE_NAME, "Root block not found");

            return ErrorNumber.InvalidArgument;
        }

        _rootBlock = MarshalRootBlock(rootBlockSector);

        AaruLogging.Debug(MODULE_NAME, "Root block hash table size: {0}", _rootBlock.hashTableSize);
        AaruLogging.Debug(MODULE_NAME, "Bitmap valid flag: 0x{0:X8}",     _rootBlock.bitmapFlag);

        // Calculate total blocks
        _totalBlocks = (uint)((_partition.End - _partition.Start + 1) * _imagePlugin.Info.SectorSize / _blockSize);

        // Cache root directory entries
        _rootDirectoryCache = new Dictionary<string, uint>();

        errno = LoadRootDirectory();

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error loading root directory: {0}", errno);

            return errno;
        }

        // Build metadata
        string diskName = StringHandlers.PascalToString(_rootBlock.diskName, _encoding);

        // Determine filesystem type string based on variant
        string fsType = (_bootBlock.diskType & 0xFF) switch
                        {
                            0 => FS_TYPE_OFS,  // OFS
                            1 => FS_TYPE_FFS,  // FFS
                            2 => FS_TYPE_OFS,  // OFS INTL
                            3 => FS_TYPE_FFS,  // FFS INTL
                            4 => FS_TYPE_OFS,  // OFS DIRCACHE
                            5 => FS_TYPE_FFS,  // FFS DIRCACHE
                            6 => FS_TYPE_OFS2, // OFS2 (long filenames)
                            7 => FS_TYPE_FFS2, // FFS2 (long filenames)
                            _ => _isFfs ? FS_TYPE_FFS : FS_TYPE_OFS
                        };

        Metadata = new FileSystem
        {
            Type             = fsType,
            VolumeName       = diskName,
            Clusters         = _totalBlocks,
            ClusterSize      = _blockSize,
            Dirty            = _rootBlock.bitmapFlag != 0xFFFFFFFF,
            Bootable         = _bootBlockValid,
            VolumeSerial     = $"{_rootBlock.checksum:X8}",
            CreationDate     = DateHandlers.AmigaToDateTime(_rootBlock.cDays, _rootBlock.cMins, _rootBlock.cTicks),
            ModificationDate = DateHandlers.AmigaToDateTime(_rootBlock.vDays, _rootBlock.vMins, _rootBlock.vTicks)
        };

        _mounted = true;

        AaruLogging.Debug(MODULE_NAME, "Volume mounted successfully: {0}", diskName);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Unmount()
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        _rootDirectoryCache?.Clear();
        _mounted = false;

        return ErrorNumber.NoError;
    }

    /// <summary>Loads the root directory entries into the cache</summary>
    /// <returns>Error code</returns>
    ErrorNumber LoadRootDirectory()
    {
        AaruLogging.Debug(MODULE_NAME, "Loading root directory");

        // Read entries from hash table
        foreach(uint hashEntry in _rootBlock.hashTable)
        {
            uint blockPtr = hashEntry;

            if(blockPtr == 0) continue;

            // Follow the hash chain
            while(blockPtr != 0)
            {
                ErrorNumber errno = ReadBlock(blockPtr, out byte[] blockData);

                if(errno != ErrorNumber.NoError)
                {
                    AaruLogging.Debug(MODULE_NAME, "Error reading block {0}: {1}", blockPtr, errno);

                    return errno;
                }

                // Validate block type
                var type = BigEndianBitConverter.ToUInt32(blockData, 0x00);

                if(type != TYPE_HEADER)
                {
                    AaruLogging.Debug(MODULE_NAME, "Invalid block type at {0}: {1}", blockPtr, type);

                    break;
                }

                // Get secondary type (at end of block)
                var secType = BigEndianBitConverter.ToUInt32(blockData, blockData.Length - 4);

                // Get entry name (Pascal string at fixed offset from end)
                // BLK_DIRECTORYNAME_START = SizeBlock - 20 (in longs) = blockData.Length - 80 bytes
                int  nameOffset = blockData.Length - 20 * 4;
                byte nameLen    = blockData[nameOffset];

                // Standard AmigaDOS allows 30 characters, name is stored as Pascal string
                // The length byte determines actual length, but we validate against max
                if(nameLen > MAX_NAME_LENGTH) nameLen = MAX_NAME_LENGTH;

                // Ensure we don't read beyond block boundaries
                int availableSpace = blockData.Length - nameOffset - 1;

                if(nameLen > availableSpace) nameLen = (byte)availableSpace;

                string name = nameLen > 0 ? _encoding.GetString(blockData, nameOffset + 1, nameLen) : string.Empty;

                // Store in cache
                if(!string.IsNullOrEmpty(name) && !_rootDirectoryCache.ContainsKey(name))
                {
                    _rootDirectoryCache[name] = blockPtr;

                    AaruLogging.Debug(MODULE_NAME, "Found entry: {0} -> block {1}, type {2}", name, blockPtr, secType);
                }

                // Get next in hash chain (at fixed offset from end)
                // BLK_HASHCHAIN = SizeBlock - 4 (in longs) = blockData.Length - 16 bytes
                int nextHashOffset = blockData.Length - 4 * 4;
                blockPtr = BigEndianBitConverter.ToUInt32(blockData, nextHashOffset);
            }
        }

        AaruLogging.Debug(MODULE_NAME, "Loaded {0} root directory entries", _rootDirectoryCache.Count);

        return ErrorNumber.NoError;
    }
}