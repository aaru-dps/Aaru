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
//     Contains properties for Digital Research's DISKCOPY disk images.
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

namespace Aaru.DiscImages;

public sealed partial class DriDiskCopy
{
#region IWritableImage Members

    /// <inheritdoc />
    public ImageInfo Info => _imageInfo;

    /// <inheritdoc />
    public string Name => Localization.DriDiskCopy_Name;

    /// <inheritdoc />
    public Guid Id => new("9F0BE551-8BAB-4038-8B5A-691F1BF5FFF3");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public string Format => "Digital Research DiskCopy";

    /// <inheritdoc />
    public List<DumpHardware> DumpHardware => null;

    /// <inheritdoc />
    public Metadata AaruMetadata => null;

    /// <inheritdoc />
    public IEnumerable<MediaTagType> SupportedMediaTags => Array.Empty<MediaTagType>();

    /// <inheritdoc />
    public IEnumerable<SectorTagType> SupportedSectorTags => Array.Empty<SectorTagType>();

    // TODO: Test with real hardware to see real supported media
    /// <inheritdoc />
    public IEnumerable<MediaType> SupportedMediaTypes => new[]
    {
        MediaType.ACORN_35_DS_DD, MediaType.ACORN_35_DS_HD, MediaType.Apricot_35, MediaType.ATARI_35_DS_DD,
        MediaType.ATARI_35_DS_DD_11, MediaType.ATARI_35_SS_DD, MediaType.ATARI_35_SS_DD_11, MediaType.DMF,
        MediaType.DMF_82, MediaType.DOS_35_DS_DD_8, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_ED,
        MediaType.DOS_35_HD, MediaType.DOS_35_SS_DD_8, MediaType.DOS_35_SS_DD_9, MediaType.DOS_525_DS_DD_8,
        MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_525_SS_DD_8, MediaType.DOS_525_SS_DD_9,
        MediaType.FDFORMAT_35_DD, MediaType.FDFORMAT_35_HD, MediaType.FDFORMAT_525_DD, MediaType.FDFORMAT_525_HD,
        MediaType.RX50, MediaType.XDF_35, MediaType.XDF_525, MediaType.MetaFloppy_Mod_I, MediaType.MetaFloppy_Mod_II
    };

    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions =>
        Array.Empty<(string name, Type type, string description, object @default)>();

    /// <inheritdoc />
    public IEnumerable<string> KnownExtensions => new[] { ".dsk" };

    /// <inheritdoc />
    public bool IsWriting { get; private set; }

    /// <inheritdoc />
    public string ErrorMessage { get; private set; }

#endregion
}