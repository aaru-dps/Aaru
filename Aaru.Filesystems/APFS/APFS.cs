// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : APFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple filesystem plugin.
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
/// <summary>Implements detection of the Apple File System (APFS)</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class APFS : IFilesystem
{
    /// <inheritdoc />
    public string Name => Localization.APFS_Name;
    /// <inheritdoc />
    public Guid Id => new("A4060F9D-2909-42E2-9D95-DB31FA7EA797");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;
}