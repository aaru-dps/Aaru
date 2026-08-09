using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Aaru.Images;
using Aaru.Localization;
using MediaType = Aaru.CommonTypes.MediaType;
using TapeFile = Aaru.CommonTypes.Structs.TapeFile;
using TapePartition = Aaru.CommonTypes.Structs.TapePartition;

namespace Aaru.Core.Image;

public partial class Convert
{
    const    string                                      MODULE_NAME = "Image Conversion";
    readonly bool                                        _bypassPs3Decryption;
    readonly bool                                        _bypassWiiDecryption;
    readonly bool                                        _bypassWiiuDecryption;
    readonly string                                      _comments;
    readonly uint                                        _count;
    readonly string                                      _creator;
    readonly bool                                        _decrypt;
    readonly string                                      _driveFirmwareRevision;
    readonly string                                      _driveManufacturer;
    readonly string                                      _driveModel;
    readonly string                                      _driveSerialNumber;
    readonly int                                         _errorRecovery;
    readonly bool                                        _fixSubchannel;
    readonly bool                                        _fixSubchannelCrc;
    readonly bool                                        _fixSubchannelPosition;
    readonly bool                                        _force;
    readonly bool                                        _generateSubchannels;
    readonly (uint cylinders, uint heads, uint sectors)? _geometryValues;
    readonly IMediaImage                                 _inputImage;
    readonly string                                      _inputPath;
    readonly int                                         _lastMediaSequence;
    readonly string                                      _mediaBarcode;
    readonly string                                      _mediaManufacturer;
    readonly string                                      _mediaModel;
    readonly string                                      _mediaPartNumber;
    readonly int                                         _mediaSequence;
    readonly string                                      _mediaSerialNumber;
    readonly string                                      _mediaTitle;
    readonly MediaType                                   _mediaType;
    readonly uint                                        _negativeSectors;
    readonly IWritableImage                              _outputImage;
    readonly string                                      _outputPath;
    readonly uint                                        _overflowSectors;
    readonly Dictionary<string, string>                  _parsedOptions;
    readonly PluginRegister                              _plugins;
    readonly Resume                                      _resume;
    readonly Metadata                                    _sidecar;
    volatile bool                                        _aborted;

    // TODO: Abort
    public Convert(IMediaImage inputImage, IWritableImage outputImage, MediaType mediaType, bool force,
                   string outputPath, Dictionary<string, string> parsedOptions, uint negativeSectors,
                   uint overflowSectors, string comments, string creator, string driveFirmwareRevision,
                   string driveManufacturer, string driveModel, string driveSerialNumber, int lastMediaSequence,
                   string mediaBarcode, string mediaManufacturer, string mediaModel, string mediaPartNumber,
                   int mediaSequence, string mediaSerialNumber, string mediaTitle, bool decrypt, uint count,
                   PluginRegister plugins, bool fixSubchannelPosition, bool fixSubchannel, bool fixSubchannelCrc,
                   bool generateSubchannels, (uint cylinders, uint heads, uint sectors)? geometryValues, Resume resume,
                   Metadata sidecar, bool bypassPs3Decryption, bool bypassWiiuDecryption, bool bypassWiiDecryption,
                   string inputPath, int errorRecovery)
    {
        _inputImage            = inputImage;
        _outputImage           = outputImage;
        _mediaType             = mediaType;
        _force                 = force;
        _outputPath            = outputPath;
        _parsedOptions         = parsedOptions;
        _negativeSectors       = negativeSectors;
        _overflowSectors       = overflowSectors;
        _comments              = comments;
        _creator               = creator;
        _driveFirmwareRevision = driveFirmwareRevision;
        _driveManufacturer     = driveManufacturer;
        _driveModel            = driveModel;
        _driveSerialNumber     = driveSerialNumber;
        _lastMediaSequence     = lastMediaSequence;
        _mediaBarcode          = mediaBarcode;
        _mediaManufacturer     = mediaManufacturer;
        _mediaModel            = mediaModel;
        _mediaPartNumber       = mediaPartNumber;
        _mediaSequence         = mediaSequence;
        _mediaSerialNumber     = mediaSerialNumber;
        _mediaTitle            = mediaTitle;
        _decrypt               = decrypt;
        _count                 = count;
        _plugins               = plugins;
        _fixSubchannelPosition = fixSubchannelPosition;
        _fixSubchannel         = fixSubchannel;
        _fixSubchannelCrc      = fixSubchannelCrc;
        _generateSubchannels   = generateSubchannels;
        _geometryValues        = geometryValues;
        _resume                = resume;
        _sidecar               = sidecar;
        _bypassPs3Decryption   = bypassPs3Decryption;
        _bypassWiiuDecryption  = bypassWiiuDecryption;
        _bypassWiiDecryption   = bypassWiiDecryption;
        _inputPath             = inputPath;
        _errorRecovery         = errorRecovery;
    }

    public ErrorNumber Start()
    {
        // Validate that output format supports the media type and tags
        ErrorNumber errno = ValidateMediaCapabilities();

        if(errno != ErrorNumber.NoError) return errno;

        // Validate sector tags compatibility between formats
        errno = ValidateSectorTags(out bool useLong);

        if(errno != ErrorNumber.NoError) return errno;

        // Check and setup tape image support if needed
        var inputTape  = _inputImage as ITapeImage;
        var outputTape = _outputImage as IWritableTapeImage;

        errno = ValidateTapeImage(inputTape, outputTape);

        if(errno != ErrorNumber.NoError) return errno;

        var ret = false;

        errno = SetupTapeImage(inputTape, outputTape);

        if(errno != ErrorNumber.NoError) return errno;

        // Validate optical media capabilities (sessions, hidden tracks, etc.)
        if((_outputImage as IWritableOpticalImage)?.OpticalCapabilities.HasFlag(OpticalImageCapabilities
                                                                                   .CanStoreSessions) !=
           true &&
           (_inputImage as IOpticalMediaImage)?.Sessions?.Count > 1)
        {
            // TODO: Disabled until 6.0
            /*if(!_force)
            {*/
            StoppingErrorMessage?.Invoke(Localization.Core.Output_format_does_not_support_sessions);

            return ErrorNumber.UnsupportedMedia;
            /*}

            StoppingErrorMessage?.Invoke("Output format does not support sessions, this will end in a loss of data, continuing...");*/
        }

        // Check for hidden tracks support in optical media
        if((_outputImage as IWritableOpticalImage)?.OpticalCapabilities.HasFlag(OpticalImageCapabilities
                                                                                   .CanStoreHiddenTracks) !=
           true &&
           (_inputImage as IOpticalMediaImage)?.Tracks?.Any(static t => t.Sequence == 0) == true)
        {
            // TODO: Disabled until 6.0
            /*if(!_force)
            {*/
            StoppingErrorMessage?.Invoke(Localization.Core.Output_format_does_not_support_hidden_tracks);

            return ErrorNumber.UnsupportedMedia;
            /*}

            StoppingErrorMessage?.Invoke("Output format does not support sessions, this will end in a loss of data, continuing...");*/
        }

        // Create the output image file with appropriate settings
        errno = CreateOutputImage();

        if(errno != ErrorNumber.NoError) return errno;

        // Set image metadata in the output file
        errno = SetImageMetadata();

        if(errno != ErrorNumber.NoError) return errno;

        // Prepare metadata and dump hardware information
        Metadata           metadata     = _inputImage.AaruMetadata;
        List<DumpHardware> dumpHardware = _inputImage.DumpHardware;

        // Determine if PS3 conversion path should be active
        bool isPs3Conversion = _outputImage is AaruFormat                        &&
                               _mediaType is MediaType.PS3BD or MediaType.PS3DVD &&
                               !_bypassPs3Decryption;

        // Determine if Wii U conversion path should be active
        bool isWiiuConversion = _outputImage is AaruFormat && _mediaType == MediaType.WUOD && !_bypassWiiuDecryption;

        // Determine if GameCube/Wii conversion path should be active
        bool isNgcwConversion = _outputImage is AaruFormat                   &&
                                _mediaType is MediaType.GOD or MediaType.WOD &&
                                !(_mediaType == MediaType.WOD && _bypassWiiDecryption);

        // Inject PS3-specific media tags before copying normal media tags
        if(isPs3Conversion)
        {
            errno = InjectPs3MediaTags();

            if(errno != ErrorNumber.NoError) return errno;
        }

        // Inject Wii U-specific media tags before copying normal media tags
        if(isWiiuConversion)
        {
            errno = InjectWiiuMediaTags();

            if(errno != ErrorNumber.NoError) return errno;
        }

        // Inject GameCube/Wii-specific media tags before copying normal media tags
        if(isNgcwConversion)
        {
            errno = InjectNgcwMediaTags();

            if(errno != ErrorNumber.NoError) return errno;
        }

        // Convert media tags from input to output format
        errno = ConvertMediaTags();

        if(errno != ErrorNumber.NoError && !_force) return errno;

        UpdateStatus?.Invoke(string.Format(UI._0_sectors_to_convert, _inputImage.Info.Sectors));

        // Perform the actual data conversion from input to output image
        if(_inputImage is IOpticalMediaImage inputOptical      &&
           _outputImage is IWritableOpticalImage outputOptical &&
           inputOptical.Tracks != null                         &&
           !isPs3Conversion                                    &&
           !isWiiuConversion                                   &&
           !isNgcwConversion)
        {
            errno = ConvertOptical(inputOptical, outputOptical, useLong);

            if(errno != ErrorNumber.NoError) return errno;
        }
        else if(isWiiuConversion)
        {
            if(_inputImage is IOpticalMediaImage wiiuInputOptical      &&
               _outputImage is IWritableOpticalImage wiiuOutputOptical &&
               wiiuInputOptical.Tracks != null)
            {
                if(!wiiuOutputOptical.SetTracks(wiiuInputOptical.Tracks))
                {
                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_sending_tracks_list_to_output_image,
                                                               wiiuOutputOptical.ErrorMessage));

                    return ErrorNumber.WriteError;
                }
            }

            errno = ConvertWiiuSectors();

            if(errno != ErrorNumber.NoError) return errno;
        }
        else if(isPs3Conversion)
        {
            if(_inputImage is IOpticalMediaImage ps3InputOptical      &&
               _outputImage is IWritableOpticalImage ps3OutputOptical &&
               ps3InputOptical.Tracks != null)
            {
                if(!ps3OutputOptical.SetTracks(ps3InputOptical.Tracks))
                {
                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_sending_tracks_list_to_output_image,
                                                               ps3OutputOptical.ErrorMessage));

                    return ErrorNumber.WriteError;
                }
            }

            errno = ConvertPs3Sectors();

            if(errno != ErrorNumber.NoError) return errno;
        }
        else if(isNgcwConversion)
        {
            if(_inputImage is IOpticalMediaImage ngcwInputOptical      &&
               _outputImage is IWritableOpticalImage ngcwOutputOptical &&
               ngcwInputOptical.Tracks != null)
            {
                if(!ngcwOutputOptical.SetTracks(ngcwInputOptical.Tracks))
                {
                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_sending_tracks_list_to_output_image,
                                                               ngcwOutputOptical.ErrorMessage));

                    return ErrorNumber.WriteError;
                }
            }

            errno = ConvertNgcwSectors(useLong);

            if(errno != ErrorNumber.NoError) return errno;
        }
        else
        {
            if(inputTape == null || outputTape == null || !inputTape.IsTape)
            {
                (uint cylinders, uint heads, uint sectors) chs =
                    _geometryValues != null
                        ? (_geometryValues.Value.cylinders, _geometryValues.Value.heads, _geometryValues.Value.sectors)
                        : (_inputImage.Info.Cylinders, _inputImage.Info.Heads, _inputImage.Info.SectorsPerTrack);

                UpdateStatus?.Invoke(string.Format(UI.Setting_geometry_to_0_cylinders_1_heads_and_2_sectors_per_track,
                                                   chs.cylinders,
                                                   chs.heads,
                                                   chs.sectors));

                if(!_outputImage.SetGeometry(chs.cylinders, chs.heads, chs.sectors))
                {
                    ErrorMessage?.Invoke(string.Format(UI.Error_0_setting_geometry_image_may_be_incorrect_continuing,
                                                       _outputImage.ErrorMessage));
                }
            }

            errno = ConvertSectors(useLong, inputTape?.IsTape == true);

            if(errno != ErrorNumber.NoError) return errno;

            errno = ConvertSectorsTags(useLong);

            if(errno != ErrorNumber.NoError) return errno;

            if(_inputImage is IFluxImage inputFlux && _outputImage is IWritableFluxImage outputFlux)
            {
                errno = ConvertFlux(inputFlux, outputFlux);

                if(errno != ErrorNumber.NoError) return errno;
            }

            if(inputTape != null && outputTape != null && inputTape.IsTape)
            {
                InitProgress?.Invoke();
                var currentFile = 0;

                foreach(TapeFile tapeFile in inputTape.Files)
                {
                    if(_aborted) break;

                    UpdateProgress?.Invoke(string.Format(UI.Converting_file_0_of_partition_1,
                                                         tapeFile.File,
                                                         tapeFile.Partition),
                                           currentFile + 1,
                                           inputTape.Files.Count);

                    outputTape.AddFile(tapeFile);
                    currentFile++;
                }

                EndProgress?.Invoke();

                InitProgress?.Invoke();
                var currentPartition = 0;

                foreach(TapePartition tapePartition in inputTape.TapePartitions)
                {
                    if(_aborted) break;

                    UpdateProgress?.Invoke(string.Format(UI.Converting_tape_partition_0, tapePartition.Number),
                                           currentPartition + 1,
                                           inputTape.TapePartitions.Count);

                    outputTape.AddPartition(tapePartition);
                    currentPartition++;
                }

                EndProgress?.Invoke();
            }
        }

        if(!isPs3Conversion && !isWiiuConversion && _negativeSectors > 0)
        {
            errno = ConvertNegativeSectors(useLong);

            if(errno != ErrorNumber.NoError) return errno;
        }

        if(!isPs3Conversion && !isWiiuConversion && _overflowSectors > 0)
        {
            errno = ConvertOverflowSectors(useLong);

            if(errno != ErrorNumber.NoError) return errno;
        }

        if((_resume != null || dumpHardware != null) && !_aborted)
        {
            InitProgress?.Invoke();

            PulseProgress?.Invoke(UI.Writing_dump_hardware_list);

            if(_resume != null)
                ret                           = _outputImage.SetDumpHardware(_resume.Tries);
            else if(dumpHardware != null) ret = _outputImage.SetDumpHardware(dumpHardware);

            if(ret) UpdateStatus?.Invoke(UI.Written_dump_hardware_list_to_output_image);

            EndProgress?.Invoke();
        }

        ret = false;

        if((_sidecar != null || metadata != null) && !_aborted)
        {
            InitProgress?.Invoke();
            PulseProgress?.Invoke(UI.Writing_metadata);

            if(_sidecar != null)
                ret                       = _outputImage.SetMetadata(_sidecar);
            else if(metadata != null) ret = _outputImage.SetMetadata(metadata);

            if(ret) UpdateStatus?.Invoke(UI.Written_Aaru_Metadata_to_output_image);

            EndProgress?.Invoke();
        }

        if(_aborted)
        {
            UpdateStatus?.Invoke(Localization.Core.Operation_aborted_by_user);
            UpdateStatus?.Invoke(UI.Operation_canceled_the_output_file_is_not_correct);

            return ErrorNumber.Canceled;
        }

        // After all metadata has been copied, enrich title/part number from PS3 sources if still missing
        if(isPs3Conversion) EnrichPs3TitleAndPartNumber();

        // After all metadata has been copied, enrich product code and disc number from Wii U disc header
        if(isWiiuConversion) EnrichWiiuMetadata();

        // After all metadata has been copied, enrich title/part number from GameCube/Wii disc header
        if(isNgcwConversion) EnrichNgcwMetadata();

        var closed = false;

        InitProgress?.Invoke();
        PulseProgress?.Invoke(UI.Closing_output_image);
        closed = _outputImage.Close();
        EndProgress?.Invoke();

        if(!closed)
        {
            StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_closing_output_image_Contents_are_not_correct,
                                                       _outputImage.ErrorMessage));

            return ErrorNumber.WriteError;
        }

        UpdateStatus?.Invoke(UI.Conversion_done);

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