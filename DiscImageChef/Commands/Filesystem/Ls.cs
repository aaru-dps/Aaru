// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Ls.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'ls' verb.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using DiscImageChef.Core;

namespace DiscImageChef.Commands.Filesystem
{
    internal class LsCommand : Command
    {
        public LsCommand() : base("list", "Lists files in disc image.")
        {
            AddAlias("ls");

            Add(new Option(new[]
                {
                    "--encoding", "-e"
                }, "Name of character encoding to use.")
                {
                    Argument = new Argument<string>(() => null), Required = false
                });

            Add(new Option(new[]
                {
                    "--long-format", "-l"
                }, "Uses long format.")
                {
                    Argument = new Argument<bool>(() => true), Required = false
                });

            Add(new Option(new[]
                {
                    "--options", "-O"
                }, "Comma separated name=value pairs of options to pass to filesystem plugin.")
                {
                    Argument = new Argument<string>(() => null), Required = false
                });

            Add(new Option(new[]
                {
                    "--namespace", "-n"
                }, "Namespace to use for filenames.")
                {
                    Argument = new Argument<string>(() => null), Required = false
                });

            AddArgument(new Argument<string>
            {
                Arity = ArgumentArity.ExactlyOne, Description = "Media image path", Name = "image-path"
            });

            Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
        }

        public static int Invoke(bool debug, bool verbose, string encoding, string imagePath, bool longFormat,
                                 string @namespace, string options)
        {
            MainClass.PrintCopyright();

            if(debug)
                DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            DicConsole.DebugWriteLine("Ls command", "--debug={0}", debug);
            DicConsole.DebugWriteLine("Ls command", "--encoding={0}", encoding);
            DicConsole.DebugWriteLine("Ls command", "--input={0}", imagePath);
            DicConsole.DebugWriteLine("Ls command", "--options={0}", options);
            DicConsole.DebugWriteLine("Ls command", "--verbose={0}", verbose);
            Statistics.AddCommand("ls");

            var     filtersList = new FiltersList();
            IFilter inputFilter = filtersList.GetFilter(imagePath);

            Dictionary<string, string> parsedOptions = Core.Options.Parse(options);
            DicConsole.DebugWriteLine("Ls command", "Parsed options:");

            foreach(KeyValuePair<string, string> parsedOption in parsedOptions)
                DicConsole.DebugWriteLine("Ls command", "{0} = {1}", parsedOption.Key, parsedOption.Value);

            parsedOptions.Add("debug", debug.ToString());

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

                try
                {
                    if(!imageFormat.Open(inputFilter))
                    {
                        DicConsole.WriteLine("Unable to open image format");
                        DicConsole.WriteLine("No error given");

                        return(int)ErrorNumber.CannotOpenFormat;
                    }

                    DicConsole.DebugWriteLine("Ls command", "Correctly opened image file.");

                    DicConsole.DebugWriteLine("Ls command", "Image without headers is {0} bytes.",
                                              imageFormat.Info.ImageSize);

                    DicConsole.DebugWriteLine("Ls command", "Image has {0} sectors.", imageFormat.Info.Sectors);

                    DicConsole.DebugWriteLine("Ls command", "Image identifies disk type as {0}.",
                                              imageFormat.Info.MediaType);

                    Statistics.AddMediaFormat(imageFormat.Format);
                    Statistics.AddMedia(imageFormat.Info.MediaType, false);
                    Statistics.AddFilter(inputFilter.Name);
                }
                catch(Exception ex)
                {
                    DicConsole.ErrorWriteLine("Unable to open image format");
                    DicConsole.ErrorWriteLine("Error: {0}", ex.Message);

                    return(int)ErrorNumber.CannotOpenFormat;
                }

                List<Partition> partitions = Core.Partitions.GetAll(imageFormat);
                Core.Partitions.AddSchemesToStats(partitions);

                List<string>        idPlugins;
                IReadOnlyFilesystem plugin;
                Errno               error;

                if(partitions.Count == 0)
                    DicConsole.DebugWriteLine("Ls command", "No partitions found");
                else
                {
                    DicConsole.WriteLine("{0} partitions found.", partitions.Count);

                    for(int i = 0; i < partitions.Count; i++)
                    {
                        DicConsole.WriteLine();
                        DicConsole.WriteLine("Partition {0}:", partitions[i].Sequence);

                        DicConsole.WriteLine("Identifying filesystem on partition");

                        Core.Filesystems.Identify(imageFormat, out idPlugins, partitions[i]);

                        if(idPlugins.Count == 0)
                            DicConsole.WriteLine("Filesystem not identified");
                        else if(idPlugins.Count > 1)
                        {
                            DicConsole.WriteLine($"Identified by {idPlugins.Count} plugins");

                            foreach(string pluginName in idPlugins)
                                if(plugins.ReadOnlyFilesystems.TryGetValue(pluginName, out plugin))
                                {
                                    DicConsole.WriteLine($"As identified by {plugin.Name}.");

                                    var fs = (IReadOnlyFilesystem)plugin.
                                                                  GetType().GetConstructor(Type.EmptyTypes)?.
                                                                  Invoke(new object[]
                                                                             { });

                                    if(fs == null)
                                        continue;

                                    error = fs.Mount(imageFormat, partitions[i], encodingClass, parsedOptions,
                                                     @namespace);

                                    if(error == Errno.NoError)
                                    {
                                        ListFilesInDir("/", fs, longFormat);

                                        Statistics.AddFilesystem(fs.XmlFsType.Type);
                                    }
                                    else
                                        DicConsole.ErrorWriteLine("Unable to mount device, error {0}",
                                                                  error.ToString());
                                }
                        }
                        else
                        {
                            plugins.ReadOnlyFilesystems.TryGetValue(idPlugins[0], out plugin);

                            if(plugin == null)
                                continue;

                            DicConsole.WriteLine($"Identified by {plugin.Name}.");

                            var fs = (IReadOnlyFilesystem)plugin.
                                                          GetType().GetConstructor(Type.EmptyTypes)?.Invoke(new object[]
                                                                                                                { });

                            if(fs == null)
                                continue;

                            error = fs.Mount(imageFormat, partitions[i], encodingClass, parsedOptions, @namespace);

                            if(error == Errno.NoError)
                            {
                                ListFilesInDir("/", fs, longFormat);

                                Statistics.AddFilesystem(fs.XmlFsType.Type);
                            }
                            else
                                DicConsole.ErrorWriteLine("Unable to mount device, error {0}", error.ToString());
                        }
                    }
                }

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
                        if(plugins.ReadOnlyFilesystems.TryGetValue(pluginName, out plugin))
                        {
                            DicConsole.WriteLine($"As identified by {plugin.Name}.");

                            var fs = (IReadOnlyFilesystem)plugin.
                                                          GetType().GetConstructor(Type.EmptyTypes)?.Invoke(new object[]
                                                                                                                { });

                            if(fs == null)
                                continue;

                            error = fs.Mount(imageFormat, wholePart, encodingClass, parsedOptions, @namespace);

                            if(error == Errno.NoError)
                            {
                                ListFilesInDir("/", fs, longFormat);

                                Statistics.AddFilesystem(fs.XmlFsType.Type);
                            }
                            else
                                DicConsole.ErrorWriteLine("Unable to mount device, error {0}", error.ToString());
                        }
                }
                else
                {
                    plugins.ReadOnlyFilesystems.TryGetValue(idPlugins[0], out plugin);

                    if(plugin != null)
                    {
                        DicConsole.WriteLine($"Identified by {plugin.Name}.");

                        var fs = (IReadOnlyFilesystem)plugin.
                                                      GetType().GetConstructor(Type.EmptyTypes)?.Invoke(new object[]
                                                                                                            { });

                        if(fs != null)
                        {
                            error = fs.Mount(imageFormat, wholePart, encodingClass, parsedOptions, @namespace);

                            if(error == Errno.NoError)
                            {
                                ListFilesInDir("/", fs, longFormat);

                                Statistics.AddFilesystem(fs.XmlFsType.Type);
                            }
                            else
                                DicConsole.ErrorWriteLine("Unable to mount device, error {0}", error.ToString());
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                DicConsole.ErrorWriteLine($"Error reading file: {ex.Message}");
                DicConsole.DebugWriteLine("Ls command", ex.StackTrace);

                return(int)ErrorNumber.UnexpectedException;
            }

            return(int)ErrorNumber.NoError;
        }

        static void ListFilesInDir(string path, IReadOnlyFilesystem fs, bool longFormat)
        {
            if(path.StartsWith("/"))
                path = path.Substring(1);

            DicConsole.WriteLine(string.IsNullOrEmpty(path) ? "Root directory" : $"Directory: {path}");

            Errno error = fs.ReadDir(path, out List<string> directory);

            if(error != Errno.NoError)
            {
                DicConsole.ErrorWriteLine("Error {0} reading root directory {1}", error.ToString(), path);

                return;
            }

            Dictionary<string, FileEntryInfo> stats = new Dictionary<string, FileEntryInfo>();

            foreach(string entry in directory)
            {
                fs.Stat(path + "/" + entry, out FileEntryInfo stat);

                stats.Add(entry, stat);
            }

            foreach(KeyValuePair<string, FileEntryInfo> entry in
                stats.OrderBy(e => e.Value?.Attributes.HasFlag(FileAttributes.Directory) == false))
                if(longFormat)
                {
                    if(entry.Value != null)
                    {
                        if(entry.Value.Attributes.HasFlag(FileAttributes.Directory))
                            DicConsole.WriteLine("{0, 10:d} {0, 12:T}  {1, -20}  {2}", entry.Value.CreationTimeUtc,
                                                 "<DIR>", entry.Key);
                        else
                            DicConsole.WriteLine("{0, 10:d} {0, 12:T}  {1, 6}{2, 14:##,#}  {3}",
                                                 entry.Value.CreationTimeUtc, entry.Value.Inode, entry.Value.Length,
                                                 entry.Key);

                        error = fs.ListXAttr(path + "/" + entry.Key, out List<string> xattrs);

                        if(error != Errno.NoError)
                            continue;

                        foreach(string xattr in xattrs)
                        {
                            byte[] xattrBuf = new byte[0];
                            error = fs.GetXattr(path + "/" + entry.Key, xattr, ref xattrBuf);

                            if(error == Errno.NoError)
                                DicConsole.WriteLine("\t\t{0}\t{1:##,#}", xattr, xattrBuf.Length);
                        }
                    }
                    else
                        DicConsole.WriteLine("{0, 47}{1}", string.Empty, entry.Key);
                }
                else
                {
                    if(entry.Value != null &&
                       entry.Value.Attributes.HasFlag(FileAttributes.Directory))
                        DicConsole.WriteLine("{0}/", entry.Key);
                    else
                        DicConsole.WriteLine("{0}", entry.Key);
                }

            DicConsole.WriteLine();

            foreach(KeyValuePair<string, FileEntryInfo> subdirectory in
                stats.Where(e => e.Value?.Attributes.HasFlag(FileAttributes.Directory) == true))
                ListFilesInDir(path + "/" + subdirectory.Key, fs, longFormat);
        }
    }
}