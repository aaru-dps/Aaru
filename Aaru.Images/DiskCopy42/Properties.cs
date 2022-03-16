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
//     Contains properties for Apple DiskCopy 4.2 disk images.
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

public sealed partial class DiskCopy42
{
    /// <inheritdoc />
    public ImageInfo Info => imageInfo;
    /// <inheritdoc />
    public string Name => "Apple DiskCopy 4.2";
    /// <inheritdoc />
    public Guid Id => new("0240B7B1-E959-4CDC-B0BD-386D6E467B88");
    /// <inheritdoc />
    public string Author => "Natalia Portillo";
    /// <inheritdoc />
    public List<DumpHardwareType> DumpHardware => null;
    /// <inheritdoc />
    public CICMMetadataType CicmMetadata => null;
    /// <inheritdoc />
    public string Format => "Apple DiskCopy 4.2";
    /// <inheritdoc />
    public IEnumerable<MediaTagType> SupportedMediaTags => Array.Empty<MediaTagType>();
    /// <inheritdoc />
    public IEnumerable<SectorTagType> SupportedSectorTags => new[]
    {
        SectorTagType.AppleSectorTag
    };
    /// <inheritdoc />
    public IEnumerable<MediaType> SupportedMediaTypes => new[]
    {
        MediaType.AppleFileWare, MediaType.AppleHD20, MediaType.AppleProfile, MediaType.AppleSonyDS,
        MediaType.AppleSonySS, MediaType.AppleWidget, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD, MediaType.DMF
    };
    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions => new[]
    {
        ("macosx", typeof(bool), "Use Mac OS X format byte", (object)false)
    };
    /// <inheritdoc />
    public IEnumerable<string> KnownExtensions => new[]
    {
        ".dc42", ".diskcopy42", ".image"
    };
    /// <inheritdoc />
    public bool IsWriting { get; private set; }
    /// <inheritdoc />
    public string ErrorMessage { get; private set; }
}