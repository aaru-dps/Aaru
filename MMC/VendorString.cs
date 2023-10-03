// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : VendorString.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes MultiMediaCard vendor code.
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

using System.Diagnostics.CodeAnalysis;

namespace Aaru.Decoders.MMC;

/// <summary>Decodes MultiMediaCard vendors</summary>
public static class VendorString
{
    /// <summary>Converts the byte value of a MultiMediaCard vendor ID to the manufacturer's name string</summary>
    /// <param name="mmcVendorId">MMC vendor ID</param>
    /// <returns>Manufacturer</returns>
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public static string Prettify(byte mmcVendorId) => mmcVendorId switch
                                                       {
                                                           0x07 => "Nokia",
                                                           0x15 => "Samsung",
                                                           0x2C => "extreMEmory",
                                                           _ => string.Format(
                                                               Localization.Unknown_manufacturer_ID_0, mmcVendorId)
                                                       };
}