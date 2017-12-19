// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : LIF.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : HP Logical Interchange Format plugin
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the HP Logical Interchange Format and shows information.
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
using DiscImageChef.Console;

namespace DiscImageChef.Filesystems
{
    // Information from http://www.hp9845.net/9845/projects/hpdir/#lif_filesystem
    public class LIF : Filesystem
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct LIF_SystemBlock
        {
            public ushort magic;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] volumeLabel;
            public uint directoryStart;
            public ushort lifId;
            public ushort unused;
            public uint directorySize;
            public ushort lifVersion;
            public ushort unused2;
            public uint tracks;
            public uint heads;
            public uint sectors;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] creationDate;
        }

        const uint LIF_Magic = 0x8000;

        public LIF()
        {
            Name = "HP Logical Interchange Format Plugin";
            PluginUUID = new Guid("41535647-77A5-477B-9206-DA727ACDC704");
            CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
        }

        public LIF(Encoding encoding)
        {
            Name = "HP Logical Interchange Format Plugin";
            PluginUUID = new Guid("41535647-77A5-477B-9206-DA727ACDC704");
            if(encoding == null)
                CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
            else
                CurrentEncoding = encoding;
        }

        public LIF(ImagePlugins.ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "HP Logical Interchange Format Plugin";
            PluginUUID = new Guid("41535647-77A5-477B-9206-DA727ACDC704");
            if(encoding == null)
                CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
            else
                CurrentEncoding = encoding;
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, Partition partition)
        {
            if(imagePlugin.GetSectorSize() < 256)
                return false;

            LIF_SystemBlock LIFSb = new LIF_SystemBlock();

            byte[] sector = imagePlugin.ReadSector(partition.Start);
            LIFSb = BigEndianMarshal.ByteArrayToStructureBigEndian<LIF_SystemBlock>(sector);
            DicConsole.DebugWriteLine("LIF plugin", "magic 0x{0:X8} (expected 0x{1:X8})", LIFSb.magic, LIF_Magic);

            return LIFSb.magic == LIF_Magic;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, Partition partition, out string information)
        {
            information = "";

            if(imagePlugin.GetSectorSize() < 256)
                return;

            LIF_SystemBlock LIFSb = new LIF_SystemBlock();

            byte[] sector = imagePlugin.ReadSector(partition.Start);
            LIFSb = BigEndianMarshal.ByteArrayToStructureBigEndian<LIF_SystemBlock>(sector);

            if(LIFSb.magic != LIF_Magic)
                return;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("HP Logical Interchange Format");
            sb.AppendFormat("Directory starts at cluster {0}", LIFSb.directoryStart).AppendLine();
            sb.AppendFormat("LIF identifier: {0}", LIFSb.lifId).AppendLine();
            sb.AppendFormat("Directory size: {0} clusters", LIFSb.directorySize).AppendLine();
            sb.AppendFormat("LIF version: {0}", LIFSb.lifVersion).AppendLine();
            // How is this related to volume size? I have only CDs to test and makes no sense there
            sb.AppendFormat("{0} tracks", LIFSb.tracks).AppendLine();
            sb.AppendFormat("{0} heads", LIFSb.heads).AppendLine();
            sb.AppendFormat("{0} sectors", LIFSb.sectors).AppendLine();
            sb.AppendFormat("Volume name: {0}", StringHandlers.CToString(LIFSb.volumeLabel, CurrentEncoding)).AppendLine();
            sb.AppendFormat("Volume created on {0}", DateHandlers.LifToDateTime(LIFSb.creationDate)).AppendLine();

            information = sb.ToString();

            xmlFSType = new Schemas.FileSystemType
            {
                Type = "HP Logical Interchange Format",
                ClusterSize = 256,
                Clusters = (long)(partition.Size / 256),
                CreationDate = DateHandlers.LifToDateTime(LIFSb.creationDate),
                CreationDateSpecified = true,
                VolumeName = StringHandlers.CToString(LIFSb.volumeLabel, CurrentEncoding)
            };
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