// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : High Performance Optical File System plugin.
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

using System.Collections.Generic;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Filesystems;

public sealed partial class HPOFS
{
    /// <inheritdoc />
    public ErrorNumber OpenDir(string path, out IDirNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        ErrorNumber errno = GetEntry(path, out string key, out Entry entry);

        if(errno != ErrorNumber.NoError) return errno;

        if(entry is not null && !entry.IsDirectory) return ErrorNumber.NotDirectory;

        if(!_children.TryGetValue(key, out List<string> contents)) return ErrorNumber.NoSuchFile;

        node = new HpofsDirNode
        {
            Path     = "/" + key,
            Position = 0,
            Contents = contents.ToArray()
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseDir(IDirNode node)
    {
        if(node is not HpofsDirNode hpofsNode) return ErrorNumber.InvalidArgument;

        hpofsNode.Position = -1;
        hpofsNode.Contents = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadDir(IDirNode node, out string filename)
    {
        filename = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not HpofsDirNode hpofsNode) return ErrorNumber.InvalidArgument;

        if(hpofsNode.Position < 0) return ErrorNumber.InvalidArgument;

        if(hpofsNode.Position >= hpofsNode.Contents.Length) return ErrorNumber.NoError;

        filename = hpofsNode.Contents[hpofsNode.Position++];

        return ErrorNumber.NoError;
    }

    /// <summary>Finds an entry by path</summary>
    /// <param name="path">Path</param>
    /// <param name="key">Normalized path, without leading or trailing slashes, empty for root</param>
    /// <param name="entry">Entry, <c>null</c> for the root directory if it has no record</param>
    ErrorNumber GetEntry(string path, out string key, out Entry entry)
    {
        key   = (path ?? "").Replace('\\', '/').Trim('/');
        entry = null;

        if(key is "" or ".")
        {
            key   = "";
            entry = _root;

            return ErrorNumber.NoError;
        }

        return _entries.TryGetValue(key, out entry) ? ErrorNumber.NoError : ErrorNumber.NoSuchFile;
    }
}