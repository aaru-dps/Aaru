// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CompactDisc.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps CDs and DDCDs.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Extents;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using DiscImageChef.Core.Logging;
using DiscImageChef.Core.Media.Detection;
using DiscImageChef.Database.Models;
using DiscImageChef.Decoders.CD;
using DiscImageChef.Devices;
using Schemas;
using PlatformID = DiscImageChef.CommonTypes.Interop.PlatformID;
using TrackType = DiscImageChef.CommonTypes.Enums.TrackType;

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

namespace DiscImageChef.Core.Devices.Dumping
{
    /// <summary>Implement dumping Compact Discs</summary>

    // TODO: Barcode
    partial class Dump
    {
        /// <summary>Dumps a compact disc</summary>
        /// <param name="dskType">Disc type as detected in MMC layer</param>
        void CompactDisc(out MediaType dskType)
        {
            ExtentsULong           audioExtents;                                 // Extents with audio sectors
            ulong                  blocks;                                       // Total number of positive sectors
            uint                   blockSize;                                    // Size of the read sector in bytes
            CdOffset               cdOffset;                                     // Read offset from database
            byte[]                 cmdBuf;                                       // Data buffer
            DumpHardwareType       currentTry   = null;                          // Current dump hardware try
            double                 currentSpeed = 0;                             // Current read speed
            DateTime               dumpStart    = DateTime.UtcNow;               // Time of dump start
            DateTime               end;                                          // Time of operation end
            ExtentsULong           extents = null;                               // Extents
            bool                   hiddenData;                                   // Hidden track is data
            IbgLog                 ibgLog;                                       // IMGBurn log
            double                 imageWriteDuration = 0;                       // Duration of image write
            long                   lastSector;                                   // Last sector number
            var                    leadOutExtents = new ExtentsULong();          // Lead-out extents
            Dictionary<int, long>  leadOutStarts  = new Dictionary<int, long>(); // Lead-out starts
            double                 maxSpeed       = double.MinValue;             // Maximum speed
            MhddLog                mhddLog;                                      // MHDD log
            double                 minSpeed = double.MaxValue;                   // Minimum speed
            bool                   newTrim;                                      // Is trim a new one?
            int                    offsetBytes = 0;                              // Read offset
            bool                   read6       = false;                          // Device supports READ(6)
            bool                   read10      = false;                          // Device supports READ(10)
            bool                   read12      = false;                          // Device supports READ(12)
            bool                   read16      = false;                          // Device supports READ(16)
            bool                   readcd;                                       // Device supports READ CD
            bool                   ret;                                          // Image writing return status
            const uint             sectorSize       = 2352;                      // Full sector size
            int                    sectorsForOffset = 0;                         // Sectors needed to fix offset
            bool                   sense            = true;                      // Sense indicator
            int                    sessions;                                     // Number of sessions in disc
            DateTime               start;                                        // Start of operation
            uint                   subSize;                                      // Subchannel size in bytes
            TrackSubchannelType    subType;                                      // Track subchannel type
            bool                   supportsLongSectors = true;                   // Supports reading EDC and ECC
            bool                   supportsPqSubchannel;                         // Supports reading PQ subchannel
            bool                   supportsRwSubchannel;                         // Supports reading RW subchannel
            byte[]                 tmpBuf;                                       // Temporary buffer
            FullTOC.CDFullTOC?     toc;                                          // Full CD TOC
            double                 totalDuration = 0;                            // Total commands duration
            Dictionary<byte, byte> trackFlags    = new Dictionary<byte, byte>(); // Track flags
            Track[]                tracks;                                       // Tracks in disc

            int           firstTrackLastSession; // Number of first track in last session
            bool          hiddenTrack;           // Disc has a hidden track before track 1
            MmcSubchannel supportedSubchannel;   // Drive's maximum supported subchannel

            Dictionary<MediaTagType, byte[]> mediaTags = new Dictionary<MediaTagType, byte[]>(); // Media tags

            dskType = MediaType.CD;

            if(_dumpRaw)
            {
                _dumpLog.WriteLine("Raw CD dumping not yet implemented");
                StoppingErrorMessage?.Invoke("Raw CD dumping not yet implemented");

                return;
            }

            // Check subchannels support
            supportsPqSubchannel = SupportsPqSubchannel(_dev, _dumpLog, UpdateStatus);
            supportsRwSubchannel = SupportsRwSubchannel(_dev, _dumpLog, UpdateStatus);

            switch(_subchannel)
            {
                case DumpSubchannel.Any:
                    if(supportsRwSubchannel)
                        supportedSubchannel = MmcSubchannel.Raw;
                    else if(supportsPqSubchannel)
                        supportedSubchannel = MmcSubchannel.Q16;
                    else
                        supportedSubchannel = MmcSubchannel.None;

                    break;
                case DumpSubchannel.Rw:
                    if(supportsRwSubchannel)
                        supportedSubchannel = MmcSubchannel.Raw;
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
                        supportedSubchannel = MmcSubchannel.Raw;
                    else if(supportsPqSubchannel)
                        supportedSubchannel = MmcSubchannel.Q16;
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
                        supportedSubchannel = MmcSubchannel.Q16;
                    else
                    {
                        _dumpLog.WriteLine("Drive does not support the requested subchannel format, not continuing...");

                        StoppingErrorMessage?.
                            Invoke("Drive does not support the requested subchannel format, not continuing...");

                        return;
                    }

                    break;
                case DumpSubchannel.None:
                    supportedSubchannel = MmcSubchannel.None;

                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            // Check if output format supports subchannels
            if(!_outputPlugin.SupportedSectorTags.Contains(SectorTagType.CdSectorSubchannel) &&
               supportedSubchannel != MmcSubchannel.None)
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

                supportedSubchannel = MmcSubchannel.None;
            }

            switch(supportedSubchannel)
            {
                case MmcSubchannel.None:
                    _dumpLog.WriteLine("Checking if drive supports reading without subchannel...");
                    UpdateStatus?.Invoke("Checking if drive supports reading without subchannel...");

                    readcd = !_dev.ReadCd(out cmdBuf, out _, 0, sectorSize, 1, MmcSectorTypes.AllTypes, false, false,
                                          true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                          supportedSubchannel, _dev.Timeout, out _);

                    if(!readcd)
                    {
                        _dumpLog.WriteLine("Drive does not support READ CD, trying SCSI READ commands...");
                        ErrorMessage?.Invoke("Drive does not support READ CD, trying SCSI READ commands...");

                        _dumpLog.WriteLine("Checking if drive supports READ(6)...");
                        UpdateStatus?.Invoke("Checking if drive supports READ(6)...");
                        read6 = !_dev.Read6(out cmdBuf, out _, 0, 2048, 1, _dev.Timeout, out _);
                        _dumpLog.WriteLine("Checking if drive supports READ(10)...");
                        UpdateStatus?.Invoke("Checking if drive supports READ(10)...");

                        read10 = !_dev.Read10(out cmdBuf, out _, 0, false, true, false, false, 0, 2048, 0, 1,
                                              _dev.Timeout, out _);

                        _dumpLog.WriteLine("Checking if drive supports READ(12)...");
                        UpdateStatus?.Invoke("Checking if drive supports READ(12)...");

                        read12 = !_dev.Read12(out cmdBuf, out _, 0, false, true, false, false, 0, 2048, 0, 1, false,
                                              _dev.Timeout, out _);

                        _dumpLog.WriteLine("Checking if drive supports READ(16)...");
                        UpdateStatus?.Invoke("Checking if drive supports READ(16)...");

                        read16 = !_dev.Read16(out cmdBuf, out _, 0, false, true, false, 0, 2048, 0, 1, false,
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
                    _dumpLog.WriteLine("WARNING: If disc says CD+G, CD+EG, CD-MIDI, CD Graphics or CD Enhanced Graphics, dump will be incorrect!");
                    UpdateStatus?.Invoke("Drive can read without subchannel...");

                    UpdateStatus?.
                        Invoke("WARNING: If disc says CD+G, CD+EG, CD-MIDI, CD Graphics or CD Enhanced Graphics, dump will be incorrect!");

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

            blockSize = sectorSize + subSize;

            tracks = GetCdTracks(ref blockSize, _dev, dskType, _dumpLog, _force, out lastSector, leadOutStarts,
                                 mediaTags, StoppingErrorMessage, subType, out toc, trackFlags, UpdateStatus);

            if(tracks is null)
                return;

            SolveTrackPregaps(_dev, _dumpLog, UpdateStatus, tracks, supportsPqSubchannel, supportsRwSubchannel, _dbDev,
                              out bool inexactPositioning);

            if(inexactPositioning)
            {
                _dumpLog.WriteLine("WARNING: The drive has returned incorrect Q positioning when calculating pregaps. A best effort has been tried but they may be incorrect.");

                UpdateStatus?.
                    Invoke("WARNING: The drive has returned incorrect Q positioning when calculating pregaps. A best effort has been tried but they may be incorrect.");
            }

            for(int t = 1; t < tracks.Length; t++)
                tracks[t - 1].TrackEndSector = tracks[t].TrackStartSector - 1;

            tracks[tracks.Length - 1].TrackEndSector = (ulong)lastSector;
            blocks                                   = (ulong)(lastSector + 1);

            if(blocks == 0)
            {
                StoppingErrorMessage?.Invoke("Cannot dump blank media.");

                return;
            }

            ResumeSupport.Process(true, true, blocks, _dev.Manufacturer, _dev.Model, _dev.Serial, _dev.PlatformId,
                                  ref _resume, ref currentTry, ref extents);

            if(currentTry == null ||
               extents    == null)
            {
                StoppingErrorMessage?.Invoke("Could not process resume file, not continuing...");

                return;
            }

            // Read media tags
            ReadCdTags(ref dskType, mediaTags, out sessions, out firstTrackLastSession);

            // Check if output format supports all disc tags we have retrieved so far
            foreach(MediaTagType tag in mediaTags.Keys)
            {
                if(_outputPlugin.SupportedMediaTags.Contains(tag))
                    continue;

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
            }

            if(leadOutStarts.Any())
            {
                UpdateStatus?.Invoke("Solving lead-outs...");

                foreach(KeyValuePair<int, long> leadOuts in leadOutStarts)
                    for(int i = 0; i < tracks.Length; i++)
                    {
                        if(tracks[i].TrackSession != leadOuts.Key)
                            continue;

                        if(tracks[i].TrackEndSector >= (ulong)leadOuts.Value)
                            tracks[i].TrackEndSector = (ulong)leadOuts.Value - 1;
                    }

                var dataExtents = new ExtentsULong();

                foreach(Track trk in tracks)
                    dataExtents.Add(trk.TrackStartSector, trk.TrackEndSector);

                Tuple<ulong, ulong>[] dataExtentsArray = dataExtents.ToArray();

                for(int i = 0; i < dataExtentsArray.Length - 1; i++)
                    leadOutExtents.Add(dataExtentsArray[i].Item2 + 1, dataExtentsArray[i + 1].Item1 - 1);
            }

            _dumpLog.WriteLine("Detecting disc type...");
            UpdateStatus?.Invoke("Detecting disc type...");

            MMC.DetectDiscType(ref dskType, sessions, toc, _dev, out hiddenTrack, out hiddenData,
                               firstTrackLastSession);

            if(hiddenTrack)
            {
                _dumpLog.WriteLine("Disc contains a hidden track...");
                UpdateStatus?.Invoke("Disc contains a hidden track...");

                List<Track> trkList = new List<Track>
                {
                    new Track
                    {
                        TrackSequence          = 0, TrackSession = 1,
                        TrackType              = hiddenData ? TrackType.Data : TrackType.Audio,
                        TrackStartSector       = 0, TrackBytesPerSector               = (int)sectorSize,
                        TrackRawBytesPerSector = (int)sectorSize, TrackSubchannelType = subType,
                        TrackEndSector         = tracks.First(t => t.TrackSequence == 1).TrackStartSector - 1
                    }
                };

                trkList.AddRange(tracks);
                tracks = trkList.ToArray();
            }

            // Check mode for tracks
            for(int t = 0; t < tracks.Length; t++)
            {
                if(!readcd)
                {
                    tracks[t].TrackType = TrackType.CdMode1;

                    continue;
                }

                if(tracks[t].TrackType == TrackType.Audio)
                    continue;

                _dumpLog.WriteLine("Checking mode for track {0}...", tracks[t].TrackSequence);
                UpdateStatus?.Invoke($"Checking mode for track {tracks[t].TrackSequence}...");

                sense = !_dev.ReadCd(out cmdBuf, out _, (uint)tracks[t].TrackStartSector, blockSize, 1,
                                     MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true,
                                     MmcErrorField.None, supportedSubchannel, _dev.Timeout, out _);

                if(!sense)
                {
                    _dumpLog.WriteLine("Unable to guess mode for track {0}, continuing...", tracks[t].TrackSequence);
                    UpdateStatus?.Invoke($"Unable to guess mode for track {tracks[t].TrackSequence}, continuing...");

                    continue;
                }

                switch(cmdBuf[15])
                {
                    case 1:
                        UpdateStatus?.Invoke($"Track {tracks[t].TrackSequence} is MODE1");
                        _dumpLog.WriteLine("Track {0} is MODE1", tracks[t].TrackSequence);
                        tracks[t].TrackType = TrackType.CdMode1;

                        break;
                    case 2:
                        if(dskType == MediaType.CDI ||
                           dskType == MediaType.CDIREADY)
                        {
                            UpdateStatus?.Invoke($"Track {tracks[t].TrackSequence} is MODE2");
                            _dumpLog.WriteLine("Track {0} is MODE2", tracks[t].TrackSequence);
                            tracks[t].TrackType = TrackType.CdMode2Formless;

                            break;
                        }

                        if((cmdBuf[0x012] & 0x20) == 0x20) // mode 2 form 2
                        {
                            UpdateStatus?.Invoke($"Track {tracks[t].TrackSequence} is MODE2 FORM 2");
                            _dumpLog.WriteLine("Track {0} is MODE2 FORM 2", tracks[t].TrackSequence);
                            tracks[t].TrackType = TrackType.CdMode2Form2;

                            break;
                        }

                        UpdateStatus?.Invoke($"Track {tracks[t].TrackSequence} is MODE2 FORM 1");
                        _dumpLog.WriteLine("Track {0} is MODE2 FORM 1", tracks[t].TrackSequence);
                        tracks[t].TrackType = TrackType.CdMode2Form1;

                        // These media type specifications do not legally allow mode 2 tracks to be present
                        if(dskType == MediaType.CDROM  ||
                           dskType == MediaType.CDPLUS ||
                           dskType == MediaType.CDV)
                            dskType = MediaType.CD;

                        break;
                    default:
                        UpdateStatus?.Invoke($"Track {tracks[t].TrackSequence} is unknown mode {cmdBuf[15]}");
                        _dumpLog.WriteLine("Track {0} is unknown mode {1}", tracks[t].TrackSequence, cmdBuf[15]);

                        break;
                }
            }

            if(_outputPlugin.Id == new Guid("12345678-AAAA-BBBB-CCCC-123456789000"))
            {
                if(tracks.Length > 1)
                {
                    StoppingErrorMessage?.Invoke("Output format does not support more than 1 track, not continuing...");
                    _dumpLog.WriteLine("Output format does not support more than 1 track, not continuing...");

                    return;
                }

                if(tracks.Any(t => t.TrackType == TrackType.Audio))
                {
                    StoppingErrorMessage?.Invoke("Output format does not support audio tracks, not continuing...");
                    _dumpLog.WriteLine("Output format does not support audio tracks, not continuing...");

                    return;
                }

                if(tracks.Any(t => t.TrackType != TrackType.CdMode1))
                {
                    StoppingErrorMessage?.Invoke("Output format only supports MODE 1 tracks, not continuing...");
                    _dumpLog.WriteLine("Output format only supports MODE 1 tracks, not continuing...");

                    return;
                }

                supportsLongSectors = false;
            }

            // Check if something prevents from dumping the first track pregap
            if(_dumpFirstTrackPregap && readcd)
            {
                if(_dev.PlatformId == PlatformID.FreeBSD &&
                   !_dev.IsRemote)
                {
                    if(_force)
                    {
                        _dumpLog.
                            WriteLine("FreeBSD panics when reading CD first track pregap, see upstream bug #224253. continuing");

                        ErrorMessage?.
                            Invoke("FreeBSD panics when reading CD first track pregap, see upstream bug #224253. continuing");
                    }
                    else
                    {
                        _dumpLog.
                            WriteLine("FreeBSD panics when reading CD first track pregap, see upstream bug #224253. Not continuing");

                        StoppingErrorMessage?.
                            Invoke("FreeBSD panics when reading CD first track pregap, see upstream bug #224253. Not continuing");

                        return;
                    }

                    _dumpFirstTrackPregap = false;
                }

                if(!_outputPlugin.SupportedMediaTags.Contains(MediaTagType.CD_FirstTrackPregap))
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
            }

            // Try to read the first track pregap
            if(_dumpFirstTrackPregap && readcd)
                ReadCdFirstTrackPregap(blockSize, ref currentSpeed, mediaTags, supportedSubchannel, ref totalDuration);

            // Try how many blocks are readable at once
            while(true)
            {
                if(readcd)
                {
                    sense = _dev.ReadCd(out cmdBuf, out _, 0, blockSize, _maximumReadable, MmcSectorTypes.AllTypes,
                                        false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                        supportedSubchannel, _dev.Timeout, out _);

                    if(_dev.Error || sense)
                        _maximumReadable /= 2;
                }
                else if(read16)
                {
                    sense = _dev.Read16(out cmdBuf, out _, 0, false, true, false, 0, blockSize, 0, _maximumReadable,
                                        false, _dev.Timeout, out _);

                    if(_dev.Error || sense)
                        _maximumReadable /= 2;
                }
                else if(read12)
                {
                    sense = _dev.Read12(out cmdBuf, out _, 0, false, true, false, false, 0, blockSize, 0,
                                        _maximumReadable, false, _dev.Timeout, out _);

                    if(_dev.Error || sense)
                        _maximumReadable /= 2;
                }
                else if(read10)
                {
                    sense = _dev.Read10(out cmdBuf, out _, 0, false, true, false, false, 0, blockSize, 0,
                                        (ushort)_maximumReadable, _dev.Timeout, out _);

                    if(_dev.Error || sense)
                        _maximumReadable /= 2;
                }
                else if(read6)
                {
                    sense = _dev.Read6(out cmdBuf, out _, 0, blockSize, (byte)_maximumReadable, _dev.Timeout, out _);

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

            ret = _outputPlugin.Create(_outputPath, dskType, _formatOptions, blocks,
                                       supportsLongSectors ? blockSize : 2048);

            // Cannot create image
            if(!ret)
            {
                _dumpLog.WriteLine("Error creating output image, not continuing.");
                _dumpLog.WriteLine(_outputPlugin.ErrorMessage);

                StoppingErrorMessage?.Invoke("Error creating output image, not continuing." + Environment.NewLine +
                                             _outputPlugin.ErrorMessage);
            }

            // Send track list to output plugin. This may fail if subchannel is set but unsupported.
            ret = (_outputPlugin as IWritableOpticalImage).SetTracks(tracks.ToList());

            if(!ret &&
               supportedSubchannel == MmcSubchannel.None)
            {
                _dumpLog.WriteLine("Error sending tracks to output image, not continuing.");
                _dumpLog.WriteLine(_outputPlugin.ErrorMessage);

                StoppingErrorMessage?.Invoke("Error sending tracks to output image, not continuing." +
                                             Environment.NewLine                                     +
                                             _outputPlugin.ErrorMessage);

                return;
            }

            // If a subchannel is supported, check if output plugin allows us to write it.
            if(supportedSubchannel != MmcSubchannel.None)
            {
                _dev.ReadCd(out cmdBuf, out _, 0, blockSize, 1, MmcSectorTypes.AllTypes, false, false, true,
                            MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, supportedSubchannel,
                            _dev.Timeout, out _);

                tmpBuf = new byte[subSize];
                Array.Copy(cmdBuf, sectorSize, tmpBuf, 0, subSize);

                ret = _outputPlugin.WriteSectorTag(tmpBuf, 0, SectorTagType.CdSectorSubchannel);

                if(!ret)
                {
                    if(_force)
                    {
                        _dumpLog.WriteLine("Error writing subchannel to output image, {0}continuing...",
                                           _force ? "" : "not ");

                        _dumpLog.WriteLine(_outputPlugin.ErrorMessage);

                        ErrorMessage?.Invoke("Error writing subchannel to output image, continuing..." +
                                             Environment.NewLine                                       +
                                             _outputPlugin.ErrorMessage);
                    }
                    else
                    {
                        StoppingErrorMessage?.Invoke("Error writing subchannel to output image, not continuing..." +
                                                     Environment.NewLine                                           +
                                                     _outputPlugin.ErrorMessage);

                        return;
                    }

                    supportedSubchannel = MmcSubchannel.None;
                    subSize             = 0;
                    blockSize           = sectorSize + subSize;

                    for(int t = 0; t < tracks.Length; t++)
                        tracks[t].TrackSubchannelType = TrackSubchannelType.None;

                    ret = (_outputPlugin as IWritableOpticalImage).SetTracks(tracks.ToList());

                    if(!ret)
                    {
                        _dumpLog.WriteLine("Error sending tracks to output image, not continuing.");
                        _dumpLog.WriteLine(_outputPlugin.ErrorMessage);

                        StoppingErrorMessage?.Invoke("Error sending tracks to output image, not continuing..." +
                                                     Environment.NewLine                                       +
                                                     _outputPlugin.ErrorMessage);

                        return;
                    }
                }
            }

            // Set track flags
            foreach(KeyValuePair<byte, byte> kvp in trackFlags)
            {
                Track track = tracks.FirstOrDefault(t => t.TrackSequence == kvp.Key);

                if(track.TrackSequence == 0)
                    continue;

                _dumpLog.WriteLine("Setting flags for track {0}...", track.TrackSequence);
                UpdateStatus?.Invoke($"Setting flags for track {track.TrackSequence}...");

                _outputPlugin.WriteSectorTag(new[]
                {
                    kvp.Value
                }, track.TrackStartSector, SectorTagType.CdTrackFlags);
            }

            // Set MCN
            sense = _dev.ReadMcn(out string mcn, out _, out _, _dev.Timeout, out _);

            if(!sense                 &&
               mcn != null            &&
               mcn != "0000000000000" &&
               _outputPlugin.WriteMediaTag(Encoding.ASCII.GetBytes(mcn), MediaTagType.CD_MCN))
            {
                UpdateStatus?.Invoke($"Setting disc Media Catalogue Number to {mcn}");
                _dumpLog.WriteLine("Setting disc Media Catalogue Number to {0}", mcn);
            }

            // Set ISRCs
            foreach(Track trk in tracks)
            {
                sense = _dev.ReadIsrc((byte)trk.TrackSequence, out string isrc, out _, out _, _dev.Timeout, out _);

                if(sense        ||
                   isrc == null ||
                   isrc == "000000000000")
                    continue;

                if(!_outputPlugin.WriteSectorTag(Encoding.ASCII.GetBytes(isrc), trk.TrackStartSector,
                                                 SectorTagType.CdTrackIsrc))
                    continue;

                UpdateStatus?.Invoke($"Setting ISRC for track {trk.TrackSequence} to {isrc}");
                _dumpLog.WriteLine("Setting ISRC for track {0} to {1}", trk.TrackSequence, isrc);
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
                    Invoke($"Track {trk.TrackSequence} starts at LBA {trk.TrackStartSector} and ends at LBA {trk.TrackEndSector}");
        #endif

            if(dskType == MediaType.CDIREADY)
            {
                _dumpLog.WriteLine("There will be thousand of errors between track 0 and track 1, that is normal and you can ignore them.");

                UpdateStatus?.
                    Invoke("There will be thousand of errors between track 0 and track 1, that is normal and you can ignore them.");
            }

            // Check offset
            if(_fixOffset)
            {
                if(tracks.All(t => t.TrackType != TrackType.Audio))
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
            else if(tracks.Any(t => t.TrackType == TrackType.Audio))
            {
                _dumpLog.WriteLine("There are audio tracks and offset fixing is disabled, dump may not be correct.");
                UpdateStatus?.Invoke("There are audio tracks and offset fixing is disabled, dump may not be correct.");
            }

            // Search for read offset in master database
            cdOffset = _ctx.CdOffsets.FirstOrDefault(d => d.Manufacturer == _dev.Manufacturer && d.Model == _dev.Model);

            Media.Info.CompactDisc.GetOffset(cdOffset, _dbDev, _debug, _dev, dskType, _dumpLog, tracks, UpdateStatus,
                                             out int? driveOffset, out int? combinedOffset);

            if(combinedOffset is null)
            {
                if(driveOffset is null)
                {
                    _dumpLog.WriteLine("Drive reading offset not found in database.");
                    UpdateStatus?.Invoke("Drive reading offset not found in database.");
                    _dumpLog.WriteLine("Disc offset cannot be calculated.");
                    UpdateStatus?.Invoke("Disc offset cannot be calculated.");

                    if(tracks.Any(t => t.TrackType == TrackType.Audio))
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

                    int? discOffset = offsetBytes - driveOffset;

                    _dumpLog.WriteLine($"Disc offsets is {discOffset} bytes ({discOffset / 4} samples)");

                    UpdateStatus?.Invoke($"Disc offsets is {discOffset} bytes ({discOffset / 4} samples)");
                }
            }

            if(!_fixOffset ||
               tracks.All(t => t.TrackType != TrackType.Audio))
            {
                offsetBytes      = 0;
                sectorsForOffset = 0;
            }

            mhddLog = new MhddLog(_outputPrefix + ".mhddlog.bin", _dev, blocks, blockSize, _maximumReadable);
            ibgLog  = new IbgLog(_outputPrefix  + ".ibg", 0x0008);

            audioExtents = new ExtentsULong();

            foreach(Track audioTrack in tracks.Where(t => t.TrackType == TrackType.Audio))
            {
                audioExtents.Add(audioTrack.TrackStartSector, audioTrack.TrackEndSector);
            }

            // Set speed
            if(_speedMultiplier >= 0)
            {
                _dumpLog.WriteLine($"Setting speed to {(_speed   == 0 ? "MAX" : $"{_speed}x")}.");
                UpdateStatus?.Invoke($"Setting speed to {(_speed == 0 ? "MAX" : $"{_speed}x")}.");

                _speed *= _speedMultiplier;

                if(_speed == 0 ||
                   _speed > 0xFFFF)
                    _speed = 0xFFFF;

                _dev.SetCdSpeed(out _, RotationalControl.ClvAndImpureCav, (ushort)_speed, 0, _dev.Timeout, out _);
            }

            // Start reading
            start = DateTime.UtcNow;

            ReadCdData(audioExtents, blocks, blockSize, ref currentSpeed, currentTry, extents, ibgLog,
                       ref imageWriteDuration, lastSector, leadOutExtents, ref maxSpeed, mhddLog, ref minSpeed,
                       out newTrim, tracks[0].TrackType != TrackType.Audio, offsetBytes, read6, read10, read12, read16,
                       readcd, sectorsForOffset, subSize, supportedSubchannel, supportsLongSectors, ref totalDuration,
                       tracks);

            // TODO: Enable when underlying images support lead-outs
            /*
            DumpCdLeadOuts(blocks, blockSize, ref currentSpeed, currentTry, extents, ibgLog, ref imageWriteDuration,
                           leadOutExtents, ref maxSpeed, mhddLog, ref minSpeed, read6, read10, read12, read16, readcd,
                           supportedSubchannel, subSize, ref totalDuration);
            */

            end = DateTime.UtcNow;
            mhddLog.Close();

            ibgLog.Close(_dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024,
                         (blockSize * (double)(blocks + 1)) / 1024                         / (totalDuration / 1000),
                         _devicePath);

            UpdateStatus?.Invoke($"Dump finished in {(end - start).TotalSeconds} seconds.");

            UpdateStatus?.
                Invoke($"Average dump speed {((double)blockSize * (double)(blocks + 1)) / 1024 / (totalDuration / 1000):F3} KiB/sec.");

            UpdateStatus?.
                Invoke($"Average write speed {((double)blockSize * (double)(blocks + 1)) / 1024 / imageWriteDuration:F3} KiB/sec.");

            _dumpLog.WriteLine("Dump finished in {0} seconds.", (end - start).TotalSeconds);

            _dumpLog.WriteLine("Average dump speed {0:F3} KiB/sec.",
                               ((double)blockSize * (double)(blocks + 1)) / 1024 / (totalDuration / 1000));

            _dumpLog.WriteLine("Average write speed {0:F3} KiB/sec.",
                               ((double)blockSize * (double)(blocks + 1)) / 1024 / imageWriteDuration);

            TrimCdUserData(audioExtents, blockSize, currentTry, extents, newTrim, offsetBytes, read6, read10, read12,
                           read16, readcd, sectorsForOffset, subSize, supportedSubchannel, supportsLongSectors,
                           ref totalDuration);

            RetryCdUserData(audioExtents, blockSize, currentTry, extents, offsetBytes, readcd, sectorsForOffset,
                            subSize, supportedSubchannel, ref totalDuration);

            // Write media tags to image
            if(!_aborted)
                foreach(KeyValuePair<MediaTagType, byte[]> tag in mediaTags)
                {
                    if(tag.Value is null)
                    {
                        DicConsole.ErrorWriteLine("Error: Tag type {0} is null, skipping...", tag.Key);

                        continue;
                    }

                    ret = _outputPlugin.WriteMediaTag(tag.Value, tag.Key);

                    if(ret || _force)
                        continue;

                    // Cannot write tag to image
                    _dumpLog.WriteLine($"Cannot write tag {tag.Key}.");
                    StoppingErrorMessage?.Invoke(_outputPlugin.ErrorMessage);

                    return;
                }

            _resume.BadBlocks.Sort();

            foreach(ulong bad in _resume.BadBlocks)
                _dumpLog.WriteLine("Sector {0} could not be read.", bad);

            currentTry.Extents = ExtentsConverter.ToMetadata(extents);

            _outputPlugin.SetDumpHardware(_resume.Tries);

            if(_preSidecar != null)
                _outputPlugin.SetCicmMetadata(_preSidecar);

            _dumpLog.WriteLine("Closing output file.");
            UpdateStatus?.Invoke("Closing output file.");
            DateTime closeStart = DateTime.Now;
            _outputPlugin.Close();
            DateTime closeEnd = DateTime.Now;
            UpdateStatus?.Invoke($"Closed in {(closeEnd - closeStart).TotalSeconds} seconds.");

            if(_aborted)
            {
                _dumpLog.WriteLine("Aborted!");

                return;
            }

            double totalChkDuration = 0;

            if(_metadata)
                WriteOpticalSidecar(blockSize, blocks, dskType, null, mediaTags, sessions, out totalChkDuration);

            end = DateTime.UtcNow;
            UpdateStatus?.Invoke("");

            UpdateStatus?.
                Invoke($"Took a total of {(end - dumpStart).TotalSeconds:F3} seconds ({totalDuration / 1000:F3} processing commands, {totalChkDuration / 1000:F3} checksumming, {imageWriteDuration:F3} writing, {(closeEnd - closeStart).TotalSeconds:F3} closing).");

            UpdateStatus?.
                Invoke($"Average speed: {((double)blockSize * (double)(blocks + 1)) / 1048576 / (totalDuration / 1000):F3} MiB/sec.");

            UpdateStatus?.Invoke($"Fastest speed burst: {maxSpeed:F3} MiB/sec.");
            UpdateStatus?.Invoke($"Slowest speed burst: {minSpeed:F3} MiB/sec.");
            UpdateStatus?.Invoke($"{_resume.BadBlocks.Count} sectors could not be read.");
            UpdateStatus?.Invoke("");

            Statistics.AddMedia(dskType, true);
        }
    }
}