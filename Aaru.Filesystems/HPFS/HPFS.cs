// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HPFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : OS/2 High Performance File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the OS/2 High Performance File System and shows information.
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

// Information from an old unnamed document
/// <inheritdoc />
/// <summary>Implements IBM's High Performance File System (HPFS)</summary>
public sealed partial class HPFS : IReadOnlyFilesystem
{
    const string                               MODULE_NAME = "HPFS";
    BiosParameterBlock                         _bpb;
    uint                                       _bytesPerSector;
    byte[]                                     _codePageTable;
    bool                                       _debug;
    Dictionary<uint, DNode>                    _dnodeCache;
    Dictionary<uint, Dictionary<string, uint>> _directoryCache;
    Encoding                                   _encoding;
    Dictionary<uint, FNode>                    _fnodeCache;
    IMediaImage                                _image;
    bool                                       _mounted;
    Partition                                  _partition;
    Dictionary<string, uint>                   _rootDirectoryCache;
    uint                                       _rootDnode;
    uint                                       _rootFnode;
    SpareBlock                                 _spareblock;
    FileSystemInfo                             _statfs;
    SuperBlock                                 _superblock;

    /// <inheritdoc />
    public FileSystem                                                Metadata         { get; private set; }
    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description)> SupportedOptions { get; } = [];
    /// <inheritdoc />
    public Dictionary<string, string>                                Namespaces       { get; } = [];

    static Dictionary<string, string> GetDefaultOptions() => new()
    {
        {
            "debug", false.ToString()
        }
    };

#region IFilesystem Members

    /// <inheritdoc />
    public string Name => Localization.HPFS_Name;

    /// <inheritdoc />
    public Guid Id => new("33513B2C-f590-4acb-8bf2-0b1d5e19dec5");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

#endregion
}