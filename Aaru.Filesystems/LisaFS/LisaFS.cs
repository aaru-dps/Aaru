// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : LisaFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Lisa filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Constructors and common variables for the Apple Lisa filesystem plugin.
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

namespace Aaru.Filesystems.LisaFS
{
    // All information by Natalia Portillo
    // Variable names from Lisa API
    /// <inheritdoc />
    /// <summary>
    /// Implements the Apple Lisa File System
    /// </summary>
    public sealed partial class LisaFS : IReadOnlyFilesystem
    {
        bool        _debug;
        IMediaImage _device;
        int         _devTagSize;
        MDDF        _mddf;
        bool        _mounted;
        SRecord[]   _srecords;
        ulong       _volumePrefix;

        /// <inheritdoc />
        public string         Name      => "Apple Lisa File System";
        /// <inheritdoc />
        public Guid           Id        => new Guid("7E6034D1-D823-4248-A54D-239742B28391");
        /// <inheritdoc />
        public Encoding       Encoding  { get; private set; }
        /// <inheritdoc />
        public FileSystemType XmlFsType { get; private set; }
        /// <inheritdoc />
        public string         Author    => "Natalia Portillo";

        // TODO: Implement Lisa 7/7 namespace (needs decoding {!CATALOG} file)
        /// <inheritdoc />
        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new (string name, Type type, string description)[]
                {};

        /// <inheritdoc />
        public Dictionary<string, string> Namespaces => new Dictionary<string, string>
        {
            {
                "workshop", "Filenames as shown by the Lisa Pascal Workshop (default)"
            },
            {
                "office", "Filenames as shown by the Lisa Office System (not yet implemented)"
            }
        };

        static Dictionary<string, string> GetDefaultOptions() => new Dictionary<string, string>
        {
            {
                "debug", false.ToString()
            }
        };

        #region Caches
        /// <summary>Caches Extents Files</summary>
        Dictionary<short, ExtentFile> _extentCache;
        /// <summary>Caches system files</summary>
        Dictionary<short, byte[]> _systemFileCache;
        /// <summary>Caches user files files</summary>
        Dictionary<short, byte[]> _fileCache;
        /// <summary>Caches catalogs</summary>
        List<CatalogEntry> _catalogCache;
        /// <summary>Caches file size</summary>
        Dictionary<short, int> _fileSizeCache;
        /// <summary>Lists Extents Files already printed in debug mode to not repeat them</summary>
        List<short> _printedExtents;
        /// <summary>Caches the creation times for subdirectories as to not have to traverse the Catalog File on each stat</summary>
        Dictionary<short, DateTime> _directoryDtcCache;
        #endregion Caches
    }
}