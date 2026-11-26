// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Mount.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : JXFileSystem plugin.
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
using Aaru.Helpers;
using Aaru.Logging;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class JXFS
{
    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options,     string    @namespace)
    {
        AaruLogging.Debug(MODULE_NAME, "Mounting JXFS volume");

        ErrorNumber errno = Initialize(imagePlugin, partition, encoding);

        if(errno != ErrorNumber.NoError) return errno;

        Metadata = BuildMetadata();

        _mounted = true;

        AaruLogging.Debug(MODULE_NAME, "Volume mounted successfully");

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Unmount()
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        _directoryCache.Clear();
        _nodeFile    = null;
        _dirFile     = null;
        _miscFile    = null;
        _imagePlugin = null;
        _encoding    = null;
        _mounted     = false;

        return ErrorNumber.NoError;
    }

    /// <summary>Reads the superblock, the system files and the root directory</summary>
    /// <param name="imagePlugin">Image to read from</param>
    /// <param name="partition">Partition that holds the volume</param>
    /// <param name="encoding">Encoding for names and comments, Latin-1 if <c>null</c></param>
    /// <returns>Error number</returns>
    ErrorNumber Initialize(IMediaImage imagePlugin, Partition partition, Encoding encoding)
    {
        _imagePlugin = imagePlugin;
        _partition   = partition;
        _encoding    = encoding ?? Encoding.GetEncoding("iso-8859-1");
        _nodeFile    = null;
        _dirFile     = null;
        _miscFile    = null;
        _directoryCache.Clear();

        ErrorNumber errno = ReadSuperBlock(imagePlugin, partition, out _superBlock, out _);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading the superblock: {0}", errno);

            return errno;
        }

        _blockSize     = _superBlock.blockSize;
        _caseSensitive = (_superBlock.flags & VolumeFlags.CaseSensitive) != 0;
        _nodesPerBlock = (_blockSize - NODECONTAINER_HEADER_SIZE) / NODE_SIZE;

        AaruLogging.Debug(MODULE_NAME, "Block size: {0}, total blocks: {1}", _blockSize, _superBlock.totalBlocks);

        // A clean journal starts with TROK, anything else is a transaction that the filesystem replays on mount
        errno = ReadBlock(_imagePlugin, _partition, _blockSize, JOURNAL_BLOCK, out byte[] journal);

        _unfinishedTransaction = errno == ErrorNumber.NoError &&
                                 BigEndianBitConverter.ToUInt32(journal, 0) != TRANSACTION_OK_ID;

        if(_unfinishedTransaction) AaruLogging.Debug(MODULE_NAME, "The journal holds an unfinished transaction");

        // The volume bitmap starts at block 0x800, and :node: starts on the next 128 blocks boundary after it
        ulong bitmapBits   = ((ulong)_superBlock.totalBlocks + (1u << EXTENT_UNIT_SHIFT) - 1) >> EXTENT_UNIT_SHIFT;
        ulong bitsPerBlock = (ulong)(_blockSize - BITMAP_HEADER_SIZE) * 8;
        ulong bitmapBlocks = (bitmapBits + bitsPerBlock - 1) / bitsPerBlock;
        ulong unit         = 1u << EXTENT_UNIT_SHIFT;

        _nodeFileStart = BITMAP_START + (bitmapBlocks + unit - 1) / unit * unit;

        errno = LoadSystemFile(NODE_FILE_NODE, out _nodeFile);

        if(errno != ErrorNumber.NoError) return errno;

        errno = LoadSystemFile(MISC_FILE_NODE, out List<Extent> misc);

        if(errno != ErrorNumber.NoError) return errno;

        _miscFile = misc;

        errno = LoadSystemFile(DIR_FILE_NODE, out _dirFile);

        if(errno != ErrorNumber.NoError) return errno;

        errno = ReadNode(ROOT_NODE, out Node rootNode);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading the root node: {0}", errno);

            return errno;
        }

        if((rootNode.Flags & NodeFlags.Directory) == 0) return ErrorNumber.InvalidArgument;

        // The root directory entry is the first one in the first directory container, and its name is the volume name
        errno = ReadSystemFileBlock(_dirFile, 1, OBJECTCONTAINER_ID, out byte[] rootContainer);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading the root directory container: {0}", errno);

            return errno;
        }

        _freeBlocks = BigEndianBitConverter.ToUInt32(rootContainer, (int)_blockSize - ROOT_FREE_BLOCKS_FROM_END);

        string volumeName = "";

        if(BigEndianBitConverter.ToUInt32(rootContainer, OBJECTCONTAINER_HEADER_SIZE) == ROOT_KEY)
            volumeName = ReadName(rootContainer, OBJECTCONTAINER_HEADER_SIZE + 8, out _);

        _rootEntry = new DirectoryEntry
        {
            Key  = ROOT_KEY,
            Name = volumeName,
            Node = rootNode
        };

        AaruLogging.Debug(MODULE_NAME, "Volume name: {0}, free blocks: {1}", volumeName, _freeBlocks);

        return ErrorNumber.NoError;
    }

    /// <summary>Reads the superblock and checks it</summary>
    /// <param name="imagePlugin">Image to read from</param>
    /// <param name="partition">Partition that holds the volume</param>
    /// <param name="superBlock">The superblock</param>
    /// <param name="data">The superblock block contents</param>
    /// <returns>Error number</returns>
    static ErrorNumber ReadSuperBlock(IMediaImage imagePlugin, Partition partition, out SuperBlock superBlock,
                                      out byte[] data)
    {
        superBlock = default(SuperBlock);
        data       = null;

        if(partition.Start >= partition.End) return ErrorNumber.InvalidArgument;

        ErrorNumber errno = imagePlugin.ReadSector(partition.Start, false, out byte[] sector, out _);

        if(errno != ErrorNumber.NoError) return errno;

        if(sector.Length < Marshal.SizeOf<SuperBlock>()) return ErrorNumber.InvalidArgument;

        superBlock = Marshal.ByteArrayToStructureBigEndian<SuperBlock>(sector);

        if(superBlock.magic != JXFS_MAGIC || superBlock.version != JXFS_VERSION) return ErrorNumber.InvalidArgument;

        uint blockSize = superBlock.blockSize;

        // The block size is a power of two, at least big enough to hold a node container
        if(blockSize < 256 || blockSize > 65536 || (blockSize & blockSize - 1) != 0)
            return ErrorNumber.InvalidArgument;

        ulong partitionBytes = (partition.End - partition.Start + 1) * imagePlugin.Info.SectorSize;

        if((ulong)superBlock.totalBlocks * blockSize > partitionBytes) return ErrorNumber.InvalidArgument;

        errno = ReadBlock(imagePlugin, partition, blockSize, 0, out data);

        if(errno != ErrorNumber.NoError) return errno;

        if(!ValidChecksum(data) || superBlock.ownBlock != 0) return ErrorNumber.InvalidArgument;

        return ErrorNumber.NoError;
    }

    /// <summary>Reads the node of a system file and gets its extents</summary>
    /// <param name="number">System file node</param>
    /// <param name="extents">Extents of the system file</param>
    /// <returns>Error number</returns>
    ErrorNumber LoadSystemFile(uint number, out List<Extent> extents)
    {
        extents = null;

        ErrorNumber errno = ReadNode(number, out Node node);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading system file node {0}: {1}", number, errno);

            return errno;
        }

        errno = GetExtents(node, out extents);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading the extents of system file node {0}: {1}", number, errno);

            return errno;
        }

        if(extents.Count != 0) return ErrorNumber.NoError;

        AaruLogging.Debug(MODULE_NAME, "System file node {0} has no extents", number);

        return ErrorNumber.InvalidArgument;
    }

    /// <summary>Builds the filesystem metadata of an initialized volume</summary>
    /// <returns>The metadata</returns>
    FileSystem BuildMetadata()
    {
        return new FileSystem
        {
            Type             = FS_TYPE,
            VolumeName       = _rootEntry.Name,
            Clusters         = _superBlock.totalBlocks,
            ClusterSize      = _blockSize,
            FreeClusters     = _freeBlocks,
            CreationDate     = _amigaEpoch.AddSeconds(_superBlock.creationDate),
            ModificationDate = TicksToDateTime(_rootEntry.Node.Date),
            Dirty            = _unfinishedTransaction
        };
    }
}