/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : BFS.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Filesystem plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Identifies BeOS' filesystem and shows information.
 
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

// Information from Practical Filesystem Design, ISBN 1-55860-497-9
namespace DiscImageChef.Plugins
{
    class BeFS : Plugin
    {
        // Little endian constants (that is, as read by .NET :p)
        const UInt32 BEFS_MAGIC1 = 0x42465331;
        const UInt32 BEFS_MAGIC2 = 0xDD121031;
        const UInt32 BEFS_MAGIC3 = 0x15B6830E;
        const UInt32 BEFS_ENDIAN = 0x42494745;
        // Big endian constants
        const UInt32 BEFS_CIGAM1 = 0x31534642;
        const UInt32 BEFS_NAIDNE = 0x45474942;
        // Common constants
        const UInt32 BEFS_CLEAN = 0x434C454E;
        const UInt32 BEFS_DIRTY = 0x44495254;

        public BeFS()
        {
            Name = "Be Filesystem";
            PluginUUID = new Guid("dc8572b3-b6ad-46e4-8de9-cbe123ff6672");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            if ((2 + partitionStart) >= imagePlugin.GetSectors())
                return false;

            UInt32 magic;
            UInt32 magic_be;

            byte[] sb_sector = imagePlugin.ReadSector(0 + partitionStart);

            magic = BitConverter.ToUInt32(sb_sector, 0x20);
            magic_be = BigEndianBitConverter.ToUInt32(sb_sector, 0x20);

            if (magic == BEFS_MAGIC1 || magic_be == BEFS_MAGIC1)
                return true;

            magic = BitConverter.ToUInt32(sb_sector, 0x220);
            magic_be = BigEndianBitConverter.ToUInt32(sb_sector, 0x220);

            if (magic == BEFS_MAGIC1 || magic_be == BEFS_MAGIC1)
                return true;

            sb_sector = imagePlugin.ReadSector(1 + partitionStart);
				
            magic = BitConverter.ToUInt32(sb_sector, 0x20);
            magic_be = BigEndianBitConverter.ToUInt32(sb_sector, 0x20);

            if (magic == BEFS_MAGIC1 || magic_be == BEFS_MAGIC1)
                return true;
            return false;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd, out string information)
        {
            information = "";
            byte[] name_bytes = new byte[32];

            StringBuilder sb = new StringBuilder();
			
            BeSuperBlock besb = new BeSuperBlock();

            byte[] sb_sector = imagePlugin.ReadSector(0 + partitionStart);

            BigEndianBitConverter.IsLittleEndian = true; // Default for little-endian

            besb.magic1 = BigEndianBitConverter.ToUInt32(sb_sector, 0x20);
            if (besb.magic1 == BEFS_MAGIC1 || besb.magic1 == BEFS_CIGAM1) // Magic is at offset
            {
                BigEndianBitConverter.IsLittleEndian &= besb.magic1 != BEFS_CIGAM1;
            }
            else
            {
                sb_sector = imagePlugin.ReadSector(1 + partitionStart);
                besb.magic1 = BigEndianBitConverter.ToUInt32(sb_sector, 0x20);
				
                if (besb.magic1 == BEFS_MAGIC1 || besb.magic1 == BEFS_CIGAM1) // There is a boot sector
                {
                    BigEndianBitConverter.IsLittleEndian &= besb.magic1 != BEFS_CIGAM1;
                }
                else
                {
                    byte[] temp = imagePlugin.ReadSector(0 + partitionStart);
                    besb.magic1 = BigEndianBitConverter.ToUInt32(temp, 0x220);

                    if (besb.magic1 == BEFS_MAGIC1 || besb.magic1 == BEFS_CIGAM1) // There is a boot sector
                    {
                        BigEndianBitConverter.IsLittleEndian &= besb.magic1 != BEFS_CIGAM1;
                        sb_sector = new byte[0x200]; 
                        Array.Copy(temp, 0x200, sb_sector, 0, 0x200);
                    }
                    else
                        return;
                }
            }

            Array.Copy(sb_sector, 0x000, name_bytes, 0, 0x20);
            besb.name = StringHandlers.CToString(name_bytes);
            besb.magic1 = BigEndianBitConverter.ToUInt32(sb_sector, 0x20);
            besb.fs_byte_order = BigEndianBitConverter.ToUInt32(sb_sector, 0x24);
            besb.block_size = BigEndianBitConverter.ToUInt32(sb_sector, 0x28);
            besb.block_shift = BigEndianBitConverter.ToUInt32(sb_sector, 0x2C);
            besb.num_blocks = BigEndianBitConverter.ToInt64(sb_sector, 0x30);
            besb.used_blocks = BigEndianBitConverter.ToInt64(sb_sector, 0x38);
            besb.inode_size = BigEndianBitConverter.ToInt32(sb_sector, 0x40);
            besb.magic2 = BigEndianBitConverter.ToUInt32(sb_sector, 0x44);
            besb.blocks_per_ag = BigEndianBitConverter.ToInt32(sb_sector, 0x48);
            besb.ag_shift = BigEndianBitConverter.ToInt32(sb_sector, 0x4C);
            besb.num_ags = BigEndianBitConverter.ToInt32(sb_sector, 0x50);
            besb.flags = BigEndianBitConverter.ToUInt32(sb_sector, 0x54);
            besb.log_blocks_ag = BigEndianBitConverter.ToInt32(sb_sector, 0x58);
            besb.log_blocks_start = BigEndianBitConverter.ToUInt16(sb_sector, 0x5C);
            besb.log_blocks_len = BigEndianBitConverter.ToUInt16(sb_sector, 0x5E);
            besb.log_start = BigEndianBitConverter.ToInt64(sb_sector, 0x60);
            besb.log_end = BigEndianBitConverter.ToInt64(sb_sector, 0x68);
            besb.magic3 = BigEndianBitConverter.ToUInt32(sb_sector, 0x70);
            besb.root_dir_ag = BigEndianBitConverter.ToInt32(sb_sector, 0x74);
            besb.root_dir_start = BigEndianBitConverter.ToUInt16(sb_sector, 0x78);
            besb.root_dir_len = BigEndianBitConverter.ToUInt16(sb_sector, 0x7A);
            besb.indices_ag = BigEndianBitConverter.ToInt32(sb_sector, 0x7C);
            besb.indices_start = BigEndianBitConverter.ToUInt16(sb_sector, 0x80);
            besb.indices_len = BigEndianBitConverter.ToUInt16(sb_sector, 0x82);
			
            if (!BigEndianBitConverter.IsLittleEndian) // Big-endian filesystem
				sb.AppendLine("Big-endian BeFS");
            else
                sb.AppendLine("Little-endian BeFS");
			
            if (besb.magic1 != BEFS_MAGIC1 || besb.fs_byte_order != BEFS_ENDIAN ||
            besb.magic2 != BEFS_MAGIC2 || besb.magic3 != BEFS_MAGIC3 ||
            besb.root_dir_len != 1 || besb.indices_len != 1 ||
            (1 << (int)besb.block_shift) != besb.block_size)
            {
                sb.AppendLine("Superblock seems corrupt, following information may be incorrect");
                sb.AppendFormat("Magic 1: 0x{0:X8} (Should be 0x42465331)", besb.magic1).AppendLine();
                sb.AppendFormat("Magic 2: 0x{0:X8} (Should be 0xDD121031)", besb.magic2).AppendLine();
                sb.AppendFormat("Magic 3: 0x{0:X8} (Should be 0x15B6830E)", besb.magic3).AppendLine();
                sb.AppendFormat("Filesystem endianness: 0x{0:X8} (Should be 0x42494745)", besb.fs_byte_order).AppendLine();
                sb.AppendFormat("Root folder's i-node size: {0} blocks (Should be 1)", besb.root_dir_len).AppendLine();
                sb.AppendFormat("Indices' i-node size: {0} blocks (Should be 1)", besb.indices_len).AppendLine();
                sb.AppendFormat("1 << block_shift == block_size => 1 << {0} == {1} (Should be {2})", besb.block_shift,
                    1 << (int)besb.block_shift, besb.block_size).AppendLine();
            }
			
            if (besb.flags == BEFS_CLEAN)
            {
                if (besb.log_start == besb.log_end)
                    sb.AppendLine("Filesystem is clean");
                else
                    sb.AppendLine("Filesystem is dirty");
            }
            else if (besb.flags == BEFS_DIRTY)
                sb.AppendLine("Filesystem is dirty");
            else
                sb.AppendFormat("Unknown flags: {0:X8}", besb.flags).AppendLine();
			
            sb.AppendFormat("Volume name: {0}", besb.name).AppendLine();
            sb.AppendFormat("{0} bytes per block", besb.block_size).AppendLine();
            sb.AppendFormat("{0} blocks in volume ({1} bytes)", besb.num_blocks, besb.num_blocks * besb.block_size).AppendLine();
            sb.AppendFormat("{0} used blocks ({1} bytes)", besb.used_blocks, besb.used_blocks * besb.block_size).AppendLine();
            sb.AppendFormat("{0} bytes per i-node", besb.inode_size).AppendLine();
            sb.AppendFormat("{0} blocks per allocation group ({1} bytes)", besb.blocks_per_ag, besb.blocks_per_ag * besb.block_size).AppendLine();
            sb.AppendFormat("{0} allocation groups in volume", besb.num_ags).AppendLine();
            sb.AppendFormat("Journal resides in block {0} of allocation group {1} and runs for {2} blocks ({3} bytes)", besb.log_blocks_start,
                besb.log_blocks_ag, besb.log_blocks_len, besb.log_blocks_len * besb.block_size).AppendLine();
            sb.AppendFormat("Journal starts in byte {0} and ends in byte {1}", besb.log_start, besb.log_end).AppendLine();
            sb.AppendFormat("Root folder's i-node resides in block {0} of allocation group {1} and runs for {2} blocks ({3} bytes)", besb.root_dir_start,
                besb.root_dir_ag, besb.root_dir_len, besb.root_dir_len * besb.block_size).AppendLine();
            sb.AppendFormat("Indices' i-node resides in block {0} of allocation group {1} and runs for {2} blocks ({3} bytes)", besb.indices_start,
                besb.indices_ag, besb.indices_len, besb.indices_len * besb.block_size).AppendLine();
			
            information = sb.ToString();
        }

        struct BeSuperBlock
        {
            public string name;
            // 0x000, Volume name, 32 bytes
            public UInt32 magic1;
            // 0x020, "BFS1", 0x42465331
            public UInt32 fs_byte_order;
            // 0x024, "BIGE", 0x42494745
            public UInt32 block_size;
            // 0x028, Bytes per block
            public UInt32 block_shift;
            // 0x02C, 1 << block_shift == block_size
            public Int64 num_blocks;
            // 0x030, Blocks in volume
            public Int64 used_blocks;
            // 0x038, Used blocks in volume
            public Int32 inode_size;
            // 0x040, Bytes per inode
            public UInt32 magic2;
            // 0x044, 0xDD121031
            public Int32 blocks_per_ag;
            // 0x048, Blocks per allocation group
            public Int32 ag_shift;
            // 0x04C, 1 << ag_shift == blocks_per_ag
            public Int32 num_ags;
            // 0x050, Allocation groups in volume
            public UInt32 flags;
            // 0x054, 0x434c454e if clean, 0x44495254 if dirty
            public Int32 log_blocks_ag;
            // 0x058, Allocation group of journal
            public UInt16 log_blocks_start;
            // 0x05C, Start block of journal, inside ag
            public UInt16 log_blocks_len;
            // 0x05E, Length in blocks of journal, inside ag
            public Int64 log_start;
            // 0x060, Start of journal
            public Int64 log_end;
            // 0x068, End of journal
            public UInt32 magic3;
            // 0x070, 0x15B6830E
            public Int32 root_dir_ag;
            // 0x074, Allocation group where root folder's i-node resides
            public UInt16 root_dir_start;
            // 0x078, Start in ag of root folder's i-node
            public UInt16 root_dir_len;
            // 0x07A, As this is part of inode_addr, this is 1
            public Int32 indices_ag;
            // 0x07C, Allocation group where indices' i-node resides
            public UInt16 indices_start;
            // 0x080, Start in ag of indices' i-node
            public UInt16 indices_len;
            // 0x082, As this is part of inode_addr, this is 1
        }
    }
}