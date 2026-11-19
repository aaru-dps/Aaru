// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MDFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : MiniDisc File System plugin.
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

// Reverse engineered from Sony's MD DATA drivers for DOS (MDFSEX.EXE, MDFSCK.EXE), Windows 95 (MDFSD.VXD) and Mac OS
// (MD File System), and verified against volumes written by all of them.
/// <inheritdoc />
/// <summary>Implements Sony's MiniDisc File System, used on MD DATA discs</summary>
public sealed partial class MDFS : IReadOnlyFilesystem
{
    const string MODULE_NAME = "MDFS Plugin";

    /// <summary>Sectors per allocation unit</summary>
    uint _allocSize;
    /// <summary>Children of each directory, in on-disk order, keyed by directory path without leading slash</summary>
    Dictionary<string, List<string>> _children;
    /// <summary>Use 8.3 names instead of long names</summary>
    bool _dosNames;
    /// <summary>Encoding requested by the user for Windows and DOS names, <c>null</c> to use the defaults</summary>
    Encoding _encoding;
    /// <summary>All entries, keyed by path without leading slash</summary>
    Dictionary<string, Entry> _entries;
    IMediaImage _image;
    /// <summary>Management table, one entry per sector of the volume management area</summary>
    uint[] _mtb;
    bool      _mounted;
    Partition _partition;
    Entry     _root;
    /// <summary>Device sectors per 2048-byte logical block</summary>
    uint _sectorsPerBlock;
    FileSystemInfo   _statfs;
    VolumeDescriptor _vd;

    /// <inheritdoc />
    public FileSystem Metadata { get; private set; }

    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description)> SupportedOptions => [];

    /// <inheritdoc />
    public Dictionary<string, string> Namespaces => new()
    {
        {
            "long", Localization.MDFS_Long_names_namespace
        },
        {
            "dos", Localization.MDFS_DOS_names_namespace
        }
    };

#region Nested type: Entry

    /// <summary>A file or directory, as decoded from its directory record</summary>
    sealed class Entry
    {
        /// <summary>Attributes</summary>
        public ushort Attributes;
        /// <summary>Creation time</summary>
        public uint CreationTime;
        /// <summary>For directories, first sector of its records, relative to the volume management area</summary>
        public ushort DirectoryLocation;
        /// <summary>Data fork length in bytes, or directory length in bytes</summary>
        public uint Length;
        /// <summary>Data fork allocation units</summary>
        public uint DataCount;
        /// <summary>Data fork first allocation unit or extent record id</summary>
        public uint DataLocation;
        /// <summary>Finder information, 32 bytes, if the entry was written by Mac OS</summary>
        public byte[] FinderInfo;
        /// <summary>Record id</summary>
        public uint Id;
        /// <summary>Modification time</summary>
        public uint ModificationTime;
        /// <summary>Resource fork allocation units</summary>
        public uint ResourceCount;
        /// <summary>Resource fork length in bytes</summary>
        public uint ResourceLength;
        /// <summary>Resource fork first allocation unit or extent record id</summary>
        public uint ResourceLocation;
        /// <summary>Status change time</summary>
        public uint StatusTime;
        /// <summary>Is this a directory?</summary>
        public bool IsDirectory => (Attributes & ATTR_DIRECTORY) != 0;
    }

#endregion

#region Nested type: Extent

    /// <summary>A run of contiguous allocation units</summary>
    readonly struct Extent(uint start, uint count)
    {
        /// <summary>First allocation unit</summary>
        public readonly uint Start = start;
        /// <summary>Number of allocation units</summary>
        public readonly uint Count = count;
    }

#endregion

#region Nested type: MdfsDirNode

    sealed class MdfsDirNode : IDirNode
    {
        internal string[] Contents;
        internal int      Position;

        /// <inheritdoc />
        public string Path { get; init; }
    }

#endregion

#region Nested type: MdfsFileNode

    sealed class MdfsFileNode : IFileNode
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
    public string Name => Localization.MDFS_Name;

    /// <inheritdoc />
    public Guid Id => new("5bfb7c2e-4f2f-4c38-9d0a-6c2f8b7a9e31");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

#endregion
}