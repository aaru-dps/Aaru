// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Object.cs
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
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements the Smart File System</summary>
public sealed partial class SFS
{
    /// <summary>Reads an object's name from object data</summary>
    /// <param name="objectContainerData">The object container block data</param>
    /// <param name="objectOffset">Offset to the object within the container</param>
    /// <returns>The object name</returns>
    string ReadObjectName(byte[] objectContainerData, int objectOffset)
    {
        // Object structure: skip header fields to reach the name
        // SFS\0: ownerUid(2) + ownerGid(2) + objectNode(4) + protection(4) +
        //        dataOrHashtable(4) + sizeOrFirstDirBlock(4) + dateModified(4) + bits(1) = 25 bytes
        // SFS\2: Same + sizeh(2) = 27 bytes
        // The name immediately follows the fixed part of the object
        int nameOffset = objectOffset + _objectSize;

        // Read null-terminated string
        var nameBytes = new List<byte>();

        for(int i = nameOffset; i < objectContainerData.Length && objectContainerData[i] != 0; i++)
            nameBytes.Add(objectContainerData[i]);

        return _encoding.GetString(nameBytes.ToArray());
    }

    /// <summary>Finds an object node's block address</summary>
    /// <param name="nodeNumber">The node number to find</param>
    /// <param name="blockAddress">The block address of the object container</param>
    /// <returns>Error code indicating success or failure</returns>
    ErrorNumber FindObjectNode(uint nodeNumber, out uint blockAddress)
    {
        blockAddress = 0;
        uint nodeIndex = _objectNodeRoot;

        // Index node container entries are block numbers stored shifted left.
        // SFS\0: shift = log2(block size) - BLCKFACCURACY
        // SFS\2: shift = 1
        int shiftsBlock32 = _isSfs2 ? NODE_SHIFT_SFS2 : _blockShift - BLCKFACCURACY;

        if(shiftsBlock32 < 0) shiftsBlock32 = 0;

        while(true)
        {
            ErrorNumber errno = ReadBlock(nodeIndex, out byte[] nodeData);

            if(errno != ErrorNumber.NoError) return errno;

            // Validate node container
            var id = BigEndianBitConverter.ToUInt32(nodeData, 0);

            if(id != NODECONTAINER_ID)
            {
                AaruLogging.Debug(MODULE_NAME, "Invalid node container ID: 0x{0:X8}", id);

                return ErrorNumber.InvalidArgument;
            }

            // NodeContainer: header(12) + nodeNumber(4) + nodes(4)
            var nodeContainerNodeNumber = BigEndianBitConverter.ToUInt32(nodeData, 12);
            var nodesPerContainer       = BigEndianBitConverter.ToUInt32(nodeData, 16);

            if(nodesPerContainer == 1)
            {
                // This is a leaf node container - find the actual node
                // ObjectNode: data(4) + next(4) + hash16(2) = 10 bytes
                const int objectNodeSize = 10;
                uint      nodeOffset     = (nodeNumber - nodeContainerNodeNumber) * objectNodeSize;
                int       dataOffset     = 20 + (int)nodeOffset; // After header

                if(dataOffset + 4 > nodeData.Length)
                {
                    AaruLogging.Debug(MODULE_NAME, "Node offset out of bounds");

                    return ErrorNumber.InvalidArgument;
                }

                blockAddress = BigEndianBitConverter.ToUInt32(nodeData, dataOffset);

                return ErrorNumber.NoError;
            }

            // Index node - descend to the appropriate child
            uint containerEntry = (nodeNumber - nodeContainerNodeNumber) / nodesPerContainer;
            int  entryOffset    = 20 + (int)containerEntry * 4;

            if(entryOffset + 4 > nodeData.Length)
            {
                AaruLogging.Debug(MODULE_NAME, "Container entry offset out of bounds");

                return ErrorNumber.InvalidArgument;
            }

            var entryValue = BigEndianBitConverter.ToUInt32(nodeData, entryOffset);

            // Extract block number from the entry (stored shifted)
            nodeIndex = entryValue >> shiftsBlock32;
        }
    }

    /// <summary>Finds an object in an object container</summary>
    /// <param name="containerData">The container data</param>
    /// <param name="objectNode">The object node number to find</param>
    /// <param name="objectOffset">The offset to the object in the container</param>
    /// <returns>Error code indicating success or failure</returns>
    ErrorNumber FindObjectInContainer(byte[] containerData, uint objectNode, out int objectOffset)
    {
        // Start after the ObjectContainer header: header(12) + parent(4) + next(4) + previous(4) = 24 bytes
        objectOffset = 24;
        int endOffset = (int)_blockSize - _objectSize - 2;

        while(objectOffset < endOffset)
        {
            // Check if there's a valid object (name[0] != 0)
            int nameOffset = objectOffset + _objectSize;

            if(nameOffset >= containerData.Length || containerData[nameOffset] == 0) break;

            // Read object node number
            var nodeNum = BigEndianBitConverter.ToUInt32(containerData, objectOffset + 4);

            if(nodeNum == objectNode) return ErrorNumber.NoError;

            // Move to next object
            objectOffset = GetNextObjectOffset(containerData, objectOffset);

            if(objectOffset < 0) break;
        }

        return ErrorNumber.NoData;
    }

    /// <summary>Gets the offset to the next object in a container</summary>
    /// <param name="containerData">The container data</param>
    /// <param name="currentOffset">Current object offset</param>
    /// <returns>Offset to next object, or -1 if none</returns>
    int GetNextObjectOffset(byte[] containerData, int currentOffset)
    {
        // Skip to name field
        int nameOffset = currentOffset + _objectSize;

        if(nameOffset >= containerData.Length) return -1;

        // Skip name (null-terminated)
        while(nameOffset < containerData.Length && containerData[nameOffset] != 0) nameOffset++;

        nameOffset++; // Skip null terminator

        // Skip comment (null-terminated)
        while(nameOffset < containerData.Length && containerData[nameOffset] != 0) nameOffset++;

        nameOffset++; // Skip null terminator

        // Align to word boundary
        if((nameOffset & 1) != 0) nameOffset++;

        return nameOffset;
    }

    /// <summary>Parses objects in a container and adds them to the directory cache</summary>
    /// <param name="containerData">The container data</param>
    void ParseObjectContainer(byte[] containerData)
    {
        // Start after the ObjectContainer header: header(12) + parent(4) + next(4) + previous(4) = 24 bytes
        var offset    = 24;
        int endOffset = (int)_blockSize - _objectSize - 2;

        while(offset < endOffset && offset < containerData.Length - _objectSize)
        {
            // Check if there's a valid object (name[0] != 0)
            int nameOffset = offset + _objectSize;

            if(nameOffset >= containerData.Length || containerData[nameOffset] == 0) break;

            // Read object node number
            var objectNode = BigEndianBitConverter.ToUInt32(containerData, offset + 4);

            // Read name
            string name = ReadObjectName(containerData, offset);

            if(!string.IsNullOrEmpty(name) && !_rootDirectoryCache.ContainsKey(name))
            {
                _rootDirectoryCache[name] = objectNode;
                AaruLogging.Debug(MODULE_NAME, "Found entry: {0} -> node {1}", name, objectNode);
            }

            // Move to next object
            offset = GetNextObjectOffset(containerData, offset);

            if(offset < 0) break;
        }
    }

    /// <summary>Parses objects in a container and adds them to a dictionary</summary>
    /// <param name="containerData">The container data</param>
    /// <param name="entries">Dictionary to add entries to</param>
    void ParseObjectContainerToDict(byte[] containerData, Dictionary<string, uint> entries)
    {
        // Start after the ObjectContainer header: header(12) + parent(4) + next(4) + previous(4) = 24 bytes
        var offset    = 24;
        int endOffset = (int)_blockSize - _objectSize - 2;

        while(offset < endOffset && offset < containerData.Length - _objectSize)
        {
            // Check if there's a valid object (name[0] != 0)
            int nameOffset = offset + _objectSize;

            if(nameOffset >= containerData.Length || containerData[nameOffset] == 0) break;

            // Read object node number
            var objectNode = BigEndianBitConverter.ToUInt32(containerData, offset + 4);

            // Read name
            string name = ReadObjectName(containerData, offset);

            if(!string.IsNullOrEmpty(name) && !entries.ContainsKey(name)) entries[name] = objectNode;

            // Move to next object
            offset = GetNextObjectOffset(containerData, offset);

            if(offset < 0) break;
        }
    }

    /// <summary>Gets the object node number for a given path</summary>
    /// <param name="path">The path to resolve</param>
    /// <param name="objectNode">Output object node number</param>
    /// <returns>Error code indicating success or failure</returns>
    ErrorNumber GetObjectNodeForPath(string path, out uint objectNode)
    {
        objectNode = 0;

        // Root directory
        if(path == "/" || string.Equals(path, "/", StringComparison.OrdinalIgnoreCase))
        {
            objectNode = ROOTNODE;

            return ErrorNumber.NoError;
        }

        // Remove leading slash and split path
        string pathWithoutLeadingSlash = path.StartsWith("/", StringComparison.Ordinal) ? path[1..] : path;

        string[] pathComponents = pathWithoutLeadingSlash.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if(pathComponents.Length == 0) return ErrorNumber.InvalidArgument;

        // Start traversal from root directory cache
        Dictionary<string, uint> currentDirectory = _rootDirectoryCache;

        // Traverse all path components
        for(var i = 0; i < pathComponents.Length; i++)
        {
            string component = pathComponents[i];

            // Find the component in current directory (handle case sensitivity)
            string foundKey =
                currentDirectory.Keys.FirstOrDefault(key => string.Equals(key,
                                                                          component,
                                                                          _caseSensitive
                                                                              ? StringComparison.Ordinal
                                                                              : StringComparison.OrdinalIgnoreCase));

            if(foundKey == null) return ErrorNumber.NoSuchFile;

            objectNode = currentDirectory[foundKey];

            // If this is the last component, we found our target
            if(i == pathComponents.Length - 1) return ErrorNumber.NoError;

            // Not the last component - read directory contents for next iteration
            ErrorNumber errno = ReadDirectoryContents(objectNode, out Dictionary<string, uint> childEntries);

            if(errno != ErrorNumber.NoError) return errno;

            currentDirectory = childEntries;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Reads the comment for an object given its node number</summary>
    /// <param name="objectNode">The object node number</param>
    /// <param name="comment">Output comment string</param>
    /// <returns>Error code indicating success or failure</returns>
    ErrorNumber ReadObjectComment(uint objectNode, out string comment)
    {
        comment = null;

        // Find the object container for this node
        ErrorNumber errno = FindObjectNode(objectNode, out uint objectBlock);

        if(errno != ErrorNumber.NoError) return errno;

        errno = ReadBlock(objectBlock, out byte[] objectData);

        if(errno != ErrorNumber.NoError) return errno;

        // Find the object in the container
        errno = FindObjectInContainer(objectData, objectNode, out int objectOffset);

        if(errno != ErrorNumber.NoError) return errno;

        // Object structure: fixed fields + name (null-terminated) + comment (null-terminated)
        // SFS\0: 25 bytes, SFS\2: 27 bytes
        // Skip to name field
        int nameOffset = objectOffset + _objectSize;

        if(nameOffset >= objectData.Length) return ErrorNumber.InvalidArgument;

        // Skip the name (null-terminated string)
        int commentOffset = nameOffset;

        while(commentOffset < objectData.Length && objectData[commentOffset] != 0) commentOffset++;

        commentOffset++; // Skip null terminator

        if(commentOffset >= objectData.Length) return ErrorNumber.NoError; // No comment

        // Read the comment (null-terminated string)
        var commentBytes = new List<byte>();

        while(commentOffset < objectData.Length && objectData[commentOffset] != 0)
            commentBytes.Add(objectData[commentOffset++]);

        if(commentBytes.Count > 0) comment = _encoding.GetString(commentBytes.ToArray());

        return ErrorNumber.NoError;
    }
}