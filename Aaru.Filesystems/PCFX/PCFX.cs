// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : PCEngine.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : NEC PC-FX plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the NEC PC-FX track header and shows information.
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
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Filesystems;

// Not a filesystem, more like an executable header
/// <inheritdoc />
/// <summary>Implements detection of NEC PC-FX headers</summary>
public sealed partial class PCFX : IFilesystem
{
    /// <inheritdoc />
    public string Name => Localization.PCFX_Name;
    /// <inheritdoc />
    public Guid Id => new("8BC27CCE-D9E9-48F8-BA93-C66A86EB565A");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;
}