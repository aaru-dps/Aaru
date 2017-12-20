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
using DiscImageChef.DiscImages;

namespace DiscImageChef.Core
{
    public static class Filesystems
    {
        public static void Identify(ImagePlugin imagePlugin, out List<string> idPlugins, Partition partition)
        {
            idPlugins = new List<string>();
            PluginBase plugins = new PluginBase();
            plugins.RegisterAllPlugins();

            foreach(Filesystem plugin in plugins.PluginsList.Values) if(plugin.Identify(imagePlugin, partition)) idPlugins.Add(plugin.Name.ToLower());
        }
    }
}