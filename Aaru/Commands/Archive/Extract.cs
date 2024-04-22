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
using Aaru.Helpers;
using Aaru.Localization;
using Spectre.Console;

namespace Aaru.Commands.Archive;

sealed class ArchiveExtractCommand : Command
{
    const int    BUFFER_SIZE = 16777216;
    const string MODULE_NAME = "Extract-Files command";

    public ArchiveExtractCommand() : base("extract", UI.Archive_Extract_Command_Description)
    {
        AddAlias("x");

        Add(new Option<string>(new[]
        {
            "--encoding", "-e"
        }, () => null, UI.Name_of_character_encoding_to_use));

        Add(new Option<bool>(new[]
        {
            "--xattrs", "-x"
        }, () => false, UI.Extract_extended_attributes_if_present));

        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = UI.Archive_file_path,
            Name        = "archive-path"
        });

        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = UI.Directory_where_extracted_files_will_be_created,
            Name        = "output-dir"
        });

        Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)) ?? throw new NullReferenceException());
    }

    public static int Invoke(bool   debug, bool verbose, string encoding, bool xattrs, string archivePath,
                             string outputDir)
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

            AaruConsole.WriteExceptionEvent += ex => { stderrConsole.WriteException(ex); };
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

        Statistics.AddCommand("archive-extract");

        AaruConsole.DebugWriteLine(MODULE_NAME, "--debug={0}",    debug);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--encoding={0}", Markup.Escape(encoding    ?? ""));
        AaruConsole.DebugWriteLine(MODULE_NAME, "--input={0}",    Markup.Escape(archivePath ?? ""));
        AaruConsole.DebugWriteLine(MODULE_NAME, "--output={0}",   Markup.Escape(outputDir   ?? ""));
        AaruConsole.DebugWriteLine(MODULE_NAME, "--verbose={0}",  verbose);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--xattrs={0}",   xattrs);

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

        try
        {
            IArchive archive = null;

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask(UI.Identifying_archive_format).IsIndeterminate();
                archive = ArchiveFormat.Detect(inputFilter);
            });

            if(archive == null)
            {
                AaruConsole.WriteLine(UI.Archive_format_not_identified_not_proceeding_with_extraction);

                return (int)ErrorNumber.UnrecognizedFormat;
            }

            if(verbose)
                AaruConsole.VerboseWriteLine(UI.Archive_format_identified_by_0_1, archive.Name, archive.Id);
            else
                AaruConsole.WriteLine(UI.Archive_format_identified_by_0, archive.Name);

            try
            {
                ErrorNumber opened = ErrorNumber.NoData;

                Core.Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask(UI.Opening_archive).IsIndeterminate();
                    opened = archive.Open(inputFilter, encodingClass);
                });

                if(opened != ErrorNumber.NoError)
                {
                    AaruConsole.ErrorWriteLine(UI.Unable_to_open_archive_format);
                    AaruConsole.ErrorWriteLine(Localization.Core.Error_0, opened);

                    return (int)opened;
                }

                AaruConsole.DebugWriteLine(MODULE_NAME, UI.Correctly_opened_archive_file);

                // TODO: Implement
                //Statistics.AddArchiveFormat(archive.Name);
                Statistics.AddFilter(inputFilter.Name);
            }
            catch(Exception ex)
            {
                AaruConsole.ErrorWriteLine(UI.Unable_to_open_archive_format);
                AaruConsole.ErrorWriteLine(Localization.Core.Error_0, ex.Message);
                AaruConsole.WriteException(ex);

                return (int)ErrorNumber.CannotOpenFormat;
            }


            for(var i = 0; i < archive.NumberOfEntries; i++)
            {
                ErrorNumber errno = archive.GetFilename(i, out string fileName);

                if(errno != ErrorNumber.NoError)
                {
                    AaruConsole.ErrorWriteLine(UI.Error_0_getting_filename_for_archive_entry_1, errno, i);
                    continue;
                }

                errno = archive.Stat(i, out FileEntryInfo stat);
                if(errno != ErrorNumber.NoError)
                {
                    AaruConsole.ErrorWriteLine(UI.Error_0_retrieving_stat_for_archive_entry_1, errno, i);
                    continue;
                }

                errno = archive.GetUncompressedSize(i, out long uncompressedSize);
                if(errno != ErrorNumber.NoError)
                {
                    AaruConsole.ErrorWriteLine(UI.Error_0_getting_uncompressed_size_for_archive_entry_1, errno, i);
                    continue;
                }

                errno = archive.GetEntry(i, out IFilter filter);
                if(errno != ErrorNumber.NoError)
                {
                    AaruConsole.ErrorWriteLine(UI.Error_0_getting_filter_for_archive_entry_1, errno, i);
                    continue;
                }

                if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    fileName = fileName.Replace('<', '\uFF1C').
                                        Replace('>',  '\uFF1E').
                                        Replace(':',  '\uFF1A').
                                        Replace('\"', '\uFF02').
                                        Replace('|',  '\uFF5C').
                                        Replace('?',  '\uFF1F').
                                        Replace('*',  '\uFF0A').
                                        Replace('/',  '\\');
                }

                // Prevent absolute path attack
                fileName = fileName.TrimStart('\\').TrimStart('/');

                string outputPath     = Path.Combine(outputDir, fileName);
                string destinationDir = Path.GetDirectoryName(outputPath);

                if(File.Exists(destinationDir))
                {
                    AaruConsole.ErrorWriteLine(UI.Cannot_write_file_0_output_exists, Markup.Escape(fileName));
                    continue;
                }

                if(destinationDir is not null)
                    Directory.CreateDirectory(destinationDir);

                if(!File.Exists(outputPath) && !Directory.Exists(outputPath))
                {
                    AnsiConsole.Progress().
                                AutoClear(true).
                                HideCompleted(true).
                                Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn()).
                                Start(ctx =>
                                {
                                    var position = 0;

                                    var outputFile = new FileStream(outputPath, FileMode.CreateNew,
                                                                    FileAccess.ReadWrite, FileShare.None);

                                    ProgressTask task =
                                        ctx.AddTask(string.Format(UI.Reading_file_0, Markup.Escape(fileName)));

                                    task.MaxValue = uncompressedSize;
                                    var    outBuf    = new byte[BUFFER_SIZE];
                                    Stream inputFile = filter.GetDataForkStream();

                                    while(position < stat.Length)
                                    {
                                        int bytesToRead;

                                        if(stat.Length - position > BUFFER_SIZE)
                                            bytesToRead = BUFFER_SIZE;
                                        else
                                            bytesToRead = (int)(stat.Length - position);

                                        int bytesRead = inputFile.EnsureRead(outBuf, 0, bytesToRead);

                                        outputFile.Write(outBuf, 0, bytesRead);

                                        position += bytesToRead;
                                        task.Increment(bytesToRead);
                                    }

                                    inputFile.Close();
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
                    AaruConsole.WriteLine(UI.Written_0_bytes_of_file_1_to_2, uncompressedSize, Markup.Escape(fileName),
                                          Markup.Escape(outputPath));
                }
                else
                    AaruConsole.ErrorWriteLine(UI.Cannot_write_file_0_output_exists, Markup.Escape(fileName));

                if(!xattrs)
                    continue;

                errno = archive.ListXAttr(i, out List<string> xattrNames);

                if(errno != ErrorNumber.NoError)
                {
                    AaruConsole.ErrorWriteLine(UI.Error_0_listing_extended_attributes_for_archive_entry_1, errno, i);
                    continue;
                }

                foreach(string xattrName in xattrNames)
                {
                    byte[] xattrBuffer = Array.Empty<byte>();
                    Core.Spectre.ProgressSingleSpinner(ctx =>
                    {
                        ctx.AddTask(UI.Reading_extended_attribute).IsIndeterminate();
                        errno = archive.GetXattr(i, xattrName, ref xattrBuffer);
                    });

                    if(errno != ErrorNumber.NoError)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   UI.Error_0_reading_extended_attribute_1_for_archive_entry_2, errno,
                                                   xattrName, i);
                        continue;
                    }

                    outputPath = Path.Combine(outputDir, ".xattrs", xattrName, fileName);

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

                    destinationDir = Path.GetDirectoryName(outputPath);
                    if(destinationDir is not null)
                        Directory.CreateDirectory(destinationDir);

                    if(!File.Exists(outputPath) && !Directory.Exists(outputPath))
                    {
                        Core.Spectre.ProgressSingleSpinner(ctx =>
                        {
                            ctx.AddTask(UI.Writing_extended_attribute).IsIndeterminate();

                            var outputFile = new FileStream(outputPath, FileMode.CreateNew,
                                                            FileAccess.ReadWrite, FileShare.None);

                            outputFile.Write(xattrBuffer, 0, xattrBuffer.Length);

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
                            AaruConsole.WriteLine(UI.Written_0_bytes_of_file_1_to_2, uncompressedSize,
                                                  Markup.Escape(fileName), Markup.Escape(outputPath));
                        });
                    }
                    else
                        AaruConsole.ErrorWriteLine(UI.Cannot_write_file_0_output_exists, Markup.Escape(fileName));
                }
            }
        }
        catch(Exception ex)
        {
            AaruConsole.ErrorWriteLine(string.Format(UI.Error_reading_file_0, Markup.Escape(ex.Message)));
            AaruConsole.WriteException(ex);

            return (int)ErrorNumber.UnexpectedException;
        }

        return (int)ErrorNumber.NoError;
    }
}