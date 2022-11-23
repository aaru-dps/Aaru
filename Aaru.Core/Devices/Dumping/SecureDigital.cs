// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SecureDigital.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps SecureDigital and MultiMediaCard flash cards.
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
// ****************************************************************************/

// ReSharper disable JoinDeclarationAndInitializer

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
using Aaru.Core.Logging;
using Aaru.Decoders.MMC;
using Aaru.Decoders.SecureDigital;
using Schemas;
using CSD = Aaru.Decoders.MMC.CSD;
using DeviceType = Aaru.CommonTypes.Enums.DeviceType;
using MediaType = Aaru.CommonTypes.MediaType;
using Version = Aaru.CommonTypes.Interop.Version;

namespace Aaru.Core.Devices.Dumping;

/// <summary>Implements dumping a MultiMediaCard or SecureDigital flash card</summary>
public partial class Dump
{
    /// <summary>Dumps a MultiMediaCard or SecureDigital flash card</summary>
    void SecureDigital()
    {
        if(_dumpRaw)
        {
            if(_force)
                ErrorMessage?.Invoke(Localization.Core.Raw_dumping_is_not_supported_in_MMC_or_SD_devices_Continuing);
            else
            {
                StoppingErrorMessage?.Invoke(Localization.Core.
                                                          Raw_dumping_is_not_supported_in_MMC_or_SD_devices_Aborting);

                return;
            }
        }

        bool         sense;
        const ushort sdProfile = 0x0001;
        const uint   timeout   = 5;
        double       duration;
        ushort       blocksToRead      = 128;
        uint         blockSize         = 512;
        ulong        blocks            = 0;
        byte[]       csd               = null;
        byte[]       ocr               = null;
        byte[]       ecsd              = null;
        byte[]       scr               = null;
        uint         physicalBlockSize = 0;
        bool         byteAddressed     = true;
        uint[]       response;
        bool         supportsCmd23 = false;
        var          outputFormat  = _outputPlugin as IWritableImage;

        Dictionary<MediaTagType, byte[]> mediaTags = new();

        switch(_dev.Type)
        {
            case DeviceType.MMC:
            {
                UpdateStatus?.Invoke(Localization.Core.Reading_CSD);
                _dumpLog.WriteLine(Localization.Core.Reading_CSD);
                sense = _dev.ReadCsd(out csd, out response, timeout, out duration);

                if(!sense)
                {
                    CSD csdDecoded = Decoders.MMC.Decoders.DecodeCSD(csd);
                    blocks    = (ulong)((csdDecoded.Size + 1) * Math.Pow(2, csdDecoded.SizeMultiplier + 2));
                    blockSize = (uint)Math.Pow(2, csdDecoded.ReadBlockLength);

                    mediaTags.Add(MediaTagType.MMC_CSD, null);

                    // Found at least since MMC System Specification 3.31
                    supportsCmd23 = csdDecoded.Version >= 3;

                    if(csdDecoded.Size == 0xFFF)
                    {
                        UpdateStatus?.Invoke(Localization.Core.Reading_Extended_CSD);
                        _dumpLog.WriteLine(Localization.Core.Reading_Extended_CSD);
                        sense = _dev.ReadExtendedCsd(out ecsd, out response, timeout, out duration);

                        if(!sense)
                        {
                            ExtendedCSD ecsdDecoded = Decoders.MMC.Decoders.DecodeExtendedCSD(ecsd);
                            blocks    = ecsdDecoded.SectorCount;
                            blockSize = (uint)(ecsdDecoded.SectorSize == 1 ? 4096 : 512);

                            physicalBlockSize = ecsdDecoded.NativeSectorSize switch
                            {
                                0 => 512,
                                1 => 4096,
                                _ => physicalBlockSize
                            };

                            blocksToRead = (ushort)(ecsdDecoded.OptimalReadSize * 4096 / blockSize);

                            if(blocksToRead == 0)
                                blocksToRead = 128;

                            // Supposing it's high-capacity MMC if it has Extended CSD...
                            byteAddressed = false;
                            mediaTags.Add(MediaTagType.MMC_ExtendedCSD, null);
                        }
                        else
                        {
                            _errorLog?.WriteLine("Read eCSD", _dev.Error, _dev.LastError, response);
                            ecsd = null;
                        }
                    }
                }
                else
                {
                    _errorLog?.WriteLine("Read CSD", _dev.Error, _dev.LastError, response);
                    csd = null;
                }

                UpdateStatus?.Invoke(Localization.Core.Reading_OCR);
                _dumpLog.WriteLine(Localization.Core.Reading_OCR);
                sense = _dev.ReadOcr(out ocr, out response, timeout, out duration);

                if(sense)
                {
                    _errorLog?.WriteLine("Read OCR", _dev.Error, _dev.LastError, response);
                    ocr = null;
                }
                else
                    mediaTags.Add(MediaTagType.MMC_OCR, null);

                break;
            }

            case DeviceType.SecureDigital:
            {
                UpdateStatus?.Invoke(Localization.Core.Reading_CSD);
                _dumpLog.WriteLine(Localization.Core.Reading_CSD);
                sense = _dev.ReadCsd(out csd, out response, timeout, out duration);

                if(!sense)
                {
                    Decoders.SecureDigital.CSD csdDecoded = Decoders.SecureDigital.Decoders.DecodeCSD(csd);

                    blocks = (ulong)(csdDecoded.Structure == 0
                                         ? (csdDecoded.Size + 1) * Math.Pow(2, csdDecoded.SizeMultiplier + 2)
                                         : (csdDecoded.Size + 1) * 1024);

                    blockSize = (uint)Math.Pow(2, csdDecoded.ReadBlockLength);

                    // Structure >=1 for SDHC/SDXC, so that's block addressed
                    byteAddressed = csdDecoded.Structure == 0;
                    mediaTags.Add(MediaTagType.SD_CSD, null);

                    physicalBlockSize = blockSize;

                    if(blockSize != 512)
                    {
                        uint ratio = blockSize / 512;
                        blocks    *= ratio;
                        blockSize =  512;
                    }
                }
                else
                {
                    _errorLog?.WriteLine("Read CSD", _dev.Error, _dev.LastError, response);
                    csd = null;
                }

                UpdateStatus?.Invoke(Localization.Core.Reading_OCR);
                _dumpLog.WriteLine(Localization.Core.Reading_OCR);
                sense = _dev.ReadSdocr(out ocr, out response, timeout, out duration);

                if(sense)
                {
                    _errorLog?.WriteLine("Read OCR", _dev.Error, _dev.LastError, response);
                    ocr = null;
                }
                else
                    mediaTags.Add(MediaTagType.SD_OCR, null);

                UpdateStatus?.Invoke(Localization.Core.Reading_SCR);
                _dumpLog.WriteLine(Localization.Core.Reading_SCR);
                sense = _dev.ReadScr(out scr, out response, timeout, out duration);

                if(sense)
                {
                    _errorLog?.WriteLine("Read SCR", _dev.Error, _dev.LastError, response);
                    scr = null;
                }
                else
                {
                    supportsCmd23 = Decoders.SecureDigital.Decoders.DecodeSCR(scr)?.CommandSupport.
                                             HasFlag(CommandSupport.SetBlockCount) ?? false;

                    mediaTags.Add(MediaTagType.SD_SCR, null);
                }

                break;
            }
        }

        UpdateStatus?.Invoke(Localization.Core.Reading_CID);
        _dumpLog.WriteLine(Localization.Core.Reading_CID);
        sense = _dev.ReadCid(out byte[] cid, out response, timeout, out duration);

        if(sense)
        {
            _errorLog?.WriteLine("Read CID", _dev.Error, _dev.LastError, response);
            cid = null;
        }
        else
            mediaTags.Add(_dev.Type == DeviceType.SecureDigital ? MediaTagType.SD_CID : MediaTagType.MMC_CID, null);

        DateTime start;
        DateTime end;
        double   totalDuration = 0;
        double   currentSpeed  = 0;
        double   maxSpeed      = double.MinValue;
        double   minSpeed      = double.MaxValue;

        if(blocks == 0)
        {
            _dumpLog.WriteLine(Localization.Core.Unable_to_get_device_size);
            StoppingErrorMessage?.Invoke(Localization.Core.Unable_to_get_device_size);

            return;
        }

        UpdateStatus?.Invoke(string.Format(Localization.Core.Device_reports_0_blocks, blocks));
        _dumpLog.WriteLine(Localization.Core.Device_reports_0_blocks, blocks);

        byte[] cmdBuf;
        bool   error;

        if(blocksToRead > _maximumReadable)
            blocksToRead = (ushort)_maximumReadable;

        if(supportsCmd23 && blocksToRead > 1)
        {
            sense = _dev.ReadWithBlockCount(out cmdBuf, out _, 0, blockSize, 1, byteAddressed, timeout, out duration);

            if(sense || _dev.Error)
                supportsCmd23 = false;

            // Need to restart device, otherwise is it just busy streaming data with no one listening
            sense = _dev.ReOpen();

            if(sense)
            {
                StoppingErrorMessage?.Invoke(string.Format(Localization.Core.Error_0_reopening_device, _dev.LastError));

                return;
            }
        }

        if(supportsCmd23 && blocksToRead > 1)
        {
            while(true)
            {
                error = _dev.ReadWithBlockCount(out cmdBuf, out _, 0, blockSize, blocksToRead, byteAddressed, timeout,
                                                out duration);

                if(error)
                    blocksToRead /= 2;

                if(!error ||
                   blocksToRead == 1)
                    break;
            }

            if(error)
            {
                _dumpLog.WriteLine(Localization.Core.ERROR_Cannot_get_blocks_to_read_device_error_0, _dev.LastError);

                StoppingErrorMessage?.
                    Invoke(string.Format(Localization.Core.Device_error_0_trying_to_guess_ideal_transfer_length,
                                         _dev.LastError));

                return;
            }
        }

        if(_useBufferedReads &&
           blocksToRead > 1  &&
           !supportsCmd23)
        {
            while(true)
            {
                error = _dev.BufferedOsRead(out cmdBuf, 0, blockSize * blocksToRead, out duration);

                if(error)
                    blocksToRead /= 2;

                if(!error ||
                   blocksToRead == 1)
                    break;

                // Device is in timeout, reopen to reset
                if(_dev.LastError == 110)
                    _dev.ReOpen();
            }

            if(error)
            {
                UpdateStatus?.Invoke(Localization.Core.DumBuffered_OS_reads_are_not_working_trying_direct_commands);
                _dumpLog.WriteLine(Localization.Core.DumBuffered_OS_reads_are_not_working_trying_direct_commands);
                blocksToRead      = 1;
                _useBufferedReads = false;
            }
        }

        if(!_useBufferedReads &&
           blocksToRead > 1   &&
           !supportsCmd23)
        {
            while(true)
            {
                error = _dev.ReadMultipleUsingSingle(out cmdBuf, out _, 0, blockSize, blocksToRead, byteAddressed,
                                                     timeout, out duration);

                if(error)
                    blocksToRead /= 2;

                // Device is in timeout, reopen to reset
                if(_dev.LastError == 110)
                    _dev.ReOpen();

                if(!error ||
                   blocksToRead == 1)
                    break;
            }

            if(error)
            {
                _dumpLog.WriteLine(Localization.Core.ERROR_Cannot_get_blocks_to_read_device_error_0, _dev.LastError);

                StoppingErrorMessage?.
                    Invoke(string.Format(Localization.Core.Device_error_0_trying_to_guess_ideal_transfer_length,
                                         _dev.LastError));

                return;
            }
        }

        if(blocksToRead == 1)
        {
            error = _dev.ReadSingleBlock(out cmdBuf, out _, 0, blockSize, byteAddressed, timeout, out duration);

            if(error)
            {
                _dumpLog.WriteLine(Localization.Core.ERROR_Could_not_read_from_device_device_error_0, _dev.LastError);

                StoppingErrorMessage?.Invoke(string.Format(Localization.Core.Device_error_0_trying_to_read_from_device,
                                                           _dev.LastError));

                return;
            }
        }

        if(supportsCmd23 || blocksToRead == 1)
        {
            UpdateStatus?.Invoke(string.Format(Localization.Core.Device_can_read_0_blocks_at_a_time, blocksToRead));
            _dumpLog.WriteLine(Localization.Core.Device_can_read_0_blocks_at_a_time, blocksToRead);
        }
        else if(_useBufferedReads)
        {
            UpdateStatus?.
                Invoke(string.Format(Localization.Core.Device_can_read_0_blocks_at_a_time_using_OS_buffered_reads,
                                     blocksToRead));

            _dumpLog.WriteLine(Localization.Core.Device_can_read_0_blocks_at_a_time_using_OS_buffered_reads,
                               blocksToRead);
        }
        else
        {
            UpdateStatus?.Invoke(string.Format(Localization.Core.Device_can_read_0_blocks_using_sequential_commands,
                                               blocksToRead));

            _dumpLog.WriteLine(Localization.Core.Device_can_read_0_blocks_using_sequential_commands, blocksToRead);
        }

        if(_skip < blocksToRead)
            _skip = blocksToRead;

        DumpHardwareType currentTry = null;
        ExtentsULong     extents    = null;

        ResumeSupport.Process(true, false, blocks, _dev.Manufacturer, _dev.Model, _dev.Serial, _dev.PlatformId,
                              ref _resume, ref currentTry, ref extents, _dev.FirmwareRevision, _private, _force);

        if(currentTry == null ||
           extents    == null)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Could_not_process_resume_file_not_continuing);

            return;
        }

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

        var mhddLog = new MhddLog(_outputPrefix + ".mhddlog.bin", _dev, blocks, blockSize, blocksToRead, _private);
        var ibgLog  = new IbgLog(_outputPrefix  + ".ibg", sdProfile);

        ret = outputFormat.Create(_outputPath,
                                  _dev.Type == DeviceType.SecureDigital ? MediaType.SecureDigital : MediaType.MMC,
                                  _formatOptions, blocks, blockSize);

        // Cannot create image
        if(!ret)
        {
            _dumpLog.WriteLine(Localization.Core.Error_creating_output_image_not_continuing);
            _dumpLog.WriteLine(outputFormat.ErrorMessage);

            StoppingErrorMessage?.Invoke(Localization.Core.Error_creating_output_image_not_continuing +
                                         Environment.NewLine + outputFormat.ErrorMessage);

            return;
        }

        if(cid != null)
        {
            switch(_dev.Type)
            {
                case DeviceType.SecureDigital when _private:
                    // Clear serial number and manufacturing date
                    cid[9]  = 0;
                    cid[10] = 0;
                    cid[11] = 0;
                    cid[12] = 0;
                    cid[13] = 0;
                    cid[14] = 0;

                    break;
                case DeviceType.MMC when _private:
                    // Clear serial number and manufacturing date
                    cid[10] = 0;
                    cid[11] = 0;
                    cid[12] = 0;
                    cid[13] = 0;
                    cid[14] = 0;

                    break;
            }

            ret =
                outputFormat.WriteMediaTag(cid,
                                           _dev.Type == DeviceType.SecureDigital ? MediaTagType.SD_CID
                                               : MediaTagType.MMC_CID);

            // Cannot write CID to image
            if(!ret &&
               !_force)
            {
                _dumpLog.WriteLine(Localization.Core.Cannot_write_CID_to_output_image);

                StoppingErrorMessage?.Invoke(Localization.Core.Cannot_write_CID_to_output_image + Environment.NewLine +
                                             outputFormat.ErrorMessage);

                return;
            }
        }

        if(csd != null)
        {
            ret =
                outputFormat.WriteMediaTag(csd,
                                           _dev.Type == DeviceType.SecureDigital ? MediaTagType.SD_CSD
                                               : MediaTagType.MMC_CSD);

            // Cannot write CSD to image
            if(!ret &&
               !_force)
            {
                _dumpLog.WriteLine(Localization.Core.Cannot_write_CSD_to_output_image);

                StoppingErrorMessage?.Invoke(Localization.Core.Cannot_write_CSD_to_output_image + Environment.NewLine +
                                             outputFormat.ErrorMessage);

                return;
            }
        }

        if(ecsd != null)
        {
            ret = outputFormat.WriteMediaTag(ecsd, MediaTagType.MMC_ExtendedCSD);

            // Cannot write Extended CSD to image
            if(!ret &&
               !_force)
            {
                _dumpLog.WriteLine(Localization.Core.Cannot_write_Extended_CSD_to_output_image);

                StoppingErrorMessage?.Invoke(Localization.Core.Cannot_write_Extended_CSD_to_output_image +
                                             Environment.NewLine + outputFormat.ErrorMessage);

                return;
            }
        }

        if(ocr != null)
        {
            ret =
                outputFormat.WriteMediaTag(ocr,
                                           _dev.Type == DeviceType.SecureDigital ? MediaTagType.SD_OCR
                                               : MediaTagType.MMC_OCR);

            // Cannot write OCR to image
            if(!ret &&
               !_force)
            {
                _dumpLog.WriteLine(Localization.Core.Cannot_write_OCR_to_output_image);

                StoppingErrorMessage?.Invoke(Localization.Core.Cannot_write_OCR_to_output_image + Environment.NewLine +
                                             outputFormat.ErrorMessage);

                return;
            }
        }

        if(scr != null)
        {
            ret = outputFormat.WriteMediaTag(scr, MediaTagType.SD_SCR);

            // Cannot write SCR to image
            if(!ret &&
               !_force)
            {
                _dumpLog.WriteLine(Localization.Core.Cannot_write_SCR_to_output_image);

                StoppingErrorMessage?.Invoke(Localization.Core.Cannot_write_SCR_to_output_image + Environment.NewLine +
                                             outputFormat.ErrorMessage);

                return;
            }
        }

        if(_resume.NextBlock > 0)
        {
            UpdateStatus?.Invoke(string.Format(Localization.Core.Resuming_from_block_0, _resume.NextBlock));
            _dumpLog.WriteLine(Localization.Core.Resuming_from_block_0, _resume.NextBlock);
        }

        start = DateTime.UtcNow;
        double   imageWriteDuration = 0;
        bool     newTrim            = false;
        DateTime timeSpeedStart     = DateTime.UtcNow;
        ulong    sectorSpeedStart   = 0;

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
                blocksToRead = (byte)(blocks - i);

            if(currentSpeed > maxSpeed &&
               currentSpeed > 0)
                maxSpeed = currentSpeed;

            if(currentSpeed < minSpeed &&
               currentSpeed > 0)
                minSpeed = currentSpeed;

            UpdateProgress?.
                Invoke(string.Format(Localization.Core.Reading_sector_0_of_1_2_MiB_sec, i, blocks, currentSpeed),
                       (long)i, (long)blocks);

            if(blocksToRead == 1)
                error = _dev.ReadSingleBlock(out cmdBuf, out _, (uint)i, blockSize, byteAddressed, timeout,
                                             out duration);
            else if(supportsCmd23)
                error = _dev.ReadWithBlockCount(out cmdBuf, out _, (uint)i, blockSize, blocksToRead, byteAddressed,
                                                timeout, out duration);
            else if(_useBufferedReads)
                error = _dev.BufferedOsRead(out cmdBuf, (long)(i * blockSize), blockSize * blocksToRead, out duration);
            else
                error = _dev.ReadMultipleUsingSingle(out cmdBuf, out _, (uint)i, blockSize, blocksToRead, byteAddressed,
                                                     timeout, out duration);

            if(!error)
            {
                mhddLog.Write(i, duration);
                ibgLog.Write(i, currentSpeed * 1024);
                DateTime writeStart = DateTime.Now;
                outputFormat.WriteSectors(cmdBuf, i, blocksToRead);
                imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                extents.Add(i, blocksToRead, true);
            }
            else
            {
                _errorLog?.WriteLine(i, _dev.Error, _dev.LastError, byteAddressed, response);

                if(i + _skip > blocks)
                    _skip = (uint)(blocks - i);

                for(ulong b = i; b < i + _skip; b++)
                    _resume.BadBlocks.Add(b);

                mhddLog.Write(i, duration < 500 ? 65535 : duration);

                ibgLog.Write(i, 0);
                DateTime writeStart = DateTime.Now;
                outputFormat.WriteSectors(new byte[blockSize * _skip], i, _skip);
                imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                _dumpLog.WriteLine(Localization.Core.Skipping_0_blocks_from_errored_block_1, _skip, i);
                i       += _skip - blocksToRead;
                newTrim =  true;
            }

            sectorSpeedStart  += blocksToRead;
            _resume.NextBlock =  i + blocksToRead;

            double elapsed = (DateTime.UtcNow - timeSpeedStart).TotalSeconds;

            if(elapsed <= 0)
                continue;

            currentSpeed     = sectorSpeedStart * blockSize / (1048576 * elapsed);
            sectorSpeedStart = 0;
            timeSpeedStart   = DateTime.UtcNow;
        }

        _resume.BadBlocks = _resume.BadBlocks.Distinct().ToList();

        end = DateTime.Now;
        EndProgress?.Invoke();
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

                error = _dev.ReadSingleBlock(out cmdBuf, out response, (uint)badSector, blockSize, byteAddressed,
                                             timeout, out duration);

                totalDuration += duration;

                if(error)
                {
                    _errorLog?.WriteLine(badSector, _dev.Error, _dev.LastError, byteAddressed, response);

                    continue;
                }

                _resume.BadBlocks.Remove(badSector);
                extents.Add(badSector);
                outputFormat.WriteSector(cmdBuf, badSector);
            }

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
        {
            int  pass    = 1;
            bool forward = true;

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

                PulseProgress?.Invoke(forward
                                          ? string.Format(Localization.Core.Retrying_sector_0_pass_1_forward, badSector,
                                                          pass)
                                          : string.Format(Localization.Core.Retrying_sector_0_pass_1_reverse, badSector,
                                                          pass));

                error = _dev.ReadSingleBlock(out cmdBuf, out response, (uint)badSector, blockSize, byteAddressed,
                                             timeout, out duration);

                totalDuration += duration;

                if(error)
                {
                    _errorLog?.WriteLine(badSector, _dev.Error, _dev.LastError, byteAddressed, response);

                    continue;
                }

                _resume.BadBlocks.Remove(badSector);
                extents.Add(badSector);
                outputFormat.WriteSector(cmdBuf, badSector);

                UpdateStatus?.Invoke(string.Format(Localization.Core.Correctly_retried_block_0_in_pass_1, badSector,
                                                   pass));

                _dumpLog.WriteLine(Localization.Core.Correctly_retried_block_0_in_pass_1, badSector, pass);
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

                goto repeatRetryLba;
            }

            EndProgress?.Invoke();
        }
        #endregion Error handling

        currentTry.Extents = ExtentsConverter.ToMetadata(extents);

        outputFormat.SetDumpHardware(_resume.Tries);

        // TODO: Drive info
        var metadata = new CommonTypes.Structs.ImageInfo
        {
            Application        = "Aaru",
            ApplicationVersion = Version.GetVersion()
        };

        if(!outputFormat.SetMetadata(metadata))
            ErrorMessage?.Invoke(Localization.Core.Error_0_setting_metadata + Environment.NewLine +
                                 outputFormat.ErrorMessage);

        if(_preSidecar != null)
            outputFormat.SetCicmMetadata(_preSidecar);

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
            UpdateStatus?.Invoke(Localization.Core.Creating_sidecar);
            _dumpLog.WriteLine(Localization.Core.Creating_sidecar);
            var         filters     = new FiltersList();
            IFilter     filter      = filters.GetFilter(_outputPath);
            var         inputPlugin = ImageFormat.Detect(filter) as IMediaImage;
            ErrorNumber opened      = inputPlugin.Open(filter);

            if(opened != ErrorNumber.NoError)
                StoppingErrorMessage?.Invoke(string.Format(Localization.Core.Error_0_opening_created_image, opened));

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

            if(!_aborted)
            {
                if(_preSidecar != null)
                {
                    _preSidecar.BlockMedia = sidecar.BlockMedia;
                    sidecar                = _preSidecar;
                }

                end = DateTime.UtcNow;

                totalChkDuration = (end - chkStart).TotalMilliseconds;

                UpdateStatus?.Invoke(string.Format(Localization.Core.Sidecar_created_in_0_seconds,
                                                   (end - chkStart).TotalSeconds));

                UpdateStatus?.Invoke(string.Format(Localization.Core.Average_checksum_speed_0_KiB_sec,
                                                   blockSize * (double)(blocks + 1) / 1024 /
                                                   (totalChkDuration / 1000)));

                _dumpLog.WriteLine(Localization.Core.Sidecar_created_in_0_seconds, (end - chkStart).TotalSeconds);

                _dumpLog.WriteLine(Localization.Core.Average_checksum_speed_0_KiB_sec,
                                   blockSize * (double)(blocks + 1) / 1024 / (totalChkDuration / 1000));

                (string type, string subType) xmlType = (null, null);

                switch(_dev.Type)
                {
                    case DeviceType.MMC:
                        xmlType = CommonTypes.Metadata.MediaType.MediaTypeToString(MediaType.MMC);

                        sidecar.BlockMedia[0].Dimensions = Dimensions.DimensionsFromMediaType(MediaType.MMC);

                        break;
                    case DeviceType.SecureDigital:
                        CommonTypes.Metadata.MediaType.MediaTypeToString(MediaType.SecureDigital);

                        sidecar.BlockMedia[0].Dimensions = Dimensions.DimensionsFromMediaType(MediaType.SecureDigital);

                        break;
                }

                sidecar.BlockMedia[0].DiskType    = xmlType.type;
                sidecar.BlockMedia[0].DiskSubType = xmlType.subType;

                // TODO: Implement device firmware revision
                sidecar.BlockMedia[0].LogicalBlocks     = blocks;
                sidecar.BlockMedia[0].PhysicalBlockSize = physicalBlockSize > 0 ? physicalBlockSize : blockSize;
                sidecar.BlockMedia[0].LogicalBlockSize  = blockSize;
                sidecar.BlockMedia[0].Manufacturer      = _dev.Manufacturer;
                sidecar.BlockMedia[0].Model             = _dev.Model;

                if(!_private)
                    sidecar.BlockMedia[0].Serial = _dev.Serial;

                sidecar.BlockMedia[0].Size = blocks * blockSize;

                UpdateStatus?.Invoke(Localization.Core.Writing_metadata_sidecar);

                var xmlFs = new FileStream(_outputPrefix + ".cicm.xml", FileMode.Create);

                var xmlSer = new XmlSerializer(typeof(CICMMetadataType));
                xmlSer.Serialize(xmlFs, sidecar);
                xmlFs.Close();
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

        if(_resume.BadBlocks.Count > 0)
            _resume.BadBlocks.Sort();

        switch(_dev.Type)
        {
            case DeviceType.MMC:
                Statistics.AddMedia(MediaType.MMC, true);

                break;
            case DeviceType.SecureDigital:
                Statistics.AddMedia(MediaType.SecureDigital, true);

                break;
        }
    }
}