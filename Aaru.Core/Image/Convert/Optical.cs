using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core.Media;
using Aaru.Decryption.DVD;
using Aaru.Devices;
using Aaru.Localization;
using Aaru.Logging;

namespace Aaru.Core.Image;

public partial class Convert
{
    ErrorNumber ConvertOptical(IOpticalMediaImage inputOptical, IWritableOpticalImage outputOptical, bool useLong)
    {
        if(!outputOptical.SetTracks(inputOptical.Tracks))
        {
            StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_sending_tracks_list_to_output_image,
                                                       outputOptical.ErrorMessage));

            return ErrorNumber.WriteError;
        }

        if(_decrypt) UpdateStatus?.Invoke(UI.Decrypting_encrypted_sectors);

        // Convert all sectors track by track
        ErrorNumber errno = ConvertOpticalSectors(inputOptical, outputOptical, useLong);

        if(errno != ErrorNumber.NoError) return errno;

        Dictionary<byte, string> isrcs                     = [];
        Dictionary<byte, byte>   trackFlags                = [];
        string                   mcn                       = null;
        HashSet<int>             subchannelExtents         = [];
        Dictionary<byte, int>    smallestPregapLbaPerTrack = [];
        var                      tracks                    = new Track[inputOptical.Tracks.Count];

        for(var i = 0; i < tracks.Length; i++)
        {
            tracks[i] = new Track
            {
                Indexes           = [],
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

        // Gets tracks ISRCs
        foreach(SectorTagType tag in inputOptical.Info.ReadableSectorTags
                                                 .Where(static t => t == SectorTagType.CdTrackIsrc)
                                                 .Order())
        {
            foreach(Track track in tracks)
            {
                errno = inputOptical.ReadSectorTag(track.Sequence, false, tag, out byte[] isrc);

                if(errno != ErrorNumber.NoError) continue;

                isrcs[(byte)track.Sequence] = Encoding.UTF8.GetString(isrc);
            }
        }

        // Gets tracks flags
        foreach(SectorTagType tag in inputOptical.Info.ReadableSectorTags
                                                 .Where(static t => t == SectorTagType.CdTrackFlags)
                                                 .Order())
        {
            foreach(Track track in tracks)
            {
                errno = inputOptical.ReadSectorTag(track.Sequence, false, tag, out byte[] flags);

                if(errno != ErrorNumber.NoError) continue;

                trackFlags[(byte)track.Sequence] = flags[0];
            }
        }

        // Gets subchannel extents
        for(ulong s = 0; s < inputOptical.Info.Sectors; s++)
        {
            if(s > int.MaxValue) break;

            subchannelExtents.Add((int)s);
        }

        errno = ConvertOpticalSectorsTags(inputOptical,
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

        var     errorNumber = inputOptical.ReadDPM(out uint dpmStartSector, out uint dpmResolution, out uint numberOfDpmEntries, out ulong[] dpm);

        if(errorNumber == ErrorNumber.NoError)
        {
            outputOptical.HeldDpmStartSector     = dpmStartSector;
            outputOptical.HeldDpmResolution      = dpmResolution;
            outputOptical.HeldNumberOfDpmEntries = numberOfDpmEntries;
            outputOptical.HeldDpm                = dpm;
        }


        if(!IsCompactDiscMedia(inputOptical.Info.MediaType) || !_generateSubchannels) return ErrorNumber.NoError;

        // Generate subchannel data
        CompactDisc.GenerateSubchannels(subchannelExtents,
                                        tracks,
                                        trackFlags,
                                        inputOptical.Info.Sectors,
                                        null,
                                        InitProgress,
                                        UpdateProgress,
                                        EndProgress,
                                        outputOptical);

        return ErrorNumber.NoError;
    }

    ErrorNumber ConvertOpticalSectors(IOpticalMediaImage inputOptical, IWritableOpticalImage outputOptical,
                                      bool               useLong)
    {
        if(_aborted) return ErrorNumber.NoError;
        InitProgress?.Invoke();
        InitProgress2?.Invoke();
        byte[] generatedTitleKeys = null;
        var    currentTrack       = 0;

        foreach(Track track in inputOptical.Tracks)
        {
            if(_aborted) break;

            UpdateProgress?.Invoke(string.Format(UI.Converting_sectors_in_track_0_of_1,
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

                if(trackSectors - doneSectors >= _count)
                    sectorsToDo = _count;
                else
                    sectorsToDo = (uint)(trackSectors - doneSectors);

                UpdateProgress2?.Invoke(string.Format(UI.Converting_sectors_0_to_1_in_track_2,
                                                      doneSectors + track.StartSector,
                                                      doneSectors + sectorsToDo + track.StartSector,
                                                      track.Sequence),
                                        (long)doneSectors,
                                        (long)trackSectors);

                var          useNotLong        = false;
                var          result            = false;
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
                    else
                    {
                        result = true;

                        if(_force)
                        {
                            ErrorMessage?.Invoke(string.Format(UI.Error_0_reading_sector_1_continuing,
                                                               errno,
                                                               doneSectors + track.StartSector));
                        }
                        else
                        {
                            StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_sector_1_not_continuing,
                                                                       errno,
                                                                       doneSectors + track.StartSector));

                            return ErrorNumber.WriteError;
                        }
                    }

                    if(!result && sector.Length % 2352 != 0)
                    {
                        if(!_force)
                        {
                            StoppingErrorMessage
                              ?.Invoke(UI.Input_image_is_not_returning_raw_sectors_use_force_if_you_want_to_continue);

                            return ErrorNumber.InOutError;
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
                    if(IsDvdMedia(inputOptical.Info.MediaType) && _decrypt)
                    {
                        DecryptDvdSector(ref sector,
                                         inputOptical,
                                         doneSectors + track.StartSector,
                                         sectorsToDo,
                                         _plugins,
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

                        if(_force)
                        {
                            ErrorMessage?.Invoke(string.Format(UI.Error_0_reading_sector_1_continuing,
                                                               errno,
                                                               doneSectors + track.StartSector));
                        }
                        else
                        {
                            StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_reading_sector_1_not_continuing,
                                                                       errno,
                                                                       doneSectors + track.StartSector));

                            return ErrorNumber.WriteError;
                        }
                    }
                }

                if(!result)
                {
                    if(_force)
                    {
                        ErrorMessage?.Invoke(string.Format(UI.Error_0_writing_sector_1_continuing,
                                                           outputOptical.ErrorMessage,
                                                           doneSectors + track.StartSector));
                    }
                    else
                    {
                        StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_sector_1_not_continuing,
                                                                   outputOptical.ErrorMessage,
                                                                   doneSectors + track.StartSector));

                        return ErrorNumber.WriteError;
                    }
                }

                doneSectors += sectorsToDo;
            }

            currentTrack++;
        }

        EndProgress2?.Invoke();
        EndProgress?.Invoke();

        return ErrorNumber.NoError;
    }

    ErrorNumber ConvertOpticalSectorsTags(IOpticalMediaImage inputOptical, IWritableOpticalImage outputOptical,
                                          bool useLong, Dictionary<byte, string> isrcs, ref string mcn, Track[] tracks,
                                          HashSet<int> subchannelExtents,
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
                    // This tags are inline in long sector
                    continue;
            }

            if(_force && !outputOptical.SupportedSectorTags.Contains(tag)) continue;

            ErrorNumber errno = ErrorNumber.NoError;

            InitProgress?.Invoke();
            InitProgress2?.Invoke();
            var currentTrack = 0;

            foreach(Track track in inputOptical.Tracks)
            {
                UpdateProgress?.Invoke(string.Format(UI.Converting_tags_in_track_0_of_1,
                                                     currentTrack + 1,
                                                     inputOptical.Tracks.Count),
                                       currentTrack,
                                       inputOptical.Tracks.Count);

                ulong  doneSectors  = 0;
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
                                result = outputOptical.WriteSectorTag(sector, track.Sequence, false, tag);

                                break;
                            default:
                            {
                                if(_force)
                                {
                                    ErrorMessage?.Invoke(string.Format(UI.Error_0_writing_tag_continuing,
                                                                       outputOptical.ErrorMessage));

                                    continue;
                                }

                                StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_tag_not_continuing,
                                                                           outputOptical.ErrorMessage));

                                return ErrorNumber.WriteError;
                            }
                        }

                        if(!result)
                        {
                            if(_force)
                            {
                                ErrorMessage?.Invoke(string.Format(UI.Error_0_writing_tag_continuing,
                                                                   outputOptical.ErrorMessage));
                            }
                            else
                            {
                                StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_tag_not_continuing,
                                                                           outputOptical.ErrorMessage));

                                return ErrorNumber.WriteError;
                            }
                        }

                        continue;
                }

                while(doneSectors < trackSectors)
                {
                    if(_aborted) break;
                    uint sectorsToDo;

                    if(trackSectors - doneSectors >= _count)
                        sectorsToDo = _count;
                    else
                        sectorsToDo = (uint)(trackSectors - doneSectors);

                    UpdateProgress2?.Invoke(string.Format(UI.Converting_tag_3_for_sectors_0_to_1_in_track_2,
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
                                    _fixSubchannelPosition,
                                    outputOptical,
                                    _fixSubchannel,
                                    _fixSubchannelCrc,
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

                            if(_force)
                            {
                                ErrorMessage?.Invoke(string.Format(UI.Error_0_reading_tag_for_sector_1_continuing,
                                                                   errno,
                                                                   doneSectors + track.StartSector));
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
                                    _fixSubchannelPosition,
                                    outputOptical,
                                    _fixSubchannel,
                                    _fixSubchannelCrc,
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

                            if(_force)
                            {
                                ErrorMessage?.Invoke(string.Format(UI.Error_0_reading_tag_for_sector_1_continuing,
                                                                   errno,
                                                                   doneSectors + track.StartSector));
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
                    }

                    if(!result)
                    {
                        if(_force)
                        {
                            ErrorMessage?.Invoke(string.Format(UI.Error_0_writing_tag_for_sector_1_continuing,
                                                               outputOptical.ErrorMessage,
                                                               doneSectors + track.StartSector));
                        }
                        else
                        {
                            StoppingErrorMessage
                              ?.Invoke(string.Format(UI.Error_0_writing_tag_for_sector_1_not_continuing,
                                                     outputOptical.ErrorMessage,
                                                     doneSectors + track.StartSector));

                            return ErrorNumber.WriteError;
                        }
                    }

                    doneSectors += sectorsToDo;
                }

                currentTrack++;
            }

            EndProgress?.Invoke();
            EndProgress2?.Invoke();

            if(errno != ErrorNumber.NoError && !_force) return errno;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Decrypts DVD sectors using CSS (Content Scramble System) decryption
    ///     Retrieves decryption keys from sector tags or generates them from ISO9660 filesystem
    ///     Only MPEG packets within sectors can be encrypted
    /// </summary>
    void DecryptDvdSector(ref byte[]     sector, IOpticalMediaImage inputOptical, ulong sectorAddress, uint sectorsToDo,
                          PluginRegister plugins, ref byte[] generatedTitleKeys)
    {
        if(_aborted) return;

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

    /// <summary>
    ///     Generates DVD CSS title keys from ISO9660 filesystem
    ///     Used when explicit title keys are not available in sector tags
    ///     Searches for ISO9660 partitions to derive decryption keys
    /// </summary>
    void GenerateDvdTitleKeys(IOpticalMediaImage inputOptical, PluginRegister plugins, ref byte[] generatedTitleKeys)
    {
        if(_aborted) return;

        List<Partition> partitions = Partitions.GetAll(inputOptical);

        partitions = partitions.FindAll(p =>
        {
            Filesystems.Identify(inputOptical, out List<string> idPlugins, p);

            return idPlugins.Contains("iso9660 filesystem");
        });

        if(!plugins.ReadOnlyFilesystems.TryGetValue("iso9660 filesystem", out IReadOnlyFilesystem rofs)) return;

        AaruLogging.Debug(MODULE_NAME, UI.Generating_decryption_keys);

        generatedTitleKeys = CSS.GenerateTitleKeys(inputOptical, partitions, inputOptical.Info.Sectors, rofs);
    }

    bool IsDvdMedia(MediaType mediaType) =>

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
}