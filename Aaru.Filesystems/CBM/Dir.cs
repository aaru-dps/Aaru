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
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the filesystem used in 8-bit Commodore microcomputers</summary>
public sealed partial class CBM
{
#region IReadOnlyFilesystem Members

    /// <inheritdoc />
    public ErrorNumber OpenDir(string path, out IDirNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(!string.IsNullOrEmpty(path) && string.Compare(path, "/", StringComparison.OrdinalIgnoreCase) != 0)
            return ErrorNumber.NotSupported;

        var contents = _cache.Keys.ToList();

        contents.Sort();

        node = new CbmDirNode
        {
            Path     = path,
            Position = 0,
            Contents = contents.ToArray()
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseDir(IDirNode node)
    {
        if(node is not CbmDirNode mynode) return ErrorNumber.InvalidArgument;

        mynode.Position = -1;
        mynode.Contents = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadDir(IDirNode node, out string filename)
    {
        filename = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not CbmDirNode mynode) return ErrorNumber.InvalidArgument;

        if(mynode.Position < 0) return ErrorNumber.InvalidArgument;

        if(mynode.Position >= mynode.Contents.Length) return ErrorNumber.NoError;

        filename = mynode.Contents[mynode.Position++];

        return ErrorNumber.NoError;
    }

#endregion
}