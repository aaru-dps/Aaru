// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : FAT.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft FAT filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Microsoft FAT filesystem and shows information.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using Schemas;

namespace DiscImageChef.Filesystems.FAT
{
    // TODO: Differentiate between Atari and X68k FAT, as this one uses a standard BPB.
    // X68K uses cdate/adate from direntry for extending filename
    public partial class FAT : IReadOnlyFilesystem
    {
        uint                                                   bytesPerCluster;
        byte[]                                                 cachedEaData;
        CultureInfo                                            cultureInfo;
        bool                                                   debug;
        Dictionary<string, Dictionary<string, DirectoryEntry>> directoryCache;
        DirectoryEntry                                         eaDirEntry;
        bool                                                   fat12;
        bool                                                   fat16;
        bool                                                   fat32;
        ushort[]                                               fatEntries;
        ulong                                                  fatFirstSector;
        ulong                                                  firstClusterSector;
        bool                                                   mounted;
        Namespace                                              @namespace;
        uint                                                   reservedSectors;
        Dictionary<string, DirectoryEntry>                     rootDirectoryCache;
        uint                                                   sectorsPerCluster;
        uint                                                   sectorsPerFat;
        FileSystemInfo                                         statfs;
        bool                                                   useFirstFat;

        public FileSystemType XmlFsType { get; private set; }

        public Encoding Encoding { get; private set; }
        public string   Name     => "Microsoft File Allocation Table";
        public Guid     Id       => new Guid("33513B2C-0D26-0D2D-32C3-79D8611158E0");
        public string   Author   => "Natalia Portillo";

        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new (string name, Type type, string description)[] { };

        public Dictionary<string, string> Namespaces =>
            new Dictionary<string, string>
            {
                {"dos", "DOS (8.3 all uppercase)"},
                {"nt", "Windows NT (8.3 mixed case)"},
                {"lfn", "Long file names (default)"}
            };

        static Dictionary<string, string> GetDefaultOptions() =>
            new Dictionary<string, string> {{"debug", false.ToString()}};
    }
}