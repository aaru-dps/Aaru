// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Ls.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'ls' command.
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
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Core;
using JetBrains.Annotations;

namespace Aaru.Commands.Filesystem
{
    internal sealed class LsCommand : Command
    {
        public LsCommand() : base("list", "Lists files in disc image.")
        {
            AddAlias("ls");

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
                    "--long-format", "-l"
                }, "Uses long format.")
                {
                    Argument = new Argument<bool>(() => true),
                    Required = false
                });

            Add(new Option(new[]
                {
                    "--options", "-O"
                }, "Comma separated name=value pairs of options to pass to filesystem plugin.")
                {
                    Argument = new Argument<string>(() => null),
                    Required = false
                });

            Add(new Option(new[]
                {
                    "--namespace", "-n"
                }, "Namespace to use for filenames.")
                {
                    Argument = new Argument<string>(() => null),
                    Required = false
                });

            AddArgument(new Argument<string>
            {
                Arity       = ArgumentArity.ExactlyOne,
                Description = "Media image path",
                Name        = "image-path"
            });

            Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
        }

        public static int Invoke(bool debug, bool verbose, string encoding, string imagePath, bool longFormat,
                                 string @namespace, string options)
        {
            MainClass.PrintCopyright();

            if(debug)
                AaruConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                AaruConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            AaruConsole.DebugWriteLine("Ls command", "--debug={0}", debug);
            AaruConsole.DebugWriteLine("Ls command", "--encoding={0}", encoding);
            AaruConsole.DebugWriteLine("Ls command", "--input={0}", imagePath);
            AaruConsole.DebugWriteLine("Ls command", "--options={0}", options);
            AaruConsole.DebugWriteLine("Ls command", "--verbose={0}", verbose);
            Statistics.AddCommand("ls");

            var     filtersList = new FiltersList();
            IFilter inputFilter = filtersList.GetFilter(imagePath);

            Dictionary<string, string> parsedOptions = Core.Options.Parse(options);
            AaruConsole.DebugWriteLine("Ls command", "Parsed options:");

            foreach(KeyValuePair<string, string> parsedOption in parsedOptions)
                AaruConsole.DebugWriteLine("Ls command", "{0} = {1}", parsedOption.Key, parsedOption.Value);

            parsedOptions.Add("debug", debug.ToString());

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

                try
                {
                    if(!imageFormat.Open(inputFilter))
                    {
                        AaruConsole.WriteLine("Unable to open image format");
                        AaruConsole.WriteLine("No error given");

                        return (int)ErrorNumber.CannotOpenFormat;
                    }

                    AaruConsole.DebugWriteLine("Ls command", "Correctly opened image file.");

                    AaruConsole.DebugWriteLine("Ls command", "Image without headers is {0} bytes.",
                                               imageFormat.Info.ImageSize);

                    AaruConsole.DebugWriteLine("Ls command", "Image has {0} sectors.", imageFormat.Info.Sectors);

                    AaruConsole.DebugWriteLine("Ls command", "Image identifies disk type as {0}.",
                                               imageFormat.Info.MediaType);

                    Statistics.AddMediaFormat(imageFormat.Format);
                    Statistics.AddMedia(imageFormat.Info.MediaType, false);
                    Statistics.AddFilter(inputFilter.Name);
                }
                catch(Exception ex)
                {
                    AaruConsole.ErrorWriteLine("Unable to open image format");
                    AaruConsole.ErrorWriteLine("Error: {0}", ex.Message);

                    return (int)ErrorNumber.CannotOpenFormat;
                }

                List<Partition> partitions = Core.Partitions.GetAll(imageFormat);
                Core.Partitions.AddSchemesToStats(partitions);

                if(partitions.Count == 0)
                {
                    AaruConsole.DebugWriteLine("Ls command", "No partitions found");

                    partitions.Add(new Partition
                    {
                        Description = "Whole device",
                        Length      = imageFormat.Info.Sectors,
                        Offset      = 0,
                        Size        = imageFormat.Info.SectorSize * imageFormat.Info.Sectors,
                        Sequence    = 1,
                        Start       = 0
                    });
                }

                AaruConsole.WriteLine("{0} partitions found.", partitions.Count);

                for(int i = 0; i < partitions.Count; i++)
                {
                    AaruConsole.WriteLine();
                    AaruConsole.WriteLine("Partition {0}:", partitions[i].Sequence);

                    AaruConsole.WriteLine("Identifying filesystem on partition");

                    Core.Filesystems.Identify(imageFormat, out List<string> idPlugins, partitions[i]);

                    if(idPlugins.Count == 0)
                        AaruConsole.WriteLine("Filesystem not identified");
                    else
                    {
                        IReadOnlyFilesystem plugin;
                        Errno               error;

                        if(idPlugins.Count > 1)
                        {
                            AaruConsole.WriteLine($"Identified by {idPlugins.Count} plugins");

                            foreach(string pluginName in idPlugins)
                                if(plugins.ReadOnlyFilesystems.TryGetValue(pluginName, out plugin))
                                {
                                    AaruConsole.WriteLine($"As identified by {plugin.Name}.");

                                    var fs = (IReadOnlyFilesystem)plugin.GetType().GetConstructor(Type.EmptyTypes)?.
                                                                         Invoke(new object[]
                                                                                    {});

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
                                        AaruConsole.ErrorWriteLine("Unable to mount device, error {0}",
                                                                   error.ToString());
                                }
                        }
                        else
                        {
                            plugins.ReadOnlyFilesystems.TryGetValue(idPlugins[0], out plugin);

                            if(plugin == null)
                                continue;

                            AaruConsole.WriteLine($"Identified by {plugin.Name}.");

                            var fs = (IReadOnlyFilesystem)plugin.GetType().GetConstructor(Type.EmptyTypes)?.
                                                                 Invoke(new object[]
                                                                            {});

                            if(fs == null)
                                continue;

                            error = fs.Mount(imageFormat, partitions[i], encodingClass, parsedOptions, @namespace);

                            if(error == Errno.NoError)
                            {
                                ListFilesInDir("/", fs, longFormat);

                                Statistics.AddFilesystem(fs.XmlFsType.Type);
                            }
                            else
                                AaruConsole.ErrorWriteLine("Unable to mount device, error {0}", error.ToString());
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                AaruConsole.ErrorWriteLine($"Error reading file: {ex.Message}");
                AaruConsole.DebugWriteLine("Ls command", ex.StackTrace);

                return (int)ErrorNumber.UnexpectedException;
            }

            return (int)ErrorNumber.NoError;
        }

        static void ListFilesInDir(string path, [NotNull] IReadOnlyFilesystem fs, bool longFormat)
        {
            if(path.StartsWith('/'))
                path = path.Substring(1);

            AaruConsole.WriteLine(string.IsNullOrEmpty(path) ? "Root directory" : $"Directory: {path}");

            Errno error = fs.ReadDir(path, out List<string> directory);

            if(error != Errno.NoError)
            {
                AaruConsole.ErrorWriteLine("Error {0} reading root directory {1}", error.ToString(), path);

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
                            AaruConsole.WriteLine("{0, 10:d} {0, 12:T}  {1, -20}  {2}", entry.Value.CreationTimeUtc,
                                                  "<DIR>", entry.Key);
                        else
                            AaruConsole.WriteLine("{0, 10:d} {0, 12:T}  {1, 6}{2, 14:N0}  {3}",
                                                  entry.Value.CreationTimeUtc, entry.Value.Inode, entry.Value.Length,
                                                  entry.Key);

                        error = fs.ListXAttr(path + "/" + entry.Key, out List<string> xattrs);

                        if(error != Errno.NoError)
                            continue;

                        foreach(string xattr in xattrs)
                        {
                            byte[] xattrBuf = Array.Empty<byte>();
                            error = fs.GetXattr(path + "/" + entry.Key, xattr, ref xattrBuf);

                            if(error == Errno.NoError)
                                AaruConsole.WriteLine("\t\t{0}\t{1:##,#}", xattr, xattrBuf.Length);
                        }
                    }
                    else
                        AaruConsole.WriteLine("{0, 47}{1}", string.Empty, entry.Key);
                }
                else
                {
                    AaruConsole.
                        WriteLine(entry.Value?.Attributes.HasFlag(FileAttributes.Directory) == true ? "{0}/" : "{0}",
                                  entry.Key);
                }

            AaruConsole.WriteLine();

            foreach(KeyValuePair<string, FileEntryInfo> subdirectory in
                stats.Where(e => e.Value?.Attributes.HasFlag(FileAttributes.Directory) == true))
                ListFilesInDir(path + "/" + subdirectory.Key, fs, longFormat);
        }
    }
}