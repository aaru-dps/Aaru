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
//     Contains properties for MAME Compressed Hunks of Data disk images.
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
using System.Linq;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Structs;
using Partition = Aaru.CommonTypes.Partition;
using Track = Aaru.CommonTypes.Structs.Track;

namespace Aaru.Images;

public sealed partial class Chd
{
#region IOpticalMediaImage Members

    /// <inheritdoc />

    // ReSharper disable once ConvertToAutoProperty
    public ImageInfo Info => _imageInfo;

    /// <inheritdoc />
    public string Name => Localization.Chd_Name;

    /// <inheritdoc />
    public Guid Id => new("0D50233A-08BD-47D4-988B-27EAA0358597");

    /// <inheritdoc />
    public string Format => "Compressed Hunks of Data";

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public List<Partition> Partitions => _isHdd ? null : _partitions;

    /// <inheritdoc />
    public List<Track> Tracks => _isHdd ? null : _tracks.Values.ToList();

    /// <inheritdoc />
    public List<Session> Sessions
    {
        get
        {
            if(_isHdd) return null;

            // TODO: Implement
            return null;
        }
    }

    /// <inheritdoc />
    public List<DumpHardware> DumpHardware => null;

    /// <inheritdoc />
    public Metadata AaruMetadata => null;

#endregion
}