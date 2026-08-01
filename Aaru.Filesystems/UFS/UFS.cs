// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : UFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : UNIX FIle System plugin.
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

// Using information from Linux kernel headers, several UNIX and BSD headers, and FreeBSD
/// <inheritdoc />
/// <summary>Implements the BSD Fast File System (FFS, aka UNIX File System)</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed partial class UFSPlugin : IReadOnlyFilesystem
{
    const string MODULE_NAME = "UFS plugin";

    bool                     _bigEndian;
    Encoding                 _encoding;
    uint                     _extAttrDirInode;
    bool                     _hasFreeBsdExtattr;
    IMediaImage              _imagePlugin;
    bool                     _mounted;
    Partition                _partition;
    List<DirectoryEntryInfo> _rootEntries;
    UfsSuperBlock            _superBlock;

    /// <summary>
    ///     Cached directory contents (directory inode number -> parsed entries). Read-only filesystem, so
    ///     entries never go stale; without this every Stat/OpenFile re-reads whole directories per path
    ///     component, which is quadratic on huge or deeply nested directories.
    /// </summary>
    readonly Dictionary<uint, CachedDirectory> _directoryCache = new();

#region IFilesystem Members

    /// <inheritdoc />
    public string Name => Localization.UFSPlugin_Name;

    /// <inheritdoc />
    public Guid Id => new("CC90D342-05DB-48A8-988C-C1FE000034A3");

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