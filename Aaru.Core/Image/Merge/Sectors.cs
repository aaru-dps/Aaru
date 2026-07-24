using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Localization;

namespace Aaru.Core.Image;

public sealed partial class Merger
{
    ErrorNumber CopySectorsPrimary(bool useLong, bool isTape, IMediaImage primaryImage, IWritableImage outputImage)
    {
        if(_aborted) return ErrorNumber.NoError;

        InitProgress?.Invoke();
        ulong doneSectors = 0;

        while(doneSectors < primaryImage.Info.Sectors)
        {
            byte[] sector;

            uint sectorsToDo;

            if(isTape)
                sectorsToDo = 1;
            else if(primaryImage.Info.Sectors - doneSectors >= (ulong)count)
                sectorsToDo = (uint)count;
            else
                sectorsToDo = (uint)(primaryImage.Info.Sectors - doneSectors);

            UpdateProgress?.Invoke(string.Format(UI.Copying_sectors_0_to_1, doneSectors, doneSectors + sectorsToDo),
                                   (long)doneSectors,
                                   (long)primaryImage.Info.Sectors);

            bool         result;
            SectorStatus sectorStatus      = SectorStatus.NotDumped;
            var          sectorStatusArray = new SectorStatus[1];

            ErrorNumber errno;

            if(useLong)
            {
                errno = sectorsToDo == 1
                            ? primaryImage.ReadSectorLong(doneSectors, false, out sector, out sectorStatus)
                            : primaryImage.ReadSectorsLong(doneSectors,
                                                           false,
                                                           sectorsToDo,
                                                           out sector,
                                                           out sectorStatusArray);

                if(errno == ErrorNumber.NoError)
                {
                    result = sectorsToDo == 1
                                 ? outputImage.WriteSectorLong(sector, doneSectors, false, sectorStatus)
                                 : outputImage.WriteSectorsLong(sector,
                                                                doneSectors,
                                                                false,
                                                                sectorsToDo,
                                                                sectorStatusArray);
                }
                else if(ignoreSectorNotFound && IsSkippableNotFound(errno))
                {
                    ErrorMessage?.Invoke(string.Format(UI.Skipping_sector_0_not_found, doneSectors));
                    doneSectors += sectorsToDo;

                    continue;
                }
                else
                {
                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_sector_1_not_continuing,
                                                               errno,
                                                               doneSectors));

                    return errno;
                }
            }
            else
            {
                errno = sectorsToDo == 1
                            ? primaryImage.ReadSector(doneSectors, false, out sector, out sectorStatus)
                            : primaryImage.ReadSectors(doneSectors,
                                                       false,
                                                       sectorsToDo,
                                                       out sector,
                                                       out sectorStatusArray);

                if(errno == ErrorNumber.NoError)
                {
                    result = sectorsToDo == 1
                                 ? outputImage.WriteSector(sector, doneSectors, false, sectorStatus)
                                 : outputImage.WriteSectors(sector, doneSectors, false, sectorsToDo, sectorStatusArray);
                }
                else if(ignoreSectorNotFound && IsSkippableNotFound(errno))
                {
                    ErrorMessage?.Invoke(string.Format(UI.Skipping_sector_0_not_found, doneSectors));
                    doneSectors += sectorsToDo;

                    continue;
                }
                else
                {
                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_sector_1_not_continuing,
                                                               errno,
                                                               doneSectors));

                    return errno;
                }
            }

            if(!result)
            {
                StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_sector_1_not_continuing,
                                                           outputImage.ErrorMessage,
                                                           doneSectors));

                return ErrorNumber.WriteError;
            }

            doneSectors += sectorsToDo;
        }

        return ErrorNumber.NoError;
    }

    ErrorNumber CopySectorsTagPrimary(bool useLong, IMediaImage primaryImage, IWritableImage outputImage)
    {
        if(_aborted) return ErrorNumber.NoError;

        foreach(SectorTagType tag in primaryImage.Info.ReadableSectorTags.TakeWhile(_ => useLong))
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
            }


            ulong doneSectors = 0;

            InitProgress?.Invoke();

            while(doneSectors < primaryImage.Info.Sectors)
            {
                uint sectorsToDo;

                if(primaryImage.Info.Sectors - doneSectors >= (ulong)count)
                    sectorsToDo = (uint)count;
                else
                    sectorsToDo = (uint)(primaryImage.Info.Sectors - doneSectors);

                UpdateProgress?.Invoke(string.Format(UI.Copying_tag_2_for_sectors_0_to_1,
                                                     doneSectors,
                                                     doneSectors + sectorsToDo,
                                                     tag),
                                       (long)doneSectors,
                                       (long)primaryImage.Info.Sectors);

                bool result;

                ErrorNumber errno = sectorsToDo == 1
                                        ? primaryImage.ReadSectorTag(doneSectors, false, tag, out byte[] sector)
                                        : primaryImage.ReadSectorsTag(doneSectors, false, sectorsToDo, tag, out sector);

                if(errno == ErrorNumber.NoError)
                {
                    result = sectorsToDo == 1
                                 ? outputImage.WriteSectorTag(sector, doneSectors, false, tag)
                                 : outputImage.WriteSectorsTag(sector, doneSectors, false, sectorsToDo, tag);
                }
                else if(ignoreSectorNotFound && IsSkippableNotFound(errno))
                {
                    ErrorMessage?.Invoke(string.Format(UI.Skipping_tag_0_for_sector_1_not_found, tag, doneSectors));
                    doneSectors += sectorsToDo;

                    continue;
                }
                else
                {
                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_sector_1_not_continuing,
                                                               errno,
                                                               doneSectors));

                    return errno;
                }

                if(!result)
                {
                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_sector_1_not_continuing,
                                                               outputImage.ErrorMessage,
                                                               doneSectors));

                    return ErrorNumber.WriteError;
                }

                doneSectors += sectorsToDo;
            }

            EndProgress?.Invoke();
        }

        return ErrorNumber.NoError;
    }

    ErrorNumber CopySectorsSecondary(bool useLong, bool isTape, IMediaImage secondaryImage, IWritableImage outputImage,
                                     List<ulong> sectorsToCopy)
    {
        if(_aborted) return ErrorNumber.NoError;

        InitProgress?.Invoke();
        var doneSectors        = 0;
        int totalSectorsToCopy = sectorsToCopy.Count(t => t < secondaryImage.Info.Sectors);

        foreach(ulong sectorAddress in sectorsToCopy.Where(t => t < secondaryImage.Info.Sectors))
        {
            byte[] sector;

            UpdateProgress?.Invoke(string.Format(UI.Copying_sector_0, sectorAddress), doneSectors, totalSectorsToCopy);

            bool result;

            ErrorNumber errno;

            if(useLong)
            {
                errno = secondaryImage.ReadSectorLong(sectorAddress, false, out sector, out SectorStatus sectorStatus);

                if(errno == ErrorNumber.NoError)
                    result = outputImage.WriteSectorLong(sector, sectorAddress, false, sectorStatus);
                else if(ignoreSectorNotFound && IsSkippableNotFound(errno))
                {
                    ErrorMessage?.Invoke(string.Format(UI.Skipping_sector_0_not_found, sectorAddress));
                    doneSectors++;

                    continue;
                }
                else
                {
                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_sector_1_not_continuing,
                                                               errno,
                                                               doneSectors));

                    return errno;
                }
            }
            else
            {
                errno = secondaryImage.ReadSector(sectorAddress, false, out sector, out SectorStatus sectorStatus);

                if(errno == ErrorNumber.NoError)
                    result = outputImage.WriteSector(sector, sectorAddress, false, sectorStatus);
                else if(ignoreSectorNotFound && IsSkippableNotFound(errno))
                {
                    ErrorMessage?.Invoke(string.Format(UI.Skipping_sector_0_not_found, sectorAddress));
                    doneSectors++;

                    continue;
                }
                else
                {
                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_sector_1_not_continuing,
                                                               errno,
                                                               doneSectors));

                    return errno;
                }
            }

            if(!result)
            {
                StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_sector_1_not_continuing,
                                                           outputImage.ErrorMessage,
                                                           doneSectors));

                return ErrorNumber.WriteError;
            }

            doneSectors++;
        }

        return ErrorNumber.NoError;
    }

    ErrorNumber CopySectorsTagSecondary(bool        useLong, IMediaImage secondaryImage, IWritableImage outputImage,
                                        List<ulong> sectorsToCopy)
    {
        if(_aborted) return ErrorNumber.NoError;

        foreach(SectorTagType tag in secondaryImage.Info.ReadableSectorTags.TakeWhile(_ => useLong))
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
            }


            var doneSectors        = 0;
            int sectorsToCopyCount = sectorsToCopy.Count(t => t < secondaryImage.Info.Sectors);

            InitProgress?.Invoke();

            foreach(ulong sectorAddress in sectorsToCopy.Where(t => t < secondaryImage.Info.Sectors))
            {
                UpdateProgress?.Invoke(string.Format(UI.Copying_tag_0_for_sector_1, tag, sectorAddress),
                                       doneSectors,
                                       sectorsToCopyCount);

                bool result;

                ErrorNumber errno = secondaryImage.ReadSectorTag(sectorAddress, false, tag, out byte[] sector);

                if(errno == ErrorNumber.NoError)
                    result = outputImage.WriteSectorTag(sector, sectorAddress, false, tag);
                else if(ignoreSectorNotFound && IsSkippableNotFound(errno))
                {
                    ErrorMessage?.Invoke(string.Format(UI.Skipping_tag_0_for_sector_1_not_found, tag, sectorAddress));
                    doneSectors++;

                    continue;
                }
                else
                {
                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_sector_1_not_continuing,
                                                               errno,
                                                               sectorAddress));

                    return errno;
                }

                if(!result)
                {
                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_sector_1_not_continuing,
                                                               outputImage.ErrorMessage,
                                                               sectorAddress));

                    return ErrorNumber.WriteError;
                }

                doneSectors++;
            }

            EndProgress?.Invoke();
        }

        return ErrorNumber.NoError;
    }
}