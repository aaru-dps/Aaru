// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : JXFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : JXFileSystem plugin.
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
/// <summary>Implements the AmigaOS 4 JXFileSystem</summary>
public sealed partial class JXFS : IReadOnlyFilesystem
{
    const string MODULE_NAME = "JXFS plugin";

    /// <summary>Loaded directories, by node number</summary>
    readonly Dictionary<uint, Dictionary<string, DirectoryEntry>> _directoryCache = new();

    /// <summary>Block size in bytes</summary>
    uint _blockSize;

    /// <summary>Whether names are compared case-sensitively</summary>
    bool _caseSensitive;

    /// <summary>Extents of the <c>:dir:</c> system file, which holds the directory containers</summary>
    List<Extent> _dirFile;

    /// <summary>The encoding used for names and comments</summary>
    Encoding _encoding;

    /// <summary>Free blocks, as stored in the root directory container</summary>
    uint _freeBlocks;

    /// <summary>The media image plugin used to read from the device</summary>
    IMediaImage _imagePlugin;

    /// <summary>Extents of the <c>:misc:</c> system file, which holds small file data, attributes and links</summary>
    List<Extent> _miscFile;

    /// <summary>Whether the filesystem is mounted</summary>
    bool _mounted;

    /// <summary>Extents of the <c>:node:</c> system file, which holds the nodes</summary>
    List<Extent> _nodeFile;

    /// <summary>First block of the <c>:node:</c> system file, used to read the first nodes before its extents are known</summary>
    ulong _nodeFileStart;

    /// <summary>Nodes in each node container block</summary>
    uint _nodesPerBlock;

    /// <summary>The partition being mounted</summary>
    Partition _partition;

    /// <summary>The root directory entry</summary>
    DirectoryEntry _rootEntry;

    /// <summary>The superblock</summary>
    SuperBlock _superBlock;

    /// <summary>Whether the journal holds a transaction that has not been applied to the volume</summary>
    bool _unfinishedTransaction;

    /// <inheritdoc />
    public FileSystem Metadata { get; private set; }

    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description)> SupportedOptions { get; } = [];

    /// <inheritdoc />
    public Dictionary<string, string> Namespaces { get; } = [];

#region IFilesystem Members

    /// <inheritdoc />
    public string Name => Localization.JXFS_Name;

    /// <inheritdoc />
    public Guid Id => new("8E0A3C55-2F6B-4D1E-9A47-6C1B0F5D3E21");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

#endregion
}