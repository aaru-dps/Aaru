// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Convert.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Converts from one media image to another.
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
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Aaru.Console;
using Aaru.Core;
using Aaru.Core.Media;
using Aaru.Decryption.DVD;
using Aaru.Devices;
using Aaru.Localization;
using Schemas;
using Spectre.Console;
using File = System.IO.File;
using ImageInfo = Aaru.CommonTypes.Structs.ImageInfo;
using MediaType = Aaru.CommonTypes.MediaType;
using Partition = Aaru.CommonTypes.Partition;
using TapeFile = Aaru.CommonTypes.Structs.TapeFile;
using TapePartition = Aaru.CommonTypes.Structs.TapePartition;
using Track = Aaru.CommonTypes.Structs.Track;
using Version = Aaru.CommonTypes.Interop.Version;

namespace Aaru.Commands.Image;

sealed class ConvertImageCommand : Command
{
    const string MODULE_NAME = "Convert-image command";

    public ConvertImageCommand() : base("convert", UI.Image_Convert_Command_Description)
    {
        Add(new Option<string>(new[] { "--cicm-xml", "-x" }, () => null,
                               UI.Take_metadata_from_existing_CICM_XML_sidecar));

        Add(new Option<string>("--comments", () => null, UI.Image_comments));

        Add(new Option<int>(new[] { "--count", "-c" }, () => 64, UI.How_many_sectors_to_convert_at_once));

        Add(new Option<string>("--creator", () => null, UI.Who_person_created_the_image));

        Add(new Option<string>("--drive-manufacturer", () => null, UI.Manufacturer_of_drive_read_the_media_by_image));

        Add(new Option<string>("--drive-model", () => null, UI.Model_of_drive_read_the_media_by_image));

        Add(new Option<string>("--drive-revision", () => null, UI.Firmware_revision_of_drive_read_the_media_by_image));

        Add(new Option<string>("--drive-serial", () => null, UI.Serial_number_of_drive_read_the_media_by_image));

        Add(new Option<bool>(new[] { "--force", "-f" }, UI.Continue_conversion_even_if_data_lost));

        Add(new Option<string>(new[] { "--format", "-p" }, () => null,
                               UI.Format_of_the_output_image_as_plugin_name_or_plugin_id));

        Add(new Option<string>("--media-barcode", () => null, UI.Barcode_of_the_media_by_image));

        Add(new Option<int>("--media-lastsequence", () => 0, UI.Last_media_of_sequence_by_image));

        Add(new Option<string>("--media-manufacturer", () => null, UI.Manufacturer_of_media_by_image));

        Add(new Option<string>("--media-model",      () => null, UI.Model_of_media_by_image));
        Add(new Option<string>("--media-partnumber", () => null, UI.Part_number_of_media_by_image));
        Add(new Option<int>("--media-sequence", () => 0, UI.Number_in_sequence_for_media_by_image));
        Add(new Option<string>("--media-serial", () => null, UI.Serial_number_of_media_by_image));
        Add(new Option<string>("--media-title",  () => null, UI.Title_of_media_represented_by_image));

        Add(new Option<string>(new[] { "--options", "-O" }, () => null,
                               UI.Comma_separated_name_value_pairs_of_image_options));

        Add(new Option<string>(new[] { "--resume-file", "-r" }, () => null,
                               UI.Take_dump_hardware_from_existing_resume));

        Add(new Option<string>(new[] { "--geometry", "-g" }, () => null, UI.Force_geometry_help));

        Add(new Option<bool>(new[] { "--fix-subchannel-position" }, () => true, UI.Fix_subchannel_position_help));

        Add(new Option<bool>(new[] { "--fix-subchannel" }, () => false, UI.Fix_subchannel_help));

        Add(new Option<bool>(new[] { "--fix-subchannel-crc" }, () => false, UI.Fix_subchannel_crc_help));

        Add(new Option<bool>(new[] { "--generate-subchannels" }, () => false, UI.Generates_subchannels_help));

        Add(new Option<bool>(new[] { "--decrypt" }, () => false, UI.Decrypt_sectors_help));

        Add(new Option<string>(new[] { "--aaru-metadata", "-m" }, () => null,
                               "Take metadata from existing Aaru Metadata sidecar."));

        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = UI.Input_image_path,
            Name        = "input-path"
        });

        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = UI.Output_image_path,
            Name        = "output-path"
        });

        Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
    }

    public static int Invoke(bool   verbose, bool debug, string cicmXml, string comments, int count, string creator,
                             string driveFirmwareRevision, string driveManufacturer, string driveModel,
                             string driveSerialNumber, bool force, string inputPath, int lastMediaSequence,
                             string mediaBarcode, string mediaManufacturer, string mediaModel, string mediaPartNumber,
                             int    mediaSequence, string mediaSerialNumber, string mediaTitle, string outputPath,
                             string options, string resumeFile, string format, string geometry,
                             bool   fixSubchannelPosition, bool fixSubchannel, bool fixSubchannelCrc,
                             bool   generateSubchannels, bool decrypt, string aaruMetadata)
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

        if(fixSubchannelCrc)
            fixSubchannel = true;

        if(fixSubchannel)
            fixSubchannelPosition = true;

        Statistics.AddCommand("convert-image");

        AaruConsole.DebugWriteLine(MODULE_NAME, "--cicm-xml={0}",                cicmXml);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--comments={0}",                comments);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--count={0}",                   count);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--creator={0}",                 creator);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--debug={0}",                   debug);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--drive-manufacturer={0}",      driveManufacturer);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--drive-model={0}",             driveModel);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--drive-revision={0}",          driveFirmwareRevision);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--drive-serial={0}",            driveSerialNumber);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--force={0}",                   force);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--format={0}",                  format);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--geometry={0}",                geometry);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--input={0}",                   inputPath);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--media-barcode={0}",           mediaBarcode);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--media-lastsequence={0}",      lastMediaSequence);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--media-manufacturer={0}",      mediaManufacturer);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--media-model={0}",             mediaModel);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--media-partnumber={0}",        mediaPartNumber);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--media-sequence={0}",          mediaSequence);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--media-serial={0}",            mediaSerialNumber);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--media-title={0}",             mediaTitle);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--options={0}",                 options);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--output={0}",                  outputPath);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--resume-file={0}",             resumeFile);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--verbose={0}",                 verbose);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--fix-subchannel-position={0}", fixSubchannelPosition);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--fix-subchannel={0}",          fixSubchannel);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--fix-subchannel-crc={0}",      fixSubchannelCrc);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--generate-subchannels={0}",    generateSubchannels);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--decrypt={0}",                 decrypt);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--aaru-metadata={0}",           aaruMetadata);

        Dictionary<string, string> parsedOptions = Core.Options.Parse(options);
        AaruConsole.DebugWriteLine(MODULE_NAME, UI.Parsed_options);

        foreach(KeyValuePair<string, string> parsedOption in parsedOptions)
            AaruConsole.DebugWriteLine(MODULE_NAME, "{0} = {1}", parsedOption.Key, parsedOption.Value);

        if(count == 0)
        {
            AaruConsole.ErrorWriteLine(UI.Need_to_specify_more_than_zero_sectors_to_copy_at_once);

            return (int)ErrorNumber.InvalidArgument;
        }

        (uint cylinders, uint heads, uint sectors)? geometryValues = null;

        if(geometry != null)
        {
            string[] geometryPieces = geometry.Split('/');

            if(geometryPieces.Length == 0)
                geometryPieces = geometry.Split('-');

            if(geometryPieces.Length != 3)
            {
                AaruConsole.ErrorWriteLine(UI.Invalid_geometry_specified);

                return (int)ErrorNumber.InvalidArgument;
            }

            if(!uint.TryParse(geometryPieces[0], out uint cylinders) ||
               cylinders == 0)
            {
                AaruConsole.ErrorWriteLine(UI.Invalid_number_of_cylinders_specified);

                return (int)ErrorNumber.InvalidArgument;
            }

            if(!uint.TryParse(geometryPieces[1], out uint heads) ||
               heads == 0)
            {
                AaruConsole.ErrorWriteLine(UI.Invalid_number_of_heads_specified);

                return (int)ErrorNumber.InvalidArgument;
            }

            if(!uint.TryParse(geometryPieces[2], out uint spt) ||
               spt == 0)
            {
                AaruConsole.ErrorWriteLine(UI.Invalid_sectors_per_track_specified);

                return (int)ErrorNumber.InvalidArgument;
            }

            geometryValues = (cylinders, heads, spt);
        }

        Resume    resume  = null;
        Metadata  sidecar = null;
        MediaType mediaType;

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
                catch(Exception ex)
                {
                    AaruConsole.ErrorWriteLine(UI.Incorrect_metadata_sidecar_file_not_continuing);
                    AaruConsole.DebugWriteLine(MODULE_NAME, $"{ex}");

                    return (int)ErrorNumber.InvalidSidecar;
                }
            }
            else
            {
                AaruConsole.ErrorWriteLine(UI.Could_not_find_metadata_sidecar);

                return (int)ErrorNumber.NoSuchFile;
            }
        }

        else if(cicmXml != null)
        {
            if(File.Exists(cicmXml))
            {
                try
                {
                    // Should be covered by virtue of being the same exact class as the JSON above
                #pragma warning disable IL2026
                    var xs = new XmlSerializer(typeof(CICMMetadataType));
                #pragma warning restore IL2026

                    var sr = new StreamReader(cicmXml);

                    // Should be covered by virtue of being the same exact class as the JSON above
                #pragma warning disable IL2026
                    sidecar = (CICMMetadataType)xs.Deserialize(sr);
                #pragma warning restore IL2026

                    sr.Close();
                }
                catch(Exception ex)
                {
                    AaruConsole.ErrorWriteLine(UI.Incorrect_metadata_sidecar_file_not_continuing);
                    AaruConsole.DebugWriteLine(MODULE_NAME, $"{ex}");

                    return (int)ErrorNumber.InvalidSidecar;
                }
            }
            else
            {
                AaruConsole.ErrorWriteLine(UI.Could_not_find_metadata_sidecar);

                return (int)ErrorNumber.NoSuchFile;
            }
        }

        if(resumeFile != null)
        {
            if(File.Exists(resumeFile))
            {
                try
                {
                    if(resumeFile.EndsWith(".metadata.json", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var fs = new FileStream(resumeFile, FileMode.Open);

                        resume =
                            (JsonSerializer.Deserialize(fs, typeof(ResumeJson),
                                                        ResumeJsonContext.Default) as ResumeJson)?.Resume;

                        fs.Close();
                    }
                    else
                    {
                        // Bypassed by JSON source generator used above
                    #pragma warning disable IL2026
                        var xs = new XmlSerializer(typeof(Resume));
                    #pragma warning restore IL2026

                        var sr = new StreamReader(resumeFile);

                        // Bypassed by JSON source generator used above
                    #pragma warning disable IL2026
                        resume = (Resume)xs.Deserialize(sr);
                    #pragma warning restore IL2026

                        sr.Close();
                    }
                }
                catch(Exception ex)
                {
                    AaruConsole.ErrorWriteLine(UI.Incorrect_resume_file_not_continuing);
                    AaruConsole.DebugWriteLine(MODULE_NAME, $"{ex}");

                    return (int)ErrorNumber.InvalidResume;
                }
            }
            else
            {
                AaruConsole.ErrorWriteLine(UI.Could_not_find_resume_file);

                return (int)ErrorNumber.NoSuchFile;
            }
        }

        var     filtersList = new FiltersList();
        IFilter inputFilter = null;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Identifying_file_filter).IsIndeterminate();
            inputFilter = filtersList.GetFilter(inputPath);
        });

        if(inputFilter == null)
        {
            AaruConsole.ErrorWriteLine(UI.Cannot_open_specified_file);

            return (int)ErrorNumber.CannotOpenFile;
        }

        if(File.Exists(outputPath))
        {
            AaruConsole.ErrorWriteLine(UI.Output_file_already_exists);

            return (int)ErrorNumber.FileExists;
        }

        PluginBase  plugins     = PluginBase.Singleton;
        IMediaImage inputFormat = null;
        IBaseImage  baseImage   = null;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Identifying_image_format).IsIndeterminate();
            baseImage   = ImageFormat.Detect(inputFilter);
            inputFormat = baseImage as IMediaImage;
        });

        if(inputFormat == null)
        {
            AaruConsole.WriteLine(UI.Input_image_format_not_identified);

            return (int)ErrorNumber.UnrecognizedFormat;
        }

        // TODO: Implement
        if(inputFormat == null)
        {
            AaruConsole.WriteLine(UI.Command_not_yet_supported_for_this_image_type);

            return (int)ErrorNumber.InvalidArgument;
        }

        if(verbose)
            AaruConsole.VerboseWriteLine(UI.Input_image_format_identified_by_0_1, inputFormat.Name, inputFormat.Id);
        else
            AaruConsole.WriteLine(UI.Input_image_format_identified_by_0, inputFormat.Name);

        try
        {
            ErrorNumber opened = ErrorNumber.NoData;

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask(UI.Invoke_Opening_image_file).IsIndeterminate();
                opened = inputFormat.Open(inputFilter);
            });

            if(opened != ErrorNumber.NoError)
            {
                AaruConsole.WriteLine(UI.Unable_to_open_image_format);
                AaruConsole.WriteLine(Localization.Core.Error_0, opened);

                return (int)opened;
            }

            mediaType = inputFormat.Info.MediaType;

            // Obsolete types
        #pragma warning disable 612
            mediaType = mediaType switch
                        {
                            MediaType.SQ1500     => MediaType.SyJet,
                            MediaType.Bernoulli  => MediaType.Bernoulli10,
                            MediaType.Bernoulli2 => MediaType.BernoulliBox2_20,
                            _                    => inputFormat.Info.MediaType
                        };
        #pragma warning restore 612

            AaruConsole.DebugWriteLine(MODULE_NAME, UI.Correctly_opened_image_file);

            AaruConsole.DebugWriteLine(MODULE_NAME, UI.Image_without_headers_is_0_bytes,
                                       inputFormat.Info.ImageSize);

            AaruConsole.DebugWriteLine(MODULE_NAME, UI.Image_has_0_sectors, inputFormat.Info.Sectors);

            AaruConsole.DebugWriteLine(MODULE_NAME, UI.Image_identifies_media_type_as_0, mediaType);

            Statistics.AddMediaFormat(inputFormat.Format);
            Statistics.AddMedia(mediaType, false);
            Statistics.AddFilter(inputFilter.Name);
        }
        catch(Exception ex)
        {
            AaruConsole.ErrorWriteLine(UI.Unable_to_open_image_format);
            AaruConsole.ErrorWriteLine(Localization.Core.Error_0, ex.Message);
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Stack_trace_0, ex.StackTrace);

            return (int)ErrorNumber.CannotOpenFormat;
        }

        List<IBaseWritableImage> candidates = new();

        // Try extension
        if(string.IsNullOrEmpty(format))
        {
            candidates.AddRange(from pluginType in plugins.WritableImages.Values
                                select Activator.CreateInstance(pluginType) as IBaseWritableImage
                                into plugin
                                where plugin is not null
                                where plugin.KnownExtensions.Contains(Path.GetExtension(outputPath))
                                select plugin);
        }

        // Try Id
        else if(Guid.TryParse(format, out Guid outId))
        {
            candidates.AddRange(from pluginType in plugins.WritableImages.Values
                                select Activator.CreateInstance(pluginType) as IBaseWritableImage
                                into plugin
                                where plugin is not null
                                where plugin.Id.Equals(outId)
                                select plugin);
        }

        // Try name
        else
        {
            candidates.AddRange(from pluginType in plugins.WritableImages.Values
                                select Activator.CreateInstance(pluginType) as IBaseWritableImage
                                into plugin
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

        IBaseWritableImage outputFormat = candidates[0];

        if(verbose)
            AaruConsole.VerboseWriteLine(UI.Output_image_format_0_1, outputFormat.Name, outputFormat.Id);
        else
            AaruConsole.WriteLine(UI.Output_image_format_0, outputFormat.Name);

        if(!outputFormat.SupportedMediaTypes.Contains(mediaType))
        {
            AaruConsole.ErrorWriteLine(UI.Output_format_does_not_support_media_type);

            return (int)ErrorNumber.UnsupportedMedia;
        }

        foreach(MediaTagType mediaTag in inputFormat.Info.ReadableMediaTags.Where(mediaTag =>
                    !outputFormat.SupportedMediaTags.Contains(mediaTag) && !force))
        {
            AaruConsole.ErrorWriteLine(UI.Converting_image_will_lose_media_tag_0, mediaTag);
            AaruConsole.ErrorWriteLine(UI.If_you_dont_care_use_force_option);

            return (int)ErrorNumber.DataWillBeLost;
        }

        bool useLong = inputFormat.Info.ReadableSectorTags.Count != 0;

        foreach(SectorTagType sectorTag in inputFormat.Info.ReadableSectorTags.Where(sectorTag =>
                    !outputFormat.SupportedSectorTags.Contains(sectorTag)))
        {
            if(force)
            {
                if(sectorTag != SectorTagType.CdTrackFlags &&
                   sectorTag != SectorTagType.CdTrackIsrc  &&
                   sectorTag != SectorTagType.CdSectorSubchannel)
                    useLong = false;

                continue;
            }

            AaruConsole.ErrorWriteLine(UI.Converting_image_will_lose_sector_tag_0, sectorTag);

            AaruConsole.ErrorWriteLine(UI.
                                           If_you_dont_care_use_force_option_This_will_skip_all_sector_tags_converting_only_user_data);

            return (int)ErrorNumber.DataWillBeLost;
        }

        var inputTape  = inputFormat as ITapeImage;
        var outputTape = outputFormat as IWritableTapeImage;

        if(inputTape?.IsTape == true &&
           outputTape is null)
        {
            AaruConsole.ErrorWriteLine(UI.Input_format_contains_a_tape_image_and_is_not_supported_by_output_format);

            return (int)ErrorNumber.UnsupportedMedia;
        }

        var ret = false;

        if(inputTape?.IsTape == true &&
           outputTape        != null)
        {
            ret = outputTape.SetTape();

            // Cannot set image to tape mode
            if(!ret)
            {
                AaruConsole.ErrorWriteLine(UI.Error_setting_output_image_in_tape_mode);
                AaruConsole.ErrorWriteLine(outputFormat.ErrorMessage);

                return (int)ErrorNumber.WriteError;
            }
        }

        if((outputFormat as IWritableOpticalImage)?.OpticalCapabilities.HasFlag(OpticalImageCapabilities.
               CanStoreSessions) != true &&
           (inputFormat as IOpticalMediaImage)?.Sessions?.Count > 1)
        {
            // TODO: Disabled until 6.0
            /*if(!_force)
            {*/
            AaruConsole.ErrorWriteLine(Localization.Core.Output_format_does_not_support_sessions);

            return (int)ErrorNumber.UnsupportedMedia;
            /*}

            AaruConsole.ErrorWriteLine("Output format does not support sessions, this will end in a loss of data, continuing...");*/
        }

        if((outputFormat as IWritableOpticalImage)?.OpticalCapabilities.HasFlag(OpticalImageCapabilities.
               CanStoreHiddenTracks) != true &&
           (inputFormat as IOpticalMediaImage)?.Tracks?.Any(t => t.Sequence == 0) == true)
        {
            // TODO: Disabled until 6.0
            /*if(!_force)
            {*/
            AaruConsole.ErrorWriteLine(Localization.Core.Output_format_does_not_support_hidden_tracks);

            return (int)ErrorNumber.UnsupportedMedia;
            /*}

            AaruConsole.ErrorWriteLine("Output format does not support sessions, this will end in a loss of data, continuing...");*/
        }

        var created = false;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Invoke_Opening_image_file).IsIndeterminate();

            created = outputFormat.Create(outputPath, mediaType, parsedOptions,
                                          inputFormat.Info.Sectors,
                                          inputFormat.Info.SectorSize);
        });

        if(!created)
        {
            AaruConsole.ErrorWriteLine(UI.Error_0_creating_output_image, outputFormat.ErrorMessage);

            return (int)ErrorNumber.CannotCreateFormat;
        }

        var imageInfo = new ImageInfo
        {
            Application           = "Aaru",
            ApplicationVersion    = Version.GetVersion(),
            Comments              = comments              ?? inputFormat.Info.Comments,
            Creator               = creator               ?? inputFormat.Info.Creator,
            DriveFirmwareRevision = driveFirmwareRevision ?? inputFormat.Info.DriveFirmwareRevision,
            DriveManufacturer     = driveManufacturer     ?? inputFormat.Info.DriveManufacturer,
            DriveModel            = driveModel            ?? inputFormat.Info.DriveModel,
            DriveSerialNumber     = driveSerialNumber     ?? inputFormat.Info.DriveSerialNumber,
            LastMediaSequence     = lastMediaSequence != 0 ? lastMediaSequence : inputFormat.Info.LastMediaSequence,
            MediaBarcode          = mediaBarcode      ?? inputFormat.Info.MediaBarcode,
            MediaManufacturer     = mediaManufacturer ?? inputFormat.Info.MediaManufacturer,
            MediaModel            = mediaModel        ?? inputFormat.Info.MediaModel,
            MediaPartNumber       = mediaPartNumber   ?? inputFormat.Info.MediaPartNumber,
            MediaSequence         = mediaSequence != 0 ? mediaSequence : inputFormat.Info.MediaSequence,
            MediaSerialNumber     = mediaSerialNumber ?? inputFormat.Info.MediaSerialNumber,
            MediaTitle            = mediaTitle        ?? inputFormat.Info.MediaTitle
        };

        if(!outputFormat.SetImageInfo(imageInfo))
        {
            if(!force)
            {
                AaruConsole.ErrorWriteLine(UI.Error_0_setting_metadata_not_continuing, outputFormat.ErrorMessage);

                return (int)ErrorNumber.WriteError;
            }

            AaruConsole.ErrorWriteLine(Localization.Core.Error_0_setting_metadata, outputFormat.ErrorMessage);
        }

        Metadata           metadata     = inputFormat.AaruMetadata;
        List<DumpHardware> dumpHardware = inputFormat.DumpHardware;

        foreach(MediaTagType mediaTag in inputFormat.Info.ReadableMediaTags.Where(mediaTag =>
                    !force || outputFormat.SupportedMediaTags.Contains(mediaTag)))
        {
            ErrorNumber errorNumber = ErrorNumber.NoError;

            AnsiConsole.Progress().AutoClear(false).HideCompleted(false).
                        Columns(new TaskDescriptionColumn(), new SpinnerColumn()).Start(ctx =>
                        {
                            ctx.AddTask(string.Format(UI.Converting_media_tag_0,
                                                      Markup.Escape(mediaTag.ToString())));
                            ErrorNumber errno = inputFormat.ReadMediaTag(mediaTag, out byte[] tag);

                            if(errno != ErrorNumber.NoError)
                            {
                                if(force)
                                    AaruConsole.ErrorWriteLine(UI.Error_0_reading_media_tag, errno);
                                else
                                {
                                    AaruConsole.ErrorWriteLine(UI.Error_0_reading_media_tag_not_continuing,
                                                               errno);

                                    errorNumber = errno;
                                }

                                return;
                            }

                            if((outputFormat as IWritableImage)?.WriteMediaTag(tag, mediaTag) == true)
                                return;

                            if(force)
                            {
                                AaruConsole.ErrorWriteLine(UI.Error_0_writing_media_tag,
                                                           outputFormat.ErrorMessage);
                            }
                            else
                            {
                                AaruConsole.ErrorWriteLine(UI.Error_0_writing_media_tag_not_continuing,
                                                           outputFormat.ErrorMessage);

                                errorNumber = ErrorNumber.WriteError;
                            }
                        });

            if(errorNumber != ErrorNumber.NoError)
                return (int)errorNumber;
        }

        AaruConsole.WriteLine(UI._0_sectors_to_convert, inputFormat.Info.Sectors);
        ulong doneSectors = 0;

        if(inputFormat is IOpticalMediaImage inputOptical      &&
           outputFormat is IWritableOpticalImage outputOptical &&
           inputOptical.Tracks != null)
        {
            if(!outputOptical.SetTracks(inputOptical.Tracks))
            {
                AaruConsole.ErrorWriteLine(UI.Error_0_sending_tracks_list_to_output_image, outputOptical.ErrorMessage);

                return (int)ErrorNumber.WriteError;
            }

            ErrorNumber errno = ErrorNumber.NoError;

            if(decrypt)
                AaruConsole.WriteLine("Decrypting encrypted sectors.");

            AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
                        Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn()).
                        Start(ctx =>
                        {
                            ProgressTask discTask = ctx.AddTask(UI.Converting_disc);
                            discTask.MaxValue = inputOptical.Tracks.Count;
                            byte[] generatedTitleKeys = null;

                            foreach(Track track in inputOptical.Tracks)
                            {
                                discTask.Description = string.Format(UI.Converting_sectors_in_track_0_of_1,
                                                                     discTask.Value + 1, discTask.MaxValue);

                                doneSectors = 0;
                                ulong trackSectors = track.EndSector - track.StartSector + 1;

                                ProgressTask trackTask = ctx.AddTask(UI.Converting_track);
                                trackTask.MaxValue = trackSectors;

                                while(doneSectors < trackSectors)
                                {
                                    byte[] sector;

                                    uint sectorsToDo;

                                    if(trackSectors - doneSectors >= (ulong)count)
                                        sectorsToDo = (uint)count;
                                    else
                                        sectorsToDo = (uint)(trackSectors - doneSectors);

                                    trackTask.Description =
                                        string.Format(UI.Converting_sectors_0_to_1_in_track_2,
                                                      doneSectors + track.StartSector,
                                                      doneSectors + sectorsToDo + track.StartSector,
                                                      track.Sequence);

                                    var useNotLong = false;
                                    var result     = false;

                                    if(useLong)
                                    {
                                        errno = sectorsToDo == 1
                                                    ? inputOptical.ReadSectorLong(doneSectors + track.StartSector,
                                                        out sector)
                                                    : inputOptical.
                                                        ReadSectorsLong(doneSectors + track.StartSector,
                                                                        sectorsToDo, out sector);

                                        if(errno == ErrorNumber.NoError)
                                        {
                                            result = sectorsToDo == 1
                                                         ? outputOptical.WriteSectorLong(sector,
                                                             doneSectors + track.StartSector)
                                                         : outputOptical.WriteSectorsLong(sector,
                                                             doneSectors + track.StartSector, sectorsToDo);
                                        }
                                        else
                                        {
                                            result = true;

                                            if(force)
                                            {
                                                AaruConsole.ErrorWriteLine(UI.Error_0_reading_sector_1_continuing,
                                                                           errno, doneSectors + track.StartSector);
                                            }
                                            else
                                            {
                                                AaruConsole.
                                                    ErrorWriteLine(UI.Error_0_reading_sector_1_not_continuing,
                                                                   errno, doneSectors + track.StartSector);

                                                errno = ErrorNumber.WriteError;

                                                return;
                                            }
                                        }

                                        if(!result &&
                                           sector.Length % 2352 != 0)
                                        {
                                            if(!force)
                                            {
                                                AaruConsole.
                                                    ErrorWriteLine(UI.
                                                                       Input_image_is_not_returning_raw_sectors_use_force_if_you_want_to_continue);

                                                errno = ErrorNumber.InOutError;

                                                return;
                                            }

                                            useNotLong = true;
                                        }
                                    }

                                    if(!useLong || useNotLong)
                                    {
                                        errno = sectorsToDo == 1
                                                    ? inputOptical.ReadSector(doneSectors + track.StartSector,
                                                                              out sector)
                                                    : inputOptical.ReadSectors(doneSectors + track.StartSector,
                                                                               sectorsToDo, out sector);

                                        // TODO: Move to generic place when anything but CSS DVDs can be decrypted 
                                        if(inputOptical.Info.MediaType is MediaType.DVDROM or MediaType.DVDR
                                            or MediaType.DVDRDL or MediaType.DVDPR or MediaType.DVDPRDL &&
                                           decrypt)
                                        {
                                            // Only sectors which are MPEG packets can be encrypted.
                                            if(MPEG.ContainsMpegPackets(sector, sectorsToDo))
                                            {
                                                byte[] cmi, titleKey;

                                                if(sectorsToDo == 1)
                                                {
                                                    if(inputOptical.ReadSectorTag(doneSectors + track.StartSector,
                                                           SectorTagType.DvdSectorCmi, out cmi) ==
                                                       ErrorNumber.NoError &&
                                                       inputOptical.ReadSectorTag(doneSectors + track.StartSector,
                                                           SectorTagType.DvdTitleKeyDecrypted,
                                                           out titleKey) ==
                                                       ErrorNumber.NoError)
                                                        sector = CSS.DecryptSector(sector, titleKey, cmi);
                                                    else
                                                    {
                                                        if(generatedTitleKeys == null)
                                                        {
                                                            List<Partition> partitions =
                                                                Core.Partitions.GetAll(inputOptical);

                                                            partitions = partitions.FindAll(p =>
                                                            {
                                                                Core.Filesystems.
                                                                     Identify(inputOptical,
                                                                              out List<string>
                                                                                  idPlugins, p);

                                                                return idPlugins.
                                                                    Contains("iso9660 filesystem");
                                                            });

                                                            if(plugins.ReadOnlyFilesystems.
                                                                       TryGetValue("iso9660 filesystem",
                                                                           out Type pluginType))
                                                            {
                                                                AaruConsole.DebugWriteLine(MODULE_NAME,
                                                                    UI.Generating_decryption_keys);

                                                                generatedTitleKeys =
                                                                    CSS.GenerateTitleKeys(inputOptical,
                                                                        partitions, trackSectors,
                                                                        pluginType);
                                                            }
                                                        }

                                                        if(generatedTitleKeys != null)
                                                        {
                                                            sector = CSS.DecryptSector(sector,
                                                                generatedTitleKeys.
                                                                    Skip((int)(5 * (doneSectors +
                                                                                           track.
                                                                                               StartSector))).
                                                                    Take(5).ToArray(), null);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if(inputOptical.
                                                           ReadSectorsTag(doneSectors + track.StartSector,
                                                                          sectorsToDo,
                                                                          SectorTagType.DvdSectorCmi,
                                                                          out cmi) ==
                                                       ErrorNumber.NoError &&
                                                       inputOptical.
                                                           ReadSectorsTag(doneSectors + track.StartSector,
                                                                          sectorsToDo,
                                                                          SectorTagType.DvdTitleKeyDecrypted,
                                                                          out titleKey) ==
                                                       ErrorNumber.NoError)
                                                    {
                                                        sector = CSS.DecryptSector(sector, titleKey, cmi,
                                                            sectorsToDo);
                                                    }
                                                    else
                                                    {
                                                        if(generatedTitleKeys == null)
                                                        {
                                                            List<Partition> partitions =
                                                                Core.Partitions.GetAll(inputOptical);

                                                            partitions = partitions.FindAll(p =>
                                                            {
                                                                Core.Filesystems.
                                                                     Identify(inputOptical,
                                                                              out List<string>
                                                                                  idPlugins, p);

                                                                return idPlugins.
                                                                    Contains("iso9660 filesystem");
                                                            });

                                                            if(plugins.ReadOnlyFilesystems.
                                                                       TryGetValue("iso9660 filesystem",
                                                                           out Type pluginType))
                                                            {
                                                                AaruConsole.DebugWriteLine(MODULE_NAME,
                                                                    UI.Generating_decryption_keys);

                                                                generatedTitleKeys =
                                                                    CSS.GenerateTitleKeys(inputOptical,
                                                                        partitions, trackSectors,
                                                                        pluginType);
                                                            }
                                                        }

                                                        if(generatedTitleKeys != null)
                                                        {
                                                            sector = CSS.DecryptSector(sector,
                                                                generatedTitleKeys.
                                                                    Skip((int)(5 * (doneSectors +
                                                                                           track.
                                                                                               StartSector))).
                                                                    Take((int)(5 * sectorsToDo)).ToArray(),
                                                                null,
                                                                sectorsToDo);
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        if(errno == ErrorNumber.NoError)
                                        {
                                            result = sectorsToDo == 1
                                                         ? outputOptical.WriteSector(sector,
                                                             doneSectors + track.StartSector)
                                                         : outputOptical.WriteSectors(sector,
                                                             doneSectors + track.StartSector, sectorsToDo);
                                        }
                                        else
                                        {
                                            result = true;

                                            if(force)
                                            {
                                                AaruConsole.ErrorWriteLine(UI.Error_0_reading_sector_1_continuing,
                                                                           errno, doneSectors + track.StartSector);
                                            }
                                            else
                                            {
                                                AaruConsole.
                                                    ErrorWriteLine(UI.Error_0_reading_sector_1_not_continuing,
                                                                   errno, doneSectors + track.StartSector);

                                                errno = ErrorNumber.WriteError;

                                                return;
                                            }
                                        }
                                    }

                                    if(!result)
                                    {
                                        if(force)
                                        {
                                            AaruConsole.ErrorWriteLine(UI.Error_0_writing_sector_1_continuing,
                                                                       outputOptical.ErrorMessage,
                                                                       doneSectors + track.StartSector);
                                        }
                                        else
                                        {
                                            AaruConsole.ErrorWriteLine(UI.Error_0_writing_sector_1_not_continuing,
                                                                       outputOptical.ErrorMessage,
                                                                       doneSectors + track.StartSector);

                                            errno = ErrorNumber.WriteError;

                                            return;
                                        }
                                    }

                                    doneSectors     += sectorsToDo;
                                    trackTask.Value += sectorsToDo;
                                }

                                trackTask.StopTask();
                                discTask.Increment(1);
                            }
                        });

            if(errno != ErrorNumber.NoError)
                return (int)errno;

            Dictionary<byte, string> isrcs                     = new();
            Dictionary<byte, byte>   trackFlags                = new();
            string                   mcn                       = null;
            HashSet<int>             subchannelExtents         = new();
            Dictionary<byte, int>    smallestPregapLbaPerTrack = new();
            var                      tracks                    = new Track[inputOptical.Tracks.Count];

            for(var i = 0; i < tracks.Length; i++)
            {
                tracks[i] = new Track
                {
                    Indexes           = new Dictionary<ushort, int>(),
                    Description       = inputOptical.Tracks[i].Description,
                    EndSector         = inputOptical.Tracks[i].EndSector,
                    StartSector       = inputOptical.Tracks[i].StartSector,
                    Pregap            = inputOptical.Tracks[i].Pregap,
                    Sequence          = inputOptical.Tracks[i].Sequence,
                    Session           = inputOptical.Tracks[i].Session,
                    BytesPerSector    = inputOptical.Tracks[i].BytesPerSector,
                    RawBytesPerSector = inputOptical.Tracks[i].RawBytesPerSector,
                    Type              = inputOptical.Tracks[i].Type,
                    SubchannelType    = inputOptical.Tracks[i].SubchannelType
                };

                foreach(KeyValuePair<ushort, int> idx in inputOptical.Tracks[i].Indexes)
                    tracks[i].Indexes[idx.Key] = idx.Value;
            }

            foreach(SectorTagType tag in inputOptical.Info.ReadableSectorTags.
                                                      Where(t => t == SectorTagType.CdTrackIsrc).OrderBy(t => t))
            {
                foreach(Track track in tracks)
                {
                    errno = inputOptical.ReadSectorTag(track.Sequence, tag, out byte[] isrc);

                    if(errno != ErrorNumber.NoError)
                        continue;

                    isrcs[(byte)track.Sequence] = Encoding.UTF8.GetString(isrc);
                }
            }

            foreach(SectorTagType tag in inputOptical.Info.ReadableSectorTags.
                                                      Where(t => t == SectorTagType.CdTrackFlags).OrderBy(t => t))
            {
                foreach(Track track in tracks)
                {
                    errno = inputOptical.ReadSectorTag(track.Sequence, tag, out byte[] flags);

                    if(errno != ErrorNumber.NoError)
                        continue;

                    trackFlags[(byte)track.Sequence] = flags[0];
                }
            }

            for(ulong s = 0; s < inputOptical.Info.Sectors; s++)
            {
                if(s > int.MaxValue)
                    break;

                subchannelExtents.Add((int)s);
            }

            foreach(SectorTagType tag in inputOptical.Info.ReadableSectorTags.OrderBy(t => t).TakeWhile(_ => useLong))
            {
                switch(tag)
                {
                    case SectorTagType.AppleSectorTag:
                    case SectorTagType.CdSectorSync:
                    case SectorTagType.CdSectorHeader:
                    case SectorTagType.CdSectorSubHeader:
                    case SectorTagType.CdSectorEdc:
                    case SectorTagType.CdSectorEccP:
                    case SectorTagType.CdSectorEccQ:
                    case SectorTagType.CdSectorEcc:
                    case SectorTagType.DvdSectorCmi:
                    case SectorTagType.DvdSectorTitleKey:
                    case SectorTagType.DvdSectorEdc:
                    case SectorTagType.DvdSectorIed:
                    case SectorTagType.DvdSectorInformation:
                    case SectorTagType.DvdSectorNumber:
                        // This tags are inline in long sector
                        continue;
                }

                if(force && !outputOptical.SupportedSectorTags.Contains(tag))
                    continue;

                errno = ErrorNumber.NoError;

                AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
                            Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn()).
                            Start(ctx =>
                            {
                                ProgressTask discTask = ctx.AddTask(UI.Converting_disc);
                                discTask.MaxValue = inputOptical.Tracks.Count;

                                foreach(Track track in inputOptical.Tracks)
                                {
                                    discTask.Description =
                                        string.Format(UI.Converting_tags_in_track_0_of_1, discTask.Value + 1,
                                                      discTask.MaxValue);

                                    doneSectors = 0;
                                    ulong  trackSectors = track.EndSector - track.StartSector + 1;
                                    byte[] sector;
                                    bool   result;

                                    switch(tag)
                                    {
                                        case SectorTagType.CdTrackFlags:
                                        case SectorTagType.CdTrackIsrc:
                                            errno = inputOptical.ReadSectorTag(track.Sequence, tag, out sector);

                                            switch(errno)
                                            {
                                                case ErrorNumber.NoData:
                                                    errno = ErrorNumber.NoError;

                                                    continue;
                                                case ErrorNumber.NoError:
                                                    result = outputOptical.WriteSectorTag(sector, track.Sequence,
                                                        tag);

                                                    break;
                                                default:
                                                {
                                                    if(force)
                                                    {
                                                        AaruConsole.
                                                            ErrorWriteLine(UI.Error_0_writing_tag_continuing,
                                                                           outputOptical.ErrorMessage);

                                                        continue;
                                                    }

                                                    AaruConsole.
                                                        ErrorWriteLine(UI.Error_0_writing_tag_not_continuing,
                                                                       outputOptical.ErrorMessage);

                                                    errno = ErrorNumber.WriteError;

                                                    return;
                                                }
                                            }

                                            if(!result)
                                            {
                                                if(force)
                                                {
                                                    AaruConsole.ErrorWriteLine(UI.Error_0_writing_tag_continuing,
                                                                               outputOptical.ErrorMessage);
                                                }
                                                else
                                                {
                                                    AaruConsole.
                                                        ErrorWriteLine(UI.Error_0_writing_tag_not_continuing,
                                                                       outputOptical.ErrorMessage);

                                                    errno = ErrorNumber.WriteError;

                                                    return;
                                                }
                                            }

                                            continue;
                                    }

                                    ProgressTask trackTask = ctx.AddTask(UI.Converting_track);
                                    trackTask.MaxValue = trackSectors;

                                    while(doneSectors < trackSectors)
                                    {
                                        uint sectorsToDo;

                                        if(trackSectors - doneSectors >= (ulong)count)
                                            sectorsToDo = (uint)count;
                                        else
                                            sectorsToDo = (uint)(trackSectors - doneSectors);

                                        trackTask.Description =
                                            string.Format(UI.Converting_tag_3_for_sectors_0_to_1_in_track_2,
                                                          doneSectors + track.StartSector,
                                                          doneSectors + sectorsToDo + track.StartSector,
                                                          track.Sequence,
                                                          tag);

                                        if(sectorsToDo == 1)
                                        {
                                            errno = inputOptical.ReadSectorTag(doneSectors + track.StartSector,
                                                                               tag,
                                                                               out sector);

                                            if(errno == ErrorNumber.NoError)
                                            {
                                                if(tag == SectorTagType.CdSectorSubchannel)
                                                {
                                                    bool indexesChanged =
                                                        CompactDisc.WriteSubchannelToImage(MmcSubchannel.Raw,
                                                            MmcSubchannel.Raw, sector,
                                                            doneSectors + track.StartSector, 1, null, isrcs,
                                                            (byte)track.Sequence, ref mcn, tracks,
                                                            subchannelExtents, fixSubchannelPosition,
                                                            outputOptical, fixSubchannel, fixSubchannelCrc,
                                                            null,
                                                            null, smallestPregapLbaPerTrack, false, out _);

                                                    if(indexesChanged)
                                                        outputOptical.SetTracks(tracks.ToList());

                                                    result = true;
                                                }
                                                else
                                                {
                                                    result =
                                                        outputOptical.WriteSectorTag(sector,
                                                            doneSectors + track.StartSector, tag);
                                                }
                                            }
                                            else
                                            {
                                                result = true;

                                                if(force)
                                                {
                                                    AaruConsole.
                                                        ErrorWriteLine(UI.Error_0_reading_tag_for_sector_1_continuing,
                                                                       errno, doneSectors + track.StartSector);
                                                }
                                                else
                                                {
                                                    AaruConsole.
                                                        ErrorWriteLine(
                                                            UI.Error_0_reading_tag_for_sector_1_not_continuing,
                                                            errno, doneSectors + track.StartSector);

                                                    return;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            errno = inputOptical.ReadSectorsTag(doneSectors + track.StartSector,
                                                sectorsToDo, tag, out sector);

                                            if(errno == ErrorNumber.NoError)
                                            {
                                                if(tag == SectorTagType.CdSectorSubchannel)
                                                {
                                                    bool indexesChanged =
                                                        CompactDisc.WriteSubchannelToImage(MmcSubchannel.Raw,
                                                            MmcSubchannel.Raw, sector,
                                                            doneSectors + track.StartSector, sectorsToDo,
                                                            null,
                                                            isrcs, (byte)track.Sequence, ref mcn, tracks,
                                                            subchannelExtents, fixSubchannelPosition,
                                                            outputOptical, fixSubchannel, fixSubchannelCrc,
                                                            null,
                                                            null, smallestPregapLbaPerTrack, false, out _);

                                                    if(indexesChanged)
                                                        outputOptical.SetTracks(tracks.ToList());

                                                    result = true;
                                                }
                                                else
                                                {
                                                    result =
                                                        outputOptical.WriteSectorsTag(sector,
                                                            doneSectors + track.StartSector, sectorsToDo,
                                                            tag);
                                                }
                                            }
                                            else
                                            {
                                                result = true;

                                                if(force)
                                                {
                                                    AaruConsole.
                                                        ErrorWriteLine(UI.Error_0_reading_tag_for_sector_1_continuing,
                                                                       errno, doneSectors + track.StartSector);
                                                }
                                                else
                                                {
                                                    AaruConsole.
                                                        ErrorWriteLine(
                                                            UI.Error_0_reading_tag_for_sector_1_not_continuing,
                                                            errno, doneSectors + track.StartSector);

                                                    return;
                                                }
                                            }
                                        }

                                        if(!result)
                                        {
                                            if(force)
                                            {
                                                AaruConsole.
                                                    ErrorWriteLine(UI.Error_0_writing_tag_for_sector_1_continuing,
                                                                   outputOptical.ErrorMessage,
                                                                   doneSectors + track.StartSector);
                                            }
                                            else
                                            {
                                                AaruConsole.
                                                    ErrorWriteLine(UI.Error_0_writing_tag_for_sector_1_not_continuing,
                                                                   outputOptical.ErrorMessage,
                                                                   doneSectors + track.StartSector);

                                                errno = ErrorNumber.WriteError;

                                                return;
                                            }
                                        }

                                        doneSectors     += sectorsToDo;
                                        trackTask.Value += sectorsToDo;
                                    }

                                    trackTask.StopTask();
                                    discTask.Increment(1);
                                }
                            });

                if(errno != ErrorNumber.NoError &&
                   !force)
                    return (int)errno;
            }

            if(isrcs.Count > 0)
            {
                foreach(KeyValuePair<byte, string> isrc in isrcs)
                {
                    outputOptical.WriteSectorTag(Encoding.UTF8.GetBytes(isrc.Value), isrc.Key,
                                                 SectorTagType.CdTrackIsrc);
                }
            }

            if(trackFlags.Count > 0)
            {
                foreach((byte track, byte flags) in trackFlags)
                    outputOptical.WriteSectorTag(new[] { flags }, track, SectorTagType.CdTrackFlags);
            }

            if(mcn != null)
                outputOptical.WriteMediaTag(Encoding.UTF8.GetBytes(mcn), MediaTagType.CD_MCN);

            // TODO: Progress
            if(inputOptical.Info.MediaType is MediaType.CD or MediaType.CDDA or MediaType.CDG or MediaType.CDEG
                                           or MediaType.CDI or MediaType.CDROM or MediaType.CDROMXA or MediaType.CDPLUS
                                           or MediaType.CDMO
                                           or MediaType.CDR or MediaType.CDRW or MediaType.CDMRW or MediaType.VCD
                                           or MediaType.SVCD
                                           or MediaType.PCD or MediaType.DTSCD or MediaType.CDMIDI or MediaType.CDV
                                           or MediaType.CDIREADY
                                           or MediaType.FMTOWNS or MediaType.PS1CD or MediaType.PS2CD
                                           or MediaType.MEGACD or MediaType.SATURNCD
                                           or MediaType.GDROM or MediaType.GDR or MediaType.MilCD
                                           or MediaType.SuperCDROM2 or MediaType.JaguarCD
                                           or MediaType.ThreeDO or MediaType.PCFX or MediaType.NeoGeoCD
                                           or MediaType.CDTV or MediaType.CD32
                                           or MediaType.Playdia or MediaType.Pippin or MediaType.VideoNow
                                           or MediaType.VideoNowColor
                                           or MediaType.VideoNowXp or MediaType.CVD && generateSubchannels)
            {
                Core.Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask(Localization.Core.Generating_subchannels).
                        IsIndeterminate();

                    CompactDisc.GenerateSubchannels(subchannelExtents, tracks,
                                                    trackFlags, inputOptical.Info.Sectors,
                                                    null, null, null, null, null, outputOptical);
                });
            }
        }
        else
        {
            var outputMedia = outputFormat as IWritableImage;

            if(inputTape  == null ||
               outputTape == null ||
               !inputTape.IsTape)
            {
                (uint cylinders, uint heads, uint sectors) chs =
                    geometryValues != null
                        ? (geometryValues.Value.cylinders, geometryValues.Value.heads, geometryValues.Value.sectors)
                        : (inputFormat.Info.Cylinders, inputFormat.Info.Heads, inputFormat.Info.SectorsPerTrack);

                AaruConsole.WriteLine(UI.Setting_geometry_to_0_cylinders_1_heads_and_2_sectors_per_track, chs.cylinders,
                                      chs.heads, chs.sectors);

                if(!outputMedia.SetGeometry(chs.cylinders, chs.heads, chs.sectors))
                {
                    AaruConsole.ErrorWriteLine(UI.Error_0_setting_geometry_image_may_be_incorrect_continuing,
                                               outputMedia.ErrorMessage);
                }
            }

            ErrorNumber errno = ErrorNumber.NoError;

            AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
                        Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn()).
                        Start(ctx =>
                        {
                            ProgressTask mediaTask = ctx.AddTask(UI.Converting_media);
                            mediaTask.MaxValue = inputFormat.Info.Sectors;

                            while(doneSectors < inputFormat.Info.Sectors)
                            {
                                byte[] sector;

                                uint sectorsToDo;

                                if(inputTape?.IsTape == true)
                                    sectorsToDo = 1;
                                else if(inputFormat.Info.Sectors - doneSectors >= (ulong)count)
                                    sectorsToDo = (uint)count;
                                else
                                    sectorsToDo = (uint)(inputFormat.Info.Sectors - doneSectors);

                                mediaTask.Description =
                                    string.Format(UI.Converting_sectors_0_to_1, doneSectors,
                                                  doneSectors + sectorsToDo);

                                bool result;

                                if(useLong)
                                {
                                    errno = sectorsToDo == 1
                                                ? inputFormat.ReadSectorLong(doneSectors, out sector)
                                                : inputFormat.ReadSectorsLong(doneSectors, sectorsToDo,
                                                                              out sector);

                                    if(errno == ErrorNumber.NoError)
                                    {
                                        result = sectorsToDo == 1
                                                     ? outputMedia.WriteSectorLong(sector, doneSectors)
                                                     : outputMedia.WriteSectorsLong(sector, doneSectors,
                                                         sectorsToDo);
                                    }
                                    else
                                    {
                                        result = true;

                                        if(force)
                                        {
                                            AaruConsole.ErrorWriteLine(UI.Error_0_reading_sector_1_continuing,
                                                                       errno,
                                                                       doneSectors);
                                        }
                                        else
                                        {
                                            AaruConsole.ErrorWriteLine(UI.Error_0_reading_sector_1_not_continuing,
                                                                       errno, doneSectors);

                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    errno = sectorsToDo == 1
                                                ? inputFormat.ReadSector(doneSectors, out sector)
                                                : inputFormat.ReadSectors(doneSectors, sectorsToDo, out sector);

                                    if(errno == ErrorNumber.NoError)
                                    {
                                        result = sectorsToDo == 1
                                                     ? outputMedia.WriteSector(sector, doneSectors)
                                                     : outputMedia.WriteSectors(sector, doneSectors, sectorsToDo);
                                    }
                                    else
                                    {
                                        result = true;

                                        if(force)
                                        {
                                            AaruConsole.ErrorWriteLine(UI.Error_0_reading_sector_1_continuing,
                                                                       errno,
                                                                       doneSectors);
                                        }
                                        else
                                        {
                                            AaruConsole.ErrorWriteLine(UI.Error_0_reading_sector_1_not_continuing,
                                                                       errno, doneSectors);

                                            return;
                                        }
                                    }
                                }

                                if(!result)
                                {
                                    if(force)
                                    {
                                        AaruConsole.ErrorWriteLine(UI.Error_0_writing_sector_1_continuing,
                                                                   outputMedia.ErrorMessage, doneSectors);
                                    }
                                    else
                                    {
                                        AaruConsole.ErrorWriteLine(UI.Error_0_writing_sector_1_not_continuing,
                                                                   outputMedia.ErrorMessage, doneSectors);

                                        errno = ErrorNumber.WriteError;

                                        return;
                                    }
                                }

                                doneSectors     += sectorsToDo;
                                mediaTask.Value += sectorsToDo;
                            }

                            mediaTask.StopTask();

                            foreach(SectorTagType tag in
                                    inputFormat.Info.ReadableSectorTags.TakeWhile(_ => useLong))
                            {
                                switch(tag)
                                {
                                    case SectorTagType.AppleSectorTag:
                                    case SectorTagType.CdSectorSync:
                                    case SectorTagType.CdSectorHeader:
                                    case SectorTagType.CdSectorSubHeader:
                                    case SectorTagType.CdSectorEdc:
                                    case SectorTagType.CdSectorEccP:
                                    case SectorTagType.CdSectorEccQ:
                                    case SectorTagType.CdSectorEcc:
                                        // This tags are inline in long sector
                                        continue;
                                }

                                if(force && !outputMedia.SupportedSectorTags.Contains(tag))
                                    continue;

                                doneSectors = 0;

                                ProgressTask tagsTask = ctx.AddTask(UI.Converting_tags);
                                tagsTask.MaxValue = inputFormat.Info.Sectors;

                                while(doneSectors < inputFormat.Info.Sectors)
                                {
                                    uint sectorsToDo;

                                    if(inputFormat.Info.Sectors - doneSectors >= (ulong)count)
                                        sectorsToDo = (uint)count;
                                    else
                                        sectorsToDo = (uint)(inputFormat.Info.Sectors - doneSectors);

                                    tagsTask.Description =
                                        string.Format(UI.Converting_tag_2_for_sectors_0_to_1, doneSectors,
                                                      doneSectors + sectorsToDo, tag);

                                    bool result;

                                    errno = sectorsToDo == 1
                                                ? inputFormat.ReadSectorTag(doneSectors, tag, out byte[] sector)
                                                : inputFormat.ReadSectorsTag(doneSectors, sectorsToDo, tag,
                                                                             out sector);

                                    if(errno == ErrorNumber.NoError)
                                    {
                                        result = sectorsToDo == 1
                                                     ? outputMedia.WriteSectorTag(sector, doneSectors, tag)
                                                     : outputMedia.WriteSectorsTag(sector, doneSectors,
                                                         sectorsToDo,
                                                         tag);
                                    }
                                    else
                                    {
                                        result = true;

                                        if(force)
                                        {
                                            AaruConsole.ErrorWriteLine(UI.Error_0_reading_sector_1_continuing,
                                                                       errno,
                                                                       doneSectors);
                                        }
                                        else
                                        {
                                            AaruConsole.ErrorWriteLine(UI.Error_0_reading_sector_1_not_continuing,
                                                                       errno, doneSectors);

                                            return;
                                        }
                                    }

                                    if(!result)
                                    {
                                        if(force)
                                        {
                                            AaruConsole.ErrorWriteLine(UI.Error_0_writing_sector_1_continuing,
                                                                       outputMedia.ErrorMessage, doneSectors);
                                        }
                                        else
                                        {
                                            AaruConsole.ErrorWriteLine(UI.Error_0_writing_sector_1_not_continuing,
                                                                       outputMedia.ErrorMessage, doneSectors);

                                            errno = ErrorNumber.WriteError;

                                            return;
                                        }
                                    }

                                    doneSectors    += sectorsToDo;
                                    tagsTask.Value += sectorsToDo;
                                }

                                tagsTask.StopTask();
                            }

                            if(inputFormat is IFluxImage inputFlux &&
                               outputFormat is IWritableFluxImage outputFlux)
                            {
                                for(ushort track = 0; track < inputFlux.Info.Cylinders; track++)
                                {
                                    for(uint head = 0; head < inputFlux.Info.Heads; head++)
                                    {
                                        ErrorNumber error =
                                            inputFlux.SubTrackLength(head, track, out byte subTrackLen);

                                        if(error != ErrorNumber.NoError)
                                            continue;

                                        for(byte subTrackIndex = 0; subTrackIndex < subTrackLen; subTrackIndex++)
                                        {
                                            error = inputFlux.CapturesLength(head, track, subTrackIndex,
                                                                             out uint capturesLen);

                                            if(error != ErrorNumber.NoError)
                                                continue;

                                            for(uint captureIndex = 0; captureIndex < capturesLen; captureIndex++)
                                            {
                                                inputFlux.ReadFluxCapture(head, track, subTrackIndex,
                                                                          captureIndex,
                                                                          out ulong indexResolution,
                                                                          out ulong dataResolution,
                                                                          out byte[] indexBuffer,
                                                                          out byte[] dataBuffer);

                                                outputFlux.WriteFluxCapture(indexResolution, dataResolution,
                                                                            indexBuffer, dataBuffer, head, track,
                                                                            subTrackIndex,
                                                                            captureIndex);
                                            }
                                        }
                                    }
                                }
                            }

                            if(inputTape  == null ||
                               outputTape == null ||
                               !inputTape.IsTape)
                                return;

                            ProgressTask filesTask = ctx.AddTask(UI.Converting_files);
                            filesTask.MaxValue = inputTape.Files.Count;

                            foreach(TapeFile tapeFile in inputTape.Files)
                            {
                                filesTask.Description =
                                    string.Format(UI.Converting_file_0_of_partition_1, tapeFile.File,
                                                  tapeFile.Partition);

                                outputTape.AddFile(tapeFile);
                                filesTask.Increment(1);
                            }

                            filesTask.StopTask();

                            ProgressTask partitionTask = ctx.AddTask(UI.Converting_files);
                            partitionTask.MaxValue = inputTape.TapePartitions.Count;

                            foreach(TapePartition tapePartition in inputTape.TapePartitions)
                            {
                                partitionTask.Description =
                                    string.Format(UI.Converting_tape_partition_0, tapePartition.Number);

                                outputTape.AddPartition(tapePartition);
                            }

                            partitionTask.StopTask();
                        });

            if(errno != ErrorNumber.NoError)
                return (int)errno;
        }

        if(resume       != null ||
           dumpHardware != null)
        {
            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask(UI.Writing_dump_hardware_list).IsIndeterminate();

                if(resume != null)
                    ret = outputFormat.SetDumpHardware(resume.Tries);
                else if(dumpHardware != null)
                    ret = outputFormat.SetDumpHardware(dumpHardware);
            });

            if(ret)
                AaruConsole.WriteLine(UI.Written_dump_hardware_list_to_output_image);
        }

        ret = false;

        if(sidecar  != null ||
           metadata != null)
        {
            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask(UI.Writing_metadata).IsIndeterminate();

                if(sidecar != null)
                    ret = outputFormat.SetMetadata(sidecar);
                else if(metadata != null)
                    ret = outputFormat.SetMetadata(metadata);
            });

            if(ret)
                AaruConsole.WriteLine(UI.Written_Aaru_Metadata_to_output_image);
        }

        var closed = false;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Closing_output_image).IsIndeterminate();
            closed = outputFormat.Close();
        });

        if(!closed)
        {
            AaruConsole.ErrorWriteLine(UI.Error_0_closing_output_image_Contents_are_not_correct,
                                       outputFormat.ErrorMessage);

            return (int)ErrorNumber.WriteError;
        }

        AaruConsole.WriteLine(UI.Conversion_done);

        return (int)ErrorNumber.NoError;
    }
}