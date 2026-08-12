// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Helpers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : EWF logical evidence plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains helper methods for Expert Witness Format logical evidence files.
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
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using Aaru.Compression;
using Aaru.Logging;

namespace Aaru.Archives;

public sealed partial class EwfArchive
{
    /// <summary>Generates the next segment filename following EWF naming conventions.</summary>
    static string GetNextSegmentFilename(string currentPath)
    {
        string dir  = Path.GetDirectoryName(currentPath) ?? "";
        string name = Path.GetFileNameWithoutExtension(currentPath);
        string ext  = Path.GetExtension(currentPath);

        if(string.IsNullOrEmpty(ext) || ext.Length < 2) return null;

        bool isV2    = ext.Length == 5;
        bool isLower = char.IsLower(ext[1]);

        if(isV2)
        {
            string numPart = ext.Substring(3);
            char   prefix1 = ext[1];
            char   prefix2 = ext[2];

            if(int.TryParse(numPart, out int num) && num < 99)
                return Path.Combine(dir, name + ext.Substring(0, 3) + (num + 1).ToString("D2"));

            if(int.TryParse(numPart, out _))
            {
                char a = isLower ? 'a' : 'A';

                return Path.Combine(dir, name + $".{prefix1}{prefix2}{a}{a}");
            }

            char c1       = ext[3];
            char c2       = ext[4];
            char baseChar = isLower ? 'a' : 'A';
            char maxChar  = isLower ? 'z' : 'Z';

            if(c2 < maxChar) return Path.Combine(dir, name + $".{prefix1}{prefix2}{c1}{(char)(c2 + 1)}");

            if(c1 < maxChar) return Path.Combine(dir, name + $".{prefix1}{prefix2}{(char)(c1 + 1)}{baseChar}");

            return null;
        }

        char   firstChar = ext[1];
        string suffix    = ext.Substring(2);

        if(int.TryParse(suffix, out int segNum) && segNum < 99)
            return Path.Combine(dir, name + $".{firstChar}{segNum + 1:D2}");

        if(int.TryParse(suffix, out _))
        {
            char a = isLower ? 'a' : 'A';

            return Path.Combine(dir, name + $".{firstChar}{a}{a}");
        }

        char ch1   = ext[1];
        char ch2   = ext[2];
        char ch3   = ext[3];
        char lBase = isLower ? 'a' : 'A';
        char lMax  = isLower ? 'z' : 'Z';

        if(ch3 < lMax) return Path.Combine(dir, name + $".{ch1}{ch2}{(char)(ch3 + 1)}");

        if(ch2 < lMax) return Path.Combine(dir, name + $".{ch1}{(char)(ch2 + 1)}{lBase}");

        if(ch1 < lMax) return Path.Combine(dir, name + $".{(char)(ch1 + 1)}{lBase}{lBase}");

        return null;
    }

    /// <summary>Decompresses zlib (RFC 1950) compressed data.</summary>
    static byte[] DecompressZlib(byte[] compressedData, int uncompressedSize)
    {
        if(compressedData == null || compressedData.Length < 3 || uncompressedSize <= 0)
            return new byte[Math.Max(uncompressedSize, 0)];

        var decompressed = new byte[uncompressedSize];

        using var ms        = new MemoryStream(compressedData, 2, compressedData.Length - 2);
        using var deflate   = new DeflateStream(ms, CompressionMode.Decompress);
        var       totalRead = 0;

        while(totalRead < uncompressedSize)
        {
            int read = deflate.Read(decompressed, totalRead, uncompressedSize - totalRead);

            if(read == 0) break;

            totalRead += read;
        }

        return decompressed;
    }

    /// <summary>Parses the ltree text data into a flat list of file entries.</summary>
    List<EwfFileEntry> ParseLtreeData(byte[] decompressedData)
    {
        string text;

        // ltree data is UTF-16 LE, may or may not have BOM
        if(decompressedData.Length >= 2 && decompressedData[0] == 0xFF && decompressedData[1] == 0xFE)
            text = Encoding.Unicode.GetString(decompressedData, 2, decompressedData.Length - 2);
        else
            text = Encoding.Unicode.GetString(decompressedData);

        var entries = new List<EwfFileEntry>();

        string[] lines     = text.Split('\n');
        var      lineIndex = 0;

        // Find the "entry" category
        while(lineIndex < lines.Length)
        {
            string line = lines[lineIndex].Trim('\r', ' ');

            if(line == "entry")
            {
                lineIndex++;

                break;
            }

            lineIndex++;
        }

        if(lineIndex >= lines.Length)
        {
            AaruLogging.Debug(MODULE_NAME, "No entry category found in ltree data");

            return entries;
        }

        // Skip empty lines after category name
        while(lineIndex < lines.Length && string.IsNullOrWhiteSpace(lines[lineIndex].Trim('\r', ' '))) lineIndex++;

        if(lineIndex >= lines.Length) return entries;

        // Read type indicators line (tab-separated field names)
        string   typeIndicatorLine = lines[lineIndex].Trim('\r', ' ');
        string[] fieldNames        = typeIndicatorLine.Split('\t');
        lineIndex++;

        // Skip empty lines
        while(lineIndex < lines.Length && string.IsNullOrWhiteSpace(lines[lineIndex].Trim('\r', ' '))) lineIndex++;

        // Parse root entry and its children recursively
        ParseLtreeEntries(lines, ref lineIndex, fieldNames, "", entries);

        return entries;
    }

    /// <summary>Recursively parses ltree entries.</summary>
    void ParseLtreeEntries(string[]           lines, ref int lineIndex, string[] fieldNames, string parentPath,
                           List<EwfFileEntry> entries)
    {
        if(lineIndex >= lines.Length) return;

        // First line of an entry: "<is_parent> <num_children>"
        string headerLine = lines[lineIndex].Trim('\r', ' ');
        lineIndex++;

        string[] headerParts = headerLine.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

        if(headerParts.Length < 2) return;

        // Parse the values line
        if(lineIndex >= lines.Length) return;

        string valuesLine = lines[lineIndex].Trim('\r', ' ');
        lineIndex++;

        string[] values = valuesLine.Split('\t');

        // Build field→value map
        var fieldMap = new Dictionary<string, string>();

        for(var i = 0; i < fieldNames.Length && i < values.Length; i++)
        {
            string key = fieldNames[i].Trim();

            if(!string.IsNullOrEmpty(key)) fieldMap[key] = values[i].Trim();
        }

        // Extract entry properties
        fieldMap.TryGetValue("n",   out string entryName);
        fieldMap.TryGetValue("id",  out string entryId);
        fieldMap.TryGetValue("ls",  out string logicalSize);
        fieldMap.TryGetValue("cr",  out string creationTime);
        fieldMap.TryGetValue("wr",  out string writeTime);
        fieldMap.TryGetValue("ac",  out string accessTime);
        fieldMap.TryGetValue("ha",  out string md5Hash);
        fieldMap.TryGetValue("sha", out string sha1Hash);
        fieldMap.TryGetValue("opr", out string flagsStr);
        fieldMap.TryGetValue("p",   out string isParent);
        fieldMap.TryGetValue("du",  out string duplicateOffset);
        fieldMap.TryGetValue("be",  out string binaryExtents);

        bool isDir = isParent == "1" ||
                     !string.IsNullOrEmpty(flagsStr)      &&
                     uint.TryParse(flagsStr, out uint fv) &&
                     (fv & (uint)EwfLefEntryFlags.Folder) != 0;

        // Build full path
        string fullPath;

        if(string.IsNullOrEmpty(entryName) || entryName == "NoName")
            fullPath = parentPath;
        else
            fullPath = string.IsNullOrEmpty(parentPath) ? entryName : parentPath + "/" + entryName;

        // Parse numeric fields
        long.TryParse(entryId,     out long id);
        long.TryParse(logicalSize, out long size);
        uint.TryParse(flagsStr, out uint flags);

        // Parse data location from binary extents
        long dataOffset = 0;
        long dataSize   = 0;

        if(!string.IsNullOrEmpty(binaryExtents)) ParseBinaryExtents(binaryExtents, out dataOffset, out dataSize);

        // Parse duplicate offset if no binary extents
        if(dataSize == 0 && !string.IsNullOrEmpty(duplicateOffset) && duplicateOffset != "-1")
            long.TryParse(duplicateOffset, out dataOffset);

        // Parse timestamps
        DateTime creation = ParsePosixTimestamp(creationTime);
        DateTime modify   = ParsePosixTimestamp(writeTime);
        DateTime access   = ParsePosixTimestamp(accessTime);

        // Skip root entry (empty path), but add all others
        if(!string.IsNullOrEmpty(fullPath))
        {
            entries.Add(new EwfFileEntry
            {
                Id           = id,
                Name         = entryName ?? "",
                FullPath     = fullPath,
                IsDirectory  = isDir,
                Size         = size,
                DataOffset   = dataOffset,
                DataSize     = dataSize > 0 ? dataSize : size,
                CreationTime = creation,
                ModifyTime   = modify,
                AccessTime   = access,
                Md5Hash      = md5Hash,
                Sha1Hash     = sha1Hash,
                Flags        = flags
            });
        }

        // Parse children
        if(!int.TryParse(headerParts[1], out int numChildren)) return;

        for(var i = 0; i < numChildren; i++)
        {
            if(lineIndex >= lines.Length) break;

            ParseLtreeEntries(lines, ref lineIndex, fieldNames, fullPath, entries);
        }
    }

    /// <summary>Parses binary extents field: "count offset_hex size_hex"</summary>
    static void ParseBinaryExtents(string extents, out long offset, out long size)
    {
        offset = 0;
        size   = 0;

        string[] parts = extents.Split([' '], StringSplitOptions.RemoveEmptyEntries);

        if(parts.Length < 3) return;

        // First part is count, then pairs of offset+size in hex
        long.TryParse(parts[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out offset);
        long.TryParse(parts[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out size);
    }

    /// <summary>Parses a POSIX timestamp string to DateTime.</summary>
    static DateTime ParsePosixTimestamp(string timestamp)
    {
        if(string.IsNullOrEmpty(timestamp) || !long.TryParse(timestamp, out long epoch) || epoch <= 0)
            return DateTime.MinValue;

        try
        {
            return DateTimeOffset.FromUnixTimeSeconds(epoch).UtcDateTime;
        }
        catch
        {
            return DateTime.MinValue;
        }
    }

    /// <summary>Reads and decompresses a chunk.</summary>
    internal byte[] ReadChunk(ulong chunkIndex)
    {
        if(_chunkCache.TryGetValue(chunkIndex, out byte[] cached)) return cached;

        if(!_chunkTable.TryGetValue(chunkIndex, out (int segmentIndex, long offset, uint size, bool compressed) chunk))
            return null;

        Stream segStream = _segmentStreams[chunk.segmentIndex];

        // Reject chunks pointing outside their segment file
        if(chunk.offset < 0 || chunk.size == 0 || chunk.offset + chunk.size > segStream.Length) return null;

        segStream.Seek(chunk.offset, SeekOrigin.Begin);

        byte[] chunkData;

        if(chunk.compressed)
        {
            var compressedData = new byte[chunk.size];
            segStream.ReadExactly(compressedData, 0, (int)chunk.size);

            if(_isV2 && _compressionMethod == EwfCompressionMethod.Bzip2)
            {
                chunkData = new byte[_chunkSize];
                BZip2.DecodeBuffer(compressedData, chunkData);
            }
            else
                chunkData = DecompressZlib(compressedData, (int)_chunkSize);
        }
        else
        {
            uint dataLen = chunk.size >= 4 ? chunk.size - 4 : chunk.size;
            chunkData = new byte[dataLen];
            segStream.ReadExactly(chunkData, 0, (int)dataLen);
        }

        if(_chunkCache.Count >= _maxChunkCache) _chunkCache.Clear();

        _chunkCache[chunkIndex] = chunkData;

        return chunkData;
    }
}