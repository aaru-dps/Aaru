// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : JFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : IBM JFS filesystem plugin
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

// ReSharper disable UnusedMember.Local

using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Interfaces;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements IBM's Journaled File System</summary>
public sealed partial class JFS : IReadOnlyFilesystem
{
    /// <summary>Cache of root directory entries mapped from filename to inode number</summary>
    readonly Dictionary<string, uint> _rootDirectoryCache = new();

    /// <summary>
    ///     Cached directory contents (directory inode number -> entries). Read-only filesystem, so entries
    ///     never go stale; without this every Stat/OpenFile/OpenDir re-reads and re-parses the on-disk D-tree
    ///     per path component, which is quadratic on huge or deeply nested directories.
    /// </summary>
    readonly Dictionary<uint, Dictionary<string, uint>> _directoryCache = new();

    /// <summary>The encoding to use for text data</summary>
    Encoding _encoding;

    /// <summary>The cached fileset inode (FILESYSTEM_I) used to look up fileset inodes</summary>
    Inode _fsInode;

    /// <summary>The media image plugin used to read from the device</summary>
    IMediaImage _imagePlugin;

    /// <summary>log2 of blocks per page (L2PSIZE - s_l2bsize)</summary>
    int _l2nbperpage;

    /// <summary>Indicates if the filesystem is currently mounted</summary>
    bool _mounted;

    /// <summary>The partition being mounted</summary>
    Partition _partition;

    /// <summary>The filesystem superblock</summary>
    SuperBlock _superblock;

    /// <inheritdoc />
    public FileSystem                                                Metadata         { get; private set; }
    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description)> SupportedOptions => [];
    /// <inheritdoc />
    public Dictionary<string, string>                                Namespaces       => [];

#region IFilesystem Members

    /// <inheritdoc />
    public string Name => Localization.JFS_Name;

    /// <inheritdoc />
    public Guid Id => new("D3BE2A41-8F28-4055-94DC-BB6C72A0E9C4");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

#endregion
}