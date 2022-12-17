// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : RT11.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : RT-11 file system plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the RT-11 file system and shows information.
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
using System.Text;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Filesystems;

// Information from http://www.trailing-edge.com/~shoppa/rt11fs/
/// <inheritdoc />
/// <summary>Implements detection of the DEC RT-11 filesystem</summary>
public sealed partial class RT11 : IFilesystem
{
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => Localization.RT11_Name;
    /// <inheritdoc />
    public Guid Id => new("DB3E2F98-8F98-463C-8126-E937843DA024");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;
}