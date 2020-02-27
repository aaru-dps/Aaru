// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains structures for Quasi88 disk images.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;
using Aaru.Decoders.Floppy;

namespace Aaru.DiscImages
{
    public partial class D88
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct D88Header
        {
            /// <summary>
            ///     Disk name, nul-terminated ASCII
            ///     ディスクの名前(ASCII + '\0')
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public byte[] name;
            /// <summary>
            ///     Reserved
            ///     ディスクの名前(ASCII + '\0')
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
            public byte[] reserved;
            /// <summary>
            ///     Write protect status
            ///     ライトプロテクト： 0x00 なし、0x10 あり
            /// </summary>
            public byte write_protect;
            /// <summary>
            ///     Disk type
            ///     ディスクの種類： 0x00 2D、 0x10 2DD、 0x20 2HD
            /// </summary>
            public DiskType disk_type;
            /// <summary>
            ///     Disk image size
            ///     ディスクのサイズ
            /// </summary>
            public int disk_size;
            /// <summary>
            ///     Track pointers
            ///     トラック部のオフセットテーブル 0 Track ～ 163 Track
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 164)]
            public int[] track_table;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct SectorHeader
        {
            /// <summary>
            ///     Cylinder
            ///     ID の C
            /// </summary>
            public byte c;
            /// <summary>
            ///     Head
            ///     ID の H
            /// </summary>
            public byte h;
            /// <summary>
            ///     Sector number
            ///     ID の R
            /// </summary>
            public byte r;
            /// <summary>
            ///     Sector size
            ///     ID の N
            /// </summary>
            public IBMSectorSizeCode n;
            /// <summary>
            ///     Number of sectors in this track
            ///     このトラック内に存在するセクタの数
            /// </summary>
            public short spt;
            /// <summary>
            ///     Density: 0x00 MFM, 0x40 FM
            ///     記録密度： 0x00 倍密度、0x40 単密度
            /// </summary>
            public DensityType density;
            /// <summary>
            ///     Deleted sector, 0x00 not deleted, 0x10 deleted
            ///     DELETED MARK： 0x00 ノーマル、 0x10 DELETED
            /// </summary>
            public byte deleted_mark;
            /// <summary>
            ///     Sector status
            ///     ステータス
            /// </summary>
            public byte status;
            /// <summary>
            ///     Reserved
            ///     リザーブ
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public byte[] reserved;
            /// <summary>
            ///     Size of data following this field
            ///     このセクタ部のデータサイズ
            /// </summary>
            public short size_of_data;
        }
    }
}