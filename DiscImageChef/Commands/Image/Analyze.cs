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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;
using DiscImageChef.Core;

namespace DiscImageChef.Commands.Image
{
    internal class AnalyzeCommand : Command
    {
        public AnalyzeCommand() : base("analyze",
                                       "Analyzes a disc image and searches for partitions and/or filesystems.")
        {
            Add(new Option(new[]
                {
                    "--encoding", "-e"
                }, "Name of character encoding to use.")
                {
                    Argument = new Argument<string>(() => null), Required = false
                });

            Add(new Option(new[]
                {
                    "--filesystems", "-f"
                }, "Searches and analyzes filesystems.")
                {
                    Argument = new Argument<bool>(() => true), Required = false
                });

            Add(new Option(new[]
                {
                    "--partitions", "-p"
                }, "Searches and interprets partitions.")
                {
                    Argument = new Argument<bool>(() => true), Required = false
                });

            AddArgument(new Argument<string>
            {
                Arity = ArgumentArity.ExactlyOne, Description = "Disc image path", Name = "image-path"
            });

            Handler = CommandHandler.Create(typeof(AnalyzeCommand).GetMethod(nameof(Invoke)));
        }

        public static int Invoke(bool verbose, bool debug, string encoding, bool filesystems, bool partitions,
                                 string imagePath)
        {
            MainClass.PrintCopyright();

            if(debug)
                DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            Statistics.AddCommand("analyze");

            DicConsole.DebugWriteLine("Analyze command", "--debug={0}", debug);
            DicConsole.DebugWriteLine("Analyze command", "--encoding={0}", encoding);
            DicConsole.DebugWriteLine("Analyze command", "--filesystems={0}", filesystems);
            DicConsole.DebugWriteLine("Analyze command", "--input={0}", imagePath);
            DicConsole.DebugWriteLine("Analyze command", "--partitions={0}", partitions);
            DicConsole.DebugWriteLine("Analyze command", "--verbose={0}", verbose);

            var     filtersList = new FiltersList();
            IFilter inputFilter = filtersList.GetFilter(imagePath);

            if(inputFilter == null)
            {
                DicConsole.ErrorWriteLine("Cannot open specified file.");

                return(int)ErrorNumber.CannotOpenFile;
            }

            Encoding encodingClass = null;

            if(encoding != null)
                try
                {
                    encodingClass = Claunia.Encoding.Encoding.GetEncoding(encoding);

                    if(verbose)
                        DicConsole.VerboseWriteLine("Using encoding for {0}.", encodingClass.EncodingName);
                }
                catch(ArgumentException)
                {
                    DicConsole.ErrorWriteLine("Specified encoding is not supported.");

                    return(int)ErrorNumber.EncodingUnknown;
                }

            PluginBase plugins = GetPluginBase.Instance;

            bool checkRaw = false;

            try
            {
                IMediaImage imageFormat = ImageFormat.Detect(inputFilter);

                if(imageFormat == null)
                {
                    DicConsole.WriteLine("Image format not identified, not proceeding with analysis.");

                    return(int)ErrorNumber.UnrecognizedFormat;
                }

                if(verbose)
                    DicConsole.VerboseWriteLine("Image format identified by {0} ({1}).", imageFormat.Name,
                                                imageFormat.Id);
                else
                    DicConsole.WriteLine("Image format identified by {0}.", imageFormat.Name);

                DicConsole.WriteLine();

                try
                {
                    if(!imageFormat.Open(inputFilter))
                    {
                        DicConsole.WriteLine("Unable to open image format");
                        DicConsole.WriteLine("No error given");

                        return(int)ErrorNumber.CannotOpenFormat;
                    }

                    if(verbose)
                    {
                        ImageInfo.PrintImageInfo(imageFormat);
                        DicConsole.WriteLine();
                    }

                    Statistics.AddMediaFormat(imageFormat.Format);
                    Statistics.AddMedia(imageFormat.Info.MediaType, false);
                    Statistics.AddFilter(inputFilter.Name);
                }
                catch(Exception ex)
                {
                    DicConsole.ErrorWriteLine("Unable to open image format");
                    DicConsole.ErrorWriteLine("Error: {0}", ex.Message);
                    DicConsole.DebugWriteLine("Analyze command", "Stack trace: {0}", ex.StackTrace);

                    return(int)ErrorNumber.CannotOpenFormat;
                }

                List<string> idPlugins;
                IFilesystem  plugin;
                string       information;

                if(partitions)
                {
                    List<Partition> partitionsList = Core.Partitions.GetAll(imageFormat);
                    Core.Partitions.AddSchemesToStats(partitionsList);

                    if(partitionsList.Count == 0)
                    {
                        DicConsole.DebugWriteLine("Analyze command", "No partitions found");

                        if(!filesystems)
                        {
                            DicConsole.WriteLine("No partitions founds, not searching for filesystems");

                            return(int)ErrorNumber.NothingFound;
                        }

                        checkRaw = true;
                    }
                    else
                    {
                        DicConsole.WriteLine("{0} partitions found.", partitionsList.Count);

                        for(int i = 0; i < partitionsList.Count; i++)
                        {
                            DicConsole.WriteLine();
                            DicConsole.WriteLine("Partition {0}:", partitionsList[i].Sequence);
                            DicConsole.WriteLine("Partition name: {0}", partitionsList[i].Name);
                            DicConsole.WriteLine("Partition type: {0}", partitionsList[i].Type);

                            DicConsole.WriteLine("Partition start: sector {0}, byte {1}", partitionsList[i].Start,
                                                 partitionsList[i].Offset);

                            DicConsole.WriteLine("Partition length: {0} sectors, {1} bytes", partitionsList[i].Length,
                                                 partitionsList[i].Size);

                            DicConsole.WriteLine("Partition scheme: {0}", partitionsList[i].Scheme);
                            DicConsole.WriteLine("Partition description:");
                            DicConsole.WriteLine(partitionsList[i].Description);

                            if(!filesystems)
                                continue;

                            DicConsole.WriteLine("Identifying filesystem on partition");

                            Core.Filesystems.Identify(imageFormat, out idPlugins, partitionsList[i]);

                            if(idPlugins.Count == 0)
                                DicConsole.WriteLine("Filesystem not identified");
                            else if(idPlugins.Count > 1)
                            {
                                DicConsole.WriteLine($"Identified by {idPlugins.Count} plugins");

                                foreach(string pluginName in idPlugins)
                                    if(plugins.PluginsList.TryGetValue(pluginName, out plugin))
                                    {
                                        DicConsole.WriteLine($"As identified by {plugin.Name}.");

                                        plugin.GetInformation(imageFormat, partitionsList[i], out information,
                                                              encodingClass);

                                        DicConsole.Write(information);
                                        Statistics.AddFilesystem(plugin.XmlFsType.Type);
                                    }
                            }
                            else
                            {
                                plugins.PluginsList.TryGetValue(idPlugins[0], out plugin);

                                if(plugin == null)
                                    continue;

                                DicConsole.WriteLine($"Identified by {plugin.Name}.");
                                plugin.GetInformation(imageFormat, partitionsList[i], out information, encodingClass);
                                DicConsole.Write("{0}", information);
                                Statistics.AddFilesystem(plugin.XmlFsType.Type);
                            }
                        }
                    }
                }

                if(checkRaw)
                {
                    var wholePart = new Partition
                    {
                        Name = "Whole device", Length = imageFormat.Info.Sectors,
                        Size = imageFormat.Info.Sectors * imageFormat.Info.SectorSize
                    };

                    Core.Filesystems.Identify(imageFormat, out idPlugins, wholePart);

                    if(idPlugins.Count == 0)
                        DicConsole.WriteLine("Filesystem not identified");
                    else if(idPlugins.Count > 1)
                    {
                        DicConsole.WriteLine($"Identified by {idPlugins.Count} plugins");

                        foreach(string pluginName in idPlugins)
                            if(plugins.PluginsList.TryGetValue(pluginName, out plugin))
                            {
                                DicConsole.WriteLine($"As identified by {plugin.Name}.");
                                plugin.GetInformation(imageFormat, wholePart, out information, encodingClass);
                                DicConsole.Write(information);
                                Statistics.AddFilesystem(plugin.XmlFsType.Type);
                            }
                    }
                    else
                    {
                        plugins.PluginsList.TryGetValue(idPlugins[0], out plugin);

                        if(plugin != null)
                        {
                            DicConsole.WriteLine($"Identified by {plugin.Name}.");
                            plugin.GetInformation(imageFormat, wholePart, out information, encodingClass);
                            DicConsole.Write(information);
                            Statistics.AddFilesystem(plugin.XmlFsType.Type);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                DicConsole.ErrorWriteLine($"Error reading file: {ex.Message}");
                DicConsole.DebugWriteLine("Analyze command", ex.StackTrace);

                return(int)ErrorNumber.UnexpectedException;
            }

            return(int)ErrorNumber.NoError;
        }
    }
}