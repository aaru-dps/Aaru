// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BTRFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : B-tree file system plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the B-tree file system and shows information.
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
/// <summary>Implements detection of the b-tree filesystem (btrfs)</summary>
public sealed partial class BTRFS : IReadOnlyFilesystem
{
    const string MODULE_NAME = "BTRFS Plugin";

    /// <summary>Chunk map for logical-to-physical address translation</summary>
    List<ChunkMapping> _chunkMap;

    /// <summary>The encoding used for filenames</summary>
    Encoding _encoding;

    /// <summary>Level of the FS tree root node</summary>
    byte _fsTreeLevel;

    /// <summary>Logical byte address of the FS tree root node</summary>
    ulong _fsTreeRoot;

    /// <summary>The image plugin being accessed</summary>
    IMediaImage _imagePlugin;

    /// <summary>Whether the filesystem is mounted</summary>
    bool _mounted;

    /// <summary>The partition being mounted</summary>
    Partition _partition;

    /// <summary>Cached root directory entries (filename to DirEntry)</summary>
    Dictionary<string, DirEntry> _rootDirectoryCache;

    /// <summary>The cached superblock</summary>
    SuperBlock _superblock;

    /// <summary>Cache of tree block data keyed by logical byte address</summary>
    Dictionary<ulong, byte[]> _treeBlockCache;

    /// <summary>
    ///     Cached directory contents keyed by (directory objectid, tree root). Read-only filesystem, so entries
    ///     never go stale; without this every Stat/OpenFile/OpenDir re-walks the B-tree per path component,
    ///     which is quadratic on huge or deeply nested directories.
    /// </summary>
    Dictionary<(ulong dirObjectId, ulong treeRoot), Dictionary<string, DirEntry>> _directoryCache;

    /// <inheritdoc />
    public FileSystem                                                Metadata         { get; private set; }
    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description)> SupportedOptions => [];
    /// <inheritdoc />
    public Dictionary<string, string>                                Namespaces       => [];

#region IFilesystem Members

    /// <inheritdoc />
    public string Name => Localization.BTRFS_Name;

    /// <inheritdoc />
    public Guid Id => new("C904CF15-5222-446B-B7DB-02EAC5D781B3");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

#endregion
}