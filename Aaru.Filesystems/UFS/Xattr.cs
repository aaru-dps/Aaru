// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Xattr.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : UNIX FIle System plugin.
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
using System.Diagnostics.CodeAnalysis;
using Aaru.CommonTypes.Enums;

namespace Aaru.Filesystems;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed partial class UFSPlugin
{
    // UFS2 extattr namespace constants
    const byte EXTATTR_NAMESPACE_USER   = 1;
    const byte EXTATTR_NAMESPACE_SYSTEM = 2;

    // FreeBSD UFS1 extattr constants
    const uint UFS_EXTATTR_MAGIC           = 0x00b5d5ec;
    const uint UFS_EXTATTR_VERSION         = 0x00000003;
    const uint UFS_EXTATTR_ATTR_FLAG_INUSE = 0x00000001;
    const int  UFS_EXTATTR_FILEHEADER_SIZE = 12; // magic(4) + version(4) + size(4)
    const int  UFS_EXTATTR_HEADER_SIZE     = 12; // flags(4) + len(4) + i_gen(4)

    /// <inheritdoc />
    public ErrorNumber ListXAttr(string path, out List<string> xattrs)
    {
        xattrs = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(_superBlock.fs_isUfs2) return ListXAttrUfs2(path, out xattrs);

        if(_superBlock.fs_isSolaris) return ListXAttrSolaris(path, out xattrs);

        if(_hasFreeBsdExtattr) return ListXAttrFreeBsd(path, out xattrs);

        return ErrorNumber.NotSupported;
    }

    /// <inheritdoc />
    public ErrorNumber GetXattr(string path, string xattr, ref byte[] buf)
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        if(_superBlock.fs_isUfs2) return GetXattrUfs2(path, xattr, ref buf);

        if(_superBlock.fs_isSolaris) return GetXattrSolaris(path, xattr, ref buf);

        if(_hasFreeBsdExtattr) return GetXattrFreeBsd(path, xattr, ref buf);

        return ErrorNumber.NotSupported;
    }

    /// <summary>Lists extended attributes for UFS2 (inline extattr in di_extb blocks)</summary>
    ErrorNumber ListXAttrUfs2(string path, out List<string> xattrs)
    {
        xattrs = null;

        ErrorNumber errno = ResolvePath(path, out uint inodeNumber);

        if(errno != ErrorNumber.NoError) return errno;

        errno = ReadInode2(inodeNumber, out Inode2 inode);

        if(errno != ErrorNumber.NoError) return errno;

        if(inode.di_extsize == 0)
        {
            xattrs = [];

            return ErrorNumber.NoError;
        }

        errno = ReadExtAttrData(inode, out byte[] eaData);

        if(errno != ErrorNumber.NoError) return errno;

        xattrs = [];
        ParseExtAttrList(eaData, xattrs);

        return ErrorNumber.NoError;
    }

    /// <summary>Gets an extended attribute for UFS2 (inline extattr in di_extb blocks)</summary>
    ErrorNumber GetXattrUfs2(string path, string xattr, ref byte[] buf)
    {
        ErrorNumber errno = ResolvePath(path, out uint inodeNumber);

        if(errno != ErrorNumber.NoError) return errno;

        errno = ReadInode2(inodeNumber, out Inode2 inode);

        if(errno != ErrorNumber.NoError) return errno;

        if(inode.di_extsize == 0) return ErrorNumber.NoSuchExtendedAttribute;

        errno = ReadExtAttrData(inode, out byte[] eaData);

        if(errno != ErrorNumber.NoError) return errno;

        return FindExtAttr(eaData, xattr, ref buf);
    }

    /// <summary>Lists extended attributes for Solaris UFS (xattr directory via ic_oeftflag)</summary>
    ErrorNumber ListXAttrSolaris(string path, out List<string> xattrs)
    {
        xattrs = null;

        ErrorNumber errno = ResolvePath(path, out uint inodeNumber);

        if(errno != ErrorNumber.NoError) return errno;

        errno = ReadInodeSun(inodeNumber, out InodeSun inode);

        if(errno != ErrorNumber.NoError) return errno;

        if(inode.ic_oeftflag == 0)
        {
            xattrs = [];

            return ErrorNumber.NoError;
        }

        // ic_oeftflag is the inode number of the xattr directory
        errno = ParseDirectory(inode.ic_oeftflag, out List<DirectoryEntryInfo> entries);

        if(errno != ErrorNumber.NoError) return errno;

        xattrs = [];

        foreach(DirectoryEntryInfo entry in entries)
        {
            if(entry.Name is "." or ".." or "SUNWattr_ro" or "SUNWattr_rw") continue;

            xattrs.Add(entry.Name);
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Gets an extended attribute for Solaris UFS (xattr directory via ic_oeftflag)</summary>
    ErrorNumber GetXattrSolaris(string path, string xattr, ref byte[] buf)
    {
        ErrorNumber errno = ResolvePath(path, out uint inodeNumber);

        if(errno != ErrorNumber.NoError) return errno;

        errno = ReadInodeSun(inodeNumber, out InodeSun inode);

        if(errno != ErrorNumber.NoError) return errno;

        if(inode.ic_oeftflag == 0) return ErrorNumber.NoSuchExtendedAttribute;

        // ic_oeftflag is the inode number of the xattr directory
        errno = ParseDirectory(inode.ic_oeftflag, out List<DirectoryEntryInfo> entries);

        if(errno != ErrorNumber.NoError) return errno;

        foreach(DirectoryEntryInfo entry in entries)
        {
            if(entry.Name != xattr) continue;

            // Read the xattr file's content
            ErrorNumber readErr = ReadInodeSun(entry.Inode, out InodeSun xattrInode);

            if(readErr != ErrorNumber.NoError) return readErr;

            readErr = ReadInodeData(entry.Inode, 0, (long)xattrInode.ic_lsize, out buf);

            if(readErr != ErrorNumber.NoError) return readErr;

            return ErrorNumber.NoError;
        }

        return ErrorNumber.NoSuchExtendedAttribute;
    }

    /// <summary>Parses UFS2 extattr records to build a name list</summary>
    void ParseExtAttrList(byte[] eaData, List<string> xattrs)
    {
        var pos = 0;

        while(pos + 7 <= eaData.Length)
        {
            var eaLength = BitConverter.ToUInt32(eaData, pos);

            if(eaLength < 7 || eaLength == 0) break;

            if(pos + (int)eaLength > eaData.Length) break;

            byte eaNamespace = eaData[pos + 4];
            byte eaNameLen   = eaData[pos + 6];

            if(eaNameLen > 0 && pos + 7 + eaNameLen <= eaData.Length)
            {
                string nsPrefix = eaNamespace switch
                                  {
                                      EXTATTR_NAMESPACE_USER   => "user.",
                                      EXTATTR_NAMESPACE_SYSTEM => "system.",
                                      _                        => $"ns{eaNamespace}."
                                  };

                string name = _encoding.GetString(eaData, pos + 7, eaNameLen);
                xattrs.Add(nsPrefix + name);
            }

            pos += (int)eaLength;
        }
    }

    /// <summary>Finds a UFS2 extattr by name and returns its content</summary>
    ErrorNumber FindExtAttr(byte[] eaData, string xattr, ref byte[] buf)
    {
        byte   targetNamespace;
        string targetName;

        if(xattr.StartsWith("user.", StringComparison.Ordinal))
        {
            targetNamespace = EXTATTR_NAMESPACE_USER;
            targetName      = xattr[5..];
        }
        else if(xattr.StartsWith("system.", StringComparison.Ordinal))
        {
            targetNamespace = EXTATTR_NAMESPACE_SYSTEM;
            targetName      = xattr[7..];
        }
        else
        {
            targetNamespace = EXTATTR_NAMESPACE_USER;
            targetName      = xattr;
        }

        var pos = 0;

        while(pos + 7 <= eaData.Length)
        {
            var eaLength = BitConverter.ToUInt32(eaData, pos);

            if(eaLength < 7 || eaLength == 0) break;

            if(pos + (int)eaLength > eaData.Length) break;

            byte eaNamespace     = eaData[pos + 4];
            byte eaContentPadLen = eaData[pos + 5];
            byte eaNameLen       = eaData[pos + 6];

            if(eaNamespace == targetNamespace && eaNameLen == targetName.Length && pos + 7 + eaNameLen <= eaData.Length)
            {
                string name = _encoding.GetString(eaData, pos + 7, eaNameLen);

                if(name == targetName)
                {
                    int baseLen     = 7 + eaNameLen + 7 & ~7;
                    int contentSize = (int)eaLength - baseLen - eaContentPadLen;

                    if(contentSize > 0 && pos + baseLen + contentSize <= eaData.Length)
                    {
                        buf = new byte[contentSize];
                        Array.Copy(eaData, pos + baseLen, buf, 0, contentSize);
                    }
                    else
                        buf = [];

                    return ErrorNumber.NoError;
                }
            }

            pos += (int)eaLength;
        }

        return ErrorNumber.NoSuchExtendedAttribute;
    }

    /// <summary>Reads the extattr data from a UFS2 inode's di_extb blocks</summary>
    ErrorNumber ReadExtAttrData(Inode2 inode, out byte[] data)
    {
        data = null;

        uint extSize = inode.di_extsize;

        if(extSize == 0)
        {
            data = [];

            return ErrorNumber.NoError;
        }

        data = new byte[extSize];

        var offset = 0;
        int bsize  = _superBlock.fs_bsize;

        for(var i = 0; i < NXADDR && offset < (int)extSize; i++)
        {
            if(inode.di_extb[i] == 0)
            {
                int toCopy = Math.Min(bsize, (int)extSize - offset);
                offset += toCopy;

                continue;
            }

            ErrorNumber errno = ReadFragments(inode.di_extb[i], _superBlock.fs_frag, out byte[] blockData);

            if(errno != ErrorNumber.NoError) return errno;

            int copyLen = Math.Min(blockData.Length, (int)extSize - offset);
            Array.Copy(blockData, 0, data, offset, copyLen);
            offset += copyLen;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Lists extended attributes for FreeBSD UFS1 (/.attribute/ backing files)</summary>
    ErrorNumber ListXAttrFreeBsd(string path, out List<string> xattrs)
    {
        xattrs = null;

        ErrorNumber errno = ResolvePath(path, out uint inodeNumber);

        if(errno != ErrorNumber.NoError) return errno;

        // Read the target inode to get its generation number
        errno = ReadInode(inodeNumber, out Inode targetInode);

        if(errno != ErrorNumber.NoError) return errno;

        xattrs = [];

        // Scan /.attribute/system/ and /.attribute/user/ directories
        errno = GetDirectory(_extAttrDirInode, out CachedDirectory attrDir);

        if(errno != ErrorNumber.NoError) return errno;

        foreach(DirectoryEntryInfo nsEntry in attrDir.Entries)
        {
            string nsPrefix;

            if(nsEntry.Name == "system")
                nsPrefix = "system.";
            else if(nsEntry.Name == "user")
                nsPrefix = "user.";
            else
                continue;

            errno = GetDirectory(nsEntry.Inode, out CachedDirectory nsDir);

            if(errno != ErrorNumber.NoError) continue;

            foreach(DirectoryEntryInfo attrEntry in nsDir.Entries)
            {
                if(attrEntry.Name is "." or "..") continue;

                // Check if this attribute is set for the target inode
                if(IsFreeBsdExtAttrSet(attrEntry.Inode, inodeNumber, targetInode.di_gen))
                    xattrs.Add(nsPrefix + attrEntry.Name);
            }
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Gets an extended attribute for FreeBSD UFS1 (/.attribute/ backing files)</summary>
    ErrorNumber GetXattrFreeBsd(string path, string xattr, ref byte[] buf)
    {
        ErrorNumber errno = ResolvePath(path, out uint inodeNumber);

        if(errno != ErrorNumber.NoError) return errno;

        errno = ReadInode(inodeNumber, out Inode targetInode);

        if(errno != ErrorNumber.NoError) return errno;

        // Parse "namespace.name" format
        string targetNsDir;
        string targetName;

        if(xattr.StartsWith("user.", StringComparison.Ordinal))
        {
            targetNsDir = "user";
            targetName  = xattr[5..];
        }
        else if(xattr.StartsWith("system.", StringComparison.Ordinal))
        {
            targetNsDir = "system";
            targetName  = xattr[7..];
        }
        else
        {
            targetNsDir = "user";
            targetName  = xattr;
        }

        // Find the namespace subdirectory
        errno = GetDirectory(_extAttrDirInode, out CachedDirectory attrDir);

        if(errno != ErrorNumber.NoError) return errno;

        if(!attrDir.ByName.TryGetValue(targetNsDir, out uint nsDirInode) || nsDirInode == 0)
            return ErrorNumber.NoSuchExtendedAttribute;

        // Find the attribute backing file
        errno = GetDirectory(nsDirInode, out CachedDirectory nsDir);

        if(errno != ErrorNumber.NoError) return errno;

        List<DirectoryEntryInfo> attrEntries = nsDir.Entries;

        uint attrFileInode = 0;

        foreach(DirectoryEntryInfo attrEntry in attrEntries)
        {
            if(attrEntry.Name != targetName) continue;

            attrFileInode = attrEntry.Inode;

            break;
        }

        if(attrFileInode == 0) return ErrorNumber.NoSuchExtendedAttribute;

        return ReadFreeBsdExtAttr(attrFileInode, inodeNumber, targetInode.di_gen, out buf);
    }

    /// <summary>Checks if a FreeBSD UFS1 extattr backing file has data for a given inode</summary>
    bool IsFreeBsdExtAttrSet(uint backingFileInode, uint targetInode, int targetGen)
    {
        ErrorNumber errno =
            ReadFreeBsdExtAttrHeader(backingFileInode, out uint magic, out uint version, out uint attrSize);

        if(errno != ErrorNumber.NoError || magic != UFS_EXTATTR_MAGIC) return false;

        // Read the per-inode header
        long entryOffset = UFS_EXTATTR_FILEHEADER_SIZE + (long)targetInode * (UFS_EXTATTR_HEADER_SIZE + attrSize);

        errno = ReadInodeData(backingFileInode, entryOffset, UFS_EXTATTR_HEADER_SIZE, out byte[] headerData);

        if(errno != ErrorNumber.NoError || headerData.Length < UFS_EXTATTR_HEADER_SIZE) return false;

        var flags = BitConverter.ToUInt32(headerData, 0);

        return (flags & UFS_EXTATTR_ATTR_FLAG_INUSE) != 0;
    }

    /// <summary>Reads the file header of a FreeBSD UFS1 extattr backing file</summary>
    ErrorNumber ReadFreeBsdExtAttrHeader(uint backingFileInode, out uint magic, out uint version, out uint attrSize)
    {
        magic    = 0;
        version  = 0;
        attrSize = 0;

        ErrorNumber errno = ReadInodeData(backingFileInode, 0, UFS_EXTATTR_FILEHEADER_SIZE, out byte[] data);

        if(errno != ErrorNumber.NoError || data.Length < UFS_EXTATTR_FILEHEADER_SIZE)
            return errno != ErrorNumber.NoError ? errno : ErrorNumber.InvalidArgument;

        magic    = BitConverter.ToUInt32(data, 0);
        version  = BitConverter.ToUInt32(data, 4);
        attrSize = BitConverter.ToUInt32(data, 8);

        return ErrorNumber.NoError;
    }

    /// <summary>Reads the extattr data for a specific inode from a FreeBSD UFS1 backing file</summary>
    ErrorNumber ReadFreeBsdExtAttr(uint backingFileInode, uint targetInode, int targetGen, out byte[] buf)
    {
        buf = null;

        ErrorNumber errno =
            ReadFreeBsdExtAttrHeader(backingFileInode, out uint magic, out uint version, out uint attrSize);

        if(errno != ErrorNumber.NoError) return errno;

        if(magic != UFS_EXTATTR_MAGIC) return ErrorNumber.InvalidArgument;

        long entryOffset = UFS_EXTATTR_FILEHEADER_SIZE + (long)targetInode * (UFS_EXTATTR_HEADER_SIZE + attrSize);

        // Read the per-inode header
        errno = ReadInodeData(backingFileInode, entryOffset, UFS_EXTATTR_HEADER_SIZE, out byte[] headerData);

        if(errno != ErrorNumber.NoError || headerData.Length < UFS_EXTATTR_HEADER_SIZE)
            return ErrorNumber.NoSuchExtendedAttribute;

        var flags = BitConverter.ToUInt32(headerData, 0);
        var len   = BitConverter.ToUInt32(headerData, 4);
        var iGen  = BitConverter.ToUInt32(headerData, 8);

        if((flags & UFS_EXTATTR_ATTR_FLAG_INUSE) == 0) return ErrorNumber.NoSuchExtendedAttribute;

        if(iGen != (uint)targetGen) return ErrorNumber.NoSuchExtendedAttribute;

        if(len > attrSize) len = attrSize;

        // Read the attribute data
        errno = ReadInodeData(backingFileInode, entryOffset + UFS_EXTATTR_HEADER_SIZE, len, out buf);

        return errno;
    }
}