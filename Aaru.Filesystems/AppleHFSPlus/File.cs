// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Hierarchical File System Plus plugin.
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
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Filesystems;

// Information from Apple TechNote 1150: https://developer.apple.com/legacy/library/technotes/tn/tn1150.html
/// <summary>Implements detection of Apple Hierarchical File System Plus (HFS+)</summary>
public sealed partial class AppleHFSPlus
{
    /// <inheritdoc />
    public ErrorNumber OpenFile(string path, out IFileNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize path
        string normalizedPath = string.IsNullOrEmpty(path) ? "/" : path;

        // Root directory cannot be opened as a file
        if(string.Equals(normalizedPath, "/", StringComparison.OrdinalIgnoreCase)) return ErrorNumber.IsDirectory;

        // Parse path components
        string cutPath = normalizedPath.StartsWith("/", StringComparison.Ordinal)
                             ? normalizedPath[1..]
                             : normalizedPath;

        string[] pieces = cutPath.Split(['/'], StringSplitOptions.RemoveEmptyEntries);

        if(pieces.Length == 0) return ErrorNumber.NoSuchFile;

        // Start from root directory
        Dictionary<string, CatalogEntry> currentDirectory = _rootDirectoryCache;
        uint                             currentDirCNID   = kHFSRootFolderID;

        // Traverse through all but the last path component
        for(var p = 0; p < pieces.Length - 1; p++)
        {
            string component = pieces[p];

            // Convert colons back to slashes for catalog lookup
            component = component.Replace(":", "/");

            // Look for the component in current directory
            CatalogEntry foundEntry = null;

            if(currentDirectory != null)
            {
                foreach(KeyValuePair<string, CatalogEntry> entry in currentDirectory)
                {
                    if(string.Equals(entry.Key, component, StringComparison.OrdinalIgnoreCase))
                    {
                        foundEntry = entry.Value;

                        break;
                    }
                }
            }

            if(foundEntry == null) return ErrorNumber.NoSuchFile;

            // Check if it's a directory
            if(foundEntry.Type != (int)BTreeRecordType.kHFSPlusFolderRecord) return ErrorNumber.NotDirectory;

            // Update current directory info
            currentDirCNID = foundEntry.CNID;

            // Load next directory level
            ErrorNumber cacheErr = CacheDirectoryIfNeeded(currentDirCNID);

            if(cacheErr != ErrorNumber.NoError) return cacheErr;

            currentDirectory = GetDirectoryEntries(currentDirCNID);

            if(currentDirectory == null) return ErrorNumber.NoSuchFile;
        }

        // Now look for the final component (the file)
        string lastComponent = pieces[^1];

        // Convert colons back to slashes for catalog lookup
        lastComponent = lastComponent.Replace(":", "/");

        CatalogEntry finalEntry = null;

        if(currentDirectory != null)
        {
            foreach(KeyValuePair<string, CatalogEntry> entry in currentDirectory)
            {
                if(string.Equals(entry.Key, lastComponent, StringComparison.OrdinalIgnoreCase))
                {
                    finalEntry = entry.Value;

                    break;
                }
            }
        }

        if(finalEntry == null) return ErrorNumber.NoSuchFile;

        // Must be a file, not a directory
        if(finalEntry is not FileEntry fileEntry) return ErrorNumber.IsDirectory;

        // Check if file is compressed
        bool isCompressed = IsFileCompressed(fileEntry, out _, out ulong uncompressedSize);

        // Open the data fork (or decompressed version if compressed)
        node = new HfsPlusFileNode
        {
            Path             = normalizedPath,
            Length           = isCompressed ? (long)uncompressedSize : (long)fileEntry.DataForkLogicalSize,
            Offset           = 0,
            FileEntry        = fileEntry,
            AllExtents       = null, // Will be lazily loaded on first read
            IsCompressed     = isCompressed,
            DecompressedData = null // Will be lazily decompressed on first read
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not HfsPlusFileNode myNode) return ErrorNumber.InvalidArgument;

        // Clear references
        myNode.FileEntry        = null;
        myNode.AllExtents       = null;
        myNode.DecompressedData = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(buffer is null || buffer.Length < length) return ErrorNumber.InvalidArgument;

        if(node is not HfsPlusFileNode myNode) return ErrorNumber.InvalidArgument;

        // Clamp read length to remaining file size
        if(myNode.Offset + length > myNode.Length) length = myNode.Length - myNode.Offset;

        if(length <= 0) return ErrorNumber.NoError;

        // Handle compressed files transparently
        if(myNode.IsCompressed)
        {
            // Lazy decompress on first read
            if(myNode.DecompressedData == null)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "ReadFile: Decompressing file CNID={0} on first read",
                                  myNode.FileEntry.CNID);

                ErrorNumber decompErr = DecompressFile(myNode.FileEntry, out byte[] decompressedData);

                if(decompErr != ErrorNumber.NoError)
                {
                    AaruLogging.Debug(MODULE_NAME,
                                      "ReadFile: Failed to decompress file CNID={0}: {1}",
                                      myNode.FileEntry.CNID,
                                      decompErr);

                    return decompErr;
                }

                myNode.DecompressedData = decompressedData;
            }

            // Read from decompressed data
            var toRead = (int)Math.Min(length, myNode.DecompressedData.Length - myNode.Offset);

            if(toRead > 0)
            {
                Array.Copy(myNode.DecompressedData, (int)myNode.Offset, buffer, 0, toRead);
                myNode.Offset += toRead;
                read          =  toRead;
            }

            return ErrorNumber.NoError;
        }

        // Handle uncompressed files (existing code)
        // Lazy load extents if not already loaded
        if(myNode.AllExtents == null)
        {
            ErrorNumber extentErr = GetFileExtents(myNode.FileEntry.CNID,
                                                   myNode.FileEntry.DataForkExtents,
                                                   myNode.FileEntry.DataForkTotalBlocks,
                                                   out List<HFSPlusExtentDescriptor> allExtents);

            if(extentErr != ErrorNumber.NoError) return extentErr;

            myNode.AllExtents = allExtents ?? [];
        }

        if(myNode.AllExtents.Count == 0)
        {
            if(length > 0) return ErrorNumber.InvalidArgument;

            return ErrorNumber.NoError;
        }

        // Find the starting extent and offset within that extent
        long bytesProcessed = 0;
        long currentOffset  = myNode.Offset;
        long bytesToRead    = length;
        var  bufferPos      = 0;

        foreach(HFSPlusExtentDescriptor extent in myNode.AllExtents)
        {
            if(extent.blockCount == 0) break;

            ulong extentSizeBytes = (ulong)extent.blockCount * _volumeHeader.blockSize;

            // Skip extents that are entirely before our current offset
            if(currentOffset >= (long)extentSizeBytes)
            {
                currentOffset -= (long)extentSizeBytes;

                continue;
            }

            // Calculate how much to read from this extent
            var offsetInExtent   = (ulong)currentOffset;
            var toReadFromExtent = (ulong)Math.Min((long)extentSizeBytes - currentOffset, bytesToRead);

            // Calculate block offset for this extent
            ulong blockOffsetBytes = (ulong)extent.startBlock * _volumeHeader.blockSize + offsetInExtent;

            // Convert to device sector address
            // For wrapped volumes, blocks start after the HFS+ volume offset
            // For pure HFS+, _hfsPlusVolumeOffset is 0
            ulong deviceSector = ((_partitionStart + _hfsPlusVolumeOffset) * _sectorSize + blockOffsetBytes) /
                                 _sectorSize;

            var byteOffset = (uint)(((_partitionStart + _hfsPlusVolumeOffset) * _sectorSize + blockOffsetBytes) %
                                    _sectorSize);

            var sectorCount = (uint)((toReadFromExtent + byteOffset + _sectorSize - 1) / _sectorSize);

            AaruLogging.Debug(MODULE_NAME,
                              "ReadFile: Reading extent at block={0}, blockCount={1}, sectorCount={2}, toRead={3}",
                              extent.startBlock,
                              extent.blockCount,
                              sectorCount,
                              toReadFromExtent);

            ErrorNumber readErr = _imagePlugin.ReadSectors(deviceSector,
                                                           false,
                                                           sectorCount,
                                                           out byte[] sectorData,
                                                           out _);

            if(readErr != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "ReadFile: Failed to read sectors: {0}", readErr);

                return readErr;
            }

            if(sectorData == null || sectorData.Length == 0)
            {
                AaruLogging.Debug(MODULE_NAME, "ReadFile: Got empty sector data");

                return ErrorNumber.InvalidArgument;
            }

            // Copy data from sector buffer to output buffer, accounting for offset
            uint bytesToCopy = Math.Min((uint)toReadFromExtent, (uint)(sectorData.Length - byteOffset));

            Array.Copy(sectorData, (int)byteOffset, buffer, bufferPos, bytesToCopy);

            bufferPos      += (int)bytesToCopy;
            bytesProcessed += bytesToCopy;
            bytesToRead    -= bytesToCopy;

            // Reset offset for next extent
            currentOffset = 0;

            if(bytesToRead <= 0) break;
        }

        read          =  bytesProcessed;
        myNode.Offset += bytesProcessed;

        AaruLogging.Debug(MODULE_NAME, "ReadFile: Read {0} bytes, new offset={1}", bytesProcessed, myNode.Offset);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadLink(string path, out string dest)
    {
        dest = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize path
        string normalizedPath = string.IsNullOrEmpty(path) ? "/" : path;

        // Root directory cannot be a symlink
        if(string.Equals(normalizedPath, "/", StringComparison.OrdinalIgnoreCase)) return ErrorNumber.InvalidArgument;

        // Get the file entry
        ErrorNumber err = GetFileEntryForPath(normalizedPath, out CatalogEntry entry);

        if(err != ErrorNumber.NoError) return err;

        // Must be a file, not a directory
        if(entry is not FileEntry fileEntry) return ErrorNumber.InvalidArgument;

        // Check if it's a symbolic link
        // Symlinks have:
        // 1. File type in fileMode = S_IFLNK (0xA000 in octal, 0xA in the upper nibble of fileMode >> 12)
        // 2. File type 'slnk' (0x736C6E6B)
        // 3. Creator code 'rhap' (0x72686170)

        // Check file type from permissions
        if((fileEntry.permissions.fileMode & 0xF000) != 0xA000) return ErrorNumber.InvalidArgument;

        // Check Finder info file type and creator
        if(fileEntry.FinderInfo.fdType != 0x736C6E6B) // 'slnk'
            return ErrorNumber.InvalidArgument;

        if(fileEntry.FinderInfo.fdCreator != 0x72686170) // 'rhap'
            return ErrorNumber.InvalidArgument;

        // The symlink target is stored in the data fork
        if(fileEntry.DataForkLogicalSize == 0 || fileEntry.DataForkLogicalSize > 4096)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "ReadLink: Invalid symlink data fork size: {0}",
                              fileEntry.DataForkLogicalSize);

            return ErrorNumber.InvalidArgument;
        }

        // Open the file and read the data fork
        ErrorNumber openErr = OpenFile(normalizedPath, out IFileNode node);

        if(openErr != ErrorNumber.NoError) return openErr;

        var buffer = new byte[fileEntry.DataForkLogicalSize];

        ErrorNumber readErr = ReadFile(node, (long)fileEntry.DataForkLogicalSize, buffer, out long bytesRead);

        _ = CloseFile(node);

        if(readErr != ErrorNumber.NoError) return readErr;

        if(bytesRead != (long)fileEntry.DataForkLogicalSize)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "ReadLink: Failed to read full symlink data. Expected {0}, got {1}",
                              fileEntry.DataForkLogicalSize,
                              bytesRead);

            return ErrorNumber.InvalidArgument;
        }

        // The path is UTF-8 encoded with no null bytes
        // Validate it's valid UTF-8
        try
        {
            dest = Encoding.UTF8.GetString(buffer, 0, (int)bytesRead);

            // Verify no null bytes in the middle (should not exist in valid symlinks)
            if(dest.Contains('\0'))
            {
                AaruLogging.Debug(MODULE_NAME, "ReadLink: Symlink path contains null bytes");

                return ErrorNumber.InvalidArgument;
            }

            AaruLogging.Debug(MODULE_NAME, "ReadLink: Read symlink {0} -> {1}", normalizedPath, dest);

            return ErrorNumber.NoError;
        }
        catch(DecoderFallbackException)
        {
            AaruLogging.Debug(MODULE_NAME, "ReadLink: Symlink path is not valid UTF-8");

            return ErrorNumber.InvalidArgument;
        }
    }

    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize path
        string normalizedPath = string.IsNullOrEmpty(path) ? "/" : path;

        // Root directory case
        if(string.Equals(normalizedPath, "/", StringComparison.OrdinalIgnoreCase))
        {
            if(_rootFolder.folderID != kHFSRootFolderID) return ErrorNumber.InvalidArgument;

            FileAttributes attributes = FileAttributes.Directory;

            // Translate Finder flags to file attributes
            if(_rootFolder.userInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kIsAlias))
                attributes |= FileAttributes.Alias;

            if(_rootFolder.userInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kHasBundle))
                attributes |= FileAttributes.Bundle;

            if(_rootFolder.userInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kHasBeenInited))
                attributes |= FileAttributes.HasBeenInited;

            if(_rootFolder.userInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kHasCustomIcon))
                attributes |= FileAttributes.HasCustomIcon;

            if(_rootFolder.userInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kHasNoINITs))
                attributes |= FileAttributes.HasNoINITs;

            if(_rootFolder.userInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kIsInvisible))
                attributes |= FileAttributes.Hidden;

            // HFS+ specific: immutable flag from BSD adminFlags (UF_IMMUTABLE = 0x00000002)
            if((_rootFolder.permissions.adminFlags & 0x02) != 0) attributes |= FileAttributes.Immutable;

            // HFS+ specific: archived flag from BSD ownerFlags (SF_ARCHIVED = 0x00000001)
            if((_rootFolder.permissions.ownerFlags & 0x01) != 0) attributes |= FileAttributes.Archive;

            if(_rootFolder.userInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kIsOnDesk))
                attributes |= FileAttributes.IsOnDesk;

            if(_rootFolder.userInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kIsShared))
                attributes |= FileAttributes.Shared;

            if(_rootFolder.userInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kIsStationery))
                attributes |= FileAttributes.Stationery;

            stat = new FileEntryInfo
            {
                Attributes       = attributes,
                BlockSize        = _volumeHeader.blockSize,
                Inode            = _rootFolder.folderID,
                Links            = 1,
                CreationTime     = DateHandlers.MacToDateTime(_rootFolder.createDate),
                LastWriteTime    = DateHandlers.MacToDateTime(_rootFolder.contentModDate),
                LastWriteTimeUtc = DateHandlers.MacToDateTime(_rootFolder.contentModDate),
                AccessTime       = DateHandlers.MacToDateTime(_rootFolder.accessDate),
                AccessTimeUtc    = DateHandlers.MacToDateTime(_rootFolder.accessDate),
                BackupTime       = DateHandlers.MacToDateTime(_rootFolder.backupDate),
                BackupTimeUtc    = DateHandlers.MacToDateTime(_rootFolder.backupDate),
                UID              = _rootFolder.permissions.ownerID,
                GID              = _rootFolder.permissions.groupID,
                Mode             = (uint)_rootFolder.permissions.fileMode & 0x0000FFFF
            };

            return ErrorNumber.NoError;
        }

        // Parse path components
        string cutPath = normalizedPath.StartsWith("/", StringComparison.Ordinal)
                             ? normalizedPath[1..]
                             : normalizedPath;

        string[] pieces = cutPath.Split(['/'], StringSplitOptions.RemoveEmptyEntries);

        if(pieces.Length == 0) return ErrorNumber.NoSuchFile;

        // Start from root directory
        Dictionary<string, CatalogEntry> currentDirectory = _rootDirectoryCache;
        uint                             currentDirCNID   = kHFSRootFolderID;

        // Traverse through all but the last path component
        for(var p = 0; p < pieces.Length - 1; p++)
        {
            string component = pieces[p];

            // Convert colons back to slashes for catalog lookup
            component = component.Replace(":", "/");

            // Look for the component in current directory
            CatalogEntry foundEntry = null;

            if(currentDirectory != null)
            {
                foreach(KeyValuePair<string, CatalogEntry> entry in currentDirectory)
                {
                    if(string.Equals(entry.Key, component, StringComparison.OrdinalIgnoreCase))
                    {
                        foundEntry = entry.Value;

                        break;
                    }
                }
            }

            if(foundEntry == null) return ErrorNumber.NoSuchFile;

            // Check if it's a directory
            if(foundEntry.Type != (int)BTreeRecordType.kHFSPlusFolderRecord) return ErrorNumber.NotDirectory;

            // Update current directory info
            currentDirCNID = foundEntry.CNID;

            // Load next directory level
            ErrorNumber cacheErr = CacheDirectoryIfNeeded(currentDirCNID);

            if(cacheErr != ErrorNumber.NoError) return cacheErr;

            currentDirectory = GetDirectoryEntries(currentDirCNID);

            if(currentDirectory == null) return ErrorNumber.NoSuchFile;
        }

        // Now look for the final component
        string lastComponent = pieces[pieces.Length - 1];

        // Convert colons back to slashes for catalog lookup
        lastComponent = lastComponent.Replace(":", "/");

        CatalogEntry finalEntry = null;

        if(currentDirectory != null)
        {
            foreach(KeyValuePair<string, CatalogEntry> entry in currentDirectory)
            {
                if(string.Equals(entry.Key, lastComponent, StringComparison.OrdinalIgnoreCase))
                {
                    finalEntry = entry.Value;

                    break;
                }
            }
        }

        if(finalEntry == null) return ErrorNumber.NoSuchFile;

        // Populate stat info based on entry type
        if(finalEntry is DirectoryEntry dirEntry)
        {
            FileAttributes attributes = FileAttributes.Directory;

            // Translate Finder flags to file attributes
            if(dirEntry.FinderInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kIsAlias))
                attributes |= FileAttributes.Alias;

            if(dirEntry.FinderInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kHasBundle))
                attributes |= FileAttributes.Bundle;

            if(dirEntry.FinderInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kHasBeenInited))
                attributes |= FileAttributes.HasBeenInited;

            if(dirEntry.FinderInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kHasCustomIcon))
                attributes |= FileAttributes.HasCustomIcon;

            if(dirEntry.FinderInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kHasNoINITs))
                attributes |= FileAttributes.HasNoINITs;

            if(dirEntry.FinderInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kIsInvisible))
                attributes |= FileAttributes.Hidden;

            // HFS+ specific: immutable flag from BSD adminFlags (UF_IMMUTABLE = 0x00000002)
            if((dirEntry.permissions.adminFlags & 0x02) != 0) attributes |= FileAttributes.Immutable;

            // HFS+ specific: archived flag from BSD ownerFlags (SF_ARCHIVED = 0x00000001)
            if((dirEntry.permissions.ownerFlags & 0x01) != 0) attributes |= FileAttributes.Archive;

            if(dirEntry.FinderInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kIsOnDesk))
                attributes |= FileAttributes.IsOnDesk;

            if(dirEntry.FinderInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kIsShared))
                attributes |= FileAttributes.Shared;

            if(dirEntry.FinderInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kIsStationery))
                attributes |= FileAttributes.Stationery;

            stat = new FileEntryInfo
            {
                Attributes       = attributes,
                BlockSize        = _volumeHeader.blockSize,
                Inode            = dirEntry.CNID,
                Links            = 1,
                CreationTime     = DateHandlers.MacToDateTime(dirEntry.CreationDate),
                LastWriteTime    = DateHandlers.MacToDateTime(dirEntry.ContentModDate),
                LastWriteTimeUtc = DateHandlers.MacToDateTime(dirEntry.ContentModDate),
                AccessTime       = DateHandlers.MacToDateTime(dirEntry.AccessDate),
                AccessTimeUtc    = DateHandlers.MacToDateTime(dirEntry.AccessDate),
                BackupTime       = DateHandlers.MacToDateTime(dirEntry.BackupDate),
                BackupTimeUtc    = DateHandlers.MacToDateTime(dirEntry.BackupDate),
                UID              = dirEntry.permissions.ownerID,
                GID              = dirEntry.permissions.groupID,
                Mode             = (uint)dirEntry.permissions.fileMode & 0x0000FFFF
            };
        }
        else if(finalEntry is FileEntry fileEntry)
        {
            // Use data fork size as file length
            ulong fileSize = fileEntry.DataForkLogicalSize;

            FileAttributes attributes = FileAttributes.File;

            // Translate Finder flags to file attributes
            if(fileEntry.FinderInfo.fdFlags.HasFlag(AppleCommon.FinderFlags.kIsAlias))
                attributes |= FileAttributes.Alias;

            if(fileEntry.FinderInfo.fdFlags.HasFlag(AppleCommon.FinderFlags.kHasBundle))
                attributes |= FileAttributes.Bundle;

            if(fileEntry.FinderInfo.fdFlags.HasFlag(AppleCommon.FinderFlags.kHasBeenInited))
                attributes |= FileAttributes.HasBeenInited;

            if(fileEntry.FinderInfo.fdFlags.HasFlag(AppleCommon.FinderFlags.kHasCustomIcon))
                attributes |= FileAttributes.HasCustomIcon;

            if(fileEntry.FinderInfo.fdFlags.HasFlag(AppleCommon.FinderFlags.kHasNoINITs))
                attributes |= FileAttributes.HasNoINITs;

            if(fileEntry.FinderInfo.fdFlags.HasFlag(AppleCommon.FinderFlags.kIsInvisible))
                attributes |= FileAttributes.Hidden;

            // HFS+ specific: immutable flag from BSD adminFlags (UF_IMMUTABLE = 0x00000002)
            if((fileEntry.permissions.adminFlags & 0x02) != 0) attributes |= FileAttributes.Immutable;

            // HFS+ specific: archived flag from BSD ownerFlags (SF_ARCHIVED = 0x00000001)
            if((fileEntry.permissions.ownerFlags & 0x01) != 0) attributes |= FileAttributes.Archive;

            if(fileEntry.FinderInfo.fdFlags.HasFlag(AppleCommon.FinderFlags.kIsOnDesk))
                attributes |= FileAttributes.IsOnDesk;

            if(fileEntry.FinderInfo.fdFlags.HasFlag(AppleCommon.FinderFlags.kIsShared))
                attributes |= FileAttributes.Shared;

            if(fileEntry.FinderInfo.fdFlags.HasFlag(AppleCommon.FinderFlags.kIsStationery))
                attributes |= FileAttributes.Stationery;

            if(!attributes.HasFlag(FileAttributes.Alias)  &&
               !attributes.HasFlag(FileAttributes.Bundle) &&
               !attributes.HasFlag(FileAttributes.Stationery))
                attributes |= FileAttributes.File;

            stat = new FileEntryInfo
            {
                Attributes       = attributes,
                Blocks           = (long)((fileSize + _volumeHeader.blockSize - 1) / _volumeHeader.blockSize),
                BlockSize        = _volumeHeader.blockSize,
                Length           = (long)fileSize,
                Inode            = fileEntry.CNID,
                Links            = 1,
                CreationTime     = DateHandlers.MacToDateTime(fileEntry.CreationDate),
                LastWriteTime    = DateHandlers.MacToDateTime(fileEntry.ContentModDate),
                LastWriteTimeUtc = DateHandlers.MacToDateTime(fileEntry.ContentModDate),
                AccessTime       = DateHandlers.MacToDateTime(fileEntry.AccessDate),
                AccessTimeUtc    = DateHandlers.MacToDateTime(fileEntry.AccessDate),
                BackupTime       = DateHandlers.MacToDateTime(fileEntry.BackupDate),
                BackupTimeUtc    = DateHandlers.MacToDateTime(fileEntry.BackupDate),
                UID              = fileEntry.permissions.ownerID,
                GID              = fileEntry.permissions.groupID,
                Mode             = (uint)fileEntry.permissions.fileMode & 0x0000FFFF
            };
        }
        else
            return ErrorNumber.InvalidArgument;

        return ErrorNumber.NoError;
    }

    /// <summary>Gets a catalog entry (file or directory) by path</summary>
    /// <param name="path">Path to the entry</param>
    /// <param name="entry">The catalog entry if found</param>
    /// <returns>Error number</returns>
    private ErrorNumber GetFileEntryForPath(string path, out CatalogEntry entry)
    {
        entry = null;

        // Normalize path
        string normalizedPath = string.IsNullOrEmpty(path) ? "/" : path;

        // Root directory case
        if(string.Equals(normalizedPath, "/", StringComparison.OrdinalIgnoreCase))
            return ErrorNumber.InvalidArgument; // Root is a directory, not accessible via this method

        // Parse path components
        string cutPath = normalizedPath.StartsWith("/", StringComparison.Ordinal)
                             ? normalizedPath[1..]
                             : normalizedPath;

        string[] pieces = cutPath.Split(['/'], StringSplitOptions.RemoveEmptyEntries);

        if(pieces.Length == 0) return ErrorNumber.NoSuchFile;

        // Start from root directory
        Dictionary<string, CatalogEntry> currentDirectory = _rootDirectoryCache;

        // Traverse through all but the last path component
        for(var p = 0; p < pieces.Length - 1; p++)
        {
            string component = pieces[p];

            // Convert colons back to slashes for catalog lookup
            component = component.Replace(":", "/");

            // Look for the component in current directory
            CatalogEntry foundEntry = null;

            currentDirectory?.TryGetValue(component, out foundEntry);

            if(foundEntry == null) return ErrorNumber.NoSuchFile;

            // Check if it's a directory
            if(foundEntry.Type != (int)BTreeRecordType.kHFSPlusFolderRecord) return ErrorNumber.NotDirectory;

            // Load next directory level
            ErrorNumber cacheErr = CacheDirectoryIfNeeded(foundEntry.CNID);

            if(cacheErr != ErrorNumber.NoError) return cacheErr;

            currentDirectory = GetDirectoryEntries(foundEntry.CNID);

            if(currentDirectory == null) return ErrorNumber.NoSuchFile;
        }

        // Now look for the final component
        string lastComponent = pieces[^1];

        // Convert colons back to slashes for catalog lookup
        lastComponent = lastComponent.Replace(":", "/");

        if(currentDirectory == null) return ErrorNumber.NoSuchFile;

        if(currentDirectory.TryGetValue(lastComponent, out entry)) return ErrorNumber.NoError;

        return ErrorNumber.NoSuchFile;
    }

    /// <summary>Gets all extents for a file's data fork, including overflow extents</summary>
    /// <param name="cnid">Catalog Node ID of the file</param>
    /// <param name="firstExtents">First 8 extents from the catalog</param>
    /// <param name="totalBlocks">Total blocks for this fork</param>
    /// <param name="allExtents">List of all extents found</param>
    /// <returns>Error number</returns>
    private ErrorNumber GetFileExtents(uint cnid, HFSPlusExtentRecord firstExtents, uint totalBlocks,
                                       out List<HFSPlusExtentDescriptor> allExtents)
    {
        allExtents = [];

        // Add the first 8 extents from the fork data
        uint blocksProcessed = 0;

        foreach(HFSPlusExtentDescriptor extent in firstExtents.extentDescriptors)
        {
            if(extent.blockCount == 0) break;

            allExtents.Add(extent);
            blocksProcessed += extent.blockCount;

            AaruLogging.Debug(MODULE_NAME,
                              "GetFileExtents: Adding extent from catalog: startBlock={0}, blockCount={1}",
                              extent.startBlock,
                              extent.blockCount);
        }

        // If we've got all blocks, we're done
        if(blocksProcessed >= totalBlocks) return ErrorNumber.NoError;

        // We need to read overflow extents from the Extents Overflow File
        AaruLogging.Debug(MODULE_NAME, "GetFileExtents: Need to read overflow extents for file {0}", cnid);

        // Read overflow extents from the Extents Overflow File
        // Data fork has forkType = 0
        ErrorNumber overflowErr = ReadDataForkOverflowExtents(cnid, allExtents);

        if(overflowErr != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "GetFileExtents: Failed to read overflow extents: {0}", overflowErr);

            return overflowErr;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Reads overflow extents from the Extents Overflow File for a data fork</summary>
    /// <param name="cnid">Catalog Node ID of the file</param>
    /// <param name="allExtents">List to append overflow extents to</param>
    /// <returns>Error number</returns>
    private ErrorNumber ReadDataForkOverflowExtents(uint cnid, List<HFSPlusExtentDescriptor> allExtents)
    {
        if(_volumeHeader.extentsFile.totalBlocks == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "ReadDataForkOverflowExtents: No Extents Overflow File present");

            return ErrorNumber.NoError;
        }

        // Ensure the Extents Overflow File header is loaded
        ErrorNumber headerErr = EnsureExtentsFileHeaderLoaded();

        if(headerErr != ErrorNumber.NoError) return headerErr;

        AaruLogging.Debug(MODULE_NAME,
                          "ReadDataForkOverflowExtents: Searching Extents Overflow File for data fork extents (CNID={0})",
                          cnid);

        // Search the Extents Overflow File B-Tree for extent records with:
        // - CNID = cnid
        // - ForkType = 0 (data fork)
        ErrorNumber errno = SearchExtentsOverflowFile(cnid, 0, allExtents);

        return errno;
    }
}