/***************************************************************************
FileSystem identifier and checker
----------------------------------------------------------------------------
 
Filename       : Main.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Main program loop.

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Contains the main program loop.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$

using System;
using System.Collections.Generic;
using FileSystemIDandChk.ImagePlugins;
using FileSystemIDandChk.PartPlugins;
using FileSystemIDandChk.Plugins;

namespace FileSystemIDandChk
{
    class MainClass
    {
        static PluginBase plugins;
        public static bool chkPartitions;
        public static bool chkFilesystems;
        public static bool isDebug;

        public static void Main(string[] args)
        {
            plugins = new PluginBase();
			
            chkPartitions = true;
            chkFilesystems = true;
            // RELEASE
            //isDebug = false;
            // DEBUG
            isDebug = true;
			
            Console.WriteLine("Filesystem Identifier and Checker");
            Console.WriteLine("Copyright (C) Natalia Portillo, All Rights Reserved");
			
            // For debug
            if (isDebug)
            {
                plugins.RegisterAllPlugins();
                Runner("/Users/claunia/Desktop/disk_images/dc42.dc42");
            }
            else
            {
                if (args.Length == 0)
                {
                    Usage();
                }
                else if (args.Length == 1)
                {
                    plugins.RegisterAllPlugins();
				
                    if (args[0] == "--formats")
                    {
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
                    else
                        Runner(args[0]);
                }
                else
                {
                    for (int i = 0; i < args.Length - 1; i++)
                    {
                        switch (args[i])
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
                        }
                    }
				
                    Runner(args[args.Length - 1]);
                }    
            }
        }

        static void Runner(string filename)
        {
            List<string> id_plugins;
            Plugin _plugin;
            string information;
            bool checkraw = false;
            ImagePlugin _imageFormat;
			
            try
            {
                _imageFormat = null;

                foreach (ImagePlugin _imageplugin in plugins.ImagePluginsList.Values)
                {
                    if (_imageplugin.IdentifyImage(filename))
                    {
                        _imageFormat = _imageplugin;
                        Console.WriteLine("Image format identified by {0}.", _imageplugin.Name);
                        break;
                    }
                }

                if (_imageFormat == null)
                {
                    Console.WriteLine("Image format not identified, not proceeding.");
                    return;
                }

                try
                {
                    if (!_imageFormat.OpenImage(filename))
                    {
                        Console.WriteLine("Unable to open image format");
                        Console.WriteLine("No error given");
                        return;
                    }

                    if (isDebug)
                    {
                        Console.WriteLine("DEBUG: Correctly opened image file.");
                        Console.WriteLine("DEBUG: Image without headers is {0} bytes.", _imageFormat.GetImageSize());
                        Console.WriteLine("DEBUG: Image has {0} sectors.", _imageFormat.GetSectors());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to open image format");
                    Console.WriteLine("Error: {0}", ex.Message);
                    return;
                }

                Console.WriteLine("Image identified as {0}.", _imageFormat.GetImageFormat());

                if (chkPartitions)
                {
                    List<Partition> partitions = new List<Partition>();
                    string partition_scheme = "";
					
                    // TODO: Solve possibility of multiple partition schemes (CUE + MBR, MBR + RDB, CUE + APM, etc)
                    foreach (PartPlugin _partplugin in plugins.PartPluginsList.Values)
                    {
                        List<Partition> _partitions;

                        if (_partplugin.GetInformation(_imageFormat, out _partitions))
                        {
                            partition_scheme = _partplugin.Name;
                            partitions = _partitions;
                            break;
                        }
                    }

                    if (_imageFormat.ImageHasPartitions())
                    {
                        partition_scheme = _imageFormat.GetImageFormat();
                        partitions = _imageFormat.GetPartitions();
                    }
					
                    if (partition_scheme == "")
                    {
                        Console.WriteLine("DEBUG: No partitions found");
                        if (!chkFilesystems)
                        {
                            Console.WriteLine("No partitions founds, not searching for filesystems");
                            return;
                        }
                        checkraw = true;
                    }
                    else
                    {
                        Console.WriteLine("Partition scheme identified as {0}", partition_scheme);
                        Console.WriteLine("{0} partitions found.", partitions.Count);
						
                        for (int i = 0; i < partitions.Count; i++)
                        {
                            Console.WriteLine();
                            Console.WriteLine("Partition {0}:", partitions[i].PartitionSequence);	
                            Console.WriteLine("Partition name: {0}", partitions[i].PartitionName);	
                            Console.WriteLine("Partition type: {0}", partitions[i].PartitionType);	
                            Console.WriteLine("Partition start: {0}", partitions[i].PartitionStart);	
                            Console.WriteLine("Partition length: {0}", partitions[i].PartitionLength);	
                            Console.WriteLine("Partition description:");	
                            Console.WriteLine(partitions[i].PartitionDescription);
							
                            if (chkFilesystems)
                            {
                                Console.WriteLine("Identifying filesystem on partition");
								
                                Identify(_imageFormat, out id_plugins, partitions[i].PartitionStart);
                                if (id_plugins.Count == 0)
                                    Console.WriteLine("Filesystem not identified");
                                else if (id_plugins.Count > 1)
                                {
                                    Console.WriteLine(String.Format("Identified by {0} plugins", id_plugins.Count));
									
                                    foreach (string plugin_name in id_plugins)
                                    {
                                        if (plugins.PluginsList.TryGetValue(plugin_name, out _plugin))
                                        {
                                            Console.WriteLine(String.Format("As identified by {0}.", _plugin.Name));
                                            _plugin.GetInformation(_imageFormat, partitions[i].PartitionStart, out information);
                                            Console.Write(information);
                                        }
                                    }
                                }
                                else
                                {
                                    plugins.PluginsList.TryGetValue(id_plugins[0], out _plugin);
                                    Console.WriteLine(String.Format("Identified by {0}.", _plugin.Name));
                                    _plugin.GetInformation(_imageFormat, partitions[i].PartitionStart, out information);
                                    Console.Write(information);
                                }
                            }
                        }
                    }
                }
				
                if (checkraw)
                {
                    Identify(_imageFormat, out id_plugins, 0);
                    if (id_plugins.Count == 0)
                        Console.WriteLine("Filesystem not identified");
                    else if (id_plugins.Count > 1)
                    {
                        Console.WriteLine(String.Format("Identified by {0} plugins", id_plugins.Count));
						
                        foreach (string plugin_name in id_plugins)
                        {
                            if (plugins.PluginsList.TryGetValue(plugin_name, out _plugin))
                            {
                                Console.WriteLine(String.Format("As identified by {0}.", _plugin.Name));
                                _plugin.GetInformation(_imageFormat, 0, out information);
                                Console.Write(information);
                            }
                        }
                    }
                    else
                    {
                        plugins.PluginsList.TryGetValue(id_plugins[0], out _plugin);
                        Console.WriteLine(String.Format("Identified by {0}.", _plugin.Name));
                        _plugin.GetInformation(_imageFormat, 0, out information);
                        Console.Write(information);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Error reading file: {0}", ex.Message));
                if (isDebug)
                    Console.WriteLine(ex.StackTrace);
            }
        }

        static void Identify(ImagePlugin imagePlugin, out List<string> id_plugins, ulong partitionOffset)
        {
            id_plugins = new List<string>();
			
            foreach (Plugin _plugin in plugins.PluginsList.Values)
            {
                if (_plugin.Identify(imagePlugin, partitionOffset))
                    id_plugins.Add(_plugin.Name.ToLower());
            }
        }

        static void Usage()
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

