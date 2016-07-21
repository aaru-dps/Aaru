/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : Opera.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Filesystem plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Identifies 3DO filesystems (aka Opera) and shows information.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$

using System;
using System.Text;
using DiscImageChef;
using System.Collections.Generic;

namespace DiscImageChef.Filesystems
{
    class OperaFS : Filesystem
    {
        public OperaFS()
        {
            Name = "Opera Filesystem Plugin";
            PluginUUID = new Guid("0ec84ec7-eae6-4196-83fe-943b3fe46dbd");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            if((2 + partitionStart) >= imagePlugin.GetSectors())
                return false;

            byte[] sb_sector = imagePlugin.ReadSector(0 + partitionStart);

            byte record_type;
            byte[] sync_bytes = new byte[5];
            byte record_version;

            record_type = sb_sector[0x000];
            Array.Copy(sb_sector, 0x001, sync_bytes, 0, 5);
            record_version = sb_sector[0x006];

            if(record_type != 1 || record_version != 1)
                return false;
            return Encoding.ASCII.GetString(sync_bytes) == "ZZZZZ";

        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd, out string information)
        {
            information = "";
            StringBuilder SuperBlockMetadata = new StringBuilder();

            byte[] sb_sector = imagePlugin.ReadSector(0 + partitionStart);

            OperaSuperBlock sb = new OperaSuperBlock();
            byte[] cString = new byte[32];
            sb.sync_bytes = new byte[5];

            sb.record_type = sb_sector[0x000];
            Array.Copy(sb_sector, 0x001, sb.sync_bytes, 0, 5);
            sb.record_version = sb_sector[0x006];
            sb.volume_flags = sb_sector[0x007];
            Array.Copy(sb_sector, 0x008, cString, 0, 32);
            sb.volume_comment = StringHandlers.CToString(cString);
            Array.Copy(sb_sector, 0x028, cString, 0, 32);
            sb.volume_label = StringHandlers.CToString(cString);
            sb.volume_id = BigEndianBitConverter.ToInt32(sb_sector, 0x048);
            sb.block_size = BigEndianBitConverter.ToInt32(sb_sector, 0x04C);
            sb.block_count = BigEndianBitConverter.ToInt32(sb_sector, 0x050);
            sb.root_dirid = BigEndianBitConverter.ToInt32(sb_sector, 0x054);
            sb.rootdir_blocks = BigEndianBitConverter.ToInt32(sb_sector, 0x058);
            sb.rootdir_bsize = BigEndianBitConverter.ToInt32(sb_sector, 0x05C);
            sb.last_root_copy = BigEndianBitConverter.ToInt32(sb_sector, 0x060);

            if(sb.record_type != 1 || sb.record_version != 1)
                return;
            if(Encoding.ASCII.GetString(sb.sync_bytes) != "ZZZZZ")
                return;

            if(sb.volume_comment.Length == 0)
                sb.volume_comment = "Not set.";

            if(sb.volume_label.Length == 0)
                sb.volume_label = "Not set.";

            SuperBlockMetadata.AppendFormat("Opera filesystem disc.").AppendLine();
            SuperBlockMetadata.AppendFormat("Volume label: {0}", sb.volume_label).AppendLine();
            SuperBlockMetadata.AppendFormat("Volume comment: {0}", sb.volume_comment).AppendLine();
            SuperBlockMetadata.AppendFormat("Volume identifier: 0x{0:X8}", sb.volume_id).AppendLine();
            SuperBlockMetadata.AppendFormat("Block size: {0} bytes", sb.block_size).AppendLine();
            if(imagePlugin.GetSectorSize() == 2336 || imagePlugin.GetSectorSize() == 2352 || imagePlugin.GetSectorSize() == 2448)
            {
                if(sb.block_size != 2048)
                    SuperBlockMetadata.AppendFormat("WARNING: Filesystem indicates {0} bytes/block while device indicates {1} bytes/block", sb.block_size, 2048);
            }
            else if(imagePlugin.GetSectorSize() != sb.block_size)
                SuperBlockMetadata.AppendFormat("WARNING: Filesystem indicates {0} bytes/block while device indicates {1} bytes/block", sb.block_size, imagePlugin.GetSectorSize());
            SuperBlockMetadata.AppendFormat("Volume size: {0} blocks, {1} bytes", sb.block_count, sb.block_size * sb.block_count).AppendLine();
            if((ulong)sb.block_count > imagePlugin.GetSectors())
                SuperBlockMetadata.AppendFormat("WARNING: Filesystem indicates {0} blocks while device indicates {1} blocks", sb.block_count, imagePlugin.GetSectors());
            SuperBlockMetadata.AppendFormat("Root directory identifier: 0x{0:X8}", sb.root_dirid).AppendLine();
            SuperBlockMetadata.AppendFormat("Root directory block size: {0} bytes", sb.rootdir_bsize).AppendLine();
            SuperBlockMetadata.AppendFormat("Root directory size: {0} blocks, {1} bytes", sb.rootdir_blocks, sb.rootdir_bsize * sb.rootdir_blocks).AppendLine();
            SuperBlockMetadata.AppendFormat("Last root directory copy: {0}", sb.last_root_copy).AppendLine();

            information = SuperBlockMetadata.ToString();

            xmlFSType = new Schemas.FileSystemType();
            xmlFSType.Type = "Opera";
            xmlFSType.VolumeName = sb.volume_label;
            xmlFSType.ClusterSize = sb.block_size;
            xmlFSType.Clusters = sb.block_count;
        }

        struct OperaSuperBlock
        {
            /// <summary>0x000, Record type, must be 1</summary>
            public byte record_type;
            /// <summary>0x001, 5 bytes, "ZZZZZ"</summary>
            public byte[] sync_bytes;
            /// <summary>0x006, Record version, must be 1</summary>
            public byte record_version;
            /// <summary>0x007, Volume flags</summary>
            public byte volume_flags;
            /// <summary>0x008, 32 bytes, volume comment</summary>
            public string volume_comment;
            /// <summary>0x028, 32 bytes, volume label</summary>
            public string volume_label;
            /// <summary>0x048, Volume ID</summary>
            public Int32 volume_id;
            /// <summary>0x04C, Block size in bytes</summary>
            public Int32 block_size;
            /// <summary>0x050, Blocks in volume</summary>
            public Int32 block_count;
            /// <summary>0x054, Root directory ID</summary>
            public Int32 root_dirid;
            /// <summary>0x058, Root directory blocks</summary>
            public Int32 rootdir_blocks;
            /// <summary>0x05C, Root directory block size</summary>
            public Int32 rootdir_bsize;
            /// <summary>0x060, Last root directory copy</summary>
            public Int32 last_root_copy;
        }

        public override Errno Mount()
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