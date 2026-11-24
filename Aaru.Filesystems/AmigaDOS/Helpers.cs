// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Helpers.cs
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

using System;
using Aaru.CommonTypes.Enums;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class AmigaDOSPlugin
{
    static RootBlock MarshalRootBlock(byte[] block)
    {
        var tmp = new byte[228];
        Array.Copy(block, 0,                  tmp, 0,  24);
        Array.Copy(block, block.Length - 200, tmp, 28, 200);
        RootBlock root = Marshal.ByteArrayToStructureBigEndian<RootBlock>(tmp);
        root.hashTable = new uint[(block.Length - 224) / 4];

        for(var i = 0; i < root.hashTable.Length; i++)
            root.hashTable[i] = BigEndianBitConverter.ToUInt32(block, 24 + i * 4);

        return root;
    }

    /// <summary>Gets the raw name of a directory, file or link header block</summary>
    /// <param name="block">Header block</param>
    /// <param name="longNames">Whether the volume uses long names (DOS\6 and DOS\7)</param>
    /// <returns>Name bytes, without the length byte</returns>
    internal static byte[] GetEntryNameBytes(byte[] block, bool longNames)
    {
        // Long name blocks start the merged name and comment field with the name
        int nameOffset = block.Length - (longNames ? ENTRY_COMMENT_FROM_END : ENTRY_NAME_FROM_END);
        int maxLength  = longNames ? MAX_LONG_NAME_LENGTH : MAX_NAME_LENGTH;

        if(nameOffset < 0) return [];

        int nameLength = Math.Min(Math.Min(block[nameOffset], maxLength), block.Length - nameOffset - 1);

        var name = new byte[nameLength];
        Array.Copy(block, nameOffset + 1, name, 0, nameLength);

        return name;
    }

    /// <summary>Gets the comment stored inside a directory, file or link header block</summary>
    /// <param name="block">Header block</param>
    /// <param name="longNames">Whether the volume uses long names (DOS\6 and DOS\7)</param>
    /// <returns>Comment bytes, without the length byte, empty if there is none stored in the block</returns>
    internal static byte[] GetInlineCommentBytes(byte[] block, bool longNames)
    {
        int commentOffset = block.Length - ENTRY_COMMENT_FROM_END;

        if(commentOffset < 0) return [];

        int fieldEnd = longNames ? commentOffset + LONG_NAME_AND_COMMENT_SIZE : commentOffset + 1 + MAX_COMMENT_LENGTH;

        // Long name blocks store the comment right after the name, if both fit
        if(longNames) commentOffset += 1 + block[commentOffset];

        if(commentOffset >= fieldEnd) return [];

        int commentLength = block[commentOffset];

        if(commentLength > MAX_COMMENT_LENGTH || commentOffset + 1 + commentLength > fieldEnd) return [];

        var comment = new byte[commentLength];
        Array.Copy(block, commentOffset + 1, comment, 0, commentLength);

        return comment;
    }

    /// <summary>Gets the comment block pointer of a long name header block</summary>
    /// <param name="block">Header block</param>
    /// <param name="longNames">Whether the volume uses long names (DOS\6 and DOS\7)</param>
    /// <returns>Comment block number, or 0 if there is none</returns>
    internal static uint GetCommentBlockPointer(byte[] block, bool longNames) =>
        longNames && block.Length >= ENTRY_COMMENT_FROM_END
            ? BigEndianBitConverter.ToUInt32(block, block.Length - LONG_COMMENT_BLOCK_FROM_END)
            : 0;

    /// <summary>Gets the offset of the date stamp of a directory, file or link header block</summary>
    /// <param name="blockLength">Block size in bytes</param>
    /// <param name="longNames">Whether the volume uses long names (DOS\6 and DOS\7)</param>
    /// <returns>Offset of the days field, followed by the minutes and ticks</returns>
    internal static int GetEntryDateOffset(int blockLength, bool longNames) =>
        blockLength - (longNames ? LONG_DATE_FROM_END : ENTRY_DATE_FROM_END);

    /// <summary>Gets the name of a directory, file or link header block</summary>
    /// <param name="block">Header block</param>
    /// <returns>Name</returns>
    string GetEntryName(byte[] block)
    {
        byte[] name = GetEntryNameBytes(block, _hasLongNames);

        return name.Length > 0 ? _encoding.GetString(name) : string.Empty;
    }

    /// <summary>Gets the comment of a directory, file or link header block</summary>
    /// <param name="block">Header block</param>
    /// <param name="blockNum">Number of the header block</param>
    /// <param name="comment">Comment bytes, empty if there is none</param>
    /// <returns>Error code</returns>
    ErrorNumber GetEntryComment(byte[] block, uint blockNum, out byte[] comment)
    {
        comment = GetInlineCommentBytes(block, _hasLongNames);

        if(comment.Length > 0) return ErrorNumber.NoError;

        // Long name blocks keep the comment in its own block when it does not fit next to the name
        uint commentBlock = GetCommentBlockPointer(block, _hasLongNames);

        if(commentBlock == 0) return ErrorNumber.NoError;

        ErrorNumber errno = ReadBlock(commentBlock, out byte[] commentData);

        if(errno != ErrorNumber.NoError) return errno;

        var type      = BigEndianBitConverter.ToUInt32(commentData, 0x00);
        var ownKey    = BigEndianBitConverter.ToUInt32(commentData, 0x04);
        var headerKey = BigEndianBitConverter.ToUInt32(commentData, 0x08);

        if(type != TYPE_COMMENT || ownKey != commentBlock || headerKey != blockNum)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Invalid comment block {0} for header {1}: type {2}, own key {3}, header key {4}",
                              commentBlock,
                              blockNum,
                              type,
                              ownKey,
                              headerKey);

            return ErrorNumber.InvalidArgument;
        }

        var checksum                      = BigEndianBitConverter.ToUInt32(commentData, 20);
        commentData[20] = commentData[21] = commentData[22] = commentData[23] = 0;

        if(AmigaChecksum(commentData) != checksum)
        {
            AaruLogging.Debug(MODULE_NAME, "Invalid checksum in comment block {0}", commentBlock);

            return ErrorNumber.InvalidArgument;
        }

        int commentLength = Math.Min((int)commentData[COMMENT_BLOCK_TEXT_OFFSET], MAX_COMMENT_LENGTH);

        comment = new byte[commentLength];
        Array.Copy(commentData, COMMENT_BLOCK_TEXT_OFFSET + 1, comment, 0, commentLength);

        return ErrorNumber.NoError;
    }

    static uint AmigaChecksum(byte[] data)
    {
        uint sum = 0;

        for(var i = 0; i < data.Length; i += 4)
            sum += (uint)((data[i] << 24) + (data[i + 1] << 16) + (data[i + 2] << 8) + data[i + 3]);

        return (uint)-sum;
    }

    static uint AmigaBootChecksum(byte[] data)
    {
        uint sum = 0;

        for(var i = 0; i < data.Length; i += 4)
        {
            uint psum = sum;

            if((sum += (uint)((data[i] << 24) + (data[i + 1] << 16) + (data[i + 2] << 8) + data[i + 3])) < psum) sum++;
        }

        return ~sum;
    }
}