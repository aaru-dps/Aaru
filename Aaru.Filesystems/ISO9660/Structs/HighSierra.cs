// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.Helpers;

namespace Aaru.Filesystems;

public sealed partial class ISO9660
{
    static DecodedVolumeDescriptor DecodeVolumeDescriptor(HighSierraPrimaryVolumeDescriptor pvd)
    {
        var decodedVd = new DecodedVolumeDescriptor
        {
            SystemIdentifier       = Encoding.ASCII.GetString(pvd.system_id).TrimEnd().Trim('\0'),
            VolumeIdentifier       = Encoding.ASCII.GetString(pvd.volume_id).TrimEnd().Trim('\0'),
            VolumeSetIdentifier    = Encoding.ASCII.GetString(pvd.volume_set_id).TrimEnd().Trim('\0'),
            PublisherIdentifier    = Encoding.ASCII.GetString(pvd.publisher_id).TrimEnd().Trim('\0'),
            DataPreparerIdentifier = Encoding.ASCII.GetString(pvd.preparer_id).TrimEnd().Trim('\0'),
            ApplicationIdentifier  = Encoding.ASCII.GetString(pvd.application_data).TrimEnd().Trim('\0')
        };

        if(pvd.creation_date[0] == '0' ||
           pvd.creation_date[0] == 0x00)
            decodedVd.CreationTime = DateTime.MinValue;
        else
            decodedVd.CreationTime = DateHandlers.HighSierraToDateTime(pvd.creation_date);

        if(pvd.modification_date[0] == '0' ||
           pvd.modification_date[0] == 0x00)
            decodedVd.HasModificationTime = false;
        else
        {
            decodedVd.HasModificationTime = true;
            decodedVd.ModificationTime    = DateHandlers.HighSierraToDateTime(pvd.modification_date);
        }

        if(pvd.expiration_date[0] == '0' ||
           pvd.expiration_date[0] == 0x00)
            decodedVd.HasExpirationTime = false;
        else
        {
            decodedVd.HasExpirationTime = true;
            decodedVd.ExpirationTime    = DateHandlers.HighSierraToDateTime(pvd.expiration_date);
        }

        if(pvd.effective_date[0] == '0' ||
           pvd.effective_date[0] == 0x00)
            decodedVd.HasEffectiveTime = false;
        else
        {
            decodedVd.HasEffectiveTime = true;
            decodedVd.EffectiveTime    = DateHandlers.HighSierraToDateTime(pvd.effective_date);
        }

        decodedVd.Blocks    = pvd.volume_space_size;
        decodedVd.BlockSize = pvd.logical_block_size;

        return decodedVd;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct HighSierraPrimaryVolumeDescriptor
    {
        public readonly uint volume_lbn;
        public readonly uint volume_lbn_be;
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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] escape_sequences;
        public readonly ushort                    volume_set_size;
        public readonly ushort                    volume_set_size_be;
        public readonly ushort                    volume_sequence_number;
        public readonly ushort                    volume_sequence_number_be;
        public readonly ushort                    logical_block_size;
        public readonly ushort                    logical_block_size_be;
        public readonly uint                      path_table_size;
        public readonly uint                      path_table_size_be;
        public readonly uint                      mandatory_path_table_lsb;
        public readonly uint                      opt_path_table_lsb_1;
        public readonly uint                      opt_path_table_lsb_2;
        public readonly uint                      opt_path_table_lsb_3;
        public readonly uint                      mandatory_path_table_msb;
        public readonly uint                      opt_path_table_msb_1;
        public readonly uint                      opt_path_table_msb_2;
        public readonly uint                      opt_path_table_msb_3;
        public readonly HighSierraDirectoryRecord root_directory_record;
        public readonly byte                      root_directory_name;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public readonly byte[] volume_set_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public readonly byte[] publisher_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public readonly byte[] preparer_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public readonly byte[] application_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] copyright_file_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] abstract_file_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] creation_date;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] modification_date;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] expiration_date;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] effective_date;
        public readonly byte file_structure_version;
        public readonly byte reserved2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public readonly byte[] application_data;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 680)]
        public readonly byte[] reserved3;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct HighSierraDirectoryRecord
    {
        public readonly byte                length;
        public readonly byte                xattr_len;
        public readonly uint                extent;
        public readonly uint                extent_be;
        public readonly uint                size;
        public readonly uint                size_be;
        public readonly HighSierraTimestamp date;
        public readonly FileFlags           flags;
        public readonly byte                reserved;
        public readonly byte                interleave_size;
        public readonly byte                interleave;
        public readonly ushort              volume_sequence_number;
        public readonly ushort              volume_sequence_number_be;
        public readonly byte                name_len;

        // Followed by name[name_len] and then system area until length arrives
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct HighSierraTimestamp
    {
        public readonly byte Years;
        public readonly byte Month;
        public readonly byte Day;
        public readonly byte Hour;
        public readonly byte Minute;
        public readonly byte Second;
    }

    // There are two tables one in little endian one in big endian
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct HighSierraPathTableEntry
    {
        public readonly uint   start_lbn;
        public readonly byte   xattr_len;
        public readonly byte   name_len;
        public readonly ushort parent_dirno;

        // Followed by name[name_len]
    }
}