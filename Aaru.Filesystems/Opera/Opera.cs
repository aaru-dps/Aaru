// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Opera.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Opera filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Opera filesystem and shows information.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Schemas;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements the 3DO Opera filesystem</summary>
public sealed partial class OperaFS : IReadOnlyFilesystem
{
    bool                                                               _debug;
    Dictionary<string, Dictionary<string, DirectoryEntryWithPointers>> _directoryCache;
    IMediaImage                                                        _image;
    bool                                                               _mounted;
    Dictionary<string, DirectoryEntryWithPointers>                     _rootDirectoryCache;
    FileSystemInfo                                                     _statfs;
    uint                                                               _volumeBlockSizeRatio;

    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => Localization.OperaFS_Name;
    /// <inheritdoc />
    public Guid Id => new("0ec84ec7-eae6-4196-83fe-943b3fe46dbd");
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
    public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
        Array.Empty<(string name, Type type, string description)>();

    /// <inheritdoc />
    public Dictionary<string, string> Namespaces => null;

    static Dictionary<string, string> GetDefaultOptions() => new()
    {
        {
            "debug", false.ToString()
        }
    };
}