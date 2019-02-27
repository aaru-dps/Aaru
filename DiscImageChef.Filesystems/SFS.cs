// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SmartFileSystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the SmartFileSystem and shows information.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using Schemas;
using Marshal = DiscImageChef.Helpers.Marshal;

namespace DiscImageChef.Filesystems
{
    public class SFS : IFilesystem
    {
        /// <summary>Identifier for SFS v1</summary>
        const uint SFS_MAGIC = 0x53465300;
        /// <summary>Identifier for SFS v2</summary>
        const uint SFS2_MAGIC = 0x53465302;

        public FileSystemType XmlFsType { get; private set; }
        public Encoding       Encoding  { get; private set; }
        public string         Name      => "SmartFileSystem";
        public Guid           Id        => new Guid("26550C19-3671-4A2D-BC2F-F20CEB7F48DC");
        public string         Author    => "Natalia Portillo";

        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(partition.Start >= partition.End) return false;

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            byte[] sector = imagePlugin.ReadSector(partition.Start);

            uint magic = BigEndianBitConverter.ToUInt32(sector, 0x00);

            return magic == SFS_MAGIC || magic == SFS2_MAGIC;
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding    encoding)
        {
            Encoding = encoding ?? Encoding.GetEncoding("iso-8859-1");
            byte[]    rootBlockSector = imagePlugin.ReadSector(partition.Start);
            RootBlock rootBlock       = Marshal.ByteArrayToStructureBigEndian<RootBlock>(rootBlockSector);

            StringBuilder sbInformation = new StringBuilder();

            sbInformation.AppendLine("SmartFileSystem");

            sbInformation.AppendFormat("Volume version {0}", rootBlock.version).AppendLine();
            sbInformation.AppendFormat("Volume starts on device byte {0} and ends on byte {1}", rootBlock.firstbyte,
                                       rootBlock.lastbyte).AppendLine();
            sbInformation
               .AppendFormat("Volume has {0} blocks of {1} bytes each", rootBlock.totalblocks, rootBlock.blocksize)
               .AppendLine();
            sbInformation.AppendFormat("Volume created on {0}",
                                       DateHandlers.UnixUnsignedToDateTime(rootBlock.datecreated).AddYears(8))
                         .AppendLine();
            sbInformation.AppendFormat("Bitmap starts in block {0}", rootBlock.bitmapbase).AppendLine();
            sbInformation.AppendFormat("Admin space container starts in block {0}", rootBlock.adminspacecontainer)
                         .AppendLine();
            sbInformation.AppendFormat("Root object container starts in block {0}", rootBlock.rootobjectcontainer)
                         .AppendLine();
            sbInformation.AppendFormat("Root node of the extent B-tree resides in block {0}", rootBlock.extentbnoderoot)
                         .AppendLine();
            sbInformation.AppendFormat("Root node of the object B-tree resides in block {0}", rootBlock.objectnoderoot)
                         .AppendLine();
            if(rootBlock.bits.HasFlag(SFSFlags.CaseSensitive)) sbInformation.AppendLine("Volume is case sensitive");
            if(rootBlock.bits.HasFlag(SFSFlags.RecyledFolder))
                sbInformation.AppendLine("Volume moves deleted files to a recycled folder");
            information = sbInformation.ToString();

            XmlFsType = new FileSystemType
            {
                CreationDate          = DateHandlers.UnixUnsignedToDateTime(rootBlock.datecreated).AddYears(8),
                CreationDateSpecified = true,
                Clusters              = rootBlock.totalblocks,
                ClusterSize           = (int)rootBlock.blocksize,
                Type                  = "SmartFileSystem"
            };
        }

        [Flags]
        enum SFSFlags : byte
        {
            RecyledFolder = 64,
            CaseSensitive = 128
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct RootBlock
        {
            public uint     blockId;
            public uint     blockChecksum;
            public uint     blockSelfPointer;
            public ushort   version;
            public ushort   sequence;
            public uint     datecreated;
            public SFSFlags bits;
            public byte     padding1;
            public ushort   padding2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public uint[] reserved1;
            public ulong firstbyte;
            public ulong lastbyte;
            public uint  totalblocks;
            public uint  blocksize;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public uint[] reserved2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public uint[] reserved3;
            public uint bitmapbase;
            public uint adminspacecontainer;
            public uint rootobjectcontainer;
            public uint extentbnoderoot;
            public uint objectnoderoot;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public uint[] reserved4;
        }
    }
}