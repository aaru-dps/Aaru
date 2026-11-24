// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Mount.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SmartFileSystem plugin.
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
using Aaru.Helpers;
using Aaru.Logging;
using Partition = Aaru.CommonTypes.Partition;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements the Smart File System</summary>
public sealed partial class SFS
{
    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options,     string    @namespace)
    {
        AaruLogging.Debug(MODULE_NAME, "Mounting SFS volume");

        _imagePlugin = imagePlugin;
        _partition   = partition;
        _encoding    = encoding ?? Encoding.GetEncoding("iso-8859-1");

        // Read root block (block 0) - first just one sector to get the block size
        ErrorNumber errno = ReadBlock(0, out byte[] rootBlockData);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading root block: {0}", errno);

            return errno;
        }

        _rootBlock = Marshal.ByteArrayToStructureBigEndian<RootBlock>(rootBlockData);

        // Validate root block magic
        if(_rootBlock.blockId is not SFS_MAGIC and not SFS2_MAGIC)
        {
            AaruLogging.Debug(MODULE_NAME, "Invalid root block magic: 0x{0:X8}", _rootBlock.blockId);

            return ErrorNumber.InvalidArgument;
        }

        // Now we know the block size, re-read if block size > sector size
        if(_rootBlock.blocksize > _imagePlugin.Info.SectorSize)
        {
            _blockSize = _rootBlock.blocksize;

            errno = ReadBlock(0, out rootBlockData);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Error re-reading root block with correct size: {0}", errno);

                return errno;
            }

            // Re-parse after reading full block
            _rootBlock = Marshal.ByteArrayToStructureBigEndian<RootBlock>(rootBlockData);
        }

        // Determine if this is SFS\2 format, needed to know the checksum seed
        _isSfs2 = _rootBlock.blockId == SFS2_MAGIC;

        // Validate checksum
        if(!ValidateChecksum(rootBlockData, _isSfs2))
        {
            AaruLogging.Debug(MODULE_NAME, "Root block checksum invalid");

            return ErrorNumber.InvalidArgument;
        }

        // Validate own block pointer
        if(_rootBlock.blockSelfPointer != 0)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Root block self-pointer invalid: {0}, expected 0",
                              _rootBlock.blockSelfPointer);

            return ErrorNumber.InvalidArgument;
        }

        // Validate version, SFS\0 uses version 3 and SFS\2 uses version 4
        ushort expectedVersion = _isSfs2 ? STRUCTURE_VERSION_SFS2 : STRUCTURE_VERSION_SFS0;

        if(_rootBlock.version != expectedVersion)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Unsupported structure version: {0}, expected {1}",
                              _rootBlock.version,
                              expectedVersion);

            return ErrorNumber.NotSupported;
        }

        _objectSize = _isSfs2 ? OBJECT_SIZE_SFS2 : OBJECT_SIZE_SFS0;

        AaruLogging.Debug(MODULE_NAME, "SFS format: {0}, object size: {1}", _isSfs2 ? "SFS\\2" : "SFS\\0", _objectSize);

        _blockSize           = _rootBlock.blocksize;
        _blockShift          = CalculateBlockShift(_blockSize);
        _totalBlocks         = _rootBlock.totalblocks;
        _rootObjectContainer = _rootBlock.rootobjectcontainer;
        _objectNodeRoot      = _rootBlock.objectnoderoot;
        _caseSensitive       = _rootBlock.bits.HasFlag(Flags.CaseSensitive);

        AaruLogging.Debug(MODULE_NAME, "Block size: {0} bytes, shift: {1}", _blockSize, _blockShift);
        AaruLogging.Debug(MODULE_NAME, "Total blocks: {0}",                 _totalBlocks);
        AaruLogging.Debug(MODULE_NAME, "Root object container: {0}",        _rootObjectContainer);
        AaruLogging.Debug(MODULE_NAME, "Object node root: {0}",             _objectNodeRoot);
        AaruLogging.Debug(MODULE_NAME, "Case sensitive: {0}",               _caseSensitive);

        // Read root object container to get volume name and root info
        errno = ReadBlock(_rootObjectContainer, out byte[] rootOcData);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading root object container: {0}", errno);

            return errno;
        }

        // Validate object container
        var ocId = BigEndianBitConverter.ToUInt32(rootOcData, 0);

        if(ocId != OBJECTCONTAINER_ID)
        {
            AaruLogging.Debug(MODULE_NAME, "Invalid object container ID: 0x{0:X8}", ocId);

            return ErrorNumber.InvalidArgument;
        }

        if(!ValidateChecksum(rootOcData, _isSfs2))
        {
            AaruLogging.Debug(MODULE_NAME, "Root object container checksum invalid");

            return ErrorNumber.InvalidArgument;
        }

        // Parse the root info structure at the end of the block
        int rootInfoOffset = (int)_blockSize - Marshal.SizeOf<RootInfo>();
        var rootInfoData   = new byte[Marshal.SizeOf<RootInfo>()];
        Array.Copy(rootOcData, rootInfoOffset, rootInfoData, 0, rootInfoData.Length);
        _rootInfo = Marshal.ByteArrayToStructureBigEndian<RootInfo>(rootInfoData);

        // Parse the root object (volume root directory) at offset after the ObjectContainer header
        int objectOffset = Marshal.SizeOf<ObjectContainer>();

        // Read the volume name from the root object
        string volumeName = ReadObjectName(rootOcData, objectOffset);

        AaruLogging.Debug(MODULE_NAME, "Volume name: {0}", volumeName);
        AaruLogging.Debug(MODULE_NAME, "Free blocks: {0}", _rootInfo.freeBlocks);

        // Load root directory entries
        errno = LoadRootDirectory();

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error loading root directory: {0}", errno);

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME, "Loaded {0} root directory entries", _rootDirectoryCache.Count);

        // Build metadata
        Metadata = new FileSystem
        {
            Type         = FS_TYPE,
            VolumeName   = volumeName,
            Clusters     = _totalBlocks,
            ClusterSize  = _blockSize,
            FreeClusters = _rootInfo.freeBlocks,
            CreationDate = DateHandlers.UnixUnsignedToDateTime(_rootBlock.datecreated).AddYears(8)
        };

        _mounted = true;

        AaruLogging.Debug(MODULE_NAME, "Volume mounted successfully");

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Unmount()
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        AaruLogging.Debug(MODULE_NAME, "Unmounting filesystem...");

        _rootDirectoryCache.Clear();
        _imagePlugin = null;
        _encoding    = null;
        _mounted     = false;

        AaruLogging.Debug(MODULE_NAME, "Filesystem unmounted successfully");

        return ErrorNumber.NoError;
    }


    /// <summary>Loads the root directory entries into the cache</summary>
    /// <returns>Error code indicating success or failure</returns>
    ErrorNumber LoadRootDirectory()
    {
        AaruLogging.Debug(MODULE_NAME, "Loading root directory");

        // Read the root object to get the first directory block
        ErrorNumber errno = FindObjectNode(ROOTNODE, out uint rootObjectBlock);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error finding root object node: {0}", errno);

            return errno;
        }

        errno = ReadBlock(rootObjectBlock, out byte[] rootObjectData);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading root object block: {0}", errno);

            return errno;
        }

        // Find the root object in the container
        errno = FindObjectInContainer(rootObjectData, ROOTNODE, out int objectOffset);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error finding root object in container: {0}", errno);

            return errno;
        }

        // Read first directory block pointer from the object
        // Offset to firstdirblock in directory object: ownerUid(2) + ownerGid(2) + objectNode(4) + protection(4) +
        //                                              hashtable(4) + firstdirblock(4)
        int firstDirBlockOffset = objectOffset + 16;
        var firstDirBlock       = BigEndianBitConverter.ToUInt32(rootObjectData, firstDirBlockOffset);

        AaruLogging.Debug(MODULE_NAME, "First directory block: {0}", firstDirBlock);

        // Traverse the directory chain
        uint nextBlock = firstDirBlock;

        while(nextBlock != 0)
        {
            errno = ReadBlock(nextBlock, out byte[] dirBlockData);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Error reading directory block {0}: {1}", nextBlock, errno);

                return errno;
            }

            // Validate block
            var blockId = BigEndianBitConverter.ToUInt32(dirBlockData, 0);

            if(blockId != OBJECTCONTAINER_ID)
            {
                AaruLogging.Debug(MODULE_NAME, "Invalid block ID in directory block: 0x{0:X8}", blockId);

                break;
            }

            // Parse objects in this container
            ParseObjectContainer(dirBlockData);

            // Get next block in chain
            // ObjectContainer: header(12) + parent(4) + next(4) + previous(4)
            nextBlock = BigEndianBitConverter.ToUInt32(dirBlockData, 16);
        }

        return ErrorNumber.NoError;
    }
}