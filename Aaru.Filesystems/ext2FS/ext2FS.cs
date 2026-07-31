// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ext2FS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Linux extended filesystem 2, 3 and 4 plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Linux extended filesystem 2, 3 and 4 and shows information.
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

// Information from the Linux kernel
/// <inheritdoc />
/// <summary>Implements the Linux extended filesystem v2, v3 and v4</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]

// ReSharper disable once InconsistentNaming
public sealed partial class ext2FS : IReadOnlyFilesystem
{
    /// <summary>Cached root directory entries (filename to inode number)</summary>
    readonly Dictionary<string, uint> _rootDirectoryCache = [];

    /// <summary>Cached directory entries per directory inode number, with "." and ".." filtered out</summary>
    readonly Dictionary<uint, Dictionary<string, uint>> _directoryCache = [];

    /// <summary>Block group descriptors</summary>
    BlockGroupDescriptor[] _blockGroupDescriptors;

    /// <summary>Computed block size in bytes</summary>
    uint _blockSize;

    /// <summary>Number of block groups</summary>
    uint _blockGroupCount;

    /// <summary>Block group descriptor size</summary>
    ushort _descSize;

    /// <summary>The encoding used for filenames</summary>
    Encoding _encoding;

    /// <summary>Whether filetype is stored in directory entries</summary>
    bool _hasFileType;

    /// <summary>Whether the filesystem uses e2compr compression</summary>
    bool _hasCompression;

    /// <summary>The image plugin being accessed</summary>
    IMediaImage _imagePlugin;

    /// <summary>Inode size in bytes</summary>
    ushort _inodeSize;

    /// <summary>Whether 64-bit feature is enabled</summary>
    bool _is64Bit;

    /// <summary>Whether filesystem was created on GNU/Hurd</summary>
    bool _isHurd;

    /// <summary>Whether filesystem was created on MASIX</summary>
    bool _isMasix;

    /// <summary>Whether filesystem was created on Visopsys</summary>
    bool _isVisopsys;

    /// <summary>Whether the filesystem is mounted</summary>
    bool _mounted;

    /// <summary>The partition being mounted</summary>
    Partition _partition;

    /// <summary>The cached superblock</summary>
    SuperBlock _superblock;

#region IFilesystem Members

    /// <inheritdoc />
    public string Name => Localization.ext2FS_Name_Linux_extended_Filesystem_2_3_and_4;

    /// <inheritdoc />
    public Guid Id => new("6AA91B88-150B-4A7B-AD56-F84FB2DF4184");

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