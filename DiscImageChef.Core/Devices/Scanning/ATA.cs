// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using DiscImageChef.Core.Logging;
using DiscImageChef.Decoders.ATA;

namespace DiscImageChef.Core.Devices.Scanning
{
    /// <summary>
    ///     Implements scanning the media from an ATA device
    /// </summary>
    public partial class MediaScan
    {
        /// <summary>
        ///     Scans the media from an ATA device
        /// </summary>
        /// <returns>Scanning results</returns>
        ScanResults Ata()
        {
            ScanResults results = new ScanResults();
            bool        sense;
            results.Blocks = 0;
            const ushort ATA_PROFILE = 0x0001;
            const uint   TIMEOUT     = 5;

            sense = dev.AtaIdentify(out byte[] cmdBuf, out _);
            if(!sense && Identify.Decode(cmdBuf).HasValue)
            {
                // Initializate reader
                Reader ataReader = new Reader(dev, TIMEOUT, cmdBuf);
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
                const int SEEK_TIMES = 1000;

                double seekCur;

                Random rnd = new Random();

                MhddLog mhddLog;
                IbgLog  ibgLog;
                double  duration;
                if(ataReader.IsLba)
                {
                    UpdateStatus?.Invoke($"Reading {blocksToRead} sectors at a time.");

                    InitBlockMap?.Invoke(results.Blocks, blockSize, blocksToRead, ATA_PROFILE);
                    mhddLog = new MhddLog(mhddLogPath, dev, results.Blocks, blockSize, blocksToRead);
                    ibgLog  = new IbgLog(ibgLogPath, ATA_PROFILE);

                    start = DateTime.UtcNow;
                    DateTime timeSpeedStart   = DateTime.UtcNow;
                    ulong    sectorSpeedStart = 0;
                    InitProgress?.Invoke();
                    for(ulong i = 0; i < results.Blocks; i += blocksToRead)
                    {
                        if(aborted) break;

                        if(results.Blocks - i < blocksToRead) blocksToRead = (byte)(results.Blocks - i);

                        #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                        if(currentSpeed > results.MaxSpeed && currentSpeed != 0) results.MaxSpeed = currentSpeed;
                        if(currentSpeed < results.MinSpeed && currentSpeed != 0) results.MinSpeed = currentSpeed;
                        #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                        UpdateProgress?.Invoke($"Reading sector {i} of {results.Blocks} ({currentSpeed:F3} MiB/sec.)",
                                               (long)i, (long)results.Blocks);

                        bool error = ataReader.ReadBlocks(out cmdBuf, i, blocksToRead, out duration);

                        if(!error)
                        {
                            if(duration      >= 500) results.F += blocksToRead;
                            else if(duration >= 150) results.E += blocksToRead;
                            else if(duration >= 50) results.D  += blocksToRead;
                            else if(duration >= 10) results.C  += blocksToRead;
                            else if(duration >= 3) results.B   += blocksToRead;
                            else results.A                     += blocksToRead;

                            ScanTime?.Invoke(i, duration);
                            mhddLog.Write(i, duration);
                            ibgLog.Write(i, currentSpeed * 1024);
                        }
                        else
                        {
                            ScanUnreadable?.Invoke(i);
                            results.Errored += blocksToRead;
                            for(ulong b = i; b < i + blocksToRead; b++) results.UnreadableSectors.Add(b);

                            mhddLog.Write(i, duration < 500 ? 65535 : duration);

                            ibgLog.Write(i, 0);
                        }

                        sectorSpeedStart += blocksToRead;

                        double elapsed = (DateTime.UtcNow - timeSpeedStart).TotalSeconds;
                        if(elapsed < 1) continue;

                        currentSpeed = sectorSpeedStart * blockSize / (1048576 * elapsed);
                        ScanSpeed?.Invoke(i, currentSpeed                      * 1024);
                        sectorSpeedStart = 0;
                        timeSpeedStart   = DateTime.UtcNow;
                    }

                    end = DateTime.UtcNow;
                    EndProgress?.Invoke();
                    mhddLog.Close();
                    ibgLog.Close(dev, results.Blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024,
                                 blockSize * (double)(results.Blocks + 1) / 1024 /
                                 (results.ProcessingTime / 1000),
                                 devicePath);

                    InitProgress?.Invoke();
                    if(ataReader.CanSeekLba)
                        for(int i = 0; i < SEEK_TIMES; i++)
                        {
                            if(aborted) break;

                            uint seekPos = (uint)rnd.Next((int)results.Blocks);

                            PulseProgress?.Invoke($"Seeking to sector {seekPos}...\t\t");

                            ataReader.Seek(seekPos, out seekCur);

                            #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                            if(seekCur > results.SeekMax && seekCur != 0) results.SeekMax = seekCur;
                            if(seekCur < results.SeekMin && seekCur != 0) results.SeekMin = seekCur;
                            #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                            results.SeekTotal += seekCur;
                            GC.Collect();
                        }

                    EndProgress?.Invoke();
                }
                else
                {
                    InitBlockMap?.Invoke(results.Blocks, blockSize, blocksToRead, ATA_PROFILE);
                    mhddLog = new MhddLog(mhddLogPath, dev, results.Blocks, blockSize, blocksToRead);
                    ibgLog  = new IbgLog(ibgLogPath, ATA_PROFILE);

                    ulong currentBlock = 0;
                    results.Blocks = (ulong)(cylinders * heads * sectors);
                    start          = DateTime.UtcNow;
                    DateTime timeSpeedStart   = DateTime.UtcNow;
                    ulong    sectorSpeedStart = 0;
                    InitProgress?.Invoke();
                    for(ushort cy = 0; cy < cylinders; cy++)
                    {
                        for(byte hd = 0; hd < heads; hd++)
                        {
                            for(byte sc = 1; sc < sectors; sc++)
                            {
                                if(aborted) break;

                                #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                                if(currentSpeed > results.MaxSpeed && currentSpeed != 0)
                                    results.MaxSpeed = currentSpeed;
                                if(currentSpeed < results.MinSpeed && currentSpeed != 0)
                                    results.MinSpeed = currentSpeed;
                                #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                                PulseProgress
                                  ?.Invoke($"Reading cylinder {cy} head {hd} sector {sc} ({currentSpeed:F3} MiB/sec.)");

                                bool error = ataReader.ReadChs(out cmdBuf, cy, hd, sc, out duration);

                                if(!error)
                                {
                                    if(duration      >= 500) results.F += blocksToRead;
                                    else if(duration >= 150) results.E += blocksToRead;
                                    else if(duration >= 50) results.D  += blocksToRead;
                                    else if(duration >= 10) results.C  += blocksToRead;
                                    else if(duration >= 3) results.B   += blocksToRead;
                                    else results.A                     += blocksToRead;

                                    ScanTime?.Invoke(currentBlock, duration);
                                    mhddLog.Write(currentBlock, duration);
                                    ibgLog.Write(currentBlock, currentSpeed * 1024);
                                }
                                else
                                {
                                    ScanUnreadable?.Invoke(currentBlock);
                                    results.Errored += blocksToRead;
                                    results.UnreadableSectors.Add(currentBlock);
                                    mhddLog.Write(currentBlock, duration < 500 ? 65535 : duration);

                                    ibgLog.Write(currentBlock, 0);
                                }

                                sectorSpeedStart++;
                                currentBlock++;

                                double elapsed = (DateTime.UtcNow - timeSpeedStart).TotalSeconds;
                                if(elapsed < 1) continue;

                                currentSpeed = sectorSpeedStart * blockSize / (1048576 * elapsed);
                                ScanSpeed?.Invoke(currentBlock, currentSpeed           * 1024);
                                sectorSpeedStart = 0;
                                timeSpeedStart   = DateTime.UtcNow;
                            }
                        }
                    }

                    end = DateTime.UtcNow;
                    EndProgress?.Invoke();
                    mhddLog.Close();
                    ibgLog.Close(dev, results.Blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024,
                                 blockSize * (double)(results.Blocks + 1) / 1024 /
                                 (results.ProcessingTime / 1000),
                                 devicePath);

                    InitProgress?.Invoke();
                    if(ataReader.CanSeek)
                        for(int i = 0; i < SEEK_TIMES; i++)
                        {
                            if(aborted) break;

                            ushort seekCy = (ushort)rnd.Next(cylinders);
                            byte   seekHd = (byte)rnd.Next(heads);
                            byte   seekSc = (byte)rnd.Next(sectors);

                            PulseProgress
                              ?.Invoke($"\rSeeking to cylinder {seekCy}, head {seekHd}, sector {seekSc}...\t\t");

                            ataReader.SeekChs(seekCy, seekHd, seekSc, out seekCur);

                            #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                            if(seekCur > results.SeekMax && seekCur != 0) results.SeekMax = seekCur;
                            if(seekCur < results.SeekMin && seekCur != 0) results.SeekMin = seekCur;
                            #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                            results.SeekTotal += seekCur;
                            GC.Collect();
                        }

                    EndProgress?.Invoke();
                }

                results.ProcessingTime /= 1000;
                results.TotalTime      =  (end - start).TotalSeconds;
                results.AvgSpeed       =  blockSize * (double)(results.Blocks + 1) / 1048576 / results.ProcessingTime;
                results.SeekTimes      =  SEEK_TIMES;

                return results;
            }

            StoppingErrorMessage?.Invoke("Unable to communicate with ATA device.");
            return results;
        }
    }
}