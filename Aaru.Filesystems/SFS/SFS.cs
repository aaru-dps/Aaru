// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SmartFileSystem plugin.
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

/// <inheritdoc />
/// <summary>Implements detection of the Smart File System</summary>
public sealed partial class SFS : IFilesystem
{
    /// <inheritdoc />
    public string Name => Localization.SFS_Name;
    /// <inheritdoc />
    public Guid Id => new("26550C19-3671-4A2D-BC2F-F20CEB7F48DC");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;
}