// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ProDOS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple ProDOS filesystem plugin.
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

// Information from Apple ProDOS 8 Technical Reference
/// <inheritdoc />
/// <summary>Implements detection of Apple ProDOS filesystem</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local"), SuppressMessage("ReSharper", "UnusedType.Local")]
public sealed partial class ProDOSPlugin : IFilesystem
{
    /// <inheritdoc />
    public string Name => Localization.ProDOSPlugin_Name;
    /// <inheritdoc />
    public Guid Id => new("43874265-7B8A-4739-BCF7-07F80D5932BF");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;
}