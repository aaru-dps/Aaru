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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using DiscImageChef.Console;
using DiscImageChef.Core.Logging;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Devices;

namespace DiscImageChef.Core.Devices.Scanning
{
    /// <summary>
    ///     Implements scanning the media from an ATA device
    /// </summary>
    public static class Ata
    {
        /// <summary>
        ///     Scans the media from an ATA device
        /// </summary>
        /// <param name="mhddLogPath">Path to a MHDD log file</param>
        /// <param name="ibgLogPath">Path to a IMGBurn log file</param>
        /// <param name="devicePath">Device path</param>
        /// <param name="dev">Device</param>
        /// <returns>Scanning results</returns>
        public static ScanResults Scan(string mhddLogPath, string ibgLogPath, string devicePath, Device dev)
        {
            ScanResults results = new ScanResults();
            bool        aborted;
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
                    DicConsole.ErrorWriteLine(ataReader.ErrorMessage);
                    return results;
                }

                // Check block sizes
                if(ataReader.GetBlockSize())
                {
                    DicConsole.ErrorWriteLine(ataReader.ErrorMessage);
                    return results;
                }

                uint blockSize = ataReader.LogicalBlockSize;
                // Check how many blocks to read, if error show and return
                if(ataReader.GetBlocksToRead())
                {
                    DicConsole.ErrorWriteLine(ataReader.ErrorMessage);
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

                aborted                       =  false;
                System.Console.CancelKeyPress += (sender, e) => e.Cancel = aborted = true;

                MhddLog mhddLog;
                IbgLog  ibgLog;
                double  duration;
                if(ataReader.IsLba)
                {
                    DicConsole.WriteLine("Reading {0} sectors at a time.", blocksToRead);

                    mhddLog = new MhddLog(mhddLogPath, dev, results.Blocks, blockSize, blocksToRead);
                    ibgLog  = new IbgLog(ibgLogPath, ATA_PROFILE);

                    start = DateTime.UtcNow;
                    for(ulong i = 0; i < results.Blocks; i += blocksToRead)
                    {
                        if(aborted) break;

                        if(results.Blocks - i < blocksToRead) blocksToRead = (byte)(results.Blocks - i);

                        #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                        if(currentSpeed > results.MaxSpeed && currentSpeed != 0) results.MaxSpeed = currentSpeed;
                        if(currentSpeed < results.MinSpeed && currentSpeed != 0) results.MinSpeed = currentSpeed;
                        #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                        DicConsole.Write("\rReading sector {0} of {1} ({2:F3} MiB/sec.)", i, results.Blocks,
                                         currentSpeed);

                        bool error = ataReader.ReadBlocks(out cmdBuf, i, blocksToRead, out duration);

                        if(!error)
                        {
                            if(duration      >= 500) results.F += blocksToRead;
                            else if(duration >= 150) results.E += blocksToRead;
                            else if(duration >= 50) results.D  += blocksToRead;
                            else if(duration >= 10) results.C  += blocksToRead;
                            else if(duration >= 3) results.B   += blocksToRead;
                            else results.A                     += blocksToRead;

                            mhddLog.Write(i, duration);
                            ibgLog.Write(i, currentSpeed * 1024);
                        }
                        else
                        {
                            results.Errored += blocksToRead;
                            for(ulong b = i; b < i + blocksToRead; b++) results.UnreadableSectors.Add(b);

                            mhddLog.Write(i, duration < 500 ? 65535 : duration);

                            ibgLog.Write(i, 0);
                        }

                        double newSpeed =
                            (double)blockSize * blocksToRead / 1048576 / (duration / 1000);
                        if(!double.IsInfinity(newSpeed)) currentSpeed = newSpeed;
                    }

                    end = DateTime.UtcNow;
                    DicConsole.WriteLine();
                    mhddLog.Close();
                    ibgLog.Close(dev, results.Blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024,
                                 blockSize * (double)(results.Blocks + 1) / 1024 /
                                 (results.ProcessingTime / 1000),
                                 devicePath);

                    if(ataReader.CanSeekLba)
                        for(int i = 0; i < SEEK_TIMES; i++)
                        {
                            if(aborted) break;

                            uint seekPos = (uint)rnd.Next((int)results.Blocks);

                            DicConsole.Write("\rSeeking to sector {0}...\t\t", seekPos);

                            ataReader.Seek(seekPos, out seekCur);

                            #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                            if(seekCur > results.SeekMax && seekCur != 0) results.SeekMax = seekCur;
                            if(seekCur < results.SeekMin && seekCur != 0) results.SeekMin = seekCur;
                            #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                            results.SeekTotal += seekCur;
                            GC.Collect();
                        }
                }
                else
                {
                    mhddLog = new MhddLog(mhddLogPath, dev, results.Blocks, blockSize, blocksToRead);
                    ibgLog  = new IbgLog(ibgLogPath, ATA_PROFILE);

                    ulong currentBlock = 0;
                    results.Blocks = (ulong)(cylinders * heads * sectors);
                    start          = DateTime.UtcNow;
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

                                DicConsole.Write("\rReading cylinder {0} head {1} sector {2} ({3:F3} MiB/sec.)", cy, hd,
                                                 sc, currentSpeed);

                                bool error = ataReader.ReadChs(out cmdBuf, cy, hd, sc, out duration);

                                if(!error)
                                {
                                    if(duration      >= 500) results.F += blocksToRead;
                                    else if(duration >= 150) results.E += blocksToRead;
                                    else if(duration >= 50) results.D  += blocksToRead;
                                    else if(duration >= 10) results.C  += blocksToRead;
                                    else if(duration >= 3) results.B   += blocksToRead;
                                    else results.A                     += blocksToRead;

                                    mhddLog.Write(currentBlock, duration);
                                    ibgLog.Write(currentBlock, currentSpeed * 1024);
                                }
                                else
                                {
                                    results.Errored += blocksToRead;
                                    results.UnreadableSectors.Add(currentBlock);
                                    mhddLog.Write(currentBlock, duration < 500 ? 65535 : duration);

                                    ibgLog.Write(currentBlock, 0);
                                }

                                double newSpeed =
                                    blockSize / (double)1048576 / (duration / 1000);
                                if(!double.IsInfinity(newSpeed)) currentSpeed = newSpeed;

                                currentBlock++;
                            }
                        }
                    }

                    end = DateTime.UtcNow;
                    DicConsole.WriteLine();
                    mhddLog.Close();
                    ibgLog.Close(dev, results.Blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024,
                                 blockSize * (double)(results.Blocks + 1) / 1024 /
                                 (results.ProcessingTime / 1000),
                                 devicePath);

                    if(ataReader.CanSeek)
                        for(int i = 0; i < SEEK_TIMES; i++)
                        {
                            if(aborted) break;

                            ushort seekCy = (ushort)rnd.Next(cylinders);
                            byte   seekHd = (byte)rnd.Next(heads);
                            byte   seekSc = (byte)rnd.Next(sectors);

                            DicConsole.Write("\rSeeking to cylinder {0}, head {1}, sector {2}...\t\t", seekCy, seekHd,
                                             seekSc);

                            ataReader.SeekChs(seekCy, seekHd, seekSc, out seekCur);

                            #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                            if(seekCur > results.SeekMax && seekCur != 0) results.SeekMax = seekCur;
                            if(seekCur < results.SeekMin && seekCur != 0) results.SeekMin = seekCur;
                            #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                            results.SeekTotal += seekCur;
                            GC.Collect();
                        }
                }

                DicConsole.WriteLine();

                results.ProcessingTime /= 1000;
                results.TotalTime      =  (end - start).TotalSeconds;
                results.AvgSpeed       =  blockSize * (double)(results.Blocks + 1) / 1048576 / results.ProcessingTime;
                results.SeekTimes      =  SEEK_TIMES;

                return results;
            }

            DicConsole.ErrorWriteLine("Unable to communicate with ATA device.");
            return results;
        }
    }
}