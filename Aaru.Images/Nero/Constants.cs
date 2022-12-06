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
//     Contains constants for Nero Burning ROM disc images.
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

namespace Aaru.DiscImages
{
    public sealed partial class Nero
    {
        const uint NERO_FOOTER_V1  = 0x4E45524F; // "NERO"
        const uint NERO_FOOTER_V2  = 0x4E455235; // "NER5"
        const uint NERO_CUE_V1     = 0x43554553; // "CUES"
        const uint NERO_CUE_V2     = 0x43554558; // "CUEX"
        const uint NERO_TAO_V0     = 0x54494E46; // "TINF"
        const uint NERO_TAO_V1     = 0x45544E46; // "ETNF"
        const uint NERO_TAO_V2     = 0x45544E32; // "ETN2"
        const uint NERO_DAO_V1     = 0x44414F49; // "DAOI"
        const uint NERO_DAO_V2     = 0x44414F58; // "DAOX"
        const uint NERO_CDTEXT     = 0x43445458; // "CDTX"
        const uint NERO_SESSION    = 0x53494E46; // "SINF"
        const uint NERO_DISC_TYPE  = 0x4D545950; // "MTYP"
        const uint NERO_DISC_INFO  = 0x44494E46; // "DINF"
        const uint NERO_TOC        = 0x544F4354; // "TOCT"
        const uint NERO_RELOCATION = 0x52454C4F; // "RELO"
        const uint NERO_END        = 0x454E4421; // "END!"
    }
}