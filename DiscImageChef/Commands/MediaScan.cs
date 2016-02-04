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
using System.IO;
using DiscImageChef.Devices;
using System.Text;
using System.Collections.Generic;
using System.Globalization;

namespace DiscImageChef.Commands
{
    public static class MediaScan
    {
        static bool aborted;
        static FileStream mhddFs;
        static FileStream ibgFs;
        static StringBuilder ibgSb;
        static DateTime ibgDatePoint;
        static CultureInfo ibgCulture;
        static double ibgStartSpeed;
        static string ibgMediaType;
        static double ibgDivider;
        static bool ibgStartSet;
        static double ibgMaxSpeed;
        static double ibgIntSpeed;
        static int ibgSnaps;
        static ulong                 ibgIntSector = 0;
        static int ibgSampleRate;

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

            mhddFs = null;
            ibgFs = null;

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
            throw new NotImplementedException("ATA devices not yet supported.");
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

                initMHDDLogFile(MHDDLogPath, dev, blocks, blockSize, blocksToRead);
                initIBGLogFile(IBGLogPath, currentProfile);

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

                        writeMHDDLogFile(i, cmdDuration);
                        writeIBGLogFile(i, currentSpeed * 1024);
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
                                    writeMHDDLogFile(i, 65535);
                                else
                                    writeMHDDLogFile(i, cmdDuration);
                                    
                                writeIBGLogFile(i, 0);
                            }
                        }
                        else
                        {
                            errored += blocksToRead;
                            unreadableSectors.Add(i);
                            if (cmdDuration < 500)
                                writeMHDDLogFile(i, 65535);
                            else
                                writeMHDDLogFile(i, cmdDuration);
                                
                            writeIBGLogFile(i, 0);
                        }
                    }

                    currentSpeed = ((double)blockSize * blocksToRead / (double)1048576) / (cmdDuration / (double)1000);
                    GC.Collect();
                }
                end = DateTime.UtcNow;
                DicConsole.WriteLine();
                closeMHDDLogFile();
                closeIBGLogFile(dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024, (((double)blockSize * (double)(blocks + 1)) / 1024) / (totalDuration / 1000), devicePath);
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

                initMHDDLogFile(MHDDLogPath, dev, blocks, blockSize, blocksToRead);
                initIBGLogFile(IBGLogPath, currentProfile);

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

                        writeMHDDLogFile(i, cmdDuration);
                        writeIBGLogFile(i, currentSpeed * 1024);
                    }
                    // TODO: Separate errors on kind of errors.
                    else
                    {
                        errored += blocksToRead;
                        unreadableSectors.Add(i);
                        DicConsole.DebugWriteLine("Media-Scan", "READ error:\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                        if (cmdDuration < 500)
                            writeMHDDLogFile(i, 65535);
                        else
                            writeMHDDLogFile(i, cmdDuration);
                        writeIBGLogFile(i, 0);
                    }

                    currentSpeed = ((double)blockSize * blocksToRead / (double)1048576) / (cmdDuration / (double)1000);
                }
                end = DateTime.UtcNow;
                DicConsole.WriteLine();
                closeMHDDLogFile();
                closeIBGLogFile(dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024, (((double)blockSize * (double)(blocks + 1)) / 1024) / (totalDuration / 1000), devicePath);
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
            DicConsole.WriteLine("{0} sectors could not be read.", errored);
            if (unreadableSectors.Count > 0)
            {
                foreach (ulong bad in unreadableSectors)
                    DicConsole.WriteLine("Sector {0} could not be read", bad);
            }
            DicConsole.WriteLine();

            if (seekTotal != 0 || seekMin != double.MaxValue || seekMax != double.MinValue)
                DicConsole.WriteLine("Testing {0} seeks, longest seek took {1} ms, fastest one took {2} ms. ({3} ms average)",
                    seekTimes, seekMax, seekMin, seekTotal / 1000);

            Core.Statistics.AddMediaScan((long)A, (long)B, (long)C, (long)D, (long)E, (long)F, (long)blocks, (long)errored, (long)(blocks - errored));
        }

        static void initMHDDLogFile(string outputFile, Device dev, ulong blocks, ulong blockSize, ulong blocksToRead)
        {
            if (dev != null && !string.IsNullOrEmpty(outputFile))
            {
                mhddFs = new FileStream(outputFile, FileMode.Create);

                string device;
                string mode;
                string fw;
                string sn;
                string sectors;
                string sectorsize;
                string scanblocksize;
                string ver;

                switch (dev.Type)
                {
                    case DeviceType.ATA:
                    case DeviceType.ATAPI:
                        mode = "MODE: IDE";
                        break;
                    case DeviceType.SCSI:
                        mode = "MODE: SCSI";
                        break;
                    case DeviceType.MMC:
                        mode = "MODE: MMC";
                        break;
                    case DeviceType.NVMe:
                        mode = "MODE: NVMe";
                        break;
                    case DeviceType.SecureDigital:
                        mode = "MODE: SD";
                        break;
                    default:
                        mode = "MODE: IDE";
                        break;
                }

                device = String.Format("DEVICE: {0} {1}", dev.Manufacturer, dev.Model);
                fw = String.Format("F/W: {0}", dev.Revision);
                sn = String.Format("S/N: {0}", dev.Serial);
                sectors = String.Format(new System.Globalization.CultureInfo("en-US"), "SECTORS: {0:n0}", blocks);
                sectorsize = String.Format(new System.Globalization.CultureInfo("en-US"), "SECTOR SIZE: {0:n0} bytes", blockSize);
                scanblocksize = String.Format(new System.Globalization.CultureInfo("en-US"), "SCAN BLOCK SIZE: {0:n0} sectors", blocksToRead);
                ver = "VER:2 ";

                byte[] deviceBytes = Encoding.ASCII.GetBytes(device);
                byte[] modeBytes = Encoding.ASCII.GetBytes(mode);
                byte[] fwBytes = Encoding.ASCII.GetBytes(fw);
                byte[] snBytes = Encoding.ASCII.GetBytes(sn);
                byte[] sectorsBytes = Encoding.ASCII.GetBytes(sectors);
                byte[] sectorsizeBytes = Encoding.ASCII.GetBytes(sectorsize);
                byte[] scanblocksizeBytes = Encoding.ASCII.GetBytes(scanblocksize);
                byte[] verBytes = Encoding.ASCII.GetBytes(ver);

                uint Pointer = (uint)(deviceBytes.Length + modeBytes.Length + fwBytes.Length +
                               snBytes.Length + sectorsBytes.Length + sectorsizeBytes.Length +
                               scanblocksizeBytes.Length + verBytes.Length +
                               2 * 9 + // New lines
                               4); // Pointer

                byte[] newLine = new byte[2];
                newLine[0] = 0x0D;
                newLine[1] = 0x0A;

                mhddFs.Write(BitConverter.GetBytes(Pointer), 0, 4);
                mhddFs.Write(newLine, 0, 2);
                mhddFs.Write(verBytes, 0, verBytes.Length);
                mhddFs.Write(newLine, 0, 2);
                mhddFs.Write(modeBytes, 0, modeBytes.Length);
                mhddFs.Write(newLine, 0, 2);
                mhddFs.Write(deviceBytes, 0, deviceBytes.Length);
                mhddFs.Write(newLine, 0, 2);
                mhddFs.Write(fwBytes, 0, fwBytes.Length);
                mhddFs.Write(newLine, 0, 2);
                mhddFs.Write(snBytes, 0, snBytes.Length);
                mhddFs.Write(newLine, 0, 2);
                mhddFs.Write(sectorsBytes, 0, sectorsBytes.Length);
                mhddFs.Write(newLine, 0, 2);
                mhddFs.Write(sectorsizeBytes, 0, sectorsizeBytes.Length);
                mhddFs.Write(newLine, 0, 2);
                mhddFs.Write(scanblocksizeBytes, 0, scanblocksizeBytes.Length);
                mhddFs.Write(newLine, 0, 2);
            }
        }

        static void closeMHDDLogFile()
        {
            if (mhddFs != null)
                mhddFs.Close();
        }

        static void writeMHDDLogFile(ulong sector, double duration)
        {
            if (mhddFs != null)
            {
                byte[] sectorBytes = BitConverter.GetBytes(sector);
                byte[] durationBytes = BitConverter.GetBytes((ulong)(duration * 1000));

                mhddFs.Write(sectorBytes, 0, 8);
                mhddFs.Write(durationBytes, 0, 8);
            }
        }

        static void writeIBGLogFile(ulong sector, double currentSpeed)
        {
            if (ibgFs != null)
            {
                ibgIntSpeed += currentSpeed;
                ibgSampleRate += (int)Math.Floor((DateTime.Now - ibgDatePoint).TotalMilliseconds);
                ibgSnaps++;
                
                if(ibgSampleRate >= 100)
                {
                if (ibgIntSpeed > 0 && !ibgStartSet)
                {
                    ibgStartSpeed = ibgIntSpeed / ibgSnaps / ibgDivider;
                    ibgStartSet = true;
                }
                
                ibgSb.AppendFormat("{0:0.00},{1},{2:0},0", ibgIntSpeed / ibgSnaps / ibgDivider, ibgIntSector, ibgSampleRate).AppendLine();
                if ((ibgIntSpeed / ibgSnaps / ibgDivider) > ibgMaxSpeed)
                    ibgMaxSpeed = ibgIntSpeed / ibgDivider;
                    
                ibgDatePoint = DateTime.Now;
                    ibgIntSpeed = 0;
                    ibgSampleRate = 0;
                    ibgSnaps = 0;
                    ibgIntSector = sector;
                }
            }
        }

        static void initIBGLogFile(string outputFile, ushort currentProfile)
        {
            if (!string.IsNullOrEmpty(outputFile))
            {
                ibgFs = new FileStream(outputFile, FileMode.Create);
                ibgSb = new StringBuilder();
                ibgDatePoint = DateTime.Now;
                ibgCulture = new CultureInfo("en-US");
                ibgStartSet = false;
                ibgMaxSpeed = 0;
                ibgIntSpeed = 0;
                ibgSnaps = 0;
                ibgIntSector = 0;

                switch (currentProfile)
                {
                    case 0x0001:
                        ibgMediaType = "HDD";
                        ibgDivider = 1353;
                        break;
                    case 0x0005:
                        ibgMediaType = "CD-MO";
                        ibgDivider = 150;
                        break;
                    case 0x0008:
                        ibgMediaType = "CD-ROM";
                        ibgDivider = 150;
                        break;
                    case 0x0009:
                        ibgMediaType = "CD-R";
                        ibgDivider = 150;
                        break;
                    case 0x000A:
                        ibgMediaType = "CD-RW";
                        ibgDivider = 150;
                        break;
                    case 0x0010:
                        ibgMediaType = "DVD-ROM";
                        ibgDivider = 1353;
                        break;
                    case 0x0011:
                        ibgMediaType = "DVD-R";
                        ibgDivider = 1353;
                        break;
                    case 0x0012:
                        ibgMediaType = "DVD-RAM";
                        ibgDivider = 1353;
                        break;
                    case 0x0013:
                    case 0x0014:
                        ibgMediaType = "DVD-RW";
                        ibgDivider = 1353;
                        break;
                    case 0x0015:
                    case 0x0016:
                        ibgMediaType = "DVD-R DL";
                        ibgDivider = 1353;
                        break;
                    case 0x0017:
                        ibgMediaType = "DVD-RW DL";
                        ibgDivider = 1353;
                        break;
                    case 0x0018:
                        ibgMediaType = "DVD-Download";
                        ibgDivider = 1353;
                        break;
                    case 0x001A:
                        ibgMediaType = "DVD+RW";
                        ibgDivider = 1353;
                        break;
                    case 0x001B:
                        ibgMediaType = "DVD+R";
                        ibgDivider = 1353;
                        break;
                    case 0x0020:
                        ibgMediaType = "DDCD-ROM";
                        ibgDivider = 150;
                        break;
                    case 0x0021:
                        ibgMediaType = "DDCD-R";
                        ibgDivider = 150;
                        break;
                    case 0x0022:
                        ibgMediaType = "DDCD-RW";
                        ibgDivider = 150;
                        break;
                    case 0x002A:
                        ibgMediaType = "DVD+RW DL";
                        ibgDivider = 1353;
                        break;
                    case 0x002B:
                        ibgMediaType = "DVD+R DL";
                        ibgDivider = 1353;
                        break;
                    case 0x0040:
                        ibgMediaType = "BD-ROM";
                        ibgDivider = 4500;
                        break;
                    case 0x0041:
                    case 0x0042:
                        ibgMediaType = "BD-R";
                        ibgDivider = 4500;
                        break;
                    case 0x0043:
                        ibgMediaType = "BD-RE";
                        ibgDivider = 4500;
                        break;
                    case 0x0050:
                        ibgMediaType = "HD DVD-ROM";
                        ibgDivider = 4500;
                        break;
                    case 0x0051:
                        ibgMediaType = "HD DVD-R";
                        ibgDivider = 4500;
                        break;
                    case 0x0052:
                        ibgMediaType = "HD DVD-RAM";
                        ibgDivider = 4500;
                        break;
                    case 0x0053:
                        ibgMediaType = "HD DVD-RW";
                        ibgDivider = 4500;
                        break;
                    case 0x0058:
                        ibgMediaType = "HD DVD-R DL";
                        ibgDivider = 4500;
                        break;
                    case 0x005A:
                        ibgMediaType = "HD DVD-RW DL";
                        ibgDivider = 4500;
                        break;
                    default:
                        ibgMediaType = "Unknown";
                        ibgDivider = 1353;
                        break;
                }

            }
        }

        static void closeIBGLogFile(Device dev, ulong blocks, ulong blockSize, double totalSeconds, double currentSpeed, double averageSpeed, string devicePath)
        {
            if (ibgFs != null)
            {
                StringBuilder ibgHeader = new StringBuilder();
                string ibgBusType;
                
                if (dev.IsUSB)
                    ibgBusType = "USB";
                else if (dev.IsFireWire)
                    ibgBusType = "FireWire";
                else
                    ibgBusType = dev.Type.ToString();
                                
                ibgHeader.AppendLine("IBGD");
                ibgHeader.AppendLine();
                ibgHeader.AppendLine("[START_CONFIGURATION]");
                ibgHeader.AppendLine("IBGD_VERSION=2");
                ibgHeader.AppendLine();
                ibgHeader.AppendFormat("DATE={0}", DateTime.Now).AppendLine();
                ibgHeader.AppendLine();
                ibgHeader.AppendFormat("SAMPLE_RATE={0}", 100).AppendLine();
                
                ibgHeader.AppendLine();
                ibgHeader.AppendFormat("DEVICE=[0:0:0] {0} {1} ({2}) ({3})",
                    dev.Manufacturer, dev.Model, devicePath, ibgBusType).AppendLine();
                ibgHeader.AppendLine("DEVICE_ADDRESS=0:0:0");
                ibgHeader.AppendFormat("DEVICE_MAKEMODEL={0} {1}", dev.Manufacturer, dev.Model).AppendLine();
                ibgHeader.AppendFormat("DEVICE_FIRMWAREVERSION={0}", dev.Revision).AppendLine();
                ibgHeader.AppendFormat("DEVICE_DRIVELETTER={0}", devicePath).AppendLine();
                ibgHeader.AppendFormat("DEVICE_BUSTYPE={0}", ibgBusType).AppendLine();
                ibgHeader.AppendLine();
                
                ibgHeader.AppendFormat("MEDIA_TYPE={0}", ibgMediaType).AppendLine();
                ibgHeader.AppendLine("MEDIA_BOOKTYPE=Unknown");
                ibgHeader.AppendLine("MEDIA_ID=N/A");
                ibgHeader.AppendLine("MEDIA_TRACKPATH=PTP");
                ibgHeader.AppendLine("MEDIA_SPEEDS=N/A");
                ibgHeader.AppendFormat("MEDIA_CAPACITY={0}", blocks).AppendLine();
                ibgHeader.AppendLine("MEDIA_LAYER_BREAK=0");
                ibgHeader.AppendLine();
                ibgHeader.AppendLine("DATA_IMAGEFILE=/dev/null");
                ibgHeader.AppendFormat("DATA_SECTORS={0}", blocks).AppendLine();
                ibgHeader.AppendFormat("DATA_TYPE=MODE1/{0}", blockSize).AppendLine();
                ibgHeader.AppendLine("DATA_VOLUMEIDENTIFIER=");
                ibgHeader.AppendLine();
                ibgHeader.AppendFormat(ibgCulture, "VERIFY_SPEED_START={0:0.00}", ibgStartSpeed).AppendLine();
                ibgHeader.AppendFormat(ibgCulture, "VERIFY_SPEED_END={0:0.00}", currentSpeed / ibgDivider).AppendLine();
                ibgHeader.AppendFormat(ibgCulture, "VERIFY_SPEED_AVERAGE={0:0.00}", averageSpeed / ibgDivider).AppendLine();
                ibgHeader.AppendFormat(ibgCulture, "VERIFY_SPEED_MAX={0:0.00}", ibgMaxSpeed).AppendLine();
                ibgHeader.AppendFormat(ibgCulture, "VERIFY_TIME_TAKEN={0:0}", Math.Floor(totalSeconds)).AppendLine();
                ibgHeader.AppendLine("[END_CONFIGURATION]");
                ibgHeader.AppendLine();
                ibgHeader.AppendLine("HRPC=True");
                ibgHeader.AppendLine();
                ibgHeader.AppendLine("[START_VERIFY_GRAPH_VALUES]");
                ibgHeader.Append(ibgSb.ToString());
                ibgHeader.AppendLine("[END_VERIFY_GRAPH_VALUES]");
                ibgHeader.AppendLine();
                ibgHeader.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
                    
                StreamWriter sr = new StreamWriter(ibgFs);
                sr.Write(ibgHeader.ToString());
                sr.Close();
                ibgFs.Close();
            }
        }
    }
}

