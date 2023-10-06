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
//     Contains properties for SuperCardPro flux images.
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
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;

namespace Aaru.Images;

public sealed partial class SuperCardPro
{
    /// <summary>
    ///     SCP can only have one resolution. This is to help avoid changing the resolution and therefore create broken
    ///     SCP files.
    /// </summary>
    bool IsResolutionSet { get; set; }

    /// <summary>
    ///     SCP can only have the same amount of revolutions for all tracks. This is to help avoid changing the number of
    ///     revolutions and therefore create broken SCP files.
    /// </summary>
    bool IsRevolutionsSet { get; set; }

#region IFluxImage Members

    /// <inheritdoc />
    // ReSharper disable once ConvertToAutoProperty
    public ImageInfo Info => _imageInfo;

    /// <inheritdoc />
    public string Name => Localization.SuperCardPro_Name;

    /// <inheritdoc />
    public Guid Id => new("C5D3182E-1D45-4767-A205-E6E5C83444DC");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public string Format => "SuperCardPro";

    /// <inheritdoc />
    public List<DumpHardware> DumpHardware => null;

    /// <inheritdoc />
    public Metadata AaruMetadata => null;

#endregion

#region IWritableImage Members

    /// <inheritdoc />
    public IEnumerable<MediaTagType> SupportedMediaTags => null;

    /// <inheritdoc />
    public IEnumerable<MediaType> SupportedMediaTypes => new[]
    {
        // TODO: SCP supports a lot more formats, please add more whence tested.
        MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD, MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD,
        MediaType.Unknown
    };

    /// <inheritdoc />
    public IEnumerable<SectorTagType> SupportedSectorTags => Array.Empty<SectorTagType>();

    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions =>
        Array.Empty<(string name, Type type, string description, object @default)>();

    /// <inheritdoc />
    public IEnumerable<string> KnownExtensions => new[]
    {
        ".scp"
    };

    /// <inheritdoc />
    public bool IsWriting { get; private set; }

    /// <inheritdoc />
    public string ErrorMessage { get; private set; }

#endregion
}