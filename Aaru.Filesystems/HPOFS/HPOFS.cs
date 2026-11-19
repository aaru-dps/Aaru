// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HPOFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : High Performance Optical File System plugin.
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

// Reverse engineered from OS/2 HPOFS20.IFS and UHPOFS.DLL, and verified against volumes written by them.
// IBM document GA32-0224 (IBM 3995 Optical Library Dataserver Products: Optical Disk Format) describes the format.
/// <inheritdoc />
/// <summary>Implements IBM's High Performance Optical File System</summary>
public sealed partial class HPOFS : IReadOnlyFilesystem
{
    const string MODULE_NAME = "HPOFS Plugin";

    BiosParameterBlock _bpb;
    uint               _bytesPerSector;
    /// <summary>Children of each directory, in on-disk (key) order, keyed by directory path without leading slash</summary>
    Dictionary<string, List<string>> _children;
    /// <summary>All entries, keyed by path without leading slash</summary>
    Dictionary<string, Entry> _entries;
    Encoding                  _encoding;
    IMediaImage               _image;
    MediaInformationBlock     _medInfo;
    bool                      _mounted;
    Partition                 _partition;
    Entry                     _root;
    FileSystemInfo            _statfs;
    VolumeInformationBlock    _volInfo;

    /// <inheritdoc />
    public FileSystem Metadata { get; private set; }

    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description)> SupportedOptions => [];

    /// <inheritdoc />
    public Dictionary<string, string> Namespaces => [];

#region Nested type: Entry

    /// <summary>A file or directory, as decoded from its directory key and file record</summary>
    sealed class Entry
    {
        /// <summary>Decoded file record header</summary>
        public FileRecord Record;
        /// <summary>Subfile descriptors present in the record</summary>
        public SubfileDescriptor[] Subfiles;
        /// <summary>Version of the entry, on WORM volumes, counting down from 0xFFFFFFFE</summary>
        public uint Version;
        /// <summary>Is this a directory?</summary>
        public bool IsDirectory => (Record.attributes & 0x10) != 0;
    }

#endregion

#region Nested type: Extent

    /// <summary>A run of contiguous sectors</summary>
    readonly struct Extent(ulong lba, uint sectors)
    {
        /// <summary>First sector, relative to the partition</summary>
        public readonly ulong Lba = lba;
        /// <summary>Number of sectors</summary>
        public readonly uint Sectors = sectors;
    }

#endregion

#region Nested type: HpofsDirNode

    sealed class HpofsDirNode : IDirNode
    {
        internal string[] Contents;
        internal int      Position;

        /// <inheritdoc />
        public string Path { get; init; }
    }

#endregion

#region Nested type: HpofsFileNode

    sealed class HpofsFileNode : IFileNode
    {
        internal Extent[] Extents;

        /// <inheritdoc />
        public string Path { get; init; }

        /// <inheritdoc />
        public long Length { get; init; }

        /// <inheritdoc />
        public long Offset { get; set; }
    }

#endregion

#region IFilesystem Members

    /// <inheritdoc />
    public string Name => Localization.HPOFS_Name;

    /// <inheritdoc />
    public Guid Id => new("1b72dcd5-d031-4757-8a9f-8d2fb18c59e2");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

#endregion
}