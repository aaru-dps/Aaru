// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
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
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class SFS
{
    /// <summary>Amiga epoch: January 1, 1978</summary>
    static readonly DateTime _amigaEpoch = new(1978, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public ErrorNumber ReadLink(string path, out string dest)
    {
        dest = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize the path
        string normalizedPath = path ?? "";

        if(normalizedPath == "" || normalizedPath == "." || normalizedPath == "/") return ErrorNumber.InvalidArgument;

        // Get file stat to validate it's a symlink
        ErrorNumber errno = Stat(normalizedPath, out FileEntryInfo stat);

        if(errno != ErrorNumber.NoError) return errno;

        // Verify it's a symbolic link
        if(!stat.Attributes.HasFlag(FileAttributes.Symlink)) return ErrorNumber.InvalidArgument;

        // Get the object node for this path
        errno = GetObjectNodeForPath(normalizedPath, out uint objectNode);

        if(errno != ErrorNumber.NoError) return errno;

        // Find the object container for this node
        errno = FindObjectNode(objectNode, out uint objectBlock);

        if(errno != ErrorNumber.NoError) return errno;

        errno = ReadBlock(objectBlock, out byte[] objectData);

        if(errno != ErrorNumber.NoError) return errno;

        // Find the object in the container
        errno = FindObjectInContainer(objectData, objectNode, out int objectOffset);

        if(errno != ErrorNumber.NoError) return errno;

        // Read the softlink block pointer (data field at offset 12)
        // For soft links, the data field points to a SOFTLINK_ID block
        var softLinkBlock = BigEndianBitConverter.ToUInt32(objectData, objectOffset + 12);

        if(softLinkBlock == 0) return ErrorNumber.InvalidArgument;

        // Read the soft link block
        errno = ReadBlock(softLinkBlock, out byte[] linkData);

        if(errno != ErrorNumber.NoError) return errno;

        // Validate block ID
        var blockId = BigEndianBitConverter.ToUInt32(linkData, 0);

        if(blockId != SOFTLINK_ID) return ErrorNumber.InvalidArgument;

        // SoftLink structure: header (12) + parent (4) + next (4) + previous (4) = 24 bytes
        // Followed by null-terminated path string
        const int stringOffset = 24;

        // Read null-terminated string
        var pathBytes = new List<byte>();

        for(int i = stringOffset; i < linkData.Length && linkData[i] != 0; i++) pathBytes.Add(linkData[i]);

        if(pathBytes.Count == 0) return ErrorNumber.InvalidArgument;

        dest = _encoding.GetString(pathBytes.ToArray());

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber OpenFile(string path, out IFileNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize the path
        string normalizedPath = path ?? "";

        if(normalizedPath == "" || normalizedPath == "." || normalizedPath == "/") return ErrorNumber.IsDirectory;

        // Get file stat to validate it's a file and get its size
        ErrorNumber errno = Stat(normalizedPath, out FileEntryInfo stat);

        if(errno != ErrorNumber.NoError) return errno;

        // Verify it's a regular file, not a directory
        if(stat.Attributes.HasFlag(FileAttributes.Directory)) return ErrorNumber.IsDirectory;

        // Get the object node for this path
        errno = GetObjectNodeForPath(normalizedPath, out uint objectNode);

        if(errno != ErrorNumber.NoError) return errno;

        // Find the object container for this node
        errno = FindObjectNode(objectNode, out uint objectBlock);

        if(errno != ErrorNumber.NoError) return errno;

        errno = ReadBlock(objectBlock, out byte[] objectData);

        if(errno != ErrorNumber.NoError) return errno;

        // Find the object in the container
        errno = FindObjectInContainer(objectData, objectNode, out int objectOffset);

        if(errno != ErrorNumber.NoError) return errno;

        // Read the first extent block (data field at offset 12)
        var firstExtent = BigEndianBitConverter.ToUInt32(objectData, objectOffset + 12);

        // Create file node
        node = new SfsFileNode
        {
            Path          = normalizedPath,
            Length        = stat.Length,
            Offset        = 0,
            FirstExtent   = firstExtent,
            CurrentExtent = firstExtent,
            ExtentOffset  = 0
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not SfsFileNode) return ErrorNumber.InvalidArgument;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(buffer is null || buffer.Length < length) return ErrorNumber.InvalidArgument;

        if(node is not SfsFileNode sfsNode) return ErrorNumber.InvalidArgument;

        if(sfsNode.Offset < 0 || length < 0) return ErrorNumber.InvalidArgument;

        // If at or past end of file, return zero bytes read
        if(sfsNode.Offset >= sfsNode.Length) return ErrorNumber.NoError;

        // Adjust length to not read past end of file
        long bytesToRead = length;

        if(sfsNode.Offset + bytesToRead > sfsNode.Length) bytesToRead = sfsNode.Length - sfsNode.Offset;

        if(bytesToRead == 0) return ErrorNumber.NoError;

        // Seek to the correct extent if needed
        ErrorNumber errno = SeekToOffset(sfsNode);

        if(errno != ErrorNumber.NoError) return errno;

        // Read data from extent chain
        long bufferOffset = 0;

        while(bytesToRead > 0 && sfsNode.CurrentExtent != 0)
        {
            // Find the current extent B-node
            errno = FindExtentBNode(sfsNode.CurrentExtent,
                                    out uint extentKey,
                                    out uint extentNext,
                                    out uint extentBlocks);

            if(errno != ErrorNumber.NoError) return errno;

            // Calculate how many bytes are available in this extent
            long extentSize      = (long)extentBlocks << _blockShift;
            long bytesInExtent   = extentSize - sfsNode.ExtentOffset;
            long bytesFromExtent = Math.Min(bytesToRead, bytesInExtent);

            // Calculate starting block and offset within the extent
            long offsetInExtent = sfsNode.ExtentOffset;
            uint startBlock     = extentKey + (uint)(offsetInExtent >> _blockShift);
            var  offsetInBlock  = (int)(offsetInExtent & _blockSize - 1);

            // Read blocks
            while(bytesFromExtent > 0)
            {
                errno = ReadBlock(startBlock, out byte[] blockData);

                if(errno != ErrorNumber.NoError) return errno;

                var bytesFromBlock = (int)Math.Min(bytesFromExtent, _blockSize - offsetInBlock);
                Array.Copy(blockData, offsetInBlock, buffer, bufferOffset, bytesFromBlock);

                bufferOffset         += bytesFromBlock;
                bytesFromExtent      -= bytesFromBlock;
                bytesToRead          -= bytesFromBlock;
                read                 += bytesFromBlock;
                sfsNode.Offset       += bytesFromBlock;
                sfsNode.ExtentOffset += bytesFromBlock;

                offsetInBlock = 0;
                startBlock++;
            }

            // Move to next extent if we've exhausted this one
            if(sfsNode.ExtentOffset >= extentSize && extentNext != 0)
            {
                sfsNode.CurrentExtent = extentNext;
                sfsNode.ExtentOffset  = 0;
            }
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize the path
        string normalizedPath = path ?? "/";

        if(normalizedPath == "" || normalizedPath == ".") normalizedPath = "/";

        // Root directory handling
        if(normalizedPath == "/" || string.Equals(normalizedPath, "/", StringComparison.OrdinalIgnoreCase))
        {
            // Read root object to get its metadata
            ErrorNumber errno = FindObjectNode(ROOTNODE, out uint rootBlock);

            if(errno != ErrorNumber.NoError) return errno;

            errno = ReadBlock(rootBlock, out byte[] rootData);

            if(errno != ErrorNumber.NoError) return errno;

            errno = FindObjectInContainer(rootData, ROOTNODE, out int rootOffset);

            if(errno != ErrorNumber.NoError) return errno;

            return StatFromObjectData(rootData, rootOffset, ROOTNODE, out stat);
        }

        // Remove leading slash and split path
        string pathWithoutLeadingSlash = normalizedPath.StartsWith("/", StringComparison.Ordinal)
                                             ? normalizedPath[1..]
                                             : normalizedPath;

        string[] pathComponents = pathWithoutLeadingSlash.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if(pathComponents.Length == 0) return ErrorNumber.InvalidArgument;

        // Start traversal from root directory cache
        Dictionary<string, uint> currentDirectory = _rootDirectoryCache;
        uint                     targetNode       = 0;

        // Traverse all path components
        for(var i = 0; i < pathComponents.Length; i++)
        {
            string component = pathComponents[i];

            // Find the component in current directory (handle case sensitivity)
            string foundKey = null;

            foreach(string key in currentDirectory.Keys)
            {
                if(string.Equals(key,
                                 component,
                                 _caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
                {
                    foundKey = key;

                    break;
                }
            }

            if(foundKey == null) return ErrorNumber.NoSuchFile;

            targetNode = currentDirectory[foundKey];

            // If this is the last component, we found our target
            if(i == pathComponents.Length - 1) break;

            // Not the last component - read directory contents for next iteration
            ErrorNumber errno = ReadDirectoryContents(targetNode, out Dictionary<string, uint> childEntries);

            if(errno != ErrorNumber.NoError) return errno;

            currentDirectory = childEntries;
        }

        // Read the target object's metadata
        ErrorNumber findErr = FindObjectNode(targetNode, out uint objectBlock);

        if(findErr != ErrorNumber.NoError) return findErr;

        findErr = ReadBlock(objectBlock, out byte[] objectData);

        if(findErr != ErrorNumber.NoError) return findErr;

        findErr = FindObjectInContainer(objectData, targetNode, out int objectOffset);

        if(findErr != ErrorNumber.NoError) return findErr;

        return StatFromObjectData(objectData, objectOffset, targetNode, out stat);
    }

    /// <summary>Seeks to the correct extent for the current file offset</summary>
    /// <param name="node">The file node</param>
    /// <returns>Error code indicating success or failure</returns>
    ErrorNumber SeekToOffset(SfsFileNode node)
    {
        // If we're before the current extent position, restart from the beginning
        long currentExtentStart = node.Offset - node.ExtentOffset;

        if(node.Offset < currentExtentStart || node.CurrentExtent == 0)
        {
            node.CurrentExtent = node.FirstExtent;
            node.ExtentOffset  = 0;
            currentExtentStart = 0;
        }

        // Walk the extent chain to find the correct extent
        while(node.CurrentExtent != 0)
        {
            ErrorNumber errno = FindExtentBNode(node.CurrentExtent, out _, out uint extentNext, out uint extentBlocks);

            if(errno != ErrorNumber.NoError) return errno;

            long extentSize = (long)extentBlocks << _blockShift;
            long extentEnd  = currentExtentStart + extentSize;

            if(node.Offset < extentEnd)
            {
                // Found the correct extent
                node.ExtentOffset = node.Offset - currentExtentStart;

                return ErrorNumber.NoError;
            }

            // Move to next extent
            currentExtentStart = extentEnd;
            node.CurrentExtent = extentNext;
            node.ExtentOffset  = 0;
        }

        return ErrorNumber.InvalidArgument;
    }

    /// <summary>Finds an extent B-node by key in the extent B-tree</summary>
    /// <param name="key">The extent key (block number)</param>
    /// <param name="extentKey">Output: the extent's data block start</param>
    /// <param name="extentNext">Output: the next extent in the chain</param>
    /// <param name="extentBlocks">Output: number of blocks in this extent</param>
    /// <returns>Error code indicating success or failure</returns>
    ErrorNumber FindExtentBNode(uint key, out uint extentKey, out uint extentNext, out uint extentBlocks)
    {
        extentKey    = 0;
        extentNext   = 0;
        extentBlocks = 0;

        // Start at the extent B-tree root
        uint currentBlock = _rootBlock.extentbnoderoot;

        while(true)
        {
            ErrorNumber errno = ReadBlock(currentBlock, out byte[] blockData);

            if(errno != ErrorNumber.NoError) return errno;

            // Validate block ID
            var blockId = BigEndianBitConverter.ToUInt32(blockData, 0);

            if(blockId != BNODECONTAINER_ID) return ErrorNumber.InvalidArgument;

            // BTreeContainer starts at offset 12 (after header)
            // nodecount (2) + isleaf (1) + nodesize (1)
            var  nodeCount = BigEndianBitConverter.ToUInt16(blockData, 12);
            byte isLeaf    = blockData[14];
            byte nodeSize  = blockData[15];

            if(nodeCount == 0) return ErrorNumber.InvalidArgument;

            // Search for the key in this container
            // BNode entries start at offset 16
            var nodeOffset = 16;

            if(isLeaf != 0)
            {
                // Leaf node - contains ExtentBNode structures
                // SFS\0: key (4) + next (4) + prev (4) + blocks (2) = 14 bytes
                // SFS\2: key (4) + next (4) + prev (4) + blocks (4) = 16 bytes
                // nodeSize field tells us the actual size
                for(var i = 0; i < nodeCount; i++)
                {
                    var nodeKey = BigEndianBitConverter.ToUInt32(blockData, nodeOffset);

                    if(nodeKey == key)
                    {
                        extentKey  = nodeKey;
                        extentNext = BigEndianBitConverter.ToUInt32(blockData, nodeOffset + 4);

                        // Read blocks field - 16-bit in SFS\0 (nodeSize=14), 32-bit in SFS\2 (nodeSize=16)
                        extentBlocks = nodeSize >= 16
                                           ? BigEndianBitConverter.ToUInt32(blockData, nodeOffset + 12)
                                           : BigEndianBitConverter.ToUInt16(blockData, nodeOffset + 12);

                        return ErrorNumber.NoError;
                    }

                    nodeOffset += nodeSize;
                }

                // Key not found
                return ErrorNumber.InvalidArgument;
            }

            // Index node - find the child to descend into
            // BNode: key (4) + data (4) = 8 bytes
            uint childBlock = 0;

            for(int i = nodeCount - 1; i >= 0; i--)
            {
                int entryOffset = nodeOffset + i * nodeSize;
                var nodeKey     = BigEndianBitConverter.ToUInt32(blockData, entryOffset);

                if(key >= nodeKey)
                {
                    childBlock = BigEndianBitConverter.ToUInt32(blockData, entryOffset + 4);

                    break;
                }
            }

            if(childBlock == 0)
            {
                // Use first entry
                childBlock = BigEndianBitConverter.ToUInt32(blockData, nodeOffset + 4);
            }

            currentBlock = childBlock;
        }
    }

    /// <summary>Creates a FileEntryInfo from raw object data in an ObjectContainer</summary>
    /// <param name="objectData">The ObjectContainer block data</param>
    /// <param name="objectOffset">Offset to the object within the container</param>
    /// <param name="objectNode">The object's node number</param>
    /// <param name="stat">Output file entry information</param>
    /// <returns>Error code indicating success or failure</returns>
    ErrorNumber StatFromObjectData(byte[] objectData, int objectOffset, uint objectNode, out FileEntryInfo stat)
    {
        stat = null;

        // Object structure (from objects.h):
        // SFS\0 (25 bytes):
        //   owneruid (2) + ownergid (2) + objectnode (4) + protection (4) +
        //   data/hashtable (4) + size/firstdirblock (4) + datemodified (4) + bits (1)
        // SFS\2 (27 bytes):
        //   owneruid (2) + ownergid (2) + objectnode (4) + protection (4) + data/hashtable (4) +
        //   sizeh (2) + size (4) for files, or firstdirblock (4) + pad (2) for directories +
        //   datemodified (4) + bits (1)

        if(objectOffset + _objectSize > objectData.Length) return ErrorNumber.InvalidArgument;

        ushort ownerUid   = BigEndianBitConverter.ToUInt16(objectData, objectOffset);
        ushort ownerGid   = BigEndianBitConverter.ToUInt16(objectData, objectOffset + 2);
        uint   protection = BigEndianBitConverter.ToUInt32(objectData, objectOffset + 8);

        long       fileSize;
        uint       dateModified;
        ObjectBits bits;

        if(_isSfs2)
        {
            // SFS\2: 48-bit size as sizeh (high 16 bits) at 16 and size (low 32 bits) at 18,
            // datemodified at 22, bits at 26
            ushort sizeh = BigEndianBitConverter.ToUInt16(objectData, objectOffset + 16);
            uint   size  = BigEndianBitConverter.ToUInt32(objectData, objectOffset + 18);

            fileSize     = (long)sizeh << 32 | size;
            dateModified = BigEndianBitConverter.ToUInt32(objectData, objectOffset + 22);
            bits         = (ObjectBits)objectData[objectOffset + 26];
        }
        else
        {
            // SFS\0: size at 16, datemodified at 20, bits at 24
            fileSize     = BigEndianBitConverter.ToUInt32(objectData, objectOffset + 16);
            dateModified = BigEndianBitConverter.ToUInt32(objectData, objectOffset + 20);
            bits         = (ObjectBits)objectData[objectOffset + 24];
        }

        // Determine file attributes
        FileAttributes attributes = FileAttributes.None;

        if((bits & ObjectBits.Directory) != 0)
            attributes |= FileAttributes.Directory;
        else
            attributes |= FileAttributes.File;

        if((bits & ObjectBits.Hidden) != 0) attributes |= FileAttributes.Hidden;

        if((bits & ObjectBits.Link) != 0 && (bits & ObjectBits.HardLink) == 0) attributes |= FileAttributes.Symlink;

        // SFS protection bits: opposite of AmigaDOS
        // Default is 0x0000000F (R, W, E, D set)
        // If write bit (bit 1) is NOT set, file is read-only
        if((protection & 0x02) == 0) attributes |= FileAttributes.ReadOnly;

        // Calculate file size and blocks
        long length = 0;
        long blocks = 0;

        if((bits & ObjectBits.Directory) == 0)
        {
            length = fileSize;
            blocks = (length + _blockSize - 1) / _blockSize;
        }

        // Convert SFS timestamp (seconds since 1-1-1978) to DateTime
        DateTime? lastWriteTimeUtc = _amigaEpoch.AddSeconds(dateModified);

        stat = new FileEntryInfo
        {
            Attributes       = attributes,
            Inode            = objectNode,
            Length           = length,
            Blocks           = blocks,
            BlockSize        = _blockSize,
            Links            = 1, // SFS doesn't track hard link count in the object
            UID              = ownerUid,
            GID              = ownerGid,
            Mode             = protection,
            LastWriteTimeUtc = lastWriteTimeUtc
        };

        return ErrorNumber.NoError;
    }
}