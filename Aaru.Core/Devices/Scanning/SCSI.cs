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
// Copyright © 2011-2021 Natalia Portillo
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

namespace Aaru.Core.Devices.Scanning
{
    /// <summary>Implements scanning the media from an SCSI device</summary>
    public sealed partial class MediaScan
    {
        ScanResults Scsi()
        {
            var     results = new ScanResults();
            MhddLog mhddLog;
            IbgLog  ibgLog;
            byte[]  senseBuf;
            bool    sense            = false;
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
                                    PulseProgress?.Invoke("Waiting for drive to become ready");
                                    Thread.Sleep(2000);
                                    sense = _dev.ScsiTestUnitReady(out senseBuf, _dev.Timeout, out _);

                                    if(!sense)
                                        break;

                                    leftRetries--;
                                }

                                if(sense)
                                {
                                    StoppingErrorMessage?.Invoke("Please insert media in drive");

                                    return results;
                                }

                                break;
                            }
                            case 0x04 when decSense.Value.ASCQ == 0x01:
                            {
                                int leftRetries = 10;

                                while(leftRetries > 0)
                                {
                                    PulseProgress?.Invoke("Waiting for drive to become ready");
                                    Thread.Sleep(2000);
                                    sense = _dev.ScsiTestUnitReady(out senseBuf, _dev.Timeout, out _);

                                    if(!sense)
                                        break;

                                    leftRetries--;
                                }

                                if(sense)
                                {
                                    StoppingErrorMessage?.
                                        Invoke($"Error testing unit was ready:\n{Sense.PrettifySense(senseBuf)}");

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
                                    PulseProgress?.Invoke("Waiting for drive to become ready");
                                    Thread.Sleep(2000);
                                    sense = _dev.ScsiTestUnitReady(out senseBuf, _dev.Timeout, out _);

                                    if(!sense)
                                        break;

                                    leftRetries--;
                                }

                                if(sense)
                                {
                                    StoppingErrorMessage?.
                                        Invoke($"Error testing unit was ready:\n{Sense.PrettifySense(senseBuf)}");

                                    return results;
                                }

                                break;
                            }
                            default:
                                StoppingErrorMessage?.
                                    Invoke($"Error testing unit was ready:\n{Sense.PrettifySense(senseBuf)}");

                                return results;
                        }
                    else
                    {
                        StoppingErrorMessage?.Invoke("Unknown testing unit was ready.");

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
                        StoppingErrorMessage?.Invoke("Unable to read medium.");

                        return results;
                    }

                    blockSize = scsiReader.LogicalBlockSize;

                    if(results.Blocks != 0 &&
                       blockSize      != 0)
                    {
                        results.Blocks++;

                        ulong totalSize = results.Blocks * blockSize;

                        if(totalSize > 1099511627776)
                            UpdateStatus?.
                                Invoke($"Media has {results.Blocks} blocks of {blockSize} bytes/each. (for a total of {totalSize / 1099511627776d:F3} TiB)");
                        else if(totalSize > 1073741824)
                            UpdateStatus?.
                                Invoke($"Media has {results.Blocks} blocks of {blockSize} bytes/each. (for a total of {totalSize / 1073741824d:F3} GiB)");
                        else if(totalSize > 1048576)
                            UpdateStatus?.
                                Invoke($"Media has {results.Blocks} blocks of {blockSize} bytes/each. (for a total of {totalSize / 1048576d:F3} MiB)");
                        else if(totalSize > 1024)
                            UpdateStatus?.
                                Invoke($"Media has {results.Blocks} blocks of {blockSize} bytes/each. (for a total of {totalSize / 1024d:F3} KiB)");
                        else
                            UpdateStatus?.
                                Invoke($"Media has {results.Blocks} blocks of {blockSize} bytes/each. (for a total of {totalSize} bytes)");
                    }

                    break;
                case PeripheralDeviceTypes.SequentialAccess:
                    StoppingErrorMessage?.Invoke("Scanning will never be supported on SCSI Streaming Devices." +
                                                 Environment.NewLine                                           +
                                                 "It has no sense to do it, and it will put too much strain on the tape.");

                    return results;
            }

            if(results.Blocks == 0)
            {
                StoppingErrorMessage?.Invoke("Unable to read medium or empty medium present...");

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
            DateTime start;
            DateTime end;
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
                    StoppingErrorMessage?.Invoke("Error trying to decode TOC...");

                    return results;
                }

                readcd = !_dev.ReadCd(out _, out senseBuf, 0, 2352, 1, MmcSectorTypes.AllTypes, false, false, true,
                                      MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.None,
                                      _dev.Timeout, out _);

                if(readcd)
                    UpdateStatus?.Invoke("Using MMC READ CD command.");
                else if(!foundReadCommand)
                {
                    StoppingErrorMessage?.Invoke("Unable to read medium.");

                    return results;
                }

                start = DateTime.UtcNow;

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
                        Invoke($"Device error {_dev.LastError} trying to guess ideal transfer length.");

                    return results;
                }

                UpdateStatus?.Invoke($"Reading {blocksToRead} sectors at a time.");

                InitBlockMap?.Invoke(results.Blocks, blockSize, blocksToRead, currentProfile);
                mhddLog = new MhddLog(_mhddLogPath, _dev, results.Blocks, blockSize, blocksToRead, false);
                ibgLog  = new IbgLog(_ibgLogPath, currentProfile);
                DateTime timeSpeedStart   = DateTime.UtcNow;
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

                    UpdateProgress?.Invoke($"Reading sector {i} of {results.Blocks} ({currentSpeed:F3} MiB/sec.)",
                                           (long)i, (long)results.Blocks);

                    if(readcd)
                    {
                        sense = _dev.ReadCd(out _, out senseBuf, (uint)i, 2352, blocksToRead, MmcSectorTypes.AllTypes,
                                            false, false, true, MmcHeaderCodes.AllHeaders, true, true,
                                            MmcErrorField.None, MmcSubchannel.None, _dev.Timeout, out cmdDuration);
                    }
                    else
                    {
                        sense = scsiReader.ReadBlocks(out _, i, blocksToRead, out cmdDuration, out _, out _);
                    }

                    results.ProcessingTime += cmdDuration;

                    if(!sense)
                    {
                        if(cmdDuration >= 500)
                            results.F += blocksToRead;
                        else if(cmdDuration >= 150)
                            results.E += blocksToRead;
                        else if(cmdDuration >= 50)
                            results.D += blocksToRead;
                        else if(cmdDuration >= 10)
                            results.C += blocksToRead;
                        else if(cmdDuration >= 3)
                            results.B += blocksToRead;
                        else
                            results.A += blocksToRead;

                        ScanTime?.Invoke(i, cmdDuration);
                        mhddLog.Write(i, cmdDuration);
                        ibgLog.Write(i, currentSpeed * 1024);
                    }
                    else
                    {
                        DecodedSense? senseDecoded = null;

                        if(readcd)
                        {
                            AaruConsole.DebugWriteLine("Media-Scan", "READ CD error:\n{0}",
                                                       Sense.PrettifySense(senseBuf));

                            senseDecoded = Sense.Decode(senseBuf);

                            if(senseDecoded.HasValue)
                            {
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

                    double elapsed = (DateTime.UtcNow - timeSpeedStart).TotalSeconds;

                    if(elapsed <= 0)
                        continue;

                    currentSpeed = sectorSpeedStart * blockSize / (1048576 * elapsed);
                    ScanSpeed?.Invoke(i, currentSpeed                      * 1024);
                    sectorSpeedStart = 0;
                    timeSpeedStart   = DateTime.UtcNow;
                }

                end = DateTime.UtcNow;
                EndProgress?.Invoke();
                mhddLog.Close();

                currentSpeed = sectorSpeedStart * blockSize / (1048576 * (end - timeSpeedStart).TotalSeconds);

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if(results.MaxSpeed == double.MinValue)
                    results.MaxSpeed = currentSpeed;

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if(results.MinSpeed == double.MaxValue)
                    results.MinSpeed = currentSpeed;

                ibgLog.Close(_dev, results.Blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024,
                             blockSize * (double)(results.Blocks + 1) / 1024 / (results.ProcessingTime / 1000),
                             _devicePath);
            }
            else
            {
                start = DateTime.UtcNow;

                UpdateStatus?.Invoke($"Reading {scsiReader.BlocksToRead} sectors at a time.");

                InitBlockMap?.Invoke(results.Blocks, blockSize, scsiReader.BlocksToRead, currentProfile);
                mhddLog = new MhddLog(_mhddLogPath, _dev, results.Blocks, blockSize, scsiReader.BlocksToRead, false);
                ibgLog  = new IbgLog(_ibgLogPath, currentProfile);
                DateTime timeSpeedStart   = DateTime.UtcNow;
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

                    UpdateProgress?.Invoke($"Reading sector {i} of {results.Blocks} ({currentSpeed:F3} MiB/sec.)",
                                           (long)i, (long)results.Blocks);

                    sense = scsiReader.ReadBlocks(out _, i, blocksToRead, out double cmdDuration, out _, out _);
                    results.ProcessingTime += cmdDuration;

                    if(!sense &&
                       !_dev.Error)
                    {
                        if(cmdDuration >= 500)
                            results.F += blocksToRead;
                        else if(cmdDuration >= 150)
                            results.E += blocksToRead;
                        else if(cmdDuration >= 50)
                            results.D += blocksToRead;
                        else if(cmdDuration >= 10)
                            results.C += blocksToRead;
                        else if(cmdDuration >= 3)
                            results.B += blocksToRead;
                        else
                            results.A += blocksToRead;

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

                    double elapsed = (DateTime.UtcNow - timeSpeedStart).TotalSeconds;

                    if(elapsed <= 0)
                        continue;

                    currentSpeed = sectorSpeedStart * blockSize / (1048576 * elapsed);
                    ScanSpeed?.Invoke(i, currentSpeed                      * 1024);
                    sectorSpeedStart = 0;
                    timeSpeedStart   = DateTime.UtcNow;
                }

                end = DateTime.UtcNow;
                EndProgress?.Invoke();
                mhddLog.Close();

                ibgLog.Close(_dev, results.Blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024,
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

                PulseProgress?.Invoke($"Seeking to sector {seekPos}...\t\t");

                double seekCur;

                if(scsiReader.CanSeek)
                    scsiReader.Seek(seekPos, out seekCur);
                else if(readcd)
                    _dev.ReadCd(out _, out _, seekPos, 2352, 1, MmcSectorTypes.AllTypes, false, false, true,
                                MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.None,
                                _dev.Timeout, out seekCur);
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
            results.TotalTime      =  (end - start).TotalSeconds;
            results.AvgSpeed       =  blockSize * (double)(results.Blocks + 1) / 1048576 / results.ProcessingTime;
            results.SeekTimes      =  seekTimes;

            return results;
        }
    }
}