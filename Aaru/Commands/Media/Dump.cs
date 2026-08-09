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
// Copyright © 2011-2026 Natalia Portillo
// Copyright © 2020-2026 Rebecca Wallander
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Aaru.Core;
using Aaru.Core.Devices.Dumping;
using Aaru.Core.Logging;
using Aaru.Images;
using Aaru.Localization;
using Aaru.Logging;
using Schemas;
using Sentry;
using Spectre.Console;
using Spectre.Console.Cli;
using Dump = Aaru.Core.Devices.Dumping.Dump;
using File = System.IO.File;

namespace Aaru.Commands.Media;

// TODO: Add raw dumping
sealed class DumpMediaCommand : Command<DumpMediaCommand.Settings>
{
    const  string       MODULE_NAME = "Dump-Media command";
    static ProgressTask _progressTask1;
    static ProgressTask _progressTask2;

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        MainClass.PrintCopyright();

        bool fixSubchannel         = settings.FixSubchannel;
        bool fixSubchannelCrc      = settings.FixSubchannelCrc;
        bool fixSubchannelPosition = settings.FixSubchannelPosition;
        var  maxBlocks             = (uint)settings.MaxBlocks;
        bool eject                 = settings.Eject;

        fixSubchannel         |= fixSubchannelCrc;
        fixSubchannelPosition |= settings.RetrySubchannel || fixSubchannel;

        if(maxBlocks == 0) maxBlocks = 64;

        Statistics.AddCommand("dump-media");

        AaruLogging.Debug(MODULE_NAME, "--cicm-xml={0}",                Markup.Escape(settings.CicmXml ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--debug={0}",                   settings.Debug);
        AaruLogging.Debug(MODULE_NAME, "--device={0}",                  Markup.Escape(settings.DevicePath ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--encoding={0}",                Markup.Escape(settings.Encoding   ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--first-pregap={0}",            settings.FirstPregap);
        AaruLogging.Debug(MODULE_NAME, "--fix-offset={0}",              settings.FixOffset);
        AaruLogging.Debug(MODULE_NAME, "--force={0}",                   settings.Force);
        AaruLogging.Debug(MODULE_NAME, "--format={0}",                  Markup.Escape(settings.Format ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--metadata={0}",                settings.Metadata);
        AaruLogging.Debug(MODULE_NAME, "--options={0}",                 Markup.Escape(settings.Options    ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--output={0}",                  Markup.Escape(settings.OutputPath ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--persistent={0}",              settings.Persistent);
        AaruLogging.Debug(MODULE_NAME, "--resume={0}",                  settings.Resume);
        AaruLogging.Debug(MODULE_NAME, "--retry-passes={0}",            settings.RetryPasses);
        AaruLogging.Debug(MODULE_NAME, "--skip={0}",                    settings.Skip);
        AaruLogging.Debug(MODULE_NAME, "--stop-on-error={0}",           settings.StopOnError);
        AaruLogging.Debug(MODULE_NAME, "--trim={0}",                    settings.Trim);
        AaruLogging.Debug(MODULE_NAME, "--verbose={0}",                 settings.Verbose);
        AaruLogging.Debug(MODULE_NAME, "--subchannel={0}",              Markup.Escape(settings.Subchannel ?? ""));
        AaruLogging.Debug(MODULE_NAME, "----private={0}",               settings.Private);
        AaruLogging.Debug(MODULE_NAME, "--fix-subchannel-position={0}", settings.FixSubchannelPosition);
        AaruLogging.Debug(MODULE_NAME, "--retry-subchannel={0}",        settings.RetrySubchannel);
        AaruLogging.Debug(MODULE_NAME, "--fix-subchannel={0}",          fixSubchannel);
        AaruLogging.Debug(MODULE_NAME, "--fix-subchannel-crc={0}",      fixSubchannelCrc);
        AaruLogging.Debug(MODULE_NAME, "--generate-subchannels={0}",    settings.GenerateSubchannels);
        AaruLogging.Debug(MODULE_NAME, "--skip-cdiready-hole={0}",      settings.SkipCdiReadyHole);
        AaruLogging.Debug(MODULE_NAME, "--eject={0}",                   eject);
        AaruLogging.Debug(MODULE_NAME, "--max-blocks={0}",              maxBlocks);
        AaruLogging.Debug(MODULE_NAME, "--use-buffered-reads={0}",      settings.UseBufferedReads);
        AaruLogging.Debug(MODULE_NAME, "--store-encrypted={0}",         settings.StoreEncrypted);
        AaruLogging.Debug(MODULE_NAME, "--bypass-wii-decryption={0}",   settings.BypassWiiDecryption);
        AaruLogging.Debug(MODULE_NAME, "--title-keys={0}",              settings.TitleKeys);
        AaruLogging.Debug(MODULE_NAME, "--ignore-cdr-runouts={0}",      settings.IgnoreCdrRunOuts);
        AaruLogging.Debug(MODULE_NAME, "--create-graph={0}",            settings.CreateGraph);
        AaruLogging.Debug(MODULE_NAME, "--dimensions={0}",              settings.Dimensions);
        AaruLogging.Debug(MODULE_NAME, "--aaru-metadata={0}",           Markup.Escape(settings.AaruMetadata ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--c2-repair={0}",               settings.C2Repair);
        AaruLogging.Debug(MODULE_NAME, "--paranoia={0}",                settings.Paranoia);
        AaruLogging.Debug(MODULE_NAME, "--cure-paranoia={0}",           settings.CureParanoia);
        AaruLogging.Debug(MODULE_NAME, "--raw={0}",                     settings.Raw);
        AaruLogging.Debug(MODULE_NAME, "--start-reverse={0}",           settings.StartReverse);
        AaruLogging.Debug(MODULE_NAME, "--error-recovery={0}",          settings.ErrorRecovery);
        AaruLogging.Debug(MODULE_NAME, "--hyper-speed={0}",             settings.HyperSpeed);
        AaruLogging.Debug(MODULE_NAME, "--ludicrous-speed={0}",         settings.LudicrousSpeed);
        AaruLogging.Debug(MODULE_NAME, "--lead-out={0}",                settings.LeadOut);
        AaruLogging.Debug(MODULE_NAME, "--skip-safedisc={0}",           settings.SkipSafeDisc);

        Dictionary<string, string> parsedOptions = Options.Parse(settings.Options);
        AaruLogging.Debug(MODULE_NAME, UI.Parsed_options);

        foreach(KeyValuePair<string, string> parsedOption in parsedOptions)
            AaruLogging.Debug(MODULE_NAME, "{0} = {1}", parsedOption.Key, parsedOption.Value);

        Encoding encodingClass = null;

        if(settings.Encoding != null)
        {
            try
            {
                encodingClass = Claunia.Encoding.Encoding.GetEncoding(settings.Encoding);

                if(settings.Verbose) AaruLogging.Verbose(UI.encoding_for_0, encodingClass.EncodingName);
            }
            catch(ArgumentException)
            {
                AaruLogging.Error(UI.Specified_encoding_is_not_supported);

                return (int)ErrorNumber.EncodingUnknown;
            }
        }

        DumpSubchannel wantedSubchannel = DumpSubchannel.Any;

        if(settings.Subchannel?.ToLower(CultureInfo.CurrentUICulture) == UI.Subchannel_name_any ||
           settings.Subchannel is null)
            wantedSubchannel = DumpSubchannel.Any;
        else if(settings.Subchannel?.ToLowerInvariant() == UI.Subchannel_name_rw)
            wantedSubchannel = DumpSubchannel.Rw;
        else if(settings.Subchannel?.ToLowerInvariant() == UI.Subchannel_name_rw_or_pq)
            wantedSubchannel = DumpSubchannel.RwOrPq;
        else if(settings.Subchannel?.ToLowerInvariant() == UI.Subchannel_name_pq)
            wantedSubchannel = DumpSubchannel.Pq;
        else if(settings.Subchannel?.ToLowerInvariant() == UI.Subchannel_name_none)
            wantedSubchannel = DumpSubchannel.None;
        else
            AaruLogging.WriteLine(UI.Incorrect_subchannel_type_0_requested, settings.Subchannel);

        string filename = Path.GetFileNameWithoutExtension(settings.OutputPath);

        bool isResponse = filename.StartsWith("#", StringComparison.OrdinalIgnoreCase) &&
                          File.Exists(Path.Combine(Path.GetDirectoryName(settings.OutputPath),
                                                   Path.GetFileNameWithoutExtension(settings.OutputPath)));

        TextReader resReader;

        if(isResponse)
        {
            resReader = new StreamReader(Path.Combine(Path.GetDirectoryName(settings.OutputPath),
                                                      Path.GetFileNameWithoutExtension(settings.OutputPath)));
        }
        else
            resReader = new StringReader(Path.GetFileNameWithoutExtension(settings.OutputPath));

        if(isResponse) eject = true;

        PluginRegister           plugins    = PluginRegister.Singleton;
        List<IBaseWritableImage> candidates = [];
        string                   extension  = Path.GetExtension(settings.OutputPath);

        // Try extension
        if(string.IsNullOrEmpty(settings.Format))
        {
            candidates.AddRange(from plugin in plugins.WritableImages.Values
                                where plugin is not null
                                where plugin.KnownExtensions.Contains(extension)
                                select plugin);
        }

        // Try Id
        else if(Guid.TryParse(settings.Format, out Guid outId))
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
                                where plugin.Name.Equals(settings.Format, StringComparison.InvariantCultureIgnoreCase)
                                select plugin);
        }

        switch(candidates.Count)
        {
            case 0:
                AaruLogging.Error(UI.No_plugin_supports_requested_extension);

                return (int)ErrorNumber.FormatNotFound;
            case > 1:
                AaruLogging.Error(UI.More_than_one_plugin_supports_requested_extension);

                return (int)ErrorNumber.TooManyFormats;
        }

        while(true)
        {
            string responseLine = resReader.ReadLine();

            if(responseLine is null) break;

            if(responseLine.Any(static c => c < 0x20))
            {
                AaruLogging.Error(UI.Invalid_characters_found_in_list_of_files);

                return (int)ErrorNumber.InvalidArgument;
            }

            if(isResponse)
            {
                AaruLogging.WriteLine(UI.Please_insert_media_with_title_0_and_press_any_key_to_continue_, responseLine);

                Console.ReadKey();
                Thread.Sleep(1000);
            }

            responseLine = responseLine.Replace('/', '／');

            // Replace Windows forbidden filename characters with Japanese equivalents that are visually the same, but bigger.
            if(DetectOS.IsWindows)
            {
                responseLine = responseLine.Replace('<', '\uFF1C')
                                           .Replace('>',  '\uFF1E')
                                           .Replace(':',  '\uFF1A')
                                           .Replace('"',  '\u2033')
                                           .Replace('\\', '＼')
                                           .Replace('|',  '｜')
                                           .Replace('?',  '？')
                                           .Replace('*',  '＊');
            }

            string devicePath = settings.DevicePath;

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
                    AaruLogging.Error(string.Format(UI.Could_not_open_device_error_0, Error.Print(devErrno)));

                    if(isResponse) continue;

                    return (int)ErrorNumber.CannotOpenDevice;
                }
                case Devices.Remote.Device remoteDev:
                    Statistics.AddRemote(remoteDev.RemoteApplication,
                                         remoteDev.RemoteVersion,
                                         remoteDev.RemoteOperatingSystem,
                                         remoteDev.RemoteOperatingSystemVersion,
                                         remoteDev.RemoteArchitecture);

                    break;
            }

            if(dev.Error)
            {
                AaruLogging.Error(Error.Print(dev.LastError));

                if(isResponse) continue;

                return (int)ErrorNumber.CannotOpenDevice;
            }

            Statistics.AddDevice(dev);

            string outputPrefix = Path.Combine(Path.GetDirectoryName(settings.OutputPath), responseLine);

            Resume resumeClass = null;

            if(settings.Resume)
            {
                try
                {
                    if(File.Exists(outputPrefix + ".resume.json"))
                    {
                        var fs = new FileStream(outputPrefix + ".resume.json", FileMode.Open);

                        resumeClass =
                            (JsonSerializer.Deserialize(fs,
                                                        typeof(ResumeJson),
                                                        ResumeJsonContext.Default) as ResumeJson)?.Resume;

                        fs.Close();
                    }

                    // DEPRECATED: To be removed in Aaru 7
                    else if(File.Exists(outputPrefix + ".resume.xml") && settings.Resume)
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
                catch(Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                    AaruLogging.Error(UI.Incorrect_resume_file_not_continuing);

                    if(isResponse) continue;

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
                AaruLogging.WriteLine(UI.Media_already_dumped_correctly_not_continuing);

                if(isResponse) continue;

                return (int)ErrorNumber.AlreadyDumped;
            }

            Metadata sidecar = null;

            if(settings.AaruMetadata != null)
            {
                if(File.Exists(settings.AaruMetadata))
                {
                    try
                    {
                        var fs = new FileStream(settings.AaruMetadata, FileMode.Open);

                        sidecar =
                            (JsonSerializer.Deserialize(fs, typeof(MetadataJson), MetadataJsonContext.Default) as
                                 MetadataJson)?.AaruMetadata;

                        fs.Close();
                    }
                    catch(Exception ex)
                    {
                        SentrySdk.CaptureException(ex);
                        AaruLogging.Error(UI.Incorrect_metadata_sidecar_file_not_continuing);

                        if(isResponse) continue;

                        return (int)ErrorNumber.InvalidSidecar;
                    }
                }
                else
                {
                    AaruLogging.Error(UI.Could_not_find_metadata_sidecar);

                    if(isResponse) continue;

                    return (int)ErrorNumber.NoSuchFile;
                }
            }
            else if(settings.CicmXml != null)
            {
                if(File.Exists(settings.CicmXml))
                {
                    try
                    {
                        var sr = new StreamReader(settings.CicmXml);

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
                    catch(Exception ex)
                    {
                        SentrySdk.CaptureException(ex);
                        AaruLogging.Error(UI.Incorrect_metadata_sidecar_file_not_continuing);

                        if(isResponse) continue;

                        return (int)ErrorNumber.InvalidSidecar;
                    }
                }
                else
                {
                    AaruLogging.Error(UI.Could_not_find_metadata_sidecar);

                    if(isResponse) continue;

                    return (int)ErrorNumber.NoSuchFile;
                }
            }

            plugins    = PluginRegister.Singleton;
            candidates = [];

            // Try extension
            if(string.IsNullOrEmpty(settings.Format))
            {
                candidates.AddRange(from plugin in plugins.WritableImages.Values
                                    where plugin is not null
                                    where plugin.KnownExtensions.Contains(Path.GetExtension(settings.OutputPath))
                                    select plugin);
            }

            // Try Id
            else if(Guid.TryParse(settings.Format, out Guid outId))
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
                                    where plugin.Name.Equals(settings.Format,
                                                             StringComparison.InvariantCultureIgnoreCase)
                                    select plugin);
            }

            IBaseWritableImage outputFormat = candidates[0];

            if(settings.ErrorRecovery > 0)
            {
                if(outputFormat is not AaruFormat)
                {
                    AaruLogging.Error(UI.Error_recovery_is_only_supported_in_AaruFormat);

                    return (int)ErrorNumber.NotSupported;
                }

                if(settings.ErrorRecovery > 100)
                {
                    AaruLogging.Error(UI.Maximum_error_recovery_is_100);

                    return (int)ErrorNumber.InvalidArgument;
                }
            }

            DeviceLog.StartLog(dev, settings.Private);

            if(settings.Verbose)
                AaruLogging.Verbose(UI.Output_image_format_0_1, outputFormat.Name, outputFormat.Id);
            else
                AaruLogging.WriteLine(UI.Output_image_format_0, outputFormat.Name);

            var errorLog = new ErrorLog(outputPrefix + ".error.log");

            var dumper = new Dump(settings.Resume,
                                  dev,
                                  devicePath,
                                  outputFormat,
                                  (ushort)settings.RetryPasses,
                                  settings.Force,
                                  settings.Raw,
                                  settings.Persistent,
                                  settings.StopOnError,
                                  resumeClass,
                                  encodingClass,
                                  outputPrefix,
                                  outputPrefix + extension,
                                  parsedOptions,
                                  sidecar,
                                  (uint)settings.Skip,
                                  settings.Metadata,
                                  settings.Trim,
                                  settings.FirstPregap,
                                  settings.FixOffset,
                                  settings.Debug,
                                  wantedSubchannel,
                                  settings.Speed,
                                  settings.Private,
                                  fixSubchannelPosition,
                                  settings.RetrySubchannel,
                                  fixSubchannel,
                                  fixSubchannelCrc,
                                  settings.SkipCdiReadyHole,
                                  errorLog,
                                  settings.GenerateSubchannels,
                                  maxBlocks,
                                  settings.UseBufferedReads,
                                  settings.StoreEncrypted,
                                  settings.TitleKeys,
                                  (uint)settings.IgnoreCdrRunOuts,
                                  settings.CreateGraph,
                                  (uint)settings.Dimensions,
                                  settings.Paranoia,
                                  settings.CureParanoia,
                                  settings.BypassWiiDecryption,
                                  settings.StartReverse,
                                  settings.ErrorRecovery,
                                  settings.HyperSpeed,
                                  settings.LudicrousSpeed,
                                  settings.LeadOut,
                                  settings.SkipSafeDisc,
                                  settings.C2Repair);

            AnsiConsole.Progress()
                       .AutoClear(true)
                       .HideCompleted(true)
                       .Columns(new ProgressBarColumn(),
                                new RemainingTimeColumn(),
                                new PercentageColumn(),
                                new TaskDescriptionColumn())
                       .Start(ctx =>
                        {
                            dumper.UpdateStatus += static text => { AaruLogging.WriteLine(text); };

                            dumper.ErrorMessage += static text => AaruLogging.Error(text);

                            dumper.StoppingErrorMessage += static text => { AaruLogging.Error(text); };

                            dumper.UpdateProgress += (text, current, maximum) =>
                            {
                                _progressTask1             ??= ctx.AddTask("Progress");
                                _progressTask1.Description =   text;
                                _progressTask1.Value       =   current;
                                _progressTask1.MaxValue    =   maximum;
                            };

                            dumper.PulseProgress += text =>
                            {
                                if(_progressTask1 is null)
                                    ctx.AddTask(text).IsIndeterminate();
                                else
                                {
                                    _progressTask1.Description     = text;
                                    _progressTask1.IsIndeterminate = true;
                                }
                            };

                            dumper.InitProgress += () => { _progressTask1 = ctx.AddTask("Progress"); };

                            dumper.EndProgress += static () =>
                            {
                                _progressTask1?.StopTask();
                                _progressTask1 = null;
                            };

                            dumper.InitProgress2 += () => { _progressTask2 = ctx.AddTask("Progress"); };

                            dumper.EndProgress2 += static () =>
                            {
                                _progressTask2?.StopTask();
                                _progressTask2 = null;
                            };

                            dumper.UpdateProgress2 += (text, current, maximum) =>
                            {
                                _progressTask2             ??= ctx.AddTask("Progress");
                                _progressTask2.Description =   text;
                                _progressTask2.Value       =   current;
                                _progressTask2.MaxValue    =   maximum;
                            };

                            Console.CancelKeyPress += (_, e) =>
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

#region Nested type: Settings

    public class Settings : MediaFamily
    {
        [LocalizedDescription(nameof(UI.Take_metadata_from_existing_CICM_XML_sidecar))]
        [CommandOption("-x|--cicm-xml")]
        [DefaultValue(null)]
        public string CicmXml { get; init; }
        [LocalizedDescription(nameof(UI.Name_of_character_encoding_to_use))]
        [CommandOption("-e|--encoding")]
        [DefaultValue(null)]
        public string Encoding { get; init; }
        [LocalizedDescription(nameof(UI.Try_to_read_first_track_pregap))]
        [CommandOption("--first-pregap")]
        [DefaultValue(false)]
        public bool FirstPregap { get; init; }
        [LocalizedDescription(nameof(UI.Fix_audio_tracks_offset))]
        [CommandOption("--fix-offset")]
        [DefaultValue(true)]
        public bool FixOffset { get; init; }
        [LocalizedDescription(nameof(UI.Continue_dumping_whatever_happens))]
        [CommandOption("-f|--force")]
        [DefaultValue(false)]
        public bool Force { get; init; }
        [LocalizedDescription(nameof(UI.Format_of_the_output_image_as_plugin_name_or_plugin_id))]
        [CommandOption("-t|--format")]
        [DefaultValue(null)]
        public string Format { get; init; }
        [LocalizedDescription(nameof(UI.Enables_creating_Aaru_Metadata_sidecar))]
        [CommandOption("--metadata")]
        [DefaultValue(true)]
        public bool Metadata { get; init; }
        [LocalizedDescription(nameof(UI.Enables_trimming_errored_from_skipped_sectors))]
        [CommandOption("--trim")]
        [DefaultValue(true)]
        public bool Trim { get; init; }
        [LocalizedDescription(nameof(UI.Comma_separated_name_value_pairs_of_image_options))]
        [CommandOption("-O|--options")]
        [DefaultValue(null)]
        public string Options { get; init; }
        [LocalizedDescription(nameof(UI.Try_to_recover_partial_or_incorrect_data))]
        [CommandOption("--persistent")]
        [DefaultValue(false)]
        public bool Persistent { get; init; }
        [LocalizedDescription(nameof(UI.Create_or_use_resume_mapfile))]
        [CommandOption("-r|--resume")]
        [DefaultValue(true)]
        public bool Resume { get; init; }
        [LocalizedDescription(nameof(UI.How_many_retry_passes_to_do))]
        [CommandOption("-p|--retry-passes")]
        [DefaultValue(5)]
        public int RetryPasses { get; init; }
        [LocalizedDescription(nameof(UI.When_an_unreadable_sector_is_found_skip_this_many_sectors))]
        [CommandOption("-k|--skip")]
        [DefaultValue(512)]
        public int Skip { get; init; }
        [LocalizedDescription(nameof(UI.Stop_media_dump_on_first_error))]
        [CommandOption("-s|--stop-on-error")]
        [DefaultValue(false)]
        public bool StopOnError { get; init; }
        [LocalizedDescription(nameof(UI.Subchannel_to_dump_help))]
        [CommandOption("--subchannel")]
        [DefaultValue("any")]
        public string Subchannel { get; init; }
        [LocalizedDescription(nameof(UI.Speed_to_dump))]
        [CommandOption("--speed")]
        [DefaultValue(0)]
        public int Speed { get; init; }
        [LocalizedDescription(nameof(UI.Do_not_store_paths_and_serial_numbers_in_log_or_metadata))]
        [CommandOption("--private")]
        [DefaultValue(false)]
        public bool Private { get; init; }
        [LocalizedDescription(nameof(UI.Fix_subchannel_position_help))]
        [CommandOption("--fix-subchannel-position")]
        [DefaultValue(true)]
        public bool FixSubchannelPosition { get; init; }
        [LocalizedDescription(nameof(UI.Retry_subchannel_help))]
        [CommandOption("--retry-subchannel")]
        [DefaultValue(true)]
        public bool RetrySubchannel { get; init; }
        [LocalizedDescription(nameof(UI.Fix_subchannel_help))]
        [CommandOption("--fix-subchannel")]
        [DefaultValue(false)]
        public bool FixSubchannel { get; init; }
        [LocalizedDescription(nameof(UI.Fix_subchannel_crc_help))]
        [CommandOption("--fix-subchannel-crc")]
        [DefaultValue(false)]
        public bool FixSubchannelCrc { get; init; }
        [LocalizedDescription(nameof(UI.Generate_subchannels_dump_help))]
        [CommandOption("--generate-subchannels")]
        [DefaultValue(false)]
        public bool GenerateSubchannels { get; init; }
        [LocalizedDescription(nameof(UI.Skip_CDi_Ready_hole_help))]
        [CommandOption("--skip-cdiready-hole")]
        [DefaultValue(true)]
        public bool SkipCdiReadyHole { get; init; }
        [LocalizedDescription(nameof(UI.Eject_media_after_dump_finishes))]
        [CommandOption("--eject")]
        [DefaultValue(false)]
        public bool Eject { get; init; }
        [LocalizedDescription(nameof(UI.Maximum_number_of_blocks_to_read_at_once))]
        [CommandOption("--max-blocks")]
        [DefaultValue(64)]
        public int MaxBlocks { get; init; }
        [LocalizedDescription(nameof(UI.OS_buffered_reads_help))]
        [CommandOption("--use-buffered-reads")]
        [DefaultValue(true)]
        public bool UseBufferedReads { get; init; }
        [LocalizedDescription(nameof(UI.Store_encrypted_data_as_is))]
        [CommandOption("--store-encrypted")]
        [DefaultValue(true)]
        public bool StoreEncrypted { get; init; }
        [LocalizedDescription(nameof(UI.Bypass_Wii_decryption_help))]
        [CommandOption("--bypass-wii-decryption")]
        [DefaultValue(false)]
        public bool BypassWiiDecryption { get; init; }
        [LocalizedDescription(nameof(UI.Try_to_read_the_title_keys_from_CSS_DVDs))]
        [CommandOption("--title-keys")]
        [DefaultValue(true)]
        public bool TitleKeys { get; init; }
        [LocalizedDescription(nameof(UI.How_many_CDRW_run_out_sectors_to_ignore_and_regenerate))]
        [CommandOption("--ignore-cdr-runouts")]
        [DefaultValue(10)]
        public int IgnoreCdrRunOuts { get; init; }
        [LocalizedDescription(nameof(UI.Create_graph_of_dumped_media))]
        [CommandOption("-g|--create-graph")]
        [DefaultValue(true)]
        public bool CreateGraph { get; init; }
        [LocalizedDescription(nameof(UI.Dump_graph_dimensions_argument_help))]
        [CommandOption("--dimensions")]
        [DefaultValue(1080)]
        public int Dimensions { get; init; }
        [LocalizedDescription(nameof(UI.Take_metadata_from_existing_Aaru_sidecar))]
        [CommandOption("--aaru-metadata")]
        [DefaultValue(null)]
        public string AaruMetadata { get; init; }
        [LocalizedDescription(nameof(UI.Paranoia_help))]
        [CommandOption("--paranoia")]
        [DefaultValue(false)]
        public bool Paranoia { get; init; }
        [LocalizedDescription(nameof(UI.C2_repair_help))]
        [CommandOption("--c2-repair")]
        [DefaultValue(true)]
        public bool C2Repair { get; init; }
        [LocalizedDescription(nameof(UI.Cure_paranoia_help))]
        [CommandOption("--cure-paranoia")]
        [DefaultValue(false)]
        public bool CureParanoia { get; init; }
        [LocalizedDescription(nameof(UI.Raw_dumping_experimental_help))]
        [CommandOption("--raw")]
        [DefaultValue(false)]
        public bool Raw { get; init; }
        [LocalizedDescription(nameof(UI.Device_path))]
        [CommandArgument(0, "<device-path>")]
        public string DevicePath { get; init; }
        [LocalizedDescription(nameof(UI.Output_image_path_Dump_help))]
        [CommandArgument(1, "<output-path>")]
        public string OutputPath { get; init; }
        [LocalizedDescription(nameof(UI.Start_reverse_error_retry))]
        [CommandOption("--start-reverse")]
        public bool StartReverse { get; init; }
        [LocalizedDescription(nameof(UI.Add_error_recovery))]
        [DefaultValue(0)]
        [CommandOption("--error-recovery")]
        public int ErrorRecovery { get; set; }
        [LocalizedDescription(nameof(UI.Help_hyperspeed))]
        [DefaultValue(false)]
        [CommandOption("--hyper-speed")]
        public bool HyperSpeed { get; set; }
        [LocalizedDescription(nameof(UI.Help_ludicrousspeed))]
        [DefaultValue(false)]
        [CommandOption("--ludicrous-speed")]
        public bool LudicrousSpeed { get; set; }
        [LocalizedDescription(nameof(UI.Help_Leadout))]
        [DefaultValue(false)]
        [CommandOption("--lead-out")]
        public bool LeadOut { get; set; }
        [Description("Skip retrying sectors from 800 to 10000, that are typically used by SafeDisc protection")]
        [DefaultValue(false)]
        [CommandOption("--skip-safedisc")]
        public bool SkipSafeDisc { get; set; }
    }

#endregion
}