// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Reiser4.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Reiser4 filesystem plugin
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
/// <summary>Implements detection of the Reiser v4 filesystem</summary>
public sealed partial class Reiser4 : IReadOnlyFilesystem
{
    /// <summary>Block cache to avoid re-reading disk blocks during tree traversal</summary>
    Dictionary<ulong, byte[]> _blockCache;

    /// <summary>Block size in bytes</summary>
    uint _blockSize;

    /// <summary>Encoding used for filenames</summary>
    Encoding _encoding;

    /// <summary>Cached format40 super block</summary>
    Format40DiskSuperblock _format40Sb;

    /// <summary>Image plugin being accessed</summary>
    IMediaImage _imagePlugin;

    /// <summary>Size of an item header, depends on key size</summary>
    int _itemHeaderSize;

    /// <summary>Size of an on-disk key in bytes (24 or 32)</summary>
    int _keySize;

    /// <summary>Whether the filesystem uses large (32-byte) keys</summary>
    bool _largeKeys;

    /// <summary>Cached master super block</summary>
    Superblock _masterSb;

    /// <summary>Whether the filesystem is currently mounted</summary>
    bool _mounted;

    /// <summary>Partition being mounted</summary>
    Partition _partition;

    /// <summary>Cached root directory entries (filename → LargeKey of stat-data)</summary>
    Dictionary<string, LargeKey> _rootDirectoryCache;

    /// <summary>Cached directory contents keyed by directory objectid, so path resolution does not re-walk the tree</summary>
    Dictionary<ulong, Dictionary<string, LargeKey>> _directoryCache;

    /// <summary>Cached stat-data mode keyed by objectid, so ancestor directories are not re-searched on every call</summary>
    Dictionary<ulong, ushort> _modeCache;

    /// <summary>Cached parsed stat data keyed by objectid</summary>
    Dictionary<ulong, Aaru.CommonTypes.Structs.FileEntryInfo> _statCache;

    /// <summary>Cached resolved paths, so a path deep in the tree does not re-walk the whole ancestor chain</summary>
    Dictionary<string, LargeKey> _pathCache;

    /// <inheritdoc />
    public FileSystem Metadata { get; private set; }

    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description)> SupportedOptions => [];

    /// <inheritdoc />
    public Dictionary<string, string> Namespaces => [];

#region IFilesystem Members

    /// <inheritdoc />
    public string Name => Localization.Reiser4_Name;

    /// <inheritdoc />
    public Guid Id => new("301F2D00-E8D5-4F04-934E-81DFB21D15BA");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

#endregion
}