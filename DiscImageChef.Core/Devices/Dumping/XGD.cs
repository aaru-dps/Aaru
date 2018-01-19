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
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using DiscImageChef.Console;
using DiscImageChef.Core.Logging;
using DiscImageChef.Decoders.DVD;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Decoders.Xbox;
using DiscImageChef.Devices;
using DiscImageChef.DiscImages;
using DiscImageChef.Filters;
using DiscImageChef.Metadata;
using Extents;
using Schemas;
using MediaType = DiscImageChef.CommonTypes.MediaType;

namespace DiscImageChef.Core.Devices.Dumping
{
    /// <summary>
    ///     Implements dumping an Xbox Game Disc using a Kreon drive
    /// </summary>
    static class Xgd
    {
        /// <summary>
        ///     Dumps an Xbox Game Disc using a Kreon drive
        /// </summary>
        /// <param name="dev">Device</param>
        /// <param name="devicePath">Path to the device</param>
        /// <param name="outputPrefix">Prefix for output data files</param>
        /// <param name="outputPlugin">Plugin for output file</param>
        /// <param name="retryPasses">How many times to retry</param>
        /// <param name="force">Force to continue dump whenever possible</param>
        /// <param name="dumpRaw">Dump raw/long sectors</param>
        /// <param name="persistent">Store whatever data the drive returned on error</param>
        /// <param name="stopOnError">Stop dump on first error</param>
        /// <param name="resume">Information for dump resuming</param>
        /// <param name="dumpLog">Dump logger</param>
        /// <param name="encoding">Encoding to use when analyzing dump</param>
        /// <param name="mediaTags">Media tags as retrieved in MMC layer</param>
        /// <param name="dskType">Disc type as detected in MMC layer</param>
        /// <param name="outputPath">Path to output file</param>
        /// <param name="formatOptions">Formats to pass to output file plugin</param>
        /// <exception cref="InvalidOperationException">
        ///     If the provided resume does not correspond with the current in progress
        ///     dump
        /// </exception>
        internal static void Dump(Device                           dev, string              devicePath,
                                  IWritableImage                   outputPlugin, ushort     retryPasses,
                                  bool                             force, bool              dumpRaw,
                                  bool                             persistent, bool         stopOnError,
                                  Dictionary<MediaTagType, byte[]> mediaTags, ref MediaType dskType,
                                  ref                                             Resume    resume,
                                  ref                                             DumpLog   dumpLog,
                                  Encoding                                                  encoding, string outputPrefix, string outputPath,
                                  Dictionary<string, string>                                formatOptions)
        {
            bool       sense;
            ulong      blocks;
            const uint BLOCK_SIZE   = 2048;
            uint       blocksToRead = 64;
            DateTime   start;
            DateTime   end;
            double     totalDuration      = 0;
            double     totalChkDuration   = 0;
            double     currentSpeed       = 0;
            double     maxSpeed           = double.MinValue;
            double     minSpeed           = double.MaxValue;
            bool       aborted            = false;
            System.Console.CancelKeyPress += (sender, e) => e.Cancel = aborted = true;

            if(mediaTags.ContainsKey(MediaTagType.DVD_PFI)) mediaTags.Remove(MediaTagType.DVD_PFI);
            if(mediaTags.ContainsKey(MediaTagType.DVD_DMI)) mediaTags.Remove(MediaTagType.DVD_DMI);

            dumpLog.WriteLine("Reading Xbox Security Sector.");
            sense = dev.KreonExtractSs(out byte[] ssBuf, out byte[] senseBuf, dev.Timeout, out _);
            if(sense)
            {
                dumpLog.WriteLine("Cannot get Xbox Security Sector, not continuing.");
                DicConsole.ErrorWriteLine("Cannot get Xbox Security Sector, not continuing.");
                return;
            }

            dumpLog.WriteLine("Decoding Xbox Security Sector.");
            SS.SecuritySector? xboxSs = SS.Decode(ssBuf);
            if(!xboxSs.HasValue)
            {
                dumpLog.WriteLine("Cannot decode Xbox Security Sector, not continuing.");
                DicConsole.ErrorWriteLine("Cannot decode Xbox Security Sector, not continuing.");
                return;
            }

            byte[] tmpBuf = new byte[ssBuf.Length        - 4];
            Array.Copy(ssBuf, 4, tmpBuf, 0, ssBuf.Length - 4);
            mediaTags.Add(MediaTagType.Xbox_SecuritySector, tmpBuf);

            ulong l0Video, l1Video, middleZone, gameSize, totalSize, layerBreak;

            // Get video partition size
            DicConsole.DebugWriteLine("Dump-media command", "Getting video partition size");
            dumpLog.WriteLine("Locking drive.");
            sense = dev.KreonLock(out senseBuf, dev.Timeout, out _);
            if(sense)
            {
                dumpLog.WriteLine("Cannot lock drive, not continuing.");
                DicConsole.ErrorWriteLine("Cannot lock drive, not continuing.");
                return;
            }

            dumpLog.WriteLine("Getting video partition size.");
            sense = dev.ReadCapacity(out byte[] readBuffer, out senseBuf, dev.Timeout, out _);
            if(sense)
            {
                dumpLog.WriteLine("Cannot get disc capacity.");
                DicConsole.ErrorWriteLine("Cannot get disc capacity.");
                return;
            }

            totalSize = (ulong)((readBuffer[0] << 24) + (readBuffer[1] << 16) + (readBuffer[2] << 8) + readBuffer[3]);
            dumpLog.WriteLine("Reading Physical Format Information.");
            sense = dev.ReadDiscStructure(out readBuffer, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                          MmcDiscStructureFormat.PhysicalInformation, 0, 0, out _);
            if(sense)
            {
                dumpLog.WriteLine("Cannot get PFI.");
                DicConsole.ErrorWriteLine("Cannot get PFI.");
                return;
            }

            tmpBuf = new byte[readBuffer.Length                    - 4];
            Array.Copy(readBuffer, 4, tmpBuf, 0, readBuffer.Length - 4);
            mediaTags.Add(MediaTagType.DVD_PFI, tmpBuf);
            DicConsole.DebugWriteLine("Dump-media command", "Video partition total size: {0} sectors", totalSize);
            l0Video = PFI.Decode(readBuffer).Value.Layer0EndPSN - PFI.Decode(readBuffer).Value.DataAreaStartPSN + 1;
            l1Video = totalSize                                 - l0Video                                       + 1;
            dumpLog.WriteLine("Reading Disc Manufacturing Information.");
            sense = dev.ReadDiscStructure(out readBuffer, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                          MmcDiscStructureFormat.DiscManufacturingInformation, 0, 0, out _);
            if(sense)
            {
                dumpLog.WriteLine("Cannot get DMI.");
                DicConsole.ErrorWriteLine("Cannot get DMI.");
                return;
            }

            tmpBuf = new byte[readBuffer.Length                    - 4];
            Array.Copy(readBuffer, 4, tmpBuf, 0, readBuffer.Length - 4);
            mediaTags.Add(MediaTagType.DVD_DMI, tmpBuf);

            // Get game partition size
            DicConsole.DebugWriteLine("Dump-media command", "Getting game partition size");
            dumpLog.WriteLine("Unlocking drive (Xtreme).");
            sense = dev.KreonUnlockXtreme(out senseBuf, dev.Timeout, out _);
            if(sense)
            {
                dumpLog.WriteLine("Cannot unlock drive, not continuing.");
                DicConsole.ErrorWriteLine("Cannot unlock drive, not continuing.");
                return;
            }

            dumpLog.WriteLine("Getting game partition size.");
            sense = dev.ReadCapacity(out readBuffer, out senseBuf, dev.Timeout, out _);
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
            sense = dev.KreonUnlockWxripper(out senseBuf, dev.Timeout, out _);
            if(sense)
            {
                dumpLog.WriteLine("Cannot unlock drive, not continuing.");
                DicConsole.ErrorWriteLine("Cannot unlock drive, not continuing.");
                return;
            }

            dumpLog.WriteLine("Getting disc size.");
            sense = dev.ReadCapacity(out readBuffer, out senseBuf, dev.Timeout, out _);
            if(sense)
            {
                dumpLog.WriteLine("Cannot get disc capacity.");
                DicConsole.ErrorWriteLine("Cannot get disc capacity.");
                return;
            }

            totalSize = (ulong)((readBuffer[0] << 24) + (readBuffer[1] << 16) + (readBuffer[2] << 8) + readBuffer[3]);
            dumpLog.WriteLine("Reading Physical Format Information.");
            sense = dev.ReadDiscStructure(out readBuffer, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                          MmcDiscStructureFormat.PhysicalInformation, 0, 0, out _);
            if(sense)
            {
                dumpLog.WriteLine("Cannot get PFI.");
                DicConsole.ErrorWriteLine("Cannot get PFI.");
                return;
            }

            DicConsole.DebugWriteLine("Dump-media command", "Unlocked total size: {0} sectors", totalSize);
            blocks     = totalSize + 1;
            middleZone =
                totalSize                                                        - (PFI.Decode(readBuffer).Value.Layer0EndPSN -
                                   PFI.Decode(readBuffer).Value.DataAreaStartPSN +
                                   1)                                            - gameSize + 1;

            tmpBuf = new byte[readBuffer.Length                    - 4];
            Array.Copy(readBuffer, 4, tmpBuf, 0, readBuffer.Length - 4);
            mediaTags.Add(MediaTagType.Xbox_PFI, tmpBuf);

            dumpLog.WriteLine("Reading Disc Manufacturing Information.");
            sense = dev.ReadDiscStructure(out readBuffer, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                          MmcDiscStructureFormat.DiscManufacturingInformation, 0, 0, out _);
            if(sense)
            {
                dumpLog.WriteLine("Cannot get DMI.");
                DicConsole.ErrorWriteLine("Cannot get DMI.");
                return;
            }

            tmpBuf = new byte[readBuffer.Length                    - 4];
            Array.Copy(readBuffer, 4, tmpBuf, 0, readBuffer.Length - 4);
            mediaTags.Add(MediaTagType.Xbox_DMI, tmpBuf);

            totalSize  = l0Video + l1Video    + middleZone * 2 + gameSize;
            layerBreak = l0Video + middleZone + gameSize   / 2;

            DicConsole.WriteLine("Video layer 0 size: {0} sectors", l0Video);
            DicConsole.WriteLine("Video layer 1 size: {0} sectors", l1Video);
            DicConsole.WriteLine("Middle zone size: {0} sectors",   middleZone);
            DicConsole.WriteLine("Game data size: {0} sectors",     gameSize);
            DicConsole.WriteLine("Total size: {0} sectors",         totalSize);
            DicConsole.WriteLine("Real layer break: {0}",           layerBreak);
            DicConsole.WriteLine();

            dumpLog.WriteLine("Video layer 0 size: {0} sectors", l0Video);
            dumpLog.WriteLine("Video layer 1 size: {0} sectors", l1Video);
            dumpLog.WriteLine("Middle zone 0 size: {0} sectors", middleZone);
            dumpLog.WriteLine("Game data 0 size: {0} sectors",   gameSize);
            dumpLog.WriteLine("Total 0 size: {0} sectors",       totalSize);
            dumpLog.WriteLine("Real layer break: {0}",           layerBreak);

            bool read12 = !dev.Read12(out readBuffer, out senseBuf, 0, false, true, false, false, 0, BLOCK_SIZE, 0, 1,
                                      false, dev.Timeout, out _);
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
                    sense = dev.Read12(out readBuffer, out senseBuf, 0, false, false, false, false, 0, BLOCK_SIZE, 0,
                                       blocksToRead, false, dev.Timeout, out _);
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

            bool ret = true;

            foreach(MediaTagType tag in mediaTags.Keys)
            {
                if(outputPlugin.SupportedMediaTags.Contains(tag)) continue;

                ret = false;
                dumpLog.WriteLine($"Output format does not support {tag}.");
                DicConsole.ErrorWriteLine($"Output format does not support {tag}.");
            }

            if(!ret)
            {
                dumpLog.WriteLine("Several media tags not supported, {0}continuing...", force ? "" : "not ");
                DicConsole.ErrorWriteLine("Several media tags not supported, {0}continuing...", force ? "" : "not ");
                if(!force) return;
            }

            dumpLog.WriteLine("Reading {0} sectors at a time.", blocksToRead);
            DicConsole.WriteLine("Reading {0} sectors at a time.", blocksToRead);

            MhddLog mhddLog = new MhddLog(outputPrefix + ".mhddlog.bin", dev, blocks, BLOCK_SIZE, blocksToRead);
            IbgLog  ibgLog  = new IbgLog(outputPrefix  + ".ibg", 0x0010);
            ret             = outputPlugin.Create(outputPath, dskType, formatOptions, blocks, BLOCK_SIZE);

            // Cannot create image
            if(!ret)
            {
                dumpLog.WriteLine("Error creating output image, not continuing.");
                dumpLog.WriteLine(outputPlugin.ErrorMessage);
                DicConsole.ErrorWriteLine("Error creating output image, not continuing.");
                DicConsole.ErrorWriteLine(outputPlugin.ErrorMessage);
                return;
            }

            start = DateTime.UtcNow;

            double           cmdDuration      = 0;
            uint             saveBlocksToRead = blocksToRead;
            DumpHardwareType currentTry       = null;
            ExtentsULong     extents          = null;
            ResumeSupport.Process(true, true, totalSize, dev.Manufacturer, dev.Model, dev.Serial, dev.PlatformId,
                                  ref resume, ref currentTry, ref extents);
            if(currentTry == null || extents == null)
                throw new NotImplementedException("Could not process resume file, not continuing...");

            ulong currentSector = resume.NextBlock;
            if(resume.NextBlock > 0) dumpLog.WriteLine("Resuming from block {0}.", resume.NextBlock);

            dumpLog.WriteLine("Reading game partition.");
            for(int e = 0; e <= 16; e++)
            {
                if(aborted)
                {
                    resume.NextBlock   = currentSector;
                    currentTry.Extents = ExtentsConverter.ToMetadata(extents);
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
                        extentStart = (xboxSs.Value.Layer0EndPSN                     + 1) * 2 -
                                      ((xboxSs.Value.Extents[e].StartPSN ^ 0xFFFFFF) + 1)     - 0x30000;
                    if(xboxSs.Value.Extents[e].EndPSN <= xboxSs.Value.Layer0EndPSN)
                        extentEnd = xboxSs.Value.Extents[e].EndPSN - 0x30000;
                    else
                        extentEnd = (xboxSs.Value.Layer0EndPSN                   + 1) * 2 -
                                    ((xboxSs.Value.Extents[e].EndPSN ^ 0xFFFFFF) + 1)     - 0x30000;
                }
                // After last extent
                else
                {
                    extentStart = blocks;
                    extentEnd   = blocks;
                }

                if(currentSector > extentEnd) continue;

                for(ulong i = currentSector; i < extentStart; i += blocksToRead)
                {
                    saveBlocksToRead = blocksToRead;

                    if(aborted)
                    {
                        currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                        dumpLog.WriteLine("Aborted!");
                        break;
                    }

                    if(extentStart - i < blocksToRead) blocksToRead = (uint)(extentStart - i);

                    #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                    if(currentSpeed > maxSpeed && currentSpeed != 0) maxSpeed = currentSpeed;
                    if(currentSpeed < minSpeed && currentSpeed != 0) minSpeed = currentSpeed;
                    #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                    DicConsole.Write("\rReading sector {0} of {1} ({2:F3} MiB/sec.)", i, totalSize, currentSpeed);

                    sense = dev.Read12(out readBuffer, out senseBuf, 0, false, false, false, false, (uint)i, BLOCK_SIZE,
                                       0, blocksToRead, false, dev.Timeout, out cmdDuration);
                    totalDuration += cmdDuration;

                    if(!sense && !dev.Error)
                    {
                        mhddLog.Write(i, cmdDuration);
                        ibgLog.Write(i, currentSpeed * 1024);
                        outputPlugin.WriteSectors(readBuffer, i, blocksToRead);
                        extents.Add(i, blocksToRead, true);
                    }
                    else
                    {
                        // TODO: Reset device after X errors
                        if(stopOnError) return; // TODO: Return more cleanly

                        // Write empty data
                        outputPlugin.WriteSectors(new byte[BLOCK_SIZE * blocksToRead], i, blocksToRead);

                        for(ulong b = i; b < i + blocksToRead; b++) resume.BadBlocks.Add(b);

                        DicConsole.DebugWriteLine("Dump-Media", "READ error:\n{0}", Sense.PrettifySense(senseBuf));
                        mhddLog.Write(i, cmdDuration < 500 ? 65535 : cmdDuration);

                        ibgLog.Write(i, 0);

                        dumpLog.WriteLine("Error reading {0} blocks from block {1}.", blocksToRead, i);
                        string[] senseLines = Sense.PrettifySense(senseBuf).Split(new[] {Environment.NewLine},
                                                                                  StringSplitOptions
                                                                                     .RemoveEmptyEntries);
                        foreach(string senseLine in senseLines) dumpLog.WriteLine(senseLine);
                    }

                    double newSpeed =
                        (double)BLOCK_SIZE * blocksToRead / 1048576 / (cmdDuration / 1000);
                    if(!double.IsInfinity(newSpeed)) currentSpeed = newSpeed;
                    blocksToRead                                  = saveBlocksToRead;
                    currentSector                                 = i + 1;
                    resume.NextBlock                              = currentSector;
                }

                for(ulong i = extentStart; i <= extentEnd; i += blocksToRead)
                {
                    saveBlocksToRead = blocksToRead;
                    if(aborted)
                    {
                        currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                        dumpLog.WriteLine("Aborted!");
                        break;
                    }

                    if(extentEnd - i < blocksToRead) blocksToRead = (uint)(extentEnd - i) + 1;

                    mhddLog.Write(i, cmdDuration);
                    ibgLog.Write(i, currentSpeed * 1024);
                    // Write empty data
                    outputPlugin.WriteSectors(new byte[BLOCK_SIZE * blocksToRead], i, blocksToRead);
                    blocksToRead = saveBlocksToRead;
                    extents.Add(i, blocksToRead, true);
                    currentSector    = i + 1;
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
                    currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                    dumpLog.WriteLine("Aborted!");
                    break;
                }

                if(middleZone - 1 - middle < blocksToRead) blocksToRead = (uint)(middleZone - 1 - middle);

                DicConsole.Write("\rReading sector {0} of {1} ({2:F3} MiB/sec.)", middle + currentSector, totalSize,
                                 currentSpeed);

                mhddLog.Write(middle + currentSector, cmdDuration);
                ibgLog.Write(middle  + currentSector, currentSpeed * 1024);
                // Write empty data
                outputPlugin.WriteSectors(new byte[BLOCK_SIZE * blocksToRead], middle + currentSector, blocksToRead);
                extents.Add(currentSector, blocksToRead, true);

                currentSector    += blocksToRead;
                resume.NextBlock =  currentSector;
            }

            blocksToRead = saveBlocksToRead;

            dumpLog.WriteLine("Locking drive.");
            sense = dev.KreonLock(out senseBuf, dev.Timeout, out _);
            if(sense)
            {
                dumpLog.WriteLine("Cannot lock drive, not continuing.");
                DicConsole.ErrorWriteLine("Cannot lock drive, not continuing.");
                return;
            }

            sense = dev.ReadCapacity(out readBuffer, out senseBuf, dev.Timeout, out _);
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
                    currentTry.Extents = ExtentsConverter.ToMetadata(extents);
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

                sense = dev.Read12(out readBuffer, out senseBuf, 0, false, false, false, false, (uint)l1, BLOCK_SIZE, 0,
                                   blocksToRead, false, dev.Timeout, out cmdDuration);
                totalDuration += cmdDuration;

                if(!sense && !dev.Error)
                {
                    mhddLog.Write(currentSector, cmdDuration);
                    ibgLog.Write(currentSector, currentSpeed * 1024);
                    outputPlugin.WriteSectors(readBuffer, currentSector, blocksToRead);
                    extents.Add(currentSector, blocksToRead, true);
                }
                else
                {
                    // TODO: Reset device after X errors
                    if(stopOnError) return; // TODO: Return more cleanly

                    // Write empty data
                    outputPlugin.WriteSectors(new byte[BLOCK_SIZE * blocksToRead], currentSector, blocksToRead);

                    // TODO: Handle errors in video partition
                    //errored += blocksToRead;
                    //resume.BadBlocks.Add(l1);
                    DicConsole.DebugWriteLine("Dump-Media", "READ error:\n{0}", Sense.PrettifySense(senseBuf));
                    mhddLog.Write(l1, cmdDuration < 500 ? 65535 : cmdDuration);

                    ibgLog.Write(l1, 0);
                    dumpLog.WriteLine("Error reading {0} blocks from block {1}.", blocksToRead, l1);
                    string[] senseLines = Sense.PrettifySense(senseBuf).Split(new[] {Environment.NewLine},
                                                                              StringSplitOptions.RemoveEmptyEntries);
                    foreach(string senseLine in senseLines) dumpLog.WriteLine(senseLine);
                }

                double newSpeed =
                    (double)BLOCK_SIZE * blocksToRead / 1048576 / (cmdDuration / 1000);
                if(!double.IsInfinity(newSpeed)) currentSpeed =  newSpeed;
                currentSector                                 += blocksToRead;
                resume.NextBlock                              =  currentSector;
            }

            dumpLog.WriteLine("Unlocking drive (Wxripper).");
            sense = dev.KreonUnlockWxripper(out senseBuf, dev.Timeout, out _);
            if(sense)
            {
                dumpLog.WriteLine("Cannot unlock drive, not continuing.");
                DicConsole.ErrorWriteLine("Cannot unlock drive, not continuing.");
                return;
            }

            sense = dev.ReadCapacity(out readBuffer, out senseBuf, dev.Timeout, out _);
            if(sense)
            {
                DicConsole.ErrorWriteLine("Cannot get disc capacity.");
                return;
            }

            end = DateTime.UtcNow;
            DicConsole.WriteLine();
            mhddLog.Close();
            ibgLog.Close(dev, blocks, BLOCK_SIZE, (end - start).TotalSeconds, currentSpeed * 1024,
                         BLOCK_SIZE                                                        * (double)(blocks + 1) /
                         1024                                                              / (totalDuration       / 1000), devicePath);
            dumpLog.WriteLine("Dump finished in {0} seconds.",
                              (end - start).TotalSeconds);
            dumpLog.WriteLine("Average dump speed {0:F3} KiB/sec.",
                              (double)BLOCK_SIZE * (double)(blocks + 1) / 1024 / (totalDuration / 1000));

            #region Error handling
            if(resume.BadBlocks.Count > 0 && !aborted)
            {
                List<ulong> tmpList = new List<ulong>();

                foreach(ulong ur in resume.BadBlocks)
                    for(ulong i = ur; i < ur + blocksToRead; i++)
                        tmpList.Add(i);

                tmpList.Sort();

                int  pass              = 0;
                bool forward           = true;
                bool runningPersistent = false;

                resume.BadBlocks = tmpList;

                repeatRetry:
                ulong[] tmpArray = resume.BadBlocks.ToArray();
                foreach(ulong badSector in tmpArray)
                {
                    if(aborted)
                    {
                        currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                        dumpLog.WriteLine("Aborted!");
                        break;
                    }

                    DicConsole.Write("\rRetrying sector {0}, pass {1}, {3}{2}", badSector, pass + 1,
                                     forward ? "forward" : "reverse",
                                     runningPersistent ? "recovering partial data, " : "");

                    sense = dev.Read12(out readBuffer, out senseBuf, 0, false, false, false, false, (uint)badSector,
                                       BLOCK_SIZE, 0, 1, false, dev.Timeout, out cmdDuration);
                    totalDuration += cmdDuration;

                    if(!sense && !dev.Error)
                    {
                        resume.BadBlocks.Remove(badSector);
                        extents.Add(badSector);
                        outputPlugin.WriteSector(readBuffer, badSector);
                        dumpLog.WriteLine("Correctly retried block {0} in pass {1}.", badSector, pass);
                    }
                    else if(runningPersistent)
                        outputPlugin.WriteSector(readBuffer, badSector);
                }

                if(pass < retryPasses && !aborted && resume.BadBlocks.Count > 0)
                {
                    pass++;
                    forward = !forward;
                    resume.BadBlocks.Sort();
                    resume.BadBlocks.Reverse();
                    goto repeatRetry;
                }

                Modes.ModePage? currentModePage = null;
                byte[]          md6;
                byte[]          md10;

                if(!runningPersistent && persistent)
                {
                    if(dev.ScsiType == PeripheralDeviceTypes.MultiMediaDevice)
                    {
                        Modes.ModePage_01_MMC pgMmc       =
                            new Modes.ModePage_01_MMC {PS = false, ReadRetryCount = 255, Parameter = 0x20};
                        Modes.DecodedMode md              = new Modes.DecodedMode
                        {
                            Header = new Modes.ModeHeader(),
                            Pages  = new[]
                            {
                                new Modes.ModePage
                                {
                                    Page         = 0x01,
                                    Subpage      = 0x00,
                                    PageResponse = Modes.EncodeModePage_01_MMC(pgMmc)
                                }
                            }
                        };
                        md6  = Modes.EncodeMode6(md, dev.ScsiType);
                        md10 = Modes.EncodeMode10(md, dev.ScsiType);
                    }
                    else
                    {
                        Modes.ModePage_01 pg = new Modes.ModePage_01
                        {
                            PS             = false,
                            AWRE           = false,
                            ARRE           = false,
                            TB             = true,
                            RC             = false,
                            EER            = true,
                            PER            = false,
                            DTE            = false,
                            DCR            = false,
                            ReadRetryCount = 255
                        };
                        Modes.DecodedMode md = new Modes.DecodedMode
                        {
                            Header = new Modes.ModeHeader(),
                            Pages  = new[]
                            {
                                new Modes.ModePage
                                {
                                    Page         = 0x01,
                                    Subpage      = 0x00,
                                    PageResponse = Modes.EncodeModePage_01(pg)
                                }
                            }
                        };
                        md6  = Modes.EncodeMode6(md, dev.ScsiType);
                        md10 = Modes.EncodeMode10(md, dev.ScsiType);
                    }

                    dumpLog.WriteLine("Sending MODE SELECT to drive.");
                    sense           = dev.ModeSelect(md6, out senseBuf, true, false, dev.Timeout, out _);
                    if(sense) sense = dev.ModeSelect10(md10, out senseBuf, true, false, dev.Timeout, out _);

                    runningPersistent = true;
                    if(!sense && !dev.Error)
                    {
                        pass--;
                        goto repeatRetry;
                    }
                }
                else if(runningPersistent && persistent && currentModePage.HasValue)
                {
                    Modes.DecodedMode md = new Modes.DecodedMode
                    {
                        Header = new Modes.ModeHeader(),
                        Pages  = new[] {currentModePage.Value}
                    };
                    md6  = Modes.EncodeMode6(md, dev.ScsiType);
                    md10 = Modes.EncodeMode10(md, dev.ScsiType);

                    dumpLog.WriteLine("Sending MODE SELECT to drive.");
                    sense = dev.ModeSelect(md6, out senseBuf, true, false, dev.Timeout, out _);
                    if(sense) dev.ModeSelect10(md10, out senseBuf, true, false, dev.Timeout, out _);
                }

                DicConsole.WriteLine();
            }
            #endregion Error handling

            resume.BadBlocks.Sort();
            currentTry.Extents = ExtentsConverter.ToMetadata(extents);

            foreach(KeyValuePair<MediaTagType, byte[]> tag in mediaTags)
            {
                ret = outputPlugin.WriteMediaTag(tag.Value, tag.Key);
                if(ret || force) continue;

                // Cannot write tag to image
                dumpLog.WriteLine($"Cannot write tag {tag.Key}.");
                throw new ArgumentException(outputPlugin.ErrorMessage);
            }

            dumpLog.WriteLine("Closing output file.");
            DicConsole.WriteLine("Closing output file.");
            outputPlugin.Close();

            resume.BadBlocks.Sort();
            currentTry.Extents = ExtentsConverter.ToMetadata(extents);

            if(aborted)
            {
                dumpLog.WriteLine("Aborted!");
                return;
            }

            dumpLog.WriteLine("Creating sidecar.");
            FiltersList filters     = new FiltersList();
            IFilter     filter      = filters.GetFilter(outputPath);
            IMediaImage inputPlugin = ImageFormat.Detect(filter);
            if(!inputPlugin.Open(filter)) throw new ArgumentException("Could not open created image.");

            DateTime         chkStart = DateTime.UtcNow;
            CICMMetadataType sidecar  = Sidecar.Create(inputPlugin, outputPath, filter.Id, encoding);
            end                       = DateTime.UtcNow;

            totalChkDuration = (end                                   - chkStart).TotalMilliseconds;
            dumpLog.WriteLine("Sidecar created in {0} seconds.", (end - chkStart).TotalSeconds);
            dumpLog.WriteLine("Average checksum speed {0:F3} KiB/sec.",
                              (double)BLOCK_SIZE * (double)(blocks + 1) / 1024 / (totalChkDuration / 1000));

            foreach(KeyValuePair<MediaTagType, byte[]> tag in mediaTags)
                Mmc.AddMediaTagToSidecar(outputPath, tag, ref sidecar);

            sidecar.OpticalDisc[0].Layers = new LayersType
            {
                type          = LayersTypeType.OTP,
                typeSpecified = true,
                Sectors       = new SectorsType[1]
            };
            sidecar.OpticalDisc[0].Layers.Sectors[0] = new SectorsType {Value = (long)layerBreak};
            sidecar.OpticalDisc[0].Sessions          = 1;
            sidecar.OpticalDisc[0].Dimensions        = Dimensions.DimensionsFromMediaType(dskType);
            Metadata.MediaType.MediaTypeToString(dskType, out string xmlDskTyp, out string xmlDskSubTyp);
            sidecar.OpticalDisc[0].DiscType    = xmlDskTyp;
            sidecar.OpticalDisc[0].DiscSubType = xmlDskSubTyp;

            foreach(KeyValuePair<MediaTagType, byte[]> tag in mediaTags)
                if(outputPlugin.SupportedMediaTags.Contains(tag.Key))
                    Mmc.AddMediaTagToSidecar(outputPath, tag, ref sidecar);

            if(!aborted)
            {
                DicConsole.WriteLine("Writing metadata sidecar");

                FileStream xmlFs = new FileStream(outputPrefix + ".cicm.xml", FileMode.Create);

                XmlSerializer xmlSer = new XmlSerializer(typeof(CICMMetadataType));
                xmlSer.Serialize(xmlFs, sidecar);
                xmlFs.Close();
            }

            Statistics.AddMedia(dskType, true);
        }
    }
}