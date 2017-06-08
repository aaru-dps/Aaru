// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : XGD.cs
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
using System.IO;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Core.Logging;
using DiscImageChef.Devices;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using DiscImageChef.ImagePlugins;
using DiscImageChef.PartPlugins;
using Schemas;

namespace DiscImageChef.Core.Devices.Dumping
{
    internal static class XGD
    {
        internal static void Dump(Device dev, string devicePath, string outputPrefix, ushort retryPasses, bool force, bool dumpRaw, bool persistent, bool stopOnError, ref CICMMetadataType sidecar, ref MediaType dskType, ref Metadata.Resume resume)
        {
            MHDDLog mhddLog;
            IBGLog ibgLog;
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
            List<ulong> unreadableSectors = new List<ulong>();
            Checksum dataChk;
            DataFile dumpFile = null;
            bool aborted = false;
            System.Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = aborted = true;
            };

            sense = dev.KreonExtractSS(out byte[] ssBuf, out byte[] senseBuf, dev.Timeout, out double duration);
            if(sense)
            {
                DicConsole.ErrorWriteLine("Cannot get Xbox Security Sector, not continuing.");
                return;
            }

            Decoders.Xbox.SS.SecuritySector? xboxSS = Decoders.Xbox.SS.Decode(ssBuf);
            if(!xboxSS.HasValue)
            {
                DicConsole.ErrorWriteLine("Cannot decode Xbox Security Sector, not continuing.");
                return;
            }

            // TODO: Correct metadata
            /*sidecar.OpticalDisc[0].XboxSecuritySectors = new DumpType();
            sidecar.OpticalDisc[0].XboxSecuritySectors.Image = outputPrefix + ".bca.bin";
            sidecar.OpticalDisc[0].XboxSecuritySectors.Size = cmdBuf.Length;
            sidecar.OpticalDisc[0].XboxSecuritySectors.Checksums = Checksum.GetChecksums(cmbBuf).ToArray();
            DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].XboxSecuritySectors.Image, tmpBuf);*/
            DataFile.WriteTo("SCSI Dump", outputPrefix + ".ss.bin", ssBuf);

            ulong l0Video, l1Video, middleZone, gameSize, totalSize, layerBreak;

            // Get video partition size
            DicConsole.DebugWriteLine("Dump-media command", "Getting video partition size");
            sense = dev.KreonLock(out senseBuf, dev.Timeout, out duration);
            if(sense)
            {
                DicConsole.ErrorWriteLine("Cannot lock drive, not continuing.");
                return;
            }
            sense = dev.ReadCapacity(out byte[] readBuffer, out senseBuf, dev.Timeout, out duration);
            if(sense)
            {
                DicConsole.ErrorWriteLine("Cannot get disc capacity.");
                return;
            }
            totalSize = (ulong)((readBuffer[0] << 24) + (readBuffer[1] << 16) + (readBuffer[2] << 8) + (readBuffer[3]));
            sense = dev.ReadDiscStructure(out readBuffer, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.PhysicalInformation, 0, 0, out duration);
            if(sense)
            {
                DicConsole.ErrorWriteLine("Cannot get PFI.");
                return;
            }
            DicConsole.DebugWriteLine("Dump-media command", "Video partition total size: {0} sectors", totalSize);
            l0Video = Decoders.DVD.PFI.Decode(readBuffer).Value.Layer0EndPSN - Decoders.DVD.PFI.Decode(readBuffer).Value.DataAreaStartPSN + 1;
            l1Video = totalSize - l0Video + 1;

            // Get game partition size
            DicConsole.DebugWriteLine("Dump-media command", "Getting game partition size");
            sense = dev.KreonUnlockXtreme(out senseBuf, dev.Timeout, out duration);
            if(sense)
            {
                DicConsole.ErrorWriteLine("Cannot unlock drive, not continuing.");
                return;
            }
            sense = dev.ReadCapacity(out readBuffer, out senseBuf, dev.Timeout, out duration);
            if(sense)
            {
                DicConsole.ErrorWriteLine("Cannot get disc capacity.");
                return;
            }
            gameSize = (ulong)((readBuffer[0] << 24) + (readBuffer[1] << 16) + (readBuffer[2] << 8) + (readBuffer[3])) + 1;
            DicConsole.DebugWriteLine("Dump-media command", "Game partition total size: {0} sectors", gameSize);

            // Get middle zone size
            DicConsole.DebugWriteLine("Dump-media command", "Getting middle zone size");
            sense = dev.KreonUnlockWxripper(out senseBuf, dev.Timeout, out duration);
            if(sense)
            {
                DicConsole.ErrorWriteLine("Cannot unlock drive, not continuing.");
                return;
            }
            sense = dev.ReadCapacity(out readBuffer, out senseBuf, dev.Timeout, out duration);
            if(sense)
            {
                DicConsole.ErrorWriteLine("Cannot get disc capacity.");
                return;
            }
            totalSize = (ulong)((readBuffer[0] << 24) + (readBuffer[1] << 16) + (readBuffer[2] << 8) + (readBuffer[3]));
            sense = dev.ReadDiscStructure(out readBuffer, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.PhysicalInformation, 0, 0, out duration);
            if(sense)
            {
                DicConsole.ErrorWriteLine("Cannot get PFI.");
                return;
            }
            DicConsole.DebugWriteLine("Dump-media command", "Unlocked total size: {0} sectors", totalSize);
            blocks = totalSize + 1;
            middleZone = totalSize - (Decoders.DVD.PFI.Decode(readBuffer).Value.Layer0EndPSN - Decoders.DVD.PFI.Decode(readBuffer).Value.DataAreaStartPSN + 1) - gameSize + 1;

            totalSize = l0Video + l1Video + middleZone * 2 + gameSize;
            layerBreak = l0Video + middleZone + gameSize / 2;

            DicConsole.WriteLine("Video layer 0 size: {0} sectors", l0Video);
            DicConsole.WriteLine("Video layer 1 size: {0} sectors", l1Video);
            DicConsole.WriteLine("Middle zone size: {0} sectors", middleZone);
            DicConsole.WriteLine("Game data size: {0} sectors", gameSize);
            DicConsole.WriteLine("Total size: {0} sectors", totalSize);
            DicConsole.WriteLine("Real layer break: {0}", layerBreak);
            DicConsole.WriteLine();

            bool read12 = !dev.Read12(out readBuffer, out senseBuf, 0, false, true, false, false, 0, blockSize, 0, 1, false, dev.Timeout, out duration);
            if(!read12)
            {
                DicConsole.ErrorWriteLine("Cannot read medium, aborting scan...");
                return;
            }

            DicConsole.WriteLine("Using SCSI READ (12) command.");

            while(true)
            {
                if(read12)
                {
                    sense = dev.Read12(out readBuffer, out senseBuf, 0, false, false, false, false, 0, blockSize, 0, blocksToRead, false, dev.Timeout, out duration);
                    if(sense || dev.Error)
                        blocksToRead /= 2;
                }

                if(!dev.Error || blocksToRead == 1)
                    break;
            }

            if(dev.Error)
            {
                DicConsole.ErrorWriteLine("Device error {0} trying to guess ideal transfer length.", dev.LastError);
                return;
            }


            DicConsole.WriteLine("Reading {0} sectors at a time.", blocksToRead);

            mhddLog = new MHDDLog(outputPrefix + ".mhddlog.bin", dev, blocks, blockSize, blocksToRead);
            ibgLog = new IBGLog(outputPrefix + ".ibg", 0x0010);
            dumpFile = new DataFile(outputPrefix + ".iso");

            start = DateTime.UtcNow;

            readBuffer = null;

            ulong currentSector = 0;
            double cmdDuration = 0;
            uint saveBlocksToRead = blocksToRead;

            for(int e = 0; e <= 16; e++)
            {
                ulong extentStart, extentEnd;
                // Extents
                if(e < 16)
                {
                    if(xboxSS.Value.Extents[e].StartPSN <= xboxSS.Value.Layer0EndPSN)
                        extentStart = xboxSS.Value.Extents[e].StartPSN - 0x30000;
                    else
                        extentStart = (xboxSS.Value.Layer0EndPSN + 1) * 2 - ((xboxSS.Value.Extents[e].StartPSN ^ 0xFFFFFF) + 1) - 0x30000;
                    if(xboxSS.Value.Extents[e].EndPSN <= xboxSS.Value.Layer0EndPSN)
                        extentEnd = xboxSS.Value.Extents[e].EndPSN - 0x30000;
                    else
                        extentEnd = (xboxSS.Value.Layer0EndPSN + 1) * 2 - ((xboxSS.Value.Extents[e].EndPSN ^ 0xFFFFFF) + 1) - 0x30000;
                }
                // After last extent
                else
                {
                    extentStart = blocks;
                    extentEnd = blocks;
                }

                for(ulong i = currentSector; i < extentStart; i += blocksToRead)
                {
                    saveBlocksToRead = blocksToRead;
                    if(aborted)
                        break;

                    if((extentStart - i) < blocksToRead)
                        blocksToRead = (uint)(extentStart - i);

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                    if(currentSpeed > maxSpeed && currentSpeed != 0)
                        maxSpeed = currentSpeed;
                    if(currentSpeed < minSpeed && currentSpeed != 0)
                        minSpeed = currentSpeed;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                    DicConsole.Write("\rReading sector {0} of {1} ({2:F3} MiB/sec.)", i, totalSize, currentSpeed);

                    sense = dev.Read12(out readBuffer, out senseBuf, 0, false, false, false, false, (uint)i, blockSize, 0, blocksToRead, false, dev.Timeout, out cmdDuration);
                    totalDuration += cmdDuration;

                    if(!sense && !dev.Error)
                    {
                        mhddLog.Write(i, cmdDuration);
                        ibgLog.Write(i, currentSpeed * 1024);
                        dumpFile.Write(readBuffer);
                    }
                    else
                    {
                        // TODO: Reset device after X errors
                        if(stopOnError)
                            return; // TODO: Return more cleanly

                        // Write empty data
                        dumpFile.Write(new byte[blockSize * blocksToRead]);

                        // TODO: Record error on mapfile

                        errored += blocksToRead;
                        for(ulong b = i; b < i + blocksToRead; b++)
                            unreadableSectors.Add(b);
                        DicConsole.DebugWriteLine("Dump-Media", "READ error:\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                        if(cmdDuration < 500)
                            mhddLog.Write(i, 65535);
                        else
                            mhddLog.Write(i, cmdDuration);

                        ibgLog.Write(i, 0);
                    }

#pragma warning disable IDE0004 // Remove Unnecessary Cast
                    currentSpeed = ((double)blockSize * blocksToRead / (double)1048576) / (cmdDuration / (double)1000);
#pragma warning restore IDE0004 // Remove Unnecessary Cast
                    blocksToRead = saveBlocksToRead;
                }

                for(ulong i = extentStart; i <= extentEnd; i += blocksToRead)
                {
                    saveBlocksToRead = blocksToRead;
                    if(aborted)
                        break;

                    if((extentEnd - i) < blocksToRead)
                        blocksToRead = (uint)(extentEnd - i) + 1;

                    mhddLog.Write(i, cmdDuration);
                    ibgLog.Write(i, currentSpeed * 1024);
                    dumpFile.Write(new byte[blocksToRead * 2048]);
                    blocksToRead = saveBlocksToRead;
                }

                currentSector = extentEnd + 1;
                if(currentSector >= blocks)
                    break;
            }

            // Middle Zone D
            for(ulong middle = 0; middle < (middleZone - 1); middle += blocksToRead)
            {
                if(aborted)
                    break;

                if(((middleZone - 1) - middle) < blocksToRead)
                    blocksToRead = (uint)((middleZone - 1) - middle);

                DicConsole.Write("\rReading sector {0} of {1} ({2:F3} MiB/sec.)", middle + currentSector, totalSize, currentSpeed);

                mhddLog.Write(middle + currentSector, cmdDuration);
                ibgLog.Write(middle + currentSector, currentSpeed * 1024);
                dumpFile.Write(new byte[blockSize * blocksToRead]);

                currentSector += blocksToRead;
            }

            blocksToRead = saveBlocksToRead;

            sense = dev.KreonLock(out senseBuf, dev.Timeout, out duration);
            if(sense)
            {
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
            for(ulong l1 = l0Video; l1 < (l0Video + l1Video); l1 += blocksToRead)
            {
                if(aborted)
                    break;

                if(((l0Video + l1Video) - l1) < blocksToRead)
                    blocksToRead = (uint)((l0Video + l1Video) - l1);

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                if(currentSpeed > maxSpeed && currentSpeed != 0)
                    maxSpeed = currentSpeed;
                if(currentSpeed < minSpeed && currentSpeed != 0)
                    minSpeed = currentSpeed;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                DicConsole.Write("\rReading sector {0} of {1} ({2:F3} MiB/sec.)", currentSector, totalSize, currentSpeed);

                sense = dev.Read12(out readBuffer, out senseBuf, 0, false, false, false, false, (uint)l1, blockSize, 0, blocksToRead, false, dev.Timeout, out cmdDuration);
                totalDuration += cmdDuration;

                if(!sense && !dev.Error)
                {
                    mhddLog.Write(currentSector, cmdDuration);
                    ibgLog.Write(currentSector, currentSpeed * 1024);
                    dumpFile.Write(readBuffer);
                }
                else
                {
                    // TODO: Reset device after X errors
                    if(stopOnError)
                        return; // TODO: Return more cleanly

                    // Write empty data
                    dumpFile.Write(new byte[blockSize * blocksToRead]);

                    // TODO: Record error on mapfile

                    // TODO: Handle errors in video partition
                    //errored += blocksToRead;
                    //unreadableSectors.Add(l1);
                    DicConsole.DebugWriteLine("Dump-Media", "READ error:\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                    if(cmdDuration < 500)
                        mhddLog.Write(l1, 65535);
                    else
                        mhddLog.Write(l1, cmdDuration);

                    ibgLog.Write(l1, 0);
                }

#pragma warning disable IDE0004 // Remove Unnecessary Cast
                currentSpeed = ((double)blockSize * blocksToRead / (double)1048576) / (cmdDuration / (double)1000);
#pragma warning restore IDE0004 // Remove Unnecessary Cast
                currentSector += blocksToRead;
            }

            sense = dev.KreonUnlockWxripper(out senseBuf, dev.Timeout, out duration);
            if(sense)
            {
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
            ibgLog.Close(dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024, (((double)blockSize * (double)(blocks + 1)) / 1024) / (totalDuration / 1000), devicePath);
#pragma warning restore IDE0004 // Remove Unnecessary Cast

            #region Error handling
            if(unreadableSectors.Count > 0 && !aborted)
            {
                List<ulong> tmpList = new List<ulong>();

                foreach(ulong ur in unreadableSectors)
                {
                    for(ulong i = ur; i < ur + blocksToRead; i++)
                        tmpList.Add(i);
                }

                tmpList.Sort();

                int pass = 0;
                bool forward = true;
                bool runningPersistent = false;

                unreadableSectors = tmpList;

            repeatRetry:
                ulong[] tmpArray = unreadableSectors.ToArray();
                foreach(ulong badSector in tmpArray)
                {
                    if(aborted)
                        break;

                    cmdDuration = 0;

                    DicConsole.Write("\rRetrying sector {0}, pass {1}, {3}{2}", badSector, pass + 1, forward ? "forward" : "reverse", runningPersistent ? "recovering partial data, " : "");

                    sense = dev.Read12(out readBuffer, out senseBuf, 0, false, false, false, false, (uint)badSector, blockSize, 0, 1, false, dev.Timeout, out cmdDuration);
                    totalDuration += cmdDuration;

                    if(!sense && !dev.Error)
                    {
                        unreadableSectors.Remove(badSector);
                        dumpFile.WriteAt(readBuffer, badSector, blockSize);
                    }
                    else if(runningPersistent)
                        dumpFile.WriteAt(readBuffer, badSector, blockSize);
                }

                if(pass < retryPasses && !aborted && unreadableSectors.Count > 0)
                {
                    pass++;
                    forward = !forward;
                    unreadableSectors.Sort();
                    unreadableSectors.Reverse();
                    goto repeatRetry;
                }

                Decoders.SCSI.Modes.DecodedMode? currentMode = null;
                Decoders.SCSI.Modes.ModePage? currentModePage = null;
                byte[] md6 = null;
                byte[] md10 = null;

                if(!runningPersistent && persistent)
                {
                    sense = dev.ModeSense6(out readBuffer, out senseBuf, false, ScsiModeSensePageControl.Current, 0x01, dev.Timeout, out duration);
                    if(sense)
                    {
                        sense = dev.ModeSense10(out readBuffer, out senseBuf, false, ScsiModeSensePageControl.Current, 0x01, dev.Timeout, out duration);
                        if(!sense)
                            currentMode = Decoders.SCSI.Modes.DecodeMode10(readBuffer, dev.SCSIType);
                    }
                    else
                        currentMode = Decoders.SCSI.Modes.DecodeMode6(readBuffer, dev.SCSIType);

                    if(currentMode.HasValue)
                        currentModePage = currentMode.Value.Pages[0];

                    if(dev.SCSIType == Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice)
                    {
                        Decoders.SCSI.Modes.ModePage_01_MMC pgMMC = new Decoders.SCSI.Modes.ModePage_01_MMC
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
		                            PageResponse = Decoders.SCSI.Modes.EncodeModePage_01_MMC(pgMMC)
		                        }
                            }
                        };
                        md6 = Decoders.SCSI.Modes.EncodeMode6(md, dev.SCSIType);
                        md10 = Decoders.SCSI.Modes.EncodeMode10(md, dev.SCSIType);
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
                        md6 = Decoders.SCSI.Modes.EncodeMode6(md, dev.SCSIType);
                        md10 = Decoders.SCSI.Modes.EncodeMode10(md, dev.SCSIType);
                    }

                    sense = dev.ModeSelect(md6, out senseBuf, true, false, dev.Timeout, out duration);
                    if(sense)
                    {
                        sense = dev.ModeSelect10(md10, out senseBuf, true, false, dev.Timeout, out duration);
                    }

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
                        Pages = new Decoders.SCSI.Modes.ModePage[]
                        {
                            currentModePage.Value
                        }
                    };
                    md6 = Decoders.SCSI.Modes.EncodeMode6(md, dev.SCSIType);
                    md10 = Decoders.SCSI.Modes.EncodeMode10(md, dev.SCSIType);

                    sense = dev.ModeSelect(md6, out senseBuf, true, false, dev.Timeout, out duration);
                    if(sense)
                    {
                        sense = dev.ModeSelect10(md10, out senseBuf, true, false, dev.Timeout, out duration);
                    }
                }

                DicConsole.WriteLine();
            }
            #endregion Error handling

            dataChk = new Checksum();
            dumpFile.Seek(0, SeekOrigin.Begin);
            blocksToRead = 500;

            blocks = totalSize;

            for(ulong i = 0; i < blocks; i += blocksToRead)
            {
                if(aborted)
                    break;

                if((blocks - i) < blocksToRead)
                    blocksToRead = (uint)(blocks - i);

                DicConsole.Write("\rChecksumming sector {0} of {1} ({2:F3} MiB/sec.)", i, blocks, currentSpeed);

                DateTime chkStart = DateTime.UtcNow;
                byte[] dataToCheck = new byte[blockSize * blocksToRead];
                dumpFile.Read(dataToCheck, 0, (int)(blockSize * blocksToRead));
                dataChk.Update(dataToCheck);
                DateTime chkEnd = DateTime.UtcNow;

                double chkDuration = (chkEnd - chkStart).TotalMilliseconds;
                totalChkDuration += chkDuration;

#pragma warning disable IDE0004 // Cast is necessary, otherwise incorrect value is created
                currentSpeed = ((double)blockSize * blocksToRead / (double)1048576) / (chkDuration / (double)1000);
#pragma warning restore IDE0004 // Cast is necessary, otherwise incorrect value is created
            }
            DicConsole.WriteLine();
            dumpFile.Close();
            end = DateTime.UtcNow;

            PluginBase plugins = new PluginBase();
            plugins.RegisterAllPlugins();
            ImagePlugin _imageFormat;
            FiltersList filtersList = new FiltersList();
            Filter inputFilter = filtersList.GetFilter(outputPrefix + ".iso");

            if(inputFilter == null)
            {
                DicConsole.ErrorWriteLine("Cannot open file just created, this should not happen.");
                return;
            }

            _imageFormat = ImageFormat.Detect(inputFilter);
            PartitionType[] xmlFileSysInfo = null;

            try
            {
                if(!_imageFormat.OpenImage(inputFilter))
                    _imageFormat = null;
            }
            catch
            {
                _imageFormat = null;
            }

            if(_imageFormat != null)
            {
                List<Partition> partitions = new List<Partition>();

                foreach(PartPlugin _partplugin in plugins.PartPluginsList.Values)
                {

                    if(_partplugin.GetInformation(_imageFormat, out List<Partition> _partitions))
                    {
                        partitions.AddRange(_partitions);
                        Statistics.AddPartition(_partplugin.Name);
                    }
                }

                if(partitions.Count > 0)
                {
                    xmlFileSysInfo = new PartitionType[partitions.Count];
                    for(int i = 0; i < partitions.Count; i++)
                    {
                        xmlFileSysInfo[i] = new PartitionType
                        {
                            Description = partitions[i].PartitionDescription,
                            EndSector = (int)(partitions[i].PartitionStartSector + partitions[i].PartitionSectors - 1),
                            Name = partitions[i].PartitionName,
                            Sequence = (int)partitions[i].PartitionSequence,
                            StartSector = (int)partitions[i].PartitionStartSector,
                            Type = partitions[i].PartitionType
                        };
                        List<FileSystemType> lstFs = new List<FileSystemType>();

                        foreach(Filesystem _plugin in plugins.PluginsList.Values)
                        {
                            try
                            {
                                if(_plugin.Identify(_imageFormat, partitions[i].PartitionStartSector, partitions[i].PartitionStartSector + partitions[i].PartitionSectors - 1))
                                {
                                    _plugin.GetInformation(_imageFormat, partitions[i].PartitionStartSector, partitions[i].PartitionStartSector + partitions[i].PartitionSectors - 1, out string foo);
                                    lstFs.Add(_plugin.XmlFSType);
                                    Statistics.AddFilesystem(_plugin.XmlFSType.Type);

                                    if(_plugin.XmlFSType.Type == "Opera")
                                        dskType = MediaType.ThreeDO;
                                    if(_plugin.XmlFSType.Type == "PC Engine filesystem")
                                        dskType = MediaType.SuperCDROM2;
                                    if(_plugin.XmlFSType.Type == "Nintendo Wii filesystem")
                                        dskType = MediaType.WOD;
                                    if(_plugin.XmlFSType.Type == "Nintendo Gamecube filesystem")
                                        dskType = MediaType.GOD;
                                }
                            }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                            catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                            {
                                //DicConsole.DebugWriteLine("Dump-media command", "Plugin {0} crashed", _plugin.Name);
                            }
                        }

                        if(lstFs.Count > 0)
                            xmlFileSysInfo[i].FileSystems = lstFs.ToArray();
                    }
                }
                else
                {
                    xmlFileSysInfo = new PartitionType[1];
                    xmlFileSysInfo[0] = new PartitionType
                    {
                        EndSector = (int)(blocks - 1),
                        StartSector = 0
                    };
                    List<FileSystemType> lstFs = new List<FileSystemType>();

                    foreach(Filesystem _plugin in plugins.PluginsList.Values)
                    {
                        try
                        {
                            if(_plugin.Identify(_imageFormat, (blocks - 1), 0))
                            {
                                _plugin.GetInformation(_imageFormat, (blocks - 1), 0, out string foo);
                                lstFs.Add(_plugin.XmlFSType);
                                Statistics.AddFilesystem(_plugin.XmlFSType.Type);

                                if(_plugin.XmlFSType.Type == "Opera")
                                    dskType = MediaType.ThreeDO;
                                if(_plugin.XmlFSType.Type == "PC Engine filesystem")
                                    dskType = MediaType.SuperCDROM2;
                                if(_plugin.XmlFSType.Type == "Nintendo Wii filesystem")
                                    dskType = MediaType.WOD;
                                if(_plugin.XmlFSType.Type == "Nintendo Gamecube filesystem")
                                    dskType = MediaType.GOD;
                            }
                        }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                        catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                        {
                            //DicConsole.DebugWriteLine("Create-sidecar command", "Plugin {0} crashed", _plugin.Name);
                        }
                    }

                    if(lstFs.Count > 0)
                        xmlFileSysInfo[0].FileSystems = lstFs.ToArray();
                }
            }

            sidecar.OpticalDisc[0].Checksums = dataChk.End().ToArray();
            sidecar.OpticalDisc[0].DumpHardwareArray = new DumpHardwareType[1];
            sidecar.OpticalDisc[0].DumpHardwareArray[0] = new DumpHardwareType
            {
                Extents = new ExtentType[1]
            };
            sidecar.OpticalDisc[0].DumpHardwareArray[0].Extents[0] = new ExtentType
            {
                Start = 0,
                End = blocks - 1
            };
            sidecar.OpticalDisc[0].DumpHardwareArray[0].Manufacturer = dev.Manufacturer;
            sidecar.OpticalDisc[0].DumpHardwareArray[0].Model = dev.Model;
            sidecar.OpticalDisc[0].DumpHardwareArray[0].Revision = dev.Revision;
            sidecar.OpticalDisc[0].DumpHardwareArray[0].Software = Version.GetSoftwareType(dev.PlatformID);
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
            sidecar.OpticalDisc[0].Layers.Sectors[0] = new SectorsType
            {
                Value = (long)layerBreak
            };
            sidecar.OpticalDisc[0].Sessions = 1;
            sidecar.OpticalDisc[0].Tracks = new[] { 1 };
            sidecar.OpticalDisc[0].Track = new Schemas.TrackType[1];
            sidecar.OpticalDisc[0].Track[0] = new Schemas.TrackType
            {
                BytesPerSector = (int)blockSize,
                Checksums = sidecar.OpticalDisc[0].Checksums,
                EndSector = (long)(blocks - 1),
                Image = new ImageType()
                {
                    format = "BINARY",
                    offset = 0,
                    offsetSpecified = true,
                    Value = sidecar.OpticalDisc[0].Image.Value
                },
                Sequence = new TrackSequenceType()
                {
                    Session = 1,
                    TrackNumber = 1
                },
                Size = (long)(totalSize * blockSize),
                StartSector = 0
            };
            if(xmlFileSysInfo != null)
                sidecar.OpticalDisc[0].Track[0].FileSystemInformation = xmlFileSysInfo;
            sidecar.OpticalDisc[0].Track[0].TrackType1 = TrackTypeTrackType.dvd;
            sidecar.OpticalDisc[0].Dimensions = Metadata.Dimensions.DimensionsFromMediaType(dskType);
            Metadata.MediaType.MediaTypeToString(dskType, out string xmlDskTyp, out string xmlDskSubTyp);
            sidecar.OpticalDisc[0].DiscType = xmlDskTyp;
            sidecar.OpticalDisc[0].DiscSubType = xmlDskSubTyp;
        }
    }
}
