// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FATX.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : FATX filesystem plugin
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
using System.Globalization;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements the Xbox File Allocation Table (FATX or XTAF) filesystem.</summary>
public sealed partial class XboxFatPlugin : IReadOnlyFilesystem
{
    uint                                                   _bytesPerCluster;
    CultureInfo                                            _cultureInfo;
    bool                                                   _debug;
    Dictionary<string, Dictionary<string, DirectoryEntry>> _directoryCache;
    Encoding                                               _encoding;
    ushort[]                                               _fat16;
    uint[]                                                 _fat32;
    ulong                                                  _fatStartSector;
    ulong                                                  _firstClusterSector;
    IMediaImage                                            _imagePlugin;
    bool                                                   _littleEndian;
    bool                                                   _mounted;
    Dictionary<string, DirectoryEntry>                     _rootDirectory;
    uint                                                   _sectorsPerCluster;
    FileSystemInfo                                         _statfs;
    Superblock                                             _superblock;

    /// <inheritdoc />
    public FileSystem Metadata { get; private set; }
    /// <inheritdoc />
    public string Name => Localization.XboxFatPlugin_Name;
    /// <inheritdoc />
    public Guid Id => new("ED27A721-4A17-4649-89FD-33633B46E228");
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