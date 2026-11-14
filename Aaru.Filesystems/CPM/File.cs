// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CP/M filesystem plugin.
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
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Filesystems;

public sealed partial class CPM
{
#region IReadOnlyFilesystem Members

    /// <inheritdoc />
    public ErrorNumber OpenFile(string path, out IFileNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        string filename = GetFilename(path);

        if(filename is null) return ErrorNumber.IsDirectory;

        if(!_fileCache.TryGetValue(filename, out byte[] file)) return ErrorNumber.NoSuchFile;

        node = new CpmFileNode
        {
            Path   = path,
            Length = file.Length,
            Offset = 0,
            Cache  = file
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not CpmFileNode mynode) return ErrorNumber.InvalidArgument;

        mynode.Cache = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(buffer is null || buffer.Length < length) return ErrorNumber.InvalidArgument;

        if(node is not CpmFileNode mynode) return ErrorNumber.InvalidArgument;

        read = length;

        if(length + mynode.Offset >= mynode.Length) read = mynode.Length - mynode.Offset;

        Array.Copy(mynode.Cache, mynode.Offset, buffer, 0, read);

        mynode.Offset += read;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadLink(string path, out string dest)
    {
        dest = null;

        return !_mounted ? ErrorNumber.AccessDenied : ErrorNumber.NotSupported;
    }

    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        string filename = GetFilename(path);

        if(filename is not null)
            return _statCache.TryGetValue(filename, out stat) ? ErrorNumber.NoError : ErrorNumber.NoSuchFile;

        stat = new FileEntryInfo
        {
            Attributes = FileAttributes.Directory,
            BlockSize  = Metadata.ClusterSize
        };

        if(_labelCreationDate != null) stat.CreationTime = DateHandlers.CpmToDateTime(_labelCreationDate);

        if(_labelUpdateDate != null) stat.StatusChangeTime = DateHandlers.CpmToDateTime(_labelUpdateDate);

        return ErrorNumber.NoError;
    }

#endregion
}