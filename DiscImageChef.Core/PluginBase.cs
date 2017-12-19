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
using DiscImageChef.ImagePlugins;
using DiscImageChef.PartPlugins;
using DiscImageChef.Filesystems;
using DiscImageChef.Console;
using System.Text;

namespace DiscImageChef.Core
{
    public class PluginBase
    {
        public SortedDictionary<string, Filesystem> PluginsList;
        public SortedDictionary<string, PartPlugin> PartPluginsList;
        public SortedDictionary<string, ImagePlugin> ImagePluginsList;

        public PluginBase()
        {
            PluginsList = new SortedDictionary<string, Filesystem>();
            PartPluginsList = new SortedDictionary<string, PartPlugin>();
            ImagePluginsList = new SortedDictionary<string, ImagePlugin>();
        }

        public void RegisterAllPlugins(Encoding encoding = null)
        {
            Assembly assembly;

            assembly = Assembly.GetAssembly(typeof(ImagePlugin));

            foreach(Type type in assembly.GetTypes())
            {
                try
                {
                    if(type.IsSubclassOf(typeof(ImagePlugin)))
                    {
                        ImagePlugin plugin = (ImagePlugin)type.GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
                        RegisterImagePlugin(plugin);
                    }
                }
                catch(Exception exception)
                {
                    DicConsole.ErrorWriteLine("Exception {0}", exception);
                }
            }

            assembly = Assembly.GetAssembly(typeof(PartPlugin));

            foreach(Type type in assembly.GetTypes())
            {
                try
                {
                    if(type.IsSubclassOf(typeof(PartPlugin)))
                    {
                        PartPlugin plugin = (PartPlugin)type.GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
                        RegisterPartPlugin(plugin);
                    }
                }
                catch(Exception exception)
                {
                    DicConsole.ErrorWriteLine("Exception {0}", exception);
                }
            }

            assembly = Assembly.GetAssembly(typeof(Filesystem));

            foreach(Type type in assembly.GetTypes())
            {
                try
                {
                    if(type.IsSubclassOf(typeof(Filesystem)))
                    {
                        Filesystem plugin;
                        if(encoding != null)
                            plugin = (Filesystem)type.GetConstructor(new Type[] { encoding.GetType() }).Invoke(new object[] { encoding });
                        else
                            plugin = (Filesystem)type.GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
                        RegisterPlugin(plugin);
                    }
                }
                catch(Exception exception)
                {
                    DicConsole.ErrorWriteLine("Exception {0}", exception);
                }
            }
        }

        void RegisterImagePlugin(ImagePlugin plugin)
        {
            if(!ImagePluginsList.ContainsKey(plugin.Name.ToLower()))
            {
                ImagePluginsList.Add(plugin.Name.ToLower(), plugin);
            }
        }

        void RegisterPlugin(Filesystem plugin)
        {
            if(!PluginsList.ContainsKey(plugin.Name.ToLower()))
            {
                PluginsList.Add(plugin.Name.ToLower(), plugin);
            }
        }

        void RegisterPartPlugin(PartPlugin partplugin)
        {
            if(!PartPluginsList.ContainsKey(partplugin.Name.ToLower()))
            {
                PartPluginsList.Add(partplugin.Name.ToLower(), partplugin);
            }
        }
    }
}

