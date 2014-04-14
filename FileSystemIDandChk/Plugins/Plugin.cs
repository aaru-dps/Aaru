using System;
using System.IO;

namespace FileSystemIDandChk.Plugins
{
	public abstract class Plugin
	{
        public string Name;
        public Guid PluginUUID;

        protected Plugin()
        {
        }
		
        public abstract bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset);
        public abstract void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset, out string information);
	}
}

