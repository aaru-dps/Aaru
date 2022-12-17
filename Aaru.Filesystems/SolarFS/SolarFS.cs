// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SolarFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SolarOS filesystem plugin.
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
using System.Text;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Filesystems;

// Based on FAT's BPB, cannot find a FAT or directory
/// <inheritdoc />
/// <summary>Implements detection of the Solar OS filesystem</summary>
public sealed partial class SolarFS : IFilesystem
{
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => Localization.SolarFS_Name;
    /// <inheritdoc />
    public Guid Id => new("EA3101C1-E777-4B4F-B5A3-8C57F50F6E65");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;
}