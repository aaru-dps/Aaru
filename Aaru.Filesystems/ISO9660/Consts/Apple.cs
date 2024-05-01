// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Apple.cs
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
// Copyright © 2011-2024 Natalia Portillo
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

namespace Aaru.Filesystems;

public sealed partial class ISO9660
{
    const ushort APPLE_MAGIC     = 0x4141; // "AA"
    const ushort APPLE_MAGIC_OLD = 0x4241; // "BA"

#region Nested type: AppleId

    enum AppleId : byte
    {
        ProDOS = 1,
        HFS    = 2
    }

#endregion

#region Nested type: AppleOldId

    enum AppleOldId : byte
    {
        ProDOS                = 1,
        TypeCreator           = 2,
        TypeCreatorBundle     = 3,
        TypeCreatorIcon       = 4,
        TypeCreatorIconBundle = 5,
        HFS                   = 6
    }

#endregion
}