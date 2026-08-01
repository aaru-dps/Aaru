// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Reiser.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Reiser filesystem plugin
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
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Interfaces;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements the Reiser v3 filesystem</summary>
public sealed partial class Reiser : IReadOnlyFilesystem
{
    /// <summary>Block cache to avoid re-reading disk blocks during tree traversal</summary>
    Dictionary<uint, byte[]> _blockCache;

    /// <summary>Block size in bytes</summary>
    int _blockSize;

    /// <summary>Encoding used for filenames</summary>
    Encoding _encoding;

    /// <summary>Image plugin being accessed</summary>
    IMediaImage _imagePlugin;

    /// <summary>Key version: KEY_FORMAT_3_5 or KEY_FORMAT_3_6</summary>
    int _keyVersion;

    /// <summary>Whether filesystem is mounted</summary>
    bool _mounted;

    /// <summary>Partition being mounted</summary>
    Partition                                       _partition;
    /// <summary>Cached root directory entries (filename → (dirId, objectId))</summary>
    Dictionary<string, (uint dirId, uint objectId)> _rootDirectoryCache;

    /// <summary>Cached directory contents keyed by (dirId, objectId), so path resolution does not re-walk the S+Tree</summary>
    Dictionary<(uint dirId, uint objectId), Dictionary<string, (uint dirId, uint objectId)>> _directoryCache;

    /// <summary>Cached raw stat-data mode keyed by (dirId, objectId), so ancestor directories are not re-searched on every call</summary>
    Dictionary<(uint dirId, uint objectId), ushort> _modeCache;

    /// <summary>Cached parsed stat data keyed by (dirId, objectId)</summary>
    Dictionary<(uint dirId, uint objectId), Aaru.CommonTypes.Structs.FileEntryInfo> _statCache;

    /// <summary>Cached resolved paths, so a path deep in the tree does not re-walk the whole ancestor chain</summary>
    Dictionary<string, (uint dirId, uint objectId)> _pathCache;

    /// <summary>Cached superblock</summary>
    Superblock _superblock;

    /// <summary>Cached xattr root directory entries (dirname → (dirId, objectId)), or null if xattrs unavailable</summary>
    Dictionary<string, (uint dirId, uint objectId)> _xattrRootEntries;

    /// <inheritdoc />
    public FileSystem Metadata { get; private set; }

    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description)> SupportedOptions => [];

    /// <inheritdoc />
    public Dictionary<string, string> Namespaces => [];

#region IFilesystem Members

    /// <inheritdoc />
    public string Name => Localization.Reiser_Name;

    /// <inheritdoc />
    public Guid Id => new("1D8CD8B8-27E6-410F-9973-D16409225FBA");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

#endregion
}