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
        public static void Scan(string MHDDLogPath, string IBGLogPath, string devicePath, Device dev)
        {
            bool aborted;
            MHDDLog mhddLog;
            IBGLog ibgLog;
            byte[] cmdBuf;
            bool sense;
            ulong blocks = 0;
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
                    blocks = (ulong)(cylinders * heads * sectors);
                }

                if((ataId.CurrentCylinders == 0 || ataId.CurrentHeads == 0 || ataId.CurrentSectorsPerTrack == 0) &&
                   (ataId.Cylinders > 0 && ataId.Heads > 0 && ataId.SectorsPerTrack > 0))
                {
                    cylinders = ataId.Cylinders;
                    heads = (byte)ataId.Heads;
                    sectors = (byte)ataId.SectorsPerTrack;
                    blocks = (ulong)(cylinders * heads * sectors);
                }

                if(ataId.Capabilities.HasFlag(Decoders.ATA.Identify.CapabilitiesBit.LBASupport))
                {
                    blocks = ataId.LBASectors;
                    lbaMode = true;
                }

                if(ataId.CommandSet2.HasFlag(Decoders.ATA.Identify.CommandSetBit2.LBA48))
                {
                    blocks = ataId.LBA48Sectors;
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
                    if(blocks > 0xFFFFFFF && !ReadLba48 && !ReadDmaLba48)
                    {
                        DicConsole.ErrorWriteLine("Device needs 48-bit LBA commands but I can't issue them... Aborting.");
                        return;
                    }

                    if(!ReadLba && !ReadRetryLba && !ReadDmaLba && !ReadDmaRetryLba)
                    {
                        DicConsole.ErrorWriteLine("Device needs 28-bit LBA commands but I can't issue them... Aborting.");
                        return;
                    }
                }
                else
                {
                    if(!Read && !ReadRetry && !ReadDma && !ReadDmaRetry)
                    {
                        DicConsole.ErrorWriteLine("Device needs CHS commands but I can't issue them... Aborting.");
                        return;
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
                    return;
                }

                ulong A = 0; // <3ms
                ulong B = 0; // >=3ms, <10ms
                ulong C = 0; // >=10ms, <50ms
                ulong D = 0; // >=50ms, <150ms
                ulong E = 0; // >=150ms, <500ms
                ulong F = 0; // >=500ms
                ulong errored = 0;
                DateTime start;
                DateTime end;
                double totalDuration = 0;
                double currentSpeed = 0;
                double maxSpeed = double.MinValue;
                double minSpeed = double.MaxValue;
                List<ulong> unreadableSectors = new List<ulong>();
                double seekMax = double.MinValue;
                double seekMin = double.MaxValue;
                double seekTotal = 0;
                const int seekTimes = 1000;

                double seekCur = 0;

                Random rnd = new Random();

                uint seekPos = (uint)rnd.Next((int)blocks);
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

                    mhddLog = new MHDDLog(MHDDLogPath, dev, blocks, blockSize, blocksToRead);
                    ibgLog = new IBGLog(IBGLogPath, currentProfile);

                    start = DateTime.UtcNow;
                    for(ulong i = 0; i < blocks; i += blocksToRead)
                    {
                        if(aborted)
                            break;

                        double cmdDuration = 0;

                        if((blocks - i) < blocksToRead)
                            blocksToRead = (byte)(blocks - i);

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                        if(currentSpeed > maxSpeed && currentSpeed != 0)
                            maxSpeed = currentSpeed;
                        if(currentSpeed < minSpeed && currentSpeed != 0)
                            minSpeed = currentSpeed;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                        DicConsole.Write("\rReading sector {0} of {1} ({2:F3} MiB/sec.)", i, blocks, currentSpeed);

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
                                F += blocksToRead;
                            }
                            else if(cmdDuration >= 150)
                            {
                                E += blocksToRead;
                            }
                            else if(cmdDuration >= 50)
                            {
                                D += blocksToRead;
                            }
                            else if(cmdDuration >= 10)
                            {
                                C += blocksToRead;
                            }
                            else if(cmdDuration >= 3)
                            {
                                B += blocksToRead;
                            }
                            else
                            {
                                A += blocksToRead;
                            }

                            mhddLog.Write(i, cmdDuration);
                            ibgLog.Write(i, currentSpeed * 1024);
                        }
                        else
                        {
                            DicConsole.DebugWriteLine("Media-Scan", "ATA ERROR: {0} STATUS: {1}", errorByte, status);
                            errored += blocksToRead;
                            unreadableSectors.Add(i);
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
                    ibgLog.Close(dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024, (((double)blockSize * (double)(blocks + 1)) / 1024) / (totalDuration / 1000), devicePath);
#pragma warning restore IDE0004 // Without this specific cast, it gives incorrect values

                    if(SeekLba)
                    {
                        for(int i = 0; i < seekTimes; i++)
                        {
                            if(aborted)
                                break;

                            seekPos = (uint)rnd.Next((int)blocks);

                            DicConsole.Write("\rSeeking to sector {0}...\t\t", seekPos);

                            if(SeekLba)
                                dev.Seek(out errorLba, seekPos, timeout, out seekCur);

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                            if(seekCur > seekMax && seekCur != 0)
                                seekMax = seekCur;
                            if(seekCur < seekMin && seekCur != 0)
                                seekMin = seekCur;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                            seekTotal += seekCur;
                            GC.Collect();
                        }
                    }
                }
                else
                {
                    mhddLog = new MHDDLog(MHDDLogPath, dev, blocks, blockSize, blocksToRead);
                    ibgLog = new IBGLog(IBGLogPath, currentProfile);

                    ulong currentBlock = 0;
                    blocks = (ulong)(cylinders * heads * sectors);
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
                                if(currentSpeed > maxSpeed && currentSpeed != 0)
                                    maxSpeed = currentSpeed;
                                if(currentSpeed < minSpeed && currentSpeed != 0)
                                    minSpeed = currentSpeed;
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
                                        F += blocksToRead;
                                    }
                                    else if(cmdDuration >= 150)
                                    {
                                        E += blocksToRead;
                                    }
                                    else if(cmdDuration >= 50)
                                    {
                                        D += blocksToRead;
                                    }
                                    else if(cmdDuration >= 10)
                                    {
                                        C += blocksToRead;
                                    }
                                    else if(cmdDuration >= 3)
                                    {
                                        B += blocksToRead;
                                    }
                                    else
                                    {
                                        A += blocksToRead;
                                    }

                                    mhddLog.Write(currentBlock, cmdDuration);
                                    ibgLog.Write(currentBlock, currentSpeed * 1024);
                                }
                                else
                                {
                                    DicConsole.DebugWriteLine("Media-Scan", "ATA ERROR: {0} STATUS: {1}", errorByte, status);
                                    errored += blocksToRead;
                                    unreadableSectors.Add(currentBlock);
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
                    ibgLog.Close(dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024, (((double)blockSize * (double)(blocks + 1)) / 1024) / (totalDuration / 1000), devicePath);
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
                            if(seekCur > seekMax && seekCur != 0)
                                seekMax = seekCur;
                            if(seekCur < seekMin && seekCur != 0)
                                seekMin = seekCur;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                            seekTotal += seekCur;
                            GC.Collect();
                        }
                    }
                }

                DicConsole.WriteLine();

                DicConsole.WriteLine("Took a total of {0} seconds ({1} processing commands).", (end - start).TotalSeconds, totalDuration / 1000);
#pragma warning disable IDE0004 // Without this specific cast, it gives incorrect values
                DicConsole.WriteLine("Avegare speed: {0:F3} MiB/sec.", (((double)blockSize * (double)(blocks + 1)) / 1048576) / (totalDuration / 1000));
#pragma warning restore IDE0004 // Without this specific cast, it gives incorrect values
                DicConsole.WriteLine("Fastest speed burst: {0:F3} MiB/sec.", maxSpeed);
                DicConsole.WriteLine("Slowest speed burst: {0:F3} MiB/sec.", minSpeed);
                DicConsole.WriteLine("Summary:");
                DicConsole.WriteLine("{0} sectors took less than 3 ms.", A);
                DicConsole.WriteLine("{0} sectors took less than 10 ms but more than 3 ms.", B);
                DicConsole.WriteLine("{0} sectors took less than 50 ms but more than 10 ms.", C);
                DicConsole.WriteLine("{0} sectors took less than 150 ms but more than 50 ms.", D);
                DicConsole.WriteLine("{0} sectors took less than 500 ms but more than 150 ms.", E);
                DicConsole.WriteLine("{0} sectors took more than 500 ms.", F);
                DicConsole.WriteLine("{0} sectors could not be read.", unreadableSectors.Count);
                if(unreadableSectors.Count > 0)
                {
                    foreach(ulong bad in unreadableSectors)
                        DicConsole.WriteLine("Sector {0} could not be read", bad);
                }
                DicConsole.WriteLine();

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                if(seekTotal != 0 || seekMin != double.MaxValue || seekMax != double.MinValue)
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
                    DicConsole.WriteLine("Testing {0} seeks, longest seek took {1:F3} ms, fastest one took {2:F3} ms. ({3:F3} ms average)",
                                     seekTimes, seekMax, seekMin, seekTotal / 1000);

                Statistics.AddMediaScan((long)A, (long)B, (long)C, (long)D, (long)E, (long)F, (long)blocks, (long)errored, (long)(blocks - errored));
            }
            else
                DicConsole.ErrorWriteLine("Unable to communicate with ATA device.");
        }
    }
}
