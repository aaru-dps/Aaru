// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ISO9660.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Constructors and common variables for the ISO9660 filesystem plugin.
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
// Copyright © 2011-2022 Natalia Portillo
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Schemas;

namespace Aaru.Filesystems;

// This is coded following ECMA-119.
/// <inheritdoc />
/// <summary>Implements the High Sierra, ISO9660 and CD-i filesystems</summary>
[SuppressMessage("ReSharper", "UnusedType.Local")]
public sealed partial class ISO9660 : IReadOnlyFilesystem
{
    bool                                      _cdi;
    bool                                      _debug;
    bool                                      _highSierra;
    IMediaImage                               _image;
    bool                                      _joliet;
    bool                                      _mounted;
    Namespace                                 _namespace;
    PathTableEntryInternal[]                  _pathTable;
    Dictionary<string, DecodedDirectoryEntry> _rootDirectoryCache;
    FileSystemInfo                            _statfs;
    bool                                      _useEvd;
    bool                                      _usePathTable;
    bool                                      _useTransTbl;
    ushort                                    _blockSize;

    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => "ISO9660 Filesystem";
    /// <inheritdoc />
    public Guid Id => new("d812f4d3-c357-400d-90fd-3b22ef786aa8");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
        new (string name, Type type, string description)[]
        {
            ("use_path_table", typeof(bool), "Use path table for directory traversal"),
            ("use_trans_tbl", typeof(bool), "Use TRANS.TBL for filenames"),
            ("use_evd", typeof(bool),
             "If present, use Enhanced Volume Descriptor with specified encoding (overrides namespace)")
        };

    /// <inheritdoc />
    public Dictionary<string, string> Namespaces => new()
    {
        {
            "normal", "Primary Volume Descriptor, ignoring ;1 suffixes"
        },
        {
            "vms", "Primary Volume Descriptor, showing version suffixes"
        },
        {
            "joliet", "Joliet Volume Descriptor (default)"
        },
        {
            "rrip", "Rock Ridge"
        },
        {
            "romeo", "Primary Volume Descriptor using the specified encoding codepage"
        }
    };

    static Dictionary<string, string> GetDefaultOptions() => new()
    {
        {
            "debug", false.ToString()
        }
    };
}