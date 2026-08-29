// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Files-11 On-Disk Structure plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     File operations for the Files-11 On-Disk Structure.
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

namespace Aaru.Filesystems;

public sealed partial class ODS
{
    // VMS file protection bits (each nibble for system/owner/group/world)
    const byte VMS_PROT_DENY_READ  = 0x01;
    const byte VMS_PROT_DENY_WRITE = 0x02;
    const byte VMS_PROT_DENY_EXEC  = 0x04;

    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize path
        string normalizedPath = string.IsNullOrWhiteSpace(path) ? "/" : path;

        if(!normalizedPath.StartsWith("/", StringComparison.Ordinal)) normalizedPath = "/" + normalizedPath;

        // Root directory case
        if(normalizedPath == "/")
        {
            // Read MFD file header
            ErrorNumber errno = ReadFileHeader(MFD_FID, out FileHeader mfdHeader);

            if(errno != ErrorNumber.NoError) return errno;

            stat = BuildFileEntryInfo(mfdHeader, MFD_FID);

            return ErrorNumber.NoError;
        }

        // Look up the file
        ErrorNumber lookupErr = LookupFile(normalizedPath, out CachedFile cachedFile);

        if(lookupErr != ErrorNumber.NoError) return lookupErr;

        // Read the file header with sequence validation
        ErrorNumber readErr = ReadFileHeader(cachedFile.Fid, out FileHeader fileHeader);

        if(readErr != ErrorNumber.NoError) return readErr;

        stat = BuildFileEntryInfo(fileHeader, cachedFile.Fid.num);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber OpenFile(string path, out IFileNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize path
        string normalizedPath = string.IsNullOrWhiteSpace(path) ? "/" : path;

        if(!normalizedPath.StartsWith("/", StringComparison.Ordinal)) normalizedPath = "/" + normalizedPath;

        // Cannot open root directory as a file
        if(normalizedPath == "/") return ErrorNumber.IsDirectory;

        // Look up the file
        ErrorNumber errno = LookupFile(normalizedPath, out CachedFile cachedFile);

        if(errno != ErrorNumber.NoError) return errno;

        // Read the file header with sequence validation
        errno = ReadFileHeader(cachedFile.Fid, out FileHeader fileHeader);

        if(errno != ErrorNumber.NoError) return errno;

        // Cannot open directories as files
        if(fileHeader.filechar.HasFlag(FileCharacteristicFlags.Directory)) return ErrorNumber.IsDirectory;

        // Get the mapping data for file extents
        byte[] mapData = GetMapData(fileHeader);

        if(mapData == null || mapData.Length == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "File has no mapping data");

            return ErrorNumber.InvalidArgument;
        }

        // Calculate file size from FAT
        // File size = (efblk - 1) * blocksize + ffbyte
        uint efblk = fileHeader.recattr.efblk.Value;
        long length;

        if(efblk > 0)
            length = ((long)efblk - 1) * ODS_BLOCK_SIZE + fileHeader.recattr.ffbyte;
        else
            length = 0;

        var fileNode = new OdsFileNode
        {
            Path          = normalizedPath,
            Fid           = cachedFile.Fid,
            FileHeader    = fileHeader,
            MapData       = mapData,
            Length        = length,
            Offset        = 0,
            ExtensionMaps = null
        };

        // Check if file has extension headers (multi-extent file)
        // ext_fid.num != 0 means there's an extension header
        if(fileHeader.ext_fid.num != 0 || fileHeader.ext_fid.nmx != 0)
        {
            // Load extension header chain
            errno = LoadExtensionHeaders(fileNode);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Error loading extension headers: {0}", errno);

                return errno;
            }
        }

        node = fileNode;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not OdsFileNode myNode) return ErrorNumber.InvalidArgument;

        // Clear cached data
        myNode.MapData       = null;
        myNode.ExtensionMaps = null;
        myNode.FileHeader    = default(FileHeader);
        myNode.Offset        = -1;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(buffer is null || buffer.Length < length) return ErrorNumber.InvalidArgument;

        if(node is not OdsFileNode myNode) return ErrorNumber.InvalidArgument;

        if(myNode.Offset < 0) return ErrorNumber.InvalidArgument;

        // Nothing to read
        if(length <= 0) return ErrorNumber.NoError;

        // Clamp read length to remaining file size
        if(myNode.Offset + length > myNode.Length) length = myNode.Length - myNode.Offset;

        if(length <= 0) return ErrorNumber.NoError;

        var bufferPos = 0;

        while(length > 0)
        {
            // Calculate which VBN we need (1-based)
            // VBN = (offset / blocksize) + 1
            uint vbn         = (uint)(myNode.Offset / ODS_BLOCK_SIZE) + 1;
            var  offsetInVbn = (int)(myNode.Offset % ODS_BLOCK_SIZE);

            // Map VBN to LBN using multi-extent support
            ErrorNumber errno = MapVbnToLbnMultiExtent(myNode, vbn, out uint lbn, out uint extent);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Error mapping VBN {0} to LBN: {1}", vbn, errno);

                return errno;
            }

            // Calculate how many consecutive blocks we can read from this extent
            // Don't read more blocks than needed
            var  blocksNeeded = (uint)((length + offsetInVbn + ODS_BLOCK_SIZE - 1) / ODS_BLOCK_SIZE);
            uint blocksToRead = Math.Min(extent, blocksNeeded);

            // Read the ODS block(s)
            errno = ReadOdsBlock(_image, _partition, lbn, out byte[] blockData);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Error reading block at LBN {0}: {1}", lbn, errno);

                return errno;
            }

            // Calculate how much data to copy from this block
            int bytesAvailable = blockData.Length - offsetInVbn;
            var bytesToCopy    = (int)Math.Min(bytesAvailable, length);

            // Copy data to buffer
            Array.Copy(blockData, offsetInVbn, buffer, bufferPos, bytesToCopy);

            bufferPos     += bytesToCopy;
            myNode.Offset += bytesToCopy;
            length        -= bytesToCopy;
            read          += bytesToCopy;
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadLink(string path, out string dest)
    {
        dest = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize path
        string normalizedPath = string.IsNullOrWhiteSpace(path) ? "/" : path;

        if(!normalizedPath.StartsWith("/", StringComparison.Ordinal)) normalizedPath = "/" + normalizedPath;

        // Root directory cannot be a symlink
        if(normalizedPath == "/") return ErrorNumber.InvalidArgument;

        // Look up the file
        ErrorNumber errno = LookupFile(normalizedPath, out CachedFile cachedFile);

        if(errno != ErrorNumber.NoError) return errno;

        // Read the file header with sequence validation
        errno = ReadFileHeader(cachedFile.Fid, out FileHeader fileHeader);

        if(errno != ErrorNumber.NoError) return errno;

        // Verify it's a symbolic link (special file organization with symlink type)
        if(fileHeader.recattr.Organization != FileOrganization.Special) return ErrorNumber.InvalidArgument;

        var specialType = (SpecialFileType)((byte)fileHeader.recattr.rattrib & 0x0F);

        if(specialType != SpecialFileType.SymLink && specialType != SpecialFileType.SymbolicLink)
            return ErrorNumber.InvalidArgument;

        // Get the mapping data for file extents
        byte[] mapData = GetMapData(fileHeader);

        if(mapData == null || mapData.Length == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "Symlink has no mapping data");

            return ErrorNumber.InvalidArgument;
        }

        // Calculate file size (symlink target length)
        uint efblk = fileHeader.recattr.efblk.Value;
        long length;

        if(efblk > 0)
            length = ((long)efblk - 1) * ODS_BLOCK_SIZE + fileHeader.recattr.ffbyte;
        else
            length = 0;

        if(length <= 0)
        {
            dest = string.Empty;

            return ErrorNumber.NoError;
        }

        // Read the symlink target (file content)
        var linkData = new byte[length];
        var offset   = 0L;
        var read     = 0;

        while(read < length)
        {
            // Calculate which VBN we need (1-based)
            uint vbn         = (uint)(offset / ODS_BLOCK_SIZE) + 1;
            var  offsetInVbn = (int)(offset % ODS_BLOCK_SIZE);

            // Map VBN to LBN
            errno = MapVbnToLbn(mapData, fileHeader.map_inuse, vbn, out uint lbn, out _);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Error mapping symlink VBN {0} to LBN: {1}", vbn, errno);

                return errno;
            }

            // Read the ODS block
            errno = ReadOdsBlock(_image, _partition, lbn, out byte[] blockData);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Error reading symlink block at LBN {0}: {1}", lbn, errno);

                return errno;
            }

            // Calculate how much data to copy from this block
            int bytesAvailable = blockData.Length - offsetInVbn;
            var bytesToCopy    = (int)Math.Min(bytesAvailable, length - read);

            // Copy data to buffer
            Array.Copy(blockData, offsetInVbn, linkData, read, bytesToCopy);

            offset += bytesToCopy;
            read   += bytesToCopy;
        }

        // Convert to string using the filesystem encoding
        dest = _encoding.GetString(linkData).TrimEnd('\0');

        return ErrorNumber.NoError;
    }

    /// <summary>Loads extension file headers for a multi-extent file.</summary>
    /// <param name="fileNode">File node to populate with extension maps.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber LoadExtensionHeaders(OdsFileNode fileNode)
    {
        fileNode.ExtensionMaps = [];

        // Calculate the VBN sum from the primary header's map
        uint vbnSum = GetMapVbnCount(fileNode.MapData, fileNode.FileHeader.map_inuse);

        // Start with the extension file ID from the primary header
        FileId currentExtFid = fileNode.FileHeader.ext_fid;

        // Follow the chain of extension headers
        while(currentExtFid.num != 0 || currentExtFid.nmx != 0)
        {
            // Read the extension file header.
            // Use the FileId overload so the file number extension (nmx) is honored and the
            // header is cross-checked against the requested file ID and sequence number.
            ErrorNumber errno = ReadFileHeader(currentExtFid, out FileHeader extHeader);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "Error reading extension file header ({0},{1},{2}): {3}",
                                  currentExtFid.num + (currentExtFid.nmx << 16),
                                  currentExtFid.seq,
                                  currentExtFid.rvn,
                                  errno);

                return errno;
            }

            // Get mapping data from extension header
            byte[] extMapData = GetMapData(extHeader);

            if(extMapData == null || extMapData.Length == 0)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "Extension header {0} has no mapping data",
                                  currentExtFid.num + (currentExtFid.nmx << 16));

                break;
            }

            // Add to extension maps list
            fileNode.ExtensionMaps.Add(new ExtensionMapInfo
            {
                ExtFid   = currentExtFid,
                MapData  = extMapData,
                MapInUse = extHeader.map_inuse,
                VbnSum   = vbnSum
            });

            // Update VBN sum for next extension
            vbnSum += GetMapVbnCount(extMapData, extHeader.map_inuse);

            // Move to next extension header in chain
            currentExtFid = extHeader.ext_fid;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Maps a VBN to LBN using multi-extent support.</summary>
    /// <param name="fileNode">File node with primary and extension map data.</param>
    /// <param name="vbn">Virtual block number (1-based).</param>
    /// <param name="lbn">Output logical block number.</param>
    /// <param name="extent">Output extent size.</param>
    /// <returns>Error number indicating success or failure.</returns>
    static ErrorNumber MapVbnToLbnMultiExtent(OdsFileNode fileNode, uint vbn, out uint lbn, out uint extent)
    {
        lbn    = 0;
        extent = 0;

        // First try the primary file header's map
        ErrorNumber errno = MapVbnToLbnWithSum(fileNode.MapData,
                                               fileNode.FileHeader.map_inuse,
                                               vbn,
                                               0,
                                               out lbn,
                                               out extent,
                                               out uint endSum);

        if(errno == ErrorNumber.NoError) return ErrorNumber.NoError;

        // If no extension maps, VBN is not found
        if(fileNode.ExtensionMaps == null || fileNode.ExtensionMaps.Count == 0) return ErrorNumber.InvalidArgument;

        // Search through extension maps
        foreach(ExtensionMapInfo extMap in fileNode.ExtensionMaps)
        {
            errno = MapVbnToLbnWithSum(extMap.MapData, extMap.MapInUse, vbn, extMap.VbnSum, out lbn, out extent, out _);

            if(errno == ErrorNumber.NoError) return ErrorNumber.NoError;
        }

        // VBN not found in any map
        return ErrorNumber.InvalidArgument;
    }

    /// <summary>Looks up a file by path and returns its cached entry.</summary>
    /// <param name="path">Normalized path starting with /.</param>
    /// <param name="cachedFile">Output cached file entry.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber LookupFile(string path, out CachedFile cachedFile)
    {
        cachedFile = null;

        string   cutPath = path[1..]; // Remove leading '/'
        string[] pieces  = cutPath.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);

        if(pieces.Length == 0) return ErrorNumber.NoSuchFile;

        // Start from root directory
        Dictionary<string, CachedFile> currentDirectory = _rootDirectoryCache;

        for(var p = 0; p < pieces.Length; p++)
        {
            string component = pieces[p].ToUpperInvariant();

            // Handle version number based on namespace
            int versionPos = component.IndexOf(';');

            if(_namespace == NAMESPACE_NOVERSIONS)
            {
                // noversions namespace - always strip version and look up without it
                if(versionPos >= 0) component = component[..versionPos];
            }

            // default namespace - if version specified, look up with version; otherwise without
            // The cache stores both "FILE" and "FILE;1" entries
            // If user specifies "FILE;1", look for "FILE;1"
            // If user specifies "FILE", look for "FILE" (which gives latest version)
            // Look for the component in current directory
            if(!currentDirectory.TryGetValue(component, out CachedFile found)) return ErrorNumber.NoSuchFile;

            // If this is the last component, return it
            if(p == pieces.Length - 1)
            {
                cachedFile = found;

                return ErrorNumber.NoError;
            }

            // Not the last component - must be a directory
            // For directories, we need to strip version for lookup
            string dirComponent  = component;
            int    dirVersionPos = dirComponent.IndexOf(';');

            if(dirVersionPos >= 0) dirComponent = dirComponent[..dirVersionPos];

            if(!currentDirectory.TryGetValue(dirComponent, out found)) return ErrorNumber.NoSuchFile;

            ErrorNumber errno = ReadFileHeader(found.Fid.num, out FileHeader fileHeader);

            if(errno != ErrorNumber.NoError) return errno;

            if(!fileHeader.filechar.HasFlag(FileCharacteristicFlags.Directory)) return ErrorNumber.NotDirectory;

            // Read directory entries, skipping self-referential entry
            errno = ReadDirectoryEntries(fileHeader, out Dictionary<string, CachedFile> dirEntries, found.Fid.num);

            if(errno != ErrorNumber.NoError) return errno;

            currentDirectory = dirEntries;
        }

        return ErrorNumber.NoSuchFile;
    }

    /// <summary>Builds a FileEntryInfo from a file header.</summary>
    /// <param name="header">File header.</param>
    /// <param name="fileNum">File number (inode).</param>
    /// <returns>FileEntryInfo structure.</returns>
    FileEntryInfo BuildFileEntryInfo(in FileHeader header, ushort fileNum)
    {
        var info = new FileEntryInfo
        {
            Inode     = fileNum,
            Links     = header.linkcount > 0 ? (ulong)header.linkcount : 1,
            BlockSize = ODS_BLOCK_SIZE,
            UID       = header.fileowner.member,
            GID       = header.fileowner.group
        };

        // Calculate file size
        // File size = (efblk - 1) * blocksize + ffbyte
        // But efblk is stored as high:low words
        uint efblk = header.recattr.efblk.Value;

        if(efblk > 0)
            info.Length = ((long)efblk - 1) * ODS_BLOCK_SIZE + header.recattr.ffbyte;
        else
            info.Length = 0;

        // Calculate blocks allocated
        info.Blocks = header.recattr.hiblk.Value;

        // Get timestamps from ident area
        ReadFileIdent(header,
                      out ulong credate,
                      out ulong revdate,
                      out _,
                      out ulong bakdate,
                      out ulong accdate,
                      out ulong attdate);

        if(credate > 0) info.CreationTime = DateHandlers.VmsToDateTime(credate);

        if(revdate > 0) info.LastWriteTime = DateHandlers.VmsToDateTime(revdate);

        if(bakdate > 0) info.BackupTime = DateHandlers.VmsToDateTime(bakdate);

        if(accdate > 0) info.AccessTime = DateHandlers.VmsToDateTime(accdate);

        if(attdate > 0) info.StatusChangeTime = DateHandlers.VmsToDateTime(attdate);

        // Set file type and attributes
        info.Attributes = CommonTypes.Structs.FileAttributes.None;

        // Check for directory
        if(header.filechar.HasFlag(FileCharacteristicFlags.Directory))
            info.Attributes |= CommonTypes.Structs.FileAttributes.Directory;
        else
        {
            // Check for special file types based on file organization
            FileOrganization org = header.recattr.Organization;

            if(org == FileOrganization.Special)
            {
                // Check rattrib for special file type
                var specialType = (SpecialFileType)((byte)header.recattr.rattrib & 0x0F);

                switch(specialType)
                {
                    case SpecialFileType.Fifo:
                        info.Attributes |= CommonTypes.Structs.FileAttributes.FIFO;

                        break;
                    case SpecialFileType.CharSpecial:
                        info.Attributes |= CommonTypes.Structs.FileAttributes.CharDevice;

                        break;
                    case SpecialFileType.BlockSpecial:
                        info.Attributes |= CommonTypes.Structs.FileAttributes.BlockDevice;

                        break;
                    case SpecialFileType.SymLink:
                    case SpecialFileType.SymbolicLink:
                        info.Attributes |= CommonTypes.Structs.FileAttributes.Symlink;

                        break;
                    default:
                        info.Attributes |= CommonTypes.Structs.FileAttributes.File;

                        break;
                }
            }
            else
                info.Attributes |= CommonTypes.Structs.FileAttributes.File;
        }

        // Map file characteristics to attributes
        if(header.filechar.HasFlag(FileCharacteristicFlags.Locked))
            info.Attributes |= CommonTypes.Structs.FileAttributes.Immutable;

        if(header.filechar.HasFlag(FileCharacteristicFlags.Contig) ||
           header.filechar.HasFlag(FileCharacteristicFlags.WasContig))
            info.Attributes |= CommonTypes.Structs.FileAttributes.Extents;

        if(header.filechar.HasFlag(FileCharacteristicFlags.NoBackup))
            info.Attributes |= CommonTypes.Structs.FileAttributes.NoDump;

        if(header.filechar.HasFlag(FileCharacteristicFlags.Spool))
            info.Attributes |= CommonTypes.Structs.FileAttributes.Temporary;

        if(header.filechar.HasFlag(FileCharacteristicFlags.MarkDel))
            info.Attributes |= CommonTypes.Structs.FileAttributes.Deleted;

        if(header.filechar.HasFlag(FileCharacteristicFlags.Erase))
            info.Attributes |= CommonTypes.Structs.FileAttributes.Secured;

        if(header.filechar.HasFlag(FileCharacteristicFlags.Shelved))
            info.Attributes |= CommonTypes.Structs.FileAttributes.Offline;

        // Convert VMS protection to POSIX mode
        // VMS protection is 16 bits: system(4) | owner(4) | group(4) | world(4)
        // Each nibble has bits: delete | execute | write | read (deny bits)
        info.Mode = ConvertVmsProtectionToMode(header.fileprot,
                                               header.filechar.HasFlag(FileCharacteristicFlags.Directory));

        return info;
    }

    /// <summary>Reads the file ident area from a file header to extract timestamps.</summary>
    /// <param name="header">File header.</param>
    /// <param name="credate">Creation date.</param>
    /// <param name="revdate">Revision date.</param>
    /// <param name="expdate">Expiration date.</param>
    /// <param name="bakdate">Backup date.</param>
    /// <param name="accdate">Access date (ODS-5 only).</param>
    /// <param name="attdate">Attribute change date (ODS-5 only).</param>
    static void ReadFileIdent(in  FileHeader header,  out ulong credate, out ulong revdate, out ulong expdate,
                              out ulong      bakdate, out ulong accdate, out ulong attdate)
    {
        credate = revdate = expdate = bakdate = accdate = attdate = 0;

        // Ident area starts at idoffset words from start of header
        int identOffset = header.idoffset * 2;

        // The ident area is within the reserved area of the file header
        // Reserved area starts at offset 0x50 (80 bytes) and is 430 bytes
        const int reservedStart    = 0x50;
        int       identOffsetInRes = identOffset - reservedStart;

        if(header.reserved == null || identOffsetInRes < 0) return;

        // Determine structure level from header
        var strucLevel = (byte)(header.struclev >> 8 & 0xFF);

        if(strucLevel == 5 && identOffsetInRes + 52 <= header.reserved.Length)
        {
            // ODS-5 ident area
            // Skip control byte (1) and namelen (1), revision (2) = offset 4 for credate
            credate = BitConverter.ToUInt64(header.reserved, identOffsetInRes + 4);
            revdate = BitConverter.ToUInt64(header.reserved, identOffsetInRes + 12);
            expdate = BitConverter.ToUInt64(header.reserved, identOffsetInRes + 20);
            bakdate = BitConverter.ToUInt64(header.reserved, identOffsetInRes + 28);
            accdate = BitConverter.ToUInt64(header.reserved, identOffsetInRes + 36);
            attdate = BitConverter.ToUInt64(header.reserved, identOffsetInRes + 44);
        }
        else if(identOffsetInRes + 54 <= header.reserved.Length)
        {
            // ODS-2 ident area
            // Skip filename (20 bytes), revision (2) = offset 22 for credate
            credate = BitConverter.ToUInt64(header.reserved, identOffsetInRes + 22);
            revdate = BitConverter.ToUInt64(header.reserved, identOffsetInRes + 30);
            expdate = BitConverter.ToUInt64(header.reserved, identOffsetInRes + 38);
            bakdate = BitConverter.ToUInt64(header.reserved, identOffsetInRes + 46);
        }
    }

    /// <summary>Converts VMS file protection to POSIX mode bits.</summary>
    /// <param name="fileprot">VMS protection word.</param>
    /// <param name="isDirectory">Whether this is a directory.</param>
    /// <returns>POSIX mode bits.</returns>
    static uint ConvertVmsProtectionToMode(ushort fileprot, bool isDirectory)
    {
        // VMS protection is stored as: system(4) | owner(4) | group(4) | world(4)
        // Each 4-bit nibble has deny bits: delete(3) | execute(2) | write(1) | read(0)
        // If a bit is SET, the permission is DENIED
        // Note: VMS system permissions don't map directly to POSIX, so we skip them
        var owner = (byte)(fileprot >> 8 & 0x0F);
        var group = (byte)(fileprot >> 4 & 0x0F);
        var world = (byte)(fileprot      & 0x0F);

        uint mode = 0;

        // File type bits
        if(isDirectory)
            mode |= 0x4000; // S_IFDIR
        else
            mode |= 0x8000; // S_IFREG

        // Owner permissions (bits 8-6)
        if((owner & VMS_PROT_DENY_READ)  == 0) mode |= 0x0100; // S_IRUSR
        if((owner & VMS_PROT_DENY_WRITE) == 0) mode |= 0x0080; // S_IWUSR
        if((owner & VMS_PROT_DENY_EXEC)  == 0) mode |= 0x0040; // S_IXUSR

        // Group permissions (bits 5-3)
        if((group & VMS_PROT_DENY_READ)  == 0) mode |= 0x0020; // S_IRGRP
        if((group & VMS_PROT_DENY_WRITE) == 0) mode |= 0x0010; // S_IWGRP
        if((group & VMS_PROT_DENY_EXEC)  == 0) mode |= 0x0008; // S_IXGRP

        // World/other permissions (bits 2-0)
        if((world & VMS_PROT_DENY_READ)  == 0) mode |= 0x0004; // S_IROTH
        if((world & VMS_PROT_DENY_WRITE) == 0) mode |= 0x0002; // S_IWOTH
        if((world & VMS_PROT_DENY_EXEC)  == 0) mode |= 0x0001; // S_IXOTH

        return mode;
    }

    /// <summary>Calculates the LBN of a file header inside the index file.</summary>
    /// <remarks>
    ///     This assumes the index file bitmap and the header area are one contiguous run, which holds for a volume
    ///     whose index file has not been extended.
    /// </remarks>
    /// <param name="ibmaplbn">LBN of the index file bitmap, from the home block.</param>
    /// <param name="ibmapsize">Size in blocks of the index file bitmap, from the home block.</param>
    /// <param name="fileNum">File number, including the file number extension (nmx) in its high bits.</param>
    /// <returns>LBN of the file header.</returns>
    internal static uint FileHeaderLbn(uint ibmaplbn, ushort ibmapsize, uint fileNum) =>
        ibmaplbn + ibmapsize + fileNum - 1;

    /// <summary>Reads a file header by file number (without sequence validation).</summary>
    /// <param name="fileNum">File number (1-based).</param>
    /// <param name="header">Output file header.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber ReadFileHeader(ushort fileNum, out FileHeader header)
    {
        header = default(FileHeader);

        // File number 0 is not a valid file, and would resolve to the last index bitmap block
        if(fileNum == 0) return ErrorNumber.InvalidArgument;

        uint headerLbn = FileHeaderLbn(_homeBlock.ibmaplbn, _homeBlock.ibmapsize, fileNum);

        ErrorNumber errno = ReadOdsBlock(_image, _partition, headerLbn, out byte[] headerSector);

        if(errno != ErrorNumber.NoError) return errno;

        header = Marshal.ByteArrayToStructureLittleEndian<FileHeader>(headerSector);

        // Validate file header checksum
        ushort calculatedChecksum = 0;

        for(var i = 0; i < 0x1FE; i += 2) calculatedChecksum += BitConverter.ToUInt16(headerSector, i);

        if(calculatedChecksum != header.checksum)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "File header checksum mismatch for file {0}: expected {1:X4}, calculated {2:X4}",
                              fileNum,
                              header.checksum,
                              calculatedChecksum);

            return ErrorNumber.InvalidArgument;
        }

        // Validate structure level
        var headerStrucLevel = (byte)(header.struclev >> 8 & 0xFF);

        if(headerStrucLevel != 2 && headerStrucLevel != 5)
        {
            AaruLogging.Debug(MODULE_NAME, "Invalid file header structure level: {0}", headerStrucLevel);

            return ErrorNumber.InvalidArgument;
        }

        // Validate offsets are in correct order
        if(header.idoffset > header.mpoffset || header.mpoffset > header.acoffset || header.acoffset > header.rsoffset)
        {
            AaruLogging.Debug(MODULE_NAME, "Invalid file header offsets");

            return ErrorNumber.InvalidArgument;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Reads a file header by file ID with sequence validation.</summary>
    /// <param name="fid">File ID including number, sequence, rvn, and nmx.</param>
    /// <param name="header">Output file header.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber ReadFileHeader(FileId fid, out FileHeader header)
    {
        header = default(FileHeader);

        // File number includes nmx in high bits for large volumes
        var fileNum = (uint)(fid.num + (fid.nmx << 16));

        // File number 0 is not a valid file, and would resolve to the last index bitmap block
        if(fileNum == 0) return ErrorNumber.InvalidArgument;

        uint headerLbn = FileHeaderLbn(_homeBlock.ibmaplbn, _homeBlock.ibmapsize, fileNum);

        ErrorNumber errno = ReadOdsBlock(_image, _partition, headerLbn, out byte[] headerSector);

        if(errno != ErrorNumber.NoError) return errno;

        header = Marshal.ByteArrayToStructureLittleEndian<FileHeader>(headerSector);

        // Validate file header checksum
        ushort calculatedChecksum = 0;

        for(var i = 0; i < 0x1FE; i += 2) calculatedChecksum += BitConverter.ToUInt16(headerSector, i);

        if(calculatedChecksum != header.checksum)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "File header checksum mismatch for file {0}: expected {1:X4}, calculated {2:X4}",
                              fileNum,
                              header.checksum,
                              calculatedChecksum);

            return ErrorNumber.InvalidArgument;
        }

        // Validate structure level
        var headerStrucLevel = (byte)(header.struclev >> 8 & 0xFF);

        if(headerStrucLevel != 2 && headerStrucLevel != 5)
        {
            AaruLogging.Debug(MODULE_NAME, "Invalid file header structure level: {0}", headerStrucLevel);

            return ErrorNumber.InvalidArgument;
        }

        // Validate offsets are in correct order
        if(header.idoffset > header.mpoffset || header.mpoffset > header.acoffset || header.acoffset > header.rsoffset)
        {
            AaruLogging.Debug(MODULE_NAME, "Invalid file header offsets");

            return ErrorNumber.InvalidArgument;
        }

        // Validate file ID matches (prevents reading stale/reused headers)
        if(header.fid.num != fid.num || header.fid.nmx != fid.nmx)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "File ID mismatch: requested ({0},{1}), header has ({2},{3})",
                              fid.num,
                              fid.nmx,
                              header.fid.num,
                              header.fid.nmx);

            return ErrorNumber.InvalidArgument;
        }

        // Validate sequence number if provided (non-zero)
        if(fid.seq != 0 && header.fid.seq != fid.seq)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "File ID sequence mismatch for file {0}: requested {1}, header has {2}",
                              fileNum,
                              fid.seq,
                              header.fid.seq);

            return ErrorNumber.InvalidArgument;
        }

        return ErrorNumber.NoError;
    }
}