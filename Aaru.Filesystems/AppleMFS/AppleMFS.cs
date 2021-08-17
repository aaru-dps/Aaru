// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AppleMFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Macintosh File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Constructors and common variables for the Apple Macintosh File System plugin.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.Interfaces;
using Schemas;

namespace Aaru.Filesystems
{
    // Information from Inside Macintosh Volume II
    /// <inheritdoc />
    /// <summary>
    /// Implements the Apple Macintosh File System
    /// </summary>
    public sealed partial class AppleMFS : IReadOnlyFilesystem
    {
        bool                        _mounted;
        bool                        _debug;
        IMediaImage                 _device;
        ulong                       _partitionStart;
        Dictionary<uint, string>    _idToFilename;
        Dictionary<uint, FileEntry> _idToEntry;
        Dictionary<string, uint>    _filenameToId;
        MasterDirectoryBlock        _volMdb;
        byte[]                      _bootBlocks;
        byte[]                      _mdbBlocks;
        byte[]                      _directoryBlocks;
        byte[]                      _blockMapBytes;
        uint[]                      _blockMap;
        int                         _sectorsPerBlock;
        byte[]                      _bootTags;
        byte[]                      _mdbTags;
        byte[]                      _directoryTags;
        byte[]                      _bitmapTags;

        /// <inheritdoc />
        public FileSystemType XmlFsType { get; private set; }
        /// <inheritdoc />
        public string         Name      => "Apple Macintosh File System";
        /// <inheritdoc />
        public Guid           Id        => new Guid("36405F8D-0D26-4066-6538-5DBF5D065C3A");
        /// <inheritdoc />
        public Encoding       Encoding  { get; private set; }
        /// <inheritdoc />
        public string         Author    => "Natalia Portillo";

        // TODO: Implement Finder namespace (requires decoding Desktop database)
        /// <inheritdoc />
        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new (string name, Type type, string description)[]
                {};

        /// <inheritdoc />
        public Dictionary<string, string> Namespaces => null;

        static Dictionary<string, string> GetDefaultOptions() => new Dictionary<string, string>
        {
            {
                "debug", false.ToString()
            }
        };
    }
}