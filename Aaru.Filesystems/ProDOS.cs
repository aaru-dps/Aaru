// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ProDOS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple ProDOS filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Apple ProDOS filesystem and shows information.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Claunia.Encoding;
using Schemas;
using Encoding = System.Text.Encoding;

// ReSharper disable NotAccessedField.Local

namespace Aaru.Filesystems
{
    // Information from Apple ProDOS 8 Technical Reference
    /// <inheritdoc />
    /// <summary>Implements detection of Apple ProDOS filesystem</summary>
    [SuppressMessage("ReSharper", "UnusedMember.Local"), SuppressMessage("ReSharper", "UnusedType.Local")]
    public sealed class ProDOSPlugin : IFilesystem
    {
        const byte EMPTY_STORAGE_TYPE = 0x00;
        /// <summary>A file that occupies one block or less</summary>
        const byte SEEDLING_FILE_TYPE = 0x01;
        /// <summary>A file that occupies between 2 and 256 blocks</summary>
        const byte SAPLING_FILE_TYPE = 0x02;
        /// <summary>A file that occupies between 257 and 32768 blocks</summary>
        const byte TREE_FILE_TYPE = 0x03;
        const byte PASCAL_AREA_TYPE         = 0x04;
        const byte SUBDIRECTORY_TYPE        = 0x0D;
        const byte SUBDIRECTORY_HEADER_TYPE = 0x0E;
        const byte ROOT_DIRECTORY_TYPE      = 0x0F;

        const byte VERSION1 = 0x00;

        const uint YEAR_MASK   = 0xFE000000;
        const uint MONTH_MASK  = 0x1E00000;
        const uint DAY_MASK    = 0x1F0000;
        const uint HOUR_MASK   = 0x1F00;
        const uint MINUTE_MASK = 0x3F;

        const byte DESTROY_ATTRIBUTE       = 0x80;
        const byte RENAME_ATTRIBUTE        = 0x40;
        const byte BACKUP_ATTRIBUTE        = 0x20;
        const byte WRITE_ATTRIBUTE         = 0x02;
        const byte READ_ATTRIBUTE          = 0x01;
        const byte RESERVED_ATTRIBUTE_MASK = 0x1C;

        const byte STORAGE_TYPE_MASK = 0xF0;
        const byte NAME_LENGTH_MASK  = 0x0F;
        const byte ENTRY_LENGTH      = 0x27;
        const byte ENTRIES_PER_BLOCK = 0x0D;

        /// <inheritdoc />
        public FileSystemType XmlFsType { get; private set; }
        /// <inheritdoc />
        public Encoding Encoding { get; private set; }
        /// <inheritdoc />
        public string Name => "Apple ProDOS filesystem";
        /// <inheritdoc />
        public Guid Id => new Guid("43874265-7B8A-4739-BCF7-07F80D5932BF");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

        /// <inheritdoc />
        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(partition.Length < 3)
                return false;

            uint multiplier = (uint)(imagePlugin.Info.SectorSize == 256 ? 2 : 1);

            // Blocks 0 and 1 are boot code
            byte[] rootDirectoryKeyBlock = imagePlugin.ReadSectors((2 * multiplier) + partition.Start, multiplier);
            bool   apmFromHddOnCd        = false;

            if(imagePlugin.Info.SectorSize == 2352 ||
               imagePlugin.Info.SectorSize == 2448 ||
               imagePlugin.Info.SectorSize == 2048)
            {
                byte[] tmp = imagePlugin.ReadSectors(partition.Start, 2);

                foreach(int offset in new[]
                {
                    0, 0x200, 0x400, 0x600, 0x800, 0xA00
                }.Where(offset => tmp.Length > offset + 0x200 && BitConverter.ToUInt16(tmp, offset) == 0 &&
                                  (byte)((tmp[offset + 0x04] & STORAGE_TYPE_MASK) >> 4) == ROOT_DIRECTORY_TYPE &&
                                  tmp[offset + 0x23] == ENTRY_LENGTH && tmp[offset + 0x24] == ENTRIES_PER_BLOCK))
                {
                    Array.Copy(tmp, offset, rootDirectoryKeyBlock, 0, 0x200);
                    apmFromHddOnCd = true;

                    break;
                }
            }

            ushort prePointer = BitConverter.ToUInt16(rootDirectoryKeyBlock, 0);
            AaruConsole.DebugWriteLine("ProDOS plugin", "prePointer = {0}", prePointer);

            if(prePointer != 0)
                return false;

            byte storageType = (byte)((rootDirectoryKeyBlock[0x04] & STORAGE_TYPE_MASK) >> 4);
            AaruConsole.DebugWriteLine("ProDOS plugin", "storage_type = {0}", storageType);

            if(storageType != ROOT_DIRECTORY_TYPE)
                return false;

            byte entryLength = rootDirectoryKeyBlock[0x23];
            AaruConsole.DebugWriteLine("ProDOS plugin", "entry_length = {0}", entryLength);

            if(entryLength != ENTRY_LENGTH)
                return false;

            byte entriesPerBlock = rootDirectoryKeyBlock[0x24];
            AaruConsole.DebugWriteLine("ProDOS plugin", "entries_per_block = {0}", entriesPerBlock);

            if(entriesPerBlock != ENTRIES_PER_BLOCK)
                return false;

            ushort bitMapPointer = BitConverter.ToUInt16(rootDirectoryKeyBlock, 0x27);
            AaruConsole.DebugWriteLine("ProDOS plugin", "bit_map_pointer = {0}", bitMapPointer);

            if(bitMapPointer > partition.End)
                return false;

            ushort totalBlocks = BitConverter.ToUInt16(rootDirectoryKeyBlock, 0x29);

            if(apmFromHddOnCd)
                totalBlocks /= 4;

            AaruConsole.DebugWriteLine("ProDOS plugin", "{0} <= ({1} - {2} + 1)? {3}", totalBlocks, partition.End,
                                       partition.Start, totalBlocks <= partition.End - partition.Start + 1);

            return totalBlocks <= partition.End - partition.Start + 1;
        }

        /// <inheritdoc />
        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding = encoding ?? new Apple2c();
            var  sbInformation = new StringBuilder();
            uint multiplier    = (uint)(imagePlugin.Info.SectorSize == 256 ? 2 : 1);

            // Blocks 0 and 1 are boot code
            byte[] rootDirectoryKeyBlockBytes = imagePlugin.ReadSectors((2 * multiplier) + partition.Start, multiplier);

            bool apmFromHddOnCd = false;

            if(imagePlugin.Info.SectorSize == 2352 ||
               imagePlugin.Info.SectorSize == 2448 ||
               imagePlugin.Info.SectorSize == 2048)
            {
                byte[] tmp = imagePlugin.ReadSectors(partition.Start, 2);

                foreach(int offset in new[]
                {
                    0, 0x200, 0x400, 0x600, 0x800, 0xA00
                }.Where(offset => BitConverter.ToUInt16(tmp, offset) == 0 &&
                                  (byte)((tmp[offset + 0x04] & STORAGE_TYPE_MASK) >> 4) == ROOT_DIRECTORY_TYPE &&
                                  tmp[offset + 0x23] == ENTRY_LENGTH && tmp[offset + 0x24] == ENTRIES_PER_BLOCK))
                {
                    Array.Copy(tmp, offset, rootDirectoryKeyBlockBytes, 0, 0x200);
                    apmFromHddOnCd = true;

                    break;
                }
            }

            var rootDirectoryKeyBlock = new RootDirectoryKeyBlock
            {
                header       = new RootDirectoryHeader(),
                zero         = BitConverter.ToUInt16(rootDirectoryKeyBlockBytes, 0x00),
                next_pointer = BitConverter.ToUInt16(rootDirectoryKeyBlockBytes, 0x02)
            };

            rootDirectoryKeyBlock.header.storage_type =
                (byte)((rootDirectoryKeyBlockBytes[0x04] & STORAGE_TYPE_MASK) >> 4);

            rootDirectoryKeyBlock.header.name_length = (byte)(rootDirectoryKeyBlockBytes[0x04] & NAME_LENGTH_MASK);
            byte[] temporal = new byte[rootDirectoryKeyBlock.header.name_length];
            Array.Copy(rootDirectoryKeyBlockBytes, 0x05, temporal, 0, rootDirectoryKeyBlock.header.name_length);
            rootDirectoryKeyBlock.header.volume_name = Encoding.GetString(temporal);
            rootDirectoryKeyBlock.header.reserved    = BitConverter.ToUInt64(rootDirectoryKeyBlockBytes, 0x14);

            ushort tempTimestampLeft  = BitConverter.ToUInt16(rootDirectoryKeyBlockBytes, 0x1C);
            ushort tempTimestampRight = BitConverter.ToUInt16(rootDirectoryKeyBlockBytes, 0x1E);

            bool dateCorrect;

            try
            {
                uint tempTimestamp = (uint)((tempTimestampLeft << 16) + tempTimestampRight);
                int  year          = (int)((tempTimestamp & YEAR_MASK)  >> 25);
                int  month         = (int)((tempTimestamp & MONTH_MASK) >> 21);
                int  day           = (int)((tempTimestamp & DAY_MASK)   >> 16);
                int  hour          = (int)((tempTimestamp & HOUR_MASK)  >> 8);
                int  minute        = (int)(tempTimestamp & MINUTE_MASK);
                year += 1900;

                if(year < 1940)
                    year += 100;

                AaruConsole.DebugWriteLine("ProDOS plugin", "temp_timestamp_left = 0x{0:X4}", tempTimestampLeft);
                AaruConsole.DebugWriteLine("ProDOS plugin", "temp_timestamp_right = 0x{0:X4}", tempTimestampRight);
                AaruConsole.DebugWriteLine("ProDOS plugin", "temp_timestamp = 0x{0:X8}", tempTimestamp);

                AaruConsole.DebugWriteLine("ProDOS plugin",
                                           "Datetime field year {0}, month {1}, day {2}, hour {3}, minute {4}.", year,
                                           month, day, hour, minute);

                rootDirectoryKeyBlock.header.creation_time = new DateTime(year, month, day, hour, minute, 0);
                dateCorrect                                = true;
            }
            catch(ArgumentOutOfRangeException)
            {
                dateCorrect = false;
            }

            rootDirectoryKeyBlock.header.version           = rootDirectoryKeyBlockBytes[0x20];
            rootDirectoryKeyBlock.header.min_version       = rootDirectoryKeyBlockBytes[0x21];
            rootDirectoryKeyBlock.header.access            = rootDirectoryKeyBlockBytes[0x22];
            rootDirectoryKeyBlock.header.entry_length      = rootDirectoryKeyBlockBytes[0x23];
            rootDirectoryKeyBlock.header.entries_per_block = rootDirectoryKeyBlockBytes[0x24];

            rootDirectoryKeyBlock.header.file_count      = BitConverter.ToUInt16(rootDirectoryKeyBlockBytes, 0x25);
            rootDirectoryKeyBlock.header.bit_map_pointer = BitConverter.ToUInt16(rootDirectoryKeyBlockBytes, 0x27);
            rootDirectoryKeyBlock.header.total_blocks    = BitConverter.ToUInt16(rootDirectoryKeyBlockBytes, 0x29);

            if(apmFromHddOnCd)
                sbInformation.AppendLine("ProDOS uses 512 bytes/sector while devices uses 2048 bytes/sector.").
                              AppendLine();

            if(rootDirectoryKeyBlock.header.version     != VERSION1 ||
               rootDirectoryKeyBlock.header.min_version != VERSION1)
            {
                sbInformation.AppendLine("Warning! Detected unknown ProDOS version ProDOS filesystem.");
                sbInformation.AppendLine("All of the following information may be incorrect");
            }

            if(rootDirectoryKeyBlock.header.version == VERSION1)
                sbInformation.AppendLine("ProDOS version 1 used to create this volume.");
            else
                sbInformation.AppendFormat("Unknown ProDOS version with field {0} used to create this volume.",
                                           rootDirectoryKeyBlock.header.version).AppendLine();

            if(rootDirectoryKeyBlock.header.min_version == VERSION1)
                sbInformation.AppendLine("ProDOS version 1 at least required for reading this volume.");
            else
                sbInformation.
                    AppendFormat("Unknown ProDOS version with field {0} is at least required for reading this volume.",
                                 rootDirectoryKeyBlock.header.min_version).AppendLine();

            sbInformation.AppendFormat("Volume name is {0}", rootDirectoryKeyBlock.header.volume_name).AppendLine();

            if(dateCorrect)
                sbInformation.AppendFormat("Volume created on {0}", rootDirectoryKeyBlock.header.creation_time).
                              AppendLine();

            sbInformation.AppendFormat("{0} bytes per directory entry", rootDirectoryKeyBlock.header.entry_length).
                          AppendLine();

            sbInformation.
                AppendFormat("{0} entries per directory block", rootDirectoryKeyBlock.header.entries_per_block).
                AppendLine();

            sbInformation.AppendFormat("{0} files in root directory", rootDirectoryKeyBlock.header.file_count).
                          AppendLine();

            sbInformation.AppendFormat("{0} blocks in volume", rootDirectoryKeyBlock.header.total_blocks).AppendLine();

            sbInformation.AppendFormat("Bitmap starts at block {0}", rootDirectoryKeyBlock.header.bit_map_pointer).
                          AppendLine();

            if((rootDirectoryKeyBlock.header.access & READ_ATTRIBUTE) == READ_ATTRIBUTE)
                sbInformation.AppendLine("Volume can be read");

            if((rootDirectoryKeyBlock.header.access & WRITE_ATTRIBUTE) == WRITE_ATTRIBUTE)
                sbInformation.AppendLine("Volume can be written");

            if((rootDirectoryKeyBlock.header.access & RENAME_ATTRIBUTE) == RENAME_ATTRIBUTE)
                sbInformation.AppendLine("Volume can be renamed");

            if((rootDirectoryKeyBlock.header.access & DESTROY_ATTRIBUTE) == DESTROY_ATTRIBUTE)
                sbInformation.AppendLine("Volume can be destroyed");

            if((rootDirectoryKeyBlock.header.access & BACKUP_ATTRIBUTE) == BACKUP_ATTRIBUTE)
                sbInformation.AppendLine("Volume must be backed up");

            if((rootDirectoryKeyBlock.header.access & RESERVED_ATTRIBUTE_MASK) != 0)
                AaruConsole.DebugWriteLine("ProDOS plugin", "Reserved attributes are set: {0:X2}",
                                           rootDirectoryKeyBlock.header.access);

            information = sbInformation.ToString();

            XmlFsType = new FileSystemType
            {
                VolumeName     = rootDirectoryKeyBlock.header.volume_name,
                Files          = rootDirectoryKeyBlock.header.file_count,
                FilesSpecified = true,
                Clusters       = rootDirectoryKeyBlock.header.total_blocks,
                Type           = "ProDOS"
            };

            XmlFsType.ClusterSize = (uint)((partition.End - partition.Start + 1) * imagePlugin.Info.SectorSize /
                                           XmlFsType.Clusters);

            if(!dateCorrect)
                return;

            XmlFsType.CreationDate          = rootDirectoryKeyBlock.header.creation_time;
            XmlFsType.CreationDateSpecified = true;
        }

        /// <summary>ProDOS directory entry, decoded structure</summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        struct Entry
        {
            /// <summary>Type of file pointed by this entry Offset 0x00, mask 0xF0</summary>
            public byte storage_type;
            /// <summary>Length of name_length pascal string Offset 0x00, mask 0x0F</summary>
            public byte name_length;
            /// <summary>Pascal string of file name Offset 0x01, 15 bytes</summary>
            public string file_name;
            /// <summary>Descriptor of internal structure of the file Offset 0x10, 1 byte</summary>
            public byte file_type;
            /// <summary>
            ///     Block address of master index block for tree files. Block address of index block for sapling files. Block
            ///     address of block for seedling files. Offset 0x11, 2 bytes
            /// </summary>
            public ushort key_pointer;
            /// <summary>Blocks used by file or directory, including index blocks. Offset 0x13, 2 bytes</summary>
            public ushort blocks_used;
            /// <summary>Size of file in bytes Offset 0x15, 3 bytes</summary>
            public uint EOF;
            /// <summary>File creation datetime Offset 0x18, 4 bytes</summary>
            public DateTime creation_time;
            /// <summary>Version of ProDOS that created this file Offset 0x1C, 1 byte</summary>
            public byte version;
            /// <summary>Minimum version of ProDOS needed to access this file Offset 0x1D, 1 byte</summary>
            public byte min_version;
            /// <summary>File permissions Offset 0x1E, 1 byte</summary>
            public byte access;
            /// <summary>General purpose field to store additional information about file format Offset 0x1F, 2 bytes</summary>
            public ushort aux_type;
            /// <summary>File last modification date time Offset 0x21, 4 bytes</summary>
            public DateTime last_mod;
            /// <summary>Block address pointer to key block of the directory containing this entry Offset 0x25, 2 bytes</summary>
            public ushort header_pointer;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        struct RootDirectoryHeader
        {
            /// <summary>Constant 0x0F Offset 0x04, mask 0xF0</summary>
            public byte storage_type;
            /// <summary>Length of volume_name pascal string Offset 0x04, mask 0x0F</summary>
            public byte name_length;
            /// <summary>The name of the volume. Offset 0x05, 15 bytes</summary>
            public string volume_name;
            /// <summary>Reserved for future expansion Offset 0x14, 8 bytes</summary>
            public ulong reserved;
            /// <summary>Creation time of the volume Offset 0x1C, 4 bytes</summary>
            public DateTime creation_time;
            /// <summary>Version number of the volume format Offset 0x20, 1 byte</summary>
            public byte version;
            /// <summary>Reserved for future use Offset 0x21, 1 byte</summary>
            public byte min_version;
            /// <summary>Permissions for the volume Offset 0x22, 1 byte</summary>
            public byte access;
            /// <summary>Length of an entry in this directory Const 0x27 Offset 0x23, 1 byte</summary>
            public byte entry_length;
            /// <summary>Number of entries per block Const 0x0D Offset 0x24, 1 byte</summary>
            public byte entries_per_block;
            /// <summary>Number of active files in this directory Offset 0x25, 2 bytes</summary>
            public ushort file_count;
            /// <summary>
            ///     Block address of the first block of the volume's bitmap, one for every 4096 blocks or fraction Offset 0x27, 2
            ///     bytes
            /// </summary>
            public ushort bit_map_pointer;
            /// <summary>Total number of blocks in the volume Offset 0x29, 2 bytes</summary>
            public ushort total_blocks;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        struct DirectoryHeader
        {
            /// <summary>Constant 0x0E Offset 0x04, mask 0xF0</summary>
            public byte storage_type;
            /// <summary>Length of volume_name pascal string Offset 0x04, mask 0x0F</summary>
            public byte name_length;
            /// <summary>The name of the directory. Offset 0x05, 15 bytes</summary>
            public string directory_name;
            /// <summary>Reserved for future expansion Offset 0x14, 8 bytes</summary>
            public ulong reserved;
            /// <summary>Creation time of the volume Offset 0x1C, 4 bytes</summary>
            public DateTime creation_time;
            /// <summary>Version number of the volume format Offset 0x20, 1 byte</summary>
            public byte version;
            /// <summary>Reserved for future use Offset 0x21, 1 byte</summary>
            public byte min_version;
            /// <summary>Permissions for the volume Offset 0x22, 1 byte</summary>
            public byte access;
            /// <summary>Length of an entry in this directory Const 0x27 Offset 0x23, 1 byte</summary>
            public byte entry_length;
            /// <summary>Number of entries per block Const 0x0D Offset 0x24, 1 byte</summary>
            public byte entries_per_block;
            /// <summary>Number of active files in this directory Offset 0x25, 2 bytes</summary>
            public ushort file_count;
            /// <summary>Block address of parent directory block that contains this entry Offset 0x27, 2 bytes</summary>
            public ushort parent_pointer;
            /// <summary>Entry number within the block indicated in parent_pointer Offset 0x29, 1 byte</summary>
            public byte parent_entry_number;
            /// <summary>Length of the entry that holds this directory, in the parent entry Const 0x27 Offset 0x2A, 1 byte</summary>
            public byte parent_entry_length;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        struct DirectoryKeyBlock
        {
            /// <summary>Always 0 Offset 0x00, 2 bytes</summary>
            public ushort zero;
            /// <summary>Pointer to next directory block, 0 if last Offset 0x02, 2 bytes</summary>
            public ushort next_pointer;
            /// <summary>Directory header Offset 0x04, 39 bytes</summary>
            public DirectoryHeader header;
            /// <summary>Directory entries Offset 0x2F, 39 bytes each, 12 entries</summary>
            public Entry[] entries;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        struct RootDirectoryKeyBlock
        {
            /// <summary>Always 0 Offset 0x00, 2 bytes</summary>
            public ushort zero;
            /// <summary>Pointer to next directory block, 0 if last Offset 0x02, 2 bytes</summary>
            public ushort next_pointer;
            /// <summary>Directory header Offset 0x04, 39 bytes</summary>
            public RootDirectoryHeader header;
            /// <summary>Directory entries Offset 0x2F, 39 bytes each, 12 entries</summary>
            public Entry[] entries;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        struct DirectoryBlock
        {
            /// <summary>Pointer to previous directory block Offset 0x00, 2 bytes</summary>
            public ushort zero;
            /// <summary>Pointer to next directory block, 0 if last Offset 0x02, 2 bytes</summary>
            public ushort next_pointer;
            /// <summary>Directory entries Offset 0x2F, 39 bytes each, 13 entries</summary>
            public Entry[] entries;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        struct IndexBlock
        {
            /// <summary>Up to 256 pointers to blocks, 0 to indicate the block is sparsed (non-allocated)</summary>
            public ushort[] block_pointer;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        struct MasterIndexBlock
        {
            /// <summary>Up to 128 pointers to index blocks</summary>
            public ushort[] index_block_pointer;
        }
    }
}