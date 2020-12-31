// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains enumerations for Alcohol 120% disc images.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;

namespace Aaru.DiscImages
{
    public sealed partial class Alcohol120
    {
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        enum MediumType : ushort
        {
            CD  = 0x00, CDR  = 0x01, CDRW = 0x02,
            DVD = 0x10, DVDR = 0x12
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        enum TrackMode : byte
        {
            NoData  = 0x00, DVD        = 0x02, Audio      = 0xA9,
            Mode1   = 0xAA, Mode2      = 0xAB, Mode2F1    = 0xEC,
            Mode2F2 = 0xED, Mode2F1Alt = 0xAC, Mode2F2Alt = 0xAD
        }

        enum SubchannelMode : byte
        {
            None = 0x00, Interleaved = 0x08
        }
    }
}