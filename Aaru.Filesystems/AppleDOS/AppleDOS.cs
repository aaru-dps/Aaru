// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AppleDOS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple DOS filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Constructors and common variables for the Apple DOS filesystem plugin.
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
// ****************************************************************************/

namespace Aaru.Filesystems;

using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.Interfaces;
using Schemas;

/// <inheritdoc />
/// <summary>Implements the Apple DOS 3 filesystem</summary>
public sealed partial class AppleDOS : IReadOnlyFilesystem
{
    bool        _debug;
    IMediaImage _device;
    bool        _mounted;
    int         _sectorsPerTrack;
    ulong       _start;
    ulong       _totalFileEntries;
    bool        _track1UsedByFiles;
    bool        _track2UsedByFiles;
    uint        _usedSectors;
    Vtoc        _vtoc;

    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => "Apple DOS File System";
    /// <inheritdoc />
    public Guid Id => new("8658A1E9-B2E7-4BCC-9638-157A31B0A700\n");
    /// <inheritdoc />
    public string Author => "Natalia Portillo";

    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
        Array.Empty<(string name, Type type, string description)>();

    /// <inheritdoc />
    public Dictionary<string, string> Namespaces => null;

    static Dictionary<string, string> GetDefaultOptions() => new()
    {
        {
            "debug", false.ToString()
        }
    };

    #region Caches
    /// <summary>Caches track/sector lists</summary>
    Dictionary<string, byte[]> _extentCache;
    /// <summary>Caches files</summary>
    Dictionary<string, byte[]> _fileCache;
    /// <summary>Caches catalog</summary>
    Dictionary<string, ushort> _catalogCache;
    /// <summary>Caches file size</summary>
    Dictionary<string, int> _fileSizeCache;
    /// <summary>Caches VTOC</summary>
    byte[] _vtocBlocks;
    /// <summary>Caches catalog</summary>
    byte[] _catalogBlocks;
    /// <summary>Caches boot code</summary>
    byte[] _bootBlocks;
    /// <summary>Caches file type</summary>
    Dictionary<string, byte> _fileTypeCache;
    /// <summary>Caches locked files</summary>
    List<string> _lockedFiles;
    #endregion Caches
}