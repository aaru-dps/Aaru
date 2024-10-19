// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : PCEngine.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : NEC PC-Engine CD filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the NEC PC-Engine CD filesystem and shows information.
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

using System;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the PC-Engine CD file headers</summary>
public sealed partial class PCEnginePlugin : IFilesystem
{
#region IFilesystem Members

    /// <inheritdoc />
    public string Name => Localization.PCEnginePlugin_Name;

    /// <inheritdoc />
    public Guid Id => new("e5ee6d7c-90fa-49bd-ac89-14ef750b8af3");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

#endregion
}