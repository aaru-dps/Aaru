using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Aaru.Images;
using Aaru.Localization;
using File = System.IO.File;
using MediaType = Aaru.CommonTypes.MediaType;
using TapeFile = Aaru.CommonTypes.Structs.TapeFile;
using TapePartition = Aaru.CommonTypes.Structs.TapePartition;

namespace Aaru.Core.Image;

[SuppressMessage("Philips Naming", "PH2082:Positive Naming")]
public sealed partial class Merger
(
    string                     primaryImagePath,
    string                     secondaryImagePath,
    string                     outputImagePath,
    bool                       useSecondaryTags,
    string                     sectorsFile,
    bool                       ignoreMediaType,
    string                     comments,
    int                        count,
    string                     creator,
    string                     driveManufacturer,
    string                     driveModel,
    string                     driveFirmwareRevision,
    string                     driveSerialNumber,
    string                     format,
    string                     mediaBarcode,
    int                        lastMediaSequence,
    string                     mediaManufacturer,
    string                     mediaModel,
    string                     mediaPartNumber,
    int                        mediaSequence,
    string                     mediaSerialNumber,
    string                     mediaTitle,
    Dictionary<string, string> options,
    string                     primaryResumeFile,
    string                     secondaryResumeFile,
    string                     geometry,
    bool                       fixSubchannelPosition,
    bool                       fixSubchannel,
    bool                       fixSubchannelCrc,
    bool                       generateSubchannels,
    bool                       decrypt,
    bool                       ignoreNegativeSectors,
    bool                       ignoreOverflowSectors,
    bool                       ignoreSectorNotFound,
    int                        errorRecovery
)
{
    const string            MODULE_NAME = "Image merger";
    bool                    _aborted;
    readonly PluginRegister _plugins = PluginRegister.Singleton;
    byte[][]?               _aacsDecryptedCpsUnitKeys;

    static bool IsSkippableNotFound(ErrorNumber errno) => errno is ErrorNumber.SectorNotFound or ErrorNumber.NoData;

    public ErrorNumber Start()
    {
        // Validate sector count parameter
        if(count == 0)
        {
            StoppingErrorMessage?.Invoke(UI.Need_to_specify_more_than_zero_sectors_to_copy_at_once);

            return ErrorNumber.InvalidArgument;
        }

// Parse and validate CHS geometry if specified
        (bool success, uint cylinders, uint heads, uint sectors)? geometryResult = ParseGeometry(geometry);
        (uint cylinders, uint heads, uint sectors)?               geometryValues = null;

        if(geometryResult is not null)
        {
            if(!geometryResult.Value.success) return ErrorNumber.InvalidArgument;

            geometryValues = (geometryResult.Value.cylinders, geometryResult.Value.heads, geometryResult.Value.sectors);
        }

        // Load resume information from sidecar files

        (bool success, Resume primaryResume) = LoadMetadata(primaryResumeFile);

        if(!success) return ErrorNumber.InvalidArgument;

        (success, Resume secondaryResume) = LoadMetadata(secondaryResumeFile);

        if(!success) return ErrorNumber.InvalidArgument;

        // Verify output file doesn't already exist
        if(File.Exists(outputImagePath))
        {
            StoppingErrorMessage?.Invoke(UI.Output_file_already_exists);

            return ErrorNumber.FileExists;
        }

        (ErrorNumber errno, IMediaImage primaryImage) = GetInputImage(primaryImagePath);

        if(primaryImage is null) return errno;

        (errno, IMediaImage secondaryImage) = GetInputImage(secondaryImagePath);

        if(secondaryImage is null) return errno;

        // Get media type and handle obsolete type mappings for backwards compatibility
        MediaType primaryMediaType = primaryImage.Info.MediaType;

        // Obsolete types
#pragma warning disable 612
        primaryMediaType = primaryMediaType switch
                           {
                               MediaType.SQ1500     => MediaType.SyJet,
                               MediaType.Bernoulli  => MediaType.Bernoulli10,
                               MediaType.Bernoulli2 => MediaType.BernoulliBox2_20,
                               _                    => primaryImage.Info.MediaType
                           };
#pragma warning restore 612

        MediaType secondaryMediaType = secondaryImage.Info.MediaType;

        // Obsolete types
#pragma warning disable 612
        secondaryMediaType = secondaryMediaType switch
                             {
                                 MediaType.SQ1500     => MediaType.SyJet,
                                 MediaType.Bernoulli  => MediaType.Bernoulli10,
                                 MediaType.Bernoulli2 => MediaType.BernoulliBox2_20,
                                 _                    => secondaryImage.Info.MediaType
                             };
#pragma warning restore 612

        if(!ignoreMediaType && primaryMediaType != secondaryMediaType)
        {
            StoppingErrorMessage?.Invoke(UI.Images_have_different_media_types_cannot_merge);

            return ErrorNumber.InvalidArgument;
        }

        // Discover and load output format plugin
        IWritableImage outputFormat = FindOutputFormat(PluginRegister.Singleton, format, outputImagePath);

        if(outputFormat == null) return ErrorNumber.FormatNotFound;

        UpdateStatus?.Invoke(string.Format(UI.Output_image_format_0, outputFormat.Name));

        if(errorRecovery > 0)
        {
            if(outputFormat is not AaruFormat)
            {
                StoppingErrorMessage?.Invoke(UI.Error_recovery_is_only_supported_in_AaruFormat);

                return ErrorNumber.NotSupported;
            }

            if(errorRecovery > 100)
            {
                StoppingErrorMessage?.Invoke(UI.Maximum_error_recovery_is_100);

                return ErrorNumber.InvalidArgument;
            }
        }

        if(primaryImage.Info.Sectors != secondaryImage.Info.Sectors)
        {
            StoppingErrorMessage?.Invoke(UI.Images_have_different_number_of_sectors_cannot_merge);

            return ErrorNumber.InvalidArgument;
        }

        errno = ValidateMediaCapabilities(primaryImage, secondaryImage, outputFormat, primaryMediaType);

        if(errno != ErrorNumber.NoError) return errno;

        // Validate sector tags compatibility between formats
        errno = ValidateSectorTags(primaryImage, outputFormat, out bool useLong);

        if(errno != ErrorNumber.NoError) return errno;

        // Check and setup tape image support if needed
        var primaryTape   = primaryImage as ITapeImage;
        var secondaryTape = secondaryImage as ITapeImage;
        var outputTape    = outputFormat as IWritableTapeImage;

        errno = ValidateTapeImage(primaryTape, secondaryTape, outputTape);

        if(errno != ErrorNumber.NoError) return errno;

        InitProgress?.Invoke();
        PulseProgress?.Invoke(UI.Parsing_sectors_file);

        (List<ulong> overrideSectorsList, List<uint> overrideNegativeSectorsList) =
            ParseOverrideSectorsList(sectorsFile);

        EndProgress?.Invoke();

        if(overrideNegativeSectorsList.Contains(0))
        {
            StoppingErrorMessage?.Invoke(UI.Sectors_file_contains_invalid_sector_number_0_not_continuing);

            return ErrorNumber.InvalidArgument;
        }

        uint nominalNegativeSectors = 0;
        uint nominalOverflowSectors = 0;

        if(!ignoreNegativeSectors)
        {
            nominalNegativeSectors = primaryImage.Info.NegativeSectors;

            if(secondaryImage.Info.NegativeSectors > nominalNegativeSectors)
                nominalNegativeSectors = secondaryImage.Info.NegativeSectors;
        }

        if(!ignoreOverflowSectors)
        {
            nominalOverflowSectors = primaryImage.Info.OverflowSectors;

            if(secondaryImage.Info.OverflowSectors > nominalOverflowSectors)
                nominalOverflowSectors = secondaryImage.Info.OverflowSectors;
        }

        // Check if any override sectors is bigger than the biggest overflow sector
        ulong maxAllowedSector = primaryImage.Info.Sectors - 1 + nominalOverflowSectors;

        if(overrideSectorsList.Count > 0 && overrideSectorsList.Max() > maxAllowedSector)
        {
            StoppingErrorMessage
              ?.Invoke(string.Format(UI
                                        .Sectors_file_contains_sector_0_which_exceeds_the_maximum_allowed_sector_1_not_continuing,
                                     overrideSectorsList.Max(),
                                     maxAllowedSector));

            return ErrorNumber.InvalidArgument;
        }

        InitProgress?.Invoke();
        PulseProgress?.Invoke(UI.Calculating_sectors_to_merge);

        List<ulong> sectorsToCopyFromSecondImage =
            CalculateSectorsToCopy(primaryImage, secondaryImage, primaryResume, secondaryResume, overrideSectorsList);

        EndProgress?.Invoke();

        // Flux images might contain no decoded data, which results in a sector count of 0. We allow this if the image contains flux.
        bool containsFlux = primaryImage is IFluxImage || secondaryImage is IFluxImage;

        if(sectorsToCopyFromSecondImage.Count == 0 && !containsFlux)
        {
            StoppingErrorMessage
              ?.Invoke(UI.No_sectors_to_merge__output_image_will_be_identical_to_primary_image_not_continuing);

            return ErrorNumber.InvalidArgument;
        }

        errno = SetupTapeImage(primaryTape, secondaryTape, outputTape);

        if(errno != ErrorNumber.NoError) return errno;

        // Validate optical media capabilities (sessions, hidden tracks, etc.)
        if((outputFormat as IWritableOpticalImage)?.OpticalCapabilities.HasFlag(OpticalImageCapabilities
                                                                                   .CanStoreSessions) !=
           true &&
           (primaryImage as IOpticalMediaImage)?.Sessions?.Count > 1)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Output_format_does_not_support_sessions);

            return ErrorNumber.UnsupportedMedia;
        }

        // Check for hidden tracks support in optical media
        if((outputFormat as IWritableOpticalImage)?.OpticalCapabilities.HasFlag(OpticalImageCapabilities
                                                                                   .CanStoreHiddenTracks) !=
           true &&
           (primaryImage as IOpticalMediaImage)?.Tracks?.Any(static t => t.Sequence == 0) == true)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Output_format_does_not_support_hidden_tracks);

            return ErrorNumber.UnsupportedMedia;
        }

        // Create the output image file with appropriate settings
        errno = CreateOutputImage(primaryImage,
                                  primaryMediaType,
                                  outputFormat,
                                  nominalNegativeSectors,
                                  nominalOverflowSectors);

        if(errno != ErrorNumber.NoError) return errno;

        // Set image metadata in the output file
        errno = SetImageMetadata(primaryImage, secondaryImage, outputFormat);

        if(errno != ErrorNumber.NoError) return errno;

        // Prepare metadata and dump hardware information
        Metadata metadata = primaryImage.AaruMetadata;

        List<DumpHardware> dumpHardware =
            CalculateMergedDumpHardware(primaryImage,
                                        secondaryImage,
                                        primaryResume,
                                        secondaryResume,
                                        sectorsToCopyFromSecondImage);

        // Convert media tags from input to output format
        errno = CopyMediaTags(primaryImage, secondaryImage, outputFormat);

        if(errno != ErrorNumber.NoError) return errno;

        UpdateStatus?.Invoke(string.Format(UI.Copying_0_sectors_from_primary_image, primaryImage.Info.Sectors));

        // Perform the actual data conversion from input to output image
        if(primaryImage is IOpticalMediaImage primaryOptical     &&
           secondaryImage is IOpticalMediaImage secondaryOptical &&
           outputFormat is IWritableOpticalImage outputOptical   &&
           primaryOptical.Tracks != null)
        {
            errno = CopyOptical(primaryOptical, secondaryOptical, outputOptical, useLong, sectorsToCopyFromSecondImage);

            if(errno != ErrorNumber.NoError) return errno;
        }
        else
        {
            if(primaryTape == null || outputTape == null || !primaryTape.IsTape)
            {
                (uint cylinders, uint heads, uint sectors) chs =
                    geometryValues != null
                        ? (geometryValues.Value.cylinders, geometryValues.Value.heads, geometryValues.Value.sectors)
                        : (primaryImage.Info.Cylinders, primaryImage.Info.Heads, primaryImage.Info.SectorsPerTrack);

                UpdateStatus?.Invoke(string.Format(UI.Setting_geometry_to_0_cylinders_1_heads_and_2_sectors_per_track,
                                                   chs.cylinders,
                                                   chs.heads,
                                                   chs.sectors));

                if(!outputFormat.SetGeometry(chs.cylinders, chs.heads, chs.sectors))
                {
                    ErrorMessage?.Invoke(string.Format(UI.Error_0_setting_geometry_image_may_be_incorrect_continuing,
                                                       outputFormat.ErrorMessage));
                }
            }

            errno = CopySectorsPrimary(useLong, primaryTape?.IsTape == true, primaryImage, outputFormat);

            if(errno != ErrorNumber.NoError) return errno;

            errno = CopySectorsTagPrimary(useLong, primaryImage, outputFormat);

            if(errno != ErrorNumber.NoError) return errno;

            UpdateStatus?.Invoke(string.Format(UI.Will_copy_0_sectors_from_secondary_image,
                                               sectorsToCopyFromSecondImage.Count));

            errno = CopySectorsSecondary(useLong,
                                         primaryTape?.IsTape == true,
                                         primaryImage,
                                         outputFormat,
                                         sectorsToCopyFromSecondImage);

            if(errno != ErrorNumber.NoError) return errno;

            errno = CopySectorsTagSecondary(useLong, primaryImage, outputFormat, sectorsToCopyFromSecondImage);

            if(errno != ErrorNumber.NoError) return errno;

            if(primaryImage is IFluxImage primaryFlux && outputFormat is IWritableFluxImage outputFlux)
            {
                var secondaryFlux = secondaryImage as IFluxImage;

                UpdateStatus?.Invoke(secondaryFlux != null
                                         ? UI.Flux_merge_primary_then_secondary_appended
                                         : UI.Flux_merge_primary_flux_only_secondary_not_flux_image);

                errno = MergeFlux(primaryFlux, secondaryFlux, outputFlux);

                if(errno != ErrorNumber.NoError) return errno;
            }

            if(primaryTape != null && outputTape != null && primaryTape.IsTape)
            {
                InitProgress?.Invoke();
                var currentFile = 0;

                foreach(TapeFile tapeFile in primaryTape.Files)
                {
                    if(_aborted) break;

                    UpdateProgress?.Invoke(string.Format(UI.Copying_file_0_of_partition_1,
                                                         tapeFile.File,
                                                         tapeFile.Partition),
                                           currentFile + 1,
                                           primaryTape.Files.Count);

                    outputTape.AddFile(tapeFile);
                    currentFile++;
                }

                EndProgress?.Invoke();

                InitProgress?.Invoke();
                var currentPartition = 0;

                foreach(TapePartition tapePartition in primaryTape.TapePartitions)
                {
                    if(_aborted) break;

                    UpdateProgress?.Invoke(string.Format(UI.Copying_tape_partition_0, tapePartition.Number),
                                           currentPartition + 1,
                                           primaryTape.TapePartitions.Count);

                    outputTape.AddPartition(tapePartition);
                    currentPartition++;
                }

                EndProgress?.Invoke();
            }
        }

        if(nominalNegativeSectors > 0)
        {
            errno = CopyNegativeSectorsPrimary(useLong,
                                               primaryImage,
                                               outputFormat,
                                               nominalNegativeSectors,
                                               overrideNegativeSectorsList);

            if(errno != ErrorNumber.NoError) return errno;

            if(secondaryImage.Info.NegativeSectors > 0)
                CopyNegativeSectorsSecondary(useLong, secondaryImage, outputFormat, overrideNegativeSectorsList);
        }

        if(nominalOverflowSectors > 0)
        {
            var overrideOverflowSectorsList = overrideSectorsList
                                             .Where(sector => sector >= primaryImage.Info.Sectors)
                                             .ToList();

            errno = CopyOverflowSectorsPrimary(useLong,
                                               primaryImage,
                                               outputFormat,
                                               nominalOverflowSectors,
                                               overrideOverflowSectorsList);

            if(errno != ErrorNumber.NoError) return errno;

            if(secondaryImage.Info.OverflowSectors > 0)
                CopyOverflowSectorsSecondary(useLong, secondaryImage, outputFormat, overrideOverflowSectorsList);
        }

        bool ret;

        if(dumpHardware != null && !_aborted)
        {
            InitProgress?.Invoke();

            PulseProgress?.Invoke(UI.Writing_dump_hardware_list);

            ret = outputFormat.SetDumpHardware(dumpHardware);

            if(ret) UpdateStatus?.Invoke(UI.Written_dump_hardware_list_to_output_image);

            EndProgress?.Invoke();
        }

        if(metadata != null && !_aborted)
        {
            InitProgress?.Invoke();
            PulseProgress?.Invoke(UI.Writing_metadata);

            ret = outputFormat.SetMetadata(metadata);

            if(ret) UpdateStatus?.Invoke(UI.Written_Aaru_Metadata_to_output_image);

            EndProgress?.Invoke();
        }

        if(_aborted)
        {
            UpdateStatus?.Invoke(UI.Operation_canceled_the_output_file_is_not_correct);

            return ErrorNumber.Canceled;
        }

        InitProgress?.Invoke();
        PulseProgress?.Invoke(UI.Closing_output_image);
        bool closed = outputFormat.Close();
        EndProgress?.Invoke();

        if(!closed)
        {
            StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_closing_output_image_Contents_are_not_correct,
                                                       outputFormat.ErrorMessage));

            return ErrorNumber.WriteError;
        }

        UpdateStatus?.Invoke(UI.Merge_completed_successfully);

        return ErrorNumber.NoError;
    }

    /// <summary>Event raised when the progress bar is no longer needed</summary>
    public event EndProgressHandler EndProgress;

    /// <summary>Event raised when a progress bar is needed</summary>
    public event InitProgressHandler InitProgress;

    /// <summary>Event raised to report status updates</summary>
    public event UpdateStatusHandler UpdateStatus;

    /// <summary>Event raised to report a non-fatal error</summary>
    public event ErrorMessageHandler ErrorMessage;

    /// <summary>Event raised to report a fatal error that stops the dumping operation and should call user's attention</summary>
    public event ErrorMessageHandler StoppingErrorMessage;

    /// <summary>Event raised to update the values of a determinate progress bar</summary>
    public event UpdateProgressHandler UpdateProgress;

    /// <summary>Event raised to update the status of an indeterminate progress bar</summary>
    public event PulseProgressHandler PulseProgress;

    /// <summary>Event raised when the progress bar is no longer needed</summary>
    public event EndProgressHandler2 EndProgress2;

    /// <summary>Event raised when a progress bar is needed</summary>
    public event InitProgressHandler2 InitProgress2;

    /// <summary>Event raised to update the values of a determinate progress bar</summary>
    public event UpdateProgressHandler2 UpdateProgress2;

    public void Abort()
    {
        _aborted = true;
    }
}