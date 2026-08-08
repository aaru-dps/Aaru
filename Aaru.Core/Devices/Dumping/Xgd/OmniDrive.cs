// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : OmniDrive.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps Xbox Game Discs using OmniDrives.
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
using System.Linq;
using System.Security.Cryptography;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core.Graphics;
using Aaru.Core.Logging;
using Aaru.Decoders.DVD;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.Xbox;
using Aaru.Devices;
using Aaru.Images;
using Aaru.Logging;
using Humanizer;
using Layers = Aaru.CommonTypes.AaruMetadata.Layers;
using Track = Aaru.CommonTypes.Structs.Track;
using TrackType = Aaru.CommonTypes.Enums.TrackType;
using Version = Aaru.CommonTypes.Interop.Version;

namespace Aaru.Core.Devices.Dumping;

partial class Dump
{
    /// <summary>Dumps an Xbox Game Disc using an OmniDrive</summary>
    /// <param name="mediaTags">Media tags as retrieved in MMC layer</param>
    /// <param name="dskType">Disc type as detected in MMC layer</param>
    void DumpXgdOmniDrive(Dictionary<MediaTagType, byte[]> mediaTags, MediaType dskType)
    {
        bool   sense;
        uint   blocksToRead  = 32;
        double totalDuration = 0;
        double currentSpeed  = 0;
        double maxSpeed      = double.MinValue;
        double minSpeed      = double.MaxValue;

        UpdateStatus?.Invoke(string.Format(Localization.Core.Media_identified_as_0, dskType.Humanize()));

        if(_outputPlugin is not IWritableImage outputFormat)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Image_is_not_writable_aborting);

            return;
        }

        sense = _dev.ReadCapacity(out byte[] readBuffer, out _, _dev.Timeout, out _);

        if(sense)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_get_disc_capacity);

            return;
        }

        ulong blocks = (ulong)((readBuffer[0] << 24) + (readBuffer[1] << 16) + (readBuffer[2] << 8) + readBuffer[3] &
                               0xFFFFFFFF) +
                       1;

        UpdateStatus?.Invoke(Localization.Core.Reading_Xbox_Security_Sector);
        sense = _dev.OmniDriveReadRawDvd(out byte[] ssBuf, out _, 0xFD021E, 1, _dev.Timeout, out _, rawAddresing: true);

        if(sense)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_get_Xbox_Security_Sector_not_continuing);

            return;
        }

        byte[] rebuiltSs = RebuildSs(ssBuf);
        ssBuf = rebuiltSs ?? Sector.GetUserData(ssBuf);

        SS.SecuritySector? xboxSs = SS.Decode(ssBuf);
        UpdateStatus?.Invoke(SS.Prettify(xboxSs));

        mediaTags.Add(MediaTagType.Xbox_SecuritySector, ssBuf);

        bool readable =
            !_dev.OmniDriveReadRawDvd(out readBuffer, out ReadOnlySpan<byte> senseBuf, 0, 1, _dev.Timeout, out _);

        if(!readable)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_read_medium_aborting_scan);

            return;
        }

        var speedSectorCounter = 0;

        // Set speed
        if(_speedMultiplier >= 0)
        {
            UpdateStatus?.Invoke(_speed == 0
                                     ? Localization.Core.Setting_speed_to_MAX
                                     : string.Format(Localization.Core.Setting_speed_to_0_x, _speed));

            _speed *= _speedMultiplier;

            if(_speed is 0 or > 0xFFFF) _speed = 0xFFFF;

            _dev.SetCdSpeed(out _, RotationalControl.ClvAndImpureCav, (ushort)_speed, 0, _dev.Timeout, out _);
        }

        while(true)
        {
            _dev.OmniDriveReadRawDvd(out readBuffer, out senseBuf, 0, blocksToRead, _dev.Timeout, out _);

            if(sense || _dev.Error) blocksToRead /= 2;

            if(!_dev.Error || blocksToRead == 1) break;
        }

        if(_dev.Error)
        {
            StoppingErrorMessage?.Invoke(string.Format(Localization.Core
                                                                   .Device_error_0_trying_to_guess_ideal_transfer_length,
                                                       _dev.LastError));

            return;
        }

        if(_skip < blocksToRead) _skip = blocksToRead;

        var ret = true;

        foreach(MediaTagType tag in mediaTags.Keys.Where(tag => !outputFormat.SupportedMediaTags.Contains(tag)))
        {
            ret = false;
            ErrorMessage?.Invoke(string.Format(Localization.Core.Output_format_does_not_support_0, tag));
        }

        if(!ret)
        {
            if(_force)
                ErrorMessage?.Invoke(Localization.Core.Several_media_tags_not_supported_continuing);
            else
            {
                StoppingErrorMessage?.Invoke(Localization.Core.Several_media_tags_not_supported_not_continuing);

                return;
            }
        }

        UpdateStatus?.Invoke(string.Format(Localization.Core.Reading_0_sectors_at_a_time, blocksToRead));

        var mhddLog = new MhddLog(_outputPrefix + ".mhddlog.bin",
                                  _dev,
                                  blocks,
                                  2048,
                                  blocksToRead,
                                  _private,
                                  _dimensions);

        var ibgLog = new IbgLog(_outputPrefix + ".ibg", 0x0010);
        ret = outputFormat.Create(_outputPath, dskType, _formatOptions, blocks, 0, 0, 2048);

        // Cannot create image
        if(!ret)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Error_creating_output_image_not_continuing +
                                         Environment.NewLine                                          +
                                         outputFormat.ErrorMessage);

            return;
        }

        if(outputFormat is AaruFormat aif && _errorRecovery > 0) aif.SetErasureCodingAuto((byte)_errorRecovery);

        _dumpStopwatch.Restart();
        double imageWriteDuration = 0;

        uint         saveBlocksToRead = blocksToRead;
        DumpHardware currentTry       = null;
        ExtentsULong extents          = null;

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
            StoppingErrorMessage?.Invoke(Localization.Core.Could_not_process_resume_file_not_continuing);

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

        (outputFormat as IWritableOpticalImage).SetTracks([
                                                              new Track
                                                              {
                                                                  BytesPerSector    = 2048,
                                                                  EndSector         = blocks - 1,
                                                                  Sequence          = 1,
                                                                  RawBytesPerSector = 2064,
                                                                  SubchannelType    = TrackSubchannelType.None,
                                                                  Session           = 1,
                                                                  Type              = TrackType.Data
                                                              }
                                                          ]);

        ulong currentSector = _resume.NextBlock;

        if(_resume.NextBlock > 0)
            UpdateStatus?.Invoke(string.Format(Localization.Core.Resuming_from_block_0, _resume.NextBlock));

        var newTrim = false;

        _speedStopwatch.Reset();
        ulong sectorSpeedStart = 0;
        InitProgress?.Invoke();
        double elapsed = 0;

        // XGD1 uses 16 protection extents (indices 0-15).
        // XGD2 and XGD3 only have actual protection zones at indices 0 and 3;
        // the other entries contain challenge-response authentication data, not unreadable sector ranges.
        int[] protectionExtentIndices = dskType == MediaType.XGD
                                            ? [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]
                                            : [0, 3];

        for(var ei = 0; ei <= protectionExtentIndices.Length; ei++)
        {
            if(_aborted)
            {
                _resume.NextBlock  = currentSector;
                currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                UpdateStatus?.Invoke(Localization.Core.Aborted);

                break;
            }

            if(currentSector >= blocks) break;

            ulong extentStart, extentEnd;

            // Extents
            if(ei < protectionExtentIndices.Length)
            {
                int e = protectionExtentIndices[ei];

                if(xboxSs.Value.Extents[e].StartPSN <= xboxSs.Value.Layer0EndPSN)
                    extentStart = xboxSs.Value.Extents[e].StartPSN - 0x30000;
                else
                {
                    extentStart = (xboxSs.Value.Layer0EndPSN + 1) * 2                 -
                                  ((xboxSs.Value.Extents[e].StartPSN ^ 0xFFFFFF) + 1) -
                                  0x30000;
                }

                if(xboxSs.Value.Extents[e].EndPSN <= xboxSs.Value.Layer0EndPSN)
                    extentEnd = xboxSs.Value.Extents[e].EndPSN - 0x30000;
                else
                {
                    extentEnd = (xboxSs.Value.Layer0EndPSN + 1) * 2               -
                                ((xboxSs.Value.Extents[e].EndPSN ^ 0xFFFFFF) + 1) -
                                0x30000;
                }
            }

            // After last extent — sentinel: extentEnd < extentStart so the protection write loop does not run
            else
            {
                extentStart = blocks;
                extentEnd   = blocks - 1;
            }

            if(currentSector > extentEnd) continue;

            for(ulong i = currentSector; i < extentStart; i += blocksToRead)
            {
                saveBlocksToRead = blocksToRead;

                if(_aborted)
                {
                    currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                    UpdateStatus?.Invoke(Localization.Core.Aborted);

                    break;
                }

                if(extentStart - i < blocksToRead) blocksToRead = (uint)(extentStart - i);

                if(speedSectorCounter > 1000)
                {
                    _dev.SetCdSpeed(out _, RotationalControl.ClvAndImpureCav, (ushort)_speed, 0, _dev.Timeout, out _);

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

                _speedStopwatch.Restart();

                sense = _dev.OmniDriveReadRawDvd(out readBuffer,
                                                 out senseBuf,
                                                 (uint)i,
                                                 blocksToRead,
                                                 _dev.Timeout,
                                                 out _);

                _speedStopwatch.Stop();
                elapsed       += _speedStopwatch.Elapsed.TotalMilliseconds;
                totalDuration += _speedStopwatch.Elapsed.TotalMilliseconds;

                if(!sense && !_dev.Error)
                {
                    mhddLog.Write(i, _speedStopwatch.Elapsed.TotalMilliseconds, blocksToRead);
                    _writeStopwatch.Restart();

                    if(_dumpRaw)
                    {
                        outputFormat.WriteSectorsLong(readBuffer,
                                                      i,
                                                      false,
                                                      blocksToRead,
                                                      Enumerable.Repeat(SectorStatus.Dumped, (int)blocksToRead)
                                                                .ToArray());
                    }
                    else
                    {
                        var sector = new byte[2064];

                        for(var b = 0; b < blocksToRead; b++)
                        {
                            Array.Copy(readBuffer, b * 2064, sector, 0, 2064);
                            byte[] cooked = Sector.GetUserData(sector);
                            outputFormat.WriteSector(cooked, i + (ulong)b, false, SectorStatus.Dumped);
                        }
                    }

                    imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;
                    extents.Add(i, blocksToRead, true);
                    _mediaGraph?.PaintSectorsGood(i, blocksToRead);
                }
                else
                {
                    _errorLog?.WriteLine(i, _dev.Error, _dev.LastError, senseBuf);

                    // TODO: Reset device after X errors
                    if(_stopOnError) return; // TODO: Return more cleanly

                    if(i + _skip > blocks) _skip = (uint)(blocks - i);

                    // Write empty data
                    _writeStopwatch.Restart();

                    if(_dumpRaw)
                    {
                        outputFormat.WriteSectorsLong(new byte[2064 * _skip],
                                                      i,
                                                      false,
                                                      _skip,
                                                      ErroredThenSkippedStatuses(blocksToRead, _skip));
                    }
                    else
                    {
                        outputFormat.WriteSectors(new byte[2048 * _skip],
                                                  i,
                                                  false,
                                                  _skip,
                                                  ErroredThenSkippedStatuses(blocksToRead, _skip));
                    }

                    imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;

                    for(ulong b = i; b < i + _skip; b++) _resume.BadBlocks.Add(b);

                    AaruLogging.Debug(MODULE_NAME, Localization.Core.READ_error_0, Sense.PrettifySense(senseBuf));

                    mhddLog.Write(i,
                                  _speedStopwatch.Elapsed.TotalMilliseconds < 500
                                      ? 65535
                                      : _speedStopwatch.Elapsed.TotalMilliseconds,
                                  _skip);

                    AaruLogging.WriteLine(Localization.Core.Skipping_0_blocks_from_errored_block_1, _skip, i);
                    i += _skip - blocksToRead;

                    newTrim = true;
                }

                _writeStopwatch.Stop();
                currentSector     =  i + blocksToRead;
                _resume.NextBlock =  currentSector;
                sectorSpeedStart  += blocksToRead;
                blocksToRead      =  saveBlocksToRead;

                if(elapsed < 100) continue;

                currentSpeed = sectorSpeedStart * 2064 / (1048576 * elapsed / 1000);
                ibgLog.Write(i, currentSpeed                                * 1024);
                sectorSpeedStart = 0;
                elapsed          = 0;
                _speedStopwatch.Reset();
            }

            _speedStopwatch.Stop();

            for(ulong i = extentStart; i <= extentEnd; i += blocksToRead)
            {
                saveBlocksToRead = blocksToRead;

                if(_aborted)
                {
                    currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                    UpdateStatus?.Invoke(Localization.Core.Aborted);

                    break;
                }

                if(extentEnd - i < blocksToRead) blocksToRead = (uint)(extentEnd - i) + 1;

                mhddLog.Write(i, _speedStopwatch.Elapsed.TotalMilliseconds, blocksToRead);
                ibgLog.Write(i, currentSpeed * 1024);

                // Write empty data
                _writeStopwatch.Restart();

                if(_dumpRaw)
                {
                    outputFormat.WriteSectorsLong(new byte[2064 * blocksToRead],
                                                  i,
                                                  false,
                                                  blocksToRead,
                                                  Enumerable.Repeat(SectorStatus.Dumped, (int)blocksToRead).ToArray());
                }
                else
                {
                    outputFormat.WriteSectors(new byte[2048 * blocksToRead],
                                              i,
                                              false,
                                              blocksToRead,
                                              Enumerable.Repeat(SectorStatus.Dumped, (int)blocksToRead).ToArray());
                }


                imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;
                extents.Add(i, blocksToRead, true);
                _mediaGraph?.PaintSectorsGood(i, blocksToRead);
                currentSector     = i + blocksToRead;
                _resume.NextBlock = currentSector;
                blocksToRead      = saveBlocksToRead;
            }

            if(!_aborted) currentSector = extentEnd + 1;
        }

        _writeStopwatch.Stop();
        _resume.BadBlocks = _resume.BadBlocks.Distinct().ToList();

        EndProgress?.Invoke();

        _dumpStopwatch.Stop();
        AaruLogging.WriteLine();
        mhddLog.Close();

        ibgLog.Close(_dev,
                     blocks,
                     2064,
                     _dumpStopwatch.Elapsed.TotalSeconds,
                     currentSpeed                * 1024,
                     2064 * (double)(blocks + 1) / 1024 / (totalDuration / 1000),
                     _devicePath);

        UpdateStatus?.Invoke(string.Format(Localization.Core.Dump_finished_in_0,
                                           _dumpStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second)));

        UpdateStatus?.Invoke(string.Format(Localization.Core.Average_dump_speed_0,
                                           ByteSize.FromBytes(2064 * (blocks + 1))
                                                   .Per(totalDuration.Milliseconds())
                                                   .Humanize()));

        UpdateStatus?.Invoke(string.Format(Localization.Core.Average_write_speed_0,
                                           ByteSize.FromBytes(2064 * (blocks + 1))
                                                   .Per(imageWriteDuration.Seconds())
                                                   .Humanize()));

#region Trimming

        if(_resume.BadBlocks.Count > 0 && !_aborted && _trim && newTrim)
        {
            _trimStopwatch.Restart();
            UpdateStatus?.Invoke(Localization.Core.Trimming_skipped_sectors);

            ulong[] tmpArray = _resume.BadBlocks.ToArray();
            InitProgress?.Invoke();

            foreach(ulong badSector in tmpArray)
            {
                if(_aborted)
                {
                    currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                    UpdateStatus?.Invoke(Localization.Core.Aborted);

                    break;
                }

                PulseProgress?.Invoke(string.Format(Localization.Core.Trimming_sector_0, badSector));

                sense = _dev.OmniDriveReadRawDvd(out readBuffer,
                                                 out senseBuf,
                                                 (uint)badSector,
                                                 1,
                                                 _dev.Timeout,
                                                 out double cmdDuration);

                totalDuration += cmdDuration;

                if(sense || _dev.Error)
                {
                    _errorLog?.WriteLine(badSector, _dev.Error, _dev.LastError, senseBuf);

                    continue;
                }

                _resume.BadBlocks.Remove(badSector);
                extents.Add(badSector);

                if(_dumpRaw)
                    outputFormat.WriteSectorLong(readBuffer, badSector, false, SectorStatus.Dumped);
                else
                    outputFormat.WriteSector(Sector.GetUserData(readBuffer), badSector, false, SectorStatus.Dumped);

                _mediaGraph?.PaintSectorGood(badSector);
            }

            EndProgress?.Invoke();
            _trimStopwatch.Stop();

            UpdateStatus?.Invoke(string.Format(Localization.Core.Trimming_finished_in_0,
                                               _trimStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second)));
        }

#endregion Trimming

#region Error handling

        if(_resume.BadBlocks.Count > 0 && !_aborted && _retryPasses > 0)
        {
            List<ulong> tmpList = [];

            foreach(ulong ur in _resume.BadBlocks)
                for(ulong i = ur; i < ur + blocksToRead; i++)
                    tmpList.Add(i);

            tmpList.Sort();

            var pass    = 1;
            var forward = true;

            _resume.BadBlocks = tmpList;

            InitProgress?.Invoke();
        repeatRetry:
            ulong[] tmpArray = _resume.BadBlocks.ToArray();

            foreach(ulong badSector in tmpArray)
            {
                if(_aborted)
                {
                    currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                    UpdateStatus?.Invoke(Localization.Core.Aborted);

                    break;
                }

                if(forward)
                {
                    PulseProgress?.Invoke(string.Format(Localization.Core.Retrying_sector_0_pass_1_forward,
                                                        badSector,
                                                        pass));
                }
                else
                {
                    PulseProgress?.Invoke(string.Format(Localization.Core.Retrying_sector_0_pass_1_reverse,
                                                        badSector,
                                                        pass));
                }

                sense = _dev.OmniDriveReadRawDvd(out readBuffer,
                                                 out senseBuf,
                                                 (uint)badSector,
                                                 1,
                                                 _dev.Timeout,
                                                 out double cmdDuration,
                                                 true);

                totalDuration += cmdDuration;

                if(sense || _dev.Error) _errorLog?.WriteLine(badSector, _dev.Error, _dev.LastError, senseBuf.ToArray());

                if(!sense && !_dev.Error)
                {
                    _resume.BadBlocks.Remove(badSector);
                    extents.Add(badSector);

                    if(_dumpRaw)
                        outputFormat.WriteSectorLong(readBuffer, badSector, false, SectorStatus.Dumped);
                    else
                        outputFormat.WriteSector(Sector.GetUserData(readBuffer), badSector, false, SectorStatus.Dumped);

                    _mediaGraph?.PaintSectorGood(badSector);

                    UpdateStatus?.Invoke(string.Format(Localization.Core.Correctly_retried_block_0_in_pass_1,
                                                       badSector,
                                                       pass));
                }
            }

            if(pass < _retryPasses && !_aborted && _resume.BadBlocks.Count > 0)
            {
                pass++;
                forward = !forward;
                _resume.BadBlocks.Sort();

                if(!forward) _resume.BadBlocks.Reverse();

                goto repeatRetry;
            }

            EndProgress?.Invoke();
        }

#endregion Error handling

        _resume.BadBlocks.Sort();
        currentTry.Extents = ExtentsConverter.ToMetadata(extents);

        foreach(KeyValuePair<MediaTagType, byte[]> tag in mediaTags)
        {
            if(tag.Value is null)
            {
                AaruLogging.Error(Localization.Core.Error_Tag_type_0_is_null_skipping, tag.Key);

                continue;
            }

            ret = outputFormat.WriteMediaTag(tag.Value, tag.Key);

            if(ret || _force) continue;

            // Cannot write tag to image
            StoppingErrorMessage?.Invoke(string.Format(Localization.Core.Cannot_write_tag_0, tag.Key) +
                                         Environment.NewLine                                          +
                                         outputFormat.ErrorMessage);

            return;
        }

        _resume.BadBlocks.Sort();

        foreach(ulong bad in _resume.BadBlocks)
            AaruLogging.WriteLine(Localization.Core.Sector_0_could_not_be_read, bad);

        currentTry.Extents = ExtentsConverter.ToMetadata(extents);

        outputFormat.SetDumpHardware(_resume.Tries);

        var metadata = new CommonTypes.Structs.ImageInfo
        {
            Application        = "Aaru",
            ApplicationVersion = Version.GetInformationalVersion()
        };

        if(!outputFormat.SetImageInfo(metadata))
        {
            ErrorMessage?.Invoke(Localization.Core.Error_0_setting_metadata +
                                 Environment.NewLine                        +
                                 outputFormat.ErrorMessage);
        }

        if(_preSidecar != null) outputFormat.SetMetadata(_preSidecar);

        UpdateStatus?.Invoke(Localization.Core.Closing_output_file);
        _imageCloseStopwatch.Restart();
        outputFormat.Close();
        _imageCloseStopwatch.Stop();

        UpdateStatus?.Invoke(string.Format(Localization.Core.Closed_in_0,
                                           _imageCloseStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second)));


        if(_aborted)
        {
            UpdateStatus?.Invoke(Localization.Core.Aborted);

            return;
        }

        double totalChkDuration = 0;

        if(_metadata)
        {
            var layers = new Layers
            {
                Type = LayerType.OTP,
                Sectors =
                [
                    new Sectors
                    {
                        Value = xboxSs.Value.Layer0EndPSN - 0x30000
                    }
                ]
            };

            WriteOpticalSidecar(2064, blocks, dskType, layers, mediaTags, 1, out totalChkDuration, null);
        }

        UpdateStatus?.Invoke("");

        UpdateStatus?.Invoke(string.Format(Localization.Core
                                                       .Took_a_total_of_0_1_processing_commands_2_checksumming_3_writing_4_closing,
                                           _dumpStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second),
                                           totalDuration.Milliseconds().Humanize(minUnit: TimeUnit.Second),
                                           totalChkDuration.Milliseconds().Humanize(minUnit: TimeUnit.Second),
                                           imageWriteDuration.Seconds().Humanize(minUnit: TimeUnit.Second),
                                           _imageCloseStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second)));

        UpdateStatus?.Invoke(string.Format(Localization.Core.Average_speed_0,
                                           ByteSize.FromBytes(2064 * (blocks + 1))
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

        UpdateStatus?.Invoke(string.Format(Localization.Core._0_sectors_could_not_be_read, _resume.BadBlocks.Count));
        UpdateStatus?.Invoke("");

        Statistics.AddMedia(dskType, true);
    }

    /// <summary>
    ///     Rebuilds and repairs an Xbox Security Sector from a raw 2064-byte DVD sector buffer.
    ///     Ported from RebuildSS.py by Deterous (https://github.com/Deterous/ParseXboxMetadata).
    /// </summary>
    /// <param name="rawSectorBuffer">2064-byte raw DVD sector containing the SS</param>
    /// <returns>Rebuilt 2048-byte SS buffer, or null if rebuild is not possible</returns>
    static byte[] RebuildSs(byte[] rawSectorBuffer)
    {
        if(rawSectorBuffer == null || rawSectorBuffer.Length < 2064) return null;

        // Detect XGD version from layer0End bytes in raw buffer
        int xgd;

        if(rawSectorBuffer[0x19] == 0x20 && rawSectorBuffer[0x1A] == 0x33 && rawSectorBuffer[0x1B] == 0xAF)
            xgd = 1;
        else if(rawSectorBuffer[0x19] == 0x20 && rawSectorBuffer[0x1A] == 0x33 && rawSectorBuffer[0x1B] == 0x9F)
            xgd = 2;
        else if(rawSectorBuffer[0x19] == 0x23 && rawSectorBuffer[0x1A] == 0x8E && rawSectorBuffer[0x1B] == 0x0F)
            xgd = 3;
        else
            return null;

        if(xgd == 2)
        {
            (int start, int end)[] zeroRanges =
            [
                (0x01D, 0x10C), (0x128, 0x20C), (0x2DB, 0x2DC), (0x2E0, 0x30C), (0x30E, 0x310), (0x40C, 0x46C),
                (0x47C, 0x4AA), (0x4B3, 0x4C6), (0x5F7, 0x606)
            ];

            foreach((int start, int end) range in zeroRanges)
            {
                for(int i = range.start; i < range.end; i++)
                    if(rawSectorBuffer[i] != 0)
                        return null;
            }
        }
        else if(xgd == 3)
        {
            (int start, int end)[] zeroRanges =
            [
                (0x01D, 0x027), (0x028, 0x02C), (0x101, 0x10B), (0x30E, 0x310), (0x40C, 0x46C), (0x47C, 0x4AA),
                (0x4B3, 0x4C6), (0x5F7, 0x606)
            ];

            foreach((int start, int end) range in zeroRanges)
            {
                for(int i = range.start; i < range.end; i++)
                    if(rawSectorBuffer[i] != 0)
                        return null;
            }
        }

        // Extract cprMai (4 bytes at offset 0x07 in raw buffer)
        var cprMai = new byte[4];
        Array.Copy(rawSectorBuffer, 0x007, cprMai, 0, 4);

        // Trim sector: take bytes 0x00C..0x80B (2048 bytes)
        var data = new byte[2048];
        Array.Copy(rawSectorBuffer, 0x00C, data, 0, 2048);

        // Place cprMai back at the correct offset
        if(xgd == 3)
            Array.Copy(cprMai, 0, data, 0x0F0, 4);
        else
            Array.Copy(cprMai, 0, data, 0x2D0, 4);

        // Descramble SS ranges — RibShark algorithm
        // XOR the 112 scramble index bytes with cprMai repeated
        var scrambleIndices = new byte[0xD0];

        for(var i = 0; i < scrambleIndices.Length; i++) scrambleIndices[i] = (byte)(data[0x730 + i] ^ cprMai[i % 4]);

        // Use scramble indices to reorder the scrambled range bytes (all but last index)
        var ssRange = new byte[0xCF];

        for(var i = 0; i < scrambleIndices.Length - 1; i++) ssRange[i] = data[0x661 + scrambleIndices[i]];

        Array.Copy(ssRange, 0, data, 0x661, 0xCF);
        Array.Copy(ssRange, 0, data, 0x730, 0xCF);

        if(xgd == 1) return data;

        // Validate CCRT header fields before attempting repair
        if(data[0x300] != 2) return null;

        if(data[0x301] != 21) return null;

        if(data[0x65F] != 0x02) return null;

        if(data[0x49E] != 0x04) return null;

        // Both copies of the SS range must match
        for(var i = 0; i < 0xCF; i++)
            if(data[0x661 + i] != data[0x730 + i])
                return null;

        return RepairCcrt2(data, xgd, cprMai);
    }

    // Decrypts and repairs the Challenge/Response Table entries in the SS.
    static byte[] RepairCcrt2(byte[] data, int xgd, byte[] cprMai)
    {
        var goodSs = (byte[])data.Clone();
        int offset = xgd == 3 ? 0x20 : 0x200;

        bool isKreonSs = data[555] == 0 &&
                         data[556] == 0 &&
                         data[564] == 0 &&
                         data[565] == 0 &&
                         data[573] == 0 &&
                         data[574] == 0 &&
                         data[582] == 0 &&
                         data[583] == 0;

        // Decrypt CCRT using AES-128 ECB with manual CBC chaining (IV = 0)
        byte[] aesKey =
        [
            0xD1, 0xE3, 0xB3, 0x3A, 0x6C, 0x1E, 0xF7, 0x70, 0x5F, 0x6D, 0xE9, 0x3B, 0xB6, 0xC0, 0xDC, 0x71
        ];

        var iv   = new byte[16];
        var dcrt = new byte[252]; // 21 entries × 12 bytes

        using var aes = Aes.Create();
        aes.Mode    = CipherMode.ECB;
        aes.Padding = PaddingMode.None;
        aes.Key     = aesKey;

        using ICryptoTransform decryptor = aes.CreateDecryptor();
        var                    ct        = new byte[16];
        var                    pt        = new byte[16];

        for(var i = 0; i < 16; i++)
        {
            Array.Copy(data, 0x304 + i * 16, ct, 0, 16);

            if(i == 15)
            {
                // Last block: copy first 12 bytes of ciphertext directly (no decrypt)
                Array.Copy(ct, 0, dcrt, i * 16, 12);
            }
            else
            {
                decryptor.TransformBlock(ct, 0, 16, pt, 0);

                for(var j = 0; j < 16; j++) pt[j] ^= iv[j];

                Array.Copy(pt, 0, dcrt, i * 16, 16);
                Array.Copy(ct, 0, iv,   0,      16);
            }
        }

        // Count non-zero 12-byte encrypted response entries in data[0x304..0x400]
        var encResponseCount = 0;

        for(var i = 0x304; i < 0x400; i += 12)
        {
            for(var j = 0; j < 12; j++)
            {
                if(data[i + j] == 0) continue;

                encResponseCount++;

                break;
            }
        }

        if(encResponseCount != 21) return null;

        // Parse decrypted CCRT entries (21 entries × 12 bytes)
        var dcrtEntries = new DcrtEntry[21];

        for(var i = 0; i < 21; i++)
        {
            int base12 = i * 12;
            var cd     = new byte[4];
            var resp   = new byte[4];
            Array.Copy(dcrt, base12 + 4, cd,   0, 4);
            Array.Copy(dcrt, base12 + 8, resp, 0, 4);

            dcrtEntries[i] = new DcrtEntry
            {
                ChallengeType = dcrt[base12],
                ChallengeId   = dcrt[base12 + 1],
                Cd            = cd,
                Response      = resp,
                Angle         = (ushort)(dcrt[base12 + 10] << 8 | dcrt[base12 + 11])
            };
        }

        // Validate all CT01 entries share the same CD, and it matches cprMai
        byte[] ct01FirstCd    = null;
        var    ct01Conflict   = false;
        var    cprMaiMismatch = false;

        foreach(DcrtEntry entry in dcrtEntries)
        {
            if(entry.ChallengeType != 0x01) continue;

            if(ct01FirstCd == null)
                ct01FirstCd = (byte[])entry.Cd.Clone();
            else if(!BytesEqual(ct01FirstCd, entry.Cd))
            {
                ct01Conflict = true;

                break;
            }
        }

        if(ct01Conflict) return null;

        if(ct01FirstCd != null && !BytesEqual(ct01FirstCd, cprMai)) cprMaiMismatch = true;

        if(cprMaiMismatch) return null;

        // Validate CT values are all known types
        foreach(DcrtEntry entry in dcrtEntries)
        {
            byte ct2 = entry.ChallengeType;

            if(ct2          != 0x01 &&
               ct2          != 0xE0 &&
               ct2          != 0x14 &&
               ct2          != 0x15 &&
               ct2          != 0x24 &&
               ct2          != 0x25 &&
               (ct2 & 0xF0) != 0xF0)
                return null;
        }

        // Count non-zero 9-byte CRT entries from data[0x730..]
        var responseCount = 0;

        for(var i = 0; i < 23; i++)
        {
            int baseOff = 0x730 + i * 9;
            var nonZero = false;

            for(var j = 0; j < 9; j++)
            {
                if(data[baseOff + j] == 0) continue;

                nonZero = true;

                break;
            }

            if(nonZero) responseCount++;
        }

        // Parse CRT entries
        var crtEntries = new List<CrtEntry>();

        for(var i = 0; i < responseCount; i++)
        {
            int baseRt = 0x730 + i * 9;

            if((data[baseRt] & 0xF0) == 0xF0) continue;

            int baseOff = offset + i * 9;
            var cd      = new byte[4];
            var resp    = new byte[5];
            Array.Copy(data, baseOff,     cd,   0, 4);
            Array.Copy(data, baseOff + 4, resp, 0, 5);

            crtEntries.Add(new CrtEntry
            {
                Index        = i,
                ResponseType = data[baseRt],
                ChallengeId  = data[baseRt + 1],
                Cd           = cd,
                Response     = resp,
                Angle        = (ushort)(data[baseOff + 5] << 8 | data[baseOff + 4]),
                Angle2       = (ushort)(data[baseOff + 8] << 8 | data[baseOff + 7])
            });
        }

        // Validate CT/RT pairing
        foreach(CrtEntry crt in crtEntries)
        {
            foreach(DcrtEntry dcrtEntry in dcrtEntries)
            {
                if(dcrtEntry.ChallengeId != crt.ChallengeId) continue;

                bool badPair = dcrtEntry.ChallengeType == 0x15 && crt.ResponseType != 0x01 ||
                               dcrtEntry.ChallengeType == 0x14 && crt.ResponseType != 0x03 ||
                               dcrtEntry.ChallengeType == 0x25 && crt.ResponseType != 0x05 ||
                               dcrtEntry.ChallengeType == 0x24 && crt.ResponseType != 0x07;

                if(badPair) return null;

                break;
            }
        }

        // Fix mismatches between DCRT and CRT entries
        foreach(CrtEntry crt in crtEntries)
        {
            foreach(DcrtEntry dcrtEntry in dcrtEntries)
            {
                if(dcrtEntry.ChallengeId != crt.ChallengeId) continue;

                if(dcrtEntry.ChallengeType == 0xE0 || dcrtEntry.ChallengeType == 0x01) break;

                if(dcrtEntry.ChallengeType is 0x24 or 0x25)
                {
                    // Angle challenge: only fix mismatched CD; validate angle range
                    if(dcrtEntry.Angle > 359) return null;

                    if(crt.Angle > 359 || crt.Angle2 > 359) return null;

                    if(!BytesEqual(dcrtEntry.Cd, crt.Cd)) Array.Copy(dcrtEntry.Cd, crt.Cd, 4);

                    break;
                }

                bool cdMismatch   = !BytesEqual(dcrtEntry.Cd, crt.Cd);
                bool respMismatch = !BytesEqual4(dcrtEntry.Response, crt.Response);

                if(cdMismatch && respMismatch)
                {
                    Array.Copy(dcrtEntry.Cd,       crt.Cd, 4);
                    Array.Copy(dcrtEntry.Response, 0,      crt.Response, 0, 4);
                }
                else if(cdMismatch)
                    Array.Copy(dcrtEntry.Cd,                         crt.Cd, 4);
                else if(respMismatch) Array.Copy(dcrtEntry.Response, 0,      crt.Response, 0, 4);

                break;
            }
        }

        // Write fixed CRT entries back
        foreach(CrtEntry crt in crtEntries)
        {
            int baseOff = offset + crt.Index * 9;
            Array.Copy(crt.Cd,       0, goodSs, baseOff,     4);
            Array.Copy(crt.Response, 0, goodSs, baseOff + 4, 5);
        }

        CleanSs(goodSs, xgd, !isKreonSs);

        return goodSs;
    }

    // Sets fixed angle bytes at known offsets by XGD version.
    static void CleanSs(byte[] ss, int xgd, bool ssv2)
    {
        if(xgd == 1) return;

        if(xgd == 2)
        {
            ss[552] = 0x01;
            ss[553] = 0x00;
            ss[555] = ssv2 ? (byte)0x01 : (byte)0x00;
            ss[556] = 0x00;
            ss[561] = 0x5B;
            ss[562] = 0x00;
            ss[564] = ssv2 ? (byte)0x5B : (byte)0x00;
            ss[565] = 0x00;
            ss[570] = 0xB5;
            ss[571] = 0x00;
            ss[573] = ssv2 ? (byte)0xB5 : (byte)0x00;
            ss[574] = 0x00;
            ss[579] = 0x0F;
            ss[580] = 0x01;
            ss[582] = ssv2 ? (byte)0x0F : (byte)0x00;
            ss[583] = ssv2 ? (byte)0x01 : (byte)0x00;

            return;
        }

        if(xgd != 3) return;

        ss[72]  = 0x01;
        ss[73]  = 0x00;
        ss[75]  = 0x01;
        ss[76]  = 0x00;
        ss[81]  = 0x5B;
        ss[82]  = 0x00;
        ss[84]  = 0x5B;
        ss[85]  = 0x00;
        ss[90]  = 0xB5;
        ss[91]  = 0x00;
        ss[93]  = 0xB5;
        ss[94]  = 0x00;
        ss[99]  = 0x0F;
        ss[100] = 0x01;
        ss[102] = 0x0F;
        ss[103] = 0x01;
    }

    static bool BytesEqual(byte[] a, byte[] b)
    {
        if(a.Length != b.Length) return false;

        for(var i = 0; i < a.Length; i++)
            if(a[i] != b[i])
                return false;

        return true;
    }

    // Compares only the first 4 bytes of b against all 4 bytes of a (Response is 4 bytes in DCRT, 5 in CRT)
    static bool BytesEqual4(byte[] a, byte[] b)
    {
        for(var i = 0; i < 4; i++)
            if(a[i] != b[i])
                return false;

        return true;
    }

    struct DcrtEntry
    {
        public byte   ChallengeType;
        public byte   ChallengeId;
        public byte[] Cd;
        public byte[] Response;
        public ushort Angle;
    }

    struct CrtEntry
    {
        public int    Index;
        public byte   ResponseType;
        public byte   ChallengeId;
        public byte[] Cd;
        public byte[] Response;
        public ushort Angle;
        public ushort Angle2;
    }
}