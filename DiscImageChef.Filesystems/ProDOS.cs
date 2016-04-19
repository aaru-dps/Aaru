/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : ProDOS.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Filesystem plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Identifies Apple ProDOS and Apple SOS file systems.
 
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
Copyright (C) 2011-2015 Claunia.com
****************************************************************************/
//$Id$

using System;
using System.Text;
using DiscImageChef;

// Information from Apple ProDOS 8 Technical Reference
using DiscImageChef.Console;


namespace DiscImageChef.Plugins
{
    public class ProDOSPlugin : Plugin
    {
        const byte EmptyStorageType = 0x00;
        /// <summary>
        /// A file that occupies one block or less
        /// </summary>
        const byte SeedlingFileType = 0x01;
        /// <summary>
        /// A file that occupies between 2 and 256 blocks
        /// </summary>
        const byte SaplingFileType = 0x02;
        /// <summary>
        /// A file that occupies between 257 and 32768 blocks
        /// </summary>
        const byte TreeFileType = 0x03;
        const byte PascalAreaType = 0x04;
        const byte SubDirectoryType = 0x0D;
        const byte SubDirectoryHeaderType = 0x0E;
        const byte RootDirectoryType = 0x0F;

        const byte ProDOSVersion1 = 0x00;

        const UInt32 ProDOSYearMask = 0xFE000000;
        const UInt32 ProDOSMonthMask = 0x1E00000;
        const UInt32 ProDOSDayMask = 0x1F0000;
        const UInt32 ProDOSHourMask = 0x1F00;
        const UInt32 ProDOSMinuteMask = 0x3F;

        const byte ProDOSDestroyAttribute = 0x80;
        const byte ProDOSRenameAttribute = 0x40;
        const byte ProDOSBackupAttribute = 0x20;
        const byte ProDOSWriteAttribute = 0x02;
        const byte ProDOSReadAttribute = 0x01;
        const byte ProDOSReservedAttributeMask = 0x1C;

        const byte ProDOSStorageTypeMask = 0xF0;
        const byte ProDOSNameLengthMask = 0x0F;
        const byte ProDOSEntryLength = 0x27;
        const byte ProDOSEntriesPerBlock = 0x0D;

        public ProDOSPlugin()
        {
            Name = "Apple ProDOS filesystem";
            PluginUUID = new Guid("43874265-7B8A-4739-BCF7-07F80D5932BF");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            if(imagePlugin.GetSectors() < 3)
                return false;

            // Blocks 0 and 1 are boot code
            byte[] rootDirectoryKeyBlock = imagePlugin.ReadSector(2 + partitionStart);

            UInt16 prePointer = BitConverter.ToUInt16(rootDirectoryKeyBlock, 0);
            if(prePointer != 0)
                return false;

            byte storage_type = (byte)((rootDirectoryKeyBlock[0x04] & ProDOSStorageTypeMask) >> 4);
            if(storage_type != RootDirectoryType)
                return false;

            byte entry_length = rootDirectoryKeyBlock[0x23];
            if(entry_length != ProDOSEntryLength)
                return false;

            byte entries_per_block = rootDirectoryKeyBlock[0x24];
            if(entries_per_block != ProDOSEntriesPerBlock)
                return false;

            UInt16 bit_map_pointer = BitConverter.ToUInt16(rootDirectoryKeyBlock, 0x27);
            if(bit_map_pointer > imagePlugin.GetSectors())
                return false;

            UInt16 total_blocks = BitConverter.ToUInt16(rootDirectoryKeyBlock, 0x29);
            return total_blocks <= imagePlugin.GetSectors();
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd, out string information)
        {
            StringBuilder sbInformation = new StringBuilder();

            // Blocks 0 and 1 are boot code
            byte[] rootDirectoryKeyBlockBytes = imagePlugin.ReadSector(2 + partitionStart);

            ProDOSRootDirectoryKeyBlock rootDirectoryKeyBlock = new ProDOSRootDirectoryKeyBlock();
            rootDirectoryKeyBlock.header = new ProDOSRootDirectoryHeader();

            byte[] temporal;
            int year, month, day, hour, minute;
            UInt16 temp_timestamp_left, temp_timestamp_right;
            UInt32 temp_timestamp;

            rootDirectoryKeyBlock.zero = BitConverter.ToUInt16(rootDirectoryKeyBlockBytes, 0x00);
            rootDirectoryKeyBlock.next_pointer = BitConverter.ToUInt16(rootDirectoryKeyBlockBytes, 0x02);
            rootDirectoryKeyBlock.header.storage_type = (byte)((rootDirectoryKeyBlockBytes[0x04] & ProDOSStorageTypeMask) >> 4);
            rootDirectoryKeyBlock.header.name_length = (byte)(rootDirectoryKeyBlockBytes[0x04] & ProDOSNameLengthMask);
            temporal = new byte[rootDirectoryKeyBlock.header.name_length];
            Array.Copy(rootDirectoryKeyBlockBytes, 0x05, temporal, 0, rootDirectoryKeyBlock.header.name_length);
            rootDirectoryKeyBlock.header.volume_name = Encoding.ASCII.GetString(temporal);
            rootDirectoryKeyBlock.header.reserved = BitConverter.ToUInt64(rootDirectoryKeyBlockBytes, 0x14);

            temp_timestamp_left = BitConverter.ToUInt16(rootDirectoryKeyBlockBytes, 0x1C);
            temp_timestamp_right = BitConverter.ToUInt16(rootDirectoryKeyBlockBytes, 0x1E);
            temp_timestamp = (uint)((temp_timestamp_left << 16) + temp_timestamp_right);
            year = (int)((temp_timestamp & ProDOSYearMask) >> 25);
            month = (int)((temp_timestamp & ProDOSMonthMask) >> 21);
            day = (int)((temp_timestamp & ProDOSDayMask) >> 16);
            hour = (int)((temp_timestamp & ProDOSHourMask) >> 8);
            minute = (int)(temp_timestamp & ProDOSMinuteMask);
            year += 1900;
            if(year < 1940)
                year += 100;

            DicConsole.DebugWriteLine("ProDOS plugin", "temp_timestamp_left = 0x{0:X4}", temp_timestamp_left);
            DicConsole.DebugWriteLine("ProDOS plugin", "temp_timestamp_right = 0x{0:X4}", temp_timestamp_right);
            DicConsole.DebugWriteLine("ProDOS plugin", "temp_timestamp = 0x{0:X8}", temp_timestamp);
            DicConsole.DebugWriteLine("ProDOS plugin", "Datetime field year {0}, month {1}, day {2}, hour {3}, minute {4}.", year, month, day, hour, minute);

            rootDirectoryKeyBlock.header.creation_time = new DateTime(year, month, day, hour, minute, 0);

            rootDirectoryKeyBlock.header.version = rootDirectoryKeyBlockBytes[0x20];
            rootDirectoryKeyBlock.header.min_version = rootDirectoryKeyBlockBytes[0x21];
            rootDirectoryKeyBlock.header.access = rootDirectoryKeyBlockBytes[0x22];
            rootDirectoryKeyBlock.header.entry_length = rootDirectoryKeyBlockBytes[0x23];
            rootDirectoryKeyBlock.header.entries_per_block = rootDirectoryKeyBlockBytes[0x24];

            rootDirectoryKeyBlock.header.file_count = BitConverter.ToUInt16(rootDirectoryKeyBlockBytes, 0x25);
            rootDirectoryKeyBlock.header.bit_map_pointer = BitConverter.ToUInt16(rootDirectoryKeyBlockBytes, 0x27);
            rootDirectoryKeyBlock.header.total_blocks = BitConverter.ToUInt16(rootDirectoryKeyBlockBytes, 0x29);

            if(rootDirectoryKeyBlock.header.version != ProDOSVersion1 || rootDirectoryKeyBlock.header.min_version != ProDOSVersion1)
            {
                sbInformation.AppendLine("Warning! Detected unknown ProDOS version ProDOS filesystem.");
                sbInformation.AppendLine("All of the following information may be incorrect");
            }

            if(rootDirectoryKeyBlock.header.version == ProDOSVersion1)
                sbInformation.AppendLine("ProDOS version 1 used to create this volume.");
            else
                sbInformation.AppendFormat("Unknown ProDOS version with field {0} used to create this volume.", rootDirectoryKeyBlock.header.version).AppendLine();

            if(rootDirectoryKeyBlock.header.min_version == ProDOSVersion1)
                sbInformation.AppendLine("ProDOS version 1 at least required for reading this volume.");
            else
                sbInformation.AppendFormat("Unknown ProDOS version with field {0} is at least required for reading this volume.", rootDirectoryKeyBlock.header.min_version).AppendLine();

            sbInformation.AppendFormat("Volume name is {0}", rootDirectoryKeyBlock.header.volume_name).AppendLine();
            sbInformation.AppendFormat("Volume created on {0}", rootDirectoryKeyBlock.header.creation_time).AppendLine();
            sbInformation.AppendFormat("{0} bytes per directory entry", rootDirectoryKeyBlock.header.entry_length).AppendLine();
            sbInformation.AppendFormat("{0} entries per directory block", rootDirectoryKeyBlock.header.entries_per_block).AppendLine();
            sbInformation.AppendFormat("{0} files in root directory", rootDirectoryKeyBlock.header.file_count).AppendLine();
            sbInformation.AppendFormat("{0} blocks in volume", rootDirectoryKeyBlock.header.total_blocks).AppendLine();
            sbInformation.AppendFormat("Bitmap starts at block {0}", rootDirectoryKeyBlock.header.bit_map_pointer).AppendLine();

            if((rootDirectoryKeyBlock.header.access & ProDOSReadAttribute) == ProDOSReadAttribute)
                sbInformation.AppendLine("Volume can be read");
            if((rootDirectoryKeyBlock.header.access & ProDOSWriteAttribute) == ProDOSWriteAttribute)
                sbInformation.AppendLine("Volume can be written");
            if((rootDirectoryKeyBlock.header.access & ProDOSRenameAttribute) == ProDOSRenameAttribute)
                sbInformation.AppendLine("Volume can be renamed");
            if((rootDirectoryKeyBlock.header.access & ProDOSDestroyAttribute) == ProDOSDestroyAttribute)
                sbInformation.AppendLine("Volume can be destroyed");
            if((rootDirectoryKeyBlock.header.access & ProDOSBackupAttribute) == ProDOSBackupAttribute)
                sbInformation.AppendLine("Volume must be backed up");

            if((rootDirectoryKeyBlock.header.access & ProDOSReservedAttributeMask) != 0)
                DicConsole.DebugWriteLine("ProDOS plugin", "Reserved attributes are set: {0:X2}", rootDirectoryKeyBlock.header.access);

            information = sbInformation.ToString();

            xmlFSType = new Schemas.FileSystemType();
            xmlFSType.VolumeName = rootDirectoryKeyBlock.header.volume_name;
            if(year != 0 || month != 0 || day != 0 || hour != 0 || minute != 0)
            {
                xmlFSType.CreationDate = rootDirectoryKeyBlock.header.creation_time;
                xmlFSType.CreationDateSpecified = true;
            }
            xmlFSType.Files = rootDirectoryKeyBlock.header.file_count;
            xmlFSType.FilesSpecified = true;
            xmlFSType.Clusters = rootDirectoryKeyBlock.header.total_blocks;
            xmlFSType.ClusterSize = (int)((partitionEnd - partitionStart + 1) / (ulong)xmlFSType.Clusters);
            xmlFSType.Type = "ProDOS";

            return;
        }

        /// <summary>
        /// ProDOS directory entry, decoded structure
        /// </summary>
        struct ProDOSEntry
        {
            /// <summary>
            /// Type of file pointed by this entry
            /// Offset 0x00, mask 0xF0
            /// </summary>
            public byte storage_type;
            /// <summary>
            /// Length of name_length pascal string
            /// Offset 0x00, mask 0x0F
            /// </summary>
            public byte name_length;
            /// <summary>
            /// Pascal string of file name
            /// Offset 0x01, 15 bytes
            /// </summary>
            public string file_name;
            /// <summary>
            /// Descriptor of internal structure of the file
            /// Offset 0x10, 1 byte
            /// </summary>
            public byte file_type;
            /// <summary>
            /// Block address of master index block for tree files.
            /// Block address of index block for sapling files.
            /// Block address of block for seedling files.
            /// Offset 0x11, 2 bytes
            /// </summary>
            public UInt16 key_pointer;
            /// <summary>
            /// Blocks used by file or directory, including index blocks.
            /// Offset 0x13, 2 bytes
            /// </summary>
            public UInt16 blocks_used;
            /// <summary>
            /// Size of file in bytes
            /// Offset 0x15, 3 bytes
            /// </summary>
            public UInt32 EOF;
            /// <summary>
            /// File creation datetime
            /// Offset 0x18, 4 bytes
            /// </summary>
            public DateTime creation_time;
            /// <summary>
            /// Version of ProDOS that created this file
            /// Offset 0x1C, 1 byte
            /// </summary>
            public byte version;
            /// <summary>
            /// Minimum version of ProDOS needed to access this file
            /// Offset 0x1D, 1 byte
            /// </summary>
            public byte min_version;
            /// <summary>
            /// File permissions
            /// Offset 0x1E, 1 byte
            /// </summary>
            public byte access;
            /// <summary>
            /// General purpose field to store additional information about file format
            /// Offset 0x1F, 2 bytes
            /// </summary>
            public UInt16 aux_type;
            /// <summary>
            /// File last modification date time
            /// Offset 0x21, 4 bytes
            /// </summary>
            public DateTime last_mod;
            /// <summary>
            /// Block address pointer to key block of the directory containing this entry
            /// Offset 0x25, 2 bytes
            /// </summary>
            public UInt16 header_pointer;
        }

        struct ProDOSRootDirectoryHeader
        {
            /// <summary>
            /// Constant 0x0F
            /// Offset 0x04, mask 0xF0
            /// </summary>
            public byte storage_type;
            /// <summary>
            /// Length of volume_name pascal string
            /// Offset 0x04, mask 0x0F
            /// </summary>
            public byte name_length;
            /// <summary>
            /// The name of the volume.
            /// Offset 0x05, 15 bytes
            /// </summary>
            public string volume_name;
            /// <summary>
            /// Reserved for future expansion
            /// Offset 0x14, 8 bytes
            /// </summary>
            public UInt64 reserved;
            /// <summary>
            /// Creation time of the volume
            /// Offset 0x1C, 4 bytes
            /// </summary>
            public DateTime creation_time;
            /// <summary>
            /// Version number of the volume format
            /// Offset 0x20, 1 byte
            /// </summary>
            public byte version;
            /// <summary>
            /// Reserved for future use
            /// Offset 0x21, 1 byte
            /// </summary>
            public byte min_version;
            /// <summary>
            /// Permissions for the volume
            /// Offset 0x22, 1 byte
            /// </summary>
            public byte access;
            /// <summary>
            /// Length of an entry in this directory
            /// Const 0x27
            /// Offset 0x23, 1 byte
            /// </summary>
            public byte entry_length;
            /// <summary>
            /// Number of entries per block
            /// Const 0x0D
            /// Offset 0x24, 1 byte
            /// </summary>
            public byte entries_per_block;
            /// <summary>
            /// Number of active files in this directory
            /// Offset 0x25, 2 bytes
            /// </summary>
            public UInt16 file_count;
            /// <summary>
            /// Block address of the first block of the volume's bitmap,
            /// one for every 4096 blocks or fraction
            /// Offset 0x27, 2 bytes
            /// </summary>
            public UInt16 bit_map_pointer;
            /// <summary>
            /// Total number of blocks in the volume
            /// Offset 0x29, 2 bytes
            /// </summary>
            public UInt16 total_blocks;
        }

        struct ProDOSDirectoryHeader
        {
            /// <summary>
            /// Constant 0x0E
            /// Offset 0x04, mask 0xF0
            /// </summary>
            public byte storage_type;
            /// <summary>
            /// Length of volume_name pascal string
            /// Offset 0x04, mask 0x0F
            /// </summary>
            public byte name_length;
            /// <summary>
            /// The name of the directory.
            /// Offset 0x05, 15 bytes
            /// </summary>
            public string directory_name;
            /// <summary>
            /// Reserved for future expansion
            /// Offset 0x14, 8 bytes
            /// </summary>
            public UInt64 reserved;
            /// <summary>
            /// Creation time of the volume
            /// Offset 0x1C, 4 bytes
            /// </summary>
            public DateTime creation_time;
            /// <summary>
            /// Version number of the volume format
            /// Offset 0x20, 1 byte
            /// </summary>
            public byte version;
            /// <summary>
            /// Reserved for future use
            /// Offset 0x21, 1 byte
            /// </summary>
            public byte min_version;
            /// <summary>
            /// Permissions for the volume
            /// Offset 0x22, 1 byte
            /// </summary>
            public byte access;
            /// <summary>
            /// Length of an entry in this directory
            /// Const 0x27
            /// Offset 0x23, 1 byte
            /// </summary>
            public byte entry_length;
            /// <summary>
            /// Number of entries per block
            /// Const 0x0D
            /// Offset 0x24, 1 byte
            /// </summary>
            public byte entries_per_block;
            /// <summary>
            /// Number of active files in this directory
            /// Offset 0x25, 2 bytes
            /// </summary>
            public UInt16 file_count;
            /// <summary>
            /// Block address of parent directory block that contains this entry
            /// Offset 0x27, 2 bytes
            /// </summary>
            public UInt16 parent_pointer;
            /// <summary>
            /// Entry number within the block indicated in parent_pointer
            /// Offset 0x29, 1 byte
            /// </summary>
            public byte parent_entry_number;
            /// <summary>
            /// Length of the entry that holds this directory, in the parent entry
            /// Const 0x27
            /// Offset 0x2A, 1 byte
            /// </summary>
            public byte parent_entry_length;
        }

        struct ProDOSDirectoryKeyBlock
        {
            /// <summary>
            /// Always 0
            /// Offset 0x00, 2 bytes
            /// </summary>
            public UInt16 zero;
            /// <summary>
            /// Pointer to next directory block, 0 if last
            /// Offset 0x02, 2 bytes
            /// </summary>
            public UInt16 next_pointer;
            /// <summary>
            /// Directory header
            /// Offset 0x04, 39 bytes
            /// </summary>
            public ProDOSDirectoryHeader header;
            /// <summary>
            /// Directory entries
            /// Offset 0x2F, 39 bytes each, 12 entries
            /// </summary>
            public ProDOSEntry[] entries;
        }

        struct ProDOSRootDirectoryKeyBlock
        {
            /// <summary>
            /// Always 0
            /// Offset 0x00, 2 bytes
            /// </summary>
            public UInt16 zero;
            /// <summary>
            /// Pointer to next directory block, 0 if last
            /// Offset 0x02, 2 bytes
            /// </summary>
            public UInt16 next_pointer;
            /// <summary>
            /// Directory header
            /// Offset 0x04, 39 bytes
            /// </summary>
            public ProDOSRootDirectoryHeader header;
            /// <summary>
            /// Directory entries
            /// Offset 0x2F, 39 bytes each, 12 entries
            /// </summary>
            public ProDOSEntry[] entries;
        }

        struct ProDOSDirectoryBlock
        {
            /// <summary>
            /// Pointer to previous directory block
            /// Offset 0x00, 2 bytes
            /// </summary>
            public UInt16 zero;
            /// <summary>
            /// Pointer to next directory block, 0 if last
            /// Offset 0x02, 2 bytes
            /// </summary>
            public UInt16 next_pointer;
            /// <summary>
            /// Directory entries
            /// Offset 0x2F, 39 bytes each, 13 entries
            /// </summary>
            public ProDOSEntry[] entries;
        }

        struct ProDOSIndexBlock
        {
            /// <summary>
            /// Up to 256 pointers to blocks, 0 to indicate the block is sparsed (non-allocated)
            /// </summary>
            public UInt16[] block_pointer;
        }

        struct ProDOSMasterIndexBlock
        {
            /// <summary>
            /// Up to 128 pointers to index blocks
            /// </summary>
            public UInt16[] index_block_pointer;
        }
    }
}

