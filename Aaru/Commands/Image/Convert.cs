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
// Copyright © 2011-2025 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Aaru.Core;
using Aaru.Core.Media;
using Aaru.Decryption.DVD;
using Aaru.Devices;
using Aaru.Localization;
using Aaru.Logging;
using Schemas;
using Spectre.Console;
using Spectre.Console.Cli;
using File = System.IO.File;
using ImageInfo = Aaru.CommonTypes.Structs.ImageInfo;
using MediaType = Aaru.CommonTypes.MediaType;
using Partition = Aaru.CommonTypes.Partition;
using TapeFile = Aaru.CommonTypes.Structs.TapeFile;
using TapePartition = Aaru.CommonTypes.Structs.TapePartition;
using Track = Aaru.CommonTypes.Structs.Track;
using Version = Aaru.CommonTypes.Interop.Version;

namespace Aaru.Commands.Image;

sealed class ConvertImageCommand : Command<ConvertImageCommand.Settings>
{
    const string MODULE_NAME = "Convert-image command";

    public override int Execute(CommandContext context, Settings settings)
    {
        MainClass.PrintCopyright();

        // Initialize subchannel fix flags with cascading dependencies
        bool fixSubchannel         = settings.FixSubchannel;
        bool fixSubchannelCrc      = settings.FixSubchannelCrc;
        bool fixSubchannelPosition = settings.FixSubchannelPosition;

        if(fixSubchannelCrc) fixSubchannel = true;

        if(fixSubchannel) fixSubchannelPosition = true;

        Statistics.AddCommand("convert-image");

        // Log all command parameters for debugging and auditing
        Dictionary<string, string> parsedOptions = Options.Parse(settings.Options);
        LogCommandParameters(settings, fixSubchannelPosition, fixSubchannel, fixSubchannelCrc, parsedOptions);

        // Validate sector count parameter
        if(settings.Count == 0)
        {
            AaruLogging.Error(UI.Need_to_specify_more_than_zero_sectors_to_copy_at_once);

            return (int)ErrorNumber.InvalidArgument;
        }

        // Parse and validate CHS geometry if specified
        (bool success, uint cylinders, uint heads, uint sectors)? geometryResult = ParseGeometry(settings.Geometry);
        (uint cylinders, uint heads, uint sectors)?               geometryValues = null;

        if(geometryResult.HasValue)
        {
            if(!geometryResult.Value.success) return (int)ErrorNumber.InvalidArgument;

            geometryValues = (geometryResult.Value.cylinders, geometryResult.Value.heads, geometryResult.Value.sectors);
        }

        // Load metadata and resume information from sidecar files
        Resume    resume  = null;
        Metadata  sidecar = null;
        MediaType mediaType;

        (bool success, Metadata sidecar, Resume resume) metadataResult =
            LoadMetadata(settings.AaruMetadata, settings.CicmXml, settings.ResumeFile);

        if(!metadataResult.success) return (int)ErrorNumber.InvalidArgument;

        sidecar = metadataResult.sidecar;
        resume  = metadataResult.resume;

        // Identify input file filter (determines file type handler)
        IFilter inputFilter = null;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Identifying_file_filter).IsIndeterminate();
            inputFilter = PluginRegister.Singleton.GetFilter(settings.InputPath);
        });

        if(inputFilter == null)
        {
            AaruLogging.Error(UI.Cannot_open_specified_file);

            return (int)ErrorNumber.CannotOpenFile;
        }

        // Verify output file doesn't already exist
        if(File.Exists(settings.OutputPath))
        {
            AaruLogging.Error(UI.Output_file_already_exists);

            return (int)ErrorNumber.FileExists;
        }

        // Identify input image format
        PluginRegister plugins     = PluginRegister.Singleton;
        IMediaImage    inputFormat = null;
        IBaseImage     baseImage   = null;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Identifying_image_format).IsIndeterminate();
            baseImage   = ImageFormat.Detect(inputFilter);
            inputFormat = baseImage as IMediaImage;
        });

        if(inputFormat == null)
        {
            AaruLogging.WriteLine(UI.Input_image_format_not_identified);

            return (int)ErrorNumber.UnrecognizedFormat;
        }

        // TODO: Implement
        if(inputFormat == null)
        {
            AaruLogging.WriteLine(UI.Command_not_yet_supported_for_this_image_type);

            return (int)ErrorNumber.InvalidArgument;
        }

        if(settings.Verbose)
            AaruLogging.Verbose(UI.Input_image_format_identified_by_0_1, inputFormat.Name, inputFormat.Id);
        else
            AaruLogging.WriteLine(UI.Input_image_format_identified_by_0, inputFormat.Name);

        uint nominalNegativeSectors = 0;
        uint nominalOverflowSectors = 0;

        try
        {
            // Open the input image file for reading
            ErrorNumber opened = ErrorNumber.NoData;

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask(UI.Invoke_Opening_image_file).IsIndeterminate();
                opened = inputFormat.Open(inputFilter);
            });

            if(opened != ErrorNumber.NoError)
            {
                AaruLogging.WriteLine(UI.Unable_to_open_image_format);
                AaruLogging.WriteLine(Localization.Core.Error_0, opened);

                return (int)opened;
            }

            nominalNegativeSectors = settings.IgnoreNegativeSectors ? 0 : inputFormat.Info.NegativeSectors;
            nominalOverflowSectors = settings.IgnoreOverflowSectors ? 0 : inputFormat.Info.OverflowSectors;

            // Get media type and handle obsolete type mappings for backwards compatibility
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

            AaruLogging.Debug(MODULE_NAME, UI.Correctly_opened_image_file);

            // Log image statistics for debugging
            AaruLogging.Debug(MODULE_NAME, UI.Image_without_headers_is_0_bytes, inputFormat.Info.ImageSize);

            AaruLogging.Debug(MODULE_NAME, UI.Image_has_0_sectors, inputFormat.Info.Sectors);

            AaruLogging.Debug(MODULE_NAME, UI.Image_identifies_media_type_as_0, mediaType);

            Statistics.AddMediaFormat(inputFormat.Format);
            Statistics.AddMedia(mediaType, false);
            Statistics.AddFilter(inputFilter.Name);
        }
        catch(Exception ex)
        {
            AaruLogging.Error(UI.Unable_to_open_image_format);
            AaruLogging.Error(Localization.Core.Error_0, ex.Message);
            AaruLogging.Exception(ex, Localization.Core.Error_0, ex.Message);

            return (int)ErrorNumber.CannotOpenFormat;
        }

        // Discover and load output format plugin
        IBaseWritableImage outputFormat = FindOutputFormat(plugins, settings.Format, settings.OutputPath);

        if(outputFormat == null) return (int)ErrorNumber.FormatNotFound;

        if(settings.Verbose)
            AaruLogging.Verbose(UI.Output_image_format_0_1, outputFormat.Name, outputFormat.Id);
        else
            AaruLogging.WriteLine(UI.Output_image_format_0, outputFormat.Name);

        // Validate that output format supports the media type and tags
        int mediaCapabilityResult =
            ValidateMediaCapabilities(outputFormat as IWritableImage, inputFormat, mediaType, settings);

        if(mediaCapabilityResult != (int)ErrorNumber.NoError) return mediaCapabilityResult;

        // Validate sector tags compatibility between formats
        bool useLong;

        int sectorTagValidationResult =
            ValidateSectorTags(outputFormat as IWritableImage, inputFormat, settings, out useLong);

        if(sectorTagValidationResult != (int)ErrorNumber.NoError) return sectorTagValidationResult;

        // Check and setup tape image support if needed
        var inputTape  = inputFormat as ITapeImage;
        var outputTape = outputFormat as IWritableTapeImage;

        int tapeValidationResult = ValidateTapeImage(inputTape, outputTape);

        if(tapeValidationResult != (int)ErrorNumber.NoError) return tapeValidationResult;

        var ret = false;

        int tapeSetupResult = SetupTapeImage(inputTape, outputTape, outputFormat as IWritableImage);

        if(tapeSetupResult != (int)ErrorNumber.NoError) return tapeSetupResult;

        // Validate optical media capabilities (sessions, hidden tracks, etc.)
        if((outputFormat as IWritableOpticalImage)?.OpticalCapabilities.HasFlag(OpticalImageCapabilities
                                                                                   .CanStoreSessions) !=
           true &&
           (inputFormat as IOpticalMediaImage)?.Sessions?.Count > 1)
        {
            // TODO: Disabled until 6.0
            /*if(!_force)
            {*/
            AaruLogging.Error(Localization.Core.Output_format_does_not_support_sessions);

            return (int)ErrorNumber.UnsupportedMedia;
            /*}

            AaruLogging.ErrorWriteLine("Output format does not support sessions, this will end in a loss of data, continuing...");*/
        }

        // Check for hidden tracks support in optical media
        if((outputFormat as IWritableOpticalImage)?.OpticalCapabilities.HasFlag(OpticalImageCapabilities
                                                                                   .CanStoreHiddenTracks) !=
           true &&
           (inputFormat as IOpticalMediaImage)?.Tracks?.Any(t => t.Sequence == 0) == true)
        {
            // TODO: Disabled until 6.0
            /*if(!_force)
            {*/
            AaruLogging.Error(Localization.Core.Output_format_does_not_support_hidden_tracks);

            return (int)ErrorNumber.UnsupportedMedia;
            /*}

            AaruLogging.ErrorWriteLine("Output format does not support sessions, this will end in a loss of data, continuing...");*/
        }

        // Create the output image file with appropriate settings
        int createResult = CreateOutputImage(outputFormat as IWritableImage,
                                             settings.OutputPath,
                                             mediaType,
                                             parsedOptions,
                                             inputFormat,
                                             nominalNegativeSectors,
                                             nominalOverflowSectors);

        if(createResult != (int)ErrorNumber.NoError) return createResult;

        // Set image metadata in the output file
        int imageInfoResult = SetImageMetadata(inputFormat, outputFormat as IWritableImage, settings);

        if(imageInfoResult != (int)ErrorNumber.NoError) return imageInfoResult;

        // Prepare metadata and dump hardware information
        Metadata           metadata     = inputFormat.AaruMetadata;
        List<DumpHardware> dumpHardware = inputFormat.DumpHardware;

        // Convert media tags from input to output format
        int tagConversionResult = ConvertMediaTags(inputFormat, outputFormat as IWritableImage, settings);

        if(tagConversionResult != (int)ErrorNumber.NoError) return tagConversionResult;

        AaruLogging.WriteLine(UI._0_sectors_to_convert, inputFormat.Info.Sectors);
        ulong doneSectors = 0;

        // Handle optical media conversion (with tracks, subchannels, etc.)
        if(inputFormat is IOpticalMediaImage inputOptical      &&
           outputFormat is IWritableOpticalImage outputOptical &&
           inputOptical.Tracks != null)
        {
            if(!outputOptical.SetTracks(inputOptical.Tracks))
            {
                AaruLogging.Error(UI.Error_0_sending_tracks_list_to_output_image, outputOptical.ErrorMessage);

                return (int)ErrorNumber.WriteError;
            }

            ErrorNumber errno = ErrorNumber.NoError;

            if(settings.Decrypt) AaruLogging.WriteLine("Decrypting encrypted sectors.");

            AnsiConsole.Progress()
                       .AutoClear(true)
                       .HideCompleted(true)
                       .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn())
                       .Start(ctx =>
                        {
                            ProgressTask discTask = ctx.AddTask(UI.Converting_disc);
                            discTask.MaxValue = inputOptical.Tracks.Count;
                            byte[] generatedTitleKeys = null;

                            foreach(Track track in inputOptical.Tracks)
                            {
                                discTask.Description = string.Format(UI.Converting_sectors_in_track_0_of_1,
                                                                     discTask.Value + 1,
                                                                     discTask.MaxValue);

                                doneSectors = 0;
                                ulong trackSectors = track.EndSector - track.StartSector + 1;

                                ProgressTask trackTask = ctx.AddTask(UI.Converting_track);
                                trackTask.MaxValue = trackSectors;

                                while(doneSectors < trackSectors)
                                {
                                    byte[] sector;

                                    uint sectorsToDo;

                                    if(trackSectors - doneSectors >= (ulong)settings.Count)
                                        sectorsToDo = (uint)settings.Count;
                                    else
                                        sectorsToDo = (uint)(trackSectors - doneSectors);

                                    trackTask.Description = string.Format(UI.Converting_sectors_0_to_1_in_track_2,
                                                                          doneSectors + track.StartSector,
                                                                          doneSectors + sectorsToDo + track.StartSector,
                                                                          track.Sequence);

                                    var          useNotLong        = false;
                                    var          result            = false;
                                    SectorStatus sectorStatus      = SectorStatus.NotDumped;
                                    var          sectorStatusArray = new SectorStatus[1];

                                    if(useLong)
                                    {
                                        errno = sectorsToDo == 1
                                                    ? inputOptical.ReadSectorLong(doneSectors + track.StartSector,
                                                        false,
                                                        out sector,
                                                        out sectorStatus)
                                                    : inputOptical.ReadSectorsLong(doneSectors + track.StartSector,
                                                        false,
                                                        sectorsToDo,
                                                        out sector,
                                                        out sectorStatusArray);

                                        if(errno == ErrorNumber.NoError)
                                        {
                                            result = sectorsToDo == 1
                                                         ? outputOptical.WriteSectorLong(sector,
                                                             doneSectors + track.StartSector,
                                                             false,
                                                             sectorStatus)
                                                         : outputOptical.WriteSectorsLong(sector,
                                                             doneSectors + track.StartSector,
                                                             false,
                                                             sectorsToDo,
                                                             sectorStatusArray);
                                        }
                                        else
                                        {
                                            result = true;

                                            if(settings.Force)
                                            {
                                                AaruLogging.Error(UI.Error_0_reading_sector_1_continuing,
                                                                  errno,
                                                                  doneSectors + track.StartSector);
                                            }
                                            else
                                            {
                                                AaruLogging.Error(UI.Error_0_reading_sector_1_not_continuing,
                                                                  errno,
                                                                  doneSectors + track.StartSector);

                                                errno = ErrorNumber.WriteError;

                                                return;
                                            }
                                        }

                                        if(!result && sector.Length % 2352 != 0)
                                        {
                                            if(!settings.Force)
                                            {
                                                AaruLogging.Error(UI
                                                                     .Input_image_is_not_returning_raw_sectors_use_force_if_you_want_to_continue);

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
                                                                              false,
                                                                              out sector,
                                                                              out sectorStatus)
                                                    : inputOptical.ReadSectors(doneSectors + track.StartSector,
                                                                               false,
                                                                               sectorsToDo,
                                                                               out sector,
                                                                               out sectorStatusArray);

                                        // TODO: Move to generic place when anything but CSS DVDs can be decrypted
                                        if(IsDvdMedia(inputOptical.Info.MediaType) && settings.Decrypt)
                                        {
                                            DecryptDvdSector(ref sector,
                                                             inputOptical,
                                                             doneSectors + track.StartSector,
                                                             sectorsToDo,
                                                             plugins,
                                                             ref generatedTitleKeys);
                                        }

                                        if(errno == ErrorNumber.NoError)
                                        {
                                            result = sectorsToDo == 1
                                                         ? outputOptical.WriteSector(sector,
                                                             doneSectors + track.StartSector,
                                                             false,
                                                             sectorStatus)
                                                         : outputOptical.WriteSectors(sector,
                                                             doneSectors + track.StartSector,
                                                             false,
                                                             sectorsToDo,
                                                             sectorStatusArray);
                                        }
                                        else
                                        {
                                            result = true;

                                            if(settings.Force)
                                            {
                                                AaruLogging.Error(UI.Error_0_reading_sector_1_continuing,
                                                                  errno,
                                                                  doneSectors + track.StartSector);
                                            }
                                            else
                                            {
                                                AaruLogging.Error(UI.Error_0_reading_sector_1_not_continuing,
                                                                  errno,
                                                                  doneSectors + track.StartSector);

                                                errno = ErrorNumber.WriteError;

                                                return;
                                            }
                                        }
                                    }

                                    if(!result)
                                    {
                                        if(settings.Force)
                                        {
                                            AaruLogging.Error(UI.Error_0_writing_sector_1_continuing,
                                                              outputOptical.ErrorMessage,
                                                              doneSectors + track.StartSector);
                                        }
                                        else
                                        {
                                            AaruLogging.Error(UI.Error_0_writing_sector_1_not_continuing,
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

            if(errno != ErrorNumber.NoError) return (int)errno;

            Dictionary<byte, string> isrcs                     = new();
            Dictionary<byte, byte>   trackFlags                = new();
            string                   mcn                       = null;
            HashSet<int>             subchannelExtents         = [];
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

            foreach(SectorTagType tag in inputOptical.Info.ReadableSectorTags.Where(t => t == SectorTagType.CdTrackIsrc)
                                                     .OrderBy(t => t))
            {
                foreach(Track track in tracks)
                {
                    errno = inputOptical.ReadSectorTag(track.Sequence, false, tag, out byte[] isrc);

                    if(errno != ErrorNumber.NoError) continue;

                    isrcs[(byte)track.Sequence] = Encoding.UTF8.GetString(isrc);
                }
            }

            foreach(SectorTagType tag in inputOptical.Info.ReadableSectorTags
                                                     .Where(t => t == SectorTagType.CdTrackFlags)
                                                     .OrderBy(t => t))
            {
                foreach(Track track in tracks)
                {
                    errno = inputOptical.ReadSectorTag(track.Sequence, false, tag, out byte[] flags);

                    if(errno != ErrorNumber.NoError) continue;

                    trackFlags[(byte)track.Sequence] = flags[0];
                }
            }

            for(ulong s = 0; s < inputOptical.Info.Sectors; s++)
            {
                if(s > int.MaxValue) break;

                subchannelExtents.Add((int)s);
            }

            foreach(SectorTagType tag in inputOptical.Info.ReadableSectorTags.OrderBy(t => t).TakeWhile(_ => useLong))
            {
                switch(tag)
                {
                    case SectorTagType.AppleSonyTag:
                    case SectorTagType.AppleProfileTag:
                    case SectorTagType.PriamDataTowerTag:
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

                if(settings.Force && !outputOptical.SupportedSectorTags.Contains(tag)) continue;

                errno = ErrorNumber.NoError;

                AnsiConsole.Progress()
                           .AutoClear(true)
                           .HideCompleted(true)
                           .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn())
                           .Start(ctx =>
                            {
                                ProgressTask discTask = ctx.AddTask(UI.Converting_disc);
                                discTask.MaxValue = inputOptical.Tracks.Count;

                                foreach(Track track in inputOptical.Tracks)
                                {
                                    discTask.Description =
                                        string.Format(UI.Converting_tags_in_track_0_of_1,
                                                      discTask.Value + 1,
                                                      discTask.MaxValue);

                                    doneSectors = 0;
                                    ulong  trackSectors = track.EndSector - track.StartSector + 1;
                                    byte[] sector;
                                    bool   result;

                                    switch(tag)
                                    {
                                        case SectorTagType.CdTrackFlags:
                                        case SectorTagType.CdTrackIsrc:
                                            errno = inputOptical.ReadSectorTag(track.Sequence, false, tag, out sector);

                                            switch(errno)
                                            {
                                                case ErrorNumber.NoData:
                                                    errno = ErrorNumber.NoError;

                                                    continue;
                                                case ErrorNumber.NoError:
                                                    result = outputOptical.WriteSectorTag(sector,
                                                        track.Sequence,
                                                        false,
                                                        tag);

                                                    break;
                                                default:
                                                {
                                                    if(settings.Force)
                                                    {
                                                        AaruLogging.Error(UI.Error_0_writing_tag_continuing,
                                                                          outputOptical.ErrorMessage);

                                                        continue;
                                                    }

                                                    AaruLogging.Error(UI.Error_0_writing_tag_not_continuing,
                                                                      outputOptical.ErrorMessage);

                                                    errno = ErrorNumber.WriteError;

                                                    return;
                                                }
                                            }

                                            if(!result)
                                            {
                                                if(settings.Force)
                                                {
                                                    AaruLogging.Error(UI.Error_0_writing_tag_continuing,
                                                                      outputOptical.ErrorMessage);
                                                }
                                                else
                                                {
                                                    AaruLogging.Error(UI.Error_0_writing_tag_not_continuing,
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

                                        if(trackSectors - doneSectors >= (ulong)settings.Count)
                                            sectorsToDo = (uint)settings.Count;
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
                                                                               false,
                                                                               tag,
                                                                               out sector);

                                            if(errno == ErrorNumber.NoError)
                                            {
                                                if(tag == SectorTagType.CdSectorSubchannel)
                                                {
                                                    bool indexesChanged =
                                                        CompactDisc.WriteSubchannelToImage(MmcSubchannel.Raw,
                                                            MmcSubchannel.Raw,
                                                            sector,
                                                            doneSectors + track.StartSector,
                                                            1,
                                                            null,
                                                            isrcs,
                                                            (byte)track.Sequence,
                                                            ref mcn,
                                                            tracks,
                                                            subchannelExtents,
                                                            fixSubchannelPosition,
                                                            outputOptical,
                                                            fixSubchannel,
                                                            fixSubchannelCrc,
                                                            null,
                                                            smallestPregapLbaPerTrack,
                                                            false,
                                                            out _);

                                                    if(indexesChanged) outputOptical.SetTracks(tracks.ToList());

                                                    result = true;
                                                }
                                                else
                                                {
                                                    result = outputOptical.WriteSectorTag(sector,
                                                        doneSectors + track.StartSector,
                                                        false,
                                                        tag);
                                                }
                                            }
                                            else
                                            {
                                                result = true;

                                                if(settings.Force)
                                                {
                                                    AaruLogging.Error(UI.Error_0_reading_tag_for_sector_1_continuing,
                                                                      errno,
                                                                      doneSectors + track.StartSector);
                                                }
                                                else
                                                {
                                                    AaruLogging
                                                       .Error(UI.Error_0_reading_tag_for_sector_1_not_continuing,
                                                              errno,
                                                              doneSectors + track.StartSector);

                                                    return;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            errno = inputOptical.ReadSectorsTag(doneSectors + track.StartSector,
                                                                                    false,
                                                                                    sectorsToDo,
                                                                                    tag,
                                                                                    out sector);

                                            if(errno == ErrorNumber.NoError)
                                            {
                                                if(tag == SectorTagType.CdSectorSubchannel)
                                                {
                                                    bool indexesChanged =
                                                        CompactDisc.WriteSubchannelToImage(MmcSubchannel.Raw,
                                                            MmcSubchannel.Raw,
                                                            sector,
                                                            doneSectors + track.StartSector,
                                                            sectorsToDo,
                                                            null,
                                                            isrcs,
                                                            (byte)track.Sequence,
                                                            ref mcn,
                                                            tracks,
                                                            subchannelExtents,
                                                            fixSubchannelPosition,
                                                            outputOptical,
                                                            fixSubchannel,
                                                            fixSubchannelCrc,
                                                            null,
                                                            smallestPregapLbaPerTrack,
                                                            false,
                                                            out _);

                                                    if(indexesChanged) outputOptical.SetTracks(tracks.ToList());

                                                    result = true;
                                                }
                                                else
                                                {
                                                    result = outputOptical.WriteSectorsTag(sector,
                                                        doneSectors + track.StartSector,
                                                        false,
                                                        sectorsToDo,
                                                        tag);
                                                }
                                            }
                                            else
                                            {
                                                result = true;

                                                if(settings.Force)
                                                {
                                                    AaruLogging.Error(UI.Error_0_reading_tag_for_sector_1_continuing,
                                                                      errno,
                                                                      doneSectors + track.StartSector);
                                                }
                                                else
                                                {
                                                    AaruLogging
                                                       .Error(UI.Error_0_reading_tag_for_sector_1_not_continuing,
                                                              errno,
                                                              doneSectors + track.StartSector);

                                                    return;
                                                }
                                            }
                                        }

                                        if(!result)
                                        {
                                            if(settings.Force)
                                            {
                                                AaruLogging.Error(UI.Error_0_writing_tag_for_sector_1_continuing,
                                                                  outputOptical.ErrorMessage,
                                                                  doneSectors + track.StartSector);
                                            }
                                            else
                                            {
                                                AaruLogging.Error(UI.Error_0_writing_tag_for_sector_1_not_continuing,
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

                if(errno != ErrorNumber.NoError && !settings.Force) return (int)errno;
            }

            foreach(KeyValuePair<byte, string> isrc in isrcs)
            {
                outputOptical.WriteSectorTag(Encoding.UTF8.GetBytes(isrc.Value),
                                             isrc.Key,
                                             false,
                                             SectorTagType.CdTrackIsrc);
            }

            if(trackFlags.Count > 0)
            {
                foreach((byte track, byte flags) in trackFlags)
                    outputOptical.WriteSectorTag([flags], track, false, SectorTagType.CdTrackFlags);
            }

            if(mcn != null) outputOptical.WriteMediaTag(Encoding.UTF8.GetBytes(mcn), MediaTagType.CD_MCN);

            // TODO: Progress
            if(IsCompactDiscMedia(inputOptical.Info.MediaType) && settings.GenerateSubchannels)
            {
                Core.Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask(Localization.Core.Generating_subchannels).IsIndeterminate();

                    CompactDisc.GenerateSubchannels(subchannelExtents,
                                                    tracks,
                                                    trackFlags,
                                                    inputOptical.Info.Sectors,
                                                    null,
                                                    null,
                                                    null,
                                                    null,
                                                    outputOptical);
                });
            }

            var     errorNumber = inputOptical.ReadDPM(out uint dpmStartSector, out uint dpmResolution, out uint numberOfDpmEntries, out ulong[] dpm);

            if(errorNumber == ErrorNumber.NoError)
            {
                outputOptical.HeldDpmStartSector = dpmStartSector;
                outputOptical.HeldDpmResolution  = dpmResolution;
                outputOptical.HeldNumberOfDpmEntries = numberOfDpmEntries;
                outputOptical.HeldDpm            = dpm;
            }
        }
        else
        {
            var outputMedia = outputFormat as IWritableImage;

            if(inputTape == null || outputTape == null || !inputTape.IsTape)
            {
                (uint cylinders, uint heads, uint sectors) chs =
                    geometryValues != null
                        ? (geometryValues.Value.cylinders, geometryValues.Value.heads, geometryValues.Value.sectors)
                        : (inputFormat.Info.Cylinders, inputFormat.Info.Heads, inputFormat.Info.SectorsPerTrack);

                AaruLogging.WriteLine(UI.Setting_geometry_to_0_cylinders_1_heads_and_2_sectors_per_track,
                                      chs.cylinders,
                                      chs.heads,
                                      chs.sectors);

                if(!outputMedia.SetGeometry(chs.cylinders, chs.heads, chs.sectors))
                {
                    AaruLogging.Error(UI.Error_0_setting_geometry_image_may_be_incorrect_continuing,
                                      outputMedia.ErrorMessage);
                }
            }

            ErrorNumber errno = ErrorNumber.NoError;

            AnsiConsole.Progress()
                       .AutoClear(true)
                       .HideCompleted(true)
                       .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn())
                       .Start(ctx =>
                        {
                            ProgressTask mediaTask = ctx.AddTask(UI.Converting_media);
                            mediaTask.MaxValue = inputFormat.Info.Sectors;

                            while(doneSectors < inputFormat.Info.Sectors)
                            {
                                byte[] sector;

                                uint sectorsToDo;

                                if(inputTape?.IsTape == true)
                                    sectorsToDo = 1;
                                else if(inputFormat.Info.Sectors - doneSectors >= (ulong)settings.Count)
                                    sectorsToDo = (uint)settings.Count;
                                else
                                    sectorsToDo = (uint)(inputFormat.Info.Sectors - doneSectors);

                                mediaTask.Description =
                                    string.Format(UI.Converting_sectors_0_to_1, doneSectors, doneSectors + sectorsToDo);

                                bool         result;
                                SectorStatus sectorStatus      = SectorStatus.NotDumped;
                                var          sectorStatusArray = new SectorStatus[1];

                                if(useLong)
                                {
                                    errno = sectorsToDo == 1
                                                ? inputFormat.ReadSectorLong(doneSectors,
                                                                             false,
                                                                             out sector,
                                                                             out sectorStatus)
                                                : inputFormat.ReadSectorsLong(doneSectors,
                                                                              false,
                                                                              sectorsToDo,
                                                                              out sector,
                                                                              out sectorStatusArray);

                                    if(errno == ErrorNumber.NoError)
                                    {
                                        result = sectorsToDo == 1
                                                     ? outputMedia.WriteSectorLong(sector,
                                                         doneSectors,
                                                         false,
                                                         sectorStatus)
                                                     : outputMedia.WriteSectorsLong(sector,
                                                         doneSectors,
                                                         false,
                                                         sectorsToDo,
                                                         sectorStatusArray);
                                    }
                                    else
                                    {
                                        result = true;

                                        if(settings.Force)
                                        {
                                            AaruLogging.Error(UI.Error_0_reading_sector_1_continuing,
                                                              errno,
                                                              doneSectors);
                                        }
                                        else
                                        {
                                            AaruLogging.Error(UI.Error_0_reading_sector_1_not_continuing,
                                                              errno,
                                                              doneSectors);

                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    errno = sectorsToDo == 1
                                                ? inputFormat.ReadSector(doneSectors,
                                                                         false,
                                                                         out sector,
                                                                         out sectorStatus)
                                                : inputFormat.ReadSectors(doneSectors,
                                                                          false,
                                                                          sectorsToDo,
                                                                          out sector,
                                                                          out sectorStatusArray);

                                    if(errno == ErrorNumber.NoError)
                                    {
                                        result = sectorsToDo == 1
                                                     ? outputMedia.WriteSector(sector, doneSectors, false, sectorStatus)
                                                     : outputMedia.WriteSectors(sector,
                                                                                    doneSectors,
                                                                                    false,
                                                                                    sectorsToDo,
                                                                                    sectorStatusArray);
                                    }
                                    else
                                    {
                                        result = true;

                                        if(settings.Force)
                                        {
                                            AaruLogging.Error(UI.Error_0_reading_sector_1_continuing,
                                                              errno,
                                                              doneSectors);
                                        }
                                        else
                                        {
                                            AaruLogging.Error(UI.Error_0_reading_sector_1_not_continuing,
                                                              errno,
                                                              doneSectors);

                                            return;
                                        }
                                    }
                                }

                                if(!result)
                                {
                                    if(settings.Force)
                                    {
                                        AaruLogging.Error(UI.Error_0_writing_sector_1_continuing,
                                                          outputMedia.ErrorMessage,
                                                          doneSectors);
                                    }
                                    else
                                    {
                                        AaruLogging.Error(UI.Error_0_writing_sector_1_not_continuing,
                                                          outputMedia.ErrorMessage,
                                                          doneSectors);

                                        errno = ErrorNumber.WriteError;

                                        return;
                                    }
                                }

                                doneSectors     += sectorsToDo;
                                mediaTask.Value += sectorsToDo;
                            }

                            mediaTask.StopTask();

                            foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags.TakeWhile(_ => useLong))
                            {
                                switch(tag)
                                {
                                    case SectorTagType.AppleSonyTag:
                                    case SectorTagType.AppleProfileTag:
                                    case SectorTagType.PriamDataTowerTag:
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

                                if(settings.Force && !outputMedia.SupportedSectorTags.Contains(tag)) continue;

                                doneSectors = 0;

                                ProgressTask tagsTask = ctx.AddTask(UI.Converting_tags);
                                tagsTask.MaxValue = inputFormat.Info.Sectors;

                                while(doneSectors < inputFormat.Info.Sectors)
                                {
                                    uint sectorsToDo;

                                    if(inputFormat.Info.Sectors - doneSectors >= (ulong)settings.Count)
                                        sectorsToDo = (uint)settings.Count;
                                    else
                                        sectorsToDo = (uint)(inputFormat.Info.Sectors - doneSectors);

                                    tagsTask.Description = string.Format(UI.Converting_tag_2_for_sectors_0_to_1,
                                                                         doneSectors,
                                                                         doneSectors + sectorsToDo,
                                                                         tag);

                                    bool result;

                                    errno = sectorsToDo == 1
                                                ? inputFormat.ReadSectorTag(doneSectors, false, tag, out byte[] sector)
                                                : inputFormat.ReadSectorsTag(doneSectors,
                                                                             false,
                                                                             sectorsToDo,
                                                                             tag,
                                                                             out sector);

                                    if(errno == ErrorNumber.NoError)
                                    {
                                        result = sectorsToDo == 1
                                                     ? outputMedia.WriteSectorTag(sector, doneSectors, false, tag)
                                                     : outputMedia.WriteSectorsTag(sector,
                                                         doneSectors,
                                                         false,
                                                         sectorsToDo,
                                                         tag);
                                    }
                                    else
                                    {
                                        result = true;

                                        if(settings.Force)
                                        {
                                            AaruLogging.Error(UI.Error_0_reading_sector_1_continuing,
                                                              errno,
                                                              doneSectors);
                                        }
                                        else
                                        {
                                            AaruLogging.Error(UI.Error_0_reading_sector_1_not_continuing,
                                                              errno,
                                                              doneSectors);

                                            return;
                                        }
                                    }

                                    if(!result)
                                    {
                                        if(settings.Force)
                                        {
                                            AaruLogging.Error(UI.Error_0_writing_sector_1_continuing,
                                                              outputMedia.ErrorMessage,
                                                              doneSectors);
                                        }
                                        else
                                        {
                                            AaruLogging.Error(UI.Error_0_writing_sector_1_not_continuing,
                                                              outputMedia.ErrorMessage,
                                                              doneSectors);

                                            errno = ErrorNumber.WriteError;

                                            return;
                                        }
                                    }

                                    doneSectors    += sectorsToDo;
                                    tagsTask.Value += sectorsToDo;
                                }

                                tagsTask.StopTask();
                            }

                            if(inputFormat is IFluxImage inputFlux && outputFormat is IWritableFluxImage outputFlux)
                            {
                                for(ushort track = 0; track < inputFlux.Info.Cylinders; track++)
                                {
                                    for(uint head = 0; head < inputFlux.Info.Heads; head++)
                                    {
                                        ErrorNumber error = inputFlux.SubTrackLength(head, track, out byte subTrackLen);

                                        if(error != ErrorNumber.NoError) continue;

                                        for(byte subTrackIndex = 0; subTrackIndex < subTrackLen; subTrackIndex++)
                                        {
                                            error = inputFlux.CapturesLength(head,
                                                                             track,
                                                                             subTrackIndex,
                                                                             out uint capturesLen);

                                            if(error != ErrorNumber.NoError) continue;

                                            for(uint captureIndex = 0; captureIndex < capturesLen; captureIndex++)
                                            {
                                                inputFlux.ReadFluxCapture(head,
                                                                          track,
                                                                          subTrackIndex,
                                                                          captureIndex,
                                                                          out ulong indexResolution,
                                                                          out ulong dataResolution,
                                                                          out byte[] indexBuffer,
                                                                          out byte[] dataBuffer);

                                                outputFlux.WriteFluxCapture(indexResolution,
                                                                            dataResolution,
                                                                            indexBuffer,
                                                                            dataBuffer,
                                                                            head,
                                                                            track,
                                                                            subTrackIndex,
                                                                            captureIndex);
                                            }
                                        }
                                    }
                                }
                            }

                            if(inputTape == null || outputTape == null || !inputTape.IsTape) return;

                            ProgressTask filesTask = ctx.AddTask(UI.Converting_files);
                            filesTask.MaxValue = inputTape.Files.Count;

                            foreach(TapeFile tapeFile in inputTape.Files)
                            {
                                filesTask.Description =
                                    string.Format(UI.Converting_file_0_of_partition_1,
                                                  tapeFile.File,
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

            if(errno != ErrorNumber.NoError) return (int)errno;
        }

        if(nominalNegativeSectors > 0)
        {
            var outputMedia = outputFormat as IWritableImage;

            int negativeResult =
                ConvertNegativeSectors(inputFormat, outputMedia, nominalNegativeSectors, useLong, settings);

            if(negativeResult != (int)ErrorNumber.NoError) return negativeResult;
        }


        if(nominalOverflowSectors > 0)
        {
            var outputMedia = outputFormat as IWritableImage;

            int overflowResult =
                ConvertOverflowSectors(inputFormat, outputMedia, nominalOverflowSectors, useLong, settings);

            if(overflowResult != (int)ErrorNumber.NoError) return overflowResult;
        }

        if(resume != null || dumpHardware != null)
        {
            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask(UI.Writing_dump_hardware_list).IsIndeterminate();

                if(resume != null)
                    ret                           = outputFormat.SetDumpHardware(resume.Tries);
                else if(dumpHardware != null) ret = outputFormat.SetDumpHardware(dumpHardware);
            });

            if(ret) AaruLogging.WriteLine(UI.Written_dump_hardware_list_to_output_image);
        }

        ret = false;

        if(sidecar != null || metadata != null)
        {
            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask(UI.Writing_metadata).IsIndeterminate();

                if(sidecar != null)
                    ret                       = outputFormat.SetMetadata(sidecar);
                else if(metadata != null) ret = outputFormat.SetMetadata(metadata);
            });

            if(ret) AaruLogging.WriteLine(UI.Written_Aaru_Metadata_to_output_image);
        }

        var closed = false;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Closing_output_image).IsIndeterminate();
            closed = outputFormat.Close();
        });

        if(!closed)
        {
            AaruLogging.Error(UI.Error_0_closing_output_image_Contents_are_not_correct, outputFormat.ErrorMessage);

            return (int)ErrorNumber.WriteError;
        }

        AaruLogging.WriteLine(UI.Conversion_done);

        return (int)ErrorNumber.NoError;
    }

    private (bool success, uint cylinders, uint heads, uint sectors)? ParseGeometry(string geometryString)
    {
        // Parses CHS (Cylinder/Head/Sector) geometry string in format "C/H/S" or "C-H-S"
        // Returns tuple with success flag and parsed values, or null if not specified

        if(geometryString == null) return null;

        string[] geometryPieces = geometryString.Split('/');

        if(geometryPieces.Length == 0) geometryPieces = geometryString.Split('-');

        if(geometryPieces.Length != 3)
        {
            AaruLogging.Error(UI.Invalid_geometry_specified);

            return (false, 0, 0, 0);
        }

        if(!uint.TryParse(geometryPieces[0], out uint cylinders) || cylinders == 0)
        {
            AaruLogging.Error(UI.Invalid_number_of_cylinders_specified);

            return (false, 0, 0, 0);
        }

        if(!uint.TryParse(geometryPieces[1], out uint heads) || heads == 0)
        {
            AaruLogging.Error(UI.Invalid_number_of_heads_specified);

            return (false, 0, 0, 0);
        }

        if(!uint.TryParse(geometryPieces[2], out uint sectors) || sectors == 0)
        {
            AaruLogging.Error(UI.Invalid_sectors_per_track_specified);

            return (false, 0, 0, 0);
        }

        return (true, cylinders, heads, sectors);
    }

    private (bool success, Metadata sidecar, Resume resume) LoadMetadata(
        string aaruMetadataPath, string cicmXmlPath, string resumeFilePath)
    {
        // Loads metadata and resume information from sidecar files
        // Supports both Aaru JSON and legacy CICM XML formats
        // Returns tuple with success flag, metadata, and resume information

        Metadata sidecar = null;
        Resume   resume  = null;

        if(aaruMetadataPath != null)
        {
            if(File.Exists(aaruMetadataPath))
            {
                try
                {
                    var fs = new FileStream(aaruMetadataPath, FileMode.Open);

                    sidecar =
                        (JsonSerializer.Deserialize(fs, typeof(MetadataJson), MetadataJsonContext.Default) as
                             MetadataJson)?.AaruMetadata;

                    fs.Close();
                }
                catch(Exception ex)
                {
                    AaruLogging.Error(UI.Incorrect_metadata_sidecar_file_not_continuing);
                    AaruLogging.Exception(ex, UI.Incorrect_metadata_sidecar_file_not_continuing);

                    return (false, null, null);
                }
            }
            else
            {
                AaruLogging.Error(UI.Could_not_find_metadata_sidecar);

                return (false, null, null);
            }
        }
        else if(cicmXmlPath != null)
        {
            if(File.Exists(cicmXmlPath))
            {
                try
                {
                    // Should be covered by virtue of being the same exact class as the JSON above
#pragma warning disable IL2026, CS0618
                    var xs = new XmlSerializer(typeof(CICMMetadataType));
#pragma warning restore IL2026, CS0618

                    var sr = new StreamReader(cicmXmlPath);

                    // Should be covered by virtue of being the same exact class as the JSON above
#pragma warning disable IL2026, CS0618
                    sidecar = (CICMMetadataType)xs.Deserialize(sr);
#pragma warning restore IL2026, CS0618

                    sr.Close();
                }
                catch(Exception ex)
                {
                    AaruLogging.Error(UI.Incorrect_metadata_sidecar_file_not_continuing);
                    AaruLogging.Exception(ex, UI.Incorrect_metadata_sidecar_file_not_continuing);

                    return (false, null, null);
                }
            }
            else
            {
                AaruLogging.Error(UI.Could_not_find_metadata_sidecar);

                return (false, null, null);
            }
        }

        if(resumeFilePath == null) return (true, sidecar, null);

        if(File.Exists(resumeFilePath))
        {
            try
            {
                if(resumeFilePath.EndsWith(".metadata.json", StringComparison.CurrentCultureIgnoreCase))
                {
                    var fs = new FileStream(resumeFilePath, FileMode.Open);

                    resume =
                        (JsonSerializer.Deserialize(fs, typeof(ResumeJson), ResumeJsonContext.Default) as ResumeJson)
                      ?.Resume;

                    fs.Close();
                }
                else
                {
                    // Bypassed by JSON source generator used above
#pragma warning disable IL2026
                    var xs = new XmlSerializer(typeof(Resume));
#pragma warning restore IL2026

                    var sr = new StreamReader(resumeFilePath);

                    // Bypassed by JSON source generator used above
#pragma warning disable IL2026
                    resume = (Resume)xs.Deserialize(sr);
#pragma warning restore IL2026

                    sr.Close();
                }
            }
            catch(Exception ex)
            {
                AaruLogging.Error(UI.Incorrect_resume_file_not_continuing);
                AaruLogging.Exception(ex, UI.Incorrect_resume_file_not_continuing);

                return (false, sidecar, null);
            }
        }
        else
        {
            AaruLogging.Error(UI.Could_not_find_resume_file);

            return (false, sidecar, null);
        }

        return (true, sidecar, resume);
    }

    private void LogCommandParameters(Settings settings,         bool fixSubchannelPosition, bool fixSubchannel,
                                      bool     fixSubchannelCrc, Dictionary<string, string> parsedOptions)
    {
        // Logs all command-line parameters for debugging and audit trail purposes
        // Consolidated from 46+ individual logging statements

        AaruLogging.Debug(MODULE_NAME, "--cicm-xml={0}", Markup.Escape(settings.CicmXml  ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--comments={0}", Markup.Escape(settings.Comments ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--count={0}",    settings.Count);
        AaruLogging.Debug(MODULE_NAME, "--creator={0}",  Markup.Escape(settings.Creator ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--debug={0}",    settings.Debug);

        AaruLogging.Debug(MODULE_NAME, "--drive-manufacturer={0}", Markup.Escape(settings.DriveManufacturer ?? ""));

        AaruLogging.Debug(MODULE_NAME, "--drive-model={0}", Markup.Escape(settings.DriveModel ?? ""));

        AaruLogging.Debug(MODULE_NAME, "--drive-revision={0}", Markup.Escape(settings.DriveFirmwareRevision ?? ""));

        AaruLogging.Debug(MODULE_NAME, "--drive-serial={0}",       Markup.Escape(settings.DriveSerialNumber ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--force={0}",              settings.Force);
        AaruLogging.Debug(MODULE_NAME, "--format={0}",             Markup.Escape(settings.Format       ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--geometry={0}",           Markup.Escape(settings.Geometry     ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--input={0}",              Markup.Escape(settings.InputPath    ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--media-barcode={0}",      Markup.Escape(settings.MediaBarcode ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--media-lastsequence={0}", settings.LastMediaSequence);

        AaruLogging.Debug(MODULE_NAME, "--media-manufacturer={0}", Markup.Escape(settings.MediaManufacturer ?? ""));

        AaruLogging.Debug(MODULE_NAME, "--media-model={0}", Markup.Escape(settings.MediaModel ?? ""));

        AaruLogging.Debug(MODULE_NAME, "--media-partnumber={0}", Markup.Escape(settings.MediaPartNumber ?? ""));

        AaruLogging.Debug(MODULE_NAME, "--media-sequence={0}", settings.MediaSequence);
        AaruLogging.Debug(MODULE_NAME, "--media-serial={0}", Markup.Escape(settings.MediaSerialNumber ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--media-title={0}", Markup.Escape(settings.MediaTitle ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--options={0}", Markup.Escape(settings.Options ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--output={0}", Markup.Escape(settings.OutputPath ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--resume-file={0}", Markup.Escape(settings.ResumeFile ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--verbose={0}", settings.Verbose);
        AaruLogging.Debug(MODULE_NAME, "--fix-subchannel-position={0}", fixSubchannelPosition);
        AaruLogging.Debug(MODULE_NAME, "--fix-subchannel={0}", fixSubchannel);
        AaruLogging.Debug(MODULE_NAME, "--fix-subchannel-crc={0}", fixSubchannelCrc);
        AaruLogging.Debug(MODULE_NAME, "--generate-subchannels={0}", settings.GenerateSubchannels);
        AaruLogging.Debug(MODULE_NAME, "--decrypt={0}", settings.Decrypt);
        AaruLogging.Debug(MODULE_NAME, "--aaru-metadata={0}", Markup.Escape(settings.AaruMetadata ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--ignore-negative-sectors={0}", settings.IgnoreNegativeSectors);
        AaruLogging.Debug(MODULE_NAME, "--ignore-overflow-sectors={0}", settings.IgnoreOverflowSectors);

        AaruLogging.Debug(MODULE_NAME, UI.Parsed_options);

        foreach(KeyValuePair<string, string> parsedOption in parsedOptions)
            AaruLogging.Debug(MODULE_NAME, "{0} = {1}", parsedOption.Key, parsedOption.Value);
    }

    private void DecryptDvdSector(ref byte[] sector,      IOpticalMediaImage inputOptical, ulong sectorAddress,
                                  uint       sectorsToDo, PluginRegister     plugins, ref byte[] generatedTitleKeys)
    {
        // Decrypts DVD sectors using CSS (Content Scramble System) decryption
        // Retrieves decryption keys from sector tags or generates them from ISO9660 filesystem
        // Only MPEG packets within sectors can be encrypted

        // Only sectors which are MPEG packets can be encrypted.
        if(!Mpeg.ContainsMpegPackets(sector, sectorsToDo)) return;

        byte[] cmi, titleKey;

        if(sectorsToDo == 1)
        {
            if(inputOptical.ReadSectorTag(sectorAddress, false, SectorTagType.DvdSectorCmi, out cmi) ==
               ErrorNumber.NoError &&
               inputOptical.ReadSectorTag(sectorAddress, false, SectorTagType.DvdTitleKeyDecrypted, out titleKey) ==
               ErrorNumber.NoError)
                sector = CSS.DecryptSector(sector, titleKey, cmi);
            else
            {
                if(generatedTitleKeys == null) GenerateDvdTitleKeys(inputOptical, plugins, ref generatedTitleKeys);

                if(generatedTitleKeys != null)
                {
                    sector = CSS.DecryptSector(sector,
                                               generatedTitleKeys.Skip((int)(5 * sectorAddress)).Take(5).ToArray(),
                                               null);
                }
            }
        }
        else
        {
            if(inputOptical.ReadSectorsTag(sectorAddress, false, sectorsToDo, SectorTagType.DvdSectorCmi, out cmi) ==
               ErrorNumber.NoError &&
               inputOptical.ReadSectorsTag(sectorAddress,
                                           false,
                                           sectorsToDo,
                                           SectorTagType.DvdTitleKeyDecrypted,
                                           out titleKey) ==
               ErrorNumber.NoError)
                sector = CSS.DecryptSector(sector, titleKey, cmi, sectorsToDo);
            else
            {
                if(generatedTitleKeys == null) GenerateDvdTitleKeys(inputOptical, plugins, ref generatedTitleKeys);

                if(generatedTitleKeys != null)
                {
                    sector = CSS.DecryptSector(sector,
                                               generatedTitleKeys.Skip((int)(5 * sectorAddress))
                                                                 .Take((int)(5 * sectorsToDo))
                                                                 .ToArray(),
                                               null,
                                               sectorsToDo);
                }
            }
        }
    }

    private void GenerateDvdTitleKeys(IOpticalMediaImage inputOptical, PluginRegister plugins,
                                      ref byte[]         generatedTitleKeys)
    {
        // Generates DVD CSS title keys from ISO9660 filesystem
        // Used when explicit title keys are not available in sector tags
        // Searches for ISO9660 partitions to derive decryption keys

        List<Partition> partitions = Core.Partitions.GetAll(inputOptical);

        partitions = partitions.FindAll(p =>
        {
            Core.Filesystems.Identify(inputOptical, out List<string> idPlugins, p);

            return idPlugins.Contains("iso9660 filesystem");
        });

        if(!plugins.ReadOnlyFilesystems.TryGetValue("iso9660 filesystem", out IReadOnlyFilesystem rofs)) return;

        AaruLogging.Debug(MODULE_NAME, UI.Generating_decryption_keys);

        generatedTitleKeys = CSS.GenerateTitleKeys(inputOptical, partitions, inputOptical.Info.Sectors, rofs);
    }

    private bool IsDvdMedia(MediaType mediaType) =>

        // Checks if media type is any variant of DVD (ROM, R, RDL, PR, PRDL)
        // Consolidates media type checking logic used throughout conversion process
        mediaType is MediaType.DVDROM or MediaType.DVDR or MediaType.DVDRDL or MediaType.DVDPR or MediaType.DVDPRDL;

    private bool IsCompactDiscMedia(MediaType mediaType) =>

        // Checks if media type is any variant of compact disc (CD, CDDA, CDR, CDRW, etc.)
        // Covers all 45+ CD-based media types including gaming and specialty formats
        mediaType is MediaType.CD
                  or MediaType.CDDA
                  or MediaType.CDG
                  or MediaType.CDEG
                  or MediaType.CDI
                  or MediaType.CDROM
                  or MediaType.CDROMXA
                  or MediaType.CDPLUS
                  or MediaType.CDMO
                  or MediaType.CDR
                  or MediaType.CDRW
                  or MediaType.CDMRW
                  or MediaType.VCD
                  or MediaType.SVCD
                  or MediaType.PCD
                  or MediaType.DTSCD
                  or MediaType.CDMIDI
                  or MediaType.CDV
                  or MediaType.CDIREADY
                  or MediaType.FMTOWNS
                  or MediaType.PS1CD
                  or MediaType.PS2CD
                  or MediaType.MEGACD
                  or MediaType.SATURNCD
                  or MediaType.GDROM
                  or MediaType.GDR
                  or MediaType.MilCD
                  or MediaType.SuperCDROM2
                  or MediaType.JaguarCD
                  or MediaType.ThreeDO
                  or MediaType.PCFX
                  or MediaType.NeoGeoCD
                  or MediaType.CDTV
                  or MediaType.CD32
                  or MediaType.Playdia
                  or MediaType.Pippin
                  or MediaType.VideoNow
                  or MediaType.VideoNowColor
                  or MediaType.VideoNowXp
                  or MediaType.CVD;

    private int ConvertMediaTags(IMediaImage inputFormat, IWritableImage outputFormat, Settings settings)
    {
        // Converts media tags (TOC, lead-in, etc.) from input to output format
        // Handles force mode to skip unsupported tags or fail on data loss
        // Shows progress for each tag being converted

        foreach(MediaTagType mediaTag in inputFormat.Info.ReadableMediaTags.Where(mediaTag => !settings.Force ||
                    outputFormat.SupportedMediaTags.Contains(mediaTag)))
        {
            ErrorNumber errorNumber = ErrorNumber.NoError;

            AnsiConsole.Progress()
                       .AutoClear(false)
                       .HideCompleted(false)
                       .Columns(new TaskDescriptionColumn(), new SpinnerColumn())
                       .Start(ctx =>
                        {
                            ctx.AddTask(string.Format(UI.Converting_media_tag_0, Markup.Escape(mediaTag.ToString())));
                            ErrorNumber errno = inputFormat.ReadMediaTag(mediaTag, out byte[] tag);

                            if(errno != ErrorNumber.NoError)
                            {
                                if(settings.Force)
                                    AaruLogging.Error(UI.Error_0_reading_media_tag, errno);
                                else
                                {
                                    AaruLogging.Error(UI.Error_0_reading_media_tag_not_continuing, errno);

                                    errorNumber = errno;
                                }

                                return;
                            }

                            if(outputFormat?.WriteMediaTag(tag, mediaTag) == true) return;

                            if(settings.Force)
                                AaruLogging.Error(UI.Error_0_writing_media_tag, outputFormat?.ErrorMessage);
                            else
                            {
                                AaruLogging.Error(UI.Error_0_writing_media_tag_not_continuing,
                                                  outputFormat?.ErrorMessage);

                                errorNumber = ErrorNumber.WriteError;
                            }
                        });

            if(errorNumber != ErrorNumber.NoError) return (int)errorNumber;
        }

        return (int)ErrorNumber.NoError;
    }

    private int SetImageMetadata(IMediaImage inputFormat, IWritableImage outputFormat, Settings settings)
    {
        // Builds and applies complete ImageInfo metadata to output image
        // Copies input metadata and applies command-line overrides (title, comments, creator, drive info, etc.)
        // Sets Aaru application version and applies all metadata fields to output format

        var imageInfo = new ImageInfo
        {
            Application           = "Aaru",
            ApplicationVersion    = Version.GetInformationalVersion(),
            Comments              = settings.Comments              ?? inputFormat.Info.Comments,
            Creator               = settings.Creator               ?? inputFormat.Info.Creator,
            DriveFirmwareRevision = settings.DriveFirmwareRevision ?? inputFormat.Info.DriveFirmwareRevision,
            DriveManufacturer     = settings.DriveManufacturer     ?? inputFormat.Info.DriveManufacturer,
            DriveModel            = settings.DriveModel            ?? inputFormat.Info.DriveModel,
            DriveSerialNumber     = settings.DriveSerialNumber     ?? inputFormat.Info.DriveSerialNumber,
            LastMediaSequence =
                settings.LastMediaSequence != 0 ? settings.LastMediaSequence : inputFormat.Info.LastMediaSequence,
            MediaBarcode      = settings.MediaBarcode      ?? inputFormat.Info.MediaBarcode,
            MediaManufacturer = settings.MediaManufacturer ?? inputFormat.Info.MediaManufacturer,
            MediaModel        = settings.MediaModel        ?? inputFormat.Info.MediaModel,
            MediaPartNumber   = settings.MediaPartNumber   ?? inputFormat.Info.MediaPartNumber,
            MediaSequence     = settings.MediaSequence != 0 ? settings.MediaSequence : inputFormat.Info.MediaSequence,
            MediaSerialNumber = settings.MediaSerialNumber ?? inputFormat.Info.MediaSerialNumber,
            MediaTitle        = settings.MediaTitle        ?? inputFormat.Info.MediaTitle
        };

        if(outputFormat.SetImageInfo(imageInfo)) return (int)ErrorNumber.NoError;

        if(!settings.Force)
        {
            AaruLogging.Error(UI.Error_0_setting_metadata_not_continuing, outputFormat.ErrorMessage);

            return (int)ErrorNumber.WriteError;
        }

        AaruLogging.Error(Localization.Core.Error_0_setting_metadata, outputFormat.ErrorMessage);

        return (int)ErrorNumber.NoError;
    }

    private int CreateOutputImage(IWritableImage             outputFormat,    string outputPath, MediaType mediaType,
                                  Dictionary<string, string> parsedOptions,   IMediaImage inputFormat,
                                  uint                       negativeSectors, uint overflowSectors)
    {
        // Creates output image file with specified parameters
        // Calls the output format plugin's Create() method with sector count and format options
        // Shows progress indicator during file creation
        // Returns error code if creation fails

        var created = false;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Invoke_Opening_image_file).IsIndeterminate();

            // TODO: Get the source image number of negative and overflow sectors to convert them too
            created = outputFormat.Create(outputPath,
                                          mediaType,
                                          parsedOptions,
                                          inputFormat.Info.Sectors,
                                          negativeSectors,
                                          overflowSectors,
                                          inputFormat.Info.SectorSize);
        });

        if(created) return (int)ErrorNumber.NoError;

        AaruLogging.Error(UI.Error_0_creating_output_image, outputFormat.ErrorMessage);

        return (int)ErrorNumber.CannotCreateFormat;
    }

    private int SetupTapeImage(ITapeImage inputTape, IWritableTapeImage outputTape, IWritableImage outputFormat)
    {
        // Configures output format for tape image handling
        // Calls SetTape() on output to initialize tape mode if both input and output support tapes
        // Returns error if tape mode initialization fails

        if(inputTape?.IsTape != true || outputTape == null) return (int)ErrorNumber.NoError;

        bool ret = outputTape.SetTape();

        // Cannot set image to tape mode
        if(ret) return (int)ErrorNumber.NoError;

        AaruLogging.Error(UI.Error_setting_output_image_in_tape_mode);
        AaruLogging.Error(outputFormat.ErrorMessage);

        return (int)ErrorNumber.WriteError;
    }

    private int ValidateTapeImage(ITapeImage inputTape, IWritableTapeImage outputTape)
    {
        // Validates tape image format compatibility
        // Checks if input is tape-based but output format doesn't support tape images
        // Returns error if unsupported media type combination detected

        if(inputTape?.IsTape != true || outputTape is not null) return (int)ErrorNumber.NoError;

        AaruLogging.Error(UI.Input_format_contains_a_tape_image_and_is_not_supported_by_output_format);

        return (int)ErrorNumber.UnsupportedMedia;
    }

    private int ValidateSectorTags(IWritableImage outputFormat, IMediaImage inputFormat, Settings settings,
                                   out bool       useLong)
    {
        // Validates sector tag compatibility between formats
        // Sets useLong flag based on sector tag support to determine sector size (512 vs 2352 bytes)
        // Some tags like CD flags/ISRC don't require long sectors; subchannel data does
        // In force mode, skips unsupported tags; otherwise reports error if data would be lost

        useLong = inputFormat.Info.ReadableSectorTags.Count != 0;

        foreach(SectorTagType sectorTag in inputFormat.Info.ReadableSectorTags.Where(sectorTag =>
                    !outputFormat.SupportedSectorTags.Contains(sectorTag)))
        {
            if(settings.Force)
            {
                if(sectorTag != SectorTagType.CdTrackFlags &&
                   sectorTag != SectorTagType.CdTrackIsrc  &&
                   sectorTag != SectorTagType.CdSectorSubchannel)
                    useLong = false;

                continue;
            }

            AaruLogging.Error(UI.Converting_image_will_lose_sector_tag_0, sectorTag);

            AaruLogging.Error(UI
                                 .If_you_dont_care_use_force_option_This_will_skip_all_sector_tags_converting_only_user_data);

            return (int)ErrorNumber.DataWillBeLost;
        }

        return (int)ErrorNumber.NoError;
    }

    private int ValidateMediaCapabilities(IWritableImage outputFormat, IMediaImage inputFormat, MediaType mediaType,
                                          Settings       settings)
    {
        // Validates media type and media tag support in output format
        // Checks if output format supports the media type being converted
        // Validates all readable media tags are supported by output (unless force mode enabled)
        // Returns error if required features not supported and data would be lost

        if(!outputFormat.SupportedMediaTypes.Contains(mediaType))
        {
            AaruLogging.Error(UI.Output_format_does_not_support_media_type);

            return (int)ErrorNumber.UnsupportedMedia;
        }

        foreach(MediaTagType mediaTag in inputFormat.Info.ReadableMediaTags.Where(mediaTag =>
                    !outputFormat.SupportedMediaTags.Contains(mediaTag) && !settings.Force))
        {
            AaruLogging.Error(UI.Converting_image_will_lose_media_tag_0, mediaTag);
            AaruLogging.Error(UI.If_you_dont_care_use_force_option);

            return (int)ErrorNumber.DataWillBeLost;
        }

        return (int)ErrorNumber.NoError;
    }

    private IBaseWritableImage FindOutputFormat(PluginRegister plugins, string format, string outputPath)
    {
        // Discovers output format plugin by extension, GUID, or name
        // Searches writable format plugins matching any of three methods:
        // 1. By file extension (if format not specified)
        // 2. By plugin GUID (if format is valid GUID)
        // 3. By plugin name (case-insensitive string match)
        // Returns null if no match or multiple matches found

        List<IBaseWritableImage> candidates = [];

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

        switch(candidates.Count)
        {
            case 0:
                AaruLogging.WriteLine(UI.No_plugin_supports_requested_extension);

                return null;
            case > 1:
                AaruLogging.WriteLine(UI.More_than_one_plugin_supports_requested_extension);

                return null;
        }

        return candidates[0];
    }

    private int ConvertNegativeSectors(IMediaImage inputFormat, IWritableImage outputMedia, uint nominalNegativeSectors,
                                       bool        useLong,     Settings       settings)
    {
        // Converts negative sectors (pre-gap) from input to output image
        // Handles both long and short sector formats with progress indication
        // Also converts associated sector tags if present
        // Returns error code if conversion fails in non-force mode

        ErrorNumber errno = ErrorNumber.NoError;

        AnsiConsole.Progress()
                   .AutoClear(true)
                   .HideCompleted(true)
                   .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn())
                   .Start(ctx =>
                    {
                        ProgressTask mediaTask = ctx.AddTask(UI.Converting_media);
                        mediaTask.MaxValue = nominalNegativeSectors;

                        // There's no -0
                        for(uint i = 1; i <= nominalNegativeSectors; i++)
                        {
                            byte[] sector;

                            mediaTask.Description =
                                string.Format(UI.Converting_negative_sector_0_of_1, i, nominalNegativeSectors);

                            bool         result;
                            SectorStatus sectorStatus;

                            if(useLong)
                            {
                                errno = inputFormat.ReadSectorLong(i, true, out sector, out sectorStatus);

                                if(errno == ErrorNumber.NoError)
                                    result = outputMedia.WriteSectorLong(sector, i, true, sectorStatus);
                                else
                                {
                                    result = true;

                                    if(settings.Force)
                                    {
                                        AaruLogging.Error(UI.Error_0_reading_negative_sector_1_continuing, errno, i);
                                    }
                                    else
                                    {
                                        AaruLogging.Error(UI.Error_0_reading_negative_sector_1_not_continuing,
                                                          errno,
                                                          i);

                                        return;
                                    }
                                }
                            }
                            else
                            {
                                errno = inputFormat.ReadSector(i, true, out sector, out sectorStatus);

                                if(errno == ErrorNumber.NoError)
                                    result = outputMedia.WriteSector(sector, i, true, sectorStatus);
                                else
                                {
                                    result = true;

                                    if(settings.Force)
                                    {
                                        AaruLogging.Error(UI.Error_0_reading_negative_sector_1_continuing, errno, i);
                                    }
                                    else
                                    {
                                        AaruLogging.Error(UI.Error_0_reading_negative_sector_1_not_continuing,
                                                          errno,
                                                          i);

                                        return;
                                    }
                                }
                            }

                            if(!result)
                            {
                                if(settings.Force)
                                {
                                    AaruLogging.Error(UI.Error_0_writing_negative_sector_1_continuing,
                                                      outputMedia.ErrorMessage,
                                                      i);
                                }
                                else
                                {
                                    AaruLogging.Error(UI.Error_0_writing_negative_sector_1_not_continuing,
                                                      outputMedia.ErrorMessage,
                                                      i);

                                    errno = ErrorNumber.WriteError;

                                    return;
                                }
                            }

                            mediaTask.Value++;
                        }

                        mediaTask.StopTask();

                        foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags.TakeWhile(_ => useLong))
                        {
                            switch(tag)
                            {
                                case SectorTagType.AppleSonyTag:
                                case SectorTagType.AppleProfileTag:
                                case SectorTagType.PriamDataTowerTag:
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

                            if(settings.Force && !outputMedia.SupportedSectorTags.Contains(tag)) continue;

                            ProgressTask tagsTask = ctx.AddTask(UI.Converting_tags);
                            tagsTask.MaxValue = nominalNegativeSectors;

                            for(uint i = 1; i <= nominalNegativeSectors; i++)
                            {
                                tagsTask.Description = string.Format(UI.Converting_tag_1_for_negative_sector_0, i, tag);

                                bool result;

                                errno = inputFormat.ReadSectorTag(i, true, tag, out byte[] sector);

                                if(errno == ErrorNumber.NoError)
                                    result = outputMedia.WriteSectorTag(sector, i, true, tag);
                                else
                                {
                                    result = true;

                                    if(settings.Force)
                                    {
                                        AaruLogging.Error(UI.Error_0_reading_negative_sector_1_continuing, errno, i);
                                    }
                                    else
                                    {
                                        AaruLogging.Error(UI.Error_0_reading_negative_sector_1_not_continuing,
                                                          errno,
                                                          i);

                                        return;
                                    }
                                }

                                if(!result)
                                {
                                    if(settings.Force)
                                    {
                                        AaruLogging.Error(UI.Error_0_writing_negative_sector_1_continuing,
                                                          outputMedia.ErrorMessage,
                                                          i);
                                    }
                                    else
                                    {
                                        AaruLogging.Error(UI.Error_0_writing_negative_sector_1_not_continuing,
                                                          outputMedia.ErrorMessage,
                                                          i);

                                        errno = ErrorNumber.WriteError;

                                        return;
                                    }
                                }

                                tagsTask.Value++;
                            }

                            tagsTask.StopTask();
                        }
                    });

        return (int)errno;
    }

    private int ConvertOverflowSectors(IMediaImage inputFormat, IWritableImage outputMedia, uint nominalOverflowSectors,
                                       bool        useLong,     Settings       settings)
    {
        // Converts overflow sectors (lead-out) from input to output image
        // Handles both long and short sector formats with progress indication
        // Also converts associated sector tags if present
        // Returns error code if conversion fails in non-force mode

        ErrorNumber errno = ErrorNumber.NoError;

        AnsiConsole.Progress()
                   .AutoClear(true)
                   .HideCompleted(true)
                   .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn())
                   .Start(ctx =>
                    {
                        ProgressTask mediaTask = ctx.AddTask(UI.Converting_media);
                        mediaTask.MaxValue = nominalOverflowSectors;

                        for(uint i = 0; i < nominalOverflowSectors; i++)
                        {
                            byte[] sector;

                            mediaTask.Description =
                                string.Format(UI.Converting_overflow_sector_0_of_1, i, nominalOverflowSectors);

                            bool         result;
                            SectorStatus sectorStatus;

                            if(useLong)
                            {
                                errno = inputFormat.ReadSectorLong(inputFormat.Info.Sectors + i,
                                                                   false,
                                                                   out sector,
                                                                   out sectorStatus);

                                if(errno == ErrorNumber.NoError)
                                {
                                    result = outputMedia.WriteSectorLong(sector,
                                                                         inputFormat.Info.Sectors + i,
                                                                         false,
                                                                         sectorStatus);
                                }
                                else
                                {
                                    result = true;

                                    if(settings.Force)
                                    {
                                        AaruLogging.Error(UI.Error_0_reading_overflow_sector_1_continuing, errno, i);
                                    }
                                    else
                                    {
                                        AaruLogging.Error(UI.Error_0_reading_overflow_sector_1_not_continuing,
                                                          errno,
                                                          i);

                                        return;
                                    }
                                }
                            }
                            else
                            {
                                errno = inputFormat.ReadSector(inputFormat.Info.Sectors + i,
                                                               false,
                                                               out sector,
                                                               out sectorStatus);

                                if(errno == ErrorNumber.NoError)
                                {
                                    result = outputMedia.WriteSector(sector,
                                                                     inputFormat.Info.Sectors + i,
                                                                     false,
                                                                     sectorStatus);
                                }
                                else
                                {
                                    result = true;

                                    if(settings.Force)
                                    {
                                        AaruLogging.Error(UI.Error_0_reading_overflow_sector_1_continuing, errno, i);
                                    }
                                    else
                                    {
                                        AaruLogging.Error(UI.Error_0_reading_overflow_sector_1_not_continuing,
                                                          errno,
                                                          i);

                                        return;
                                    }
                                }
                            }

                            if(!result)
                            {
                                if(settings.Force)
                                {
                                    AaruLogging.Error(UI.Error_0_writing_overflow_sector_1_continuing,
                                                      outputMedia.ErrorMessage,
                                                      i);
                                }
                                else
                                {
                                    AaruLogging.Error(UI.Error_0_writing_overflow_sector_1_not_continuing,
                                                      outputMedia.ErrorMessage,
                                                      i);

                                    errno = ErrorNumber.WriteError;

                                    return;
                                }
                            }

                            mediaTask.Value++;
                        }

                        mediaTask.StopTask();

                        foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags.TakeWhile(_ => useLong))
                        {
                            switch(tag)
                            {
                                case SectorTagType.AppleSonyTag:
                                case SectorTagType.AppleProfileTag:
                                case SectorTagType.PriamDataTowerTag:
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

                            if(settings.Force && !outputMedia.SupportedSectorTags.Contains(tag)) continue;

                            ProgressTask tagsTask = ctx.AddTask(UI.Converting_tags);
                            tagsTask.MaxValue = nominalOverflowSectors;

                            for(uint i = 1; i <= nominalOverflowSectors; i++)
                            {
                                tagsTask.Description = string.Format(UI.Converting_tag_1_for_overflow_sector_0, i, tag);

                                bool result;

                                errno = inputFormat.ReadSectorTag(inputFormat.Info.Sectors + i,
                                                                  false,
                                                                  tag,
                                                                  out byte[] sector);

                                if(errno == ErrorNumber.NoError)
                                {
                                    result = outputMedia.WriteSectorTag(sector,
                                                                        inputFormat.Info.Sectors + i,
                                                                        false,
                                                                        tag);
                                }
                                else
                                {
                                    result = true;

                                    if(settings.Force)
                                    {
                                        AaruLogging.Error(UI.Error_0_reading_overflow_sector_1_continuing,
                                                          errno,
                                                          inputFormat.Info.Sectors + i);
                                    }
                                    else
                                    {
                                        AaruLogging.Error(UI.Error_0_reading_overflow_sector_1_not_continuing,
                                                          errno,
                                                          inputFormat.Info.Sectors + i);

                                        return;
                                    }
                                }

                                if(!result)
                                {
                                    if(settings.Force)
                                    {
                                        AaruLogging.Error(UI.Error_0_writing_overflow_sector_1_continuing,
                                                          outputMedia.ErrorMessage,
                                                          inputFormat.Info.Sectors + i);
                                    }
                                    else
                                    {
                                        AaruLogging.Error(UI.Error_0_writing_overflow_sector_1_not_continuing,
                                                          outputMedia.ErrorMessage,
                                                          inputFormat.Info.Sectors + i);

                                        errno = ErrorNumber.WriteError;

                                        return;
                                    }
                                }

                                tagsTask.Value++;
                            }

                            tagsTask.StopTask();
                        }
                    });

        return (int)errno;
    }

    public class Settings : ImageFamily
    {
        [Description("Take metadata from existing CICM XML sidecar.")]
        [DefaultValue(null)]
        [CommandOption("-x|--cicm-xml")]
        public string CicmXml { get; init; }
        [Description("Image comments.")]
        [DefaultValue(null)]
        [CommandOption("--comments")]
        public string Comments { get; init; }
        [Description("How many sectors to convert at once.")]
        [DefaultValue(64)]
        [CommandOption("-c|--count")]
        public int Count { get; init; }
        [Description("Who (person) created the image?")]
        [DefaultValue(null)]
        [CommandOption("--creator")]
        public string Creator { get; init; }
        [Description("Manufacturer of the drive used to read the media represented by the image.")]
        [DefaultValue(null)]
        [CommandOption("--drive-manufacturer")]
        public string DriveManufacturer { get; init; }
        [Description("Model of the drive used to read the media represented by the image.")]
        [DefaultValue(null)]
        [CommandOption("--drive-model")]
        public string DriveModel { get; init; }
        [Description("Firmware revision of the drive used to read the media represented by the image.")]
        [DefaultValue(null)]
        [CommandOption("--drive-revision")]
        public string DriveFirmwareRevision { get; init; }
        [Description("Serial number of the drive used to read the media represented by the image.")]
        [DefaultValue(null)]
        [CommandOption("--drive-serial")]
        public string DriveSerialNumber { get; init; }
        [Description("Continue conversion even if sector or media tags will be lost in the process.")]
        [DefaultValue(false)]
        [CommandOption("-f|--force")]
        public bool Force { get; init; }
        [Description("Format of the output image, as plugin name or plugin id. If not present, will try to detect it from output image extension.")]
        [DefaultValue(null)]
        [CommandOption("-p|--format")]
        public string Format { get; init; }
        [Description("Barcode of the media represented by the image.")]
        [DefaultValue(null)]
        [CommandOption("--media-barcode")]
        public string MediaBarcode { get; init; }
        [Description("Last media of the sequence the media represented by the image corresponds to.")]
        [DefaultValue(0)]
        [CommandOption("--media-lastsequence")]
        public int LastMediaSequence { get; init; }
        [Description("Manufacturer of the media represented by the image.")]
        [DefaultValue(null)]
        [CommandOption("--media-manufacturer")]
        public string MediaManufacturer { get; init; }
        [Description("Model of the media represented by the image.")]
        [DefaultValue(null)]
        [CommandOption("--media-model")]
        public string MediaModel { get; init; }
        [Description("Part number of the media represented by the image.")]
        [DefaultValue(null)]
        [CommandOption("--media-partnumber")]
        public string MediaPartNumber { get; init; }
        [Description("Number in sequence for the media represented by the image.")]
        [DefaultValue(0)]
        [CommandOption("--media-sequence")]
        public int MediaSequence { get; init; }
        [Description("Serial number of the media represented by the image.")]
        [DefaultValue(null)]
        [CommandOption("--media-serial")]
        public string MediaSerialNumber { get; init; }
        [Description("Title of the media represented by the image.")]
        [DefaultValue(null)]
        [CommandOption("--media-title")]
        public string MediaTitle { get; init; }
        [Description("Comma separated name=value pairs of options to pass to output image plugin.")]
        [DefaultValue(null)]
        [CommandOption("-O|--options")]
        public string Options { get; init; }
        [Description("Take list of dump hardware from existing resume file.")]
        [DefaultValue(null)]
        [CommandOption("-r|--resume-file")]
        public string ResumeFile { get; init; }
        [Description("Force geometry, only supported in not tape block media. Specify as C/H/S.")]
        [DefaultValue(null)]
        [CommandOption("-g|--geometry")]
        public string Geometry { get; init; }
        [Description("Store subchannel according to the sector they describe.")]
        [DefaultValue(true)]
        [CommandOption("--fix-subchannel-position")]
        public bool FixSubchannelPosition { get; init; }
        [Description("Try to fix subchannel. Implies fixing subchannel position.")]
        [DefaultValue(false)]
        [CommandOption("--fix-subchannel")]
        public bool FixSubchannel { get; init; }
        [Description("If subchannel looks OK but CRC fails, rewrite it. Implies fixing subchannel.")]
        [DefaultValue(false)]
        [CommandOption("--fix-subchannel-crc")]
        public bool FixSubchannelCrc { get; init; }
        [Description("Generates missing subchannels.")]
        [DefaultValue(false)]
        [CommandOption("--generate-subchannels")]
        public bool GenerateSubchannels { get; init; }
        [Description("Try to decrypt encrypted sectors.")]
        [DefaultValue(false)]
        [CommandOption("--decrypt")]
        public bool Decrypt { get; init; }
        [Description("Take metadata from existing Aaru Metadata sidecar.")]
        [DefaultValue(null)]
        [CommandOption("-m|--aaru-metadata")]
        public string AaruMetadata { get; init; }
        [Description("Input image path")]
        [CommandArgument(0, "<input-image>")]
        public string InputPath { get; init; }
        [Description("Output image path")]
        [CommandArgument(1, "<output-image>")]
        public string OutputPath { get; init; }
        [Description("Ignore negative sectors.")]
        [DefaultValue(false)]
        [CommandOption("--ignore-negative-sectors")]
        public bool IgnoreNegativeSectors { get; init; }
        [Description("Ignore overflow sectors.")]
        [DefaultValue(false)]
        [CommandOption("--ignore-overflow-sectors")]
        public bool IgnoreOverflowSectors { get; init; }
    }
}