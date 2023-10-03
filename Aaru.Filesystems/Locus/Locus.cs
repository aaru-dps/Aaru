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
using commitcnt_t = System.Int32;

// Disk address
using daddr_t = System.Int32;

// Fstore
using fstore_t = System.Int32;

// Global File System number
using gfs_t = System.Int32;

// Inode number
using ino_t = System.Int32;

// Filesystem pack number
using pckno_t = System.Int16;

// Timestamp
using time_t = System.Int32;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Local

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the Locus filesystem</summary>
public sealed partial class Locus : IFilesystem
{
    /// <inheritdoc />
    public string Name => Localization.Locus_Name;
    /// <inheritdoc />
    public Guid Id => new("1A70B30A-437D-479A-88E1-D0C9C1797FF4");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;
    const string MODULE_NAME = "Locus plugin";
}