﻿// /***************************************************************************
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
//     Contains properties for Connectix and Microsoft Virtual PC disk images.
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

using System;
using System.Collections.Generic;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;

namespace Aaru.Images;

public sealed partial class Vhd
{
#region IWritableImage Members

    /// <inheritdoc />

    // ReSharper disable once ConvertToAutoProperty
    public ImageInfo Info => _imageInfo;

    /// <inheritdoc />
    public string Name => Localization.Vhd_Name;

    /// <inheritdoc />
    public Guid Id => new("8014d88f-64cd-4484-9441-7635c632958a");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public string Format => _thisFooter.DiskType switch
                            {
                                TYPE_FIXED        => "Virtual PC fixed size disk image",
                                TYPE_DYNAMIC      => "Virtual PC dynamic size disk image",
                                TYPE_DIFFERENCING => "Virtual PC differencing disk image",
                                _                 => "Virtual PC disk image"
                            };

    /// <inheritdoc />
    public List<DumpHardware> DumpHardware => null;

    /// <inheritdoc />
    public Metadata AaruMetadata => null;

    /// <inheritdoc />
    public IEnumerable<MediaTagType> SupportedMediaTags => Array.Empty<MediaTagType>();

    /// <inheritdoc />
    public IEnumerable<SectorTagType> SupportedSectorTags => Array.Empty<SectorTagType>();

    /// <inheritdoc />
    public IEnumerable<MediaType> SupportedMediaTypes => new[]
    {
        MediaType.GENERIC_HDD, MediaType.Unknown, MediaType.FlashDrive, MediaType.CompactFlash,
        MediaType.CompactFlashType2, MediaType.PCCardTypeI, MediaType.PCCardTypeII, MediaType.PCCardTypeIII,
        MediaType.PCCardTypeIV
    };

    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions => new[]
    {
        ("dynamic", typeof(bool), Localization.Create_a_dynamic_image, (object)true),
        ("block_size", typeof(uint), Localization.Vhd_Block_size_Must_be_a_power_of_2, 2097152)
    };

    /// <inheritdoc />
    public IEnumerable<string> KnownExtensions => new[]
    {
        ".vhd"
    };

    /// <inheritdoc />
    public bool IsWriting { get; private set; }

    /// <inheritdoc />
    public string ErrorMessage { get; private set; }

#endregion
}