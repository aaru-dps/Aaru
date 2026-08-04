using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.Core;
using Aaru.Core.Image;
using Aaru.Localization;
using Aaru.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Aaru.Commands.Image;

class MergeCommand : AsyncCommand<MergeCommand.Settings>
{
    const  string       MODULE_NAME = "Merge-image command";
    static ProgressTask _progressTask1;
    static ProgressTask _progressTask2;

    protected override async Task<int> ExecuteAsync(CommandContext    context, Settings settings,
                                                    CancellationToken cancellationToken)
    {
        MainClass.PrintCopyright();

        // Initialize subchannel fix flags with cascading dependencies
        bool fixSubchannel         = settings.FixSubchannel;
        bool fixSubchannelCrc      = settings.FixSubchannelCrc;
        bool fixSubchannelPosition = settings.FixSubchannelPosition;

        if(fixSubchannelCrc) fixSubchannel = true;

        if(fixSubchannel) fixSubchannelPosition = true;

        Statistics.AddCommand("merge-image");

        // Log all command parameters for debugging and auditing
        Dictionary<string, string> parsedOptions = Options.Parse(settings.Options);
        LogCommandParameters(settings, fixSubchannelPosition, fixSubchannel, fixSubchannelCrc, parsedOptions);

        var merger = new Merger(settings.PrimaryImagePath,
                                settings.SecondaryImagePath,
                                settings.OutputImagePath,
                                settings.UseSecondaryTags,
                                settings.SectorsFile,
                                settings.IgnoreMediaType,
                                settings.IgnoreSectorCount,
                                settings.Comments,
                                settings.Count,
                                settings.Creator,
                                settings.DriveManufacturer,
                                settings.DriveModel,
                                settings.DriveFirmwareRevision,
                                settings.DriveSerialNumber,
                                settings.Format,
                                settings.MediaBarcode,
                                settings.LastMediaSequence,
                                settings.MediaManufacturer,
                                settings.MediaModel,
                                settings.MediaPartNumber,
                                settings.MediaSequence,
                                settings.MediaSerialNumber,
                                settings.MediaTitle,
                                parsedOptions,
                                settings.PrimaryResumeFile,
                                settings.SecondaryResumeFile,
                                settings.Geometry,
                                fixSubchannelPosition,
                                fixSubchannel,
                                fixSubchannelCrc,
                                settings.GenerateSubchannels,
                                settings.Decrypt,
                                settings.IgnoreNegativeSectors,
                                settings.IgnoreOverflowSectors,
                                settings.IgnoreSectorNotFound,
                                settings.ErrorRecovery);

        ErrorNumber errno = ErrorNumber.NoError;

        AnsiConsole.Progress()
                   .AutoClear(true)
                   .HideCompleted(true)
                   .Columns(new ProgressBarColumn(), new PercentageColumn(), new TaskDescriptionColumn())
                   .Start(ctx =>
                    {
                        merger.UpdateStatus += static text => AaruLogging.WriteLine(text);

                        merger.ErrorMessage += static text => AaruLogging.Error(text);

                        merger.StoppingErrorMessage += static text => AaruLogging.Error(text);

                        merger.UpdateProgress += (text, current, maximum) =>
                        {
                            _progressTask1             ??= ctx.AddTask("Progress");
                            _progressTask1.Description =   text;
                            _progressTask1.Value       =   current;
                            _progressTask1.MaxValue    =   maximum;
                        };

                        merger.PulseProgress += text =>
                        {
                            if(_progressTask1 is null)
                                ctx.AddTask(text).IsIndeterminate();
                            else
                            {
                                _progressTask1.Description     = text;
                                _progressTask1.IsIndeterminate = true;
                            }
                        };

                        merger.InitProgress += () => _progressTask1 = ctx.AddTask("Progress");

                        merger.EndProgress += static () =>
                        {
                            _progressTask1?.StopTask();
                            _progressTask1 = null;
                        };

                        merger.InitProgress2 += () => _progressTask2 = ctx.AddTask("Progress");

                        merger.EndProgress2 += static () =>
                        {
                            _progressTask2?.StopTask();
                            _progressTask2 = null;
                        };

                        merger.UpdateProgress2 += (text, current, maximum) =>
                        {
                            _progressTask2             ??= ctx.AddTask("Progress");
                            _progressTask2.Description =   text;
                            _progressTask2.Value       =   current;
                            _progressTask2.MaxValue    =   maximum;
                        };

                        Console.CancelKeyPress += (_, e) =>
                        {
                            e.Cancel = true;
                            merger.Abort();
                        };

                        errno = merger.Start();
                    });

        return (int)errno;
    }

    private static void LogCommandParameters(Settings settings,         bool fixSubchannelPosition, bool fixSubchannel,
                                             bool     fixSubchannelCrc, Dictionary<string, string> parsedOptions)
    {
        // Logs all command-line parameters for debugging and audit trail purposes

        AaruLogging.Debug(MODULE_NAME, "--secondary-tags={0}", settings.UseSecondaryTags);
        AaruLogging.Debug(MODULE_NAME, "--comments={0}",       Markup.Escape(settings.Comments ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--count={0}",          settings.Count);
        AaruLogging.Debug(MODULE_NAME, "--creator={0}",        Markup.Escape(settings.Creator ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--debug={0}",          settings.Debug);

        AaruLogging.Debug(MODULE_NAME, "--drive-manufacturer={0}", Markup.Escape(settings.DriveManufacturer ?? ""));

        AaruLogging.Debug(MODULE_NAME, "--drive-model={0}", Markup.Escape(settings.DriveModel ?? ""));

        AaruLogging.Debug(MODULE_NAME, "--drive-revision={0}", Markup.Escape(settings.DriveFirmwareRevision ?? ""));

        AaruLogging.Debug(MODULE_NAME, "--drive-serial={0}",        Markup.Escape(settings.DriveSerialNumber ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--ignore-media-type={0}",   settings.IgnoreMediaType);
        AaruLogging.Debug(MODULE_NAME, "--ignore-sector-count={0}", settings.IgnoreSectorCount);
        AaruLogging.Debug(MODULE_NAME, "--format={0}",              Markup.Escape(settings.Format       ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--geometry={0}",            Markup.Escape(settings.Geometry     ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--media-barcode={0}",       Markup.Escape(settings.MediaBarcode ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--media-lastsequence={0}",  settings.LastMediaSequence);

        AaruLogging.Debug(MODULE_NAME, "--media-manufacturer={0}", Markup.Escape(settings.MediaManufacturer ?? ""));

        AaruLogging.Debug(MODULE_NAME, "--media-model={0}", Markup.Escape(settings.MediaModel ?? ""));

        AaruLogging.Debug(MODULE_NAME, "--media-partnumber={0}", Markup.Escape(settings.MediaPartNumber ?? ""));

        AaruLogging.Debug(MODULE_NAME, "--media-sequence={0}", settings.MediaSequence);
        AaruLogging.Debug(MODULE_NAME, "--media-serial={0}", Markup.Escape(settings.MediaSerialNumber ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--media-title={0}", Markup.Escape(settings.MediaTitle ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--options={0}", Markup.Escape(settings.Options ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--secondary-resume={0}", Markup.Escape(settings.SecondaryResumeFile ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--primary-resume={0}", Markup.Escape(settings.PrimaryResumeFile ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--verbose={0}", settings.Verbose);
        AaruLogging.Debug(MODULE_NAME, "--fix-subchannel-position={0}", fixSubchannelPosition);
        AaruLogging.Debug(MODULE_NAME, "--fix-subchannel={0}", fixSubchannel);
        AaruLogging.Debug(MODULE_NAME, "--fix-subchannel-crc={0}", fixSubchannelCrc);
        AaruLogging.Debug(MODULE_NAME, "--generate-subchannels={0}", settings.GenerateSubchannels);
        AaruLogging.Debug(MODULE_NAME, "--decrypt={0}", settings.Decrypt);
        AaruLogging.Debug(MODULE_NAME, "--sectors-file={0}", Markup.Escape(settings.SectorsFile ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--ignore-negative-sectors={0}", settings.IgnoreNegativeSectors);
        AaruLogging.Debug(MODULE_NAME, "--ignore-overflow-sectors={0}", settings.IgnoreOverflowSectors);
        AaruLogging.Debug(MODULE_NAME, "--ignore-sector-not-found={0}", settings.IgnoreSectorNotFound);
        AaruLogging.Debug(MODULE_NAME, "--error-recovery={0}", settings.ErrorRecovery);

        AaruLogging.Debug(MODULE_NAME, UI.Parsed_options);

        foreach(KeyValuePair<string, string> parsedOption in parsedOptions)
            AaruLogging.Debug(MODULE_NAME, "{0} = {1}", parsedOption.Key, parsedOption.Value);
    }

    public class Settings : ImageFamily
    {
        [LocalizedDescription(nameof(UI.Path_to_the_primary_image_file))]
        [CommandArgument(0, "<primary-image>")]
        public string PrimaryImagePath { get; init; }
        [LocalizedDescription(nameof(UI.Path_to_the_secondary_image_file))]
        [CommandArgument(1, "<secondary-image>")]
        public string SecondaryImagePath { get; init; }
        [LocalizedDescription(nameof(UI.Path_to_the_output_merged_image_file))]
        [CommandArgument(2, "<output-image>")]
        public string OutputImagePath { get; init; }
        [LocalizedDescription(nameof(UI.Use_media_tags_from_secondary_image))]
        [DefaultValue(false)]
        [CommandOption("--secondary-tags")]
        public bool UseSecondaryTags { get; init; }
        [LocalizedDescription(nameof(UI.File_containing_list_of_sectors_to_take_from_secondary_image))]
        [DefaultValue(null)]
        [CommandOption("--sectors-file")]
        public string SectorsFile { get; init; }
        [LocalizedDescription(nameof(UI.Ignore_mismatched_image_media_type))]
        [DefaultValue(false)]
        [CommandOption("--ignore-media-type")]
        public bool IgnoreMediaType { get; init; }
        [LocalizedDescription(nameof(UI.Ignore_mismatched_image_sector_count))]
        [DefaultValue(false)]
        [CommandOption("--ignore-sector-count")]
        public bool IgnoreSectorCount { get; init; }
        [LocalizedDescription(nameof(UI.Image_comments))]
        [DefaultValue(null)]
        [CommandOption("--comments")]
        public string Comments { get; init; }
        [LocalizedDescription(nameof(UI.How_many_sectors_to_convert_at_once))]
        [DefaultValue(64)]
        [CommandOption("-c|--count")]
        public int Count { get; init; }
        [LocalizedDescription(nameof(UI.Who_person_created_the_image))]
        [DefaultValue(null)]
        [CommandOption("--creator")]
        public string Creator { get; init; }
        [LocalizedDescription(nameof(UI.Manufacturer_of_drive_read_the_media_by_image))]
        [DefaultValue(null)]
        [CommandOption("--drive-manufacturer")]
        public string DriveManufacturer { get; init; }
        [LocalizedDescription(nameof(UI.Model_of_drive_used_by_media))]
        [DefaultValue(null)]
        [CommandOption("--drive-model")]
        public string DriveModel { get; init; }
        [LocalizedDescription(nameof(UI.Firmware_revision_of_drive_read_the_media_by_image))]
        [DefaultValue(null)]
        [CommandOption("--drive-revision")]
        public string DriveFirmwareRevision { get; init; }
        [LocalizedDescription(nameof(UI.Serial_number_of_drive_read_the_media_by_image))]
        [DefaultValue(null)]
        [CommandOption("--drive-serial")]
        public string DriveSerialNumber { get; init; }
        [LocalizedDescription(nameof(UI.Format_of_the_output_image_as_plugin_name_or_plugin_id))]
        [DefaultValue(null)]
        [CommandOption("-p|--format")]
        public string Format { get; init; }
        [LocalizedDescription(nameof(UI.Barcode_of_the_media))]
        [DefaultValue(null)]
        [CommandOption("--media-barcode")]
        public string MediaBarcode { get; init; }
        [LocalizedDescription(nameof(UI.Last_media_of_sequence_by_image))]
        [DefaultValue(0)]
        [CommandOption("--media-lastsequence")]
        public int LastMediaSequence { get; init; }
        [LocalizedDescription(nameof(UI.Manufacturer_of_media_by_image))]
        [DefaultValue(null)]
        [CommandOption("--media-manufacturer")]
        public string MediaManufacturer { get; init; }
        [LocalizedDescription(nameof(UI.Model_of_media_by_image))]
        [DefaultValue(null)]
        [CommandOption("--media-model")]
        public string MediaModel { get; init; }
        [LocalizedDescription(nameof(UI.Part_number_of_media_by_image))]
        [DefaultValue(null)]
        [CommandOption("--media-partnumber")]
        public string MediaPartNumber { get; init; }
        [LocalizedDescription(nameof(UI.Number_in_sequence_for_media_by_image))]
        [DefaultValue(0)]
        [CommandOption("--media-sequence")]
        public int MediaSequence { get; init; }
        [LocalizedDescription(nameof(UI.Serial_number_of_media_by_image))]
        [DefaultValue(null)]
        [CommandOption("--media-serial")]
        public string MediaSerialNumber { get; init; }
        [LocalizedDescription(nameof(UI.Title_of_media_represented_by_image))]
        [DefaultValue(null)]
        [CommandOption("--media-title")]
        public string MediaTitle { get; init; }
        [LocalizedDescription(nameof(UI.Comma_separated_name_value_pairs_of_image_options))]
        [DefaultValue(null)]
        [CommandOption("-O|--options")]
        public string Options { get; init; }
        [LocalizedDescription(nameof(UI.Resume_file_for_primary_image))]
        [DefaultValue(null)]
        [CommandOption("--primary-resume")]
        public string PrimaryResumeFile { get; init; }
        [LocalizedDescription(nameof(UI.Resume_file_for_secondary_image))]
        [DefaultValue(null)]
        [CommandOption("--secondary-resume")]
        public string SecondaryResumeFile { get; init; }
        [LocalizedDescription(nameof(UI.Force_geometry_help))]
        [DefaultValue(null)]
        [CommandOption("-g|--geometry")]
        public string Geometry { get; init; }
        [LocalizedDescription(nameof(UI.Fix_subchannel_position_help))]
        [DefaultValue(true)]
        [CommandOption("--fix-subchannel-position")]
        public bool FixSubchannelPosition { get; init; }
        [LocalizedDescription(nameof(UI.Fix_subchannel_help))]
        [DefaultValue(false)]
        [CommandOption("--fix-subchannel")]
        public bool FixSubchannel { get; init; }
        [LocalizedDescription(nameof(UI.Fix_subchannel_crc_help))]
        [DefaultValue(false)]
        [CommandOption("--fix-subchannel-crc")]
        public bool FixSubchannelCrc { get; init; }
        [LocalizedDescription(nameof(UI.Generates_subchannels_help))]
        [DefaultValue(false)]
        [CommandOption("--generate-subchannels")]
        public bool GenerateSubchannels { get; init; }
        [LocalizedDescription(nameof(UI.Decrypt_sectors_help))]
        [DefaultValue(false)]
        [CommandOption("--decrypt")]
        public bool Decrypt { get; init; }
        [LocalizedDescription(nameof(UI.Ignore_negative_sectors))]
        [DefaultValue(false)]
        [CommandOption("--ignore-negative-sectors")]
        public bool IgnoreNegativeSectors { get; init; }
        [LocalizedDescription(nameof(UI.Ignore_overflow_sectors))]
        [DefaultValue(false)]
        [CommandOption("--ignore-overflow-sectors")]
        public bool IgnoreOverflowSectors { get; init; }
        [LocalizedDescription(nameof(UI.Ignore_sector_not_found))]
        [DefaultValue(false)]
        [CommandOption("--ignore-sector-not-found")]
        public bool IgnoreSectorNotFound { get; init; }
        [LocalizedDescription(nameof(UI.Add_error_recovery))]
        [DefaultValue(0)]
        [CommandOption("--error-recovery")]
        public int ErrorRecovery { get; set; }
    }
}