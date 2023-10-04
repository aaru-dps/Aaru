// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AppleMFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Macintosh File System plugin.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Filesystems;

// Information from Inside Macintosh Volume II
/// <inheritdoc />
/// <summary>Implements the Apple Macintosh File System</summary>
public sealed partial class AppleMFS : IReadOnlyFilesystem
{
    const string                MODULE_NAME = "Apple MFS plugin";
    byte[]                      _bitmapTags;
    uint[]                      _blockMap;
    byte[]                      _blockMapBytes;
    byte[]                      _bootBlocks;
    byte[]                      _bootTags;
    bool                        _debug;
    IMediaImage                 _device;
    byte[]                      _directoryBlocks;
    byte[]                      _directoryTags;
    Encoding                    _encoding;
    Dictionary<string, uint>    _filenameToId;
    Dictionary<uint, FileEntry> _idToEntry;
    Dictionary<uint, string>    _idToFilename;
    byte[]                      _mdbBlocks;
    byte[]                      _mdbTags;
    bool                        _mounted;
    ulong                       _partitionStart;
    int                         _sectorsPerBlock;
    MasterDirectoryBlock        _volMdb;

#region IReadOnlyFilesystem Members

    /// <inheritdoc />
    public string Name => Localization.AppleMFS_Name;

    /// <inheritdoc />
    public FileSystem Metadata { get; private set; }

    /// <inheritdoc />
    public Guid Id => new("36405F8D-0D26-4066-6538-5DBF5D065C3A");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    // TODO: Implement Finder namespace (requires decoding Desktop database)
    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
        Array.Empty<(string name, Type type, string description)>();

    /// <inheritdoc />
    public Dictionary<string, string> Namespaces => null;

#endregion

    static Dictionary<string, string> GetDefaultOptions() => new()
    {
        {
            "debug", false.ToString()
        }
    };
}