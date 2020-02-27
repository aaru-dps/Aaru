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
// Copyright © 2011-2020 Natalia Portillo
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Schemas;

namespace Aaru.Filesystems.ISO9660
{
    // This is coded following ECMA-119.
    public partial class ISO9660 : IReadOnlyFilesystem
    {
        bool                                      cdi;
        bool                                      debug;
        bool                                      highSierra;
        IMediaImage                               image;
        bool                                      joliet;
        bool                                      mounted;
        Namespace                                 @namespace;
        PathTableEntryInternal[]                  pathTable;
        Dictionary<string, DecodedDirectoryEntry> rootDirectoryCache;
        FileSystemInfo                            statfs;
        bool                                      useEvd;
        bool                                      usePathTable;
        bool                                      useTransTbl;

        public FileSystemType XmlFsType { get; private set; }
        public Encoding       Encoding  { get; private set; }
        public string         Name      => "ISO9660 Filesystem";
        public Guid           Id        => new Guid("d812f4d3-c357-400d-90fd-3b22ef786aa8");
        public string         Author    => "Natalia Portillo";

        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new (string name, Type type, string description)[]
            {
                ("use_path_table", typeof(bool), "Use path table for directory traversal"),
                ("use_trans_tbl", typeof(bool), "Use TRANS.TBL for filenames"),
                ("use_evd", typeof(bool),
                 "If present, use Enhanced Volume Descriptor with specified encoding (overrides namespace)")
            };

        public Dictionary<string, string> Namespaces =>
            new Dictionary<string, string>
            {
                {"normal", "Primary Volume Descriptor, ignoring ;1 suffixes"},
                {"vms", "Primary Volume Descriptor, showing version suffixes"},
                {"joliet", "Joliet Volume Descriptor (default)"},
                {"rrip", "Rock Ridge"},
                {"romeo", "Primary Volume Descriptor using the specified encoding codepage"}
            };

        static Dictionary<string, string> GetDefaultOptions() =>
            new Dictionary<string, string> {{"debug", false.ToString()}};
    }
}