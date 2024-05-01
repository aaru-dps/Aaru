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
using System.Collections.Generic;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the filesystem used in 8-bit Commodore microcomputers</summary>
public sealed partial class CBM : IReadOnlyFilesystem
{
    byte[]                         _bam;
    Dictionary<string, CachedFile> _cache;
    bool                           _debug;
    byte[]                         _diskHeader;
    bool                           _is1581;
    bool                           _mounted;
    byte[]                         _root;
    FileSystemInfo                 _statfs;

#region IReadOnlyFilesystem Members

    /// <inheritdoc />
    public string Name => Localization.CBM_Name;

    /// <inheritdoc />
    public Guid Id => new("D104744E-A376-450C-BAC0-1347C93F983B");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public ErrorNumber ListXAttr(string path, out List<string> xattrs)
    {
        xattrs = null;

        return ErrorNumber.NotSupported;
    }

    /// <inheritdoc />
    public ErrorNumber GetXattr(string path, string xattr, ref byte[] buf) => ErrorNumber.NotSupported;

    /// <inheritdoc />
    public ErrorNumber ReadLink(string path, out string dest)
    {
        dest = null;

        return ErrorNumber.NotSupported;
    }

    /// <inheritdoc />
    public FileSystem Metadata { get; private set; }

    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
        Array.Empty<(string name, Type type, string description)>();

    /// <inheritdoc />
    public Dictionary<string, string> Namespaces => null;

#endregion

    static Dictionary<string, string> GetDefaultOptions() => new()
    {
        {
            "debug", false.ToString()
        }
    };

    ulong CbmChsToLba(byte track, byte sector, bool is1581)
    {
        if(track == 0 || track > 40) return 0;

        if(is1581) return (ulong)((track - 1) * 40 + sector - 1);

        return trackStartingLbas[track - 1] + sector;
    }
}