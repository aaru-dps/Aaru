// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Files-11 On-Disk Structure plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Directory operations for the Files-11 On-Disk Structure.
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
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Filesystems;

public sealed partial class ODS
{
    /// <inheritdoc />
    public ErrorNumber OpenDir(string path, out IDirNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize path
        string normalizedPath = string.IsNullOrWhiteSpace(path) ? "/" : path;

        if(!normalizedPath.StartsWith("/", StringComparison.Ordinal)) normalizedPath = "/" + normalizedPath;

        // Root directory case
        if(normalizedPath == "/")
        {
            if(_rootDirectoryCache == null) return ErrorNumber.InvalidArgument;

            node = new OdsDirNode
            {
                Path     = "/",
                Position = 0,
                Entries  = GetDirectoryEntries(_rootDirectoryCache)
            };

            return ErrorNumber.NoError;
        }

        // Parse path components
        string cutPath = normalizedPath[1..]; // Remove leading '/'

        string[] pieces = cutPath.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);

        if(pieces.Length == 0) return ErrorNumber.NoSuchFile;

        // Start from root directory
        Dictionary<string, CachedFile> currentDirectory = _rootDirectoryCache;

        // Traverse through path components
        for(var p = 0; p < pieces.Length; p++)
        {
            string component = pieces[p].ToUpperInvariant();

            // ODS filenames may include version - strip it for lookup
            int versionPos = component.IndexOf(';');

            if(versionPos >= 0) component = component[..versionPos];

            // Look for the component in current directory
            if(!currentDirectory.TryGetValue(component, out CachedFile cachedFile)) return ErrorNumber.NoSuchFile;

            // Read file header to check if it's a directory
            ErrorNumber errno = ReadFileHeader(cachedFile.Fid, out FileHeader fileHeader);

            if(errno != ErrorNumber.NoError) return errno;

            if(!fileHeader.filechar.HasFlag(FileCharacteristicFlags.Directory)) return ErrorNumber.NotDirectory;

            // Read directory entries, skipping self-referential entry
            errno = ReadDirectoryEntries(fileHeader, out Dictionary<string, CachedFile> dirEntries, cachedFile.Fid);

            if(errno != ErrorNumber.NoError) return errno;

            // If this is the last component, we're opening this directory
            if(p == pieces.Length - 1)
            {
                node = new OdsDirNode
                {
                    Path     = normalizedPath,
                    Position = 0,
                    Entries  = GetDirectoryEntries(dirEntries)
                };

                return ErrorNumber.NoError;
            }

            // Not the last component - move to next level
            currentDirectory = dirEntries;
        }

        return ErrorNumber.NoSuchFile;
    }

    /// <inheritdoc />
    public ErrorNumber CloseDir(IDirNode node)
    {
        if(node is not OdsDirNode myNode) return ErrorNumber.InvalidArgument;

        myNode.Position = -1;
        myNode.Entries  = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadDir(IDirNode node, out string filename)
    {
        filename = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not OdsDirNode myNode) return ErrorNumber.InvalidArgument;

        if(myNode.Position < 0) return ErrorNumber.InvalidArgument;

        // End of directory
        if(myNode.Position >= myNode.Entries.Length) return ErrorNumber.NoError;

        // Get current filename and advance position
        filename = myNode.Entries[myNode.Position++].Filename;

        return ErrorNumber.NoError;
    }

    /// <summary>Gets directory entries filtered by the current namespace.</summary>
    /// <param name="cache">Directory cache with all entries.</param>
    /// <returns>Array of entries appropriate for the current namespace.</returns>
    (string Filename, CachedFile File)[] GetDirectoryEntries(Dictionary<string, CachedFile> cache)
    {
        if(_namespace == NAMESPACE_NOVERSIONS)
        {
            // noversions namespace - show only entries without version suffix (latest version)
            return cache.Where(static kvp => !kvp.Key.Contains(';'))
                        .OrderBy(static k => k.Key)
                        .Select(static kvp => (kvp.Key, kvp.Value))
                        .ToArray();
        }

        // default namespace - show all entries WITH version suffix (FILE;1, FILE;2, etc)
        return cache.Where(static kvp => kvp.Key.Contains(';'))
                    .OrderBy(static k => k.Key)
                    .Select(static kvp => (kvp.Key, kvp.Value))
                    .ToArray();
    }

    /// <summary>Reads directory entries from a directory file header.</summary>
    /// <param name="dirHeader">File header of the directory.</param>
    /// <param name="entries">Output dictionary of cached entries.</param>
    /// <param name="skipFid">Optional file ID to skip (for filtering self-referential entries).</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber ReadDirectoryEntries(in FileHeader dirHeader, out Dictionary<string, CachedFile> entries,
                                     FileId        skipFid = default)
    {
        entries = new Dictionary<string, CachedFile>();

        return ReadDirectoryEntriesInto(dirHeader, entries, skipFid);
    }

    /// <summary>Reads directory entries from a directory file header into an existing cache.</summary>
    /// <remarks>
    ///     Directories can span more than one file header, so the extension header chain is followed and the whole
    ///     multi-extent map is used. A directory that cannot be fully mapped is an error, not a short listing.
    /// </remarks>
    /// <param name="dirHeader">File header of the directory.</param>
    /// <param name="entries">Cache dictionary to populate.</param>
    /// <param name="skipFid">Optional file ID to skip (for filtering self-referential entries).</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber ReadDirectoryEntriesInto(in FileHeader dirHeader, Dictionary<string, CachedFile> entries,
                                         FileId        skipFid)
    {
        // Get mapping information
        byte[] mapData = GetMapData(dirHeader);

        if(mapData == null || mapData.Length == 0) return ErrorNumber.NoError; // Empty directory

        // Calculate file size from FAT
        long fileSize = ((long)dirHeader.recattr.efblk.Value - 1) * ODS_BLOCK_SIZE + dirHeader.recattr.ffbyte;

        if(fileSize <= 0) return ErrorNumber.NoError; // Empty directory

        // Directories big enough to need extension headers must follow the whole chain
        var dirNode = new OdsFileNode
        {
            Fid        = dirHeader.fid,
            FileHeader = dirHeader,
            MapData    = mapData
        };

        ErrorNumber errno = LoadExtensionHeaders(dirNode);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error loading directory extension headers: {0}", errno);

            return errno;
        }

        // Read directory contents VBN by VBN
        var vbn = 1;

        while((vbn - 1) * ODS_BLOCK_SIZE < fileSize)
        {
            errno = MapVbnToLbnMultiExtent(dirNode, (uint)vbn, out uint lbn, out _);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Error mapping directory VBN {0}: {1}", vbn, errno);

                return errno;
            }

            errno = ReadOdsBlock(_image, _partition, lbn, out byte[] dirBlock);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Error reading directory block at LBN {0}: {1}", lbn, errno);

                return errno;
            }

            // Parse directory entries in this block
            ParseDirectoryBlockToCache(dirBlock, entries, _encoding, skipFid);

            vbn++;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Compares two file IDs, including the file number extension and the sequence number.</summary>
    /// <param name="left">First file ID.</param>
    /// <param name="right">Second file ID.</param>
    /// <returns><c>true</c> if both file IDs designate the same file.</returns>
    static bool SameFileId(FileId left, FileId right) =>
        left.num == right.num && left.nmx == right.nmx && left.seq == right.seq;

    /// <summary>Parses directory entries from a directory block into a cache dictionary.</summary>
    /// <remarks>
    ///     Each directory record holds an array of (version, file ID) pairs, one per version of the file. All of them
    ///     are cached as <c>NAME;VERSION</c>, and the bare <c>NAME</c> key resolves to the highest version.
    /// </remarks>
    /// <param name="block">Directory block data.</param>
    /// <param name="cache">Cache dictionary to populate.</param>
    /// <param name="encoding">Encoding used for non-UCS-2 filenames.</param>
    /// <param name="skipFid">Optional file ID to skip (for filtering self-referential entries).</param>
    internal static void ParseDirectoryBlockToCache(byte[]                         block,
                                                   Dictionary<string, CachedFile> cache, Encoding encoding,
                                                   FileId                         skipFid = default)
    {
        bool skipping = skipFid.num != 0 || skipFid.nmx != 0;
        var  offset   = 0;

        while(offset < block.Length - 2)
        {
            // Check for end of records marker
            var size = BitConverter.ToUInt16(block, offset);

            if(size is NO_MORE_RECORDS or 0) break;

            // Ensure we have enough data for the record header
            if(offset + 6 > block.Length) break;

            byte flags     = block[offset + 4];
            byte namecount = block[offset + 5];

            // Extract name type from flags
            var nameType = (DirectoryNameType)(flags >> 3 & 0x07);

            // Read filename
            int nameOffset = offset + 6;

            if(nameOffset + namecount > block.Length) break;

            string filename = nameType == DirectoryNameType.Ucs2
                                  ? Encoding.Unicode.GetString(block, nameOffset, namecount)
                                  : encoding.GetString(block, nameOffset, namecount);

            // Value field (directory entries) starts after name, word-aligned
            int valueOffset = nameOffset + (namecount + 1 & ~1);

            // The record must be big enough to hold its own name plus at least one version, otherwise the
            // record size is corrupt and advancing by it would rescan the same region
            int recordEnd = offset + size + 2;

            if(recordEnd <= offset || recordEnd < valueOffset + 8) break;

            if(recordEnd > block.Length) recordEnd = block.Length;

            string bareName = filename.ToUpperInvariant();

            // A record holds one (version, file ID) pair per version of the file
            for(int value = valueOffset; value + 8 <= recordEnd; value += 8)
            {
                var entryVersion = BitConverter.ToUInt16(block, value);

                FileId fid = Marshal.ByteArrayToStructureLittleEndian<FileId>(block, value + 2, 6);

                // Skip self-referential entries (like 000000.DIR pointing to the MFD)
                if(skipping && SameFileId(fid, skipFid)) continue;

                // Store without version for directory listing, keeping the highest version
                if(!cache.TryGetValue(bareName, out CachedFile latest) || entryVersion > latest.Version)
                {
                    cache[bareName] = new CachedFile
                    {
                        Fid     = fid,
                        Version = entryVersion
                    };
                }

                // Store with version too
                cache[$"{bareName};{entryVersion}"] = new CachedFile
                {
                    Fid     = fid,
                    Version = entryVersion
                };
            }

            // Move to next record
            offset += size + 2; // size doesn't include the size field itself
        }
    }
}
