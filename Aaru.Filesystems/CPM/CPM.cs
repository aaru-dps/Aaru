// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CPM.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CP/M filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Constructors and common variables for the CP/M filesystem plugin.
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
using Aaru.CommonTypes.Structs;
using Schemas;

namespace Aaru.Filesystems
{
    /// <inheritdoc />
    /// <summary>Implements the CP/M filesystem</summary>
    public sealed partial class CPM : IReadOnlyFilesystem
    {
        /// <summary>True if <see cref="Identify" /> thinks this is a CP/M filesystem</summary>
        bool _cpmFound;

        /// <summary>Cached <see cref="FileSystemInfo" /></summary>
        FileSystemInfo _cpmStat;

        /// <summary>Cached file passwords, decoded</summary>
        Dictionary<string, byte[]> _decodedPasswordCache;

        /// <summary>Stores all known CP/M disk definitions</summary>
        CpmDefinitions _definitions;
        IMediaImage _device;
        /// <summary>Cached directory listing</summary>
        List<string> _dirList;
        /// <summary>CP/M disc parameter block (on-memory)</summary>
        DiscParameterBlock _dpb;
        /// <summary>Cached file data</summary>
        Dictionary<string, byte[]> _fileCache;
        /// <summary>The volume label, if the CP/M filesystem contains one</summary>
        string _label;
        /// <summary>Timestamp in volume label for creation</summary>
        byte[] _labelCreationDate;
        /// <summary>Timestamp in volume label for update</summary>
        byte[] _labelUpdateDate;
        bool _mounted;
        /// <summary>Cached file passwords</summary>
        Dictionary<string, byte[]> _passwordCache;
        /// <summary>Sector deinterleaving mask</summary>
        int[] _sectorMask;
        /// <summary>True if there are CP/M 3 timestamps</summary>
        bool _standardTimestamps;
        /// <summary>Cached file <see cref="FileEntryInfo" /></summary>
        Dictionary<string, FileEntryInfo> _statCache;
        /// <summary>True if there are timestamps in Z80DOS or DOS+ format</summary>
        bool _thirdPartyTimestamps;
        /// <summary>If <see cref="Identify" /> thinks this is a CP/M filesystem, this is the definition for it</summary>
        CpmDefinition _workingDefinition;

        /// <inheritdoc />
        public FileSystemType XmlFsType { get; private set; }
        /// <inheritdoc />
        public Encoding Encoding { get; private set; }
        /// <inheritdoc />
        public string Name => "CP/M File System";
        /// <inheritdoc />
        public Guid Id => new Guid("AA2B8585-41DF-4E3B-8A35-D1A935E2F8A1");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

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