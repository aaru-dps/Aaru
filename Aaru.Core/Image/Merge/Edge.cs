using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Localization;

namespace Aaru.Core.Image;

public sealed partial class Merger
{
    ErrorNumber CopyNegativeSectorsPrimary(bool useLong,         IMediaImage primaryImage, IWritableImage outputImage,
                                           uint negativeSectors, List<uint>  overrideNegativeSectors)
    {
        if(_aborted) return ErrorNumber.NoError;

        ErrorNumber errno = ErrorNumber.NoError;

        InitProgress?.Invoke();

        List<uint> notDumped = [];

        // There's no -0
        for(uint i = 1; i <= negativeSectors; i++)
        {
            if(_aborted) break;

            byte[] sector;

            UpdateProgress?.Invoke(string.Format(UI.Copying_negative_sector_0_of_1, i, negativeSectors),
                                   i,
                                   negativeSectors);

            bool         result;
            SectorStatus sectorStatus;

            if(useLong)
            {
                errno = primaryImage.ReadSectorLong(i, true, out sector, out sectorStatus);

                if(errno == ErrorNumber.NoError)
                {
                    if(sectorStatus == SectorStatus.NotDumped)
                    {
                        notDumped.Add(i);

                        continue;
                    }

                    result = outputImage.WriteSectorLong(sector, i, true, sectorStatus);
                }
                else
                {
                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_negative_sector_1_not_continuing,
                                                               errno,
                                                               i));

                    return errno;
                }
            }
            else
            {
                errno = primaryImage.ReadSector(i, true, out sector, out sectorStatus);

                if(errno == ErrorNumber.NoError)
                {
                    if(sectorStatus == SectorStatus.NotDumped)
                    {
                        notDumped.Add(i);

                        continue;
                    }

                    result = outputImage.WriteSector(sector, i, true, sectorStatus);
                }
                else
                {
                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_negative_sector_1_not_continuing,
                                                               errno,
                                                               i));

                    return errno;
                }
            }

            if(result) continue;

            StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_negative_sector_1_not_continuing,
                                                       outputImage.ErrorMessage,
                                                       i));

            return ErrorNumber.WriteError;
        }

        EndProgress?.Invoke();

        foreach(SectorTagType tag in primaryImage.Info.ReadableSectorTags.TakeWhile(_ => useLong)
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
                    // These tags are inline in long sector
                    continue;
                case SectorTagType.CdTrackFlags:
                case SectorTagType.CdTrackIsrc:
                case SectorTagType.CdTrackText:
                    // These tags are track tags
                    continue;
            }

            InitProgress?.Invoke();

            for(uint i = 1; i <= negativeSectors; i++)
            {
                if(_aborted) break;

                if(notDumped.Contains(i)) continue;

                UpdateProgress?.Invoke(string.Format(UI.Copying_tag_1_for_negative_sector_0, i, tag),
                                       i,
                                       negativeSectors);

                bool result;

                errno = primaryImage.ReadSectorTag(i, true, tag, out byte[] sector);

                if(errno == ErrorNumber.NoError)
                    result = outputImage.WriteSectorTag(sector, i, true, tag);
                else
                {
                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_negative_sector_1_not_continuing,
                                                               errno,
                                                               i));

                    return errno;
                }

                if(result) continue;

                StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_negative_sector_1_not_continuing,
                                                           outputImage.ErrorMessage,
                                                           i));

                return ErrorNumber.WriteError;
            }
        }

        overrideNegativeSectors.AddRange(notDumped.Where(t => !overrideNegativeSectors.Contains(t)));
        overrideNegativeSectors.Sort();

        return errno;
    }

    ErrorNumber CopyNegativeSectorsSecondary(bool       useLong, IMediaImage secondaryImage, IWritableImage outputImage,
                                             List<uint> overrideNegativeSectors)
    {
        if(_aborted) return ErrorNumber.NoError;

        ErrorNumber errno = ErrorNumber.NoError;

        InitProgress?.Invoke();

        List<uint> notDumped    = [];
        var        currentCount = 0;
        int        totalCount   = overrideNegativeSectors.Count;

        foreach(uint sectorAddress in overrideNegativeSectors)
        {
            if(_aborted) break;

            byte[] sector;

            UpdateProgress?.Invoke(string.Format(UI.Copying_negative_sector_0, sectorAddress),
                                   currentCount,
                                   totalCount);

            bool         result;
            SectorStatus sectorStatus;

            if(useLong)
            {
                errno = secondaryImage.ReadSectorLong(sectorAddress, true, out sector, out sectorStatus);

                if(errno == ErrorNumber.NoError)
                {
                    if(sectorStatus == SectorStatus.NotDumped)
                    {
                        notDumped.Add(sectorAddress);

                        continue;
                    }

                    result = outputImage.WriteSectorLong(sector, sectorAddress, true, sectorStatus);
                }
                else
                {
                    ErrorMessage?.Invoke(string.Format(UI.Error_0_reading_negative_sector_1_continuing,
                                                       errno,
                                                       sectorAddress));

                    continue;
                }
            }
            else
            {
                errno = secondaryImage.ReadSector(sectorAddress, true, out sector, out sectorStatus);

                if(errno == ErrorNumber.NoError)
                {
                    if(sectorStatus == SectorStatus.NotDumped)
                    {
                        notDumped.Add(sectorAddress);

                        continue;
                    }

                    result = outputImage.WriteSector(sector, sectorAddress, true, sectorStatus);
                }
                else
                {
                    ErrorMessage?.Invoke(string.Format(UI.Error_0_reading_negative_sector_1_continuing,
                                                       errno,
                                                       sectorAddress));

                    continue;
                }
            }

            currentCount++;

            if(result)
            {
                _secondaryEdgeSectors.Add((sectorAddress, true));

                continue;
            }

            ErrorMessage?.Invoke(string.Format(UI.Error_0_writing_negative_sector_1_continuing,
                                               outputImage.ErrorMessage,
                                               sectorAddress));
        }

        EndProgress?.Invoke();

        foreach(SectorTagType tag in secondaryImage.Info.ReadableSectorTags.TakeWhile(_ => useLong)
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
                    // These tags are inline in long sector
                    continue;
                case SectorTagType.CdTrackFlags:
                case SectorTagType.CdTrackIsrc:
                case SectorTagType.CdTrackText:
                    // These tags are track tags
                    continue;
            }

            InitProgress?.Invoke();

            currentCount = 0;

            foreach(uint sectorAddress in overrideNegativeSectors)
            {
                if(_aborted) break;

                if(notDumped.Contains(sectorAddress)) continue;

                UpdateProgress?.Invoke(string.Format(UI.Copying_tag_1_for_negative_sector_0, sectorAddress, tag),
                                       currentCount,
                                       totalCount);

                bool result;

                errno = secondaryImage.ReadSectorTag(sectorAddress, true, tag, out byte[] sector);

                if(errno == ErrorNumber.NoError)
                    result = outputImage.WriteSectorTag(sector, sectorAddress, true, tag);
                else
                {
                    ErrorMessage?.Invoke(string.Format(UI.Error_0_reading_negative_sector_1_continuing,
                                                       errno,
                                                       sectorAddress));

                    continue;
                }

                currentCount++;

                if(result) continue;

                ErrorMessage?.Invoke(string.Format(UI.Error_0_writing_negative_sector_1_continuing,
                                                   outputImage.ErrorMessage,
                                                   sectorAddress));
            }
        }

        return errno;
    }

    ErrorNumber CopyOverflowSectorsPrimary(bool useLong,         IMediaImage primaryImage, IWritableImage outputImage,
                                           uint overflowSectors, List<ulong> overrideOverflowSectors)
    {
        if(_aborted) return ErrorNumber.NoError;

        ErrorNumber errno = ErrorNumber.NoError;

        InitProgress?.Invoke();

        List<uint> notDumped = [];

        for(uint i = 0; i < overflowSectors; i++)
        {
            if(_aborted) break;

            byte[] sector;

            UpdateProgress?.Invoke(string.Format(UI.Copying_overflow_sector_0_of_1, i, overflowSectors),
                                   i,
                                   overflowSectors);

            bool         result;
            SectorStatus sectorStatus;

            if(useLong)
            {
                errno = primaryImage.ReadSectorLong(primaryImage.Info.Sectors + i, false, out sector, out sectorStatus);

                if(errno == ErrorNumber.NoError)
                {
                    if(sectorStatus == SectorStatus.NotDumped)
                    {
                        notDumped.Add(i);

                        continue;
                    }

                    result = outputImage.WriteSectorLong(sector, primaryImage.Info.Sectors + i, false, sectorStatus);
                }
                else
                {
                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_overflow_sector_1_not_continuing,
                                                               errno,
                                                               i));

                    return errno;
                }
            }
            else
            {
                errno = primaryImage.ReadSector(primaryImage.Info.Sectors + i, false, out sector, out sectorStatus);

                if(errno == ErrorNumber.NoError)
                {
                    if(sectorStatus == SectorStatus.NotDumped)
                    {
                        notDumped.Add(i);

                        continue;
                    }

                    result = outputImage.WriteSector(sector, primaryImage.Info.Sectors + i, false, sectorStatus);
                }
                else
                {
                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_overflow_sector_1_not_continuing,
                                                               errno,
                                                               i));

                    return errno;
                }
            }

            if(result) continue;

            StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_overflow_sector_1_not_continuing,
                                                       outputImage.ErrorMessage,
                                                       i));

            return ErrorNumber.WriteError;
        }

        EndProgress?.Invoke();

        foreach(SectorTagType tag in primaryImage.Info.ReadableSectorTags.TakeWhile(_ => useLong)
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
                    // These tags are inline in long sector
                    continue;
                case SectorTagType.CdTrackFlags:
                case SectorTagType.CdTrackIsrc:
                case SectorTagType.CdTrackText:
                    // These tags are track tags
                    continue;
            }

            InitProgress?.Invoke();

            for(uint i = 1; i < overflowSectors; i++)
            {
                if(_aborted) break;

                if(notDumped.Contains(i)) continue;

                UpdateProgress?.Invoke(string.Format(UI.Copying_tag_1_for_overflow_sector_0, i, tag),
                                       i,
                                       overflowSectors);

                bool result;

                errno = primaryImage.ReadSectorTag(primaryImage.Info.Sectors + i, false, tag, out byte[] sector);

                if(errno == ErrorNumber.NoError)
                    result = outputImage.WriteSectorTag(sector, primaryImage.Info.Sectors + i, false, tag);
                else
                {
                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_overflow_sector_1_not_continuing,
                                                               errno,
                                                               primaryImage.Info.Sectors + i));

                    return errno;
                }

                if(result) continue;

                StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_overflow_sector_1_not_continuing,
                                                           outputImage.ErrorMessage,
                                                           primaryImage.Info.Sectors + i));

                return ErrorNumber.WriteError;
            }

            EndProgress?.Invoke();
        }

        foreach(uint sector in notDumped.Where(t => !overrideOverflowSectors.Contains(primaryImage.Info.Sectors + t)))
            overrideOverflowSectors.Add(primaryImage.Info.Sectors + sector);

        overrideOverflowSectors.Sort();

        return errno;
    }

    ErrorNumber CopyOverflowSectorsSecondary(bool useLong, IMediaImage secondaryImage, IWritableImage outputImage,
                                             List<ulong> overrideOverflowSectors)
    {
        if(_aborted) return ErrorNumber.NoError;

        ErrorNumber errno = ErrorNumber.NoError;

        InitProgress?.Invoke();

        List<ulong> notDumped = [];

        int overflowSectors = overrideOverflowSectors.Count;
        var currentSector   = 0;

        foreach(ulong sectorAddress in overrideOverflowSectors)
        {
            if(_aborted) break;

            byte[] sector;

            UpdateProgress?.Invoke(string.Format(UI.Copying_overflow_sector_0, sectorAddress),
                                   currentSector,
                                   overflowSectors);

            bool         result;
            SectorStatus sectorStatus;

            if(useLong)
            {
                errno = secondaryImage.ReadSectorLong(sectorAddress, false, out sector, out sectorStatus);

                if(errno == ErrorNumber.NoError)
                {
                    if(sectorStatus == SectorStatus.NotDumped)
                    {
                        notDumped.Add(sectorAddress);

                        continue;
                    }

                    result = outputImage.WriteSectorLong(sector, sectorAddress, false, sectorStatus);
                }
                else
                {
                    ErrorMessage?.Invoke(string.Format(UI.Error_0_reading_overflow_sector_1_continuing,
                                                       errno,
                                                       sectorAddress));

                    continue;
                }
            }
            else
            {
                errno = secondaryImage.ReadSector(sectorAddress, false, out sector, out sectorStatus);

                if(errno == ErrorNumber.NoError)
                {
                    if(sectorStatus == SectorStatus.NotDumped)
                    {
                        notDumped.Add(sectorAddress);

                        continue;
                    }

                    result = outputImage.WriteSector(sector, sectorAddress, false, sectorStatus);
                }
                else
                {
                    ErrorMessage?.Invoke(string.Format(UI.Error_0_reading_overflow_sector_1_continuing,
                                                       errno,
                                                       sectorAddress));

                    continue;
                }
            }

            currentSector++;

            if(result)
            {
                _secondaryEdgeSectors.Add((sectorAddress, false));

                continue;
            }

            ErrorMessage?.Invoke(string.Format(UI.Error_0_writing_overflow_sector_1_continuing,
                                               outputImage.ErrorMessage,
                                               sectorAddress));
        }

        EndProgress?.Invoke();

        foreach(SectorTagType tag in secondaryImage.Info.ReadableSectorTags.TakeWhile(_ => useLong)
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
                    // These tags are inline in long sector
                    continue;
                case SectorTagType.CdTrackFlags:
                case SectorTagType.CdTrackIsrc:
                case SectorTagType.CdTrackText:
                    // These tags are track tags
                    continue;
            }

            InitProgress?.Invoke();

            currentSector = 0;

            foreach(ulong sectorAddress in overrideOverflowSectors)
            {
                if(_aborted) break;

                if(notDumped.Contains(sectorAddress)) continue;

                UpdateProgress?.Invoke(string.Format(UI.Copying_tag_1_for_overflow_sector_0, sectorAddress, tag),
                                       currentSector,
                                       overflowSectors);

                bool result;

                errno = secondaryImage.ReadSectorTag(sectorAddress, false, tag, out byte[] sector);

                if(errno == ErrorNumber.NoError)
                    result = outputImage.WriteSectorTag(sector, sectorAddress, false, tag);
                else
                {
                    ErrorMessage?.Invoke(string.Format(UI.Error_0_reading_overflow_sector_1_continuing,
                                                       errno,
                                                       sectorAddress));

                    continue;
                }

                currentSector++;

                if(result) continue;

                ErrorMessage?.Invoke(string.Format(UI.Error_0_writing_overflow_sector_1_continuing,
                                                   outputImage.ErrorMessage,
                                                   sectorAddress));
            }

            EndProgress?.Invoke();
        }

        return errno;
    }
}