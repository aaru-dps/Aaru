// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SSC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps media from SCSI Streaming devices.
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
using System.Threading;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core.Logging;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.SCSI.SSC;
using Aaru.Devices;
using Aaru.Helpers;
using Humanizer;
using Humanizer.Bytes;
using Humanizer.Localisation;
using TapeFile = Aaru.CommonTypes.Structs.TapeFile;
using TapePartition = Aaru.CommonTypes.Structs.TapePartition;
using Version = Aaru.CommonTypes.Interop.Version;

namespace Aaru.Core.Devices.Dumping;

partial class Dump
{
    /// <summary>Dumps the tape from a SCSI Streaming device</summary>
    void Ssc()
    {
        DecodedSense? decSense;
        bool          sense;
        uint          blockSize;
        ulong         blocks = 0;
        MediaType     dskType;
        double        totalDuration = 0;
        double        currentSpeed  = 0;
        double        maxSpeed      = double.MinValue;
        double        minSpeed      = double.MaxValue;
        var           outputTape    = _outputPlugin as IWritableTapeImage;

        _dev.RequestSense(out byte[] senseBuf, _dev.Timeout, out double duration);
        decSense = Sense.Decode(senseBuf);

        InitProgress?.Invoke();

        if(decSense.HasValue && decSense.Value.SenseKey != SenseKeys.NoSense)
        {
            _dumpLog.WriteLine(Localization.Core.Device_not_ready_Sense, decSense.Value.SenseKey, decSense.Value.ASC,
                               decSense.Value.ASCQ);

            StoppingErrorMessage?.Invoke(Localization.Core.Drive_has_status_error_please_correct_Sense_follows +
                                         Environment.NewLine                                                   +
                                         decSense.Value.Description);

            return;
        }

        // Not in BOM/P
        if(decSense is { ASC: 0x00 }       &&
           decSense.Value.ASCQ     != 0x00 &&
           decSense.Value.ASCQ     != 0x04 &&
           decSense.Value.SenseKey != SenseKeys.IllegalRequest)
        {
            _dumpLog.WriteLine(Localization.Core.Rewinding_please_wait);
            PulseProgress?.Invoke(Localization.Core.Rewinding_please_wait);

            // Rewind, let timeout apply
            _dev.Rewind(out senseBuf, _dev.Timeout, out duration);

            // Still rewinding?
            // TODO: Pause?
            do
            {
                PulseProgress?.Invoke(Localization.Core.Rewinding_please_wait);
                _dev.RequestSense(out senseBuf, _dev.Timeout, out duration);
                decSense = Sense.Decode(senseBuf);
            } while(decSense is { ASC: 0x00, ASCQ: 0x1A or not (0x04 and 0x00) });

            _dev.RequestSense(out senseBuf, _dev.Timeout, out duration);
            decSense = Sense.Decode(senseBuf);

            // And yet, did not rewind!
            if(decSense.HasValue &&
               (decSense.Value.ASC == 0x00 && decSense.Value.ASCQ != 0x04 && decSense.Value.ASCQ != 0x00 ||
                decSense.Value.ASC != 0x00))
            {
                StoppingErrorMessage?.Invoke(Localization.Core.Drive_could_not_rewind_please_correct_Sense_follows +
                                             Environment.NewLine                                                   +
                                             decSense.Value.Description);

                _dumpLog.WriteLine(Localization.Core.Drive_could_not_rewind_please_correct_Sense_follows);

                _dumpLog.WriteLine(Localization.Core.Device_not_ready_Sense, decSense.Value.SenseKey,
                                   decSense.Value.ASC, decSense.Value.ASCQ);

                return;
            }
        }

        // Check position
        sense = _dev.ReadPosition(out byte[] cmdBuf, out senseBuf, SscPositionForms.Short, _dev.Timeout, out duration);

        if(sense)
        {
            // READ POSITION is mandatory starting SCSI-2, so do not cry if the drive does not recognize the command (SCSI-1 or earlier)
            // Anyway, <=SCSI-1 tapes do not support partitions
            decSense = Sense.Decode(senseBuf);

            if(decSense.HasValue &&
               (decSense.Value.ASC == 0x20 && decSense.Value.ASCQ     != 0x00 ||
                decSense.Value.ASC != 0x20 && decSense.Value.SenseKey != SenseKeys.IllegalRequest))
            {
                StoppingErrorMessage?.Invoke(Localization.Core.Could_not_get_position_Sense_follows +
                                             Environment.NewLine                                    +
                                             decSense.Value.Description);

                _dumpLog.WriteLine(Localization.Core.Could_not_get_position_Sense_follows);

                _dumpLog.WriteLine(Localization.Core.Device_not_ready_Sense, decSense.Value.SenseKey,
                                   decSense.Value.ASC, decSense.Value.ASCQ);

                return;
            }
        }
        else
        {
            // Not in partition 0
            if(cmdBuf[1] != 0)
            {
                UpdateStatus?.Invoke(Localization.Core.Drive_not_in_partition_0_Rewinding_please_wait);
                _dumpLog.WriteLine(Localization.Core.Drive_not_in_partition_0_Rewinding_please_wait);

                // Rewind, let timeout apply
                sense = _dev.Locate(out senseBuf, false, 0, 0, _dev.Timeout, out duration);

                if(sense)
                {
                    StoppingErrorMessage?.Invoke(Localization.Core.Drive_could_not_rewind_please_correct_Sense_follows +
                                                 Environment.NewLine                                                   +
                                                 decSense?.Description);

                    _dumpLog.WriteLine(Localization.Core.Drive_could_not_rewind_please_correct_Sense_follows);

                    _dumpLog.WriteLine(Localization.Core.Device_not_ready_Sense, decSense?.SenseKey, decSense?.ASC,
                                       decSense?.ASCQ);

                    return;
                }

                // Still rewinding?
                // TODO: Pause?
                do
                {
                    Thread.Sleep(1000);
                    PulseProgress?.Invoke(Localization.Core.Rewinding_please_wait);
                    _dev.RequestSense(out senseBuf, _dev.Timeout, out duration);
                    decSense = Sense.Decode(senseBuf);
                } while(decSense is { ASC: 0x00, ASCQ: 0x1A or 0x19 });

                // And yet, did not rewind!
                if(decSense.HasValue &&
                   (decSense.Value.ASC == 0x00 && decSense.Value.ASCQ != 0x04 && decSense.Value.ASCQ != 0x00 ||
                    decSense.Value.ASC != 0x00))
                {
                    StoppingErrorMessage?.Invoke(Localization.Core.Drive_could_not_rewind_please_correct_Sense_follows +
                                                 Environment.NewLine                                                   +
                                                 decSense.Value.Description);

                    _dumpLog.WriteLine(Localization.Core.Drive_could_not_rewind_please_correct_Sense_follows);

                    _dumpLog.WriteLine(Localization.Core.Device_not_ready_Sense, decSense.Value.SenseKey,
                                       decSense.Value.ASC, decSense.Value.ASCQ);

                    return;
                }

                sense = _dev.ReadPosition(out cmdBuf, out senseBuf, SscPositionForms.Short, _dev.Timeout, out duration);

                if(sense)
                {
                    decSense = Sense.Decode(senseBuf);

                    StoppingErrorMessage?.Invoke(Localization.Core.Drive_could_not_rewind_please_correct_Sense_follows +
                                                 Environment.NewLine                                                   +
                                                 decSense?.Description);

                    _dumpLog.WriteLine(Localization.Core.Drive_could_not_rewind_please_correct_Sense_follows);

                    _dumpLog.WriteLine(Localization.Core.Device_not_ready_Sense, decSense?.SenseKey, decSense?.ASC,
                                       decSense?.ASCQ);

                    return;
                }

                // Still not in partition 0!!!?
                if(cmdBuf[1] != 0)
                {
                    StoppingErrorMessage?.Invoke(Localization.Core.
                                                              Drive_could_not_rewind_to_partition_0_but_no_error_occurred);

                    _dumpLog.WriteLine(Localization.Core.Drive_could_not_rewind_to_partition_0_but_no_error_occurred);

                    return;
                }
            }
        }

        EndProgress?.Invoke();

        byte   scsiMediumTypeTape  = 0;
        byte   scsiDensityCodeTape = 0;
        byte[] mode6Data           = null;
        byte[] mode10Data          = null;

        UpdateStatus?.Invoke(Localization.Core.Requesting_MODE_SENSE_10);

        sense = _dev.ModeSense10(out cmdBuf, out senseBuf, false, true, ScsiModeSensePageControl.Current, 0x3F, 0xFF, 5,
                                 out duration);

        if(!sense || _dev.Error)
        {
            sense = _dev.ModeSense10(out cmdBuf, out senseBuf, false, true, ScsiModeSensePageControl.Current, 0x3F,
                                     0x00, 5, out duration);
        }

        Modes.DecodedMode? decMode = null;

        if(!sense && !_dev.Error)
        {
            if(Modes.DecodeMode10(cmdBuf, _dev.ScsiType).HasValue)
                decMode = Modes.DecodeMode10(cmdBuf, _dev.ScsiType);
        }

        UpdateStatus?.Invoke(Localization.Core.Requesting_MODE_SENSE_6);

        sense = _dev.ModeSense6(out cmdBuf, out senseBuf, false, ScsiModeSensePageControl.Current, 0x3F, 0x00, 5,
                                out duration);

        if(sense || _dev.Error)
        {
            sense = _dev.ModeSense6(out cmdBuf, out senseBuf, false, ScsiModeSensePageControl.Current, 0x3F, 0x00, 5,
                                    out duration);
        }

        if(sense || _dev.Error)
            sense = _dev.ModeSense(out cmdBuf, out senseBuf, 5, out duration);

        if(!sense && !_dev.Error)
        {
            if(Modes.DecodeMode6(cmdBuf, _dev.ScsiType).HasValue)
                decMode = Modes.DecodeMode6(cmdBuf, _dev.ScsiType);
        }

        // TODO: Check partitions page
        if(decMode.HasValue)
        {
            scsiMediumTypeTape = (byte)decMode.Value.Header.MediumType;

            if(decMode.Value.Header.BlockDescriptors?.Length > 0)
                scsiDensityCodeTape = (byte)decMode.Value.Header.BlockDescriptors[0].Density;

            blockSize = decMode.Value.Header.BlockDescriptors?[0].BlockLength ?? 0;

            UpdateStatus?.Invoke(string.Format(Localization.Core.Device_reports_0_blocks, blocks));
        }
        else
            blockSize = 1;

        if(!_dev.ReadBlockLimits(out cmdBuf, out senseBuf, _dev.Timeout, out _))
        {
            BlockLimits.BlockLimitsData? blockLimits = BlockLimits.Decode(cmdBuf);

            if(blockLimits?.minBlockLen > blockSize)
                blockSize = blockLimits?.minBlockLen ?? 0;
        }

        if(blockSize == 0)
            blockSize = 1;

        dskType = MediaTypeFromDevice.GetFromScsi((byte)_dev.ScsiType, _dev.Manufacturer, _dev.Model,
                                                  scsiMediumTypeTape, scsiDensityCodeTape, blocks, blockSize,
                                                  _dev.IsUsb, false);

        if(dskType == MediaType.Unknown)
            dskType = MediaType.UnknownTape;

        UpdateStatus?.Invoke(string.Format(Localization.Core.SCSI_device_type_0,    _dev.ScsiType));
        UpdateStatus?.Invoke(string.Format(Localization.Core.SCSI_medium_type_0,    scsiMediumTypeTape));
        UpdateStatus?.Invoke(string.Format(Localization.Core.SCSI_density_type_0,   scsiDensityCodeTape));
        UpdateStatus?.Invoke(string.Format(Localization.Core.Media_identified_as_0, dskType));

        _dumpLog.WriteLine(Localization.Core.SCSI_device_type_0,    _dev.ScsiType);
        _dumpLog.WriteLine(Localization.Core.SCSI_medium_type_0,    scsiMediumTypeTape);
        _dumpLog.WriteLine(Localization.Core.SCSI_density_type_0,   scsiDensityCodeTape);
        _dumpLog.WriteLine(Localization.Core.Media_identified_as_0, dskType);

        var   endOfMedia       = false;
        ulong currentBlock     = 0;
        uint  currentFile      = 0;
        byte  currentPartition = 0;
        byte  totalPartitions  = 1; // TODO: Handle partitions.
        var   fixedLen         = false;
        uint  transferLen      = blockSize;

    firstRead:

        sense = _dev.Read6(out cmdBuf, out senseBuf, false, fixedLen, transferLen, blockSize, _dev.Timeout,
                           out duration);

        if(sense)
        {
            decSense = Sense.Decode(senseBuf);

            if(decSense.HasValue)
            {
                switch(decSense)
                {
                    case { SenseKey: SenseKeys.IllegalRequest }:
                    {
                        sense = _dev.Space(out senseBuf, SscSpaceCodes.LogicalBlock, -1, _dev.Timeout, out duration);

                        if(sense)
                        {
                            decSense = Sense.Decode(senseBuf);

                            bool eom = decSense?.Fixed?.EOM == true;

                            if(decSense?.Descriptor != null &&
                               decSense.Value.Descriptor.Value.Descriptors.TryGetValue(4, out byte[] sscDescriptor))
                                Sense.DecodeDescriptor04(sscDescriptor, out _, out eom, out _);

                            if(!eom)
                            {
                                StoppingErrorMessage?.Invoke(Localization.Core.
                                                                          Drive_could_not_return_back_Sense_follows +
                                                             Environment.NewLine                                    +
                                                             decSense.Value.Description);

                                _dumpLog.WriteLine(Localization.Core.Drive_could_not_return_back_Sense_follows);

                                _dumpLog.WriteLine(Localization.Core.Device_not_ready_Sense, decSense.Value.SenseKey,
                                                   decSense.Value.ASC, decSense.Value.ASCQ);

                                return;
                            }
                        }

                        fixedLen    = true;
                        transferLen = 1;

                        sense = _dev.Read6(out cmdBuf, out senseBuf, false, fixedLen, transferLen, blockSize,
                                           _dev.Timeout, out duration);

                        if(sense)
                        {
                            decSense = Sense.Decode(senseBuf);

                            StoppingErrorMessage?.Invoke(Localization.Core.Drive_could_not_read_Sense_follows +
                                                         Environment.NewLine                                  +
                                                         decSense.Value.Description);

                            _dumpLog.WriteLine(Localization.Core.Drive_could_not_read_Sense_follows);

                            _dumpLog.WriteLine(Localization.Core.Device_not_ready_Sense, decSense.Value.SenseKey,
                                               decSense.Value.ASC, decSense.Value.ASCQ);

                            return;
                        }

                        break;
                    }
                    case { ASC: 0x00, ASCQ: 0x00 }:
                    {
                        bool ili         = decSense.Value.Fixed?.ILI              == true;
                        bool valid       = decSense.Value.Fixed?.InformationValid == true;
                        uint information = decSense.Value.Fixed?.Information ?? 0;

                        if(decSense.Value.Descriptor.HasValue)
                        {
                            valid = decSense.Value.Descriptor.Value.Descriptors.TryGetValue(0, out byte[] desc00);

                            if(valid)
                                information = (uint)Sense.DecodeDescriptor00(desc00);

                            if(decSense.Value.Descriptor.Value.Descriptors.TryGetValue(4, out byte[] desc04))
                                Sense.DecodeDescriptor04(desc04, out _, out _, out ili);
                        }

                        if(ili && valid)
                        {
                            blockSize = (uint)((int)blockSize -
                                               BitConverter.ToInt32(BitConverter.GetBytes(information), 0));

                            transferLen = blockSize;

                            UpdateStatus?.
                                Invoke(string.Format(Localization.Core.Blocksize_changed_to_0_bytes_at_block_1,
                                                     blockSize, currentBlock));

                            _dumpLog.WriteLine(Localization.Core.Blocksize_changed_to_0_bytes_at_block_1, blockSize,
                                               currentBlock);

                            sense = _dev.Space(out senseBuf, SscSpaceCodes.LogicalBlock, -1, _dev.Timeout,
                                               out duration);

                            totalDuration += duration;

                            if(sense)
                            {
                                decSense = Sense.Decode(senseBuf);

                                StoppingErrorMessage?.Invoke(Localization.Core.
                                                                          Drive_could_not_go_back_one_block_Sense_follows +
                                                             Environment.NewLine +
                                                             decSense.Value.Description);

                                _dumpLog.WriteLine(Localization.Core.Drive_could_not_go_back_one_block_Sense_follows);

                                _dumpLog.WriteLine(Localization.Core.Device_not_ready_Sense, decSense.Value.SenseKey,
                                                   decSense.Value.ASC, decSense.Value.ASCQ);

                                return;
                            }

                            goto firstRead;
                        }

                        StoppingErrorMessage?.Invoke(Localization.Core.Drive_could_not_read_Sense_follows +
                                                     Environment.NewLine                                  +
                                                     decSense.Value.Description);

                        _dumpLog.WriteLine(Localization.Core.Drive_could_not_read_Sense_follows);

                        _dumpLog.WriteLine(Localization.Core.Device_not_ready_Sense, decSense.Value.SenseKey,
                                           decSense.Value.ASC, decSense.Value.ASCQ);

                        return;
                    }
                    default:
                        StoppingErrorMessage?.Invoke(Localization.Core.Drive_could_not_read_Sense_follows +
                                                     Environment.NewLine                                  +
                                                     decSense.Value.Description);

                        _dumpLog.WriteLine(Localization.Core.Drive_could_not_read_Sense_follows);

                        _dumpLog.WriteLine(Localization.Core.Device_not_ready_Sense, decSense.Value.SenseKey,
                                           decSense.Value.ASC, decSense.Value.ASCQ);

                        return;
                }
            }
            else
            {
                StoppingErrorMessage?.Invoke(Localization.Core.Cannot_read_device_dont_know_why_exiting);
                _dumpLog.WriteLine(Localization.Core.Cannot_read_device_dont_know_why_exiting);

                return;
            }
        }

        sense = _dev.Space(out senseBuf, SscSpaceCodes.LogicalBlock, -1, _dev.Timeout, out duration);

        if(sense)
        {
            decSense = Sense.Decode(senseBuf);

            bool eom = decSense?.Fixed?.EOM == true;

            if(decSense.Value.Descriptor.HasValue &&
               decSense.Value.Descriptor.Value.Descriptors.TryGetValue(4, out byte[] desc04))
                Sense.DecodeDescriptor04(desc04, out _, out eom, out _);

            if(!eom)
            {
                StoppingErrorMessage?.Invoke(Localization.Core.Drive_could_not_return_back_Sense_follows +
                                             Environment.NewLine                                         +
                                             decSense.Value.Description);

                _dumpLog.WriteLine(Localization.Core.Drive_could_not_return_back_Sense_follows);

                _dumpLog.WriteLine(Localization.Core.Device_not_ready_Sense, decSense.Value.SenseKey,
                                   decSense.Value.ASC, decSense.Value.ASCQ);

                return;
            }
        }

        DumpHardware currentTry = null;
        ExtentsULong extents    = null;

        ResumeSupport.Process(true, _dev.IsRemovable, blocks, _dev.Manufacturer, _dev.Model, _dev.Serial,
                              _dev.PlatformId, ref _resume, ref currentTry, ref extents, _dev.FirmwareRevision,
                              _private, _force, true);

        if(currentTry == null || extents == null)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Could_not_process_resume_file_not_continuing);

            return;
        }

        var canLocateLong = false;
        var canLocate     = false;

        UpdateStatus?.Invoke(Localization.Core.Positioning_tape_to_block_1);
        _dumpLog.WriteLine(Localization.Core.Positioning_tape_to_block_1);

        sense = _dev.Locate16(out senseBuf, 1, _dev.Timeout, out _);

        if(!sense)
        {
            sense = _dev.ReadPositionLong(out cmdBuf, out senseBuf, _dev.Timeout, out _);

            if(!sense)
            {
                ulong position = Swapping.Swap(BitConverter.ToUInt64(cmdBuf, 8));

                if(position == 1)
                {
                    canLocateLong = true;
                    UpdateStatus?.Invoke(Localization.Core.LOCATE_LONG_works);
                    _dumpLog.WriteLine(Localization.Core.LOCATE_LONG_works);
                }
            }
        }

        sense = _dev.Locate(out senseBuf, 1, _dev.Timeout, out _);

        if(!sense)
        {
            sense = _dev.ReadPosition(out cmdBuf, out senseBuf, _dev.Timeout, out _);

            if(!sense)
            {
                ulong position = Swapping.Swap(BitConverter.ToUInt32(cmdBuf, 4));

                if(position == 1)
                {
                    canLocate = true;
                    UpdateStatus?.Invoke(Localization.Core.LOCATE_works);
                    _dumpLog.WriteLine(Localization.Core.LOCATE_works);
                }
            }
        }

        if(_resume.NextBlock > 0)
        {
            UpdateStatus?.Invoke(string.Format(Localization.Core.Positioning_tape_to_block_0, _resume.NextBlock));
            _dumpLog.WriteLine(Localization.Core.Positioning_tape_to_block_0, _resume.NextBlock);

            if(canLocateLong)
            {
                sense = _dev.Locate16(out senseBuf, _resume.NextBlock, _dev.Timeout, out _);

                if(!sense)
                {
                    sense = _dev.ReadPositionLong(out cmdBuf, out senseBuf, _dev.Timeout, out _);

                    if(sense)
                    {
                        if(!_force)
                        {
                            _dumpLog.WriteLine(Localization.Core.
                                                            Could_not_check_current_position_unable_to_resume_If_you_want_to_continue_use_force);

                            StoppingErrorMessage?.Invoke(Localization.Core.
                                                                      Could_not_check_current_position_unable_to_resume_If_you_want_to_continue_use_force);

                            return;
                        }

                        _dumpLog.WriteLine(Localization.Core.
                                                        Could_not_check_current_position_unable_to_resume_Dumping_from_the_start);

                        ErrorMessage?.Invoke(Localization.Core.
                                                          Could_not_check_current_position_unable_to_resume_Dumping_from_the_start);

                        canLocateLong = false;
                    }
                    else
                    {
                        ulong position = Swapping.Swap(BitConverter.ToUInt64(cmdBuf, 8));

                        if(position != _resume.NextBlock)
                        {
                            if(!_force)
                            {
                                _dumpLog.WriteLine(Localization.Core.
                                                                Current_position_is_not_as_expected_unable_to_resume_If_you_want_to_continue_use_force);

                                StoppingErrorMessage?.Invoke(Localization.Core.
                                                                          Current_position_is_not_as_expected_unable_to_resume_If_you_want_to_continue_use_force);

                                return;
                            }

                            _dumpLog.WriteLine(Localization.Core.
                                                            Current_position_is_not_as_expected_unable_to_resume_Dumping_from_the_start);

                            ErrorMessage?.Invoke(Localization.Core.
                                                              Current_position_is_not_as_expected_unable_to_resume_Dumping_from_the_start);

                            canLocateLong = false;
                        }
                    }
                }
                else
                {
                    if(!_force)
                    {
                        _dumpLog.WriteLine(Localization.Core.
                                                        Cannot_reposition_tape_unable_to_resume_If_you_want_to_continue_use_force);

                        StoppingErrorMessage?.Invoke(Localization.Core.
                                                                  Cannot_reposition_tape_unable_to_resume_If_you_want_to_continue_use_force);

                        return;
                    }

                    _dumpLog.WriteLine(Localization.Core.
                                                    Cannot_reposition_tape_unable_to_resume_Dumping_from_the_start);

                    ErrorMessage?.Invoke(Localization.Core.
                                                      Cannot_reposition_tape_unable_to_resume_Dumping_from_the_start);

                    canLocateLong = false;
                }
            }
            else if(canLocate)
            {
                sense = _dev.Locate(out senseBuf, (uint)_resume.NextBlock, _dev.Timeout, out _);

                if(!sense)
                {
                    sense = _dev.ReadPosition(out cmdBuf, out senseBuf, _dev.Timeout, out _);

                    if(sense)
                    {
                        if(!_force)
                        {
                            _dumpLog.WriteLine(Localization.Core.
                                                            Could_not_check_current_position_unable_to_resume_If_you_want_to_continue_use_force);

                            StoppingErrorMessage?.Invoke(Localization.Core.
                                                                      Could_not_check_current_position_unable_to_resume_If_you_want_to_continue_use_force);

                            return;
                        }

                        _dumpLog.WriteLine(Localization.Core.
                                                        Could_not_check_current_position_unable_to_resume_Dumping_from_the_start);

                        ErrorMessage?.Invoke(Localization.Core.
                                                          Could_not_check_current_position_unable_to_resume_Dumping_from_the_start);

                        canLocate = false;
                    }
                    else
                    {
                        ulong position = Swapping.Swap(BitConverter.ToUInt32(cmdBuf, 4));

                        if(position != _resume.NextBlock)
                        {
                            if(!_force)
                            {
                                _dumpLog.WriteLine(Localization.Core.
                                                                Current_position_is_not_as_expected_unable_to_resume_If_you_want_to_continue_use_force);

                                StoppingErrorMessage?.Invoke(Localization.Core.
                                                                          Current_position_is_not_as_expected_unable_to_resume_If_you_want_to_continue_use_force);

                                return;
                            }

                            _dumpLog.WriteLine(Localization.Core.
                                                            Current_position_is_not_as_expected_unable_to_resume_Dumping_from_the_start);

                            ErrorMessage?.Invoke(Localization.Core.
                                                              Current_position_is_not_as_expected_unable_to_resume_Dumping_from_the_start);

                            canLocate = false;
                        }
                    }
                }
                else
                {
                    if(!_force)
                    {
                        _dumpLog.WriteLine(Localization.Core.
                                                        Cannot_reposition_tape_unable_to_resume_If_you_want_to_continue_use_force);

                        StoppingErrorMessage?.Invoke(Localization.Core.
                                                                  Cannot_reposition_tape_unable_to_resume_If_you_want_to_continue_use_force);

                        return;
                    }

                    _dumpLog.WriteLine(Localization.Core.
                                                    Cannot_reposition_tape_unable_to_resume_Dumping_from_the_start);

                    ErrorMessage?.Invoke(Localization.Core.
                                                      Cannot_reposition_tape_unable_to_resume_Dumping_from_the_start);

                    canLocate = false;
                }
            }
            else
            {
                if(!_force)
                {
                    _dumpLog.WriteLine(Localization.Core.
                                                    Cannot_reposition_tape_unable_to_resume_If_you_want_to_continue_use_force);

                    StoppingErrorMessage?.Invoke(Localization.Core.
                                                              Cannot_reposition_tape_unable_to_resume_If_you_want_to_continue_use_force);

                    return;
                }

                _dumpLog.WriteLine(Localization.Core.Cannot_reposition_tape_unable_to_resume_Dumping_from_the_start);
                ErrorMessage?.Invoke(Localization.Core.Cannot_reposition_tape_unable_to_resume_Dumping_from_the_start);
                canLocate = false;
            }
        }
        else
        {
            _ = canLocateLong
                    ? _dev.Locate16(out senseBuf, false, 0, 0, _dev.Timeout, out duration)
                    : _dev.Locate(out senseBuf, false, 0, 0, _dev.Timeout, out duration);

            do
            {
                Thread.Sleep(1000);
                PulseProgress?.Invoke(Localization.Core.Rewinding_please_wait);
                _dev.RequestSense(out senseBuf, _dev.Timeout, out duration);
                decSense = Sense.Decode(senseBuf);
            } while(decSense is { ASC: 0x00, ASCQ: 0x1A or 0x19 });

            // And yet, did not rewind!
            if(decSense.HasValue &&
               (decSense.Value.ASC == 0x00 && decSense.Value.ASCQ != 0x00 && decSense.Value.ASCQ != 0x04 ||
                decSense.Value.ASC != 0x00))
            {
                StoppingErrorMessage?.Invoke(Localization.Core.Drive_could_not_rewind_please_correct_Sense_follows +
                                             Environment.NewLine                                                   +
                                             decSense.Value.Description);

                _dumpLog.WriteLine(Localization.Core.Drive_could_not_rewind_please_correct_Sense_follows);

                _dumpLog.WriteLine(Localization.Core.Device_not_ready_Sense, decSense.Value.SenseKey,
                                   decSense.Value.ASC, decSense.Value.ASCQ);

                return;
            }
        }

        bool ret = outputTape.SetTape();

        // Cannot set image to tape mode
        if(!ret)
        {
            _dumpLog.WriteLine(Localization.Core.Error_setting_output_image_in_tape_mode_not_continuing);
            _dumpLog.WriteLine(outputTape.ErrorMessage);

            StoppingErrorMessage?.Invoke(Localization.Core.Error_setting_output_image_in_tape_mode_not_continuing +
                                         Environment.NewLine                                                      +
                                         outputTape.ErrorMessage);

            return;
        }

        ret = outputTape.Create(_outputPath, dskType, _formatOptions, 0, 0);

        // Cannot create image
        if(!ret)
        {
            _dumpLog.WriteLine(Localization.Core.Error_creating_output_image_not_continuing);
            _dumpLog.WriteLine(outputTape.ErrorMessage);

            StoppingErrorMessage?.Invoke(Localization.Core.Error_creating_output_image_not_continuing +
                                         Environment.NewLine                                          +
                                         outputTape.ErrorMessage);

            return;
        }

        _dumpStopwatch.Restart();
        var mhddLog = new MhddLog(_outputPrefix + ".mhddlog.bin", _dev, blocks, blockSize, 1, _private);
        var ibgLog  = new IbgLog(_outputPrefix  + ".ibg", 0x0008);

        var currentTapeFile = new TapeFile
        {
            File       = currentFile,
            FirstBlock = currentBlock,
            Partition  = currentPartition
        };

        var currentTapePartition = new TapePartition
        {
            Number     = currentPartition,
            FirstBlock = currentBlock
        };

        if((canLocate || canLocateLong) && _resume.NextBlock > 0)
        {
            currentBlock = _resume.NextBlock;

            currentTapeFile =
                outputTape.Files.FirstOrDefault(f => f.LastBlock == outputTape?.Files.Max(g => g.LastBlock));

            currentTapePartition =
                outputTape.TapePartitions.FirstOrDefault(p => p.LastBlock ==
                                                              outputTape?.TapePartitions.Max(g => g.LastBlock));
        }

        if(mode6Data != null)
            outputTape.WriteMediaTag(mode6Data, MediaTagType.SCSI_MODESENSE_6);

        if(mode10Data != null)
            outputTape.WriteMediaTag(mode10Data, MediaTagType.SCSI_MODESENSE_10);

        ulong  currentSpeedSize   = 0;
        double imageWriteDuration = 0;

        InitProgress?.Invoke();

        _speedStopwatch.Restart();

        while(currentPartition < totalPartitions)
        {
            if(_aborted)
            {
                currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                UpdateStatus?.Invoke(Localization.Core.Aborted);
                _dumpLog.WriteLine(Localization.Core.Aborted);

                break;
            }

            if(endOfMedia)
            {
                UpdateStatus?.Invoke(string.Format(Localization.Core.Finished_partition_0, currentPartition));
                _dumpLog.WriteLine(Localization.Core.Finished_partition_0, currentPartition);

                currentTapeFile.LastBlock = currentBlock - 1;

                if(currentTapeFile.LastBlock > currentTapeFile.FirstBlock)
                    outputTape.AddFile(currentTapeFile);

                currentTapePartition.LastBlock = currentBlock - 1;
                outputTape.AddPartition(currentTapePartition);

                currentPartition++;

                if(currentPartition < totalPartitions)
                {
                    currentFile++;

                    currentTapeFile = new TapeFile
                    {
                        File       = currentFile,
                        FirstBlock = currentBlock,
                        Partition  = currentPartition
                    };

                    currentTapePartition = new TapePartition
                    {
                        Number     = currentPartition,
                        FirstBlock = currentBlock
                    };

                    UpdateStatus?.Invoke(string.Format(Localization.Core.Seeking_to_partition_0, currentPartition));
                    _dev.Locate(out senseBuf, false, currentPartition, 0, _dev.Timeout, out duration);
                    totalDuration += duration;
                }

                continue;
            }

            if(currentSpeed > maxSpeed && currentSpeed > 0)
                maxSpeed = currentSpeed;

            if(currentSpeed < minSpeed && currentSpeed > 0)
                minSpeed = currentSpeed;

            PulseProgress?.Invoke(string.Format(Localization.Core.Reading_block_0_1, currentBlock,
                                                ByteSize.FromBytes(currentSpeed).Per(_oneSecond).Humanize()));

            sense = _dev.Read6(out cmdBuf, out senseBuf, false, fixedLen, transferLen, blockSize, _dev.Timeout,
                               out duration);

            totalDuration += duration;

            if(sense && senseBuf?.Length != 0 && !ArrayHelpers.ArrayIsNullOrEmpty(senseBuf))
            {
                decSense = Sense.Decode(senseBuf);

                bool ili         = decSense?.Fixed?.ILI              == true;
                bool valid       = decSense?.Fixed?.InformationValid == true;
                uint information = decSense?.Fixed?.Information ?? 0;
                bool eom         = decSense?.Fixed?.EOM      == true;
                bool filemark    = decSense?.Fixed?.Filemark == true;

                if(decSense?.Descriptor.HasValue == true)
                {
                    if(decSense.Value.Descriptor.Value.Descriptors.TryGetValue(0, out byte[] desc00))
                    {
                        valid       = true;
                        information = (uint)Sense.DecodeDescriptor00(desc00);
                    }

                    if(decSense.Value.Descriptor.Value.Descriptors.TryGetValue(4, out byte[] desc04))
                        Sense.DecodeDescriptor04(desc04, out filemark, out eom, out ili);
                }

                if(decSense.Value is { ASC: 0x00, ASCQ: 0x00 } && ili && valid)
                {
                    blockSize = (uint)((int)blockSize - BitConverter.ToInt32(BitConverter.GetBytes(information), 0));

                    if(!fixedLen)
                        transferLen = blockSize;

                    UpdateStatus?.Invoke(string.Format(Localization.Core.Blocksize_changed_to_0_bytes_at_block_1,
                                                       blockSize, currentBlock));

                    _dumpLog.WriteLine(Localization.Core.Blocksize_changed_to_0_bytes_at_block_1, blockSize,
                                       currentBlock);

                    sense = _dev.Space(out senseBuf, SscSpaceCodes.LogicalBlock, -1, _dev.Timeout, out duration);

                    totalDuration += duration;

                    if(sense)
                    {
                        decSense = Sense.Decode(senseBuf);

                        StoppingErrorMessage?.Invoke(Localization.Core.Drive_could_not_go_back_one_block_Sense_follows +
                                                     Environment.NewLine                                               +
                                                     decSense.Value.Description);

                        outputTape.Close();
                        _dumpLog.WriteLine(Localization.Core.Drive_could_not_go_back_one_block_Sense_follows);

                        _dumpLog.WriteLine(Localization.Core.Device_not_ready_Sense, decSense.Value.SenseKey,
                                           decSense.Value.ASC, decSense.Value.ASCQ);

                        return;
                    }

                    continue;
                }

                switch(decSense.Value.SenseKey)
                {
                    case SenseKeys.BlankCheck when currentBlock == 0:
                        StoppingErrorMessage?.Invoke(Localization.Core.Cannot_dump_a_blank_tape);
                        outputTape.Close();
                        _dumpLog.WriteLine(Localization.Core.Cannot_dump_a_blank_tape);

                        return;

                    // For sure this is an end-of-tape/partition
                    case SenseKeys.BlankCheck when decSense.Value.ASC == 0x00 &&
                                                   (decSense.Value.ASCQ is 0x02 or 0x05 || eom):
                        // TODO: Detect end of partition
                        endOfMedia = true;
                        UpdateStatus?.Invoke(Localization.Core.Found_end_of_tape_partition);
                        _dumpLog.WriteLine(Localization.Core.Found_end_of_tape_partition);

                        continue;
                    case SenseKeys.BlankCheck:
                        StoppingErrorMessage?.Invoke(Localization.Core.Blank_block_found_end_of_tape);
                        endOfMedia = true;
                        _dumpLog.WriteLine(Localization.Core.Blank_block_found_end_of_tape);

                        continue;
                }

                switch(decSense.Value.SenseKey)
                {
                    case SenseKeys.NoSense or SenseKeys.RecoveredError when decSense.Value.ASCQ is 0x02 or 0x05 || eom:
                        // TODO: Detect end of partition
                        endOfMedia = true;
                        UpdateStatus?.Invoke(Localization.Core.Found_end_of_tape_partition);
                        _dumpLog.WriteLine(Localization.Core.Found_end_of_tape_partition);

                        continue;
                    case SenseKeys.NoSense or SenseKeys.RecoveredError when decSense.Value.ASCQ == 0x01 || filemark:
                        currentTapeFile.LastBlock = currentBlock - 1;
                        outputTape.AddFile(currentTapeFile);

                        currentFile++;

                        currentTapeFile = new TapeFile
                        {
                            File       = currentFile,
                            FirstBlock = currentBlock,
                            Partition  = currentPartition
                        };

                        UpdateStatus?.Invoke(string.Format(Localization.Core.Changed_to_file_0_at_block_1, currentFile,
                                                           currentBlock));

                        _dumpLog.WriteLine(Localization.Core.Changed_to_file_0_at_block_1, currentFile, currentBlock);

                        continue;
                }

                if(decSense is null)
                {
                    StoppingErrorMessage?.
                        Invoke(string.Format(Localization.Core.Drive_could_not_read_block_0_Sense_cannot_be_decoded_look_at_log_for_dump,
                                             currentBlock));

                    _dumpLog.WriteLine(string.Format(Localization.Core.Drive_could_not_read_block_0_Sense_bytes_follow,
                                                     currentBlock));

                    _dumpLog.WriteLine(PrintHex.ByteArrayToHexArrayString(senseBuf, 32));
                }
                else
                {
                    StoppingErrorMessage?.
                        Invoke(string.Format(Localization.Core.Drive_could_not_read_block_0_Sense_follow_1_2,
                                             currentBlock, decSense.Value.SenseKey, decSense.Value.Description));

                    _dumpLog.WriteLine(string.Format(Localization.Core.Drive_could_not_read_block_0_Sense_follows,
                                                     currentBlock));

                    _dumpLog.WriteLine(Localization.Core.Device_not_ready_Sense, decSense.Value.SenseKey,
                                       decSense.Value.ASC, decSense.Value.ASCQ);
                }

                // TODO: Reset device after X errors
                if(_stopOnError)
                    return; // TODO: Return more cleanly

                // Write empty data
                _writeStopwatch.Restart();
                outputTape.WriteSector(new byte[blockSize], currentBlock);
                imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;

                mhddLog.Write(currentBlock, duration < 500 ? 65535 : duration);
                ibgLog.Write(currentBlock, 0);
                _resume.BadBlocks.Add(currentBlock);
            }
            else
            {
                mhddLog.Write(currentBlock, duration);
                ibgLog.Write(currentBlock, currentSpeed * 1024);
                _writeStopwatch.Restart();
                outputTape.WriteSector(cmdBuf, currentBlock);
                imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;
                extents.Add(currentBlock, 1, true);
            }

            _writeStopwatch.Stop();
            currentBlock++;
            _resume.NextBlock++;
            currentSpeedSize += blockSize;

            double elapsed = _speedStopwatch.Elapsed.TotalSeconds;

            if(elapsed <= 0)
                continue;

            currentSpeed     = currentSpeedSize / (1048576 * elapsed);
            currentSpeedSize = 0;
            _speedStopwatch.Restart();
        }

        _resume.BadBlocks = _resume.BadBlocks.Distinct().ToList();
        blocks            = currentBlock + 1;
        _speedStopwatch.Stop();
        _dumpStopwatch.Stop();

        // If not aborted this is added at the end of medium
        if(_aborted)
        {
            currentTapeFile.LastBlock = currentBlock - 1;
            outputTape.AddFile(currentTapeFile);

            currentTapePartition.LastBlock = currentBlock - 1;
            outputTape.AddPartition(currentTapePartition);
        }

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

    #region Error handling

        if(_resume.BadBlocks.Count > 0 && !_aborted && _retryPasses > 0 && (canLocate || canLocateLong))
        {
            var        pass              = 1;
            var        forward           = false;
            const bool runningPersistent = false;

            Modes.ModePage? currentModePage = null;

            if(_persistent)
            {
                // TODO: Implement persistent
            }

            InitProgress?.Invoke();
        repeatRetry:
            ulong[] tmpArray = _resume.BadBlocks.ToArray();

            foreach(ulong badBlock in tmpArray)
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
                                                         badBlock, pass)
                                              : string.Format(Localization.Core.Retrying_sector_0_pass_1_forward,
                                                              badBlock, pass));
                }
                else
                {
                    PulseProgress?.Invoke(runningPersistent
                                              ? string.
                                                  Format(Localization.Core.Retrying_sector_0_pass_1_recovering_partial_data_reverse,
                                                         badBlock, pass)
                                              : string.Format(Localization.Core.Retrying_sector_0_pass_1_reverse,
                                                              badBlock, pass));
                }

                UpdateStatus?.Invoke(string.Format(Localization.Core.Positioning_tape_to_block_0, badBlock));
                _dumpLog.WriteLine(string.Format(Localization.Core.Positioning_tape_to_block_0,   badBlock));

                if(canLocateLong)
                {
                    sense = _dev.Locate16(out senseBuf, _resume.NextBlock, _dev.Timeout, out _);

                    if(!sense)
                    {
                        sense = _dev.ReadPositionLong(out cmdBuf, out senseBuf, _dev.Timeout, out _);

                        if(sense)
                        {
                            _dumpLog.WriteLine(Localization.Core.Could_not_check_current_position_continuing);
                            StoppingErrorMessage?.Invoke(Localization.Core.Could_not_check_current_position_continuing);

                            continue;
                        }

                        ulong position = Swapping.Swap(BitConverter.ToUInt64(cmdBuf, 8));

                        if(position != _resume.NextBlock)
                        {
                            _dumpLog.WriteLine(Localization.Core.Current_position_is_not_as_expected_continuing);

                            StoppingErrorMessage?.Invoke(Localization.Core.
                                                                      Current_position_is_not_as_expected_continuing);

                            continue;
                        }
                    }
                    else
                    {
                        _dumpLog.WriteLine(string.Format(Localization.Core.Cannot_position_tape_to_block_0, badBlock));

                        ErrorMessage?.Invoke(string.Format(Localization.Core.Cannot_position_tape_to_block_0,
                                                           badBlock));

                        continue;
                    }
                }
                else
                {
                    sense = _dev.Locate(out senseBuf, (uint)_resume.NextBlock, _dev.Timeout, out _);

                    if(!sense)
                    {
                        sense = _dev.ReadPosition(out cmdBuf, out senseBuf, _dev.Timeout, out _);

                        if(sense)
                        {
                            _dumpLog.WriteLine(Localization.Core.Could_not_check_current_position_continuing);
                            StoppingErrorMessage?.Invoke(Localization.Core.Could_not_check_current_position_continuing);

                            continue;
                        }

                        ulong position = Swapping.Swap(BitConverter.ToUInt32(cmdBuf, 4));

                        if(position != _resume.NextBlock)
                        {
                            _dumpLog.WriteLine(Localization.Core.Current_position_is_not_as_expected_continuing);

                            StoppingErrorMessage?.Invoke(Localization.Core.
                                                                      Current_position_is_not_as_expected_continuing);

                            continue;
                        }
                    }
                    else
                    {
                        _dumpLog.WriteLine(string.Format(Localization.Core.Cannot_position_tape_to_block_0, badBlock));

                        ErrorMessage?.Invoke(string.Format(Localization.Core.Cannot_position_tape_to_block_0,
                                                           badBlock));

                        continue;
                    }
                }

                sense = _dev.Read6(out cmdBuf, out senseBuf, false, fixedLen, transferLen, blockSize, _dev.Timeout,
                                   out duration);

                totalDuration += duration;

                if(!sense && !_dev.Error)
                {
                    _resume.BadBlocks.Remove(badBlock);
                    extents.Add(badBlock);
                    outputTape.WriteSector(cmdBuf, badBlock);

                    UpdateStatus?.Invoke(string.Format(Localization.Core.Correctly_retried_block_0_in_pass_1, badBlock,
                                                       pass));

                    _dumpLog.WriteLine(Localization.Core.Correctly_retried_block_0_in_pass_1, badBlock, pass);
                }
                else if(runningPersistent)
                    outputTape.WriteSector(cmdBuf, badBlock);
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
                // TODO: Persistent mode
            }

            EndProgress?.Invoke();
        }

    #endregion Error handling

        _resume.BadBlocks.Sort();

        foreach(ulong bad in _resume.BadBlocks)
            _dumpLog.WriteLine(Localization.Core.Block_0_could_not_be_read, bad);

        currentTry.Extents = ExtentsConverter.ToMetadata(extents);

        outputTape.SetDumpHardware(_resume.Tries);

        // TODO: Media Serial Number
        var metadata = new CommonTypes.Structs.ImageInfo
        {
            Application        = "Aaru",
            ApplicationVersion = Version.GetVersion()
        };

        if(!outputTape.SetImageInfo(metadata))
        {
            ErrorMessage?.Invoke(Localization.Core.Error_0_setting_metadata +
                                 Environment.NewLine                        +
                                 outputTape.ErrorMessage);
        }

        if(_preSidecar != null)
            outputTape.SetMetadata(_preSidecar);

        _dumpLog.WriteLine(Localization.Core.Closing_output_file);
        UpdateStatus?.Invoke(Localization.Core.Closing_output_file);
        _imageCloseStopwatch.Restart();
        outputTape.Close();
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
            var         filters     = new FiltersList();
            IFilter     filter      = filters.GetFilter(_outputPath);
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
                totalChkDuration = _sidecarStopwatch.ElapsedMilliseconds;

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

                // TODO: Implement device firmware revision
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

                sidecar.BlockMedias[0].LogicalBlocks = blocks;
                sidecar.BlockMedias[0].Manufacturer  = _dev.Manufacturer;
                sidecar.BlockMedias[0].Model         = _dev.Model;

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
                                 _sidecarStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second),
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