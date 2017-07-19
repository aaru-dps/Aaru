// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Reiser4.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Component
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Reiser4 filesystem and shows information.
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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;

namespace DiscImageChef.Filesystems
{
    public class Reiser4 : Filesystem
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Reiser4_Superblock
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] magic;
            public ushort diskformat;
            public ushort blocksize;
            public Guid uuid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] label;
        }

        readonly byte[] Reiser4_Magic = { 0x52, 0x65, 0x49, 0x73, 0x45, 0x72, 0x34, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        const uint Reiser4_SuperOffset = 0x10000;

        public Reiser4()
        {
            Name = "Reiser4 Filesystem Plugin";
            PluginUUID = new Guid("301F2D00-E8D5-4F04-934E-81DFB21D15BA");
            CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
        }

        public Reiser4(ImagePlugins.ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "Reiser4 Filesystem Plugin";
            PluginUUID = new Guid("301F2D00-E8D5-4F04-934E-81DFB21D15BA");
            if(encoding == null)
                CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, Partition partition)
        {
            if(imagePlugin.GetSectorSize() < 512)
                return false;

            uint sbAddr = Reiser4_SuperOffset / imagePlugin.GetSectorSize();
            if(sbAddr == 0)
                sbAddr = 1;

            Reiser4_Superblock reiserSb = new Reiser4_Superblock();

            uint sbSize = (uint)(Marshal.SizeOf(reiserSb) / imagePlugin.GetSectorSize());
            if(Marshal.SizeOf(reiserSb) % imagePlugin.GetSectorSize() != 0)
                sbSize++;

            byte[] sector = imagePlugin.ReadSectors(partition.Start + sbAddr, sbSize);
            if(sector.Length < Marshal.SizeOf(reiserSb))
                return false;

            IntPtr sbPtr = Marshal.AllocHGlobal(Marshal.SizeOf(reiserSb));
            Marshal.Copy(sector, 0, sbPtr, Marshal.SizeOf(reiserSb));
            reiserSb = (Reiser4_Superblock)Marshal.PtrToStructure(sbPtr, typeof(Reiser4_Superblock));
            Marshal.FreeHGlobal(sbPtr);

            return Reiser4_Magic.SequenceEqual(reiserSb.magic);
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, Partition partition, out string information)
        {
            information = "";
            if(imagePlugin.GetSectorSize() < 512)
                return;

            uint sbAddr = Reiser4_SuperOffset / imagePlugin.GetSectorSize();
            if(sbAddr == 0)
                sbAddr = 1;

            Reiser4_Superblock reiserSb = new Reiser4_Superblock();

            uint sbSize = (uint)(Marshal.SizeOf(reiserSb) / imagePlugin.GetSectorSize());
            if(Marshal.SizeOf(reiserSb) % imagePlugin.GetSectorSize() != 0)
                sbSize++;

            byte[] sector = imagePlugin.ReadSectors(partition.Start + sbAddr, sbSize);
            if(sector.Length < Marshal.SizeOf(reiserSb))
                return;

            IntPtr sbPtr = Marshal.AllocHGlobal(Marshal.SizeOf(reiserSb));
            Marshal.Copy(sector, 0, sbPtr, Marshal.SizeOf(reiserSb));
            reiserSb = (Reiser4_Superblock)Marshal.PtrToStructure(sbPtr, typeof(Reiser4_Superblock));
            Marshal.FreeHGlobal(sbPtr);

            if(!Reiser4_Magic.SequenceEqual(reiserSb.magic))
                return;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Reiser 4 filesystem");
            sb.AppendFormat("{0} bytes per block", reiserSb.blocksize).AppendLine();
            sb.AppendFormat("Volume disk format: {0}", reiserSb.diskformat).AppendLine();
            sb.AppendFormat("Volume UUID: {0}", reiserSb.uuid).AppendLine();
            sb.AppendFormat("Volume name: {0}", StringHandlers.CToString(reiserSb.label, CurrentEncoding)).AppendLine();

            information = sb.ToString();

            xmlFSType = new Schemas.FileSystemType();
            xmlFSType.Type = "Reiser 4 filesystem";
            xmlFSType.ClusterSize = reiserSb.blocksize;
            xmlFSType.Clusters = (long)(((partition.End - partition.Start) * imagePlugin.GetSectorSize()) / reiserSb.blocksize);
            xmlFSType.VolumeName = StringHandlers.CToString(reiserSb.label, CurrentEncoding);
            xmlFSType.VolumeSerial = reiserSb.uuid.ToString();
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