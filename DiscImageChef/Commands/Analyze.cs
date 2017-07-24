// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Analyze.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'analyze' verb.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using DiscImageChef.ImagePlugins;
using DiscImageChef.PartPlugins;

namespace DiscImageChef.Commands
{
    public static class Analyze
    {
        public static void doAnalyze(AnalyzeOptions options)
        {
            DicConsole.DebugWriteLine("Analyze command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Analyze command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Analyze command", "--input={0}", options.InputFile);
            DicConsole.DebugWriteLine("Analyze command", "--filesystems={0}", options.SearchForFilesystems);
            DicConsole.DebugWriteLine("Analyze command", "--partitions={0}", options.SearchForPartitions);

            FiltersList filtersList = new FiltersList();
            Filter inputFilter = filtersList.GetFilter(options.InputFile);

            if(inputFilter == null)
            {
                DicConsole.ErrorWriteLine("Cannot open specified file.");
                return;
            }

            PluginBase plugins = new PluginBase();
            plugins.RegisterAllPlugins();

            List<string> id_plugins;
            Filesystem _plugin;
            string information;
            bool checkraw = false;
            ImagePlugin _imageFormat;

            try
            {
                _imageFormat = ImageFormat.Detect(inputFilter);

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
                    if(!_imageFormat.OpenImage(inputFilter))
                    {
                        DicConsole.WriteLine("Unable to open image format");
                        DicConsole.WriteLine("No error given");
                        return;
                    }

                    DicConsole.DebugWriteLine("Analyze command", "Correctly opened image file.");
                    DicConsole.DebugWriteLine("Analyze command", "Image without headers is {0} bytes.", _imageFormat.GetImageSize());
                    DicConsole.DebugWriteLine("Analyze command", "Image has {0} sectors.", _imageFormat.GetSectors());
                    DicConsole.DebugWriteLine("Analyze command", "Image identifies disk type as {0}.", _imageFormat.GetMediaType());

                    Core.Statistics.AddMediaFormat(_imageFormat.GetImageFormat());
                    Core.Statistics.AddMedia(_imageFormat.ImageInfo.mediaType, false);
                    Core.Statistics.AddFilter(inputFilter.Name);
                }
                catch(Exception ex)
                {
                    DicConsole.ErrorWriteLine("Unable to open image format");
                    DicConsole.ErrorWriteLine("Error: {0}", ex.Message);
                    return;
                }

                if(options.SearchForPartitions)
                {
                    List<Partition> partitions = Partitions.GetAll(_imageFormat);
                    Partitions.AddSchemesToStats(partitions);

                    if(partitions.Count == 0)
                    {
                        DicConsole.DebugWriteLine("Analyze command", "No partitions found");
                        if(!options.SearchForFilesystems)
                        {
                            DicConsole.WriteLine("No partitions founds, not searching for filesystems");
                            return;
                        }
                        checkraw = true;
                    }
                    else
                    {
                        DicConsole.WriteLine("{0} partitions found.", partitions.Count);

                        for(int i = 0; i < partitions.Count; i++)
                        {
                            DicConsole.WriteLine();
                            DicConsole.WriteLine("Partition {0}:", partitions[i].Sequence);
                            DicConsole.WriteLine("Partition name: {0}", partitions[i].Name);
                            DicConsole.WriteLine("Partition type: {0}", partitions[i].Type);
                            DicConsole.WriteLine("Partition start: sector {0}, byte {1}", partitions[i].Start, partitions[i].Offset);
                            DicConsole.WriteLine("Partition length: {0} sectors, {1} bytes", partitions[i].Length, partitions[i].Size);
                            DicConsole.WriteLine("Partition scheme: {0}", partitions[i].Scheme);
                            DicConsole.WriteLine("Partition description:");
                            DicConsole.WriteLine(partitions[i].Description);

                            if(options.SearchForFilesystems)
                            {
                                DicConsole.WriteLine("Identifying filesystem on partition");

                                Core.Filesystems.Identify(_imageFormat, out id_plugins, partitions[i]);
                                if(id_plugins.Count == 0)
                                    DicConsole.WriteLine("Filesystem not identified");
                                else if(id_plugins.Count > 1)
                                {
                                    DicConsole.WriteLine(string.Format("Identified by {0} plugins", id_plugins.Count));

                                    foreach(string plugin_name in id_plugins)
                                    {
                                        if(plugins.PluginsList.TryGetValue(plugin_name, out _plugin))
                                        {
                                            DicConsole.WriteLine(string.Format("As identified by {0}.", _plugin.Name));
                                            _plugin.GetInformation(_imageFormat, partitions[i], out information);
                                            DicConsole.Write(information);
                                            Core.Statistics.AddFilesystem(_plugin.XmlFSType.Type);
                                        }
                                    }
                                }
                                else
                                {
                                    plugins.PluginsList.TryGetValue(id_plugins[0], out _plugin);
                                    DicConsole.WriteLine(string.Format("Identified by {0}.", _plugin.Name));
                                    _plugin.GetInformation(_imageFormat, partitions[i], out information);
                                    DicConsole.Write(information);
                                    Core.Statistics.AddFilesystem(_plugin.XmlFSType.Type);
                                }
                            }
                        }
                    }
                }

                if(checkraw)
                {
                    Partition wholePart = new Partition
                    {
                        Name = "Whole device",
                        Length = _imageFormat.GetSectors(),
                        Size = _imageFormat.GetSectors() * _imageFormat.GetSectorSize()
                    };

                    Core.Filesystems.Identify(_imageFormat, out id_plugins, wholePart);
                    if(id_plugins.Count == 0)
                        DicConsole.WriteLine("Filesystem not identified");
                    else if(id_plugins.Count > 1)
                    {
                        DicConsole.WriteLine(string.Format("Identified by {0} plugins", id_plugins.Count));

                        foreach(string plugin_name in id_plugins)
                        {
                            if(plugins.PluginsList.TryGetValue(plugin_name, out _plugin))
                            {
                                DicConsole.WriteLine(string.Format("As identified by {0}.", _plugin.Name));
                                _plugin.GetInformation(_imageFormat, wholePart, out information);
                                DicConsole.Write(information);
                                Core.Statistics.AddFilesystem(_plugin.XmlFSType.Type);
                            }
                        }
                    }
                    else
                    {
                        plugins.PluginsList.TryGetValue(id_plugins[0], out _plugin);
                        DicConsole.WriteLine(string.Format("Identified by {0}.", _plugin.Name));
                        _plugin.GetInformation(_imageFormat, wholePart, out information);
                        DicConsole.Write(information);
                        Core.Statistics.AddFilesystem(_plugin.XmlFSType.Type);
                    }
                }
            }
            catch(Exception ex)
            {
                DicConsole.ErrorWriteLine(string.Format("Error reading file: {0}", ex.Message));
                DicConsole.DebugWriteLine("Analyze command", ex.StackTrace);
            }

            Core.Statistics.AddCommand("analyze");
        }
    }
}

