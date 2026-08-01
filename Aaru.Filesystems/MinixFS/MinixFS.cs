// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MinixFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : MINIX filesystem plugin.
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

// Information from the Minix source code
/// <inheritdoc />
/// <summary>Implements the MINIX filesystem</summary>
public sealed partial class MinixFS : IReadOnlyFilesystem
{
    /// <summary>Cached V2/V3 inodes (inode number -> inode)</summary>
    readonly Dictionary<uint, V2DiskInode> _inodeCache   = new();
    /// <summary>Cached V1 inodes (inode number -> inode)</summary>
    readonly Dictionary<uint, V1DiskInode> _inodeCacheV1 = new();

    /// <summary>Cached root directory entries (filename -> inode number)</summary>
    readonly Dictionary<string, uint> _rootDirectoryCache = new();

    /// <summary>
    ///     Cached directory contents (directory inode number -> entries). Read-only filesystem, so entries never
    ///     go stale; without this every Stat/OpenFile re-reads whole directories per path component, which is
    ///     quadratic on huge (million-entry) or deeply nested directories.
    /// </summary>
    readonly Dictionary<uint, Dictionary<string, uint>> _directoryCache = new();

    /// <summary>Cached indirect zone blocks (block number -> data), so sequential reads do not re-read the chain</summary>
    readonly Dictionary<int, byte[]> _indirectBlockCache = new();


    /// <summary>Block size in bytes</summary>
    int _blockSize;

    /// <summary>Encoding used for filenames</summary>
    Encoding _encoding;

    /// <summary>Maximum filename size</summary>
    int _filenameSize;

    /// <summary>First data zone</summary>
    int _firstDataZone;

    /// <summary>First block containing inodes</summary>
    int _firstInodeBlock;

    /// <summary>Image plugin being accessed</summary>
    IMediaImage _imagePlugin;

    /// <summary>Number of inode bitmap blocks</summary>
    int _imapBlocks;

    /// <summary>Calculated inodes per block</summary>
    int _inodesPerBlock;

    /// <summary>Whether filesystem was cleanly unmounted</summary>
    bool _isClean;

    /// <summary>Whether the filesystem uses little-endian byte order</summary>
    bool _littleEndian;

    /// <summary>Log2 of zones per block</summary>
    int _logZoneSize;

    /// <summary>Maximum file size</summary>
    uint _maxSize;

    /// <summary>Whether filesystem is mounted</summary>
    bool _mounted;

    /// <summary>Total number of inodes</summary>
    uint _ninodes;

    /// <summary>Partition being mounted</summary>
    Partition _partition;


    /// <summary>Filesystem version</summary>
    FilesystemVersion _version;

    /// <summary>Number of zone bitmap blocks</summary>
    int _zmapBlocks;

    /// <summary>Total number of zones</summary>
    uint _zones;

    /// <inheritdoc />
    public FileSystem Metadata { get; private set; }

    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description)> SupportedOptions { get; } = [];

    /// <inheritdoc />
    public Dictionary<string, string> Namespaces { get; } = [];

#region IFilesystem Members

    /// <inheritdoc />
    public string Name => Localization.MinixFS_Name;

    /// <inheritdoc />
    public Guid Id => new("FE248C3B-B727-4AE5-A39F-79EA9A07D4B3");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

#endregion
}