// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Opera.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Opera filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Opera filesystem and shows information.
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
using DiscImageChef.DiscImages;
using Schemas;

namespace DiscImageChef.Filesystems
{
    public class OperaFS : IFilesystem
    {
        Encoding currentEncoding;
        FileSystemType xmlFsType;
        public FileSystemType XmlFsType => xmlFsType;

        public Encoding Encoding => currentEncoding;
        public string Name => "Opera Filesystem Plugin";
        public Guid Id => new Guid("0ec84ec7-eae6-4196-83fe-943b3fe46dbd");

        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(2 + partition.Start >= partition.End) return false;

            byte[] sbSector = imagePlugin.ReadSector(0 + partition.Start);

            byte[] syncBytes = new byte[5];

            byte recordType = sbSector[0x000];
            Array.Copy(sbSector, 0x001, syncBytes, 0, 5);
            byte recordVersion = sbSector[0x006];

            if(recordType != 1 || recordVersion != 1) return false;

            return Encoding.ASCII.GetString(syncBytes) == "ZZZZZ";
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
        {
            // TODO: Find correct default encoding
            currentEncoding = Encoding.ASCII;
            information = "";
            StringBuilder superBlockMetadata = new StringBuilder();

            byte[] sbSector = imagePlugin.ReadSector(0 + partition.Start);

            OperaSuperBlock sb = BigEndianMarshal.ByteArrayToStructureBigEndian<OperaSuperBlock>(sbSector);
            sb.sync_bytes = new byte[5];

            if(sb.record_type != 1 || sb.record_version != 1) return;
            if(Encoding.ASCII.GetString(sb.sync_bytes) != "ZZZZZ") return;

            superBlockMetadata.AppendFormat("Opera filesystem disc.").AppendLine();
            if(!string.IsNullOrEmpty(StringHandlers.CToString(sb.volume_label, currentEncoding)))
                superBlockMetadata
                    .AppendFormat("Volume label: {0}", StringHandlers.CToString(sb.volume_label, currentEncoding))
                    .AppendLine();
            if(!string.IsNullOrEmpty(StringHandlers.CToString(sb.volume_comment, currentEncoding)))
                superBlockMetadata.AppendFormat("Volume comment: {0}",
                                                StringHandlers.CToString(sb.volume_comment, currentEncoding))
                                  .AppendLine();
            superBlockMetadata.AppendFormat("Volume identifier: 0x{0:X8}", sb.volume_id).AppendLine();
            superBlockMetadata.AppendFormat("Block size: {0} bytes", sb.block_size).AppendLine();
            if(imagePlugin.Info.SectorSize == 2336 || imagePlugin.Info.SectorSize == 2352 ||
               imagePlugin.Info.SectorSize == 2448)
            {
                if(sb.block_size != 2048)
                    superBlockMetadata
                        .AppendFormat("WARNING: Filesystem indicates {0} bytes/block while device indicates {1} bytes/block",
                                      sb.block_size, 2048);
            }
            else if(imagePlugin.Info.SectorSize != sb.block_size)
                superBlockMetadata
                    .AppendFormat("WARNING: Filesystem indicates {0} bytes/block while device indicates {1} bytes/block",
                                  sb.block_size, imagePlugin.Info.SectorSize);
            superBlockMetadata
                .AppendFormat("Volume size: {0} blocks, {1} bytes", sb.block_count, sb.block_size * sb.block_count)
                .AppendLine();
            if((ulong)sb.block_count > imagePlugin.Info.Sectors)
                superBlockMetadata
                    .AppendFormat("WARNING: Filesystem indicates {0} blocks while device indicates {1} blocks",
                                  sb.block_count, imagePlugin.Info.Sectors);
            superBlockMetadata.AppendFormat("Root directory identifier: 0x{0:X8}", sb.root_dirid).AppendLine();
            superBlockMetadata.AppendFormat("Root directory block size: {0} bytes", sb.rootdir_bsize).AppendLine();
            superBlockMetadata.AppendFormat("Root directory size: {0} blocks, {1} bytes", sb.rootdir_blocks,
                                            sb.rootdir_bsize * sb.rootdir_blocks).AppendLine();
            superBlockMetadata.AppendFormat("Last root directory copy: {0}", sb.last_root_copy).AppendLine();

            information = superBlockMetadata.ToString();

            xmlFsType = new FileSystemType
            {
                Type = "Opera",
                VolumeName = StringHandlers.CToString(sb.volume_label, currentEncoding),
                ClusterSize = sb.block_size,
                Clusters = sb.block_count
            };
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct OperaSuperBlock
        {
            /// <summary>0x000, Record type, must be 1</summary>
            public byte record_type;
            /// <summary>0x001, 5 bytes, "ZZZZZ"</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)] public byte[] sync_bytes;
            /// <summary>0x006, Record version, must be 1</summary>
            public byte record_version;
            /// <summary>0x007, Volume flags</summary>
            public byte volume_flags;
            /// <summary>0x008, 32 bytes, volume comment</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] volume_comment;
            /// <summary>0x028, 32 bytes, volume label</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] volume_label;
            /// <summary>0x048, Volume ID</summary>
            public int volume_id;
            /// <summary>0x04C, Block size in bytes</summary>
            public int block_size;
            /// <summary>0x050, Blocks in volume</summary>
            public int block_count;
            /// <summary>0x054, Root directory ID</summary>
            public int root_dirid;
            /// <summary>0x058, Root directory blocks</summary>
            public int rootdir_blocks;
            /// <summary>0x05C, Root directory block size</summary>
            public int rootdir_bsize;
            /// <summary>0x060, Last root directory copy</summary>
            public int last_root_copy;
        }
    }
}