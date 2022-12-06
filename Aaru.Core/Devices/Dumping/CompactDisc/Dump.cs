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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Core.Graphics;
using Aaru.Core.Logging;
using Aaru.Core.Media.Detection;
using Aaru.Database.Models;
using Aaru.Decoders.CD;
using Aaru.Devices;
using Schemas;
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
        ExtentsULong             audioExtents; // Extents with audio sectors
        ulong                    blocks; // Total number of positive sectors
        uint                     blockSize; // Size of the read sector in bytes
        CdOffset                 cdOffset; // Read offset from database
        byte[]                   cmdBuf; // Data buffer
        DumpHardwareType         currentTry   = null; // Current dump hardware try
        double                   currentSpeed = 0; // Current read speed
        int?                     discOffset   = null; // Disc write offset
        DateTime                 dumpStart    = DateTime.UtcNow; // Time of dump start
        DateTime                 end; // Time of operation end
        ExtentsULong             extents = null; // Extents
        bool                     hiddenData; // Hidden track is data
        IbgLog                   ibgLog; // IMGBurn log
        double                   imageWriteDuration = 0; // Duration of image write
        long                     lastSector; // Last sector number
        var                      leadOutExtents = new ExtentsULong(); // Lead-out extents
        Dictionary<int, long>    leadOutStarts  = new(); // Lead-out starts
        double                   maxSpeed       = double.MinValue; // Maximum speed
        MhddLog                  mhddLog; // MHDD log
        double                   minSpeed = double.MaxValue; // Minimum speed
        bool                     newTrim; // Is trim a new one?
        int                      offsetBytes = 0; // Read offset
        bool                     read6       = false; // Device supports READ(6)
        bool                     read10      = false; // Device supports READ(10)
        bool                     read12      = false; // Device supports READ(12)
        bool                     read16      = false; // Device supports READ(16)
        bool                     readcd      = true; // Device supports READ CD
        bool                     ret; // Image writing return status
        const uint               sectorSize       = 2352; // Full sector size
        int                      sectorsForOffset = 0; // Sectors needed to fix offset
        bool                     sense            = true; // Sense indicator
        int                      sessions; // Number of sessions in disc
        DateTime                 start; // Start of operation
        SubchannelLog            subLog              = null; // Subchannel log
        uint                     subSize             = 0; // Subchannel size in bytes
        TrackSubchannelType      subType             = TrackSubchannelType.None; // Track subchannel type
        bool                     supportsLongSectors = true; // Supports reading EDC and ECC
        bool                     supportsPqSubchannel; // Supports reading PQ subchannel
        bool                     supportsRwSubchannel; // Supports reading RW subchannel
        byte[]                   tmpBuf; // Temporary buffer
        FullTOC.CDFullTOC?       toc; // Full CD TOC
        double                   totalDuration = 0; // Total commands duration
        Dictionary<byte, byte>   trackFlags    = new(); // Track flags
        Track[]                  tracks; // Tracks in disc
        int                      firstTrackLastSession; // Number of first track in last session
        bool                     hiddenTrack; // Disc has a hidden track before track 1
        MmcSubchannel            supportedSubchannel; // Drive's maximum supported subchannel
        MmcSubchannel            desiredSubchannel; // User requested subchannel
        bool                     bcdSubchannel       = false; // Subchannel positioning is in BCD
        Dictionary<byte, string> isrcs               = new();
        string                   mcn                 = null;
        HashSet<int>             subchannelExtents   = new();
        bool                     cdiReadyReadAsAudio = false;
        uint                     firstLba;
        var                      outputOptical = _outputPlugin as IWritableOpticalImage;

        Dictionary<MediaTagType, byte[]> mediaTags                 = new(); // Media tags
        Dictionary<byte, int>            smallestPregapLbaPerTrack = new();

        MediaType dskType = MediaType.CD;

        if(_dumpRaw)
        {
            _dumpLog.WriteLine(Localization.Core.Raw_CD_dumping_not_yet_implemented);
            StoppingErrorMessage?.Invoke(Localization.Core.Raw_CD_dumping_not_yet_implemented);

            return;
        }

        tracks = GetCdTracks(_dev, _dumpLog, _force, out lastSector, leadOutStarts, mediaTags, StoppingErrorMessage,
                             out toc, trackFlags, UpdateStatus);

        if(tracks is null)
        {
            _dumpLog.WriteLine(Localization.Core.Could_not_get_tracks);
            StoppingErrorMessage?.Invoke(Localization.Core.Could_not_get_tracks);

            return;
        }

        firstLba = (uint)tracks.Min(t => t.StartSector);

        // Check subchannels support
        supportsPqSubchannel = SupportsPqSubchannel(_dev, _dumpLog, UpdateStatus, firstLba);
        supportsRwSubchannel = SupportsRwSubchannel(_dev, _dumpLog, UpdateStatus, firstLba);

        if(supportsRwSubchannel)
            supportedSubchannel = MmcSubchannel.Raw;
        else if(supportsPqSubchannel)
            supportedSubchannel = MmcSubchannel.Q16;
        else
            supportedSubchannel = MmcSubchannel.None;

        switch(_subchannel)
        {
            case DumpSubchannel.Any:
                if(supportsRwSubchannel)
                    desiredSubchannel = MmcSubchannel.Raw;
                else if(supportsPqSubchannel)
                    desiredSubchannel = MmcSubchannel.Q16;
                else
                    desiredSubchannel = MmcSubchannel.None;

                break;
            case DumpSubchannel.Rw:
                if(supportsRwSubchannel)
                    desiredSubchannel = MmcSubchannel.Raw;
                else
                {
                    _dumpLog.WriteLine(Localization.Core.
                                                    Drive_does_not_support_the_requested_subchannel_format_not_continuing);

                    StoppingErrorMessage?.Invoke(Localization.Core.
                                                              Drive_does_not_support_the_requested_subchannel_format_not_continuing);

                    return;
                }

                break;
            case DumpSubchannel.RwOrPq:
                if(supportsRwSubchannel)
                    desiredSubchannel = MmcSubchannel.Raw;
                else if(supportsPqSubchannel)
                    desiredSubchannel = MmcSubchannel.Q16;
                else
                {
                    _dumpLog.WriteLine(Localization.Core.
                                                    Drive_does_not_support_the_requested_subchannel_format_not_continuing);

                    StoppingErrorMessage?.Invoke(Localization.Core.
                                                              Drive_does_not_support_the_requested_subchannel_format_not_continuing);

                    return;
                }

                break;
            case DumpSubchannel.Pq:
                if(supportsPqSubchannel)
                    desiredSubchannel = MmcSubchannel.Q16;
                else
                {
                    _dumpLog.WriteLine(Localization.Core.
                                                    Drive_does_not_support_the_requested_subchannel_format_not_continuing);

                    StoppingErrorMessage?.Invoke(Localization.Core.
                                                              Drive_does_not_support_the_requested_subchannel_format_not_continuing);

                    return;
                }

                break;
            case DumpSubchannel.None:
                desiredSubchannel = MmcSubchannel.None;

                break;
            default: throw new ArgumentOutOfRangeException();
        }

        if(desiredSubchannel == MmcSubchannel.Q16 && supportsPqSubchannel)
            supportedSubchannel = MmcSubchannel.Q16;

        // Check if output format supports subchannels
        if(!outputOptical.SupportedSectorTags.Contains(SectorTagType.CdSectorSubchannel) &&
           desiredSubchannel != MmcSubchannel.None)
        {
            if(_force || _subchannel == DumpSubchannel.None)
            {
                _dumpLog.WriteLine(Localization.Core.Output_format_does_not_support_subchannels_continuing);
                UpdateStatus?.Invoke(Localization.Core.Output_format_does_not_support_subchannels_continuing);
            }
            else
            {
                _dumpLog.WriteLine(Localization.Core.Output_format_does_not_support_subchannels_not_continuing);

                StoppingErrorMessage?.Invoke(Localization.Core.
                                                          Output_format_does_not_support_subchannels_not_continuing);

                return;
            }

            desiredSubchannel = MmcSubchannel.None;
        }

        switch(supportedSubchannel)
        {
            case MmcSubchannel.None:
                _dumpLog.WriteLine(Localization.Core.Checking_if_drive_supports_reading_without_subchannel);
                UpdateStatus?.Invoke(Localization.Core.Checking_if_drive_supports_reading_without_subchannel);

                readcd = !_dev.ReadCd(out cmdBuf, out _, firstLba, sectorSize, 1, MmcSectorTypes.AllTypes, false, false,
                                      true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                      supportedSubchannel, _dev.Timeout, out _);

                if(!readcd)
                {
                    _dumpLog.WriteLine(Localization.Core.Drive_does_not_support_READ_CD_trying_SCSI_READ_commands);
                    ErrorMessage?.Invoke(Localization.Core.Drive_does_not_support_READ_CD_trying_SCSI_READ_commands);

                    _dumpLog.WriteLine(Localization.Core.Checking_if_drive_supports_READ_6);
                    UpdateStatus?.Invoke(Localization.Core.Checking_if_drive_supports_READ_6);
                    read6 = !_dev.Read6(out cmdBuf, out _, firstLba, 2048, 1, _dev.Timeout, out _);
                    _dumpLog.WriteLine(Localization.Core.Checking_if_drive_supports_READ_10);
                    UpdateStatus?.Invoke(Localization.Core.Checking_if_drive_supports_READ_10);

                    read10 = !_dev.Read10(out cmdBuf, out _, 0, false, true, false, false, firstLba, 2048, 0, 1,
                                          _dev.Timeout, out _);

                    _dumpLog.WriteLine(Localization.Core.Checking_if_drive_supports_READ_12);
                    UpdateStatus?.Invoke(Localization.Core.Checking_if_drive_supports_READ_12);

                    read12 = !_dev.Read12(out cmdBuf, out _, 0, false, true, false, false, firstLba, 2048, 0, 1, false,
                                          _dev.Timeout, out _);

                    _dumpLog.WriteLine(Localization.Core.Checking_if_drive_supports_READ_16);
                    UpdateStatus?.Invoke(Localization.Core.Checking_if_drive_supports_READ_16);

                    read16 = !_dev.Read16(out cmdBuf, out _, 0, false, true, false, firstLba, 2048, 0, 1, false,
                                          _dev.Timeout, out _);

                    switch(read6)
                    {
                        case false when !read10 && !read12 && !read16:
                            _dumpLog.WriteLine(Localization.Core.Cannot_read_from_disc_not_continuing);
                            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_read_from_disc_not_continuing);

                            return;
                        case true:
                            _dumpLog.WriteLine(Localization.Core.Drive_supports_READ_6);
                            UpdateStatus?.Invoke(Localization.Core.Drive_supports_READ_6);

                            break;
                    }

                    if(read10)
                    {
                        _dumpLog.WriteLine(Localization.Core.Drive_supports_READ_10);
                        UpdateStatus?.Invoke(Localization.Core.Drive_supports_READ_10);
                    }

                    if(read12)
                    {
                        _dumpLog.WriteLine(Localization.Core.Drive_supports_READ_12);
                        UpdateStatus?.Invoke(Localization.Core.Drive_supports_READ_12);
                    }

                    if(read16)
                    {
                        _dumpLog.WriteLine(Localization.Core.Drive_supports_READ_16);
                        UpdateStatus?.Invoke(Localization.Core.Drive_supports_READ_16);
                    }
                }

                _dumpLog.WriteLine(Localization.Core.Drive_can_read_without_subchannel);
                UpdateStatus?.Invoke(Localization.Core.Drive_can_read_without_subchannel);

                subSize = 0;
                subType = TrackSubchannelType.None;

                break;
            case MmcSubchannel.Raw:
                _dumpLog.WriteLine(Localization.Core.Full_raw_subchannel_reading_supported);
                UpdateStatus?.Invoke(Localization.Core.Full_raw_subchannel_reading_supported);
                subType = TrackSubchannelType.Raw;
                subSize = 96;

                break;
            case MmcSubchannel.Q16:
                _dumpLog.WriteLine(Localization.Core.PQ_subchannel_reading_supported);
                _dumpLog.WriteLine(Localization.Core.WARNING_If_disc_says_CDG_CDEG_CDMIDI_dump_will_be_incorrect);
                UpdateStatus?.Invoke(Localization.Core.PQ_subchannel_reading_supported);

                UpdateStatus?.Invoke(Localization.Core.WARNING_If_disc_says_CDG_CDEG_CDMIDI_dump_will_be_incorrect);

                subType = TrackSubchannelType.Q16;
                subSize = 16;

                break;
        }

        switch(desiredSubchannel)
        {
            case MmcSubchannel.None:
                subType = TrackSubchannelType.None;

                break;
            case MmcSubchannel.Raw:
            case MmcSubchannel.Q16:
                subType = TrackSubchannelType.Raw;

                break;
        }

        blockSize = sectorSize + subSize;

        // Check if subchannel is BCD
        if(supportedSubchannel != MmcSubchannel.None)
        {
            sense = _dev.ReadCd(out cmdBuf, out _, (((firstLba / 75) + 1) * 75) + 35, blockSize, 1,
                                MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true,
                                MmcErrorField.None, supportedSubchannel, _dev.Timeout, out _);

            if(!sense)
            {
                tmpBuf = new byte[subSize];
                Array.Copy(cmdBuf, sectorSize, tmpBuf, 0, subSize);

                if(supportedSubchannel == MmcSubchannel.Q16)
                    tmpBuf = Subchannel.ConvertQToRaw(tmpBuf);

                tmpBuf = Subchannel.Deinterleave(tmpBuf);

                // 9th Q subchannel is always FRAME when in user data area
                // LBA 35 => MSF 00:02:35 => FRAME 35 (in hexadecimal 0x23)
                // Sometimes drive returns a pregap here but MSF 00:02:3x => FRAME 3x (hexadecimal 0x20 to 0x27)
                bcdSubchannel = (tmpBuf[21] & 0x30) > 0;

                if(bcdSubchannel)
                {
                    _dumpLog.WriteLine(Localization.Core.Drive_returns_subchannel_in_BCD);
                    UpdateStatus?.Invoke(Localization.Core.Drive_returns_subchannel_in_BCD);
                }
                else
                {
                    _dumpLog.WriteLine(Localization.Core.Drive_does_not_returns_subchannel_in_BCD);
                    UpdateStatus?.Invoke(Localization.Core.Drive_does_not_returns_subchannel_in_BCD);
                }
            }
        }

        foreach(Track trk in tracks)
            trk.SubchannelType = subType;

        _dumpLog.WriteLine(Localization.Core.Calculating_pregaps__can_take_some_time);
        UpdateStatus?.Invoke(Localization.Core.Calculating_pregaps__can_take_some_time);

        SolveTrackPregaps(_dev, _dumpLog, UpdateStatus, tracks, supportsPqSubchannel, supportsRwSubchannel, _dbDev,
                          out bool inexactPositioning, true);

        if(inexactPositioning)
        {
            _dumpLog.WriteLine(Localization.Core.The_drive_has_returned_incorrect_Q_positioning_calculating_pregaps);

            UpdateStatus?.Invoke(Localization.Core.The_drive_has_returned_incorrect_Q_positioning_calculating_pregaps);
        }

        if(!outputOptical.OpticalCapabilities.HasFlag(OpticalImageCapabilities.CanStoreRawData))
        {
            if(!_force)
            {
                _dumpLog.WriteLine(Localization.Core.Output_format_does_not_support_storing_raw_data_not_continuing);

                StoppingErrorMessage?.Invoke(Localization.Core.
                                                          Output_format_does_not_support_storing_raw_data_not_continuing);

                return;
            }

            _dumpLog.WriteLine(Localization.Core.Output_format_does_not_support_storing_raw_data_continuing);

            ErrorMessage?.Invoke(Localization.Core.Output_format_does_not_support_storing_raw_data_continuing);
        }

        if(!outputOptical.OpticalCapabilities.HasFlag(OpticalImageCapabilities.CanStoreAudioTracks) &&
           tracks.Any(track => track.Type == TrackType.Audio))
        {
            _dumpLog.WriteLine(Localization.Core.Output_format_does_not_support_audio_tracks_cannot_continue);

            StoppingErrorMessage?.Invoke(Localization.Core.Output_format_does_not_support_audio_tracks_cannot_continue);

            return;
        }

        if(!outputOptical.OpticalCapabilities.HasFlag(OpticalImageCapabilities.CanStorePregaps) &&
           tracks.Where(track => track.Sequence != tracks.First(t => t.Session == track.Session).Sequence).
                  Any(track => track.Pregap     > 0))
        {
            if(!_force)
            {
                _dumpLog.WriteLine(Localization.Core.Output_format_does_not_support_pregaps_not_continuing);

                StoppingErrorMessage?.Invoke(Localization.Core.Output_format_does_not_support_pregaps_not_continuing);

                return;
            }

            _dumpLog.WriteLine(Localization.Core.Output_format_does_not_support_pregaps_continuing);

            ErrorMessage?.Invoke(Localization.Core.Output_format_does_not_support_pregaps_continuing);
        }

        for(int t = 1; t < tracks.Length; t++)
            tracks[t - 1].EndSector = tracks[t].StartSector - 1;

        tracks[^1].EndSector = (ulong)lastSector;
        blocks               = (ulong)(lastSector + 1);

        if(blocks == 0)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_dump_blank_media);

            return;
        }

        ResumeSupport.Process(true, true, blocks, _dev.Manufacturer, _dev.Model, _dev.Serial, _dev.PlatformId,
                              ref _resume, ref currentTry, ref extents, _dev.FirmwareRevision, _private, _force);

        if(currentTry == null ||
           extents    == null)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Could_not_process_resume_file_not_continuing);

            return;
        }

        // Read media tags
        ReadCdTags(ref dskType, mediaTags, out sessions, out firstTrackLastSession);

        if(!outputOptical.OpticalCapabilities.HasFlag(OpticalImageCapabilities.CanStoreSessions) &&
           sessions > 1)
        {
            // TODO: Disabled until 6.0
            /*if(!_force)
            {*/
            _dumpLog.WriteLine(Localization.Core.Output_format_does_not_support_sessions);

            StoppingErrorMessage?.Invoke(Localization.Core.Output_format_does_not_support_sessions);

            return;
            /*}

            _dumpLog.WriteLine("Output format does not support sessions, this will end in a loss of data, continuing...");

            ErrorMessage?.
                Invoke("Output format does not support sessions, this will end in a loss of data, continuing...");*/
        }

        // Check if output format supports all disc tags we have retrieved so far
        foreach(MediaTagType tag in mediaTags.Keys.Where(tag => !outputOptical.SupportedMediaTags.Contains(tag)))
            if(_force)
            {
                _dumpLog.WriteLine(Localization.Core.Output_format_does_not_support_0_continuing, tag);
                ErrorMessage?.Invoke(string.Format(Localization.Core.Output_format_does_not_support_0_continuing, tag));
            }
            else
            {
                _dumpLog.WriteLine(Localization.Core.Output_format_does_not_support_0_not_continuing, tag);

                StoppingErrorMessage?.
                    Invoke(string.Format(Localization.Core.Output_format_does_not_support_0_not_continuing, tag));

                return;
            }

        if(leadOutStarts.Any())
        {
            UpdateStatus?.Invoke(Localization.Core.Solving_lead_outs);

            foreach(KeyValuePair<int, long> leadOuts in leadOutStarts)
                foreach(Track trk in tracks.Where(trk => trk.Session   == leadOuts.Key).
                                            Where(trk => trk.EndSector >= (ulong)leadOuts.Value))
                    trk.EndSector = (ulong)leadOuts.Value - 1;

            var dataExtents = new ExtentsULong();

            foreach(Track trk in tracks)
                dataExtents.Add(trk.StartSector, trk.EndSector);

            Tuple<ulong, ulong>[] dataExtentsArray = dataExtents.ToArray();

            for(int i = 0; i < dataExtentsArray.Length - 1; i++)
                leadOutExtents.Add(dataExtentsArray[i].Item2 + 1, dataExtentsArray[i + 1].Item1 - 1);
        }

        _dumpLog.WriteLine(Localization.Core.Detecting_disc_type);
        UpdateStatus?.Invoke(Localization.Core.Detecting_disc_type);

        MMC.DetectDiscType(ref dskType, sessions, toc, _dev, out hiddenTrack, out hiddenData, firstTrackLastSession,
                           blocks);

        if(hiddenTrack || firstLba > 0)
        {
            _dumpLog.WriteLine(Localization.Core.Disc_contains_a_hidden_track);
            UpdateStatus?.Invoke(Localization.Core.Disc_contains_a_hidden_track);

            if(!outputOptical.OpticalCapabilities.HasFlag(OpticalImageCapabilities.CanStoreHiddenTracks))
            {
                StoppingErrorMessage?.Invoke(Localization.Core.Output_format_does_not_support_hidden_tracks);
                _dumpLog.WriteLine(Localization.Core.Output_format_does_not_support_hidden_tracks);

                return;
            }

            List<Track> trkList = new()
            {
                new Track
                {
                    Sequence          = (uint)(tracks.Any(t => t.Sequence == 1) ? 0 : 1),
                    Session           = 1,
                    Type              = hiddenData ? TrackType.Data : TrackType.Audio,
                    StartSector       = 0,
                    BytesPerSector    = (int)sectorSize,
                    RawBytesPerSector = (int)sectorSize,
                    SubchannelType    = subType,
                    EndSector         = tracks.First(t => t.Sequence >= 1).StartSector - 1
                }
            };

            trkList.AddRange(tracks);
            tracks = trkList.ToArray();
        }

        if(tracks.Any(t => t.Type == TrackType.Audio) &&
           desiredSubchannel != MmcSubchannel.Raw)
        {
            _dumpLog.WriteLine(Localization.Core.WARNING_If_disc_says_CDG_CDEG_CDMIDI_dump_will_be_incorrect);

            UpdateStatus?.Invoke(Localization.Core.WARNING_If_disc_says_CDG_CDEG_CDMIDI_dump_will_be_incorrect);
        }

        // Check mode for tracks
        foreach(Track trk in tracks.Where(t => t.Type != TrackType.Audio))
        {
            if(!readcd)
            {
                trk.Type = TrackType.CdMode1;

                continue;
            }

            _dumpLog.WriteLine(Localization.Core.Checking_mode_for_track_0, trk.Sequence);
            UpdateStatus?.Invoke(string.Format(Localization.Core.Checking_mode_for_track_0, trk.Sequence));

            sense = _dev.ReadCd(out cmdBuf, out _, (uint)(trk.StartSector + trk.Pregap), blockSize, 1,
                                MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true,
                                MmcErrorField.None, supportedSubchannel, _dev.Timeout, out _);

            if(sense)
            {
                _dumpLog.WriteLine(Localization.Core.Unable_to_guess_mode_for_track_0_continuing, trk.Sequence);

                UpdateStatus?.Invoke(string.Format(Localization.Core.Unable_to_guess_mode_for_track_0_continuing,
                                                   trk.Sequence));

                continue;
            }

            int bufOffset = 0;

            while(cmdBuf[0  + bufOffset] != 0x00 ||
                  cmdBuf[1  + bufOffset] != 0xFF ||
                  cmdBuf[2  + bufOffset] != 0xFF ||
                  cmdBuf[3  + bufOffset] != 0xFF ||
                  cmdBuf[4  + bufOffset] != 0xFF ||
                  cmdBuf[5  + bufOffset] != 0xFF ||
                  cmdBuf[6  + bufOffset] != 0xFF ||
                  cmdBuf[7  + bufOffset] != 0xFF ||
                  cmdBuf[8  + bufOffset] != 0xFF ||
                  cmdBuf[9  + bufOffset] != 0xFF ||
                  cmdBuf[10 + bufOffset] != 0xFF ||
                  cmdBuf[11 + bufOffset] != 0x00)
            {
                if(bufOffset + 12 >= cmdBuf.Length)
                    break;

                bufOffset++;
            }

            switch(cmdBuf[15 + bufOffset])
            {
                case 1:
                case 0x61: // Scrambled
                    UpdateStatus?.Invoke(string.Format(Localization.Core.Track_0_is_MODE1, trk.Sequence));
                    _dumpLog.WriteLine(Localization.Core.Track_0_is_MODE1, trk.Sequence);
                    trk.Type = TrackType.CdMode1;

                    break;
                case 2:
                case 0x62: // Scrambled
                    if(dskType is MediaType.CDI or MediaType.CDIREADY)
                    {
                        UpdateStatus?.Invoke(string.Format(Localization.Core.Track_0_is_MODE2, trk.Sequence));
                        _dumpLog.WriteLine(Localization.Core.Track_0_is_MODE2, trk.Sequence);
                        trk.Type = TrackType.CdMode2Formless;

                        break;
                    }

                    if((cmdBuf[0x012] & 0x20) == 0x20) // mode 2 form 2
                    {
                        UpdateStatus?.Invoke(string.Format(Localization.Core.Track_0_is_MODE2_FORM_2, trk.Sequence));
                        _dumpLog.WriteLine(Localization.Core.Track_0_is_MODE2_FORM_2, trk.Sequence);
                        trk.Type = TrackType.CdMode2Form2;

                        break;
                    }

                    UpdateStatus?.Invoke(string.Format(Localization.Core.Track_0_is_MODE2_FORM_1, trk.Sequence));
                    _dumpLog.WriteLine(Localization.Core.Track_0_is_MODE2_FORM_1, trk.Sequence);
                    trk.Type = TrackType.CdMode2Form1;

                    // These media type specifications do not legally allow mode 2 tracks to be present
                    if(dskType is MediaType.CDROM or MediaType.CDPLUS or MediaType.CDV)
                        dskType = MediaType.CD;

                    break;
                default:
                    UpdateStatus?.Invoke(string.Format(Localization.Core.Track_0_is_unknown_mode_1, trk.Sequence,
                                                       cmdBuf[15]));

                    _dumpLog.WriteLine(Localization.Core.Track_0_is_unknown_mode_1, trk.Sequence, cmdBuf[15]);

                    break;
            }
        }

        if(outputOptical.Id == new Guid("12345678-AAAA-BBBB-CCCC-123456789000"))
        {
            if(tracks.Length > 1)
            {
                StoppingErrorMessage?.Invoke(Localization.Core.
                                                          Output_format_does_not_support_more_than_1_track_not_continuing);

                _dumpLog.WriteLine(Localization.Core.Output_format_does_not_support_more_than_1_track_not_continuing);

                return;
            }

            if(tracks.Any(t => t.Type == TrackType.Audio))
            {
                StoppingErrorMessage?.Invoke(Localization.Core.
                                                          Output_format_does_not_support_audio_tracks_not_continuing);

                _dumpLog.WriteLine(Localization.Core.Output_format_does_not_support_audio_tracks_not_continuing);

                return;
            }

            if(tracks.Any(t => t.Type != TrackType.CdMode1))
            {
                StoppingErrorMessage?.Invoke(Localization.Core.
                                                          Output_format_only_supports_MODE_1_tracks_not_continuing);

                _dumpLog.WriteLine(Localization.Core.Output_format_only_supports_MODE_1_tracks_not_continuing);

                return;
            }

            supportsLongSectors = false;
        }

        // Check if something prevents from dumping the first track pregap
        if(_dumpFirstTrackPregap && readcd)
            if(!outputOptical.SupportedMediaTags.Contains(MediaTagType.CD_FirstTrackPregap))
            {
                if(_force)
                {
                    _dumpLog.WriteLine(Localization.Core.
                                                    Output_format_does_not_support_CD_first_track_pregap_continuing);

                    ErrorMessage?.Invoke(Localization.Core.
                                                      Output_format_does_not_support_CD_first_track_pregap_continuing);
                }
                else
                {
                    _dumpLog.WriteLine(Localization.Core.
                                                    Output_format_does_not_support_CD_first_track_pregap_not_continuing);

                    StoppingErrorMessage?.Invoke(Localization.Core.
                                                              Output_format_does_not_support_CD_first_track_pregap_not_continuing);

                    return;
                }

                _dumpFirstTrackPregap = false;
            }

        // Try how many blocks are readable at once
        while(true)
        {
            if(readcd)
            {
                sense = _dev.ReadCd(out cmdBuf, out _, firstLba, blockSize, _maximumReadable, MmcSectorTypes.AllTypes,
                                    false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                    supportedSubchannel, _dev.Timeout, out _);

                if(_dev.Error || sense)
                    _maximumReadable /= 2;
            }
            else if(read16)
            {
                sense = _dev.Read16(out cmdBuf, out _, 0, false, true, false, firstLba, blockSize, 0, _maximumReadable,
                                    false, _dev.Timeout, out _);

                if(_dev.Error || sense)
                    _maximumReadable /= 2;
            }
            else if(read12)
            {
                sense = _dev.Read12(out cmdBuf, out _, 0, false, true, false, false, firstLba, blockSize, 0,
                                    _maximumReadable, false, _dev.Timeout, out _);

                if(_dev.Error || sense)
                    _maximumReadable /= 2;
            }
            else if(read10)
            {
                sense = _dev.Read10(out cmdBuf, out _, 0, false, true, false, false, firstLba, blockSize, 0,
                                    (ushort)_maximumReadable, _dev.Timeout, out _);

                if(_dev.Error || sense)
                    _maximumReadable /= 2;
            }
            else if(read6)
            {
                sense = _dev.Read6(out cmdBuf, out _, firstLba, blockSize, (byte)_maximumReadable, _dev.Timeout, out _);

                if(_dev.Error || sense)
                    _maximumReadable /= 2;
            }

            if(!_dev.Error ||
               _maximumReadable == 1)
                break;
        }

        if(_dev.Error || sense)
        {
            _dumpLog.WriteLine(Localization.Core.Device_error_0_trying_to_guess_ideal_transfer_length, _dev.LastError);

            StoppingErrorMessage?.
                Invoke(string.Format(Localization.Core.Device_error_0_trying_to_guess_ideal_transfer_length,
                                     _dev.LastError));
        }

        bool cdiWithHiddenTrack1 = false;

        if(dskType is MediaType.CDIREADY &&
           tracks.Min(t => t.Sequence) == 1)
        {
            cdiWithHiddenTrack1 = true;
            dskType             = MediaType.CDI;
        }

        // Try to read the first track pregap
        if(_dumpFirstTrackPregap && readcd)
            ReadCdFirstTrackPregap(blockSize, ref currentSpeed, mediaTags, supportedSubchannel, ref totalDuration);

        _dumpLog.WriteLine(Localization.Core.Reading_0_sectors_at_a_time, _maximumReadable);
        _dumpLog.WriteLine(Localization.Core.Device_reports_0_blocks_1_bytes, blocks, blocks * blockSize);
        _dumpLog.WriteLine(Localization.Core.Device_can_read_0_blocks_at_a_time, _maximumReadable);
        _dumpLog.WriteLine(Localization.Core.Device_reports_0_bytes_per_logical_block, blockSize);
        _dumpLog.WriteLine(Localization.Core.SCSI_device_type_0, _dev.ScsiType);
        _dumpLog.WriteLine(Localization.Core.Media_identified_as_0, dskType);

        UpdateStatus?.Invoke(string.Format(Localization.Core.Reading_0_sectors_at_a_time, _maximumReadable));

        UpdateStatus?.Invoke(string.Format(Localization.Core.Device_reports_0_blocks_1_bytes, blocks,
                                           blocks * blockSize));

        UpdateStatus?.Invoke(string.Format(Localization.Core.Device_can_read_0_blocks_at_a_time, _maximumReadable));
        UpdateStatus?.Invoke(string.Format(Localization.Core.Device_reports_0_bytes_per_logical_block, blockSize));
        UpdateStatus?.Invoke(string.Format(Localization.Core.SCSI_device_type_0, _dev.ScsiType));
        UpdateStatus?.Invoke(string.Format(Localization.Core.Media_identified_as_0, dskType));

        ret = outputOptical.Create(_outputPath, dskType, _formatOptions, blocks,
                                   supportsLongSectors ? blockSize : 2048);

        // Cannot create image
        if(!ret)
        {
            _dumpLog.WriteLine(Localization.Core.Error_creating_output_image_not_continuing);
            _dumpLog.WriteLine(outputOptical.ErrorMessage);

            StoppingErrorMessage?.Invoke(Localization.Core.Error_creating_output_image_not_continuing +
                                         Environment.NewLine + outputOptical.ErrorMessage);

            return;
        }

        ErrorNumber errno = outputOptical.ReadMediaTag(MediaTagType.CD_MCN, out byte[] mcnBytes);

        if(errno == ErrorNumber.NoError)
            mcn = Encoding.ASCII.GetString(mcnBytes);

        if(outputOptical.Tracks != null)
            foreach(Track imgTrack in outputOptical.Tracks)
            {
                errno = outputOptical.ReadSectorTag(imgTrack.Sequence, SectorTagType.CdTrackIsrc, out byte[] isrcBytes);

                if(errno == ErrorNumber.NoError)
                    isrcs[(byte)imgTrack.Sequence] = Encoding.ASCII.GetString(isrcBytes);

                Track trk = tracks.FirstOrDefault(t => t.Sequence == imgTrack.Sequence);

                if(trk == null)
                    continue;

                trk.Pregap      = imgTrack.Pregap;
                trk.StartSector = imgTrack.StartSector;
                trk.EndSector   = imgTrack.EndSector;

                foreach(KeyValuePair<ushort, int> imgIdx in imgTrack.Indexes)
                    trk.Indexes[imgIdx.Key] = imgIdx.Value;
            }

        // Send track list to output plugin. This may fail if subchannel is set but unsupported.
        ret = outputOptical.SetTracks(tracks.ToList());

        if(!ret &&
           desiredSubchannel == MmcSubchannel.None)
        {
            _dumpLog.WriteLine(Localization.Core.Error_sending_tracks_to_output_image_not_continuing);
            _dumpLog.WriteLine(outputOptical.ErrorMessage);

            StoppingErrorMessage?.Invoke(Localization.Core.Error_sending_tracks_to_output_image_not_continuing +
                                         Environment.NewLine + outputOptical.ErrorMessage);

            return;
        }

        // If a subchannel is supported, check if output plugin allows us to write it.
        if(desiredSubchannel != MmcSubchannel.None &&
           !outputOptical.OpticalCapabilities.HasFlag(OpticalImageCapabilities.CanStoreSubchannelRw))
        {
            if(_force)
            {
                _dumpLog.WriteLine(Localization.Core.Output_image_does_not_support_subchannels_continuing);
                ErrorMessage?.Invoke(Localization.Core.Output_image_does_not_support_subchannels_continuing);
            }
            else
            {
                _dumpLog.WriteLine(Localization.Core.Output_image_does_not_support_subchannels_not_continuing);

                StoppingErrorMessage?.Invoke(Localization.Core.
                                                          Output_image_does_not_support_subchannels_not_continuing);

                return;
            }
        }

        if(supportedSubchannel != MmcSubchannel.None)
        {
            _dumpLog.WriteLine(string.Format(Localization.Core.Creating_subchannel_log_in_0,
                                             _outputPrefix + ".sub.log"));

            subLog = new SubchannelLog(_outputPrefix + ".sub.log", bcdSubchannel);
        }

        // Set track flags
        foreach(KeyValuePair<byte, byte> kvp in trackFlags)
        {
            Track track = tracks.FirstOrDefault(t => t.Sequence == kvp.Key);

            if(track is null)
                continue;

            _dumpLog.WriteLine(Localization.Core.Setting_flags_for_track_0, track.Sequence);
            UpdateStatus?.Invoke(string.Format(Localization.Core.Setting_flags_for_track_0, track.Sequence));

            outputOptical.WriteSectorTag(new[]
            {
                kvp.Value
            }, kvp.Key, SectorTagType.CdTrackFlags);
        }

        // Set MCN
        if(supportedSubchannel == MmcSubchannel.None)
        {
            sense = _dev.ReadMcn(out mcn, out _, out _, _dev.Timeout, out _);

            if(!sense      &&
               mcn != null &&
               mcn != "0000000000000")
            {
                UpdateStatus?.Invoke(string.Format(Localization.Core.Found_Media_Catalogue_Number_0, mcn));
                _dumpLog.WriteLine(Localization.Core.Found_Media_Catalogue_Number_0, mcn);
            }
            else
                mcn = null;
        }

        // Set ISRCs
        if(supportedSubchannel == MmcSubchannel.None)
            foreach(Track trk in tracks)
            {
                sense = _dev.ReadIsrc((byte)trk.Sequence, out string isrc, out _, out _, _dev.Timeout, out _);

                if(sense || isrc is null or "000000000000")
                    continue;

                isrcs[(byte)trk.Sequence] = isrc;

                UpdateStatus?.Invoke(string.Format(Localization.Core.Found_ISRC_for_track_0_1, trk.Sequence, isrc));
                _dumpLog.WriteLine(string.Format(Localization.Core.Found_ISRC_for_track_0_1, trk.Sequence, isrc));
            }

        if(supportedSubchannel != MmcSubchannel.None &&
           desiredSubchannel   != MmcSubchannel.None)
        {
            subchannelExtents = new HashSet<int>();

            _resume.BadSubchannels ??= new List<int>();

            foreach(int sub in _resume.BadSubchannels)
                subchannelExtents.Add(sub);

            if(_resume.NextBlock < blocks)
                for(ulong i = _resume.NextBlock; i < blocks; i++)
                    subchannelExtents.Add((int)i);
        }

        if(_resume.NextBlock > 0)
        {
            UpdateStatus?.Invoke(string.Format(Localization.Core.Resuming_from_block_0, _resume.NextBlock));
            _dumpLog.WriteLine(Localization.Core.Resuming_from_block_0, _resume.NextBlock);
        }

        if(_skip < _maximumReadable)
            _skip = _maximumReadable;

    #if DEBUG
        foreach(Track trk in tracks)
            UpdateStatus?.Invoke(string.Format(Localization.Core.Track_0_starts_at_LBA_1_and_ends_at_LBA_2,
                                               trk.Sequence, trk.StartSector, trk.EndSector));
    #endif

        // Check offset
        if(_fixOffset)
        {
            if(tracks.All(t => t.Type != TrackType.Audio))
            {
                // No audio tracks so no need to fix offset
                _dumpLog.WriteLine(Localization.Core.No_audio_tracks_disabling_offset_fix);
                UpdateStatus.Invoke(Localization.Core.No_audio_tracks_disabling_offset_fix);

                _fixOffset = false;
            }

            if(!readcd)
            {
                _dumpLog.WriteLine(Localization.Core.
                                                READ_CD_command_is_not_supported_disabling_offset_fix_Dump_may_not_be_correct);

                UpdateStatus?.Invoke(Localization.Core.
                                                  READ_CD_command_is_not_supported_disabling_offset_fix_Dump_may_not_be_correct);

                _fixOffset = false;
            }
        }
        else if(tracks.Any(t => t.Type == TrackType.Audio))
        {
            _dumpLog.WriteLine(Localization.Core.
                                            There_are_audio_tracks_and_offset_fixing_is_disabled_dump_may_not_be_correct);

            UpdateStatus?.Invoke(Localization.Core.
                                              There_are_audio_tracks_and_offset_fixing_is_disabled_dump_may_not_be_correct);
        }

        // Search for read offset in main database
        cdOffset =
            _ctx.CdOffsets.FirstOrDefault(d => (d.Manufacturer == _dev.Manufacturer ||
                                                d.Manufacturer == _dev.Manufacturer.Replace('/', '-')) &&
                                               (d.Model == _dev.Model || d.Model == _dev.Model.Replace('/', '-')));

        Media.Info.CompactDisc.GetOffset(cdOffset, _dbDev, _debug, _dev, dskType, _dumpLog, tracks, UpdateStatus,
                                         out int? driveOffset, out int? combinedOffset, out _supportsPlextorD8);

        if(combinedOffset is null)
        {
            if(driveOffset is null)
            {
                _dumpLog.WriteLine(Localization.Core.Drive_reading_offset_not_found_in_database);
                UpdateStatus?.Invoke(Localization.Core.Drive_reading_offset_not_found_in_database);
                _dumpLog.WriteLine(Localization.Core.Disc_offset_cannot_be_calculated);
                UpdateStatus?.Invoke(Localization.Core.Disc_offset_cannot_be_calculated);

                if(tracks.Any(t => t.Type == TrackType.Audio))
                {
                    _dumpLog.WriteLine(Localization.Core.Dump_may_not_be_correct);

                    UpdateStatus?.Invoke(Localization.Core.Dump_may_not_be_correct);
                }

                if(_fixOffset)
                    _fixOffset = false;
            }
            else
            {
                _dumpLog.WriteLine(string.Format(Localization.Core.Drive_reading_offset_is_0_bytes_1_samples,
                                                 driveOffset, driveOffset / 4));

                UpdateStatus?.Invoke(string.Format(Localization.Core.Drive_reading_offset_is_0_bytes_1_samples,
                                                   driveOffset, driveOffset / 4));

                _dumpLog.WriteLine(Localization.Core.Disc_write_offset_is_unknown_dump_may_not_be_correct);
                UpdateStatus?.Invoke(Localization.Core.Disc_write_offset_is_unknown_dump_may_not_be_correct);

                offsetBytes = driveOffset.Value;

                sectorsForOffset = offsetBytes / (int)sectorSize;

                if(sectorsForOffset < 0)
                    sectorsForOffset *= -1;

                if(offsetBytes % sectorSize != 0)
                    sectorsForOffset++;
            }
        }
        else
        {
            offsetBytes      = combinedOffset.Value;
            sectorsForOffset = offsetBytes / (int)sectorSize;

            if(sectorsForOffset < 0)
                sectorsForOffset *= -1;

            if(offsetBytes % sectorSize != 0)
                sectorsForOffset++;

            if(driveOffset is null)
            {
                _dumpLog.WriteLine(Localization.Core.Drive_reading_offset_not_found_in_database);
                UpdateStatus?.Invoke(Localization.Core.Drive_reading_offset_not_found_in_database);

                _dumpLog.WriteLine(string.Format(Localization.Core.Combined_disc_and_drive_offset_are_0_bytes_1_samples,
                                                 offsetBytes, offsetBytes / 4));

                UpdateStatus?.
                    Invoke(string.Format(Localization.Core.Combined_disc_and_drive_offset_are_0_bytes_1_samples,
                                         offsetBytes, offsetBytes / 4));
            }
            else
            {
                _dumpLog.WriteLine(string.Format(Localization.Core.Drive_reading_offset_is_0_bytes_1_samples,
                                                 driveOffset, driveOffset / 4));

                UpdateStatus?.Invoke(string.Format(Localization.Core.Drive_reading_offset_is_0_bytes_1_samples,
                                                   driveOffset, driveOffset / 4));

                discOffset = offsetBytes - driveOffset;

                _dumpLog.WriteLine(string.Format(Localization.Core.Disc_offset_is_0_bytes_1_samples, discOffset,
                                                 discOffset / 4));

                UpdateStatus?.Invoke(string.Format(Localization.Core.Disc_offset_is_0_bytes_1_samples, discOffset,
                                                   discOffset / 4));
            }
        }

        if(!_fixOffset ||
           tracks.All(t => t.Type != TrackType.Audio))
        {
            offsetBytes      = 0;
            sectorsForOffset = 0;
        }

        mhddLog = new MhddLog(_outputPrefix + ".mhddlog.bin", _dev, blocks, blockSize, _maximumReadable, _private,
                              _dimensions);

        ibgLog = new IbgLog(_outputPrefix + ".ibg", 0x0008);

        if(_createGraph)
        {
            Spiral.DiscParameters discSpiralParameters = Spiral.DiscParametersFromMediaType(dskType);

            if(discSpiralParameters is not null)
                _mediaGraph = new Spiral((int)_dimensions, (int)_dimensions, discSpiralParameters, blocks);
            else
                _mediaGraph = new BlockMap((int)_dimensions, (int)_dimensions, blocks);

            if(_mediaGraph is not null)
                foreach(Tuple<ulong, ulong> e in extents.ToArray())
                    _mediaGraph?.PaintSectorsGood(e.Item1, (uint)(e.Item2 - e.Item1 + 2));

            _mediaGraph?.PaintSectorsBad(_resume.BadBlocks);
        }

        audioExtents = new ExtentsULong();

        foreach(Track audioTrack in tracks.Where(t => t.Type == TrackType.Audio))
            audioExtents.Add(audioTrack.StartSector, audioTrack.EndSector);

        // Set speed
        if(_speedMultiplier >= 0)
        {
            _dumpLog.WriteLine(_speed == 0xFFFF ? Localization.Core.Setting_speed_to_MAX_for_data_reading
                                   : string.Format(Localization.Core.Setting_speed_to_0_x_for_data_reading, _speed));

            UpdateStatus?.Invoke(_speed == 0xFFFF ? Localization.Core.Setting_speed_to_MAX_for_data_reading
                                     : string.Format(Localization.Core.Setting_speed_to_0_x_for_data_reading, _speed));

            _speed *= _speedMultiplier;

            if(_speed is 0 or > 0xFFFF)
                _speed = 0xFFFF;

            _dev.SetCdSpeed(out _, RotationalControl.ClvAndImpureCav, (ushort)_speed, 0, _dev.Timeout, out _);
        }

        // Start reading
        start = DateTime.UtcNow;

        if(dskType == MediaType.CDIREADY || cdiWithHiddenTrack1)
        {
            Track track0 = tracks.FirstOrDefault(t => t.Sequence is 0 or 1);

            track0.Type = TrackType.CdMode2Formless;

            if(!supportsLongSectors)
            {
                _dumpLog.WriteLine(Localization.Core.
                                                Dumping_CD_i_Ready_requires_the_output_image_format_to_support_long_sectors);

                StoppingErrorMessage?.Invoke(Localization.Core.
                                                          Dumping_CD_i_Ready_requires_the_output_image_format_to_support_long_sectors);

                return;
            }

            if(!readcd)
            {
                _dumpLog.WriteLine(Localization.Core.
                                                Dumping_CD_i_Ready_requires_the_drive_to_support_the_READ_CD_command);

                StoppingErrorMessage?.Invoke(Localization.Core.
                                                          Dumping_CD_i_Ready_requires_the_drive_to_support_the_READ_CD_command);

                return;
            }

            _dev.ReadCd(out cmdBuf, out _, 0, 2352, 1, MmcSectorTypes.AllTypes, false, false, true,
                        MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.None, _dev.Timeout,
                        out _);

            hiddenData = IsData(cmdBuf);

            if(!hiddenData)
            {
                cdiReadyReadAsAudio = IsScrambledData(cmdBuf, 0, out combinedOffset);

                if(cdiReadyReadAsAudio)
                {
                    offsetBytes      = combinedOffset.Value;
                    sectorsForOffset = offsetBytes / (int)sectorSize;

                    if(sectorsForOffset < 0)
                        sectorsForOffset *= -1;

                    if(offsetBytes % sectorSize != 0)
                        sectorsForOffset++;

                    _dumpLog.WriteLine(Localization.Core.
                                                    Enabling_skipping_CD_i_Ready_hole_because_drive_returns_data_as_audio);

                    UpdateStatus?.Invoke(Localization.Core.
                                                      Enabling_skipping_CD_i_Ready_hole_because_drive_returns_data_as_audio);

                    _skipCdireadyHole = true;

                    if(driveOffset is null)
                    {
                        _dumpLog.WriteLine(Localization.Core.Drive_reading_offset_not_found_in_database);
                        UpdateStatus?.Invoke(Localization.Core.Drive_reading_offset_not_found_in_database);

                        _dumpLog.
                            WriteLine(string.
                                          Format(Localization.Core.Combined_disc_and_drive_offset_are_0_bytes_1_samples,
                                                 offsetBytes, offsetBytes / 4));

                        UpdateStatus?.
                            Invoke(string.Format(Localization.Core.Combined_disc_and_drive_offset_are_0_bytes_1_samples,
                                                 offsetBytes, offsetBytes / 4));
                    }
                    else
                    {
                        _dumpLog.WriteLine(string.Format(Localization.Core.Drive_reading_offset_is_0_bytes_1_samples,
                                                         driveOffset, driveOffset / 4));

                        UpdateStatus?.Invoke(string.Format(Localization.Core.Drive_reading_offset_is_0_bytes_1_samples,
                                                           driveOffset, driveOffset / 4));

                        discOffset = offsetBytes - driveOffset;

                        _dumpLog.WriteLine(string.Format(Localization.Core.Disc_offset_is_0_bytes_1_samples, discOffset,
                                                         discOffset / 4));

                        UpdateStatus?.Invoke(string.Format(Localization.Core.Disc_offset_is_0_bytes_1_samples,
                                                           discOffset, discOffset / 4));
                    }
                }
            }

            if(!_skipCdireadyHole)
            {
                _dumpLog.WriteLine(Localization.Core.
                                                There_will_be_thousand_of_errors_between_track_0_and_track_1_that_is_normal_and_you_can_ignore_them);

                UpdateStatus?.Invoke(Localization.Core.
                                                  There_will_be_thousand_of_errors_between_track_0_and_track_1_that_is_normal_and_you_can_ignore_them);
            }

            if(_skipCdireadyHole)
                ReadCdiReady(blockSize, ref currentSpeed, currentTry, extents, ibgLog, ref imageWriteDuration,
                             leadOutExtents, ref maxSpeed, mhddLog, ref minSpeed, subSize, supportedSubchannel,
                             ref totalDuration, tracks, subLog, desiredSubchannel, isrcs, ref mcn, subchannelExtents,
                             blocks, cdiReadyReadAsAudio, offsetBytes, sectorsForOffset, smallestPregapLbaPerTrack);
        }

        ReadCdData(audioExtents, blocks, blockSize, ref currentSpeed, currentTry, extents, ibgLog,
                   ref imageWriteDuration, lastSector, leadOutExtents, ref maxSpeed, mhddLog, ref minSpeed, out newTrim,
                   tracks[0].Type != TrackType.Audio, offsetBytes, read6, read10, read12, read16, readcd,
                   sectorsForOffset, subSize, supportedSubchannel, supportsLongSectors, ref totalDuration, tracks,
                   subLog, desiredSubchannel, isrcs, ref mcn, subchannelExtents, smallestPregapLbaPerTrack);

        // TODO: Enable when underlying images support lead-outs
        /*
        DumpCdLeadOuts(blocks, blockSize, ref currentSpeed, currentTry, extents, ibgLog, ref imageWriteDuration,
                       leadOutExtents, ref maxSpeed, mhddLog, ref minSpeed, read6, read10, read12, read16, readcd,
                       supportedSubchannel, subSize, ref totalDuration, subLog, desiredSubchannel, isrcs, ref mcn, tracks,
                       smallestPregapLbaPerTrack);
        */

        end = DateTime.UtcNow;
        mhddLog.Close();

        ibgLog.Close(_dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024,
                     blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000), _devicePath);

        UpdateStatus?.Invoke(string.Format(Localization.Core.Dump_finished_in_0_seconds, (end - start).TotalSeconds));

        UpdateStatus?.Invoke(string.Format(Localization.Core.Average_dump_speed_0_KiB_sec,
                                           blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000)));

        UpdateStatus?.Invoke(string.Format(Localization.Core.Average_write_speed_0_KiB_sec,
                                           blockSize * (double)(blocks + 1) / 1024 / imageWriteDuration));

        _dumpLog.WriteLine(Localization.Core.Dump_finished_in_0_seconds, (end - start).TotalSeconds);

        _dumpLog.WriteLine(Localization.Core.Average_dump_speed_0_KiB_sec,
                           blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000));

        _dumpLog.WriteLine(Localization.Core.Average_write_speed_0_KiB_sec,
                           blockSize * (double)(blocks + 1) / 1024 / imageWriteDuration);

        TrimCdUserData(audioExtents, blockSize, currentTry, extents, newTrim, offsetBytes, read6, read10, read12,
                       read16, readcd, sectorsForOffset, subSize, supportedSubchannel, supportsLongSectors,
                       ref totalDuration, subLog, desiredSubchannel, tracks, isrcs, ref mcn, subchannelExtents,
                       smallestPregapLbaPerTrack);

        if(dskType is MediaType.CDR or MediaType.CDRW &&
           _resume.BadBlocks.Count > 0                &&
           _ignoreCdrRunOuts       > 0)
            HandleCdrRunOutSectors(blocks, desiredSubchannel, extents, subchannelExtents, subLog, supportsLongSectors,
                                   trackFlags, tracks);

        RetryCdUserData(audioExtents, blockSize, currentTry, extents, offsetBytes, readcd, sectorsForOffset, subSize,
                        supportedSubchannel, ref totalDuration, subLog, desiredSubchannel, tracks, isrcs, ref mcn,
                        subchannelExtents, smallestPregapLbaPerTrack, supportsLongSectors);

        foreach(Tuple<ulong, ulong> leadoutExtent in leadOutExtents.ToArray())
            for(ulong e = leadoutExtent.Item1; e <= leadoutExtent.Item2; e++)
                subchannelExtents.Remove((int)e);

        if(subchannelExtents.Count > 0 &&
           _retryPasses            > 0 &&
           _retrySubchannel)
            RetrySubchannel(readcd, subSize, supportedSubchannel, ref totalDuration, subLog, desiredSubchannel, tracks,
                            isrcs, ref mcn, subchannelExtents, smallestPregapLbaPerTrack);

        // Write media tags to image
        if(!_aborted)
            foreach(KeyValuePair<MediaTagType, byte[]> tag in mediaTags)
            {
                if(tag.Value is null)
                {
                    AaruConsole.ErrorWriteLine(Localization.Core.Error_Tag_type_0_is_null_skipping, tag.Key);

                    continue;
                }

                ret = outputOptical.WriteMediaTag(tag.Value, tag.Key);

                if(ret || _force)
                    continue;

                // Cannot write tag to image
                _dumpLog.WriteLine(string.Format(Localization.Core.Cannot_write_tag_0, tag.Key));
                StoppingErrorMessage?.Invoke(outputOptical.ErrorMessage);

                return;
            }

        _resume.BadBlocks.Sort();

        foreach(ulong bad in _resume.BadBlocks)
            _dumpLog.WriteLine(Localization.Core.Sector_0_could_not_be_read, bad);

        currentTry.Extents = ExtentsConverter.ToMetadata(extents);

        _resume.BadSubchannels = new List<int>();
        _resume.BadSubchannels.AddRange(subchannelExtents);
        _resume.BadSubchannels.Sort();

        if(_generateSubchannels                                                         &&
           outputOptical.SupportedSectorTags.Contains(SectorTagType.CdSectorSubchannel) &&
           !_aborted)
            Media.CompactDisc.GenerateSubchannels(subchannelExtents, tracks, trackFlags, blocks, subLog, _dumpLog,
                                                  InitProgress, UpdateProgress, EndProgress, outputOptical);

        // TODO: Disc ID
        var metadata = new CommonTypes.Structs.ImageInfo
        {
            Application        = "Aaru",
            ApplicationVersion = Version.GetVersion()
        };

        if(!outputOptical.SetMetadata(metadata))
            ErrorMessage?.Invoke(Localization.Core.Error_0_setting_metadata + Environment.NewLine +
                                 outputOptical.ErrorMessage);

        outputOptical.SetDumpHardware(_resume.Tries);

        if(_preSidecar != null)
            outputOptical.SetCicmMetadata(_preSidecar);

        foreach(KeyValuePair<byte, string> isrc in isrcs)
        {
            // TODO: Track tags
            if(!outputOptical.WriteSectorTag(Encoding.ASCII.GetBytes(isrc.Value), isrc.Key, SectorTagType.CdTrackIsrc))
                continue;

            UpdateStatus?.Invoke(string.Format(Localization.Core.Setting_ISRC_for_track_0_to_1, isrc.Key, isrc.Value));
            _dumpLog.WriteLine(Localization.Core.Setting_ISRC_for_track_0_to_1, isrc.Key, isrc.Value);
        }

        if(mcn != null &&
           outputOptical.WriteMediaTag(Encoding.ASCII.GetBytes(mcn), MediaTagType.CD_MCN))
        {
            UpdateStatus?.Invoke(string.Format(Localization.Core.Setting_disc_Media_Catalogue_Number_to_0, mcn));
            _dumpLog.WriteLine(Localization.Core.Setting_disc_Media_Catalogue_Number_to_0, mcn);
        }

        foreach(Track trk in tracks)
        {
            // Fix track starts in each session's first track
            if(tracks.Where(t => t.Session == trk.Session).MinBy(t => t.Sequence).Sequence == trk.Sequence)
            {
                if(trk.Sequence == 1)
                    continue;

                trk.StartSector -= trk.Pregap;
                trk.Indexes[0]  =  (int)trk.StartSector;

                continue;
            }

            if(trk.Indexes.TryGetValue(0, out int idx0) &&
               trk.Indexes.TryGetValue(1, out int idx1) &&
               idx0 == idx1)
                trk.Indexes.Remove(0);
        }

        outputOptical.SetTracks(tracks.ToList());

        _dumpLog.WriteLine(Localization.Core.Closing_output_file);
        UpdateStatus?.Invoke(Localization.Core.Closing_output_file);
        DateTime closeStart = DateTime.Now;
        outputOptical.Close();
        DateTime closeEnd = DateTime.Now;

        UpdateStatus?.Invoke(string.Format(Localization.Core.Closed_in_0_seconds,
                                           (closeEnd - closeStart).TotalSeconds));

        subLog?.Close();

        if(_aborted)
        {
            _dumpLog.WriteLine(Localization.Core.Aborted);

            return;
        }

        double totalChkDuration = 0;

        if(_metadata)
            WriteOpticalSidecar(blockSize, blocks, dskType, null, mediaTags, sessions, out totalChkDuration,
                                discOffset);

        end = DateTime.UtcNow;
        UpdateStatus?.Invoke("");

        UpdateStatus?.
            Invoke(string.Format(Localization.Core.Took_a_total_of_0_seconds_1_processing_commands_2_checksumming_3_writing_4_closing,
                                 (end - dumpStart).TotalSeconds, totalDuration / 1000, totalChkDuration / 1000,
                                 imageWriteDuration, (closeEnd - closeStart).TotalSeconds));

        UpdateStatus?.Invoke(string.Format(Localization.Core.Average_speed_0_MiB_sec,
                                           blockSize * (double)(blocks + 1) / 1048576 / (totalDuration / 1000)));

        if(maxSpeed > 0)
            UpdateStatus?.Invoke(string.Format(Localization.Core.Fastest_speed_burst_0_MiB_sec, maxSpeed));

        if(minSpeed is > 0 and < double.MaxValue)
            UpdateStatus?.Invoke(string.Format(Localization.Core.Slowest_speed_burst_0_MiB_sec, minSpeed));

        UpdateStatus?.Invoke(string.Format(Localization.Core._0_sectors_could_not_be_read, _resume.BadBlocks.Count));

        UpdateStatus?.Invoke(string.Format(Localization.Core._0_subchannels_could_not_be_read,
                                           _resume.BadSubchannels.Count));

        UpdateStatus?.Invoke("");

        Statistics.AddMedia(dskType, true);
    }
}