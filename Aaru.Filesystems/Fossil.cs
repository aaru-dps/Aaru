// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Fossil.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Fossil filesystem plugin
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Fossil filesystem and shows information.
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
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems
{
    /// <summary>
    /// Implements detection for the Plan-9 Fossil on-disk filesystem
    /// </summary>
    public sealed class Fossil : IFilesystem
    {
        const uint FOSSIL_HDR_MAGIC = 0x3776AE89;
        const uint FOSSIL_SB_MAGIC  = 0x2340A3B1;

        // Fossil header starts at 128KiB
        const ulong HEADER_POS = 128 * 1024;

        /// <inheritdoc />
        public FileSystemType XmlFsType { get; private set; }
        /// <inheritdoc />
        public Encoding       Encoding  { get; private set; }
        /// <inheritdoc />
        public string         Name      => "Fossil Filesystem Plugin";
        /// <inheritdoc />
        public Guid           Id        => new Guid("932BF104-43F6-494F-973C-45EF58A51DA9");
        /// <inheritdoc />
        public string         Author    => "Natalia Portillo";

        /// <inheritdoc />
        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            ulong hdrSector = HEADER_POS / imagePlugin.Info.SectorSize;

            if(partition.Start + hdrSector > imagePlugin.Info.Sectors)
                return false;

            byte[] sector = imagePlugin.ReadSector(partition.Start + hdrSector);
            Header hdr    = Marshal.ByteArrayToStructureBigEndian<Header>(sector);

            AaruConsole.DebugWriteLine("Fossil plugin", "magic at 0x{0:X8} (expected 0x{1:X8})", hdr.magic,
                                       FOSSIL_HDR_MAGIC);

            return hdr.magic == FOSSIL_HDR_MAGIC;
        }

        /// <inheritdoc />
        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            // Technically everything on Plan 9 from Bell Labs is in UTF-8
            Encoding    = Encoding.UTF8;
            information = "";

            if(imagePlugin.Info.SectorSize < 512)
                return;

            ulong hdrSector = HEADER_POS / imagePlugin.Info.SectorSize;

            byte[] sector = imagePlugin.ReadSector(partition.Start + hdrSector);
            Header hdr    = Marshal.ByteArrayToStructureBigEndian<Header>(sector);

            AaruConsole.DebugWriteLine("Fossil plugin", "magic at 0x{0:X8} (expected 0x{1:X8})", hdr.magic,
                                       FOSSIL_HDR_MAGIC);

            var sb = new StringBuilder();

            sb.AppendLine("Fossil");
            sb.AppendFormat("Filesystem version {0}", hdr.version).AppendLine();
            sb.AppendFormat("{0} bytes per block", hdr.blockSize).AppendLine();
            sb.AppendFormat("Superblock resides in block {0}", hdr.super).AppendLine();
            sb.AppendFormat("Labels resides in block {0}", hdr.label).AppendLine();
            sb.AppendFormat("Data starts at block {0}", hdr.data).AppendLine();
            sb.AppendFormat("Volume has {0} blocks", hdr.end).AppendLine();

            ulong sbLocation = (hdr.super * (hdr.blockSize / imagePlugin.Info.SectorSize)) + partition.Start;

            XmlFsType = new FileSystemType
            {
                Type        = "Fossil filesystem",
                ClusterSize = hdr.blockSize,
                Clusters    = hdr.end
            };

            if(sbLocation <= partition.End)
            {
                sector = imagePlugin.ReadSector(sbLocation);
                SuperBlock fsb = Marshal.ByteArrayToStructureBigEndian<SuperBlock>(sector);

                AaruConsole.DebugWriteLine("Fossil plugin", "magic 0x{0:X8} (expected 0x{1:X8})", fsb.magic,
                                           FOSSIL_SB_MAGIC);

                if(fsb.magic == FOSSIL_SB_MAGIC)
                {
                    sb.AppendFormat("Epoch low {0}", fsb.epochLow).AppendLine();
                    sb.AppendFormat("Epoch high {0}", fsb.epochHigh).AppendLine();
                    sb.AppendFormat("Next QID {0}", fsb.qid).AppendLine();
                    sb.AppendFormat("Active root block {0}", fsb.active).AppendLine();
                    sb.AppendFormat("Next root block {0}", fsb.next).AppendLine();
                    sb.AppendFormat("Current root block {0}", fsb.current).AppendLine();
                    sb.AppendFormat("Volume label: \"{0}\"", StringHandlers.CToString(fsb.name, Encoding)).AppendLine();
                    XmlFsType.VolumeName = StringHandlers.CToString(fsb.name, Encoding);
                }
            }

            information = sb.ToString();
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct Header
        {
            /// <summary>Magic number</summary>
            public readonly uint magic;
            /// <summary>Header version</summary>
            public readonly ushort version;
            /// <summary>Block size</summary>
            public readonly ushort blockSize;
            /// <summary>Block containing superblock</summary>
            public readonly uint super;
            /// <summary>Block containing labels</summary>
            public readonly uint label;
            /// <summary>Where do data blocks start</summary>
            public readonly uint data;
            /// <summary>How many data blocks does it have</summary>
            public readonly uint end;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct SuperBlock
        {
            /// <summary>Magic number</summary>
            public readonly uint magic;
            /// <summary>Header version</summary>
            public readonly ushort version;
            /// <summary>file system low epoch</summary>
            public readonly uint epochLow;
            /// <summary>file system high(active) epoch</summary>
            public readonly uint epochHigh;
            /// <summary>next qid to allocate</summary>
            public readonly ulong qid;
            /// <summary>data block number: root of active file system</summary>
            public readonly int active;
            /// <summary>data block number: root of next file system to archive</summary>
            public readonly int next;
            /// <summary>data block number: root of file system currently being archived</summary>
            public readonly int current;
            /// <summary>Venti score of last successful archive</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public readonly byte[] last;
            /// <summary>name of file system(just a comment)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public readonly byte[] name;
        }
    }
}