// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : XFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : XFS filesystem plugin.
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
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of SGI's XFS</summary>
public sealed partial class XFS : IFilesystem
{
    const string MODULE_NAME = "XFS plugin";

#region IFilesystem Members

    /// <inheritdoc />
    public string Name => Localization.XFS_Name;

    /// <inheritdoc />
    public Guid Id => new("1D8CD8B8-27E6-410F-9973-D16409225FBA");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

#endregion
}