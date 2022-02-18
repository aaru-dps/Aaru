// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : UCSDPascal.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : U.C.S.D. Pascal filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Constructors and common variables for the U.C.S.D. Pascal filesystem plugin.
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

using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Schemas;

namespace Aaru.Filesystems.UCSDPascal
{
    // Information from Call-A.P.P.L.E. Pascal Disk Directory Structure
    /// <inheritdoc />
    /// <summary>Implements the U.C.S.D. Pascal filesystem</summary>
    public sealed partial class PascalPlugin : IReadOnlyFilesystem
    {
        byte[]                _bootBlocks;
        byte[]                _catalogBlocks;
        bool                  _debug;
        IMediaImage           _device;
        List<PascalFileEntry> _fileEntries;
        bool                  _mounted;
        PascalVolumeEntry     _mountedVolEntry;
        /// <summary>Apple II disks use 256 bytes / sector, but filesystem assumes it's 512 bytes / sector</summary>
        uint _multiplier;

        /// <inheritdoc />
        public FileSystemType XmlFsType { get; private set; }
        /// <inheritdoc />
        public string Name => "U.C.S.D. Pascal filesystem";
        /// <inheritdoc />
        public Guid Id => new("B0AC2CB5-72AA-473A-9200-270B5A2C2D53");
        /// <inheritdoc />
        public Encoding Encoding { get; private set; }
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

        /// <inheritdoc />
        public ErrorNumber ListXAttr(string path, out List<string> xattrs)
        {
            xattrs = null;

            return ErrorNumber.NotSupported;
        }

        /// <inheritdoc />
        public ErrorNumber GetXattr(string path, string xattr, ref byte[] buf) => ErrorNumber.NotSupported;

        /// <inheritdoc />
        public ErrorNumber ReadLink(string path, out string dest)
        {
            dest = null;

            return ErrorNumber.NotSupported;
        }

        /// <inheritdoc />
        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new (string name, Type type, string description)[]
                {};

        /// <inheritdoc />
        public Dictionary<string, string> Namespaces => null;

        static Dictionary<string, string> GetDefaultOptions() => new()
        {
            {
                "debug", false.ToString()
            }
        };
    }
}