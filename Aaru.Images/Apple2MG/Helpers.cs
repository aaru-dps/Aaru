// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Helpers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains helpers for XGS emulator disk images.
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

using Aaru.CommonTypes;

namespace Aaru.DiscImages
{
    public sealed partial class Apple2Mg
    {
        MediaType GetMediaType()
        {
            switch(_imageInfo.Sectors)
            {
                case 455:  return MediaType.Apple32SS;
                case 910:  return MediaType.Apple32DS;
                case 560:  return MediaType.Apple33SS;
                case 1120: return MediaType.Apple33DS;
                case 800:  return MediaType.AppleSonySS;
                case 1600: return MediaType.AppleSonyDS;
                case 2880: return MediaType.DOS_35_HD;
                default:   return MediaType.Unknown;
            }
        }
    }
}