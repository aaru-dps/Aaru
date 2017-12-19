// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using DiscImageChef.CommonTypes;
using DiscImageChef.Filesystems;
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.Core
{
    public static class Filesystems
    {
        public static void Identify(ImagePlugin imagePlugin, out List<string> id_plugins, Partition partition)
        {
            id_plugins = new List<string>();
            PluginBase plugins = new PluginBase();
            plugins.RegisterAllPlugins();

            foreach(Filesystem _plugin in plugins.PluginsList.Values)
            {
                if(_plugin.Identify(imagePlugin, partition))
                    id_plugins.Add(_plugin.Name.ToLower());
            }
        }
    }
}
