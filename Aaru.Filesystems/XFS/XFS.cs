// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : XFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : XFS filesystem plugin.
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
/// <summary>Implements SGI's XFS</summary>
public sealed partial class XFS : IReadOnlyFilesystem
{
    const string MODULE_NAME = "XFS plugin";

    /// <summary>Cached inodes (inode number -> dinode)</summary>
    readonly Dictionary<ulong, Dinode> _inodeCache = new();

    /// <summary>Cached root directory entries (filename -> inode number)</summary>
    readonly Dictionary<string, ulong> _rootDirectoryCache = new();

    /// <summary>
    ///     Cached directory contents (directory inode number -> entries). Read-only filesystem, so entries
    ///     never go stale; without this every Stat/OpenFile/OpenDir re-reads and re-parses whole directories
    ///     per path component, which is quadratic on huge or deeply nested directories.
    /// </summary>
    readonly Dictionary<ulong, Dictionary<string, ulong>> _directoryCache = new();

    /// <summary>Number of filesystem blocks per directory block (1 for dir v1, 1 &lt;&lt; dirblklog for dir v2)</summary>
    uint _dirBlockFsBlocks;

    /// <summary>Encoding used for filenames</summary>
    Encoding _encoding;

    /// <summary>Whether the filesystem has ftype support in directory entries</summary>
    bool _hasFtype;

    /// <summary>Image plugin being accessed</summary>
    IMediaImage _imagePlugin;

    /// <summary>Whether the filesystem uses directory format v1 (pre-DIRV2BIT)</summary>
    bool _isDirV1;

    /// <summary>Whether filesystem is mounted</summary>
    bool _mounted;

    /// <summary>Partition being mounted</summary>
    Partition _partition;

    /// <summary>Cached superblock</summary>
    Superblock _superblock;

    /// <summary>Whether v3 inodes are in use (v5 superblock)</summary>
    bool _v3Inodes;

    /// <inheritdoc />
    public FileSystem                                                Metadata         { get; private set; }
    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description)> SupportedOptions => [];
    /// <inheritdoc />
    public Dictionary<string, string>                                Namespaces       => [];

#region IFilesystem Members

    /// <inheritdoc />
    public string Name => Localization.XFS_Name;

    /// <inheritdoc />
    public Guid Id => new("1D8CD8B8-27E6-410F-9973-D16409225FBA");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

#endregion
}