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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Core;
using Aaru.Localization;
using JetBrains.Annotations;
using Spectre.Console;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Commands.Filesystem;

sealed class ExtractFilesCommand : Command
{
    const long   BUFFER_SIZE = 16777216;
    const string MODULE_NAME = "Extract-Files command";

    public ExtractFilesCommand() : base("extract", UI.Filesystem_Extract_Command_Description)
    {
        Add(new Option<string>(new[]
        {
            "--encoding", "-e"
        }, () => null, UI.Name_of_character_encoding_to_use));

        Add(new Option<string>(new[]
        {
            "--options", "-O"
        }, () => null, UI.Comma_separated_name_value_pairs_of_filesystem_options));

        Add(new Option<bool>(new[]
        {
            "--xattrs", "-x"
        }, () => false, UI.Extract_extended_attributes_if_present));

        Add(new Option<string>(new[]
        {
            "--namespace", "-n"
        }, () => null, UI.Namespace_to_use_for_filenames));

        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = UI.Disc_image_path,
            Name        = "image-path"
        });

        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = UI.Directory_where_extracted_files_will_be_created,
            Name        = "output-dir"
        });

        Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)) ?? throw new NullReferenceException());
    }

    public static int Invoke(bool   debug,      bool   verbose,   string encoding, bool xattrs, string imagePath,
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
        {
            AaruConsole.WriteEvent += (format, objects) =>
            {
                if(objects is null)
                    AnsiConsole.Markup(format);
                else
                    AnsiConsole.Markup(format, objects);
            };
        }

        Statistics.AddCommand("extract-files");

        AaruConsole.DebugWriteLine(MODULE_NAME, "--debug={0}",    debug);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--encoding={0}", encoding);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--input={0}",    imagePath);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--options={0}",  options);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--output={0}",   outputDir);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--verbose={0}",  verbose);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--xattrs={0}",   xattrs);

        IFilter inputFilter = null;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Identifying_file_filter).IsIndeterminate();
            inputFilter = PluginRegister.Singleton.GetFilter(imagePath);
        });

        Dictionary<string, string> parsedOptions = Core.Options.Parse(options);
        AaruConsole.DebugWriteLine(MODULE_NAME, UI.Parsed_options);

        foreach(KeyValuePair<string, string> parsedOption in parsedOptions)
            AaruConsole.DebugWriteLine(MODULE_NAME, "{0} = {1}", parsedOption.Key, parsedOption.Value);

        parsedOptions.Add("debug", debug.ToString());

        if(inputFilter == null)
        {
            AaruConsole.ErrorWriteLine(UI.Cannot_open_specified_file);

            return (int)ErrorNumber.CannotOpenFile;
        }

        Encoding encodingClass = null;

        if(encoding != null)
        {
            try
            {
                encodingClass = Claunia.Encoding.Encoding.GetEncoding(encoding);

                if(verbose)
                    AaruConsole.VerboseWriteLine(UI.encoding_for_0, encodingClass.EncodingName);
            }
            catch(ArgumentException)
            {
                AaruConsole.ErrorWriteLine(UI.Specified_encoding_is_not_supported);

                return (int)ErrorNumber.EncodingUnknown;
            }
        }

        PluginRegister plugins = PluginRegister.Singleton;

        try
        {
            IMediaImage imageFormat = null;
            IBaseImage  baseImage   = null;

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask(UI.Identifying_image_format).IsIndeterminate();
                baseImage   = ImageFormat.Detect(inputFilter);
                imageFormat = baseImage as IMediaImage;
            });

            if(baseImage == null)
            {
                AaruConsole.WriteLine(UI.Image_format_not_identified_not_proceeding_with_file_extraction);

                return (int)ErrorNumber.UnrecognizedFormat;
            }

            if(imageFormat == null)
            {
                AaruConsole.WriteLine(UI.Command_not_supported_for_this_image_type);

                return (int)ErrorNumber.InvalidArgument;
            }

            if(verbose)
                AaruConsole.VerboseWriteLine(UI.Image_format_identified_by_0_1, imageFormat.Name, imageFormat.Id);
            else
                AaruConsole.WriteLine(UI.Image_format_identified_by_0, imageFormat.Name);

            if(outputDir == null)
            {
                AaruConsole.WriteLine(UI.Output_directory_missing);

                return (int)ErrorNumber.MissingArgument;
            }

            if(Directory.Exists(outputDir) || File.Exists(outputDir))
            {
                AaruConsole.ErrorWriteLine(UI.Destination_exists_aborting);

                return (int)ErrorNumber.FileExists;
            }

            Directory.CreateDirectory(outputDir);

            try
            {
                ErrorNumber opened = ErrorNumber.NoData;

                Core.Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask(UI.Invoke_Opening_image_file).IsIndeterminate();
                    opened = imageFormat.Open(inputFilter);
                });

                if(opened != ErrorNumber.NoError)
                {
                    AaruConsole.WriteLine(UI.Unable_to_open_image_format);
                    AaruConsole.WriteLine(Localization.Core.Error_0, opened);

                    return (int)opened;
                }

                AaruConsole.DebugWriteLine(MODULE_NAME, UI.Correctly_opened_image_file);

                AaruConsole.DebugWriteLine(MODULE_NAME, UI.Image_without_headers_is_0_bytes,
                                           imageFormat.Info.ImageSize);

                AaruConsole.DebugWriteLine(MODULE_NAME, UI.Image_has_0_sectors, imageFormat.Info.Sectors);

                AaruConsole.DebugWriteLine(MODULE_NAME, UI.Image_identifies_media_type_as_0,
                                           imageFormat.Info.MediaType);

                Statistics.AddMediaFormat(imageFormat.Format);
                Statistics.AddMedia(imageFormat.Info.MediaType, false);
                Statistics.AddFilter(inputFilter.Name);
            }
            catch(Exception ex)
            {
                AaruConsole.ErrorWriteLine(UI.Unable_to_open_image_format);
                AaruConsole.ErrorWriteLine(Localization.Core.Error_0, ex.Message);

                return (int)ErrorNumber.CannotOpenFormat;
            }

            List<Partition> partitions = null;

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask(UI.Enumerating_partitions).IsIndeterminate();
                partitions = Core.Partitions.GetAll(imageFormat);
            });

            Core.Partitions.AddSchemesToStats(partitions);

            if(partitions.Count == 0)
            {
                AaruConsole.DebugWriteLine(MODULE_NAME, UI.No_partitions_found);

                partitions.Add(new Partition
                {
                    Description = Localization.Core.Whole_device,
                    Length      = imageFormat.Info.Sectors,
                    Offset      = 0,
                    Size        = imageFormat.Info.SectorSize * imageFormat.Info.Sectors,
                    Sequence    = 1,
                    Start       = 0
                });
            }

            AaruConsole.WriteLine(UI._0_partitions_found, partitions.Count);

            for(var i = 0; i < partitions.Count; i++)
            {
                AaruConsole.WriteLine();
                AaruConsole.WriteLine($"[bold]{string.Format(UI.Partition_0, partitions[i].Sequence)}[/]");

                List<string> idPlugins = null;

                Core.Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask(UI.Identifying_filesystems_on_partition).IsIndeterminate();
                    Core.Filesystems.Identify(imageFormat, out idPlugins, partitions[i]);
                });

                if(idPlugins.Count == 0)
                    AaruConsole.WriteLine(UI.Filesystem_not_identified);
                else
                {
                    ErrorNumber error = ErrorNumber.InvalidArgument;

                    if(idPlugins.Count > 1)
                    {
                        AaruConsole.WriteLine($"[italic]{string.Format(UI.Identified_by_0_plugins, idPlugins.Count)
                        }[/]");

                        foreach(string pluginName in idPlugins)
                        {
                            if(!plugins.ReadOnlyFilesystems.TryGetValue(pluginName, out IReadOnlyFilesystem fs))
                                continue;

                            if(fs is null)
                                continue;

                            AaruConsole.WriteLine($"[bold]{string.Format(UI.As_identified_by_0, fs.Name)
                            }[/]");

                            Core.Spectre.ProgressSingleSpinner(ctx =>
                            {
                                ctx.AddTask(UI.Mounting_filesystem).IsIndeterminate();

                                error = fs.Mount(imageFormat, partitions[i], encodingClass, parsedOptions, @namespace);
                            });

                            if(error == ErrorNumber.NoError)
                            {
                                string volumeName = string.IsNullOrEmpty(fs.Metadata.VolumeName)
                                                        ? "NO NAME"
                                                        : fs.Metadata.VolumeName;

                                ExtractFilesInDir("/", fs, volumeName, outputDir, xattrs);

                                Statistics.AddFilesystem(fs.Metadata.Type);
                            }
                            else
                                AaruConsole.ErrorWriteLine(UI.Unable_to_mount_volume_error_0, error.ToString());
                        }
                    }
                    else
                    {
                        plugins.ReadOnlyFilesystems.TryGetValue(idPlugins[0], out IReadOnlyFilesystem fs);

                        if(fs is null)
                            continue;

                        AaruConsole.WriteLine($"[bold]{string.Format(UI.Identified_by_0, fs.Name)}[/]");

                        Core.Spectre.ProgressSingleSpinner(ctx =>
                        {
                            ctx.AddTask(UI.Mounting_filesystem).IsIndeterminate();
                            error = fs.Mount(imageFormat, partitions[i], encodingClass, parsedOptions, @namespace);
                        });

                        if(error == ErrorNumber.NoError)
                        {
                            string volumeName = string.IsNullOrEmpty(fs.Metadata.VolumeName)
                                                    ? "NO NAME"
                                                    : fs.Metadata.VolumeName;

                            ExtractFilesInDir("/", fs, volumeName, outputDir, xattrs);

                            Statistics.AddFilesystem(fs.Metadata.Type);
                        }
                        else
                            AaruConsole.ErrorWriteLine(UI.Unable_to_mount_volume_error_0, error.ToString());
                    }
                }
            }
        }
        catch(Exception ex)
        {
            AaruConsole.ErrorWriteLine(string.Format(UI.Error_reading_file_0, ex.Message));
            AaruConsole.DebugWriteLine(MODULE_NAME, ex.StackTrace);

            return (int)ErrorNumber.UnexpectedException;
        }

        return (int)ErrorNumber.NoError;
    }

    static void ExtractFilesInDir(string path, [NotNull] IReadOnlyFilesystem fs, string volumeName, string outputDir,
                                  bool   doXattrs)
    {
        if(path.StartsWith('/'))
            path = path[1..];

        ErrorNumber error = fs.OpenDir(path, out IDirNode node);

        if(error != ErrorNumber.NoError)
        {
            AaruConsole.ErrorWriteLine(UI.Error_0_reading_directory_1, error.ToString(), path);

            return;
        }

        while(fs.ReadDir(node, out string entry) == ErrorNumber.NoError && entry is not null)
        {
            FileEntryInfo stat = new();

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask(UI.Retrieving_file_information).IsIndeterminate();
                error = fs.Stat(path + "/" + entry, out stat);
            });

            if(error == ErrorNumber.NoError)
            {
                string outputPath;

                if(stat.Attributes.HasFlag(FileAttributes.Directory))
                {
                    outputPath = Path.Combine(outputDir, fs.Metadata.Type, volumeName, path, entry);

                    if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        outputPath = outputPath.Replace('<', '\uFF1C').
                                                Replace('>',  '\uFF1E').
                                                Replace(':',  '\uFF1A').
                                                Replace('\"', '\uFF02').
                                                Replace('|',  '\uFF5C').
                                                Replace('?',  '\uFF1F').
                                                Replace('*',  '\uFF0A').
                                                Replace('/',  '\\');
                    }

                    Directory.CreateDirectory(outputPath);

                    AaruConsole.WriteLine(UI.Created_subdirectory_at_0, Markup.Escape(outputPath));

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
                        ctx.AddTask(UI.Listing_extended_attributes).IsIndeterminate();
                        error = fs.ListXAttr(path + "/" + entry, out xattrs);
                    });

                    if(error == ErrorNumber.NoError)
                    {
                        foreach(string xattr in xattrs)
                        {
                            byte[] xattrBuf = Array.Empty<byte>();

                            Core.Spectre.ProgressSingleSpinner(ctx =>
                            {
                                ctx.AddTask(UI.Reading_extended_attribute).IsIndeterminate();
                                error = fs.GetXattr(path + "/" + entry, xattr, ref xattrBuf);
                            });

                            if(error != ErrorNumber.NoError)
                                continue;

                            outputPath = Path.Combine(outputDir, fs.Metadata.Type, volumeName, ".xattrs", path, xattr);

                            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            {
                                outputPath = outputPath.Replace('<', '\uFF1C').
                                                        Replace('>',  '\uFF1E').
                                                        Replace(':',  '\uFF1A').
                                                        Replace('\"', '\uFF02').
                                                        Replace('|',  '\uFF5C').
                                                        Replace('?',  '\uFF1F').
                                                        Replace('*',  '\uFF0A').
                                                        Replace('/',  '\\');
                            }

                            Directory.CreateDirectory(outputPath);

                            outputPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                                             ? Path.Combine(outputPath,
                                                            entry.Replace('/', '\uFF0F').Replace('\\', '\uFF3C'))
                                             : Path.Combine(outputPath, entry);

                            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            {
                                outputPath = outputPath.Replace('<', '\uFF1C').
                                                        Replace('>',  '\uFF1E').
                                                        Replace(':',  '\uFF1A').
                                                        Replace('\"', '\uFF02').
                                                        Replace('|',  '\uFF5C').
                                                        Replace('?',  '\uFF1F').
                                                        Replace('*',  '\uFF0A').
                                                        Replace('/',  '\\');
                            }

                            if(!File.Exists(outputPath) && !Directory.Exists(outputPath))
                            {
                                Core.Spectre.ProgressSingleSpinner(ctx =>
                                {
                                    ctx.AddTask(UI.Writing_extended_attribute).IsIndeterminate();

                                    outputFile = new FileStream(outputPath, FileMode.CreateNew, FileAccess.ReadWrite,
                                                                FileShare.None);

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
                                AaruConsole.WriteLine(UI.Written_0_bytes_of_xattr_1_from_file_2_to_3, xattrBuf.Length,
                                                      xattr, entry, outputPath);
                            }
                            else
                                AaruConsole.ErrorWriteLine(UI.Cannot_write_xattr_0_for_1_output_exists, xattr, entry);
                        }
                    }
                }

                outputPath = Path.Combine(outputDir, fs.Metadata.Type, volumeName, path);

                if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    outputPath = outputPath.Replace('<', '\uFF1C').
                                            Replace('>',  '\uFF1E').
                                            Replace(':',  '\uFF1A').
                                            Replace('\"', '\uFF02').
                                            Replace('|',  '\uFF5C').
                                            Replace('?',  '\uFF1F').
                                            Replace('*',  '\uFF0A').
                                            Replace('/',  '\\');
                }

                Directory.CreateDirectory(outputPath);

                outputPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                                 ? Path.Combine(outputPath, entry.Replace('/', '\uFF0F').Replace('\\', '\uFF3C'))
                                 : Path.Combine(outputPath, entry);

                if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    outputPath = outputPath.Replace('<', '\uFF1C').
                                            Replace('>',  '\uFF1E').
                                            Replace(':',  '\uFF1A').
                                            Replace('\"', '\uFF02').
                                            Replace('|',  '\uFF5C').
                                            Replace('?',  '\uFF1F').
                                            Replace('*',  '\uFF0A').
                                            Replace('/',  '\\');
                }

                if(!File.Exists(outputPath) && !Directory.Exists(outputPath))
                {
                    long position = 0;

                    outputFile = new FileStream(outputPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);

                    AnsiConsole.Progress().
                                AutoClear(true).
                                HideCompleted(true).
                                Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn()).
                                Start(ctx =>
                                {
                                    ProgressTask task =
                                        ctx.AddTask(string.Format(UI.Reading_file_0, Markup.Escape(entry)));

                                    task.MaxValue = stat.Length;
                                    var outBuf = new byte[BUFFER_SIZE];
                                    error = fs.OpenFile(path + "/" + entry, out IFileNode fileNode);

                                    if(error == ErrorNumber.NoError)
                                    {
                                        while(position < stat.Length)
                                        {
                                            long bytesToRead;

                                            if(stat.Length - position > BUFFER_SIZE)
                                                bytesToRead = BUFFER_SIZE;
                                            else
                                                bytesToRead = stat.Length - position;

                                            error = fs.ReadFile(fileNode, bytesToRead, outBuf, out long bytesRead);

                                            if(error == ErrorNumber.NoError)
                                                outputFile.Write(outBuf, 0, (int)bytesRead);
                                            else
                                            {
                                                AaruConsole.ErrorWriteLine(UI.Error_0_reading_file_1, error, entry);

                                                break;
                                            }

                                            position += bytesToRead;
                                            task.Increment(bytesToRead);
                                        }

                                        fs.CloseFile(fileNode);
                                    }
                                    else
                                        AaruConsole.ErrorWriteLine(UI.Error_0_reading_file_1, error, entry);
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
                    AaruConsole.WriteLine(UI.Written_0_bytes_of_file_1_to_2, position, Markup.Escape(entry),
                                          Markup.Escape(outputPath));
                }
                else
                    AaruConsole.ErrorWriteLine(UI.Cannot_write_file_0_output_exists, Markup.Escape(entry));
            }
            else
                AaruConsole.ErrorWriteLine(UI.Error_reading_file_0, Markup.Escape(entry));
        }

        fs.CloseDir(node);
    }
}