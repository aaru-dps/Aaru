// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Properties.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains properties for Apple nibbelized disk images.
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
using System.Collections.Generic;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Structs;

namespace Aaru.DiscImages;

public sealed partial class AppleNib
{
#region IMediaImage Members

    /// <inheritdoc />
    // ReSharper disable once ConvertToAutoProperty
    public ImageInfo Info => _imageInfo;

    /// <inheritdoc />
    public string Name => Localization.AppleNib_Name;

    /// <inheritdoc />
    public Guid Id => new("AE171AE8-6747-49CC-B861-9D450B7CD42E");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public string Format => "Apple nibbles";

    /// <inheritdoc />
    public List<DumpHardware> DumpHardware => null;

    /// <inheritdoc />
    public Metadata AaruMetadata => null;

#endregion
}