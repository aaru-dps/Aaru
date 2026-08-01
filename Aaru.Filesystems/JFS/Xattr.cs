// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Xattr.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : IBM JFS filesystem plugin
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
using Aaru.Logging;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of IBM's Journaled File System</summary>
public sealed partial class JFS
{
    /// <inheritdoc />
    public ErrorNumber ListXAttr(string path, out List<string> xattrs)
    {
        xattrs = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        AaruLogging.Debug(MODULE_NAME, "ListXAttr: path='{0}'", path);

        ErrorNumber errno = GetInodeForPath(path, out Inode inode);

        if(errno != ErrorNumber.NoError) return errno;

        errno = ReadEaList(inode, out byte[] eaData);

        if(errno != ErrorNumber.NoError) return errno;

        if(eaData is null || eaData.Length < 4)
        {
            xattrs = [];

            return ErrorNumber.NoError;
        }

        var names = new List<string>();

        errno = ParseEaList(eaData, (name, _) => names.Add(name));

        xattrs = names;

        return errno;
    }

    /// <inheritdoc />
    public ErrorNumber GetXattr(string path, string xattr, ref byte[] buf)
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        AaruLogging.Debug(MODULE_NAME, "GetXattr: path='{0}', xattr='{1}'", path, xattr);

        ErrorNumber errno = GetInodeForPath(path, out Inode inode);

        if(errno != ErrorNumber.NoError) return errno;

        errno = ReadEaList(inode, out byte[] eaData);

        if(errno != ErrorNumber.NoError) return errno;

        if(eaData is null || eaData.Length < 4) return ErrorNumber.NoSuchExtendedAttribute;

        byte[]      found      = null;
        ErrorNumber parseErrno = ErrorNumber.NoSuchExtendedAttribute;

        ParseEaList(eaData,
                    (name, value) =>
                    {
                        if(!string.Equals(name, xattr, StringComparison.Ordinal)) return;

                        found      = value;
                        parseErrno = ErrorNumber.NoError;
                    });

        if(parseErrno != ErrorNumber.NoError) return parseErrno;

        buf = found;

        return ErrorNumber.NoError;
    }

    /// <summary>Resolves a path to its inode</summary>
    /// <param name="path">The filesystem path</param>
    /// <param name="inode">The resolved inode</param>
    /// <returns>Error code indicating success or failure</returns>
    ErrorNumber GetInodeForPath(string path, out Inode inode)
    {
        inode = default(Inode);

        string normalizedPath = path ?? "/";

        if(normalizedPath is "" or ".") normalizedPath = "/";

        // Root directory
        if(normalizedPath == "/") return ReadFilesetInode(ROOT_I, out inode);

        string pathWithoutLeadingSlash = normalizedPath.StartsWith("/", StringComparison.Ordinal)
                                             ? normalizedPath[1..]
                                             : normalizedPath;

        string[] pathComponents = pathWithoutLeadingSlash.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if(pathComponents.Length == 0) return ErrorNumber.InvalidArgument;

        Dictionary<string, uint> currentEntries = _rootDirectoryCache;

        // Traverse all but the last component (they must be directories)
        for(var i = 0; i < pathComponents.Length - 1; i++)
        {
            string component = pathComponents[i];

            if(!currentEntries.TryGetValue(component, out uint dirInodeNumber)) return ErrorNumber.NoSuchFile;

            ErrorNumber errno = ReadFilesetInode(dirInodeNumber, out Inode dirInode);

            if(errno != ErrorNumber.NoError) return errno;

            if((dirInode.di_mode & 0xF000) != 0x4000) return ErrorNumber.NotDirectory;

            errno = GetDirectoryEntries(dirInodeNumber, dirInode.di_u, out Dictionary<string, uint> childEntries);

            if(errno != ErrorNumber.NoError) return errno;

            currentEntries = childEntries;
        }

        // Find the target
        return !currentEntries.TryGetValue(pathComponents[^1], out uint targetInodeNumber)
                   ? ErrorNumber.NoSuchFile
                   : ReadFilesetInode(targetInodeNumber, out inode);
    }

    /// <summary>Reads the EA list data for a given inode, handling both inline and extent EAs</summary>
    /// <param name="inode">The inode whose EAs to read</param>
    /// <param name="eaData">Output buffer containing the raw jfs_ea_list data</param>
    /// <returns>Error code indicating success or failure</returns>
    ErrorNumber ReadEaList(in Inode inode, out byte[] eaData)
    {
        eaData = null;

        byte eaFlag = inode.di_ea.flag;
        uint eaSize = inode.di_ea.size;

        AaruLogging.Debug(MODULE_NAME, "ReadEaList: flag=0x{0:X2}, size={1}", eaFlag, eaSize);

        // No EAs
        if(eaFlag == 0 || eaSize == 0) return ErrorNumber.NoError;

        if((eaFlag & DXD_INLINE) != 0)
        {
            // Inline EA: data is in the inode extension area at INLINEEA_OFFSET
            if(inode.di_u is null || inode.di_u.Length < INLINEEA_OFFSET + eaSize)
            {
                AaruLogging.Debug(MODULE_NAME, "ReadEaList: inline EA data too short");

                return ErrorNumber.InvalidArgument;
            }

            var inlineSize = (int)Math.Min(eaSize, INLINEEA_SIZE);
            eaData = new byte[inlineSize];
            Array.Copy(inode.di_u, INLINEEA_OFFSET, eaData, 0, inlineSize);

            AaruLogging.Debug(MODULE_NAME, "ReadEaList: read {0} bytes inline EA", inlineSize);

            return ErrorNumber.NoError;
        }

        if((eaFlag & DXD_EXTENT) != 0)
        {
            // External EA: data is in blocks pointed to by di_ea.loc
            ulong extAddr = ExtentAddress(inode.di_ea.loc);
            uint  extLen  = ExtentLength(inode.di_ea.loc);

            if(extAddr == 0 || extLen == 0)
            {
                AaruLogging.Debug(MODULE_NAME, "ReadEaList: extent EA has no backing blocks");

                return ErrorNumber.NoError;
            }

            AaruLogging.Debug(MODULE_NAME, "ReadEaList: reading extent EA at block {0}, len={1}", extAddr, extLen);

            // Read all blocks of the EA extent
            var totalSize = (int)eaSize;
            eaData = new byte[totalSize];

            var bytesRead = 0;
            int nbperpage = 1 << _l2nbperpage;
            var nblocks   = (int)(extLen << _l2nbperpage);

            for(var i = 0; i < nblocks && bytesRead < totalSize; i += nbperpage)
            {
                int nb = Math.Min(PSIZE, totalSize - bytesRead);

                ErrorNumber errno = ReadBytes(((long)extAddr + i) * _superblock.s_bsize, nb, out byte[] blockData);

                if(errno != ErrorNumber.NoError) return errno;

                int toCopy = Math.Min(nb, blockData.Length);
                Array.Copy(blockData, 0, eaData, bytesRead, toCopy);
                bytesRead += toCopy;
            }

            AaruLogging.Debug(MODULE_NAME, "ReadEaList: read {0} bytes extent EA", bytesRead);

            return ErrorNumber.NoError;
        }

        AaruLogging.Debug(MODULE_NAME, "ReadEaList: unknown EA flag 0x{0:X2}", eaFlag);

        return ErrorNumber.NoError;
    }

    /// <summary>Parses a jfs_ea_list buffer, calling a callback for each attribute found</summary>
    /// <param name="eaData">The raw EA list data</param>
    /// <param name="callback">Callback invoked with (name, value) for each EA</param>
    /// <returns>Error code indicating success or failure</returns>
    static ErrorNumber ParseEaList(byte[] eaData, Action<string, byte[]> callback)
    {
        if(eaData.Length < 4) return ErrorNumber.NoError;

        // First 4 bytes: overall EA list size (le32)
        var listSize = BitConverter.ToUInt32(eaData, 0);

        if(listSize < 4 || listSize > eaData.Length) listSize = (uint)eaData.Length;

        // First EA starts at offset 4 (after the size field)
        var offset = 4;

        while(offset < listSize)
        {
            // Each jfs_ea: flag(1) + namelen(1) + valuelen(2) + name(namelen+1) + value(valuelen)
            if(offset + 4 > eaData.Length) break;

            // byte 0 is flag (unused), skip it
            byte namelen  = eaData[offset                        + 1];
            var  valuelen = BitConverter.ToUInt16(eaData, offset + 2);

            int entrySize = 4 + namelen + 1 + valuelen;

            if(offset + entrySize > eaData.Length) break;

            if(namelen == 0) break;

            // Name is at offset+4, null-terminated, length namelen
            string name = Encoding.ASCII.GetString(eaData, offset + 4, namelen);

            // Check if this is a known namespace; if not, prepend "os2."
            if(!name.StartsWith("system.",   StringComparison.Ordinal) &&
               !name.StartsWith("user.",     StringComparison.Ordinal) &&
               !name.StartsWith("security.", StringComparison.Ordinal) &&
               !name.StartsWith("trusted.",  StringComparison.Ordinal))
                name = XATTR_OS2_PREFIX + name;

            // Value is at offset + 4 + namelen + 1
            var value = new byte[valuelen];

            if(valuelen > 0) Array.Copy(eaData, offset + 4 + namelen + 1, value, 0, valuelen);

            callback(name, value);

            offset += entrySize;
        }

        return ErrorNumber.NoError;
    }
}