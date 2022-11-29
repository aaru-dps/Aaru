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
//     Contains properties for DiscJuggler disc images.
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

using System;
using System.Collections.Generic;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Structs;
using Schemas;

namespace Aaru.DiscImages;

public sealed partial class DiscJuggler
{
    /// <inheritdoc />
    public string Name => Localization.DiscJuggler_Name;
    /// <inheritdoc />
    public Guid Id => new("2444DBC6-CD35-424C-A227-39B0C4DB01B2");
    /// <inheritdoc />
    public ImageInfo Info => _imageInfo;
    /// <inheritdoc />
    public string Format => "DiscJuggler";
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;
    /// <inheritdoc />
    public List<Partition> Partitions { get; private set; }
    /// <inheritdoc />
    public List<Track> Tracks { get; private set; }
    /// <inheritdoc />
    public List<Session> Sessions { get; private set; }
    /// <inheritdoc />
    public List<DumpHardwareType> DumpHardware => null;
    /// <inheritdoc />
    public CICMMetadataType CicmMetadata => null;
}