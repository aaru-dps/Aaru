// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains enumerations for Quasi88 disk images.
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
// ****************************************************************************/

namespace Aaru.DiscImages;

using System.Diagnostics.CodeAnalysis;

[SuppressMessage("ReSharper", "UnusedMember.Local"), SuppressMessage("ReSharper", "UnusedType.Local")]
public sealed partial class D88
{
    enum DiskType : byte
    {
        D2  = 0x00,
        Dd2 = 0x10,
        Hd2 = 0x20
    }

    enum DensityType : byte
    {
        Mfm = 0x00,
        Fm  = 0x40
    }

    /// <summary>Status as returned by PC-98 BIOS ステータスは、PC-98x1 のBIOS が返してくるステータスで、</summary>
    enum StatusType : byte
    {
        /// <summary>Normal 正常</summary>
        Normal = 0x00,
        /// <summary>Deleted 正常(DELETED DATA)</summary>
        Deleted = 0x10,
        /// <summary>CRC error in address fields ID CRC エラー</summary>
        IdError = 0xA0,
        /// <summary>CRC error in data block データ CRC エラー</summary>
        DataError = 0xB0,
        /// <summary>Address mark not found アドレスマークなし</summary>
        AddressMarkNotFound = 0xE0,
        /// <summary>Data mark not found データマークなし</summary>
        DataMarkNotFound = 0xF0
    }
}