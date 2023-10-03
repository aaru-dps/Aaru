// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CdOffset.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Model for storing Compact Disc read offsets in database.
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

using System;

namespace Aaru.Database.Models;

/// <inheritdoc />
/// <summary>CD read offset</summary>
public class CdOffset : CommonTypes.Metadata.CdOffset
{
    /// <summary>Builds an empty CD read offset</summary>
    public CdOffset() {}

    /// <summary>Builds a CD read offset with the specified parameters</summary>
    /// <param name="manufacturer">Manufacturer</param>
    /// <param name="model">Model</param>
    /// <param name="offset">Read offset</param>
    /// <param name="submissions">Number of submissions</param>
    /// <param name="agreement">Percentage of agreement of submissions</param>
    public CdOffset(string manufacturer, string model, short offset, int submissions, float agreement)
    {
        Manufacturer = manufacturer;
        Model        = model;
        Offset       = offset;
        Submissions  = submissions;
        Agreement    = agreement;
        AddedWhen    = ModifiedWhen = DateTime.UtcNow;
    }

    /// <summary>Builds a CD read offset from the metadata type</summary>
    /// <param name="offset">Read offset metadata</param>
    public CdOffset(CommonTypes.Metadata.CdOffset offset)
    {
        Manufacturer = offset.Manufacturer;
        Model        = offset.Model;
        Offset       = offset.Offset;
        Submissions  = offset.Submissions;
        Agreement    = offset.Agreement;
        AddedWhen    = ModifiedWhen = DateTime.UtcNow;
    }

    /// <summary>Database ID</summary>
    public int Id { get; set; }

    /// <summary>Date when model has been added to the database</summary>
    public DateTime AddedWhen { get; set; }

    /// <summary>Date when model was last modified</summary>
    public DateTime ModifiedWhen { get; set; }
}