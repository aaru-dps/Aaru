// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MiniDisc.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps MiniDisc devices.
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
using Aaru.Console;
using Aaru.Core.Graphics;
using Aaru.Core.Logging;
using Aaru.Decoders.SCSI;
using Aaru.Devices;
using Humanizer;
using Humanizer.Bytes;
using Humanizer.Localisation;
using Version = Aaru.CommonTypes.Interop.Version;

namespace Aaru.Core.Devices.Dumping;

/// <summary>Implements dumping MiniDisc Data devices</summary>
partial class Dump
{
    /// <summary>Dumps a MiniDisc Data device</summary>
    void MiniDisc()
    {
        bool                             sense;
        byte                             scsiMediumType = 0;
        const ushort                     sbcProfile     = 0x0001;
        double                           totalDuration  = 0;
        double                           currentSpeed   = 0;
        double                           maxSpeed       = double.MinValue;
        double                           minSpeed       = double.MaxValue;
        byte[]                           readBuffer;
        Modes.DecodedMode?               decMode   = null;
        Dictionary<MediaTagType, byte[]> mediaTags = new();
        bool                             ret;

        if(_outputPlugin is not IWritableImage outputFormat)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Image_is_not_writable_aborting);

            return;
        }

        _dumpLog.WriteLine(Localization.Core.Initializing_reader);
        var   scsiReader = new Reader(_dev, _dev.Timeout, null, _errorLog);
        ulong blocks     = scsiReader.GetDeviceBlocks();
        uint  blockSize  = scsiReader.LogicalBlockSize;

        _dumpLog.WriteLine(Localization.Core.Requesting_MODE_SENSE_6);
        UpdateStatus?.Invoke(Localization.Core.Requesting_MODE_SENSE_6);

        sense = _dev.ModeSense6(out byte[] cmdBuf, out _, true, ScsiModeSensePageControl.Current, 0x3F, 5, out _);

        if(!sense && !_dev.Error && Modes.DecodeMode6(cmdBuf, _dev.ScsiType).HasValue)
            decMode = Modes.DecodeMode6(cmdBuf, _dev.ScsiType);

        if(decMode.HasValue)
            scsiMediumType = (byte)(decMode?.Header.MediumType ?? default(MediumTypes));

        if(blockSize != 2048)
        {
            _dumpLog.WriteLine(Localization.Core.
                                            MiniDisc_albums_NetMD_discs_or_user_written_audio_MiniDisc_cannot_be_dumped);

            StoppingErrorMessage?.Invoke(Localization.Core.
                                                      MiniDisc_albums_NetMD_discs_or_user_written_audio_MiniDisc_cannot_be_dumped);

            return;
        }

        const MediaType dskType = MediaType.MDData;

        if(scsiReader.FindReadCommand())
        {
            _dumpLog.WriteLine(Localization.Core.ERROR_Cannot_find_correct_read_command_0, scsiReader.ErrorMessage);
            StoppingErrorMessage?.Invoke(Localization.Core.Unable_to_read_medium);

            return;
        }

        if(blocks != 0)
        {
            blocks++;

            UpdateStatus?.Invoke(string.Format(Localization.Core.Media_has_0_blocks_of_1_bytes_each_for_a_total_of_2,
                                               blocks, blockSize,
                                               ByteSize.FromBytes(blocks * blockSize).ToString("0.000")));
        }

        // Check how many blocks to read, if error show and return
        // 64 works, gets maximum speed (150KiB/s), slow I know...
        if(scsiReader.GetBlocksToRead())
        {
            _dumpLog.WriteLine(Localization.Core.ERROR_Cannot_get_blocks_to_read_0, scsiReader.ErrorMessage);
            StoppingErrorMessage?.Invoke(scsiReader.ErrorMessage);

            return;
        }

        uint blocksToRead      = scsiReader.BlocksToRead;
        uint physicalBlockSize = scsiReader.PhysicalBlockSize;

        if(blocks == 0)
        {
            _dumpLog.WriteLine(Localization.Core.ERROR_Unable_to_read_medium_or_empty_medium_present);
            StoppingErrorMessage?.Invoke(Localization.Core.Unable_to_read_medium_or_empty_medium_present);

            return;
        }

        UpdateStatus?.Invoke(string.Format(Localization.Core.Device_reports_0_blocks_1_bytes, blocks,
                                           blocks * blockSize));

        UpdateStatus?.Invoke(string.Format(Localization.Core.Device_can_read_0_blocks_at_a_time,       blocksToRead));
        UpdateStatus?.Invoke(string.Format(Localization.Core.Device_reports_0_bytes_per_logical_block, blockSize));

        UpdateStatus?.Invoke(string.Format(Localization.Core.Device_reports_0_bytes_per_physical_block,
                                           scsiReader.LongBlockSize));

        UpdateStatus?.Invoke(string.Format(Localization.Core.SCSI_device_type_0,    _dev.ScsiType));
        UpdateStatus?.Invoke(string.Format(Localization.Core.SCSI_medium_type_0,    scsiMediumType));
        UpdateStatus?.Invoke(string.Format(Localization.Core.Media_identified_as_0, dskType));

        _dumpLog.WriteLine(Localization.Core.Device_reports_0_blocks_1_bytes,           blocks, blocks * blockSize);
        _dumpLog.WriteLine(Localization.Core.Device_can_read_0_blocks_at_a_time,        blocksToRead);
        _dumpLog.WriteLine(Localization.Core.Device_reports_0_bytes_per_logical_block,  blockSize);
        _dumpLog.WriteLine(Localization.Core.Device_reports_0_bytes_per_physical_block, scsiReader.LongBlockSize);
        _dumpLog.WriteLine(Localization.Core.SCSI_device_type_0,                        _dev.ScsiType);
        _dumpLog.WriteLine(Localization.Core.SCSI_medium_type_0,                        scsiMediumType);
        _dumpLog.WriteLine(Localization.Core.Media_identified_as_0,                     dskType);

        sense = _dev.MiniDiscGetType(out cmdBuf, out _, _dev.Timeout, out _);

        if(!sense && !_dev.Error)
            mediaTags.Add(MediaTagType.MiniDiscType, cmdBuf);

        sense = _dev.MiniDiscD5(out cmdBuf, out _, _dev.Timeout, out _);

        if(!sense && !_dev.Error)
            mediaTags.Add(MediaTagType.MiniDiscD5, cmdBuf);

        sense = _dev.MiniDiscReadDataTOC(out cmdBuf, out _, _dev.Timeout, out _);

        if(!sense && !_dev.Error)
            mediaTags.Add(MediaTagType.MiniDiscDTOC, cmdBuf);

        var utocMs = new MemoryStream();

        for(uint i = 0; i < 3; i++)
        {
            sense = _dev.MiniDiscReadUserTOC(out cmdBuf, out _, i, _dev.Timeout, out _);

            if(sense || _dev.Error)
                break;

            utocMs.Write(cmdBuf, 0, 2336);
        }

        if(utocMs.Length > 0)
            mediaTags.Add(MediaTagType.MiniDiscUTOC, utocMs.ToArray());

        ret = true;

        foreach(MediaTagType tag in mediaTags.Keys.Where(tag => !outputFormat.SupportedMediaTags.Contains(tag)))
        {
            ret = false;
            _dumpLog.WriteLine(string.Format(Localization.Core.Output_format_does_not_support_0,   tag));
            ErrorMessage?.Invoke(string.Format(Localization.Core.Output_format_does_not_support_0, tag));
        }

        if(!ret)
        {
            if(_force)
            {
                _dumpLog.WriteLine(Localization.Core.Several_media_tags_not_supported_continuing);
                ErrorMessage?.Invoke(Localization.Core.Several_media_tags_not_supported_continuing);
            }
            else
            {
                _dumpLog.WriteLine(Localization.Core.Several_media_tags_not_supported_not_continuing);
                StoppingErrorMessage?.Invoke(Localization.Core.Several_media_tags_not_supported_not_continuing);

                return;
            }
        }

        UpdateStatus?.Invoke(string.Format(Localization.Core.Reading_0_sectors_at_a_time, blocksToRead));
        _dumpLog.WriteLine(Localization.Core.Reading_0_sectors_at_a_time, blocksToRead);

        var mhddLog = new MhddLog(_outputPrefix + ".mhddlog.bin", _dev, blocks, blockSize, blocksToRead, _private,
                                  _dimensions);

        var ibgLog = new IbgLog(_outputPrefix + ".ibg", sbcProfile);
        ret = outputFormat.Create(_outputPath, dskType, _formatOptions, blocks, blockSize);

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

        _dumpStopwatch.Restart();
        double imageWriteDuration = 0;

        if(decMode?.Pages != null)
        {
            var setGeometry = false;

            foreach(Modes.ModePage page in decMode.Value.Pages)
            {
                switch(page.Page)
                {
                    case 0x04 when page.Subpage == 0x00:
                    {
                        Modes.ModePage_04? rigidPage = Modes.DecodeModePage_04(page.PageResponse);

                        if(!rigidPage.HasValue || setGeometry)
                            continue;

                        _dumpLog.
                            WriteLine(Localization.Core.Setting_geometry_to_0_cylinders_1_heads_2_sectors_per_track,
                                      rigidPage.Value.Cylinders, rigidPage.Value.Heads,
                                      (uint)(blocks / (rigidPage.Value.Cylinders * rigidPage.Value.Heads)));

                        UpdateStatus?.
                            Invoke(string.
                                       Format(Localization.Core.Setting_geometry_to_0_cylinders_1_heads_2_sectors_per_track,
                                              rigidPage.Value.Cylinders, rigidPage.Value.Heads,
                                              (uint)(blocks / (rigidPage.Value.Cylinders * rigidPage.Value.Heads))));

                        outputFormat.SetGeometry(rigidPage.Value.Cylinders, rigidPage.Value.Heads,
                                                 (uint)(blocks / (rigidPage.Value.Cylinders * rigidPage.Value.Heads)));

                        setGeometry = true;

                        break;
                    }
                    case 0x05 when page.Subpage == 0x00:
                    {
                        Modes.ModePage_05? flexiblePage = Modes.DecodeModePage_05(page.PageResponse);

                        if(!flexiblePage.HasValue)
                            continue;

                        _dumpLog.
                            WriteLine(Localization.Core.Setting_geometry_to_0_cylinders_1_heads_2_sectors_per_track,
                                      flexiblePage.Value.Cylinders, flexiblePage.Value.Heads,
                                      flexiblePage.Value.SectorsPerTrack);

                        UpdateStatus?.
                            Invoke(string.
                                       Format(Localization.Core.Setting_geometry_to_0_cylinders_1_heads_2_sectors_per_track,
                                              flexiblePage.Value.Cylinders, flexiblePage.Value.Heads,
                                              flexiblePage.Value.SectorsPerTrack));

                        outputFormat.SetGeometry(flexiblePage.Value.Cylinders, flexiblePage.Value.Heads,
                                                 flexiblePage.Value.SectorsPerTrack);

                        setGeometry = true;

                        break;
                    }
                }
            }
        }

        DumpHardware currentTry = null;
        ExtentsULong extents    = null;

        ResumeSupport.Process(true, _dev.IsRemovable, blocks, _dev.Manufacturer, _dev.Model, _dev.Serial,
                              _dev.PlatformId, ref _resume, ref currentTry, ref extents, _dev.FirmwareRevision,
                              _private, _force);

        if(currentTry == null || extents == null)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Could_not_process_resume_file_not_continuing);

            return;
        }

        if(_resume.NextBlock > 0)
        {
            UpdateStatus?.Invoke(string.Format(Localization.Core.Resuming_from_block_0, _resume.NextBlock));
            _dumpLog.WriteLine(Localization.Core.Resuming_from_block_0, _resume.NextBlock);
        }

        if(_createGraph)
        {
            Spiral.DiscParameters discSpiralParameters = Spiral.DiscParametersFromMediaType(dskType);

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
        _speedStopwatch.Restart();
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

            if(blocks - i < blocksToRead)
                blocksToRead = (uint)(blocks - i);

            if(currentSpeed > maxSpeed && currentSpeed > 0)
                maxSpeed = currentSpeed;

            if(currentSpeed < minSpeed && currentSpeed > 0)
                minSpeed = currentSpeed;

            UpdateProgress?.
                Invoke(string.Format(Localization.Core.Reading_sector_0_of_1_2, i, blocks, ByteSize.FromMegabytes(currentSpeed).Per(_oneSecond).Humanize()),
                       (long)i, (long)blocks);

            sense = _dev.Read6(out readBuffer, out _, (uint)i, blockSize, (byte)blocksToRead, _dev.Timeout,
                               out double cmdDuration);

            totalDuration += cmdDuration;

            if(!sense && !_dev.Error)
            {
                mhddLog.Write(i, cmdDuration, blocksToRead);
                ibgLog.Write(i, currentSpeed * 1024);
                _writeStopwatch.Restart();
                outputFormat.WriteSectors(readBuffer, i, blocksToRead);
                imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;
                extents.Add(i, blocksToRead, true);
                _mediaGraph?.PaintSectorsGood(i, blocksToRead);
            }
            else
            {
                // TODO: Reset device after X errors
                if(_stopOnError)
                    return; // TODO: Return more cleanly

                if(i + _skip > blocks)
                    _skip = (uint)(blocks - i);

                // Write empty data
                _writeStopwatch.Restart();
                outputFormat.WriteSectors(new byte[blockSize * _skip], i, _skip);
                imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;

                for(ulong b = i; b < i + _skip; b++)
                    _resume.BadBlocks.Add(b);

                mhddLog.Write(i, cmdDuration < 500 ? 65535 : cmdDuration, _skip);

                ibgLog.Write(i, 0);
                _dumpLog.WriteLine(Localization.Core.Skipping_0_blocks_from_errored_block_1, _skip, i);
                i       += _skip - blocksToRead;
                newTrim =  true;
            }

            _writeStopwatch.Stop();
            sectorSpeedStart  += blocksToRead;
            _resume.NextBlock =  i + blocksToRead;

            double elapsed = _speedStopwatch.Elapsed.TotalSeconds;

            if(elapsed <= 0)
                continue;

            currentSpeed     = sectorSpeedStart * blockSize / (1048576 * elapsed);
            sectorSpeedStart = 0;
            _speedStopwatch.Restart();
        }

        _speedStopwatch.Stop();
        _resume.BadBlocks = _resume.BadBlocks.Distinct().ToList();

        _dumpStopwatch.Stop();
        EndProgress?.Invoke();
        mhddLog.Close();

        ibgLog.Close(_dev, blocks, blockSize, _dumpStopwatch.Elapsed.TotalSeconds, currentSpeed * 1024,
                     blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000), _devicePath);

        UpdateStatus?.Invoke(string.Format(Localization.Core.Dump_finished_in_0,
                                           _dumpStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second)));

        UpdateStatus?.Invoke(string.Format(Localization.Core.Average_dump_speed_0,
                                           ByteSize.FromBytes(blockSize * (blocks + 1)).
                                                    Per(totalDuration.Milliseconds())));

        UpdateStatus?.Invoke(string.Format(Localization.Core.Average_write_speed_0,
                                           ByteSize.FromBytes(blockSize * (blocks + 1)).
                                                    Per(imageWriteDuration.Seconds())));

        _dumpLog.WriteLine(string.Format(Localization.Core.Dump_finished_in_0,
                                         _dumpStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second)));

        _dumpLog.WriteLine(string.Format(Localization.Core.Average_dump_speed_0,
                                         ByteSize.FromBytes(blockSize * (blocks + 1)).
                                                  Per(totalDuration.Milliseconds())));

        _dumpLog.WriteLine(string.Format(Localization.Core.Average_write_speed_0,
                                         ByteSize.FromBytes(blockSize * (blocks + 1)).
                                                  Per(imageWriteDuration.Seconds())));

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

                sense = _dev.Read6(out readBuffer, out _, (uint)badSector, blockSize, 1, _dev.Timeout, out double _);

                if(sense || _dev.Error)
                    continue;

                _resume.BadBlocks.Remove(badSector);
                extents.Add(badSector);
                outputFormat.WriteSector(readBuffer, badSector);
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
            var pass              = 1;
            var forward           = true;
            var runningPersistent = false;

            Modes.ModePage? currentModePage = null;
            byte[]          md6;

            if(_persistent)
            {
                Modes.ModePage_01 pg;

                sense = _dev.ModeSense6(out readBuffer, out _, false, ScsiModeSensePageControl.Current, 0x01,
                                        _dev.Timeout, out _);

                if(!sense)
                {
                    Modes.DecodedMode? dcMode6 = Modes.DecodeMode6(readBuffer, _dev.ScsiType);

                    if(dcMode6?.Pages != null)
                    {
                        foreach(Modes.ModePage modePage in dcMode6.Value.Pages.Where(modePage =>
                            modePage is { Page: 0x01, Subpage: 0x00 }))
                            currentModePage = modePage;
                    }
                }

                if(currentModePage == null)
                {
                    pg = new Modes.ModePage_01
                    {
                        PS             = false,
                        AWRE           = true,
                        ARRE           = true,
                        TB             = false,
                        RC             = false,
                        EER            = true,
                        PER            = false,
                        DTE            = true,
                        DCR            = false,
                        ReadRetryCount = 32
                    };

                    currentModePage = new Modes.ModePage
                    {
                        Page         = 0x01,
                        Subpage      = 0x00,
                        PageResponse = Modes.EncodeModePage_01(pg)
                    };
                }

                pg = new Modes.ModePage_01
                {
                    PS             = false,
                    AWRE           = false,
                    ARRE           = false,
                    TB             = true,
                    RC             = false,
                    EER            = true,
                    PER            = false,
                    DTE            = false,
                    DCR            = false,
                    ReadRetryCount = 255
                };

                var md = new Modes.DecodedMode
                {
                    Header = new Modes.ModeHeader(),
                    Pages = new[]
                    {
                        new Modes.ModePage
                        {
                            Page         = 0x01,
                            Subpage      = 0x00,
                            PageResponse = Modes.EncodeModePage_01(pg)
                        }
                    }
                };

                md6 = Modes.EncodeMode6(md, _dev.ScsiType);

                UpdateStatus?.Invoke(Localization.Core.Sending_MODE_SELECT_to_drive_return_damaged_blocks);
                _dumpLog.WriteLine(Localization.Core.Sending_MODE_SELECT_to_drive_return_damaged_blocks);
                sense = _dev.ModeSelect(md6, out byte[] senseBuf, true, false, _dev.Timeout, out _);

                if(sense)
                {
                    UpdateStatus?.Invoke(Localization.Core.
                                                      Drive_did_not_accept_MODE_SELECT_command_for_persistent_error_reading);

                    AaruConsole.DebugWriteLine(Localization.Core.Error_0, Sense.PrettifySense(senseBuf));

                    _dumpLog.WriteLine(Localization.Core.
                                                    Drive_did_not_accept_MODE_SELECT_command_for_persistent_error_reading);
                }
                else
                    runningPersistent = true;
            }

            InitProgress?.Invoke();
        repeatRetry:
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
                    PulseProgress?.Invoke(runningPersistent
                                              ? string.
                                                  Format(Localization.Core.Retrying_sector_0_pass_1_recovering_partial_data_forward,
                                                         badSector, pass)
                                              : string.Format(Localization.Core.Retrying_sector_0_pass_1_forward,
                                                              badSector, pass));
                }
                else
                {
                    PulseProgress?.Invoke(runningPersistent
                                              ? string.
                                                  Format(Localization.Core.Retrying_sector_0_pass_1_recovering_partial_data_reverse,
                                                         badSector, pass)
                                              : string.Format(Localization.Core.Retrying_sector_0_pass_1_reverse,
                                                              badSector, pass));
                }

                sense = _dev.Read6(out readBuffer, out _, (uint)badSector, blockSize, 1, _dev.Timeout,
                                   out double cmdDuration);

                totalDuration += cmdDuration;

                if(!sense && !_dev.Error)
                {
                    _resume.BadBlocks.Remove(badSector);
                    extents.Add(badSector);
                    outputFormat.WriteSector(readBuffer, badSector);
                    _mediaGraph?.PaintSectorGood(badSector);

                    UpdateStatus?.Invoke(string.Format(Localization.Core.Correctly_retried_block_0_in_pass_1, badSector,
                                                       pass));

                    _dumpLog.WriteLine(Localization.Core.Correctly_retried_block_0_in_pass_1, badSector, pass);
                }
                else if(runningPersistent)
                    outputFormat.WriteSector(readBuffer, badSector);
            }

            if(pass < _retryPasses && !_aborted && _resume.BadBlocks.Count > 0)
            {
                pass++;
                forward = !forward;
                _resume.BadBlocks.Sort();

                if(!forward)
                    _resume.BadBlocks.Reverse();

                goto repeatRetry;
            }

            if(runningPersistent && currentModePage.HasValue)
            {
                var md = new Modes.DecodedMode
                {
                    Header = new Modes.ModeHeader(),
                    Pages = new[]
                    {
                        currentModePage.Value
                    }
                };

                md6 = Modes.EncodeMode6(md, _dev.ScsiType);

                UpdateStatus?.Invoke(Localization.Core.Sending_MODE_SELECT_to_drive_return_device_to_previous_status);
                _dumpLog.WriteLine(Localization.Core.Sending_MODE_SELECT_to_drive_return_device_to_previous_status);
                _dev.ModeSelect(md6, out _, true, false, _dev.Timeout, out _);
            }

            EndProgress?.Invoke();
        }

    #endregion Error handling

        _resume.BadBlocks.Sort();

        foreach(ulong bad in _resume.BadBlocks)
            _dumpLog.WriteLine(Localization.Core.Sector_0_could_not_be_read, bad);

        currentTry.Extents = ExtentsConverter.ToMetadata(extents);

        outputFormat.SetDumpHardware(_resume.Tries);

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

        if(_preSidecar != null)
            outputFormat.SetMetadata(_preSidecar);

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
            UpdateStatus?.Invoke(Localization.Core.Aborted);
            _dumpLog.WriteLine(Localization.Core.Aborted);

            return;
        }

        double totalChkDuration = 0;

        if(_metadata)
        {
            UpdateStatus?.Invoke(Localization.Core.Creating_sidecar);
            _dumpLog.WriteLine(Localization.Core.Creating_sidecar);
            IFilter     filter      = PluginRegister.Singleton.GetFilter(_outputPath);
            var         inputPlugin = ImageFormat.Detect(filter) as IMediaImage;
            ErrorNumber opened      = inputPlugin.Open(filter);

            if(opened != ErrorNumber.NoError)
            {
                StoppingErrorMessage?.Invoke(string.Format(Localization.Core.Error_0_opening_created_image, opened));

                return;
            }

            _sidecarStopwatch.Restart();
            _sidecarClass                      =  new Sidecar(inputPlugin, _outputPath, filter.Id, _encoding);
            _sidecarClass.InitProgressEvent    += InitProgress;
            _sidecarClass.UpdateProgressEvent  += UpdateProgress;
            _sidecarClass.EndProgressEvent     += EndProgress;
            _sidecarClass.InitProgressEvent2   += InitProgress2;
            _sidecarClass.UpdateProgressEvent2 += UpdateProgress2;
            _sidecarClass.EndProgressEvent2    += EndProgress2;
            _sidecarClass.UpdateStatusEvent    += UpdateStatus;
            Metadata sidecar = _sidecarClass.Create();
            _sidecarStopwatch.Stop();

            if(!_aborted)
            {
                totalChkDuration = _sidecarStopwatch.Elapsed.TotalMilliseconds;

                UpdateStatus?.Invoke(string.Format(Localization.Core.Sidecar_created_in_0,
                                                   _sidecarStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second)));

                UpdateStatus?.Invoke(string.Format(Localization.Core.Average_checksum_speed_0,
                                                   ByteSize.FromBytes(blockSize * (blocks + 1)).
                                                            Per(totalChkDuration.Milliseconds())));

                _dumpLog.WriteLine(Localization.Core.Sidecar_created_in_0,
                                   _sidecarStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second));

                _dumpLog.WriteLine(Localization.Core.Average_checksum_speed_0,
                                   ByteSize.FromBytes(blockSize * (blocks + 1)).
                                            Per(totalChkDuration.Milliseconds()).
                                            Humanize());

                if(_preSidecar != null)
                {
                    _preSidecar.BlockMedias = sidecar.BlockMedias;
                    sidecar                 = _preSidecar;
                }

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
                                                          }).
                                                          Distinct())
                    {
                        UpdateStatus?.Invoke(string.Format(Localization.Core.Found_filesystem_0_at_sector_1,
                                                           filesystem.type, filesystem.start));

                        _dumpLog.WriteLine(Localization.Core.Found_filesystem_0_at_sector_1, filesystem.type,
                                           filesystem.start);
                    }
                }

                sidecar.BlockMedias[0].Dimensions = Dimensions.FromMediaType(dskType);
                (string type, string subType) xmlType = CommonTypes.Metadata.MediaType.MediaTypeToString(dskType);
                sidecar.BlockMedias[0].MediaType    = xmlType.type;
                sidecar.BlockMedias[0].MediaSubType = xmlType.subType;

                if(!_dev.IsRemovable || _dev.IsUsb)
                {
                    if(_dev.Type == DeviceType.ATAPI)
                        sidecar.BlockMedias[0].Interface = "ATAPI";
                    else if(_dev.IsUsb)
                        sidecar.BlockMedias[0].Interface = "USB";
                    else if(_dev.IsFireWire)
                        sidecar.BlockMedias[0].Interface = "FireWire";
                    else
                        sidecar.BlockMedias[0].Interface = "SCSI";
                }

                sidecar.BlockMedias[0].LogicalBlocks     = blocks;
                sidecar.BlockMedias[0].PhysicalBlockSize = physicalBlockSize;
                sidecar.BlockMedias[0].LogicalBlockSize  = blockSize;
                sidecar.BlockMedias[0].Manufacturer      = _dev.Manufacturer;
                sidecar.BlockMedias[0].Model             = _dev.Model;

                if(!_private)
                    sidecar.BlockMedias[0].Serial = _dev.Serial;

                sidecar.BlockMedias[0].Size = blocks * blockSize;

                if(_dev.IsRemovable)
                    sidecar.BlockMedias[0].DumpHardware = _resume.Tries;

                UpdateStatus?.Invoke(Localization.Core.Writing_metadata_sidecar);

                var jsonFs = new FileStream(_outputPrefix + ".metadata.json", FileMode.Create);

                JsonSerializer.Serialize(jsonFs, new MetadataJson
                {
                    AaruMetadata = sidecar
                }, typeof(MetadataJson), MetadataJsonContext.Default);

                jsonFs.Close();
            }
        }

        UpdateStatus?.Invoke("");

        UpdateStatus?.
            Invoke(string.Format(Localization.Core.Took_a_total_of_0_1_processing_commands_2_checksumming_3_writing_4_closing,
                                 _dumpStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second),
                                 totalDuration.Milliseconds().Humanize(minUnit: TimeUnit.Second),
                                 totalChkDuration.Milliseconds().Humanize(minUnit: TimeUnit.Second),
                                 imageWriteDuration.Seconds().Humanize(minUnit: TimeUnit.Second),
                                 _imageCloseStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second)));

        UpdateStatus?.Invoke(string.Format(Localization.Core.Average_speed_0,
                                           ByteSize.FromBytes(blockSize * (blocks + 1)).
                                                    Per(totalDuration.Milliseconds()).
                                                    Humanize()));

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
}