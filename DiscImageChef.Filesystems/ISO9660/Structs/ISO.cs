// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ISO.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     ISO9660 filesystem structures.
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

namespace DiscImageChef.Filesystems.ISO9660
{
    public partial class ISO9660
    {
        static DecodedVolumeDescriptor DecodeVolumeDescriptor(PrimaryVolumeDescriptor pvd)
        {
            DecodedVolumeDescriptor decodedVD = new DecodedVolumeDescriptor
            {
                SystemIdentifier       = StringHandlers.CToString(pvd.system_id).TrimEnd(),
                VolumeIdentifier       = StringHandlers.CToString(pvd.volume_id).TrimEnd(),
                VolumeSetIdentifier    = StringHandlers.CToString(pvd.volume_set_id).TrimEnd(),
                PublisherIdentifier    = StringHandlers.CToString(pvd.publisher_id).TrimEnd(),
                DataPreparerIdentifier = StringHandlers.CToString(pvd.preparer_id).TrimEnd(),
                ApplicationIdentifier  = StringHandlers.CToString(pvd.application_id).TrimEnd()
            };

            if(pvd.creation_date[0] == '0' || pvd.creation_date[0] == 0x00) decodedVD.CreationTime = DateTime.MinValue;
            else
                decodedVD.CreationTime =
                    DateHandlers.Iso9660ToDateTime(pvd.creation_date);

            if(pvd.modification_date[0] == '0' || pvd.modification_date[0] == 0x00)
                decodedVD.HasModificationTime = false;
            else
            {
                decodedVD.HasModificationTime = true;
                decodedVD.ModificationTime    = DateHandlers.Iso9660ToDateTime(pvd.modification_date);
            }

            if(pvd.expiration_date[0] == '0' || pvd.expiration_date[0] == 0x00) decodedVD.HasExpirationTime = false;
            else
            {
                decodedVD.HasExpirationTime = true;
                decodedVD.ExpirationTime    = DateHandlers.Iso9660ToDateTime(pvd.expiration_date);
            }

            if(pvd.effective_date[0] == '0' || pvd.effective_date[0] == 0x00) decodedVD.HasEffectiveTime = false;
            else
            {
                decodedVD.HasEffectiveTime = true;
                decodedVD.EffectiveTime    = DateHandlers.Iso9660ToDateTime(pvd.effective_date);
            }

            decodedVD.Blocks    = pvd.volume_space_size;
            decodedVD.BlockSize = pvd.logical_block_size;

            return decodedVD;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PrimaryVolumeDescriptor
        {
            public byte type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public byte[] id;
            public byte version;
            // Only used in SVDs
            public byte flags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] system_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] volume_id;
            public ulong reserved1;
            public uint  volume_space_size;
            public uint  volume_space_size_be;
            // Only used in SVDs
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] escape_sequences;
            public ushort          volume_set_size;
            public ushort          volume_set_size_be;
            public ushort          volume_sequence_number;
            public ushort          volume_sequence_number_be;
            public ushort          logical_block_size;
            public ushort          logical_block_size_be;
            public uint            path_table_size;
            public uint            path_table_size_be;
            public uint            type_1_path_table;
            public uint            opt_type_1_path_table;
            public uint            type_m_path_table;
            public uint            opt_type_m_path_table;
            public DirectoryRecord root_directory_record;
            public byte            root_directory_name;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] volume_set_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] publisher_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] preparer_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] application_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 37)]
            public byte[] copyright_file_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 37)]
            public byte[] abstract_file_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 37)]
            public byte[] bibliographic_file_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public byte[] creation_date;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public byte[] modification_date;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public byte[] expiration_date;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public byte[] effective_date;
            public byte file_structure_version;
            public byte reserved2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
            public byte[] application_data;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 653)]
            public byte[] reserved3;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BootRecord
        {
            public byte type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public byte[] id;
            public byte version;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] system_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] boot_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1977)]
            public byte[] boot_use;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PartitionDescriptor
        {
            public byte type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public byte[] id;
            public byte version;
            public byte reserved1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] system_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] partition_id;
            public uint partition_location;
            public uint partition_location_be;
            public uint partition_size;
            public uint partition_size_be;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1960)]
            public byte[] system_use;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DirectoryRecord
        {
            public byte length;
            public byte xattr_len;
            public uint extent;
            public uint extent_be;
            public uint size;
            public uint size_be;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            public byte[] date;
            public FileFlags flags;
            public byte      file_unit_size;
            public byte      interleave;
            public ushort    volume_sequence_number;
            public ushort    volume_sequence_number_be;
            public byte      name_len;
            // Followed by name[name_len] and then system area until length arrives
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ExtendedAttributeRecord
        {
            public ushort      owner;
            public ushort      owner_be;
            public ushort      group;
            public ushort      group_be;
            public Permissions permissions;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public byte[] creation_date;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public byte[] modification_date;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public byte[] expiration_date;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public byte[] effective_date;
            public RecordFormat    record_format;
            public RecordAttribute record_attributes;
            public ushort          record_length;
            public ushort          record_length_be;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] system_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] system_use;
            public byte record_version;
            public byte escape_len;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] reserved1;
            public ushort app_use_len;
            public ushort app_use_len_be;
        }

        // There are two tables one in little endian one in big endian
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PathTableEntry
        {
            public byte   name_len;
            public byte   xattr_len;
            public uint   start_lbn;
            public ushort parent_dirno;
            // Followed by name[name_len]
        }
    }
}