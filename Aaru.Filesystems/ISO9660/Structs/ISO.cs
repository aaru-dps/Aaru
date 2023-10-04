// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ISO.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
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
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

// ReSharper disable UnusedType.Local

using System;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.Helpers;

namespace Aaru.Filesystems;

public sealed partial class ISO9660
{
    static DecodedVolumeDescriptor DecodeVolumeDescriptor(PrimaryVolumeDescriptor pvd, Encoding encoding = null)
    {
        encoding ??= Encoding.ASCII;

        var decodedVd = new DecodedVolumeDescriptor
        {
            SystemIdentifier       = StringHandlers.CToString(pvd.system_id,      encoding).TrimEnd(),
            VolumeIdentifier       = StringHandlers.CToString(pvd.volume_id,      encoding).TrimEnd(),
            VolumeSetIdentifier    = StringHandlers.CToString(pvd.volume_set_id,  encoding).TrimEnd(),
            PublisherIdentifier    = StringHandlers.CToString(pvd.publisher_id,   encoding).TrimEnd(),
            DataPreparerIdentifier = StringHandlers.CToString(pvd.preparer_id,    encoding).TrimEnd(),
            ApplicationIdentifier  = StringHandlers.CToString(pvd.application_id, encoding).TrimEnd()
        };

        if(pvd.creation_date[0] == '0' || pvd.creation_date[0] == 0x00)
            decodedVd.CreationTime = DateTime.MinValue;
        else
            decodedVd.CreationTime = DateHandlers.Iso9660ToDateTime(pvd.creation_date);

        if(pvd.modification_date[0] == '0' || pvd.modification_date[0] == 0x00)
            decodedVd.HasModificationTime = false;
        else
        {
            decodedVd.HasModificationTime = true;
            decodedVd.ModificationTime    = DateHandlers.Iso9660ToDateTime(pvd.modification_date);
        }

        if(pvd.expiration_date[0] == '0' || pvd.expiration_date[0] == 0x00)
            decodedVd.HasExpirationTime = false;
        else
        {
            decodedVd.HasExpirationTime = true;
            decodedVd.ExpirationTime    = DateHandlers.Iso9660ToDateTime(pvd.expiration_date);
        }

        if(pvd.effective_date[0] == '0' || pvd.effective_date[0] == 0x00)
            decodedVd.HasEffectiveTime = false;
        else
        {
            decodedVd.HasEffectiveTime = true;
            decodedVd.EffectiveTime    = DateHandlers.Iso9660ToDateTime(pvd.effective_date);
        }

        decodedVd.Blocks    = pvd.volume_space_size;
        decodedVd.BlockSize = pvd.logical_block_size;

        return decodedVd;
    }

#region Nested type: BootRecord

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct BootRecord
    {
        public readonly byte type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public readonly byte[] id;
        public readonly byte version;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] system_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] boot_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1977)]
        public readonly byte[] boot_use;
    }

#endregion

#region Nested type: DirectoryRecord

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct DirectoryRecord
    {
        public readonly byte         length;
        public readonly byte         xattr_len;
        public readonly uint         extent;
        public readonly uint         extent_be;
        public readonly uint         size;
        public readonly uint         size_be;
        public readonly IsoTimestamp date;
        public readonly FileFlags    flags;
        public readonly byte         file_unit_size;
        public readonly byte         interleave;
        public readonly ushort       volume_sequence_number;
        public readonly ushort       volume_sequence_number_be;
        public readonly byte         name_len;

        // Followed by name[name_len] and then system area until length arrives
    }

#endregion

#region Nested type: ExtendedAttributeRecord

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct ExtendedAttributeRecord
    {
        public readonly ushort      owner;
        public readonly ushort      owner_be;
        public readonly ushort      group;
        public readonly ushort      group_be;
        public readonly Permissions permissions;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public readonly byte[] creation_date;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public readonly byte[] modification_date;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public readonly byte[] expiration_date;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public readonly byte[] effective_date;
        public readonly RecordFormat    record_format;
        public readonly RecordAttribute record_attributes;
        public readonly ushort          record_length;
        public readonly ushort          record_length_be;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] system_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] system_use;
        public readonly byte record_version;
        public readonly byte escape_len;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] reserved1;
        public readonly ushort app_use_len;
        public readonly ushort app_use_len_be;
    }

#endregion

#region Nested type: IsoTimestamp

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct IsoTimestamp
    {
        public readonly byte  Years;
        public readonly byte  Month;
        public readonly byte  Day;
        public readonly byte  Hour;
        public readonly byte  Minute;
        public readonly byte  Second;
        public readonly sbyte GmtOffset;
    }

#endregion

#region Nested type: PartitionDescriptor

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct PartitionDescriptor
    {
        public readonly byte type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public readonly byte[] id;
        public readonly byte version;
        public readonly byte reserved1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] system_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] partition_id;
        public readonly uint partition_location;
        public readonly uint partition_location_be;
        public readonly uint partition_size;
        public readonly uint partition_size_be;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1960)]
        public readonly byte[] system_use;
    }

#endregion

#region Nested type: PathTableEntry

    // There are two tables one in little endian one in big endian
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct PathTableEntry
    {
        public readonly byte   name_len;
        public readonly byte   xattr_len;
        public readonly uint   start_lbn;
        public readonly ushort parent_dirno;

        // Followed by name[name_len]
    }

#endregion

#region Nested type: PrimaryVolumeDescriptor

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct PrimaryVolumeDescriptor
    {
        public readonly byte type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public readonly byte[] id;
        public readonly byte version;

        // Only used in SVDs
        public readonly byte flags;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] system_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] volume_id;
        public readonly ulong reserved1;
        public readonly uint  volume_space_size;
        public readonly uint  volume_space_size_be;

        // Only used in SVDs
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] escape_sequences;
        public readonly ushort          volume_set_size;
        public readonly ushort          volume_set_size_be;
        public readonly ushort          volume_sequence_number;
        public readonly ushort          volume_sequence_number_be;
        public readonly ushort          logical_block_size;
        public readonly ushort          logical_block_size_be;
        public readonly uint            path_table_size;
        public readonly uint            path_table_size_be;
        public readonly uint            type_l_path_table;
        public readonly uint            opt_type_l_path_table;
        public readonly uint            type_m_path_table;
        public readonly uint            opt_type_m_path_table;
        public readonly DirectoryRecord root_directory_record;
        public readonly byte            root_directory_name;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public readonly byte[] volume_set_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public readonly byte[] publisher_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public readonly byte[] preparer_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public readonly byte[] application_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 37)]
        public readonly byte[] copyright_file_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 37)]
        public readonly byte[] abstract_file_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 37)]
        public readonly byte[] bibliographic_file_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public readonly byte[] creation_date;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public readonly byte[] modification_date;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public readonly byte[] expiration_date;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public readonly byte[] effective_date;
        public readonly byte file_structure_version;
        public readonly byte reserved2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public readonly byte[] application_data;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 653)]
        public readonly byte[] reserved3;
    }

#endregion
}