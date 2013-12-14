using System;
using System.IO;
using System.Collections.Generic;
using FileSystemIDandChk.Plugins;
using FileSystemIDandChk.PartPlugins;

namespace FileSystemIDandChk
{
	class MainClass
	{
		static PluginBase plugins;
		public static bool chkPartitions;
		public static bool chkFilesystems;
		public static bool isDebug;
		
		public static void Main (string[] args)
		{
			plugins = new PluginBase();
			
			chkPartitions = true;
			chkFilesystems = true;
			isDebug = false;
			
			Console.WriteLine ("Filesystem Identifier and Checker");
			Console.WriteLine ("Copyright (C) Natalia Portillo, All Rights Reserved");
			
			if(args.Length==0)
			{
				Usage();
			}
			else if(args.Length==1)
			{
				plugins.RegisterAllPlugins();
				
				if(args[0]=="--formats")
				{
					Console.WriteLine("Supported filesystems:");
					foreach(KeyValuePair<string, Plugin> kvp in plugins.PluginsList)
						Console.WriteLine(kvp.Value.Name);
					Console.WriteLine();
					Console.WriteLine("Supported partitions:");
					foreach(KeyValuePair<string, PartPlugin> kvp in plugins.PartPluginsList)
						Console.WriteLine(kvp.Value.Name);
				}
				else
					Runner(args[0]);
			}
			else
			{
				for(int i = 0; i<args.Length-1; i++)
				{
					switch(args[i])	
					{
						case "--filesystems":
							chkFilesystems = true;
							chkPartitions = false;
							break;
						case "--partitions":
							chkFilesystems = false;
							chkPartitions = true;
							break;
						case "--all":
							chkFilesystems = true;
							chkPartitions = true;
							break;
						case "--debug":
							isDebug = true;
							break;
						default:
							break;
					}
				}
				
				Runner(args[args.Length-1]);
			}
		}

		private static void Runner (string filename)
		{
			FileStream stream;
			List<string> id_plugins;
			Plugin _plugin;
			string information;
			bool checkraw = false;
			
			try
			{
				stream = File.OpenRead(filename);
				
				if(chkPartitions)
				{
					List<Partition> partitions = new List<Partition>();
					string partition_scheme = "";
					
					foreach (PartPlugin _partplugin in plugins.PartPluginsList.Values)
          			{
						List<Partition> _partitions;

            			if (_partplugin.GetInformation(stream, out _partitions))
						{
							partition_scheme=_partplugin.Name;
							partitions = _partitions;
							break;
						}
           			}
					
					if(partition_scheme=="")
					{
						if(!chkFilesystems)
						{
							Console.WriteLine("No partitions founds, not searching for filesystems");
							return;
						}
						else
							checkraw = true;
					}
					else
					{
						Console.WriteLine("Partition scheme identified as {0}", partition_scheme);
						Console.WriteLine("{0} partitions found.", partitions.Count);
						
						for(int i = 0; i< partitions.Count; i++)
						{
							Console.WriteLine();
							Console.WriteLine("Partition {0}:", partitions[i].PartitionSequence);	
							Console.WriteLine("Partition name: {0}", partitions[i].PartitionName);	
							Console.WriteLine("Partition type: {0}", partitions[i].PartitionType);	
							Console.WriteLine("Partition start: {0}", partitions[i].PartitionStart);	
							Console.WriteLine("Partition length: {0}", partitions[i].PartitionLength);	
							Console.WriteLine("Partition description:");	
							Console.WriteLine(partitions[i].PartitionDescription);
							
							if(chkFilesystems)
							{
								Console.WriteLine("Identifying filesystem on partition");
								
								Identify(stream, out id_plugins, partitions[i].PartitionStart);
								if(id_plugins.Count==0)
									Console.WriteLine("Filesystem not identified");
								else if(id_plugins.Count>1)
								{
									Console.WriteLine(String.Format("Identified by {0} plugins", id_plugins.Count));
									
									foreach(string plugin_name in id_plugins)
									{
										if(plugins.PluginsList.TryGetValue(plugin_name, out _plugin))
										{
											Console.WriteLine(String.Format("As identified by {0}.", _plugin.Name));
											_plugin.GetInformation(stream, partitions[i].PartitionStart, out information);
											Console.Write(information);
										}
									}
								}
								else
								{
									plugins.PluginsList.TryGetValue(id_plugins[0], out _plugin);
									Console.WriteLine(String.Format("Identified by {0}.", _plugin.Name));
									_plugin.GetInformation(stream, partitions[i].PartitionStart, out information);
									Console.Write(information);
								}
							}
						}
					}
				}
				
				if(checkraw)
				{
					Identify(stream, out id_plugins, 0);
					if(id_plugins.Count==0)
						Console.WriteLine("Filesystem not identified");
					else if(id_plugins.Count>1)
					{
						Console.WriteLine(String.Format("Identified by {0} plugins", id_plugins.Count));
						
						foreach(string plugin_name in id_plugins)
						{
							if(plugins.PluginsList.TryGetValue(plugin_name, out _plugin))
							{
								Console.WriteLine(String.Format("As identified by {0}.", _plugin.Name));
								_plugin.GetInformation(stream, 0, out information);
								Console.Write(information);
							}
						}
					}
					else
					{
						plugins.PluginsList.TryGetValue(id_plugins[0], out _plugin);
						Console.WriteLine(String.Format("Identified by {0}.", _plugin.Name));
						_plugin.GetInformation(stream, 0, out information);
						Console.Write(information);
					}
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine(String.Format("Error reading file: {0}", ex.Message));
				if(isDebug)
					Console.WriteLine(ex.StackTrace);
			}
			finally
			{
                stream = null;
			}
		}

		private static void Identify (FileStream stream, out List<string> id_plugins, long offset)
		{
			id_plugins = new List<string>();
			
			foreach (Plugin _plugin in plugins.PluginsList.Values)
            {
            	if (_plugin.Identify(stream, offset))
                	id_plugins.Add(_plugin.Name.ToLower());
            }
		}
		
		private static void Usage ()
		{
			Console.WriteLine("Usage: filesystemidandchk [options] file");
			Console.WriteLine();
			Console.WriteLine(" --formats     List all suported partition and filesystems");
			Console.WriteLine(" --debug       Show debug information");
			Console.WriteLine(" --partitions  Check only for partitions");
			Console.WriteLine(" --filesystems Check only for filesystems");
			Console.WriteLine(" --all         Check for partitions and filesystems (default)");
			Console.WriteLine();
		}
	}
}

