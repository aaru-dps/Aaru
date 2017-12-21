// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using DiscImageChef.Console;
using DiscImageChef.Core.Logging;
using DiscImageChef.Decoders.MMC;
using DiscImageChef.Devices;

namespace DiscImageChef.Core.Devices.Scanning
{
    public static class SecureDigital
    {
        public static ScanResults Scan(string mhddLogPath, string ibgLogPath, string devicePath, Device dev)
        {
            ScanResults results = new ScanResults();
            bool aborted;
            MhddLog mhddLog;
            IbgLog ibgLog;
            byte[] cmdBuf;
            bool sense;
            results.Blocks = 0;
            uint[] response;
            uint timeout = 5;
            double duration = 0;
            ushort currentProfile = 0x0001;
            uint blocksToRead = 128;
            uint blockSize = 512;
            bool byteAddressed = true;

            switch(dev.Type) {
                case DeviceType.MMC:
                {
                    ExtendedCSD ecsd = new ExtendedCSD();
                    CSD csd = new CSD();

                    sense = dev.ReadExtendedCsd(out cmdBuf, out response, timeout, out duration);
                    if(!sense)
                    {
                        ecsd = Decoders.MMC.Decoders.DecodeExtendedCSD(cmdBuf);
                        blocksToRead = ecsd.OptimalReadSize;
                        results.Blocks = ecsd.SectorCount;
                        blockSize = (uint)(ecsd.SectorSize == 1 ? 4096 : 512);
                        // Supposing it's high-capacity MMC if it has Extended CSD...
                        byteAddressed = false;
                    }

                    if(sense || results.Blocks == 0)
                    {
                        sense = dev.ReadCsd(out cmdBuf, out response, timeout, out duration);
                        if(!sense)
                        {
                            csd = Decoders.MMC.Decoders.DecodeCSD(cmdBuf);
                            results.Blocks = (ulong)((csd.Size + 1) * Math.Pow(2, csd.SizeMultiplier + 2));
                            blockSize = (uint)Math.Pow(2, csd.ReadBlockLength);
                        }
                    }
                    break;
                }
                case DeviceType.SecureDigital:
                {
                    Decoders.SecureDigital.CSD csd = new Decoders.SecureDigital.CSD();

                    sense = dev.ReadCsd(out cmdBuf, out response, timeout, out duration);
                    if(!sense)
                    {
                        csd = Decoders.SecureDigital.Decoders.DecodeCSD(cmdBuf);
                        results.Blocks = (ulong)(csd.Structure == 0
                                                     ? (csd.Size + 1) * Math.Pow(2, csd.SizeMultiplier + 2)
                                                     : (csd.Size + 1) * 1024);
                        blockSize = (uint)Math.Pow(2, csd.ReadBlockLength);
                        // Structure >=1 for SDHC/SDXC, so that's block addressed
                        byteAddressed = csd.Structure == 0;
                    }
                    break;
                }
            }

            if(results.Blocks == 0)
            {
                DicConsole.ErrorWriteLine("Unable to get device size.");
                return results;
            }

            sense = true;

            while(true)
            {
                sense = dev.Read(out cmdBuf, out response, 0, blockSize, blocksToRead, byteAddressed, timeout,
                                 out duration);

                if(sense) blocksToRead /= 2;

                if(!sense || blocksToRead == 1) break;
            }

            if(sense)
            {
                blocksToRead = 1;
                DicConsole.ErrorWriteLine("Device error {0} trying to guess ideal transfer length.", dev.LastError);
                return results;
            }

            results.A = 0; // <3ms
            results.B = 0; // >=3ms, <10ms
            results.C = 0; // >=10ms, <50ms
            results.D = 0; // >=50ms, <150ms
            results.E = 0; // >=150ms, <500ms
            results.F = 0; // >=500ms
            results.Errored = 0;
            DateTime start;
            DateTime end;
            results.ProcessingTime = 0;
            double currentSpeed = 0;
            results.MaxSpeed = double.MinValue;
            results.MinSpeed = double.MaxValue;
            results.UnreadableSectors = new List<ulong>();
            results.SeekMax = double.MinValue;
            results.SeekMin = double.MaxValue;
            results.SeekTotal = 0;
            const int SEEK_TIMES = 1000;

            double seekCur = 0;

            Random rnd = new Random();

            uint seekPos = (uint)rnd.Next((int)results.Blocks);

            aborted = false;
            System.Console.CancelKeyPress += (sender, e) => { e.Cancel = aborted = true; };

            DicConsole.WriteLine("Reading {0} sectors at a time.", blocksToRead);

            mhddLog = new MhddLog(mhddLogPath, dev, results.Blocks, blockSize, blocksToRead);
            ibgLog = new IbgLog(ibgLogPath, currentProfile);

            start = DateTime.UtcNow;
            for(ulong i = 0; i < results.Blocks; i += blocksToRead)
            {
                if(aborted) break;

                if(results.Blocks - i < blocksToRead) blocksToRead = (byte)(results.Blocks - i);

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                if(currentSpeed > results.MaxSpeed && currentSpeed != 0) results.MaxSpeed = currentSpeed;
                if(currentSpeed < results.MinSpeed && currentSpeed != 0) results.MinSpeed = currentSpeed;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                DicConsole.Write("\rReading sector {0} of {1} ({2:F3} MiB/sec.)", i, results.Blocks, currentSpeed);

                bool error = dev.Read(out cmdBuf, out response, (uint)i, blockSize, blocksToRead, byteAddressed,
                                      timeout, out duration);

                if(!error)
                {
                    if(duration >= 500) results.F += blocksToRead;
                    else if(duration >= 150) results.E += blocksToRead;
                    else if(duration >= 50) results.D += blocksToRead;
                    else if(duration >= 10) results.C += blocksToRead;
                    else if(duration >= 3) results.B += blocksToRead;
                    else results.A += blocksToRead;

                    mhddLog.Write(i, duration);
                    ibgLog.Write(i, currentSpeed * 1024);
                }
                else
                {
                    results.Errored += blocksToRead;
                    for(ulong b = i; b < i + blocksToRead; b++) results.UnreadableSectors.Add(b);

                    if(duration < 500) mhddLog.Write(i, 65535);
                    else mhddLog.Write(i, duration);

                    ibgLog.Write(i, 0);
                }

#pragma warning disable IDE0004 // Without this specific cast, it gives incorrect values
                currentSpeed = (double)blockSize * blocksToRead / 1048576 / (duration / 1000);
#pragma warning restore IDE0004 // Without this specific cast, it gives incorrect values
                GC.Collect();
            }

            end = DateTime.UtcNow;
            DicConsole.WriteLine();
            mhddLog.Close();
#pragma warning disable IDE0004 // Without this specific cast, it gives incorrect values
            ibgLog.Close(dev, results.Blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024,
                         blockSize * (double)(results.Blocks + 1) / 1024 / (results.ProcessingTime / 1000),
                         devicePath);
#pragma warning restore IDE0004 // Without this specific cast, it gives incorrect values

            for(int i = 0; i < SEEK_TIMES; i++)
            {
                if(aborted) break;

                seekPos = (uint)rnd.Next((int)results.Blocks);

                DicConsole.Write("\rSeeking to sector {0}...\t\t", seekPos);

                dev.Read(out cmdBuf, out response, seekPos, blockSize, blocksToRead, byteAddressed, timeout,
                         out seekCur);

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                if(seekCur > results.SeekMax && seekCur != 0) results.SeekMax = seekCur;
                if(seekCur < results.SeekMin && seekCur != 0) results.SeekMin = seekCur;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                results.SeekTotal += seekCur;
                GC.Collect();
            }

            DicConsole.WriteLine();

            results.ProcessingTime /= 1000;
            results.TotalTime = (end - start).TotalSeconds;
#pragma warning disable IDE0004 // Without this specific cast, it gives incorrect values
            results.AvgSpeed = blockSize * (double)(results.Blocks + 1) / 1048576 / results.ProcessingTime;
#pragma warning restore IDE0004 // Without this specific cast, it gives incorrect values
            results.SeekTimes = SEEK_TIMES;

            return results;
        }
    }
}