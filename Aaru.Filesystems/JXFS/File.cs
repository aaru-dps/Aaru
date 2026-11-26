// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
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

using System;
using System.Collections.Generic;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Aaru.Logging;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class JXFS
{
    /// <summary>Start of the AmigaDOS dates</summary>
    static readonly DateTime _amigaEpoch = new(1978, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        ErrorNumber errno = GetEntry(path, out DirectoryEntry entry);

        if(errno != ErrorNumber.NoError) return errno;

        Node node = entry.Node;
        uint fib  = node.Protection ^ PROTECTION_INVERTED_BITS;

        stat = new FileEntryInfo
        {
            Inode            = node.Number,
            Links            = 1,
            BlockSize        = _blockSize,
            LastWriteTimeUtc = TicksToDateTime(node.Date),
            Mode             = ProtectionToUnixMode(fib)
        };

        if((node.Flags & NodeFlags.Directory) != 0)
            stat.Attributes = FileAttributes.Directory;
        else if((node.Flags & NodeFlags.SoftLink) != 0)
            stat.Attributes = FileAttributes.Symlink;
        else
        {
            stat.Attributes = FileAttributes.File;
            stat.Length     = (long)node.Size;

            // Small files use the payload of their data blocks, other files whole extent units
            if(IsSmallFile(node))
            {
                long payload = _blockSize - DATA_HEADER_SIZE;
                stat.Blocks = (stat.Length + payload - 1) / payload;
            }
            else if(GetExtents(node, out List<Extent> extents) == ErrorNumber.NoError)
            {
                foreach(Extent extent in extents) stat.Blocks += (long)extent.Blocks;
            }
        }

        // FIBB_ARCHIVE
        if((fib & 1 << 4) != 0) stat.Attributes |= FileAttributes.Archive;

        // FIBB_PURE, resident
        if((fib & 1 << 5) != 0) stat.Attributes |= FileAttributes.System;

        // FIBB_WRITE, active low
        if((fib & 1 << 2) != 0) stat.Attributes |= FileAttributes.ReadOnly;

        if(ReadAttributes(node, out uint? uid, out uint? gid, out _) == ErrorNumber.NoError)
        {
            stat.UID = uid;
            stat.GID = gid;
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadLink(string path, out string dest)
    {
        dest = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        ErrorNumber errno = GetEntry(path, out DirectoryEntry entry);

        if(errno != ErrorNumber.NoError) return errno;

        if((entry.Node.Flags & NodeFlags.SoftLink) == 0) return ErrorNumber.InvalidArgument;

        errno = ReadSystemFileBlock(_miscFile, entry.Node.Pointer, SOFTLINK_ID, out byte[] data);

        if(errno != ErrorNumber.NoError) return errno;

        dest = ReadName(data, SOFTLINK_TARGET_OFFSET, out _);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber OpenFile(string path, out IFileNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        ErrorNumber errno = GetEntry(path, out DirectoryEntry entry);

        if(errno != ErrorNumber.NoError) return errno;

        if((entry.Node.Flags & NodeFlags.Directory) != 0) return ErrorNumber.IsDirectory;

        if((entry.Node.Flags & NodeFlags.SoftLink) != 0) return ErrorNumber.InvalidArgument;

        JxfsFileNode fileNode = new JxfsFileNode
        {
            Path      = path,
            Length    = (long)entry.Node.Size,
            Offset    = 0,
            SmallFile = IsSmallFile(entry.Node),
            FirstData = FirstDataBlock(entry.Node)
        };

        if(!fileNode.SmallFile)
        {
            errno = GetExtents(entry.Node, out List<Extent> extents);

            if(errno != ErrorNumber.NoError) return errno;

            fileNode.Extents = extents;
        }

        node = fileNode;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(node is not JxfsFileNode fileNode) return ErrorNumber.InvalidArgument;

        fileNode.Extents    = null;
        fileNode.ChainData  = null;
        fileNode.ChainBlock = 0;
        fileNode.ChainIndex = 0;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not JxfsFileNode fileNode) return ErrorNumber.InvalidArgument;

        if(buffer is null || length < 0 || buffer.Length < length) return ErrorNumber.InvalidArgument;

        if(fileNode.Offset >= fileNode.Length) return ErrorNumber.NoError;

        if(length > fileNode.Length - fileNode.Offset) length = fileNode.Length - fileNode.Offset;

        int payload = fileNode.SmallFile ? (int)_blockSize - DATA_HEADER_SIZE : (int)_blockSize;
        int header  = fileNode.SmallFile ? DATA_HEADER_SIZE : 0;

        while(read < length)
        {
            long   index   = fileNode.Offset / payload;
            int    inBlock = (int)(fileNode.Offset % payload);
            int    toCopy  = (int)Math.Min(payload - inBlock, length - read);
            byte[] data;

            ErrorNumber errno = fileNode.SmallFile
                                    ? ReadSmallFileBlock(fileNode, index, out data)
                                    : ReadExtentBlock(fileNode, index, out data);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Error reading block {0} of \"{1}\": {2}", index, fileNode.Path, errno);

                return errno;
            }

            Array.Copy(data, header + inBlock, buffer, read, toCopy);

            read            += toCopy;
            fileNode.Offset += toCopy;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Reads a data block of a small file, following the chain from the last block read</summary>
    /// <param name="fileNode">Open file</param>
    /// <param name="index">Data block index in the file</param>
    /// <param name="data">The data block contents</param>
    /// <returns>Error number</returns>
    ErrorNumber ReadSmallFileBlock(JxfsFileNode fileNode, long index, out byte[] data)
    {
        data = null;

        // Restart from the first block when seeking backwards
        if(fileNode.ChainData is null || index < fileNode.ChainIndex)
        {
            if(fileNode.FirstData == 0) return ErrorNumber.InvalidArgument;

            ErrorNumber errno = ReadSystemFileBlock(_miscFile, fileNode.FirstData, DATA_ID, out byte[] first);

            if(errno != ErrorNumber.NoError) return errno;

            fileNode.ChainBlock = fileNode.FirstData;
            fileNode.ChainIndex = 0;
            fileNode.ChainData  = first;
        }

        while(fileNode.ChainIndex < index)
        {
            uint next = BigEndianBitConverter.ToUInt32(fileNode.ChainData, DATA_NEXT_OFFSET);

            if(next == 0) return ErrorNumber.InvalidArgument;

            ErrorNumber errno = ReadSystemFileBlock(_miscFile, next, DATA_ID, out byte[] nextData);

            if(errno != ErrorNumber.NoError) return errno;

            fileNode.ChainBlock = next;
            fileNode.ChainIndex++;
            fileNode.ChainData = nextData;
        }

        data = fileNode.ChainData;

        return ErrorNumber.NoError;
    }

    /// <summary>Reads a block of a file stored in extents</summary>
    /// <param name="fileNode">Open file</param>
    /// <param name="index">Block index in the file</param>
    /// <param name="data">The block contents</param>
    /// <returns>Error number</returns>
    ErrorNumber ReadExtentBlock(JxfsFileNode fileNode, long index, out byte[] data)
    {
        data = null;

        ErrorNumber errno = MapBlock(fileNode.Extents, (ulong)index, out ulong block);

        if(errno != ErrorNumber.NoError) return errno;

        return ReadBlock(_imagePlugin, _partition, _blockSize, block, out data);
    }

    /// <summary>Reads the owner and comment attributes of a node</summary>
    /// <param name="node">Node</param>
    /// <param name="uid">Owner user, if stored</param>
    /// <param name="gid">Owner group, if stored</param>
    /// <param name="comment">Comment, if stored</param>
    /// <returns>Error number</returns>
    ErrorNumber ReadAttributes(Node node, out uint? uid, out uint? gid, out string comment)
    {
        uid     = null;
        gid     = null;
        comment = null;

        if(node.AttributeBlock == 0) return ErrorNumber.NoSuchExtendedAttribute;

        ErrorNumber errno = ReadSystemFileBlock(_miscFile, node.AttributeBlock, ATTRIBUTE_ID, out byte[] data);

        if(errno != ErrorNumber.NoError) return errno;

        // Each entry is its length, the attribute name and the value
        int offset = ATTRIBUTE_HEADER_SIZE;

        while(offset + 2 <= data.Length)
        {
            ushort entryLength = BigEndianBitConverter.ToUInt16(data, offset);

            if(entryLength < 2 || offset + entryLength > data.Length) break;

            int    nameEnd = Array.IndexOf(data, (byte)0, offset + 2, entryLength - 2);
            string name    = nameEnd < 0 ? "" : _encoding.GetString(data, offset + 2, nameEnd - offset - 2);

            switch(name)
            {
                case ATTRIBUTE_UID when entryLength >= ATTRIBUTE_NUMBER_OFFSET + 4:
                    uid = BigEndianBitConverter.ToUInt32(data, offset + ATTRIBUTE_NUMBER_OFFSET);

                    break;
                case ATTRIBUTE_GID when entryLength >= ATTRIBUTE_NUMBER_OFFSET + 4:
                    gid = BigEndianBitConverter.ToUInt32(data, offset + ATTRIBUTE_NUMBER_OFFSET);

                    break;
                case ATTRIBUTE_COMMENT when entryLength > ATTRIBUTE_COMMENT_OFFSET:
                    int commentEnd = Array.IndexOf(data,
                                                   (byte)0,
                                                   offset + ATTRIBUTE_COMMENT_OFFSET,
                                                   entryLength - ATTRIBUTE_COMMENT_OFFSET);

                    if(commentEnd < 0) commentEnd = offset + entryLength;

                    comment = _encoding.GetString(data,
                                                  offset + ATTRIBUTE_COMMENT_OFFSET,
                                                  commentEnd - offset - ATTRIBUTE_COMMENT_OFFSET);

                    break;
            }

            offset += entryLength;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Converts AmigaDOS protection bits to a Unix mode</summary>
    /// <param name="protection">AmigaDOS protection, with the RWED bits active low</param>
    /// <returns>Unix mode</returns>
    static uint ProtectionToUnixMode(uint protection)
    {
        uint mode = 0;

        // Owner bits are active low
        if((protection & 1 << 3) == 0) mode |= 0x100;

        if((protection & 1 << 2) == 0) mode |= 0x080;

        if((protection & 1 << 1) == 0) mode |= 0x040;

        // Group and other bits are active high
        if((protection & 1 << 11) != 0) mode |= 0x020;

        if((protection & 1 << 10) != 0) mode |= 0x010;

        if((protection & 1 << 9) != 0) mode |= 0x008;

        if((protection & 1 << 15) != 0) mode |= 0x004;

        if((protection & 1 << 14) != 0) mode |= 0x002;

        if((protection & 1 << 13) != 0) mode |= 0x001;

        return mode;
    }
}