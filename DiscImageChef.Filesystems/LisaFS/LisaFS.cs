/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : LisaFS.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Filesystem plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Identifies Apple Lisa filesystems and shows information.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$

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
