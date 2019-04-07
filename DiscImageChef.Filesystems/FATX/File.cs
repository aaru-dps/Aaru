// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : FATX filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Methods to handle files.
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
using DiscImageChef.CommonTypes.Structs;

namespace DiscImageChef.Filesystems.FATX
{
    public partial class XboxFatPlugin
    {
        public Errno MapBlock(string path, long fileBlock, out long deviceBlock)
        {
            deviceBlock = 0;
            if(!mounted) return Errno.AccessDenied;

            throw new NotImplementedException();
        }

        public Errno GetAttributes(string path, out FileAttributes attributes)
        {
            attributes = new FileAttributes();
            if(!mounted) return Errno.AccessDenied;

            throw new NotImplementedException();
        }

        public Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            if(!mounted) return Errno.AccessDenied;

            throw new NotImplementedException();
        }

        public Errno Stat(string path, out FileEntryInfo stat)
        {
            stat = null;
            if(!mounted) return Errno.AccessDenied;

            throw new NotImplementedException();
        }

        uint[] GetClusters(uint startCluster)
        {
            if(startCluster == 0) return null;

            if(fat16 is null)
            {
                if(startCluster >= fat32.Length) return null;
            }
            else if(startCluster >= fat16.Length) return null;

            List<uint> clusters = new List<uint>();

            uint nextCluster = startCluster;

            if(fat16 is null)
                while((nextCluster & FAT32_MASK) > 0 && (nextCluster & FAT32_MASK) <= FAT32_BAD)
                {
                    clusters.Add(nextCluster);
                    nextCluster = fat32[nextCluster];
                }
            else
                while(nextCluster > 0 && nextCluster <= FAT16_BAD)
                {
                    clusters.Add(nextCluster);
                    nextCluster = fat16[nextCluster];
                }

            return clusters.ToArray();
        }
    }
}