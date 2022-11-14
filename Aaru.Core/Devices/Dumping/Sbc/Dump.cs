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
// Copyright © 2011-2022 Natalia Portillo
// Copyright © 2020-2022 Rebecca Wallander
// ****************************************************************************/

using DVDDecryption = Aaru.Decryption.DVD.Dump;

// ReSharper disable JoinDeclarationAndInitializer

namespace Aaru.Core.Devices.Dumping;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Aaru.CommonTypes.Structs;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Console;
using Aaru.Core.Logging;
using Aaru.Core.Media.Detection;
using Aaru.Decoders.DVD;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Devices;
using Aaru.Settings;
using Schemas;
using DeviceReport = Aaru.Core.Devices.Report.DeviceReport;
using MediaType = Aaru.CommonTypes.MediaType;
using TrackType = Aaru.CommonTypes.Enums.TrackType;
using Version = Aaru.CommonTypes.Interop.Version;

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
        var                containsFloppyPage = false;
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

        _dumpLog.WriteLine("Initializing reader.");
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
                _dumpLog.WriteLine("Requesting MODE SENSE (10).");
                UpdateStatus?.Invoke("Requesting MODE SENSE (10).");

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

                _dumpLog.WriteLine("Requesting MODE SENSE (6).");
                UpdateStatus?.Invoke("Requesting MODE SENSE (6).");

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
                _dumpLog.WriteLine("MiniDisc albums, NetMD discs or user-written audio MiniDisc cannot be dumped.");

                StoppingErrorMessage?.
                    Invoke("MiniDisc albums, NetMD discs or user-written audio MiniDisc cannot be dumped.");

                return;
            case MediaType.Unknown when _dev.IsUsb && containsFloppyPage:
                dskType = MediaType.FlashDrive;

                break;
        }

        if(scsiReader.FindReadCommand())
        {
            _dumpLog.WriteLine("ERROR: Cannot find correct read command: {0}.", scsiReader.ErrorMessage);
            StoppingErrorMessage?.Invoke("Unable to read medium.");

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
                    UpdateStatus?.Invoke($"Media has {blocks} blocks of {blockSize} bytes/each. (for a total of {
                        totalSize / 1099511627776d:F3} TiB)");

                    break;
                case > 1073741824:
                    UpdateStatus?.Invoke($"Media has {blocks} blocks of {blockSize} bytes/each. (for a total of {
                        totalSize / 1073741824d:F3} GiB)");

                    break;
                case > 1048576:
                    UpdateStatus?.Invoke($"Media has {blocks} blocks of {blockSize} bytes/each. (for a total of {
                        totalSize / 1048576d:F3} MiB)");

                    break;
                case > 1024:
                    UpdateStatus?.Invoke($"Media has {blocks} blocks of {blockSize} bytes/each. (for a total of {
                        totalSize / 1024d:F3} KiB)");

                    break;
                default:
                    UpdateStatus?.Invoke($"Media has {blocks} blocks of {blockSize} bytes/each. (for a total of {
                        totalSize} bytes)");

                    break;
            }
        }

        // Check how many blocks to read, if error show and return
        if(scsiReader.GetBlocksToRead(_maximumReadable))
        {
            _dumpLog.WriteLine("ERROR: Cannot get blocks to read: {0}.", scsiReader.ErrorMessage);
            StoppingErrorMessage?.Invoke(scsiReader.ErrorMessage);

            return;
        }

        uint blocksToRead      = scsiReader.BlocksToRead;
        uint logicalBlockSize  = blockSize;
        uint physicalBlockSize = scsiReader.PhysicalBlockSize;

        if(blocks == 0)
        {
            _dumpLog.WriteLine("ERROR: Unable to read medium or empty medium present...");
            StoppingErrorMessage?.Invoke("Unable to read medium or empty medium present...");

            return;
        }

        UpdateStatus?.Invoke($"Device reports {blocks} blocks ({blocks * blockSize} bytes).");
        UpdateStatus?.Invoke($"Device can read {blocksToRead} blocks at a time.");
        UpdateStatus?.Invoke($"Device reports {blockSize} bytes per logical block.");
        UpdateStatus?.Invoke($"Device reports {scsiReader.LongBlockSize} bytes per physical block.");
        UpdateStatus?.Invoke($"SCSI device type: {_dev.ScsiType}.");
        UpdateStatus?.Invoke($"SCSI medium type: {scsiMediumType}.");
        UpdateStatus?.Invoke($"SCSI density type: {scsiDensityCode}.");
        UpdateStatus?.Invoke($"SCSI floppy mode page present: {containsFloppyPage}.");
        UpdateStatus?.Invoke($"Media identified as {dskType}");

        _dumpLog.WriteLine("Device reports {0} blocks ({1} bytes).", blocks, blocks * blockSize);
        _dumpLog.WriteLine("Device can read {0} blocks at a time.", blocksToRead);
        _dumpLog.WriteLine("Device reports {0} bytes per logical block.", blockSize);
        _dumpLog.WriteLine("Device reports {0} bytes per physical block.", scsiReader.LongBlockSize);
        _dumpLog.WriteLine("SCSI device type: {0}.", _dev.ScsiType);
        _dumpLog.WriteLine("SCSI medium type: {0}.", scsiMediumType);
        _dumpLog.WriteLine("SCSI density type: {0}.", scsiDensityCode);
        _dumpLog.WriteLine("SCSI floppy mode page present: {0}.", containsFloppyPage);
        _dumpLog.WriteLine("Media identified as {0}.", dskType);

        uint longBlockSize = scsiReader.LongBlockSize;

        if(_dumpRaw)
            if(blockSize == longBlockSize)
            {
                ErrorMessage?.Invoke(!scsiReader.CanReadRaw
                                         ? "Device doesn't seem capable of reading raw data from media."
                                         : "Device is capable of reading raw data but I've been unable to guess correct sector size.");

                if(!_force)
                {
                    StoppingErrorMessage?.
                        Invoke("Not continuing. If you want to continue reading cooked data when raw is not available use the force option.");

                    // TODO: Exit more gracefully
                    return;
                }

                ErrorMessage?.Invoke("Continuing dumping cooked data.");
            }
            else
            {
                // Only a block will be read, but it contains 16 sectors and command expect sector number not block number
                blocksToRead = (uint)(longBlockSize == 37856 ? 16 : 1);

                UpdateStatus?.Invoke($"Reading {longBlockSize} raw bytes ({blockSize * blocksToRead
                } cooked bytes) per sector.");

                physicalBlockSize = longBlockSize;
                blockSize         = longBlockSize;
            }

        ret = true;

        foreach(MediaTagType tag in mediaTags.Keys.Where(tag => !outputFormat.SupportedMediaTags.Contains(tag)))
        {
            ret = false;
            _dumpLog.WriteLine($"Output format does not support {tag}.");
            ErrorMessage?.Invoke($"Output format does not support {tag}.");
        }

        if(!ret)
        {
            if(_force)
            {
                _dumpLog.WriteLine("Several media tags not supported, continuing...");
                ErrorMessage?.Invoke("Several media tags not supported, continuing...");
            }
            else
            {
                _dumpLog.WriteLine("Several media tags not supported, not continuing...");
                StoppingErrorMessage?.Invoke("Several media tags not supported, not continuing...");

                return;
            }
        }

        UpdateStatus?.Invoke($"Reading {blocksToRead} sectors at a time.");
        _dumpLog.WriteLine("Reading {0} sectors at a time.", blocksToRead);

        var mhddLog      = new MhddLog(_outputPrefix + ".mhddlog.bin", _dev, blocks, blockSize, blocksToRead, _private);
        var ibgLog       = new IbgLog(_outputPrefix  + ".ibg", sbcProfile);
        var imageCreated = false;

        if(!opticalDisc)
        {
            ret = outputFormat.Create(_outputPath, dskType, _formatOptions, blocks, blockSize);

            // Cannot create image
            if(!ret)
            {
                _dumpLog.WriteLine("Error creating output image, not continuing.");
                _dumpLog.WriteLine(outputFormat.ErrorMessage);

                StoppingErrorMessage?.Invoke("Error creating output image, not continuing." + Environment.NewLine +
                                             outputFormat.ErrorMessage);

                return;
            }

            imageCreated = true;
        }

        start = DateTime.UtcNow;
        double imageWriteDuration      = 0;
        var    writeSingleOpticalTrack = true;

        if(opticalDisc)
        {
            if(outputFormat is IWritableOpticalImage opticalPlugin)
            {
                sense = _dev.ReadDiscInformation(out byte[] readBuffer, out _, MmcDiscInformationDataTypes.DiscInformation,
                                                 _dev.Timeout, out _);

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
                                _dumpLog.
                                    WriteLine("Image does not support multiple sessions in non Compact Disc dumps, continuing...");

                                ErrorMessage?.
                                    Invoke("Image does not support multiple sessions in non Compact Disc dumps, continuing...");
                            }
                            else
                            {
                                _dumpLog.
                                    WriteLine("Image does not support multiple sessions in non Compact Disc dumps, not continuing...");

                                StoppingErrorMessage?.
                                    Invoke("Image does not support multiple sessions in non Compact Disc dumps, not continuing...");

                                return;
                            }
                        }

                        if((discInformation?.LastTrackLastSession - discInformation?.FirstTrackNumber > 0 ||
                            discInformation?.FirstTrackNumber                                         != 1) &&
                           !canStoreNotCdTracks)
                        {
                            if(_force)
                            {
                                _dumpLog.
                                    WriteLine("Image does not support multiple tracks in non Compact Disc dumps, continuing...");

                                ErrorMessage?.
                                    Invoke("Image does not support multiple tracks in non Compact Disc dumps, continuing...");
                            }
                            else
                            {
                                _dumpLog.
                                    WriteLine("Image does not support multiple tracks in non Compact Disc dumps, not continuing...");

                                StoppingErrorMessage?.
                                    Invoke("Image does not support multiple tracks in non Compact Disc dumps, not continuing...");

                                return;
                            }
                        }

                        UpdateStatus?.Invoke("Building track map...");
                        _dumpLog.WriteLine("Building track map...");

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
                            _dumpLog.WriteLine("Error creating output image, not continuing.");
                            _dumpLog.WriteLine(outputFormat.ErrorMessage);

                            StoppingErrorMessage?.Invoke("Error creating output image, not continuing." +
                                                         Environment.NewLine + outputFormat.ErrorMessage);

                            return;
                        }

                        imageCreated = true;

                    #if DEBUG
                        foreach(Track trk in tracks)
                            UpdateStatus?.Invoke($"Track {trk.Sequence} starts at LBA {trk.StartSector
                            } and ends at LBA {trk.EndSector}");
                    #endif

                        if(canStoreNotCdTracks)
                        {
                            ret = opticalPlugin.SetTracks(tracks);

                            if(!ret)
                            {
                                _dumpLog.WriteLine("Error sending tracks to output image, not continuing.");
                                _dumpLog.WriteLine(opticalPlugin.ErrorMessage);

                                StoppingErrorMessage?.Invoke("Error sending tracks to output image, not continuing." +
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
                _dumpLog.WriteLine("The specified plugin does not support storing optical disc images..");
                StoppingErrorMessage?.Invoke("The specified plugin does not support storing optical disc images.");

                return;
            }
        }
        else if(decMode?.Pages != null)
        {
            var setGeometry = false;

            foreach(Modes.ModePage page in decMode.Value.Pages)
                switch(page.Page)
                {
                    case 0x04 when page.Subpage == 0x00:
                    {
                        Modes.ModePage_04? rigidPage = Modes.DecodeModePage_04(page.PageResponse);

                        if(!rigidPage.HasValue || setGeometry)
                            continue;

                        _dumpLog.WriteLine("Setting geometry to {0} cylinders, {1} heads, {2} sectors per track",
                                           rigidPage.Value.Cylinders, rigidPage.Value.Heads,
                                           (uint)(blocks / (rigidPage.Value.Cylinders * rigidPage.Value.Heads)));

                        UpdateStatus?.Invoke($"Setting geometry to {rigidPage.Value.Cylinders} cylinders, {
                            rigidPage.Value.Heads} heads, {
                                (uint)(blocks / (rigidPage.Value.Cylinders * rigidPage.Value.Heads))
                            } sectors per track");

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

                        _dumpLog.WriteLine("Setting geometry to {0} cylinders, {1} heads, {2} sectors per track",
                                           flexiblePage.Value.Cylinders, flexiblePage.Value.Heads,
                                           flexiblePage.Value.SectorsPerTrack);

                        UpdateStatus?.Invoke($"Setting geometry to {flexiblePage.Value.Cylinders} cylinders, {
                            flexiblePage.Value.Heads} heads, {flexiblePage.Value.SectorsPerTrack} sectors per track");

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
                _dumpLog.WriteLine("Error creating output image, not continuing.");
                _dumpLog.WriteLine(outputFormat.ErrorMessage);

                StoppingErrorMessage?.Invoke("Error creating output image, not continuing." + Environment.NewLine +
                                             outputFormat.ErrorMessage);

                return;
            }

            if(writeSingleOpticalTrack)
            {
                _dumpLog.WriteLine("Creating single track as could not retrieve track list from drive.");

                UpdateStatus?.Invoke("Creating single track as could not retrieve track list from drive.");

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

        DumpHardwareType currentTry = null;
        ExtentsULong     extents    = null;

        ResumeSupport.Process(true, _dev.IsRemovable, blocks, _dev.Manufacturer, _dev.Model, _dev.Serial,
                              _dev.PlatformId, ref _resume, ref currentTry, ref extents, _dev.FirmwareRevision,
                              _private, _force);

        if(currentTry == null ||
           extents    == null)
        {
            StoppingErrorMessage?.Invoke("Could not process resume file, not continuing...");

            return;
        }

        if(_resume.NextBlock > 0)
        {
            UpdateStatus?.Invoke($"Resuming from block {_resume.NextBlock}.");
            _dumpLog.WriteLine("Resuming from block {0}.", _resume.NextBlock);
        }

        // Set speed
        if(_speedMultiplier >= 0)
        {
            _dumpLog.WriteLine($"Setting speed to {(_speed   == 0 ? "MAX." : $"{_speed}x")}.");
            UpdateStatus?.Invoke($"Setting speed to {(_speed == 0 ? "MAX." : $"{_speed}x")}.");

            _speed *= _speedMultiplier;

            if(_speed is 0 or > 0xFFFF)
                _speed = 0xFFFF;

            _dev.SetCdSpeed(out _, RotationalControl.ClvAndImpureCav, (ushort)_speed, 0, _dev.Timeout, out _);
        }

        if(_resume?.BlankExtents != null)
            blankExtents = ExtentsConverter.FromMetadata(_resume.BlankExtents);

        var newTrim = false;

        if(mediaTags.TryGetValue(MediaTagType.DVD_CMI, out byte[] cmi) &&
           Settings.Current.EnableDecryption                           &&
           _titleKeys                                                  &&
           dskType               == MediaType.DVDROM                   &&
           (CopyrightType)cmi[0] == CopyrightType.CSS)
        {
            UpdateStatus?.Invoke("Title keys dumping is enabled. This will be very slow.");
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

        UpdateStatus?.Invoke($"Dump finished in {(end - start).TotalSeconds} seconds.");

        UpdateStatus?.Invoke($"Average dump speed {blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000)
            :F3} KiB/sec.");

        UpdateStatus?.Invoke($"Average write speed {blockSize * (double)(blocks + 1) / 1024 / imageWriteDuration
            :F3} KiB/sec.");

        _dumpLog.WriteLine("Dump finished in {0} seconds.", (end - start).TotalSeconds);

        _dumpLog.WriteLine("Average dump speed {0:F3} KiB/sec.",
                           blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000));

        _dumpLog.WriteLine("Average write speed {0:F3} KiB/sec.",
                           blockSize * (double)(blocks + 1) / 1024 / imageWriteDuration);

        #region Trimming
        if(_resume.BadBlocks.Count > 0 &&
           !_aborted                   &&
           _trim                       &&
           newTrim)
        {
            start = DateTime.UtcNow;
            UpdateStatus?.Invoke("Trimming skipped sectors");
            _dumpLog.WriteLine("Trimming skipped sectors");

            InitProgress?.Invoke();

            TrimSbcData(scsiReader, extents, currentTry, blankExtents);

            EndProgress?.Invoke();
            end = DateTime.UtcNow;
            UpdateStatus?.Invoke($"Trimming finished in {(end - start).TotalSeconds} seconds.");
            _dumpLog.WriteLine("Trimming finished in {0} seconds.", (end - start).TotalSeconds);
        }
        #endregion Trimming

        #region Error handling
        if(_resume.BadBlocks.Count > 0 &&
           !_aborted                   &&
           _retryPasses > 0)
            RetrySbcData(scsiReader, currentTry, extents, ref totalDuration, blankExtents);

        if(_resume.MissingTitleKeys?.Count > 0 &&
           !_aborted                           &&
           _retryPasses > 0                    &&
           Settings.Current.EnableDecryption   &&
           _titleKeys                          &&
           mediaTags.ContainsKey(MediaTagType.DVD_DiscKey_Decrypted))
            RetryTitleKeys(dvdDecrypt, mediaTags[MediaTagType.DVD_DiscKey_Decrypted], ref totalDuration);
        #endregion Error handling

        if(opticalDisc)
            foreach(KeyValuePair<MediaTagType, byte[]> tag in mediaTags)
            {
                if(tag.Value is null)
                {
                    AaruConsole.ErrorWriteLine("Error: Tag type {0} is null, skipping...", tag.Key);

                    continue;
                }

                ret = outputFormat.WriteMediaTag(tag.Value, tag.Key);

                if(ret || _force)
                    continue;

                // Cannot write tag to image
                StoppingErrorMessage?.Invoke($"Cannot write tag {tag.Key}.");

                _dumpLog.WriteLine($"Cannot write tag {tag.Key}." + Environment.NewLine + outputFormat.ErrorMessage);

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
                    UpdateStatus?.Invoke("Reading USB descriptors.");
                    _dumpLog.WriteLine("Reading USB descriptors.");
                    ret = outputFormat.WriteMediaTag(_dev.UsbDescriptors, MediaTagType.USB_Descriptors);

                    if(!ret &&
                       !_force)
                    {
                        _dumpLog.WriteLine("Cannot write USB descriptors.");

                        StoppingErrorMessage?.Invoke("Cannot write USB descriptors." + Environment.NewLine +
                                                     outputFormat.ErrorMessage);

                        return;
                    }
                }

                byte[] cmdBuf;

                if(_dev.Type == DeviceType.ATAPI)
                {
                    UpdateStatus?.Invoke("Requesting ATAPI IDENTIFY PACKET DEVICE.");
                    _dumpLog.WriteLine("Requesting ATAPI IDENTIFY PACKET DEVICE.");
                    sense = _dev.AtapiIdentify(out cmdBuf, out _);

                    if(!sense)
                    {
                        if(_private)
                            cmdBuf = DeviceReport.ClearIdentify(cmdBuf);

                        ret = outputFormat.WriteMediaTag(cmdBuf, MediaTagType.ATAPI_IDENTIFY);

                        if(!ret &&
                           !_force)
                        {
                            _dumpLog.WriteLine("Cannot write ATAPI IDENTIFY PACKET DEVICE.");

                            StoppingErrorMessage?.Invoke("Cannot write ATAPI IDENTIFY PACKET DEVICE." +
                                                         Environment.NewLine + outputFormat.ErrorMessage);

                            return;
                        }
                    }
                }

                sense = _dev.ScsiInquiry(out cmdBuf, out _);

                if(!sense)
                {
                    UpdateStatus?.Invoke("Requesting SCSI INQUIRY.");
                    _dumpLog.WriteLine("Requesting SCSI INQUIRY.");
                    ret = outputFormat.WriteMediaTag(cmdBuf, MediaTagType.SCSI_INQUIRY);

                    if(!ret &&
                       !_force)
                    {
                        StoppingErrorMessage?.Invoke("Cannot write SCSI INQUIRY.");

                        _dumpLog.WriteLine("Cannot write SCSI INQUIRY." + Environment.NewLine +
                                           outputFormat.ErrorMessage);

                        return;
                    }

                    UpdateStatus?.Invoke("Requesting MODE SENSE (10).");
                    _dumpLog.WriteLine("Requesting MODE SENSE (10).");

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
                                _dumpLog.WriteLine("Cannot write SCSI MODE SENSE (10).");

                                StoppingErrorMessage?.Invoke("Cannot write SCSI MODE SENSE (10)." +
                                                             Environment.NewLine + outputFormat.ErrorMessage);

                                return;
                            }
                        }

                    UpdateStatus?.Invoke("Requesting MODE SENSE (6).");
                    _dumpLog.WriteLine("Requesting MODE SENSE (6).");

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
                                _dumpLog.WriteLine("Cannot write SCSI MODE SENSE (6).");

                                StoppingErrorMessage?.Invoke("Cannot write SCSI MODE SENSE (6)." + Environment.NewLine +
                                                             outputFormat.ErrorMessage);

                                return;
                            }
                        }
                }
            }
        }

        _resume.BadBlocks.Sort();

        foreach(ulong bad in _resume.BadBlocks)
            _dumpLog.WriteLine("Sector {0} could not be read.", bad);

        currentTry.Extents = ExtentsConverter.ToMetadata(extents);

        outputFormat.SetDumpHardware(_resume.Tries);

        // TODO: Media Serial Number
        // TODO: Non-removable drive information
        var metadata = new ImageInfo
        {
            Application        = "Aaru",
            ApplicationVersion = Version.GetVersion()
        };

        if(!outputFormat.SetMetadata(metadata))
            ErrorMessage?.Invoke("Error {0} setting metadata, continuing..." + Environment.NewLine +
                                 outputFormat.ErrorMessage);

        if(_preSidecar != null)
            outputFormat.SetCicmMetadata(_preSidecar);

        _dumpLog.WriteLine("Closing output file.");
        UpdateStatus?.Invoke("Closing output file.");
        DateTime closeStart = DateTime.Now;
        outputFormat.Close();
        DateTime closeEnd = DateTime.Now;
        UpdateStatus?.Invoke($"Closed in {(closeEnd - closeStart).TotalSeconds} seconds.");
        _dumpLog.WriteLine("Closed in {0} seconds.", (closeEnd - closeStart).TotalSeconds);

        if(_aborted)
        {
            UpdateStatus?.Invoke("Aborted!");
            _dumpLog.WriteLine("Aborted!");

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
                UpdateStatus?.Invoke("Creating sidecar.");
                _dumpLog.WriteLine("Creating sidecar.");
                var         filters     = new FiltersList();
                IFilter     filter      = filters.GetFilter(_outputPath);
                var         inputPlugin = ImageFormat.Detect(filter) as IMediaImage;
                ErrorNumber opened      = inputPlugin.Open(filter);

                if(opened != ErrorNumber.NoError)
                {
                    StoppingErrorMessage?.Invoke(string.Format("Error {0} opening created image.", opened));

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
                CICMMetadataType sidecar = _sidecarClass.Create();
                end = DateTime.UtcNow;

                if(!_aborted)
                {
                    totalChkDuration = (end - chkStart).TotalMilliseconds;
                    UpdateStatus?.Invoke($"Sidecar created in {(end - chkStart).TotalSeconds} seconds.");

                    UpdateStatus?.Invoke($"Average checksum speed {
                        blockSize * (double)(blocks + 1) / 1024 / (totalChkDuration / 1000):F3} KiB/sec.");

                    _dumpLog.WriteLine("Sidecar created in {0} seconds.", (end - chkStart).TotalSeconds);

                    _dumpLog.WriteLine("Average checksum speed {0:F3} KiB/sec.",
                                       blockSize * (double)(blocks + 1) / 1024 / (totalChkDuration / 1000));

                    if(_preSidecar != null)
                    {
                        _preSidecar.BlockMedia = sidecar.BlockMedia;
                        sidecar                = _preSidecar;
                    }

                    // All USB flash drives report as removable, even if the media is not removable
                    if(!_dev.IsRemovable ||
                       _dev.IsUsb)
                    {
                        if(_dev.IsUsb &&
                           _dev.UsbDescriptors != null)
                            if(outputFormat.SupportedMediaTags.Contains(MediaTagType.USB_Descriptors))
                                sidecar.BlockMedia[0].USB = new USBType
                                {
                                    ProductID = _dev.UsbProductId,
                                    VendorID  = _dev.UsbVendorId,
                                    Descriptors = new DumpType
                                    {
                                        Image     = _outputPath,
                                        Size      = (ulong)_dev.UsbDescriptors.Length,
                                        Checksums = Checksum.GetChecksums(_dev.UsbDescriptors).ToArray()
                                    }
                                };

                        byte[] cmdBuf;

                        if(_dev.Type == DeviceType.ATAPI)
                        {
                            sense = _dev.AtapiIdentify(out cmdBuf, out _);

                            if(!sense)
                                if(outputFormat.SupportedMediaTags.Contains(MediaTagType.ATAPI_IDENTIFY))
                                    sidecar.BlockMedia[0].ATA = new ATAType
                                    {
                                        Identify = new DumpType
                                        {
                                            Image     = _outputPath,
                                            Size      = (ulong)cmdBuf.Length,
                                            Checksums = Checksum.GetChecksums(cmdBuf).ToArray()
                                        }
                                    };
                        }

                        sense = _dev.ScsiInquiry(out cmdBuf, out _);

                        if(!sense)
                        {
                            if(outputFormat.SupportedMediaTags.Contains(MediaTagType.SCSI_INQUIRY))
                                sidecar.BlockMedia[0].SCSI = new SCSIType
                                {
                                    Inquiry = new DumpType
                                    {
                                        Image     = _outputPath,
                                        Size      = (ulong)cmdBuf.Length,
                                        Checksums = Checksum.GetChecksums(cmdBuf).ToArray()
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
                                    List<EVPDType> evpds = new List<EVPDType>();
                                    foreach(byte page in pages)
                                    {
                                        dumpLog.WriteLine("Requesting page {0:X2}h.", page);
                                        sense = dev.ScsiInquiry(out cmdBuf, out _, page);
                                        if(sense) continue;

                                        EVPDType evpd = new EVPDType
                                        {
                                            Image = $"{outputPrefix}.evpd_{page:X2}h.bin",
                                            Checksums = Checksum.GetChecksums(cmdBuf).ToArray(),
                                            Size = cmdBuf.Length
                                        };
                                        evpd.Checksums = Checksum.GetChecksums(cmdBuf).ToArray();
                                        DataFile.WriteTo("SCSI Dump", evpd.Image, cmdBuf);
                                        evpds.Add(evpd);
                                    }

                                    if(evpds.Count > 0) sidecar.BlockMedia[0].SCSI.EVPD = evpds.ToArray();
                                }
                            }
                            */

                            UpdateStatus?.Invoke("Requesting MODE SENSE (10).");
                            _dumpLog.WriteLine("Requesting MODE SENSE (10).");

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
                                        sidecar.BlockMedia[0].SCSI.ModeSense10 = new DumpType
                                        {
                                            Image     = _outputPath,
                                            Size      = (ulong)cmdBuf.Length,
                                            Checksums = Checksum.GetChecksums(cmdBuf).ToArray()
                                        };

                            UpdateStatus?.Invoke("Requesting MODE SENSE (6).");
                            _dumpLog.WriteLine("Requesting MODE SENSE (6).");

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
                                        sidecar.BlockMedia[0].SCSI.ModeSense = new DumpType
                                        {
                                            Image     = _outputPath,
                                            Size      = (ulong)cmdBuf.Length,
                                            Checksums = Checksum.GetChecksums(cmdBuf).ToArray()
                                        };
                        }
                    }

                    List<(ulong start, string type)> filesystems = new();

                    if(sidecar.BlockMedia[0].FileSystemInformation != null)
                        filesystems.AddRange(from partition in sidecar.BlockMedia[0].FileSystemInformation
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
                            UpdateStatus?.Invoke($"Found filesystem {filesystem.type} at sector {filesystem.start}");

                            _dumpLog.WriteLine("Found filesystem {0} at sector {1}", filesystem.type, filesystem.start);
                        }

                    sidecar.BlockMedia[0].Dimensions = Dimensions.DimensionsFromMediaType(dskType);

                    (string type, string subType) xmlType = CommonTypes.Metadata.MediaType.MediaTypeToString(dskType);

                    sidecar.BlockMedia[0].DiskType    = xmlType.type;
                    sidecar.BlockMedia[0].DiskSubType = xmlType.subType;

                    // TODO: Implement device firmware revision
                    if(!_dev.IsRemovable ||
                       _dev.IsUsb)
                        if(_dev.Type == DeviceType.ATAPI)
                            sidecar.BlockMedia[0].Interface = "ATAPI";
                        else if(_dev.IsUsb)
                            sidecar.BlockMedia[0].Interface = "USB";
                        else if(_dev.IsFireWire)
                            sidecar.BlockMedia[0].Interface = "FireWire";
                        else
                            sidecar.BlockMedia[0].Interface = "SCSI";

                    sidecar.BlockMedia[0].LogicalBlocks     = blocks;
                    sidecar.BlockMedia[0].PhysicalBlockSize = physicalBlockSize;
                    sidecar.BlockMedia[0].LogicalBlockSize  = logicalBlockSize;
                    sidecar.BlockMedia[0].Manufacturer      = _dev.Manufacturer;
                    sidecar.BlockMedia[0].Model             = _dev.Model;

                    if(!_private)
                        sidecar.BlockMedia[0].Serial = _dev.Serial;

                    sidecar.BlockMedia[0].Size = blocks * blockSize;

                    if(_dev.IsRemovable)
                        sidecar.BlockMedia[0].DumpHardwareArray = _resume.Tries.ToArray();

                    UpdateStatus?.Invoke("Writing metadata sidecar");

                    var xmlFs = new FileStream(_outputPrefix + ".cicm.xml", FileMode.Create);

                    var xmlSer = new XmlSerializer(typeof(CICMMetadataType));
                    xmlSer.Serialize(xmlFs, sidecar);
                    xmlFs.Close();
                }
            }
        }

        UpdateStatus?.Invoke("");

        UpdateStatus?.Invoke($"Took a total of {(end - start).TotalSeconds:F3} seconds ({totalDuration / 1000
            :F3} processing commands, {totalChkDuration / 1000:F3} checksumming, {imageWriteDuration:F3} writing, {
            (closeEnd - closeStart).TotalSeconds:F3} closing).");

        UpdateStatus?.Invoke($"Average speed: {blockSize * (double)(blocks + 1) / 1048576 / (totalDuration / 1000)
            :F3} MiB/sec.");

        if(maxSpeed > 0)
            UpdateStatus?.Invoke($"Fastest speed burst: {maxSpeed:F3} MiB/sec.");

        if(minSpeed is > 0 and < double.MaxValue)
            UpdateStatus?.Invoke($"Slowest speed burst: {minSpeed:F3} MiB/sec.");

        UpdateStatus?.Invoke($"{_resume.BadBlocks.Count} sectors could not be read.");
        UpdateStatus?.Invoke("");

        Statistics.AddMedia(dskType, true);
    }
}