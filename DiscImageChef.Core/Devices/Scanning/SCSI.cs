// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;
using DiscImageChef.Console;
using DiscImageChef.Core.Logging;
using DiscImageChef.Decoders.CD;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Decoders.SCSI.MMC;
using DiscImageChef.Devices;

namespace DiscImageChef.Core.Devices.Scanning
{
    /// <summary>
    ///     Implements scanning the media from an SCSI device
    /// </summary>
    public static class Scsi
    {
        public static ScanResults Scan(string mhddLogPath, string ibgLogPath, string devicePath, Device dev)
        {
            ScanResults results = new ScanResults();
            bool        aborted;
            MhddLog     mhddLog;
            IbgLog      ibgLog;
            byte[]      senseBuf;
            bool        sense = false;
            results.Blocks = 0;
            uint   blockSize      = 0;
            ushort currentProfile = 0x0001;

            if(dev.IsRemovable)
            {
                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out _);
                if(sense)
                {
                    FixedSense? decSense = Sense.DecodeFixed(senseBuf);
                    if(decSense.HasValue)
                        if(decSense.Value.ASC == 0x3A)
                        {
                            int leftRetries = 5;
                            while(leftRetries > 0)
                            {
                                DicConsole.WriteLine("\rWaiting for drive to become ready");
                                Thread.Sleep(2000);
                                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out _);
                                if(!sense) break;

                                leftRetries--;
                            }

                            if(sense)
                            {
                                DicConsole.ErrorWriteLine("Please insert media in drive");
                                return results;
                            }
                        }
                        else if(decSense.Value.ASC == 0x04 && decSense.Value.ASCQ == 0x01)
                        {
                            int leftRetries = 10;
                            while(leftRetries > 0)
                            {
                                DicConsole.WriteLine("\rWaiting for drive to become ready");
                                Thread.Sleep(2000);
                                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out _);
                                if(!sense) break;

                                leftRetries--;
                            }

                            if(sense)
                            {
                                DicConsole.ErrorWriteLine("Error testing unit was ready:\n{0}",
                                                          Sense.PrettifySense(senseBuf));
                                return results;
                            }
                        }
                        // These should be trapped by the OS but seems in some cases they're not
                        else if(decSense.Value.ASC == 0x28)
                        {
                            int leftRetries = 10;
                            while(leftRetries > 0)
                            {
                                DicConsole.WriteLine("\rWaiting for drive to become ready");
                                Thread.Sleep(2000);
                                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out _);
                                if(!sense) break;

                                leftRetries--;
                            }

                            if(sense)
                            {
                                DicConsole.ErrorWriteLine("Error testing unit was ready:\n{0}",
                                                          Sense.PrettifySense(senseBuf));
                                return results;
                            }
                        }
                        else
                        {
                            DicConsole.ErrorWriteLine("Error testing unit was ready:\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                            return results;
                        }
                    else
                    {
                        DicConsole.ErrorWriteLine("Unknown testing unit was ready.");
                        return results;
                    }
                }
            }

            Reader scsiReader = null;

            switch(dev.ScsiType)
            {
                case PeripheralDeviceTypes.DirectAccess:
                case PeripheralDeviceTypes.MultiMediaDevice:
                case PeripheralDeviceTypes.OCRWDevice:
                case PeripheralDeviceTypes.OpticalDevice:
                case PeripheralDeviceTypes.SimplifiedDevice:
                case PeripheralDeviceTypes.WriteOnceDevice:
                    scsiReader     = new Reader(dev, dev.Timeout, null);
                    results.Blocks = scsiReader.GetDeviceBlocks();
                    if(scsiReader.FindReadCommand())
                    {
                        DicConsole.ErrorWriteLine("Unable to read medium.");
                        return results;
                    }

                    blockSize = scsiReader.LogicalBlockSize;

                    if(results.Blocks != 0 && blockSize != 0)
                    {
                        results.Blocks++;
                        DicConsole.WriteLine("Media has {0} blocks of {1} bytes/each. (for a total of {2} bytes)",
                                             results.Blocks, blockSize, results.Blocks * (ulong)blockSize);
                    }

                    break;
                case PeripheralDeviceTypes.SequentialAccess:
                    DicConsole.WriteLine("Scanning will never be supported on SCSI Streaming Devices.");
                    DicConsole.WriteLine("It has no sense to do it, and it will put too much strain on the tape.");
                    return results;
            }

            if(results.Blocks == 0)
            {
                DicConsole.ErrorWriteLine("Unable to read medium or empty medium present...");
                return results;
            }

            bool               compactDisc = true;
            FullTOC.CDFullTOC? toc         = null;

            if(dev.ScsiType == PeripheralDeviceTypes.MultiMediaDevice)
            {
                sense = dev.GetConfiguration(out byte[] cmdBuf, out senseBuf, 0, MmcGetConfigurationRt.Current,
                                             dev.Timeout, out _);
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
                    bool tocSense     = dev.ReadRawToc(out cmdBuf, out senseBuf, 1, dev.Timeout, out _);
                    if(!tocSense) toc = FullTOC.Decode(cmdBuf);
                }
            }
            else compactDisc = false;

            uint blocksToRead = 64;

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

            aborted                       =  false;
            System.Console.CancelKeyPress += (sender, e) => e.Cancel = aborted = true;

            if(compactDisc)
            {
                if(toc == null)
                {
                    DicConsole.ErrorWriteLine("Error trying to decode TOC...");
                    return results;
                }

                bool readcd = !dev.ReadCd(out _, out senseBuf, 0, 2352, 1, MmcSectorTypes.AllTypes, false, false, true,
                                          MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.None,
                                          dev.Timeout, out _);

                if(readcd) DicConsole.WriteLine("Using MMC READ CD command.");

                start = DateTime.UtcNow;

                while(true)
                {
                    if(readcd)
                    {
                        sense = dev.ReadCd(out _, out senseBuf, 0, 2352, blocksToRead, MmcSectorTypes.AllTypes, false,
                                           false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                           MmcSubchannel.None, dev.Timeout, out _);
                        if(dev.Error) blocksToRead /= 2;
                    }

                    if(!dev.Error || blocksToRead == 1) break;
                }

                if(dev.Error)
                {
                    DicConsole.ErrorWriteLine("Device error {0} trying to guess ideal transfer length.", dev.LastError);
                    return results;
                }

                DicConsole.WriteLine("Reading {0} sectors at a time.", blocksToRead);

                mhddLog = new MhddLog(mhddLogPath, dev, results.Blocks, blockSize, blocksToRead);
                ibgLog  = new IbgLog(ibgLogPath, currentProfile);

                for(ulong i = 0; i < results.Blocks; i += blocksToRead)
                {
                    if(aborted) break;

                    double cmdDuration = 0;

                    if(results.Blocks - i < blocksToRead) blocksToRead = (uint)(results.Blocks - i);

                    #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                    if(currentSpeed > results.MaxSpeed && currentSpeed != 0) results.MaxSpeed = currentSpeed;
                    if(currentSpeed < results.MinSpeed && currentSpeed != 0) results.MinSpeed = currentSpeed;
                    #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                    DicConsole.Write("\rReading sector {0} of {1} ({2:F3} MiB/sec.)", i, results.Blocks, currentSpeed);

                    if(readcd)
                    {
                        sense = dev.ReadCd(out _, out senseBuf, (uint)i, 2352, blocksToRead, MmcSectorTypes.AllTypes,
                                           false, false, true, MmcHeaderCodes.AllHeaders, true, true,
                                           MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out cmdDuration);
                        results.ProcessingTime += cmdDuration;
                    }

                    if(!sense)
                    {
                        if(cmdDuration      >= 500) results.F += blocksToRead;
                        else if(cmdDuration >= 150) results.E += blocksToRead;
                        else if(cmdDuration >= 50) results.D  += blocksToRead;
                        else if(cmdDuration >= 10) results.C  += blocksToRead;
                        else if(cmdDuration >= 3) results.B   += blocksToRead;
                        else results.A                        += blocksToRead;

                        mhddLog.Write(i, cmdDuration);
                        ibgLog.Write(i, currentSpeed * 1024);
                    }
                    else
                    {
                        DicConsole.DebugWriteLine("Media-Scan", "READ CD error:\n{0}", Sense.PrettifySense(senseBuf));

                        FixedSense? senseDecoded = Sense.DecodeFixed(senseBuf);
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
                                for(ulong b = i; b < i + blocksToRead; b++) results.UnreadableSectors.Add(b);

                                mhddLog.Write(i, cmdDuration < 500 ? 65535 : cmdDuration);

                                ibgLog.Write(i, 0);
                            }
                        }
                        else
                        {
                            results.Errored += blocksToRead;
                            for(ulong b = i; b < i + blocksToRead; b++) results.UnreadableSectors.Add(b);

                            mhddLog.Write(i, cmdDuration < 500 ? 65535 : cmdDuration);

                            ibgLog.Write(i, 0);
                        }
                    }

                    double newSpeed =
                        (double)blockSize * blocksToRead / 1048576 / (cmdDuration / 1000);
                    if(!double.IsInfinity(newSpeed)) currentSpeed = newSpeed;
                }

                end = DateTime.UtcNow;
                DicConsole.WriteLine();
                mhddLog.Close();
                ibgLog.Close(dev, results.Blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024,
                             blockSize * (double)(results.Blocks + 1) / 1024 /
                             (results.ProcessingTime / 1000),
                             devicePath);
            }
            else
            {
                start = DateTime.UtcNow;

                DicConsole.WriteLine("Reading {0} sectors at a time.", blocksToRead);

                mhddLog = new MhddLog(mhddLogPath, dev, results.Blocks, blockSize, blocksToRead);
                ibgLog  = new IbgLog(ibgLogPath, currentProfile);

                for(ulong i = 0; i < results.Blocks; i += blocksToRead)
                {
                    if(aborted) break;

                    if(results.Blocks - i < blocksToRead) blocksToRead = (uint)(results.Blocks - i);

                    #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                    if(currentSpeed > results.MaxSpeed && currentSpeed != 0) results.MaxSpeed = currentSpeed;
                    if(currentSpeed < results.MinSpeed && currentSpeed != 0) results.MinSpeed = currentSpeed;
                    #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                    DicConsole.Write("\rReading sector {0} of {1} ({2:F3} MiB/sec.)", i, results.Blocks, currentSpeed);

                    sense                  =  scsiReader.ReadBlocks(out _, i, blocksToRead, out double cmdDuration);
                    results.ProcessingTime += cmdDuration;

                    if(!sense && !dev.Error)
                    {
                        if(cmdDuration      >= 500) results.F += blocksToRead;
                        else if(cmdDuration >= 150) results.E += blocksToRead;
                        else if(cmdDuration >= 50) results.D  += blocksToRead;
                        else if(cmdDuration >= 10) results.C  += blocksToRead;
                        else if(cmdDuration >= 3) results.B   += blocksToRead;
                        else results.A                        += blocksToRead;

                        mhddLog.Write(i, cmdDuration);
                        ibgLog.Write(i, currentSpeed * 1024);
                    }
                    // TODO: Separate errors on kind of errors.
                    else
                    {
                        results.Errored += blocksToRead;
                        for(ulong b = i; b < i + blocksToRead; b++) results.UnreadableSectors.Add(b);

                        mhddLog.Write(i, cmdDuration < 500 ? 65535 : cmdDuration);
                        ibgLog.Write(i, 0);
                    }

                    double newSpeed =
                        (double)blockSize * blocksToRead / 1048576 / (cmdDuration / 1000);
                    if(!double.IsInfinity(newSpeed)) currentSpeed = newSpeed;
                }

                end = DateTime.UtcNow;
                DicConsole.WriteLine();
                mhddLog.Close();
                ibgLog.Close(dev, results.Blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024,
                             blockSize * (double)(results.Blocks + 1) / 1024 /
                             (results.ProcessingTime / 1000),
                             devicePath);
            }

            results.SeekMax   = double.MinValue;
            results.SeekMin   = double.MaxValue;
            results.SeekTotal = 0;
            const int SEEK_TIMES = 1000;

            Random rnd = new Random();

            for(int i = 0; i < SEEK_TIMES; i++)
            {
                if(aborted) break;

                uint seekPos = (uint)rnd.Next((int)results.Blocks);

                DicConsole.Write("\rSeeking to sector {0}...\t\t", seekPos);

                double seekCur;
                if(scsiReader.CanSeek) scsiReader.Seek(seekPos, out seekCur);
                else scsiReader.ReadBlock(out _, seekPos, out seekCur);

                #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                if(seekCur > results.SeekMax && seekCur != 0) results.SeekMax = seekCur;
                if(seekCur < results.SeekMin && seekCur != 0) results.SeekMin = seekCur;
                #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                results.SeekTotal += seekCur;
                GC.Collect();
            }

            DicConsole.WriteLine();

            results.ProcessingTime /= 1000;
            results.TotalTime      =  (end - start).TotalSeconds;
            results.AvgSpeed       =  blockSize * (double)(results.Blocks + 1) / 1048576 / results.ProcessingTime;
            results.SeekTimes      =  SEEK_TIMES;

            return results;
        }
    }
}