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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Extents;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using DiscImageChef.Core.Logging;
using DiscImageChef.Core.Media.Detection;
using DiscImageChef.Database;
using DiscImageChef.Decoders.CD;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Decoders.SCSI.MMC;
using DiscImageChef.Devices;
using Schemas;
using CdOffset = DiscImageChef.Database.Models.CdOffset;
using Device = DiscImageChef.Database.Models.Device;
using MediaType = DiscImageChef.CommonTypes.MediaType;
using PlatformID = DiscImageChef.CommonTypes.Interop.PlatformID;
using Session = DiscImageChef.Decoders.CD.Session;
using TrackType = DiscImageChef.CommonTypes.Enums.TrackType;

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

namespace DiscImageChef.Core.Devices.Dumping
{
    /// <summary>Implement dumping Compact Discs</summary>

    // TODO: Barcode and pregaps
    partial class Dump
    {
        /// <summary>Dumps a compact disc</summary>
        /// <param name="dskType">Disc type as detected in MMC layer</param>
        void CompactDisc(out MediaType dskType)
        {
            ulong                  blocks;                                           // Total number of positive sectors
            uint                   blockSize;                                        // Size of the read sector in bytes
            uint                   blocksToRead = 64;                                // How many sectors to read at once
            CdOffset               cdOffset;                                         // Read offset from database
            byte[]                 cmdBuf;                                           // Data buffer
            double                 cmdDuration = 0;                                  // Command execution time
            DicContext             ctx;                                              // Master database context
            DumpHardwareType       currentTry   = null;                              // Current dump hardware try
            double                 currentSpeed = 0;                                 // Current read speed
            Device                 dbDev;                                            // Device database entry
            DateTime               dumpStart = DateTime.UtcNow;                      // Time of dump start
            DateTime               end;                                              // Time of operation end
            ExtentsULong           extents        = null;                            // Extents
            TrackType              firstTrackType = TrackType.Audio;                 // Type of first track
            IbgLog                 ibgLog;                                           // IMGBurn log
            double                 imageWriteDuration = 0;                           // Duration of image write
            long                   lastSector         = 0;                           // Last sector number
            var                    leadOutExtents     = new ExtentsULong();          // Lead-out extents
            Dictionary<int, long>  leadOutStarts      = new Dictionary<int, long>(); // Lead-out starts
            double                 maxSpeed           = double.MinValue;             // Maximum speed
            MhddLog                mhddLog;                                          // MHDD log
            double                 minSpeed = double.MaxValue;                       // Minimum speed
            bool                   newTrim  = false;                                 // Is trim a new one?
            bool                   read6    = false;                                 // Device supports READ(6)
            bool                   read10   = false;                                 // Device supports READ(10)
            bool                   read12   = false;                                 // Device supports READ(12)
            bool                   read16   = false;                                 // Device supports READ(16)
            bool                   readcd;                                           // Device supports READ CD
            bool                   ret;                                              // Image writing return status
            const uint             sectorSize       = 2352;                          // Full sector size
            ulong                  sectorSpeedStart = 0;                             // Used to calculate correct speed
            bool                   sense;                                            // Sense indicator
            byte[]                 senseBuf;                                         // Sense buffer
            int                    sessions = 1;                                     // Number of sessions in disc
            DateTime               start;                                            // Start of operation
            uint                   subSize;                                          // Subchannel size in bytes
            TrackSubchannelType    subType;                                          // Track subchannel type
            bool                   supportsLongSectors = true;                       // Supports reading EDC and ECC
            byte[]                 tmpBuf;                                           // Temporary buffer
            FullTOC.CDFullTOC?     toc           = null;                             // Full CD TOC
            double                 totalDuration = 0;                                // Total commands duration
            Dictionary<byte, byte> trackFlags    = new Dictionary<byte, byte>();     // Track flags
            List<Track>            trackList     = new List<Track>();                // Tracks in disc
            Track[]                tracks;                                           // Tracks in disc as array

            Dictionary<MediaTagType, byte[]> mediaTags = new Dictionary<MediaTagType, byte[]>(); // Media tags

            int firstTrackLastSession = 0; // Number of first track in last session

            MmcSubchannel supportedSubchannel; // Drive's maximum supported subchannel

            DateTime timeSpeedStart; // Time of start for speed calculation

            dskType = MediaType.CD;

            if(_dumpRaw)
            {
                _dumpLog.WriteLine("Raw CD dumping not yet implemented");
                StoppingErrorMessage?.Invoke("Raw CD dumping not yet implemented");

                return;
            }

            // Open master database
            ctx = DicContext.Create(Settings.Settings.MasterDbPath);

            // Search for device in master database
            dbDev = ctx.Devices.FirstOrDefault(d => d.Manufacturer == _dev.Manufacturer && d.Model == _dev.Model &&
                                                    d.Revision     == _dev.Revision);

            if(dbDev is null)
            {
                _dumpLog.WriteLine("Device not in database, please create a device report and attach it to a Github issue.");

                UpdateStatus?.
                    Invoke("Device not in database, please create a device report and attach it to a Github issue.");
            }
            else
            {
                _dumpLog.WriteLine($"Device in database since {dbDev.LastSynchronized}.");
                UpdateStatus?.Invoke($"Device in database since {dbDev.LastSynchronized}.");

                if(dbDev.OptimalMultipleSectorsRead > 0)
                    blocksToRead = (uint)dbDev.OptimalMultipleSectorsRead;
            }

            // Search for read offset in master database
            cdOffset = ctx.CdOffsets.FirstOrDefault(d => d.Manufacturer == _dev.Manufacturer && d.Model == _dev.Model);

            if(cdOffset is null)
            {
                _dumpLog.WriteLine("CD reading offset not found in database.");
                UpdateStatus?.Invoke("CD reading offset not found in database.");
            }
            else
            {
                _dumpLog.WriteLine($"CD reading offset is {cdOffset.Offset} samples.");
                UpdateStatus?.Invoke($"CD reading offset is {cdOffset.Offset} samples.");
            }

            supportedSubchannel = MmcSubchannel.Raw;
            _dumpLog.WriteLine("Checking if drive supports full raw subchannel reading...");
            UpdateStatus?.Invoke("Checking if drive supports full raw subchannel reading...");

            readcd = !_dev.ReadCd(out cmdBuf, out senseBuf, 0, sectorSize + 96, 1, MmcSectorTypes.AllTypes, false,
                                  false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                  supportedSubchannel, _dev.Timeout, out _);

            if(readcd)
            {
                _dumpLog.WriteLine("Full raw subchannel reading supported...");
                UpdateStatus?.Invoke("Full raw subchannel reading supported...");
                subSize = 96;
            }
            else
            {
                supportedSubchannel = MmcSubchannel.Q16;
                _dumpLog.WriteLine("Checking if drive supports PQ subchannel reading...");
                UpdateStatus?.Invoke("Checking if drive supports PQ subchannel reading...");

                readcd = !_dev.ReadCd(out cmdBuf, out senseBuf, 0, sectorSize + 16, 1, MmcSectorTypes.AllTypes, false,
                                      false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                      supportedSubchannel, _dev.Timeout, out _);

                if(readcd)
                {
                    _dumpLog.WriteLine("PQ subchannel reading supported...");
                    _dumpLog.WriteLine("WARNING: If disc says CD+G, CD+EG, CD-MIDI, CD Graphics or CD Enhanced Graphics, dump will be incorrect!");
                    UpdateStatus?.Invoke("PQ subchannel reading supported...");

                    UpdateStatus?.
                        Invoke("WARNING: If disc says CD+G, CD+EG, CD-MIDI, CD Graphics or CD Enhanced Graphics, dump will be incorrect!");

                    subSize = 16;
                }
                else
                {
                    supportedSubchannel = MmcSubchannel.None;
                    _dumpLog.WriteLine("Checking if drive supports reading without subchannel...");
                    UpdateStatus?.Invoke("Checking if drive supports reading without subchannel...");

                    readcd = !_dev.ReadCd(out cmdBuf, out senseBuf, 0, sectorSize, 1, MmcSectorTypes.AllTypes, false,
                                          false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                          supportedSubchannel, _dev.Timeout, out _);

                    if(!readcd)
                    {
                        _dumpLog.WriteLine("Drive does not support READ CD, trying SCSI READ commands...");
                        ErrorMessage?.Invoke("Drive does not support READ CD, trying SCSI READ commands...");

                        _dumpLog.WriteLine("Checking if drive supports READ(6)...");
                        UpdateStatus?.Invoke("Checking if drive supports READ(6)...");
                        read6 = !_dev.Read6(out cmdBuf, out senseBuf, 0, 2048, 1, _dev.Timeout, out _);
                        _dumpLog.WriteLine("Checking if drive supports READ(10)...");
                        UpdateStatus?.Invoke("Checking if drive supports READ(10)...");

                        read10 = !_dev.Read10(out cmdBuf, out senseBuf, 0, false, true, false, false, 0, 2048, 0, 1,
                                              _dev.Timeout, out _);

                        _dumpLog.WriteLine("Checking if drive supports READ(12)...");
                        UpdateStatus?.Invoke("Checking if drive supports READ(12)...");

                        read12 = !_dev.Read12(out cmdBuf, out senseBuf, 0, false, true, false, false, 0, 2048, 0, 1,
                                              false, _dev.Timeout, out _);

                        _dumpLog.WriteLine("Checking if drive supports READ(16)...");
                        UpdateStatus?.Invoke("Checking if drive supports READ(16)...");

                        read16 = !_dev.Read16(out cmdBuf, out senseBuf, 0, false, true, false, 0, 2048, 0, 1, false,
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

                    _dumpLog.WriteLine("Drive can only read without subchannel...");
                    _dumpLog.WriteLine("WARNING: If disc says CD+G, CD+EG, CD-MIDI, CD Graphics or CD Enhanced Graphics, dump will be incorrect!");
                    UpdateStatus?.Invoke("Drive can only read without subchannel...");

                    UpdateStatus?.
                        Invoke("WARNING: If disc says CD+G, CD+EG, CD-MIDI, CD Graphics or CD Enhanced Graphics, dump will be incorrect!");

                    subSize = 0;
                }
            }

            // Check if output format supports subchannels
            if(!_outputPlugin.SupportedSectorTags.Contains(SectorTagType.CdSectorSubchannel) &&
               supportedSubchannel != MmcSubchannel.None)
            {
                if(!_force)
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
                subSize             = 0;
            }

            blockSize = sectorSize + subSize;

            switch(supportedSubchannel)
            {
                case MmcSubchannel.None:
                    subType = TrackSubchannelType.None;

                    break;
                case MmcSubchannel.Raw:
                    subType = TrackSubchannelType.Raw;

                    break;
                case MmcSubchannel.Q16:
                    subType = TrackSubchannelType.Q16;

                    break;
                default:
                    _dumpLog.WriteLine("Handling subchannel type {0} not supported, exiting...", supportedSubchannel);

                    StoppingErrorMessage?.
                        Invoke($"Handling subchannel type {supportedSubchannel} not supported, exiting...");

                    return;
            }

            // We discarded all discs that falsify a TOC before requesting a real TOC
            // No TOC, no CD (or an empty one)
            _dumpLog.WriteLine("Reading full TOC");
            UpdateStatus?.Invoke("Reading full TOC");
            sense = _dev.ReadRawToc(out cmdBuf, out senseBuf, 0, _dev.Timeout, out _);

            if(!sense)
            {
                toc = FullTOC.Decode(cmdBuf);

                if(toc.HasValue)
                {
                    tmpBuf = new byte[cmdBuf.Length - 2];
                    Array.Copy(cmdBuf, 2, tmpBuf, 0, cmdBuf.Length - 2);
                    mediaTags.Add(MediaTagType.CD_FullTOC, tmpBuf);
                }
            }

            UpdateStatus?.Invoke("Building track map...");
            _dumpLog.WriteLine("Building track map...");

            if(toc.HasValue)
            {
                FullTOC.TrackDataDescriptor[] sortedTracks =
                    toc.Value.TrackDescriptors.OrderBy(track => track.POINT).ToArray();

                foreach(FullTOC.TrackDataDescriptor trk in sortedTracks.Where(trk => trk.ADR == 1 || trk.ADR == 4))
                    if(trk.POINT >= 0x01 &&
                       trk.POINT <= 0x63)
                    {
                        trackList.Add(new Track
                        {
                            TrackSequence = trk.POINT, TrackSession = trk.SessionNumber,
                            TrackType = (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrack ||
                                        (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrackIncremental
                                            ? TrackType.Data : TrackType.Audio,
                            TrackStartSector =
                                (ulong)(((trk.PHOUR * 3600 * 75) + (trk.PMIN * 60 * 75) + (trk.PSEC * 75) +
                                         trk.PFRAME) - 150),
                            TrackBytesPerSector    = (int)sectorSize,
                            TrackRawBytesPerSector = (int)sectorSize,
                            TrackSubchannelType    = subType
                        });

                        trackFlags.Add(trk.POINT, trk.CONTROL);
                    }
                    else if(trk.POINT == 0xA2)
                    {
                        int phour, pmin, psec, pframe;

                        if(trk.PFRAME == 0)
                        {
                            pframe = 74;

                            if(trk.PSEC == 0)
                            {
                                psec = 59;

                                if(trk.PMIN == 0)
                                {
                                    pmin  = 59;
                                    phour = trk.PHOUR - 1;
                                }
                                else
                                {
                                    pmin  = trk.PMIN - 1;
                                    phour = trk.PHOUR;
                                }
                            }
                            else
                            {
                                psec  = trk.PSEC - 1;
                                pmin  = trk.PMIN;
                                phour = trk.PHOUR;
                            }
                        }
                        else
                        {
                            pframe = trk.PFRAME - 1;
                            psec   = trk.PSEC;
                            pmin   = trk.PMIN;
                            phour  = trk.PHOUR;
                        }

                        lastSector = ((phour * 3600 * 75) + (pmin * 60 * 75) + (psec * 75) + pframe) - 150;
                        leadOutStarts.Add(trk.SessionNumber, lastSector                    + 1);
                    }
                    else if(trk.POINT == 0xA0 &&
                            trk.ADR   == 1)
                    {
                        switch(trk.PSEC)
                        {
                            case 0x10:
                                dskType = MediaType.CDI;

                                break;
                            case 0x20:
                                if(dskType == MediaType.CD ||
                                   dskType == MediaType.CDROM)
                                    dskType = MediaType.CDROMXA;

                                break;
                        }

                        firstTrackType =
                            (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrack ||
                            (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrackIncremental ? TrackType.Data
                                : TrackType.Audio;
                    }
            }
            else
            {
                UpdateStatus?.Invoke("Cannot read RAW TOC, requesting processed one...");
                _dumpLog.WriteLine("Cannot read RAW TOC, requesting processed one...");
                sense = _dev.ReadToc(out cmdBuf, out senseBuf, false, 0, _dev.Timeout, out _);

                TOC.CDTOC? oldToc = TOC.Decode(cmdBuf);

                if((sense || !oldToc.HasValue) &&
                   !_force)
                {
                    _dumpLog.WriteLine("Could not read TOC, if you want to continue, use force, and will try from LBA 0 to 360000...");

                    StoppingErrorMessage?.
                        Invoke("Could not read TOC, if you want to continue, use force, and will try from LBA 0 to 360000...");

                    return;
                }

                foreach(TOC.CDTOCTrackDataDescriptor trk in oldToc.
                                                            Value.TrackDescriptors.OrderBy(t => t.TrackNumber).
                                                            Where(trk => trk.ADR == 1 || trk.ADR == 4))
                    if(trk.TrackNumber >= 0x01 &&
                       trk.TrackNumber <= 0x63)
                    {
                        trackList.Add(new Track
                        {
                            TrackSequence = trk.TrackNumber, TrackSession = 1,
                            TrackType = (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrack ||
                                        (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrackIncremental
                                            ? TrackType.Data : TrackType.Audio,
                            TrackStartSector       = trk.TrackStartAddress, TrackBytesPerSector = (int)sectorSize,
                            TrackRawBytesPerSector = (int)sectorSize, TrackSubchannelType       = subType
                        });

                        trackFlags.Add(trk.TrackNumber, trk.CONTROL);
                    }
                    else if(trk.TrackNumber == 0xAA)
                    {
                        firstTrackType =
                            (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrack ||
                            (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrackIncremental ? TrackType.Data
                                : TrackType.Audio;

                        lastSector = trk.TrackStartAddress - 1;
                    }
            }

            if(trackList.Count == 0)
            {
                UpdateStatus?.Invoke("No tracks found, adding a single track from 0 to Lead-Out");
                _dumpLog.WriteLine("No tracks found, adding a single track from 0 to Lead-Out");

                trackList.Add(new Track
                {
                    TrackSequence       = 1, TrackSession = 1, TrackType = firstTrackType,
                    TrackStartSector    = 0,
                    TrackBytesPerSector = (int)sectorSize, TrackRawBytesPerSector = (int)sectorSize,
                    TrackSubchannelType = subType
                });

                trackFlags.Add(1, (byte)(firstTrackType == TrackType.Audio ? 0 : 4));
            }

            if(lastSector == 0)
            {
                sense = _dev.ReadCapacity16(out cmdBuf, out senseBuf, _dev.Timeout, out _);

                if(!sense)
                {
                    byte[] temp = new byte[8];

                    Array.Copy(cmdBuf, 0, temp, 0, 8);
                    Array.Reverse(temp);
                    lastSector = (long)BitConverter.ToUInt64(temp, 0);
                    blockSize  = (uint)((cmdBuf[5] << 24) + (cmdBuf[5] << 16) + (cmdBuf[6] << 8) + cmdBuf[7]);
                }
                else
                {
                    sense = _dev.ReadCapacity(out cmdBuf, out senseBuf, _dev.Timeout, out _);

                    if(!sense)
                    {
                        lastSector = (cmdBuf[0] << 24) + (cmdBuf[1] << 16) + (cmdBuf[2] << 8) + cmdBuf[3];
                        blockSize  = (uint)((cmdBuf[5] << 24) + (cmdBuf[5] << 16) + (cmdBuf[6] << 8) + cmdBuf[7]);
                    }
                }

                if(lastSector <= 0)
                {
                    if(!_force)
                    {
                        StoppingErrorMessage?.
                            Invoke("Could not find Lead-Out, if you want to continue use force option and will continue until 360000 sectors...");

                        _dumpLog.
                            WriteLine("Could not find Lead-Out, if you want to continue use force option and will continue until 360000 sectors...");

                        return;
                    }

                    UpdateStatus?.
                        Invoke("WARNING: Could not find Lead-Out start, will try to read up to 360000 sectors, probably will fail before...");

                    _dumpLog.WriteLine("WARNING: Could not find Lead-Out start, will try to read up to 360000 sectors, probably will fail before...");
                    lastSector = 360000;
                }
            }

            tracks = trackList.ToArray();

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

            // ATIP exists on blank CDs
            _dumpLog.WriteLine("Reading ATIP");
            UpdateStatus?.Invoke("Reading ATIP");
            sense = _dev.ReadAtip(out cmdBuf, out senseBuf, _dev.Timeout, out _);

            if(!sense)
            {
                ATIP.CDATIP? atip = ATIP.Decode(cmdBuf);

                if(atip.HasValue)
                {
                    // Only CD-R and CD-RW have ATIP
                    dskType = atip.Value.DiscType ? MediaType.CDRW : MediaType.CDR;

                    tmpBuf = new byte[cmdBuf.Length - 4];
                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                    mediaTags.Add(MediaTagType.CD_ATIP, tmpBuf);
                }
            }

            _dumpLog.WriteLine("Reading Disc Information");
            UpdateStatus?.Invoke("Reading Disc Information");

            sense = _dev.ReadDiscInformation(out cmdBuf, out senseBuf, MmcDiscInformationDataTypes.DiscInformation,
                                             _dev.Timeout, out _);

            if(!sense)
            {
                DiscInformation.StandardDiscInformation? discInfo = DiscInformation.Decode000b(cmdBuf);

                if(discInfo.HasValue &&
                   dskType == MediaType.CD)
                    switch(discInfo.Value.DiscType)
                    {
                        case 0x10:
                            dskType = MediaType.CDI;

                            break;
                        case 0x20:
                            dskType = MediaType.CDROMXA;

                            break;
                    }
            }

            _dumpLog.WriteLine("Reading PMA");
            UpdateStatus?.Invoke("Reading PMA");
            sense = _dev.ReadPma(out cmdBuf, out senseBuf, _dev.Timeout, out _);

            if(!sense &&
               PMA.Decode(cmdBuf).HasValue)
            {
                tmpBuf = new byte[cmdBuf.Length - 4];
                Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                mediaTags.Add(MediaTagType.CD_PMA, tmpBuf);
            }

            _dumpLog.WriteLine("Reading Session Information");
            UpdateStatus?.Invoke("Reading Session Information");
            sense = _dev.ReadSessionInfo(out cmdBuf, out senseBuf, _dev.Timeout, out _);

            if(!sense)
            {
                Session.CDSessionInfo? session = Session.Decode(cmdBuf);

                if(session.HasValue)
                {
                    sessions              = session.Value.LastCompleteSession;
                    firstTrackLastSession = session.Value.TrackDescriptors[0].TrackNumber;
                }
            }

            _dumpLog.WriteLine("Reading CD-Text from Lead-In");
            UpdateStatus?.Invoke("Reading CD-Text from Lead-In");
            sense = _dev.ReadCdText(out cmdBuf, out senseBuf, _dev.Timeout, out _);

            if(!sense &&
               CDTextOnLeadIn.Decode(cmdBuf).HasValue)
            {
                tmpBuf = new byte[cmdBuf.Length - 4];
                Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                mediaTags.Add(MediaTagType.CD_TEXT, tmpBuf);
            }

            // Check if output format supports all disc tags we have retrieved so far
            foreach(MediaTagType tag in mediaTags.Keys)
            {
                if(_outputPlugin.SupportedMediaTags.Contains(tag))
                    continue;

                if(!_force)
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

            // Check for hidden data before start of track 1
            if(tracks.First(t => t.TrackSequence == 1).TrackStartSector > 0 && readcd)
            {
                _dumpLog.WriteLine("First track starts after sector 0, checking for a hidden track...");
                UpdateStatus?.Invoke("First track starts after sector 0, checking for a hidden track...");

                sense = _dev.ReadCd(out cmdBuf, out senseBuf, 0, blockSize, 1, MmcSectorTypes.AllTypes, false, false,
                                    true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                    supportedSubchannel, _dev.Timeout, out _);

                if(_dev.Error || sense)
                {
                    _dumpLog.WriteLine("Could not read sector 0, continuing...");
                    UpdateStatus?.Invoke("Could not read sector 0, continuing...");
                }
                else
                {
                    byte[] syncMark =
                    {
                        0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00
                    };

                    byte[] cdiMark =
                    {
                        0x01, 0x43, 0x44, 0x2D
                    };

                    byte[] testMark = new byte[12];
                    Array.Copy(cmdBuf, 0, testMark, 0, 12);

                    bool hiddenData = syncMark.SequenceEqual(testMark) &&
                                      (cmdBuf[0xF] == 0 || cmdBuf[0xF] == 1 || cmdBuf[0xF] == 2);

                    if(hiddenData && cmdBuf[0xF] == 2)
                    {
                        sense = _dev.ReadCd(out cmdBuf, out senseBuf, 16, blockSize, 1, MmcSectorTypes.AllTypes, false,
                                            false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                            supportedSubchannel, _dev.Timeout, out _);

                        if(!_dev.Error &&
                           !sense)
                        {
                            testMark = new byte[4];
                            Array.Copy(cmdBuf, 24, testMark, 0, 4);

                            if(cdiMark.SequenceEqual(testMark))
                                dskType = MediaType.CDIREADY;
                        }

                        List<Track> trkList = new List<Track>
                        {
                            new Track
                            {
                                TrackSequence          = 0,
                                TrackSession           = 1,
                                TrackType              = hiddenData ? TrackType.Data : TrackType.Audio,
                                TrackStartSector       = 0,
                                TrackBytesPerSector    = (int)sectorSize,
                                TrackRawBytesPerSector = (int)sectorSize,
                                TrackSubchannelType    = subType,
                                TrackEndSector         = tracks.First(t => t.TrackSequence == 1).TrackStartSector - 1
                            }
                        };

                        trkList.AddRange(tracks);
                        tracks = trkList.ToArray();
                    }
                }
            }

            if(dskType == MediaType.CD ||
               dskType == MediaType.CDROMXA)
            {
                // TODO: Add other detectors here
                _dumpLog.WriteLine("Detecting disc type...");
                UpdateStatus?.Invoke("Detecting disc type...");

                bool hasDataTrack                  = false;
                bool hasAudioTrack                 = false;
                bool allFirstSessionTracksAreAudio = true;
                bool hasVideoTrack                 = false;

                foreach(FullTOC.TrackDataDescriptor track in toc.Value.TrackDescriptors)
                {
                    if(track.TNO == 1 &&
                       ((TocControl)(track.CONTROL & 0x0D) == TocControl.DataTrack ||
                        (TocControl)(track.CONTROL & 0x0D) == TocControl.DataTrackIncremental))
                        allFirstSessionTracksAreAudio &= firstTrackLastSession != 1;

                    if((TocControl)(track.CONTROL & 0x0D) == TocControl.DataTrack ||
                       (TocControl)(track.CONTROL & 0x0D) == TocControl.DataTrackIncremental)
                    {
                        hasDataTrack                  =  true;
                        allFirstSessionTracksAreAudio &= track.POINT >= firstTrackLastSession;
                    }
                    else
                    {
                        hasAudioTrack = true;
                    }

                    hasVideoTrack |= track.ADR == 4;
                }

                if(hasDataTrack                  &&
                   hasAudioTrack                 &&
                   allFirstSessionTracksAreAudio &&
                   sessions == 2)
                    dskType = MediaType.CDPLUS;

                if(!hasDataTrack &&
                   hasAudioTrack &&
                   sessions == 1)
                    dskType = MediaType.CDDA;

                if(hasDataTrack   &&
                   !hasAudioTrack &&
                   sessions == 1)
                    dskType = MediaType.CDROM;

                if(hasVideoTrack &&
                   !hasDataTrack &&
                   sessions == 1)
                    dskType = MediaType.CDV;

                byte[] videoNowColorFrame = new byte[9 * 2352];

                for(int i = 0; i < 9; i++)
                {
                    sense = _dev.ReadCd(out cmdBuf, out senseBuf, (uint)i, 2352, 1, MmcSectorTypes.AllTypes, false,
                                        false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                        MmcSubchannel.None, _dev.Timeout, out _);

                    if(sense || _dev.Error)
                    {
                        sense = _dev.ReadCd(out cmdBuf, out senseBuf, (uint)i, 2352, 1, MmcSectorTypes.Cdda, false,
                                            false, true, MmcHeaderCodes.None, true, true, MmcErrorField.None,
                                            MmcSubchannel.None, _dev.Timeout, out _);

                        if(sense || !_dev.Error)
                        {
                            videoNowColorFrame = null;

                            break;
                        }
                    }

                    Array.Copy(cmdBuf, 0, videoNowColorFrame, i * 2352, 2352);
                }

                if(MMC.IsVideoNowColor(videoNowColorFrame))
                    dskType = MediaType.VideoNowColor;
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

                sense = !_dev.ReadCd(out cmdBuf, out senseBuf, (uint)tracks[t].TrackStartSector, blockSize, 1,
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
            {
                bool gotFirstTrackPregap         = false;
                int  firstTrackPregapSectorsGood = 0;
                var  firstTrackPregapMs          = new MemoryStream();

                cmdBuf = null;

                _dumpLog.WriteLine("Reading first track pregap");
                UpdateStatus?.Invoke("Reading first track pregap");
                InitProgress?.Invoke();
                timeSpeedStart = DateTime.UtcNow;

                for(int firstTrackPregapBlock = -150; firstTrackPregapBlock < 0 && _resume.NextBlock == 0;
                    firstTrackPregapBlock++)
                {
                    if(_aborted)
                    {
                        _dumpLog.WriteLine("Aborted!");
                        UpdateStatus?.Invoke("Aborted!");

                        break;
                    }

                    PulseProgress?.
                        Invoke($"Trying to read first track pregap sector {firstTrackPregapBlock} ({currentSpeed:F3} MiB/sec.)");

                    sense = _dev.ReadCd(out cmdBuf, out senseBuf, (uint)firstTrackPregapBlock, blockSize, 1,
                                        MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true,
                                        true, MmcErrorField.None, supportedSubchannel, _dev.Timeout, out cmdDuration);

                    if(!sense &&
                       !_dev.Error)
                    {
                        firstTrackPregapMs.Write(cmdBuf, 0, (int)blockSize);
                        gotFirstTrackPregap = true;
                        firstTrackPregapSectorsGood++;
                        totalDuration += cmdDuration;
                    }
                    else
                    {
                        // Write empty data
                        if(gotFirstTrackPregap)
                            firstTrackPregapMs.Write(new byte[blockSize], 0, (int)blockSize);
                    }

                    sectorSpeedStart++;

                    double elapsed = (DateTime.UtcNow - timeSpeedStart).TotalSeconds;

                    if(elapsed < 1)
                        continue;

                    currentSpeed     = (sectorSpeedStart * blockSize) / (1048576 * elapsed);
                    sectorSpeedStart = 0;
                    timeSpeedStart   = DateTime.UtcNow;
                }

                if(firstTrackPregapSectorsGood > 0)
                    mediaTags.Add(MediaTagType.CD_FirstTrackPregap, firstTrackPregapMs.ToArray());

                EndProgress?.Invoke();
                UpdateStatus?.Invoke($"Got {firstTrackPregapSectorsGood} first track pregap sectors.");
                _dumpLog.WriteLine("Got {0} first track pregap sectors.", firstTrackPregapSectorsGood);

                firstTrackPregapMs.Close();
            }

            // Try how many blocks are readable at once
            while(true)
            {
                if(readcd)
                {
                    sense = _dev.ReadCd(out cmdBuf, out senseBuf, 0, blockSize, blocksToRead, MmcSectorTypes.AllTypes,
                                        false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                        supportedSubchannel, _dev.Timeout, out _);

                    if(_dev.Error || sense)
                        blocksToRead /= 2;
                }
                else if(read16)
                {
                    sense = _dev.Read16(out cmdBuf, out senseBuf, 0, false, true, false, 0, blockSize, 0, blocksToRead,
                                        false, _dev.Timeout, out _);

                    if(_dev.Error || sense)
                        blocksToRead /= 2;
                }
                else if(read12)
                {
                    sense = _dev.Read12(out cmdBuf, out senseBuf, 0, false, true, false, false, 0, blockSize, 0,
                                        blocksToRead, false, _dev.Timeout, out _);

                    if(_dev.Error || sense)
                        blocksToRead /= 2;
                }
                else if(read10)
                {
                    sense = _dev.Read10(out cmdBuf, out senseBuf, 0, false, true, false, false, 0, blockSize, 0,
                                        (ushort)blocksToRead, _dev.Timeout, out _);

                    if(_dev.Error || sense)
                        blocksToRead /= 2;
                }
                else if(read6)
                {
                    sense = _dev.Read6(out cmdBuf, out senseBuf, 0, blockSize, (byte)blocksToRead, _dev.Timeout, out _);

                    if(_dev.Error || sense)
                        blocksToRead /= 2;
                }

                if(!_dev.Error ||
                   blocksToRead == 1)
                    break;
            }

            if(_dev.Error || sense)
            {
                _dumpLog.WriteLine("Device error {0} trying to guess ideal transfer length.", _dev.LastError);
                StoppingErrorMessage?.Invoke($"Device error {_dev.LastError} trying to guess ideal transfer length.");
            }

            _dumpLog.WriteLine("Reading {0} sectors at a time.", blocksToRead);
            _dumpLog.WriteLine("Device reports {0} blocks ({1} bytes).", blocks, blocks * blockSize);
            _dumpLog.WriteLine("Device can read {0} blocks at a time.", blocksToRead);
            _dumpLog.WriteLine("Device reports {0} bytes per logical block.", blockSize);
            _dumpLog.WriteLine("SCSI device type: {0}.", _dev.ScsiType);
            _dumpLog.WriteLine("Media identified as {0}.", dskType);

            UpdateStatus?.Invoke($"Reading {blocksToRead} sectors at a time.");
            UpdateStatus?.Invoke($"Device reports {blocks} blocks ({blocks * blockSize} bytes).");
            UpdateStatus?.Invoke($"Device can read {blocksToRead} blocks at a time.");
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
                _dev.ReadCd(out cmdBuf, out senseBuf, 0, blockSize, 1, MmcSectorTypes.AllTypes, false, false, true,
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

            if(_skip < blocksToRead)
                _skip = blocksToRead;

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

            mhddLog = new MhddLog(_outputPrefix + ".mhddlog.bin", _dev, blocks, blockSize, blocksToRead);
            ibgLog  = new IbgLog(_outputPrefix  + ".ibg", 0x0008);

            // Start reading
            start            = DateTime.UtcNow;
            currentSpeed     = 0;
            sectorSpeedStart = 0;
            timeSpeedStart   = DateTime.UtcNow;
            InitProgress?.Invoke();

            for(long i = (long)_resume.NextBlock; i <= lastSector; i += blocksToRead)
            {
                if(_aborted)
                {
                    currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                    UpdateStatus?.Invoke("Aborted!");
                    _dumpLog.WriteLine("Aborted!");

                    break;
                }

                ulong ui = (ulong)i;

                if((lastSector + 1) - i < blocksToRead)
                    blocksToRead = (uint)((lastSector + 1) - i);

                #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator

                // ReSharper disable CompareOfFloatsByEqualityOperator
                if(currentSpeed > maxSpeed &&
                   currentSpeed != 0)
                    maxSpeed = currentSpeed;

                if(currentSpeed < minSpeed &&
                   currentSpeed != 0)
                    minSpeed = currentSpeed;
                #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                // ReSharper restore CompareOfFloatsByEqualityOperator

                UpdateProgress?.Invoke($"Reading sector {i} of {blocks} ({currentSpeed:F3} MiB/sec.)", i, (long)blocks);

                if(readcd)
                {
                    sense = _dev.ReadCd(out cmdBuf, out senseBuf, (uint)i, blockSize, blocksToRead,
                                        MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true,
                                        true, MmcErrorField.None, supportedSubchannel, _dev.Timeout, out cmdDuration);

                    totalDuration += cmdDuration;
                }
                else if(read16)
                {
                    sense = _dev.Read16(out cmdBuf, out senseBuf, 0, false, true, false, ui, blockSize, 0, blocksToRead,
                                        false, _dev.Timeout, out cmdDuration);
                }
                else if(read12)
                {
                    sense = _dev.Read12(out cmdBuf, out senseBuf, 0, false, true, false, false, (uint)i, blockSize, 0,
                                        blocksToRead, false, _dev.Timeout, out cmdDuration);
                }
                else if(read10)
                {
                    sense = _dev.Read10(out cmdBuf, out senseBuf, 0, false, true, false, false, (uint)i, blockSize, 0,
                                        (ushort)blocksToRead, _dev.Timeout, out cmdDuration);
                }
                else if(read6)
                {
                    sense = _dev.Read6(out cmdBuf, out senseBuf, (uint)i, blockSize, (byte)blocksToRead, _dev.Timeout,
                                       out cmdDuration);
                }

                if(!sense &&
                   !_dev.Error)
                {
                    mhddLog.Write(ui, cmdDuration);
                    ibgLog.Write(ui, currentSpeed * 1024);
                    extents.Add(ui, blocksToRead, true);
                    DateTime writeStart = DateTime.Now;

                    if(supportedSubchannel != MmcSubchannel.None)
                    {
                        byte[] data = new byte[sectorSize * blocksToRead];
                        byte[] sub  = new byte[subSize    * blocksToRead];

                        for(int b = 0; b < blocksToRead; b++)
                        {
                            Array.Copy(cmdBuf, (int)(0 + (b * blockSize)), data, sectorSize * b, sectorSize);

                            Array.Copy(cmdBuf, (int)(sectorSize + (b * blockSize)), sub, subSize * b, subSize);
                        }

                        _outputPlugin.WriteSectorsLong(data, ui, blocksToRead);
                        _outputPlugin.WriteSectorsTag(sub, ui, blocksToRead, SectorTagType.CdSectorSubchannel);
                    }
                    else
                    {
                        if(supportsLongSectors)
                        {
                            _outputPlugin.WriteSectorsLong(cmdBuf, ui, blocksToRead);
                        }
                        else
                        {
                            if(cmdBuf.Length % 2352 == 0)
                            {
                                byte[] data = new byte[2048 * blocksToRead];

                                for(int b = 0; b < blocksToRead; b++)
                                    Array.Copy(cmdBuf, (int)(16 + (b * blockSize)), data, 2048 * b, 2048);

                                _outputPlugin.WriteSectors(data, ui, blocksToRead);
                            }
                            else
                            {
                                _outputPlugin.WriteSectorsLong(cmdBuf, ui, blocksToRead);
                            }
                        }
                    }

                    imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                }
                else
                {
                    // TODO: Reset device after X errors
                    if(_stopOnError)
                        return; // TODO: Return more cleanly

                    if(ui + _skip > blocks)
                        _skip = (uint)(blocks - ui);

                    // Write empty data
                    DateTime writeStart = DateTime.Now;

                    if(supportedSubchannel != MmcSubchannel.None)
                    {
                        _outputPlugin.WriteSectorsLong(new byte[sectorSize * _skip], ui, _skip);

                        _outputPlugin.WriteSectorsTag(new byte[subSize * _skip], ui, _skip,
                                                      SectorTagType.CdSectorSubchannel);
                    }
                    else
                    {
                        if(supportsLongSectors)
                        {
                            _outputPlugin.WriteSectorsLong(new byte[blockSize * _skip], ui, _skip);
                        }
                        else
                        {
                            if(cmdBuf.Length % 2352 == 0)
                                _outputPlugin.WriteSectors(new byte[2048 * _skip], ui, _skip);
                            else
                                _outputPlugin.WriteSectorsLong(new byte[blockSize * _skip], ui, _skip);
                        }
                    }

                    imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;

                    for(ulong b = ui; b < ui + _skip; b++)
                        _resume.BadBlocks.Add(b);

                    DicConsole.DebugWriteLine("Dump-Media", "READ error:\n{0}", Sense.PrettifySense(senseBuf));
                    mhddLog.Write(ui, cmdDuration < 500 ? 65535 : cmdDuration);

                    ibgLog.Write(ui, 0);
                    _dumpLog.WriteLine("Skipping {0} blocks from errored block {1}.", _skip, i);
                    i       += _skip - blocksToRead;
                    newTrim =  true;
                }

                sectorSpeedStart += blocksToRead;

                _resume.NextBlock = ui + blocksToRead;

                double elapsed = (DateTime.UtcNow - timeSpeedStart).TotalSeconds;

                if(elapsed < 1)
                    continue;

                currentSpeed     = (sectorSpeedStart * blockSize) / (1048576 * elapsed);
                sectorSpeedStart = 0;
                timeSpeedStart   = DateTime.UtcNow;
            }

            EndProgress?.Invoke();

            // TODO: Enable when underlying images support lead-outs
            /*
            if(persistent)
            {
                UpdateStatus?.Invoke("Reading lead-outs");
                dumpLog.WriteLine("Reading lead-outs");

                InitProgress?.Invoke();
                foreach(Tuple<ulong, ulong> leadout in leadOutExtents.ToArray())
                for(ulong i = leadout.Item1; i <= leadout.Item2; i++)
                {
                    if(aborted)
                    {
                        currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                        dumpLog.WriteLine("Aborted!");
                        break;
                    }

                    double cmdDuration = 0;

                    #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                    if(currentSpeed > maxSpeed && currentSpeed != 0) maxSpeed = currentSpeed;
                    if(currentSpeed < minSpeed && currentSpeed != 0) minSpeed = currentSpeed;
                    #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                    PulseProgress?.Invoke(string.Format("Reading sector {0} at lead-out ({1:F3} MiB/sec.)", i, blocks,
                                     currentSpeed));

                    if(readcd)
                    {
                        sense = dev.ReadCd(out cmdBuf, out senseBuf, (uint)i, blockSize, 1,
                                           MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                                           true, true, MmcErrorField.None, supportedSubchannel, dev.Timeout,
                                           out cmdDuration);
                        totalDuration += cmdDuration;
                    }
                    else if(read16)
                        sense = dev.Read16(out cmdBuf, out senseBuf, 0, false, true, false, i, blockSize, 0, 1,
                                           false, dev.Timeout, out cmdDuration);
                    else if(read12)
                        sense = dev.Read12(out cmdBuf, out senseBuf, 0, false, true, false, false, (uint)i,
                                           blockSize, 0, 1, false, dev.Timeout, out cmdDuration);
                    else if(read10)
                        sense = dev.Read10(out cmdBuf, out senseBuf, 0, false, true, false, false, (uint)i,
                                           blockSize, 0, 1, dev.Timeout, out cmdDuration);
                    else if(read6)
                        sense = dev.Read6(out cmdBuf, out senseBuf, (uint)i, blockSize, 1, dev.Timeout,
                                          out cmdDuration);

                    if(!sense && !dev.Error)
                    {
                        mhddLog.Write(i, cmdDuration);
                        ibgLog.Write(i, currentSpeed * 1024);
                        extents.Add(i, blocksToRead, true);
                        leadOutExtents.Remove(i);
                        DateTime writeStart = DateTime.Now;
                        if(supportedSubchannel != MmcSubchannel.None)
                        {
                            byte[] data = new byte[SECTOR_SIZE * blocksToRead];
                            byte[] sub  = new byte[subSize     * blocksToRead];

                            for(int b = 0; b < blocksToRead; b++)
                            {
                                Array.Copy(cmdBuf, (int)(0 + b * blockSize), data, SECTOR_SIZE * b,
                                           SECTOR_SIZE);
                                Array.Copy(cmdBuf, (int)(SECTOR_SIZE + b * blockSize), sub, subSize * b,
                                           subSize);
                            }

                            outputPlugin.WriteSectorsLong(data, i, blocksToRead);
                            outputPlugin.WriteSectorsTag(sub, i, blocksToRead, SectorTagType.CdSectorSubchannel);
                        }
                        else outputPlugin.WriteSectors(cmdBuf, i, blocksToRead);

                        imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                    }
                    else
                    {
                        // TODO: Reset device after X errors
                        if(stopOnError) return; // TODO: Return more cleanly

                        // Write empty data
                        DateTime writeStart = DateTime.Now;
                        if(supportedSubchannel != MmcSubchannel.None)
                        {
                            outputPlugin.WriteSectorsLong(new byte[SECTOR_SIZE * skip], i, 1);
                            outputPlugin.WriteSectorsTag(new byte[subSize * skip], i, 1,
                                                         SectorTagType.CdSectorSubchannel);
                        }
                        else outputPlugin.WriteSectors(new byte[blockSize * skip], i, 1);

                        imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;

                        mhddLog.Write(i, cmdDuration < 500 ? 65535 : cmdDuration);

                        ibgLog.Write(i, 0);
                    }

                    double newSpeed =
                        (double)blockSize * blocksToRead / 1048576 / (cmdDuration / 1000);
                    if(!double.IsInfinity(newSpeed)) currentSpeed = newSpeed;
                    resume.NextBlock = i + 1;
                }

                EndProgress?.Invoke();
            }*/

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

            #region Compact Disc Error trimming
            if(_resume.BadBlocks.Count > 0 &&
               !_aborted                   &&
               !_notrim                    &&
               newTrim)
            {
                start = DateTime.UtcNow;
                UpdateStatus?.Invoke("Trimming bad sectors");
                _dumpLog.WriteLine("Trimming bad sectors");

                ulong[] tmpArray = _resume.BadBlocks.ToArray();
                InitProgress?.Invoke();

                foreach(ulong badSector in tmpArray)
                {
                    if(_aborted)
                    {
                        currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                        UpdateStatus?.Invoke("Aborted!");
                        _dumpLog.WriteLine("Aborted!");

                        break;
                    }

                    PulseProgress?.Invoke($"Trimming sector {badSector}");

                    if(readcd)
                        sense = _dev.ReadCd(out cmdBuf, out senseBuf, (uint)badSector, blockSize, 1,
                                            MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                                            true, true, MmcErrorField.None, supportedSubchannel, _dev.Timeout,
                                            out cmdDuration);
                    else if(read16)
                        sense = _dev.Read16(out cmdBuf, out senseBuf, 0, false, true, false, badSector, blockSize, 0,
                                            blocksToRead, false, _dev.Timeout, out cmdDuration);
                    else if(read12)
                        sense = _dev.Read12(out cmdBuf, out senseBuf, 0, false, true, false, false, (uint)badSector,
                                            blockSize, 0, blocksToRead, false, _dev.Timeout, out cmdDuration);
                    else if(read10)
                        sense = _dev.Read10(out cmdBuf, out senseBuf, 0, false, true, false, false, (uint)badSector,
                                            blockSize, 0, (ushort)blocksToRead, _dev.Timeout, out cmdDuration);
                    else if(read6)
                        sense = _dev.Read6(out cmdBuf, out senseBuf, (uint)badSector, blockSize, (byte)blocksToRead,
                                           _dev.Timeout, out cmdDuration);

                    totalDuration += cmdDuration;

                    if(sense || _dev.Error)
                        continue;

                    if(!sense &&
                       !_dev.Error)
                    {
                        _resume.BadBlocks.Remove(badSector);
                        extents.Add(badSector);
                    }

                    if(supportedSubchannel != MmcSubchannel.None)
                    {
                        byte[] data = new byte[sectorSize];
                        byte[] sub  = new byte[subSize];
                        Array.Copy(cmdBuf, 0, data, 0, sectorSize);
                        Array.Copy(cmdBuf, sectorSize, sub, 0, subSize);
                        _outputPlugin.WriteSectorLong(data, badSector);
                        _outputPlugin.WriteSectorTag(sub, badSector, SectorTagType.CdSectorSubchannel);
                    }
                    else
                    {
                        if(supportsLongSectors)
                        {
                            _outputPlugin.WriteSectorLong(cmdBuf, badSector);
                        }
                        else
                        {
                            if(cmdBuf.Length % 2352 == 0)
                            {
                                byte[] data = new byte[2048];

                                for(int b = 0; b < blocksToRead; b++)
                                    Array.Copy(cmdBuf, 16, data, 0, 2048);

                                _outputPlugin.WriteSector(data, badSector);
                            }
                            else
                            {
                                _outputPlugin.WriteSectorLong(cmdBuf, badSector);
                            }
                        }
                    }
                }

                EndProgress?.Invoke();
                end = DateTime.UtcNow;
                UpdateStatus?.Invoke($"Trimming finished in {(end - start).TotalSeconds} seconds.");
                _dumpLog.WriteLine("Trimming finished in {0} seconds.", (end - start).TotalSeconds);
            }
            #endregion Compact Disc Error trimming

            #region Compact Disc Error handling
            if(_resume.BadBlocks.Count > 0 &&
               !_aborted                   &&
               _retryPasses > 0)
            {
                int  pass              = 1;
                bool forward           = true;
                bool runningPersistent = false;

                Modes.ModePage? currentModePage = null;
                byte[]          md6;
                byte[]          md10;

                if(_persistent)
                {
                    Modes.ModePage_01_MMC pgMmc;

                    sense = _dev.ModeSense6(out cmdBuf, out _, false, ScsiModeSensePageControl.Current, 0x01,
                                            _dev.Timeout, out _);

                    if(sense)
                    {
                        sense = _dev.ModeSense10(out cmdBuf, out _, false, ScsiModeSensePageControl.Current, 0x01,
                                                 _dev.Timeout, out _);

                        if(!sense)
                        {
                            Modes.DecodedMode? dcMode10 =
                                Modes.DecodeMode10(cmdBuf, PeripheralDeviceTypes.MultiMediaDevice);

                            if(dcMode10?.Pages != null)
                                foreach(Modes.ModePage modePage in dcMode10.Value.Pages)
                                    if(modePage.Page    == 0x01 &&
                                       modePage.Subpage == 0x00)
                                        currentModePage = modePage;
                        }
                    }
                    else
                    {
                        Modes.DecodedMode? dcMode6 = Modes.DecodeMode6(cmdBuf, PeripheralDeviceTypes.MultiMediaDevice);

                        if(dcMode6?.Pages != null)
                            foreach(Modes.ModePage modePage in dcMode6.Value.Pages)
                                if(modePage.Page    == 0x01 &&
                                   modePage.Subpage == 0x00)
                                    currentModePage = modePage;
                    }

                    if(currentModePage == null)
                    {
                        pgMmc = new Modes.ModePage_01_MMC
                        {
                            PS = false, ReadRetryCount = 32, Parameter = 0x00
                        };

                        currentModePage = new Modes.ModePage
                        {
                            Page = 0x01, Subpage = 0x00, PageResponse = Modes.EncodeModePage_01_MMC(pgMmc)
                        };
                    }

                    pgMmc = new Modes.ModePage_01_MMC
                    {
                        PS = false, ReadRetryCount = 255, Parameter = 0x20
                    };

                    var md = new Modes.DecodedMode
                    {
                        Header = new Modes.ModeHeader(), Pages = new[]
                        {
                            new Modes.ModePage
                            {
                                Page = 0x01, Subpage = 0x00, PageResponse = Modes.EncodeModePage_01_MMC(pgMmc)
                            }
                        }
                    };

                    md6  = Modes.EncodeMode6(md, _dev.ScsiType);
                    md10 = Modes.EncodeMode10(md, _dev.ScsiType);

                    UpdateStatus?.Invoke("Sending MODE SELECT to drive (return damaged blocks).");
                    _dumpLog.WriteLine("Sending MODE SELECT to drive (return damaged blocks).");
                    sense = _dev.ModeSelect(md6, out senseBuf, true, false, _dev.Timeout, out _);

                    if(sense)
                        sense = _dev.ModeSelect10(md10, out senseBuf, true, false, _dev.Timeout, out _);

                    if(sense)
                    {
                        UpdateStatus?.
                            Invoke("Drive did not accept MODE SELECT command for persistent error reading, try another drive.");

                        DicConsole.DebugWriteLine("Error: {0}", Sense.PrettifySense(senseBuf));

                        _dumpLog.
                            WriteLine("Drive did not accept MODE SELECT command for persistent error reading, try another drive.");
                    }
                    else
                    {
                        runningPersistent = true;
                    }
                }

                InitProgress?.Invoke();
                cdRepeatRetry:
                ulong[]     tmpArray              = _resume.BadBlocks.ToArray();
                List<ulong> sectorsNotEvenPartial = new List<ulong>();

                foreach(ulong badSector in tmpArray)
                {
                    if(_aborted)
                    {
                        currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                        _dumpLog.WriteLine("Aborted!");

                        break;
                    }

                    PulseProgress?.Invoke(string.Format("Retrying sector {0}, pass {1}, {3}{2}", badSector, pass,
                                                        forward ? "forward" : "reverse",
                                                        runningPersistent ? "recovering partial data, " : ""));

                    if(readcd)
                    {
                        sense = _dev.ReadCd(out cmdBuf, out senseBuf, (uint)badSector, blockSize, 1,
                                            MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                                            true, true, MmcErrorField.None, supportedSubchannel, _dev.Timeout,
                                            out cmdDuration);

                        totalDuration += cmdDuration;
                    }

                    if(sense || _dev.Error)
                    {
                        if(!runningPersistent)
                            continue;

                        FixedSense? decSense = Sense.DecodeFixed(senseBuf);

                        // MEDIUM ERROR, retry with ignore error below
                        if(decSense.HasValue &&
                           decSense.Value.ASC == 0x11)
                            if(!sectorsNotEvenPartial.Contains(badSector))
                                sectorsNotEvenPartial.Add(badSector);
                    }

                    if(!sense &&
                       !_dev.Error)
                    {
                        _resume.BadBlocks.Remove(badSector);
                        extents.Add(badSector);
                        UpdateStatus?.Invoke($"Correctly retried sector {badSector} in pass {pass}.");
                        _dumpLog.WriteLine("Correctly retried sector {0} in pass {1}.", badSector, pass);
                        sectorsNotEvenPartial.Remove(badSector);
                    }

                    if(supportedSubchannel != MmcSubchannel.None)
                    {
                        byte[] data = new byte[sectorSize];
                        byte[] sub  = new byte[subSize];
                        Array.Copy(cmdBuf, 0, data, 0, sectorSize);
                        Array.Copy(cmdBuf, sectorSize, sub, 0, subSize);
                        _outputPlugin.WriteSectorLong(data, badSector);
                        _outputPlugin.WriteSectorTag(sub, badSector, SectorTagType.CdSectorSubchannel);
                    }
                    else
                    {
                        _outputPlugin.WriteSectorLong(cmdBuf, badSector);
                    }
                }

                if(pass < _retryPasses &&
                   !_aborted           &&
                   _resume.BadBlocks.Count > 0)
                {
                    pass++;
                    forward = !forward;
                    _resume.BadBlocks.Sort();
                    _resume.BadBlocks.Reverse();

                    goto cdRepeatRetry;
                }

                EndProgress?.Invoke();

                // Try to ignore read errors, on some drives this allows to recover partial even if damaged data
                if(_persistent && sectorsNotEvenPartial.Count > 0)
                {
                    var pgMmc = new Modes.ModePage_01_MMC
                    {
                        PS = false, ReadRetryCount = 255, Parameter = 0x01
                    };

                    var md = new Modes.DecodedMode
                    {
                        Header = new Modes.ModeHeader(), Pages = new[]
                        {
                            new Modes.ModePage
                            {
                                Page = 0x01, Subpage = 0x00, PageResponse = Modes.EncodeModePage_01_MMC(pgMmc)
                            }
                        }
                    };

                    md6  = Modes.EncodeMode6(md, _dev.ScsiType);
                    md10 = Modes.EncodeMode10(md, _dev.ScsiType);

                    _dumpLog.WriteLine("Sending MODE SELECT to drive (ignore error correction).");
                    sense = _dev.ModeSelect(md6, out senseBuf, true, false, _dev.Timeout, out _);

                    if(sense)
                        sense = _dev.ModeSelect10(md10, out senseBuf, true, false, _dev.Timeout, out _);

                    if(!sense)
                    {
                        runningPersistent = true;

                        InitProgress?.Invoke();

                        foreach(ulong badSector in sectorsNotEvenPartial)
                        {
                            if(_aborted)
                            {
                                currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                                _dumpLog.WriteLine("Aborted!");

                                break;
                            }

                            PulseProgress?.Invoke($"Trying to get partial data for sector {badSector}");

                            if(readcd)
                            {
                                sense = _dev.ReadCd(out cmdBuf, out senseBuf, (uint)badSector, blockSize, 1,
                                                    MmcSectorTypes.AllTypes, false, false, true,
                                                    MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                                    supportedSubchannel, _dev.Timeout, out cmdDuration);

                                totalDuration += cmdDuration;
                            }

                            if(sense || _dev.Error)
                                continue;

                            _dumpLog.WriteLine("Got partial data for sector {0} in pass {1}.", badSector, pass);

                            if(supportedSubchannel != MmcSubchannel.None)
                            {
                                byte[] data = new byte[sectorSize];
                                byte[] sub  = new byte[subSize];
                                Array.Copy(cmdBuf, 0, data, 0, sectorSize);
                                Array.Copy(cmdBuf, sectorSize, sub, 0, subSize);
                                _outputPlugin.WriteSectorLong(data, badSector);
                                _outputPlugin.WriteSectorTag(sub, badSector, SectorTagType.CdSectorSubchannel);
                            }
                            else
                            {
                                _outputPlugin.WriteSectorLong(cmdBuf, badSector);
                            }
                        }

                        EndProgress?.Invoke();
                    }
                }

                if(runningPersistent && currentModePage.HasValue)
                {
                    // TODO: Enable when underlying images support lead-outs
                    /*
                    dumpLog.WriteLine("Retrying lead-outs");

                    InitProgress?.Invoke();
                    foreach(Tuple<ulong, ulong> leadout in leadOutExtents.ToArray())
                        for(ulong i = leadout.Item1; i <= leadout.Item2; i++)
                        {
                            if(aborted)
                            {
                                currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                                dumpLog.WriteLine("Aborted!");
                                break;
                            }

                            double cmdDuration = 0;

                            #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                            if(currentSpeed > maxSpeed && currentSpeed != 0) maxSpeed = currentSpeed;
                            if(currentSpeed < minSpeed && currentSpeed != 0) minSpeed = currentSpeed;
                            #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                            PulseProgress?.Invoke(string.Format("Reading sector {0} at lead-out ({1:F3} MiB/sec.)", i,
                                                  blocks, currentSpeed));

                            if(readcd)
                            {
                                sense = dev.ReadCd(out cmdBuf, out senseBuf, (uint)i, blockSize, 1,
                                                   MmcSectorTypes.AllTypes, false, false, true,
                                                   MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                                   supportedSubchannel, dev.Timeout, out cmdDuration);
                                totalDuration += cmdDuration;
                            }
                            else if(read16)
                                sense = dev.Read16(out cmdBuf, out senseBuf, 0, false, true, false, i, blockSize, 0,
                                                   1, false, dev.Timeout, out cmdDuration);
                            else if(read12)
                                sense = dev.Read12(out cmdBuf, out senseBuf, 0, false, true, false, false, (uint)i,
                                                   blockSize, 0, 1, false, dev.Timeout, out cmdDuration);
                            else if(read10)
                                sense = dev.Read10(out cmdBuf, out senseBuf, 0, false, true, false, false, (uint)i,
                                                   blockSize, 0, 1, dev.Timeout, out cmdDuration);
                            else if(read6)
                                sense = dev.Read6(out cmdBuf, out senseBuf, (uint)i, blockSize, 1, dev.Timeout,
                                                  out cmdDuration);

                            if(!sense && !dev.Error)
                            {
                                mhddLog.Write(i, cmdDuration);
                                ibgLog.Write(i, currentSpeed * 1024);
                                extents.Add(i, blocksToRead, true);
                                leadOutExtents.Remove(i);
                                DateTime writeStart = DateTime.Now;
                                if(supportedSubchannel != MmcSubchannel.None)
                                {
                                    byte[] data = new byte[SECTOR_SIZE * blocksToRead];
                                    byte[] sub  = new byte[subSize     * blocksToRead];

                                    for(int b = 0; b < blocksToRead; b++)
                                    {
                                        Array.Copy(cmdBuf, (int)(0 + b * blockSize), data, SECTOR_SIZE * b,
                                                   SECTOR_SIZE);
                                        Array.Copy(cmdBuf, (int)(SECTOR_SIZE + b * blockSize), sub, subSize * b,
                                                   subSize);
                                    }

                                    outputPlugin.WriteSectorsLong(data, i, blocksToRead);
                                    outputPlugin.WriteSectorsTag(sub, i, blocksToRead,
                                                                 SectorTagType.CdSectorSubchannel);
                                }
                                else outputPlugin.WriteSectors(cmdBuf, i, blocksToRead);

                                imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                            }
                            else
                            {
                                // TODO: Reset device after X errors
                                if(stopOnError) return; // TODO: Return more cleanly

                                // Write empty data
                                DateTime writeStart = DateTime.Now;
                                if(supportedSubchannel != MmcSubchannel.None)
                                {
                                    outputPlugin.WriteSectorsLong(new byte[SECTOR_SIZE * skip], i, 1);
                                    outputPlugin.WriteSectorsTag(new byte[subSize * skip], i, 1,
                                                                 SectorTagType.CdSectorSubchannel);
                                }
                                else outputPlugin.WriteSectors(new byte[blockSize * skip], i, 1);

                                imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;

                                mhddLog.Write(i, cmdDuration < 500 ? 65535 : cmdDuration);

                                ibgLog.Write(i, 0);
                            }

                            double newSpeed =
                                (double)blockSize * blocksToRead / 1048576 / (cmdDuration / 1000);
                            if(!double.IsInfinity(newSpeed)) currentSpeed = newSpeed;
                        }

                    EndProgress?.Invoke();
                    */

                    var md = new Modes.DecodedMode
                    {
                        Header = new Modes.ModeHeader(), Pages = new[]
                        {
                            currentModePage.Value
                        }
                    };

                    md6  = Modes.EncodeMode6(md, _dev.ScsiType);
                    md10 = Modes.EncodeMode10(md, _dev.ScsiType);

                    _dumpLog.WriteLine("Sending MODE SELECT to drive (return device to previous status).");
                    sense = _dev.ModeSelect(md6, out senseBuf, true, false, _dev.Timeout, out _);

                    if(sense)
                        _dev.ModeSelect10(md10, out senseBuf, true, false, _dev.Timeout, out _);
                }

                EndProgress?.Invoke();
            }
            #endregion Compact Disc Error handling

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

            if(!_nometadata)
            {
                _dumpLog.WriteLine("Creating sidecar.");
                var         filters     = new FiltersList();
                IFilter     filter      = filters.GetFilter(_outputPath);
                IMediaImage inputPlugin = ImageFormat.Detect(filter);

                if(!inputPlugin.Open(filter))
                {
                    StoppingErrorMessage?.Invoke("Could not open created image.");

                    return;
                }

                DateTime chkStart = DateTime.UtcNow;

                // ReSharper disable once UseObjectOrCollectionInitializer
                _sidecarClass                      =  new Sidecar(inputPlugin, _outputPath, filter.Id, _encoding);
                _sidecarClass.InitProgressEvent    += InitProgress;
                _sidecarClass.UpdateProgressEvent  += UpdateProgress;
                _sidecarClass.EndProgressEvent     += EndProgress;
                _sidecarClass.InitProgressEvent2   += InitProgress2;
                _sidecarClass.UpdateProgressEvent2 += UpdateProgress2;
                _sidecarClass.EndProgressEvent2    += EndProgress2;
                _sidecarClass.UpdateStatusEvent    += UpdateStatus;
                CICMMetadataType sidecar = _sidecarClass.Create();
                end = DateTime.UtcNow;

                totalChkDuration = (end - chkStart).TotalMilliseconds;
                _dumpLog.WriteLine("Sidecar created in {0} seconds.", (end - chkStart).TotalSeconds);

                _dumpLog.WriteLine("Average checksum speed {0:F3} KiB/sec.",
                                   ((double)blockSize * (double)(blocks + 1)) / 1024 / (totalChkDuration / 1000));

                if(_preSidecar != null)
                {
                    _preSidecar.OpticalDisc = sidecar.OpticalDisc;
                    sidecar                 = _preSidecar;
                }

                List<(ulong start, string type)> filesystems = new List<(ulong start, string type)>();

                if(sidecar.OpticalDisc[0].Track != null)
                    filesystems.AddRange(from xmlTrack in sidecar.OpticalDisc[0].Track
                                         where xmlTrack.FileSystemInformation != null
                                         from partition in xmlTrack.FileSystemInformation
                                         where partition.FileSystems != null from fileSystem in partition.FileSystems
                                         select (partition.StartSector, fileSystem.Type));

                if(filesystems.Count > 0)
                    foreach(var filesystem in filesystems.Select(o => new
                    {
                        o.start, o.type
                    }).Distinct())
                        _dumpLog.WriteLine("Found filesystem {0} at sector {1}", filesystem.type, filesystem.start);

                sidecar.OpticalDisc[0].Dimensions = Dimensions.DimensionsFromMediaType(dskType);
                (string type, string subType) discType = CommonTypes.Metadata.MediaType.MediaTypeToString(dskType);
                sidecar.OpticalDisc[0].DiscType          = discType.type;
                sidecar.OpticalDisc[0].DiscSubType       = discType.subType;
                sidecar.OpticalDisc[0].DumpHardwareArray = _resume.Tries.ToArray();

                foreach(KeyValuePair<MediaTagType, byte[]> tag in mediaTags)
                    if(_outputPlugin.SupportedMediaTags.Contains(tag.Key))
                        AddMediaTagToSidecar(_outputPath, tag, ref sidecar);

                UpdateStatus?.Invoke("Writing metadata sidecar");

                var xmlFs = new FileStream(_outputPrefix + ".cicm.xml", FileMode.Create);

                var xmlSer = new XmlSerializer(typeof(CICMMetadataType));
                xmlSer.Serialize(xmlFs, sidecar);
                xmlFs.Close();
            }

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