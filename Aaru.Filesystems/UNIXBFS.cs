// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Schemas;

namespace Aaru.Filesystems
{
    // Information from the Linux kernel
    /// <inheritdoc />
    /// <summary>Implements detection of the UNIX boot filesystem</summary>
    public sealed class BFS : IFilesystem
    {
        const uint BFS_MAGIC = 0x1BADFACE;

        /// <inheritdoc />
        public FileSystemType XmlFsType { get; private set; }
        /// <inheritdoc />
        public Encoding Encoding { get; private set; }
        /// <inheritdoc />
        public string Name => "UNIX Boot filesystem";
        /// <inheritdoc />
        public Guid Id => new("1E6E0DA6-F7E4-494C-80C6-CB5929E96155");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

        /// <inheritdoc />
        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(2 + partition.Start >= partition.End)
                return false;

            ErrorNumber errno = imagePlugin.ReadSector(0 + partition.Start, out byte[] tmp);

            if(errno != ErrorNumber.NoError)
                return false;

            uint magic = BitConverter.ToUInt32(tmp, 0);

            return magic == BFS_MAGIC;
        }

        /// <inheritdoc />
        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
            information = "";

            var         sb    = new StringBuilder();
            ErrorNumber errno = imagePlugin.ReadSector(0 + partition.Start, out byte[] bfsSbSector);

            if(errno != ErrorNumber.NoError)
                return;

            byte[] sbStrings = new byte[6];

            var bfsSb = new SuperBlock
            {
                s_magic = BitConverter.ToUInt32(bfsSbSector, 0x00),
                s_start = BitConverter.ToUInt32(bfsSbSector, 0x04),
                s_end   = BitConverter.ToUInt32(bfsSbSector, 0x08),
                s_from  = BitConverter.ToUInt32(bfsSbSector, 0x0C),
                s_to    = BitConverter.ToUInt32(bfsSbSector, 0x10),
                s_bfrom = BitConverter.ToInt32(bfsSbSector, 0x14),
                s_bto   = BitConverter.ToInt32(bfsSbSector, 0x18)
            };

            Array.Copy(bfsSbSector, 0x1C, sbStrings, 0, 6);
            bfsSb.s_fsname = StringHandlers.CToString(sbStrings, Encoding);
            Array.Copy(bfsSbSector, 0x22, sbStrings, 0, 6);
            bfsSb.s_volume = StringHandlers.CToString(sbStrings, Encoding);

            AaruConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_magic: 0x{0:X8}", bfsSb.s_magic);
            AaruConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_start: 0x{0:X8}", bfsSb.s_start);
            AaruConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_end: 0x{0:X8}", bfsSb.s_end);
            AaruConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_from: 0x{0:X8}", bfsSb.s_from);
            AaruConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_to: 0x{0:X8}", bfsSb.s_to);
            AaruConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_bfrom: 0x{0:X8}", bfsSb.s_bfrom);
            AaruConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_bto: 0x{0:X8}", bfsSb.s_bto);
            AaruConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_fsname: 0x{0}", bfsSb.s_fsname);
            AaruConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_volume: 0x{0}", bfsSb.s_volume);

            sb.AppendLine("UNIX Boot filesystem");

            sb.AppendFormat("Volume goes from byte {0} to byte {1}, for {2} bytes", bfsSb.s_start, bfsSb.s_end,
                            bfsSb.s_end - bfsSb.s_start).AppendLine();

            sb.AppendFormat("Filesystem name: {0}", bfsSb.s_fsname).AppendLine();
            sb.AppendFormat("Volume name: {0}", bfsSb.s_volume).AppendLine();

            XmlFsType = new FileSystemType
            {
                Type        = "BFS",
                VolumeName  = bfsSb.s_volume,
                ClusterSize = imagePlugin.Info.SectorSize,
                Clusters    = partition.End - partition.Start + 1
            };

            information = sb.ToString();
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        struct SuperBlock
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