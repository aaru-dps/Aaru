// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : XGD.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps Xbox Game Discs.
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
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Interop;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Console;
using Aaru.Core.Graphics;
using Aaru.Core.Logging;
using Aaru.Decoders.DVD;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.Xbox;
using Aaru.Devices;
using Humanizer;
using Humanizer.Bytes;
using Humanizer.Localisation;
using Device = Aaru.Devices.Remote.Device;
using Layers = Aaru.CommonTypes.AaruMetadata.Layers;
using PlatformID = Aaru.CommonTypes.Interop.PlatformID;
using Track = Aaru.CommonTypes.Structs.Track;
using TrackType = Aaru.CommonTypes.Enums.TrackType;
using Version = Aaru.CommonTypes.Interop.Version;

namespace Aaru.Core.Devices.Dumping;

/// <summary>Implements dumping an Xbox Game Disc using a Kreon drive</summary>
partial class Dump
{
    /// <summary>Dumps an Xbox Game Disc using a Kreon drive</summary>
    /// <param name="mediaTags">Media tags as retrieved in MMC layer</param>
    /// <param name="dskType">Disc type as detected in MMC layer</param>
    void Xgd(Dictionary<MediaTagType, byte[]> mediaTags, MediaType dskType)
    {
        bool       sense;
        const uint blockSize    = 2048;
        uint       blocksToRead = 64;
        double     totalDuration = 0;
        double     currentSpeed  = 0;
        double     maxSpeed      = double.MinValue;
        double     minSpeed      = double.MaxValue;

        if(_outputPlugin is not IWritableImage outputFormat)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Image_is_not_writable_aborting);

            return;
        }

        if(DetectOS.GetRealPlatformID() != PlatformID.Win32NT)
        {
            bool isAdmin = _dev is Device remoteDev ? remoteDev.IsAdmin : DetectOS.IsAdmin;

            if(!isAdmin)
            {
                AaruConsole.ErrorWriteLine(Localization.Core.
                                                        Because_of_commands_sent_dumping_XGD_must_be_administrative_Cannot_continue);

                _dumpLog.WriteLine(Localization.Core.Cannot_dump_XGD_without_administrative_privileges);

                return;
            }
        }

        if(mediaTags.ContainsKey(MediaTagType.DVD_PFI))
            mediaTags.Remove(MediaTagType.DVD_PFI);

        if(mediaTags.ContainsKey(MediaTagType.DVD_DMI))
            mediaTags.Remove(MediaTagType.DVD_DMI);

        // Drive shall move to lock state when a new disc is inserted. Old kreon versions do not lock correctly so save this
        sense = _dev.ReadCapacity(out byte[] coldReadCapacity, out byte[] senseBuf, _dev.Timeout, out _);

        if(sense)
        {
            _dumpLog.WriteLine(Localization.Core.Cannot_get_disc_capacity);
            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_get_disc_capacity);

            return;
        }

        // Drive shall move to lock state when a new disc is inserted. Old kreon versions do not lock correctly so save this
        sense = _dev.ReadDiscStructure(out byte[] coldPfi, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                       MmcDiscStructureFormat.PhysicalInformation, 0, 0, out _);

        if(sense)
        {
            _dumpLog.WriteLine(Localization.Core.Cannot_get_PFI);
            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_get_PFI);

            return;
        }

        UpdateStatus?.Invoke(Localization.Core.Reading_Xbox_Security_Sector);
        _dumpLog.WriteLine(Localization.Core.Reading_Xbox_Security_Sector);
        sense = _dev.KreonExtractSs(out byte[] ssBuf, out senseBuf, _dev.Timeout, out _);

        if(sense)
        {
            _dumpLog.WriteLine(Localization.Core.Cannot_get_Xbox_Security_Sector_not_continuing);
            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_get_Xbox_Security_Sector_not_continuing);

            return;
        }

        _dumpLog.WriteLine(Localization.Core.Decoding_Xbox_Security_Sector);
        UpdateStatus?.Invoke(Localization.Core.Decoding_Xbox_Security_Sector);
        SS.SecuritySector? xboxSs = SS.Decode(ssBuf);

        if(!xboxSs.HasValue)
        {
            _dumpLog.WriteLine(Localization.Core.Cannot_decode_Xbox_Security_Sector_not_continuing);
            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_decode_Xbox_Security_Sector_not_continuing);

            return;
        }

        byte[] tmpBuf = new byte[ssBuf.Length - 4];
        Array.Copy(ssBuf, 4, tmpBuf, 0, ssBuf.Length - 4);
        mediaTags.Add(MediaTagType.Xbox_SecuritySector, tmpBuf);

        // Get video partition size
        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Getting_video_partition_size);
        UpdateStatus?.Invoke(Localization.Core.Locking_drive);
        _dumpLog.WriteLine(Localization.Core.Locking_drive);
        sense = _dev.KreonLock(out senseBuf, _dev.Timeout, out _);

        if(sense)
        {
            _errorLog?.WriteLine("Kreon lock", _dev.Error, _dev.LastError, senseBuf);

            _dumpLog.WriteLine(Localization.Core.Cannot_lock_drive_not_continuing);
            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_lock_drive_not_continuing);

            return;
        }

        UpdateStatus?.Invoke(Localization.Core.Getting_video_partition_size);
        _dumpLog.WriteLine(Localization.Core.Getting_video_partition_size);
        sense = _dev.ReadCapacity(out byte[] readBuffer, out senseBuf, _dev.Timeout, out _);

        if(sense)
        {
            _dumpLog.WriteLine(Localization.Core.Cannot_get_disc_capacity);
            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_get_disc_capacity);

            return;
        }

        ulong totalSize =
            (ulong)((readBuffer[0] << 24) + (readBuffer[1] << 16) + (readBuffer[2] << 8) + readBuffer[3]) & 0xFFFFFFFF;

        UpdateStatus?.Invoke(Localization.Core.Reading_Physical_Format_Information);
        _dumpLog.WriteLine(Localization.Core.Reading_Physical_Format_Information);

        sense = _dev.ReadDiscStructure(out readBuffer, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                       MmcDiscStructureFormat.PhysicalInformation, 0, 0, out _);

        if(sense)
        {
            _dumpLog.WriteLine(Localization.Core.Cannot_get_PFI);
            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_get_PFI);

            return;
        }

        tmpBuf = new byte[readBuffer.Length - 4];
        Array.Copy(readBuffer, 4, tmpBuf, 0, readBuffer.Length - 4);
        mediaTags.Add(MediaTagType.DVD_PFI, tmpBuf);

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Video_partition_total_size_0_sectors,
                                   totalSize);

        ulong l0Video = (PFI.Decode(readBuffer, MediaType.DVDROM)?.Layer0EndPSN     ?? 0) -
                        (PFI.Decode(readBuffer, MediaType.DVDROM)?.DataAreaStartPSN ?? 0) + 1;

        ulong l1Video = totalSize - l0Video + 1;
        UpdateStatus?.Invoke(Localization.Core.Reading_Disc_Manufacturing_Information);
        _dumpLog.WriteLine(Localization.Core.Reading_Disc_Manufacturing_Information);

        sense = _dev.ReadDiscStructure(out readBuffer, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                       MmcDiscStructureFormat.DiscManufacturingInformation, 0, 0, out _);

        if(sense)
        {
            _dumpLog.WriteLine(Localization.Core.Cannot_get_DMI);
            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_get_DMI);

            return;
        }

        tmpBuf = new byte[readBuffer.Length - 4];
        Array.Copy(readBuffer, 4, tmpBuf, 0, readBuffer.Length - 4);
        mediaTags.Add(MediaTagType.DVD_DMI, tmpBuf);

        // Should be a safe value to detect the lock command was ignored, and we're indeed getting the whole size and not the locked one
        if(totalSize > 300000)
        {
            UpdateStatus?.Invoke(Localization.Core.Video_partition_is_too_big_did_lock_work_Trying_cold_values);
            _dumpLog.WriteLine(Localization.Core.Video_partition_is_too_big_did_lock_work_Trying_cold_values);

            totalSize = (ulong)((coldReadCapacity[0] << 24) + (coldReadCapacity[1] << 16) + (coldReadCapacity[2] << 8) +
                                coldReadCapacity[3]) & 0xFFFFFFFF;

            tmpBuf = new byte[coldPfi.Length - 4];
            Array.Copy(coldPfi, 4, tmpBuf, 0, coldPfi.Length - 4);
            mediaTags.Remove(MediaTagType.DVD_PFI);
            mediaTags.Add(MediaTagType.DVD_PFI, tmpBuf);

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Video_partition_total_size_0_sectors,
                                       totalSize);

            l0Video = (PFI.Decode(coldPfi, MediaType.DVDROM)?.Layer0EndPSN     ?? 0) -
                      (PFI.Decode(coldPfi, MediaType.DVDROM)?.DataAreaStartPSN ?? 0) + 1;

            l1Video = totalSize - l0Video + 1;

            if(totalSize > 300000)
            {
                _dumpLog.WriteLine(Localization.Core.Cannot_get_video_partition_size_not_continuing);

                StoppingErrorMessage?.Invoke(Localization.Core.Cannot_get_video_partition_size_not_continuing);

                return;
            }
        }

        // Get game partition size
        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Getting_game_partition_size);
        UpdateStatus?.Invoke(Localization.Core.Unlocking_drive_Xtreme_);
        _dumpLog.WriteLine(Localization.Core.Unlocking_drive_Xtreme_);
        sense = _dev.KreonUnlockXtreme(out senseBuf, _dev.Timeout, out _);

        if(sense)
        {
            _errorLog?.WriteLine("Kreon Xtreme unlock", _dev.Error, _dev.LastError, senseBuf);
            _dumpLog.WriteLine(Localization.Core.Cannot_unlock_drive_not_continuing);
            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_unlock_drive_not_continuing);

            return;
        }

        UpdateStatus?.Invoke(Localization.Core.Getting_game_partition_size);
        _dumpLog.WriteLine(Localization.Core.Getting_game_partition_size);
        sense = _dev.ReadCapacity(out readBuffer, out senseBuf, _dev.Timeout, out _);

        if(sense)
        {
            _dumpLog.WriteLine(Localization.Core.Cannot_get_disc_capacity);
            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_get_disc_capacity);

            return;
        }

        ulong gameSize =
            ((ulong)((readBuffer[0] << 24) + (readBuffer[1] << 16) + (readBuffer[2] << 8) + readBuffer[3]) &
             0xFFFFFFFF) + 1;

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Game_partition_total_size_0_sectors,
                                   gameSize);

        // Get middle zone size
        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Getting_middle_zone_size);
        UpdateStatus?.Invoke(Localization.Core.Unlocking_drive_Wxripper);
        _dumpLog.WriteLine(Localization.Core.Unlocking_drive_Wxripper);
        sense = _dev.KreonUnlockWxripper(out senseBuf, _dev.Timeout, out _);

        if(sense)
        {
            _errorLog?.WriteLine("Kreon Wxripper unlock", _dev.Error, _dev.LastError, senseBuf);
            _dumpLog.WriteLine(Localization.Core.Cannot_unlock_drive_not_continuing);
            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_unlock_drive_not_continuing);

            return;
        }

        UpdateStatus?.Invoke(Localization.Core.Getting_disc_size);
        _dumpLog.WriteLine(Localization.Core.Getting_disc_size);
        sense = _dev.ReadCapacity(out readBuffer, out senseBuf, _dev.Timeout, out _);

        if(sense)
        {
            _dumpLog.WriteLine(Localization.Core.Cannot_get_disc_capacity);
            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_get_disc_capacity);

            return;
        }

        totalSize = (ulong)((readBuffer[0] << 24) + (readBuffer[1] << 16) + (readBuffer[2] << 8) + readBuffer[3]) &
                    0xFFFFFFFF;

        UpdateStatus?.Invoke(Localization.Core.Reading_Physical_Format_Information);
        _dumpLog.WriteLine(Localization.Core.Reading_Physical_Format_Information);

        sense = _dev.ReadDiscStructure(out readBuffer, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                       MmcDiscStructureFormat.PhysicalInformation, 0, 0, out _);

        if(sense)
        {
            _dumpLog.WriteLine(Localization.Core.Cannot_get_PFI);
            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_get_PFI);

            return;
        }

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Unlocked_total_size_0_sectors, totalSize);
        ulong blocks = totalSize + 1;

        PFI.PhysicalFormatInformation? wxRipperPfiNullable = PFI.Decode(readBuffer, MediaType.DVDROM);

        if(wxRipperPfiNullable == null)
        {
            _dumpLog.WriteLine(Localization.Core.Cannot_decode_PFI);
            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_decode_PFI);

            return;
        }

        PFI.PhysicalFormatInformation wxRipperPfi = wxRipperPfiNullable.Value;

        UpdateStatus?.Invoke(string.Format(Localization.Core.WxRipper_PFI_Data_Area_Start_PSN_0_sectors,
                                           wxRipperPfi.DataAreaStartPSN));

        UpdateStatus?.Invoke(string.Format(Localization.Core.WxRipper_PFI_Layer_0_End_PSN_0_sectors,
                                           wxRipperPfi.Layer0EndPSN));

        _dumpLog.WriteLine(string.Format(Localization.Core.WxRipper_PFI_Data_Area_Start_PSN_0_sectors,
                                         wxRipperPfi.DataAreaStartPSN));

        _dumpLog.WriteLine(string.Format(Localization.Core.WxRipper_PFI_Layer_0_End_PSN_0_sectors,
                                         wxRipperPfi.Layer0EndPSN));

        ulong middleZone = totalSize - (wxRipperPfi.Layer0EndPSN - wxRipperPfi.DataAreaStartPSN + 1) - gameSize + 1;

        tmpBuf = new byte[readBuffer.Length - 4];
        Array.Copy(readBuffer, 4, tmpBuf, 0, readBuffer.Length - 4);
        mediaTags.Add(MediaTagType.Xbox_PFI, tmpBuf);

        UpdateStatus?.Invoke(Localization.Core.Reading_Disc_Manufacturing_Information);
        _dumpLog.WriteLine(Localization.Core.Reading_Disc_Manufacturing_Information);

        sense = _dev.ReadDiscStructure(out readBuffer, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                       MmcDiscStructureFormat.DiscManufacturingInformation, 0, 0, out _);

        if(sense)
        {
            _dumpLog.WriteLine(Localization.Core.Cannot_get_DMI);
            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_get_DMI);

            return;
        }

        tmpBuf = new byte[readBuffer.Length - 4];
        Array.Copy(readBuffer, 4, tmpBuf, 0, readBuffer.Length - 4);
        mediaTags.Add(MediaTagType.Xbox_DMI, tmpBuf);

        totalSize = l0Video + l1Video + (middleZone * 2) + gameSize;
        ulong layerBreak = l0Video + middleZone + (gameSize / 2);

        UpdateStatus?.Invoke(string.Format(Localization.Core.Video_layer_0_size_0_sectors, l0Video));
        UpdateStatus?.Invoke(string.Format(Localization.Core.Video_layer_1_size_0_sectors, l1Video));
        UpdateStatus?.Invoke(string.Format(Localization.Core.Middle_zone_size_0_sectors, middleZone));
        UpdateStatus?.Invoke(string.Format(Localization.Core.Game_data_size_0_sectors, gameSize));
        UpdateStatus?.Invoke(string.Format(Localization.Core.Total_size_0_sectors, totalSize));
        UpdateStatus?.Invoke(string.Format(Localization.Core.Real_layer_break_0, layerBreak));
        UpdateStatus?.Invoke("");

        _dumpLog.WriteLine(Localization.Core.Video_layer_0_size_0_sectors, l0Video);
        _dumpLog.WriteLine(Localization.Core.Video_layer_1_size_0_sectors, l1Video);
        _dumpLog.WriteLine(Localization.Core.Middle_zone_size_0_sectors, middleZone);
        _dumpLog.WriteLine(Localization.Core.Game_data_size_0_sectors, gameSize);
        _dumpLog.WriteLine(Localization.Core.Total_size_0_sectors, totalSize);
        _dumpLog.WriteLine(Localization.Core.Real_layer_break_0, layerBreak);

        bool read12 = !_dev.Read12(out readBuffer, out senseBuf, 0, false, true, false, false, 0, blockSize, 0, 1,
                                   false, _dev.Timeout, out _);

        if(!read12)
        {
            _dumpLog.WriteLine(Localization.Core.Cannot_read_medium_aborting_scan);
            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_read_medium_aborting_scan);

            return;
        }

        _dumpLog.WriteLine(Localization.Core.Using_SCSI_READ_12_command);
        UpdateStatus?.Invoke(Localization.Core.Using_SCSI_READ_12_command);

        // Set speed
        if(_speedMultiplier >= 0)
        {
            if(_speed == 0)
            {
                _dumpLog.WriteLine(Localization.Core.Setting_speed_to_MAX);
                UpdateStatus?.Invoke(Localization.Core.Setting_speed_to_MAX);
            }
            else
            {
                _dumpLog.WriteLine(string.Format(Localization.Core.Setting_speed_to_0_x, _speed));
                UpdateStatus?.Invoke(string.Format(Localization.Core.Setting_speed_to_0_x, _speed));
            }

            _speed *= _speedMultiplier;

            if(_speed is 0 or > 0xFFFF)
                _speed = 0xFFFF;

            _dev.SetCdSpeed(out _, RotationalControl.ClvAndImpureCav, (ushort)_speed, 0, _dev.Timeout, out _);
        }

        while(true)
        {
            sense = _dev.Read12(out readBuffer, out senseBuf, 0, false, false, false, false, 0, blockSize, 0,
                                blocksToRead, false, _dev.Timeout, out _);

            if(sense || _dev.Error)
                blocksToRead /= 2;

            if(!_dev.Error ||
               blocksToRead == 1)
                break;
        }

        if(_dev.Error)
        {
            _dumpLog.WriteLine(Localization.Core.Device_error_0_trying_to_guess_ideal_transfer_length, _dev.LastError);

            StoppingErrorMessage?.
                Invoke(string.Format(Localization.Core.Device_error_0_trying_to_guess_ideal_transfer_length,
                                     _dev.LastError));

            return;
        }

        if(_skip < blocksToRead)
            _skip = blocksToRead;

        bool ret = true;

        foreach(MediaTagType tag in mediaTags.Keys.Where(tag => !outputFormat.SupportedMediaTags.Contains(tag)))
        {
            ret = false;
            _dumpLog.WriteLine(string.Format(Localization.Core.Output_format_does_not_support_0, tag));
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

        _dumpLog.WriteLine(Localization.Core.Reading_0_sectors_at_a_time, blocksToRead);
        UpdateStatus?.Invoke(string.Format(Localization.Core.Reading_0_sectors_at_a_time, blocksToRead));

        var mhddLog = new MhddLog(_outputPrefix + ".mhddlog.bin", _dev, blocks, blockSize, blocksToRead, _private,
                                  _dimensions);

        var ibgLog = new IbgLog(_outputPrefix + ".ibg", 0x0010);
        ret = outputFormat.Create(_outputPath, dskType, _formatOptions, blocks, blockSize);

        // Cannot create image
        if(!ret)
        {
            _dumpLog.WriteLine(Localization.Core.Error_creating_output_image_not_continuing);
            _dumpLog.WriteLine(outputFormat.ErrorMessage);

            StoppingErrorMessage?.Invoke(Localization.Core.Error_creating_output_image_not_continuing +
                                         Environment.NewLine + outputFormat.ErrorMessage);

            return;
        }

        _dumpStopwatch.Restart();
        double imageWriteDuration = 0;

        double       cmdDuration      = 0;
        uint         saveBlocksToRead = blocksToRead;
        DumpHardware currentTry       = null;
        ExtentsULong extents          = null;

        ResumeSupport.Process(true, true, totalSize, _dev.Manufacturer, _dev.Model, _dev.Serial, _dev.PlatformId,
                              ref _resume, ref currentTry, ref extents, _dev.FirmwareRevision, _private, _force);

        if(currentTry == null ||
           extents    == null)
            StoppingErrorMessage?.Invoke(Localization.Core.Could_not_process_resume_file_not_continuing);

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

        (outputFormat as IWritableOpticalImage).SetTracks(new List<Track>
        {
            new()
            {
                BytesPerSector    = (int)blockSize,
                EndSector         = blocks - 1,
                Sequence          = 1,
                RawBytesPerSector = (int)blockSize,
                SubchannelType    = TrackSubchannelType.None,
                Session           = 1,
                Type              = TrackType.Data
            }
        });

        ulong currentSector = _resume.NextBlock;

        if(_resume.NextBlock > 0)
        {
            UpdateStatus?.Invoke(string.Format(Localization.Core.Resuming_from_block_0, _resume.NextBlock));
            _dumpLog.WriteLine(Localization.Core.Resuming_from_block_0, _resume.NextBlock);
        }

        bool newTrim = false;

        _dumpLog.WriteLine(Localization.Core.Reading_game_partition);
        UpdateStatus?.Invoke(Localization.Core.Reading_game_partition);
        _speedStopwatch.Restart();
        ulong    sectorSpeedStart = 0;
        InitProgress?.Invoke();

        for(int e = 0; e <= 16; e++)
        {
            if(_aborted)
            {
                _resume.NextBlock  = currentSector;
                currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                UpdateStatus?.Invoke(Localization.Core.Aborted);
                _dumpLog.WriteLine(Localization.Core.Aborted);

                break;
            }

            if(currentSector >= blocks)
                break;

            ulong extentStart, extentEnd;

            // Extents
            if(e < 16)
            {
                if(xboxSs.Value.Extents[e].StartPSN <= xboxSs.Value.Layer0EndPSN)
                    extentStart = xboxSs.Value.Extents[e].StartPSN - 0x30000;
                else
                    extentStart = ((xboxSs.Value.Layer0EndPSN + 1) * 2)               -
                                  ((xboxSs.Value.Extents[e].StartPSN ^ 0xFFFFFF) + 1) - 0x30000;

                if(xboxSs.Value.Extents[e].EndPSN <= xboxSs.Value.Layer0EndPSN)
                    extentEnd = xboxSs.Value.Extents[e].EndPSN - 0x30000;
                else
                    extentEnd = ((xboxSs.Value.Layer0EndPSN + 1) * 2)             -
                                ((xboxSs.Value.Extents[e].EndPSN ^ 0xFFFFFF) + 1) - 0x30000;
            }

            // After last extent
            else
            {
                extentStart = blocks;
                extentEnd   = blocks;
            }

            if(currentSector > extentEnd)
                continue;

            for(ulong i = currentSector; i < extentStart; i += blocksToRead)
            {
                saveBlocksToRead = blocksToRead;

                if(_aborted)
                {
                    currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                    UpdateStatus?.Invoke(Localization.Core.Aborted);
                    _dumpLog.WriteLine(Localization.Core.Aborted);

                    break;
                }

                if(extentStart - i < blocksToRead)
                    blocksToRead = (uint)(extentStart - i);

                if(currentSpeed > maxSpeed &&
                   currentSpeed > 0)
                    maxSpeed = currentSpeed;

                if(currentSpeed < minSpeed &&
                   currentSpeed > 0)
                    minSpeed = currentSpeed;

                UpdateProgress?.
                    Invoke(string.Format(Localization.Core.Reading_sector_0_of_1_2, i, blocks, ByteSize.FromMegabytes(currentSpeed).Per(_oneSecond).Humanize()),
                           (long)i, (long)totalSize);

                sense = _dev.Read12(out readBuffer, out senseBuf, 0, false, false, false, false, (uint)i, blockSize, 0,
                                    blocksToRead, false, _dev.Timeout, out cmdDuration);

                totalDuration += cmdDuration;

                if(!sense &&
                   !_dev.Error)
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
                    _errorLog?.WriteLine(i, _dev.Error, _dev.LastError, senseBuf);

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

                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_error_0,
                                               Sense.PrettifySense(senseBuf));

                    mhddLog.Write(i, cmdDuration < 500 ? 65535 : cmdDuration, _skip);

                    ibgLog.Write(i, 0);

                    _dumpLog.WriteLine(Localization.Core.Skipping_0_blocks_from_errored_block_1, _skip, i);
                    i += _skip - blocksToRead;

                    string[] senseLines = Sense.PrettifySense(senseBuf).Split(new[]
                    {
                        Environment.NewLine
                    }, StringSplitOptions.RemoveEmptyEntries);

                    foreach(string senseLine in senseLines)
                        _dumpLog.WriteLine(senseLine);

                    newTrim = true;
                }

                _writeStopwatch.Stop();
                blocksToRead      =  saveBlocksToRead;
                currentSector     =  i + 1;
                _resume.NextBlock =  currentSector;
                sectorSpeedStart  += blocksToRead;

                double elapsed = _speedStopwatch.Elapsed.TotalSeconds;

                if(elapsed <= 0)
                    continue;

                currentSpeed     = sectorSpeedStart * blockSize / (1048576 * elapsed);
                sectorSpeedStart = 0;
                _speedStopwatch.Restart();
            }

            _speedStopwatch.Stop();

            for(ulong i = extentStart; i <= extentEnd; i += blocksToRead)
            {
                saveBlocksToRead = blocksToRead;

                if(_aborted)
                {
                    currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                    UpdateStatus?.Invoke(Localization.Core.Aborted);
                    _dumpLog.WriteLine(Localization.Core.Aborted);

                    break;
                }

                if(extentEnd - i < blocksToRead)
                    blocksToRead = (uint)(extentEnd - i) + 1;

                mhddLog.Write(i, cmdDuration, blocksToRead);
                ibgLog.Write(i, currentSpeed * 1024);

                // Write empty data
                _writeStopwatch.Restart();
                outputFormat.WriteSectors(new byte[blockSize * blocksToRead], i, blocksToRead);
                imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;
                blocksToRead       =  saveBlocksToRead;
                extents.Add(i, blocksToRead, true);
                currentSector     = i + 1;
                _resume.NextBlock = currentSector;
                _mediaGraph?.PaintSectorsGood(i, blocksToRead);
            }

            if(!_aborted)
                currentSector = extentEnd + 1;
        }

        _writeStopwatch.Stop();
        _resume.BadBlocks = _resume.BadBlocks.Distinct().ToList();

        EndProgress?.Invoke();

        // Middle Zone D
        UpdateStatus?.Invoke(Localization.Core.Writing_Middle_Zone_D_empty);
        _dumpLog.WriteLine(Localization.Core.Writing_Middle_Zone_D_empty);
        InitProgress?.Invoke();

        for(ulong middle = currentSector - blocks - 1; middle < middleZone - 1; middle += blocksToRead)
        {
            if(_aborted)
            {
                currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                UpdateStatus?.Invoke(Localization.Core.Aborted);
                _dumpLog.WriteLine(Localization.Core.Aborted);

                break;
            }

            if(middleZone - 1 - middle < blocksToRead)
                blocksToRead = (uint)(middleZone - 1 - middle);

            UpdateProgress?.
                Invoke(string.Format(Localization.Core.Reading_sector_0_of_1_2, middle + currentSector, totalSize, ByteSize.FromMegabytes(currentSpeed).Per(_oneSecond).Humanize()),
                       (long)(middle + currentSector), (long)totalSize);

            mhddLog.Write(middle + currentSector, cmdDuration, blocksToRead);
            ibgLog.Write(middle  + currentSector, currentSpeed * 1024);

            // Write empty data
            _writeStopwatch.Restart();
            outputFormat.WriteSectors(new byte[blockSize * blocksToRead], middle + currentSector, blocksToRead);
            imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;
            extents.Add(currentSector, blocksToRead, true);
            _writeStopwatch.Stop();

            currentSector     += blocksToRead;
            _resume.NextBlock =  currentSector;
        }

        EndProgress?.Invoke();

        blocksToRead = saveBlocksToRead;

        UpdateStatus?.Invoke(Localization.Core.Locking_drive);
        _dumpLog.WriteLine(Localization.Core.Locking_drive);
        sense = _dev.KreonLock(out senseBuf, _dev.Timeout, out _);

        if(sense)
        {
            _dumpLog.WriteLine(Localization.Core.Cannot_lock_drive_not_continuing);
            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_lock_drive_not_continuing);

            return;
        }

        sense = _dev.ReadCapacity(out readBuffer, out senseBuf, _dev.Timeout, out _);

        if(sense)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_get_disc_capacity);

            return;
        }

        // Video Layer 1
        _dumpLog.WriteLine(Localization.Core.Reading_Video_Layer_1);
        UpdateStatus?.Invoke(Localization.Core.Reading_Video_Layer_1);
        InitProgress?.Invoke();

        for(ulong l1 = currentSector - blocks - middleZone + l0Video; l1 < l0Video + l1Video; l1 += blocksToRead)
        {
            if(_aborted)
            {
                currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                UpdateStatus?.Invoke(Localization.Core.Aborted);
                _dumpLog.WriteLine(Localization.Core.Aborted);

                break;
            }

            if(l0Video + l1Video - l1 < blocksToRead)
                blocksToRead = (uint)(l0Video + l1Video - l1);

            if(currentSpeed > maxSpeed &&
               currentSpeed > 0)
                maxSpeed = currentSpeed;

            if(currentSpeed < minSpeed &&
               currentSpeed > 0)
                minSpeed = currentSpeed;

            UpdateProgress?.
                Invoke(string.Format(Localization.Core.Reading_sector_0_of_1_2, currentSector, totalSize, ByteSize.FromMegabytes(currentSpeed).Per(_oneSecond).Humanize()),
                       (long)currentSector, (long)totalSize);

            sense = _dev.Read12(out readBuffer, out senseBuf, 0, false, false, false, false, (uint)l1, blockSize, 0,
                                blocksToRead, false, _dev.Timeout, out cmdDuration);

            totalDuration += cmdDuration;

            if(!sense &&
               !_dev.Error)
            {
                mhddLog.Write(currentSector, cmdDuration, blocksToRead);
                ibgLog.Write(currentSector, currentSpeed * 1024);
                _writeStopwatch.Restart();
                outputFormat.WriteSectors(readBuffer, currentSector, blocksToRead);
                imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;
                extents.Add(currentSector, blocksToRead, true);
                _mediaGraph?.PaintSectorsGood(currentSector, blocksToRead);
            }
            else
            {
                _errorLog?.WriteLine(currentSector, _dev.Error, _dev.LastError, senseBuf);

                // TODO: Reset device after X errors
                if(_stopOnError)
                    return; // TODO: Return more cleanly

                // Write empty data
                _writeStopwatch.Restart();
                outputFormat.WriteSectors(new byte[blockSize * _skip], currentSector, _skip);
                imageWriteDuration += _writeStopwatch.Elapsed.TotalSeconds;

                // TODO: Handle errors in video partition
                //errored += blocksToRead;
                //resume.BadBlocks.Add(l1);
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_error_0, Sense.PrettifySense(senseBuf));
                mhddLog.Write(l1, cmdDuration < 500 ? 65535 : cmdDuration, _skip);

                ibgLog.Write(l1, 0);
                _dumpLog.WriteLine(Localization.Core.Skipping_0_blocks_from_errored_block_1, _skip, l1);
                l1 += _skip - blocksToRead;

                string[] senseLines = Sense.PrettifySense(senseBuf).Split(new[]
                {
                    Environment.NewLine
                }, StringSplitOptions.RemoveEmptyEntries);

                foreach(string senseLine in senseLines)
                    _dumpLog.WriteLine(senseLine);
            }

            _writeStopwatch.Stop();

            currentSector     += blocksToRead;
            _resume.NextBlock =  currentSector;
            sectorSpeedStart  += blocksToRead;

            double elapsed = _speedStopwatch.Elapsed.TotalSeconds;

            if(elapsed <= 0)
                continue;

            currentSpeed     = sectorSpeedStart * blockSize / (1048576 * elapsed);
            sectorSpeedStart = 0;
            _speedStopwatch.Restart();
        }

        EndProgress?.Invoke();

        UpdateStatus?.Invoke(Localization.Core.Unlocking_drive_Wxripper);
        _dumpLog.WriteLine(Localization.Core.Unlocking_drive_Wxripper);
        sense = _dev.KreonUnlockWxripper(out senseBuf, _dev.Timeout, out _);

        if(sense)
        {
            _errorLog?.WriteLine("Kreon Wxripper unlock", _dev.Error, _dev.LastError, senseBuf);
            _dumpLog.WriteLine(Localization.Core.Cannot_unlock_drive_not_continuing);
            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_unlock_drive_not_continuing);

            return;
        }

        sense = _dev.ReadCapacity(out readBuffer, out senseBuf, _dev.Timeout, out _);

        if(sense)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_get_disc_capacity);

            return;
        }

        _dumpStopwatch.Stop();
        AaruConsole.WriteLine();
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
        if(_resume.BadBlocks.Count > 0 &&
           !_aborted                   &&
           _trim                       &&
           newTrim)
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
                    _dumpLog.WriteLine(Localization.Core.Aborted);

                    break;
                }

                PulseProgress?.Invoke(string.Format(Localization.Core.Trimming_sector_0, badSector));

                sense = _dev.Read12(out readBuffer, out senseBuf, 0, false, false, false, false, (uint)badSector,
                                    blockSize, 0, 1, false, _dev.Timeout, out cmdDuration);

                totalDuration += cmdDuration;

                if(sense || _dev.Error)
                {
                    _errorLog?.WriteLine(badSector, _dev.Error, _dev.LastError, senseBuf);

                    continue;
                }

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
        if(_resume.BadBlocks.Count > 0 &&
           !_aborted                   &&
           _retryPasses > 0)
        {
            List<ulong> tmpList = new();

            foreach(ulong ur in _resume.BadBlocks)
                for(ulong i = ur; i < ur + blocksToRead; i++)
                    tmpList.Add(i);

            tmpList.Sort();

            int  pass              = 1;
            bool forward           = true;
            bool runningPersistent = false;

            _resume.BadBlocks = tmpList;
            Modes.ModePage? currentModePage = null;
            byte[]          md6;
            byte[]          md10;

            if(_persistent)
            {
                Modes.ModePage_01_MMC pgMmc;

                sense = _dev.ModeSense6(out readBuffer, out _, false, ScsiModeSensePageControl.Current, 0x01,
                                        _dev.Timeout, out _);

                if(sense)
                {
                    sense = _dev.ModeSense10(out readBuffer, out _, false, ScsiModeSensePageControl.Current, 0x01,
                                             _dev.Timeout, out _);

                    if(!sense)
                    {
                        Modes.DecodedMode? dcMode10 =
                            Modes.DecodeMode10(readBuffer, PeripheralDeviceTypes.MultiMediaDevice);

                        if(dcMode10.HasValue)
                            foreach(Modes.ModePage modePage in dcMode10.Value.Pages.Where(modePage =>
                                        modePage is { Page: 0x01, Subpage: 0x00 }))
                                currentModePage = modePage;
                    }
                }
                else
                {
                    Modes.DecodedMode? dcMode6 = Modes.DecodeMode6(readBuffer, PeripheralDeviceTypes.MultiMediaDevice);

                    if(dcMode6.HasValue)
                        foreach(Modes.ModePage modePage in dcMode6.Value.Pages.Where(modePage =>
                                                                                   modePage is { Page: 0x01, Subpage: 0x00 }))
                            currentModePage = modePage;
                }

                if(currentModePage == null)
                {
                    pgMmc = new Modes.ModePage_01_MMC
                    {
                        PS             = false,
                        ReadRetryCount = 0x20,
                        Parameter      = 0x00
                    };

                    currentModePage = new Modes.ModePage
                    {
                        Page         = 0x01,
                        Subpage      = 0x00,
                        PageResponse = Modes.EncodeModePage_01_MMC(pgMmc)
                    };
                }

                pgMmc = new Modes.ModePage_01_MMC
                {
                    PS             = false,
                    ReadRetryCount = 255,
                    Parameter      = 0x20
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
                            PageResponse = Modes.EncodeModePage_01_MMC(pgMmc)
                        }
                    }
                };

                md6  = Modes.EncodeMode6(md, _dev.ScsiType);
                md10 = Modes.EncodeMode10(md, _dev.ScsiType);

                UpdateStatus?.Invoke(Localization.Core.Sending_MODE_SELECT_to_drive_return_damaged_blocks);
                _dumpLog.WriteLine(Localization.Core.Sending_MODE_SELECT_to_drive_return_damaged_blocks);
                sense = _dev.ModeSelect(md6, out senseBuf, true, false, _dev.Timeout, out _);

                if(sense)
                    sense = _dev.ModeSelect10(md10, out senseBuf, true, false, _dev.Timeout, out _);

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
                    PulseProgress?.Invoke(runningPersistent
                                              ? string.
                                                  Format(Localization.Core.Retrying_sector_0_pass_1_recovering_partial_data_forward,
                                                         badSector, pass)
                                              : string.Format(Localization.Core.Retrying_sector_0_pass_1_forward,
                                                              badSector, pass));
                else
                    PulseProgress?.Invoke(runningPersistent
                                              ? string.
                                                  Format(Localization.Core.Retrying_sector_0_pass_1_recovering_partial_data_reverse,
                                                         badSector, pass)
                                              : string.Format(Localization.Core.Retrying_sector_0_pass_1_reverse,
                                                              badSector, pass));

                sense = _dev.Read12(out readBuffer, out senseBuf, 0, false, false, false, false, (uint)badSector,
                                    blockSize, 0, 1, false, _dev.Timeout, out cmdDuration);

                totalDuration += cmdDuration;

                if(sense || _dev.Error)
                    _errorLog?.WriteLine(currentSector, _dev.Error, _dev.LastError, senseBuf);

                if(!sense &&
                   !_dev.Error)
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

            if(pass < _retryPasses &&
               !_aborted           &&
               _resume.BadBlocks.Count > 0)
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

                md6  = Modes.EncodeMode6(md, _dev.ScsiType);
                md10 = Modes.EncodeMode10(md, _dev.ScsiType);

                UpdateStatus?.Invoke(Localization.Core.Sending_MODE_SELECT_to_drive_return_device_to_previous_status);
                _dumpLog.WriteLine(Localization.Core.Sending_MODE_SELECT_to_drive_return_device_to_previous_status);
                sense = _dev.ModeSelect(md6, out senseBuf, true, false, _dev.Timeout, out _);

                if(sense)
                    _dev.ModeSelect10(md10, out senseBuf, true, false, _dev.Timeout, out _);
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
                AaruConsole.ErrorWriteLine(Localization.Core.Error_Tag_type_0_is_null_skipping, tag.Key);

                continue;
            }

            ret = outputFormat.WriteMediaTag(tag.Value, tag.Key);

            if(ret || _force)
                continue;

            // Cannot write tag to image
            _dumpLog.WriteLine(string.Format(Localization.Core.Cannot_write_tag_0, tag.Key));

            StoppingErrorMessage?.Invoke(string.Format(Localization.Core.Cannot_write_tag_0, tag.Key) +
                                         Environment.NewLine + outputFormat.ErrorMessage);

            return;
        }

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
            ErrorMessage?.Invoke(Localization.Core.Error_0_setting_metadata + Environment.NewLine +
                                 outputFormat.ErrorMessage);

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
            var layers = new Layers
            {
                Type = LayerType.OTP,
                Sectors = new List<Sectors>
                {
                    new()
                    {
                        Value = layerBreak
                    }
                }
            };

            WriteOpticalSidecar(blockSize, blocks, dskType, layers, mediaTags, 1, out totalChkDuration, null);
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
                                                    Per(totalDuration.Milliseconds()).Humanize()));

        if(maxSpeed > 0)
            UpdateStatus?.Invoke(string.Format(Localization.Core.Fastest_speed_burst_0,
                                               ByteSize.FromMegabytes(maxSpeed).Per(_oneSecond).Humanize()));

        if(minSpeed is > 0 and < double.MaxValue)
            UpdateStatus?.Invoke(string.Format(Localization.Core.Slowest_speed_burst_0,
                                               ByteSize.FromMegabytes(minSpeed).Per(_oneSecond).Humanize()));

        UpdateStatus?.Invoke(string.Format(Localization.Core._0_sectors_could_not_be_read, _resume.BadBlocks.Count));
        UpdateStatus?.Invoke("");

        Statistics.AddMedia(dskType, true);
    }
}