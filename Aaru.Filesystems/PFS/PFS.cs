// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : PFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Professional File System plugin.
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

// ReSharper disable UnusedType.Local

using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Interfaces;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements the Professional File System</summary>
public sealed partial class PFS : IReadOnlyFilesystem
{
    const    string                                MODULE_NAME         = "PFS plugin";
    readonly Dictionary<string, DirEntryCacheItem> _rootDirectoryCache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Cached directory contents (directory anode number -> entries). Read-only filesystem, so entries never
    ///     go stale; without this every Stat/OpenFile re-reads whole directories per path component, which is
    ///     quadratic on huge or deeply nested directories.
    /// </summary>
    readonly Dictionary<uint, Dictionary<string, DirEntryCacheItem>> _directoryCache = new();
    ushort                                                           _anodesPerBlock;
    uint                                                             _blockSize;
    Encoding                                                         _encoding;
    ushort                                                           _filenameSize;
    uint                                                             _firstReserved;
    bool                                                             _hasExtension;
    IMediaImage                                                      _imagePlugin;
    bool                                                             _isMultiUser;
    bool                                                             _largeDirSupport;
    uint                                                             _lastReserved;
    ModeFlags                                                        _modeFlags;

    // Instance fields for mounted volume
    bool               _mounted;
    Partition          _partition;
    ushort             _reservedBlockSize;
    RootBlock          _rootBlock;
    RootBlockExtension _rootBlockExtension;
    bool               _splitAnodeMode;
    string             _volumeName;

    /// <inheritdoc />
    public FileSystem                                                Metadata         { get; private set; }
    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description)> SupportedOptions { get; } = [];
    /// <inheritdoc />
    public Dictionary<string, string>                                Namespaces       { get; } = [];


#region IFilesystem Members

    /// <inheritdoc />
    public string Name => Localization.PFS_Name;

    /// <inheritdoc />
    public Guid Id => new("68DE769E-D957-406A-8AE4-3781CA8CDA77");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

#endregion
}