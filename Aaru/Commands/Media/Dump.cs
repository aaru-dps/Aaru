// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dump.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'dump' command.
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
// Copyright © 2020-2023 Rebecca Wallander
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Xml.Serialization;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Interop;
using Aaru.CommonTypes.Metadata;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Console;
using Aaru.Core;
using Aaru.Core.Devices.Dumping;
using Aaru.Core.Logging;
using Aaru.Localization;
using Schemas;
using Spectre.Console;
using Dump = Aaru.Core.Devices.Dumping.Dump;
using File = System.IO.File;

namespace Aaru.Commands.Media;

// TODO: Add raw dumping
sealed class DumpMediaCommand : Command
{
    const  string       MODULE_NAME = "Dump-Media command";
    static ProgressTask _progressTask1;
    static ProgressTask _progressTask2;

    public DumpMediaCommand() : base("dump", UI.Media_Dump_Command_Description)
    {
        Add(new Option<string>(new[]
        {
            "--cicm-xml", "-x"
        }, () => null, UI.Take_metadata_from_existing_CICM_XML_sidecar));

        Add(new Option<string>(new[]
        {
            "--encoding", "-e"
        }, () => null, UI.Name_of_character_encoding_to_use));

        Add(new Option<bool>("--first-pregap", () => false, UI.Try_to_read_first_track_pregap));

        Add(new Option<bool>("--fix-offset", () => true, UI.Fix_audio_tracks_offset));

        Add(new Option<bool>(new[]
        {
            "--force", "-f"
        }, () => false, UI.Continue_dumping_whatever_happens));

        Add(new Option<string>(new[]
        {
            "--format", "-t"
        }, () => null, UI.Format_of_the_output_image_as_plugin_name_or_plugin_id));

        Add(new Option<bool>("--metadata", () => true, UI.Enables_creating_Aaru_Metadata_sidecar));

        Add(new Option<bool>("--trim", () => true, UI.Enables_trimming_errored_from_skipped_sectors));

        Add(new Option<string>(new[]
        {
            "--options", "-O"
        }, () => null, UI.Comma_separated_name_value_pairs_of_image_options));

        Add(new Option<bool>("--persistent", () => false, UI.Try_to_recover_partial_or_incorrect_data));

        Add(new Option<bool>(new[]
        {
            "--resume", "-r"
        }, () => true, UI.Create_or_use_resume_mapfile));

        Add(new Option<ushort>(new[]
        {
            "--retry-passes", "-p"
        }, () => 5, UI.How_many_retry_passes_to_do));

        Add(new Option<uint>(new[]
        {
            "--skip", "-k"
        }, () => 512, UI.When_an_unreadable_sector_is_found_skip_this_many_sectors));

        Add(new Option<bool>(new[]
        {
            "--stop-on-error", "-s"
        }, () => false, UI.Stop_media_dump_on_first_error));

        Add(new Option<string>("--subchannel", () => UI.Subchannel_name_any, UI.Subchannel_to_dump_help));

        Add(new Option<byte>("--speed", () => 0, UI.Speed_to_dump));

        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = UI.Device_path,
            Name        = "device-path"
        });

        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = UI.Output_image_path_Dump_help,
            Name        = "output-path"
        });

        Add(new Option<bool>(new[]
        {
            "--private"
        }, () => false, UI.Do_not_store_paths_and_serial_numbers_in_log_or_metadata));

        Add(new Option<bool>(new[]
        {
            "--fix-subchannel-position"
        }, () => true, UI.Fix_subchannel_position_help));

        Add(new Option<bool>(new[]
        {
            "--retry-subchannel"
        }, () => true, UI.Retry_subchannel_help));

        Add(new Option<bool>(new[]
        {
            "--fix-subchannel"
        }, () => false, UI.Fix_subchannel_help));

        Add(new Option<bool>(new[]
        {
            "--fix-subchannel-crc"
        }, () => false, UI.Fix_subchannel_crc_help));

        Add(new Option<bool>(new[]
        {
            "--generate-subchannels"
        }, () => false, UI.Generate_subchannels_dump_help));

        Add(new Option<bool>(new[]
        {
            "--skip-cdiready-hole"
        }, () => true, UI.Skip_CDi_Ready_hole_help));

        Add(new Option<bool>(new[]
        {
            "--eject"
        }, () => false, UI.Eject_media_after_dump_finishes));

        Add(new Option<uint>(new[]
        {
            "--max-blocks"
        }, () => 64, UI.Maximum_number_of_blocks_to_read_at_once));

        Add(new Option<bool>(new[]
        {
            "--use-buffered-reads"
        }, () => true, UI.OS_buffered_reads_help));

        Add(new Option<bool>(new[]
        {
            "--store-encrypted"
        }, () => true, UI.Store_encrypted_data_as_is));

        Add(new Option<bool>(new[]
        {
            "--title-keys"
        }, () => true, UI.Try_to_read_the_title_keys_from_CSS_DVDs));

        Add(new Option<uint>(new[]
        {
            "--ignore-cdr-runouts"
        }, () => 10, UI.How_many_CDRW_run_out_sectors_to_ignore_and_regenerate));

        Add(new Option<bool>(new[]
        {
            "--create-graph", "-g"
        }, () => true, UI.Create_graph_of_dumped_media));

        Add(new Option<uint>(new[]
        {
            "--dimensions"
        }, () => 1080, UI.Dump_graph_dimensions_argument_help));

        Add(new Option<string>(new[]
        {
            "--aaru-metadata", "-m"
        }, () => null, "Take metadata from existing Aaru Metadata sidecar."));

        Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)) ?? throw new NullReferenceException());
    }

    public static int Invoke(bool debug, bool verbose, string cicmXml, string devicePath, bool resume, string encoding,
                             bool firstPregap, bool fixOffset, bool force, bool metadata, bool trim, string outputPath,
                             string options, bool persistent, ushort retryPasses, uint skip, byte speed,
                             bool stopOnError, string format, string subchannel, bool @private,
                             bool fixSubchannelPosition, bool retrySubchannel, bool fixSubchannel,
                             bool fixSubchannelCrc, bool generateSubchannels, bool skipCdiReadyHole, bool eject,
                             uint maxBlocks, bool useBufferedReads, bool storeEncrypted, bool titleKeys,
                             uint ignoreCdrRunOuts, bool createGraph, uint dimensions, string aaruMetadata)
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

        fixSubchannel         |= fixSubchannelCrc;
        fixSubchannelPosition |= retrySubchannel || fixSubchannel;

        if(maxBlocks == 0)
            maxBlocks = 64;

        Statistics.AddCommand("dump-media");

        AaruConsole.DebugWriteLine(MODULE_NAME, "--cicm-xml={0}",                cicmXml);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--debug={0}",                   debug);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--device={0}",                  devicePath);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--encoding={0}",                encoding);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--first-pregap={0}",            firstPregap);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--fix-offset={0}",              fixOffset);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--force={0}",                   force);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--format={0}",                  format);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--metadata={0}",                metadata);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--options={0}",                 options);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--output={0}",                  outputPath);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--persistent={0}",              persistent);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--resume={0}",                  resume);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--retry-passes={0}",            retryPasses);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--skip={0}",                    skip);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--stop-on-error={0}",           stopOnError);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--trim={0}",                    trim);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--verbose={0}",                 verbose);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--subchannel={0}",              subchannel);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--private={0}",                 @private);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--fix-subchannel-position={0}", fixSubchannelPosition);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--retry-subchannel={0}",        retrySubchannel);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--fix-subchannel={0}",          fixSubchannel);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--fix-subchannel-crc={0}",      fixSubchannelCrc);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--generate-subchannels={0}",    generateSubchannels);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--skip-cdiready-hole={0}",      skipCdiReadyHole);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--eject={0}",                   eject);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--max-blocks={0}",              maxBlocks);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--use-buffered-reads={0}",      useBufferedReads);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--store-encrypted={0}",         storeEncrypted);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--title-keys={0}",              titleKeys);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--ignore-cdr-runouts={0}",      ignoreCdrRunOuts);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--create-graph={0}",            createGraph);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--dimensions={0}",              dimensions);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--aaru-metadata={0}",           aaruMetadata);

        // TODO: Disabled temporarily
        //AaruConsole.DebugWriteLine(MODULE_NAME, "--raw={0}", raw);

        Dictionary<string, string> parsedOptions = Core.Options.Parse(options);
        AaruConsole.DebugWriteLine(MODULE_NAME, UI.Parsed_options);

        foreach(KeyValuePair<string, string> parsedOption in parsedOptions)
            AaruConsole.DebugWriteLine(MODULE_NAME, "{0} = {1}", parsedOption.Key, parsedOption.Value);

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

        DumpSubchannel wantedSubchannel = DumpSubchannel.Any;

        if(subchannel?.ToLower(CultureInfo.CurrentUICulture) == UI.Subchannel_name_any || subchannel is null)
            wantedSubchannel = DumpSubchannel.Any;
        else if(subchannel?.ToLowerInvariant() == UI.Subchannel_name_rw)
            wantedSubchannel = DumpSubchannel.Rw;
        else if(subchannel?.ToLowerInvariant() == UI.Subchannel_name_rw_or_pq)
            wantedSubchannel = DumpSubchannel.RwOrPq;
        else if(subchannel?.ToLowerInvariant() == UI.Subchannel_name_pq)
            wantedSubchannel = DumpSubchannel.Pq;
        else if(subchannel?.ToLowerInvariant() == UI.Subchannel_name_none)
            wantedSubchannel = DumpSubchannel.None;
        else
            AaruConsole.WriteLine(UI.Incorrect_subchannel_type_0_requested, subchannel);

        string filename = Path.GetFileNameWithoutExtension(outputPath);

        bool isResponse = filename.StartsWith("#", StringComparison.OrdinalIgnoreCase) &&
                          File.Exists(Path.Combine(Path.GetDirectoryName(outputPath),
                                                   Path.GetFileNameWithoutExtension(outputPath)));

        TextReader resReader;

        if(isResponse)
        {
            resReader = new StreamReader(Path.Combine(Path.GetDirectoryName(outputPath),
                                                      Path.GetFileNameWithoutExtension(outputPath)));
        }
        else
            resReader = new StringReader(Path.GetFileNameWithoutExtension(outputPath));

        if(isResponse)
            eject = true;

        PluginRegister           plugins    = PluginRegister.Singleton;
        List<IBaseWritableImage> candidates = new();
        string                   extension  = Path.GetExtension(outputPath);

        // Try extension
        if(string.IsNullOrEmpty(format))
        {
            candidates.AddRange(from plugin in plugins.WritableImages.Values
                                where plugin is not null
                                where plugin.KnownExtensions.Contains(extension)
                                select plugin);
        }

        // Try Id
        else if(Guid.TryParse(format, out Guid outId))
        {
            candidates.AddRange(from plugin in plugins.WritableImages.Values
                                where plugin is not null
                                where plugin.Id.Equals(outId)
                                select plugin);
        }

        // Try name
        else
        {
            candidates.AddRange(from plugin in plugins.WritableImages.Values
                                where plugin is not null
                                where plugin.Name.Equals(format, StringComparison.InvariantCultureIgnoreCase)
                                select plugin);
        }

        switch(candidates.Count)
        {
            case 0:
                AaruConsole.WriteLine(UI.No_plugin_supports_requested_extension);

                return (int)ErrorNumber.FormatNotFound;
            case > 1:
                AaruConsole.WriteLine(UI.More_than_one_plugin_supports_requested_extension);

                return (int)ErrorNumber.TooManyFormats;
        }

        while(true)
        {
            string responseLine = resReader.ReadLine();

            if(responseLine is null)
                break;

            if(responseLine.Any(c => c < 0x20))
            {
                AaruConsole.ErrorWriteLine(UI.Invalid_characters_found_in_list_of_files);

                return (int)ErrorNumber.InvalidArgument;
            }

            if(isResponse)
            {
                AaruConsole.WriteLine(UI.Please_insert_media_with_title_0_and_press_any_key_to_continue_, responseLine);

                System.Console.ReadKey();
                Thread.Sleep(1000);
            }

            responseLine = responseLine.Replace('/', '／');

            // Replace Windows forbidden filename characters with Japanese equivalents that are visually the same, but bigger.
            if(DetectOS.IsWindows)
            {
                responseLine = responseLine.Replace('<', '\uFF1C').
                                            Replace('>',  '\uFF1E').
                                            Replace(':',  '\uFF1A').
                                            Replace('"',  '\u2033').
                                            Replace('\\', '＼').
                                            Replace('|',  '｜').
                                            Replace('?',  '？').
                                            Replace('*',  '＊');
            }

            if(devicePath.Length == 2 && devicePath[1] == ':' && devicePath[0] != '/' && char.IsLetter(devicePath[0]))
                devicePath = "\\\\.\\" + char.ToUpper(devicePath[0]) + ':';

            Devices.Device dev      = null;
            ErrorNumber    devErrno = ErrorNumber.NoError;

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask(UI.Opening_device).IsIndeterminate();
                dev = Devices.Device.Create(devicePath, out devErrno);
            });

            switch(dev)
            {
                case null:
                {
                    AaruConsole.ErrorWriteLine(string.Format(UI.Could_not_open_device_error_0, devErrno));

                    if(isResponse)
                        continue;

                    return (int)devErrno;
                }
                case Devices.Remote.Device remoteDev:
                    Statistics.AddRemote(remoteDev.RemoteApplication, remoteDev.RemoteVersion,
                                         remoteDev.RemoteOperatingSystem, remoteDev.RemoteOperatingSystemVersion,
                                         remoteDev.RemoteArchitecture);

                    break;
            }

            if(dev.Error)
            {
                AaruConsole.ErrorWriteLine(Error.Print(dev.LastError));

                if(isResponse)
                    continue;

                return (int)ErrorNumber.CannotOpenDevice;
            }

            Statistics.AddDevice(dev);

            string outputPrefix = Path.Combine(Path.GetDirectoryName(outputPath), responseLine);

            Resume resumeClass = null;

            if(resume)
            {
                try
                {
                    if(File.Exists(outputPrefix + ".resume.json"))
                    {
                        var fs = new FileStream(outputPrefix + ".resume.json", FileMode.Open);

                        resumeClass =
                            (JsonSerializer.Deserialize(fs, typeof(ResumeJson),
                                                        ResumeJsonContext.Default) as ResumeJson)?.Resume;

                        fs.Close();
                    }

                    // DEPRECATED: To be removed in Aaru 7
                    else if(File.Exists(outputPrefix + ".resume.xml") && resume)
                    {
                        // Should be covered by virtue of being the same exact class as the JSON above
                    #pragma warning disable IL2026
                        var xs = new XmlSerializer(typeof(Resume));
                    #pragma warning restore IL2026

                        var sr = new StreamReader(outputPrefix + ".resume.xml");

                        // Should be covered by virtue of being the same exact class as the JSON above
                    #pragma warning disable IL2026
                        resumeClass = (Resume)xs.Deserialize(sr);
                    #pragma warning restore IL2026

                        sr.Close();
                    }
                }
                catch
                {
                    AaruConsole.ErrorWriteLine(UI.Incorrect_resume_file_not_continuing);

                    if(isResponse)
                        continue;

                    return (int)ErrorNumber.InvalidResume;
                }
            }

            if(resumeClass                 != null                                               &&
               resumeClass.NextBlock       > resumeClass.LastBlock                               &&
               resumeClass.BadBlocks.Count == 0                                                  &&
               !resumeClass.Tape                                                                 &&
               (resumeClass.BadSubchannels is null   || resumeClass.BadSubchannels.Count   == 0) &&
               (resumeClass.MissingTitleKeys is null || resumeClass.MissingTitleKeys.Count == 0))
            {
                AaruConsole.WriteLine(UI.Media_already_dumped_correctly_not_continuing);

                if(isResponse)
                    continue;

                return (int)ErrorNumber.AlreadyDumped;
            }

            Metadata sidecar = null;

            if(aaruMetadata != null)
            {
                if(File.Exists(aaruMetadata))
                {
                    try
                    {
                        var fs = new FileStream(aaruMetadata, FileMode.Open);

                        sidecar =
                            (JsonSerializer.Deserialize(fs, typeof(MetadataJson), MetadataJsonContext.Default) as
                                 MetadataJson)?.AaruMetadata;

                        fs.Close();
                    }
                    catch
                    {
                        AaruConsole.ErrorWriteLine(UI.Incorrect_metadata_sidecar_file_not_continuing);

                        if(isResponse)
                            continue;

                        return (int)ErrorNumber.InvalidSidecar;
                    }
                }
                else
                {
                    AaruConsole.ErrorWriteLine(UI.Could_not_find_metadata_sidecar);

                    if(isResponse)
                        continue;

                    return (int)ErrorNumber.NoSuchFile;
                }
            }
            else if(cicmXml != null)
            {
                if(File.Exists(cicmXml))
                {
                    try
                    {
                        var sr = new StreamReader(cicmXml);

                        // Bypassed by JSON source generator used above
                    #pragma warning disable IL2026
                        var sidecarXs = new XmlSerializer(typeof(CICMMetadataType));
                    #pragma warning restore IL2026

                        // Bypassed by JSON source generator used above
                    #pragma warning disable IL2026
                        sidecar = (CICMMetadataType)sidecarXs.Deserialize(sr);
                    #pragma warning restore IL2026

                        sr.Close();
                    }
                    catch
                    {
                        AaruConsole.ErrorWriteLine(UI.Incorrect_metadata_sidecar_file_not_continuing);

                        if(isResponse)
                            continue;

                        return (int)ErrorNumber.InvalidSidecar;
                    }
                }
                else
                {
                    AaruConsole.ErrorWriteLine(UI.Could_not_find_metadata_sidecar);

                    if(isResponse)
                        continue;

                    return (int)ErrorNumber.NoSuchFile;
                }
            }

            plugins    = PluginRegister.Singleton;
            candidates = new List<IBaseWritableImage>();

            // Try extension
            if(string.IsNullOrEmpty(format))
            {
                candidates.AddRange(from plugin in plugins.WritableImages.Values
                                    where plugin is not null
                                    where plugin.KnownExtensions.Contains(Path.GetExtension(outputPath))
                                    select plugin);
            }

            // Try Id
            else if(Guid.TryParse(format, out Guid outId))
            {
                candidates.AddRange(from plugin in plugins.WritableImages.Values
                                    where plugin is not null
                                    where plugin.Id.Equals(outId)
                                    select plugin);
            }

            // Try name
            else
            {
                candidates.AddRange(from plugin in plugins.WritableImages.Values
                                    where plugin is not null
                                    where plugin.Name.Equals(format, StringComparison.InvariantCultureIgnoreCase)
                                    select plugin);
            }

            IBaseWritableImage outputFormat = candidates[0];

            var dumpLog = new DumpLog(outputPrefix + ".log", dev, @private);

            if(verbose)
            {
                dumpLog.WriteLine(UI.Output_image_format_0_1, outputFormat.Name, outputFormat.Id);
                AaruConsole.VerboseWriteLine(UI.Output_image_format_0_1, outputFormat.Name, outputFormat.Id);
            }
            else
            {
                dumpLog.WriteLine(UI.Output_image_format_0, outputFormat.Name);
                AaruConsole.WriteLine(UI.Output_image_format_0, outputFormat.Name);
            }

            var errorLog = new ErrorLog(outputPrefix + ".error.log");

            var dumper = new Dump(resume, dev, devicePath, outputFormat, retryPasses, force, false, persistent,
                                  stopOnError, resumeClass, dumpLog, encodingClass, outputPrefix,
                                  outputPrefix + extension, parsedOptions, sidecar, skip, metadata, trim, firstPregap,
                                  fixOffset, debug, wantedSubchannel, speed, @private, fixSubchannelPosition,
                                  retrySubchannel, fixSubchannel, fixSubchannelCrc, skipCdiReadyHole, errorLog,
                                  generateSubchannels, maxBlocks, useBufferedReads, storeEncrypted, titleKeys,
                                  ignoreCdrRunOuts, createGraph, dimensions);

            AnsiConsole.Progress().
                        AutoClear(true).
                        HideCompleted(true).
                        Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn()).
                        Start(ctx =>
                        {
                            dumper.UpdateStatus += text => { AaruConsole.WriteLine(Markup.Escape(text)); };

                            dumper.ErrorMessage += text =>
                                {
                                    AaruConsole.ErrorWriteLine($"[red]{Markup.Escape(text)}[/]");
                                };

                            dumper.StoppingErrorMessage += text =>
                            {
                                AaruConsole.ErrorWriteLine($"[red]{Markup.Escape(text)}[/]");
                            };

                            dumper.UpdateProgress += (text, current, maximum) =>
                            {
                                _progressTask1             ??= ctx.AddTask("Progress");
                                _progressTask1.Description =   Markup.Escape(text);
                                _progressTask1.Value       =   current;
                                _progressTask1.MaxValue    =   maximum;
                            };

                            dumper.PulseProgress += text =>
                            {
                                if(_progressTask1 is null)
                                    ctx.AddTask(Markup.Escape(text)).IsIndeterminate();
                                else
                                {
                                    _progressTask1.Description     = Markup.Escape(text);
                                    _progressTask1.IsIndeterminate = true;
                                }
                            };

                            dumper.InitProgress += () => { _progressTask1 = ctx.AddTask("Progress"); };

                            dumper.EndProgress += () =>
                            {
                                _progressTask1?.StopTask();
                                _progressTask1 = null;
                            };

                            dumper.InitProgress2 += () => { _progressTask2 = ctx.AddTask("Progress"); };

                            dumper.EndProgress2 += () =>
                            {
                                _progressTask2?.StopTask();
                                _progressTask2 = null;
                            };

                            dumper.UpdateProgress2 += (text, current, maximum) =>
                            {
                                _progressTask2             ??= ctx.AddTask("Progress");
                                _progressTask2.Description =   Markup.Escape(text);
                                _progressTask2.Value       =   current;
                                _progressTask2.MaxValue    =   maximum;
                            };

                            System.Console.CancelKeyPress += (_, e) =>
                            {
                                e.Cancel = true;
                                dumper.Abort();
                            };

                            dumper.Start();
                        });

            if(eject && dev.IsRemovable)
            {
                Core.Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask(UI.Ejecting_media).IsIndeterminate();

                    switch(dev.Type)
                    {
                        case DeviceType.ATA:
                            dev.DoorUnlock(out _, dev.Timeout, out _);
                            dev.MediaEject(out _, dev.Timeout, out _);

                            break;
                        case DeviceType.ATAPI:
                        case DeviceType.SCSI:
                            switch(dev.ScsiType)
                            {
                                case PeripheralDeviceTypes.DirectAccess:
                                case PeripheralDeviceTypes.SimplifiedDevice:
                                case PeripheralDeviceTypes.SCSIZonedBlockDevice:
                                case PeripheralDeviceTypes.WriteOnceDevice:
                                case PeripheralDeviceTypes.OpticalDevice:
                                case PeripheralDeviceTypes.OCRWDevice:
                                    dev.SpcAllowMediumRemoval(out _, dev.Timeout, out _);
                                    dev.EjectTray(out _, dev.Timeout, out _);

                                    break;
                                case PeripheralDeviceTypes.MultiMediaDevice:
                                    dev.AllowMediumRemoval(out _, dev.Timeout, out _);
                                    dev.EjectTray(out _, dev.Timeout, out _);

                                    break;
                                case PeripheralDeviceTypes.SequentialAccess:
                                    dev.SpcAllowMediumRemoval(out _, dev.Timeout, out _);
                                    dev.LoadUnload(out _, true, false, false, false, false, dev.Timeout, out _);

                                    break;
                            }

                            break;
                    }
                });
            }

            dev.Close();
        }

        return (int)ErrorNumber.NoError;
    }
}