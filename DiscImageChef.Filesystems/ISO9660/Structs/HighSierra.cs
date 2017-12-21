// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : HighSierra.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     High Sierra Format structures.
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
using System.Runtime.InteropServices;
using System.Text;

namespace DiscImageChef.Filesystems.ISO9660
{
    public partial class ISO9660 : Filesystem
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HighSierraPrimaryVolumeDescriptor
        {
            public uint volume_lbn;
            public uint volume_lbn_be;
            public byte type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)] public byte[] id;
            public byte version;
            // Only used in SVDs
            public byte flags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] system_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] volume_id;
            public ulong reserved1;
            public uint volume_space_size;
            public uint volume_space_size_be;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] escape_sequences;
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
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] volume_set_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] publisher_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] preparer_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] application_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] copyright_file_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] abstract_file_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] creation_date;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] modification_date;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] expiration_date;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] effective_date;
            public byte file_structure_version;
            public byte reserved2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)] public byte[] application_data;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 680)] public byte[] reserved3;
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
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public byte[] date;
            public FileFlags flags;
            public byte reserved;
            public byte interleave_size;
            public byte interleave;
            public ushort volume_sequence_number;
            public ushort volume_sequence_number_be;
            public byte name_len;
            // Followed by name[name_len] and then system area until length arrives
        }

        static DecodedVolumeDescriptor DecodeVolumeDescriptor(HighSierraPrimaryVolumeDescriptor pvd)
        {
            DecodedVolumeDescriptor decodedVD = new DecodedVolumeDescriptor();

            decodedVD.SystemIdentifier = Encoding.ASCII.GetString(pvd.system_id).TrimEnd().Trim('\0');
            decodedVD.VolumeIdentifier = Encoding.ASCII.GetString(pvd.volume_id).TrimEnd().Trim('\0');
            decodedVD.VolumeSetIdentifier = Encoding.ASCII.GetString(pvd.volume_set_id).TrimEnd().Trim('\0');
            decodedVD.PublisherIdentifier = Encoding.ASCII.GetString(pvd.publisher_id).TrimEnd().Trim('\0');
            decodedVD.DataPreparerIdentifier = Encoding.ASCII.GetString(pvd.preparer_id).TrimEnd().Trim('\0');
            decodedVD.ApplicationIdentifier =
                Encoding.ASCII.GetString(pvd.application_data).TrimEnd().Trim('\0');
            if(pvd.creation_date[0] == '0' || pvd.creation_date[0] == 0x00)
                decodedVD.CreationTime = DateTime.MinValue;
            else decodedVD.CreationTime = DateHandlers.HighSierraToDateTime(pvd.creation_date);

            if(pvd.modification_date[0] == '0' || pvd.modification_date[0] == 0x00) decodedVD.HasModificationTime = false;
            else
            {
                decodedVD.HasModificationTime = true;
                decodedVD.ModificationTime = DateHandlers.HighSierraToDateTime(pvd.modification_date);
            }

            if(pvd.expiration_date[0] == '0' || pvd.expiration_date[0] == 0x00) decodedVD.HasExpirationTime = false;
            else
            {
                decodedVD.HasExpirationTime = true;
                decodedVD.ExpirationTime = DateHandlers.HighSierraToDateTime(pvd.expiration_date);
            }

            if(pvd.effective_date[0] == '0' || pvd.effective_date[0] == 0x00) decodedVD.HasEffectiveTime = false;
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