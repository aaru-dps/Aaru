﻿// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Filesystems.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Logic to use filesystem plugins.
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

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;

namespace Aaru.Core;

/// <summary>Core filesystem operations</summary>
public static class Filesystems
{
    /// <summary>
    ///     Traverses all known filesystems and outputs a list of all that recognized what is in the specified image and
    ///     partition
    /// </summary>
    /// <param name="imagePlugin">Media image</param>
    /// <param name="idPlugins">List of plugins recognizing the filesystem</param>
    /// <param name="partition">Partition</param>
    /// <param name="getGuid">Gets plugin GUID</param>
    public static void Identify(IMediaImage imagePlugin, out List<string> idPlugins, Partition partition,
                                bool        getGuid = false)
    {
        PluginRegister plugins = PluginRegister.Singleton;

        idPlugins = [];

        foreach(IFilesystem plugin in plugins.Filesystems.Values.Where(p => p is not null))
        {
            try
            {
                if(plugin.Identify(imagePlugin, partition))
                    idPlugins.Add(getGuid ? plugin.Id.ToString() : plugin.Name.ToLower());
            }
            catch(Exception ex)
            {
                AaruConsole
                   .ErrorWriteLine("Error identifying filesystem {0}. Please open a report with the following line in a Github issue.",
                                   plugin.Name);

                AaruConsole.WriteException(ex);
            }
        }
    }
}