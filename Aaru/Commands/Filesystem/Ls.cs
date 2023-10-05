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
using System.CommandLine.NamingConventionBinder;
using System.Linq;
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

namespace Aaru.Commands.Filesystem;

sealed class LsCommand : Command
{
    const string MODULE_NAME = "Ls command";

    public LsCommand() : base("list", UI.Filesystem_List_Command_Description)
    {
        AddAlias("ls");

        Add(new Option<string>(new[]
        {
            "--encoding", "-e"
        }, () => null, UI.Name_of_character_encoding_to_use));

        Add(new Option<bool>(new[]
        {
            "--long-format", "-l"
        }, () => true, UI.Use_long_format));

        Add(new Option<string>(new[]
        {
            "--options", "-O"
        }, () => null, UI.Comma_separated_name_value_pairs_of_filesystem_options));

        Add(new Option<string>(new[]
        {
            "--namespace", "-n"
        }, () => null, UI.Namespace_to_use_for_filenames));

        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = UI.Media_image_path,
            Name        = "image-path"
        });

        Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)) ?? throw new NullReferenceException());
    }

    public static int Invoke(bool   debug,      bool   verbose, string encoding, string imagePath, bool longFormat,
                             string @namespace, string options)
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
                    stderrConsole.MarkupLine(Markup.Escape(format));
                else
                    stderrConsole.MarkupLine(Markup.Escape(format), objects);
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

        AaruConsole.DebugWriteLine(MODULE_NAME, "--debug={0}",    debug);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--encoding={0}", encoding);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--input={0}",    imagePath);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--options={0}",  options);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--verbose={0}",  verbose);
        Statistics.AddCommand("ls");

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
                AaruConsole.WriteLine(UI.Image_format_not_identified_not_proceeding_with_listing);

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

                            AaruConsole.WriteLine($"[bold]{string.Format(UI.As_identified_by_0, fs.Name)}[/]");

                            Core.Spectre.ProgressSingleSpinner(ctx =>
                            {
                                ctx.AddTask(UI.Mounting_filesystem).IsIndeterminate();

                                error = fs.Mount(imageFormat, partitions[i], encodingClass, parsedOptions, @namespace);
                            });

                            if(error == ErrorNumber.NoError)
                            {
                                ListFilesInDir("/", fs, longFormat);

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
                            ListFilesInDir("/", fs, longFormat);

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

    static void ListFilesInDir(string path, [NotNull] IReadOnlyFilesystem fs, bool longFormat)
    {
        ErrorNumber error = ErrorNumber.InvalidArgument;
        IDirNode    node  = null;

        if(path.StartsWith('/'))
            path = path[1..];

        AaruConsole.WriteLine(string.IsNullOrEmpty(path)
                                  ? UI.Root_directory
                                  : string.Format(UI.Directory_0, Markup.Escape(path)));

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Reading_directory).IsIndeterminate();
            error = fs.OpenDir(path, out node);
        });

        if(error != ErrorNumber.NoError)
        {
            AaruConsole.ErrorWriteLine(UI.Error_0_reading_directory_1, error.ToString(), path);

            return;
        }

        Dictionary<string, FileEntryInfo> stats = new();

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Retrieving_file_information).IsIndeterminate();

            while(fs.ReadDir(node, out string entry) == ErrorNumber.NoError && entry is not null)
            {
                fs.Stat(path + "/" + entry, out FileEntryInfo stat);

                stats.Add(entry, stat);
            }

            fs.CloseDir(node);
        });

        foreach(KeyValuePair<string, FileEntryInfo> entry in
            stats.OrderBy(e => e.Value?.Attributes.HasFlag(FileAttributes.Directory) == false))
        {
            if(longFormat)
            {
                if(entry.Value != null)
                {
                    if(entry.Value.Attributes.HasFlag(FileAttributes.Directory))
                    {
                        AaruConsole.WriteLine("{0, 10:d} {0, 12:T}  {1, -20}  {2}", entry.Value.CreationTimeUtc,
                                              UI.Directory_abbreviation, Markup.Escape(entry.Key));
                    }
                    else
                    {
                        AaruConsole.WriteLine("{0, 10:d} {0, 12:T}  {1, 6}{2, 14:N0}  {3}", entry.Value.CreationTimeUtc,
                                              entry.Value.Inode, entry.Value.Length, Markup.Escape(entry.Key));
                    }

                    error = fs.ListXAttr(path + "/" + entry.Key, out List<string> xattrs);

                    if(error != ErrorNumber.NoError)
                        continue;

                    foreach(string xattr in xattrs)
                    {
                        byte[] xattrBuf = Array.Empty<byte>();
                        error = fs.GetXattr(path + "/" + entry.Key, xattr, ref xattrBuf);

                        if(error == ErrorNumber.NoError)
                            AaruConsole.WriteLine("\t\t{0}\t{1:##,#}", Markup.Escape(xattr), xattrBuf.Length);
                    }
                }
                else
                    AaruConsole.WriteLine("{0, 47}{1}", string.Empty, Markup.Escape(entry.Key));
            }
            else
            {
                AaruConsole.
                    WriteLine(entry.Value?.Attributes.HasFlag(FileAttributes.Directory) == true ? "{0}/" : "{0}",
                              entry.Key);
            }
        }

        AaruConsole.WriteLine();

        foreach(KeyValuePair<string, FileEntryInfo> subdirectory in
            stats.Where(e => e.Value?.Attributes.HasFlag(FileAttributes.Directory) == true))
            ListFilesInDir(path + "/" + subdirectory.Key, fs, longFormat);
    }
}