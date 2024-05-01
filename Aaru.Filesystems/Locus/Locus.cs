// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Locus.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Locus filesystem plugin
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
//     License aint with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

// Commit count

using System;
using Aaru.CommonTypes.Interfaces;

// Disk address

// Fstore

// Global File System number

// Inode number

// Filesystem pack number

// Timestamp

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Local

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the Locus filesystem</summary>
public sealed partial class Locus : IFilesystem
{
    const string MODULE_NAME = "Locus plugin";

#region IFilesystem Members

    /// <inheritdoc />
    public string Name => Localization.Locus_Name;

    /// <inheritdoc />
    public Guid Id => new("1A70B30A-437D-479A-88E1-D0C9C1797FF4");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

#endregion
}