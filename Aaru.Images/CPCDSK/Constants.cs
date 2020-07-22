// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Constants.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains constants for CPCEMU disk images.
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

namespace Aaru.DiscImages
{
    public sealed partial class Cpcdsk
    {
        /// <summary>Identifier for CPCEMU disk images, "MV - CPC" + usually : "EMU Disk-File\r\nDisk-Info\r\n" but not required</summary>
        readonly byte[] _cpcdskId =
        {
            0x4D, 0x56, 0x20, 0x2D, 0x20, 0x43, 0x50, 0x43
        };
        /// <summary>Identifier for DU54 disk images, "MV - CPC format Disk Image (DU54)"</summary>
        readonly byte[] _du54Id =
        {
            0x4D, 0x56, 0x20, 0x2D, 0x20, 0x43, 0x50, 0x43, 0x20, 0x66, 0x6F, 0x72, 0x6D, 0x61, 0x74, 0x20, 0x44, 0x69,
            0x73, 0x6B, 0x20
        };
        /// <summary>Identifier for Extended CPCEMU disk images, "EXTENDED CPC DSK File"</summary>
        readonly byte[] _edskId =
        {
            0x45, 0x58, 0x54, 0x45, 0x4E, 0x44, 0x45, 0x44, 0x20, 0x43, 0x50, 0x43, 0x20, 0x44, 0x53, 0x4B, 0x20, 0x46,
            0x69, 0x6C, 0x65
        };
        /// <summary>Identifier for track information, "Track-Info\r\n"</summary>
        readonly byte[] _trackId =
        {
            0x54, 0x72, 0x61, 0x63, 0x6B, 0x2D, 0x49, 0x6E, 0x66, 0x6F
        };
    }
}