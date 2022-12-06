// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'fs-info' command.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Core;

namespace Aaru.Commands.Filesystem
{
    internal sealed class FilesystemInfoCommand : Command
    {
        public FilesystemInfoCommand() : base("info",
                                              "Opens a disc image and prints info on the found partitions and/or filesystems.")
        {
            Add(new Option(new[]
                {
                    "--encoding", "-e"
                }, "Name of character encoding to use.")
                {
                    Argument = new Argument<string>(() => null),
                    Required = false
                });

            Add(new Option(new[]
                {
                    "--filesystems", "-f"
                }, "Searches and prints information about filesystems.")
                {
                    Argument = new Argument<bool>(() => true),
                    Required = false
                });

            Add(new Option(new[]
                {
                    "--partitions", "-p"
                }, "Searches and interprets partitions.")
                {
                    Argument = new Argument<bool>(() => true),
                    Required = false
                });

            AddArgument(new Argument<string>
            {
                Arity       = ArgumentArity.ExactlyOne,
                Description = "Media image path",
                Name        = "image-path"
            });

            Handler = CommandHandler.Create(typeof(FilesystemInfoCommand).GetMethod(nameof(Invoke)));
        }

        public static int Invoke(bool verbose, bool debug, string encoding, bool filesystems, bool partitions,
                                 string imagePath)
        {
            MainClass.PrintCopyright();

            if(debug)
                AaruConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                AaruConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            Statistics.AddCommand("fs-info");

            AaruConsole.DebugWriteLine("Fs-info command", "--debug={0}", debug);
            AaruConsole.DebugWriteLine("Fs-info command", "--encoding={0}", encoding);
            AaruConsole.DebugWriteLine("Fs-info command", "--filesystems={0}", filesystems);
            AaruConsole.DebugWriteLine("Fs-info command", "--input={0}", imagePath);
            AaruConsole.DebugWriteLine("Fs-info command", "--partitions={0}", partitions);
            AaruConsole.DebugWriteLine("Fs-info command", "--verbose={0}", verbose);

            var     filtersList = new FiltersList();
            IFilter inputFilter = filtersList.GetFilter(imagePath);

            if(inputFilter == null)
            {
                AaruConsole.ErrorWriteLine("Cannot open specified file.");

                return (int)ErrorNumber.CannotOpenFile;
            }

            Encoding encodingClass = null;

            if(encoding != null)
                try
                {
                    encodingClass = Claunia.Encoding.Encoding.GetEncoding(encoding);

                    if(verbose)
                        AaruConsole.VerboseWriteLine("Using encoding for {0}.", encodingClass.EncodingName);
                }
                catch(ArgumentException)
                {
                    AaruConsole.ErrorWriteLine("Specified encoding is not supported.");

                    return (int)ErrorNumber.EncodingUnknown;
                }

            PluginBase plugins = GetPluginBase.Instance;

            bool checkRaw = false;

            try
            {
                IMediaImage imageFormat = ImageFormat.Detect(inputFilter);

                if(imageFormat == null)
                {
                    AaruConsole.WriteLine("Image format not identified, not proceeding with analysis.");

                    return (int)ErrorNumber.UnrecognizedFormat;
                }

                if(verbose)
                    AaruConsole.VerboseWriteLine("Image format identified by {0} ({1}).", imageFormat.Name,
                                                 imageFormat.Id);
                else
                    AaruConsole.WriteLine("Image format identified by {0}.", imageFormat.Name);

                AaruConsole.WriteLine();

                try
                {
                    if(!imageFormat.Open(inputFilter))
                    {
                        AaruConsole.WriteLine("Unable to open image format");
                        AaruConsole.WriteLine("No error given");

                        return (int)ErrorNumber.CannotOpenFormat;
                    }

                    if(verbose)
                    {
                        ImageInfo.PrintImageInfo(imageFormat);
                        AaruConsole.WriteLine();
                    }

                    Statistics.AddMediaFormat(imageFormat.Format);
                    Statistics.AddMedia(imageFormat.Info.MediaType, false);
                    Statistics.AddFilter(inputFilter.Name);
                }
                catch(Exception ex)
                {
                    AaruConsole.ErrorWriteLine("Unable to open image format");
                    AaruConsole.ErrorWriteLine("Error: {0}", ex.Message);
                    AaruConsole.DebugWriteLine("Fs-info command", "Stack trace: {0}", ex.StackTrace);

                    return (int)ErrorNumber.CannotOpenFormat;
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
                        AaruConsole.DebugWriteLine("Fs-info command", "No partitions found");

                        if(!filesystems)
                        {
                            AaruConsole.WriteLine("No partitions founds, not searching for filesystems");

                            return (int)ErrorNumber.NothingFound;
                        }

                        checkRaw = true;
                    }
                    else
                    {
                        AaruConsole.WriteLine("{0} partitions found.", partitionsList.Count);

                        for(int i = 0; i < partitionsList.Count; i++)
                        {
                            AaruConsole.WriteLine();
                            AaruConsole.WriteLine("Partition {0}:", partitionsList[i].Sequence);
                            AaruConsole.WriteLine("Partition name: {0}", partitionsList[i].Name);
                            AaruConsole.WriteLine("Partition type: {0}", partitionsList[i].Type);

                            AaruConsole.WriteLine("Partition start: sector {0}, byte {1}", partitionsList[i].Start,
                                                  partitionsList[i].Offset);

                            AaruConsole.WriteLine("Partition length: {0} sectors, {1} bytes", partitionsList[i].Length,
                                                  partitionsList[i].Size);

                            AaruConsole.WriteLine("Partition scheme: {0}", partitionsList[i].Scheme);
                            AaruConsole.WriteLine("Partition description:");
                            AaruConsole.WriteLine(partitionsList[i].Description);

                            if(!filesystems)
                                continue;

                            AaruConsole.WriteLine("Identifying filesystem on partition");

                            Core.Filesystems.Identify(imageFormat, out idPlugins, partitionsList[i]);

                            if(idPlugins.Count == 0)
                                AaruConsole.WriteLine("Filesystem not identified");
                            else if(idPlugins.Count > 1)
                            {
                                AaruConsole.WriteLine($"Identified by {idPlugins.Count} plugins");

                                foreach(string pluginName in idPlugins)
                                    if(plugins.PluginsList.TryGetValue(pluginName, out plugin))
                                    {
                                        AaruConsole.WriteLine($"As identified by {plugin.Name}.");

                                        plugin.GetInformation(imageFormat, partitionsList[i], out information,
                                                              encodingClass);

                                        AaruConsole.Write(information);
                                        Statistics.AddFilesystem(plugin.XmlFsType.Type);
                                    }
                            }
                            else
                            {
                                plugins.PluginsList.TryGetValue(idPlugins[0], out plugin);

                                if(plugin == null)
                                    continue;

                                AaruConsole.WriteLine($"Identified by {plugin.Name}.");
                                plugin.GetInformation(imageFormat, partitionsList[i], out information, encodingClass);
                                AaruConsole.Write("{0}", information);
                                Statistics.AddFilesystem(plugin.XmlFsType.Type);
                            }
                        }
                    }
                }

                if(checkRaw)
                {
                    var wholePart = new Partition
                    {
                        Name   = "Whole device",
                        Length = imageFormat.Info.Sectors,
                        Size   = imageFormat.Info.Sectors * imageFormat.Info.SectorSize
                    };

                    Core.Filesystems.Identify(imageFormat, out idPlugins, wholePart);

                    if(idPlugins.Count == 0)
                        AaruConsole.WriteLine("Filesystem not identified");
                    else if(idPlugins.Count > 1)
                    {
                        AaruConsole.WriteLine($"Identified by {idPlugins.Count} plugins");

                        foreach(string pluginName in idPlugins)
                            if(plugins.PluginsList.TryGetValue(pluginName, out plugin))
                            {
                                AaruConsole.WriteLine($"As identified by {plugin.Name}.");
                                plugin.GetInformation(imageFormat, wholePart, out information, encodingClass);
                                AaruConsole.Write(information);
                                Statistics.AddFilesystem(plugin.XmlFsType.Type);
                            }
                    }
                    else
                    {
                        plugins.PluginsList.TryGetValue(idPlugins[0], out plugin);

                        if(plugin != null)
                        {
                            AaruConsole.WriteLine($"Identified by {plugin.Name}.");
                            plugin.GetInformation(imageFormat, wholePart, out information, encodingClass);
                            AaruConsole.Write(information);
                            Statistics.AddFilesystem(plugin.XmlFsType.Type);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                AaruConsole.ErrorWriteLine($"Error reading file: {ex.Message}");
                AaruConsole.DebugWriteLine("Fs-info command", ex.StackTrace);

                return (int)ErrorNumber.UnexpectedException;
            }

            return (int)ErrorNumber.NoError;
        }
    }
}