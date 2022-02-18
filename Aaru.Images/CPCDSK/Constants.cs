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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.DiscImages
{
    public sealed partial class Cpcdsk
    {
        /// <summary>Identifier for CPCEMU disk images, "MV - CPC" + usually : "EMU Disk-File\r\nDisk-Info\r\n" but not required</summary>
        const string CPCDSK_ID = "MV - CPCEMU Disk-File";

        /// <summary>Identifier for DU54 disk images, "MV - CPC format Disk Image (DU54)"</summary>
        const string DU54_ID = "MV - CPC format Disk Image (DU54)";

        /// <summary>Identifier for Extended CPCEMU disk images</summary>
        const string EDSK_ID = "EXTENDED CPC DSK File";

        /// <summary>Identifier for track information, </summary>
        const string TRACK_ID = "Track-Info";
    }
}