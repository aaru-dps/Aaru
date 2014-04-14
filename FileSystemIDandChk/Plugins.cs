using System;
using System.Collections.Generic;
using System.Reflection;
using FileSystemIDandChk.ImagePlugins;
using FileSystemIDandChk.PartPlugins;
using FileSystemIDandChk.Plugins;

namespace FileSystemIDandChk
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

