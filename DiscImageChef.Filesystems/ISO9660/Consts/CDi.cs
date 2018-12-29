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
//     CDi filesystem constants and enumerations.
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

namespace DiscImageChef.Filesystems.ISO9660
{
    public partial class ISO9660
    {
        const string CDI_MAGIC = "CD-I ";

        [Flags]
        enum CdiVolumeFlags : byte
        {
            // Escapes are not ISO 2375 but ISO 2022
            NotISO2375 = 1
        }

        [Flags]
        enum CdiFileFlags : byte
        {
            Hidden = 0x01
        }

        [Flags]
        enum CdiAttributes : ushort
        {
            OwnerRead    = 1 << 0,
            OwnerExecute = 1 << 2,
            GroupRead    = 1 << 4,
            GroupExecute = 1 << 6,
            OtherRead    = 1 << 8,
            OtherExecute = 1 << 10,
            DigitalAudio = 1 << 14,
            Directory    = 1 << 15
        }
    }
}