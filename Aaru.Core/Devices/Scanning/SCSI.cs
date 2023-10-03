// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SCSI.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Scan media from SCSI devices.
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
using System.Threading;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Console;
using Aaru.Core.Logging;
using Aaru.Decoders.CD;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Devices;
using Humanizer;
using Humanizer.Bytes;

namespace Aaru.Core.Devices.Scanning;

/// <summary>Implements scanning the media from an SCSI device</summary>
public sealed partial class MediaScan
{
    ScanResults Scsi()
    {
        var     results = new ScanResults();
        MhddLog mhddLog;
        IbgLog  ibgLog;
        byte[]  senseBuf;
        bool    sense;
        uint    blockSize        = 0;
        ushort  currentProfile   = 0x0001;
        bool    foundReadCommand = false;
        bool    readcd           = false;

        results.Blocks = 0;

        if(_dev.IsRemovable)
        {
            sense = _dev.ScsiTestUnitReady(out senseBuf, _dev.Timeout, out _);

            if(sense)
            {
                InitProgress?.Invoke();
                DecodedSense? decSense = Sense.Decode(senseBuf);

                if(decSense.HasValue)
                    switch(decSense.Value.ASC)
                    {
                        case 0x3A:
                        {
                            int leftRetries = 5;

                            while(leftRetries > 0)
                            {
                                PulseProgress?.Invoke(Localization.Core.Waiting_for_drive_to_become_ready);
                                Thread.Sleep(2000);
                                sense = _dev.ScsiTestUnitReady(out senseBuf, _dev.Timeout, out _);

                                if(!sense)
                                    break;

                                leftRetries--;
                            }

                            if(sense)
                            {
                                StoppingErrorMessage?.Invoke(Localization.Core.Please_insert_media_in_drive);

                                return results;
                            }

                            break;
                        }
                        case 0x04 when decSense.Value.ASCQ == 0x01:
                        {
                            int leftRetries = 10;

                            while(leftRetries > 0)
                            {
                                PulseProgress?.Invoke(Localization.Core.Waiting_for_drive_to_become_ready);
                                Thread.Sleep(2000);
                                sense = _dev.ScsiTestUnitReady(out senseBuf, _dev.Timeout, out _);

                                if(!sense)
                                    break;

                                leftRetries--;
                            }

                            if(sense)
                            {
                                StoppingErrorMessage?.
                                    Invoke(string.Format(Localization.Core.Error_testing_unit_was_ready_0,
                                                         Sense.PrettifySense(senseBuf)));

                                return results;
                            }

                            break;
                        }

                        // These should be trapped by the OS but seems in some cases they're not
                        case 0x28:
                        {
                            int leftRetries = 10;

                            while(leftRetries > 0)
                            {
                                PulseProgress?.Invoke(Localization.Core.Waiting_for_drive_to_become_ready);
                                Thread.Sleep(2000);
                                sense = _dev.ScsiTestUnitReady(out senseBuf, _dev.Timeout, out _);

                                if(!sense)
                                    break;

                                leftRetries--;
                            }

                            if(sense)
                            {
                                StoppingErrorMessage?.
                                    Invoke(string.Format(Localization.Core.Error_testing_unit_was_ready_0,
                                                         Sense.PrettifySense(senseBuf)));

                                return results;
                            }

                            break;
                        }
                        default:
                            StoppingErrorMessage?.Invoke(string.Format(Localization.Core.Error_testing_unit_was_ready_0,
                                                                       Sense.PrettifySense(senseBuf)));

                            return results;
                    }
                else
                {
                    StoppingErrorMessage?.Invoke(Localization.Core.Unknown_sense_testing_unit_was_ready);

                    return results;
                }

                EndProgress?.Invoke();
            }
        }

        Reader scsiReader = null;

        switch(_dev.ScsiType)
        {
            case PeripheralDeviceTypes.DirectAccess:
            case PeripheralDeviceTypes.MultiMediaDevice:
            case PeripheralDeviceTypes.OCRWDevice:
            case PeripheralDeviceTypes.OpticalDevice:
            case PeripheralDeviceTypes.SimplifiedDevice:
            case PeripheralDeviceTypes.WriteOnceDevice:
                scsiReader       = new Reader(_dev, _dev.Timeout, null, null);
                results.Blocks   = scsiReader.GetDeviceBlocks();
                foundReadCommand = !scsiReader.FindReadCommand();

                if(!foundReadCommand &&
                   _dev.ScsiType != PeripheralDeviceTypes.MultiMediaDevice)
                {
                    StoppingErrorMessage?.Invoke(Localization.Core.Unable_to_read_medium);

                    return results;
                }

                blockSize = scsiReader.LogicalBlockSize;

                if(results.Blocks != 0 &&
                   blockSize      != 0)
                {
                    results.Blocks++;

                    UpdateStatus?.
                        Invoke(string.Format(Localization.Core.Media_has_0_blocks_of_1_bytes_each_for_a_total_of_2,
                                             results.Blocks, blockSize,
                                             ByteSize.FromBytes(results.Blocks * blockSize).ToString("0.000")));
                }

                break;
            case PeripheralDeviceTypes.SequentialAccess:
                StoppingErrorMessage?.Invoke(Localization.Core.Scanning_never_supported_in_SSC);

                return results;
        }

        if(results.Blocks == 0)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Unable_to_read_medium_or_empty_medium_present);

            return results;
        }

        bool               compactDisc = true;
        FullTOC.CDFullTOC? toc         = null;

        if(_dev.ScsiType == PeripheralDeviceTypes.MultiMediaDevice)
        {
            sense = _dev.GetConfiguration(out byte[] cmdBuf, out senseBuf, 0, MmcGetConfigurationRt.Current,
                                          _dev.Timeout, out _);

            if(!sense)
            {
                Features.SeparatedFeatures ftr = Features.Separate(cmdBuf);

                currentProfile = ftr.CurrentProfile;

                switch(ftr.CurrentProfile)
                {
                    case 0x0005:
                    case 0x0008:
                    case 0x0009:
                    case 0x000A:
                    case 0x0020:
                    case 0x0021:
                    case 0x0022: break;
                    default:
                        compactDisc = false;

                        break;
                }
            }

            if(compactDisc)
            {
                currentProfile = 0x0008;

                // We discarded all discs that falsify a TOC before requesting a real TOC
                // No TOC, no CD (or an empty one)
                bool tocSense = _dev.ReadRawToc(out cmdBuf, out senseBuf, 1, _dev.Timeout, out _);

                if(!tocSense)
                    toc = FullTOC.Decode(cmdBuf);
            }
        }
        else
            compactDisc = false;

        scsiReader.GetBlocksToRead();

        uint blocksToRead;
        results.A       = 0; // <3ms
        results.B       = 0; // >=3ms, <10ms
        results.C       = 0; // >=10ms, <50ms
        results.D       = 0; // >=50ms, <150ms
        results.E       = 0; // >=150ms, <500ms
        results.F       = 0; // >=500ms
        results.Errored = 0;
        results.ProcessingTime = 0;
        results.TotalTime      = 0;
        double currentSpeed = 0;
        results.MaxSpeed          = double.MinValue;
        results.MinSpeed          = double.MaxValue;
        results.UnreadableSectors = new List<ulong>();

        if(compactDisc)
        {
            blocksToRead = 64;

            if(toc == null)
            {
                StoppingErrorMessage?.Invoke(Localization.Core.Error_trying_to_decode_TOC);

                return results;
            }

            readcd = !_dev.ReadCd(out _, out senseBuf, 0, 2352, 1, MmcSectorTypes.AllTypes, false, false, true,
                                  MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.None,
                                  _dev.Timeout, out _);

            if(readcd)
                UpdateStatus?.Invoke(Localization.Core.Using_MMC_READ_CD_command);
            else if(!foundReadCommand)
            {
                StoppingErrorMessage?.Invoke(Localization.Core.Unable_to_read_medium);

                return results;
            }

            _scanStopwatch.Restart();

            if(readcd)
                while(true)
                {
                    sense = _dev.ReadCd(out _, out senseBuf, 0, 2352, blocksToRead, MmcSectorTypes.AllTypes, false,
                                        false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                        MmcSubchannel.None, _dev.Timeout, out _);

                    if(_dev.Error || sense)
                        blocksToRead /= 2;

                    if(!_dev.Error ||
                       blocksToRead == 1)
                        break;
                }

            if(_dev.Error)
            {
                StoppingErrorMessage?.
                    Invoke(string.Format(Localization.Core.Device_error_0_trying_to_guess_ideal_transfer_length,
                                         _dev.LastError));

                return results;
            }

            UpdateStatus?.Invoke(string.Format(Localization.Core.Reading_0_sectors_at_a_time, blocksToRead));

            InitBlockMap?.Invoke(results.Blocks, blockSize, blocksToRead, currentProfile);
            mhddLog = new MhddLog(_mhddLogPath, _dev, results.Blocks, blockSize, blocksToRead, false);
            ibgLog  = new IbgLog(_ibgLogPath, currentProfile);
            _speedStopwatch.Restart();
            ulong    sectorSpeedStart = 0;

            InitProgress?.Invoke();

            for(ulong i = 0; i < results.Blocks; i += blocksToRead)
            {
                if(_aborted)
                    break;

                double cmdDuration;

                if(results.Blocks - i < blocksToRead)
                    blocksToRead = (uint)(results.Blocks - i);

                if(currentSpeed > results.MaxSpeed &&
                   currentSpeed > 0)
                    results.MaxSpeed = currentSpeed;

                if(currentSpeed < results.MinSpeed &&
                   currentSpeed > 0)
                    results.MinSpeed = currentSpeed;

                UpdateProgress?.
                    Invoke(string.Format(Localization.Core.Reading_sector_0_of_1_2, i, results.Blocks, ByteSize.FromMegabytes(currentSpeed).Per(_oneSecond).Humanize()),
                           (long)i, (long)results.Blocks);

                if(readcd)
                    sense = _dev.ReadCd(out _, out senseBuf, (uint)i, 2352, blocksToRead, MmcSectorTypes.AllTypes,
                                        false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                        MmcSubchannel.None, _dev.Timeout, out cmdDuration);
                else
                    sense = scsiReader.ReadBlocks(out _, i, blocksToRead, out cmdDuration, out _, out _);

                results.ProcessingTime += cmdDuration;

                if(!sense)
                {
                    switch(cmdDuration)
                    {
                        case >= 500:
                            results.F += blocksToRead;

                            break;
                        case >= 150:
                            results.E += blocksToRead;

                            break;
                        case >= 50:
                            results.D += blocksToRead;

                            break;
                        case >= 10:
                            results.C += blocksToRead;

                            break;
                        case >= 3:
                            results.B += blocksToRead;

                            break;
                        default:
                            results.A += blocksToRead;

                            break;
                    }

                    ScanTime?.Invoke(i, cmdDuration);
                    mhddLog.Write(i, cmdDuration);
                    ibgLog.Write(i, currentSpeed * 1024);
                }
                else
                {
                    DecodedSense? senseDecoded = null;

                    if(readcd)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_CD_error_0,
                                                   Sense.PrettifySense(senseBuf));

                        senseDecoded = Sense.Decode(senseBuf);

                        if(senseDecoded.HasValue)

                            // TODO: This error happens when changing from track type afaik. Need to solve that more cleanly
                            // LOGICAL BLOCK ADDRESS OUT OF RANGE
                            if((senseDecoded.Value.ASC != 0x21 || senseDecoded.Value.ASCQ != 0x00) &&

                               // ILLEGAL MODE FOR THIS TRACK (requesting sectors as-is, this is a firmware misconception when audio sectors
                               // are in a track where subchannel indicates data)
                               (senseDecoded.Value.ASC != 0x64 || senseDecoded.Value.ASCQ != 0x00))
                            {
                                results.Errored += blocksToRead;

                                for(ulong b = i; b < i + blocksToRead; b++)
                                    results.UnreadableSectors.Add(b);

                                ScanUnreadable?.Invoke(i);
                                mhddLog.Write(i, cmdDuration < 500 ? 65535 : cmdDuration);

                                ibgLog.Write(i, 0);
                            }
                    }

                    if(!senseDecoded.HasValue)
                    {
                        ScanUnreadable?.Invoke(i);
                        results.Errored += blocksToRead;

                        for(ulong b = i; b < i + blocksToRead; b++)
                            results.UnreadableSectors.Add(b);

                        mhddLog.Write(i, cmdDuration < 500 ? 65535 : cmdDuration);

                        ibgLog.Write(i, 0);
                    }
                }

                sectorSpeedStart += blocksToRead;

                double elapsed = _speedStopwatch.Elapsed.TotalSeconds;

                if(elapsed <= 0)
                    continue;

                currentSpeed = sectorSpeedStart * blockSize / (1048576 * elapsed);
                ScanSpeed?.Invoke(i, currentSpeed                      * 1024);
                sectorSpeedStart = 0;
                _speedStopwatch.Restart();
            }

            _speedStopwatch.Stop();
            _scanStopwatch.Stop();
            EndProgress?.Invoke();
            mhddLog.Close();

            currentSpeed = sectorSpeedStart * blockSize / (1048576 * _speedStopwatch.Elapsed.TotalSeconds);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if(results.MaxSpeed == double.MinValue)
                results.MaxSpeed = currentSpeed;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if(results.MinSpeed == double.MaxValue)
                results.MinSpeed = currentSpeed;

            ibgLog.Close(_dev, results.Blocks, blockSize, _scanStopwatch.Elapsed.TotalSeconds, currentSpeed * 1024,
                         blockSize * (double)(results.Blocks + 1) / 1024 / (results.ProcessingTime / 1000),
                         _devicePath);
        }
        else
        {
            _scanStopwatch.Restart();

            UpdateStatus?.Invoke(string.Format(Localization.Core.Reading_0_sectors_at_a_time, scsiReader.BlocksToRead));

            InitBlockMap?.Invoke(results.Blocks, blockSize, scsiReader.BlocksToRead, currentProfile);
            mhddLog = new MhddLog(_mhddLogPath, _dev, results.Blocks, blockSize, scsiReader.BlocksToRead, false);
            ibgLog  = new IbgLog(_ibgLogPath, currentProfile);
            _speedStopwatch.Restart();
            ulong    sectorSpeedStart = 0;

            InitProgress?.Invoke();

            for(ulong i = 0; i < results.Blocks; i += scsiReader.BlocksToRead)
            {
                if(_aborted)
                    break;

                blocksToRead = scsiReader.BlocksToRead;

                if(results.Blocks - i < blocksToRead)
                    blocksToRead = (uint)(results.Blocks - i);

                if(currentSpeed > results.MaxSpeed &&
                   currentSpeed > 0)
                    results.MaxSpeed = currentSpeed;

                if(currentSpeed < results.MinSpeed &&
                   currentSpeed > 0)
                    results.MinSpeed = currentSpeed;

                UpdateProgress?.
                    Invoke(string.Format(Localization.Core.Reading_sector_0_of_1_2, i, results.Blocks, ByteSize.FromMegabytes(currentSpeed).Per(_oneSecond).Humanize()),
                           (long)i, (long)results.Blocks);

                sense = scsiReader.ReadBlocks(out _, i, blocksToRead, out double cmdDuration, out _, out _);
                results.ProcessingTime += cmdDuration;

                if(!sense &&
                   !_dev.Error)
                {
                    switch(cmdDuration)
                    {
                        case >= 500:
                            results.F += blocksToRead;

                            break;
                        case >= 150:
                            results.E += blocksToRead;

                            break;
                        case >= 50:
                            results.D += blocksToRead;

                            break;
                        case >= 10:
                            results.C += blocksToRead;

                            break;
                        case >= 3:
                            results.B += blocksToRead;

                            break;
                        default:
                            results.A += blocksToRead;

                            break;
                    }

                    ScanTime?.Invoke(i, cmdDuration);
                    mhddLog.Write(i, cmdDuration);
                    ibgLog.Write(i, currentSpeed * 1024);
                }

                // TODO: Separate errors on kind of errors.
                else
                {
                    ScanUnreadable?.Invoke(i);
                    results.Errored += blocksToRead;

                    for(ulong b = i; b < i + blocksToRead; b++)
                        results.UnreadableSectors.Add(b);

                    mhddLog.Write(i, cmdDuration < 500 ? 65535 : cmdDuration);
                    ibgLog.Write(i, 0);
                }

                sectorSpeedStart += blocksToRead;

                double elapsed = _speedStopwatch.Elapsed.TotalSeconds;

                if(elapsed <= 0)
                    continue;

                currentSpeed = sectorSpeedStart * blockSize / (1048576 * elapsed);
                ScanSpeed?.Invoke(i, currentSpeed                      * 1024);
                sectorSpeedStart = 0;
                _speedStopwatch.Restart();
            }

            _speedStopwatch.Stop();
            _scanStopwatch.Stop();
            EndProgress?.Invoke();
            mhddLog.Close();

            ibgLog.Close(_dev, results.Blocks, blockSize, _scanStopwatch.Elapsed.TotalSeconds, currentSpeed * 1024,
                         blockSize * (double)(results.Blocks + 1) / 1024 / (results.ProcessingTime / 1000),
                         _devicePath);
        }

        results.SeekMax   = double.MinValue;
        results.SeekMin   = double.MaxValue;
        results.SeekTotal = 0;
        const int seekTimes = 1000;

        var rnd = new Random();

        InitProgress?.Invoke();

        for(int i = 0; i < seekTimes; i++)
        {
            if(_aborted || !_seekTest)
                break;

            uint seekPos = (uint)rnd.Next((int)results.Blocks);

            PulseProgress?.Invoke(string.Format(Localization.Core.Seeking_to_sector_0, seekPos));

            double seekCur;

            if(scsiReader.CanSeek)
                scsiReader.Seek(seekPos, out seekCur);
            else if(readcd)
                _dev.ReadCd(out _, out _, seekPos, 2352, 1, MmcSectorTypes.AllTypes, false, false, true,
                            MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.None, _dev.Timeout,
                            out seekCur);
            else
                scsiReader.ReadBlock(out _, seekPos, out seekCur, out _, out _);

            if(seekCur > results.SeekMax &&
               seekCur > 0)
                results.SeekMax = seekCur;

            if(seekCur < results.SeekMin &&
               seekCur > 0)
                results.SeekMin = seekCur;

            results.SeekTotal += seekCur;
            GC.Collect();
        }

        EndProgress?.Invoke();

        results.ProcessingTime /= 1000;
        results.TotalTime      =  _scanStopwatch.Elapsed.TotalSeconds;
        results.AvgSpeed       =  blockSize * (double)(results.Blocks + 1) / 1048576 / results.ProcessingTime;
        results.SeekTimes      =  seekTimes;

        return results;
    }
}