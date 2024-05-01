// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MicroDOS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : MicroDOS filesystem plugin
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local

using System;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>
///     Implements detection for the MicroDOS filesystem. Information from http://www.owg.ru/mkt/BK/MKDOS.TXT Thanks
///     to tarlabnor for translating it
/// </summary>
public sealed partial class MicroDOS : IFilesystem
{
#region IFilesystem Members

    /// <inheritdoc />
    public string Name => Localization.MicroDOS_Name;

    /// <inheritdoc />
    public Guid Id => new("9F9A364A-1A27-48A3-B730-7A7122000324");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

#endregion
}