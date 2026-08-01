// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AppleHFSPlus.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Hierarchical File System Plus plugin.
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
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

namespace Aaru.Filesystems;

// Information from Apple TechNote 1150: https://developer.apple.com/legacy/library/technotes/tn/tn1150.html
/// <inheritdoc />
/// <summary>Implements detection of Apple Hierarchical File System Plus (HFS+)</summary>
public sealed partial class AppleHFSPlus : IReadOnlyFilesystem
{
    /// <summary>Module name for debugging</summary>
    const string MODULE_NAME = "HFS+ plugin";

    /// <summary>Attributes B-Tree header information (null until first read)</summary>
    BTHeaderRec? _attributesBTreeHeader;

    /// <summary>Attributes File fork data (null if attributes file doesn't exist)</summary>
    HFSPlusForkData? _attributesFile;

    /// <summary>Whether the Extents Overflow File B-Tree header has been read</summary>
    bool _extentsHeaderLoaded;

    /// <summary>Name equality comparer matching the volume's key comparison rules</summary>
    HfsPlusNameComparer _nameComparer;

    /// <summary>Catalog B-Tree header information</summary>
    BTHeaderRec                                        _catalogBTreeHeader;
    /// <summary>Cached directory entries by CNID, each entry keyed by filename</summary>
    Dictionary<uint, Dictionary<string, CatalogEntry>> _directoryCaches;

    /// <summary>Extents Overflow File B-Tree header information</summary>
    BTHeaderRec _extentsFileHeader;

    /// <summary>Filesystem information</summary>
    FileSystemInfo _fileSystemInfo;

    /// <summary>Offset in sectors to the start of HFS+ volume (for wrapped volumes, 0 for pure HFS+)</summary>
    ulong _hfsPlusVolumeOffset;

    /// <summary>Media image plugin reference</summary>
    IMediaImage _imagePlugin;

    /// <summary>Whether the volume uses case-sensitive name comparison (HFSX only)</summary>
    bool _isCaseSensitive;

    /// <summary>Whether the filesystem is mounted</summary>
    bool _mounted;

    /// <summary>Partition start sector</summary>
    ulong _partitionStart;

    /// <summary>Cached root folder entries</summary>
    Dictionary<string, CatalogEntry> _rootDirectoryCache;

    /// <summary>Cached root folder metadata</summary>
    HFSPlusCatalogFolder _rootFolder;

    /// <summary>Device sector size</summary>
    uint _sectorSize;

    /// <summary>HFS+ Volume Header</summary>
    VolumeHeader _volumeHeader;

    /// <inheritdoc />
    public FileSystem                                                Metadata         { get; set; }
    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description)> SupportedOptions { get; } = [];
    /// <inheritdoc />
    public Dictionary<string, string>                                Namespaces       { get; } = [];

#region IFilesystem Members

    /// <inheritdoc />
    public string Name => Localization.AppleHFSPlus_Name;

    /// <inheritdoc />
    public Guid Id => new("36405F8D-0D26-6EBE-436F-62F0586B4F08");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

#endregion
}