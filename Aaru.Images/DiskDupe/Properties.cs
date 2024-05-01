// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Properties.cs
// Author(s)      : Michael Drüing <michael@drueing.de>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains properties for DiskDupe DDI disk images.
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
// Copyright © 2021-2024 Michael Drüing
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Structs;

namespace Aaru.Images;

public sealed partial class DiskDupe
{
#region IMediaImage Members

    /// <inheritdoc />
    public string Name => Localization.DiskDupe_Name;

    /// <inheritdoc />
    public Guid Id => new("5439B4A2-5F38-33A7-B8DC-3910D296B3DD");

    /// <inheritdoc />
    public string Author => Authors.MichaelDruing;

    /// <inheritdoc />
    public string Format => "DDI disk image";

    /// <inheritdoc />

    // ReSharper disable once ConvertToAutoProperty
    public ImageInfo Info => _imageInfo;

    /// <inheritdoc />
    public List<DumpHardware> DumpHardware => null;

    /// <inheritdoc />
    public Metadata AaruMetadata => null;

#endregion
}