using System;
using System.Reflection;
using System.Collections.Generic;
using FileSystemIDandChk.Plugins;
using FileSystemIDandChk.PartPlugins;

namespace FileSystemIDandChk
{
	public class PluginBase
	{
		public Dictionary<string, Plugin> PluginsList;
		public Dictionary<string, PartPlugin> PartPluginsList;
		
		public PluginBase ()
		{
			this.PluginsList = new Dictionary<string, Plugin>();
			this.PartPluginsList = new Dictionary<string, PartPlugin>();
		}
		
		public void RegisterAllPlugins()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            foreach (Type type in assembly.GetTypes())
            {
                try
                {
                    if (type.IsSubclassOf(typeof(Plugin)))
                    {
                        Plugin plugin = (Plugin)type.GetConstructor(new Type[] { typeof(PluginBase) }).Invoke(new object[] { this });
                        this.RegisterPlugin(plugin);
                    }
					else if (type.IsSubclassOf(typeof(PartPlugin)))
                    {
                        PartPlugin partplugin = (PartPlugin)type.GetConstructor(new Type[] { typeof(PluginBase) }).Invoke(new object[] { this });
                        this.RegisterPartPlugin(partplugin);
                    }
					
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.ToString());
                }
            }
        }

        private void RegisterPlugin(Plugin plugin)
        {
            if (!this.PluginsList.ContainsKey(plugin.Name.ToLower()))
            {
                this.PluginsList.Add(plugin.Name.ToLower(), plugin);
            }
        }

	    private void RegisterPartPlugin(PartPlugin partplugin)
        {
            if (!this.PartPluginsList.ContainsKey(partplugin.Name.ToLower()))
            {
                this.PartPluginsList.Add(partplugin.Name.ToLower(), partplugin);
            }
        }
	}
}

