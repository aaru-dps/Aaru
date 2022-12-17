// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : VMfs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : VMware file system plugin.
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

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the VMware filesystem</summary>
[SuppressMessage("ReSharper", "UnusedType.Local"), SuppressMessage("ReSharper", "IdentifierTypo"),
 SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class VMfs : IFilesystem
{
    /// <inheritdoc />
    public string Name => Localization.VMfs_Name;
    /// <inheritdoc />
    public Guid Id => new("EE52BDB8-B49C-4122-A3DA-AD21CBE79843");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;
}