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
//     Decodes SecureDigital vendor code.
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

namespace Aaru.Decoders.SecureDigital;

/// <summary>Decodes SecureDigital vendors</summary>
public static class VendorString
{
    /// <summary>Converts the byte value of a SecureDigital vendor ID to the manufacturer's name string</summary>
    /// <param name="sdVendorId">SD vendor ID</param>
    /// <returns>Manufacturer</returns>
    public static string Prettify(byte sdVendorId) => sdVendorId switch
                                                      {
                                                          0x41 => "Kingston",
                                                          0x02 => "Kingston",
                                                          0x03 => "Sandisk",
                                                          0x27 => "CnMemory",
                                                          0xAA => "QEMU",
                                                          _ => string.Format(Localization.Unknown_manufacturer_ID_0,
                                                                             sdVendorId)
                                                      };
}