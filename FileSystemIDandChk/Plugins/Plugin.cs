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
		
		public abstract bool Identify(FileStream stream, long offset);
        public abstract void GetInformation(FileStream stream, long offset, out string information);
	}
}

