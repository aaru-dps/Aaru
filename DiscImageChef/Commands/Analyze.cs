using System;
using System.Collections.Generic;
using DiscImageChef.Plugins;
using DiscImageChef.ImagePlugins;
using DiscImageChef.PartPlugins;

namespace DiscImageChef.Commands
{
    public static class Analyze
    {
        public static void doAnalyze(AnalyzeSubOptions options)
        {
            if (MainClass.isDebug)
            {
                Console.WriteLine("--debug={0}", options.Debug);
                Console.WriteLine("--verbose={0}", options.Verbose);
                Console.WriteLine("--input={0}", options.InputFile);
                Console.WriteLine("--filesystems={0}", options.SearchForFilesystems);
                Console.WriteLine("--partitions={0}", options.SearchForPartitions);
            }

            if (!System.IO.File.Exists(options.InputFile))
            {
                Console.WriteLine("Specified file does not exist.");
                return;
            }

            PluginBase plugins = new PluginBase();
            plugins.RegisterAllPlugins();

            List<string> id_plugins;
            Plugin _plugin;
            string information;
            bool checkraw = false;
            ImagePlugin _imageFormat;

            try
            {
                _imageFormat = ImageFormat.Detect(options.InputFile);

                if(_imageFormat == null)
                {
                    Console.WriteLine("Image format not identified, not proceeding with analysis.");
                }
                else
                {
                    if(MainClass.isVerbose)
                        Console.WriteLine("Image format identified by {0} ({1}).", _imageFormat.Name, _imageFormat.PluginUUID);
                    else
                        Console.WriteLine("Image format identified by {0}.", _imageFormat.Name);
                }

                try
                {
                    if (!_imageFormat.OpenImage(options.InputFile))
                    {
                        Console.WriteLine("Unable to open image format");
                        Console.WriteLine("No error given");
                        return;
                    }

                    if (MainClass.isDebug)
                    {
                        Console.WriteLine("DEBUG: Correctly opened image file.");
                        Console.WriteLine("DEBUG: Image without headers is {0} bytes.", _imageFormat.GetImageSize());
                        Console.WriteLine("DEBUG: Image has {0} sectors.", _imageFormat.GetSectors());
                        Console.WriteLine("DEBUG: Image identifies disk type as {0}.", _imageFormat.GetDiskType());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to open image format");
                    Console.WriteLine("Error: {0}", ex.Message);
                    return;
                }

                if (options.SearchForPartitions)
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
                        if(MainClass.isDebug)
                            Console.WriteLine("DEBUG: No partitions found");
                        if (!options.SearchForFilesystems)
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
                            Console.WriteLine("Partition start: sector {0}, byte {1}", partitions[i].PartitionStartSector, partitions[i].PartitionStart);   
                            Console.WriteLine("Partition length: {0} sectors, {1} bytes", partitions[i].PartitionSectors, partitions[i].PartitionLength);   
                            Console.WriteLine("Partition description:");    
                            Console.WriteLine(partitions[i].PartitionDescription);

                            if (options.SearchForFilesystems)
                            {
                                Console.WriteLine("Identifying filesystem on partition");

                                IdentifyFilesystems(_imageFormat, out id_plugins, partitions[i].PartitionStartSector);
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
                                            _plugin.GetInformation(_imageFormat, partitions[i].PartitionStartSector, out information);
                                            Console.Write(information);
                                        }
                                    }
                                }
                                else
                                {
                                    plugins.PluginsList.TryGetValue(id_plugins[0], out _plugin);
                                    Console.WriteLine(String.Format("Identified by {0}.", _plugin.Name));
                                    _plugin.GetInformation(_imageFormat, partitions[i].PartitionStartSector, out information);
                                    Console.Write(information);
                                }
                            }
                        }
                    }
                }

                if (checkraw)
                {
                    IdentifyFilesystems(_imageFormat, out id_plugins, 0);
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
                if (MainClass.isDebug)
                    Console.WriteLine(ex.StackTrace);
            }
        }

        static void IdentifyFilesystems(ImagePlugin imagePlugin, out List<string> id_plugins, ulong partitionOffset)
        {
            id_plugins = new List<string>();
            PluginBase plugins = new PluginBase();
            plugins.RegisterAllPlugins();

            foreach (Plugin _plugin in plugins.PluginsList.Values)
            {
                if (_plugin.Identify(imagePlugin, partitionOffset))
                    id_plugins.Add(_plugin.Name.ToLower());
            }
        }
    }
}

