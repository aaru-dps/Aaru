// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ATA.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps media from ATA devices.
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core.Devices.Report;
using Aaru.Core.Graphics;
using Aaru.Core.Logging;
using Aaru.Decoders.ATA;
using Aaru.Decoders.PCMCIA;
using Humanizer;
using Humanizer.Bytes;
using Humanizer.Localisation;
using Identify = Aaru.CommonTypes.Structs.Devices.ATA.Identify;
using Tuple = Aaru.Decoders.PCMCIA.Tuple;
using Version = Aaru.CommonTypes.Interop.Version;

namespace Aaru.Core.Devices.Dumping;

/// <summary>Implements dumping ATA devices</summary>
public partial class Dump
{
    /// <summary>Dumps an ATA device</summary>
    void Ata()
    {
        if(_outputPlugin is not IWritableImage outputFormat)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Image_is_not_writable_aborting);

            return;
        }

        if(_dumpRaw)
        {
            if(_force)
                ErrorMessage?.Invoke(Localization.Core.Raw_dumping_not_yet_supported_in_ATA_devices_continuing);
            else
            {
                StoppingErrorMessage?.Invoke(Localization.Core.Raw_dumping_not_yet_supported_in_ATA_devices_aborting);

                return;
            }
        }

        const ushort ataProfile         = 0x0001;
        const uint   timeout            = 5;
        double       imageWriteDuration = 0;
        MediaType    mediaType          = MediaType.Unknown;

        UpdateStatus?.Invoke(Localization.Core.Requesting_ATA_IDENTIFY_DEVICE);
        _dumpLog.WriteLine(Localization.Core.Requesting_ATA_IDENTIFY_DEVICE);
        bool sense = _dev.AtaIdentify(out byte[] cmdBuf, out AtaErrorRegistersChs errorChs);

        if(sense)
            _errorLog?.WriteLine("ATA IDENTIFY DEVICE", _dev.Error, _dev.LastError, errorChs);
        else if(Identify.Decode(cmdBuf).HasValue)
        {
            Identify.IdentifyDevice? ataIdNullable = Identify.Decode(cmdBuf);

            if(ataIdNullable != null)
            {
                // Guaranteed to never fall into default
                Identify.IdentifyDevice ataId         = ataIdNullable ?? default(Identify.IdentifyDevice);
                byte[]                  ataIdentify   = cmdBuf;
                double                  totalDuration = 0;
                double                  currentSpeed  = 0;
                double                  maxSpeed      = double.MinValue;
                double                  minSpeed      = double.MaxValue;
                cmdBuf = Array.Empty<byte>();

                // Initialize reader
                UpdateStatus?.Invoke(Localization.Core.Initializing_reader);
                _dumpLog.WriteLine(Localization.Core.Initializing_reader);
                var ataReader = new Reader(_dev, timeout, ataIdentify, _errorLog);

                // Fill reader blocks
                ulong blocks = ataReader.GetDeviceBlocks();

                // Check block sizes
                if(ataReader.GetBlockSize())
                {
                    _dumpLog.WriteLine(Localization.Core.ERROR_Cannot_get_block_size_0, ataReader.ErrorMessage);
                    ErrorMessage(ataReader.ErrorMessage);

                    return;
                }

                uint blockSize          = ataReader.LogicalBlockSize;
                uint physicalSectorSize = ataReader.PhysicalBlockSize;

                if(ataReader.FindReadCommand())
                {
                    _dumpLog.WriteLine(Localization.Core.ERROR_Cannot_find_correct_read_command_0,
                                       ataReader.ErrorMessage);

                    ErrorMessage(ataReader.ErrorMessage);

                    return;
                }

                // Check how many blocks to read, if error show and return
                if(ataReader.GetBlocksToRead(_maximumReadable))
                {
                    _dumpLog.WriteLine(Localization.Core.ERROR_Cannot_get_blocks_to_read_0, ataReader.ErrorMessage);
                    ErrorMessage(ataReader.ErrorMessage);

                    return;
                }

                uint   blocksToRead = ataReader.BlocksToRead;
                ushort cylinders    = ataReader.Cylinders;
                byte   heads        = ataReader.Heads;
                byte   sectors      = ataReader.Sectors;

                UpdateStatus?.Invoke(string.Format(Localization.Core.Device_reports_0_blocks_1_bytes,
                                                   blocks,
                                                   blocks * blockSize));

                UpdateStatus?.Invoke(string.Format(Localization.Core
                                                               .Device_reports_0_cylinders_1_heads_2_sectors_per_track,
                                                   cylinders,
                                                   heads,
                                                   sectors));

                UpdateStatus?.Invoke(string.Format(Localization.Core.Device_can_read_0_blocks_at_a_time, blocksToRead));

                UpdateStatus?.Invoke(string.Format(Localization.Core.Device_reports_0_bytes_per_logical_block,
                                                   blockSize));

                UpdateStatus?.Invoke(string.Format(Localization.Core.Device_reports_0_bytes_per_physical_block,
                                                   physicalSectorSize));

                _dumpLog.WriteLine(Localization.Core.Device_reports_0_blocks_1_bytes, blocks, blocks * blockSize);

                _dumpLog.WriteLine(Localization.Core.Device_reports_0_cylinders_1_heads_2_sectors_per_track,
                                   cylinders,
                                   heads,
                                   sectors);

                _dumpLog.WriteLine(Localization.Core.Device_can_read_0_blocks_at_a_time,        blocksToRead);
                _dumpLog.WriteLine(Localization.Core.Device_reports_0_bytes_per_logical_block,  blockSize);
                _dumpLog.WriteLine(Localization.Core.Device_reports_0_bytes_per_physical_block, physicalSectorSize);

                bool removable = !_dev.IsCompactFlash &&
                                 ataId.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.Removable);

                DumpHardware currentTry = null;
                ExtentsULong extents    = null;

                ResumeSupport.Process(ataReader.IsLba,
                                      removable,
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

                MhddLog mhddLog;
                IbgLog  ibgLog;
                double  duration;

                var ret = true;

                if(_dev.IsUsb                  &&
                   _dev.UsbDescriptors != null &&
                   !outputFormat.SupportedMediaTags.Contains(MediaTagType.USB_Descriptors))
                {
                    ret = false;
                    _dumpLog.WriteLine(Localization.Core.Output_format_does_not_support_USB_descriptors);
                    ErrorMessage(Localization.Core.Output_format_does_not_support_USB_descriptors);
                }

                if(_dev.IsPcmcia    &&
                   _dev.Cis != null &&
                   !outputFormat.SupportedMediaTags.Contains(MediaTagType.PCMCIA_CIS))
                {
                    ret = false;
                    _dumpLog.WriteLine(Localization.Core.Output_format_does_not_support_PCMCIA_CIS_descriptors);
                    ErrorMessage(Localization.Core.Output_format_does_not_support_PCMCIA_CIS_descriptors);
                }

                if(!outputFormat.SupportedMediaTags.Contains(MediaTagType.ATA_IDENTIFY))
                {
                    ret = false;
                    _dumpLog.WriteLine(Localization.Core.Dump_Ata_Output_format_does_not_support_ATA_IDENTIFY_);
                    ErrorMessage(Localization.Core.Dump_Ata_Output_format_does_not_support_ATA_IDENTIFY_);
                }

                if(!ret)
                {
                    if(_force)
                    {
                        _dumpLog.WriteLine(Localization.Core.Several_media_tags_not_supported_continuing);
                        ErrorMessage(Localization.Core.Several_media_tags_not_supported_continuing);
                    }
                    else
                    {
                        _dumpLog.WriteLine(Localization.Core.Several_media_tags_not_supported_not_continuing);
                        StoppingErrorMessage?.Invoke(Localization.Core.Several_media_tags_not_supported_not_continuing);

                        return;
                    }
                }

                mediaType = MediaTypeFromDevice.GetFromAta(_dev.Manufacturer,
                                                           _dev.Model,
                                                           _dev.IsRemovable,
                                                           _dev.IsCompactFlash,
                                                           _dev.IsPcmcia,
                                                           blocks);

                ret = outputFormat.Create(_outputPath, mediaType, _formatOptions, blocks, blockSize);

                // Cannot create image
                if(!ret)
                {
                    _dumpLog.WriteLine(Localization.Core.Error_creating_output_image_not_continuing);
                    _dumpLog.WriteLine(outputFormat.ErrorMessage);

                    StoppingErrorMessage?.Invoke(Localization.Core.Error_creating_output_image_not_continuing +
                                                 Environment.NewLine                                          +
                                                 outputFormat.ErrorMessage);

                    return;
                }

                // Setting geometry
                outputFormat.SetGeometry(cylinders, heads, sectors);

                bool recoveredError;

                if(ataReader.IsLba)
                {
                    UpdateStatus?.Invoke(string.Format(Localization.Core.Reading_0_sectors_at_a_time, blocksToRead));

                    if(_skip < blocksToRead) _skip = blocksToRead;

                    mhddLog = new MhddLog(_outputPrefix + ".mhddlog.bin",
                                          _dev,
                                          blocks,
                                          blockSize,
                                          blocksToRead,
                                          _private,
                                          _dimensions);

                    ibgLog = new IbgLog(_outputPrefix + ".ibg", ataProfile);

                    if(_resume.NextBlock > 0)
                    {
                        UpdateStatus?.Invoke(string.Format(Localization.Core.Resuming_from_block_0, _resume.NextBlock));
                        _dumpLog.WriteLine(Localization.Core.Resuming_from_block_0, _resume.NextBlock);
                    }

                    if(_createGraph)
                    {
                        Spiral.DiscParameters discSpiralParameters = Spiral.DiscParametersFromMediaType(mediaType);

                        if(discSpiralParameters is not null)
                            _mediaGraph = new Spiral((int)_dimensions, (int)_dimensions, discSpiralParameters, blocks);
                        else
                            _mediaGraph = new BlockMap((int)_dimensions, (int)_dimensions, blocks);

                        if(_mediaGraph is not null)
                        {
                            foreach(Tuple<ulong, ulong> e in extents.ToArray())
                                _mediaGraph?.PaintSectorsGood(e.Item1, (uint)(e.Item2 - e.Item1 + 2));
                        }

                        _mediaGraph?.PaintSectorsBad(_resume.BadBlocks);
                    }

                    var newTrim = false;

                    _dumpStopwatch.Restart();
                    _speedStopwatch.Reset();
                    ulong sectorSpeedStart = 0;
                    InitProgress?.Invoke();

                    for(ulong i = _resume.NextBlock; i < blocks; i += blocksToRead)
                    {
                        if(_aborted)
                        {
                            currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                            UpdateStatus?.Invoke(Localization.Core.Aborted);
                            _dumpLog.WriteLine(Localization.Core.Aborted);

                            break;
                        }

                        if(blocks - i < blocksToRead) blocksToRead = (byte)(blocks - i);

                        if(currentSpeed > maxSpeed && currentSpeed > 0) maxSpeed = currentSpeed;

                        if(currentSpeed < minSpeed && currentSpeed > 0) minSpeed = currentSpeed;

                        UpdateProgress?.Invoke(string.Format(Localization.Core.Reading_sector_0_of_1_2,
                                                             i,
                                                             blocks,
                                                             ByteSize.FromMegabytes(currentSpeed)
                                                                     .Per(_oneSecond)
                                                                     .Humanize()),
                                               (long)i,
                                               (long)blocks);

                        _speedStopwatch.Start();
                        bool error = ataReader.ReadBlocks(out cmdBuf, i, blocksToRead, out duration, out _, out _);
                        _speedStopwatch.Stop();

                        _writeStopwatch.Restart();

                        if(!error)
                        {
                            mhddLog.Write(i, duration, blocksToRead);
                            ibgLog.Write(i, currentSpeed * 1024);
                            outputFormat.WriteSectors(cmdBuf, i, blocksToRead);
                            imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;
                            extents.Add(i, blocksToRead, true);
                            _mediaGraph?.PaintSectorsGood(i, blocksToRead);
                        }
                        else
                        {
                            if(i + _skip > blocks) _skip = (uint)(blocks - i);

                            for(ulong b = i; b < i + _skip; b++) _resume.BadBlocks.Add(b);

                            mhddLog.Write(i, duration < 500 ? 65535 : duration, _skip);

                            ibgLog.Write(i, 0);
                            outputFormat.WriteSectors(new byte[blockSize * _skip], i, _skip);
                            imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;
                            _dumpLog.WriteLine(Localization.Core.Skipping_0_blocks_from_errored_block_1, _skip, i);
                            i       += _skip - blocksToRead;
                            newTrim =  true;
                        }

                        _writeStopwatch.Stop();

                        sectorSpeedStart  += blocksToRead;
                        _resume.NextBlock =  i + blocksToRead;

                        double elapsed = _speedStopwatch.Elapsed.TotalSeconds;

                        if(elapsed <= 0 || sectorSpeedStart * blockSize < 524288) continue;

                        currentSpeed     = sectorSpeedStart * blockSize / (1048576 * elapsed);
                        sectorSpeedStart = 0;
                        _speedStopwatch.Reset();
                    }

                    _speedStopwatch.Stop();

                    _resume.BadBlocks = _resume.BadBlocks.Distinct().ToList();

                    _dumpStopwatch.Stop();
                    EndProgress?.Invoke();
                    mhddLog.Close();

                    ibgLog.Close(_dev,
                                 blocks,
                                 blockSize,
                                 _dumpStopwatch.Elapsed.TotalSeconds,
                                 currentSpeed                     * 1024,
                                 blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000),
                                 _devicePath);

                    UpdateStatus?.Invoke(string.Format(Localization.Core.Dump_finished_in_0,
                                                       _dumpStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second)));

                    UpdateStatus?.Invoke(string.Format(Localization.Core.Average_dump_speed_0,
                                                       ByteSize.FromBytes(blockSize * (blocks + 1))
                                                               .Per(totalDuration.Milliseconds())
                                                               .Humanize()));

                    UpdateStatus?.Invoke(string.Format(Localization.Core.Average_write_speed_0,
                                                       ByteSize.FromBytes(blockSize * (blocks + 1))
                                                               .Per(imageWriteDuration.Seconds())
                                                               .Humanize()));

                    _dumpLog.WriteLine(string.Format(Localization.Core.Dump_finished_in_0,
                                                     _dumpStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second)));

                    _dumpLog.WriteLine(Localization.Core.Average_dump_speed_0,
                                       ByteSize.FromBytes(blockSize * (blocks + 1))
                                               .Per(totalDuration.Milliseconds())
                                               .Humanize());

                    _dumpLog.WriteLine(string.Format(Localization.Core.Average_write_speed_0,
                                                     ByteSize.FromBytes(blockSize * (blocks + 1))
                                                             .Per(imageWriteDuration.Seconds())
                                                             .Humanize()));

#region Trimming

                    if(_resume.BadBlocks.Count > 0 && !_aborted && _trim && newTrim)
                    {
                        _trimStopwatch.Restart();
                        UpdateStatus?.Invoke(Localization.Core.Trimming_skipped_sectors);
                        _dumpLog.WriteLine(Localization.Core.Trimming_skipped_sectors);

                        ulong[] tmpArray = _resume.BadBlocks.ToArray();
                        InitProgress?.Invoke();

                        foreach(ulong badSector in tmpArray)
                        {
                            if(_aborted)
                            {
                                currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                                UpdateStatus?.Invoke(Localization.Core.Aborted);
                                _dumpLog.WriteLine(Localization.Core.Aborted);

                                break;
                            }

                            PulseProgress?.Invoke(string.Format(Localization.Core.Trimming_sector_0, badSector));

                            bool error =
                                ataReader.ReadBlock(out cmdBuf, badSector, out duration, out recoveredError, out _);

                            totalDuration += duration;

                            if(error && !recoveredError) continue;

                            _resume.BadBlocks.Remove(badSector);
                            extents.Add(badSector);
                            outputFormat.WriteSector(cmdBuf, badSector);
                            _mediaGraph?.PaintSectorGood(badSector);
                        }

                        EndProgress?.Invoke();
                        _trimStopwatch.Stop();

                        UpdateStatus?.Invoke(string.Format(Localization.Core.Trimming_finished_in_0,
                                                           _trimStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second)));

                        _dumpLog.WriteLine(string.Format(Localization.Core.Trimming_finished_in_0,
                                                         _trimStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second)));
                    }

#endregion Trimming

#region Error handling

                    if(_resume.BadBlocks.Count > 0 && !_aborted && _retryPasses > 0)
                    {
                        var pass    = 1;
                        var forward = true;

                        InitProgress?.Invoke();
                    repeatRetryLba:
                        ulong[] tmpArray = _resume.BadBlocks.ToArray();

                        foreach(ulong badSector in tmpArray)
                        {
                            if(_aborted)
                            {
                                currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                                UpdateStatus?.Invoke(Localization.Core.Aborted);
                                _dumpLog.WriteLine(Localization.Core.Aborted);

                                break;
                            }

                            if(forward)
                            {
                                PulseProgress?.Invoke(_persistent
                                                          ? string.Format(Localization.Core
                                                                             .Retrying_sector_0_pass_1_recovering_partial_data_forward,
                                                                          badSector,
                                                                          pass)
                                                          : string.Format(Localization.Core
                                                                             .Retrying_sector_0_pass_1_forward,
                                                                          badSector,
                                                                          pass));
                            }
                            else
                            {
                                PulseProgress?.Invoke(_persistent
                                                          ? string.Format(Localization.Core
                                                                             .Retrying_sector_0_pass_1_recovering_partial_data_reverse,
                                                                          badSector,
                                                                          pass)
                                                          : string.Format(Localization.Core
                                                                             .Retrying_sector_0_pass_1_reverse,
                                                                          badSector,
                                                                          pass));
                            }

                            bool error =
                                ataReader.ReadBlock(out cmdBuf, badSector, out duration, out recoveredError, out _);

                            totalDuration += duration;

                            if(!error || recoveredError)
                            {
                                _resume.BadBlocks.Remove(badSector);
                                extents.Add(badSector);
                                outputFormat.WriteSector(cmdBuf, badSector);
                                _mediaGraph?.PaintSectorGood(badSector);

                                UpdateStatus?.Invoke(string.Format(Localization.Core
                                                                      .Correctly_retried_block_0_in_pass_1,
                                                                   badSector,
                                                                   pass));

                                _dumpLog.WriteLine(Localization.Core.Correctly_retried_block_0_in_pass_1,
                                                   badSector,
                                                   pass);
                            }
                            else if(_persistent) outputFormat.WriteSector(cmdBuf, badSector);
                        }

                        if(pass < _retryPasses && !_aborted && _resume.BadBlocks.Count > 0)
                        {
                            pass++;
                            forward = !forward;
                            _resume.BadBlocks.Sort();

                            if(!forward) _resume.BadBlocks.Reverse();

                            goto repeatRetryLba;
                        }

                        EndProgress?.Invoke();
                    }

#endregion Error handling LBA

                    currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                }
                else
                {
                    mhddLog = new MhddLog(_outputPrefix + ".mhddlog.bin",
                                          _dev,
                                          blocks,
                                          blockSize,
                                          blocksToRead,
                                          _private,
                                          _dimensions);

                    ibgLog = new IbgLog(_outputPrefix + ".ibg", ataProfile);

                    if(_createGraph)
                    {
                        Spiral.DiscParameters discSpiralParameters = Spiral.DiscParametersFromMediaType(mediaType);

                        if(discSpiralParameters is not null)
                            _mediaGraph = new Spiral((int)_dimensions, (int)_dimensions, discSpiralParameters, blocks);
                        else
                            _mediaGraph = new BlockMap((int)_dimensions, (int)_dimensions, blocks);

                        if(_mediaGraph is not null)
                        {
                            foreach(Tuple<ulong, ulong> e in extents.ToArray())
                                _mediaGraph?.PaintSectorsGood(e.Item1, (uint)(e.Item2 - e.Item1 + 2));
                        }

                        _mediaGraph?.PaintSectorsBad(_resume.BadBlocks);
                    }

                    ulong currentBlock = 0;
                    blocks = (ulong)(cylinders * heads * sectors);
                    _dumpStopwatch.Restart();
                    _speedStopwatch.Reset();
                    ulong sectorSpeedStart = 0;
                    InitProgress?.Invoke();

                    for(ushort cy = 0; cy < cylinders; cy++)
                    {
                        for(byte hd = 0; hd < heads; hd++)
                        {
                            for(byte sc = 1; sc < sectors; sc++)
                            {
                                if(_aborted)
                                {
                                    currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                                    UpdateStatus?.Invoke(Localization.Core.Aborted);
                                    _dumpLog.WriteLine(Localization.Core.Aborted);

                                    break;
                                }

                                if(currentSpeed > maxSpeed && currentSpeed > 0) maxSpeed = currentSpeed;

                                if(currentSpeed < minSpeed && currentSpeed > 0) minSpeed = currentSpeed;

                                PulseProgress?.Invoke(string.Format(Localization.Core
                                                                       .Reading_cylinder_0_head_1_sector_2_3,
                                                                    cy,
                                                                    hd,
                                                                    sc,
                                                                    ByteSize.FromMegabytes(currentSpeed)
                                                                            .Per(_oneSecond)
                                                                            .Humanize()));

                                _speedStopwatch.Start();

                                bool error =
                                    ataReader.ReadChs(out cmdBuf, cy, hd, sc, out duration, out recoveredError);

                                _speedStopwatch.Stop();

                                totalDuration += duration;

                                _writeStopwatch.Restart();

                                if(!error || recoveredError)
                                {
                                    mhddLog.Write(currentBlock, duration);
                                    ibgLog.Write(currentBlock, currentSpeed * 1024);

                                    outputFormat.WriteSector(cmdBuf, (ulong)((cy * heads + hd) * sectors + (sc - 1)));

                                    imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;
                                    extents.Add(currentBlock);
                                    _mediaGraph?.PaintSectorGood((ulong)((cy * heads + hd) * sectors + (sc - 1)));

                                    _dumpLog.WriteLine(Localization.Core.Error_reading_cylinder_0_head_1_sector_2,
                                                       cy,
                                                       hd,
                                                       sc);
                                }
                                else
                                {
                                    _resume.BadBlocks.Add(currentBlock);
                                    mhddLog.Write(currentBlock, duration < 500 ? 65535 : duration);

                                    ibgLog.Write(currentBlock, 0);

                                    outputFormat.WriteSector(new byte[blockSize],
                                                             (ulong)((cy * heads + hd) * sectors + (sc - 1)));

                                    imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;
                                }

                                _writeStopwatch.Stop();

                                sectorSpeedStart++;
                                currentBlock++;

                                double elapsed = _speedStopwatch.Elapsed.TotalSeconds;

                                if(elapsed <= 0 || sectorSpeedStart * blockSize < 524288) continue;

                                currentSpeed     = sectorSpeedStart * blockSize / (1048576 * elapsed);
                                sectorSpeedStart = 0;
                                _speedStopwatch.Reset();
                            }
                        }
                    }

                    _speedStopwatch.Stop();
                    _resume.BadBlocks = _resume.BadBlocks.Distinct().ToList();

                    _dumpStopwatch.Stop();
                    EndProgress?.Invoke();
                    mhddLog.Close();

                    ibgLog.Close(_dev,
                                 blocks,
                                 blockSize,
                                 _dumpStopwatch.Elapsed.TotalSeconds,
                                 currentSpeed                     * 1024,
                                 blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000),
                                 _devicePath);

                    UpdateStatus?.Invoke(string.Format(Localization.Core.Dump_finished_in_0,
                                                       _dumpStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second)));

                    UpdateStatus?.Invoke(string.Format(Localization.Core.Average_dump_speed_0,
                                                       ByteSize.FromBytes(blockSize * (blocks + 1))
                                                               .Per(totalDuration.Milliseconds())
                                                               .Humanize()));

                    UpdateStatus?.Invoke(string.Format(Localization.Core.Average_write_speed_0,
                                                       ByteSize.FromBytes(blockSize * (blocks + 1))
                                                               .Per(imageWriteDuration.Seconds())
                                                               .Humanize()));

                    _dumpLog.WriteLine(string.Format(Localization.Core.Dump_finished_in_0,
                                                     _dumpStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second)));

                    _dumpLog.WriteLine(Localization.Core.Average_dump_speed_0,
                                       ByteSize.FromBytes(blockSize * (blocks + 1))
                                               .Per(totalDuration.Milliseconds())
                                               .Humanize());

                    _dumpLog.WriteLine(Localization.Core.Average_write_speed_0,
                                       ByteSize.FromBytes(blockSize * (blocks + 1))
                                               .Per(imageWriteDuration.Seconds())
                                               .Humanize());
                }

                foreach(ulong bad in _resume.BadBlocks)
                    _dumpLog.WriteLine(Localization.Core.Sector_0_could_not_be_read, bad);

                outputFormat.SetDumpHardware(_resume.Tries);

                // TODO: Non-removable
                var metadata = new CommonTypes.Structs.ImageInfo
                {
                    Application        = "Aaru",
                    ApplicationVersion = Version.GetVersion()
                };

                if(!outputFormat.SetImageInfo(metadata))
                {
                    ErrorMessage?.Invoke(Localization.Core.Error_0_setting_metadata +
                                         Environment.NewLine                        +
                                         outputFormat.ErrorMessage);
                }

                if(_preSidecar != null) outputFormat.SetMetadata(_preSidecar);

                _dumpLog.WriteLine(Localization.Core.Closing_output_file);
                UpdateStatus?.Invoke(Localization.Core.Closing_output_file);
                _imageCloseStopwatch.Restart();
                outputFormat.Close();
                _imageCloseStopwatch.Stop();

                UpdateStatus?.Invoke(string.Format(Localization.Core.Closed_in_0,
                                                   _imageCloseStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second)));

                _dumpLog.WriteLine(Localization.Core.Closed_in_0,
                                   _imageCloseStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second));

                if(_aborted)
                {
                    _dumpLog.WriteLine(Localization.Core.Aborted);
                    UpdateStatus?.Invoke(Localization.Core.Aborted);

                    return;
                }

                double totalChkDuration = 0;

                outputFormat.WriteMediaTag(ataIdentify, MediaTagType.ATA_IDENTIFY);

                if(_dev.IsUsb && _dev.UsbDescriptors != null)
                    outputFormat.WriteMediaTag(_dev.UsbDescriptors, MediaTagType.USB_Descriptors);

                if(_dev.IsPcmcia && _dev.Cis != null) outputFormat.WriteMediaTag(_dev.Cis, MediaTagType.PCMCIA_CIS);

                if(_metadata)
                {
                    _dumpLog.WriteLine(Localization.Core.Creating_sidecar);
                    UpdateStatus?.Invoke(Localization.Core.Creating_sidecar);
                    IFilter     filter      = PluginRegister.Singleton.GetFilter(_outputPath);
                    var         inputPlugin = ImageFormat.Detect(filter) as IMediaImage;
                    ErrorNumber opened      = inputPlugin.Open(filter);

                    if(opened != ErrorNumber.NoError)
                    {
                        StoppingErrorMessage?.Invoke(string.Format(Localization.Core.Error_0_opening_created_image,
                                                                   opened));

                        return;
                    }

                    _sidecarStopwatch.Restart();

                    _sidecarClass = new Sidecar(inputPlugin, _outputPath, filter.Id, _encoding);

                    _sidecarClass.InitProgressEvent    += InitProgress;
                    _sidecarClass.UpdateProgressEvent  += UpdateProgress;
                    _sidecarClass.EndProgressEvent     += EndProgress;
                    _sidecarClass.InitProgressEvent2   += InitProgress2;
                    _sidecarClass.UpdateProgressEvent2 += UpdateProgress2;
                    _sidecarClass.EndProgressEvent2    += EndProgress2;
                    _sidecarClass.UpdateStatusEvent    += UpdateStatus;
                    Metadata sidecar = _sidecarClass.Create();

                    if(!_aborted)
                    {
                        if(_preSidecar != null)
                        {
                            _preSidecar.BlockMedias = sidecar.BlockMedias;
                            sidecar                 = _preSidecar;
                        }

                        if(_dev.IsUsb && _dev.UsbDescriptors != null)
                        {
                            _dumpLog.WriteLine(Localization.Core.Reading_USB_descriptors);
                            UpdateStatus?.Invoke(Localization.Core.Reading_USB_descriptors);

                            sidecar.BlockMedias[0].Usb = new Usb
                            {
                                ProductID = _dev.UsbProductId,
                                VendorID  = _dev.UsbVendorId,
                                Descriptors = new CommonTypes.AaruMetadata.Dump
                                {
                                    Image     = _outputPath,
                                    Size      = (ulong)_dev.UsbDescriptors.Length,
                                    Checksums = Checksum.GetChecksums(_dev.UsbDescriptors)
                                }
                            };
                        }

                        if(_dev.IsPcmcia && _dev.Cis != null)
                        {
                            _dumpLog.WriteLine(Localization.Core.Reading_PCMCIA_CIS);
                            UpdateStatus?.Invoke(Localization.Core.Reading_PCMCIA_CIS);

                            sidecar.BlockMedias[0].Pcmcia = new Pcmcia
                            {
                                Cis = new CommonTypes.AaruMetadata.Dump
                                {
                                    Image     = _outputPath,
                                    Size      = (ulong)_dev.Cis.Length,
                                    Checksums = Checksum.GetChecksums(_dev.Cis)
                                }
                            };

                            _dumpLog.WriteLine(Localization.Core.Decoding_PCMCIA_CIS);
                            UpdateStatus?.Invoke(Localization.Core.Decoding_PCMCIA_CIS);
                            Tuple[] tuples = CIS.GetTuples(_dev.Cis);

                            if(tuples != null)
                            {
                                foreach(Tuple tuple in tuples)
                                {
                                    switch(tuple.Code)
                                    {
                                        case TupleCodes.CISTPL_MANFID:
                                            ManufacturerIdentificationTuple manufacturerId =
                                                CIS.DecodeManufacturerIdentificationTuple(tuple);

                                            if(manufacturerId != null)
                                            {
                                                sidecar.BlockMedias[0].Pcmcia.ManufacturerCode =
                                                    manufacturerId.ManufacturerID;

                                                sidecar.BlockMedias[0].Pcmcia.CardCode = manufacturerId.CardID;
                                            }

                                            break;
                                        case TupleCodes.CISTPL_VERS_1:
                                            Level1VersionTuple version = CIS.DecodeLevel1VersionTuple(tuple);

                                            if(version != null)
                                            {
                                                sidecar.BlockMedias[0].Pcmcia.Manufacturer = version.Manufacturer;
                                                sidecar.BlockMedias[0].Pcmcia.ProductName  = version.Product;

                                                sidecar.BlockMedias[0].Pcmcia.Compliance =
                                                    $"{version.MajorVersion}.{version.MinorVersion}";

                                                sidecar.BlockMedias[0].Pcmcia.AdditionalInformation =
                                                    new List<string>(version.AdditionalInformation);
                                            }

                                            break;
                                    }
                                }
                            }
                        }

                        if(_private) DeviceReport.ClearIdentify(ataIdentify);

                        sidecar.BlockMedias[0].ATA = new ATA
                        {
                            Identify = new CommonTypes.AaruMetadata.Dump
                            {
                                Image     = _outputPath,
                                Size      = (ulong)cmdBuf.Length,
                                Checksums = Checksum.GetChecksums(cmdBuf)
                            }
                        };

                        _sidecarStopwatch.Stop();

                        totalChkDuration = _sidecarStopwatch.Elapsed.TotalMilliseconds;

                        UpdateStatus?.Invoke(string.Format(Localization.Core.Sidecar_created_in_0,
                                                           _sidecarStopwatch.Elapsed
                                                                            .Humanize(minUnit: TimeUnit.Second)));

                        UpdateStatus?.Invoke(string.Format(Localization.Core.Average_checksum_speed_0,
                                                           ByteSize.FromBytes(blockSize * (blocks + 1))
                                                                   .Per(totalChkDuration.Milliseconds())
                                                                   .Humanize()));

                        _dumpLog.WriteLine(Localization.Core.Sidecar_created_in_0,
                                           _sidecarStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second));

                        _dumpLog.WriteLine(Localization.Core.Average_checksum_speed_0,
                                           ByteSize.FromBytes(blockSize * (blocks + 1))
                                                   .Per(totalChkDuration.Milliseconds())
                                                   .Humanize());

                        List<(ulong start, string type)> filesystems = new();

                        if(sidecar.BlockMedias[0].FileSystemInformation != null)
                        {
                            filesystems.AddRange(from partition in sidecar.BlockMedias[0].FileSystemInformation
                                                 where partition.FileSystems != null
                                                 from fileSystem in partition.FileSystems
                                                 select (partition.StartSector, fileSystem.Type));
                        }

                        if(filesystems.Count > 0)
                        {
                            foreach(var filesystem in filesystems.Select(o => new
                                                                  {
                                                                      o.start,
                                                                      o.type
                                                                  })
                                                                 .Distinct())
                            {
                                UpdateStatus?.Invoke(string.Format(Localization.Core.Found_filesystem_0_at_sector_1,
                                                                   filesystem.type,
                                                                   filesystem.start));

                                _dumpLog.WriteLine(Localization.Core.Found_filesystem_0_at_sector_1,
                                                   filesystem.type,
                                                   filesystem.start);
                            }
                        }

                        (string type, string subType) = CommonTypes.Metadata.MediaType.MediaTypeToString(mediaType);

                        sidecar.BlockMedias[0].MediaType         = type;
                        sidecar.BlockMedias[0].MediaSubType      = subType;
                        sidecar.BlockMedias[0].Interface         = "ATA";
                        sidecar.BlockMedias[0].LogicalBlocks     = blocks;
                        sidecar.BlockMedias[0].PhysicalBlockSize = physicalSectorSize;
                        sidecar.BlockMedias[0].LogicalBlockSize  = blockSize;
                        sidecar.BlockMedias[0].Manufacturer      = _dev.Manufacturer;
                        sidecar.BlockMedias[0].Model             = _dev.Model;

                        if(!_private) sidecar.BlockMedias[0].Serial = _dev.Serial;

                        sidecar.BlockMedias[0].Size = blocks * blockSize;

                        if(cylinders > 0 && heads > 0 && sectors > 0)
                        {
                            sidecar.BlockMedias[0].Cylinders       = cylinders;
                            sidecar.BlockMedias[0].Heads           = heads;
                            sidecar.BlockMedias[0].SectorsPerTrack = sectors;
                        }

                        UpdateStatus?.Invoke(Localization.Core.Writing_metadata_sidecar);

                        var jsonFs = new FileStream(_outputPrefix + ".metadata.json", FileMode.Create);

                        JsonSerializer.Serialize(jsonFs,
                                                 new MetadataJson
                                                 {
                                                     AaruMetadata = sidecar
                                                 },
                                                 typeof(MetadataJson),
                                                 MetadataJsonContext.Default);

                        jsonFs.Close();
                    }
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

                UpdateStatus?.Invoke(string.Format(Localization.Core._0_sectors_could_not_be_read,
                                                   _resume.BadBlocks.Count));

                if(_resume.BadBlocks.Count > 0) _resume.BadBlocks.Sort();

                UpdateStatus?.Invoke("");
            }

            Statistics.AddMedia(mediaType, true);
        }
        else
            StoppingErrorMessage?.Invoke(Localization.Core.Unable_to_communicate_with_ATA_device);
    }
}