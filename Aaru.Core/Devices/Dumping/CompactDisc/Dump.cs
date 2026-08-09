// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dump.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CompactDisc dumping.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps CompactDiscs.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core.Graphics;
using Aaru.Core.Logging;
using Aaru.Core.Media.Detection;
using Aaru.Database.Models;
using Aaru.Decoders.CD;
using Aaru.Devices;
using Aaru.Images;
using Aaru.Localization;
using Aaru.Logging;
using Humanizer;
using Spectre.Console;
using Track = Aaru.CommonTypes.Structs.Track;
using TrackType = Aaru.CommonTypes.Enums.TrackType;
using Version = Aaru.CommonTypes.Interop.Version;

namespace Aaru.Core.Devices.Dumping;

/// <summary>Implement dumping Compact Discs</summary>

// TODO: Barcode
sealed partial class Dump
{
    /// <summary>Dumps a compact disc</summary>
    void CompactDisc()
    {
        ExtentsULong                     audioExtents;                        // Extents with audio sectors
        ulong                            blocks;                              // Total number of positive sectors
        uint                             blockSize;                           // Size of the read sector in bytes
        bool                             bcdSubchannel;                       // Subchannel positioning is in BCD
        CdOffset                         cdOffset;                            // Read offset from database
        byte[]                           cmdBuf;                              // Data buffer
        DumpHardware                     currentTry   = null;                 // Current dump hardware try
        double                           currentSpeed = 0;                    // Current read speed
        int?                             discOffset   = null;                 // Disc write offset
        MmcSubchannel                    desiredSubchannel;                   // User requested subchannel
        ExtentsULong                     extents = null;                      // Extents
        bool                             hiddenData;                          // Hidden track is data
        IbgLog                           ibgLog;                              // IMGBurn log
        double                           imageWriteDuration = 0;              // Duration of image write
        long                             lastSector;                          // Last sector number
        var                              leadOutExtents = new ExtentsULong(); // Lead-out extents
        Dictionary<int, long>            leadOutStarts  = [];                 // Lead-out starts
        double                           maxSpeed       = double.MinValue;    // Maximum speed
        MhddLog                          mhddLog;                             // MHDD log
        double                           minSpeed = double.MaxValue;          // Minimum speed
        bool                             newTrim;                             // Is trim a new one?
        var                              offsetBytes = 0;                     // Read offset
        bool                             ret;                                 // Image writing return status
        bool                             read6;                               // Device supports READ(6)
        bool                             read10;                              // Device supports READ(10)
        bool                             read12;                              // Device supports READ(12)
        bool                             read16;                              // Device supports READ(16)
        bool                             readcd;                              // Device supports READ CD
        var                              sectorsForOffset = 0;                // Sectors needed to fix offset
        const uint                       sectorSize       = 2352;             // Full sector size
        uint                             subSize;                             // Subchannel size in bytes
        int                              sessions;                            // Number of sessions in disc
        SubchannelLog                    subLog              = null;          // Subchannel log
        var                              supportsLongSectors = true;          // Supports reading EDC and ECC
        FullTOC.CDFullTOC?               toc;                                 // Full CD TOC
        double                           totalDuration = 0;                   // Total commands duration
        Dictionary<byte, byte>           trackFlags    = [];                  // Track flags
        Track[]                          tracks;                              // Tracks in disc
        var                              sense = true;                        // Sense indicator
        TrackSubchannelType              subType;                             // Track subchannel type
        bool                             supportsPqSubchannel;                // Supports reading PQ subchannel
        bool                             supportsRwSubchannel;                // Supports reading RW subchannel
        MmcSubchannel                    supportedSubchannel;                 // Drive's maximum supported subchannel
        int                              firstTrackLastSession;               // Number of first track in last session
        bool                             hiddenTrack;                         // Disc has a hidden track before track 1
        bool                             shallReturn;                         // Shall return due to errors
        Dictionary<byte, string>         isrcs               = [];
        string                           mcn                 = null;
        HashSet<int>                     subchannelExtents   = [];
        var                              cdiReadyReadAsAudio = false;
        uint                             firstLba;
        var                              outputOptical             = _outputPlugin as IWritableOpticalImage;
        Dictionary<MediaTagType, byte[]> mediaTags                 = []; // Media tags
        Dictionary<byte, int>            smallestPregapLbaPerTrack = [];
        MediaType                        dskType                   = MediaType.CD;

        if(_dumpRaw)
        {
            AaruLogging.WriteLine(Localization.Core.Raw_CD_dumping_not_yet_implemented);
            StoppingErrorMessage?.Invoke(Localization.Core.Raw_CD_dumping_not_yet_implemented);

            return;
        }

        tracks = GetCdTracks(_dev,
                             _force,
                             out lastSector,
                             leadOutStarts,
                             mediaTags,
                             StoppingErrorMessage,
                             out toc,
                             trackFlags,
                             UpdateStatus);

        if(tracks is null)
        {
            AaruLogging.WriteLine(Localization.Core.Could_not_get_tracks);
            StoppingErrorMessage?.Invoke(Localization.Core.Could_not_get_tracks);

            return;
        }

        firstLba = (uint)tracks.Min(static t => t.StartSector);

        shallReturn = CheckSubchannelSupport(firstLba,
                                             outputOptical,
                                             out subSize,
                                             out subType,
                                             out supportsPqSubchannel,
                                             out supportsRwSubchannel,
                                             out desiredSubchannel,
                                             out supportedSubchannel,
                                             out read6,
                                             out read10,
                                             out read12,
                                             out read16,
                                             out readcd,
                                             out bcdSubchannel);

        if(shallReturn) return;

        blockSize = sectorSize + subSize;

        // Probe, once, whether the drive returns C2 error pointers alongside subchannel and in which byte order, so
        // audio sectors can be checked for concealed (interpolated) samples. Diagnostic only: sets capability fields
        // and logs the outcome; leaves the dump path unchanged when C2 is unavailable.
        DetectC2Layout(firstLba, readcd, supportedSubchannel, tracks.Any(static t => t.Type == TrackType.Audio));

        foreach(Track trk in tracks) trk.SubchannelType = subType;

        UpdateStatus?.Invoke(Localization.Core.Calculating_pregaps__can_take_some_time);

        SolveTrackPregaps(_dev,
                          UpdateStatus,
                          tracks,
                          supportsPqSubchannel,
                          supportsRwSubchannel,
                          _dbDev,
                          out bool inexactPositioning,
                          true);

        if(inexactPositioning)
            UpdateStatus?.Invoke(Localization.Core.The_drive_has_returned_incorrect_Q_positioning_calculating_pregaps);

        if(!outputOptical.OpticalCapabilities.HasFlag(OpticalImageCapabilities.CanStoreRawData))
        {
            if(!_force)
            {
                StoppingErrorMessage?.Invoke(Localization.Core
                                                         .Output_format_does_not_support_storing_raw_data_not_continuing);

                return;
            }


            ErrorMessage?.Invoke(Localization.Core.Output_format_does_not_support_storing_raw_data_continuing);
        }

        if(!outputOptical.OpticalCapabilities.HasFlag(OpticalImageCapabilities.CanStoreAudioTracks) &&
           tracks.Any(static track => track.Type == TrackType.Audio))
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Output_format_does_not_support_audio_tracks_cannot_continue);

            return;
        }

        if(!outputOptical.OpticalCapabilities.HasFlag(OpticalImageCapabilities.CanStorePregaps) &&
           tracks.Any(track => track.Sequence != tracks.First(t => t.Session == track.Session).Sequence &&
                               track.Pregap   > 0))
        {
            if(!_force)
            {
                StoppingErrorMessage?.Invoke(Localization.Core.Output_format_does_not_support_pregaps_not_continuing);

                return;
            }


            ErrorMessage?.Invoke(Localization.Core.Output_format_does_not_support_pregaps_continuing);
        }

        for(var t = 1; t < tracks.Length; t++) tracks[t - 1].EndSector = tracks[t].StartSector - 1;

        tracks[^1].EndSector = (ulong)lastSector;
        blocks               = (ulong)(lastSector + 1);

        if(blocks == 0)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_dump_blank_media);

            return;
        }

        ResumeSupport.Process(true,
                              true,
                              blocks,
                              _dev.Manufacturer,
                              _dev.Model,
                              _dev.Serial,
                              _dev.PlatformId,
                              ref _resume,
                              ref currentTry,
                              ref extents,
                              _dev.FirmwareRevision,
                              _private,
                              _force);

        if(currentTry == null || extents == null)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Could_not_process_resume_file_not_continuing);

            return;
        }

        // The resume file is only available now (created by ResumeSupport.Process above). Seed the C2 suspect set from
        // the concealed audio sectors it recorded, so a resumed dump retries their convergence.
        if(_c2Supported)
        {
            _resume.ConcealedBlocks ??= [];

            foreach(ulong concealed in _resume.ConcealedBlocks) _c2SuspectAudio.Add(concealed);
        }

        // Read media tags
        ReadCdTags(ref dskType, mediaTags, out sessions, out firstTrackLastSession);

        if(!outputOptical.OpticalCapabilities.HasFlag(OpticalImageCapabilities.CanStoreSessions) && sessions > 1)
        {
            // TODO: Disabled until 6.0
            /*if(!_force)
            {*/

            StoppingErrorMessage?.Invoke(Localization.Core.Output_format_does_not_support_sessions);

            return;
            /*}

            _dumpLog.WriteLine("Output format does not support sessions, this will end in a loss of data, continuing...");

            ErrorMessage?.
                Invoke("Output format does not support sessions, this will end in a loss of data, continuing...");*/
        }

        // Check if output format supports all disc tags we have retrieved so far
        foreach(MediaTagType tag in mediaTags.Keys.Where(tag => !outputOptical.SupportedMediaTags.Contains(tag)))
        {
            if(_force)
                ErrorMessage?.Invoke(string.Format(Localization.Core.Output_format_does_not_support_0_continuing, tag));
            else
            {
                StoppingErrorMessage?.Invoke(string.Format(Localization.Core
                                                                       .Output_format_does_not_support_0_not_continuing,
                                                           tag));

                return;
            }
        }

        if(leadOutStarts.Any())
        {
            UpdateStatus?.Invoke(Localization.Core.Solving_lead_outs);

            foreach(KeyValuePair<int, long> leadOuts in leadOutStarts)
            {
                foreach(Track trk in tracks.Where(trk => trk.Session   == leadOuts.Key &&
                                                         trk.EndSector >= (ulong)leadOuts.Value))
                    trk.EndSector = (ulong)leadOuts.Value - 1;
            }

            var dataExtents = new ExtentsULong();

            foreach(Track trk in tracks) dataExtents.Add(trk.StartSector, trk.EndSector);

            Tuple<ulong, ulong>[] dataExtentsArray = dataExtents.ToArray();

            for(var i = 0; i < dataExtentsArray.Length - 1; i++)
                leadOutExtents.Add(dataExtentsArray[i].Item2 + 1, dataExtentsArray[i + 1].Item1 - 1);
        }

        UpdateStatus?.Invoke(Localization.Core.Detecting_disc_type);

        MMC.DetectDiscType(ref dskType,
                           sessions,
                           toc,
                           _dev,
                           out hiddenTrack,
                           out hiddenData,
                           firstTrackLastSession,
                           blocks);

        // Fix CD-i discs with wrong Lead-Out type
        if(dskType is MediaType.CDI or MediaType.CDIREADY && tracks.Length == 1 && tracks[0].Type == TrackType.Audio)
            tracks[0].Type = TrackType.CdMode2Formless;

        if(hiddenTrack || firstLba > 0)
        {
            UpdateStatus?.Invoke(Localization.Core.Disc_contains_a_hidden_track);

            if(!outputOptical.OpticalCapabilities.HasFlag(OpticalImageCapabilities.CanStoreHiddenTracks))
            {
                StoppingErrorMessage?.Invoke(Localization.Core.Output_format_does_not_support_hidden_tracks);

                return;
            }

            List<Track> trkList =
            [
                new()
                {
                    Sequence          = (uint)(tracks.Any(static t => t.Sequence == 1) ? 0 : 1),
                    Session           = 1,
                    Type              = hiddenData ? TrackType.Data : TrackType.Audio,
                    StartSector       = 0,
                    BytesPerSector    = (int)sectorSize,
                    RawBytesPerSector = (int)sectorSize,
                    SubchannelType    = subType,
                    EndSector         = tracks.First(static t => t.Sequence >= 1).StartSector - 1
                },
                .. tracks
            ];

            tracks = [.. trkList];
        }

        if(tracks.Any(static t => t.Type == TrackType.Audio) && desiredSubchannel != MmcSubchannel.Raw)
            UpdateStatus?.Invoke(Localization.Core.WARNING_If_disc_says_CDG_CDEG_CDMIDI_dump_will_be_incorrect);

        CheckTracksMode(readcd, tracks, blockSize, supportedSubchannel, dskType);

        if(outputOptical.Id == new Guid("12345678-AAAA-BBBB-CCCC-123456789000"))
        {
            if(tracks.Length > 1)
            {
                StoppingErrorMessage?.Invoke(Localization.Core
                                                         .Output_format_does_not_support_more_than_1_track_not_continuing);


                return;
            }

            if(tracks.Any(static t => t.Type == TrackType.Audio))
            {
                StoppingErrorMessage?.Invoke(Localization.Core
                                                         .Output_format_does_not_support_audio_tracks_not_continuing);


                return;
            }

            if(tracks.Any(static t => t.Type != TrackType.CdMode1))
            {
                StoppingErrorMessage?.Invoke(Localization.Core
                                                         .Output_format_only_supports_MODE_1_tracks_not_continuing);


                return;
            }

            supportsLongSectors = false;
        }

        // Check if something prevents from dumping the first track pregap
        if(_dumpFirstTrackPregap && readcd)
        {
            if(!outputOptical.SupportedMediaTags.Contains(MediaTagType.CD_FirstTrackPregap))
            {
                if(_force)
                {
                    ErrorMessage?.Invoke(Localization.Core
                                                     .Output_format_does_not_support_CD_first_track_pregap_continuing);
                }
                else
                {
                    StoppingErrorMessage?.Invoke(Localization.Core
                                                             .Output_format_does_not_support_CD_first_track_pregap_not_continuing);

                    return;
                }

                _dumpFirstTrackPregap = false;
            }
        }

        // Try how many blocks are readable at once
        while(true)
        {
            if(readcd)
            {
                sense = _dev.ReadCd(out cmdBuf,
                                    out _,
                                    firstLba,
                                    blockSize,
                                    _maximumReadable,
                                    MmcSectorTypes.AllTypes,
                                    false,
                                    false,
                                    true,
                                    MmcHeaderCodes.AllHeaders,
                                    true,
                                    true,
                                    MmcErrorField.None,
                                    supportedSubchannel,
                                    _dev.Timeout,
                                    out _);

                if(_dev.Error || sense) _maximumReadable /= 2;
            }
            else if(read16)
            {
                sense = _dev.Read16(out cmdBuf,
                                    out _,
                                    0,
                                    false,
                                    true,
                                    false,
                                    firstLba,
                                    blockSize,
                                    0,
                                    _maximumReadable,
                                    false,
                                    _dev.Timeout,
                                    out _);

                if(_dev.Error || sense) _maximumReadable /= 2;
            }
            else if(read12)
            {
                sense = _dev.Read12(out cmdBuf,
                                    out _,
                                    0,
                                    false,
                                    true,
                                    false,
                                    false,
                                    firstLba,
                                    blockSize,
                                    0,
                                    _maximumReadable,
                                    false,
                                    _dev.Timeout,
                                    out _);

                if(_dev.Error || sense) _maximumReadable /= 2;
            }
            else if(read10)
            {
                sense = _dev.Read10(out cmdBuf,
                                    out _,
                                    0,
                                    false,
                                    true,
                                    false,
                                    false,
                                    firstLba,
                                    blockSize,
                                    0,
                                    (ushort)_maximumReadable,
                                    _dev.Timeout,
                                    out _);

                if(_dev.Error || sense) _maximumReadable /= 2;
            }
            else if(read6)
            {
                sense = _dev.Read6(out cmdBuf, out _, firstLba, blockSize, (byte)_maximumReadable, _dev.Timeout, out _);

                if(_dev.Error || sense) _maximumReadable /= 2;
            }

            if(!_dev.Error || _maximumReadable == 1) break;
        }

        if(_dev.Error || sense)
        {
            StoppingErrorMessage?.Invoke(string.Format(Localization.Core
                                                                   .Device_error_0_trying_to_guess_ideal_transfer_length,
                                                       _dev.LastError));

            return;
        }

        var cdiWithHiddenTrack1 = false;

        if(dskType is MediaType.CDIREADY && tracks.Min(static t => t.Sequence) == 1)
        {
            cdiWithHiddenTrack1 = true;
            dskType             = MediaType.CDI;
        }

        UpdateStatus?.Invoke(string.Format(Localization.Core.Reading_0_sectors_at_a_time, _maximumReadable));

        UpdateStatus?.Invoke(string.Format(Localization.Core.Device_reports_0_blocks_1_bytes,
                                           blocks,
                                           blocks * blockSize));

        UpdateStatus?.Invoke(string.Format(Localization.Core.Device_can_read_0_blocks_at_a_time, _maximumReadable));
        UpdateStatus?.Invoke(string.Format(Localization.Core.Device_reports_0_bytes_per_logical_block, blockSize));
        UpdateStatus?.Invoke(string.Format(Localization.Core.SCSI_device_type_0, _dev.ScsiType));
        UpdateStatus?.Invoke(string.Format(Localization.Core.Media_identified_as_0, dskType.Humanize()));

        ret = outputOptical.Create(_outputPath,
                                   dskType,
                                   _formatOptions,
                                   blocks,
                                   (uint)(outputOptical.OpticalCapabilities.HasFlag(OpticalImageCapabilities
                                             .CanStoreNegativeSectors)
                                              ? 2750
                                              : 0),
                                   (uint)(outputOptical.OpticalCapabilities.HasFlag(OpticalImageCapabilities
                                             .CanStoreOverflowSectors)
                                              ? 2750
                                              : 0),
                                   supportsLongSectors ? blockSize : 2048);

        // Cannot create image
        if(!ret)
        {
            AaruLogging.WriteLine(Localization.Core.Error_creating_output_image_not_continuing);
            AaruLogging.WriteLine(outputOptical.ErrorMessage);

            StoppingErrorMessage?.Invoke(Localization.Core.Error_creating_output_image_not_continuing +
                                         Environment.NewLine                                          +
                                         outputOptical.ErrorMessage);

            return;
        }

        if(outputOptical is AaruFormat aif && _errorRecovery > 0) aif.SetErasureCodingAuto((byte)_errorRecovery);

        ErrorNumber errno = outputOptical.ReadMediaTag(MediaTagType.CD_MCN, out byte[] mcnBytes);

        if(errno == ErrorNumber.NoError) mcn = Encoding.ASCII.GetString(mcnBytes);

        // Resuming, read previous tracks
        if(outputOptical.Tracks != null)
        {
            foreach(Track imgTrack in outputOptical.Tracks)
            {
                errno = outputOptical.ReadSectorTag(imgTrack.Sequence,
                                                    false,
                                                    SectorTagType.CdTrackIsrc,
                                                    out byte[] isrcBytes);

                if(errno == ErrorNumber.NoError) isrcs[(byte)imgTrack.Sequence] = Encoding.ASCII.GetString(isrcBytes);

                Track trk = tracks.FirstOrDefault(t => t.Sequence == imgTrack.Sequence);

                if(trk == null) continue;

                trk.Pregap      = imgTrack.Pregap;
                trk.StartSector = imgTrack.StartSector;
                trk.EndSector   = imgTrack.EndSector;

                foreach(KeyValuePair<ushort, int> imgIdx in imgTrack.Indexes) trk.Indexes[imgIdx.Key] = imgIdx.Value;
            }
        }

        // Send track list to output plugin. This may fail if subchannel is set but unsupported.
        ret = outputOptical.SetTracks([.. tracks]);

        if(!ret && desiredSubchannel == MmcSubchannel.None)
        {
            AaruLogging.WriteLine(Localization.Core.Error_sending_tracks_to_output_image_not_continuing);
            AaruLogging.WriteLine(outputOptical.ErrorMessage);

            StoppingErrorMessage?.Invoke(Localization.Core.Error_sending_tracks_to_output_image_not_continuing +
                                         Environment.NewLine                                                   +
                                         outputOptical.ErrorMessage);

            return;
        }

        // If a subchannel is supported, check if output plugin allows us to write it.
        if(desiredSubchannel != MmcSubchannel.None &&
           !outputOptical.OpticalCapabilities.HasFlag(OpticalImageCapabilities.CanStoreSubchannelRw))
        {
            if(_force)
                ErrorMessage?.Invoke(Localization.Core.Output_format_does_not_support_subchannels_continuing);
            else
            {
                StoppingErrorMessage?.Invoke(Localization.Core
                                                         .Output_format_does_not_support_subchannels_not_continuing);

                return;
            }
        }

        if(supportedSubchannel != MmcSubchannel.None)
        {
            AaruLogging.WriteLine(string.Format(Localization.Core.Creating_subchannel_log_in_0,
                                                Markup.Escape(_outputPrefix + ".sub.log")));

            subLog = new SubchannelLog(_outputPrefix + ".sub.log", bcdSubchannel);
        }

        // Set track flags
        foreach(KeyValuePair<byte, byte> kvp in trackFlags)
        {
            Track track = tracks.FirstOrDefault(t => t.Sequence == kvp.Key);

            if(track is null) continue;

            UpdateStatus?.Invoke(string.Format(Localization.Core.Setting_flags_for_track_0, track.Sequence));

            outputOptical.WriteSectorTag([kvp.Value], kvp.Key, false, SectorTagType.CdTrackFlags);
        }

        // Set MCN
        if(supportedSubchannel == MmcSubchannel.None)
        {
            sense = _dev.ReadMcn(out mcn, out _, out _, _dev.Timeout, out _);

            if(!sense && mcn != null && mcn != "0000000000000")
                UpdateStatus?.Invoke(string.Format(Localization.Core.Found_Media_Catalogue_Number_0, mcn));
            else
                mcn = null;
        }

        // Set ISRCs
        if(supportedSubchannel == MmcSubchannel.None)
        {
            foreach(Track trk in tracks)
            {
                sense = _dev.ReadIsrc((byte)trk.Sequence, out string isrc, out _, out _, _dev.Timeout, out _);

                if(sense || isrc is null or "000000000000") continue;

                isrcs[(byte)trk.Sequence] = isrc;

                UpdateStatus?.Invoke(string.Format(Localization.Core.Found_ISRC_for_track_0_1, trk.Sequence, isrc));
            }
        }

        if(supportedSubchannel != MmcSubchannel.None && desiredSubchannel != MmcSubchannel.None)
        {
            subchannelExtents = [];

            _resume.BadSubchannels ??= [];

            foreach(int sub in _resume.BadSubchannels) subchannelExtents.Add(sub);

            if(_resume.NextBlock < blocks)
            {
                for(ulong i = _resume.NextBlock; i < blocks; i++) subchannelExtents.Add((int)i);
            }
        }

        if(_resume.NextBlock > 0)
            UpdateStatus?.Invoke(string.Format(Localization.Core.Resuming_from_block_0, _resume.NextBlock));

        if(_skip < _maximumReadable) _skip = _maximumReadable;

#if DEBUG
        foreach(Track trk in tracks)
        {
            UpdateStatus?.Invoke(string.Format(Localization.Core.Track_0_starts_at_LBA_1_and_ends_at_LBA_2,
                                               trk.Sequence,
                                               trk.StartSector,
                                               trk.EndSector));
        }
#endif

        // Check offset
        if(_fixOffset)
        {
            if(tracks.All(static t => t.Type != TrackType.Audio))
            {
                // No audio tracks so no need to fix offset
                UpdateStatus?.Invoke(Localization.Core.No_audio_tracks_disabling_offset_fix);

                _fixOffset = false;
            }

            if(!readcd)
            {
                UpdateStatus?.Invoke(Localization.Core
                                                 .READ_CD_command_is_not_supported_disabling_offset_fix_Dump_may_not_be_correct);

                _fixOffset = false;
            }
        }
        else if(tracks.Any(static t => t.Type == TrackType.Audio))
        {
            UpdateStatus?.Invoke(Localization.Core
                                             .There_are_audio_tracks_and_offset_fixing_is_disabled_dump_may_not_be_correct);
        }

        // Search for read offset in main database
        cdOffset =
            _ctx.CdOffsets.FirstOrDefault(d => (d.Manufacturer == _dev.Manufacturer ||
                                                d.Manufacturer == _dev.Manufacturer.Replace('/', '-')) &&
                                               (d.Model == _dev.Model || d.Model == _dev.Model.Replace('/', '-')));

        Media.Info.CompactDisc.GetOffset(cdOffset,
                                         _dbDev,
                                         _debug,
                                         _dev,
                                         dskType,
                                         tracks,
                                         UpdateStatus,
                                         out int? driveOffset,
                                         out int? combinedOffset,
                                         out _supportsPlextorD8);

        if(combinedOffset is null)
        {
            if(driveOffset is null)
            {
                UpdateStatus?.Invoke(Localization.Core.Drive_reading_offset_not_found_in_database);
                UpdateStatus?.Invoke(Localization.Core.Disc_offset_cannot_be_calculated);

                if(tracks.Any(static t => t.Type == TrackType.Audio))
                    UpdateStatus?.Invoke(Localization.Core.Dump_may_not_be_correct);

                if(_fixOffset) _fixOffset = false;
            }
            else
            {
                UpdateStatus?.Invoke(string.Format(Localization.Core.Drive_reading_offset_is_0_bytes_1_samples,
                                                   driveOffset,
                                                   driveOffset / 4));

                if(tracks.Any(static t => t.Type == TrackType.Audio))
                    UpdateStatus?.Invoke(Localization.Core.Disc_write_offset_is_unknown_dump_may_not_be_correct);

                offsetBytes = driveOffset.Value;

                sectorsForOffset = offsetBytes / (int)sectorSize;

                if(sectorsForOffset < 0) sectorsForOffset *= -1;

                if(offsetBytes % sectorSize != 0) sectorsForOffset++;
            }
        }
        else
        {
            offsetBytes      = combinedOffset.Value;
            sectorsForOffset = offsetBytes / (int)sectorSize;

            if(sectorsForOffset < 0) sectorsForOffset *= -1;

            if(offsetBytes % sectorSize != 0) sectorsForOffset++;

            if(driveOffset is null)
            {
                UpdateStatus?.Invoke(Localization.Core.Drive_reading_offset_not_found_in_database);

                UpdateStatus?.Invoke(string.Format(Localization.Core
                                                               .Combined_disc_and_drive_offset_are_0_bytes_1_samples,
                                                   offsetBytes,
                                                   offsetBytes / 4));
            }
            else
            {
                UpdateStatus?.Invoke(string.Format(Localization.Core.Drive_reading_offset_is_0_bytes_1_samples,
                                                   driveOffset,
                                                   driveOffset / 4));

                discOffset = offsetBytes - driveOffset;


                UpdateStatus?.Invoke(string.Format(Localization.Core.Disc_offset_is_0_bytes_1_samples,
                                                   discOffset,
                                                   discOffset / 4));
            }
        }

        mhddLog = new MhddLog(_outputPrefix + ".mhddlog.bin",
                              _dev,
                              blocks,
                              blockSize,
                              _maximumReadable,
                              _private,
                              _dimensions);

        ibgLog = new IbgLog(_outputPrefix + ".ibg", 0x0008);

        if(_createGraph)
        {
            Spiral.DiscParameters discSpiralParameters = Spiral.DiscParametersFromMediaType(dskType);

            if(discSpiralParameters is not null)
                _mediaGraph = new Spiral((int)_dimensions, (int)_dimensions, discSpiralParameters, blocks);
            else
                _mediaGraph = new BlockMap((int)_dimensions, (int)_dimensions, blocks);

            _mediaGraph?.TryLoadExisting($"{_outputPrefix}.graph.png");

            if(_mediaGraph is not null)
            {
                foreach(Tuple<ulong, ulong> e in extents.ToArray())
                    _mediaGraph?.PaintSectorsGood(e.Item1, (uint)(e.Item2 - e.Item1 + 1));
            }

            _mediaGraph?.PaintSectorsBad(_resume.BadBlocks);
        }

        // Try to read the first track pregap
        if(_dumpFirstTrackPregap && (readcd || _omnidrive))
        {
            ReadCdFirstTrackPregap(blockSize,
                                   ref currentSpeed,
                                   mediaTags,
                                   supportedSubchannel,
                                   ref totalDuration,
                                   outputOptical,
                                   subLog,
                                   offsetBytes,
                                   sectorsForOffset,
                                   desiredSubchannel,
                                   subchannelExtents);
        }

        audioExtents = new ExtentsULong();

        foreach(Track audioTrack in tracks.Where(static t => t.Type == TrackType.Audio))
            audioExtents.Add(audioTrack.StartSector, audioTrack.EndSector);

        // Set speed
        _speedCapKbps = ComputeSpeedCap();

        if(_speedMultiplier >= 0)
        {
            UpdateStatus?.Invoke(_speed is 0xFFFF or 0
                                     ? Localization.Core.Setting_speed_to_MAX_for_data_reading
                                     : string.Format(Localization.Core.Setting_speed_to_0_x_for_data_reading, _speed));

            SetCdSpeedClamped(0xFFFF);
        }

        // Start reading
        _dumpStopwatch.Restart();

        if(dskType == MediaType.CDIREADY || cdiWithHiddenTrack1)
        {
            Track track0 = tracks.FirstOrDefault(static t => t.Sequence is 0 or 1);

            track0.Type = TrackType.CdMode2Formless;

            if(!supportsLongSectors)
            {
                StoppingErrorMessage?.Invoke(Localization.Core
                                                         .Dumping_CD_i_Ready_requires_the_output_image_format_to_support_long_sectors);

                return;
            }

            if(!readcd)
            {
                StoppingErrorMessage?.Invoke(Localization.Core
                                                         .Dumping_CD_i_Ready_requires_the_drive_to_support_the_READ_CD_command);

                return;
            }

            _dev.ReadCd(out cmdBuf,
                        out _,
                        0,
                        2352,
                        1,
                        MmcSectorTypes.AllTypes,
                        false,
                        false,
                        true,
                        MmcHeaderCodes.AllHeaders,
                        true,
                        true,
                        MmcErrorField.None,
                        MmcSubchannel.None,
                        _dev.Timeout,
                        out _);

            hiddenData = IsData(cmdBuf);

            if(!hiddenData)
            {
                cdiReadyReadAsAudio = IsScrambledData(cmdBuf, 0, out combinedOffset);

                if(cdiReadyReadAsAudio)
                {
                    offsetBytes      = combinedOffset.Value;
                    sectorsForOffset = offsetBytes / (int)sectorSize;

                    if(sectorsForOffset < 0) sectorsForOffset *= -1;

                    if(offsetBytes % sectorSize != 0) sectorsForOffset++;

                    UpdateStatus?.Invoke(Localization.Core
                                                     .Enabling_skipping_CD_i_Ready_hole_because_drive_returns_data_as_audio);

                    _skipCdireadyHole = true;

                    if(driveOffset is null)
                    {
                        UpdateStatus?.Invoke(Localization.Core.Drive_reading_offset_not_found_in_database);


                        UpdateStatus?.Invoke(string.Format(Localization.Core
                                                                       .Combined_disc_and_drive_offset_are_0_bytes_1_samples,
                                                           offsetBytes,
                                                           offsetBytes / 4));
                    }
                    else
                    {
                        UpdateStatus?.Invoke(string.Format(Localization.Core.Drive_reading_offset_is_0_bytes_1_samples,
                                                           driveOffset,
                                                           driveOffset / 4));

                        discOffset = offsetBytes - driveOffset;


                        UpdateStatus?.Invoke(string.Format(Localization.Core.Disc_offset_is_0_bytes_1_samples,
                                                           discOffset,
                                                           discOffset / 4));
                    }
                }
            }

            if(!_skipCdireadyHole)
            {
                UpdateStatus?.Invoke(Localization.Core
                                                 .There_will_be_thousand_of_errors_between_track_0_and_track_1_that_is_normal_and_you_can_ignore_them);
            }

            if(_skipCdireadyHole)
            {
                // This codepath is disabled because it's doing strange things, let the OmniDrive behave like a normal drive here
                /*
                if(_omnidrive)
                {
                    OmniDriveReadCdiReady(blockSize,
                                          ref currentSpeed,
                                          currentTry,
                                          extents,
                                          ibgLog,
                                          ref imageWriteDuration,
                                          leadOutExtents,
                                          ref maxSpeed,
                                          mhddLog,
                                          ref minSpeed,
                                          subSize,
                                          supportedSubchannel,
                                          ref totalDuration,
                                          tracks,
                                          subLog,
                                          desiredSubchannel,
                                          isrcs,
                                          ref mcn,
                                          subchannelExtents,
                                          blocks,
                                          cdiReadyReadAsAudio,
                                          offsetBytes,
                                          sectorsForOffset,
                                          smallestPregapLbaPerTrack);
                }
                else
                {
                */
                ReadCdiReady(blockSize,
                             ref currentSpeed,
                             currentTry,
                             extents,
                             ibgLog,
                             ref imageWriteDuration,
                             leadOutExtents,
                             ref maxSpeed,
                             mhddLog,
                             ref minSpeed,
                             subSize,
                             supportedSubchannel,
                             ref totalDuration,
                             tracks,
                             subLog,
                             desiredSubchannel,
                             isrcs,
                             ref mcn,
                             subchannelExtents,
                             blocks,
                             cdiReadyReadAsAudio,
                             offsetBytes,
                             sectorsForOffset,
                             smallestPregapLbaPerTrack);

                //}
            }
        }

        if(_omnidrive)
        {
            ReadCdDataOmniDrive(audioExtents,
                                blocks,
                                blockSize,
                                ref currentSpeed,
                                currentTry,
                                extents,
                                ibgLog,
                                ref imageWriteDuration,
                                lastSector,
                                leadOutExtents,
                                ref maxSpeed,
                                mhddLog,
                                ref minSpeed,
                                out newTrim,
                                offsetBytes,
                                sectorsForOffset,
                                subSize,
                                supportedSubchannel,
                                supportsLongSectors,
                                ref totalDuration,
                                tracks,
                                subLog,
                                desiredSubchannel,
                                isrcs,
                                ref mcn,
                                subchannelExtents,
                                smallestPregapLbaPerTrack);
        }
        else
        {
            ReadCdData(audioExtents,
                       blocks,
                       blockSize,
                       ref currentSpeed,
                       currentTry,
                       extents,
                       ibgLog,
                       ref imageWriteDuration,
                       lastSector,
                       leadOutExtents,
                       ref maxSpeed,
                       mhddLog,
                       ref minSpeed,
                       out newTrim,
                       tracks[0].Type != TrackType.Audio,
                       offsetBytes,
                       read6,
                       read10,
                       read12,
                       read16,
                       readcd,
                       sectorsForOffset,
                       subSize,
                       supportedSubchannel,
                       supportsLongSectors,
                       ref totalDuration,
                       tracks,
                       subLog,
                       desiredSubchannel,
                       isrcs,
                       ref mcn,
                       subchannelExtents,
                       smallestPregapLbaPerTrack);
        }

        if(_omnidrive                                                                                  &&
           outputOptical.OpticalCapabilities.HasFlag(OpticalImageCapabilities.CanStoreOverflowSectors) &&
           _leadout)
        {
            leadOutExtents.Add((ulong)(lastSector + 1), (ulong)(lastSector + 2749));

            DumpCdLeadOuts(blockSize,
                           ref currentSpeed,
                           currentTry,
                           extents,
                           ibgLog,
                           ref imageWriteDuration,
                           leadOutExtents,
                           ref maxSpeed,
                           mhddLog,
                           ref minSpeed,
                           read6,
                           read10,
                           read12,
                           read16,
                           readcd,
                           supportedSubchannel,
                           subSize,
                           ref totalDuration,
                           subLog,
                           desiredSubchannel,
                           isrcs,
                           ref mcn,
                           tracks,
                           subchannelExtents,
                           smallestPregapLbaPerTrack,
                           offsetBytes,
                           sectorsForOffset);
        }

        _dumpStopwatch.Stop();
        mhddLog.Close();

        ibgLog.Close(_dev,
                     blocks,
                     blockSize,
                     _dumpStopwatch.Elapsed.TotalSeconds,
                     currentSpeed                     * 1024,
                     blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000),
                     _devicePath);

        UpdateStatus?.Invoke(string.Format(_aborted ? Localization.Core.Dump_aborted_after_0 : Localization.Core.Dump_finished_in_0,
                                           _dumpStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second)));

        UpdateStatus?.Invoke(string.Format(Localization.Core.Average_dump_speed_0,
                                           ByteSize.FromBytes(blockSize * (blocks + 1))
                                                   .Per(totalDuration.Milliseconds())
                                                   .Humanize()));

        UpdateStatus?.Invoke(string.Format(Localization.Core.Average_write_speed_0,
                                           ByteSize.FromBytes(blockSize * (blocks + 1))
                                                   .Per(imageWriteDuration.Seconds())
                                                   .Humanize()));


        TrimCdUserData(audioExtents,
                       blockSize,
                       currentTry,
                       extents,
                       newTrim,
                       offsetBytes,
                       read6,
                       read10,
                       read12,
                       read16,
                       readcd,
                       sectorsForOffset,
                       subSize,
                       supportedSubchannel,
                       supportsLongSectors,
                       ref totalDuration,
                       subLog,
                       desiredSubchannel,
                       tracks,
                       isrcs,
                       ref mcn,
                       subchannelExtents,
                       smallestPregapLbaPerTrack);

        if(dskType is MediaType.CDR or MediaType.CDRW && _resume.BadBlocks.Count > 0 && _ignoreCdrRunOuts > 0)
        {
            HandleCdrRunOutSectors(blocks,
                                   desiredSubchannel,
                                   extents,
                                   subchannelExtents,
                                   subLog,
                                   supportsLongSectors,
                                   trackFlags,
                                   tracks);
        }

        RetryCdUserData(audioExtents,
                        blockSize,
                        currentTry,
                        extents,
                        offsetBytes,
                        readcd,
                        sectorsForOffset,
                        subSize,
                        supportedSubchannel,
                        ref totalDuration,
                        subLog,
                        desiredSubchannel,
                        tracks,
                        isrcs,
                        ref mcn,
                        subchannelExtents,
                        smallestPregapLbaPerTrack,
                        supportsLongSectors);

        // Re-read the audio sectors the drive concealed until they converge on stable, C2-clean data, rewriting the
        // ones that do. Runs after the normal retries so hard errors are dealt with first.
        ConvergeAudioC2(offsetBytes, sectorsForOffset, supportedSubchannel, extents, outputOptical, readcd);

        if(_c2Supported && audioExtents.Count > 0)
        {
            int concealed = _c2SuspectAudio?.Count ?? 0;

            UpdateStatus?.Invoke(concealed == 0
                                     ? Localization.Core.C2_audio_check_no_concealed
                                     : string.Format(Localization.Core.C2_audio_check_0_concealed_samples, concealed));

            AaruLogging.WriteLine(string.Format(Localization.Core.C2_audio_check_0_flagged, concealed));
        }

        foreach(Tuple<ulong, ulong> leadoutExtent in leadOutExtents.ToArray())
        {
            for(ulong e = leadoutExtent.Item1; e <= leadoutExtent.Item2; e++) subchannelExtents.Remove((int)e);
        }

        if(subchannelExtents.Count > 0 && _retryPasses > 0 && _retrySubchannel)
        {
            RetrySubchannel(readcd,
                            subSize,
                            supportedSubchannel,
                            ref totalDuration,
                            subLog,
                            desiredSubchannel,
                            tracks,
                            isrcs,
                            ref mcn,
                            subchannelExtents,
                            smallestPregapLbaPerTrack);
        }

        // Write media tags to image
        if(!_aborted)
        {
            foreach(KeyValuePair<MediaTagType, byte[]> tag in mediaTags)
            {
                if(tag.Value is null)
                {
                    AaruLogging.Error(Localization.Core.Error_Tag_type_0_is_null_skipping, tag.Key);

                    continue;
                }

                ret = outputOptical.WriteMediaTag(tag.Value, tag.Key);

                if(ret || _force) continue;

                // Cannot write tag to image
                StoppingErrorMessage?.Invoke(outputOptical.ErrorMessage);

                return;
            }
        }

        _resume.BadBlocks.Sort();

        foreach(ulong bad in _resume.BadBlocks)
            AaruLogging.Information(Localization.Core.Sector_0_could_not_be_read, bad);

        currentTry.Extents = ExtentsConverter.ToMetadata(extents);

        _resume.BadSubchannels = [];
        _resume.BadSubchannels.AddRange(subchannelExtents);
        _resume.BadSubchannels.Sort();

        if(_generateSubchannels                                                         &&
           outputOptical.SupportedSectorTags.Contains(SectorTagType.CdSectorSubchannel) &&
           !_aborted)
        {
            Media.CompactDisc.GenerateSubchannels(subchannelExtents,
                                                  tracks,
                                                  trackFlags,
                                                  blocks,
                                                  subLog,
                                                  InitProgress,
                                                  UpdateProgress,
                                                  EndProgress,
                                                  outputOptical);
        }

        // TODO: Disc ID
        var metadata = new CommonTypes.Structs.ImageInfo
        {
            Application        = "Aaru",
            ApplicationVersion = Version.GetInformationalVersion()
        };

        if(!outputOptical.SetImageInfo(metadata))
        {
            ErrorMessage?.Invoke(Localization.Core.Error_0_setting_metadata +
                                 Environment.NewLine                        +
                                 outputOptical.ErrorMessage);
        }

        outputOptical.SetDumpHardware(_resume.Tries);

        if(_preSidecar != null) outputOptical.SetMetadata(_preSidecar);

        foreach(KeyValuePair<byte, string> isrc in isrcs)
        {
            // TODO: Track tags
            if(!outputOptical.WriteSectorTag(Encoding.ASCII.GetBytes(isrc.Value),
                                             isrc.Key,
                                             false,
                                             SectorTagType.CdTrackIsrc))
                continue;

            UpdateStatus?.Invoke(string.Format(Localization.Core.Setting_ISRC_for_track_0_to_1, isrc.Key, isrc.Value));
        }

        if(mcn != null && outputOptical.WriteMediaTag(Encoding.ASCII.GetBytes(mcn), MediaTagType.CD_MCN))
            UpdateStatus?.Invoke(string.Format(Localization.Core.Setting_disc_Media_Catalogue_Number_to_0, mcn));

        foreach(Track trk in tracks)
        {
            // Fix track starts in each session's first track
            if(tracks.Where(t => t.Session == trk.Session).MinBy(static t => t.Sequence).Sequence == trk.Sequence)
            {
                if(trk.Sequence == 1) continue;

                trk.StartSector -= trk.Pregap;
                trk.Indexes[0]  =  (int)trk.StartSector;

                continue;
            }

            if(trk.Indexes.TryGetValue(0, out int idx0) && trk.Indexes.TryGetValue(1, out int idx1) && idx0 == idx1)
                trk.Indexes.Remove(0);
        }

        outputOptical.SetTracks(tracks.ToList());

        UpdateStatus?.Invoke(Localization.Core.Closing_output_file);
        _imageCloseStopwatch.Restart();
        outputOptical.Close();
        _imageCloseStopwatch.Stop();

        UpdateStatus?.Invoke(string.Format(Localization.Core.Closed_in_0,
                                           _imageCloseStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second)));

        subLog?.Close();

        if(_aborted)
        {
            UpdateStatus?.Invoke(Localization.Core.Aborted);

            return;
        }

        double totalChkDuration = 0;

        if(_metadata)
        {
            WriteOpticalSidecar(blockSize,
                                blocks,
                                dskType,
                                null,
                                mediaTags,
                                sessions,
                                out totalChkDuration,
                                discOffset);
        }

        _dumpStopwatch.Stop();
        UpdateStatus?.Invoke("");

        UpdateStatus?.Invoke(string.Format(Localization.Core
                                                       .Took_a_total_of_0_1_processing_commands_2_checksumming_3_writing_4_closing,
                                           _dumpStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second),
                                           totalDuration.Milliseconds().Humanize(minUnit: TimeUnit.Second),
                                           totalChkDuration.Milliseconds().Humanize(minUnit: TimeUnit.Second),
                                           imageWriteDuration.Seconds().Humanize(minUnit: TimeUnit.Second),
                                           _imageCloseStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second)));

        UpdateStatus?.Invoke(string.Format(Localization.Core.Average_speed_0,
                                           ByteSize.FromBytes(blockSize * (blocks + 1))
                                                   .Per(totalDuration.Milliseconds())
                                                   .Humanize()));

        if(maxSpeed > 0)
        {
            UpdateStatus?.Invoke(string.Format(Localization.Core.Fastest_speed_burst_0,
                                               ByteSize.FromMegabytes(maxSpeed).Per(_oneSecond).Humanize()));
        }

        if(minSpeed is > 0 and < double.MaxValue)
        {
            UpdateStatus?.Invoke(string.Format(Localization.Core.Slowest_speed_burst_0,
                                               ByteSize.FromMegabytes(minSpeed).Per(_oneSecond).Humanize()));
        }

        if(_omnidrive)
        {
            UpdateStatus?.Invoke(string.Format(UI._0_sectors_were_correct_when_read, _correctSectors));
            UpdateStatus?.Invoke(string.Format(UI._0_sectors_had_to_be_fixed,        _fixedSectors));
        }

        UpdateStatus?.Invoke(string.Format(Localization.Core._0_sectors_could_not_be_read, _resume.BadBlocks.Count));

        UpdateStatus?.Invoke(string.Format(Localization.Core._0_subchannels_could_not_be_read,
                                           _resume.BadSubchannels.Count));

        UpdateStatus?.Invoke("");

        Statistics.AddMedia(dskType, true);
    }
}