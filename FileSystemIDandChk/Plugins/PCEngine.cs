using System;
using System.IO;
using System.Text;
using FileSystemIDandChk;

// Information from Inside Macintosh

namespace FileSystemIDandChk.Plugins
{
	class PCEnginePlugin : Plugin
	{
		public PCEnginePlugin(PluginBase Core)
        {
            base.Name = "PC Engine CD Plugin";
            base.PluginUUID = new Guid("e5ee6d7c-90fa-49bd-ac89-14ef750b8af3");
        }
		
		public override bool Identify(FileStream stream, long offset)
		{
            byte[] system_descriptor = new byte[23];

            stream.Seek(2080 + offset, SeekOrigin.Begin);

            stream.Read(system_descriptor, 0, 23);

            if(Encoding.ASCII.GetString(system_descriptor) == "PC Engine CD-ROM SYSTEM")
                return true;
            else
                return false;
		}
		
		public override void GetInformation (FileStream stream, long offset, out string information)
		{
			information = "";
		}
	}
}

