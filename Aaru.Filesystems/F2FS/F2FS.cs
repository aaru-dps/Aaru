// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : F2FS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : F2FS filesystem plugin.
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
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Interfaces;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements the Flash-Friendly File System (F2FS)</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class F2FS : IReadOnlyFilesystem
{
    const string MODULE_NAME = "F2FS plugin";

    IMediaImage _imagePlugin;
    Partition   _partition;
    Encoding    _encoding;
    Superblock  _superblock;
    Checkpoint  _checkpoint;
    byte[]      _natBitmap;
    byte[]      _checkpointData;
    bool        _mounted;
    uint        _blockSize;
    uint        _blocksPerSegment;
    uint        _cpStartAddr;
    uint        _maxBlockAddr;
    uint        _maxNid;

    /// <summary>NAT journal entries from the checkpoint's hot data summary: nid → NatEntry</summary>
    readonly Dictionary<uint, NatEntry> _natJournal = new();

    /// <summary>Cache of root directory entries: filename → inode number</summary>
    readonly Dictionary<string, uint> _rootDirectoryCache = new();

    /// <summary>Cached directory contents (node id -> entries), so path resolution does not re-walk each directory</summary>
    readonly Dictionary<uint, Dictionary<string, uint>> _directoryCache = new();

#region IFilesystem Members

    /// <inheritdoc />
    public string Name => Localization.F2FS_Name;

    /// <inheritdoc />
    public Guid Id => new("82B0920F-5F0D-4063-9F57-ADE0AE02ECE5");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

#endregion

    /// <inheritdoc />
    public FileSystem                                                Metadata         { get; private set; }
    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description)> SupportedOptions => [];
    /// <inheritdoc />
    public Dictionary<string, string>                                Namespaces       => [];
}