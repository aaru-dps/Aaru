// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Xattr.cs
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
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

// Information from Apple ProDOS 8 Technical Reference
/// <inheritdoc />
public sealed partial class ProDOSPlugin
{
    /// <inheritdoc />
    public ErrorNumber ListXAttr(string path, out List<string> xattrs)
    {
        xattrs = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Root directory has no xattrs
        if(string.IsNullOrEmpty(path) || path == "/" || path == ".")
        {
            xattrs = new List<string>();

            return ErrorNumber.NoError;
        }

        // Get the entry for this path
        ErrorNumber errno = GetEntryForPath(path, out CachedEntry entry);

        if(errno != ErrorNumber.NoError) return errno;

        xattrs = new List<string>();

        // Directories don't have extended attributes in ProDOS
        if(entry.IsDirectory) return ErrorNumber.NoError;

        // All files have type and aux_type
        xattrs.Add(Xattrs.XATTR_APPLE_PRODOS_TYPE);
        xattrs.Add(Xattrs.XATTR_APPLE_PRODOS_AUX_TYPE);

        // Extended files have resource fork
        if(entry.StorageType == EXTENDED_FILE_TYPE)
        {
            // Read extended key block to check if resource fork has data
            errno = ReadBlock(entry.KeyBlock, out byte[] extBlock);

            if(errno == ErrorNumber.NoError)
            {
                ExtendedKeyBlock extKeyBlock = Marshal.ByteArrayToStructureLittleEndian<ExtendedKeyBlock>(extBlock);

                // Resource fork EOF
                var resForkEof = (uint)(extKeyBlock.resource_fork.eof[0]      |
                                        extKeyBlock.resource_fork.eof[1] << 8 |
                                        extKeyBlock.resource_fork.eof[2] << 16);

                if(resForkEof > 0) xattrs.Add(Xattrs.XATTR_APPLE_RESOURCE_FORK);

                // Check for FinderInfo (FInfo + FXInfo) in reserved1 area (32 bytes)
                var hasFinderInfo = false;

                for(var i = 0; i < 32 && i < extKeyBlock.reserved1.Length; i++)
                {
                    if(extKeyBlock.reserved1[i] != 0)
                    {
                        hasFinderInfo = true;

                        break;
                    }
                }

                if(hasFinderInfo) xattrs.Add(Xattrs.XATTR_APPLE_FINDER_INFO);
            }
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetXattr(string path, string xattr, ref byte[] buf)
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        // Root directory has no xattrs
        if(string.IsNullOrEmpty(path) || path == "/" || path == ".") return ErrorNumber.NoSuchExtendedAttribute;

        // Get the entry for this path
        ErrorNumber errno = GetEntryForPath(path, out CachedEntry entry);

        if(errno != ErrorNumber.NoError) return errno;

        // Directories don't have extended attributes in ProDOS
        if(entry.IsDirectory) return ErrorNumber.NoSuchExtendedAttribute;

        switch(xattr)
        {
            case Xattrs.XATTR_APPLE_PRODOS_TYPE:
                buf    = new byte[1];
                buf[0] = entry.FileType;

                return ErrorNumber.NoError;

            case Xattrs.XATTR_APPLE_PRODOS_AUX_TYPE:
                buf = BitConverter.GetBytes(entry.AuxType);

                return ErrorNumber.NoError;

            case Xattrs.XATTR_APPLE_RESOURCE_FORK:
                return ReadResourceFork(entry, ref buf);

            case Xattrs.XATTR_APPLE_FINDER_INFO:
                return ReadFinderInfo(entry, ref buf);

            default:
                return ErrorNumber.NoSuchExtendedAttribute;
        }
    }

    /// <summary>Reads the resource fork of an extended file</summary>
    /// <param name="entry">The file entry</param>
    /// <param name="buf">Buffer to store the resource fork data</param>
    /// <returns>Error number</returns>
    ErrorNumber ReadResourceFork(CachedEntry entry, ref byte[] buf)
    {
        // Only extended files have resource forks
        if(entry.StorageType != EXTENDED_FILE_TYPE) return ErrorNumber.NoSuchExtendedAttribute;

        // Read the extended key block
        ErrorNumber errno = ReadBlock(entry.KeyBlock, out byte[] extBlock);

        if(errno != ErrorNumber.NoError) return errno;

        ExtendedKeyBlock extKeyBlock = Marshal.ByteArrayToStructureLittleEndian<ExtendedKeyBlock>(extBlock);

        // Get resource fork size
        var resForkEof = (uint)(extKeyBlock.resource_fork.eof[0]      |
                                extKeyBlock.resource_fork.eof[1] << 8 |
                                extKeyBlock.resource_fork.eof[2] << 16);

        if(resForkEof == 0) return ErrorNumber.NoSuchExtendedAttribute;

        // Read the resource fork data based on its storage type
        var storageType = extKeyBlock.resource_fork.storage_type;

        return ReadForkData(storageType, extKeyBlock.resource_fork.key_block, resForkEof, ref buf);
    }

    /// <summary>Reads the FinderInfo from an extended file's reserved area</summary>
    /// <param name="entry">The file entry</param>
    /// <param name="buf">Buffer to store the FinderInfo data (32 bytes: FInfo 16 bytes + FXInfo 16 bytes)</param>
    /// <returns>Error number</returns>
    ErrorNumber ReadFinderInfo(CachedEntry entry, ref byte[] buf)
    {
        // Only extended files may have FinderInfo
        if(entry.StorageType != EXTENDED_FILE_TYPE) return ErrorNumber.NoSuchExtendedAttribute;

        // Read the extended key block
        ErrorNumber errno = ReadBlock(entry.KeyBlock, out byte[] extBlock);

        if(errno != ErrorNumber.NoError) return errno;

        ExtendedKeyBlock extKeyBlock = Marshal.ByteArrayToStructureLittleEndian<ExtendedKeyBlock>(extBlock);

        // FinderInfo (FInfo + FXInfo) is stored at the beginning of reserved1 area
        // FInfo: 16 bytes at offset 0 of reserved1
        // FXInfo: 16 bytes at offset 16 of reserved1
        // Check if there's any non-zero data in either FInfo or FXInfo
        var hasFinderInfo = false;

        for(var i = 0; i < 32 && i < extKeyBlock.reserved1.Length; i++)
        {
            if(extKeyBlock.reserved1[i] != 0)
            {
                hasFinderInfo = true;

                break;
            }
        }

        if(!hasFinderInfo) return ErrorNumber.NoSuchExtendedAttribute;

        // Copy the 32 bytes of FinderInfo (FInfo + FXInfo)
        buf = new byte[32];
        Array.Copy(extKeyBlock.reserved1, 0, buf, 0, Math.Min(32, extKeyBlock.reserved1.Length));

        return ErrorNumber.NoError;
    }

    /// <summary>Reads fork data based on storage type</summary>
    /// <param name="storageType">Storage type (1=seedling, 2=sapling, 3=tree)</param>
    /// <param name="keyBlock">Key block pointer</param>
    /// <param name="length">Length of data to read</param>
    /// <param name="buf">Buffer to store the data</param>
    /// <returns>Error number</returns>
    ErrorNumber ReadForkData(byte storageType, ushort keyBlock, uint length, ref byte[] buf)
    {
        buf = new byte[length];
        var bytesRead = 0;

        switch(storageType)
        {
            case SEEDLING_FILE_TYPE:
                // Data is in a single block
                ErrorNumber errno = ReadBlock(keyBlock, out byte[] seedlingBlock);

                if(errno != ErrorNumber.NoError) return errno;

                Array.Copy(seedlingBlock, 0, buf, 0, (int)Math.Min(length, 512));

                return ErrorNumber.NoError;

            case SAPLING_FILE_TYPE:
                // Key block is an index block pointing to data blocks
                errno = ReadBlock(keyBlock, out byte[] indexBlock);

                if(errno != ErrorNumber.NoError) return errno;

                IndirectBlock index = Marshal.ByteArrayToStructureLittleEndian<IndirectBlock>(indexBlock);

                for(var i = 0; i < 256 && bytesRead < length; i++)
                {
                    var blockPtr = (ushort)(index.lsbyte[i] | index.msbyte[i] << 8);

                    if(blockPtr == 0)
                    {
                        // Sparse block - fill with zeros
                        var toFill = (int)Math.Min(512, length - bytesRead);
                        Array.Clear(buf, bytesRead, toFill);
                        bytesRead += toFill;

                        continue;
                    }

                    errno = ReadBlock(blockPtr, out byte[] dataBlock);

                    if(errno != ErrorNumber.NoError) return errno;

                    var toCopy = (int)Math.Min(512, length - bytesRead);
                    Array.Copy(dataBlock, 0, buf, bytesRead, toCopy);
                    bytesRead += toCopy;
                }

                return ErrorNumber.NoError;

            case TREE_FILE_TYPE:
                // Key block is a master index block pointing to index blocks
                errno = ReadBlock(keyBlock, out byte[] masterIndexBlock);

                if(errno != ErrorNumber.NoError) return errno;

                IndirectBlock masterIndex = Marshal.ByteArrayToStructureLittleEndian<IndirectBlock>(masterIndexBlock);

                for(var i = 0; i < 256 && bytesRead < length; i++)
                {
                    var indexBlockPtr = (ushort)(masterIndex.lsbyte[i] | masterIndex.msbyte[i] << 8);

                    if(indexBlockPtr == 0)
                    {
                        // Sparse index block - skip 256 * 512 bytes
                        var toFill = (int)Math.Min(256 * 512, length - bytesRead);
                        Array.Clear(buf, bytesRead, toFill);
                        bytesRead += toFill;

                        continue;
                    }

                    errno = ReadBlock(indexBlockPtr, out byte[] subIndexBlock);

                    if(errno != ErrorNumber.NoError) return errno;

                    IndirectBlock subIndex = Marshal.ByteArrayToStructureLittleEndian<IndirectBlock>(subIndexBlock);

                    for(var j = 0; j < 256 && bytesRead < length; j++)
                    {
                        var blockPtr = (ushort)(subIndex.lsbyte[j] | subIndex.msbyte[j] << 8);

                        if(blockPtr == 0)
                        {
                            // Sparse block - fill with zeros
                            var toFill = (int)Math.Min(512, length - bytesRead);
                            Array.Clear(buf, bytesRead, toFill);
                            bytesRead += toFill;

                            continue;
                        }

                        errno = ReadBlock(blockPtr, out byte[] dataBlock);

                        if(errno != ErrorNumber.NoError) return errno;

                        var toCopy = (int)Math.Min(512, length - bytesRead);
                        Array.Copy(dataBlock, 0, buf, bytesRead, toCopy);
                        bytesRead += toCopy;
                    }
                }

                return ErrorNumber.NoError;

            default:
                return ErrorNumber.InvalidArgument;
        }
    }
}