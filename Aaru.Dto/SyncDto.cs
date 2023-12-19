// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SyncDto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru Server.
//
// --[ Description ] ----------------------------------------------------------
//
//     DTO for syncing server and client databases.
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

using System.Collections.Generic;

namespace Aaru.Dto;

/// <summary>DTO for database synchronization.</summary>
public class SyncDto
{
    /// <summary>List of USB vendors</summary>
    public List<UsbVendorDto> UsbVendors { get; set; }

    /// <summary>List of USB products</summary>
    public List<UsbProductDto> UsbProducts { get; set; }

    /// <summary>List of CD read offsets</summary>
    public List<CdOffsetDto> Offsets { get; set; }

    /// <summary>List of known devices</summary>
    public List<DeviceDto> Devices { get; set; }

    /// <summary>List of known iNES/NES 2.0 headers</summary>
    public List<NesHeaderDto> NesHeaders { get; set; }
}