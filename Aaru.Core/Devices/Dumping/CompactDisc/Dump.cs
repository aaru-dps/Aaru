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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

namespace Aaru.Core.Devices.Dumping;

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
using Aaru.Core.Logging;
using Aaru.Core.Media.Detection;
using Aaru.Database.Models;
using Aaru.Decoders.CD;
using Aaru.Devices;
using Schemas;
using TrackType = Aaru.CommonTypes.Enums.TrackType;
using Version = Aaru.CommonTypes.Interop.Version;

/// <summary>Implement dumping Compact Discs</summary>

// TODO: Barcode
sealed partial class Dump
{
    /// <summary>Dumps a compact disc</summary>
    void CompactDisc()
    {
        ExtentsULong             audioExtents;                        // Extents with audio sectors
        ulong                    blocks;                              // Total number of positive sectors
        uint                     blockSize;                           // Size of the read sector in bytes
        CdOffset                 cdOffset;                            // Read offset from database
        byte[]                   cmdBuf;                              // Data buffer
        DumpHardwareType         currentTry   = null;                 // Current dump hardware try
        double                   currentSpeed = 0;                    // Current read speed
        int?                     discOffset   = null;                 // Disc write offset
        DateTime                 dumpStart    = DateTime.UtcNow;      // Time of dump start
        DateTime                 end;                                 // Time of operation end
        ExtentsULong             extents = null;                      // Extents
        bool                     hiddenData;                          // Hidden track is data
        IbgLog                   ibgLog;                              // IMGBurn log
        double                   imageWriteDuration = 0;              // Duration of image write
        long                     lastSector;                          // Last sector number
        var                      leadOutExtents = new ExtentsULong(); // Lead-out extents
        Dictionary<int, long>    leadOutStarts  = new();              // Lead-out starts
        double                   maxSpeed       = double.MinValue;    // Maximum speed
        MhddLog                  mhddLog;                             // MHDD log
        double                   minSpeed = double.MaxValue;          // Minimum speed
        bool                     newTrim;                             // Is trim a new one?
        var                      offsetBytes = 0;                     // Read offset
        var                      read6       = false;                 // Device supports READ(6)
        var                      read10      = false;                 // Device supports READ(10)
        var                      read12      = false;                 // Device supports READ(12)
        var                      read16      = false;                 // Device supports READ(16)
        bool                     readcd;                              // Device supports READ CD
        bool                     ret;                                 // Image writing return status
        const uint               sectorSize       = 2352;             // Full sector size
        var                      sectorsForOffset = 0;                // Sectors needed to fix offset
        var                      sense            = true;             // Sense indicator
        int                      sessions;                            // Number of sessions in disc
        DateTime                 start;                               // Start of operation
        SubchannelLog            subLog = null;                       // Subchannel log
        uint                     subSize;                             // Subchannel size in bytes
        TrackSubchannelType      subType;                             // Track subchannel type
        var                      supportsLongSectors = true;          // Supports reading EDC and ECC
        bool                     supportsPqSubchannel;                // Supports reading PQ subchannel
        bool                     supportsRwSubchannel;                // Supports reading RW subchannel
        byte[]                   tmpBuf;                              // Temporary buffer
        FullTOC.CDFullTOC?       toc;                                 // Full CD TOC
        double                   totalDuration = 0;                   // Total commands duration
        Dictionary<byte, byte>   trackFlags    = new();               // Track flags
        Track[]                  tracks;                              // Tracks in disc
        int                      firstTrackLastSession;               // Number of first track in last session
        bool                     hiddenTrack;                         // Disc has a hidden track before track 1
        MmcSubchannel            supportedSubchannel;                 // Drive's maximum supported subchannel
        MmcSubchannel            desiredSubchannel;                   // User requested subchannel
        var                      bcdSubchannel       = false;         // Subchannel positioning is in BCD
        Dictionary<byte, string> isrcs               = new();
        string                   mcn                 = null;
        HashSet<int>             subchannelExtents   = new();
        var                      cdiReadyReadAsAudio = false;
        uint                     firstLba;
        var                      outputOptical = _outputPlugin as IWritableOpticalImage;

        Dictionary<MediaTagType, byte[]> mediaTags                 = new(); // Media tags
        Dictionary<byte, int>            smallestPregapLbaPerTrack = new();

        MediaType dskType = MediaType.CD;

        if(_dumpRaw)
        {
            _dumpLog.WriteLine("Raw CD dumping not yet implemented");
            StoppingErrorMessage?.Invoke("Raw CD dumping not yet implemented");

            return;
        }

        tracks = GetCdTracks(_dev, _dumpLog, _force, out lastSector, leadOutStarts, mediaTags, StoppingErrorMessage,
                             out toc, trackFlags, UpdateStatus);

        if(tracks is null)
        {
            _dumpLog.WriteLine("Could not get tracks!");
            StoppingErrorMessage?.Invoke("Could not get tracks!");

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
                    _dumpLog.WriteLine("Drive does not support the requested subchannel format, not continuing...");

                    StoppingErrorMessage?.
                        Invoke("Drive does not support the requested subchannel format, not continuing...");

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
                    _dumpLog.WriteLine("Drive does not support the requested subchannel format, not continuing...");

                    StoppingErrorMessage?.
                        Invoke("Drive does not support the requested subchannel format, not continuing...");

                    return;
                }

                break;
            case DumpSubchannel.Pq:
                if(supportsPqSubchannel)
                    desiredSubchannel = MmcSubchannel.Q16;
                else
                {
                    _dumpLog.WriteLine("Drive does not support the requested subchannel format, not continuing...");

                    StoppingErrorMessage?.
                        Invoke("Drive does not support the requested subchannel format, not continuing...");

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
                _dumpLog.WriteLine("Output format does not support subchannels, continuing...");
                UpdateStatus?.Invoke("Output format does not support subchannels, continuing...");
            }
            else
            {
                _dumpLog.WriteLine("Output format does not support subchannels, not continuing...");
                StoppingErrorMessage?.Invoke("Output format does not support subchannels, not continuing...");

                return;
            }

            desiredSubchannel = MmcSubchannel.None;
        }

        switch(supportedSubchannel)
        {
            case MmcSubchannel.None:
                _dumpLog.WriteLine("Checking if drive supports reading without subchannel...");
                UpdateStatus?.Invoke("Checking if drive supports reading without subchannel...");

                readcd = !_dev.ReadCd(out cmdBuf, out _, firstLba, sectorSize, 1, MmcSectorTypes.AllTypes, false, false,
                                      true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                      supportedSubchannel, _dev.Timeout, out _);

                if(!readcd)
                {
                    _dumpLog.WriteLine("Drive does not support READ CD, trying SCSI READ commands...");
                    ErrorMessage?.Invoke("Drive does not support READ CD, trying SCSI READ commands...");

                    _dumpLog.WriteLine("Checking if drive supports READ(6)...");
                    UpdateStatus?.Invoke("Checking if drive supports READ(6)...");
                    read6 = !_dev.Read6(out cmdBuf, out _, firstLba, 2048, 1, _dev.Timeout, out _);
                    _dumpLog.WriteLine("Checking if drive supports READ(10)...");
                    UpdateStatus?.Invoke("Checking if drive supports READ(10)...");

                    read10 = !_dev.Read10(out cmdBuf, out _, 0, false, true, false, false, firstLba, 2048, 0, 1,
                                          _dev.Timeout, out _);

                    _dumpLog.WriteLine("Checking if drive supports READ(12)...");
                    UpdateStatus?.Invoke("Checking if drive supports READ(12)...");

                    read12 = !_dev.Read12(out cmdBuf, out _, 0, false, true, false, false, firstLba, 2048, 0, 1, false,
                                          _dev.Timeout, out _);

                    _dumpLog.WriteLine("Checking if drive supports READ(16)...");
                    UpdateStatus?.Invoke("Checking if drive supports READ(16)...");

                    read16 = !_dev.Read16(out cmdBuf, out _, 0, false, true, false, firstLba, 2048, 0, 1, false,
                                          _dev.Timeout, out _);

                    if(!read6  &&
                       !read10 &&
                       !read12 &&
                       !read16)
                    {
                        _dumpLog.WriteLine("Cannot read from disc, not continuing...");
                        StoppingErrorMessage?.Invoke("Cannot read from disc, not continuing...");

                        return;
                    }

                    if(read6)
                    {
                        _dumpLog.WriteLine("Drive supports READ(6)...");
                        UpdateStatus?.Invoke("Drive supports READ(6)...");
                    }

                    if(read10)
                    {
                        _dumpLog.WriteLine("Drive supports READ(10)...");
                        UpdateStatus?.Invoke("Drive supports READ(10)...");
                    }

                    if(read12)
                    {
                        _dumpLog.WriteLine("Drive supports READ(12)...");
                        UpdateStatus?.Invoke("Drive supports READ(12)...");
                    }

                    if(read16)
                    {
                        _dumpLog.WriteLine("Drive supports READ(16)...");
                        UpdateStatus?.Invoke("Drive supports READ(16)...");
                    }
                }

                _dumpLog.WriteLine("Drive can read without subchannel...");
                UpdateStatus?.Invoke("Drive can read without subchannel...");

                subSize = 0;
                subType = TrackSubchannelType.None;

                break;
            case MmcSubchannel.Raw:
                _dumpLog.WriteLine("Full raw subchannel reading supported...");
                UpdateStatus?.Invoke("Full raw subchannel reading supported...");
                subType = TrackSubchannelType.Raw;
                subSize = 96;
                readcd  = true;

                break;
            case MmcSubchannel.Q16:
                _dumpLog.WriteLine("PQ subchannel reading supported...");
                _dumpLog.WriteLine("WARNING: If disc says CD+G, CD+EG, CD-MIDI, CD Graphics or CD Enhanced Graphics, dump will be incorrect!");
                UpdateStatus?.Invoke("PQ subchannel reading supported...");

                UpdateStatus?.
                    Invoke("WARNING: If disc says CD+G, CD+EG, CD-MIDI, CD Graphics or CD Enhanced Graphics, dump will be incorrect!");

                subType = TrackSubchannelType.Q16;
                subSize = 16;
                readcd  = true;

                break;
            default:
                _dumpLog.WriteLine("Handling subchannel type {0} not supported, exiting...", supportedSubchannel);

                StoppingErrorMessage?.
                    Invoke($"Handling subchannel type {supportedSubchannel} not supported, exiting...");

                return;
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
            sense = _dev.ReadCd(out cmdBuf, out _, (firstLba / 75 + 1) * 75 + 35, blockSize, 1, MmcSectorTypes.AllTypes,
                                false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                supportedSubchannel, _dev.Timeout, out _);

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
                    _dumpLog.WriteLine("Drive returns subchannel in BCD...");
                    UpdateStatus?.Invoke("Drive returns subchannel in BCD...");
                }
                else
                {
                    _dumpLog.WriteLine("Drive does not returns subchannel in BCD...");
                    UpdateStatus?.Invoke("Drive does not returns subchannel in BCD...");
                }
            }
        }

        foreach(Track trk in tracks)
            trk.SubchannelType = subType;

        _dumpLog.WriteLine("Calculating pregaps, can take some time...");
        UpdateStatus?.Invoke("Calculating pregaps, can take some time...");

        SolveTrackPregaps(_dev, _dumpLog, UpdateStatus, tracks, supportsPqSubchannel, supportsRwSubchannel, _dbDev,
                          out bool inexactPositioning, true);

        if(inexactPositioning)
        {
            _dumpLog.WriteLine("WARNING: The drive has returned incorrect Q positioning when calculating pregaps. A best effort has been tried but they may be incorrect.");

            UpdateStatus?.
                Invoke("WARNING: The drive has returned incorrect Q positioning when calculating pregaps. A best effort has been tried but they may be incorrect.");
        }

        if(!outputOptical.OpticalCapabilities.HasFlag(OpticalImageCapabilities.CanStoreRawData))
        {
            if(!_force)
            {
                _dumpLog.WriteLine("Output format does not support storing raw data, this may end in a loss of data, not continuing...");

                StoppingErrorMessage?.
                    Invoke("Output format does not support storing raw data, this may end in a loss of data, not continuing...");

                return;
            }

            _dumpLog.WriteLine("Output format does not support storing raw data, this may end in a loss of data, continuing...");

            ErrorMessage?.
                Invoke("Output format does not support storing raw data, this may end in a loss of data, continuing...");
        }

        if(!outputOptical.OpticalCapabilities.HasFlag(OpticalImageCapabilities.CanStoreAudioTracks) &&
           tracks.Any(track => track.Type == TrackType.Audio))
        {
            _dumpLog.WriteLine("Output format does not support audio tracks, cannot continue...");

            StoppingErrorMessage?.Invoke("Output format does not support audio tracks, cannot continue...");

            return;
        }

        if(!outputOptical.OpticalCapabilities.HasFlag(OpticalImageCapabilities.CanStorePregaps) &&
           tracks.Where(track => track.Sequence != tracks.First(t => t.Session == track.Session).Sequence).
                  Any(track => track.Pregap     > 0))
        {
            if(!_force)
            {
                _dumpLog.WriteLine("Output format does not support pregaps, this may end in a loss of data, not continuing...");

                StoppingErrorMessage?.
                    Invoke("Output format does not support pregaps, this may end in a loss of data, not continuing...");

                return;
            }

            _dumpLog.WriteLine("Output format does not support pregaps, this may end in a loss of data, continuing...");

            ErrorMessage?.
                Invoke("Output format does not support pregaps, this may end in a loss of data, continuing...");
        }

        for(var t = 1; t < tracks.Length; t++)
            tracks[t - 1].EndSector = tracks[t].StartSector - 1;

        tracks[^1].EndSector = (ulong)lastSector;
        blocks               = (ulong)(lastSector + 1);

        if(blocks == 0)
        {
            StoppingErrorMessage?.Invoke("Cannot dump blank media.");

            return;
        }

        ResumeSupport.Process(true, true, blocks, _dev.Manufacturer, _dev.Model, _dev.Serial, _dev.PlatformId,
                              ref _resume, ref currentTry, ref extents, _dev.FirmwareRevision, _private, _force);

        if(currentTry == null ||
           extents    == null)
        {
            StoppingErrorMessage?.Invoke("Could not process resume file, not continuing...");

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
            _dumpLog.WriteLine("Output format does not support sessions, this will end in a loss of data, not continuing...");

            StoppingErrorMessage?.
                Invoke("Output format does not support sessions, this will end in a loss of data, not continuing...");

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
                _dumpLog.WriteLine("Output format does not support {0}, continuing...", tag);
                ErrorMessage?.Invoke($"Output format does not support {tag}, continuing...");
            }
            else
            {
                _dumpLog.WriteLine("Output format does not support {0}, not continuing...", tag);
                StoppingErrorMessage?.Invoke($"Output format does not support {tag}, not continuing...");

                return;
            }

        if(leadOutStarts.Any())
        {
            UpdateStatus?.Invoke("Solving lead-outs...");

            foreach(KeyValuePair<int, long> leadOuts in leadOutStarts)
                foreach(Track trk in tracks.Where(trk => trk.Session   == leadOuts.Key).
                                            Where(trk => trk.EndSector >= (ulong)leadOuts.Value))
                    trk.EndSector = (ulong)leadOuts.Value - 1;

            var dataExtents = new ExtentsULong();

            foreach(Track trk in tracks)
                dataExtents.Add(trk.StartSector, trk.EndSector);

            Tuple<ulong, ulong>[] dataExtentsArray = dataExtents.ToArray();

            for(var i = 0; i < dataExtentsArray.Length - 1; i++)
                leadOutExtents.Add(dataExtentsArray[i].Item2 + 1, dataExtentsArray[i + 1].Item1 - 1);
        }

        _dumpLog.WriteLine("Detecting disc type...");
        UpdateStatus?.Invoke("Detecting disc type...");

        MMC.DetectDiscType(ref dskType, sessions, toc, _dev, out hiddenTrack, out hiddenData, firstTrackLastSession,
                           blocks);

        if(hiddenTrack || firstLba > 0)
        {
            _dumpLog.WriteLine("Disc contains a hidden track...");
            UpdateStatus?.Invoke("Disc contains a hidden track...");

            List<Track> trkList = new()
            {
                new Track
                {
                    Sequence          = 0,
                    Session           = 1,
                    Type              = hiddenData ? TrackType.Data : TrackType.Audio,
                    StartSector       = 0,
                    BytesPerSector    = (int)sectorSize,
                    RawBytesPerSector = (int)sectorSize,
                    SubchannelType    = subType,
                    EndSector         = tracks.First(t => t.Sequence == 1).StartSector - 1
                }
            };

            trkList.AddRange(tracks);
            tracks = trkList.ToArray();
        }

        if(tracks.Any(t => t.Type == TrackType.Audio) &&
           desiredSubchannel != MmcSubchannel.Raw)
        {
            _dumpLog.WriteLine("WARNING: If disc says CD+G, CD+EG, CD-MIDI, CD Graphics or CD Enhanced Graphics, dump will be incorrect!");

            UpdateStatus?.
                Invoke("WARNING: If disc says CD+G, CD+EG, CD-MIDI, CD Graphics or CD Enhanced Graphics, dump will be incorrect!");
        }

        // Check mode for tracks
        foreach(Track trk in tracks.Where(t => t.Type != TrackType.Audio))
        {
            if(!readcd)
            {
                trk.Type = TrackType.CdMode1;

                continue;
            }

            _dumpLog.WriteLine("Checking mode for track {0}...", trk.Sequence);
            UpdateStatus?.Invoke($"Checking mode for track {trk.Sequence}...");

            sense = _dev.ReadCd(out cmdBuf, out _, (uint)(trk.StartSector + trk.Pregap), blockSize, 1,
                                MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true,
                                MmcErrorField.None, supportedSubchannel, _dev.Timeout, out _);

            if(sense)
            {
                _dumpLog.WriteLine("Unable to guess mode for track {0}, continuing...", trk.Sequence);

                UpdateStatus?.Invoke($"Unable to guess mode for track {trk.Sequence}, continuing...");

                continue;
            }

            var bufOffset = 0;

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
                    UpdateStatus?.Invoke($"Track {trk.Sequence} is MODE1");
                    _dumpLog.WriteLine("Track {0} is MODE1", trk.Sequence);
                    trk.Type = TrackType.CdMode1;

                    break;
                case 2:
                case 0x62: // Scrambled
                    if(dskType is MediaType.CDI or MediaType.CDIREADY)
                    {
                        UpdateStatus?.Invoke($"Track {trk.Sequence} is MODE2");
                        _dumpLog.WriteLine("Track {0} is MODE2", trk.Sequence);
                        trk.Type = TrackType.CdMode2Formless;

                        break;
                    }

                    if((cmdBuf[0x012] & 0x20) == 0x20) // mode 2 form 2
                    {
                        UpdateStatus?.Invoke($"Track {trk.Sequence} is MODE2 FORM 2");
                        _dumpLog.WriteLine("Track {0} is MODE2 FORM 2", trk.Sequence);
                        trk.Type = TrackType.CdMode2Form2;

                        break;
                    }

                    UpdateStatus?.Invoke($"Track {trk.Sequence} is MODE2 FORM 1");
                    _dumpLog.WriteLine("Track {0} is MODE2 FORM 1", trk.Sequence);
                    trk.Type = TrackType.CdMode2Form1;

                    // These media type specifications do not legally allow mode 2 tracks to be present
                    if(dskType is MediaType.CDROM or MediaType.CDPLUS or MediaType.CDV)
                        dskType = MediaType.CD;

                    break;
                default:
                    UpdateStatus?.Invoke($"Track {trk.Sequence} is unknown mode {cmdBuf[15]}");
                    _dumpLog.WriteLine("Track {0} is unknown mode {1}", trk.Sequence, cmdBuf[15]);

                    break;
            }
        }

        if(outputOptical.Id == new Guid("12345678-AAAA-BBBB-CCCC-123456789000"))
        {
            if(tracks.Length > 1)
            {
                StoppingErrorMessage?.Invoke("Output format does not support more than 1 track, not continuing...");
                _dumpLog.WriteLine("Output format does not support more than 1 track, not continuing...");

                return;
            }

            if(tracks.Any(t => t.Type == TrackType.Audio))
            {
                StoppingErrorMessage?.Invoke("Output format does not support audio tracks, not continuing...");
                _dumpLog.WriteLine("Output format does not support audio tracks, not continuing...");

                return;
            }

            if(tracks.Any(t => t.Type != TrackType.CdMode1))
            {
                StoppingErrorMessage?.Invoke("Output format only supports MODE 1 tracks, not continuing...");
                _dumpLog.WriteLine("Output format only supports MODE 1 tracks, not continuing...");

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
                    _dumpLog.WriteLine("Output format does not support CD first track pregap, continuing...");
                    ErrorMessage?.Invoke("Output format does not support CD first track pregap, continuing...");
                }
                else
                {
                    _dumpLog.WriteLine("Output format does not support CD first track pregap, not continuing...");

                    StoppingErrorMessage?.
                        Invoke("Output format does not support CD first track pregap, not continuing...");

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
            _dumpLog.WriteLine("Device error {0} trying to guess ideal transfer length.", _dev.LastError);
            StoppingErrorMessage?.Invoke($"Device error {_dev.LastError} trying to guess ideal transfer length.");
        }

        // Try to read the first track pregap
        if(_dumpFirstTrackPregap && readcd)
            ReadCdFirstTrackPregap(blockSize, ref currentSpeed, mediaTags, supportedSubchannel, ref totalDuration);

        _dumpLog.WriteLine("Reading {0} sectors at a time.", _maximumReadable);
        _dumpLog.WriteLine("Device reports {0} blocks ({1} bytes).", blocks, blocks * blockSize);
        _dumpLog.WriteLine("Device can read {0} blocks at a time.", _maximumReadable);
        _dumpLog.WriteLine("Device reports {0} bytes per logical block.", blockSize);
        _dumpLog.WriteLine("SCSI device type: {0}.", _dev.ScsiType);
        _dumpLog.WriteLine("Media identified as {0}.", dskType);

        UpdateStatus?.Invoke($"Reading {_maximumReadable} sectors at a time.");
        UpdateStatus?.Invoke($"Device reports {blocks} blocks ({blocks * blockSize} bytes).");
        UpdateStatus?.Invoke($"Device can read {_maximumReadable} blocks at a time.");
        UpdateStatus?.Invoke($"Device reports {blockSize} bytes per logical block.");
        UpdateStatus?.Invoke($"SCSI device type: {_dev.ScsiType}.");
        UpdateStatus?.Invoke($"Media identified as {dskType}.");

        ret = outputOptical.Create(_outputPath, dskType, _formatOptions, blocks,
                                   supportsLongSectors ? blockSize : 2048);

        // Cannot create image
        if(!ret)
        {
            _dumpLog.WriteLine("Error creating output image, not continuing.");
            _dumpLog.WriteLine(outputOptical.ErrorMessage);

            StoppingErrorMessage?.Invoke("Error creating output image, not continuing." + Environment.NewLine +
                                         outputOptical.ErrorMessage);

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
            _dumpLog.WriteLine("Error sending tracks to output image, not continuing.");
            _dumpLog.WriteLine(outputOptical.ErrorMessage);

            StoppingErrorMessage?.Invoke("Error sending tracks to output image, not continuing." + Environment.NewLine +
                                         outputOptical.ErrorMessage);

            return;
        }

        // If a subchannel is supported, check if output plugin allows us to write it.
        if(desiredSubchannel != MmcSubchannel.None &&
           !outputOptical.OpticalCapabilities.HasFlag(OpticalImageCapabilities.CanStoreSubchannelRw))
        {
            _dumpLog.WriteLine("Output image does not support subchannels, {0}continuing...", _force ? "" : "not ");

            if(_force)
                ErrorMessage?.Invoke("Output image does not support subchannels, continuing...");
            else
            {
                StoppingErrorMessage?.Invoke("Output image does not support subchannels, not continuing...");

                return;
            }
        }

        if(supportedSubchannel != MmcSubchannel.None)
        {
            _dumpLog.WriteLine($"Creating subchannel log in {_outputPrefix + ".sub.log"}");
            subLog = new SubchannelLog(_outputPrefix + ".sub.log", bcdSubchannel);
        }

        // Set track flags
        foreach(KeyValuePair<byte, byte> kvp in trackFlags)
        {
            Track track = tracks.FirstOrDefault(t => t.Sequence == kvp.Key);

            if(track is null)
                continue;

            _dumpLog.WriteLine("Setting flags for track {0}...", track.Sequence);
            UpdateStatus?.Invoke($"Setting flags for track {track.Sequence}...");

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
                UpdateStatus?.Invoke($"Found Media Catalogue Number: {mcn}");
                _dumpLog.WriteLine("Found Media Catalogue Number: {0}", mcn);
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

                UpdateStatus?.Invoke($"Found ISRC for track {trk.Sequence}: {isrc}");
                _dumpLog.WriteLine($"Found ISRC for track {trk.Sequence}: {isrc}");
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
            UpdateStatus?.Invoke($"Resuming from block {_resume.NextBlock}.");
            _dumpLog.WriteLine("Resuming from block {0}.", _resume.NextBlock);
        }

        if(_skip < _maximumReadable)
            _skip = _maximumReadable;

    #if DEBUG
        foreach(Track trk in tracks)
            UpdateStatus?.
                Invoke($"Track {trk.Sequence} starts at LBA {trk.StartSector} and ends at LBA {trk.EndSector}");
    #endif

        // Check offset
        if(_fixOffset)
        {
            if(tracks.All(t => t.Type != TrackType.Audio))
            {
                // No audio tracks so no need to fix offset
                _dumpLog.WriteLine("No audio tracks, disabling offset fix.");
                UpdateStatus.Invoke("No audio tracks, disabling offset fix.");

                _fixOffset = false;
            }

            if(!readcd)
            {
                _dumpLog.WriteLine("READ CD command is not supported, disabling offset fix. Dump may not be correct.");

                UpdateStatus?.
                    Invoke("READ CD command is not supported, disabling offset fix. Dump may not be correct.");

                _fixOffset = false;
            }
        }
        else if(tracks.Any(t => t.Type == TrackType.Audio))
        {
            _dumpLog.WriteLine("There are audio tracks and offset fixing is disabled, dump may not be correct.");
            UpdateStatus?.Invoke("There are audio tracks and offset fixing is disabled, dump may not be correct.");
        }

        // Search for read offset in main database
        cdOffset =
            _ctx.CdOffsets.FirstOrDefault(d => (d.Manufacturer == _dev.Manufacturer ||
                                                d.Manufacturer == _dev.Manufacturer.Replace('/', '-')) &&
                                               (d.Model == _dev.Model || d.Model == _dev.Model.Replace('/', '-')));

        Core.Media.Info.CompactDisc.GetOffset(cdOffset, _dbDev, _debug, _dev, dskType, _dumpLog, tracks, UpdateStatus,
                                              out int? driveOffset, out int? combinedOffset, out _supportsPlextorD8);

        if(combinedOffset is null)
        {
            if(driveOffset is null)
            {
                _dumpLog.WriteLine("Drive reading offset not found in database.");
                UpdateStatus?.Invoke("Drive reading offset not found in database.");
                _dumpLog.WriteLine("Disc offset cannot be calculated.");
                UpdateStatus?.Invoke("Disc offset cannot be calculated.");

                if(tracks.Any(t => t.Type == TrackType.Audio))
                {
                    _dumpLog.WriteLine("Dump may not be correct.");

                    UpdateStatus?.Invoke("Dump may not be correct.");
                }

                if(_fixOffset)
                    _fixOffset = false;
            }
            else
            {
                _dumpLog.WriteLine($"Drive reading offset is {driveOffset} bytes ({driveOffset   / 4} samples).");
                UpdateStatus?.Invoke($"Drive reading offset is {driveOffset} bytes ({driveOffset / 4} samples).");

                _dumpLog.WriteLine("Disc write offset is unknown, dump may not be correct.");
                UpdateStatus?.Invoke("Disc write offset is unknown, dump may not be correct.");

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
                _dumpLog.WriteLine("Drive reading offset not found in database.");
                UpdateStatus?.Invoke("Drive reading offset not found in database.");
                _dumpLog.WriteLine($"Combined disc and drive offsets are {offsetBytes} bytes ({offsetBytes / 4} samples).");

                UpdateStatus?.
                    Invoke($"Combined disc and drive offsets are {offsetBytes} bytes ({offsetBytes / 4} samples).");
            }
            else
            {
                _dumpLog.WriteLine($"Drive reading offset is {driveOffset} bytes ({driveOffset   / 4} samples).");
                UpdateStatus?.Invoke($"Drive reading offset is {driveOffset} bytes ({driveOffset / 4} samples).");

                discOffset = offsetBytes - driveOffset;

                _dumpLog.WriteLine($"Disc offsets is {discOffset} bytes ({discOffset / 4} samples)");

                UpdateStatus?.Invoke($"Disc offsets is {discOffset} bytes ({discOffset / 4} samples)");
            }
        }

        if(!_fixOffset ||
           tracks.All(t => t.Type != TrackType.Audio))
        {
            offsetBytes      = 0;
            sectorsForOffset = 0;
        }

        mhddLog = new MhddLog(_outputPrefix + ".mhddlog.bin", _dev, blocks, blockSize, _maximumReadable, _private);
        ibgLog  = new IbgLog(_outputPrefix  + ".ibg", 0x0008);

        audioExtents = new ExtentsULong();

        foreach(Track audioTrack in tracks.Where(t => t.Type == TrackType.Audio))
            audioExtents.Add(audioTrack.StartSector, audioTrack.EndSector);

        // Set speed
        if(_speedMultiplier >= 0)
        {
            _dumpLog.WriteLine($"Setting speed to {(_speed   == 0 ? "MAX for data reading" : $"{_speed}x")}.");
            UpdateStatus?.Invoke($"Setting speed to {(_speed == 0 ? "MAX for data reading" : $"{_speed}x")}.");

            _speed *= _speedMultiplier;

            if(_speed is 0 or > 0xFFFF)
                _speed = 0xFFFF;

            _dev.SetCdSpeed(out _, RotationalControl.ClvAndImpureCav, (ushort)_speed, 0, _dev.Timeout, out _);
        }

        // Start reading
        start = DateTime.UtcNow;

        if(dskType == MediaType.CDIREADY)
        {
            Track track0 = tracks.FirstOrDefault(t => t.Sequence == 0);

            track0.Type = TrackType.CdMode2Formless;

            if(!supportsLongSectors)
            {
                _dumpLog.WriteLine("Dumping CD-i Ready requires the output image format to support long sectors.");

                StoppingErrorMessage?.
                    Invoke("Dumping CD-i Ready requires the output image format to support long sectors.");

                return;
            }

            if(!readcd)
            {
                _dumpLog.WriteLine("Dumping CD-i Ready requires the drive to support the READ CD command.");

                StoppingErrorMessage?.Invoke("Dumping CD-i Ready requires the drive to support the READ CD command.");

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

                    _dumpLog.WriteLine("Enabling skipping CD-i Ready hole because drive returns data as audio.");

                    UpdateStatus?.Invoke("Enabling skipping CD-i Ready hole because drive returns data as audio.");

                    _skipCdireadyHole = true;

                    if(driveOffset is null)
                    {
                        _dumpLog.WriteLine("Drive reading offset not found in database.");
                        UpdateStatus?.Invoke("Drive reading offset not found in database.");

                        _dumpLog.
                            WriteLine($"Combined disc and drive offsets are {offsetBytes} bytes ({offsetBytes / 4} samples).");

                        UpdateStatus?.
                            Invoke($"Combined disc and drive offsets are {offsetBytes} bytes ({offsetBytes / 4} samples).");
                    }
                    else
                    {
                        _dumpLog.WriteLine($"Drive reading offset is {driveOffset} bytes ({driveOffset / 4} samples).");

                        UpdateStatus?.
                            Invoke($"Drive reading offset is {driveOffset} bytes ({driveOffset / 4} samples).");

                        discOffset = offsetBytes - driveOffset;

                        _dumpLog.WriteLine($"Disc offsets is {discOffset} bytes ({discOffset / 4} samples)");

                        UpdateStatus?.Invoke($"Disc offsets is {discOffset} bytes ({discOffset / 4} samples)");
                    }
                }
            }

            if(!_skipCdireadyHole)
            {
                _dumpLog.WriteLine("There will be thousand of errors between track 0 and track 1, that is normal and you can ignore them.");

                UpdateStatus?.
                    Invoke("There will be thousand of errors between track 0 and track 1, that is normal and you can ignore them.");
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

        UpdateStatus?.Invoke($"Dump finished in {(end - start).TotalSeconds} seconds.");

        UpdateStatus?.
            Invoke($"Average dump speed {blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000):F3} KiB/sec.");

        UpdateStatus?.
            Invoke($"Average write speed {blockSize * (double)(blocks + 1) / 1024 / imageWriteDuration:F3} KiB/sec.");

        _dumpLog.WriteLine("Dump finished in {0} seconds.", (end - start).TotalSeconds);

        _dumpLog.WriteLine("Average dump speed {0:F3} KiB/sec.",
                           blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000));

        _dumpLog.WriteLine("Average write speed {0:F3} KiB/sec.",
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
                    AaruConsole.ErrorWriteLine("Error: Tag type {0} is null, skipping...", tag.Key);

                    continue;
                }

                ret = outputOptical.WriteMediaTag(tag.Value, tag.Key);

                if(ret || _force)
                    continue;

                // Cannot write tag to image
                _dumpLog.WriteLine($"Cannot write tag {tag.Key}.");
                StoppingErrorMessage?.Invoke(outputOptical.ErrorMessage);

                return;
            }

        _resume.BadBlocks.Sort();

        foreach(ulong bad in _resume.BadBlocks)
            _dumpLog.WriteLine("Sector {0} could not be read.", bad);

        currentTry.Extents = ExtentsConverter.ToMetadata(extents);

        _resume.BadSubchannels = new List<int>();
        _resume.BadSubchannels.AddRange(subchannelExtents);
        _resume.BadSubchannels.Sort();

        if(_generateSubchannels                                                         &&
           outputOptical.SupportedSectorTags.Contains(SectorTagType.CdSectorSubchannel) &&
           !_aborted)
            Core.Media.CompactDisc.GenerateSubchannels(subchannelExtents, tracks, trackFlags, blocks, subLog, _dumpLog,
                                                       InitProgress, UpdateProgress, EndProgress, outputOptical);

        // TODO: Disc ID
        var metadata = new ImageInfo
        {
            Application        = "Aaru",
            ApplicationVersion = Version.GetVersion()
        };

        if(!outputOptical.SetMetadata(metadata))
            ErrorMessage?.Invoke("Error {0} setting metadata, continuing..." + Environment.NewLine +
                                 outputOptical.ErrorMessage);

        outputOptical.SetDumpHardware(_resume.Tries);

        if(_preSidecar != null)
            outputOptical.SetCicmMetadata(_preSidecar);

        foreach(KeyValuePair<byte, string> isrc in isrcs)
        {
            // TODO: Track tags
            if(!outputOptical.WriteSectorTag(Encoding.ASCII.GetBytes(isrc.Value), isrc.Key, SectorTagType.CdTrackIsrc))
                continue;

            UpdateStatus?.Invoke($"Setting ISRC for track {isrc.Key} to {isrc.Value}");
            _dumpLog.WriteLine("Setting ISRC for track {0} to {1}", isrc.Key, isrc.Value);
        }

        if(mcn != null &&
           outputOptical.WriteMediaTag(Encoding.ASCII.GetBytes(mcn), MediaTagType.CD_MCN))
        {
            UpdateStatus?.Invoke($"Setting disc Media Catalogue Number to {mcn}");
            _dumpLog.WriteLine("Setting disc Media Catalogue Number to {0}", mcn);
        }

        foreach(Track trk in tracks)
        {
            // Fix track starts in each session's first track
            if(tracks.Where(t => t.Session == trk.Session).OrderBy(t => t.Sequence).FirstOrDefault().Sequence ==
               trk.Sequence)
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

        _dumpLog.WriteLine("Closing output file.");
        UpdateStatus?.Invoke("Closing output file.");
        DateTime closeStart = DateTime.Now;
        outputOptical.Close();
        DateTime closeEnd = DateTime.Now;
        UpdateStatus?.Invoke($"Closed in {(closeEnd - closeStart).TotalSeconds} seconds.");

        subLog?.Close();

        if(_aborted)
        {
            _dumpLog.WriteLine("Aborted!");

            return;
        }

        double totalChkDuration = 0;

        if(_metadata)
            WriteOpticalSidecar(blockSize, blocks, dskType, null, mediaTags, sessions, out totalChkDuration,
                                discOffset);

        end = DateTime.UtcNow;
        UpdateStatus?.Invoke("");

        UpdateStatus?.
            Invoke($"Took a total of {(end - dumpStart).TotalSeconds:F3} seconds ({totalDuration / 1000:F3} processing commands, {totalChkDuration / 1000:F3} checksumming, {imageWriteDuration:F3} writing, {(closeEnd - closeStart).TotalSeconds:F3} closing).");

        UpdateStatus?.
            Invoke($"Average speed: {blockSize * (double)(blocks + 1) / 1048576 / (totalDuration / 1000):F3} MiB/sec.");

        if(maxSpeed > 0)
            UpdateStatus?.Invoke($"Fastest speed burst: {maxSpeed:F3} MiB/sec.");

        if(minSpeed > 0 &&
           minSpeed < double.MaxValue)
            UpdateStatus?.Invoke($"Slowest speed burst: {minSpeed:F3} MiB/sec.");

        UpdateStatus?.Invoke($"{_resume.BadBlocks.Count} sectors could not be read.");
        UpdateStatus?.Invoke($"{_resume.BadSubchannels.Count} subchannels could not be read.");
        UpdateStatus?.Invoke("");

        Statistics.AddMedia(dskType, true);
    }
}