// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : XFS filesystem plugin.
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
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Logging;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

public sealed partial class XFS
{
    /// <inheritdoc />
    public ErrorNumber OpenDir(string path, out IDirNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize path
        string normalizedPath = path ?? "/";

        if(normalizedPath is "" or ".") normalizedPath = "/";

        // Root directory case
        if(normalizedPath == "/")
        {
            if(_rootDirectoryCache.Count == 0) return ErrorNumber.NoSuchFile;

            node = new XfsDirNode
            {
                Path     = "/",
                Position = 0,
                Entries  = _rootDirectoryCache.Keys.ToArray()
            };

            return ErrorNumber.NoError;
        }

        // Subdirectory traversal
        string pathWithoutLeadingSlash = normalizedPath.StartsWith("/", StringComparison.Ordinal)
                                             ? normalizedPath[1..]
                                             : normalizedPath;

        string[] pathComponents = pathWithoutLeadingSlash.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if(pathComponents.Length == 0) return ErrorNumber.InvalidArgument;

        AaruLogging.Debug(MODULE_NAME, "OpenDir: Traversing path with {0} components", pathComponents.Length);

        // Start from root directory cache
        Dictionary<string, ulong> currentEntries = _rootDirectoryCache;

        // Traverse each path component
        for(var p = 0; p < pathComponents.Length; p++)
        {
            string component = pathComponents[p];

            AaruLogging.Debug(MODULE_NAME, "OpenDir: Navigating to component '{0}'", component);

            if(component is "." or "..") continue;

            if(!currentEntries.TryGetValue(component, out ulong inodeNumber))
            {
                AaruLogging.Debug(MODULE_NAME, "OpenDir: Component '{0}' not found in directory", component);

                return ErrorNumber.NoSuchFile;
            }

            AaruLogging.Debug(MODULE_NAME, "OpenDir: Component '{0}' found with inode {1}", component, inodeNumber);

            // Read the inode
            ErrorNumber errno = ReadInode(inodeNumber, out Dinode inode);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "OpenDir: Error reading inode {0}: {1}", inodeNumber, errno);

                return errno;
            }

            // Check if it's a directory
            if((inode.di_mode & S_IFMT) != S_IFDIR)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "OpenDir: '{0}' is not a directory (mode=0x{1:X4})",
                                  component,
                                  inode.di_mode);

                return ErrorNumber.NotDirectory;
            }

            // Get or read directory contents
            errno = GetDirectoryContents(inodeNumber, inode, out Dictionary<string, ulong> dirEntries);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "OpenDir: Error reading directory contents: {0}", errno);

                return errno;
            }

            // If this is the last component, we're opening this directory
            if(p == pathComponents.Length - 1)
            {
                node = new XfsDirNode
                {
                    Path     = normalizedPath,
                    Position = 0,
                    Entries  = dirEntries.Keys.ToArray()
                };

                AaruLogging.Debug(MODULE_NAME,
                                  "OpenDir: Successfully opened directory '{0}' with {1} entries",
                                  normalizedPath,
                                  dirEntries.Count);

                return ErrorNumber.NoError;
            }

            // Not the last component — move to next level
            currentEntries = dirEntries;
        }

        return ErrorNumber.NoSuchFile;
    }

    /// <inheritdoc />
    public ErrorNumber CloseDir(IDirNode node)
    {
        if(node is not XfsDirNode xfsNode) return ErrorNumber.InvalidArgument;

        xfsNode.Position = -1;
        xfsNode.Entries  = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadDir(IDirNode node, out string filename)
    {
        filename = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not XfsDirNode xfsNode) return ErrorNumber.InvalidArgument;

        if(xfsNode.Position < 0) return ErrorNumber.InvalidArgument;

        // End of directory
        if(xfsNode.Position >= xfsNode.Entries.Length) return ErrorNumber.NoError;

        // Get current filename and advance position
        filename = xfsNode.Entries[xfsNode.Position++];

        return ErrorNumber.NoError;
    }

    /// <summary>Gets or reads directory contents for an inode, using the directory cache</summary>
    /// <param name="inodeNumber">Inode number of the directory</param>
    /// <param name="inode">The directory inode structure</param>
    /// <param name="entries">Output dictionary of entries (filename -> inode number), excluding . and ..</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber GetDirectoryContents(ulong inodeNumber, Dinode inode, out Dictionary<string, ulong> entries)
    {
        if(_directoryCache.TryGetValue(inodeNumber, out entries)) return ErrorNumber.NoError;

        ErrorNumber cacheErrno = ReadDirectoryContents(inodeNumber, inode, out entries);

        if(cacheErrno == ErrorNumber.NoError) _directoryCache[inodeNumber] = entries;

        return cacheErrno;
    }

    /// <summary>Reads directory contents for an inode from disk, parsing the on-disk directory structure</summary>
    /// <param name="inodeNumber">Inode number of the directory</param>
    /// <param name="inode">The directory inode structure</param>
    /// <param name="entries">Output dictionary of entries (filename -> inode number), excluding . and ..</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ReadDirectoryContents(ulong inodeNumber, Dinode inode, out Dictionary<string, ulong> entries)
    {
        entries = new Dictionary<string, ulong>();

        ErrorNumber errno;

        if(_isDirV1)
        {
            errno = inode.di_format switch
                    {
                        XFS_DINODE_FMT_LOCAL   => ReadV1ShortformDirectory(inodeNumber, inode, entries),
                        XFS_DINODE_FMT_EXTENTS => ReadV1ExtentDirectory(inodeNumber, inode, entries),
                        XFS_DINODE_FMT_BTREE   => ReadV1BtreeDirectory(inodeNumber, inode, entries),
                        _                      => ErrorNumber.NotSupported
                    };
        }
        else
        {
            errno = inode.di_format switch
                    {
                        XFS_DINODE_FMT_LOCAL   => ReadShortformDirectory(inodeNumber, inode, entries),
                        XFS_DINODE_FMT_EXTENTS => ReadExtentDirectory(inodeNumber, inode, entries),
                        XFS_DINODE_FMT_BTREE   => ReadBtreeDirectory(inodeNumber, inode, entries),
                        _                      => ErrorNumber.NotSupported
                    };
        }

        return errno;
    }

    /// <summary>Reads a shortform (inline) directory from the inode data fork</summary>
    /// <param name="inodeNumber">Inode number to read raw data from</param>
    /// <param name="inode">The directory inode</param>
    /// <param name="entries">Dictionary to populate with entries</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ReadShortformDirectory(ulong inodeNumber, Dinode inode, Dictionary<string, ulong> entries)
    {
        AaruLogging.Debug(MODULE_NAME, "Reading shortform directory for inode {0}", inodeNumber);

        int coreSize = _v3Inodes ? Marshal.SizeOf<Dinode>() : 100;

        ErrorNumber errno = ReadInodeRaw(inodeNumber, out byte[] rawInode);

        if(errno != ErrorNumber.NoError) return errno;

        if(rawInode.Length < coreSize + 6)
        {
            AaruLogging.Debug(MODULE_NAME, "Raw inode too small for shortform directory");

            return ErrorNumber.InvalidArgument;
        }

        int pos = coreSize;

        byte count   = rawInode[pos];
        byte i8count = rawInode[pos + 1];

        bool use64BitInodes = i8count > 0;

        int parentInoSize = use64BitInodes ? 8 : 4;
        int headerSize    = 2 + parentInoSize;

        AaruLogging.Debug(MODULE_NAME, "SF dir: count={0}, i8count={1}, use64={2}", count, i8count, use64BitInodes);

        if(pos + headerSize > rawInode.Length)
        {
            AaruLogging.Debug(MODULE_NAME, "Raw inode too small for shortform header");

            return ErrorNumber.InvalidArgument;
        }

        pos += headerSize;

        int totalEntries = count;

        for(var i = 0; i < totalEntries; i++)
        {
            if(pos + 3 > rawInode.Length)
            {
                AaruLogging.Debug(MODULE_NAME, "Truncated shortform entry at index {0}", i);

                break;
            }

            byte nameLen = rawInode[pos];
            pos += 1 + 2; // skip namelen + offset

            if(pos + nameLen > rawInode.Length)
            {
                AaruLogging.Debug(MODULE_NAME, "Truncated name in shortform entry at index {0}", i);

                break;
            }

            string name = _encoding.GetString(rawInode, pos, nameLen);
            pos += nameLen;

            if(_hasFtype)
            {
                if(pos + 1 > rawInode.Length) break;

                pos += 1;
            }

            ulong entryIno;

            if(use64BitInodes)
            {
                if(pos + 8 > rawInode.Length) break;

                entryIno =  BigEndianBitConverter.ToUInt64(rawInode, pos) & XFS_MAXINUMBER;
                pos      += 8;
            }
            else
            {
                if(pos + 4 > rawInode.Length) break;

                entryIno =  BigEndianBitConverter.ToUInt32(rawInode, pos);
                pos      += 4;
            }

            if(name is "." or "..") continue;

            entries[name] = entryIno;

            AaruLogging.Debug(MODULE_NAME, "SF entry: \"{0}\" -> inode {1}", name, entryIno);
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Reads a directory stored in extent format</summary>
    /// <param name="inodeNumber">Inode number to read raw data from</param>
    /// <param name="inode">The directory inode</param>
    /// <param name="entries">Dictionary to populate with entries</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ReadExtentDirectory(ulong inodeNumber, Dinode inode, Dictionary<string, ulong> entries)
    {
        int coreSize = _v3Inodes ? Marshal.SizeOf<Dinode>() : 100;

        ErrorNumber errno = ReadInodeRaw(inodeNumber, out byte[] rawInode);

        if(errno != ErrorNumber.NoError) return errno;

        // Determine extent count based on NREXT64 feature (per-inode flag in di_flags2)
        ulong extentCount;

        if(_v3Inodes && (inode.di_flags2 & XFS_DIFLAG2_NREXT64) != 0)
        {
            // di_big_nextents is at offset 24 (di_v2_pad/di_flushiter position) as 8 bytes for V3 NREXT64
            if(rawInode.Length < 32)
            {
                AaruLogging.Debug(MODULE_NAME, "Truncated inode data for NREXT64");

                return ErrorNumber.InvalidArgument;
            }

            extentCount = BigEndianBitConverter.ToUInt64(rawInode, 24);

            AaruLogging.Debug(MODULE_NAME,
                              "Reading extent directory for inode {0} ({1} extents, NREXT64)",
                              inodeNumber,
                              extentCount);
        }
        else
        {
            extentCount = inode.di_nextents;

            AaruLogging.Debug(MODULE_NAME,
                              "Reading extent directory for inode {0} ({1} extents)",
                              inodeNumber,
                              extentCount);
        }

        int pos = coreSize;

        // Directory blocks may span multiple filesystem blocks (1 << dirblklog)
        uint dirBlockFsBlocks = _dirBlockFsBlocks;

        for(ulong i = 0; i < extentCount; i++)
        {
            if(pos + 16 > rawInode.Length)
            {
                AaruLogging.Debug(MODULE_NAME, "Truncated extent record at index {0}", i);

                break;
            }

            var l0 = BigEndianBitConverter.ToUInt64(rawInode, pos);
            var l1 = BigEndianBitConverter.ToUInt64(rawInode, pos + 8);
            pos += 16;

            DecodeBmbtRec(l0, l1, out _, out ulong startBlock, out uint blockCount, out _);

            AaruLogging.Debug(MODULE_NAME, "Extent {0}: startblock={1}, count={2}", i, startBlock, blockCount);

            for(uint b = 0; b < blockCount; b += dirBlockFsBlocks)
            {
                uint fsBlocksToRead = Math.Min(dirBlockFsBlocks, blockCount - b);

                // Read and assemble the full directory block
                var dirBlockData = new byte[fsBlocksToRead * _superblock.blocksize];
                var validRead    = true;

                for(uint fb = 0; fb < fsBlocksToRead; fb++)
                {
                    errno = ReadBlock(startBlock + b + fb, out byte[] blockData);

                    if(errno != ErrorNumber.NoError)
                    {
                        AaruLogging.Debug(MODULE_NAME,
                                          "Error reading directory block {0}: {1}",
                                          startBlock + b + fb,
                                          errno);

                        validRead = false;

                        break;
                    }

                    Array.Copy(blockData, 0, dirBlockData, fb * _superblock.blocksize, blockData.Length);
                }

                if(validRead) ParseDirectoryDataBlock(dirBlockData, entries);
            }
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Reads a directory stored in btree format</summary>
    /// <param name="inodeNumber">Inode number to read raw data from</param>
    /// <param name="inode">The directory inode</param>
    /// <param name="entries">Dictionary to populate with entries</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ReadBtreeDirectory(ulong inodeNumber, Dinode inode, Dictionary<string, ulong> entries)
    {
        AaruLogging.Debug(MODULE_NAME, "Reading btree directory for inode {0}", inodeNumber);

        int coreSize = _v3Inodes ? Marshal.SizeOf<Dinode>() : 100;

        ErrorNumber errno = ReadInodeRaw(inodeNumber, out byte[] rawInode);

        if(errno != ErrorNumber.NoError) return errno;

        int pos = coreSize;

        if(pos + 4 > rawInode.Length)
        {
            AaruLogging.Debug(MODULE_NAME, "Raw inode too small for bmdr_block header");

            return ErrorNumber.InvalidArgument;
        }

        var level   = BigEndianBitConverter.ToUInt16(rawInode, pos);
        var numrecs = BigEndianBitConverter.ToUInt16(rawInode, pos + 2);

        AaruLogging.Debug(MODULE_NAME, "BMDR: level={0}, numrecs={1}", level, numrecs);

        if(level == 0)
        {
            int recPos = pos + 4;

            for(var i = 0; i < numrecs; i++)
            {
                if(recPos + 16 > rawInode.Length) break;

                var l0 = BigEndianBitConverter.ToUInt64(rawInode, recPos);
                var l1 = BigEndianBitConverter.ToUInt64(rawInode, recPos + 8);
                recPos += 16;

                DecodeBmbtRec(l0, l1, out _, out ulong startBlock, out uint blockCount, out _);

                uint dirBlockFsBlocks = _dirBlockFsBlocks;

                for(uint b = 0; b < blockCount; b += dirBlockFsBlocks)
                {
                    uint fsBlocksToRead = Math.Min(dirBlockFsBlocks, blockCount - b);

                    var dirBlockData = new byte[fsBlocksToRead * _superblock.blocksize];
                    var validRead    = true;

                    for(uint fb = 0; fb < fsBlocksToRead; fb++)
                    {
                        errno = ReadBlock(startBlock + b + fb, out byte[] blockData);

                        if(errno != ErrorNumber.NoError)
                        {
                            validRead = false;

                            break;
                        }

                        Array.Copy(blockData, 0, dirBlockData, fb * _superblock.blocksize, blockData.Length);
                    }

                    if(validRead) ParseDirectoryDataBlock(dirBlockData, entries);
                }
            }
        }
        else
        {
            int forkSize = inode.di_forkoff > 0 ? inode.di_forkoff * 8 : rawInode.Length - coreSize;

            // maxrecs = max records that fit in the BMDR block
            // Layout: header(4) + keys(maxrecs*8) + ptrs(maxrecs*8)
            int maxrecs   = (forkSize - 4) / 16;
            int ptrsStart = pos + 4 + maxrecs * 8;

            for(var i = 0; i < numrecs; i++)
            {
                int ptrPos = ptrsStart + i * 8;

                if(ptrPos + 8 > rawInode.Length) break;

                var childBlock = BigEndianBitConverter.ToUInt64(rawInode, ptrPos);

                errno = ReadBmapBtreeBlock(childBlock, level - 1, entries);

                if(errno != ErrorNumber.NoError)
                    AaruLogging.Debug(MODULE_NAME, "Error reading bmap btree child block {0}: {1}", childBlock, errno);
            }
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Reads a V1 shortform (inline) directory from the inode data fork</summary>
    /// <param name="inodeNumber">Inode number to read raw data from</param>
    /// <param name="inode">The directory inode</param>
    /// <param name="entries">Dictionary to populate with entries</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ReadV1ShortformDirectory(ulong inodeNumber, Dinode inode, Dictionary<string, ulong> entries)
    {
        AaruLogging.Debug(MODULE_NAME, "Reading V1 shortform directory for inode {0}", inodeNumber);

        int coreSize = _v3Inodes ? Marshal.SizeOf<Dinode>() : 100;

        ErrorNumber errno = ReadInodeRaw(inodeNumber, out byte[] rawInode);

        if(errno != ErrorNumber.NoError) return errno;

        // V1 shortform header: xfs_dir_ino_t parent (8 bytes) + uint8 count (1 byte) = 9 bytes
        if(rawInode.Length < coreSize + 9)
        {
            AaruLogging.Debug(MODULE_NAME, "Raw inode too small for V1 shortform directory");

            return ErrorNumber.InvalidArgument;
        }

        int pos = coreSize;

        // parent inode is 8 bytes big-endian (xfs_dir_ino_t)
        pos += 8;

        byte count = rawInode[pos];
        pos++;

        AaruLogging.Debug(MODULE_NAME, "V1 SF dir: count={0}", count);

        // Each V1 shortform entry: xfs_dir_ino_t inumber (8 bytes) + uint8 namelen (1 byte) + name
        for(var i = 0; i < count; i++)
        {
            if(pos + 9 > rawInode.Length)
            {
                AaruLogging.Debug(MODULE_NAME, "Truncated V1 shortform entry at index {0}", i);

                break;
            }

            ulong entryIno = BigEndianBitConverter.ToUInt64(rawInode, pos) & XFS_MAXINUMBER;
            pos += 8;

            byte nameLen = rawInode[pos];
            pos++;

            if(nameLen == 0 || pos + nameLen > rawInode.Length)
            {
                AaruLogging.Debug(MODULE_NAME, "Truncated V1 shortform name at index {0}", i);

                break;
            }

            string name = _encoding.GetString(rawInode, pos, nameLen);
            pos += nameLen;

            if(name is "." or "..") continue;

            entries[name] = entryIno;

            AaruLogging.Debug(MODULE_NAME, "V1 SF entry: \"{0}\" -> inode {1}", name, entryIno);
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Reads a V1 directory stored in extent format</summary>
    /// <param name="inodeNumber">Inode number to read raw data from</param>
    /// <param name="inode">The directory inode</param>
    /// <param name="entries">Dictionary to populate with entries</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ReadV1ExtentDirectory(ulong inodeNumber, Dinode inode, Dictionary<string, ulong> entries)
    {
        AaruLogging.Debug(MODULE_NAME,
                          "Reading V1 extent directory for inode {0} ({1} extents)",
                          inodeNumber,
                          inode.di_nextents);

        int coreSize = _v3Inodes ? Marshal.SizeOf<Dinode>() : 100;

        ErrorNumber errno = ReadInodeRaw(inodeNumber, out byte[] rawInode);

        if(errno != ErrorNumber.NoError) return errno;

        ulong extentCount = inode.di_nextents;
        int   pos         = coreSize;

        for(ulong i = 0; i < extentCount; i++)
        {
            if(pos + 16 > rawInode.Length)
            {
                AaruLogging.Debug(MODULE_NAME, "Truncated extent record at index {0}", i);

                break;
            }

            var l0 = BigEndianBitConverter.ToUInt64(rawInode, pos);
            var l1 = BigEndianBitConverter.ToUInt64(rawInode, pos + 8);
            pos += 16;

            DecodeBmbtRec(l0, l1, out _, out ulong startBlock, out uint blockCount, out _);

            AaruLogging.Debug(MODULE_NAME, "V1 Extent {0}: startblock={1}, count={2}", i, startBlock, blockCount);

            // V1 directory blocks are always 1 filesystem block
            for(uint b = 0; b < blockCount; b++)
            {
                errno = ReadBlock(startBlock + b, out byte[] blockData);

                if(errno != ErrorNumber.NoError)
                {
                    AaruLogging.Debug(MODULE_NAME, "Error reading V1 directory block {0}: {1}", startBlock + b, errno);

                    continue;
                }

                ParseV1DirectoryBlock(blockData, entries);
            }
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Reads a V1 directory stored in btree format</summary>
    /// <param name="inodeNumber">Inode number to read raw data from</param>
    /// <param name="inode">The directory inode</param>
    /// <param name="entries">Dictionary to populate with entries</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ReadV1BtreeDirectory(ulong inodeNumber, Dinode inode, Dictionary<string, ulong> entries)
    {
        AaruLogging.Debug(MODULE_NAME, "Reading V1 btree directory for inode {0}", inodeNumber);

        int coreSize = _v3Inodes ? Marshal.SizeOf<Dinode>() : 100;

        ErrorNumber errno = ReadInodeRaw(inodeNumber, out byte[] rawInode);

        if(errno != ErrorNumber.NoError) return errno;

        int pos = coreSize;

        if(pos + 4 > rawInode.Length)
        {
            AaruLogging.Debug(MODULE_NAME, "Raw inode too small for bmdr_block header");

            return ErrorNumber.InvalidArgument;
        }

        var level   = BigEndianBitConverter.ToUInt16(rawInode, pos);
        var numrecs = BigEndianBitConverter.ToUInt16(rawInode, pos + 2);

        AaruLogging.Debug(MODULE_NAME, "V1 BMDR: level={0}, numrecs={1}", level, numrecs);

        if(level == 0)
        {
            int recPos = pos + 4;

            for(var i = 0; i < numrecs; i++)
            {
                if(recPos + 16 > rawInode.Length) break;

                var l0 = BigEndianBitConverter.ToUInt64(rawInode, recPos);
                var l1 = BigEndianBitConverter.ToUInt64(rawInode, recPos + 8);
                recPos += 16;

                DecodeBmbtRec(l0, l1, out _, out ulong startBlock, out uint blockCount, out _);

                for(uint b = 0; b < blockCount; b++)
                {
                    errno = ReadBlock(startBlock + b, out byte[] blockData);

                    if(errno != ErrorNumber.NoError) continue;

                    ParseV1DirectoryBlock(blockData, entries);
                }
            }
        }
        else
        {
            int forkSize = inode.di_forkoff > 0 ? inode.di_forkoff * 8 : rawInode.Length - coreSize;

            int maxrecs   = (forkSize - 4) / 16;
            int ptrsStart = pos + 4 + maxrecs * 8;

            for(var i = 0; i < numrecs; i++)
            {
                int ptrPos = ptrsStart + i * 8;

                if(ptrPos + 8 > rawInode.Length) break;

                var childBlock = BigEndianBitConverter.ToUInt64(rawInode, ptrPos);

                errno = ReadV1BmapBtreeBlock(childBlock, level - 1, entries);

                if(errno != ErrorNumber.NoError)
                {
                    AaruLogging.Debug(MODULE_NAME,
                                      "Error reading V1 bmap btree child block {0}: {1}",
                                      childBlock,
                                      errno);
                }
            }
        }

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Parses a V1 directory block. Checks the magic number in the da_blkinfo header
    ///     to distinguish leaf blocks (0xFEEB) from DA btree nodes (0xFEBE), and only
    ///     extracts entries from leaf blocks.
    /// </summary>
    /// <param name="blockData">The raw block data</param>
    /// <param name="entries">Dictionary to populate with entries</param>
    void ParseV1DirectoryBlock(byte[] blockData, Dictionary<string, ulong> entries)
    {
        // da_blkinfo_t: forw(4) + back(4) + magic(2) + pad(2) = 12 bytes
        if(blockData.Length < 32) return;

        var magic = BigEndianBitConverter.ToUInt16(blockData, 8);

        if(magic == XFS_DA_NODE_MAGIC)
        {
            // DA btree internal node — skip, contains only index data
            return;
        }

        if(magic != XFS_DIR_LEAF_MAGIC)
        {
            AaruLogging.Debug(MODULE_NAME, "Unknown V1 directory block magic: 0x{0:X4}", magic);

            return;
        }

        ParseV1DirectoryLeafBlock(blockData, entries);
    }

    /// <summary>
    ///     Parses a V1 directory leaf block (magic 0xFEEB).
    ///     Layout: xfs_dir_leaf_hdr (32 bytes), then leaf entries (8 bytes each) packed from top,
    ///     and name data grows from the bottom. Each leaf entry has a nameidx pointing into
    ///     the block where the xfs_dir_leaf_name_t (8-byte inode + name) resides.
    /// </summary>
    /// <param name="blockData">The raw leaf block data</param>
    /// <param name="entries">Dictionary to populate with entries</param>
    void ParseV1DirectoryLeafBlock(byte[] blockData, Dictionary<string, ulong> entries)
    {
        // xfs_dir_leaf_hdr: da_blkinfo(12) + count(2) + namebytes(2) + firstused(2) + holes(1) + pad1(1) + freemap(12) = 32
        const int V1_LEAF_HDR_SIZE   = 32;
        const int V1_LEAF_ENTRY_SIZE = 8; // hashval(4) + nameidx(2) + namelen(1) + pad(1)

        if(blockData.Length < V1_LEAF_HDR_SIZE) return;

        var count = BigEndianBitConverter.ToUInt16(blockData, 12);

        AaruLogging.Debug(MODULE_NAME, "V1 leaf block: count={0}", count);

        int pos = V1_LEAF_HDR_SIZE;

        for(var i = 0; i < count; i++)
        {
            if(pos + V1_LEAF_ENTRY_SIZE > blockData.Length)
            {
                AaruLogging.Debug(MODULE_NAME, "Truncated V1 leaf entry at index {0}", i);

                break;
            }

            // xfs_dir_leaf_entry: hashval(4) + nameidx(2) + namelen(1) + pad(1)
            var  nameIdx = BigEndianBitConverter.ToUInt16(blockData, pos + 4);
            byte nameLen = blockData[pos                                 + 6];
            pos += V1_LEAF_ENTRY_SIZE;

            if(nameLen == 0 || nameIdx + 8 + nameLen > blockData.Length)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "Invalid V1 leaf name at index {0}: idx={1}, len={2}",
                                  i,
                                  nameIdx,
                                  nameLen);

                continue;
            }

            // xfs_dir_leaf_name_t at nameIdx: xfs_dir_ino_t inumber (8 bytes) + name
            ulong  entryIno = BigEndianBitConverter.ToUInt64(blockData, nameIdx) & XFS_MAXINUMBER;
            string name     = _encoding.GetString(blockData, nameIdx + 8, nameLen);

            if(name is "." or "..") continue;

            entries[name] = entryIno;

            AaruLogging.Debug(MODULE_NAME, "V1 leaf entry: \"{0}\" -> inode {1}", name, entryIno);
        }
    }

    /// <summary>Parses a directory data block (v2 or v3 format) and adds entries to the given dictionary</summary>
    /// <param name="blockData">The raw block data</param>
    /// <param name="entries">Dictionary to populate with entries</param>
    void ParseDirectoryDataBlock(byte[] blockData, Dictionary<string, ulong> entries)
    {
        if(blockData.Length < 16) return;

        var magic = BigEndianBitConverter.ToUInt32(blockData, 0);

        int dataStart;
        int dataEnd = blockData.Length;

        switch(magic)
        {
            case XFS_DIR2_BLOCK_MAGIC:
                dataStart = 16;

                // Block-format: the tail is at the end, containing leaf count + stale count
                // xfs_dir2_block_tail is at blockData[length - 4 (stale) - 4 (count)] = last 8 bytes
                // The leaf entries sit before the tail: count * 8 bytes
                if(blockData.Length >= 8)
                {
                    int tailOffset = blockData.Length - 8;
                    var leafCount  = BigEndianBitConverter.ToUInt32(blockData, tailOffset);
                    dataEnd = tailOffset - (int)leafCount * 8;
                }

                break;
            case XFS_DIR2_DATA_MAGIC:
                dataStart = 16;

                break;
            case XFS_DIR3_BLOCK_MAGIC:
                dataStart = 64;

                if(blockData.Length >= 8)
                {
                    int tailOffset = blockData.Length - 8;
                    var leafCount  = BigEndianBitConverter.ToUInt32(blockData, tailOffset);
                    dataEnd = tailOffset - (int)leafCount * 8;
                }

                break;
            case XFS_DIR3_DATA_MAGIC:
                dataStart = 64;

                break;
            default:
                AaruLogging.Debug(MODULE_NAME, "Unknown directory block magic: 0x{0:X8}", magic);

                return;
        }

        int pos = dataStart;

        while(pos + 10 < dataEnd)
        {
            var freetag = BigEndianBitConverter.ToUInt16(blockData, pos);

            if(freetag == 0xFFFF)
            {
                if(pos + 4 > dataEnd) break;

                var freeLen = BigEndianBitConverter.ToUInt16(blockData, pos + 2);

                if(freeLen == 0 || pos + freeLen > dataEnd) break;

                pos += freeLen;

                continue;
            }

            if(pos + 9 > dataEnd) break;

            var  entryIno = BigEndianBitConverter.ToUInt64(blockData, pos);
            byte nameLen  = blockData[pos + 8];

            if(nameLen == 0 || pos + 9 + nameLen > dataEnd) break;

            string name = _encoding.GetString(blockData, pos + 9, nameLen);

            int afterName = pos + 9 + nameLen;

            if(_hasFtype) afterName++;

            int entryEnd = (afterName + 2 + 7) / 8 * 8;

            if(name is not "." and not "..")
            {
                entries[name] = entryIno;

                AaruLogging.Debug(MODULE_NAME, "Block entry: \"{0}\" -> inode {1}", name, entryIno);
            }

            pos = entryEnd;
        }
    }
}