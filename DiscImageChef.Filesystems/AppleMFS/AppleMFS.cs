// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using DiscImageChef.CommonTypes.Interfaces;
using Schemas;

namespace DiscImageChef.Filesystems.AppleMFS
{
    // Information from Inside Macintosh Volume II
    public partial class AppleMFS : IReadOnlyFilesystem
    {
        bool        mounted;
        bool        debug;
        IMediaImage device;
        ulong       partitionStart;

        Dictionary<uint, string>        idToFilename;
        Dictionary<uint, MFS_FileEntry> idToEntry;
        Dictionary<string, uint>        filenameToId;

        MFS_MasterDirectoryBlock volMDB;
        byte[]                   bootBlocks;
        byte[]                   mdbBlocks;
        byte[]                   directoryBlocks;
        byte[]                   blockMapBytes;
        uint[]                   blockMap;
        int                      sectorsPerBlock;
        byte[]                   bootTags;
        byte[]                   mdbTags;
        byte[]                   directoryTags;
        byte[]                   bitmapTags;

        public FileSystemType XmlFsType { get; private set; }
        public string         Name      => "Apple Macintosh File System";
        public Guid           Id        => new Guid("36405F8D-0D26-4066-6538-5DBF5D065C3A");
        public Encoding       Encoding  { get; private set; }
        public string         Author    => "Natalia Portillo";

        // TODO: Implement Finder namespace (requires decoding Desktop database)
        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new (string name, Type type, string description)[] { };

        public Dictionary<string, string> Namespaces => null;

        static Dictionary<string, string> GetDefaultOptions() =>
            new Dictionary<string, string> {{"debug", false.ToString()}};
    }
}