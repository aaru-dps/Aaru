// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MediaScan.cs
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
using DiscImageChef.Console;
using DiscImageChef.Devices;
using System.Collections.Generic;

namespace DiscImageChef.Commands
{
    public static class MediaScan
    {
        static bool aborted;
        static Core.MHDDLog mhddLog;
        static Core.IBGLog ibgLog;

        public static void doMediaScan(MediaScanSubOptions options)
        {
            DicConsole.DebugWriteLine("Media-Scan command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Media-Scan command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Media-Scan command", "--device={0}", options.DevicePath);
            DicConsole.DebugWriteLine("Media-Scan command", "--mhdd-log={0}", options.MHDDLogPath);
            DicConsole.DebugWriteLine("Media-Scan command", "--ibg-log={0}", options.IBGLogPath);

            if (!System.IO.File.Exists(options.DevicePath))
            {
                DicConsole.ErrorWriteLine("Specified device does not exist.");
                return;
            }

            if (options.DevicePath.Length == 2 && options.DevicePath[1] == ':' &&
                options.DevicePath[0] != '/' && Char.IsLetter(options.DevicePath[0]))
            {
                options.DevicePath = "\\\\.\\" + Char.ToUpper(options.DevicePath[0]) + ':';
            }

            mhddLog = null;
            ibgLog = null;

            Device dev = new Device(options.DevicePath);

            if (dev.Error)
            {
                DicConsole.ErrorWriteLine("Error {0} opening device.", dev.LastError);
                return;
            }

            Core.Statistics.AddDevice(dev);

            switch (dev.Type)
            {
                case DeviceType.ATA:
                    doATAMediaScan(options.MHDDLogPath, options.IBGLogPath, options.DevicePath, dev);
                    break;
                case DeviceType.MMC:
                case DeviceType.SecureDigital:
                    doSDMediaScan(options.MHDDLogPath, options.IBGLogPath, options.DevicePath, dev);
                    break;
                case DeviceType.NVMe:
                    doNVMeMediaScan(options.MHDDLogPath, options.IBGLogPath, options.DevicePath, dev);
                    break;
                case DeviceType.ATAPI:
                case DeviceType.SCSI:
                    doSCSIMediaScan(options.MHDDLogPath, options.IBGLogPath, options.DevicePath, dev);
                    break;
                default:
                    throw new NotSupportedException("Unknown device type.");
            }

            Core.Statistics.AddCommand("media-scan");
        }

        static void doATAMediaScan(string MHDDLogPath, string IBGLogPath, string devicePath, Device dev)
        {
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
            if (!sense && Decoders.ATA.Identify.Decode(cmdBuf).HasValue)
            {
                Decoders.ATA.Identify.IdentifyDevice ataId = Decoders.ATA.Identify.Decode(cmdBuf).Value;

                if (ataId.CurrentCylinders > 0 && ataId.CurrentHeads > 0 && ataId.CurrentSectorsPerTrack > 0)
                {
                    cylinders = ataId.CurrentCylinders;
                    heads = (byte)ataId.CurrentHeads;
                    sectors = (byte)ataId.CurrentSectorsPerTrack;
                    blocks = (ulong)(cylinders * heads * sectors);
                }

                if ((ataId.CurrentCylinders == 0 || ataId.CurrentHeads == 0 || ataId.CurrentSectorsPerTrack == 0) &&
                   (ataId.Cylinders > 0 && ataId.Heads > 0 && ataId.SectorsPerTrack > 0))
                {
                    cylinders = ataId.Cylinders;
                    heads = (byte)ataId.Heads;
                    sectors = (byte)ataId.SectorsPerTrack;
                    blocks = (ulong)(cylinders * heads * sectors);
                }

                if (ataId.Capabilities.HasFlag(Decoders.ATA.Identify.CapabilitiesBit.LBASupport))
                {
                    blocks = ataId.LBASectors;
                    lbaMode = true;
                }

                if (ataId.CommandSet2.HasFlag(Decoders.ATA.Identify.CommandSetBit2.LBA48))
                {
                    blocks = ataId.LBA48Sectors;
                    lbaMode = true;
                }

                if ((ataId.PhysLogSectorSize & 0x8000) == 0x0000 &&
                    (ataId.PhysLogSectorSize & 0x4000) == 0x4000)
                {
                    if ((ataId.PhysLogSectorSize & 0x1000) == 0x1000)
                    {
                        if (ataId.LogicalSectorWords <= 255 || ataId.LogicalAlignment == 0xFFFF)
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

                if (!lbaMode)
                {
                    if (blocks > 0xFFFFFFF && !ReadLba48 && !ReadDmaLba48)
                    {
                        DicConsole.ErrorWriteLine("Device needs 48-bit LBA commands but I can't issue them... Aborting.");
                        return;
                    }

                    if (!ReadLba && !ReadRetryLba && !ReadDmaLba && !ReadDmaRetryLba)
                    {
                        DicConsole.ErrorWriteLine("Device needs 28-bit LBA commands but I can't issue them... Aborting.");
                        return;
                    }
                }
                else
                {
                    if (!Read && !ReadRetry && !ReadDma && !ReadDmaRetry)
                    {
                        DicConsole.ErrorWriteLine("Device needs CHS commands but I can't issue them... Aborting.");
                        return;
                    }
                }

                if (ReadDmaLba48)
                    DicConsole.WriteLine("Using ATA READ DMA EXT command.");
                else if (ReadLba48)
                    DicConsole.WriteLine("Using ATA READ EXT command.");
                else if (ReadDmaRetryLba)
                    DicConsole.WriteLine("Using ATA READ DMA command with retries (LBA).");
                else if (ReadDmaLba)
                    DicConsole.WriteLine("Using ATA READ DMA command (LBA).");
                else if (ReadRetryLba)
                    DicConsole.WriteLine("Using ATA READ command with retries (LBA).");
                else if (ReadLba)
                    DicConsole.WriteLine("Using ATA READ command (LBA).");
                else if (ReadDmaRetry)
                    DicConsole.WriteLine("Using ATA READ DMA command with retries (CHS).");
                else if (ReadDma)
                    DicConsole.WriteLine("Using ATA READ DMA command (CHS).");
                else if (ReadRetry)
                    DicConsole.WriteLine("Using ATA READ command with retries (CHS).");
                else if (Read)
                    DicConsole.WriteLine("Using ATA READ command (CHS).");

                byte blocksToRead = 254;
                bool error = true;
                while (lbaMode)
                {
                    if (ReadDmaLba48)
                    {
                        sense = dev.ReadDma(out cmdBuf, out errorLba48, 0, blocksToRead, timeout, out duration);
                        error = !(!sense && (errorLba48.status & 0x27) == 0 && errorLba48.error == 0 && cmdBuf.Length > 0);
                    }
                    else if (ReadLba48)
                    {
                        sense = dev.Read(out cmdBuf, out errorLba48, 0, blocksToRead, timeout, out duration);
                        error = !(!sense && (errorLba48.status & 0x27) == 0 && errorLba48.error == 0 && cmdBuf.Length > 0);
                    }
                    else if (ReadDmaRetryLba)
                    {
                        sense = dev.ReadDma(out cmdBuf, out errorLba, true, 0, blocksToRead, timeout, out duration);
                        error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                    }
                    else if (ReadDmaLba)
                    {
                        sense = dev.ReadDma(out cmdBuf, out errorLba, false, 0, blocksToRead, timeout, out duration);
                        error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                    }
                    else if (ReadRetryLba)
                    {
                        sense = dev.Read(out cmdBuf, out errorLba, true, 0, blocksToRead, timeout, out duration);
                        error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                    }
                    else if (ReadLba)
                    {
                        sense = dev.Read(out cmdBuf, out errorLba, false, 0, blocksToRead, timeout, out duration);
                        error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                    }

                    if (error)
                        blocksToRead /= 2;

                    if (!error || blocksToRead == 1)
                        break;
                }

                if (error && lbaMode)
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
                ushort seekCy = (ushort)rnd.Next((int)cylinders);
                byte seekHd = (byte)rnd.Next((int)heads);
                byte seekSc = (byte)rnd.Next((int)sectors);

                aborted = false;
                System.Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = aborted = true;
                };

                if (lbaMode)
                {
                    DicConsole.WriteLine("Reading {0} sectors at a time.", blocksToRead);

                    mhddLog = new Core.MHDDLog(MHDDLogPath, dev, blocks, blockSize, blocksToRead);
                    ibgLog = new Core.IBGLog(IBGLogPath, currentProfile);

                    start = DateTime.UtcNow;
                    for (ulong i = 0; i < blocks; i += blocksToRead)
                    {
                        if (aborted)
                            break;

                        double cmdDuration = 0;

                        if ((blocks - i) < blocksToRead)
                            blocksToRead = (byte)(blocks - i);

                        if (currentSpeed > maxSpeed && currentSpeed != 0)
                            maxSpeed = currentSpeed;
                        if (currentSpeed < minSpeed && currentSpeed != 0)
                            minSpeed = currentSpeed;

                        DicConsole.Write("\rReading sector {0} of {1} ({2:F3} MiB/sec.)", i, blocks, currentSpeed);

                        error = true;
                        byte status = 0, errorByte = 0;

                        if (ReadDmaLba48)
                        {
                            sense = dev.ReadDma(out cmdBuf, out errorLba48, i, blocksToRead, timeout, out cmdDuration);
                            error = !(!sense && (errorLba48.status & 0x27) == 0 && errorLba48.error == 0 && cmdBuf.Length > 0);
                            status = errorLba48.status;
                            errorByte = errorLba48.error;
                        }
                        else if (ReadLba48)
                        {
                            sense = dev.Read(out cmdBuf, out errorLba48, i, blocksToRead, timeout, out cmdDuration);
                            error = !(!sense && (errorLba48.status & 0x27) == 0 && errorLba48.error == 0 && cmdBuf.Length > 0);
                            status = errorLba48.status;
                            errorByte = errorLba48.error;
                        }
                        else if (ReadDmaRetryLba)
                        {
                            sense = dev.ReadDma(out cmdBuf, out errorLba, true, (uint)i, blocksToRead, timeout, out cmdDuration);
                            error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                            status = errorLba.status;
                            errorByte = errorLba.error;
                        }
                        else if (ReadDmaLba)
                        {
                            sense = dev.ReadDma(out cmdBuf, out errorLba, false, (uint)i, blocksToRead, timeout, out cmdDuration);
                            error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                            status = errorLba.status;
                            errorByte = errorLba.error;
                        }
                        else if (ReadRetryLba)
                        {
                            sense = dev.Read(out cmdBuf, out errorLba, true, (uint)i, blocksToRead, timeout, out cmdDuration);
                            error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                            status = errorLba.status;
                            errorByte = errorLba.error;
                        }
                        else if (ReadLba)
                        {
                            sense = dev.Read(out cmdBuf, out errorLba, false, (uint)i, blocksToRead, timeout, out cmdDuration);
                            error = !(!sense && (errorLba.status & 0x27) == 0 && errorLba.error == 0 && cmdBuf.Length > 0);
                            status = errorLba.status;
                            errorByte = errorLba.error;
                        }

                        if (!error)
                        {
                            if (cmdDuration >= 500)
                            {
                                F += blocksToRead;
                            }
                            else if (cmdDuration >= 150)
                            {
                                E += blocksToRead;
                            }
                            else if (cmdDuration >= 50)
                            {
                                D += blocksToRead;
                            }
                            else if (cmdDuration >= 10)
                            {
                                C += blocksToRead;
                            }
                            else if (cmdDuration >= 3)
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
                            if (cmdDuration < 500)
                                mhddLog.Write(i, 65535);
                            else
                                mhddLog.Write(i, cmdDuration);

                            ibgLog.Write(i, 0);
                        }

                        currentSpeed = ((double)blockSize * blocksToRead / (double)1048576) / (cmdDuration / (double)1000);
                        GC.Collect();
                    }
                    end = DateTime.UtcNow;
                    DicConsole.WriteLine();
                    mhddLog.Close();
                    ibgLog.Close(dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024, (((double)blockSize * (double)(blocks + 1)) / 1024) / (totalDuration / 1000), devicePath);

                    if (SeekLba)
                    {
                        for (int i = 0; i < seekTimes; i++)
                        {
                            if (aborted)
                                break;

                            seekPos = (uint)rnd.Next((int)blocks);

                            DicConsole.Write("\rSeeking to sector {0}...\t\t", seekPos);

                            if (SeekLba)
                                dev.Seek(out errorLba, seekPos, timeout, out seekCur);

                            if (seekCur > seekMax && seekCur != 0)
                                seekMax = seekCur;
                            if (seekCur < seekMin && seekCur != 0)
                                seekMin = seekCur;

                            seekTotal += seekCur;
                            GC.Collect();
                        }
                    }
                }
                else
                {
                    mhddLog = new Core.MHDDLog(MHDDLogPath, dev, blocks, blockSize, blocksToRead);
                    ibgLog = new Core.IBGLog(IBGLogPath, currentProfile);

                    ulong currentBlock = 0;
                    blocks = (ulong)(cylinders * heads * sectors);
                    start = DateTime.UtcNow;
                    for (ushort Cy = 0; Cy < cylinders; Cy++)
                    {
                        for (byte Hd = 0; Hd < heads; Hd++)
                        {
                            for (byte Sc = 1; Sc < sectors; Sc++)
                            {
                                if (aborted)
                                    break;

                                double cmdDuration = 0;

                                if (currentSpeed > maxSpeed && currentSpeed != 0)
                                    maxSpeed = currentSpeed;
                                if (currentSpeed < minSpeed && currentSpeed != 0)
                                    minSpeed = currentSpeed;

                                DicConsole.Write("\rReading cylinder {0} head {1} sector {2} ({3:F3} MiB/sec.)", Cy, Hd, Sc, currentSpeed);

                                error = true;
                                byte status = 0, errorByte = 0;

                                if (ReadDmaRetry)
                                {
                                    sense = dev.ReadDma(out cmdBuf, out errorChs, true, Cy, Hd, Sc, 1, timeout, out cmdDuration);
                                    error = !(!sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0 && cmdBuf.Length > 0);
                                    status = errorChs.status;
                                    errorByte = errorChs.error;
                                }
                                else if (ReadDma)
                                {
                                    sense = dev.ReadDma(out cmdBuf, out errorChs, false, Cy, Hd, Sc, 1, timeout, out cmdDuration);
                                    error = !(!sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0 && cmdBuf.Length > 0);
                                    status = errorChs.status;
                                    errorByte = errorChs.error;
                                }
                                else if (ReadRetry)
                                {
                                    sense = dev.Read(out cmdBuf, out errorChs, true, Cy, Hd, Sc, 1, timeout, out cmdDuration);
                                    error = !(!sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0 && cmdBuf.Length > 0);
                                    status = errorChs.status;
                                    errorByte = errorChs.error;
                                }
                                else if (Read)
                                {
                                    sense = dev.Read(out cmdBuf, out errorChs, false, Cy, Hd, Sc, 1, timeout, out cmdDuration);
                                    error = !(!sense && (errorChs.status & 0x27) == 0 && errorChs.error == 0 && cmdBuf.Length > 0);
                                    status = errorChs.status;
                                    errorByte = errorChs.error;
                                }

                                if (!error)
                                {
                                    if (cmdDuration >= 500)
                                    {
                                        F += blocksToRead;
                                    }
                                    else if (cmdDuration >= 150)
                                    {
                                        E += blocksToRead;
                                    }
                                    else if (cmdDuration >= 50)
                                    {
                                        D += blocksToRead;
                                    }
                                    else if (cmdDuration >= 10)
                                    {
                                        C += blocksToRead;
                                    }
                                    else if (cmdDuration >= 3)
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
                                    if (cmdDuration < 500)
                                        mhddLog.Write(currentBlock, 65535);
                                    else
                                        mhddLog.Write(currentBlock, cmdDuration);

                                    ibgLog.Write(currentBlock, 0);
                                }

                                currentSpeed = ((double)blockSize / (double)1048576) / (cmdDuration / (double)1000);
                                GC.Collect();

                                currentBlock++;
                            }
                        }
                    }
                    end = DateTime.UtcNow;
                    DicConsole.WriteLine();
                    mhddLog.Close();
                    ibgLog.Close(dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024, (((double)blockSize * (double)(blocks + 1)) / 1024) / (totalDuration / 1000), devicePath);

                    if (Seek)
                    {
                        for (int i = 0; i < seekTimes; i++)
                        {
                            if (aborted)
                                break;

                            seekCy = (ushort)rnd.Next((int)cylinders);
                            seekHd = (byte)rnd.Next((int)heads);
                            seekSc = (byte)rnd.Next((int)sectors);

                            DicConsole.Write("\rSeeking to cylinder {0}, head {1}, sector {2}...\t\t", seekCy, seekHd, seekSc);

                            if (Seek)
                                dev.Seek(out errorChs, seekCy, seekHd, seekSc, timeout, out seekCur);

                            if (seekCur > seekMax && seekCur != 0)
                                seekMax = seekCur;
                            if (seekCur < seekMin && seekCur != 0)
                                seekMin = seekCur;

                            seekTotal += seekCur;
                            GC.Collect();
                        }
                    }
                }

                DicConsole.WriteLine();

                DicConsole.WriteLine("Took a total of {0} seconds ({1} processing commands).", (end - start).TotalSeconds, totalDuration / 1000);
                DicConsole.WriteLine("Avegare speed: {0:F3} MiB/sec.", (((double)blockSize * (double)(blocks + 1)) / 1048576) / (totalDuration / 1000));
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
                if (unreadableSectors.Count > 0)
                {
                    foreach (ulong bad in unreadableSectors)
                        DicConsole.WriteLine("Sector {0} could not be read", bad);
                }
                DicConsole.WriteLine();

                if (seekTotal != 0 || seekMin != double.MaxValue || seekMax != double.MinValue)
                    DicConsole.WriteLine("Testing {0} seeks, longest seek took {1:F3} ms, fastest one took {2:F3} ms. ({3:F3} ms average)",
                                     seekTimes, seekMax, seekMin, seekTotal / 1000);

                Core.Statistics.AddMediaScan((long)A, (long)B, (long)C, (long)D, (long)E, (long)F, (long)blocks, (long)errored, (long)(blocks - errored));
            }
            else
                DicConsole.ErrorWriteLine("Unable to communicate with ATA device.");
        }

        static void doNVMeMediaScan(string MHDDLogPath, string IBGLogPath, string devicePath, Device dev)
        {
            throw new NotImplementedException("NVMe devices not yet supported.");
        }

        static void doSDMediaScan(string MHDDLogPath, string IBGLogPath, string devicePath, Device dev)
        {
            throw new NotImplementedException("MMC/SD devices not yet supported.");
        }

        static void doSCSIMediaScan(string MHDDLogPath, string IBGLogPath, string devicePath, Device dev)
        {
            byte[] cmdBuf;
            byte[] senseBuf;
            bool sense = false;
            double duration;
            ulong blocks = 0;
            uint blockSize = 0;
            ushort currentProfile = 0x0001;

            if (dev.IsRemovable)
            {
                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out duration);
                if (sense)
                {
                    Decoders.SCSI.FixedSense? decSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf);
                    if (decSense.HasValue)
                    {
                        if (decSense.Value.ASC == 0x3A)
                        {
                            int leftRetries = 5;
                            while (leftRetries > 0)
                            {
                                DicConsole.WriteLine("\rWaiting for drive to become ready");
                                System.Threading.Thread.Sleep(2000);
                                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out duration);
                                if (!sense)
                                    break;

                                leftRetries--;
                            }

                            if (sense)
                            {
                                DicConsole.ErrorWriteLine("Please insert media in drive");
                                return;
                            }
                        }
                        else if (decSense.Value.ASC == 0x04 && decSense.Value.ASCQ == 0x01)
                        {
                            int leftRetries = 10;
                            while (leftRetries > 0)
                            {
                                DicConsole.WriteLine("\rWaiting for drive to become ready");
                                System.Threading.Thread.Sleep(2000);
                                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out duration);
                                if (!sense)
                                    break;

                                leftRetries--;
                            }

                            if (sense)
                            {
                                DicConsole.ErrorWriteLine("Error testing unit was ready:\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                                return;
                            }
                        }
                        else
                        {
                            DicConsole.ErrorWriteLine("Error testing unit was ready:\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                            return;
                        }
                    }
                    else
                    {
                        DicConsole.ErrorWriteLine("Unknown testing unit was ready.");
                        return;
                    }
                }
            }

            if (dev.SCSIType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.DirectAccess ||
                dev.SCSIType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice ||
                dev.SCSIType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.OCRWDevice ||
                dev.SCSIType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.OpticalDevice ||
                dev.SCSIType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.SimplifiedDevice ||
                dev.SCSIType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.WriteOnceDevice)
            {
                sense = dev.ReadCapacity(out cmdBuf, out senseBuf, dev.Timeout, out duration);
                if (!sense)
                {
                    blocks = (ulong)((cmdBuf[0] << 24) + (cmdBuf[1] << 16) + (cmdBuf[2] << 8) + (cmdBuf[3]));
                    blockSize = (uint)((cmdBuf[5] << 24) + (cmdBuf[5] << 16) + (cmdBuf[6] << 8) + (cmdBuf[7]));
                }

                if (sense || blocks == 0xFFFFFFFF)
                {
                    sense = dev.ReadCapacity16(out cmdBuf, out senseBuf, dev.Timeout, out duration);

                    if (sense && blocks == 0)
                    {
                        // Not all MMC devices support READ CAPACITY, as they have READ TOC
                        if (dev.SCSIType != DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice)
                        {
                            DicConsole.ErrorWriteLine("Unable to get media capacity");
                            DicConsole.ErrorWriteLine("{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                        }
                    }

                    if (!sense)
                    {
                        byte[] temp = new byte[8];

                        Array.Copy(cmdBuf, 0, temp, 0, 8);
                        Array.Reverse(temp);
                        blocks = BitConverter.ToUInt64(temp, 0);
                        blockSize = (uint)((cmdBuf[5] << 24) + (cmdBuf[5] << 16) + (cmdBuf[6] << 8) + (cmdBuf[7]));
                    }
                }

                if (blocks != 0 && blockSize != 0)
                {
                    blocks++;
                    DicConsole.WriteLine("Media has {0} blocks of {1} bytes/each. (for a total of {2} bytes)",
                        blocks, blockSize, blocks * (ulong)blockSize);
                }
            }

            if (dev.SCSIType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.SequentialAccess)
            {
                DicConsole.WriteLine("Scanning will never be supported on SCSI Streaming Devices.");
                DicConsole.WriteLine("It has no sense to do it, and it will put too much strain on the tape.");
                return;
            }            

            if (blocks == 0)
            {
                DicConsole.ErrorWriteLine("Unable to read medium or empty medium present...");
                return;
            }

            bool compactDisc = true;
            Decoders.CD.FullTOC.CDFullTOC? toc = null;

            if (dev.SCSIType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice)
            {
                sense = dev.GetConfiguration(out cmdBuf, out senseBuf, 0, MmcGetConfigurationRt.Current, dev.Timeout, out duration);
                if (!sense)
                {
                    Decoders.SCSI.MMC.Features.SeparatedFeatures ftr = Decoders.SCSI.MMC.Features.Separate(cmdBuf);

                    currentProfile = ftr.CurrentProfile;

                    switch (ftr.CurrentProfile)
                    {
                        case 0x0005:
                        case 0x0008:
                        case 0x0009:
                        case 0x000A:
                        case 0x0020:
                        case 0x0021:
                        case 0x0022:
                            break;
                        default:
                            compactDisc = false;
                            break;
                    }
                }

                if (compactDisc)
                {
                    currentProfile = 0x0008;
                    // We discarded all discs that falsify a TOC before requesting a real TOC
                    // No TOC, no CD (or an empty one)
                    bool tocSense = dev.ReadRawToc(out cmdBuf, out senseBuf, 1, dev.Timeout, out duration);
                    if (!tocSense)
                        toc = Decoders.CD.FullTOC.Decode(cmdBuf);
                }
            }
            else
                compactDisc = false;

            byte[] readBuffer;
            uint blocksToRead = 64;

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

            aborted = false;
            System.Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = aborted = true;
            };

            bool read6 = false, read10 = false, read12 = false, read16 = false, readcd;

            if (compactDisc)
            {
                if (toc == null)
                {
                    DicConsole.ErrorWriteLine("Error trying to decode TOC...");
                    return;
                }

                readcd = !dev.ReadCd(out readBuffer, out senseBuf, 0, 2352, 1, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                    true, true, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out duration);

                if (readcd)
                    DicConsole.WriteLine("Using MMC READ CD command.");

                start = DateTime.UtcNow;

                while (true)
                {
                    if (readcd)
                    {
                        sense = dev.ReadCd(out readBuffer, out senseBuf, 0, 2352, blocksToRead, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                            true, true, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out duration);
                        if (dev.Error)
                            blocksToRead /= 2;
                    }

                    if (!dev.Error || blocksToRead == 1)
                        break;
                }

                if (dev.Error)
                {
                    DicConsole.ErrorWriteLine("Device error {0} trying to guess ideal transfer length.", dev.LastError);
                    return;
                }

                DicConsole.WriteLine("Reading {0} sectors at a time.", blocksToRead);

                mhddLog = new Core.MHDDLog(MHDDLogPath, dev, blocks, blockSize, blocksToRead);
                ibgLog = new Core.IBGLog(IBGLogPath, currentProfile);

                for (ulong i = 0; i < blocks; i += blocksToRead)
                {
                    if (aborted)
                        break;

                    double cmdDuration = 0;

                    if ((blocks - i) < blocksToRead)
                        blocksToRead = (uint)(blocks - i);

                    if (currentSpeed > maxSpeed && currentSpeed != 0)
                        maxSpeed = currentSpeed;
                    if (currentSpeed < minSpeed && currentSpeed != 0)
                        minSpeed = currentSpeed;

                    DicConsole.Write("\rReading sector {0} of {1} ({2:F3} MiB/sec.)", i, blocks, currentSpeed);

                    if (readcd)
                    {
                        sense = dev.ReadCd(out readBuffer, out senseBuf, (uint)i, 2352, blocksToRead, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                            true, true, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out cmdDuration);
                        totalDuration += cmdDuration;
                    }

                    if (!sense)
                    {
                        if (cmdDuration >= 500)
                        {
                            F += blocksToRead;
                        }
                        else if (cmdDuration >= 150)
                        {
                            E += blocksToRead;
                        }
                        else if (cmdDuration >= 50)
                        {
                            D += blocksToRead;
                        }
                        else if (cmdDuration >= 10)
                        {
                            C += blocksToRead;
                        }
                        else if (cmdDuration >= 3)
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
                        DicConsole.DebugWriteLine("Media-Scan", "READ CD error:\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));

                        Decoders.SCSI.FixedSense? senseDecoded = Decoders.SCSI.Sense.DecodeFixed(senseBuf);
                        if (senseDecoded.HasValue)
                        {
                            // TODO: This error happens when changing from track type afaik. Need to solve that more cleanly
                            // LOGICAL BLOCK ADDRESS OUT OF RANGE
                            if ((senseDecoded.Value.ASC != 0x21 || senseDecoded.Value.ASCQ != 0x00) &&
                                // ILLEGAL MODE FOR THIS TRACK (requesting sectors as-is, this is a firmware misconception when audio sectors
                                // are in a track where subchannel indicates data)
                                (senseDecoded.Value.ASC != 0x64 || senseDecoded.Value.ASCQ != 0x00))
                            {
                                errored += blocksToRead;
                                unreadableSectors.Add(i);
                                if (cmdDuration < 500)
                                    mhddLog.Write(i, 65535);
                                else
                                    mhddLog.Write(i, cmdDuration);
                                    
                                ibgLog.Write(i, 0);
                            }
                        }
                        else
                        {
                            errored += blocksToRead;
                            unreadableSectors.Add(i);
                            if (cmdDuration < 500)
                                mhddLog.Write(i, 65535);
                            else
                                mhddLog.Write(i, cmdDuration);
                                
                            ibgLog.Write(i, 0);
                        }
                    }

                    currentSpeed = ((double)blockSize * blocksToRead / (double)1048576) / (cmdDuration / (double)1000);
                    GC.Collect();
                }
                end = DateTime.UtcNow;
                DicConsole.WriteLine();
                mhddLog.Close();
                ibgLog.Close(dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024, (((double)blockSize * (double)(blocks + 1)) / 1024) / (totalDuration / 1000), devicePath);
            }
            else
            {
                read6 = !dev.Read6(out readBuffer, out senseBuf, 0, blockSize, dev.Timeout, out duration);

                read10 = !dev.Read10(out readBuffer, out senseBuf, 0, false, true, false, false, 0, blockSize, 0, 1, dev.Timeout, out duration);

                read12 = !dev.Read12(out readBuffer, out senseBuf, 0, false, true, false, false, 0, blockSize, 0, 1, false, dev.Timeout, out duration);

                read16 = !dev.Read16(out readBuffer, out senseBuf, 0, false, true, false, 0, blockSize, 0, 1, false, dev.Timeout, out duration);

                if (!read6 && !read10 && !read12 && !read16)
                {
                    DicConsole.ErrorWriteLine("Cannot read medium, aborting scan...");
                    return;
                }

                if (read6 && !read10 && !read12 && !read16 && blocks > (0x001FFFFF + 1))
                {
                    DicConsole.ErrorWriteLine("Device only supports SCSI READ (6) but has more than {0} blocks ({1} blocks total)", 0x001FFFFF + 1, blocks);
                    return;
                }

                if (!read16 && blocks > ((long)0xFFFFFFFF + (long)1))
                {
                    DicConsole.ErrorWriteLine("Device only supports SCSI READ (10) but has more than {0} blocks ({1} blocks total)", (long)0xFFFFFFFF + (long)1, blocks);
                    return;
                }

                if (read16)
                    DicConsole.WriteLine("Using SCSI READ (16) command.");
                else if (read12)
                    DicConsole.WriteLine("Using SCSI READ (12) command.");
                else if (read10)
                    DicConsole.WriteLine("Using SCSI READ (10) command.");
                else if (read6)
                    DicConsole.WriteLine("Using SCSI READ (6) command.");

                start = DateTime.UtcNow;

                while (true)
                {
                    if (read16)
                    {
                        sense = dev.Read16(out readBuffer, out senseBuf, 0, false, true, false, 0, blockSize, 0, blocksToRead, false, dev.Timeout, out duration);
                        if (dev.Error)
                            blocksToRead /= 2;
                    }
                    else if (read12)
                    {
                        sense = dev.Read12(out readBuffer, out senseBuf, 0, false, false, false, false, 0, blockSize, 0, blocksToRead, false, dev.Timeout, out duration);
                        if (dev.Error)
                            blocksToRead /= 2;
                    }
                    else if (read10)
                    {
                        sense = dev.Read10(out readBuffer, out senseBuf, 0, false, true, false, false, 0, blockSize, 0, (ushort)blocksToRead, dev.Timeout, out duration);
                        if (dev.Error)
                            blocksToRead /= 2;
                    }
                    else if (read6)
                    {
                        sense = dev.Read6(out readBuffer, out senseBuf, 0, blockSize, (byte)blocksToRead, dev.Timeout, out duration);
                        if (dev.Error)
                            blocksToRead /= 2;
                    }

                    if (!dev.Error || blocksToRead == 1)
                        break;
                }

                if (dev.Error)
                {
                    DicConsole.ErrorWriteLine("Device error {0} trying to guess ideal transfer length.", dev.LastError);
                    return;
                }

                DicConsole.WriteLine("Reading {0} sectors at a time.", blocksToRead);

                mhddLog = new Core.MHDDLog(MHDDLogPath, dev, blocks, blockSize, blocksToRead);
                ibgLog = new Core.IBGLog(IBGLogPath, currentProfile);

                for (ulong i = 0; i < blocks; i += blocksToRead)
                {
                    if (aborted)
                        break;

                    double cmdDuration = 0;

                    if ((blocks - i) < blocksToRead)
                        blocksToRead = (uint)(blocks - i);

                    if (currentSpeed > maxSpeed && currentSpeed != 0)
                        maxSpeed = currentSpeed;
                    if (currentSpeed < minSpeed && currentSpeed != 0)
                        minSpeed = currentSpeed;

                    DicConsole.Write("\rReading sector {0} of {1} ({2:F3} MiB/sec.)", i, blocks, currentSpeed);

                    if (read16)
                    {
                        sense = dev.Read16(out readBuffer, out senseBuf, 0, false, true, false, i, blockSize, 0, blocksToRead, false, dev.Timeout, out cmdDuration);
                        totalDuration += cmdDuration;
                    }
                    else if (read12)
                    {
                        sense = dev.Read12(out readBuffer, out senseBuf, 0, false, false, false, false, (uint)i, blockSize, 0, blocksToRead, false, dev.Timeout, out cmdDuration);
                        totalDuration += cmdDuration;
                    }
                    else if (read10)
                    {
                        sense = dev.Read10(out readBuffer, out senseBuf, 0, false, true, false, false, (uint)i, blockSize, 0, (ushort)blocksToRead, dev.Timeout, out cmdDuration);
                        totalDuration += cmdDuration;
                    }
                    else if (read6)
                    {
                        sense = dev.Read6(out readBuffer, out senseBuf, (uint)i, blockSize, (byte)blocksToRead, dev.Timeout, out cmdDuration);
                        totalDuration += cmdDuration;
                    }

                    if (!sense && !dev.Error)
                    {
                        if (cmdDuration >= 500)
                            F += blocksToRead;
                        else if (cmdDuration >= 150)
                            E += blocksToRead;
                        else if (cmdDuration >= 50)
                            D += blocksToRead;
                        else if (cmdDuration >= 10)
                            C += blocksToRead;
                        else if (cmdDuration >= 3)
                            B += blocksToRead;
                        else
                            A += blocksToRead;

                        mhddLog.Write(i, cmdDuration);
                        ibgLog.Write(i, currentSpeed * 1024);
                    }
                    // TODO: Separate errors on kind of errors.
                    else
                    {
                        errored += blocksToRead;
                        unreadableSectors.Add(i);
                        DicConsole.DebugWriteLine("Media-Scan", "READ error:\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                        if (cmdDuration < 500)
                            mhddLog.Write(i, 65535);
                        else
                            mhddLog.Write(i, cmdDuration);
                        ibgLog.Write(i, 0);
                    }

                    currentSpeed = ((double)blockSize * blocksToRead / (double)1048576) / (cmdDuration / (double)1000);
                }
                end = DateTime.UtcNow;
                DicConsole.WriteLine();
                mhddLog.Close();
                ibgLog.Close(dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024, (((double)blockSize * (double)(blocks + 1)) / 1024) / (totalDuration / 1000), devicePath);
            }

            bool seek6, seek10;

            seek6 = !dev.Seek6(out senseBuf, 0, dev.Timeout, out duration);

            seek10 = !dev.Seek10(out senseBuf, 0, dev.Timeout, out duration);

            double seekMax = double.MinValue;
            double seekMin = double.MaxValue;
            double seekTotal = 0;
            const int seekTimes = 1000;

            double seekCur = 0;

            Random rnd = new Random();

            uint seekPos = (uint)rnd.Next((int)blocks);

            if (seek6)
            {
                dev.Seek6(out senseBuf, seekPos, dev.Timeout, out seekCur);
                DicConsole.WriteLine("Using SCSI SEEK (6) command.");
            }
            else if (seek10)
            {
                dev.Seek10(out senseBuf, seekPos, dev.Timeout, out seekCur);
                DicConsole.WriteLine("Using SCSI SEEK (10) command.");
            }
            else
            {
                if (read16)
                    DicConsole.WriteLine("Using SCSI READ (16) command for seeking.");
                else if (read12)
                    DicConsole.WriteLine("Using SCSI READ (12) command for seeking.");
                else if (read10)
                    DicConsole.WriteLine("Using SCSI READ (10) command for seeking.");
                else if (read6)
                    DicConsole.WriteLine("Using SCSI READ (6) command for seeking.");
            }

            for (int i = 0; i < seekTimes; i++)
            {
                if (aborted)
                    break;
                
                seekPos = (uint)rnd.Next((int)blocks);

                DicConsole.Write("\rSeeking to sector {0}...\t\t", seekPos);

                if (seek6)
                    dev.Seek6(out senseBuf, seekPos, dev.Timeout, out seekCur);
                else if (seek10)
                    dev.Seek10(out senseBuf, seekPos, dev.Timeout, out seekCur);
                else
                {
                    if (read16)
                    {
                        dev.Read16(out readBuffer, out senseBuf, 0, false, true, false, seekPos, blockSize, 0, 1, false, dev.Timeout, out seekCur);
                    }
                    else if (read12)
                    {
                        dev.Read12(out readBuffer, out senseBuf, 0, false, false, false, false, seekPos, blockSize, 0, 1, false, dev.Timeout, out seekCur);
                    }
                    else if (read10)
                    {
                        dev.Read10(out readBuffer, out senseBuf, 0, false, true, false, false, seekPos, blockSize, 0, (ushort)1, dev.Timeout, out seekCur);
                    }
                    else if (read6)
                    {
                        dev.Read6(out readBuffer, out senseBuf, seekPos, blockSize, 1, dev.Timeout, out seekCur);
                    }
                }

                if (seekCur > seekMax && seekCur != 0)
                    seekMax = seekCur;
                if (seekCur < seekMin && seekCur != 0)
                    seekMin = seekCur;

                seekTotal += seekCur;
                GC.Collect();
            }

            DicConsole.WriteLine();

            DicConsole.WriteLine("Took a total of {0} seconds ({1} processing commands).", (end - start).TotalSeconds, totalDuration / 1000);
            DicConsole.WriteLine("Avegare speed: {0:F3} MiB/sec.", (((double)blockSize * (double)(blocks + 1)) / 1048576) / (totalDuration / 1000));
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
            if (unreadableSectors.Count > 0)
            {
                foreach (ulong bad in unreadableSectors)
                    DicConsole.WriteLine("Sector {0} could not be read", bad);
            }
            DicConsole.WriteLine();

            if (seekTotal != 0 || seekMin != double.MaxValue || seekMax != double.MinValue)
                DicConsole.WriteLine("Testing {0} seeks, longest seek took {1:F3} ms, fastest one took {2:F3} ms. ({3:F3} ms average)",
                    seekTimes, seekMax, seekMin, seekTotal / 1000);

            Core.Statistics.AddMediaScan((long)A, (long)B, (long)C, (long)D, (long)E, (long)F, (long)blocks, (long)errored, (long)(blocks - errored));
        }
    }
}

