// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : UNICOS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : UNICOS filesystem plugin.
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

// UNICOS is ILP64 so let's think everything is 64-bit

using System;
using Aaru.CommonTypes.Interfaces;
using blkno_t = System.Int64;
using daddr_t = System.Int64;
using dev_t = System.Int64;
using extent_t = System.Int64;
using ino_t = System.Int64;
using time_t = System.Int64;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection for the Cray UNICOS filesystem</summary>
public sealed partial class UNICOS : IFilesystem
{
    /// <inheritdoc />
    public string Name => Localization.UNICOS_Name;
    /// <inheritdoc />
    public Guid Id => new("61712F04-066C-44D5-A2A0-1E44C66B33F0");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;
    const string MODULE_NAME = "UNICOS plugin";
}