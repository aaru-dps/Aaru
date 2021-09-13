// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ExtractFiles.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'extract' command.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Core;
using JetBrains.Annotations;
using Spectre.Console;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Commands.Filesystem
{
    internal sealed class ExtractFilesCommand : Command
    {
        const long BUFFER_SIZE = 16777216;

        public ExtractFilesCommand() : base("extract", "Extracts all files in disc image.")
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
                    "--options", "-O"
                }, "Comma separated name=value pairs of options to pass to filesystem plugin.")
                {
                    Argument = new Argument<string>(() => null),
                    Required = false
                });

            Add(new Option(new[]
                {
                    "--xattrs", "-x"
                }, "Extract extended attributes if present.")
                {
                    Argument = new Argument<bool>(() => false),
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
                Description = "Disc image path",
                Name        = "image-path"
            });

            AddArgument(new Argument<string>
            {
                Arity       = ArgumentArity.ExactlyOne,
                Description = "Directory where extracted files will be created. Will abort if it exists",
                Name        = "output-dir"
            });

            Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
        }

        public static int Invoke(bool debug, bool verbose, string encoding, bool xattrs, string imagePath,
                                 string @namespace, string outputDir, string options)
        {
            MainClass.PrintCopyright();

            if(debug)
            {
                IAnsiConsole stderrConsole = AnsiConsole.Create(new AnsiConsoleSettings
                {
                    Out = new AnsiConsoleOutput(System.Console.Error)
                });

                AaruConsole.DebugWriteLineEvent += (format, objects) =>
                {
                    if(objects is null)
                        stderrConsole.MarkupLine(format);
                    else
                        stderrConsole.MarkupLine(format, objects);
                };
            }

            if(verbose)
                AaruConsole.WriteEvent += (format, objects) =>
                {
                    if(objects is null)
                        AnsiConsole.Markup(format);
                    else
                        AnsiConsole.Markup(format, objects);
                };

            Statistics.AddCommand("extract-files");

            AaruConsole.DebugWriteLine("Extract-Files command", "--debug={0}", debug);
            AaruConsole.DebugWriteLine("Extract-Files command", "--encoding={0}", encoding);
            AaruConsole.DebugWriteLine("Extract-Files command", "--input={0}", imagePath);
            AaruConsole.DebugWriteLine("Extract-Files command", "--options={0}", options);
            AaruConsole.DebugWriteLine("Extract-Files command", "--output={0}", outputDir);
            AaruConsole.DebugWriteLine("Extract-Files command", "--verbose={0}", verbose);
            AaruConsole.DebugWriteLine("Extract-Files command", "--xattrs={0}", xattrs);

            var     filtersList = new FiltersList();
            IFilter inputFilter = null;

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Identifying file filter...").IsIndeterminate();
                inputFilter = filtersList.GetFilter(imagePath);
            });

            Dictionary<string, string> parsedOptions = Core.Options.Parse(options);
            AaruConsole.DebugWriteLine("Extract-Files command", "Parsed options:");

            foreach(KeyValuePair<string, string> parsedOption in parsedOptions)
                AaruConsole.DebugWriteLine("Extract-Files command", "{0} = {1}", parsedOption.Key, parsedOption.Value);

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
                IMediaImage imageFormat = null;

                Core.Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Identifying image format...").IsIndeterminate();
                    imageFormat = ImageFormat.Detect(inputFilter);
                });

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

                if(outputDir == null)
                {
                    AaruConsole.WriteLine("Output directory missing.");

                    return (int)ErrorNumber.MissingArgument;
                }

                if(Directory.Exists(outputDir) ||
                   File.Exists(outputDir))
                {
                    AaruConsole.ErrorWriteLine("Destination exists, aborting.");

                    return (int)ErrorNumber.DestinationExists;
                }

                Directory.CreateDirectory(outputDir);

                try
                {
                    bool opened = false;

                    Core.Spectre.ProgressSingleSpinner(ctx =>
                    {
                        ctx.AddTask("Opening image file...").IsIndeterminate();
                        opened = imageFormat.Open(inputFilter);
                    });

                    if(!opened)
                    {
                        AaruConsole.WriteLine("Unable to open image format");
                        AaruConsole.WriteLine("No error given");

                        return (int)ErrorNumber.CannotOpenFormat;
                    }

                    AaruConsole.DebugWriteLine("Extract-Files command", "Correctly opened image file.");

                    AaruConsole.DebugWriteLine("Extract-Files command", "Image without headers is {0} bytes.",
                                               imageFormat.Info.ImageSize);

                    AaruConsole.DebugWriteLine("Extract-Files command", "Image has {0} sectors.",
                                               imageFormat.Info.Sectors);

                    AaruConsole.DebugWriteLine("Extract-Files command", "Image identifies disk type as {0}.",
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

                List<Partition> partitions = null;

                Core.Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Enumerating partitions...").IsIndeterminate();
                    partitions = Core.Partitions.GetAll(imageFormat);
                });

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
                    AaruConsole.WriteLine("[bold]Partition {0}:[/]", partitions[i].Sequence);

                    List<string> idPlugins = null;

                    Core.Spectre.ProgressSingleSpinner(ctx =>
                    {
                        ctx.AddTask("Identifying filesystems on partition...").IsIndeterminate();
                        Core.Filesystems.Identify(imageFormat, out idPlugins, partitions[i]);
                    });

                    if(idPlugins.Count == 0)
                        AaruConsole.WriteLine("Filesystem not identified");
                    else
                    {
                        IReadOnlyFilesystem plugin;
                        Errno               error = Errno.InvalidArgument;

                        if(idPlugins.Count > 1)
                        {
                            AaruConsole.WriteLine($"[italic]Identified by {idPlugins.Count} plugins[/]");

                            foreach(string pluginName in idPlugins)
                                if(plugins.ReadOnlyFilesystems.TryGetValue(pluginName, out plugin))
                                {
                                    AaruConsole.WriteLine($"[bold]As identified by {plugin.Name}.[/]");

                                    var fs = (IReadOnlyFilesystem)plugin.GetType().GetConstructor(Type.EmptyTypes)?.
                                                                         Invoke(new object[]
                                                                                    {});

                                    Core.Spectre.ProgressSingleSpinner(ctx =>
                                    {
                                        ctx.AddTask("Mounting filesystem...").IsIndeterminate();

                                        error = fs.Mount(imageFormat, partitions[i], encodingClass, parsedOptions,
                                                         @namespace);
                                    });

                                    if(error == Errno.NoError)
                                    {
                                        string volumeName = string.IsNullOrEmpty(fs.XmlFsType.VolumeName) ? "NO NAME"
                                                                : fs.XmlFsType.VolumeName;

                                        ExtractFilesInDir("/", fs, volumeName, outputDir, xattrs);

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

                            AaruConsole.WriteLine($"[bold]Identified by {plugin.Name}.[/]");

                            var fs = (IReadOnlyFilesystem)plugin.GetType().GetConstructor(Type.EmptyTypes)?.
                                                                 Invoke(new object[]
                                                                            {});

                            Core.Spectre.ProgressSingleSpinner(ctx =>
                            {
                                ctx.AddTask("Mounting filesystem...").IsIndeterminate();
                                error = fs.Mount(imageFormat, partitions[i], encodingClass, parsedOptions, @namespace);
                            });

                            if(error == Errno.NoError)
                            {
                                string volumeName = string.IsNullOrEmpty(fs.XmlFsType.VolumeName) ? "NO NAME"
                                                        : fs.XmlFsType.VolumeName;

                                ExtractFilesInDir("/", fs, volumeName, outputDir, xattrs);

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
                AaruConsole.DebugWriteLine("Extract-Files command", ex.StackTrace);

                return (int)ErrorNumber.UnexpectedException;
            }

            return (int)ErrorNumber.NoError;
        }

        static void ExtractFilesInDir(string path, [NotNull] IReadOnlyFilesystem fs, string volumeName,
                                      string outputDir, bool doXattrs)
        {
            if(path.StartsWith('/'))
                path = path.Substring(1);

            Errno error = fs.ReadDir(path, out List<string> directory);

            if(error != Errno.NoError)
            {
                AaruConsole.ErrorWriteLine("Error {0} reading root directory {0}", error.ToString());

                return;
            }

            foreach(string entry in directory)
            {
                FileEntryInfo stat = new();

                Core.Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Retrieving file information...").IsIndeterminate();
                    error = fs.Stat(path + "/" + entry, out stat);
                });

                if(error == Errno.NoError)
                {
                    string outputPath;

                    if(stat.Attributes.HasFlag(FileAttributes.Directory))
                    {
                        outputPath = Path.Combine(outputDir, fs.XmlFsType.Type, volumeName, path, entry);

                        Directory.CreateDirectory(outputPath);

                        AaruConsole.WriteLine("Created subdirectory at {0}", Markup.Escape(outputPath));

                        ExtractFilesInDir(path + "/" + entry, fs, volumeName, outputDir, doXattrs);

                        var di = new DirectoryInfo(outputPath);

                        #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                        try
                        {
                            if(stat.CreationTimeUtc.HasValue)
                                di.CreationTimeUtc = stat.CreationTimeUtc.Value;
                        }
                        catch
                        {
                            // ignored
                        }

                        try
                        {
                            if(stat.LastWriteTimeUtc.HasValue)
                                di.LastWriteTimeUtc = stat.LastWriteTimeUtc.Value;
                        }
                        catch
                        {
                            // ignored
                        }

                        try
                        {
                            if(stat.AccessTimeUtc.HasValue)
                                di.LastAccessTimeUtc = stat.AccessTimeUtc.Value;
                        }
                        catch
                        {
                            // ignored
                        }
                        #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body

                        continue;
                    }

                    FileStream outputFile;

                    if(doXattrs)
                    {
                        List<string> xattrs = null;

                        Core.Spectre.ProgressSingleSpinner(ctx =>
                        {
                            ctx.AddTask("Listing extended attributes...").IsIndeterminate();
                            error = fs.ListXAttr(path + "/" + entry, out xattrs);
                        });

                        if(error == Errno.NoError)
                            foreach(string xattr in xattrs)
                            {
                                byte[] xattrBuf = Array.Empty<byte>();

                                Core.Spectre.ProgressSingleSpinner(ctx =>
                                {
                                    ctx.AddTask("Reading extended attribute...").IsIndeterminate();
                                    error = fs.GetXattr(path + "/" + entry, xattr, ref xattrBuf);
                                });

                                if(error != Errno.NoError)
                                    continue;

                                outputPath = Path.Combine(outputDir, fs.XmlFsType.Type, volumeName, ".xattrs", path,
                                                          xattr);

                                Directory.CreateDirectory(outputPath);

                                outputPath = Path.Combine(outputPath, entry);

                                if(!File.Exists(outputPath))
                                {
                                    Core.Spectre.ProgressSingleSpinner(ctx =>
                                    {
                                        ctx.AddTask("Writing extended attribute...").IsIndeterminate();

                                        outputFile = new FileStream(outputPath, FileMode.CreateNew,
                                                                    FileAccess.ReadWrite, FileShare.None);

                                        outputFile.Write(xattrBuf, 0, xattrBuf.Length);
                                        outputFile.Close();
                                    });

                                    var fi = new FileInfo(outputPath);
                                    #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                                    try
                                    {
                                        if(stat.CreationTimeUtc.HasValue)
                                            fi.CreationTimeUtc = stat.CreationTimeUtc.Value;
                                    }
                                    catch
                                    {
                                        // ignored
                                    }

                                    try
                                    {
                                        if(stat.LastWriteTimeUtc.HasValue)
                                            fi.LastWriteTimeUtc = stat.LastWriteTimeUtc.Value;
                                    }
                                    catch
                                    {
                                        // ignored
                                    }

                                    try
                                    {
                                        if(stat.AccessTimeUtc.HasValue)
                                            fi.LastAccessTimeUtc = stat.AccessTimeUtc.Value;
                                    }
                                    catch
                                    {
                                        // ignored
                                    }
                                    #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                                    AaruConsole.WriteLine("Written {0} bytes of xattr {1} from file {2} to {3}",
                                                          xattrBuf.Length, xattr, entry, outputPath);
                                }
                                else
                                    AaruConsole.ErrorWriteLine("Cannot write xattr {0} for {1}, output exists", xattr,
                                                               entry);
                            }
                    }

                    outputPath = Path.Combine(outputDir, fs.XmlFsType.Type, volumeName, path);

                    Directory.CreateDirectory(outputPath);

                    outputPath = Path.Combine(outputPath, entry);

                    if(!File.Exists(outputPath))
                    {
                        long position = 0;

                        outputFile = new FileStream(outputPath, FileMode.CreateNew, FileAccess.ReadWrite,
                                                    FileShare.None);

                        AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
                                    Columns(new TaskDescriptionColumn(), new ProgressBarColumn(),
                                            new PercentageColumn()).Start(ctx =>
                                    {
                                        ProgressTask task = ctx.AddTask($"Reading file {Markup.Escape(entry)}...");

                                        task.MaxValue = stat.Length;

                                        while(position < stat.Length)
                                        {
                                            long bytesToRead;

                                            if(stat.Length - position > BUFFER_SIZE)
                                                bytesToRead = BUFFER_SIZE;
                                            else
                                                bytesToRead = stat.Length - position;

                                            byte[] outBuf = Array.Empty<byte>();

                                            error = fs.Read(path + "/" + entry, position, bytesToRead, ref outBuf);

                                            if(error == Errno.NoError)
                                                outputFile.Write(outBuf, 0, outBuf.Length);
                                            else
                                            {
                                                AaruConsole.ErrorWriteLine("Error {0} reading file {1}", error, entry);

                                                break;
                                            }

                                            position += bytesToRead;
                                            task.Increment(bytesToRead);
                                        }
                                    });

                        outputFile.Close();

                        var fi = new FileInfo(outputPath);
                        #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                        try
                        {
                            if(stat.CreationTimeUtc.HasValue)
                                fi.CreationTimeUtc = stat.CreationTimeUtc.Value;
                        }
                        catch
                        {
                            // ignored
                        }

                        try
                        {
                            if(stat.LastWriteTimeUtc.HasValue)
                                fi.LastWriteTimeUtc = stat.LastWriteTimeUtc.Value;
                        }
                        catch
                        {
                            // ignored
                        }

                        try
                        {
                            if(stat.AccessTimeUtc.HasValue)
                                fi.LastAccessTimeUtc = stat.AccessTimeUtc.Value;
                        }
                        catch
                        {
                            // ignored
                        }
                        #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                        AaruConsole.WriteLine("Written {0} bytes of file {1} to {2}", position, Markup.Escape(entry),
                                              Markup.Escape(outputPath));
                    }
                    else
                        AaruConsole.ErrorWriteLine("Cannot write file {0}, output exists", Markup.Escape(entry));
                }
                else
                    AaruConsole.ErrorWriteLine("Error reading file {0}", Markup.Escape(entry));
            }
        }
    }
}