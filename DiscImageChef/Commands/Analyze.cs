/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : Verify.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Verbs.

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Implements the 'analyze' verb.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$
using System;
using System.Collections.Generic;
using DiscImageChef.Plugins;
using DiscImageChef.ImagePlugins;
using DiscImageChef.PartPlugins;
using DiscImageChef.Console;

namespace DiscImageChef.Commands
{
    public static class Analyze
    {
        public static void doAnalyze(AnalyzeSubOptions options)
        {
            DicConsole.DebugWriteLine("Analyze command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Analyze command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Analyze command", "--input={0}", options.InputFile);
            DicConsole.DebugWriteLine("Analyze command", "--filesystems={0}", options.SearchForFilesystems);
            DicConsole.DebugWriteLine("Analyze command", "--partitions={0}", options.SearchForPartitions);

            if (!System.IO.File.Exists(options.InputFile))
            {
                DicConsole.WriteLine("Specified file does not exist.");
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
                    DicConsole.WriteLine("Image format not identified, not proceeding with analysis.");
                    return;
                }
                else
                {
                    if(options.Verbose)
                        DicConsole.VerboseWriteLine("Image format identified by {0} ({1}).", _imageFormat.Name, _imageFormat.PluginUUID);
                    else
                        DicConsole.WriteLine("Image format identified by {0}.", _imageFormat.Name);
                }

                try
                {
                    if (!_imageFormat.OpenImage(options.InputFile))
                    {
                        DicConsole.WriteLine("Unable to open image format");
                        DicConsole.WriteLine("No error given");
                        return;
                    }

                    DicConsole.DebugWriteLine("Analyze command", "Correctly opened image file.");
                    DicConsole.DebugWriteLine("Analyze command", "Image without headers is {0} bytes.", _imageFormat.GetImageSize());
                    DicConsole.DebugWriteLine("Analyze command", "Image has {0} sectors.", _imageFormat.GetSectors());
                    DicConsole.DebugWriteLine("Analyze command", "Image identifies disk type as {0}.", _imageFormat.GetMediaType());
                }
                catch (Exception ex)
                {
                    DicConsole.ErrorWriteLine("Unable to open image format");
                    DicConsole.ErrorWriteLine("Error: {0}", ex.Message);
                    return;
                }

                if (options.SearchForPartitions)
                {
                    List<CommonTypes.Partition> partitions = new List<CommonTypes.Partition>();
                    string partition_scheme = "";

                    // TODO: Solve possibility of multiple partition schemes (CUE + MBR, MBR + RDB, CUE + APM, etc)
                    foreach (PartPlugin _partplugin in plugins.PartPluginsList.Values)
                    {
                        List<CommonTypes.Partition> _partitions;

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
                        DicConsole.DebugWriteLine("Analyze command", "No partitions found");
                        if (!options.SearchForFilesystems)
                        {
                            DicConsole.WriteLine("No partitions founds, not searching for filesystems");
                            return;
                        }
                        checkraw = true;
                    }
                    else
                    {
                        DicConsole.WriteLine("Partition scheme identified as {0}", partition_scheme);
                        DicConsole.WriteLine("{0} partitions found.", partitions.Count);

                        for (int i = 0; i < partitions.Count; i++)
                        {
                            DicConsole.WriteLine();
                            DicConsole.WriteLine("Partition {0}:", partitions[i].PartitionSequence);   
                            DicConsole.WriteLine("Partition name: {0}", partitions[i].PartitionName);  
                            DicConsole.WriteLine("Partition type: {0}", partitions[i].PartitionType);  
                            DicConsole.WriteLine("Partition start: sector {0}, byte {1}", partitions[i].PartitionStartSector, partitions[i].PartitionStart);   
                            DicConsole.WriteLine("Partition length: {0} sectors, {1} bytes", partitions[i].PartitionSectors, partitions[i].PartitionLength);   
                            DicConsole.WriteLine("Partition description:");    
                            DicConsole.WriteLine(partitions[i].PartitionDescription);

                            if (options.SearchForFilesystems)
                            {
                                DicConsole.WriteLine("Identifying filesystem on partition");

                                IdentifyFilesystems(_imageFormat, out id_plugins, partitions[i].PartitionStartSector, partitions[i].PartitionStartSector+partitions[i].PartitionSectors);
                                if (id_plugins.Count == 0)
                                    DicConsole.WriteLine("Filesystem not identified");
                                else if (id_plugins.Count > 1)
                                {
                                    DicConsole.WriteLine(String.Format("Identified by {0} plugins", id_plugins.Count));

                                    foreach (string plugin_name in id_plugins)
                                    {
                                        if (plugins.PluginsList.TryGetValue(plugin_name, out _plugin))
                                        {
                                            DicConsole.WriteLine(String.Format("As identified by {0}.", _plugin.Name));
                                            _plugin.GetInformation(_imageFormat, partitions[i].PartitionStartSector, partitions[i].PartitionStartSector+partitions[i].PartitionSectors, out information);
                                            DicConsole.Write(information);
                                        }
                                    }
                                }
                                else
                                {
                                    plugins.PluginsList.TryGetValue(id_plugins[0], out _plugin);
                                    DicConsole.WriteLine(String.Format("Identified by {0}.", _plugin.Name));
                                    _plugin.GetInformation(_imageFormat, partitions[i].PartitionStartSector, partitions[i].PartitionStartSector+partitions[i].PartitionSectors, out information);
                                    DicConsole.Write(information);
                                }
                            }
                        }
                    }
                }

                if (checkraw)
                {
                    IdentifyFilesystems(_imageFormat, out id_plugins, 0, _imageFormat.GetSectors()-1);
                    if (id_plugins.Count == 0)
                        DicConsole.WriteLine("Filesystem not identified");
                    else if (id_plugins.Count > 1)
                    {
                        DicConsole.WriteLine(String.Format("Identified by {0} plugins", id_plugins.Count));

                        foreach (string plugin_name in id_plugins)
                        {
                            if (plugins.PluginsList.TryGetValue(plugin_name, out _plugin))
                            {
                                DicConsole.WriteLine(String.Format("As identified by {0}.", _plugin.Name));
                                _plugin.GetInformation(_imageFormat, 0, _imageFormat.GetSectors()-1, out information);
                                DicConsole.Write(information);
                            }
                        }
                    }
                    else
                    {
                        plugins.PluginsList.TryGetValue(id_plugins[0], out _plugin);
                        DicConsole.WriteLine(String.Format("Identified by {0}.", _plugin.Name));
                        _plugin.GetInformation(_imageFormat, 0, _imageFormat.GetSectors()-1, out information);
                        DicConsole.Write(information);
                    }
                }
            }
            catch (Exception ex)
            {
                DicConsole.ErrorWriteLine(String.Format("Error reading file: {0}", ex.Message));
                DicConsole.DebugWriteLine("Analyze command", ex.StackTrace);
            }
        }

        static void IdentifyFilesystems(ImagePlugin imagePlugin, out List<string> id_plugins, ulong partitionStart, ulong partitionEnd)
        {
            id_plugins = new List<string>();
            PluginBase plugins = new PluginBase();
            plugins.RegisterAllPlugins();

            foreach (Plugin _plugin in plugins.PluginsList.Values)
            {
                if (_plugin.Identify(imagePlugin, partitionStart, partitionEnd))
                    id_plugins.Add(_plugin.Name.ToLower());
            }
        }
    }
}

