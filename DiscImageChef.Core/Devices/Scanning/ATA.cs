// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ATA.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
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
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using System.Collections.Generic;
using DiscImageChef.Console;
using DiscImageChef.Core.Logging;
using DiscImageChef.Devices;

namespace DiscImageChef.Core.Devices.Scanning
{
    public static class ATA
    {
        public static ScanResults Scan(string MHDDLogPath, string IBGLogPath, string devicePath, Device dev)
        {
            ScanResults results = new ScanResults();
            bool aborted;
            MHDDLog mhddLog;
            IBGLog ibgLog;
            byte[] cmdBuf;
            bool sense;
            results.blocks = 0;
            ushort currentProfile = 0x0001;
            Decoders.ATA.AtaErrorRegistersCHS errorChs;
            bool lbaMode = false;
            uint timeout = 5;
            double duration = 0;

            sense = dev.AtaIdentify(out cmdBuf, out errorChs);
            if(!sense && Decoders.ATA.Identify.Decode(cmdBuf).HasValue)
            {
                Decoders.ATA.Identify.IdentifyDevice ataId = Decoders.ATA.Identify.Decode(cmdBuf).Value;

                // Initializate reader
                Reader ataReader = new Reader(dev, timeout, cmdBuf);
                // Fill reader blocks
                results.blocks = ataReader.GetDeviceBlocks();
                // Check block sizes
                if(ataReader.GetBlockSize())
                {
                    DicConsole.ErrorWriteLine(ataReader.ErrorMessage);
                    return results;
                }
                uint blockSize = ataReader.LogicalBlockSize;
                // Check how many blocks to read, if error show and return
                if(ataReader.GetBlocksToRead(254))
                {
                    DicConsole.ErrorWriteLine(ataReader.ErrorMessage);
                    return results;
                }
                uint blocksToRead = ataReader.BlocksToRead;
                ushort cylinders = ataReader.Cylinders;
                byte heads = ataReader.Heads;
                byte sectors = ataReader.Sectors;

                results.A = 0; // <3ms
                results.B = 0; // >=3ms, <10ms
                results.C = 0; // >=10ms, <50ms
                results.D = 0; // >=50ms, <150ms
                results.E = 0; // >=150ms, <500ms
                results.F = 0; // >=500ms
                results.errored = 0;
                DateTime start;
                DateTime end;
                results.processingTime = 0;
                double currentSpeed = 0;
                results.maxSpeed = double.MinValue;
                results.minSpeed = double.MaxValue;
                results.unreadableSectors = new List<ulong>();
                results.seekMax = double.MinValue;
                results.seekMin = double.MaxValue;
                results.seekTotal = 0;
                const int seekTimes = 1000;

                double seekCur = 0;

                Random rnd = new Random();

                uint seekPos = (uint)rnd.Next((int)results.blocks);
                ushort seekCy = (ushort)rnd.Next(cylinders);
                byte seekHd = (byte)rnd.Next(heads);
                byte seekSc = (byte)rnd.Next(sectors);

                aborted = false;
                System.Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = aborted = true;
                };

                if(lbaMode)
                {
                    DicConsole.WriteLine("Reading {0} sectors at a time.", blocksToRead);

                    mhddLog = new MHDDLog(MHDDLogPath, dev, results.blocks, blockSize, blocksToRead);
                    ibgLog = new IBGLog(IBGLogPath, currentProfile);

                    start = DateTime.UtcNow;
                    for(ulong i = 0; i < results.blocks; i += blocksToRead)
                    {
                        if(aborted)
                            break;

                        if((results.blocks - i) < blocksToRead)
                            blocksToRead = (byte)(results.blocks - i);

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                        if(currentSpeed > results.maxSpeed && currentSpeed != 0)
                            results.maxSpeed = currentSpeed;
                        if(currentSpeed < results.minSpeed && currentSpeed != 0)
                            results.minSpeed = currentSpeed;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                        DicConsole.Write("\rReading sector {0} of {1} ({2:F3} MiB/sec.)", i, results.blocks, currentSpeed);

                        bool error = ataReader.ReadBlocks(out cmdBuf, i, blocksToRead, out duration);

                        if(!error)
                        {
                            if(duration >= 500)
                            {
                                results.F += blocksToRead;
                            }
                            else if(duration >= 150)
                            {
                                results.E += blocksToRead;
                            }
                            else if(duration >= 50)
                            {
                                results.D += blocksToRead;
                            }
                            else if(duration >= 10)
                            {
                                results.C += blocksToRead;
                            }
                            else if(duration >= 3)
                            {
                                results.B += blocksToRead;
                            }
                            else
                            {
                                results.A += blocksToRead;
                            }

                            mhddLog.Write(i, duration);
                            ibgLog.Write(i, currentSpeed * 1024);
                        }
                        else
                        {
                            results.errored += blocksToRead;
                            results.unreadableSectors.Add(i);
                            if(duration < 500)
                                mhddLog.Write(i, 65535);
                            else
                                mhddLog.Write(i, duration);

                            ibgLog.Write(i, 0);
                        }

#pragma warning disable IDE0004 // Without this specific cast, it gives incorrect values
                        currentSpeed = ((double)blockSize * blocksToRead / (double)1048576) / (duration / (double)1000);
#pragma warning restore IDE0004 // Without this specific cast, it gives incorrect values
                        GC.Collect();
                    }
                    end = DateTime.UtcNow;
                    DicConsole.WriteLine();
                    mhddLog.Close();
#pragma warning disable IDE0004 // Without this specific cast, it gives incorrect values
                    ibgLog.Close(dev, results.blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024, (((double)blockSize * (double)(results.blocks + 1)) / 1024) / (results.processingTime / 1000), devicePath);
#pragma warning restore IDE0004 // Without this specific cast, it gives incorrect values

                    if(ataReader.CanSeekLBA)
                    {
                        for(int i = 0; i < seekTimes; i++)
                        {
                            if(aborted)
                                break;

                            seekPos = (uint)rnd.Next((int)results.blocks);

                            DicConsole.Write("\rSeeking to sector {0}...\t\t", seekPos);

                            ataReader.Seek(seekPos, out seekCur);

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                            if(seekCur > results.seekMax && seekCur != 0)
                                results.seekMax = seekCur;
                            if(seekCur < results.seekMin && seekCur != 0)
                                results.seekMin = seekCur;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                            results.seekTotal += seekCur;
                            GC.Collect();
                        }
                    }
                }
                else
                {
                    mhddLog = new MHDDLog(MHDDLogPath, dev, results.blocks, blockSize, blocksToRead);
                    ibgLog = new IBGLog(IBGLogPath, currentProfile);

                    ulong currentBlock = 0;
                    results.blocks = (ulong)(cylinders * heads * sectors);
                    start = DateTime.UtcNow;
                    for(ushort Cy = 0; Cy < cylinders; Cy++)
                    {
                        for(byte Hd = 0; Hd < heads; Hd++)
                        {
                            for(byte Sc = 1; Sc < sectors; Sc++)
                            {
                                if(aborted)
                                    break;

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                                if(currentSpeed > results.maxSpeed && currentSpeed != 0)
                                    results.maxSpeed = currentSpeed;
                                if(currentSpeed < results.minSpeed && currentSpeed != 0)
                                    results.minSpeed = currentSpeed;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                                DicConsole.Write("\rReading cylinder {0} head {1} sector {2} ({3:F3} MiB/sec.)", Cy, Hd, Sc, currentSpeed);

                                bool error = ataReader.ReadCHS(out cmdBuf, Cy, Hd, Sc, out duration);

                                if(!error)
                                {
                                    if(duration >= 500)
                                    {
                                        results.F += blocksToRead;
                                    }
                                    else if(duration >= 150)
                                    {
                                        results.E += blocksToRead;
                                    }
                                    else if(duration >= 50)
                                    {
                                        results.D += blocksToRead;
                                    }
                                    else if(duration >= 10)
                                    {
                                        results.C += blocksToRead;
                                    }
                                    else if(duration >= 3)
                                    {
                                        results.B += blocksToRead;
                                    }
                                    else
                                    {
                                        results.A += blocksToRead;
                                    }

                                    mhddLog.Write(currentBlock, duration);
                                    ibgLog.Write(currentBlock, currentSpeed * 1024);
                                }
                                else
                                {
                                    results.errored += blocksToRead;
                                    results.unreadableSectors.Add(currentBlock);
                                    if(duration < 500)
                                        mhddLog.Write(currentBlock, 65535);
                                    else
                                        mhddLog.Write(currentBlock, duration);

                                    ibgLog.Write(currentBlock, 0);
                                }

#pragma warning disable IDE0004 // Without this specific cast, it gives incorrect values
                                currentSpeed = ((double)blockSize / (double)1048576) / (duration / (double)1000);
#pragma warning restore IDE0004 // Without this specific cast, it gives incorrect values
                                GC.Collect();

                                currentBlock++;
                            }
                        }
                    }
                    end = DateTime.UtcNow;
                    DicConsole.WriteLine();
                    mhddLog.Close();
#pragma warning disable IDE0004 // Without this specific cast, it gives incorrect values
                    ibgLog.Close(dev, results.blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024, (((double)blockSize * (double)(results.blocks + 1)) / 1024) / (results.processingTime / 1000), devicePath);
#pragma warning restore IDE0004 // Without this specific cast, it gives incorrect values

                    if(ataReader.CanSeek)
                    {
                        for(int i = 0; i < seekTimes; i++)
                        {
                            if(aborted)
                                break;

                            seekCy = (ushort)rnd.Next(cylinders);
                            seekHd = (byte)rnd.Next(heads);
                            seekSc = (byte)rnd.Next(sectors);

                            DicConsole.Write("\rSeeking to cylinder {0}, head {1}, sector {2}...\t\t", seekCy, seekHd, seekSc);

                            ataReader.SeekCHS(seekCy, seekHd, seekSc, out seekCur);

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                            if(seekCur > results.seekMax && seekCur != 0)
                                results.seekMax = seekCur;
                            if(seekCur < results.seekMin && seekCur != 0)
                                results.seekMin = seekCur;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                            results.seekTotal += seekCur;
                            GC.Collect();
                        }
                    }
                }

                DicConsole.WriteLine();

                results.processingTime /= 1000;
                results.totalTime = (end - start).TotalSeconds;
#pragma warning disable IDE0004 // Without this specific cast, it gives incorrect values
                results.avgSpeed = (((double)blockSize * (double)(results.blocks + 1)) / 1048576) / results.processingTime;
#pragma warning restore IDE0004 // Without this specific cast, it gives incorrect values
                results.seekTimes = seekTimes;

                return results;
            }

            DicConsole.ErrorWriteLine("Unable to communicate with ATA device.");
            return results;
        }
    }
}
