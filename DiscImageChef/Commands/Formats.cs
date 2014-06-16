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
            if(MainClass.isVerbose)
                Console.WriteLine("GUID\t\t\t\t\tPlugin");
            foreach (KeyValuePair<string, ImagePlugin> kvp in plugins.ImagePluginsList)
            {
                if(MainClass.isVerbose)
                    Console.WriteLine("{0}\t{1}", kvp.Value.PluginUUID, kvp.Value.Name);
                else
                    Console.WriteLine(kvp.Value.Name);
            }
            Console.WriteLine();
            Console.WriteLine("Supported filesystems:");
            if(MainClass.isVerbose)
                Console.WriteLine("GUID\t\t\t\t\tPlugin");
            foreach (KeyValuePair<string, Plugin> kvp in plugins.PluginsList)
            {
                if(MainClass.isVerbose)
                    Console.WriteLine("{0}\t{1}", kvp.Value.PluginUUID, kvp.Value.Name);
                else
                    Console.WriteLine(kvp.Value.Name);
            }
            Console.WriteLine();
            Console.WriteLine("Supported partitions:");
            if(MainClass.isVerbose)
                Console.WriteLine("GUID\t\t\t\t\tPlugin");
            foreach (KeyValuePair<string, PartPlugin> kvp in plugins.PartPluginsList)
            {
                if(MainClass.isVerbose)
                    Console.WriteLine("{0}\t{1}", kvp.Value.PluginUUID, kvp.Value.Name);
                else
                    Console.WriteLine(kvp.Value.Name);
            }
        }
    }
}

