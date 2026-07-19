using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.Localization;

namespace Aaru.Core.Image;

public partial class Convert
{
    /// <summary>
    ///     Converts negative sectors (pre-gap) from input to output image
    ///     Handles both long and short sector formats with progress indication
    ///     Also converts associated sector tags if present
    /// </summary>
    /// <returns>Error code if conversion fails in non-force mode</returns>
    ErrorNumber ConvertNegativeSectors(bool useLong)
    {
        if(_aborted) return ErrorNumber.NoError;

        ErrorNumber errno = ErrorNumber.NoError;

        InitProgress?.Invoke();

        List<uint> notDumped = [];

        // There's no -0
        for(uint i = 1; i <= _negativeSectors; i++)
        {
            if(_aborted) break;

            byte[] sector;

            UpdateProgress?.Invoke(string.Format(UI.Converting_negative_sector_0_of_1, i, _negativeSectors),
                                   i,
                                   _negativeSectors);

            bool         result;
            SectorStatus sectorStatus;

            if(useLong)
            {
                errno = _inputImage.ReadSectorLong(i, true, out sector, out sectorStatus);

                if(errno is ErrorNumber.NoError or ErrorNumber.NoData)
                {
                    if(sectorStatus == SectorStatus.NotDumped || errno == ErrorNumber.NoData)
                    {
                        notDumped.Add(i);

                        continue;
                    }

                    result = _outputImage.WriteSectorLong(sector, i, true, sectorStatus);
                }
                else
                {
                    result = true;

                    if(_force)
                        ErrorMessage?.Invoke(string.Format(UI.Error_0_reading_negative_sector_1_continuing, errno, i));
                    else
                    {
                        StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_negative_sector_1_not_continuing,
                                                                   errno,
                                                                   i));

                        return errno;
                    }
                }
            }
            else
            {
                errno = _inputImage.ReadSector(i, true, out sector, out sectorStatus);

                if(errno is ErrorNumber.NoError or ErrorNumber.NoData)
                {
                    if(sectorStatus == SectorStatus.NotDumped || errno == ErrorNumber.NoData)
                    {
                        notDumped.Add(i);

                        continue;
                    }

                    result = _outputImage.WriteSector(sector, i, true, sectorStatus);
                }
                else
                {
                    result = true;

                    if(_force)
                        ErrorMessage?.Invoke(string.Format(UI.Error_0_reading_negative_sector_1_continuing, errno, i));
                    else
                    {
                        StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_negative_sector_1_not_continuing,
                                                                   errno,
                                                                   i));

                        return errno;
                    }
                }
            }

            if(result) continue;

            if(_force)
            {
                ErrorMessage?.Invoke(string.Format(UI.Error_0_writing_negative_sector_1_continuing,
                                                   _outputImage.ErrorMessage,
                                                   i));
            }
            else
            {
                StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_negative_sector_1_not_continuing,
                                                           _outputImage.ErrorMessage,
                                                           i));

                return ErrorNumber.WriteError;
            }
        }

        EndProgress?.Invoke();

        foreach(SectorTagType tag in _inputImage.Info.ReadableSectorTags.TakeWhile(_ => useLong)
                                                .TakeWhile(_ => !_aborted))
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
                    // These tags are inline in long sector
                    continue;
                case SectorTagType.CdTrackFlags:
                case SectorTagType.CdTrackIsrc:
                case SectorTagType.CdTrackText:
                    // These tags are track tags
                    continue;
            }

            if(_force && !_outputImage.SupportedSectorTags.Contains(tag)) continue;

            InitProgress?.Invoke();

            for(uint i = 1; i <= _negativeSectors; i++)
            {
                if(_aborted) break;

                if(notDumped.Contains(i)) continue;

                UpdateProgress?.Invoke(string.Format(UI.Converting_tag_1_for_negative_sector_0, i, tag),
                                       i,
                                       _negativeSectors);

                bool result;

                errno = _inputImage.ReadSectorTag(i, true, tag, out byte[] sector);

                if(errno == ErrorNumber.NoError)
                    result = _outputImage.WriteSectorTag(sector, i, true, tag);
                else
                {
                    result = true;

                    if(_force)
                        ErrorMessage?.Invoke(string.Format(UI.Error_0_reading_negative_sector_1_continuing, errno, i));
                    else
                    {
                        StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_negative_sector_1_not_continuing,
                                                                   errno,
                                                                   i));

                        return errno;
                    }
                }

                if(result) continue;

                if(_force)
                {
                    ErrorMessage?.Invoke(string.Format(UI.Error_0_writing_negative_sector_1_continuing,
                                                       _outputImage.ErrorMessage,
                                                       i));
                }
                else
                {
                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_negative_sector_1_not_continuing,
                                                               _outputImage.ErrorMessage,
                                                               i));

                    return ErrorNumber.WriteError;
                }
            }
        }

        return errno;
    }

    /// <summary>
    ///     Converts overflow sectors (lead-out) from input to output image
    ///     Handles both long and short sector formats with progress indication
    ///     Also converts associated sector tags if present
    /// </summary>
    /// <returns>Error code if conversion fails in non-force mode</returns>
    ErrorNumber ConvertOverflowSectors(bool useLong)
    {
        if(_aborted) return ErrorNumber.NoError;

        ErrorNumber errno = ErrorNumber.NoError;

        InitProgress?.Invoke();

        List<uint> notDumped = [];

        for(uint i = 0; i < _overflowSectors; i++)
        {
            if(_aborted) break;

            byte[] sector;

            UpdateProgress?.Invoke(string.Format(UI.Converting_overflow_sector_0_of_1, i, _overflowSectors),
                                   i,
                                   _overflowSectors);

            bool         result;
            SectorStatus sectorStatus;

            if(useLong)
            {
                errno = _inputImage.ReadSectorLong(_inputImage.Info.Sectors + i, false, out sector, out sectorStatus);

                if(errno is ErrorNumber.NoError or ErrorNumber.NoData)
                {
                    if(sectorStatus == SectorStatus.NotDumped || errno == ErrorNumber.NoData)
                    {
                        notDumped.Add(i);

                        continue;
                    }

                    result = _outputImage.WriteSectorLong(sector, _inputImage.Info.Sectors + i, false, sectorStatus);
                }
                else
                {
                    result = true;

                    if(_force)
                        ErrorMessage?.Invoke(string.Format(UI.Error_0_reading_overflow_sector_1_continuing, errno, i));
                    else
                    {
                        StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_overflow_sector_1_not_continuing,
                                                                   errno,
                                                                   i));

                        return errno;
                    }
                }
            }
            else
            {
                errno = _inputImage.ReadSector(_inputImage.Info.Sectors + i, false, out sector, out sectorStatus);

                if(errno is ErrorNumber.NoError or ErrorNumber.NoData)
                {
                    if(sectorStatus == SectorStatus.NotDumped || errno == ErrorNumber.NoData)
                    {
                        notDumped.Add(i);

                        continue;
                    }

                    result = _outputImage.WriteSector(sector, _inputImage.Info.Sectors + i, false, sectorStatus);
                }
                else
                {
                    result = true;

                    if(_force)
                        ErrorMessage?.Invoke(string.Format(UI.Error_0_reading_overflow_sector_1_continuing, errno, i));
                    else
                    {
                        StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_overflow_sector_1_not_continuing,
                                                                   errno,
                                                                   i));

                        return errno;
                    }
                }
            }

            if(result) continue;

            if(_force)
            {
                ErrorMessage?.Invoke(string.Format(UI.Error_0_writing_overflow_sector_1_continuing,
                                                   _outputImage.ErrorMessage,
                                                   i));
            }
            else
            {
                StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_overflow_sector_1_not_continuing,
                                                           _outputImage.ErrorMessage,
                                                           i));

                return ErrorNumber.WriteError;
            }
        }

        EndProgress?.Invoke();

        foreach(SectorTagType tag in _inputImage.Info.ReadableSectorTags.TakeWhile(_ => useLong)
                                                .TakeWhile(_ => !_aborted))
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
                    // These tags are inline in long sector
                    continue;
                case SectorTagType.CdTrackFlags:
                case SectorTagType.CdTrackIsrc:
                case SectorTagType.CdTrackText:
                    // These tags are track tags
                    continue;
            }

            if(_force && !_outputImage.SupportedSectorTags.Contains(tag)) continue;

            InitProgress?.Invoke();

            for(uint i = 1; i < _overflowSectors; i++)
            {
                if(_aborted) break;

                if(notDumped.Contains(i)) continue;

                UpdateProgress?.Invoke(string.Format(UI.Converting_tag_1_for_overflow_sector_0, i, tag),
                                       i,
                                       _overflowSectors);

                bool result;

                errno = _inputImage.ReadSectorTag(_inputImage.Info.Sectors + i, false, tag, out byte[] sector);

                if(errno == ErrorNumber.NoError)
                    result = _outputImage.WriteSectorTag(sector, _inputImage.Info.Sectors + i, false, tag);
                else
                {
                    result = true;

                    if(_force)
                    {
                        ErrorMessage?.Invoke(string.Format(UI.Error_0_reading_overflow_sector_1_continuing,
                                                           errno,
                                                           _inputImage.Info.Sectors + i));
                    }
                    else
                    {
                        StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_overflow_sector_1_not_continuing,
                                                                   errno,
                                                                   _inputImage.Info.Sectors + i));

                        return errno;
                    }
                }

                if(result) continue;

                if(_force)
                {
                    ErrorMessage?.Invoke(string.Format(UI.Error_0_writing_overflow_sector_1_continuing,
                                                       _outputImage.ErrorMessage,
                                                       _inputImage.Info.Sectors + i));
                }
                else
                {
                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_overflow_sector_1_not_continuing,
                                                               _outputImage.ErrorMessage,
                                                               _inputImage.Info.Sectors + i));

                    return ErrorNumber.WriteError;
                }
            }

            EndProgress?.Invoke();
        }

        return errno;
    }
}