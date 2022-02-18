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
//     Scans SecureDigital and MultiMediaCard devices.
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

using System;
using System.Collections.Generic;
using Aaru.Core.Logging;
using Aaru.Decoders.MMC;
using Aaru.Decoders.SecureDigital;
using CSD = Aaru.Decoders.MMC.CSD;
using DeviceType = Aaru.CommonTypes.Enums.DeviceType;

// ReSharper disable JoinDeclarationAndInitializer

namespace Aaru.Core.Devices.Scanning
{
    /// <summary>Implements scanning a SecureDigital or MultiMediaCard flash card</summary>
    public sealed partial class MediaScan
    {
        ScanResults SecureDigital()
        {
            var    results = new ScanResults();
            byte[] cmdBuf;
            bool   sense;
            results.Blocks = 0;
            const uint   timeout = 5;
            double       duration;
            const ushort sdProfile     = 0x0001;
            ushort       blocksToRead  = 128;
            uint         blockSize     = 512;
            bool         byteAddressed = true;
            bool         supportsCmd23 = false;

            switch(_dev.Type)
            {
                case DeviceType.MMC:
                {
                    sense = _dev.ReadCsd(out cmdBuf, out _, timeout, out _);

                    if(!sense)
                    {
                        CSD csd = Decoders.MMC.Decoders.DecodeCSD(cmdBuf);
                        results.Blocks = (ulong)((csd.Size + 1) * Math.Pow(2, csd.SizeMultiplier + 2));
                        blockSize      = (uint)Math.Pow(2, csd.ReadBlockLength);

                        // Found at least since MMC System Specification 3.31
                        supportsCmd23 = csd.Version >= 3;

                        if(csd.Size == 0xFFF)
                        {
                            sense = _dev.ReadExtendedCsd(out cmdBuf, out _, timeout, out _);

                            if(!sense)
                            {
                                ExtendedCSD ecsd = Decoders.MMC.Decoders.DecodeExtendedCSD(cmdBuf);
                                results.Blocks = ecsd.SectorCount;
                                blockSize      = (uint)(ecsd.SectorSize == 1 ? 4096 : 512);

                                blocksToRead = (ushort)(ecsd.OptimalReadSize * 4096 / blockSize);

                                if(blocksToRead == 0)
                                    blocksToRead = 128;

                                // Supposing it's high-capacity MMC if it has Extended CSD...
                                byteAddressed = false;
                            }
                        }
                    }

                    break;
                }

                case DeviceType.SecureDigital:
                {
                    sense = _dev.ReadCsd(out cmdBuf, out _, timeout, out _);

                    if(!sense)
                    {
                        Decoders.SecureDigital.CSD csd = Decoders.SecureDigital.Decoders.DecodeCSD(cmdBuf);

                        results.Blocks = (ulong)(csd.Structure == 0
                                                     ? (csd.Size + 1) * Math.Pow(2, csd.SizeMultiplier + 2)
                                                     : (csd.Size + 1) * 1024);

                        blockSize = (uint)Math.Pow(2, csd.ReadBlockLength);

                        // Structure >=1 for SDHC/SDXC, so that's block addressed
                        byteAddressed = csd.Structure == 0;

                        if(blockSize != 512)
                        {
                            uint ratio = blockSize / 512;
                            results.Blocks *= ratio;
                            blockSize      =  512;
                        }

                        sense = _dev.ReadScr(out cmdBuf, out _, timeout, out _);

                        if(!sense)
                            supportsCmd23 = Decoders.SecureDigital.Decoders.DecodeSCR(cmdBuf)?.CommandSupport.
                                                     HasFlag(CommandSupport.SetBlockCount) ?? false;
                    }

                    break;
                }
            }

            if(results.Blocks == 0)
            {
                StoppingErrorMessage?.Invoke("Unable to get device size.");

                return results;
            }

            if(supportsCmd23)
            {
                sense = _dev.ReadWithBlockCount(out cmdBuf, out _, 0, blockSize, 1, byteAddressed, timeout,
                                                out duration);

                if(sense || _dev.Error)
                {
                    UpdateStatus?.
                        Invoke("Environment does not support setting block count, downgrading to OS reading.");

                    supportsCmd23 = false;
                }

                // Need to restart device, otherwise is it just busy streaming data with no one listening
                sense = _dev.ReOpen();

                if(sense)
                {
                    StoppingErrorMessage?.Invoke($"Error {_dev.LastError} reopening device.");

                    return results;
                }
            }

            if(supportsCmd23)
            {
                while(true)
                {
                    sense = _dev.ReadWithBlockCount(out cmdBuf, out _, 0, blockSize, blocksToRead, byteAddressed,
                                                    timeout, out duration);

                    if(sense)
                        blocksToRead /= 2;

                    if(!sense ||
                       blocksToRead == 1)
                        break;
                }

                if(sense)
                {
                    StoppingErrorMessage?.
                        Invoke($"Device error {_dev.LastError} trying to guess ideal transfer length.");

                    return results;
                }
            }

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
            double currentSpeed = 0;
            results.MaxSpeed          = double.MinValue;
            results.MinSpeed          = double.MaxValue;
            results.UnreadableSectors = new List<ulong>();
            results.SeekMax           = double.MinValue;
            results.SeekMin           = double.MaxValue;
            results.SeekTotal         = 0;
            const int seekTimes = 100;

            var rnd = new Random();

            if(supportsCmd23 || blocksToRead == 1)
                UpdateStatus?.Invoke($"Reading {blocksToRead} sectors at a time.");
            else if(_useBufferedReads)
                UpdateStatus?.Invoke($"Reading {blocksToRead} sectors at a time using OS buffered reads.");
            else
                UpdateStatus?.Invoke($"Reading {blocksToRead} sectors using sequential single commands.");

            InitBlockMap?.Invoke(results.Blocks, blockSize, blocksToRead, sdProfile);
            var mhddLog = new MhddLog(_mhddLogPath, _dev, results.Blocks, blockSize, blocksToRead, false);
            var ibgLog  = new IbgLog(_ibgLogPath, sdProfile);

            start = DateTime.UtcNow;
            DateTime timeSpeedStart   = DateTime.UtcNow;
            ulong    sectorSpeedStart = 0;
            InitProgress?.Invoke();

            for(ulong i = 0; i < results.Blocks; i += blocksToRead)
            {
                if(_aborted)
                    break;

                if(results.Blocks - i < blocksToRead)
                    blocksToRead = (byte)(results.Blocks - i);

                if(currentSpeed > results.MaxSpeed &&
                   currentSpeed > 0)
                    results.MaxSpeed = currentSpeed;

                if(currentSpeed < results.MinSpeed &&
                   currentSpeed > 0)
                    results.MinSpeed = currentSpeed;

                UpdateProgress?.Invoke($"Reading sector {i} of {results.Blocks} ({currentSpeed:F3} MiB/sec.)", (long)i,
                                       (long)results.Blocks);

                bool error;

                if(blocksToRead == 1)
                    error = _dev.ReadSingleBlock(out cmdBuf, out _, (uint)i, blockSize, byteAddressed, timeout,
                                                 out duration);
                else if(supportsCmd23)
                    error = _dev.ReadWithBlockCount(out cmdBuf, out _, (uint)i, blockSize, blocksToRead, byteAddressed,
                                                    timeout, out duration);
                else if(_useBufferedReads)
                    error = _dev.BufferedOsRead(out cmdBuf, (long)(i * blockSize), blockSize * blocksToRead,
                                                out duration);
                else
                    error = _dev.ReadMultipleUsingSingle(out cmdBuf, out _, (uint)i, blockSize, blocksToRead,
                                                         byteAddressed, timeout, out duration);

                if(!error)
                {
                    if(duration >= 500)
                        results.F += blocksToRead;
                    else if(duration >= 150)
                        results.E += blocksToRead;
                    else if(duration >= 50)
                        results.D += blocksToRead;
                    else if(duration >= 10)
                        results.C += blocksToRead;
                    else if(duration >= 3)
                        results.B += blocksToRead;
                    else
                        results.A += blocksToRead;

                    ScanTime?.Invoke(i, duration);
                    mhddLog.Write(i, duration);
                    ibgLog.Write(i, currentSpeed * 1024);
                }
                else
                {
                    ScanUnreadable?.Invoke(i);
                    results.Errored += blocksToRead;

                    for(ulong b = i; b < i + blocksToRead; b++)
                        results.UnreadableSectors.Add(b);

                    mhddLog.Write(i, duration < 500 ? 65535 : duration);

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

            InitProgress?.Invoke();

            for(int i = 0; i < seekTimes; i++)
            {
                if(_aborted || !_seekTest)
                    break;

                uint seekPos = (uint)rnd.Next((int)results.Blocks);

                PulseProgress?.Invoke($"Seeking to sector {seekPos}...\t\t");

                _dev.ReadSingleBlock(out cmdBuf, out _, seekPos, blockSize, byteAddressed, timeout, out double seekCur);

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