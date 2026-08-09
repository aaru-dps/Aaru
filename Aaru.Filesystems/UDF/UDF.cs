// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : UDF.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Universal Disk Format plugin.
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
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the Universal Disk Format filesystem</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class UDF : IReadOnlyFilesystem
{
    const string MODULE_NAME = "UDF Plugin";

    Dictionary<string, Dictionary<string, UdfDirectoryEntry>> _directoryCache;
    IMediaImage                                               _imagePlugin;
    bool                                                      _mounted;
    bool                                                      _mrw;
    uint                                                      _partitionStartingLocation;
    Dictionary<string, UdfDirectoryEntry>                     _rootDirectoryCache;
    LongAllocationDescriptor                                  _rootDirectoryIcb;
    uint                                                      _sectorSize;
    FileSystemInfo                                            _statfs;

    // UDF 1.50+ fields
    ushort                     _udfVersion;
    uint[]                     _vat;          // Virtual Allocation Table for virtual partitions
    Dictionary<uint, uint>     _sparingTable; // Sparing table for sparable partitions (original -> mapped)
    Dictionary<ushort, ushort> _partitionMap; // Maps logical partition numbers to physical partition numbers
    bool                       _hasVirtualPartition;
    bool                       _hasSparablePartition;
    ushort                     _virtualPartitionNumber;
    ushort                     _sparablePartitionNumber;
    uint                       _sparablePacketLength;

    // UDF 2.50+ metadata partition fields
    bool   _hasMetadataPartition;
    ushort _metadataPartitionNumber;
    uint   _metadataFileLocation;
    uint   _metadataMirrorFileLocation;
    uint   _metadataAllocationUnitSize;
    byte[] _metadataFileData; // Cached metadata file content

#region IFilesystem Members

    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description)> SupportedOptions => [];

    /// <inheritdoc />
    public FileSystem Metadata { get; private set; }

    /// <inheritdoc />
    public Dictionary<string, string> Namespaces => null;

    /// <inheritdoc />
    public string Name => Localization.UDF_Name;

    /// <inheritdoc />
    public Guid Id => new("83976FEC-A91B-464B-9293-56C719461BAB");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

#endregion
}