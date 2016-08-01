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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.Filesystems.AppleMFS
{
    // Information from Inside Macintosh Volume II
    partial class AppleMFS : Filesystem
    {
        bool mounted;
        bool debug;
        ImagePlugin device;
        ulong partitionStart;

        Dictionary<uint, string> idToFilename;
        Dictionary<uint, MFS_FileEntry> idToEntry;
        Dictionary<string, uint> filenameToId;

        MFS_MasterDirectoryBlock volMDB;
        byte[] bootBlocks;
        byte[] mdbBlocks;
        byte[] directoryBlocks;
        byte[] blockMapBytes;
        uint[] blockMap;
        int sectorsPerBlock;
        byte[] bootTags;
        byte[] mdbTags;
        byte[] directoryTags;
        byte[] bitmapTags;

        public AppleMFS()
        {
            Name = "Apple Macintosh File System";
            PluginUUID = new Guid("36405F8D-0D26-4066-6538-5DBF5D065C3A");
        }

        public AppleMFS(ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            Name = "Apple Macintosh File System";
            PluginUUID = new Guid("36405F8D-0D26-4066-6538-5DBF5D065C3A");
            device = imagePlugin;
            this.partitionStart = partitionStart;
        }
    }
}
