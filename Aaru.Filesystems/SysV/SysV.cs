// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SysV.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : UNIX System V filesystem plugin.
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

// ReSharper disable NotAccessedField.Local

using System;
using System.Diagnostics.CodeAnalysis;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Filesystems;

// Information from the Linux kernel
/// <inheritdoc />
/// <summary>Implements detection of the UNIX System V filesystem</summary>
[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "UnusedMember.Local"),
 SuppressMessage("ReSharper", "UnusedType.Local")]
public sealed partial class SysVfs : IFilesystem
{
    /// <inheritdoc />
    public string Name => Localization.SysVfs_Name;
    /// <inheritdoc />
    public Guid Id => new("9B8D016A-8561-400E-A12A-A198283C211D");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;
}