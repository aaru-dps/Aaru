// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BaseOperatingSystem.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Base operating system abstract class.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Database.Models;

/// <inheritdoc />
/// <summary>Operating system statistics</summary>
public abstract class BaseOperatingSystem : BaseModel
{
    /// <summary>Operating system name</summary>
    public string Name { get; set; }
    /// <summary>Operating system version</summary>
    public string Version { get; set; }
    /// <summary>Has already been synchronized with Aaru's server</summary>
    public bool Synchronized { get; set; }
    /// <summary>Statistical count</summary>
    public ulong Count { get; set; }
}