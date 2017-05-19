// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : FATX.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Component
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the FATX filesystem and shows information.
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace DiscImageChef.Filesystems
{
    class FATX : Filesystem
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct FATX_Superblock
        {
            public uint magic;
            public uint id;
            public uint sectorsPerCluster;
            public uint rootDirectoryCluster;
        }

        const uint FATX_Magic = 0x58544146;

        public FATX()
        {
            Name = "FATX Filesystem Plugin";
            PluginUUID = new Guid("ED27A721-4A17-4649-89FD-33633B46E228");
        }

        public FATX(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            Name = "FATX Filesystem Plugin";
            PluginUUID = new Guid("ED27A721-4A17-4649-89FD-33633B46E228");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            if(imagePlugin.GetSectorSize() < 512)
                return false;

            FATX_Superblock fatxSb = new FATX_Superblock();
            byte[] sector = imagePlugin.ReadSector(partitionStart);

            fatxSb = BigEndianMarshal.ByteArrayToStructureBigEndian<FATX_Superblock>(sector);

            return fatxSb.magic == FATX_Magic;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd, out string information)
        {
            information = "";
            if(imagePlugin.GetSectorSize() < 512)
                return;

            FATX_Superblock fatxSb = new FATX_Superblock();

            byte[] sector = imagePlugin.ReadSector(partitionStart);

            fatxSb = BigEndianMarshal.ByteArrayToStructureBigEndian<FATX_Superblock>(sector);

            if(fatxSb.magic != FATX_Magic)
                return;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("FATX filesystem");
            sb.AppendFormat("Filesystem id {0}", fatxSb.id).AppendLine();
            sb.AppendFormat("{0} sectors ({1} bytes) per cluster", fatxSb.sectorsPerCluster, fatxSb.sectorsPerCluster * imagePlugin.ImageInfo.sectorSize).AppendLine();
            sb.AppendFormat("Root directory starts on cluster {0}", fatxSb.rootDirectoryCluster).AppendLine();

            information = sb.ToString();

            xmlFSType = new Schemas.FileSystemType();
            xmlFSType.Type = "FATX filesystem";
            xmlFSType.ClusterSize = (int)(fatxSb.sectorsPerCluster * imagePlugin.ImageInfo.sectorSize);
            xmlFSType.Clusters = (long)((partitionEnd - partitionStart + 1) * imagePlugin.ImageInfo.sectorSize / (ulong)xmlFSType.ClusterSize);
        }

        public override Errno Mount()
        {
            return Errno.NotImplemented;
        }

        public override Errno Mount(bool debug)
        {
            return Errno.NotImplemented;
        }

        public override Errno Unmount()
        {
            return Errno.NotImplemented;
        }

        public override Errno MapBlock(string path, long fileBlock, ref long deviceBlock)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetAttributes(string path, ref FileAttributes attributes)
        {
            return Errno.NotImplemented;
        }

        public override Errno ListXAttr(string path, ref List<string> xattrs)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadDir(string path, ref List<string> contents)
        {
            return Errno.NotImplemented;
        }

        public override Errno StatFs(ref FileSystemInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno Stat(string path, ref FileEntryInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadLink(string path, ref string dest)
        {
            return Errno.NotImplemented;
        }
    }
}