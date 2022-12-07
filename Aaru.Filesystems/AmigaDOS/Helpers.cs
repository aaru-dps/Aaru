// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Helpers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Amiga Fast File System plugin.
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

using System;
using Aaru.Helpers;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of Amiga Fast File System (AFFS)</summary>
public sealed partial class AmigaDOSPlugin
{
    static RootBlock MarshalRootBlock(byte[] block)
    {
        byte[] tmp = new byte[228];
        Array.Copy(block, 0, tmp, 0, 24);
        Array.Copy(block, block.Length - 200, tmp, 28, 200);
        RootBlock root = Marshal.ByteArrayToStructureBigEndian<RootBlock>(tmp);
        root.hashTable = new uint[(block.Length - 224) / 4];

        for(int i = 0; i < root.hashTable.Length; i++)
            root.hashTable[i] = BigEndianBitConverter.ToUInt32(block, 24 + (i * 4));

        return root;
    }

    static uint AmigaChecksum(byte[] data)
    {
        uint sum = 0;

        for(int i = 0; i < data.Length; i += 4)
            sum += (uint)((data[i] << 24) + (data[i + 1] << 16) + (data[i + 2] << 8) + data[i + 3]);

        return (uint)-sum;
    }

    static uint AmigaBootChecksum(byte[] data)
    {
        uint sum = 0;

        for(int i = 0; i < data.Length; i += 4)
        {
            uint psum = sum;

            if((sum += (uint)((data[i] << 24) + (data[i + 1] << 16) + (data[i + 2] << 8) + data[i + 3])) < psum)
                sum++;
        }

        return ~sum;
    }
}