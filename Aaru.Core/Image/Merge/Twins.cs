using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Localization;

namespace Aaru.Core.Image;

public sealed partial class Merger
{
    /// <summary>
    ///     Gets the twin sectors of an input image, checking the output format can store them.
    /// </summary>
    /// <param name="inputImage">Input image.</param>
    /// <param name="inputImagePath">Path to the input image, for messages.</param>
    /// <param name="outputImage">Output image.</param>
    /// <param name="twinSectors">Twin sectors to copy, empty if none or if they are to be discarded.</param>
    /// <returns>Error status</returns>
    ErrorNumber GetTwinSectors(IMediaImage inputImage, string inputImagePath, IWritableImage outputImage,
                               out List<TwinSectorInfo> twinSectors)
    {
        twinSectors = [];

        if(_aborted || inputImage is not ITwinSectorsImage inputTwins) return ErrorNumber.NoError;

        ErrorNumber errno = inputTwins.GetTwinSectors(out List<TwinSectorInfo> twins);

        if(errno != ErrorNumber.NoError)
        {
            StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_twin_sectors_of_image_1_not_continuing,
                                                       errno,
                                                       inputImagePath));

            return errno;
        }

        if(twins is null || twins.Count == 0) return ErrorNumber.NoError;

        if(outputImage is ITwinSectorsImage)
        {
            twinSectors = twins;

            return ErrorNumber.NoError;
        }

        if(!force)
        {
            StoppingErrorMessage
              ?.Invoke(string.Format(UI.Image_0_has_twin_sectors_output_format_does_not_support_not_continuing,
                                     inputImagePath));

            return ErrorNumber.DataWillBeLost;
        }

        ErrorMessage?.Invoke(string.Format(UI.Image_0_has_twin_sectors_output_format_does_not_support_continuing,
                                           inputImagePath));

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Whether a sector tag belongs to each twin and must be copied on its own when copying in long form.
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
            case SectorTagType.CdTrackFlags:
            case SectorTagType.CdTrackIsrc:
            case SectorTagType.CdTrackText:
                return false;
            default:
                return true;
        }
    }

    /// <summary>
    ///     Copies the twins (#1 and up) of the given sectors from an input image. Twin #0 is the sector itself, already
    ///     written by the sector copy.
    /// </summary>
    /// <param name="useLong">Copy the sectors in long form, with their tags.</param>
    /// <param name="inputImage">Image the sectors were written from.</param>
    /// <param name="outputImage">Output image, must support twin sectors.</param>
    /// <param name="twinSectors">Twinned sectors to copy.</param>
    /// <returns>Error status</returns>
    ErrorNumber CopyTwinSectors(bool useLong, IMediaImage inputImage, IWritableImage outputImage,
                                List<TwinSectorInfo> twinSectors)
    {
        if(_aborted || twinSectors.Count == 0) return ErrorNumber.NoError;

        var inputTwins  = (ITwinSectorsImage)inputImage;
        var outputTwins = (ITwinSectorsImage)outputImage;

        List<SectorTagType> tags = useLong ? inputImage.Info.ReadableSectorTags.Where(IsTwinTag).ToList() : [];

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
                    // Later twins cannot be written without leaving a gap
                    if(ignoreSectorNotFound && IsSkippableNotFound(errno))
                    {
                        ErrorMessage?.Invoke(string.Format(UI.Skipping_twin_1_of_sector_0_not_found,
                                                           sectorName,
                                                           index));

                        break;
                    }

                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_twin_1_of_sector_2_not_continuing,
                                                               errno,
                                                               index,
                                                               sectorName));

                    return errno;
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
                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_twin_1_of_sector_2_not_continuing,
                                                               outputImage.ErrorMessage,
                                                               index,
                                                               sectorName));

                    return ErrorNumber.WriteError;
                }

                foreach(SectorTagType tag in tags)
                {
                    errno = inputTwins.ReadSectorTwinTag(twin.sectorAddress,
                                                         twin.negative,
                                                         index,
                                                         tag,
                                                         out byte[] tagData);

                    // A twin does not need to carry every tag
                    if(IsSkippableNotFound(errno)) continue;

                    if(errno != ErrorNumber.NoError)
                    {
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

                    StoppingErrorMessage?.Invoke(string.Format(UI
                                                                  .Error_0_writing_tag_1_of_twin_2_of_sector_3_not_continuing,
                                                               outputImage.ErrorMessage,
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