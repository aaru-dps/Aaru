// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Component
//
// --[ Description ] ----------------------------------------------------------
//
//     Description
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DiscImageChef.Filesystems.ISO9660
{
    public partial class ISO9660 : Filesystem
    {
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
            public uint volume_space_size;
            public uint volume_space_size_be;
            // Only used in SVDs
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] escape_sequences;
            public ushort volume_set_size;
            public ushort volume_set_size_be;
            public ushort volume_sequence_number;
            public ushort volume_sequence_number_be;
            public ushort logical_block_size;
            public ushort logical_block_size_be;
            public uint path_table_size;
            public uint path_table_size_be;
            public uint type_1_path_table;
            public uint opt_type_1_path_table;
            public uint type_m_path_table;
            public uint opt_type_m_path_table;
            public DirectoryRecord root_directory_record;
            public byte root_directory_name;
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
        struct HighSierraPrimaryVolumeDescriptor
        {
            public uint volume_lbn;
            public uint volume_lbn_be;
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
            public uint volume_space_size;
            public uint volume_space_size_be;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] escape_sequences;
            public ushort volume_set_size;
            public ushort volume_set_size_be;
            public ushort volume_sequence_number;
            public ushort volume_sequence_number_be;
            public ushort logical_block_size;
            public ushort logical_block_size_be;
            public uint path_table_size;
            public uint path_table_size_be;
            public uint manditory_path_table_lsb;
            public uint opt_path_table_lsb_1;
            public uint opt_path_table_lsb_2;
            public uint opt_path_table_lsb_3;
            public uint manditory_path_table_msb;
            public uint opt_path_table_msb_1;
            public uint opt_path_table_msb_2;
            public uint opt_path_table_msb_3;
            public HighSierraDirectoryRecord root_directory_record;
            public byte root_directory_name;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] volume_set_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] publisher_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] preparer_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] application_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] copyright_file_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] abstract_file_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] creation_date;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] modification_date;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] expiration_date;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] effective_date;
            public byte file_structure_version;
            public byte reserved2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
            public byte[] application_data;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 680)]
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
            public byte file_unit_size;
            public byte interleave;
            public ushort volume_sequence_number;
            public ushort volume_sequence_number_be;
            public byte name_len;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HighSierraDirectoryRecord
        {
            public byte length;
            public byte xattr_len;
            public uint extent;
            public uint extent_be;
            public uint size;
            public uint size_be;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] date;
            public FileFlags flags;
            public byte reserved;
            public byte interleave_size;
            public byte interleave;
            public ushort volume_sequence_number;
            public ushort volume_sequence_number_be;
            public byte name_len;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ExtendedAttributeRecord
        {
            public ushort owner;
            public ushort owner_be;
            public ushort group;
            public ushort group_be;
            public Permissions permissions;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public byte[] creation_date;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public byte[] modification_date;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public byte[] expiration_date;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public byte[] effective_date;
            public RecordFormat record_format;
            public RecordAttribute record_attributes;
            public ushort record_length;
            public ushort record_length_be;
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

        struct DecodedVolumeDescriptor
        {
            public string SystemIdentifier;
            public string VolumeIdentifier;
            public string VolumeSetIdentifier;
            public string PublisherIdentifier;
            public string DataPreparerIdentifier;
            public string ApplicationIdentifier;
            public DateTime CreationTime;
            public bool HasModificationTime;
            public DateTime ModificationTime;
            public bool HasExpirationTime;
            public DateTime ExpirationTime;
            public bool HasEffectiveTime;
            public DateTime EffectiveTime;
            public ushort BlockSize;
            public uint Blocks;
        }

        static DecodedVolumeDescriptor DecodeJolietDescriptor(PrimaryVolumeDescriptor jolietvd)
        {
            DecodedVolumeDescriptor decodedVD = new DecodedVolumeDescriptor();

            decodedVD.SystemIdentifier = Encoding.BigEndianUnicode.GetString(jolietvd.system_id).TrimEnd().Trim(new[] { '\u0000' });
            decodedVD.VolumeIdentifier = Encoding.BigEndianUnicode.GetString(jolietvd.volume_id).TrimEnd().Trim(new[] { '\u0000' });
            decodedVD.VolumeSetIdentifier = Encoding.BigEndianUnicode.GetString(jolietvd.volume_set_id).TrimEnd().Trim(new[] { '\u0000' });
            decodedVD.PublisherIdentifier = Encoding.BigEndianUnicode.GetString(jolietvd.publisher_id).TrimEnd().Trim(new[] { '\u0000' });
            decodedVD.DataPreparerIdentifier = Encoding.BigEndianUnicode.GetString(jolietvd.preparer_id).TrimEnd().Trim(new[] { '\u0000' });
            decodedVD.ApplicationIdentifier = Encoding.BigEndianUnicode.GetString(jolietvd.application_id).TrimEnd().Trim(new[] { '\u0000' });
            if(jolietvd.creation_date[0] < 0x31 || jolietvd.creation_date[0] > 0x39)
                decodedVD.CreationTime = DateTime.MinValue;
            else
                decodedVD.CreationTime = DateHandlers.ISO9660ToDateTime(jolietvd.creation_date);

            if(jolietvd.modification_date[0] < 0x31 || jolietvd.modification_date[0] > 0x39)
            {
                decodedVD.HasModificationTime = false;
            }
            else
            {
                decodedVD.HasModificationTime = true;
                decodedVD.ModificationTime = DateHandlers.ISO9660ToDateTime(jolietvd.modification_date);
            }

            if(jolietvd.expiration_date[0] < 0x31 || jolietvd.expiration_date[0] > 0x39)
            {
                decodedVD.HasExpirationTime = false;
            }
            else
            {
                decodedVD.HasExpirationTime = true;
                decodedVD.ExpirationTime = DateHandlers.ISO9660ToDateTime(jolietvd.expiration_date);
            }

            if(jolietvd.effective_date[0] < 0x31 || jolietvd.effective_date[0] > 0x39)
            {
                decodedVD.HasEffectiveTime = false;
            }
            else
            {
                decodedVD.HasEffectiveTime = true;
                decodedVD.EffectiveTime = DateHandlers.ISO9660ToDateTime(jolietvd.effective_date);
            }

            decodedVD.Blocks = jolietvd.volume_space_size;
            decodedVD.BlockSize = jolietvd.logical_block_size;

            return decodedVD;
        }

        static DecodedVolumeDescriptor DecodeVolumeDescriptor(PrimaryVolumeDescriptor pvd)
        {
            DecodedVolumeDescriptor decodedVD = new DecodedVolumeDescriptor();

            decodedVD.SystemIdentifier = Encoding.ASCII.GetString(pvd.system_id).TrimEnd().Trim(new[] { '\0' });
            decodedVD.VolumeIdentifier = Encoding.ASCII.GetString(pvd.volume_id).TrimEnd().Trim(new[] { '\0' });
            decodedVD.VolumeSetIdentifier = Encoding.ASCII.GetString(pvd.volume_set_id).TrimEnd().Trim(new[] { '\0' });
            decodedVD.PublisherIdentifier = Encoding.ASCII.GetString(pvd.publisher_id).TrimEnd().Trim(new[] { '\0' });
            decodedVD.DataPreparerIdentifier = Encoding.ASCII.GetString(pvd.preparer_id).TrimEnd().Trim(new[] { '\0' });
            decodedVD.ApplicationIdentifier = Encoding.ASCII.GetString(pvd.application_data).TrimEnd().Trim(new[] { '\0' });
            if(pvd.creation_date[0] == '0' || pvd.creation_date[0] == 0x00)
                decodedVD.CreationTime = DateTime.MinValue;
            else
                decodedVD.CreationTime = DateHandlers.ISO9660ToDateTime(pvd.creation_date);

            if(pvd.modification_date[0] == '0' || pvd.modification_date[0] == 0x00)
            {
                decodedVD.HasModificationTime = false;
            }
            else
            {
                decodedVD.HasModificationTime = true;
                decodedVD.ModificationTime = DateHandlers.ISO9660ToDateTime(pvd.modification_date);
            }

            if(pvd.expiration_date[0] == '0' || pvd.expiration_date[0] == 0x00)
            {
                decodedVD.HasExpirationTime = false;
            }
            else
            {
                decodedVD.HasExpirationTime = true;
                decodedVD.ExpirationTime = DateHandlers.ISO9660ToDateTime(pvd.expiration_date);
            }

            if(pvd.effective_date[0] == '0' || pvd.effective_date[0] == 0x00)
            {
                decodedVD.HasEffectiveTime = false;
            }
            else
            {
                decodedVD.HasEffectiveTime = true;
                decodedVD.EffectiveTime = DateHandlers.ISO9660ToDateTime(pvd.effective_date);
            }

            decodedVD.Blocks = pvd.volume_space_size;
            decodedVD.BlockSize = pvd.logical_block_size;

            return decodedVD;
        }

        static DecodedVolumeDescriptor DecodeVolumeDescriptor(HighSierraPrimaryVolumeDescriptor pvd)
        {
            DecodedVolumeDescriptor decodedVD = new DecodedVolumeDescriptor();

            decodedVD.SystemIdentifier = Encoding.ASCII.GetString(pvd.system_id).TrimEnd().Trim(new[] { '\0' });
            decodedVD.VolumeIdentifier = Encoding.ASCII.GetString(pvd.volume_id).TrimEnd().Trim(new[] { '\0' });
            decodedVD.VolumeSetIdentifier = Encoding.ASCII.GetString(pvd.volume_set_id).TrimEnd().Trim(new[] { '\0' });
            decodedVD.PublisherIdentifier = Encoding.ASCII.GetString(pvd.publisher_id).TrimEnd().Trim(new[] { '\0' });
            decodedVD.DataPreparerIdentifier = Encoding.ASCII.GetString(pvd.preparer_id).TrimEnd().Trim(new[] { '\0' });
            decodedVD.ApplicationIdentifier = Encoding.ASCII.GetString(pvd.application_data).TrimEnd().Trim(new[] { '\0' });
            if(pvd.creation_date[0] == '0' || pvd.creation_date[0] == 0x00)
                decodedVD.CreationTime = DateTime.MinValue;
            else
                decodedVD.CreationTime = DateHandlers.HighSierraToDateTime(pvd.creation_date);

            if(pvd.modification_date[0] == '0' || pvd.modification_date[0] == 0x00)
            {
                decodedVD.HasModificationTime = false;
            }
            else
            {
                decodedVD.HasModificationTime = true;
                decodedVD.ModificationTime = DateHandlers.HighSierraToDateTime(pvd.modification_date);
            }

            if(pvd.expiration_date[0] == '0' || pvd.expiration_date[0] == 0x00)
            {
                decodedVD.HasExpirationTime = false;
            }
            else
            {
                decodedVD.HasExpirationTime = true;
                decodedVD.ExpirationTime = DateHandlers.HighSierraToDateTime(pvd.expiration_date);
            }

            if(pvd.effective_date[0] == '0' || pvd.effective_date[0] == 0x00)
            {
                decodedVD.HasEffectiveTime = false;
            }
            else
            {
                decodedVD.HasEffectiveTime = true;
                decodedVD.EffectiveTime = DateHandlers.HighSierraToDateTime(pvd.effective_date);
            }

            decodedVD.Blocks = pvd.volume_space_size;
            decodedVD.BlockSize = pvd.logical_block_size;

            return decodedVD;
        }
    }
}
