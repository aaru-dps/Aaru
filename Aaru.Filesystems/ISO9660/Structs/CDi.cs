// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CDi.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     CD-i extensions structures.
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
using Aaru.Helpers;

namespace Aaru.Filesystems;

public sealed partial class ISO9660
{
    static DecodedVolumeDescriptor DecodeVolumeDescriptor(FileStructureVolumeDescriptor pvd)
    {
        var decodedVd = new DecodedVolumeDescriptor
        {
            SystemIdentifier       = StringHandlers.CToString(pvd.system_id).TrimEnd(),
            VolumeIdentifier       = StringHandlers.CToString(pvd.volume_id).TrimEnd(),
            VolumeSetIdentifier    = StringHandlers.CToString(pvd.volume_set_id).TrimEnd(),
            PublisherIdentifier    = StringHandlers.CToString(pvd.publisher_id).TrimEnd(),
            DataPreparerIdentifier = StringHandlers.CToString(pvd.preparer_id).TrimEnd(),
            ApplicationIdentifier  = StringHandlers.CToString(pvd.application_data).TrimEnd()
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
    readonly struct FileStructureVolumeDescriptor
    {
        public readonly byte type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public readonly byte[] id;
        public readonly byte           version;
        public readonly CdiVolumeFlags flags;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] system_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] volume_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] reserved1;
        public readonly uint volume_space_size;

        // Only used in SVDs
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] escape_sequences;
        public readonly ushort reserved2;
        public readonly ushort volume_set_size;
        public readonly ushort reserved3;
        public readonly ushort volume_sequence_number;
        public readonly ushort reserved4;
        public readonly ushort logical_block_size;
        public readonly uint   reserved5;
        public readonly uint   path_table_size;
        public readonly ulong  reserved6;
        public readonly uint   path_table_addr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 38)]
        public readonly byte[] reserved7;
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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public readonly byte[] reserved8;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] abstract_file_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public readonly byte[] reserved9;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] bibliographic_file_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public readonly byte[] reserved10;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] creation_date;
        public readonly byte reserved11;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] modification_date;
        public readonly byte reserved12;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] expiration_date;
        public readonly byte reserved13;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] effective_date;
        public readonly byte reserved14;
        public readonly byte file_structure_version;
        public readonly byte reserved15;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public readonly byte[] application_data;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 653)]
        public readonly byte[] reserved16;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct CdiDirectoryRecord
    {
        public readonly byte                length;
        public readonly byte                xattr_len;
        public readonly uint                reserved1;
        public readonly uint                start_lbn;
        public readonly uint                reserved2;
        public readonly uint                size;
        public readonly HighSierraTimestamp date;
        public readonly byte                reserved3;
        public readonly CdiFileFlags        flags;
        public readonly ushort              file_unit_size;
        public readonly ushort              reserved4;
        public readonly ushort              volume_sequence_number;
        public readonly byte                name_len;

        // Followed by name[name_len] and then CdiSystemArea until length arrives
    }

    // Follows filename on directory record
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct CdiSystemArea
    {
        public readonly ushort        group;
        public readonly ushort        owner;
        public readonly CdiAttributes attributes;
        public readonly ushort        reserved1;
        public readonly byte          file_no;
        public readonly byte          reserved2;
    }
}