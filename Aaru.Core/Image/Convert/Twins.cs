using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Localization;

namespace Aaru.Core.Image;

public partial class Convert
{
    /// <summary>
    ///     Gets the twin sectors of the input image, checking they can be kept in the output image.
    /// </summary>
    /// <param name="canKeepTwins">False when the conversion rewrites the sectors, so their twins cannot be kept.</param>
    /// <param name="twinSectors">Twin sectors to convert, empty if none or if they are to be discarded.</param>
    /// <returns>Error status</returns>
    ErrorNumber GetTwinSectors(bool canKeepTwins, out List<TwinSectorInfo> twinSectors)
    {
        twinSectors = [];

        if(_aborted || _inputImage is not ITwinSectorsImage inputTwins) return ErrorNumber.NoError;

        ErrorNumber errno = inputTwins.GetTwinSectors(out List<TwinSectorInfo> twins);

        if(errno != ErrorNumber.NoError)
        {
            StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_twin_sectors_of_image_1_not_continuing,
                                                       errno,
                                                       _inputPath));

            return errno;
        }

        if(twins is null || twins.Count == 0) return ErrorNumber.NoError;

        if(_outputImage is ITwinSectorsImage && canKeepTwins)
        {
            twinSectors = twins;

            return ErrorNumber.NoError;
        }

        if(!_force)
        {
            StoppingErrorMessage?.Invoke(string.Format(canKeepTwins
                                                           ? UI
                                                              .Image_0_has_twin_sectors_output_format_does_not_support_not_continuing
                                                           : UI
                                                              .Image_0_has_twin_sectors_that_cannot_be_kept_converting_this_media_not_continuing,
                                                       _inputPath) +
                                         Environment.NewLine +
                                         UI.If_you_dont_care_use_force_option);

            return ErrorNumber.DataWillBeLost;
        }

        ErrorMessage?.Invoke(string.Format(canKeepTwins
                                               ? UI.Image_0_has_twin_sectors_output_format_does_not_support_continuing
                                               : UI
                                                  .Image_0_has_twin_sectors_that_cannot_be_kept_converting_this_media_continuing,
                                           _inputPath));

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Whether a sector tag belongs to each twin and must be converted on its own when converting in long form.
    /// </summary>
    /// <param name="tag">Sector tag.</param>
    /// <returns><c>false</c> for tags inline in the long sector and for track tags shared by all twins.</returns>
    static bool IsTwinTag(SectorTagType tag)
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
            case SectorTagType.BluRaySectorEdc:
            case SectorTagType.CdTrackFlags:
            case SectorTagType.CdTrackIsrc:
            case SectorTagType.CdTrackText:
                return false;
            default:
                return true;
        }
    }

    /// <summary>
    ///     Converts the twins (#1 and up) of the given sectors. Twin #0 is the sector itself, already written by the
    ///     sector conversion.
    /// </summary>
    /// <param name="useLong">Convert the sectors in long form, with their tags.</param>
    /// <param name="twinSectors">Twinned sectors to convert.</param>
    /// <returns>Error status</returns>
    ErrorNumber ConvertTwinSectors(bool useLong, List<TwinSectorInfo> twinSectors)
    {
        if(_aborted || twinSectors.Count == 0) return ErrorNumber.NoError;

        var inputTwins  = (ITwinSectorsImage)_inputImage;
        var outputTwins = (ITwinSectorsImage)_outputImage;

        List<SectorTagType> tags = useLong
                                       ? _inputImage.Info.ReadableSectorTags
                                                    .Where(tag => IsTwinTag(tag) &&
                                                                  (!_force ||
                                                                   _outputImage.SupportedSectorTags.Contains(tag)))
                                                    .ToList()
                                       : [];

        UpdateStatus?.Invoke(string.Format(UI.Copying_twin_sectors_of_0_sectors, twinSectors.Count));

        InitProgress?.Invoke();
        var doneSectors = 0;

        foreach(TwinSectorInfo twin in twinSectors)
        {
            if(_aborted) break;

            string sectorName = twin.negative ? $"-{twin.sectorAddress}" : twin.sectorAddress.ToString();

            for(uint index = 1; index < twin.count; index++)
            {
                UpdateProgress?.Invoke(string.Format(UI.Copying_twin_1_of_sector_0, sectorName, index),
                                       doneSectors,
                                       twinSectors.Count);

                ErrorNumber errno = useLong
                                        ? inputTwins.ReadSectorTwinLong(twin.sectorAddress,
                                                                        twin.negative,
                                                                        index,
                                                                        out byte[] sector,
                                                                        out SectorStatus sectorStatus)
                                        : inputTwins.ReadSectorTwin(twin.sectorAddress,
                                                                    twin.negative,
                                                                    index,
                                                                    out sector,
                                                                    out sectorStatus);

                if(errno != ErrorNumber.NoError)
                {
                    if(!_force)
                    {
                        StoppingErrorMessage?.Invoke(string.Format(UI
                                                                      .Error_0_reading_twin_1_of_sector_2_not_continuing,
                                                                   errno,
                                                                   index,
                                                                   sectorName));

                        return errno;
                    }

                    // Later twins cannot be written without leaving a gap
                    ErrorMessage?.Invoke(string.Format(UI.Error_0_reading_twin_1_of_sector_2_continuing,
                                                       errno,
                                                       index,
                                                       sectorName));

                    break;
                }

                errno = useLong
                            ? outputTwins.WriteSectorTwinLong(twin.sectorAddress,
                                                              twin.negative,
                                                              index,
                                                              sector,
                                                              sectorStatus)
                            : outputTwins.WriteSectorTwin(twin.sectorAddress,
                                                          twin.negative,
                                                          index,
                                                          sector,
                                                          sectorStatus);

                if(errno != ErrorNumber.NoError)
                {
                    if(!_force)
                    {
                        StoppingErrorMessage?.Invoke(string.Format(UI
                                                                      .Error_0_writing_twin_1_of_sector_2_not_continuing,
                                                                   _outputImage.ErrorMessage,
                                                                   index,
                                                                   sectorName));

                        return ErrorNumber.WriteError;
                    }

                    // Later twins cannot be written without leaving a gap
                    ErrorMessage?.Invoke(string.Format(UI.Error_0_writing_twin_1_of_sector_2_continuing,
                                                       _outputImage.ErrorMessage,
                                                       index,
                                                       sectorName));

                    break;
                }

                foreach(SectorTagType tag in tags)
                {
                    errno = inputTwins.ReadSectorTwinTag(twin.sectorAddress,
                                                         twin.negative,
                                                         index,
                                                         tag,
                                                         out byte[] tagData);

                    // A twin does not need to carry every tag
                    if(errno is ErrorNumber.SectorNotFound or ErrorNumber.NoData) continue;

                    if(errno != ErrorNumber.NoError)
                    {
                        if(_force)
                        {
                            ErrorMessage?.Invoke(string.Format(UI
                                                                  .Error_0_reading_tag_1_of_twin_2_of_sector_3_continuing,
                                                               errno,
                                                               tag,
                                                               index,
                                                               sectorName));

                            continue;
                        }

                        StoppingErrorMessage?.Invoke(string.Format(UI
                                                                      .Error_0_reading_tag_1_of_twin_2_of_sector_3_not_continuing,
                                                                   errno,
                                                                   tag,
                                                                   index,
                                                                   sectorName));

                        return errno;
                    }

                    errno = outputTwins.WriteSectorTwinTag(twin.sectorAddress, twin.negative, index, tagData, tag);

                    if(errno == ErrorNumber.NoError) continue;

                    if(_force)
                    {
                        ErrorMessage?.Invoke(string.Format(UI.Error_0_writing_tag_1_of_twin_2_of_sector_3_continuing,
                                                           _outputImage.ErrorMessage,
                                                           tag,
                                                           index,
                                                           sectorName));

                        continue;
                    }

                    StoppingErrorMessage?.Invoke(string.Format(UI
                                                                  .Error_0_writing_tag_1_of_twin_2_of_sector_3_not_continuing,
                                                               _outputImage.ErrorMessage,
                                                               tag,
                                                               index,
                                                               sectorName));

                    return ErrorNumber.WriteError;
                }
            }

            doneSectors++;
        }

        EndProgress?.Invoke();

        return ErrorNumber.NoError;
    }
}