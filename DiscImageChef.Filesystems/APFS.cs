// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : APFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Apple filesystem and shows information.
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
using DiscImageChef.CommonTypes;

namespace DiscImageChef.Filesystems
{
    public class APFS : Filesystem
    {
        const uint ApfsContainerMagic = 0x4253584E; // "NXSB"
        const uint ApfsVolumeMagic = 0x42535041; // "APSB"

        public APFS()
        {
            Name = "Apple File System";
            PluginUUID = new Guid("A4060F9D-2909-42E2-9D95-DB31FA7EA797");
            CurrentEncoding = Encoding.UTF8;
        }

        public APFS(ImagePlugins.ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "Apple File System";
            PluginUUID = new Guid("A4060F9D-2909-42E2-9D95-DB31FA7EA797");
            CurrentEncoding = Encoding.UTF8;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ApfsContainerSuperBlock
        {
            public ulong unknown1; // Varies between copies of the superblock
            public ulong unknown2;
            public ulong unknown3; // Varies by 1 between copies of the superblock
            public ulong unknown4;
            public uint magic;
            public uint blockSize;
            public ulong containerBlocks;
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, Partition partition)
        {
            if(partition.Start >= partition.End)
                return false;

            byte[] sector = imagePlugin.ReadSector(partition.Start);
            ApfsContainerSuperBlock nxSb;

            try
            {
                GCHandle handle = GCHandle.Alloc(sector, GCHandleType.Pinned);
                nxSb = (ApfsContainerSuperBlock)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(ApfsContainerSuperBlock));
                handle.Free();
            }
            catch
            {
                return false;
            }

            if(nxSb.magic == ApfsContainerMagic)
                return true;

            return false;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, Partition partition, out string information)
        {
            StringBuilder sbInformation = new StringBuilder();
            xmlFSType = new Schemas.FileSystemType();
            information = "";

            if(partition.Start >= partition.End)
                return;

            byte[] sector = imagePlugin.ReadSector(partition.Start);
            ApfsContainerSuperBlock nxSb;

            try
            {
                GCHandle handle = GCHandle.Alloc(sector, GCHandleType.Pinned);
                nxSb = (ApfsContainerSuperBlock)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(ApfsContainerSuperBlock));
                handle.Free();
            }
            catch
            {
                return;
            }

            if(nxSb.magic != ApfsContainerMagic)
                return;

            sbInformation.AppendLine("Apple File System");
            sbInformation.AppendLine();
            sbInformation.AppendFormat("{0} bytes per block", nxSb.blockSize).AppendLine();
            sbInformation.AppendFormat("Container has {0} bytes in {1} blocks", nxSb.containerBlocks * nxSb.blockSize, nxSb.containerBlocks).AppendLine();

            information = sbInformation.ToString();

            xmlFSType = new Schemas.FileSystemType();
            xmlFSType.Bootable = false;
            xmlFSType.Clusters = (long)nxSb.containerBlocks;
            xmlFSType.ClusterSize = (int)nxSb.blockSize;
            xmlFSType.Type = "Apple File System";
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

