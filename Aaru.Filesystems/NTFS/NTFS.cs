// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : NTFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft NT File System plugin.
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
using Aaru.CommonTypes.Structs;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

// Information from Inside Windows NT and the Linux kernel NTFS driver (fs/ntfs)
/// <inheritdoc />
/// <summary>Implements the New Technology File System (NTFS)</summary>
public sealed partial class NTFS : IReadOnlyFilesystem
{
    const string MODULE_NAME = "NTFS";

    Dictionary<AttributeType, AttrDef> _attributeDefinitions;
    BiosParameterBlock                 _bpb;
    uint                               _bytesPerCluster;
    uint                               _bytesPerSector;
    bool                               _debug;
    Encoding                           _encoding;
    IMediaImage                        _image;
    uint                               _indexBlockSize;
    List<(long offset, long length)>   _mftDataRuns;
    uint                               _mftRecordSize;
    bool                               _mounted;
    byte                               _ntfsMajorVersion;
    byte                               _ntfsMinorVersion;
    string                             _ntfsVersion;
    Partition                          _partition;
    Dictionary<string, ulong>          _rootDirectoryCache;

    /// <summary>
    ///     Cached directory contents (MFT record number -> entries). Read-only filesystem, so entries never go
    ///     stale; without this every Stat/OpenFile re-reads and re-parses directory indexes per path component,
    ///     which is quadratic on huge or deeply nested directories.
    /// </summary>
    readonly Dictionary<uint, Dictionary<string, ulong>> _directoryCache = new();
    uint                                                 _sectorsPerCluster;
    Dictionary<uint, byte[]>                             _securityDescriptors;
    FileSystemInfo                                       _statfs;

    /// <inheritdoc />
    public FileSystem                                                Metadata         { get; private set; }
    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description)> SupportedOptions => [];
    /// <inheritdoc />
    public Dictionary<string, string>                                Namespaces       => [];

    static Dictionary<string, string> GetDefaultOptions() => new()
    {
        {
            "debug", false.ToString()
        }
    };

#region IFilesystem Members

    /// <inheritdoc />
    public string Name => Localization.NTFS_Name;

    /// <inheritdoc />
    public Guid Id => new("33513B2C-1e6d-4d21-a660-0bbc789c3871");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

#endregion
}