using System;
using System.Collections.Generic;
using DiscImageChef.ImagePlugins;
using DiscImageChef.PartPlugins;
using DiscImageChef.Plugins;

namespace DiscImageChef.Commands
{
    public static class Formats
    {
        public static void ListFormats()
        {
            PluginBase plugins = new PluginBase();
            plugins.RegisterAllPlugins();

            Console.WriteLine("Supported images:");
            foreach (KeyValuePair<string, ImagePlugin> kvp in plugins.ImagePluginsList)
                Console.WriteLine(kvp.Value.Name);
            Console.WriteLine();
            Console.WriteLine("Supported filesystems:");
            foreach (KeyValuePair<string, Plugin> kvp in plugins.PluginsList)
                Console.WriteLine(kvp.Value.Name);
            Console.WriteLine();
            Console.WriteLine("Supported partitions:");
            foreach (KeyValuePair<string, PartPlugin> kvp in plugins.PartPluginsList)
                Console.WriteLine(kvp.Value.Name);
        }
    }
}

