// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : PluginBase.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Gets lists of all known plugins.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Core;

/// <summary>Plugin base operations</summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class PluginBase
{
    public static void Init()
    {
        PluginRegister.Singleton.InitPlugins(new List<IPluginRegister>
        {
            new Register(),
            new Filters.Register(),
            new Images.Register(),
            new Aaru.Filesystems.Register(),
            new Aaru.Partitions.Register(),
            new Archives.Register()
        });
    }
}