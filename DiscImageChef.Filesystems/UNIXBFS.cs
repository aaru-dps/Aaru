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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.DiscImages;
using Schemas;

namespace DiscImageChef.Filesystems
{
    // Information from the Linux kernel
    public class BFS : IFilesystem
    {
        const uint BFS_MAGIC = 0x1BADFACE;

        Encoding currentEncoding;
        FileSystemType xmlFsType;
        public FileSystemType XmlFsType => xmlFsType;

        public Encoding Encoding => currentEncoding;
        public string Name => "UNIX Boot filesystem";
        public Guid Id => new Guid("1E6E0DA6-F7E4-494C-80C6-CB5929E96155");

        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(2 + partition.Start >= partition.End) return false;

            uint magic;

            magic = BitConverter.ToUInt32(imagePlugin.ReadSector(0 + partition.Start), 0);

            return magic == BFS_MAGIC;
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
        {
            currentEncoding = encoding ?? Encoding.GetEncoding("iso-8859-15");
            information = "";

            StringBuilder sb = new StringBuilder();
            byte[] bfsSbSector = imagePlugin.ReadSector(0 + partition.Start);
            byte[] sbStrings = new byte[6];

            BFSSuperBlock bfsSb = new BFSSuperBlock
            {
                s_magic = BitConverter.ToUInt32(bfsSbSector, 0x00),
                s_start = BitConverter.ToUInt32(bfsSbSector, 0x04),
                s_end = BitConverter.ToUInt32(bfsSbSector, 0x08),
                s_from = BitConverter.ToUInt32(bfsSbSector, 0x0C),
                s_to = BitConverter.ToUInt32(bfsSbSector, 0x10),
                s_bfrom = BitConverter.ToInt32(bfsSbSector, 0x14),
                s_bto = BitConverter.ToInt32(bfsSbSector, 0x18)
            };

            Array.Copy(bfsSbSector, 0x1C, sbStrings, 0, 6);
            bfsSb.s_fsname = StringHandlers.CToString(sbStrings, currentEncoding);
            Array.Copy(bfsSbSector, 0x22, sbStrings, 0, 6);
            bfsSb.s_volume = StringHandlers.CToString(sbStrings, currentEncoding);

            DicConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_magic: 0x{0:X8}", bfsSb.s_magic);
            DicConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_start: 0x{0:X8}", bfsSb.s_start);
            DicConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_end: 0x{0:X8}", bfsSb.s_end);
            DicConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_from: 0x{0:X8}", bfsSb.s_from);
            DicConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_to: 0x{0:X8}", bfsSb.s_to);
            DicConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_bfrom: 0x{0:X8}", bfsSb.s_bfrom);
            DicConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_bto: 0x{0:X8}", bfsSb.s_bto);
            DicConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_fsname: 0x{0}", bfsSb.s_fsname);
            DicConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_volume: 0x{0}", bfsSb.s_volume);

            sb.AppendLine("UNIX Boot filesystem");
            sb.AppendFormat("Volume goes from byte {0} to byte {1}, for {2} bytes", bfsSb.s_start, bfsSb.s_end,
                            bfsSb.s_end - bfsSb.s_start).AppendLine();
            sb.AppendFormat("Filesystem name: {0}", bfsSb.s_fsname).AppendLine();
            sb.AppendFormat("Volume name: {0}", bfsSb.s_volume).AppendLine();

            xmlFsType = new FileSystemType
            {
                Type = "BFS",
                VolumeName = bfsSb.s_volume,
                ClusterSize = (int)imagePlugin.Info.SectorSize,
                Clusters = (long)(partition.End - partition.Start + 1)
            };

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
    }
}