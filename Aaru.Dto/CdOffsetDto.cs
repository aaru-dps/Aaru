// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CdOffsetDto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DTOs.
//
// --[ Description ] ----------------------------------------------------------
//
//     DTO for syncing Compact Disc read offsets.
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

using System.Diagnostics.CodeAnalysis;
using Aaru.CommonTypes.Metadata;

namespace Aaru.Dto;

/// <inheritdoc />
/// <summary>DTO from a CD drive read offset</summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class CdOffsetDto : CdOffset
{
    /// <summary>Build an empty DTO</summary>
    public CdOffsetDto() {}

    /// <summary>Build a DTO using the specified offset and database ID</summary>
    /// <param name="offset">CD reading offset</param>
    /// <param name="id">Database ID</param>
    public CdOffsetDto(CdOffset offset, int id)
    {
        Manufacturer = offset.Manufacturer;
        Model        = offset.Model;
        Offset       = offset.Offset;
        Submissions  = offset.Submissions;
        Agreement    = offset.Agreement;
        Id           = id;
    }

    /// <summary>Database ID</summary>
    public int Id { get; set; }
}