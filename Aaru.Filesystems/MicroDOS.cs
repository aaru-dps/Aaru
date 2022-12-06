// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MicroDOS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : MicroDOS filesystem plugin
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the MicroDOS filesystem and shows information.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local

namespace Aaru.Filesystems
{
    /// <inheritdoc />
    /// <summary>
    ///     Implements detection for the MicroDOS filesystem. Information from http://www.owg.ru/mkt/BK/MKDOS.TXT Thanks
    ///     to tarlabnor for translating it
    /// </summary>
    public sealed class MicroDOS : IFilesystem
    {
        const ushort MAGIC  = 0xA72E;
        const ushort MAGIC2 = 0x530C;

        /// <inheritdoc />
        public FileSystemType XmlFsType { get; private set; }
        /// <inheritdoc />
        public Encoding Encoding { get; private set; }
        /// <inheritdoc />
        public string Name => "MicroDOS file system";
        /// <inheritdoc />
        public Guid Id => new Guid("9F9A364A-1A27-48A3-B730-7A7122000324");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

        /// <inheritdoc />
        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(1 + partition.Start >= partition.End)
                return false;

            if(imagePlugin.Info.SectorSize < 512)
                return false;

            byte[] bk0 = imagePlugin.ReadSector(0 + partition.Start);

            Block0 block0 = Marshal.ByteArrayToStructureLittleEndian<Block0>(bk0);

            return block0.label == MAGIC && block0.mklabel == MAGIC2;
        }

        /// <inheritdoc />
        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding    = encoding ?? Encoding.GetEncoding("koi8-r");
            information = "";

            var sb = new StringBuilder();

            byte[] bk0 = imagePlugin.ReadSector(0 + partition.Start);

            Block0 block0 = Marshal.ByteArrayToStructureLittleEndian<Block0>(bk0);

            sb.AppendLine("MicroDOS filesystem");
            sb.AppendFormat("Volume has {0} blocks ({1} bytes)", block0.blocks, block0.blocks * 512).AppendLine();

            sb.AppendFormat("Volume has {0} blocks used ({1} bytes)", block0.usedBlocks, block0.usedBlocks * 512).
               AppendLine();

            sb.AppendFormat("Volume contains {0} files", block0.files).AppendLine();
            sb.AppendFormat("First used block is {0}", block0.firstUsedBlock).AppendLine();

            XmlFsType = new FileSystemType
            {
                Type                  = "MicroDOS",
                ClusterSize           = 512,
                Clusters              = block0.blocks,
                Files                 = block0.files,
                FilesSpecified        = true,
                FreeClusters          = (ulong)(block0.blocks - block0.usedBlocks),
                FreeClustersSpecified = true
            };

            information = sb.ToString();
        }

        // Followed by directory entries
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct Block0
        {
            /// <summary>BK starts booting here</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
            public readonly byte[] bootCode;
            /// <summary>Number of files in directory</summary>
            public readonly ushort files;
            /// <summary>Total number of blocks in files of the directory</summary>
            public readonly ushort usedBlocks;
            /// <summary>Unknown</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 228)]
            public readonly byte[] unknown;
            /// <summary>Ownership label (label that shows it belongs to Micro DOS format)</summary>
            public readonly ushort label;
            /// <summary>MK-DOS directory format label</summary>
            public readonly ushort mklabel;
            /// <summary>Unknown</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public readonly byte[] unknown2;
            /// <summary>
            ///     Disk size in blocks (absolute value for the system unlike NORD, NORTON etc.) that doesn't use two fixed values
            ///     40 or 80 tracks, but i.e. if you drive works with 76 tracks this field will contain an appropriate number of blocks
            /// </summary>
            public readonly ushort blocks;
            /// <summary> Number of the first file's block. Value is changable</summary>
            public readonly ushort firstUsedBlock;
            /// <summary>Unknown</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public readonly byte[] unknown3;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct DirectoryEntry
        {
            /// <summary>File status</summary>
            public readonly byte status;
            /// <summary>Directory number (0 - root)</summary>
            public readonly byte directory;
            /// <summary>File name 14. symbols in ASCII KOI8</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public readonly byte[] filename;
            /// <summary>Block number</summary>
            public readonly ushort blockNo;
            /// <summary>Length in blocks</summary>
            public readonly ushort blocks;
            /// <summary>Address</summary>
            public readonly ushort address;
            /// <summary>Length</summary>
            public readonly ushort length;
        }

        enum FileStatus : byte
        {
            CommonFile = 0, Protected  = 1, LogicalDisk = 2,
            BadFile    = 0x80, Deleted = 0xFF
        }
    }
}