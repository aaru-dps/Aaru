// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : UNIXBFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : UnixWare boot filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the UnixWare boot filesystem and shows information.
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
using System.Text;
using DiscImageChef.Console;

namespace DiscImageChef.Filesystems
{
    // Information from the Linux kernel
    class BFS : Filesystem
    {
        const uint BFS_MAGIC = 0x1BADFACE;

        public BFS()
        {
            Name = "UNIX Boot filesystem";
            PluginUUID = new Guid("1E6E0DA6-F7E4-494C-80C6-CB5929E96155");
        }

        public BFS(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            Name = "UNIX Boot filesystem";
            PluginUUID = new Guid("1E6E0DA6-F7E4-494C-80C6-CB5929E96155");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            if((2 + partitionStart) >= partitionEnd)
                return false;

            uint magic;

            magic = BitConverter.ToUInt32(imagePlugin.ReadSector(0 + partitionStart), 0);

            return magic == BFS_MAGIC;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd, out string information)
        {
            information = "";

            StringBuilder sb = new StringBuilder();
            byte[] bfs_sb_sector = imagePlugin.ReadSector(0 + partitionStart);
            byte[] sb_strings = new byte[6];

            BFSSuperBlock bfs_sb = new BFSSuperBlock();

            bfs_sb.s_magic = BitConverter.ToUInt32(bfs_sb_sector, 0x00);
            bfs_sb.s_start = BitConverter.ToUInt32(bfs_sb_sector, 0x04);
            bfs_sb.s_end = BitConverter.ToUInt32(bfs_sb_sector, 0x08);
            bfs_sb.s_from = BitConverter.ToUInt32(bfs_sb_sector, 0x0C);
            bfs_sb.s_to = BitConverter.ToUInt32(bfs_sb_sector, 0x10);
            bfs_sb.s_bfrom = BitConverter.ToInt32(bfs_sb_sector, 0x14);
            bfs_sb.s_bto = BitConverter.ToInt32(bfs_sb_sector, 0x18);
            Array.Copy(bfs_sb_sector, 0x1C, sb_strings, 0, 6);
            bfs_sb.s_fsname = StringHandlers.CToString(sb_strings);
            Array.Copy(bfs_sb_sector, 0x22, sb_strings, 0, 6);
            bfs_sb.s_volume = StringHandlers.CToString(sb_strings);

            DicConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_magic: 0x{0:X8}", bfs_sb.s_magic);
            DicConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_start: 0x{0:X8}", bfs_sb.s_start);
            DicConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_end: 0x{0:X8}", bfs_sb.s_end);
            DicConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_from: 0x{0:X8}", bfs_sb.s_from);
            DicConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_to: 0x{0:X8}", bfs_sb.s_to);
            DicConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_bfrom: 0x{0:X8}", bfs_sb.s_bfrom);
            DicConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_bto: 0x{0:X8}", bfs_sb.s_bto);
            DicConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_fsname: 0x{0}", bfs_sb.s_fsname);
            DicConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_volume: 0x{0}", bfs_sb.s_volume);

            sb.AppendLine("UNIX Boot filesystem");
            sb.AppendFormat("Volume goes from byte {0} to byte {1}, for {2} bytes", bfs_sb.s_start, bfs_sb.s_end, bfs_sb.s_end - bfs_sb.s_start).AppendLine();
            sb.AppendFormat("Filesystem name: {0}", bfs_sb.s_fsname).AppendLine();
            sb.AppendFormat("Volume name: {0}", bfs_sb.s_volume).AppendLine();

            xmlFSType = new Schemas.FileSystemType();
            xmlFSType.Type = "BFS";
            xmlFSType.VolumeName = bfs_sb.s_volume;
            xmlFSType.ClusterSize = (int)imagePlugin.GetSectorSize();
            xmlFSType.Clusters = (long)(partitionEnd - partitionStart + 1);

            information = sb.ToString();
        }

        struct BFSSuperBlock
        {
            /// <summary>0x00, 0x1BADFACE</summary>
            public uint s_magic;
            /// <summary>0x04, start in bytes of volume</summary>
            public uint s_start;
            /// <summary>0x08, end in bytes of volume</summary>
            public uint s_end;
            /// <summary>0x0C, unknown :p</summary>
            public uint s_from;
            /// <summary>0x10, unknown :p</summary>
            public uint s_to;
            /// <summary>0x14, unknown :p</summary>
            public int s_bfrom;
            /// <summary>0x18, unknown :p</summary>
            public int s_bto;
            /// <summary>0x1C, 6 bytes, filesystem name</summary>
            public string s_fsname;
            /// <summary>0x22, 6 bytes, volume name</summary>
            public string s_volume;
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