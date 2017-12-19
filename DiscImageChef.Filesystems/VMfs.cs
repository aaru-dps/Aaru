// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : VMfs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : VMware file system plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the VMware file system and shows information.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;

namespace DiscImageChef.Filesystems
{
    public class VMfs : Filesystem
    {
        public VMfs()
        {
            Name = "VMware filesystem";
            PluginUUID = new Guid("EE52BDB8-B49C-4122-A3DA-AD21CBE79843");
            CurrentEncoding = Encoding.UTF8;
        }

        public VMfs(Encoding encoding)
        {
            Name = "VMware filesystem";
            PluginUUID = new Guid("EE52BDB8-B49C-4122-A3DA-AD21CBE79843");
            if(encoding == null)
                CurrentEncoding = Encoding.UTF8;
            else
                CurrentEncoding = encoding;
        }

        public VMfs(ImagePlugins.ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "VMware filesystem";
            PluginUUID = new Guid("EE52BDB8-B49C-4122-A3DA-AD21CBE79843");
            if(encoding == null)
                CurrentEncoding = Encoding.UTF8;
            else
                CurrentEncoding = encoding;
        }

        [Flags]
        enum VMfsFlags : byte
        {
            RecyledFolder = 64,
            CaseSensitive = 128
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VolumeInfo
        {
            public uint magic;
            public uint version;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] unknown1;
            public byte lun;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] unknown2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
            public byte[] name;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 49)]
            public byte[] unknown3;
            public uint size;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 31)]
            public byte[] unknown4;
            public Guid uuid;
            public ulong ctime;
            public ulong mtime;
        }

        /// <summary>
        /// Identifier for VMfs
        /// </summary>
        const uint VMfs_MAGIC = 0xC001D00D;
        const uint VMfs_Base = 0x00100000;

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, Partition partition)
        {
            if(partition.Start >= partition.End)
                return false;

            ulong vmfsSuperOff = VMfs_Base / imagePlugin.ImageInfo.sectorSize;

            if(partition.Start + vmfsSuperOff > partition.End)
                return false;
            
            byte[] sector = imagePlugin.ReadSector(partition.Start + vmfsSuperOff);

            uint magic = BitConverter.ToUInt32(sector, 0x00);

            return magic == VMfs_MAGIC;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, Partition partition, out string information)
        {
            ulong vmfsSuperOff = VMfs_Base / imagePlugin.ImageInfo.sectorSize;
            byte[] sector = imagePlugin.ReadSector(partition.Start + vmfsSuperOff);

            VolumeInfo volInfo = new VolumeInfo();
            IntPtr volInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(volInfo));
            Marshal.Copy(sector, 0, volInfoPtr, Marshal.SizeOf(volInfo));
            volInfo = (VolumeInfo)Marshal.PtrToStructure(volInfoPtr, typeof(VolumeInfo));
            Marshal.FreeHGlobal(volInfoPtr);

            StringBuilder sbInformation = new StringBuilder();

            sbInformation.AppendLine("VMware file system");

            uint ctimeSecs = (uint)(volInfo.ctime / 1000000);
            uint ctimeNanoSecs = (uint)(volInfo.ctime % 1000000);
            uint mtimeSecs = (uint)(volInfo.mtime / 1000000);
            uint mtimeNanoSecs = (uint)(volInfo.mtime % 1000000);

            sbInformation.AppendFormat("Volume version {0}", volInfo.version).AppendLine();
            sbInformation.AppendFormat("Volume name {0}", StringHandlers.CToString(volInfo.name, CurrentEncoding)).AppendLine();
            sbInformation.AppendFormat("Volume size {0} bytes", volInfo.size * 256).AppendLine();
            sbInformation.AppendFormat("Volume UUID {0}", volInfo.uuid).AppendLine();
            sbInformation.AppendFormat("Volume created on {0}", DateHandlers.UNIXUnsignedToDateTime(ctimeSecs, ctimeNanoSecs)).AppendLine();
            sbInformation.AppendFormat("Volume last modified on {0}", DateHandlers.UNIXUnsignedToDateTime(mtimeSecs, mtimeNanoSecs)).AppendLine();

            information = sbInformation.ToString();

            xmlFSType = new Schemas.FileSystemType();
            xmlFSType.Type = "VMware file system";
            xmlFSType.CreationDate = DateHandlers.UNIXUnsignedToDateTime(ctimeSecs, ctimeNanoSecs);
            xmlFSType.CreationDateSpecified = true;
            xmlFSType.ModificationDate = DateHandlers.UNIXUnsignedToDateTime(mtimeSecs, mtimeNanoSecs);
            xmlFSType.ModificationDateSpecified = true;
            xmlFSType.Clusters = volInfo.size * 256 / imagePlugin.ImageInfo.sectorSize;
            xmlFSType.ClusterSize = (int)imagePlugin.ImageInfo.sectorSize;
            xmlFSType.VolumeSerial = volInfo.uuid.ToString();
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