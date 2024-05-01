// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CBM.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commodore file system plugin.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the filesystem used in 8-bit Commodore microcomputers</summary>
public sealed partial class CBM
{
#region IReadOnlyFilesystem Members

    /// <inheritdoc />
    public ErrorNumber GetAttributes(string path, out FileAttributes attributes)
    {
        attributes = new FileAttributes();

        if(!_mounted) return ErrorNumber.AccessDenied;

        string[] pathElements = path.Split(new[]
                                           {
                                               '/'
                                           },
                                           StringSplitOptions.RemoveEmptyEntries);

        if(pathElements.Length != 1) return ErrorNumber.NotSupported;

        string filename = pathElements[0].ToUpperInvariant();

        if(!_cache.TryGetValue(filename, out CachedFile file)) return ErrorNumber.NoSuchFile;

        attributes = file.attributes;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        string[] pathElements = path.Split(new[]
                                           {
                                               '/'
                                           },
                                           StringSplitOptions.RemoveEmptyEntries);

        if(pathElements.Length != 1) return ErrorNumber.NotSupported;

        string filename = pathElements[0].ToUpperInvariant();

        if(filename.Length > 14) return ErrorNumber.NameTooLong;

        if(!_cache.TryGetValue(filename, out CachedFile file)) return ErrorNumber.NoSuchFile;

        stat = new FileEntryInfo
        {
            Attributes = file.attributes,
            BlockSize  = 256,
            Length     = (long)file.length,
            Blocks     = file.blocks,
            Inode      = file.id,
            Links      = 1
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber OpenFile(string path, out IFileNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        string[] pathElements = path.Split(new[]
                                           {
                                               '/'
                                           },
                                           StringSplitOptions.RemoveEmptyEntries);

        if(pathElements.Length != 1) return ErrorNumber.NotSupported;

        string filename = pathElements[0].ToUpperInvariant();

        if(filename.Length > 14) return ErrorNumber.NameTooLong;

        if(!_cache.TryGetValue(filename, out CachedFile file)) return ErrorNumber.NoSuchFile;

        node = new CbmFileNode
        {
            Path   = path,
            Length = (long)file.length,
            Offset = 0,
            Cache  = file.data
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not CbmFileNode mynode) return ErrorNumber.InvalidArgument;

        mynode.Cache = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(buffer is null || buffer.Length < length) return ErrorNumber.InvalidArgument;

        if(node is not CbmFileNode mynode) return ErrorNumber.InvalidArgument;

        read = length;

        if(length + mynode.Offset >= mynode.Length) read = mynode.Length - mynode.Offset;

        Array.Copy(mynode.Cache, mynode.Offset, buffer, 0, read);

        mynode.Offset += read;

        return ErrorNumber.NoError;
    }

#endregion
}