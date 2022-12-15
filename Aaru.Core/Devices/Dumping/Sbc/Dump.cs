// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SBC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps SCSI Block devices.
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
// Copyright © 2020-2023 Rebecca Wallander
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Console;
using Aaru.Core.Graphics;
using Aaru.Core.Logging;
using Aaru.Core.Media.Detection;
using Aaru.Decoders.Bluray;
using Aaru.Decoders.DVD;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Devices;
using DeviceReport = Aaru.Core.Devices.Report.DeviceReport;
using DVDDecryption = Aaru.Decryption.DVD.Dump;
using MediaType = Aaru.CommonTypes.MediaType;
using Track = Aaru.CommonTypes.Structs.Track;
using TrackType = Aaru.CommonTypes.Enums.TrackType;
using Usb = Aaru.CommonTypes.AaruMetadata.Usb;
using Version = Aaru.CommonTypes.Interop.Version;

// ReSharper disable JoinDeclarationAndInitializer

namespace Aaru.Core.Devices.Dumping;

/// <summary>Implements dumping SCSI Block Commands and Reduced Block Commands devices</summary>
partial class Dump
{
    /// <summary>Dumps a SCSI Block Commands device or a Reduced Block Commands devices</summary>
    /// <param name="opticalDisc">If device contains an optical disc (e.g. DVD or BD)</param>
    /// <param name="mediaTags">Media tags as retrieved in MMC layer</param>
    /// <param name="dskType">Disc type as detected in SCSI or MMC layer</param>
    /// <param name="dvdDecrypt">DVD CSS decryption module</param>
    void Sbc(Dictionary<MediaTagType, byte[]> mediaTags, MediaType dskType, bool opticalDisc,
             DVDDecryption dvdDecrypt = null)
    {
        bool               sense;
        byte               scsiMediumType     = 0;
        byte               scsiDensityCode    = 0;
        bool               containsFloppyPage = false;
        const ushort       sbcProfile         = 0x0001;
        DateTime           start;
        DateTime           end;
        double             totalDuration = 0;
        double             currentSpeed  = 0;
        double             maxSpeed      = double.MinValue;
        double             minSpeed      = double.MaxValue;
        Modes.DecodedMode? decMode       = null;
        bool               ret;
        ExtentsULong       blankExtents = null;
        var                outputFormat = _outputPlugin as IWritableImage;

        if(opticalDisc)
            switch(dskType)
            {
                case MediaType.REV35:
                case MediaType.REV70:
                case MediaType.REV120:
                    opticalDisc = false;

                    break;
            }

        _dumpLog.WriteLine(Localization.Core.Initializing_reader);
        var   scsiReader = new Reader(_dev, _dev.Timeout, null, _errorLog, _dumpRaw);
        ulong blocks     = scsiReader.GetDeviceBlocks();
        uint  blockSize  = scsiReader.LogicalBlockSize;

        if(!opticalDisc)
        {
            mediaTags = new Dictionary<MediaTagType, byte[]>();

            if(_dev.IsUsb &&
               _dev.UsbDescriptors != null)
                mediaTags.Add(MediaTagType.USB_Descriptors, null);

            if(_dev.Type == DeviceType.ATAPI)
                mediaTags.Add(MediaTagType.ATAPI_IDENTIFY, null);

            if(_dev.IsPcmcia &&
               _dev.Cis != null)
                mediaTags.Add(MediaTagType.PCMCIA_CIS, null);

            sense = _dev.ScsiInquiry(out byte[] cmdBuf, out _);

            if(_private)
                cmdBuf = DeviceReport.ClearInquiry(cmdBuf);

            mediaTags.Add(MediaTagType.SCSI_INQUIRY, cmdBuf);

            if(!sense)
            {
                _dumpLog.WriteLine(Localization.Core.Requesting_MODE_SENSE_10);
                UpdateStatus?.Invoke(Localization.Core.Requesting_MODE_SENSE_10);

                sense = _dev.ModeSense10(out cmdBuf, out _, false, true, ScsiModeSensePageControl.Current, 0x3F, 0xFF,
                                         5, out _);

                if(!sense ||
                   _dev.Error)
                    sense = _dev.ModeSense10(out cmdBuf, out _, false, true, ScsiModeSensePageControl.Current, 0x3F,
                                             0x00, 5, out _);

                if(!sense &&
                   !_dev.Error)
                    if(Modes.DecodeMode10(cmdBuf, _dev.ScsiType).HasValue)
                    {
                        mediaTags.Add(MediaTagType.SCSI_MODESENSE_10, cmdBuf);
                        decMode = Modes.DecodeMode10(cmdBuf, _dev.ScsiType);
                    }

                _dumpLog.WriteLine(Localization.Core.Requesting_MODE_SENSE_6);
                UpdateStatus?.Invoke(Localization.Core.Requesting_MODE_SENSE_6);

                sense = _dev.ModeSense6(out cmdBuf, out _, false, ScsiModeSensePageControl.Current, 0x3F, 0x00, 5,
                                        out _);

                if(sense || _dev.Error)
                    sense = _dev.ModeSense6(out cmdBuf, out _, false, ScsiModeSensePageControl.Current, 0x3F, 0x00, 5,
                                            out _);

                if(sense || _dev.Error)
                    sense = _dev.ModeSense(out cmdBuf, out _, 5, out _);

                if(!sense &&
                   !_dev.Error)
                    if(Modes.DecodeMode6(cmdBuf, _dev.ScsiType).HasValue)
                    {
                        mediaTags.Add(MediaTagType.SCSI_MODESENSE_6, cmdBuf);
                        decMode = Modes.DecodeMode6(cmdBuf, _dev.ScsiType);
                    }

                if(decMode.HasValue)
                {
                    scsiMediumType = (byte)decMode.Value.Header.MediumType;

                    if(decMode.Value.Header.BlockDescriptors?.Length > 0)
                        scsiDensityCode = (byte)decMode.Value.Header.BlockDescriptors[0].Density;

                    // TODO: Fix this
                    containsFloppyPage = decMode.Value.Pages?.Aggregate(containsFloppyPage,
                                                                        (current, modePage) =>
                                                                            current | (modePage.Page == 0x05)) == true;
                }
            }
        }

        if(dskType == MediaType.Unknown)
            dskType = MediaTypeFromDevice.GetFromScsi((byte)_dev.ScsiType, _dev.Manufacturer, _dev.Model,
                                                      scsiMediumType, scsiDensityCode, blocks + 1, blockSize,
                                                      _dev.IsUsb, opticalDisc);

        if(_dev.ScsiType == PeripheralDeviceTypes.MultiMediaDevice)
            MMC.DetectDiscType(ref dskType, 1, null, _dev, out _, out _, 0, blocks + 1);

        switch(dskType)
        {
            // Hi-MD devices show the disks while in Hi-MD mode, but they cannot be read using any known command
            // SonicStage changes the device mode, so it is no longer a mass storage device, and can only read
            // tracks written by that same application ID (changes between computers).
            case MediaType.MD:
                _dumpLog.WriteLine(Localization.Core.
                                                MiniDisc_albums_NetMD_discs_or_user_written_audio_MiniDisc_cannot_be_dumped);

                StoppingErrorMessage?.Invoke(Localization.Core.
                                                          MiniDisc_albums_NetMD_discs_or_user_written_audio_MiniDisc_cannot_be_dumped);

                return;
            case MediaType.Unknown when _dev.IsUsb && containsFloppyPage:
                dskType = MediaType.FlashDrive;

                break;
        }

        if(scsiReader.FindReadCommand())
        {
            _dumpLog.WriteLine(Localization.Core.ERROR_Cannot_find_correct_read_command_0, scsiReader.ErrorMessage);
            StoppingErrorMessage?.Invoke(Localization.Core.Unable_to_read_medium);

            return;
        }

        if(blocks    != 0 &&
           blockSize != 0)
        {
            blocks++;

            ulong totalSize = blocks * blockSize;

            switch(totalSize)
            {
                case > 1099511627776:
                    UpdateStatus?.
                        Invoke(string.Format(Localization.Core.Media_has_0_blocks_of_1_bytes_each_for_a_total_of_2_TiB,
                                             blocks, blockSize, totalSize / 1099511627776d));

                    break;
                case > 1073741824:
                    UpdateStatus?.
                        Invoke(string.Format(Localization.Core.Media_has_0_blocks_of_1_bytes_each_for_a_total_of_2_GiB,
                                             blocks, blockSize, totalSize / 1073741824d));

                    break;
                case > 1048576:
                    UpdateStatus?.
                        Invoke(string.Format(Localization.Core.Media_has_0_blocks_of_1_bytes_each_for_a_total_of_2_MiB,
                                             blocks, blockSize, totalSize / 1048576d));

                    break;
                case > 1024:
                    UpdateStatus?.
                        Invoke(string.Format(Localization.Core.Media_has_0_blocks_of_1_bytes_each_for_a_total_of_2_KiB,
                                             blocks, blockSize, totalSize / 1024d));

                    break;
                default:
                    UpdateStatus?.
                        Invoke(string.Format(Localization.Core.Media_has_0_blocks_of_1_bytes_each_for_a_total_of_2_bytes,
                                             blocks, blockSize, totalSize));

                    break;
            }
        }

        // Check how many blocks to read, if error show and return
        if(scsiReader.GetBlocksToRead(_maximumReadable))
        {
            _dumpLog.WriteLine(Localization.Core.ERROR_Cannot_get_blocks_to_read_0, scsiReader.ErrorMessage);
            StoppingErrorMessage?.Invoke(scsiReader.ErrorMessage);

            return;
        }

        uint blocksToRead      = scsiReader.BlocksToRead;
        uint logicalBlockSize  = blockSize;
        uint physicalBlockSize = scsiReader.PhysicalBlockSize;

        if(blocks == 0)
        {
            _dumpLog.WriteLine(Localization.Core.ERROR_Unable_to_read_medium_or_empty_medium_present);
            StoppingErrorMessage?.Invoke(Localization.Core.Unable_to_read_medium_or_empty_medium_present);

            return;
        }

        UpdateStatus?.Invoke(string.Format(Localization.Core.Device_reports_0_blocks_1_bytes, blocks,
                                           blocks * blockSize));

        UpdateStatus?.Invoke(string.Format(Localization.Core.Device_can_read_0_blocks_at_a_time, blocksToRead));
        UpdateStatus?.Invoke(string.Format(Localization.Core.Device_reports_0_bytes_per_logical_block, blockSize));

        UpdateStatus?.Invoke(string.Format(Localization.Core.Device_reports_0_bytes_per_physical_block,
                                           scsiReader.LongBlockSize));

        UpdateStatus?.Invoke(string.Format(Localization.Core.SCSI_device_type_0, _dev.ScsiType));
        UpdateStatus?.Invoke(string.Format(Localization.Core.SCSI_medium_type_0, scsiMediumType));
        UpdateStatus?.Invoke(string.Format(Localization.Core.SCSI_density_type_0, scsiDensityCode));
        UpdateStatus?.Invoke(string.Format(Localization.Core.SCSI_floppy_mode_page_present_0, containsFloppyPage));
        UpdateStatus?.Invoke(string.Format(Localization.Core.Media_identified_as_0, dskType));

        _dumpLog.WriteLine(Localization.Core.Device_reports_0_blocks_1_bytes, blocks, blocks * blockSize);
        _dumpLog.WriteLine(Localization.Core.Device_can_read_0_blocks_at_a_time, blocksToRead);
        _dumpLog.WriteLine(Localization.Core.Device_reports_0_bytes_per_logical_block, blockSize);
        _dumpLog.WriteLine(Localization.Core.Device_reports_0_bytes_per_physical_block, scsiReader.LongBlockSize);
        _dumpLog.WriteLine(Localization.Core.SCSI_device_type_0, _dev.ScsiType);
        _dumpLog.WriteLine(Localization.Core.SCSI_medium_type_0, scsiMediumType);
        _dumpLog.WriteLine(Localization.Core.SCSI_density_type_0, scsiDensityCode);
        _dumpLog.WriteLine(Localization.Core.SCSI_floppy_mode_page_present_0, containsFloppyPage);
        _dumpLog.WriteLine(Localization.Core.Media_identified_as_0, dskType);

        uint longBlockSize = scsiReader.LongBlockSize;

        if(_dumpRaw)
            if(blockSize == longBlockSize)
            {
                ErrorMessage?.Invoke(!scsiReader.CanReadRaw
                                         ? Localization.Core.Device_doesnt_seem_capable_of_reading_raw_data_from_media
                                         : Localization.Core.
                                                        Device_is_capable_of_reading_raw_data_but_Ive_been_unable_to_guess_correct_sector_size);

                if(!_force)
                {
                    StoppingErrorMessage?.Invoke(Localization.Core.
                                                              If_you_want_to_continue_reading_cooked_data_when_raw_is_not_available_use_the_force_option);

                    // TODO: Exit more gracefully
                    return;
                }

                ErrorMessage?.Invoke(Localization.Core.Continuing_dumping_cooked_data);
            }
            else
            {
                // Only a block will be read, but it contains 16 sectors and command expect sector number not block number
                blocksToRead = (uint)(longBlockSize == 37856 ? 16 : 1);

                UpdateStatus?.Invoke(string.Format(Localization.Core.Reading_0_raw_bytes_1_cooked_bytes_per_sector,
                                                   longBlockSize, blockSize * blocksToRead));

                physicalBlockSize = longBlockSize;
                blockSize         = longBlockSize;
            }

        ret = true;

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

        UpdateStatus?.Invoke(string.Format(Localization.Core.Reading_0_sectors_at_a_time, blocksToRead));
        _dumpLog.WriteLine(Localization.Core.Reading_0_sectors_at_a_time, blocksToRead);

        var mhddLog = new MhddLog(_outputPrefix + ".mhddlog.bin", _dev, blocks, blockSize, blocksToRead, _private,
                                  _dimensions);

        var  ibgLog       = new IbgLog(_outputPrefix + ".ibg", sbcProfile);
        bool imageCreated = false;

        if(!opticalDisc)
        {
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

            imageCreated = true;
        }

        start = DateTime.UtcNow;
        double imageWriteDuration      = 0;
        bool   writeSingleOpticalTrack = true;

        if(opticalDisc)
        {
            if(outputFormat is IWritableOpticalImage opticalPlugin)
            {
                sense = _dev.ReadDiscInformation(out byte[] readBuffer, out _,
                                                 MmcDiscInformationDataTypes.DiscInformation, _dev.Timeout, out _);

                if(!sense)
                {
                    DiscInformation.StandardDiscInformation? discInformation = DiscInformation.Decode000b(readBuffer);

                    // This means the output image can store sessions that are not on a CD, like on a DVD or Blu-ray
                    bool canStoreNotCdSessions =
                        opticalPlugin.OpticalCapabilities.HasFlag(OpticalImageCapabilities.CanStoreNotCdSessions);

                    // This means the output image can store tracks that are not on a CD, like on a DVD or Blu-ray
                    bool canStoreNotCdTracks =
                        opticalPlugin.OpticalCapabilities.HasFlag(OpticalImageCapabilities.CanStoreNotCdTracks);

                    if(discInformation.HasValue)
                    {
                        writeSingleOpticalTrack = false;

                        if(discInformation?.Sessions > 1 &&
                           !canStoreNotCdSessions)
                        {
                            if(_force)
                            {
                                _dumpLog.WriteLine(Localization.Core.
                                                                Image_does_not_support_multiple_sessions_in_non_Compact_Disc_dumps_continuing);

                                ErrorMessage?.Invoke(Localization.Core.
                                                                  Image_does_not_support_multiple_sessions_in_non_Compact_Disc_dumps_continuing);
                            }
                            else
                            {
                                _dumpLog.WriteLine(Localization.Core.
                                                                Image_does_not_support_multiple_sessions_in_non_Compact_Disc_dumps_not_continuing);

                                StoppingErrorMessage?.Invoke(Localization.Core.
                                                                          Image_does_not_support_multiple_sessions_in_non_Compact_Disc_dumps_not_continuing);

                                return;
                            }
                        }

                        if((discInformation?.LastTrackLastSession - discInformation?.FirstTrackNumber > 0 ||
                            discInformation?.FirstTrackNumber                                         != 1) &&
                           !canStoreNotCdTracks)
                        {
                            if(_force)
                            {
                                _dumpLog.WriteLine(Localization.Core.
                                                                Image_does_not_support_multiple_tracks_in_non_Compact_Disc_dumps_continuing);

                                ErrorMessage?.Invoke(Localization.Core.
                                                                  Image_does_not_support_multiple_tracks_in_non_Compact_Disc_dumps_continuing);
                            }
                            else
                            {
                                _dumpLog.WriteLine(Localization.Core.
                                                                Image_does_not_support_multiple_tracks_in_non_Compact_Disc_dumps_not_continuing);

                                StoppingErrorMessage?.Invoke(Localization.Core.
                                                                          Image_does_not_support_multiple_tracks_in_non_Compact_Disc_dumps_not_continuing);

                                return;
                            }
                        }

                        UpdateStatus?.Invoke(Localization.Core.Building_track_map);
                        _dumpLog.WriteLine(Localization.Core.Building_track_map);

                        List<Track> tracks = new();

                        for(ushort tno = discInformation.Value.FirstTrackNumber;
                            tno <= discInformation?.LastTrackLastSession; tno++)
                        {
                            sense = _dev.ReadTrackInformation(out readBuffer, out _, false,
                                                              TrackInformationType.LogicalTrackNumber, tno,
                                                              _dev.Timeout, out _);

                            if(sense)
                                continue;

                            var trkInfo = TrackInformation.Decode(readBuffer);

                            if(trkInfo is null)
                                continue;

                            // Some drives return this invalid value with recordable discs
                            if(trkInfo.LogicalTrackNumber == 0)
                                continue;

                            // Fixes a firmware bug in some DVD drives
                            if((int)trkInfo.LogicalTrackStartAddress < 0)
                                trkInfo.LogicalTrackStartAddress = 0;

                            // Some drives return this invalid value with recordable discs
                            if(trkInfo.LogicalTrackSize == 0xFFFFFFFF)
                                trkInfo.LogicalTrackSize = (uint)(blocks - trkInfo.LogicalTrackStartAddress);

                            var track = new Track
                            {
                                Sequence          = trkInfo.LogicalTrackNumber,
                                Session           = (ushort)(canStoreNotCdSessions ? trkInfo.SessionNumber : 1),
                                Type              = TrackType.Data,
                                StartSector       = trkInfo.LogicalTrackStartAddress,
                                EndSector         = trkInfo.LogicalTrackSize + trkInfo.LogicalTrackStartAddress - 1,
                                RawBytesPerSector = (int)blockSize,
                                BytesPerSector    = (int)blockSize,
                                SubchannelType    = TrackSubchannelType.None
                            };

                            if(track.EndSector >= blocks)
                                blocks = track.EndSector + 1;

                            tracks.Add(track);
                        }

                        if(tracks.Count == 0)
                            tracks.Add(new Track
                            {
                                BytesPerSector    = (int)blockSize,
                                EndSector         = blocks - 1,
                                Sequence          = 1,
                                RawBytesPerSector = (int)blockSize,
                                SubchannelType    = TrackSubchannelType.None,
                                Session           = 1,
                                Type              = TrackType.Data
                            });
                        else
                            tracks = tracks.OrderBy(t => t.Sequence).ToList();

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

                        imageCreated = true;

                    #if DEBUG
                        foreach(Track trk in tracks)
                            UpdateStatus?.
                                Invoke(string.Format(Localization.Core.Track_0_starts_at_LBA_1_and_ends_at_LBA_2,
                                                     trk.Sequence, trk.StartSector, trk.EndSector));
                    #endif

                        if(canStoreNotCdTracks)
                        {
                            ret = opticalPlugin.SetTracks(tracks);

                            if(!ret)
                            {
                                _dumpLog.WriteLine(Localization.Core.
                                                                Error_sending_tracks_to_output_image_not_continuing);

                                _dumpLog.WriteLine(opticalPlugin.ErrorMessage);

                                StoppingErrorMessage?.Invoke(Localization.Core.
                                                                          Error_sending_tracks_to_output_image_not_continuing +
                                                             Environment.NewLine + opticalPlugin.ErrorMessage);

                                return;
                            }
                        }
                        else
                            opticalPlugin.SetTracks(new List<Track>
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
                    }
                }
            }
            else
            {
                _dumpLog.WriteLine(Localization.Core.The_specified_plugin_does_not_support_storing_optical_disc_images);

                StoppingErrorMessage?.Invoke(Localization.Core.
                                                          The_specified_plugin_does_not_support_storing_optical_disc_images);

                return;
            }
        }
        else if(decMode?.Pages != null)
        {
            bool setGeometry = false;

            foreach(Modes.ModePage page in decMode.Value.Pages)
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

        if(!imageCreated)
        {
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

            if(writeSingleOpticalTrack)
            {
                _dumpLog.WriteLine(Localization.Core.Creating_single_track_as_could_not_retrieve_track_list_from_drive);

                UpdateStatus?.Invoke(Localization.Core.
                                                  Creating_single_track_as_could_not_retrieve_track_list_from_drive);

                (outputFormat as IWritableOpticalImage)?.SetTracks(new List<Track>
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
            }
        }

        DumpHardware currentTry = null;
        ExtentsULong extents    = null;

        ResumeSupport.Process(true, _dev.IsRemovable, blocks, _dev.Manufacturer, _dev.Model, _dev.Serial,
                              _dev.PlatformId, ref _resume, ref currentTry, ref extents, _dev.FirmwareRevision,
                              _private, _force);

        if(_createGraph)
        {
            bool discIs80Mm =
                (mediaTags?.TryGetValue(MediaTagType.DVD_PFI, out byte[] pfiBytes) == true &&
                 PFI.Decode(pfiBytes, dskType)?.DiscSize                           == DVDSize.Eighty) ||
                (mediaTags?.TryGetValue(MediaTagType.BD_DI, out byte[] diBytes) == true && DI.
                     Decode(diBytes)?.Units?.Any(s => s.DiscSize == DI.BluSize.Eighty) == true);

            Spiral.DiscParameters discSpiralParameters = Spiral.DiscParametersFromMediaType(dskType, discIs80Mm);

            if(discSpiralParameters is not null)
                _mediaGraph = new Spiral((int)_dimensions, (int)_dimensions, discSpiralParameters, blocks);
            else
                _mediaGraph = new BlockMap((int)_dimensions, (int)_dimensions, blocks);

            if(_mediaGraph is not null)
                foreach(Tuple<ulong, ulong> e in extents.ToArray())
                    _mediaGraph?.PaintSectorsGood(e.Item1, (uint)(e.Item2 - e.Item1 + 2));

            _mediaGraph?.PaintSectorsBad(_resume.BadBlocks);
        }

        if(currentTry == null ||
           extents    == null)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Could_not_process_resume_file_not_continuing);

            return;
        }

        if(_resume.NextBlock > 0)
        {
            UpdateStatus?.Invoke(string.Format(Localization.Core.Resuming_from_block_0, _resume.NextBlock));
            _dumpLog.WriteLine(Localization.Core.Resuming_from_block_0, _resume.NextBlock);
        }

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

        if(_resume?.BlankExtents != null)
            blankExtents = ExtentsConverter.FromMetadata(_resume.BlankExtents);

        bool newTrim = false;

        if(mediaTags.TryGetValue(MediaTagType.DVD_CMI, out byte[] cmi) &&
           Settings.Settings.Current.EnableDecryption                  &&
           _titleKeys                                                  &&
           dskType               == MediaType.DVDROM                   &&
           (CopyrightType)cmi[0] == CopyrightType.CSS)
        {
            UpdateStatus?.Invoke(Localization.Core.Title_keys_dumping_is_enabled_This_will_be_very_slow);
            _resume.MissingTitleKeys ??= new List<ulong>(Enumerable.Range(0, (int)blocks).Select(n => (ulong)n));
        }

        if(_dev.ScsiType == PeripheralDeviceTypes.OpticalDevice)
            ReadOpticalData(blocks, blocksToRead, blockSize, currentTry, extents, ref currentSpeed, ref minSpeed,
                            ref maxSpeed, ref totalDuration, scsiReader, mhddLog, ibgLog, ref imageWriteDuration,
                            ref newTrim, ref blankExtents);
        else
            ReadSbcData(blocks, blocksToRead, blockSize, currentTry, extents, ref currentSpeed, ref minSpeed,
                        ref maxSpeed, ref totalDuration, scsiReader, mhddLog, ibgLog, ref imageWriteDuration,
                        ref newTrim, ref dvdDecrypt,
                        mediaTags.ContainsKey(MediaTagType.DVD_DiscKey_Decrypted)
                            ? mediaTags[MediaTagType.DVD_DiscKey_Decrypted] : null);

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

        #region Trimming
        if(_resume.BadBlocks.Count > 0 &&
           !_aborted                   &&
           _trim                       &&
           newTrim)
        {
            start = DateTime.UtcNow;
            UpdateStatus?.Invoke(Localization.Core.Trimming_skipped_sectors);
            _dumpLog.WriteLine(Localization.Core.Trimming_skipped_sectors);

            InitProgress?.Invoke();

            TrimSbcData(scsiReader, extents, currentTry, blankExtents);

            EndProgress?.Invoke();
            end = DateTime.UtcNow;

            UpdateStatus?.Invoke(string.Format(Localization.Core.Trimming_finished_in_0_seconds,
                                               (end - start).TotalSeconds));

            _dumpLog.WriteLine(Localization.Core.Trimming_finished_in_0_seconds, (end - start).TotalSeconds);
        }
        #endregion Trimming

        #region Error handling
        if(_resume.BadBlocks.Count > 0 &&
           !_aborted                   &&
           _retryPasses > 0)
            RetrySbcData(scsiReader, currentTry, extents, ref totalDuration, blankExtents);

        if(_resume.MissingTitleKeys?.Count > 0        &&
           !_aborted                                  &&
           _retryPasses > 0                           &&
           Settings.Settings.Current.EnableDecryption &&
           _titleKeys                                 &&
           mediaTags.ContainsKey(MediaTagType.DVD_DiscKey_Decrypted))
            RetryTitleKeys(dvdDecrypt, mediaTags[MediaTagType.DVD_DiscKey_Decrypted], ref totalDuration);
        #endregion Error handling

        if(opticalDisc)
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
                StoppingErrorMessage?.Invoke(string.Format(Localization.Core.Cannot_write_tag_0, tag.Key));

                _dumpLog.WriteLine(string.Format(Localization.Core.Cannot_write_tag_0, tag.Key) + Environment.NewLine +
                                   outputFormat.ErrorMessage);

                return;
            }
        else
        {
            if(!_dev.IsRemovable ||
               _dev.IsUsb)
            {
                if(_dev.IsUsb &&
                   _dev.UsbDescriptors != null)
                {
                    UpdateStatus?.Invoke(Localization.Core.Reading_USB_descriptors);
                    _dumpLog.WriteLine(Localization.Core.Reading_USB_descriptors);
                    ret = outputFormat.WriteMediaTag(_dev.UsbDescriptors, MediaTagType.USB_Descriptors);

                    if(!ret &&
                       !_force)
                    {
                        _dumpLog.WriteLine(Localization.Core.Cannot_write_USB_descriptors);

                        StoppingErrorMessage?.Invoke(Localization.Core.Cannot_write_USB_descriptors +
                                                     Environment.NewLine + outputFormat.ErrorMessage);

                        return;
                    }
                }

                byte[] cmdBuf;

                if(_dev.Type == DeviceType.ATAPI)
                {
                    UpdateStatus?.Invoke(Localization.Core.Requesting_ATAPI_IDENTIFY_PACKET_DEVICE);
                    _dumpLog.WriteLine(Localization.Core.Requesting_ATAPI_IDENTIFY_PACKET_DEVICE);
                    sense = _dev.AtapiIdentify(out cmdBuf, out _);

                    if(!sense)
                    {
                        if(_private)
                            cmdBuf = DeviceReport.ClearIdentify(cmdBuf);

                        ret = outputFormat.WriteMediaTag(cmdBuf, MediaTagType.ATAPI_IDENTIFY);

                        if(!ret &&
                           !_force)
                        {
                            _dumpLog.WriteLine(Localization.Core.Cannot_write_ATAPI_IDENTIFY_PACKET_DEVICE);

                            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_write_ATAPI_IDENTIFY_PACKET_DEVICE +
                                                         Environment.NewLine + outputFormat.ErrorMessage);

                            return;
                        }
                    }
                }

                sense = _dev.ScsiInquiry(out cmdBuf, out _);

                if(!sense)
                {
                    UpdateStatus?.Invoke(Localization.Core.Requesting_SCSI_INQUIRY);
                    _dumpLog.WriteLine(Localization.Core.Requesting_SCSI_INQUIRY);
                    ret = outputFormat.WriteMediaTag(cmdBuf, MediaTagType.SCSI_INQUIRY);

                    if(!ret &&
                       !_force)
                    {
                        StoppingErrorMessage?.Invoke(Localization.Core.Cannot_write_SCSI_INQUIRY);

                        _dumpLog.WriteLine(Localization.Core.Cannot_write_SCSI_INQUIRY + Environment.NewLine +
                                           outputFormat.ErrorMessage);

                        return;
                    }

                    UpdateStatus?.Invoke(Localization.Core.Requesting_MODE_SENSE_10);
                    _dumpLog.WriteLine(Localization.Core.Requesting_MODE_SENSE_10);

                    sense = _dev.ModeSense10(out cmdBuf, out _, false, true, ScsiModeSensePageControl.Current, 0x3F,
                                             0xFF, 5, out _);

                    if(!sense ||
                       _dev.Error)
                        sense = _dev.ModeSense10(out cmdBuf, out _, false, true, ScsiModeSensePageControl.Current, 0x3F,
                                                 0x00, 5, out _);

                    if(!sense &&
                       !_dev.Error)
                        if(Modes.DecodeMode10(cmdBuf, _dev.ScsiType).HasValue)
                        {
                            ret = outputFormat.WriteMediaTag(cmdBuf, MediaTagType.SCSI_MODESENSE_10);

                            if(!ret &&
                               !_force)
                            {
                                _dumpLog.WriteLine(Localization.Core.Cannot_write_SCSI_MODE_SENSE_10);

                                StoppingErrorMessage?.Invoke(Localization.Core.Cannot_write_SCSI_MODE_SENSE_10 +
                                                             Environment.NewLine + outputFormat.ErrorMessage);

                                return;
                            }
                        }

                    UpdateStatus?.Invoke(Localization.Core.Requesting_MODE_SENSE_6);
                    _dumpLog.WriteLine(Localization.Core.Requesting_MODE_SENSE_6);

                    sense = _dev.ModeSense6(out cmdBuf, out _, false, ScsiModeSensePageControl.Current, 0x3F, 0x00, 5,
                                            out _);

                    if(sense || _dev.Error)
                        sense = _dev.ModeSense6(out cmdBuf, out _, false, ScsiModeSensePageControl.Current, 0x3F, 0x00,
                                                5, out _);

                    if(sense || _dev.Error)
                        sense = _dev.ModeSense(out cmdBuf, out _, 5, out _);

                    if(!sense &&
                       !_dev.Error)
                        if(Modes.DecodeMode6(cmdBuf, _dev.ScsiType).HasValue)
                        {
                            ret = outputFormat.WriteMediaTag(cmdBuf, MediaTagType.SCSI_MODESENSE_6);

                            if(!ret &&
                               !_force)
                            {
                                _dumpLog.WriteLine(Localization.Core.Cannot_write_SCSI_MODE_SENSE_6);

                                StoppingErrorMessage?.Invoke(Localization.Core.Cannot_write_SCSI_MODE_SENSE_6 +
                                                             Environment.NewLine + outputFormat.ErrorMessage);

                                return;
                            }
                        }
                }
            }
        }

        _resume.BadBlocks.Sort();

        foreach(ulong bad in _resume.BadBlocks)
            _dumpLog.WriteLine(Localization.Core.Sector_0_could_not_be_read, bad);

        currentTry.Extents = ExtentsConverter.ToMetadata(extents);

        outputFormat.SetDumpHardware(_resume.Tries);

        // TODO: Media Serial Number
        // TODO: Non-removable drive information
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
        DateTime closeStart = DateTime.Now;
        outputFormat.Close();
        DateTime closeEnd = DateTime.Now;

        UpdateStatus?.Invoke(string.Format(Localization.Core.Closed_in_0_seconds,
                                           (closeEnd - closeStart).TotalSeconds));

        _dumpLog.WriteLine(Localization.Core.Closed_in_0_seconds, (closeEnd - closeStart).TotalSeconds);

        if(_aborted)
        {
            UpdateStatus?.Invoke(Localization.Core.Aborted);
            _dumpLog.WriteLine(Localization.Core.Aborted);

            return;
        }

        double totalChkDuration = 0;

        if(_metadata)
        {
            // TODO: Layers
            if(opticalDisc)
                WriteOpticalSidecar(blockSize, blocks, dskType, null, mediaTags, 1, out totalChkDuration, null);
            else
            {
                UpdateStatus?.Invoke(Localization.Core.Creating_sidecar);
                _dumpLog.WriteLine(Localization.Core.Creating_sidecar);
                var         filters     = new FiltersList();
                IFilter     filter      = filters.GetFilter(_outputPath);
                var         inputPlugin = ImageFormat.Detect(filter) as IMediaImage;
                ErrorNumber opened      = inputPlugin.Open(filter);

                if(opened != ErrorNumber.NoError)
                {
                    StoppingErrorMessage?.Invoke(string.Format(Localization.Core.Error_0_opening_created_image,
                                                               opened));

                    return;
                }

                DateTime chkStart = DateTime.UtcNow;
                _sidecarClass                      =  new Sidecar(inputPlugin, _outputPath, filter.Id, _encoding);
                _sidecarClass.InitProgressEvent    += InitProgress;
                _sidecarClass.UpdateProgressEvent  += UpdateProgress;
                _sidecarClass.EndProgressEvent     += EndProgress;
                _sidecarClass.InitProgressEvent2   += InitProgress2;
                _sidecarClass.UpdateProgressEvent2 += UpdateProgress2;
                _sidecarClass.EndProgressEvent2    += EndProgress2;
                _sidecarClass.UpdateStatusEvent    += UpdateStatus;
                Metadata sidecar = _sidecarClass.Create();
                end = DateTime.UtcNow;

                if(!_aborted)
                {
                    totalChkDuration = (end - chkStart).TotalMilliseconds;

                    UpdateStatus?.Invoke(string.Format(Localization.Core.Sidecar_created_in_0_seconds,
                                                       (end - chkStart).TotalSeconds));

                    UpdateStatus?.Invoke(string.Format(Localization.Core.Average_checksum_speed_0_KiB_sec,
                                                       blockSize * (double)(blocks + 1) / 1024 /
                                                       (totalChkDuration / 1000)));

                    _dumpLog.WriteLine(Localization.Core.Sidecar_created_in_0_seconds, (end - chkStart).TotalSeconds);

                    _dumpLog.WriteLine(Localization.Core.Average_checksum_speed_0_KiB_sec,
                                       blockSize * (double)(blocks + 1) / 1024 / (totalChkDuration / 1000));

                    if(_preSidecar != null)
                    {
                        _preSidecar.BlockMedias = sidecar.BlockMedias;
                        sidecar                 = _preSidecar;
                    }

                    // All USB flash drives report as removable, even if the media is not removable
                    if(!_dev.IsRemovable ||
                       _dev.IsUsb)
                    {
                        if(_dev.IsUsb &&
                           _dev.UsbDescriptors != null)
                            if(outputFormat.SupportedMediaTags.Contains(MediaTagType.USB_Descriptors))
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

                        byte[] cmdBuf;

                        if(_dev.Type == DeviceType.ATAPI)
                        {
                            sense = _dev.AtapiIdentify(out cmdBuf, out _);

                            if(!sense)
                                if(outputFormat.SupportedMediaTags.Contains(MediaTagType.ATAPI_IDENTIFY))
                                    sidecar.BlockMedias[0].ATA = new ATA
                                    {
                                        Identify = new CommonTypes.AaruMetadata.Dump
                                        {
                                            Image     = _outputPath,
                                            Size      = (ulong)cmdBuf.Length,
                                            Checksums = Checksum.GetChecksums(cmdBuf)
                                        }
                                    };
                        }

                        sense = _dev.ScsiInquiry(out cmdBuf, out _);

                        if(!sense)
                        {
                            if(outputFormat.SupportedMediaTags.Contains(MediaTagType.SCSI_INQUIRY))
                                sidecar.BlockMedias[0].SCSI = new SCSI
                                {
                                    Inquiry = new CommonTypes.AaruMetadata.Dump
                                    {
                                        Image     = _outputPath,
                                        Size      = (ulong)cmdBuf.Length,
                                        Checksums = Checksum.GetChecksums(cmdBuf)
                                    }
                                };

                            // TODO: SCSI Extended Vendor Page descriptors
                            /*
                            UpdateStatus?.Invoke("Reading SCSI Extended Vendor Page Descriptors.");
                            dumpLog.WriteLine("Reading SCSI Extended Vendor Page Descriptors.");
                            sense = dev.ScsiInquiry(out cmdBuf, out _, 0x00);
                            if(!sense)
                            {
                                byte[] pages = EVPD.DecodePage00(cmdBuf);

                                if(pages != null)
                                {
                                    List<Evpd> evpds = new();
                                    foreach(byte page in pages)
                                    {
                                        dumpLog.WriteLine("Requesting page {0:X2}h.", page);
                                        sense = dev.ScsiInquiry(out cmdBuf, out _, page);
                                        if(sense) continue;

                                        Evpd evpd = new()
                                        {
                                            Image = $"{outputPrefix}.evpd_{page:X2}h.bin",
                                            Checksums = Checksum.GetChecksums(cmdBuf),
                                            Size = cmdBuf.Length
                                        };
                                        evpd.Checksums = Checksum.GetChecksums(cmdBuf);
                                        DataFile.WriteTo("SCSI Dump", evpd.Image, cmdBuf);
                                        evpds.Add(evpd);
                                    }

                                    if(evpds.Count > 0) sidecar.BlockMedias[0].SCSI.Evpds = evpds;
                                }
                            }
                            */

                            UpdateStatus?.Invoke(Localization.Core.Requesting_MODE_SENSE_10);
                            _dumpLog.WriteLine(Localization.Core.Requesting_MODE_SENSE_10);

                            sense = _dev.ModeSense10(out cmdBuf, out _, false, true, ScsiModeSensePageControl.Current,
                                                     0x3F, 0xFF, 5, out _);

                            if(!sense ||
                               _dev.Error)
                                sense = _dev.ModeSense10(out cmdBuf, out _, false, true,
                                                         ScsiModeSensePageControl.Current, 0x3F, 0x00, 5, out _);

                            if(!sense &&
                               !_dev.Error)
                                if(Modes.DecodeMode10(cmdBuf, _dev.ScsiType).HasValue)
                                    if(outputFormat.SupportedMediaTags.Contains(MediaTagType.SCSI_MODESENSE_10))
                                        sidecar.BlockMedias[0].SCSI.ModeSense10 = new CommonTypes.AaruMetadata.Dump
                                        {
                                            Image     = _outputPath,
                                            Size      = (ulong)cmdBuf.Length,
                                            Checksums = Checksum.GetChecksums(cmdBuf)
                                        };

                            UpdateStatus?.Invoke(Localization.Core.Requesting_MODE_SENSE_6);
                            _dumpLog.WriteLine(Localization.Core.Requesting_MODE_SENSE_6);

                            sense = _dev.ModeSense6(out cmdBuf, out _, false, ScsiModeSensePageControl.Current, 0x3F,
                                                    0x00, 5, out _);

                            if(sense || _dev.Error)
                                sense = _dev.ModeSense6(out cmdBuf, out _, false, ScsiModeSensePageControl.Current,
                                                        0x3F, 0x00, 5, out _);

                            if(sense || _dev.Error)
                                sense = _dev.ModeSense(out cmdBuf, out _, 5, out _);

                            if(!sense &&
                               !_dev.Error)
                                if(Modes.DecodeMode6(cmdBuf, _dev.ScsiType).HasValue)
                                    if(outputFormat.SupportedMediaTags.Contains(MediaTagType.SCSI_MODESENSE_6))
                                        sidecar.BlockMedias[0].SCSI.ModeSense = new CommonTypes.AaruMetadata.Dump
                                        {
                                            Image     = _outputPath,
                                            Size      = (ulong)cmdBuf.Length,
                                            Checksums = Checksum.GetChecksums(cmdBuf)
                                        };
                        }
                    }

                    List<(ulong start, string type)> filesystems = new();

                    if(sidecar.BlockMedias[0].FileSystemInformation != null)
                        filesystems.AddRange(from partition in sidecar.BlockMedias[0].FileSystemInformation
                                             where partition.FileSystems != null
                                             from fileSystem in partition.FileSystems
                                             select (partition.StartSector, fileSystem.Type));

                    if(filesystems.Count > 0)
                        foreach(var filesystem in filesystems.Select(o => new
                                {
                                    o.start,
                                    o.type
                                }).Distinct())
                        {
                            UpdateStatus?.Invoke(string.Format(Localization.Core.Found_filesystem_0_at_sector_1,
                                                               filesystem.type, filesystem.start));

                            _dumpLog.WriteLine(Localization.Core.Found_filesystem_0_at_sector_1, filesystem.type,
                                               filesystem.start);
                        }

                    sidecar.BlockMedias[0].Dimensions = Dimensions.DimensionsFromMediaType(dskType);

                    (string type, string subType) xmlType = CommonTypes.Metadata.MediaType.MediaTypeToString(dskType);

                    sidecar.BlockMedias[0].MediaType    = xmlType.type;
                    sidecar.BlockMedias[0].MediaSubType = xmlType.subType;

                    // TODO: Implement device firmware revision
                    if(!_dev.IsRemovable ||
                       _dev.IsUsb)
                        if(_dev.Type == DeviceType.ATAPI)
                            sidecar.BlockMedias[0].Interface = "ATAPI";
                        else if(_dev.IsUsb)
                            sidecar.BlockMedias[0].Interface = "USB";
                        else if(_dev.IsFireWire)
                            sidecar.BlockMedias[0].Interface = "FireWire";
                        else
                            sidecar.BlockMedias[0].Interface = "SCSI";

                    sidecar.BlockMedias[0].LogicalBlocks     = blocks;
                    sidecar.BlockMedias[0].PhysicalBlockSize = physicalBlockSize;
                    sidecar.BlockMedias[0].LogicalBlockSize  = logicalBlockSize;
                    sidecar.BlockMedias[0].Manufacturer      = _dev.Manufacturer;
                    sidecar.BlockMedias[0].Model             = _dev.Model;

                    if(!_private)
                        sidecar.BlockMedias[0].Serial = _dev.Serial;

                    sidecar.BlockMedias[0].Size = blocks * blockSize;

                    if(_dev.IsRemovable)
                        sidecar.BlockMedias[0].DumpHardware = _resume.Tries;

                    UpdateStatus?.Invoke(Localization.Core.Writing_metadata_sidecar);

                    var jsonFs = new FileStream(_outputPrefix + ".metadata.json", FileMode.Create);

                    JsonSerializer.Serialize(jsonFs, sidecar, new JsonSerializerOptions
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        WriteIndented          = true
                    });

                    jsonFs.Close();
                }
            }
        }

        UpdateStatus?.Invoke("");

        UpdateStatus?.
            Invoke(string.Format(Localization.Core.Took_a_total_of_0_seconds_1_processing_commands_2_checksumming_3_writing_4_closing,
                                 (end - start).TotalSeconds, totalDuration / 1000, totalChkDuration / 1000,
                                 imageWriteDuration, (closeEnd - closeStart).TotalSeconds));

        UpdateStatus?.Invoke(string.Format(Localization.Core.Average_speed_0_MiB_sec,
                                           blockSize * (double)(blocks + 1) / 1048576 / (totalDuration / 1000)));

        if(maxSpeed > 0)
            UpdateStatus?.Invoke(string.Format(Localization.Core.Fastest_speed_burst_0_MiB_sec, maxSpeed));

        if(minSpeed is > 0 and < double.MaxValue)
            UpdateStatus?.Invoke(string.Format(Localization.Core.Slowest_speed_burst_0_MiB_sec, minSpeed));

        UpdateStatus?.Invoke(string.Format(Localization.Core._0_sectors_could_not_be_read, _resume.BadBlocks.Count));
        UpdateStatus?.Invoke("");

        Statistics.AddMedia(dskType, true);
    }
}