// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HAMMER.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : HAMMER filesystem plugin.
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

#pragma warning disable 169

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection for the HAMMER filesystem</summary>
public sealed partial class HAMMER : IFilesystem
{
#region IFilesystem Members

    /// <inheritdoc />
    public string Name => Localization.HAMMER_Name;

    /// <inheritdoc />
    public Guid Id => new("91A188BF-5FD7-4677-BBD3-F59EBA9C864D");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

#endregion
}