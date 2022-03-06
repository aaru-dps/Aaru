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
//     Contains helpers for CPCEMU disk images.
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

using Aaru.Decoders.Floppy;

namespace Aaru.DiscImages;

public sealed partial class Cpcdsk
{
    static int SizeCodeToBytes(IBMSectorSizeCode code)
    {
        switch(code)
        {
            case IBMSectorSizeCode.EighthKilo:       return 128;
            case IBMSectorSizeCode.QuarterKilo:      return 256;
            case IBMSectorSizeCode.HalfKilo:         return 512;
            case IBMSectorSizeCode.Kilo:             return 1024;
            case IBMSectorSizeCode.TwiceKilo:        return 2048;
            case IBMSectorSizeCode.FriceKilo:        return 4096;
            case IBMSectorSizeCode.TwiceFriceKilo:   return 8192;
            case IBMSectorSizeCode.FricelyFriceKilo: return 16384;
            default:                                 return 0;
        }
    }
}