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
//     Contains properties for VMware disk images.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.DiscImages;

using System;
using System.Collections.Generic;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Schemas;

public sealed partial class VMware
{
    /// <inheritdoc />
    public ImageInfo Info => _imageInfo;

    /// <inheritdoc />
    public string Name => "VMware disk image";
    /// <inheritdoc />
    public Guid Id => new("E314DE35-C103-48A3-AD36-990F68523C46");
    /// <inheritdoc />
    public string Author => "Natalia Portillo";
    /// <inheritdoc />
    public string Format => "VMware";
    /// <inheritdoc />
    public List<DumpHardwareType> DumpHardware => null;
    /// <inheritdoc />
    public CICMMetadataType CicmMetadata => null;
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
        ("adapter_type", typeof(string), "Type of adapter type. Possible values: ide, lsilogic, buslogic, legacyESX.",
         "ide"),
        ("hwversion", typeof(uint), "VDMK hardware version.", 4),
        ("sparse", typeof(bool), "Use sparse extents.", false),
        ("split", typeof(bool), "Split data file at 2GiB.", (object)false)
    };
    /// <inheritdoc />
    public IEnumerable<string> KnownExtensions => new[]
    {
        ".vmdk"
    };
    /// <inheritdoc />
    public bool IsWriting { get; private set; }
    /// <inheritdoc />
    public string ErrorMessage { get; private set; }
}