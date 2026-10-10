// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Hierarchical File System plugin.
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

// ReSharper disable UnusedType.Local

// ReSharper disable IdentifierTypo
// ReSharper disable MemberCanBePrivate.Local

using System;
using System.Collections.Generic;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Filesystems;

// Information from Inside Macintosh
// https://developer.apple.com/legacy/library/documentation/mac/pdf/Files/File_Manager.pdf
public sealed partial class AppleHFS
{
    /// <inheritdoc />
    public ErrorNumber OpenFile(string path, out IFileNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Get the file entry
        ErrorNumber err = GetFileEntry(path, out CatalogEntry entry);

        if(err != ErrorNumber.NoError) return err;

        if(entry is not FileEntry fileEntry) return ErrorNumber.IsDirectory;

        // We'll open the data fork by default
        node = new HfsFileNode
        {
            Path         = path,
            Length       = fileEntry.DataForkLogicalSize,
            Offset       = 0,
            FileEntry    = fileEntry,
            ForkType     = ForkType.Data,
            FirstExtents = fileEntry.DataForkExtents,
            AllExtents   = null // Will be lazily loaded on first read if needed
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not HfsFileNode myNode) return ErrorNumber.InvalidArgument;

        // Clear references
        myNode.FileEntry  = null;
        myNode.AllExtents = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(buffer is null || buffer.Length < length) return ErrorNumber.InvalidArgument;

        if(node is not HfsFileNode myNode) return ErrorNumber.InvalidArgument;

        // Clamp read length to remaining file size
        if(myNode.Offset + length > myNode.Length) length = myNode.Length - myNode.Offset;

        if(length <= 0) return ErrorNumber.NoError;

        // Lazy load extents if not already loaded
        if(myNode.AllExtents == null)
        {
            ErrorNumber extentErr = GetFileExtents(myNode.FileEntry.CNID,
                                                   myNode.ForkType,
                                                   myNode.FirstExtents,
                                                   out List<ExtDescriptor> allExtents);

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

        foreach(ExtDescriptor extent in myNode.AllExtents)
        {
            if(extent.xdrNumABlks == 0) break;

            uint extentSizeBytes = extent.xdrNumABlks * _mdb.drAlBlkSiz;

            // Skip extents that are entirely before our current offset
            if(currentOffset >= extentSizeBytes)
            {
                currentOffset -= extentSizeBytes;

                continue;
            }

            // Calculate how much to read from this extent
            var offsetInExtent   = (uint)currentOffset;
            var toReadFromExtent = (uint)Math.Min(extentSizeBytes - offsetInExtent, bytesToRead);

            // Calculate sector info for this extent
            // HFS uses 512-byte sectors internally; extent.xdrStABN is in allocation blocks
            // drAlBlSt is the first allocation block start sector (in 512-byte sectors)
            ulong extentOffsetSector512 =
                _mdb.drAlBlSt + (ulong)extent.xdrStABN * _mdb.drAlBlkSiz / 512 + offsetInExtent / 512;

            // Convert to device sector address
            HfsOffsetToDeviceSector(extentOffsetSector512, out ulong deviceSector, out uint byteOffset);

            // Adjust byte offset for offset within the first extent
            byteOffset += offsetInExtent % 512;
            uint sectorCount = (toReadFromExtent + byteOffset + _sectorSize - 1) / _sectorSize;

            AaruLogging.Debug(MODULE_NAME,
                              "ReadFile: Reading extent at block={0}, offset={1}, sectorCount={2}, toRead={3}",
                              extent.xdrStABN,
                              offsetInExtent,
                              sectorCount,
                              toReadFromExtent);

            ErrorNumber readErr =
                _imagePlugin.ReadSectors(deviceSector, false, sectorCount, out byte[] sectorData, out _);

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
            uint bytesToCopy = Math.Min(toReadFromExtent, (uint)(sectorData.Length - byteOffset));

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
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize path - accept both '/' and ':' as path separators
        // readdir returns paths with ':' for display, we split on '/' first then convert ':' in components
        string normalizedPath = string.IsNullOrEmpty(path) ? "/" : path;

        // Root directory case
        if(string.Equals(normalizedPath, "/", StringComparison.OrdinalIgnoreCase))
        {
            if(_rootDirectory.dirDirID != kRootCnid) return ErrorNumber.InvalidArgument;

            FileAttributes attributes = FileAttributes.Directory;

            // Translate Finder flags to file attributes (matches AppleMFS pattern)
            if(_rootDirectory.dirUsrInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kIsAlias))
                attributes |= FileAttributes.Alias;

            if(_rootDirectory.dirUsrInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kHasBundle))
                attributes |= FileAttributes.Bundle;

            if(_rootDirectory.dirUsrInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kHasBeenInited))
                attributes |= FileAttributes.HasBeenInited;

            if(_rootDirectory.dirUsrInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kHasCustomIcon))
                attributes |= FileAttributes.HasCustomIcon;

            if(_rootDirectory.dirUsrInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kHasNoINITs))
                attributes |= FileAttributes.HasNoINITs;

            if(_rootDirectory.dirUsrInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kIsInvisible))
                attributes |= FileAttributes.Hidden;

            if(_rootDirectory.dirFndrInfo.frXFlags.HasFlag(AppleCommon.ExtendedFinderFlags.kExtendedFlagIsImmutable))
                attributes |= FileAttributes.Immutable;

            if(_rootDirectory.dirUsrInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kIsOnDesk))
                attributes |= FileAttributes.IsOnDesk;

            if(_rootDirectory.dirUsrInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kIsShared))
                attributes |= FileAttributes.Shared;

            if(_rootDirectory.dirUsrInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kIsStationery))
                attributes |= FileAttributes.Stationery;

            stat = new FileEntryInfo
            {
                Attributes       = attributes,
                BlockSize        = _mdb.drAlBlkSiz,
                Inode            = _rootDirectory.dirDirID,
                Links            = 1,
                CreationTime     = DateHandlers.MacToDateTime(_rootDirectory.dirCrDat),
                LastWriteTime    = DateHandlers.MacToDateTime(_rootDirectory.dirMdDat),
                LastWriteTimeUtc = DateHandlers.MacToDateTime(_rootDirectory.dirMdDat),
                AccessTime       = DateHandlers.MacToDateTime(_rootDirectory.dirMdDat),
                AccessTimeUtc    = DateHandlers.MacToDateTime(_rootDirectory.dirMdDat),
                BackupTime       = DateHandlers.MacToDateTime(_rootDirectory.dirBkDat),
                BackupTimeUtc    = DateHandlers.MacToDateTime(_rootDirectory.dirBkDat)
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
        uint                             currentDirCNID   = kRootCnid;

        // Traverse through all but the last path component
        for(var p = 0; p < pieces.Length - 1; p++)
        {
            string component = pieces[p];

            // Convert colons to slashes in component (readdir returns display names with colons)
            component = component.Replace(":", "/");

            // Look for the component in current directory
            KeyValuePair<string, CatalogEntry> foundEntry = default;
            var                                found      = false;

            if(currentDirectory != null)
            {
                foreach(KeyValuePair<string, CatalogEntry> entry in currentDirectory)
                {
                    if(string.Equals(entry.Key, component, StringComparison.OrdinalIgnoreCase))
                    {
                        foundEntry = entry;
                        found      = true;

                        break;
                    }
                }
            }

            if(!found) return ErrorNumber.NoSuchFile;

            CatalogEntry catalogEntry = foundEntry.Value;

            // Check if it's a directory
            if(catalogEntry.Type != kCatalogRecordTypeDirectory) return ErrorNumber.NotDirectory;

            // Update current directory info
            currentDirCNID = catalogEntry.CNID;

            // Load next directory level
            ErrorNumber cacheErr = CacheDirectoryIfNeeded(currentDirCNID);

            if(cacheErr != ErrorNumber.NoError) return cacheErr;

            currentDirectory = GetDirectoryEntries(currentDirCNID);

            if(currentDirectory == null) return ErrorNumber.NoSuchFile;
        }

        // Now look for the final component
        string lastComponent = pieces[pieces.Length - 1];

        // Convert colons to slashes in component (readdir returns display names with colons)
        lastComponent = lastComponent.Replace(":", "/");

        KeyValuePair<string, CatalogEntry> finalEntry = default;
        var                                foundFinal = false;

        if(currentDirectory != null)
        {
            foreach(KeyValuePair<string, CatalogEntry> entry in currentDirectory)
            {
                if(string.Equals(entry.Key, lastComponent, StringComparison.OrdinalIgnoreCase))
                {
                    finalEntry = foundFinal ? throw new InvalidOperationException("Duplicate file entries") : entry;
                    foundFinal = true;
                }
            }
        }

        if(!foundFinal) return ErrorNumber.NoSuchFile;

        CatalogEntry finalCatalogEntry = finalEntry.Value;

        // Populate stat info based on entry type
        if(finalCatalogEntry is DirectoryEntry dirEntry)
        {
            FileAttributes attributes = FileAttributes.Directory;

            // Translate Finder flags to file attributes (matches AppleMFS pattern)
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

            if(dirEntry.ExtendedFinderInfo.frXFlags.HasFlag(AppleCommon.ExtendedFinderFlags.kExtendedFlagIsImmutable))
                attributes |= FileAttributes.Immutable;

            if(dirEntry.FinderInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kIsOnDesk))
                attributes |= FileAttributes.IsOnDesk;

            if(dirEntry.FinderInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kIsShared))
                attributes |= FileAttributes.Shared;

            if(dirEntry.FinderInfo.frFlags.HasFlag(AppleCommon.FinderFlags.kIsStationery))
                attributes |= FileAttributes.Stationery;

            stat = new FileEntryInfo
            {
                Attributes       = attributes,
                BlockSize        = _mdb.drAlBlkSiz,
                Inode            = dirEntry.CNID,
                Links            = 1,
                CreationTime     = DateHandlers.MacToDateTime(dirEntry.CreationDate),
                LastWriteTime    = DateHandlers.MacToDateTime(dirEntry.ModificationDate),
                LastWriteTimeUtc = DateHandlers.MacToDateTime(dirEntry.ModificationDate),
                AccessTime       = DateHandlers.MacToDateTime(dirEntry.ModificationDate),
                AccessTimeUtc    = DateHandlers.MacToDateTime(dirEntry.ModificationDate),
                BackupTime       = DateHandlers.MacToDateTime(dirEntry.BackupDate),
                BackupTimeUtc    = DateHandlers.MacToDateTime(dirEntry.BackupDate)
            };
        }
        else if(finalCatalogEntry is FileEntry fileEntry)
        {
            // Use data fork size as file length
            uint fileSize = fileEntry.DataForkLogicalSize;

            FileAttributes attributes = FileAttributes.File;

            // Translate Finder flags to file attributes (matches AppleMFS pattern)
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

            if(fileEntry.ExtendedFinderInfo.fdXFlags.HasFlag(AppleCommon.ExtendedFinderFlags.kExtendedFlagIsImmutable))
                attributes |= FileAttributes.Immutable;

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
                Blocks           = (fileSize + _mdb.drAlBlkSiz - 1) / _mdb.drAlBlkSiz,
                BlockSize        = _mdb.drAlBlkSiz,
                Length           = fileSize,
                Inode            = fileEntry.CNID,
                Links            = 1,
                CreationTime     = DateHandlers.MacToDateTime(fileEntry.CreationDate),
                LastWriteTime    = DateHandlers.MacToDateTime(fileEntry.ModificationDate),
                LastWriteTimeUtc = DateHandlers.MacToDateTime(fileEntry.ModificationDate),
                AccessTime       = DateHandlers.MacToDateTime(fileEntry.ModificationDate),
                BackupTime       = DateHandlers.MacToDateTime(fileEntry.BackupDate),
                BackupTimeUtc    = DateHandlers.MacToDateTime(fileEntry.BackupDate)
            };
        }
        else
            return ErrorNumber.InvalidArgument;

        return ErrorNumber.NoError;
    }

    /// <summary>Gets a catalog entry (file or directory) by path</summary>
    /// <param name="path">Path to the file or directory</param>
    /// <param name="entry">The catalog entry if found</param>
    /// <returns>Error number</returns>
    private ErrorNumber GetFileEntry(string path, out CatalogEntry entry)
    {
        entry = null;

        // Root directory case
        if(string.Equals(path, "/", StringComparison.OrdinalIgnoreCase))
        {
            // Return a pseudo-directory entry for root
            entry = new DirectoryEntry
            {
                CNID               = _rootDirectory.dirDirID,
                ParentID           = kRootParentCnid,
                Type               = kCatalogRecordTypeDirectory,
                Valence            = _rootDirectory.dirVal,
                FinderInfo         = _rootDirectory.dirUsrInfo,
                ExtendedFinderInfo = _rootDirectory.dirFndrInfo,
                CreationDate       = _rootDirectory.dirCrDat,
                ModificationDate   = _rootDirectory.dirMdDat,
                BackupDate         = _rootDirectory.dirBkDat
            };

            return ErrorNumber.NoError;
        }

        // Parse path components
        string cutPath = path.StartsWith("/", StringComparison.Ordinal) ? path[1..] : path;

        string[] pieces = cutPath.Split(['/'], StringSplitOptions.RemoveEmptyEntries);

        if(pieces.Length == 0) return ErrorNumber.NoSuchFile;

        // Start from root directory
        Dictionary<string, CatalogEntry> currentDirectory = _rootDirectoryCache;

        // Traverse through all path components
        for(var p = 0; p < pieces.Length; p++)
        {
            string component = pieces[p];

            // Convert colons to slashes in component (readdir returns display names with colons)
            component = component.Replace(":", "/");

            // Look for the component in current directory
            var          found        = false;
            CatalogEntry catalogEntry = null;

            if(currentDirectory != null)
            {
                foreach(KeyValuePair<string, CatalogEntry> dirEntry in currentDirectory)
                {
                    if(!string.Equals(dirEntry.Key, component, StringComparison.OrdinalIgnoreCase)) continue;

                    catalogEntry = dirEntry.Value;
                    found        = true;

                    break;
                }
            }

            if(!found) return ErrorNumber.NoSuchFile;

            // Last component
            if(p == pieces.Length - 1)
            {
                entry = catalogEntry;

                return ErrorNumber.NoError;
            }

            // Not last component - must be a directory
            if(catalogEntry.Type != kCatalogRecordTypeDirectory) return ErrorNumber.NotDirectory;

            // Update current directory info
            uint currentDirCnid = catalogEntry.CNID;

            // Load next directory level
            ErrorNumber cacheErr = CacheDirectoryIfNeeded(currentDirCnid);

            if(cacheErr != ErrorNumber.NoError) return cacheErr;

            currentDirectory = GetDirectoryEntries(currentDirCnid);

            if(currentDirectory == null) return ErrorNumber.NoSuchFile;
        }

        return ErrorNumber.NoSuchFile;
    }

    /// <summary>Reads a file fork (data or resource) from the disk using extents information</summary>
    /// <param name="fileEntry">The file entry containing fork information</param>
    /// <param name="forkType">Type of fork to read (Data or Resource)</param>
    /// <param name="logicalSize">Logical size of the fork to read</param>
    /// <param name="firstExtents">The first 3 extent descriptors for this fork</param>
    /// <param name="forkData">The fork data read from disk</param>
    /// <returns>Error number</returns>
    private ErrorNumber ReadFork(FileEntry  fileEntry, ForkType forkType, uint logicalSize, ExtDataRec firstExtents,
                                 out byte[] forkData)
    {
        forkData = null;

        if(logicalSize == 0)
        {
            forkData = [];

            return ErrorNumber.NoError;
        }

        AaruLogging.Debug(MODULE_NAME,
                          "ReadFork: Reading {0} bytes of fork data (file CNID={1}, fork={2})",
                          logicalSize,
                          fileEntry.CNID,
                          forkType);

        // Debug: Log initial extents
        for(var i = 0; i < 3; i++)
        {
            if(firstExtents.xdr == null || i >= firstExtents.xdr.Length || firstExtents.xdr[i].xdrNumABlks == 0)
            {
                AaruLogging.Debug(MODULE_NAME, "Initial extent {0}: empty", i);

                break;
            }

            AaruLogging.Debug(MODULE_NAME,
                              "Initial extent {0}: startBlock={1}, numBlocks={2}",
                              i,
                              firstExtents.xdr[i].xdrStABN,
                              firstExtents.xdr[i].xdrNumABlks);
        }

        // Get all extents for this fork (may include overflow extents from B-Tree)
        ErrorNumber extentErr =
            GetFileExtents(fileEntry.CNID, forkType, firstExtents, out List<ExtDescriptor> allExtents);

        if(extentErr != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Failed to get extents for file {0}: {1}", fileEntry.CNID, extentErr);

            return extentErr;
        }

        if(allExtents == null || allExtents.Count == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "No extents found for file {0}", fileEntry.CNID);

            forkData = [];

            return ErrorNumber.NoError;
        }

        AaruLogging.Debug(MODULE_NAME, "Got {0} total extents for file {1}", allExtents.Count, fileEntry.CNID);

        forkData = new byte[logicalSize];
        uint bytesRead = 0;

        // Read each extent
        foreach(ExtDescriptor extent in allExtents)
        {
            if(extent.xdrNumABlks == 0) break;

            uint extentSize = extent.xdrNumABlks * _mdb.drAlBlkSiz;
            uint toRead     = Math.Min(extentSize, logicalSize - bytesRead);

            // Read the allocation blocks for this extent
            // drAlBlSt is the first allocation block start sector (in 512-byte sectors)
            ulong extentOffsetSector512 = _mdb.drAlBlSt + (ulong)extent.xdrStABN * _mdb.drAlBlkSiz / 512;

            // Convert to device sector address
            HfsOffsetToDeviceSector(extentOffsetSector512, out ulong deviceSector, out uint byteOffset);
            uint sectorCnt = (toRead + byteOffset + _sectorSize - 1) / _sectorSize;

            AaruLogging.Debug(MODULE_NAME,
                              "Reading extent: startBlock={0}, numBlocks={1}, size={2} bytes at sector {3}, sectorCnt={4}",
                              extent.xdrStABN,
                              extent.xdrNumABlks,
                              toRead,
                              deviceSector,
                              sectorCnt);

            ErrorNumber readErr =
                _imagePlugin.ReadSectors(deviceSector, false, sectorCnt, out byte[] extentData, out _);

            if(readErr != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Failed to read extent data: {0}", readErr);

                return readErr;
            }

            if(extentData == null || extentData.Length == 0)
            {
                AaruLogging.Debug(MODULE_NAME, "Read returned null or empty data for extent");

                return ErrorNumber.InvalidArgument;
            }

            // Account for sector offset when copying data
            if(extentData.Length < (int)byteOffset + toRead)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "Insufficient data read: got {0} bytes, expected at least {1}",
                                  extentData.Length,
                                  byteOffset + toRead);

                return ErrorNumber.InvalidArgument;
            }

            uint bytesToCopy = Math.Min(toRead, (uint)(extentData.Length - byteOffset));

            AaruLogging.Debug(MODULE_NAME,
                              "Read {0} bytes from extent, copying {1} bytes from offset {2}",
                              extentData.Length,
                              bytesToCopy,
                              byteOffset);

            Array.Copy(extentData, (int)byteOffset, forkData, bytesRead, bytesToCopy);

            bytesRead += bytesToCopy;

            if(bytesRead >= logicalSize) break;
        }

        if(bytesRead < logicalSize)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Warning: Read only {0} bytes out of {1} bytes for fork of file {2}",
                              bytesRead,
                              logicalSize,
                              fileEntry.CNID);
        }

        return ErrorNumber.NoError;
    }
}