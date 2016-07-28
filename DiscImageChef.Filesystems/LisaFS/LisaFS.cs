// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;

// All information by Natalia Portillo
// Variable names from Lisa API
using System.Collections.Generic;
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.Filesystems.LisaFS
{
    partial class LisaFS : Filesystem
    {
        bool mounted;
        bool debug;
        readonly ImagePlugin device;

        MDDF mddf;
        ulong volumePrefix;
        int devTagSize;
        SRecord[] srecords;

        #region Caches
        Dictionary<Int16, ExtentFile> extentCache;
        Dictionary<Int16, byte[]> systemFileCache;
        Dictionary<Int16, byte[]> fileCache;
        Dictionary<Int16, List<CatalogEntry>> catalogCache;
        Dictionary<Int16, Int32> fileSizeCache;
        List<Int16> printedExtents;
        #endregion Caches

        public LisaFS()
        {
            Name = "Apple Lisa File System";
            PluginUUID = new Guid("7E6034D1-D823-4248-A54D-239742B28391");
        }

        public LisaFS(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            device = imagePlugin;
            Name = "Apple Lisa File System";
            PluginUUID = new Guid("7E6034D1-D823-4248-A54D-239742B28391");
        }
    }
}
