// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : PluginBase.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Class to hold all installed plugins.
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

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using DiscImageChef.Console;
using DiscImageChef.DiscImages;
using DiscImageChef.Filesystems;
using DiscImageChef.Partitions;

namespace DiscImageChef.Core
{
    /// <summary>
    ///     Contain all plugins (filesystem, partition and image)
    /// </summary>
    public class PluginBase
    {
        /// <summary>
        ///     List of all media image plugins
        /// </summary>
        public SortedDictionary<string, ImagePlugin> ImagePluginsList;
        /// <summary>
        ///     List of all partition plugins
        /// </summary>
        public SortedDictionary<string, PartitionPlugin> PartPluginsList;
        /// <summary>
        ///     List of all filesystem plugins
        /// </summary>
        public SortedDictionary<string, Filesystem> PluginsList;

        /// <summary>
        ///     Initializes the plugins lists
        /// </summary>
        public PluginBase()
        {
            PluginsList = new SortedDictionary<string, Filesystem>();
            PartPluginsList = new SortedDictionary<string, PartitionPlugin>();
            ImagePluginsList = new SortedDictionary<string, ImagePlugin>();
        }

        /// <summary>
        ///     Fills the plugins lists
        /// </summary>
        /// <param name="encoding">Which encoding to pass to plugins</param>
        public void RegisterAllPlugins(Encoding encoding = null)
        {
            Assembly assembly = Assembly.GetAssembly(typeof(ImagePlugin));

            foreach(Type type in assembly.GetTypes())
                try
                {
                    if(!type.IsSubclassOf(typeof(ImagePlugin))) continue;

                    ImagePlugin plugin = (ImagePlugin)type.GetConstructor(Type.EmptyTypes)?.Invoke(new object[] { });
                    RegisterImagePlugin(plugin);
                }
                catch(Exception exception) { DicConsole.ErrorWriteLine("Exception {0}", exception); }

            assembly = Assembly.GetAssembly(typeof(PartitionPlugin));

            foreach(Type type in assembly.GetTypes())
                try
                {
                    if(!type.IsSubclassOf(typeof(PartitionPlugin))) continue;

                    PartitionPlugin plugin =
                        (PartitionPlugin)type.GetConstructor(Type.EmptyTypes)?.Invoke(new object[] { });
                    RegisterPartPlugin(plugin);
                }
                catch(Exception exception) { DicConsole.ErrorWriteLine("Exception {0}", exception); }

            assembly = Assembly.GetAssembly(typeof(Filesystem));

            foreach(Type type in assembly.GetTypes())
                try
                {
                    if(!type.IsSubclassOf(typeof(Filesystem))) continue;

                    Filesystem plugin;
                    if(encoding != null)
                        plugin = (Filesystem)type.GetConstructor(new[] {encoding.GetType()})
                                                 ?.Invoke(new object[] {encoding});
                    else plugin = (Filesystem)type.GetConstructor(Type.EmptyTypes)?.Invoke(new object[] { });
                    RegisterPlugin(plugin);
                }
                catch(Exception exception) { DicConsole.ErrorWriteLine("Exception {0}", exception); }
        }

        void RegisterImagePlugin(ImagePlugin plugin)
        {
            if(!ImagePluginsList.ContainsKey(plugin.Name.ToLower()))
                ImagePluginsList.Add(plugin.Name.ToLower(), plugin);
        }

        void RegisterPlugin(Filesystem plugin)
        {
            if(!PluginsList.ContainsKey(plugin.Name.ToLower())) PluginsList.Add(plugin.Name.ToLower(), plugin);
        }

        void RegisterPartPlugin(PartitionPlugin partplugin)
        {
            if(!PartPluginsList.ContainsKey(partplugin.Name.ToLower()))
                PartPluginsList.Add(partplugin.Name.ToLower(), partplugin);
        }
    }
}