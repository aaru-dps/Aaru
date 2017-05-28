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
            uint blockSize;
            ushort currentProfile = 0x0001;
            Decoders.ATA.AtaErrorRegistersCHS errorChs;
            Decoders.ATA.AtaErrorRegistersLBA28 errorLba;
            Decoders.ATA.AtaErrorRegistersLBA48 errorLba48;
            bool lbaMode = false;
            byte heads = 0, sectors = 0;
            ushort cylinders = 0;
            uint timeout = 5;
            double duration;

            sense = dev.AtaIdentify(out cmdBuf, out errorChs);
            if(!sense && Decoders.ATA.Identify.Decode(cmdBuf).HasValue)
            {
                Decoders.ATA.Identify.IdentifyDevice ataId = Decoders.ATA.Identify.Decode(cmdBuf).Value;

                if(ataId.CurrentCylinders > 0 && ataId.CurrentHeads > 0 && ataId.CurrentSectorsPerTrack > 0)
                {
                    cylinders = ataId.CurrentCylinders;
                    heads = (byte)ataId.CurrentHeads;
                    sectors = (byte)ataId.CurrentSectorsPerTrack;
                    results.blocks = (ulong)(cylinders * heads * sectors);
                }

                if((ataId.CurrentCylinders == 0 || ataId.CurrentHeads == 0 || ataId.CurrentSectorsPerTrack == 0) &&
                   (ataId.Cylinders > 0 && ataId.Heads > 0 && ataId.SectorsPerTrack > 0))
                {
                    cylinders = ataId.Cylinders;
                    heads = (byte)ataId.Heads;
                    sectors = (byte)ataId.SectorsPerTrack;
                    results.blocks = (ulong)(cylinders * heads * sectors);
                }

                if(ataId.Capabilities.HasFlag(Decoders.ATA.Identify.CapabilitiesBit.LBASupport))
                {
                    results.blocks = ataId.LBASectors;
                    lbaMode = true;
                }

                if(ataId.CommandSet2.HasFlag(Decoders.ATA.Identify.CommandSetBit2.LBA48))
                {
                    results.blocks = ataId.LBA48Sectors;
                    lbaMode = true;
                }

                if((ataId.PhysLogSectorSize & 0x8000) == 0x0000 &&
                    (ataId.PhysLogSectorSize & 0x4000) == 0x4000)
                {
                    if((ataId.PhysLogSectorSize & 0x1000) == 0x1000)
                    {
                        if(ataId.LogicalSectorWords <= 255 || ataId.LogicalAlignment == 0xFFFF)
                            blockSize = 512;
                        else
                            blockSize = ataId.LogicalSectorWords * 2;
                    }
                    else
                        blockSize = 512;
                }
                else
                    blockSize = 512;

                bool ReadLba = false;
                bool ReadRetryLba = false;
                bool ReadDmaLba = false;
                bool ReadDmaRetryLba = false;
                bool SeekLba = false;

                bool ReadLba48 = false;
                bool ReadDmaLba48 = false;

                bool Read = false;
                bool ReadRetry = false;
                bool ReadDma = false;
                bool ReadDmaRetry = false;
                bool Seek = false;

                sense = dev.Read(out cmdBuf, out errorChs, false, 0, 0, 1, 1, timeout, out duration);
                Read = (!sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0 && cmdBuf.Length > 0);
                sense = dev.Read(out cmdBuf, out errorChs, true, 0, 0, 1, 1, timeout, out duration);
                ReadRetry = (!sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0 && cmdBuf.Length > 0);
                sense = dev.ReadDma(out cmdBuf, out errorChs, false, 0, 0, 1, 1, timeout, out duration);
                ReadDma = (!sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0 && cmdBuf.Length > 0);
                sense = dev.ReadDma(out cmdBuf, out errorChs, true, 0, 0, 1, 1, timeout, out duration);
                ReadDmaRetry = (!sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0 && cmdBuf.Length > 0);
                sense = dev.Seek(out errorChs, 0, 0, 1, timeout, out duration);
                Seek = (!sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0);

                sense = dev.Read(out cmdBuf, out errorLba, false, 0, 1, timeout, out duration);
                ReadLba = (!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                sense = dev.Read(out cmdBuf, out errorLba, true, 0, 1, timeout, out duration);
                ReadRetryLba = (!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                sense = dev.ReadDma(out cmdBuf, out errorLba, false, 0, 1, timeout, out duration);
                ReadDmaLba = (!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                sense = dev.ReadDma(out cmdBuf, out errorLba, true, 0, 1, timeout, out duration);
                ReadDmaRetryLba = (!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                sense = dev.Seek(out errorLba, 0, timeout, out duration);
                SeekLba = (!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0);

                sense = dev.Read(out cmdBuf, out errorLba48, 0, 1, timeout, out duration);
                ReadLba48 = (!sense && (errorLba48.status & 0x27) == 0 && errorLba48.error == 0 && cmdBuf.Length > 0);
                sense = dev.ReadDma(out cmdBuf, out errorLba48, 0, 1, timeout, out duration);
                ReadDmaLba48 = (!sense && (errorLba48.status & 0x27) == 0 && errorLba48.error == 0 && cmdBuf.Length > 0);

                if(!lbaMode)
                {
                    if(results.blocks > 0xFFFFFFF && !ReadLba48 && !ReadDmaLba48)
                    {
                        DicConsole.ErrorWriteLine("Device needs 48-bit LBA commands but I can't issue them... Aborting.");
                        return results;
                    }

                    if(!ReadLba && !ReadRetryLba && !ReadDmaLba && !ReadDmaRetryLba)
                    {
                        DicConsole.ErrorWriteLine("Device needs 28-bit LBA commands but I can't issue them... Aborting.");
                        return results;
                    }
                }
                else
                {
                    if(!Read && !ReadRetry && !ReadDma && !ReadDmaRetry)
                    {
                        DicConsole.ErrorWriteLine("Device needs CHS commands but I can't issue them... Aborting.");
                        return results;
                    }
                }

                if(ReadDmaLba48)
                    DicConsole.WriteLine("Using ATA READ DMA EXT command.");
                else if(ReadLba48)
                    DicConsole.WriteLine("Using ATA READ EXT command.");
                else if(ReadDmaRetryLba)
                    DicConsole.WriteLine("Using ATA READ DMA command with retries (LBA).");
                else if(ReadDmaLba)
                    DicConsole.WriteLine("Using ATA READ DMA command (LBA).");
                else if(ReadRetryLba)
                    DicConsole.WriteLine("Using ATA READ command with retries (LBA).");
                else if(ReadLba)
                    DicConsole.WriteLine("Using ATA READ command (LBA).");
                else if(ReadDmaRetry)
                    DicConsole.WriteLine("Using ATA READ DMA command with retries (CHS).");
                else if(ReadDma)
                    DicConsole.WriteLine("Using ATA READ DMA command (CHS).");
                else if(ReadRetry)
                    DicConsole.WriteLine("Using ATA READ command with retries (CHS).");
                else if(Read)
                    DicConsole.WriteLine("Using ATA READ command (CHS).");

                byte blocksToRead = 254;
                bool error = true;
                while(lbaMode)
                {
                    if(ReadDmaLba48)
                    {
                        sense = dev.ReadDma(out cmdBuf, out errorLba48, 0, blocksToRead, timeout, out duration);
                        error = !(!sense && (errorLba48.status & 0x27) == 0 && errorLba48.error == 0 && cmdBuf.Length > 0);
                    }
                    else if(ReadLba48)
                    {
                        sense = dev.Read(out cmdBuf, out errorLba48, 0, blocksToRead, timeout, out duration);
                        error = !(!sense && (errorLba48.status & 0x27) == 0 && errorLba48.error == 0 && cmdBuf.Length > 0);
                    }
                    else if(ReadDmaRetryLba)
                    {
                        sense = dev.ReadDma(out cmdBuf, out errorLba, true, 0, blocksToRead, timeout, out duration);
                        error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                    }
                    else if(ReadDmaLba)
                    {
                        sense = dev.ReadDma(out cmdBuf, out errorLba, false, 0, blocksToRead, timeout, out duration);
                        error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                    }
                    else if(ReadRetryLba)
                    {
                        sense = dev.Read(out cmdBuf, out errorLba, true, 0, blocksToRead, timeout, out duration);
                        error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                    }
                    else if(ReadLba)
                    {
                        sense = dev.Read(out cmdBuf, out errorLba, false, 0, blocksToRead, timeout, out duration);
                        error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                    }

                    if(error)
                        blocksToRead /= 2;

                    if(!error || blocksToRead == 1)
                        break;
                }

                if(error && lbaMode)
                {
                    DicConsole.ErrorWriteLine("Device error {0} trying to guess ideal transfer length.", dev.LastError);
                    return results;
                }

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

                        double cmdDuration = 0;

                        if((results.blocks - i) < blocksToRead)
                            blocksToRead = (byte)(results.blocks - i);

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                        if(currentSpeed > results.maxSpeed && currentSpeed != 0)
                            results.maxSpeed = currentSpeed;
                        if(currentSpeed < results.minSpeed && currentSpeed != 0)
                            results.minSpeed = currentSpeed;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                        DicConsole.Write("\rReading sector {0} of {1} ({2:F3} MiB/sec.)", i, results.blocks, currentSpeed);

                        error = true;
                        byte status = 0, errorByte = 0;

                        if(ReadDmaLba48)
                        {
                            sense = dev.ReadDma(out cmdBuf, out errorLba48, i, blocksToRead, timeout, out cmdDuration);
                            error = !(!sense && (errorLba48.status & 0x27) == 0 && errorLba48.error == 0 && cmdBuf.Length > 0);
                            status = errorLba48.status;
                            errorByte = errorLba48.error;
                        }
                        else if(ReadLba48)
                        {
                            sense = dev.Read(out cmdBuf, out errorLba48, i, blocksToRead, timeout, out cmdDuration);
                            error = !(!sense && (errorLba48.status & 0x27) == 0 && errorLba48.error == 0 && cmdBuf.Length > 0);
                            status = errorLba48.status;
                            errorByte = errorLba48.error;
                        }
                        else if(ReadDmaRetryLba)
                        {
                            sense = dev.ReadDma(out cmdBuf, out errorLba, true, (uint)i, blocksToRead, timeout, out cmdDuration);
                            error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                            status = errorLba.status;
                            errorByte = errorLba.error;
                        }
                        else if(ReadDmaLba)
                        {
                            sense = dev.ReadDma(out cmdBuf, out errorLba, false, (uint)i, blocksToRead, timeout, out cmdDuration);
                            error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                            status = errorLba.status;
                            errorByte = errorLba.error;
                        }
                        else if(ReadRetryLba)
                        {
                            sense = dev.Read(out cmdBuf, out errorLba, true, (uint)i, blocksToRead, timeout, out cmdDuration);
                            error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                            status = errorLba.status;
                            errorByte = errorLba.error;
                        }
                        else if(ReadLba)
                        {
                            sense = dev.Read(out cmdBuf, out errorLba, false, (uint)i, blocksToRead, timeout, out cmdDuration);
                            error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                            status = errorLba.status;
                            errorByte = errorLba.error;
                        }

                        if(!error)
                        {
                            if(cmdDuration >= 500)
                            {
                                results.F += blocksToRead;
                            }
                            else if(cmdDuration >= 150)
                            {
                                results.E += blocksToRead;
                            }
                            else if(cmdDuration >= 50)
                            {
                                results.D += blocksToRead;
                            }
                            else if(cmdDuration >= 10)
                            {
                                results.C += blocksToRead;
                            }
                            else if(cmdDuration >= 3)
                            {
                                results.B += blocksToRead;
                            }
                            else
                            {
                                results.A += blocksToRead;
                            }

                            mhddLog.Write(i, cmdDuration);
                            ibgLog.Write(i, currentSpeed * 1024);
                        }
                        else
                        {
                            DicConsole.DebugWriteLine("Media-Scan", "ATA ERROR: {0} STATUS: {1}", errorByte, status);
                            results.errored += blocksToRead;
                            results.unreadableSectors.Add(i);
                            if(cmdDuration < 500)
                                mhddLog.Write(i, 65535);
                            else
                                mhddLog.Write(i, cmdDuration);

                            ibgLog.Write(i, 0);
                        }

#pragma warning disable IDE0004 // Without this specific cast, it gives incorrect values
                        currentSpeed = ((double)blockSize * blocksToRead / (double)1048576) / (cmdDuration / (double)1000);
#pragma warning restore IDE0004 // Without this specific cast, it gives incorrect values
                        GC.Collect();
                    }
                    end = DateTime.UtcNow;
                    DicConsole.WriteLine();
                    mhddLog.Close();
#pragma warning disable IDE0004 // Without this specific cast, it gives incorrect values
                    ibgLog.Close(dev, results.blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024, (((double)blockSize * (double)(results.blocks + 1)) / 1024) / (results.processingTime / 1000), devicePath);
#pragma warning restore IDE0004 // Without this specific cast, it gives incorrect values

                    if(SeekLba)
                    {
                        for(int i = 0; i < seekTimes; i++)
                        {
                            if(aborted)
                                break;

                            seekPos = (uint)rnd.Next((int)results.blocks);

                            DicConsole.Write("\rSeeking to sector {0}...\t\t", seekPos);

                            if(SeekLba)
                                dev.Seek(out errorLba, seekPos, timeout, out seekCur);

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

                                double cmdDuration = 0;

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                                if(currentSpeed > results.maxSpeed && currentSpeed != 0)
                                    results.maxSpeed = currentSpeed;
                                if(currentSpeed < results.minSpeed && currentSpeed != 0)
                                    results.minSpeed = currentSpeed;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                                DicConsole.Write("\rReading cylinder {0} head {1} sector {2} ({3:F3} MiB/sec.)", Cy, Hd, Sc, currentSpeed);

                                error = true;
                                byte status = 0, errorByte = 0;

                                if(ReadDmaRetry)
                                {
                                    sense = dev.ReadDma(out cmdBuf, out errorChs, true, Cy, Hd, Sc, 1, timeout, out cmdDuration);
                                    error = !(!sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0 && cmdBuf.Length > 0);
                                    status = errorChs.status;
                                    errorByte = errorChs.error;
                                }
                                else if(ReadDma)
                                {
                                    sense = dev.ReadDma(out cmdBuf, out errorChs, false, Cy, Hd, Sc, 1, timeout, out cmdDuration);
                                    error = !(!sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0 && cmdBuf.Length > 0);
                                    status = errorChs.status;
                                    errorByte = errorChs.error;
                                }
                                else if(ReadRetry)
                                {
                                    sense = dev.Read(out cmdBuf, out errorChs, true, Cy, Hd, Sc, 1, timeout, out cmdDuration);
                                    error = !(!sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0 && cmdBuf.Length > 0);
                                    status = errorChs.status;
                                    errorByte = errorChs.error;
                                }
                                else if(Read)
                                {
                                    sense = dev.Read(out cmdBuf, out errorChs, false, Cy, Hd, Sc, 1, timeout, out cmdDuration);
                                    error = !(!sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0 && cmdBuf.Length > 0);
                                    status = errorChs.status;
                                    errorByte = errorChs.error;
                                }

                                if(!error)
                                {
                                    if(cmdDuration >= 500)
                                    {
                                        results.F += blocksToRead;
                                    }
                                    else if(cmdDuration >= 150)
                                    {
                                        results.E += blocksToRead;
                                    }
                                    else if(cmdDuration >= 50)
                                    {
                                        results.D += blocksToRead;
                                    }
                                    else if(cmdDuration >= 10)
                                    {
                                        results.C += blocksToRead;
                                    }
                                    else if(cmdDuration >= 3)
                                    {
                                        results.B += blocksToRead;
                                    }
                                    else
                                    {
                                        results.A += blocksToRead;
                                    }

                                    mhddLog.Write(currentBlock, cmdDuration);
                                    ibgLog.Write(currentBlock, currentSpeed * 1024);
                                }
                                else
                                {
                                    DicConsole.DebugWriteLine("Media-Scan", "ATA ERROR: {0} STATUS: {1}", errorByte, status);
                                    results.errored += blocksToRead;
                                    results.unreadableSectors.Add(currentBlock);
                                    if(cmdDuration < 500)
                                        mhddLog.Write(currentBlock, 65535);
                                    else
                                        mhddLog.Write(currentBlock, cmdDuration);

                                    ibgLog.Write(currentBlock, 0);
                                }

#pragma warning disable IDE0004 // Without this specific cast, it gives incorrect values
                                currentSpeed = ((double)blockSize / (double)1048576) / (cmdDuration / (double)1000);
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

                    if(Seek)
                    {
                        for(int i = 0; i < seekTimes; i++)
                        {
                            if(aborted)
                                break;

                            seekCy = (ushort)rnd.Next(cylinders);
                            seekHd = (byte)rnd.Next(heads);
                            seekSc = (byte)rnd.Next(sectors);

                            DicConsole.Write("\rSeeking to cylinder {0}, head {1}, sector {2}...\t\t", seekCy, seekHd, seekSc);

                            if(Seek)
                                dev.Seek(out errorChs, seekCy, seekHd, seekSc, timeout, out seekCur);

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
