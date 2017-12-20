// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : XGD.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps Xbox Game Discs.
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
using System.IO;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Core.Logging;
using DiscImageChef.Devices;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using DiscImageChef.DiscImages;
using DiscImageChef.Partitions;
using Extents;
using Schemas;

namespace DiscImageChef.Core.Devices.Dumping
{
    static class Xgd
    {
        internal static void Dump(Device dev, string devicePath, string outputPrefix, ushort retryPasses, bool force,
                                  bool dumpRaw, bool persistent, bool stopOnError, ref CICMMetadataType sidecar,
                                  ref MediaType dskType, ref Metadata.Resume resume, ref DumpLog dumpLog,
                                  Encoding encoding)
        {
            MhddLog mhddLog;
            IbgLog ibgLog;
            bool sense = false;
            ulong blocks = 0;
            uint blockSize = 2048;
            uint blocksToRead = 64;
            ulong errored = 0;
            DateTime start;
            DateTime end;
            double totalDuration = 0;
            double totalChkDuration = 0;
            double currentSpeed = 0;
            double maxSpeed = double.MinValue;
            double minSpeed = double.MaxValue;
            Checksum dataChk;
            DataFile dumpFile = null;
            bool aborted = false;
            System.Console.CancelKeyPress += (sender, e) => { e.Cancel = aborted = true; };

            dumpLog.WriteLine("Reading Xbox Security Sector.");
            sense = dev.KreonExtractSs(out byte[] ssBuf, out byte[] senseBuf, dev.Timeout, out double duration);
            if(sense)
            {
                dumpLog.WriteLine("Cannot get Xbox Security Sector, not continuing.");
                DicConsole.ErrorWriteLine("Cannot get Xbox Security Sector, not continuing.");
                return;
            }

            dumpLog.WriteLine("Decoding Xbox Security Sector.");
            Decoders.Xbox.SS.SecuritySector? xboxSs = Decoders.Xbox.SS.Decode(ssBuf);
            if(!xboxSs.HasValue)
            {
                dumpLog.WriteLine("Cannot decode Xbox Security Sector, not continuing.");
                DicConsole.ErrorWriteLine("Cannot decode Xbox Security Sector, not continuing.");
                return;
            }

            byte[] tmpBuf = new byte[ssBuf.Length - 4];
            Array.Copy(ssBuf, 4, tmpBuf, 0, ssBuf.Length - 4);
            sidecar.OpticalDisc[0].Xbox = new XboxType()
            {
                SecuritySectors = new XboxSecuritySectorsType[]
                {
                    new XboxSecuritySectorsType()
                    {
                        RequestNumber = 0,
                        RequestVersion = 1,
                        SecuritySectors = new DumpType()
                        {
                            Image = outputPrefix + ".ss.bin",
                            Size = tmpBuf.Length,
                            Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
                        }
                    }
                }
            };
            DataFile.WriteTo("SCSI Dump", outputPrefix + ".ss.bin", ssBuf);

            ulong l0Video, l1Video, middleZone, gameSize, totalSize, layerBreak;

            // Get video partition size
            DicConsole.DebugWriteLine("Dump-media command", "Getting video partition size");
            dumpLog.WriteLine("Locking drive.");
            sense = dev.KreonLock(out senseBuf, dev.Timeout, out duration);
            if(sense)
            {
                dumpLog.WriteLine("Cannot lock drive, not continuing.");
                DicConsole.ErrorWriteLine("Cannot lock drive, not continuing.");
                return;
            }

            dumpLog.WriteLine("Getting video partition size.");
            sense = dev.ReadCapacity(out byte[] readBuffer, out senseBuf, dev.Timeout, out duration);
            if(sense)
            {
                dumpLog.WriteLine("Cannot get disc capacity.");
                DicConsole.ErrorWriteLine("Cannot get disc capacity.");
                return;
            }

            totalSize = (ulong)((readBuffer[0] << 24) + (readBuffer[1] << 16) + (readBuffer[2] << 8) + readBuffer[3]);
            dumpLog.WriteLine("Reading Physical Format Information.");
            sense = dev.ReadDiscStructure(out readBuffer, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                          MmcDiscStructureFormat.PhysicalInformation, 0, 0, out duration);
            if(sense)
            {
                dumpLog.WriteLine("Cannot get PFI.");
                DicConsole.ErrorWriteLine("Cannot get PFI.");
                return;
            }

            tmpBuf = new byte[readBuffer.Length - 4];
            Array.Copy(readBuffer, 4, tmpBuf, 0, readBuffer.Length - 4);
            sidecar.OpticalDisc[0].PFI = new DumpType
            {
                Image = outputPrefix + ".pfi.bin",
                Size = tmpBuf.Length,
                Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
            };
            DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].PFI.Image, tmpBuf, "Locked PFI", true);
            DicConsole.DebugWriteLine("Dump-media command", "Video partition total size: {0} sectors", totalSize);
            l0Video = Decoders.DVD.PFI.Decode(readBuffer).Value.Layer0EndPSN -
                      Decoders.DVD.PFI.Decode(readBuffer).Value.DataAreaStartPSN + 1;
            l1Video = totalSize - l0Video + 1;
            dumpLog.WriteLine("Reading Disc Manufacturing Information.");
            sense = dev.ReadDiscStructure(out readBuffer, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                          MmcDiscStructureFormat.DiscManufacturingInformation, 0, 0, out duration);
            if(sense)
            {
                dumpLog.WriteLine("Cannot get DMI.");
                DicConsole.ErrorWriteLine("Cannot get DMI.");
                return;
            }

            tmpBuf = new byte[readBuffer.Length - 4];
            Array.Copy(readBuffer, 4, tmpBuf, 0, readBuffer.Length - 4);
            sidecar.OpticalDisc[0].DMI = new DumpType
            {
                Image = outputPrefix + ".dmi.bin",
                Size = tmpBuf.Length,
                Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
            };
            DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].DMI.Image, tmpBuf, "Locked DMI", true);

            // Get game partition size
            DicConsole.DebugWriteLine("Dump-media command", "Getting game partition size");
            dumpLog.WriteLine("Unlocking drive (Xtreme).");
            sense = dev.KreonUnlockXtreme(out senseBuf, dev.Timeout, out duration);
            if(sense)
            {
                dumpLog.WriteLine("Cannot unlock drive, not continuing.");
                DicConsole.ErrorWriteLine("Cannot unlock drive, not continuing.");
                return;
            }

            dumpLog.WriteLine("Getting game partition size.");
            sense = dev.ReadCapacity(out readBuffer, out senseBuf, dev.Timeout, out duration);
            if(sense)
            {
                dumpLog.WriteLine("Cannot get disc capacity.");
                DicConsole.ErrorWriteLine("Cannot get disc capacity.");
                return;
            }

            gameSize = (ulong)((readBuffer[0] << 24) + (readBuffer[1] << 16) + (readBuffer[2] << 8) + readBuffer[3]) +
                       1;
            DicConsole.DebugWriteLine("Dump-media command", "Game partition total size: {0} sectors", gameSize);

            // Get middle zone size
            DicConsole.DebugWriteLine("Dump-media command", "Getting middle zone size");
            dumpLog.WriteLine("Unlocking drive (Wxripper).");
            sense = dev.KreonUnlockWxripper(out senseBuf, dev.Timeout, out duration);
            if(sense)
            {
                dumpLog.WriteLine("Cannot unlock drive, not continuing.");
                DicConsole.ErrorWriteLine("Cannot unlock drive, not continuing.");
                return;
            }

            dumpLog.WriteLine("Getting disc size.");
            sense = dev.ReadCapacity(out readBuffer, out senseBuf, dev.Timeout, out duration);
            if(sense)
            {
                dumpLog.WriteLine("Cannot get disc capacity.");
                DicConsole.ErrorWriteLine("Cannot get disc capacity.");
                return;
            }

            totalSize = (ulong)((readBuffer[0] << 24) + (readBuffer[1] << 16) + (readBuffer[2] << 8) + readBuffer[3]);
            dumpLog.WriteLine("Reading Physical Format Information.");
            sense = dev.ReadDiscStructure(out readBuffer, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                          MmcDiscStructureFormat.PhysicalInformation, 0, 0, out duration);
            if(sense)
            {
                dumpLog.WriteLine("Cannot get PFI.");
                DicConsole.ErrorWriteLine("Cannot get PFI.");
                return;
            }

            DicConsole.DebugWriteLine("Dump-media command", "Unlocked total size: {0} sectors", totalSize);
            blocks = totalSize + 1;
            middleZone = totalSize - (Decoders.DVD.PFI.Decode(readBuffer).Value.Layer0EndPSN -
                                      Decoders.DVD.PFI.Decode(readBuffer).Value.DataAreaStartPSN + 1) - gameSize + 1;

            tmpBuf = new byte[readBuffer.Length - 4];
            Array.Copy(readBuffer, 4, tmpBuf, 0, readBuffer.Length - 4);
            sidecar.OpticalDisc[0].Xbox.PFI = new DumpType
            {
                Image = outputPrefix + ".xboxpfi.bin",
                Size = tmpBuf.Length,
                Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
            };
            DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].Xbox.PFI.Image, tmpBuf, "Unlocked PFI", true);

            dumpLog.WriteLine("Reading Disc Manufacturing Information.");
            sense = dev.ReadDiscStructure(out readBuffer, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                          MmcDiscStructureFormat.DiscManufacturingInformation, 0, 0, out duration);
            if(sense)
            {
                dumpLog.WriteLine("Cannot get DMI.");
                DicConsole.ErrorWriteLine("Cannot get DMI.");
                return;
            }

            tmpBuf = new byte[readBuffer.Length - 4];
            Array.Copy(readBuffer, 4, tmpBuf, 0, readBuffer.Length - 4);
            sidecar.OpticalDisc[0].Xbox.DMI = new DumpType
            {
                Image = outputPrefix + ".xboxdmi.bin",
                Size = tmpBuf.Length,
                Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
            };
            DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].Xbox.DMI.Image, tmpBuf, "Unlocked DMI", true);

            totalSize = l0Video + l1Video + middleZone * 2 + gameSize;
            layerBreak = l0Video + middleZone + gameSize / 2;

            DicConsole.WriteLine("Video layer 0 size: {0} sectors", l0Video);
            DicConsole.WriteLine("Video layer 1 size: {0} sectors", l1Video);
            DicConsole.WriteLine("Middle zone size: {0} sectors", middleZone);
            DicConsole.WriteLine("Game data size: {0} sectors", gameSize);
            DicConsole.WriteLine("Total size: {0} sectors", totalSize);
            DicConsole.WriteLine("Real layer break: {0}", layerBreak);
            DicConsole.WriteLine();

            dumpLog.WriteLine("Video layer 0 size: {0} sectors", l0Video);
            dumpLog.WriteLine("Video layer 1 size: {0} sectors", l1Video);
            dumpLog.WriteLine("Middle zone 0 size: {0} sectors", middleZone);
            dumpLog.WriteLine("Game data 0 size: {0} sectors", gameSize);
            dumpLog.WriteLine("Total 0 size: {0} sectors", totalSize);
            dumpLog.WriteLine("Real layer break: {0}", layerBreak);

            bool read12 = !dev.Read12(out readBuffer, out senseBuf, 0, false, true, false, false, 0, blockSize, 0, 1,
                                      false, dev.Timeout, out duration);
            if(!read12)
            {
                dumpLog.WriteLine("Cannot read medium, aborting scan...");
                DicConsole.ErrorWriteLine("Cannot read medium, aborting scan...");
                return;
            }

            dumpLog.WriteLine("Using SCSI READ (12) command.");
            DicConsole.WriteLine("Using SCSI READ (12) command.");

            while(true)
            {
                if(read12)
                {
                    sense = dev.Read12(out readBuffer, out senseBuf, 0, false, false, false, false, 0, blockSize, 0,
                                       blocksToRead, false, dev.Timeout, out duration);
                    if(sense || dev.Error) blocksToRead /= 2;
                }

                if(!dev.Error || blocksToRead == 1) break;
            }

            if(dev.Error)
            {
                dumpLog.WriteLine("Device error {0} trying to guess ideal transfer length.", dev.LastError);
                DicConsole.ErrorWriteLine("Device error {0} trying to guess ideal transfer length.", dev.LastError);
                return;
            }

            dumpLog.WriteLine("Reading {0} sectors at a time.", blocksToRead);
            DicConsole.WriteLine("Reading {0} sectors at a time.", blocksToRead);

            mhddLog = new MhddLog(outputPrefix + ".mhddlog.bin", dev, blocks, blockSize, blocksToRead);
            ibgLog = new IbgLog(outputPrefix + ".ibg", 0x0010);
            dumpFile = new DataFile(outputPrefix + ".iso");

            start = DateTime.UtcNow;

            readBuffer = null;

            double cmdDuration = 0;
            uint saveBlocksToRead = blocksToRead;
            DumpHardwareType currentTry = null;
            ExtentsULong extents = null;
            ResumeSupport.Process(true, true, totalSize, dev.Manufacturer, dev.Model, dev.Serial, dev.PlatformId,
                                  ref resume, ref currentTry, ref extents);
            if(currentTry == null || extents == null)
                throw new Exception("Could not process resume file, not continuing...");

            ulong currentSector = resume.NextBlock;
            dumpFile.Seek(resume.NextBlock, blockSize);
            if(resume.NextBlock > 0) dumpLog.WriteLine("Resuming from block {0}.", resume.NextBlock);

            dumpLog.WriteLine("Reading game partition.");
            for(int e = 0; e <= 16; e++)
            {
                if(aborted)
                {
                    resume.NextBlock = currentSector;
                    currentTry.Extents = Metadata.ExtentsConverter.ToMetadata(extents);
                    dumpLog.WriteLine("Aborted!");
                    break;
                }

                if(currentSector >= blocks) break;

                ulong extentStart, extentEnd;
                // Extents
                if(e < 16)
                {
                    if(xboxSs.Value.Extents[e].StartPSN <= xboxSs.Value.Layer0EndPSN)
                        extentStart = xboxSs.Value.Extents[e].StartPSN - 0x30000;
                    else
                        extentStart = (xboxSs.Value.Layer0EndPSN + 1) * 2 -
                                      ((xboxSs.Value.Extents[e].StartPSN ^ 0xFFFFFF) + 1) - 0x30000;
                    if(xboxSs.Value.Extents[e].EndPSN <= xboxSs.Value.Layer0EndPSN)
                        extentEnd = xboxSs.Value.Extents[e].EndPSN - 0x30000;
                    else
                        extentEnd = (xboxSs.Value.Layer0EndPSN + 1) * 2 -
                                    ((xboxSs.Value.Extents[e].EndPSN ^ 0xFFFFFF) + 1) - 0x30000;
                }
                // After last extent
                else
                {
                    extentStart = blocks;
                    extentEnd = blocks;
                }

                if(currentSector > extentEnd) continue;

                for(ulong i = currentSector; i < extentStart; i += blocksToRead)
                {
                    saveBlocksToRead = blocksToRead;

                    if(aborted)
                    {
                        currentTry.Extents = Metadata.ExtentsConverter.ToMetadata(extents);
                        dumpLog.WriteLine("Aborted!");
                        break;
                    }

                    if(extentStart - i < blocksToRead) blocksToRead = (uint)(extentStart - i);

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                    if(currentSpeed > maxSpeed && currentSpeed != 0) maxSpeed = currentSpeed;
                    if(currentSpeed < minSpeed && currentSpeed != 0) minSpeed = currentSpeed;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                    DicConsole.Write("\rReading sector {0} of {1} ({2:F3} MiB/sec.)", i, totalSize, currentSpeed);

                    sense = dev.Read12(out readBuffer, out senseBuf, 0, false, false, false, false, (uint)i, blockSize,
                                       0, blocksToRead, false, dev.Timeout, out cmdDuration);
                    totalDuration += cmdDuration;

                    if(!sense && !dev.Error)
                    {
                        mhddLog.Write(i, cmdDuration);
                        ibgLog.Write(i, currentSpeed * 1024);
                        dumpFile.Write(readBuffer);
                        extents.Add(i, blocksToRead, true);
                    }
                    else
                    {
                        // TODO: Reset device after X errors
                        if(stopOnError) return; // TODO: Return more cleanly

                        // Write empty data
                        dumpFile.Write(new byte[blockSize * blocksToRead]);

                        errored += blocksToRead;
                        for(ulong b = i; b < i + blocksToRead; b++) resume.BadBlocks.Add(b);

                        DicConsole.DebugWriteLine("Dump-Media", "READ error:\n{0}",
                                                  Decoders.SCSI.Sense.PrettifySense(senseBuf));
                        if(cmdDuration < 500) mhddLog.Write(i, 65535);
                        else mhddLog.Write(i, cmdDuration);

                        ibgLog.Write(i, 0);

                        dumpLog.WriteLine("Error reading {0} blocks from block {1}.", blocksToRead, i);
                        string[] senseLines = Decoders
                            .SCSI.Sense.PrettifySense(senseBuf).Split(new[] {Environment.NewLine},
                                                                      StringSplitOptions.RemoveEmptyEntries);
                        foreach(string senseLine in senseLines) dumpLog.WriteLine(senseLine);
                    }

#pragma warning disable IDE0004 // Remove Unnecessary Cast
                    currentSpeed = (double)blockSize * blocksToRead / (double)1048576 / (cmdDuration / (double)1000);
#pragma warning restore IDE0004 // Remove Unnecessary Cast
                    blocksToRead = saveBlocksToRead;
                    currentSector = i + 1;
                    resume.NextBlock = currentSector;
                }

                for(ulong i = extentStart; i <= extentEnd; i += blocksToRead)
                {
                    saveBlocksToRead = blocksToRead;
                    if(aborted)
                    {
                        currentTry.Extents = Metadata.ExtentsConverter.ToMetadata(extents);
                        dumpLog.WriteLine("Aborted!");
                        break;
                    }

                    if(extentEnd - i < blocksToRead) blocksToRead = (uint)(extentEnd - i) + 1;

                    mhddLog.Write(i, cmdDuration);
                    ibgLog.Write(i, currentSpeed * 1024);
                    dumpFile.Write(new byte[blocksToRead * 2048]);
                    blocksToRead = saveBlocksToRead;
                    extents.Add(i, blocksToRead, true);
                    currentSector = i + 1;
                    resume.NextBlock = currentSector;
                }

                if(!aborted) currentSector = extentEnd + 1;
            }

            // Middle Zone D
            dumpLog.WriteLine("Writing Middle Zone D (empty).");
            for(ulong middle = currentSector - blocks - 1; middle < middleZone - 1; middle += blocksToRead)
            {
                if(aborted)
                {
                    currentTry.Extents = Metadata.ExtentsConverter.ToMetadata(extents);
                    dumpLog.WriteLine("Aborted!");
                    break;
                }

                if(middleZone - 1 - middle < blocksToRead) blocksToRead = (uint)(middleZone - 1 - middle);

                DicConsole.Write("\rReading sector {0} of {1} ({2:F3} MiB/sec.)", middle + currentSector, totalSize,
                                 currentSpeed);

                mhddLog.Write(middle + currentSector, cmdDuration);
                ibgLog.Write(middle + currentSector, currentSpeed * 1024);
                dumpFile.Write(new byte[blockSize * blocksToRead]);
                extents.Add(currentSector, blocksToRead, true);

                currentSector += blocksToRead;
                resume.NextBlock = currentSector;
            }

            blocksToRead = saveBlocksToRead;

            dumpLog.WriteLine("Locking drive.");
            sense = dev.KreonLock(out senseBuf, dev.Timeout, out duration);
            if(sense)
            {
                dumpLog.WriteLine("Cannot lock drive, not continuing.");
                DicConsole.ErrorWriteLine("Cannot lock drive, not continuing.");
                return;
            }

            sense = dev.ReadCapacity(out readBuffer, out senseBuf, dev.Timeout, out duration);
            if(sense)
            {
                DicConsole.ErrorWriteLine("Cannot get disc capacity.");
                return;
            }

            // Video Layer 1
            dumpLog.WriteLine("Reading Video Layer 1.");
            for(ulong l1 = currentSector - blocks - middleZone + l0Video; l1 < l0Video + l1Video; l1 += blocksToRead)
            {
                if(aborted)
                {
                    currentTry.Extents = Metadata.ExtentsConverter.ToMetadata(extents);
                    dumpLog.WriteLine("Aborted!");
                    break;
                }

                if(l0Video + l1Video - l1 < blocksToRead) blocksToRead = (uint)(l0Video + l1Video - l1);

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                if(currentSpeed > maxSpeed && currentSpeed != 0) maxSpeed = currentSpeed;
                if(currentSpeed < minSpeed && currentSpeed != 0) minSpeed = currentSpeed;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                DicConsole.Write("\rReading sector {0} of {1} ({2:F3} MiB/sec.)", currentSector, totalSize,
                                 currentSpeed);

                sense = dev.Read12(out readBuffer, out senseBuf, 0, false, false, false, false, (uint)l1, blockSize, 0,
                                   blocksToRead, false, dev.Timeout, out cmdDuration);
                totalDuration += cmdDuration;

                if(!sense && !dev.Error)
                {
                    mhddLog.Write(currentSector, cmdDuration);
                    ibgLog.Write(currentSector, currentSpeed * 1024);
                    dumpFile.Write(readBuffer);
                    extents.Add(currentSector, blocksToRead, true);
                }
                else
                {
                    // TODO: Reset device after X errors
                    if(stopOnError) return; // TODO: Return more cleanly

                    // Write empty data
                    dumpFile.Write(new byte[blockSize * blocksToRead]);

                    // TODO: Handle errors in video partition
                    //errored += blocksToRead;
                    //resume.BadBlocks.Add(l1);
                    DicConsole.DebugWriteLine("Dump-Media", "READ error:\n{0}",
                                              Decoders.SCSI.Sense.PrettifySense(senseBuf));
                    if(cmdDuration < 500) mhddLog.Write(l1, 65535);
                    else mhddLog.Write(l1, cmdDuration);

                    ibgLog.Write(l1, 0);
                    dumpLog.WriteLine("Error reading {0} blocks from block {1}.", blocksToRead, l1);
                    string[] senseLines = Decoders.SCSI.Sense.PrettifySense(senseBuf).Split(new[] {Environment.NewLine},
                                                                                            StringSplitOptions
                                                                                                .RemoveEmptyEntries);
                    foreach(string senseLine in senseLines) dumpLog.WriteLine(senseLine);
                }

#pragma warning disable IDE0004 // Remove Unnecessary Cast
                currentSpeed = (double)blockSize * blocksToRead / (double)1048576 / (cmdDuration / (double)1000);
#pragma warning restore IDE0004 // Remove Unnecessary Cast
                currentSector += blocksToRead;
                resume.NextBlock = currentSector;
            }

            dumpLog.WriteLine("Unlocking drive (Wxripper).");
            sense = dev.KreonUnlockWxripper(out senseBuf, dev.Timeout, out duration);
            if(sense)
            {
                dumpLog.WriteLine("Cannot unlock drive, not continuing.");
                DicConsole.ErrorWriteLine("Cannot unlock drive, not continuing.");
                return;
            }

            sense = dev.ReadCapacity(out readBuffer, out senseBuf, dev.Timeout, out duration);
            if(sense)
            {
                DicConsole.ErrorWriteLine("Cannot get disc capacity.");
                return;
            }

            end = DateTime.UtcNow;
            DicConsole.WriteLine();
            mhddLog.Close();
#pragma warning disable IDE0004 // Remove Unnecessary Cast
            ibgLog.Close(dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024,
                         (double)blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000), devicePath);
#pragma warning restore IDE0004 // Remove Unnecessary Cast
            dumpLog.WriteLine("Dump finished in {0} seconds.", (end - start).TotalSeconds);
            dumpLog.WriteLine("Average dump speed {0:F3} KiB/sec.",
                              (double)blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000));

            #region Error handling
            if(resume.BadBlocks.Count > 0 && !aborted)
            {
                List<ulong> tmpList = new List<ulong>();

                foreach(ulong ur in resume.BadBlocks) for(ulong i = ur; i < ur + blocksToRead; i++) tmpList.Add(i);

                tmpList.Sort();

                int pass = 0;
                bool forward = true;
                bool runningPersistent = false;

                resume.BadBlocks = tmpList;

                repeatRetry:
                ulong[] tmpArray = resume.BadBlocks.ToArray();
                foreach(ulong badSector in tmpArray)
                {
                    if(aborted)
                    {
                        currentTry.Extents = Metadata.ExtentsConverter.ToMetadata(extents);
                        dumpLog.WriteLine("Aborted!");
                        break;
                    }

                    cmdDuration = 0;

                    DicConsole.Write("\rRetrying sector {0}, pass {1}, {3}{2}", badSector, pass + 1,
                                     forward ? "forward" : "reverse",
                                     runningPersistent ? "recovering partial data, " : "");

                    sense = dev.Read12(out readBuffer, out senseBuf, 0, false, false, false, false, (uint)badSector,
                                       blockSize, 0, 1, false, dev.Timeout, out cmdDuration);
                    totalDuration += cmdDuration;

                    if(!sense && !dev.Error)
                    {
                        resume.BadBlocks.Remove(badSector);
                        extents.Add(badSector);
                        dumpFile.WriteAt(readBuffer, badSector, blockSize);
                        dumpLog.WriteLine("Correctly retried block {0} in pass {1}.", badSector, pass);
                    }
                    else if(runningPersistent) dumpFile.WriteAt(readBuffer, badSector, blockSize);
                }

                if(pass < retryPasses && !aborted && resume.BadBlocks.Count > 0)
                {
                    pass++;
                    forward = !forward;
                    resume.BadBlocks.Sort();
                    resume.BadBlocks.Reverse();
                    goto repeatRetry;
                }

                Decoders.SCSI.Modes.DecodedMode? currentMode = null;
                Decoders.SCSI.Modes.ModePage? currentModePage = null;
                byte[] md6 = null;
                byte[] md10 = null;

                if(!runningPersistent && persistent)
                {
                    sense = dev.ModeSense6(out readBuffer, out senseBuf, false, ScsiModeSensePageControl.Current, 0x01,
                                           dev.Timeout, out duration);
                    if(sense)
                    {
                        sense = dev.ModeSense10(out readBuffer, out senseBuf, false, ScsiModeSensePageControl.Current,
                                                0x01, dev.Timeout, out duration);
                        if(!sense) currentMode = Decoders.SCSI.Modes.DecodeMode10(readBuffer, dev.ScsiType);
                    }
                    else currentMode = Decoders.SCSI.Modes.DecodeMode6(readBuffer, dev.ScsiType);

                    if(currentMode.HasValue) currentModePage = currentMode.Value.Pages[0];

                    if(dev.ScsiType == Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice)
                    {
                        Decoders.SCSI.Modes.ModePage_01_MMC pgMmc =
                            new Decoders.SCSI.Modes.ModePage_01_MMC
                            {
                                PS = false,
                                ReadRetryCount = 255,
                                Parameter = 0x20
                            };
                        Decoders.SCSI.Modes.DecodedMode md = new Decoders.SCSI.Modes.DecodedMode
                        {
                            Header = new Decoders.SCSI.Modes.ModeHeader(),
                            Pages = new Decoders.SCSI.Modes.ModePage[]
                            {
                                new Decoders.SCSI.Modes.ModePage
                                {
                                    Page = 0x01,
                                    Subpage = 0x00,
                                    PageResponse = Decoders.SCSI.Modes.EncodeModePage_01_MMC(pgMmc)
                                }
                            }
                        };
                        md6 = Decoders.SCSI.Modes.EncodeMode6(md, dev.ScsiType);
                        md10 = Decoders.SCSI.Modes.EncodeMode10(md, dev.ScsiType);
                    }
                    else
                    {
                        Decoders.SCSI.Modes.ModePage_01 pg = new Decoders.SCSI.Modes.ModePage_01
                        {
                            PS = false,
                            AWRE = false,
                            ARRE = false,
                            TB = true,
                            RC = false,
                            EER = true,
                            PER = false,
                            DTE = false,
                            DCR = false,
                            ReadRetryCount = 255
                        };
                        Decoders.SCSI.Modes.DecodedMode md = new Decoders.SCSI.Modes.DecodedMode
                        {
                            Header = new Decoders.SCSI.Modes.ModeHeader(),
                            Pages = new Decoders.SCSI.Modes.ModePage[]
                            {
                                new Decoders.SCSI.Modes.ModePage
                                {
                                    Page = 0x01,
                                    Subpage = 0x00,
                                    PageResponse = Decoders.SCSI.Modes.EncodeModePage_01(pg)
                                }
                            }
                        };
                        md6 = Decoders.SCSI.Modes.EncodeMode6(md, dev.ScsiType);
                        md10 = Decoders.SCSI.Modes.EncodeMode10(md, dev.ScsiType);
                    }

                    dumpLog.WriteLine("Sending MODE SELECT to drive.");
                    sense = dev.ModeSelect(md6, out senseBuf, true, false, dev.Timeout, out duration);
                    if(sense) sense = dev.ModeSelect10(md10, out senseBuf, true, false, dev.Timeout, out duration);

                    runningPersistent = true;
                    if(!sense && !dev.Error)
                    {
                        pass--;
                        goto repeatRetry;
                    }
                }
                else if(runningPersistent && persistent && currentModePage.HasValue)
                {
                    Decoders.SCSI.Modes.DecodedMode md = new Decoders.SCSI.Modes.DecodedMode
                    {
                        Header = new Decoders.SCSI.Modes.ModeHeader(),
                        Pages = new Decoders.SCSI.Modes.ModePage[] {currentModePage.Value}
                    };
                    md6 = Decoders.SCSI.Modes.EncodeMode6(md, dev.ScsiType);
                    md10 = Decoders.SCSI.Modes.EncodeMode10(md, dev.ScsiType);

                    dumpLog.WriteLine("Sending MODE SELECT to drive.");
                    sense = dev.ModeSelect(md6, out senseBuf, true, false, dev.Timeout, out duration);
                    if(sense) sense = dev.ModeSelect10(md10, out senseBuf, true, false, dev.Timeout, out duration);
                }

                DicConsole.WriteLine();
            }
            #endregion Error handling

            resume.BadBlocks.Sort();
            currentTry.Extents = Metadata.ExtentsConverter.ToMetadata(extents);

            dataChk = new Checksum();
            dumpFile.Seek(0, SeekOrigin.Begin);
            blocksToRead = 500;

            blocks = totalSize;

            dumpLog.WriteLine("Checksum starts.");
            for(ulong i = 0; i < blocks; i += blocksToRead)
            {
                if(aborted)
                {
                    dumpLog.WriteLine("Aborted!");
                    break;
                }

                if(blocks - i < blocksToRead) blocksToRead = (uint)(blocks - i);

                DicConsole.Write("\rChecksumming sector {0} of {1} ({2:F3} MiB/sec.)", i, blocks, currentSpeed);

                DateTime chkStart = DateTime.UtcNow;
                byte[] dataToCheck = new byte[blockSize * blocksToRead];
                dumpFile.Read(dataToCheck, 0, (int)(blockSize * blocksToRead));
                dataChk.Update(dataToCheck);
                DateTime chkEnd = DateTime.UtcNow;

                double chkDuration = (chkEnd - chkStart).TotalMilliseconds;
                totalChkDuration += chkDuration;

#pragma warning disable IDE0004 // Cast is necessary, otherwise incorrect value is created
                currentSpeed = (double)blockSize * blocksToRead / (double)1048576 / (chkDuration / (double)1000);
#pragma warning restore IDE0004 // Cast is necessary, otherwise incorrect value is created
            }

            DicConsole.WriteLine();
            dumpFile.Close();
            end = DateTime.UtcNow;
            dumpLog.WriteLine("Checksum finished in {0} seconds.", (end - start).TotalSeconds);
            dumpLog.WriteLine("Average checksum speed {0:F3} KiB/sec.",
                              (double)blockSize * (double)(blocks + 1) / 1024 / (totalChkDuration / 1000));

            PluginBase plugins = new PluginBase();
            plugins.RegisterAllPlugins(encoding);
            ImagePlugin imageFormat;
            FiltersList filtersList = new FiltersList();
            Filter inputFilter = filtersList.GetFilter(outputPrefix + ".iso");

            if(inputFilter == null)
            {
                DicConsole.ErrorWriteLine("Cannot open file just created, this should not happen.");
                return;
            }

            imageFormat = ImageFormat.Detect(inputFilter);
            PartitionType[] xmlFileSysInfo = null;

            try { if(!imageFormat.OpenImage(inputFilter)) imageFormat = null; }
            catch { imageFormat = null; }

            if(imageFormat != null)
            {
                dumpLog.WriteLine("Getting partitions.");
                List<Partition> partitions = Partitions.GetAll(imageFormat);
                Partitions.AddSchemesToStats(partitions);
                dumpLog.WriteLine("Found {0} partitions.", partitions.Count);

                if(partitions.Count > 0)
                {
                    xmlFileSysInfo = new PartitionType[partitions.Count];
                    for(int i = 0; i < partitions.Count; i++)
                    {
                        xmlFileSysInfo[i] = new PartitionType
                        {
                            Description = partitions[i].Description,
                            EndSector = (int)(partitions[i].Start + partitions[i].Length - 1),
                            Name = partitions[i].Name,
                            Sequence = (int)partitions[i].Sequence,
                            StartSector = (int)partitions[i].Start,
                            Type = partitions[i].Type
                        };
                        List<FileSystemType> lstFs = new List<FileSystemType>();
                        dumpLog.WriteLine("Getting filesystems on partition {0}, starting at {1}, ending at {2}, with type {3}, under scheme {4}.",
                                          i, partitions[i].Start, partitions[i].End, partitions[i].Type,
                                          partitions[i].Scheme);

                        foreach(Filesystem plugin in plugins.PluginsList.Values)
                            try
                            {
                                if(plugin.Identify(imageFormat, partitions[i]))
                                {
                                    plugin.GetInformation(imageFormat, partitions[i], out string foo);
                                    lstFs.Add(plugin.XmlFSType);
                                    Statistics.AddFilesystem(plugin.XmlFSType.Type);
                                    dumpLog.WriteLine("Filesystem {0} found.", plugin.XmlFSType.Type);

                                    if(plugin.XmlFSType.Type == "Opera") dskType = MediaType.ThreeDO;
                                    if(plugin.XmlFSType.Type == "PC Engine filesystem")
                                        dskType = MediaType.SuperCDROM2;
                                    if(plugin.XmlFSType.Type == "Nintendo Wii filesystem") dskType = MediaType.WOD;
                                    if(plugin.XmlFSType.Type == "Nintendo Gamecube filesystem")
                                        dskType = MediaType.GOD;
                                }
                            }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                            catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                            {
                                //DicConsole.DebugWriteLine("Dump-media command", "Plugin {0} crashed", _plugin.Name);
                            }

                        if(lstFs.Count > 0) xmlFileSysInfo[i].FileSystems = lstFs.ToArray();
                    }
                }
                else
                {
                    dumpLog.WriteLine("Getting filesystem for whole device.");
                    xmlFileSysInfo = new PartitionType[1];
                    xmlFileSysInfo[0] = new PartitionType {EndSector = (int)(blocks - 1), StartSector = 0};
                    List<FileSystemType> lstFs = new List<FileSystemType>();

                    Partition wholePart =
                        new Partition {Name = "Whole device", Length = blocks, Size = blocks * blockSize};

                    foreach(Filesystem plugin in plugins.PluginsList.Values)
                        try
                        {
                            if(plugin.Identify(imageFormat, wholePart))
                            {
                                plugin.GetInformation(imageFormat, wholePart, out string foo);
                                lstFs.Add(plugin.XmlFSType);
                                Statistics.AddFilesystem(plugin.XmlFSType.Type);
                                dumpLog.WriteLine("Filesystem {0} found.", plugin.XmlFSType.Type);

                                if(plugin.XmlFSType.Type == "Opera") dskType = MediaType.ThreeDO;
                                if(plugin.XmlFSType.Type == "PC Engine filesystem") dskType = MediaType.SuperCDROM2;
                                if(plugin.XmlFSType.Type == "Nintendo Wii filesystem") dskType = MediaType.WOD;
                                if(plugin.XmlFSType.Type == "Nintendo Gamecube filesystem") dskType = MediaType.GOD;
                            }
                        }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                        catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                        {
                            //DicConsole.DebugWriteLine("Create-sidecar command", "Plugin {0} crashed", _plugin.Name);
                        }

                    if(lstFs.Count > 0) xmlFileSysInfo[0].FileSystems = lstFs.ToArray();
                }
            }

            sidecar.OpticalDisc[0].Checksums = dataChk.End().ToArray();
            sidecar.OpticalDisc[0].DumpHardwareArray = resume.Tries.ToArray();
            sidecar.OpticalDisc[0].Image = new ImageType
            {
                format = "Raw disk image (sector by sector copy)",
                Value = outputPrefix + ".iso"
            };
            sidecar.OpticalDisc[0].Layers = new LayersType
            {
                type = LayersTypeType.OTP,
                typeSpecified = true,
                Sectors = new SectorsType[1]
            };
            sidecar.OpticalDisc[0].Layers.Sectors[0] = new SectorsType {Value = (long)layerBreak};
            sidecar.OpticalDisc[0].Sessions = 1;
            sidecar.OpticalDisc[0].Tracks = new[] {1};
            sidecar.OpticalDisc[0].Track = new Schemas.TrackType[1];
            sidecar.OpticalDisc[0].Track[0] = new Schemas.TrackType
            {
                BytesPerSector = (int)blockSize,
                Checksums = sidecar.OpticalDisc[0].Checksums,
                EndSector = (long)(blocks - 1),
                Image =
                    new ImageType()
                    {
                        format = "BINARY",
                        offset = 0,
                        offsetSpecified = true,
                        Value = sidecar.OpticalDisc[0].Image.Value
                    },
                Sequence = new TrackSequenceType() {Session = 1, TrackNumber = 1},
                Size = (long)(totalSize * blockSize),
                StartSector = 0
            };
            if(xmlFileSysInfo != null) sidecar.OpticalDisc[0].Track[0].FileSystemInformation = xmlFileSysInfo;
            sidecar.OpticalDisc[0].Track[0].TrackType1 = TrackTypeTrackType.dvd;
            sidecar.OpticalDisc[0].Dimensions = Metadata.Dimensions.DimensionsFromMediaType(dskType);
            Metadata.MediaType.MediaTypeToString(dskType, out string xmlDskTyp, out string xmlDskSubTyp);
            sidecar.OpticalDisc[0].DiscType = xmlDskTyp;
            sidecar.OpticalDisc[0].DiscSubType = xmlDskSubTyp;

            if(!aborted)
            {
                DicConsole.WriteLine("Writing metadata sidecar");

                FileStream xmlFs = new FileStream(outputPrefix + ".cicm.xml", FileMode.Create);

                System.Xml.Serialization.XmlSerializer xmlSer =
                    new System.Xml.Serialization.XmlSerializer(typeof(CICMMetadataType));
                xmlSer.Serialize(xmlFs, sidecar);
                xmlFs.Close();
            }

            Statistics.AddMedia(dskType, true);
        }
    }
}