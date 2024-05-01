// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : UsbVendorDto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru Server.
//
// --[ Description ] ----------------------------------------------------------
//
//     DTO for syncing USB vendor identifiers in database.
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
// ****************************************************************************/

namespace Aaru.Dto;

/// <summary>DTO for USB vendor field</summary>
public class UsbVendorDto
{
    /// <summary>Vendor ID</summary>
    public ushort VendorId { get; set; }

    /// <summary>Vendor</summary>
    public string Vendor { get; set; }
}