// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple ProDOS filesystem plugin.
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

// ReSharper disable NotAccessedField.Local

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Claunia.Encoding;
using Encoding = System.Text.Encoding;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

// Information from Apple ProDOS 8 Technical Reference
/// <inheritdoc />
/// <summary>Implements detection of Apple ProDOS filesystem</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "UnusedType.Local")]
public sealed partial class ProDOSPlugin
{
#region IFilesystem Members

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(partition.Length < 3) return false;

        var multiplier = (uint)(imagePlugin.Info.SectorSize == 256 ? 2 : 1);

        // Blocks 0 and 1 are boot code
        ErrorNumber errno =
            imagePlugin.ReadSectors(2 * multiplier + partition.Start, multiplier, out byte[] rootDirectoryKeyBlock);

        if(errno != ErrorNumber.NoError) return false;

        var apmFromHddOnCd = false;

        if(imagePlugin.Info.SectorSize is 2352 or 2448 or 2048)
        {
            errno = imagePlugin.ReadSectors(partition.Start, 2, out byte[] tmp);

            if(errno != ErrorNumber.NoError) return false;

            foreach(int offset in new[]
                    {
                        0, 0x200, 0x400, 0x600, 0x800, 0xA00
                    }.Where(offset => tmp.Length                                            > offset + 0x200       &&
                                      BitConverter.ToUInt16(tmp, offset)                    == 0                   &&
                                      (byte)((tmp[offset + 0x04] & STORAGE_TYPE_MASK) >> 4) == ROOT_DIRECTORY_TYPE &&
                                      tmp[offset + 0x23]                                    == ENTRY_LENGTH        &&
                                      tmp[offset + 0x24]                                    == ENTRIES_PER_BLOCK))
            {
                Array.Copy(tmp, offset, rootDirectoryKeyBlock, 0, 0x200);
                apmFromHddOnCd = true;

                break;
            }
        }

        var prePointer = BitConverter.ToUInt16(rootDirectoryKeyBlock, 0);
        AaruConsole.DebugWriteLine(MODULE_NAME, "prePointer = {0}", prePointer);

        if(prePointer != 0) return false;

        var storageType = (byte)((rootDirectoryKeyBlock[0x04] & STORAGE_TYPE_MASK) >> 4);
        AaruConsole.DebugWriteLine(MODULE_NAME, "storage_type = {0}", storageType);

        if(storageType != ROOT_DIRECTORY_TYPE) return false;

        byte entryLength = rootDirectoryKeyBlock[0x23];
        AaruConsole.DebugWriteLine(MODULE_NAME, "entry_length = {0}", entryLength);

        if(entryLength != ENTRY_LENGTH) return false;

        byte entriesPerBlock = rootDirectoryKeyBlock[0x24];
        AaruConsole.DebugWriteLine(MODULE_NAME, "entries_per_block = {0}", entriesPerBlock);

        if(entriesPerBlock != ENTRIES_PER_BLOCK) return false;

        var bitMapPointer = BitConverter.ToUInt16(rootDirectoryKeyBlock, 0x27);
        AaruConsole.DebugWriteLine(MODULE_NAME, "bit_map_pointer = {0}", bitMapPointer);

        if(bitMapPointer > partition.End) return false;

        var totalBlocks = BitConverter.ToUInt16(rootDirectoryKeyBlock, 0x29);

        if(apmFromHddOnCd) totalBlocks /= 4;

        AaruConsole.DebugWriteLine(MODULE_NAME,
                                   "{0} <= ({1} - {2} + 1)? {3}",
                                   totalBlocks,
                                   partition.End,
                                   partition.Start,
                                   totalBlocks <= partition.End - partition.Start + 1);

        return totalBlocks <= partition.End - partition.Start + 1;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        encoding    ??= new Apple2c();
        information =   "";
        metadata    =   new FileSystem();
        var sbInformation = new StringBuilder();
        var multiplier    = (uint)(imagePlugin.Info.SectorSize == 256 ? 2 : 1);

        // Blocks 0 and 1 are boot code
        ErrorNumber errno = imagePlugin.ReadSectors(2 * multiplier + partition.Start,
                                                    multiplier,
                                                    out byte[] rootDirectoryKeyBlockBytes);

        if(errno != ErrorNumber.NoError) return;

        var apmFromHddOnCd = false;

        if(imagePlugin.Info.SectorSize is 2352 or 2448 or 2048)
        {
            errno = imagePlugin.ReadSectors(partition.Start, 2, out byte[] tmp);

            if(errno != ErrorNumber.NoError) return;

            foreach(int offset in new[]
                    {
                        0, 0x200, 0x400, 0x600, 0x800, 0xA00
                    }.Where(offset => BitConverter.ToUInt16(tmp, offset)                    == 0                   &&
                                      (byte)((tmp[offset + 0x04] & STORAGE_TYPE_MASK) >> 4) == ROOT_DIRECTORY_TYPE &&
                                      tmp[offset + 0x23]                                    == ENTRY_LENGTH        &&
                                      tmp[offset + 0x24]                                    == ENTRIES_PER_BLOCK))
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

        rootDirectoryKeyBlock.header.storage_type = (byte)((rootDirectoryKeyBlockBytes[0x04] & STORAGE_TYPE_MASK) >> 4);

        rootDirectoryKeyBlock.header.name_length = (byte)(rootDirectoryKeyBlockBytes[0x04] & NAME_LENGTH_MASK);
        var temporal = new byte[rootDirectoryKeyBlock.header.name_length];
        Array.Copy(rootDirectoryKeyBlockBytes, 0x05, temporal, 0, rootDirectoryKeyBlock.header.name_length);
        rootDirectoryKeyBlock.header.volume_name = encoding.GetString(temporal);
        rootDirectoryKeyBlock.header.reserved    = BitConverter.ToUInt64(rootDirectoryKeyBlockBytes, 0x14);

        var tempTimestampLeft  = BitConverter.ToUInt16(rootDirectoryKeyBlockBytes, 0x1C);
        var tempTimestampRight = BitConverter.ToUInt16(rootDirectoryKeyBlockBytes, 0x1E);

        bool dateCorrect;

        try
        {
            var tempTimestamp = (uint)((tempTimestampLeft << 16) + tempTimestampRight);
            var year          = (int)((tempTimestamp & YEAR_MASK)  >> 25);
            var month         = (int)((tempTimestamp & MONTH_MASK) >> 21);
            var day           = (int)((tempTimestamp & DAY_MASK)   >> 16);
            var hour          = (int)((tempTimestamp & HOUR_MASK)  >> 8);
            var minute        = (int)(tempTimestamp & MINUTE_MASK);
            year += 1900;

            if(year < 1940) year += 100;

            AaruConsole.DebugWriteLine(MODULE_NAME, "temp_timestamp_left = 0x{0:X4}",  tempTimestampLeft);
            AaruConsole.DebugWriteLine(MODULE_NAME, "temp_timestamp_right = 0x{0:X4}", tempTimestampRight);
            AaruConsole.DebugWriteLine(MODULE_NAME, "temp_timestamp = 0x{0:X8}",       tempTimestamp);

            AaruConsole.DebugWriteLine(MODULE_NAME,
                                       Localization.Datetime_field_year_0_month_1_day_2_hour_3_minute_4,
                                       year,
                                       month,
                                       day,
                                       hour,
                                       minute);

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
        {
            sbInformation.AppendLine(Localization.ProDOS_uses_512_bytes_sector_while_device_uses_2048_bytes_sector)
                         .AppendLine();
        }

        if(rootDirectoryKeyBlock.header.version != VERSION1 || rootDirectoryKeyBlock.header.min_version != VERSION1)
        {
            sbInformation.AppendLine(Localization.Warning_Detected_unknown_ProDOS_version_ProDOS_filesystem);
            sbInformation.AppendLine(Localization.All_of_the_following_information_may_be_incorrect);
        }

        if(rootDirectoryKeyBlock.header.version == VERSION1)
            sbInformation.AppendLine(Localization.ProDOS_version_one_used_to_create_this_volume);
        else
        {
            sbInformation.AppendFormat(Localization.Unknown_ProDOS_version_with_field_0_used_to_create_this_volume,
                                       rootDirectoryKeyBlock.header.version)
                         .AppendLine();
        }

        if(rootDirectoryKeyBlock.header.min_version == VERSION1)
            sbInformation.AppendLine(Localization.ProDOS_version_one_at_least_required_for_reading_this_volume);
        else
        {
            sbInformation
               .AppendFormat(Localization
                                .Unknown_ProDOS_version_with_field_0_is_at_least_required_for_reading_this_volume,
                             rootDirectoryKeyBlock.header.min_version)
               .AppendLine();
        }

        sbInformation.AppendFormat(Localization.Volume_name_is_0, rootDirectoryKeyBlock.header.volume_name)
                     .AppendLine();

        if(dateCorrect)
        {
            sbInformation.AppendFormat(Localization.Volume_created_on_0, rootDirectoryKeyBlock.header.creation_time)
                         .AppendLine();
        }

        sbInformation.AppendFormat(Localization._0_bytes_per_directory_entry, rootDirectoryKeyBlock.header.entry_length)
                     .AppendLine();

        sbInformation.AppendFormat(Localization._0_entries_per_directory_block,
                                   rootDirectoryKeyBlock.header.entries_per_block)
                     .AppendLine();

        sbInformation.AppendFormat(Localization._0_files_in_root_directory, rootDirectoryKeyBlock.header.file_count)
                     .AppendLine();

        sbInformation.AppendFormat(Localization._0_blocks_in_volume, rootDirectoryKeyBlock.header.total_blocks)
                     .AppendLine();

        sbInformation.AppendFormat(Localization.Bitmap_starts_at_block_0, rootDirectoryKeyBlock.header.bit_map_pointer)
                     .AppendLine();

        if((rootDirectoryKeyBlock.header.access & READ_ATTRIBUTE) == READ_ATTRIBUTE)
            sbInformation.AppendLine(Localization.Volume_can_be_read);

        if((rootDirectoryKeyBlock.header.access & WRITE_ATTRIBUTE) == WRITE_ATTRIBUTE)
            sbInformation.AppendLine(Localization.Volume_can_be_written);

        if((rootDirectoryKeyBlock.header.access & RENAME_ATTRIBUTE) == RENAME_ATTRIBUTE)
            sbInformation.AppendLine(Localization.Volume_can_be_renamed);

        if((rootDirectoryKeyBlock.header.access & DESTROY_ATTRIBUTE) == DESTROY_ATTRIBUTE)
            sbInformation.AppendLine(Localization.Volume_can_be_destroyed);

        if((rootDirectoryKeyBlock.header.access & BACKUP_ATTRIBUTE) == BACKUP_ATTRIBUTE)
            sbInformation.AppendLine(Localization.Volume_must_be_backed_up);

        // TODO: Fix mask
        if((rootDirectoryKeyBlock.header.access & RESERVED_ATTRIBUTE_MASK) != 0)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME,
                                       Localization.Reserved_attributes_are_set_0,
                                       rootDirectoryKeyBlock.header.access);
        }

        information = sbInformation.ToString();

        metadata = new FileSystem
        {
            VolumeName = rootDirectoryKeyBlock.header.volume_name,
            Files      = rootDirectoryKeyBlock.header.file_count,
            Clusters   = rootDirectoryKeyBlock.header.total_blocks,
            Type       = FS_TYPE
        };

        metadata.ClusterSize =
            (uint)((partition.End - partition.Start + 1) * imagePlugin.Info.SectorSize / metadata.Clusters);

        if(!dateCorrect) return;

        metadata.CreationDate = rootDirectoryKeyBlock.header.creation_time;
    }

#endregion
}