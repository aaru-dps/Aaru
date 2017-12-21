// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CDi.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     CDi extensions structures.
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
        struct FileStructureVolumeDescriptor
        {
            public byte type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)] public byte[] id;
            public byte version;
            public CdiVolumeFlags flags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] system_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] volume_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public byte[] reserved1;
            public uint volume_space_size;
            // Only used in SVDs
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] escape_sequences;
            public ushort reserved2;
            public ushort volume_set_size;
            public ushort reserved3;
            public ushort volume_sequence_number;
            public ushort reserved4;
            public ushort logical_block_size;
            public uint reserved5;
            public uint path_table_size;
            public ulong reserved6;
            public uint path_table_addr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 38)] public byte[] reserved7;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] volume_set_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] publisher_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] preparer_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] application_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] copyright_file_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)] public byte[] reserved8;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] abstract_file_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)] public byte[] reserved9;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] bibliographic_file_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)] public byte[] reserved10;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] creation_date;
            public byte reserved11;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] modification_date;
            public byte reserved12;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] expiration_date;
            public byte reserved13;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] effective_date;
            public byte reserved14;
            public byte file_structure_version;
            public byte reserved15;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)] public byte[] application_data;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 653)] public byte[] reserved16;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CdiDirectoryRecord
        {
            public byte length;
            public byte xattr_len;
            public uint reserved1;
            public uint start_lbn;
            public uint reserved2;
            public uint size;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public byte[] date;
            public byte reserved3;
            public CdiFileFlags flags;
            public ushort file_unit_size;
            public ushort reserved4;
            public ushort volume_sequence_number;
            public byte name_len;
            // Followed by name[name_len] and then CdiSystemArea until length arrives
        }

        // Follows filename on directory record
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CdiSystemArea
        {
            public uint owner;
            public CdiAttributes attributes;
            public ushort reserved1;
            public byte file_no;
            public byte reserved2;
        }

        static DecodedVolumeDescriptor DecodeVolumeDescriptor(FileStructureVolumeDescriptor pvd)
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