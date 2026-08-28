using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core.Media;
using Aaru.Devices;
using Aaru.Localization;
using MediaType = Aaru.CommonTypes.MediaType;

namespace Aaru.Core.Image;

public sealed partial class Merger
{
    ErrorNumber CopyOptical(IOpticalMediaImage    primaryOptical, IOpticalMediaImage secondaryOptical,
                            IWritableOpticalImage outputOptical, bool useLong, List<ulong> sectorsToCopyFromSecondImage)
    {
        if(!outputOptical.SetTracks(primaryOptical.Tracks))
        {
            StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_sending_tracks_list_to_output_image,
                                                       outputOptical.ErrorMessage));

            return ErrorNumber.WriteError;
        }

        if(decrypt) UpdateStatus?.Invoke(UI.Decrypting_encrypted_sectors);

        // Convert all sectors track by track
        ErrorNumber errno = CopyOpticalSectorsPrimary(primaryOptical, outputOptical, useLong);

        if(errno != ErrorNumber.NoError) return errno;

        Dictionary<byte, string> isrcs                     = [];
        Dictionary<byte, byte>   trackFlags                = [];
        string                   mcn                       = null;
        HashSet<int>             subchannelExtents         = [];
        Dictionary<byte, int>    smallestPregapLbaPerTrack = [];
        var                      tracks                    = new Track[primaryOptical.Tracks.Count];

        for(var i = 0; i < tracks.Length; i++)
        {
            tracks[i] = new Track
            {
                Indexes           = [],
                Description       = primaryOptical.Tracks[i].Description,
                EndSector         = primaryOptical.Tracks[i].EndSector,
                StartSector       = primaryOptical.Tracks[i].StartSector,
                Pregap            = primaryOptical.Tracks[i].Pregap,
                Sequence          = primaryOptical.Tracks[i].Sequence,
                Session           = primaryOptical.Tracks[i].Session,
                BytesPerSector    = primaryOptical.Tracks[i].BytesPerSector,
                RawBytesPerSector = primaryOptical.Tracks[i].RawBytesPerSector,
                Type              = primaryOptical.Tracks[i].Type,
                SubchannelType    = primaryOptical.Tracks[i].SubchannelType
            };

            foreach(KeyValuePair<ushort, int> idx in primaryOptical.Tracks[i].Indexes)
                tracks[i].Indexes[idx.Key] = idx.Value;
        }

        // Gets tracks ISRCs
        foreach(SectorTagType tag in primaryOptical.Info.ReadableSectorTags
                                                   .Where(static t => t == SectorTagType.CdTrackIsrc)
                                                   .Order())
        {
            foreach(Track track in tracks)
            {
                errno = primaryOptical.ReadSectorTag(track.Sequence, false, tag, out byte[] isrc);

                if(errno != ErrorNumber.NoError) continue;

                isrcs[(byte)track.Sequence] = Encoding.UTF8.GetString(isrc);
            }
        }

        // Gets tracks flags
        foreach(SectorTagType tag in primaryOptical.Info.ReadableSectorTags
                                                   .Where(static t => t == SectorTagType.CdTrackFlags)
                                                   .Order())
        {
            foreach(Track track in tracks)
            {
                errno = primaryOptical.ReadSectorTag(track.Sequence, false, tag, out byte[] flags);

                if(errno != ErrorNumber.NoError) continue;

                trackFlags[(byte)track.Sequence] = flags[0];
            }
        }

        // Gets subchannel extents
        for(ulong s = 0; s < primaryOptical.Info.Sectors; s++)
        {
            if(s > int.MaxValue) break;

            subchannelExtents.Add((int)s);
        }

        errno = CopyOpticalSectorsTagsPrimary(primaryOptical,
                                              outputOptical,
                                              useLong,
                                              isrcs,
                                              ref mcn,
                                              tracks,
                                              subchannelExtents,
                                              smallestPregapLbaPerTrack);

        if(errno != ErrorNumber.NoError) return errno;

        // Write ISRCs
        foreach(KeyValuePair<byte, string> isrc in isrcs)
        {
            outputOptical.WriteSectorTag(Encoding.UTF8.GetBytes(isrc.Value),
                                         isrc.Key,
                                         false,
                                         SectorTagType.CdTrackIsrc);
        }

        // Write track flags
        if(trackFlags.Count > 0)
        {
            foreach((byte track, byte flags) in trackFlags)
                outputOptical.WriteSectorTag([flags], track, false, SectorTagType.CdTrackFlags);
        }

        // Write MCN
        if(mcn != null) outputOptical.WriteMediaTag(Encoding.UTF8.GetBytes(mcn), MediaTagType.CD_MCN);

        UpdateStatus?.Invoke(string.Format(UI.Will_copy_0_sectors_from_secondary_image,
                                           sectorsToCopyFromSecondImage.Count));

        // Copy sectors from secondary image
        errno = CopyOpticalSectorsSecondary(secondaryOptical, outputOptical, useLong, sectorsToCopyFromSecondImage);

        if(errno != ErrorNumber.NoError) return errno;

        errno = CopyOpticalSectorsTagsSecondary(secondaryOptical,
                                                outputOptical,
                                                useLong,
                                                isrcs,
                                                ref mcn,
                                                tracks,
                                                subchannelExtents,
                                                smallestPregapLbaPerTrack,
                                                sectorsToCopyFromSecondImage);

        if(errno != ErrorNumber.NoError) return errno;


        if(!IsCompactDiscMedia(primaryOptical.Info.MediaType) || !generateSubchannels) return ErrorNumber.NoError;

        // Generate subchannel data
        CompactDisc.GenerateSubchannels(subchannelExtents,
                                        tracks,
                                        trackFlags,
                                        primaryOptical.Info.Sectors,
                                        null,
                                        InitProgress,
                                        UpdateProgress,
                                        EndProgress,
                                        outputOptical);

        return ErrorNumber.NoError;
    }

    ErrorNumber CopyOpticalSectorsPrimary(IOpticalMediaImage inputOptical, IWritableOpticalImage outputOptical,
                                          bool               useLong)
    {
        if(_aborted) return ErrorNumber.NoError;

        InitProgress?.Invoke();
        InitProgress2?.Invoke();
        var currentTrack = 0;

        _aacsDecryptedCpsUnitKeys = null;

        if(decrypt)
        {
            if(IsHdDvdAacsMedia(inputOptical.Info.MediaType))
            {
                _aacsDecryptedCpsUnitKeys =
                    AacsKeyResolver.TryGetDecryptedCpsUnitKeysHddvd(inputOptical,
                                                                    _plugins,
                                                                    Encoding.UTF8,
                                                                    out string hddvdErr);

                if(_aacsDecryptedCpsUnitKeys == null)
                {
                    StoppingErrorMessage?.Invoke(hddvdErr ?? Localization.Core.Aacs_hddvd_missing_unit_keys);

                    return ErrorNumber.NoData;
                }

                return CopyAacsHddvdOpticalSectorsPrimary(inputOptical, outputOptical);
            }

            if(IsBluRayAacsMedia(inputOptical.Info.MediaType))
            {
                _aacsDecryptedCpsUnitKeys = AacsKeyResolver.TryGetDecryptedCpsUnitKeys(inputOptical,
                    _plugins,
                    Encoding.UTF8,
                    out string aacsErr);

                if(_aacsDecryptedCpsUnitKeys == null)
                {
                    StoppingErrorMessage?.Invoke(aacsErr ?? Localization.Core.Aacs_missing_unit_keys);

                    return ErrorNumber.NoData;
                }

                return CopyAacsBdOpticalSectorsPrimary(inputOptical, outputOptical);
            }

            if(CssDvdSectorDecrypt.IsDvdMedia(inputOptical.Info.MediaType))
                return CopyCssDvdOpticalSectorsPrimary(inputOptical, outputOptical, useLong);
        }

        foreach(Track track in inputOptical.Tracks)
        {
            if(_aborted) break;

            UpdateProgress?.Invoke(string.Format(UI.Copying_sectors_in_track_0_of_1,
                                                 currentTrack + 1,
                                                 inputOptical.Tracks.Count),
                                   currentTrack,
                                   inputOptical.Tracks.Count);

            ulong doneSectors  = 0;
            ulong trackSectors = track.EndSector - track.StartSector + 1;

            while(doneSectors < trackSectors)
            {
                if(_aborted) break;

                byte[] sector;

                uint sectorsToDo;

                if(trackSectors - doneSectors >= (ulong)count)
                    sectorsToDo = (uint)count;
                else
                    sectorsToDo = (uint)(trackSectors - doneSectors);

                UpdateProgress2?.Invoke(string.Format(UI.Copying_sectors_0_to_1_in_track_2,
                                                      doneSectors + track.StartSector,
                                                      doneSectors + sectorsToDo + track.StartSector,
                                                      track.Sequence),
                                        (long)doneSectors,
                                        (long)trackSectors);

                bool         result;
                SectorStatus sectorStatus      = SectorStatus.NotDumped;
                var          sectorStatusArray = new SectorStatus[1];
                ErrorNumber  errno;

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
                    else if(ignoreSectorNotFound && IsSkippableNotFound(errno))
                    {
                        ErrorMessage?.Invoke(string.Format(UI.Skipping_sector_0_not_found,
                                                           doneSectors + track.StartSector));

                        doneSectors += sectorsToDo;

                        continue;
                    }
                    else
                    {
                        StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_sector_1_not_continuing,
                                                                   errno,
                                                                   doneSectors + track.StartSector));

                        return ErrorNumber.WriteError;
                    }

                    if(!result && sector.Length % 2352 != 0)
                    {
                        StoppingErrorMessage?.Invoke(UI.Input_image_is_not_returning_long_sectors_not_continuing);

                        return ErrorNumber.InOutError;
                    }
                }
                else
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
                    else if(ignoreSectorNotFound && IsSkippableNotFound(errno))
                    {
                        ErrorMessage?.Invoke(string.Format(UI.Skipping_sector_0_not_found,
                                                           doneSectors + track.StartSector));

                        doneSectors += sectorsToDo;

                        continue;
                    }
                    else
                    {
                        StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_sector_1_not_continuing,
                                                                   errno,
                                                                   doneSectors + track.StartSector));

                        return ErrorNumber.WriteError;
                    }
                }

                if(!result)
                {
                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_sector_1_not_continuing,
                                                               outputOptical.ErrorMessage,
                                                               doneSectors + track.StartSector));

                    return ErrorNumber.WriteError;
                }

                doneSectors += sectorsToDo;
            }

            currentTrack++;
        }

        EndProgress2?.Invoke();
        EndProgress?.Invoke();

        return ErrorNumber.NoError;
    }

    ErrorNumber CopyOpticalSectorsSecondary(IOpticalMediaImage inputOptical, IWritableOpticalImage outputOptical,
                                            bool               useLong,      List<ulong>           sectorsToCopy)
    {
        if(_aborted) return ErrorNumber.NoError;

        if(decrypt && CssDvdSectorDecrypt.IsDvdMedia(inputOptical.Info.MediaType))
            return CopyCssDvdOpticalSectorsSecondary(inputOptical, outputOptical, useLong, sectorsToCopy);

        InitProgress?.Invoke();
        int howManySectorsToCopy = sectorsToCopy.Count(t => t < inputOptical.Info.Sectors);
        var howManySectorsCopied = 0;

        foreach(ulong sectorAddress in sectorsToCopy.Where(t => t < inputOptical.Info.Sectors)
                                                    .TakeWhile(_ => !_aborted))
        {
            UpdateProgress?.Invoke(string.Format(UI.Copying_sector_0, sectorAddress),
                                   howManySectorsCopied,
                                   howManySectorsToCopy);

            if(_aborted) break;

            byte[]       sector;
            bool         result;
            SectorStatus sectorStatus;
            ErrorNumber  errno;

            if(useLong)
            {
                errno = inputOptical.ReadSectorLong(sectorAddress, false, out sector, out sectorStatus);

                if(errno == ErrorNumber.NoError)
                    result = outputOptical.WriteSectorLong(sector, sectorAddress, false, sectorStatus);
                else if(ignoreSectorNotFound && IsSkippableNotFound(errno))
                {
                    ErrorMessage?.Invoke(string.Format(UI.Skipping_sector_0_not_found, sectorAddress));
                    howManySectorsCopied++;

                    continue;
                }
                else
                {
                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_sector_1_not_continuing,
                                                               errno,
                                                               sectorAddress));

                    return ErrorNumber.WriteError;
                }

                if(!result && sector.Length % 2352 != 0)
                {
                    StoppingErrorMessage?.Invoke(UI.Input_image_is_not_returning_long_sectors_not_continuing);

                    return ErrorNumber.InOutError;
                }
            }

            else
            {
                errno = inputOptical.ReadSector(sectorAddress, false, out sector, out sectorStatus);

                if(errno == ErrorNumber.NoError)
                    result = outputOptical.WriteSector(sector, sectorAddress, false, sectorStatus);
                else if(ignoreSectorNotFound && IsSkippableNotFound(errno))
                {
                    ErrorMessage?.Invoke(string.Format(UI.Skipping_sector_0_not_found, sectorAddress));
                    howManySectorsCopied++;

                    continue;
                }
                else
                {
                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_sector_1_not_continuing,
                                                               errno,
                                                               sectorAddress));

                    return ErrorNumber.WriteError;
                }
            }

            if(!result)
            {
                StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_sector_1_not_continuing,
                                                           outputOptical.ErrorMessage,
                                                           sectorAddress));

                return ErrorNumber.WriteError;
            }

            howManySectorsCopied++;
        }

        EndProgress?.Invoke();

        return ErrorNumber.NoError;
    }

    ErrorNumber CopyOpticalSectorsTagsPrimary(IOpticalMediaImage inputOptical, IWritableOpticalImage outputOptical,
                                              bool useLong, Dictionary<byte, string> isrcs, ref string mcn,
                                              Track[] tracks, HashSet<int> subchannelExtents,
                                              Dictionary<byte, int> smallestPregapLbaPerTrack)
    {
        foreach(SectorTagType tag in inputOptical.Info.ReadableSectorTags.Order()
                                                 .TakeWhile(_ => useLong)
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
                    // This tags are inline in long sector
                    continue;
            }

            InitProgress?.Invoke();
            InitProgress2?.Invoke();
            var currentTrack = 0;

            foreach(Track track in inputOptical.Tracks)
            {
                UpdateProgress?.Invoke(string.Format(UI.Copying_tags_in_track_0_of_1,
                                                     currentTrack + 1,
                                                     inputOptical.Tracks.Count),
                                       currentTrack,
                                       inputOptical.Tracks.Count);

                ulong  doneSectors  = 0;
                ulong  trackSectors = track.EndSector - track.StartSector + 1;
                byte[] sector;
                bool   result;

                ErrorNumber errno;

                switch(tag)
                {
                    case SectorTagType.CdTrackFlags:
                    case SectorTagType.CdTrackIsrc:
                        errno = inputOptical.ReadSectorTag(track.Sequence, false, tag, out sector);

                        switch(errno)
                        {
                            case ErrorNumber.NoData:

                                continue;
                            case ErrorNumber.NoError:
                                result = outputOptical.WriteSectorTag(sector, track.Sequence, false, tag);

                                break;
                            default:
                            {
                                StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_tag_not_continuing,
                                                                           outputOptical.ErrorMessage));

                                return ErrorNumber.WriteError;
                            }
                        }

                        if(!result)
                        {
                            StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_tag_not_continuing,
                                                                       outputOptical.ErrorMessage));

                            return ErrorNumber.WriteError;
                        }

                        continue;
                }

                while(doneSectors < trackSectors)
                {
                    if(_aborted) break;

                    uint sectorsToDo;

                    if(trackSectors - doneSectors >= (ulong)count)
                        sectorsToDo = (uint)count;
                    else
                        sectorsToDo = (uint)(trackSectors - doneSectors);

                    UpdateProgress2?.Invoke(string.Format(UI.Copying_tag_3_for_sectors_0_to_1_in_track_2,
                                                          doneSectors + track.StartSector,
                                                          doneSectors + sectorsToDo + track.StartSector,
                                                          track.Sequence,
                                                          tag),
                                            (long)(doneSectors + track.StartSector),
                                            (long)(doneSectors + sectorsToDo + track.StartSector));

                    if(sectorsToDo == 1)
                    {
                        errno = inputOptical.ReadSectorTag(doneSectors + track.StartSector, false, tag, out sector);

                        if(errno == ErrorNumber.NoError)
                        {
                            if(tag == SectorTagType.CdSectorSubchannel)
                            {
                                bool indexesChanged = CompactDisc.WriteSubchannelToImage(MmcSubchannel.Raw,
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
                        else if(IsSkippableNotFound(errno) && (ignoreSectorNotFound || IsOptionalTag(tag)))
                        {
                            if(!IsOptionalTag(tag))
                            {
                                ErrorMessage?.Invoke(string.Format(UI.Skipping_tag_0_for_sector_1_not_found,
                                                                   tag,
                                                                   doneSectors + track.StartSector));
                            }

                            doneSectors += sectorsToDo;

                            continue;
                        }
                        else
                        {
                            StoppingErrorMessage
                              ?.Invoke(string.Format(UI.Error_0_reading_tag_for_sector_1_not_continuing,
                                                     errno,
                                                     doneSectors + track.StartSector));

                            return errno;
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
                                bool indexesChanged = CompactDisc.WriteSubchannelToImage(MmcSubchannel.Raw,
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
                        else if(IsSkippableNotFound(errno) && (ignoreSectorNotFound || IsOptionalTag(tag)))
                        {
                            if(!IsOptionalTag(tag))
                            {
                                ErrorMessage?.Invoke(string.Format(UI.Skipping_tag_0_for_sector_1_not_found,
                                                                   tag,
                                                                   doneSectors + track.StartSector));
                            }

                            doneSectors += sectorsToDo;

                            continue;
                        }
                        else
                        {
                            StoppingErrorMessage
                              ?.Invoke(string.Format(UI.Error_0_reading_tag_for_sector_1_not_continuing,
                                                     errno,
                                                     doneSectors + track.StartSector));

                            return errno;
                        }
                    }

                    if(!result)
                    {
                        StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_tag_for_sector_1_not_continuing,
                                                                   outputOptical.ErrorMessage,
                                                                   doneSectors + track.StartSector));

                        return ErrorNumber.WriteError;
                    }

                    doneSectors += sectorsToDo;
                }

                currentTrack++;
            }

            EndProgress?.Invoke();
            EndProgress2?.Invoke();
        }

        return ErrorNumber.NoError;
    }

    ErrorNumber CopyOpticalSectorsTagsSecondary(IOpticalMediaImage inputOptical, IWritableOpticalImage outputOptical,
                                                bool useLong, Dictionary<byte, string> isrcs, ref string mcn,
                                                Track[] tracks, HashSet<int> subchannelExtents,
                                                Dictionary<byte, int> smallestPregapLbaPerTrack,
                                                List<ulong> sectorsToCopy)
    {
        foreach(SectorTagType tag in inputOptical.Info.ReadableSectorTags.Order()
                                                 .TakeWhile(_ => useLong)
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
                    // This tags are inline in long sector
                    continue;
            }

            InitProgress?.Invoke();


            int numberOfSectorsToCopy = sectorsToCopy.Count(t => t < inputOptical.Info.Sectors);
            var currentSectorIndex    = 0;

            foreach(ulong sectorAddress in sectorsToCopy.Where(t => t < inputOptical.Info.Sectors))
            {
                if(_aborted) break;

                Track track = inputOptical.Tracks.FirstOrDefault(t => t.StartSector <= sectorAddress &&
                                                                      t.EndSector   >= sectorAddress);

                UpdateProgress?.Invoke(string.Format(UI.Copying_tag_0_for_sector_1, tag, sectorAddress),
                                       currentSectorIndex,
                                       numberOfSectorsToCopy);

                ErrorNumber errno = inputOptical.ReadSectorTag(sectorAddress, false, tag, out byte[] sector);

                bool result;

                if(errno == ErrorNumber.NoError)
                {
                    if(tag == SectorTagType.CdSectorSubchannel)
                    {
                        bool indexesChanged = CompactDisc.WriteSubchannelToImage(MmcSubchannel.Raw,
                            MmcSubchannel.Raw,
                            sector,
                            sectorAddress,
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
                        result = outputOptical.WriteSectorTag(sector, sectorAddress, false, tag);
                }
                else if(IsSkippableNotFound(errno) && (ignoreSectorNotFound || IsOptionalTag(tag)))
                {
                    if(!IsOptionalTag(tag))
                    {
                        ErrorMessage?.Invoke(string.Format(UI.Skipping_tag_0_for_sector_1_not_found,
                                                           tag,
                                                           sectorAddress));
                    }

                    currentSectorIndex++;

                    continue;
                }
                else
                {
                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_tag_for_sector_1_not_continuing,
                                                               errno,
                                                               sectorAddress));

                    return errno;
                }

                if(!result)
                {
                    StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_tag_for_sector_1_not_continuing,
                                                               outputOptical.ErrorMessage,
                                                               sectorAddress));

                    return ErrorNumber.WriteError;
                }

                currentSectorIndex++;
            }

            EndProgress?.Invoke();
        }

        return ErrorNumber.NoError;
    }

    static bool IsBluRayAacsMedia(MediaType mediaType) => mediaType is MediaType.BDROM
                                                                    or MediaType.BDR
                                                                    or MediaType.BDRE
                                                                    or MediaType.BDRXL
                                                                    or MediaType.BDREXL
                                                                    or MediaType.UHDBD;

    static bool IsHdDvdAacsMedia(MediaType mediaType) => mediaType is MediaType.HDDVDROM
                                                                   or MediaType.HDDVDRAM
                                                                   or MediaType.HDDVDR
                                                                   or MediaType.HDDVDRW
                                                                   or MediaType.HDDVDRDL
                                                                   or MediaType.HDDVDRWDL;

    private static bool IsCompactDiscMedia(MediaType mediaType) =>

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
}