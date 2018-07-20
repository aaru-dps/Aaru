// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using Schemas;

namespace DiscImageChef.Filesystems
{
    // Information from http://www.owg.ru/mkt/BK/MKDOS.TXT
    // Thanks to tarlabnor for translating it
    public class MicroDOS : IFilesystem
    {
        const ushort MAGIC  = 0xA72E;
        const ushort MAGIC2 = 0x530C;

        public FileSystemType XmlFsType { get; private set; }
        public Encoding       Encoding  { get; private set; }
        public string         Name      => "MicroDOS file system";
        public Guid           Id        => new Guid("9F9A364A-1A27-48A3-B730-7A7122000324");

        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(1 + partition.Start >= partition.End) return false;

            if(imagePlugin.Info.SectorSize < 512) return false;

            byte[] bk0 = imagePlugin.ReadSector(0 + partition.Start);

            GCHandle handle = GCHandle.Alloc(bk0, GCHandleType.Pinned);
            MicroDosBlock0 block0 =
                (MicroDosBlock0)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(MicroDosBlock0));
            handle.Free();

            return block0.label == MAGIC && block0.mklabel == MAGIC2;
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding    encoding)
        {
            Encoding    = encoding ?? Encoding.GetEncoding("koi8-r");
            information = "";

            StringBuilder sb = new StringBuilder();

            byte[] bk0 = imagePlugin.ReadSector(0 + partition.Start);

            GCHandle handle = GCHandle.Alloc(bk0, GCHandleType.Pinned);
            MicroDosBlock0 block0 =
                (MicroDosBlock0)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(MicroDosBlock0));
            handle.Free();

            sb.AppendLine("MicroDOS filesystem");
            sb.AppendFormat("Volume has {0} blocks ({1} bytes)", block0.blocks, block0.blocks * 512).AppendLine();
            sb.AppendFormat("Volume has {0} blocks used ({1} bytes)", block0.usedBlocks, block0.usedBlocks * 512)
              .AppendLine();
            sb.AppendFormat("Volume contains {0} files", block0.files).AppendLine();
            sb.AppendFormat("First used block is {0}", block0.firstUsedBlock).AppendLine();

            XmlFsType = new FileSystemType
            {
                Type                  = "MicroDOS",
                ClusterSize           = 512,
                Clusters              = block0.blocks,
                Files                 = block0.files,
                FilesSpecified        = true,
                FreeClusters          = block0.blocks - block0.usedBlocks,
                FreeClustersSpecified = true
            };

            information = sb.ToString();
        }

        // Followed by directory entries
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct MicroDosBlock0
        {
            /// <summary>BK starts booting here</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
            public byte[] bootCode;
            /// <summary>Number of files in directory</summary>
            public ushort files;
            /// <summary>Total number of blocks in files of the directory</summary>
            public ushort usedBlocks;
            /// <summary>Unknown</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 228)]
            public byte[] unknown;
            /// <summary>Ownership label (label that shows it belongs to Micro DOS format)</summary>
            public ushort label;
            /// <summary>MK-DOS directory format label</summary>
            public ushort mklabel;
            /// <summary>Unknown</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public byte[] unknown2;
            /// <summary>
            ///     Disk size in blocks (absolute value for the system unlike NORD, NORTON etc.) that
            ///     doesn't use two fixed values 40 or 80 tracks, but i.e. if you drive works with 76 tracks
            ///     this field will contain an appropriate number of blocks
            /// </summary>
            public ushort blocks;
            /// <summary> Number of the first file's block. Value is changable</summary>
            public ushort firstUsedBlock;
            /// <summary>Unknown</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] unknown3;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DirectoryEntry
        {
            /// <summary>File status</summary>
            public byte status;
            /// <summary>Directory number (0 - root)</summary>
            public byte directory;
            /// <summary>File name 14. symbols in ASCII KOI8</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] filename;
            /// <summary>Block number</summary>
            public ushort blockNo;
            /// <summary>Length in blocks</summary>
            public ushort blocks;
            /// <summary>Address</summary>
            public ushort address;
            /// <summary>Length</summary>
            public ushort length;
        }

        enum FileStatus : byte
        {
            CommonFile  = 0,
            Protected   = 1,
            LogicalDisk = 2,
            BadFile     = 0x80,
            Deleted     = 0xFF
        }
    }
}