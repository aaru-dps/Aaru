// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Mount.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : OS/2 High Performance File System plugin.
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
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Aaru.Logging;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class HPFS
{
    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options,     string    @namespace)
    {
        _image     = imagePlugin;
        _partition = partition;
        _encoding  = encoding ?? Encoding.GetEncoding("ibm850");

        options ??= GetDefaultOptions();

        if(options.TryGetValue("debug", out string debugString)) bool.TryParse(debugString, out _debug);

        // HPFS uses 512-byte sectors
        _bytesPerSector = 512;

        // Read boot block (sector 0)
        AaruLogging.Debug(MODULE_NAME, "Reading boot block at sector 0");

        ErrorNumber errno = _image.ReadSector(_partition.Start, false, out byte[] bootSector, out _);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading boot sector: {0}", errno);

            return errno;
        }

        _bpb = Marshal.ByteArrayToStructureLittleEndian<BiosParameterBlock>(bootSector);

        // Validate boot block signature
        if(_bpb.signature2 != BB_MAGIC)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Invalid boot block signature: 0x{0:X4} (expected 0x{1:X4})",
                              _bpb.signature2,
                              BB_MAGIC);

            return ErrorNumber.InvalidArgument;
        }

        // Read superblock (sector 16)
        AaruLogging.Debug(MODULE_NAME, "Reading superblock at sector 16");

        errno = _image.ReadSector(_partition.Start + 16, false, out byte[] sbSector, out _);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading superblock: {0}", errno);

            return errno;
        }

        _superblock = Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sbSector);

        // Validate superblock magic
        if(_superblock.magic1 != SB_MAGIC || _superblock.magic2 != SB_MAGIC2)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Invalid superblock magic: 0x{0:X8} 0x{1:X8} (expected 0x{2:X8} 0x{3:X8})",
                              _superblock.magic1,
                              _superblock.magic2,
                              SB_MAGIC,
                              SB_MAGIC2);

            return ErrorNumber.InvalidArgument;
        }

        // Read spareblock (sector 17)
        AaruLogging.Debug(MODULE_NAME, "Reading spareblock at sector 17");

        errno = _image.ReadSector(_partition.Start + 17, false, out byte[] spSector, out _);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading spareblock: {0}", errno);

            return errno;
        }

        _spareblock = Marshal.ByteArrayToStructureLittleEndian<SpareBlock>(spSector);

        // Validate spareblock magic
        if(_spareblock.magic1 != SP_MAGIC || _spareblock.magic2 != SP_MAGIC2)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Invalid spareblock magic: 0x{0:X8} 0x{1:X8} (expected 0x{2:X8} 0x{3:X8})",
                              _spareblock.magic1,
                              _spareblock.magic2,
                              SP_MAGIC,
                              SP_MAGIC2);

            return ErrorNumber.InvalidArgument;
        }

        // Check HPFS version (functional version must be 2 or 3)
        if(_superblock.func_version is not 2 and not 3)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Unsupported HPFS functional version: {0} (expected 2 or 3)",
                              _superblock.func_version);

            return ErrorNumber.InvalidArgument;
        }

        // Validate filesystem size
        if(_superblock.sectors >= 0x80000000)
        {
            AaruLogging.Debug(MODULE_NAME, "Invalid filesystem size in superblock: 0x{0:X8}", _superblock.sectors);

            return ErrorNumber.InvalidArgument;
        }

        AaruLogging.Debug(MODULE_NAME, "HPFS version: {0}.{1}",        _superblock.version, _superblock.func_version);
        AaruLogging.Debug(MODULE_NAME, "Filesystem size: {0} sectors", _superblock.sectors);
        AaruLogging.Debug(MODULE_NAME, "Root fnode: {0}",              _superblock.root_fnode);

        _rootFnode = _superblock.root_fnode;

        // Initialize caches
        _fnodeCache         = new Dictionary<uint, FNode>();
        _dnodeCache         = new Dictionary<uint, DNode>();
        _directoryCache     = new Dictionary<uint, Dictionary<string, uint>>();
        _rootDirectoryCache = new Dictionary<string, uint>();

        // Load code page table if available
        if(_spareblock.codepages > 0 && _spareblock.codepage_lsn != 0)
        {
            errno = LoadCodePageTable(_spareblock.codepage_lsn);

            if(errno != ErrorNumber.NoError)
                AaruLogging.Debug(MODULE_NAME, "Warning: could not load code page table: {0}", errno);
        }

        // Read root fnode to get root dnode
        errno = ReadFNode(_rootFnode, out FNode rootFnodeStruct);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading root fnode: {0}", errno);

            return errno;
        }

        // Validate root fnode is a directory
        if(!rootFnodeStruct.IsDirectory)
        {
            AaruLogging.Debug(MODULE_NAME, "Root fnode is not a directory");

            return ErrorNumber.InvalidArgument;
        }

        // Get root dnode sector from the fnode's btree
        // For a directory fnode, the first extent points to the root dnode
        BPlusLeafNode[] leafNodes = GetBPlusLeafNodes(rootFnodeStruct.btree, rootFnodeStruct.btree_data);

        if(leafNodes.Length == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "Root fnode has no extents");

            return ErrorNumber.InvalidArgument;
        }

        _rootDnode = leafNodes[0].disk_secno;
        AaruLogging.Debug(MODULE_NAME, "Root dnode: {0}", _rootDnode);

        // Read and cache the root directory
        errno = CacheRootDirectory();

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error caching root directory: {0}", errno);

            return errno;
        }

        // Build metadata
        Metadata = new FileSystem
        {
            Type         = FS_TYPE,
            Clusters     = _superblock.sectors,
            ClusterSize  = _bytesPerSector,
            Dirty        = _spareblock.flags1.HasFlag(SpareBlockFlags.Dirty),
            VolumeName   = StringHandlers.CToString(_bpb.volume_label, _encoding),
            VolumeSerial = $"{_bpb.serial_no:X8}"
        };

        // Build filesystem info for StatFs
        _statfs = new FileSystemInfo
        {
            Blocks         = _superblock.sectors,
            FilenameLength = 254, // HPFS supports filenames up to 254 characters
            Files          = 0,   // Would require traversing all directories
            FreeBlocks     = 0,   // Would require parsing the bitmap
            FreeFiles      = 0,
            Id = new FileSystemId
            {
                IsInt    = true,
                Serial32 = _bpb.serial_no
            },
            PluginId = Id,
            Type     = FS_TYPE
        };

        _mounted = true;

        return ErrorNumber.NoError;
    }


    /// <inheritdoc />
    public ErrorNumber Unmount()
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        _fnodeCache?.Clear();
        _dnodeCache?.Clear();
        _directoryCache?.Clear();
        _rootDirectoryCache?.Clear();
        _codePageTable = null;
        _mounted       = false;

        return ErrorNumber.NoError;
    }

    /// <summary>Caches the root directory entries.</summary>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber CacheRootDirectory()
    {
        ErrorNumber errno = ReadDNode(_rootDnode, out DNode rootDnode);

        if(errno != ErrorNumber.NoError) return errno;

        return CacheDNodeEntries(rootDnode, _rootDirectoryCache);
    }
}