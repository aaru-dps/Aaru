// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : BSD Fast File System plugin.
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
using System.Diagnostics.CodeAnalysis;
using Aaru.CommonTypes.Interfaces;
using time_t = int;
using ufs_daddr_t = int;

namespace Aaru.Filesystems;

// Using information from Linux kernel headers
/// <inheritdoc />
/// <summary>Implements detection of BSD Fast File System (FFS, aka UNIX File System)</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed partial class FFSPlugin : IFilesystem
{
    const string MODULE_NAME = "FFS plugin";

#region IFilesystem Members

    /// <inheritdoc />
    public string Name => Localization.FFSPlugin_Name;

    /// <inheritdoc />
    public Guid Id => new("CC90D342-05DB-48A8-988C-C1FE000034A3");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

#endregion
}