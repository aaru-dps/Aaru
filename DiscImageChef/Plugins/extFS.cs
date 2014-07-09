/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : extFS.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Filesystem plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Identifies Linux extended filesystem and shows information.
 
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

// Information from the Linux kernel
namespace DiscImageChef.Plugins
{
    class extFS : Plugin
    {
        public extFS(PluginBase Core)
        {
            Name = "Linux extended Filesystem";
            PluginUUID = new Guid("076CB3A2-08C2-4D69-BC8A-FCAA2E502BE2");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset)
        {
            if ((2 + partitionOffset) >= imagePlugin.GetSectors())
                return false;

            byte[] sb_sector = imagePlugin.ReadSector(2 + partitionOffset); // Superblock resides at 0x400

            UInt16 magic = BitConverter.ToUInt16(sb_sector, 0x038); // Here should reside magic number
			
            return magic == extFSMagic;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset, out string information)
        {
            information = "";
			
            StringBuilder sb = new StringBuilder();

            byte[] sb_sector = imagePlugin.ReadSector(2 + partitionOffset); // Superblock resides at 0x400
            extFSSuperBlock ext_sb = new extFSSuperBlock();

            ext_sb.inodes = BitConverter.ToUInt32(sb_sector, 0x000);
            ext_sb.zones = BitConverter.ToUInt32(sb_sector, 0x004);
            ext_sb.firstfreeblk = BitConverter.ToUInt32(sb_sector, 0x008);
            ext_sb.freecountblk = BitConverter.ToUInt32(sb_sector, 0x00C);
            ext_sb.firstfreeind = BitConverter.ToUInt32(sb_sector, 0x010);
            ext_sb.freecountind = BitConverter.ToUInt32(sb_sector, 0x014);
            ext_sb.firstdatazone = BitConverter.ToUInt32(sb_sector, 0x018);
            ext_sb.logzonesize = BitConverter.ToUInt32(sb_sector, 0x01C);
            ext_sb.maxsize = BitConverter.ToUInt32(sb_sector, 0x020);

            sb.AppendLine("ext filesystem");
            sb.AppendFormat("{0} zones on volume", ext_sb.zones);
            sb.AppendFormat("{0} free blocks ({1} bytes)", ext_sb.freecountblk, ext_sb.freecountblk * 1024);
            sb.AppendFormat("{0} inodes on volume, {1} free ({2}%)", ext_sb.inodes, ext_sb.freecountind, ext_sb.freecountind * 100 / ext_sb.inodes);
            sb.AppendFormat("First free inode is {0}", ext_sb.firstfreeind);
            sb.AppendFormat("First free block is {0}", ext_sb.firstfreeblk);
            sb.AppendFormat("First data zone is {0}", ext_sb.firstdatazone);
            sb.AppendFormat("Log zone size: {0}", ext_sb.logzonesize);
            sb.AppendFormat("Max zone size: {0}", ext_sb.maxsize);

            information = sb.ToString();
        }

        public const UInt16 extFSMagic = 0x137D;

        public struct extFSSuperBlock
        {
            public UInt32 inodes;
            // 0x000, inodes on volume
            public UInt32 zones;
            // 0x004, zones on volume
            public UInt32 firstfreeblk;
            // 0x008, first free block
            public UInt32 freecountblk;
            // 0x00C, free blocks count
            public UInt32 firstfreeind;
            // 0x010, first free inode
            public UInt32 freecountind;
            // 0x014, free inodes count
            public UInt32 firstdatazone;
            // 0x018, first data zone
            public UInt32 logzonesize;
            // 0x01C, log zone size
            public UInt32 maxsize;
            // 0x020, max zone size
            public UInt32 reserved1;
            // 0x024, reserved
            public UInt32 reserved2;
            // 0x028, reserved
            public UInt32 reserved3;
            // 0x02C, reserved
            public UInt32 reserved4;
            // 0x030, reserved
            public UInt32 reserved5;
            // 0x034, reserved
            public UInt16 magic;
            // 0x038, 0x137D (little endian)
        }
    }
}