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
//     Scans media from ATA devices.
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
using Aaru.CommonTypes.Structs.Devices.ATA;
using Aaru.Core.Logging;
using Humanizer;

namespace Aaru.Core.Devices.Scanning;

/// <summary>Implements scanning the media from an ATA device</summary>
public sealed partial class MediaScan
{
    /// <summary>Scans the media from an ATA device</summary>
    /// <returns>Scanning results</returns>
    ScanResults Ata()
    {
        var results = new ScanResults
        {
            Blocks = 0
        };

        const ushort ataProfile = 0x0001;
        const uint   timeout    = 5;

        bool sense = _dev.AtaIdentify(out byte[] cmdBuf, out _);

        if(sense || !Identify.Decode(cmdBuf).HasValue)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Unable_to_communicate_with_ATA_device);

            return results;
        }

        // Initialize reader
        var ataReader = new Reader(_dev, timeout, cmdBuf, null);

        // Fill reader blocks
        results.Blocks = ataReader.GetDeviceBlocks();

        if(ataReader.FindReadCommand())
        {
            StoppingErrorMessage?.Invoke(ataReader.ErrorMessage);

            return results;
        }

        // Check block sizes
        if(ataReader.GetBlockSize())
        {
            StoppingErrorMessage?.Invoke(ataReader.ErrorMessage);

            return results;
        }

        uint blockSize = ataReader.LogicalBlockSize;

        // Check how many blocks to read, if error show and return
        if(ataReader.GetBlocksToRead())
        {
            StoppingErrorMessage?.Invoke(ataReader.ErrorMessage);

            return results;
        }

        uint   blocksToRead = ataReader.BlocksToRead;
        ushort cylinders    = ataReader.Cylinders;
        byte   heads        = ataReader.Heads;
        byte   sectors      = ataReader.Sectors;

        results.A              = 0; // <3ms
        results.B              = 0; // >=3ms, <10ms
        results.C              = 0; // >=10ms, <50ms
        results.D              = 0; // >=50ms, <150ms
        results.E              = 0; // >=150ms, <500ms
        results.F              = 0; // >=500ms
        results.Errored        = 0;
        results.ProcessingTime = 0;
        double currentSpeed = 0;
        results.MaxSpeed          = double.MinValue;
        results.MinSpeed          = double.MaxValue;
        results.UnreadableSectors = [];
        results.SeekMax           = double.MinValue;
        results.SeekMin           = double.MaxValue;
        results.SeekTotal         = 0;
        const int seekTimes = 1000;

        double seekCur;

        var rnd = new Random();

        MhddLog mhddLog;
        IbgLog  ibgLog;

        if(ataReader.IsLba)
        {
            UpdateStatus?.Invoke(string.Format(Localization.Core.Reading_0_sectors_at_a_time, blocksToRead));

            InitBlockMap?.Invoke(results.Blocks, blockSize, blocksToRead, ataProfile);
            mhddLog = new MhddLog(_mhddLogPath, _dev, results.Blocks, blockSize, blocksToRead, false);
            ibgLog  = new IbgLog(_ibgLogPath, ataProfile);

            InitProgress?.Invoke();
            uint   accumulatedSpeedSectors = 0;
            double accumulatedSpeedMs      = 0;

            for(ulong i = 0; i < results.Blocks; i += blocksToRead)
            {
                if(_aborted) break;

                if(results.Blocks - i < blocksToRead) blocksToRead = (byte)(results.Blocks - i);

                if(currentSpeed > results.MaxSpeed && currentSpeed > 0) results.MaxSpeed = currentSpeed;

                if(currentSpeed < results.MinSpeed && currentSpeed > 0) results.MinSpeed = currentSpeed;

                UpdateProgress?.Invoke(string.Format(Localization.Core.Reading_sector_0_of_1_2,
                                                     i,
                                                     results.Blocks,
                                                     ByteSize.FromMegabytes(currentSpeed).Per(_oneSecond).Humanize()),
                                       (long)i,
                                       (long)results.Blocks);

                _speedStopwatch.Restart();
                bool error = ataReader.ReadBlocks(out cmdBuf, i, blocksToRead, out double _, out _, out _);
                _speedStopwatch.Stop();
                accumulatedSpeedMs      += _speedStopwatch.Elapsed.TotalMilliseconds;
                accumulatedSpeedSectors += blocksToRead;
                results.ProcessingTime  += _speedStopwatch.Elapsed.TotalMilliseconds;

                if(!error)
                {
                    switch(_speedStopwatch.Elapsed.TotalMilliseconds)
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

                    ScanTime?.Invoke(i, _speedStopwatch.Elapsed.TotalMilliseconds);
                    mhddLog.Write(i, _speedStopwatch.Elapsed.TotalMilliseconds);
                    ibgLog.Write(i, currentSpeed * 1024);
                }
                else
                {
                    ScanUnreadable?.Invoke(i);
                    results.Errored += blocksToRead;

                    for(ulong b = i; b < i + blocksToRead; b++) results.UnreadableSectors.Add(b);

                    mhddLog.Write(i,
                                  _speedStopwatch.Elapsed.TotalMilliseconds < 500
                                      ? 65535
                                      : _speedStopwatch.Elapsed.TotalMilliseconds);

                    ibgLog.Write(i, 0);
                }

                if(accumulatedSpeedMs < 100) continue;

                currentSpeed = accumulatedSpeedSectors * blockSize / (1048576 * (accumulatedSpeedMs / 1000.0));
                ScanSpeed?.Invoke(i, currentSpeed                             * 1024);
                accumulatedSpeedMs      = 0;
                accumulatedSpeedSectors = 0;
            }

            _speedStopwatch.Stop();
            _scanStopwatch.Stop();
            EndProgress?.Invoke();
            mhddLog.Close();

            currentSpeed = accumulatedSpeedSectors * blockSize / (1048576 * (accumulatedSpeedMs / 1000.0));

            ibgLog.Close(_dev,
                         results.Blocks,
                         blockSize,
                         _scanStopwatch.Elapsed.TotalSeconds,
                         currentSpeed                             * 1024,
                         blockSize * (double)(results.Blocks + 1) / 1024 / (results.ProcessingTime / 1000),
                         _devicePath);

            InitProgress?.Invoke();

            if(ataReader.CanSeekLba && _seekTest)
            {
                for(var i = 0; i < seekTimes; i++)
                {
                    if(_aborted) break;

                    var seekPos = (uint)rnd.Next((int)results.Blocks);

                    PulseProgress?.Invoke(string.Format(Localization.Core.Seeking_to_sector_0, seekPos));

                    ataReader.Seek(seekPos, out seekCur);

                    if(seekCur > results.SeekMax && seekCur > 0) results.SeekMax = seekCur;

                    if(seekCur < results.SeekMin && seekCur > 0) results.SeekMin = seekCur;

                    results.SeekTotal += seekCur;
                    GC.Collect();
                }
            }

            EndProgress?.Invoke();
        }
        else
        {
            InitBlockMap?.Invoke(results.Blocks, blockSize, blocksToRead, ataProfile);
            mhddLog = new MhddLog(_mhddLogPath, _dev, results.Blocks, blockSize, blocksToRead, false);
            ibgLog  = new IbgLog(_ibgLogPath, ataProfile);

            ulong currentBlock = 0;
            results.Blocks = (ulong)(cylinders * heads * sectors);
            InitProgress?.Invoke();
            uint   accumulatedSpeedSectors = 0;
            double accumulatedSpeedMs      = 0;

            for(ushort cy = 0; cy < cylinders; cy++)
            {
                for(byte hd = 0; hd < heads; hd++)
                {
                    for(byte sc = 1; sc <= sectors; sc++)
                    {
                        if(_aborted) break;

                        if(currentSpeed > results.MaxSpeed && currentSpeed > 0) results.MaxSpeed = currentSpeed;

                        if(currentSpeed < results.MinSpeed && currentSpeed > 0) results.MinSpeed = currentSpeed;

                        PulseProgress?.Invoke(string.Format(Localization.Core.Reading_cylinder_0_head_1_sector_2_3,
                                                            cy,
                                                            hd,
                                                            sc,
                                                            ByteSize.FromMegabytes(currentSpeed)
                                                                    .Per(_oneSecond)
                                                                    .Humanize()));

                        _speedStopwatch.Restart();
                        bool error = ataReader.ReadChs(out cmdBuf, cy, hd, sc, out double _, out _);
                        accumulatedSpeedMs += _speedStopwatch.Elapsed.TotalMilliseconds;
                        accumulatedSpeedSectors++;
                        results.ProcessingTime += _speedStopwatch.Elapsed.TotalMilliseconds;

                        if(!error)
                        {
                            switch(_speedStopwatch.Elapsed.TotalMilliseconds)
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

                            ScanTime?.Invoke(currentBlock, _speedStopwatch.Elapsed.TotalMilliseconds);
                            mhddLog.Write(currentBlock, _speedStopwatch.Elapsed.TotalMilliseconds);
                            ibgLog.Write(currentBlock, currentSpeed * 1024);
                        }
                        else
                        {
                            ScanUnreadable?.Invoke(currentBlock);
                            results.Errored++;
                            results.UnreadableSectors.Add(currentBlock);

                            mhddLog.Write(currentBlock,
                                          _speedStopwatch.Elapsed.TotalMilliseconds < 500
                                              ? 65535
                                              : _speedStopwatch.Elapsed.TotalMilliseconds);

                            ibgLog.Write(currentBlock, 0);
                        }

                        if(accumulatedSpeedMs < 100) continue;

                        currentSpeed = accumulatedSpeedSectors * blockSize / (1048576 * (accumulatedSpeedMs / 1000.0));

                        ScanSpeed?.Invoke(currentBlock, currentSpeed * 1024);
                        accumulatedSpeedMs      = 0;
                        accumulatedSpeedSectors = 0;
                    }
                }
            }

            _speedStopwatch.Stop();
            _scanStopwatch.Stop();
            EndProgress?.Invoke();
            mhddLog.Close();

            currentSpeed = accumulatedSpeedSectors * blockSize / (1048576 * (accumulatedSpeedMs / 1000.0));

            ibgLog.Close(_dev,
                         results.Blocks,
                         blockSize,
                         _scanStopwatch.Elapsed.TotalSeconds,
                         currentSpeed                             * 1024,
                         blockSize * (double)(results.Blocks + 1) / 1024 / (results.ProcessingTime / 1000),
                         _devicePath);

            InitProgress?.Invoke();

            if(ataReader.CanSeek)
            {
                for(var i = 0; i < seekTimes; i++)
                {
                    if(_aborted) break;

                    var seekCy = (ushort)rnd.Next(cylinders);
                    var seekHd = (byte)rnd.Next(heads);
                    var seekSc = (byte)rnd.Next(sectors);

                    PulseProgress?.Invoke(string.Format(Localization.Core.Seeking_to_cylinder_0_head_1_sector_2,
                                                        seekCy,
                                                        seekHd,
                                                        seekSc));

                    ataReader.SeekChs(seekCy, seekHd, seekSc, out seekCur);

                    if(seekCur > results.SeekMax && seekCur > 0) results.SeekMax = seekCur;

                    if(seekCur < results.SeekMin && seekCur > 0) results.SeekMin = seekCur;

                    results.SeekTotal += seekCur;
                    GC.Collect();
                }
            }

            EndProgress?.Invoke();
        }

        results.ProcessingTime /= 1000;
        results.TotalTime      =  _scanStopwatch.Elapsed.TotalSeconds;
        results.AvgSpeed       =  blockSize * (double)(results.Blocks + 1) / 1048576 / results.ProcessingTime;
        results.SeekTimes      =  seekTimes;

        return results;
    }
}