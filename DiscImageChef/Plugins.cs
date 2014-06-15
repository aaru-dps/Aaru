/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : Plugins.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Base methods for plugins.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$

using System;
using System.Collections.Generic;
using System.Reflection;
using DiscImageChef.ImagePlugins;
using DiscImageChef.PartPlugins;
using DiscImageChef.Plugins;

namespace DiscImageChef
{
    public class PluginBase
    {
        public Dictionary<string, Plugin> PluginsList;
        public Dictionary<string, PartPlugin> PartPluginsList;
        public Dictionary<string, ImagePlugin> ImagePluginsList;

        public PluginBase()
        {
            PluginsList = new Dictionary<string, Plugin>();
            PartPluginsList = new Dictionary<string, PartPlugin>();
            ImagePluginsList = new Dictionary<string, ImagePlugin>();
        }

        public void RegisterAllPlugins()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            foreach (Type type in assembly.GetTypes())
            {
                try
                {
                    if (type.IsSubclassOf(typeof(ImagePlugin)))
                    {
                        ImagePlugin plugin = (ImagePlugin)type.GetConstructor(new [] { typeof(PluginBase) }).Invoke(new object[] { this });
                        RegisterImagePlugin(plugin);
                    }
                    if (type.IsSubclassOf(typeof(Plugin)))
                    {
                        Plugin plugin = (Plugin)type.GetConstructor(new [] { typeof(PluginBase) }).Invoke(new object[] { this });
                        RegisterPlugin(plugin);
                    }
                    else if (type.IsSubclassOf(typeof(PartPlugin)))
                    {
                        PartPlugin partplugin = (PartPlugin)type.GetConstructor(new [] { typeof(PluginBase) }).Invoke(new object[] { this });
                        RegisterPartPlugin(partplugin);
                    }
					
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }

        void RegisterImagePlugin(ImagePlugin plugin)
        {
            if (!ImagePluginsList.ContainsKey(plugin.Name.ToLower()))
            {
                ImagePluginsList.Add(plugin.Name.ToLower(), plugin);
            }
        }

        void RegisterPlugin(Plugin plugin)
        {
            if (!PluginsList.ContainsKey(plugin.Name.ToLower()))
            {
                PluginsList.Add(plugin.Name.ToLower(), plugin);
            }
        }

        void RegisterPartPlugin(PartPlugin partplugin)
        {
            if (!PartPluginsList.ContainsKey(partplugin.Name.ToLower()))
            {
                PartPluginsList.Add(partplugin.Name.ToLower(), partplugin);
            }
        }
    }
}

