// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : OmniDrive.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CompactDisc dumping with OmniDrive.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps user data part using an OmniDrive.
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aaru.Checksums;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core.Logging;
using Aaru.Decoders.CD;
using Aaru.Decoders.SCSI;
using Aaru.Devices;
using Aaru.Localization;
using Aaru.Logging;
using Humanizer;
using Track = Aaru.CommonTypes.Structs.Track;
using TrackType = Aaru.CommonTypes.Enums.TrackType;

namespace Aaru.Core.Devices.Dumping;

partial class Dump
{
    // TODO: Data mode
    /// <summary>Reads all CD user data</summary>
    /// <param name="audioExtents">Extents with audio sectors</param>
    /// <param name="blocks">Total number of positive sectors</param>
    /// <param name="blockSize">Size of the read sector in bytes</param>
    /// <param name="currentSpeed">Current read speed</param>
    /// <param name="currentTry">Current dump hardware try</param>
    /// <param name="extents">Extents</param>
    /// <param name="ibgLog">IMGBurn log</param>
    /// <param name="imageWriteDuration">Duration of image write</param>
    /// <param name="lastSector">Last sector number</param>
    /// <param name="leadOutExtents">Lead-out extents</param>
    /// <param name="maxSpeed">Maximum speed</param>
    /// <param name="mhddLog">MHDD log</param>
    /// <param name="minSpeed">Minimum speed</param>
    /// <param name="newTrim">Is trim a new one?</param>
    /// <param name="offsetBytes">Read offset</param>
    /// <param name="sectorsForOffset">Sectors needed to fix offset</param>
    /// <param name="subSize">Subchannel size in bytes</param>
    /// <param name="supportedSubchannel">Drive's maximum supported subchannel</param>
    /// <param name="supportsLongSectors">Supports reading EDC and ECC</param>
    /// <param name="totalDuration">Total commands duration</param>
    /// <param name="tracks">Disc tracks</param>
    /// <param name="subLog">Subchannel log</param>
    /// <param name="desiredSubchannel">Subchannel desired to save</param>
    /// <param name="isrcs">List of disc ISRCs</param>
    /// <param name="mcn">Disc media catalogue number</param>
    /// <param name="subchannelExtents">List of subchannels not yet dumped correctly</param>
    /// <param name="smallestPregapLbaPerTrack">List of smallest pregap relative address per track</param>
    void ReadCdDataOmniDrive(ExtentsULong audioExtents, ulong blocks, uint blockSize, ref double currentSpeed,
                             DumpHardware currentTry, ExtentsULong extents, IbgLog ibgLog,
                             ref double imageWriteDuration, long lastSector, ExtentsULong leadOutExtents,
                             ref double maxSpeed, MhddLog mhddLog, ref double minSpeed, out bool newTrim,
                             int offsetBytes, int sectorsForOffset, uint subSize, MmcSubchannel supportedSubchannel,
                             bool supportsLongSectors, ref double totalDuration, Track[] tracks, SubchannelLog subLog,
                             MmcSubchannel desiredSubchannel, Dictionary<byte, string> isrcs, ref string mcn,
                             HashSet<int> subchannelExtents, Dictionary<byte, int> smallestPregapLbaPerTrack)
    {
        ulong              sectorSpeedStart = 0; // Used to calculate correct speed
        uint               blocksToRead;         // How many sectors to read at once
        bool               sense;                // Sense indicator
        byte[]             cmdBuf     = null;    // Data buffer
        ReadOnlySpan<byte> senseBuf   = null;    // Sense buffer
        const uint         sectorSize = 2352;    // Full sector size
        newTrim = false;
        var outputFormat = _outputPlugin as IWritableImage;

        InitProgress?.Invoke();

        var    crossingLeadOut       = false;
        var    failedCrossingLeadOut = false;
        var    skippingLead          = false;
        double elapsed               = 0;
        var    speedSectorCounter    = 0;

        if(_ludicrousSpeed)
        {
            UpdateStatus?.Invoke(UI.Yes__sir__Setting_ludicrous_speed_sir);

            SetCdSpeedClamped(0xFFFF);
        }
        else if(_hyperSpeed)
        {
            Track t = tracks.FirstOrDefault(t => t.StartSector <= _resume.NextBlock &&
                                                 t.EndSector   >= _resume.NextBlock);

            if(t is null)
            {
                UpdateStatus?.Invoke(_speed is 0xFFFF or 0
                                         ? Localization.Core.Setting_speed_to_MAX_for_data_reading
                                         : string.Format(Localization.Core.Setting_speed_to_0_x_for_data_reading,
                                                         _speed));

                SetCdSpeedClamped(0xFFFF);
            }
            else if(t.Type == TrackType.Audio)
            {
                UpdateStatus?.Invoke(Localization.Core.Setting_speed_to_8x_for_audio_reading);

                SetCdSpeedClamped(1416);
            }
            else
            {
                UpdateStatus?.Invoke(_speed is 0xFFFF or 0
                                         ? Localization.Core.Setting_speed_to_MAX_for_data_reading
                                         : string.Format(Localization.Core.Setting_speed_to_0_x_for_data_reading,
                                                         _speed));

                SetCdSpeedClamped(0xFFFF);
            }
        }
        else
        {
            UpdateStatus?.Invoke(UI.Setting_speed_to_8x_for_scrambled_reading);

            SetCdSpeedClamped(1416);
        }

        // Spin up
        _dev.OmniDriveReadCd(out _, out _, (uint)_resume.NextBlock, _maximumReadable, _dev.Timeout, out _);

        for(ulong i = _resume.NextBlock; (long)i <= lastSector; i += blocksToRead)
        {
            _speedStopwatch.Reset();

            if(_aborted)
            {
                currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                UpdateStatus?.Invoke(Localization.Core.Aborted);

                break;
            }

            while(leadOutExtents.Contains(i))
            {
                skippingLead = true;
                i++;
            }

            if((long)i > lastSector) break;

            var firstSectorToRead = (uint)i;

            Track track = tracks.OrderBy(static t => t.StartSector).LastOrDefault(t => i >= t.StartSector);

            blocksToRead = _c2Repair ? 24u : 32u;

            if(blocksToRead == 1) blocksToRead += (uint)sectorsForOffset;

            if(blocksToRead == 0)
            {
                if(!skippingLead) i += (ulong)sectorsForOffset;

                skippingLead = false;

                continue;
            }

            if(offsetBytes < 0)
            {
                if(i == 0)
                    firstSectorToRead = uint.MaxValue - (uint)(sectorsForOffset - 1); // -1
                else
                    firstSectorToRead -= (uint)sectorsForOffset;

                if(blocksToRead <= sectorsForOffset) blocksToRead += (uint)sectorsForOffset;
            }

            if(speedSectorCounter > 1000)
            {
                if(_ludicrousSpeed)
                    SetCdSpeedClamped(0xFFFF);
                else if(_hyperSpeed)
                {
                    Track t = tracks.FirstOrDefault(t => t.StartSector <= _resume.NextBlock &&
                                                         t.EndSector   >= _resume.NextBlock);

                    if(t is not null && t.Type == TrackType.Audio)
                        SetCdSpeedClamped(1416);
                    else
                        SetCdSpeedClamped(0xFFFF);
                }
                else
                    SetCdSpeedClamped(1416);

                speedSectorCounter = 0;
            }
            else
                speedSectorCounter += (int)blocksToRead;

            if(currentSpeed > maxSpeed && currentSpeed > 0) maxSpeed = currentSpeed;

            if(currentSpeed < minSpeed && currentSpeed > 0) minSpeed = currentSpeed;

            UpdateProgress?.Invoke(string.Format(Localization.Core.Reading_sector_0_of_1_2,
                                                 i,
                                                 blocks,
                                                 ByteSize.FromMegabytes(currentSpeed).Per(_oneSecond).Humanize()),
                                   (long)i,
                                   (long)blocks);

            _speedStopwatch.Start();

            // Only gather C2 when there is audio to protect and repair is enabled; C2 also caps the read size, so with
            // repair off we read larger plain blocks instead. OmniDrive returns C2 at the same speed as a plain read,
            // so gathering it in the main pass detects concealed audio up front. RepackAudioC2 collapses the C2 layout
            // into the normal layout in place (no per-read allocation) and flags concealed audio sectors.
            if(_c2Supported && _c2Repair)
            {
                sense = _dev.OmniDriveReadCdWithC2(out cmdBuf,
                                                   out senseBuf,
                                                   firstSectorToRead,
                                                   blocksToRead,
                                                   _dev.Timeout,
                                                   out _);

                // Fall back to a plain read if the C2 read fails.
                if(!sense && !_dev.Error)
                    RepackAudioC2(ref cmdBuf, blocksToRead, blockSize, subSize, firstSectorToRead, audioExtents);
                else
                    sense = _dev.OmniDriveReadCd(out cmdBuf,
                                                 out senseBuf,
                                                 firstSectorToRead,
                                                 blocksToRead,
                                                 _dev.Timeout,
                                                 out _);
            }
            else
                sense = _dev.OmniDriveReadCd(out cmdBuf,
                                             out senseBuf,
                                             firstSectorToRead,
                                             blocksToRead,
                                             _dev.Timeout,
                                             out _);

            _speedStopwatch.Stop();


            if(!sense && !_dev.Error)
            {
                // Because one block has been partially used to fix the offset
                if(offsetBytes != 0)
                {
                    FixOffsetData(offsetBytes,
                                  sectorSize,
                                  sectorsForOffset,
                                  supportedSubchannel,
                                  ref blocksToRead,
                                  subSize,
                                  ref cmdBuf,
                                  blockSize,
                                  failedCrossingLeadOut);
                }

                mhddLog.Write(i, _speedStopwatch.Elapsed.TotalMilliseconds, blocksToRead);
                ibgLog.Write(i, currentSpeed * 1024);
                extents.Add(i, blocksToRead, true);
                _writeStopwatch.Restart();

                var         data         = new byte[sectorSize * blocksToRead];
                var         sub          = new byte[subSize    * blocksToRead];
                var         sectorStatus = new SectorStatus[blocksToRead];
                var         sector       = new byte[sectorSize];
                List<ulong> paintBad     = [];

                for(var b = 0; b < blocksToRead; b++)
                {
                    // A read can overrun into a Lead-Out - either past the last user area sector (e.g. near
                    // the end of the disc, where blocksToRead is not clamped to lastSector) or into an
                    // inter-session Lead-Out on a multi-session disc. Those sectors are not part of the
                    // user area and must never be flagged as bad / queued for trimming.
                    ulong sectorNumber   = i + (ulong)b;
                    bool  beyondUserArea = (long)sectorNumber > lastSector || leadOutExtents.Contains(sectorNumber);

                    Array.Copy(cmdBuf, (int)(0 + b * blockSize), sector, 0, sectorSize);

                    if(IsScrambledData(sector, (int)(i + (ulong)b), out _))
                    {
                        sector = Sector.Scramble(sector);
                        SectorFixResult fixStatus = CdChecksums.FixSector(sector);
                        if(fixStatus == SectorFixResult.Correct) _correctSectors++;
                        if(fixStatus == SectorFixResult.Fixed) _fixedSectors++;

                        if(fixStatus == SectorFixResult.CouldNotFix && !beyondUserArea)
                        {
                            sectorStatus[b] = SectorStatus.Errored;
                            _resume.BadBlocks.Add(i + (ulong)b);
                            paintBad.Add(i          + (ulong)b);

                            _errorLog?.WriteLine(i + (ulong)b, Localization.Core.Reason_ECC_EDC_could_not_be_fixed);
                        }

                        // A sector inside audioExtents reaching here means it unscrambled into
                        // something that looks like Yellow Book data (that's what routed it into this
                        // branch in the first place) despite being classified as audio - the track type
                        // metadata cannot be trusted for it, so it must not be exempted from this check.
                        if(fixStatus == SectorFixResult.NotApplicable &&
                           (!HasValidSync(sector)          ||
                            (sector[0x00F] & 0x03) != 0x02 ||
                            (sector[0x012] & 0x20) != 0x20) &&
                           !beyondUserArea)
                        {
                            sectorStatus[b] = SectorStatus.Errored;
                            _resume.BadBlocks.Add(i + (ulong)b);
                            paintBad.Add(i          + (ulong)b);

                            _errorLog?.WriteLine(i + (ulong)b, Localization.Core.Reason_scrambled_but_not_data);
                        }

                        Array.Copy(sector, 0, cmdBuf, (int)(0 + b * blockSize), sectorSize);
                    }

                    // Should be data but it's not?
                    else if(!audioExtents.Contains(i + (ulong)b) && !beyondUserArea)
                    {
                        sectorStatus[b] = SectorStatus.Errored;
                        _resume.BadBlocks.Add(i + (ulong)b);
                        paintBad.Add(i          + (ulong)b);

                        _errorLog?.WriteLine(i + (ulong)b, Localization.Core.Reason_expected_data_not_scrambled);
                    }

                    Array.Copy(cmdBuf, (int)(0 + b * blockSize), data, sectorSize * b, sectorSize);

                    Array.Copy(cmdBuf, (int)(sectorSize + b * blockSize), sub, subSize * b, subSize);

                    // Not already set
                    if(sectorStatus[b] != SectorStatus.Errored) sectorStatus[b] = SectorStatus.Dumped;
                }

                if(supportsLongSectors)
                    outputFormat.WriteSectorsLong(data, i, false, blocksToRead, sectorStatus);
                else
                {
                    var cooked = new MemoryStream();

                    for(var b = 0; b < blocksToRead; b++)
                    {
                        Array.Copy(cmdBuf, (int)(0 + b * blockSize), sector, 0, sectorSize);
                        byte[] cookedSector = Sector.GetUserData(sector);
                        cooked.Write(cookedSector, 0, cookedSector.Length);
                    }

                    outputFormat.WriteSectors(cooked.ToArray(), i, false, blocksToRead, sectorStatus);
                }

                bool indexesChanged = Media.CompactDisc.WriteSubchannelToImage(supportedSubchannel,
                                                                               desiredSubchannel,
                                                                               sub,
                                                                               i,
                                                                               blocksToRead,
                                                                               subLog,
                                                                               isrcs,
                                                                               (byte)track.Sequence,
                                                                               ref mcn,
                                                                               tracks,
                                                                               subchannelExtents,
                                                                               _fixSubchannelPosition,
                                                                               outputFormat as IWritableOpticalImage,
                                                                               _fixSubchannel,
                                                                               _fixSubchannelCrc,
                                                                               UpdateStatus,
                                                                               smallestPregapLbaPerTrack,
                                                                               true,
                                                                               out List<ulong> newPregapSectors);

                // Set tracks and go back
                if(indexesChanged)
                {
                    (outputFormat as IWritableOpticalImage).SetTracks([.. tracks]);

                    foreach(ulong newPregapSector in newPregapSectors)
                    {
                        if(newPregapSector == 0) continue;

                        Track sectorTrack =
                            tracks.FirstOrDefault(t => t.StartSector <= newPregapSector &&
                                                       t.EndSector   >= newPregapSector);

                        if(sectorTrack.Sequence == 1) continue;

                        Track prevTrack = tracks.FirstOrDefault(t => t.Sequence == sectorTrack.Sequence - 1);

                        if(sectorTrack.Session != prevTrack.Session) continue;

                        if(sectorTrack.Type != prevTrack.Type)
                        {
                            _resume.BadBlocks.Add(newPregapSector);

                            _errorLog?.WriteLine(newPregapSector, Localization.Core.Reason_pregap_track_type_mismatch);
                        }
                    }

                    if(i >= blocksToRead)
                        i -= blocksToRead;
                    else
                        i = 0;

                    if(i > 0) i--;

                    foreach(Track aTrack in tracks.Where(static aTrack => aTrack.Type == TrackType.Audio))
                        audioExtents.Add(aTrack.StartSector, aTrack.EndSector);

                    continue;
                }

                _mediaGraph?.PaintSectorsGood(i, blocksToRead);
                foreach(ulong b in paintBad) _mediaGraph?.PaintSectorsBad(b, 1);

                imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;

                // OmniDrive rarely reports sense: damaged sectors surface here as uncorrectable ECC/EDC in an
                // otherwise successful read, so the skip-on-error behaviour must be applied in this branch too.
                if(paintBad.Count > 0)
                {
                    newTrim = true;

                    ulong firstBad  = paintBad[0];
                    ulong skipStart = i        + blocksToRead;
                    ulong skipEnd   = firstBad + _skip;

                    if(skipEnd > (ulong)(lastSector + 1)) skipEnd = (ulong)(lastSector + 1);

                    // Do not skip over Lead-Out sectors, only user area sectors
                    for(ulong b = skipStart; b < skipEnd; b++)
                    {
                        if(!leadOutExtents.Contains(b)) continue;

                        skipEnd = b;

                        break;
                    }

                    if(_skip > 0 && skipEnd > skipStart)
                    {
                        var sectorsToSkip   = (uint)(skipEnd - skipStart);
                        var skippedStatuses = new SectorStatus[sectorsToSkip];
                        Array.Fill(skippedStatuses, SectorStatus.NotDumped);

                        _writeStopwatch.Restart();

                        if(supportsLongSectors)
                        {
                            outputFormat.WriteSectorsLong(new byte[sectorSize * sectorsToSkip],
                                                          skipStart,
                                                          false,
                                                          sectorsToSkip,
                                                          skippedStatuses);
                        }
                        else
                        {
                            outputFormat.WriteSectors(new byte[2048 * sectorsToSkip],
                                                      skipStart,
                                                      false,
                                                      sectorsToSkip,
                                                      skippedStatuses);
                        }

                        if(supportedSubchannel != MmcSubchannel.None && desiredSubchannel != MmcSubchannel.None)
                        {
                            outputFormat.WriteSectorsTag(new byte[subSize * sectorsToSkip],
                                                         skipStart,
                                                         false,
                                                         sectorsToSkip,
                                                         SectorTagType.CdSectorSubchannel);
                        }

                        imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;

                        for(ulong b = skipStart; b < skipEnd; b++) _resume.BadBlocks.Add(b);

                        AaruLogging.WriteLine(Localization.Core.Skipping_0_blocks_from_errored_block_1,
                                              sectorsToSkip,
                                              firstBad);

                        i = skipEnd - blocksToRead;
                    }
                }
            }
            else
            {
                if(crossingLeadOut && Sense.Decode(senseBuf)?.ASC == 0x21)
                {
                    if(failedCrossingLeadOut) break;

                    failedCrossingLeadOut = true;
                    blocksToRead          = 0;

                    continue;
                }

                _errorLog?.WriteLine(firstSectorToRead, _dev.Error, _dev.LastError, senseBuf.ToArray());

                // TODO: Reset device after X errors
                if(_stopOnError) return; // TODO: Return more cleanly

                if(i + _skip > blocks) _skip = (uint)(blocks - i);

                // Do not mark Lead-Out sectors as bad, only user area sectors
                uint sectorsToMark = _skip;

                for(ulong b = i; b < i + _skip; b++)
                {
                    if(!leadOutExtents.Contains(b)) continue;

                    sectorsToMark = (uint)(b - i);

                    break;
                }

                // Write empty data
                _writeStopwatch.Restart();

                if(supportedSubchannel != MmcSubchannel.None)
                {
                    outputFormat.WriteSectorsLong(new byte[sectorSize * sectorsToMark],
                                                  i,
                                                  false,
                                                  sectorsToMark,
                                                  ErroredThenSkippedStatuses(blocksToRead, sectorsToMark));

                    if(desiredSubchannel != MmcSubchannel.None)
                    {
                        outputFormat.WriteSectorsTag(new byte[subSize * sectorsToMark],
                                                     i,
                                                     false,
                                                     sectorsToMark,
                                                     SectorTagType.CdSectorSubchannel);
                    }
                }
                else
                {
                    if(supportsLongSectors)
                    {
                        outputFormat.WriteSectorsLong(new byte[blockSize * sectorsToMark],
                                                      i,
                                                      false,
                                                      sectorsToMark,
                                                      ErroredThenSkippedStatuses(blocksToRead, sectorsToMark));
                    }
                    else
                    {
                        if(cmdBuf.Length % sectorSize == 0)
                        {
                            outputFormat.WriteSectors(new byte[2048 * sectorsToMark],
                                                      i,
                                                      false,
                                                      sectorsToMark,
                                                      ErroredThenSkippedStatuses(blocksToRead, sectorsToMark));
                        }
                        else
                        {
                            outputFormat.WriteSectorsLong(new byte[blockSize * sectorsToMark],
                                                          i,
                                                          false,
                                                          sectorsToMark,
                                                          ErroredThenSkippedStatuses(blocksToRead, sectorsToMark));
                        }
                    }
                }

                imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;

                for(ulong b = i; b < i + sectorsToMark; b++) _resume.BadBlocks.Add(b);

                AaruLogging.Debug(MODULE_NAME, Localization.Core.READ_error_0, Sense.PrettifySense(senseBuf));

                mhddLog.Write(i,
                              _speedStopwatch.Elapsed.TotalMilliseconds < 500
                                  ? 65535
                                  : _speedStopwatch.Elapsed.TotalMilliseconds,
                              _skip);

                AaruLogging.WriteLine(Localization.Core.Skipping_0_blocks_from_errored_block_1, _skip, i);
                i       += _skip - blocksToRead;
                newTrim =  true;
            }

            totalDuration += _speedStopwatch.Elapsed.TotalMilliseconds;

            _writeStopwatch.Stop();

            sectorSpeedStart += blocksToRead;

            _resume.NextBlock = i + blocksToRead;

            elapsed += _speedStopwatch.Elapsed.TotalMilliseconds;

            if(elapsed < 100) continue;

            currentSpeed = sectorSpeedStart * blockSize / (1048576 * elapsed / 1000);
            ibgLog.Write(i, currentSpeed                                     * 1024);
            sectorSpeedStart = 0;
            elapsed          = 0;
        }

        _speedStopwatch.Stop();
        EndProgress?.Invoke();

        _resume.BadBlocks = [.. _resume.BadBlocks.Distinct()];
    }

    /// <summary>Reads all the hidden track in CD-i Ready discs</summary>
    /// <param name="blocks">Total number of positive sectors</param>
    /// <param name="blockSize">Size of the read sector in bytes</param>
    /// <param name="currentSpeed">Current read speed</param>
    /// <param name="currentTry">Current dump hardware try</param>
    /// <param name="extents">Extents</param>
    /// <param name="ibgLog">IMGBurn log</param>
    /// <param name="imageWriteDuration">Duration of image write</param>
    /// <param name="leadOutExtents">Lead-out extents</param>
    /// <param name="maxSpeed">Maximum speed</param>
    /// <param name="mhddLog">MHDD log</param>
    /// <param name="minSpeed">Minimum speed</param>
    /// <param name="offsetBytes">Read offset</param>
    /// <param name="sectorsForOffset">Sectors needed to fix offset</param>
    /// <param name="subSize">Subchannel size in bytes</param>
    /// <param name="supportedSubchannel">Drive's maximum supported subchannel</param>
    /// <param name="totalDuration">Total commands duration</param>
    /// <param name="cdiReadyReadAsAudio">Is the drive returning CD-i Ready hidden track as audio?</param>
    /// <param name="tracks">Disc tracks</param>
    /// <param name="subLog">Subchannel log</param>
    /// <param name="desiredSubchannel">Subchannel desired to save</param>
    /// <param name="isrcs">List of disc ISRCs</param>
    /// <param name="mcn">Disc media catalogue number</param>
    /// <param name="subchannelExtents">List of subchannels not yet dumped correctly</param>
    /// <param name="smallestPregapLbaPerTrack">List of smallest pregap relative address per track</param>
    void OmniDriveReadCdiReady(uint blockSize, ref double currentSpeed, DumpHardware currentTry, ExtentsULong extents,
                               IbgLog ibgLog, ref double imageWriteDuration, ExtentsULong leadOutExtents,
                               ref double maxSpeed, MhddLog mhddLog, ref double minSpeed, uint subSize,
                               MmcSubchannel supportedSubchannel, ref double totalDuration, Track[] tracks,
                               SubchannelLog subLog, MmcSubchannel desiredSubchannel, Dictionary<byte, string> isrcs,
                               ref string mcn, HashSet<int> subchannelExtents, ulong blocks, bool cdiReadyReadAsAudio,
                               int offsetBytes, int sectorsForOffset, Dictionary<byte, int> smallestPregapLbaPerTrack)
    {
        ulong              sectorSpeedStart = 0; // Used to calculate correct speed
        bool               sense;                // Sense indicator
        byte[]             cmdBuf;               // Data buffer
        ReadOnlySpan<byte> senseBuf;             // Sense buffer
        double             cmdDuration;          // Command execution time
        const uint         sectorSize = 2352;    // Full sector size
        Track              firstTrack = tracks.FirstOrDefault();
        uint               blocksToRead; // How many sectors to read at once
        var                outputOptical      = _outputPlugin as IWritableOpticalImage;
        var                speedSectorCounter = 0;

        if(firstTrack is null) return;

        if(_ludicrousSpeed)
        {
            UpdateStatus?.Invoke(UI.Yes__sir__Setting_ludicrous_speed_sir);

            SetCdSpeedClamped(0xFFFF);
        }
        else if(_hyperSpeed)
        {
            Track t = tracks.FirstOrDefault(t => t.StartSector <= _resume.NextBlock &&
                                                 t.EndSector   >= _resume.NextBlock);

            if(t is null)
            {
                UpdateStatus?.Invoke(_speed is 0xFFFF or 0
                                         ? Localization.Core.Setting_speed_to_MAX_for_data_reading
                                         : string.Format(Localization.Core.Setting_speed_to_0_x_for_data_reading,
                                                         _speed));

                SetCdSpeedClamped(0xFFFF);
            }
            else if(t.Type == TrackType.Audio)
            {
                UpdateStatus?.Invoke(Localization.Core.Setting_speed_to_8x_for_audio_reading);

                SetCdSpeedClamped(1416);
            }
            else
            {
                UpdateStatus?.Invoke(_speed is 0xFFFF or 0
                                         ? Localization.Core.Setting_speed_to_MAX_for_data_reading
                                         : string.Format(Localization.Core.Setting_speed_to_0_x_for_data_reading,
                                                         _speed));

                SetCdSpeedClamped(0xFFFF);
            }
        }
        else
        {
            UpdateStatus?.Invoke(UI.Setting_speed_to_8x_for_scrambled_reading);

            SetCdSpeedClamped(1416);
        }

        InitProgress?.Invoke();

        for(ulong i = _resume.NextBlock; i <= firstTrack.EndSector; i += blocksToRead)
        {
            if(_aborted)
            {
                currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                UpdateStatus?.Invoke(Localization.Core.Aborted);

                break;
            }

            var firstSectorToRead = (uint)i;

            blocksToRead = _maximumReadable;

            if(blocksToRead == 1) blocksToRead += (uint)sectorsForOffset;

            if(offsetBytes < 0)
            {
                if(i == 0)
                    firstSectorToRead = uint.MaxValue - (uint)(sectorsForOffset - 1); // -1
                else
                    firstSectorToRead -= (uint)sectorsForOffset;
            }

            if(speedSectorCounter > 1000)
            {
                if(_ludicrousSpeed)
                    SetCdSpeedClamped(0xFFFF);
                else if(_hyperSpeed)
                {
                    Track t = tracks.FirstOrDefault(t => t.StartSector <= _resume.NextBlock &&
                                                         t.EndSector   >= _resume.NextBlock);

                    if(t is not null && t.Type == TrackType.Audio)
                        SetCdSpeedClamped(1416);
                    else
                        SetCdSpeedClamped(0xFFFF);
                }
                else
                    SetCdSpeedClamped(1416);

                speedSectorCounter = 0;
            }
            else
                speedSectorCounter += (int)blocksToRead;

            if(currentSpeed > maxSpeed && currentSpeed > 0) maxSpeed = currentSpeed;

            if(currentSpeed < minSpeed && currentSpeed > 0) minSpeed = currentSpeed;

            UpdateProgress?.Invoke(string.Format(Localization.Core.Reading_sector_0_of_1_2,
                                                 i,
                                                 blocks,
                                                 ByteSize.FromMegabytes(currentSpeed).Per(_oneSecond).Humanize()),
                                   (long)i,
                                   (long)blocks);

            _speedStopwatch.Start();

            sense = _dev.OmniDriveReadCd(out cmdBuf,
                                         out senseBuf,
                                         firstSectorToRead,
                                         blocksToRead,
                                         _dev.Timeout,
                                         out cmdDuration);

            // OmniDrive seems to be doing something crazy, the command should return scrambled sectors, but as
            // CD-i Ready is "magic", it seems to be making it return descrambled sectors here, so let's check
            var dorothy   = new byte[2352];
            var scrambled = true;
            Array.Copy(cmdBuf, 0, dorothy, 0, 2352);
            if(IsData(dorothy) && !IsScrambledData(dorothy, (int)firstSectorToRead, out _)) scrambled = false;

            totalDuration += cmdDuration;
            _speedStopwatch.Stop();
            double elapsed;

            if(!sense && !_dev.Error)
            {
                if(scrambled)
                {
                    FixOffsetData(offsetBytes,
                                  sectorSize,
                                  sectorsForOffset,
                                  supportedSubchannel,
                                  ref blocksToRead,
                                  subSize,
                                  ref cmdBuf,
                                  blockSize,
                                  false);
                }

                mhddLog.Write(i, cmdDuration);
                ibgLog.Write(i, currentSpeed * 1024);
                extents.Add(i, blocksToRead, true);
                _writeStopwatch.Restart();

                List<ulong> paintBad = [];

                if(supportedSubchannel != MmcSubchannel.None)
                {
                    var data         = new byte[sectorSize * blocksToRead];
                    var sub          = new byte[subSize    * blocksToRead];
                    var tmpData      = new byte[sectorSize];
                    var sectorStatus = new SectorStatus[blocksToRead];

                    for(var b = 0; b < blocksToRead; b++)
                    {
                        sectorStatus[b] = SectorStatus.Dumped;
                        Array.Copy(cmdBuf, (int)(0 + b * blockSize), tmpData, 0, sectorSize);
                        if(scrambled) tmpData       = Sector.Scramble(tmpData);
                        SectorFixResult fixedStatus = CdChecksums.FixSector(tmpData);

                        if(fixedStatus == SectorFixResult.Correct) _correctSectors++;
                        if(fixedStatus == SectorFixResult.Fixed) _fixedSectors++;

                        if(fixedStatus == SectorFixResult.CouldNotFix) // Damaged
                        {
                            sectorStatus[b] = SectorStatus.Errored;
                            _resume.BadBlocks.Add(i + (ulong)b);
                            paintBad.Add(i          + (ulong)b);

                            _errorLog?.WriteLine(i + (ulong)b, Localization.Core.Reason_ECC_EDC_could_not_be_fixed);
                        }

                        Array.Copy(tmpData, 0, data, sectorSize * b, sectorSize);

                        Array.Copy(cmdBuf, (int)(sectorSize + b * blockSize), sub, subSize * b, subSize);
                    }

                    outputOptical.WriteSectorsLong(data, i, false, blocksToRead, sectorStatus);

                    bool indexesChanged = Media.CompactDisc.WriteSubchannelToImage(supportedSubchannel,
                        desiredSubchannel,
                        sub,
                        i,
                        blocksToRead,
                        subLog,
                        isrcs,
                        1,
                        ref mcn,
                        tracks,
                        subchannelExtents,
                        _fixSubchannelPosition,
                        outputOptical,
                        _fixSubchannel,
                        _fixSubchannelCrc,
                        UpdateStatus,
                        smallestPregapLbaPerTrack,
                        true,
                        out List<ulong> newPregapSectors);

                    // Set tracks and go back
                    if(indexesChanged)
                    {
                        outputOptical.SetTracks(tracks.ToList());

                        _resume.BadBlocks.AddRange(newPregapSectors);

                        foreach(ulong newPregapSector in newPregapSectors)
                            _errorLog?.WriteLine(newPregapSector, Localization.Core.Reason_pregap_track_type_mismatch);

                        if(i >= blocksToRead)
                            i -= blocksToRead;
                        else
                            i = 0;

                        if(i > 0) i--;

                        continue;
                    }
                }
                else
                {
                    var tmpData = new byte[sectorSize];
                    var data    = new byte[sectorSize * blocksToRead];
                    var status  = new SectorStatus[blocksToRead];

                    for(var b = 0; b < blocksToRead; b++)
                    {
                        status[b] = SectorStatus.Dumped;
                        Array.Copy(cmdBuf, (int)(b * sectorSize), tmpData, 0, sectorSize);
                        if(scrambled) tmpData       = Sector.Scramble(tmpData);
                        SectorFixResult fixedStatus = CdChecksums.FixSector(tmpData);
                        if(fixedStatus == SectorFixResult.Correct) _correctSectors++;
                        if(fixedStatus == SectorFixResult.Fixed) _fixedSectors++;

                        if(fixedStatus == SectorFixResult.CouldNotFix) // Damaged
                        {
                            _resume.BadBlocks.Add(i + (ulong)b);
                            paintBad.Add(i          + (ulong)b);
                            status[b] = SectorStatus.Errored;

                            _errorLog?.WriteLine(i + (ulong)b, Localization.Core.Reason_ECC_EDC_could_not_be_fixed);
                        }

                        Array.Copy(tmpData, 0, data, sectorSize * b, sectorSize);
                    }

                    outputOptical.WriteSectorsLong(data, i, false, blocksToRead, status);
                }

                imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;

                _mediaGraph?.PaintSectorsGood(i, blocksToRead);
            }
            else
            {
                _errorLog?.WriteLine(i, _dev.Error, _dev.LastError, senseBuf.ToArray());

                _resume.NextBlock = firstTrack.EndSector + 1;

                break;
            }

            _writeStopwatch.Stop();
            sectorSpeedStart += blocksToRead;

            _resume.NextBlock = i + blocksToRead;

            elapsed = _speedStopwatch.Elapsed.TotalSeconds;

            if(elapsed <= 0 || sectorSpeedStart * blockSize < 524288) continue;

            currentSpeed     = sectorSpeedStart * blockSize / (1048576 * elapsed);
            sectorSpeedStart = 0;
            _speedStopwatch.Reset();
        }

        _speedStopwatch.Stop();
        EndProgress?.Invoke();
    }
}