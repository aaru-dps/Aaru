// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple ProDOS filesystem plugin.
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
using Aaru.Logging;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class ProDOSPlugin
{
    /// <inheritdoc />
    public ErrorNumber OpenFile(string path, out IFileNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        AaruLogging.Debug(MODULE_NAME, "OpenFile: path='{0}'", path);

        // Get the entry for this path
        ErrorNumber errno = GetEntryForPath(path, out CachedEntry entry);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error getting entry for path: {0}", errno);

            return errno;
        }

        // Cannot open directories
        if(entry.IsDirectory)
        {
            AaruLogging.Debug(MODULE_NAME, "Cannot open directory as file");

            return ErrorNumber.IsDirectory;
        }

        // Get file length - for extended files, we need to read the extended key block
        long   fileLength;
        byte   storageType = entry.StorageType;
        ushort keyBlock    = entry.KeyBlock;

        if(storageType == EXTENDED_FILE_TYPE)
        {
            errno = ReadBlock(entry.KeyBlock, out byte[] extBlock);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Error reading extended key block: {0}", errno);

                return errno;
            }

            ExtendedKeyBlock extKeyBlock = Marshal.ByteArrayToStructureLittleEndian<ExtendedKeyBlock>(extBlock);

            // Data fork size
            fileLength = extKeyBlock.data_fork.eof[0]      |
                         extKeyBlock.data_fork.eof[1] << 8 |
                         extKeyBlock.data_fork.eof[2] << 16;

            // Use data fork's storage type and key block
            storageType = extKeyBlock.data_fork.storage_type;
            keyBlock    = extKeyBlock.data_fork.key_block;
        }
        else
            fileLength = entry.Eof;

        // Create file node
        var fileNode = new ProDosFileNode
        {
            Path   = path,
            Entry  = entry,
            Length = fileLength,
            Offset = 0,

            // Store effective storage type and key block (possibly from extended file's data fork)
            EffectiveStorageType = storageType,
            EffectiveKeyBlock    = keyBlock
        };

        // Pre-load index block(s) for sapling and tree files
        errno = LoadIndexBlocks(fileNode);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error loading index blocks: {0}", errno);

            return errno;
        }

        node = fileNode;

        AaruLogging.Debug(MODULE_NAME, "OpenFile successful: path='{0}', size={1}", path, fileLength);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(node is not ProDosFileNode proDosNode) return ErrorNumber.InvalidArgument;

        AaruLogging.Debug(MODULE_NAME, "CloseFile: path='{0}'", proDosNode.Path);

        // Clear cached data
        proDosNode.IndexBlock       = null;
        proDosNode.MasterIndexBlock = null;
        proDosNode.CachedBlockData  = null;
        proDosNode.CachedBlockIndex = -1;
        proDosNode.Offset           = -1;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not ProDosFileNode proDosNode) return ErrorNumber.InvalidArgument;

        if(proDosNode.Offset < 0) return ErrorNumber.InvalidArgument;

        if(buffer == null || buffer.Length < length) return ErrorNumber.InvalidArgument;

        AaruLogging.Debug(MODULE_NAME,
                          "ReadFile: path='{0}', offset={1}, length={2}",
                          proDosNode.Path,
                          proDosNode.Offset,
                          length);

        // Check if at or past EOF
        if(proDosNode.Offset >= proDosNode.Length)
        {
            AaruLogging.Debug(MODULE_NAME, "ReadFile: at EOF");

            return ErrorNumber.NoError;
        }

        // Adjust length if it would read past EOF
        long bytesToRead = length;

        if(proDosNode.Offset + bytesToRead > proDosNode.Length) bytesToRead = proDosNode.Length - proDosNode.Offset;

        if(bytesToRead <= 0) return ErrorNumber.NoError;

        // Read data block by block
        long bufferOffset = 0;

        while(bytesToRead > 0)
        {
            // Calculate which block we need
            var blockIndex    = (int)(proDosNode.Offset / 512);
            var offsetInBlock = (int)(proDosNode.Offset % 512);

            // Get the data block
            ErrorNumber errno = ReadFileBlock(proDosNode, blockIndex, out byte[] blockData);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Error reading file block {0}: {1}", blockIndex, errno);

                return errno;
            }

            // Calculate how much to copy from this block
            var bytesFromBlock = (int)Math.Min(bytesToRead, 512 - offsetInBlock);

            // Copy to output buffer
            Array.Copy(blockData, offsetInBlock, buffer, bufferOffset, bytesFromBlock);

            bufferOffset      += bytesFromBlock;
            proDosNode.Offset += bytesFromBlock;
            bytesToRead       -= bytesFromBlock;
            read              += bytesFromBlock;
        }

        AaruLogging.Debug(MODULE_NAME, "ReadFile successful: read {0} bytes, new offset={1}", read, proDosNode.Offset);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Handle root directory specially
        if(string.IsNullOrEmpty(path) || path == "/" || path == ".")
        {
            stat = new FileEntryInfo
            {
                Attributes   = FileAttributes.Directory,
                BlockSize    = 512,
                Blocks       = 4, // Volume directory is always 4 blocks
                CreationTime = _creationTime,
                Inode        = 2, // Root directory starts at block 2
                Links        = 1,
                Mode         = 0x16D, // drwxrw-r-x
                DeviceNo     = 0,
                GID          = 0,
                UID          = 0,
                Length       = 4 * 512
            };

            return ErrorNumber.NoError;
        }

        // Get the entry for this path
        ErrorNumber errno = GetEntryForPath(path, out CachedEntry entry);

        if(errno != ErrorNumber.NoError) return errno;

        stat = new FileEntryInfo
        {
            BlockSize     = 512,
            CreationTime  = entry.CreationTime,
            LastWriteTime = entry.ModificationTime,
            Links         = 1,
            DeviceNo      = 0,
            GID           = 0,
            UID           = 0
        };

        // Directory
        if(entry.IsDirectory)
        {
            stat.Attributes = FileAttributes.Directory;
            stat.Blocks     = entry.BlocksUsed;
            stat.Inode      = entry.KeyBlock;
            stat.Length     = entry.BlocksUsed * 512;
            stat.Mode       = 0x16D; // drwxrw-r-x

            return ErrorNumber.NoError;
        }

        // File
        stat.Inode = entry.KeyBlock;

        // Set attributes
        stat.Attributes = FileAttributes.File;

        if((entry.Access & READ_ATTRIBUTE) == 0) stat.Attributes |= FileAttributes.Hidden;

        if((entry.Access & WRITE_ATTRIBUTE) == 0) stat.Attributes |= FileAttributes.ReadOnly;

        if((entry.Access & BACKUP_ATTRIBUTE) != 0) stat.Attributes |= FileAttributes.Archive;

        // Calculate mode from access flags
        uint mode = 0x8000; // Regular file

        if((entry.Access & READ_ATTRIBUTE) != 0) mode |= 0x124; // r--r--r--

        if((entry.Access & WRITE_ATTRIBUTE) != 0) mode |= 0x92; // -w--w--w-

        stat.Mode = mode;

        // For extended files (with resource fork), read the extended key block to get data fork size
        if(entry.StorageType == EXTENDED_FILE_TYPE)
        {
            errno = ReadBlock(entry.KeyBlock, out byte[] extBlock);

            if(errno != ErrorNumber.NoError) return errno;

            ExtendedKeyBlock extKeyBlock = Marshal.ByteArrayToStructureLittleEndian<ExtendedKeyBlock>(extBlock);

            // Data fork size (per user requirement: data fork mandates file size)
            var dataForkEof = (uint)(extKeyBlock.data_fork.eof[0]      |
                                     extKeyBlock.data_fork.eof[1] << 8 |
                                     extKeyBlock.data_fork.eof[2] << 16);

            stat.Length = dataForkEof;
            stat.Blocks = extKeyBlock.data_fork.blocks_used;
        }
        else
        {
            // Non-extended file: use entry's EOF and blocks_used
            stat.Length = entry.Eof;
            stat.Blocks = entry.BlocksUsed;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Loads index blocks for sapling and tree storage types</summary>
    ErrorNumber LoadIndexBlocks(ProDosFileNode fileNode)
    {
        byte   storageType = fileNode.EffectiveStorageType;
        ushort keyBlock    = fileNode.EffectiveKeyBlock;

        switch(storageType)
        {
            case SEEDLING_FILE_TYPE:
                // Seedling: key block is the single data block, no index needed
                return ErrorNumber.NoError;

            case SAPLING_FILE_TYPE:
            {
                // Sapling: key block is an indirect block containing up to 256 block pointers
                ErrorNumber errno = ReadBlock(keyBlock, out byte[] indexBlock);

                if(errno != ErrorNumber.NoError) return errno;

                // Parse the indirect block (LSB in first 256 bytes, MSB in second 256 bytes)
                fileNode.IndexBlock = new ushort[256];

                for(var i = 0; i < 256; i++)
                    fileNode.IndexBlock[i] = (ushort)(indexBlock[i] | indexBlock[256 + i] << 8);

                return ErrorNumber.NoError;
            }

            case TREE_FILE_TYPE:
            {
                // Tree: key block is a master (double-indirect) block
                ErrorNumber errno = ReadBlock(keyBlock, out byte[] masterBlock);

                if(errno != ErrorNumber.NoError) return errno;

                // Parse the master index block
                fileNode.MasterIndexBlock = new ushort[256];

                for(var i = 0; i < 256; i++)
                    fileNode.MasterIndexBlock[i] = (ushort)(masterBlock[i] | masterBlock[256 + i] << 8);

                return ErrorNumber.NoError;
            }

            default:
                AaruLogging.Debug(MODULE_NAME, "Unknown storage type: 0x{0:X2}", storageType);

                return ErrorNumber.InvalidArgument;
        }
    }

    /// <summary>Reads a specific data block from a file</summary>
    ErrorNumber ReadFileBlock(ProDosFileNode fileNode, int blockIndex, out byte[] blockData)
    {
        blockData = null;

        // Check if this block is already cached
        if(fileNode.CachedBlockIndex == blockIndex && fileNode.CachedBlockData != null)
        {
            blockData = fileNode.CachedBlockData;

            return ErrorNumber.NoError;
        }

        byte storageType = fileNode.EffectiveStorageType;

        ushort diskBlock;

        switch(storageType)
        {
            case SEEDLING_FILE_TYPE:
                // Seedling: key block is the single data block
                if(blockIndex != 0) return ErrorNumber.InvalidArgument;

                diskBlock = fileNode.EffectiveKeyBlock;

                break;

            case SAPLING_FILE_TYPE:
                // Sapling: index block contains block pointers
                if(blockIndex >= 256) return ErrorNumber.InvalidArgument;

                diskBlock = fileNode.IndexBlock[blockIndex];

                break;

            case TREE_FILE_TYPE:
            {
                // Tree: master index -> index block -> data block
                int masterIndex = blockIndex / 256;
                int indexOffset = blockIndex % 256;

                if(masterIndex >= 256) return ErrorNumber.InvalidArgument;

                ushort indexBlockPtr = fileNode.MasterIndexBlock[masterIndex];

                // Sparse block (index block pointer is 0)
                if(indexBlockPtr == 0)
                {
                    blockData                 = new byte[512];
                    fileNode.CachedBlockIndex = blockIndex;
                    fileNode.CachedBlockData  = blockData;

                    return ErrorNumber.NoError;
                }

                // Load the index block (cache it for subsequent reads in the same range)
                if(fileNode.IndexBlock == null || fileNode.CachedIndexBlockNumber != masterIndex)
                {
                    ErrorNumber errno = ReadBlock(indexBlockPtr, out byte[] indexBlock);

                    if(errno != ErrorNumber.NoError) return errno;

                    fileNode.IndexBlock = new ushort[256];

                    for(var i = 0; i < 256; i++)
                        fileNode.IndexBlock[i] = (ushort)(indexBlock[i] | indexBlock[256 + i] << 8);

                    fileNode.CachedIndexBlockNumber = masterIndex;
                }

                diskBlock = fileNode.IndexBlock[indexOffset];

                break;
            }

            default:
                return ErrorNumber.InvalidArgument;
        }

        // Sparse block (block pointer is 0 means unallocated = zeros)
        if(diskBlock == 0)
        {
            blockData                 = new byte[512];
            fileNode.CachedBlockIndex = blockIndex;
            fileNode.CachedBlockData  = blockData;

            return ErrorNumber.NoError;
        }

        // Read the actual data block
        ErrorNumber readErrno = ReadBlock(diskBlock, out blockData);

        if(readErrno != ErrorNumber.NoError) return readErrno;

        // Cache the block
        fileNode.CachedBlockIndex = blockIndex;
        fileNode.CachedBlockData  = blockData;

        return ErrorNumber.NoError;
    }

    /// <summary>Gets a cached entry for the given path</summary>
    /// <param name="path">Path to the file or directory</param>
    /// <param name="entry">Output cached entry</param>
    /// <returns>Error number</returns>
    ErrorNumber GetEntryForPath(string path, out CachedEntry entry)
    {
        entry = null;

        if(string.IsNullOrEmpty(path) || path == "/" || path == ".") return ErrorNumber.IsDirectory;

        string[] pathComponents = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if(pathComponents.Length == 0) return ErrorNumber.IsDirectory;

        // Start from root directory cache
        Dictionary<string, CachedEntry> currentDir = _rootDirectoryCache;

        for(var i = 0; i < pathComponents.Length; i++)
        {
            string component = pathComponents[i];

            if(component is "." or "..") continue;

            if(!currentDir.TryGetValue(component, out CachedEntry currentEntry)) return ErrorNumber.NoSuchFile;

            // Last component - return this entry
            if(i == pathComponents.Length - 1)
            {
                entry = currentEntry;

                return ErrorNumber.NoError;
            }

            // Intermediate component must be a directory
            if(!currentEntry.IsDirectory) return ErrorNumber.NotDirectory;

            // Read subdirectory contents
            ErrorNumber errno =
                ReadDirectoryContents(currentEntry.KeyBlock, false, out Dictionary<string, CachedEntry> subDir);

            if(errno != ErrorNumber.NoError) return errno;

            currentDir = subDir;
        }

        return ErrorNumber.NoSuchFile;
    }

    /// <summary>Applies GS/OS case bits to a filename</summary>
    static string ApplyCaseBits(string name, ushort caseBits)
    {
        if((caseBits & 0x8000) == 0) return name;

        char[] chars = name.ToCharArray();
        var    bit   = 0x4000;

        for(var i = 0; i < chars.Length && bit > 0; i++)
        {
            if((caseBits & bit) != 0) chars[i] = char.ToLower(chars[i]);

            bit >>= 1;
        }

        return new string(chars);
    }
}