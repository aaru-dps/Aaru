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
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Core;
using Aaru.Localization;
using Spectre.Console;

namespace Aaru.Commands.Archive;

sealed class ArchiveListCommand : Command
{
    const string MODULE_NAME = "Archive list command";

    public ArchiveListCommand() : base("list", "Lists files contained in an archive.")
    {
        AddAlias("l");

        Add(new Option<string>(new[]
        {
            "--encoding", "-e"
        }, () => null, UI.Name_of_character_encoding_to_use));

        Add(new Option<bool>(new[]
        {
            "--long-format", "-l"
        }, () => false, UI.Use_long_format));

        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = "Archive file path",
            Name        = "archive-path"
        });

        Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)) ?? throw new NullReferenceException());
    }

    public static int Invoke(bool debug, bool verbose, string encoding, string archivePath, bool longFormat)
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

        AaruConsole.DebugWriteLine(MODULE_NAME, "--debug={0}",       debug);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--encoding={0}",    encoding);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--long-format={0}", longFormat);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--input={0}",       archivePath);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--verbose={0}",     verbose);
        Statistics.AddCommand("archive-list");

        IFilter inputFilter = null;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Identifying_file_filter).IsIndeterminate();
            inputFilter = PluginRegister.Singleton.GetFilter(archivePath);
        });

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
            IArchive archive = null;

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask(UI.Identifying_image_format).IsIndeterminate();
                archive = ArchiveFormat.Detect(inputFilter);
            });

            if(archive == null)
            {
                AaruConsole.WriteLine("Archive format not identified, not proceeding with listing.");

                return (int)ErrorNumber.UnrecognizedFormat;
            }

            if(verbose)
                AaruConsole.VerboseWriteLine("Archive format identified by {0} ({1}).", archive.Name, archive.Id);
            else
                AaruConsole.WriteLine("Archive format identified by {0}.", archive.Name);

            try
            {
                ErrorNumber opened = ErrorNumber.NoData;

                Core.Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask(UI.Invoke_Opening_image_file).IsIndeterminate();
                    opened = archive.Open(inputFilter, encodingClass);
                });

                if(opened != ErrorNumber.NoError)
                {
                    AaruConsole.ErrorWriteLine("Unable to open archive format");
                    AaruConsole.ErrorWriteLine(Localization.Core.Error_0, opened);

                    return (int)opened;
                }

                AaruConsole.DebugWriteLine(MODULE_NAME, "Correctly opened archive file.");

                // TODO: Implement
                //Statistics.AddArchiveFormat(archive.Name);
                Statistics.AddFilter(inputFilter.Name);
            }
            catch(Exception ex)
            {
                AaruConsole.ErrorWriteLine("Unable to open archive format");
                AaruConsole.ErrorWriteLine(Localization.Core.Error_0, ex.Message);

                return (int)ErrorNumber.CannotOpenFormat;
            }

            if(!longFormat)
            {
                for(var i = 0; i < archive.NumberOfEntries; i++)
                {
                    ErrorNumber errno = archive.GetFilename(i, out string fileName);

                    // Ignore that file
                    if(errno != ErrorNumber.NoError)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, "Error {0} getting filename for archive entry #{1}",
                                                   errno, i);
                        continue;
                    }

                    AaruConsole.WriteLine(Markup.Escape(fileName));
                }

                return (int)ErrorNumber.NoError;
            }

            var  table             = new Table();
            var  files             = 0;
            var  folders           = 0;
            long totalSize         = 0;
            long totalUncompressed = 0;

            AnsiConsole.Live(table).
                        Start(ctx =>
                        {
                            table.HideFooters();

                            table.AddColumn(new TableColumn("Date")
                            {
                                NoWrap    = true,
                                Alignment = Justify.Center
                            });
                            ctx.Refresh();

                            table.AddColumn(new TableColumn("Time")
                            {
                                NoWrap    = true,
                                Alignment = Justify.Center
                            });
                            ctx.Refresh();

                            table.AddColumn(new TableColumn("Attr")
                            {
                                NoWrap    = true,
                                Alignment = Justify.Right
                            });
                            ctx.Refresh();

                            table.AddColumn(new TableColumn("Size")
                            {
                                NoWrap    = true,
                                Alignment = Justify.Right
                            });
                            ctx.Refresh();

                            if(archive.ArchiveFeatures.HasFlag(ArchiveSupportedFeature.SupportsCompression))
                            {
                                table.AddColumn(new TableColumn("Compressed")
                                {
                                    NoWrap    = true,
                                    Alignment = Justify.Right
                                });
                            }

                            ctx.Refresh();

                            table.AddColumn(new TableColumn("Name")
                            {
                                Alignment = Justify.Left
                            });
                            ctx.Refresh();

                            for(var i = 0; i < archive.NumberOfEntries; i++)
                            {
                                ErrorNumber errno = archive.GetFilename(i, out string fileName);

                                if(errno != ErrorNumber.NoError)
                                {
                                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                                               "Error {0} retrieving filename for file #{1}.", errno,
                                                               i);
                                    continue;
                                }

                                errno = archive.Stat(i, out FileEntryInfo stat);
                                if(errno != ErrorNumber.NoError)
                                {
                                    AaruConsole.DebugWriteLine(MODULE_NAME, "Error {0} retrieving stat for file #{1}.",
                                                               errno, i);
                                    continue;
                                }

                                var attr = new char[5];

                                if(stat.Attributes.HasFlag(FileAttributes.Directory))
                                {
                                    folders++;
                                    attr[0] = 'D';
                                }
                                else if(stat.Attributes.HasFlag(FileAttributes.File))
                                {
                                    files++;
                                    attr[0] = 'F';
                                }
                                else
                                {
                                    attr[0] = stat.Attributes.HasFlag(FileAttributes.Alias)   ||
                                              stat.Attributes.HasFlag(FileAttributes.Symlink) ||
                                              stat.Attributes.HasFlag(FileAttributes.Shadow) ? 'L' :
                                              stat.Attributes.HasFlag(FileAttributes.Device) ? 'V' :
                                              stat.Attributes.HasFlag(FileAttributes.Pipe)   ? 'P' : '.';
                                }

                                attr[1] = stat.Attributes.HasFlag(FileAttributes.Archive) ? 'A' : '.';
                                attr[2] = stat.Attributes.HasFlag(FileAttributes.Immutable) ||
                                          stat.Attributes.HasFlag(FileAttributes.ReadOnly)
                                              ? 'R'
                                              : '.';
                                attr[3] = stat.Attributes.HasFlag(FileAttributes.System) ? 'S' : '.';
                                attr[4] = stat.Attributes.HasFlag(FileAttributes.Hidden) ? 'H' : '.';

                                errno = archive.GetCompressedSize(i, out long compressedSize);
                                if(errno != ErrorNumber.NoError)
                                {
                                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                                               "Error {0} getting compressed size for file #{1}.",
                                                               errno, i);
                                    continue;
                                }

                                errno = archive.GetUncompressedSize(i, out long uncompressedSize);
                                if(errno != ErrorNumber.NoError)
                                {
                                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                                               "Error {0} getting uncompressed size for file #{1}.",
                                                               errno, i);
                                    continue;
                                }

                                if(archive.ArchiveFeatures.HasFlag(ArchiveSupportedFeature.SupportsCompression))
                                {
                                    table.AddRow(stat.CreationTime?.ToShortDateString() ?? "",
                                                 stat.CreationTime?.ToLongTimeString()  ?? "", new string(attr),
                                                 uncompressedSize.ToString(), compressedSize.ToString(), fileName);
                                }
                                else
                                {
                                    table.AddRow(stat.CreationTime?.ToShortDateString() ?? "",
                                                 stat.CreationTime?.ToLongTimeString()  ?? "", new string(attr),
                                                 uncompressedSize.ToString(), fileName);
                                }

                                totalSize         += compressedSize;
                                totalUncompressed += uncompressedSize;

                                ctx.Refresh();
                            }

                            table.ShowFooters();
                            table.Columns[0].Footer(inputFilter.CreationTime.ToShortDateString());
                            table.Columns[1].Footer(inputFilter.CreationTime.ToLongTimeString());
                            table.Columns[3].Footer(totalUncompressed.ToString());

                            if(archive.ArchiveFeatures.HasFlag(ArchiveSupportedFeature.SupportsCompression))
                            {
                                table.Columns[4].Footer(totalSize.ToString());
                                table.Columns[5].
                                      Footer(archive.ArchiveFeatures.HasFlag(ArchiveSupportedFeature.
                                                                                 HasExplicitDirectories)
                                                 ? $"{files} files, {folders} folders"
                                                 : $"{files} files");
                            }
                            else
                            {
                                table.Columns[4].
                                      Footer(archive.ArchiveFeatures.HasFlag(ArchiveSupportedFeature.
                                                                                 HasExplicitDirectories)
                                                 ? $"{files} files, {folders} folders"
                                                 : $"{files} files");
                            }

                            table.ShowFooters();
                        });
        }
        catch(Exception ex)
        {
            AaruConsole.ErrorWriteLine(string.Format(UI.Error_reading_file_0, ex.Message));
            AaruConsole.DebugWriteLine(MODULE_NAME, ex.StackTrace);

            return (int)ErrorNumber.UnexpectedException;
        }

        return (int)ErrorNumber.NoError;
    }
}